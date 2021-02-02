using PX.Data;

namespace PX.Objects.CN.Subcontracts.PM.Descriptor
{
    public class CommitmentBaseType
    {
        public const string PurchaseOrder = "Purchase Order";
        public const string Subcontract = "Subcontract";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] CommitmentBaseTypes =
            {
                PurchaseOrder,
                Subcontract
            };

            public ListAttribute()
                : base(CommitmentBaseTypes, CommitmentBaseTypes)
            {
            }
        }
    }
}