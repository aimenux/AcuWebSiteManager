using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CL.Graphs
{
    public class PrintEmailLienWaiversProcess : PXGraph<PrintEmailLienWaiversProcess>
    {
        public PXCancel<ProcessLienWaiversFilter> Cancel;

        public PXFilter<ProcessLienWaiversFilter> Filter;

        [PXFilterable]
        public PXFilteredProcessingJoin<ComplianceDocument, ProcessLienWaiversFilter,
            LeftJoin<ComplianceAttribute,
                On<ComplianceDocument.documentTypeValue.IsEqual<ComplianceAttribute.attributeId>>,
            InnerJoin<ComplianceAttributeType,
                On<ComplianceDocument.documentType.IsEqual<ComplianceAttributeType.complianceAttributeTypeID>>>>,
            Where<ComplianceAttributeType.type.IsEqual<ComplianceDocumentType.lienWaiver>
                .And<ComplianceDocument.projectID.IsEqual<ProcessLienWaiversFilter.projectId.FromCurrent>
                    .Or<ProcessLienWaiversFilter.projectId.FromCurrent.IsNull>>
                .And<ComplianceDocument.vendorID.IsEqual<ProcessLienWaiversFilter.vendorId.FromCurrent>
                    .Or<ProcessLienWaiversFilter.vendorId.FromCurrent.IsNull>>
                .And<ComplianceAttribute.value.IsEqual<ProcessLienWaiversFilter.lienWaiverType.FromCurrent>
                    .Or<ProcessLienWaiversFilter.lienWaiverType.FromCurrent.IsNull>>
                .And<ComplianceDocument.creationDate.IsGreaterEqual<ProcessLienWaiversFilter.startDate.FromCurrent>
                    .Or<ProcessLienWaiversFilter.startDate.FromCurrent.IsNull>>
                .And<ComplianceDocument.creationDate.IsLessEqual<ProcessLienWaiversFilter.endDate.FromCurrent>
                    .Or<ProcessLienWaiversFilter.endDate.FromCurrent.IsNull>>
                .And<ComplianceDocument.isProcessed.IsEqual<False>
                    .Or<ComplianceDocument.isProcessed.IsNull>
                    .Or<ProcessLienWaiversFilter.shouldShowProcessed.FromCurrent.IsEqual<True>>>>> LienWaivers;

        public PrintEmailLienWaiversProcess()
        {
            FeaturesSetHelper.CheckConstructionFeature();
        }

        [InjectDependency]
        public IPrintLienWaiversService PrintLienWaiversService
        {
            get;
            set;
        }

        [InjectDependency]
        public IEmailLienWaiverService EmailLienWaiverService
        {
            get;
            set;
        }

        protected virtual void _(Events.RowInserted<ProcessLienWaiversFilter> args)
        {
            var filter = args.Row;
            if (filter != null)
            {
                filter.StartDate = Accessinfo.BusinessDate;
                filter.EndDate = Accessinfo.BusinessDate;
            }
        }

        protected virtual void _(Events.RowSelected<ProcessLienWaiversFilter> args)
        {
            var filter = args.Row;
            if (filter?.Action != null)
            {
                InitializeProcessDelegate(filter.Action);
                SetPrintSettingFieldsVisibility(args.Cache, filter);
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            var complianceDocument = args.Row;
            if (complianceDocument != null && !PrintLienWaiversService.IsLienWaiverValid(complianceDocument))
            {
                args.Cache.RaiseException<ComplianceDocument.documentTypeValue>(complianceDocument,
                    ComplianceMessages.LienWaiver.DocumentTypeOptionVendorAndProjectMustBeSpecified, errorLevel:
                    PXErrorLevel.RowWarning);
            }
        }

        private void InitializeProcessDelegate(string action)
        {
            if (action == ProcessLienWaiverActionsAttribute.PrintLienWaiver)
            {
                LienWaivers.SetProcessDelegate(PrintLienWaiversService.Process);
            }
            else
            {
                LienWaivers.SetProcessDelegate(EmailLienWaiverService.Process);
            }
        }

        private static void SetPrintSettingFieldsVisibility(PXCache cache, ProcessLienWaiversFilter filter)
        {
            var shouldShowPrintSettings = PXAccess.FeatureInstalled<FeaturesSet.deviceHub>() &&
                filter.Action == ProcessLienWaiverActionsAttribute.PrintLienWaiver;
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter
                .printWithDeviceHub>(cache, filter, shouldShowPrintSettings);
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter
                .definePrinterManually>(cache, filter, shouldShowPrintSettings);
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter.printerID>(cache, filter, shouldShowPrintSettings);
            PXUIFieldAttribute.SetVisible<ProcessLienWaiversFilter.numberOfCopies>(cache, filter,
                shouldShowPrintSettings);
        }
    }
}