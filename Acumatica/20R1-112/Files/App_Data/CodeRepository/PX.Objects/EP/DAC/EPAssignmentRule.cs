namespace PX.Objects.EP
{
	using System;
	using PX.Data;
	using System.Diagnostics;
	using System.Collections.Generic;
	using System.Linq;

	[System.SerializableAttribute()]
	[DebuggerDisplay("Entity={Entity} FieldName={FieldName} FieldValue={FieldValue}")]
	[PXCacheName(Messages.LegacyAssignmentRule)]
	public partial class EPAssignmentRule : PX.Data.IBqlTable
	{
		public const int FieldLength = 60;

		#region AssignmentRuleID
		public abstract class assignmentRuleID : PX.Data.BQL.BqlInt.Field<assignmentRuleID> { }
		protected Int32? _AssignmentRuleID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? AssignmentRuleID
		{
			get
			{
				return this._AssignmentRuleID;
			}
			set
			{
				this._AssignmentRuleID = value;
			}
		}
		#endregion
		#region AssignmentRouteID
		public abstract class assignmentRouteID : PX.Data.BQL.BqlInt.Field<assignmentRouteID> { }
		protected Int32? _AssignmentRouteID;
		[PXDBInt()]
		[PXDBLiteDefault(typeof(EPAssignmentRoute.assignmentRouteID))]
		[PXParent(typeof(Select<EPAssignmentRoute, Where<EPAssignmentRoute.assignmentRouteID, Equal<Current<EPAssignmentRule.assignmentRouteID>>>>))]
		public virtual Int32? AssignmentRouteID
		{
			get
			{
				return this._AssignmentRouteID;
			}
			set
			{
				this._AssignmentRouteID = value;
			}
		}
		#endregion
		#region Entity
		public abstract class entity : PX.Data.BQL.BqlString.Field<entity> { }
		protected String _Entity;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Entity")]
		public virtual String Entity
		{
			get
			{
				return this._Entity;
			}
			set
			{
				this._Entity = value;
			}
		}
		#endregion
		#region FieldName
		public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }
		protected String _FieldName;
		[PXDBString(FieldLength)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Field Name", Visibility = PXUIVisibility.Visible)]
		[PXFormula(typeof(Default<EPAssignmentRule.entity>))]
		public virtual String FieldName
		{
			get
			{
				return this._FieldName;
			}
			set
			{
				this._FieldName = value;
			}
		}
		#endregion
		#region FieldValue
		public abstract class fieldValue : PX.Data.BQL.BqlString.Field<fieldValue> { }
		protected String _FieldValue;
		[PXDBString(FieldLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Field Value", Visibility = PXUIVisibility.Visible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<EPAssignmentRule.fieldName>))]
		public virtual String FieldValue
		{
			get
			{
				return this._FieldValue;
			}
			set
			{
				this._FieldValue = value;
			}
		}
		#endregion
		#region Condition
		public abstract class condition : PX.Data.BQL.BqlInt.Field<condition> { }
		protected Int32? _Condition;
		[PXDBInt()]
		[PXDefault(0)]
		[RuleConditionsListAttribute]
		[PXUIField(DisplayName = "Condition", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? Condition
		{
			get
			{
				return this._Condition;
			}
			set
			{
				this._Condition = value;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}

	public class RuleConditionsListAttribute : PXIntListAttribute
	{
		public RuleConditionsListAttribute() : base()
		{
			var enums = PXEnumDescriptionAttribute.GetValueNamePairs(typeof(PXCondition), localize: false);
			var conditions = new[]
			{
				PXCondition.EQ,
				PXCondition.NE,
				PXCondition.GT,
				PXCondition.GE,
				PXCondition.LT,
				PXCondition.LE,
				PXCondition.LIKE,
				PXCondition.RLIKE,
				PXCondition.LLIKE,
				PXCondition.NOTLIKE,
				//PXCondition.BETWEEN,
				PXCondition.ISNULL,
				PXCondition.ISNOTNULL,
			};
			this._AllowedValues = conditions.Select(i => (int)i).ToArray();
			this._AllowedLabels = enums.Where(i => conditions.Contains((PXCondition)i.Key)).Select(i => i.Value).ToArray();
			this._NeutralAllowedLabels = this._AllowedLabels.ToArray(); // to make copy
		}
	}
}
