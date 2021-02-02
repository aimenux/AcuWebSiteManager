using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PROvertimeRule)]
	[PXPrimaryGraph(typeof(PROvertimeRuleMaint))]
	public class PROvertimeRule : IBqlTable
	{
		#region OvertimeRuleID
		public abstract class overtimeRuleID : BqlString.Field<overtimeRuleID> { }
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Overtime Rule")]
		[PXReferentialIntegrityCheck]
		public virtual string OvertimeRuleID { get; set; }
		#endregion
		#region Description
		public abstract class description : BqlString.Field<description> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description { get; set; }
		#endregion
		#region IsActive //ToDo AC-149516: Check that the Disbursing Earning Type is still correct when the Overtime Rule is re-activated.
		public abstract class isActive : BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region DisbursingTypeCD
		public abstract class disbursingTypeCD : BqlString.Field<disbursingTypeCD> { }
		[PXDBString(2, IsUnicode = true, IsFixed = true)]
		[PXUIField(DisplayName = "Disbursing Earning Type")]
		[PXDefault]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>.
			Where<EPEarningType.isActive.IsEqual<True>.
				And<EPEarningType.isOvertime.IsEqual<True>>>), 
			DescriptionField = typeof(EPEarningType.description))]
		[PXForeignReference(typeof(Field<disbursingTypeCD>.IsRelatedTo<EPEarningType.typeCD>))] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
		public virtual string DisbursingTypeCD { get; set; }
		#endregion
		#region OvertimeMultiplier
		public abstract class overtimeMultiplier : BqlDecimal.Field<overtimeMultiplier> { }
		[PXDecimal]
		[PXFormula(typeof(Selector<disbursingTypeCD, EPEarningType.overtimeMultiplier>))]
		[PXUIField(DisplayName = "Multiplier", Enabled = false)]
		public virtual decimal? OvertimeMultiplier { get; set; }
		#endregion
		#region RuleType
		public abstract class ruleType : BqlString.Field<ruleType> { }
		[PXDBString(3, IsUnicode = false, IsFixed = true, InputMask = ">LLL")]
		[PXDefault]
		[PROvertimeRuleType.List]
		[PXUIField(DisplayName = "Type", Required = true)]
		[PXCheckUnique(typeof(isActive), typeof(weekDay), typeof(overtimeThreshold),
			typeof(state), typeof(unionID), typeof(projectID), ClearOnDuplicate = false)]
		public virtual string RuleType { get; set; }
		#endregion
		#region WeekDay
		public abstract class weekDay : BqlByte.Field<weekDay> { }
		[PXDBByte]
		[EP.DayOfWeek]
		[PXUIField(DisplayName = "Day of Week")]
		[PXUIEnabled(typeof(ruleType.IsEqual<PROvertimeRuleType.daily>))]
		[PXFormula(typeof(Switch<Case<Where<ruleType, NotEqual<PROvertimeRuleType.daily>>, Null>>))]
		public virtual byte? WeekDay { get; set; }
		#endregion
		#region OvertimeThreshold
		public abstract class overtimeThreshold : BqlDecimal.Field<overtimeThreshold> { }
		[PXDefault]
		[PXDBDecimal(2, MinValue = 0, MaxValue = 999.99)]
		[PXUIField(DisplayName = "Threshold for Overtime (hours)", Required = true)]
		public virtual decimal? OvertimeThreshold { get; set; }
		#endregion
		#region DailyThreshold
		public abstract class dailyThreshold : BqlDecimal.Field<dailyThreshold> { }
		[PXDecimal]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(overtimeThreshold.When<ruleType.IsEqual<PROvertimeRuleType.daily>>.Else<overtimeThreshold.Divide<workingDaysPerWeek>>))]
		public virtual decimal? DailyThreshold { get; set; }
		#endregion
		#region Country
		public abstract class countryID : BqlString.Field<countryID> { }
		[PXString(2)]
		// ToDo: AC-138220, In the Payroll Phase 2 review all the places where the country is set to "US" by the default
		[PXUnboundDefault(typeof(LocationConstants.CountryUS))]
		[PXUIField(Visible = false)]
		public virtual string CountryID { get; set; }
		#endregion
		#region State
		public abstract class state : BqlString.Field<state> { }
		[PXDBString(50, IsUnicode = true)]
		[State(typeof(countryID))]
		[PXUIField(DisplayName = "State")]
		public virtual string State { get; set; }
		#endregion
		#region UnionID
		public abstract class unionID : BqlString.Field<unionID> { }
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PMUnion(null, null, FieldClass = null)]
		public virtual string UnionID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : BqlInt.Field<projectID> { }
		[ProjectBase(DisplayName = "Project")]
		public virtual int? ProjectID { get; set; }
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

		private class workingDaysPerWeek : BqlDecimal.Constant<workingDaysPerWeek>
		{
			public workingDaysPerWeek() : base(5m)
			{
			}
		}
	}
}
