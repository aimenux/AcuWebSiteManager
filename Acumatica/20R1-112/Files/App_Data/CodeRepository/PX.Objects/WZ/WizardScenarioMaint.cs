using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.SM;
using PX.TM;
using PX.Objects.EP;

namespace PX.Objects.WZ
{
    public class WizardScenarioMaint : PXGraph<WizardScenarioMaint>
    {
        #region Views/Selects

        public PXSelect<WZScenario> Scenario;
        [PXFilterable]
        public PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Current<WZScenario.scenarioID>>, And<WZTask.status, NotEqual<WizardTaskStatusesAttribute.Disabled>>>, OrderBy<Asc<WZTask.order>>> Tasks;

        public PXFilter<WZTaskAssign> CurrentTask;

        public PXSelectJoin<WZTaskPredecessorRelation,
                        InnerJoin<WZTask, On<WZTask.taskID, Equal<WZTaskPredecessorRelation.predecessorID>>>,
                        Where<WZTaskPredecessorRelation.taskID, Equal<Current<WZTask.taskID>>>> Predecessors;

        #endregion

        #region Data Handlers

        protected virtual IEnumerable tasks()
        {
            int order = 0;
            int level = 0;
            foreach (WZTask task in PXSelectReadonly<WZTask, 
                                        Where<WZTask.scenarioID, Equal<Current<WZScenario.scenarioID>>,
                                            And<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>,
                                            And<WZTask.status, NotEqual<WizardTaskStatusesAttribute.Disabled>>>>, 
                                        OrderBy<Asc<WZTask.position>>>.Select(this, Guid.Empty))
            {
                task.Order = order;
                task.Offset = level;
                yield return task;
                order += 1;
                foreach (WZTask childTask in GetChildTasks(task.TaskID, level + 1))
                {
                    childTask.Order = order;
                    yield return childTask;
                    order += 1;
                }
            }
        }

        public IEnumerable GetChildTasks(Guid? taskID, int level)
        {
            foreach (WZTask task in PXSelectReadonly<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>,
                                                                    And<WZTask.status, NotEqual<WizardTaskStatusesAttribute.Disabled>>>,
                                        OrderBy<Asc<WZTask.position>>>.Select(this, taskID))
            {
                task.Offset = level;
                yield return task;
                foreach (WZTask childTask in GetChildTasks(task.TaskID, level + 1))
                {
                    yield return childTask;
                }
            }
        }

        #endregion

        #region Actions

        public PXSave<WZScenario> save;
        public PXCancel<WZScenario> cancel;

        public PXAction<WZScenario> startTask;
        public PXAction<WZScenario> viewTask;
        public PXAction<WZScenario> markAsCompleted;
        public PXAction<WZScenario> assign;
        
        public PXMenuAction<WZScenario> Action;

        public PXAction<WZScenario> reopen;
        public PXAction<WZScenario> skip;
        public PXAction<WZScenario> completeScenario;

        [PXUIField(DisplayName = "Start Task")]
        [PXButton]
        public virtual IEnumerable StartTask(PXAdapter adapter)
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();
            WZTask selectedTask = Tasks.Current;
            if (selectedTask != null && selectedTask.Status == WizardTaskStatusesAttribute._ACTIVE)
            {
                viewTask.Press();
                return adapter.Get();
            }
            if (selectedTask != null && selectedTask.Status == WizardTaskStatusesAttribute._OPEN)
            {
                selectedTask.Status = WizardTaskStatusesAttribute._ACTIVE;
                graph.TaskInfo.Update(selectedTask);
                graph.Save.Press();

                viewTask.Press();
            }

            Tasks.Cache.ClearQueryCacheObsolete();
            Scenario.Cache.ClearQueryCacheObsolete();
            Scenario.Cache.Clear();
            Scenario.View.RequestRefresh();
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Reopen")]
        [PXButton]
        public virtual IEnumerable Reopen(PXAdapter adapter)
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();
            WZTask selectedTask = Tasks.Current;
            if (selectedTask != null && (selectedTask.Status == WizardTaskStatusesAttribute._COMPLETED ||
                                         selectedTask.Status == WizardTaskStatusesAttribute._SKIPPED))
            {
                selectedTask.Status = WizardTaskStatusesAttribute._OPEN;
                graph.TaskInfo.Update(selectedTask);
                graph.Save.Press();
            }

            Tasks.Cache.ClearQueryCacheObsolete();
            Scenario.Cache.ClearQueryCacheObsolete();
            Scenario.Cache.Clear();
            Scenario.View.RequestRefresh();
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Skip")]
        [PXButton]
        public virtual IEnumerable Skip(PXAdapter adapter) 
        {
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();
            WZTask selectedTask = Tasks.Current;
            if (selectedTask != null && selectedTask.IsOptional == true)
            {
                selectedTask.Status = WizardTaskStatusesAttribute._SKIPPED;
                graph.TaskInfo.Update(selectedTask);
                graph.Save.Press();
            }

            Tasks.Cache.ClearQueryCacheObsolete();
            Scenario.Cache.ClearQueryCacheObsolete();
            Scenario.Cache.Clear();
            Scenario.View.RequestRefresh();
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Assign")]
        [PXButton]
        public virtual IEnumerable Assign(PXAdapter adapter)
        {
            if (CurrentTask.AskExtRequired())
            {
                if (CurrentTask.Current.AssignedTo != null)
                {
                    WZTaskEntry taskGraph = PXGraph.CreateInstance<WZTaskEntry>();
                    WZTask task = PXSelect<WZTask, Where<WZTask.taskID, Equal<Required<WZTask.taskID>>>>.Select(this,
                        CurrentTask.Current.TaskID);

                    if (task != null)
                    {
                        WZTask taskCopy = (WZTask)taskGraph.TaskInfo.Cache.CreateCopy(task);
                        taskCopy.AssignedTo = CurrentTask.Current.AssignedTo;
                        taskGraph.TaskInfo.Update(taskCopy);

                        taskGraph.TaskInfo.Current = task;
                        taskGraph.Scenario.Current = Scenario.Current;
                        if (CurrentTask.Current.OverrideAssignee == true)
                        {
                            foreach (WZTask subTask in taskGraph.Childs.Select())
                            {
                                WZTask subTaskCopy = (WZTask)taskGraph.TaskInfo.Cache.CreateCopy(subTask);
                                subTaskCopy.AssignedTo = CurrentTask.Current.AssignedTo;
                                taskGraph.TaskInfo.Update(subTaskCopy);
                            }
                        }
                        taskGraph.Save.Press();
                        this.CurrentTask.Cache.IsDirty = false;
                    }
                }
                Tasks.Cache.ClearQueryCacheObsolete();
                CurrentTask.Cache.SetDefaultExt<WZTaskAssign.overrideAssignee>(CurrentTask.Current);
                CurrentTask.Cache.ClearQueryCacheObsolete();
                CurrentTask.View.RequestRefresh();
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = "View Task")]
        [PXButton]
        public virtual IEnumerable ViewTask(PXAdapter adapter)
        {
            WZTask task = PXSelect<WZTask, Where<WZTask.taskID, Equal<Current<WZTask.taskID>>>>.Select(this);

            if (task == null) return adapter.Get();

            PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(typeof(WizardArticleMaint));
            if (node == null)
                throw new PXException(Messages.NoAccessRightsToWizardArticle);
            throw new PXRedirectToUrlException(node.Url+"?TaskID="+task.TaskID, PXBaseRedirectException.WindowMode.InlineWindow, task.Name);
        }
        #endregion

        #region Event Handlers        

        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Predecessor Name", Visible = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault()]
        [PXParent(typeof(Select<WZTask, Where<WZTask.taskID, Equal<Current<WZTaskPredecessorRelation.predecessorID>>>>))]
        [PXSelector(typeof(Search<WZTask.taskID,
                                Where<WZTask.scenarioID, Equal<Current<WZTaskPredecessorRelation.scenarioID>>,
                                And<WZTask.taskID, NotEqual<Current<WZTask.taskID>>>>>), SubstituteKey = typeof(WZTask.name))]
        protected virtual void WZTaskPredecessorRelation_PredecessorID_CacheAttached(PXCache sender)
        {
        }
        #endregion

    }

    [Serializable]
	[PXHidden]
	public partial class WZTaskAssign : IBqlTable
    {
        #region ScenarioID
        public abstract class scenarioID : PX.Data.BQL.BqlGuid.Field<scenarioID> { }

        [PXGuid(IsKey = true)]
        [PXDefault(typeof(WZScenario.scenarioID))]
        public virtual Guid? ScenarioID { get; set; }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlGuid.Field<taskID> { }
        [PXGuid(IsKey = true)]
        [PXDefault(typeof(WZTask.taskID))]
        public virtual Guid? TaskID { get; set; }
        #endregion
        #region OverrideAssignee
        public abstract class overrideAssignee : PX.Data.BQL.BqlBool.Field<overrideAssignee> { }
        [PXBool()]
        [PXUIField(DisplayName = "Override Subtasks", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? OverrideAssignee { get; set; }
        #endregion
        #region AssignedTo
        public abstract class assignedTo : PX.Data.BQL.BqlGuid.Field<assignedTo> { }
        protected Guid? _AssignedTo;
        [PXGuid]
        [PXUIField(DisplayName = "Assigned To")]
        [PXSelector(typeof(Search2<Users.pKID,
                                LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>>,
                                Where<Users.isHidden, Equal<False>,
                                And<Users.isApproved, Equal<True>,
                                And<Users.guest, NotEqual<True>>>>>),
                                new Type[] {
                                    typeof(Users.username),
                                    typeof(Users.displayName),
                                    typeof(Users.fullName),
                                    typeof(Users.state),
                                    typeof(EPEmployee.acctCD),
                                    typeof(EPEmployee.acctName)
                                }
                                , DescriptionField = typeof(Users.comment), SubstituteKey = typeof(Users.username), DirtyRead = true)]
        public virtual Guid? AssignedTo { get; set; }
        #endregion
    }
}
