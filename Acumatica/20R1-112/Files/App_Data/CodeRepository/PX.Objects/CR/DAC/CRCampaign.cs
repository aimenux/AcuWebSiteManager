using PX.Objects.PM;

namespace PX.Objects.CR
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.CS;
	using PX.Data.EP;
	using AR;
	using AP;
	using GL;
	using TM;
    using SO;

    [PXCacheName(Messages.Campaign)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(CampaignMaint))]
	[PXCopyPasteHiddenFields(typeof(projectID), typeof(projectTaskID))]
	public partial class CRCampaign : PX.Data.IBqlTable, INotable
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
		#region CampaignID
		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
		protected String _CampaignID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = Messages.CampaignID, Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(CRSetup.campaignNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
        [PXFieldDescription]
		public virtual String CampaignID
		{
			get
			{
				return this._CampaignID;
			}
			set
			{
				this._CampaignID = value;
			}
		}
		#endregion
		#region CampaignName
		public abstract class campaignName : PX.Data.BQL.BqlString.Field<campaignName> { }
		protected String _CampaignName;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Campaign Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXNavigateSelector(typeof(CRCampaign.campaignName))]
        [PXFieldDescription]
		public virtual String CampaignName
		{
			get
			{
				return this._CampaignName;
			}
			set
			{
				this._CampaignName = value;
			}
		}
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBText(IsUnicode = true)]
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
                _plainText = null;
            }
        }
        #endregion

        #region DescriptionAsPlainText
        public abstract class descriptionAsPlainText : PX.Data.BQL.BqlString.Field<descriptionAsPlainText> { }

        private string _plainText;
        [PXString(IsUnicode = true)]
        [PXUIField(Visible = false)]
        public virtual String DescriptionAsPlainText
        {
            get
            {
                return _plainText ?? (_plainText = PX.Data.Search.SearchService.Html2PlainText(this.Description));
            }
        }
        #endregion

        #region CampaignType
        public abstract class campaignType : PX.Data.BQL.BqlString.Field<campaignType> { }
		protected String _CampaignType;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Campaign Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CRCampaignType.typeID), DescriptionField = typeof(CRCampaignType.description))]
		public virtual String CampaignType
		{
			get
			{
				return this._CampaignType;
			}
			set
			{
				this._CampaignType = value;
			}
		}
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }

		[CRAttributesField(typeof(CRCampaign.campaignType))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Stage", Visibility = PXUIVisibility.SelectorVisible)]
        [PXStringListAttribute(new string[0], new string[0])]
        public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
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
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
        #endregion

        #region ExpectedRevenue
        public abstract class expectedRevenue : PX.Data.BQL.BqlDecimal.Field<expectedRevenue> { }
        protected Decimal? _ExpectedRevenue;
        [PXDBBaseCury()]
        [PXUIField(DisplayName = "Expected Return")]
        public virtual Decimal? ExpectedRevenue
        {
            get
            {
                return this._ExpectedRevenue;
            }
            set
            {
                this._ExpectedRevenue = value;
            }
        }
        #endregion        

        #region PlannedBudget
        public abstract class plannedBudget : PX.Data.BQL.BqlDecimal.Field<plannedBudget> { }
		protected Decimal? _PlannedBudget;
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Planned Budget")]
		public virtual Decimal? PlannedBudget
		{
			get
			{
				return this._PlannedBudget;
			}
			set
			{
				this._PlannedBudget = value;
			}
		}
		#endregion		

		#region ExpectedResponse
		public abstract class expectedResponse : PX.Data.BQL.BqlInt.Field<expectedResponse> { }
		protected Int32? _ExpectedResponse;
		[PXDBInt()]
		[PXUIField(DisplayName = "Expected Response")]
		public virtual Int32? ExpectedResponse
		{
			get
			{
				return this._ExpectedResponse;
			}
			set
			{
				this._ExpectedResponse = value;
			}
		}
		#endregion
		#region MailsSent
		public abstract class mailsSent : PX.Data.BQL.BqlInt.Field<mailsSent> { }
		protected Int32? _MailsSent;
		[PXDBInt()]
		[PXUIField(DisplayName = "Mails Sent")]
		public virtual Int32? MailsSent
		{
			get
			{
				return this._MailsSent;
			}
			set
			{
				this._MailsSent = value;
			}
		}
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXDBGuid()]
		[PXOwnerSelector(typeof(CRCampaign.workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		public Guid? OwnerID { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		public Int32? WorkgroupID { get; set; }		
		#endregion

        #region PromoCodeID
        public abstract class promoCodeID : PX.Data.BQL.BqlString.Field<promoCodeID> { }
		protected String _PromoCodeID;
		[PXDBString(30, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Promo Code", Required = false)]
		public virtual String PromoCodeID
		{
			get
			{
				return this._PromoCodeID;
			}
			set
			{
				this._PromoCodeID = value;
			}
        }
        #endregion

        #region SendFilter
        public abstract class sendFilter : PX.Data.BQL.BqlString.Field<sendFilter> { }
		protected String _SendFilter = SendFilterSourcesAttribute._NEVERSENT;
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Sent Emails Filter")]
		[PXDefault(SendFilterSourcesAttribute._NEVERSENT)]        
        [SendFilterSources]
        public virtual String SendFilter
		{
			get
			{
				return this._SendFilter;
			}
			set
			{
				this._SendFilter = value;
			}
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXRestrictor(typeof(Where<PMProject.visibleInCR, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBase]
		[PXUIField(DisplayName = "Project ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt()]
		[PXUIField(DisplayName = "Project Task ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDimensionSelector(
			ProjectTaskAttribute.DimensionName,
			typeof(Search2<PMTask.taskID,
			LeftJoin<CRCampaign, 
				On<CRCampaign.projectID, Equal<PMTask.projectID>, And<CRCampaign.projectTaskID, Equal<PMTask.taskID>>>>,
			Where<PMTask.projectID, Equal<Current<CRCampaign.projectID>>,
				And<CRCampaign.campaignID, IsNull, Or<PMTask.taskID, Equal<Current<CRCampaign.projectTaskID>>>>>>),
			typeof(PMTask.taskCD),
			typeof(PMTask.taskCD),
			typeof(PMTask.description),
			typeof(PMTask.status),
			DescriptionField = typeof(PMTask.description)
			)]
		public virtual Int32? ProjectTaskID { get; set; }
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
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
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
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
        [PXNote(
        DescriptionField = typeof(CRCampaign.campaignID),
        Selector = typeof(CRCampaign.campaignID),
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
	}
	public class CRSystemCampaignMemberStatusCodes
	{
		public class Sent : PX.Data.BQL.BqlString.Constant<Sent>
		{
			public Sent()
				: base("S")
			{
			}
		}
		public class Responsed : PX.Data.BQL.BqlString.Constant<Responsed>
		{
			public Responsed()
				: base("P")
			{
			}
		}
	}
	public class CRSystemLeadStatusCodes
	{
		public class Converted : PX.Data.BQL.BqlString.Constant<Converted>
		{
			public Converted()
				: base("C")
			{
			}
		}
	}
}
