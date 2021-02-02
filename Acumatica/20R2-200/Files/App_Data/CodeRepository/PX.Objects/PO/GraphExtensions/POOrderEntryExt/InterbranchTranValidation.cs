using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<POOrderEntry, POOrder.branchID, POLine, POLine.siteID>
	{
		protected override void _(Events.FieldVerifying<POOrder.branchID> e)
		{
			var headerRow = Base.CurrentDocument.Current;
			if (headerRow?.OrderType == POOrderType.StandardBlanket)
				return;

			base._(e);
		}

		protected override void _(Events.RowPersisting<POLine> e)
		{
			var headerRow = Base.CurrentDocument.Current;
			if (headerRow?.OrderType == POOrderType.StandardBlanket)
				return;

			base._(e);
		}

		protected override IEnumerable<POLine> GetDetails()
		{
			return Base.Transactions.Select().RowCast<POLine>();
		}
	}
}