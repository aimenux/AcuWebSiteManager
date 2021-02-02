using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing - Order Type - Functions
    /// </summary>
    public class OrderTypeFunction
    {
        /// <summary>
        /// All  types of orders - order type - function
        /// </summary>
        public const int All = 0;

        /// <summary>
        /// Regular/standard production order - order type - function
        /// </summary>
        public const int Regular = 1;

        /// <summary>
        /// Planning order - order type - function
        /// Planning orders are not released or processed with transactions. They are for planning purposes only such as generated from MRP
        /// </summary>
        public const int Planning = 2;

        /// <summary>
        /// Disassemble order - order type - function
        /// Disassemble orders are for Disassembly only
        /// </summary>
        public const int Disassemble = 3;

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Regular => Messages.GetLocal(Messages.Regular);
            public static string Planning => Messages.GetLocal(Messages.Planning);
            public static string Disassemble => Messages.GetLocal(Messages.Disassemble);
            public static string All => Messages.GetLocal(Messages.All);
        }

        /// <summary>
        /// List of order type functions
        /// </summary>
        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                    new int[] { Regular, Planning, Disassemble },
                    new string[] { Messages.Regular, Messages.Planning, Messages.Disassemble })
            { }
        }

        /// <summary>
        /// List of order type functions
        /// </summary>
        public class ListAllAttribute : PXIntListAttribute
        {
            public ListAllAttribute()
                : base(
                    new int[] { All, Regular, Planning, Disassemble },
                    new string[] { Messages.All, Messages.Regular, Messages.Planning, Messages.Disassemble })
            { }
        }

        /// <summary>
        /// Regular/standard production order - order type - function
        /// </summary>
        public class regular : PX.Data.BQL.BqlInt.Constant<regular>
        {
            public regular() : base(Regular) { }
        }

        /// <summary>
        /// Planning order - order type - function
        /// Planning orders are not released or processed with transactions. They are for planning purposes only such as generated from MRP
        /// </summary>
        public class planning : PX.Data.BQL.BqlInt.Constant<planning>
        {
            public planning() : base(Planning) { }
        }

        /// <summary>
        /// Disassemble order - order type - function
        /// Disassemble orders are for Disassembly only
        /// </summary>
        public class disassemble : PX.Data.BQL.BqlInt.Constant<disassemble>
        {
            public disassemble() : base(Disassemble) { }
        }

        /// <summary>
        /// All Order Types - order type - function
        /// </summary>
        public class all : PX.Data.BQL.BqlInt.Constant<all>
        {
            public all() : base(All) { }
        }

        /// <summary>
        /// Return the functions user friendly description
        /// </summary>
        /// <param name="function">function ID</param>
        /// <returns>OrderTypeFunction.Desc value (same as the list drop down)</returns>
        public static string GetDescription(int? function)
        {
            if (function == null)
            {
                return Messages.Unknown;
            }

            return new OrderTypeFunction.ListAttribute().ValueLabelDic[function.GetValueOrDefault()];
        }
    }
}