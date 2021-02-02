using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRPaymentFringeEarningDecreasingRate)]
	[Serializable]
	public class PRPaymentFringeEarningDecreasingRate : IBqlTable
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
		[CertifiedProject(DisplayName = "Project", Visible = false)]
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
				.Where<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID>>),
			Visible = false)]
		[PXDefault]
		[PXForeignReference(typeof(Field<laborItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LaborItemID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[ProjectTaskNoDefault(typeof(projectID), Visible = false)]
		[PXParent(typeof(Select<PRPaymentFringeBenefit,
			Where<PRPaymentFringeBenefit.docType, Equal<Current<docType>>,
				And<PRPaymentFringeBenefit.refNbr, Equal<Current<refNbr>>,
				And<PRPaymentFringeBenefit.projectID, Equal<Current<projectID>>,
				And<PRPaymentFringeBenefit.laborItemID, Equal<Current<laborItemID>>,
				And<Where<PRPaymentFringeBenefit.projectTaskID, Equal<Current<projectTaskID>>,
					Or<PRPaymentFringeBenefit.projectTaskID, IsNull,
						And<Current<projectTaskID>, IsNull>>>>>>>>>),
			ParentCreate = true)]
		public virtual int? ProjectTaskID { get; set; }
		#endregion
		#region EarningTypeCD
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		[PXDBString(2, IsUnicode = true, InputMask = ">LL")]
		[PXUIField(DisplayName = "Earning Type Code")]
		[PXDefault]
		[PREarningTypeSelector]
		[PXForeignReference(typeof(Field<earningTypeCD>.IsRelatedTo<EPEarningType.typeCD>))]
		[PXCheckUnique(typeof(docType), typeof(refNbr), typeof(projectID), typeof(laborItemID), typeof(projectTaskID))]
		public string EarningTypeCD { get; set; }
		#endregion
		#region ApplicableHours
		public abstract class applicableHours : PX.Data.BQL.BqlDecimal.Field<applicableHours> { }
		[PXDBDecimal]
		[PXDefault]
		[PXUIField(DisplayName = "Applicable Hours")]
		public decimal? ApplicableHours { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault]
		public decimal? Amount { get; set; }
		#endregion
		#region BenefitRate
		public abstract class benefitRate : PX.Data.BQL.BqlDecimal.Field<benefitRate> { }
		[PRCurrency(6)]
		[PXUIField(DisplayName = "Benefit Rate")]
		[PXDefault]
		[EnsureParentPXFormula(
			typeof(Div<amount, applicableHours>),
			typeof(SumCalc<PRPaymentFringeBenefit.reducingRate>),
			new Type[] { typeof(PRPaymentFringeBenefit.docType), typeof(PRPaymentFringeBenefit.refNbr), typeof(PRPaymentFringeBenefit.projectID), typeof(PRPaymentFringeBenefit.laborItemID), typeof(PRPaymentFringeBenefit.projectTaskID) },
			new Type[] { typeof(docType), typeof(refNbr), typeof(projectID), typeof(laborItemID), typeof(projectTaskID) })]
		public decimal? BenefitRate { get; set; }
		#endregion
		#region AnnualHours
		public abstract class annualHours : PX.Data.BQL.BqlInt.Field<annualHours> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Annual Hours")]
		[AnnualBaseForCertified(typeof(annualizationException), AnnualBaseForCertifiedAttribute.BaseRange.Hours)]
		public int? AnnualHours { get; set; }
		#endregion
		#region AnnualHours
		public abstract class annualWeeks : PX.Data.BQL.BqlByte.Field<annualWeeks> { }
		[PXDBByte]
		[PXUIField(DisplayName = "Annual Weeks")]
		[AnnualBaseForCertified(typeof(annualizationException), AnnualBaseForCertifiedAttribute.BaseRange.Weeks)]
		public byte? AnnualWeeks { get; set; }
		#endregion

		#region ActualPayRate
		public abstract class actualPayRate : PX.Data.BQL.BqlDecimal.Field<actualPayRate> { }
		private decimal? _ActualPayRate;
		[PXDBDecimal]
		[PXUIField(DisplayName = "Pay Rate")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? ActualPayRate
		{
			get => _ActualPayRate != 0 ? _ActualPayRate : null;
			set => _ActualPayRate = value;
		}
		#endregion
		#region PrevailingWage
		public abstract class prevailingWage : PX.Data.BQL.BqlDecimal.Field<prevailingWage> { }
		private decimal? _PrevailingWage;
		[PXDBDecimal]
		[PXUIField(DisplayName = "Prevailing Wage")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? PrevailingWage
		{
			get => _PrevailingWage != 0 ? _PrevailingWage : null;
			set => _PrevailingWage = value;
		}
		#endregion

		#region AnnualizationException
		public abstract class annualizationException : PX.Data.BQL.BqlBool.Field<annualizationException> { }
		[PXBool]
		[PXUIField(DisplayName = "Annualization Exception")]
		[AnnualizationException(typeof(projectID))]
		public virtual bool? AnnualizationException { get; set; }
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
