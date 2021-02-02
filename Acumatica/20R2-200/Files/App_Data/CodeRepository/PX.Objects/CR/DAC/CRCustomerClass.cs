using PX.SM;
using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	/// <exclude/>
	[PXCacheName(Messages.CustomerClass)]
	[Serializable]
	[PXPrimaryGraph(typeof(CRCustomerClassMaint))]
	public partial class CRCustomerClass : CRBaseClass, PX.Data.IBqlTable
	{
		#region CRCustomerClassID
		public abstract class cRCustomerClassID : PX.Data.BQL.BqlString.Field<cRCustomerClassID> { }

		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
		[PXUIField(DisplayName = "Business Account Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String CRCustomerClassID { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(250, IsUnicode = true)]
		public virtual String Description { get; set; }
		#endregion
		#region DefaultOwner
		public abstract class defaultOwner : PX.Data.BQL.BqlInt.Field<defaultOwner> { }

		[PXDBString]
		[PXUIField(DisplayName = "Default Owner")]
		[CRDefaultOwner]
		public override string DefaultOwner { get; set; }
		#endregion
		#region DefaultAssignmentMapID
		public abstract class defaultAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultAssignmentMapID> { }

		[PXDBInt]
		[PXSelector(typeof(Search<
				EPAssignmentMap.assignmentMapID,
				Where<
					EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeProspect>>>),
			typeof(EPAssignmentMap.assignmentMapID),
			typeof(EPAssignmentMap.name),
			DescriptionField = typeof(EPAssignmentMap.name)
		)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIEnabled(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIField(DisplayName = "Assignment Map")]
		public override int? DefaultAssignmentMapID { get; set; }
		#endregion
		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.BQL.BqlInt.Field<defaultEMailAccountID> { }

		[PXSelector(typeof(EMailAccount.emailAccountID), typeof(EMailAccount.description), DescriptionField = typeof(EMailAccount.description))]
		[PXUIField(DisplayName = "Default Email Account")]
		[PXDBInt]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion
		#region IsInternal

		public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }
		protected Boolean? _IsInternal;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsInternal
		{
			get
			{
				return this._IsInternal;
			}
			set
			{
				this._IsInternal = value;
			}
		}

		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
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
		#endregion
	}
}