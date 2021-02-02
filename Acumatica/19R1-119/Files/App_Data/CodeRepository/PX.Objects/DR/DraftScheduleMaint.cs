using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Diagnostics;

using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.CS;
using PX.Objects.CM;

namespace PX.Objects.DR
{
	[Serializable]
	public class DraftScheduleMaint : PXGraph<DraftScheduleMaint, DRSchedule>
	{
		#region internal types
		public enum Context
		{
			Normal,
			Recalculate
		}
		#endregion

		#region Views

		public PXSelect<DRSchedule> Schedule;
		public PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRSchedule.scheduleID>>>> DocumentProperties;

		public PXSelect<
			DRScheduleDetail,
			Where<
				DRScheduleDetail.scheduleID, Equal<Current<DRSchedule.scheduleID>>,
				And<DRScheduleDetail.isResidual, Equal<False>>>>
			Components;

		public PXSelectJoin<
			ARTran,
			InnerJoin<InventoryItem,
				On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>>,
			LeftJoin<INComponent,
				On<InventoryItem.inventoryID, Equal<INComponent.inventoryID>>,
			LeftJoin<DRScheduleDetail,
				On<ARTran.tranType, Equal<DRScheduleDetail.docType>,
				And<ARTran.refNbr, Equal<DRScheduleDetail.refNbr>,
				And<ARTran.lineNbr, Equal<DRScheduleDetail.lineNbr>,
				And<Where<INComponent.componentID, Equal<DRScheduleDetail.componentID>,
				Or<InventoryItem.inventoryID, Equal<DRScheduleDetail.componentID>>>>>>>>>>,
			Where<DRScheduleDetail.scheduleID, Equal<Current<DRSchedule.scheduleID>>,
				And<ARTran.tranType, Equal<Current<DRSchedule.docType>>,
				And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
				And<ARTran.deferredCode, IsNotNull,
				And<Current<DRSchedule.module>, Equal<BatchModule.moduleAR>,
				And<Current<DRSchedule.refNbr>, IsNotNull,
				And<Current<DRSchedule.lineNbr>, IsNull,
				And<Current<DRSchedule.isOverridden>, NotEqual<True>>>>>>>>>,
			OrderBy<Asc<DRScheduleDetail.detailLineNbr>>>
			ReallocationPool;

		public PXSelectJoin<
			DRScheduleDetail,
			LeftJoin<ARTran,
				On<DRScheduleDetail.docType, Equal<ARTran.tranType>,
				And<DRScheduleDetail.refNbr, Equal<ARTran.refNbr>,
				And<DRScheduleDetail.lineNbr, Equal<ARTran.lineNbr>>>>,
			LeftJoin<InventoryItem,
				On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>>,
			LeftJoin<INComponent,
				On<DRScheduleDetail.componentID, Equal<INComponent.componentID>,
					And<InventoryItem.inventoryID, Equal<INComponent.inventoryID>>>>>>,
			Where<DRScheduleDetail.scheduleID, Equal<Current<DRSchedule.scheduleID>>,
				And<ARTran.tranType, Equal<Current<DRSchedule.docType>>,
				And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
				And<ARTran.deferredCode, IsNotNull,
				And<Current<DRSchedule.module>, Equal<BatchModule.moduleAR>,
				And<Current<DRSchedule.refNbr>, IsNotNull,
				And<Current<DRSchedule.lineNbr>, IsNull,
				And<Current<DRSchedule.isOverridden>, NotEqual<True>>>>>>>>>,
			OrderBy<Asc<DRScheduleDetail.detailLineNbr>>>
			ReallocationPool_DR;

		public PXSelect<
			DRScheduleDetail,
			Where<
				DRScheduleDetail.scheduleID, Equal<Current<DRSchedule.scheduleID>>,
				And<DRScheduleDetail.isResidual, Equal<True>>>>
			ResidualComponent;

		[PXImport(typeof(DRSchedule))]
		public PXSelect<
			DRScheduleTran,
			Where<DRScheduleTran.scheduleID, Equal<Optional<DRScheduleDetail.scheduleID>>,
				And<DRScheduleTran.componentID, Equal<Optional<DRScheduleDetail.componentID>>,
				And<DRScheduleTran.detailLineNbr, Equal<Optional<DRScheduleDetail.detailLineNbr>>,
				And<DRScheduleTran.lineNbr, NotEqual<Optional<DRScheduleDetail.creditLineNbr>>>>>>> Transactions;
		public PXSelect<
			DRScheduleTran,
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>,
				And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
				And<DRScheduleTran.detailLineNbr, Equal<Current<DRScheduleDetail.detailLineNbr>>,
				And<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>,
				And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>>>> OpenTransactions;
		public PXSelect<
			DRScheduleTran,
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>,
				And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
				And<DRScheduleTran.detailLineNbr, Equal<Current<DRScheduleDetail.detailLineNbr>>,
				And<DRScheduleTran.status, Equal<DRScheduleTranStatus.ProjectedStatus>,
				And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>>>> ProjectedTransactions;
		public PXSelect<
			DRDeferredCode,
			Where<DRDeferredCode.deferredCodeID, Equal<Current<DRScheduleDetail.defCode>>>> DeferredCode;
		public PXSelect<DRExpenseBalance> ExpenseBalance;
		public PXSelect<DRExpenseProjectionAccum> ExpenseProjection;
		public PXSelect<DRRevenueBalance> RevenueBalance;
		public PXSelect<DRRevenueProjectionAccum> RevenueProjection;
		public PXSelectReadonly<DRScheduleEx> Associated;
		public PXSelect<CM.CurrencyInfo, Where<CM.CurrencyInfo.curyInfoID, Equal<Current<DRSchedule.curyInfoID>>>> CurrencyInfo;

		public PXSelectJoin<
			DRScheduleDetail,
			InnerJoin<DRDeferredCode, On<DRScheduleDetail.defCode, Equal<DRDeferredCode.deferredCodeID>>>,
			Where2<
				Where<DRScheduleDetail.scheduleID, Equal<Current<DRSchedule.scheduleID>>>,
				And<Where<
					DRDeferredCode.method, Equal<DeferredMethodType.flexibleExactDays>,
					Or<DRDeferredCode.method, Equal<DeferredMethodType.flexibleProrateDays>>>>>> ComponentsWithFlexibleCodes;

		public PXSetup<DRSetup> Setup;

		[PXHidden]
		public PXSelect<INComponent> _dummyINComponent;
		[PXHidden]
		public PXSelect<ARTran> _dummyARTran;

		#endregion

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }
		public Context CurrentContext { get; set; }

		public DraftScheduleMaint()
		{
			DRSetup setup = Setup.Current;
			Transactions.Cache.AllowInsert = false;

			var transImporter = Transactions.GetAttribute<PXImportAttribute>();

			if (transImporter != null)
			{
				transImporter.RowImporting += DRScheduleTranRowImporting;
			}
		}

		public virtual IEnumerable associated([PXDBString] string scheduleNbr)
		{
			IEnumerable emptyScheduleList = new List<DRScheduleEx>();

			if (scheduleNbr == null) return emptyScheduleList;

			DRSchedule deferralSchedule = PXSelect<
				DRSchedule,
				Where<
					DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>
				.Select(this);

			if (deferralSchedule == null) return emptyScheduleList;

			if (deferralSchedule.Module == BatchModule.AR)
			{
				if (deferralSchedule.DocType == ARDocType.CreditMemo)
				{
					ARTran documentLine = PXSelect<
						ARTran,
						Where<
							ARTran.tranType, Equal<Current<DRScheduleEx.docType>>,
							And<ARTran.refNbr, Equal<Current<DRScheduleEx.refNbr>>,
							And<ARTran.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>
						.Select(this, deferralSchedule.LineNbr);

					if (documentLine != null)
					{
						return PXSelect<
							DRScheduleEx,
							Where<
								DRScheduleEx.scheduleID,
								Equal<Required<DRScheduleEx.scheduleID>>>>
							.Select(this, documentLine.DefScheduleID);
					}
				}
				else if (deferralSchedule.DocType == ARDocType.Invoice || deferralSchedule.DocType == ARDocType.DebitMemo)
				{
					List<DRScheduleEx> list = new List<DRScheduleEx>();

					foreach (ARTran documentLine in PXSelect<
						ARTran,
						Where<
							ARTran.defScheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>
						.Select(this))
					{
						DRScheduleEx relatedSchedule = PXSelect<
							DRScheduleEx,
							Where<
								DRScheduleEx.module, Equal<BatchModule.moduleAR>,
								And<DRScheduleEx.docType, Equal<Required<ARTran.tranType>>,
								And<DRScheduleEx.refNbr, Equal<Required<ARTran.refNbr>>,
								And<DRSchedule.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>>
							.Select(this, documentLine.TranType, documentLine.RefNbr, documentLine.LineNbr);

						if (relatedSchedule != null)
						{
							list.Add(relatedSchedule);
						}
					}

					return list;
				}
			}
			else if (deferralSchedule.Module == BatchModule.AP)
			{
				if (deferralSchedule.DocType == APDocType.DebitAdj)
				{
					APTran documentLine = PXSelect<
						APTran,
						Where<
							APTran.tranType, Equal<Current<DRScheduleDetail.docType>>,
							And<APTran.refNbr, Equal<Current<DRScheduleDetail.refNbr>>,
							And<APTran.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>
						.Select(this, deferralSchedule.LineNbr);

					if (documentLine != null)
					{
						return PXSelect<
							DRScheduleEx,
							Where<
								DRScheduleEx.scheduleID, Equal<Required<DRScheduleEx.scheduleID>>>>
							.Select(this, documentLine.DefScheduleID);
					}
				}
				else if (deferralSchedule.DocType == APDocType.Invoice || deferralSchedule.DocType == APDocType.CreditAdj)
				{
					List<DRScheduleEx> list = new List<DRScheduleEx>();

					foreach (APTran documentLine in PXSelect<
						APTran,
						Where<
							APTran.defScheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>
						.Select(this))
					{
						DRScheduleEx relatedSchedule = PXSelect<
							DRScheduleEx,
							Where<
								DRScheduleEx.module, Equal<BatchModule.moduleAP>,
								And<DRScheduleEx.docType, Equal<Required<APTran.tranType>>,
								And<DRScheduleEx.refNbr, Equal<Required<APTran.refNbr>>,
								And<DRSchedule.lineNbr, Equal<Required<APTran.lineNbr>>>>>>>
							.Select(this, documentLine.TranType, documentLine.RefNbr, documentLine.LineNbr);

						list.Add(relatedSchedule);
					}

					return list;
				}
			}

			return emptyScheduleList;
		}

		public virtual IEnumerable reallocationPool()
		{
			if(Schedule.Current.IsRecalculated == true && Schedule.Current.IsOverridden == false)
			{
				return reallocationPoolDR();
			}

			return ReallocationPool.View.QuickSelect();
		}

		private IEnumerable reallocationPoolDR()
		{
			foreach (PXResult<DRScheduleDetail, ARTran, InventoryItem, INComponent> result in
							ReallocationPool_DR.View.QuickSelect())
			{
				ARTran arTran = result;
				InventoryItem inventoryItem = result;
				INComponent inComponent = result;
				DRScheduleDetail drScheduleDetail = result;

				yield return new PXResult<ARTran, InventoryItem, INComponent, DRScheduleDetail>
					(arTran, inventoryItem, inComponent, drScheduleDetail);
			}
		}

		#region Actions/Buttons

		public PXAction<DRSchedule> viewDoc;
		[PXUIField(DisplayName = Messages.ViewDocument, Enabled = false)]
		[PXButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			if (Schedule.Current != null)
			{
				DRRedirectHelper.NavigateToOriginalDocument(this, Schedule.Current);
			}
			return adapter.Get();
		}

		public PXAction<DRSchedule> viewSchedule;
		[PXUIField(DisplayName = Messages.ViewSchedule)]
		[PXButton]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			if (Associated.Current != null)
			{
				DraftScheduleMaint target = PXGraph.CreateInstance<DraftScheduleMaint>();
				target.Clear();
				target.Schedule.Current = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, Associated.Current.ScheduleID);
				throw new PXRedirectRequiredException(target, "View Referenced Schedule");
			}
			return adapter.Get();
		}

		public PXAction<DRSchedule> viewBatch;
		[PXUIField(DisplayName = "View GL Batch")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			JournalEntry target = PXGraph.CreateInstance<JournalEntry>();
			target.Clear();
			Batch batch = PXSelect<Batch, Where<Batch.module, Equal<BatchModule.moduleDR>, And<Batch.batchNbr, Equal<Current<DRScheduleTran.batchNbr>>>>>.Select(this);
			if (batch != null)
			{
				target.BatchModule.Current = batch;
				throw new PXRedirectRequiredException(target, "ViewBatch"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}

			return adapter.Get();
		}

		public PXAction<DRSchedule> release;
		[PXUIField(DisplayName = "Release", Enabled = false)]
		[PXButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			List<DRSchedule> scheduleList = adapter
				.Get()
				.Cast<DRSchedule>()
				.Where(schedule => schedule.IsDraft == true)
				.ToList();

			if (scheduleList.Any())
			{
				Save.Press();
				PXLongOperation.StartOperation(this, () => ScheduleMaint.ReleaseCustomSchedules(scheduleList));
			}
			else throw new PXException(Messages.ScheduleAlreadyReleased);

			return scheduleList;
		}

		public PXAction<DRSchedule> recalculate;
		[PXUIField(DisplayName = Messages.Recalculate, Visible = false, Enabled = false)]
		[PXButton]
		public virtual IEnumerable Recalculate(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<DRSchedule> generateTransactions;
		[PXUIField(DisplayName = Messages.GenerateTransactions, Enabled = false)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable GenerateTransactions(PXAdapter adapter)
		{
			if (Components.Current != null)
			{
				DRDeferredCode deferralCode = DeferredCode.Select();

				if (deferralCode != null)
				{
					var scheduleTransactions = Transactions.Select().AsEnumerable();

					if (!scheduleTransactions.Any())
					{
						CreateTransactions(deferralCode);
					}
					else
					{
						if (Components.Current.Status != DRScheduleStatus.Draft &&
							scheduleTransactions
								.RowCast<DRScheduleTran>()
								.Any(t => t.Status == DRScheduleTranStatus.Posted))
						{
							throw new PXException(Messages.CantRegeneratePostedTransactions);
						}

						WebDialogResult result = Components.View.Ask(
							Components.Current,
							GL.Messages.Confirmation,
							Messages.RegenerateTran,
							MessageButtons.YesNo,
							MessageIcon.Question);

						if (result == WebDialogResult.Yes)
						{
							CreateTransactions(deferralCode);
						}
					}
				}
			}
			return adapter.Get();
		}

		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void DRScheduleDetail_ComponentID_CacheAttached(PXCache sender) { }
		#endregion

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == nameof(Schedule) && 
				keys[nameof(DRSchedule.ScheduleID)] == null)
			{
				foreach (DRSchedule insertedSchedule in Schedule.Cache.Inserted)
				{
					keys[nameof(DRSchedule.ScheduleID)] = insertedSchedule.ScheduleID;
				}
			}

			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		#region Helper Methods

		/// <summary>
		/// Using the document type and reference number specified in the
		/// provided schedule object, searches for the existing AR / AP
		/// document. If it exists, returns the business account ID
		/// that is specified in the document.
		/// </summary>
		private int? GetDocumentBusinessAccountID(DRSchedule schedule)
	    {
		    int? documentBusinessAccountID = null;

		    if (schedule.Module == BatchModule.AR)
		    {
			    ARInvoice invoice = PXSelect<
				    ARInvoice,
				    Where<
					    ARInvoice.docType, Equal<Current<DRSchedule.docType>>,
					    And<ARInvoice.refNbr, Equal<Current<DRSchedule.refNbr>>>>>
				    .Select(this);

			    documentBusinessAccountID = invoice?.CustomerID;
		    }
		    else if (schedule.Module == BatchModule.AP)
		    {
			    APInvoice bill = PXSelect<
				    APInvoice,
				    Where<
					    APInvoice.docType, Equal<Current<DRSchedule.docType>>,
					    And<APInvoice.refNbr, Equal<Current<DRSchedule.refNbr>>>>>
				    .Select(this);

			    documentBusinessAccountID = bill?.VendorID;
		    }

		    return documentBusinessAccountID;
	    }

		/// <summary>
		/// Checks that there are no other deferral schedules with the same
		/// combination of document type, reference number, and line number.
		/// If there are such schedules, throws an error.
		/// </summary>
		/// <param name="sender">A cache of <see cref="DRSchedule"/> objects.</param>
		/// <param name="deferralSchedule">The deferral schedule record to validate.</param>
		/// <param name="lineNumberToCheck">
		/// Optional line number to check. May be different from the line 
		/// number in the <paramref name="deferralSchedule"/> object
		/// in case when this method is called e.g. from within the
		/// FieldUpdating event. If <c>null</c>, then the line number 
		/// value held by <paramref name="deferralSchedule"/> will be used.
		/// </param>
	    private void VerifyNoDuplicateSchedules(
			PXCache sender, 
			DRSchedule deferralSchedule, 
			int? lineNumberToCheck = null)
		{
			lineNumberToCheck = lineNumberToCheck ?? deferralSchedule.LineNbr;

			DRSchedule duplicateSchedule = PXSelect<
				DRSchedule,
				Where<
					DRSchedule.module, Equal<Required<DRSchedule.module>>,
					And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
					And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
					And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>,
					And<DRSchedule.scheduleID, NotEqual<Required<DRSchedule.scheduleID>>>>>>>>
				.Select(
					this, 
					deferralSchedule.Module,
					deferralSchedule.DocType, 
					deferralSchedule.RefNbr, 
					lineNumberToCheck, 
					deferralSchedule.ScheduleID);

			if (duplicateSchedule != null)
			{
				PXSetPropertyException setPropertyException = new PXSetPropertyException(
					Messages.DuplicateSchedule,
					duplicateSchedule.ScheduleNbr);

				if (sender.RaiseExceptionHandling<DRSchedule.lineNbr>(
					deferralSchedule, 
					lineNumberToCheck, 
					setPropertyException))
				{
					throw setPropertyException;
				}
			}
		}

		/// <summary>
		/// When both document reference number and line number
		/// are specified, checks that there exists a document line 
		/// with such number. Note that this method does not throw an 
		/// error if the line number is <c>null</c> -- use a separate 
		/// check if needed.
		/// </summary>
		/// <param name="sender">A cache of <see cref="DRSchedule"/> objects.</param>
		/// <param name="deferralSchedule">The deferral schedule record to validate.</param>
		/// <param name="lineNumberToCheck">
		/// Optional line number to check. May be different from the line 
		/// number in the <paramref name="deferralSchedule"/> object
		/// in case when this method is called e.g. from within the
		/// FieldUpdating event. If <c>null</c>, then the line number 
		/// value held by <paramref name="deferralSchedule"/> will be used.
		/// </param>
		private void VerifyDocumentLineExists(
		    PXCache sender,
			DRSchedule deferralSchedule,
		    int? lineNumberToCheck = null)
	    {
			lineNumberToCheck = lineNumberToCheck ?? deferralSchedule.LineNbr;

			if (!lineNumberToCheck.HasValue) return;

			BqlCommand documentQuery;

			if (deferralSchedule.Module == BatchModule.AP)
			{
				documentQuery = BqlCommand.CreateInstance(typeof(Select<
					APTran,
					Where<
						APTran.refNbr, Equal<Required<APTran.refNbr>>,
						And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>));
			}
			else if (deferralSchedule.Module == BatchModule.AR)
			{
				documentQuery = BqlCommand.CreateInstance(typeof(Select<
					ARTran,
					Where<
						ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
						And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>));
			}
			else
			{
				throw new PXException(Messages.UnexpectedModuleSpecified);
			}

			if (new PXView(this, true, documentQuery)
					.SelectSingle(deferralSchedule.RefNbr, lineNumberToCheck) == null)
			{
				PXSetPropertyException setPropertyException = new PXSetPropertyException(
					Messages.NonExistentLineNumber);

				if (sender.RaiseExceptionHandling<DRSchedule.lineNbr>(
					deferralSchedule, 
					lineNumberToCheck, 
					setPropertyException))
				{
					throw setPropertyException;
				}
			}
		}

		protected void FillBranch(DRScheduleDetail scheduleDetail)
		{
			if (Schedule.Current == null) return;

			if (Schedule.Current.RefNbr == null)
			{
				if(scheduleDetail.BranchID == null)
					scheduleDetail.BranchID = Accessinfo.BranchID;
				return;
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>()
				&& Schedule.Current.IsCustom == true
				&& Schedule.Current.RefNbr != null)
			{
				if (scheduleDetail.BranchID == null)
					scheduleDetail.BranchID = Accessinfo.BranchID;
				return;
			}

			if (Schedule.Current.Module == BatchModule.AP)
			{
				APTran documentLine = PXSelect<
					APTran,
					Where<
						APTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>
					.Select(this);
				scheduleDetail.BranchID = documentLine?.BranchID;
			}
			else if (Schedule.Current.Module == BatchModule.AR)
			{
				ARTran documentLine = PXSelect<
					ARTran,
					Where<
						ARTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<ARTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>
					.Select(this);
				scheduleDetail.BranchID = documentLine?.BranchID;
			}
			else
			{
				throw new PXException(Messages.UnexpectedModuleSpecified);
			}
		}

		#endregion

		#region Event Handlers

		protected virtual void DRSchedule_RefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule schedule = e.Row as DRSchedule;

			if (schedule == null) return;

			int? documentBusinessAccountID = GetDocumentBusinessAccountID(schedule);

			if (documentBusinessAccountID.HasValue)
			{
				sender.SetValueExt<DRSchedule.bAccountID>(
					schedule,
					documentBusinessAccountID);
			}
			sender.SetValueExt<DRSchedule.lineNbr>(schedule, null);

			CurrencyInfo origCurrencyInfo = null;

			if (schedule.RefNbr != null && schedule.Module == BatchModule.AP)
			{
				origCurrencyInfo = PXSelectJoin<CurrencyInfo, InnerJoin<APRegister, On<CurrencyInfo.curyInfoID, Equal<APRegister.curyInfoID>>>,
					Where<APRegister.docType, Equal<Required<APRegister.docType>>,
					And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, schedule.DocType, schedule.RefNbr);
			}

			if (schedule.RefNbr != null && schedule.Module == BatchModule.AR)
			{
				origCurrencyInfo = PXSelectJoin<CurrencyInfo, InnerJoin<ARRegister, On<CurrencyInfo.curyInfoID, Equal<ARRegister.curyInfoID>>>,
					Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
					And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, schedule.DocType, schedule.RefNbr);
			}

			if (origCurrencyInfo != null)
			{
				CurrencyInfo.Cache.Clear();

				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)origCurrencyInfo);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(CurrencyInfo.Insert(info));

				DRSchedule scheduleCopy = (DRSchedule)Schedule.Cache.CreateCopy(schedule);
				scheduleCopy.CuryInfoID = info.CuryInfoID;
				Schedule.Update(scheduleCopy);
			}
		}

		protected virtual void DRSchedule_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<DRSchedule.docDate>(sender, e);
		}

		protected virtual void DRSchedule_LineNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule schedule = e.Row as DRSchedule;

			if (schedule == null) return;

			foreach (DRScheduleDetail component in Components.Select())
			{
				FillBranch(component);
				Components.Cache.Update(component);
			}
		}

		protected virtual void DRSchedule_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule schedule = e.Row as DRSchedule;

			if (schedule == null) return;

			sender.SetDefaultExt<DRSchedule.bAccountLocID>(schedule);

			// If selected document does has a different business account
			// than the one that is currently specified, then reference
			// number field should be emptied.
			// -
			if (schedule.BAccountID != GetDocumentBusinessAccountID(schedule))
			{
				sender.SetDefaultExt<DRSchedule.refNbr>(schedule);
			}
		}

		protected virtual void DRSchedule_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}

		protected virtual void DRSchedule_DocumentType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule deferralSchedule = e.Row as DRSchedule;

			if (deferralSchedule == null) return;

			deferralSchedule.DocType = DRScheduleDocumentType.ExtractDocType(deferralSchedule.DocumentType);
			
			sender.SetValueExt<DRSchedule.module>(
				deferralSchedule,
				DRScheduleDocumentType.ExtractModule(deferralSchedule.DocumentType));

			if (deferralSchedule.Module == BatchModule.AR)
			{
				deferralSchedule.BAccountType = BAccountType.CustomerType;
			}
			else if (deferralSchedule.Module == BatchModule.AP)
			{
				deferralSchedule.BAccountType = BAccountType.VendorType;
			}
		}

		/// <summary>
		/// If the schedule's module has changed, force deletion of all 
		/// schedule components.
		/// </summary>
		protected virtual void DRSchedule_Module_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRSchedule deferralSchedule = e.Row as DRSchedule;
			string oldModule = e.OldValue as string;

			if (deferralSchedule == null || oldModule == null)
				return;

			if (deferralSchedule.Module != oldModule)
			{
				Components
					.Select()
					.RowCast<DRScheduleDetail>()
					.ForEach(scheduleDetail => Components.Delete(scheduleDetail));
			}
		}

		protected virtual void DRSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;

			if (row == null) return;

			if (string.IsNullOrEmpty(row.Module))
			{
				Components.Cache.RaiseExceptionHandling<DRScheduleDetail.documentType>(Components.Current, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(DRScheduleDetail.documentType).Name));
			}

			if (string.IsNullOrEmpty(row.FinPeriodID))
			{
				Components.Cache.RaiseExceptionHandling<DRScheduleDetail.finPeriodID>(Components.Current, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(DRScheduleDetail.finPeriodID).Name));
			}

			VerifyNoDuplicateSchedules(sender, row);
			VerifyDocumentLineExists(sender, row);

			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>() != true)
			{
				VerifyFlexibleComponentsConsistency(sender, row);
			}

			if((PXAccess.FeatureInstalled<FeaturesSet.aSC606>() == true && row.Module == BatchModule.AR) == false)
			{
				VerifyAndAdjustComponentAmounts();
			}
		}

		protected virtual void DRSchedule_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;

			if (row != null)
			{
				row.IsCustom = true;
				row.IsDraft = true;
			}
		}

		protected virtual void DRSchedule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRSchedule deferralSchedule = e.Row as DRSchedule;

			if (deferralSchedule == null) return;

			deferralSchedule.DocumentType = DRScheduleDocumentType.BuildDocumentType(
				deferralSchedule.Module, 
				deferralSchedule.DocType);

			deferralSchedule.OrigLineAmt = null;

			if (deferralSchedule.Module == BatchModule.AR)
			{
                deferralSchedule.BAccountType = BAccountType.CustomerType;
				GetPostingAmount(deferralSchedule);
			}
			else
			{
                deferralSchedule.BAccountType = BAccountType.VendorType;

				APTran documentLine = PXSelect<
					APTran, 
					Where<
						APTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>
					.Select(this);

				if (documentLine != null)
				{
					deferralSchedule.OrigLineAmt =
						APReleaseProcess.GetExpensePostingAmount(this, documentLine).Base ?? 0m;
				}
			}

			bool isReleasedSchedule = deferralSchedule.IsDraft != true;
			bool isCustomSchedule = deferralSchedule.IsCustom == true;
			bool isUnreleasedCustomSchedule = !isReleasedSchedule && isCustomSchedule;
			bool isReferenceNumberFilled = deferralSchedule.RefNbr != null;
			bool isLineNumberFilled = deferralSchedule.LineNbr != null;

			release.SetVisible(isCustomSchedule);
			release.SetEnabled(
				isUnreleasedCustomSchedule && 
				AllComponentsHaveTransactions());

			viewDoc.SetEnabled(isReferenceNumberFilled);

            //TODO: AC-121990. We cannot change the DisplayName for Field after SetEnabled.
            //SetEnabled() creates a copy of the row and we cannot change her.
            PXUIFieldAttribute.SetDisplayName<DRSchedule.projectID>(sender, GL.Messages.ProjectContract);

			PXUIFieldAttribute.SetEnabled<DRSchedule.documentType>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.finPeriodID>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.refNbr>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.lineNbr>(sender, deferralSchedule, isUnreleasedCustomSchedule && isReferenceNumberFilled);
			PXUIFieldAttribute.SetEnabled<DRSchedule.docDate>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.bAccountID>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.bAccountLocID>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.projectID>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.taskID>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.termStartDate>(sender, deferralSchedule, isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.termEndDate>(sender, deferralSchedule, isUnreleasedCustomSchedule);

			PXUIFieldAttribute.SetVisible<DRSchedule.origLineAmt>(sender, deferralSchedule, isLineNumberFilled);
			PXUIFieldAttribute.SetVisible<DRSchedule.finPeriodID>(sender, deferralSchedule, !PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>());
			PXUIFieldAttribute.SetVisible<DRScheduleDetail.finPeriodID>(
				Components.Cache,
				null,
				PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>());

			bool isAnyComponentFlexible = ComponentsWithFlexibleCodes.Any();

			PXUIFieldAttribute.SetRequired<DRSchedule.termStartDate>(sender, isAnyComponentFlexible);
			PXUIFieldAttribute.SetRequired<DRSchedule.termEndDate>(sender, isAnyComponentFlexible);

			sender.AllowDelete = deferralSchedule.IsDraft == true;

			Components.Cache.AllowInsert = deferralSchedule.IsDraft == true;
			Components.Cache.AllowUpdate = deferralSchedule.IsDraft == true;
			Components.Cache.AllowDelete = deferralSchedule.IsDraft == true;

			sender
				.GetAttributes<DRSchedule.refNbr>(deferralSchedule)
				.OfType<DRDocumentSelectorAttribute>()
				.ForEach(attribute => attribute.ExcludeUnreleased = deferralSchedule.IsCustom == true);

			PXUIFieldAttribute.SetEnabled<DRScheduleDetail.branchID>(
				Components.Cache,
				null,
				isUnreleasedCustomSchedule && !isReferenceNumberFilled && !isLineNumberFilled);

			PXUIFieldAttribute.SetReadOnly<DRScheduleDetail.branchID>(
				Components.Cache,
				null,
				(isUnreleasedCustomSchedule && !isReferenceNumberFilled && !isLineNumberFilled) == false);

			SetScheduleStatus(deferralSchedule);
		}

		private void SetScheduleStatus(DRSchedule schedule)
		{
			if (schedule.IsDraft == true)
			{
				schedule.Status = DRScheduleStatus.Draft;
			}
			else
			{
				schedule.Status = DRScheduleStatus.Closed;
				foreach (var d in this.Components.Select())
				{
					DRScheduleDetail det = d;
					if(det?.IsOpen == true)
					{
						schedule.Status = DRScheduleStatus.Open;
						break;
					}
				}
			}
		}

		private void GetPostingAmount(DRSchedule schedule)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				ARReleaseProcess.Amount netLinesAmount;

				if (schedule.IsCustom == true)
				{
					netLinesAmount = ASC606Helper.CalculateSalesPostingAmount(this, schedule);
				}
				else
				{
					netLinesAmount = ASC606Helper.CalculateNetAmount(this, schedule);
				}

				schedule.OrigLineAmt = 0m;
				schedule.CuryNetTranPrice = netLinesAmount.Cury;
				schedule.NetTranPrice = netLinesAmount.Base;
			}
			else
			{
				ARTran documentLine = PXSelect<
					ARTran,
					Where<
						ARTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<ARTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>
					.Select(this);

				if (documentLine != null)
				{
					schedule.OrigLineAmt = ARReleaseProcess.GetSalesPostingAmount(this, documentLine).Base ?? 0m;
					return;
				}

				schedule.OrigLineAmt = 0m;
			}
		}

		protected virtual void DRSchedule_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;

			if (!sender.ObjectsEqual<DRSchedule.documentType, DRSchedule.refNbr, DRSchedule.lineNbr, DRSchedule.bAccountID, DRSchedule.finPeriodID, DRSchedule.docDate>(e.Row, e.OldRow))
			{
				foreach (DRScheduleDetail detail in Components.Select())
				{
					SynchronizeDetailPropertiesFromSchedule(row, detail);
					Components.Update(detail);
				}
			}
		}

		protected virtual void DRSchedule_LineNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DRSchedule deferralSchedule = e.Row as DRSchedule;

			if (deferralSchedule?.RefNbr == null) return;

			VerifyNoDuplicateSchedules(sender, deferralSchedule, e.NewValue as int?);
			VerifyDocumentLineExists(sender, deferralSchedule, e.NewValue as int?);
		}

		private void SynchronizeDetailPropertiesFromSchedule(
			DRSchedule deferralSchedule, 
			DRScheduleDetail scheduleDetail)
		{
			scheduleDetail.Module = deferralSchedule.Module;
			scheduleDetail.DocumentType = deferralSchedule.DocumentType;
			scheduleDetail.DocType = deferralSchedule.DocType;
			scheduleDetail.RefNbr = deferralSchedule.RefNbr;
			if (!(PXAccess.FeatureInstalled<FeaturesSet.aSC606>() && deferralSchedule.Module == BatchModule.AR))
			{
				scheduleDetail.LineNbr = deferralSchedule.LineNbr;
			}
			scheduleDetail.BAccountID = deferralSchedule.BAccountID;

			if (deferralSchedule.RefNbr == null)
				FinPeriodIDAttribute.SetPeriodsByMaster<DRScheduleDetail.finPeriodID>(Components.Cache, scheduleDetail, deferralSchedule.FinPeriodID);

			scheduleDetail.DocDate = deferralSchedule.DocDate;
		}

	    private static void SynchronizeDeferralAmountWithTotalAmount(DRScheduleDetail scheduleDetail)
	    {
			if (
				scheduleDetail != null && 
				scheduleDetail.Status == DRScheduleStatus.Draft && 
				scheduleDetail.IsResidual != true)
			{
				scheduleDetail.CuryDefAmt = scheduleDetail.CuryTotalAmt;
				scheduleDetail.DefAmt = scheduleDetail.TotalAmt;
			}
		}
		
		protected virtual void DRScheduleDetail_DocumentType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				string newModule = DRScheduleDocumentType.ExtractModule(row.DocumentType);
				row.DocType = DRScheduleDocumentType.ExtractDocType(row.DocumentType);

				if (row.Module != newModule)
				{
					row.Module = newModule;
					row.DefCode = null;
					row.DefAcctID = null;
					row.DefSubID = null;
					row.BAccountID = null;
					row.AccountID = null;
					row.SubID = null;

					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<DRScheduleDetail.componentID>>>>.Select(this, row.ComponentID);
					if (item != null)
					{
						row.AccountID = row.Module == BatchModule.AP ? item.COGSAcctID : item.SalesAcctID;
						row.SubID = row.Module == BatchModule.AP ? item.COGSSubID : item.SalesSubID;
					}
				}

				row.RefNbr = null;
			}
		}

		/// <summary>
		/// Fills the schedule detail's amount as a residual between the 
		/// original line amount and the sum of all other details' total amounts.
		/// </summary>
	    protected virtual void DRScheduleDetail_TotalAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null || Schedule.Current.IsCustom != true)
			{
				return;
			}

			decimal remainingAmount = Components
				.Select()
				.RowCast<DRScheduleDetail>()
				.Select(detail => detail.TotalAmt ?? 0m)
				.Aggregate(
					Schedule.Current.OrigLineAmt ?? 0m,
					(accumulator, detailAmount) => accumulator - detailAmount);

			if (remainingAmount < 0m)
			{
				remainingAmount = 0m;
			}

			e.NewValue = remainingAmount;

			if (e.NewValue != null)
			{
				decimal curyTotalAmt = 0m;

				if (CurrencyInfo.Current.CuryID != CurrencyInfo.Current.BaseCuryID)
				{
					CM.PXCurrencyAttribute.CuryConvCury<DRScheduleDetail.curyInfoID>(sender, scheduleDetail, remainingAmount, out curyTotalAmt);
				}
				else
				{
					curyTotalAmt = remainingAmount;
				}

				scheduleDetail.CuryTotalAmt = curyTotalAmt;
			}
		}

		protected virtual void DRScheduleDetail_TotalAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null)
			{
				return;
			}

			if (scheduleDetail.TotalAmt != null)
			{
				decimal curyTotalAmt = 0m;
				CM.PXCurrencyAttribute.CuryConvCury<DRScheduleDetail.curyInfoID>(sender, scheduleDetail, scheduleDetail.TotalAmt.Value, out curyTotalAmt);
				scheduleDetail.CuryTotalAmt = curyTotalAmt;
			}

			SynchronizeDeferralAmountWithTotalAmount(scheduleDetail);
		}

		/// <summary>
		/// This is auto setting of the <see cref="DRScheduleDetail.curyDefAmt"/> field because of only the base currency are allowed for DR Schedules for now.
		/// </summary>
		protected virtual void DRScheduleDetail_DefAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null) return;

			if (scheduleDetail.DefAmt != null && scheduleDetail.CuryDefAmt == null)
			{
				decimal curyDefAmt = 0m;
				CM.PXCurrencyAttribute.CuryConvCury<DRScheduleDetail.curyInfoID>(sender, scheduleDetail, scheduleDetail.DefAmt.Value, out curyDefAmt);
				scheduleDetail.CuryDefAmt = curyDefAmt;
			}
		}

		protected virtual void DRScheduleDetail_DefCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRScheduleDetail.defCode>>>>.Select(this, row.DefCode);

				if (defCode != null)
				{
					row.DefCode = defCode.DeferredCodeID;
					row.DefAcctID = defCode.AccountID;
					row.DefSubID = defCode.SubID;
				}
			}
		}

		protected virtual void DRScheduleDetail_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}

        protected virtual void DRScheduleDetail_DefCodeType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (Schedule.Current != null)
            {
                e.NewValue = Schedule.Current.Module == BatchModule.AP? DeferredAccountType.Expense: DeferredAccountType.Income;
                e.Cancel = true;
            }
        }

		protected virtual void DRScheduleDetail_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail?.BAccountID == null) return;

			if (scheduleDetail.ComponentID != null &&
				scheduleDetail.ComponentID != DRScheduleDetail.EmptyComponentID &&
				scheduleDetail.AccountID != null)
			{
				return;
			}

			if (scheduleDetail.Module == BatchModule.AP)
			{
				PXResultset<Vendor> vendorWithLocation = PXSelectJoin<
					Vendor,
					InnerJoin<Location, 
						On<Vendor.bAccountID, Equal<Location.bAccountID>,
							And<Vendor.defLocationID, Equal<Location.locationID>>>>,
					Where<
						Vendor.bAccountID, Equal<Current<DRScheduleDetail.bAccountID>>>>
					.Select(this);

				Location location = (Location)vendorWithLocation[0][1];

				if (location.VExpenseAcctID != null)
				{
					scheduleDetail.AccountID = location.VExpenseAcctID;
					scheduleDetail.SubID = location.VExpenseSubID;
				}
			}
			else if (scheduleDetail.Module == BatchModule.AR)
			{
				PXResultset<Customer> customerWithLocation = PXSelectJoin<
					Customer,
					InnerJoin<Location, 
						On<Customer.bAccountID, Equal<Customer.bAccountID>,
							And<Customer.defLocationID, Equal<Location.locationID>>>>,
					Where<
						Customer.bAccountID, Equal<Current<DRScheduleDetail.bAccountID>>>>
					.Select(this);

				Location location = (Location)customerWithLocation[0][1];

				if (location.CSalesAcctID != null)
				{
					scheduleDetail.AccountID = location.CSalesAcctID;
					scheduleDetail.SubID = location.CSalesSubID;
				}
			}
		}

		protected virtual void DRScheduleDetail_ScheduleID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void DRScheduleDetail_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;
			FillBranch(scheduleDetail);
		}

		protected virtual void DRScheduleDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null || Schedule.Current == null) return;

			scheduleDetail.ComponentID = scheduleDetail.ComponentID ?? DRScheduleDetail.EmptyComponentID;
			scheduleDetail.ScheduleID = Schedule.Current.ScheduleID;

			DRScheduleDetail duplicate = PXSelect<
				DRScheduleDetail,
				Where<
					DRScheduleDetail.scheduleID, Equal<Current<DRSchedule.scheduleID>>,
					And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>>>>
				.Select(this, scheduleDetail.ComponentID ?? DRScheduleDetail.EmptyComponentID);

			if (duplicate != null)
			{
				var componentID = sender.GetValueExt<DRScheduleDetail.componentID>(scheduleDetail);
				sender.RaiseExceptionHandling<DRScheduleDetail.componentID>(scheduleDetail, componentID?.ToString(), new PXSetPropertyException(Messages.DuplicateComponent));
				e.Cancel = true;
			}
		}

		protected virtual void DRScheduleDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if(CurrentContext == Context.Recalculate)
			{
				return;
			}

			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

		    if (scheduleDetail == null || Schedule.Current == null) return;

			scheduleDetail.Status = DRScheduleStatus.Draft;
			scheduleDetail.IsCustom = true;
			
			SynchronizeDetailPropertiesFromSchedule(Schedule.Current, scheduleDetail);
		    SynchronizeDeferralAmountWithTotalAmount(scheduleDetail);

			InventoryItem inventoryItem = PXSelect<
				InventoryItem, 
				Where<
					InventoryItem.inventoryID, Equal<Required<DRScheduleDetail.componentID>>>>
				.Select(this, scheduleDetail.ComponentID);

		    if (inventoryItem == null) return;

		    scheduleDetail.AccountID = 
				scheduleDetail.Module == BatchModule.AP ? inventoryItem.COGSAcctID : inventoryItem.SalesAcctID;

			scheduleDetail.SubID = 
				scheduleDetail.Module == BatchModule.AP ? inventoryItem.COGSSubID : inventoryItem.SalesSubID;
			
		    DRDeferredCode deferralCode = PXSelect<
				DRDeferredCode, 
				Where<
					DRDeferredCode.deferredCodeID, Equal<Required<DRScheduleDetail.defCode>>>>
				.Select(this, inventoryItem.DeferredCode);

		    if (deferralCode != null)
		    {
			    scheduleDetail.DefCode = deferralCode.DeferredCodeID;
			    scheduleDetail.DefAcctID = deferralCode.AccountID;
			    scheduleDetail.DefSubID = deferralCode.SubID;
		    }
		}

		protected virtual void DRScheduleDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (IsCopyPasteContext)
			{
				DRScheduleDetail row = e.Row as DRScheduleDetail;
				sender.SetValue<DRScheduleDetail.status>(row, DRScheduleStatus.Draft);
			}
		}

		protected virtual void DRScheduleDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null) return;

			scheduleDetail.DocumentType = DRScheduleDocumentType.BuildDocumentType(scheduleDetail.Module, scheduleDetail.DocType);
			scheduleDetail.DefTotal = SumOpenAndProjectedTransactions(scheduleDetail);

			bool isCustomComponent = scheduleDetail.IsCustom == true;
			bool isEmptyComponent = scheduleDetail.ComponentID == DRScheduleDetail.EmptyComponentID;
			bool isNullComponent = scheduleDetail.ComponentID == null;
			bool hasTransactions = Transactions.View.SelectSingleBound(new object[] { scheduleDetail }) != null;

			PXUIFieldAttribute.SetEnabled<DRScheduleDetail.componentID>(
				sender,
				scheduleDetail,
				isNullComponent || isCustomComponent && !hasTransactions);

			PXDefaultAttribute.SetPersistingCheck<DRScheduleDetail.defCode>(
				sender, 
				scheduleDetail, 
				scheduleDetail.IsResidual == true ? 
					PXPersistingCheck.Nothing : 
					PXPersistingCheck.NullOrBlank);

			bool anyComponents = Components.Any();

			Transactions.Cache.AllowInsert = 
				Transactions.Cache.AllowUpdate = 
					Transactions.Cache.AllowDelete = 
						scheduleDetail.Status != DRScheduleStatus.Closed && anyComponents;

			bool anyRecognizedTransactions = Transactions
				.Select()
				.RowCast<DRScheduleTran>()
				.Any(transaction => transaction.Status == DRScheduleTranStatus.Posted);

			generateTransactions.SetEnabled(!anyRecognizedTransactions);
		}
		
		protected virtual void DRScheduleTran_RecDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null)
			{
				FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(row.RecDate, PXAccess.GetParentOrganizationID(row.BranchID));
				row.FinPeriodID = finPeriod.FinPeriodID;
				row.TranPeriodID = finPeriod.MasterFinPeriodID;
			}
		}

		protected virtual void DRScheduleTran_Amount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EnsureCurrentComponentIsUpdated();
		}

		protected virtual void DRScheduleTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.recDate>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.amount>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.accountID>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.subID>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.branchID>(sender, row, false);
			}
		}

		protected virtual void DRScheduleTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (!sender.ObjectsEqual<DRScheduleTran.finPeriodID, DRScheduleTran.accountID, DRScheduleTran.subID, DRScheduleTran.amount>(e.Row, e.OldRow))
			{
				if (Components.Current != null && Components.Current.Status == DRScheduleStatus.Open)
				{
					DRScheduleTran oldRow = (DRScheduleTran)e.OldRow;
					Subtract(oldRow);
					Add(row);
				}
			}
		}

		protected virtual void DRScheduleTranRowImporting(object sender, PXImportAttribute.RowImportingEventArgs e)
		{
			if (e.Mode == PXImportAttribute.ImportMode.Value.UpdateExisting && e.Keys.Contains(nameof(DRScheduleTran.LineNbr)))
			{
				DRScheduleTran existingRecord = Transactions.Search<DRScheduleTran.lineNbr>(e.Keys[nameof(DRScheduleTran.LineNbr)]);

				if (existingRecord != null && 
					existingRecord.Status != DRScheduleTranStatus.Open && 
					existingRecord.Status != DRScheduleTranStatus.Projected)
				{
					e.Cancel = true;
				}
			}
		}

		protected virtual void DRScheduleTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			DRScheduleTran scheduleTran = e.Row as DRScheduleTran;

			if (scheduleTran == null || Components.Current == null) return;

			scheduleTran.BranchID = Components.Current.BranchID;
		}

		protected virtual void DRScheduleTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;

			EnsureCurrentComponentIsUpdated();

			if (row != null && Components.Current != null && Components.Current.Status == DRScheduleStatus.Open)
			{
				Add(row);
			}
		}

		protected virtual void DRScheduleTran_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var tran = e.Row as DRScheduleTran;

			if (tran != null && tran.Status == DRScheduleTranStatus.Posted)
			{
				throw new PXException(Messages.CannotDeletePostedTransaction);
			}
		}

		protected virtual void DRScheduleTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;

			EnsureCurrentComponentIsUpdated();

			if (row != null && Components.Current != null && Components.Current.Status == DRScheduleStatus.Open)
			{
				Subtract(row);
			}
		}

		private void EnsureCurrentComponentIsUpdated()
		{
			if (Components.Current != null)
			{
				Components.Cache.MarkUpdated(Components.Current);
			}
		}

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Document Line Quantity")]
		protected virtual void ARTran_Qty_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "MDA Component Quantity")]
		protected virtual void INComponent_Qty_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "MDA Component UOM")]
		protected virtual void INComponent_UOM_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "MDA Component ID")]
        protected virtual void INComponent_ComponentID_CacheAttached(PXCache sender) { }

        [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Ext. Price")]
		protected virtual void ARTran_CuryExtPrice_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Amount")]
		protected virtual void ARTran_CuryTranAmt_CacheAttached(PXCache sender) { }
		#endregion

		private bool AllComponentsHaveTransactions()
		{
			bool allComponentsHaveTransactions = Components.Any();

			foreach (DRScheduleDetail scheduleDetail in Components.Select())
			{
				if (!Transactions.Any(
					scheduleDetail.ScheduleID,
					scheduleDetail.ComponentID,
					scheduleDetail.DetailLineNbr,
					scheduleDetail.CreditLineNbr))
				{
					return false;
				}
			}

			return allComponentsHaveTransactions;
		}

		/// <summary>
		/// Performs a check if any of the schedule components are flexible. If so,
		/// verifies that term start date and term end date are present and consistent
		/// with each other, as well as marks them as required via <see cref="PXUIFieldAttribute"/>.
		/// </summary>
		/// <param name="cache">A cache of <see cref="DRSchedule"/> records.</param>
		/// <param name="deferralSchedule">The current deferral schedule record.</param>
		private void VerifyFlexibleComponentsConsistency(PXCache cache, DRSchedule deferralSchedule)
	    {
			bool isAnyComponentFlexible = ComponentsWithFlexibleCodes.Any();

			if (isAnyComponentFlexible && !deferralSchedule.TermStartDate.HasValue)
			{
				cache.RaiseExceptionHandling<DRSchedule.termStartDate>(
					deferralSchedule,
					null,
					new PXSetPropertyException<DRSchedule.termStartDate>(
						ErrorMessages.FieldIsEmpty,
						PXErrorLevel.Error,
						typeof(DRSchedule.termStartDate).Name));
			}

			if (isAnyComponentFlexible && !deferralSchedule.TermEndDate.HasValue)
			{
				cache.RaiseExceptionHandling<DRSchedule.termEndDate>(
					deferralSchedule,
					null,
					new PXSetPropertyException<DRSchedule.termEndDate>(
						ErrorMessages.FieldIsEmpty,
						PXErrorLevel.Error,
						typeof(DRSchedule.termEndDate).Name));
			}

			if (isAnyComponentFlexible && deferralSchedule.TermStartDate > deferralSchedule.TermEndDate)
			{
				cache.RaiseExceptionHandling<DRSchedule.termEndDate>(
					deferralSchedule,
					null,
					new PXSetPropertyException<DRSchedule.termEndDate>(
						Messages.TermCantBeNegative,
						PXErrorLevel.Error,
						deferralSchedule.TermEndDate,
						deferralSchedule.TermStartDate));
			}
		}

		private void VerifyAndAdjustComponentAmounts()
		{
			if (Schedule.Current?.RefNbr == null ||
				Schedule.Current.OrigLineAmt.GetValueOrDefault() == 0)
			{
				return;
			}

			decimal detailsTotal = 0;

			foreach (DRScheduleDetail scheduleDetail in Components.Select())
			{
				detailsTotal += scheduleDetail.TotalAmt ?? 0;

				DRDeferredCode detailDeferralCode = PXSelect<
					DRDeferredCode,
					Where<
						DRDeferredCode.deferredCodeID, Equal<Current<DRScheduleDetail.defCode>>>>
					.Select(this);

				bool checkTotal =
					detailDeferralCode == null ||
					detailDeferralCode.Method != DeferredMethodType.CashReceipt;

				if (!checkTotal) continue;

				decimal defTotal = SumOpenAndProjectedTransactions(scheduleDetail);

				if (defTotal != scheduleDetail.DefAmt)
				{
					if (Components.Cache.RaiseExceptionHandling<DRScheduleDetail.defTotal>(
						scheduleDetail,
						defTotal,
						new PXSetPropertyException(Messages.DeferredAmountSumError)))
					{
						throw new PXRowPersistingException(
							typeof(DRScheduleDetail.defTotal).Name,
							defTotal,
							Messages.DeferredAmountSumError);
					}
				}
			}

			DRScheduleDetail residualDetail = ResidualComponent.SelectSingle();

			if (residualDetail != null)
			{
				ResidualComponent.Cache.RaiseRowSelected(residualDetail);

				decimal residual = (Schedule.Current.OrigLineAmt ?? 0) - detailsTotal;

				if (residual < 0)
				{
					throw new PXException(Messages.ResidualWillGoNegativeWithCustomAmounts);
				}

				if (residualDetail.TotalAmt != residual)
				{
					residualDetail.TotalAmt = residual;
					residualDetail = ResidualComponent.Update(residualDetail);
				}

				detailsTotal += residualDetail.TotalAmt ?? 0;
			}

			if (Schedule.Current.OrigLineAmt != detailsTotal)
			{
				throw new PXException(Messages.SumOfComponentsError);
			}
		}

		private void CreateTransactions(DRDeferredCode deferralCode)
		{
			if (Components.Current == null) return;

			foreach (DRScheduleTran existingTransaction in Transactions.Select())
			{
				if (Components.Current.Status != DRScheduleStatus.Draft && 
					existingTransaction.Status == DRScheduleTranStatus.Posted)
				{
					throw new PXException(Messages.CantRegeneratePostedTransactions);
				}

				Transactions.Delete(existingTransaction);
			}

			if (Schedule.Current != null && 
				Components.Current != null && 
				deferralCode != null)
			{
				IList<DRScheduleTran> newTransactions = 
					GetTransactionsGenerator(deferralCode)
						.GenerateTransactions(Schedule.Current, Components.Current);

				foreach (DRScheduleTran deferralTransaction in newTransactions)
				{
					Transactions.Insert(deferralTransaction);
				}
			}
		}

		private void Subtract(DRScheduleTran tran)
		{
			Debug.Print("Subtract FinPeriod={0} Status={1} Amount={2}", tran.FinPeriodID, tran.Status, tran.Amount);
			DRDeferredCode code = DeferredCode.Select();

			if (code.AccountType == DeferredAccountType.Expense)
			{
				SubtractExpenseFromProjection(tran);
				SubtractExpenseFromBalance(tran);
			}
			else
			{
				SubtractRevenueFromProjection(tran);
				SubtractRevenueFromBalance(tran);
			}
		}

		private void SubtractRevenueFromProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(tran.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			tranHist.TranPTDProjected -= tran.Amount;
		}

		private void SubtractExpenseFromProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(tran.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			tranHist.TranPTDProjected -= tran.Amount;
		}

		private void SubtractRevenueFromBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRRevenueBalance finHist = CreateDRRevenueBalance(tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			finHist.EndProjected += tran.Amount;

			tranHist.TranPTDProjected -= tran.Amount;
			tranHist.TranEndProjected += tran.Amount;
		}

		private void SubtractExpenseFromBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRExpenseBalance finHist = CreateDRExpenseBalance(tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			finHist.EndProjected += tran.Amount;

			tranHist.PTDProjected -= tran.Amount;
			tranHist.EndProjected += tran.Amount;
		}

		private void Add(DRScheduleTran tran)
		{
			Debug.Print("Add FinPeriod={0} Status={1} Amount={2}", tran.FinPeriodID, tran.Status, tran.Amount);
			DRDeferredCode code = DeferredCode.Select();

			if (code.AccountType == DeferredAccountType.Expense)
			{
				AddExpenseToProjection(tran);
				AddExpenseToBalance(tran);
			}
			else
			{
				AddRevenueToProjection(tran);
				AddRevenueToBalance(tran);
			}
		}

		private void AddRevenueToProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(tran.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			tranHist.TranPTDProjected += tran.Amount;
		}

		private void AddExpenseToProjection(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(tran.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			tranHist.TranPTDProjected += tran.Amount;
		}

		private void AddRevenueToBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRRevenueBalance finHist = CreateDRRevenueBalance(tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			finHist.EndProjected -= tran.Amount;

			tranHist.TranPTDProjected += tran.Amount;
			tranHist.TranEndProjected -= tran.Amount;
		}

		private void AddExpenseToBalance(DRScheduleTran tran)
		{
			if (tran.FinPeriodID == null || tran.TranPeriodID == null)
				return;

			DRExpenseBalance finHist = CreateDRExpenseBalance(tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			finHist.EndProjected -= tran.Amount;

			tranHist.TranPTDProjected += tran.Amount;
			tranHist.TranEndProjected -= tran.Amount;
		}

		private DRExpenseBalance CreateDRExpenseBalance(string periodID)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = periodID;
			hist.BranchID = Components.Current.BranchID;
			hist.AcctID = Components.Current.DefAcctID;
			hist.SubID = Components.Current.DefSubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.VendorID = Components.Current.BAccountID ?? 0;

			return ExpenseBalance.Insert(hist);
		}

		private DRRevenueBalance CreateDRRevenueBalance(string periodID)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = periodID;
			hist.BranchID = Components.Current.BranchID;
			hist.AcctID = Components.Current.DefAcctID;
			hist.SubID = Components.Current.DefSubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.CustomerID = Components.Current.BAccountID ?? 0;

			return RevenueBalance.Insert(hist);
		}

		private DRExpenseProjectionAccum CreateDRExpenseProjectionAccum(string periodID)
		{
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = periodID;
			hist.BranchID = Components.Current.BranchID;
			hist.AcctID = Components.Current.AccountID;
			hist.SubID = Components.Current.SubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.VendorID = Components.Current.BAccountID ?? 0;

			return ExpenseProjection.Insert(hist);
		}

		private DRRevenueProjectionAccum CreateDRRevenueProjectionAccum(string periodID)
		{
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = periodID;
			hist.BranchID = Components.Current.BranchID;
			hist.AcctID = Components.Current.AccountID;
			hist.SubID = Components.Current.SubID;
			hist.ComponentID = Components.Current.ComponentID ?? 0;
			hist.ProjectID = Components.Current.ProjectID ?? 0;
			hist.CustomerID = Components.Current.BAccountID ?? 0;

			return RevenueProjection.Insert(hist);
		}

		private decimal SumOpenAndProjectedTransactions(DRScheduleDetail row)
		{
			decimal total = 0;
			foreach (DRScheduleTran tran in OpenTransactions.View.SelectMultiBound(new object[]{row}))
			{
				total += tran.Amount.Value;
			}

			foreach (DRScheduleTran tran in ProjectedTransactions.View.SelectMultiBound(new object[] { row }))
			{
				total += tran.Amount.Value;
			}

			return total;
		}

        [PXCacheName(Messages.DRScheduleEx)]
        [Serializable]
		public partial class DRScheduleEx : DRSchedule
		{
			#region ScheduleNbr
			public new abstract class scheduleNbr : PX.Data.BQL.BqlString.Field<scheduleNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXUIField(DisplayName = Messages.ScheduleNbr)]
			public override string ScheduleNbr { get; set; }
			#endregion
		}

		#region Factory Methods

		protected virtual TransactionsGenerator GetTransactionsGenerator(DRDeferredCode deferralCode)
			=> new TransactionsGenerator(this, deferralCode);

		#endregion
	}

    public class DraftScheduleMaintProjectFieldVisibilityGraphExtension : PXGraphExtension<DraftScheduleMaint>
    {
        protected virtual void _(Events.RowSelected<DRSchedule> e)
        {
            if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<DRSchedule.projectID>(e.Cache, e.Row,
                PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() || PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>());
            PXUIFieldAttribute.SetDisplayName<DRSchedule.projectID>(e.Cache, GL.Messages.ProjectContract);
        }
    }
}
