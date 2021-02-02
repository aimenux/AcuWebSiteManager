using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRBatchDeduct)]
	[Serializable]
	public class PRBatchDeduct : IBqlTable
	{
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr))]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<PRBatchDeduct.batchNbr>>>))]
		public string BatchNbr { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Deduction Code", Enabled = false)]
		[PXSelector(typeof(SearchFor<PRDeductCode.codeID>
			.Where<PRDeductCode.isActive.IsEqual<True>
				.And<PRDeductCode.isWorkersCompensation.IsEqual<False>>
				.And<PRDeductCode.isCertifiedProject.IsEqual<False>>
				.And<PRDeductCode.isUnion.IsEqual<False>>>),
			SubstituteKey = typeof(PRDeductCode.codeCD),
			DescriptionField = typeof(PRDeductCode.description))]
		[PXDefault]
		public int? CodeID { get; set; }
		#endregion
		#region IsEnabled
		public abstract class isEnabled : PX.Data.BQL.BqlBool.Field<isEnabled> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Enabled")]
		[PXDefault(true)]
		public bool? IsEnabled { get; set; }
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