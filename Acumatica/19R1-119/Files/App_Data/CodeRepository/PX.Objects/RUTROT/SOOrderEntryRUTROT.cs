using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using System.Linq;
using System.Collections.Generic;

namespace PX.Objects.RUTROT
{
	public class SOOrderEntryRUTROT : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}

		public RUTROTManager<SOOrder, SOLine, SOOrderRUTROT, SOLineRUTROT> RRManager;

		public override void Initialize()
		{
			base.Initialize();
			RRManager = new RUTROTManager<SOOrder, SOLine, SOOrderRUTROT, SOLineRUTROT>(
				this.Base, Rutrots, RRDistribution, Base.Document, Base.Transactions, Base.currencyinfo);
		}

		public PXSelect<RUTROT, Where<RUTROT.docType, Equal<Current<SOOrder.orderType>>,
						And<RUTROT.refNbr, Equal<Current<SOOrder.orderNbr>>>>> Rutrots;

		public PXSelect<RUTROTDistribution,
					Where<RUTROTDistribution.docType, Equal<Current<RUTROT.docType>>,
						And<RUTROTDistribution.refNbr, Equal<Current<RUTROT.refNbr>>>>> RRDistribution;

		public PXSetup<ARSetup> ARSetup;

		public PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>> CurrentBranch;

		public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>> InventoryItem;

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(SOOrder.orderType))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<RUTROT.docType>>,
		And<SOOrder.orderNbr, Equal<Current<RUTROT.refNbr>>>>>))]
		protected virtual void RUTROT_DocType_CacheAttached(PXCache sender)
		{ }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(SOOrder.orderNbr))]
		protected virtual void RUTROT_RefNbr_CacheAttached(PXCache sender)
		{ }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(RUTROT.docType))]
		protected virtual void RUTROTDistribution_DocType_CacheAttached(PXCache sender)
		{ }
		#region Overrides
		public delegate void CopyOrderProcDelegate(SOOrder order, CopyParamFilter copyFilter);
		[PXOverride]
		public virtual void CopyOrderProc(SOOrder order, CopyParamFilter copyFilter, CopyOrderProcDelegate baseMethod)
		{
			List<RUTROTDistribution> rutrotDistribution = null;
			SOOrderRUTROT orderRR = RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(order);
			if (RUTROTHelper.IsRUTROTcompatibleType(order.OrderType) && orderRR?.IsRUTROTDeductible == true)
			{
				rutrotDistribution = RRDistribution.Select().RowCast<RUTROTDistribution>().ToList();
			}

			baseMethod(order, copyFilter);
			if (RUTROTHelper.IsRUTROTcompatibleType(copyFilter.OrderType) && rutrotDistribution.Count > 0)
			{
				foreach (RUTROTDistribution distribution in rutrotDistribution)
				{
					RUTROTDistribution new_distribution = (RUTROTDistribution)RRDistribution.Cache.CreateCopy(distribution);
					new_distribution.RefNbr = null;
					new_distribution.DocType = null;
					RRDistribution.Insert(new_distribution);
				}
			}
		}
		[PXOverride]
		public virtual void OrderCreated(SOOrder document, SOOrder source, SOOrderEntry.OrderCreatedDelegate baseMethod)
		{
			baseMethod(document, source);
			SOOrderRUTROT sourceRR = RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(source);
			if (RUTROTHelper.IsRUTROTcompatibleType(source.OrderType) && sourceRR?.IsRUTROTDeductible == true)
			{
				if (RUTROTHelper.IsRUTROTcompatibleType(document.OrderType))
				{
					RUTROT rutrotRecord = PXSelect<RUTROT,
						Where<RUTROT.docType, Equal<Required<SOOrder.orderType>>,
						And<RUTROT.refNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(this.Base, source.OrderType, source.OrderNbr);
					rutrotRecord = RUTROTHelper.CreateCopy(Rutrots.Cache, rutrotRecord, Base.Document.Current.OrderType, Base.Document.Current.OrderNbr);
					rutrotRecord = Rutrots.Update(rutrotRecord);
				}
				else
				{
					Base.Document.Cache.SetValueExt<SOOrderRUTROT.isRUTROTDeductible>(document, false);
					Base.Document.Update(document);
				}
			}
		}

		public delegate void InsertSOAdjustmentsDelegate(SOOrder order, ARPaymentEntry docgraph, ARPayment payment);
		[PXOverride]
		public virtual void InsertSOAdjustments(SOOrder order, ARPaymentEntry docgraph, ARPayment payment, InsertSOAdjustmentsDelegate baseMethod)
		{
			baseMethod(order, docgraph, payment);
			SOOrderRUTROT orderRR = RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(order);
			if (orderRR.IsRUTROTDeductible == true)
			{
				RUTROT rutrot = PXSelect<RUTROT, Where<RUTROT.docType, Equal<Required<SOOrder.orderType>>,
					And<RUTROT.refNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(Base, order.OrderType, order.OrderNbr);
				foreach (SOAdjust adj in docgraph.SOAdjustments.Select())
				{
					SOAdjust other = PXSelectGroupBy<SOAdjust,
						Where<SOAdjust.voided, Equal<False>,
						And<SOAdjust.adjdOrderType, Equal<Required<SOAdjust.adjdOrderType>>,
						And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>,
						And<Where<SOAdjust.adjgDocType, NotEqual<Required<SOAdjust.adjgDocType>>, Or<SOAdjust.adjgRefNbr, NotEqual<Required<SOAdjust.adjgRefNbr>>>>>>>>,
						Aggregate<GroupBy<SOAdjust.adjdOrderType,
						GroupBy<SOAdjust.adjdOrderNbr, Sum<SOAdjust.curyAdjdAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(Base, adj.AdjdOrderType, adj.AdjdOrderNbr, adj.AdjgDocType, adj.AdjgRefNbr);
					if (other == null || other.AdjdOrderNbr == null)
					{
						docgraph.SOAdjustments.Cache.SetValueExt<SOAdjust.curyAdjgAmt>(adj, adj.CuryAdjgAmt - rutrot.CuryTotalAmt);
						docgraph.SOAdjustments.Update(adj);
					}
				}
			}
		}
		#endregion
		#region Events
		#region SOOrder
		protected virtual void SOOrder_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RRManager.UncheckRUTROTIfProhibited(RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(e.Row as SOOrder), CurrentBranch.SelectSingle(((SOOrder)e.Row).BranchID));
		}

		protected virtual void SOOrder_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RRManager.UncheckRUTROTIfProhibited(RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>((SOOrder)e.Row), CurrentBranch.SelectSingle(((SOOrder)e.Row).BranchID));
		}

		protected virtual void SOOrder_IsRUTROTDeductible_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = (SOOrder)e.Row;
			if (row != null)
			{
				SOOrderRUTROT rowRR = RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(row);
				if (rowRR.IsRUTROTDeductible == true)
				{
					row.BillSeparately = true;
				}
			}
		}

		protected virtual void SOOrder_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (SOOrder)e.Row;

			if (row == null)
			{
				return;
			}

			SOOrderRUTROT doc = RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(row);

			if (!RUTROTHelper.CurrenciesMatch(RUTROTHelper.GetBranchRUTROT(Base), doc))
			{
				doc.IsRUTROTDeductible = false;
			}
		}

		protected virtual void SOOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected baseMethod)
		{
			baseMethod(cache, e);
			SOOrderRUTROT doc = RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(e.Row as SOOrder);
			if (doc == null) return;

		    if (doc.IsRUTROTDeductible == true)
		    {
		        PXUIFieldAttribute.SetEnabled<SOOrder.billSeparately>(cache, e.Row, false);
		    }

		    RRManager.Update((SOOrder)e.Row);
			UpdateLinesControls(doc.IsRUTROTDeductible == true);
		}
		#endregion
		#region SOLine
		public virtual void SOLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RRManager.UpdateTranDeductibleFromInventory(RUTROTHelper.GetExtensionNullable<SOLine, SOLineRUTROT>((SOLine)e.Row), RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(Base.Document.Current));
		}

		protected virtual void SOLine_RUTROTType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLineRUTROT row = RUTROTHelper.GetExtensionNullable<SOLine, SOLineRUTROT>((SOLine)e.Row);
			if (row != null)
			{
				row.RUTROTWorkTypeID = null;
			}
		}

		protected virtual void SOLine_RUTROTItemType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			SOLineRUTROT rowRR = RUTROTHelper.GetExtensionNullable<SOLine, SOLineRUTROT>(row);

			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<SOLineRUTROT.rUTROTWorkTypeID>(sender, row, rowRR.RUTROTItemType != RUTROTItemTypes.OtherCost);
				if (rowRR.RUTROTItemType == RUTROTItemTypes.OtherCost)
				{
					sender.SetValueExt<SOLineRUTROT.rUTROTWorkTypeID>(row, null);
				}
			}
		}

		protected virtual void SOLine_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
			SOLine tran = (SOLine)e.Row;
			if (tran == null)
				return;
			if (Base.Document.Current == null)
				return;
			if (e.NewValue == null)
				return;

			string value = Rutrots.Current?.RUTROTType;
			InventoryItem item = (InventoryItem)InventoryItem.Select((int)e.NewValue);
			InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<InventoryItem, InventoryItemRUTROT>(item);
			if (!RUTROTHelper.IsItemMatchRUTROTType(value, item, itemRR, itemRR?.IsRUTROTDeductible == true))
			{
				sender.RaiseExceptionHandling<SOLine.inventoryID>(tran, item.InventoryCD, new PXSetPropertyException<SOLine.inventoryID>(RUTROTMessages.LineDoesNotMatchDoc));
				e.NewValue = item.InventoryCD;
				throw new PXSetPropertyException<SOLine.inventoryID>(RUTROTMessages.LineDoesNotMatchDoc);
			}
		}

		public virtual void SOLine_IsRUTROTDeductible_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine tran = (SOLine)e.Row;
			if (tran == null)
				return;
			if (Base.Document.Current == null)
				return;
			if (e.NewValue == null || (bool)e.NewValue == false)
				return;

			string value = Rutrots.Current?.RUTROTType;
			InventoryItem item = (InventoryItem)InventoryItem.Select(tran.InventoryID);
			InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<InventoryItem, InventoryItemRUTROT>(item);
			if (!RUTROTHelper.IsItemMatchRUTROTType(value, item, itemRR, (bool)e.NewValue))
			{
				sender.RaiseExceptionHandling<SOLineRUTROT.isRUTROTDeductible>(tran, false, new PXSetPropertyException<SOLineRUTROT.isRUTROTDeductible>(RUTROTMessages.LineDoesNotMatchDoc));
				e.NewValue = false;
				throw new PXSetPropertyException<SOLineRUTROT.isRUTROTDeductible>(RUTROTMessages.LineDoesNotMatchDoc);
			}
		}

		protected virtual void SOLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;
			SOLine row = e.Row as SOLine;
			SOLineRUTROT rowRR = RUTROTHelper.GetExtensionNullable<SOLine, SOLineRUTROT>(row);
			SOOrder doc = Base.Document.Current;
			SOOrderRUTROT docRR = RUTROTHelper.GetExtensionNullable<SOOrder, SOOrderRUTROT>(doc);
			RUTROTWorkType workType = PXSelect<RUTROTWorkType, Where<RUTROTWorkType.workTypeID, Equal<Required<RUTROTWorkType.workTypeID>>>>.Select(this.Base, rowRR.RUTROTWorkTypeID);
			if (rowRR?.IsRUTROTDeductible == true && doc.Completed != true && rowRR?.RUTROTItemType != RUTROTItemTypes.OtherCost
				&& doc.Rejected != true && !RUTROTHelper.IsUpToDateWorkType(workType, doc.OrderDate ?? DateTime.Now))
			{
				sender.RaiseExceptionHandling<SOLineRUTROT.rUTROTWorkTypeID>(row, workType?.Description, new PXSetPropertyException(RUTROTMessages.ObsoleteWorkType));
			}
			else
			{
				sender.RaiseExceptionHandling<SOLineRUTROT.rUTROTWorkTypeID>(row, workType?.Description, null);
			}
			PXUIFieldAttribute.SetEnabled<SOLineRUTROT.isRUTROTDeductible>(Base.Transactions.Cache, row, row.IsStockItem != true && docRR.IsRUTROTDeductible == true);
		}

        protected virtual void SOLine_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (SOLine)e.Row;
            SOLineRUTROT rowRR = RUTROTHelper.GetExtensionNullable<SOLine, SOLineRUTROT>(row);

            rowRR.CuryRUTROTTaxAmountDeductible = decimal.Zero;
        }
        #endregion

        #endregion

        private void UpdateLinesControls(bool showSection)
		{
			PXUIFieldAttribute.SetVisible<SOLineRUTROT.isRUTROTDeductible>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetVisible<SOLineRUTROT.rUTROTItemType>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetEnabled<SOLineRUTROT.rUTROTItemType>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetVisible<SOLineRUTROT.rUTROTWorkTypeID>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetVisible<SOLineRUTROT.curyRUTROTAvailableAmt>(Base.Transactions.Cache, null, showSection);
		}
	}
}
