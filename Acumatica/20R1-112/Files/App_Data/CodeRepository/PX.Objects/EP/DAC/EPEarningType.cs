using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.SM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	[PXCacheName(Messages.EarningType)]
	[Serializable]
	public partial class EPEarningType : IBqlTable
	{

		#region TypeCD
		public abstract class typeCD : PX.Data.BQL.BqlString.Field<typeCD> { }
		[PXDefault()]
		[PXDBString(2, IsUnicode = false, IsKey = true, InputMask = ">LL",  IsFixed = true)]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXReferentialIntegrityCheck]
		public virtual String TypeCD { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description { get; set; }
		#endregion

		#region isActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive { get; set; }
		#endregion

		#region isOvertime
		public abstract class isOvertime : PX.Data.BQL.BqlBool.Field<isOvertime> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Overtime")]
		public virtual Boolean? IsOvertime { get; set; }
		#endregion

		#region isBillable
		public abstract class isbillable : PX.Data.BQL.BqlBool.Field<isbillable> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billable")]
		public virtual Boolean? isBillable { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>>), DisplayName = "Default Project Code")]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[PM.ProjectTask(typeof(EPEarningType.projectID), DisplayName = "Default Task", AllowNull = true)]
		public virtual Int32? TaskID { get; set; }
		#endregion

		#region OvertimeMultiplier
		public abstract class overtimeMultiplier : PX.Data.BQL.BqlDecimal.Field<overtimeMultiplier> { }
		[PXDBDecimal(2, MinValue = 0.01, MaxValue = 99.99)]
		[PXUIEnabled(typeof(isOvertime))]
		[PXFormula(typeof(Switch<Case<Where<isOvertime, NotEqual<True>>, CS.decimal1>, overtimeMultiplier>))]
		[PXUIField(DisplayName = "Multiplier")]
		public virtual Decimal? OvertimeMultiplier { get; set; }
		#endregion


		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion

	}
}
