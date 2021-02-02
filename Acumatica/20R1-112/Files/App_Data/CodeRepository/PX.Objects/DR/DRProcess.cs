using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR.Descriptor;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using static PX.Objects.AR.ARReleaseProcess;

namespace PX.Objects.DR
{
	public class DRProcess 
		: PXGraph<DRProcess>, IDREntityStorage, IBusinessAccountProvider, IInventoryItemProvider
	{		
		public PXSelect<DRSchedule> Schedule;
		public PXSelect<DRScheduleDetail> ScheduleDetail;
		public PXSelect<DRScheduleTran> Transactions;
		public PXSelect<DRExpenseBalance> ExpenseBalance;
		public PXSelect<DRExpenseProjectionAccum> ExpenseProjection;
		public PXSelect<DRRevenueBalance> RevenueBalance;
		public PXSelect<DRRevenueProjectionAccum> RevenueProjection;
		public PXSetup<DRSetup> Setup;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		#region Exceptions
		private List<PXException> _Exceptions = new List<PXException>();
		public List<PXException> Exceptions { get { return _Exceptions; } }
		#endregion

		public DRProcess()
		{
			DRSetup setup = Setup.Current;
		}

		protected virtual void DRScheduleDetail_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void DRScheduleDetail_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void DRSchedule_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void DRScheduleDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as DRScheduleDetail;

			if (row == null)
				return;

			PXDefaultAttribute.SetPersistingCheck<DRScheduleDetail.defCode>(sender, row, row.IsResidual == true ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}

		/// <summary>
		/// This is auto setting of the <see cref="DRScheduleDetail.curyDefAmt"/> field because of only the base currency are allowed for DR Schedules for now.
		/// </summary>
		protected virtual void DRScheduleDetail_DefAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				return;
			}

			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null) return;

			if (scheduleDetail.CuryDefAmt != null && scheduleDetail.CuryDefAmt == null)
			{
				decimal curyDefAmt = 0m;
				CM.PXCurrencyAttribute.CuryConvCury<DRScheduleDetail.curyInfoID>(sender, scheduleDetail, scheduleDetail.CuryTotalAmt.Value, out curyDefAmt);
				scheduleDetail.CuryDefAmt = curyDefAmt;
			}
		}

		/// <summary>
		/// This is auto setting of the <see cref="DRScheduleDetail.curyTotalAmt"/> field because of only the base currency are allowed for DR Schedules for now.
		/// </summary>
		protected virtual void DRScheduleDetail_TotalAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if(PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				return;
			}

			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null) return;

			if (scheduleDetail.CuryTotalAmt != null && scheduleDetail.CuryTotalAmt == null)
			{
				decimal curyTotalAmt = 0m;
				CM.PXCurrencyAttribute.CuryConvCury<DRScheduleDetail.curyInfoID>(sender, scheduleDetail, scheduleDetail.CuryTotalAmt.Value, out curyTotalAmt);
				scheduleDetail.CuryTotalAmt = curyTotalAmt;
			}
		}

		public List<Batch> RunRecognition(List<DRRecognition.DRBatch> list, DateTime? recDate)
		{
			List<Batch> batchlist = new List<Batch>(list.Count);
			Exceptions.Clear();

			foreach (DRRecognition.DRBatch drBatch in list)
			{
				JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
				je.FieldVerifying.AddHandler<GLTran.referenceID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				je.FieldVerifying.AddHandler<GLTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
				je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

				Batch newbatch = new Batch();
				newbatch.Module = BatchModule.DR;
				newbatch.Status = "U";
				newbatch.Released = true;
				newbatch.Hold = false;
				newbatch.BranchID = drBatch.BranchID;
				newbatch.FinPeriodID = drBatch.FinPeriod;
				newbatch.TranPeriodID = drBatch.FinPeriod;
				newbatch.DateEntered = recDate;
				je.BatchModule.Insert(newbatch);

				List<DRScheduleTran> drTranList = new List<DRScheduleTran>();
				GLTran tran = null, tran2 = null;
				foreach (DRRecognition.DRTranKey drTranKey in drBatch.Trans)
				{
					try
					{
						DRScheduleTran drTran = PXSelect<
								DRScheduleTran,
								Where<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
									And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
									And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>,
									And<DRScheduleTran.lineNbr, Equal<Required<DRScheduleTran.lineNbr>>>>>>>
								.Select(je, drTranKey.ScheduleID, drTranKey.ComponentID, drTranKey.DetailLineNbr, drTranKey.LineNbr);

						DRSchedule schedule = GetDeferralSchedule(drTranKey.ScheduleID);
						DRScheduleDetail scheduleDetail = GetScheduleDetailForComponent(drTranKey.ScheduleID, drTranKey.ComponentID, drTranKey.DetailLineNbr);
						#region Tran 1
						tran = new GLTran();
						tran.SummPost = false;
						tran.TranType = schedule.DocType;
						tran.RefNbr = schedule.ScheduleNbr;
						if (scheduleDetail.BAccountID != null && scheduleDetail.BAccountID != DRScheduleDetail.EmptyBAccountID)
							tran.ReferenceID = scheduleDetail.BAccountID;
						if (scheduleDetail.ComponentID != null && scheduleDetail.ComponentID != DRScheduleDetail.EmptyComponentID)
							tran.InventoryID = scheduleDetail.ComponentID;

						var isReversed = IsReversed(scheduleDetail);

						if (drTran.Amount >= 0 && !isReversed ||
							 drTran.Amount < 0 && isReversed)
						{
							tran.AccountID = scheduleDetail.DefAcctID;
							tran.SubID = scheduleDetail.DefSubID;
							tran.ReclassificationProhibited = true;
						}
						else
						{
							tran.AccountID = drTran.AccountID;
							tran.SubID = drTran.SubID;
						}

						tran.BranchID = drTran.BranchID;
						tran.CuryDebitAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran.DebitAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran.CuryCreditAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
						tran.CreditAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
						tran.TranDesc = schedule.TranDesc;
						tran.Released = true;
						tran.TranDate = drTran.RecDate;
						tran.ProjectID = schedule.ProjectID;
						tran.TaskID = schedule.TaskID;
						tran.TranLineNbr = drTran.LineNbr;

						tran = je.GLTranModuleBatNbr.Insert(tran);
						#endregion
						#region Tran 2
						tran2 = new GLTran();
						tran2.SummPost = false;
						tran2.TranType = schedule.DocType;
						tran2.RefNbr = schedule.ScheduleNbr;
						if (scheduleDetail.BAccountID != null && scheduleDetail.BAccountID != DRScheduleDetail.EmptyBAccountID)
							tran2.ReferenceID = scheduleDetail.BAccountID;
						if (scheduleDetail.ComponentID != null && scheduleDetail.ComponentID != DRScheduleDetail.EmptyComponentID)
							tran2.InventoryID = scheduleDetail.ComponentID;

						if (drTran.Amount >= 0 && !isReversed ||
							 drTran.Amount < 0 && isReversed)
						{
							tran2.AccountID = drTran.AccountID;
							tran2.SubID = drTran.SubID;
						}
						else
						{
							tran2.AccountID = scheduleDetail.DefAcctID;
							tran2.SubID = scheduleDetail.DefSubID;
							tran2.ReclassificationProhibited = true;
						}

						tran2.BranchID = drTran.BranchID;
						tran2.CuryDebitAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
						tran2.DebitAmt = scheduleDetail.Module == BatchModule.AR ? 0 : Math.Abs(drTran.Amount.Value);
						tran2.CuryCreditAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran2.CreditAmt = scheduleDetail.Module == BatchModule.AR ? Math.Abs(drTran.Amount.Value) : 0;
						tran2.TranDesc = schedule.TranDesc;
						tran2.Released = true;
						tran2.TranDate = drTran.RecDate;
						tran2.ProjectID = schedule.ProjectID;
						tran2.TaskID = schedule.TaskID;
						tran2.TranLineNbr = drTran.LineNbr;

						tran2 = je.GLTranModuleBatNbr.Insert(tran2);
						#endregion

						drTranList.Add(drTran);
					}
					catch (PXException e)
					{
						if (tran != null)
						{
							je.GLTranModuleBatNbr.Delete(tran);
						}
						if (tran2 != null)
						{
							je.GLTranModuleBatNbr.Delete(tran2);
						}
						PXTrace.WriteError(e.ToString());
						e.Data.Add(typeof(DRSchedule.scheduleID).Name, drTranKey.ScheduleID);
						Exceptions.Add(e);
					}
				}

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					je.Save.Press();

					batchlist.Add(je.BatchModule.Current);

					foreach (DRScheduleTran drTran in drTranList)
					{
						drTran.BatchNbr = je.BatchModule.Current.BatchNbr;
						drTran.Status = DRScheduleTranStatus.Posted;
						drTran.TranDate = je.BatchModule.Current.DateEntered;
						drTran.FinPeriodID = je.BatchModule.Current.FinPeriodID;//Bug: 20528
						Transactions.Update(drTran);

						DRScheduleDetail scheduleDetail = GetScheduleDetailForComponent(drTran.ScheduleID, drTran.ComponentID, drTran.DetailLineNbr);

						decimal tranAmt = drTran.Amount ?? 0m;
						decimal detailDefAmt = scheduleDetail.CuryDefAmt ?? 0m;

						scheduleDetail.CuryDefAmt -= Math.Sign(tranAmt) * Math.Min(Math.Abs(tranAmt), Math.Abs(detailDefAmt));
						scheduleDetail.LastRecFinPeriodID = drTran.FinPeriodID;

						if (scheduleDetail.CuryDefAmt == 0)
						{
							scheduleDetail.Status = DRScheduleStatus.Closed;
							scheduleDetail.CloseFinPeriodID = drTran.FinPeriodID;
							scheduleDetail.IsOpen = false;
						}

						ScheduleDetail.Update(scheduleDetail);

						DRDeferredCode deferralCode = PXSelect<
								DRDeferredCode,
								Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>
							.Select(this, scheduleDetail.DefCode);

						UpdateBalance(drTran, scheduleDetail, deferralCode.AccountType);
					}
					Transactions.Cache.Persist(PXDBOperation.Update);
					ts.Complete();
				}
			}

			this.Actions.PressSave();

			return batchlist;

		}

		/// <summary>
		/// Creates a <see cref="DRSchedule"/> record with multiple <see cref="DRScheduleDetail"/> 
		/// records depending on the original AR document and document line transactions, as well as
		/// on the deferral code parameters.
		/// </summary>
		/// <param name="arTransaction">An AR document line transaction that corresponds to the original document.</param>
		/// <param name="deferralCode">The deferral code to be used in schedule details.</param>
		/// <param name="originalDocument">Original AR document.</param>
		/// <remarks>
		/// Records are created only in the cache. You have to persist them manually.
		/// </remarks>
		public virtual void CreateSchedule(ARTran arTransaction, DRDeferredCode deferralCode, ARRegister originalDocument, decimal amount, bool isDraft)
		{
			if (arTransaction == null) throw new ArgumentNullException(nameof(arTransaction));
			if (deferralCode == null) throw new ArgumentNullException(nameof(deferralCode));

			InventoryItem inventoryItem = GetInventoryItem(arTransaction.InventoryID);

			ARSetup arSetup = PXSelect<ARSetup>.Select(this);

			DRSchedule existingSchedule = PXSelect<
				DRSchedule,
				Where<
					DRSchedule.module, Equal<BatchModule.moduleAR>,
					And<DRSchedule.docType, Equal<Required<ARTran.tranType>>,
					And<DRSchedule.refNbr, Equal<Required<ARTran.refNbr>>,
					And<DRSchedule.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>>
				.Select(this, arTransaction.TranType, arTransaction.RefNbr, arTransaction.LineNbr);

			if (arTransaction.DefScheduleID == null)
			{
				decimal transactionQuantityInBaseUnits = (arTransaction.Qty ?? 0);

				if (arTransaction.InventoryID != null)
				{
					transactionQuantityInBaseUnits = INUnitAttribute.ConvertToBase(
						this.Caches[typeof(ARTran)], 
						arTransaction.InventoryID,
						arTransaction.UOM, 
						(arTransaction.Qty ?? 0), 
						INPrecision.QUANTITY);
				}

				decimal? compoundDiscountRate = (1.0m - (arTransaction.DiscPctDR ?? 0.0m) * 0.01m);

				decimal unitPriceDR;

				PXCurrencyAttribute.CuryConvBase<ARTran.curyInfoID>(
					Caches[typeof(ARTran)], 
					arTransaction, 
					arTransaction.CuryUnitPriceDR ?? 0.0m, 
					out unitPriceDR);
				
				DRScheduleParameters scheduleParameters = GetScheduleParameters(originalDocument, arTransaction);
				ScheduleCreator scheduleCreator = new ScheduleCreator(
					this, new ARSubaccountProvider(this), this, this,
					FinPeriodRepository,
                    roundingFunction: x => PXCurrencyAttribute.BaseRound(this, x), 
					branchID: arTransaction.BranchID, isDraft: isDraft);

				if (existingSchedule == null)
				{
					scheduleCreator.CreateOriginalSchedule(
						scheduleParameters, 
						deferralCode, 
						inventoryItem, 
						new AccountSubaccountPair(arTransaction.AccountID, arTransaction.SubID), 
						amount, 
						unitPriceDR, 
						compoundDiscountRate, 
						transactionQuantityInBaseUnits);
				}
				else
				{
					scheduleCreator.ReevaluateSchedule(
						existingSchedule, 
						scheduleParameters, 
						deferralCode, 
						amount,
						attachedToOriginalSchedule: false);
				}
			}
			else
			{
				if (deferralCode.Method == DeferredMethodType.CashReceipt)
				{
					if (originalDocument.DocType == ARDocType.CreditMemo)
					{
						UpdateOriginalSchedule(arTransaction, deferralCode, amount, originalDocument.DocDate, originalDocument.FinPeriodID, originalDocument.CustomerID, originalDocument.CustomerLocationID);
					}
				}
				else
				{
					if (originalDocument.DocType == ARDocType.CreditMemo)
					{
						if (existingSchedule == null)
						{
							DRScheduleParameters scheduleParameters = GetScheduleParameters(originalDocument, arTransaction);
							CreateRelatedSchedule(scheduleParameters, arTransaction.BranchID, arTransaction.DefScheduleID, amount, deferralCode, inventoryItem, arTransaction.AccountID, arTransaction.SubID, isDraft, true);
						}
						else
						{
							DRScheduleParameters scheduleParameters = GetScheduleParameters(originalDocument, arTransaction);
							ScheduleCreator scheduleCreator = new ScheduleCreator(this, new ARSubaccountProvider(this), this, this, FinPeriodRepository,
                                roundingFunction: x => PXCurrencyAttribute.BaseRound(this, x), branchID: arTransaction.BranchID, isDraft: isDraft);

							scheduleCreator.ReevaluateSchedule(
								existingSchedule, 
								scheduleParameters, 
								deferralCode, 
								amount,
								attachedToOriginalSchedule: true);
						}
					}
					else if (originalDocument.DocType == ARDocType.DebitMemo)
					{
						if (existingSchedule == null)
						{
							DRScheduleParameters scheduleParameters = GetScheduleParameters(originalDocument, arTransaction);
							CreateRelatedSchedule(scheduleParameters, arTransaction.BranchID, arTransaction.DefScheduleID, amount, deferralCode, inventoryItem, arTransaction.AccountID, arTransaction.SubID, isDraft, false);
						}
						else
						{
							DRScheduleParameters scheduleParameters = GetScheduleParameters(originalDocument, arTransaction);
							ScheduleCreator scheduleCreator = new ScheduleCreator(this, new ARSubaccountProvider(this), this, this, FinPeriodRepository,
                                roundingFunction: x => PXCurrencyAttribute.BaseRound(this, x), branchID: arTransaction.BranchID, isDraft: isDraft);

							scheduleCreator.ReevaluateSchedule(
								existingSchedule,
								scheduleParameters,
								deferralCode,
								amount,
								attachedToOriginalSchedule: true);
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates DRSchedule record with multiple DRScheduleDetail records depending on the DeferredCode schedule.
		/// </summary>
		/// <param name="tran">AP Transaction</param>
		/// <param name="defCode">Deferred Code</param>
		/// <param name="document">AP Invoice</param>
		/// <remarks>
		/// Records are created only in the Cache. You have to manually call Perist method.
		/// </remarks>
		public virtual void CreateSchedule(APTran tran, DRDeferredCode defCode, APInvoice document, decimal amount, bool isDraft)
		{
			if (tran == null) throw new ArgumentNullException(nameof(tran));
			if (defCode == null) throw new ArgumentNullException(nameof(defCode));

			InventoryItem inventoryItem = GetInventoryItem(tran.InventoryID);

			APSetup apSetup = PXSelect<APSetup>.Select(this);

			DRSchedule existingSchedule = PXSelect<
				DRSchedule,
				Where<
					DRSchedule.module, Equal<BatchModule.moduleAP>,
					And<DRSchedule.docType, Equal<Required<APTran.tranType>>,
					And<DRSchedule.refNbr, Equal<Required<APTran.refNbr>>,
					And<DRSchedule.lineNbr, Equal<Required<APTran.lineNbr>>>>>>>
				.Select(this, tran.TranType, tran.RefNbr, tran.LineNbr);

			if (tran.DefScheduleID == null)
			{
				decimal quantityInBaseUnit = tran.Qty ?? 0;

				if (tran.InventoryID != null)
				{
					quantityInBaseUnit = INUnitAttribute.ConvertToBase(
						Caches[typeof(APTran)], 
						tran.InventoryID, 
						tran.UOM,
						tran.Qty ?? 0, 
						INPrecision.QUANTITY);
				}

				DRScheduleParameters scheduleParams = GetScheduleParameters(document, tran);
				ScheduleCreator creator = new ScheduleCreator(this, new APSubaccountProvider(this), this, this, FinPeriodRepository,
                    roundingFunction: x => PXCurrencyAttribute.BaseRound(this, x), 
					branchID: tran.BranchID, 
					isDraft: isDraft);

				if (existingSchedule != null)
				{
					creator.ReevaluateSchedule(
						existingSchedule, 
						scheduleParams, 
						defCode, 
						amount, 
						attachedToOriginalSchedule: false);
				}
				else
				{
					creator.CreateOriginalSchedule(
						scheduleParams, 
						defCode, 
						inventoryItem, 
						new AccountSubaccountPair(tran.AccountID, tran.SubID), 
						amount, 
						null, 
						null, 
						quantityInBaseUnit);
				}
			}
			else
			{
				if (document.DocType == APDocType.DebitAdj)
				{
					if (existingSchedule != null)
					{
						DRScheduleParameters scheduleParams = GetScheduleParameters(document, tran);
						ScheduleCreator creator = new ScheduleCreator(this, new APSubaccountProvider(this), this, this, FinPeriodRepository,
                            roundingFunction: x => PXCurrencyAttribute.BaseRound(this, x), 
							branchID: tran.BranchID, 
							isDraft: isDraft);

						creator.ReevaluateSchedule(
							existingSchedule,
							scheduleParams,
							defCode,
							amount,
							attachedToOriginalSchedule: true);
					}
					else
					{
						DRScheduleParameters scheduleParams = GetScheduleParameters(document, tran);
						CreateRelatedSchedule(scheduleParams, tran.BranchID, tran.DefScheduleID,
							amount, defCode, inventoryItem, tran.AccountID, tran.SubID, isDraft, true);
					}
				}
			}
		}

		public virtual void RunIntegrityCheck(List<DRBalanceValidation.DRBalanceType> list, string finPeriodID)
		{
			bool failed = false;
			foreach (DRBalanceValidation.DRBalanceType item in list)
			{
				PXProcessing<DRBalanceValidation.DRBalanceType>.SetCurrentItem(item);
				try
				{
					RunIntegrityCheck(item, finPeriodID);
					PXProcessing<DRBalanceValidation.DRBalanceType>.SetProcessed();
				}
				catch (Exception ex)
				{
					failed = true;
					PXProcessing<DRBalanceValidation.DRBalanceType>.SetError(ex.Message);
				}
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}
		}

		/// <summary>
		/// Rebuilds DR Balance History Tables.
		/// </summary>
		/// <param name="item">Type of Balance to rebuild</param>
		public virtual void RunIntegrityCheck(DRBalanceValidation.DRBalanceType item, string finPeriodID)
		{
			string requiredModule;

			switch (item.AccountType)
			{
				case DeferredAccountType.Income:
					requiredModule = BatchModule.AR;
					break;

				case DeferredAccountType.Expense:
					requiredModule = BatchModule.AP;
					break;

				default:
					throw new PXException(
						Messages.InvalidAccountType,
						DeferredAccountType.Expense,
						DeferredAccountType.Income,
						item.AccountType);
			}

			ValidateFinPeriod(finPeriodID, requiredModule);

			PXSelectBase<DRScheduleTran> incomingTransactions = new PXSelectJoin<
				DRScheduleTran,
				InnerJoin<DRScheduleDetail,
					On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
					And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>,
					And<DRScheduleDetail.detailLineNbr, Equal<DRScheduleTran.detailLineNbr>>>>>,
				Where<
					DRScheduleTran.lineNbr, Equal<DRScheduleDetail.creditLineNbr>,
					And<DRScheduleDetail.module, Equal<Required<DRScheduleDetail.module>>,
					And<DRScheduleDetail.status, NotEqual<DRScheduleStatus.DraftStatus>,
					And<DRScheduleTran.tranPeriodID, GreaterEqual<Required<DRScheduleTran.tranPeriodID>>>>>>>
				(this);

			foreach (PXResult<DRScheduleTran, DRScheduleDetail> detailAndTransaction in
				incomingTransactions.Select(requiredModule, finPeriodID))
			{
				DRScheduleTran deferralTransaction = detailAndTransaction;
				DRScheduleDetail scheduleDetail = detailAndTransaction;

				InitBalance(deferralTransaction, scheduleDetail, item.AccountType);
			}

			PXSelectBase<DRScheduleTran> openTransactions = new PXSelectJoin<
				DRScheduleTran,
				InnerJoin<DRScheduleDetail,
					On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
					And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>,
					And<DRScheduleDetail.detailLineNbr, Equal<DRScheduleTran.detailLineNbr>>>>>,
				Where<
					DRScheduleTran.lineNbr, NotEqual<DRScheduleDetail.creditLineNbr>,
					And<DRScheduleDetail.module, Equal<Required<DRScheduleDetail.module>>,
					And<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>,
					And<DRScheduleDetail.status, NotEqual<DRScheduleStatus.DraftStatus>,
					And<DRScheduleTran.tranPeriodID, GreaterEqual<Required<DRScheduleTran.tranPeriodID>>>>>>>>
				(this);

			foreach (PXResult<DRScheduleTran, DRScheduleDetail> detailAndTransaction in
				openTransactions.Select(requiredModule, finPeriodID))
			{
				DRScheduleTran deferralTransaction = detailAndTransaction;
				DRScheduleDetail scheduleDetail = detailAndTransaction;

				UpdateBalanceProjection(deferralTransaction, scheduleDetail, item.AccountType);
			}

			PXSelectBase<DRScheduleTran> postedTransactions = new PXSelectJoin<
				DRScheduleTran,
				InnerJoin<DRScheduleDetail,
					On<DRScheduleDetail.scheduleID, Equal<DRScheduleTran.scheduleID>,
					And<DRScheduleDetail.componentID, Equal<DRScheduleTran.componentID>,
					And<DRScheduleDetail.detailLineNbr, Equal<DRScheduleTran.detailLineNbr>>>>>,
				Where<
					DRScheduleTran.lineNbr, NotEqual<DRScheduleDetail.creditLineNbr>,
					And<DRScheduleDetail.module, Equal<Required<DRScheduleDetail.module>>,
					And<DRScheduleTran.status, Equal<DRScheduleTranStatus.PostedStatus>,
					And<DRScheduleTran.tranPeriodID, GreaterEqual<Required<DRScheduleTran.tranPeriodID>>>>>>>
				(this);

			foreach (PXResult<DRScheduleTran, DRScheduleDetail> detailAndTransaction in
				postedTransactions.Select(requiredModule, finPeriodID))
			{
				DRScheduleTran deferralTransaction = detailAndTransaction;
				DRScheduleDetail scheduleDetail = detailAndTransaction;

				UpdateBalance(deferralTransaction, scheduleDetail, item.AccountType, isIntegrityCheck: true);
			}
			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(FinPeriod.organizationID.MasterValue, finPeriodID);

			using (new PXConnectionScope())
			{
				using (PXTransactionScope transactionScope = new PXTransactionScope())
				{
					if (requiredModule == BatchModule.AR)
					{
						PXUpdateJoin<
							Set<DRRevenueBalance.begBalance, IsNull<DRRevenueBalance2.endBalance, Zero>,
							Set<DRRevenueBalance.begProjected, IsNull<DRRevenueBalance2.endProjected, Zero>,
							Set<DRRevenueBalance.pTDDeferred, Zero,
							Set<DRRevenueBalance.pTDRecognized, Zero,
							Set<DRRevenueBalance.pTDRecognizedSamePeriod, Zero,
							Set<DRRevenueBalance.pTDProjected, Zero,
							Set<DRRevenueBalance.endBalance, IsNull<DRRevenueBalance2.endBalance, Zero>,
							Set<DRRevenueBalance.endProjected, IsNull<DRRevenueBalance2.endProjected, Zero>>>>>>>>>,
						DRRevenueBalance,
						LeftJoin<Branch,
							On<DRRevenueBalance.branchID, Equal<Branch.branchID>>,
						LeftJoin<FinPeriod,
							On<DRRevenueBalance.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<Branch.organizationID, Equal<FinPeriod.organizationID>>>,
						LeftJoin<OrganizationFinPeriodExt,
							  On<OrganizationFinPeriodExt.masterFinPeriodID, Equal<Required<OrganizationFinPeriodExt.masterFinPeriodID>>,
							  And<Branch.organizationID, Equal<OrganizationFinPeriodExt.organizationID>>>,
						LeftJoin<DRRevenueBalanceByPeriod,
							On<DRRevenueBalanceByPeriod.branchID, Equal<DRRevenueBalance.branchID>,
							And<DRRevenueBalanceByPeriod.acctID, Equal<DRRevenueBalance.acctID>,
							And<DRRevenueBalanceByPeriod.subID, Equal<DRRevenueBalance.subID>,
							And<DRRevenueBalanceByPeriod.componentID, Equal<DRRevenueBalance.componentID>,
							And<DRRevenueBalanceByPeriod.customerID, Equal<DRRevenueBalance.customerID>,
							And<DRRevenueBalanceByPeriod.projectID, Equal<DRRevenueBalance.projectID>,
							And<DRRevenueBalanceByPeriod.finPeriodID, Equal<OrganizationFinPeriodExt.prevFinPeriodID>>>>>>>>,
						LeftJoin<DRRevenueBalance2,
							  On<DRRevenueBalance2.branchID, Equal<DRRevenueBalance.branchID>,
							 And<DRRevenueBalance2.acctID, Equal<DRRevenueBalance.acctID>,
							 And<DRRevenueBalance2.subID, Equal<DRRevenueBalance.subID>,
							 And<DRRevenueBalance2.componentID, Equal<DRRevenueBalance.componentID>,
							 And<DRRevenueBalance2.customerID, Equal<DRRevenueBalance.customerID>,
							 And<DRRevenueBalance2.projectID, Equal<DRRevenueBalance.projectID>,
							 And<DRRevenueBalance2.finPeriodID, Equal<DRRevenueBalanceByPeriod.lastActivityPeriod>>>>>>>>>>>>>,
						Where<FinPeriod.masterFinPeriodID, GreaterEqual<Required<FinPeriod.masterFinPeriodID>>>>
						.Update(this, finPeriodID, finPeriodID);

						PXUpdateJoin<
							Set<DRRevenueBalance.tranBegBalance, IsNull<DRRevenueBalance2.tranEndBalance, Zero>,
							Set<DRRevenueBalance.tranBegProjected, IsNull<DRRevenueBalance2.tranEndProjected, Zero>,
							Set<DRRevenueBalance.tranPTDDeferred, Zero,
							Set<DRRevenueBalance.tranPTDRecognized, Zero,
							Set<DRRevenueBalance.tranPTDRecognizedSamePeriod, Zero,
							Set<DRRevenueBalance.tranPTDProjected, Zero,
							Set<DRRevenueBalance.tranEndBalance, IsNull<DRRevenueBalance2.tranEndBalance, Zero>,
							Set<DRRevenueBalance.tranEndProjected, IsNull<DRRevenueBalance2.tranEndProjected, Zero>>>>>>>>>,
						DRRevenueBalance,
						LeftJoin<DRRevenueBalanceByPeriod,
							 On<DRRevenueBalanceByPeriod.branchID, Equal<DRRevenueBalance.branchID>,
							 And<DRRevenueBalanceByPeriod.acctID, Equal<DRRevenueBalance.acctID>,
							 And<DRRevenueBalanceByPeriod.subID, Equal<DRRevenueBalance.subID>,
							 And<DRRevenueBalanceByPeriod.componentID, Equal<DRRevenueBalance.componentID>,
							 And<DRRevenueBalanceByPeriod.customerID, Equal<DRRevenueBalance.customerID>,
							 And<DRRevenueBalanceByPeriod.projectID, Equal<DRRevenueBalance.projectID>,
							And<DRRevenueBalanceByPeriod.finPeriodID, Equal<Required<FinPeriod.masterFinPeriodID>>>>>>>>>,
						LeftJoin<DRRevenueBalance2,
							On<DRRevenueBalance2.branchID, Equal<DRRevenueBalance.branchID>,
							 And<DRRevenueBalance2.acctID, Equal<DRRevenueBalance.acctID>,
							 And<DRRevenueBalance2.subID, Equal<DRRevenueBalance.subID>,
							 And<DRRevenueBalance2.componentID, Equal<DRRevenueBalance.componentID>,
							 And<DRRevenueBalance2.customerID, Equal<DRRevenueBalance.customerID>,
							 And<DRRevenueBalance2.projectID, Equal<DRRevenueBalance.projectID>,
							 And<DRRevenueBalance2.finPeriodID, Equal<DRRevenueBalanceByPeriod.lastActivityPeriod>>>>>>>>>>,
						Where<DRRevenueBalance.finPeriodID, GreaterEqual<Required<DRRevenueBalance.finPeriodID>>>>
						.Update(this, prevPeriod?.FinPeriodID, finPeriodID);

						PXUpdateJoin<
							Set<DRRevenueProjection.pTDRecognized, Zero,
							Set<DRRevenueProjection.pTDRecognizedSamePeriod, Zero,
							Set<DRRevenueProjection.pTDProjected, Zero>>>,
						DRRevenueProjection,
						LeftJoin<Branch,
							On<DRRevenueProjection.branchID, Equal<Branch.branchID>>,
						LeftJoin<FinPeriod,
							On<DRRevenueProjection.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<Branch.organizationID, Equal<FinPeriod.organizationID>>>>>,
						Where<FinPeriod.masterFinPeriodID, GreaterEqual<Required<FinPeriod.masterFinPeriodID>>>>
						.Update(this, finPeriodID);

						PXUpdate<
							Set<DRRevenueProjection.tranPTDRecognized, Zero,
							Set<DRRevenueProjection.tranPTDRecognizedSamePeriod, Zero,
							Set<DRRevenueProjection.tranPTDProjected, Zero>>>,
						DRRevenueProjection,
						Where<DRRevenueProjection.finPeriodID, GreaterEqual<Required<DRRevenueProjection.finPeriodID>>>>
						.Update(this, finPeriodID);

						if (Setup.Current.PendingRevenueValidate == true)
							PXDatabase.Update<DRSetup>(new PXDataFieldAssign<DRSetup.pendingRevenueValidate>(false));
					}
					else if (requiredModule == BatchModule.AP)
					{
						PXUpdateJoin<
							Set<DRExpenseBalance.begBalance, IsNull<DRExpenseBalance2.endBalance, Zero>,
							Set<DRExpenseBalance.begProjected, IsNull<DRExpenseBalance2.endProjected, Zero>,
							Set<DRExpenseBalance.pTDDeferred, Zero,
							Set<DRExpenseBalance.pTDRecognized, Zero,
							Set<DRExpenseBalance.pTDRecognizedSamePeriod, Zero,
							Set<DRExpenseBalance.pTDProjected, Zero,
							Set<DRExpenseBalance.endBalance, IsNull<DRExpenseBalance2.endBalance, Zero>,
							Set<DRExpenseBalance.endProjected, IsNull<DRExpenseBalance2.endProjected, Zero>>>>>>>>>,
						DRExpenseBalance,
						LeftJoin<Branch,
							On<DRExpenseBalance.branchID, Equal<Branch.branchID>>,
						LeftJoin<FinPeriod,
							On<DRExpenseBalance.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<Branch.organizationID, Equal<FinPeriod.organizationID>>>,
						LeftJoin<OrganizationFinPeriodExt,
							  On<OrganizationFinPeriodExt.masterFinPeriodID, Equal<Required<OrganizationFinPeriodExt.masterFinPeriodID>>,
							  And<Branch.organizationID, Equal<OrganizationFinPeriodExt.organizationID>>>,
						LeftJoin<DRExpenseBalanceByPeriod,
							On<DRExpenseBalanceByPeriod.branchID, Equal<DRExpenseBalance.branchID>,
							And<DRExpenseBalanceByPeriod.acctID, Equal<DRExpenseBalance.acctID>,
							And<DRExpenseBalanceByPeriod.subID, Equal<DRExpenseBalance.subID>,
							And<DRExpenseBalanceByPeriod.componentID, Equal<DRExpenseBalance.componentID>,
							And<DRExpenseBalanceByPeriod.vendorID, Equal<DRExpenseBalance.vendorID>,
							And<DRExpenseBalanceByPeriod.projectID, Equal<DRExpenseBalance.projectID>,
							And<DRExpenseBalanceByPeriod.finPeriodID, Equal<OrganizationFinPeriodExt.prevFinPeriodID>>>>>>>>,
						LeftJoin<DRExpenseBalance2,
							  On<DRExpenseBalance2.branchID, Equal<DRExpenseBalance.branchID>,
							 And<DRExpenseBalance2.acctID, Equal<DRExpenseBalance.acctID>,
							 And<DRExpenseBalance2.subID, Equal<DRExpenseBalance.subID>,
							 And<DRExpenseBalance2.componentID, Equal<DRExpenseBalance.componentID>,
							 And<DRExpenseBalance2.vendorID, Equal<DRExpenseBalance.vendorID>,
							 And<DRExpenseBalance2.projectID, Equal<DRExpenseBalance.projectID>,
							 And<DRExpenseBalance2.finPeriodID, Equal<DRExpenseBalanceByPeriod.lastActivityPeriod>>>>>>>>>>>>>,
						Where<FinPeriod.masterFinPeriodID, GreaterEqual<Required<FinPeriod.masterFinPeriodID>>>>
						.Update(this, finPeriodID, finPeriodID);

						PXUpdateJoin<
							Set<DRExpenseBalance.tranBegBalance, IsNull<DRExpenseBalance2.tranEndBalance, Zero>,
							Set<DRExpenseBalance.tranBegProjected, IsNull<DRExpenseBalance2.tranEndProjected, Zero>,
							Set<DRExpenseBalance.tranPTDDeferred, Zero,
							Set<DRExpenseBalance.tranPTDRecognized, Zero,
							Set<DRExpenseBalance.tranPTDRecognizedSamePeriod, Zero,
							Set<DRExpenseBalance.tranPTDProjected, Zero,
							Set<DRExpenseBalance.tranEndBalance, IsNull<DRExpenseBalance2.tranEndBalance, Zero>,
							Set<DRExpenseBalance.tranEndProjected, IsNull<DRExpenseBalance2.tranEndProjected, Zero>>>>>>>>>,
						DRExpenseBalance,
						LeftJoin<DRExpenseBalanceByPeriod,
							 On<DRExpenseBalanceByPeriod.branchID, Equal<DRExpenseBalance.branchID>,
							 And<DRExpenseBalanceByPeriod.acctID, Equal<DRExpenseBalance.acctID>,
							 And<DRExpenseBalanceByPeriod.subID, Equal<DRExpenseBalance.subID>,
							 And<DRExpenseBalanceByPeriod.componentID, Equal<DRExpenseBalance.componentID>,
							 And<DRExpenseBalanceByPeriod.vendorID, Equal<DRExpenseBalance.vendorID>,
							 And<DRExpenseBalanceByPeriod.projectID, Equal<DRExpenseBalance.projectID>,
							And<DRExpenseBalanceByPeriod.finPeriodID, Equal<Required<FinPeriod.masterFinPeriodID>>>>>>>>>,
						LeftJoin<DRExpenseBalance2,
							On<DRExpenseBalance2.branchID, Equal<DRExpenseBalance.branchID>,
							 And<DRExpenseBalance2.acctID, Equal<DRExpenseBalance.acctID>,
							 And<DRExpenseBalance2.subID, Equal<DRExpenseBalance.subID>,
							 And<DRExpenseBalance2.componentID, Equal<DRExpenseBalance.componentID>,
							 And<DRExpenseBalance2.vendorID, Equal<DRExpenseBalance.vendorID>,
							 And<DRExpenseBalance2.projectID, Equal<DRExpenseBalance.projectID>,
							 And<DRExpenseBalance2.finPeriodID, Equal<DRExpenseBalanceByPeriod.lastActivityPeriod>>>>>>>>>>,
						Where<DRExpenseBalance.finPeriodID, GreaterEqual<Required<DRExpenseBalance.finPeriodID>>>>
						.Update(this, prevPeriod?.FinPeriodID, finPeriodID);



						PXUpdateJoin<
							Set<DRExpenseProjection.pTDRecognized, Zero,
							Set<DRExpenseProjection.pTDRecognizedSamePeriod, Zero,
							Set<DRExpenseProjection.pTDProjected, Zero>>>,
						DRExpenseProjection,
						LeftJoin<Branch,
							On<DRExpenseProjection.branchID, Equal<Branch.branchID>>,
						LeftJoin<FinPeriod,
							On<DRExpenseProjection.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<Branch.organizationID, Equal<FinPeriod.organizationID>>>>>,
						Where<FinPeriod.masterFinPeriodID, GreaterEqual<Required<FinPeriod.masterFinPeriodID>>>>
						.Update(this, finPeriodID);

						PXUpdate<
							Set<DRExpenseProjection.tranPTDRecognized, Zero,
							Set<DRExpenseProjection.tranPTDRecognizedSamePeriod, Zero,
							Set<DRExpenseProjection.tranPTDProjected, Zero>>>,
						DRExpenseProjection,
						Where<DRExpenseProjection.finPeriodID, GreaterEqual<Required<DRExpenseProjection.finPeriodID>>>>
						.Update(this, finPeriodID);

						if (Setup.Current.PendingExpenseValidate == true)
							PXDatabase.Update<DRSetup>(new PXDataFieldAssign<DRSetup.pendingExpenseValidate>(false));
					}

					this.Actions.PressSave();
					transactionScope.Complete(this);
				}
			}
		}

		//BUGFIX: Presumably, the function never throws an exception
		private void ValidateFinPeriod(string finPeriodID, string requiredModule)
		{
			if (Setup.Current.PendingRevenueValidate == true && requiredModule == BatchModule.AR)
			{
				string minFinPeriod = finPeriodID;

				DRRevenueBalance dRRevenueBalance = (DRRevenueBalance)PXSelectOrderBy<
					DRRevenueBalance,
					OrderBy<
						Asc<DRRevenueBalance.finPeriodID>>>
					.SelectSingleBound(this, null);

				if (dRRevenueBalance != null && string.Compare(dRRevenueBalance.FinPeriodID, minFinPeriod) < 0)
					minFinPeriod = dRRevenueBalance.FinPeriodID;

				DRRevenueProjection dRRevenueProjection = (DRRevenueProjection)PXSelectOrderBy<
					DRRevenueProjection,
					OrderBy<
						Asc<DRRevenueProjection.finPeriodID>>>
					.SelectSingleBound(this, null);

				if (dRRevenueProjection != null && string.Compare(dRRevenueProjection.FinPeriodID, minFinPeriod) < 0)
					minFinPeriod = dRRevenueProjection.FinPeriodID;

				if (string.Compare(minFinPeriod, finPeriodID) < 0)
					throw new PXException(Messages.RevenueDeferralsHaveToBeValidated, FinPeriodIDFormattingAttribute.FormatForError(minFinPeriod));
			}

			if (Setup.Current.PendingExpenseValidate == true && requiredModule == BatchModule.AP)
			{
				string minFinPeriod = finPeriodID;

				DRExpenseBalance dRExpenseBalance = (DRExpenseBalance)PXSelectOrderBy<
						DRExpenseBalance,
						OrderBy<
							Asc<DRExpenseBalance.finPeriodID>>>
						.SelectSingleBound(this, null);

				if (dRExpenseBalance != null && string.Compare(dRExpenseBalance.FinPeriodID, minFinPeriod) < 0)
					minFinPeriod = dRExpenseBalance.FinPeriodID;

				DRExpenseProjection dRExpenseProjection = (DRExpenseProjection)PXSelectOrderBy<
					DRExpenseProjection,
					OrderBy<
						Asc<DRExpenseProjection.finPeriodID>>>
					.SelectSingleBound(this, null);

				if (dRExpenseProjection != null && string.Compare(dRExpenseProjection.FinPeriodID, minFinPeriod) < 0)
					minFinPeriod = dRExpenseProjection.FinPeriodID;

				if (string.Compare(minFinPeriod, finPeriodID) < 0)
					throw new PXException(Messages.ExpenseDeferralsHaveToBeValidated, FinPeriodIDFormattingAttribute.FormatForError(minFinPeriod));
			}
		}

		/// <summary>
		/// Encapsulates the information necessary to create
		/// a deferral schedule. Includes all fields from DRSchedule
		/// along with the necessary document transaction information.
		/// </summary>
		public class DRScheduleParameters : DRSchedule
		{
			public int? EmployeeID { get; set; }
			public int? SalesPersonID { get; set; }
			public int? SubID { get; set; }
		}

		/// <summary>
		/// Gets the deferral schedule parameters based on the 
		/// original document and document line transaction.
		/// </summary>
		public static DRScheduleParameters GetScheduleParameters(ARRegister document, ARTran tran)
		{
			return new DRScheduleParameters
			{
				Module = BatchModule.AR,
				DocType = tran.TranType,
				RefNbr = tran.RefNbr,
				LineNbr = tran.LineNbr,
				DocDate = document.DocDate,
				BAccountID = document.CustomerID,
				BAccountLocID = document.CustomerLocationID,
				FinPeriodID = tran.TranPeriodID,
				TranDesc = tran.TranDesc,
				ProjectID = tran.ProjectID,
				TaskID = tran.TaskID,

				EmployeeID = tran.EmployeeID,
				SalesPersonID = tran.SalesPersonID,
				SubID = tran.SubID,

				TermStartDate = tran.DRTermStartDate,
				TermEndDate = tran.DRTermEndDate
			};
		}

		/// <summary>
		/// Gets the deferral schedule parameters based on the 
		/// original document and document line transaction.
		/// </summary>
		public static DRScheduleParameters GetScheduleParameters(APRegister document, APTran tran)
		{
			return new DRScheduleParameters
			{
				Module = BatchModule.AP,
				DocType = tran.TranType,
				RefNbr = tran.RefNbr,
				LineNbr = tran.LineNbr,
				DocDate = document.DocDate,
				BAccountID = document.VendorID,
				BAccountLocID = document.VendorLocationID,
				FinPeriodID = tran.TranPeriodID,
				TranDesc = tran.TranDesc,
				ProjectID = tran.ProjectID,
				TaskID = tran.TaskID,

				EmployeeID = tran.EmployeeID,
				SubID = tran.SubID
			};
		}

		/// <param name="takeFromSales">
		/// If <c>true</c>, deferral transactions that are already posted will be handled.
		/// If <c>false</c>, only open deferral transactions will be used to create a related schedule.
		/// </param>
		private void CreateRelatedSchedule(DRScheduleParameters scheduleParameters, int? branchID, int? defScheduleID, decimal? tranAmt, DRDeferredCode defCode, InventoryItem inventoryItem, int? acctID, int? subID, bool isDraft, bool accountForPostedTransactions)
		{
			DRSchedule relatedSchedule = (this as IDREntityStorage).CreateCopy(scheduleParameters);
			relatedSchedule.IsDraft = isDraft;
			relatedSchedule.IsCustom = false;

			relatedSchedule = Schedule.Insert(relatedSchedule);

			DRSchedule originalDeferralSchedule = GetDeferralSchedule(defScheduleID);

			PXResultset<DRScheduleDetail> originalDetails = PXSelect<
				DRScheduleDetail, 
				Where<
					DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>
				.Select(this, originalDeferralSchedule.ScheduleID);

			decimal originalDetailsTotal = SumTotal(originalDetails);
			decimal adjustmentTotal = tranAmt.Value;

			List<DRScheduleDetail> nonResidualOriginalDetails = originalDetails
				.RowCast<DRScheduleDetail>()
				.Where(detail => detail.IsResidual != true)
				.ToList();

			decimal newDetailTotal = 0m;

			foreach (DRScheduleDetail originalDetail in nonResidualOriginalDetails)
			{
				decimal detailPartRaw = originalDetailsTotal == 0 ? 
					0 :
					originalDetail.CuryTotalAmt.Value * adjustmentTotal / originalDetailsTotal;

				decimal detailPart = PXDBCurrencyAttribute.BaseRound(this, detailPartRaw);

				decimal takeFromPostedRaw = 0;

				if (accountForPostedTransactions && originalDetail.CuryTotalAmt.Value != 0)
				{
					takeFromPostedRaw = 
						detailPartRaw * (originalDetail.CuryTotalAmt.Value - originalDetail.CuryDefAmt.Value) / originalDetail.CuryTotalAmt.Value;
				}

				decimal takeFromPosted = PXDBCurrencyAttribute.BaseRound(this, takeFromPostedRaw);

				decimal adjustmentDeferredAmountRaw = detailPartRaw - takeFromPosted;
				decimal adjustmentDeferredAmount = PXDBCurrencyAttribute.BaseRound(this, adjustmentDeferredAmountRaw);

				INComponent inventoryItemComponent = null;
				DRDeferredCode componentDeferralCode = null;

				if (inventoryItem != null)
				{
					inventoryItemComponent = GetInventoryItemComponent(inventoryItem.InventoryID, originalDetail.ComponentID);

					if (inventoryItemComponent != null)
					{
						componentDeferralCode = PXSelect<
							DRDeferredCode, 
							Where<
								DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>
							.Select(this, inventoryItemComponent.DeferredCode);
					}
				}
								
				InventoryItem component = GetInventoryItem(originalDetail.ComponentID);

				DRScheduleDetail relatedScheduleDetail;

				if (componentDeferralCode != null)
				{
					// Use component's deferral code
					// -
					relatedScheduleDetail = InsertScheduleDetail(
						branchID, 
						relatedSchedule, 
						inventoryItemComponent, 
						component, 
						componentDeferralCode, 
						detailPart, 
						originalDetail.DefAcctID, 
						originalDetail.DefSubID, 
						isDraft);                    
				}
				else
				{
					// Use deferral code and accounts from the document line
					// -
					relatedScheduleDetail = InsertScheduleDetail(
						branchID, 
						relatedSchedule, 
						component == null ? DRScheduleDetail.EmptyComponentID : component.InventoryID, 
						defCode, 
						detailPart, 
						originalDetail.DefAcctID, 
						originalDetail.DefSubID, 
						acctID, 
						subID, 
						isDraft);
				}

				newDetailTotal += detailPart;

				IList<DRScheduleTran> relatedTransactions = new List<DRScheduleTran>();
				DRDeferredCode relatedTransactionsDeferralCode = componentDeferralCode ?? defCode;

				IEnumerable<DRScheduleTran> originalPostedTransactions = null;

				if (accountForPostedTransactions)
				{
					originalPostedTransactions = PXSelect<
						DRScheduleTran,
						Where<
							DRScheduleTran.status, Equal<DRScheduleTranStatus.PostedStatus>,
							And<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
							And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
							And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>,
							And<DRScheduleTran.lineNbr, NotEqual<Required<DRScheduleTran.lineNbr>>>>>>>>
						.Select(
							this,
							originalDetail.ScheduleID,
							originalDetail.ComponentID,
							originalDetail.DetailLineNbr,
							originalDetail.CreditLineNbr)
						.RowCast<DRScheduleTran>();
				}

				if (adjustmentDeferredAmount != 0m
					|| accountForPostedTransactions && takeFromPosted != 0m)
				{
					string requiredTransactionStatus =
						relatedTransactionsDeferralCode.Method == DeferredMethodType.CashReceipt ?
							DRScheduleTranStatus.Projected :
							DRScheduleTranStatus.Open;

					IEnumerable<DRScheduleTran> originalOpenTransactions = PXSelect<
						DRScheduleTran,
						Where<
							DRScheduleTran.status, Equal<Required<DRScheduleTran.status>>,
							And<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
							And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
							And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>>>>>>
						.Select(
							this,
							requiredTransactionStatus,
							originalDetail.ScheduleID,
							originalDetail.ComponentID,
							originalDetail.DetailLineNbr)
						.RowCast<DRScheduleTran>();

					IList<DRScheduleTran> relatedDeferralTransactions = 
						GetTransactionsGenerator(relatedTransactionsDeferralCode).GenerateRelatedTransactions(
							relatedScheduleDetail,
							originalOpenTransactions,
							originalPostedTransactions,
							adjustmentDeferredAmount,
							takeFromPosted,
							branchID);

					foreach (DRScheduleTran deferralTransaction in relatedDeferralTransactions)
					{
						Transactions.Insert(deferralTransaction);
						relatedTransactions.Add(deferralTransaction);
					}
				}

				UpdateBalanceProjection(relatedTransactions, relatedScheduleDetail, defCode.AccountType);
			}

			DRScheduleDetail residualDetail = originalDetails
				.RowCast<DRScheduleDetail>()
				.FirstOrDefault(detail => detail.IsResidual == true);

			if (residualDetail != null)
			{
				decimal residualAmount = adjustmentTotal - newDetailTotal;

				InsertResidualScheduleDetail(
					relatedSchedule, 
					residualDetail, 
					residualAmount, 
					isDraft);
			}
		}

		

		protected void UpdateOriginalSchedule(ARTran tran, DRDeferredCode defCode, decimal amount, DateTime? docDate, string docFinPeriod, int? bAccountID, int? locationID)
		{
			DRSchedule origSchedule = GetDeferralSchedule(tran.DefScheduleID);

			DRScheduleDetail origDetail = PXSelect<DRScheduleDetail, 
				Where<
					DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>
				.Select(this, origSchedule.ScheduleID);

			decimal origTotalAmt = origDetail.CuryTotalAmt.Value;

			decimal adjustTotalAmt;
			decimal extra = 0;

			if (origTotalAmt >= 0 && origTotalAmt <= amount || origTotalAmt < 0 && origTotalAmt >= amount)
			{
				adjustTotalAmt = origTotalAmt;
				extra = amount - origTotalAmt;
			}
			else
			{
				adjustTotalAmt = amount;
			}

			decimal part = origDetail.CuryTotalAmt.Value * adjustTotalAmt / origTotalAmt;
			decimal takeFromSalesRaw = part * (origDetail.CuryTotalAmt.Value - origDetail.CuryDefAmt.Value) / origDetail.CuryTotalAmt.Value;
			decimal takeFromSales = PXDBCurrencyAttribute.BaseRound(this, takeFromSalesRaw);
			decimal partWithExtra = origDetail.CuryTotalAmt.Value * amount / origTotalAmt;
			decimal adjustDeferredAmountRaw = partWithExtra - takeFromSales;
			decimal adjustDeferredAmount = PXDBCurrencyAttribute.BaseRound(this, adjustDeferredAmountRaw);
			InventoryItem component = GetInventoryItem(origDetail.ComponentID);
			
			if (takeFromSales != 0)
			{
				DRScheduleTran nowTran = new DRScheduleTran();
				nowTran.BranchID = origDetail.BranchID;
				nowTran.AccountID = origDetail.AccountID;
				nowTran.SubID = origDetail.SubID;
				nowTran.Amount = -takeFromSales;
				nowTran.RecDate = this.Accessinfo.BusinessDate;
				nowTran.FinPeriodID = docFinPeriod;
				nowTran.ScheduleID = origDetail.ScheduleID;
				nowTran.ComponentID = origDetail.ComponentID;
				nowTran.DetailLineNbr = origDetail.DetailLineNbr;
				nowTran.Status = DRScheduleTranStatus.Open;

				Transactions.Insert(nowTran);
				UpdateBalanceProjection(nowTran, origDetail, defCode.AccountType);

				origDetail.CuryDefAmt -= takeFromSales;
			}

			if (adjustDeferredAmount != 0)
			{
				origDetail.CuryDefAmt -= adjustDeferredAmount;

				PXSelectBase<DRScheduleTran> projectedTranSelect = new
					PXSelect<DRScheduleTran, Where<DRScheduleTran.scheduleID, Equal<Required<DRSchedule.scheduleID>>,
					And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
					And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>,
					And<DRScheduleTran.status, Equal<DRScheduleTranStatus.ProjectedStatus>>>>>>(this);
				PXResultset<DRScheduleTran> projectedTrans = projectedTranSelect.Select(origDetail.ScheduleID, origDetail.ComponentID, origDetail.DetailLineNbr);

				decimal deltaRaw = adjustDeferredAmount / projectedTrans.Count;
				decimal delta = PXCurrencyAttribute.BaseRound(this, deltaRaw);

				foreach (DRScheduleTran dt in projectedTrans)
				{
					DRRevenueBalance histMin = new DRRevenueBalance();
					histMin.FinPeriodID = tran.FinPeriodID;
					histMin.BranchID = origDetail.BranchID;
					histMin.AcctID = origDetail.DefAcctID;
					histMin.SubID = origDetail.DefSubID;
					histMin.ComponentID = origDetail.ComponentID ?? 0;
					histMin.ProjectID = origDetail.ProjectID ?? 0;
					histMin.CustomerID = origDetail.BAccountID;
					histMin = RevenueBalance.Insert(histMin);
					histMin.PTDProjected -= dt.Amount;
					histMin.EndProjected += dt.Amount;

					DRRevenueProjectionAccum projMin = new DRRevenueProjectionAccum();
					projMin.FinPeriodID = tran.FinPeriodID;
					projMin.BranchID = origDetail.BranchID;
					projMin.AcctID = origDetail.AccountID;
					projMin.SubID = origDetail.SubID;
					projMin.ComponentID = origDetail.ComponentID ?? 0;
					projMin.ProjectID = origDetail.ProjectID ?? 0;
					projMin.CustomerID = origDetail.BAccountID;
					projMin = RevenueProjection.Insert(projMin);
					projMin.PTDProjected -= dt.Amount;
					
					dt.Amount -= delta;
					Transactions.Update(dt);

					DRRevenueBalance histPlus = new DRRevenueBalance();
					histPlus.FinPeriodID = tran.FinPeriodID;
					histPlus.BranchID = origDetail.BranchID;
					histPlus.AcctID = origDetail.DefAcctID;
					histPlus.SubID = origDetail.DefSubID;
					histPlus.ComponentID = origDetail.ComponentID ?? 0;
					histPlus.ProjectID = origDetail.ProjectID ?? 0;
					histPlus.CustomerID = origDetail.BAccountID;
					histPlus = RevenueBalance.Insert(histPlus);
					histPlus.PTDProjected += dt.Amount;
					histPlus.EndProjected -= dt.Amount;

					DRRevenueProjectionAccum projPlus = new DRRevenueProjectionAccum();
					projPlus.FinPeriodID = tran.FinPeriodID;
					projPlus.BranchID = origDetail.BranchID;
					projPlus.AcctID = origDetail.AccountID;
					projPlus.SubID = origDetail.SubID;
					projPlus.ComponentID = origDetail.ComponentID ?? 0;
					projPlus.ProjectID = origDetail.ProjectID ?? 0;
					projPlus.CustomerID = origDetail.BAccountID;
					projPlus = RevenueProjection.Insert(projPlus);
					projPlus.PTDProjected += dt.Amount;
				}
			}

			ScheduleDetail.Update(origDetail);

			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID =  docFinPeriod;
			hist.BranchID = origDetail.BranchID;
			hist.AcctID = origDetail.DefAcctID;
			hist.SubID = origDetail.DefSubID;
			hist.ComponentID = origDetail.ComponentID ?? 0;
			hist.ProjectID = origDetail.ProjectID ?? 0;
			hist.CustomerID = origDetail.BAccountID;

			hist = RevenueBalance.Insert(hist);
			hist.PTDDeferred -= amount;
			hist.EndBalance -= amount;
			
		}

		private decimal SumTotal(PXResultset<DRScheduleDetail> details)
		{
			decimal result = 0;

			foreach (DRScheduleDetail row in details)
				result += row.CuryTotalAmt.Value;

			return result;
		}

		private DRSchedule GetDeferralSchedule(int? scheduleID)
		{
			return PXSelect<
				DRSchedule, 
				Where<
					DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>
				.Select(this, scheduleID);
		}

		protected InventoryItem GetInventoryItem(int? inventoryItemID)
		{
			return PXSelect<
				InventoryItem, 
				Where<
					InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
				.Select(this, inventoryItemID);
		}

		protected INComponent GetInventoryItemComponent(int? inventoryID, int? componentID)
		{
			return PXSelect<
				INComponent, 
				Where<
					INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>,
					And<INComponent.componentID, Equal<Required<INComponent.componentID>>>>>
				.Select(this, inventoryID, componentID);
		}

		public PXResultset<DRScheduleDetail> GetScheduleDetailByOrigLineNbr(int? scheduleID, int? lineNbr)
		{
			return PXSelect<
				DRScheduleDetail,
				Where<
					DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
					And<DRScheduleDetail.lineNbr, Equal<Required<ARTran.lineNbr>>>>>
				.Select(this, scheduleID, lineNbr);
		}

		/// <summary>
		/// Retrieves all schedule details for the specified 
		/// deferral schedule ID.
		/// </summary>
		public IList<DRScheduleDetail> GetScheduleDetails(int? scheduleID)
		{
			return PXSelect<
				DRScheduleDetail,
				Where<
					DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>
				.Select(this, scheduleID)
				.RowCast<DRScheduleDetail>()
				.ToList();
		}

		public EPEmployee GetEmployee(int? employeeID)
		{
			return PXSelect<
				EPEmployee,
				Where<
					EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
				.Select(this, employeeID);
		}

		public Location GetLocation(int? businessAccountID, int? businessAccountLocationId)
		{
			return PXSelect<
				Location,
				Where<
					Location.bAccountID, Equal<Required<Location.bAccountID>>,
					And<Location.locationID, Equal<Required<Location.locationID>>>>>
				.Select(this, businessAccountID, businessAccountLocationId);
		}

		/// <summary>
		/// Generates and adds deferral transactions to the specified 
		/// deferral schedule and schedule detail.
		/// </summary>
		public IEnumerable<DRScheduleTran> GenerateAndAddDeferralTransactions(
			DRSchedule deferralSchedule, 
			DRScheduleDetail scheduleDetail, 
			DRDeferredCode deferralCode)
			=> GetTransactionsGenerator(deferralCode)
				.GenerateTransactions(deferralSchedule, scheduleDetail)
				.Select(transaction => Transactions.Insert(transaction))
				.ToArray();

		#region History & Accumulator functions
		protected void UpdateBalanceProjection(IEnumerable<DRScheduleTran> tranList, DRScheduleDetail sd, string deferredAccountType)
		{
			foreach (DRScheduleTran tran in tranList)
			{
				UpdateBalanceProjection(tran, sd, deferredAccountType);
			}
		}

		private void UpdateBalanceProjection(DRScheduleTran tran, DRScheduleDetail sd, string deferredAccountType)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					UpdateExpenseBalanceProjection(tran, sd);
					UpdateExpenseProjection(tran, sd);
					break;
				case DeferredAccountType.Income:
					UpdateRevenueBalanceProjection(tran, sd);
					UpdateRevenueProjection(tran, sd);
					break;

				default:
					throw new PXException(Messages.InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		private void UpdateRevenueBalanceProjection(DRScheduleTran tran, DRScheduleDetail scheduleDetail)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(scheduleDetail, tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(scheduleDetail, tran.TranPeriodID);

			if (IsReversed(scheduleDetail))
			{
				finHist.PTDProjected -= tran.Amount;
				finHist.EndProjected += tran.Amount;

				tranHist.TranPTDProjected -= tran.Amount;
				tranHist.TranEndProjected += tran.Amount;
			}
			else
			{
				finHist.PTDProjected += tran.Amount;
				finHist.EndProjected -= tran.Amount;

				tranHist.TranPTDProjected += tran.Amount;
				tranHist.TranEndProjected -= tran.Amount;
			}
		}

		private void UpdateExpenseBalanceProjection(DRScheduleTran tran, DRScheduleDetail scheduleDetail)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(scheduleDetail, tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(scheduleDetail, tran.TranPeriodID);

			finHist = ExpenseBalance.Insert(finHist);

			if (IsReversed(scheduleDetail))
			{
				finHist.PTDProjected -= tran.Amount;
				finHist.EndProjected += tran.Amount;

				tranHist.TranPTDProjected -= tran.Amount;
				tranHist.TranEndProjected += tran.Amount;
			}
			else
			{
				finHist.PTDProjected += tran.Amount;
				finHist.EndProjected -= tran.Amount;

				tranHist.TranPTDProjected += tran.Amount;
				tranHist.TranEndProjected -= tran.Amount;
			}
		}

		/// <summary>
		/// Using the information from the provided <see cref="DRScheduleTran"/> and <see cref="DRScheduleDetail"/> objects
		/// to update the deferral balances and projections.
		/// </summary>
		private void UpdateBalance(
			DRScheduleTran deferralTransaction,
			DRScheduleDetail deferralScheduleDetail,
			string deferredAccountType, 
			bool isIntegrityCheck = false)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					UpdateExpenseBalance(
						deferralTransaction, 
						deferralScheduleDetail, 
						updateEndProjected: isIntegrityCheck);

					UpdateExpenseProjectionUponRecognition(
						deferralTransaction, 
						deferralScheduleDetail,
						updatePTDRecognizedSamePeriod: isIntegrityCheck,
						updatePTDProjected: isIntegrityCheck);
					break;

				case DeferredAccountType.Income:
					UpdateRevenueBalance(
						deferralTransaction, 
						deferralScheduleDetail, 
						updateEndProjected: isIntegrityCheck);

					UpdateRevenueProjectionUponRecognition(
						deferralTransaction, 
						deferralScheduleDetail,
						updatePTDRecognizedSamePeriod: isIntegrityCheck,
						updatePTDProjected: isIntegrityCheck);
					break;

				default:
					throw new PXException(Messages.InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		/// <param name="updateEndProjected">
		/// A boolean flag indicating whether <see cref="DRRevenueBalance.EndProjected"/> should be updated.
		/// This can be required during the Balance Validation process.
		/// </param>
		private void UpdateRevenueBalance(DRScheduleTran tran, DRScheduleDetail scheduleDetail, bool updateEndProjected = false)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(scheduleDetail, tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(scheduleDetail, tran.TranPeriodID);

			if (IsReversed(scheduleDetail))
			{
				finHist.PTDRecognized -= tran.Amount;
				finHist.EndBalance += tran.Amount;

				tranHist.TranPTDRecognized -= tran.Amount;
				tranHist.TranEndBalance += tran.Amount;

				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod -= tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod -= tran.Amount;
				}

				if (updateEndProjected)
				{
					finHist.EndProjected += tran.Amount;
					tranHist.TranEndProjected += tran.Amount;
				}
			}
			else
			{
				finHist.PTDRecognized += tran.Amount;
				finHist.EndBalance -= tran.Amount;

				tranHist.TranPTDRecognized += tran.Amount;
				tranHist.TranEndBalance -= tran.Amount;

				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod += tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod += tran.Amount;
				}

				if (updateEndProjected)
				{
					finHist.EndProjected -= tran.Amount;
					tranHist.TranEndProjected -= tran.Amount;
				}
			}
		}

		/// <param name="updateEndProjected">
		/// A boolean flag indicating whether <see cref="DRExpenseBalance.EndProjected"/> balances should be updated. 
		/// This can be required during the Balance Validation process.
		/// </param>
		private void UpdateExpenseBalance(DRScheduleTran tran, DRScheduleDetail scheduleDetail, bool updateEndProjected = false)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(scheduleDetail, tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(scheduleDetail, tran.TranPeriodID);

			if (IsReversed(scheduleDetail))
			{
				finHist.PTDRecognized -= tran.Amount;
				finHist.EndBalance += tran.Amount;

				tranHist.TranPTDRecognized -= tran.Amount;
				tranHist.TranEndBalance += tran.Amount;

				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod -= tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod -= tran.Amount;
				}

				if (updateEndProjected)
				{
					finHist.EndProjected += tran.Amount;
					tranHist.TranEndProjected += tran.Amount;
				}
			}
			else
			{
				finHist.PTDRecognized += tran.Amount;
				finHist.EndBalance -= tran.Amount;

				tranHist.TranPTDRecognized += tran.Amount;
				tranHist.TranEndBalance -= tran.Amount;

				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod += tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod += tran.Amount;
				}

				if (updateEndProjected)
				{
					finHist.EndProjected -= tran.Amount;
					tranHist.TranEndProjected -= tran.Amount;
				}
			}
		}
				
		private void UpdateRevenueProjection(DRScheduleTran tran, DRScheduleDetail scheduleDetail)
		{
			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(scheduleDetail, tran.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(scheduleDetail, tran.TranPeriodID);

			if (IsReversed(scheduleDetail))
			{
				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod -= tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod -= tran.Amount;
				}
				finHist.PTDProjected -= tran.Amount;
				tranHist.TranPTDProjected -= tran.Amount;
			}
			else
			{
				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod += tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod += tran.Amount;
			}
				finHist.PTDProjected += tran.Amount;
				tranHist.TranPTDProjected += tran.Amount;
		}
		}

		private void UpdateExpenseProjection(DRScheduleTran tran, DRScheduleDetail scheduleDetail)
		{
			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(scheduleDetail, tran.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(scheduleDetail, tran.TranPeriodID);

			if (IsReversed(scheduleDetail))
			{
				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod -= tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod -= tran.Amount;
				}
				finHist.PTDProjected -= tran.Amount;
				tranHist.TranPTDProjected -= tran.Amount;
			}
			else
			{
				if (tran.FinPeriodID == scheduleDetail.FinPeriodID)
				{
					finHist.PTDRecognizedSamePeriod += tran.Amount;
					tranHist.TranPTDRecognizedSamePeriod += tran.Amount;
				}
				finHist.PTDProjected += tran.Amount;
				tranHist.TranPTDProjected += tran.Amount;
			}
		}

		/// <summary>
		/// Updates revenue projection history table to reflect the recognition of a deferral transaction.
		/// </summary>
		/// <param name="updatePTDRecognizedSamePeriod">
		/// A boolean flag indicating whether <see cref="DRRevenueProjection.PTDRecognizedSamePeriod"/> balances 
		/// should be updated. This can be required during the Balance Validation process.
		/// </param>
		/// <param name="updatePTDProjected">
		/// A boolean flag indicating whether <see cref="DRRevenueProjection.PTDProjected"/> balances
		/// should be updated. This can be required during the Balance Validation process, but undesirable
		/// during normal recognition.
		/// </param>
		private void UpdateRevenueProjectionUponRecognition(
			DRScheduleTran transaction, 
			DRScheduleDetail scheduleDetail, 
			bool updatePTDRecognizedSamePeriod = false,
			bool updatePTDProjected = false)
		{
			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(scheduleDetail, transaction.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(scheduleDetail, transaction.TranPeriodID);

			decimal? transactionAmount = IsReversed(scheduleDetail) ? -transaction.Amount : transaction.Amount;

			finHist.PTDRecognized += transactionAmount;
			tranHist.TranPTDRecognized += transactionAmount;

			if (updatePTDRecognizedSamePeriod && transaction.FinPeriodID == scheduleDetail.FinPeriodID)
			{
				finHist.PTDRecognizedSamePeriod += transactionAmount;
				tranHist.TranPTDRecognizedSamePeriod += transactionAmount;
			}

			if (updatePTDProjected)
			{
				finHist.PTDProjected += transactionAmount;
				tranHist.TranPTDProjected += transactionAmount;
			}
		}

		/// <summary>
		/// Updates expense projection history table to reflect the recognition of a deferral transaction.
		/// </summary>
		/// <param name="updatePTDRecognizedSamePeriod">
		/// A boolean flag indicating whether <see cref="DRRevenueProjection.PTDRecognizedSamePeriod"/> balances 
		/// should be updated. This can be required during the Balance Validation process.
		/// </param>
		/// <param name="updatePTDProjected">
		/// A boolean flag indicating whether <see cref="DRRevenueProjection.PTDProjected"/> balances
		/// should be updated. This can be required during the Balance Validation process, but undesirable
		/// during normal recognition.
		/// </param>
		private void UpdateExpenseProjectionUponRecognition(
			DRScheduleTran transaction, 
			DRScheduleDetail scheduleDetail, 
			bool updatePTDRecognizedSamePeriod = false,
			bool updatePTDProjected = false)
		{
			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(scheduleDetail, transaction.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(scheduleDetail, transaction.TranPeriodID);

			decimal? transactionAmount = IsReversed(scheduleDetail) ? -transaction.Amount : transaction.Amount;
			
			finHist.PTDRecognized += transactionAmount;
			tranHist.TranPTDRecognized += transactionAmount;

			if (updatePTDRecognizedSamePeriod && transaction.FinPeriodID == scheduleDetail.FinPeriodID)
			{
				finHist.PTDRecognizedSamePeriod += transactionAmount;
				tranHist.TranPTDRecognizedSamePeriod += transactionAmount;
			}

			if (updatePTDProjected)
			{
				finHist.PTDProjected += transactionAmount;
				tranHist.TranPTDProjected += transactionAmount;
			}
		}

		private void InitBalance(DRScheduleTran transaction, DRScheduleDetail scheduleDetail, string deferredAccountType)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					InitExpenseBalance(transaction, scheduleDetail);
					break;
				case DeferredAccountType.Income:
					InitRevenueBalance(transaction, scheduleDetail);
					break;
				default:
					throw new PXException(Messages.InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		private void InitRevenueBalance(DRScheduleTran transaction, DRScheduleDetail scheduleDetail)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(scheduleDetail, transaction.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(scheduleDetail, transaction.TranPeriodID);

			if (IsReversed(scheduleDetail))
			{
				finHist.PTDDeferred -= transaction.Amount;
				finHist.EndBalance -= transaction.Amount;
				finHist.EndProjected -= transaction.Amount;

				tranHist.TranPTDDeferred -= transaction.Amount;
				tranHist.TranEndBalance -= transaction.Amount;
				tranHist.TranEndProjected -= transaction.Amount;
			}
			else
			{
				finHist.PTDDeferred += transaction.Amount;
				finHist.EndBalance += transaction.Amount;
				finHist.EndProjected += transaction.Amount;

				tranHist.TranPTDDeferred += transaction.Amount;
				tranHist.TranEndBalance += transaction.Amount;
				tranHist.TranEndProjected += transaction.Amount;
			}
		}

		private void InitExpenseBalance(DRScheduleTran transaction, DRScheduleDetail scheduleDetail)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(scheduleDetail, transaction.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(scheduleDetail, transaction.TranPeriodID);

			if (IsReversed(scheduleDetail))
			{
				finHist.PTDDeferred -= transaction.Amount;
				finHist.EndBalance -= transaction.Amount;
				finHist.EndProjected -= transaction.Amount;

				tranHist.TranPTDDeferred -= transaction.Amount;
				tranHist.TranEndBalance -= transaction.Amount;
				tranHist.TranEndProjected -= transaction.Amount;
			}
			else
			{
				finHist.PTDDeferred += transaction.Amount;
				finHist.EndBalance += transaction.Amount;
				finHist.EndProjected += transaction.Amount;

				tranHist.TranPTDDeferred += transaction.Amount;
				tranHist.TranEndBalance += transaction.Amount;
				tranHist.TranEndProjected += transaction.Amount;
			}
		}

		public void Subtract(DRScheduleDetail scheduleDetail, DRScheduleTran transaction, string deferralCodeType)
		{
			Add(scheduleDetail, transaction, deferralCodeType, amountMultiplier: -1);
		}

		public void Add(
			DRScheduleDetail scheduleDetail, 
			DRScheduleTran transaction, 
			string deferralCodeType, 
			decimal amountMultiplier = 1m)
		{
			if (deferralCodeType == DeferredAccountType.Expense)
			{
				AddExpenseToProjection(transaction, scheduleDetail, amountMultiplier);
				AddExpenseToBalance(transaction, scheduleDetail, amountMultiplier);
			}
			else if (deferralCodeType == DeferredAccountType.Income)
			{
				AddRevenueToProjection(transaction, scheduleDetail, amountMultiplier);
				AddRevenueToBalance(transaction, scheduleDetail, amountMultiplier);
			}
			else
			{
				throw new PXArgumentException(
					Messages.InvalidAccountType,
					DeferredAccountType.Expense,
					DeferredAccountType.Income,
					deferralCodeType);
			}
		}

		private void AddRevenueToProjection(DRScheduleTran transaction, DRScheduleDetail scheduleDetail, decimal amountMultiplier)
		{
			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(scheduleDetail, transaction.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(scheduleDetail, transaction.TranPeriodID);

			finHist.PTDProjected += amountMultiplier * transaction.Amount;
			tranHist.TranPTDProjected += amountMultiplier * transaction.Amount;
		}

		private void AddExpenseToProjection(DRScheduleTran transaction, DRScheduleDetail scheduleDetail, decimal amountMultiplier)
		{
			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(scheduleDetail, transaction.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(scheduleDetail, transaction.TranPeriodID);

			finHist.PTDProjected += amountMultiplier * transaction.Amount;
			tranHist.TranPTDProjected += amountMultiplier * transaction.Amount;
		}

		private void AddRevenueToBalance(DRScheduleTran transaction, DRScheduleDetail scheduleDetail, decimal amountMultiplier)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(scheduleDetail, transaction.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(scheduleDetail, transaction.TranPeriodID);

			finHist.PTDProjected += amountMultiplier * transaction.Amount;
			finHist.EndProjected -= amountMultiplier * transaction.Amount;

			tranHist.TranPTDProjected += amountMultiplier * transaction.Amount;
			tranHist.TranEndProjected -= amountMultiplier * transaction.Amount;
		}

		private void AddExpenseToBalance(DRScheduleTran transaction, DRScheduleDetail scheduleDetail, decimal amountMultiplier)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(scheduleDetail, transaction.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(scheduleDetail, transaction.TranPeriodID);

			finHist.PTDProjected += amountMultiplier * transaction.Amount;
			finHist.EndProjected -= amountMultiplier * transaction.Amount;

			tranHist.TranPTDProjected += amountMultiplier * transaction.Amount;
			tranHist.TranEndProjected -= amountMultiplier * transaction.Amount;
		}
		#endregion

		protected DRScheduleDetail InsertScheduleDetail(int? branchID, DRSchedule sc, INComponent component, InventoryItem compItem, DRDeferredCode defCode, decimal amount, int? defAcctID, int? defSubID, bool isDraft)
		{
			int? acctID = sc.Module == BatchModule.AP ? compItem.COGSAcctID : component.SalesAcctID;
			int? subID = sc.Module == BatchModule.AP ? compItem.COGSSubID : component.SalesSubID;

			return InsertScheduleDetail(branchID, sc, compItem.InventoryID, defCode, amount, defAcctID, defSubID, acctID, subID, isDraft);
		}

		protected DRScheduleDetail InsertScheduleDetail(int? branchID, DRSchedule sc, int? componentID, DRDeferredCode defCode, decimal amount, int? defAcctID, int? defSubID, int? acctID, int? subID, bool isDraft)
		{
            DRScheduleDetail sd = new DRScheduleDetail();
			sd.ScheduleID = sc.ScheduleID;
			sd.BranchID = branchID;
			sd.ComponentID = componentID;
			sd.CuryTotalAmt = amount;
			sd.CuryDefAmt = amount;
			sd.DefCode = defCode.DeferredCodeID;
			sd.Status = DRScheduleStatus.Open;
			sd.IsOpen = true;
			sd.Module = sc.Module;
			sd.DocType = sc.DocType;
			sd.RefNbr = sc.RefNbr;
			sd.LineNbr = sc.LineNbr;
            FinPeriodIDAttribute.SetPeriodsByMaster<DRScheduleDetail.finPeriodID>(ScheduleDetail.Cache, sd, sc.FinPeriodID);
		    sd.BAccountID = sc.BAccountID;
			sd.AccountID = acctID;
			sd.SubID = subID;
			sd.DefAcctID = defAcctID;
			sd.DefSubID = defSubID;
			sd.CreditLineNbr = 0;
			sd.DocDate = sc.DocDate;
			sd.BAccountType = sc.Module == BatchModule.AP ? PX.Objects.CR.BAccountType.VendorType : PX.Objects.CR.BAccountType.CustomerType;
			sd = ScheduleDetail.Insert(sd);
			sd.Status = isDraft ? DRScheduleStatus.Draft : DRScheduleStatus.Open;
			sd.IsCustom = false;

			if (!isDraft)
			{
				//create credit line:
				CreateCreditLine(sd, defCode, branchID);
			}

			return sd;
		}

		private void InsertResidualScheduleDetail(DRSchedule schedule, DRScheduleDetail reidualDetail, decimal amount, bool isDraft)
		{
			DRScheduleDetail sd = new DRScheduleDetail();
			sd.ScheduleID = schedule.ScheduleID;
			sd.BranchID = reidualDetail.BranchID;
			sd.ComponentID = reidualDetail.ComponentID;
			sd.CuryTotalAmt = amount;
			sd.CuryDefAmt = 0.0m;
			sd.DefCode = null;
			sd.IsOpen = false;
			sd.CloseFinPeriodID = sd.FinPeriodID;
			sd.Module = schedule.Module;
			sd.DocType = schedule.DocType;
			sd.RefNbr = schedule.RefNbr;
			sd.LineNbr = schedule.LineNbr;
		    FinPeriodIDAttribute.SetPeriodsByMaster<DRScheduleDetail.finPeriodID>(ScheduleDetail.Cache, sd, schedule.FinPeriodID);
			sd.BAccountID = schedule.BAccountID;
			sd.AccountID = reidualDetail.AccountID;
			sd.SubID = reidualDetail.SubID;
			sd.DefAcctID = reidualDetail.AccountID;
			sd.DefSubID = reidualDetail.SubID;
			sd.CreditLineNbr = 0;
			sd.DocDate = schedule.DocDate;
			sd.BAccountType = schedule.Module == BatchModule.AP ? PX.Objects.CR.BAccountType.VendorType : PX.Objects.CR.BAccountType.CustomerType;

			sd.Status = isDraft ? DRScheduleStatus.Draft : DRScheduleStatus.Closed;
			sd.IsCustom = false;
			sd.IsResidual = true;

			ScheduleDetail.Insert(sd);
		}

		public void CreateCreditLine(DRScheduleDetail scheduleDetail, DRDeferredCode deferralCode, int? branchID)
		{
			DRScheduleTran creditLineTransaction = new DRScheduleTran
			{
				BranchID = branchID,
				AccountID = scheduleDetail.AccountID,
				SubID = scheduleDetail.SubID,
				Amount = scheduleDetail.CuryTotalAmt,
				RecDate = this.Accessinfo.BusinessDate,
				TranDate = this.Accessinfo.BusinessDate,
				FinPeriodID = scheduleDetail.FinPeriodID,
				LineNbr = 0,
				DetailLineNbr = scheduleDetail.DetailLineNbr,
				ScheduleID = scheduleDetail.ScheduleID,
				ComponentID = scheduleDetail.ComponentID,
				Status = DRScheduleTranStatus.Posted
			};

			creditLineTransaction = Transactions.Insert(creditLineTransaction);

			InitBalance(creditLineTransaction, scheduleDetail, deferralCode.AccountType);
		}

		/// <summary>
		/// Determines whether a given deferral schedule detail originates
		/// from a reversal document.
		/// </summary>
		private static bool IsReversed(DRScheduleDetail scheduleDetail)
		{
			return 
				scheduleDetail.DocType == ARDocType.CreditMemo || scheduleDetail.DocType == APDocType.DebitAdj || 
				scheduleDetail.DocType == ARDocType.CashReturn || scheduleDetail.DocType == APDocType.VoidQuickCheck;
		}

		private DRExpenseBalance CreateDRExpenseBalance(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.DefAcctID;
			hist.SubID = scheduleDetail.DefSubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.VendorID = scheduleDetail.BAccountID ?? 0;

			return ExpenseBalance.Insert(hist);
		}

		private DRRevenueBalance CreateDRRevenueBalance(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.DefAcctID;
			hist.SubID = scheduleDetail.DefSubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.CustomerID = scheduleDetail.BAccountID ?? 0;

			return RevenueBalance.Insert(hist);
		}

		private DRExpenseProjectionAccum CreateDRExpenseProjectionAccum(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.AccountID;
			hist.SubID = scheduleDetail.SubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.VendorID = scheduleDetail.BAccountID ?? 0;

			return ExpenseProjection.Insert(hist);
		}

		private DRRevenueProjectionAccum CreateDRRevenueProjectionAccum(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.AccountID;
			hist.SubID = scheduleDetail.SubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.CustomerID = scheduleDetail.BAccountID ?? 0;

			return RevenueProjection.Insert(hist);
		}

        #region Factory Methods

	    public virtual TransactionsGenerator GetTransactionsGenerator(DRDeferredCode deferralCode)
			=> new TransactionsGenerator(this, deferralCode);

		#endregion

		#region Explicit Interface Implementation

		SalesPerson IBusinessAccountProvider.GetSalesPerson(int? salesPersonID)
		{
			return PXSelect<
				SalesPerson,
				Where<
					SalesPerson.salesPersonID, Equal<Required<SalesPerson.salesPersonID>>>>
				.Select(this, salesPersonID);
		}

		IEnumerable<InventoryItemComponentInfo> IInventoryItemProvider.GetInventoryItemComponents(
			int? inventoryItemID,
			string requiredAllocationMethod)
		{
			bool hasDeferralCode = requiredAllocationMethod != INAmountOption.Residual;

			if (hasDeferralCode)
			{
				return PXSelectJoin<
					INComponent,
						InnerJoin<DRDeferredCode, On<INComponent.deferredCode, Equal<DRDeferredCode.deferredCodeID>>,
						InnerJoin<InventoryItem, On<INComponent.componentID, Equal<InventoryItem.inventoryID>>>>,
					Where<
						INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>,
						And<INComponent.amtOption, Equal<Required<INComponent.amtOption>>>>>
					.Select(this, inventoryItemID, requiredAllocationMethod).AsEnumerable()
					.Cast<PXResult<INComponent, DRDeferredCode, InventoryItem>>()
					.Select(result => new InventoryItemComponentInfo
					{
						Component = result,
						Item = result,
						DeferralCode = result,
					});
			}
			else
			{
				return PXSelectJoin<
					INComponent,
						InnerJoin<InventoryItem, On<INComponent.componentID, Equal<InventoryItem.inventoryID>>>,
					Where<
						INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>,
						And<INComponent.amtOption, Equal<Required<INComponent.amtOption>>>>>
					.Select(this, inventoryItemID, requiredAllocationMethod).AsEnumerable()
					.Cast<PXResult<INComponent, InventoryItem>>()
					.Select(result => new InventoryItemComponentInfo
					{
						Component = result,
						Item = result,
						DeferralCode = null,
					});
			}
		}

		string IInventoryItemProvider.GetComponentName(INComponent component)
			=> this.Caches[typeof(INComponent)].GetValueExt<INComponent.componentID>(component) as string;

		class APSubaccountProvider : SubaccountProviderBase
		{
			public APSubaccountProvider(PXGraph graph) : base(graph)
			{ }

			public override string MakeSubaccount<Field>(string mask, object[] sourceFieldValues, Type[] sourceFields)
			{
				return SubAccountMaskAPAttribute.MakeSub<Field>(
					_graph,
					mask,
					sourceFieldValues,
					sourceFields);
			}
		}

		protected class ARSubaccountProvider : SubaccountProviderBase
		{
			public ARSubaccountProvider(PXGraph graph) : base(graph)
			{ }

			public override string MakeSubaccount<Field>(string mask, object[] sourceFieldValues, Type[] sourceFields)
			{
				return SubAccountMaskARAttribute.MakeSub<Field>(
					_graph,
					mask,
					sourceFieldValues,
					sourceFields);
			}
		}

		DRSchedule IDREntityStorage.CreateCopy(DRSchedule originalSchedule)
			=> Schedule.Cache.CreateCopy(originalSchedule) as DRSchedule;

		DRScheduleTran IDREntityStorage.CreateCopy(DRScheduleTran originalTransaction)
			=> Transactions.Cache.CreateCopy(originalTransaction) as DRScheduleTran;

		IList<DRScheduleTran> IDREntityStorage.GetDeferralTransactions(int? scheduleID, int? componentID, int? detailLineNbr)
		{
			return PXSelect<
				DRScheduleTran,
				Where<
					DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
					And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
					And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>>>>>
				.Select(this, scheduleID, componentID, detailLineNbr)
				.RowCast<DRScheduleTran>()
				.ToList();
		}

		DRDeferredCode IDREntityStorage.GetDeferralCode(string deferralCodeID)
		{
			return PXSelect<
				DRDeferredCode,
				Where<
					DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>
				.Select(this, deferralCodeID);
		}

		DRSchedule IDREntityStorage.Insert(DRSchedule schedule)
			=> this.Schedule.Insert(schedule);

		DRSchedule IDREntityStorage.Update(DRSchedule schedule)
			=> this.Schedule.Update(schedule);

		DRScheduleDetail IDREntityStorage.Insert(DRScheduleDetail scheduleDetail)
			=> this.ScheduleDetail.Insert(scheduleDetail);

		DRScheduleDetail IDREntityStorage.Update(DRScheduleDetail scheduleDetail)
			=> this.ScheduleDetail.Update(scheduleDetail);

		void IDREntityStorage.ScheduleTransactionModified(
			DRScheduleDetail scheduleDetail,
			DRDeferredCode deferralCode, 
			DRScheduleTran oldTransaction, 
			DRScheduleTran newTransaction)
		{
			Subtract(scheduleDetail, oldTransaction, deferralCode.AccountType);
			Add(scheduleDetail, newTransaction, deferralCode.AccountType);

			Transactions.Update(newTransaction);
		}

		IEnumerable<DRScheduleTran> IDREntityStorage.CreateDeferralTransactions(
			DRSchedule deferralSchedule, 
			DRScheduleDetail scheduleDetail,
			DRDeferredCode deferralCode, 
			int? branchID) 
			=> GenerateAndAddDeferralTransactions(deferralSchedule, scheduleDetail, deferralCode);

		void IDREntityStorage.CreateCreditLineTransaction(
			DRScheduleDetail scheduleDetail,
			DRDeferredCode deferralCode,
			int? branchID) =>
				this.CreateCreditLine(scheduleDetail, deferralCode, branchID);

		void IDREntityStorage.NonDraftDeferralTransactionsPrepared(
			DRScheduleDetail scheduleDetail, 
			DRDeferredCode deferralCode, 
			IEnumerable<DRScheduleTran> deferralTransactions)
			=> UpdateBalanceProjection(deferralTransactions, scheduleDetail, deferralCode.AccountType);

		private DRScheduleDetail GetScheduleDetailForComponent(int? scheduleID, int? componentID, int? detailLineNbr)
		{
			return PXSelect<
				DRScheduleDetail,
				Where<
					DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
					And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>,
					And<DRScheduleDetail.detailLineNbr, Equal<Required<DRScheduleDetail.detailLineNbr>>>>>>
				.Select(this, scheduleID, componentID, detailLineNbr);
		}
		#endregion
	}
}
