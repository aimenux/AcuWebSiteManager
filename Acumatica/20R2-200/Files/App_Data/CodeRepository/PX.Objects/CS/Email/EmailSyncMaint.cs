using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Data.Update.WebServices;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.CS.Email
{
	public class EmailsSyncMaint : PXGraph<EmailsSyncMaint>
	{
		protected static readonly Guid SUID = Guid.NewGuid();
		
		#region Actions
		public PXCancel<EMailAccountSyncFilter> Cancel;

		public PXAction<EMailAccountSyncFilter> ResetContacts;
		[PXButton]
		[PXUIField(DisplayName = "Reset", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected IEnumerable resetContacts(PXAdapter adapter)
		{
			EMailAccountSyncFilter filter = Filter.Current;
			EMailSyncAccount account = CurrentItem.Current;

			if (filter == null || account == null) return adapter.Get();
			
			PXLongOperation.StartOperation(this, delegate()
			{
				foreach (var result in ContactsReferences
					.Select(account.ContactsExportDate ?? new DateTime(1900, 1, 1))
					.RowCast<EMailSyncReference>())
				{
					Caches[typeof(EMailSyncReference)].Delete(result);
				}
				Caches[typeof(EMailSyncReference)].Persist(PXDBOperation.Delete);
				
				EMailSyncAccount accountCopy = (EMailSyncAccount) CurrentItem.Cache.CreateCopy(account);
				
				ToDefault(accountCopy);

				accountCopy.ContactsExportDate = account.ContactsExportDate;
				accountCopy.ContactsExportFolder = null;
				accountCopy.ContactsImportDate = account.ContactsImportDate;
				accountCopy.ContactsImportFolder = null;
				accountCopy.IsReset = true;

				Caches[typeof(EMailSyncAccount)].Update(accountCopy);
				Caches[typeof(EMailSyncAccount)].Persist(PXDBOperation.Update);
			});

			ClearViews();

			return adapter.Get();
		}

		public PXAction<EMailAccountSyncFilter> ResetTasks;
		[PXButton]
		[PXUIField(DisplayName = "Reset", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected IEnumerable resetTasks(PXAdapter adapter)
		{
			EMailAccountSyncFilter filter = Filter.Current;
			EMailSyncAccount account = CurrentItem.Current;

			if (filter == null || account == null) return adapter.Get();
			
			PXLongOperation.StartOperation(this, delegate()
			{
				foreach (var result in TasksReferences
					.Select(account.TasksExportDate ?? new DateTime(1900, 1, 1))
					.RowCast<EMailSyncReference>())
				{
					Caches[typeof(EMailSyncReference)].Delete(result);
				}
				Caches[typeof(EMailSyncReference)].Persist(PXDBOperation.Delete);

				EMailSyncAccount accountCopy = (EMailSyncAccount) CurrentItem.Cache.CreateCopy(account);

				ToDefault(accountCopy);

				accountCopy.TasksExportDate = account.TasksExportDate;
				accountCopy.TasksExportFolder = null;
				accountCopy.TasksImportDate = account.TasksImportDate;
				accountCopy.TasksImportFolder = null;
				accountCopy.IsReset = true;

				Caches[typeof(EMailSyncAccount)].Update(accountCopy);
				Caches[typeof(EMailSyncAccount)].Persist(PXDBOperation.Update);
			});

			ClearViews();

			return adapter.Get();
		}

		public PXAction<EMailAccountSyncFilter> ResetEvents;
		[PXButton]
		[PXUIField(DisplayName = "Reset", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected IEnumerable resetEvents(PXAdapter adapter)
		{
			EMailAccountSyncFilter filter = Filter.Current;
			EMailSyncAccount account = CurrentItem.Current;

			if (filter == null || account == null) return adapter.Get();
			
			PXLongOperation.StartOperation(this, delegate()
			{
				foreach (var result in EventsReferences
					.Select(account.EventsExportDate ?? new DateTime(1900, 1, 1))
					.RowCast<EMailSyncReference>())
				{
					Caches[typeof(EMailSyncReference)].Delete(result);
				}
				Caches[typeof(EMailSyncReference)].Persist(PXDBOperation.Delete);

				EMailSyncAccount accountCopy = (EMailSyncAccount) CurrentItem.Cache.CreateCopy(account);

				ToDefault(accountCopy);

				accountCopy.EventsExportDate = account.EventsExportDate;
				accountCopy.EventsExportFolder = null;
				accountCopy.EventsImportDate = account.EventsImportDate;
				accountCopy.EventsImportFolder = null;
				accountCopy.IsReset = true;

				Caches[typeof(EMailSyncAccount)].Update(accountCopy);
				Caches[typeof(EMailSyncAccount)].Persist(PXDBOperation.Update);
			});

			ClearViews();

			return adapter.Get();
		}
		
		public PXAction<EMailAccountSyncFilter> ResetEmails;
		[PXButton]
		[PXUIField(DisplayName = "Reset", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected IEnumerable resetEmails(PXAdapter adapter)
		{
			EMailAccountSyncFilter filter = Filter.Current;
			EMailSyncAccount account = CurrentItem.Current;

			if (filter == null || account == null) return adapter.Get();
			
			PXLongOperation.StartOperation(this, delegate()
			{
				foreach (var result in EventsReferences
					.Select(account.EmailsExportDate ?? new DateTime(1900, 1, 1))
					.RowCast<EMailSyncReference>())
				{
					Caches[typeof(EMailSyncReference)].Delete(result);
				}
				Caches[typeof(EMailSyncReference)].Persist(PXDBOperation.Delete);

				EMailSyncAccount accountCopy = (EMailSyncAccount) CurrentItem.Cache.CreateCopy(account);

				ToDefault(accountCopy);

				accountCopy.EmailsExportDate = account.EmailsExportDate;
				accountCopy.EmailsExportFolder = null;
				accountCopy.EmailsImportDate = account.EmailsImportDate;
				accountCopy.EmailsImportFolder = null;
				accountCopy.IsReset = true;

				Caches[typeof(EMailSyncAccount)].Update(accountCopy);
				Caches[typeof(EMailSyncAccount)].Persist(PXDBOperation.Update);
			});
			
			ClearViews();

			return adapter.Get();
		}

		private void ToDefault(EMailSyncAccount row)
		{
			foreach (var field in new[]
			{
				typeof(EMailSyncAccount.contactsExportDate),
				typeof(EMailSyncAccount.contactsImportDate),
				typeof(EMailSyncAccount.tasksExportDate),
				typeof(EMailSyncAccount.tasksImportDate),
				typeof(EMailSyncAccount.eventsExportDate),
				typeof(EMailSyncAccount.eventsImportDate),
				typeof(EMailSyncAccount.emailsExportDate),
				typeof(EMailSyncAccount.emailsImportDate),
			})
			{
				Caches[typeof(EMailSyncAccount)].SetValue(row, field.Name, Caches[typeof(EMailSyncAccount)].GetValueOriginal(row, field.Name));
			}
		}

		public PXAction<EMailAccountSyncFilter> ViewEmployee;
		[PXButton]
		[PXUIField(DisplayName = "View Employee", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected IEnumerable viewEmployee(PXAdapter adapter)
		{
			if (SelectedItems.Current == null)
				return adapter.Get();

			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.SelectSingleBound(this, null, SelectedItems.Current.EmployeeID);
			if(employee != null)
				PXRedirectHelper.TryRedirect(this.Caches[typeof(EPEmployee)], employee, string.Empty, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}

		protected void ClearViews()
		{
			SelectedItems.Cache.ClearQueryCacheObsolete();
			SelectedItems.View.Clear();
			SelectedItems.Cache.Clear();
			
			CurrentItem.Cache.ClearQueryCacheObsolete();
			CurrentItem.View.Clear();
			CurrentItem.Cache.Clear();

			CurrentItem.View.RequestRefresh();
			SelectedItems.View.RequestRefresh();
		}

		public PXAction<EMailAccountSyncFilter> ClearLog;
		[PXButton]
		[PXUIField(DisplayName = "Clear Log", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected IEnumerable clearLog(PXAdapter adapter)
		{
			EMailAccountSyncFilter filter = Filter.Current;
			EMailSyncAccount account = SelectedItems.Current;

			if (filter == null || account == null) return adapter.Get();


			PXDatabase.Delete<EMailSyncLog>(
				new PXDataFieldRestrict<EMailSyncLog.address>(account.Address),
				new PXDataFieldRestrict<EMailSyncLog.serverID>(account.ServerID));

			OperationLog.View.RequestRefresh();
			OperationLog.View.Clear();

			return adapter.Get();
		}

		public PXAction<EMailAccountSyncFilter> ResetWarning;
		[PXButton]
		[PXUIField(DisplayName = "Reset Warning", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected IEnumerable resetWarning(PXAdapter adapter)
		{
			EMailAccountSyncFilter filter = Filter.Current;
			EMailSyncAccount account = SelectedItems.Current;

			if (filter == null || account == null) return adapter.Get();


			PXDatabase.Update<EMailSyncAccount>(
				new PXDataFieldAssign<EMailSyncAccount.hasErrors>(false),
				new PXDataFieldRestrict<EMailSyncAccount.serverID>(account.ServerID),
				new PXDataFieldRestrict<EMailSyncAccount.employeeID>(account.EmployeeID));

			SelectedItems.View.RequestRefresh();
			SelectedItems.View.Clear();

			return adapter.Get();
		}

		public PXAction<EMailAccountSyncFilter> Status;
		[PXButton(VisibleOnProcessingResults = true)]
		[PXUIField(DisplayName = "Synchronization Status", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected IEnumerable status(PXAdapter adapter)
		{
			if (SelectedItems.Current == null) return adapter.Get();
			if (CurrentItem.AskExt() != WebDialogResult.OK)
			{
				CurrentItem.Cache.Clear();
				return adapter.Get();
			}

			return adapter.Get();
		}
		#endregion

		public PXFilter<EMailAccountSyncFilter> Filter;
		public PXFilteredProcessingJoin<EMailSyncAccount, EMailAccountSyncFilter,
			InnerJoin<EMailSyncServer, On<EMailSyncServer.accountID, Equal<EMailSyncAccount.serverID>>,
			InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EMailSyncAccount.employeeID>>,
			InnerJoin<Contact, On<EPEmployee.defContactID, Equal<Contact.contactID>, And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>>>,
			Where<EMailSyncServer.isActive, Equal<True>>,
			OrderBy<Asc<EMailSyncAccount.serverID, Asc<EMailSyncAccount.employeeID>>>> SelectedItems;
		
		public PXSelect<EMailSyncAccount, Where<EMailSyncAccount.serverID, Equal<Current<EMailSyncAccount.serverID>>, And<EMailSyncAccount.employeeID, Equal<Current<EMailSyncAccount.employeeID>>>>> CurrentItem;

		[PXFilterable]
		public PXSelectJoin<EMailSyncLog, 
			LeftJoin<Contact, On<EMailSyncLog.address, Equal<Contact.eMail>>,
			LeftJoin<EPEmployee, On<EPEmployee.defContactID, Equal<Contact.contactID>, And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>, 
				And<EPEmployee.bAccountID, Equal<Current<EMailSyncAccount.employeeID>>, Or<EMailSyncLog.address, IsNull>>>>>>,
			Where<EMailSyncLog.serverID, Equal<Current<EMailSyncAccount.serverID>>
				>,
				OrderBy<Desc<EMailSyncLog.eventID>>> OperationLog;

		public PXSelectJoin<EMailSyncReference,
			InnerJoin<Contact,
				On<Contact.noteID, Equal<EMailSyncReference.noteID>,
				And<Contact.createdDateTime, GreaterEqual<Required<Contact.createdDateTime>>>>>,
			Where<EMailSyncReference.address, Equal<Current<EMailSyncAccount.address>>,
				And<EMailSyncReference.serverID, Equal<Current<EMailSyncAccount.serverID>>>>> ContactsReferences;

		public PXSelectJoin<EMailSyncReference,
			InnerJoin<CRActivity,
				On<CRActivity.noteID, Equal<EMailSyncReference.noteID>,
				And<CRActivity.classID, Equal<CRActivityClass.task>,
				And<CRActivity.createdDateTime, GreaterEqual<Required<CRActivity.createdDateTime>>>>>>,
			Where<EMailSyncReference.address, Equal<Current<EMailSyncAccount.address>>,
				And<EMailSyncReference.serverID, Equal<Current<EMailSyncAccount.serverID>>>>> TasksReferences;

		public PXSelectJoin<EMailSyncReference,
			InnerJoin<CRActivity,
				On<CRActivity.noteID, Equal<EMailSyncReference.noteID>,
				And<CRActivity.classID, Equal<CRActivityClass.events>,
				And<CRActivity.createdDateTime, GreaterEqual<Required<CRActivity.createdDateTime>>>>>>,
			Where<EMailSyncReference.address, Equal<Current<EMailSyncAccount.address>>,
				And<EMailSyncReference.serverID, Equal<Current<EMailSyncAccount.serverID>>>>> EventsReferences;

		public PXSelectJoin<EMailSyncReference,
			InnerJoin<CRActivity,
				On<CRActivity.noteID, Equal<EMailSyncReference.noteID>,
				And<CRActivity.classID, Equal<CRActivityClass.email>,
				And<CRActivity.createdDateTime, GreaterEqual<Required<CRActivity.createdDateTime>>>>>>,
			Where<EMailSyncReference.address, Equal<Current<EMailSyncAccount.address>>,
				And<EMailSyncReference.serverID, Equal<Current<EMailSyncAccount.serverID>>>>> EmailsReferences;

		protected virtual IEnumerable selectedItems()
		{
			EMailAccountSyncFilter filter = Filter.Current;

			BqlCommand cmd = SelectedItems.View.BqlSelect;
			if (filter != null && filter.ServerID != null)
				cmd = cmd.WhereAnd<Where<EMailSyncAccount.serverID, Equal<Current<EMailAccountSyncFilter.serverID>>>>();
			if (filter != null && !String.IsNullOrEmpty(filter.PolicyName))
				cmd = cmd.WhereAnd<Where<EMailSyncAccount.policyName, Equal<Current<EMailAccountSyncFilter.policyName>>>>();

			int totalRows = 0;
			int startRow = PXView.StartRow;
			PXView view = new PXView(this, false, cmd); view.Clear( );
			foreach (PXResult<EMailSyncAccount, EMailSyncServer, EPEmployee, Contact> item in
				view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				Contact contact = item;
				EMailSyncAccount account = item;
				account.Address = (contact != null ? contact.EMail : null) ?? account.Address;

				//SelectedItems.Cache.SetStatus(account, PXEntryStatus.Notchanged);

				yield return item;
			}
		}

		public EmailsSyncMaint()
		{
			Object uid = this.UID;
			string screenid = PXSiteMap.CurrentScreenID;
			EMailAccountSyncFilter currentFilter = Filter.Current;

			SelectedItems.SetProcessDelegate(delegate(List<EMailSyncAccount> accounts) { Process(currentFilter, accounts, uid, screenid); });

			SelectedItems.SetProcessCaption(ActionsMessages.Process);
			SelectedItems.SetProcessAllCaption(ActionsMessages.ProcessAll);
			SelectedItems.SetSelected<EMailSyncAccount.selected>();

			OperationLog.AllowInsert = false;
			OperationLog.AllowUpdate = false;
			OperationLog.AllowDelete = false;
			
			PXUIFieldAttribute.SetEnabled(CurrentItem.Cache, null, true);

			ResetContacts.SetEnabled(false);
			ResetTasks.SetEnabled(false);
			ResetEvents.SetEnabled(false);
			ResetEmails.SetEnabled(false);

			this.FieldVerifying.AddHandler(typeof(EMailSyncAccount), typeof(EMailSyncAccount.contactsExportDate).Name + "_Date",
				EMailSyncAccount_ContactsExportDate_FieldVerifying);

			this.FieldVerifying.AddHandler(typeof(EMailSyncAccount), typeof(EMailSyncAccount.tasksExportDate).Name + "_Date",
				EMailSyncAccount_TasksExportDate_FieldVerifying);

			this.FieldVerifying.AddHandler(typeof(EMailSyncAccount), typeof(EMailSyncAccount.eventsExportDate).Name + "_Date",
				EMailSyncAccount_EventsExportDate_FieldVerifying);

			this.FieldVerifying.AddHandler(typeof(EMailSyncAccount), typeof(EMailSyncAccount.emailsExportDate).Name + "_Date",
				EMailSyncAccount_EmailsExportDate_FieldVerifying);
		}

		
		#region Processing
		protected class ProcessingContext
		{
			public readonly EMailAccountSyncFilter Filter;
			public readonly List<EMailSyncAccount> Accounts;
			public readonly Dictionary<int, List<string>> Exceptions;
			public readonly Dictionary<string, EMailSyncPolicy> Policies;

			public ProcessingContext(EMailAccountSyncFilter filter, List<EMailSyncAccount> accounts, Dictionary<string, EMailSyncPolicy> policies)
			{
				Filter = filter;
				Accounts = accounts;
				Policies = policies;
				Exceptions = new Dictionary<int, List<string>>();
			}

			public void StoreError(int server, string address, string message)
			{
				int index = Accounts.FindIndex(a => a.ServerID == server && a.Address == address);
				List<string> list = null;
				if (!Exceptions.TryGetValue(index, out list)) Exceptions[index] = list = new List<string>();
				list.Add(message);
			}
		}
		protected class ProcessingBox
		{
			public EMailSyncPolicy Policy;

			public List<PXSyncMailbox> Emails = new List<PXSyncMailbox>();
			public List<PXSyncMailbox> Contacts = new List<PXSyncMailbox>();
			public List<PXSyncMailbox> Tasks = new List<PXSyncMailbox>();
			public List<PXSyncMailbox> Events = new List<PXSyncMailbox>();

			public Boolean EmailsPending { get { return Emails.Count > 0; } }
			public Boolean ContactsPending { get { return Contacts.Count > 0; } }
			public Boolean TasksPending { get { return Tasks.Count > 0; } }
			public Boolean EventsPending { get { return Events.Count > 0; } }

			public ProcessingBox(EMailSyncPolicy policy)
			{
				Policy = policy;
			}
		}

		public static void Process(EMailAccountSyncFilter filter, List<EMailSyncAccount> accounts, Object uid, string screenid)
		{
			//Check that process is singleton
			foreach (RowTaskInfo info in PXLongOperation.GetTaskList())
			{
				if (Object.Equals(uid, info.NativeKey)) continue;

				PXLongRunStatus status = PXLongOperation.GetStatus(info.NativeKey);
				if (status != PXLongRunStatus.InProcess) continue;

				string company = PXLogin.ExtractCompany(info.User);
				string screen = (info.Screen ?? String.Empty).Replace(".", "");
				if (screen == screenid && company == PXAccess.GetCompanyName())
					throw new PXException(ErrorMessages.PrevOperationNotCompleteYet);
			}

			EmailsSyncMaint graph = CreateInstance<EmailsSyncMaint>();
			using (new PXUTCTimeZoneScope())
			{
				graph.ProcessInternal(new ProcessingContext(filter, accounts, graph.GetPolicies()));
			}
		}
		protected void ProcessInternal(ProcessingContext context)
		{
			Dictionary<int, List<int>> processing = new Dictionary<int, List<int>>( );
			foreach (EMailSyncAccount account in context.Accounts)
			{
				List<int> list;
				if (!processing.TryGetValue(account.ServerID.Value, out list))
					processing[account.ServerID.Value] = list = new List<int>();
				list.Add(account.EmployeeID.Value);
			}

			foreach (int serverID in processing.Keys)
			{
				Dictionary<string, ProcessingBox> buckets = new Dictionary<string, ProcessingBox>();

				EMailSyncServer server = null;
				foreach (PXResult<EMailSyncAccount, EMailSyncServer, EMailAccount, EPEmployee> row in 
					PXSelectJoin<EMailSyncAccount,
						InnerJoin<EMailSyncServer, On<EMailSyncServer.accountID, Equal<EMailSyncAccount.serverID>>,
						InnerJoin<EMailAccount, On<EMailAccount.emailAccountID, Equal<EMailSyncAccount.emailAccountID>>,
						LeftJoin<EPEmployee, On<EMailSyncAccount.employeeID, Equal<EPEmployee.bAccountID>>>>>,
					Where<EMailSyncServer.accountID, Equal<Required<EMailSyncServer.accountID>>, And<EMailSyncAccount.address, IsNotNull>>,
					OrderBy<Asc<EMailSyncAccount.serverID, Asc<EMailSyncAccount.employeeID>>>>.Select(this, serverID))
				{
					server = (EMailSyncServer)row;
					EMailSyncAccount account = (EMailSyncAccount)row;
					EMailAccount eMailAccount = (EMailAccount)row;

					if (!processing.ContainsKey(serverID) || !processing[serverID].Contains(account.EmployeeID.Value)) continue;

					string address = account.Address;
					
					EMailSyncPolicy policy = null;
					if (!String.IsNullOrEmpty(account.PolicyName)) policy = context.Policies[account.PolicyName];
					if (policy == null && !String.IsNullOrEmpty(server.DefaultPolicyName)) policy = context.Policies[server.DefaultPolicyName];
					if (policy == null) throw new PXException(Messages.EmailExchangePolicyNotFound, account.Address);

					ProcessingBox bucket;
					if (!buckets.TryGetValue(policy.PolicyName, out bucket)) buckets[policy.PolicyName] = bucket = new ProcessingBox(policy);


					if (policy.ContactsSync ?? false)
						bucket.Contacts.Add(
							new PXSyncMailbox(address,
								account.EmployeeID.Value, 
								account.EmailAccountID,
								new PXSyncMailboxPreset(account.ContactsExportDate, account.ContactsExportFolder), 
								new PXSyncMailboxPreset(account.ContactsImportDate, account.ContactsImportFolder), 
								eMailAccount.IncomingProcessing ?? false)
							{
								Reinitialize = account.ToReinitialize == true,
								IsReset = account.IsReset == true
							});

					if (policy.EmailsSync ?? false)
						bucket.Emails.Add(
							new PXSyncMailbox(address,
								account.EmployeeID.Value, 
								account.EmailAccountID,
								new PXSyncMailboxPreset(account.EmailsExportDate, account.EmailsExportFolder), 
								new PXSyncMailboxPreset(account.EmailsImportDate, account.EmailsImportFolder), 
								eMailAccount.IncomingProcessing ?? false)
							{
								Reinitialize = account.ToReinitialize == true,
								IsReset = account.IsReset == true
							});

					if (policy.TasksSync ?? false)
						bucket.Tasks.Add(
							new PXSyncMailbox(address,
								account.EmployeeID.Value, 
								account.EmailAccountID,
								new PXSyncMailboxPreset(account.TasksExportDate, account.TasksExportFolder), 
								new PXSyncMailboxPreset(account.TasksImportDate, account.TasksImportFolder), 
								eMailAccount.IncomingProcessing ?? false)
							{
								Reinitialize = account.ToReinitialize == true,
								IsReset = account.IsReset == true
							});

					if (policy.EventsSync ?? false)
						bucket.Events.Add(
							new PXSyncMailbox(address,
								account.EmployeeID.Value, 
								account.EmailAccountID,
								new PXSyncMailboxPreset(account.EventsExportDate, account.EventsExportFolder), 
								new PXSyncMailboxPreset(account.EventsImportDate, account.EventsImportFolder), 
								eMailAccount.IncomingProcessing ?? false)
							{
								Reinitialize = account.ToReinitialize == true,
								IsReset = account.IsReset == true
							});
				}
				if (server == null) continue;

				List<Exception> errors = new List<Exception>();
				foreach (string policy in buckets.Keys)
				{
					ProcessingBox bucket = buckets[policy];
					using (IEmailSyncProvider provider = PXEmailSyncHelper.GetExchanger(server, bucket.Policy))
					{						 
						foreach(PXEmailSyncOperation.Operations operation in Enum.GetValues(typeof(PXEmailSyncOperation.Operations))) //do one time for all existing sync types
						{
							try
							{
								switch (operation)
								{
									case PXEmailSyncOperation.Operations.Emails:
										if (bucket.EmailsPending) provider.EmailsSync(bucket.Policy, PXEmailSyncDirection.Parse(bucket.Policy.EmailsDirection), bucket.Emails);
										break;
									case PXEmailSyncOperation.Operations.Contacts:
										if (bucket.ContactsPending) provider.ContactsSync(bucket.Policy, PXEmailSyncDirection.Parse(bucket.Policy.ContactsDirection), bucket.Contacts);
										break;
									case PXEmailSyncOperation.Operations.Tasks:
										if (bucket.TasksPending) provider.TasksSync(bucket.Policy, PXEmailSyncDirection.Parse(bucket.Policy.TasksDirection), bucket.Tasks);
										break;
									case PXEmailSyncOperation.Operations.Events:
										if (bucket.EventsPending) provider.EventsSync(bucket.Policy, PXEmailSyncDirection.Parse(bucket.Policy.EventsDirection), bucket.Events);
										break;
								}
							}
							catch (PXExchangeSyncItemsException ex)
							{
								if (bucket.Policy.SkipError == true)
									continue;

								if (ex.Errors.Count > 0)
								{
									foreach (string address in ex.Errors.Keys)
									{
										string message = String.Join(Environment.NewLine, ex.Errors[address].ToArray());
										context.StoreError(serverID, address, message);
									}
								}
							}
							catch (PXExchangeSyncFatalException ex)
							{
								if (bucket.Policy.SkipError == true)
									continue;

								errors.Add(new PXException(Messages.EmailExchangeSyncOperationError, operation.ToString().ToLowerInvariant()));
								if (!String.IsNullOrEmpty(ex.Mailbox))
									context.StoreError(serverID, ex.Mailbox, ex.InnerMessage);
								else
									errors.Add(ex);
							}
							catch (Exception ex)
							{
								if (bucket.Policy.SkipError == true)
									continue;

								errors.Add(new PXException(Messages.EmailExchangeSyncOperationError, operation.ToString().ToLowerInvariant()));
								errors.Add(ex);
							}
						}
					}
				}
				if (errors.Count > 0 && context.Exceptions.Count == 0)
				{
					throw new PXException(String.Join(Environment.NewLine, errors.Select(e => e.Message).ToArray()));
				}
			}

			for (int index = 0; index < context.Accounts.Count; index++)
			{
				PXProcessing.SetInfo(index, ActionsMessages.RecordProcessed);
			}

			//handle exceptions 
			if(context.Exceptions.Count > 0)
			{
				foreach (int index in context.Exceptions.Keys)
				{
					List<string> errors = context.Exceptions[index];
					if (errors == null || errors.Count < 0) continue;

					PXProcessing.SetError(index, String.Join(Environment.NewLine, errors.ToArray( )));
				}
				throw new PXException(Messages.EmailExchangeSyncFailed);
			}
		}

		protected Dictionary<string, EMailSyncPolicy> GetPolicies()
		{
			Dictionary<string, EMailSyncPolicy> result = new Dictionary<string, EMailSyncPolicy>();
			foreach (EMailSyncPolicy policy in PXSelect<EMailSyncPolicy>.Select(this))
			{
				result[policy.PolicyName] = policy;
			}
			return result;
		}
		#endregion

		#region Event Handlers
		protected void EMailSyncAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EMailSyncAccount row = e.Row as EMailSyncAccount;
			Status.SetEnabled(row != null);
			if (row == null) return;
			
			ResetContacts.SetEnabled(row.IsContactsReset ?? false);
			ResetTasks.SetEnabled(row.IsTasksReset ?? false);
			ResetEvents.SetEnabled(row.IsEventsReset ?? false);
			ResetEmails.SetEnabled(row.IsEmailsReset ?? false);

			if (row.HasErrors == true)
			{
				cache.RaiseExceptionHandling<EMailSyncAccount.address>(row, row.Address,
					new PXSetPropertyException(
						"Some items were not synchronized. Check synchronization status and log for more details.",
						PXErrorLevel.Warning));
			}
		}
		
		protected void EMailSyncAccount_ContactsExportDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (DatesVerifier<EMailSyncAccount.contactsExportDate>(cache, e))
				cache.SetValue<EMailSyncAccount.isContactsReset>(e.Row, true);
		}
		
		protected void EMailSyncAccount_TasksExportDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (DatesVerifier<EMailSyncAccount.tasksExportDate>(cache, e))
				cache.SetValue<EMailSyncAccount.isTasksReset>(e.Row, true);
		}

		protected void EMailSyncAccount_EventsExportDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (DatesVerifier<EMailSyncAccount.eventsExportDate>(cache, e))
				cache.SetValue<EMailSyncAccount.isEventsReset>(e.Row, true);
		}
		
		protected void EMailSyncAccount_EmailsExportDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (DatesVerifier<EMailSyncAccount.emailsExportDate>(cache, e))
				cache.SetValue<EMailSyncAccount.isEmailsReset>(e.Row, true);
		}
		
		protected void EMailSyncAccount_ContactsExportDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			DatesSwapper<EMailSyncAccount.contactsExportDate, EMailSyncAccount.contactsImportDate>(cache, e);
		}

		protected void EMailSyncAccount_TasksExportDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			DatesSwapper<EMailSyncAccount.tasksExportDate, EMailSyncAccount.tasksImportDate>(cache, e);
		}

		protected void EMailSyncAccount_EventsExportDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			DatesSwapper<EMailSyncAccount.eventsExportDate, EMailSyncAccount.eventsImportDate>(cache, e);
		}

		protected void EMailSyncAccount_EmailsExportDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			DatesSwapper<EMailSyncAccount.emailsExportDate, EMailSyncAccount.emailsImportDate>(cache, e);
		}

		private static void DatesSwapper<TExport, TImport>(PXCache cache, PXFieldUpdatedEventArgs e)
			where TExport : IBqlField
			where TImport : IBqlField
		{
			EMailSyncAccount row = e.Row as EMailSyncAccount;
			if (row == null) return;
			
			var state = cache.GetStateExt(row, typeof(TExport).Name) as PXFieldState;
			cache.SetValue<TImport>(row, state?.Value);
		}

		private static bool DatesVerifier<TExport>(PXCache cache, PXFieldVerifyingEventArgs e)
			where TExport : IBqlField
		{
			EMailSyncAccount row = e.Row as EMailSyncAccount;
			if (row == null) return false;

			var origDate = (DateTime?)cache.GetValueOriginal<TExport>(row);
			var newDate = (DateTime?)e.NewValue;
			if (origDate == null || newDate == null) return false;

			if (newDate >= PXTimeZoneInfo.Now)
			{
				e.NewValue = origDate;

				cache.RaiseExceptionHandling<TExport>(row, origDate, new PXSetPropertyException(Messages.DateLessNow, PXErrorLevel.Error));

				return false;
			}
			
			if (newDate >= origDate)
			{
				cache.RaiseExceptionHandling<TExport>(row, newDate, new PXSetPropertyException(Messages.DateGreaterOld, PXErrorLevel.Warning));
			}

			return true;
		}
		#endregion
	}
}
