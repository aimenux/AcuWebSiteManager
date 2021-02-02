using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Indirect Labor Selector Attribute.
    /// </summary>

    public class IndirectLaborAttribute : PXSelectorAttribute
    {
        public IndirectLaborAttribute()
            : base(typeof(Search<AMLaborCode.laborCodeID, Where<AMLaborCode.laborType, Equal<AMLaborType.indirect>>>)
            , new Type[] { typeof(AMLaborCode.laborCodeID), typeof(AMLaborCode.descr) })
        {

        }
    }
}