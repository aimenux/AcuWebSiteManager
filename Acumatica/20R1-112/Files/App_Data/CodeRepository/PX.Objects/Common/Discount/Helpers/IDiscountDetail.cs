namespace PX.Objects.Common.Discount
{
	public interface IDiscountDetail
	{
		int? RecordID { get; set; }
		ushort? LineNbr { get; set; }
		bool? SkipDiscount { get; set; }
		string DiscountID { get; set; }
		string DiscountSequenceID { get; set; }
		string Type { get; set; }
		decimal? CuryDiscountableAmt { get; set; }
		decimal? DiscountableQty { get; set; }
		decimal? CuryDiscountAmt { get; set; }
		decimal? DiscountPct { get; set; }
		int? FreeItemID { get; set; }
		decimal? FreeItemQty { get; set; }
		bool? IsManual { get; set; }
		bool? IsOrigDocDiscount { get; set; }
		string ExtDiscCode { get; set; }
		string Description { get; set; }
	}
}