using System;
using System.Collections.Generic;
using PX.Data;
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

	public class POLineComparer : IEqualityComparer<APTran>
	{
		public POLineComparer()
		{
		}

		#region IEqualityComparer<APTran> Members

		public bool Equals(APTran x, APTran y)
		{
			return x.POOrderType == y.POOrderType
				&& x.PONbr == y.PONbr
				&& x.POLineNbr == y.POLineNbr;
		}

		public int GetHashCode(APTran obj)
		{
			unchecked
			{
				int ret = 17;
				ret = ret * 23 + obj.POOrderType?.GetHashCode() ?? 0;
				ret = ret * 23 + obj.PONbr?.GetHashCode() ?? 0;
				ret = ret * 23 + obj.POLineNbr?.GetHashCode() ?? 0;
				return ret;
			}
		}

		#endregion
	}

	public class POAccrualSet : HashSet<APTran>
	{
		private APTran _apTran;

		public POAccrualSet()
			: base(new POAccrualComparer())
		{
		}

		public POAccrualSet(IEqualityComparer<APTran> comparer)
			: base(comparer)
		{
		}

		public POAccrualSet(IEnumerable<APTran> trans, IEqualityComparer<APTran> comparer)
			: base(trans, comparer)
		{
		}

		public bool Contains(POLineRS line)
		{
			if (_apTran == null) _apTran = new APTran();
			_apTran.POOrderType = line.OrderType;
			_apTran.PONbr = line.OrderNbr;
			_apTran.POLineNbr = line.LineNbr;
			_apTran.POAccrualType = line.POAccrualType;
			_apTran.POAccrualRefNoteID = line.OrderNoteID;
			_apTran.POAccrualLineNbr = line.LineNbr;
			return this.Contains(_apTran);
		}

		public bool Contains(LinkLineOrder line)
		{
			if (line.POAccrualType != POAccrualType.Order)
				throw new PXArgumentException(nameof(line));
			if (_apTran == null) _apTran = new APTran();
			_apTran.POOrderType = line.OrderType;
			_apTran.PONbr = line.OrderNbr;
			_apTran.POLineNbr = line.OrderLineNbr;
			_apTran.POAccrualType = line.POAccrualType;
			_apTran.POAccrualRefNoteID = line.OrderNoteID;
			_apTran.POAccrualLineNbr = line.OrderLineNbr;
			return this.Contains(_apTran);
		}
	}
}
