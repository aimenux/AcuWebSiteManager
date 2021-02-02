using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.AM
{
    public class AMEmployeeCostEngine : EmployeeCostEngine
    {
        public AMEmployeeCostEngine(PXGraph graph) : base(graph)
        {
        }

        public virtual EmployeeCostEngine.Rate GetEmployeeRate(int? projectID, int? projectTaskID, int? employeeId, DateTime? date)
        {
            // When non-project cost code the task will be null, so lets only check a project when the task contains a value.
            var useProjectId = projectTaskID == null ? null : projectID;

            //public virtual EmployeeCostEngine.Rate GetEmployeeRate(int? laborItemID, int? projectID, int? projectTaskID, bool? certifiedJob, string unionID, int? employeeId, DateTime? date)
            return base.GetEmployeeRate(null, useProjectId, projectTaskID, null, null, employeeId, date);
        }

        public virtual decimal? GetEmployeeHourlyRate(int? projectID, int? projectTaskID, int? employeeId, DateTime? date)
        {
            return GetEmployeeRate(projectID, projectTaskID, employeeId, date)?.HourlyRate;
        }
    }
}