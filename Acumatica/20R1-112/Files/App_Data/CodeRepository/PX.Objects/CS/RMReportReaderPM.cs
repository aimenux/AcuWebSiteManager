using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.IN;
using PX.Reports.ARm;
using PX.Reports.ARm.Data;
using PX.Objects.Common.Extensions;

using PX.CS;
using System.Threading.Tasks;

namespace PX.Objects.CS
{
    public class RMReportReaderPM : PXGraphExtension<RMReportReaderGL, RMReportReader>
    {
		#region DAC Attributes Override

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt(IsKey = true)]
		protected virtual void PMTask_ProjectID_CacheAttached(PXCache sender) { }

		#endregion

		#region Views

		public PXSelect<PMTask> DummyTask;

		#endregion

		#region report

		private HashSet<int> _historyLoaded;
		private HashSet<PMHistoryKeyTuple> _historySegments;
		private Dictionary<PMHistoryKeyTuple, Dictionary<string, PMHistory>> _pmhistoryPeriodsByKey;
		private Dictionary<BudgetKeyTuple, PMBudget> _budgetByKey;

		private RMReportPeriods<PMHistory> _reportPeriods;

		private RMReportRange<PMAccountGroup> _accountGroupsRangeCache;
		private RMReportRange<PMProject> _projectsRangeCache;
		private RMReportRange<PMTask> _tasksRangeCache;
		private RMReportRange<InventoryItem> _itemRangeCache;
		private RMReportRange<PMCostCode> _costCodeRangeCache;

	    private string _accountGroupMask;
	    private string _projectMask;
	    private string _projectTaskMask;
	    private string _inventoryMask;

		[PXOverride]
        public void Clear(Action del)
        {
            del();

            _historyLoaded = null;
            _accountGroupsRangeCache = null;
            _projectsRangeCache = null;
            _tasksRangeCache = null;
            _itemRangeCache = null;
			_costCodeRangeCache = null;
		}

        public virtual void PMEnsureInitialized()
        {
            if (_historyLoaded == null)
            {
                _reportPeriods = new RMReportPeriods<PMHistory>(this.Base);

                var accountGroupSelect = new PXSelectOrderBy<PMAccountGroup, OrderBy<Asc<PMAccountGroup.groupCD>>>(this.Base);
                accountGroupSelect.View.Clear();
                accountGroupSelect.Cache.Clear();
               
                var projectSelect = new PXSelect<PMProject, Where<PMProject.nonProject, Equal<False>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>, OrderBy<Asc<PMProject.contractCD>>>(this.Base);
                projectSelect.View.Clear();
                projectSelect.Cache.Clear();

                var taskSelect = new PXSelectJoin<PMTask, InnerJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.baseType, Equal<CT.CTPRType.project>, And<PMProject.nonProject, Equal<False>>>,OrderBy<Asc<PMTask.taskCD>>>(this.Base);
                taskSelect.View.Clear();
                taskSelect.Cache.Clear();
                
                var itemFromHistorySelect = new PXSelectJoinGroupBy<InventoryItem, InnerJoin<PMHistory, On<InventoryItem.inventoryID, Equal<PMHistory.inventoryID>>>, BqlNone, Aggregate<GroupBy<InventoryItem.inventoryID>>, OrderBy<Asc<InventoryItem.inventoryCD>>>(this.Base);
                itemFromHistorySelect.View.Clear();
                itemFromHistorySelect.Cache.Clear();

				var itemFromBudgetSelect = new PXSelectJoinGroupBy<InventoryItem, InnerJoin<PMBudget, On<InventoryItem.inventoryID, Equal<PMBudget.inventoryID>>>, BqlNone, Aggregate<GroupBy<InventoryItem.inventoryID>>, OrderBy<Asc<InventoryItem.inventoryCD>>>(this.Base);
				itemFromBudgetSelect.View.Clear();
				itemFromBudgetSelect.Cache.Clear();

				var costCodeFromHistorySelect = new PXSelectJoinGroupBy<PMCostCode, InnerJoin<PMHistory, On<PMCostCode.costCodeID, Equal<PMHistory.costCodeID>>>, BqlNone, Aggregate<GroupBy<PMCostCode.costCodeID>>, OrderBy<Asc<PMCostCode.costCodeCD>>>(this.Base);
				costCodeFromHistorySelect.View.Clear();
				costCodeFromHistorySelect.Cache.Clear();

				var costCodeFromBudgetSelect = new PXSelectJoinGroupBy<PMCostCode, InnerJoin<PMBudget, On<PMCostCode.costCodeID, Equal<PMBudget.costCodeID>>>, BqlNone, Aggregate<GroupBy<PMCostCode.costCodeID>>, OrderBy<Asc<PMCostCode.costCodeCD>>>(this.Base);
				costCodeFromBudgetSelect.View.Clear();
				costCodeFromBudgetSelect.Cache.Clear();

				_budgetByKey = new Dictionary<BudgetKeyTuple, PMBudget>();

				foreach (PXResult<PMBudget, PMTask> res in SelectBudgetRecords())
				{
					PMBudget budget = res;
					_budgetByKey.Add(new BudgetKeyTuple(budget.ProjectID.Value, budget.ProjectTaskID.Value, budget.AccountGroupID.Value, budget.InventoryID.Value, budget.CostCodeID.Value), budget);
				}
				
				if (Base.Report.Current.ApplyRestrictionGroups == true)
				{
					projectSelect.WhereAnd<Where<Match<Current<AccessInfo.userName>>>>();
					taskSelect.WhereAnd<Where<Match<Current<AccessInfo.userName>>>>();
					itemFromHistorySelect.WhereAnd<Where<Match<Current<AccessInfo.userName>>>>();
					itemFromBudgetSelect.WhereAnd<Where<Match<Current<AccessInfo.userName>>>>();
				}

                var tempAccountGroup = accountGroupSelect.Select().ToList();
                var tempProjects = projectSelect.Select().ToList();
                var tempRasks = taskSelect.Select().ToList();
				
                foreach (InventoryItem item in itemFromHistorySelect.Select().Union(itemFromBudgetSelect.Select()))
                {
                    //The PXSelectJoinGroupBy is read-only, and Inventory items won't be added to the cache. Add them manually.
                    itemFromHistorySelect.Cache.SetStatus(item, PXEntryStatus.Notchanged);
                }

				foreach (PMCostCode item in costCodeFromHistorySelect.Select().Union(costCodeFromBudgetSelect.Select()))
				{
					//The PXSelectJoinGroupBy is read-only, and Cost Codes won't be added to the cache. Add them manually.
					costCodeFromHistorySelect.Cache.SetStatus(item, PXEntryStatus.Notchanged);
				}

				_historySegments = new HashSet<PMHistoryKeyTuple>();
                _pmhistoryPeriodsByKey = new Dictionary<PMHistoryKeyTuple, Dictionary<string, PMHistory>>();
                _historyLoaded = new HashSet<int>();

                _accountGroupsRangeCache = new RMReportRange<PMAccountGroup>(Base, PM.AccountGroupAttribute.DimensionName, RMReportConstants.WildcardMode.Fixed, RMReportConstants.BetweenMode.Fixed);
                _projectsRangeCache = new RMReportRange<PMProject>(Base, PM.ProjectAttribute.DimensionName, RMReportConstants.WildcardMode.Fixed, RMReportConstants.BetweenMode.Fixed);
                _tasksRangeCache = new RMReportRange<PMTask>(Base, PM.ProjectTaskAttribute.DimensionName, RMReportConstants.WildcardMode.Normal, RMReportConstants.BetweenMode.Fixed);
                _itemRangeCache = new RMReportRange<InventoryItem>(Base, IN.InventoryAttribute.DimensionName, RMReportConstants.WildcardMode.Fixed, RMReportConstants.BetweenMode.Fixed);
				_costCodeRangeCache = new RMReportRange<PMCostCode>(Base, PM.PMCostCode.costCodeCD.DimensionName, RMReportConstants.WildcardMode.Fixed, RMReportConstants.BetweenMode.Fixed);

	            _accountGroupMask = (this.Base.Caches[typeof(PMAccountGroup)].GetStateExt<PMAccountGroup.groupCD>(null) as PXStringState)?.InputMask;
	            _projectMask = (this.Base.Caches[typeof(PMProject)].GetStateExt<PMProject.contractCD>(null) as PXStringState)?.InputMask;
	            _projectTaskMask = (this.Base.Caches[typeof(PMTask)].GetStateExt<PMTask.taskCD>(null) as PXStringState)?.InputMask;
	            _inventoryMask = (this.Base.Caches[typeof(InventoryItem)].GetStateExt<InventoryItem.inventoryCD>(null) as PXStringState)?.InputMask;
			}
        }

		public virtual PXResultset<PMBudget> SelectBudgetRecords()
		{
			PXSelectBase<PMBudget> selectBudget = new PXSelectJoin<PMBudget, InnerJoin<PMTask, On<PMBudget.projectTaskID, Equal<PMTask.taskID>>>>(this.Base);

			using (new PXFieldScope(selectBudget.View, new Type[] { typeof(PMBudget.projectID), typeof(PMBudget.projectTaskID), typeof(PMBudget.accountGroupID), typeof(PMBudget.inventoryID), typeof(PMBudget.costCodeID),
						typeof(PMBudget.qty), typeof(PMBudget.curyAmount), typeof(PMBudget.revisedQty), typeof(PMBudget.curyRevisedAmount),
						typeof(PMBudget.curyCommittedOrigAmount),typeof(PMBudget.committedOrigAmount),typeof(PMBudget.curyCommittedAmount), typeof(PMBudget.committedAmount), typeof(PMBudget.curyCommittedInvoicedAmount), typeof(PMBudget.committedInvoicedQty), typeof(PMBudget.curyCommittedOpenAmount), typeof(PMBudget.committedOpenQty), typeof(PMBudget.committedOrigQty), typeof(PMBudget.committedQty), typeof(PMBudget.committedReceivedQty),
					typeof(PMBudget.actualQty), typeof(PMBudget.actualAmount), typeof(PMTask.taskID), typeof(PMTask.taskCD)}))
			{
				return selectBudget.Select();
			}
		}

		public virtual void NormalizeDataSource(RMDataSourcePM dsPM)
        {
            if (dsPM.StartAccountGroup != null && dsPM.StartAccountGroup.TrimEnd() == "")
            {
                dsPM.StartAccountGroup = null;
            }
            if (dsPM.EndAccountGroup != null && dsPM.EndAccountGroup.TrimEnd() == "")
            {
                dsPM.EndAccountGroup = null;
            }
            if (dsPM.StartProject != null && dsPM.StartProject.TrimEnd() == "")
            {
                dsPM.StartProject = null;
            }
            if (dsPM.EndProject != null && dsPM.EndProject.TrimEnd() == "")
            {
                dsPM.EndProject = null;
            }
            if (dsPM.StartProjectTask != null && dsPM.StartProjectTask.TrimEnd() == "")
            {
                dsPM.StartProjectTask = null;
            }
            if (dsPM.EndProjectTask != null && dsPM.EndProjectTask.TrimEnd() == "")
            {
                dsPM.EndProjectTask = null;
            }
            if (dsPM.StartInventory != null && dsPM.StartInventory.TrimEnd() == "")
            {
                dsPM.StartInventory = null;
            }
            if (dsPM.EndInventory != null && dsPM.EndInventory.TrimEnd() == "")
            {
                dsPM.EndInventory = null;
            }
        }

		public void ProcessPMResultset(PXResultset<PMHistory> resultset)
        {
            foreach (PXResult<PMHistory, PMTask> result in resultset)
            {
                var hist = (PMHistory) result;
                var task = (PMTask) result;

                var key = new PMHistoryKeyTuple(hist.ProjectID.Value, task.TaskCD, hist.AccountGroupID.Value, hist.InventoryID.Value, hist.CostCodeID.Value);
                Dictionary<string, PMHistory> keyData;
                if (_pmhistoryPeriodsByKey.TryGetValue(key, out keyData))
                {
                    if (keyData.TryGetValue(hist.PeriodID, out PMHistory existingHist))
                    {
                        existingHist.TranPTDAmount += hist.TranPTDAmount;
                        existingHist.TranPTDCuryAmount += hist.TranPTDCuryAmount;
                        existingHist.TranPTDQty += hist.TranPTDQty;
                        existingHist.TranYTDAmount += hist.TranYTDAmount;
                        existingHist.TranYTDCuryAmount += hist.TranYTDCuryAmount;
                        existingHist.TranYTDQty += hist.TranYTDQty;
                    }
                    else
                    {
                        keyData.Add(hist.PeriodID, hist);
                    }
                }
                else
                {
                    _pmhistoryPeriodsByKey.Add(key, new Dictionary<string, PMHistory> { { hist.PeriodID, hist } });
                }

                _historySegments.Add(new PMHistoryKeyTuple(0, String.Empty, hist.AccountGroupID.Value, 0, 0));
                _historySegments.Add(new PMHistoryKeyTuple(hist.ProjectID.Value, String.Empty, hist.AccountGroupID.Value, 0, 0));
                _historySegments.Add(new PMHistoryKeyTuple(hist.ProjectID.Value, task.TaskCD, hist.AccountGroupID.Value, 0, 0));
            }
        }

		public void UpdateHistorySegmentsWithRecordsFromBudget()
		{
			foreach (PXResult<PMBudget, PMTask> res in SelectBudgetRecords())
			{
				PMBudget budget = res;
				PMTask task = res;

				_historySegments.Add(new PMHistoryKeyTuple(0, String.Empty, budget.AccountGroupID.Value, 0, 0));
				_historySegments.Add(new PMHistoryKeyTuple(budget.ProjectID.Value, String.Empty, budget.AccountGroupID.Value, 0, 0));
				_historySegments.Add(new PMHistoryKeyTuple(budget.ProjectID.Value, task.TaskCD, budget.AccountGroupID.Value, 0, 0));
			}
		}

        [PXOverride]
        public virtual object GetHistoryValue(ARmDataSet dataSet, bool drilldown, Func<ARmDataSet, bool, object> del)
        {
            string rmType = Base.Report.Current.Type;
            if (rmType == ARmReport.PM)
            {
                RMDataSource ds = Base.DataSourceByID.Current;
                RMDataSourcePM dsPM = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourcePM>(ds);

                ds.AmountType = (short?)dataSet[RMReportReaderGL.Keys.AmountType];
                dsPM.StartAccountGroup = dataSet[Keys.StartAccountGroup] as string ?? "";
                dsPM.EndAccountGroup = dataSet[Keys.EndAccountGroup] as string ?? "";
                dsPM.StartProject = dataSet[Keys.StartProject] as string ?? "";
                dsPM.EndProject = dataSet[Keys.EndProject] as string ?? "";
                dsPM.StartProjectTask = dataSet[Keys.StartProjectTask] as string ?? "";
                dsPM.EndProjectTask = dataSet[Keys.EndProjectTask] as string ?? "";
                dsPM.StartInventory = dataSet[Keys.StartInventory] as string ?? "";
                dsPM.EndInventory = dataSet[Keys.EndInventory] as string ?? "";

                RMDataSourceGL dsGL = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);
                dsGL.StartBranch = dataSet[RMReportReaderGL.Keys.StartBranch] as string ?? "";
                dsGL.EndBranch = dataSet[RMReportReaderGL.Keys.EndBranch] as string ?? "";
                dsGL.EndPeriod = ((dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Length > 2 ? ((dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Length > 2 ? (dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").Substring(0, 2) : dataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "");
                dsGL.EndPeriodOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.EndOffset];
                dsGL.EndPeriodYearOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.EndYearOffset];
                dsGL.StartPeriod = ((dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Length > 2 ? ((dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Length > 2 ? (dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").Substring(0, 2) : dataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "");
                dsGL.StartPeriodOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.StartOffset];
                dsGL.StartPeriodYearOffset = (short?)(int?)dataSet[RMReportReaderGL.Keys.StartYearOffset];

                List<object[]> splitret = null;

                if (ds.Expand != ExpandType.Nothing)
                {
                    splitret = new List<object[]>();
                }

                if (ds.AmountType == null || ds.AmountType == 0)
                {
                    return 0m;
                }

                PMEnsureInitialized();
                EnsureHistoryLoaded(dsPM);
                NormalizeDataSource(dsPM);

	            List<PMAccountGroup> accountGroups = GetItemsInRange<PMAccountGroup>(dataSet);
	            List<PMProject> projects = GetItemsInRange<PMProject>(dataSet);
                List<PMTask> tasks = GetItemsInRange<PMTask>(dataSet);
	            List<InventoryItem> items = GetItemsInRange<InventoryItem>(dataSet);
				List<PMCostCode> costCodes = GetItemsInRange<PMCostCode>(dataSet);
				
				if (ds.Expand == ExpandType.AccountGroup)
                {
	                foreach (var accountGroup in accountGroups)
	                {
		                var dataSetCopy = new ARmDataSet(dataSet);
		                dataSetCopy[Keys.StartAccountGroup] = dataSetCopy[Keys.EndAccountGroup] = accountGroup.GroupCD;
		                // ReSharper disable once PossibleNullReferenceException
		                splitret.Add(new object[] { accountGroup.GroupCD, accountGroup.Description, 0m, dataSetCopy, string.Empty, Mask.Format(_accountGroupMask, accountGroup.GroupCD) });
	                }
                }
                else if (ds.Expand == ExpandType.Project)
                {
					foreach (var project in projects)
					{
						var dataSetCopy = new ARmDataSet(dataSet);
						dataSetCopy[Keys.StartProject] = dataSetCopy[Keys.EndProject] = project.ContractCD;
						// ReSharper disable once PossibleNullReferenceException
						splitret.Add(new object[] { project.ContractCD, project.Description, 0m, dataSetCopy, string.Empty, Mask.Format(_projectMask, project.ContractCD) });
					}
                }
                else if (ds.Expand == ExpandType.ProjectTask)
                {
                    var tasksByCD = tasks
	                    .Where(t => projects.Any(p => t.ProjectID == p.ContractID))
                        .GroupBy(t => t.TaskCD);
                    var maxTasksInGroup = tasksByCD.Max(g => g.Count());
                    var groupsCount = tasksByCD.Count();
                    var resultTasks = new PMTask[groupsCount * maxTasksInGroup];
                    tasksByCD.ForEach((g, gi) =>{
                        string taskCD = g.Key;
                        var dataSetCopy = new ARmDataSet(dataSet)
                            { [Keys.StartProjectTask] = taskCD
                            , [Keys.EndProjectTask] = taskCD };
                        splitret.Add( new object[] { 
                            taskCD, 
                            g.Min(t => t.Description), 
                            0m, 
                            dataSetCopy, 
                            string.Empty, 
                            Mask.Format(_projectTaskMask, taskCD) });
                        g.ForEach((t, ti) => resultTasks[ti*groupsCount + gi] = t);
                    });
                    tasks = resultTasks.ToList();
                }
                else if (ds.Expand == ExpandType.Inventory)
                {
	                foreach (var item in items)
	                {
		                var dataSetCopy = new ARmDataSet(dataSet);
		                dataSetCopy[Keys.StartInventory] = dataSetCopy[Keys.EndInventory] = item.InventoryCD;
		                // ReSharper disable once PossibleNullReferenceException
		                splitret.Add(new object[] { item.InventoryCD, item.Descr, 0m, dataSetCopy, string.Empty, Mask.Format(_inventoryMask, item.InventoryCD) });
	                }
                }

                return CalculateAndExpandValue(drilldown, ds, dsGL, dsPM, dataSet, accountGroups, projects, tasks, items, costCodes, splitret);
            }
            else
            {
                return del(dataSet, drilldown);
            }
        }

		private List<T> GetItemsInRange<T>(ARmDataSet dataSet)
	    {
		    return (List<T>)Base.GetItemsInRange(typeof (T), dataSet);
	    }

	    [PXOverride]
	    public virtual IEnumerable GetItemsInRange(Type table, ARmDataSet dataSet, Func<Type, ARmDataSet, IEnumerable> del)
	    {
		    if (table == typeof (PMAccountGroup))
		    {
			    return _accountGroupsRangeCache.GetItemsInRange(dataSet[Keys.StartAccountGroup] as string,
				    group => group.GroupCD,
				    (group, code) => group.GroupCD = code);
		    }

		    if (table == typeof (PMProject))
		    {
			    return _projectsRangeCache.GetItemsInRange(dataSet[Keys.StartProject] as string,
				    project => project.ContractCD,
				    (project, code) => project.ContractCD = code);
		    }

		    if (table == typeof (PMTask))
		    {
			    return _tasksRangeCache.GetItemsInRange(dataSet[Keys.StartProjectTask] as string,
				    task => task.TaskCD,
				    (task, code) => task.TaskCD = code);
			}

		    if (table == typeof (InventoryItem))
		    {
			    return _itemRangeCache.GetItemsInRange(dataSet[Keys.StartInventory] as string,
				    item => item.InventoryCD,
				    (item, code) => item.InventoryCD = code);
			}

		    if (table == typeof (PMCostCode))
		    {
			    return _costCodeRangeCache.Cache.Cached.RowCast<PMCostCode>().ToList();
			}

			if (del != null)
		    {
			    return del(table, dataSet);
		    }

		    throw new NotSupportedException();
	    }


		public object CalculateAndExpandValue(bool drilldown, RMDataSource ds, RMDataSourceGL dsGL, RMDataSourcePM dsPM, ARmDataSet dataSet, List<PMAccountGroup> accountGroups, List<PMProject> projects, List<PMTask> tasks, List<InventoryItem> items, List<PMCostCode> costCodes, List<object[]> splitret)
        {
            decimal totalAmount = 0;
			object locker = new object();
			Dictionary<PMHistoryKeyTuple, PXResult<PMHistory, PMBudget, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode>> drilldownData = null;

            object[] splitLockers = null;
            bool expansion = false;
            if (splitret != null 
                && new string[]{
                    ExpandType.AccountGroup, ExpandType.Project, ExpandType.ProjectTask, ExpandType.Inventory}
                    .Contains(ds.Expand))
            {
                expansion = true;
                splitLockers = new object[splitret.Count];
                for (int i = 0; i < splitret.Count; ++i) splitLockers[i] = new object();
            }

            if (drilldown)
            {
				drilldownData = new Dictionary<PMHistoryKeyTuple, PXResult<PMHistory, PMBudget, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode>>();
            }

			const bool forceNonParallel = false;//set to true for debugging in single thread
			var options = new ParallelOptions { MaxDegreeOfParallelism = forceNonParallel ? 1 : -1 };

			var projectsDict = new Dictionary<int?, Tuple<PMProject, int>>(projects.Count);
			for (int i = 0; i < projects.Count; i++)
				projectsDict[projects[i].ContractID] = new Tuple<PMProject, int>(projects[i], i);

			Parallel.For(0, accountGroups.Count, options, accountGroup =>
            {
                PMAccountGroup currentAccountGroup = accountGroups[accountGroup];
                if (!_historySegments.Contains(new PMHistoryKeyTuple(0, String.Empty, currentAccountGroup.GroupID.Value, 0, 0))) return;

                    Parallel.For(0, tasks.Count, options, task =>
                    {
                        PMTask currentTask = tasks[task];
					Tuple<PMProject, int> projectEntry;
					if (currentTask == null || !projectsDict.TryGetValue(currentTask.ProjectID, out projectEntry)) return;
					PMProject currentProject = projectEntry.Item1;
					int project = projectEntry.Item2;
					if (!_historySegments.Contains(new PMHistoryKeyTuple(currentProject.ContractID.Value, string.Empty, currentAccountGroup.GroupID.Value, 0, 0))) return;
                        if (!_historySegments.Contains(new PMHistoryKeyTuple(currentProject.ContractID.Value, currentTask.TaskCD, currentAccountGroup.GroupID.Value, 0, 0))) return;
						if (ds.Expand != ExpandType.ProjectTask && currentTask.ProjectID != currentProject.ContractID) return;

						Parallel.For(0, costCodes.Count, options, costCode =>
						{
							PMCostCode currentCostCode = costCodes[costCode];

							Parallel.For(0, items.Count, options, item =>
							{
								InventoryItem currentItem = items[item];
                                List<Tuple<decimal, PMHistory>> amounts = new List<Tuple<decimal, PMHistory>>();

								List<PMHistory> periods = GetPeriodsToCalculate(currentProject, currentTask, currentAccountGroup, currentItem, currentCostCode, ds, dsGL, drilldown);
								if (periods == null)
								{
								if (IsBudgetValue(ds))
                                    {
										PMHistory dummy = new PMHistory();
										dummy.ProjectID = currentProject.ContractID;
										dummy.ProjectTaskID = currentTask.TaskID;
										dummy.AccountGroupID = currentAccountGroup.GroupID;
										dummy.InventoryID = currentItem.InventoryID;
										dummy.CostCodeID = currentCostCode.CostCodeID;

										var key = new BudgetKeyTuple(currentProject.ContractID.Value, currentTask.TaskID.Value, currentAccountGroup.GroupID.Value, currentItem.InventoryID.Value, currentCostCode.CostCodeID.Value);
                                        decimal amount = GetAmountFromPMBudget(ds, key);
                                        amounts.Add(new Tuple<decimal, PMHistory>(amount, dummy));
									}
                                    else
                                    {
                                        return;
                                    }
								}
                                else
                                {
									var key = new BudgetKeyTuple(
												currentProject.ContractID.Value,
												currentTask.TaskID.Value,
												currentAccountGroup.GroupID.Value,
												currentItem.InventoryID.Value,
												currentCostCode.CostCodeID.Value);
									if (IsBudgetValue(ds))
									{
										bool first = true;
										foreach (PMHistory hist in periods)
										{
											amounts.Add(new Tuple<decimal, PMHistory>(first ? GetAmountFromPMBudget(ds, key) : 0, hist));
											first = false;
										}
									}
									else
									{
										foreach (PMHistory hist in periods)
											amounts.Add(new Tuple<decimal, PMHistory>(GetAmountFromPMHistory(ds, hist), hist));
									}
                                }

								foreach (Tuple<decimal, PMHistory> hist in amounts)
								{                                    
									lock (locker)
									{
										totalAmount += hist.Item1;
									}

									if (drilldown)
									{
										// To ignore inventoryID: var key = new PMHistoryKeyTuple(currentProject.ContractID.Value, currentTask.TaskCD, currentAccountGroup.GroupID.Value, 0, currentCostCode.CostCodeID.Value);
										var key = new PMHistoryKeyTuple(currentProject.ContractID.Value, currentTask.TaskCD, currentAccountGroup.GroupID.Value, currentItem.InventoryID.Value, currentCostCode.CostCodeID.Value);
										PXResult<PMHistory, PMBudget, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode> drilldownRow = null;

                                        var keyBudget = new BudgetKeyTuple(currentProject.ContractID.Value, currentTask.TaskID.Value, currentAccountGroup.GroupID.Value, currentItem.InventoryID.Value, currentCostCode.CostCodeID.Value);
                                        PMBudget currentBudget;
                                        if (!_budgetByKey.TryGetValue(keyBudget, out currentBudget))
                                        {
											//BudgetKeyTuple key2 = new BudgetKeyTuple(keyBudget.ProjectID, keyBudget.ProjectTaskID, keyBudget.AccountGroupID, PM.PMInventorySelectorAttribute.EmptyInventoryID, keyBudget.CostCodeID);
											//if (!_budgetByKey.TryGetValue(key2, out currentBudget))
											{
												currentBudget = new PMBudget();
												currentBudget.ProjectID = currentProject.ContractID;
												currentBudget.ProjectTaskID = currentTask.TaskID;
												currentBudget.AccountGroupID = currentAccountGroup.GroupID;
												currentBudget.InventoryID = currentItem.InventoryID;
												currentBudget.CostCodeID = currentCostCode.CostCodeID;
											}
											
                                        }
                                        
										lock (drilldownData)
										{
											if (!drilldownData.TryGetValue(key, out drilldownRow))
											{
												// To ignore inventoryID: drilldownRow = new PXResult<PMHistory, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode>(new PMHistory(), currentProject, currentTask, currentAccountGroup, items.Last(), currentCostCode);
												drilldownRow = new PXResult<PMHistory, PMBudget, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode>(new PMHistory(), currentBudget, currentProject, currentTask, currentAccountGroup, currentItem, currentCostCode);
												drilldownData.Add(key, drilldownRow);
											}
										}

										lock (drilldownRow)
										{
											AggregatePMHistoryForDrilldown(drilldownRow, hist.Item2);
											// To ignore inventoryID:((PMHistory)drilldownRow).InventoryID = 0;
										}
									}

                                    if (!expansion) continue;
                                    int splitIndex = -1;
                                    RMReportReaderPM.Keys startKey, endKey;
                                    startKey = endKey = default;
                                    string identifierCD = string.Empty;
                                    switch (ds.Expand)
                                    {
                                    case ExpandType.AccountGroup:
                                        splitIndex = accountGroup; identifierCD = currentAccountGroup.GroupCD;
                                        startKey = RMReportReaderPM.Keys.StartAccountGroup; endKey = RMReportReaderPM.Keys.EndAccountGroup;
                                        break;
                                    case ExpandType.Project:
                                        splitIndex = project; identifierCD = currentProject.ContractCD;
                                        startKey = RMReportReaderPM.Keys.StartProject; endKey = RMReportReaderPM.Keys.EndProject;
                                        break;
                                    case ExpandType.ProjectTask:
                                        splitIndex = task; identifierCD = currentTask.TaskCD;
                                        startKey = RMReportReaderPM.Keys.StartProjectTask; endKey = RMReportReaderPM.Keys.EndProjectTask;
                                        break;
                                    case ExpandType.Inventory:
                                        splitIndex = item; identifierCD = currentItem.InventoryCD;
                                        startKey = RMReportReaderPM.Keys.StartInventory; endKey = RMReportReaderPM.Keys.EndInventory;
                                        break;
                                    }
                                    if (splitIndex > -1)
                                    {
                                        splitIndex = splitIndex % splitret.Count;
                                        lock(splitLockers[splitIndex])
                                            PopulateRet(
                                                splitret[splitIndex], hist.Item1, 
                                                dataSet, identifierCD,
                                                startKey, endKey);
									}
    							}
						});
                    });
                });
            });

            if (drilldown)
            {
				var resultset = new PXResultset<PMHistory, PMBudget, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode>();
				foreach (var r in
					from row in drilldownData.Values
					let projectCD = ((PMProject)row[typeof(PMProject)]).With(_ => _.ContractCD)
					let taskCD = ((PMTask)row[typeof(Sub)]).With(_ => _.TaskCD)
					let accGroupCD = ((PMAccountGroup)row[typeof(PMAccountGroup)]).With(_ => _.GroupCD)
					let inventoryCD = ((InventoryItem)row[typeof(InventoryItem)]).With(_ => _.InventoryCD)
					let costCodeCD = ((PMCostCode)row[typeof(PMCostCode)]).With(_ => _.CostCodeCD)
					orderby projectCD, taskCD, accGroupCD, inventoryCD, costCodeCD
					select row)
				{
					resultset.Add(r);
				}
                return resultset;
            }
            else if (ds.Expand != ExpandType.Nothing)
            {
                return splitret;
            }
            else
            {
                return totalAmount;
            }
        }

        private static void PopulateRet(
            object[] ret, decimal value,
            ARmDataSet dataSet, string datasetValue,
            RMReportReaderPM.Keys datasetStartKey, RMReportReaderPM.Keys datasetEndKey)
        {
            ret[2] = (decimal)ret[2] + value;
            if (ret[3] == null)
            {
                var dataSetCopy = new ARmDataSet(dataSet)
                    { [datasetStartKey] = datasetValue
                    , [datasetEndKey] = datasetValue };
                ret[3] = dataSetCopy;
            }
        }

	    private static void AggregatePMHistoryForDrilldown(PMHistory resulthist, PMHistory hist)
	    {
			resulthist.ProjectID = hist.ProjectID;
			resulthist.ProjectTaskID = hist.ProjectTaskID;
			resulthist.AccountGroupID = hist.AccountGroupID;
			resulthist.InventoryID = hist.InventoryID;
			resulthist.CostCodeID = hist.CostCodeID;
			resulthist.PeriodID = hist.PeriodID;
					    
			resulthist.FinPTDAmount = resulthist.FinPTDAmount.GetValueOrDefault() + hist.FinPTDAmount.GetValueOrDefault();
			resulthist.FinYTDAmount = hist.FinYTDAmount.GetValueOrDefault();
			resulthist.FinPTDQty = resulthist.FinPTDQty.GetValueOrDefault() + hist.FinPTDQty.GetValueOrDefault();
			resulthist.FinYTDQty = hist.FinYTDQty.GetValueOrDefault();
			resulthist.TranPTDAmount = resulthist.TranPTDAmount.GetValueOrDefault() + hist.TranPTDAmount.GetValueOrDefault();
			resulthist.TranYTDAmount = hist.TranYTDAmount.GetValueOrDefault();
			resulthist.TranPTDQty = resulthist.TranPTDQty.GetValueOrDefault() + hist.TranPTDQty.GetValueOrDefault();
			resulthist.TranYTDQty = hist.TranYTDQty.GetValueOrDefault();
		}

		public List<PMHistory> GetPeriodsToCalculate(PMProject project, PMTask task, PMAccountGroup accountGroup, InventoryItem item, PMCostCode costCode, RMDataSource ds, RMDataSourceGL dsGL, bool allowStartOfProject)
        {
            Dictionary<string, PMHistory> periodsForKey = null;

            var key = new PMHistoryKeyTuple(project.ContractID.Value, task.TaskCD, accountGroup.GroupID.Value, item.InventoryID.Value, costCode.CostCodeID.Value);
            if (!_pmhistoryPeriodsByKey.TryGetValue(key, out periodsForKey))
            {
                return null;
            }

			if (FromStart(ds, allowStartOfProject))
            {
				dsGL.StartPeriod = _reportPeriods.PerWildcard;
				dsGL.StartPeriodOffset = 0;
                dsGL.StartPeriodYearOffset = 0;
            }

            return _reportPeriods.GetPeriodsForRegularAmount(dsGL, periodsForKey);
        }

		private bool FromStart(RMDataSource ds, bool drilldown)
		{
			//might need some other types
			return drilldown 
				?
				ds.AmountType == BalanceType.Amount || IsBudgetValue(ds)
				:
				ds.AmountType == BalanceType.Amount || ds.AmountType == BalanceType.Quantity ||
				ds.AmountType == BalanceType.BudgetAmount || ds.AmountType == BalanceType.BudgetQuantity ||
				ds.AmountType == BalanceType.RevisedAmount || ds.AmountType == BalanceType.RevisedQuantity ||
				ds.AmountType == BalanceType.CommittedAmount || ds.AmountType == BalanceType.CommittedQuantity ||
				ds.AmountType == BalanceType.CommittedOpenAmount || ds.AmountType == BalanceType.CommittedOpenQuantity ||
				ds.AmountType == BalanceType.CommittedInvoicedAmount || ds.AmountType == BalanceType.CommittedInvoicedQuantity || ds.AmountType == BalanceType.CommittedReceivedQuantity ||
				ds.AmountType == BalanceType.ChangeOrderQuantity || ds.AmountType == BalanceType.ChangeOrderAmount;
		}
        public static decimal GetAmountFromPMHistory(RMDataSource ds, PMHistory hist)
        {
            //We always use the PTD amounts; depending on BalanceType we will expand our calculation to more periods (GetPeriodsToCalculate)
            switch (ds.AmountType.Value)
            {
                case BalanceType.Amount:
                case BalanceType.TurnoverAmount:
                    return hist.TranPTDAmount.Value;
                case BalanceType.Quantity:
                case BalanceType.TurnoverQuantity:
                    return hist.TranPTDQty.Value;
               				
				default:
                    System.Diagnostics.Debug.Assert(false, "Unknown amount type: " + ds.AmountType.Value);
                    return 0;
            }
        }

		public virtual bool IsBudgetValue(RMDataSource ds)
		{
			switch (ds.AmountType.Value)
			{
				case BalanceType.BudgetAmount:
				case BalanceType.BudgetQuantity:
				case BalanceType.RevisedAmount:
				case BalanceType.RevisedQuantity:
				case BalanceType.OriginalCommittedAmount:
				case BalanceType.OriginalCommittedQuantity:
				case BalanceType.CommittedAmount:
				case BalanceType.CommittedQuantity:
				case BalanceType.CommittedReceivedQuantity:
				case BalanceType.CommittedInvoicedAmount:
				case BalanceType.CommittedInvoicedQuantity:
				case BalanceType.CommittedOpenAmount:
				case BalanceType.CommittedOpenQuantity:
				case BalanceType.ChangeOrderQuantity:
				case BalanceType.ChangeOrderAmount:
					return true;
				default:
					return false;
			}
		}
		public virtual decimal GetAmountFromPMBudget(RMDataSource ds, BudgetKeyTuple key)
		{
			PMBudget budget;
			if (!_budgetByKey.TryGetValue(key, out budget))
			{
				//if (key.InventoryID != PM.PMInventorySelectorAttribute.EmptyInventoryID)
				//{
				//	BudgetKeyTuple key2 = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, PM.PMInventorySelectorAttribute.EmptyInventoryID, key.CostCodeID);
				//	if (!_budgetByKey.TryGetValue(key2, out budget))
				//	{
				//		return 0;
				//	}
				//}
				//else
				{
					return 0;
				}

			}
						
			switch (ds.AmountType.Value)
			{
				case BalanceType.BudgetAmount:
					return budget.CuryAmount.Value;
				case BalanceType.BudgetQuantity:
					return budget.Qty.Value;
				case BalanceType.RevisedAmount:
					return budget.CuryRevisedAmount.Value;
				case BalanceType.RevisedQuantity:
					return budget.RevisedQty.Value;
				case BalanceType.OriginalCommittedAmount:
					return budget.CuryCommittedOrigAmount.Value;
				case BalanceType.OriginalCommittedQuantity:
					return budget.CommittedOrigQty.Value;
				case BalanceType.CommittedAmount:
					return budget.CuryCommittedAmount.Value;
				case BalanceType.CommittedQuantity:
					return budget.CommittedQty.Value;
				case BalanceType.CommittedReceivedQuantity:
					return budget.CommittedReceivedQty.Value;
				case BalanceType.CommittedInvoicedAmount:
					return budget.CuryCommittedInvoicedAmount.Value;
				case BalanceType.CommittedInvoicedQuantity:
					return budget.CommittedInvoicedQty.Value;
				case BalanceType.CommittedOpenAmount:
					return budget.CuryCommittedOpenAmount.Value;
				case BalanceType.CommittedOpenQuantity:
					return budget.CommittedOpenQty.Value;
				case BalanceType.ChangeOrderQuantity:
					return budget.ChangeOrderQty.Value;
				case BalanceType.ChangeOrderAmount:
					return budget.CuryChangeOrderAmount.Value;
				case BalanceType.DraftChangeOrderQuantity:
					return budget.DraftChangeOrderQty.Value;
				case BalanceType.DraftChangeOrderAmount:
					return budget.CuryDraftChangeOrderAmount.Value;
				default:
					return 0;
			}
		}


		[PXOverride]
        public string GetUrl(Func<string> del)
        {
            string rmType = Base.Report.Current.Type;
            if (rmType == ARmReport.PM)
            {
                PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID("CS600010");
                if (node != null)
                {
                    return PX.Common.PXUrl.TrimUrl(node.Url);
                }
                throw new PXException(ErrorMessages.NotEnoughRightsToAccessObject, "CS600010");
            }
            else
            {
                return del();
            }
        }

		public void EnsureHistoryLoaded(RMDataSourcePM dsPM)
        {
            //Unlike RMReportReaderGL, there is no lazy loading for now, given the way PMHistory is structured we need to load whole project history to get balances for a given project
            //We could do lazy loading by project, but that would be slower if report including a large number of projects (ex: project profitability list)
            var key = 1;
            if (!_historyLoaded.Contains(key))
            {
                ProcessPMResultset(PXSelectReadonly2<PMHistory, 
                    InnerJoin<PMTask, On<PMHistory.projectTaskID, Equal<PMTask.taskID>>>>.Select(this.Base));
				UpdateHistorySegmentsWithRecordsFromBudget();
				_historyLoaded.Add(key);
            }
        }

        #endregion

        #region IARmDataSource

        // Initializing IDictionary with Keys and their string represantations in static constructor for perfomance
        // (Enum.GetNames(), Enum.TryParse(), Enum.GetValues(), Enum.IsDefined() are very slow because of using Reflection)

        static RMReportReaderPM()
        {
            _keysDictionary = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToDictionary(@e => @e.ToString(), @e => @e);
        }

		private static readonly IDictionary<string, Keys> _keysDictionary;

        public enum Keys
        {
            StartAccountGroup,
            EndAccountGroup,
            StartProject,
            EndProject,
            StartProjectTask,
            EndProjectTask,
            StartInventory,
            EndInventory,
        }

        [PXOverride]
        public bool IsParameter(ARmDataSet ds, string name, ValueBucket value, Func<ARmDataSet, string, ValueBucket, bool> del)
        {
            bool flag = del(ds, name, value);
            if (!flag)
            {
                Keys key;
                if (_keysDictionary.TryGetValue(name, out key))
                {
                    value.Value = ds[key];
                    return true;
                }
                return false;
            }
            return flag;
        }

        [PXOverride]
        public ARmDataSet MergeDataSet(IEnumerable<ARmDataSet> list, string expand, MergingMode mode, Func<IEnumerable<ARmDataSet>, string, MergingMode, ARmDataSet> del)
        {
            ARmDataSet dataSet = del(list, expand, mode);

            string rmType = Base.Report.Current.Type;
            if (rmType == ARmReport.PM)
            {
                foreach (ARmDataSet set in list)
                {
                    if (set == null) continue;

					RMReportWildcard.ConcatenateRangeWithDataSet(dataSet, set, Keys.StartAccountGroup, Keys.EndAccountGroup, mode);
					RMReportWildcard.ConcatenateRangeWithDataSet(dataSet, set, Keys.StartProject, Keys.EndProject, mode);
					RMReportWildcard.ConcatenateRangeWithDataSet(dataSet, set, Keys.StartProjectTask, Keys.EndProjectTask, mode);
					RMReportWildcard.ConcatenateRangeWithDataSet(dataSet, set, Keys.StartInventory, Keys.EndInventory, mode);
                }

                dataSet.Expand = list.First().Expand;
            }

            return dataSet;
        }

        [PXOverride]
        public ARmReport GetReport(Func<ARmReport> del)
        {
            ARmReport ar = del();

            int? id = Base.Report.Current.StyleID;
            if (id != null)
            {
                RMStyle st = Base.StyleByID.SelectSingle(id);
                Base.fillStyle(st, ar.Style);
            }

            id = Base.Report.Current.DataSourceID;
            if (id != null)
            {
                RMDataSource ds = Base.DataSourceByID.SelectSingle(id);
                FillDataSourceInternal(ds, ar.DataSet, ar.Type);
            }

            List<ARmReport.ARmReportParameter> aRp = ar.ARmParams;
            PXFieldState state;
            RMReportPM rPM = Base.Report.Cache.GetExtension<RMReportPM>(Base.Report.Current);

            if (ar.Type == ARmReport.PM)
            {
                string sViewName = string.Empty;
                string sInputMask = string.Empty;

                // StartAccountGroup, EndAccountGroup
                bool RequestEndAccountGroup = rPM.RequestEndAccountGroup ?? false;
                //int colSpan = RequestEndAccountGroup ? 1 : 2;
                int colSpan = 2;
                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startAccountGroup>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.StartAccountGroup, "StartAccountGroup", Messages.GetLocal(Messages.StartAccTitle), ar.DataSet[Keys.StartAccountGroup] as string, rPM.RequestStartAccountGroup ?? false, colSpan, sViewName, sInputMask, aRp);

                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endAccountGroup>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.EndAccountGroup, "EndAccountGroup", Messages.GetLocal(Messages.EndAccTitle), ar.DataSet[Keys.EndAccountGroup] as string, RequestEndAccountGroup, colSpan, sViewName, sInputMask, aRp);

                // StartProject, EndProject
                bool bRequestEndProject = rPM.RequestEndProject ?? false;
                //colSpan = bRequestEndProject ? 1 : 2;
                colSpan = 2;
                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startProject>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.StartProject, "StartProject", Messages.GetLocal(Messages.StartProjectTitle), ar.DataSet[Keys.StartProject] as string, rPM.RequestStartProject ?? false, colSpan, sViewName, sInputMask, aRp);

                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endProject>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.EndProject, "EndProject", Messages.GetLocal(Messages.EndProjectTitle), ar.DataSet[Keys.EndProject] as string, bRequestEndProject, colSpan, sViewName, sInputMask, aRp);

                // StartTask, EndTask
                bool RequestEndProjectTask = rPM.RequestEndProjectTask ?? false;

                //colSpan = RequestEndProjectTask ? 1 : 2;
                colSpan = 2;
                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startProjectTask>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.StartProjectTask, "StartTask", Messages.GetLocal(Messages.StartTaskTitle), ar.DataSet[Keys.StartProjectTask] as string, rPM.RequestStartProjectTask ?? false, colSpan, sViewName, sInputMask, aRp);

                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endProjectTask>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.EndProjectTask, "EndTask", Messages.GetLocal(Messages.EndTaskTitle), ar.DataSet[Keys.EndProjectTask] as string, RequestEndProjectTask, colSpan, sViewName, sInputMask, aRp);

                // StartInventory, EndInventory
                bool bRequestEndInventory = rPM.RequestEndInventory ?? false;
                //colSpan = bRequestEndInventory ? 1 : 2;
                colSpan = 2;

                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.startInventory>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.StartInventory, "StartInventory", Messages.GetLocal(Messages.StartInventoryTitle), ar.DataSet[Keys.StartInventory] as string, rPM.RequestStartInventory ?? false, colSpan, sViewName, sInputMask, aRp);

                sViewName = sInputMask = string.Empty;
                state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourcePM.endInventory>(null) as PXFieldState;
                if (state != null && !String.IsNullOrEmpty(state.ViewName))
                {
                    sViewName = state.ViewName;
                    if (state is PXStringState)
                    {
                        sInputMask = ((PXStringState)state).InputMask;
                    }
                }
                Base.CreateParameter(Keys.EndInventory, "EndInventory", Messages.GetLocal(Messages.EndInventoryTitle), ar.DataSet[Keys.EndInventory] as string, bRequestEndInventory, colSpan, sViewName, sInputMask, aRp);
            }

            return ar;
        }

        [PXOverride]
        public virtual List<ARmUnit> ExpandUnit(RMDataSource ds, ARmUnit unit, Func<RMDataSource, ARmUnit, List<ARmUnit>> del)
        {
            string rmType = Base.Report.Current.Type;
            if (rmType == ARmReport.PM)
            {
                if (unit.DataSet.Expand != ExpandType.Nothing)
                {
                    PMEnsureInitialized();
                    if (ds.Expand == ExpandType.AccountGroup)
                    {
                        return RMReportUnitExpansion<PMAccountGroup>.ExpandUnit(Base, ds, unit, Keys.StartAccountGroup, Keys.EndAccountGroup,
                            GetItemsInRange<PMAccountGroup>,
                            accountGroup => accountGroup.GroupCD, accountGroup => accountGroup.Description,
	                        (accountGroup, wildcard) => { accountGroup.GroupCD = wildcard; accountGroup.Description = wildcard; });
                    }

                    if (ds.Expand == ExpandType.Project)
                    {
                        return RMReportUnitExpansion<PMProject>.ExpandUnit(Base, ds, unit, Keys.StartProject, Keys.EndProject,
                            GetItemsInRange<PMProject>,
                            project => project.ContractCD, project => project.Description,
                            (project, wildcard) => { project.ContractCD = wildcard; project.Description = wildcard; });
                    }

                    if (ds.Expand == ExpandType.ProjectTask)
                    {
                        return RMReportUnitExpansion<PMTask>.ExpandUnit(Base, ds, unit, Keys.StartProjectTask, Keys.EndProjectTask,
                            rangeToFetch => {
                                List<PMTask> tasks = GetItemsInRange<PMTask>(rangeToFetch);

                                ARmDataSet projectRange = new ARmDataSet();
                                RMReportWildcard.ConcatenateRangeWithDataSet(projectRange, unit.DataSet, Keys.StartProject, Keys.EndProject, MergingMode.Intersection);

                                if (!String.IsNullOrEmpty(projectRange[Keys.StartProject] as string))
                                {
                                    //A project range is specified in the unit; restrict tasks to tasks of this project range.
	                                List<PMProject> projects = GetItemsInRange<PMProject>(rangeToFetch);
                                    tasks = tasks.Where(t => projects.Any(p => t.ProjectID == p.ContractID)).ToList<PMTask>();
                                }

                                //Same project TaskCD can be reused in multiple projects; it only makes sense to get distinct values for the purpose of filling the unit tree
                                List<PMTask> groupedTasks = tasks.GroupBy(t => t.TaskCD).Select(g => new PMTask() { TaskCD = g.Key, Description = g.Min(t => t.Description) }).ToList<PMTask>();
                                return groupedTasks;
                            },
                            task => task.TaskCD, project => project.Description,
                            (task, wildcard) => { task.TaskCD = wildcard; task.Description = wildcard; });
                    }

                    if (ds.Expand == ExpandType.Inventory)
                    {
                        return RMReportUnitExpansion<InventoryItem>.ExpandUnit(Base, ds, unit, Keys.StartInventory, Keys.EndInventory,
                            GetItemsInRange<InventoryItem>,
                            item => item.InventoryCD, item => item.Descr,
                            (item, wildcard) => { item.InventoryCD = wildcard; item.Descr = wildcard; });
                    }
                }
            }
            else
            {
                return del(ds, unit);
            }

            return null;
        }

        [PXOverride]
        public void FillDataSource(RMDataSource ds, ARmDataSet dst, string rmType, Action<RMDataSource, ARmDataSet, string> del)
        {
            del(ds, dst, rmType);
            
            if (rmType == ARmReport.PM)
            {
                FillDataSourceInternal(ds, dst, rmType);
            }
        }

        private void FillDataSourceInternal(RMDataSource ds, ARmDataSet dst, string rmType)
        {
            if (ds != null && ds.DataSourceID != null)
            {
                RMDataSourcePM dsPM = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourcePM>(ds);
                dst[Keys.StartAccountGroup] = dsPM.StartAccountGroup;
                dst[Keys.EndAccountGroup] = dsPM.EndAccountGroup;
                dst[Keys.StartProject] = dsPM.StartProject;
                dst[Keys.EndProject] = dsPM.EndProject;
                dst[Keys.StartProjectTask] = dsPM.StartProjectTask;
                dst[Keys.EndProjectTask] = dsPM.EndProjectTask;
                dst[Keys.StartInventory] = dsPM.StartInventory;
                dst[Keys.EndInventory] = dsPM.EndInventory;
            }
        }

        private ArmDATA _Data;
        [PXOverride]
        public object GetExprContext()
        {
            if (_Data == null)
            {
                _Data = new ArmDATA();
            }
            return _Data;
        }

        #endregion
    }

    public struct PMHistoryKeyTuple
    {
        public readonly int ProjectID;
        public readonly string ProjectTaskCD;
        public readonly int AccountGroupID;
        public readonly int InventoryID;
		public readonly int CostCodeID;

		public PMHistoryKeyTuple(int projectID, string projectTaskCD, int accountGroupID, int inventoryID, int costCodeID)
        {
            ProjectID = projectID;
            ProjectTaskCD = projectTaskCD;
            AccountGroupID = accountGroupID;
            InventoryID = inventoryID;
			CostCodeID = costCodeID;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + ProjectID.GetHashCode();
                hash = hash * 23 + ProjectTaskCD.GetHashCode();
                hash = hash * 23 + AccountGroupID.GetHashCode();
                hash = hash * 23 + InventoryID.GetHashCode();
				hash = hash * 23 + CostCodeID.GetHashCode();
				return hash;
            }
        }
    }

	[System.Diagnostics.DebuggerDisplay("{ProjectID}.{ProjectTaskID}.{AccountGroupID}.{InventoryID}.{CostCodeID}")]
	public struct BudgetKeyTuple
	{
		public readonly int ProjectID;
		public readonly int ProjectTaskID;
		public readonly int AccountGroupID;
		public readonly int InventoryID;
		public readonly int CostCodeID;

		public BudgetKeyTuple(int projectID, int projectTaskID, int accountGroupID, int inventoryID, int costCodeID)
		{
			ProjectID = projectID;
			ProjectTaskID = projectTaskID;
			AccountGroupID = accountGroupID;
			InventoryID = inventoryID;
			CostCodeID = costCodeID;
		}

		public static BudgetKeyTuple Create(IProjectFilter budget)
		{
			return new BudgetKeyTuple(
				budget.ProjectID.GetValueOrDefault(),
				budget.TaskID.GetValueOrDefault(),
				budget.AccountGroupID.GetValueOrDefault(),
				budget.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID),
				budget.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				hash = hash * 23 + ProjectID.GetHashCode();
				hash = hash * 23 + ProjectTaskID.GetHashCode();
				hash = hash * 23 + AccountGroupID.GetHashCode();
				hash = hash * 23 + InventoryID.GetHashCode();
				hash = hash * 23 + CostCodeID.GetHashCode();
				return hash;
			}
		}
	}
	
}
