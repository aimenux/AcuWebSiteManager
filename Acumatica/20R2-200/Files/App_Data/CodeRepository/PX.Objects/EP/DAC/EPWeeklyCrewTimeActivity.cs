using PX.Data;
using PX.Objects.PM;
using PX.TM;
using System;

namespace PX.Objects.EP
{
	[Serializable]
	[PXCacheName(Messages.EPWeeklyCrewTimeActivity)]
	public class EPWeeklyCrewTimeActivity : IBqlTable
	{
		#region WorkgroupID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXDefault]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, IsWorkgroupOrSubgroupOfContact<Current<AccessInfo.contactID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual int? WorkgroupID { get; set; }
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		#endregion

		#region Week
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Week")]
		[PXDefault]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		public virtual int? Week { get; set; }
		public abstract class week : PX.Data.BQL.BqlInt.Field<week> { }
		#endregion

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}