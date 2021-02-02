using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// PXButton by default is not inherited
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class InheritedPXButtonAttribute : PXButtonAttribute
    {
    }
}