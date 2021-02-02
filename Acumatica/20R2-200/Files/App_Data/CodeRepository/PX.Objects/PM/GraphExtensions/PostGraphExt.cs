using CommonServiceLocator;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public class PostGraphExt : PXGraphExtension<PostGraph>
    {
		public PXSelect<PMRegister> ProjectDocs;
		public PXSelect<PMTran> ProjectTrans;
		public PXSelect<PMHistoryAccum> ProjectHistory;
		Dictionary<string, PMTask> tasksToAutoAllocate = new Dictionary<string, PMTask>();

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>();
		}

		#region Types

		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		[PXBreakInheritance]
		public class OffsetAccount : Account
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
		[PXBreakInheritance]
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

		#endregion

		#region Cache Attached Events
		#region PMTran
		#region TranID
		[PXDBLongIdentity(IsKey = true)]
		protected virtual void _(Events.CacheAttached<PMTran.tranID> e)
		{
		}
		#endregion
		#region RefNbr
		[PXDBDefault(typeof(PMRegister.refNbr))]// is handled by the graph
		[PXDBString(15, IsUnicode = true)]
		protected virtual void _(Events.CacheAttached<PMTran.refNbr> e)
		{
		}
		#endregion
		#region BatchNbr
		[PXDBDefault(typeof(Batch.batchNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "BatchNbr")]
		protected virtual void _(Events.CacheAttached<PMTran.batchNbr> e)
		{
		}
		#endregion
		#region Date
		[PXDBDate()]
		[PXDefault(typeof(PMRegister.date))]
		public virtual void _(Events.CacheAttached<PMTran.date> e)
		{
		}
		#endregion
		#region BaseCuryInfoID
		public abstract class baseCuryInfoID : IBqlField { }
		[PXDBLong]
		[CurrencyInfoDBDefault(typeof(CurrencyInfo.curyInfoID))]
		public virtual void _(Events.CacheAttached<PMTran.baseCuryInfoID> e)
		{
		}

		#endregion
		#region ProjectCuryInfoID
		public abstract class projectCuryInfoID : IBqlField { }
		[PXDBLong]
		[CurrencyInfoDBDefault(typeof(CurrencyInfo.curyInfoID))]
		public virtual void _(Events.CacheAttached<PMTran.projectCuryInfoID> e)
		{
		}
		#endregion
		#endregion
		#endregion

		[PXOverride]
		public virtual void UpdateAllocationBalance(Batch b)
		{
			UpdateProjectBalance(b);
		}

		[PXOverride]
		public virtual void ReleaseBatchProc(Batch b, bool unholdBatch, Action<Batch, bool> baseMethod)
        {
			tasksToAutoAllocate.Clear();

			baseMethod(b, unholdBatch);
						
			if (tasksToAutoAllocate.Count > 0)
			{
				try
				{
					AutoAllocateTasks(new List<PMTask>(tasksToAutoAllocate.Values));
				}
				catch (Exception ex)
				{
					throw new PXException(ex, PM.Messages.AutoAllocationFailed);
				}

			}
		}

		[PXOverride]
		public virtual void CreateProjectTransactions(Batch b)
		{			
			ProjectBalance pb = CreateProjectBalance();

			if (b.Module == GL.BatchModule.GL)
			{
				PXSelectBase<GLTran> select = new PXSelectJoin<GLTran,
				InnerJoin<Account, On<GLTran.accountID, Equal<Account.accountID>>,
				InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>,
				InnerJoin<PMProject, On<PMProject.contractID, Equal<GLTran.projectID>, And<PMProject.nonProject, Equal<False>>>,
				InnerJoin<PMTask, On<PMTask.projectID, Equal<GLTran.projectID>, And<PMTask.taskID, Equal<GLTran.taskID>>>>>>>,
				Where<GLTran.module, Equal<BatchModule.moduleGL>,
				And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
				And<Account.accountGroupID, IsNotNull,
				And<GLTran.isNonPM, NotEqual<True>>>>>>(Base);

				PXResultset<GLTran> resultset = select.Select(b.BatchNbr);

				if (resultset.Count > 0)
				{
					PMRegister doc = new PMRegister();
					doc.Module = b.Module;
					doc.Date = b.DateEntered;
					doc.Description = b.Description;
					doc.Released = true;
					doc.Status = PMRegister.status.Released;
					ProjectDocs.Insert(doc);
					ProjectDocs.Cache.Persist(PXDBOperation.Insert);
				}

				List<PMTran> sourceForAllocation = new List<PMTran>();

				foreach (PXResult<GLTran, Account, PMAccountGroup, PMProject, PMTask> res in resultset)
				{
					GLTran tran = (GLTran)res;
					Account acc = (Account)res;
					PMAccountGroup ag = (PMAccountGroup)res;
					PMProject project = (PMProject)res;
					PMTask task = (PMTask)res;

					PMTran pmt = (PMTran)ProjectTrans.Cache.Insert();
					pmt.BranchID = tran.BranchID;
					pmt.AccountGroupID = acc.AccountGroupID;
					pmt.AccountID = tran.AccountID;
					pmt.SubID = tran.SubID;
					pmt.BAccountID = tran.ReferenceID;
					pmt.BatchNbr = tran.BatchNbr;
					pmt.Date = tran.TranDate;
					pmt.Description = tran.TranDesc;
					pmt.FinPeriodID = tran.FinPeriodID;
					pmt.TranPeriodID = tran.TranPeriodID;
					pmt.InventoryID = tran.InventoryID ?? PM.PMInventorySelectorAttribute.EmptyInventoryID;
					pmt.OrigLineNbr = tran.LineNbr;
					pmt.OrigModule = tran.Module;
					pmt.OrigRefNbr = tran.RefNbr;
					pmt.OrigTranType = tran.TranType;
					pmt.ProjectID = tran.ProjectID;
					pmt.TaskID = tran.TaskID;
					pmt.CostCodeID = tran.CostCodeID;
					pmt.Billable = tran.NonBillable != true;
					pmt.UseBillableQty = true;
					pmt.UOM = tran.UOM;

					pmt.Amount = tran.DebitAmt - tran.CreditAmt;

					Ledger ledger = Base.Ledger_LedgerID.Select();

					CurrencyInfo projectCuryInfo = null;
					if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
					{
						pmt.TranCuryID = Base.BatchModule.Current.CuryID;
						pmt.ProjectCuryID = project.CuryID;
						pmt.BaseCuryInfoID = tran.CuryInfoID;
						pmt.TranCuryAmount = tran.CuryDebitAmt - tran.CuryCreditAmt;


						if (project.CuryID == ledger.BaseCuryID)
						{
							pmt.ProjectCuryInfoID = tran.CuryInfoID;
							pmt.ProjectCuryAmount = pmt.Amount;
						}
						else if (project.CuryID == Base.BatchModule.Current.CuryID)
						{
							PX.Objects.CM.Extensions.IPXCurrencyService currencyService = ServiceLocator.Current.GetInstance<Func<PXGraph, PX.Objects.CM.Extensions.IPXCurrencyService>>()(Base);

							projectCuryInfo = new CurrencyInfo();
							projectCuryInfo.ModuleCode = GL.BatchModule.PM;
							projectCuryInfo.BaseCuryID = project.CuryID;
							projectCuryInfo.CuryID = project.CuryID;
							projectCuryInfo.CuryRateTypeID = project.RateTypeID ?? currencyService.DefaultRateTypeID(GL.BatchModule.PM);
							projectCuryInfo.CuryEffDate = tran.TranDate;
							projectCuryInfo.CuryRate = 1;
							projectCuryInfo.RecipRate = 1;
							projectCuryInfo = Base.CurrencyInfo_ID.Insert(projectCuryInfo);
							pmt.ProjectCuryInfoID = projectCuryInfo.CuryInfoID;
							pmt.ProjectCuryAmount = pmt.TranCuryAmount;
						}
						else
						{
							PX.Objects.CM.Extensions.IPXCurrencyService currencyService = ServiceLocator.Current.GetInstance<Func<PXGraph, PX.Objects.CM.Extensions.IPXCurrencyService>>()(Base);

							projectCuryInfo = new CurrencyInfo();
							projectCuryInfo.ModuleCode = GL.BatchModule.PM;
							projectCuryInfo.BaseCuryID = project.CuryID;
							projectCuryInfo.CuryID = Base.BatchModule.Current.CuryID;
							projectCuryInfo.CuryRateTypeID = project.RateTypeID ?? currencyService.DefaultRateTypeID(GL.BatchModule.PM);
							projectCuryInfo.CuryEffDate = tran.TranDate;

							var rate = currencyService.GetRate(projectCuryInfo.CuryID, projectCuryInfo.BaseCuryID, projectCuryInfo.CuryRateTypeID, projectCuryInfo.CuryEffDate);
							if (rate == null)
							{
								throw new PXException(PM.Messages.FxTranToProjectNotFound, projectCuryInfo.CuryID, projectCuryInfo.BaseCuryID, projectCuryInfo.CuryRateTypeID, tran.TranDate);
							}

							projectCuryInfo = Base.CurrencyInfo_ID.Insert(projectCuryInfo);
							pmt.ProjectCuryInfoID = projectCuryInfo.CuryInfoID;
							decimal val;
							PXCurrencyAttribute.CuryConvBase(Base.BatchModule.Cache, projectCuryInfo, pmt.TranCuryAmount.GetValueOrDefault(), out val);
							pmt.ProjectCuryAmount = val;
						}
					}
					else
					{
						pmt.TranCuryAmount = pmt.Amount;
						pmt.ProjectCuryAmount = pmt.Amount;
						pmt.TranCuryID = ledger.BaseCuryID;
						pmt.ProjectCuryID = ledger.BaseCuryID;

						if (Base.BatchModule.Current.CuryID != ledger.BaseCuryID)
						{
							CurrencyInfo baseCuryInfo = new CurrencyInfo();
							baseCuryInfo.ModuleCode = GL.BatchModule.PM;
							baseCuryInfo.BaseCuryID = ledger.BaseCuryID;
							baseCuryInfo.CuryID = ledger.BaseCuryID;
							baseCuryInfo.CuryRateTypeID = null;
							baseCuryInfo.CuryEffDate = tran.TranDate;
							baseCuryInfo.CuryRate = 1;
							baseCuryInfo.RecipRate = 1;
							baseCuryInfo = Base.CurrencyInfo_ID.Insert(baseCuryInfo);
							pmt.ProjectCuryInfoID = baseCuryInfo.CuryInfoID;
							pmt.BaseCuryInfoID = baseCuryInfo.CuryInfoID;
						}
						else
						{
							pmt.ProjectCuryInfoID = tran.CuryInfoID;
							pmt.BaseCuryInfoID = tran.CuryInfoID;
						}
					}

					pmt.Qty = tran.Qty;// pmt.Amount >= 0 ? tran.Qty : (tran.Qty * -1);
					int sign = 1;
					if (acc.Type == AccountType.Income || acc.Type == AccountType.Liability)
					{
						sign = -1;
					}

					if (ProjectBalance.IsFlipRequired(acc.Type, ag.Type))
					{
						pmt.ProjectCuryAmount = -pmt.ProjectCuryAmount;
						pmt.TranCuryAmount = -pmt.TranCuryAmount;
						pmt.Amount = -pmt.Amount;
						pmt.Qty = -pmt.Qty;
					}
					pmt.BillableQty = tran.Qty;
					pmt.Released = true;

					PXNoteAttribute.CopyNoteAndFiles(Base.GLTran_Module_BatNbr.Cache, tran, ProjectTrans.Cache, pmt);

					ProjectTrans.Update(pmt);
					Base.CurrencyInfo_ID.Cache.Persist(PXDBOperation.Insert);
					ProjectTrans.Cache.Persist(pmt, PXDBOperation.Insert);

					tran.PMTranID = pmt.TranID;
					Base.Caches[typeof(GLTran)].Update(tran);

					if (pmt.TaskID != null && (pmt.Qty != 0 || pmt.Amount != 0)) //TaskID will be null for Contract
					{
						ProjectBalance.Result balance = pb.Calculate(project, pmt, ag, acc.Type, sign, 1);

						if (balance.Status != null)
						{
							var ps = new PMBudgetAccum();
							ps.ProjectID = balance.Status.ProjectID;
							ps.ProjectTaskID = balance.Status.ProjectTaskID;
							ps.AccountGroupID = balance.Status.AccountGroupID;
							ps.InventoryID = balance.Status.InventoryID;
							ps.CostCodeID = balance.Status.CostCodeID;
							ps.UOM = balance.Status.UOM;
							ps.Type = balance.Status.Type;
							ps.CuryInfoID = balance.Status.CuryInfoID;

							ps = (PMBudgetAccum)Base.Caches[typeof(PMBudgetAccum)].Insert(ps);
							ps.ActualQty += balance.Status.ActualQty.GetValueOrDefault();
							ps.CuryActualAmount += balance.Status.CuryActualAmount.GetValueOrDefault();
							ps.ActualAmount += balance.Status.ActualAmount.GetValueOrDefault();

							Base.Views.Caches.Add(typeof(PMBudgetAccum));
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

							forecast = (PMForecastHistoryAccum)Base.Caches[typeof(PMForecastHistoryAccum)].Insert(forecast);

							forecast.ActualQty += balance.ForecastHistory.ActualQty.GetValueOrDefault();
							forecast.CuryActualAmount += balance.ForecastHistory.CuryActualAmount.GetValueOrDefault();
							forecast.ActualAmount += balance.ForecastHistory.ActualAmount.GetValueOrDefault();
							Base.Views.Caches.Add(typeof(PMForecastHistoryAccum));
						}

						if (balance.TaskTotal != null)
						{
							var ta = new PMTaskTotal();
							ta.ProjectID = balance.TaskTotal.ProjectID;
							ta.TaskID = balance.TaskTotal.TaskID;

							ta = (PMTaskTotal)Base.Caches[typeof(PMTaskTotal)].Insert(ta);
							ta.CuryAsset += balance.TaskTotal.CuryAsset.GetValueOrDefault();
							ta.Asset += balance.TaskTotal.Asset.GetValueOrDefault();
							ta.CuryLiability += balance.TaskTotal.CuryLiability.GetValueOrDefault();
							ta.Liability += balance.TaskTotal.Liability.GetValueOrDefault();
							ta.CuryIncome += balance.TaskTotal.CuryIncome.GetValueOrDefault();
							ta.Income += balance.TaskTotal.Income.GetValueOrDefault();
							ta.CuryExpense += balance.TaskTotal.CuryExpense.GetValueOrDefault();
							ta.Expense += balance.TaskTotal.Expense.GetValueOrDefault();

							Base.Views.Caches.Add(typeof(PMTaskTotal));
						}

						RegisterReleaseProcess.AddToUnbilledSummary(Base, pmt);

						sourceForAllocation.Add(pmt);
						if (pmt.Allocated != true && pmt.ExcludedFromAllocation != true && project.AutoAllocate == true)
						{
							if (!tasksToAutoAllocate.ContainsKey(string.Format("{0}.{1}", task.ProjectID, task.TaskID)))
							{
								tasksToAutoAllocate.Add(string.Format("{0}.{1}", task.ProjectID, task.TaskID), task);
							}
						}
					}
				}
				Base.Caches[typeof(PMUnbilledDailySummaryAccum)].Persist(PXDBOperation.Insert);
				Base.Caches[typeof(PMBudgetAccum)].Persist(PXDBOperation.Insert);
				Base.Caches[typeof(PMTaskTotal)].Persist(PXDBOperation.Insert);
			}
		}

		public virtual ProjectBalance CreateProjectBalance()
		{
			return new ProjectBalance(Base);
		}

		protected virtual void UpdateProjectBalance(Batch b)
		{
			PXSelectBase<GLTran> select = new PXSelectJoin<GLTran,
				InnerJoin<PMProject, On<GLTran.projectID, Equal<PMProject.contractID>>,
				InnerJoin<PMTran, On<GLTran.pMTranID, Equal<PMTran.tranID>>,
				LeftJoin<Account, On<PMTran.accountID, Equal<Account.accountID>>,
				InnerJoin<PMAccountGroup, On<PMTran.accountGroupID, Equal<PMAccountGroup.groupID>>,
				LeftJoin<OffsetAccount, On<PMTran.offsetAccountID, Equal<OffsetAccount.accountID>>,
				LeftJoin<OffsetPMAccountGroup, On<OffsetAccount.accountGroupID, Equal<OffsetPMAccountGroup.groupID>>>>>>>>,
				Where<GLTran.module, Equal<Required<GLTran.module>>,
				And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
				And<GLTran.pMTranID, IsNotNull,
				And<GLTran.projectID, NotEqual<Required<GLTran.projectID>>>>>>>(Base);

			ProjectBalance pb = CreateProjectBalance();

			foreach (PXResult<GLTran, PMProject, PMTran, Account, PMAccountGroup, OffsetAccount, OffsetPMAccountGroup> res in select.Select(b.Module, b.BatchNbr, ProjectDefaultAttribute.NonProject()))
			{
				GLTran tran = (GLTran)res;
				PMProject project = (PMProject)res;
				PMTran pmt = (PMTran)res;
				Account account = (Account)res;
				PMAccountGroup ag = (PMAccountGroup)res;
				OffsetAccount offsetAccount = (OffsetAccount)res;
				OffsetPMAccountGroup offsetAg = (OffsetPMAccountGroup)res;

				if (pmt.RemainderOfTranID != null)
					continue; //skip remainder transactions. 

				var balances = pb.Calculate(project, pmt, account, ag, offsetAccount, offsetAg);

				foreach (ProjectBalance.Result balance in balances)
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

						hist = ProjectHistory.Insert(hist);
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

		protected virtual void AutoAllocateTasks(List<PMTask> tasks)
		{
			PMSetup setup = PXSelect<PMSetup>.Select(Base);
			bool autoreleaseAllocation = setup.AutoReleaseAllocation == true;

			PMAllocator allocator = PXGraph.CreateInstance<PMAllocator>();
			allocator.Clear();
			allocator.TimeStamp = Base.TimeStamp;
			allocator.Execute(tasks);
			allocator.Actions.PressSave();

			if (allocator.Document.Current != null && autoreleaseAllocation)
			{
				List<PMRegister> list = new List<PMRegister>();
				list.Add(allocator.Document.Current);
				List<ProcessInfo<Batch>> batchList;
				bool releaseSuccess = RegisterRelease.ReleaseWithoutPost(list, false, out batchList);
				if (!releaseSuccess)
				{
					throw new PXException(PM.Messages.AutoReleaseFailed);
				}
			}
		}

		
	}
}
