using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using PX.Api;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.TM;

namespace PX.Objects.WZ
{
    public class WizardArticleMaint : PXGraph<WizardArticleMaint>
    {
        public PXSelect<WZTask> Tasks;
        public PXFilter<WZTaskAssign> CurrentTask;

        public PXSelect<WZSubTask, Where<WZSubTask.parentTaskID, Equal<Current<WZTask.taskID>>>> SubTasks;

        public PXSelectJoin<WZTaskPredecessorRelation,
                InnerJoin<WZTask, On<WZTask.taskID, Equal<WZTaskPredecessorRelation.predecessorID>>>,
                Where<WZTaskPredecessorRelation.taskID, Equal<Current<WZTask.taskID>>>> Predecessors;

        public PXSelectJoin<WZTaskSuccessorRelation,
                                InnerJoin<WZTask, On<WZTask.taskID, Equal<WZTaskSuccessorRelation.taskID>>>,
                                Where<WZTaskSuccessorRelation.predecessorID, Equal<Current<WZTask.taskID>>>> Successors;

        public PXCancel<WZTask> cancel;
        public PXAction<WZTask> startTask;
        public PXAction<WZTask> skip;
        public PXAction<WZTask> skipSubtask;
        public PXAction<WZTask> markAsComplete;
        public PXAction<WZTask> goToScreen;
        public PXAction<WZTask> assign;
        public PXAction<WZTask> viewPredecessorTask;
        public PXAction<WZTask> viewBlockedTask; 
        public PXAction<WZTask> viewSubTask;
        


        protected virtual void WZTask_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            WZTask row = e.Row as WZTask;
            if(row == null) return;

            if (this.Actions.Contains("CancelClose"))
            {
                this.Actions["CancelClose"].SetTooltip(Messages.BackToScenario);
            }

            PXUIFieldAttribute.SetEnabled<WZTask.details>(Tasks.Cache, row, false);
            PXUIFieldAttribute.SetEnabled<WZTask.assignedTo>(Tasks.Cache, row, false);
            bool canBeComplete = row.Status == WizardTaskStatusesAttribute._ACTIVE || row.Status == WizardTaskStatusesAttribute._OPEN;
            bool canBeReopened = row.Status == WizardTaskStatusesAttribute._COMPLETED;
            markAsComplete.SetEnabled(canBeComplete);
            skip.SetVisible(false);
            if (row.IsOptional == true)
            {
                skip.SetVisible(true);    
                skip.SetEnabled(!(row.Status == WizardTaskStatusesAttribute._SKIPPED || row.Status == WizardTaskStatusesAttribute._COMPLETED));
            }
            

            startTask.SetEnabled(row.Status == WizardTaskStatusesAttribute._OPEN);

            SubTasks.AllowUpdate = false;
            Predecessors.AllowUpdate = false;
            Successors.AllowUpdate = false;

            var predecessors = Predecessors.Select();
            var successors = Successors.Select();
            var subTasks = SubTasks.Select();

            if (predecessors.Count == 0)
            {
                Predecessors.Cache.AllowSelect = false;
            }
            if (successors.Count == 0)
            {
                Successors.Cache.AllowSelect = false;
            }
            if (subTasks.Count == 0)
            {
                SubTasks.Cache.AllowSelect = false;
            }
            goToScreen.SetVisible(row.ScreenID != null);
        }
        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Predecessor Name", Visible = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(Search<WZTask.taskID,
                                Where<WZTask.scenarioID, Equal<Current<WZTaskPredecessorRelation.scenarioID>>,
                                And<WZTask.taskID, NotEqual<Current<WZTask.taskID>>>>>), SubstituteKey = typeof(WZTask.name))]
        protected virtual void WZTaskPredecessorRelation_PredecessorID_CacheAttached(PXAdapter adapter)
        {
            
        }

        [PXUIField(DisplayName = "Go to Screen", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable GoToScreen(PXAdapter adapter)
        {
            WZTask task = PXSelect<WZTask, Where<WZTask.taskID, Equal<Current<WZTask.taskID>>>>.Select(this);

            if (task == null) return adapter.Get();

            if (task.ScreenID != null)
            {

                PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(task.ScreenID);
                if (task.ImportScenarioID != null)
                {
                    SYMappingActive importScenario =
                        PXSelect
                            <SYMappingActive, Where<SYMappingActive.mappingID, Equal<Required<SYMapping.mappingID>>>
                                >
                            .Select(this, task.ImportScenarioID);
                    if (importScenario != null)
                    {
                        SYImportProcessSingle importGraph = CreateInstance<SYImportProcessSingle>();

                        importGraph.MappingsSingle.Current = (SYMappingActive) importScenario;
                        throw new PXRedirectRequiredException(importGraph, true, task.Name);
                    }
                }
                else
                {
                    if (node.GraphType != null)
                    {
                        Type t = GraphHelper.GetType(node.GraphType);
                        if (t == typeof(FeaturesMaint))
                        {
                            FeaturesMaint featuresMaint = PXGraph.CreateInstance<FeaturesMaint>();
                            featuresMaint.Features.Current = featuresMaint.Features.Select();
                            featuresMaint.ActivationBehaviour.Cache.SetValueExt<AfterActivation.refresh>(featuresMaint.ActivationBehaviour.Current, false);

                            PXRedirectHelper.TryRedirect(featuresMaint, PXRedirectHelper.WindowMode.InlineWindow);
                        }
                    }
                    
                    throw new PXRedirectToUrlException(node.Url, PXBaseRedirectException.WindowMode.NewWindow, task.Name);
                }
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Mark as Completed", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable MarkAsComplete(PXAdapter adapter)
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();
            WZTask selectedTask = Tasks.Current;
            if (selectedTask != null && selectedTask.Status != WizardTaskStatusesAttribute._PENDING
                                     && selectedTask.Status != WizardTaskStatusesAttribute._DISABLED)
            {
                if (graph.CanTaskBeCompleted(selectedTask))
                {
                    selectedTask.Status = WizardTaskStatusesAttribute._COMPLETED;
                    graph.TaskInfo.Update(selectedTask);
                    graph.Save.Press();
                }
                else
                {
                    throw new PXException(Messages.CannotBeCompletedWileOpenTasks);
                }
            }

            Tasks.Cache.ClearQueryCacheObsolete();
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Skip")]
        [PXButton]
        public virtual IEnumerable Skip(PXAdapter adapter)
        {
            WZTask task = Tasks.Current;
            WZTaskEntry taskGraph = PXGraph.CreateInstance<WZTaskEntry>();

            if (task != null && (task.Status == WizardTaskStatusesAttribute._OPEN ||
                                task.Status == WizardTaskStatusesAttribute._PENDING ||
                                task.Status == WizardTaskStatusesAttribute._ACTIVE)
                             && task.IsOptional == true)
            {
                task.Status = WizardTaskStatusesAttribute._SKIPPED;
                taskGraph.TaskInfo.Update(task);
                taskGraph.Actions.PressSave();

                Tasks.Cache.Clear();
                Tasks.Cache.ClearQueryCacheObsolete();
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Skip")]
        [PXButton]
        public virtual IEnumerable SkipSubtask(PXAdapter adapter)
        {
            WZTask task = SubTasks.Current;
            WZTaskEntry taskGraph = PXGraph.CreateInstance<WZTaskEntry>();

            if (task != null && (task.Status == WizardTaskStatusesAttribute._OPEN ||
                                task.Status == WizardTaskStatusesAttribute._PENDING ||
                                task.Status == WizardTaskStatusesAttribute._ACTIVE)
                             && task.IsOptional == true)
            {
                task.Status = WizardTaskStatusesAttribute._SKIPPED;
                taskGraph.TaskInfo.Update(task);
                taskGraph.Actions.PressSave();

                Tasks.Cache.Clear();
                Tasks.Cache.ClearQueryCacheObsolete();
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Start Task")]
        [PXButton]
        public virtual IEnumerable StartTask(PXAdapter adapter)
        {
            WZTask task = Tasks.Current;
            WZTaskEntry taskGraph = PXGraph.CreateInstance<WZTaskEntry>();

            if (task != null && task.Status == WizardTaskStatusesAttribute._OPEN)
            {
                task.Status = WizardTaskStatusesAttribute._ACTIVE;
                taskGraph.TaskInfo.Update(task);
                taskGraph.Save.Press();

                Tasks.Cache.Clear();
                Tasks.Cache.ClearQueryCacheObsolete();
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = "Assign")]
        [PXButton]
        public virtual IEnumerable Assign(PXAdapter adapter)
        {
            if (CurrentTask.AskExt(true) == WebDialogResult.OK)
            {
                if (CurrentTask.Current.AssignedTo != null)
                {
                    WZTaskEntry taskGraph = PXGraph.CreateInstance<WZTaskEntry>();
                    WZTask task = Tasks.Current as WZTask;

                    if (task != null)
                    {
                        task.AssignedTo = CurrentTask.Current.AssignedTo;
                        taskGraph.TaskInfo.Update(task);
                        taskGraph.Actions.PressSave();
                    }
                }
                Tasks.Cache.ClearQueryCacheObsolete();
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = "View Blocked Task")]
        [PXButton]
        public virtual IEnumerable ViewBlockedTask(PXAdapter adapter)
        {
            WZTaskSuccessorRelation relation = Successors.Current;
            if (relation == null) return adapter.Get();

            WZTask task = PXSelect<WZTask, Where<WZTask.taskID, Equal<Required<WZTask.taskID>>>>.Select(this, relation.TaskID);

            if (task == null) return adapter.Get();

            PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(typeof(WizardArticleMaint));
            throw new PXRedirectToUrlException(node.Url + "?TaskID=" + task.TaskID, PXBaseRedirectException.WindowMode.InlineWindow, task.Name);
        }

        [PXUIField(DisplayName = "View Predecessor Task")]
        [PXButton]
        public virtual IEnumerable ViewPredecessorTask(PXAdapter adapter)
        {
            WZTaskPredecessorRelation relation = Predecessors.Current;
            if (relation == null) return adapter.Get();

            WZTask task = PXSelect<WZTask, Where<WZTask.taskID, Equal<Required<WZTask.taskID>>>>.Select(this, relation.PredecessorID);

            if (task == null) return adapter.Get();

            PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(typeof(WizardArticleMaint));
            throw new PXRedirectToUrlException(node.Url + "?TaskID=" + task.TaskID, PXBaseRedirectException.WindowMode.InlineWindow, task.Name);
        }

        [PXUIField(DisplayName = "View Subtask")]
        [PXButton]
        public virtual IEnumerable ViewSubTask(PXAdapter adapter)
        {
            WZTask task = SubTasks.Current;

            if (task == null) return adapter.Get();

            PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(typeof(WizardArticleMaint));
            throw new PXRedirectToUrlException(node.Url + "?TaskID=" + task.TaskID, PXBaseRedirectException.WindowMode.InlineWindow, task.Name);
        }
    }

    [Serializable]
    public class WZSubTask : WZTask
    {
        #region Name
        public new abstract class name : PX.Data.BQL.BqlString.Field<name> { }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public override String Name { get; set; }
        #endregion

        #region ParentTaskID

        public new abstract class parentTaskID : PX.Data.BQL.BqlGuid.Field<parentTaskID> { }
        [PXDBGuid]
        [PXUIField(DisplayName = "Parent Task", Visibility = PXUIVisibility.SelectorVisible)]
        public override Guid? ParentTaskID { get; set; }

        #endregion

        #region IsOptional

        public new abstract class isOptional : PX.Data.BQL.BqlBool.Field<isOptional> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Optional", Visibility = PXUIVisibility.SelectorVisible)]
        public override bool? IsOptional { get; set; }

        #endregion

        #region AssignedTo
        public new abstract class assignedTo : PX.Data.BQL.BqlGuid.Field<assignedTo> { }


        [PXDBGuid]
        [PXUIField(DisplayName = "Assigned To", Enabled = false)]
        [PXOwnerSelector]
        public override Guid? AssignedTo { get; set; }
        
        #endregion

        #region Status
        public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

        [PXDBString(2, IsFixed = true)]
        [WizardTaskStatuses]
        [PXDefault(WizardTaskStatusesAttribute._PENDING)]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        public override String Status { get; set; }
        #endregion
    }
}
