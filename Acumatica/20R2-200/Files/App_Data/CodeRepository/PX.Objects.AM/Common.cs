using System;
using System.Collections.Generic;
using PX.Data;
using System.Reflection;
using PX.Common;
using PX.Data.BQL;
using PX.Data.Update;

namespace PX.Objects.AM
{
    public static class Common
    {
        public const string ModuleAM = "AM";

        /// <summary>
        /// AM%
        /// </summary>
        public class AMwildcard_ : BqlType<IBqlString, string>.Constant<AMwildcard_>
        {
            public AMwildcard_()
                : base("AM%")
            {
            }
        }

        static Common()
        {
            _db = PXInstanceHelper.DatabaseInfo.DatabaseName;
#if DEBUG
            AMDebug.TraceWriteLine("[AM COMMON] Version = {0} ; Date = {1}", AMVersionNumber, AMVersionDate);
#endif
        }

        public static bool IsPortal => PXSiteMap.IsPortal;

        /// <summary>
        /// Represents the Manufacturing module string "AM" for use in BQL Statements
        /// </summary>
        public sealed class moduleAM : PX.Data.BQL.BqlString.Constant<moduleAM>
        {
            public moduleAM() : base(Common.ModuleAM)
            {
            }
        }

        public const string ERPEmptyLicenseKey = "00000000";
        public static string ERPVersionNumber => PXVersionInfo.Version;

        internal static string ERPcustomer => PXLicenseHelper.License?.CustomerName;
        internal static string ERPlk => PXLicenseHelper.License?.LicenseKey;
        private static string _db;
        internal static string ERPdb => _db;
        internal static string InstallID => PXVersionInfo.InstallationID;

        /// <summary>
        /// Get the current Manufacturing library version number
        /// </summary>
        /// <returns></returns>
        public static string AMVersionNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_versionNumber))
                {
                    _versionNumber = FormatVersionNumber(Assembly.GetExecutingAssembly().GetName().Version);
                }
                return _versionNumber ?? string.Empty;
            }
        }

        public static string FormatVersionNumber(Version version)
        {
            return version == null
                ? string.Empty
                : string.Join(".",
                    version.Major,
                    version.Minor,
                    version.Build.ToString().PadLeft(4, '0'),
                    version.Revision.ToString().PadLeft(2, '0'));
        }

        private static string _versionNumber;

        /// <summary>
        /// Get the current Manufacturing library version number
        /// </summary>
        /// <returns></returns>
        public static string AMMajorVersionNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_majorVersionNumber))
                {
                    _majorVersionNumber = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
                }
                return _majorVersionNumber ?? string.Empty;
            }
        }

        private static string _majorVersionNumber;

        /// <summary>
        /// Get the current Manufacturing library version number
        /// </summary>
        /// <returns></returns>
        public static DateTime AMVersionDate
        {
            get
            {
                if (!_versionDate.HasValue)
                {
                    _versionDate = GetVersionDateEasternTime();
                }
                return _versionDate ?? new System.DateTime();
            }
        }

        private static System.DateTime? _versionDate;

        /// <summary>
        /// Retrieves the linker timestamp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <remarks>http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html</remarks>
        private static System.DateTime RetrieveLinkerTimestamp(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Common.Dates.BeginOfTimeDate;
            }

            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var b = new byte[2048];
            System.IO.FileStream s = null;
            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            return new System.DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(System.BitConverter.ToInt32(b,
                System.BitConverter.ToInt32(b, peHeaderOffset) + linkerTimestampOffset));
            //return dt.AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
        }

        private static DateTime GetVersionDateEasternTime()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Common.Dates.BeginOfTimeDate;
            }

            return RetrieveLinkerTimestamp(filePath).AddHours(-5);
        }

        public static class SiteMap
        {
            /// <summary>
            /// Returns the screen title based on the screen ID as it exists in the site map
            /// </summary>
            /// <param name="screenID">ScreenID</param>
            /// <returns>Screen Title</returns>
            public static string GetScreenTitle(string screenID)
            {
                if (string.IsNullOrWhiteSpace(screenID))
                {
                    return string.Empty;
                }

                //Requires reference to System.Web
                var sm = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID);

                return sm == null ? string.Empty : sm.Title;
            }

            /// <summary>
            /// Returns the screen title based on the screens graph type as it exists in the site map
            /// </summary>
            /// <param name="graphType">Screen graph type</param>
            /// <returns>Screen Title</returns>
            public static string GetScreenTitle(Type graphType)
            {
                //Requires reference to System.Web
                var sm = PXSiteMap.Provider.FindSiteMapNode(graphType);

                return sm == null ? string.Empty : sm.Title;
            }

            /// <summary>
            /// Returns the screen ID based on the screens graph type as it exists in the site map
            /// </summary>
            /// <param name="graphType">Screen graph type</param>
            /// <returns>Screen ID</returns>
            public static string GetScreenID(Type graphType)
            {
                //Requires reference to System.Web
                var sm = PXSiteMap.Provider.FindSiteMapNode(graphType);

                return sm == null ? string.Empty : sm.ScreenID;
            }
        }

        public static class Cache
        {
            public static string GetCacheName(Type cacheType)
            {
                if (cacheType.IsDefined(typeof(PXCacheNameAttribute), true))
                {
                    PXCacheNameAttribute attr =
                        (PXCacheNameAttribute) (cacheType.GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
                    return attr.GetName();
                }

                return cacheType.Name;
            }

            /// <summary>
            /// Returns the current enabled status of a given field
            /// </summary>
            public static bool? GetEnabled<Field>(PXCache cache, object data) where Field : IBqlField
            {
                return GetEnabled(cache, data, typeof(Field).Name);
            }

            /// <summary>
            /// Returns the current enabled status of a given field
            /// </summary>
            public static bool? GetEnabled(PXCache cache, object data, string fieldName)
            {
                if (data == null)
                {
                    return null;
                }

                foreach (PXUIFieldAttribute pxuiFieldAttribute in cache.GetAttributesOfType<PXUIFieldAttribute>(data, fieldName))
                {
                    if (pxuiFieldAttribute != null)
                    {
                        return pxuiFieldAttribute.Enabled;
                    }
                }

                return null;
            }

            public static T GetFirstAttributeOfType<T>(PXCache cache, object data, string fieldName)
                where T : PXEventSubscriberAttribute
            {
                if (data == null)
                {
                    return null;
                }

                foreach (T attribute in cache.GetAttributesOfType<T>(data, fieldName))
                {
                    if (attribute != null)
                    {
                        return attribute;
                    }
                }

                return null;
            }

            /// <summary>
            /// Add a given DAC view to a graphs cache 
            /// </summary>
            /// <typeparam name="TCache"></typeparam>
            /// <param name="graph">graph to add cache to</param>
            /// <returns>True if view added</returns>
            public static bool AddCacheView<TCache>(PXGraph graph) where TCache : IBqlTable
            {
                return AddCacheView(graph, typeof(TCache));
            }

            /// <summary>
            /// Add a given DAC view to a graphs cache 
            /// </summary>
            /// <param name="graph">graph to add cache to</param>
            /// <param name="cacheType">Type of cache</param>
            /// <returns>True if view added</returns>
            public static bool AddCacheView(PXGraph graph, Type cacheType)
            {
                if (!graph.Views.Caches.Contains(cacheType))
                {
#if DEBUG
                    AMDebug.TraceWriteMethodName(cacheType?.Name);
#endif
                    graph.Views.Caches.Add(cacheType);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Add a given DAC to a graphs cache 
            /// </summary>
            /// <typeparam name="TCache"></typeparam>
            /// <param name="graph">graph to add cache to</param>
            /// <returns>True if cache added</returns>
            public static bool AddCache<TCache>(PXGraph graph) where TCache : class, IBqlTable, new()
            {
                if (!graph.Caches.ContainsKey(typeof(TCache)))
                {
                    var newCache = new PXCache<TCache>(graph);
                    newCache.Load();
                    graph.Caches[typeof(TCache)] = newCache;
#if DEBUG
                    AMDebug.TraceWriteMethodName(typeof(TCache).Name);
#endif
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Find the given row in cache, if not return the row given
            /// </summary>
            public static dac FindRow<dac>(PXGraph graph, dac row) where dac : class, IBqlTable, new()
            {
                return (dac)graph.Caches[typeof(dac)].LocateElse(row);
            }

            public static void TransferCache(PXGraph fromGraph, PXGraph toGraph)
            {
                TransferCache(fromGraph, toGraph, null);
            }

            public static void TransferCache(PXGraph fromGraph, PXGraph toGraph, params Type[] excludedCacheTypes)
            {
                var excludedCacheNames = new HashSet<string>();
                if (excludedCacheTypes != null)
                {
                    foreach (var excludedCacheType in excludedCacheTypes)
                    {
                        excludedCacheNames.Add(excludedCacheType.Name);
                    }
                }

                foreach (var cacheKvp in fromGraph.Caches)
                {
                    var fromCache = cacheKvp.Value;
                    if (fromCache == null || !fromCache.IsDirty || excludedCacheNames.Contains(cacheKvp.Key.Name))
                    {
                        continue;
                    }

                    var toCache = toGraph.Caches[cacheKvp.Key];
#if DEBUG
                    var addedCacheAsView = false;
#endif
                    if(fromGraph.Views.Caches.Contains(cacheKvp.Key))
                    {
#if DEBUG
                        addedCacheAsView = 
#endif
                        AddCacheView(toGraph, fromCache.GetItemType());
                    }
#if DEBUG
                    AMDebug.TraceWriteMethodName($"Cache: {cacheKvp.Key.Name}; Inserted: {fromCache.Inserted?.Count()}; Updated: {fromCache.Updated?.Count()}; Deleted: {fromCache.Deleted?.Count()}; AddedCacheAsView: {addedCacheAsView}".Indent(1));
#endif
                    TransferCacheInserted(fromCache, toCache);
                    TransferCacheUpdated(fromCache, toCache);
                    TransferCacheDeleted(fromCache, toCache);
                }
            }

            public static void TransferCacheInserted(PXCache fromCache, PXCache toCache)
            {
                if (fromCache?.Inserted == null || toCache == null)
                {
                    return;
                }

                foreach (var insertedRow in fromCache.Inserted)
                {
                    try
                    {
                        //if to cache contains the same key... lets just call the update for what has already been inserted
                        var locatedToCache = toCache.Locate(insertedRow);
                        if (locatedToCache == null)
                        {
                            toCache.Insert(insertedRow);
                            continue;
                        }

                        toCache.Update(insertedRow);
                    }
                    catch (Exception e)
                    {
                        PXTrace.WriteError($"Error processing {toCache.GetItemType()}. Row keys {toCache.Graph.GetRowKeyValues(insertedRow, toCache.GetItemType())}. {e.Message}");
                        throw;
                    }
                }
            }

            public static void TransferCacheUpdated(PXCache fromCache, PXCache toCache)
            {
                if (fromCache?.Updated == null || toCache == null)
                {
                    return;
                }

                foreach (var updatedRow in fromCache.Updated)
                {
                    try
                    {
                        toCache.Update(updatedRow);
                    }
                    catch (Exception e)
                    {
                        PXTrace.WriteError($"Error processing {toCache.GetItemType()}. Row keys {toCache.Graph.GetRowKeyValues(updatedRow, toCache.GetItemType())}. {e.Message}");
                        throw;
                    }
                }
            }

            public static void TransferCacheDeleted(PXCache fromCache, PXCache toCache)
            {
                if (fromCache?.Deleted == null || toCache == null)
                {
                    return;
                }

                foreach (var deletedRow in fromCache.Deleted)
                {
                    try
                    {
                        toCache.Delete(deletedRow);
                    }
                    catch (Exception e)
                    {
                        PXTrace.WriteError($"Error processing {toCache.GetItemType()}. Row keys {toCache.Graph.GetRowKeyValues(deletedRow, toCache.GetItemType())}. {e.Message}");
                        throw;
                    }
                }
            }
        }

        public static class Current
        {
            public const string CompanyIDKey = "CompanyID";

            public static int CompanyID
            {
                get { return PX.Data.Update.PXInstanceHelper.CurrentCompany; }
            }

            public static string CompanyName
            {
                get { return PXAccess.GetCompanyName(); }
            }

            /// <summary>
            /// Return the current business date of the graph
            /// </summary>
            /// <param name="graph">calling graph used to get the business date</param>
            /// <returns>Non-nullable DateTime</returns>
            public static DateTime BusinessDate(PXGraph graph)
            {
                return graph.Accessinfo.BusinessDate ?? Dates.UtcToday;
            }

            public static DateTime BusinessTimeOfDay(PXGraph graph)
            {
                //business date only stores date, use actual time without seconds
                var nowDate = graph.Accessinfo.BusinessDate ?? Dates.Now;
                return nowDate.Date + new TimeSpan(Dates.Now.TimeOfDay.Hours, Dates.Now.TimeOfDay.Minutes, 0);
            }

        }

        public static class Dates
        {
            public static DateTime UtcNow => PX.Common.PXTimeZoneInfo.UtcNow;

            public static DateTime UtcToday => PX.Common.PXTimeZoneInfo.UtcToday;

            public static DateTime Now => PX.Common.PXTimeZoneInfo.Now;

            public static DateTime Today => PX.Common.PXTimeZoneInfo.Today;

            /// <summary>
            /// 6/6/2079
            /// </summary>
            public static DateTime EndOfTimeDate => new DateTime(2079, 6, 6, 0, 0, 0);

            /// <summary>
            /// 1/1/1900
            /// </summary>
            public static DateTime BeginOfTimeDate => new DateTime(1900, 1, 1, 0, 0, 0);

            /// <summary>
            /// <see cref="Now"/> without seconds
            /// </summary>
            public static DateTime NowTimeOfDay => Now.Date + new TimeSpan(Now.TimeOfDay.Hours, Now.TimeOfDay.Minutes, 0);

            /// <summary>
            /// Formats the DateTime to the correct culture string value - date and time portion.
            /// This is useful for passing date parameters as string when the culture might have DD/MM/YYYY vs the US standard of MM/DD/YYYY
            /// </summary>
            public static string ToCultureString(PXGraph graph, DateTime? dateTime)
            {
                if (dateTime == null)
                {
                    return null;
                }

                var culture = Equals(graph.Culture, System.Globalization.CultureInfo.InvariantCulture)
                    ? LocaleInfo.GetUICulture()
                    : graph.Culture;

                return dateTime.GetValueOrDefault().ToString(culture);
            }

            /// <summary>
            /// Formats the DateTime to the correct culture string value - date portion only.
            /// This is useful for display date values in strings
            /// </summary>
            public static string ToCultureShortDateString(PXGraph graph, DateTime? dateTime)
            {
                if (dateTime == null)
                {
                    return null;
                }

                var culture = Equals(graph.Culture, System.Globalization.CultureInfo.InvariantCulture)
                    ? LocaleInfo.GetUICulture()
                    : graph.Culture;

                return dateTime.GetValueOrDefault().ToString("d", culture);
            }

            /// <summary>
            /// Checks if a date is null or 1/1/1900
            /// </summary>
            /// <param name="dateTime"></param>
            /// <returns></returns>
            public static bool IsDateNull(DateTime? dateTime)
            {
                DateTime nonNullNullDateTime = BeginOfTimeDate;

                return DatesEqual(dateTime ?? nonNullNullDateTime, nonNullNullDateTime);
            }


            public static bool DatesEqual(DateTime? date1, DateTime? date2)
            {
                return DatesEqual(date1 ?? BeginOfTimeDate, date2 ?? BeginOfTimeDate);
            }

            public static bool DatesEqual(DateTime date1, DateTime date2)
            {
                return DateTime.Compare(date1, date2) == 0;
            }

            /// <summary>
            /// Check if the date is begin/end of time date
            /// </summary>
            /// <param name="dateTime"></param>
            /// <returns>True if the date matches the begin or end of time dates</returns>
            public static bool IsDefaultDate(DateTime dateTime)
            {
                return DateTime.Compare(dateTime, BeginOfTimeDate) == 0 ||
                       DateTime.Compare(dateTime, EndOfTimeDate) == 0;
            }

            public static bool IsDefaultDate(DateTime? dateTime)
            {
                return IsDefaultDate(dateTime ?? BeginOfTimeDate);
            }

            public static bool IsMinMaxDate(DateTime dateTime)
            {
                return DateTime.Compare(dateTime, DateTime.MinValue) == 0 ||
                       DateTime.Compare(dateTime, DateTime.MaxValue) == 0;
            }

            /// <summary>
            /// Get the next weekday date from a date that may or may not be on a weekend
            /// </summary>
            /// <param name="date">Date to convert to the next weekday</param>
            /// <param name="calculateBackward">Should the weekday be before the weekend (if true) or after the weekend date (false)</param>
            /// <returns></returns>
            public static DateTime NextWeekday(DateTime date, bool calculateBackward = false)
            {
                int direction = (calculateBackward ? -1 : 1);
                DateTime nextDay = date;

                while (IsWeekendDate(nextDay))
                {
                    nextDay = nextDay.AddDays(direction);
                }

                return nextDay;
            }

            /// <summary>
            /// Is provided date a weekend date
            /// </summary>
            /// <param name="date">Date to check</param>
            /// <returns>true when day is saturday or sunday</returns>
            public static bool IsWeekendDate(DateTime date)
            {
                return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
            }

            /// <summary>
            /// Determine if start date/time is before end date/time
            /// </summary>
            /// <returns>True if start date/time is before end date/time</returns>
            public static bool StartBeforeEnd(DateTime startDateTime, DateTime endDateTime)
            {
                return DateTime.Compare(startDateTime, endDateTime) <= 0;
            }

            /// <summary>
            /// Determine if start date/time is before end date/time with nulls allowed
            /// </summary>
            /// <returns>True if start date/time is before end date/time</returns>
            public static bool StartBeforeEnd(DateTime? startDateTime, DateTime? endDateTime)
            {
                return StartBeforeEnd(startDateTime ?? BeginOfTimeDate, endDateTime ?? EndOfTimeDate);
            }

            public static int Compare(DateTime? startDateTime, DateTime? endDateTime)
            {
                return DateTime.Compare(startDateTime ?? BeginOfTimeDate, endDateTime ?? EndOfTimeDate);
            }

            public static int DaysBetween(DateTime? date1, DateTime? date2)
            {
                if (IsDefaultDate(date1) ||
                    IsDefaultDate(date2))
                {
                    return 0;
                }

                return DaysBetween(date1 ?? BeginOfTimeDate, date2 ?? BeginOfTimeDate);
            }

            public static int DaysBetween(DateTime date1, DateTime date2)
            {
                return date1.Subtract(date2).Days;
            }
        }

        public static class Xml
        {
            public static string RemoveCDATA(string s)
            {
                return s?.Replace("![CDATA[", "").Replace("]]", "");
            }

            public static string RemoveCDATAXml(string s)
            {
                return s?.Replace("<![CDATA[", "").Replace("]]>", "");
            }
        }

        public static class Strings
        {
            public static string TimespanFormatedDisplay(TimeSpan timeSpan)
            {
                if (timeSpan.Days > 0)
                {
                    return timeSpan.ToString(@"dd\.hh\:mm\:ss");
                }

                if (timeSpan.TotalSeconds <= 5)
                {
                    return timeSpan.ToString(@"hh\:mm\:ss\.fff");
                }

                return timeSpan.ToString(@"hh\:mm\:ss");
            }

            public static string TimespanMessageVerbose(TimeSpan timeSpan)
            {
                const string seconds = "seconds";

                string timeDisplay = seconds;
                double timeUnits = timeSpan.TotalSeconds < 0.9999 ? 0 : timeSpan.TotalSeconds;
                int decimalPlaces = 0;

                if (Math.Round(timeUnits, decimalPlaces, MidpointRounding.AwayFromZero) >= 60)
                {
                    decimalPlaces = 1;
                    timeDisplay = "minutes";
                    timeUnits = timeSpan.TotalMinutes < 1 ? 1 : timeSpan.TotalMinutes;
                }

                if (timeUnits >= 60)
                {
                    decimalPlaces = 2;
                    timeDisplay = "hours";
                    timeUnits = timeSpan.TotalHours;
                }

                if (timeUnits == 0 && timeDisplay == seconds)
                {
                    decimalPlaces = 0;
                    timeDisplay = "milliseconds";
                    timeUnits = timeSpan.TotalMilliseconds;
                }

                return $"{Math.Round(timeUnits, decimalPlaces, MidpointRounding.AwayFromZero)} {timeDisplay}";
            }

            /// <summary>
            /// Write a timespan as a runtime message
            /// </summary>
            /// <param name="timeSpan">timespan representing runtime</param>
            /// <returns></returns>
            public static string TimespanMessage(TimeSpan timeSpan)
            {
                return TimespanFormatedDisplay(timeSpan);
            }
        }

        public static class BQLConstants
        {
            public class int60 : PX.Data.BQL.BqlInt.Constant<int60>
            {
                public const int Int60 = (int)60;
                public int60() : base(Int60)
                {
                    
                }
            }

            public class decimal60 : PX.Data.BQL.BqlDecimal.Constant<decimal60>
            {
                public const decimal Decimal60 = (decimal)60.0;
                public decimal60() : base(Decimal60)
                {

                }
            }
        }
    }
}

