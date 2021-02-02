using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using AccountTypeList = PX.Objects.GL.AccountType;

namespace PX.Objects.GL
{
    public class JournalEntryImport : PXGraph<JournalEntryImport, GLTrialBalanceImportMap>, PXImportAttribute.IPXPrepareItems
    {
        #region OperationParam

        [Serializable]
        public partial class OperationParam : IBqlTable
        {
            public abstract class action : PX.Data.BQL.BqlString.Field<action> { }

            protected String _Action;
            [PXDefault(_VALIDATE_ACTION)]
            public String Action
            {
                get { return _Action; }
                set { _Action = value; }
            }
        }

        #endregion

        #region TrialBalanceTemplate
        //Alias
        [Serializable]
        public partial class TrialBalanceTemplate : GLTrialBalanceImportDetails
        {
            public new abstract class mapNumber : PX.Data.BQL.BqlString.Field<mapNumber> { }
            public new abstract class line : PX.Data.BQL.BqlInt.Field<line> { }
            public new abstract class importAccountCD : PX.Data.BQL.BqlString.Field<importAccountCD> { }
            public new abstract class mapAccountID : PX.Data.BQL.BqlInt.Field<mapAccountID> { }
            public new abstract class importSubAccountCD : PX.Data.BQL.BqlString.Field<importSubAccountCD> { }
            public new abstract class mapSubAccountID : PX.Data.BQL.BqlInt.Field<mapSubAccountID> { }
            public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
            public new abstract class status : PX.Data.BQL.BqlInt.Field<status> { }
            public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        }

        #endregion

        #region GLHistoryEnquiryWithSubResult

        [Serializable]
        public partial class GLHistoryEnquiryWithSubResult : GLHistoryEnquiryResult
        {
            public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

            #region SubID
            public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

            protected Int32? _SubID;
            [SubAccount(typeof(accountID))]
            public virtual Int32? SubID
            {
                get { return _SubID; }
                set { _SubID = value; }
            }
            #endregion

            #region AccountType
            public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }

            protected String _AccountType;
            [PXDBString(1)]
            [PXDefault(AccountTypeList.Asset)]
            [AccountTypeList.List()]
            [PXUIField(DisplayName = "Account Type")]
            public virtual String AccountType
            {
                get { return _AccountType; }
                set { _AccountType = value; }
            }

            #endregion
        }

        #endregion

        #region JournalEntryImportProcessing

        public class JournalEntryImportProcessing : PXGraph<JournalEntryImportProcessing>
        {
            #region Fields

            public PXSetup<Company> CompanySetup;

            public PXSetup<GLSetup> GLSetup;

			public PXSelect<CurrencyInfo> CurrencyInfoView;
            
			public PXSelect<Batch> Batch;

            public PXSelect<GLTran, Where<GLTran.batchNbr, Equal<Current<Batch.batchNbr>>>> GLTrans;

            public PXSelect<GLTrialBalanceImportMap> Map;

            public PXSelect<GLTrialBalanceImportDetails,
                Where<GLTrialBalanceImportDetails.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> MapDetails;

	        public PXSetup<Ledger, Where<Ledger.ledgerID, Equal<Optional<Batch.ledgerID>>>> LedgerView;

	        private Dictionary<string, CurrencyInfo> _curyInfosByCuryID;

            #endregion

	        public JournalEntryImportProcessing()
	        {
		        _curyInfosByCuryID = new Dictionary<string, CurrencyInfo>();
	        }

	        #region ReleaseImport

            public static void ReleaseImport(object mapNumber, bool isReversedSign)
            {
                var graph = (JournalEntryImportProcessing)PXGraph.CreateInstance(typeof(JournalEntryImportProcessing));

                GLTrialBalanceImportMap map = graph.Map.Search<GLTrialBalanceImportMap.number>(mapNumber);
                if (map == null) return;

                Batch newBatch = new Batch();
                graph.Map.Current = map;

                using (new PXConnectionScope())
                {
                    using (var ts = new PXTransactionScope())
                    {
                        List<GLTrialBalanceImportDetails> details = new List<GLTrialBalanceImportDetails>();
                        foreach (GLTrialBalanceImportDetails item in graph.MapDetails.Select())
                            details.Add(item);
                        var refNumber = _TRAN_REFNUMBER_PREFIX + mapNumber;
						var ledger = graph.LedgerView.Current;


                        newBatch.BranchID = map.BranchID;
                        newBatch = (Batch)graph.Batch.Cache.Insert(newBatch);
                        newBatch.Description = map.Description;
                        newBatch.DebitTotal = 0m;
                        newBatch.CreditTotal = 0m;
                        newBatch.BatchType = BatchTypeCode.TrialBalance;

	                    var curyInfo = graph.GetOrCreateCuryInfo(ledger.BaseCuryID, ledger.BaseCuryID);

	                    newBatch.CuryInfoID = curyInfo.CuryInfoID;
	                    newBatch.CuryID = curyInfo.CuryID;

                        foreach (var item in
                            GetBalances(graph, isReversedSign, map.BranchID, map.LedgerID,
                                        map.FinPeriodID, map.BegFinPeriod))
                        {
                            GLTrialBalanceImportDetails importItem = null;
                            for (int index = 0; index < details.Count; index++)
                            {
                                GLTrialBalanceImportDetails detail = details[index];
                                if (detail.MapAccountID == item.AccountID && detail.MapSubAccountID == item.SubID)
                                {
                                    importItem = detail;
                                    details.RemoveAt(index);
                                    break;
                                }
                            }

                            decimal diff = (importItem == null ? 0m : (decimal)importItem.YtdBalance) - (decimal)item.EndBalance;
                            decimal curyDiff = (importItem == null ? 0m : (decimal)importItem.CuryYtdBalance) - (decimal)item.CuryEndBalance;
                            if (diff == 0m) continue;

							var account = GetAccount(graph, item.AccountID);

                            graph.GLTrans.Cache.Insert();
                            GLTran tran = graph.GLTrans.Current;
							tran.AccountID = account.AccountID;
                            tran.SubID = item.SubID;
                            FillDebitAndCreditAmt(tran, diff, curyDiff, isReversedSign, item.AccountType);
                            tran.RefNbr = refNumber;
							tran.CuryInfoID = graph.GetOrCreateCuryInfoForTran(account, ledger);
                            graph.GLTrans.Update(tran);
                        }
                        foreach (var item in details)
                        {
                            decimal diff = (decimal)item.YtdBalance;
                            decimal curyDiff = (decimal)item.CuryYtdBalance;
                            if (diff == 0m) continue;

							var account = GetAccount(graph, item.MapAccountID);

							graph.GLTrans.Cache.Insert();
                            GLTran tran = graph.GLTrans.Current;
							tran.AccountID = account.AccountID;
                            tran.SubID = item.MapSubAccountID;
							FillDebitAndCreditAmt(tran, diff, curyDiff, isReversedSign, account.Type);
                            tran.RefNbr = refNumber;
							tran.CuryInfoID = graph.GetOrCreateCuryInfoForTran(account, ledger);
                            graph.GLTrans.Update(tran);
                        }
                        newBatch.ControlTotal = (newBatch.DebitTotal == newBatch.CreditTotal) ? newBatch.DebitTotal : 0m;
                        graph.Batch.Update(newBatch);

                        map.Status = TrialBalanceImportMapStatusAttribute.RELEASED;
                        graph.Map.Update(map);
						graph.Actions.PressSave();

                        ts.Complete();
                    }
                }

                using (new PXTimeStampScope(null))
                {
                    graph.Clear();
                    newBatch = graph.Batch.Search<Batch.batchNbr, Batch.module>(newBatch.BatchNbr, newBatch.Module);
                    PXRedirectHelper.TryRedirect(graph.Batch.Cache, newBatch, Messages.ViewBatch);
                }
            }

			private long? GetOrCreateCuryInfoForTran(Account account, Ledger ledger)
			{
				if (account == null)
					return null;

				var curyID = account.CuryID != null && ledger.BalanceType != LedgerBalanceType.Report
								? account.CuryID
								: ledger.BaseCuryID;

				var curyInfo = GetOrCreateCuryInfo(curyID, ledger.BaseCuryID);

				return curyInfo.CuryInfoID;
			}

			private CurrencyInfo GetOrCreateCuryInfo(string curyID, string baseCuryID)
	        {
		        if (_curyInfosByCuryID.ContainsKey(curyID))
			        return _curyInfosByCuryID[curyID];

				var curyInfo = new CurrencyInfo
				{
					BaseCuryID = baseCuryID,
					CuryID = curyID,
					BaseCalc = false,
					CuryRate = 1,
					RecipRate = 1
				};

		        curyInfo = CurrencyInfoView.Insert(curyInfo);
		        CurrencyInfoView.Cache.PersistInserted(curyInfo);
				CurrencyInfoView.Cache.Persisted(false);

				_curyInfosByCuryID.Add(curyInfo.CuryID, curyInfo);

		        return curyInfo;
	        }

            #endregion


            #region Batch

            protected virtual void Batch_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
            {
                if (Map.Current != null) Map.Current.BatchNbr = ((Batch)e.Row).BatchNbr;
            }

            protected virtual void Batch_Module_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
            {
                e.NewValue = BatchModule.GL;
            }

            protected virtual void Batch_Status_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
            {
                e.NewValue = BatchStatus.Balanced;
            }

            protected virtual void Batch_DateEntered_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
            {
                if (Map.Current != null) e.NewValue = Map.Current.ImportDate;
            }

            protected virtual void Batch_FinPeriodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
            {
                if (Map.Current != null) e.NewValue = Map.Current.FinPeriodID;
                e.Cancel = true;
            }

            protected virtual void Batch_LedgerID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
            {
                if (Map.Current != null) e.NewValue = GetLedger(cache.Graph, Map.Current.LedgerID).LedgerCD;
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Constants

        private const string _VALIDATE_ACTION = "Validate";
        private const string _MERGE_DUPLICATES_ACTION = "Merge Duplicates";
        private const string _IMPORTTEMPLATE_VIEWNAME = "ImportTemplate";
        private const string _TRAN_REFNUMBER_PREFIX = "";

        #endregion

        private readonly string _mapNumberFieldName;

        [PXHidden]
        public PXSetup<GLSetup> GLSetup;

        [PXHidden]
        public PXFilter<OperationParam> Operations;

        public PXSelect<GLTrialBalanceImportMap> Map;

        [PXFilterable]
        public PXSelect<GLTrialBalanceImportDetails,
            Where<GLTrialBalanceImportDetails.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> MapDetails;

        [PXImport(typeof(GLTrialBalanceImportMap))]
		public PXSelect<TrialBalanceTemplate,
			Where<TrialBalanceTemplate.mapNumber, Equal<Current<GLTrialBalanceImportMap.number>>>> ImportTemplate;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelectOrderBy<GLHistoryEnquiryWithSubResult,
            OrderBy<Asc<GLHistoryEnquiryWithSubResult.accountID, Asc<GLHistoryEnquiryWithSubResult.subID>>>> Exceptions;

		public PXSetup<Ledger,
							Where<Ledger.ledgerID, Equal<Optional<GLTrialBalanceImportMap.ledgerID>>>> Ledger;

        #endregion

        #region Ctors

        public JournalEntryImport()
        {
			var importAttribute = ImportTemplate.GetAttribute<PXImportAttribute>();
			importAttribute.MappingPropertiesInit += MappingPropertiesInit;


            _mapNumberFieldName = ImportTemplate.Cache.GetField(typeof(TrialBalanceTemplate.mapNumber));

            MapDetails.Cache.AllowInsert = true;
            MapDetails.Cache.AllowUpdate = true;
            MapDetails.Cache.AllowDelete = true;
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.selected>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importAccountCD>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.importSubAccountCD>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.ytdBalance>(MapDetails.Cache, null, true);
            PXUIFieldAttribute.SetReadOnly<GLTrialBalanceImportDetails.status>(MapDetails.Cache, null);
            
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapNumber>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapAccountID>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.mapSubAccountID>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.selected>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.status>(ImportTemplate.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<TrialBalanceTemplate.description>(ImportTemplate.Cache, null, false);
			PXUIFieldAttribute.SetDisplayName<TrialBalanceTemplate.importAccountCD>(ImportTemplate.Cache, Messages.FieldNameAccount);
            PXUIFieldAttribute.SetDisplayName<TrialBalanceTemplate.importSubAccountCD>(ImportTemplate.Cache, Messages.Sub);

	        PXUIFieldAttribute.SetReadOnly(Exceptions.Cache, null);
        }

        #endregion

        #region Actions

        public PXAction<GLTrialBalanceImportMap> process;
        [PXUIField(DisplayName = Messages.Process)]
        [PXButton]
        protected virtual IEnumerable Process(PXAdapter adapter)
        {
            if (CanEdit)
            {
                Dictionary<int, GLTrialBalanceImportDetails> dict = new Dictionary<int, GLTrialBalanceImportDetails>();
                foreach (GLTrialBalanceImportDetails item in MapDetails.Select(Operations.Current.Action))
                {   
                    if (item.Selected == true && item.Line != null)
                    {
                        int line = (int)item.Line;
                        if (!dict.ContainsKey(line))
                        {   dict.Add(line, (GLTrialBalanceImportDetails)MapDetails.Cache.CreateCopy(item));
                        }
                    }
                }
                if (Operations.Current != null)
                {   ProcessHandler(dict, Operations.Current.Action, true);
                }
            }
            return adapter.Get();
        }

        public PXAction<GLTrialBalanceImportMap> processAll;
        [PXUIField(DisplayName = Messages.ProcessAll)]
        [PXButton]
        protected virtual IEnumerable ProcessAll(PXAdapter adapter)
        {
            if (CanEdit)
            {
                Dictionary<int, GLTrialBalanceImportDetails> dict = new Dictionary<int, GLTrialBalanceImportDetails>();
                foreach (GLTrialBalanceImportDetails item in MapDetails.Select(Operations.Current.Action))
                {
                    if (item.Line != null)
                    {
                        int line = (int)item.Line;
                        if (!dict.ContainsKey(line))
                        {
                            item.Selected = true;
                            dict.Add(line, (GLTrialBalanceImportDetails)MapDetails.Cache.CreateCopy(item));
                        }
                    }
                }
                if (Operations.Current != null)
                {   ProcessHandler(dict, Operations.Current.Action, true);
                }
            }
            return adapter.Get();
        }

        public PXAction<GLTrialBalanceImportMap> release;
        [PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            GLTrialBalanceImportMap map = Map.Current;
            if (map != null)
            {
                if (map.Status != TrialBalanceImportMapStatusAttribute.BALANCED)
                    throw new PXException(Messages.ImportStatusInvalid);

                if (map.CreditTotalBalance != map.TotalBalance)
					throw new Exception(Messages.DocumentIsOutOfBalancePleaseReview);
                if (map.DebitTotalBalance != map.TotalBalance)
					throw new Exception(Messages.DocumentIsOutOfBalancePleaseReview);

                PXResultset<GLTrialBalanceImportMap> res = PXSelectJoin<GLTrialBalanceImportMap,
                    InnerJoin<Batch,
                        On<Batch.batchNbr, Equal<GLTrialBalanceImportMap.batchNbr>,
                        And<Batch.module, Equal<BatchModule.moduleGL>,
                        And<Batch.posted, Equal<False>>>>>,
                    Where<GLTrialBalanceImportMap.finPeriodID, LessEqual<Current<GLTrialBalanceImportMap.finPeriodID>>>>.SelectSingleBound(this, null, null);
                if (res.Count > 0)
                {
                    throw new Exception(Messages.PreviousBatchesNotPosted);
                }

                Save.Press();
                bool isUnsignOperations = IsUnsignOperations(this);
                object mapNumber = Map.Current.Number;
                PXLongOperation.StartOperation(this,
                    delegate()
                    {
                        JournalEntryImportProcessing.ReleaseImport(mapNumber, isUnsignOperations);
                    });
            }

            yield return Map.Current;
        }

        #endregion

        #region Select Handlers

        protected virtual void mapDetails([PXString] ref string action)
        {
            if (action != null) Operations.Current.Action = action;
        }

        protected virtual IEnumerable exceptions()
        {
            if (Map.Current == null) yield break;

            foreach (GLHistoryEnquiryWithSubResult item in
                GetBalances(this, Map.Current.BranchID, Map.Current.LedgerID, Map.Current.FinPeriodID, Map.Current.BegFinPeriod))
            {
                if (item.EndBalance == 0m) continue;

                if (PXSelect<GLTrialBalanceImportDetails,
                    Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
                        And<GLTrialBalanceImportDetails.mapAccountID, Equal<Required<GLTrialBalanceImportDetails.mapAccountID>>,
                        And<GLTrialBalanceImportDetails.mapSubAccountID, Equal<Required<GLTrialBalanceImportDetails.mapSubAccountID>>>>>>.
                    Select(this, Map.Current.Number, item.AccountID, item.SubID).Count > 0) continue;
                yield return item;
            }
        }

        #endregion

        #region Processing

        protected virtual void ProcessHandler(Dictionary<int, GLTrialBalanceImportDetails> dict, string operation, bool update)
        {
            switch (operation)
            {
                case _VALIDATE_ACTION:

                    foreach (GLTrialBalanceImportDetails item in dict.Values)
                    {
                        bool validSubaccount = true;
                        bool validAccount = SetValue(MapDetails.Cache, item, "ImportAccountCD", "MapAccountID", Messages.ImportAccountCDNotFound, Messages.ImportAccountCDIsEmpty);

                        if (!validAccount)
                        {
                            item.MapSubAccountID = null;
                            PersistErrorAttribute.ClearError(MapDetails.Cache, item, "ImportSubAccountCD");
                        }
                        else if (PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
                        {   validSubaccount = SetValue(MapDetails.Cache, item, "ImportSubAccountCD", "MapSubAccountID", null, Messages.ImportSubAccountCDIsEmpty);
                        }

                        item.Status = validAccount && validSubaccount ? TrialBalanceImportStatusAttribute.VALID : TrialBalanceImportStatusAttribute.ERROR;
                    }

                    foreach (GLTrialBalanceImportDetails item in dict.Values)
                    {
                        PXResultset<GLTrialBalanceImportDetails> duplicates = SearchDuplicates(item);
                        if (duplicates.Count >= 2)
                        {
                            if (item.Status != TrialBalanceImportStatusAttribute.ERROR)
                            {   item.Status = TrialBalanceImportStatusAttribute.DUPLICATE;
                            }
                        }
                        if (update)
                        {   MapDetails.Cache.Update(item);
                        }
                    }
                    
                    break;

                case _MERGE_DUPLICATES_ACTION:

                    foreach (GLTrialBalanceImportDetails item in dict.Values)
                    {
                        PXEntryStatus itemStatus = MapDetails.Cache.GetStatus(item);
                        if (itemStatus != PXEntryStatus.Deleted && itemStatus != PXEntryStatus.InsertedDeleted)
                        {
                            foreach (GLTrialBalanceImportDetails duplicate in SearchDuplicates(item))
                            {
                                if (duplicate.Line != null && duplicate.Line != item.Line && dict.ContainsKey((int)duplicate.Line))
                                {                                 
                                    item.YtdBalance += duplicate.YtdBalance;
                                    item.CuryYtdBalance += duplicate.CuryYtdBalance;
                                    MapDetails.Cache.Delete(duplicate);
                                }
                            }

                            if (item.Status != TrialBalanceImportStatusAttribute.ERROR)
                            {
                                bool accountCDNotValidated = !string.IsNullOrEmpty(item.ImportAccountCD) && item.MapAccountID == null;
                                bool subAccountCDNotValidated = !string.IsNullOrEmpty(item.ImportSubAccountCD) && item.MapSubAccountID == null;
                                item.Status = (accountCDNotValidated || subAccountCDNotValidated) ? TrialBalanceImportStatusAttribute.NEW : TrialBalanceImportStatusAttribute.VALID;
                            }
                            
                            MapDetails.Cache.Update(item);
                        }
                    }
                    
                    break;
            }
        }

        #endregion

        #region Event Handlers
		
	    private void MappingPropertiesInit(object sender, PXImportAttribute.MappingPropertiesInitEventArgs e)
	    {
		    if (!IsCuryYtdBalanceFieldUsable())
		    {
				var fieldName = MapDetails.Cache.GetField(typeof(GLTrialBalanceImportDetails.curyYtdBalance));
				e.Names.Remove(fieldName);

				var displayName = PXUIFieldAttribute.GetDisplayName<GLTrialBalanceImportDetails.curyYtdBalance>(MapDetails.Cache);
				e.DisplayNames.Remove(displayName);
		    }
	    }


	    #region GLTrialBalanceImportMap

        protected virtual void GLTrialBalanceImportMap_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {   return;
            }

            GLTrialBalanceImportMap row = (GLTrialBalanceImportMap)e.Row;
            bool isEditable = IsEditable(row);
            if (isEditable)
            {   CheckTotalBalance(sender, row, IsRequireControlTotal);
            }

            PXUIFieldAttribute.SetVisible<GLTrialBalanceImportMap.totalBalance>(Map.Cache, null, IsRequireControlTotal);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.totalBalance>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.importDate>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.finPeriodID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.description>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.ledgerID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.branchID>(sender, row, isEditable);
            PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportMap.isHold>(sender, row, isEditable);
            Map.Cache.AllowDelete = isEditable;
            Map.Cache.AllowUpdate = isEditable;
            MapDetails.Cache.AllowInsert = isEditable;
            MapDetails.Cache.AllowUpdate = isEditable;
            MapDetails.Cache.AllowDelete = isEditable;
            Actions["Release"].SetEnabled(row.Status == TrialBalanceImportMapStatusAttribute.BALANCED);
            Actions["Process"].SetEnabled(isEditable);
            Actions["ProcessAll"].SetEnabled(isEditable);
            PXImportAttribute.SetEnabled(this, "ImportTemplate", isEditable);

			PXUIFieldAttribute.SetVisible<GLTrialBalanceImportDetails.curyYtdBalance>(MapDetails.Cache, null, IsCuryYtdBalanceFieldUsable());
		}

        #endregion

        #region TrialBalanceTemplate

        protected virtual void TrialBalanceTemplate_ImportAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void TrialBalanceTemplate_ImportSubAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void TrialBalanceTemplate_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            //MapDetails.Update(ConvertToImportDetails(sender, (TrialBalanceTemplate)e.Row));
            MapDetails.Update((TrialBalanceTemplate)e.Row);
        }

        protected virtual void TrialBalanceTemplate_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            //MapDetails.Update(ConvertToImportDetails(sender, (TrialBalanceTemplate)e.Row));
            MapDetails.Update((TrialBalanceTemplate)e.Row);
        }

        protected virtual void TrialBalanceTemplate_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }

        #endregion

        #region GLTrialBalanceImportDetails

        protected virtual void GLTrialBalanceImportDetails_ImportAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void GLTrialBalanceImportDetails_ImportSubAccountCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void GLTrialBalanceImportDetails_MapSubAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var row = e.Row as GLTrialBalanceImportDetails;
            if (row == null || row.MapAccountID == null || e.NewValue == null) return;
            Account acc = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, row.MapAccountID);
            if (acc.IsCashAccount != true)
            {
                return;
            }
            CA.CashAccount cashAccount = PXSelect<CA.CashAccount, Where<CA.CashAccount.accountID, Equal<Required<CA.CashAccount.accountID>>,
                And<CA.CashAccount.subID, Equal<Required<CA.CashAccount.subID>>>>>.Select(sender.Graph, row.MapAccountID, (int?)e.NewValue);
            if (cashAccount == null)
            {
                throw new PXSetPropertyException(Messages.InvalidCashAccountSub);
            }
        }



        protected virtual void GLTrialBalanceImportDetails_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }

            CheckMappingAndBalance(sender, Map.Current, row);
			PXUIFieldAttribute.SetEnabled<GLTrialBalanceImportDetails.curyYtdBalance>(sender, row, IsCuryYtdBalanceFieldUsable() && 
				!string.IsNullOrEmpty(row.AccountCuryID) && row.AccountCuryID != Ledger.Current?.BaseCuryID);
		}

        protected virtual void GLTrialBalanceImportDetails_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            GLTrialBalanceImportDetails oldRow = (GLTrialBalanceImportDetails)e.OldRow;
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (oldRow == null || row == null)
            {   return;
            }

            bool process = false;
            if (row.ImportAccountCD != oldRow.ImportAccountCD)
            {
                process = true;
                row.ImportAccountCDError = null;
                row.MapAccountID = null;
                row.ImportSubAccountCDError = null;
                row.MapSubAccountID = null;
                row.AccountType = null;
                row.Description = null;
            }
            if (row.ImportSubAccountCD != oldRow.ImportSubAccountCD)
            {
                process = true;
                row.ImportSubAccountCDError = null;
                row.MapSubAccountID = null;
            }

            if (process)
            {   ProcessRow(row);
            }

            CalculateRowBalance(oldRow, row);
        }

        protected virtual void GLTrialBalanceImportDetails_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }

            CalculateRowBalance(null, row);
        }

        protected virtual void GLTrialBalanceImportDetails_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }

            CalculateRowBalance(row, null);
        }

        protected virtual void GLTrialBalanceImportDetails_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            GLTrialBalanceImportDetails row = (GLTrialBalanceImportDetails)e.Row;
            if (row == null)
            {   return;
            }

            if (e.Operation != PXDBOperation.Delete)
            {
                CheckMappingAndBalance(sender, Map.Current, row);
            }
        }

        protected virtual void GLTrialBalanceImportMap_IsHold_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            GLTrialBalanceImportMap header = (GLTrialBalanceImportMap)e.Row;
            if (header == null)
            {   return;
            }

            int newStatus = TrialBalanceImportMapStatusAttribute.HOLD;
            if (header.IsHold != true)
            {
                newStatus = TrialBalanceImportMapStatusAttribute.BALANCED;
                PXResultset<GLTrialBalanceImportDetails> rows = MapDetails.Select();
                foreach (GLTrialBalanceImportDetails row in rows)
                {
                    if (!CheckMappingAndBalance(MapDetails.Cache, header, row))
                    {
                        MapDetails.Cache.Update(row);
                        break;
                    }
                }
            }

            header.Status = newStatus;
        }

        protected virtual void GLTrialBalanceImportMap_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<GLTrialBalanceImportMap.ledgerID>(e.Row);
        }

		protected virtual void GLTrialBalanceImportMap_LedgerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!IsCuryYtdBalanceFieldUsable())
			{
				var details = MapDetails.Select().RowCast<GLTrialBalanceImportDetails>();

				foreach (var detail in details)
				{
					detail.CuryYtdBalance = detail.YtdBalance;
					MapDetails.Update(detail);
				}
			}
		}

        #endregion

        #endregion

        #region Private Methods

	    private bool IsCuryYtdBalanceFieldUsable()
	    {
		    if (Ledger.Current == null)
			    return true;

			return Ledger.Current.BalanceType != LedgerBalanceType.Report;
	    }

		private PXResultset<GLTrialBalanceImportDetails> SearchDuplicates(GLTrialBalanceImportDetails item)
		{
			MapDetails.Cache.ClearQueryCacheObsolete();

			if (PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
			{
				return PXSelect<GLTrialBalanceImportDetails,
				 Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
					  And<GLTrialBalanceImportDetails.importAccountCD, Equal<Required<GLTrialBalanceImportDetails.importAccountCD>>,
							And<GLTrialBalanceImportDetails.importSubAccountCD, Equal<Required<GLTrialBalanceImportDetails.importSubAccountCD>>>>>>.
				 Select(this, item.MapNumber, item.ImportAccountCD, item.ImportSubAccountCD);
			}
			else
			{
				return PXSelect<GLTrialBalanceImportDetails,
				 Where<GLTrialBalanceImportDetails.mapNumber, Equal<Required<GLTrialBalanceImportDetails.mapNumber>>,
					  And<GLTrialBalanceImportDetails.importAccountCD, Equal<Required<GLTrialBalanceImportDetails.importAccountCD>>>>>.
				 Select(this, item.MapNumber, item.ImportAccountCD);
			}
		}

		private static bool SetValue(PXCache cache, GLTrialBalanceImportDetails item, string sourceFieldName, string fieldName, string alternativeError, string emptyError)
        {
            string error = null;
            PXUIFieldAttribute.SetError(cache, item, fieldName, null);
            object value = cache.GetValue(item, sourceFieldName);

            if (value == null || value is string && string.IsNullOrEmpty(value.ToString())) error = emptyError;
            else
                try
                {
                    cache.SetValueExt(item, fieldName, value);
                }
                catch (PXSetPropertyException e)
                {
                    error = e.Message;
                }
                finally
                {
                    if (error == null) error = PXUIFieldAttribute.GetError(cache, item, fieldName);
                }

            if (!string.IsNullOrEmpty(error))
            {
                PersistErrorAttribute.SetError(cache, item, sourceFieldName,
                    (error != emptyError && alternativeError != null) ? alternativeError : error);
                return false;
            }
            PersistErrorAttribute.ClearError(cache, item, sourceFieldName);
            return true;
        }

        private static bool IsUnsignOperations(JournalEntryImport graph)
        {
            return graph.GLSetup.Current.TrialBalanceSign != GL.GLSetup.trialBalanceSign.Normal;
        }

        private static Account GetAccount(PXGraph graph, object accountID)
        {
            PXResultset<Account> result =
                PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.
                Select(graph, accountID);
            return result.Count > 0 ? (Account)result[0] : null;
        }

        private static Ledger GetLedger(PXGraph graph, int? ledgerID)
        {
            PXResultset<Ledger> resultset = PXSelect<Ledger,
                Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(graph, ledgerID);
            return resultset.Count > 0 ? resultset[0] : null;
        }

        private bool CanEdit
        {
            get
            {
                return Map.Current != null && IsEditable(Map.Current);
            }
        }

        private static bool IsEditable(GLTrialBalanceImportMap map)
        {
            return map.Status != TrialBalanceImportMapStatusAttribute.RELEASED;
        }

        private static void FillDebitAndCreditAmt(GLTran tran, decimal diff, decimal curyDiff, bool isReversedSign, string accountType)
        {
            if ((accountType == AccountType.Asset || accountType == AccountType.Expense) && diff > 0m ||
                (accountType == AccountType.Liability || accountType == AccountType.Income) &&
                (isReversedSign && diff > 0m || !isReversedSign && diff < 0m))
            {
                tran.DebitAmt = Math.Abs(diff);
                tran.CuryDebitAmt = Math.Abs(curyDiff);
                tran.CreditAmt = 0m;
            }
            else
            {
                tran.DebitAmt = 0m;
                tran.CreditAmt = Math.Abs(diff);
                tran.CuryCreditAmt = Math.Abs(curyDiff);
            }
        }

        private static IEnumerable<GLHistoryEnquiryWithSubResult> GetBalances(JournalEntryImport graph, int? branchID, int? ledgerID, string finPeriodID, string begFinPeriod)
        {
            return GetBalances(graph, IsUnsignOperations(graph), branchID, ledgerID, finPeriodID, begFinPeriod);
        }

        private static IEnumerable<GLHistoryEnquiryWithSubResult> GetBalances(PXGraph graph, bool isReversedSign, int? branchID, int? ledgerID, string finPeriodID, string begFinPeriod)
        {
            if (ledgerID == null || finPeriodID == null) yield break;

            PXSelectBase<GLHistoryByPeriod> cmd = new PXSelectJoinGroupBy<GLHistoryByPeriod,
                                InnerJoin<Account,
                                        On<GLHistoryByPeriod.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
                                InnerJoin<Sub,
                                        On<GLHistoryByPeriod.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>,
                                LeftJoin<GLHistory, On<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
                                        And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
                                        And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
                                        And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
                                        And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
                                LeftJoin<AH, On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
                                        And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
                                        And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
                                        And<GLHistoryByPeriod.subID, Equal<AH.subID>,
                                        And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>>,
                                Where<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>,
                                        And<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>,
	                                    And<GLHistoryByPeriod.branchID, Equal<Required<GLHistoryByPeriod.branchID>>,
                                        And2<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                                            Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
                                        And<
                                            Where2<
                                                Where<Account.type, Equal<AccountType.asset>,
                                                    Or<Account.type, Equal<AccountType.liability>>>,
                                            Or<Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,
                                                And<Where<Account.type, Equal<AccountType.expense>,
                                                Or<Account.type, Equal<AccountType.income>>>>>>>>>>>>,
                                Aggregate<
                                        Sum<AH.finYtdBalance,
                                        Sum<AH.curyFinYtdBalance,
                                        Sum<GLHistory.finPtdDebit,
                                        Sum<GLHistory.finPtdCredit,
                                        Sum<GLHistory.finBegBalance,
                                        Sum<GLHistory.finYtdBalance,
                                        Sum<GLHistory.curyFinBegBalance,
                                        Sum<GLHistory.curyFinYtdBalance,
                                        Sum<GLHistory.curyFinPtdCredit,
                                        Sum<GLHistory.curyFinPtdDebit,
                                        GroupBy<GLHistoryByPeriod.ledgerID,
                                        GroupBy<GLHistoryByPeriod.accountID,
                                        GroupBy<GLHistoryByPeriod.subID,
                                        GroupBy<GLHistoryByPeriod.finPeriodID
                                 >>>>>>>>>>>>>>>>(graph);

            foreach (PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, AH> it in
                cmd.Select(ledgerID, finPeriodID, branchID, begFinPeriod))
            {
                GLHistoryByPeriod baseview = (GLHistoryByPeriod)it;
                Account acct = (Account)it;
                GLHistory ah = (GLHistory)it;
                AH ah1 = (AH)it;

                GLHistoryEnquiryWithSubResult item = new GLHistoryEnquiryWithSubResult();
                item.LedgerID = baseview.LedgerID;
                item.AccountID = baseview.AccountID;
	            item.AccountCD = acct.AccountCD;
                item.AccountType = acct.Type;
                item.SubID = baseview.SubID;
                item.Type = acct.Type;
                item.Description = acct.Description;
                item.CuryID = acct.CuryID;
                item.LastActivityPeriod = baseview.LastActivityPeriod;
                item.PtdCreditTotal = ah.FinPtdCredit;
                item.PtdDebitTotal = ah.FinPtdDebit;
                item.CuryPtdCreditTotal = ah.CuryFinPtdCredit;
                item.CuryPtdDebitTotal = ah.CuryFinPtdDebit;
                bool reverseBalance = isReversedSign &&
                    (item.AccountType == AccountTypeList.Liability || item.AccountType == AccountTypeList.Income);
                item.EndBalance = reverseBalance ? -ah1.FinYtdBalance : ah1.FinYtdBalance;
                item.CuryEndBalance = reverseBalance ? -ah1.CuryFinYtdBalance : ah1.CuryFinYtdBalance;
                item.ConsolAccountCD = acct.GLConsolAccountCD;
                item.BegBalance = item.EndBalance + (reverseBalance ? item.PtdSaldo : -item.PtdSaldo);
                item.CuryBegBalance = item.CuryEndBalance + (reverseBalance ? item.CuryPtdSaldo : -item.CuryPtdSaldo);
                yield return item;
            }
        }

        private bool CheckMappingAndBalance(PXCache sender, GLTrialBalanceImportMap header, GLTrialBalanceImportDetails row)
        {
            bool ok = true;

            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importAccountCD>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapAccountID>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importSubAccountCD>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapSubAccountID>(row, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.ytdBalance>(row, null, null);

            if (header != null && IsEditable(header) && header.IsHold != true)
            {
                if (row.ImportAccountCD == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importAccountCD>
                        (row, row.ImportAccountCD, new PXSetPropertyException(Messages.ImportAccountCDIsEmpty, PXErrorLevel.Error));
                }
                if (row.MapAccountID == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapAccountID>
                        (row, row.MapAccountID, new PXSetPropertyException(Messages.ImportAccountIDIsEmpty, PXErrorLevel.Error));
                }
                if (row.ImportSubAccountCD == null && PXAccess.FeatureInstalled<CS.FeaturesSet.subAccount>() == true)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.importSubAccountCD>
                        (row, row.ImportSubAccountCD, new PXSetPropertyException(Messages.ImportSubAccountCDIsEmpty, PXErrorLevel.Error));
                }
                if (row.MapSubAccountID == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.mapSubAccountID>
                        (row, row.MapSubAccountID, new PXSetPropertyException(Messages.ImportSubAccountIDIsEmpty, PXErrorLevel.Error));
                }
                if (row.YtdBalance == null)
                {   ok = false;
                    sender.RaiseExceptionHandling<GLTrialBalanceImportDetails.ytdBalance>
                        (row, row.YtdBalance, new PXSetPropertyException(Messages.ImportYtdBalanceIsEmpty, PXErrorLevel.Error));
                }
            }

            return ok;
        }

        private bool IsRequireControlTotal
        {
            get
            {
                return GLSetup.Current != null && GLSetup.Current.RequireControlTotal == true;
            }
        }

        private void ProcessRow(GLTrialBalanceImportDetails row)
        {
            if (row.Status == TrialBalanceImportStatusAttribute.VALID && row.Line != null)
            {   
                Dictionary<int, GLTrialBalanceImportDetails> dict = new Dictionary<int, GLTrialBalanceImportDetails>();
                dict.Add((int)row.Line, row);
                ProcessHandler(dict, _VALIDATE_ACTION, false);
            }
            else
            {   row.Status = TrialBalanceImportStatusAttribute.NEW;
            }
        }

        private void CalculateRowBalance(GLTrialBalanceImportDetails oldRow, GLTrialBalanceImportDetails row)
        {
            GLTrialBalanceImportMap header = Map.Current;
            if (header == null)
            {   return;
            }

            if (ReferenceEquals(oldRow, row))
            {
                header.DebitTotalBalance = 0m;
                header.CreditTotalBalance = 0m;
                PXResultset<GLTrialBalanceImportDetails> rows = MapDetails.Select();
                
                foreach (GLTrialBalanceImportDetails item in rows)
                {
                    PXEntryStatus status = Map.Cache.GetStatus(item);
                    if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
                    {
                        Account account = GetRowAccount(item);
                        if (account != null)
                        {   ChangeTotalBalance(header, account, item.YtdBalance);
                        }
                    }
                }
            }
            else
            {   decimal? value = 0m;
                Account rowAccount = GetRowAccount(row);
                Account oldRowAccount = GetRowAccount(oldRow);

                if (rowAccount == null && oldRowAccount == null)
                {   return;
                }
                else if (rowAccount == null)
                {
                    value = -oldRow.YtdBalance;
                    rowAccount = oldRowAccount;
                }
                else if (oldRowAccount == null)
                {   value = row.YtdBalance;
                }
                else 
                {   if (rowAccount.Type != oldRowAccount.Type)
                    {
                        value = row.YtdBalance;
                        ChangeTotalBalance(header, oldRowAccount, -oldRow.YtdBalance);
                    }
                    else
                    {   value = row.YtdBalance - oldRow.YtdBalance;
                    }
                }

                ChangeTotalBalance(header, rowAccount, value);
            }
            if (!IsRequireControlTotal)
            {   header.TotalBalance = header.CreditTotalBalance;
            }
        }

        private void ChangeTotalBalance(GLTrialBalanceImportMap header, Account account, decimal? value)
        {
            if (value != 0)
            {
                switch (account.Type)
                {
                    case AccountType.Asset:
                    case AccountType.Expense:
                        header.DebitTotalBalance += value;
                    break;
                    case AccountType.Liability:
                    case AccountType.Income:
                        header.CreditTotalBalance += IsUnsignOperations(this) ? -value : value;
                    break;
                }

                PXEntryStatus status = Map.Cache.GetStatus(header);
                if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
                {   Map.Cache.Update(header);
                }
            }
        }

        private Account GetRowAccount(GLTrialBalanceImportDetails row)
        {
            return row != null && row.YtdBalance != null && row.MapAccountID != null ? GetAccount(this, row.MapAccountID) : null;
        }

        private void CheckTotalBalance(PXCache sender, GLTrialBalanceImportMap header, bool require)
        {
            sender.RaiseExceptionHandling<GLTrialBalanceImportMap.debitTotalBalance>(header, null, null);
            sender.RaiseExceptionHandling<GLTrialBalanceImportMap.creditTotalBalance>(header, null, null);

            if (header.IsHold != true)
            {
                bool debitIncorrect = require ? header.DebitTotalBalance != header.TotalBalance : header.DebitTotalBalance != header.CreditTotalBalance;
                bool creditIncorrect = require ? header.CreditTotalBalance != header.TotalBalance : false;

                if (debitIncorrect)
                {   sender.RaiseExceptionHandling<GLTrialBalanceImportMap.debitTotalBalance>
						(header, header.DebitTotalBalance, new PXSetPropertyException(Messages.DocumentIsOutOfBalancePleaseReview, PXErrorLevel.Error));
                }
                if (creditIncorrect)
                {   sender.RaiseExceptionHandling<GLTrialBalanceImportMap.creditTotalBalance>
						(header, header.CreditTotalBalance, new PXSetPropertyException(Messages.DocumentIsOutOfBalancePleaseReview, PXErrorLevel.Error));
                }
            }
        }

        #endregion

        #region Implementation of PXImportAttribute.IPXPrepareItems

        public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            if (viewName == _IMPORTTEMPLATE_VIEWNAME && Map.Current != null)
            {
                string value = Map.Current.Number;
                if (keys.Contains(_mapNumberFieldName)) keys[_mapNumberFieldName] = value;
                else keys.Add(_mapNumberFieldName, value);
            }
            return true;
        }

        public bool RowImporting(string viewName, object row)
        {
            return row == null;
        }

        public bool RowImported(string viewName, object row, object oldRow)
        {
            return oldRow == null;
        }

	    public virtual void PrepareItems(string viewName, IEnumerable items)
	    {
		    
	    }

        #endregion
    }
}
