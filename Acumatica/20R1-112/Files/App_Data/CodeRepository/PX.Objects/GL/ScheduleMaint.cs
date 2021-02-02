using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.Common;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.GL.Overrides.ScheduleMaint;

namespace PX.Objects.GL
{
	public class ScheduleMaint : ScheduleMaintBase<ScheduleMaint, ScheduleProcess>
	{
		#region Views
		public PXSelect<
			Batch,
			Where<
				Batch.scheduleID, Equal<Current<Schedule.scheduleID>>,
				And<Batch.scheduled, Equal<False>>>>
			Batch_History;

		public PXSelect<
			BatchSelection,
			Where<
				BatchSelection.scheduleID, Equal<Current<Schedule.scheduleID>>,
				And<BatchSelection.scheduled, Equal<True>>>>
			Batch_Detail;

		public PXSelect<GLTran> GLTransactions;
		[Obsolete("Will be removed in Acumatica 2019R1")]
		public PXSelect<CATran> CATransactions;

		public PXSetup<GLSetup> GLSetup;

		#endregion

		public ScheduleMaint()
		{
			GLSetup gls = GLSetup.Current;

			Batch_History.Cache.AllowDelete = false;
			Batch_History.Cache.AllowInsert = false;
			Batch_History.Cache.AllowUpdate = false;

			Schedule_Header.WhereAnd<Where<Schedule.module, Equal<BatchModule.moduleGL>>>();

			Views.Caches.Remove(typeof(Batch));
		}

		internal override bool AnyScheduleDetails() => Batch_Detail.Any();

		public PXAction<Schedule> viewBatchD;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatchD(PXAdapter adapter)
		{
			Batch row = Batch_Detail.Current;
			if (row != null)
			{
				JournalEntry graph = CreateInstance<JournalEntry>();
				graph.BatchModule.Current = row;
				throw new PXRedirectRequiredException(graph, true, "View Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<Schedule> viewBatch;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			Batch row = Batch_History.Current;
			if (row != null)
			{
				JournalEntry graph = CreateInstance<JournalEntry>();
				graph.BatchModule.Current = row;
				throw new PXRedirectRequiredException(graph, true, "View Batch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Module", Enabled = false, IsReadOnly = true, Visible = false)]
		protected virtual void Batch_Module_CacheAttached(PXCache sender) { }

		[PXDBLong]
		[Overrides.ScheduleMaint.GLCashTranID]
		protected virtual void GLTran_CATranID_CacheAttached(PXCache sender) { }

		protected virtual void BatchSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || string.IsNullOrWhiteSpace(((BatchSelection)e.Row).BatchNbr)) return;

			PXUIFieldAttribute.SetEnabled<BatchSelection.module>(sender, e.Row, !(bool)((BatchSelection)e.Row).Scheduled);
			PXUIFieldAttribute.SetEnabled<BatchSelection.batchNbr>(sender, e.Row, !(bool)((BatchSelection)e.Row).Scheduled);
		}

		protected override void SetControlsState(PXCache cache, Schedule s)
		{
			base.SetControlsState(cache, s);

			PXUIFieldAttribute.SetEnabled<BatchSelection.module>(Batch_Detail.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<BatchSelection.batchNbr>(Batch_Detail.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<BatchSelection.ledgerID>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.dateEntered>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.finPeriodID>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.curyControlTotal>(Batch_Detail.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<BatchSelection.curyID>(Batch_Detail.Cache, null, false);
		}

		protected virtual void BatchSelection_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Batch b = e.Row as Batch;
			if (b != null && b.Voided == false)
			{
				b.Scheduled = true;
				b.ScheduleID = Schedule_Header.Current.ScheduleID;
			}

			BatchSelection batch = e.Row as BatchSelection;
			if (batch != null && !string.IsNullOrWhiteSpace(batch.Module) && !string.IsNullOrWhiteSpace(batch.BatchNbr) &&
				PXSelectorAttribute.Select<BatchSelection.batchNbr>(cache, batch) == null)
			{
				cache.RaiseExceptionHandling<BatchSelection.batchNbr>(batch, batch.BatchNbr, new PXSetPropertyException(Messages.BatchNbrNotValid));
				Batch_Detail.Cache.Remove(batch);
			}
		}

		protected virtual void BatchSelection_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			BatchSelection batch = e.Row as BatchSelection;

			if (batch != null && 
				!string.IsNullOrWhiteSpace(batch.Module) && 
				!string.IsNullOrWhiteSpace(batch.BatchNbr))
			{
                batch = PXSelectReadonly<
					BatchSelection,
                    Where<
						BatchSelection.module, Equal<Required<BatchSelection.module>>,
						And<BatchSelection.batchNbr, Equal<Required<BatchSelection.batchNbr>>>>>
					.Select(this, batch.Module, batch.BatchNbr);

                PXSelectorAttribute selectorAttr = (PXSelectorAttribute)sender.GetAttributesReadonly<BatchSelection.batchNbr>(batch).FirstOrDefault(
					(PXEventSubscriberAttribute attr) => { return attr is PXSelectorAttribute; });

				BqlCommand selectorSearch = selectorAttr.GetSelect();

				if (batch != null && selectorSearch.Meet(sender, batch))
				{
					Batch_Detail.Delete(batch);
					Batch_Detail.Update(batch);
				}
				else
				{
					batch = (BatchSelection)e.Row;
					sender.RaiseExceptionHandling<BatchSelection.batchNbr>(batch, batch.BatchNbr, new PXSetPropertyException(Messages.BatchNbrNotValid));
					Batch_Detail.Delete(batch);
				}
			}
		}

		protected virtual void Schedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (BatchSelection batch in PXSelect<
				BatchSelection, 
				Where<
					BatchSelection.scheduleID, Equal<Required<Schedule.scheduleID>>>>
				.Select(this, ((Schedule)e.Row).ScheduleID))
			{
				if (batch.Scheduled == true)
				{
					batch.Voided = true;
					batch.Scheduled = false;
				}

				batch.ScheduleID = null;

				Batch_Detail.Cache.MarkUpdated(batch);

				PXDBDefaultAttribute.SetDefaultForUpdate<BatchSelection.scheduleID>(Batch_Detail.Cache, batch, false);
			}
		}

		public override void Persist()
		{
            foreach (BatchSelection updatedBatch in Batch_Detail.Cache.Updated)
			{
				UpdateTransactionsLedgerBalanceType(updatedBatch);

				if (updatedBatch.Voided == false)
				{
					updatedBatch.Scheduled = true;
					updatedBatch.ScheduleID = Schedule_Header.Current.ScheduleID;

					Batch_Detail.Cache.Update(updatedBatch);
				}
			}

			foreach (BatchSelection deletedBatch in Batch_Detail.Cache.Deleted)
			{
				PXDBDefaultAttribute.SetDefaultForUpdate<BatchSelection.scheduleID>(Batch_Detail.Cache, deletedBatch, false);

				UpdateTransactionsLedgerBalanceType(deletedBatch);

				deletedBatch.Scheduled = false;
				deletedBatch.Voided = true;
				deletedBatch.ScheduleID = null;

				Batch_Detail.Cache.SetStatus(deletedBatch, PXEntryStatus.Updated);
				Batch_Detail.Cache.Update(deletedBatch);
			}

			base.Persist();
		}

		private void UpdateTransactionsLedgerBalanceType(BatchSelection updatedBatch)
		{
			foreach (GLTran glTransaction in PXSelect<GLTran,
									Where<GLTran.module, Equal<Current<BatchSelection.module>>,
										And<GLTran.batchNbr, Equal<Current<BatchSelection.batchNbr>>>>>
									.SelectMultiBound(this, new object[] { updatedBatch }))
			{
				glTransaction.LedgerBalanceType = "N";

				GLTransactions.Update(glTransaction);
			}
		}

		[Obsolete("Will be removed in Acumatica 2019R1")]
		protected virtual void RemoveBatchCashTransactions(BatchSelection batch)
		{
			foreach (CATran cashTransaction in PXSelect<
				CATran,
				Where<
					CATran.origModule, Equal<Current<BatchSelection.module>>,
					And<CATran.origTranType, Equal<GLTranType.gLEntry>,
					And<CATran.origRefNbr, Equal<Current<BatchSelection.batchNbr>>>>>>
				.SelectMultiBound(this, new object[] { batch }))
			{
				CATransactions.Delete(cashTransaction);
			}
		}
	}
}

namespace PX.Objects.GL.Overrides.ScheduleMaint
{
	public class GLCashTranIDAttribute : GL.GLCashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return DefaultValues<BatchSelection, BatchSelection.module, BatchSelection.batchNbr>(sender, catran_Row, orig_Row);
		}
	}

	[PXPrimaryGraph(null)]
    [PXCacheName(Messages.BatchSelection)]
    [Serializable]
	public partial class BatchSelection : Batch
	{
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Visible = false, IsReadOnly = true)]
		public override String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion		
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXSelector(typeof(Search2<
			Batch.batchNbr, 
				LeftJoin<GLVoucher, 
					On<GLVoucher.refNoteID, Equal<Batch.noteID>, 
					And<FeatureInstalled<FeaturesSet.gLWorkBooks>>>>, 
			Where<
				Batch.released, Equal<False>, 
				And<Batch.hold, Equal<False>, 
				And<Batch.voided, Equal<False>, 
				And<Batch.module, Equal<BatchModule.moduleGL>, 
				And<Batch.batchType, NotEqual<BatchTypeCode.allocation>,
				And<Batch.batchType, NotEqual<BatchTypeCode.reclassification>,
				And<Batch.batchType, NotEqual<BatchTypeCode.trialBalance>,
				And<GLVoucher.refNbr, IsNull>>>>>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		public override string BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region ScheduleID
		public new abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(Schedule.scheduleID))]
		[PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<Batch.scheduleID>>>>), LeaveChildren=true)]
		public override string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID(
			typeof(Batch.dateEntered),
			typeof(Batch.branchID),
			masterFinPeriodIDType: typeof(Batch.tranPeriodID),
			IsHeader = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
		public override String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		[PeriodID]		
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region Scheduled
		public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion
		#region CreatedByID
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region LastModifiedByID
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
	}
}