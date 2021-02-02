﻿using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.CL.Graphs
{
    [DashboardType((int) DashboardTypeAttribute.Type.Default)]
    public class ComplianceDocumentEntry : PXGraph<ComplianceDocumentEntry>
    {
        [PXFilterable]
        public PXSelectOrderBy<ComplianceDocument, OrderBy<Asc<ComplianceDocument.complianceDocumentID>>> Documents;

        public PXSelect<CSAttributeGroup,
            Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>> ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;
        public PXSelect<ComplianceDocumentReference> DocumentReference;

        public PXSave<ComplianceDocument> Save;
        public PXCancel<ComplianceDocument> Cancel;

        public ComplianceDocumentEntry()
        {
            FeaturesSetHelper.CheckConstructionFeature();
            var service = new ComplianceDocumentService(this, ComplianceAttributeGroups, Documents, nameof(Documents));
            service.GenerateColumns(Documents.Cache, nameof(ComplianceAnswers));
        }

        public virtual void _(Events.RowUpdated<ComplianceDocument> args)
        {
            Documents.View.RequestRefresh();
        }

        public override void Persist()
        {
            base.Persist();
            Documents.View.RequestRefresh();
        }

        protected virtual void ComplianceDocument_DocumentType_FieldVerifying(PXCache cache,
            PXFieldVerifyingEventArgs arguments)
        {
            if (arguments.NewValue == null)
            {
                throw new PXSetPropertyException(ComplianceMessages.RequiredFieldMessage);
            }
        }
    }

    public class ComplianceDocumentEntryExt : ComplianceViewEntityExtension<ComplianceDocumentEntry, ComplianceDocument> { }
}