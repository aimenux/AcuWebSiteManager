using PX.Data;
using PX.Objects.EP;
using System;
using System.Collections;

namespace PX.Objects.FS
{
    public class RouteWrkSheetInq : PXGraph<RouteWrkSheetInq>
    {
        private RouteDocumentMaint routeDocumentMaint;

        public RouteWrkSheetInq()
            : base()
        {
            Routes.Cache.AllowInsert = false;
            routeDocumentMaint = PXGraph.CreateInstance<RouteDocumentMaint>();

            CreateNew.SetEnabled(RouteDocumentMaint.IsReadyToBeUsed(this));
        }
  
        #region DACFilter
        [Serializable]
        public partial class RouteWrkSheetFilter : IBqlTable
        {
            #region FromDate
            public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

            [PXDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual DateTime? FromDate { get; set; }
            #endregion
            #region ToDate
            public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

            [PXDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "To", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual DateTime? ToDate { get; set; }
            #endregion
            #region DriverID
            public abstract class driverID : PX.Data.BQL.BqlInt.Field<driverID> { }

            [PXInt]
            [PXUIField(DisplayName = "Driver", Visibility = PXUIVisibility.SelectorVisible)]
            [PXDefault(typeof(Search<EPEmployee.bAccountID,
                    Where<
                        FSxEPEmployee.sDEnabled, Equal<True>,
                        And<FSxEPEmployee.isDriver, Equal<True>,
                        And<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
            [FSSelector_Driver_AllAttribute]
            public virtual int? DriverID { get; set; }
            #endregion
        }
        #endregion

        #region CacheAttached
        #region FSRouteDocument_Date
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSRouteDocument_Date_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteDocument_RouteID
        [PXDBInt]
        [PXUIField(DisplayName = "Route ID", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorRouteID]
        protected virtual void FSRouteDocument_RouteID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Filter+Select
        public PXFilter<RouteWrkSheetFilter> Filter;
        public PXCancel<RouteWrkSheetFilter> Cancel;

        public PXSelect<EPEmployee,
               Where<
                   FSxEPEmployee.sDEnabled, Equal<True>,
                   And<FSxEPEmployee.isDriver, Equal<True>,
                   And<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>>> currentEmployee;

        [PXFilterable]
        public PXSelectJoin<FSRouteDocument,
                InnerJoin<FSRoute,
                    On<FSRouteDocument.routeID, Equal<FSRoute.routeID>>>,
                Where2<
                    Where<Current2<RouteWrkSheetFilter.driverID>, IsNull,
                        Or<FSRouteDocument.driverID, Equal<Current2<RouteWrkSheetFilter.driverID>>,
                        Or<FSRouteDocument.additionalDriverID, Equal<Current2<RouteWrkSheetFilter.driverID>>>>>,
                    And2<
                        Where<FSRouteDocument.status, Equal<ListField_Status_Route.Open>,
                            Or<FSRouteDocument.status, Equal<ListField_Status_Route.InProcess>>>,
                        And2<
                            Where<Current2<RouteWrkSheetFilter.fromDate>, IsNull,
                            Or<FSRouteDocument.date, GreaterEqual<Current2<RouteWrkSheetFilter.fromDate>>>>,
                            And<
                                Where<Current2<RouteWrkSheetFilter.toDate>, IsNull,
                                Or<FSRouteDocument.date, LessEqual<Current2<RouteWrkSheetFilter.toDate>>>>>>>>, 
                OrderBy<
                        Asc<FSRouteDocument.date,
                        Asc<FSRouteDocument.timeBegin>>>>
                Routes;

        #region VehicleSelection
        public SharedClasses.RouteSelected_view VehicleRouteSelected;
        public VehicleSelectionHelper.VehicleRecords_View VehicleRecords;
        public PXFilter<VehicleSelectionFilter> VehicleFilter;
        #endregion

        #region DriverSelection
        public SharedClasses.RouteSelected_view DriverRouteSelected;
        public DriverSelectionHelper.DriverRecords_View DriverRecords;
        public PXFilter<DriverSelectionFilter> DriverFilter;
        #endregion

        #endregion

        #region Actions

        public PXAction<RouteWrkSheetFilter> OpenRouteDocument;
        [PXButton]
        [PXUIField(DisplayName = "Open Route Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openRouteDocument()
        {
            if (Routes.Current != null)
            {
                RouteDocumentMaint graphRouteDocumentMaint = (RouteDocumentMaint)PXGraph.CreateInstance(typeof(RouteDocumentMaint));

                graphRouteDocumentMaint.RouteRecords.Current = graphRouteDocumentMaint
                                                               .RouteRecords.Search<FSAppointment.refNbr>(Routes.Current.RefNbr);

                throw new PXRedirectRequiredException(graphRouteDocumentMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }

        public PXAction<RouteWrkSheetFilter> OpenDriverSelector;
        [PXButton]
        [PXUIField(DisplayName = "Assign Driver", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openDriverSelector()
        {
            if (Routes.Current != null
                    && Routes.Current.Status != ID.Status_Route.CANCELED)
            {
                DriverRouteSelected.Current = DriverRouteSelected.Search<FSRouteDocument.routeDocumentID>(Routes.Current.RouteDocumentID);

                if (DriverRouteSelected.AskExt() == WebDialogResult.OK
                        && DriverRecords.Current != null
                            && Routes.Current.DriverID != DriverRecords.Current.BAccountID)
                {
                    Routes.Current.DriverID = DriverRecords.Current.BAccountID;
                    UpdateRoute(Routes.Current);                    
                }
            }
        }

        public PXAction<RouteWrkSheetFilter> OpenVehicleSelector;
        [PXButton]
        [PXUIField(DisplayName = "Assign Vehicle", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openVehicleSelector()
        {
            if (Routes.Current != null 
                    && VehicleFilter.Current != null
                        && Routes.Current.Status != ID.Status_Route.CANCELED)
            {
                VehicleFilter.Current.RouteDocumentID = Routes.Current.RouteDocumentID;
                VehicleRouteSelected.Current = VehicleRouteSelected.Search<FSRouteDocument.routeDocumentID>(Routes.Current.RouteDocumentID);

                if (VehicleRouteSelected.AskExt() == WebDialogResult.OK
                        && VehicleRecords.Current != null
                            && Routes.Current.VehicleID != VehicleRecords.Current.SMEquipmentID)
                {
                    Routes.Current.VehicleID = VehicleRecords.Current.SMEquipmentID;
                    UpdateRoute(Routes.Current);
                }
            }
        }

        #if (!Acumatica_5_10)
        public PXAction<RouteWrkSheetFilter> CreateNew;
        [PXInsertButton]
        [PXUIField(DisplayName = "")]
        protected virtual void createNew()
        {
            RouteDocumentMaint graphRouteDocumentMaint = (RouteDocumentMaint)PXGraph.CreateInstance(typeof(RouteDocumentMaint));
            graphRouteDocumentMaint.Clear(PXClearOption.ClearAll);
            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)graphRouteDocumentMaint.RouteRecords.Cache.CreateInstance();
            graphRouteDocumentMaint.RouteRecords.Insert(fsRouteDocumentRow);
            graphRouteDocumentMaint.RouteRecords.Cache.IsDirty = false;
            PXRedirectHelper.TryRedirect(graphRouteDocumentMaint, PXRedirectHelper.WindowMode.InlineWindow);
        }

        public PXAction<RouteWrkSheetFilter> EditDetail;
        [PXEditDetailButton]
        [PXUIField(DisplayName = "")]
        protected virtual void editDetail()
        {
            if (Routes.Current == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = PXSelect<FSRouteDocument,
                                                 Where<
                                                    FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>
                                                .SelectSingleBound(this, null, Routes.Current.RouteDocumentID);

            RouteDocumentMaint graphRouteDocumentMaint = (RouteDocumentMaint)PXGraph.CreateInstance(typeof(RouteDocumentMaint));

            graphRouteDocumentMaint.RouteRecords.Current = graphRouteDocumentMaint
                                                           .RouteRecords.Search<FSAppointment.refNbr>(fsRouteDocumentRow.RefNbr);

            throw new PXRedirectRequiredException(graphRouteDocumentMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endif
        #endregion

        #region EventFilter
        protected virtual IEnumerable vehicleRecords()
        {
            return VehicleSelectionHelper.VehicleRecordsDelegate(this, VehicleRouteSelected, VehicleFilter);
        }

        protected virtual IEnumerable driverRecords()
        {
            return DriverSelectionHelper.DriverRecordsDelegate(this, DriverRouteSelected, DriverFilter);
        }
        #endregion

        #region Event Handlers

        #region FSRouteDocument

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.vehicleID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRowDocumentRow = (FSRouteDocument)e.Row;

            UpdateRoute(fsRowDocumentRow);
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.additionalVehicleID1> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            UpdateRoute(fsRouteDocumentRow);
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.additionalVehicleID2> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            UpdateRoute(fsRouteDocumentRow);
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.driverID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            UpdateRoute(fsRouteDocumentRow);
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.additionalDriverID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            UpdateRoute(fsRouteDocumentRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSRouteDocument> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;
            PXCache cache = e.Cache;

            bool enableVehiceIDAndDriverIDFields = fsRouteDocumentRow.Status != ID.Status_Route.CANCELED;

            //The IsDirty flags get turned off so the 'save changes' confirmation is not thrown
            this.Routes.Cache.IsDirty = false;
            this.VehicleRouteSelected.Cache.IsDirty = false;
            this.DriverRouteSelected.Cache.IsDirty = false;

            PXUIFieldAttribute.SetEnabled<FSRouteDocument.refNbr>(cache, fsRouteDocumentRow, false);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.routeID>(cache, fsRouteDocumentRow, false);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.date>(cache, fsRouteDocumentRow, false);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.timeBegin>(cache, fsRouteDocumentRow, false);

            PXUIFieldAttribute.SetEnabled<FSRouteDocument.vehicleID>(cache, fsRouteDocumentRow, enableVehiceIDAndDriverIDFields);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.additionalVehicleID1>(cache, fsRouteDocumentRow, enableVehiceIDAndDriverIDFields);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.additionalVehicleID2>(cache, fsRouteDocumentRow, enableVehiceIDAndDriverIDFields);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.driverID>(cache, fsRouteDocumentRow, enableVehiceIDAndDriverIDFields);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.additionalDriverID>(cache, fsRouteDocumentRow, enableVehiceIDAndDriverIDFields);
        }

        protected virtual void _(Events.RowInserting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSRouteDocument> e)
        {
        }

        #endregion
        
        #endregion

        #region Virtual Functions
        /// <summary>
        /// Save the FSRouteDocument row.
        /// </summary>
        public virtual void UpdateRoute(FSRouteDocument fsRouteDocumentRow)
        {
            routeDocumentMaint.RouteRecords.Current = routeDocumentMaint.RouteRecords.Search<FSRouteDocument.routeDocumentID>(fsRouteDocumentRow.RouteDocumentID);

            FSRouteDocument fsRouteDocumentRow_New = routeDocumentMaint.RouteRecords.Current;

            if (fsRouteDocumentRow_New.DriverID != fsRouteDocumentRow.DriverID
                    || fsRouteDocumentRow_New.AdditionalDriverID != fsRouteDocumentRow.AdditionalDriverID
                        || fsRouteDocumentRow_New.VehicleID != fsRouteDocumentRow.VehicleID
                            || fsRouteDocumentRow_New.AdditionalVehicleID1 != fsRouteDocumentRow.AdditionalVehicleID1
                                || fsRouteDocumentRow_New.AdditionalVehicleID2 != fsRouteDocumentRow.AdditionalVehicleID2)
            {
                if (fsRouteDocumentRow_New.VehicleID != fsRouteDocumentRow.VehicleID)
                {
                    routeDocumentMaint.RouteRecords.SetValueExt<FSRouteDocument.vehicleID>
                        (routeDocumentMaint.RouteRecords.Current, fsRouteDocumentRow.VehicleID);
                }

                if (fsRouteDocumentRow_New.AdditionalVehicleID1 != fsRouteDocumentRow.AdditionalVehicleID1)
                {
                    routeDocumentMaint.RouteRecords.SetValueExt<FSRouteDocument.additionalVehicleID1>
                        (routeDocumentMaint.RouteRecords.Current, fsRouteDocumentRow.AdditionalVehicleID1);
                }

                if (fsRouteDocumentRow_New.AdditionalVehicleID2 != fsRouteDocumentRow.AdditionalVehicleID2)
                {
                    routeDocumentMaint.RouteRecords.SetValueExt<FSRouteDocument.additionalVehicleID2>
                        (routeDocumentMaint.RouteRecords.Current, fsRouteDocumentRow.AdditionalVehicleID2);
                }

                if (fsRouteDocumentRow_New.DriverID != fsRouteDocumentRow.DriverID)
                {
                    routeDocumentMaint.RouteRecords.SetValueExt<FSRouteDocument.driverID>
                        (routeDocumentMaint.RouteRecords.Current, fsRouteDocumentRow.DriverID);
                }

                if (fsRouteDocumentRow_New.AdditionalDriverID != fsRouteDocumentRow.AdditionalDriverID)
                {
                    routeDocumentMaint.RouteRecords.SetValueExt<FSRouteDocument.additionalDriverID>
                        (routeDocumentMaint.RouteRecords.Current, fsRouteDocumentRow.AdditionalDriverID);
                }

                routeDocumentMaint.RouteRecords.Update(routeDocumentMaint.RouteRecords.Current);
                routeDocumentMaint.Save.Press();
            }
        }
        #endregion
    }
}