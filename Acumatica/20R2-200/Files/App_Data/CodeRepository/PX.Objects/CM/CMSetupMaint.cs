using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CM
{
	public class CMSetupMaint : PXGraph<CMSetupMaint>
	{
		public PXSelect<CMSetup> cmsetup;
		public PXSave<CMSetup> Save;
		public PXCancel<CMSetup> Cancel;
		public PXSelect<Currency, Where<Currency.curyID, Equal<Current<Company.baseCuryID>>>> basecurrency;
		public PXSetup<Company> company;
		public PXSelect<TranslDef, Where<TranslDef.translDefId, Equal<Current<CMSetup.translDefId>>>> baseTranslDef;

		public CMSetupMaint()
		{
			Company setup = company.Current;
			if (string.IsNullOrEmpty(setup.BaseCuryID))
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}

		protected virtual void Currency_RealGainAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void Currency_RealLossAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void Currency_TranslationGainAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void Currency_TranslationLossAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void CMSetup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (basecurrency.Current == null)
			{
				basecurrency.Current = basecurrency.Select();
			}

			basecurrency.Cache.MarkUpdated(basecurrency.Current);
		}

		protected virtual void CMSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (basecurrency.Current != null)
			{
				basecurrency.Cache.MarkUpdated(basecurrency.Current);
			}
		}

        #region Cache Attached Events 
        #region Currency
        #region CuryID

        [PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
        [PXDefault()]
        [PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<Currency.curyID>), CacheGlobal = true)]
        protected virtual void Currency_CuryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
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
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		protected virtual void Currency_RoundingGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingGainSubID
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		protected virtual void Currency_RoundingGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingLossAcctID
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		protected virtual void Currency_RoundingLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingLossSubID
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		protected virtual void Currency_RoundingLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealGainAcctID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RealGainAcctID_CacheAttached(PXCache cache) { }
		#endregion
		#region RealLossAcctID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_RealLossAcctID_CacheAttached(PXCache cache) { }
		#endregion
		#region TranslationGainAcctID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_TranslationGainAcctID_CacheAttached(PXCache cache) { }
		#endregion
		#region TranslationLossAcctID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[AvoidControlAccounts]
		protected virtual void Currency_TranslationLossAcctID_CacheAttached(PXCache cache) { }
		#endregion
		#endregion
		#endregion
	}
}
