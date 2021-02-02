using System;
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	public class TaxZoneMaint : PXGraph<TaxZoneMaint, TaxZone>
	{
		public PXSelect<TaxZone> TxZone;
		public PXSelectJoin<TaxZoneDet, InnerJoin<Tax, On<TaxZoneDet.taxID, Equal<Tax.taxID>>>, Where<TaxZoneDet.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>> Details;
        public PXSelect<TaxZoneDet> TxZoneDet;
        [PXImport(typeof(TaxZone))]
		public PXSelect<TaxZoneZip, Where<TaxZoneZip.taxZoneID, Equal<Current<TaxZone.taxZoneID>>>> Zip;

		public TaxZoneMaint()
		{
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(GL.Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}
		public PXSetup<GL.Branch> Company;

		protected virtual void TaxZoneDet_TaxID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			TaxZoneDet tax = (TaxZoneDet)e.Row;
			string taxId = (string) e.NewValue;
			if (tax.TaxID != taxId) 
			{
				foreach (TaxZoneDet iTax in this.Details.Select())
				{
					if (iTax.TaxID == taxId) 
					{
						e.Cancel = true;
						throw new PXSetPropertyException(Messages.TaxAlreadyInList);
					}
				}
			}
		}

		protected virtual void TaxZone_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TaxZone row = e.Row as TaxZone;
			if (row == null) return;

			var externalTaxActive = PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();

			PXUIFieldAttribute.SetVisible<TaxZone.isExternal>(cache, null, externalTaxActive);
			PXUIFieldAttribute.SetVisible<TaxZone.taxPluginID>(cache, e.Row, row.IsExternal == true);
			PXUIFieldAttribute.SetVisible<TaxZone.taxVendorID>(cache, e.Row, row.IsExternal == true);
			PXUIFieldAttribute.SetVisible<TaxZone.taxID>(cache, e.Row, row.IsManualVATZone == true);
			PXDefaultAttribute.SetPersistingCheck<TaxZone.taxID>(cache, e.Row, 
				PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>() && row.IsManualVATZone == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<TaxZone.taxPluginID>(cache, e.Row, row.IsExternal == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<TaxZone.taxVendorID>(cache, e.Row, row.IsExternal == true ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			Details.Cache.AllowInsert = row.IsExternal != true;
			Details.Cache.AllowUpdate = row.IsExternal != true;
			Details.Cache.AllowDelete = row.IsExternal != true;
			Zip.Cache.AllowInsert = row.IsExternal != true;
			Zip.Cache.AllowUpdate = row.IsExternal != true;
			Zip.Cache.AllowDelete = row.IsExternal != true;
		}
		
		protected virtual void TaxZone_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			TaxZone newrow = e.NewRow as TaxZone;
			if (newrow == null)
				return;

			TaxZone row = e.Row as TaxZone;
			if (row == null)
				return;

			if (newrow.IsManualVATZone == false && row.IsManualVATZone != false)
			{
				cache.SetValueExt<TaxZone.taxID>(newrow, null);
			}
		}
	}


}
