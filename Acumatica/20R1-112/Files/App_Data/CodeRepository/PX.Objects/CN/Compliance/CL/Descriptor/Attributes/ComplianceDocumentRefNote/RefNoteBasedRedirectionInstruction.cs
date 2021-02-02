using System;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote
{
    public class RefNoteBasedRedirectionInstruction
    {
        public RefNoteBasedRedirectionInstruction(Type entityType, Type graphType, Type referenceTypeField,
            Type referenceNumberField)
        {
            EntityType = entityType;
            GraphType = graphType;
            ReferenceTypeField = referenceTypeField;
            ReferenceNumberField = referenceNumberField;
        }

        public Type EntityType
        {
            get;
        }

        public Type GraphType
        {
            get;
        }

        public Type ReferenceTypeField
        {
            get;
        }

        public Type ReferenceNumberField
        {
            get;
        }
    }
}