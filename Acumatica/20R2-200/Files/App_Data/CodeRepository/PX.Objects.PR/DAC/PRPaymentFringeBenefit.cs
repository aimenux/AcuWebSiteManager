using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRPaymentFringeBenefit)]
	[Serializable]
	public class PRPaymentFringeBenefit : IBqlTable
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual int? RecordID { get; set; }
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Doc. Type", Visible = false)]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref. Number", Visible = false)]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<docType>>, And<PRPayment.refNbr, Equal<Current<refNbr>>>>>))]
		public string RefNbr { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[CertifiedProject(DisplayName = "Project")]
		[PXDefault]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		public int? ProjectID { get; set; }
		#endregion
		#region LaborItemID
		public abstract class laborItemID : PX.Data.BQL.BqlInt.Field<laborItemID> { }
		[PMLaborItem(
			typeof(projectID),
			null,
			typeof(SelectFrom<EPEmployee>
				.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<docType.FromCurrent>
					.And<PRPayment.refNbr.IsEqual<refNbr.FromCurrent>>>
				.Where<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID>>))]
		[PXForeignReference(typeof(Field<laborItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LaborItemID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[ProjectTaskNoDefault(typeof(projectID))]
		[PXCheckUnique(typeof(docType), typeof(refNbr), typeof(projectID), typeof(laborItemID))]
		public virtual int? ProjectTaskID { get; set; }
		#endregion
		#region ApplicableHours
		public abstract class applicableHours : PX.Data.BQL.BqlDecimal.Field<applicableHours> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Applicable Hours")]
		public virtual decimal? ApplicableHours { get; set; }
		#endregion
		#region ProjectHours
		public abstract class projectHours : PX.Data.BQL.BqlDecimal.Field<projectHours> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Project Hours")]
		public virtual decimal? ProjectHours { get; set; }
		#endregion
		#region FringeRate
		public abstract class fringeRate : PX.Data.BQL.BqlDecimal.Field<fringeRate> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Fringe Rate")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FringeRate { get; set; }
		#endregion
		#region ReducingRate
		public abstract class reducingRate : PX.Data.BQL.BqlDecimal.Field<reducingRate> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Benefit Rate Reducing the Fringe Rate")]
		public virtual decimal? ReducingRate { get; set; }
		#endregion
		#region PaidFringeAmount
		public abstract class paidFringeAmount : PX.Data.BQL.BqlDecimal.Field<paidFringeAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Paid Fringe Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Mult<calculatedFringeRate, applicableHours>))]
		public virtual decimal? PaidFringeAmount { get; set; }
		#endregion
		#region FringeAmountInBenefit
		public abstract class fringeAmountInBenefit : PX.Data.BQL.BqlDecimal.Field<fringeAmountInBenefit> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FringeAmountInBenefit { get; set; }
		#endregion

		#region CalculatedFringeRate
		public abstract class calculatedFringeRate : PX.Data.BQL.BqlDecimal.Field<calculatedFringeRate> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Calculated Fringe Rate")]
		[PXFormula(typeof(Switch<Case<Where<Sub<fringeRate, reducingRate>, Greater<decimal0>>, Sub<fringeRate, reducingRate>>, decimal0>))]
		public virtual decimal? CalculatedFringeRate { get; set; }
		#endregion

		#region System columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
