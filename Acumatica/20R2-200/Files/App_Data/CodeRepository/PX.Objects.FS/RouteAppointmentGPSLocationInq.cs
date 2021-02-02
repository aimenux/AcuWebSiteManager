using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using System;
using System.Collections;

namespace PX.Objects.FS
{
    public class RouteAppointmentGPSLocationInq : PXGraph<RouteAppointmentGPSLocationInq>
    {
        public RouteAppointmentGPSLocationInq()
        {
            RouteAppointmentGPSLocationRecords.Cache.AllowInsert = false;
            RouteAppointmentGPSLocationRecords.Cache.AllowUpdate = false;
            RouteAppointmentGPSLocationRecords.Cache.AllowDelete = false;
        }

        #region AppointmentData
        [Serializable]
        public class AppointmentData : FSAppointmentInRoute
        {
            #region Route ID
            public new abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

            [PXDBInt]
            [PXUIField(DisplayName = "Route", Visible = false)]
            [FSSelectorRouteID]
            public override int? RouteID { get; set; }
            #endregion
            #region RouteDocumentID
            public new abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

            [PXDBInt]
            [PXSelector(typeof(Search<FSRouteDocument.routeDocumentID>), SubstituteKey = typeof(FSRouteDocument.refNbr))]
            [PXUIField(DisplayName = "Route Nbr.", Visible = false)]
            public override int? RouteDocumentID { get; set; }
            #endregion
            #region CustomerID
            public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

            [PXDBInt(BqlField = typeof(FSServiceOrder.customerID))]
            [PXUIField(DisplayName = "Customer")]
            [FSSelectorCustomer]
            public override int? CustomerID { get; set; }
            #endregion
            #region LocationID
            public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

            [PXDBInt(BqlField = typeof(FSServiceOrder.locationID))]
            [PXUIField(DisplayName = "Location")]
            [PXSelector(typeof(Search<Location.locationID>), SubstituteKey = typeof(Location.locationCD), DescriptionField = typeof(Location.descr))]
            public override int? LocationID { get; set; }
            #endregion
            #region Address
            public abstract class address : PX.Data.BQL.BqlString.Field<address> { }

            [PXString(50, IsUnicode = true)]
            [PXUIField(DisplayName = "Address")]
            public virtual string Address
            {
                get
                {
                    return SharedFunctions.GetAddressForGeolocation(this.PostalCode, this.AddressLine1, this.AddressLine2, this.City, this.State, string.Empty);
                }
            }
            #endregion
            #region GPS Start Coordinate
            public abstract class gPSStartCoordinate : PX.Data.BQL.BqlString.Field<gPSStartCoordinate> { }

            [PXString(250, IsUnicode = true)]
            [PXUIField(DisplayName = "GPS Start Coordinate")]
            public virtual string GPSStartCoordinate
            {
                get
                {
                    return SharedFunctions.GetCompleteCoordinate(this.GPSLongitudeStart, this.GPSLatitudeStart);
                }
            }
            #endregion
            #region GPS Start Address
            public abstract class gPSStartAddress : PX.Data.BQL.BqlString.Field<gPSStartAddress> { }

            [PXString(250, IsUnicode = true)]
            [PXUIField(DisplayName = "GPS Start Address")]
            public virtual string GPSStartAddress
            {
                get
                {
                    if (this.GPSLatitudeStart == null || this.GPSLongitudeStart == null)
                    {
                        return string.Empty;
                    }

                    return Geocoder.ReverseGeocode(new LatLng((double)this.GPSLatitudeStart, (double)this.GPSLongitudeStart), this.MapApiKey);
                }
            }
            #endregion
            #region GPS Complete Coordinate
            public abstract class gPSCompleteCoordinate : PX.Data.BQL.BqlString.Field<gPSCompleteCoordinate> { }

            [PXString(250, IsUnicode = true)]
            [PXUIField(DisplayName = "GPS Complete Coordinate")]
            public virtual string GPSCompleteCoordinate
            {
                get
                {
                    return SharedFunctions.GetCompleteCoordinate(this.GPSLongitudeComplete, this.GPSLatitudeComplete);
                }
            }
            #endregion
            #region GPS Complete Address
            public abstract class gPSCompleteAddress : PX.Data.BQL.BqlString.Field<gPSCompleteAddress> { }

            [PXString(250, IsUnicode = true)]
            [PXUIField(DisplayName = "GPS Complete Address")]
            public virtual string GPSCompleteAddress
            {
                get
                {
                    if (this.GPSLatitudeComplete == null || this.GPSLongitudeComplete == null)
                    {
                        return string.Empty;
                    }

                    return Geocoder.ReverseGeocode(new LatLng((double)this.GPSLatitudeComplete, (double)this.GPSLongitudeComplete), this.MapApiKey);
                }
            }
            #endregion
        }
        #endregion

        #region Selects
        public PXCancel<RouteAppointmentGPSLocationFilter> Cancel;

        public PXFilter<RouteAppointmentGPSLocationFilter> RouteAppointmentGPSLocationFilter;

        [PXFilterable]
        public PXSelectJoinGroupBy<AppointmentData,
                LeftJoin<FSAppointmentDet,
                    On<
                        FSAppointmentDet.appointmentID, Equal<AppointmentData.appointmentID>>>,
                Where2<
                    Where<
                        Current<RouteAppointmentGPSLocationFilter.loadData>, Equal<True>,
                        And<AppointmentData.routeDocumentID, IsNotNull,
                        And<
                            Where<AppointmentData.status, Equal<AppointmentData.status.Closed>,
                            Or<AppointmentData.status, Equal<AppointmentData.status.Completed>>>>>>,
                //Start filter conditions
                And2<
                    Where<Current<RouteAppointmentGPSLocationFilter.dateFrom>, IsNull,
                    Or<AppointmentData.scheduledDateTimeBegin, GreaterEqual<Current<RouteAppointmentGPSLocationFilter.dateFrom>>>>,
                And2<
                    Where<Current<RouteAppointmentGPSLocationFilter.dateTo>, IsNull,
                    Or<AppointmentData.scheduledDateTimeEnd, LessEqual<Current<RouteAppointmentGPSLocationFilter.dateTo>>>>,
                And2<
                    Where<Current<RouteAppointmentGPSLocationFilter.customerID>, IsNull,
                    Or<AppointmentData.customerID, Equal<Current<RouteAppointmentGPSLocationFilter.customerID>>>>,
                And2<
                    Where<Current<RouteAppointmentGPSLocationFilter.customerLocationID>, IsNull,
                    Or<AppointmentData.locationID, Equal<Current<RouteAppointmentGPSLocationFilter.customerLocationID>>>>,
                And2<
                    Where<Current<RouteAppointmentGPSLocationFilter.serviceID>, IsNull,
                    Or<FSAppointmentDet.inventoryID, Equal<Current<RouteAppointmentGPSLocationFilter.serviceID>>>>,
                And2<
                    Where<Current<RouteAppointmentGPSLocationFilter.routeDocumentID>, IsNull,
                    Or<AppointmentData.routeDocumentID, Equal<Current<RouteAppointmentGPSLocationFilter.routeDocumentID>>>>,
                And<
                    Where<Current<RouteAppointmentGPSLocationFilter.routeID>, IsNull,
                    Or<AppointmentData.routeID, Equal<Current<RouteAppointmentGPSLocationFilter.routeID>>>>>>>>>>>>,
                Aggregate<
                    GroupBy<AppointmentData.appointmentID>>,
                OrderBy<
                    Asc<AppointmentData.routeID,
                    Asc<AppointmentData.scheduledDateTimeBegin>>>> RouteAppointmentGPSLocationRecords;
        #endregion

        #region Actions

        #region FilterManually
        public PXAction<RouteAppointmentGPSLocationFilter> filterManually;
        [PXUIField(DisplayName = "Generate")]
        public virtual IEnumerable FilterManually(PXAdapter adapter)
        {
            RouteAppointmentGPSLocationFilter.Current.LoadData = true;
            return adapter.Get();
        }

        #endregion

        #region Report
        public PXAction<RouteAppointmentGPSLocationFilter> report;
        [PXButton]
        [PXUIField(DisplayName = "View as Report", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void Report()
        {
            PXReportResultset reportData = new PXReportResultset(typeof(AppointmentData));

            foreach (AppointmentData row in RouteAppointmentGPSLocationRecords.Select())
            {
                reportData.Add(row);
            }

            throw new PXReportRequiredException(reportData, ID.ReportID.ROUTE_APP_GPS_LOCATION, PXBaseRedirectException.WindowMode.NewWindow, "Report");
        }
        #endregion

        #region OpenLocationScreen
        public PXAction<RouteAppointmentGPSLocationFilter> OpenLocationScreen;
        [PXButton]
        [PXUIField(Visible = false)]
        protected virtual void openLocationScreen()
        {
            if (RouteAppointmentGPSLocationRecords.Current != null)
            {
                CustomerLocationMaint graphCustomerLocationMaint = PXGraph.CreateInstance<CustomerLocationMaint>();

                BAccount bAccountRow = PXSelect<BAccount,
                                       Where<
                                           BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
                                       .Select(this, RouteAppointmentGPSLocationRecords.Current.CustomerID);

                graphCustomerLocationMaint.Location.Current = graphCustomerLocationMaint.Location.Search<Location.locationID>
                                                                (RouteAppointmentGPSLocationRecords.Current.LocationID, bAccountRow.AcctCD);

                throw new PXRedirectRequiredException(graphCustomerLocationMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region EditDetail
        public PXAction<RouteAppointmentGPSLocationFilter> EditDetail;
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        protected virtual IEnumerable editDetail(PXAdapter adapter)
        {
            if (RouteAppointmentGPSLocationRecords.Current == null)
            {
                return RouteAppointmentGPSLocationFilter.Select();
            }

            var graph = PXGraph.CreateInstance<AppointmentEntry>();

            graph.AppointmentRecords.Current = graph.AppointmentRecords.Search<FSAppointment.refNbr>
                                                                        (RouteAppointmentGPSLocationRecords.Current.RefNbr,
                                                                         RouteAppointmentGPSLocationRecords.Current.SrvOrdType);

            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #endregion

        #region EvenHandlers

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<RouteAppointmentGPSLocationFilter, RouteAppointmentGPSLocationFilter.dateFrom> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (Accessinfo.BusinessDate.Value != null)
            {
                e.NewValue = Accessinfo.BusinessDate.Value;
            }
        }
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<RouteAppointmentGPSLocationFilter> e)
        {
        }

        protected virtual void _(Events.RowSelected<RouteAppointmentGPSLocationFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (RouteAppointmentGPSLocationFilter.Current != null && RouteAppointmentGPSLocationFilter.Current.LoadData != null)
            {
                report.SetEnabled((bool)RouteAppointmentGPSLocationFilter.Current.LoadData);
            }
        }

        protected virtual void _(Events.RowInserting<RouteAppointmentGPSLocationFilter> e)
        {
        }

        protected virtual void _(Events.RowInserted<RouteAppointmentGPSLocationFilter> e)
        {
        }

        protected virtual void _(Events.RowUpdating<RouteAppointmentGPSLocationFilter> e)
        {
        }

        protected virtual void _(Events.RowUpdated<RouteAppointmentGPSLocationFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            RouteAppointmentGPSLocationFilter.Current.LoadData = false;
        }

        protected virtual void _(Events.RowDeleting<RouteAppointmentGPSLocationFilter> e)
        {
        }

        protected virtual void _(Events.RowDeleted<RouteAppointmentGPSLocationFilter> e)
        {
        }

        protected virtual void _(Events.RowPersisting<RouteAppointmentGPSLocationFilter> e)
        {
        }

        protected virtual void _(Events.RowPersisted<RouteAppointmentGPSLocationFilter> e)
        {
        }

        #endregion
    }
}