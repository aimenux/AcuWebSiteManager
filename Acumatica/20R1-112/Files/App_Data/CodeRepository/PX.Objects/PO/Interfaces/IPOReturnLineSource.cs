using System;

namespace PX.Objects.PO
{
	public interface IPOReturnLineSource
	{
		string ReceiptNbr { get; }
		int? LineNbr { get; }
		string LineType { get; }
		string POType { get; }
		string PONbr { get; }
		int? POLineNbr { get; }
		int? InventoryID { get; }
		bool? AccrueCost { get; }
		int? SubItemID { get; }
		int? SiteID { get; }
		int? LocationID { get; }
		string LotSerialNbr { get; }
		DateTime? ExpireDate { get; }
		string UOM { get; }
		decimal? ReceiptQty { get; }
		decimal? BaseReceiptQty { get; }
		decimal? ReturnedQty { get; set; }
		decimal? BaseReturnedQty { get; }
		int? ExpenseAcctID { get; }
		int? ExpenseSubID { get; }
		int? POAccrualAcctID { get; }
		int? POAccrualSubID { get; }
		string TranDesc { get; }
		int? CostCodeID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		decimal? UnitCost { get; }
		decimal? TranCostFinal { get; }
		decimal? TranCost { get; }
	}
}
