using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.Data;
using System.Collections;
using PX.SM;
using PX.TM;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	public class EPAssignmentProcessor<Table> : PXGraph<EPAssignmentProcessor<Table>>
		where Table : class, PX.Data.EP.IAssign, IBqlTable, new()
	{
		private readonly PXGraph _Graph;

		public EPAssignmentProcessor(PXGraph graph)
			: this()
		{
			_Graph = graph;
		}

		public EPAssignmentProcessor()
		{
			_Graph = this;
		}

		public virtual bool Assign(Table item, int? assignmentMapID)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			if (assignmentMapID < 0)
				throw new ArgumentOutOfRangeException(nameof(assignmentMapID));

			EPAssignmentMap map =
			PXSelect<EPAssignmentMap, Where<EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>>
				.SelectWindowed(this, 0, 1, assignmentMapID);
			if (map == null) return false;

			ApproveInfo info;

			switch (map.MapType)
			{
				case EPMapType.Legacy:
					try
					{
						info = new EPAssignmentProcessHelper<Table>(_Graph).Assign(item, map, false).FirstOrDefault();
						if (info != null)
						{
							item.OwnerID = info.OwnerID;
							item.WorkgroupID = info.WorkgroupID;

							return true;
						}
					}
					catch
					{
						return false;
					}
					return false;

				case EPMapType.Assignment:
					try
					{
						info = new EPAssignmentHelper<Table>(_Graph).Assign(item, map, false, 0).FirstOrDefault();
						if (info != null)
						{
							item.OwnerID = info.OwnerID;
							item.WorkgroupID = info.WorkgroupID;

							return true;
						}
					}
					catch(Exception ex)
					{
						PXTrace.WriteInformation(ex);
						return false;
					}
					return false;

				case EPMapType.Approval:
					throw new ArgumentException(nameof(assignmentMapID));

				default:
					return false;
			}
		}

		public virtual IEnumerable<ApproveInfo> Approve(Table item, EPAssignmentMap map, int? currentStepSequence)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			if (map == null)
				throw new ArgumentOutOfRangeException(nameof(map));
			

			switch (map.MapType)
			{
				case EPMapType.Legacy:
					return new EPAssignmentProcessHelper<Table>(_Graph).Assign(item, map, false);

				case EPMapType.Assignment:
					throw new ArgumentException(Messages.AssignmentNotApprovalMap);

				case EPMapType.Approval:
					return new EPAssignmentHelper<Table>(_Graph).Assign(item, map, true, currentStepSequence);

				default:
					return null;
			}
		}
	}
}
