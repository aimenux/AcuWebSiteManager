using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Labor rate types to define the location containing the labor rate used for production labor transactions
    /// </summary>
    public class LaborRateType
    {
        /// <summary>
        /// Employee labor rate - hourly rate of the employee
        /// </summary>
        public const string Employee = "E";
        /// <summary>
        /// Standard labor rate - rate used from a Work Center
        /// </summary>
        public const string Standard = "S";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string Employee => Messages.GetLocal(Messages.Employee);
            public static string Standard => Messages.GetLocal(Messages.Standard);
        }

        /// <summary>
        /// Employee labor rate - hourly rate of the employee
        /// </summary>
        public class employee : PX.Data.BQL.BqlString.Constant<employee>
        {
            public employee() : base(Employee) { }
        }
        /// <summary>
        /// Standard labor rate - rate used from a Work Center
        /// </summary>
        public class standard : PX.Data.BQL.BqlString.Constant<standard>
        {
            public standard() : base(Standard) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { Employee, Standard },
                    new string[] { Messages.Employee, Messages.Standard }) { }
        }
    }
}