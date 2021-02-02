using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Production order statuses
    /// </summary>
    public class ProductionOrderStatus
    {
        /// <summary>
        /// Planned = P (New order default)
        /// </summary>
        public const string Planned = "P";
        /// <summary>
        /// Released = R
        /// </summary>
        public const string Released = "R";
        /// <summary>
        /// InProcess = I
        /// </summary>
        public const string InProcess = "I";
        /// <summary>
        /// Hold = H
        /// Status is not set in DB when order is placed on hold. Used for inquiry displays
        /// </summary>
        public const string Hold = "H";
        /// <summary>
        /// Cancel = X
        /// </summary>
        public const string Cancel = "X";
        /// <summary>
        /// Completed = M
        /// </summary>
        public const string Completed = "M";
        /// <summary>
        /// Closed = C
        /// </summary>
        public const string Closed = "C";
        /// <summary>
        /// Deleted = D
        /// Status is not set in DB when order is deleted. Used for inquiry displays where reference production order
        /// has been deleted.
        /// </summary>
        public const string Deleted = "D";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string Planned => Messages.GetLocal(Messages.Planned);
            public static string Released => Messages.GetLocal(Messages.Released);
            public static string InProcess => Messages.GetLocal(Messages.InProcess);
            public static string Hold => Messages.GetLocal(Messages.Hold);
            public static string Cancel => Messages.GetLocal(Messages.Canceled);
            public static string Completed => Messages.GetLocal(Messages.Completed);
            public static string Closed => Messages.GetLocal(Messages.Closed);
            public static string Deleted => Messages.GetLocal(Messages.Deleted);
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { 
                        Planned, 
                        Released,
                        InProcess, 
                        Cancel, 
                        Completed, 
                        Closed},
                    new string[] {
                        Messages.Planned, 
                        Messages.Released, 
                        Messages.InProcess,
                        Messages.Canceled,
                        Messages.Completed,
                        Messages.Closed}) {}
        }

        public class ListAllAttribute : PXStringListAttribute
        {
            public ListAllAttribute()
                : base(
                    new string[] { 
                        Planned, 
                        Released, 
                        InProcess, 
                        Hold, 
                        Cancel, 
                        Completed, 
                        Closed, 
                        Deleted
                    },
                    new string[] {
                        Messages.Planned, 
                        Messages.Released, 
                        Messages.InProcess, 
                        Messages.Hold, 
                        Messages.Canceled, 
                        Messages.Completed, 
                        Messages.Closed, 
                        Messages.Deleted
                    })
            {}
        }

        #region METHODS

        /// <summary>
        /// Returns the production status description/label from the status single letter ID
        /// </summary>
        /// <param name="statusID">single letter status ID</param>
        /// <returns>Production description/label</returns>
        public static string GetStatusDescription(string statusID)
        {
            switch (statusID)
            {
                case Planned:
                    return Desc.Planned;
                case Released:
                    return Desc.Released;
                case InProcess:
                    return Desc.InProcess;
                case Hold:
                    return Desc.Hold;
                case Cancel:
                    return Desc.Cancel;
                case Completed:
                    return Desc.Completed;
                case Closed:
                    return Desc.Closed;
                case Deleted:
                    return Desc.Deleted;
            }

            return Messages.GetLocal(Messages.Unknown);
        }

        /// <summary>
        /// Can the given production order status be set to on hold
        /// </summary>
        /// <param name="statusID">Production status ID</param>
        /// <returns>true if can be on hold</returns>
        public static bool CanHold(string statusID)
        {
            //OK TO HOLD IF CURRENT STATUS IS: Plan,Released,In Process
            switch (statusID.Trim().ToUpper())
            {
                case Planned:
                    return true;
                case Released:
                    return true;
                case InProcess:
                    return true;
            }

            return false;
        }

        #endregion
        
        public class planned : PX.Data.BQL.BqlString.Constant<planned>
        {
            public planned() : base(Planned) { ;}
        }
        public class released : PX.Data.BQL.BqlString.Constant<released>
        {
            public released() : base(Released) { ;}
        }
        public class inProcess : PX.Data.BQL.BqlString.Constant<inProcess>
        {
            public inProcess() : base(InProcess) { ;}
        }
        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) { ;}
        }
        public class cancel : PX.Data.BQL.BqlString.Constant<cancel>
        {
            public cancel() : base(Cancel) { ;}
        }
        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(Completed) { ;}
        }
        public class closed : PX.Data.BQL.BqlString.Constant<closed>
        {
            public closed() : base(Closed) { ;}
        }
        public class deleted : PX.Data.BQL.BqlString.Constant<deleted>
        {
            public deleted() : base(Deleted) { ;}
        }
    }
}