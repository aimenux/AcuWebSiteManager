using System;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [Serializable]
    [PXCacheName("Compliance Attribute Type")]
    public class ComplianceAttributeType : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? ComplianceAttributeTypeID
        {
            get;
            set;
        }

        [PXDBString(255)]
        [PXDefault]
        public virtual string Type
        {
            get;
            set;
        }

        public abstract class complianceAttributeTypeID : BqlInt.Field<complianceAttributeTypeID>
        {
        }

        public abstract class type : BqlString.Field<type>
        {
        }
    }
}