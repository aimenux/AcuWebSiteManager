using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;
using PX.Reports.ARm;
using PX.CS;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CS
{
    public class RMReportConstants
    {
        public enum WildcardMode
        {
            Normal,
            Fixed
        }

        public enum BetweenMode
        {
            ByChar,
            Fixed
        }

        public const char DefaultWildcardChar = '_';
        public static readonly char[] WildcardChars = { '?', ' ' };

        public const char RangeIntersectionChar = '|';
        public const char RangeUnionChar = ',';
        public const char RangeDelimiterChar = ':';

        public const char NonExpandingWildcardChar = '*';
    }

    public static class RMReportWildcard
    {
        public static void ConcatenateRangeWithDataSet(ARmDataSet target, ARmDataSet source, object startKey, object endKey, MergingMode mergingMode)
        {
            string startValue = NormalizeDsValue(source[startKey]);
            string endValue = NormalizeDsValue(source[endKey]);
	        char mergeChar = default(char);
	        switch (mergingMode)
	        {
		        case MergingMode.Intersection:
			        mergeChar = RMReportConstants.RangeIntersectionChar;
			        break;
				case MergingMode.Union:
			        mergeChar = RMReportConstants.RangeUnionChar;
			        break;
				default:
					throw new NotSupportedException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.MergingModeNotSupported, mergingMode));
	        }

            if (!(startValue == String.Empty && endValue == String.Empty))
            {
                if (startValue.Contains(RMReportConstants.RangeDelimiterChar) && endValue != String.Empty)
                {
                    throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ValueShouldBeEmpty, startKey, startValue, endKey, endValue));
                }
                else if (startValue == String.Empty && endValue != String.Empty)
                {
                    throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.DataSourceIncomplete, endKey, startKey, endValue));
                }

                if (!String.IsNullOrEmpty(target[startKey] as string))
                {
                    // There's existing data stored; we will intersect/union previous range with this new range at a later stage
					target[startKey] += mergeChar.ToString();
                }

                target[startKey] += startValue;
                if (endValue != String.Empty)
                {
                    target[startKey] += RMReportConstants.RangeDelimiterChar.ToString() + endValue;
                }
            }
        }

        public static string WildcardToFixed(string str, string wildcard)
        {
            if (str == null) return null;
            return wildcard == null || (wildcard.Length - str.Length) <= 0
                                ? str
                                : str + new string(' ', wildcard.Length - str.Length);
        }

        public static string EnsureWildcard(string str, string wildcard)
        {
            return EnsureWildcard(str, wildcard, RMReportConstants.WildcardChars);
        }

        public static string EnsureWildcardForFixed(string str, string wildcard)
        {
            return EnsureWildcard(WildcardToFixed(str, wildcard), wildcard, RMReportConstants.WildcardChars.Where(c => c != ' ').ToArray());
        }

        private static string EnsureWildcard(string str, string wildcard, char[] wildcardChars)
        {
            if (str == null || wildcard == null || wildcardChars == null) return str;
            var outputString = new System.Text.StringBuilder(wildcard.Length);
            foreach (char c in str)
            {
                bool wildcardFound = false;
                foreach (char w in wildcardChars)
                {
                    if (c == w)
                    {
                        outputString.Append(RMReportConstants.DefaultWildcardChar);
                        wildcardFound = true;
                        break;
                    }
                }

                if (!wildcardFound) outputString.Append(c);
            }

            if (str.Length < wildcard.Length)
            {
                outputString.Append(wildcard, 0, wildcard.Length - str.Length);
            }

            return outputString.ToString();
        }

        public static IEnumerable<T> GetBetween<T>(string start, string end, string wildcard, IEnumerable items, Func<T, string> convertFunc)
        {
            return GetBetween<T>(start, end, wildcard, items, convertFunc, RMReportConstants.WildcardChars);
        }

        public static IEnumerable<T> GetBetweenForFixed<T>(string start, string end, string wildcard, IEnumerable items, Func<T, string> convertFunc)
        {
            return GetBetween<T>(start, end, wildcard, items, convertFunc, RMReportConstants.WildcardChars.Where(c => c != ' ').ToArray());
        }

        private static IEnumerable<T> GetBetween<T>(string start, string end, string wildcard, IEnumerable items, Func<T, string> convertFunc, char[] wildcardChars)
        {
            string @from = EnsureWildcard(start, wildcard, wildcardChars).Replace(RMReportConstants.DefaultWildcardChar, ' ');
            string @to = (end.Replace(wildcardChars, 'z') + new string('z', wildcard.Length)).Substring(0, wildcard.Length);
            return from T item in items where IsBetween(@from, @to, convertFunc(item)) select item;
        }

        public static bool IsLike(string mask, string value)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                if (i >= value.Length || mask[i] != value[i] && mask[i] != '_')
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsBetween(string from, string to, string value)
        {
            return String.Compare(value, from) >= 0 && String.Compare(value, to) <= 0;
        }

        public static bool IsBetweenByChar(string from, string to, string value)
        {
            bool skipfrom = false;
            bool skipto = false;
            for (int i = 0; i < value.Length; i++)
            {
                if (!skipfrom && i < from.Length && !RMReportConstants.WildcardChars.Contains(from[i]))
                {
                    if (value[i] < from[i])
                        return false;
                    else if (value[i] > from[i])
                        skipfrom = true;
                }
                if (!skipto && i < to.Length && !RMReportConstants.WildcardChars.Contains(to[i]))
                {
                    if (value[i] > to[i])
                        return false;
                    else if (value[i] < to[i])
                        skipto = true;
                }
            }
            return true;
        }

        [PXInternalUseOnly]
        public static string NormalizeDsValue(object value)
        {
	        string sval = value as string;
	        return String.IsNullOrWhiteSpace(sval) ? String.Empty : sval;
        }
    }

    public static class RMReportUnitExpansion<T>
    {
        public static List<ARmUnit> ExpandUnit(RMReportReader report, RMDataSource ds, ARmUnit unit, object startKey, object endKey, Func<ARmDataSet, List<T>> fetchRangePredicate, Func<T, string> unitCodePredicate, Func<T, string> unitDescriptionPredicate, Action<T, string> applyWildcardToItemAction)
        {
            string nonExpandingWildcard = null;
            PreProcessNonExpandingWildcardChar(unit.DataSet, startKey, out nonExpandingWildcard);

            //This will validate Start/End range and merge the pair into a single range-like value that FillSubsList expects.
            ARmDataSet rangeToFetch = new ARmDataSet();
            RMReportWildcard.ConcatenateRangeWithDataSet(rangeToFetch, unit.DataSet, startKey, endKey, MergingMode.Intersection);

            List<T> unitItems = fetchRangePredicate(rangeToFetch);

            if (nonExpandingWildcard != null)
            {
                //TODO: Build a better auto-description based on segment values instead of copying code to description?
                unitItems = ReduceListByNonExpandingWildcard(nonExpandingWildcard, unitItems,
                    (u) => unitCodePredicate(u),
                    (u, mv) => applyWildcardToItemAction(u, mv));
            }

            switch (ds.RowDescription.Trim().ToUpper())
            {
                case RowDescriptionType.CodeDescription:
                    unitItems.Sort((x, y) => (unitCodePredicate(x) + unitDescriptionPredicate(x)).CompareTo(unitCodePredicate(y) + unitDescriptionPredicate(y)));
                    break;
                case RowDescriptionType.DescriptionCode:
                    unitItems.Sort((x, y) => (unitDescriptionPredicate(x) + unitCodePredicate(x)).CompareTo(unitDescriptionPredicate(y) + unitCodePredicate(y)));
                    break;
                case RowDescriptionType.Description:
                    unitItems.Sort((x, y) => unitDescriptionPredicate(x).CompareTo(unitDescriptionPredicate(y)));
                    break;
                default:
                    unitItems.Sort((x, y) => unitCodePredicate(x).CompareTo(unitCodePredicate(y)));
                    break;
            }

            List<ARmUnit> units = new List<ARmUnit>();
            int n = 0;
            foreach (T unitItem in unitItems)
            {
                n++;
                var u = new ARmUnit();
                report.FillDataSource(ds, u.DataSet, report.Report.Current.Type);
                u.DataSet[startKey] = unitCodePredicate(unitItem);
                u.DataSet[endKey] = null;
                u.Code = unit.Code + n.ToString("D5");

                switch (ds.RowDescription.Trim())
                {
                    case RowDescriptionType.CodeDescription:
                        u.Description = string.Format("{0}{1}{2}", unitCodePredicate(unitItem).Trim(), "-", unitDescriptionPredicate(unitItem));
                        break;
                    case RowDescriptionType.DescriptionCode:
                        u.Description = string.Format("{0}{1}{2}", unitDescriptionPredicate(unitItem), "-", unitCodePredicate(unitItem).Trim());
                        break;
                    case RowDescriptionType.Description:
                        u.Description = unitDescriptionPredicate(unitItem);
                        break;
                    default:
                        u.Description = unitCodePredicate(unitItem).Trim();
                        break;
                }

                u.Formula = unit.Formula;
                u.PrintingGroup = unit.PrintingGroup;
                units.Add(u);
            }
            return units;
        }

        private static void PreProcessNonExpandingWildcardChar(ARmDataSet dataSet, object key, out string nonExpandingWildcard)
        {
            bool hasWildcard = false;

            string originalValue = (dataSet[key] as string ?? "");
            StringBuilder processedValue = new StringBuilder(originalValue.Length);

            for (int i = 0; i < originalValue.Length; i++)
            {
                if (originalValue[i] == RMReportConstants.NonExpandingWildcardChar)
                {
                    processedValue.Append(RMReportConstants.DefaultWildcardChar);
                    hasWildcard = true;
                }
                else
                {
                    processedValue.Append(originalValue[i]);
                }
            }

            if (hasWildcard == true)
            {
                dataSet[key] = processedValue.ToString();
                nonExpandingWildcard = originalValue;
            }
            else
            {
                nonExpandingWildcard = null;
            }
        }

        private static List<T> ReduceListByNonExpandingWildcard(string nonExpandingWildcard, List<T> list, Func<T, string> value, Action<T, string> applyWildcardToItemAction)
        {
            HashSet<string> wildcards = new HashSet<string>();
            List<T> maskedValues = new List<T>();
            foreach (T item in list)
            {
                string maskedValue = MaskStringWithWildcard(value(item), nonExpandingWildcard);
                if (!wildcards.Contains(maskedValue))
                {
                    wildcards.Add(maskedValue);
                    applyWildcardToItemAction(item, maskedValue);
                    maskedValues.Add(item);
                }
            }
            return maskedValues;
        }

        private static string MaskStringWithWildcard(string value, string nonExpandingWildcard)
        {
            if (value.Length > nonExpandingWildcard.Length) throw new ArgumentException(Messages.WildcardSmallerMasked);

            StringBuilder maskedString = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                if (nonExpandingWildcard[i] == RMReportConstants.NonExpandingWildcardChar)
                {
                    maskedString.Append(RMReportConstants.DefaultWildcardChar);
                }
                else
                {
                    maskedString.Append(value[i]);
                }
            }

            return maskedString.ToString();
        }
    }

    public class RMReportRange<T> : RMReportRange
    {
        private T _instance;
        private PXCache _cache;
        private Dictionary<string, List<T>> _ranges;
        private Dictionary<string, HashSet<T>> _rangeSegments;
        private string _wildcard;
        private RMReportConstants.WildcardMode _wildcardMode;
        private RMReportConstants.BetweenMode _betweenMode;

        public RMReportRange(PXGraph graph, string dimensionName, RMReportConstants.WildcardMode wildcardMode, RMReportConstants.BetweenMode betweenMode)
        {
            _cache = graph.Caches[typeof(T)];
            _instance = (T)_cache.CreateInstance();
            _ranges = new Dictionary<string, List<T>>();
            _rangeSegments = new Dictionary<string, HashSet<T>>();

            Dimension dim = PXSelect<Dimension,
                Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>>>
                .Select(graph, dimensionName);
            if (dim != null && dim.Length != null)
            {
                _wildcard = new String(RMReportConstants.DefaultWildcardChar, (int)dim.Length);
            }
            else
            {
                _wildcard = "";
            }

            _wildcardMode = wildcardMode;
            _betweenMode = betweenMode;
        }

        public T Instance
        {
            get { return _instance; }
        }

        public string Wildcard
        {
            get { return _wildcard; }
        }

        public PXCache Cache
        {
            get { return _cache; }
        }

        public List<T> GetItemsInRange(string range, Func<T, string> getValuePredicate, Action<T, string> prepareForLocatePredicate)
        {
            return GetItemsInRange(range, r => r, r => GetItems(r, getValuePredicate, prepareForLocatePredicate));
        }

        public List<T> GetItemsInRange(string range, Func<string, string> cacheKey, Func<string, HashSet<T>> getItemsInRangePredicate)
        {
            List<T> list = null;

            if (_ranges.TryGetValue(cacheKey(range ?? ""), out list))
            {
                return list;
            }
            else
            {
				HashSet<T> items = null;
				string[] ranges = String.IsNullOrEmpty(range) ? new string[] { String.Empty } : range.Split(RMReportConstants.RangeIntersectionChar);

                // Process each range to extract list of items in this range, and then intersect it with previous datasource
                foreach (string r in ranges)
                {
                    HashSet<T> currentRangeItems;
                    if (!_rangeSegments.TryGetValue(cacheKey(r), out currentRangeItems))
                    {
                        currentRangeItems = getItemsInRangePredicate(r);
                        _rangeSegments.Add(cacheKey(r), currentRangeItems);
                    }

                    if (ranges.Length == 1)
                    {
                        //Only one range, no need to copy to hashtable for intersecting
                        items = currentRangeItems;
                    }
                    else if (items == null)
                    {
                        //First pass
                        items = new HashSet<T>(currentRangeItems);
                    }
                    else
                    {
						//On subsequent pass, only keep accounts that intersect with previous list
						items.IntersectWith(currentRangeItems);
                    }
                }

                list = new List<T>(items);
                _ranges.Add(cacheKey(range ?? ""), list);
            }

            return list;
        }

        private HashSet<T> GetItems(string range, Func<T, string> getValuePredicate, Action<T, string> prepareForLocatePredicate)
        {
			//Check existed items by whole range 
	        var items = GetItems(range, null, getValuePredicate, prepareForLocatePredicate);
	        if (items?.Count > 0) return items;

	        var startEndPair = range.Split(RMReportConstants.RangeDelimiterChar);
	        if (startEndPair?.Length == 2)
	        {
		        items = GetItems(startEndPair[0], startEndPair[1], getValuePredicate, prepareForLocatePredicate);
		        if (items?.Count > 0) return items;
	        }

	        items = new HashSet<T>();
            string[] pairs = range.Split(RMReportConstants.RangeUnionChar);

            foreach (string pair in pairs)
            {
                string start, end;
                ParseRangeStartEndPair(pair, out start, out end);

                items.UnionWith(GetItems(start, end, getValuePredicate, prepareForLocatePredicate));
            }
            return items;
        }

	    private HashSet<T> GetItems(string start, string end, Func<T, string> getValuePredicate,
		    Action<T, string> prepareForLocatePredicate)
	    {
		    var items = new HashSet<T>();

		    if (!String.IsNullOrEmpty(start))
		    {
			    if (String.IsNullOrEmpty(end) || end == start)
			    {
				    string itemCode = String.Empty;

				    if (_wildcardMode == RMReportConstants.WildcardMode.Fixed)
				    {
					    itemCode = RMReportWildcard.EnsureWildcardForFixed(start, _wildcard);
				    }
				    else if (_wildcardMode == RMReportConstants.WildcardMode.Normal)
				    {
					    itemCode = RMReportWildcard.EnsureWildcard(start, _wildcard);
				    }
				    else throw new ArgumentException(Messages.InvalidWildcardMode);

				    if (itemCode.Contains(RMReportConstants.DefaultWildcardChar))
				    {
					    items.UnionWith(from T x in _cache.Cached
						    where RMReportWildcard.IsLike(itemCode, getValuePredicate(x))
						    select x);
				    }
				    else
				    {
					    prepareForLocatePredicate(_instance, itemCode);
					    if (_cache.IsKeysFilled(_instance))
					    {
						    T x = (T) _cache.Locate(_instance);
						    if (x != null)
						    {
							    items.Add(x);
						    }
					    }
					    else // composite key
					    {
						    items.UnionWith(from T x in _cache.Cached
							    where String.Equals(itemCode, getValuePredicate(x), StringComparison.Ordinal)
							    select x);
					    }
				    }
			    }
			    else
			    {
				    if (_betweenMode == RMReportConstants.BetweenMode.ByChar)
				    {
					    items.UnionWith(from T x in _cache.Cached
						    where RMReportWildcard.IsBetweenByChar(start, end, getValuePredicate(x))
						    select x);
				    }
				    else if (_betweenMode == RMReportConstants.BetweenMode.Fixed)
				    {
					    items.UnionWith(
						    RMReportWildcard.GetBetweenForFixed<T>(start, end, _wildcard, _cache.Cached, getValuePredicate));
				    }
				    else throw new ArgumentException(Messages.InvalidBetweenMode);
			    }
		    }
		    else
		    {
			    items.UnionWith(_cache.Cached.Cast<T>());
		    }
		    return items;
	    }
    }

	public abstract class RMReportRange
	{
		public static void ParseRangeStartEndPair(string range, out string start, out string end)
		{
			if (String.IsNullOrEmpty(range))
			{
				start = String.Empty;
				end = String.Empty;
			}
			else
			{
				string[] splittedRange = range.Split(RMReportConstants.RangeDelimiterChar);
				if (splittedRange.Length == 2)
				{
					start = splittedRange[0];
					end = splittedRange[1];
				}
				else if (splittedRange.Length == 1)
				{
					start = splittedRange[0];
					end = start;
				}
				else
				{
					throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.RangeInvalid, range));
				}
			}
		}
	}
}