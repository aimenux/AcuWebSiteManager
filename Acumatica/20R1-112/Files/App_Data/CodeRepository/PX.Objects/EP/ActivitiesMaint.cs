using System;
using PX.Common;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[Serializable]
	[PXHidden]
	public partial class ActivitySource : IBqlTable
	{
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXGuid]
		[PXNote]
		public Guid? NoteID { get; set; }
	}

	public class ActivitiesMaint : PXGraph<ActivitiesMaint>
	{
		[PXHidden] 
		public PXFilter<ActivitySource>
			Filter;

		[PXViewName(CR.Messages.Activities)]
		[PXFilterable]
		public CRActivityList<ActivitySource>
			Activities;

		public ActivitiesMaint()
		{
			Activities.EnshureTableData = false;
		}
	}
}
