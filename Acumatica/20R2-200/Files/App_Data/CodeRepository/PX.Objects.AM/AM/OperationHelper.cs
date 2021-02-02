using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public static class OperationHelper
    {
        /// <summary>
        /// Get the operation record before the given operation CD value
        /// </summary>
        /// <param name="operations">Set of operations from the same order</param>
        /// <param name="operationCD">Operation CD value to check</param>
        /// <returns></returns>
        public static TOper GetOperationBefore<TOper>(IEnumerable<TOper> operations, string operationCD)
            where TOper : IOperationMaster
        {
            return GetOperationBefore<TOper>(operations, operationCD, null);
        }

        /// <summary>
        /// Get the operation record before the given operation CD value
        /// </summary>
        /// <param name="operations">Set of operations from the same order</param>
        /// <param name="operationCD">Operation CD value to check</param>
        /// <param name="excludeOperationCD">(optional) excluding operation CD in case checking for a change in operation order and the changing operation exists in the collection of operations</param>
        /// <returns></returns>
        public static TOper GetOperationBefore<TOper>(IEnumerable<TOper> operations, string operationCD, string excludeOperationCD)
            where TOper : IOperationMaster
        {
            if (string.IsNullOrWhiteSpace(operationCD))
            {
                throw new ArgumentNullException(nameof(operationCD));
            }

            if (string.IsNullOrWhiteSpace(excludeOperationCD))
            {
                excludeOperationCD = string.Empty;
            }

            return operations.Where(x => x.OperationCD != null && x.OperationCD != excludeOperationCD && LessThan(x.OperationCD, operationCD))
                .OrderByDescending(x => x.OperationCD)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get the operation record after the given operation CD value
        /// </summary>
        /// <param name="operations">Set of operations from the same order</param>
        /// <param name="operationCD">Operation CD value to check</param>
        /// <returns></returns>
        public static TOper GetOperationAfter<TOper>(IEnumerable<TOper> operations, string operationCD)
            where TOper : IOperationMaster
        {
            return GetOperationAfter<TOper>(operations, operationCD, null);
        }

        /// <summary>
        /// Get the operation record after the given operation CD value
        /// </summary>
        /// <param name="operations">Set of operations from the same order</param>
        /// <param name="operationCD">Operation CD value to check</param>
        /// <param name="excludeOperationCD">(optional) excluding operation CD in case checking for a change in operation order and the changing operation exists in the collection of operations</param>
        /// <returns></returns>
        public static TOper GetOperationAfter<TOper>(IEnumerable<TOper> operations, string operationCD, string excludeOperationCD)
            where TOper : IOperationMaster
        {
            if (string.IsNullOrWhiteSpace(operationCD))
            {
                throw new ArgumentNullException(nameof(operationCD));
            }

            if (string.IsNullOrWhiteSpace(excludeOperationCD))
            {
                excludeOperationCD = string.Empty;
            }

            return operations.Where(x => x.OperationCD != null && x.OperationCD != excludeOperationCD && GreaterThan(x.OperationCD, operationCD))
                .OrderBy(x => x.OperationCD)
                .FirstOrDefault();
        }

        public static bool GreaterThan(string cd1, string cd2)
        {
            return string.CompareOrdinal(cd1, cd2) > 0;
        }

        public static bool LessThan(string cd1, string cd2)
        {
            return string.CompareOrdinal(cd1, cd2) < 0;
        }

        public static bool EqualTo(string cd1, string cd2)
        {
            return string.CompareOrdinal(cd1, cd2) == 0;
        }

        public static int ToCalculatedOperationID(string operationCD)
        {
            return !string.IsNullOrWhiteSpace(operationCD) && int.TryParse(FullCharacterOperationCD(operationCD), out var operationCdInt) ? operationCdInt : 0;
        }

        public static string FullCharacterOperationCD(string operationCD)
        {
            return operationCD?.PadRight(OperationCDFieldAttribute.OperationFieldLength, '0');
        }

        public static IEnumerable<TOperation> OrderOperations<TOperation>(this IEnumerable<TOperation> operations)
            where TOperation : IOperationMaster
        {
            return operations?.OrderBy(x => x.OperationCD).ThenBy(x => x.OperationID);
        }

        public static IEnumerable<TOperation> OrderOperationsDescending<TOperation>(this IEnumerable<TOperation> operations)
            where TOperation : IOperationMaster
        {
            return operations?.OrderByDescending(x => x.OperationCD).ThenByDescending(x => x.OperationID);
        }
    }
}
