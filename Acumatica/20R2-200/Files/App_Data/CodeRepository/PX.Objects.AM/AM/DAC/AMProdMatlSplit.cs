using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.AM.Attributes;
using System.Text;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    [PXCacheName(Messages.ProductionMatlSplit)]
    public class AMProdMatlSplit : IBqlTable, ILSDetail, IProdOper
    {
#if DEBUG
        //  DEVELOPER NOTE: adding bound fields to this dac might require modifying PXProjection AMProdMatlSplit2
#endif
        internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}] OperationID = {OperationID}, LineID = {LineID}, SplitLineNbr = {SplitLineNbr}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdMatlSplit>.By<orderType, prodOrdID, operationID, lineID, splitLineNbr>
        {
            public static AMProdMatlSplit Find(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID, int? splitLineNbr) 
                => FindBy(graph, orderType, prodOrdID, operationID, lineID, splitLineNbr);

            public static AMProdMatlSplit FindDirty(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID, int? splitLineNbr)
                => PXSelect<AMProdMatlSplit,
                    Where<orderType, Equal<Required<orderType>>,
                        And<prodOrdID, Equal<Required<prodOrdID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>,
                        And<splitLineNbr, Equal<Required<splitLineNbr>>>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, operationID, lineID, splitLineNbr);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdMatlSplit>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdMatlSplit>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMProdMatlSplit>.By<orderType, prodOrdID, operationID> { }
            public class Material : AMProdMatl.PK.ForeignKeyOf<AMProdMatlSplit>.By<orderType, prodOrdID, operationID, lineID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMProdMatlSplit>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMProdMatlSplit>.By<siteID> { }
        }

        #endregion

        #region Selected (select check box)
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected", Enabled = true)]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdMatl.orderType))]
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
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false)]
        [ProductionOrderSelector(typeof(AMProdMatlSplit.orderType), true, ValidateValue = false)]
        [PXDBDefault(typeof(AMProdMatl.prodOrdID))]
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
        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdMatl.operationID))]
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
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdMatl.lineID))]
        [PXParent(typeof(Select<AMProdMatl,
            Where<AMProdMatl.orderType, Equal<Current<AMProdMatlSplit.orderType>>,
            And<AMProdMatl.prodOrdID, Equal<Current<AMProdMatlSplit.prodOrdID>>,
            And<AMProdMatl.operationID, Equal<Current<AMProdMatlSplit.operationID>>,
            And<AMProdMatl.lineID, Equal<Current<AMProdMatlSplit.lineID>>>>>>>))]
        [PXParent(typeof(Select<AMProdOper,
            Where<AMProdOper.orderType, Equal<Current<AMProdMatlSplit.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMProdMatlSplit.prodOrdID>>,
                    And<AMProdOper.operationID, Equal<Current<AMProdMatlSplit.operationID>>>>>>))]
        [PXParent(typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdMatlSplit.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdMatlSplit.prodOrdID>>>>>))]
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
        #region SplitLineNbr
        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(AMProdMatl.splitLineCntr))]
        [PXUIField(DisplayName = "Allocation ID", Enabled = false, Visible = false)]
        public virtual Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
            }
        }
        #endregion
        #region InventoryID

        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        //Cannot use AnyInventoryAttribute as this causes LSSelect class to fail with Obj Ref Error
        [Inventory(Enabled = false)]
        [PXDefault(typeof(AMProdMatl.inventoryID))]
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
        [SubItem(typeof(AMProdMatlSplit.inventoryID), Visible = false)]
        [PXDefault(typeof(AMProdMatl.subItemID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        [Site(DisplayName = "Alloc. Warehouse")]
        [PXDefault(typeof(AMProdMatl.siteID))]
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
        [Location(typeof(AMProdMatlSplit.siteID), Enabled = false, Visible = false)]
        [PXDefault(typeof(AMProdMatl.locationID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        [INUnit(typeof(AMProdMatlSplit.inventoryID), Enabled = false)]
        [PXDefault(typeof(AMProdMatl.uOM))]
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
        [PXDBQuantity(typeof(AMProdMatlSplit.uOM), typeof(AMProdMatlSplit.baseQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity", Enabled = false)]
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
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        protected Int64? _PlanID;
        [PXDBLong(IsImmutable = true)]
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
        #region AssignedNbr
        public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr>{ }

        protected String _AssignedNbr;
        [PXString(30, IsUnicode = true)]
        public virtual String AssignedNbr
        {
            get
            {
                return this._AssignedNbr;
            }
            set
            {
                this._AssignedNbr = value;
            }
        }
        #endregion
        #region LotSerClassID
        public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

        protected String _LotSerClassID;
        [PXString(10, IsUnicode = true)]
        public virtual String LotSerClassID
        {
            get
            {
                return this._LotSerClassID;
            }
            set
            {
                this._LotSerClassID = value;
            }
        }
        #endregion
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Stock Item")]
        [PXFormula(typeof(Selector<AMProdMatlSplit.inventoryID, InventoryItem.stkItem>))]
        public bool? IsStockItem
        {
            get;
            set;
        }
        #endregion
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXDefault(INTranType.Issue)]
        [INTranType.List]
        [PXUIField(DisplayName = "Tran. Type")]
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
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate]
        [PXDefault(typeof(AMProdMatl.tranDate))]
        [PXUIField(DisplayName = "Allocation Date")]
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
        [PXDefault((short)1)]
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
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [AMProdMatlLotSerialNbr(typeof(AMProdMatlSplit.inventoryID), typeof(AMProdMatlSplit.subItemID), typeof(AMProdMatlSplit.locationID), 
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
        #region ExpireDate
        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(AMProdMatlSplit.inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXInt]
        [PXUIField(DisplayName = "Project", Visible = false, Enabled = false)]
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
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        protected Int32? _TaskID;
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXInt]
        [PXUIField(DisplayName = "Task", Visible = false, Enabled = false)]
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
        #region IsAllocated

        public abstract class isAllocated : PX.Data.BQL.BqlBool.Field<isAllocated> { }

        protected Boolean? _IsAllocated;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allocated")]
        public virtual Boolean? IsAllocated
        {
            get
            {
                return this._IsAllocated;
            }
            set
            {
                this._IsAllocated = value;
            }
        }
        #endregion
        #region CostSubItemID

        public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }

        protected Int32? _CostSubItemID;
        [PXInt]
        public virtual Int32? CostSubItemID
        {
            get
            {
                return this._CostSubItemID;
            }
            set
            {
                this._CostSubItemID = value;
            }
        }
        #endregion
        #region CostSiteID

        public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }

        protected Int32? _CostSiteID;
        [PXInt]
        public virtual Int32? CostSiteID
        {
            get
            {
                return this._CostSiteID;
            }
            set
            {
                this._CostSiteID = value;
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

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

        #region Allocation Fields
        #region QtyComplete
        public abstract class qtyComplete : PX.Data.BQL.BqlDecimal.Field<qtyComplete> { }

        protected Decimal? _QtyComplete;
        [PXDBQuantity(typeof(AMProdMatlSplit.uOM), typeof(AMProdMatlSplit.baseQtyComplete), MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Complete", Enabled = false)]
        public virtual Decimal? QtyComplete
        {
            get
            {
                return this._QtyComplete;
            }
            set
            {
                this._QtyComplete = value;
            }
        }
        #endregion
        #region BaseQtyComplete
        public abstract class baseQtyComplete : PX.Data.BQL.BqlDecimal.Field<baseQtyComplete> { }

        protected Decimal? _BaseQtyComplete;
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseQtyComplete
        {
            get
            {
                return this._BaseQtyComplete;
            }
            set
            {
                this._BaseQtyComplete = value;
            }
        }
        #endregion
        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        protected Boolean? _Completed;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public virtual Boolean? Completed
        {
            get
            {
                return this._Completed;
            }
            set
            {
                this._Completed = value;
            }
        }
        #endregion
        #region ParentSplitLineNbr
        public abstract class parentSplitLineNbr : PX.Data.BQL.BqlInt.Field<parentSplitLineNbr> { }

        protected Int32? _ParentSplitLineNbr;
        [PXDBInt]
        [PXUIField(DisplayName = "Parent Allocation ID", Visible = false, IsReadOnly = true)]
        public virtual Int32? ParentSplitLineNbr
        {
            get
            {
                return this._ParentSplitLineNbr;
            }
            set
            {
                this._ParentSplitLineNbr = value;
            }
        }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        public class PXRefNoteAttribute : PX.Data.PXRefNoteAttribute
        {
            public PXRefNoteAttribute()
                : base()
            {
            }

            public class PXLinkState : PXStringState
            {
                protected object[] _keys;
                protected Type _target;

                public object[] keys
                {
                    get { return _keys; }
                }

                public Type target
                {
                    get { return _target; }
                }

                public PXLinkState(object value)
                    : base(value)
                {
                }

                public static PXFieldState CreateInstance(object value, Type target, object[] keys)
                {
                    PXLinkState state = value as PXLinkState;
                    if (state == null)
                    {
                        PXFieldState field = value as PXFieldState;
                        if (field != null && field.DataType != typeof(object) && field.DataType != typeof(string))
                        {
                            return field;
                        }
                        state = new PXLinkState(value);
                    }
                    if (target != null)
                    {
                        state._target = target;
                    }
                    if (keys != null)
                    {
                        state._keys = keys;
                    }

                    return state;
                }
            }

            public override void CacheAttached(PXCache sender)
            {
                base.CacheAttached(sender);

                PXButtonDelegate del = delegate (PXAdapter adapter)
                {
                    PXCache cache = adapter.View.Graph.Caches[typeof(AMProdMatlSplit)];
                    if (cache.Current != null)
                    {
                        object val = cache.GetValueExt(cache.Current, _FieldName);

                        PXLinkState state = val as PXLinkState;
                        if (state != null)
                        {
                            helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
                        }
                        else
                        {
                            helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, _FieldName), PXRedirectHelper.WindowMode.NewWindow);
                        }
                    }

                    return adapter.Get();
                };

                string ActionName = sender.GetItemType().Name + "$" + _FieldName + "$Link";
                sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(typeof(AMProdItem)), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
            }

            public virtual object GetEntityRowID(PXCache cache, object[] keys)
            {
                return GetEntityRowID(cache, keys, ", ");
            }

            public static object GetEntityRowID(PXCache cache, object[] keys, string separator)
            {
                StringBuilder result = new StringBuilder();
                int i = 0;
                foreach (string key in cache.Keys)
                {
                    if (i >= keys.Length) break;
                    object val = keys[i++];
                    cache.RaiseFieldSelecting(key, null, ref val, true);

                    if (val != null)
                    {
                        if (result.Length != 0) result.Append(separator);
                        result.Append(val.ToString().TrimEnd());
                    }
                }
                return result.ToString();
            }

            public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
            {
                var row = (AMProdMatlSplit)e.Row;
                if (row == null)
                {
                    base.FieldSelecting(sender, e);
                    return;
                }

                // Put receipt before PO as the receipt line also as the PO (for improved usage in Unreleased Allocation process)
                if (!string.IsNullOrEmpty(row.POReceiptNbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches<POReceipt>(), new object[] { row.POReceiptType, row.POReceiptNbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(POReceipt), new object[] { row.POReceiptType, row.POReceiptNbr });
                    return;
                }

                if (!string.IsNullOrEmpty(row.POOrderNbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches<POOrder>(), new object[] { row.POOrderType, row.POOrderNbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(POOrder), new object[] { row.POOrderType, row.POOrderNbr });
                    return;
                }

                if (!string.IsNullOrEmpty(row.AMBatNbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches<AMBatch>(), new object[] { row.AMDocType, row.AMBatNbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(AMBatch), new object[] { row.AMDocType, row.AMBatNbr });
                    return;
                }

                if (!string.IsNullOrEmpty(row.AMProdOrdID))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches<AMProdItem>(), new object[] { row.AMOrderType, row.AMProdOrdID });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(AMProdItem), new object[] { row.AMOrderType, row.AMProdOrdID });
                    return;
                }

                base.FieldSelecting(sender, e);
            }
        }
        #endregion
        #region QtyReceived
        public abstract class qtyReceived : PX.Data.BQL.BqlDecimal.Field<qtyReceived> { }

        protected Decimal? _QtyReceived;
        [PXDBQuantity(typeof(AMProdMatlSplit.uOM), typeof(AMProdMatlSplit.baseQtyReceived), MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Received", Enabled = false)]
        public virtual Decimal? QtyReceived
        {
            get
            {
                return this._QtyReceived;
            }
            set
            {
                this._QtyReceived = value;
            }
        }
        #endregion
        #region BaseQtyReceived
        public abstract class baseQtyReceived : PX.Data.BQL.BqlDecimal.Field<baseQtyReceived> { }

        protected Decimal? _BaseQtyReceived;
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseQtyReceived
        {
            get
            {
                return this._BaseQtyReceived;
            }
            set
            {
                this._BaseQtyReceived = value;
            }
        }
        #endregion
        #region POCreate
        public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }

        protected Boolean? _POCreate;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Mark for PO", Visible = true, Enabled = false)]
        public virtual Boolean? POCreate
        {
            get
            {
                return this._POCreate;
            }
            set
            {
                this._POCreate = value ?? false;
            }
        }
        #endregion
        #region POCompleted
        public abstract class pOCompleted : PX.Data.BQL.BqlBool.Field<pOCompleted> { }

        protected Boolean? _POCompleted;
        [PXDBBool]
        [PXDefault(false)]
        public virtual Boolean? POCompleted
        {
            get
            {
                return this._POCompleted;
            }
            set
            {
                this._POCompleted = value;
            }
        }
        #endregion
        #region POCancelled
        public abstract class pOCancelled : PX.Data.BQL.BqlBool.Field<pOCancelled> { }

        protected Boolean? _POCancelled;
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? POCancelled
        {
            get
            {
                return this._POCancelled;
            }
            set
            {
                this._POCancelled = value;
            }
        }
        #endregion
        #region POOrderType
        public abstract class pOOrderType : PX.Data.BQL.BqlString.Field<pOOrderType> { }

        protected String _POOrderType;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PO Type", Enabled = false)]
        [POOrderType.RBDList]
        public virtual String POOrderType
        {
            get
            {
                return this._POOrderType;
            }
            set
            {
                this._POOrderType = value;
            }
        }
        #endregion
        #region POOrderNbr
        public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }

        protected String _POOrderNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Order Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<AMProdMatlSplit.pOOrderType>>>>), DescriptionField = typeof(POOrder.orderDesc), ValidateValue = false)]
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
        [PXUIField(DisplayName = "PO Line Nbr.", Enabled = false)]
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
        #region POReceiptType
        public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

        protected String _POReceiptType;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PO Receipt Type", Enabled = false)]
        public virtual String POReceiptType
        {
            get
            {
                return this._POReceiptType;
            }
            set
            {
                this._POReceiptType = value;
            }
        }
        #endregion
        #region POReceiptNbr
        public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

        protected String _POReceiptNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Receipt Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<AMProdMatlSplit.pOReceiptType>>>>), DescriptionField = typeof(POReceipt.invoiceNbr), ValidateValue = false)]
        public virtual String POReceiptNbr
        {
            get
            {
                return this._POReceiptNbr;
            }
            set
            {
                this._POReceiptNbr = value;
            }
        }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        protected String _AMOrderType;
        [AMOrderTypeField(Visible = false, Enabled = false, DisplayName = "Sub. Assy. Order Type")]
        public virtual String AMOrderType
        {
            get
            {
                return this._AMOrderType;
            }
            set
            {
                this._AMOrderType = value;
            }
        }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        protected String _AMProdOrdID;
        [ProductionNbr(Visible = false, Enabled = false, DisplayName = "Sub. Assy. Production Nbr.")]
        public virtual String AMProdOrdID
        {
            get
            {
                return this._AMProdOrdID;
            }
            set
            {
                this._AMProdOrdID = value;
            }
        }
        #endregion
        #region ProdCreate
        public abstract class prodCreate : PX.Data.BQL.BqlString.Field<prodCreate> { }

        protected Boolean? _ProdCreate;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Mark for Production", Visible = true, Enabled = false)]
        public virtual Boolean? ProdCreate
        {
            get
            {
                return this._ProdCreate;
            }
            set
            {
                this._ProdCreate = value ?? false;
            }
        }
        #endregion
        #region AMDocType
        public abstract class aMDocType : PX.Data.BQL.BqlString.Field<aMDocType> { }

        protected String _AMDocType;
        [AMDocType.List]
        [PXUIField(DisplayName = "AM Document Type", Enabled = false, Visible = false)]
        [PXDBString(1, IsFixed = true)]
        public virtual String AMDocType
        {
            get
            {
                return this._AMDocType;
            }
            set
            {
                this._AMDocType = value;
            }
        }
        #endregion
        #region AMBatNbr
        public abstract class aMBatNbr : PX.Data.BQL.BqlString.Field<aMBatNbr> { }

        protected String _AMBatNbr;
        [PXUIField(DisplayName = "AM Batch Nbr", Enabled = false)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<Current<AMProdMatlSplit.aMDocType>>>, OrderBy<Desc<AMBatch.batNbr>>>), Filterable = true, ValidateValue = false)]
        [PXDBString(15, IsUnicode = true)]
        public virtual String AMBatNbr
        {
            get
            {
                return this._AMBatNbr;
            }
            set
            {
                this._AMBatNbr = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        protected Int32? _VendorID;
        [PXDBInt]
        [PXFormula(typeof(Switch<Case<Where<AMProdMatlSplit.isAllocated, Equal<False>>, Current<AMProdMatl.vendorID>>, Null>))]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #endregion

        #region Methods

        public static implicit operator INCostSubItemXRef(AMProdMatlSplit item)
        {
            INCostSubItemXRef ret = new INCostSubItemXRef();
            ret.SubItemID = item.SubItemID;
            ret.CostSubItemID = item.CostSubItemID;

            return ret;
        }

        public static implicit operator INLotSerialStatus(AMProdMatlSplit item)
        {
            INLotSerialStatus ret = new INLotSerialStatus();
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.LocationID = item.LocationID;
            ret.SubItemID = item.SubItemID;
            ret.LotSerialNbr = item.LotSerialNbr;

            return ret;
        }

        public static implicit operator INCostSite(AMProdMatlSplit item)
        {
            INCostSite ret = new INCostSite();
            ret.CostSiteID = item.CostSiteID;

            return ret;
        }

        public static implicit operator INCostStatus(AMProdMatlSplit item)
        {
            INCostStatus ret = new INCostStatus();
            ret.InventoryID = item.InventoryID;
            ret.CostSubItemID = item.CostSubItemID;
            ret.CostSiteID = item.CostSiteID;
            ret.LayerType = INLayerType.Normal;

            return ret;
        }

        #endregion
    }

    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMProdMatlSplit>), Persistent = true)]
    public class AMProdMatlSplit2 : IBqlTable, IProdOper
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, BqlField = typeof(AMProdMatlSplit.orderType))]
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
        [ProductionNbr(IsKey = true, BqlField = typeof(AMProdMatlSplit.prodOrdID))]
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
        [OperationIDField(IsKey = true, BqlField = typeof(AMProdMatlSplit.operationID))]
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
        [PXDBInt(IsKey = true, BqlField = typeof(AMProdMatlSplit.lineID))]
        [PXUIField(DisplayName = "Line Nbr.")]
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
        #region SplitLineNbr
        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(AMProdMatlSplit.splitLineNbr))]
        [PXUIField(DisplayName = "Allocation ID", Enabled = false, Visible = false)]
        public virtual Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
            }
        }
        #endregion
        #region InventoryID

        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDBInt(BqlField = typeof(AMProdMatlSplit.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID")]
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
        [PXDBInt(BqlField = typeof(AMProdMatlSplit.subItemID))]
        [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM")]
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
        [PXDBInt(BqlField = typeof(AMProdMatlSplit.siteID))]
        [PXUIField(DisplayName = "Alloc.Warehouse", FieldClass = "INSITE")]
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
        [PXDBInt(BqlField = typeof(AMProdMatlSplit.locationID))]
        [PXUIField(DisplayName = "Location", FieldClass = "INLOCATION")]
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
        [INUnit(typeof(AMProdMatlSplit2.inventoryID), Enabled = false, BqlField = typeof(AMProdMatlSplit.uOM))]
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
        [PXDBQuantity(typeof(AMProdMatlSplit2.uOM), typeof(AMProdMatlSplit2.baseQty), BqlField = typeof(AMProdMatlSplit.qty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity", Enabled = false)]
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
        [PXDBQuantity(BqlField = typeof(AMProdMatlSplit.baseQty))]
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
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        [PXDBBool(BqlField = typeof(AMProdMatlSplit.isStockItem))]
        [PXUIField(DisplayName = "Stock Item")]
        public bool? IsStockItem
        {
            get;
            set;
        }
        #endregion
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true, BqlField = typeof(AMProdMatlSplit.tranType))]
        [INTranType.List]
        [PXUIField(DisplayName = "Tran. Type")]
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
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate(BqlField = typeof(AMProdMatlSplit.tranDate))]
        [PXUIField(DisplayName = "Allocation Date")]
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
        [PXDBShort(BqlField = typeof(AMProdMatlSplit.invtMult))]
        [PXDefault((short)1)]
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
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [PXDBString(100, InputMask = "", IsUnicode = true, BqlField = typeof(AMProdMatlSplit.lotSerialNbr))]
        [PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
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
        #region ExpireDate
        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [PXDBDate(DisplayMask = "d", InputMask = "d", BqlField = typeof(AMProdMatlSplit.expireDate))]
        [PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial")]
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
        #region IsAllocated

        public abstract class isAllocated : PX.Data.BQL.BqlBool.Field<isAllocated> { }

        protected Boolean? _IsAllocated;
        [PXDBBool(BqlField = typeof(AMProdMatlSplit.isAllocated))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allocated")]
        public virtual Boolean? IsAllocated
        {
            get
            {
                return this._IsAllocated;
            }
            set
            {
                this._IsAllocated = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID(BqlField = typeof(AMProdMatlSplit.createdByID))]
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
        [PXDBCreatedByScreenID(BqlField = typeof(AMProdMatlSplit.createdByScreenID))]
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
        [PXDBCreatedDateTime(BqlField = typeof(AMProdMatlSplit.createdDateTime))]
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
        [PXDBLastModifiedByID(BqlField = typeof(AMProdMatlSplit.lastModifiedByID))]
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
        [PXDBLastModifiedByScreenID(BqlField = typeof(AMProdMatlSplit.lastModifiedByScreenID))]
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
        [PXDBLastModifiedDateTime(BqlField = typeof(AMProdMatlSplit.lastModifiedDateTime))]
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp(BqlField = typeof(AMProdMatlSplit.Tstamp))]
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

        #region Allocation Fields
        #region QtyComplete
        public abstract class qtyComplete : PX.Data.BQL.BqlDecimal.Field<qtyComplete> { }

        protected Decimal? _QtyComplete;
        [PXDBQuantity(typeof(AMProdMatlSplit2.uOM), typeof(AMProdMatlSplit2.baseQtyComplete), MinValue = 0, BqlField = typeof(AMProdMatlSplit.qtyComplete))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Complete", Enabled = false)]
        public virtual Decimal? QtyComplete
        {
            get
            {
                return this._QtyComplete;
            }
            set
            {
                this._QtyComplete = value;
            }
        }
        #endregion
        #region BaseQtyComplete
        public abstract class baseQtyComplete : PX.Data.BQL.BqlDecimal.Field<baseQtyComplete> { }

        protected Decimal? _BaseQtyComplete;
        [PXDBDecimal(6, MinValue = 0, BqlField = typeof(AMProdMatlSplit.baseQtyComplete))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseQtyComplete
        {
            get
            {
                return this._BaseQtyComplete;
            }
            set
            {
                this._BaseQtyComplete = value;
            }
        }
        #endregion
        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        protected Boolean? _Completed;
        [PXDBBool(BqlField = typeof(AMProdMatlSplit.completed))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public virtual Boolean? Completed
        {
            get
            {
                return this._Completed;
            }
            set
            {
                this._Completed = value;
            }
        }
        #endregion
        #region ParentSplitLineNbr
        public abstract class parentSplitLineNbr : PX.Data.BQL.BqlInt.Field<parentSplitLineNbr> { }

        protected Int32? _ParentSplitLineNbr;
        [PXDBInt(BqlField = typeof(AMProdMatlSplit.parentSplitLineNbr))]
        [PXUIField(DisplayName = "Parent Allocation ID", Visible = false, IsReadOnly = true)]
        public virtual Int32? ParentSplitLineNbr
        {
            get
            {
                return this._ParentSplitLineNbr;
            }
            set
            {
                this._ParentSplitLineNbr = value;
            }
        }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXDBGuid(BqlField = typeof(AMProdMatlSplit.refNoteID))]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
        #region QtyReceived
        public abstract class qtyReceived : PX.Data.BQL.BqlDecimal.Field<qtyReceived> { }

        protected Decimal? _QtyReceived;
        [PXDBQuantity(typeof(AMProdMatlSplit2.uOM), typeof(AMProdMatlSplit2.baseQtyReceived), MinValue = 0, BqlField = typeof(AMProdMatlSplit.qtyReceived))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Received", Enabled = false)]
        public virtual Decimal? QtyReceived
        {
            get
            {
                return this._QtyReceived;
            }
            set
            {
                this._QtyReceived = value;
            }
        }
        #endregion
        #region BaseQtyReceived
        public abstract class baseQtyReceived : PX.Data.BQL.BqlDecimal.Field<baseQtyReceived> { }

        protected Decimal? _BaseQtyReceived;
        [PXDBDecimal(6, MinValue = 0, BqlField = typeof(AMProdMatlSplit.baseQtyReceived))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseQtyReceived
        {
            get
            {
                return this._BaseQtyReceived;
            }
            set
            {
                this._BaseQtyReceived = value;
            }
        }
        #endregion
        #region POCreate
        public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }

        protected Boolean? _POCreate;
        [PXDBBool(BqlField = typeof(AMProdMatlSplit.pOCreate))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Mark for PO", Visible = true, Enabled = false)]
        public virtual Boolean? POCreate
        {
            get
            {
                return this._POCreate;
            }
            set
            {
                this._POCreate = value ?? false;
            }
        }
        #endregion
        #region POCompleted
        public abstract class pOCompleted : PX.Data.BQL.BqlBool.Field<pOCompleted> { }

        protected Boolean? _POCompleted;
        [PXDBBool(BqlField = typeof(AMProdMatlSplit.pOCompleted))]
        [PXDefault(false)]
        public virtual Boolean? POCompleted
        {
            get
            {
                return this._POCompleted;
            }
            set
            {
                this._POCompleted = value;
            }
        }
        #endregion
        #region POCancelled
        public abstract class pOCancelled : PX.Data.BQL.BqlBool.Field<pOCancelled> { }

        protected Boolean? _POCancelled;
        [PXDBBool(BqlField = typeof(AMProdMatlSplit.pOCancelled))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? POCancelled
        {
            get
            {
                return this._POCancelled;
            }
            set
            {
                this._POCancelled = value;
            }
        }
        #endregion
        #region POOrderType
        public abstract class pOOrderType : PX.Data.BQL.BqlString.Field<pOOrderType> { }

        protected String _POOrderType;
        [PXDBString(2, IsFixed = true, BqlField = typeof(AMProdMatlSplit.pOOrderType))]
        [PXUIField(DisplayName = "PO Type", Enabled = false)]
        [POOrderType.RBDList]
        public virtual String POOrderType
        {
            get
            {
                return this._POOrderType;
            }
            set
            {
                this._POOrderType = value;
            }
        }
        #endregion
        #region POOrderNbr
        public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }

        protected String _POOrderNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMProdMatlSplit.pOOrderNbr))]
        [PXUIField(DisplayName = "PO Order Nbr.", Enabled = false)]
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
        [PXDBInt(BqlField = typeof(AMProdMatlSplit.pOLineNbr))]
        [PXUIField(DisplayName = "PO Line Nbr.", Enabled = false)]
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
        #region POReceiptType
        public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

        protected String _POReceiptType;
        [PXDBString(2, IsFixed = true, BqlField = typeof(AMProdMatlSplit.pOReceiptType))]
        [PXUIField(DisplayName = "PO Receipt Type", Enabled = false)]
        public virtual String POReceiptType
        {
            get
            {
                return this._POReceiptType;
            }
            set
            {
                this._POReceiptType = value;
            }
        }
        #endregion
        #region POReceiptNbr
        public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

        protected String _POReceiptNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMProdMatlSplit.pOReceiptNbr))]
        [PXUIField(DisplayName = "PO Receipt Nbr.", Enabled = false)]
        public virtual String POReceiptNbr
        {
            get
            {
                return this._POReceiptNbr;
            }
            set
            {
                this._POReceiptNbr = value;
            }
        }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }

        protected String _AMOrderType;
        [AMOrderTypeField(Visible = false, Enabled = false, DisplayName = "Sub. Assy. Order Type", BqlField = typeof(AMProdMatlSplit.aMOrderType))]
        public virtual String AMOrderType
        {
            get
            {
                return this._AMOrderType;
            }
            set
            {
                this._AMOrderType = value;
            }
        }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }

        protected String _AMProdOrdID;
        [ProductionNbr(Visible = false, Enabled = false, DisplayName = "Sub. Assy. Production Nbr.", BqlField = typeof(AMProdMatlSplit.aMProdOrdID))]
        public virtual String AMProdOrdID
        {
            get
            {
                return this._AMProdOrdID;
            }
            set
            {
                this._AMProdOrdID = value;
            }
        }
        #endregion
        #region ProdCreate
        public abstract class prodCreate : PX.Data.BQL.BqlString.Field<prodCreate> { }

        protected Boolean? _ProdCreate;
        [PXDBBool(BqlField = typeof(AMProdMatlSplit.prodCreate))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Mark for Production", Visible = true, Enabled = false)]
        public virtual Boolean? ProdCreate
        {
            get
            {
                return this._ProdCreate;
            }
            set
            {
                this._ProdCreate = value ?? false;
            }
        }
        #endregion
        #region AMDocType
        public abstract class aMDocType : PX.Data.BQL.BqlString.Field<aMDocType> { }

        protected String _AMDocType;
        [AMDocType.List]
        [PXUIField(DisplayName = "AM Document Type", Enabled = false, Visible = false)]
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMProdMatlSplit.aMDocType))]
        public virtual String AMDocType
        {
            get
            {
                return this._AMDocType;
            }
            set
            {
                this._AMDocType = value;
            }
        }
        #endregion
        #region AMBatNbr
        public abstract class aMBatNbr : PX.Data.BQL.BqlString.Field<aMBatNbr> { }

        protected String _AMBatNbr;
        [PXUIField(DisplayName = "AM Batch Nbr", Enabled = false)]
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMProdMatlSplit.aMBatNbr))]
        public virtual String AMBatNbr
        {
            get
            {
                return this._AMBatNbr;
            }
            set
            {
                this._AMBatNbr = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        protected Int32? _VendorID;
        [PXDBInt(BqlField = typeof(AMProdMatlSplit.vendorID))]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #endregion
    }

    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMProdMatlSplit>))]
    public class AMProdMatlSplitPlan : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, DisplayName = "Production Order Type", Enabled = false, BqlField = typeof(AMProdMatlSplit.orderType))]
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
        [ProductionNbr(IsKey = true, Enabled = false, BqlField = typeof(AMProdMatlSplit.prodOrdID))]
        [ProductionOrderSelector(typeof(AMProdMatlSplit.orderType), true, ValidateValue = false)]
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
        [OperationIDField(IsKey = true, Visible = false, Enabled = false, BqlField = typeof(AMProdMatlSplit.operationID))]
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
        [PXDBInt(IsKey = true, BqlField = typeof(AMProdMatlSplit.lineID))]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
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
        #region SplitLineNbr
        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(AMProdMatlSplit.splitLineNbr))]
        [PXUIField(DisplayName = "Allocation ID", Enabled = false, Visible = false)]
        public virtual Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
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