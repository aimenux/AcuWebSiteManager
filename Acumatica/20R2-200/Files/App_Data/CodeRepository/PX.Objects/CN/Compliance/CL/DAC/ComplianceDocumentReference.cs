using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [PXCacheName("Compliance Document Reference")]
    public class ComplianceDocumentReference : BaseCache, IBqlTable
    {
        [PXDBGuid(IsKey = true)]
        public virtual Guid? ComplianceDocumentReferenceId
        {
            get;
            set;
        }

        [PXDBString]
        public virtual string Type
        {
            get;
            set;
        }

        [PXDBString]
        public virtual string ReferenceNumber
        {
            get;
            set;
        }

        [PXDBGuid]
        public virtual Guid? RefNoteId
        {
            get;
            set;
        }

        [PXDBCreatedByID(Visibility = PXUIVisibility.Invisible)]
        public override Guid? CreatedById
        {
            get;
            set;
        }

        public abstract class complianceDocumentReferenceId : BqlGuid.Field<complianceDocumentReferenceId>
        {
        }

        public abstract class type : BqlString.Field<type>
        {
        }

        public abstract class referenceNumber : BqlString.Field<referenceNumber>
        {
        }

        public abstract class refNoteId : BqlGuid.Field<refNoteId>
        {
        }
    }
}