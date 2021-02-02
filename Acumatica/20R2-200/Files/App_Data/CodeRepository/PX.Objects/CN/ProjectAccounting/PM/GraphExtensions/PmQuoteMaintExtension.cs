using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class PmQuoteMaintExtension : PXGraphExtension<PMQuoteMaint>
    {
        [InjectDependency]
        public IProjectTaskDataProvider ProjectTaskDataProvider
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public virtual void AddingTasksToProject(PMQuote quote, ProjectEntry projectEntry,
            Dictionary<string, int> taskMap, Action<PMQuote, ProjectEntry, Dictionary<string, int>> baseHandler)
        {
            baseHandler(quote, projectEntry, taskMap);
            var quoteTasks = Base.Tasks.Select().FirstTableItems;
            var projectTasks = projectEntry.Tasks.Select().FirstTableItems;
            projectTasks.ForEach(t => CopyTaskType(t, quoteTasks));
        }

        [PXOverride]
        public virtual void RedefaultTasksFromTemplate(PMQuote quote, Action<PMQuote> baseHandler)
        {
            Base.Tasks.Cache.ClearQueryCache();
            var tasks = ProjectTaskDataProvider.GetProjectTasks(Base, quote.TemplateID);
            foreach (var task in tasks)
            {
                var quoteTask = InsertQuoteTask(quote, task);
                PXDBLocalizableStringAttribute.CopyTranslations<PMTask.description, PMQuoteTask.description>(
                    Base.Caches<PMTask>(), task, Base.Tasks.Cache, quoteTask);
            }
        }

        private PMQuoteTask InsertQuoteTask(PMQuote quote, PMTask task)
        {
            var quoteTask = CreateQuoteTask(quote, task);
            quoteTask = Base.Tasks.Insert(quoteTask);
            quoteTask.Type = task.Type;
            return quoteTask;
        }

        private static PMQuoteTask CreateQuoteTask(PMQuote quote, PMTask task)
        {
            return new PMQuoteTask
            {
                QuoteID = quote.QuoteID,
                TaskCD = task.TaskCD,
                Description = task.Description,
                IsDefault = task.IsDefault,
                TaxCategoryID = task.TaxCategoryID
            };
        }

        private static void CopyTaskType(PMTask task, IEnumerable<PMQuoteTask> quoteTasks)
        {
            var relatedQuoteTask = quoteTasks.SingleOrDefault(qt => qt.TaskCD == task.TaskCD);
            task.Type = relatedQuoteTask?.Type ?? ProjectTaskType.CostRevenue;
        }
    }
}
