using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Services
{
    public class DailyFieldReportCopyConfigurationService
    {
        private readonly ICollection<(string objectName, string fieldName)> excludedFields;
        private readonly DailyFieldReportCopyConfiguration dailyFieldReportCopyConfiguration;

        public DailyFieldReportCopyConfigurationService(PXGraph graph)
        {
            excludedFields = new List<(string, string)>();
            dailyFieldReportCopyConfiguration = GetCopyConfiguration(graph);
        }

        public void ConfigureCopyPasteFields(List<Command> script, List<Container> containers)
        {
            ExcludeDailyFieldReportFields();
            ExcludeEmployeeActivityFields();
            ExcludeSubcontractorFields();
            ExcludeEquipmentFields();
            var indicesToRemove = script.SelectIndexesWhere(command =>
                excludedFields.Contains((command.ObjectName, command.FieldName))).Reverse();
            foreach (var index in indicesToRemove)
            {
                script.RemoveAt(index);
                containers.RemoveAt(index);
            }
        }

        private void ExcludeDailyFieldReportFields()
        {
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.Date,
                nameof(DailyFieldReportEntry.DailyFieldReport), nameof(DailyFieldReport.Date));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.ProjectManager,
                nameof(DailyFieldReportEntry.DailyFieldReport), nameof(DailyFieldReport.ProjectManagerId));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.Notes,
                nameof(DailyFieldReportEntry.DailyFieldReport), nameof(Note.NoteText));
        }

        private void ExcludeEmployeeActivityFields()
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
            {
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeTime,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    GetTimeFieldFullName(nameof(EPActivityApprove.Date)));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.Employee,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.OwnerID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeEarningType,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.EarningTypeID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeProjectTask,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.ProjectTaskID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeCostCode,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.CostCodeID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeTimeSpent,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.TimeSpent));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeIsBillable,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.IsBillable));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeBillableTime,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.TimeBillable));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeDescription,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.Summary));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeTask,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.ParentTaskNoteID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeCertifiedJob,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.CertifiedJob));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeUnionLocal,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.UnionID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeLaborItem,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.LabourItemID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeWccCode,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.WorkCodeID));
                ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EmployeeContract,
                    nameof(DailyFieldReportEntryEmployeeActivityExtension.EmployeeActivities),
                    nameof(EPActivityApprove.ContractID));
            }
        }

        private void ExcludeSubcontractorFields()
        {
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorVendorId,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                nameof(DailyFieldReportSubcontractorActivity.VendorId));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorProjectTask,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                nameof(DailyFieldReportSubcontractorActivity.ProjectTaskID));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorCostCode,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                nameof(DailyFieldReportSubcontractorActivity.CostCodeId));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorNumberOfWorkers,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                nameof(DailyFieldReportSubcontractorActivity.NumberOfWorkers));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorTimeArrived,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                GetTimeFieldFullName(nameof(DailyFieldReportSubcontractorActivity.TimeArrived)));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorTimeDeparted,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                GetTimeFieldFullName(nameof(DailyFieldReportSubcontractorActivity.TimeDeparted)));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorWorkingHours,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                nameof(DailyFieldReportSubcontractorActivity.WorkingTimeSpent));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.SubcontractorDescription,
                nameof(DailyFieldReportEntrySubcontractorActivityExtension.Subcontractors),
                nameof(DailyFieldReportSubcontractorActivity.Description));
        }

        private void ExcludeEquipmentFields()
        {
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentId,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.EquipmentId));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentProjectTask,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.ProjectTaskID));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentCostCode,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.CostCodeID));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentIsBillable,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.IsBillable));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentSetupTime,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.SetupTime));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentRunTime,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.RunTime));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentSuspendTime,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.SuspendTime));
            ExcludeFieldIfNeeded(dailyFieldReportCopyConfiguration.EquipmentDescription,
                nameof(DailyFieldReportEntryEquipmentExtension.Equipment),
                nameof(EquipmentProjection.Description));
        }

        private void ExcludeFieldIfNeeded(bool? value, string objectName, string fieldName)
        {
            if (value != true)
            {
                excludedFields.Add((objectName, fieldName));
            }
        }

        private DailyFieldReportCopyConfiguration GetCopyConfiguration(PXGraph graph)
        {
            return SelectFrom<DailyFieldReportCopyConfiguration>.View.Select(graph);
        }

        private static string GetTimeFieldFullName(string fieldName)
        {
            return string.Concat(fieldName, PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX);
        }
    }
}