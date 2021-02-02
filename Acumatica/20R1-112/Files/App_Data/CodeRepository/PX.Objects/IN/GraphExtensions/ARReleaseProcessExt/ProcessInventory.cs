using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.TX;
using GLTranInsertionContext = PX.Objects.AR.ARReleaseProcess.GLTranInsertionContext;

namespace PX.Objects.IN.GraphExtensions.ARReleaseProcessExt
{
	public class ProcessInventory : PXGraphExtension<ARReleaseProcess>
	{
		public PXSetup<SOSetup> SOSetup;

		public PXSelect<INTran,
				Where<INTran.sOShipmentType, Equal<Current<ARTran.sOShipmentType>>,
					And<INTran.sOShipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>,
					And<INTran.sOOrderType, Equal<Current<ARTran.sOOrderType>>,
					And<INTran.sOOrderNbr, Equal<Current<ARTran.sOOrderNbr>>,
					And<INTran.sOOrderLineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>>>
			intranselect;

		public override void Initialize()
		{
			base.Initialize();

			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.refNbr>(intranselect.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranDate>(intranselect.Cache, null, false);
		}

		[PXOverride]
		public virtual void ReleaseInvoiceTransactionPostProcessed(JournalEntry je, ARInvoice ardoc, ARTran n,
			Action<JournalEntry, ARInvoice, ARTran> baseMethod)
		{
			baseMethod(je, ardoc, n);

			ProcessARTranInventory(n, ardoc, je);
		}

		public virtual void ProcessARTranInventory(ARTran n, ARInvoice ardoc, JournalEntry je)
		{
			if (Base.IsIntegrityCheck) return;

			bool tranCostSet = HandleARTranCost(n, ardoc, je);
			HandleARTranCostOrig(n, tranCostSet);
		}

		public virtual bool HandleARTranCost(ARTran n, ARInvoice ardoc, JournalEntry je)
		{
			if (n.LineType.IsNotIn(SOLineType.Inventory, SOLineType.NonInventory))
				return false;

			bool tranCostSet = false;
			n.TranCost = 0m;

			foreach (INTran intran in this.intranselect.View.SelectMultiBound(new object[] { n }))
			{
				intran.ARDocType = n.TranType;
				intran.ARRefNbr = n.RefNbr;
				intran.ARLineNbr = n.LineNbr;
				intran.UnitPrice = n.UnitPrice;
				intran.TranAmt = Math.Sign((decimal)n.Qty) != Math.Sign((decimal)n.BaseQty) ? -n.TranAmt : n.TranAmt;

				if (n.Qty != 0m && n.SOShipmentLineNbr == null)
				{
					object TranAmt = Math.Sign((decimal)n.Qty) != Math.Sign((decimal)n.BaseQty) ? -n.TranAmt : n.TranAmt / n.Qty * intran.Qty;
					this.intranselect.Cache.RaiseFieldUpdating<INTran.tranAmt>(intran, ref TranAmt);
					intran.TranAmt = (decimal?)TranAmt;
				}

				this.intranselect.Cache.MarkUpdated(intran);

				if (intran.Released == true)
				{
					INReleaseProcess.UpdateCustSalesStats(Base, intran);

					var initem = InventoryItem.PK.Find(Base, n.InventoryID);
					if (initem != null && initem.StkItem != true && initem.KitItem == true && PXAccess.FeatureInstalled<FeaturesSet.kitAssemblies>() && this.SOSetup.Current != null)
					{
						switch (this.SOSetup.Current.SalesProfitabilityForNSKits)
						{
							case SalesProfitabilityNSKitMethod.NSKitStandardCostOnly: //do nothing 
								break;
							case SalesProfitabilityNSKitMethod.NSKitStandardAndStockComponentsCost: //kit standard cost will be added later
								n.TranCost += intran.TranCost;
								n.TranCostOrig = n.TranCost;
								n.IsTranCostFinal = true;
								break;
							case SalesProfitabilityNSKitMethod.StockComponentsCostOnly:
								n.TranCost += intran.TranCost;
								n.TranCostOrig = n.TranCost;
								n.IsTranCostFinal = true;
								tranCostSet = true;
								break;
						}
					}
					else
					{
						n.TranCost += intran.TranCost;
						n.TranCostOrig = n.TranCost;
						n.IsTranCostFinal = true;
						tranCostSet = true;
					}
				}

				PostShippedNotInvoiced(intran, n, ardoc, je);
			}
			if (n.SOShipmentType == SO.SOShipmentType.DropShip)
			{
				foreach (INTran intran in PXSelect<INTran,
					Where<INTran.pOReceiptNbr, Equal<Current<ARTran.sOShipmentNbr>>,
						And<INTran.pOReceiptLineNbr, Equal<Current<ARTran.sOShipmentLineNbr>>,
						And<INTran.docType, Equal<IN.INDocType.adjustment>,
						And<INTran.released, Equal<True>, And<INTran.aRRefNbr, IsNull,
						And<INTran.sOShipmentNbr, IsNull>>>>>>>
					.SelectMultiBound(Base, new object[] { n }))
				{
					n.TranCost += intran.TranCost;
				}
			}

			return tranCostSet;
		}

		public virtual void PostShippedNotInvoiced(INTran intran, ARTran n, ARInvoice ardoc, JournalEntry je)
		{
			if (intran.UpdateShippedNotInvoiced != true)
				return;
			if (intran.Released != true)
			{
				throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(AR.Messages.ShippedNotInvoicedINtranNotReleased, intran.RefNbr));
			}
			var trancosts = PXSelect<INTranCost,
				Where<INTranCost.costDocType, Equal<Required<INTranCost.costDocType>>,
					And<INTranCost.costRefNbr, Equal<Required<INTranCost.costRefNbr>>,
					And<INTranCost.lineNbr, Equal<Required<INTranCost.lineNbr>>>>>>
				.Select(Base, intran.DocType, intran.RefNbr, intran.LineNbr);
			foreach (INTranCost trancost in trancosts)
			{
				var itemPostClassRes = (PXResult<InventoryItem, INPostClass>)PXSelectJoin<InventoryItem,
					LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
					.Select(Base, intran.InventoryID);
				ReasonCode reasoncode = INTran.FK.ReasonCode.FindParent(Base, intran);
				INSite site = INSite.PK.Find(Base, intran.SiteID);

				if (trancost != null && trancost.COGSAcctID != null && intran != null)
				{
					bool isSalesTransaction = ((trancost.InvtMult == (short)1) ^ n.IsCancellation == true);

					//Credit shipped-not-invoiced account
					GLTran tranFromIN = new GLTran();
					tranFromIN.SummPost = Base.SummPost;
					tranFromIN.BranchID = intran.BranchID;
					tranFromIN.TranType = trancost.TranType;
					tranFromIN.TranClass = GLTran.tranClass.ShippedNotInvoiced;

					tranFromIN.AccountID = intran.COGSAcctID ?? INReleaseProcess.GetAccountDefaults<INPostClass.cOGSAcctID>(Base, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes, intran);
					tranFromIN.SubID = intran.COGSSubID ?? INReleaseProcess.GetAccountDefaults<INPostClass.cOGSSubID>(Base, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes, intran);

					tranFromIN.CuryDebitAmt = isSalesTransaction ? trancost.TranCost : 0m;
					tranFromIN.DebitAmt = isSalesTransaction ? trancost.TranCost : 0m;
					tranFromIN.CuryCreditAmt = isSalesTransaction ? 0m : trancost.TranCost;
					tranFromIN.CreditAmt = isSalesTransaction ? 0m : trancost.TranCost;

					tranFromIN.RefNbr = trancost.RefNbr;
					tranFromIN.InventoryID = trancost.InventoryID;
					tranFromIN.Qty = isSalesTransaction ? trancost.Qty : -trancost.Qty;
					tranFromIN.UOM = intran.UOM;
					tranFromIN.TranDesc = intran.TranDesc;
					tranFromIN.TranDate = n.TranDate;
					bool isNonProject = trancost.InvtMult == (short)0;
					tranFromIN.ProjectID = isNonProject ? PM.ProjectDefaultAttribute.NonProject() : intran.ProjectID;
					tranFromIN.TaskID = isNonProject ? null : intran.TaskID;
					tranFromIN.CostCodeID = tranFromIN.CostCodeID;
					tranFromIN.Released = true;

					Base.InsertInvoiceDetailsINTranCostTransaction(je, tranFromIN,
						new GLTranInsertionContext { ARRegisterRecord = ardoc, ARTranRecord = n, INTranRecord = intran, INTranCostRecord = trancost });

					//Debit COGS account
					tranFromIN = new GLTran();
					tranFromIN.SummPost = Base.SummPost;
					tranFromIN.BranchID = n.BranchID;
					if (reasoncode?.Usage == ReasonCodeUsages.Issue)
					{
						tranFromIN.AccountID = reasoncode.AccountID;
						tranFromIN.SubID = INReleaseProcess.GetReasonCodeSubID(Base, reasoncode, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes);
					}
					else
					{
						tranFromIN.AccountID = INReleaseProcess.GetAccountDefaults<INPostClass.cOGSAcctID>(Base, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes);
						if (((INPostClass)itemPostClassRes) != null && ((INPostClass)itemPostClassRes).COGSSubFromSales != true) //we cannot use intran here to retrive cogs/sales sub 
							tranFromIN.SubID = INReleaseProcess.GetAccountDefaults<INPostClass.cOGSSubID>(Base, (InventoryItem)itemPostClassRes, site, (INPostClass)itemPostClassRes);
						else
							tranFromIN.SubID = n.SubID;
					}

					tranFromIN.CuryDebitAmt = isSalesTransaction ? 0m : trancost.TranCost;
					tranFromIN.DebitAmt = isSalesTransaction ? 0m : trancost.TranCost;
					tranFromIN.CuryCreditAmt = isSalesTransaction ? trancost.TranCost : 0m;
					tranFromIN.CreditAmt = isSalesTransaction ? trancost.TranCost : 0m;

					tranFromIN.TranType = trancost.TranType;
					tranFromIN.TranClass = GLTran.tranClass.ShippedNotInvoiced;
					tranFromIN.RefNbr = trancost.RefNbr;
					tranFromIN.InventoryID = trancost.InventoryID;
					tranFromIN.Qty = isSalesTransaction ? -trancost.Qty : trancost.Qty;
					tranFromIN.UOM = intran.UOM;
					tranFromIN.TranDesc = intran.TranDesc;
					tranFromIN.TranDate = n.TranDate;
					tranFromIN.ProjectID = (trancost.InvtMult == (short)1) ? PM.ProjectDefaultAttribute.NonProject() : intran.ProjectID;
					tranFromIN.TaskID = (trancost.InvtMult == (short)1) ? null : intran.TaskID;
					tranFromIN.CostCodeID = intran.CostCodeID;
					tranFromIN.Released = true;
					tranFromIN.TranLineNbr = (tranFromIN.SummPost == true) ? null : intran.LineNbr;

					Base.InsertInvoiceDetailsINTranCostTransaction(je, tranFromIN,
						new GLTranInsertionContext { ARRegisterRecord = ardoc, ARTranRecord = n, INTranRecord = intran, INTranCostRecord = trancost });
				}
			}
		}

		public virtual void HandleARTranCostOrig(ARTran n, bool tranCostSet)
		{
			if (n.InventoryID != null && (n.LineType.IsIn(null, SOLineType.MiscCharge) || n.LineType.IsIn(SOLineType.Inventory, SO.SOLineType.NonInventory) && !tranCostSet))
			{
				//TO DO: review this part and add more accurate cost selection conditions (INItemSite?)
				PXResult<IN.InventoryItem, IN.INItemSite> result = (PXResult<IN.InventoryItem, IN.INItemSite>)PXSelectJoin<IN.InventoryItem,
					LeftJoin<IN.INItemSite, On<IN.INItemSite.inventoryID, Equal<IN.InventoryItem.inventoryID>,
					And<IN.INItemSite.siteID, Equal<Required<IN.INItemSite.siteID>>>>>,
					Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.SelectSingleBound(Base, null, n.SiteID, n.InventoryID);
				if (result != null)
				{
					IN.InventoryItem item = (IN.InventoryItem)result;
					IN.INItemSite itemSite = (IN.INItemSite)result;
					if (item.StkItem == true)
					{
						if (itemSite.ValMethod != null)
						{
							if (item.ValMethod == IN.INValMethod.Standard)
							{
								n.TranCostOrig = n.Qty * itemSite.StdCost;
							}
							else
							{
								n.TranCostOrig = n.Qty * itemSite.AvgCost;
							}
						}
					}
					else if (n.SOShipmentType != SOShipmentType.DropShip)
					{
						if (n.AccrueCost == true && n.AccruedCost != null)
						{
							n.TranCost = n.AccruedCost;
							n.TranCostOrig = n.AccruedCost;
							n.IsTranCostFinal = true;
						}
						else
						{
							if (item.KitItem == true && PXAccess.FeatureInstalled<FeaturesSet.kitAssemblies>() && this.SOSetup.Current != null)
							{
								switch (this.SOSetup.Current.SalesProfitabilityForNSKits)
								{
									case SalesProfitabilityNSKitMethod.NSKitStandardCostOnly:
										n.TranCost += n.BaseQty * item.StdCost;
										n.TranCostOrig = n.TranCost;
										n.IsTranCostFinal = true;
										break;
									case SalesProfitabilityNSKitMethod.NSKitStandardAndStockComponentsCost:
										n.TranCost += n.BaseQty * item.StdCost;
										n.TranCostOrig = n.TranCost;
										n.IsTranCostFinal = HasStockComponents(n);
										break;
									case SalesProfitabilityNSKitMethod.StockComponentsCostOnly:
										n.IsTranCostFinal = HasStockComponents(n);
										break;
								}
							}
							else
							{
								n.TranCost += n.BaseQty * item.StdCost;
								n.TranCostOrig = n.TranCost;
								n.IsTranCostFinal = true;
							}
						}
					}
				}
			}
		}

		public virtual bool HasStockComponents(ARTran n)
		{
			return (n.IsTranCostFinal == true
					|| (PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>,
					And<SOShipLineSplit.lineNbr, Equal<Current<ARTran.sOShipmentLineNbr>>,
					And<SOShipLineSplit.isStockItem, Equal<True>>>>>.SelectSingleBound(Base, new object[] { n }).Count() == 0
					&& PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Current<ARTran.sOOrderType>>,
					And<SOLineSplit.orderNbr, Equal<Current<ARTran.sOOrderNbr>>,
					And<SOLineSplit.lineNbr, Equal<Current<ARTran.sOOrderLineNbr>>,
					And<SOLineSplit.isStockItem, Equal<True>>>>>>.SelectSingleBound(Base, new object[] { n }).Count() == 0));
		}
	}
}
