namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

    /// <summary>
    /// Represents a General Ledger Subaccount.
    /// The records of this type are added and edited through the Subaccounts (GL.20.30.00) screen
    /// (corresponds to the <see cref="SubAccountMaint"/> graph).
    /// </summary>
    [System.SerializableAttribute()]
	[PXCacheName(Messages.Sub)]
	[PXPrimaryGraph(typeof(SubAccountMaint))]
	public partial class Sub : PX.Data.IBqlTable, PX.SM.IIncludable
	{
        #region Keys
        public class PK : PrimaryKeyOf<Sub>.By<subID>
        {
            public static Sub Find(PXGraph graph, int? subID) => FindBy(graph, subID);
        }
        #endregion
        #region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;

        /// <summary>
        /// Database identity.
        /// Unique identifier of the Subaccount.
        /// </summary>
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Sub. ID", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region SubCD
		public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
		protected String _SubCD;

        /// <summary>
        /// Key field.
        /// Unique user-friendly <see cref="CS.Dimension">segmented key</see> of the Subaccount.
        /// </summary>
		[PXDefault()]
		[SubAccountRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String SubCD
		{
			get
			{
				return this._SubCD;
			}
			set
			{
				this._SubCD = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;

        /// <summary>
        /// Indicates whether the Subaccount is <c>active</c>.
        /// Inactive subaccounts do not appear in the lists of available subaccounts and
        /// thus can't be selected for documents, transactions and other entities.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

        /// <summary>
        /// The description of the Subaccount.
        /// </summary>
		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region ConsoSubID
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class consoSubID : PX.Data.BQL.BqlInt.Field<consoSubID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Consolidation Subaccount ID", Visibility = PXUIVisibility.Invisible)]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public virtual int? ConsoSubID
		{
			get;
			set;
		}
		#endregion
		#region ConsoSubCode
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class consoSubCode : PX.Data.BQL.BqlString.Field<consoSubCode> { }

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Consolidation Subaccount Code", Visible=false)]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public virtual string ConsoSubCode
		{
			get;
			set;
		}
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
        protected Byte[] _GroupMask;

        /// <summary>
        /// The group mask showing which <see cref="PX.SM.RelationGroup">restriction groups</see> the Subaccount belongs to.
        /// </summary>
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the Subaccount.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
		[PXNote(DescriptionField = typeof(Sub.description))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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
		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;

        /// <summary>
        /// An unbound field used in the User Interface to include the Subaccount into a <see cref="PX.SM.RelationGroup">restriction group</see>.
        /// </summary>
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		public virtual bool? Included
		{
			get
			{
				return this._Included;
			}
			set
			{
				this._Included = value;
			}
		}
		#endregion

	    public class Keys
	    {
			public Int32? SubID { get; set; }
			public String SubCD { get; set; }
		}
	}
}
