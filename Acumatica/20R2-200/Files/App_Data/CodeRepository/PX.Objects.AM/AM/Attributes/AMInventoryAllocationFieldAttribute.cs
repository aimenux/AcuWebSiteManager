using System;

namespace PX.Objects.AM.Attributes
{
    // Base attribute limited by property usage only. We need this for cache attached and need Method usage
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    [Serializable]
    public class AMInventoryAllocationFieldAttribute : PX.Objects.IN.Attributes.InventoryAllocationFieldAttribute
    {
    }
}