using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [Serializable]
    [PXCacheName("Compliance Attribute")]
    public class ComplianceAttribute : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? AttributeId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search<ComplianceAttributeType.complianceAttributeTypeID,
                Where<ComplianceAttributeType.type, NotEqual<ComplianceDocumentType.status>>>),
            SubstituteKey = typeof(ComplianceAttributeType.type))]
        [PXDefault]
        public virtual int? Type
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Value")]
        [PXDefault]
        public virtual string Value
        {
            get;
            set;
        }

        public abstract class attributeId : BqlInt.Field<attributeId>
        {
        }

        public abstract class type : BqlInt.Field<type>
        {
        }

        public abstract class value : BqlString.Field<value>
        {
        }
    }
}