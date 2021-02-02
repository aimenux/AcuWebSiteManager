using System;
using PX.Data;

namespace PX.Objects.CR
{
	[Serializable]
	[PXPrimaryGraph(typeof(MergeMaint))]
	[PXCacheName(Messages.MergeDocument)]
	[PXHidden]
	public partial class CRMerge : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion

		#region MergeID

		public abstract class mergeID : PX.Data.BQL.BqlInt.Field<mergeID> { }

		[PXDBIdentity]
		[PXUIField(Visible = false)]
		public virtual Int32? MergeID { get; set; }

		#endregion

		#region MergeCD

		public abstract class mergeCD : PX.Data.BQL.BqlString.Field<mergeCD> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CRMerge.mergeCD),
			typeof(CRMerge.mergeCD),
			typeof(CRMerge.description),
			Filterable = true)]
		public virtual String MergeCD { get; set; }

		#endregion

		#region EntityType

		public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }

		[PXDBString(250)]
		[PXDefault]
		[PXUIField(DisplayName = "Target Type")]
		[MergableTypesSelector]
		public virtual String EntityType { get; set; }

		#endregion

		#region Description

		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description { get; set; }

		#endregion

		#region RunOnSave

		public abstract class runOnSave : PX.Data.BQL.BqlBool.Field<runOnSave> { }

		[PXDBBool]
		[PXUIField(DisplayName = "On The Fly")]
		public virtual Boolean? RunOnSave { get; set; }

		#endregion

		#region LastRun

		public abstract class lastRun : PX.Data.BQL.BqlDateTime.Field<lastRun> { }

		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Last Run", Enabled = false)]
		public virtual DateTime? LastRun { get; set; }

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
