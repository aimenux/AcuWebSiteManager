using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Defines order hold status related to order,setups, and item status
    /// </summary>
    public class OnHoldStatus
    {
        /// <summary>
        /// Order is not on hold
        /// </summary>
        public const int NotOnHold = 0;

        /// <summary>
        /// Order is on hold but included in the MRP Planning calculations
        /// </summary>
        public const int OnHoldInclude = 1;

        /// <summary>
        /// Order is on hold and excluded from the MRP Planning calculations
        /// </summary>
        public const int OnHoldExclude = 2;

        /// <summary>
        /// Item status is invalid to support the order in MRP Planning calculations
        /// </summary>
        public const int InvalidItemStatus = 3;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string NotOnHold => Messages.GetLocal(Messages.OnHoldStatusNotOnHold);
            public static string OnHoldInclude => Messages.GetLocal(Messages.OnHoldStatusOnHoldInclude);
            public static string OnHoldExclude => Messages.GetLocal(Messages.OnHoldStatusOnHoldExclude);
            public static string InvalidItemStatus => Messages.GetLocal(Messages.OnHoldStatusInvalidItemStatus);
        }

        public class notOnHold : PX.Data.BQL.BqlInt.Constant<notOnHold>
        {
            public notOnHold() : base(NotOnHold) { }
        }
        public class onHoldInclude : PX.Data.BQL.BqlInt.Constant<onHoldInclude>
        {
            public onHoldInclude() : base(OnHoldInclude) { }
        }
        public class onHoldExclude : PX.Data.BQL.BqlInt.Constant<onHoldExclude>
        {
            public onHoldExclude() : base(OnHoldExclude) { }
        }
        public class invalidItemStatus : PX.Data.BQL.BqlInt.Constant<invalidItemStatus>
        {
            public invalidItemStatus() : base(InvalidItemStatus) { }
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                    new int[] { 
                        NotOnHold, 
                        OnHoldInclude, 
                        OnHoldExclude, 
                        InvalidItemStatus},
                    new string[] {
                        Messages.OnHoldStatusNotOnHold,
                        Messages.OnHoldStatusOnHoldInclude,
                        Messages.OnHoldStatusOnHoldExclude,
                        Messages.OnHoldStatusInvalidItemStatus }) { }
        }
    }
}