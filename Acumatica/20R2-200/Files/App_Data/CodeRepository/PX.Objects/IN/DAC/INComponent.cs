using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.DR;
using PX.Objects.CM;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.DeferredRevenueComponents)]
	public partial class INComponent : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INComponent>.By<inventoryID, componentID>
		{
			public static INComponent Find(PXGraph graph, int? inventoryID, int? componentID) =>
				FindBy(graph, inventoryID, componentID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INComponent>.By<inventoryID> { }
			public class ComponentInventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INComponent>.By<componentID> { }
			public class SalesSub : Sub.PK.ForeignKeyOf<INComponent>.By<salesSubID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(FK.InventoryItem))]
		[PXDBInt(IsKey=true)]
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
		#region ComponentID
		public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
		protected Int32? _ComponentID;
		[PXDefault()]
		[Inventory(Filterable = true, IsKey = true, DisplayName = "Inventory ID")]
		[PXForeignReference(typeof(FK.ComponentInventoryItem))]
		public virtual Int32? ComponentID
		{
			get
			{
				return this._ComponentID;
			}
			set
			{
				this._ComponentID = value;
			}
		}
		#endregion
		#region DeferredCode
		public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
		protected String _DeferredCode;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
        [PXRestrictor(typeof(Where<DRDeferredCode.multiDeliverableArrangement, NotEqual<boolTrue>>), DR.Messages.ComponentsCantUseMDA)]
		[PXSelector(typeof(DRDeferredCode.deferredCodeID))]
		public virtual String DeferredCode
		{
			get
			{
				return this._DeferredCode;
			}
			set
			{
				this._DeferredCode = value;
			}
		}
		#endregion
		#region DefaultTerm
		public abstract class defaultTerm : PX.Data.BQL.BqlDecimal.Field<defaultTerm> { }

		protected decimal? _DefaultTerm;

		[PXDBDecimal(0, MinValue = 0.0, MaxValue = 10000.0)]
		[PXUIField(DisplayName = "Default Term")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DefaultTerm
		{
			get { return this._DefaultTerm; }
			set { this._DefaultTerm = value; }
		}
		#endregion
		#region DefaultTermUOM
		public abstract class defaultTermUOM : PX.Data.BQL.BqlString.Field<defaultTermUOM> { }

		protected string _DefaultTermUOM;

		[PXDBString(1, IsFixed = true, IsUnicode = false)]
		[PXUIField(DisplayName = "Default Term UOM")]
		[DRTerms.UOMList]
		[PXDefault(DRTerms.Year)]
		public virtual string DefaultTermUOM
		{
			get { return this._DefaultTermUOM; }
			set { this._DefaultTermUOM = value; }
		}
		#endregion
		#region Percentage
		public abstract class percentage : PX.Data.BQL.BqlDecimal.Field<percentage> { }
		protected Decimal? _Percentage;
		[PXDBDecimal(6, MinValue=0, MaxValue=100)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Percentage")]
		[PXFormula(null, typeof(SumCalc<InventoryItem.totalPercentage>))]
		public virtual Decimal? Percentage
		{
			get
			{
				return this._Percentage;
			}
			set
			{
				this._Percentage = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[PXDefault]
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		[PXDefault]
		[SubAccount(typeof(INComponent.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(INComponent.componentID))]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBQuantity]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.Visible)]
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
        #region FixedAmt
        public abstract class fixedAmt : PX.Data.BQL.BqlDecimal.Field<fixedAmt> { }
        protected Decimal? _FixedAmt;
        [PXDBBaseCuryAttribute()]
        [PXUIField(DisplayName = "Fixed Amount")]
        public virtual Decimal? FixedAmt
        {
            get
            {
                return this._FixedAmt;
            }
            set
            {
                this._FixedAmt = value;
            }
        }
        #endregion
		#region AmtOption
        public abstract class amtOption : PX.Data.BQL.BqlString.Field<amtOption> { }
        protected String _AmtOption;
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Allocation Method", Visibility = PXUIVisibility.Visible)]
        [PXDefault(INAmountOption.Percentage)]
        [INAmountOption.List()]
        public virtual String AmtOption
        {
            get
            {
                return this._AmtOption;
            }
            set
            {
                this._AmtOption = value;
            }
        }
        #endregion
        #region System Columns
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
		#endregion
	}

    public static class INAmountOption
    {
        public class ListAttribute : PXStringListAttribute
        {
		    public ListAttribute() : base(
			    new[]
				{
					Pair(Percentage, Messages.Percentage),
					Pair(FixedAmt, Messages.FixedAmt),
					Pair(Residual, Messages.Residual),
				}) {}
        }

        public const string Percentage = "P";
        public const string FixedAmt = "F";
		public const string Residual = "R";

        public class percentage : PX.Data.BQL.BqlString.Constant<percentage>
		{
            public percentage() : base(Percentage) { ;}
        }

        public class fixedAmt : PX.Data.BQL.BqlString.Constant<fixedAmt>
		{
            public fixedAmt() : base(FixedAmt) { ;}
        }

		public class residual : PX.Data.BQL.BqlString.Constant<residual>
		{
			public residual() : base(Residual) { }
		}

    }
}
