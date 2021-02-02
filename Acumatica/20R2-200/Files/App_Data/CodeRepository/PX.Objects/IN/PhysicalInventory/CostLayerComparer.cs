using System.Collections.Generic;

namespace PX.Objects.IN.PhysicalInventory
{
	public class CostLayerComparer : IComparer<INCostStatus>
	{
		private INItemSiteSettings _itemSite;

		public CostLayerComparer(INItemSiteSettings itemSite)
		{
			_itemSite = itemSite;
		}

		public virtual int Compare(INCostStatus x, INCostStatus y)
		{
			if (_itemSite.ValMethod == INValMethod.FIFO)
				return CompareFIFO(x, y);

			return CompareDefault(x, y);
		}

		protected virtual int CompareDefault(INCostStatus x, INCostStatus y)
		{
			if (x.AccountID != y.AccountID || x.SubID != y.SubID)
			{
				if (y.AccountID == _itemSite.InvtAcctID && y.SubID == _itemSite.InvtSubID)
					return -1;

				if (x.AccountID == _itemSite.InvtAcctID && x.SubID == _itemSite.InvtSubID)
					return 1;
			}

			int result = x.AccountID.Value.CompareTo(y.AccountID.Value);
			if (result != 0)
				return result;

			result = x.SubID.Value.CompareTo(y.SubID.Value);
			if (result != 0)
				return result;

			return x.CostID.Value.CompareTo(y.CostID.Value);
		}

		protected virtual int CompareFIFO(INCostStatus x, INCostStatus y)
		{
			int result = x.ReceiptDate.Value.CompareTo(y.ReceiptDate.Value);
			if (result != 0)
				return result;

			result = x.ReceiptNbr.CompareTo(y.ReceiptNbr);
			if (result != 0)
				return result;

			return CompareDefault(x, y);
		}
	}
}
