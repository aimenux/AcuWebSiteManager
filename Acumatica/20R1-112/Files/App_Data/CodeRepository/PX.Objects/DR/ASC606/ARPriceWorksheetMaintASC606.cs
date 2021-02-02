using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	public class ARPriceWorksheetMaintASC606 : PXGraphExtension<ARPriceWorksheetMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		public override void Initialize()
		{
			PXUIFieldAttribute.SetVisible<ARPriceWorksheet.isFairValue>(Base.Document.Cache, null, true);
			PXUIFieldAttribute.SetVisible<ARPriceWorksheet.isProrated>(Base.Document.Cache, null, true);
		}

		protected virtual void _(Events.FieldVerifying<ARPriceWorksheet, ARPriceWorksheet.isPromotional> e)
		{
			if (e.Row?.IsFairValue == true && ((bool?)e.NewValue) == true)
			{
				throw new PXSetPropertyException(AR.Messages.PromotionalCannotBeFairValue, PXErrorLevel.Error);
			}
		}

		protected virtual void _(Events.FieldVerifying<ARPriceWorksheet, ARPriceWorksheet.isFairValue> e)
		{
			if (e.Row?.IsPromotional == true && ((bool?)e.NewValue) == true)
			{
				throw new PXSetPropertyException(AR.Messages.PromotionalCannotBeFairValue, PXErrorLevel.Error);
			}
		}

		protected virtual void _(Events.FieldUpdated<ARPriceWorksheet, ARPriceWorksheet.isFairValue> e)
		{
			if (((bool?)e.OldValue) == true && e.Row?.IsFairValue == false)
			{
				e.Row.IsProrated = false;
			}
		}

		protected virtual void _(Events.FieldUpdated<CopyPricesFilter, CopyPricesFilter.isFairValue> e)
		{
			if (((bool?)e.OldValue) == true && e.Row?.IsFairValue == false)
			{
				e.Row.IsProrated = false;
			}
		}

		protected virtual void _(Events.RowSelected<CopyPricesFilter> e)
		{
			CopyPricesFilter filter = e.Row;
			if (filter == null)
			{
				return;
			}

			PXUIFieldAttribute.SetVisible<CopyPricesFilter.isFairValue>(e.Cache, filter, true);
			PXUIFieldAttribute.SetVisible<CopyPricesFilter.isProrated>(e.Cache, filter, true);

			PXUIFieldAttribute.SetEnabled<CopyPricesFilter.isPromotional>(e.Cache, filter, filter.IsFairValue != true);
			PXUIFieldAttribute.SetEnabled<CopyPricesFilter.isProrated>(e.Cache, filter, filter.IsFairValue == true);
			PXUIFieldAttribute.SetEnabled<CopyPricesFilter.isFairValue>(e.Cache, filter, filter.IsPromotional != true);
		}
	}
}
