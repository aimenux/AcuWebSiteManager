using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;

namespace PX.Objects.AM
{
	/// <summary>
	/// The helper for approval automation that does not set the Hold value to true (while the base class of the helper sets the Hold value to true).
	/// </summary>
	/// <typeparam name="SourceAssign"></typeparam>
	/// <typeparam name="Approved"></typeparam>
	/// <typeparam name="Rejected"></typeparam>
	/// <typeparam name="Hold"></typeparam>
	/// <typeparam name="SetupApproval"></typeparam>
	public class AMECRApprovalAutomation<SourceAssign, Approved, Rejected, Hold, SetupApproval> 
		:  EPApprovalAutomation<SourceAssign, Approved, Rejected, Hold, SetupApproval>
		where SourceAssign : class, IAssign, IBqlTable, new()
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where Hold : class, IBqlField
		where SetupApproval : class, IBqlTable, new()
	{
		public AMECRApprovalAutomation(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
	
		}

		public AMECRApprovalAutomation(PXGraph graph)
			: base(graph)
		{
	
		}

		protected override void Hold_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
		}

        public override List<ApprovalMap> GetAssignedMaps(SourceAssign doc, PXCache cache)
        {
            PXResultset<SetupApproval> setups = PXSetup<SetupApproval>.SelectMultiBound(cache.Graph, new object[] { doc });

            int count = setups.Count;
            var list = new List<ApprovalMap>();
            for (int i = 0; i < count; i++)
            {
                SetupApproval setup = (SetupApproval)setups[i];
                IAssignedMap map = (IAssignedMap)setup;
                if (map.IsActive == true && map.AssignmentMapID != null)
                {
                    list.Add(new ApprovalMap(map.AssignmentMapID.Value, map.AssignmentNotificationID));
                }
            }
            return list;
        }
    }
}