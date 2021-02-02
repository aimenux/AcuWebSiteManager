using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "ADD PO" (Screen AP301000)
    /// </summary>
    [Serializable]
	public class AddPOOrderExtension : PXGraphExtension<LinkLineExtension, APInvoiceEntry>
	{
        #region Data Members

        [PXCopyPasteHiddenView]
        [PXViewName(Messages.POOrder)]
        public PXSelect<POOrderRS> poorderslist;

        #endregion

        #region Initialize

        public static bool IsActive()
        {
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		public override void Initialize()
		{
			base.Initialize();

			poorderslist.Cache.AllowDelete = false;
            poorderslist.Cache.AllowInsert = false;
		}

        #endregion

        #region Actions

        public PXAction<APInvoice> addPOOrder;
        public PXAction<APInvoice> addPOOrder2;

        [PXUIField(DisplayName = Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "DISTR")]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddPOOrder(PXAdapter adapter)
		{
			Base.checkTaxCalcMode();
			if (Base.Document.Current != null &&
				Base.Document.Current.DocType == APDocType.Invoice &&
				Base.Document.Current.Released == false &&
				Base.Document.Current.Prebooked == false &&
                poorderslist.AskExt(
					(graph, view) =>
					{
                        Base1.filter.Cache.ClearQueryCache();
                        Base1.filter.View.Clear();
                        Base1.filter.Cache.Clear();

                        poorderslist.Cache.ClearQueryCache();
                        poorderslist.View.Clear();
                        poorderslist.Cache.Clear();
					}, true) == WebDialogResult.OK)
			{
				Base.updateTaxCalcMode();
				return AddPOOrder2(adapter);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.AddPOOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddPOOrder2(PXAdapter adapter)
		{
			if (Base.Document.Current != null &&
				Base.Document.Current.DocType == APDocType.Invoice &&
				Base.Document.Current.Released == false &&
				Base.Document.Current.Prebooked == false)
			{
				foreach (POOrder rc in poorderslist.Cache.Updated)
				{
					if (rc.Selected == true)
					{
						Base.InvoicePOOrder(rc, false);
					}
				}
			}
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

            addPOOrder.SetVisible(invoiceState.IsDocumentInvoice);

            PXUIFieldAttribute.SetEnabled(poorderslist.Cache, null, false);

            bool allowAddPOOrder = invoiceState.IsDocumentEditable &&
                invoiceState.AllowAddPOByProject &&
                Base.vendor.Current != null &&
                !invoiceState.IsRetainageDebAdj;

            addPOOrder.SetEnabled(allowAddPOOrder);
            PXUIFieldAttribute.SetEnabled<POOrderRS.selected>(poorderslist.Cache, null, allowAddPOOrder);
        }

        #endregion

        #region Selecting override

        public virtual IEnumerable pOOrderslist()
		{
			APInvoice doc = Base.Document.Current;
			if (doc?.VendorID == null
				|| doc.VendorLocationID == null
				|| doc.DocType != APDocType.Invoice
					&& doc.DocType != APDocType.DebitAdj)
			{
				yield break;
			}

			Dictionary<APTran, int> usedOrder = new Dictionary<APTran, int>(new POOrderComparer());

			int count;
			foreach (APTran aPTran in Base.Transactions.Cache.Inserted)
			{
				if (aPTran.PONbr != null && aPTran.POOrderType != null && string.IsNullOrEmpty(aPTran.ReceiptNbr))
				{
					usedOrder.TryGetValue(aPTran, out count);
					usedOrder[aPTran] = count + 1;
				}
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Deleted)
			{
				if (aPTran.PONbr != null && aPTran.POOrderType != null && string.IsNullOrEmpty(aPTran.ReceiptNbr) && Base.Transactions.Cache.GetStatus(aPTran) != PXEntryStatus.InsertedDeleted)
				{
					usedOrder.TryGetValue(aPTran, out count);
					usedOrder[aPTran] = count - 1;
				}
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Updated)
			{
				string originalNbr = (string)Base.Transactions.Cache.GetValueOriginal<APTran.pONbr>(aPTran);
				string originalType = (string)Base.Transactions.Cache.GetValueOriginal<APTran.pOOrderType>(aPTran);
				if (aPTran.PONbr != originalNbr || aPTran.POOrderType != originalType)
				{
					if (originalNbr != null && originalType != null)
					{
						APTran originTran = new APTran
						{
							PONbr = originalNbr,
							POOrderType = originalType
						};
						usedOrder.TryGetValue(originTran, out count);
						usedOrder[originTran] = count - 1;
					}
					if (aPTran.PONbr != null)
					{
						usedOrder.TryGetValue(aPTran, out count);
						usedOrder[aPTran] = count + 1;
					}
				}
			}

			PXSelectBase<POOrder> cmd = new PXSelectJoinGroupBy<
				POOrder,
				InnerJoin<POLine, On<POLine.orderType, Equal<POOrder.orderType>,
					And<POLine.orderNbr, Equal<POOrder.orderNbr>,
					And<POLine.pOAccrualType, Equal<POAccrualType.order>,
					And<POLine.cancelled, NotEqual<True>,
					And<POLine.closed, NotEqual<True>>>>>>,
					LeftJoin<APTran,
						On<APTran.pOOrderType, Equal<POLine.orderType>,
												And<APTran.pONbr, Equal<POLine.orderNbr>,
												And<APTran.pOLineNbr, Equal<POLine.lineNbr>,
							And<APTran.receiptNbr, IsNull,
							And<APTran.receiptLineNbr, IsNull,
							And<APTran.released, Equal<False>>>>>>>>>,
				Where<APTran.refNbr, IsNull,
					And<POOrder.orderType, NotEqual<POOrderType.blanket>,
					And<POOrder.orderType, NotEqual<POOrderType.standardBlanket>,
					And<POOrder.curyID, Equal<Current<APInvoice.curyID>>,
					And<POOrder.status, In3<POOrderStatus.open, POOrderStatus.completed>>>>>>,
					Aggregate
						<GroupBy<POOrder.orderType,
						GroupBy<POOrder.orderNbr,
						GroupBy<POOrder.orderDate,
						GroupBy<POOrder.curyID,
						GroupBy<POOrder.curyOrderTotal,
						GroupBy<POOrder.hold,
						GroupBy<POOrder.cancelled,
						Sum<POLine.orderQty,
						Sum<POLine.curyExtCost,
						Sum<POLine.extCost,
						Count<POLine.lineNbr>>>>>>>>>>>>>(Base);

			if (Base.APSetup.Current.RequireSingleProjectPerDocument == true)
			{
				cmd.WhereAnd<Where<POOrder.projectID, Equal<Current<APInvoice.projectID>>>>();
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
			{
				cmd.WhereAnd<Where<POOrder.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
					And<POOrder.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
					And<POOrder.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<POOrder.vendorID, Equal<Current<APInvoice.vendorID>>,
					And<POOrder.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>();
			}

			foreach (PXResult<POOrder, POLine> result in cmd.View.SelectMultiBound(new object[] { doc }))
			{
				POOrder order = result;
				APTran aPTran = new APTran
				{
					PONbr = order.OrderNbr,
					POOrderType = order.OrderType
				};
				if (usedOrder.TryGetValue(aPTran, out count))
				{
					usedOrder.Remove(aPTran);
					if (count < result.RowCount)
						yield return order;
				}
				else
				{
					yield return order;
				}
			}
			foreach (APTran deletedTran in usedOrder.Where(_ => _.Value < 0).Select(_ => _.Key))
			{
				yield return PXSelect<
					POOrder,
					Where<POOrder.orderNbr, Equal<Required<APTran.pONbr>>,
					And<POOrder.orderType, Equal<Required<APTran.pOOrderType>>>>>
					.SelectSingleBound(Base, new object[] { }, deletedTran.PONbr, deletedTran.POOrderType)
					.RowCast<POOrder>()
					.First();
			}

		}

		#endregion

		#region POOrderRS

		public virtual void POOrderRS_CuryID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			POOrderRS row = (POOrderRS)e.Row;
			APInvoice doc = Base.Document.Current;
			if (row != null && doc != null)
			{
				if (row.CuryID != doc.CuryID)
				{
					string fieldName = typeof(POOrderRS.curyID).Name;
					PXErrorLevel msgLevel = PXErrorLevel.RowWarning;
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
					 null, null, AP.Messages.APDocumentCurrencyDiffersFromSourceDocument, msgLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
					e.IsAltered = true;
				}
			}
		}

		#endregion

		#region POLineS Events

		public virtual void POLineS_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POLineS row = (POLineS)e.Row;
			if (row != null && !(bool)e.OldValue && (bool)row.Selected)
			{
				foreach (POLineS item in sender.Updated)
				{
					if (item.Selected == true && item != row)
					{
						sender.SetValue<POLineS.selected>(item, false);

                        Base1.linkLineOrderTran.View.RequestRefresh();
					}

				}

				foreach (POReceiptLineS item in Base1.linkLineReceiptTran.Cache.Updated)
				{
					if (item.Selected == true)
					{
						Base1.linkLineReceiptTran.Cache.SetValue<POReceiptLineS.selected>(item, false);
                        Base1.linkLineReceiptTran.View.RequestRefresh();
					}
				}

			}
		}

		public virtual void POLineS_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

		public virtual void POLineS_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#endregion


	}
}
