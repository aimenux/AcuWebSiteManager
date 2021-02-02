using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO.DAC.Unbound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "ADD PO RECEIPT" (Screen AP301000)
    /// </summary>
    [Serializable]
	public class AddPOReceiptExtension : PXGraphExtension<LinkLineExtension, APInvoiceEntry>
	{
        #region Data Members

        [PXCopyPasteHiddenView]
        [PXViewName(Messages.POReceipt)]
        public PXSelect<POReceipt> poreceiptslist;

        #endregion

        #region Initialize

        public static bool IsActive()
        {
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		public override void Initialize()
		{
			base.Initialize();

			poreceiptslist.Cache.AllowInsert = false;
			poreceiptslist.Cache.AllowDelete = false;
		}

        #endregion

        #region Actions

        public PXAction<APInvoice> addPOReceipt;
        public PXAction<APInvoice> addPOReceipt2;

        [PXUIField(DisplayName = Messages.AddPOReceipt, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "DISTR")]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddPOReceipt(PXAdapter adapter)
		{
			Base.checkTaxCalcMode();
			if (Base.Document.Current != null &&
					Base.Document.Current.Released == false &&
					Base.Document.Current.Prebooked == false &&
				poreceiptslist.AskExt(
					(graph, view) =>
					{
                        Base1.filter.Cache.ClearQueryCacheObsolete();
                        Base1.filter.View.Clear();
                        Base1.filter.Cache.Clear();

						poreceiptslist.Cache.ClearQueryCacheObsolete();
						poreceiptslist.View.Clear();
						poreceiptslist.Cache.Clear();
					}
					, true) == WebDialogResult.OK)
			{
				Base.updateTaxCalcMode();
				AddPOReceipt2(adapter);
			}
			poreceiptslist.View.Clear();
			poreceiptslist.Cache.Clear();
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.AddPOReceipt, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddPOReceipt2(PXAdapter adapter)
		{
			if (Base.Document.Current != null &&
					Base.Document.Current.Released == false &&
					Base.Document.Current.Prebooked == false)
			{
				List<POReceipt> receipts = poreceiptslist.Cache.Updated.RowCast<POReceipt>().Where(rc => rc.Selected == true).ToList();
				foreach (POReceipt rc in receipts)
				{
					Base.InvoicePOReceipt(rc, null, usePOParemeters: false, keepOrderTaxes: false, errorIfUnreleasedAPExists: false);
				}
				var orders = PXSelectJoinGroupBy<POOrder,
					InnerJoin<POOrderReceipt, On<POOrderReceipt.pOType, Equal<POOrder.orderType>, And<POOrderReceipt.pONbr, Equal<POOrder.orderNbr>>>>,
					Where<POOrderReceipt.receiptNbr, In<Required<POOrderReceipt.receiptNbr>>>,
					Aggregate<GroupBy<POOrder.orderType, GroupBy<POOrder.orderNbr>>>>
					.Select(Base, new[] { receipts.Select(r => r.ReceiptNbr).ToArray() })
					.RowCast<POOrder>().ToList();
				Base.AttachPrepayment(orders);
			}
			poreceiptslist.View.Clear();
			poreceiptslist.Cache.Clear();
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

            addPOReceipt.SetVisible(invoiceState.IsDocumentInvoice ||
                invoiceState.IsDocumentDebitAdjustment);

            PXUIFieldAttribute.SetEnabled(poreceiptslist.Cache, null, false);

            bool allowAddPOReceipt = invoiceState.IsDocumentEditable &&
                invoiceState.AllowAddPOByProject &&
                Base.vendor.Current != null &&
                !invoiceState.IsRetainageDebAdj;

            addPOReceipt.SetEnabled(allowAddPOReceipt);
            PXUIFieldAttribute.SetEnabled<POReceipt.selected>(poreceiptslist.Cache, null, allowAddPOReceipt);
        }

		#endregion

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(LocationIDAttribute), nameof(LocationIDAttribute.Visible), false)]
		public virtual void POReceipt_VendorLocationID_CacheAttached(PXCache sender) { }

		#endregion

		#region Selecting override

		public virtual IEnumerable pOreceiptslist()
		{
			APInvoice doc = Base.Document.Current;
			if (doc?.VendorID == null
				|| doc.VendorLocationID == null
				|| doc.DocType != APDocType.Invoice
				&& doc.DocType != APDocType.DebitAdj)
			{
				yield break;
			}

			string poReceiptType = doc.DocType == APDocType.Invoice
				? POReceiptType.POReceipt
				: POReceiptType.POReturn;

			Dictionary<APTran, int> usedReceipt = new Dictionary<APTran, int>(new POReceiptComparer());

			int count;
			foreach (APTran aPTran in Base.Transactions.Cache.Inserted)
			{
				if (aPTran.ReceiptNbr != null)
				{
					usedReceipt.TryGetValue(aPTran, out count);
					usedReceipt[aPTran] = count + 1;
				}
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Deleted)
			{
				if (aPTran.ReceiptNbr != null && Base.Transactions.Cache.GetStatus(aPTran) != PXEntryStatus.InsertedDeleted)
				{
					usedReceipt.TryGetValue(aPTran, out count);
					usedReceipt[aPTran] = count - 1;
				}
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Updated)
			{
				string originalValue = (string)Base.Transactions.Cache.GetValueOriginal<APTran.receiptNbr>(aPTran);
				if (aPTran.ReceiptNbr != originalValue)
				{
					if (originalValue != null)
					{
						APTran originTran = new APTran { ReceiptNbr = originalValue };
						usedReceipt.TryGetValue(originTran, out count);
						usedReceipt[originTran] = count - 1;
					}
					if (aPTran.ReceiptNbr != null)
					{
						usedReceipt.TryGetValue(aPTran, out count);
						usedReceipt[aPTran] = count + 1;
					}
				}
			}

			PXSelectBase<POReceipt> cmd = new PXSelectJoinGroupBy<
				POReceipt,
				InnerJoin<POReceiptLineS, On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>>,
				LeftJoin<APTran, On<APTran.released, Equal<False>,
					And<APTran.pOAccrualType, Equal<POReceiptLineS.pOAccrualType>,
					And<APTran.pOAccrualRefNoteID, Equal<POReceiptLineS.pOAccrualRefNoteID>,
					And<APTran.pOAccrualLineNbr, Equal<POReceiptLineS.pOAccrualLineNbr>>>>>>>,
				Where<POReceipt.hold, Equal<False>,
					 And<POReceipt.released, Equal<True>,
					 And<POReceipt.receiptType, Equal<Required<POReceipt.receiptType>>,
					 And<APTran.refNbr, IsNull,
					 And<POReceiptLineS.unbilledQty, Greater<decimal0>,
					 And<Where<POReceiptLineS.pONbr, Equal<Current<POReceiptFilter.orderNbr>>,
						Or<Current<POReceiptFilter.orderNbr>, IsNull>>>>>>>>,
				Aggregate<
					GroupBy<POReceipt.receiptType,
					GroupBy<POReceipt.receiptNbr,
					Sum<POReceiptLineS.receiptQty,
					Sum<POReceiptLineS.unbilledQty,
					Count<POReceiptLineS.lineNbr>>>>>>>(Base);

			if (Base.APSetup.Current.RequireSingleProjectPerDocument == true)
			{
				cmd.WhereAnd<Where<POReceipt.projectID, Equal<Current<APInvoice.projectID>>>>();
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
			{
				cmd.Join<LeftJoin<POOrder, On<POOrder.orderType, Equal<POReceiptLineS.pOType>, And<POOrder.orderNbr, Equal<POReceiptLineS.pONbr>>>>>();

				cmd.WhereAnd<Where<POReceipt.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
					And<POReceipt.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
					And<Where<POOrder.payToVendorID, IsNull, Or<POOrder.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<POReceipt.vendorID, Equal<Current<APInvoice.vendorID>>,
					And<POReceipt.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>();
			}

			foreach (PXResult<POReceipt, POReceiptLineS, APTran> result in cmd.View.SelectMultiBound(new object[] { doc }, poReceiptType))
			{
				POReceipt receipt = result;
				APTran aPTran = new APTran { ReceiptNbr = receipt.ReceiptNbr };

				if (usedReceipt.TryGetValue(aPTran, out count))
				{
					usedReceipt.Remove(aPTran);

					if (count < result.RowCount)
					{
						yield return receipt;
					}
				}
				else
				{
					yield return receipt;
				}
			}

			foreach (APTran deletedTran in usedReceipt.Where(_ => _.Value < 0).Select(_ => _.Key))
			{
				yield return PXSelect<POReceipt, Where<POReceipt.receiptNbr, Equal<Required<APTran.receiptNbr>>>>
					.SelectSingleBound(Base, new object[] { }, deletedTran.ReceiptNbr)
					.RowCast<POReceipt>()
					.First();
			}
		}

		#endregion

		#region POReceipt Events

		public virtual void POReceipt_ReceiptNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			POReceipt row = (POReceipt)e.Row;
			APInvoice doc = Base.Document.Current;
			if (row != null && doc != null)
			{
				PXResultset<POOrderReceiptLink> receiptLinks = PXSelectGroupBy<POOrderReceiptLink, Where<POOrderReceiptLink.receiptNbr, Equal<Required<POReceipt.receiptNbr>>>,
					Aggregate<GroupBy<POOrderReceiptLink.curyID>>>.Select(Base, row.ReceiptNbr);

				if (receiptLinks.Count == 0)
					return;
				else if (receiptLinks.Count > 1 || ((POOrderReceiptLink)receiptLinks.First()).CuryID != doc.CuryID)
				{
					string fieldName = typeof(POReceipt.curyID).Name;
					PXErrorLevel msgLevel = PXErrorLevel.RowWarning;
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
					 null, null, AP.Messages.APDocumentCurrencyDiffersFromSourcePODocument, msgLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
					e.IsAltered = true;
				}
			}
		}

		#endregion

		#endregion
	}
}
