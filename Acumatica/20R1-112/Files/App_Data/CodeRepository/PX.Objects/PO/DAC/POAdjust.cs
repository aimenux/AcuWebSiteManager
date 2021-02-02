using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using System;

namespace PX.Objects.PO
{
	[Serializable]
	[PXCacheName(Messages.POAdjust)]
	public class POAdjust : IBqlTable, IAdjustmentStub
	{
		#region Keys
		public class PK : PrimaryKeyOf<POAdjust>.By<adjgDocType, adjgRefNbr, adjdOrderType, adjdOrderNbr, adjNbr, adjdDocType, adjdRefNbr>
		{
			public static POAdjust Find(PXGraph graph, string adjgDocType, string adjgRefNbr, string adjdOrderType, string adjdOrderNbr, int adjNbr, string adjdDocType, string adjdRefNbr)
				=> FindBy(graph, adjgDocType, adjgRefNbr, adjdOrderType, adjdOrderNbr, adjNbr, adjdDocType, adjdRefNbr);
		}
		public static class FK
		{
			public class Order : POOrder.PK.ForeignKeyOf<POAdjust>.By<adjdOrderType, adjdOrderNbr> { }
		}
		#endregion

		#region IsRequest
		public abstract class isRequest : PX.Data.BQL.BqlBool.Field<isRequest> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Linked to Prepayment", Enabled = false)]
		public virtual bool? IsRequest
		{
			get;
			set;
		}
		#endregion

		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		public const int AdjgDocTypeLength = 3;
		[PXDBString(AdjgDocTypeLength, IsKey = true, IsFixed = true)]
		[APPaymentType.List()]
		[PXDefault(typeof(APPayment.docType))]
		[PXUIField(DisplayName = "Doc. Type", Enabled = false, Visible = false)]
		public virtual String AdjgDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		public const int AdjgRefNbrLength = 15;
		[PXDBString(AdjgRefNbrLength, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(APPayment.refNbr), DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false, Visible = false)]
		[PXParent(typeof(Select<APPayment, Where<APPayment.docType, Equal<Current<POAdjust.adjgDocType>>, And<APPayment.refNbr, Equal<Current<POAdjust.adjgRefNbr>>>>>))]
		public virtual String AdjgRefNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjdOrderType
		public abstract class adjdOrderType : PX.Data.BQL.BqlString.Field<adjdOrderType> { }
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(POOrderType.RegularOrder)]
		[PXStringListAttribute(new string[] { POOrderType.RegularOrder, POOrderType.DropShip },
			new string[] { Messages.RegularOrder, Messages.DropShip })]
		[PXUIField(DisplayName = "PO Type")]
		public virtual String AdjdOrderType
		{
			get;
			set;
		}
		#endregion
		#region AdjdOrderNbr
		public abstract class adjdOrderNbr : PX.Data.BQL.BqlString.Field<adjdOrderNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXParent(typeof(FK.Order))]
		[PXSelector(typeof(Search<POOrder.orderNbr,
			Where<POOrder.orderType, Equal<Current<adjdOrderType>>,
				And<Where<
					Current<isRequest>, Equal<True>,
					Or<Current<released>, Equal<True>,
					Or<Where<POOrder.status, In3<POOrderStatus.open, POOrderStatus.completed>,
						And<POOrder.curyUnprepaidTotal, Greater<decimal0>>>>>>>>>),
				typeof(POOrder.orderNbr),
				typeof(POOrder.orderDate),
				typeof(POOrder.status),
				typeof(POOrder.curyUnprepaidTotal),
				typeof(POOrder.curyLineTotal),
				typeof(POOrder.curyID),
				Filterable = true)]
		public virtual String AdjdOrderNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(APPayment.adjCntr))]
		public virtual int? AdjNbr
		{
			get;
			set;
		}
		#endregion
		#region APDocType
		public const string EmptyApDocType = "---"; // We can't put string.Empty because DB type is char (fixed size) and length should be 3 symbols
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType>
		{
		}
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(EmptyApDocType, PersistingCheck = PXPersistingCheck.Null)]
		[APDocType.List]
		[PXUIField(DisplayName = "Doc. Type")]
		public virtual string AdjdDocType
		{
			get;
			set;
		}
		#endregion
		#region APRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr>
		{
		}
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<Current<adjdDocType>>>>), DirtyRead = true)]
		public virtual string AdjdRefNbr
		{
			get;
			set;
		}
		#endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion

		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Voided", Enabled = false)]
		public virtual bool? Voided
		{
			get;
			set;
		}
		#endregion

		#region AdjgCuryInfoID
		public abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(APPayment.curyInfoID), CuryIDField = "AdjgCuryID")]
		public virtual Int64? AdjgCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
		[PXDBCurrency(typeof(POAdjust.adjgCuryInfoID), typeof(POAdjust.adjgAmt))]
		[PXUIField(DisplayName = "Applied To Order")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Switch<Case<Where<isRequest, NotEqual<True>>, curyAdjgAmt>, decimal0>), typeof(SumCalc<APPayment.curyPOApplAmt>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<isRequest, NotEqual<True>, And<released, NotEqual<True>>>, curyAdjgAmt>, decimal0>), typeof(SumCalc<APPayment.curyPOUnreleasedApplAmt>))]
		[PXUnboundFormula(typeof(Add<curyAdjgAmt, curyAdjgBilledAmt>), typeof(SumCalc<APPayment.curyPOFullApplAmt>))]
		public virtual Decimal? CuryAdjgAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjgAmt
		public abstract class adjgAmt : PX.Data.BQL.BqlDecimal.Field<adjgAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjgAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjgBilledAmt
		public abstract class curyAdjgBilledAmt : Data.BQL.BqlDecimal.Field<curyAdjgBilledAmt>
		{
		}
		[PXDBCurrency(typeof(adjgCuryInfoID), typeof(adjgBilledAmt))]
		[PXUIField(DisplayName = "Transferred to Bill", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryAdjgBilledAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjgBilledAmt
		public abstract class adjgBilledAmt : Data.BQL.BqlDecimal.Field<adjgBilledAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? AdjgBilledAmt
		{
			get;
			set;
		}
		#endregion

		#region AdjdCuryInfoID
		public abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = GL.BatchModule.PO, CuryIDField = "AdjdCuryID")]
		public virtual Int64? AdjdCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjdAmt
		public abstract class curyAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjdAmt
		public abstract class adjdAmt : PX.Data.BQL.BqlDecimal.Field<adjdAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjdAmt
		{
			get;
			set;
		}
		#endregion


		#region IAdjustmentStub members
		#region StubNbr
		public abstract class stubNbr : PX.Data.BQL.BqlString.Field<stubNbr> { }
		[PXDBString(40, IsUnicode = true)]
		public string StubNbr
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[PXDBInt]
		public int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true)]
		public string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region Unbound
		#region ForceDelete
		public abstract class forceDelete : PX.Data.BQL.BqlBool.Field<forceDelete> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? ForceDelete
		{
			get;
			set;
		}
		#endregion
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the adjustment.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote()]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region IAdjustmentStub
		public bool Persistent => true;
		public decimal? CuryAdjgDiscAmt => 0;
		public decimal? CuryOutstandingBalance => null;
		public DateTime? OutstandingBalanceDate => null;
		#endregion
	}
}
