using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.TM;
using PX.CS.Contracts.Interfaces;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.TaxProvider;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Bql;

namespace PX.Objects.CA
{
	public class CATranEntry : PXGraph<CATranEntry, CAAdj>, PX.Objects.GL.IVoucherEntry
	{
		public PXAction DeleteButton
		{
			get
			{
				return this.Delete;
			}
		}
		#region Cache Attached Events
		[Account(
			typeof(CASplit.branchID),
			typeof(Search<Account.accountID,
				Where2<Where<Account.curyID, Equal<Current<CAAdj.curyID>>, Or<Account.curyID, IsNull>>,
                    And<Match<Current<AccessInfo.userName>>>>>),
			DisplayName = "Offset Account", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Account.description), 
			CacheGlobal = false,
			AvoidControlAccounts = true)]
		[PXDefault()]
		protected virtual void CASplit_AccountID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		
		public PXCache dailycache
		{
			get
			{
				return this.Caches[typeof(CADailySummary)];
			}
		}

		public PXCache catrancache
		{
			get
			{
				return this.Caches[typeof(CATran)];
			}
		}

		public PXCache gltrancache
		{
			get
			{
				return this.Caches[typeof(GLTran)];
			}
		}

		public CATranEntry()
		{
			CASetup setup = casetup.Current;
			PXUIFieldAttribute.SetDisplayName<Account.description>(Caches[typeof(Account)], Messages.AccountDescription);
			this.FieldSelecting.AddHandler<CAAdj.tranID_CATran_batchNbr>(CAAdj_TranID_CATran_BatchNbr_FieldSelecting);
			PXUIFieldAttribute.SetVisible<CASplit.projectID>(CASplitRecords.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.CA));
			PXUIFieldAttribute.SetVisible<CASplit.taskID>(CASplitRecords.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.CA));
			PXUIFieldAttribute.SetVisible<CASplit.nonBillable>(CASplitRecords.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.CA));

			Approval.Cache.AllowUpdate = false;
            Approval.Cache.AllowInsert = false;
            Approval.Cache.AllowDelete = false;

			CAExpenseHelper.InitBackwardEditorHandlers(this);
        }

		#region Extensions

		public class CATranEntryDocumentExtension : DocumentWithLinesGraphExtension<CATranEntry>
		{
			#region Mapping

			public override void Initialize()
			{
				base.Initialize();

				Documents = new PXSelectExtension<Document>(Base.CAAdjRecords);
				Lines = new PXSelectExtension<DocumentLine>(Base.CASplitRecords);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CAAdj))
				{
					HeaderTranPeriodID = typeof(CAAdj.tranPeriodID),
					HeaderDocDate = typeof(CAAdj.tranDate)
				};
			}

			protected override DocumentLineMapping GetDocumentLineMapping()
			{
				return new DocumentLineMapping(typeof(CASplit));
			}

			#endregion

			protected override bool ShouldUpdateLinesOnDocumentUpdated(Events.RowUpdated<Document> e)
			{
				return base.ShouldUpdateLinesOnDocumentUpdated(e)
					   || !e.Cache.ObjectsEqual<Document.headerDocDate>(e.Row, e.OldRow);
			}

			protected override void ProcessLineOnDocumentUpdated(Events.RowUpdated<Document> e,
			    DocumentLine line)
			{
				base.ProcessLineOnDocumentUpdated(e, line);

				if (!e.Cache.ObjectsEqual<Document.headerDocDate>(e.Row, e.OldRow))
				{
					Lines.Cache.SetDefaultExt<DocumentLine.tranDate>(line);
				}
			}
		}

		#endregion

		public TaxZone TAXZONE
		{
			get
			{
				return taxzone.Select();
			}
		}

		#region Buttons
		public PXAction<CAAdj> hold;
		[PXUIField(DisplayName = "Hold", Visible = false)]
		[PXButton]
		protected virtual IEnumerable Hold(PXAdapter adapter)
		{
			return adapter.Get();
		}
        public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
        {
            if (viewName.ToLower() == "caadjrecords" && values != null)
            {
                if (IsImport || IsExport || IsMobile || IsContractBasedAPI)
                {
                    CAAdjRecords.Cache.Locate(keys);
                    if (values.Contains("Hold") && values["Hold"] != PXCache.NotSetValue && values["Hold"] != null)
                    {
                        var hold = CAAdjRecords.Current.Hold ?? false;
                        if (Convert.ToBoolean(values["Hold"]) != hold)
                        {
                            ((PXAction<CAAdj>)this.Actions["Hold"]).PressImpl(false);
                        }
                    }
                }

                values["Hold"] = PXCache.NotSetValue;
            }

            return base.ExecuteUpdate(viewName, keys, values, parameters);
        }

        public PXAction<CAAdj> flow;
		[PXUIField(DisplayName = "Flow")]
		protected virtual IEnumerable Flow(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<CAAdj> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter,
		[PXInt]
		[PXIntList(new int[] { 1, 2 }, new string[] { "Persist", "Update" })]
		int? actionID,
		[PXBool]
		bool refresh,
		[PXString]
		string actionName
		)
		{
			var result = new List<CAAdj>();
			if (actionName != null)
			{
				PXAction run = this.Actions[actionName];
				if (run != null)
					foreach (var item in action.Press(adapter)) ;
			}
			if (refresh)
			{
				foreach (CAAdj order in adapter.Get<CAAdj>())
					result.Add(CAAdjRecords.Search<CAAdj.adjRefNbr>(order.AdjRefNbr));
			}
			else
			{
				foreach (CAAdj order in adapter.Get<CAAdj>())
					result.Add(order);
			}
			switch (actionID)
			{
				case 1:
					Save.Press();
					break;
				case 2:
					break;
			}

			return result;
		}
		public PXAction<CAAdj> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.InquiriesFolder)]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 1 }, new string[] { "Activities" })]
			int? inquiryID
			)
		{
					if (CAAdjRecords.Current != null)
		    {
						this.Activity.ButtonViewAllActivities.PressButton();
			}

			return adapter.Get();
		}

		public PXAction<CAAdj> Release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			Save.Press();

			CAAdj adj = CAAdjRecords.Current;
			List<CARegister> registerList = new List<CARegister>();
			registerList.Add(CATrxRelease.CARegister(adj));
			PXLongOperation.StartOperation(this, () => CATrxRelease.GroupRelease(registerList, false));
			List<CAAdj> ret = new List<CAAdj>();
			ret.Add(adj);
			return ret;
		}
		protected bool reversingContext;
		public PXAction<CAAdj> Reverse;
		[PXUIField(DisplayName = "Reverse", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable reverse(PXAdapter adapter)
		{
			CAAdj current = CAAdjRecords.Current;
			if (current.Released != true)
				return adapter.Get();

			CAAdj adj =(CAAdj)CAAdjRecords.Cache.CreateCopy(CAAdjRecords.Current);
			#region set fields of adj
			adj.AdjRefNbr = null;
			adj.Status = null;
			adj.Approved = null;
			adj.Hold = null;
			adj.Released = null;
			adj.ClearDate = null;
			adj.Cleared = false;
			adj.TranID = null;
			adj.NoteID = null;
			adj.CurySplitTotal = null;
			adj.CuryVatExemptTotal = null;
			adj.CuryVatTaxableTotal = null;
			adj.SplitTotal = null;
			adj.VatExemptTotal = null;
			adj.VatTaxableTotal = null;
			adj.CuryTaxRoundDiff *= -1;
			adj.CuryControlAmt *= -1;
			adj.CuryTaxAmt *= -1;
			adj.CuryTaxTotal *= -1;
			adj.CuryTranAmt *= -1;
			adj.TaxRoundDiff *= -1;
			adj.TaxAmt *= -1;
			adj.ControlAmt *= -1;
			adj.TaxTotal *= -1;
			adj.TranAmt *= -1;
			adj.EmployeeID = null;
			#endregion

			List<Tuple<CASplit,CASplit>> splits = new List<Tuple<CASplit,CASplit>>();
			foreach (CASplit split in CASplitRecords.Select())
			{
				CASplit newSplit = (CASplit)CASplitRecords.Cache.CreateCopy(split);
				newSplit.AdjRefNbr = null;
				newSplit.NoteID = null;
				newSplit.CuryTranAmt *= -1;
				newSplit.CuryUnitPrice *= -1;
				newSplit.CuryTaxAmt *= -1;
				newSplit.CuryTaxableAmt *= -1;
				newSplit.TranAmt *= -1;
				newSplit.UnitPrice *= -1;
				newSplit.TaxAmt *= -1;
				newSplit.TaxableAmt *= -1;
				splits.Add(new Tuple<CASplit, CASplit>(split, newSplit));
			}
			List<CATaxTran> taxes = new List<CATaxTran>();
			foreach (CATaxTran taxTran in Taxes.Select())
			{
				CATaxTran newTaxTran = new CATaxTran();
				newTaxTran.AccountID = taxTran.AccountID;
				newTaxTran.BranchID = taxTran.BranchID;
				newTaxTran.FinPeriodID = taxTran.FinPeriodID;
				newTaxTran.SubID = taxTran.SubID;
				newTaxTran.TaxBucketID = taxTran.TaxBucketID;
				newTaxTran.TaxID = taxTran.TaxID;
				newTaxTran.TaxType = taxTran.TaxType;
				newTaxTran.TaxZoneID = taxTran.TaxZoneID;
				newTaxTran.TranDate = taxTran.TranDate;
				newTaxTran.VendorID = taxTran.VendorID;
				newTaxTran.CuryID = taxTran.CuryID;
				newTaxTran.Description = taxTran.Description;
				newTaxTran.NonDeductibleTaxRate = taxTran.NonDeductibleTaxRate;
				newTaxTran.TaxRate = taxTran.TaxRate;
				newTaxTran.CuryTaxableAmt = -taxTran.CuryTaxableAmt;
				newTaxTran.CuryExemptedAmt = -taxTran.CuryExemptedAmt;
				newTaxTran.CuryTaxAmt = -taxTran.CuryTaxAmt;
				newTaxTran.CuryTaxAmtSumm = -taxTran.CuryTaxAmtSumm;
				newTaxTran.CuryExpenseAmt = -taxTran.CuryExpenseAmt;
				newTaxTran.TaxableAmt = -taxTran.TaxableAmt;
				newTaxTran.ExemptedAmt = -taxTran.ExemptedAmt;
				newTaxTran.TaxAmt = -taxTran.TaxAmt;
				newTaxTran.ExpenseAmt = -taxTran.ExpenseAmt;

				taxes.Add(newTaxTran);
			}

			finperiod.Cache.Current = finperiod.View.SelectSingleBound(new object[] { adj });
			FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<Batch.finPeriodID, Batch.branchID>(CAAdjRecords.Cache, adj, finperiod);

			Clear();
			reversingContext = true;
			CAAdj insertedAdj = CAAdjRecords.Insert(adj);
			PXNoteAttribute.CopyNoteAndFiles(CAAdjRecords.Cache, current, CAAdjRecords.Cache, insertedAdj);
			foreach (Tuple<CASplit, CASplit> pair in splits)
			{
				CASplit newSplit = pair.Item2;
				newSplit = CASplitRecords.Insert(newSplit);
				PXNoteAttribute.CopyNoteAndFiles(CASplitRecords.Cache, pair.Item1, CASplitRecords.Cache, newSplit);
			}
			reversingContext = false;
			foreach (CATaxTran newTaxTran in taxes)
			{
				Taxes.Insert(newTaxTran);
			}
			//We should reenter totals depending on taxes as TaxAttribute does not recalculate them if externalCall==false
			CAAdjRecords.Cache.SetValue<CAAdj.taxRoundDiff>(insertedAdj, adj.TaxRoundDiff);
			CAAdjRecords.Cache.SetValue<CAAdj.curyTaxRoundDiff>(insertedAdj, adj.CuryTaxRoundDiff);
			CAAdjRecords.Cache.SetValue<CAAdj.taxTotal>(insertedAdj, adj.TaxAmt);
			CAAdjRecords.Cache.SetValue<CAAdj.curyTaxTotal>(insertedAdj, adj.CuryTaxAmt);
			CAAdjRecords.Cache.SetValue<CAAdj.tranAmt>(insertedAdj, adj.TranAmt);
			CAAdjRecords.Cache.SetValue<CAAdj.curyTranAmt>(insertedAdj, adj.CuryTranAmt);
            insertedAdj = CAAdjRecords.Update(insertedAdj);

			FinPeriodUtils.CopyPeriods<CAAdj, CAAdj.finPeriodID, CAAdj.tranPeriodID>(CAAdjRecords.Cache, current, insertedAdj);


			return new List<CAAdj> { insertedAdj };
		}

		#endregion

		#region Selects
		[PXViewName(Messages.CashTransactions)]
		[PXCopyPasteHiddenFields(typeof(CAAdj.cleared), typeof(CAAdj.clearDate))]
		public PXSelect<CAAdj, Where<CAAdj.draft, Equal<False>>> CAAdjRecords; 
		public PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>>> CurrentDocument;
		[PXImport(typeof(CAAdj))]
		public PXSelect<CASplit, Where<CASplit.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>,
															 And<CASplit.adjTranType, Equal<Current<CAAdj.adjTranType>>>>> CASplitRecords;
		public PXSelect<CATax, Where<CATax.adjTranType, Equal<Current<CAAdj.adjTranType>>, And<CATax.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>>>, OrderBy<Asc<CATax.adjTranType, Asc<CATax.adjRefNbr, Asc<CATax.taxID>>>>> Tax_Rows;
		public PXSelectJoin<CATaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<CATaxTran.taxID>>>, Where<CATaxTran.module, Equal<BatchModule.moduleCA>, And<CATaxTran.tranType, Equal<Current<CAAdj.adjTranType>>, And<CATaxTran.refNbr, Equal<Current<CAAdj.adjRefNbr>>>>>> Taxes;

		// We should use read only view here
		// to prevent cache merge because it
		// used only as a shared BQL query.
		// 
		public PXSelectReadonly2<CATaxTran, 
			LeftJoin<Tax, On<Tax.taxID, Equal<CATaxTran.taxID>>>, 
			Where<CATaxTran.module, Equal<BatchModule.moduleCA>, 
				And<CATaxTran.tranType, Equal<Current<CAAdj.adjTranType>>, 
				And<CATaxTran.refNbr, Equal<Current<CAAdj.adjRefNbr>>, 
			And<Tax.taxType, Equal<CSTaxType.use>>>>>> UseTaxes;

		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CAAdj.cashAccountID>>>> cashAccount;
		public PXSelect<CASetupApproval, Where<Current<CAAdj.adjTranType>, Equal<CATranType.cAAdjustment>>> SetupApproval;
        [PXViewName(Messages.Approval)]
		public EPApprovalAutomation<CAAdj, CAAdj.approved, CAAdj.rejected, CAAdj.hold, CASetupApproval> Approval;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CAAdj.curyInfoID>>>> currencyinfo;
		public ToggleCurrency<CAAdj> CurrencyView;
		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;

		public PXSetup<OrganizationFinPeriod,
							Where<OrganizationFinPeriod.finPeriodID, Equal<Current<CAAdj.finPeriodID>>,
									And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<CAAdj.branchID>>>>>
							finperiod;

		public PXSetup<CASetup> casetup;
		public PXSetup<GLSetup> glsetup;
		public PXSelect<CATran> catran;
		public PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<CAAdj.taxZoneID>>>> taxzone;
		public CMSetupSelect CMSetup;

		public CRActivityList<CAAdj>
			Activity;

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelect<CAExpense> caExpense;
		#endregion

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		#region EPApproval Cahce Attached
		[PXDBDate()]
		[PXDefault(typeof(CAAdj.tranDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid()]
		[PXDefault(typeof(Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<CAAdj.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(CAAdj.tranDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[CurrencyInfo(typeof(CAAdj.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(CAAdj.curyTranAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(CAAdj.tranAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region Functions
		public virtual void updateAmountPrice(CASplit oldSplit, CASplit newSplit)
		{
			CATranDetailHelper.UpdateNewTranDetailCuryTranAmtOrCuryUnitPrice(CASplitRecords.Cache, oldSplit, newSplit);
			}
		#endregion

		#region CATran Envents
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleCA>>>))]
		public virtual void CATran_BatchNbr_CacheAttached(PXCache sender)
		{
		}

		protected virtual void CATran_BatchNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region CAAdj Events
		protected virtual void CAAdj_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CAAdj adj = (CAAdj)e.Row;

			if (casetup.Current.RequireControlTotal != true)
			{
				sender.SetValue<CAAdj.curyControlAmt>(adj, adj.CuryTranAmt);
			}
			else
			{
				if (adj.Hold != true)
					if (adj.CuryControlAmt != adj.CuryTranAmt)
					{
						sender.RaiseExceptionHandling<CAAdj.curyControlAmt>(adj, adj.CuryControlAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<CAAdj.curyControlAmt>(adj, adj.CuryControlAmt, null);
					}
			}

			bool checkControlTaxTotal = casetup.Current.RequireControlTaxTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

			if (adj.CuryTaxTotal != adj.CuryTaxAmt && adj.Hold != true && checkControlTaxTotal)
			{
				sender.RaiseExceptionHandling<CAAdj.curyTaxAmt>(adj, adj.CuryTaxAmt, new PXSetPropertyException(AP.Messages.TaxTotalAmountDoesntMatch));
			}
			else
			{
				if (checkControlTaxTotal)
				{
					sender.RaiseExceptionHandling<CAAdj.curyTaxAmt>(adj, null, null);
				}
				else
				{
					sender.SetValueExt<CAAdj.curyTaxAmt>(adj, adj.CuryTaxTotal != null && adj.CuryTaxTotal != 0 ? adj.CuryTaxTotal : 0m);
				}
			}

			sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, null, null);

			if (adj.Hold != true && adj.Released != true && adj.TaxRoundDiff != 0)
			{
				if (checkControlTaxTotal)
				{
					if (Math.Abs(adj.TaxRoundDiff.Value) > Math.Abs(glsetup.Current.RoundingLimit.Value))
					{
						sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, adj.CuryTaxRoundDiff,
							new PXSetPropertyException(AP.Messages.RoundingAmountTooBig, currencyinfo.Current.BaseCuryID, PXDBQuantityAttribute.Round(adj.TaxRoundDiff),
								PXDBQuantityAttribute.Round(glsetup.Current.RoundingLimit)));
					}
				}
				else
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
					{
						sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, adj.CuryTaxRoundDiff,
							new PXSetPropertyException(AP.Messages.CannotEditTaxAmtWOFeature));
					}
					else
					{
						sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, adj.CuryTaxRoundDiff,
							new PXSetPropertyException(Messages.CannotEditTaxAmtWOCASetup));
					}
				}
			}
		}

		protected virtual void CAAdj_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
			if (adj == null) return;
			CMSetup cmsetup = CMSetup.Current;

			if (taxzone.Current != null && adj.TaxZoneID != taxzone.Current.TaxZoneID)
			{
				taxzone.Current = null;
			}
			bool requiredFieldsFilled = adj.CashAccountID != null && adj.EntryTypeID != null;
			bool adjNotReleased = adj.Released != true;
			bool isNotRejected = adj.Rejected != true;

			sender.AllowUpdate = adjNotReleased;
			sender.AllowDelete = adjNotReleased;

			CASplitRecords.Cache.AllowInsert = adjNotReleased && requiredFieldsFilled;
			CASplitRecords.Cache.AllowUpdate = adjNotReleased;
			CASplitRecords.Cache.AllowDelete = adjNotReleased;

			PXUIFieldAttribute.SetEnabled(sender, adj, false);
			PXUIFieldAttribute.SetEnabled<CAAdj.adjRefNbr>(sender, adj, true);

			CashAccount cashaccount = (CashAccount)PXSelectorAttribute.Select<CAAdj.cashAccountID>(sender, adj);
			bool requireControlTotal = (bool)casetup.Current.RequireControlTotal;
			bool clearEnabled = (adj.Released != true) && (cashaccount != null) && (cashaccount.Reconcile == true);
            bool hasNoDetailRecords = !this.CASplitRecords.Any();
            
			if (adjNotReleased)
			{
				PXUIFieldAttribute.SetEnabled<CAAdj.hold>(sender, adj, adjNotReleased && isNotRejected);
                PXUIFieldAttribute.SetEnabled<CAAdj.cashAccountID>(sender, adj, hasNoDetailRecords);
				PXUIFieldAttribute.SetEnabled<CAAdj.entryTypeID>(sender, adj, hasNoDetailRecords);
                PXUIFieldAttribute.SetEnabled<CAAdj.extRefNbr>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.tranDate>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.finPeriodID>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.tranDesc>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.taxZoneID>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.curyControlAmt>(sender, adj, requireControlTotal);
				PXUIFieldAttribute.SetEnabled<CAAdj.cleared>(sender, adj, clearEnabled);
				PXUIFieldAttribute.SetEnabled<CAAdj.clearDate>(sender, adj, clearEnabled && (adj.Cleared == true));
				CAEntryType entryType = PXSelect<CAEntryType, Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.Select(this, adj.EntryTypeID);
				bool isReclassyPaymentEntry = (entryType != null && entryType.UseToReclassifyPayments == true);
				PXUIFieldAttribute.SetEnabled<CASplit.inventoryID>(this.CASplitRecords.Cache, null, !isReclassyPaymentEntry);
			}
			
			Release.SetEnabled((adj.AdjTranType == CATranType.CAAdjustment) && adjNotReleased && (adj.Hold != true));
			Reverse.SetEnabled(adj.Released == true && adj.AdjTranType == CATranType.CAAdjustment);
			PXUIFieldAttribute.SetVisible<CAAdj.curyControlAmt>(sender, null, requireControlTotal);
			PXUIFieldAttribute.SetVisible<CAAdj.curyID>(sender, adj, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisible<CAAdj.approved>(sender, null, casetup.Current.RequestApproval == true && adj.AdjTranType == CATranType.CAAdjustment);
			PXUIFieldAttribute.SetRequired<CAAdj.curyControlAmt>(sender, requireControlTotal);

			Approval.AllowSelect = casetup.Current.RequestApproval == true && adj.AdjTranType == CATranType.CAAdjustment;

            bool isReclassification = adj.PaymentsReclassification == true;
            PXUIFieldAttribute.SetVisible<CASplit.cashAccountID>(this.CASplitRecords.Cache, null, isReclassification);
			PXUIFieldAttribute.SetEnabled<CASplit.cashAccountID>(this.CASplitRecords.Cache, null, isReclassification);
            PXUIFieldAttribute.SetRequired<CAAdj.extRefNbr>(sender, casetup.Current.RequireExtRefNbr == true);

			bool RequireTaxControlTotal = PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() &&
			                              (casetup.Current.RequireControlTaxTotal == true);

			PXUIFieldAttribute.SetVisible<CAAdj.curyTaxAmt>(sender, adj, RequireTaxControlTotal);
			PXUIFieldAttribute.SetEnabled<CAAdj.curyTaxAmt>(sender, adj, RequireTaxControlTotal);
			PXUIFieldAttribute.SetRequired<CAAdj.curyTaxAmt>(sender, RequireTaxControlTotal);

			PXUIFieldAttribute.SetEnabled<CAAdj.taxCalcMode>(sender, adj, adj.Released != true);

			bool showRoundingDiff = adj.CuryTaxRoundDiff != 0;
			PXUIFieldAttribute.SetVisible<CAAdj.curyTaxRoundDiff>(sender, adj, showRoundingDiff);

			if (UseTaxes.Select().Count != 0)
			{
				sender.RaiseExceptionHandling<CAAdj.curyTaxTotal>(adj, adj.CuryTaxTotal, new PXSetPropertyException(TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}

			PXUIFieldAttribute.SetVisible<CAAdj.usesManualVAT>(sender, adj, adj.UsesManualVAT == true);
			Taxes.Cache.AllowDelete = Taxes.Cache.AllowDelete && adj.UsesManualVAT != true;
			Taxes.Cache.AllowInsert = Taxes.Cache.AllowInsert && adj.UsesManualVAT != true;
			Taxes.Cache.AllowUpdate = Taxes.Cache.AllowUpdate && adj.UsesManualVAT != true;
		}
		protected virtual void CAAdj_EntryTypeId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
		    if (adj != null)
		    {
		        CAEntryType entryType = PXSelect<CAEntryType,
		            Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.
		            Select(this, adj.EntryTypeID);
		        if (entryType != null)
		        {
                    adj.DrCr = entryType.DrCr;
                    adj.PaymentsReclassification = entryType.UseToReclassifyPayments == true;
                    if (entryType.UseToReclassifyPayments == true && adj.CashAccountID.HasValue)
                    {
                        CashAccount availableAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, NotEqual<Required<CashAccount.cashAccountID>>,
                            And<CashAccount.curyID, Equal<Required<CashAccount.curyID>>>>>.SelectWindowed(sender.Graph, 0, 1, adj.CashAccountID, adj.CuryID);
                        if (availableAccount == null)
                        {
                            sender.RaiseExceptionHandling<CAAdj.entryTypeID>(adj, null, new PXSetPropertyException(Messages.EntryTypeRequiresCashAccountButNoOneIsConfigured, PXErrorLevel.Warning, adj.CuryID));
                        }
                    }		            
		        }
				sender.SetDefaultExt<CAAdj.taxCalcMode>(adj);
				sender.SetDefaultExt<CAAdj.taxZoneID>(adj);
		    }
		}
		protected virtual void CAAdj_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = casetup.Current.RequestApproval == true || casetup.Current.HoldEntry == true;
		}
		protected virtual void CAAdj_Status_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = casetup.Current.RequestApproval == true || casetup.Current.HoldEntry == true
										? CATransferStatus.Hold
										: CATransferStatus.Balanced;
		}
		protected virtual void CAAdj_Cleared_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
			if (adj.Cleared == true)
			{
				if (adj.ClearDate == null)
				{
					adj.ClearDate = adj.TranDate;
				}
			}
			else
			{
				adj.ClearDate = null;
			}
		}
		protected virtual void CAAdj_TranDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
			if (adj == null) return;

			CurrencyInfoAttribute.SetEffectiveDate<CAAdj.tranDate>(sender, e);
		}

		protected virtual void CAAdj_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj adj = (CAAdj)e.Row;

			adj.Cleared = false;
			adj.ClearDate = null;
			if (adj.CashAccountID == null)
			{
				return;
			}
			if (cashAccount.Current == null || cashAccount.Current.CashAccountID != adj.CashAccountID)
			{
				cashAccount.Current = (CashAccount)PXSelectorAttribute.Select<CAAdj.cashAccountID>(sender, adj);
			}
			if (cashAccount.Current.Reconcile != true)
			{
				adj.Cleared = true;
				adj.ClearDate = adj.TranDate;
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || sender.GetValuePending<CAAdj.curyID>(adj) == null)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<CAAdj.curyInfoID>(sender, adj);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<CAAdj.tranDate>(adj, adj.TranDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						adj.CuryID = info.CuryID;
					}
				}
			}
			sender.SetDefaultExt<CAAdj.entryTypeID>(e.Row);
		}		

		protected virtual void CAAdj_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CAAdj adj = (CAAdj)e.Row;

			PXDefaultAttribute.SetPersistingCheck<CAAdj.extRefNbr>(sender, adj,
				casetup.Current.RequireExtRefNbr == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) && adj.Status == CATransferStatus.Released)
			{
				e.Cancel = true;
				throw new PXException(Messages.ReleasedDocCanNotBeDel);
			}
		}


		protected virtual void CAAdj_Approved_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = casetup.Current != null ? casetup.Current.RequestApproval != true : true;
		}

		protected virtual void CAAdj_Rejected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CAAdj doc = (CAAdj)e.Row;

			if (doc.Rejected == true)
			{
				doc.Approved = false;
				doc.Hold = true;
				doc.Status = CATransferStatus.Rejected;
				cache.RaiseFieldUpdated<CAAdj.hold>(e.Row, null);
			}
		}

		protected virtual void CAAdj_TranID_CATran_BatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null || e.IsAltered)
			{
				string ViewName = null;
				PXCache cache = sender.Graph.Caches[typeof(CATran)];
				PXFieldState state = cache.GetStateExt<CATran.batchNbr>(null) as PXFieldState;

				if (state != null)
				{
					ViewName = state.ViewName;
				}

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, false, false, 0, 0, null, null, null, null, null, null,
                    PXErrorLevel.Undefined, false, true, true, PXUIVisibility.Visible, ViewName, null, null);
			}
		}

		#endregion

		#region CASplit Events

		protected virtual void CASplit_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

			if (adj == null || adj.EntryTypeID == null || split == null)
                return;

            e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).AccountID;
            e.Cancel = e.NewValue != null; 
		}

		protected virtual void CASplit_AccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CATranDetailHelper.OnAccountIdFieldUpdatedEvent(cache, e);

			CASplit caSplit = (CASplit)e.Row;

			if (caSplit.InventoryID == null)
				cache.SetDefaultExt<CASplit.taxCategoryID>(e.Row);
		}

		protected virtual void CASplit_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

			if (adj == null || adj.EntryTypeID == null || split == null)
                return;

		    e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).SubID;
		    e.Cancel = e.NewValue != null; 
		}

        protected virtual void CASplit_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            CAAdj adj = CAAdjRecords.Current;
            CASplit split = e.Row as CASplit;

            if (adj == null || adj.EntryTypeID == null || split == null)
				return;

	        e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).BranchID;
			e.Cancel = e.NewValue != null; 

        }

        private CASplit GetDefaultAccountValues(PXGraph graph, int? cashAccountID, string entryTypeID)
            {
            return CATranDetailHelper.CreateCATransactionDetailWithDefaultAccountValues<CASplit>(graph, cashAccountID, entryTypeID);
        }

		protected virtual void CASplit_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
            CATranDetailHelper.OnCashAccountIdFieldDefaultingEvent(sender, e);

		}

		protected virtual void CASplit_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
			CATranDetailHelper.OnCashAccountIdFieldVerifyingEvent(sender, e, CAAdjRecords.Current.CashAccountID);
        }

		protected virtual void CASplit_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            CATranDetailHelper.OnCashAccountIdFieldUpdatedEvent(sender, e);
		}

		protected virtual void CASplit_TranDesc_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

            if (adj?.EntryTypeID == null)
                return;

				CAEntryType entryType = PXSelect<CAEntryType,
                                       Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>
                                    .Select(this, adj.EntryTypeID);

				if (entryType != null)
				{
					e.NewValue = entryType.Descr;
				}
			}

		protected virtual void CASplit_CuryTranAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;

			if (split == null)
                return;

			if (casetup.Current.RequireControlTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
			{
				CAAdj adj = CAAdjRecords.Current;

				if (adj == null)
                    return;

				decimal? newVal = 0;

				if (String.IsNullOrEmpty(split.TaxCategoryID))
				{
					sender.SetDefaultExt<CASplit.taxCategoryID>(split);
				}

				newVal = TaxAttribute.CalcResidualAmt(sender, split, adj.TaxZoneID, split.TaxCategoryID, adj.TranDate.Value,
					adj.TaxCalcMode, adj.CuryControlAmt.Value, adj.CurySplitTotal.Value, adj.CuryTaxTotal.Value);
				e.NewValue = Math.Sign(newVal.Value) == Math.Sign(adj.CuryControlAmt.Value) ? newVal : 0;
				e.Cancel = true;
			}
		}

		protected virtual void CASplit_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;

			if (reversingContext || split == null)
				return;

			if (TaxAttribute.GetTaxCalc<CASplit.taxCategoryID>(sender, split) != TaxCalc.Calc ||
				split.InventoryID != null)
				return;

			Account account = null;
			if (split.AccountID != null)
			{
				account = PXSelect<Account,
								Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, split.AccountID);
			}

			if (account?.TaxCategoryID != null)
			{
				e.NewValue = account.TaxCategoryID;
			}
			else if (taxzone.Current != null &&
				!string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID))
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
			}
			else
			{
				e.NewValue = split.TaxCategoryID;
			}
		}

		protected virtual void CASplit_TaxCategoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CASplit row = e.Row as CASplit;

			if (reversingContext || row == null)
                return;

			CAAdj doc = CAAdjRecords.Current;

			if (!this.IsCopyPasteContext && casetup.Current.RequireControlTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>()
				&& row.CuryTranAmt.HasValue && row.CuryTranAmt.Value != 0 && row.Qty == (decimal)1.0 && doc.TaxCalcMode == TaxCalculationMode.Net)
			{
				PXResultset<CATax> taxes = PXSelect<CATax,
                                              Where<CATax.adjTranType, Equal<Required<CATax.adjTranType>>,
                                                And<CATax.adjRefNbr, Equal<Required<CATax.adjRefNbr>>,
					                            And<CATax.lineNbr, Equal<Required<CATax.lineNbr>>>>>>
                                          .Select(this, row.AdjTranType, row.AdjRefNbr, row.LineNbr);
				decimal curyTaxSum = 0;

				foreach (CATax tax in taxes)
				{
					curyTaxSum += tax.CuryTaxAmt.Value;
				}

				decimal? taxableAmount = TaxAttribute.CalcTaxableFromTotalAmount(sender, row, doc.TaxZoneID,
                                                                                 row.TaxCategoryID, doc.TranDate.Value,
                                                                                 aCuryTotal: row.CuryTranAmt.Value + curyTaxSum,
                                                                                 aSalesOrPurchaseSwitch: false,
                                                                                 enforceType: GLTaxAttribute.TaxCalcLevelEnforcing.EnforceCalcOnItemAmount);
				sender.SetValueExt<CASplit.curyTranAmt>(row, taxableAmount);
			}
		}

		protected virtual void CASplit_InventoryId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

			if (split != null && split.InventoryID != null)
			{
				InventoryItem invItem = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<CASplit.inventoryID>>>>.
					Select(this, split.InventoryID);

				bool isEnableForImport = split.AccountID == null;
				bool isUIContext = !(IsImportFromExcel == true || IsContractBasedAPI == true);

				if (invItem != null && adj != null && (isEnableForImport || isUIContext))
				{
					if (adj.DrCr == CADrCr.CADebit)
					{
						split.AccountID = invItem.SalesAcctID;
						split.SubID = invItem.SalesSubID;
					}
					else
					{
						split.AccountID = invItem.COGSAcctID;
						split.SubID = invItem.COGSSubID;
					}
				}
			}

			sender.SetDefaultExt<CASplit.taxCategoryID>(split);
			sender.SetDefaultExt<CASplit.uOM>(split);
		}

		protected virtual void CASplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CASplit row = (CASplit)e.Row;

			if (row == null)
				return;

		    bool isReclassification = this.CAAdjRecords.Current.PaymentsReclassification == true;

            PXUIFieldAttribute.SetEnabled<CASplit.accountID>(sender, row, !isReclassification);
            PXUIFieldAttribute.SetEnabled<CASplit.subID>(sender, row, !isReclassification);
            PXUIFieldAttribute.SetEnabled<CASplit.branchID>(sender, row, !isReclassification);
		}
        
		protected virtual void CASplit_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e) => CATranDetailHelper.OnCATranDetailRowUpdatingEvent(sender, e);

		

		protected virtual void CASplit_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			updateAmountPrice(oldSplit: new CASplit(), newSplit: e.Row as CASplit);
		}

		protected virtual void CASplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CASplit tran = e.Row as CASplit;
			object projectID = tran.ProjectID;
			try
			{
				sender.RaiseFieldVerifying<CASplit.projectID>(tran, ref projectID);
			}
			catch (PXSetPropertyException exc)
			{
				sender.RaiseExceptionHandling<CASplit.projectID>(tran, projectID, exc);
			}
		}

		protected virtual void CASplit_Qty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			e.NewValue = (decimal)1.0;
		}
	
		#endregion

		#region CATaxTran Events
		protected virtual void CATaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			bool usesManualVAT = this.CurrentDocument.Current != null && this.CurrentDocument.Current.UsesManualVAT == true;
			PXUIFieldAttribute.SetEnabled<CATaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted && !usesManualVAT);
		}
		protected virtual void CATaxTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (reversingContext) e.Cancel = true;
		}
		protected virtual void CATaxTran_TaxType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && CAAdjRecords.Current != null)
			{
				if (CAAdjRecords.Current.DrCr == CADrCr.CACredit)
				{
					AP.PurchaseTax tax = PXSelect<AP.PurchaseTax, Where<AP.PurchaseTax.taxID, Equal<Required<AP.PurchaseTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.TranTaxType;
						e.Cancel = true;
					}
				}
				else
				{
					AR.SalesTax tax = PXSelect<AR.SalesTax, Where<AR.SalesTax.taxID, Equal<Required<AR.SalesTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.TranTaxType;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CATaxTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && CAAdjRecords.Current != null)
			{
				if (CAAdjRecords.Current.DrCr == CADrCr.CACredit)
				{
					AP.PurchaseTax tax = PXSelect<AP.PurchaseTax, Where<AP.PurchaseTax.taxID, Equal<Required<AP.PurchaseTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxAcctID;
						e.Cancel = true;
					}
				}
				else
				{
					AR.SalesTax tax = PXSelect<AR.SalesTax, Where<AR.SalesTax.taxID, Equal<Required<AR.SalesTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxAcctID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CATaxTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && CAAdjRecords.Current != null)
			{
				if (CAAdjRecords.Current.DrCr == CADrCr.CACredit)
				{
					AP.PurchaseTax tax = PXSelect<AP.PurchaseTax, Where<AP.PurchaseTax.taxID, Equal<Required<AP.PurchaseTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxSubID;
						e.Cancel = true;
					}
				}
				else
				{
					AR.SalesTax tax = PXSelect<AR.SalesTax, Where<AR.SalesTax.taxID, Equal<Required<AR.SalesTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxSubID;
						e.Cancel = true;
					}
				}
			}
		}
		#endregion

		#region CurrencyInfo Events

		protected virtual void CurrencyInfo_ModuleCode_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BatchModule.GL;
			e.Cancel = true;
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CurrencyInfo currencyInfo = (CurrencyInfo)e.Row;
			if (currencyInfo == null || CAAdjRecords.Current == null || CAAdjRecords.Current.TranDate == null) return;
			e.NewValue = CAAdjRecords.Current.TranDate;
			e.Cancel = true;
		}

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashAccount.Current != null && !string.IsNullOrEmpty(cashAccount.Current.CuryID))
				{
					e.NewValue = cashAccount.Current.CuryID;
					e.Cancel = true;
				}				
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashAccount.Current != null && !string.IsNullOrEmpty(cashAccount.Current.CuryRateTypeID))
				{
					e.NewValue = cashAccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}
				else if (CMSetup.Current != null && !string.IsNullOrEmpty(CMSetup.Current.CARateTypeDflt))
				{
					e.NewValue = CMSetup.Current.CARateTypeDflt;
					e.Cancel = true;
				}
			}
		}
		/* May be need later
		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
						bool curyenabled = !(info.IsReadOnly == true || (info.CuryID == info.BaseCuryID));

						if (cashAccount.Current != null && !(bool)cashAccount.Current.AllowOverrideRate)
						{
								curyenabled = false;
						}

						PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID> (sender, info, curyenabled);
						PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>    (sender, info, curyenabled);
						PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate> (sender, info, curyenabled);
						PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
				}
		}
		*/

		#endregion

		#region External Tax Provider

		public virtual bool IsExternalTax(string taxZoneID)
				{
					return false;
				}

		public virtual CAAdj CalculateExternalTax(CAAdj invoice)
			{
			return invoice;
		}
		#endregion

	}
}
