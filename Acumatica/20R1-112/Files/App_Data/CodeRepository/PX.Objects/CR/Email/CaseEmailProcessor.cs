using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;
using System.Linq;
using System.Net.Mail;
using PX.Common;

namespace PX.Objects.CR
{
	public class NewCaseEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true ||
			    account.CreateCase != true )
			{
				return false;
			}

			var message = package.Message;

			if (!string.IsNullOrEmpty(message.Exception)
			    || message.IsIncome != true
			    || message.RefNoteID != null
			    || message.ClassID == CRActivityClass.EmailRouting)
				return false;


            var graph = PXGraph.CreateInstance<CRCaseMaint>();
			SetCRSetup(graph);
			SetAccessInfo(graph);
		    var copy = package.Graph.Caches[typeof(CRSMEmail)].CreateCopy(message);
		    try
		    {
                var caseCache = graph.Caches[typeof(CRCase)];
                var @case = (CRCase)caseCache.Insert();
			    @case = graph.Case.Search<CRCase.caseCD>(@case.CaseCD);
		        @case = PXCache<CRCase>.CreateCopy(@case);
                //@case.EMail = package.Address;
                @case.Subject = message.Subject;
                if (@case.Subject == null || @case.Subject.Trim().Length == 0)
                    @case.Subject = GetFromString(package.Address, package.Description);
                @case.Description = message.Body;

                if (account.CreateCaseClassID != null)
                    @case.CaseClassID = account.CreateCaseClassID;

                CRCaseClass caseClass = PXSelect<CRCaseClass, Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>.SelectSingleBound(graph, null, @case.CaseClassID);
                if (caseClass == null)
                    return false;

                @case = PXCache<CRCase>.CreateCopy((CRCase)caseCache.Update(@case));
                BAccount baccount = null;
                var contact = FindContact(graph, package.Address);
		        if (contact != null)
		        {
		            @case.ContactID = contact.ContactID;
		            message.ContactID = contact.ContactID;
					baccount = FindAccount(graph, contact);
				}
                else if (caseClass.RequireContact == true)
                    return false;

                if (baccount == null && contact == null)
				{
					baccount = FindAccount(graph, package.Address);
				}

		        if (baccount != null)
		        {
		            PXCache cache = graph.Caches[typeof(BAccount)];
		            graph.EnsureCachePersistence(cache.GetItemType());
			        message.BAccountID = baccount.BAccountID;
					@case.CustomerID = baccount.BAccountID;		           
		        }
				else if (caseClass.RequireCustomer == true)
					return false;

                message.RefNoteID = PXNoteAttribute.GetNoteID<CRCase.noteID>(graph.Caches[typeof(CRCase)], @case);               		        
                caseCache.Update(@case);  
		        graph.Activities.Cache.Current = message;
				graph.Save.PressImpl(false);
		    }
		    catch (Exception e)
		    {
                package.Graph.Caches[typeof(CRSMEmail)].RestoreCopy(message, copy);
		        throw new PXException(Messages.CreateCaseException, e is PXOuterException ? ("\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages)) : e.Message);
		    }
			return true;
		}

		private static string GetFromString(string address, string description)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(Messages.SenderNameAndEmail, description, address);
		}

		private void SetAccessInfo(PXGraph graph)
		{
			graph.Caches[typeof(AccessInfo)].Current = graph.Accessinfo;
		}

		private void SetCRSetup(PXGraph graph)
		{
			var crSetupCache = graph.Caches[typeof(CRSetup)];
			crSetupCache.Current = (CRSetup)PXSelect<CRSetup>.SelectWindowed(graph, 0, 1);
		}

		private BAccount FindAccount(PXGraph graph, Contact contact)
		{
			if (contact == null || contact.BAccountID == null) return null;

			PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>,
                   And<Where<BAccount.type, Equal<BAccountType.prospectType>,
                           Or<BAccount.type, Equal<BAccountType.customerType>,
                           Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>.
				Clear(graph);

            var account = (BAccount)
                PXSelect<BAccount,
                Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>,
                   And<Where<BAccount.type, Equal<BAccountType.prospectType>,
                           Or<BAccount.type, Equal<BAccountType.customerType>,
                           Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>.
										Select(graph, contact.BAccountID);
			return account;
		}

		private BAccount FindAccount(PXGraph graph, string address)
        {
            if (string.IsNullOrEmpty(address)) return null;

            var accounts = 
                PXSelectJoin<BAccount,
                InnerJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>, And<Contact.contactID, Equal<BAccount.defContactID>>>>,
                     Where<Contact.eMail, Like<Required<Contact.eMail>>,
                   And<Where<BAccount.type, Equal<BAccountType.prospectType>,
                           Or<BAccount.type, Equal<BAccountType.customerType>,
                           Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>.
                                        Select(graph, string.Concat("%", address, "%"));
            foreach(PXResult<BAccount, Contact> account in accounts)
            {
                var contact = (Contact)account;
                List<MailAddress> addressesList = PX.Common.Mail.EmailParser.ParseAddresses(contact.EMail);
                if (addressesList.Select(a => a.Address).Contains(address))
                    return (BAccount)account;
            }
            return null;
        }

		private Contact FindContact(PXGraph graph, string address)
		{
			PXSelect<Contact,
				Where<Contact.eMail, Equal<Required<Contact.eMail>>, And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>.
				Clear(graph);
            var contact = (Contact)PXSelect<Contact,
                Where<Contact.eMail, Equal<Required<Contact.eMail>>, And<Contact.contactType, Equal<ContactTypesAttribute.person>>>,
										OrderBy<Desc<Contact.isActive>>>.SelectWindowed(graph, 0, 1, address);
			return contact;
		}
	}

	public class CaseCommonEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true)
			{
				return false;
			}

			var message = package.Message;
			if (message.IsIncome != true) return false;
			if (message.RefNoteID == null) return false;


			var graph = package.Graph;

			PXSelect<CRCase,
				Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.
				Clear(graph);

			var @case = (CRCase)PXSelect<CRCase,
				Where<CRCase.noteID, Equal<Required<CRCase.noteID>>>>.
				Select(graph, message.RefNoteID);

			if (@case == null || @case.CaseCD == null) return false;
			if (@case != null && message.OwnerID == null && account.EmailAccountType == PX.SM.EmailAccountTypesAttribute.Standard)
			{
				try
				{
					message.WorkgroupID = @case.WorkgroupID;
					graph.Caches[typeof(CRSMEmail)].SetValueExt<CRSMEmail.ownerID>(message, @case.OwnerID);
				}
				catch (PXSetPropertyException)
				{
					message.OwnerID = null;
				}
			}

			CRCaseClass caseClass = new PXSelect<CRCaseClass,Where<CRCaseClass.caseClassID, Equal<Required<CRCaseClass.caseClassID>>>>(graph)
										.SelectWindowed(0, 1, @case.CaseClassID);


			if (!RouterEmailProcessor.IsOwnerEqualUser(graph, message, @case.OwnerID)
					&& @case.Released != true
					&& (@case.IsActive == true || CaseShouldBeReopenedByDate()))
			{
				var caseMaint = PXGraph.CreateInstance<CRCaseMaint>();
				caseMaint.Case.Current = caseMaint.Case.Cache.CreateCopy(@case) as CRCase;
				caseMaint.GetExtension<Workflows.CaseWorkflow>().openCaseFromProcessing.Press();
			}
			
			return true;

			bool CaseShouldBeReopenedByDate()
			{
				return @case.ResolutionDate is DateTime resolutionDate
					&& caseClass?.ReopenCaseTimeInDays is int reopenDays && reopenDays > 0
					? PX.Common.PXTimeZoneInfo.Now - resolutionDate < TimeSpan.FromDays(reopenDays)
					: false;
			}
		}
	}
}
