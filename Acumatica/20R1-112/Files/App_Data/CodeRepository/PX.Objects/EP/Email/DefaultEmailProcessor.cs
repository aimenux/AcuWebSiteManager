using System;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class DefaultEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			EMailAccount account = package.Account;
			CRSMEmail message = package.Message;
			if (message.RefNoteID != null) return false;
			if (message.Ticket == null) return false;

			var graph = package.Graph;
			if (message.StartDate == null)
				message.StartDate = PXTimeZoneInfo.Now;

			//Evaludate sender only if standart email processing
			if(account.EmailAccountType == EmailAccountTypesAttribute.Standard)
				message.OwnerID = GetKnownSender(graph, message);

			var parentMessage = GetParentOriginalActivity(graph, (int)message.Ticket);
			if (parentMessage == null) return false;

			message.ParentNoteID = parentMessage.NoteID;
			message.RefNoteID = parentMessage.RefNoteID;
			message.BAccountID = parentMessage.BAccountID;
			message.ContactID = parentMessage.ContactID;
			message.DocumentNoteID = parentMessage.DocumentNoteID;

			if (parentMessage.ProjectID != null)
			{
				var timeAct = (PMTimeActivity)graph.Caches[typeof(PMTimeActivity)].Insert();

				timeAct.ProjectID = parentMessage.ProjectID;
				timeAct.ProjectTaskID = parentMessage.ProjectTaskID;

				graph.Caches[typeof(PMTimeActivity)].Update(timeAct);
			}
			
			message.IsPrivate = parentMessage.IsPrivate;

			if (message.OwnerID == null && account.EmailAccountType == EmailAccountTypesAttribute.Standard)
			{
				try
				{
					message.WorkgroupID = parentMessage.WorkgroupID;
					graph.Caches[typeof(CRSMEmail)].SetValueExt<CRSMEmail.ownerID>(message, parentMessage.OwnerID);
				}
				catch (PXSetPropertyException)
				{
					message.OwnerID = null;
				}				
			}
			return true;
		}
		
		private Guid? GetKnownSender(PXGraph graph, CRSMEmail message)
		{
			var @from = Mailbox.Parse(message.MailFrom).With(_ => _.Address).With(_ => _.Trim());

			PXSelectJoin<EPEmployee,
				InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>,
				InnerJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
				Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
				Clear(graph);

			var employeeEmail = (EPEmployee)PXSelectJoin<EPEmployee,
				InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>,
				InnerJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>,
				Where<Contact.eMail, Equal<Required<Contact.eMail>>>>.
				Select(graph, @from);
			if (employeeEmail != null) return employeeEmail.UserID;

			return null;
		}

		public static CRPMSMEmail GetParentOriginalActivity(PXGraph graph, int id)
		{
			PXSelectReadonly<CRPMSMEmail,
				Where<CRPMSMEmail.id, Equal<Required<CRPMSMEmail.id>>>>.
				Clear(graph);

			var res = (CRPMSMEmail)PXSelectReadonly<CRPMSMEmail,
				Where<CRPMSMEmail.id, Equal<Required<CRPMSMEmail.id>>>>.
				Select(graph, id);

			while (res != null && res.ClassID == CRActivityClass.EmailRouting)
			{
				if (res.ParentNoteID == null) res = null;
				else
					res = (CRPMSMEmail)PXSelectReadonly<CRPMSMEmail,
							Where<CRPMSMEmail.noteID, Equal<Required<CRPMSMEmail.noteID>>>>.
							Select(graph, res.ParentNoteID);
			}
			return res;
		}
	}
}
