using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Graphs;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Services
{
	public class RequestForInformationEmailActivityService : EmailActivityService<RequestForInformation>
	{
		public RequestForInformationEmailActivityService(RequestForInformationMaint graph)
			: base(graph, graph.CurrentRequestForInformation.Current.OwnerID)
		{
			Entity = graph.CurrentRequestForInformation.Current;
		}

		public PXGraph GetEmailActivityGraph()
		{
			return GetEmailActivityGraph<RequestForInformation.noteID>();
		}

		protected override CRSMEmail GetEmailEntity(string recipientEmails)
		{
			var email = base.GetEmailEntity(recipientEmails);
			email.MailCc = GetSelectedContacts();
			return email;
		}

		/// <summary>
		/// TODO: review <see cref="RequestsForInformation.Descriptor.RequestForInformationMessages.RequestForInformationEmailDefaultSubject"/>.
		/// </summary>
		protected override string GetSubject()
		{
			var projectNumber = GetProjectNumber();
			return $"RFI #[{Entity.RequestForInformationCd} {projectNumber}] {Entity.Summary}";
		}

		public override string GetRecipientEmails()
		{
			var contact = GetContact();
			return contact.EMail != null
				? $"{contact.DisplayName} <{contact.EMail}>"
				: null;
		}

		private string GetSelectedContacts()
		{
			var requestsForInformationRelation = GetRequestsForInformationRelation();
			return string.Concat(requestsForInformationRelation.Select(r => $"{r.ContactName} <{r.ContactEmail}>; "));
		}

		private IEnumerable<RequestForInformationRelation> GetRequestsForInformationRelation()
		{
			return ((RequestForInformationMaint)Graph).Relations.Select().FirstTableItems.GroupBy(r => r.ContactId)
				.Select(r => r.First()).Where(r => r.AddToCc == true && r.ContactEmail != null);
		}

		private Contact GetContact()
		{
			return new PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>(Graph)
				.SelectSingle(Entity.ContactId);
		}
	}
}
