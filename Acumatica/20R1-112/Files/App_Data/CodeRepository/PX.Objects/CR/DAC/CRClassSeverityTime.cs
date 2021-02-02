namespace PX.Objects.CR
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TimeReactionBySeverity)]
	public partial class CRClassSeverityTime : PX.Data.IBqlTable
	{
		#region CaseClassID
		public abstract class caseClassID : PX.Data.BQL.BqlString.Field<caseClassID> { }
		protected String _CaseClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Case Class ID")]
		[PXSelector(typeof(CRCaseClass.caseClassID))]
		public virtual String CaseClassID
		{
			get
			{
				return this._CaseClassID;
			}
			set
			{
				this._CaseClassID = value;
			}
		}
		#endregion
		#region Severity
		public abstract class severity : PX.Data.BQL.BqlString.Field<severity> { }
		protected String _Severity;
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Severity")]
		[PXStringList(new string[] { "H", "L", "M", "U" },
			new string[] { "High", "Low", "Medium", "Urgent" },
			BqlField = typeof(CRCase.severity))]
		public virtual String Severity
		{
			get
			{
				return this._Severity;
			}
			set
			{
				this._Severity = value;
			}
		}
		#endregion
		#region TimeReaction
		public abstract class timeReaction : PX.Data.BQL.BqlInt.Field<timeReaction> { }
		protected Int32? _TimeReaction;
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.DaysHoursMinites)]
        [PXDefault(0)]
		[PXUIField(DisplayName = "Reaction Time")]
		public virtual Int32? TimeReaction
		{
			get
			{
				return this._TimeReaction;
			}
			set
			{
				this._TimeReaction = value;
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
}
