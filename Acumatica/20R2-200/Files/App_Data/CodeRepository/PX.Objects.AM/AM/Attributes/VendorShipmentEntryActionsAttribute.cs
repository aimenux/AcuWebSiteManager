using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false)]
    public class VendorShipmentEntryActionsAttribute : PXIntListAttribute
    {
        public const int ConfirmShipment = 1;

        public VendorShipmentEntryActionsAttribute() : base(
            new[]
            {
                Pair(ConfirmShipment, Messages.GetLocal(Messages.ConfirmShipment))
            })
        { }
    }
}