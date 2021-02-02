using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing extension methods on PX objects
    /// </summary>
    public static class PXExtensionMethods
    {
        /// <summary>
        /// Convert a PXResultset to a list using the primary DAC as the list object
        /// </summary>
        /// <returns>List of T</returns>
        public static List<T> ToFirstTableList<T>(this PXResultset<T> value)
            where T : class, IBqlTable, new()
        {
            return value?.FirstTableItems?.ToList();
        }

        public static IEnumerable<T> ToFirstTable<T>(this PXResultset<T> values)
            where T : class, IBqlTable, new()
        {
            foreach (T val in values)
            {
                yield return val;
            }
        }

        public static IEnumerable<PXResult<T1, T2>> ToEnumerable<T1, T2>(this PXResultset<T1> values)
            where T1 : class, IBqlTable, new()
            where T2 : class, IBqlTable, new()
        {
            foreach (PXResult<T1, T2> val in values)
            {
                yield return val;
            }
        }

        /// <summary>
        /// Locate the row in cache, else return the passed row
        /// (null safe)
        /// </summary>
        /// <typeparam name="T">DAC type</typeparam>
        /// <param name="graph">cache related to row</param>
        /// <param name="row">DAC row</param>
        /// <returns>found in cache row, else passed row</returns>
        public static T LocateElse<T>(this PXGraph graph, T row) where T : class, IBqlTable, new()
        {
            return LocateElse<T>(graph?.Caches<T>(), row);
        }

        /// <summary>
        /// Locate the row in cache, else return the passed row
        /// (null safe)
        /// </summary>
        /// <typeparam name="T">DAC type</typeparam>
        /// <param name="value">cache related to row</param>
        /// <param name="row">DAC row</param>
        /// <returns>found in cache row, else passed row</returns>
        public static T LocateElse<T>(this PXCache value, T row) where T : class, IBqlTable
        {
            return (row == null || value == null ? null : value.Locate(row) ?? row) as T;
        }

        /// <summary>
        /// LocateElse by multiple rows
        /// </summary>
        public static List<T> LocateElse<T>(this List<T> value, PXGraph graph)
            where T : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null
            //    ? null
            //    : value.Select(result => graph.Caches<T>().LocateElse((T)result)).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<T>();
            foreach (var result in value)
            {
                list.Add(graph.Caches<T>().LocateElse(result));
            }
            return list;
        }

        /// <summary>
        /// Locate the row in cache, else return the passed row and copy the object.
        /// (null safe)
        /// </summary>
        /// <typeparam name="T">DAC type</typeparam>
        /// <param name="value">cache related to row</param>
        /// <param name="row">DAC row</param>
        /// <returns>found in cache row, else passed row</returns>
        public static T LocateElseCopy<T>(this PXCache value, T row) where T : class, IBqlTable
        {
            return value.CreateCopy(value.LocateElse(row)) as T;
        }

        /// <summary>
        /// LocateElseCopy by multiple rows
        /// </summary>
        public static List<T> LocateElseCopy<T>(this List<T> value, PXGraph graph)
            where T : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null
            //    ? null
            //    : value.Select(result => graph.Caches<T>().LocateElseCopy((T)result)).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<T>();
            foreach (var result in value)
            {
                list.Add(graph.Caches<T>().LocateElseCopy(result));
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list while locating and copying the object(s) in cache
        /// </summary>
        public static List<T> ToLocatedFirstTableList<T>(this PXResultset<T> value, PXGraph graph)
            where T : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null
            //    ? null
            //    : value.Select(result => graph.Caches<T>().LocateElse((T)result)).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<T>();
            foreach (T result in value)
            {
                list.Add(graph.Caches<T>().LocateElse(result));
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults and locate each result in cache
        /// </summary>
        public static List<PXResult<T0>> ToLocatedList<T0>(this PXResultset<T0> value, PXGraph graph)
            where T0 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null 
            //    ? null 
            //    : value.Select(result => new PXResult<T0>(graph.Caches<T0>().LocateElse((T0) result))).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0>>();
            foreach (PXResult<T0> result in value)
            {
                list.Add(new PXResult<T0>(graph.Caches<T0>().LocateElse((T0)result)));
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults and locate each result in cache
        /// </summary>
        public static List<PXResult<T0, T1>> ToLocatedList<T0, T1>(this PXResultset<T0> value, PXGraph graph)
            where T0 : class, IBqlTable, new()
            where T1 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null 
            //    ? null 
            //    : (from PXResult<T0, T1> result in value
            //        select new PXResult<T0, T1>(graph.Caches<T0>().LocateElse((T0) result), 
            //            graph.Caches<T1>().LocateElse((T1) result))).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0, T1>>();
            foreach (PXResult<T0, T1> result in value)
            {
                list.Add(new PXResult<T0, T1>(graph.Caches<T0>().LocateElse((T0)result), graph.Caches<T1>().LocateElse((T1)result)));
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list while locating and copying the object(s) in cache
        /// </summary>
        public static List<T> ToLocatedCopiedFirstTableList<T>(this PXResultset<T> value, PXGraph graph)
            where T : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null
            //    ? null
            //    : value.Select(result => graph.Caches<T>().LocateElseCopy((T) result)).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<T>();
            foreach (T result in value)
            {
                list.Add(graph.Caches<T>().LocateElseCopy(result));
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults while locating and copying the object(s) in cache
        /// </summary>
        public static List<PXResult<T0>> ToLocatedCopiedList<T0>(this PXResultset<T0> value, PXGraph graph)
            where T0 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null 
            //    ? null 
            //    : value.Select(result => new PXResult<T0>(graph.Caches<T0>().LocateElseCopy((T0)result))).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0>>();
            foreach (PXResult<T0> result in value)
            {
                list.Add(new PXResult<T0>(graph.Caches<T0>().LocateElseCopy((T0)result)));
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults while locating and copying the object(s) in cache
        /// </summary>
        public static List<PXResult<T0, T1>> ToLocatedCopiedList<T0, T1>(this PXResultset<T0> value, PXGraph graph)
            where T0 : class, IBqlTable, new()
            where T1 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return graph == null || value == null
            //        ? null
            //        : (from PXResult<T0, T1> result in value
            //            select new PXResult<T0, T1>(graph.Caches<T0>().LocateElseCopy((T0) result),
            //                graph.Caches<T1>().LocateElseCopy((T1) result))).ToList();
            if (graph == null || value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0, T1>>();
            foreach (PXResult<T0, T1> result in value)
            {
                list.Add(new PXResult<T0,T1>(graph.Caches<T0>().LocateElseCopy((T0)result), graph.Caches<T1>().LocateElseCopy((T1)result)));
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults
        /// </summary>
        public static List<PXResult<T0>> ToList<T0>(this PXResultset<T0> value)
            where T0 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return value?.Cast<PXResult<T0>>().ToList();
            if (value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0>>();
            foreach (PXResult<T0> result in value)
            {
                list.Add(result);
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults
        /// </summary>
        public static System.Collections.Generic.List<PXResult<T0, T1>> ToList<T0, T1>(this PXResultset<T0> value)
            where T0 : class, IBqlTable, new()
            where T1 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return value?.Cast<PXResult<T0, T1>>().ToList();
            if (value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0, T1>>();
            foreach (PXResult<T0, T1> result in value)
            {
                list.Add(result);
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults
        /// </summary>
        public static List<PXResult<T0, T1, T2>> ToList<T0, T1, T2>(this PXResultset<T0> value)
            where T0 : class, IBqlTable, new()
            where T1 : class, IBqlTable, new()
            where T2 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return value?.Cast<PXResult<T0, T1, T2>>().ToList();
            if (value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0, T1, T2>>();
            foreach (PXResult<T0, T1, T2> result in value)
            {
                list.Add(result);
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults
        /// </summary>
        public static List<PXResult<T0, T1, T2, T3>> ToList<T0, T1, T2, T3>(this PXResultset<T0> value)
            where T0 : class, IBqlTable, new()
            where T1 : class, IBqlTable, new()
            where T2 : class, IBqlTable, new()
            where T3 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return value?.Cast<PXResult<T0, T1, T2, T3>>().ToList();
            if (value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0, T1, T2, T3>>();
            foreach (PXResult<T0, T1, T2, T3> result in value)
            {
                list.Add(result);
            }
            return list;
        }

        /// <summary>
        /// Convert ResultSet to a list of PXResults
        /// </summary>
        public static List<PXResult<T0, T1, T2, T3, T4>> ToList<T0, T1, T2, T3, T4>(this PXResultset<T0> value)
            where T0 : class, IBqlTable, new()
            where T1 : class, IBqlTable, new()
            where T2 : class, IBqlTable, new()
            where T3 : class, IBqlTable, new()
            where T4 : class, IBqlTable, new()
        {
            // 19.092.0039 causes issues using Linq - TFS bug #2441 
            //return value?.Cast<PXResult<T0, T1, T2, T3, T4>>().ToList();
            if (value == null)
            {
                return null;
            }

            var list = new List<PXResult<T0, T1, T2, T3, T4>>();
            foreach (PXResult<T0, T1, T2, T3, T4> result in value)
            {
                list.Add(result);
            }
            return list;
        }

        /// <summary>
        /// Does the given graph contain inserted rows of the given type
        /// </summary>
        /// <typeparam name="T">BQL Table to check for inserts</typeparam>
        /// <param name="graph">Graph containing Cache to check</param>
        /// <returns>True if inserted found</returns>
        public static bool ContainsInsertedCache<T>(this PXGraph graph) where T : class, IBqlTable, new()
        {
            return graph != null && graph.Caches<T>().Inserted.Any_();
        }

        /// <summary>
        /// Does the given graph contain updated rows of the given type
        /// </summary>
        /// <typeparam name="T">BQL Table to check for updates</typeparam>
        /// <param name="graph">Graph containing Cache to check</param>
        /// <returns>True if updated found</returns>
        public static bool ContainsUpdatedCache<T>(this PXGraph graph) where T : class, IBqlTable, new()
        {
            return graph != null && graph.Caches<T>().Updated.Any_();
        }

        /// <summary>
        /// Get a given row's IsKey field values as a single string.
        /// (No key field names included in return)
        /// </summary>
        /// <typeparam name="T">IBqlTable</typeparam>
        /// <param name="row">row data for key values</param>
        /// <param name="graph">graph with containing cache</param>
        /// <returns>key values separated by ":" (Ex: "value1:value2:value3")</returns>
        public static string GetRowKeyValues<T>(this T row, PXGraph graph) where T : class, IBqlTable, new()
        {
            return row != null && graph != null ? string.Join(":", new EntityHelper(graph).GetEntityRowKeys(typeof(T), row)) : null;
        }

        /// <summary>
        /// Get a given row's IsKey field values as a single string.
        /// (No key field names included in return)
        /// </summary>
        public static string GetRowKeyValues(this PXGraph graph, object row, Type rowType)
        {
            return row != null && graph != null ? string.Join(":", new EntityHelper(graph).GetEntityRowKeys(rowType, row)) : null;
        }

        /// <summary>
        /// Is the current cache row marked as deleted
        /// </summary>
        public static bool IsCurrentRowDeleted(this PXView view)
        {
            return view != null && IsCurrentRowDeleted(view.Cache);
        }

        /// <summary>
        /// Is the current cache row marked as deleted
        /// </summary>
        public static bool IsCurrentRowDeleted(this PXCache cache)
        {
            return IsRowDeleted(cache, cache?.Current);
        }

        /// <summary>
        /// Is the given row marked as deleted
        /// </summary>
        public static bool IsRowDeleted(this PXCache cache, object row)
        {
            return row != null && cache != null && cache.GetStatus(row) == PXEntryStatus.Deleted;
        }

        /// <summary>
        /// Is the current cache row a new inserted row
        /// </summary>
        public static bool IsCurrentRowInserted(this PXView view)
        {
            return view != null && IsCurrentRowInserted(view.Cache);
        }

        /// <summary>
        /// Is the current cache row a new inserted row
        /// </summary>
        public static bool IsCurrentRowInserted(this PXCache cache)
        {
            return IsRowInserted(cache, cache?.Current);
        }

        /// <summary>
        /// Is the given row a new inserted row
        /// </summary>
        public static bool IsRowInserted(this PXCache cache, object row)
        {
            return row != null && cache != null && cache.GetStatus(row) == PXEntryStatus.Inserted;
        }

        public static bool IsCurrentRowInsertedOrUpdated(this PXView view)
        {
            return view != null && IsCurrentRowInsertedOrUpdated(view.Cache);
        }

        public static bool IsCurrentRowInsertedOrUpdated(this PXCache cache)
        {
            return IsRowInsertedOrUpdated(cache, cache?.Current);
        }

        public static bool IsRowInsertedOrUpdated(this PXCache cache, object row)
        {
            if (row == null || cache == null)
            {
                return false;
            }

            var status = cache.GetStatus(row);

            return status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated;
        }

        /// <summary>
        /// Get a cache value as decimal. 
        /// (make sure you know its a decimal field - no checks for exceptions)
        /// </summary>
        public static decimal GetValueAsDecimal<Field>(this PXCache cache, object currentRow)
            where Field : IBqlField
        {
            return Convert.ToDecimal(cache.GetValue(currentRow, typeof(Field).Name.ToLower()));
        }

        /// <summary>returns the value from instance copy stored in database for Tstamp field</summary>
        public static byte[] GetOriginalTstampValue(this PXSelectBase selectBase, object row)
        {
            return row == null || selectBase == null ? null : GetOriginalTstampValue(selectBase.Cache, row);
        }

        /// <summary>returns the value from instance copy stored in database for Tstamp field</summary>
        public static byte[] GetOriginalTstampValue(this PXCache cache, object row)
        {
            return row == null || cache == null ? null : (byte[])cache.GetValueOriginal(row, "Tstamp");
        }

        /// <summary>
        /// Check the operations are equal
        /// </summary>
        public static bool OperationEqualTo(this IOperation oper1, IOperation oper2)
        {
            return oper1?.OperationID != null && oper2?.OperationID != null && oper1.OperationID == oper2.OperationID;
        }

        /// <summary>
        /// Check the operations are less than or equal to one another
        /// </summary>
        public static bool OperationLessThanOrEqualTo(this IOperation oper1, IOperation oper2, IEnumerable<IOperationMaster> operations)
        {
            return OperationEqualTo(oper1, oper2) || OperationLessThan(oper1, oper2, operations);
        }

        /// <summary>
        /// Check the operations are less than one another
        /// </summary>
        public static bool OperationLessThan(this IOperation oper1, IOperation oper2, IEnumerable<IOperationMaster> operations)
        {
            if (oper1?.OperationID == null || oper2?.OperationID == null || operations == null)
            {
                return false;
            }

            var operationMasters = operations as IOperationMaster[] ?? operations.ToArray();

            if (!TryGetOperationMaster(oper1, operationMasters, out var oper1Master))
            {
                return false;
            }

            if (!TryGetOperationMaster(oper2, operationMasters, out var oper2Master))
            {
                return false;
            }

            return oper1Master.OperationLessThan(oper2Master);
        }

        /// <summary>
        /// Check the operations are less than one another
        /// </summary>
        public static bool OperationGreaterThan(this IOperation oper1, IOperation oper2, IEnumerable<IOperationMaster> operations)
        {
            if (oper1?.OperationID == null || oper2?.OperationID == null || operations == null)
            {
                return false;
            }

            var operationMasters = operations as IOperationMaster[] ?? operations.ToArray();

            if (!TryGetOperationMaster(oper1, operationMasters, out var oper1Master))
            {
                return false;
            }

            if (!TryGetOperationMaster(oper2, operationMasters, out var oper2Master))
            {
                return false;
            }

            return oper1Master.OperationGreaterThan(oper2Master);
        }

        private static bool TryGetOperationMaster(IOperation oper, IOperationMaster[] operations, out IOperationMaster operationMaster)
        {
            operationMaster = null;
            if (oper?.OperationID == null || operations == null)
            {
                return false;
            }

            operationMaster = operations.FirstOrDefault(x => x.OperationID == oper?.OperationID);
            return !string.IsNullOrWhiteSpace(operationMaster?.OperationCD);
        }

        /// <summary>
        /// Check the operations are less than one another
        /// </summary>
        public static bool OperationLessThan(this IOperationMaster oper1, IOperationMaster oper2)
        {
            var compare = OperationCompare(oper1, oper2);
            return compare != null && compare < 0;
        }

        /// <summary>
        /// Check the operations are greater than one another
        /// </summary>
        public static bool OperationGreaterThan(this IOperationMaster oper1, IOperationMaster oper2)
        {
            var compare = OperationCompare(oper1, oper2);
            return compare != null && compare > 0;
        }

        /// <summary>
        /// Compare operation CD strings
        /// </summary>
        public static int? OperationCompare(this IOperationMaster oper1, IOperationMaster oper2)
        {
            return string.IsNullOrWhiteSpace(oper1?.OperationCD) || string.IsNullOrWhiteSpace(oper2?.OperationCD) ? (int?)null : string.CompareOrdinal(oper1.OperationCD, oper2.OperationCD);
        }

        /// <summary>
        /// Create a cache copy of <see cref="T"/>
        /// Equivalent to:  (MyDac) PXCache<MyDac>.CreateCopy(row)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T CreateCopy<T>(this T row) where T : class, IBqlTable, new()
        {
            return row == null ? null : PXCache<T>.CreateCopy(row);
        }
    }
}