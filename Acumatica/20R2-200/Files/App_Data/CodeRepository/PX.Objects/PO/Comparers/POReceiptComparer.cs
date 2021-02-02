using System;
using System.Collections.Generic;
using PX.Objects.AP;

namespace PX.Objects.PO
{
	public class POReceiptComparer : IEqualityComparer<APTran>
	{
		public POReceiptComparer()
		{
		}

		#region IEqualityComparer<APTran> Members

		public bool Equals(APTran x, APTran y)
		{
			return x.ReceiptNbr == y.ReceiptNbr;
		}

		public int GetHashCode(APTran obj)
		{
			return obj.ReceiptNbr.GetHashCode();
		}

		#endregion
	}
}
