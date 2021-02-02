using System;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
    /// <summary>
    /// This attribute used for dynamic columns generation with description fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldDescriptionForDynamicColumnsAttribute : Attribute
    {
    }
}
