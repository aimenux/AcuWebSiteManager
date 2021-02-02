using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.SERVICEMANAGEMENT_SETUP)]
    [PXPrimaryGraph(typeof(SetupMaint))]
	public class FSSetup : PX.Data.IBqlTable
	{
        public const string ServiceManagementFieldClass = "SERVICEMANAGEMENT";
        public const string EquipmentManagementFieldClass = "EQUIPMENTMANAGEMENT";
        public const string RouteManagementFieldClass = "ROUTEMANAGEMENT";

        #region AppAutoConfirmGap
        public abstract class appAutoConfirmGap : PX.Data.BQL.BqlInt.Field<appAutoConfirmGap> { }				

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
		[PXDefault(ID.TimeConstants.HOURS_12, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Appointment Auto-Confirm Time")]
		public virtual int? AppAutoConfirmGap { get; set; }
		#endregion		                
        #region AppResizePrecision
        public abstract class appResizePrecision : ListField_AppResizePrecision
		{
		}			

	    [PXDBInt]
		[PXDefault(ID.TimeConstants.MINUTES_30)]
        [appResizePrecision.ListAtrribute]
		[PXUIField(DisplayName = "Appointment Resize Precision")]
		public virtual int? AppResizePrecision { get; set; }
		#endregion		                
        #region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }

        [PXDefault]
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Work Calendar")]
        [PXSelector(typeof(CSCalendar.calendarID), DescriptionField = typeof(CSCalendar.description))]
		public virtual string CalendarID { get; set; }
		#endregion        
        #region ShowServiceOrderDaysGap
        public abstract class showServiceOrderDaysGap : PX.Data.BQL.BqlInt.Field<showServiceOrderDaysGap> { }

        [PXDefault(14, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBInt(MinValue = 0, MaxValue = 1000)]
        [PXUIField(DisplayName = "Show Service Orders in a Period Of", Visible = true)]
        public virtual int? ShowServiceOrderDaysGap { get; set; }
        #endregion
        #region DenyWarnByGeoZone
        public abstract class denyWarnByGeoZone : ListField_AppointmentValidation
        {
		}		

		[PXDBString(1, IsFixed = true)]
        [denyWarnByGeoZone.ListAttribute]
        [PXDefault(ID.ValidationType.NOT_VALIDATE)]
        [PXUIField(DisplayName = "Service Areas")]
        public virtual string DenyWarnByGeoZone { get; set; }
		#endregion
		#region DenyWarnByLicense
        public abstract class denyWarnByLicense : ListField_AppointmentValidation
        {
		}		

		[PXDBString(1, IsFixed = true)]
        [denyWarnByLicense.ListAttribute]
        [PXDefault(ID.ValidationType.NOT_VALIDATE)]
        [PXUIField(DisplayName = "Licenses")]
        public virtual string DenyWarnByLicense { get; set; }
		#endregion
		#region DenyWarnBySkill
        public abstract class denyWarnBySkill : ListField_AppointmentValidation
        {
		}		

		[PXDBString(1, IsFixed = true)]
        [denyWarnBySkill.ListAttribute]
		[PXDefault(ID.ValidationType.NOT_VALIDATE)]
		[PXUIField(DisplayName = "Skills")]
        public virtual string DenyWarnBySkill { get; set; }
		#endregion              
        #region DfltAppContactInfoSource
        public abstract class dfltAppContactInfoSource : PX.Data.BQL.BqlString.Field<dfltAppContactInfoSource> { }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Source_Info.BUSINESS_ACCOUNT)]
        [PXUIField(DisplayName = "Default Appointment Contact Info Source", Visible = false)]
        public virtual string DfltAppContactInfoSource { get; set; }
		#endregion        
        #region EmpSchedulePrecision
        public abstract class empSchedulePrecision : PX.Data.BQL.BqlInt.Field<empSchedulePrecision> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Employee Schedule Precision", Visible = false)]
        public virtual int? EmpSchedulePrecision { get; set; }
        #endregion
        #region InitialAppRefNbr
        public abstract class initialAppRefNbr : PX.Data.BQL.BqlString.Field<initialAppRefNbr> { }

        [PXDBString(4, InputMask = "9999")]
        [PXDefault("1")]
        [PXUIField(DisplayName = "Initial Appointment Ref. Nbr.")]
        public virtual string InitialAppRefNbr { get; set; }
        #endregion
        #region DfltBusinessAcctType
        public abstract class dfltBusinessAcctType : ListField_SrvOrdType_NewBusinessAcctType
        {
        }

        [PXDBString(10)]
        [dfltBusinessAcctType.ListAtrribute]
        [PXDefault(ID.BusinessAcctType.CUSTOMER)]
        [PXUIField(DisplayName = "Default Business Account Type")]
        public virtual string DfltBusinessAcctType { get; set; }
        #endregion
        #region DfltSrvOrdType
        public abstract class dfltSrvOrdType : PX.Data.BQL.BqlString.Field<dfltSrvOrdType> { }

        [PXDBString(4, IsUnicode = true, IsFixed = true)]
        [PXUIField(DisplayName = "Default Service Order Type")]
        [FSSelectorSrvOrdTypeNOTQuote]
        public virtual string DfltSrvOrdType { get; set; }
        #endregion
        #region DfltSOSrvOrdType
        public abstract class dfltSOSrvOrdType : PX.Data.BQL.BqlString.Field<dfltSOSrvOrdType> { }

        [PXDBString(4, IsUnicode = true, IsFixed = true)]
        [PXUIField(DisplayName = "Default Service Order Type for Sales Orders")]
        [FSSelectorActiveSrvOrdType]
        public virtual string DfltSOSrvOrdType { get; set; }
        #endregion         
        #region DfltCalendarMode
        public abstract class dfltCalendarViewMode : PX.Data.BQL.BqlString.Field<dfltCalendarViewMode> { }

        [PXDBString(2, IsUnicode = true, IsFixed = true)]
        [PXDefault(ID.DfltCalendarViewMode_Setup.HORIZONTAL)]
        [PXUIField(DisplayName = "View Mode")]
        [ListField_DfltCalendarViewMode.ListAtrribute]
        public virtual string DfltCalendarViewMode { get; set; }
        #endregion
        #region DfltCasesSrvOrdType
        public abstract class dfltCasesSrvOrdType : PX.Data.BQL.BqlString.Field<dfltCasesSrvOrdType> { }

        [PXDBString(4, IsUnicode = true, IsFixed = true)]
        [PXUIField(DisplayName = "Default Service Order Type for Cases")]
        [FSSelectorActiveSrvOrdType]
        public virtual string DfltCasesSrvOrdType { get; set; }
        #endregion         
        #region DfltOpportunitySrvOrdType
        public abstract class dfltOpportunitySrvOrdType : PX.Data.BQL.BqlString.Field<dfltOpportunitySrvOrdType> { }

        [PXDBString(4, IsUnicode = true)]
        [PXUIField(DisplayName = "Default Opportunities Service Order Type")]
        [FSSelectorActiveSrvOrdType]
        public virtual string DfltOpportunitySrvOrdType { get; set; }
        #endregion  
        #region DaysAheadRecurringAppointments
        public abstract class daysAheadRecurringAppointments : PX.Data.BQL.BqlInt.Field<daysAheadRecurringAppointments> { }

        [PXDBInt]
        [PXDefault(1)]
        [PXUIField(DisplayName = "Number of days ahead for recurring appointments", Visible = false)]
        public virtual int? DaysAheadRecurringAppointments { get; set; }
        #endregion 
        #region DenyWarnByEquipment
        public abstract class denyWarnByEquipment : ListField_AppointmentValidation
        {
        }

        [PXDBString(1, IsFixed = true)]
        [denyWarnByEquipment.ListAttribute]
        [PXDefault(ID.ValidationType.NOT_VALIDATE)]
        [PXUIField(DisplayName = "Equipments")]
        public virtual string DenyWarnByEquipment { get; set; }
        #endregion        
        #region EmpSchdlNumberingID
        public abstract class empSchdlNumberingID : PX.Data.BQL.BqlString.Field<empSchdlNumberingID> { }

        [PXDBString(10)]
        [PXDefault]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Staff Schedule Numbering Sequence")]
        public virtual string EmpSchdlNumberingID { get; set; }
        #endregion
        #region LicenseNumberingID
        public abstract class licenseNumberingID : PX.Data.BQL.BqlString.Field<licenseNumberingID> { }

        [PXDBString(10)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "License Numbering Sequence")]
        public virtual string LicenseNumberingID { get; set; }
        #endregion
        #region EquipmentNumberingID
        public abstract class equipmentNumberingID : PX.Data.BQL.BqlString.Field<equipmentNumberingID> { }

        [PXDBString(10)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Equipment Numbering Sequence")]
        public virtual string EquipmentNumberingID { get; set; }
        #endregion
        #region DenyWarnByAppOverlap
        public abstract class denyWarnByAppOverlap : ListField_AppointmentValidation
        {
        }

        [PXDBString(1, IsFixed = true)]
        [denyWarnByAppOverlap.ListAttribute]
        [PXDefault(ID.ValidationType.NOT_VALIDATE)]
        [PXUIField(DisplayName = "Overlapping Appointments")]
        public virtual string DenyWarnByAppOverlap { get; set; }
        #endregion
        #region ManageRooms
        public abstract class manageRooms : PX.Data.BQL.BqlBool.Field<manageRooms> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Rooms")]
        public virtual bool? ManageRooms { get; set; }
        #endregion
        #region DispatchBoardHelper
        #region DfltBranchID
        public abstract class dfltBranchID : PX.Data.BQL.BqlInt.Field<dfltBranchID> { }

        [PXInt]
        public virtual int? DfltBranchID { get; set; }
        #endregion
        #endregion
        #region EnableEmpTimeCardIntegration
        public abstract class enableEmpTimeCardIntegration : PX.Data.BQL.BqlBool.Field<enableEmpTimeCardIntegration> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Time & Expenses Integration")]
        [PXUIVisible(typeof(IIf<FeatureInstalled<FeaturesSet.timeReportingModule>, True, False>))]
        public virtual bool? EnableEmpTimeCardIntegration { get; set; }
        #endregion
        #region PostBatchNumberingID
        public abstract class postBatchNumberingID : PX.Data.BQL.BqlString.Field<postBatchNumberingID> { }

        [PXDBString(10)]
        [PXDefault]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Batch Numbering Sequence")]
        public virtual string PostBatchNumberingID { get; set; }
        #endregion
        #region ScheduleNumberingID
        public abstract class scheduleNumberingID : PX.Data.IBqlField
        {
        }
        protected String _ScheduleNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("FSSCHEDULE")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Service Contract Schedule Numbering Sequence")]
        public virtual String ScheduleNumberingID
        {
            get
            {
                return this._ScheduleNumberingID;
            }
            set
            {
                this._ScheduleNumberingID = value;
            }
        }
        #endregion
        #region ServiceContractNumberingID
        public abstract class serviceContractNumberingID : PX.Data.IBqlField
        {
        }
        protected String _ServiceContractNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("FSCONTRACT")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Service Contract Numbering Sequence")]
        public virtual String ServiceContractNumberingID
        {
            get
            {
                return this._ServiceContractNumberingID;
            }
            set
            {
                this._ServiceContractNumberingID = value;
            }
        }
        #endregion
        #region CustomerMultipleBillingOptions
        public abstract class customerMultipleBillingOptions : PX.Data.BQL.BqlBool.Field<customerMultipleBillingOptions> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manage Multiple Billing Options per Customer")]
        public virtual bool? CustomerMultipleBillingOptions { get; set; }
        #endregion 
        #region AlertBeforeCloseServiceOrder
        public abstract class alertBeforeCloseServiceOrder : PX.Data.BQL.BqlBool.Field<alertBeforeCloseServiceOrder> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Alert About Open Appointments Before Service Orders Are Closed")]
        public virtual bool? AlertBeforeCloseServiceOrder { get; set; }
        #endregion
        #region FilterInvoicingManually
        public abstract class filterInvoicingManually : PX.Data.BQL.BqlBool.Field<filterInvoicingManually> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Require Manual Filtering on Billing Forms")]
        public virtual bool? FilterInvoicingManually { get; set; }
        #endregion
        #region EnableSeasonScheduleContract
        public abstract class enableSeasonScheduleContract : PX.Data.BQL.BqlBool.Field<enableSeasonScheduleContract> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Seasons in Schedule Contracts")]
        public virtual bool? EnableSeasonScheduleContract { get; set; }
        #endregion
        #region EquipmentCalculateWarrantyFrom
        public abstract class equipmentCalculateWarrantyFrom : PX.Data.BQL.BqlString.Field<equipmentCalculateWarrantyFrom> { }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.EquipmentWarrantyFrom.SALES_ORDER_DATE)]
        [PXUIField(DisplayName = "Calculate Warranty From")]
        public virtual string EquipmentCalculateWarrantyFrom { get; set; }
        #endregion
        #region DfltCalendarStartTime
        public abstract class dfltCalendarStartTime : PX.Data.BQL.BqlDateTime.Field<dfltCalendarStartTime> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameTime = "Day Start Time")]
        [PXUIField(DisplayName = "Day Start Time")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? DfltCalendarStartTime { get; set; }
        #endregion
        #region DfltCalendarEndTime
        public abstract class dfltCalendarEndTime : PX.Data.BQL.BqlDateTime.Field<dfltCalendarEndTime> { }

        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameTime = "Day End Time")]
        [PXUIField(DisplayName = "Day End Time")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? DfltCalendarEndTime { get; set; }
        #endregion
        #region TimeRange
        public abstract class timeRange : ListField_TimeRange_Setup
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.TimeRange_Setup.DAY)]
        [timeRange.ListAtrribute]
        [PXUIField(DisplayName = "Time Range")]
        public virtual string TimeRange { get; set; }
        #endregion
        #region TimeFilter
        public abstract class timeFilter : ListField_TimeFilter_Setup
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.TimeFilter_Setup.CLEARED_FILTER)]
        [timeFilter.ListAtrribute]
        [PXUIField(DisplayName = "Time Filter")]
        public virtual string TimeFilter { get; set; }
        #endregion
        #region DayResolution
        public abstract class dayResolution : ListField_DayResolution_Setup
        {
        }

        [PXDBInt]
        [PXDefault(ID.DayResolution_Setup.D16)]
        [dayResolution.ListAtrribute]
        [PXUIField(DisplayName = "Day Resolution")]
        public virtual int? DayResolution { get; set; }
        #endregion
        #region WeekResolution
        public abstract class weekResolution : ListField_WeekResolution_Setup
        {
        }

        [PXDBInt]
        [PXDefault(ID.WeekResolution_Setup.W12)]
        [weekResolution.ListAtrribute]
        [PXUIField(DisplayName = "Week Resolution")]
        public virtual int? WeekResolution { get; set; }
        #endregion
        #region MonthResolution
        public abstract class monthResolution : ListField_MonthResolution_Setup
        {
        }

        [PXDBInt]
        [PXDefault(ID.MonthResolution_Setup.M10)]
        [monthResolution.ListAtrribute]
        [PXUIField(DisplayName = "Month Resolution")]
        public virtual int? MonthResolution { get; set; }
        #endregion
        #region MapApiKey
        public abstract class mapApiKey : PX.Data.BQL.BqlString.Field<mapApiKey> { }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Bing Map API Key")]
        public virtual string MapApiKey { get; set; }
        #endregion
        #region TrackAppointmentLocation
        public abstract class trackAppointmentLocation : PX.Data.BQL.BqlBool.Field<trackAppointmentLocation> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Track Start and Completion Appointment Locations")]
        public virtual bool? TrackAppointmentLocation { get; set; }
        #endregion
        #region EnableGPSTracking
        public abstract class enableGPSTracking : PX.Data.BQL.BqlBool.Field<enableGPSTracking> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Show Location Tracking")]
        public virtual bool? EnableGPSTracking { get; set; }
        #endregion
        #region GPSRefreshTrackingTime
        public abstract class gPSRefreshTrackingTime : PX.Data.BQL.BqlInt.Field<gPSRefreshTrackingTime> { }

        [PXDBInt]
        [PXDefault(30)]
        [PXUIField(DisplayName = "Refresh GPS Locations Every")]
        public virtual int? GPSRefreshTrackingTime { get; set; }
        #endregion
        #region HistoryDistanceAccuracy
        public abstract class historyDistanceAccuracy : PX.Data.BQL.BqlInt.Field<gPSRefreshTrackingTime> { }

        [PXDBInt(MinValue = 1)]
        [PXDefault(5)]
        [PXUIField(DisplayName = "History Distance Accuracy")]
        public virtual int? HistoryDistanceAccuracy { get; set; }
        #endregion
        #region HistoryTimeAccuracy
        public abstract class historyTimeAccuracy : PX.Data.BQL.BqlInt.Field<gPSRefreshTrackingTime> { }

        [PXDBInt(MinValue = 1)]
        [PXDefault(15)]
        [PXUIField(DisplayName = "History Time Accuracy")]
        public virtual int? HistoryTimeAccuracy { get; set; }
        #endregion
        #region DisableFixScheduleAction
        public abstract class disableFixScheduleAction : PX.Data.BQL.BqlBool.Field<disableFixScheduleAction> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Fix Schedules Without Next Execution Date")]
        public virtual bool? DisableFixScheduleAction { get; set; }
        #endregion
        #region ContractPostTo
        public abstract class contractPostTo : PX.Data.BQL.BqlString.Field<contractPostTo> { }

        [PXDBString(2)]
        [PXDefault(ID.Contract_PostTo.ACCOUNTS_RECEIVABLE_MODULE)]
        [FSPostTo.List]
        [PXUIField(DisplayName = "Generated Billing Documents")]
        public virtual string ContractPostTo { get; set; }
        #endregion
        #region DfltContractTermID_SO_AR
        public abstract class dfltContractTermIDARSO : PX.Data.BQL.BqlString.Field<dfltContractTermIDARSO> { }

        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Default Terms", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<Terms.termsID,
                            Where<
                                Terms.visibleTo, Equal<TermsVisibleTo.all>,
                                Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>),
                    DescriptionField = typeof(Terms.descr), Filterable = true)]
        public virtual string DfltContractTermIDARSO { get; set; }
        #endregion
        #region ContractPostOrderType
        public abstract class contractPostOrderType : PX.Data.BQL.BqlString.Field<contractPostOrderType> { }

        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXDefault]
        [PXUIField(DisplayName = "Order Type for Billing", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<SOOrderType.orderType,
                            Where<SOOrderType.active, Equal<True>,
                                And<FSxSOOrderType.enableFSIntegration, Equal<True>>>>),
                    DescriptionField = typeof(SOOrderType.descr))]
        public virtual string ContractPostOrderType { get; set; }
        #endregion
        #region ContractCombineSubFrom

        public abstract class contractCombineSubFrom : PX.Data.BQL.BqlString.Field<contractCombineSubFrom> { }

        [PXDefault]
        [SubAccountMask(true, DisplayName = "Combine Sales Sub. From")]
        public virtual string ContractCombineSubFrom { get; set; }
        #endregion
        #region ContractSalesAcctSource
        public abstract class contractSalesAcctSource : ListField_Contract_SalesAcctSource
        {
        }

        [PXDBString(2)]
        [contractSalesAcctSource.ListAtrribute]
        [PXDefault(ID.Contract_SalesAcctSource.CUSTOMER_LOCATION)]
        [PXUIField(DisplayName = "Use Sales Account From")]
        public virtual string ContractSalesAcctSource { get; set; }
        #endregion
        #region EnableContractPeriodWhenInvoice
        public abstract class enableContractPeriodWhenInvoice : PX.Data.BQL.BqlBool.Field<enableContractPeriodWhenInvoice> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Automatically Activate Upcoming Period")]
        public virtual bool? EnableContractPeriodWhenInvoice { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }

        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }

        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }

        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }

        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }

        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }

        #endregion
        #region EnableAllTargetEquipment
        public abstract class enableAllTargetEquipment : PX.Data.BQL.BqlBool.Field<enableAllTargetEquipment> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Service on All Target Equipment")]
        public virtual bool? EnableAllTargetEquipment { get; set; }
        #endregion  
        #region EnableDfltStaffOnServiceOrder
        public abstract class enableDfltStaffOnServiceOrder : PX.Data.BQL.BqlBool.Field<enableDfltStaffOnServiceOrder> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Default Staff in Service Orders")]
        public virtual bool? EnableDfltStaffOnServiceOrder { get; set; }
        #endregion  
        #region EnableDfltResEquipOnServiceOrder
        public abstract class enableDfltResEquipOnServiceOrder : PX.Data.BQL.BqlBool.Field<enableDfltResEquipOnServiceOrder> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Default Resource Equipment in Service Orders")]
        public virtual bool? EnableDfltResEquipOnServiceOrder { get; set; }
        #endregion 
        #region ReadyToUpgradeTo2017R2
        public abstract class readyToUpgradeTo2017R2 : PX.Data.BQL.BqlBool.Field<readyToUpgradeTo2017R2> { }

        [PXDBBool]
        [PXDefault(true)]
        public virtual bool? ReadyToUpgradeTo2017R2 { get; set; }
        #endregion

        #region WorkWave
        #region ROWWApiEndPoint
        public abstract class rOWWApiEndPoint : PX.Data.BQL.BqlString.Field<rOWWApiEndPoint> { }

        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "WorkWave API URL", Visibility = PXUIVisibility.Visible, FieldClass = "ROUTEOPTIMIZER")]
        public virtual string ROWWApiEndPoint { get; set; }
        #endregion
        #region ROWWLicensekey
        public abstract class rOWWLicensekey : PX.Data.BQL.BqlString.Field<rOWWLicensekey> { }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "License Key", Visibility = PXUIVisibility.Visible, FieldClass = "ROUTEOPTIMIZER")]
        public virtual string ROWWLicensekey { get; set; }
        #endregion

        #region ROLunchBreakDuration
        public abstract class rOLunchBreakDuration : PX.Data.BQL.BqlInt.Field<rOLunchBreakDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
        [PXDefault(ID.TimeConstants.MINUTES_60, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Lunch Break Duration", FieldClass = "ROUTEOPTIMIZER")]
        public virtual int? ROLunchBreakDuration  { get; set; }
        #endregion

        #region ROLunchBreakStartTimeFrame
        public abstract class rOLunchBreakStartTimeFrame : PX.Data.BQL.BqlDateTime.Field<rOLunchBreakStartTimeFrame> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Lunch Break Start Date", DisplayNameTime = "Lunch Break Start Time")]
        [PXUIField(DisplayName = "Lunch Break Start Time", FieldClass = "ROUTEOPTIMIZER")]
        public virtual DateTime? ROLunchBreakStartTimeFrame { get; set; }
        #endregion

        #region ROLunchBreakEndTimeFrame

        public abstract class rOLunchBreakEndTimeFrame : PX.Data.BQL.BqlDateTime.Field<rOLunchBreakEndTimeFrame> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Lunch Break End Date", DisplayNameTime = "Lunch Break End Time")]
        [PXUIField(DisplayName = "Lunch Break End Time", FieldClass = "ROUTEOPTIMIZER")]
        public virtual DateTime? ROLunchBreakEndTimeFrame { get; set; }
        #endregion
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region MemoryHelper
		#region CustomDfltCalendarStartTime
		public abstract class customDfltCalendarStartTime : PX.Data.BQL.BqlString.Field<customDfltCalendarStartTime> { }

        [PXString]
        public virtual string CustomDfltCalendarStartTime
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.DfltCalendarStartTime != null)
                {
                    return this.DfltCalendarStartTime.ToString();
                }

                return string.Empty;
            }
        }
        #endregion
        #region CustomDfltCalendarEndTime
        public abstract class customDfltCalendarEndTime : PX.Data.BQL.BqlString.Field<customDfltCalendarEndTime> { }

        [PXString]
        public virtual string CustomDfltCalendarEndTime
        {
            get
            {
                //Value cannot be calculated with PXFormula attribute
                if (this.DfltCalendarEndTime != null)
                {
                    return this.DfltCalendarEndTime.ToString();
                }

                return string.Empty;
            }
        }
        #endregion
        #region BillingOptionsChanged
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? BillingOptionsChanged { get; set; }
        #endregion
        #endregion
    }
}
