using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Cross (X) Reference Type
    /// </summary>
    public class XRefType
    {
        /// <summary>
        /// Purchase X Reference type = P
        /// </summary>
        public const string Purchase = "P";

        /// <summary>
        /// Manufacture X Reference type = M
        /// </summary>
        public const string Manufacture = "M";

        /// <summary>
        /// Transfer X Reference type = T
        /// </summary>
        public const string Transfer = "T";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Purchase => Messages.GetLocal(Messages.PurchasedXRef);
            public static string Manufacture => Messages.GetLocal(Messages.ManufactureXRef);
            public static string Transfer => Messages.GetLocal(Messages.TransferXRef);
        }

        public class purchase : PX.Data.BQL.BqlString.Constant<purchase>
        {
            public purchase() : base(Purchase) {}
        }

        public class manufacture : PX.Data.BQL.BqlString.Constant<manufacture>
        {
            public manufacture() : base(Manufacture) {}
        }

        public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
        {
            public transfer() : base(Transfer) {}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                       new string[] { 
                           Manufacture,
                           Purchase,
                           Transfer},
                       new string[] {
                           Messages.PurchasedXRef,
                           Messages.PurchasedXRef,
                           Messages.TransferXRef})
            { }
        }
    }
}