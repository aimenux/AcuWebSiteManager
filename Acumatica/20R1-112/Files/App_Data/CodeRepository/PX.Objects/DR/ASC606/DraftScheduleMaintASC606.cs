using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.DR
{
	public class DraftScheduleMaintASC606 : PXGraphExtension<DraftScheduleMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		public PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<DRSchedule.docType>>,
						And<ARRegister.refNbr, Equal<Required<DRSchedule.refNbr>>>>> OriginalDocument;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[DRDocumentSelectorASC606(typeof(DRSchedule.module), typeof(DRSchedule.docType), typeof(DRSchedule.bAccountID))]
		protected virtual void DRSchedule_RefNbr_CacheAttached(PXCache sencer) { }

		protected virtual void DRSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting baseDelegate)
		{
			var schedule = (DRSchedule)e.Row;

			if (e.Operation == PXDBOperation.Delete)
			{
				return;
			}

			switch (schedule?.Module)
			{
				case BatchModule.AP:
					baseDelegate.Invoke(sender, e);
					break;

				case BatchModule.AR:
					VerifySchedule(schedule);
					schedule.IsRecalculated = false;
					break;

				default:
					throw new NotImplementedException();
			}

			if (schedule.Module == BatchModule.AR && schedule.RefNbr != null && schedule.IsCustom == true)
			{
				var origDoc = OriginalDocument.SelectSingle(schedule.DocType, schedule.RefNbr);
				origDoc.DRSchedCntr = origDoc.LineCntr;
				OriginalDocument.Update(origDoc);
				Base.Clear(PXClearOption.ClearQueriesOnly);
			}
		}

		private void VerifySchedule(DRSchedule schedule)
		{
			if (schedule.RefNbr == null || (schedule.NetTranPrice ?? 0m) == 0)
			{
				return;
			}

			var currencyInfo = Base.CurrencyInfo.SelectSingle();

			if (schedule.IsCustom == true && schedule.NetTranPrice < schedule.ComponentsTotal)
			{
				throw new PXException(Messages.CannotAttachCustom, schedule.ComponentsTotal, schedule.NetTranPrice, currencyInfo.BaseCuryID);
			}

			if (schedule.NetTranPrice != schedule.ComponentsTotal && schedule.IsOverridden == true)
			{
				throw new PXException(Messages.CannotModifyAttached, schedule.ComponentsTotal, schedule.NetTranPrice, currencyInfo.BaseCuryID);
			}
		}

		public PXAction<DRSchedule> recalculate;
		[PXUIField(DisplayName = Messages.Recalculate, Visible = true, Enabled = false)]
		[PXButton]
		public virtual IEnumerable Recalculate(PXAdapter adapter)
		{
			DRSchedule schedule = Base.DocumentProperties.Current;

			if (schedule.IsOverridden == true)
			{
				WebDialogResult result = Base.DocumentProperties.View.Ask(
							Base.DocumentProperties.Current,
							GL.Messages.Confirmation,
							Messages.ClearOverriden,
							MessageButtons.YesNo,
							MessageIcon.Question);

				if (result != WebDialogResult.Yes)
				{
					return adapter.Get();
				}
			}

			schedule.IsOverridden = false;
			schedule.IsRecalculated = false;
			Base.DocumentProperties.Update(schedule);

			SingleScheduleCreator.RecalculateSchedule(Base);

			return adapter.Get();
		}


		protected virtual void _(Events.RowSelected<DRSchedule> e)
		{
			if (e.Row == null)
			{
				return;
			}

			SetVisibileAndEnable(e);

			if (e.Row.Module == BatchModule.AR)
			{
				CalcTotals();
			}
		}

		private void SetVisibileAndEnable(Events.RowSelected<DRSchedule> e)
		{
			bool isAR = e.Row.Module == BatchModule.AR;

			bool isReleasedSchedule = e.Row.IsDraft != true;
			bool isCustomSchedule = e.Row.IsCustom == true;
			bool isUnreleasedCustomSchedule = !isReleasedSchedule && isCustomSchedule;
			bool recalculationEnable = e.Row.Module == BatchModule.AR && e.Row.IsDraft == true && e.Row.IsCustom == false && e.Row.RefNbr != null;
			recalculate.SetEnabled(recalculationEnable);

			PXUIFieldAttribute.SetVisible<DRSchedule.isOverridden>(e.Cache, e.Row, isAR);
			PXUIFieldAttribute.SetEnabled<DRSchedule.isOverridden>(e.Cache, e.Row, recalculationEnable);
			PXUIFieldAttribute.SetVisible<DRSchedule.curyNetTranPrice>(e.Cache, e.Row, isAR);
			PXUIFieldAttribute.SetVisible<DRSchedule.curyID>(e.Cache, e.Row, isAR && e.Row.RefNbr != null);
			PXUIFieldAttribute.SetVisible<DRSchedule.componentsTotal>(e.Cache, e.Row, isAR);
			PXUIFieldAttribute.SetVisible<DRSchedule.defTotal>(e.Cache, e.Row, isAR);
			PXUIFieldAttribute.SetVisible<DRSchedule.lineNbr>(e.Cache, e.Row, !isAR);
			PXUIFieldAttribute.SetVisible<DRSchedule.origLineAmt>(e.Cache, e.Row, !isAR);
			PXUIFieldAttribute.SetVisible<DRSchedule.taskID>(e.Cache, e.Row, !isAR);
			PXUIFieldAttribute.SetVisible<DRSchedule.termStartDate>(e.Cache, e.Row, !isAR);
			PXUIFieldAttribute.SetVisible<DRSchedule.termEndDate>(e.Cache, e.Row, !isAR);

			PXUIFieldAttribute.SetVisibility<DRScheduleDetail.taskID>(Base.Components.Cache, null, isAR ? PXUIVisibility.SelectorVisible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<DRScheduleDetail.termStartDate>(Base.Components.Cache, null, isAR ? PXUIVisibility.SelectorVisible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<DRScheduleDetail.termEndDate>(Base.Components.Cache, null, isAR ? PXUIVisibility.SelectorVisible : PXUIVisibility.Invisible);

			PXUIFieldAttribute.SetVisible<DRScheduleDetail.taskID>(Base.Components.Cache, null, isAR);
			PXUIFieldAttribute.SetVisible<DRScheduleDetail.termStartDate>(Base.Components.Cache, null, isAR);
			PXUIFieldAttribute.SetVisible<DRScheduleDetail.termEndDate>(Base.Components.Cache, null, isAR);

			PXUIFieldAttribute.SetEnabled<DRSchedule.taskID>(e.Cache, e.Row, !isAR && isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.termStartDate>(e.Cache, e.Row, !isAR && isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.termEndDate>(e.Cache, e.Row, !isAR && isUnreleasedCustomSchedule);
			PXUIFieldAttribute.SetEnabled<DRSchedule.lineNbr>(e.Cache, e.Row, !isAR && isUnreleasedCustomSchedule && e.Row.RefNbr != null);

			if (isAR)
			{
				bool isEditable = e.Row.IsDraft == true && (e.Row.IsOverridden == true || e.Row.IsCustom == true);
				bool isBranchEditable = e.Row.IsDraft == true && e.Row.IsCustom == true;

				Base.Components.Cache.AllowInsert = isEditable;
				Base.Components.Cache.AllowUpdate = isEditable;
				Base.Components.Cache.AllowDelete = isEditable;
				PXUIFieldAttribute.SetEnabled(Base.Components.Cache, null, isEditable);

				PXUIFieldAttribute.SetEnabled<DRScheduleDetail.branchID>(Base.Components.Cache, null, isBranchEditable);
				PXUIFieldAttribute.SetReadOnly<DRScheduleDetail.branchID>(Base.Components.Cache, null, !isBranchEditable);
			}
		}

		private void CalcTotals()
		{
			Base.Schedule.Current.ComponentsTotal = 0m;
			Base.Schedule.Current.DefTotal = 0m;
			foreach (DRScheduleDetail detail in Base.Components.Select())
			{
				Base.Schedule.Current.ComponentsTotal += PXCurrencyAttribute.BaseRound(Base, detail.TotalAmt);
				Base.Schedule.Current.DefTotal += PXCurrencyAttribute.BaseRound(Base, detail.DefAmt);
			}
		}

		protected virtual void _(Events.RowSelected<DRScheduleDetail> e)
		{
			if (e.Row == null || e.Row.Module != BatchModule.AR)
				return;

			bool hasTransactions = Base.Transactions.View.SelectSingleBound(new object[] { e.Row }) != null;

			PXUIFieldAttribute.SetEnabled<DRScheduleDetail.componentID>(e.Cache, e.Row, !hasTransactions);
			PXUIFieldAttribute.SetEnabled<DRScheduleDetail.defCode>(e.Cache, e.Row, !hasTransactions);
			PXUIFieldAttribute.SetEnabled<DRScheduleDetail.taskID>(e.Cache, e.Row, !hasTransactions);
			PXUIFieldAttribute.SetEnabled<DRScheduleDetail.termStartDate>(e.Cache, e.Row, !hasTransactions);
			PXUIFieldAttribute.SetEnabled<DRScheduleDetail.termEndDate>(e.Cache, e.Row, !hasTransactions);
		}

		protected virtual void _(Events.RowUpdated<DRSchedule> e)
		{
			if (e.Cache.ObjectsEqual<DRSchedule.refNbr>(e.Row, e.OldRow) == true)
			{
				return;
			}

			if (e.Row.Module == BatchModule.AR && e.OldRow.RefNbr != null)
			{
				SetDRCounterOnOrigDoc(e.OldRow);
			}
		}

		protected virtual void _(Events.RowDeleted<DRSchedule> e)
		{
			if (e.Row.Module == BatchModule.AR && e.Row.RefNbr != null)
			{
				SetDRCounterOnOrigDoc(e.Row);
			}
		}

		private void SetDRCounterOnOrigDoc(DRSchedule row)
		{
			ARRegister previousDocAttached = PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<DRSchedule.docType>>,
					And<ARRegister.refNbr, Equal<Required<DRSchedule.refNbr>>>>>.Select(Base, row.DocType, row.RefNbr);

			previousDocAttached.DRSchedCntr = PXSelect<ARTran,
			Where<ARTran.tranType, Equal<Required<ARInvoice.docType>>,
				And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
				And<ARTran.deferredCode, IsNotNull,
				And<Where<
					ARTran.lineType, IsNull,
					Or<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>>>>>
			.Select(Base, row.DocType, row.RefNbr).Count;

			OriginalDocument.Update(previousDocAttached);
		}

		protected virtual void _(Events.FieldUpdated<DRSchedule, DRSchedule.isOverridden> e)
		{
			if (e.Row == null) return;

			if ((e.ExternalCall || e.Cache.Graph.IsImport)
				&& e.Row.IsOverridden == false && (bool)e.OldValue == true
				&& e.Row.DocType != null && e.Row.RefNbr != null)
			{
				SingleScheduleCreator.RecalculateSchedule(Base);
			}

			if ((e.ExternalCall || e.Cache.Graph.IsImport)
				&& e.Row.IsOverridden == true && (bool)e.OldValue == false
				&& e.Row.DocType != null && e.Row.RefNbr != null)
			{
				var copyRow = (DRSchedule)e.Cache.CreateCopy(e.Row);
				copyRow.IsRecalculated = false;
				e.Cache.Update(copyRow);
			}

			Base.ReallocationPool.View.RequestRefresh();
		}

		protected virtual void DRScheduleDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e, PXRowInserting baseDelegate)
		{
			if(Base.Schedule.Current?.Module == BatchModule.AP)
			{
				baseDelegate.Invoke(sender, e);
			}

			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null || Base.Schedule.Current == null)
			{
				return;
			}

			scheduleDetail.ComponentID = scheduleDetail.ComponentID ?? DRScheduleDetail.EmptyComponentID;
			scheduleDetail.ScheduleID = Base.Schedule.Current.ScheduleID;
		}

		public virtual IEnumerable associated([PXDBString] string scheduleNbr)
		{
			IEnumerable emptyScheduleList = new List<DraftScheduleMaint.DRScheduleEx>();

			if (scheduleNbr == null)
			{
				return emptyScheduleList;
			}

			DRSchedule deferralSchedule = PXSelect<DRSchedule,
				Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>
				.Select(Base);

			if (deferralSchedule == null)
			{
				return emptyScheduleList;
			}

			if (deferralSchedule.Module == BatchModule.AR)
			{
				if (deferralSchedule.DocType == ARDocType.CreditMemo)
				{
					var scheduleDetailsByDocumentLine = PXSelectJoin<DraftScheduleMaint.DRScheduleEx,
						InnerJoin<ARTran, On<DraftScheduleMaint.DRScheduleEx.scheduleID, Equal<ARTran.defScheduleID>>>,
						Where<ARTran.tranType, Equal<Current<DraftScheduleMaint.DRScheduleEx.docType>>,
							And<ARTran.refNbr, Equal<Current<DraftScheduleMaint.DRScheduleEx.refNbr>>>>>
						.Select(Base);

					if (scheduleDetailsByDocumentLine != null && scheduleDetailsByDocumentLine.Count > 0)
					{
						return scheduleDetailsByDocumentLine;
					}
				}
				else if (deferralSchedule.DocType == ARDocType.Invoice || deferralSchedule.DocType == ARDocType.DebitMemo)
				{
					var list = new List<DraftScheduleMaint.DRScheduleEx>();

					foreach (DraftScheduleMaint.DRScheduleEx dRScheduleEx in PXSelectJoin<DraftScheduleMaint.DRScheduleEx,
							InnerJoin<ARTran, On<DraftScheduleMaint.DRScheduleEx.module, Equal<BatchModule.moduleAR>,
								And<DraftScheduleMaint.DRScheduleEx.docType, Equal<ARTran.tranType>,
								And<DraftScheduleMaint.DRScheduleEx.refNbr, Equal<ARTran.refNbr>>>>>,
							Where<ARTran.defScheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>
							.Select(Base))
					{
						list.Add(dRScheduleEx);
					}

					return list;
				}
			}
			else if (deferralSchedule.Module == BatchModule.AP)
			{
				return Base.associated(scheduleNbr);
			}

			return emptyScheduleList;
		}
	}
}
