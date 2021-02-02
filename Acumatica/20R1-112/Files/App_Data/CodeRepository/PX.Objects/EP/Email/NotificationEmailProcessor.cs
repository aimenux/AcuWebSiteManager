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
	public class NotificationEmailProcessor : BaseRoutingProcessor
	{
		protected override bool Process(Package package)
		{
			PXGraph graph = package.Graph;
			CRSMEmail message = package.Message;

			if (message.IsIncome != true) return false;

			var isFromInternalUser = message.ClassID == CRActivityClass.EmailRouting;
			var recipients = new MailAddressList();
			if (isFromInternalUser)
				recipients.AddRange(GetFromInternal(graph, message));
			else
				recipients.AddRange(GetFromExternal(graph, message));
			
			RemoveAddress(recipients, message.MailFrom);
			RemoveAddress(recipients, message.MailTo);
			RemoveAddress(recipients, message.MailCc);
			RemoveAddress(recipients, message.MailBcc);
			
			if (recipients.Count == 0)
			{
				return false;
			}
			
			SendCopyMessageToInside(package.Graph, package.Account, message, recipients);

			graph.EnsureCachePersistence(message.GetType());

			return true;
		}

		private MailAddressList GetFromInternal(PXGraph graph, CRSMEmail message)
		{
			var routingMessage = GetRoutingMessage(graph, message);
			if (routingMessage == null) return null;
			
			var recipients = new MailAddressList();
			recipients.AddRange(GetOwnerAddressByNote(graph, routingMessage.RefNoteID, message.OwnerID));
			
			RemoveAddress(recipients, routingMessage.MailFrom);
			RemoveAddress(recipients, routingMessage.MailTo);
			RemoveAddress(recipients, routingMessage.MailCc);
			RemoveAddress(recipients, routingMessage.MailBcc);

			return recipients;
		}

		private MailAddressList GetFromExternal(PXGraph graph, CRSMEmail message)
		{
			var recipients = new MailAddressList();
			recipients.AddRange(GetOwnerAddressByNote(graph, message.RefNoteID, message.OwnerID));		

			var routingMessage = GetRoutingMessage(graph, message);
			if (routingMessage != null)
			{
				RemoveAddress(recipients, routingMessage.MailFrom);
				RemoveAddress(recipients, routingMessage.MailTo);
				RemoveAddress(recipients, routingMessage.MailCc);
				RemoveAddress(recipients, routingMessage.MailBcc);
			}

			return recipients;
		}
		
		private CRSMEmail GetRoutingMessage(PXGraph graph, CRSMEmail message)
		{
			return PXSelect<CRSMEmail,
				Where<CRSMEmail.parentNoteID, Equal<Required<CRSMEmail.parentNoteID>>>>
				.SelectWindowed(graph, 0, 1, message.NoteID);
		}
	}

	public abstract class BaseRoutingProcessor : BasicEmailProcessor
	{
		protected class MailAddressList : IEnumerable<MailAddress>
		{
			private readonly HybridDictionary _items = new HybridDictionary();

			public void AddRange(IEnumerable<MailAddress> addresses)
			{
				if (addresses != null)
					foreach (MailAddress address in addresses)
						Add(address);
			}

			public void Add(MailAddress address)
			{
				var key = address.Address.With(_ => _.Trim()).With(_ => _.ToLower());
				if (!string.IsNullOrEmpty(key) && !_items.Contains(key)) 
					_items.Add(key, address);
			}

			public void Remove(MailAddress address)
			{
				var key = address.Address.With(_ => _.Trim()).With(_ => _.ToLower());
				if (_items.Contains(key))
					_items.Remove(key);
			}

			public IEnumerator<MailAddress> GetEnumerator()
			{
				foreach (DictionaryEntry item in _items)
					yield return (MailAddress)item.Value;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public int Count
			{
				get { return _items.Count; }
			}
		}

		protected List<MailAddress> GenerateAddress(Contact employeeContact, Users user)
		{
			string displayName = null;
			string address = null;
			if (user != null && user.PKID != null)
			{
				var userDisplayName = user.FullName.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(userDisplayName))
					displayName = userDisplayName;
				var userAddress = user.Email.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(userAddress))
					address = userAddress;
			}
			if (employeeContact != null && employeeContact.BAccountID != null)
			{
				var employeeDisplayName = employeeContact.DisplayName.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(employeeDisplayName))
					displayName = employeeDisplayName;
				var employeeAddress = employeeContact.EMail.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(employeeAddress))
					address = employeeAddress;
			}
			return string.IsNullOrEmpty(address)
				? null
				: EmailParser.ParseAddresses(
					PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(address, displayName)
				);
		}
		
		protected void FindOwner(PXGraph graph, IAssign source, out Contact contact, out Users user, out EPEmployee employee)
		{
			contact = null;
			user = null;
			employee = null;
			if (source == null || source.OwnerID == null) return;

			FindOwner(graph, source.OwnerID, out contact, out user, out employee);
		}

		protected void FindOwner(PXGraph graph, Guid? ownerId, out Contact contact, out Users user, out EPEmployee employee)
		{
			contact = null;
			user = null;
			employee = null;
			if (ownerId == null) return;

			PXSelectJoin<Users,
				LeftJoin<EPEmployee, 
					On<EPEmployee.userID, Equal<Users.pKID>>,
				LeftJoin<Contact, 
					On<Contact.contactID, Equal<EPEmployee.defContactID>>>>,
				Where<Users.pKID, Equal<Required<Users.pKID>>>>
				.Clear(graph);

			var row = (PXResult<Users, EPEmployee, Contact>)PXSelectJoin<Users,
				LeftJoin<EPEmployee, 
					On<EPEmployee.userID, Equal<Users.pKID>>,
				LeftJoin<Contact, 
					On<Contact.contactID, Equal<EPEmployee.defContactID>>>>,
				Where<Users.pKID, Equal<Required<Users.pKID>>>>
				.Select(graph, ownerId);

			contact = (Contact)row;
			user = (Users)row;
			employee = (EPEmployee)row;
		}
		
		protected List<MailAddress> GetOwnerAddressByNote(PXGraph graph, Guid? noteId, Guid? mainOwner = null)
		{
			if (noteId == null) return null;

			var source = FindSource(graph, noteId.Value);
			if (source == null) return null;

			Contact owner;
			Users user;
			EPEmployee employee;
			FindOwner(graph, source as IAssign, out owner, out user, out employee);

			if (user == null) 
				return null;
			if (mainOwner != null && mainOwner == user.PKID) 
				return null;
			if (user?.State == Users.state.Disabled
				|| employee?.Status == BAccount.status.Inactive) 
				return null;

			return GenerateAddress(owner, user);
		}

		protected object FindSource(PXGraph graph, Guid noteId)
		{
			return new EntityHelper(graph).GetEntityRow(noteId);
		}
		
		protected void RemoveAddress(MailAddressList recipients, string addrStr)
		{
			if (string.IsNullOrEmpty(addrStr) || recipients.Count == 0) return;

			var addresses = EmailParser.ParseAddresses(addrStr);
			foreach (MailAddress address in addresses)
			{
				recipients.Remove(address);
			}
		}

		protected void SendCopyMessageToInside(PXGraph graph, EMailAccount account, CRSMEmail message, IEnumerable<MailAddress> addresses)
		{
			var cache = graph.Caches[message.GetType()];
			var copy = (CRSMEmail)cache.CreateCopy(message);

			copy.NoteID = null;
			copy.EmailNoteID = null;
			copy.IsIncome = false;
			copy.ParentNoteID = message.NoteID;
			MailAddress address = null;
			copy.MailFrom = EmailParser.TryParse(message.MailFrom, out address)
				? new MailAddress(account.Address, address.DisplayName).ToString()
				: account.Address;
			copy.MailTo = PXDBEmailAttribute.ToString(addresses); //TODO: need add address description
			copy.MailCc = null;
			copy.MailBcc = null;
			copy.MailReply = copy.MailFrom;
			copy.MPStatus = MailStatusListAttribute.PreProcess;
			copy.ClassID = CRActivityClass.EmailRouting;
			new AddInfoEmailProcessor().Process(new EmailProcessEventArgs(graph, account, copy));
			copy.RefNoteID = null;
			copy.BAccountID = null;
			copy.ContactID = null;
			copy.Pop3UID = null;
			copy.ImapUID = null;
			var imcUid = Guid.NewGuid();
			copy.ImcUID = imcUid;
			copy.MessageId = this.GetType().Name + "_" + imcUid.ToString().Replace("-", string.Empty);
			copy.OwnerID = null;
			copy.WorkgroupID = null;

			copy = (CRSMEmail)cache.CreateCopy(cache.Insert(copy));

			//Update owner and reset owner if employee not found
			copy.OwnerID = message.OwnerID;
			try
			{
				cache.Update(copy);
			}
			catch (PXSetPropertyException)
			{
				copy.OwnerID = null;
				copy =  (CRSMEmail)cache.CreateCopy(cache.Update(copy));
			}
			
			copy.IsPrivate = message.IsPrivate;
			copy.WorkgroupID = message.WorkgroupID;

			var noteFiles = PXNoteAttribute.GetFileNotes(cache, message);

			if (noteFiles != null)
				PXNoteAttribute.SetFileNotes(cache, copy, noteFiles);
		}
	}
}
