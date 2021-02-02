using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PO
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.IN;
	[System.SerializableAttribute()]
	[PXCacheName(Messages.POReceiptLineSplit)]
	public partial class POReceiptLineSplit : PX.Data.IBqlTable, ILSDetail
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceiptLineSplit>.By<receiptNbr, lineNbr, splitLineNbr>
		{
			public static POReceiptLineSplit Find(PXGraph graph, string receiptNbr, int? lineNbr, int? splitLineNbr)
				=> FindBy(graph, receiptNbr, lineNbr, splitLineNbr);
		}
		public static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POReceiptLineSplit>.By<receiptNbr> { }
			public class ReceiptLine : POReceiptLine.PK.ForeignKeyOf<POReceiptLineSplit>.By<receiptNbr, lineNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<POReceiptLineSplit>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<POReceiptLineSplit>.By<siteID> { }
			public class INLotSerialStatus : IN.INLotSerialStatus.PK.ForeignKeyOf<POReceiptLineSplit>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
			public class OrigPlanType : INPlanType.PK.ForeignKeyOf<POReceiptLineSplit>.By<origPlanType> { }
		}
		#endregion

		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(POReceipt.receiptNbr))]
		[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXParent(typeof(FK.ReceiptLine))]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(POReceiptLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(POReceipt.lineCntr))]
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
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
		protected string _PONbr;
		[PXDefault(typeof(Search<POReceiptLine.pONbr, Where<POReceiptLine.receiptNbr, Equal<Current<POReceiptLineSplit.receiptNbr>>, And<POReceiptLine.lineNbr, Equal<Current<POReceiptLineSplit.lineNbr>>>>>),PersistingCheck=PXPersistingCheck.Nothing)]
		[PXDBString(POReceiptLine.pONbr.Length, IsUnicode = true)]
		public string PONbr
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
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		protected String _ReceiptType;
		[PXUIField(DisplayName = "Type")]
		[PXDefault(typeof(Search<POReceiptLine.receiptType, Where<POReceiptLine.receiptNbr, Equal<Current<POReceiptLineSplit.receiptNbr>>, And<POReceiptLine.lineNbr, Equal<Current<POReceiptLineSplit.lineNbr>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(POReceiptLine.receiptType.Length, IsFixed = true)]
		public virtual String ReceiptType
		{
			get
			{
				return this._ReceiptType;
			}
			set
			{
				this._ReceiptType = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDefault(typeof(Search<POReceiptLine.lineType, Where<POReceiptLine.receiptNbr, Equal<Current<POReceiptLineSplit.receiptNbr>>, And<POReceiptLine.lineNbr, Equal<Current<POReceiptLineSplit.lineNbr>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(POReceiptLine.lineType.Length, IsFixed = true)]
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
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
		protected DateTime? _ReceiptDate;
		[PXDBDate()]
		[PXDefault(typeof(POReceipt.receiptDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Ship On", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion		
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXDefault(typeof(POReceiptLine.invtMult))]
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
		[StockItem(Visible = false)]
		[PXDefault(typeof(POReceiptLine.inventoryID))]
		[PXForeignReference(typeof(FK.InventoryItem))]
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
		#region IsStockItem
		public bool? IsStockItem
		{
			get
			{
				return true;
			}
			set { }
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		public string TranType
		{
			[PXDependsOnFields(typeof(receiptType))]
			get
			{
				return POReceiptType.GetINTranType(this._ReceiptType);
			}
		}
		#endregion
		#region TranDate
		public virtual DateTime? TranDate
		{
			get { return this._ReceiptDate; }
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site()]
		[PXDefault(typeof(POReceiptLine.siteID))]
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
		[LocationAvail(typeof(POReceiptLineSplit.inventoryID), typeof(POReceiptLineSplit.subItemID), typeof(POReceiptLineSplit.siteID), typeof(POReceiptLineSplit.tranType), typeof(POReceiptLineSplit.invtMult), KeepEntry = false)]
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
		[IN.SubItem(typeof(POReceiptLineSplit.inventoryID))]
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
        #region OrigPlanType
        public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
        protected String _OrigPlanType;
        [PXDBString(2, IsFixed = true)]
        [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
        [PXDefault(typeof(POReceiptLine.origPlanType), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String OrigPlanType
        {
            get
            {
                return this._OrigPlanType;
            }
            set
            {
                this._OrigPlanType = value;
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[POLotSerialNbrAttribute(typeof(POReceiptLineSplit.inventoryID), typeof(POReceiptLineSplit.subItemID), typeof(POReceiptLineSplit.locationID), typeof(POReceiptLine.lotSerialNbr), FieldClass = "LotSerial")]
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
		[POExpireDateAttribute(typeof(POReceiptLineSplit.inventoryID), FieldClass = "LotSerial")]
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
		
		[INUnit(typeof(POReceiptLineSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
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

		[PXDBQuantity(typeof(POReceiptLineSplit.uOM), typeof(POReceiptLineSplit.baseQty), InventoryUnitType.BaseUnit, MinValue=0)]
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
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		#region ReceivedQty
		[PXDBQuantity(typeof(uOM), typeof(baseReceivedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Received Qty.", Enabled = false)]
		public Decimal? ReceivedQty { get; set; }
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
		#endregion
		#region BaseReceivedQty
		[PXDBDecimal(6)]
		public virtual Decimal? BaseReceivedQty { get; set; }
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }
		#endregion
		#region PutAwayQty
		[PXDBQuantity(typeof(uOM), typeof(basePutAwayQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Put Away Qty.", Enabled = false)]
		public Decimal? PutAwayQty { get; set; }
		public abstract class putAwayQty : PX.Data.BQL.BqlDecimal.Field<putAwayQty> { }
		#endregion
		#region BasePutAwayQty
		[PXDBDecimal(6)]
		public virtual Decimal? BasePutAwayQty { get; set; }
		public abstract class basePutAwayQty : PX.Data.BQL.BqlDecimal.Field<basePutAwayQty> { }
		#endregion
		#region MaxTransferBaseQty
		public abstract class maxTransferBaseQty : PX.Data.BQL.BqlDecimal.Field<maxTransferBaseQty> { }
        protected Decimal? _MaxTransferBaseQty;
        [PXDBQuantity()]
        public virtual Decimal? MaxTransferBaseQty
        {
            get
            {
                return this._MaxTransferBaseQty;
            }
            set
            {
                this._MaxTransferBaseQty = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXFormula(typeof(Selector<POReceiptLineSplit.locationID, INLocation.projectID>))]
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
		[PXFormula(typeof(Selector<POReceiptLineSplit.locationID, INLocation.taskID>))]
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
	}
}
