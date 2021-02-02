using System;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.GL.DAC;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PM;
using PX.SM;
using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.PJ.RequestsForInformation.CR.Services
{
    public class EmailTemplateService
    {
        private readonly PXGraph graph;

        public EmailTemplateService(PXGraph graph)
        {
            this.graph = graph;
        }

        public string InsertRecipientNote(CRSMEmail email, int? notificationId, string requestForInformationCd,
            string recipientNotes)
        {
            var emailBody = GenerateEmailBodyWithLogo(email, notificationId, requestForInformationCd);
            var content = string.Format(
	            RequestForInformationConstants.EmailTemplate.RecipientNotesTag, recipientNotes);
            return string.Concat(content, emailBody);
        }

        public string GenerateEmailBodyWithLogo(CRSMEmail email, int? notificationId, string requestForInformationCd)
        {
            var emailBody = GenerateEmailBody(email, notificationId, requestForInformationCd);
            var logoTagIndex = emailBody.IndexOf(
	            RequestForInformationConstants.EmailTemplate.ResponseNoteTag, StringComparison.Ordinal);
            return InsertLogo(logoTagIndex, emailBody, requestForInformationCd);
        }

        private RequestForInformationMaint CreateRequestForInformationMaint(CRSMEmail email)
        {
            var requestForInformationMaint = PXGraph.CreateInstance<RequestForInformationMaint>();
            var requestForInformation = GetRequestForInformation(email);
            requestForInformationMaint.RequestForInformation.Current = requestForInformation;
            requestForInformationMaint.Email.Current = email;
            return requestForInformationMaint;
        }

        private RequestForInformation GetRequestForInformation(CRSMEmail email)
        {
            var query = new PXSelect<RequestForInformation,
                Where<RequestForInformation.noteID, Equal<Required<RequestForInformation.noteID>>>>(graph);
            return query.SelectSingle(email.RefNoteID);
        }

        private Notification GetNotification(int? notificationId)
        {
            var query = new PXSelect<Notification,
                Where<Notification.notificationID, Equal<Required<Notification.notificationID>>>>(graph);
            return query.SelectSingle(notificationId);
        }

        private Branch GetBranch(int? branchId)
        {
            var query = new PXSelect<Branch, Where<Branch.branchID,
                Equal<Required<Branch.branchID>>>>(graph);
            return query.SelectSingle(branchId);
        }

        private PMProject GetProject(string requestForInformationCd)
        {
            var query =
                new PXSelectJoin<PMProject,
                    LeftJoin<RequestForInformation, On<PMProject.contractID, Equal<RequestForInformation.projectId>>>,
                    Where<RequestForInformation.requestForInformationCd,
                        Equal<Required<RequestForInformation.requestForInformationCd>>>>(graph);
            return query.SelectSingle(requestForInformationCd);
        }

        private Organization GetOrganization(int? organizationId)
        {
            var query = new PXSelect<Organization, Where<Organization.organizationID,
                Equal<Required<Organization.organizationID>>>>(graph);
            return query.SelectSingle(organizationId);
        }

        private string InsertLogo(int logoTagIndex, string emailBody, string requestForInformationCd)
        {
            if (logoTagIndex.Equals(RequestForInformationConstants.EmailTemplate.IndexNotFound))
            {
                return emailBody;
            }
            var logoName = GetLogoName(requestForInformationCd);
            var content = GetLogoContent(logoName);
            return emailBody.Insert(
                logoTagIndex + RequestForInformationConstants.EmailTemplate.ResponseNoteTag.Length, content);
        }

        private string GetLogoName(string requestForInformationCd)
        {
            var project = GetProject(requestForInformationCd);
            var branch = GetBranch(project.DefaultBranchID);
            return GetBranchLogoName(branch);
        }

        private string GetBranchLogoName(Branch branch)
        {
            return branch != null
                ? branch.LogoName ?? GetOrganizationLogoName(branch)
                : null;
        }

        private string GetOrganizationLogoName(Branch branch)
        {
            var organization = GetOrganization(branch.OrganizationID);
            return organization.LogoName;
        }

        private static string GetLogoContent(string logoName)
        {
            var content = logoName ?? string.Empty;
            return string.Format(RequestForInformationConstants.EmailTemplate.LogoTag, content, content);
        }

        private string GenerateEmailBody(CRSMEmail email, int? notificationId, string requestForInformationCd)
        {
            var notification = GetNotification(notificationId);
            var requestForInformationMaint = CreateRequestForInformationMaint(email);
            var requestForInformationKeys = ((object) requestForInformationCd).CreateArray();
            return PXTemplateContentParser.Instance.Process(notification.Body, requestForInformationMaint,
                typeof(RequestForInformation), requestForInformationKeys);
        }
    }
}