using System;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [Serializable]
    [PXCacheName("Compliance Attribute Filter")]
    public class ComplianceAttributeFilter : IBqlTable
    {
        [PXInt]
        [PXUIField(DisplayName = "Attribute", Visibility = PXUIVisibility.SelectorVisible)]
        [PXUnboundDefault(typeof(Search<ComplianceAttributeType.complianceAttributeTypeID,
            Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.certificate>>>))]
        [PXSelector(typeof(Search<ComplianceAttributeType.complianceAttributeTypeID>),
            SubstituteKey = typeof(ComplianceAttributeType.type))]
        public virtual int? Type
        {
            get;
            set;
        }

        public abstract class type : BqlInt.Field<type>
        {
        }
    }
}