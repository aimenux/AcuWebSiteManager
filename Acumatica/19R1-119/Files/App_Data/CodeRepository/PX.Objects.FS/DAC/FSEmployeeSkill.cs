using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.FS
{
	[System.SerializableAttribute]
	public class FSEmployeeSkill : PX.Data.IBqlTable
	{
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee ID")]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<FSEmployeeSkill.employeeID>>>>))]
		[PXDBLiteDefault(typeof(EPEmployee.bAccountID))]
        public virtual int? EmployeeID { get; set; }
		#endregion
		#region SkillID
		public abstract class skillID : PX.Data.BQL.BqlInt.Field<skillID> { }

		[PXDBInt(IsKey = true)]
        [PXDefault]
		[PXUIField(DisplayName = "Skill ID")]
		[PXSelector(typeof(FSSkill.skillID), SubstituteKey = typeof(FSSkill.skillCD), DescriptionField = typeof(FSSkill.descr))]
        public virtual int? SkillID { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXUIField(DisplayName = "CreatedByID")]
		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXUIField(DisplayName = "CreatedByScreenID")]
		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXUIField(DisplayName = "CreatedDateTime")]
		[PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXUIField(DisplayName = "LastModifiedByID")]
		[PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXUIField(DisplayName = "LastModifiedByScreenID")]
		[PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXUIField(DisplayName = "LastModifiedDateTime")]
		[PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
		#endregion
	}
}
