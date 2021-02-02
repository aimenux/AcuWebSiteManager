using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.PR.Standalone;

namespace PX.Objects.CA
{
    [Serializable]
    public class CashAccountMaint : PXGraph<CashAccountMaint, CashAccount>
	{
        #region Internal Types

        [Serializable]
        [PXProjection(typeof(Select<PaymentMethodAccount>), Persistent = false)]
        public partial class PaymentMethodAccount2 : IBqlTable
        {
            #region PaymentMethodID
            public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

            [PXDBString(10, IsUnicode = true, IsKey = true, BqlField = typeof(PaymentMethodAccount.paymentMethodID))]
            [PXDefault(typeof(PaymentMethod.paymentMethodID))]
            [PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
            [PXUIField(DisplayName = "Payment Method")]
            public virtual string PaymentMethodID
                {
				get;
				set;
            }
            #endregion

            #region CashAccountID
            public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

            [PXDBInt(BqlField = typeof(PaymentMethodAccount.cashAccountID), IsKey=true)]
            [PXDefault]
            public virtual int? CashAccountID
                {
				get;
				set;
            }
            #endregion
        }

        [Flags]
        enum CashAccountOptions
        {
            None = 0x0,
            HasPTSettings = 0x1,
            HasPTInstances = 0x2
        }
        #endregion

        #region Ctor + Public Selects
        public CashAccountMaint()
        {
            GLSetup setup = GLSetup.Current;
            CASetup setup1 = this.casetup.Current;

			FieldDefaulting.AddHandler<CR.BAccountR.type>(SetDefaultBaccountType);
			action.AddMenuAction(ChangeID);
		}

		/// <summary>
		/// Sets default baccount type. Method is used as a workaround for the redirection problem with the edit button of the empty Bank ID field.
		/// </summary>
		private void SetDefaultBaccountType(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
				e.NewValue = CR.BAccountType.VendorType;
        }

        [PXDBDefault(typeof(CashAccount.cashAccountID))]
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<PaymentMethodAccount.cashAccountID>>>>))]
        protected virtual void PaymentMethodAccount_CashAccountID_CacheAttached(PXCache sender)
        {
        }

		#region Action Buttons
		public PXAction<CashAccount> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXChangeID<CashAccount, CashAccount.cashAccountCD> ChangeID;
		#endregion
		public PXSelectJoin<CashAccount,
			LeftJoin<Account, On<Account.accountID, Equal<CashAccount.accountID>>,
			LeftJoin<Sub, On<Sub.subID, Equal<CashAccount.subID>>>>,
			Where2<Match<Current<AccessInfo.userName>>,
				And2<Where2<Match<Account, Current<AccessInfo.userName>>,Or<Account.accountID, IsNull>>,
				And<Where2<Match<Sub, Current<AccessInfo.userName>>, Or<Sub.subID, IsNull>>>>>> CashAccount;
        public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>> CurrentCashAccount;
		public PXSelect<Account, Where<Account.accountID, Equal<Optional<CashAccount.accountID>>, And<Match<Current<AccessInfo.userName>>>>> Account_AccountID;
		public PXSelect<CashAccountCheck, Where<CashAccountCheck.accountID, Equal<Optional<CashAccount.cashAccountID>>>> CashAccountChecks;
        public PXSelectJoin<PaymentMethodAccount, InnerJoin<PaymentMethod,
                On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>>>,
                Where<PaymentMethodAccount.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>
            > Details;
		[PXImport(typeof(CashAccount))]
        public PXSelectJoin<CashAccountETDetail,
                    InnerJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<CashAccountETDetail.entryTypeID>>>,
                        Where<CashAccountETDetail.accountID, Equal<Current<CashAccount.cashAccountID>>>> ETDetails;
        public PXSelect<CAEntryType> EntryTypes;
        [PXCopyPasteHiddenFields(new Type[] {typeof(CashAccountDeposit.accountID) })]
        public PXSelect<CashAccountDeposit, Where<CashAccountDeposit.accountID, Equal<Current<CashAccount.cashAccountID>>>> Deposits;

        public PXSelect<PaymentMethodAccount2, Where<PaymentMethodAccount2.cashAccountID, Equal<Current2<CashAccount.cashAccountID>>>> PaymentMethodForRemittance;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<CashAccountPaymentMethodDetail,
						       InnerJoin<
										 PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CashAccountPaymentMethodDetail.paymentMethodID>,
                               And<PaymentMethodDetail.detailID, Equal<CashAccountPaymentMethodDetail.detailID>,
									 And<
										 Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                               Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>,

                               Where<CashAccountPaymentMethodDetail.accountID, Equal<Optional2<CashAccount.cashAccountID>>,
                               And<CashAccountPaymentMethodDetail.paymentMethodID, Equal<Optional2<PaymentMethodAccount2.paymentMethodID>>>>,
                                     OrderBy<Asc<PaymentMethodDetail.orderIndex>>> PaymentDetails;

        public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Optional2<PaymentMethodAccount.paymentMethodID>>,
                                And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                    Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>> PaymentTypeDetails;

        public PXSelect<VendorClass, Where<VendorClass.cashAcctID, Equal<Required<CashAccount.cashAccountID>>>> VendorClasses;
        public PXSelect<CR.Location, Where<CR.Location.vCashAccountID, Equal<Required<CashAccount.cashAccountID>>>> Locations;

        public PXSetup<Company> Company;
        public PXSetup<GLSetup> GLSetup;
        public PXSetup<CASetup> casetup;
        #endregion

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected new IEnumerable Cancel(PXAdapter a)
		{
			foreach (PXResult<CashAccount> res in (new PXCancel<CashAccount>(this, "Cancel")).Press(a))
			{
				CashAccount acct = res;

				if (acct != null)
				{
					if (CashAccount.Cache.GetStatus(acct) == PXEntryStatus.Inserted)
					{
						using (new PXReadBranchRestrictedScope())
						{
							CashAccount e1 = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>,
											And<CashAccount.cashAccountID, NotEqual<Required<CashAccount.cashAccountID>>>>>.Select(this, acct.CashAccountCD, acct.CashAccountID);
							if (e1 != null)
							{
								CashAccount.Cache.RaiseExceptionHandling<CashAccount.cashAccountCD>(acct, acct.CashAccountCD, new PXSetPropertyException(CA.Messages.CashAccountExists));
							}
						}
					}
				}

				yield return acct;
			}
		}

        public IEnumerable paymentMethodForRemittance()
        {
            PXCache cache = this.Caches[typeof(PaymentMethodAccount2)];
            cache.AllowDelete = true;
            cache.AllowInsert = true;
            cache.Clear();

			PXResultset<PaymentMethodAccount> payMethodAccounts = GetCurrentUsedPaymentMethodAccounts();
			foreach (PaymentMethodAccount payMethodAcc in payMethodAccounts)
            {
				PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, payMethodAcc.PaymentMethodID);

				if (pm == null || pm.UseForCA != true)
					continue;

                PaymentMethodDetail pmDetail = PXSelectReadonly<PaymentMethodDetail,
                                                    Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
															And<
																Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
																   Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>.
											   Select(this, payMethodAcc.PaymentMethodID);

				if (string.IsNullOrEmpty(pmDetail?.PaymentMethodID))
					continue;

				PaymentMethodAccount2 row2 = new PaymentMethodAccount2
				{
					PaymentMethodID = payMethodAcc.PaymentMethodID,
					CashAccountID = payMethodAcc.CashAccountID
				};

                cache.Insert(row2);
                string message;

                if (remittancePMErrors.TryGetValue(row2.PaymentMethodID, out message))
                {
                    cache.RaiseExceptionHandling<PaymentMethodAccount2.paymentMethodID>(row2, row2.PaymentMethodID,new PXSetPropertyException(message));
                    remittancePMErrors.Remove(row2.PaymentMethodID);
                }

                yield return row2;
            }

            cache.AllowDelete = false;
            cache.AllowInsert = false;
            cache.AllowUpdate = false;

            Caches[typeof(PaymentMethodAccount2)].IsDirty = false;
        }

        #region Internal Flags
        private bool isPaymentMergedFlag = false;
        #endregion

        #region CashAccount Events
        protected virtual void CashAccount_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            CashAccount cashacct = e.Row as CashAccount;

            if (cashacct != null)
            {
                if (CheckCashAccountForTransactions(cashacct))
                {
                    throw new PXException(Messages.ERR_CashAccountHasTransactions_DeleteForbidden);
                }
				CheckCashAccountInUse(sender, cashacct);
                DeleteCashAccountInUse(sender, cashacct);
            }
        }

		protected virtual void CashAccount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			CashAccount cashAccount = e.Row as CashAccount;

			if (cashAccount == null)
				return;

			foreach (CashAccountCheck check in CashAccountChecks.Select(cashAccount.CashAccountID))
			{
				CashAccountChecks.Delete(check);
			}

			using (new PXReadBranchRestrictedScope())
			{
				CashAccount otherCashAccount = PXSelect<CashAccount, Where<CA.CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
											  And<CA.CashAccount.cashAccountID, NotEqual<Required<CA.CashAccount.cashAccountID>>>>>.Select(this, cashAccount.AccountID, cashAccount.CashAccountID);

				Account acc = Account_AccountID.SelectSingle(cashAccount.AccountID);

				if (acc != null && acc.IsCashAccount == true && otherCashAccount == null)
				{
					acc.IsCashAccount = false;
					Account_AccountID.Update(acc);
				}
			}
		}

        protected virtual void CashAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled<CashAccount.curyID>(Caches[typeof(CashAccount)], null, false);
			PXUIFieldAttribute.SetVisible<CashAccount.curyRateTypeID>(sender, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
            CashAccount row = (CashAccount)e.Row;

            if (row == null)
                return;


            this.RecalcOptions(row);
            bool usedAsClearing = false;
            bool isClearing = (bool)row.ClearingAccount;

            if (isClearing)
            {
                CashAccountDeposit reference = PXSelect<CashAccountDeposit, Where<CashAccountDeposit.depositAcctID, Equal<Required<CashAccountDeposit.depositAcctID>>>>.Select(this, row.CashAccountID);
                usedAsClearing = (reference != null);
            }

            PXUIFieldAttribute.SetEnabled<CashAccount.clearingAccount>(sender, row, (!usedAsClearing) || (!isClearing));

            bool stmtImportToSingleAccount = this.casetup.Current.ImportToSingleAccount ?? false;

            PXUIFieldAttribute.SetVisible<CashAccount.statementImportTypeName>(sender, row, stmtImportToSingleAccount);

            this.Deposits.Cache.AllowInsert = !(bool)row.ClearingAccount;
            this.Deposits.Cache.AllowUpdate = !(bool)row.ClearingAccount;
	        bool hasCATransactions = CheckCashAccountForTransactions(row);

			PXUIFieldAttribute.SetEnabled<CashAccount.accountID>(sender, row, !hasCATransactions);
			PXUIFieldAttribute.SetEnabled<CashAccount.subID>(sender, row, !hasCATransactions);
			PXUIFieldAttribute.SetEnabled<CashAccount.branchID>(sender, row, !hasCATransactions);
        }

        protected virtual void CashAccount_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CashAccount cashacc = (CashAccount)e.Row;
            if (e.NewValue != null)
            {
                Account acct = Account_AccountID.SelectSingle((int)e.NewValue);
                if (String.IsNullOrEmpty(acct.CuryID))
                {
					if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
                    {
						e.NewValue = acct.AccountCD;
						throw new PXSetPropertyException(Messages.CashAccount_MayBeCreatedFromDenominatedAccountOnly, PXErrorLevel.Error);
                    }
                    else
                    {
                        cashacc.CuryID = Company.Current.BaseCuryID;
                        cashacc.BranchID = this.Accessinfo.BranchID;
                        acct = PXCache<Account>.CreateCopy(acct);
                        acct.CuryID = Company.Current.BaseCuryID;
                        Account_AccountID.Update(acct);
                    }
                }
                else
                {
                    cashacc.CuryID = acct.CuryID;
                    cashacc.BranchID = this.Accessinfo.BranchID;
                }
                cashacc.Descr = acct.Description;
				bool locenabled = PXDBLocalizableStringAttribute.IsEnabled;
				if (locenabled)
				{
					string[] dtranslations = Account_AccountID.Cache.GetValueExt(acct, "DescriptionTranslations") as string[];
					if (dtranslations != null)
					{
						sender.SetValueExt(cashacc, "DescrTranslations", dtranslations);
					}
				}
			}
		}

        protected virtual void CashAccount_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            CashAccount cashacct = e.Row as CashAccount;
            if (cashacct.ReconNumberingID == null && cashacct.Reconcile == true)
            {
                sender.RaiseExceptionHandling<CashAccount.reconNumberingID>(cashacct, cashacct.ReconNumberingID, new PXSetPropertyException(Messages.RequiresReconNumbering));
            }
         }

        protected virtual void CashAccount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            CashAccount cashacct = e.Row as CashAccount;
            if (cashacct == null) return;

			if (CashAccount.Cache.GetStatus(cashacct) == PXEntryStatus.Inserted)
			{
				using (new PXReadBranchRestrictedScope())
				{
					CashAccount e1 = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>,
									And<CashAccount.cashAccountID, NotEqual<Required<CashAccount.cashAccountID>>>>>.Select(this, cashacct.CashAccountCD, cashacct.CashAccountID);
					if (e1 != null)
					{
						CashAccount.Cache.RaiseExceptionHandling<CashAccount.cashAccountCD>(cashacct, cashacct.CashAccountCD, new PXSetPropertyException(Messages.CashAccountExists));
						throw new PXSetPropertyException(Messages.CashAccountExists);
					}
				}
			}


			if (cashacct.ReconNumberingID == null && cashacct.Reconcile == true)
            {
                sender.RaiseExceptionHandling<CashAccount.reconNumberingID>(cashacct, cashacct.ReconNumberingID, new PXSetPropertyException(Messages.RequiresReconNumbering));
            }
            CashAccount otherCashAcc = PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Current<CashAccount.accountID>>,
                And<CashAccount.subID, Equal<Current<CashAccount.subID>>, And<CashAccount.branchID, Equal<Current<CashAccount.branchID>>,
                And<CashAccount.cashAccountID, NotEqual<Current<CashAccount.cashAccountID>>>>>>>.Select(this);
            if (otherCashAcc != null)
            {
                throw new PXException(Messages.CashAccountExist);
            }
            if (cashacct.CashAccountID != null && cashacct.CashAccountID != -1)
            {
                PXEntryStatus status = this.CashAccount.Cache.GetStatus(e.Row);
                if (status != PXEntryStatus.InsertedDeleted && status != PXEntryStatus.Deleted)
                {
                    bool isInserted = (status == PXEntryStatus.Inserted);
                    if (isInserted)
                    {
                        if (HasGLTrans(cashacct.AccountID, cashacct.SubID, cashacct.BranchID))
                        {
                            sender.RaiseExceptionHandling<CashAccount.cashAccountCD>(e.Row, cashacct.CashAccountCD, new PXSetPropertyException(Messages.GLTranExistForThisCashAcct, PXErrorLevel.Warning));
                        }
                    }
                }
            }
			if (casetup.Current != null && casetup.Current.TransitAcctId == cashacct.AccountID)
			{
				PXUIFieldAttribute.SetError<CashAccount.accountID>(sender, cashacct, Messages.CashAccountCanNotBeTransit);
				throw new PXSetPropertyException<CashAccount.accountID>(Messages.CashAccountCanNotBeTransit);
			}

			if (cashacct.UseForCorpCard == true)
			{
				if (cashacct.ClearingAccount == true)
				{
					throw new PXRowPersistingException(typeof(CashAccount.clearingAccount).Name, cashacct.ClearingAccount, Messages.ClearingAccountNotAllowed);
				}

				var paymentMethods = this.Details.Select();
				if (paymentMethods.Count != 1)
				{
					throw new PXRowPersistingException(sender.GetItemType().Name, cashacct, Messages.CorpCardCashAccountToBeLinkedToOnePaymentMethod);
				}
				var paymentMethodAccount = (PaymentMethodAccount)paymentMethods
					?? throw new PXException(Messages.PaymentMethodAccountCannotBeFound);

				var paymentMethod = (PaymentMethod)PXSelect<PaymentMethod,
					Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>
					.Select(this, paymentMethodAccount.PaymentMethodID)
					?? throw new PXException(Messages.PaymentMethodCannotBeFound, paymentMethodAccount.PaymentMethodID);

				if (paymentMethod.APRequirePaymentRef == true
					|| paymentMethod.APAdditionalProcessing != PaymentMethod.aPAdditionalProcessing.NotRequired)
				{
					throw new PXRowPersistingException(typeof(CashAccount.clearingAccount).Name, cashacct.ClearingAccount, Messages.PaymentAndAdditionalProcessingSettingsHaveWrongValues);
				}
			}

			#region Validate CashPaymentTypeDetails for Required fields
			/*Dictionary<string, string> failedPTypes = new Dictionary<string, string>();
            foreach( CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Inserted)
            {
                CashAccountPaymentMethodDetail detail = iDet;
                if (!ValidateDetail(iDet))
                {
                    failedPTypes[detail.PaymentMethodID] = detail.PaymentMethodID;
                    e.Cancel = true;
                }
            }
            foreach (CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Updated)
            {
                CashAccountPaymentMethodDetail detail = iDet;
                if (!ValidateDetail(iDet))
                {
                    failedPTypes[detail.PaymentMethodID] = detail.PaymentMethodID;
                    e.Cancel = true;
                }
            }
            if (failedPTypes.Count > 0)
            {
                if (!failedPTypes.ContainsKey(cashacct.PaymentMethodID))
                {
                    foreach (string key in failedPTypes.Keys)
                    {
                        cashacct.PaymentMethodID = key;
                        break;
                    }
                }
            }	*/
			#endregion
		}

		protected virtual void CashAccount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CashAccount updatedCashAccount = e.Row as CashAccount;
			CashAccount oldCashAccount = e.OldRow as CashAccount;

			if (!sender.ObjectsEqual<CashAccount.accountID>(updatedCashAccount, oldCashAccount))
			{
				CashAccount[] accountsToProcess = { updatedCashAccount, oldCashAccount };

				foreach (CashAccount cashAccount in accountsToProcess)
				{
					if (cashAccount.AccountID == null)
						continue;

					Account generalLedgerAccount = Account_AccountID.SelectSingle(cashAccount.AccountID);
					using (new PXReadBranchRestrictedScope())
					{
						bool haveCashAccounts =
						PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>>>.Select(this, cashAccount.AccountID).Count > 0;

						if (generalLedgerAccount != null && generalLedgerAccount.IsCashAccount != haveCashAccounts)
						{
							generalLedgerAccount = PXCache<Account>.CreateCopy(generalLedgerAccount);
							generalLedgerAccount.IsCashAccount = haveCashAccounts;

							// For GL accounts that are cash accounts, the PostOption should always be 'Detail' to
							// avoid user confusion, as Cash Management module ignores the Post Option set in GL and
							// (in essence) always posts in Detail mode.
							// -
							if (haveCashAccounts && generalLedgerAccount.PostOption != GL.AccountPostOption.Detail)
							{
								generalLedgerAccount.PostOption = GL.AccountPostOption.Detail;
							}

							Account_AccountID.Update(generalLedgerAccount);
						}
					}
				}
			}
        }

		protected virtual void _(Events.RowInserted<CashAccount> e)
		{
			if (IsImport)
			{
				Account generalLedgerAccount = Account_AccountID.SelectSingle(e.Row.AccountID);
				if (generalLedgerAccount != null)
				{
					generalLedgerAccount = PXCache<Account>.CreateCopy(generalLedgerAccount);
					generalLedgerAccount.IsCashAccount = true;
					generalLedgerAccount.PostOption = GL.AccountPostOption.Detail;

					Account_AccountID.Update(generalLedgerAccount);
				}
			}
		}

        protected virtual void CashAccount_ClearingAccount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CashAccount row = (CashAccount)e.Row;
            if (row.ClearingAccount == true)
            {
                foreach (CashAccountDeposit it in this.Deposits.Select())
                {
                    this.Deposits.Delete(it);
                }
            }
        }

        protected virtual void CashAccount_Reconcile_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CashAccount row = (CashAccount)e.Row;
            if (row.Reconcile == false)
            {
                row.ReconNumberingID = null;
            }
        }

        protected virtual void CashAccount_ClearingAccount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CashAccount row = (CashAccount)e.Row;

            if ((bool)e.NewValue == true)
            {
                bool hasDeposits = this.Deposits.Any();

                if (hasDeposits )
                {
                    e.NewValue = false;
                    throw new PXSetPropertyException(Messages.CashAccountMayNotBeMadeClearingAccount, PXErrorLevel.Error);
                }
            }
        }
        #endregion

        #region Detail Events

        protected virtual void PaymentMethodAccount_UseForAP_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row != null)
            {
                CashAccount cx = CurrentCashAccount.Select();
                if (cx.ClearingAccount.GetValueOrDefault(false))
                {
					e.NewValue= false;
                }
                else if (String.IsNullOrEmpty(row.PaymentMethodID) == false)
                {
                    PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
					e.NewValue = (pm != null && pm.UseForAP == true);
                    e.Cancel = true;
                }
            }
        }

        protected virtual void PaymentMethodAccount_UseForAR_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			if (!String.IsNullOrEmpty(row?.PaymentMethodID))
            {
                PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
				e.NewValue = (pm != null && pm.UseForAR == true);
                e.Cancel = true;
            }
        }

		protected virtual void PaymentMethodAccount_UseForAP_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			if (!String.IsNullOrEmpty(row?.PaymentMethodID))
			{
				PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
				if (pm?.UseForAP != true && (bool)e.NewValue == true)
				{
					e.NewValue = false;
					throw new PXSetPropertyException(Messages.PaymentMethodCannotBeUsedInAP, row.PaymentMethodID);
				}
			}
		}

		protected virtual void PaymentMethodAccount_UseForAR_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			if (!String.IsNullOrEmpty(row?.PaymentMethodID))
			{
				PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
				if (pm?.UseForAR != true && (bool)e.NewValue == true)
				{
					e.NewValue = false;
					throw new PXSetPropertyException(Messages.PaymentMethodCannotBeUsedInAR, row.PaymentMethodID);
				}
			}
		}

        protected virtual void PaymentMethodAccount_BatchLastRefNbr_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row != null && String.IsNullOrEmpty(row.PaymentMethodID) == false)
            {
                PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
                if (pt.APCreateBatchPayment == true)
                {
                    e.NewValue = "00000000";
                    e.Cancel = true;
                }
                else
                {
                    e.NewValue = null;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void PaymentMethodAccount_PaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;

			if (!String.IsNullOrEmpty(row?.PaymentMethodID))
            {
                cache.SetDefaultExt<PaymentMethodAccount.aPBatchLastRefNbr>(e.Row);
				cache.SetDefaultExt<PaymentMethodAccount.useForAR>(e.Row);
				cache.SetDefaultExt<PaymentMethodAccount.useForAP>(e.Row);
            }
        }

        protected virtual void PaymentMethodAccount_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            CashAccount acct = this.CashAccount.Current;
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (acct != null)
            {
                this.CashAccount.Cache.MarkUpdated(acct);
            }
            PaymentDetails.View.RequestRefresh();
        }

        protected virtual void PaymentMethodAccount_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            PaymentMethodAccount oldRow = (PaymentMethodAccount)e.Row;
            PaymentMethodAccount newRow = (PaymentMethodAccount)e.NewRow;

            if (newRow == null)
				return;

            if (oldRow.PaymentMethodID != newRow.PaymentMethodID)
            {
                foreach (PaymentMethodAccount it in Details.Select())
                {
                    if (!object.ReferenceEquals(oldRow, it) && !object.ReferenceEquals(newRow, it) && it.PaymentMethodID == newRow.PaymentMethodID)
                    {
						throw new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.DuplicatedPaymentMethodForCashAccount, newRow.PaymentMethodID));
					}
                }
            }
        }

        protected virtual void PaymentMethodAccount_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
            if (row == null) return;

            if (row.PaymentMethodID != null)
            {
                foreach (PaymentMethodAccount it in Details.Select())
                {
                    if (!object.ReferenceEquals(row, it) && it.PaymentMethodID == row.PaymentMethodID)
                    {
                        throw new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.DuplicatedPaymentMethodForCashAccount, row.PaymentMethodID));
                    }
                }
            }

            if (row.APIsDefault == true && String.IsNullOrEmpty(row.PaymentMethodID))
            {
                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PaymentMethodAccount.paymentMethodID>(cache));
            }
        }

        protected virtual void PaymentMethodAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            CashAccount acct = this.CashAccount.Current;
            PaymentMethodAccount row = (PaymentMethodAccount)e.Row;

            if (row != null && string.IsNullOrEmpty(row.PaymentMethodID) == false)
            {
                PaymentMethod pt = PXSelect<PaymentMethod,
									  Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.
								   Select(this, row.PaymentMethodID);

                PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPBatchLastRefNbr>(cache, row, isEnabled: pt?.APCreateBatchPayment == true);
            }

            //PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAP>(cache, row, !acct.ClearingAccount.GetValueOrDefault(false));
            //***PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAR>(cache, row, false);
			bool EnableAP = row?.UseForAP == true;
            bool EnableAR = row?.UseForAR == true;

            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPIsDefault>(cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPAutoNextNbr>(cache, e.Row, EnableAP);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPLastRefNbr>(cache, e.Row, EnableAP);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefault>(cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefaultForRefund>(cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRAutoNextNbr>(cache, e.Row, EnableAR);
            PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRLastRefNbr>(cache, e.Row, EnableAR);

            if (row != null)
            {
                PXEntryStatus status = cache.GetStatus(e.Row);

                if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
                {
                    isPaymentMergedFlag = false;
                    this.FillPaymentDetails(row.PaymentMethodID);
                }
            }
        }

		protected virtual void PaymentMethodAccount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;

			if (row.APAutoNextNbr == true && row.APLastRefNbr == null)
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aPAutoNextNbr>(row, row.APAutoNextNbr, new PXSetPropertyException(Messages.SpecifyLastRefNbr, GL.Messages.ModuleAP));
			}

			if (row.ARAutoNextNbr == true && row.ARLastRefNbr == null)
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aRAutoNextNbr>(row, row.ARAutoNextNbr, new PXSetPropertyException(Messages.SpecifyLastRefNbr, GL.Messages.ModuleAR));
			}
		}

        protected virtual void PaymentMethodAccount2_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

        #region ETDetail Events
        protected virtual void CashAccountETDetail_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            CAEntryType caEntryType = PXSelect<CAEntryType,
                Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.
                SelectWindowed(this, 0, 1, ((CashAccountETDetail)e.Row).EntryTypeID);
            if (caEntryType == null)
            {
                cache.RaiseExceptionHandling<CashAccountETDetail.entryTypeID>(e.Row, ((CashAccountETDetail)e.Row).EntryTypeID, new PXException(Messages.EntryTypeIDDoesNotExist));
                e.Cancel = true;
            }
        }

		protected virtual void CashAccountETDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CashAccountETDetail row = e.NewRow as CashAccountETDetail;
			if (row.OffsetAccountID != null)
			{
				Account currentAcc = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.OffsetAccountID);
				if (currentAcc.IsCashAccount == true)
				{
					CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
						And<CashAccount.subID, Equal<Required<CashAccount.subID>>,
							And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>>>>>.Select(this, row.OffsetAccountID, row.OffsetSubID, row.OffsetBranchID);
					if (cashAccount == null)
					{
						string branchCD = (string)PXSelectorAttribute.GetField(sender, row, typeof(CashAccountETDetail.offsetBranchID).Name, row.OffsetBranchID, typeof(Branch.branchCD).Name);
						sender.RaiseExceptionHandling<CashAccountETDetail.offsetBranchID>(row, branchCD,
							new PXSetPropertyException(Messages.NoCashAccountForBranchAndSub, PXErrorLevel.Error));
						string subCD = (string)PXSelectorAttribute.GetField(sender, row, typeof(CashAccountETDetail.offsetSubID).Name, row.OffsetSubID, typeof(Sub.subCD).Name);
						sender.RaiseExceptionHandling<CashAccountETDetail.offsetSubID>(row, subCD,
							new PXSetPropertyException(Messages.NoCashAccountForBranchAndSub, PXErrorLevel.Error));
						e.Cancel = true;
						return;
					}
				}
			}
		}

        protected virtual void CashAccountETDetail_EntryTypeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            cache.SetDefaultExt<CashAccountETDetail.offsetAccountID>(e.Row);
            cache.SetDefaultExt<CashAccountETDetail.offsetSubID>(e.Row);
        }
        protected virtual void CashAccountETDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            IsCorrectCashAccount(sender, e.Row);
        }
        protected virtual void CashAccountETDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            IsCorrectCashAccount(sender, e.Row);
        }
        private bool IsCorrectCashAccount(PXCache sender, object Row)
        {
            CashAccountETDetail row = (CashAccountETDetail)Row;
             if (row != null)
             {
                 CAEntryType et = (CAEntryType)PXSelectorAttribute.Select<CashAccountETDetail.entryTypeID>(sender, Row);
                 if (et != null)
                 {
                     if (!row.OffsetCashAccountID.HasValue && et.UseToReclassifyPayments == true && !row.OffsetAccountID.HasValue)
                     {
                         CashAccount cashAcct = null;
                         cashAcct = PXSelectReadonly<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>, And<CashAccount.subID, Equal<Required<CashAccount.subID>>, And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>>>>>.Select(this, et.AccountID, et.SubID, et.BranchID);
                         CashAccount current = this.CashAccount.Current;
                         if (cashAcct != null)
                         {
                             if (current.CashAccountID == cashAcct.CashAccountID)
                             {
                                 sender.RaiseExceptionHandling<CashAccountETDetail.offsetCashAccountID>(row, null, new PXSetPropertyException(Messages.SetOffsetAccountDifferFromCurrentAccount, PXErrorLevel.Error));
                                 return false;
                             }
                             if (cashAcct.CuryID != current.CuryID)
                             {
                                 sender.RaiseExceptionHandling<CashAccountETDetail.offsetCashAccountID>(row, null, new PXSetPropertyException(Messages.SetOffsetAccountInSameCurrency, PXErrorLevel.Error));
                                 return false;
                             }
                         }
                     }
                 }
             }
             return true;
        }
        protected virtual void CashAccountETDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row != null)
            {
                bool usesCashAccount = false;
                CAEntryType et = (CAEntryType)PXSelectorAttribute.Select<CashAccountETDetail.entryTypeID>(sender, e.Row);
                if (et != null)
                {
                    if (row.OffsetAccountID.HasValue && row.OffsetSubID.HasValue)
                    {
                        CashAccount cashAcct = null;
                        Account acct = null;
                        cashAcct = PXSelectReadonly<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
                            And<CashAccount.subID, Equal<Required<CashAccount.subID>>>>>.Select(this, row.OffsetAccountID, row.OffsetSubID);
                        acct = PXSelectReadonly<GL.Account, Where<GL.Account.accountID, Equal<Required<GL.Account.accountID>>>>.Select(this, row.OffsetAccountID);
                        usesCashAccount = (cashAcct != null);
                        if (et.UseToReclassifyPayments == true)
                        {
                            if (!usesCashAccount)
                            {
                                sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount, PXErrorLevel.Error));
                            }
                            else
                            {
                                CashAccount current = this.CashAccount.Current;
                                if (current.CashAccountID == cashAcct.CashAccountID)
                                {
                                    sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountMayNotBeTheSameAsCurrentAccount, PXErrorLevel.Error));
                                }
                                else if (cashAcct.CuryID != current.CuryID)
                                {
                                    sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency, PXErrorLevel.Error));
                                }
                                else
                                {
                                    PaymentMethodAccount pmAccount = PXSelectReadonly2<PaymentMethodAccount, InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>,
                                        And<PaymentMethod.isActive, Equal<True>>>>,
                                        Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
                                            And<Where<PaymentMethodAccount.useForAP, Equal<True>,
                                                Or<PaymentMethodAccount.useForAR, Equal<True>>>>>>.Select(this, cashAcct.CashAccountID);

                                    if (pmAccount == null || pmAccount.CashAccountID.HasValue == false)
                                    {
                                        sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.EntryTypeCashAccountIsNotConfiguredToUseWithAnyPaymentMethod, PXErrorLevel.Warning));
                                    }
                                    else
                                    {
                                        sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, null, null);
                                    }
                                }
                            }
                        }
                        else
                        {
                            row.OffsetCashAccountID = null;
                            PXUIFieldAttribute.SetEnabled<CashAccountETDetail.offsetBranchID>(sender, e.Row, usesCashAccount);
                        }

                    }
                    else
                    {
                        sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, null, null);
                    }

                }

                var state = sender.GetStateExt<CashAccountETDetail.offsetAccountID>(e.Row) as PXFieldState;
                if (state?.ErrorLevel < PXErrorLevel.Error)
                {
                    AccountAttribute.VerifyAccountIsNotControl<CashAccountETDetail.offsetAccountID>(sender, e);
                }

                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.taxZoneID>(sender, e.Row, (et != null && et.Module == GL.BatchModule.CA));
                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.offsetCashAccountID>(sender, e.Row, (et != null && et.UseToReclassifyPayments == true));
                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.offsetAccountID>(sender, e.Row, (et != null && et.UseToReclassifyPayments != true));
                PXUIFieldAttribute.SetEnabled<CashAccountETDetail.offsetSubID>(sender, e.Row, (et != null && et.UseToReclassifyPayments != true));
            }
        }

        protected virtual void CashAccountETDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            bool usesCashAccount = false;
            CAEntryType et = (CAEntryType)PXSelectorAttribute.Select<CashAccountETDetail.entryTypeID>(sender, e.Row);
            if (et != null && et.UseToReclassifyPayments == true)
            {
                if (row.OffsetAccountID.HasValue && row.OffsetSubID.HasValue)
                {
                    CashAccount cashAcct = null;
                    Account acct = null;
                    cashAcct = PXSelectReadonly<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>,
                        And<CashAccount.subID, Equal<Required<CashAccount.subID>>,
                            And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>>>>>.Select(this, row.OffsetAccountID, row.OffsetSubID, row.OffsetBranchID);
                    acct = PXSelectReadonly<GL.Account, Where<GL.Account.accountID, Equal<Required<GL.Account.accountID>>>>.Select(this, row.OffsetAccountID);
                    usesCashAccount = (cashAcct != null);
                    if (!usesCashAccount)
                    {
                        if (sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount, PXErrorLevel.Error)))
                        {
                            throw new PXRowPersistingException(typeof(CashAccountETDetail.offsetAccountID).Name, acct.AccountCD, Messages.CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount);
                        }
                    }
                    else
                    {
                        CashAccount current = this.CashAccount.Current;
                        if (current.CashAccountID == cashAcct.CashAccountID)
                        {
                            if (sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountMayNotBeTheSameAsCurrentAccount, PXErrorLevel.Error)))
                            {
                                throw new PXRowPersistingException(typeof(CashAccountETDetail.offsetAccountID).Name, acct.AccountCD, Messages.OffsetAccountMayNotBeTheSameAsCurrentAccount);
                            }
                        }
                        if (cashAcct.CuryID != current.CuryID)
                        {
                            if (sender.RaiseExceptionHandling<CashAccountETDetail.offsetAccountID>(e.Row, acct.AccountCD, new PXSetPropertyException(Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency, PXErrorLevel.Error)))
                            {
                                throw new PXRowPersistingException(typeof(CashAccountETDetail.offsetAccountID).Name, acct.AccountCD, Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void CashAccountETDetail_OffsetCashAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            cache.SetDefaultExt<CashAccountETDetail.offsetAccountID>(e.Row);
            cache.SetDefaultExt<CashAccountETDetail.offsetSubID>(e.Row);
            cache.SetDefaultExt<CashAccountETDetail.offsetBranchID>(e.Row);
        }

		protected virtual void CashAccountETDetail_OffsetAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CashAccountETDetail row = e.Row as CashAccountETDetail;
			if (row == null) return;
			Account glAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, row.OffsetAccountID);
			if (glAccount != null && glAccount.IsCashAccount == true)
			{
				CashAccount cashAcct = PXSelect<CashAccount,
					Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>>>.Select(this, glAccount.AccountID);
				cache.SetValueExt<CashAccountETDetail.offsetBranchID>(row, cashAcct.BranchID);
				cache.SetValueExt<CashAccountETDetail.offsetSubID>(row, cashAcct.SubID);
			}
			else
			{
				cache.SetValue<CashAccountETDetail.offsetBranchID>(row, null);
			}
		}

        protected virtual void CashAccountETDetail_OffsetAccountID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row != null && row.OffsetCashAccountID != null)
            {
                CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.OffsetCashAccountID);
                if (acct != null)
                {
                    e.NewValue = acct.AccountID;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void CashAccountETDetail_OffsetSubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row != null && row.OffsetCashAccountID != null)
            {
                CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.OffsetCashAccountID);
                if (acct != null)
                {
                    e.NewValue = acct.SubID;
                    e.Cancel = true;
                }
            }
        }

        protected virtual void CashAccountETDetail_OffsetBranchID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            CashAccountETDetail row = (CashAccountETDetail)e.Row;
            if (row == null) return;
			if (row.OffsetCashAccountID != null)
            {
                CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.SelectWindowed(this, 0, 1, row.OffsetCashAccountID);
                if (acct != null)
                {
                    e.NewValue = acct.BranchID;
                    e.Cancel = true;
                }
            }
			else
			{
				e.NewValue = null;
				e.Cancel = true;
			}
        }

        #endregion

        #region Deposit Events
        protected virtual void CashAccountDeposit_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }

        protected virtual void CashAccountDeposit_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }

        protected virtual void CashAccountDeposit_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }

        protected virtual void CashAccountDeposit_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            CashAccountDeposit row = (CashAccountDeposit)e.Row;
        }

        protected virtual void CashAccountDeposit_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (CashAccountDeposit)e.Row;
            PXUIFieldAttribute.SetRequired<CashAccountDeposit.chargeEntryTypeID>(cache, row.ChargeRate != 0);
        }

		protected virtual void CashAccountDeposit_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = (CashAccountDeposit)e.Row;
			PXDefaultAttribute.SetPersistingCheck<CashAccountDeposit.chargeEntryTypeID>(cache, row,
				row.ChargeRate != 0 ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}
		#endregion


		#region CashAccountPaymentMethodDetail Events
		protected virtual void CashAccountPaymentMethodDetail_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert &&
			    (e.Operation & PXDBOperation.Command) != PXDBOperation.Update)
			{
				return;
			}

			var row = (CashAccountPaymentMethodDetail)e.Row;

			if (row == null)
			{
				return;
			}

			PaymentMethodDetail iTempl = this.FindTemplate(row);
			bool isRequired = (iTempl != null) && (iTempl.IsRequired ?? false);
			PXDefaultAttribute.SetPersistingCheck<CashAccountPaymentMethodDetail.detailValue>(cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        protected virtual void CashAccountPaymentMethodDetail_DetailValue_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            CashAccountPaymentMethodDetail row = (CashAccountPaymentMethodDetail)e.Row;
			string newDetailValue = e.NewValue as string;

			if (string.IsNullOrEmpty(newDetailValue))
			{
				PXSetPropertyException exc = new PXSetPropertyException(Messages.ERR_RequiredValueNotEnterd);
				cache.RaiseExceptionHandling<CashAccountPaymentMethodDetail.detailValue>(row, newDetailValue, exc);
			}
        }
        #endregion

        #region Overrides

        public override void Clear()
        {
            base.Clear();
            isPaymentMergedFlag = false;
        }

        Dictionary<string, string> remittancePMErrors = new Dictionary<string, string>();
        public override void Persist()
        {
            #region Validate CashPaymentTypeDetails for Required fields
            foreach (CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Inserted)
            {
                CashAccountPaymentMethodDetail detail = iDet;

                if (!ValidateDetail(iDet))
                {
                    remittancePMErrors[detail.PaymentMethodID] = Messages.SomeRemittanceSettingsForCashAccountAreNotValid;
                }
            }

            foreach (CashAccountPaymentMethodDetail iDet in this.PaymentDetails.Cache.Updated)
            {
                CashAccountPaymentMethodDetail detail = iDet;

                if (!ValidateDetail(iDet))
                {
                    remittancePMErrors[detail.PaymentMethodID] = Messages.SomeRemittanceSettingsForCashAccountAreNotValid;
                }
            }

            if (remittancePMErrors.Count > 0)
            {
                throw new PXException(Messages.SomeRemittanceSettingsForCashAccountAreNotValid);
            }
            #endregion

            #region Validate OffsetCashAccountID
            bool existOffsetCashAccountErrors = false;

            foreach (CashAccountETDetail etDet in this.ETDetails.Cache.Inserted)
            {
				if (!IsCorrectCashAccount(this.ETDetails.Cache, etDet))
				{
					existOffsetCashAccountErrors = true;
				}
            }

            foreach (CashAccountETDetail etDet in this.ETDetails.Cache.Updated)
            {
				if (!IsCorrectCashAccount(this.ETDetails.Cache, etDet))
				{
					existOffsetCashAccountErrors = true;
				}
            }

            if (existOffsetCashAccountErrors)
            {
                throw new PXException(ErrorMessages.RecordRaisedErrors, ErrorMessages.GetLocal(ErrorMessages.Inserting), typeof(CashAccountETDetail).Name);
            }
            #endregion

            base.Persist();
        }

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			try
			{
				return base.Persist(cacheType, operation);
			}
			catch (PXDatabaseException e)
			{
				if (cacheType == typeof(PaymentMethodAccount)
					&& (operation == PXDBOperation.Delete
						|| operation == PXDBOperation.Command)
					&& (e.ErrorCode == PXDbExceptions.DeleteForeignKeyConstraintViolation
						|| e.ErrorCode == PXDbExceptions.DeleteReferenceConstraintViolation))
				{
					string CashAccountCD = this.CurrentCashAccount.Current != null ? this.CurrentCashAccount.Current.CashAccountCD.Trim() : e.Keys[1].ToString();
					throw new PXException(Messages.CannotDeletePaymentMethodAccount, e.Keys[0], CashAccountCD);
				}
				else
				{
					throw;
				}
			}
		}

		#endregion

		#region Utility Functions

		protected virtual void FillPaymentDetails(string aPaymentMethodID)
        {
			CashAccount account = CurrentCashAccount.Current;

			if (account == null || isPaymentMergedFlag)
            {
				return;
			}

                int? accountID = account.CashAccountID;

                    if (!string.IsNullOrEmpty(aPaymentMethodID))
                    {
                        List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();

                        foreach (PaymentMethodDetail it in PXSelect<PaymentMethodDetail,
                                                            Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
													    And<
															Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                                                    Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>.
                                                                                                Select(this, aPaymentMethodID))
                        {
					PaymentMethod pm = PXSelect<PaymentMethod,
										  Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.
									   Select(this, it.PaymentMethodID);

					if (pm == null || pm.UseForCA != true)
								continue;

							CashAccountPaymentMethodDetail detail = null;

                            foreach (CashAccountPaymentMethodDetail iPDet in this.PaymentDetails.Select(accountID, aPaymentMethodID))
                            {
                                if (iPDet.DetailID == it.DetailID)
                                {
                                    detail = iPDet;
                                    break;
                                }
                            }

                            if (detail == null)
                            {
                                toAdd.Add(it);
                            }
                        }

                        using (ReadOnlyScope rs = new ReadOnlyScope(this.PaymentDetails.Cache))
                        {
                            foreach (PaymentMethodDetail it in toAdd)
                            {
						CashAccountPaymentMethodDetail detail = new CashAccountPaymentMethodDetail
						{
							AccountID = account.CashAccountID,
							PaymentMethodID = aPaymentMethodID,
							DetailID = it.DetailID
						};

                                detail = this.PaymentDetails.Insert(detail);
                            }

                            if (toAdd.Count > 0)
                            {
                                this.PaymentDetails.View.RequestRefresh();
                            }
                        }
                    }

                    this.isPaymentMergedFlag = true;
                }

        public virtual void CheckCashAccountInUse(PXCache sender, CashAccount aAcct)
        {
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, aAcct, typeof(AR.CustomerPaymentMethod.cashAccountID));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, aAcct, typeof(CashAccountDeposit.depositAcctID),
				customMessage: Messages.CannotDeleteClearingAccount);
        }

        public virtual void DeleteCashAccountInUse(PXCache sender, CashAccount aAcct)
        {
            foreach (VendorClass item in VendorClasses.Select(aAcct.CashAccountID))
            {
                item.CashAcctID = null;
                VendorClasses.Update(item);
            }

            foreach (CR.Location item in Locations.Select(aAcct.CashAccountID))
            {
                item.VCashAccountID = null;
                Locations.Update(item);
            }
        }

        public virtual bool HasGLTrans(int? aAccountID, int? subID, int? branchID)
        {
            if (aAccountID != null && subID != null && branchID != null)
            {
                GLTran glTranExist = PXSelect<GLTran, Where<GLTran.accountID, Equal<Required<GLTran.accountID>>, And<GLTran.subID, Equal<Required<GLTran.subID>>,
                    And<GLTran.branchID, Equal<Required<GLTran.branchID>>>>>>.SelectWindowed(this, 0, 1, aAccountID, subID, branchID);
                return (glTranExist != null) && (glTranExist.BatchNbr != null);
            }
            else
            {
                return false;
            }
        }


        public virtual bool CheckCashAccountForTransactions(CashAccount aAcct)
        {
            CATran ctr = PXSelect<CATran, Where<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>>>.SelectWindowed(this, 0, 1, aAcct.CashAccountID);
            if (ctr != null) return true;
            return false;
        }

        private void RecalcOptions(CashAccount aRow)
        {
            CashAccountOptions options = this.DetectOptions();
            bool cacheIsDirty = Caches[typeof(CashAccount)].IsDirty;
            aRow.PTInstancesAllowed = (options & CashAccountOptions.HasPTInstances) != 0;
            aRow.AcctSettingsAllowed = (options & CashAccountOptions.HasPTSettings) != 0;
            Caches[typeof(CashAccount)].IsDirty = cacheIsDirty;
        }

        private CashAccountOptions DetectOptions()
        {
            bool AcctSettingsAllowed = false;


            foreach (PXResult<PaymentMethodAccount, PaymentMethod> iPt in this.Details.Select())
            {
                PaymentMethod pt = (PaymentMethod)iPt;

                if (AcctSettingsAllowed == false)
                {
                    PaymentMethodDetail ptd = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
                                                And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>.Select(this, pt.PaymentMethodID);
					if (ptd != null)
					{
						PaymentMethod pm = PXSelect<PaymentMethod,
												Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>
										   .Select(this, ptd.PaymentMethodID);

						if (pm != null && pm.UseForCA == true)
							AcctSettingsAllowed = true;
					}
                }
            }
            CashAccountOptions result = CashAccountOptions.None;

            if (AcctSettingsAllowed)
                result |= CashAccountOptions.HasPTSettings;

            return result;
        }

        protected virtual PaymentMethodDetail FindTemplate(CashAccountPaymentMethodDetail aDet)
        {
            PaymentMethodDetail res = PXSelect<PaymentMethodDetail,
										 Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
                    And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>,
										   And<
											   Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
												  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>.
									  Select(this, aDet.PaymentMethodID, aDet.DetailID);
            return res;
        }

        protected virtual bool ValidateDetail(CashAccountPaymentMethodDetail aRow)
        {
            PaymentMethodDetail iTempl = this.FindTemplate(aRow);

            if (iTempl?.IsRequired == true && string.IsNullOrEmpty(aRow.DetailValue))
            {
				var exception = new PXSetPropertyException(Messages.ERR_RequiredValueNotEnterd);
				this.PaymentDetails.Cache.RaiseExceptionHandling<CashAccountPaymentMethodDetail.detailValue>(aRow, aRow.DetailValue, exception);
                return false;
            }

            return true;
        }

		public virtual PXResultset<PaymentMethodAccount> GetCurrentUsedPaymentMethodAccounts()
		{
			return PXSelect<PaymentMethodAccount,
			Where2<Where<PaymentMethodAccount.useForAP, Equal<True>,
					Or<PaymentMethodAccount.useForAR, Equal<True>>>,
				And<PaymentMethodAccount.cashAccountID, Equal<Current2<CashAccount.cashAccountID>>>>>.Select(this);
		}
		#endregion
	}
}
