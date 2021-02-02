using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.RUTROT
{
	public class SOInvoiceEntryRUTROT : PXGraphExtension<SOInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}
		[PXRemoveBaseAttribute(typeof(PXUnboundFormulaAttribute))]
		protected virtual void SOTax_CuryRUTROTTaxAmt_CacheAttached(PXCache sender)
		{ }
		#region methods overrides
		[PXOverride]
		public virtual void InvoiceCreated(ARInvoice invoice, SOOrder source, SOInvoiceEntry.InvoiceCreatedDelegate baseMethod)
		{
			baseMethod(invoice, source);

			SOOrderRUTROT orderRR = PXCache<SOOrder>.GetExtension<SOOrderRUTROT>(source);
			if (orderRR?.IsRUTROTDeductible == true && invoice != null)
			{
				ARInvoiceRUTROT invoiceRR = PXCache<ARInvoice>.GetExtension<ARInvoiceRUTROT>(invoice);

				Base.Document.SetValueExt<ARInvoiceRUTROT.isRUTROTDeductible>(invoice, true);
				Base.Document.Update(invoice);

				RUTROT rutrot = PXSelect<RUTROT, 
					Where<RUTROT.docType, Equal<Required<SOOrder.orderType>>,
						And<RUTROT.refNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(Base, source.OrderType, source.OrderNbr);
				rutrot = RUTROTHelper.CreateCopy(Base.Rutrots.Cache, rutrot, invoice.DocType, invoice.RefNbr);
				rutrot = Base.Rutrots.Update(rutrot);

				foreach (RUTROTDistribution rutrotDetail in PXSelect<RUTROTDistribution, 
					Where<RUTROTDistribution.docType, Equal<Required<SOOrder.orderType>>,
						And<RUTROTDistribution.refNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(Base, source.OrderType, source.OrderNbr))
				{
					RUTROTDistribution new_detail = (RUTROTDistribution)Base.RRDistribution.Cache.CreateCopy(rutrotDetail);
					new_detail.RefNbr = null;
					new_detail.DocType = null;
					Base.RRDistribution.Insert(new_detail);
				}
			}
		}

		public delegate ARTran CreateTranFromMiscLineDelegate(SOOrderShipment orderShipment, SOMiscLine2 orderline);
		[PXOverride]
		public virtual ARTran CreateTranFromMiscLine(SOOrderShipment orderShipment, SOMiscLine2 orderline, CreateTranFromMiscLineDelegate baseMethod)
		{
			ARTran tran = baseMethod(orderShipment, orderline);
			SOLine line = PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
			And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
			And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>.Select(Base, orderline.OrderType, orderline.OrderNbr, orderline.LineNbr);
			SOLineRUTROT lineRR = PXCache<SOLine>.GetExtension<SOLineRUTROT>(line);
			ARTranRUTROT tranRR = PXCache<ARTran>.GetExtension<ARTranRUTROT>(tran);
			tranRR.IsRUTROTDeductible = lineRR.IsRUTROTDeductible;
			tranRR.RUTROTItemType = lineRR.RUTROTItemType;
			tranRR.RUTROTWorkTypeID = lineRR.RUTROTWorkTypeID;
			return tran;
		}

		public delegate ARTran CreateTranFromShipLineDelegate(ARInvoice newdoc, SOOrderType ordertype, string operation, SOLine orderline, ref SOShipLine shipline);
		[PXOverride]
		public virtual ARTran CreateTranFromShipLine(ARInvoice newdoc, SOOrderType ordertype, string operation, SOLine orderline, ref SOShipLine shipline, CreateTranFromShipLineDelegate baseMethod)
		{
			ARTran tran = baseMethod(newdoc, ordertype, operation, orderline, ref shipline);
			SOLine line = PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
			And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>,
			And<SOLine.lineNbr, Equal<Required<SOLine.lineNbr>>>>>>.Select(Base, orderline.OrderType, orderline.OrderNbr, orderline.LineNbr);
			SOLineRUTROT lineRR = PXCache<SOLine>.GetExtension<SOLineRUTROT>(line);
			ARTranRUTROT tranRR = PXCache<ARTran>.GetExtension<ARTranRUTROT>(tran);
			tranRR.IsRUTROTDeductible = lineRR.IsRUTROTDeductible;
			tranRR.RUTROTItemType = lineRR.RUTROTItemType;
			tranRR.RUTROTWorkTypeID = lineRR.RUTROTWorkTypeID;
			return tran;
		}

        public PXAction<ARInvoice> release;

        [PXUIField(DisplayName = "Release", Visible = false)]
        [PXButton]
        public IEnumerable Release(PXAdapter adapter)
        {


            if (RUTROTHelper.IsNeedBalancing(Base, Base.Document.Current, RUTROTBalanceOn.Release))
            {
                Base.Save.Press();

                ARInvoice doc = Base.Document.Current;
                PXLongOperation.StartOperation(Base, delegate ()
                {
                    RUTROT rutrot = PXSelect<RUTROT, Where<RUTROT.refNbr, Equal<Required<ARInvoice.refNbr>>,
                            And<RUTROT.docType, Equal<Required<ARInvoice.docType>>>>>.Select(Base, doc.RefNbr, doc.DocType);

                    ARInvoiceEntry invoiceEntryGraph = PXGraph.CreateInstance<ARInvoiceEntry>();

                    RUTROTHelper.BalanceARInvoiceRUTROT(invoiceEntryGraph, doc, OnRelease: true, rutrot: rutrot);

                    RUTROTHelper.CreateAdjustment(invoiceEntryGraph, doc, rutrot);

                    Base.ReleaseProcess(new List<ARRegister> { doc });
                });

                return new List<ARInvoice> { Base.Document.Current };
            }

            return Base.Release(adapter);
        }
        #endregion

        protected virtual void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (ARTran)e.Row;

            if (row == null)
            {
                return;
            }

            IN.InventoryItem item = PXSelect<IN.InventoryItem, Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(Base, row.InventoryID);
            InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<IN.InventoryItem, InventoryItemRUTROT>(item);

            Base.Transactions.Cache.SetValueExt<ARTranRUTROT.isRUTROTDeductible>(e.Row, itemRR?.IsRUTROTDeductible ?? false);
        }
    }
}
