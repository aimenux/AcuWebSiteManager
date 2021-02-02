using System;
using System.Collections.Generic;
using PX.Objects.AP;

namespace PX.Objects.PO
{
	public class POAccrualComparer : IEqualityComparer<APTran>
	{
		public POAccrualComparer()
		{
		}

		#region IEqualityComparer<APTran> Members

		public bool Equals(APTran x, APTran y)
		{
			return x.POAccrualType == y.POAccrualType
				&& x.POAccrualRefNoteID == y.POAccrualRefNoteID
				&& x.POAccrualLineNbr == y.POAccrualLineNbr;
		}

		public int GetHashCode(APTran obj)
		{
			unchecked
			{
				int ret = 17;
				ret = ret * 23 + obj.POAccrualType?.GetHashCode() ?? 0;
				ret = ret * 23 + obj.POAccrualRefNoteID?.GetHashCode() ?? 0;
				ret = ret * 23 + obj.POAccrualLineNbr?.GetHashCode() ?? 0;
				return ret;
			}
		}

		#endregion
	}
}
