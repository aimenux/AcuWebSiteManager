using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;

namespace PX.Objects.AP
{
	[Serializable]
	[PXCacheName(Messages.APPrintCheckDetail)]
	public class APPrintCheckDetail : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<APPrintCheckDetail>.By<adjgDocType, adjgRefNbr, source, adjdDocType, adjdRefNbr>
		{
			public static APPrintCheckDetail Find(PXGraph graph, string adjgDocType, string adjgRefNbr, string source, string adjdDocType, string adjdRefNbr)
				=> FindBy(graph, adjgDocType, adjgRefNbr, source, adjdDocType, adjdRefNbr);
		}
		#endregion

		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType>
		{
			public const int Length = 3;
		}
		[PXDBString(adjgDocType.Length, IsKey = true, IsFixed = true)]
		[APPaymentType.List()]
		[PXDBDefault(typeof(APPayment.docType))]
		public virtual String AdjgDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr>
		{
			public const int Length = 15;
		}
		[PXDBString(adjgRefNbr.Length, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(APPayment.refNbr))]
		[PXParent(typeof(Select<APPayment, Where<APPayment.docType, Equal<Current<APPrintCheckDetail.adjgDocType>>, And<APPayment.refNbr, Equal<Current<APPrintCheckDetail.adjgRefNbr>>>>>))]
		public virtual String AdjgRefNbr
		{
			get;
			set;
		}
		#endregion
		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }
		[PXDBString(1, IsKey = true, IsFixed = true)]
		public string Source { get; set; }
		#endregion
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType>
		{
			public const int Length = 3;
		}
		private string _adjdDocType;
		[PXDBString(adjdDocType.Length, IsKey = true, IsFixed = true)]
		[APPaymentType.List()]
		[PXDBDefault(typeof(APPayment.docType))]
		public virtual String AdjdDocType
		{
			get => _adjdDocType;
			set
			{
				_adjdDocType = value;
				while (_adjdDocType?.Length < adjdDocType.Length)
				{
					_adjdDocType += ' '; // correct work with fixed length (for find in cache by key)
				}
			}
		}
		#endregion
		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr>
		{
			public const int Length = 15;
		}
		[PXDBString(adjdRefNbr.Length, IsUnicode = true, IsKey = true)]
		public virtual String AdjdRefNbr
		{
			get;
			set;
		}
		#endregion

		#region StubNbr
		public abstract class stubNbr : PX.Data.BQL.BqlString.Field<stubNbr> { }
		[PXDBString(40, IsUnicode = true)]
		public string StubNbr { get; set; }
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[PXDBInt]
		public int? CashAccountID { get; set; }
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true)]
		public string PaymentMethodID { get; set; }
		#endregion

		#region AdjgCuryInfoID
		public abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = GL.BatchModule.PO, CuryIDField = "AdjgCuryID")]
		public virtual Int64? AdjgCuryInfoID
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

		#region CuryOutstandingBalance
		public abstract class curyOutstandingBalance : PX.Data.BQL.BqlDecimal.Field<curyOutstandingBalance> { }
		[PXDBCurrency(typeof(adjgCuryInfoID), typeof(APPrintCheckDetail.outstandingBalance))]
		public decimal? CuryOutstandingBalance { get; set; }
		#endregion
		#region OutstandingBalance
		public abstract class outstandingBalance : PX.Data.BQL.BqlDecimal.Field<outstandingBalance> { }
		[PXDBDecimal(4)]
		public decimal? OutstandingBalance { get; set; }
		#endregion
		#region OutstandingBalanceDate
		public abstract class outstandingBalanceDate : PX.Data.BQL.BqlDateTime.Field<outstandingBalanceDate> { }
		[PXDBDate()]
		public DateTime? OutstandingBalanceDate { get; set; }
		#endregion

		#region CuryAdjgAmt
		public abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
		[PXDBCurrency(typeof(adjgCuryInfoID), typeof(adjgAmt))]
		public virtual decimal? CuryAdjgAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjgAmt
		public abstract class adjgAmt : PX.Data.BQL.BqlDecimal.Field<adjgAmt> { }
		[PXDBDecimal(4)]
		public virtual decimal? AdjgAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryAdjgDiscAmt
		public abstract class curyAdjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgDiscAmt> { }
		[PXDBCurrency(typeof(adjgCuryInfoID), typeof(adjgDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjgDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjgDiscAmt
		public abstract class adjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<adjgDiscAmt> { }
		[PXDBDecimal(4)]
		public virtual Decimal? AdjgDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region AdjdCuryDocBal
		public abstract class curyExtraDocBal : PX.Data.BQL.BqlDecimal.Field<curyExtraDocBal> { }
		[PXDBCurrency(typeof(adjdCuryInfoID), typeof(extraDocBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryExtraDocBal
		{
			get;
			set;
		}
		#endregion
		#region AdjdDocBal
		public abstract class extraDocBal : PX.Data.BQL.BqlDecimal.Field<extraDocBal> { }
		[PXDBDecimal(4)]
		public virtual Decimal? ExtraDocBal
		{
			get;
			set;
		}
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
	}
}
