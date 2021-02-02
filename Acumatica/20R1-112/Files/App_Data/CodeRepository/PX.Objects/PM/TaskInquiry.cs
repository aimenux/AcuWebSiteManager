using System.Collections;
using PX.Objects.CR;
using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.PM
{
    [DashboardType((int)DashboardTypeAttribute.Type.Task)]
	public class TaskInquiry : PXGraph<TaskInquiry>
    {
        #region Selects
        [PXViewName(Messages.Selection)]
		public PXFilter<TaskFilter>
            Filter;

        [PXViewName(Messages.PMTasks)]
        [PXFilterable]
        public PXSelectJoin<PMTask
			, LeftJoin<PMTaskTotal, On<PMTaskTotal.projectID, Equal<PMTask.projectID>, And<PMTaskTotal.taskID, Equal<PMTask.taskID>>>
				, LeftJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>
				>
			, Where<PMTask.approverID, Equal<Current<TaskFilter.approverID>>>
			> FilteredItems;

        #endregion

        #region Actions
		public PXCancel<TaskFilter> Cancel;

		public PXAction<TaskFilter> viewProject;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewProject(PXAdapter adapter)
        {
            if (FilteredItems.Current != null)
            {
				var graph = CreateInstance<ProjectAccountingService>();
				graph.NavigateToProjectScreen(FilteredItems.Current.ProjectID, PXRedirectHelper.WindowMode.NewWindow);
            }
            return adapter.Get();
        }

		public PXAction<TaskFilter> viewTask;
        [PXUIField(DisplayName ="", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewTask(PXAdapter adapter)
        {
            var graph = CreateInstance<ProjectTaskEntry>();
            graph.Task.Current = graph.Task.Search<PMTask.taskCD>(FilteredItems.Current.TaskCD, FilteredItems.Current.ProjectID);
            throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        #endregion

        #region Ctor
        public TaskInquiry()
        {
            FilteredItems.Cache.AllowInsert = false;
            FilteredItems.Cache.AllowUpdate = false;
            FilteredItems.Cache.AllowDelete = false;

            PXUIFieldAttribute.SetVisible<PMTask.rateTableID>(FilteredItems.Cache, null, false);
            PXUIFieldAttribute.SetVisible<PMTask.startDate>(FilteredItems.Cache, null, false);
			PXUIFieldAttribute.SetVisible<PMTask.locationID>(FilteredItems.Cache, null, false);
			PXUIFieldAttribute.SetVisible<PMTask.endDate>(FilteredItems.Cache, null, false);
            PXUIFieldAttribute.SetVisible<PMTask.defaultSubID>(FilteredItems.Cache, null, false);
            PXUIFieldAttribute.SetVisible<PMTask.visibleInAP>(FilteredItems.Cache, null, false);
            PXUIFieldAttribute.SetVisible<PMTask.visibleInAR>(FilteredItems.Cache, null, false);		
			PXUIFieldAttribute.SetVisible<PMTask.visibleInGL>(FilteredItems.Cache, null, false);
            PXUIFieldAttribute.SetVisible<PMTask.visibleInIN>(FilteredItems.Cache, null, false);
			PXUIFieldAttribute.SetVisible<PMTask.visibleInCA>(FilteredItems.Cache, null, false);
            PXUIFieldAttribute.SetVisible<PMTask.visibleInPO>(FilteredItems.Cache, null, false);
            PXUIFieldAttribute.SetVisible<PMTask.visibleInSO>(FilteredItems.Cache, null, false);

            PXUIFieldAttribute.SetDisplayName<PMProject.description>(Caches[typeof(PMProject)], Messages.PrjDescription);
        }
        #endregion

		#region DAC
		[Serializable]
		[PXHidden]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class TaskFilter : IBqlTable
		{
			#region ApproverID
			public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }
			protected Int32? _ApproverID;
			[PXDBInt]
			[PXSubordinateSelector]
			[PXUIField(DisplayName = "Task Activity Approver", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Int32? ApproverID
			{
				get
				{
					return this._ApproverID;
				}
				set
				{
					this._ApproverID = value;
				}
			}
			#endregion
		}
		#endregion
	}
}