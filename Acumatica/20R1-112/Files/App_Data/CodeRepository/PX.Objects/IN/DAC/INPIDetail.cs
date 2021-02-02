using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.CM;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INPIDetail)]
	public partial class INPIDetail : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INPIDetail>.By<pIID, lineNbr>
		{
			public static INPIDetail Find(PXGraph graph, string pIID, int? lineNbr) => FindBy(graph, pIID, lineNbr);
		}
		public static class FK
		{
			public class PIHeader : INPIHeader.PK.ForeignKeyOf<INPIDetail>.By<pIID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INPIDetail>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INPIDetail>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INPIDetail>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INPIDetail>.By<locationID> { }
			public class ReasonCode : CS.ReasonCode.PK.ForeignKeyOf<INPIDetail>.By<reasonCode> { }
		}
		#endregion
		#region PIID
		public abstract class pIID : PX.Data.BQL.BqlString.Field<pIID> { }
		protected String _PIID;
		[PXDBDefault(typeof(INPIHeader.pIID))]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXParent(typeof(FK.PIHeader))]
		public virtual String PIID
		{
			get
			{
				return this._PIID;
			}
			set
			{
				this._PIID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(INPIHeader.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
		public virtual int? LineNbr
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
		#region TagNumber
		public abstract class tagNumber : PX.Data.BQL.BqlInt.Field<tagNumber> { }
		protected Int32? _TagNumber;
		[PXDBInt(MinValue = 0)]
		[PXUIField(DisplayName = "Tag Nbr.", Enabled = false)]
		public virtual Int32? TagNumber
		{
			get
			{
				return this._TagNumber;
			}
			set
			{
				this._TagNumber = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
			public class InventoryBaseUnitRule :
				InventoryItem.baseUnit.PreventEditIfExists<
					Select2<INPIDetail,
					InnerJoin<INPIHeader, On<FK.PIHeader>>,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And<INPIHeader.status, NotIn3<INPIHdrStatus.completed, INPIHdrStatus.cancelled>>>>>
			{ }
		}
		protected Int32? _InventoryID;
		[StockItem(DisplayName="Inventory ID")]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<INPIDetail.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<INTranSplit.inventoryID>))]
		[SubItem(typeof(INPIDetail.inventoryID))]		
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
		#region BaseUnit
		[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R1)]
		[PXString(6, IsUnicode = true, InputMask = ">aaaaaa")]
		[PXUIField(DisplayName = "Base Unit", Visibility = PXUIVisibility.SelectorVisible, IsReadOnly = true)]
		public string BaseUnit { get; set; }
		[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R1)]
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDefault(typeof(INPIHeader.siteID))]
		[Site()]
		public Int32? SiteID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(INPIDetail.siteID), Visibility = PXUIVisibility.SelectorVisible)]		
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		//[INLotSerialNbr(typeof(INPIDetail.inventoryID), typeof(INPIDetail.subItemID), typeof(INPIDetail.locationID))]
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Lot/Serial Number", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "LotSerial")]
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
		[PXDBDate()]
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
		#region ManualCost
		public abstract class manualCost : PX.Data.BQL.BqlBool.Field<manualCost> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Cost", Enabled = false)]
		public virtual bool? ManualCost
		{
			get;
			set;
		}
		#endregion

		#region BookQty
		public abstract class bookQty : PX.Data.BQL.BqlDecimal.Field<bookQty> { }
		protected Decimal? _BookQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Book Quantity" , Enabled = false)]
		public virtual Decimal? BookQty
		{
			get
			{
				return this._BookQty;
			}
			set
			{
				this._BookQty = value;
			}
		}
		#endregion

		#region PhysicalQty
		public abstract class physicalQty : PX.Data.BQL.BqlDecimal.Field<physicalQty> { }
		protected Decimal? _PhysicalQty;
		[PXDBQuantity(MinValue = 0)]
		[PXUIField(DisplayName = "Physical Quantity")]

		// [PXFormula(null, typeof(SumCalc<INPIHeader.totalPhysicalQty>))]
		//  manually , not via PXFormula because of the problems in INPIReview with double-counting during cost recalculation called from FieldUpdated event

		public virtual Decimal? PhysicalQty
		{
			get
			{
				return this._PhysicalQty;
			}
			set
			{
				this._PhysicalQty = value;
			}
		}
		#endregion
		#region VarQty
		public abstract class varQty : PX.Data.BQL.BqlDecimal.Field<varQty> { }
		protected Decimal? _VarQty;
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Variance Quantity", Enabled = false)]

		//[PXFormula(null, typeof(SumCalc<INPIHeader.totalVarQty>))]
		//  manually , not via PXFormula because of the problems in INPIReview with double-counting during cost recalculation called from FieldUpdated event

		public virtual Decimal? VarQty
		{
			get
			{
				return this._VarQty;
			}
			set
			{
				this._VarQty = value;
			}
		}
		#endregion

		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCost(keepNullValue: true)]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
        #region ReasonCode
        public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
        protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.adjustment>>>), DescriptionField = typeof(ReasonCode.descr))]
        [PXDefault(typeof(Coalesce<Search2<INPostClass.pIReasonCode, 
	        InnerJoin<InventoryItem, 
		        On<InventoryItem.FK.PostClass>>, 
	        Where<InventoryItem.inventoryID, Equal<Current<INPIDetail.inventoryID>>, 
		        And<INPostClass.pIReasonCode, IsNotNull>>>, Search<INSetup.pIReasonCode>>))]
        [PXFormula(typeof(Default<INPIDetail.inventoryID>))]
        [PXUIField(DisplayName = "Reason Code")]
		[PXForeignReference(typeof(FK.ReasonCode))]
		public virtual String ReasonCode
        {
            get
            {
                return this._ReasonCode;
            }
            set
            {
                this._ReasonCode = value;
            }
        }
        #endregion
/*
		#region ExtBookCost
		public abstract class extBookCost : PX.Data.BQL.BqlDecimal.Field<extBookCost> { }
		protected Decimal? _ExtBookCost;
		[PXDBBaseCury()]
		//[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Book Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? ExtBookCost
		{
			get
			{
				return this._ExtBookCost;
			}
			set
			{
				this._ExtBookCost = value;
			}
		}
		#endregion
*/
		#region ExtVarCost
		public abstract class extVarCost : PX.Data.BQL.BqlDecimal.Field<extVarCost> { }
		protected Decimal? _ExtVarCost;
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Estimated Ext. Variance Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]

		//[PXFormula(null, typeof(SumCalc<INPIHeader.totalVarCost>))]
		//  manually , not via PXFormula because of the problems in INPIReview with double-counting during cost recalculation called from FieldUpdated event

		public virtual Decimal? ExtVarCost
		{
			get
			{
				return this._ExtVarCost;
			}
			set
			{
				this._ExtVarCost = value;
			}
		}
		#endregion
		#region FinalExtVarCost
		public abstract class finalExtVarCost : Data.BQL.BqlDecimal.Field<finalExtVarCost>
		{
		}
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Final Ext. Variance Cost", Enabled = false)]
		public virtual Decimal? FinalExtVarCost
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INPIDetStatus.NotEntered)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[INPIDetStatus.List()]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
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

		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INPIDetLineType.UserEntered)]
		[PXUIField(DisplayName = "Line Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[INPIDetLineType.List()]
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

		#region IsCostDefaulting
		public virtual bool? IsCostDefaulting
		{
			get;
			set;
		}
		#endregion
		

		#region //CSQtyOnHand
		/*
		public abstract class cSQtyOnHand : PX.Data.BQL.BqlDecimal.Field<cSQtyOnHand> { }
		protected Decimal? _CSQtyOnHand;
		[PXDBQuantity()]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual Decimal? CSQtyOnHand
		{
			get
			{
				return this._CSQtyOnHand;
			}
			set
			{
				this._CSQtyOnHand = value;
			}
		}
		*/
		#endregion

		#region //CSTotalCost
		/*
		public abstract class cSTotalCost : PX.Data.BQL.BqlDecimal.Field<cSTotalCost> { }
		protected Decimal? _CSTotalCost;
		[PXDBBaseCury()]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual Decimal? CSTotalCost
		{
			get
			{
				return this._CSTotalCost;
			}
			set
			{
				this._CSTotalCost = value;
			}
		}
		*/
		#endregion

	}



	public class INPIDetStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(NotEntered, Messages.NotEntered),
					Pair(Entered, Messages.Entered),
					Pair(Skipped, Messages.Skipped),
				}) {}
		}

		public const string NotEntered = "N";
		public const string Entered = "E";
		public const string Skipped = "S";

		public class notEntered : PX.Data.BQL.BqlString.Constant<notEntered>
		{
			public notEntered() : base(NotEntered) { ;}
		}

		public class entered : PX.Data.BQL.BqlString.Constant<entered>
		{
			public entered() : base(Entered) { ;}
		}

		public class skipped : PX.Data.BQL.BqlString.Constant<skipped>
		{
			public skipped() : base(Skipped) { ;}
		}
	}


	public class INPIDetLineType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Normal, Messages.Normal),
					Pair(Blank, Messages.Blank),
					Pair(UserEntered, Messages.UserEntered),
				}) {}
		}

		public const string Normal = "N";
		public const string Blank = "B";
		public const string UserEntered = "U";  // probably this functionality will be implemented in the future

		public class normal : PX.Data.BQL.BqlString.Constant<normal>
		{
			public normal() : base(Normal) { ;}
		}

		public class blank : PX.Data.BQL.BqlString.Constant<blank>
		{
			public blank() : base(Blank) { ;}
		}

		public class userEntered : PX.Data.BQL.BqlString.Constant<userEntered>
		{
			public userEntered() : base(UserEntered) { ;}
		}
	}




}

