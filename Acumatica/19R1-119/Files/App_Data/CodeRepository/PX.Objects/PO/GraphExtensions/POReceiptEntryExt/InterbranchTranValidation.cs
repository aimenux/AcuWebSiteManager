using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<POReceiptEntry, POReceipt.branchID, POReceiptLine, POReceiptLine.siteID>
	{
		protected override IEnumerable<POReceiptLine> GetDetails()
		{
			return Base.transactions.Select().RowCast<POReceiptLine>();
		}
	}
}
