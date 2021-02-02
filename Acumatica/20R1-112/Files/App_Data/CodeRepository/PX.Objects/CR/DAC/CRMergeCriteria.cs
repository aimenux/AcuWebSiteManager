using System;
using PX.Data;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.Criterion)]
	[PXHidden]
	public partial class CRMergeCriteria : IBqlTable
	{
		#region MergeID

		public abstract class mergeID : PX.Data.BQL.BqlInt.Field<mergeID> { }

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(CRMerge.mergeID))]
		[PXUIField(Visible = false)]
		public virtual Int32? MergeID { get; set; }

		#endregion

		#region LineNbr

		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false)]
		[RowNbr]
		public virtual Int32? LineNbr { get; set; }

		#endregion

		#region OpenBrackets
		public abstract class openBrackets : PX.Data.BQL.BqlInt.Field<openBrackets> { }

		[PXDefault(0)]
		[PXDBInt]
		[PXIntList(new[] { 0, 1, 2, 3, 4, 5 }, new[] { " ", "(", "((", "(((", "((((", "(((((" })]
		[PXUIField(DisplayName = "Brackets")]
		public virtual Int32? OpenBrackets { get; set; }
		#endregion

		#region DataField
		public abstract class dataField : PX.Data.BQL.BqlString.Field<dataField> { }

		[PXDefault]
		[PXDBString(50)]
		[PXUIField(DisplayName = "Property")]
		public virtual String DataField { get; set; }
		#endregion

		#region Matching
		public abstract class matching : PX.Data.BQL.BqlInt.Field<matching> { }

		[PXDBInt]
		[PXDefault(MergeMatchingTypesAttribute._THE_SAME)]
		[MergeMatchingTypes]
		[PXUIField(DisplayName = "Matching")]
		public virtual Int32? Matching { get; set; }
		#endregion

		#region Value
		public abstract class value : PX.Data.BQL.BqlByteArray.Field<value> { }

		[PXDBVariant]
		[PXUIField(DisplayName = "Value")]
		public virtual Byte[] Value { get; set; }
		#endregion

		#region CloseBrackets
		public abstract class closeBrackets : PX.Data.BQL.BqlInt.Field<closeBrackets> { }

		[PXDefault(0)]
		[PXDBInt]
		[PXIntList(new[] { 0, 1, 2, 3, 4, 5 }, new[] { " ", ")", "))", ")))", "))))", ")))))" })]
		[PXUIField(DisplayName = "Brackets")]
		public virtual Int32? CloseBrackets { get; set; }
		#endregion

		#region Operator
		public abstract class @operator : PX.Data.BQL.BqlInt.Field<@operator> { }

		[PXDefault(0)]
		[PXDBInt]
		[PXIntList(new [] {0, 1}, new [] { "Or", "And" })]
		[PXUIField(DisplayName = "Operator")]
		public virtual Int32? Operator { get; set; }
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
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "Date Reported", Enabled = false)]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "Last Activity", Enabled = false)]
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
}
