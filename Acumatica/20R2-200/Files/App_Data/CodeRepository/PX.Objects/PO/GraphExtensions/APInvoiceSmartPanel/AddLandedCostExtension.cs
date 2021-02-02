using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO.LandedCosts;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "ADD LC" (Screen AP301000)
    /// </summary>
    [Serializable]
	public class AddLandedCostExtension : PXGraphExtension<APInvoiceEntry>
	{
        #region Data Members

        [PXCopyPasteHiddenView()]
        public PXFilter<POLandedCostDetailFilter> landedCostFilter;

        [PXCopyPasteHiddenView]
        public PXSelect<POLandedCostDetailS> LandedCostDetailsAdd;

        [PXCopyPasteHiddenView]
        public PXSelect<POLandedCostDetail> LandedCostDetails;

        #endregion

        #region Initialize

        public static bool IsActive()
        {
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		public override void Initialize()
		{
			base.Initialize();
		}

        #endregion

        #region Actions

        public PXAction<APInvoice> addLandedCost;
        public PXAction<APInvoice> addLandedCost2;

        [PXUIField(DisplayName = "Add LC", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddLandedCost(PXAdapter adapter)
		{
			if (Base.Document.Current != null &&
				Base.Document.Current.Released != true)
			{
				if (LandedCostDetailsAdd.AskExt((graph, view) =>
				{
					landedCostFilter.Cache.ClearQueryCacheObsolete();
					landedCostFilter.View.Clear();
					landedCostFilter.Cache.Clear();

                    LandedCostDetailsAdd.Cache.ClearQueryCacheObsolete();
                    LandedCostDetailsAdd.View.Clear();
                    LandedCostDetailsAdd.Cache.Clear();
				}, true) == WebDialogResult.OK)
				{
					return AddLandedCost2(adapter);
				}
			}

			return adapter.Get();
		}

		[PXUIField(DisplayName = "Add LC", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AddLandedCost2(PXAdapter adapter)
		{
			if (Base.Document.Current != null &&
				Base.Document.Current.DocType.IsIn(APDocType.Invoice, APDocType.DebitAdj) &&
				Base.Document.Current.Released == false &&
				Base.Document.Current.Prebooked == false)
			{
				var landedCostDetails = LandedCostDetailsAdd.Cache.Updated.RowCast<POLandedCostDetailS>().Where(t => t.Selected == true).ToArray();
				Base.AddLandedCosts(landedCostDetails);
				landedCostDetails.ForEach(t => t.Selected = false);
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

            addLandedCost.SetVisible(invoiceState.IsDocumentInvoice || invoiceState.IsDocumentDebitAdjustment);

			PXUIFieldAttribute.SetEnabled(LandedCostDetailsAdd.Cache, null, false);

			bool allowAddLandedCost = invoiceState.IsDocumentEditable &&
				invoiceState.LandedCostEnabled &&
				!invoiceState.IsRetainageDebAdj;

			addLandedCost.SetEnabled(allowAddLandedCost);
			PXUIFieldAttribute.SetEnabled<POLandedCostDetailS.selected>(LandedCostDetailsAdd.Cache, null, allowAddLandedCost);
		}

        #region DAC Cache Attached

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBDefault(typeof(APInvoice.refNbr), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXParent(typeof(Select<APInvoice, Where<APInvoice.docType, Equal<Current<POLandedCostDetail.aPDocType>>, And<APInvoice.refNbr, Equal<Current<POLandedCostDetail.aPRefNbr>>>>>), LeaveChildren = true)]
		protected virtual void POLandedCostDetail_APRefNbr_CacheAttached(PXCache cache)
		{

		}

		#endregion

		#region Selecting override

		protected virtual IEnumerable landedCostDetailsAdd()
		{
			PXSelectBase<POLandedCostDetailS> cmd = new PXSelectJoinGroupBy<POLandedCostDetailS,
				InnerJoin<POLandedCostDoc, On<POLandedCostDetailS.docType, Equal<POLandedCostDoc.docType>, And<POLandedCostDetailS.refNbr, Equal<POLandedCostDoc.refNbr>>>,
						LeftJoin<POLandedCostReceiptLine, On<POLandedCostDetailS.docType, Equal<POLandedCostReceiptLine.docType>, And<POLandedCostDetailS.refNbr, Equal<POLandedCostReceiptLine.refNbr>>>,
						LeftJoin<POReceiptLine, On<POLandedCostReceiptLine.pOReceiptNbr, Equal<POReceiptLine.receiptNbr>, And<POLandedCostReceiptLine.pOReceiptLineNbr, Equal<POReceiptLine.lineNbr>>>>>>,
					Where<POLandedCostDetailS.aPRefNbr, IsNull,
				And2<Where<POLandedCostDoc.vendorID, Equal<Current<APRegister.vendorID>>, Or<FeatureInstalled<FeaturesSet.vendorRelations>>>,
				And2<Where<POLandedCostDoc.vendorLocationID, Equal<Current<APRegister.vendorLocationID>>, Or<FeatureInstalled<FeaturesSet.vendorRelations>>>,
				And2<Where<POLandedCostDoc.payToVendorID, Equal<Current<APRegister.vendorID>>, Or<Not<FeatureInstalled<FeaturesSet.vendorRelations>>>>,
				And<POLandedCostDoc.curyID, Equal<Current<APRegister.curyID>>,
					And<POLandedCostDoc.released, Equal<True>,
					And2<Where<Current<POLandedCostDetailFilter.receiptNbr>, IsNull, Or<Current<POLandedCostDetailFilter.receiptNbr>, Equal<POLandedCostReceiptLine.pOReceiptNbr>>>,
					And2<Where<Current<POLandedCostDetailFilter.orderNbr>, IsNull,
						Or<Where<Current<POLandedCostDetailFilter.orderType>, Equal<POReceiptLine.pOType>,
							And<Current<POLandedCostDetailFilter.orderNbr>, Equal<POReceiptLine.pONbr>>>>>,
					And2<Where<Current<POLandedCostDetailFilter.landedCostDocRefNbr>, IsNull, Or<Current<POLandedCostDetailFilter.landedCostDocRefNbr>, Equal<POLandedCostDetailS.refNbr>>>,
					And<Where<Current<POLandedCostDetailFilter.landedCostCodeID>, IsNull, Or<Current<POLandedCostDetailFilter.landedCostCodeID>, Equal<POLandedCostDetailS.landedCostCodeID>>>>
					>>>>>>>>>,
				Aggregate<GroupBy<POLandedCostDetailS.docType, GroupBy<POLandedCostDetailS.refNbr, GroupBy<POLandedCostDetailS.lineNbr>>>>>(Base);

			int startRow = PXView.StartRow;
			int totalRows = 0;

			var result = cmd.View.Select(PXView.Currents, PXView.Parameters, new object[PXView.SortColumns.Length], PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows).RowCast<POLandedCostDetailS>().ToList();
			PXView.StartRow = 0;

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
				result = result.Where(t => t.APRefNbr != Base.Document.Current.RefNbr).ToList();

			return result;
		}

		#endregion

		#endregion
	}
}
