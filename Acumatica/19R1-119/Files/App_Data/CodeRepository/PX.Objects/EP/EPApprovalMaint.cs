using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class EPApprovalMaint : PXGraph<EPApprovalMaint, EPApproval>
	{		
		public PXSelect<EPApproval> Approval;

		public EPApprovalMaint()
		{
			this.Approval.Cache.AllowDelete = false;
		}
	}
}
