using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRDeductCodeBenefitIncreasingWage)]
	[Serializable]
	public class PRDeductCodeBenefitIncreasingWage : IBqlTable
	{
		#region DeductCodeID
		public abstract class deductCodeID : BqlInt.Field<deductCodeID> { }
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<deductCodeID>>>>))]
		public virtual int? DeductCodeID { get; set; }
		#endregion
		#region ApplicableBenefitCodeID
		public abstract class applicableBenefitCodeID : BqlInt.Field<applicableBenefitCodeID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Benefit Code")]
		[PXSelector(typeof(SearchFor<PRDeductCode.codeID>
			.Where<PRDeductCode.contribType.IsNotEqual<ContributionType.employeeDeduction>
				.And<PRDeductCode.affectsTaxes.IsEqual<True>>>),
			SubstituteKey = typeof(PRDeductCode.codeCD),
			DescriptionField = typeof(PRDeductCode.description))]
		[PXForeignReference(typeof(Field<applicableBenefitCodeID>.IsRelatedTo<PRDeductCode.codeID>))]
		public virtual int? ApplicableBenefitCodeID { get; set; }
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
