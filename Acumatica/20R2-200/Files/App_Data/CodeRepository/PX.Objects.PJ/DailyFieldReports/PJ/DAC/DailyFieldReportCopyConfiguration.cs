using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Copy Configuration")]
    public class DailyFieldReportCopyConfiguration : BaseCache, IBqlTable
    {
        [PXDBBool]
        [PXUIField(DisplayName = "Override Default Copy-Paste Settings")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? IsConfigurationEnabled
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Notes", false)]
        public bool? Notes
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Date", false)]
        public bool? Date
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Project Manager", false)]
        public bool? ProjectManager
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Employee", true, IsRequired = true)]
        public bool? Employee
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Earning Type", true, IsRequired = true)]
        public bool? EmployeeEarningType
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Project Task", true, IsRequired = true)]
        public bool? EmployeeProjectTask
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Cost Code", true, IsRequired = true)]
        public bool? EmployeeCostCode
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Time", true, IsRequired = true)]
        public bool? EmployeeTime
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Time Spent", false, IsRequired = true)]
        public bool? EmployeeTimeSpent
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Billable", true, IsRequired = true)]
        public bool? EmployeeIsBillable
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Billable Time", false, IsRequired = true)]
        public bool? EmployeeBillableTime
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Description", false, IsRequired = true)]
        public bool? EmployeeDescription
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Task", false)]
        public bool? EmployeeTask
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Certified Job", false)]
        public bool? EmployeeCertifiedJob
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Union Local", false)]
        public bool? EmployeeUnionLocal
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Labor Item", false)]
        public bool? EmployeeLaborItem
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("WCC Code", false)]
        public bool? EmployeeWccCode
        {
            get;
            set;
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.timeReportingModule>))]
        [DailyFieldReportCopyConfiguration("Contract", false)]
        public bool? EmployeeContract
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Vendor ID", true, IsRequired = true)]
        public bool? SubcontractorVendorId
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Project Task", true, IsRequired = true)]
        public bool? SubcontractorProjectTask
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Cost Code", true, IsRequired = true)]
        public bool? SubcontractorCostCode
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Number of Workers", true, IsRequired = true)]
        public bool? SubcontractorNumberOfWorkers
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Arrived", false, IsRequired = true)]
        public bool? SubcontractorTimeArrived
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Departed", false, IsRequired = true)]
        public bool? SubcontractorTimeDeparted
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Working Hours", false, IsRequired = true)]
        public bool? SubcontractorWorkingHours
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Description", false, IsRequired = true)]
        public bool? SubcontractorDescription
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Equipment ID", true, IsRequired = true)]
        public bool? EquipmentId
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Project Task", true, IsRequired = true)]
        public bool? EquipmentProjectTask
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Cost Code", true, IsRequired = true)]
        public bool? EquipmentCostCode
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Billable", true)]
        public bool? EquipmentIsBillable
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Setup Time", false)]
        public bool? EquipmentSetupTime
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Run Time", false)]
        public bool? EquipmentRunTime
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Suspend Time", false)]
        public bool? EquipmentSuspendTime
        {
            get;
            set;
        }

        [DailyFieldReportCopyConfiguration("Description", true)]
        public bool? EquipmentDescription
        {
            get;
            set;
        }

        public abstract class isConfigurationEnabled : BqlBool.Field<isConfigurationEnabled>
        {
        }

        public abstract class notes : BqlBool.Field<notes>
        {
        }

        public abstract class date : BqlBool.Field<date>
        {
        }

        public abstract class projectManager : BqlBool.Field<projectManager>
        {
        }

        public abstract class employee : BqlBool.Field<employee>
        {
        }

        public abstract class employeeEarningType : BqlBool.Field<employeeEarningType>
        {
        }

        public abstract class employeeProjectTask : BqlBool.Field<employeeProjectTask>
        {
        }

        public abstract class employeeCostCode : BqlBool.Field<employeeCostCode>
        {
        }

        public abstract class employeeTime : BqlBool.Field<employeeTime>
        {
        }

        public abstract class employeeTimeSpent : BqlBool.Field<employeeTimeSpent>
        {
        }

        public abstract class employeeIsBillable : BqlBool.Field<employeeIsBillable>
        {
        }

        public abstract class employeeBillableTime : BqlBool.Field<employeeBillableTime>
        {
        }

        public abstract class employeeDescription : BqlBool.Field<employeeDescription>
        {
        }

        public abstract class employeeTask : BqlBool.Field<employeeTask>
        {
        }

        public abstract class employeeCertifiedJob : BqlBool.Field<employeeCertifiedJob>
        {
        }

        public abstract class employeeUnionLocal : BqlBool.Field<employeeUnionLocal>
        {
        }

        public abstract class employeeLaborItem : BqlBool.Field<employeeLaborItem>
        {
        }

        public abstract class employeeWccCode : BqlBool.Field<employeeWccCode>
        {
        }

        public abstract class employeeContract : BqlBool.Field<employeeContract>
        {
        }

        public abstract class subcontractorVendorId : BqlBool.Field<subcontractorVendorId>
        {
        }

        public abstract class subcontractorProjectTask : BqlBool.Field<subcontractorProjectTask>
        {
        }

        public abstract class subcontractorCostCode : BqlBool.Field<subcontractorCostCode>
        {
        }

        public abstract class subcontractorNumberOfWorkers : BqlBool.Field<subcontractorNumberOfWorkers>
        {
        }

        public abstract class subcontractorTimeArrived : BqlBool.Field<subcontractorTimeArrived>
        {
        }

        public abstract class subcontractorTimeDeparted : BqlBool.Field<subcontractorTimeDeparted>
        {
        }

        public abstract class subcontractorWorkingHours : BqlBool.Field<subcontractorWorkingHours>
        {
        }

        public abstract class subcontractorDescription : BqlBool.Field<subcontractorDescription>
        {
        }

        public abstract class equipmentId : BqlBool.Field<equipmentId>
        {
        }

        public abstract class equipmentProjectTask : BqlBool.Field<equipmentProjectTask>
        {
        }

        public abstract class equipmentCostCode : BqlBool.Field<equipmentCostCode>
        {
        }

        public abstract class equipmentIsBillable : BqlBool.Field<equipmentIsBillable>
        {
        }

        public abstract class equipmentSetupTime : BqlBool.Field<equipmentSetupTime>
        {
        }

        public abstract class equipmentRunTime : BqlBool.Field<equipmentRunTime>
        {
        }

        public abstract class equipmentSuspendTime : BqlBool.Field<equipmentSuspendTime>
        {
        }

        public abstract class equipmentDescription : BqlBool.Field<equipmentDescription>
        {
        }
    }
}