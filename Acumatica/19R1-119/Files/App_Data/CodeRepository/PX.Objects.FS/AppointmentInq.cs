using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System;
using System.Collections;

namespace PX.Objects.FS
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class AppointmentInq : PXGraph<AppointmentInq>
    {
        #region Internal Types
        [Serializable]
        public class AppointmentInqFilter : IBqlTable
        {
            #region BranchID
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

            [Branch]
            [PXRestrictor(typeof(Where<True, Equal<True>>), PX.Objects.GL.Messages.BranchInactive, ReplaceInherited = true)]
            public virtual int? BranchID { get; set; }
            #endregion
            #region BranchLocationID
            public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

            [PXInt]
            [PXUIField(DisplayName = "Branch Location")]
            [PXSelector(typeof(
                Search<FSBranchLocation.branchLocationID,
                        Where<
                            FSBranchLocation.branchID, Equal<Current<AppointmentInqFilter.branchID>>>>),
                            SubstituteKey = typeof(FSBranchLocation.branchLocationCD), DescriptionField = typeof(FSBranchLocation.descr))]
            [PXFormula(typeof(Default<AppointmentInqFilter.branchID>))]
            public virtual int? BranchLocationID { get; set; }
            #endregion
            #region CustomerID
            public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

            [PXInt]
            [PXUIField(DisplayName = "Customer")]
            [FSSelectorCustomer]
            public virtual int? CustomerID { get; set; }
            #endregion
            #region CustomerLocationID
            public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

            [LocationID(typeof(Where<Location.bAccountID, Equal<Current<AppointmentInqFilter.customerID>>>),
                        DescriptionField = typeof(Location.descr), DisplayName = "Location", DirtyRead = true)]
            [PXFormula(typeof(Default<AppointmentInqFilter.customerID>))]
            public virtual int? CustomerLocationID { get; set; }
            #endregion

            #region SORefNbr
            public abstract class sORefNbr : PX.Data.BQL.BqlString.Field<sORefNbr> { }

            [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "Service Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
            [PXSelector(typeof(
            Search3<FSServiceOrder.refNbr,
            LeftJoin<Customer,
                On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
            LeftJoin<Location,
                On<Location.locationID, Equal<FSServiceOrder.locationID>>>>,
            OrderBy<
                Desc<FSServiceOrder.refNbr,
                Asc<FSServiceOrder.srvOrdType>>>>),
                    new Type[] {
                                typeof(FSServiceOrder.refNbr),
                                typeof(FSServiceOrder.srvOrdType),
                                typeof(Customer.acctCD),
                                typeof(Customer.acctName),
                                typeof(Location.locationCD),
                                typeof(FSServiceOrder.docDesc),
                                typeof(FSServiceOrder.status),
                                typeof(FSServiceOrder.promisedDate)
                    })]
            public virtual string SORefNbr { get; set; }
            #endregion

            #region ServiceContractID
            public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

            [PXInt]
            [PXSelector(typeof(Search<FSServiceContract.serviceContractID,
                               Where<
                                    FSServiceContract.customerID, Equal<Current<AppointmentInqFilter.customerID>>>>),
                               SubstituteKey = typeof(FSServiceContract.refNbr))]
            [PXUIField(DisplayName = "Service Contract ID", FieldClass = "FSCONTRACT")]
            [PXFormula(typeof(Default<AppointmentInqFilter.customerID>))]
            public virtual int? ServiceContractID { get; set; }
            #endregion
            #region ScheduleID
            public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

            [PXInt]
            [PXSelector(typeof(Search<FSSchedule.scheduleID,
                               Where<
                                    FSSchedule.entityType, Equal<FSSchedule.entityType.Contract>,
                                    And<FSSchedule.entityID, Equal<Current<AppointmentInqFilter.serviceContractID>>>>,
                               OrderBy<
                                   Desc<FSSchedule.refNbr>>>),
                        SubstituteKey = typeof(FSSchedule.refNbr))]
            [PXUIField(DisplayName = "Schedule ID")]
            [PXFormula(typeof(Default<AppointmentInqFilter.serviceContractID>))]
            public virtual int? ScheduleID { get; set; }
            #endregion
            #region StaffMemberID
            public abstract class staffMemberID : PX.Data.BQL.BqlInt.Field<staffMemberID> { }

            [PXInt]
            [FSSelector_StaffMember_ServiceOrderProjectID]
            [PXUIField(DisplayName = "Staff Member", TabOrder = 0)]
            public virtual int? StaffMemberID { get; set; }
            #endregion
            #region SMEquipmentID
            public abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

            [PXInt]
            [PXSelector(typeof(Search<FSEquipment.SMequipmentID,
                               Where<
                                    FSEquipment.resourceEquipment, Equal<True>>>), 
                        SubstituteKey = typeof(FSEquipment.refNbr))]
            [PXUIField(DisplayName = "Resource Equipment")]
            public virtual int? SMEquipmentID { get; set; }
            #endregion

            #region FromScheduledDate
            public abstract class fromScheduledDate : PX.Data.BQL.BqlDateTime.Field<fromScheduledDate> { }

            [PXDateAndTime(UseTimeZone = true)]
            [PXUIField(DisplayName = "From Scheduled Date")]
            public virtual DateTime? FromScheduledDate { get; set; }
            #endregion
            #region ToScheduledDate
            public abstract class toScheduledDate : PX.Data.BQL.BqlDateTime.Field<toScheduledDate> { }

            [PXDateAndTime(UseTimeZone = true)]
            [PXUIField(DisplayName = "To Scheduled Date")]
            public virtual DateTime? ToScheduledDate { get; set; }
            #endregion
        }
        #endregion

        #region Views/Selects
        public PXFilter<AppointmentInqFilter> Filter;

        [PXHidden]
        public PXSelect<BAccountSelectorBase> DummyView_CustomerRecords;

        [PXFilterable]
        public PXSelectJoinGroupBy<FSAppointmentFSServiceOrder,
                        LeftJoin<FSAppointmentEmployee, On<FSAppointmentEmployee.appointmentID, Equal<FSAppointmentFSServiceOrder.appointmentID>>,
                        LeftJoin<FSAppointmentResource, On<FSAppointmentResource.appointmentID, Equal<FSAppointmentFSServiceOrder.appointmentID>>,
                        LeftJoin<FSCustomerBillingSetup, On<FSCustomerBillingSetup.cBID, Equal<FSAppointmentFSServiceOrder.cBID>>,
                        LeftJoin<FSGeoZonePostalCode, On<FSGeoZonePostalCode.postalCode, Equal<FSAppointmentFSServiceOrder.postalCode>>,
                        LeftJoinSingleTable<Customer,
                            On<Customer.bAccountID, Equal<FSAppointmentFSServiceOrder.customerID>>>>>>>,
                    Where2<
                        Where<
                            Current<AppointmentInqFilter.branchID>, IsNull,
                            Or<Current<AppointmentInqFilter.branchID>, Equal<FSAppointmentFSServiceOrder.branchID>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.branchLocationID>, IsNull,
                            Or<Current<AppointmentInqFilter.branchLocationID>, Equal<FSAppointmentFSServiceOrder.branchLocationID>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.customerID>, IsNull,
                            Or<Current<AppointmentInqFilter.customerID>, Equal<FSAppointmentFSServiceOrder.customerID>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.customerLocationID>, IsNull,
                            Or<Current<AppointmentInqFilter.customerLocationID>, Equal<FSAppointmentFSServiceOrder.locationID>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.sORefNbr>, IsNull,
                            Or<Current<AppointmentInqFilter.sORefNbr>, Equal<FSAppointmentFSServiceOrder.soRefNbr>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.serviceContractID>, IsNull,
                            Or<Current<AppointmentInqFilter.serviceContractID>, Equal<FSAppointmentFSServiceOrder.serviceContractID>,
                            Or<Current<AppointmentInqFilter.serviceContractID>, Equal<FSAppointmentFSServiceOrder.billServiceContractID>>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.scheduleID>, IsNull,
                            Or<Current<AppointmentInqFilter.scheduleID>, Equal<FSAppointmentFSServiceOrder.scheduleID>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.staffMemberID>, IsNull,
                            Or<Current<AppointmentInqFilter.staffMemberID>, Equal<FSAppointmentEmployee.employeeID>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.SMequipmentID>, IsNull,
                            Or<Current<AppointmentInqFilter.SMequipmentID>, Equal<FSAppointmentResource.SMequipmentID>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.fromScheduledDate>, IsNull,
                            Or<Current<AppointmentInqFilter.fromScheduledDate>, LessEqual<FSAppointmentFSServiceOrder.scheduledDateTimeBegin>>>,
                        And2<
                        Where<
                            Current<AppointmentInqFilter.toScheduledDate>, IsNull,
                            Or<Current<AppointmentInqFilter.toScheduledDate>, GreaterEqual<FSAppointmentFSServiceOrder.scheduledDateTimeEnd>>>,
                        And<
                        Where<
                            Customer.bAccountID, IsNull,
                            Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>>>>>>>,
                    Aggregate<
                        GroupBy<FSAppointmentFSServiceOrder.appointmentID,
                        GroupBy<FSAppointmentFSServiceOrder.scheduledDateTimeBegin>>>,
                    OrderBy<
                        Desc<FSAppointmentFSServiceOrder.scheduledDateTimeBegin,
                        Desc<FSAppointmentFSServiceOrder.srvOrdType,
                        Desc<FSAppointmentFSServiceOrder.refNbr>>>>> Appointments;
        #endregion

        #region CacheAttached
        #region FSServiceOrder_ProjectID
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Project", Visible = false, FieldClass = ProjectAttribute.DimensionName)]
        protected virtual void FSAppointmentFSServiceOrder_ProjectID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_DfltProjectTaskID
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Task", Visible = false)]
        protected virtual void FSAppointmentFSServiceOrder_DfltProjectTaskID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Ctor + Overrides
        public AppointmentInq()
        {
            Appointments.Cache.AllowDelete = false;
            Appointments.Cache.AllowInsert = false;
            Appointments.Cache.AllowUpdate = false;

            CreateNew.SetEnabled(AppointmentEntry.IsReadyToBeUsed(this, this.Accessinfo.ScreenID));
        }

        public override bool IsDirty
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Actions
        public PXCancel<AppointmentInqFilter> Cancel;

        #if (!Acumatica_5_10)

        #region CreateNew
        public PXAction<AppointmentInqFilter> CreateNew;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXInsertButton]
        protected virtual IEnumerable createNew(PXAdapter adapter)
        {
            var graph = PXGraph.CreateInstance<AppointmentEntry>();

            FSAppointment fsAppointmentRow = (FSAppointment)graph.AppointmentRecords.Cache.CreateInstance();
            graph.AppointmentRecords.Insert(fsAppointmentRow);
            graph.AppointmentRecords.Cache.IsDirty = false;

            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.Same };
        }
        #endregion
        #region EditDetail
        public PXAction<AppointmentInqFilter> EditDetail;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        protected virtual IEnumerable editDetail(PXAdapter adapter)
        {
            if (Appointments.Current == null) 
            {
                return Filter.Select();
            }

            var graph = PXGraph.CreateInstance<AppointmentEntry>();

            graph.AppointmentRecords.Current = graph.AppointmentRecords.Search<FSAppointment.refNbr>(
                                                                Appointments.Current.RefNbr,
                                                                Appointments.Current.SrvOrdType);

            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #endif

        #endregion

        #region Event Subscribers
        protected virtual void AppointmentInqFilter_FromScheduledDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (Accessinfo.BusinessDate.Value != null)
            {
                DateTime today = Accessinfo.BusinessDate.Value;
                e.NewValue = new DateTime(today.Year, today.Month, 1);
            }
        }

        protected virtual void AppointmentInqFilter_ToScheduledDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }
            
            if (Accessinfo.BusinessDate.Value != null)
            {
                DateTime today = Accessinfo.BusinessDate.Value;
                e.NewValue = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            }

            e.NewValue = new DateHandler((DateTime?)e.NewValue).EndOfDay();
        }

        protected virtual void AppointmentInqFilter_ToScheduledDate_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            if (e.NewValue.GetType().Equals(typeof(string)))
            {
                e.NewValue = new DateHandler(DateTime.Parse(e.NewValue.ToString())).EndOfDay();
            }
            else
            {
                e.NewValue = new DateHandler((DateTime?)e.NewValue).EndOfDay();
            }            
        }

        protected virtual void AppointmentInqFilter_StaffMemberID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            int? employeeID = SharedFunctions.GetCurrentEmployeeID(cache);

            if (employeeID != null)
            {
                e.NewValue = employeeID;
                e.Cancel = true;
            }
        }

        protected virtual void AppointmentInqFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null || IsMobile == false)
            {
                return;
            }

            AppointmentInqFilter filter = e.Row as AppointmentInqFilter;
            cache.SetValue<AppointmentInqFilter.toScheduledDate>(filter, new DateHandler(filter.ToScheduledDate).EndOfDay());
        }
        #endregion
    }
}
