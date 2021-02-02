using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Labor Types
    /// </summary>
    public class AMLaborType
    {
        /// <summary>
        /// Manufacturing Direct Labor
        /// </summary>
        public const string Direct = "D";
        /// <summary>
        /// Manufacturing Indirect Labor
        /// </summary>
        public const string Indirect = "I";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            /// <summary>
            /// Direct Labor
            /// </summary>
            public static string Direct => Messages.GetLocal(Messages.Direct);

            /// <summary>
            /// Indirect Labor
            /// </summary>
            public static string Indirect => Messages.GetLocal(Messages.Indirect);
        }

        /// <summary>
        /// Manufacturing Direct Labor
        /// </summary>
        public class direct : PX.Data.BQL.BqlString.Constant<direct>
        {
            public direct() : base(Direct){ ;}
        }
        /// <summary>
        /// Manufacturing Indirect Labor
        /// </summary>
        public class indirect : PX.Data.BQL.BqlString.Constant<indirect>
        {
            public indirect() : base(Indirect){ ;}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { 
                        Direct, 
                        Indirect}, 
                    new string[] {
                        Messages.Direct,
                        Messages.Indirect})
            { }
        }
    }
}