using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using SpecificCostStatus = PX.Objects.IN.Overrides.INDocumentRelease.SpecificCostStatus;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INTranCost)]
	public partial class INTranCost : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INTranCost>.By<docType, refNbr, lineNbr, costID, costDocType, costRefNbr>
		{
			public static INTranCost Find(PXGraph graph, string docType, string refNbr, int? lineNbr, long? costID, string costDocType, string costRefNbr) =>
				FindBy(graph, docType, refNbr, lineNbr, costID, costDocType, costRefNbr);
		}
		public static class FK
		{
			public class Tran : INTran.PK.ForeignKeyOf<INTranCost>.By<docType, refNbr, lineNbr> { }
			public class Register : INRegister.PK.ForeignKeyOf<INTranCost>.By<costDocType, costRefNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INTranCost>.By<inventoryID> { }
			public class CostSubItem : INSubItem.PK.ForeignKeyOf<INTranCost>.By<costSubItemID> { }
			public class CostSite : INSite.PK.ForeignKeyOf<INTranCost>.By<costSiteID> { }
			public class COGSSub : Sub.PK.ForeignKeyOf<INTranCost>.By<cOGSSubID> { }
			public class InvtSub : Sub.PK.ForeignKeyOf<INTranCost>.By<invtSubID> { }
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault()]
		[INTranType.List()]
		[PXUIField(DisplayName = "Transaction Type")]
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
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Ref. Number")]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXParent(typeof(Select<INTran, 
			Where<INTran.docType, Equal<Current<INTranCost.docType>>, 
			And<INTran.refNbr, Equal<Current<INTranCost.refNbr>>, 
			And<INTran.lineNbr, Equal<Current<INTranCost.lineNbr>>, 
			And<Current<INTranCost.isVirtual>, NotEqual<True>>>>>>), LeaveChildren = true)]
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
		#region CostID
		public abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
		protected Int64? _CostID;
		[PXDBLong(IsKey = true)]
		[PXDefault()]
		public virtual Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region CostDocType
		public abstract class costDocType : PX.Data.BQL.BqlString.Field<costDocType> { }
		protected String _CostDocType;
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public virtual String CostDocType
		{
			get
			{
				return this._CostDocType;
			}
			set
			{
				this._CostDocType = value;
			}
		}
		#endregion
		#region CostRefNbr
		public abstract class costRefNbr : PX.Data.BQL.BqlString.Field<costRefNbr> { }
		protected String _CostRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		public virtual String CostRefNbr
		{
			get
			{
				return this._CostRefNbr;
			}
			set
			{
				this._CostRefNbr = value;
			}
		}
		#endregion
		#region IsOversold
		public abstract class isOversold : PX.Data.BQL.BqlBool.Field<isOversold> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual bool? IsOversold
		{
			get;
			set;
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Inventory Multiplier")]
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
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<INTran.costedQty>))]
		[PXUIField(DisplayName = "Qty.")]
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
        #region OversoldQty
        public abstract class oversoldQty : PX.Data.BQL.BqlDecimal.Field<oversoldQty> { }
        protected Decimal? _OversoldQty;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? OversoldQty
        {
            get
            {
                return this._OversoldQty;
            }
            set
            {
                this._OversoldQty = value;
            }
        }
        #endregion
        #region TranCost
        public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		protected Decimal? _TranCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXFormula(null, typeof(SumCalc<INTran.tranCost>))]
		[PXUIField(DisplayName = "Transaction Cost")]
		public virtual Decimal? TranCost
		{
			get
			{
				return this._TranCost;
			}
			set
			{
				this._TranCost = value;
			}
		}
        #endregion
        #region OversoldTranCost
        public abstract class oversoldTranCost : PX.Data.BQL.BqlDecimal.Field<oversoldTranCost> { }
        protected Decimal? _OversoldTranCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? OversoldTranCost
        {
            get
            {
                return this._OversoldTranCost;
            }
            set
            {
                this._OversoldTranCost = value;
            }
        }
        #endregion
        #region VarCost
        public abstract class varCost : PX.Data.BQL.BqlDecimal.Field<varCost> { }
		protected Decimal? _VarCost;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VarCost
		{
			get
			{
				return this._VarCost;
			}
			set
			{
				this._VarCost = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected Decimal? _TranAmt;
		[PXDecimal(4)]
		[PXFormula(null, typeof(SumCalc<INTran.tranAmt>))]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Transaction Date")]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[PXDBString(6, IsFixed = true)]
		[PXDefault("")]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		protected String _TranPeriodID;
		[PXDBString(6, IsFixed = true)]
		[PXDefault("")]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt()]
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
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
		protected Int32? _CostSubItemID;
		//[PXDBInt()]
		[SubItem()]
		[PXDefault()]
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
		[PXDBInt()]
		[PXDefault()]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true)]
		[PXDefault(typeof(Coalesce<Search<SpecificCostStatus.lotSerialNbr, Where<SpecificCostStatus.costID, Equal<Current<INTranCost.costID>>>>,
            Search<Overrides.INDocumentRelease.SpecificTransitCostStatus.lotSerialNbr, Where<Overrides.INDocumentRelease.SpecificTransitCostStatus.costID, Equal<Current<INTranCost.costID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region IsVirtual
        public abstract class isVirtual : PX.Data.BQL.BqlBool.Field<isVirtual> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual bool? IsVirtual
        {
            get;
            set;
        }
        #endregion

        #region InvtAcctID
        public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID> { }
		protected Int32? _InvtAcctID;
		[Account()]
		[PXDefault()]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.BQL.BqlInt.Field<invtSubID> { }
		protected Int32? _InvtSubID;
		[SubAccount(typeof(INTranCost.invtAcctID))]
		[PXDefault()]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.BQL.BqlInt.Field<cOGSAcctID> { }
		protected Int32? _COGSAcctID;
		[Account()]
		public virtual Int32? COGSAcctID
		{
			get
			{
				return this._COGSAcctID;
			}
			set
			{
				this._COGSAcctID = value;
			}
		}
		#endregion
		#region COGSSubID
		public abstract class cOGSSubID : PX.Data.BQL.BqlInt.Field<cOGSSubID> { }
		protected Int32? _COGSSubID;
		[SubAccount(typeof(INTranCost.cOGSAcctID))]
		public virtual Int32? COGSSubID
		{
			get
			{
				return this._COGSSubID;
			}
			set
			{
				this._COGSSubID = value;
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
		[PXUIField(DisplayName = "Created")]
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
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		[PXDecimal(6)]
		public virtual Decimal? QtyOnHand
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
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDecimal(6)]
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
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		[PXDecimal(4)]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TranCost;
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
	}
}
