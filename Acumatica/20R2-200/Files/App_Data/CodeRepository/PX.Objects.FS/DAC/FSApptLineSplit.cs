using System;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;
using PX.Objects.PO;

namespace PX.Objects.FS
{
    [Serializable]
	[PXCacheName(TX.TableName.FSApptLineSplit)]
    public partial class FSApptLineSplit : PX.Data.IBqlTable, ILSDetail
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSApptLineSplit>.By<srvOrdType, apptNbr, lineNbr, splitLineNbr>
		{
			public static FSApptLineSplit Find(PXGraph graph, string srvOrdType, string apptNbr, int? lineNbr, int? splitLineNbr)
				=> FindBy(graph, srvOrdType, apptNbr, lineNbr, splitLineNbr);
		}

		public static class FK
		{
			public class Appointment : FSAppointment.PK.ForeignKeyOf<FSApptLineSplit>.By<srvOrdType, apptNbr> { }
            //PrimaryKeyOf<>
            public class ApptLine : FSAppointmentDet.PK.ForeignKeyOf<FSApptLineSplit>.By<srvOrdType, apptNbr, lineNbr> { }
            //PrimaryKeyOf<>
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<FSApptLineSplit>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<FSApptLineSplit>.By<siteID> { }
			//public class PlanType : INPlanType.PK.ForeignKeyOf<FSApptLineSplit>.By<planType> { }
			//public class OrigPlanType : INPlanType.PK.ForeignKeyOf<FSApptLineSplit>.By<origPlanType> { }
			public class OrigLineSplit : FSSODetSplit.PK.ForeignKeyOf<FSApptLineSplit>.By<origSrvOrdType, origSrvOrdNbr, origLineNbr, origSplitLineNbr> { }
		}
        #endregion

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region ApptNbr
        public abstract class apptNbr : PX.Data.BQL.BqlString.Field<apptNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(FK.Appointment))]
        public virtual string ApptNbr { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(FSAppointmentDet.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false, Visible = false)]
        [PXParent(typeof(FK.ApptLine))]
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
		#region OrigSrvOrdType
		public abstract class origSrvOrdType : PX.Data.BQL.BqlString.Field<origSrvOrdType> { }

		[PXDBString(4, IsFixed = true)]
		[PXDefault(typeof(FSAppointmentDet.srvOrdType))]
		public virtual String OrigSrvOrdType { get; set; }
        #endregion
		#region OrigSrvOrdNbr
		public abstract class origSrvOrdNbr : PX.Data.BQL.BqlString.Field<origSrvOrdNbr> { }

		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXDBDefault(typeof(FSAppointment.soRefNbr), DefaultForUpdate = false)]
        public virtual String OrigSrvOrdNbr { get; set; }
        #endregion
        #region OrigLineNbr
        public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		[PXDBInt()]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? OrigLineNbr { get; set; }
		#endregion
		#region OrigSplitLineNbr
		public abstract class origSplitLineNbr : PX.Data.BQL.BqlInt.Field<origSplitLineNbr> { }
		protected Int32? _OrigSplitLineNbr;
		[PXDBInt()]
        public virtual Int32? OrigSplitLineNbr
		{
			get
			{
				return this._OrigSplitLineNbr;
			}
			set
			{
				this._OrigSplitLineNbr = value;
			}
		}
		#endregion
		#region OrigPlanType
		public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
		[PXDBString(2, IsFixed = true)]
		//[PXDefault(typeof(FSAppointmentDet.origPlanType))]
		//[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
		public virtual String OrigPlanType
		{
			get;
			set;
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXDefault(typeof(FSAppointmentDet.operation))]
        //[PXSelector(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<FSApptLineSplit.origSrvOrdType>>>>))]
        public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSAppointment.splitLineCntr))]
		[PXUIField(DisplayName = "Allocation ID", Visible = false, IsReadOnly = true)]
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
		#region ParentSplitLineNbr
		public abstract class parentSplitLineNbr : PX.Data.BQL.BqlInt.Field<parentSplitLineNbr> { }
		protected Int32? _ParentSplitLineNbr;
		[PXDBInt()]
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
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXDefault(typeof(shortMinus1))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Enabled = false, Visible = true)]
		[PXDefault(typeof(FSAppointmentDet.inventoryID))]
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
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true)]
        [PXDefault(typeof(Selector<FSApptLineSplit.inventoryID, Switch<
            Case<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>, SOLineType.inventory,
            Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
            SOLineType.miscCharge>>))]
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
		#region IsStockItem
		public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
		[PXDBBool()]
		[PXFormula(typeof(Selector<FSApptLineSplit.inventoryID, InventoryItem.stkItem>))]
		public bool? IsStockItem
		{
			get;
			set;
		}
		#endregion
		#region IsComponentItem
		public abstract class isComponentItem : PX.Data.BQL.BqlBool.Field<isComponentItem> { }
		[PXDBBool()]
		[PXFormula(typeof(Switch<Case<Where<FSApptLineSplit.inventoryID, Equal<Current<FSAppointmentDet.inventoryID>>>, False>, True>))]
		public bool? IsComponentItem
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
        [PXString(SO.SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
        [PXUnboundDefault(typeof(FSAppointmentDet.tranType))]
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
        public virtual DateTime? TranDate
        {
            get { return this.ApptDate; }
        }
        #endregion
        #region PlanType
        public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;
		[PXDBString(2, IsFixed = true)]
		//[PXDefault(typeof(FSAppointmentDet.planType))]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong(IsImmutable = true)]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site()]
		[PXDefault(typeof(FSAppointmentDet.siteID))]
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
		[LocationAvail(typeof(FSApptLineSplit.inventoryID), typeof(FSApptLineSplit.subItemID), typeof(FSApptLineSplit.siteID), typeof(FSApptLineSplit.tranType), typeof(FSApptLineSplit.invtMult))]
        [PXDefault()]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[IN.SubItem(typeof(FSApptLineSplit.inventoryID))]
		[PXDefault()]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
        [FSApptLotSerialNbr(typeof(FSApptLineSplit.siteID), typeof(FSApptLineSplit.inventoryID), typeof(FSApptLineSplit.subItemID), typeof(FSApptLineSplit.locationID), typeof(FSAppointmentDet.lotSerialNbr), FieldClass = "LotSerial")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCheckUnique(Where = typeof(Where<srvOrdType, Equal<Current<FSAppointmentDet.srvOrdType>>,
											And<apptNbr, Equal<Current<FSAppointmentDet.refNbr>>,
											And<lineNbr, Equal<Current<FSAppointmentDet.lineNbr>>>>>)
										,ErrorMessage = TX.Error.LotSerialDuplicated)]
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
        #region LastLotSerialNbr
        public abstract class lastLotSerialNbr : PX.Data.BQL.BqlString.Field<lastLotSerialNbr> { }
        protected String _LastLotSerialNbr;
        [PXString(100, IsUnicode = true)]
        public virtual String LastLotSerialNbr
        {
            get
            {
                return this._LastLotSerialNbr;
            }
            set
            {
                this._LastLotSerialNbr = value;
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
		#region AssignedNbr
		public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
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
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[INExpireDate(typeof(FSApptLineSplit.inventoryID), Visible = false, FieldClass = "LotSerial")]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(FSApptLineSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXDefault]
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
		[PXDBQuantity(typeof(FSApptLineSplit.uOM), typeof(FSApptLineSplit.baseQty), InventoryUnitType.BaseUnit)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
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
		[PXDBDecimal(6)]
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
        #region ApptDate
        public abstract class apptDate : PX.Data.BQL.BqlDateTime.Field<apptDate> { }
		protected DateTime? _ApptDate;
		[PXDBDate()]
		[PXDBDefault(typeof(FSAppointment.effDocDate))]
		public virtual DateTime? ApptDate
        {
			get
			{
				return this._ApptDate;
			}
			set
			{
				this._ApptDate = value;
			}
		}
		#endregion
		#region Confirmed
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		protected Boolean? _Confirmed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Confirmed")]
		public virtual Boolean? Confirmed
		{
			get
			{
				return this._Confirmed;
			}
			set
			{
				this._Confirmed = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
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
		#region IsUnassigned
		public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
		protected Boolean? _IsUnassigned;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsUnassigned
		{
			get
			{
				return this._IsUnassigned;
			}
			set
			{
				this._IsUnassigned = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXFormula(typeof(Selector<FSApptLineSplit.locationID, INLocation.projectID>))]
		[PXInt]
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
		[PXFormula(typeof(Selector<FSApptLineSplit.locationID, INLocation.taskID>))]
		[PXInt]
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
		
		#region POCreate
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
		protected Boolean? _POCreate;
		[PXDBBool()]
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
		[PXDBBool()]
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
		[PXDBBool()]
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
		#region POSource
		public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }
		protected string _POSource;
		[PXDBString()]
		[PXFormula(typeof(Current<FSAppointmentDet.pOSource>))]
		public virtual string POSource
		{
			get
			{
				return this._POSource;
			}
			set
			{
				this._POSource = value;
			}
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt()]
		[PXFormula(typeof(Current<FSAppointmentDet.poVendorID>))]
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
		#region POSiteID
		public abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }
		protected Int32? _POSiteID;
		[PXDBInt()]
		//[PXFormula(typeof(Current<FSAppointmentDet.pos>)]
		public virtual Int32? POSiteID
		{
			get
			{
				return this._POSiteID;
			}
			set
			{
				this._POSiteID = value;
			}
		}
		#endregion
		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		protected String _POType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "PO Type", Enabled = false)]
		[POOrderType.RBDList]
		public virtual String POType
		{
			get
			{
				return this._POType;
			}
			set
			{
				this._POType = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
		protected String _PONbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<FSApptLineSplit.pOType>>>>), DescriptionField = typeof(POOrder.orderDesc))]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
		protected Int32? _POLineNbr;
		[PXDBInt()]
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
		[PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<FSApptLineSplit.pOReceiptType>>>>), DescriptionField = typeof(POReceipt.invoiceNbr))]
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

        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
        protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote()]
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
        public class PXRefNoteAttribute : PX.Objects.Common.PXRefNoteBaseAttribute
        {
            public PXRefNoteAttribute()
                : base()
            {
            }

            public override void CacheAttached(PXCache sender)
            {
                base.CacheAttached(sender);

                PXButtonDelegate del = delegate (PXAdapter adapter)
                {
                    PXCache cache = adapter.View.Graph.Caches[typeof(FSApptLineSplit)];
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
                sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(typeof(FSAppointment)), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
            }

            public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
            {
                FSApptLineSplit row = e.Row as FSApptLineSplit;

                if (row != null && !string.IsNullOrEmpty(row.PONbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches[typeof(POOrder)], new object[] { row.POType, row.PONbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(POOrder), new object[] { row.POType, row.PONbr });
                }
                else if (row != null && !string.IsNullOrEmpty(row.POReceiptNbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches[typeof(POReceipt)], new object[] { row.POReceiptType, row.POReceiptNbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(POReceipt), new object[] { row.POReceiptType, row.POReceiptNbr });
                }
                else
                {
                    base.FieldSelecting(sender, e);
                }
            }
        }

        #endregion

        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		[PXDBCreatedByScreenID()]
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedByID()]
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
		[PXDBLastModifiedByScreenID()]
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
		[PXDBLastModifiedDateTime()]
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
		[PXDBTimestamp()]
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

        #region FSSODetSplitRow
        public virtual FSSODetSplit FSSODetSplitRow { get; set; }
        #endregion
    }
}
