using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRPaymentEarning)]
	[Serializable]
	public class PRPaymentEarning : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<docType>>, And<PRPayment.refNbr, Equal<Current<refNbr>>>>>))]
		public String RefNbr { get; set; }
		#endregion
		#region TypeCD
		public abstract class typeCD : BqlString.Field<typeCD> { }
		[PXDBString(2, IsUnicode = true, IsKey = true, InputMask = ">LL")]
		[PXUIField(DisplayName = "Code")]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>), DescriptionField = typeof(EPEarningType.description))]
		[PXForeignReference(typeof(Field<typeCD>.IsRelatedTo<EPEarningType.typeCD>))] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
		public string TypeCD { get; set; }
		#endregion
		#region LocationID
		public abstract class locationID : BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Location")]
		[PXSelector(typeof(PRLocation.locationID), SubstituteKey = typeof(PRLocation.locationCD))]
		//[PXFormula(typeof(Switch<Case<Where<PRPaymentEarning.jobID, IsNotNull>, Selector<PRPaymentEarning.jobID, PRJobCode.locationID>>, Selector<PRPaymentEarning.bAccountID, PREmployee.locationID>>))]
		public int? LocationID { get; set; }
		#endregion
		#region Hours
		public abstract class hours : BqlDecimal.Field<hours> { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? Hours { get; set; }
		#endregion
		#region Rate
		public abstract class rate : BqlDecimal.Field<rate> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Rate")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(amount.Divide<hours>.When<hours.IsNotEqual<decimal0>>.Else<decimal0>))]
		public Decimal? Rate { get; set; }
		#endregion
		#region Amount
		public abstract class amount : BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Ext Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? Amount { get; set; }
		#endregion
		#region MTDAmount
		public abstract class mtdAmount : BqlDecimal.Field<mtdAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "MTD Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? MTDAmount { get; set; }
		#endregion
		#region QTDAmount
		public abstract class qtdAmount : BqlDecimal.Field<qtdAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "QTD Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? QTDAmount { get; set; }
		#endregion
		#region YTDAmount
		public abstract class ytdAmount : BqlDecimal.Field<ytdAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "YTD Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? YTDAmount { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
