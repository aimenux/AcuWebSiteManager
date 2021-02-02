using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectAccounting.PM.GraphExtensions
{
    public class ForecastMaintExt : PXGraphExtension<ForecastMaint>
    {
        public PXAction<PMForecast> Report;
        public PXAction<PMForecast> BudgetForecastReport;

        public override void Initialize()
        {
            base.Initialize();
            Report.AddMenuAction(BudgetForecastReport);
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXButton]
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        protected void report()
        {
        }

        [PXUIField(DisplayName = "Project Budget Forecast By Month", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.Report)]
        protected virtual IEnumerable budgetForecastReport(PXAdapter adapter)
        {
            Base.Save.Press();
            var filter = Base.Filter.Current;
            var parameters = new Dictionary<string, string>();
            SetProject(filter, parameters);
            SetAccountGroup(filter, parameters);
            SetTask(filter, parameters);
            SetInventoryItem(filter, parameters);
            SetCostCode(filter, parameters);
            parameters["RevisionId"] = Base.Revisions.Current.RevisionID;
            parameters["AccountGroupType"] = filter.AccountGroupType;
            throw new PXReportRequiredException(parameters, Constants.ReportIds.BudgetForecast);
        }

        private void SetProject(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var project = Base.Select<PMProject>().SingleOrDefault(p => p.ContractID == filter.ProjectID);
            parameters["ProjectId"] = filter.ProjectID != null && project != null
                ? project.ContractCD
                : string.Empty;
        }

        private void SetAccountGroup(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var group = Base.Select<PMAccountGroup>().SingleOrDefault(g => g.GroupID == filter.AccountGroupID);
            parameters["AccountGroupId"] = filter.AccountGroupID != null && group != null
                ? group.GroupCD
                : string.Empty;
        }

        private void SetTask(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var task = Base.Select<PMTask>().SingleOrDefault(t => t.TaskID == filter.ProjectTaskID);
            parameters["ProjectTaskId"] = filter.ProjectTaskID != null && task != null
                ? task.TaskCD
                : string.Empty;
        }

        private void SetInventoryItem(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            InventoryItem item =
                SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<P.AsInt>>.View.Select(Base,
                    filter.InventoryID);
            parameters["InventoryId"] = filter.InventoryID != null && item != null
                ? item.InventoryCD
                : string.Empty;
        }

        private void SetCostCode(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var code = Base.Select<PMCostCode>().SingleOrDefault(c => c.CostCodeID == filter.CostCodeID);
            parameters["CostCodeId"] = filter.ProjectID != null && code != null
                ? code.CostCodeCD
                : string.Empty;
        }
    }
}