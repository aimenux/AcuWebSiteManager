using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Configuration Revision Statuses
    /// </summary>
    public class ConfigRevisionStatus
    {
        /// <summary>
        /// Active revision ("A")
        /// </summary>
        public const string Active = "A";
        /// <summary>
        /// Inactive revision ("I")
        /// </summary>
        public const string Inactive = "I";
        /// <summary>
        /// Pending revision ("P")
        /// </summary>
        public const string Pending = "P";

        public static class Desc
        {
            public static string Active => Messages.GetLocal(Messages.Active);
            public static string Inactive => Messages.GetLocal(Messages.Inactive);
            public static string Pending => Messages.GetLocal(Messages.Pending);
        }

        #region METHODS

        public static string GetDescription(string id)
        {
            switch (id)
            {
                case Active:
                    return Desc.Active;
                case Inactive:
                    return Desc.Inactive;
                case Pending:
                    return Desc.Pending;
            }

            return Messages.GetLocal(Messages.Unknown);
        }

        #endregion

        //BQL constants declaration
        public class active : PX.Data.BQL.BqlString.Constant<active>
        {
            public active() : base(Active) { }
        }
        public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
        {
            public inactive() : base(Inactive) { }
        }
        public class pending : PX.Data.BQL.BqlString.Constant<pending>
        {
            public pending() : base(Pending) { }
        }
        
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { Active, Inactive, Pending },
                    new string[] { Messages.Active, Messages.Inactive, Messages.Pending }) { }
        }
    }
}