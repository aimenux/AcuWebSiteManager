using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class GLConsolSetupMaint : PXGraph<GLConsolSetupMaint>
	{
		public PXSelect<GLSetup> GLSetupRecord;
		public PXSave<GLSetup> Save;
		public PXCancel<GLSetup> Cancel;

		public PXFilteredProcessing<GLConsolSetup, GLSetup> ConsolSetupRecords;

		public PXSelect<Account, Where<Account.gLConsolAccountCD, Equal<Required<Account.gLConsolAccountCD>>>> Accounts;
		public PXSelect<GLConsolAccount> ConsolAccounts;
		public PXSelect<Ledger, Where<Ledger.consolAllowed, Equal<boolTrue>>> Ledgers;
		public PXSelect<Branch> Branches;
		public PXSelect<GLConsolLedger, Where<GLConsolLedger.setupID, Equal<Required<GLConsolSetup.setupID>>>> ConsolLedgers;
		public PXSelect<GLConsolBranch, Where<GLConsolBranch.setupID, Equal<Required<GLConsolBranch.setupID>>>> ConsolBranches;

		public PXSetup<GLSetup> glsetup;

		public GLConsolSetupMaint()
		{
			GLSetup setup = glsetup.Current;

			ConsolSetupRecords.SetProcessCaption(Messages.ProcSynchronize);
			ConsolSetupRecords.SetProcessAllCaption(Messages.ProcSynchronizeAll);
			ConsolSetupRecords.SetProcessDelegate<GLConsolSetupMaint>(Synchronize);
			PXUIFieldAttribute.SetEnabled(ConsolSetupRecords.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastPostPeriod>(ConsolSetupRecords.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastConsDate>(ConsolSetupRecords.Cache, null, false);
			ConsolSetupRecords.SetAutoPersist(true);
			ConsolSetupRecords.Cache.AllowDelete = true;
			ConsolSetupRecords.Cache.AllowInsert = true;

			PXUIFieldAttribute.SetRequired<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, true);
			PXUIFieldAttribute.SetRequired<GLConsolSetup.sourceLedgerCD>(ConsolSetupRecords.Cache, true);

			Save.StateSelectingEvents += new PXFieldSelecting(delegate(PXCache sender, PXFieldSelectingEventArgs e)
			{
				e.ReturnState = PXButtonState.CreateInstance(e.ReturnState, null, null, null, null, null, false,
					PXConfirmationType.Unspecified, null, null, null, null, null, null, null, null, null, null, null, null);
				((PXButtonState)e.ReturnState).Enabled = !PXLongOperation.Exists(this.UID);
			});
		}

		#region Cache Attached Events
		#region BranchCD
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		protected virtual void Branch_BranchCD_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		public IEnumerable branches()
		{
			var branches = PXSelectJoin<Branch,
										InnerJoin<Ledger, 
											On<Ledger.ledgerID, Equal<Branch.ledgerID>>>,
										Where<Ledger.consolAllowed, Equal<True>,
											Or<Ledger.balanceType, Equal<LedgerBalanceType.actual>>>>
										.Select(this);

			foreach (PXResult<Branch, Ledger> res in branches)
			{
				Branch result = res;
				Ledger ledger = res;
				result.LedgerCD = ledger.LedgerCD;

				yield return result;
			}
		}

		protected static void Synchronize(GLConsolSetupMaint graph, GLConsolSetup consolSetup)
		{
			using (PXSoapScope scope = new PXSoapScope(consolSetup.Url, consolSetup.Login, consolSetup.Password))
			{
				GLConsolSetupMaint foreign = PXGraph.CreateInstance<GLConsolSetupMaint>();
				foreign.ConsolAccounts.Select();
				foreign.Ledgers.Select();
				foreign.Branches.Select();
				scope.Process(foreign);

				PXSelect<Account> select = new PXSelect<Account>(graph);
				List<Account> list = new List<Account>();
				foreach (Account acct in select.Select())
				{
					list.Add(acct);
				}

				List<GLConsolLedger> listConsolLedger = new List<GLConsolLedger>();
				foreach (GLConsolLedger ledger in graph.ConsolLedgers.Select(consolSetup.SetupID))
				{
					listConsolLedger.Add(ledger);
				}

				List<GLConsolBranch> listConsolBranch = new List<GLConsolBranch>();
				foreach (GLConsolBranch branch in graph.ConsolBranches.Select(consolSetup.SetupID))
				{
					listConsolBranch.Add(branch);
				}

				foreach (GLConsolAccount consol in foreign.ConsolAccounts.Select())
				{
					Account acct = new Account();
					acct.AccountCD = consol.AccountCD;
					acct = select.Locate(acct);
					if (acct == null)
					{
						foreign.ConsolAccounts.Delete(consol);
					}
					else
					{
						list.Remove(acct);
						if (acct.Description != consol.Description)
						{
							consol.Description = acct.Description;
							foreign.ConsolAccounts.Update(consol);
						}
					}
				}

				foreach (Ledger ledger in foreign.Ledgers.Select())
				{
					GLConsolLedger l = new GLConsolLedger();
					l.SetupID = consolSetup.SetupID;
					l.LedgerCD = ledger.LedgerCD;
					l = graph.ConsolLedgers.Locate(l);
					if (l != null)
					{
						listConsolLedger.Remove(l);
						if (l.Description != ledger.Descr || l.BalanceType != ledger.BalanceType)
						{
							l.Description = ledger.Descr;
							l.BalanceType = ledger.BalanceType;
							graph.ConsolLedgers.Cache.SetStatus(l, PXEntryStatus.Updated);
						}
					}
					else
					{
						l = new GLConsolLedger();
						l.SetupID = consolSetup.SetupID;
						l.LedgerCD = ledger.LedgerCD;
						l.Description = ledger.Descr;
						l.BalanceType = ledger.BalanceType;
						graph.ConsolLedgers.Insert(l);
					}
				}

				foreach (GLConsolLedger ledger in listConsolLedger)
				{
					graph.ConsolLedgers.Delete(ledger);
					if (consolSetup.SourceLedgerCD == ledger.LedgerCD)
					{
						consolSetup.SourceLedgerCD = null;
						graph.ConsolSetupRecords.Update(consolSetup);
					}
				}

				foreach (Branch branch in foreign.Branches.Select())
				{
					GLConsolBranch l = new GLConsolBranch();
					l.SetupID = consolSetup.SetupID;
					l.BranchCD = branch.BranchCD;
					l = graph.ConsolBranches.Locate(l);
					if (l != null)
					{
						listConsolBranch.Remove(l);

						if (l.Description != branch.AcctName || l.LedgerCD != branch.LedgerCD)
						{
							l.LedgerCD = branch.LedgerCD;
							l.Description = branch.AcctName;
							graph.ConsolBranches.Cache.SetStatus(l, PXEntryStatus.Updated);
						}
					}
					else
					{
						l = new GLConsolBranch();
						l.SetupID = consolSetup.SetupID;
						l.BranchCD = branch.BranchCD;
						l.LedgerCD = branch.LedgerCD;
						l.Description = branch.AcctName;
						graph.ConsolBranches.Insert(l);
					}
				}

				foreach (GLConsolBranch branch in listConsolBranch)
				{
					graph.ConsolBranches.Delete(branch);
					if (consolSetup.SourceBranchCD == branch.BranchCD)
					{
						consolSetup.SourceBranchCD = null;
						graph.ConsolSetupRecords.Update(consolSetup);
					}
				}

				foreach (Account acct in list)
				{
					GLConsolAccount consol = new GLConsolAccount();
					consol.AccountCD = acct.AccountCD;
					consol.Description = acct.Description;
					foreign.ConsolAccounts.Insert(consol);
				}

				scope.Process(foreign, foreign.Save);
			}

			graph.Save.Press();
		}

		protected virtual void GLConsolSetup_Url_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			string value = e.NewValue as string;
			if (!String.IsNullOrEmpty(value))
			{
				string url = PXUrl.IngoreAllQueryParameters(value);
				string mainPagePath = PXUrl.MainPagePath.TrimStart('~');
				if (url.EndsWith(mainPagePath, StringComparison.OrdinalIgnoreCase))
					e.NewValue = url.Substring(0, url.Length - mainPagePath.Length);
			}
		}


		#region GLSetupEventHandlers

		protected virtual void GLSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) 
				return;

			GLSetup setup = (GLSetup)e.Row;

			var consColVisible = setup.ConsolSegmentId != null;

			PXUIFieldAttribute.SetEnabled<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, null, consColVisible);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.pasteFlag>(ConsolSetupRecords.Cache, null, consColVisible);

			PXUIFieldAttribute.SetVisible<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, null, consColVisible);
			PXUIFieldAttribute.SetVisible<GLConsolSetup.pasteFlag>(ConsolSetupRecords.Cache, null, consColVisible);

			PXDefaultAttribute.SetPersistingCheck<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, null,
				consColVisible ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		protected virtual void GLSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			foreach (GLConsolSetup detail in ConsolSetupRecords.Select())
			{
				ConsolSetupRecords.Cache.MarkUpdated(detail);
			}
		}

		protected virtual void GLSetup_ConsolSegmentId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var glSetup = (GLSetup) e.Row;

			if (glSetup.ConsolSegmentId == null)
			{
				var glConsolSetupRows = ConsolSetupRecords
					.Select()
					.RowCast<GLConsolSetup>();

				glConsolSetupRows.ForEach(row =>
				{
					row.PasteFlag = false;
				});
			}
		}

		#endregion


		protected virtual void GLConsolSetup_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLConsolSetup.ledgerId>(e.Row);
		}

		protected virtual void GLConsolAccount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			GLConsolAccount consol = (GLConsolAccount)e.Row;
			foreach (Account acct in Accounts.Select(consol.AccountCD))
			{
				acct.GLConsolAccountCD = null;
				Accounts.Update(acct);
			}
		}

		protected virtual void GLConsolSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLConsolSetup setup = (GLConsolSetup)e.Row;
			if (setup != null)
			{
				PXUIFieldAttribute.SetEnabled<GLConsolSetup.selected>(sender, setup, setup.IsActive == true);
			}

			GLConsolLedger coledger = PXSelect<GLConsolLedger, Where<GLConsolLedger.setupID, Equal<Current<GLConsolSetup.setupID>>, And<GLConsolLedger.ledgerCD, Equal<Current<GLConsolSetup.sourceLedgerCD>>>>>.Select(this);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.sourceBranchCD>(sender, e.Row, coledger != null);
		}

		protected virtual void GLConsolSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLConsolSetup setup = (GLConsolSetup)e.Row;
			if (!sender.ObjectsEqual<GLConsolSetup.sourceLedgerCD>(e.Row, e.OldRow))
			{
				string oldbranchid = (string)sender.GetValue<GLConsolSetup.sourceBranchCD>(e.Row);
				sender.SetValue<GLConsolSetup.sourceBranchCD>(e.Row, null);

				sender.SetValueExt<GLConsolSetup.sourceBranchCD>(e.Row, oldbranchid);
			}
		}

		protected virtual void GLConsolSetup_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var setup = (GLConsolSetup)e.NewRow;
			
			if (setup.PasteFlag == true && string.IsNullOrEmpty(setup.SegmentValue))
			{
				sender.RaiseExceptionHandling<GLConsolSetup.segmentValue>(e.NewRow, setup.SegmentValue,
					new PXSetPropertyException(Messages.ConsolidationSegmentValueMayNotBeEmpty));
				e.Cancel = true;
			}
		}

	
		protected virtual void GLConsolSetup_IsActive_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLConsolSetup setup = (GLConsolSetup)e.Row;
			if (setup != null && setup.IsActive == false)
			{
				setup.Selected = false;
			}
		}
	}
}
