using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.Common.Extensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Graphs;
using PX.Objects.CN.Compliance.CL.Models;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CL.Services
{
    public class PrintEmailLienWaiverBaseService : IPrintEmailLienWaiverBaseService
    {
        protected PrintEmailLienWaiversProcess PrintEmailLienWaiversProcess;
        protected ILienWaiverReportCreator LienWaiverReportCreator;
        protected IEnumerable<ComplianceDocument> ComplianceDocuments;
        protected LienWaiverReportGenerationModel LienWaiverReportGenerationModel;

        public PrintEmailLienWaiverBaseService(PXGraph graph)
        {
            PrintEmailLienWaiversProcess = (PrintEmailLienWaiversProcess) graph;
            LienWaiverReportCreator = PrintEmailLienWaiversProcess.GetService<ILienWaiverReportCreator>();
        }

        public bool IsLienWaiverValid(ComplianceDocument complianceDocument)
        {
            return complianceDocument.DocumentTypeValue != null && complianceDocument.ProjectID.IsNotIn(null, 0) &&
                complianceDocument.VendorID != null;
        }

        public virtual void Process(List<ComplianceDocument> complianceDocuments)
        {
            PrintEmailLienWaiversProcess.Persist();
            ComplianceDocuments = complianceDocuments.Where(IsLienWaiverValid);
            foreach (var complianceDocument in ComplianceDocuments)
            {
                var notificationSourceModels = CreateNotificationSourceModels(complianceDocument);
                notificationSourceModels.ForEach(nsm => ProcessLienWaiver(nsm, complianceDocument));
            }
        }

        protected virtual void ProcessLienWaiver(NotificationSourceModel notificationSourceModel,
            ComplianceDocument complianceDocument)
        {
	        PXProcessing.SetCurrentItem(complianceDocument);

			LienWaiverReportGenerationModel = LienWaiverReportCreator.CreateReport(
                notificationSourceModel.NotificationSource.ReportID, complianceDocument,
                notificationSourceModel.IsJointCheck);
        }

        protected void UpdateLienWaiverProcessedStatus(ComplianceDocument complianceDocument)
        {
            complianceDocument.IsProcessed = true;
            PrintEmailLienWaiversProcess.LienWaivers.Cache.Update(complianceDocument);
            PrintEmailLienWaiversProcess.LienWaivers.Cache.Persist(PXDBOperation.Update);
        }

        private string GetLienWaiverType(int? lienWaiverTypeId)
        {
            return PrintEmailLienWaiversProcess.Select<ComplianceAttribute>()
                .Single(ca => ca.AttributeId == lienWaiverTypeId).Value;
        }

        private static bool IsNotificationSourceValid(NotificationSource notificationSource)
        {
            return notificationSource?.ReportID != null && notificationSource.Active == true;
        }

        private IEnumerable<NotificationSourceModel> CreateNotificationSourceModels(
            ComplianceDocument complianceDocument)
        {
            var notificationSourceModels = new List<NotificationSourceModel>();
            AddNotificationSourceModelIfRequired(notificationSourceModels,
                complianceDocument.DocumentTypeValue, complianceDocument.VendorID, false);
            if (complianceDocument.JointVendorInternalId != null)
            {
                AddNotificationSourceModelIfRequired(notificationSourceModels,
                    complianceDocument.DocumentTypeValue, complianceDocument.JointVendorInternalId, true);
            }
            return notificationSourceModels;
        }

        private void AddNotificationSourceModelIfRequired(ICollection<NotificationSourceModel> notificationSourceModels,
            int? lienWaiverType, int? vendorId, bool isJointCheck)
        {
            var notificationSourceModel = CreateNotificationSourceModel(lienWaiverType, vendorId, isJointCheck);
            if (IsNotificationSourceValid(notificationSourceModel.NotificationSource))
            {
                notificationSourceModels.Add(notificationSourceModel);
            }
        }

        private NotificationSourceModel CreateNotificationSourceModel(int? lienWaiverTypeId,
            int? vendorId, bool isJointCheck)
        {
            var notificationSource = GetNotificationSource(lienWaiverTypeId, vendorId);
            return new NotificationSourceModel
            {
                NotificationSource = notificationSource,
                VendorId = vendorId,
                IsJointCheck = isJointCheck
            };
        }

        private NotificationSource GetNotificationSource(int? lienWaiverTypeId, int? vendorId)
        {
            var vendorNoteId = PrintEmailLienWaiversProcess.Select<BAccount>()
                .Single(v => v.BAccountID == vendorId).NoteID;
            var lienWaiverType = GetLienWaiverType(lienWaiverTypeId);
            return GetNotificationSource(lienWaiverType, vendorNoteId) ??
                GetNotificationSourceForVendorClass(lienWaiverType, vendorId);
        }

        private NotificationSource GetNotificationSource(string lienWaiverType, Guid? vendorNoteId)
        {
            return SelectFrom<NotificationSource>
                .InnerJoin<NotificationSetup>
                    .On<NotificationSource.setupID.IsEqual<NotificationSetup.setupID>>
                .Where<NotificationSetup.notificationCD.IsEqual<P.AsString>
                    .And<NotificationSource.refNoteID.IsEqual<P.AsGuid>>>.View
                .Select(PrintEmailLienWaiversProcess, lienWaiverType, vendorNoteId);
        }

        private NotificationSource GetNotificationSourceForVendorClass(string lienWaiverType, int? vendorId)
        {
            return SelectFrom<NotificationSource>
                .InnerJoin<NotificationSetup>
                    .On<NotificationSource.setupID.IsEqual<NotificationSetup.setupID>>
                .InnerJoin<Vendor>
                    .On<NotificationSource.classID.IsEqual<Vendor.vendorClassID>>
                .Where<NotificationSetup.notificationCD.IsEqual<P.AsString>
                    .And<Vendor.bAccountID.IsEqual<P.AsInt>>>.View
                .Select(PrintEmailLienWaiversProcess, lienWaiverType, vendorId);
        }
    }
}