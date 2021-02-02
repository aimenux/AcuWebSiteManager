using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[Serializable]
	[PXCacheName(Messages.EPRuleEmployeeCondition)]
	[PXTable]
	public class EPRuleEmployeeCondition : EPRuleBaseCondition, IBqlTable
	{
		#region RuleID
		public abstract class ruleID : PX.Data.BQL.BqlGuid.Field<ruleID> { }

		[PXDBGuid(IsKey = true)]
		[PXUIField(DisplayName = "Rule ID")]
		[PXParent(typeof(Select<EPRule, Where<EPRule.ruleID, Equal<Current<EPRuleEmployeeCondition.ruleID>>>>))]
		public override Guid? RuleID { get; set; }
		#endregion
		#region RowNbr
		public abstract class rowNbr : PX.Data.BQL.BqlShort.Field<rowNbr> { }
		
		[PXDBShort(IsKey = true)]
		[PXDefault]
		[RowNbr]
		public virtual short? RowNbr { get; set; }
		#endregion
		
		#region OpenBrackets
		public abstract class openBrackets : PX.Data.BQL.BqlInt.Field<openBrackets> { }

		[PXDBInt]
		[PXIntList(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new string[] { "-", "(", "((", "(((", "((((", "(((((", "Activity Exists (", "Activity Not Exists (" })]
		[PXUIField(DisplayName = "Brackets")]
		[PXDefault(0)]
		public override int? OpenBrackets { get; set; }
		#endregion
		#region Entity
		public abstract class entity : PX.Data.BQL.BqlString.Field<entity> { }

		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[EPRuleEmployeeConditionEntity.List()]
		[PXDefault]
		[PXUIField(DisplayName = "Entity", Required = true)]
		public override string Entity { get; set; }
		#endregion
		#region FieldName
		public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }

		[PXDBString(FieldLength)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Field Name", Required = true)]
		public override string FieldName { get; set; }
		#endregion
		#region Condition
		public abstract class condition : PX.Data.BQL.BqlInt.Field<condition> { }

		[PXDBInt]
		[PXDefault((int)PXCondition.EQ)]
		[PXUIField(DisplayName = "Condition")]
		[EPConditionType]
		public override int? Condition { get; set; }
		#endregion
		#region IsRelative
		public abstract class isRelative : PX.Data.BQL.BqlBool.Field<isRelative> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Relative")]
		public override bool? IsRelative { get; set; }
		#endregion
		#region IsField
		public abstract class isField : PX.Data.BQL.BqlBool.Field<isField> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "From Doc.")]
		public override bool? IsField { get; set; }
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

		[PXDBString(128, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		public override string Value { get; set; }
		#endregion
		#region Value2
		public abstract class value2 : PX.Data.BQL.BqlString.Field<value2> { }

		[PXDBString(128, IsUnicode = true)]
		[PXUIField(DisplayName = "Value 2")]
		public override string Value2 { get; set; }
		#endregion
		#region CloseBrackets
		public abstract class closeBrackets : PX.Data.BQL.BqlInt.Field<closeBrackets> { }

		[PXDBInt]
		[PXIntList(new int[] { 0, 1, 2, 3, 4, 5 }, new string[] { "-", ")", "))", ")))", "))))", ")))))" })]
		[PXUIField(DisplayName = "Brackets")]
		[PXDefault(0)]
		public override int? CloseBrackets { get; set; }
		#endregion
		#region Operator
		public abstract class operatoR : PX.Data.BQL.BqlInt.Field<operatoR> { }

		[PXDBInt]
		[PXIntList(new int[] { 0, 1 }, new string[] { "And", "Or" })]
		[PXUIField(FieldName = "Operator")]
		[PXDefault(0)]
		public override int? Operator { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}

	public class EPRuleEmployeeConditionEntity
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[]
					{
						typeof(EPEmployee).FullName,
						typeof(Address).FullName,
						typeof(Contact).FullName
					},
					new string[]
					{
						typeof(EPEmployee).Name,
						typeof(Address).Name,
						typeof(Contact).Name
					})
			{
			}
		}
	}

	public class EPConditionType : PXIntListAttribute
	{
		public EPConditionType()
			: base
			(
				new int[]
				{
					(int)PXCondition.EQ,
					(int)PXCondition.NE,
					(int)PXCondition.GT,
					(int)PXCondition.GE,
					(int)PXCondition.LT,
					(int)PXCondition.LE,
					(int)PXCondition.BETWEEN,
					(int)PXCondition.LIKE,
					(int)PXCondition.NOTLIKE,
					(int)PXCondition.RLIKE,
					(int)PXCondition.LLIKE,
					(int)PXCondition.NOTLIKE,
					(int)PXCondition.ISNULL,
					(int)PXCondition.ISNOTNULL
				},
				new string[]
				{
					InfoMessages.EqualsTo,
					InfoMessages.NotEqualsTo,
					InfoMessages.GreaterThan,
					InfoMessages.GreateThanOrEqualsTo,
					InfoMessages.LessThan,
					InfoMessages.LessThanOrEqualsTo,
					InfoMessages.Between,
					InfoMessages.Like,
					InfoMessages.NotLike,
					InfoMessages.RightLike,
					InfoMessages.LeftLike,
					InfoMessages.NotLike,
					InfoMessages.IsNull,
					InfoMessages.IsNotNull
				}
			)
			{ }
	}
}
