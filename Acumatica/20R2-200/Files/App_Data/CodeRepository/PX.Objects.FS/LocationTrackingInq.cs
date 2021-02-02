using PX.Data;
using PX.FS;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    #region Filter
    [Serializable]
    [PXHidden]
    public class FSGPSTrackingHistoryFilter : IBqlTable
    {
        #region Date
        public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
        [PXDate(UseTimeZone = true)]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? Date { get; set; }
        #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        [PXDate(UseTimeZone = true)]
        [PXUIField(DisplayName = "Start Date")]
        public virtual DateTime? StartDate 
        {
            get
            {
                if (Date.HasValue == false) 
                {
                    return Date;
                }

                return new DateTime(Date.Value.Year, Date.Value.Month, Date.Value.Day, 0, 0, 0); ;
            }
        }
        #endregion
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
        [PXDate(UseTimeZone = true)]
        [PXUIField(DisplayName = "End Date")]
        public virtual DateTime? EndDate 
        {
            get 
            {
                if (Date.HasValue == false)
                {
                    return Date;
                }

                return new DateTime(Date.Value.Year, Date.Value.Month, Date.Value.Day, 23, 59, 59); ;
            } 
        }
        #endregion
    }
    #endregion

    public class LocationTrackingInq : PXGraph<LocationTrackingInq>
    {
        public LocationTrackingInq()
            : base()
        {
            LocationTrackingRecords.AllowInsert = false;
            LocationTrackingRecords.AllowUpdate = false;
            LocationTrackingRecords.AllowDelete = false;
        }

        #region internal types definition
        [Serializable]
        [PXProjection(typeof(Select4<FSGPSTrackingHistory,
                    Where<
                        FSGPSTrackingHistory.executionDate, GreaterEqual<Current<FSGPSTrackingHistoryFilter.startDate>>,
                    And<
                        FSGPSTrackingHistory.executionDate, LessEqual<Current<FSGPSTrackingHistoryFilter.endDate>>>>,
                    Aggregate<
                        GroupBy<FSGPSTrackingHistory.trackingID>>>))]
        public class LatestFSGPSTrackingHistory : IBqlTable
        {
            #region ExecutionDate
            public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }
            [PXDBDate(IsKey = true, PreserveTime = true, UseTimeZone = true, UseSmallDateTime = false, InputMask = "g", BqlField = typeof(FSGPSTrackingHistory.executionDate))]
            [PXDefault]
            [PXUIField(DisplayName = "Execution Date")]
            public virtual DateTime? ExecutionDate { get; set; }
            #endregion
            #region TrackingID
            public abstract class trackingID : PX.Data.BQL.BqlInt.Field<trackingID> { }
            [PXDBGuid(IsKey = true, BqlField = typeof(FSGPSTrackingHistory.trackingID))]
            public virtual Guid? TrackingID { get; set; }
            #endregion
            #region CreatedDateTime
            public abstract class createdDateTime : IBqlField
            {
            }
            [PXDBCreatedDateTime(BqlField = typeof(FSGPSTrackingHistory.createdDateTime))]
            public virtual DateTime? CreatedDateTime { get; set; }
            #endregion  
        }

        [Serializable]
        [PXProjection(typeof(
            Select2<FSGPSTrackingHistory,
                InnerJoin<LatestFSGPSTrackingHistory,
                    On<LatestFSGPSTrackingHistory.trackingID, Equal<FSGPSTrackingHistory.trackingID>,
                        And<LatestFSGPSTrackingHistory.executionDate, Equal<FSGPSTrackingHistory.executionDate>>>,
                LeftJoin<FSGPSTrackingRequest,
                    On<FSGPSTrackingRequest.trackingID, Equal<FSGPSTrackingHistory.trackingID>>,
                LeftJoin<Users,
                    On<Users.username, Equal<FSGPSTrackingRequest.userName>>>>>>))]
        public class FSGPSTrackingHistoryRaw : IBqlTable 
        {
            #region ExecutionDate
            public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }
            [PXDBDate(IsKey = true, PreserveTime = true, UseTimeZone = true, UseSmallDateTime = false, InputMask = "g", BqlField = typeof(FSGPSTrackingHistory.executionDate))]
            [PXDefault]
            [PXUIField(DisplayName = "Execution Date")]
            public virtual DateTime? ExecutionDate { get; set; }
            #endregion
            #region TrackingID
            public abstract class trackingID : PX.Data.BQL.BqlGuid.Field<trackingID> { }
            [PXDBGuid(IsKey = true, BqlField = typeof(FSGPSTrackingHistory.trackingID))]
            [PXDefault]
            [PXUIField(DisplayName = "Tracking ID")]
            public virtual Guid? TrackingID { get; set; }
            #endregion
            #region Latitude
            public abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }
            [PXDBDecimal(6, BqlField = typeof(FSGPSTrackingHistory.latitude))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Latitude", Enabled = false)]
            public virtual decimal? Latitude { get; set; }
            #endregion
            #region Longitude
            public abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }
            [PXDBDecimal(6, BqlField = typeof(FSGPSTrackingHistory.longitude))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Longitude", Enabled = false)]
            public virtual decimal? Longitude { get; set; }
            #endregion
            #region Altitude
            public abstract class altitude : PX.Data.BQL.BqlDecimal.Field<altitude> { }
            [PXDBDecimal(6, BqlField = typeof(FSGPSTrackingHistory.altitude))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Altitude", Enabled = false)]
            public virtual decimal? Altitude { get; set; }
            #endregion
            #region CreatedDateTime
            public abstract class createdDateTime : IBqlField { }
            [PXDBCreatedDateTime(BqlField = typeof(FSGPSTrackingHistory.createdDateTime))]
            public virtual DateTime? CreatedDateTime { get; set; }
            #endregion
            #region PKID
            public abstract class pKID : PX.Data.BQL.BqlInt.Field<pKID> { }

            [PXDBGuidMaintainDeleted(BqlField = typeof(Users.pKID))]
            public virtual Guid? PKID { get; set; }
            #endregion
            #region Username
            public abstract class username : PX.Data.BQL.BqlInt.Field<username> { }

            [PXDBString(BqlField = typeof(Users.username))]
            [PXUIField(DisplayName = "Login", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String Username { get; set; }
            #endregion
            #region FullName
            public abstract class fullName : PX.Data.BQL.BqlInt.Field<fullName> { }
            
            [PXDBString(BqlField = typeof(Users.fullName))]
            [PXUIField(DisplayName = "Full Name", Enabled = false)]
            public virtual String FullName { get; set; }
            #endregion
        }

        [PXVirtual]
        public class GPSTrackingHistory : IBqlTable
        {
            #region ExecutionDate
            public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }

            [PXDBDate(IsKey = true, PreserveTime = true, UseTimeZone = true, UseSmallDateTime = false, InputMask = "g")]
            [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Execution Date", Enabled = false)]
            public virtual DateTime? ExecutionDate { get; set; }
            #endregion
            #region TrackingID
            public abstract class trackingID : PX.Data.BQL.BqlGuid.Field<trackingID> { }

            [PXDBGuid(IsKey = true)]
            [PXDefault]
            [PXUIField(DisplayName = "Tracking ID", Enabled = false)]
            public virtual Guid? TrackingID { get; set; }
            #endregion
            #region Latitude
            public abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }

            [PXDBDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Latitude", Enabled = false)]
            public virtual decimal? Latitude { get; set; }
            #endregion
            #region Longitude
            public abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }

            [PXDBDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Longitude", Enabled = false)]
            public virtual decimal? Longitude { get; set; }
            #endregion
            #region Altitude
            public abstract class altitude : PX.Data.BQL.BqlDecimal.Field<altitude> { }

            [PXDBDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Altitude", Enabled = false)]
            public virtual decimal? Altitude { get; set; }
            #endregion
            #region CreatedDateTime
            public abstract class createdDateTime : IBqlField { }

            [PXDBCreatedDateTime()]
            public virtual DateTime? CreatedDateTime { get; set; }
            #endregion
            #region PKID
            public abstract class pKID : PX.Data.BQL.BqlInt.Field<pKID> { }

            [PXDBGuidMaintainDeleted()]
            public virtual Guid? PKID { get; set; }
            #endregion
            #region Username
            public abstract class username : PX.Data.BQL.BqlInt.Field<username> { }

            [PXDBString()]
            [PXUIField(DisplayName = "Login", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
            public virtual String Username { get; set; }
            #endregion
            #region FullName
            public abstract class fullName : PX.Data.BQL.BqlInt.Field<fullName> { }

            [PXDBString()]
            [PXUIField(DisplayName = "Full Name", Enabled = false)]
            public virtual String FullName { get; set; }
            #endregion
            #region Distance
            public abstract class distance : PX.Data.IBqlField { }

            [PXDecimal]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Distance", Enabled = false)]
            public virtual decimal? Distance { get; set; }
            #endregion
        }
        #endregion

        #region Selects
        public PXCancel<FSGPSTrackingHistoryFilter> Cancel;

        public PXFilter<FSGPSTrackingHistoryFilter> Filter;

        [PXFilterable]
        [PXVirtualDAC]
        public PXSelect<GPSTrackingHistory>
                LocationTrackingRecords;

        public PXSelect<FSGPSTrackingHistoryRaw,
                Where<
                    FSGPSTrackingHistoryRaw.executionDate, GreaterEqual<Current<FSGPSTrackingHistoryFilter.startDate>>,
                    And<
                        FSGPSTrackingHistoryRaw.executionDate, LessEqual<Current<FSGPSTrackingHistoryFilter.endDate>>>>>
                LocationTrackingRawRecords;

        public PXSelect<FSGPSTrackingHistoryRaw,
                Where<
                    FSGPSTrackingHistoryRaw.executionDate, GreaterEqual<Current<FSGPSTrackingHistoryFilter.startDate>>,
                    And<
                        FSGPSTrackingHistoryRaw.executionDate, LessEqual<Current<FSGPSTrackingHistoryFilter.endDate>>,
                    And<
                        FSGPSTrackingHistoryRaw.pKID, Equal<Current<AccessInfo.userID>>>>>>
                CurrentUserLocationTrackingRecords;

        public IEnumerable locationTrackingRecords()
        {
            this.Caches<FSGPSTrackingHistoryRaw>().Clear();
            this.Caches<FSGPSTrackingHistoryRaw>().ClearQueryCache();

            PXView select = new PXView(this, true, LocationTrackingRawRecords.View.BqlSelect);

            Int32 totalrow = 0;
            Int32 startrow = PXView.StartRow;

            List<object> result = select.Select(PXView.Currents, PXView.Parameters,
                   PXView.Searches, PXView.SortColumns, PXView.Descendings,
                   PXView.Filters, ref startrow, PXView.MaximumRows, ref totalrow);

            PXView.StartRow = 0;

            FSGPSTrackingHistoryRaw currentUserLocationRow = CurrentUserLocationTrackingRecords.Select();
            LatLng from = null;

            if (currentUserLocationRow != null
                    && currentUserLocationRow.Latitude != null
                    && currentUserLocationRow.Longitude != null)
            {
                from = new LatLng(currentUserLocationRow.Latitude, currentUserLocationRow.Longitude);
            }

            foreach (FSGPSTrackingHistoryRaw row in result)
            {
                LatLng to = new LatLng(row.Latitude, row.Longitude);

                GPSTrackingHistory location = new GPSTrackingHistory()
                {
                    ExecutionDate = row.ExecutionDate,
                    TrackingID = row.TrackingID,
                    Latitude = row.Latitude,
                    Longitude = row.Longitude,
                    Altitude = row.Altitude,
                    CreatedDateTime = row.CreatedDateTime,
                    PKID = row.PKID,
                    Username = row.Username,
                    FullName = row.FullName,
                    Distance = from != null ? Convert.ToDecimal(Haversine.calculate(from, to, Haversine.DistanceUnit.Miles)) : 0
                };

                GPSTrackingHistory located = LocationTrackingRecords.Cache.Locate(location) as GPSTrackingHistory;

                yield return (located ?? LocationTrackingRecords.Cache.Insert(location));
                LocationTrackingRecords.Cache.IsDirty = false;
            }
        }
        #endregion

        #region Actions
        public PXAction<FSGPSTrackingHistoryFilter> ViewGPSOnMap;
        [PXButton]
        [PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void viewGPSOnMap()
        {
            if (LocationTrackingRecords.Current != null)
            {
                var googleMap = new PX.Data.GoogleMapLatLongRedirector();

                googleMap.ShowAddressByLocation(LocationTrackingRecords.Current.Latitude, LocationTrackingRecords.Current.Longitude);
            }
        }
        #endregion
    }
}