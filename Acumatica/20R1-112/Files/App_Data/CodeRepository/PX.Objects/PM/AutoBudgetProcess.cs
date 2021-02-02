using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AR;
using System.Diagnostics;

namespace PX.Objects.PM
{
	public class AutoBudgetWorkerProcess : PXGraph<AutoBudgetWorkerProcess>
	{
		#region DAC Attributes Override
		
		[PXDefault]
		[PXDBInt()]
		protected virtual void PMTran_ProjectID_CacheAttached(PXCache sender) { }

		[PXDefault]
		[PXDBInt()]
		protected virtual void PMTran_TaskID_CacheAttached(PXCache sender) { }

		[PXDefault]
		[PXDBInt()]
		protected virtual void PMTran_InventoryID_CacheAttached(PXCache sender) { }

		#endregion
		
		public PXSelect<PMTran> Transactions;
		
		public virtual List<Balance> Run(int? projectID)
		{			
			Dictionary<string, Balance> balances = new Dictionary<string, Balance>();

			List<PMTran> expenseTrans = CreateExpenseTransactions(projectID);
			List<long> expenseTranIds = new List<long>();
			Debug.Print("Created Expense Transactions:");
			Debug.Indent();
			foreach (PMTran tran in expenseTrans)
			{
				expenseTranIds.Add(tran.TranID.Value);
				Debug.Print("TranID:{0} AccountGroup:{1}, InventoryID={2}, Qty={3}, Amt={4}, Allocated={5}, Released={6}, Billed={7}, Date={8}", tran.TranID, AccountGroupFromID(tran.AccountGroupID), InventoryFromID(tran.InventoryID), tran.Qty, tran.Amount, tran.Allocated, tran.Released, tran.Billed, tran.Date);
			}
			Debug.Unindent();

			if (expenseTrans.Count == 0)
			{
				PXTrace.WriteError(Messages.FailedToEmulateExpenses);
				return new List<Balance>();
			}

			PMAllocatorEmulator ae = PXGraph.CreateInstance<PMAllocatorEmulator>();
			ae.SourceTransactions = expenseTrans;
			foreach (PMTran tran in expenseTrans)
			{
				ae.Transactions.Insert(tran);
			}

			PXSelectBase<PMTask> selectTasks = new PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.allocationID, IsNotNull>>>(this);

			List<PMTask> tasks = new List<PMTask>();
			foreach (PMTask pmTask in selectTasks.Select(projectID))
			{
				tasks.Add(pmTask);
			}

			ae.Execute(tasks);
			Debug.Print("After Allocation:");
			Debug.Indent();

			foreach (PMTran tran in ae.Transactions.Cache.Inserted)
			{
				tran.Released = true;
				Transactions.Cache.Update(tran);

				if (expenseTranIds.Contains(tran.TranID.Value))
					continue;
				
				Debug.Print("TranID:{0} AccountGroup:{1}, InventoryID={2}, Qty={3}, Amt={4}, Allocated={5}, Released={6}, Billed={7}, Date={8}", tran.TranID, AccountGroupFromID(tran.AccountGroupID), InventoryFromID(tran.InventoryID), tran.Qty, tran.Amount, tran.Allocated, tran.Released, tran.Billed, tran.Date);
			}
			Debug.Unindent();

			DateTime billingDate = DateTime.Now.AddDays(1);

			//Get ARTrans for Bill:
			Debug.Print("Bill using the following Billing date={0}", billingDate);

			PMBillEngineEmulator engine = PXGraph.CreateInstance<PMBillEngineEmulator>();
			engine.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Project can be completed.
			engine.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Task can be completed.
			engine.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			Debug.Print("Transactions passed to BillTask:");
			Debug.Indent();
			foreach (PMTran tran in Transactions.Cache.Cached)
			{
				//if (expenseTranIds.Contains(tran.TranID.Value))
				//	continue;
				engine.Transactions.Insert(tran);
				Debug.Print("TranID:{0} AccountGroup:{1}, InventoryID={2}, Qty={3}, Amt={4}, Allocated={5}, Released={6}, Billed={7}, Date={8}", tran.TranID, AccountGroupFromID(tran.AccountGroupID), InventoryFromID(tran.InventoryID), tran.Qty, tran.Amount, tran.Allocated, tran.Released, tran.Billed, tran.Date);
			}
			Debug.Unindent();
			engine.Bill(projectID, billingDate, null);

			
			Debug.Print("AR Trans:");
			Debug.Indent();
			
			foreach (ARTran tran in engine.InvoiceEntry.Transactions.Select())
			{
				if (tran.TaskID == null)
					continue;

				Debug.Print("InventoryID={0}, Qty={1}, Amt={2}", InventoryFromID(tran.InventoryID), tran.Qty, tran.TranAmt);

				Account acct = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(engine.InvoiceEntry, tran.AccountID);

				if (acct.AccountGroupID == null)
					throw new PXException(Messages.FailedEmulateBilling);

				string key = string.Format("{0}.{1}.{2}", tran.TaskID.Value, acct.AccountGroupID, tran.InventoryID ?? PMInventorySelectorAttribute.EmptyInventoryID);

				if (balances.ContainsKey(key))
				{
					balances[key].Amount += tran.TranAmt ?? 0;
					balances[key].Quantity += tran.Qty ?? 0;
				}
				else
				{
					Balance b = new Balance();
					b.TaskID = tran.TaskID.Value;
					b.AccountGroupID = acct.AccountGroupID.Value;
					b.InventoryID = tran.InventoryID ?? PMInventorySelectorAttribute.EmptyInventoryID;
					b.CostCodeID = tran.CostCodeID ?? CostCodeAttribute.GetDefaultCostCode();
					b.Amount = tran.TranAmt ?? 0;
					b.Quantity = tran.Qty ?? 0;

					balances.Add(key, b);
				}
			}

			return new List<Balance>(balances.Values);
		}

		protected virtual void PMTran_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMTran row = e.Row as PMTran;
			if (row != null && e.NewValue != null)
			{
				Account item = PXSelect<Account, Where2<Match<Current<AccessInfo.userName>>, And<Account.accountID, Equal<Required<Account.accountID>>>>>.Select(sender.Graph, e.NewValue);

				if (row != null && item != null && (item.AccountGroupID == null || item.AccountGroupID != row.AccountGroupID))
				{
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(sender.Graph, row.AccountGroupID);
					throw new PXException(PM.Messages.AccountIsNotAssociatedWithAccountGroup, item.AccountCD, accountGroup.GroupCD);
				}
			}
		}

		public virtual List<PMTran> CreateExpenseTransactions(int? projectID)
		{
			PXSelectBase<PMBudget> select = new PXSelectJoin<PMBudget,
					InnerJoin<PMProject, On<PMProject.contractID, Equal<PMBudget.projectID>>,
					InnerJoin<PMTask, On<PMTask.projectID, Equal<PMBudget.projectID>, And<PMTask.taskID, Equal<PMBudget.projectTaskID>>>,
					InnerJoin<PMAccountGroup, On<PMBudget.accountGroupID, Equal<PMAccountGroup.groupID>>,
					InnerJoin<PMCostCode, On<PMBudget.costCodeID, Equal<PMCostCode.costCodeID>>,
					LeftJoin<InventoryItem, On<PMBudget.inventoryID, Equal<InventoryItem.inventoryID>>>>>>>,
					Where<PMBudget.projectID, Equal<Required<PMTask.projectID>>,
					And<PMAccountGroup.type, Equal<AccountType.expense>>>>(this);

			List<PMTran> trans = new List<PMTran>();

			foreach (PXResult<PMBudget, PMProject, PMTask, PMAccountGroup, PMCostCode, InventoryItem> res in select.Select(projectID))
			{
				PMTran tran = ExpenseTransactionFromBudget(res, res, res, res, res, res);

				trans.Add(Transactions.Insert(tran));
			}

			return trans;
		}

		public virtual PMTran ExpenseTransactionFromBudget(PMBudget budget, PMProject project, PMTask task, PMAccountGroup accountGroup, InventoryItem item, PMCostCode costcode)
		{
			PMTran tran = new PMTran();
			tran.AccountGroupID = budget.AccountGroupID;
			tran.ProjectID = budget.ProjectID;
			tran.TaskID = budget.ProjectTaskID;
			tran.InventoryID = budget.InventoryID;
			tran.AccountID = item.InventoryID != null ? item.COGSAcctID : accountGroup.AccountID;
			tran.SubID = item.InventoryID != null ? item.COGSSubID : accountGroup.AccountID;
			tran.TranCuryAmount = budget.CuryRevisedAmount;
			tran.ProjectCuryAmount = budget.CuryRevisedAmount;
			tran.TranCuryID = project.CuryID;
			tran.ProjectCuryID = project.CuryID;
			tran.Qty = budget.RevisedQty;
			tran.UOM = budget.UOM;
			tran.BAccountID = task.CustomerID;
			tran.LocationID = task.LocationID;
			tran.Billable = true;
			tran.UseBillableQty = true;
			tran.BillableQty = budget.RevisedQty;
			tran.Released = true;

			return tran;
		}
		
		public override void Persist()
		{
			//this graph should not be persisted. its an emulation.
		}

		public string AccountGroupFromID(int? accountGroupID)
		{
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, accountGroupID);
			return ag.GroupCD;
		}

		public string InventoryFromID(int? inventoryID)
		{
			if (inventoryID == null || inventoryID == 0)
				return "<N/A>";

			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
			return item.InventoryCD;
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class Balance
		{
			public int TaskID { get; set; }
			public int AccountGroupID { get; set; }
			public int InventoryID { get; set; }
			public int CostCodeID { get; set; }
			public decimal Amount { get; set; }
			public decimal Quantity { get; set; }
		
		}
		
	}

	public class PMAllocatorEmulator : PMAllocator
	{
		/// <summary>
		/// Gets or sets Source Transactions for the allocation. If null Transactions are selected from the database.
		/// </summary>
		public List<PMTran> SourceTransactions { get; set; }

		//public override List<PMTran> GetTranByStep(PMAllocationStep step, int? projectID, int? taskID)
		//{
		//	List<PMTran> result = new List<PMTran>();
		//	foreach (int groupID in GetAccountGroupsRange(step))
		//	{
		//		if (SourceTransactions != null)
		//		{
		//			//use supplied source.
		//			//Note: do not skip allocated transactions cause the allocated flag might be just set in the previous step within the current allocation process.
		//			foreach (PMTran tran in SourceTransactions)
		//			{
		//				if (tran.Released == true &&
		//					tran.AccountGroupID == groupID &&
		//					tran.ProjectID == projectID &&
		//					tran.TaskID == taskID)
		//				{
		//					result.Add(tran);
		//				}
		//			}
		//		}
		//	}

		//	return result;
		//}

		public override PXResultset<PMTran> GetTranFromDatabase(int? projectID, int groupID)
		{
			PXResultset<PMTran> resultset = new PXResultset<PMTran>();
			
			foreach(PMTran tran in SourceTransactions)
			{
				if (tran.ProjectID == projectID && tran.AccountGroupID == groupID)
				{
					resultset.Add(new PXResult<PMTran>(tran));
				}
			}

			return resultset;
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override void Persist()
		{
			//this graph should not be persisted. its an emulation.base.Persist();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			//this graph should not be persisted. its an emulation.base.Persist();
			return 1;
		}
	}

	public class PMBillEngineEmulator : PMBillEngine
	{
		public override ARInvoiceEntry InvoiceEntry
		{
			get
			{
				if (invoiceEntry == null)
				{
					invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntryEmulator>();
					invoiceEntry.FieldVerifying.AddHandler<ARTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Task can be completed.

				}

				return invoiceEntry;
			}
		}

		public override RegisterEntry PMEntry
		{
			get
			{
				if (pmRegisterEntry == null)
				{
					pmRegisterEntry = PXGraph.CreateInstance<RegisterEntryEmulator>();
					pmRegisterEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Project can be completed.
					pmRegisterEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Task can be completed.
					pmRegisterEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

				}

				return pmRegisterEntry;
			}
		}

		public override List<PMTran> SelectBillingBase(int? projectID, int? taskID, int? accountGroupID, bool includeNonBillable)
		{
			List<PMTran> list = new List<PMTran>();

			foreach (PMTran tran in Transactions.Cache.Cached)
			{
				if (tran.ProjectID == projectID && tran.TaskID == taskID && tran.AccountGroupID == accountGroupID &&
					tran.Billed != true && tran.ExcludedFromBilling != true && tran.Released == true &&
					tran.TranType != BatchModule.AR)
					list.Add(tran);
			}

			return list;
		}

		public override List<PMTask> SelectBillableTasks(PMProject project)
		{
			List<PMTask> tasks = new List<PMTask>();
			PXSelectBase<PMTask> selectTasks = new PXSelect<PMTask,
				Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
				And<PMTask.billingID, IsNotNull>>>(this);


			foreach (PMTask task in selectTasks.Select(project.ContractID))
			{
				tasks.Add(task);
			}

			return tasks;
		}

		public override void AutoReleaseCreatedDocuments(PMProject project, BillingResult result, PMRegister wipReversalDoc)
		{

		}

		protected override PMProject SelectProjectByID(int? projectID)
		{
			PMProject project =  base.SelectProjectByID(projectID);
			project.CreateProforma = false;
			return project;
		}

		public override string GetInvoiceKey(string proformaTag, PMBillingRule rule)
		{
			return base.GetInvoiceKey("P", null);
		}

		public override string GenerateProformaTag(PMProject project, PMTask task)
		{
			return "P";
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override void Persist()
		{
			//this graph should not be persisted. its an emulation.base.Persist();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			//this graph should not be persisted. its an emulation.base.Persist();
			return 1;
		}
	}

	public class ARInvoiceEntryEmulator : ARInvoiceEntry
	{
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override void Persist()
		{
			//this graph should not be persisted. its an emulation.base.Persist();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			//this graph should not be persisted. its an emulation.base.Persist();
			return 1;
		}
	}

	public class RegisterEntryEmulator : RegisterEntry
	{
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override void Persist()
		{
			//this graph should not be persisted. its an emulation.base.Persist();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			//this graph should not be persisted. its an emulation.base.Persist();
			return 1;
		}
	}


}
