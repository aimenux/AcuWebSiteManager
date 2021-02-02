using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRBatchOvertimeRule)]
	public class PRBatchOvertimeRule : IBqlTable
	{
		#region BatchNbr
		public abstract class batchNbr : BqlString.Field<batchNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr))]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<batchNbr>>>>))]
		public virtual string BatchNbr { get; set; }
		#endregion
		#region OvertimeRuleID
		public abstract class overtimeRuleID : BqlString.Field<overtimeRuleID> { }
		[PXDBString(30, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Overtime Rule", IsReadOnly = true)]
		[PXForeignReference(typeof(Field<overtimeRuleID>.IsRelatedTo<PROvertimeRule.overtimeRuleID>))]
		public virtual string OvertimeRuleID { get; set; }
		#endregion
		#region RuleType
		public abstract class ruleType : BqlString.Field<ruleType> { }
		[PXString]
		[PXUIField(Visible = false)]
		[PXUnboundDefault(typeof(SearchFor<PROvertimeRule.ruleType>.Where<PROvertimeRule.overtimeRuleID.IsEqual<overtimeRuleID.FromCurrent>>))]
		public virtual string RuleType { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		
		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public virtual byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
