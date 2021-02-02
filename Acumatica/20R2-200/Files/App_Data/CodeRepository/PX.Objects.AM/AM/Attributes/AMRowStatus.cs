using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Row Status Attribute indicating changes
    /// </summary>
    public class AMRowStatus
    {
        /// <summary>
        /// Unchanged = 0
        /// </summary>
        public const int Unchanged = 0;
        /// <summary>
        /// Updated = 1
        /// </summary>
        public const int Updated = 1;
        /// <summary>
        /// Inserted = 2
        /// </summary>
        public const int Inserted = 2;
        /// <summary>
        /// Deleted = 3
        /// </summary>
        public const int Deleted = 3;

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Unchanged => Messages.GetLocal(Messages.Unchanged);
            public static string Updated => Messages.GetLocal(Messages.Updated);
            public static string Inserted => Messages.GetLocal(Messages.Inserted);
            public static string Deleted => Messages.GetLocal(Messages.Deleted);

        }

        public class unchanged : PX.Data.BQL.BqlInt.Constant<unchanged>
        {
            public unchanged() : base(Unchanged) { }
        }

        public class updated : PX.Data.BQL.BqlInt.Constant<updated>
        {
            public updated() : base(Updated) { }
        }

        public class inserted : PX.Data.BQL.BqlInt.Constant<inserted>
        {
            public inserted() : base(Inserted) { }
        }

        public class deleted : PX.Data.BQL.BqlInt.Constant<deleted>
        {
            public deleted() : base(Deleted) { }
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { Unchanged, Updated, Inserted, Deleted },
                new string[] { Messages.Unchanged, Messages.Updated, Messages.Inserted, Messages.Deleted })
            { }
        }
    }
}