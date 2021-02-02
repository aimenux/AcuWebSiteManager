using System;
using PX.Data;
using PX.Data.EP;
using PX.TM;
using PX.Data.Maintenance.GI;
using PX.Objects.CM;
using PX.SM;

namespace PX.Objects.CR
{
	[Serializable]
	[CRCacheIndependentPrimaryGraph(typeof(CRMarketingListMaint),
		typeof(Select<CRMarketingList, 
			Where<CRMarketingList.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>))]
	[PXCacheName(Messages.MailList)]
	public partial class CRMarketingList : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region MarketingListID
		public abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }
		protected Int32? MarketingListId;
		[PXDBIdentity()]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? MarketingListID
		{
			get
			{
				return this.MarketingListId;
			}
			set
			{
				this.MarketingListId = value;
			}
		}
		#endregion
		#region MailListCode
		public abstract class mailListCode : PX.Data.BQL.BqlString.Field<mailListCode> { }
		protected String _MailListCode;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Marketing List ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector("MLISTCD", typeof(Search<CRMarketingList.marketingListID>), typeof(CRMarketingList.mailListCode))]
        [PXFieldDescription]
        public virtual String MailListCode
		{
			get
			{
				return this._MailListCode;
			}
			set
			{
				this._MailListCode = value;
			}
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		protected String _Name;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "List Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBText(IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description")]
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
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;
		[PXDBGuid()]
		[PXOwnerSelector(typeof(CRMarketingList.workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		public virtual Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region Method

		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }

		private string _method;

		[PXDBString(1, IsFixed = true)]
		[CRContactMethods]
		[PXDefault(CRContactMethodsAttribute.Any)]
		[PXUIField(DisplayName = "Contact Method")]
		public virtual String Method
		{
			get { return _method ?? CRContactMethodsAttribute.Any; }
			set { _method = value; }
		}

		#endregion
		#region IsDynamic

		public abstract class isDynamic : PX.Data.BQL.BqlBool.Field<isDynamic> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Dynamic List", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsDynamic { get; set; }

        #endregion

        #region IsDynamic
        public abstract class isStatic : PX.Data.BQL.BqlBool.Field<isStatic> { }
        [PXBool]
        [PXUIField(DisplayName = "Static List", Visible = false)]
        public virtual Boolean? IsStatic
        {
            get
            {
                return !(IsDynamic == true);
            }            
        }
        #endregion

        #region GIDesignID
        public abstract class gIDesignID : PX.Data.BQL.BqlGuid.Field<gIDesignID> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Generic Inquiry")]
		[ContactGISelector]
		public Guid? GIDesignID { get; set; }
        #endregion      

        #region SharedGIFilter
        public abstract class sharedGIFilter : PX.Data.BQL.BqlInt.Field<sharedGIFilter> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Shared Filter to Apply")]
        [FilterList(typeof(gIDesignID), IsSiteMapIdentityScreenID = false, IsSiteMapIdentityGIDesignID = true)]
        [PXFormula(typeof(Default<gIDesignID>))]
        public virtual int? SharedGIFilter { get; set; }

		#endregion

		#region IsSelectionCriteria
		public abstract class isSelectionCriteria : PX.Data.BQL.BqlBool.Field<isSelectionCriteria> { }

		[PXBool]
		[PXUIField(DisplayName = "Selection Criteria")]
		[PXFormula(typeof(Switch<
			Case<Where<
				isDynamic, Equal<True>, And<gIDesignID, IsNull>>,
				True>,
			False>))]
		public virtual bool? IsSelectionCriteria { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
        [PXNote(
        DescriptionField = typeof(CRMarketingList.mailListCode),
        Selector = typeof(CRMarketingList.marketingListID),
        ShowInReferenceSelector = true)]
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
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
        [PXUIField(DisplayName = "Modified Date")]
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
	}
}
