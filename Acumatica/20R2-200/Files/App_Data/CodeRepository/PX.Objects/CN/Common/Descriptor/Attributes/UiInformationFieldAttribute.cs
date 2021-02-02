using System;
using PX.Objects.CN.Common.Services;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
    /// <summary>
    /// This attribute mark property as information field with dynamic colomns in grid.
    /// Used in <see cref="CommonAttributeColumnCreator"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UiInformationFieldAttribute : Attribute
    {
    }
}