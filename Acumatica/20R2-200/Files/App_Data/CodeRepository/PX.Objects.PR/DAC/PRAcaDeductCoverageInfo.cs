using PX.Data;
using PX.Data.BQL.Fluent;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAcaDeductCoverageInfo)]
	[Serializable]
	public sealed class PRAcaDeductCoverageInfo : IBqlTable
	{
		#region DeductCodeID
		public abstract class deductCodeID : PX.Data.BQL.BqlInt.Field<deductCodeID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Code ID")]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<PRAcaDeductCoverageInfo.deductCodeID>>>>))]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		public int? DeductCodeID { get; set; }
		#endregion
		#region CoverageType
		public abstract class coverageType : PX.Data.BQL.BqlString.Field<coverageType> { }
		[PXDBString(3, IsKey = true)]
		[PXUIField(DisplayName = "Coverage Type", Required = true)]
		[AcaCoverageType.List]
		public string CoverageType { get; set; }
		#endregion
		#region HealthPlanType
		public abstract class healthPlanType : PX.Data.BQL.BqlString.Field<healthPlanType> { }
		[PXDBString(3)]
		[PXUIField(DisplayName = "Health Plan Type", Required = true)]
		[AcaHealthPlanType.List]
		public string HealthPlanType { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
