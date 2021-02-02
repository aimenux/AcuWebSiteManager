using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class SM_AccessUsers : PXGraphExtension<AccessUsers>
    {
        public override void Initialize()
        {
            base.Initialize();
            LocationTracking.Cache.AllowInsert = false;
            LocationTracking.Cache.AllowDelete = false;
            LocationTracking.Cache.AllowUpdate = false;
        }

        #region Views
        [PXCopyPasteHiddenView]
        public PXSelect<FSGPSTrackingLocation, 
               Where<
                   FSGPSTrackingLocation.userName, Equal<Current<Users.username>>>,
               OrderBy<
                   Asc<FSGPSTrackingLocation.weekDay>>>
               LocationTracking;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<CSCalendar,
               InnerJoin<EPEmployee,
                   On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>,
               InnerJoin<BAccount,
                   On<BAccount.bAccountID, Equal<EPEmployee.bAccountID>>>>,
               Where<
                   BAccount.defContactID, Equal<Current<Users.contactID>>>>
               UserCalendar;
        #endregion

        #region Methods
        public virtual void EnableDisableLocationTracking(PXCache cache, UserPreferences userPreferencesRow, FSxUserPreferences fsxUserPreferencesRow)
        {
            PXUIFieldAttribute.SetEnabled<FSxUserPreferences.interval>(cache, userPreferencesRow, fsxUserPreferencesRow.TrackLocation == true);
            PXUIFieldAttribute.SetEnabled<FSxUserPreferences.distance>(cache, userPreferencesRow, fsxUserPreferencesRow.TrackLocation == true);
            LocationTracking.Cache.AllowInsert = fsxUserPreferencesRow.TrackLocation == true;
            LocationTracking.Cache.AllowDelete = fsxUserPreferencesRow.TrackLocation == true;
            LocationTracking.Cache.AllowUpdate = fsxUserPreferencesRow.TrackLocation == true;
        }

        public virtual void SetWeeklyOnDayFlag(FSGPSTrackingLocation fsGPSTrackingLocationRow)
        {
            switch (fsGPSTrackingLocationRow.WeekDay)
            {
                case (int?)DayOfWeek.Monday:
                    fsGPSTrackingLocationRow.WeeklyOnDay1 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay2 = true;
                    fsGPSTrackingLocationRow.WeeklyOnDay3 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay4 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay5 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay6 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay7 = false;
                    break;
                case (int?)DayOfWeek.Tuesday:
                    fsGPSTrackingLocationRow.WeeklyOnDay1 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay2 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay3 = true;
                    fsGPSTrackingLocationRow.WeeklyOnDay4 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay5 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay6 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay7 = false;
                    break;
                case (int?)DayOfWeek.Wednesday:
                    fsGPSTrackingLocationRow.WeeklyOnDay1 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay2 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay3 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay4 = true;
                    fsGPSTrackingLocationRow.WeeklyOnDay5 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay6 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay7 = false;
                    break;
                case (int?)DayOfWeek.Thursday:
                    fsGPSTrackingLocationRow.WeeklyOnDay1 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay2 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay3 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay4 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay5 = true;
                    fsGPSTrackingLocationRow.WeeklyOnDay6 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay7 = false;
                    break;
                case (int?)DayOfWeek.Friday:
                    fsGPSTrackingLocationRow.WeeklyOnDay1 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay2 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay3 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay4 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay5 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay6 = true;
                    fsGPSTrackingLocationRow.WeeklyOnDay7 = false;
                    break;
                case (int?)DayOfWeek.Saturday:
                    fsGPSTrackingLocationRow.WeeklyOnDay1 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay2 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay3 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay4 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay5 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay6 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay7 = true;
                    break;
                default:
                    fsGPSTrackingLocationRow.WeeklyOnDay1 = true;
                    fsGPSTrackingLocationRow.WeeklyOnDay2 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay3 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay4 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay5 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay6 = false;
                    fsGPSTrackingLocationRow.WeeklyOnDay7 = false;
                    break;
            }
        }

        public virtual void UpdateIntervalAndDistance(FSGPSTrackingLocation fsGPSTrackingLocationRow)
        {
            UserPreferences userPreferencesRow = Base.UserPrefs.Current;

            if (userPreferencesRow != null)
            {
                FSxUserPreferences fsxUserPreferencesRow = PXCache<UserPreferences>.GetExtension<FSxUserPreferences>(userPreferencesRow);
                fsGPSTrackingLocationRow.Interval = fsxUserPreferencesRow.Interval ?? 5;
                fsGPSTrackingLocationRow.Distance = fsxUserPreferencesRow.Distance ?? 250;
            }
        }
        #endregion

        #region Event Handlers

        #region Users

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<Users> e)
        {
        }

        protected virtual void _(Events.RowSelected<Users> e)
        {
        }

        protected virtual void _(Events.RowInserting<Users> e)
        {
        }

        protected virtual void _(Events.RowInserted<Users> e)
        {
        }

        protected virtual void _(Events.RowUpdating<Users> e)
        {
        }

        protected virtual void _(Events.RowUpdated<Users> e)
        {
        }

        protected virtual void _(Events.RowDeleting<Users> e)
        {
        }

        protected virtual void _(Events.RowDeleted<Users> e)
        {
        }

        protected virtual void _(Events.RowPersisting<Users> e)
        {
            if (e.Row == null)
            {
                return;
            }

            Users usersRow = e.Row as Users;
            UserPreferences userPrefRow = Base.UserPrefs.Current;

            if (userPrefRow != null)
            {
                FSxUserPreferences fsxUserPreferencesRow = PXCache<UserPreferences>.GetExtension<FSxUserPreferences>(userPrefRow);

                if (fsxUserPreferencesRow != null
                        && fsxUserPreferencesRow.TrackLocation == true
                            && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
                {
                    if (userPrefRow.TimeZone == null)
                    {
                        Base.UserPrefs.Cache.RaiseExceptionHandling<UserPreferences.timeZone>(
                                userPrefRow,
                                userPrefRow.TimeZone,
                                new PXSetPropertyException(TX.Error.TIME_ZONE_REQUIRED_LOCATION_TRACKING_ENABLED, PXErrorLevel.Error));
                    }
                    else
                    {
                        string timeZone = (string)Base.UserPrefs.Cache.GetValueOriginal<UserPreferences.timeZone>(userPrefRow);

                        if (timeZone != userPrefRow.TimeZone)
                        {
                            foreach (FSGPSTrackingLocation fsGPSTrackingLocationRow in LocationTracking.Select())
                            {
                                fsGPSTrackingLocationRow.TimeZoneID = userPrefRow.TimeZone;
                                LocationTracking.Cache.Update(fsGPSTrackingLocationRow);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void _(Events.RowPersisted<Users> e)
        {
        }

        #endregion

        #region UserPreferences

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<UserPreferences, FSxUserPreferences.trackLocation> e)
        {
            if (e.Row == null)
            {
                return;
            }

            UserPreferences userPreferencesRow = e.Row as UserPreferences;
            FSxUserPreferences fsxUserPreferencesRow = e.Cache.GetExtension<FSxUserPreferences>(userPreferencesRow);

            if (fsxUserPreferencesRow != null
                    && fsxUserPreferencesRow.TrackLocation != (bool)e.OldValue)
            {
                if (fsxUserPreferencesRow.TrackLocation == true && LocationTracking.Select().Count == 0)
                {
                    List<FSGPSTrackingLocation> trackingLocations = new List<FSGPSTrackingLocation>();
                    CSCalendar csCalendarRow = UserCalendar.SelectSingle();

                    if (csCalendarRow?.SunWorkDay == true)
                    {
                        trackingLocations.Add(new FSGPSTrackingLocation
                        {
                            WeeklyOnDay1 = true,
                            WeekDay = 0,
                            StartTime = csCalendarRow.SunStartTime,
                            EndTime = csCalendarRow.SunEndTime
                        });
                    }

                    if (csCalendarRow?.MonWorkDay == true)
                    {
                        trackingLocations.Add(new FSGPSTrackingLocation
                        {
                            WeeklyOnDay2 = true,
                            WeekDay = 1,
                            StartTime = csCalendarRow.MonStartTime,
                            EndTime = csCalendarRow.MonEndTime
                        });
                    }

                    if (csCalendarRow?.TueWorkDay == true)
                    {
                        trackingLocations.Add(new FSGPSTrackingLocation
                        {
                            WeeklyOnDay3 = true,
                            WeekDay = 2,
                            StartTime = csCalendarRow.TueStartTime,
                            EndTime = csCalendarRow.TueEndTime
                        });
                    }

                    if (csCalendarRow?.WedWorkDay == true)
                    {
                        trackingLocations.Add(new FSGPSTrackingLocation
                        {
                            WeeklyOnDay4 = true,
                            WeekDay = 3,
                            StartTime = csCalendarRow.WedStartTime,
                            EndTime = csCalendarRow.WedEndTime
                        });
                    }

                    if (csCalendarRow?.ThuWorkDay == true)
                    {
                        trackingLocations.Add(new FSGPSTrackingLocation
                        {
                            WeeklyOnDay5 = true,
                            WeekDay = 4,
                            StartTime = csCalendarRow.ThuStartTime,
                            EndTime = csCalendarRow.ThuEndTime
                        });
                    }

                    if (csCalendarRow?.FriWorkDay == true)
                    {
                        trackingLocations.Add(new FSGPSTrackingLocation
                        {
                            WeeklyOnDay6 = true,
                            WeekDay = 5,
                            StartTime = csCalendarRow.FriStartTime,
                            EndTime = csCalendarRow.FriEndTime
                        });
                    }

                    if (csCalendarRow?.SatWorkDay == true)
                    {
                        trackingLocations.Add(new FSGPSTrackingLocation
                        {
                            WeeklyOnDay7 = true,
                            WeekDay = 6,
                            StartTime = csCalendarRow.SatStartTime,
                            EndTime = csCalendarRow.SatEndTime
                        });
                    }

                    foreach (FSGPSTrackingLocation fsGPSTrackingLocationRow in trackingLocations)
                    {
                        fsGPSTrackingLocationRow.StartDate = Base.Accessinfo.BusinessDate;
                        fsGPSTrackingLocationRow.EndDate = fsGPSTrackingLocationRow.StartDate.Value.AddYears(1000);
                        fsGPSTrackingLocationRow.Interval = fsxUserPreferencesRow.Interval;
                        fsGPSTrackingLocationRow.Distance = fsxUserPreferencesRow.Distance;
                        LocationTracking.Insert(fsGPSTrackingLocationRow);
                    }
                }
                else
                {
                    foreach (FSGPSTrackingLocation fsGPSTrackingLocationRow in LocationTracking.Select())
                    {
                        fsGPSTrackingLocationRow.IsActive = fsxUserPreferencesRow.TrackLocation;
                        LocationTracking.Cache.Update(fsGPSTrackingLocationRow);
                    }
                }

                PXPersistingCheck persistingCheck = fsxUserPreferencesRow.TrackLocation == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
                PXDefaultAttribute.SetPersistingCheck<UserPreferences.timeZone>(e.Cache, userPreferencesRow, persistingCheck);
            }
        }

        protected virtual void _(Events.FieldUpdated<UserPreferences, UserPreferences.timeZone> e)
        {
            if (e.Row == null)
            {
                return;
            }

            UserPreferences userPreferencesRow = e.Row as UserPreferences;

            if (userPreferencesRow.TimeZone != (string)e.OldValue)
            {
                foreach (FSGPSTrackingLocation fsGPSLocationTrackingRow in LocationTracking.Select())
                {
                    fsGPSLocationTrackingRow.TimeZoneID = userPreferencesRow.TimeZone;
                    LocationTracking.Update(fsGPSLocationTrackingRow);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<UserPreferences, FSxUserPreferences.interval> e)
        {
            if (e.Row == null)
            {
                return;
            }

            UserPreferences userPreferencesRow = e.Row as UserPreferences;
            FSxUserPreferences fsxUserPreferencesRow = PXCache<UserPreferences>.GetExtension<FSxUserPreferences>(userPreferencesRow);

            if (fsxUserPreferencesRow.Interval != (short)e.OldValue)
            {
                foreach (FSGPSTrackingLocation fsGPSLocationTrackingRow in LocationTracking.Select())
                {
                    fsGPSLocationTrackingRow.Interval = fsxUserPreferencesRow.Interval;
                    LocationTracking.Update(fsGPSLocationTrackingRow);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<UserPreferences, FSxUserPreferences.distance> e)
        {
            if (e.Row == null)
            {
                return;
            }

            UserPreferences userPreferencesRow = e.Row as UserPreferences;
            FSxUserPreferences fsxUserPreferencesRow = PXCache<UserPreferences>.GetExtension<FSxUserPreferences>(userPreferencesRow);

            if (fsxUserPreferencesRow.Distance != (short)e.OldValue)
            {
                foreach (FSGPSTrackingLocation fsGPSLocationTrackingRow in LocationTracking.Select())
                {
                    fsGPSLocationTrackingRow.Interval = fsxUserPreferencesRow.Distance;
                    LocationTracking.Update(fsGPSLocationTrackingRow);
                }
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowSelected<UserPreferences> e)
        {
            if (e.Row == null)
            {
                return;
            }

            UserPreferences userPreferencesRow = e.Row as UserPreferences;
            PXCache cache = e.Cache;

            FSxUserPreferences fsxUserPreferencesRow = cache.GetExtension<FSxUserPreferences>(userPreferencesRow);
            EnableDisableLocationTracking(cache, userPreferencesRow, fsxUserPreferencesRow);
        }

        protected virtual void _(Events.RowInserting<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowInserted<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowUpdating<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowUpdated<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowDeleting<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowDeleted<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowPersisting<UserPreferences> e)
        {
        }

        protected virtual void _(Events.RowPersisted<UserPreferences> e)
        {
        }

        #endregion

        #region FSGPSTrackingLocation

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<FSGPSTrackingLocation, FSGPSTrackingLocation.weekDay> e)
        {
            if (e.Row == null)
            {
                return;
            }

            SetWeeklyOnDayFlag((FSGPSTrackingLocation)e.Row);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSGPSTrackingLocation> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSGPSTrackingLocation fsGPSTrackingLocationRow = e.Row as FSGPSTrackingLocation;

            if (fsGPSTrackingLocationRow.WeeklyOnDay1 == true)
            {
                fsGPSTrackingLocationRow.WeekDay = (int?)DayOfWeek.Sunday;
            }
            else if (fsGPSTrackingLocationRow.WeeklyOnDay2 == true)
            {
                fsGPSTrackingLocationRow.WeekDay = (int?)DayOfWeek.Monday;
            }
            else if (fsGPSTrackingLocationRow.WeeklyOnDay3 == true)
            {
                fsGPSTrackingLocationRow.WeekDay = (int?)DayOfWeek.Tuesday;
            }
            else if (fsGPSTrackingLocationRow.WeeklyOnDay4 == true)
            {
                fsGPSTrackingLocationRow.WeekDay = (int?)DayOfWeek.Wednesday;
            }
            else if (fsGPSTrackingLocationRow.WeeklyOnDay5 == true)
            {
                fsGPSTrackingLocationRow.WeekDay = (int?)DayOfWeek.Thursday;
            }
            else if (fsGPSTrackingLocationRow.WeeklyOnDay6 == true)
            {
                fsGPSTrackingLocationRow.WeekDay = (int?)DayOfWeek.Friday;
            }
            else if (fsGPSTrackingLocationRow.WeeklyOnDay7 == true)
            {
                fsGPSTrackingLocationRow.WeekDay = (int?)DayOfWeek.Saturday;
            }
        }

        protected virtual void _(Events.RowSelected<FSGPSTrackingLocation> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSGPSTrackingLocation> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSGPSTrackingLocation> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSGPSTrackingLocation fsGPSTrackingLocationRow = e.Row as FSGPSTrackingLocation;
            UserPreferences userPreferencesRow = Base.UserPrefs.Current;

            if (userPreferencesRow != null)
            {
                FSxUserPreferences fsxUserPreferencesRow = PXCache<UserPreferences>.GetExtension<FSxUserPreferences>(userPreferencesRow);
                fsGPSTrackingLocationRow.Interval = fsxUserPreferencesRow.Interval;
                fsGPSTrackingLocationRow.Distance = fsxUserPreferencesRow.Distance;
            }
        }

        protected virtual void _(Events.RowUpdating<FSGPSTrackingLocation> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSGPSTrackingLocation> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSGPSTrackingLocation> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSGPSTrackingLocation> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSGPSTrackingLocation> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSGPSTrackingLocation fsGPSTrackingLocationRow = e.Row as FSGPSTrackingLocation;

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                SetWeeklyOnDayFlag(fsGPSTrackingLocationRow);
                UpdateIntervalAndDistance(fsGPSTrackingLocationRow);

                // @TODO: This must be a configuring setting?
                if (e.Operation == PXDBOperation.Insert)
                {
                    fsGPSTrackingLocationRow.StartDate = Base.Accessinfo.BusinessDate;
                    fsGPSTrackingLocationRow.EndDate = fsGPSTrackingLocationRow.StartDate.Value.AddYears(1000);
                }

                if (fsGPSTrackingLocationRow.TimeZoneID == null)
                {
                    Base.UserPrefs.Cache.RaiseExceptionHandling<UserPreferences.timeZone>(
                        Base.UserPrefs.Current,
                        Base.UserPrefs.Current.TimeZone,
                        new PXSetPropertyException(TX.Error.TIME_ZONE_REQUIRED_LOCATION_TRACKING_ENABLED, PXErrorLevel.Error));

                    e.Cache.RaiseExceptionHandling<FSGPSTrackingLocation.weekDay>(
                        fsGPSTrackingLocationRow,
                        fsGPSTrackingLocationRow.WeekDay,
                        new PXSetPropertyException(TX.Error.TIME_ZONE_REQUIRED_LOCATION_TRACKING_ENABLED, PXErrorLevel.RowError));
                }
            }
        }

        protected virtual void _(Events.RowPersisted<FSGPSTrackingLocation> e)
        {
        }

        #endregion

        #endregion
    }
}