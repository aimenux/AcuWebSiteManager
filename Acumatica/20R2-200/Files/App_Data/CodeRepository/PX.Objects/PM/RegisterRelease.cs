using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.PM.GraphExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PX.Objects.PM
{
	[GL.TableDashboardType]
	public class RegisterRelease : PXGraph<RegisterRelease>
	{
		public PXCancel<PMRegister> Cancel;

		[PXFilterable]
		public PXProcessing<PMRegister, Where<PMRegister.released, Equal<False>>> Items;

		public RegisterRelease()
		{
			Items.SetProcessDelegate(
				delegate(List<PMRegister> list)
				{
					List<PMRegister> newlist = new List<PMRegister>(list.Count);
					foreach (PMRegister doc in list)
					{
						newlist.Add(doc);
					}
					Release(newlist, true);
				}
			);
			Items.SetProcessCaption(Messages.Release);
			Items.SetProcessAllCaption(Messages.ReleaseAll);
		}

		public static void Release(PMRegister doc)
		{
            List<PMRegister> list = new List<PMRegister>();
           
			list.Add(doc);
            Release(list, false);
		}

		public static void Release(List<PMRegister> list, bool isMassProcess)
		{
			List<ProcessInfo<Batch>> infoList;
			
			bool releaseSuccess = ReleaseWithoutPost(list, isMassProcess, out infoList);
			if (!releaseSuccess)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}

			bool postSuccess = Post(infoList, isMassProcess);
			if (!postSuccess)
			{
				throw new PXException(GL.Messages.DocumentsNotPosted);
			}
		}

		
		public static bool ReleaseWithoutPost(List<PMRegister> list, bool isMassProcess, out List<ProcessInfo<Batch>> infoList)
		{
			bool failed = false;
			infoList = new List<ProcessInfo<Batch>>();

			if (!list.Any())
			{
				return !failed;
			}

			RegisterReleaseProcess rg = PXGraph.CreateInstance<RegisterReleaseProcess>();
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			PMAllocator allocator = PXGraph.CreateInstance<PMAllocator>();
			//Task may be IsActive=False - it may be completed. User cannot create transactions with this
			//TaskID. But the system has to process the given task - hence override the FieldVerification in the Selector. 
			je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

			for (int i = 0; i < list.Count; i++)
			{
				ProcessInfo<Batch> info = new ProcessInfo<Batch>(i); 
				infoList.Add(info);
				PMRegister doc = list[i];

				try
				{
					List<PMTask> allocTasks;
					info.Batches.AddRange(rg.Release(je, doc, out allocTasks));
					
					allocator.Clear();
					allocator.TimeStamp = je.TimeStamp;
					
					if (allocTasks.Count > 0)
					{
						allocator.Execute(allocTasks);
						allocator.Actions.PressSave();
					}
					if (allocator.Document.Current != null && rg.AutoReleaseAllocation )
					{
						List<PMTask> allocTasks2;
						info.Batches.AddRange(rg.Release(je, allocator.Document.Current, out allocTasks2));
					}

					if (isMassProcess)
					PXProcessing<PMRegister>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						PXProcessing<PMRegister>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
						failed = true;
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}
			}

			return !failed;
		}

		public static bool Post(List<ProcessInfo<Batch>> infoList, bool isMassProcess)
		{
			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			PMSetup setup = PXSelect<PMSetup>.Select(pg);
			bool failed = false;

			if (setup != null && setup.AutoPost == true)
			{
				foreach (ProcessInfo<Batch> t in infoList)
				{
					foreach (Batch batch in t.Batches)
					{
						try
						{
							pg.Clear();
							pg.PostBatchProc(batch);
						}
						catch (Exception e)
						{
							if (isMassProcess)
							{
								failed = true;
									PXProcessing<PMRegister>.SetError(t.RecordIndex,
																	  e is PXOuterException
								                                  		? e.Message + "\r\n" +
								                                  		  String.Join("\r\n", ((PXOuterException) e).InnerMessages)
								                                  		: e.Message);
							}
							else
							{
								var message = PXMessages.LocalizeNoPrefix(Messages.PostToGLFailed) + Environment.NewLine + e.Message;
								throw new PXMassProcessException(t.RecordIndex, new PXException(message, e));
							}
						}
					}
				}
			}

			return !failed;
		}
	}

	public class ProcessInfo<T> where T : class
	{
		public int RecordIndex { get; set; }
		public List<T> Batches { get; private set; }
	   
		public ProcessInfo(int index)
		{
			this.RecordIndex = index;
			this.Batches = new List<T>();
		}
	}
	public class RegisterReleaseProcess : PXGraph<RegisterReleaseProcess>
	{
		private PMSetup setup;
				
		public bool AutoReleaseAllocation
		{
			get
			{
				if (setup == null)
				{
					setup = PXSelect<PMSetup>.Select(this);
				}

				return setup.AutoReleaseAllocation == true;
			}
		}

		public virtual PMRegister OnBeforeRelease(PMRegister doc)
		{
			return doc;
		}

		public List<Batch> Release(JournalEntry je, PMRegister doc, out List<PMTask> allocTasks)
		{
			doc = OnBeforeRelease(doc);

			allocTasks = new List<PMTask>();
			
			List<Batch> batches = new List<Batch>();
			Dictionary<string, PMTask> tasksToAutoAllocate = new Dictionary<string, PMTask>();
			List<PMTran> sourceForAllocation = new List<PMTran>();
			Dictionary<string, List<TranInfo>> transByFinPeriod = GetTransByBranchAndFinPeriod(doc);

			Debug.Assert(transByFinPeriod.Count > 0, "Failed to select transactions by finperiod in PMRegister Release.");

			ProjectBalance pb = CreateProjectBalance();

			using (var ts = new PXTransactionScope())
			{
				foreach (KeyValuePair<string, List<TranInfo>> kv in transByFinPeriod)
				{
					string[] parts = kv.Key.Split('.');
					int? branchID = parts[0] == "0" ? null : (int?)int.Parse(parts[0]);

					je.Clear(PXClearOption.ClearAll);

					CurrencyInfo info = new CurrencyInfo();
					info.CuryID = parts[2];
					info.CuryEffDate = Accessinfo.BusinessDate;
					info = je.currencyinfo.Insert(info);

					Batch newbatch = new Batch();
					newbatch.Module = doc.Module;
					newbatch.Status = BatchStatus.Unposted;
					newbatch.Released = true;
					newbatch.Hold = false;
					newbatch.BranchID = branchID;
					newbatch.FinPeriodID = parts[1];
					newbatch.CuryID = parts[2];
					newbatch.CuryInfoID = info.CuryInfoID;
					newbatch.Description = doc.Description;
					je.BatchModule.Insert(newbatch);

					bool tranAdded = false;
					foreach (TranInfo t in kv.Value)
					{
						bool isGL = false;

						if (t.Tran.Released != true && t.Tran.IsNonGL != true && t.Project.BaseType == CT.CTPRType.Project
							&& !string.IsNullOrEmpty(t.AccountGroup.Type) && t.AccountGroup.Type != PMAccountType.OffBalance && !ProjectDefaultAttribute.IsNonProject( t.Tran.ProjectID) &&
							t.Tran.AccountID != null && t.Tran.SubID != null && t.Tran.OffsetAccountID != null && t.Tran.OffsetSubID != null)
						{
							GLTran tran1 = new GLTran();
							tran1.TranDate = t.Tran.Date;
							tran1.TranPeriodID = t.Tran.TranPeriodID;
							tran1.SummPost = false;
							tran1.BranchID = t.Tran.BranchID;
							tran1.PMTranID = t.Tran.TranID;
							tran1.ProjectID = t.Tran.ProjectID;
							tran1.TaskID = t.Tran.TaskID;
							tran1.CostCodeID = t.Tran.CostCodeID;
							tran1.TranDesc = t.Tran.Description;
							tran1.ReferenceID = t.Tran.BAccountID;
							tran1.InventoryID = t.Tran.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : t.Tran.InventoryID;
							tran1.Qty = t.Tran.Qty;
							tran1.UOM = t.Tran.UOM;
							tran1.TranType = t.Tran.TranType;
							tran1.CuryInfoID = t.Tran.BaseCuryInfoID;
							tran1.CuryCreditAmt = 0;
							tran1.CreditAmt = 0;							
							tran1.CuryDebitAmt = t.Tran.TranCuryAmount;
							tran1.DebitAmt = t.Tran.Amount;
							tran1.AccountID = t.Tran.AccountID;
							tran1.SubID = t.Tran.SubID;
							tran1.Released = true;
							je.GLTranModuleBatNbr.Insert(tran1);

							GLTran tran2 = new GLTran();
							tran2.TranDate = t.Tran.Date;
							tran2.TranPeriodID = t.Tran.TranPeriodID;
							tran2.SummPost = false;
							tran2.BranchID = t.Tran.BranchID;
							tran2.PMTranID = t.Tran.TranID;
							tran2.ProjectID = t.OffsetAccountGroup.GroupID != null ? t.Tran.ProjectID : ProjectDefaultAttribute.NonProject();
							tran2.TaskID = t.OffsetAccountGroup.GroupID != null ? t.Tran.TaskID : null;
							tran2.CostCodeID = tran2.TaskID != null ? t.Tran.CostCodeID : null;
							tran2.TranDesc = t.Tran.Description;
							tran2.ReferenceID = t.Tran.BAccountID;
							tran2.InventoryID = t.Tran.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : t.Tran.InventoryID;
							tran2.Qty = t.Tran.Qty;
							tran2.UOM = t.Tran.UOM;
							tran2.TranType = t.Tran.TranType;
							tran2.CuryInfoID = t.Tran.BaseCuryInfoID;
							tran2.CuryCreditAmt = t.Tran.TranCuryAmount;
							tran2.CreditAmt = t.Tran.Amount;
							tran2.CuryDebitAmt = 0;
							tran2.DebitAmt = 0;
							tran2.AccountID = t.Tran.OffsetAccountID;
							tran2.SubID = t.Tran.OffsetSubID;
							tran2.Released = true;
							je.GLTranModuleBatNbr.Insert(tran2);

							tranAdded = true;
							isGL = true;
							t.Tran.BatchNbr = je.BatchModule.Current.BatchNbr;
						}

						if (!isGL)
						{
							if (t.Tran.AccountGroupID == null && t.Project.BaseType == CT.CTPRType.Project && t.Project.NonProject != true)
							{
								throw new PXException(Messages.AccountGroupIsRequired, doc.RefNbr);
							}
						}

						if (t.Tran.ExcludedFromBalance != true)
						{
							JournalEntryProjectExt xje = je.GetExtension<JournalEntryProjectExt>();
							IList<ProjectBalance.Result> balances = pb.Calculate(t.Project, t.Tran, t.Account, t.AccountGroup, t.OffsetAccount, t.OffsetAccountGroup);
							foreach (ProjectBalance.Result balance in balances)
							{
								if (balance.Status != null)
								{
									PMBudgetAccum ps = new PMBudgetAccum();
									ps.ProjectID = balance.Status.ProjectID;
									ps.ProjectTaskID = balance.Status.ProjectTaskID;
									ps.AccountGroupID = balance.Status.AccountGroupID;
									ps.InventoryID = balance.Status.InventoryID;
									ps.CostCodeID = balance.Status.CostCodeID;
									ps.UOM = balance.Status.UOM;
									ps.IsProduction = balance.Status.IsProduction;
									ps.Type = balance.Status.Type;
									ps.Description = balance.Status.Description;
									ps.CuryInfoID = balance.Status.CuryInfoID;

									ps = xje.ProjectBudget.Insert(ps);
									ps.ActualQty += balance.Status.ActualQty.GetValueOrDefault();
									ps.CuryActualAmount += balance.Status.CuryActualAmount.GetValueOrDefault();
									ps.ActualAmount += balance.Status.ActualAmount.GetValueOrDefault();
								}

								if (balance.ForecastHistory != null)
								{
									PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
									forecast.ProjectID = balance.ForecastHistory.ProjectID;
									forecast.ProjectTaskID = balance.ForecastHistory.ProjectTaskID;
									forecast.AccountGroupID = balance.ForecastHistory.AccountGroupID;
									forecast.InventoryID = balance.ForecastHistory.InventoryID;
									forecast.CostCodeID = balance.ForecastHistory.CostCodeID;
									forecast.PeriodID = balance.ForecastHistory.PeriodID;

									forecast = xje.ForecastHistory.Insert(forecast);

									forecast.ActualQty += balance.ForecastHistory.ActualQty.GetValueOrDefault();
									forecast.CuryActualAmount += balance.ForecastHistory.CuryActualAmount.GetValueOrDefault();
									forecast.ActualAmount += balance.ForecastHistory.ActualAmount.GetValueOrDefault();
								}

								if (balance.TaskTotal != null)
								{
									PMTaskTotal ta = new PMTaskTotal();
									ta.ProjectID = balance.TaskTotal.ProjectID;
									ta.TaskID = balance.TaskTotal.TaskID;

									ta = xje.ProjectTaskTotals.Insert(ta);
									ta.CuryAsset += balance.TaskTotal.CuryAsset.GetValueOrDefault();
									ta.Asset += balance.TaskTotal.Asset.GetValueOrDefault();
									ta.CuryLiability += balance.TaskTotal.CuryLiability.GetValueOrDefault();
									ta.Liability += balance.TaskTotal.Liability.GetValueOrDefault();
									ta.CuryIncome += balance.TaskTotal.CuryIncome.GetValueOrDefault();
									ta.Income += balance.TaskTotal.Income.GetValueOrDefault();
									ta.CuryExpense += balance.TaskTotal.CuryExpense.GetValueOrDefault();
									ta.Expense += balance.TaskTotal.Expense.GetValueOrDefault();
								}

								if (!isGL)
								{
									foreach (PMHistory item in balance.History)
									{
										PMHistoryAccum hist = new PMHistoryAccum();
										hist.ProjectID = item.ProjectID;
										hist.ProjectTaskID = item.ProjectTaskID;
										hist.AccountGroupID = item.AccountGroupID;
										hist.InventoryID = item.InventoryID;
										hist.CostCodeID = item.CostCodeID;
										hist.PeriodID = item.PeriodID;
										hist.BranchID = item.BranchID;

										hist = (PMHistoryAccum)je.Caches[typeof(PMHistoryAccum)].Insert(hist);
										hist.FinPTDCuryAmount += item.FinPTDCuryAmount.GetValueOrDefault();
										hist.FinPTDAmount += item.FinPTDAmount.GetValueOrDefault();
										hist.FinYTDCuryAmount += item.FinYTDCuryAmount.GetValueOrDefault();
										hist.FinYTDAmount += item.FinYTDAmount.GetValueOrDefault();
										hist.FinPTDQty += item.FinPTDQty.GetValueOrDefault();
										hist.FinYTDQty += item.FinYTDQty.GetValueOrDefault();
										hist.TranPTDCuryAmount += item.TranPTDCuryAmount.GetValueOrDefault();
										hist.TranPTDAmount += item.TranPTDAmount.GetValueOrDefault();
										hist.TranYTDCuryAmount += item.TranYTDCuryAmount.GetValueOrDefault();
										hist.TranYTDAmount += item.TranYTDAmount.GetValueOrDefault();
										hist.TranPTDQty += item.TranPTDQty.GetValueOrDefault();
										hist.TranYTDQty += item.TranYTDQty.GetValueOrDefault();
									}
								}
							}
						}
						
						AddToUnbilledSummary(je, t.Tran);
						
						t.Tran.Released = true;
						je.Caches[typeof(PMTran)].Update(t.Tran);

						sourceForAllocation.Add(t.Tran);
						if (t.Tran.Allocated != true && t.Tran.ExcludedFromAllocation != true && t.Project.AutoAllocate == true)
						{
							if (!tasksToAutoAllocate.ContainsKey(string.Format("{0}.{1}", t.Task.ProjectID, t.Task.TaskID)))
							{
								tasksToAutoAllocate.Add(string.Format("{0}.{1}", t.Task.ProjectID, t.Task.TaskID), t.Task);
							}
						}

					}

					if (tranAdded)
					{
						je.Save.Press();
						batches.Add(je.BatchModule.Current);
					}
					else
					{
						je.Persist(typeof(PMTran), PXDBOperation.Update);
						je.Persist(typeof(PMBudgetAccum), PXDBOperation.Insert);
						je.Persist(typeof(PMForecastHistoryAccum), PXDBOperation.Insert);
						je.Persist(typeof(PMTaskTotal), PXDBOperation.Insert);
						je.Persist(typeof(PMTaskAllocTotalAccum), PXDBOperation.Insert);
						je.Persist(typeof(PMHistoryAccum), PXDBOperation.Insert);//only non-gl balance is updated
						je.Persist(typeof(PMUnbilledDailySummaryAccum), PXDBOperation.Insert);
						je.SelectTimeStamp();
					}
				}

				allocTasks.AddRange(tasksToAutoAllocate.Values);

				doc.Released = true;
				doc.Status = PMRegister.status.Released;
				je.Caches[typeof(PMRegister)].Update(doc);

				je.Persist(typeof(PMTran), PXDBOperation.Update);
				je.Persist(typeof(PMRegister), PXDBOperation.Update);
				je.Persist(typeof(PMBudgetAccum), PXDBOperation.Insert);
				je.Persist(typeof(PMForecastHistoryAccum), PXDBOperation.Insert);
				je.Persist(typeof(PMTaskAllocTotalAccum), PXDBOperation.Insert);
				je.Persist(typeof(PMTaskTotal), PXDBOperation.Insert);
				

				ts.Complete();
			}

			return batches;
		}

		public virtual ProjectBalance CreateProjectBalance()
		{
			return new ProjectBalance(this);
		}

		/// <summary>
		/// The key of the dictionary is a BranchID.FinPeriodID key.
		/// </summary>
		private Dictionary<string, List<TranInfo>> GetTransByBranchAndFinPeriod(PMRegister doc)
		{
			Dictionary<string, List<TranInfo>> transByFinPeriod = new Dictionary<string, List<TranInfo>>();
			
			PXSelectBase<PMTran> select = new PXSelectJoin<PMTran,
				LeftJoin<Account, On<PMTran.accountID, Equal<Account.accountID>>,
				InnerJoin<PMProject, On<PMProject.contractID, Equal<PMTran.projectID>>,
				LeftJoin<PMTask, On<PMTask.projectID, Equal<PMTran.projectID>, And<PMTask.taskID, Equal<PMTran.taskID>>>,
                LeftJoin<OffsetAccount, On<PMTran.offsetAccountID, Equal<OffsetAccount.accountID>>,
                LeftJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<PMTran.accountGroupID>>,
                LeftJoin<OffsetPMAccountGroup, On<OffsetPMAccountGroup.groupID, Equal<OffsetAccount.accountGroupID>>>>>>>>,
                Where<PMTran.tranType, Equal<Required<PMTran.tranType>>, 
                And<PMTran.refNbr, Equal<Required<PMTran.refNbr>>>>>(this);

			foreach (PXResult<PMTran, Account, PMProject, PMTask, OffsetAccount, PMAccountGroup, OffsetPMAccountGroup> res in select.Select(doc.Module, doc.RefNbr))
			{
				TranInfo tran = new TranInfo(res);
				string key = string.Format("{0}.{1}.{2}", tran.Tran.BranchID.GetValueOrDefault(), tran.Tran.FinPeriodID, tran.Tran.TranCuryID);
				
				if (transByFinPeriod.ContainsKey(key))
				{
					transByFinPeriod[key].Add(tran);
				}
				else
				{
					List<TranInfo> list = new List<TranInfo>();
					list.Add(tran);
					transByFinPeriod.Add(key, list);
				}
			}

			return transByFinPeriod;
		}

		/// <summary>
		/// Increases the counter of unbilled transactions.
		/// Billed and Reversed transactions are ignored.
		/// Note: Although the method will add all necessary caches to the views collection of the graph to be saved on persist. 
		/// This will work only if the graph is persisted within the context of current request. If there will be graph load/unload
		/// between this call and the Persist please add the required Caches to the graph manualy.
		/// </summary>
		public static void AddToUnbilledSummary(PXGraph graph, PMTran tran)
		{
			if (tran.Billed == true || tran.ExcludedFromBilling == true)
				return;
			
			UpdateUnbilledSummary(graph, tran, false);
		}

		/// <summary>
		/// Decreases the counter of unbilled transactions.
		/// Only Billed or Reversed transactions are processed.
		/// Note: Although the method will add all necessary caches to the views collection of the graph to be saved on persist. 
		/// This will work only if the graph is persisted within the context of current request. If there will be graph load/unload
		/// between this call and the Persist please add the required Caches to the graph manualy.
		/// </summary>
		public static void SubtractFromUnbilledSummary(PXGraph graph, PMTran tran)
		{
			if (tran.Billed != true && tran.ExcludedFromBilling != true)
				return;

			UpdateUnbilledSummary(graph, tran, true);
		}

		private static void UpdateUnbilledSummary(PXGraph graph, PMTran tran, bool reverse)
		{
			if (tran.ProjectID == null || tran.TaskID == null || tran.AccountGroupID == null || tran.Date == null)
			{
				return;
			}
			
			graph.Views.Caches.Add(typeof(PMUnbilledDailySummaryAccum));

			int counter = reverse ? -1 : 1;

			PMUnbilledDailySummaryAccum unbilled = new PMUnbilledDailySummaryAccum();
			unbilled.ProjectID = tran.ProjectID;
			unbilled.TaskID = tran.TaskID;
			unbilled.AccountGroupID = tran.AccountGroupID;
			unbilled.Date = tran.Date;
			unbilled = (PMUnbilledDailySummaryAccum)graph.Caches[typeof(PMUnbilledDailySummaryAccum)].Insert(unbilled);

			unbilled.Billable += tran.Billable == true ? counter : 0;
			unbilled.NonBillable += tran.Billable == true ? 0 : counter;
		}


		private class TranInfo
		{
			public PMTran Tran { get; private set; }
			public Account Account { get; private set; }
			public PMAccountGroup AccountGroup { get; private set; }
			public OffsetAccount OffsetAccount { get; private set; }
			public OffsetPMAccountGroup OffsetAccountGroup { get; private set; }
			public PMProject Project { get; private set; }
			public PMTask Task { get; private set; }

			public TranInfo(PXResult<PMTran, Account, PMProject, PMTask, OffsetAccount, PMAccountGroup, OffsetPMAccountGroup> res)
			{
				this.Tran = (PMTran)res;
				this.Account = (Account)res;
				this.AccountGroup = (PMAccountGroup)res;
				this.OffsetAccount = (OffsetAccount)res;
				this.OffsetAccountGroup = (OffsetPMAccountGroup)res;
				this.Project = (PMProject)res;
				this.Task = (PMTask)res;
			}
		}
		
		[PXHidden]
        [Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class OffsetAccount : Account
        {
            #region AccountID
            public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
            #endregion
            #region AccountCD
            public new abstract class accountCD : PX.Data.BQL.BqlString.Field<accountCD> { }
            #endregion
            #region AccountGroupID
            public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
            #endregion
        }

		[PXHidden]
        [Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class OffsetPMAccountGroup : PMAccountGroup
        {
            #region GroupID
            public new abstract class groupID : PX.Data.BQL.BqlInt.Field<groupID> { }
           
            #endregion
            #region GroupCD
            public new abstract class groupCD : PX.Data.BQL.BqlString.Field<groupCD> { }
            #endregion
            #region Type
            public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
            #endregion
        }
	}
		
}
