using System;
using PX.Data;

namespace PX.Objects.CR
{	
	[Serializable]
	[PXCacheName(Messages.CaseReference)]
	public partial class CRCaseReference : IBqlTable
	{
		#region ParentCaseID
		public abstract class parentCaseID : PX.Data.BQL.BqlInt.Field<parentCaseID> { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CRCase.caseID))]
		[PXUIField(Visible = false)]
		public virtual Int32? ParentCaseID { get; set; }
		#endregion

		#region ChildCaseID
		public abstract class childCaseID : PX.Data.BQL.BqlInt.Field<childCaseID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Case ID")]
		[PXSelector(typeof(CRCase.caseID), SubstituteKey = typeof(CRCase.caseCD), DescriptionField = typeof(CRCase.subject))]
		public virtual Int32? ChildCaseID { get; set; }
		#endregion

		#region RelationType
		public abstract class relationType : PX.Data.BQL.BqlString.Field<relationType> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Relation Type", Required = true)]
		[CaseRelationType]
		[PXDefault(CaseRelationTypeAttribute._DEPENDS_ON_VALUE)]
		public virtual String RelationType { get; set; }

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
