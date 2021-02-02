using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.VendorShipmentLine)]
    public class AMVendorShipLine : IBqlTable, INotable, ILSPrimary, IProdOper
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMVendorShipLine>.By<shipmentNbr, lineNbr>
        {
            public static AMVendorShipLine Find(PXGraph graph, string shipmentNbr, int? lineNbr)
                => FindBy(graph, shipmentNbr, lineNbr);
        }

        public static class FK
        {
            public class Shipment : AMVendorShipment.PK.ForeignKeyOf<AMVendorShipLine>.By<shipmentNbr> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMVendorShipLine>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMVendorShipLine>.By<siteID> { }
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMVendorShipLine>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMVendorShipLine>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMVendorShipLine>.By<orderType, prodOrdID, operationID> { }
        }
        #endregion

        #region ShipmentNbr
        public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
        protected String _ShipmentNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDBDefault(typeof(AMVendorShipment.shipmentNbr))]
        [PXUIField(DisplayName = "Shipment Nbr.", Visible = false, Enabled = false)]
        [PXParent(typeof(Select<AMVendorShipment,
            Where<AMVendorShipment.shipmentNbr, Equal<Current<AMVendorShipLine.shipmentNbr>>>>))]
        public virtual String ShipmentNbr
        {
            get
            {
                return this._ShipmentNbr;
            }
            set
            {
                this._ShipmentNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXLineNbr(typeof(AMVendorShipment.lineCntr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region LineType
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
        protected String _LineType;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(AMShipLineType.WIP)]
        [AMShipLineType.List]
        [PXUIField(DisplayName = "Type")]
        public virtual String LineType
        {
            get
            {
                return this._LineType;
            }
            set
            {
                this._LineType = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
        [AMOrderTypeField]
        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        [PXFormula(typeof(Selector<prodOrdID, AMProdItem.orderType>))]
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
        #region TranType

        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [AMTranType.List]
        [PXDefault(AMTranType.Receipt)]
        [PXUIField(DisplayName = "Tran. Type", Enabled = false, Visible = false)]
        public virtual String TranType
        {
            get
            {
                return this._TranType;
            }
            set
            {
                this._TranType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr]
        [PXDefault]
        [ProductionOrderSelector(typeof(AMVendorShipLine.orderType))]
        [PXFormula(typeof(Validate<AMVendorShipLine.orderType>))]
        [PXRestrictor(typeof(Where<AMProdItem.hold, NotEqual<True>,
            And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>),
            Messages.ProdStatusInvalidForProcess, typeof(AMProdItem.orderType), typeof(AMProdItem.prodOrdID), typeof(AMProdItem.statusID))]
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
        [OperationIDField]
        [PXDefault]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<AMVendorShipLine.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<AMVendorShipLine.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        [PXFormula(typeof(Default<AMVendorShipLine.prodOrdID>))]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDefault]
        [Inventory]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMVendorShipLine.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(
            typeof(AMVendorShipLine.inventoryID),
            typeof(LeftJoin<INSiteStatus,
                On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
                And<INSiteStatus.inventoryID, Equal<Optional<AMVendorShipLine.inventoryID>>,
                And<INSiteStatus.siteID, Equal<Optional<AMVendorShipLine.siteID>>>>>>))]
        [PXFormula(typeof(Default<AMVendorShipLine.inventoryID>))]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXDefault(typeof(Search<AMProdItem.siteID, Where<AMProdItem.orderType, Equal<Current<AMVendorShipLine.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMVendorShipLine.prodOrdID>>>>>))]
        [SiteAvail(typeof(AMVendorShipLine.inventoryID), typeof(AMVendorShipLine.subItemID))]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
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
        [MfgLocationAvail(typeof(AMVendorShipLine.inventoryID), typeof(AMVendorShipLine.subItemID), typeof(AMVendorShipLine.siteID), false, true)]
        [PXForeignReference(typeof(CompositeKey<Field<siteID>.IsRelatedTo<INLocation.siteID>, Field<locationID>.IsRelatedTo<INLocation.locationID>>))]
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        protected String _UOM;
        [PXDefault(typeof(Switch<
            Case<Where<Current<AMVendorShipLine.lineType>, Equal<AMShipLineType.wIP>>,
                Selector<AMVendorShipLine.prodOrdID, AMProdItem.uOM>,
            Case<Where<Current<AMVendorShipLine.lineType>, Equal<AMShipLineType.material>, And<Current<AMVendorShipLine.matlLineID>, IsNotNull>>,
                Selector<AMVendorShipLine.matlLineID, AMProdMatl.uOM>>>,
            Selector<AMVendorShipLine.inventoryID, InventoryItem.baseUnit>>))]
        //[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<AMMTran.inventoryID>>>>))]
        [PXFormula(typeof(Default<AMVendorShipLine.inventoryID, AMVendorShipLine.matlLineID>))]
        [INUnit(typeof(AMVendorShipLine.inventoryID))]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity(typeof(uOM), typeof(baseQty), HandleEmptyKey = true, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity")]
        [PXFormula(null, typeof(SumCalc<AMVendorShipment.shipmentQty>))]
        public virtual Decimal? Qty
        {
            get
            {
                return this._Qty;
            }
            set
            {
                this._Qty = value;
            }
        }
        #endregion
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty")]
        public virtual Decimal? BaseQty
        {
            get
            {
                return this._BaseQty;
            }
            set
            {
                this._BaseQty = value;
            }
        }
        #endregion
        #region LotSerCntr
        public abstract class lotSerCntr : PX.Data.BQL.BqlInt.Field<lotSerCntr> { }

        protected Int32? _LotSerCntr;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "LotSerCntr")]
        public virtual Int32? LotSerCntr
        {
            get
            {
                return this._LotSerCntr;
            }
            set
            {
                this._LotSerCntr = value;
            }
        }
        #endregion
        #region LotSerialNbr

        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [AMLotSerialNbr(typeof(inventoryID), typeof(subItemID), typeof(locationID),
            PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String LotSerialNbr
        {
            get
            {
                return this._LotSerialNbr;
            }
            set
            {
                this._LotSerialNbr = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByte.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region UnassignedQty

        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        protected Decimal? _UnassignedQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnassignedQty
        {
            get
            {
                return this._UnassignedQty;
            }
            set
            {
                this._UnassignedQty = value;
            }
        }
        #endregion
        #region HasMixedProjectTasks
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

        protected bool? _HasMixedProjectTasks;
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXBool]
        [PXFormula(typeof(False))]
        public virtual bool? HasMixedProjectTasks
        {
            get
            {
                return _HasMixedProjectTasks;
            }
            set
            {
                _HasMixedProjectTasks = value;
            }
        }
        #endregion
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate]
        [PXDBDefault(typeof(AMVendorShipment.shipmentDate))]
        public virtual DateTime? TranDate
        {
            get
            {
                return this._TranDate;
            }
            set
            {
                this._TranDate = value;
            }
        }
        #endregion
        #region InvtMult

        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Multiplier")]
        public virtual Int16? InvtMult
        {
            get
            {
                return this._InvtMult;
            }
            set
            {
                this._InvtMult = value;
            }
        }
        #endregion
        #region ExpireDate

        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(inventoryID), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
        public virtual DateTime? ExpireDate
        {
            get
            {
                return this._ExpireDate;
            }
            set
            {
                this._ExpireDate = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        [ProjectBase]
        [ProjectDefault(BatchModule.IN, typeof(Search<AMProdItem.projectID, Where<AMProdItem.orderType, Equal<Current<orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>))]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PX.Objects.PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PX.Objects.PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.visibleInIN, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
        [PXFormula(typeof(Default<prodOrdID>))]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        protected Int32? _TaskID;
        [PXDefault(typeof(Search<AMProdItem.taskID, Where<AMProdItem.orderType, Equal<Current<orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<projectID>))]
        [BaseProjectTask(typeof(projectID))]
        public virtual Int32? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected Boolean? _Released;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Released", Visible = false, Enabled = false)]
        public virtual Boolean? Released
        {
            get
            {
                return this._Released;
            }
            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region MatlLineID
        public abstract class matlLineID : PX.Data.BQL.BqlInt.Field<matlLineID> { }

        protected Int32? _MatlLineID;
        [PXDBInt]
        [PXUIField(DisplayName = "Material Line Nbr.", Visible = false)]
        [PXSelector(typeof(Search<AMProdMatl.lineID,
            Where<AMProdMatl.orderType, Equal<Current<orderType>>,
                And<AMProdMatl.prodOrdID, Equal<Current<prodOrdID>>,
                And<AMProdMatl.operationID, Equal<Current<operationID>>,
                And<AMProdMatl.inventoryID, Equal<Current<inventoryID>>,
                And<Where<AMProdMatl.subItemID, Equal<Current<subItemID>>,
                    Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>>>>>>>),
            typeof(AMProdMatl.lineNbr),
            typeof(AMProdMatl.inventoryID),
            typeof(AMProdMatl.subItemID),
            typeof(AMProdMatl.statusID),
            typeof(AMProdMatl.qtyRemaining),
            typeof(AMProdMatl.uOM),
            typeof(AMProdMatl.descr))]
        public virtual Int32? MatlLineID
        {
            get
            {
                return this._MatlLineID;
            }
            set
            {
                this._MatlLineID = value;
            }
        }
        #endregion
        #region POOrderNbr
        public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }
        protected String _POOrderNbr;

        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "PO Order Nbr.", Visible = false, Enabled = false)]
        public virtual String POOrderNbr
        {
            get
            {
                return this._POOrderNbr;
            }
            set
            {
                this._POOrderNbr = value;
            }
        }
        #endregion
        #region POLineNbr
        public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
        protected Int32? _POLineNbr;
        [PXDBInt]
        [PXUIField(DisplayName = "PO Line Nbr.", Visible = false, Enabled = false)]
        public virtual Int32? POLineNbr
        {
            get
            {
                return this._POLineNbr;
            }
            set
            {
                this._POLineNbr = value;
            }
        }
        #endregion
        #region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        protected String _TranDesc;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Tran Description", Visible = false)]
        public virtual String TranDesc
        {
            get
            {
                return this._TranDesc;
            }
            set
            {
                this._TranDesc = value;
            }
        }
        #endregion
        #region Methods

        public static implicit operator AMVendorShipLineSplit(AMVendorShipLine item)
        {
            AMVendorShipLineSplit ret = new AMVendorShipLineSplit();
            ret.ShipmentNbr = item.ShipmentNbr;
            ret.TranType = item.TranType;
            ret.LineNbr = item.LineNbr;
            ret.SplitLineNbr = (int)1;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;
            ret.ExpireDate = item.ExpireDate;
            ret.Qty = Math.Abs(item.Qty.GetValueOrDefault());
            ret.BaseQty = Math.Abs(item.BaseQty.GetValueOrDefault());
            ret.UOM = item.UOM;
            ret.TranDate = item.TranDate;
            ret.InvtMult = item.InvtMult;
            ret.Released = item.Released;

            return ret;
        }
        public static implicit operator AMVendorShipLine(AMVendorShipLineSplit item)
        {
            AMVendorShipLine ret = new AMVendorShipLine();
            ret.ShipmentNbr = item.ShipmentNbr;
            ret.TranType = item.TranType;
            ret.LineNbr = item.LineNbr;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;

            //Split recs show a positive qty - ammtran shows a positive or negative qty
            ret.Qty = Math.Abs(item.Qty.GetValueOrDefault()) * (item.InvtMult * -1);
            ret.BaseQty = Math.Abs(item.BaseQty.GetValueOrDefault()) * (item.InvtMult * -1);

            ret.UOM = item.UOM;
            ret.TranDate = item.TranDate;
            ret.InvtMult = item.InvtMult;
            ret.Released = item.Released;

            return ret;
        }
        #endregion
    }
}