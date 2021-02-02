using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class ProjectEntryExt : PXGraphExtension<ProjectEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }


        public PXAction<PMProject> aia;
        [PXUIField(DisplayName = "AIA Report")]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false)]
        protected virtual IEnumerable Aia(PXAdapter adapter)
        {
            if (Base.Invoices.Current != null && !string.IsNullOrEmpty(Base.Invoices.Current.ProformaRefNbr))
            {
                ProformaEntry entry = PXGraph.CreateInstance<ProformaEntry>();
                ProformaEntryExt ext = entry.GetExtension<ProformaEntryExt>();
                entry.Document.Current = PXSelect<PMProforma, Where<PMProforma.refNbr, 
                    Equal<Current<PMBillingRecord.proformaRefNbr>>,
                    And<PMProforma.corrected, NotEqual<True>>>>.Select(Base);
                ext.aia.Press();
            }

            return adapter.Get();
        }

        [PXOverride]
        public virtual IEnumerable CreateTemplate(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            try
            {
                return baseHandler(adapter);
            }
            catch (PXRedirectRequiredException exception)
            {
                CopyTaskTypes((TemplateMaint) exception.Graph);
                throw;
            }
        }

        [PXOverride]
        public virtual PMTask CopyTask(PMTask originalTask, int projectId,
            ProjectEntry.DefaultFromTemplateSettings settings,
            Func<PMTask, int, ProjectEntry.DefaultFromTemplateSettings, PMTask> baseHandler)
        {
            var targetTask = baseHandler(originalTask, projectId, settings);
            targetTask.Type = originalTask.Type;
            return targetTask;
        }

       
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.cost>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMRevenueBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.cost>>>>>))]
        [ProjectTaskTypeValidation(
            ProjectTaskIdField = typeof(PMRevenueBudget.projectTaskID),
            Message = ProjectAccountingMessages.RevenueTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Cost)]
        protected virtual void _(Events.CacheAttached<PMRevenueBudget.projectTaskID> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMCostBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>>))]
        [ProjectTaskTypeValidation(
            ProjectTaskIdField = typeof(PMCostBudget.projectTaskID),
            Message = ProjectAccountingMessages.CostTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Revenue)]
        protected virtual void _(Events.CacheAttached<PMCostBudget.projectTaskID> e)
        {
        }

        public PXAction<PMProject> costProjection;
        [PXUIField(DisplayName = "Cost Projection", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable CostProjection(PXAdapter adapter)
        {
            if (Base.Project.Current != null)
            {
                CostProjectionEntry graph = PXGraph.CreateInstance<CostProjectionEntry>();
                var select = new PXSelect<PMCostProjection, Where<PMCostProjection.projectID, Equal<Current<PMProject.contractID>>>, OrderBy<Desc<PMCostProjection.date>>>(Base);
                PMCostProjection exists = select.Select();
                if (exists != null)
                {
                    graph.Document.Current = exists;
                }
                else
                {
                    graph.Document.Insert();
                    graph.Document.Current.ProjectID = Base.Project.Current.ContractID;
                    graph.Document.Cache.IsDirty = false;
                }

                throw new PXRedirectRequiredException(graph, "CostProjection");
            }
            return adapter.Get();
        }

        [PXOverride]
        public virtual void InitializeInquiriesMenu(Action baseMethod)
        {
            baseMethod();
            Base.inquiry.AddMenuAction(costProjection);
        }

        protected virtual void _(Events.RowPersisting<PMTask> args)
        {
            var projectTask = args.Row;
            if (projectTask != null)
            {
                var projectTaskTypeUsageService = new ProjectTaskTypeUsageInConstructionValidationService();
                projectTaskTypeUsageService.ValidateProjectTaskType(args.Cache, projectTask);
            }
        }

        protected virtual void _(Events.RowDeleting<PMTask> args)
        {
            var projectTask = args.Row;
            if (projectTask != null)
            {
                var projectTaskUsageService = new ProjectTaskUsageInConstructionValidationService();
                projectTaskUsageService.ValidateProjectTask(projectTask);
            }
        }

        protected virtual void _(Events.RowSelected<PMBillingRecord> e)
        {
            if (e.Row != null && !string.IsNullOrEmpty(e.Row.ProformaRefNbr) && IsAIAOutdated(e.Row.ProjectID, e.Row.ProformaRefNbr))
            {
                PXUIFieldAttribute.SetWarning(e.Cache, e.Row, nameof(PMBillingRecord.ProformaRefNbr), PX.Objects.PM.Messages.AIAIsOutdated);
            }
        }

        

        protected int? outdatedForProject;
        protected HashSet<string> outdatedAIA;
        protected virtual bool IsAIAOutdated(int? projectID, string proformaRefNbr)
        {
            if (outdatedForProject != null && outdatedForProject != projectID)
            {
                outdatedForProject = null;
                outdatedAIA = null;
            }

            if (outdatedAIA == null)
            {
                var select = new PXSelect<PMProforma,
                    Where<PMProforma.projectID, Equal<Required<PMProject.contractID>>,
                    And<PMProforma.isAIAOutdated, Equal<True>>>>(Base);

                outdatedForProject = projectID;
                outdatedAIA = new HashSet<string>();
                foreach (PMProforma proforma in select.Select(projectID))
                {
                    outdatedAIA.Add(proforma.RefNbr);
                }
            }

            return outdatedAIA.Contains(proformaRefNbr);
        }

        protected virtual void _(Events.RowInserted<PMCostBudget> e)
        {
            if (Base.IsCopyPaste)
            {
                e.Cache.SetValue<PMCostBudget.costProjectionCompletedPct>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionCostAtCompletion>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionCostToComplete>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionQtyAtCompletion>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionQtyToComplete>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.curyCostProjectionCostAtCompletion>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.curyCostProjectionCostToComplete>(e.Row, 0m);
            }
        }
        private void CopyTaskTypes(TemplateMaint templateMaint)
        {
            var originalTasks = Base.Tasks.Select().FirstTableItems.ToList();
            var insertedTasks = (IEnumerable<PMTask>) templateMaint.Tasks.Cache.Inserted;
            insertedTasks.ForEach(task => CopyTaskType(task, originalTasks));
        }

        private static void CopyTaskType(PMTask insertedTask, IEnumerable<PMTask> originalTasks)
        {
            var originalTask = originalTasks.Single(t => t.TaskCD == insertedTask.TaskCD);
            insertedTask.Type = originalTask.Type;
        }
    }
}
