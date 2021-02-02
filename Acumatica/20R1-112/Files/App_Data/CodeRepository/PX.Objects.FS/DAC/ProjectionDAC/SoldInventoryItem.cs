using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    #region PXProjection
    // TODO:
    // LotSerialNbr should be taken from SOShipLineSplit before SOLineSplit
    [Serializable]
    [PXProjection(typeof(
            Select2<InventoryItem,
            InnerJoin<ARTran,
                On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>,
                And<ARTran.tranType, Equal<ARDocType.invoice>>>,
            InnerJoin<ARInvoice,
                On<ARInvoice.refNbr, Equal<ARTran.refNbr>,
                And<ARInvoice.docType, Equal<ARDocType.invoice>,
                And<ARInvoice.released, Equal<True>>>>,
            InnerJoin<SOLineSplit,
                On<SOLineSplit.orderType, Equal<ARTran.sOOrderType>,
                And<SOLineSplit.orderNbr, Equal<ARTran.sOOrderNbr>,
                And<SOLineSplit.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>,
            InnerJoin<SOLine,
                On<SOLine.orderType, Equal<SOLineSplit.orderType>,
                And<SOLine.orderNbr, Equal<SOLineSplit.orderNbr>,
                And<SOLine.lineNbr, Equal<SOLineSplit.lineNbr>>>>,
            LeftJoin<FSEquipment,
                On<FSEquipment.inventoryID, Equal<InventoryItem.inventoryID>,
                And<FSEquipment.sourceType, Equal<FSEquipment.sourceType.AR_INVOICE>,
                And<FSEquipment.sourceRefNbr, Equal<ARInvoice.refNbr>,
                And<FSEquipment.arTranLineNbr, Equal<ARTran.lineNbr>>>>>>>>>>,
            Where<InventoryItem.stkItem, Equal<True>,
                And<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
                And<SOLineSplit.pOCreate, Equal<False>,
                And<FSxEquipmentModel.eQEnabled, Equal<True>,
                And<FSxEquipmentModel.equipmentItemClass, Equal<ListField_EquipmentItemClass.ModelEquipment>,
                And<FSEquipment.refNbr, IsNull>>>>>>>))]
    #endregion
    public class SoldInventoryItem : IBqlTable
    {
        #region InvoiceRefNbr
        public abstract class invoiceRefNbr : PX.Data.BQL.BqlString.Field<invoiceRefNbr> { }

        [PXSelector(typeof(Search<ARInvoice.refNbr>))]
        [PXDBString(15, IsKey = true, IsUnicode = true, BqlField = typeof(ARInvoice.refNbr))]
        [PXUIField(DisplayName = "Invoice Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string InvoiceRefNbr { get; set; }
        #endregion
        #region InvoiceLineNbr
        public abstract class invoiceLineNbr : PX.Data.BQL.BqlInt.Field<invoiceLineNbr> { }

        [PXDBInt(IsKey = true, BqlField = typeof(ARTran.lineNbr))]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible)]
        public virtual int? InvoiceLineNbr { get; set; }
        #endregion
        #region SOLineSplitNumber
        public abstract class sOLineSplitNumber : PX.Data.BQL.BqlInt.Field<sOLineSplitNumber> { }

        [PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.splitLineNbr))]
        [PXUIField(DisplayName = "Split Line Nbr.", Visibility = PXUIVisibility.Visible)]
        public virtual int? SOLineSplitNumber { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), BqlField = typeof(ARInvoice.customerID))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [LocationID(typeof(
                        Where<
                            Location.bAccountID, Equal<Optional<customerID>>, 
                            And<Location.isActive, Equal<True>, And<MatchWithBranch<Location.cBranchID>>>>), 
                    DescriptionField = typeof(Location.descr), 
                    Visibility = PXUIVisibility.SelectorVisible,
                    BqlField = typeof(ARInvoice.customerLocationID))]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        [PXDBLocalizableString(255, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [Site(DisplayName = "Warehouse", DescriptionField = typeof(INSite.descr), BqlField = typeof(ARTran.siteID))]
        public virtual int? SiteID { get; set; }
        #endregion
        #region DocDate
        public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

        [PXDBDate(BqlField = typeof(ARInvoice.docDate))]
        [PXUIField(DisplayName = "Billing Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DocDate { get; set; }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [ARInvoiceType.List]
        [PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARInvoice.docType))]
        public virtual string DocType { get; set; }
        #endregion
        #region EquipmentTypeID
        public abstract class equipmentTypeID : PX.Data.BQL.BqlInt.Field<equipmentTypeID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Equipment Type")]
        [FSSelectorEquipmentType]
        public virtual int? EquipmentTypeID { get; set; }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        [PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
        [PXSelector(typeof(Search<INItemClass.itemClassID>), SubstituteKey = typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        [PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual int? ItemClassID { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDBIdentity(BqlField = typeof(InventoryItem.inventoryID))]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region InventoryCD
        public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }

        [InventoryRaw(DisplayName = "Inventory ID", BqlField = typeof(InventoryItem.inventoryCD))]
        [PXDefault]
        [PXFieldDescription]
        public virtual string InventoryCD { get; set; }
        #endregion

        // TODO: Rename this field to LotSerialNbr.
        // Change the label to "Lot/Serial Nbr.".
        #region LotSerialNumber
        public abstract class lotSerialNumber : PX.Data.BQL.BqlString.Field<lotSerialNumber> { }

        [PXDBString(30, IsUnicode = true, BqlField = typeof(SOLineSplit.lotSerialNbr))]
        [PXUIField(DisplayName = "Lot Serial Number", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string LotSerialNumber { get; set; }
        #endregion

        #region ShippedQty
        public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }

        [PXDBQuantity(BqlField = typeof(SOLineSplit.shippedQty))]
        [PXUIField(DisplayName = "Qty", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? ShippedQty { get; set; }
        #endregion
        #region Qty
        public abstract class qty : IBqlField
        {
        }

        [PXDBQuantity(BqlField = typeof(SOLine.orderQty))]
        [PXUIField(DisplayName = "Order Qty", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? Qty { get; set; }
        #endregion
        #region SOOrderDate
        public abstract class sOOrderDate : PX.Data.BQL.BqlDateTime.Field<sOOrderDate> { }

        [PXDBDate(BqlField = typeof(SOLine.orderDate))]
        [PXUIField(DisplayName = "Order Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? SOOrderDate { get; set; }
        #endregion
        #region SOOrderType
        public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }

        [PXDBString(2, IsFixed = true, BqlField = typeof(SOLine.orderType))]
        public virtual string SOOrderType { get; set; }
        #endregion
        #region SOOrderNbr
        public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }

        [PXDBString(15, BqlField = typeof(SOLine.orderNbr))]
        [PXUIField(DisplayName = "Order Type Nbr.", Visibility = PXUIVisibility.Visible)]
        public virtual string SOOrderNbr { get; set; }
        #endregion
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXFormula(typeof(False))]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion
    }
}
