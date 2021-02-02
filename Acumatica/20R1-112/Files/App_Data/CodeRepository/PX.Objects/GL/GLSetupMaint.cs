using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class GLSetupMaint : PXGraph<GLSetupMaint>
	{
		public PXSelect<GLSetup> GLSetupRecord;
		public PXSave<GLSetup> Save;
		public PXCancel<GLSetup> Cancel;
		public PXSetup<Company> company;
		public PXSelect<Currency, Where<Currency.curyID, Equal<Current<Company.baseCuryID>>>> basecurrency;

		#region Cache Attached

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(null)]
		protected virtual void Currency_TranslationGainAcctID_CacheAttached(PXCache sender)
		{	
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Currency.translationGainAcctID))]
		protected virtual void Currency_TranslationGainSubID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(null)]
		protected virtual void Currency_TranslationLossAcctID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Currency.translationLossAcctID))]
		protected virtual void Currency_TranslationLossSubID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(null)]
		protected virtual void Currency_RealGainAcctID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Currency.realGainAcctID))]
		protected virtual void Currency_RealGainSubID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(null)]
		protected virtual void Currency_RealLossAcctID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Currency.realLossAcctID))]
		protected virtual void Currency_RealLossSubID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		public GLSetupMaint()
		{
			if (string.IsNullOrEmpty(company.Current.BaseCuryID))
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.OrganizationMaint));
			}
        }

        #region Events - GLSetup
        protected virtual void GLSetup_BegFiscalYear_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = Accessinfo.BusinessDate;
        }

        protected virtual void GLSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            GLSetup OldRow = (GLSetup)PXSelectReadonly<GLSetup>.Select(this);
            GLSetup NewRow = (GLSetup)e.Row;
            if ((OldRow == null || OldRow.COAOrder != NewRow.COAOrder) && NewRow.COAOrder < 4)
            {
                for (short i = 0; i < 4; i++)
                {
                    PXDatabase.Update<Account>(new PXDataFieldAssign("COAOrder", Convert.ToInt32(AccountType.COAOrderOptions[(int)NewRow.COAOrder].Substring((int)i, 1))),
                                                                        new PXDataFieldRestrict("Type", AccountType.Literal(i)));
                    PXDatabase.Update<PM.PMAccountGroup>(new PXDataFieldAssign(typeof(PM.PMAccountGroup.sortOrder).Name, Convert.ToInt32(AccountType.COAOrderOptions[(int)NewRow.COAOrder].Substring((int)i, 1))),
                                                                        new PXDataFieldRestrict(typeof(PM.PMAccountGroup.type).Name, AccountType.Literal(i)));
                }
            }
        }

        protected virtual void GLSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null) return;

            GLSetup setup = e.Row as GLSetup;
            bool hasHistory = GLUtility.IsAccountHistoryExist(this, setup.YtdNetIncAccountID);
            PXUIFieldAttribute.SetEnabled<GLSetup.ytdNetIncAccountID>(GLSetupRecord.Cache, setup, !hasHistory);
            PXUIFieldAttribute.SetEnabled<GLSetup.retEarnAccountID>(GLSetupRecord.Cache, setup, true);
        }

		protected virtual void GLSetup_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			GLSetup setup = e.Row as GLSetup;
			if (setup == null) return;
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>())
			{
				basecurrency.Cache.SetStatus(basecurrency.Current, PXEntryStatus.Updated);
			}
		}

        protected virtual void GLSetup_YtdNetIncAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            GLSetup row = (GLSetup)e.Row;
            if (row == null) return;

            if (e.NewValue != null)
            {
                Account YtdAccNew = PXSelect<Account, Where<Account.accountID, Equal<Required<GLSetup.ytdNetIncAccountID>>>>.Select(this, e.NewValue);
                if ((int?) e.NewValue == row.RetEarnAccountID)
                {
                    Account YtdAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.ytdNetIncAccountID>>>>.SelectSingleBound(this, new object[] {row});
                    Account REAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.retEarnAccountID>>>>.SelectSingleBound(this, new object[] {row});
                    e.NewValue = YtdAcc == null ? null : YtdAcc.AccountCD;
                    throw new PXSetPropertyException(CS.Messages.Entry_NE, REAcc.AccountCD);
                }

				if (YtdAccNew.GLConsolAccountCD != null)
				{
					throw new PXSetPropertyException(Messages.AccountCannotBeSpecifiedAsTheYTDNetIncome);
				}

                if (GLUtility.IsAccountHistoryExist(this, (int?) e.NewValue))
                {
                    e.NewValue = YtdAccNew == null ? null : YtdAccNew.AccountCD;
                    throw new PXSetPropertyException(Messages.AccountExistsHistory2);
                }
            }
        }

        protected virtual void GLSetup_RetEarnAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            GLSetup row = (GLSetup)e.Row;
            if (row == null) return;

            if (e.NewValue != null && (int?)e.NewValue == row.YtdNetIncAccountID)
            {
                Account YtdAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.ytdNetIncAccountID>>>>.SelectSingleBound(this, new object[] { row });
                Account REAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.retEarnAccountID>>>>.SelectSingleBound(this, new object[] { row });
                e.NewValue = REAcc == null ? null : REAcc.AccountCD;
                throw new PXSetPropertyException(CS.Messages.Entry_NE, YtdAcc.AccountCD);
            }

            if (e.NewValue != null && GLUtility.IsAccountHistoryExist(this, (int?)e.NewValue))
            {
                sender.RaiseExceptionHandling<GLSetup.retEarnAccountID>(e.Row, null, new PXSetPropertyException(Messages.AccountExistsHistory2, PXErrorLevel.Warning));
            }
        }

		protected virtual void Currency_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as Currency;
			if (row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetVisible<Currency.curyID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>());

			bool roundingRequired = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>();
			PXUIFieldAttribute.SetRequired<Currency.roundingGainAcctID>(sender, roundingRequired);
			PXUIFieldAttribute.SetRequired<Currency.roundingGainSubID>(sender, roundingRequired);
			PXUIFieldAttribute.SetRequired<Currency.roundingLossAcctID>(sender, roundingRequired);
			PXUIFieldAttribute.SetRequired<Currency.roundingLossSubID>(sender, roundingRequired);
		}

		protected virtual void Currency_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as Currency;
			if (row == null)
			{
				return;
			}
			
			bool roundingRequired = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>();
			PXDefaultAttribute.SetPersistingCheck<Currency.roundingGainAcctID>(sender, row, roundingRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<Currency.roundingGainSubID>(sender, row, roundingRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<Currency.roundingLossAcctID>(sender, row, roundingRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<Currency.roundingLossSubID>(sender, row, roundingRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		protected virtual void Currency_RoundingGainAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void Currency_RoundingLossAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}
        
        #endregion

		#region Cache Attached Events
		#region Currency
		#region RevalGainAcctID
		[PXDBInt()]
		protected virtual void Currency_RevalGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalGainSubID
		[PXDBInt()]
		protected virtual void Currency_RevalGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalLossAcctID
		[PXDBInt()]
		protected virtual void Currency_RevalLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalLossSubID
		[PXDBInt()]
		protected virtual void Currency_RevalLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedGainAcctID
		[PXDBInt()]
		protected virtual void Currency_UnrealizedGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedGainSubID
		[PXDBInt()]
		protected virtual void Currency_UnrealizedGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedLossAcctID
		[PXDBInt()]
		protected virtual void Currency_UnrealizedLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedLossSubID
		[PXDBInt()]
		protected virtual void Currency_UnrealizedLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingGainAcctID
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(null,
			DisplayName = "Rounding Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		protected virtual void Currency_RoundingGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingGainSubID
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Currency.roundingGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Rounding Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		protected virtual void Currency_RoundingGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingLossAcctID
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(null,
			DisplayName = "Rounding Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		protected virtual void Currency_RoundingLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingLossSubID
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Currency.roundingLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Rounding Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		protected virtual void Currency_RoundingLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion
		#endregion

		public override void Persist()
        {
            base.Persist();
        }
    }
	
}
