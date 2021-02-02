using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections;
using ContextFieldDescriptor = PX.Data.PXGraph.ContextFieldDescriptor;


namespace PX.Objects.AP
{
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.SetContextFields(new ContextFieldDescriptor[] {
															new ContextFieldDescriptor<GLVoucher.refNbr>(false),
															new ContextFieldDescriptor<APInvoice.refNbr>(true, false),
															new ContextFieldDescriptor<APInvoice.docType>(true, false),
															new ContextFieldDescriptor<APInvoice.docDate>(false),
															new ContextFieldDescriptor<APInvoice.status>(false),
															new ContextFieldDescriptor<APInvoice.invoiceNbr>(false, true),
															new ContextFieldDescriptor<APInvoice.curyID>(false,true),
															new ContextFieldDescriptor<APInvoice.curyOrigDocAmt>(false, true),
															new ContextFieldDescriptor<APInvoice.vendorID_Vendor_acctName>(false, true),
															});

			Base.SetContextHeaderFields(typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));

			Base.Document.Join<LeftJoin<GLVoucher, On<GLVoucher.docType, Equal<APInvoice.docType>,
					And<GLVoucher.refNbr, Equal<APInvoice.refNbr>, And<GLVoucher.module, Equal<GL.BatchModule.moduleAP>>>>>>();
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
		}

		#region Buttons
		public PXAction<APInvoice> SaveAndAdd;
		[PXUIField(DisplayName = GL.Messages.SaveAndAdd, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ShortcutCtrl = true, ShortcutChar = (char)65, Tooltip = GL.Messages.SaveAndAddToolTip)] //Ctrl-A
		public virtual IEnumerable saveAndAdd(PXAdapter adapter)
		{
			Base.Save.Press();
			return Base.Insert.Press(adapter);
#if USE_DOC_TEMPLATE
			Base.Save.Press();			
			Base.Insert.Press();
			if (this.Base.IsWithinContext == true)
			{
				string wbID = Base.GetContextValue<GLVoucherBatch.workBookID>();
				GLWorkBook wb = PXSelect<GLWorkBook, Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, wbID);
				if (wb != null && wb.TemplateID.HasValue)
				{
					var currentUserClipboard = PXCopyPasteData<PXGraph>.CurrentUserClipboard;
					currentUserClipboard.LoadTemplateFromDb(Convert.ToInt32(wb.TemplateID));
					currentUserClipboard.PasteTo(this.Base);
				}
			}

			List<APInvoice> res = new List<APInvoice>(1);
			res.Add(this.Base.Document.Current);
			return res; 
#endif
		}
		public PXAction<APInvoice> viewVoucherBatch;
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
		public PXAction<APInvoice> viewWorkBook;
		[PXUIField(DisplayName = GL.Messages.ViewWorkBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewWorkBook(PXAdapter adapter)
		{
			GLVoucherBatchEntry graph = PXGraph.CreateInstance<GLVoucherBatchEntry>();
			graph.filter.Current.WorkBookID = Voucher.Current.WorkBookID;
			graph.VoucherBatches.Current = graph.VoucherBatches.Search<GLVoucherBatch.voucherBatchNbr>(Voucher.Current.VoucherBatchNbr);
			throw new PXRedirectRequiredException(graph, "");
		}

		public PXAction<APInvoice> reverseInvoice;
		[PXUIField(DisplayName = "Reverse Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		public virtual IEnumerable ReverseInvoice(PXAdapter adapter)
		{
			bool isWithinContext = Base.IsWithinContext;
			if (isWithinContext)
			{
				GLVoucherEntryHelper.ReverseGLVoucher(Base, Base.Save, () => Base.ReverseInvoice(adapter), VoucherBatch.Cache);
			}

			return Base.ReverseInvoice(adapter);
		}
		#endregion

		#region Selects & Delegates
		protected virtual IEnumerable document()
		{
			return Base.SelectWithinContext(
				new Select2<APInvoice,
						LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APInvoice.vendorID>>,
						InnerJoin<GLVoucher, On<GLVoucher.refNoteID, Equal<APInvoice.noteID>>>>,
						Where<APInvoice.docType, Equal<Optional<APInvoice.docType>>,
							And<GLVoucher.workBookID, Equal<Context<GLVoucherBatch.workBookID>>,
							And<GLVoucher.voucherBatchNbr, Equal<Context<GLVoucherBatch.voucherBatchNbr>>,
							And2<Where<APInvoice.origModule, NotEqual<BatchModule.moduleTX>, Or<APInvoice.released, Equal<True>>>,
							And<Where<Vendor.bAccountID, IsNull, Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>>());
		}

		public PXSelect<GLVoucher,
					Where<GLVoucher.docType, Equal<Current<APInvoice.docType>>,
						And<GLVoucher.refNbr, Equal<Current<APInvoice.refNbr>>,
						And<GLVoucher.module, Equal<GL.BatchModule.moduleAP>>>>> Voucher;

		public PXSelect<GLVoucherBatch,
					Where<GLVoucherBatch.workBookID, Equal<Optional<GLVoucher.workBookID>>,
						And<GLVoucherBatch.voucherBatchNbr, Equal<Optional<GLVoucher.voucherBatchNbr>>>>> VoucherBatch;

        #endregion

        #region CacheAttached - Voucher Batch
        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNbr"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(APInvoice.refNbr))]
		[PXParent(typeof(Select<APInvoice, Where<APInvoice.docType, Equal<Current<GLVoucher.docType>>,
							And<APInvoice.refNbr, Equal<Current<GLVoucher.refNbr>>>>>))]
		protected virtual void GLVoucher_RefNbr_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsUnicode = true)]
		[PXDBDefault(typeof(APInvoice.docType))]
		protected virtual void GLVoucher_DocType_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNoteID"/> field.
        /// </summary>
		[PXDBGuid]
		[PXDefault(typeof(APInvoice.noteID), PersistingCheck = PXPersistingCheck.Null)]
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
			e.NewValue = GL.BatchModule.AP;
		}

		protected virtual void GLVoucher_VoucherBatchNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
		}
		#endregion

		#region Events - APInvoice
		protected virtual void APInvoice_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			SetNumbering(sender, e.Row as APInvoice);
			if (bs != null)
				bs(sender, e);
		}
		private void SetNumbering(PXCache sender, APInvoice row)
		{
			if (row == null) return;
			if (Base.IsWithinContext)
			{
				Numbering numbering = PXSelectJoin<Numbering,
					InnerJoin<GLWorkBook, On<GLWorkBook.voucherNumberingID, Equal<Numbering.numberingID>>>,
					Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				
				if (numbering.UserNumbering == true)
				{
					sender.RaiseExceptionHandling<APInvoice.refNbr>(row, row.RefNbr, new PXException(GL.Messages.ManualNumberingDisabled));
				}

                AutoNumberAttribute.SetNumberingId<APInvoice.refNbr>(sender, row.DocType, numbering.NumberingID);
			}
		}

		protected virtual void APInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted bs)
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
				sender.SetValueExt<APInvoice.docType>(e.Row, wb.DocType);
				if (!String.IsNullOrEmpty(vb))
				{
					Guid? noteID = PXNoteAttribute.GetNoteID<APInvoice.noteID>(sender, e.Row);
					this.Voucher.Insert(new GLVoucher());
					this.Voucher.Cache.IsDirty = false;
					Base.Caches[typeof(Note)].IsDirty = false;
				}

				var row = (APInvoice) e.Row;

				if (wb.DefaultDescription != null && row.DocDesc == null)
				{
					sender.SetValueExt<APInvoice.docDesc>(e.Row, wb.DefaultDescription);
				}
				if (wb.DefaultBAccountID != null && row.VendorID == null)
				{
					sender.SetValueExt<APInvoice.vendorID>(e.Row, wb.DefaultBAccountID);

					if (wb.DefaultLocationID != null)
					{
						sender.SetValueExt<APInvoice.vendorLocationID>(e.Row, wb.DefaultLocationID);
					}
				}
			}
		}
		protected virtual void APInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting bs)
		{
			if (bs != null)
				bs(sender, e);
			SetNumbering(sender, e.Row as APInvoice);
		}

	
		protected virtual void APInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected bs)
		{
			if (bs != null)
				bs(sender, e);
			APInvoice row = e.Row as APInvoice;
			if (row == null) return;
			bool isWithinContext = Base.IsWithinContext;
			GLVoucher voucher = this.Voucher.Select();
			GLVoucherBatch voucherBatch = this.VoucherBatch.Select();
			PXCache voucherCache = this.Voucher.Cache;
			bool isDetached = (voucher == null);
			PXUIFieldAttribute.SetEnabled<GLVoucher.refNbr>(voucherCache, voucher, false);
			PXUIFieldAttribute.SetVisible<GLVoucher.refNbr>(voucherCache, voucher, !isDetached);
			PXUIFieldAttribute.SetEnabled<APInvoice.refNbr>(sender, e.Row, !isWithinContext);
			this.SaveAndAdd.SetVisible(!isDetached && isWithinContext);
			this.SaveAndAdd.SetEnabled(!isDetached && isWithinContext);
			if (isWithinContext)
			{
				Base.release.SetVisible(false);
				Base.release.SetEnabled(false);
				Base.voidInvoice.SetVisible(false);
				Base.voidInvoice.SetEnabled(false);
				Base.voidDocument.SetVisible(false);
				Base.voidDocument.SetEnabled(false);
				Base.voidCheck.SetVisible(false);
				Base.voidCheck.SetEnabled(false);
			}
			if (!isDetached)
			{
				try
				{
					Base.action.SetEnabled("Add to Schedule", false);
				}
				catch { }
				Base.createSchedule.SetEnabled(false);
			}
			PXUIFieldAttribute.SetVisible<GLVoucher.voucherBatchNbr>(Voucher.Cache, null, !isDetached && !isWithinContext);
			PXUIFieldAttribute.SetVisible<GLVoucher.workBookID>(Voucher.Cache, null, !isDetached && !isWithinContext);
			if (isWithinContext && !isDetached)
			{
				sender.AllowInsert = !(voucherBatch != null && voucherBatch.Released == true);
			}
		}
		#endregion

		public virtual bool IsAttached(APInvoice aRow, out GLVoucherBatch aVoucherBatch, out GLVoucher aVoucher)
		{
			return IsAttached(Base, aRow.NoteID, out aVoucherBatch, out aVoucher);
		}
		public static bool IsAttached(PXGraph aGraph, Guid? aNoteID, out GLVoucherBatch aVoucherBatch, out GLVoucher aVoucher)
		{
			aVoucher = null;
			aVoucherBatch = null;
			if (aNoteID.HasValue)
			{
				PXResult<GLVoucher, GLVoucherBatch> result = (PXResult<GLVoucher, GLVoucherBatch>)PXSelectJoin<GLVoucher,
							   InnerJoin<GLVoucherBatch, On<GLVoucherBatch.workBookID, Equal<GLVoucher.workBookID>,
											   And<GLVoucherBatch.voucherBatchNbr, Equal<GLVoucher.voucherBatchNbr>>>>,
							   Where<GLVoucher.refNoteID, Equal<Required<GLVoucher.refNoteID>>>>.Select(aGraph, aNoteID);
				if (result != null && !String.IsNullOrEmpty(((GLVoucher)result).RefNbr))
				{
					aVoucher = result;
					aVoucherBatch = result;
					return true;
				}
			}
			return false;
		}
	}

	public class APPaymentEntryExt : PXGraphExtension<APPaymentEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.SetContextFields(new ContextFieldDescriptor[] {
															new ContextFieldDescriptor<GLVoucher.refNbr>(false),
															new ContextFieldDescriptor<APPayment.refNbr>(true, true),
															new ContextFieldDescriptor<APPayment.docType>(true, false),
															new ContextFieldDescriptor<APPayment.docDate>(false),
															new ContextFieldDescriptor<APPayment.vendorID>(false, true),
															new ContextFieldDescriptor<APPayment.curyOrigDocAmt>(false, true),
															new ContextFieldDescriptor<APPayment.curyID>(false, true),
															new ContextFieldDescriptor<APPayment.status>(false, true),
															new ContextFieldDescriptor<APPayment.vendorID_Vendor_acctName>(false, true)
															});
			Base.SetContextHeaderFields(typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));
			Base.Document.Join<LeftJoin<GLVoucher, On<GLVoucher.docType, Equal<APPayment.docType>,
					And<GLVoucher.refNbr, Equal<APPayment.refNbr>, And<GLVoucher.module, Equal<GL.BatchModule.moduleAP>>>>>>();
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
		}

		#region Buttons
		public PXAction<APPayment> SaveAndAdd;
		[PXUIField(DisplayName = GL.Messages.SaveAndAdd, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ShortcutCtrl = true, ShortcutChar = (char)65, Tooltip = GL.Messages.SaveAndAddToolTip)] //Ctrl-A
		public virtual IEnumerable saveAndAdd(PXAdapter adapter)
		{
			Base.Save.Press();
			return Base.Insert.Press(adapter);
		}
		public PXAction<APPayment> viewVoucherBatch;
		[PXUIField(DisplayName = GL.Messages.ViewVoucherBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewVoucherBatch(PXAdapter adapter)
		{
			GLVoucherBatchEntry graph = PXGraph.CreateInstance<GLVoucherBatchEntry>();
			graph.filter.Current = new GLVoucherBatchEntry.Filter();
			graph.VoucherBatches.Current = graph.VoucherBatches.Search<GLVoucherBatch.voucherBatchNbr>(Voucher.Current.VoucherBatchNbr);
			graph.VouchersInBatch.Current = graph.VouchersInBatch.Search<GLVoucher.refNbr, GLVoucher.docType, GLVoucher.module>(Voucher.Current.RefNbr, Voucher.Current.DocType, Voucher.Current.Module);
			graph.ViewDocument.Press();
			return adapter.Get();
		}
		public PXAction<APPayment> viewWorkBook;
		[PXUIField(DisplayName = GL.Messages.ViewWorkBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewWorkBook(PXAdapter adapter)
		{
			GLVoucherBatchEntry graph = PXGraph.CreateInstance<GLVoucherBatchEntry>();
			graph.filter.Current = new GLVoucherBatchEntry.Filter();
			graph.filter.Current.WorkBookID = Voucher.Current.WorkBookID;
			graph.VoucherBatches.Current = graph.VoucherBatches.Search<GLVoucherBatch.voucherBatchNbr>(Voucher.Current.VoucherBatchNbr);
			throw new PXRedirectRequiredException(graph, "");
		}
		#endregion

		#region Selects & Delegates
		protected virtual IEnumerable document()
		{
			return Base.SelectWithinContext(
				new Select2<APPayment,
						LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APPayment.vendorID>>,
						InnerJoin<GLVoucher, On<GLVoucher.refNoteID, Equal<APPayment.noteID>>>>,
						Where<APPayment.docType, Equal<Optional<APPayment.docType>>,
							And<GLVoucher.workBookID, Equal<Context<GLVoucherBatch.workBookID>>,
							And<GLVoucher.voucherBatchNbr, Equal<Context<GLVoucherBatch.voucherBatchNbr>>,
							And2<Where<APPayment.origModule, NotEqual<BatchModule.moduleTX>, Or<APPayment.released, Equal<True>>>,
							And<Where<Vendor.bAccountID, IsNull, Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>>());
		}

		public PXSelect<GLVoucher,
					Where<GLVoucher.docType, Equal<Current<APPayment.docType>>,
						And<GLVoucher.refNbr, Equal<Current<APPayment.refNbr>>,
						And<GLVoucher.module, Equal<GL.BatchModule.moduleAP>>>>> Voucher;

		public PXSelect<GLVoucherBatch,
					Where<GLVoucherBatch.workBookID, Equal<Optional<GLVoucher.workBookID>>,
						And<GLVoucherBatch.voucherBatchNbr, Equal<Optional<GLVoucher.voucherBatchNbr>>>>> VoucherBatch;

        #endregion

        #region CacheAttached - Voucher Batch
        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNbr"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(APPayment.refNbr))]
		[PXParent(typeof(Select<APPayment, Where<APPayment.docType, Equal<Current<GLVoucher.docType>>,
							And<APPayment.refNbr, Equal<Current<GLVoucher.refNbr>>>>>))]
		protected virtual void GLVoucher_RefNbr_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsUnicode = true)]
		[PXDBDefault(typeof(APPayment.docType))]
		protected virtual void GLVoucher_DocType_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNoteID"/> field.
        /// </summary>
		[PXDBGuid]
		[PXDefault(typeof(APPayment.noteID), PersistingCheck = PXPersistingCheck.Null)]
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
			e.NewValue = GL.BatchModule.AP;
		}

		protected virtual void GLVoucher_VoucherBatchNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
		}
		#endregion

		#region Events - APPayment
		protected virtual void APPayment_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			SetNumbering(sender, e.Row as APPayment);
			if (bs != null)
				bs(sender, e);
		}
		private void SetNumbering(PXCache sender, APPayment row)
		{
			if (row == null) return;
			if (Base.IsWithinContext)
			{
				Numbering numbering = PXSelectJoin<Numbering,
					InnerJoin<GLWorkBook, On<GLWorkBook.voucherNumberingID, Equal<Numbering.numberingID>>>,
					Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				
				if (numbering.UserNumbering == true)
				{
					sender.RaiseExceptionHandling<APPayment.refNbr>(row, row.RefNbr, new PXException(GL.Messages.ManualNumberingDisabled));
				}

                AutoNumberAttribute.SetNumberingId<APPayment.refNbr>(sender, row.DocType, numbering.NumberingID);
			}
		}
		protected virtual void APPayment_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted bs)
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
				sender.SetValueExt<APPayment.docType>(e.Row, wb.DocType);
				if (!String.IsNullOrEmpty(vb))
				{
					Guid? noteID = PXNoteAttribute.GetNoteID<APPayment.noteID>(sender, e.Row);
					this.Voucher.Insert(new GLVoucher());
					this.Voucher.Cache.IsDirty = false;
					Base.Caches[typeof(Note)].IsDirty = false;
				}

				var row = (APPayment)e.Row;

				if (wb.DefaultDescription != null && row.DocDesc == null)
				{
					sender.SetValueExt<APPayment.docDesc>(e.Row, wb.DefaultDescription);
				}
				if (wb.DefaultBAccountID != null && row.VendorID == null)
				{
					sender.SetValueExt<APPayment.vendorID>(e.Row, wb.DefaultBAccountID);

					if (wb.DefaultLocationID != null)
					{
						sender.SetValueExt<APPayment.vendorLocationID>(e.Row, wb.DefaultLocationID);
					}
				}
			}
		}
		protected virtual void APPayment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting bs)
		{
			if (bs != null)
				bs(sender, e);
			SetNumbering(sender, e.Row as APPayment);
			
		}
		protected virtual void APPayment_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected bs)
		{
			if (bs != null)
				bs(sender, e);
			APPayment row = e.Row as APPayment;
			if (row == null) return;
			bool isWithinContext = Base.IsWithinContext;
			GLVoucher voucher = this.Voucher.Select();
			GLVoucherBatch voucherBatch = this.VoucherBatch.Select();
			PXCache voucherCache = this.Voucher.Cache;
			bool isDetached = (voucher == null);
			PXUIFieldAttribute.SetEnabled<GLVoucher.refNbr>(voucherCache, voucher, false);
			PXUIFieldAttribute.SetVisible<GLVoucher.refNbr>(voucherCache, voucher, !isDetached);
			PXUIFieldAttribute.SetEnabled<APPayment.refNbr>(sender, e.Row, !isWithinContext);
			this.SaveAndAdd.SetVisible(!isDetached && isWithinContext);
			this.SaveAndAdd.SetEnabled(!isDetached && isWithinContext);
			if (isWithinContext)
			{
				Base.release.SetVisible(false);
				Base.release.SetEnabled(false);
				Base.voidCheck.SetVisible(false);
				Base.voidCheck.SetEnabled(false);
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

	public class APQuickCheckEntryExt : PXGraphExtension<APQuickCheckEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.SetContextFields(new ContextFieldDescriptor[] {
															new ContextFieldDescriptor<GLVoucher.refNbr>(false),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.refNbr>(true, true),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.docType>(true, false),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.docDate>(false),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.vendorID>(false, true),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.curyOrigDocAmt>(false, true),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.curyID>(false, true),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.status>(false, true),
															new ContextFieldDescriptor<AP.Standalone.APQuickCheck.vendorID_Vendor_acctName>(false, true)
															});
			Base.SetContextHeaderFields(typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));
			Base.Document.Join<LeftJoin<GLVoucher, On<GLVoucher.docType, Equal<AP.Standalone.APQuickCheck.docType>,
					And<GLVoucher.refNbr, Equal<AP.Standalone.APQuickCheck.refNbr>, And<GLVoucher.module, Equal<GL.BatchModule.moduleAP>>>>>>();
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
		}

		#region Buttons
		public PXAction<AP.Standalone.APQuickCheck> SaveAndAdd;
		[PXUIField(DisplayName = GL.Messages.SaveAndAdd, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ShortcutCtrl = true, ShortcutChar = (char)65, Tooltip = GL.Messages.SaveAndAddToolTip)] //Ctrl-A
		public virtual IEnumerable saveAndAdd(PXAdapter adapter)
		{
			Base.Save.Press();
			return Base.Insert.Press(adapter);
		}
		public PXAction<AP.Standalone.APQuickCheck> viewVoucherBatch;
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
		public PXAction<AP.Standalone.APQuickCheck> viewWorkBook;
		[PXUIField(DisplayName = GL.Messages.ViewWorkBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewWorkBook(PXAdapter adapter)
		{
			GLVoucherBatchEntry graph = PXGraph.CreateInstance<GLVoucherBatchEntry>();
			graph.filter.Current.WorkBookID = Voucher.Current.WorkBookID;
			graph.VoucherBatches.Current = graph.VoucherBatches.Search<GLVoucherBatch.voucherBatchNbr>(Voucher.Current.VoucherBatchNbr);
			throw new PXRedirectRequiredException(graph, "");
		}
		#endregion

		#region Selects & Delegates
		protected virtual IEnumerable document()
		{
			return Base.SelectWithinContext(
				new Select2<AP.Standalone.APQuickCheck,
						LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<AP.Standalone.APQuickCheck.vendorID>>,
						InnerJoin<GLVoucher, On<GLVoucher.refNoteID, Equal<AP.Standalone.APQuickCheck.noteID>>>>,
						Where<AP.Standalone.APQuickCheck.docType, Equal<Optional<AP.Standalone.APQuickCheck.docType>>,
							And<GLVoucher.workBookID, Equal<Context<GLVoucherBatch.workBookID>>,
							And<GLVoucher.voucherBatchNbr, Equal<Context<GLVoucherBatch.voucherBatchNbr>>,
							And2<Where<AP.Standalone.APQuickCheck.origModule, NotEqual<BatchModule.moduleTX>, Or<AP.Standalone.APQuickCheck.released, Equal<True>>>,
							And<Where<Vendor.bAccountID, IsNull, Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>>());
		}

		public PXSelect<GLVoucher,
					Where<GLVoucher.docType, Equal<Current<AP.Standalone.APQuickCheck.docType>>,
						And<GLVoucher.refNbr, Equal<Current<AP.Standalone.APQuickCheck.refNbr>>,
						And<GLVoucher.module, Equal<GL.BatchModule.moduleAP>>>>> Voucher;

		public PXSelect<GLVoucherBatch,
					Where<GLVoucherBatch.workBookID, Equal<Optional<GLVoucher.workBookID>>,
						And<GLVoucherBatch.voucherBatchNbr, Equal<Optional<GLVoucher.voucherBatchNbr>>>>> VoucherBatch;

        #endregion

        #region CacheAttached - Voucher Batch
        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNbr"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(AP.Standalone.APQuickCheck.refNbr))]
		[PXParent(typeof(Select<AP.Standalone.APQuickCheck, Where<AP.Standalone.APQuickCheck.docType, Equal<Current<GLVoucher.docType>>,
							And<AP.Standalone.APQuickCheck.refNbr, Equal<Current<GLVoucher.refNbr>>>>>))]
		protected virtual void GLVoucher_RefNbr_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsUnicode = true)]
		[PXDBDefault(typeof(AP.Standalone.APQuickCheck.docType))]
		protected virtual void GLVoucher_DocType_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNoteID"/> field.
        /// </summary>
		[PXDBGuid]
		[PXDefault(typeof(AP.Standalone.APQuickCheck.noteID), PersistingCheck = PXPersistingCheck.Null)]
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
			e.NewValue = GL.BatchModule.AP;
		}

		protected virtual void GLVoucher_VoucherBatchNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
		}
		#endregion

		#region Events - APQuickCheck
		protected virtual void APQuickCheck_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			SetNumbering(sender, e.Row as AP.Standalone.APQuickCheck);
			if (bs != null)
				bs(sender, e);
		}
		private void SetNumbering(PXCache sender, AP.Standalone.APQuickCheck row)
		{
			if (row == null) return;
			if (Base.IsWithinContext)
			{
				Numbering numbering = PXSelectJoin<Numbering,
					InnerJoin<GLWorkBook, On<GLWorkBook.voucherNumberingID, Equal<Numbering.numberingID>>>,
					Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				
				if (numbering.UserNumbering == true)
				{
					sender.RaiseExceptionHandling<AP.Standalone.APQuickCheck.refNbr>(row, row.RefNbr, new PXException(GL.Messages.ManualNumberingDisabled));
				}

                AutoNumberAttribute.SetNumberingId<AP.Standalone.APQuickCheck.refNbr>(sender, row.DocType, numbering.NumberingID);
			}
		}
		protected virtual void APQuickCheck_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted bs)
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
					Guid? noteID = PXNoteAttribute.GetNoteID<AP.Standalone.APQuickCheck.noteID>(sender, e.Row);
					this.Voucher.Insert(new GLVoucher());
					this.Voucher.Cache.IsDirty = false;
					Base.Caches[typeof(Note)].IsDirty = false;
				}

				var row = (AP.Standalone.APQuickCheck) e.Row;

				if (wb.DefaultDescription != null && row.DocDesc == null)
				{
					sender.SetValueExt<AP.Standalone.APQuickCheck.docDesc>(e.Row, wb.DefaultDescription);
				}

				if (wb.DefaultBAccountID != null && row.VendorID == null)
				{
					sender.SetValueExt<AP.Standalone.APQuickCheck.vendorID>(e.Row, wb.DefaultBAccountID);

					if (wb.DefaultLocationID != null)
					{
						sender.SetValueExt<AP.Standalone.APQuickCheck.vendorLocationID>(e.Row, wb.DefaultLocationID);
					}
				}
			}
		}
		protected virtual void APQuickCheck_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting bs)
		{
			if (bs != null)
				bs(sender, e);
			SetNumbering(sender, e.Row as AP.Standalone.APQuickCheck);
		}
		protected virtual void APQuickCheck_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected bs)
		{
			if (bs != null)
				bs(sender, e);
			AP.Standalone.APQuickCheck row = e.Row as AP.Standalone.APQuickCheck;
			if (row == null) return;
			bool isWithinContext = Base.IsWithinContext;
			GLVoucher voucher = this.Voucher.Select();
			GLVoucherBatch voucherBatch = this.VoucherBatch.Select();
			PXCache voucherCache = this.Voucher.Cache;
			bool isDetached = (voucher == null);
			PXUIFieldAttribute.SetEnabled<GLVoucher.refNbr>(voucherCache, voucher, false);
			PXUIFieldAttribute.SetVisible<GLVoucher.refNbr>(voucherCache, voucher, !isDetached);
			PXUIFieldAttribute.SetEnabled<AP.Standalone.APQuickCheck.refNbr>(sender, e.Row, !isWithinContext);
			this.SaveAndAdd.SetVisible(!isDetached && isWithinContext);
			this.SaveAndAdd.SetEnabled(!isDetached && isWithinContext);
			if (isWithinContext)
			{
				Base.release.SetVisible(false);
				Base.release.SetEnabled(false);
				Base.voidCheck.SetVisible(false);
				Base.voidCheck.SetEnabled(false);
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
