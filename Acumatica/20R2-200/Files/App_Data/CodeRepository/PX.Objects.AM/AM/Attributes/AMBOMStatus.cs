using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Bill of Material Revision Status
    /// </summary>
    public class AMBomStatus
    {
        /// <summary>
        /// Hold
        /// </summary>
        public const int Hold = 0;
        /// <summary>
        /// Active
        /// </summary>
        public const int Active = 1;
        /// <summary>
        /// Archived
        /// </summary>
        public const int Archived = 2;
        /// <summary>
        /// PendingApproval
        /// </summary>
        public const int PendingApproval = 3;
        /// <summary>
        /// Rejected
        /// </summary>
        public const int Rejected = 4;

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Hold => Messages.GetLocal(Messages.Hold);
            public static string Active => Messages.GetLocal(Messages.Active);
            public static string Archived => Messages.GetLocal(Messages.Archived);
            public static string PendingApproval => Messages.GetLocal(PX.Objects.EP.Messages.Balanced);
            public static string Rejected => Messages.GetLocal(PX.Objects.EP.Messages.Rejected);
        }

        /// <summary>
        /// Get the list label of the attribute value
        /// </summary>
        public static string GetListDescription(int? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            try
            {
                var x = new ListAttribute();
                return x.ValueLabelDic[value.GetValueOrDefault()];
            }
            catch
            {
                return Messages.Unknown;
            }
        }

        public class hold : PX.Data.BQL.BqlInt.Constant<hold>
        {
            public hold() : base(Hold) { }
        }

        public class active : PX.Data.BQL.BqlInt.Constant<active>
        {
            public active() : base(Active) { }
        }

        public class archived : PX.Data.BQL.BqlInt.Constant<archived>
        {
            public archived() : base(Archived) { }
        }

        public class pendingApproval : PX.Data.BQL.BqlInt.Constant<pendingApproval>
        {
            public pendingApproval() : base(PendingApproval) { }
        }

        public class rejected : PX.Data.BQL.BqlInt.Constant<rejected>
        {
            public rejected() : base(Rejected) { }
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { Hold, Active, Archived, PendingApproval, Rejected },
                new string[] { Messages.Hold, Messages.Active, Messages.Archived, PX.Objects.EP.Messages.Balanced, PX.Objects.EP.Messages.Rejected })
            { }
        }
    }
}