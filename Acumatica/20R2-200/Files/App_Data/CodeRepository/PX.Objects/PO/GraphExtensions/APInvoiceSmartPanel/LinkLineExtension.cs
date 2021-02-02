using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.PO.DAC.Unbound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "LINK LINE" (Screen AP301000)
    /// </summary>
	[Serializable]
	public class LinkLineExtension : PXGraphExtension<APInvoiceEntry>
	{
        #region Data Members

        [PXCopyPasteHiddenView]
        public PXSelectJoin<POLandedCostDetailS,
            InnerJoin<POLandedCostDoc, On<POLandedCostDetailS.refNbr, Equal<POLandedCostDoc.refNbr>, And<POLandedCostDetailS.docType, Equal<POLandedCostDoc.docType>>>,
            InnerJoin<LandedCostCode, On<POLandedCostDetailS.landedCostCodeID, Equal<LandedCostCode.landedCostCodeID>>>>,
                Where<POLandedCostDoc.released, Equal<True>,
                    And<POLandedCostDetailS.aPRefNbr, IsNull,
                    And2<Where<POLandedCostDoc.vendorID, Equal<Current<APRegister.vendorID>>, Or<FeatureInstalled<FeaturesSet.vendorRelations>>>,
                    And2<Where<POLandedCostDoc.vendorLocationID, Equal<Current<APRegister.vendorLocationID>>, Or<FeatureInstalled<FeaturesSet.vendorRelations>>>,
                    And2<Where<POLandedCostDoc.payToVendorID, Equal<Current<APRegister.vendorID>>, Or<Not<FeatureInstalled<FeaturesSet.vendorRelations>>>>,
                    And<POLandedCostDoc.curyID, Equal<Current<APRegister.curyID>>
            >>>>>>> LinkLineLandedCostDetail;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<POLineS,
            LeftJoin<POOrder, On<POLineS.orderNbr, Equal<POOrder.orderNbr>, And<POLineS.orderType, Equal<POOrder.orderType>>>>,
            Where<POLineS.pOAccrualType, Equal<POAccrualType.order>,
                And<POLineS.orderNbr, Equal<Required<POLineS.orderNbr>>,
                And<POLineS.orderType, Equal<Required<POLineS.orderType>>,
                And<POLineS.lineNbr, Equal<Required<POLineS.lineNbr>>>>>>> POLineLink;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<
            POReceiptLineS,
                LeftJoin<POReceipt,
                    On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>,
                    And<POReceiptLineS.receiptType, Equal<POReceipt.receiptType>>>>,
            Where<POReceiptLineS.receiptNbr, Equal<Required<LinkLineReceipt.receiptNbr>>,
                And<POReceiptLineS.lineNbr, Equal<Required<LinkLineReceipt.receiptLineNbr>>>>>
            ReceipLineLinked;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<
            POReceiptLineS,
                LeftJoin<POReceipt,
                    On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>,
                    And<POReceiptLineS.receiptType, Equal<POReceipt.receiptType>>>>>
            linkLineReceiptTran;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<
            POLineS,
                LeftJoin<POOrder,
                    On<POLineS.orderNbr, Equal<POOrder.orderNbr>,
                        And<POLineS.orderType, Equal<POOrder.orderType>>>>>
            linkLineOrderTran;

        public PXFilter<LinkLineFilter> linkLineFilter;

        public PXFilter<POReceiptFilter> filter;

        #endregion

        #region Initialize

		public override void Initialize()
		{
			base.Initialize();

            linkLineReceiptTran.Cache.AllowDelete = false;
            linkLineReceiptTran.Cache.AllowInsert = false;
            linkLineOrderTran.Cache.AllowDelete = false;
            linkLineOrderTran.Cache.AllowInsert = false;

			PXUIFieldAttribute.SetEnabled(linkLineReceiptTran.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<POReceiptLineS.selected>(linkLineReceiptTran.Cache, null, true);
			PXUIFieldAttribute.SetEnabled(linkLineOrderTran.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<LinkLineOrder.selected>(linkLineOrderTran.Cache, null, true);
		}

        #endregion

        #region Actions

        public PXAction<APInvoice> linkLine;

        [PXUIField(DisplayName = AP.Messages.LinkLine, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, FieldClass = "DISTR", Visible = false)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
					restrictInMigrationMode: true,
					restrictForRegularDocumentInMigrationMode: true,
					restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable LinkLine(PXAdapter adapter)
		{
            Base.checkTaxCalcMode();
            if (Base.Transactions.Current.InventoryID != null)
			{
				Base.Transactions.Cache.ClearQueryCache(); // for correct PO link detection
				WebDialogResult res;
				if ((res = linkLineFilter.AskExt(
						(graph, view) =>
						{
                            linkLineFilter.Cache.SetValueExt<LinkLineFilter.inventoryID>(linkLineFilter.Current,
								Base.Transactions.Current.InventoryID);
                            linkLineFilter.Current.UOM = Base.Transactions.Current.UOM;
                            linkLineFilter.Current.POOrderNbr = null;
                            linkLineFilter.Current.SiteID = null;

							APTran apTran = Base.Transactions.Current;

                            linkLineOrderTran.Cache.Clear(); //TODO: closing modal window can't be handled
                            linkLineReceiptTran.Cache.Clear(); //TODO: closing modal window can't be handled
                            linkLineOrderTran.View.Clear(); //TODO: closing modal window can't be handled
                            linkLineReceiptTran.View.Clear(); //TODO: closing modal window can't be handled
                            linkLineOrderTran.Cache.ClearQueryCache(); //TODO: closing modal window can't be handled
                            linkLineReceiptTran.Cache.ClearQueryCache(); //TODO: closing modal window can't be handled
						}, true
					)) != WebDialogResult.None)
				{
					if (res == WebDialogResult.Yes &&
						(linkLineReceiptTran.Cache.Updated.Count() > 0 || linkLineOrderTran.Cache.Updated.Count() > 0))
					{
						APTran apTran = Base.Transactions.Current;
						apTran.ReceiptType = null;
						apTran.ReceiptNbr = null;
						apTran.ReceiptLineNbr = null;
						apTran.POOrderType = null;
						apTran.PONbr = null;
						apTran.POLineNbr = null;
						apTran.POAccrualType = null;
						apTran.POAccrualRefNoteID = null;
						apTran.POAccrualLineNbr = null;
						apTran.AccountID = null;
						apTran.SubID = null;
						apTran.SiteID = null;

						if (linkLineFilter.Current.SelectedMode == LinkLineFilter.selectedMode.Receipt)
						{
							foreach (POReceiptLineS receipt in linkLineReceiptTran.Cache.Updated)
							{
								if (receipt.Selected == true)
								{
									receipt.SetReferenceKeyTo(apTran);
									apTran.BranchID = receipt.BranchID;
									apTran.LineType = receipt.LineType;
									apTran.AccountID = receipt.POAccrualAcctID ?? receipt.ExpenseAcctID;
									apTran.SubID = receipt.POAccrualSubID ?? receipt.ExpenseSubID;
									apTran.SiteID = receipt.SiteID;
									break;
								}
							}
						}
						if (linkLineFilter.Current.SelectedMode == LinkLineFilter.selectedMode.Order)
						{
							foreach (POLineS order in linkLineOrderTran.Cache.Updated)
							{
								if (order.Selected == true)
								{
									order.SetReferenceKeyTo(apTran);
									apTran.BranchID = order.BranchID;
									apTran.LineType = order.LineType;
									apTran.AccountID = order.POAccrualAcctID ?? order.ExpenseAcctID;
									apTran.SubID = order.POAccrualSubID ?? order.ExpenseSubID;
									apTran.SiteID = order.SiteID;
									break;
								}
							}
						}
						Base.Transactions.Cache.Update(apTran);
						if (string.IsNullOrEmpty(apTran.ReceiptNbr) && string.IsNullOrEmpty(apTran.PONbr))
						{
							Base.Transactions.Cache.SetDefaultExt<APTran.accountID>(apTran);
							Base.Transactions.Cache.SetDefaultExt<APTran.subID>(apTran);
						}
					}
				}

			}
			else if (Base.Document.Current != null && Base.Document.Current.LCEnabled == true)
			{
				Base.Transactions.Cache.ClearQueryCache(); // for correct PO link detection
				WebDialogResult res;
				if ((res = linkLineFilter.AskExt(
						(graph, view) =>
						{
                            linkLineFilter.Cache.ClearQueryCache();
                            linkLineFilter.View.Clear();
                            linkLineFilter.Cache.Clear();
                            linkLineFilter.Current.SelectedMode = LinkLineFilter.selectedMode.LandedCost;

							LinkLineLandedCostDetail.Cache.Clear(); //TODO: closing modal window can't be handled
                            LinkLineLandedCostDetail.View.Clear(); //TODO: closing modal window can't be handled
                            LinkLineLandedCostDetail.Cache.ClearQueryCache(); //TODO: closing modal window can't be handled
						}, true
					)) != WebDialogResult.None)
				{
					if (res == WebDialogResult.Yes &&
						(LinkLineLandedCostDetail.Cache.Updated.Count() > 0))
					{
						APTran apTran = Base.Transactions.Current;

						foreach (POLandedCostDetailS detail in LinkLineLandedCostDetail.Cache.Updated)
						{
							if (detail.Selected == true)
							{
								Base.LinkLandedCostDetailLine(Base.Document.Current, apTran, detail);

								break;
							}
						}
					}
				}
			}
			return adapter.Get();
		}

        #endregion

        #region Events

        protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            APInvoice document = e.Row as APInvoice;
            if (document == null) return;

            var invoiceState = Base.GetDocumentState(cache, document);

            linkLine.SetVisible(invoiceState.IsDocumentInvoice);

            linkLine.SetEnabled(
                invoiceState.IsDocumentEditable &&
                invoiceState.AllowAddPOByProject &&
                 !invoiceState.IsRetainageDebAdj);
        }

        #region Selecting override

        public virtual IEnumerable LinkLineReceiptTran()
		{
			APTran currentAPTran = Base.Transactions.Current;
			if (currentAPTran == null) yield break;

			var comparer = new POAccrualComparer();
			HashSet<APTran> usedSourceLine = new HashSet<APTran>(comparer);
			HashSet<APTran> unusedSourceLine = new HashSet<APTran>(comparer);

			foreach (APTran aPTran in Base.Transactions.Cache.Inserted)
			{
				if (currentAPTran.InventoryID == aPTran.InventoryID
					&& currentAPTran.UOM == aPTran.UOM)
				{
					usedSourceLine.Add(aPTran);
				}
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Deleted)
			{
				if (currentAPTran.InventoryID == aPTran.InventoryID
					&& currentAPTran.UOM == aPTran.UOM
					&& Base.Transactions.Cache.GetStatus(aPTran) != PXEntryStatus.InsertedDeleted)
				{
					if (!usedSourceLine.Remove(aPTran))
					{
						unusedSourceLine.Add(aPTran);
					}
				}
			}

			foreach (APTran aPTran in Base.Transactions.Cache.Updated)
			{
				if (currentAPTran.InventoryID == aPTran.InventoryID && currentAPTran.UOM == aPTran.UOM)
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
			}

			unusedSourceLine.Add(currentAPTran);

			PXSelectBase<LinkLineReceipt> cmd = new PXSelect<LinkLineReceipt,
				Where2<
					Where<Current<LinkLineFilter.pOOrderNbr>, Equal<LinkLineReceipt.orderNbr>,
						Or<Current<LinkLineFilter.pOOrderNbr>, IsNull>>,
					And2<Where<Current<LinkLineFilter.siteID>, IsNull,
						Or<LinkLineReceipt.receiptSiteID, Equal<Current<LinkLineFilter.siteID>>>>,
					And<LinkLineReceipt.inventoryID, Equal<Current<APTran.inventoryID>>,
					And<LinkLineReceipt.uOM, Equal<Current<APTran.uOM>>,
					And<LinkLineReceipt.receiptType, Equal<POReceiptType.poreceipt>>>>>>>(Base);

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

			foreach (LinkLineReceipt item in cmd.Select())
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
					var res = (PXResult<POReceiptLineS, POReceipt>)ReceipLineLinked.Select(item.ReceiptNbr, item.ReceiptLineNbr);
					if (linkLineReceiptTran.Cache.GetStatus((POReceiptLineS)res) != PXEntryStatus.Updated
						&& ((POReceiptLineS)res).CompareReferenceKey(currentAPTran))
					{
                        linkLineReceiptTran.Cache.SetValue<POReceiptLineS.selected>((POReceiptLineS)res, true);
                        linkLineReceiptTran.Cache.SetStatus((POReceiptLineS)res, PXEntryStatus.Updated);
					}
					yield return res;
				}
			}

			foreach (APTran item in unusedSourceLine.Where(t => t.POAccrualType != null))
			{
				foreach (PXResult<POReceiptLineS, POReceipt> res in PXSelectJoin<POReceiptLineS,
					LeftJoin<POReceipt,
						On<POReceiptLineS.receiptNbr, Equal<POReceipt.receiptNbr>, And<POReceiptLineS.receiptType, Equal<POReceipt.receiptType>>>>,
					Where<POReceiptLineS.pOAccrualType, Equal<Required<LinkLineReceipt.pOAccrualType>>,
						And<POReceiptLineS.pOAccrualRefNoteID, Equal<Required<LinkLineReceipt.pOAccrualRefNoteID>>,
						And<POReceiptLineS.pOAccrualLineNbr, Equal<Required<LinkLineReceipt.pOAccrualLineNbr>>>>>>
					.Select(Base, item.POAccrualType, item.POAccrualRefNoteID, item.POAccrualLineNbr))
				{
					if (currentAPTran.InventoryID == ((POReceiptLineS)res).InventoryID)
					{
						if (linkLineReceiptTran.Cache.GetStatus((POReceiptLineS)res) != PXEntryStatus.Updated
							&& ((POReceiptLineS)res).CompareReferenceKey(currentAPTran))
						{
                            linkLineReceiptTran.Cache.SetValue<POReceiptLineS.selected>((POReceiptLineS)res, true);
                            linkLineReceiptTran.Cache.SetStatus((POReceiptLineS)res, PXEntryStatus.Updated);
						}

						yield return res;
					}
				}
			}
		}

		public virtual IEnumerable LinkLineOrderTran()
		{
			APTran currentAPTran = Base.Transactions.Current;
			if (currentAPTran == null)
				yield break;

			PXSelectBase<LinkLineOrder> cmd = new PXSelect<LinkLineOrder,
				Where2<
					Where<Current<LinkLineFilter.pOOrderNbr>, Equal<LinkLineOrder.orderNbr>,
						Or<Current<LinkLineFilter.pOOrderNbr>, IsNull>>,
					And2<Where<Current<LinkLineFilter.siteID>, IsNull,
						Or<LinkLineOrder.orderSiteID, Equal<Current<LinkLineFilter.siteID>>>>,
					And<LinkLineOrder.inventoryID, Equal<Current<APTran.inventoryID>>,
					And<LinkLineOrder.uOM, Equal<Current<APTran.uOM>>,
					And<LinkLineOrder.orderCuryID, Equal<Current<APInvoice.curyID>>>>>>>>(Base);

			if (Base.APSetup.Current.RequireSingleProjectPerDocument == true)
			{
				cmd.WhereAnd<Where<LinkLineOrder.projectID, Equal<Current<APInvoice.projectID>>>>();
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
			{
				cmd.WhereAnd<Where<LinkLineOrder.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
					And<LinkLineOrder.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
					And<LinkLineOrder.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<LinkLineOrder.vendorID, Equal<Current<APInvoice.vendorID>>,
					And<LinkLineOrder.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>();
			}

			var usedPOAccrual = new Lazy<POAccrualSet>(() =>
			{
				var r = Base.GetUsedPOAccrualSet();
				r.Remove(currentAPTran);
				return r;
			});

			foreach (LinkLineOrder item in cmd.Select().RowCast<LinkLineOrder>().AsEnumerable()
				.Where(l => !usedPOAccrual.Value.Contains(l)))
			{
				var res = (PXResult<POLineS, POOrder>)POLineLink.Select(item.OrderNbr, item.OrderType, item.OrderLineNbr);
				if (linkLineOrderTran.Cache.GetStatus((POLineS)res) != PXEntryStatus.Updated &&
					((POLineS)res).CompareReferenceKey(currentAPTran))
				{
					linkLineOrderTran.Cache.SetValue<POLineS.selected>((POLineS)res, true);
					linkLineOrderTran.Cache.SetStatus((POLineS)res, PXEntryStatus.Updated);
				}
				yield return res;
			}
		}

		protected virtual IEnumerable linkLineLandedCostDetail()
		{
			var result = Base.QuickSelect(LinkLineLandedCostDetail.View.BqlSelect);

			foreach (var poLandedCostDetail in Base.Caches<POLandedCostDetail>().Updated.RowCast<POLandedCostDetail>())
			{
				var tmpResultItem = result.RowCast<POLandedCostDetailS>().SingleOrDefault(t =>
					t.DocType == poLandedCostDetail.DocType && t.RefNbr == poLandedCostDetail.RefNbr &&
					t.LineNbr == poLandedCostDetail.LineNbr);

				if (tmpResultItem != null)
				{
					tmpResultItem.APDocType = poLandedCostDetail.APDocType;
					tmpResultItem.APRefNbr = poLandedCostDetail.APRefNbr;
				}
			}

			if (Base.Document.Current != null)
				result = result.RowCast<POLandedCostDetailS>().Where(t => t.APRefNbr != Base.Document.Current.RefNbr);

			return result;
		}

		#endregion

		#region LinkLineFilter

        public virtual void _(Events.FieldVerifying<LinkLineFilter.selectedMode> e)
        {
			var row = e.Row as LinkLineFilter;

			if (row == null)
				return;

			var hasInventory = Base.Transactions.Current.InventoryID.HasValue;

			if (String.Equals(e.NewValue, LinkLineFilter.selectedMode.LandedCost) && hasInventory)
			{
				e.NewValue = row.SelectedMode;
				e.Cancel = true;
			}

			if (!hasInventory)
			{
				e.NewValue = LinkLineFilter.selectedMode.LandedCost;
				e.Cancel = true;
			}
		}

		public virtual void LinkLineFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			LinkLineFilter row = (LinkLineFilter)e.Row;
			if (row != null)
			{
				PXCache orderReceiptCache = linkLineReceiptTran.Cache;

                linkLineReceiptTran.View.AllowSelect = row.SelectedMode == LinkLineFilter.selectedMode.Receipt;
                linkLineOrderTran.View.AllowSelect = row.SelectedMode == LinkLineFilter.selectedMode.Order;

				if (row.SelectedMode == LinkLineFilter.selectedMode.LandedCost)
				{
                    LinkLineLandedCostDetail.View.AllowSelect = true;
				}
				else
				{
                    LinkLineLandedCostDetail.View.AllowSelect = false;
				}
			}

		}

		#endregion

		#endregion
	}
}
