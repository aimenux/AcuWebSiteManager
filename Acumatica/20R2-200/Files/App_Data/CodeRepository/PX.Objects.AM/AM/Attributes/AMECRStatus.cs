using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Bill of Material Revision Status
    /// </summary>
    public class AMECRStatus
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
        public const int Approved = 2;
        /// <summary>
        /// PendingApproval
        /// </summary>
        public const int PendingApproval = 3;
        /// <summary>
        /// Rejected
        /// </summary>
        public const int Rejected = 4;
        /// <summary>
        /// Completed
        /// </summary>
        public const int Completed = 5;

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Hold => Messages.GetLocal(Messages.Hold);
            public static string Active => Messages.GetLocal(Messages.Active);
            public static string Approved => Messages.GetLocal(PX.Objects.EP.Messages.Approved);
            public static string PendingApproval => Messages.GetLocal(PX.Objects.EP.Messages.PendingApproval);
            public static string Rejected => Messages.GetLocal(PX.Objects.EP.Messages.Rejected);
            public static string Completed => Messages.GetLocal(PX.Objects.EP.Messages.Completed);
        }

        public class hold : PX.Data.BQL.BqlInt.Constant<hold>
        {
            public hold() : base(Hold) { }
        }

        public class active : PX.Data.BQL.BqlInt.Constant<active>
        {
            public active() : base(Active) { }
        }

        public class approved : PX.Data.BQL.BqlInt.Constant<approved>
        {
            public approved() : base(Approved) { }
        }

        public class pendingApproval : PX.Data.BQL.BqlInt.Constant<pendingApproval>
        {
            public pendingApproval() : base(PendingApproval) { }
        }

        public class rejected : PX.Data.BQL.BqlInt.Constant<rejected>
        {
            public rejected() : base(Rejected) { }
        }
        public class completed : PX.Data.BQL.BqlInt.Constant<completed>
        {
            public completed() : base(Completed) { }
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { Hold, Active, Approved, PendingApproval, Rejected, Completed },
                new string[] { Messages.Hold, Messages.Active, EP.Messages.Approved, EP.Messages.PendingApproval, EP.Messages.Rejected, EP.Messages.Completed }) { }
        }
    }
}