using PX.Data;

namespace PX.Objects.CM
{
	public class CurrencyRateTypeMaint : PXGraph<CurrencyRateTypeMaint>
    {
        public PXSavePerRow<CurrencyRateType> Save;
		public PXCancel<CurrencyRateType> Cancel;
		[PXImport(typeof(CurrencyRateType))]
		public PXSelect<CurrencyRateType> CuryRateTypeRecords;

        protected virtual void CurrencyRateType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (CurrencyRateType)e.Row;
            if (row != null)
            { 
                PXUIFieldAttribute.SetEnabled<CurrencyRateType.onlineRateAdjustment>(sender, row, row.RefreshOnline == true);
            }
        }
    }
}