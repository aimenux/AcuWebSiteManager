using System;
using System.Collections;
using PX.Data;
using PX.Objects.Common.EntityInUse;
using PX.Objects.GL;

namespace PX.Objects.CM
{
	public class CurrencyMaint : PXGraph<CurrencyMaint, CurrencyList>
	{
		[PXCopyPasteHiddenView]
		public PXSelect<CurrencyList, Where<CurrencyList.curyID, NotEqual<Current<Company.baseCuryID>>>> CuryListRecords;
		public PXSelect<Currency, Where<Currency.curyID, NotEqual<Current<Company.baseCuryID>>, And<Currency.curyID, Equal<Current<CurrencyList.curyID>>>>> CuryRecords;

		public virtual IEnumerable curyRecords()
		{
			PXResultset<Currency> res = PXSelect<Currency,Where<Currency.curyID, NotEqual<Current<Company.baseCuryID>>,And<Currency.curyID, Equal<Current<CurrencyList.curyID>>>>>.Select(this);
			
			if (res.Count == 0 && CuryListRecords.Current != null)
			{
				Currency newrow = new Currency();
				newrow.CuryID = CuryListRecords.Current.CuryID;
				res.Add(new PXResult<Currency>(newrow));
			}
			return res;
		}

		public PXSetup<Company> company;
				
		public CurrencyMaint()
		{
			Company setup = company.Current;
			if (string.IsNullOrEmpty(setup.BaseCuryID))
			{
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
			PXUIFieldAttribute.SetVisible(CuryRecords.Cache, null, PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisible<CurrencyList.isFinancial>(CuryListRecords.Cache, null, PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>());
		}
		
		protected virtual void Currency_DecimalPlaces_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			cache.SetValue(e.Row, typeof(Currency.decimalPlaces).Name, (short)2);
		}

		protected virtual void CurrencyList_DecimalPlaces_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CurrencyList currencyList = e.Row as CurrencyList;
			if (currencyList == null) return;

			WebDialogResult wdr =
				CuryListRecords.Ask(
					CS.Messages.Warning,
					CS.Messages.ChangingCurrencyPrecisionWarning,
					MessageButtons.YesNo,
					MessageIcon.Warning);

			e.NewValue = wdr == WebDialogResult.Yes 
				? e.NewValue 
				: currencyList.DecimalPlaces;
		}

		protected virtual void CurrencyList_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CurrencyList;
			if (row == null)
				return;

			if (CuryRecords.Cache.Current != null && row.IsFinancial != true)
			{
				CuryRecords.Cache.Delete(CuryRecords.Cache.Current);
			}
			if (row.IsFinancial == true && CuryRecords.Cache.Current != null && CuryRecords.Current.tstamp == null)
			{
				CuryRecords.Cache.Insert(CuryRecords.Cache.Current);
			}
		}

		protected void CurrencyList_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CurrencyList currencyList = e.Row as CurrencyList;
			if (currencyList == null) return;

			if (EntityInUseHelper.IsEntityInUse<CurrencyInUse>(currencyList.CuryID))
			{
				PXUIFieldAttribute.SetEnabled<Currency.decimalPlaces>(cache, currencyList, false);
			}
		}

		protected void Currency_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CurrencyList currencyListRow = CuryListRecords.Current;
			Currency currencyRow = (Currency)e.Row;
			PXUIFieldAttribute.SetEnabled(cache, currencyRow, currencyListRow != null && currencyListRow.IsFinancial == true);

			PXUIFieldAttribute.SetEnabled<Currency.aRInvoiceRounding>(cache, currencyRow, currencyRow.UseARPreferencesSettings == false);
			PXUIFieldAttribute.SetEnabled<Currency.aRInvoicePrecision>(cache, currencyRow, currencyRow.UseARPreferencesSettings == false && currencyRow.ARInvoiceRounding != CS.RoundingType.Currency);
			PXUIFieldAttribute.SetEnabled<Currency.aPInvoiceRounding>(cache, currencyRow, currencyRow.UseAPPreferencesSettings == false);
			PXUIFieldAttribute.SetEnabled<Currency.aPInvoicePrecision>(cache, currencyRow, currencyRow.UseAPPreferencesSettings == false && currencyRow.APInvoiceRounding != CS.RoundingType.Currency);
		}

		protected void Currency_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CurrencyList currencyListRow = CuryListRecords.Current;
			if (currencyListRow != null && currencyListRow.IsFinancial == false && e.Operation == PXDBOperation.Insert)
			{
				e.Cancel = true;
			}
		}

		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.curyID, NotEqual<Current<Company.baseCuryID>>>>))]
		[PX.Data.EP.PXFieldDescription]
		protected virtual void CurrencyList_CuryID_CacheAttached(PXCache cache)
		{
		}

		#region Currency Control Accounts
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RealGainAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RealLossAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RevalGainAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RevalLossAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_ARProvAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_APProvAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_TranslationGainAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_TranslationLossAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_UnrealizedGainAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_UnrealizedLossAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RoundingGainAcctID_CacheAttached(PXCache cache) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RoundingLossAcctID_CacheAttached(PXCache cache) { }
		#endregion
	}
}
