using System;

namespace PX.Objects.PO
{
	public interface IAPTranSource
	{
		Int32? BranchID { get; }
		Int32? ExpenseAcctID { get; }
		Int32? ExpenseSubID { get; }
		Int32? POAccrualAcctID { get; }
		Int32? POAccrualSubID { get; }
		String LineType { get; }
		Int32? SiteID { get; }
		Int32? InventoryID { get; }
		bool? AccrueCost { get; }
		string OrigUOM { get; }
		String UOM { get; }
		Int64? CuryInfoID { get; }
		decimal? OrigQty { get; }
		decimal? BaseOrigQty { get; }
		decimal? BillQty { get; }
		decimal? BaseBillQty { get; }
		decimal? CuryUnitCost { get; }
		decimal? UnitCost { get; }
		decimal? CuryDiscAmt { get; }
		decimal? DiscAmt { get; }
		decimal? DiscPct { get; }
		decimal? CuryRetainageAmt { get; }
		decimal? RetainageAmt { get; }
		decimal? RetainagePct { get; }
		decimal? CuryLineAmt { get; }
		decimal? LineAmt { get; }
		String TaxCategoryID { get; }
		String TranDesc { get; }
		String TaxID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		string POAccrualType { get; }
		Guid? POAccrualRefNoteID { get; }
		int? POAccrualLineNbr { get; }

		int? CostCodeID { get; }
		bool IsReturn { get; }
		bool IsPartiallyBilled { get; }
		bool AggregateWithExistingTran { get; }

		String DiscountID { get; }

		String DiscountSequenceID { get; }

		decimal? GroupDiscountRate { get; }

		decimal? DocumentDiscountRate { get; }

		bool CompareReferenceKey(AP.APTran aTran);
		void SetReferenceKeyTo(AP.APTran aTran);

	}
}
