using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing lot/serial number attribute
    /// </summary>
    public class AMLotSerialNbrAttribute : INLotSerialNbrAttribute
    {
        public AMLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType) : base(InventoryType, SubItemType, LocationType)
        {
        }

        public AMLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType) : base(InventoryType, SubItemType, LocationType, ParentLotSerialNbrType)
        {
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (IsMaterialDocType(e.Row))
            {
                VerifyMaterialLotSerial(sender, e);
                return;
            }

            base.FieldVerifying(sender, e);
        }

        protected virtual string GetAMDocType(object row)
        {
            if (row == null)
            {
                return null;
            }

            if (row is AMMTran)
            {
                return ((AMMTran)row).DocType;
            }

            if (row is AMMTranSplit)
            {
                return ((AMMTranSplit)row).DocType;
            }

            return null;
        }
        
        protected bool IsMaterialDocType(object row)
        {
            return (GetAMDocType(row) ?? string.Empty) == AMDocType.Material;
        }

        protected bool IsByproductReceipt(PXCache cache, object row)
        {
            if (row is AMMTranSplit)
            {
                return IsByproductReceipt(GetParent(cache, (AMMTranSplit)row));
            }

            if (row is AMMTran)
            {
                return IsByproductReceipt((AMMTran)row);
            }

            return false;
        }

        protected bool IsByproductReceipt(AMMTran row)
        {
            return row != null && row.DocType == AMDocType.Material && row.IsByproduct.GetValueOrDefault() && row.TranType == INTranType.Receipt;
        }

        protected bool IsByproductReturn(PXCache cache, object row)
        {
            if (row is AMMTranSplit)
            {
                return IsByproductReturn(GetParent(cache, (AMMTranSplit)row));
            }

            if (row is AMMTran)
            {
                return IsByproductReturn((AMMTran) row);
            }

            return false;
        }

        protected bool IsByproductReturn(AMMTran row)
        {
            return row != null && row.DocType == AMDocType.Material && row.IsByproduct.GetValueOrDefault() && row.TranType == INTranType.Issue;
        }

        protected virtual void VerifyMaterialLotSerial(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var tranType = ((ILSMaster) e.Row).TranType;
            var docType = ((IAMBatch) e.Row).DocType;

            if (string.IsNullOrWhiteSpace(tranType) || string.IsNullOrWhiteSpace(docType))
            {
                base.FieldVerifying(sender, e);
                return;
            }

            var isReturn = docType == AMDocType.Material && (tranType == INTranType.Return ||
                                          !string.IsNullOrWhiteSpace((string) e.NewValue) && tranType == INTranType.Issue && IsByproductReturn(sender, e.Row));

            if (!isReturn)
            {
                if (!string.IsNullOrWhiteSpace((string)e.NewValue) && IsByproductReceipt(sender, e.Row))
                {
                    return;
                }

                base.FieldVerifying(sender, e);
                return;
            }

            try
            {
                if (MaterialLotSerialNotIssued(sender, (ILSMaster)e.Row, (string)e.NewValue))
                {
                    e.NewValue = null;
                    return;
                }
            }
            catch (Exception)
            {
                e.NewValue = null;
                throw;
            }

            e.Cancel = true;
        }

        protected virtual bool MaterialLotSerialNotIssued(PXCache cache, ILSMaster row)
        {
            return MaterialLotSerialNotIssued(cache, row, row?.LotSerialNbr);
        }

        protected virtual bool MaterialLotSerialNotIssued(PXCache cache, ILSMaster row, string lotSerialNbr)
        {
            if (row == null
                || string.IsNullOrWhiteSpace(lotSerialNbr))
            {
                return false;
            }

            AMMTran returningAMMTran = row as AMMTran;
            if (returningAMMTran == null && row is AMMTranSplit)
            {
                returningAMMTran = GetParent(cache, (AMMTranSplit)row);
            }

            if (returningAMMTran == null
                || string.IsNullOrWhiteSpace(returningAMMTran.OrderType)
                || string.IsNullOrWhiteSpace(returningAMMTran.ProdOrdID))
            {
                return false;
            }

            if (!MaterialLotSerialNbrIssuedToOrder(cache.Graph, returningAMMTran, lotSerialNbr))
            {
                if (cache is PXCache<AMMTranSplit>)
                {
                    RaiseMaterialLotSerialNeverIssued<AMMTranSplit.lotSerialNbr>(cache, row, returningAMMTran.OrderType, returningAMMTran.ProdOrdID, lotSerialNbr, returningAMMTran.IsByproduct.GetValueOrDefault());
                    return true;
                }

                if (cache is PXCache<AMMTran>)
                {
                    RaiseMaterialLotSerialNeverIssued<AMMTran.lotSerialNbr>(cache, row, returningAMMTran.OrderType, returningAMMTran.ProdOrdID, lotSerialNbr, returningAMMTran.IsByproduct.GetValueOrDefault());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Raise the cache exception handling
        /// </summary>
        protected static void RaiseMaterialLotSerialNeverIssued<T>(PXCache cache, object row, string orderType, string prodOrdID, string lotSerialNbr, bool isByproduct) where T : IBqlField
        {
            var msg = isByproduct
                ? Messages.GetLocal(Messages.ByproductLotSerialNotReceivedWithOrder, 
                    lotSerialNbr.TrimIfNotNullEmpty(),
                    orderType.TrimIfNotNullEmpty(),
                    prodOrdID.TrimIfNotNullEmpty())
                : Messages.GetLocal(Messages.LotSerialNotIssuedToOrder,
                    lotSerialNbr.TrimIfNotNullEmpty(),
                    orderType.TrimIfNotNullEmpty(),
                    prodOrdID.TrimIfNotNullEmpty());

            cache.RaiseExceptionHandling<T>(row, lotSerialNbr, new PXSetPropertyException<T>(msg));
        }

        /// <summary>
        /// Get the split's parent record
        /// </summary>
        protected virtual AMMTran GetParent(PXCache cache, AMMTranSplit split)
        {
            var parent = (AMMTran)PXParentAttribute.LocateParent(cache, split, typeof(AMMTran));

            if (parent != null)
            {
                return parent;
            }

            return (AMMTran) PXParentAttribute.SelectParent(cache, split, typeof(AMMTran));
        }

        /// <summary>
        /// For the given returning AMMTran record, was it ever issued to the production order.
        /// Match on: Order Type, ProdOrdID, InventoryID, and Lot/Serial nbr
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="returningAMMTran">Returning material record</param>
        /// <param name="lotSerialNbr">Lot/Serial number to search for</param>
        /// <returns>True when the lot/serial number was found as issued to the production order</returns>
        public static bool MaterialLotSerialNbrIssuedToOrder(PXGraph graph, AMMTran returningAMMTran, string lotSerialNbr)
        {
            if (returningAMMTran == null
                || string.IsNullOrWhiteSpace(lotSerialNbr)
                || string.IsNullOrWhiteSpace(returningAMMTran.OrderType)
                || string.IsNullOrWhiteSpace(returningAMMTran.ProdOrdID))
            {
                return false;
            }

            var split = FindFirstMaterialIssuedLotSerialNbr(graph, returningAMMTran, lotSerialNbr);
            return split != null;
        }

        /// <summary>
        /// First the first issue record that matches the returning ammtran record and lot/serial number.
        /// Match on: Order Type, ProdOrdID, InventoryID, and Lot/Serial nbr
        /// </summary>
        /// <param name="graph">Calling graph</param>
        /// <param name="returningAMMTran">Returning material record</param>
        /// <param name="lotSerialNbr">Lot/Serial number to search for</param>
        /// <returns>First found split record. If no return this indicates the lot/serial number was never issued</returns>
        protected static AMMTranSplit FindFirstMaterialIssuedLotSerialNbr(PXGraph graph, AMMTran returningAMMTran, string lotSerialNbr)
        {
            if (returningAMMTran == null)
            {
                return null;
            }

            var tranType = returningAMMTran.IsByproduct.GetValueOrDefault() ? AMTranType.Receipt : AMTranType.Issue;

            return PXSelectJoin<AMMTranSplit,
                InnerJoin<AMMTran, On<AMMTranSplit.docType, Equal<AMMTran.docType>,
                    And<AMMTranSplit.batNbr, Equal<AMMTran.batNbr>,
                    And<AMMTranSplit.lineNbr, Equal<AMMTran.lineNbr>>>>>,
                Where<AMMTran.docType, Equal<AMDocType.material>,
                    And<AMMTran.tranType, Equal<Required<AMMTran.tranType>>,
                    And<AMMTran.released, Equal<True>,
                    And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                    And<AMMTran.inventoryID, Equal<Required<AMMTran.inventoryID>>,
                    And<AMMTranSplit.lotSerialNbr, Equal<Required<AMMTranSplit.lotSerialNbr>>>>>>>>>
                >.SelectWindowed(graph, 0, 1, tranType, returningAMMTran.OrderType, returningAMMTran.ProdOrdID, returningAMMTran.InventoryID, lotSerialNbr);
        }
    }
}