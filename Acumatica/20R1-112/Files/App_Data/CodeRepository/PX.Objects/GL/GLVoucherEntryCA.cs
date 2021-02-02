using PX.Data;
using PX.Objects.GL;
using System;
using System.Collections;
using ContextFieldDescriptor = PX.Data.PXGraph.ContextFieldDescriptor;
using PX.Objects.CS;

namespace PX.Objects.CA
{

	public class CATranEntryExt : PXGraphExtension<CATranEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.SetContextFields(new ContextFieldDescriptor[] { 
															new ContextFieldDescriptor<GLVoucher.refNbr>(false),															
															new ContextFieldDescriptor<CAAdj.adjRefNbr>(true, true), 
															new ContextFieldDescriptor<CAAdj.adjTranType>(true, false),
															new ContextFieldDescriptor<CAAdj.tranDate>(false),
															new ContextFieldDescriptor<CAAdj.cashAccountID>(false, true),
															new ContextFieldDescriptor<CAAdj.curyID>(false),
															new ContextFieldDescriptor<CAAdj.curyTranAmt>(false, true),
															new ContextFieldDescriptor<CAAdj.status>(false, true),
															new ContextFieldDescriptor<CAAdj.entryTypeID>(false, true)
															});
			Base.SetContextHeaderFields(typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));
			Base.CAAdjRecords.Join<LeftJoin<GLVoucher, On<GLVoucher.docType, Equal<CAAdj.adjTranType>,
					And<GLVoucher.refNbr, Equal<CAAdj.adjRefNbr>, And<GLVoucher.module, Equal<GL.BatchModule.moduleCA>>>>>>();
			Base.Reverse.SetVisible(false);
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
		}

		#region Buttons
		public PXAction<CAAdj> SaveAndAdd;
		[PXUIField(DisplayName = GL.Messages.SaveAndAdd, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ShortcutCtrl = true, ShortcutChar = (char)65, Tooltip = GL.Messages.SaveAndAddToolTip)] //Ctrl-A
		public virtual IEnumerable saveAndAdd(PXAdapter adapter)
		{
			Base.Save.Press();
			return Base.Insert.Press(adapter);
		}
		public PXAction<CAAdj> viewVoucherBatch;
		[PXUIField(DisplayName = GL.Messages.ViewVoucherBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewVoucherBatch(PXAdapter adapter)
		{
			GLVoucherBatchEntry graph = PXGraph.CreateInstance<GLVoucherBatchEntry>();
			graph.filter.Current = new GLVoucherBatchEntry.Filter();
			graph.filter.Current.WorkBookID = Voucher.Current.WorkBookID;
			graph.VoucherBatches.Current = graph.VoucherBatches.Search<GLVoucherBatch.voucherBatchNbr>(Voucher.Current.VoucherBatchNbr);
			graph.VouchersInBatch.Current = graph.VouchersInBatch.Search<GLVoucher.refNbr, GLVoucher.docType, GLVoucher.module>(Voucher.Current.RefNbr, Voucher.Current.DocType, Voucher.Current.Module);
			graph.ViewDocument.Press();
			return adapter.Get();
		}
		public PXAction<CAAdj> viewWorkBook;
		[PXUIField(DisplayName = GL.Messages.ViewWorkBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewWorkBook(PXAdapter adapter)
		{
			GLVoucherBatchEntry graph = PXGraph.CreateInstance<GLVoucherBatchEntry>();
			graph.filter.Current.WorkBookID = Voucher.Current.WorkBookID;
			graph.VoucherBatches.Current = graph.VoucherBatches.Search<GLVoucherBatch.voucherBatchNbr>(Voucher.Current.VoucherBatchNbr);
			throw new PXRedirectRequiredException(graph, "");
		}

		public PXAction<CAAdj> reverse;
		[PXUIField(DisplayName = "Reverse", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		public virtual IEnumerable Reverse(PXAdapter adapter)
		{
			bool isWithinContext = Base.IsWithinContext;
			if (isWithinContext)
			{
				GLVoucherEntryHelper.ReverseGLVoucher(Base, Base.Save, () => Base.reverse(adapter), VoucherBatch.Cache);
			}

			return Base.reverse(adapter);
		}
		#endregion

		#region Selects & Delegates
		protected virtual IEnumerable caadjRecords()
		{
			return Base.SelectWithinContext(
				new Select2<CAAdj,
						InnerJoin<GLVoucher, On<GLVoucher.refNoteID, Equal<CAAdj.noteID>>>,
						Where<CAAdj.draft, Equal<False>,
							And<GLVoucher.workBookID, Equal<Context<GLVoucherBatch.workBookID>>,
							And<GLVoucher.voucherBatchNbr, Equal<Context<GLVoucherBatch.voucherBatchNbr>>>>>>());
		}

		public PXSelect<GLVoucher,
					Where<GLVoucher.docType, Equal<Current<CAAdj.adjTranType>>,
						And<GLVoucher.refNbr, Equal<Current<CAAdj.adjRefNbr>>,
						And<GLVoucher.module, Equal<GL.BatchModule.moduleCA>>>>> Voucher;

		public PXSelect<GLVoucherBatch,
					Where<GLVoucherBatch.workBookID, Equal<Optional<GLVoucher.workBookID>>,
						And<GLVoucherBatch.voucherBatchNbr, Equal<Optional<GLVoucher.voucherBatchNbr>>>>> VoucherBatch;

        #endregion

        #region CacheAttached - Voucher Batch
        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNbr"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(CAAdj.adjRefNbr))]
		[PXParent(typeof(Select<CAAdj, Where<CAAdj.adjTranType, Equal<Current<GLVoucher.docType>>,
							And<CAAdj.adjRefNbr, Equal<Current<GLVoucher.refNbr>>>>>))]
		protected virtual void GLVoucher_RefNbr_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsUnicode = true)]
		[PXDBDefault(typeof(CAAdj.adjTranType))]
		protected virtual void GLVoucher_DocType_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNoteID"/> field.
        /// </summary>
		[PXDBGuid()]
		[PXDefault(typeof(CAAdj.noteID), PersistingCheck = PXPersistingCheck.Null)]
		protected virtual void GLVoucher_RefNoteID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region Events - GLVoucher
		protected virtual void GLVoucher_WorkbookID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.workBookID>();
		}

		protected virtual void GLVoucher_Module_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GL.BatchModule.CA;
		}

		protected virtual void GLVoucher_VoucherBatchNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
		}
		#endregion

		#region Events - CAAdj
		protected virtual void CAAdj_AdjRefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			CAAdj row = e.Row as CAAdj;
			SetNumbering(sender, row);
			if (bs != null)
				bs(sender, e);
		}
		private void SetNumbering(PXCache sender, CAAdj row)
		{
			if (row == null) return;
			if (Base.IsWithinContext)
			{
				Numbering numbering = PXSelectJoin<Numbering,
					InnerJoin<GLWorkBook, On<GLWorkBook.voucherNumberingID, Equal<Numbering.numberingID>>>,
					Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				
				if (numbering.UserNumbering == true)
				{
					sender.RaiseExceptionHandling<CAAdj.adjRefNbr>(row, row.RefNbr, new PXException(GL.Messages.ManualNumberingDisabled));
				}

                AutoNumberAttribute.SetNumberingId<CAAdj.adjRefNbr>(sender, numbering.NumberingID);
			}
		}
		protected virtual void CAAdj_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted bs)
		{
			if (bs != null)
				bs(sender, e);
			bool isWithinContext = Base.IsWithinContext;
			if (isWithinContext)
			{
				string vb = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
				string wbID = Base.GetContextValue<GLVoucherBatch.workBookID>();
				this.VoucherBatch.Current = this.VoucherBatch.Select(wbID, vb);
				GLWorkBook wb = PXSelect<GLWorkBook,
					Where<GLWorkBook.workBookID, Equal<Required<GLVoucherBatch.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				if (!String.IsNullOrEmpty(vb))
				{
					Guid? noteID = PXNoteAttribute.GetNoteID<CAAdj.noteID>(sender, e.Row);
					this.Voucher.Insert(new GLVoucher());
					this.Voucher.Cache.IsDirty = false;
					Base.Caches[typeof(Note)].IsDirty = false;
				}

				var row = PXCache<CAAdj>.CreateCopy((CAAdj)e.Row);

				if (wb.DefaultDescription != null && row.TranDesc == null)
				{
					row.TranDesc = wb.DefaultDescription;
				}
				if (wb.DefaultCashAccountID != null && row.CashAccountID == null)
				{
					row.CashAccountID = wb.DefaultCashAccountID;
				}
				if (wb.DefaultEntryTypeID != null && row.EntryTypeID == null)
				{
					row.EntryTypeID = wb.DefaultEntryTypeID;
				}

				sender.Update(row);
			}
		}
		protected virtual void CAAdj_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting bs)
		{
			if (bs != null)
				bs(sender, e);
			CAAdj row = e.Row as CAAdj;
			SetNumbering(sender, row);
		}

	

		protected virtual void CAAdj_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected bs)
		{
			if (bs != null)
				bs(sender, e);
			CAAdj row = e.Row as CAAdj;
			if (row == null) return;
			bool isWithinContext = Base.IsWithinContext;
			GLVoucher voucher = this.Voucher.Select();
			GLVoucherBatch voucherBatch = this.VoucherBatch.Select();
			PXCache voucherCache = this.Voucher.Cache;
			bool isDetached = (voucher == null);
			PXUIFieldAttribute.SetEnabled<GLVoucher.refNbr>(voucherCache, voucher, false);
			PXUIFieldAttribute.SetVisible<GLVoucher.refNbr>(voucherCache, voucher, !isDetached);
			PXUIFieldAttribute.SetEnabled<CAAdj.adjRefNbr>(sender, e.Row, !isWithinContext);
			this.SaveAndAdd.SetVisible(!isDetached && isWithinContext);
			this.SaveAndAdd.SetEnabled(!isDetached && isWithinContext);
			if (isWithinContext)
			{
				Base.Release.SetVisible(false);
				Base.Release.SetEnabled(false);
			}
			PXUIFieldAttribute.SetVisible<GLVoucher.voucherBatchNbr>(Voucher.Cache, null, !isDetached && !isWithinContext);
			PXUIFieldAttribute.SetVisible<GLVoucher.workBookID>(Voucher.Cache, null, !isDetached && !isWithinContext);
			if (isWithinContext && !isDetached)
			{
				sender.AllowInsert = !(voucherBatch != null && voucherBatch.Released == true);
			}
		}
		#endregion
	}
}