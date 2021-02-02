using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
    [System.SerializableAttribute()]
    [PXProjection(typeof(Select5<INLocationStatus2, 
        InnerJoin<INTransitLine, On<INTransitLine.costSiteID, Equal<INLocationStatus2.locationID>>>,
        Where<INLocationStatus2.qtyOnHand, Greater<Zero>>,
    Aggregate<
        GroupBy<INTransitLine.transferNbr, 
        GroupBy<INLocationStatus2.inventoryID, 
        GroupBy<INLocationStatus2.subItemID,
            Sum<INLocationStatus2.qtyOnHand, 
            Sum<INLocationStatus2.qtyInTransit, Sum<INLocationStatus2.qtyInTransitToSO>>>>>>>>), Persistent = false)]
    public partial class INTransferLocationStatus : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<INTransferLocationStatus>.By<inventoryID, subItemID, transferNbr>
        {
            public static INTransferLocationStatus Find(PXGraph graph, int? inventoryID, int? subItemID, string transferNbr)
                => FindBy(graph, inventoryID, subItemID, transferNbr);
        }
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INTransferLocationStatus>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INTransferLocationStatus>.By<subItemID> { }
			public class ToSite : INSite.PK.ForeignKeyOf<INTransferLocationStatus>.By<toSiteID> { }
		}
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [PXDBInt(IsKey = true, BqlField = typeof(INLocationStatus2.inventoryID))]
        [PXDefault()]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [PXDBInt(IsKey = true, BqlField = typeof(INLocationStatus2.subItemID))]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region TransferNbr
        public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
        protected String _TransferNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(INTransitLine.transferNbr), IsKey = true)]
        public virtual String TransferNbr
        {
            get
            {
                return this._TransferNbr;
            }
            set
            {
                this._TransferNbr = value;
            }
        }
        #endregion
        #region QtyOnHand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        protected Decimal? _QtyOnHand;
        [PXDBQuantity(BqlField = typeof(INLocationStatus2.qtyOnHand))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? QtyOnHand
        {
            get
            {
                return this._QtyOnHand;
            }
            set
            {
                this._QtyOnHand = value;
            }
        }
        #endregion
        #region ToSiteID
        public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
        protected Int32? _ToSiteID;
        [IN.ToSite(DisplayName = "To Warehouse ID", DescriptionField = typeof(INSite.descr), BqlField = typeof(INTransitLine.toSiteID))]
        public virtual Int32? ToSiteID
        {
            get
            {
                return this._ToSiteID;
            }
            set
            {
                this._ToSiteID = value;
            }
        }
        #endregion
    }



    [System.SerializableAttribute()]
    [PXProjection(typeof(Select5<INTransferLocationStatus,
    InnerJoin<INCostStatus, 
        On<INCostStatus.receiptNbr, Equal<INTransferLocationStatus.transferNbr>,
            And<INCostStatus.inventoryID, Equal<INTransferLocationStatus.inventoryID>>>,
    InnerJoin<INCostSubItemXRef, 
        On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>,
            And<INCostSubItemXRef.subItemID, Equal<INTransferLocationStatus.subItemID>>>,
    CrossJoin<CS.CommonSetup>>>,
Aggregate<
    GroupBy<INCostStatus.receiptNbr,
    GroupBy<INCostStatus.inventoryID, 
    GroupBy<INCostSubItemXRef.subItemID,
        Sum<INCostStatus.qtyOnHand, 
        Sum<INCostStatus.totalCost>>>>>>>), Persistent = false)]
    [PXPrimaryGraph(new Type[] { typeof(INTransferEntry) }, new Type[] {
        typeof(Select<INRegister,
            Where<INRegister.docType, Equal<INDocType.transfer>, And<INRegister.refNbr, Equal<Current<INTransferStatus.transferNbr>>>>>) } ) ]
    public partial class INTransferStatus : PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<INTransferStatus>.By<inventoryID, subItemID, transferNbr>
        {
            public static INTransferStatus Find(PXGraph graph, int? inventoryID, int? subItemID, string transferNbr)
                => FindBy(graph, inventoryID, subItemID, transferNbr);
        }
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INTransferStatus>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INTransferStatus>.By<subItemID> { }
			public class ToSite : INSite.PK.ForeignKeyOf<INTransferStatus>.By<toSiteID> { }
		}
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [PXDefault()]
        [StockItem(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [SubItem(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region TransferNbr
        public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
        protected String _TransferNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(INCostStatus.receiptNbr), IsKey = true)]
        [PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<INDocType.transfer>>>))]
        public virtual String TransferNbr
        {
            get
            {
                return this._TransferNbr;
            }
            set
            {
                this._TransferNbr = value;
            }
        }
        #endregion
        #region QtyOnHand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        protected Decimal? _QtyOnHand;
        [PXDBQuantity(BqlField = typeof(INCostStatus.qtyOnHand))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? QtyOnHand
        {
            get
            {
                return this._QtyOnHand;
            }
            set
            {
                this._QtyOnHand = value;
            }
        }
        #endregion
        #region TotalCost
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
        protected Decimal? _TotalCost;
        [CM.PXDBBaseCury(BqlField = typeof(INCostStatus.totalCost))]
        public virtual Decimal? TotalCost
        {
            get
            {
                return this._TotalCost;
            }
            set
            {
                this._TotalCost = value;
            }
        }
        #endregion
        #region DecPlPrcCst
        public abstract class decPlPrcCst : PX.Data.BQL.BqlShort.Field<decPlPrcCst> { }
        protected Int16? _DecPlPrcCst;
        [PXDBShort(BqlField = typeof(CS.CommonSetup.decPlPrcCst))]
        public virtual Int16? DecPlPrcCst
        {
            get
            {
                return this._DecPlPrcCst;
            }
            set
            {
                this._DecPlPrcCst = value;
            }
        }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        protected Decimal? _UnitCost;
        [PXPriceCost()]
        public virtual Decimal? UnitCost
        {
            [PXDependsOnFields(typeof(qtyOnHand), typeof(totalCost), typeof(decPlPrcCst))]
            get
            {
                return (this.QtyOnHand == null || this.TotalCost == null) ? (decimal?)null : (this.QtyOnHand != 0m) ? Math.Round((decimal)this.TotalCost / (decimal)this.QtyOnHand, (int)this.DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
            }
            set
            {
            }
        }
        #endregion
        #region ToSiteID
        public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
        protected Int32? _ToSiteID;
        [IN.ToSite(DisplayName = "To Warehouse ID", DescriptionField = typeof(INSite.descr), BqlField = typeof(INTransferLocationStatus.toSiteID))]
        public virtual Int32? ToSiteID
        {
            get
            {
                return this._ToSiteID;
            }
            set
            {
                this._ToSiteID = value;
            }
        }
        #endregion
    }
}
