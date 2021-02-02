using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXPrimaryGraph(typeof(ServiceOrderEntry))]
    [PXProjection(typeof(
        Select2<FSSODet,
            InnerJoin<FSServiceOrder,
                On<FSServiceOrder.sOID, Equal<FSSODet.sOID>>,
            InnerJoin<InventoryItem,
                On<InventoryItem.inventoryID, Equal<FSSODet.inventoryID>>,
            LeftJoin<INItemClass,
                On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>,
            LeftJoin<INSite, 
                On<INSite.siteID, Equal<FSSODet.siteID>>>>>>,
        Where<
            FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
            And<FSSODet.status, NotEqual<FSSODet.status.Canceled>,
            And<FSSODet.enablePO, Equal<True>,
            And<FSSODet.poNbr, IsNull,
            And<
                Where<
                    InventoryItem.stkItem, Equal<True>, 
                    Or<
                        Where<
                            InventoryItem.nonStockShip, Equal<True>,
                            Or<InventoryItem.nonStockReceipt, Equal<True>>>>>>>>>>>))]
    public class POEnabledFSSODet : FSSODet
    {
        #region BranchID
        public abstract class srvBranchID : PX.Data.BQL.BqlInt.Field<srvBranchID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.branchID))]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? SrvBranchID { get; set; }
        #endregion
        #region SrvBranchLocationID
        public abstract class srvBranchLocationID : PX.Data.BQL.BqlInt.Field<srvBranchLocationID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.branchLocationID))]
        [PXUIField(DisplayName = "Branch Location ID")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<POEnabledFSSODet.srvBranchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? SrvBranchLocationID { get; set; }
        #endregion
        #region InventoryItemClassID
        public abstract class inventoryItemClassID : PX.Data.BQL.BqlInt.Field<inventoryItemClassID> { }

        [PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
        [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<INItemClass.itemClassID,
                    Where<
                        INItemClass.itemType, Equal<INItemTypes.serviceItem>,
                        Or<FeatureInstalled<FeaturesSet.distributionModule>>>>),
                SubstituteKey = typeof(INItemClass.itemClassCD))]
        public virtual int? InventoryItemClassID { get; set; }
        #endregion
        #region OrderDate
        public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }

        [PXDBDate(BqlField = typeof(FSServiceOrder.orderDate))]
        [PXUIField(DisplayName = "Requested on", Visibility = PXUIVisibility.SelectorVisible)]
        public override DateTime? OrderDate { get; set; }
        #endregion
        #region SrvCustomerID
        public abstract class srvCustomerID : PX.Data.BQL.BqlInt.Field<srvCustomerID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.customerID))]
        [PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<BAccountSelectorBase.status, IsNull,
                Or<BAccountSelectorBase.status, Equal<BAccount.status.active>,
                Or<BAccountSelectorBase.status, Equal<BAccount.status.oneTime>>>>),
                PX.Objects.AR.Messages.CustomerIsInStatus, typeof(BAccountSelectorBase.status))]
        [FSSelectorBusinessAccount_CU_PR_VC]
        public virtual int? SrvCustomerID { get; set; }
        #endregion
        #region SrvLocationID
        public abstract class srvLocationID : PX.Data.BQL.BqlInt.Field<srvLocationID> { }

        [LocationID(
            typeof(Where<Location.bAccountID, Equal<Current<POEnabledFSSODet.srvCustomerID>>>), 
            BqlField = typeof(FSServiceOrder.locationID), 
            DescriptionField = typeof(Location.descr), 
            DisplayName = "Location ID")]
        public virtual int? SrvLocationID { get; set; }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
        [PXDBDefault(typeof(FSServiceOrder.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSServiceOrder,
                            Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODet.srvOrdType>>,
                                And<FSServiceOrder.refNbr, Equal<Current<FSSODet.refNbr>>>>>))]
        public override string RefNbr { get; set; }
        #endregion
        #region SrvOrdType
        public new abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Enabled = false)]
        [PXDefault(typeof(FSServiceOrder.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public override string SrvOrdType { get; set; }
        #endregion
        #region PONbrCreated
        public abstract class poNbrCreated : PX.Data.BQL.BqlString.Field<poNbrCreated> { }

        [PXString]
        [PXUIField(DisplayName = "PO Nbr.")]
        [PO.PO.RefNbr(typeof(
            Search2<POOrder.orderNbr,
            LeftJoinSingleTable<Vendor,
                On<POOrder.vendorID, Equal<Vendor.bAccountID>,
                And<Match<Vendor, Current<AccessInfo.userName>>>>>,
            Where<
                POOrder.orderType, Equal<POOrderType.regularOrder>,
                And<Vendor.bAccountID, IsNotNull>>,
            OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
        public virtual string PONbrCreated { get; set; }
        #endregion
        #region POVendorID
        public new abstract class poVendorID : PX.Data.BQL.BqlInt.Field<poVendorID> { }

        [VendorNonEmployeeActive(DisplayName = "Vendor ID", DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
        public override int? POVendorID { get; set; }
        #endregion
        #region POVendorLocationID

        [PXFormula(typeof(Default<FSSODet.poVendorID>))]
        [PXDefault(typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<FSSODet.poVendorID>>>>),
                PersistingCheck = PXPersistingCheck.Nothing)]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSSODet.poVendorID>>>),
                DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Vendor Location ID", Visible = true)]
        public override int? POVendorLocationID { get; set; }
        #endregion
        #region CuryUnitCost
        public abstract class srvCuryUnitCost : PX.Data.BQL.BqlDecimal.Field<srvCuryUnitCost> { }

        [PXDBDecimal(BqlField = typeof(FSSODet.curyUnitCost))]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? SrvCuryUnitCost { get; set; }
        #endregion
    }
}
