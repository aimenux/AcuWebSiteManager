using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM
{
    public static class ObjCompareHelper
    {
        /// <summary>
        /// BOM MATERIAL
        /// </summary>

        public static bool ChangesExist(PXCache cache, AMBomMatl newRow, AMBomMatl oldRow, params string[] addlExcludedFields)
        {
            if (newRow == null || oldRow == null)
            {
                return false;
            }
            var list = new List<string>(addlExcludedFields)
            {
                "LineID",
                "RowStatus",
                "SortOrder"
            };

            return !RowsEqual(cache, newRow, oldRow, ComparePropertyAction.Exclude, list.ToArray());
        }

        /// <summary>
        /// BOM STEPS
        /// </summary>

        public static bool ChangesExist(PXCache cache, AMBomStep newRow, AMBomStep oldRow, params string[] addlExcludedFields)
        {
            if (newRow == null || oldRow == null)
            {
                return false;
            }
            var list = new List<string>(addlExcludedFields)
            {
                "LineID",
                "RowStatus"
            };

            return !RowsEqual(cache, newRow, oldRow, ComparePropertyAction.Exclude, list.ToArray());
        }

        /// <summary>
        /// BOM TOOLS
        /// </summary>

        public static bool ChangesExist(PXCache cache, AMBomTool newRow, AMBomTool oldRow, params string[] addlExcludedFields)
        {
            if (newRow == null || oldRow == null)
            {
                return false;
            }
            var list = new List<string>(addlExcludedFields)
            {
                "LineID",
                "RowStatus"
            };

            return !RowsEqual(cache, newRow, oldRow, ComparePropertyAction.Exclude, list.ToArray());
        }

        /// <summary>
        /// BOM OVERHEAD
        /// </summary

        public static bool ChangesExist(PXCache cache, AMBomOvhd newRow, AMBomOvhd oldRow, params string[] addlExcludedFields)
        {
            if (newRow == null || oldRow == null)
            {
                return false;
            }
            var list = new List<string>(addlExcludedFields)
            {
                "LineID",
                "RowStatus"
            };

            return !RowsEqual(cache, newRow, oldRow, ComparePropertyAction.Exclude, list.ToArray());
        }

        public static bool ChangesExist(PXCache cache, AMBomOper newRow, AMBomOper oldRow, params string[] addlExcludedFields)
        {
            if (newRow == null || oldRow == null)
            {
                return false;
            }
            var list = new List<string>(addlExcludedFields)
            {
                "RowStatus",
                "LineCntrStep",
                "LineCntrMatl",
                "LineCntrOvhd",
                "LineCntrTool"
            };

            return !RowsEqual(cache, newRow, oldRow, ComparePropertyAction.Exclude, list.ToArray());
        }

        public enum ComparePropertyAction
        {
            Include,
            Exclude
        }

        public static IEnumerable<string> GetFields(this PXCache cache, params string[] excludeFields)
        {
            foreach (var field in cache.Fields)
            {
                if (field.Contains("_") || IsSystemFieldName(field) || excludeFields?.Contains(field) == true)
                {
                    continue;
                }

                yield return field;
            }
        }

        /// <summary>
        /// When comparing rows these fields are always excluded from the compare for all bom tables
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> NonSystemFieldNames(this PXCache cache)
        {
            foreach (var field in cache.Fields)
            {
                if (field.Contains("_") || IsSystemFieldName(field))
                {
                    continue;
                }

                yield return field;
            }
        }

        private static bool IsSystemFieldName(string fieldName)
        {
            return fieldName.Equals("CreatedDateTime", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("CreatedByScreenID", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("CreatedByID", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("LastModifiedDateTime", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("LastModifiedByScreenID", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("LastModifiedByID", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("NoteID", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("Selected", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Equals("tstamp", StringComparison.OrdinalIgnoreCase);
        }

        public static bool RowsEqual<Dac>(this PXCache cache, Dac row1, Dac row2)
            where Dac : IBqlTable
        {
            return RowsEqual<Dac>(cache, row1, row2, ComparePropertyAction.Include, NonSystemFieldNames(cache).ToArray());
        }

        public static bool RowsEqual<Dac>(this PXCache cache, Dac row1, Dac row2, ComparePropertyAction action, string[] fields)
            where Dac : IBqlTable
        {
            if (cache == null || row1 == null || row2 == null)
            {
                return false;
            }

            if (cache.GetItemType() != typeof(Dac))
            {
                throw new ArgumentException(nameof(Dac));
            }

            if (fields == null || fields.Length == 0)
            {
                return cache.ObjectsEqual(row1, row2);
            }
            if (action == ComparePropertyAction.Exclude)
            {
                var fieldList = NonSystemFieldNames(cache).ToList();
                fieldList.RemoveAll(x => fields.Contains(x));
                fields = fieldList.ToArray();
            }

            foreach (var fieldName in fields)
            {
                if (!FieldsEqual(cache, row1, row2, fieldName))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool FieldsEqual<Dac>(this PXCache cache, Dac row1, Dac row2, string fieldName)
            where Dac : IBqlTable
        {
            return Equals(cache.GetValue(row1, fieldName), cache.GetValue(row2, fieldName));
        }

        public static void ThreeWayMerge<T>(this PXCache cache, T baseRow, T toRow, T fromRow, params string[] excludeFields)
            where T : IBqlTable
        {
            foreach (var fieldName in GetFields(cache, excludeFields))
            {
                var newValue = cache.GetValue(fromRow, fieldName);
                if (!Equals(newValue, cache.GetValue(baseRow, fieldName)))
                {
                    cache.SetValue(toRow, fieldName, newValue);
                }
            }
        }

        public static void TwoWayMerge<T>(this PXCache cache, T toRow, T fromRow, params string[] excludeFields)
            where T : IBqlTable
        {
            foreach (var fieldName in GetFields(cache, excludeFields))
            {
                cache.SetValue(toRow, fieldName, cache.GetValue(fromRow, fieldName));
            }
        }
    }
}
