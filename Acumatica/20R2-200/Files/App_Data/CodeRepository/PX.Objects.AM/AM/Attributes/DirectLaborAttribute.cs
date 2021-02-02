using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Direct Labor Selector Attribute.
    /// </summary>
    
    public class DirectLaborAttribute : PXSelectorAttribute
    {
        public DirectLaborAttribute()
            : base(typeof(Search<AMLaborCode.laborCodeID, Where<AMLaborCode.laborType, Equal<AMLaborType.direct>>>)
            , new Type[] {typeof(AMLaborCode.laborCodeID), typeof(AMLaborCode.descr)})
            {
                
            }
    }
}