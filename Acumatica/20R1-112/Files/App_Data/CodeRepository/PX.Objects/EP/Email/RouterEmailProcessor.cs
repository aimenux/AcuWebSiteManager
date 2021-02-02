using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class RouterEmailProcessor : BaseRoutingProcessor
	{
		protected override bool Process(Package package)
		{
			PXGraph graph = package.Graph;
			EMailAccount account = package.Account;
			CRSMEmail message = package.Message;
			
			if (account != null && (
					(account.ForbidRouting ?? false) ||
					!(account.RouteEmployeeEmails ?? false)
				)) return false;

			bool? isFromInternalUser = IsFromInternalUser(graph, message);
			var recipients = new MailAddressList();

			if (isFromInternalUser == true)
				recipients.AddRange(GetExternalRecipient(graph, message));
			else if (isFromInternalUser == false)
				recipients.AddRange(GetInternalRecipient(graph, message));
			else
				return false; // for those users and employees which are disabled or RouteEmails switched off

			RemoveAddress(recipients, message.MailFrom);
			RemoveAddress(recipients, message.MailTo);
			RemoveAddress(recipients, message.MailCc);
			RemoveAddress(recipients, message.MailBcc);

			if (recipients.Count == 0)
			{
				return false;
			}

			if (isFromInternalUser == true)
			{
				SendCopyMessageToOutside(graph, package.Account, message, recipients);
				MarkAsRoutingEmail(graph, message);
                MarkAsRead(graph, message);
			}
			else
			{
				SendCopyMessageToInside(graph, package.Account, message, recipients);
			}

			graph.EnsureCachePersistence(message.GetType());

			return true;
		}
		
		private List<MailAddress> GetExternalRecipient(PXGraph graph, CRSMEmail message)
		{
			var previousMessage = GetPreviousExternalMessage(graph, message);
			var result = new List<MailAddress>();
			if (previousMessage != null)
			{
				result = GetExternalMailOutside(previousMessage);

				if (previousMessage.OwnerID != null)
				{
					var ownerAddress = GetOwnerAddress(graph, previousMessage.OwnerID);
					if (ownerAddress != null)
						result.Add(ownerAddress);
				}
			}

			return result;
			//TODO: need implementation
			//var source = FindSource(graph, (long)message.RefNoteID);			
		}

		private List<MailAddress> GetInternalRecipient(PXGraph graph, CRSMEmail message)
		{
			//var previousMessage = GetPreviousInternalMessage(graph, message);

			//if (previousMessage != null)
			//	return GetInternalMailOutside(previousMessage);

			var result = new List<MailAddress>();
			var initialMessage = GetParentMessage(graph, message);

			if (message.OwnerID != null)
			{
				var ownerAddress = GetOwnerAddress(graph, message.OwnerID);
				if (ownerAddress != null)
					result.Add(ownerAddress);
			}
			if (initialMessage != null && initialMessage.OwnerID != message.OwnerID)
			{
				var ownerAddress = GetOwnerAddress(graph, initialMessage.OwnerID);
				if (ownerAddress != null)
					result.Add(ownerAddress);
			}
			
			if (result.Count == 0)
			{
				var ownerAddress = GetOwnerAddressByNote(graph, message.RefNoteID);
				if (ownerAddress != null) result.Add(ownerAddress);
				var parentOwnerAddress = GetOwnerAddress(graph, message.BAccountID);
				if (parentOwnerAddress != null) result.Add(parentOwnerAddress);
			}
			return result;
		}
		
		private List<MailAddress> GetOwnerAddress(PXGraph graph, Guid? ownerId)
		{
			Contact owner;
			Users user;
			EPEmployee employee;
			FindOwner(graph, ownerId, out owner, out user, out employee);

			if (PreventEmailRoutingFor(user, employee)) return null;

			return GenerateAddress(owner, user);
		}

		private List<MailAddress> GetOwnerAddress(PXGraph graph, int? bAccID)
		{
			PXSelectJoin<Users,
				LeftJoin<EPEmployee, 
					On<EPEmployee.userID, Equal<Users.pKID>>,
				LeftJoin<Contact, 
					On<Contact.contactID, Equal<EPEmployee.defContactID>>>>,
				Where<Contact.bAccountID, Equal<Required<Contact.bAccountID>>>>
				.Clear(graph);

			var row = (PXResult<Users, EPEmployee, Contact>)PXSelectJoin<Users,
				LeftJoin<EPEmployee, 
					On<EPEmployee.userID, Equal<Users.pKID>>,
				LeftJoin<Contact, 
					On<Contact.contactID, Equal<EPEmployee.defContactID>>>>,
				Where<Contact.bAccountID, Equal<Required<Contact.bAccountID>>>>
				.Select(graph, bAccID);

			if (row == null) return null;
			var owner = row.GetItem<Contact>();
			var user = row.GetItem<Users>();
			var employee = row.GetItem<EPEmployee>();

			if (PreventEmailRoutingFor(user, employee)) return null;

			return GenerateAddress(owner, user);
		}

		private bool PreventEmailRoutingFor(Users user, EPEmployee employee)
		{
			return (user?.State == Users.state.Disabled
				|| employee?.Status == BAccount.status.Inactive);
		}

		private List<MailAddress> GetExternalMailOutside(CRSMEmail message)
		{
			var prevActivity = message;
			var result = new List<MailAddress>();

			if (prevActivity == null) return result;

			if (prevActivity.IsIncome == true)
			{
				var mailFrom = prevActivity.MailFrom.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(mailFrom))
					result = EmailParser.ParseAddresses(mailFrom);
			}
			else
			{
				result = EmailParser.ParseAddresses(prevActivity.MailTo);
			}

			return result;
		}
		
		private static CRSMEmail GetParentMessage(PXGraph graph, CRSMEmail message)
		{
			if (message.Ticket == null) return null;
			return SelectActivity(graph, message.Ticket);
		}
		
		private static CRSMEmail GetPreviousExternalMessage(PXGraph graph, CRSMEmail message)
		{
			if (message.Ticket == null) return null;

			CRSMEmail prevActivity = SelectActivity(graph, message.Ticket);
			while (prevActivity != null && 
				prevActivity.ClassID == CRActivityClass.EmailRouting)
			{
				prevActivity = SelectParentActivity(graph, prevActivity.ParentNoteID);
			}
			return prevActivity;
		}

		private static CRSMEmail SelectActivity(PXGraph graph, int? id)
		{
			PXSelect<CRSMEmail,
				Where<CRSMEmail.id, Equal<Required<CRSMEmail.id>>>>.
				Clear(graph);

			var prevActivity = (CRSMEmail)PXSelect<CRSMEmail,
				Where<CRSMEmail.id, Equal<Required<CRSMEmail.id>>>>.
				Select(graph, id);

			return prevActivity;
		}

		private static CRSMEmail SelectParentActivity(PXGraph graph, Guid? noteID)
		{
			PXSelect<CRSMEmail,
				Where<CRSMEmail.noteID, Equal<Required<CRSMEmail.parentNoteID>>>>.
				Clear(graph);

			var prevActivity = (CRSMEmail)PXSelect<CRSMEmail,
				Where<CRSMEmail.noteID, Equal<Required<CRSMEmail.parentNoteID>>>>.
				Select(graph, noteID);

			return prevActivity;
		}
		
		private void MarkAsRoutingEmail(PXGraph graph, CRSMEmail message)
		{
			var cache = graph.Caches[message.GetType()];
			message.ClassID = CRActivityClass.EmailRouting;
			message.RefNoteID = null;
			message.BAccountID = null;
			message.ContactID = null;
			cache.Update(message);
		}

	    private void MarkAsRead(PXGraph graph, CRSMEmail message)
	    {
            EPView epview = PXSelect<EPView, Where<EPView.noteID, Equal<Required<CRSMEmail.noteID>>, And<EPView.userID, Equal<Required<CRSMEmail.ownerID>>>>>.
                Select(graph, message.NoteID, graph.Accessinfo.UserID);
            if (epview == null)
            {
                epview = new EPView
                {
                    NoteID = message.NoteID,
                    UserID = graph.Accessinfo.UserID,
                };
            }
            else
            {
                epview = PXCache<EPView>.CreateCopy(epview);
            }

	        if (epview.Status != EPViewStatusAttribute.VIEWED)
	        {
	            epview.Status = EPViewStatusAttribute.VIEWED;
	            PXCache epviewCache = graph.Caches[typeof (EPView)];
                epviewCache.Update(epview);
	        }
	    }

		//TODO: need optimizae DB requests
		internal static bool? IsFromInternalUser(PXGraph graph, CRSMEmail message)
		{
			var @from = EmailParser.ParseAddresses(message.MailFrom).FirstOrDefault().With(_ => _?.Address).With(_ => _?.Trim());

			PXSelect<Users,
				Where2<
					Where<Users.guest, Equal<False>, Or<Users.guest, IsNull>>,
					And<Users.email, Equal<Required<Users.email>>>>>
				.Clear(graph);

			var users = PXSelect<Users,
				Where2<
					Where<Users.guest, Equal<False>, Or<Users.guest, IsNull>>,
					And<Users.email, Equal<Required<Users.email>>>>>
				.Select(graph, @from);
			
			bool disabledUser = users.Count > 0 && users.RowCast<Users>().All(_ => _.State == Users.state.Disabled);

			if (disabledUser)
				return null;

			PXSelectJoin<EPEmployee,
				LeftJoin<Contact, 
					On<Contact.contactID, Equal<EPEmployee.defContactID>>>,
				Where<
					EPEmployee.userID, IsNotNull, 
					And<Contact.eMail, Equal<Required<Contact.eMail>>>>>
				.Clear(graph);

			var employees = PXSelectJoin<EPEmployee,
				LeftJoin<Contact, 
					On<Contact.contactID, Equal<EPEmployee.defContactID>>>,
				Where<
					EPEmployee.userID, IsNotNull, 
					And<Contact.eMail, Equal<Required<Contact.eMail>>>>>
				.Select(graph, @from);

			bool disabledEmployee = employees.Count > 0 && employees.RowCast<EPEmployee>().All(_ => _.Status == BAccount.status.Inactive || !(_.RouteEmails ?? false));

			if (disabledEmployee)
				return null;

			if (users.Count > 0 || employees.Count > 0)
				return true;

			return false;
		}

        internal static bool IsOwnerEqualUser(PXGraph graph, CRSMEmail message, Guid? owner)
        {
            var @from = EmailParser.ParseAddresses(message.MailFrom).FirstOrDefault().With(_ => _.Address).With(_ => _.Trim());
            PXSelect<Users,Where<Users.email, Equal<Required<Users.email>>>>.Clear(graph);
            var usersEmail = (Users)PXSelect<Users,Where<Users.email, Equal<Required<Users.email>>>>.Select(graph, @from);
            if (usersEmail != null)
                return usersEmail.PKID == owner;
            return false;
        }

		private void SendCopyMessageToOutside(PXGraph graph, EMailAccount account, CRSMEmail message, IEnumerable<MailAddress> addresses)
		{
			var cache = graph.Caches[message.GetType()];
			var copy = (CRSMEmail)cache.CreateCopy(message);
			copy.NoteID = null;
			copy.EmailNoteID = null;
			copy.IsIncome = false;
			copy.ParentNoteID = message.NoteID;
			MailAddress address;
			copy.MailFrom = EmailParser.TryParse(message.MailFrom, out address)
				? new MailAddress(account.Address, address.DisplayName).ToString()
				: account.Address;
            copy.MailTo = PXDBEmailAttribute.ToString(addresses); //TODO: need add address description
			copy.MailCc = null;
			copy.MailBcc = null;
			copy.MPStatus = MailStatusListAttribute.PreProcess;
			copy.ClassID = CRActivityClass.Email;
			var imcUid = Guid.NewGuid();
			copy.ImcUID = imcUid;
			copy.MessageId = GetType().Name + "_" + imcUid.ToString().Replace("-", string.Empty);						
			copy.IsPrivate = message.IsPrivate;
			copy.OwnerID = null;
            copy.ParentNoteID = null;

            copy = (CRSMEmail)cache.CreateCopy(cache.Insert(copy));

            //Update owner and reset owner if employee not found
            copy.OwnerID = message.OwnerID;
			try
			{
                copy = (CRSMEmail)cache.Update(copy);
            }
			catch (PXSetPropertyException)
			{
				copy.OwnerID = null;
                copy = (CRSMEmail)cache.Update(copy);
            }

            copy.ParentNoteID = message.NoteID;

            var noteFiles = PXNoteAttribute.GetFileNotes(cache, message);

            if (noteFiles != null)
				PXNoteAttribute.SetFileNotes(cache, copy, noteFiles);
		}
	}
}
