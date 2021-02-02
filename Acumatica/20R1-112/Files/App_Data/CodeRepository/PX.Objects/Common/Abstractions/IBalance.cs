namespace PX.Objects.Common
{
	public interface IBalance
	{
		/// <summary>
		/// Represents the document balance in base currency.
		/// </summary>
		decimal? DocBal { get; set; }
		/// <summary>
		/// Represents the document balance in document currency.
		/// </summary>
		decimal? CuryDocBal { get; set; }
	}
}