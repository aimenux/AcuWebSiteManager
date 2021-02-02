using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Document Status
    /// </summary>
    public class DocStatus
    {
        public const string Balanced = BatchStatus.Balanced;
        public const string Hold = BatchStatus.Hold;
        public const string Released = BatchStatus.Released;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Balanced => Messages.GetLocal(Messages.Balanced);
            public static string Hold => Messages.GetLocal(Messages.Hold);
            public static string Released => Messages.GetLocal(Messages.Released);
        }

        //BQL constants declaration
        public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
        {
            public balanced() : base(Balanced) { ;}
        }
        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) { ;}
        }
        public class released : PX.Data.BQL.BqlString.Constant<released>
        {
            public released() : base(Released) { ;}
        }

        #pragma warning disable
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { 
                        Balanced, 
                        Hold, 
                        Released},
                    new string[] {
                        Messages.Balanced,
                        Messages.Hold,
                        Messages.Released}) { }
        }
    }
}