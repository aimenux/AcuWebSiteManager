using System;
using PX.Data;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	[PXCacheName(Messages.LeadClass)]
	[PXPrimaryGraph(typeof(CRLeadClassMaint))]
	[Serializable]
	public partial class CRContactClass : IBqlTable
	{
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXSelector(typeof(CRContactClass.classID))]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual String ClassID { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(250, IsUnicode = true)]
		public virtual String Description { get; set; }
		#endregion

		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.BQL.BqlInt.Field<defaultEMailAccountID> { }

		[PXSelector(typeof(EMailAccount.emailAccountID), typeof(EMailAccount.description), DescriptionField = typeof(EMailAccount.description))]
		[PXUIField(DisplayName = "Default Email Account")]
		[PXDBInt]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion

		#region DefaultStatus
		public abstract class defaultStatus : PX.Data.BQL.BqlString.Field<defaultStatus> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Default Status")]
		[LeadStatuses]
		public virtual string DefaultStatus { get; set; }
		#endregion

		#region DefaultSource
		public abstract class defaultSource : PX.Data.BQL.BqlString.Field<defaultSource> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Default Source")]
		[CRMSources]
		public virtual string DefaultSource { get; set; }
		#endregion

		#region DefaultWorkgroupID
		public abstract class defaultWorkgroupID : PX.Data.BQL.BqlInt.Field<defaultWorkgroupID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Default Workgroup")]
		[PXCompanyTreeSelector]
		public virtual int? DefaultWorkgroupID { get; set; }
		#endregion

		#region OwnerIsCreatedUser
		public abstract class ownerIsCreatedUser : PX.Data.BQL.BqlBool.Field<ownerIsCreatedUser> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default Owner to Creator")]
		public virtual bool? OwnerIsCreatedUser { get; set; }
		#endregion

		#region DefaultOwnerWorkgroup
		public abstract class defaultOwnerWorkgroup : PX.Data.BQL.BqlBool.Field<defaultOwnerWorkgroup> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default Workgroup to Default Owner Workgroup")]
		[PXFormula(typeof(Switch<Case<Where<CRContactClass.defaultWorkgroupID, IsNotNull>, False>, defaultOwnerWorkgroup>))]
		public virtual bool? DefaultOwnerWorkgroup { get; set; }
		#endregion

		#region OwnerToBAccount
		public abstract class ownerToBAccount : PX.Data.BQL.BqlBool.Field<ownerToBAccount> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Ownership from Lead to Business Account")]
		public virtual bool? OwnerToBAccount { get; set; }
		#endregion

		#region OwnerToOpportunity
		public abstract class ownerToOpportunity : PX.Data.BQL.BqlBool.Field<ownerToOpportunity> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Ownership from Lead to Opportunity")]
		public virtual bool? OwnerToOpportunity { get; set; }
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
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? NoteID { get; set; }
		#endregion

        #region TargetBAccountClassID
        public abstract class targetBAccountClassID : PX.Data.BQL.BqlString.Field<targetBAccountClassID> { }
        protected String _TargetBAccountClassID;
        [PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
        [PXUIField(DisplayName = "Account Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBString(10, IsUnicode = true)]
        public virtual String TargetBAccountClassID
        {
            get
            {
                return this._TargetBAccountClassID;
            }
            set
            {
                this._TargetBAccountClassID = value;
            }
        }
        #endregion

        #region TargetOpportunityClassID
        public abstract class targetOpportunityClassID : PX.Data.BQL.BqlString.Field<targetOpportunityClassID> { }
        protected String _TargetOpportunityClassID;
        [PXSelector(typeof(CROpportunityClass.cROpportunityClassID))]
        [PXUIField(DisplayName = "Opportunity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBString(10, IsUnicode = true)]
        public virtual String TargetOpportunityClassID
        {
            get
            {
                return this._TargetOpportunityClassID;
            }
            set
            {
                this._TargetOpportunityClassID = value;
            }
        }
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
