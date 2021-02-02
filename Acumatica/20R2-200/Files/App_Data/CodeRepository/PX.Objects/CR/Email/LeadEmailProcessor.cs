using System;
using PX.Data;
using PX.Objects.EP;



namespace PX.Objects.CR
{
	public class NewLeadEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true ||
				account.CreateLead != true)
			{
				return false;
			}

			var message = package.Message;
			if (!string.IsNullOrEmpty(message.Exception)
				|| message.IsIncome != true
				|| message.RefNoteID != null
				|| message.ClassID == CRActivityClass.EmailRouting)
				return false;

			var copy = package.Graph.Caches[typeof(CRSMEmail)].CreateCopy(message);
			try
			{
				LeadMaint graph = PXGraph.CreateInstance<LeadMaint>();
				var leadCache = graph.Lead.Cache;
				var lead = (CRLead)leadCache.Insert();
				lead = PXCache<CRLead>.CreateCopy(graph.Lead.Search<CRLead.contactID>(lead.ContactID));

				lead.EMail = package.Address;
				lead.LastName = package.Description;
				lead.RefContactID = message.ContactID;

				lead.OverrideRefContact = true;

				CREmailActivityMaint.EmailAddress address = CREmailActivityMaint.ParseNames(message.MailFrom);

				lead.FirstName = address.FirstName;
                lead.LastName = string.IsNullOrEmpty(address.LastName) ? address.Email : address.LastName;
				if (account.CreateLeadClassID != null)
					lead.ClassID = account.CreateLeadClassID;

				lead = (CRLead)leadCache.Update(lead);

				if (lead.ClassID != null)
				{
					CRLeadClass cls = PXSelect<
							CRLeadClass,
						Where<
							CRLeadClass.classID, Equal<Required<CRLeadClass.classID>>>>
						.SelectSingleBound(graph, null, lead.ClassID);

					if (cls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
					{
						lead.WorkgroupID = message.WorkgroupID;
						lead.OwnerID = message.OwnerID;
					}
				}

				message.RefNoteID = PXNoteAttribute.GetNoteID<CRLead.noteID>(leadCache, lead);
				graph.Actions.PressSave();
			}
			catch (Exception e)
			{
				package.Graph.Caches[typeof(CRSMEmail)].RestoreCopy(message, copy);
				throw new PXException(Messages.CreateLeadException, e is PXOuterException ? ("\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages)) : e.Message);
			}

			return true;
		}
	}
}