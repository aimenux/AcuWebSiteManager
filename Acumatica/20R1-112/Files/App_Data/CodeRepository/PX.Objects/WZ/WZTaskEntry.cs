using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.SM;
using PX.Translation;

namespace PX.Objects.WZ
{
    public class WZTaskEntry : PXGraph<WZTaskEntry>
    {
        #region Views/Selects
        public PXSelect<WZScenario> Scenario;

        //[PXVirtualDAC]
        public PXSelect<WZTaskTreeItem, Where<WZTaskTreeItem.taskID, IsNotNull, And<WZTaskTreeItem.parentTaskID, Equal<Argument<Guid?>>>>,
                                OrderBy<Asc<WZTaskTreeItem.position>>> TasksTreeItems;

		[PXCopyPasteHiddenFields(typeof(WZTask.details))]
        public PXSelect<WZTask, Where<WZTask.taskID, Equal<Current<WZTaskTreeItem.taskID>>>> TaskInfo;

        public PXSelectJoin<WZTaskPredecessorRelation,
                                InnerJoin<WZTask, On<WZTask.taskID, Equal<WZTaskPredecessorRelation.predecessorID>>>,
                                Where<WZTaskPredecessorRelation.taskID, Equal<Current<WZTask.taskID>>,
                                And<WZTaskPredecessorRelation.scenarioID, Equal<Current<WZScenario.scenarioID>>>>> Predecessors;

        public PXSelectJoin<WZTaskSuccessorRelation,
                                InnerJoin<WZTask, On<WZTask.taskID, Equal<WZTaskSuccessorRelation.taskID>>>,
                                Where<WZTaskSuccessorRelation.predecessorID, Equal<Current<WZTask.taskID>>,
                                And<WZTaskSuccessorRelation.scenarioID, Equal<Current<WZScenario.scenarioID>>>>> Successors;

        public PXSelect<WZTask> Childs;
        public PXSelectOrderBy<WZTaskFeature, OrderBy<Asc<WZTaskFeature.order>>> Features;

        public PXSelectSiteMapTree<False, False, False, False, False> SiteMap;

        #endregion

        private Dictionary<string, string> FeaturesNames = new Dictionary<string, string>();
        private Dictionary<string, string> FeatureDependsOn = new Dictionary<string, string>();
        private Dictionary<string, int> FeatureOffset = new Dictionary<string, int>();
        private Dictionary<string, int> FeatureOrder = new Dictionary<string, int>();

        #region Ctor
        public WZTaskEntry()
        {
            FeaturesSet featureDac = new FeaturesSet(); ;
            Type featureType = featureDac.GetType();
            PXLicense license = PXLicenseHelper.License;
            List<string> topFeatures = new List<string>();
            Dictionary<string, List<string>> subfeatures = new Dictionary<string, List<string>>();

            PXCache cache = new PXCache<FeaturesSet>(this);
           
            foreach (var featureInfo in cache.Fields.SelectMany(f => cache.GetAttributes(cache.Current, f).Where(atr => atr is FeatureAttribute), (f, atr) => new { f, atr }))
            {
                PXFieldState fs = cache.GetStateExt(null, featureInfo.f) as PXFieldState; 
                FeatureAttribute attribute = featureInfo.atr as FeatureAttribute;
               
                if (fs == null || fs.Visible == false) continue;
                
                if (attribute != null && attribute.Top)
                {
                    topFeatures.Add(fs.Name);
                }

                if (attribute != null && attribute.Parent != null)
                {
                    Type parentFeatureType = attribute.Parent;
                    string parentFeatureName = this.Caches[featureType].GetField(parentFeatureType);
                    PXFieldState pfs = this.Caches[featureType].GetStateExt(null, parentFeatureName) as PXFieldState;

                    if (attribute.Top != true)
                    {
                        if (!subfeatures.ContainsKey(pfs.Name))
                            subfeatures.Add(pfs.Name, new List<string>());
                        subfeatures[pfs.Name].Add(fs.Name);
                    }
                    if (license.Licensed  && !PXAccess.BypassLicense )
                    {
                        if (license.Features.Contains(featureType + "+" + pfs.Name))
                        {
                            FeatureDependsOn.Add(fs.Name, pfs.Name);
                        }
                    }
                    else
                    {
                        FeatureDependsOn.Add(fs.Name, pfs.Name);
                    }

                }
                if (license.Licensed && !PXAccess.BypassLicense )
                {
                    if (license.Features.Contains(featureType + "+" + fs.Name))
                    {
                        FeaturesNames.Add(fs.Name, fs.DisplayName);
                    }
                }
                else
                {
                    FeaturesNames.Add(fs.Name, fs.DisplayName);
                }
            }
            
            List<string> rootFeatures = new List<string>();

            foreach (string featureID in FeaturesNames.Keys)
            {
                if(!FeatureDependsOn.ContainsKey(featureID))
                    rootFeatures.Add(featureID);
            }
            foreach (string topFeature in topFeatures)
            {
                if(!rootFeatures.Contains(topFeature))
                    rootFeatures.Add(topFeature);
            }
            FillFeatureHierarchy(0, 0, rootFeatures, subfeatures);
            TaskInfo.Cache.AllowInsert = false;
            deleteTask.SetEnabled(false);
            addTask.SetEnabled(false);
            up.SetEnabled(false);
            down.SetEnabled(false);
            left.SetEnabled(false);
            right.SetEnabled(false);
        }
        #endregion

        #region Cache Attached

        [PXDBGuid(IsKey = true)]
        [PXSelector(typeof(WZScenario.scenarioID), DescriptionField= typeof(WZScenario.name))]
        [PXUIField(DisplayName = "Scenario Name", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void WZScenario_ScenarioID_CacheAttached(PXCache sender)
        {

        }

        #endregion

        #region Data Handlers

        protected virtual IEnumerable tasksTreeItems([PXDBGuid] Guid? taskID)
        {
            List<WZTaskTreeItem> list = new List<WZTaskTreeItem>();

            if (!taskID.HasValue)
            {
                taskID = Guid.Empty;
            }
            IEnumerable resultSet;

            resultSet = PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Current<WZScenario.scenarioID>>,
                And<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>>>>.Select(this, taskID);

            foreach (PXResult<WZTask> record in resultSet)
            {
                WZTask task = record;
                WZTaskTreeItem treeTaskItem = new WZTaskTreeItem
                {
                    ScenarioID = task.ScenarioID,
                    TaskID = task.TaskID,
                    ParentTaskID = task.ParentTaskID,
                    Position = task.Position,
                    Name = task.Name
                };

                WZTask dbTask = PXSelectReadonly<WZTask, Where<WZTask.scenarioID, Equal<Current<WZScenario.scenarioID>>,
                    And<WZTask.taskID, Equal<Required<WZTask.taskID>>>>>.Select(this, task.TaskID);

                if(dbTask == null)
                {
                    TasksTreeItems.Cache.ActiveRow = treeTaskItem;
                }

                list.Add(treeTaskItem);
            }
            Childs.View.RequestRefresh();
            return list;
        }

        protected virtual IEnumerable childs()
        {
            List<WZTask> list = new List<WZTask>();
            if (TaskInfo.Current != null)
                list = GetChildTasks(TaskInfo.Current.TaskID);
            return list;
        }

        protected virtual IEnumerable features()
        {
            List<WZTaskFeature> list = new List<WZTaskFeature>();

            if (TasksTreeItems.Current == null || TasksTreeItems.Current.TaskID == Guid.Empty || TasksTreeItems.Current.TaskID == null) return list;

            bool featureCacheIsDirty = false;

            foreach (string featureID in FeaturesNames.Keys)
            {
                
                WZTaskFeature taskFeature = PXSelect<WZTaskFeature,
                    Where<WZTaskFeature.taskID, Equal<Required<WZTaskFeature.taskID>>,
                        And<WZTaskFeature.feature, Equal<Required<WZTaskFeature.feature>>>>>
                    .Select(this, TasksTreeItems.Current.TaskID, featureID);

                if (taskFeature == null)
                {
                    taskFeature = new WZTaskFeature()
                    {
                        ScenarioID = TasksTreeItems.Current.ScenarioID,
                        TaskID = TasksTreeItems.Current.TaskID,
                        Feature = featureID,
                        DisplayName = FeaturesNames[featureID],
                        Required = false,
                        Offset = FeatureOffset[featureID],
                        Order = FeatureOrder[featureID]
                    };
                    Features.Insert(taskFeature);
                    if (Features.Cache.GetStatus(taskFeature) == PXEntryStatus.Updated)
                    {
                        featureCacheIsDirty = true;
                    }
                    list.Add(taskFeature);
                }
                else
                {
                    taskFeature.Offset = FeatureOffset[featureID];
                    taskFeature.Order = FeatureOrder[featureID];
                    if (Features.Cache.GetStatus(taskFeature) != PXEntryStatus.Inserted &&
                        Features.Cache.GetStatus(taskFeature) != PXEntryStatus.Updated &&
                        Features.Cache.GetStatus(taskFeature) != PXEntryStatus.Notchanged)
                    {
                        taskFeature.Required = true;
                        taskFeature.DisplayName = FeaturesNames[featureID];
                    }
                    if (Features.Cache.GetStatus(taskFeature) == PXEntryStatus.Updated)
                    {
                        taskFeature.DisplayName = FeaturesNames[featureID];
                        featureCacheIsDirty = true;
                    }
                    if (Features.Cache.GetStatus(taskFeature) == PXEntryStatus.Inserted && taskFeature.Required == true)
                    {
                        taskFeature.DisplayName = FeaturesNames[featureID];
                        featureCacheIsDirty = true;
                    }
                    if (Features.Cache.GetStatus(taskFeature) == PXEntryStatus.Notchanged)
                    {
                        WZTaskFeature featureFromDB = PXSelectReadonly
                            <WZTaskFeature, Where<WZTaskFeature.taskID, Equal<Required<WZTaskFeature.taskID>>,
                                And<WZTaskFeature.feature, Equal<Required<WZTaskFeature.feature>>>>>.Select(this,
                                    taskFeature.TaskID, taskFeature.Feature);
                        if (featureFromDB != null)
                        {
                            taskFeature.Required = true;
                            taskFeature.DisplayName = FeaturesNames[featureID];
                        }
                    }
                    list.Add(taskFeature);
                }
            }
            Features.Cache.IsDirty = featureCacheIsDirty;
            return list;
        }

        #endregion

        #region Event Handlers
        #region WZScenario

        protected virtual void WZScenario_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            activateScenario.SetEnabled(false);
            completeScenario.SetEnabled(false);
            suspendScenario.SetEnabled(false);
            e.Cancel = true;
        }

        protected virtual void WZScenario_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            WZScenario row = e.Row as WZScenario;
            
            if (row != null)
            {
                deleteTask.SetEnabled(true);
                addTask.SetEnabled(true);
                up.SetEnabled(true);
                down.SetEnabled(true);
                left.SetEnabled(true);
                right.SetEnabled(true);

                bool activationAvailable = row.Status != WizardScenarioStatusesAttribute._ACTIVE;
                bool completionAvailable = row.Status == WizardScenarioStatusesAttribute._ACTIVE;
                bool suspendAvailable = row.Status != WizardScenarioStatusesAttribute._SUSPEND;
                bool scenarioCanBeEdit = row.Status != WizardScenarioStatusesAttribute._ACTIVE &&
                                         row.Status != WizardScenarioStatusesAttribute._SUSPEND;

                Successors.Cache.AllowUpdate =
                Successors.Cache.AllowDelete =
                Successors.Cache.AllowInsert = false;

                Predecessors.Cache.AllowUpdate = 
                Features.Cache.AllowUpdate = scenarioCanBeEdit;

                Predecessors.Cache.AllowDelete =
                Features.Cache.AllowDelete = scenarioCanBeEdit;

                Predecessors.Cache.AllowInsert =
                Features.Cache.AllowInsert = scenarioCanBeEdit;

                activateScenario.SetEnabled(activationAvailable);
                completeScenario.SetEnabled(completionAvailable);
                suspendScenario.SetEnabled(suspendAvailable);

                var tasks = PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(this, row.ScenarioID);
                if (tasks != null)
                {
                    bool emptyScenario = tasks.Count == 0;
                    PXUIFieldAttribute.SetEnabled(TaskInfo.Cache, null, !emptyScenario);
                }
            }
        }

        #endregion 
        #region WZTask
        protected virtual void WZTask_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            WZTask row = e.Row as WZTask;
            WZScenario scenario = Scenario.Current;
            if (row != null && scenario != null)
            {
                int maxPosition = 0;
                row.Position = maxPosition;

                if (TasksTreeItems.Current != null)
                {
                    IEnumerable list = tasksTreeItems(TasksTreeItems.Current.TaskID);
                    foreach (WZTaskTreeItem task in list)
                    {
                        if (task.Position.Value > maxPosition)
                        {
                            maxPosition = task.Position.Value;
                        }

                        row.Position = maxPosition + 1;
                    }
                }
            }
        }

        protected virtual void WZTask_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            WZTask row = (WZTask) e.Row;
            if (row == null) return;
        }

        protected virtual void WZTask_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            WZTask row = (WZTask)e.Row;
            if (row == null) return;
        }

        protected virtual void WZTask_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            WZTask row = (WZTask)e.Row;
            if (row == null) return;
        }

        protected virtual void WZTask_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            WZTask row = e.Row as WZTask;
            if (row != null && Scenario.Current != null)
            {
                Scenario.Cache.AllowUpdate = true;
                TaskInfo.Current.TaskID = row.TaskID;
                if (row.ParentTaskID == Guid.Empty)
                {
                    row.Type = WizardTaskTypesAttribute._ARTICLE;
                }

            }
            TasksTreeItems.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.View.RequestRefresh();
        }

        protected virtual void WZTask_TaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            WZTask row = e.Row as WZTask;
            if (row == null) return;
            row.ScenarioID = Scenario.Current.ScenarioID;
            row.TaskID = Guid.NewGuid();
            row.ParentTaskID = Guid.Empty;
            if (TasksTreeItems.Current != null && TasksTreeItems.Current.TaskID != Guid.Empty)
            {
                row.ParentTaskID = TasksTreeItems.Current.TaskID;
            }
        }

        protected virtual void WZTask_Name_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            WZTask row = e.Row as WZTask;
            if (row == null) return;
            row.Name = Messages.NewTask;
        }

        protected virtual void WZTask_Status_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            WZTask row = e.Row as WZTask;
            if (row == null) return;

            Debug.Print("IN EVENT Task: {0} Status: {1}", row.Name, row.Status);

            if (row.Status == WizardTaskStatusesAttribute._COMPLETED)
            {
                row.CompletedBy = Accessinfo.UserID;
                row.CompletedDate = PXTimeZoneInfo.Now;
            }
            if (row.Status == WizardTaskStatusesAttribute._ACTIVE)
            {
                row.StartedDate = PXTimeZoneInfo.Now;
                row.CompletedDate = null;
            }

            if (row.Status == WizardTaskStatusesAttribute._SKIPPED)
            {
                row.CompletedBy = Accessinfo.UserID;
                row.CompletedDate = PXTimeZoneInfo.Now;
            }

            var tasksToOpen = GetTasksToOpen(row);
            var tasksToSkip = GetTasksToSkip(row);
            var tasksToDisable = GetTasksToDisable(row);
            var tasksToEnable = GetTasksToEnable(row);

            foreach (WZTask task in tasksToOpen.Values)
            {
                Debug.Print("OPEN: {0} Status: {1}", task.Name, task.Status);
                TaskInfo.Update(task);
            }
            foreach (WZTask task in tasksToSkip.Values)
            {
                Debug.Print("SKIP ### Task: {0} Status: {1}", task.Name, task.Status);
                TaskInfo.Update(task);
            }
            foreach (WZTask task in tasksToDisable.Values)
            {
                Debug.Print("DISABLE ### Task: {0} Status: {1}", task.Name, task.Status);
                TaskInfo.Update(task);
            }
            foreach (WZTask task in tasksToEnable.Values)
            {
                Debug.Print("ENABLE ### Task: {0} Status: {1}", task.Name, task.Status);
                TaskInfo.Update(task);
            }

            if (ScenarioTasksCompleted(row.ScenarioID))
            {
                Debug.Print("Complete scenario!!!");
                if (Scenario.Current == null)
                {
                    WZScenario scenario =
                        PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>
                            .Select(this, row.ScenarioID);
                    Scenario.Current = scenario;
                }
                this.completeScenarioWithoutRefresh.Press();
            }
            
        }

        public bool ScenarioTasksCompleted(Guid? scenarioID)
        {
            var openedTasks = PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                And<Where<WZTask.status, Equal<WizardTaskStatusesAttribute.Open>,
                    Or<WZTask.status, Equal<WizardTaskStatusesAttribute.Pending>,
                    Or<WZTask.status, Equal<WizardTaskStatusesAttribute.Active>>>>>>>.Select(this, scenarioID);
            if (openedTasks == null) return true;

            if (openedTasks.Count > 0) 
                return false;
            return true;
        }

        public bool CanTaskBeCompleted(WZTask selectedTask)
        {
            foreach (WZTask childTask in PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                                    And<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>>>>.
                                                    Select(this, selectedTask.ScenarioID, selectedTask.TaskID))
            {
                if (childTask.Status == WizardTaskStatusesAttribute._OPEN ||
                    childTask.Status == WizardTaskStatusesAttribute._ACTIVE)
                {
                    return false;
                }
                if (!CanTaskBeCompleted(childTask))
                {
                    return false;
                }
            }
            return true;
        }

        private Dictionary<Guid, WZTask> GetTasksToOpen(WZTask selectedTask)
        {
            Dictionary<Guid, WZTask> tasks = new Dictionary<Guid, WZTask>();

            #region Find all child tasks which can be activated

            if (selectedTask.Status == WizardTaskStatusesAttribute._OPEN)
            {
                foreach (WZTask childTask in PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                                And<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>>>>.
                                                Select(this, selectedTask.ScenarioID, selectedTask.TaskID))
                {

                    bool canBeOpened = true;

                    #region Find all tasks that can stop activation of found task (predecessors of childs)

                    foreach (PXResult<WZTaskPredecessorRelation, WZTask> predecessorResult in
                        PXSelectJoin<WZTaskPredecessorRelation,
                        InnerJoin<WZTask, On<WZTask.taskID, Equal<WZTaskPredecessorRelation.predecessorID>>>,
                        Where<WZTaskPredecessorRelation.taskID, Equal<Required<WZTask.taskID>>>>.
                        Select(this, childTask.TaskID))
                    {
                        WZTask predecessorTask = (WZTask)predecessorResult;
                        if (predecessorTask != null)
                        {
                            if (predecessorTask.Status != WizardTaskStatusesAttribute._COMPLETED
                                && predecessorTask.Status != WizardTaskStatusesAttribute._SKIPPED)
                            {
                                canBeOpened = false;
                                break;
                            }
                        }
                    }

                    #endregion

                    if (canBeOpened)
                    {
                        if (childTask.Status != WizardTaskStatusesAttribute._DISABLED &&
                            !tasks.ContainsKey((Guid) childTask.TaskID))
                        {
                            WZTask taskCopy = (WZTask)this.TaskInfo.Cache.CreateCopy(childTask);
                            taskCopy.Status = WizardTaskStatusesAttribute._OPEN;
                            tasks.Add((Guid)taskCopy.TaskID, taskCopy);
                        }
                    }
                }
            }

            #endregion
            #region Find all tasks which must be activated after selectedTask completion (successors)

            WZScenario scenario =
                PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(this,
                    selectedTask.ScenarioID);

            if (scenario != null && scenario.Status != WizardScenarioStatusesAttribute._COMPLETED && 
                    (selectedTask.Status == WizardTaskStatusesAttribute._COMPLETED || selectedTask.Status == WizardTaskStatusesAttribute._SKIPPED))
            {
                foreach (PXResult<WZTask, WZTaskSuccessorRelation> result in PXSelectJoin<WZTask,
                    LeftJoin<WZTaskSuccessorRelation, On<WZTaskSuccessorRelation.taskID, Equal<WZTask.taskID>>>,
                    Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                        And<WZTaskSuccessorRelation.predecessorID, Equal<Required<WZTask.taskID>>>>>.
                    Select(this, selectedTask.ScenarioID, selectedTask.TaskID))
                {
                    WZTask succesorTask = result;
                    WZTaskSuccessorRelation relation = result;

                    if (relation == null || relation.TaskID == null) continue;

                    bool canBeOpened = false;

                    #region Find all tasks that can stop activation of found task (predecessors of successor)

                    foreach (PXResult<WZTask, WZTaskPredecessorRelation> predecessorResult in PXSelectJoin<WZTask,
                        LeftJoin
                            <WZTaskPredecessorRelation,
                                On<WZTaskPredecessorRelation.predecessorID, Equal<WZTask.taskID>>>,
                        Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                            And<WZTaskPredecessorRelation.taskID, Equal<Required<WZTask.taskID>>>>>.
                        Select(this, selectedTask.ScenarioID, succesorTask.TaskID))
                    {
                        WZTask predecessorTask = (WZTask)predecessorResult;
                        if (predecessorTask != null)
                        {
                            if (predecessorTask.Status == WizardTaskStatusesAttribute._COMPLETED
                                || predecessorTask.Status == WizardTaskStatusesAttribute._SKIPPED
                                || predecessorTask.Status == WizardTaskStatusesAttribute._DISABLED)
                            {
                                canBeOpened = true;
                            }
                            else
                            {
                                canBeOpened = false;
                                break;
                            }
                        }
                    }

                    #endregion

                    if (canBeOpened)
                    {
                        if (succesorTask.Status == WizardTaskStatusesAttribute._PENDING)
                        {
                            WZTask taskCopy = (WZTask)this.TaskInfo.Cache.CreateCopy(succesorTask);
                            taskCopy.Status = WizardTaskStatusesAttribute._OPEN;
                            if (!tasks.ContainsKey((Guid)taskCopy.TaskID))
                            {
                                tasks.Add((Guid)taskCopy.TaskID, taskCopy);
                            }
                        }
                    }
                }
            }

            #endregion

            return tasks;
        }

        private Dictionary<Guid, WZTask> GetTasksToDisable(WZTask selectedTask)
        {
            Dictionary<Guid, WZTask> tasks = new Dictionary<Guid, WZTask>();

            #region

            if (selectedTask.Status == WizardTaskStatusesAttribute._DISABLED)
            {
                foreach (PXResult<WZTask> result in PXSelect<WZTask, 
                                                        Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>, 
                                                        And<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>>>>.
                                                        Select(this, selectedTask.ScenarioID, selectedTask.TaskID))
                {
                    WZTask childTask = result;

                    if (childTask == null || childTask.TaskID == null) continue;

                    WZTask taskCopy = (WZTask)this.TaskInfo.Cache.CreateCopy(childTask);
                    taskCopy.Status = WizardTaskStatusesAttribute._DISABLED;
                    if (!tasks.ContainsKey((Guid)taskCopy.TaskID))
                    {
                        tasks.Add((Guid)taskCopy.TaskID, taskCopy);
                    }
                }
            }

            #endregion

            return tasks;
        }

        private Dictionary<Guid, WZTask> GetTasksToEnable(WZTask selectedTask)
        {
            Dictionary<Guid, WZTask> tasks = new Dictionary<Guid, WZTask>();

            #region

            if (selectedTask.Status == WizardTaskStatusesAttribute._PENDING)
            {
                foreach (PXResult<WZTask> result in PXSelect<WZTask, 
                                                        Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>, 
                                                        And<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>>>>.
                                                        Select(this, selectedTask.ScenarioID, selectedTask.TaskID))
                {
                    WZTask childTask = result;


                    if (childTask == null || childTask.TaskID == null) continue;

                    if (childTask.Status == WizardTaskStatusesAttribute._DISABLED)
                    {
                        bool needEnable = false;
                        var childFeatures = PXSelectReadonly<WZTaskFeature, Where<WZTaskFeature.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                                                And<WZTaskFeature.taskID, Equal<Required<WZTask.taskID>>>>>
                                                                .Select(this, childTask.ScenarioID, childTask.TaskID);
                        if (childFeatures.Count == 0)
                            needEnable = true;
                        else
                        {
                            needEnable = true;
                            foreach (WZTaskFeature feature in childFeatures)
                            {
                                if (!PXAccess.FeatureInstalled(typeof(FeaturesSet).FullName + "+" + feature.Feature))
                                {
                                    needEnable = false;
                                    break;
                                }
                            }
                        }

                        if (needEnable)
                        {
                            WZTask taskCopy = (WZTask)this.TaskInfo.Cache.CreateCopy(childTask);
                            taskCopy.Status = WizardTaskStatusesAttribute._PENDING;
                            if (!tasks.ContainsKey((Guid)taskCopy.TaskID))
                            {
                                tasks.Add((Guid)taskCopy.TaskID, taskCopy);
                            }       
                        }
                    }
                    
                }
            }

            #endregion

            return tasks;
        }

        private Dictionary<Guid, WZTask> GetTasksToSkip(WZTask selectedTask)
        {
            Dictionary<Guid, WZTask> tasks = new Dictionary<Guid, WZTask>();

            #region Find all child tasks which need to be skipped

            if (selectedTask.Status == WizardTaskStatusesAttribute._SKIPPED)
            {
                foreach (WZTask childTask in PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                                And<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>>>>.
                                                Select(this, selectedTask.ScenarioID, selectedTask.TaskID))
                {
                    if (childTask.Status != WizardTaskStatusesAttribute._DISABLED &&
                        !tasks.ContainsKey((Guid)childTask.TaskID))
                    {
                        WZTask taskCopy = (WZTask)this.TaskInfo.Cache.CreateCopy(childTask);
                        if (!(bool)taskCopy.IsOptional)
                        {
                            taskCopy.Status = WizardTaskStatusesAttribute._SKIPPED;
                            tasks.Add((Guid)taskCopy.TaskID, taskCopy);
                        }
                        else if (taskCopy.Status != WizardTaskStatusesAttribute._COMPLETED)
                        {
                            taskCopy.Status = WizardTaskStatusesAttribute._SKIPPED;
                            tasks.Add((Guid)taskCopy.TaskID, taskCopy);
                        }
                    }
                }
            }

            #endregion

            return tasks;
        }

        protected virtual void WZTask_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            WZTask row = e.Row as WZTask;
            if(row == null) return;
            if (row.Type == WizardTaskTypesAttribute._ARTICLE)
            {
                row.ScreenID = null;
                row.ImportScenarioID = null;
            }
        }

        #endregion
        #region WZTaskFeature
        protected virtual void WZTaskFeature_DisplayName_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            WZTaskFeature row = e.Row as WZTaskFeature;

            if(row == null || row.Feature == null) return;
            row.DisplayName = FeaturesNames[row.Feature];
        }

        protected virtual void WZTaskFeature_Required_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            WZTaskFeature row = e.Row as WZTaskFeature;
            if (row == null || row.Feature == null) return;
            List<string> featuresToTurnOn = GetFeatureDependency(row.Feature);

            List<string> featuresToTurnOff = GetFeaturesDependsOn(row.Feature);

            if ((bool) row.Required)
            {
                WZTask task = PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                And<WZTask.taskID, Equal<Required<WZTask.taskID>>>>>.
                                Select(this, TasksTreeItems.Current.ScenarioID, TasksTreeItems.Current.TaskID);

                if (!PXAccess.FeatureInstalled(typeof (FeaturesSet).FullName + "+" + row.Feature))
                {
                    if (task != null)
                    {
                        task.Status = WizardTaskStatusesAttribute._DISABLED;
                        TaskInfo.Update(task);
                    }
                }

                foreach (string featureID in featuresToTurnOn)
                {
                    WZTaskFeature feature = PXSelect
                        <WZTaskFeature, Where<WZTaskFeature.scenarioID, Equal<Required<WZTask.scenarioID>>,
                            And<WZTaskFeature.taskID, Equal<Required<WZTask.taskID>>,
                            And<WZTaskFeature.feature, Equal<Required<WZTaskFeature.feature>>>>>>
                        .Select(this, TasksTreeItems.Current.ScenarioID, TasksTreeItems.Current.TaskID, featureID);

                    if (feature != null)
                    {
                        feature.Required = true;
                        if (!PXAccess.FeatureInstalled(typeof(FeaturesSet).FullName+"+"+featureID))
                        {
                            if (task != null)
                            {
                                task.Status = WizardTaskStatusesAttribute._DISABLED;
                                TaskInfo.Update(task);
                            }
                        }
                        Features.Update(feature);
                    }
                }

            }
            else
            {
                WZTask task = PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                And<WZTask.taskID, Equal<Required<WZTask.taskID>>>>>.
                                Select(this, TasksTreeItems.Current.ScenarioID, TasksTreeItems.Current.TaskID);

                if (!PXAccess.FeatureInstalled(typeof(FeaturesSet).FullName + "+" + row.Feature))
                {
                    if (task != null)
                    {
                        task.Status = WizardTaskStatusesAttribute._PENDING;
                        TaskInfo.Update(task);
                    }
                }

                foreach (string featureID in featuresToTurnOff)
                {
                    WZTaskFeature feature = PXSelect
                        <WZTaskFeature, Where<WZTaskFeature.scenarioID, Equal<Required<WZTask.scenarioID>>,
                            And<WZTaskFeature.taskID, Equal<Required<WZTask.taskID>>,
                            And<WZTaskFeature.feature, Equal<Required<WZTaskFeature.feature>>>>>>
                        .Select(this, TasksTreeItems.Current.ScenarioID, TasksTreeItems.Current.TaskID, featureID);

                    if (feature != null)
                    {
                        feature.Required = false;
                        if (!PXAccess.FeatureInstalled(featureID))
                        {
                            if (task != null && task.Status == WizardTaskStatusesAttribute._DISABLED)
                            {
                                task.Status = WizardTaskStatusesAttribute._PENDING;
                                TaskInfo.Update(task);
                            }
                        }
                        Features.Update(feature);
                    }
                }
            }
            Features.View.RequestRefresh();
        }

        protected virtual void WZTaskFeature_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            WZTaskFeature row = e.Row as WZTaskFeature;
            if (row == null || row.Required == true) return;

            Features.Delete(row);
            Features.View.RequestRefresh();
        }

        #endregion
        #region WZTaskRelation

        #region WZTaskSuccessorRelation
        protected virtual void WZTaskSuccessorRelation_TaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            WZTaskSuccessorRelation row = e.Row as WZTaskSuccessorRelation;
            if (row == null) return;
            row.PredecessorID = TasksTreeItems.Current.TaskID;
        }

        protected virtual void WZTaskSuccessorRelation_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            WZTaskSuccessorRelation row = e.Row as WZTaskSuccessorRelation;
            if (row == null || row.TaskID == null || row.PredecessorID == null)
                e.Cancel = true;
        }

        protected virtual void WZTaskSuccessorRelation_TaskID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            WZTaskSuccessorRelation row = e.Row as WZTaskSuccessorRelation;
            if (row != null && e.NewValue != null)
            {
                WZTaskRelation result = PXSelect
                    <WZTaskRelation, Where<WZTaskRelation.scenarioID, Equal<Current<WZTask.scenarioID>>,
                        And<WZTaskRelation.predecessorID, Equal<Current<WZTask.taskID>>,
                        And<WZTaskRelation.taskID, Equal<Required<WZTaskRelation.taskID>>>>>>
                    .Select(this, e.NewValue);
                if (result != null)
                {
                    e.Cancel = true;
                    throw new PXSetPropertyException(Messages.TaskAlreadyInSuccessorList, PXErrorLevel.Error);
                }

                foreach (WZTask nextTask in GetRequiredTasks(TasksTreeItems.Current.ScenarioID, TasksTreeItems.Current.TaskID))
                {
                    if (nextTask.TaskID == (Guid)e.NewValue)
                    {
                        e.Cancel = true;
                        throw new PXSetPropertyException(Messages.SuccessorCycleError, PXErrorLevel.Error);
                    }
                }
            }
        }
        #endregion
        #region WZTaskPredecessorRelation
        protected virtual void WZTaskPredecessorRelation_PredecessorID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            WZTaskPredecessorRelation row = e.Row as WZTaskPredecessorRelation;
            if (row != null && e.NewValue != null)
            {
                WZTaskRelation result = PXSelect
                    <WZTaskRelation, Where<WZTaskRelation.scenarioID, Equal<Current<WZTask.scenarioID>>,
                        And<WZTaskRelation.taskID, Equal<Current<WZTask.taskID>>,
                        And<WZTaskRelation.predecessorID, Equal<Required<WZTaskRelation.predecessorID>>>>>>
                    .Select(this, e.NewValue);
                if (result != null && row.PredecessorID != null)
                {
                    e.NewValue = row.PredecessorID;
                    sender.RaiseExceptionHandling<WZTaskPredecessorRelation.predecessorID>(e.Row, null, 
                            new PXSetPropertyException(Messages.TaskAlreadyInPredecessorList, PXErrorLevel.Error));
                }

                foreach (WZTask nextTask in GetNextTasks(TasksTreeItems.Current.ScenarioID, TasksTreeItems.Current.TaskID))
                {
                    if ( nextTask.TaskID == (Guid)e.NewValue)
                    {
                        e.Cancel = true;
                        throw new PXSetPropertyException(Messages.PredecessorCycleError, PXErrorLevel.Error);
                    }
                }

            }
        }

        protected virtual void WZTaskPredecessorRelation_TaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            WZTaskPredecessorRelation row = e.Row as WZTaskPredecessorRelation;
            if (row == null) return;
            row.TaskID = TasksTreeItems.Current.TaskID;
        }

        protected virtual void WZTaskPredecessorRelation_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            WZTaskPredecessorRelation row = e.Row as WZTaskPredecessorRelation;
            if (row == null || row.TaskID == null || row.PredecessorID == null)
                e.Cancel = true;
        }

        #endregion

        #endregion
        #endregion

        #region Overrides
        public override void Persist()
        {
            foreach (WZTaskFeature feature in Features.Cache.Inserted)
            {
                if (feature.Required == null || feature.Required == false)
                {
                    Features.Cache.SetStatus(feature, PXEntryStatus.InsertedDeleted);
                }
            }

            foreach (WZTaskFeature feature in Features.Cache.Updated)
            {
                if (feature.Required == null || feature.Required == false)
                {
                    Features.Cache.SetStatus(feature, PXEntryStatus.Deleted);
                }
            }
            base.Persist();
            PXSiteMap.Provider.Clear();
		
        }
        #endregion

        #region Actions
        public PXCancel<WZScenario> Cancel;
        public PXSave<WZScenario> Save;
        public PXAction<WZScenario> activateScenario;
        public PXAction<WZScenario> activateScenarioWithoutRefresh;
        public PXAction<WZScenario> prepareTasksForActivation;

        public PXAction<WZScenario> suspendScenario;
        public PXAction<WZScenario> completeScenario;
        public PXAction<WZScenario> completeScenarioWithoutRefresh;

        public PXAction<WZScenario> deleteTask;
        public PXAction<WZScenario> addTask;
        public PXAction<WZScenario> up;
        public PXAction<WZScenario> down;
        public PXAction<WZScenario> left;
        public PXAction<WZScenario> right;


        public virtual IEnumerable PrepareTasksForActivation(PXAdapter adapter)
        {
            WZScenario scenario = Scenario.Current;
            if (scenario != null && scenario.Status == WizardScenarioStatusesAttribute._COMPLETED)
            {
                foreach (WZTask task in PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Current<WZScenario.scenarioID>>>>.Select(this))
                {
                    WZTask taskCopy = (WZTask) this.TaskInfo.Cache.CreateCopy(task);
                    if (taskCopy.Status != WizardTaskStatusesAttribute._DISABLED)
                    {
                        taskCopy.Status = WizardTaskStatusesAttribute._PENDING;
                    }
                    taskCopy.StartedDate = null;
                    taskCopy.CompletedDate = null;
                    taskCopy.CompletedBy = null;
                    TaskInfo.Update(taskCopy);
                }
                this.Actions.PressSave();
            }
            return adapter.Get();
        }

        public virtual IEnumerable ActivateScenarioWithoutRefresh(PXAdapter adapter)
        {
            WZScenario scenario = Scenario.Current;
            if (scenario != null && scenario.Status != WizardScenarioStatusesAttribute._ACTIVE)
            {

                if (scenario.Status == WizardScenarioStatusesAttribute._SUSPEND)
                {
                    scenario.Status = WizardScenarioStatusesAttribute._ACTIVE;
                    Scenario.Update(scenario);
                    this.Actions.PressSave();
                    return adapter.Get();

                }

                scenario.Status = WizardScenarioStatusesAttribute._ACTIVE;
                if ((bool) scenario.Scheduled == false)
                {
                    scenario.ExecutionDate = Accessinfo.BusinessDate;
                }
                Scenario.Update(scenario);                

                foreach (PXResult<WZTask, WZTaskRelation> result in PXSelectJoin<WZTask,
                    LeftJoin<WZTaskRelation, On<WZTaskRelation.taskID, Equal<WZTask.taskID>>>,
                    Where<WZTask.scenarioID, Equal<Required<WZScenario.scenarioID>>>>.Select(this,
                        scenario.ScenarioID))
                {
                    WZTask task = (WZTask) result;

                    bool featureForTaskInstalled = true;

                    foreach (WZTaskFeature feature in PXSelectReadonly<WZTaskFeature,
                        Where<WZTaskFeature.taskID, Equal<Required<WZTask.taskID>>>>.Select(this, task.TaskID))
                    {
                        if (!PXAccess.FeatureInstalled(typeof (FeaturesSet).FullName + "+" + feature.Feature))
                        {
                            featureForTaskInstalled = false;
                            break;
                        }
                    }

                    WZTask taskCopy = (WZTask) this.TaskInfo.Cache.CreateCopy(task);
                    if (featureForTaskInstalled)
                    {
                        bool parentIsPending = false;
                        if (task.ParentTaskID != Guid.Empty)
                        {
                            foreach (
                                WZTask parentTask in
                                    PXSelect
                                        <WZTask, Where<WZTask.scenarioID, Equal<Required<WZScenario.scenarioID>>,
                                            And<WZTask.taskID, Equal<Required<WZTask.parentTaskID>>>>>.Select(this,
                                                scenario.ScenarioID, task.ParentTaskID))
                            {
                                if (parentTask.Status != WizardTaskStatusesAttribute._ACTIVE &&
                                    parentTask.Status != WizardTaskStatusesAttribute._OPEN)
                                {
                                    parentIsPending = true;
                                    break;
                                }

                                foreach (var parentResult in PXSelectJoin<WZTask,
                                    LeftJoin<WZTaskRelation, On<WZTaskRelation.predecessorID, Equal<WZTask.taskID>>>,
                                    Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                        And<WZTaskRelation.taskID, Equal<Required<WZTask.taskID>>>>>.
                                    Select(this, parentTask.ScenarioID, parentTask.TaskID))
                                {
                                    WZTask parentPredecessor = parentResult;
                                    if (parentPredecessor.Status != WizardTaskStatusesAttribute._COMPLETED)
                                    {
                                        parentIsPending = true;
                                        break;
                                    }
                                }
                            }
                        }

                        WZTaskPredecessorRelation relation = (WZTaskPredecessorRelation) result;
                        if (relation != null)
                        {
                            if (relation.TaskID == null)
                            {
                                if (!parentIsPending)
                                {
                                    taskCopy.Status = WizardTaskStatusesAttribute._OPEN;
                                }
                            }
                            else
                            {
                                bool predecessorIsPending = false;
                                foreach (PXResult<WZTask, WZTaskRelation> predResult in PXSelectJoin<WZTask,
                                    LeftJoin<WZTaskRelation, On<WZTaskRelation.predecessorID, Equal<WZTask.taskID>>>,
                                    Where<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                        And<WZTaskRelation.taskID, Equal<Required<WZTask.taskID>>>>>.
                                    Select(this, task.ScenarioID, task.TaskID))
                                {
                                    WZTaskRelation predecessorRelation = predResult;
                                    if (predecessorRelation != null && predecessorRelation.PredecessorID != null)
                                    {
                                        WZTask predecessor = PXSelect<WZTask, Where<WZTask.taskID, Equal<Required<WZTask.taskID>>>>
                                                            .Select(this, predecessorRelation.PredecessorID);
                                        if (predecessor != null
                                            && predecessor.Status != WizardTaskStatusesAttribute._COMPLETED
                                            && predecessor.Status != WizardTaskStatusesAttribute._SKIPPED
                                            && predecessor.Status != WizardTaskStatusesAttribute._DISABLED)
                                        {
                                            predecessorIsPending = true;
                                            break;
                                        }    
                                    }
                                    
                                }
                                if (!predecessorIsPending)
                                {
                                    taskCopy.Status = WizardTaskStatusesAttribute._OPEN;
                                }
                            }
                        }

                        if (relation == null && task.ParentTaskID == Guid.Empty)
                        {
                            taskCopy.Status = WizardTaskStatusesAttribute._OPEN;
                        }

                        if (task.Status == WizardTaskStatusesAttribute._DISABLED)
                        {

                            WZTask parentTask = PXSelect<WZTask, 
                                                    Where<WZTask.scenarioID, Equal<Required<WZScenario.scenarioID>>,
                                                    And<WZTask.taskID, Equal<Required<WZTask.parentTaskID>>>>>.
                                                    Select(this, scenario.ScenarioID, task.ParentTaskID);

                            if (parentTask.Status != WizardTaskStatusesAttribute._DISABLED)
                            {
                                taskCopy.Status = WizardTaskStatusesAttribute._PENDING;
                            }
                        }
                    }
                    else
                    {
                        taskCopy.Status = WizardTaskStatusesAttribute._DISABLED;
                    }

                    TaskInfo.Update(taskCopy);
                }
                
                this.Actions.PressSave();
            }
            return adapter.Get();
        }

        public virtual IEnumerable CompleteScenarioWithoutRefresh(PXAdapter adapter)
        {
            WZScenario scenario = Scenario.Current;
            if (scenario != null && scenario.Status == WizardScenarioStatusesAttribute._ACTIVE)
            {
                scenario.Status = WizardScenarioStatusesAttribute._COMPLETED;
                Scenario.Update(scenario);

                foreach (PXResult<WZTask> result in PXSelect<WZTask,
                                                Where<WZTask.scenarioID, Equal<Required<WZScenario.scenarioID>>,
                                                    And<Where<WZTask.status, Equal<WizardTaskStatusesAttribute.Active>,
                                                        Or<WZTask.status, Equal<WizardTaskStatusesAttribute.Open>>>>>>.Select(this, scenario.ScenarioID))
                {
                    WZTask task = (WZTask)result;
                    WZTask taskCopy = (WZTask)this.TaskInfo.Cache.CreateCopy(task);
                    taskCopy.Status = WizardTaskStatusesAttribute._SKIPPED;
                    TaskInfo.Update(taskCopy);
                }

                this.Actions.PressSave();
                PXSiteMap.Provider.Clear();
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Suspend Scenario", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable SuspendScenario(PXAdapter adapter)
        {
            WZScenario scenario = Scenario.Current;
            if (scenario != null && scenario.Status != WizardScenarioStatusesAttribute._SUSPEND)
            {
                scenario.Status = WizardScenarioStatusesAttribute._SUSPEND;
                Scenario.Update(scenario);
                this.Actions.PressSave();
                PXSiteMap.Provider.Clear();
				throw new PXRefreshException();
			}
            return adapter.Get();
        }

        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
        public virtual IEnumerable DeleteTask(PXAdapter adapter)
        {
            if (Scenario.Current != null && (Scenario.Current.Status == WizardScenarioStatusesAttribute._ACTIVE ||
                                             Scenario.Current.Status == WizardScenarioStatusesAttribute._SUSPEND))
            {
                WizardScenarioStatusesAttribute statusList = new WizardScenarioStatusesAttribute();
                throw new PXException(Messages.ScenarioCannotBeEditedInStatus, statusList.ValueLabelDic[Scenario.Current.Status]);
            }

            if (TasksTreeItems.Current != null && TasksTreeItems.Current.TaskID != Guid.Empty)
            {
                if (TaskInfo.Ask(Messages.DeleteTaskHeader, Messages.AllChildTasksWillBeDeleted, MessageButtons.YesNo) ==
                    WebDialogResult.Yes)
                {
                    WZTask task =(WZTask) PXSelect<WZTask, Where<WZTask.taskID, Equal<Required<WZTask.taskID>>>>.Select(this,
                                                this.TasksTreeItems.Current.TaskID);
                    deleteTasks(TasksTreeItems.Current.TaskID);
                    
                    int position = task.Position.Value;

                    if (position > 0)
                    {
                        foreach (
                            WZTask siblingTask in
                                PXSelect<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>,
                                    And<WZTask.position, Greater<Required<WZTask.position>>,
                                    And<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>>>>>.Select(this,
                                        task.ParentTaskID, position, task.ScenarioID))
                        {
                            WZTask taskCopy = (WZTask)this.TaskInfo.Cache.CreateCopy(siblingTask);
                            taskCopy.Position -= 1;
                            TaskInfo.Update(taskCopy);
                        }
                    }

                    TaskInfo.Cache.Delete(task);
                }
            }
            TaskInfo.View.RequestRefresh();
            return adapter.Get();
        }

        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
        public virtual IEnumerable AddTask(PXAdapter adapter)
        {
            WZScenario scenario = Scenario.Current;

            if (scenario != null)
            {
                if (scenario.Status == WizardScenarioStatusesAttribute._ACTIVE ||
                    scenario.Status == WizardScenarioStatusesAttribute._SUSPEND)
                {
                    WizardScenarioStatusesAttribute statusList = new WizardScenarioStatusesAttribute();
                    throw new PXException(Messages.ScenarioCannotBeEditedInStatus, statusList.ValueLabelDic[scenario.Status]);
                }
                if (TaskInfo.Current != null)
                {
                    WZTask parentTask = PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Current<WZScenario.scenarioID>>,
                        And<WZTask.taskID, Equal<Required<WZTask.parentTaskID>>>>>
                        .Select(this, TaskInfo.Current.ParentTaskID);
                    if (parentTask != null && parentTask.ParentTaskID == Guid.Empty &&
                        TaskInfo.Current.Type == WizardTaskTypesAttribute._SCREEN)
                    {
                        throw new PXException(Messages.ScreenTaskCannotHaveChilds);
                    }
                }
                WZTask newTask = (WZTask) TaskInfo.Cache.CreateInstance();
                TaskInfo.Insert(newTask);
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp)]
        public virtual IEnumerable Up(PXAdapter adapter)
        {
            if (Scenario.Current != null && (Scenario.Current.Status == WizardScenarioStatusesAttribute._ACTIVE ||
                                             Scenario.Current.Status == WizardScenarioStatusesAttribute._SUSPEND))
            {
                WizardScenarioStatusesAttribute statusList = new WizardScenarioStatusesAttribute();
                throw new PXException(Messages.ScenarioCannotBeEditedInStatus, statusList.ValueLabelDic[Scenario.Current.Status]);
            }

            if (TaskInfo.Current != null)
            {
                WZTask nextTask;
             
                nextTask = (WZTask)PXSelect<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>,
                        And<WZTask.position, Less<Required<WZTask.position>>,
                        And<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>>>>,
                        OrderBy<Desc<WZTask.position>>>
                        .SelectWindowed(this, 0, 1, TaskInfo.Current.ParentTaskID, TaskInfo.Current.Position, TaskInfo.Current.ScenarioID);
             
                if (nextTask != null)
                {
                    int position = TaskInfo.Current.Position.Value;
                    TaskInfo.Current.Position = nextTask.Position;
                    TaskInfo.Update(TaskInfo.Current);
                    nextTask.Position = position;
                    TaskInfo.Update(nextTask);
                }
            }
            TaskInfo.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.View.RequestRefresh();
            return adapter.Get();
        }

        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown)]
        public virtual IEnumerable Down(PXAdapter adapter)
        {
            if (Scenario.Current != null && (Scenario.Current.Status == WizardScenarioStatusesAttribute._ACTIVE ||
                                             Scenario.Current.Status == WizardScenarioStatusesAttribute._SUSPEND))
            {
                WizardScenarioStatusesAttribute statusList = new WizardScenarioStatusesAttribute();
                throw new PXException(Messages.ScenarioCannotBeEditedInStatus, statusList.ValueLabelDic[Scenario.Current.Status]);
            }

            if (TaskInfo.Current != null)
            {
                WZTask nextTask;

                nextTask = (WZTask)PXSelect<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>,
                                            And<WZTask.position, Greater<Required<WZTask.position>>,
                                            And<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>>>>,
                                            OrderBy<Asc<WZTask.position>>>
                                            .SelectWindowed(this, 0, 1, TaskInfo.Current.ParentTaskID, TaskInfo.Current.Position, TaskInfo.Current.ScenarioID);
                    
                if (nextTask != null)
                {
                    int position = TaskInfo.Current.Position.Value;
                    TaskInfo.Current.Position = nextTask.Position;
                    TaskInfo.Update(TaskInfo.Current);
                    nextTask.Position = position;
                    TaskInfo.Update(nextTask);
                }
            }
            TaskInfo.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.View.RequestRefresh();
            return adapter.Get();
        }

        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowLeft)]
        public virtual IEnumerable Left(PXAdapter adapter)
        {
            if (Scenario.Current != null && (Scenario.Current.Status == WizardScenarioStatusesAttribute._ACTIVE ||
                                             Scenario.Current.Status == WizardScenarioStatusesAttribute._SUSPEND))
            {
                WizardScenarioStatusesAttribute statusList = new WizardScenarioStatusesAttribute();
                throw new PXException(Messages.ScenarioCannotBeEditedInStatus, statusList.ValueLabelDic[Scenario.Current.Status]);
            }

            if (TaskInfo.Current != null)
            {
                WZTask parentTask;
                if (TaskInfo.Current.ParentTaskID != Guid.Empty)
                {           
                    parentTask = (WZTask)PXSelect<WZTask, 
                                                Where<WZTask.taskID, Equal<Required<WZTask.parentTaskID>>>,
                                                OrderBy<Asc<WZTask.position>>>
                                                .SelectWindowed(this, 0, 1, TaskInfo.Current.ParentTaskID);
                    
                    if (parentTask != null)
                    {
                        if (parentTask.ParentTaskID == Guid.Empty &&
                            TaskInfo.Current.Type == WizardTaskTypesAttribute._SCREEN)
                        {
                            return adapter.Get();
                        }

                        WZTask lastTask;
                        if (parentTask.ParentTaskID == Guid.Empty)
                        {
                            lastTask = (WZTask) PXSelect<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.taskID>>>,
                                OrderBy<Desc<WZTask.position>>>
                                .SelectWindowed(this, 0, 1, Guid.Empty);
                        }
                        else
                        {
                            lastTask = (WZTask)PXSelect<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>>,
                                OrderBy<Desc<WZTask.position>>>
                                .SelectWindowed(this, 0, 1, parentTask.ParentTaskID);
                        }

                        if (lastTask != null)
                        {
                            TaskInfo.Current.ParentTaskID = lastTask.ParentTaskID;
                            TaskInfo.Current.Position = lastTask.Position + 1;
                            TaskInfo.Update(TaskInfo.Current);
                        }
                    }
                }
            }
            TaskInfo.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.View.RequestRefresh();

            return adapter.Get();
        }

        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowRight)]
        public virtual IEnumerable Right(PXAdapter adapter)
        {
            if (Scenario.Current != null && (Scenario.Current.Status == WizardScenarioStatusesAttribute._ACTIVE ||
                                             Scenario.Current.Status == WizardScenarioStatusesAttribute._SUSPEND))
            {
                WizardScenarioStatusesAttribute statusList = new WizardScenarioStatusesAttribute();
                throw new PXException(Messages.ScenarioCannotBeEditedInStatus, statusList.ValueLabelDic[Scenario.Current.Status]);
            }

            if (TaskInfo.Current != null)
            {
                WZTask nextTask;

                nextTask = (WZTask)PXSelect<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>,
                                                And<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>, 
                                                And<WZTask.position, Less<Required<WZTask.position>>>>>,
                                                OrderBy<Desc<WZTask.position>>>
                                            .SelectWindowed(this, 0, 1, TaskInfo.Current.ParentTaskID, TaskInfo.Current.ScenarioID, TaskInfo.Current.Position);

                if (nextTask != null)
                {

                    if (nextTask.Type == WizardTaskTypesAttribute._SCREEN)
                    {
                        return adapter.Get();
                    }

                    WZTask lastTask;

                    lastTask = (WZTask)PXSelect<WZTask, Where<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>,
                                And<WZTask.scenarioID, Equal<Required<WZTask.scenarioID>>>>,
                            OrderBy<Desc<WZTask.position>>>
                            .SelectWindowed(this, 0, 1, nextTask.TaskID, nextTask.ScenarioID);


                    if (lastTask != null)
                    {
                        TaskInfo.Current.ParentTaskID = lastTask.ParentTaskID;
                        TaskInfo.Current.Position = lastTask.Position + 1;
                        TaskInfo.Update(TaskInfo.Current);
                    }
                    else
                    {
                        TaskInfo.Current.ParentTaskID = nextTask.TaskID;
                        TaskInfo.Current.Position = 1;
                        TaskInfo.Update(TaskInfo.Current);
                    }
                }
            }
            TaskInfo.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.Cache.ClearQueryCacheObsolete();
            TasksTreeItems.View.RequestRefresh();
            return adapter.Get();
        }
        #endregion

        #region Utils
        private void deleteTasks(Guid? taskID)
        {
            if(taskID == null || taskID == Guid.Empty) return;
            foreach (WZTask task in PXSelect<WZTask, 
                                        Where<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>>>
                                        .Select(this, taskID))
            {
                if(task.TaskID != task.ParentTaskID)
                    deleteTasks(task.TaskID);
                TasksTreeItems.Cache.Delete(task);
            }

        }

        private List<string> GetFeatureDependency(string featureID)
        {
            List<string> list = new List<string>();
            if (FeatureDependsOn.ContainsKey(featureID))
            {
                list.Add(FeatureDependsOn[featureID]);
            }
            return list;
        }

        private List<string> GetFeaturesDependsOn(string featureID)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> featureDependency in FeatureDependsOn)
            {
                if (featureDependency.Value.Equals(featureID))
                {
                    list.Add(featureDependency.Key);
                }
            }
            return list;
        }

        private List<WZTask> GetRequiredTasks(Guid? scenarioID, Guid? taskID)
        {
            List<WZTask> list = new List<WZTask>();
            foreach (PXResult<WZTaskRelation, WZTask> relation in PXSelectJoin<WZTaskRelation, 
                                                    InnerJoin<WZTask, On<WZTaskRelation.predecessorID, Equal<WZTask.taskID>>>,
                                                    Where<WZTaskRelation.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                                    And<WZTaskRelation.taskID, Equal<Required<WZTask.taskID>>>>>
                                                    .Select(this, scenarioID, taskID))
            {
                WZTask task = (WZTask) relation;
                WZTaskRelation rel = (WZTaskRelation) relation;
                list.Add(task);
                foreach (WZTask parentRequiredTask in GetRequiredTasks(task.ScenarioID, rel.PredecessorID))
                {
                    list.Add(parentRequiredTask);
                }
            }
            return list;
        }
        private List<WZTask> GetNextTasks(Guid? scenarioID, Guid? taskID)
        {
            List<WZTask> list = new List<WZTask>();
            foreach (PXResult<WZTaskRelation, WZTask> relation in PXSelectJoin<WZTaskRelation,
                                                    InnerJoin<WZTask, On<WZTaskRelation.taskID, Equal<WZTask.taskID>>>,
                                                    Where<WZTaskRelation.scenarioID, Equal<Required<WZTask.scenarioID>>,
                                                    And<WZTaskRelation.predecessorID, Equal<Required<WZTask.taskID>>>>>
                                                    .Select(this, scenarioID, taskID))
            {
                WZTask task = (WZTask)relation;
                WZTaskRelation rel = (WZTaskRelation)relation;
                list.Add(task);
                foreach (WZTask parentRequiredTask in GetNextTasks(task.ScenarioID, rel.TaskID))
                {
                    list.Add(parentRequiredTask);
                }
            }
            return list;
        }

        private List<WZTask> GetChildTasks(Guid? taskID)
        {
            List<WZTask> list = new List<WZTask>();

            foreach (WZTask task in PXSelect<WZTask, Where<WZTask.scenarioID, Equal<Current<WZScenario.scenarioID>>,
                                            And<WZTask.parentTaskID, Equal<Required<WZTask.parentTaskID>>>>>
                                            .Select(this, taskID))
            {
                list.Add(task);
                foreach (WZTask childTask in GetChildTasks(task.TaskID))
                {
                    list.Add(childTask);
                }
            }
            return list;
        }

        private int FillFeatureHierarchy(int level, int order, IEnumerable<string> features, Dictionary<string, List<string>> subfeatures)
        {
            foreach (string feature in features)
            {
                order += 1;

                if (!FeatureOrder.ContainsKey(feature))
                {
                    FeatureOrder.Add(feature, order);
                }

                if (!FeatureOffset.ContainsKey(feature))
                {
                    FeatureOffset.Add(feature, level);
                }
                            
                
                if (subfeatures.ContainsKey(feature))
                   order = FillFeatureHierarchy(level + 1, order, subfeatures[feature], subfeatures);
            }
            return order;
        }
        #endregion

    }

    [Serializable]
	[PXHidden]
	public partial class WZTaskPredecessorRelation : WZTaskRelation
    {
        #region ScenarioID
        public new abstract class scenarioID : PX.Data.BQL.BqlGuid.Field<scenarioID> { }

        [PXDBGuid(IsKey = true)]
        [PXDBDefault(typeof(WZScenario.scenarioID))]
        [PXParent(typeof(Select<WZScenario, Where<WZScenario.scenarioID, Equal<Current<WZTaskPredecessorRelation.scenarioID>>>>))]
        public override Guid? ScenarioID { get; set; }
        #endregion
        #region TaskID
        public new abstract class taskID : PX.Data.BQL.BqlGuid.Field<taskID> { }
        [PXDBGuid(IsKey = true)]
        [PXDBDefault(typeof(WZTask.taskID))]
        [PXParent(typeof(Select<WZTask, Where<WZTask.taskID, Equal<Current<WZTaskPredecessorRelation.taskID>>>>))]
        public override Guid? TaskID { get; set; }
        #endregion
        #region PredecessorID
        public new abstract class predecessorID : PX.Data.BQL.BqlGuid.Field<predecessorID> { }
        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Predecessor Name", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PXParent(typeof(Select<WZTask, Where<WZTask.taskID, Equal<Current<WZTaskPredecessorRelation.predecessorID>>>>))]
        [PXSelector(typeof(Search<WZTask.taskID,
                                Where<WZTask.scenarioID, Equal<Current<WZTaskPredecessorRelation.scenarioID>>,
                                And<WZTask.taskID, NotEqual<Current<WZTask.taskID>>>>>), SubstituteKey = typeof(WZTask.name))]
        public override Guid? PredecessorID { get; set; }
        #endregion
    }

    [Serializable]
	[PXHidden]
	public partial class WZTaskSuccessorRelation : WZTaskRelation
    {
        #region ScenarioID
        public new abstract class scenarioID : PX.Data.BQL.BqlGuid.Field<scenarioID> { }

        [PXDBGuid(IsKey = true)]
        [PXDBDefault(typeof(WZScenario.scenarioID))]
        [PXParent(typeof(Select<WZScenario, Where<WZScenario.scenarioID, Equal<Current<WZTaskSuccessorRelation.scenarioID>>>>))]
        public override Guid? ScenarioID { get; set; }
        #endregion
        #region TaskID
        public new abstract class taskID : PX.Data.BQL.BqlGuid.Field<taskID> { }
        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Successor Name", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PXSelector(typeof(Search<WZTask.taskID,
                                Where<WZTask.scenarioID, Equal<Current<WZTaskSuccessorRelation.scenarioID>>,
                                And<WZTask.taskID, NotEqual<Current<WZTask.taskID>>>>>), SubstituteKey = typeof(WZTask.name))]
        public override Guid? TaskID { get; set; }
        #endregion
        #region PredecessorID
        public new abstract class predecessorID : PX.Data.BQL.BqlGuid.Field<predecessorID> { }
        [PXDBGuid(IsKey = true)]
        [PXDBDefault(typeof(WZTask.taskID))]
        public override Guid? PredecessorID { get; set; }
        #endregion
    }

    [Serializable]
    [PXVirtual]
	[PXHidden]
	public partial class WZTaskTreeItem : IBqlTable
    {
        #region ScenraioID
        public abstract class scenarioID : PX.Data.BQL.BqlGuid.Field<scenarioID> { }

        protected Guid? _ScenarioID;

        [PXDBGuid()]
        public virtual Guid? ScenarioID
        {
            get
            {
                return this._ScenarioID;
            }
            set
            {
                this._ScenarioID = value;
            }
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlGuid.Field<taskID> { }

        protected Guid? _TaskID;

        [PXDBGuid(IsKey = true)]
        public virtual Guid? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region ParentTaskID
        public abstract class parentTaskID : PX.Data.BQL.BqlGuid.Field<parentTaskID> { }

        protected Guid? _ParentTaskID;
        [PXDBGuid]
        public virtual Guid? ParentTaskID
        {
            get
            {
                return this._ParentTaskID;
            }
            set
            {
                this._ParentTaskID = value;
            }
        }
        #endregion
        #region Name
        public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

        protected String _Name;
        [PXDBString(100, IsUnicode = true)]
        public virtual String Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }

        }
        #endregion
        #region Position
        public abstract class position : PX.Data.BQL.BqlInt.Field<position> { }

        protected Int32? _Position;
        [PXDBInt]
        public virtual Int32? Position
        {
            get
            {
                return this._Position;
            }
            set
            {
                this._Position = value;
            }
        }
        #endregion
    }
}
