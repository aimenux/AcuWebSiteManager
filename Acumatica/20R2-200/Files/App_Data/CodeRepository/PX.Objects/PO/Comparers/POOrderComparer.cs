using System;
using System.Collections.Generic;
using PX.Objects.AP;

namespace PX.Objects.PO
{
	public class POOrderComparer : IEqualityComparer<APTran>
	{
		public POOrderComparer()
		{
		}

		#region IEqualityComparer<APTran> Members

		public bool Equals(APTran x, APTran y)
		{
			return x.POOrderType == y.POOrderType && x.PONbr == y.PONbr;
		}

		public int GetHashCode(APTran obj)
		{
			return obj.POOrderType.GetHashCode() + 109 * obj.PONbr.GetHashCode();
		}

		#endregion
	}
}
