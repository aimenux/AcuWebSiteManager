namespace PX.Objects.PR
{
	public interface IPaycheckDetail
	{
		int? BranchID { get; set; }
		decimal? Amount { get; set; }
		int? ParentKeyID { set; }
	}
}
