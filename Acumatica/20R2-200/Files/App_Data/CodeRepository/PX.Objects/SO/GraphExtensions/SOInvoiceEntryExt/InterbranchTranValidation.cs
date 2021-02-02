using PX.Data;
using PX.Objects.AR;
using System.Collections.Generic;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<SOInvoiceEntry, ARInvoice.branchID, ARTran, ARTran.siteID>
	{
		protected override IEnumerable<ARTran> GetDetails()
		{
			return Base.Transactions.Select().RowCast<ARTran>();
		}
	}
}
