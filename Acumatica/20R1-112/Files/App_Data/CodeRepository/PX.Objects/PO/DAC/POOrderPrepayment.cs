using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.POOrderPrepayment)]
	public class POOrderPrepayment : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POOrderPrepayment>.By<orderType, orderNbr, aPDocType, aPRefNbr>
		{
			public static POOrderPrepayment Find(PXGraph graph, string orderType, string orderNbr, string docType, string refNbr) => FindBy(graph, orderType, orderNbr, docType, refNbr);
		}
		public static class FK
		{
			public class Order : POOrder.PK.ForeignKeyOf<POOrderPrepayment>.By<orderType, orderNbr> { }
		}
		#endregion

		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault]
		[POOrderType.List]
		[PXUIField(DisplayName = "Type")]
		public virtual string OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr>
		{
		}
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public virtual string OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType>
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		[APDocType.List]
		[PXUIField(DisplayName = "Doc. Type")]
		public virtual string APDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr>
		{
		}
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(APRegister.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXSelector(typeof(Search<APRegister.refNbr, Where<APRegister.docType, Equal<Current<aPDocType>>>>), DirtyRead = true)]
		public virtual string APRefNbr
		{
			get;
			set;
		}
		#endregion

		#region IsRequest
		public abstract class isRequest : PX.Data.BQL.BqlBool.Field<isRequest>
		{
		}
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsRequest
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}
		[PXDBLong]
		[CurrencyInfo(ModuleCode = GL.BatchModule.PO)]
		[PXDefault(typeof(Search<POOrder.curyInfoID, Where<POOrder.orderType, Equal<Current<orderType>>, And<POOrder.orderNbr, Equal<Current<orderNbr>>>>>))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal>
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(lineTotal))]
		public virtual decimal? CuryLineTotal
		{
			get;
			set;
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? LineTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryAppliedAmt
		public abstract class curyAppliedAmt : PX.Data.BQL.BqlDecimal.Field<curyAppliedAmt>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(appliedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applied to Order", Enabled = false)]
		public virtual decimal? CuryAppliedAmt
		{
			get;
			set;
		}
		#endregion
		#region AppliedAmt
		public abstract class appliedAmt : PX.Data.BQL.BqlDecimal.Field<appliedAmt>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AppliedAmt
		{
			get;
			set;
		}
		#endregion

		#region PayDocType
		public abstract class payDocType : Data.BQL.BqlString.Field<payDocType>
		{
		}
		[PXDBString(3, IsFixed = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[APDocType.List]
		public virtual string PayDocType
		{
			get;
			set;
		}
		#endregion
		#region PayRefNbr
		public abstract class payRefNbr : Data.BQL.BqlString.Field<payRefNbr>
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Ref.")]
		[PXSelector(typeof(Search<APRegister.refNbr, Where<APRegister.docType, Equal<Current<payDocType>>>>))]
		public virtual string PayRefNbr
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		#region StatusText
		public abstract class statusText : Data.BQL.BqlString.Field<statusText>
		{
		}
		[PXString]
		public virtual string StatusText
		{
			get;
			set;
		}
		#endregion
	}
}
