using System;
using System.Linq;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.EP;
using PX.Data.EP;
using PX.Common.Mail;

namespace PX.Objects.CR
{
	public class ContactBAccountEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true || 
				account.CreateActivity != true)
			{
				return false;
			}
		    PXGraph graph = package.Graph;
			var message = package.Message;

            if (!string.IsNullOrEmpty(message.Exception) || message.RefNoteID != null) return false;

			List<String> addressesList = new List<String>( );
			addressesList.Add(package.Address);
			if (package.Message.IsIncome == false && package.Message.MailTo != null && package.Account.EmailAccountType == EmailAccountTypesAttribute.Exchange)
				addressesList.InsertRange(0, EmailParser.ParseAddresses(message.MailTo).Select(m => m.Address));

			foreach (String address in addressesList)
			{
				PXSelect<Contact,
					Where<
						Contact.eMail, Contains<Required<Contact.eMail>>,
						And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>
					.Clear(package.Graph);

				var contact = (Contact)PXSelect<Contact,
						Where<
							Contact.eMail, Contains<Required<Contact.eMail>>,
							And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>
					.SelectWindowed(package.Graph, 0, 1, address);

				if (contact != null && contact.ContactID != null)
				{
					graph.EnsureCachePersistence(typeof (Contact));
					graph.EnsureCachePersistence(typeof (BAccount));

					message.ContactID = contact.ContactID;

					PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
						Clear(package.Graph);

					BAccount baCcount = PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
						Select(package.Graph, contact.BAccountID);

					if (baCcount != null)
						message.BAccountID = baCcount.BAccountID;

					return true;
				}
			}
			return false;
		}
	}
}
