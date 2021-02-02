using PX.Data.EP;

namespace PX.Objects
{
	/// <summary>
	/// The interface that specifies that the document is approvable.
	/// </summary>
	public interface IApprovable : IAssign
	{
		bool? Approved
		{
			get; set;
		}

		bool? Rejected
		{
			get; set;
		}

		bool? DontApprove
		{
			get; set;
		}
	}
}