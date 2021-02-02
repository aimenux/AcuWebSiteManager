using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO.DAC.Projections;
using PX.Objects.PO.DAC.Unbound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "ADD PO RECEIPT LINE" (Screen AP301000)
    /// </summary>
    [Serializable]
	public class AddPOReceiptLineExtension : PXGraphExtension<LinkLineExtension, APInvoiceEntry>
	{
        #region Data Members

        [PXCopyPasteHiddenView]
        public PXSelect<POReceiptLineAdd> poReceiptLinesSelection;
        [PXCopyPasteHiddenView]
        public PXSelectJoin<POReceiptLineAdd,
			InnerJoin<POReceipt,
				On<POReceipt.receiptType, Equal<POReceiptLineAdd.receiptType>,
				And<POReceipt.receiptNbr, Equal<POReceiptLineAdd.receiptNbr>>>>,
            Where<POReceiptLineAdd.receiptNbr, Equal<Required<LinkLineReceipt.receiptNbr>>,
                And<POReceiptLineAdd.lineNbr, Equal<Required<LinkLineReceipt.receiptLineNbr>>>>>
            ReceipLineAdd;

        #endregion

        #region Initialize

        public static bool IsActive()
        {
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		public override void Initialize()
		{
			base.Initialize();

			poReceiptLinesSelection.Cache.AllowDelete = false;
			poReceiptLinesSelection.Cache.AllowInsert = false;

			PXUIFieldAttribute.SetEnabled<POReceiptLine.subItemID>(Base.Caches[typeof(POReceiptLine)], null, false);
			Base.Views.Caches.Remove(typeof(POOrderRS));//This prevents POOrderRS from persisting and throwing error "'IsOpenTaxValid' may not be empty"
		}

        #endregion

        #region Actions

        public PXAction<APInvoice> addReceiptLine;
        public PXAction<APInvoice> addReceiptLine2;

        [PXUIField(DisplayName = Messages.AddPOReceiptLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "DISTR")]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddReceiptLine(PXAdapter adapter)
		{
			Base.checkTaxCalcMode();
			if (Base.Document.Current != null &&
				Base.Document.Current.Released == false &&
				Base.Document.Current.Prebooked == false &&
				poReceiptLinesSelection.AskExt(
					(graph, view) =>
					{
                        Base1.filter.Cache.ClearQueryCacheObsolete();
                        Base1.filter.View.Clear();
                        Base1.filter.Cache.Clear();

						poReceiptLinesSelection.Cache.ClearQueryCacheObsolete();
						poReceiptLinesSelection.View.Clear();
						poReceiptLinesSelection.Cache.Clear();
					}
					, true) == WebDialogResult.OK)
			{
				Base.updateTaxCalcMode();
				return AddReceiptLine2(adapter);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.AddPOReceiptLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddReceiptLine2(PXAdapter adapter)
		{
			if (Base.Document.Current != null &&
				Base.Document.Current.Released == false &&
				Base.Document.Current.Prebooked == false)
			{
				POAccrualSet duplicates = Base.GetUsedPOAccrualSet();

				HashSet<string> ordersWithDiscounts = new HashSet<string>();
				foreach (POReceiptLineAdd it in poReceiptLinesSelection.Select())
				{
					if (it.Selected == true)
					{
						if ((it.RetainagePct ?? 0m) != 0m)
						{
							Base.EnableRetainage();
						}

						Base.AddPOReceiptLine(it, duplicates);

						if (Base.Document.Current != null && it.DocumentDiscountRate != null && (it.GroupDiscountRate != 1 || it.DocumentDiscountRate != 1))
						{
							Base.Document.Current.SetWarningOnDiscount = true;
							ordersWithDiscounts.Add(it.PONbr);
						}
					}
				}

				Base.AutoRecalculateDiscounts();

				Base.WritePODiscountWarningToTrace(Base.Document.Current, ordersWithDiscounts);
			}

			poReceiptLinesSelection.View.Clear();
			poReceiptLinesSelection.Cache.Clear();
			return adapter.Get();
		}

        #endregion

        #region Events

        #region APInvoice Events

        protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            APInvoice document = e.Row as APInvoice;
            if (document == null) return;

            var invoiceState = Base.GetDocumentState(cache, document);

            addReceiptLine.SetVisible(invoiceState.IsDocumentInvoice);

            PXUIFieldAttribute.SetEnabled(poReceiptLinesSelection.Cache, null, false);

            bool allowAddReceiptLine = invoiceState.IsDocumentEditable &&
                invoiceState.AllowAddPOByProject &&
                Base.vendor.Current != null &&
                !invoiceState.IsRetainageDebAdj;

            addReceiptLine.SetEnabled(allowAddReceiptLine);
            PXUIFieldAttribute.SetEnabled<POReceiptLineS.selected>(poReceiptLinesSelection.Cache, null, allowAddReceiptLine);
        }

        #endregion

        #region Selecting override

        public virtual IEnumerable POReceiptLinesSelection()
		{
			APInvoice doc = Base.Document.Current;
			if (doc?.VendorID == null || doc.VendorLocationID == null || doc.DocType != APDocType.Invoice && doc.DocType != APDocType.DebitAdj)
				yield break;

			string poReceiptType = doc.DocType == APDocType.Invoice ? POReceiptType.POReceipt : POReceiptType.POReturn;

			var comparer = new POAccrualComparer();
			HashSet<APTran> usedSourceLine = new HashSet<APTran>(comparer);
			HashSet<APTran> unusedSourceLine = new HashSet<APTran>(comparer);

			foreach (APTran aPTran in Base.Transactions.Cache.Inserted)
			{
				usedSourceLine.Add(aPTran);
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Deleted)
			{
				if (Base.Transactions.Cache.GetStatus(aPTran) != PXEntryStatus.InsertedDeleted)
					if (!usedSourceLine.Remove(aPTran))
					{
						unusedSourceLine.Add(aPTran);
					}
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Updated)
			{
				APTran originAPTran = new APTran
				{
					POAccrualType = (string)Base.Transactions.Cache.GetValueOriginal<APTran.pOAccrualType>(aPTran),
					POAccrualRefNoteID = (Guid?)Base.Transactions.Cache.GetValueOriginal<APTran.pOAccrualRefNoteID>(aPTran),
					POAccrualLineNbr = (int?)Base.Transactions.Cache.GetValueOriginal<APTran.pOAccrualLineNbr>(aPTran),
					POOrderType = (string)Base.Transactions.Cache.GetValueOriginal<APTran.pOOrderType>(aPTran),
					PONbr = (string)Base.Transactions.Cache.GetValueOriginal<APTran.pONbr>(aPTran),
					POLineNbr = (int?)Base.Transactions.Cache.GetValueOriginal<APTran.pOLineNbr>(aPTran),
					ReceiptNbr = (string)Base.Transactions.Cache.GetValueOriginal<APTran.receiptNbr>(aPTran),
					ReceiptType = (string)Base.Transactions.Cache.GetValueOriginal<APTran.receiptType>(aPTran),
					ReceiptLineNbr = (int?)Base.Transactions.Cache.GetValueOriginal<APTran.receiptLineNbr>(aPTran)
				};

				if (!usedSourceLine.Remove(originAPTran))
				{
					unusedSourceLine.Add(originAPTran);
				}

				if (!unusedSourceLine.Remove(aPTran))
				{
					usedSourceLine.Add(aPTran);
				}
			}

			PXSelectBase<LinkLineReceipt> cmd = new PXSelect<LinkLineReceipt,
				Where<LinkLineReceipt.receiptType, Equal<Required<POReceipt.receiptType>>,
					And<Where<LinkLineReceipt.orderNbr, Equal<Current<POReceiptFilter.orderNbr>>,
						Or<Current<POReceiptFilter.orderNbr>, IsNull>>>>>(Base);

			if (Base.APSetup.Current.RequireSingleProjectPerDocument == true)
			{
				cmd.WhereAnd<Where<LinkLineReceipt.projectID, Equal<Current<APInvoice.projectID>>>>();
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
			{
				cmd.WhereAnd<Where<LinkLineReceipt.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
					And<LinkLineReceipt.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
					And<Where<LinkLineReceipt.payToVendorID, IsNull, Or<LinkLineReceipt.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<LinkLineReceipt.vendorID, Equal<Current<APInvoice.vendorID>>,
					And<LinkLineReceipt.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>();
			}

			foreach (LinkLineReceipt item in cmd.View.SelectMultiBound(new object[] { doc }, poReceiptType))
			{
				APTran aPTran = new APTran
				{
					POAccrualType = item.POAccrualType,
					POAccrualRefNoteID = item.POAccrualRefNoteID,
					POAccrualLineNbr = item.POAccrualLineNbr,
					POOrderType = item.OrderType,
					PONbr = item.OrderNbr,
					POLineNbr = item.OrderLineNbr,
					ReceiptType = item.ReceiptType,
					ReceiptNbr = item.ReceiptNbr,
					ReceiptLineNbr = item.ReceiptLineNbr
				};

				if (!usedSourceLine.Contains(aPTran))
				{
					var res = (PXResult<POReceiptLineAdd, POReceipt>)ReceipLineAdd.Select(item.ReceiptNbr, item.ReceiptLineNbr);
					if (res != null) yield return res;
				}
			}

			foreach (APTran item in unusedSourceLine.Where(t => t.POAccrualType != null))
			{
				foreach (PXResult<POReceiptLineAdd, POReceipt> res in PXSelectJoin<POReceiptLineAdd,
					InnerJoin<POReceipt,
						On<POReceipt.receiptType, Equal<POReceiptLineAdd.receiptType>,
						And<POReceipt.receiptNbr, Equal<POReceiptLineAdd.receiptNbr>>>>,
					Where<POReceiptLineAdd.pOAccrualType, Equal<Required<LinkLineReceipt.pOAccrualType>>,
						And<POReceiptLineAdd.pOAccrualRefNoteID, Equal<Required<LinkLineReceipt.pOAccrualRefNoteID>>,
						And<POReceiptLineAdd.pOAccrualLineNbr, Equal<Required<LinkLineReceipt.pOAccrualLineNbr>>,
						And<POReceiptLineAdd.unbilledQty, Greater<decimal0>>>>>>
					.Select(Base, item.POAccrualType, item.POAccrualRefNoteID, item.POAccrualLineNbr))
				{
					yield return res;
				}
			}
		}

		#endregion

		#region POReceiptLineAdd Events

		public virtual void POReceiptLineAdd_ReceiptNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			POReceiptLineAdd row = (POReceiptLineAdd)e.Row;
			APInvoice doc = Base.Document.Current;
			if (row != null && doc != null)
			{
				POOrder order = PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>, And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(Base, row.POType, row.PONbr);
				if (order != null && order.CuryID != doc.CuryID)
				{
					string fieldName = typeof(POReceipt.curyID).Name;
					PXErrorLevel msgLevel = PXErrorLevel.RowWarning;
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
					 null, null, AP.Messages.APDocumentCurrencyDiffersFromSourceDocument, msgLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
					e.IsAltered = true;
				}
			}
		}

		#endregion

		#region POReceiptLineS Events

		public virtual void POReceiptLineS_ReceiptNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			POReceiptLineS row = (POReceiptLineS)e.Row;
			APInvoice doc = Base.Document.Current;
			if (row != null && doc != null)
			{
				POOrder order = PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>, And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(Base, row.POType, row.PONbr);
				if (order != null && order.CuryID != doc.CuryID)
				{
					string fieldName = typeof(POReceipt.curyID).Name;
					PXErrorLevel msgLevel = PXErrorLevel.RowWarning;
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
					 null, null, AP.Messages.APDocumentCurrencyDiffersFromSourceDocument, msgLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
					e.IsAltered = true;
				}
			}
		}

		public virtual void POReceiptLineS_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLineS row = (POReceiptLineS)e.Row;
			if (row != null && !(bool)e.OldValue && (bool)row.Selected)
			{
                foreach (POReceiptLineS item in Base1.linkLineReceiptTran.Cache.Updated)
				{
					if (item.Selected == true && item != row)
					{
						sender.SetValue<POReceiptLineS.selected>(item, false);
                        Base1.linkLineReceiptTran.View.RequestRefresh();
					}
                }

                foreach (POLineS item in Base1.linkLineOrderTran.Cache.Updated)
				{
					if (item.Selected == true)
					{
                        Base1.linkLineOrderTran.Cache.SetValue<POLineS.selected>(item, false);
                        Base1.linkLineOrderTran.View.RequestRefresh();
					}
				}

			}
		}

		public virtual void POReceiptLineS_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

		public virtual void POReceiptLineS_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}


		#endregion

		#endregion
	}
}
