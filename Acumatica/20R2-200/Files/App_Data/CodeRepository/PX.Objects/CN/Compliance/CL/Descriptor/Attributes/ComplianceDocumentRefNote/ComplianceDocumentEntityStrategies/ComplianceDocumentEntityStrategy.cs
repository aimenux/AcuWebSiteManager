using System;
using PX.Data;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public abstract class ComplianceDocumentEntityStrategy
    {
        public Type EntityType
        {
            get;
            protected set;
        }

        public Type FilterExpression
        {
            get;
            protected set;
        }

        public Type TypeField
        {
            get;
            protected set;
        }

        public string[] TypeFilterValues
        {
            get;
            protected set;
        }

        public string[] TypeFilterLabels
        {
            get;
            protected set;
        }

        public abstract Guid? GetNoteId(PXGraph graph, string clDisplayName);
    }
}