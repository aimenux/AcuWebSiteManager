namespace PX.Objects.Common.Interfaces
{
	/// <summary>
	/// The interface that is used for documents that can have the Reserved status. The interface is used in the approval process.
	/// </summary>
	public interface IReserved
	{
		bool? Hold
		{
			get; set;
		}

		bool? Released
		{
			get; set;
		}
	}
}
