using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Models;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Reports.Data;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CN.Compliance.CL.Services
{
	public class EmailLienWaiverService : PrintEmailLienWaiverBaseService, IEmailLienWaiverService
    {
        public EmailLienWaiverService(PXGraph graph)
            : base(graph)
        {
            RecipientEmailDataProvider = graph.GetService<IRecipientEmailDataProvider>();
        }

        private IRecipientEmailDataProvider RecipientEmailDataProvider
        {
            get;
        }

        protected override void ProcessLienWaiver(NotificationSourceModel notificationSourceModel,
            ComplianceDocument complianceDocument)
        {
            base.ProcessLienWaiver(notificationSourceModel, complianceDocument);
            var notificationRecipients = GetNotificationRecipients(
                notificationSourceModel.NotificationSource, notificationSourceModel.VendorId);
            notificationRecipients.ForEach(nr =>
                SendEmail(nr, notificationSourceModel, complianceDocument));

            PXProcessing.SetProcessed();
		}

        private void SendEmail(NotificationRecipient notificationRecipient,
            NotificationSourceModel notificationSourceModel, ComplianceDocument complianceDocument)
        {
            var email = RecipientEmailDataProvider
                .GetRecipientEmail(notificationRecipient, notificationSourceModel.VendorId);
            if (email != null)
            {
                var report = GetReportInAppropriateFormat(notificationRecipient.Format,
                    notificationSourceModel, complianceDocument);
                var sender = TemplateNotificationGenerator.Create(complianceDocument,
                    notificationSourceModel.NotificationSource.NotificationID);
                sender.MailAccountId = notificationSourceModel.NotificationSource.EMailAccountID;
                sender.RefNoteID = complianceDocument.NoteID;
                sender.To = email;
                sender.AddAttachmentLink(report.ReportFileInfo.UID.GetValueOrDefault());
                sender.Send();
                UpdateLienWaiverProcessedStatus(complianceDocument);
            }
        }

        private LienWaiverReportGenerationModel GetReportInAppropriateFormat(
            string format, NotificationSourceModel notificationSourceModel, ComplianceDocument complianceDocument)
        {
            return format == NotificationFormat.Excel
                ? LienWaiverReportCreator.CreateReport(
                    notificationSourceModel.NotificationSource.ReportID, complianceDocument,
                    notificationSourceModel.IsJointCheck, ReportProcessor.FilterExcel, false)
                : LienWaiverReportGenerationModel;
        }

        private IEnumerable<NotificationRecipient> GetNotificationRecipients(
            NotificationSource notificationSource, int? vendorId)
        {
            var notificationRecipientsQuery = GetNotificationRecipientsQuery(notificationSource, vendorId);
            var notificationRecipientGroups =
                notificationRecipientsQuery.FirstTableItems.GroupBy(nr => nr.ContactID);
            return notificationRecipientGroups.Select(nr => GetAppropriateNotificationRecipient(nr.ToList()))
                .Where(nr => nr.Active == true).ToList();
        }

        private static NotificationRecipient GetAppropriateNotificationRecipient(
            IReadOnlyCollection<NotificationRecipient> notificationRecipients)
        {
            return notificationRecipients.HasAtLeastTwoItems()
                ? notificationRecipients.Single(nr => nr.ClassID == null)
                : notificationRecipients.Single();
        }

        private PXResultset<NotificationRecipient> GetNotificationRecipientsQuery(
            NotificationSource notificationSource, int? vendorId)
        {
            var classId = VendorDataProvider.GetVendor(PrintEmailLienWaiversProcess, vendorId).ClassID;
            return SelectFrom<NotificationRecipient>
                .Where<NotificationRecipient.setupID.IsEqual<P.AsGuid>
                    .And<NotificationRecipient.sourceID.IsEqual<P.AsInt>
                        .Or<NotificationRecipient.classID.IsEqual<P.AsString>>>>.View
                .Select(PrintEmailLienWaiversProcess, notificationSource.SetupID, notificationSource.SourceID, classId);
        }
    }
}