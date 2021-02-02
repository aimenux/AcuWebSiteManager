using PX.Data;

namespace PX.Objects.PM.ChangeRequest.GraphExtensions
{
	public class ChangeOrderClassMaintExt : PXGraphExtension<PX.Objects.PM.ChangeOrderClassMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.changeRequest>();
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderClass, PMChangeOrderClass.isAdvance> e)
		{
			if (e.Row != null && e.Row.IsAdvance == true)
			{
				e.Row.IsCostBudgetEnabled = true;
				e.Row.IsPurchaseOrderEnabled = true;
			}
		}

		protected virtual void _(Events.RowSelected<PMChangeOrderClass> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMChangeOrderClass.isCostBudgetEnabled>(e.Cache, e.Row, e.Row.IsAdvance != true);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderClass.isPurchaseOrderEnabled>(e.Cache, e.Row, e.Row.IsAdvance != true);
			}
		}
	}
}
