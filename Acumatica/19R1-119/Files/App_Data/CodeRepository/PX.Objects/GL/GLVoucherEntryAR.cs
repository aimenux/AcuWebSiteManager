using PX.Data;
using PX.Objects.GL;
using System;
using System.Collections;
using ContextFieldDescriptor = PX.Data.PXGraph.ContextFieldDescriptor;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARPaymentEntryExt : PXGraphExtension<ARPaymentEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.SetContextFields(new ContextFieldDescriptor[] { 
															new ContextFieldDescriptor<GLVoucher.refNbr>(true),			
															new ContextFieldDescriptor<ARPayment.docType>(true, false),
															new ContextFieldDescriptor<ARPayment.docDate>(false),
															new ContextFieldDescriptor<ARPayment.customerID>(false, true),
															new ContextFieldDescriptor<ARPayment.curyOrigDocAmt>(false, true),
															new ContextFieldDescriptor<ARPayment.status>(false, true),
															new ContextFieldDescriptor<ARPayment.curyID>(false, true),
															new ContextFieldDescriptor<ARPayment.customerID_Customer_acctName>(false, true)
															});
			Base.SetContextHeaderFields(typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));
			Base.Document.Join<LeftJoin<GLVoucher, On<GLVoucher.docType, Equal<ARPayment.docType>,
					And<GLVoucher.refNbr, Equal<ARPayment.refNbr>, And<GLVoucher.module, Equal<GL.BatchModule.moduleAR>>>>>>();
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
		}

		#region Buttons
		public PXAction<ARPayment> SaveAndAdd;
		[PXUIField(DisplayName = GL.Messages.SaveAndAdd, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ShortcutCtrl = true, ShortcutChar = (char)65, Tooltip = GL.Messages.SaveAndAddToolTip)] //Ctrl-A
		public virtual IEnumerable saveAndAdd(PXAdapter adapter)
		{
			Base.Save.Press();
			return Base.Insert.Press(adapter);
		}
		public PXAction<ARPayment> viewVoucherBatch;
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
		public PXAction<ARPayment> viewWorkBook;
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
				new Select2<ARPayment,
						LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARPayment.customerID>>,
						InnerJoin<GLVoucher, On<GLVoucher.refNoteID, Equal<ARPayment.noteID>>>>,
						Where<ARPayment.docType, Equal<Optional<ARPayment.docType>>,
							And<GLVoucher.workBookID, Equal<Context<GLVoucherBatch.workBookID>>,
							And<GLVoucher.voucherBatchNbr, Equal<Context<GLVoucherBatch.voucherBatchNbr>>,
							And2<Where<ARPayment.origModule, NotEqual<BatchModule.moduleTX>, Or<ARPayment.released, Equal<True>>>,
							And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>());
		}

		public PXSelect<GLVoucher,
					Where<GLVoucher.docType, Equal<Current<ARPayment.docType>>,
						And<GLVoucher.refNbr, Equal<Current<ARPayment.refNbr>>,
						And<GLVoucher.module, Equal<GL.BatchModule.moduleAR>>>>> Voucher;

		public PXSelect<GLVoucherBatch,
					Where<GLVoucherBatch.workBookID, Equal<Optional<GLVoucher.workBookID>>,
						And<GLVoucherBatch.voucherBatchNbr, Equal<Optional<GLVoucher.voucherBatchNbr>>>>> VoucherBatch;

        #endregion

        #region CacheAttached - Voucher Batch
        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNbr"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(ARPayment.refNbr))]
		[PXParent(typeof(Select<ARPayment, Where<ARPayment.docType, Equal<Current<GLVoucher.docType>>,
							And<ARPayment.refNbr, Equal<Current<GLVoucher.refNbr>>>>>))]
		protected virtual void GLVoucher_RefNbr_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsUnicode = true)]
		[PXDBDefault(typeof(ARPayment.docType))]
		protected virtual void GLVoucher_DocType_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNoteID"/> field.
        /// </summary>
		[PXDBGuid]
		[PXDefault(typeof(ARPayment.noteID), PersistingCheck = PXPersistingCheck.Null)]
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
			e.NewValue = GL.BatchModule.AR;
		}

		protected virtual void GLVoucher_VoucherBatchNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
		}
		#endregion

		#region Events - ARPayment
		protected virtual void ARPayment_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			SetNumbering(sender, e.Row as ARPayment);
			if (bs != null)
				bs(sender, e);
		}

		private void SetNumbering(PXCache sender, ARPayment row)
		{
			if (row == null) return;
			if (Base.IsWithinContext)
			{
				Numbering numbering = PXSelectJoin<Numbering,
					InnerJoin<GLWorkBook, On<GLWorkBook.voucherNumberingID, Equal<Numbering.numberingID>>>,
					Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				
				if (numbering.UserNumbering == true)
				{
					sender.RaiseExceptionHandling<ARPayment.refNbr>(row, row.RefNbr, new PXException(GL.Messages.ManualNumberingDisabled));
				}

                AutoNumberAttribute.SetNumberingId<ARPayment.refNbr>(sender, row.DocType, numbering.NumberingID);
            }
		}

		protected virtual void ARPayment_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted bs)
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
				sender.SetValueExt<ARPayment.docType>(e.Row, wb.DocType);
				if (!String.IsNullOrEmpty(vb))
				{
					Guid? noteID = PXNoteAttribute.GetNoteID<ARPayment.noteID>(sender, e.Row);
					this.Voucher.Insert(new GLVoucher());
					this.Voucher.Cache.IsDirty = false;
					Base.Caches[typeof(Note)].IsDirty = false;
				}

				var row = (ARPayment)e.Row;

				if (wb.DefaultDescription != null && row.DocDesc == null)
				{
					sender.SetValueExt<ARPayment.docDesc>(e.Row, wb.DefaultDescription);
				}
				if (wb.DefaultBAccountID != null && row.CustomerID == null)
				{
					sender.SetValueExt<ARPayment.customerID>(e.Row, wb.DefaultBAccountID);

					if (wb.DefaultLocationID != null)
					{
						sender.SetValueExt<ARPayment.customerLocationID>(e.Row, wb.DefaultLocationID);
					}
				}
			}
		}
		protected virtual void ARPayment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting bs)
		{
			if (bs != null)
				bs(sender, e);
			SetNumbering(sender, e.Row as ARPayment);
		}
		protected virtual void ARPayment_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected bs)
		{
			if (bs != null)
				bs(sender, e);
			ARPayment row = e.Row as ARPayment;
			if (row == null) return;
			bool isWithinContext = Base.IsWithinContext;
			GLVoucher voucher = this.Voucher.Select();
			GLVoucherBatch voucherBatch = this.VoucherBatch.Select();
			PXCache voucherCache = this.Voucher.Cache;
			bool isDetached = (voucher == null);
			PXUIFieldAttribute.SetEnabled<GLVoucher.refNbr>(voucherCache, voucher, false);
			PXUIFieldAttribute.SetVisible<GLVoucher.refNbr>(voucherCache, voucher, !isDetached);
			PXUIFieldAttribute.SetEnabled<ARPayment.refNbr>(sender, e.Row, !isWithinContext);
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
	public class ARInvoiceEntryExt : PXGraphExtension<ARInvoiceEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.SetContextFields(new ContextFieldDescriptor[] { 
															new ContextFieldDescriptor<GLVoucher.refNbr>(false),															
															new ContextFieldDescriptor<ARInvoice.refNbr>(true, true), 
															new ContextFieldDescriptor<ARInvoice.docType>(true, false),
															new ContextFieldDescriptor<ARInvoice.docDate>(false),															
															new ContextFieldDescriptor<ARInvoice.invoiceNbr>(false, true),
															new ContextFieldDescriptor<ARInvoice.customerID>(false, true),
															new ContextFieldDescriptor<ARInvoice.curyOrigDocAmt>(false, true),
															new ContextFieldDescriptor<ARInvoice.status>(false, true),
															new ContextFieldDescriptor<ARInvoice.curyID>(false, true),
															new ContextFieldDescriptor<ARInvoice.customerID_Customer_acctName>(false, true)
															});
			Base.SetContextHeaderFields(typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));
			Base.Document.Join<LeftJoin<GLVoucher, On<GLVoucher.docType, Equal<ARInvoice.docType>,
					And<GLVoucher.refNbr, Equal<ARInvoice.refNbr>, And<GLVoucher.module, Equal<GL.BatchModule.moduleAR>>>>>>();
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
		}

		#region Buttons
		public PXAction<ARInvoice> SaveAndAdd;
		[PXUIField(DisplayName = GL.Messages.SaveAndAdd, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ShortcutCtrl = true, ShortcutChar = (char)65, Tooltip = GL.Messages.SaveAndAddToolTip)] //Ctrl-A
		public virtual IEnumerable saveAndAdd(PXAdapter adapter)
		{
			Base.Save.Press();
			return Base.Insert.Press(adapter);
		}
		public PXAction<ARInvoice> viewVoucherBatch;
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
		public PXAction<ARInvoice> viewWorkBook;
		[PXUIField(DisplayName = GL.Messages.ViewWorkBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewWorkBook(PXAdapter adapter)
		{
			GLVoucherBatchEntry graph = PXGraph.CreateInstance<GLVoucherBatchEntry>();
			graph.filter.Current.WorkBookID = Voucher.Current.WorkBookID;
			graph.VoucherBatches.Current = graph.VoucherBatches.Search<GLVoucherBatch.voucherBatchNbr>(Voucher.Current.VoucherBatchNbr);
			throw new PXRedirectRequiredException(graph, "");
		}

		public PXAction<ARInvoice> reverseInvoice;
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
				new Select2<ARInvoice,
						LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
						InnerJoin<GLVoucher, On<GLVoucher.refNoteID, Equal<ARInvoice.noteID>>>>,
						Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>,
							And<GLVoucher.workBookID, Equal<Context<GLVoucherBatch.workBookID>>,
							And<GLVoucher.voucherBatchNbr, Equal<Context<GLVoucherBatch.voucherBatchNbr>>,
							And2<Where<ARInvoice.origModule, NotEqual<BatchModule.moduleTX>, Or<ARInvoice.released, Equal<True>>>,
							And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>());
		}

		public PXSelect<GLVoucher,
					Where<GLVoucher.docType, Equal<Current<ARInvoice.docType>>,
						And<GLVoucher.refNbr, Equal<Current<ARInvoice.refNbr>>,
						And<GLVoucher.module, Equal<GL.BatchModule.moduleAR>>>>> Voucher;

		public PXSelect<GLVoucherBatch,
					Where<GLVoucherBatch.workBookID, Equal<Optional<GLVoucher.workBookID>>,
						And<GLVoucherBatch.voucherBatchNbr, Equal<Optional<GLVoucher.voucherBatchNbr>>>>> VoucherBatch;

        #endregion

        #region CacheAttached - Voucher Batch
        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNbr"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(ARInvoice.refNbr))]
		[PXParent(typeof(Select<ARInvoice, Where<ARInvoice.docType, Equal<Current<GLVoucher.docType>>,
							And<ARInvoice.refNbr, Equal<Current<GLVoucher.refNbr>>>>>))]
		protected virtual void GLVoucher_RefNbr_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsUnicode = true)]
		[PXDBDefault(typeof(ARInvoice.docType))]
		protected virtual void GLVoucher_DocType_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNoteID"/> field.
        /// </summary>
		[PXDBGuid]
		[PXDefault(typeof(ARInvoice.noteID), PersistingCheck = PXPersistingCheck.Null)]
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
			e.NewValue = GL.BatchModule.AR;
		}

		protected virtual void GLVoucher_VoucherBatchNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
		}
		#endregion

		#region Events - ARInvoice
		protected virtual void ARInvoice_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			SetNumbering(sender, e.Row as ARInvoice);
			if (bs != null)
				bs(sender, e);
		}
		private void SetNumbering(PXCache sender, ARInvoice row)
		{
			if (row == null) return;
			if (Base.IsWithinContext)
			{
				Numbering numbering = PXSelectJoin<Numbering,
					InnerJoin<GLWorkBook, On<GLWorkBook.voucherNumberingID, Equal<Numbering.numberingID>>>,
					Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
			
				if (numbering.UserNumbering == true)
				{
					sender.RaiseExceptionHandling<ARInvoice.refNbr>(row, row.RefNbr, new PXException(GL.Messages.ManualNumberingDisabled));
				}

                AutoNumberAttribute.SetNumberingId<ARInvoice.refNbr>(sender, row.DocType, numbering.NumberingID);
            }
		}
		protected virtual void ARInvoice_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			if (Base.IsWithinContext)
			{
				GLWorkBook wb = PXSelect<GLWorkBook,
					Where<GLWorkBook.workBookID, Equal<Required<GLVoucherBatch.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				if (wb?.DocType != null)
				{
					e.NewValue = wb?.DocType;
					e.Cancel = true;
				}
			}
		}
		protected virtual void ARInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted bs)
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
					Where<GLWorkBook.workBookID, Equal<Required<GLVoucherBatch.workBookID>>>>.Select(this.Base, wbID);
				if (!String.IsNullOrEmpty(vb))
				{
					Guid? noteID = PXNoteAttribute.GetNoteID<ARInvoice.noteID>(sender, e.Row);
					this.Voucher.Insert(new GLVoucher());
					this.Voucher.Cache.IsDirty = false;
					Base.Caches[typeof(Note)].IsDirty = false;
				}

				var row = (ARInvoice) e.Row;

				if (wb.DefaultDescription != null && row.DocDesc == null)
				{
					sender.SetValueExt<ARInvoice.docDesc>(row, wb.DefaultDescription);
				}
				if (wb.DefaultBAccountID != null && row.CustomerID == null)
				{
					sender.SetValueExt<ARInvoice.customerID>(row, wb.DefaultBAccountID);

					if (wb.DefaultLocationID != null)
					{
						sender.SetValueExt<ARInvoice.customerLocationID>(row, wb.DefaultLocationID);
					}
					sender.Update(row);
				}
			}
		}
        protected virtual void ARInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting bs)
        {
            if (bs != null)
                bs(sender, e);
            SetNumbering(sender, e.Row as ARInvoice);
        }
        protected virtual void ARInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected bs)
		{
			if (bs != null)
				bs(sender, e);
			ARInvoice row = e.Row as ARInvoice;
			if (row == null) return;
			bool isWithinContext = Base.IsWithinContext;
			GLVoucher voucher = this.Voucher.Select();
			GLVoucherBatch voucherBatch = this.VoucherBatch.Select();
			PXCache voucherCache = this.Voucher.Cache;
			bool isDetached = (voucher == null);
			PXUIFieldAttribute.SetEnabled<GLVoucher.refNbr>(voucherCache, voucher, false);
			PXUIFieldAttribute.SetVisible<GLVoucher.refNbr>(voucherCache, voucher, !isDetached);
			PXUIFieldAttribute.SetEnabled<ARInvoice.refNbr>(sender, e.Row, !isWithinContext);
			this.SaveAndAdd.SetVisible(!isDetached && isWithinContext);
			this.SaveAndAdd.SetEnabled(!isDetached && isWithinContext);
			if (isWithinContext)
			{
				Base.release.SetVisible(false);
				Base.release.SetEnabled(false);
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

		public virtual bool IsAttached(ARInvoice aRow, out GLVoucherBatch aVoucherBatch, out GLVoucher aVoucher)
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
	public class ARCashSaleEntryExt : PXGraphExtension<ARCashSaleEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
			Base.SetContextFields(new ContextFieldDescriptor[] { 
															new ContextFieldDescriptor<GLVoucher.refNbr>(false),															
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.refNbr>(true, true), 
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.docType>(true, false),
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.docDate>(false),
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.customerID>(false, true),
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.curyOrigDocAmt>(false, true),
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.status>(false, true),
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.curyID>(false, true),
															new ContextFieldDescriptor<AR.Standalone.ARCashSale.customerID_Customer_acctName>(false, true)
															});
			Base.SetContextHeaderFields(typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));
			Base.Document.Join<LeftJoin<GLVoucher, On<GLVoucher.docType, Equal<AR.Standalone.ARCashSale.docType>,
					And<GLVoucher.refNbr, Equal<AR.Standalone.ARCashSale.refNbr>, And<GLVoucher.module, Equal<GL.BatchModule.moduleAR>>>>>>();
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.gLWorkBooks>();
		}

		#region Buttons
		public PXAction<AR.Standalone.ARCashSale> SaveAndAdd;
		[PXUIField(DisplayName = GL.Messages.SaveAndAdd, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(ShortcutCtrl = true, ShortcutChar = (char)65, Tooltip = GL.Messages.SaveAndAddToolTip)] //Ctrl-A
		public virtual IEnumerable saveAndAdd(PXAdapter adapter)
		{
			Base.Save.Press();
			return Base.Insert.Press(adapter);
		}
		public PXAction<AR.Standalone.ARCashSale> viewVoucherBatch;
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
		public PXAction<AR.Standalone.ARCashSale> viewWorkBook;
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
				new Select2<AR.Standalone.ARCashSale,
						LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<AR.Standalone.ARCashSale.customerID>>,
						InnerJoin<GLVoucher, On<GLVoucher.refNoteID, Equal<AR.Standalone.ARCashSale.noteID>>>>,
						Where<AR.Standalone.ARCashSale.docType, Equal<Optional<AR.Standalone.ARCashSale.docType>>,
							And<GLVoucher.workBookID, Equal<Context<GLVoucherBatch.workBookID>>,
							And<GLVoucher.voucherBatchNbr, Equal<Context<GLVoucherBatch.voucherBatchNbr>>,
							And2<Where<AR.Standalone.ARCashSale.origModule, NotEqual<BatchModule.moduleTX>, Or<AR.Standalone.ARCashSale.released, Equal<True>>>,
							And<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>());
		}

		public PXSelect<GLVoucher,
					Where<GLVoucher.docType, Equal<Current<AR.Standalone.ARCashSale.docType>>,
						And<GLVoucher.refNbr, Equal<Current<AR.Standalone.ARCashSale.refNbr>>,
						And<GLVoucher.module, Equal<GL.BatchModule.moduleAR>>>>> Voucher;

		public PXSelect<GLVoucherBatch,
					Where<GLVoucherBatch.workBookID, Equal<Optional<GLVoucher.workBookID>>,
						And<GLVoucherBatch.voucherBatchNbr, Equal<Optional<GLVoucher.voucherBatchNbr>>>>> VoucherBatch;

        #endregion

        #region CacheAttached - Voucher Batch
        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNbr"/> field.
        /// </summary>
        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(AR.Standalone.ARCashSale.refNbr))]
		[PXParent(typeof(Select<AR.Standalone.ARCashSale, Where<AR.Standalone.ARCashSale.docType, Equal<Current<GLVoucher.docType>>,
							And<AR.Standalone.ARCashSale.refNbr, Equal<Current<GLVoucher.refNbr>>>>>))]
		protected virtual void GLVoucher_RefNbr_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsUnicode = true)]
		[PXDBDefault(typeof(AR.Standalone.ARCashSale.docType))]
		protected virtual void GLVoucher_DocType_CacheAttached(PXCache sender)
		{
		}

        /// <summary>
        /// The cache attached to the value of the <see cref="GLVoucher.RefNoteID"/> field.
        /// </summary>
		[PXDBGuid]
		[PXDefault(typeof(AR.Standalone.ARCashSale.noteID), PersistingCheck = PXPersistingCheck.Null)]
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
			e.NewValue = GL.BatchModule.AR;
		}

		protected virtual void GLVoucher_VoucherBatchNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Base.GetContextValue<GLVoucherBatch.voucherBatchNbr>();
		}
		#endregion

		#region Events - ARCashSale
		protected virtual void ARCashSale_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting bs)
		{
			SetNumbering(sender, e.Row as AR.Standalone.ARCashSale);
			if (bs != null)
				bs(sender, e);
		}
		private void SetNumbering(PXCache sender, AR.Standalone.ARCashSale row)
		{
			if (row == null) return;
			if (Base.IsWithinContext)
			{
				Numbering numbering = PXSelectJoin<Numbering,
					InnerJoin<GLWorkBook, On<GLWorkBook.voucherNumberingID, Equal<Numbering.numberingID>>>,
					Where<GLWorkBook.workBookID, Equal<Required<GLWorkBook.workBookID>>>>.Select(this.Base, Base.GetContextValue<GLVoucherBatch.workBookID>());
				
				if (numbering.UserNumbering == true)
				{
					sender.RaiseExceptionHandling<AR.Standalone.ARCashSale.refNbr>(row, row.RefNbr, new PXException(GL.Messages.ManualNumberingDisabled));
				}

                AutoNumberAttribute.SetNumberingId<AR.Standalone.ARCashSale.refNbr>(sender, row.DocType, numbering.NumberingID);
            }
		}
		protected virtual void ARCashSale_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted bs)
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
					Guid? noteID = PXNoteAttribute.GetNoteID<AR.Standalone.ARCashSale.noteID>(sender, e.Row);
					this.Voucher.Insert(new GLVoucher());
					this.Voucher.Cache.IsDirty = false;
					Base.Caches[typeof(Note)].IsDirty = false;
				}

				var row = (AR.Standalone.ARCashSale)e.Row;

				if (wb.DefaultDescription != null && row.DocDesc == null)
				{
					sender.SetValueExt<AR.Standalone.ARCashSale.docDesc>(e.Row, wb.DefaultDescription);
				}
				if (wb.DefaultBAccountID != null && row.CustomerID == null)
				{
					sender.SetValueExt<AR.Standalone.ARCashSale.customerID>(e.Row, wb.DefaultBAccountID);

					if (wb.DefaultLocationID != null)
					{
						sender.SetValueExt<AR.Standalone.ARCashSale.customerLocationID>(e.Row, wb.DefaultLocationID);
					}
				}
			}
		}
		protected virtual void ARCashSale_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting bs)
		{
			if (bs != null)
				bs(sender, e);
            SetNumbering(sender, e.Row as AR.Standalone.ARCashSale);
		}
		protected virtual void ARCashSale_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected bs)
		{
			if (bs != null)
				bs(sender, e);
			AR.Standalone.ARCashSale row = e.Row as AR.Standalone.ARCashSale;
			if (row == null) return;
			bool isWithinContext = Base.IsWithinContext;
			GLVoucher voucher = this.Voucher.Select();
			GLVoucherBatch voucherBatch = this.VoucherBatch.Select();
			PXCache voucherCache = this.Voucher.Cache;
			bool isDetached = (voucher == null);
			PXUIFieldAttribute.SetEnabled<GLVoucher.refNbr>(voucherCache, voucher, false);
			PXUIFieldAttribute.SetVisible<GLVoucher.refNbr>(voucherCache, voucher, !isDetached);
			PXUIFieldAttribute.SetEnabled<AR.Standalone.ARCashSale.refNbr>(sender, e.Row, !isWithinContext);
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

