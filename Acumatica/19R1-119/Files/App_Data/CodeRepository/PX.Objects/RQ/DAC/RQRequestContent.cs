namespace PX.Objects.RQ
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.AP;
	using PX.Objects.IN;
	using PX.Objects.CM;
	using PX.Objects.TX;
	using PX.Objects.CR;
	using PX.Objects.CS;
	using PX.Objects.EP;
	using PX.Data.ReferentialIntegrity.Attributes;

	[System.SerializableAttribute()]
	public partial class RQRequisitionContent : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<RQRequisitionContent>.By<reqNbr, reqLineNbr, orderNbr, lineNbr>
		{
			public static RQRequisitionContent Find(PXGraph graph, string reqNbr, int? reqLineNbr, string orderNbr, int? lineNbr)
				=> FindBy(graph, reqNbr, reqLineNbr, orderNbr, lineNbr);
		}
		public static class FK
		{
			public class Requisition : RQRequisition.PK.ForeignKeyOf<RQRequisitionContent>.By<reqNbr> { }
			public class RequisitionLine : RQRequisitionLine.PK.ForeignKeyOf<RQRequisitionContent>.By<reqNbr, reqLineNbr> { }
			public class RequestLine : RQRequestLine.PK.ForeignKeyOf<RQRequisitionContent>.By<orderNbr, lineNbr> { }
		}
		#endregion

		#region ReqNbr
		public abstract class reqNbr : PX.Data.BQL.BqlString.Field<reqNbr> { }
		protected String _ReqNbr;
		[PXDBDefault(typeof(RQRequisition.reqNbr))]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]		
		[PXUIField(DisplayName = "Req. Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual String ReqNbr
		{
			get
			{
				return this._ReqNbr;
			}
			set
			{
				this._ReqNbr = value;
			}
		}
		#endregion
		#region ReqLineNbr
		public abstract class reqLineNbr : PX.Data.BQL.BqlInt.Field<reqLineNbr> { }
		protected int? _ReqLineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.RequisitionLine))]
		public virtual int? ReqLineNbr
		{
			get
			{
				return this._ReqLineNbr;
			}
			set
			{
				this._ReqLineNbr = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]		
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.RequestLine))]
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
		#region ItemQty
		public abstract class itemQty : PX.Data.BQL.BqlDecimal.Field<itemQty> { }
		protected Decimal? _ItemQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty.", Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(AddCalc<RQRequestLine.reqQty>))]
		[PXFormula(null, typeof(SubCalc<RQRequestLine.openQty>))]
		public virtual Decimal? ItemQty
		{
			get
			{
				return this._ItemQty;
			}
			set
			{
				this._ItemQty = value;
			}
		}
		#endregion
		#region BaseItemQty
		public abstract class baseItemQty : PX.Data.BQL.BqlDecimal.Field<baseItemQty> { }
		protected Decimal? _BaseItemQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseItemQty
		{
			get
			{
				return this._BaseItemQty;
			}
			set
			{
				this._BaseItemQty = value;
			}
		}
		#endregion
		#region ReqQty
		public abstract class reqQty : PX.Data.BQL.BqlDecimal.Field<reqQty> { }
		protected Decimal? _ReqQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty.", Visibility = PXUIVisibility.Visible)]
		[PXFormula(null, typeof(AddCalc<RQRequisitionLine.orderQty>))]
		public virtual Decimal? ReqQty
		{
			get
			{
				return this._ReqQty;
			}
			set
			{
				this._ReqQty = value;
			}
		}
		#endregion
		#region BaseReqQty
		public abstract class baseReqQty : PX.Data.BQL.BqlDecimal.Field<baseReqQty> { }
		protected Decimal? _BaseReqQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseReqQty
		{
			get
			{
				return this._BaseReqQty;
			}
			set
			{
				this._BaseReqQty = value;
			}
		}
		#endregion
		#region RecalcOnly
		public abstract class recalcOnly : PX.Data.BQL.BqlBool.Field<recalcOnly> { }
		protected bool? _RecalcOnly;
		[PXBool]		
		public virtual bool? RecalcOnly
		{
			get
			{
				return this._RecalcOnly;
			}
			set
			{
				this._RecalcOnly = value;
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