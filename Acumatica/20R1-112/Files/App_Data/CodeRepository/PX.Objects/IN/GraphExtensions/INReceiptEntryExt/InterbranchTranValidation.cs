using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.IN.GraphExtensions.INReceiptEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<INReceiptEntry, INRegister.branchID, INTran, INTran.siteID>
	{
		protected override IEnumerable<INTran> GetDetails()
		{
			return Base.transactions.Select().RowCast<INTran>();
		}
	}
}
