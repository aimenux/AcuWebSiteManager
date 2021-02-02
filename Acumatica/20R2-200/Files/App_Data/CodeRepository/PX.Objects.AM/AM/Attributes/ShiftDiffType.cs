using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Shift differential types
    /// </summary>
    public class ShiftDiffType
    {
        /// <summary>
        /// Amount = A
        ///     Differential is a fixed amount per hour
        /// </summary>
        public const string Amount = "A";
        /// <summary>
        /// Rate = R
        ///     Differential is a rate per hour
        /// </summary>
        public const string Rate = "R";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Amount => Messages.GetLocal(Messages.AmountDiff);
            public static string Rate => Messages.GetLocal(Messages.RateDiff);
        }


        public class amount : PX.Data.BQL.BqlString.Constant<amount>
        {
            public amount() : base(Amount) { ;}
        }
        public class rate : PX.Data.BQL.BqlString.Constant<rate>
        {
            public rate() : base(Rate) { ;}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                        new string[] { Amount, Rate },
                        new string[] { Messages.AmountDiff, Messages.RateDiff }) { ; }
        }

        /// <summary>
        /// Get the shifts differential cost for the given work center
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="workCenterID">work center ID</param>
        /// <returns>Calculated total differential cost</returns>
        public static decimal GetShiftDifferentialCost(PXGraph graph, string workCenterID)
        {
            if (graph == null || string.IsNullOrWhiteSpace(workCenterID))
            {
                return 0;
            }

            AMWC workCenter = PXSelect<AMWC, Where<AMWC.wcID, Equal<Required<AMWC.wcID>>>>.Select(graph, workCenterID);

            if (workCenter == null)
            {
                return 0;
            }

            return GetShiftDifferentialCost(graph, workCenter);
        }

        /// <summary>
        /// Get the shifts differential cost for the given work center
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="workCenter">Work center row</param>
        /// <returns>Calculated total differential cost</returns>
        public static decimal GetShiftDifferentialCost(PXGraph graph, AMWC workCenter)
        {
            if (graph == null 
                || workCenter == null
                || string.IsNullOrWhiteSpace(workCenter.WcID))
            {
                return 0;
            }

            AMShiftMst amShiftMst = PXSelectJoin<AMShiftMst,
                                InnerJoin<AMShift, On<AMShiftMst.shiftID, Equal<AMShift.shiftID>>>,
                                Where<AMShift.wcID, Equal<Required<AMShift.wcID>>>,
                                    OrderBy<Asc<AMShift.shiftID>>>.SelectWindowed(graph, 0, 1, workCenter.WcID);
            
            return GetShiftDifferentialCost(workCenter, amShiftMst);
        }

        /// <summary>
        /// Get the shifts differential cost for the given work center
        /// </summary>
        /// <param name="workCenter">Work center row</param>
        /// <param name="wcShift">work center shift row</param>
        /// <returns>Calculated total differential cost</returns>
        public static decimal GetShiftDifferentialCost(AMWC workCenter, AMShiftMst shiftMst)
        {
            if (string.IsNullOrWhiteSpace(workCenter?.WcID)
                || string.IsNullOrWhiteSpace(shiftMst?.ShiftID))
            {
                return 0;
            }

            return GetShiftDifferentialCost(workCenter.StdCost, shiftMst.ShftDiff, shiftMst.DiffType);
        }

        /// <summary>
        /// Get the shifts differential cost
        /// </summary>
        /// <param name="laborCost">Work center/Employee standard labor cost</param>
        /// <param name="shiftDifferentialCost">Shift differential cost</param>
        /// <param name="wcShiftDiffType">Differential type</param>
        /// <returns>Calculated total differential cost</returns>
        public static decimal GetShiftDifferentialCost(decimal? laborCost, decimal? shiftDifferentialCost, string wcShiftDiffType)
        {
            if (string.IsNullOrWhiteSpace(wcShiftDiffType))
            {
                return 0;
            }

            if (wcShiftDiffType.EqualsWithTrim(Amount))
            {
                return laborCost.GetValueOrDefault() + shiftDifferentialCost.GetValueOrDefault();
            }

            return laborCost.GetValueOrDefault() * shiftDifferentialCost.GetValueOrDefault();

        }
    }
}
