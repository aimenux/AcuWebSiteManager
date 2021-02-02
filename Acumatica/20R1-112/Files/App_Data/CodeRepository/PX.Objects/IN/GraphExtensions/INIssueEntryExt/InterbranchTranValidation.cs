using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.IN.GraphExtensions.INIssueEntryExt
{
	public class InterbranchTranValidation
		: Extensions.InterbranchSiteRestrictionExtension<INIssueEntry, INRegister.branchID, INTran, INTran.siteID>
	{
		protected override IEnumerable<INTran> GetDetails()
		{
			return Base.transactions.Select().RowCast<INTran>();
		}
	}
}
