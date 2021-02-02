namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CM;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.ShipTermsDetail, PXDacType.Catalogue)]
	public partial class ShipTermsDetail : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ShipTermsDetail>.By<shipTermsID, lineNbr>
		{
			public static ShipTermsDetail Find(PXGraph graph, string shipTermsID, int? lineNbr) => FindBy(graph, shipTermsID, lineNbr);
		}
		public static class FK
		{
			public class ShipTerms : CS.ShipTerms.PK.ForeignKeyOf<ShipTermsDetail>.By<shipTermsID> { }
		}
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID> { }
		protected String _ShipTermsID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXDefault(typeof(ShipTerms.shipTermsID))]
		public virtual String ShipTermsID
		{
			get
			{
				return this._ShipTermsID;
			}
			set
			{
				this._ShipTermsID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(ShipTerms))]
		[PXParent(typeof(FK.ShipTerms), LeaveChildren = true)]
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
		#region BreakAmount
		public abstract class breakAmount : PX.Data.BQL.BqlDecimal.Field<breakAmount> { }
		protected Decimal? _BreakAmount;
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Break Amount")]
		public virtual Decimal? BreakAmount
		{
			get
			{
				return this._BreakAmount;
			}
			set
			{
				this._BreakAmount = value;
			}
		}
		#endregion
		#region FreightCostPercent
		public abstract class freightCostPercent : PX.Data.BQL.BqlDecimal.Field<freightCostPercent> { }
		protected Decimal? _FreightCostPercent;
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Freight Cost %")]
		public virtual Decimal? FreightCostPercent
		{
			get
			{
				return this._FreightCostPercent;
			}
			set
			{
				this._FreightCostPercent = value;
			}
		}
		#endregion
		#region InvoiceAmountPercent
		public abstract class invoiceAmountPercent : PX.Data.BQL.BqlDecimal.Field<invoiceAmountPercent> { }
		protected Decimal? _InvoiceAmountPercent;
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Invoice Amount %")]
		public virtual Decimal? InvoiceAmountPercent
		{
			get
			{
				return this._InvoiceAmountPercent;
			}
			set
			{
				this._InvoiceAmountPercent = value;
			}
		}
		#endregion
		#region ShippingHandling
		public abstract class shippingHandling : PX.Data.BQL.BqlDecimal.Field<shippingHandling> { }
		protected Decimal? _ShippingHandling;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipping and Handling")]
		public virtual Decimal? ShippingHandling
		{
			get
			{
				return this._ShippingHandling;
			}
			set
			{
				this._ShippingHandling = value;
			}
		}
		#endregion
		#region LineHandling
		public abstract class lineHandling : PX.Data.BQL.BqlDecimal.Field<lineHandling> { }
		protected Decimal? _LineHandling;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Line Handling")]
		public virtual Decimal? LineHandling
		{
			get
			{
				return this._LineHandling;
			}
			set
			{
				this._LineHandling = value;
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
	}
}
