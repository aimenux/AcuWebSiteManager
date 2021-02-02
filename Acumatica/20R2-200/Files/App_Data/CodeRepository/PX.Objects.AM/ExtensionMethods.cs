using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Extension Methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Trim a string value without the need to first check for null
        /// </summary>
        /// <param name="value"></param>
        /// <returns>null when null, otherwise the string trimmed</returns>
        public static string TrimIfNotNull(this string value)
        {
            return value?.Trim();
        }

        /// <summary>
        /// Trim a string value without the need to first check for null
        /// (Null returns empty string)
        /// </summary>
        /// <param name="value"></param>
        /// <returns>empty string when null, otherwise the string trimmed</returns>
        public static string TrimIfNotNullEmpty(this string value)
        {
            return value != null ? value.Trim() : string.Empty;
        }

        /// <summary>
        /// Pads right 1 more space if the string contains a value
        /// </summary>
        public static string PadRightSpace(this string value)
        {
            return PadRightSpace(value, 1);
        }

        /// <summary>
        /// Pads right a given set of spaces if the string contains a value
        /// </summary>
        public static string PadRightSpace(this string value, int totalSpaces)
        {
            return string.IsNullOrEmpty(value) ? value : value.PadRight(value.Length + totalSpaces);
        }

        /// <summary>
        /// Compare a pair of strings trimming both values and excluding case
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool EqualsWithTrim(this string value1, string value2)
        {
            return string.Compare(value1.TrimIfNotNullEmpty(), value2.TrimIfNotNullEmpty(), System.StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Reverse the string characters
        /// </summary>
        public static string Reverse(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static bool StartsWithNullIgnore(this string value, string startsWithValue)
        {
            return !string.IsNullOrWhiteSpace(value) && 
                   !string.IsNullOrWhiteSpace(startsWithValue) &&
                   value.StartsWith(startsWithValue);
        }

        public static int CompareNullDates(this DateTime? date1, DateTime? date2)
        {
            return Common.Dates.Compare(date1, date2);
        }

        /// <summary>
        ///  Determines if a given value is between two values (including a matching value)
        /// </summary>
        /// <returns>True if between the from/to values</returns>
        public static bool BetweenInclusive(this IComparable check, IComparable from, IComparable to)
        {
            return check.CompareTo(from) >= 0 && check.CompareTo(to) <= 0;
        }

        /// <summary>
        /// Determines if a given value is between two values (excluding a matching value)
        /// </summary>
        /// <returns>True if between the from/to values</returns>
        public static bool BetweenExclusive(this IComparable check, IComparable from, IComparable to)
        {
            return check.CompareTo(from) > 0 && check.CompareTo(to) < 0;
        }

        /// <summary>
        /// Returns the date of the previous day of week for the given date time
        /// </summary>
        /// <param name="dt">Date to get the previous date from</param>
        /// <param name="dayOfWeek">previous day of week to return</param>
        /// <returns>Date of the previous day of week. 
        /// When date matches the day of week the same date is returned.
        /// When date is null the return is null</returns>
        public static DateTime? PreviousDateOf(this DateTime? dt, DayOfWeek dayOfWeek)
        {
            if (dt == null)
            {
                return null;
            }

            return PreviousDateOf(dt.GetValueOrDefault(), dayOfWeek);
        }

        /// <summary>
        /// Returns the date of the previous day of week for the given date time
        /// </summary>
        /// <param name="dt">Date to get the previous date from</param>
        /// <param name="dayOfWeek">previous day of week to return</param>
        /// <returns>Date of the previous day of week. When date matches the day of week the same date is returned.</returns>
        public static DateTime PreviousDateOf(this DateTime dt, DayOfWeek dayOfWeek)
        {
            int diff = dt.DayOfWeek - dayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff);
        }

        /// <summary>
        /// Returns the date of the next day of week for the given date time
        /// </summary>
        /// <param name="dt">Date to get the next date from</param>
        /// <param name="dayOfWeek">next day of week to return</param>
        /// <returns>Date of the next day of week. 
        /// When date matches the day of week the same date is returned.
        /// When date is null the return is null</returns>
        public static DateTime? NextDateOf(this DateTime? dt, DayOfWeek dayOfWeek)
        {
            if (dt == null)
            {
                return null;
            }

            return NextDateOf(dt.GetValueOrDefault(), dayOfWeek);
        }

        /// <summary>
        /// Returns the date of the next day of week for the given date time
        /// </summary>
        /// <param name="dt">Date to get the next date from</param>
        /// <param name="dayOfWeek">next day of week to return</param>
        /// <returns>Date of the next day of week. When date matches the day of week the same date is returned.</returns>
        public static DateTime NextDateOf(this DateTime dt, DayOfWeek dayOfWeek)
        {
            int diff = dayOfWeek - dt.DayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(diff);
        }

        public static bool TryConvertToBool<T>(this T from , out bool to)
        {
            try
            {
                to = System.Convert.ToBoolean(from);
                return true;
            }
            catch (Exception e)
            {
                if (e is FormatException 
                    || e.InnerException is FormatException
                    || e is InvalidCastException
                    || e.InnerException is InvalidCastException)
                {
                    to = false;
                    return false;
                }

                throw;
            }
        }

        public static bool TryAddDays(this DateTime dt, int days, out DateTime dtPlusDays)
        {
            try
            {
                dtPlusDays = dt.AddDays(days);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                dtPlusDays = dt;
            }

            return false;
        }

        /// <summary>
        /// Compare public DAC fields to see if values are each equal.
        /// (Excludes audit and tstamp fields)
        /// </summary>
        /// <typeparam name="T">DAC table</typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns>True when all public properties are equal</returns>
        public static bool DacFieldsEqual<T>(this T obj1, T obj2) where T : IBqlTable
        {
            return DacFieldsEqual(obj1, obj2, IgnoreDacFieldList);
        }

        /// <summary>
        /// Compare public DAC fields to see if values are each equal.
        /// </summary>
        /// <typeparam name="T">DAC table</typeparam>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="ignore">property names to ignore in comparison</param>
        /// <returns>True when all public properties are equal</returns>
        /// <remarks>Sourced: http://stackoverflow.com/questions/506096/comparing-object-properties-in-c-sharp </remarks>
        public static bool DacFieldsEqual<T>(this T obj1, T obj2, params string[] ignore) where T : IBqlTable
        {
            if (obj1 == null || obj2 == null)
            {
                return obj1.Equals(obj2);
            }

            Type type = typeof(T);
            var ignoreList = new System.Collections.Generic.List<string>(ignore).ConvertAll(d => d.ToLowerInvariant());
            foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (ignoreList.Contains(pi.Name.ToLowerInvariant()))
                {
                    continue;
                }

                object selfValue = type.GetProperty(pi.Name).GetValue(obj1, null);
                object toValue = type.GetProperty(pi.Name).GetValue(obj2, null);

                if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                {
                    return false;
                }
            }
            return true;
        }

        private static string[] IgnoreDacFieldList
        {
            get
            {
                return new[] { "tstamp", "CreatedByID", "CreatedByScreenID", "CreatedDateTime", "LastModifiedByID", "LastModifiedByScreenID", "LastModifiedDateTime" };
            }
        }

        /// <summary>
        /// Returns a graphs long running process Timespan
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static TimeSpan GetLongRunningTimeSpan<T>(this T graph) where T : PX.Data.PXGraph
        {
            TimeSpan timespan;
            Exception ex;
            PX.Data.PXLongOperation.GetStatus(graph.UID, out timespan, out ex);
            return timespan;
        }

        public static decimal NotLessZero(this decimal? value)
        {
            return value.GetValueOrDefault() < 0m ? 0m : value.GetValueOrDefault();
        }

        public static decimal NotLessZero(this decimal value)
        {
            return value < 0m ? 0m : value;
        }

        public static int NotLessZero(this int? value)
        {
            return value.GetValueOrDefault() < 0 ? 0 : value.GetValueOrDefault();
        }

        public static int NotLessZero(this int value)
        {
            return value < 0 ? 0 : value;
        }

        public static string Indent(this string originalString, int indentLevel)
        {
            return indentLevel <= 0 || originalString == null
                ? originalString
                : $"{"".PadLeft(indentLevel * 4)}{originalString}";
        }

        public static int ToCeilingInt(this decimal value)
        {
            return value == 0 ? 0 : Convert.ToInt32(Math.Ceiling(Math.Round(value, 5)));
        }

        public static int ToCeilingInt(this decimal? value)
        {
            return value.GetValueOrDefault() == 0 ? 0 : ToCeilingInt(value.GetValueOrDefault());
        }


        private const int DefaultHoursRound = 3;
        public static decimal? ToHours(this int? value)
        {
            return ToHours(value, DefaultHoursRound);
        }

        public static decimal? ToHours(this int? value, int roundDecimals)
        {
            return value == null ? (decimal?)null : ToHours(value.GetValueOrDefault(), roundDecimals);
        }

        public static decimal ToHours(this int value)
        {
            return ToHours(value, DefaultHoursRound);
        }

        public static decimal ToHours(this int value, int roundDecimals)
        {
            return Math.Round(value / 60m, roundDecimals);
        }

        public static decimal? ToHours(this decimal? value)
        {
            return ToHours(value, DefaultHoursRound);
        }

        public static decimal? ToHours(this decimal? value, int roundDecimals)
        {
            return value == null ? (decimal?)null : ToHours(value.GetValueOrDefault(), roundDecimals);
        }

        public static decimal ToHours(this decimal value)
        {
            return ToHours(value, DefaultHoursRound);
        }

        public static decimal ToHours(this decimal value, int roundDecimals)
        {
            return Math.Round(value / 60m, roundDecimals);
        }

        public static bool GreaterThan(this DateTime? value, DateTime? compare)
        {
            if (value == null && compare != null)
            {
                return false;
            }

            if (value != null && compare == null)
            {
                return true;
            }

            return GreaterThan(value.GetValueOrDefault(), compare.GetValueOrDefault());
        }

        public static bool GreaterThan(this DateTime value, DateTime compare)
        {
            return DateTime.Compare(value, compare) > 0;
        }

        public static bool GreaterThanOrEqualTo(this DateTime? value, DateTime? compare)
        {
            if (value == null && compare != null)
            {
                return false;
            }

            if (value != null && compare == null)
            {
                return true;
            }

            return GreaterThanOrEqualTo(value.GetValueOrDefault(), compare.GetValueOrDefault());
        }

        public static bool GreaterThanOrEqualTo(this DateTime value, DateTime compare)
        {
            return DateTime.Compare(value, compare) >= 0;
        }

        public static bool LessThan(this DateTime? value, DateTime? compare)
        {
            if (value == null || compare == null)
            {
                return false;
            }

            return LessThan(value.GetValueOrDefault(), compare.GetValueOrDefault());
        }

        public static bool LessThan(this DateTime value, DateTime compare)
        {
            return DateTime.Compare(value, compare) < 0;
        }

        public static bool LessThanOrEqualTo(this DateTime? value, DateTime? compare)
        {
            if (value == null || compare == null)
            {
                return false;
            }

            return LessThanOrEqualTo(value.GetValueOrDefault(), compare.GetValueOrDefault());
        }

        public static bool LessThanOrEqualTo(this DateTime value, DateTime compare)
        {
            return DateTime.Compare(value, compare) <= 0;
        }

        public static DateTime? GreaterDateTime(this DateTime? value1, DateTime? value2)
        {
            return GreaterDateTime(value1.GetValueOrDefault(), value2.GetValueOrDefault());
        }

        public static DateTime GreaterDateTime(this DateTime value1, DateTime value2)
        {
            return GreaterThan(value1, value2) ? value1 : value2;
        }

        public static DateTime? LesserDateTime(this DateTime? value1, DateTime? value2)
        {
            return LesserDateTime(value1.GetValueOrDefault(), value2.GetValueOrDefault());
        }

        public static DateTime LesserDateTime(this DateTime value1, DateTime value2)
        {
            return LessThan(value1, value2) ? value1 : value2;
        }

        public static string ToAppendedNewLineString(this List<string> list)
        {
            if (list == null || list.Count == 0)
            {
                return null;
            }
            var sb = new System.Text.StringBuilder();
            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                sb.AppendLine(item);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Are the given values of the same sign (+, +) or (-, -)
        /// </summary>
        public static bool IsSameSign(this decimal? decimal1, decimal? decimal2)
        {
            return IsSameSign(decimal1.GetValueOrDefault(), decimal2.GetValueOrDefault());
        }

        /// <summary>
        /// Are the given values of the same sign (+, +) or (-, -)
        /// </summary>
        public static bool IsSameSign(this decimal decimal1, decimal decimal2)
        {
            return decimal1 >= 0 && decimal2 >= 0 || decimal1 < 0 && decimal2 < 0;
        }

        public static string ToCapitalized(this string text)
        {
            return string.IsNullOrEmpty(text)
                ? text
                : char.ToUpper(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// Add the key to the dictionary or if found update the existing value
        /// </summary>
        public static void AddOrIncrease(this Dictionary<string, decimal> dic, string key, decimal increase)
        {
            if(dic == null)
            {
                return;
            }

            var currentValue = 0m;
            if(dic.ContainsKey(key))
            {
                currentValue = dic[key];
            }
            dic[key] = currentValue + increase;
        }

        public static T Copy<T>(this T entity) where T : class, new()
        {
            if (entity == null)
            {
                return null;
            }

            var copy = new T();

            foreach (System.ComponentModel.PropertyDescriptor item in System.ComponentModel.TypeDescriptor.GetProperties(entity))
            {
                if (item.Name == "tstamp")
                {
                    continue;
                }

                item.SetValue(copy, item.GetValue(entity));
            }

            return copy;
        }

        /// <summary>
        /// Split a collection into X number of sub lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="parts"></param>
        /// <remarks>https://stackoverflow.com/questions/438188/split-a-collection-into-n-parts-with-linq</remarks>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            if (list == null)
            {
                return null;
            }

            var i = 0;
            var splits = from item in list
                group item by i++ % parts into part
                select part.AsEnumerable();
            return splits;
        }

        /// <summary>
        /// truncate a decimal value by only including the number of places
        /// </summary>
        /// <remarks>https://codereview.stackexchange.com/questions/51951/truncate-decimal-places</remarks>
        public static decimal TruncateDecimalPlaces(this decimal val, int places)
        {
            if (places < 0)
            {
                throw new ArgumentException(nameof(places));
            }
            return Math.Round(val - Convert.ToDecimal((0.5 / Math.Pow(10, places))), places);
        }
    }
}