using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.EP
{
	/// <summary>
	/// The helper for approval automation that is used for processing the cases when it is necessary to switch between the Open and Reserved statuses without clearing the approvals.
	/// </summary>
	/// <typeparam name="SourceAssign"></typeparam>
	/// <typeparam name="Approved"></typeparam>
	/// <typeparam name="Rejected"></typeparam>
	/// <typeparam name="Hold"></typeparam>
	/// <typeparam name="SetupApproval"></typeparam>
	public class EPApprovalAutomationWithReservedDoc<SourceAssign, Approved, Rejected, Hold, SetupApproval> :
		EPApprovalAutomationWithoutHoldDefaulting<SourceAssign, Approved, Rejected, Hold, SetupApproval>
		where SourceAssign : class, IReserved, IApprovable, IAssign, IBqlTable, new()
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where Hold : class, IBqlField
		where SetupApproval : class, IAssignedMap, IBqlTable, new()
	{
		public EPApprovalAutomationWithReservedDoc(PXGraph graph, Delegate @delegate) : base(graph, @delegate)
		{
		}

		public EPApprovalAutomationWithReservedDoc(PXGraph graph) : base(graph)
		{
		}

		protected override void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			SourceAssign doc = e.Row as SourceAssign;
			SourceAssign olddoc = e.OldRow as SourceAssign;
			if (olddoc == null || doc == null)
				return;
			bool? approved = doc.Approved;
			bool? hold = doc.Hold;
			bool? oldHold = olddoc.Hold;

			if (oldHold != null)
			{
				if (hold == true)
				{
					if (oldHold == false && doc.Released != true)
					{
						this.ClearPendingApproval(doc);
						cache.SetDefaultExt<Approved>(doc);
						cache.SetDefaultExt<Rejected>(doc);
					}
				}
				else if (oldHold == true && approved != true)
				{
					List<ApprovalMap> maps = GetAssignedMaps(doc, cache);
					if (maps.Any())
					{
						Assign(doc, maps);
					}
					else
					{
						cache.SetValue<Approved>(doc, true);
						cache.SetValue<Rejected>(doc, false);
					}
				}
			}
		}
	}
}