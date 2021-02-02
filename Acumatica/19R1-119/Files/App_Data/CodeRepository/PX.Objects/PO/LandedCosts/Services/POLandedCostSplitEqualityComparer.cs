using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO.LandedCosts
{
	[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R2)]
	public class POLandedCostSplitEqualityComparer : IEqualityComparer<POLandedCostSplit>
	{
		public bool Equals(POLandedCostSplit x, POLandedCostSplit y)
		{
			if (x == null && y == null)
				return true;

			if (x == null || y == null)
				return false;

			var result = x.DocType == y.DocType && x.RefNbr == y.RefNbr && x.ReceiptLineNbr == y.ReceiptLineNbr && x.DetailLineNbr == y.DetailLineNbr;

			return result;
		}

		public int GetHashCode(POLandedCostSplit obj)
		{
			int hash = 17;
			unchecked
			{
				hash = hash * 23 + (obj.DocType ?? "").GetHashCode();
				hash = hash * 23 + (obj.RefNbr ?? "").GetHashCode();
				hash = hash * 23 + obj.ReceiptLineNbr ?? 0;
				hash = hash * 23 + obj.DetailLineNbr ?? 0;
			}
			return hash;
		}
	}
}
