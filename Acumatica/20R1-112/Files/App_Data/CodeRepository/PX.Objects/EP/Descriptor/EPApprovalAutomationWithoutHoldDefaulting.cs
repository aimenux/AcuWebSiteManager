using System;
using PX.Data;
using PX.Data.EP;

namespace PX.Objects.EP
{
	/// <summary>
	/// The helper for approval automation that does not set the Hold value to true (while the base class of the helper sets the Hold value to true).
	/// </summary>
	/// <typeparam name="SourceAssign"></typeparam>
	/// <typeparam name="Approved"></typeparam>
	/// <typeparam name="Rejected"></typeparam>
	/// <typeparam name="Hold"></typeparam>
	/// <typeparam name="SetupApproval"></typeparam>
	public class EPApprovalAutomationWithoutHoldDefaulting<SourceAssign, Approved, Rejected, Hold, SetupApproval> 
		:  EPApprovalAutomation<SourceAssign, Approved, Rejected, Hold, SetupApproval>
		where SourceAssign : class, IAssign, IBqlTable, new()
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where Hold : class, IBqlField
		where SetupApproval : class, IAssignedMap, IBqlTable, new()
	{
		public EPApprovalAutomationWithoutHoldDefaulting(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
	
		}

		public EPApprovalAutomationWithoutHoldDefaulting(PXGraph graph)
			: base(graph)
		{
	
		}

		protected override void Hold_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
		}
	}
}