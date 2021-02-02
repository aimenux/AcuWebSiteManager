using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class EstimateStatus
    {
        public const int NewStatus = 1;
        public const int Completed = 2;
        public const int Canceled = 3;
        public const int InProcess = 4;
        public const int PendingApproval = 5;
        public const int Approved = 6;
        public const int Sent = 7;
        public const int Closed = 8;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string NewStatus => Messages.GetLocal(Messages.NewStatus);
            public static string Completed => Messages.GetLocal(Messages.Completed);
            public static string Canceled => Messages.GetLocal(Messages.Canceled);
            public static string InProcess => Messages.GetLocal(Messages.InProcess);
            public static string PendingApproval => Messages.GetLocal(PX.Objects.AP.Messages.PendingApproval);
            public static string Approved => Messages.GetLocal(PX.Objects.AP.Messages.Approved);
            public static string Sent => Messages.GetLocal(PX.Objects.CR.Messages.Sent);
            public static string Closed => Messages.GetLocal(Messages.Closed);
        }

        public static string GetDescription(int? id)
        {
            if (id == null)
            {
                return string.Empty;
            }

            try
            {
                var x = new ListAttribute();
                return x.ValueLabelDic[id.GetValueOrDefault()];
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsEditable(int? id)
        {
            return id != null && (id == NewStatus || id == InProcess);
        }

        public static bool IsFinished(int? id)
        {
            return id != null && (id == Canceled || id == Closed);
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                    new[] { 
                        NewStatus,
                        InProcess,
                        PendingApproval,
                        Approved,
                        Sent,
                        Completed,
                        Closed,
                        Canceled},
                    new[] { 
                        Messages.NewStatus,
                        Messages.InProcess,
                        AP.Messages.PendingApproval,
                        AP.Messages.Approved,
                        CR.Messages.Sent,
                        Messages.Completed,
                        Messages.Closed,
                        Messages.Canceled
                    })
            {
            }
        }

        public class newStatus : PX.Data.BQL.BqlInt.Constant<newStatus>
        {
            public newStatus() : base(NewStatus) {; }
        }

        public class completed : PX.Data.BQL.BqlInt.Constant<completed>
        {
            public completed() : base(Completed) {; }
        }

        public class canceled : PX.Data.BQL.BqlInt.Constant<canceled>
        {
            public canceled() : base(Canceled) {; }
        }

        public class inProcess : PX.Data.BQL.BqlInt.Constant<inProcess>
        {
            public inProcess() : base(InProcess) {; }
        }

        public class pendingApproval : PX.Data.BQL.BqlInt.Constant<pendingApproval>
        {
            public pendingApproval() : base(PendingApproval) {; }
        }

        public class approved : PX.Data.BQL.BqlInt.Constant<approved>
        {
            public approved() : base(Approved) {; }
        }

        public class sent : PX.Data.BQL.BqlInt.Constant<sent>
        {
            public sent() : base(Sent) {; }
        }

        public class closed : PX.Data.BQL.BqlInt.Constant<closed>
        {
            public closed() : base(Closed) {; }
        }
    }
}