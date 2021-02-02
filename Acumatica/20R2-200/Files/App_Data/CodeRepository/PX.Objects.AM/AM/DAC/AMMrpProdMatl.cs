using System;
using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select2<
        AMProdMatl,
        InnerJoin<AMProdItem, 
            On<AMProdMatl.orderType, Equal<AMProdItem.orderType>,
            And<AMProdMatl.prodOrdID, Equal<AMProdItem.prodOrdID>>>,
        InnerJoin<AMProdMatlSplit,
            On<AMProdMatl.orderType, Equal<AMProdMatlSplit.orderType>,
            And<AMProdMatl.prodOrdID, Equal<AMProdMatlSplit.prodOrdID>,
            And<AMProdMatl.operationID, Equal<AMProdMatlSplit.operationID>,
            And<AMProdMatl.lineID, Equal<AMProdMatlSplit.lineID>>>>>>>>), Persistent = false)]
    public class AMMrpProdMatl : IBqlTable, INotable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false, BqlField = typeof(AMProdMatl.orderType))]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false, BqlField = typeof(AMProdMatl.prodOrdID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(IsKey = true, Visible = false, Enabled = false, BqlField = typeof(AMProdMatl.operationID))]
        [PXDBDefault(typeof(AMProdOper.operationID))]
        public virtual int? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
        }
        #endregion
        #region LineID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;
        [PXDBInt(IsKey = true, BqlField = typeof(AMProdMatl.lineID))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual Int32? LineID
        {
            get
            {
                return this._LineID;
            }
            set
            {
                this._LineID = value;
            }
        }
        #endregion
        #region Function
        /// <summary>
        /// Production order function
        /// </summary>
        public abstract class function : PX.Data.BQL.BqlInt.Field<function> { }

        protected int? _Function;
        /// <summary>
        /// Production order function
        /// </summary>
        [PXDBInt(BqlField = typeof(AMProdItem.function))]
        [PXUIField(DisplayName = "Function", Enabled = false, Visible = false)]
        [OrderTypeFunction.List]
        public virtual int? Function
        {
            get
            {
                return this._Function;
            }
            set
            {
                this._Function = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDBInt(BqlField = typeof(AMProdMatl.inventoryID))]
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
        [PXDBInt(BqlField = typeof(AMProdMatl.subItemID))]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXDBGuid(BqlField = typeof(AMProdMatl.noteID))]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXDBInt(BqlField = typeof(AMProdMatl.siteID))]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [PXDBInt(BqlField = typeof(AMProdMatl.locationID))]
        public virtual Int32? LocationID
        {
            get
            {
                return this._LocationID;
            }
            set
            {
                this._LocationID = value;
            }
        }
        #endregion
        #region CompBOMID
        public abstract class compBOMID : PX.Data.BQL.BqlString.Field<compBOMID> { }

        protected String _CompBOMID;
        [BomID(Visible = false, DisplayName = "Comp BOM ID", BqlField = typeof(AMProdMatl.compBOMID))]

        public virtual String CompBOMID
        {
            get
            {
                return this._CompBOMID;
            }
            set
            {
                this._CompBOMID = value;
            }
        }
        #endregion
        
        #region ProdItemInventoryID (AMProdItem.inventoryID)
        public abstract class prodItemInventoryID : PX.Data.BQL.BqlInt.Field<prodItemInventoryID> { }

        [PXDBInt(BqlField = typeof(AMProdItem.inventoryID))]
        [PXUIField(DisplayName = "Production Inventory ID")]
        public virtual Int32? ProdItemInventoryID { get; set; }
        #endregion
        #region ProdItemSubItemID (AMProdItem.subItemID)
        public abstract class prodItemsubItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [PXDBInt(BqlField = typeof(AMProdItem.subItemID))]
        public virtual Int32? ProdItemSubItemID { get; set; }
        #endregion
        #region ExcludeFromMRP
        public abstract class excludeFromMRP : PX.Data.BQL.BqlBool.Field<excludeFromMRP> { }

        protected Boolean? _ExcludeFromMRP;
        [PXDBBool(BqlField = typeof(AMProdItem.excludeFromMRP))]
        [PXUIField(DisplayName = "Exclude from MRP")]
        public virtual Boolean? ExcludeFromMRP
        {
            get
            {
                return this._ExcludeFromMRP;
            }
            set
            {
                this._ExcludeFromMRP = value;
            }
        }
        #endregion

        #region StatusID
        public abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }
        protected String _StatusID;
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMProdItem.statusID))]
        [PXUIField(DisplayName = "Status", Visible = false, Enabled = false)]
        [ProductionOrderStatus.List]
        public virtual String StatusID
        {
            get
            {
                return this._StatusID;
            }
            set
            {
                this._StatusID = value;
            }
        }
        #endregion
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
        protected Int64? _PlanID;
        [PXDBLong(IsImmutable = true, BqlField = typeof(AMProdMatlSplit.planID))]
        [PXUIField(DisplayName = "Plan ID", Visible = false, Enabled = false)]
        public virtual Int64? PlanID
        {
            get
            {
                return this._PlanID;
            }
            set
            {
                this._PlanID = value;
            }
        }
        #endregion
    }
}