using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// <see cref="PXReferentialIntegrityCheckAttribute"/> with Method usage for CacheAttached.
    /// Should only be used on CacheAttached as needed if not possible to add <see cref="PXReferentialIntegrityCheckAttribute"/> directly to the DAC
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PXReferentialIntegrityCheck2Attribute : PXReferentialIntegrityCheckAttribute { }
}