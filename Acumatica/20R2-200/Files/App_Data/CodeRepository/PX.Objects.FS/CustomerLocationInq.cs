using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.SO;
using System;
using System.Collections;

namespace PX.Objects.FS
{
    #region DACProjection
    [PXProjection(typeof(
        Select2<Location,
            LeftJoin<BAccount,
                On<BAccount.bAccountID, Equal<Location.bAccountID>>>>))]
    public class BAccountLocation : IBqlTable
    {
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt(IsKey = true, BqlField = typeof(BAccount.bAccountID))]
        [PXUIField(DisplayName = "Customer")]
        [PXSelector(typeof(
                 Search<BAccount.bAccountID,
                            Where<
                                BAccount.type, Equal<BAccountType.combinedType>,
                            Or<
                                BAccount.type, Equal<BAccountType.customerType>>>>),
                    new Type[]
                    {
                        typeof(BAccount.acctCD),
                        typeof(BAccount.acctName),
                        typeof(BAccount.classID),
                        typeof(BAccount.status),
                    }, SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
        public virtual int? CustomerID { get; set; }
        #endregion

        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        [PXDBInt(IsKey = true, BqlField = typeof(Location.locationID))]
        [PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(
                 Search2<Location.locationID,
                 InnerJoin<Address,
                    On<
                        Location.defAddressID, Equal<Address.addressID>>>,
                 Where2<
                    Where<
                        Location.bAccountID, Equal<Current<BAccountLocation.customerID>>>,
                    And<
                        Where<
                            Location.locType, Equal<LocTypeList.customerLoc>,
                        Or<
                            Location.locType, Equal<LocTypeList.combinedLoc>>>>>,
                 OrderBy<Asc<Location.locationCD>>>),
                    new Type[]
                    {
                        typeof(Location.locationCD),
                        typeof(Location.descr),
                        typeof(Address.addressLine1),
                        typeof(Address.addressLine2),
                        typeof(Address.postalCode),
                        typeof(Address.city),
                        typeof(Address.state),
                    }, SubstituteKey = typeof(Location.locationCD), DescriptionField = typeof(Location.descr))]
        public virtual int? LocationID { get; set; }
        #endregion

        #region CustomerCD
        public abstract class customerCD : PX.Data.BQL.BqlString.Field<customerCD> { }

        [PXDBString(BqlField = typeof(BAccount.acctCD))]
        [PXUIField(DisplayName = "Customer ID")]
        public virtual string CustomerCD { get; set; }
        #endregion

        #region IsActive
        public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

        [PXDBBool(BqlField = typeof(Location.isActive))]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? IsActive { get; set; }
        #endregion
    }
    #endregion

    public class CustomerLocationInq : PXGraph<CustomerLocationInq>
    {
        public PXCancel<BAccountLocation> cancel;
        public PXFirst<BAccountLocation> First;
        public PXPrevious<BAccountLocation> Previous;
        public PXNext<BAccountLocation> Next;
        public PXLast<BAccountLocation> Last;
        public bool inactiveFlag = true;

        public CustomerLocationInq()
        {
            LocationRecords.Cache.AllowDelete = false;
        }

        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
            BAccountLocation bAccountLocationRow = LocationRecords.Current;

            if (bAccountLocationRow != null && bAccountLocationRow.CustomerID != null 
                && adapter.Searches.Length == 2 && adapter.Searches[0] != null)
            {
                if (bAccountLocationRow.CustomerCD.Trim() != adapter.Searches[0].ToString().Trim())
                {
                    PXResult<BAccount, Location> BaccountLocationResultSet = (PXResult < BAccount, Location>)
                                                                             PXSelectJoin<BAccount,
                                                                             InnerJoin<Location,
                                                                             On<
                                                                                 Location.bAccountID, Equal<BAccount.bAccountID>>>,
                                                                             Where<
                                                                                 BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>
                                                                             .SelectWindowed(this, 0, 1, adapter.Searches[0]);

                    Location locationRow = BaccountLocationResultSet;

                    adapter.Searches[1] = locationRow.LocationCD;
                }
            }

            foreach (BAccountLocation loc in (new PXCancel<BAccountLocation>(this, "Cancel")).Press(adapter))
            {
                return new object[] { loc };
            }

            return new object[0];
        }
        
        #region Views
        public PXSelect<BAccountLocation,
               Where<BAccountLocation.customerID, Equal<Optional<BAccountLocation.customerID>>>> LocationRecords;

        public PXSelectReadonly2<FSAppointmentDet,
               InnerJoin<FSSODet,
               On<
                   FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>,
                   And<
                       Where<
                           FSAppointmentDet.lineType, Equal<FSLineType.Service>,
                       Or<
                           FSAppointmentDet.lineType, Equal<FSLineType.NonStockItem>>>>>,
               InnerJoin<FSAppointment,
               On<
                   FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
               InnerJoin<FSServiceOrder,
               On<
                   FSServiceOrder.refNbr, Equal<FSAppointment.soRefNbr>>,
               LeftJoin<FSPostDet,
               On<
                   FSAppointmentDet.postID, Equal<FSPostDet.postID>>,
               LeftJoin<FSPostBatch,
               On<
                   FSPostBatch.batchID, Equal<FSPostDet.batchID>>>>>>>,
               Where<
                   FSServiceOrder.locationID, Equal<Current<BAccountLocation.locationID>>>,
               OrderBy<
                   Desc<FSAppointment.executionDate>>> Services;

        public PXSelectReadonly2<FSAppointmentDet,
               InnerJoin<FSAppointment,
               On<
                   FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
               InnerJoin<FSServiceOrder,
               On<
                   FSServiceOrder.refNbr, Equal<FSAppointment.soRefNbr>>,
               LeftJoin<FSPostDet,
               On<
                   FSAppointmentDet.postID, Equal<FSPostDet.postID>>,
               LeftJoin<FSPostBatch,
               On<
                   FSPostBatch.batchID, Equal<FSPostDet.batchID>>>>>>,
               Where<
                   FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
               And<
                   FSServiceOrder.locationID, Equal<Current<BAccountLocation.locationID>>>>,
               OrderBy<
                   Desc<FSAppointment.executionDate>>> PickUpDeliveryItems;

        public PXSelectReadonly2<FSRouteContractScheduleFSServiceContract,
                InnerJoin<FSScheduleRoute,
                    On<
                        FSScheduleRoute.scheduleID, Equal<FSRouteContractScheduleFSServiceContract.scheduleID>>,
                InnerJoin<FSRoute,
                    On<
                        FSRoute.routeID, Equal<FSScheduleRoute.dfltRouteID>>,
                    InnerJoin<FSServiceContract,
                    On<
                        FSServiceContract.serviceContractID, Equal<FSRouteContractScheduleFSServiceContract.entityID>>>>>,
                Where<
                    FSRouteContractScheduleFSServiceContract.customerLocationID, Equal<Current<BAccountLocation.locationID>>>,
                OrderBy<
                    Desc<FSRouteContractScheduleFSServiceContract.refNbr>>> RouteContractSchedules;

        public PXSelectReadonly2<ARPriceClass,
               InnerJoin<Location, 
               On<
                   Location.cPriceClassID, Equal<ARPriceClass.priceClassID>>>,
               Where<
                   Location.locationID, Equal<Current<BAccountLocation.locationID>>>> PriceClass1;

        public PXSelectReadonly2<Customer,
               InnerJoin<Location,
               On<
                   Location.bAccountID, Equal<Customer.bAccountID>>>,
               Where<
                   Location.locationID, Equal<Current<BAccountLocation.locationID>>>> CustomerRecords;

        [PXFilterable]
        public PXSelectReadonly2<Location,
                LeftJoin<Contact,
                    On<
                        Contact.bAccountID, Equal<Location.bAccountID>,
                    And<
                        Contact.contactID, Equal<Location.defContactID>>>,
                LeftJoin<Address,
                    On<
                        Address.bAccountID, Equal<Location.bAccountID>,
                    And<
                        Address.addressID, Equal<Location.defAddressID>>>,
                LeftJoin<Customer,
                    On<
                        Customer.bAccountID, Equal<Location.bAccountID>>,
                LeftJoin<FSBillingCycle,
                    On<
                        FSxCustomer.billingCycleID, Equal<FSBillingCycle.billingCycleID>>>>>>,
                Where<
                    Location.locationID, Equal<Current<BAccountLocation.locationID>>>> LocationSelected;
        #endregion

        #region Actions
        #region OpenRouteServiceContract
        public PXAction<BAccountLocation> OpenRouteServiceContract;
        [PXButton]
        [PXUIField(DisplayName = "Open Route Service Contract", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openRouteServiceContract()
        {
            if (RouteContractSchedules.Current != null)
            {
                RouteServiceContractEntry graphRouteServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();

                graphRouteServiceContractEntry.ServiceContractRecords.Current = PXSelect<FSServiceContract,
                                                                                Where<
                                                                                    FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                                                .Select(this, RouteContractSchedules.Current.EntityID);

                throw new PXRedirectRequiredException(graphRouteServiceContractEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #region OpenRouteSchedule
        public PXAction<BAccountLocation> OpenRouteSchedule;
        [PXButton]
        [PXUIField(DisplayName = "Open Route Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openRouteSchedule()
        {
            if (RouteContractSchedules.Current != null)
            {
                var graphRouteServiceContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                graphRouteServiceContractScheduleEntry.ContractScheduleRecords.Current = graphRouteServiceContractScheduleEntry
                                                                                         .ContractScheduleRecords.Search<FSRouteContractSchedule.scheduleID>
                                                                                         (RouteContractSchedules.Current.ScheduleID,
                                                                                          RouteContractSchedules.Current.EntityID,
                                                                                          LocationRecords.Current.CustomerID);

                throw new PXRedirectRequiredException(graphRouteServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #region OpenAppointmentByPickUpDeliveryItem
        public PXAction<BAccountLocation> OpenAppointmentByPickUpDeliveryItem;
        [PXButton]
        [PXUIField(DisplayName = "Open Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openAppointmentByPickUpDeliveryItem()
        {
            if (PickUpDeliveryItems.Current == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = PXSelect<FSAppointment,
                                             Where<
                                                 FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                             .SelectSingleBound(this, null, PickUpDeliveryItems.Current.AppointmentID);

            openAppointment(fsAppointmentRow);
        }
        #endregion
        #region OpenAppointmentByService
        public PXAction<BAccountLocation> OpenAppointmentByService;
        [PXButton]
        [PXUIField(DisplayName = "Open Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openAppointmentByService()
        {
            if (Services.Current == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = PXSelect<FSAppointment,
                                             Where<
                                                 FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                             .SelectSingleBound(this, null, Services.Current.AppointmentID);

            openAppointment(fsAppointmentRow);
        }
        #endregion
        #region OpenDocument
        public PXAction<BAccountLocation> OpenDocumentByPickUpDeliveryItem;
        [PXButton]
        [PXUIField(DisplayName = "Open Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openDocumentByPickUpDeliveryItem()
        {
            if (PickUpDeliveryItems.Current == null)
            {
                return;
            }

            FSPostDet fsPostDetRow = PXSelect<FSPostDet,
                                     Where<
                                         FSPostDet.postDetID, Equal<Required<FSPostDet.postDetID>>>>
                                     .SelectSingleBound(this, null, PickUpDeliveryItems.Current.PostID);

            openDocument(fsPostDetRow);
        }
        #endregion
        #region OpenDocument
        public PXAction<BAccountLocation> OpenDocumentByService;
        [PXButton]
        [PXUIField(DisplayName = "Open Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openDocumentByService()
        {
            if (Services.Current == null)
            {
                return;
            }

            FSPostDet fsPostDetRow = PXSelect<FSPostDet,
                                     Where<
                                         FSPostDet.postDetID, Equal<Required<FSPostDet.postDetID>>>>
                                     .SelectSingleBound(this, null, Services.Current.PostID);

            openDocument(fsPostDetRow);
        }      
        #endregion

        private void openDocument(FSPostDet fsPostDetRow)
        {
            if (fsPostDetRow.SOPosted == true)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
                    graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(fsPostDetRow.SOOrderNbr, fsPostDetRow.SOOrderType);
                    throw new PXRedirectRequiredException(graphSOOrderEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            else if (fsPostDetRow.ARPosted == true)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.ARRefNbr, fsPostDetRow.ARDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.SOInvPosted == true)
            {
                SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.ARRefNbr, fsPostDetRow.ARDocType);
                throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.APPosted == true)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(fsPostDetRow.APRefNbr, fsPostDetRow.APDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }

        private void openAppointment(FSAppointment fsAppointmentRow) 
        {
            if (fsAppointmentRow != null)
            {
                var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry
                                                                   .AppointmentRecords.Search<FSAppointment.refNbr>
                                                                   (fsAppointmentRow.RefNbr,
                                                                    fsAppointmentRow.SrvOrdType);

                throw new PXRedirectRequiredException(graphAppointmentEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
    }
}
