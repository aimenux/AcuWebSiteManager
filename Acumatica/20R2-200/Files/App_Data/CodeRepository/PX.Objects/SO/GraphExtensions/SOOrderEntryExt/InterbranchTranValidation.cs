using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System.Collections.Generic;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<SOOrderEntry, SOOrder.branchID, SOLine, SOLine.siteID>
	{
		protected override void _(Events.FieldVerifying<SOOrder.branchID> e)
		{
			var headerRow = Base.CurrentDocument.Current;
			if (headerRow?.Behavior == SOBehavior.QT)
				return;

			if (Base.IsTransferOrder && !IsDestinationSiteValid((SOOrder)e.Row, (int?)e.NewValue))
			{
				RaiseBranchFieldWarning((int?)e.NewValue, Messages.InappropriateDestSite);
				return; // To avoid warning override. Let Destination Warehouse warning go first.
			}

			base._(e);
		}

		protected override void _(Events.RowPersisting<SOLine> e)
		{
			var headerRow = Base.CurrentDocument.Current;
			if (headerRow?.Behavior == SOBehavior.QT)
				return;

			base._(e);
		}

		protected override IEnumerable<SOLine> GetDetails()
		{
			return Base.Transactions.Select().RowCast<SOLine>();
		}
		
		protected virtual bool IsDestinationSiteValid(SOOrder row, int? newBranchId)
		{
			if (newBranchId == null || PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
				return true;

			var destSite = INSite.PK.Find(Base, row.DestinationSiteID);
			if (destSite == null || PXAccess.IsSameParentOrganization(newBranchId, destSite.BranchID))
				return true;

			return false;
		}

		protected virtual void SOOrder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Delete || !Base.IsTransferOrder)
				return;

			var order = (SOOrder)e.Row;
			if (order?.BranchID == null || PXAccess.FeatureInstalled<FeaturesSet.interBranch>())
				return;

			var destSite = INSite.PK.Find(Base, order.DestinationSiteID);
			if (destSite == null || PXAccess.IsSameParentOrganization(order.BranchID, destSite.BranchID))
				return;

			cache.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, destSite.SiteCD, new PXSetPropertyException(Common.Messages.InterBranchFeatureIsDisabled));
		}
	}
}
