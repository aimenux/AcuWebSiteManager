using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Text;
using PX.Data;
using PX.Data.Update;
using PX.Data.Update.WebServices;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.CS.Email
{
	#region PXEmailExchangeHelper
	public static class PXEmailSyncHelper
	{
		private class Definition : IPrefetchable
		{
			public List<int> Exchanges;

			public void Prefetch()
			{
				Exchanges = new List<int>();

				foreach (PXDataRecord rec in PXDatabase.SelectMulti<EMailAccount>(
					new PXDataField<EMailAccount.emailAccountID>( ), 
					new PXDataFieldValue<EMailAccount.emailAccountType>(EmailAccountTypesAttribute.Exchange)))
				{
					int? id = rec.GetInt32(0);
					if (id.HasValue) Exchanges.Add(id.Value);
				}
			}
		}

		public static Dictionary<string, Type> _exchangers = new Dictionary<string, Type>();

		static PXEmailSyncHelper()
		{
			_exchangers.Add(EmailSyncServerTypesAttribute.Exchange, typeof(MicrosoftExchangeSyncProvider));
		}

		public static bool IsExchange(int emailAccountID)
		{
			Definition def = PXDatabase.GetSlot<Definition>("EmailExchangeAccounts", typeof (EMailAccount));
			return def.Exchanges != null && def.Exchanges.Contains(emailAccountID);
		}

		public static IEmailSyncProvider GetExchanger(int emailAccountID)
		{
			Tuple<EMailSyncServer, EMailSyncPolicy, PXSyncMailbox> tuple = GetConfig(emailAccountID);
			return GetExchanger(tuple.Item1, tuple.Item2);
		}
		public static IEmailSyncProvider GetExchanger(EMailSyncServer server, EMailSyncPolicy policy)
		{
			if (server == null || String.IsNullOrEmpty(server.ServerType) || !_exchangers.ContainsKey(server.ServerType))
				throw new PXException(Messages.EmailExchangeProviderNotFound);

			IEmailSyncProvider prov = (IEmailSyncProvider)Activator.CreateInstance(_exchangers[server.ServerType], server, policy);
			return prov;
		}

		public static void SendMessage(CRSMEmail message)
		{
			if (message == null || message.MailAccountID == null) 
				throw new PXException(ErrorMessages.EmailNotConfigured);

			Tuple<EMailSyncServer, EMailSyncPolicy, PXSyncMailbox> tuple = GetConfig(message.MailAccountID.Value);
			IEmailSyncProvider prov = GetExchanger(tuple.Item1, tuple.Item2);

			try
			{
				prov.SendMessage(tuple.Item3, new[] { message });
				message.Exception = null;
			}
			catch (Exception ex)
			{
				message.Exception = ex.Message;
			}
		}

		private static Tuple<EMailSyncServer, EMailSyncPolicy, PXSyncMailbox> GetConfig(int emailAccountID)
		{
			PXGraph graph = new PXGraph();

			foreach (PXResult<EMailSyncAccount, EMailSyncServer, EMailAccount, EPEmployee, Contact> row in
					PXSelectJoin<EMailSyncAccount,
						InnerJoin<EMailSyncServer, On<EMailSyncServer.accountID, Equal<EMailSyncAccount.serverID>>,
						InnerJoin<EMailAccount, On<EMailAccount.emailAccountID, Equal<EMailSyncAccount.emailAccountID>>,
						LeftJoin<EPEmployee, On<EMailSyncAccount.employeeID, Equal<EPEmployee.bAccountID>>,
						LeftJoin<Contact, On<EPEmployee.defContactID, Equal<Contact.contactID>, And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>>>>,
					Where<EMailSyncAccount.emailAccountID, Equal<Required<EMailSyncAccount.emailAccountID>>>,
					OrderBy<Asc<EMailSyncAccount.serverID, Asc<EMailSyncAccount.employeeID>>>>.SelectSingleBound(graph, null, emailAccountID))
			{
				EMailSyncServer server = row;
				EMailSyncAccount account = row;
				Contact contact = row;
				EMailAccount eMailAccount = row;

				if (server == null || account == null || String.IsNullOrEmpty(account.Address)) throw new PXException(Messages.EmailExchangeAccountNotFound);
				if (server.IsActive != true) throw new PXException(Messages.EmailExchangeAccountNotEnabled);

				string address = (contact != null ? contact.EMail : null) ?? account.Address;
				
				PXSyncMailbox mailbox = new PXSyncMailbox(address, account.EmployeeID.Value, emailAccountID, new PXSyncMailboxPreset(null, null), new PXSyncMailboxPreset(null, null), eMailAccount.IncomingProcessing ?? false);

				string policyName = account.PolicyName ?? server.DefaultPolicyName;
				EMailSyncPolicy policy = PXSelect<EMailSyncPolicy, Where<EMailSyncPolicy.policyName, Equal<Required<EMailSyncPolicy.policyName>>>>.SelectSingleBound(graph, null, policyName);
				if (policy == null) throw new PXException(Messages.EmailExchangePolicyNotFound, account.Address);

				if (String.IsNullOrEmpty(server.ServerType) || !_exchangers.ContainsKey(server.ServerType))
					throw new PXException(Messages.EmailExchangeProviderNotFound);

				return Tuple.Create(server, policy, mailbox);
			}

			throw new PXException(Messages.EmailExchangeAccountNotFound);
		}
	}
	#endregion
	#region IEmailSyncProvider
	public interface IEmailSyncProvider : IDisposable
	{
		bool AllowContactsSync { get; }
		bool AllowTasksSync { get; }
		bool AllowEventsSync { get; }
		bool AllowEmailsSync { get; }

		void ContactsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes);
		void TasksSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes);
		void EventsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes);
		void EmailsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes);

		void SendMessage(PXSyncMailbox mailbox, IEnumerable<CRSMEmail> activities);
		IEnumerable<CRSMEmail> ReceiveMessage(PXSyncMailbox mailbox);
	}
	#endregion

	#region BaseEmailExchangeProvider
	public abstract class BaseExchangeSyncProvider : IDisposable
	{
		public readonly EMailSyncServer Account;
		public readonly EMailSyncPolicy Policy;
		public readonly PXSyncCache Cache;

		public PXExchangeEventDelegate Logger;

		#region Logging
		public void LogVerbose(string mailbox, string message, params Object[] args)
		{
			LogEvent(new PXExchangeEvent(mailbox, EventLevel.Verbose, String.Format(message, args), null));
		}
		public void LogInfo(string mailbox, string message, params Object[] args)
		{
			LogEvent(new PXExchangeEvent(mailbox, EventLevel.Informational, String.Format(message, args), null));
		}
		public void LogWarning(string mailbox, string message, params Object[] args)
		{
			LogEvent(new PXExchangeEvent(mailbox, EventLevel.Warning, String.Format(message, args), null));
		}
		public void LogError(string mailbox, Exception error, string message = null)
		{
			LogEvent(new PXExchangeEvent(mailbox, EventLevel.Error, message, error));
		}

		public void LogResult(PXSyncResult result)
		{
			string action = null;
			if (!String.IsNullOrEmpty(result.ActionTitle)) action = result.ActionTitle;
			else if (result.ItemStatus != PXSyncItemStatus.None) action = result.ItemStatus.ToString( );
			else action = "Processing";

			if (result.Success)
			{
				string text = PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncSuccessful, result.Direction.ToString(), result.OperationTitle, result.Address, result.DisplayKey, action);
				LogEvent(new PXExchangeEvent(result.Address, EventLevel.Informational, text, result.Error) { Date = result.Date });
			}
			else
			{
				string text = PXMessages.LocalizeFormatNoPrefix(Messages.EmailExchangeSyncError, result.Direction.ToString(), result.OperationTitle, result.Address, result.DisplayKey, action);
				text = CreateErrorMessage(true, text, result.Message, result.Error, result.Details);
				LogEvent(new PXExchangeEvent(result.Address, EventLevel.Error, text, result.Error) { Date = result.Date } );
			}
		}
		public void LogEvent(PXExchangeEvent occasion)
		{
			bool needLog = false;
			switch (occasion.Level)
			{
				case EventLevel.Critical:
				case EventLevel.Error:
				case EventLevel.Warning:
					if (Account.LoggingLevel != null && Account.LoggingLevel != EMailSyncServer.LogLevel.None)
						needLog = true;
					break;
				case EventLevel.Informational:
					if (Account.LoggingLevel == EMailSyncServer.LogLevel.Informational || Account.LoggingLevel == EMailSyncServer.LogLevel.Verbose)
						needLog = true;
					break;
				case EventLevel.LogAlways:
				case EventLevel.Verbose:
					if (Account.LoggingLevel == EMailSyncServer.LogLevel.Verbose)
						needLog = true;
					break;
			}

			if (needLog)
			{
				if (Logger != null) 
					Logger(occasion);

				SaveEvent(occasion);
			}
		}

		public string CreateErrorMessage(bool detailed, string text, string message, Exception error, string[] details)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(text);
			if (message != null) sb.AppendLine(message);

			if (error != null)
			{
				var errorText = error.ToString();

				errorText = PXExchangeServer.MapErrors(errorText);

				sb.AppendLine(detailed ? errorText : error.Message);
			}

			if (details != null)
			{
				sb.AppendLine();
				foreach (string str in details)
				{
					sb.AppendLine(str);
				}
			}
			if (error is PXOuterException)
			{
				PXOuterException outer = error as PXOuterException;
				foreach (string inner in outer.InnerMessages)
				{
					sb.AppendLine(inner);
				}
			}

			return sb.ToString();
		}
		
		protected void SaveEvent(PXExchangeEvent occasion)
		{
			if (occasion == null) return;

			PXDatabase.Insert<EMailSyncLog>(
				new PXDataFieldAssign<EMailSyncLog.serverID>(Account.AccountID),
				new PXDataFieldAssign<EMailSyncLog.address>(occasion.Address),
				new PXDataFieldAssign<EMailSyncLog.level>((byte)occasion.Level),
				new PXDataFieldAssign<EMailSyncLog.date>(occasion.Date),
				new PXDataFieldAssign<EMailSyncLog.message>(occasion.Message),
				new PXDataFieldAssign<EMailSyncLog.details>(occasion.Details == null ? null : String.Join(Environment.NewLine, occasion.Details)));

		}

		#region Flushing
		//protected List<PXExchangeEvent> log = new List<PXExchangeEvent>();
		//protected void FlushLog()
		//{
		//	if (log == null || log.Count <= 0) return;
		//	foreach (PXExchangeEvent occasion in log)
		//	{
		//		SaveEvent(occasion);
		//	}
		//	log = new List<PXExchangeEvent>();
		//}
		#endregion
		#endregion

		protected BaseExchangeSyncProvider(EMailSyncServer account, EMailSyncPolicy policy)
		{
			if (account == null) throw new ArgumentNullException("account");
			if (policy == null) throw new ArgumentNullException("policy");

			Account = account;
			Policy = policy;

			Cache = new PXSyncCache();
			LogVerbose(null, Messages.EmailExchangeProviderInitialised, Policy.PolicyName);
		}
		public void Dispose()
		{
			//FlushLog();
		}

		#region Interface
		public bool AllowContactsSync
		{
			get { return IsMethodOverride("ContactsSync"); }
		}
		public bool AllowTasksSync
		{
			get { return IsMethodOverride("TasksSync"); }
		}
		public bool AllowEventsSync
		{
			get { return IsMethodOverride("EventsSync"); }
		}
		public bool AllowEmailsSync
		{
			get { return IsMethodOverride("EmailsSync"); }
		}

		[MethodDisabled]
		public virtual void ContactsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			throw new PXException(Messages.EmailExchangeMethodNotSupported);
		}
		[MethodDisabled]
		public virtual void TasksSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			throw new PXException(Messages.EmailExchangeMethodNotSupported);
		}
		[MethodDisabled]
		public virtual void EventsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			throw new PXException(Messages.EmailExchangeMethodNotSupported);
		}
		[MethodDisabled]
		public virtual void EmailsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			throw new PXException(Messages.EmailExchangeMethodNotSupported);
		}
		#endregion

		#region Support
		protected bool IsMethodOverride(string name)
		{
			//getting method
			foreach (MethodInfo mi in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (mi == null || mi.Name != name) continue;

				//getting MethodEnabledAttribute 
				Object[] attrs = mi.GetCustomAttributes(typeof(MethodDisabledAttribute), false);
				if (attrs.Length <= 0) return true;
			}

			return false;
		}
		#endregion
	}
	#endregion
	#region MicrosoftExchangeProvider
	public class MicrosoftExchangeSyncProvider : BaseExchangeSyncProvider, IEmailSyncProvider
	{
		protected PXExchangeServer _gate;
		public PXExchangeServer Gate
		{
			get
			{
				if (_gate == null)
				{
					_gate = PXExchangeServer.GetGate(Account);
					_gate.Logger += LogEvent;
				}
				return _gate;
			}
		}

		public MicrosoftExchangeSyncProvider(EMailSyncServer server, EMailSyncPolicy policy) : base(server, policy) { }

		public override void ContactsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			using(ExchangeBaseSyncCommand cmd = new ExchangeContactsSyncCommand(this))
			{
				cmd.ProcessSync(policy, direction, mailboxes);
			}
		}
		public override void TasksSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			using (ExchangeBaseSyncCommand cmd = new ExchangeTasksSyncCommand(this))
			{
				cmd.ProcessSync(policy, direction, mailboxes);
			}
		}
		public override void EventsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			using (ExchangeBaseSyncCommand cmd = new ExchangeEventsSyncCommand(this))
			{
				cmd.ProcessSync(policy, direction, mailboxes);
			}
		}
		public override void EmailsSync(EMailSyncPolicy policy, PXEmailSyncDirection.Directions direction, IEnumerable<PXSyncMailbox> mailboxes)
		{
			using (ExchangeBaseSyncCommand cmd = new ExchangeEmailsSyncCommand(this))
			{
				cmd.ProcessSync(policy, direction, mailboxes);
			}
		}

		public void SendMessage(PXSyncMailbox mailbox, IEnumerable<CRSMEmail> activities)
		{
			using (ExchangeEmailsSyncCommand cmd = new ExchangeEmailsSyncCommand(this))
			{
				cmd.SendMessage(mailbox, activities);
			}
		}
		public IEnumerable<CRSMEmail> ReceiveMessage(PXSyncMailbox mailbox)
		{
			using (ExchangeEmailsSyncCommand cmd = new ExchangeEmailsSyncCommand(this))
			{
				throw new NotImplementedException();
			}
		}
	}
	#endregion
}