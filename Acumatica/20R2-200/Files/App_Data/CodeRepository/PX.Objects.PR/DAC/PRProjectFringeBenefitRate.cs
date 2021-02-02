using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRProjectFringeBenefitRate)]
	[Serializable]
	public class PRProjectFringeBenefitRate : IBqlTable
	{
		#region RecordID
		public abstract class recordID : BqlInt.Field<recordID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual int? RecordID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : BqlInt.Field<projectID> { }
		[PXDBInt]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<projectID>>>>))]
		public virtual int? ProjectID { get; set; }
		#endregion
		#region LaborItemID
		public abstract class laborItemID : BqlInt.Field<laborItemID> { }
		[PMLaborItem(typeof(projectID), null, null)]
		[PXDefault]
		[PXForeignReference(typeof(Field<laborItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LaborItemID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : BqlInt.Field<projectTaskID> { }
		[ProjectTask(typeof(projectID), AllowNull = true)]
		public virtual int? ProjectTaskID { get; set; }
		#endregion
		#region Rate
		public abstract class rate : BqlDecimal.Field<rate> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Rate")]
		public virtual decimal? Rate { get; set; }
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : BqlDateTime.Field<effectiveDate> { }
		[PXDefault]
		[PXDBDate]
		[PXUIField(DisplayName = "Effective Date")]
		[PXCheckUnique(typeof(projectID), typeof(laborItemID), typeof(projectTaskID), ClearOnDuplicate = false)]
		public virtual DateTime? EffectiveDate { get; set; }
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
