using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.SO;


namespace PX.Objects.CR.DAC.Standalone
{
	[PXCacheName(Messages.CampaignStats)]
	public partial class CRCampaign : PX.Data.IBqlTable
	{
		#region Selected

		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		protected bool? _Selected = false;

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get { return _Selected; }
			set { _Selected = value; }
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
			get { return this._CampaignID; }
			set { this._CampaignID = value; }
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
			get { return this._CampaignName; }
			set { this._CampaignName = value; }
		}

		#endregion
		#region LeadsGenerated
		public abstract class leadsGenerated : PX.Data.BQL.BqlInt.Field<leadsGenerated> { }
		protected Int32? _LeadsGenerated;
		[PXInt()]
		[PXUIField(DisplayName = "Leads Generated", Enabled = false)]
		[PXDBScalar(typeof(Search4<Contact.contactID,
			            Where<Contact.campaignID, Equal<CRCampaign.campaignID>,
                            And<Contact.contactPriority, GreaterEqual<Zero>>>,
			            Aggregate<Count<Contact.contactID>>>))]		
		public virtual Int32? LeadsGenerated
		{
			get
			{
				return this._LeadsGenerated;
			}
			set
			{
				this._LeadsGenerated = value;
			}
		}
		#endregion
		#region LeadsConverted
		public abstract class leadsConverted : PX.Data.BQL.BqlInt.Field<leadsConverted> { }
		protected Int32? _LeadsConverted;
		[PXInt()]
		[PXUIField(DisplayName = "Leads Converted", Enabled = false)]
		[PXDBScalar(typeof(Search4<Contact.contactID,
			Where<Contact.campaignID, Equal<CRCampaign.campaignID>,
				And<Contact.status, Equal<LeadStatusesAttribute.converted>,
                    And<Contact.contactPriority, GreaterEqual<Zero>>>>,
			Aggregate<Count<Contact.contactID>>>))]
		public virtual Int32? LeadsConverted
		{
			get
			{
				return this._LeadsConverted;
			}
			set
			{
				this._LeadsConverted = value;
			}
		}
		#endregion
		#region Contacts
		public abstract class contacts : PX.Data.BQL.BqlInt.Field<contacts> { }
		protected Int32? _Contacts;
		[PXInt()]
		[PXUIField(DisplayName = "Total Members", Enabled = false)]
		[PXDBScalar(typeof(Search5<CRCampaignMembers.contactID,
		InnerJoin<Contact, On<Contact.contactID, Equal<CRCampaignMembers.contactID>>>,
		Where<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>>,
		Aggregate<Count<CRCampaignMembers.contactID>>>))]

		public virtual Int32? Contacts
		{
			get
			{
				return this._Contacts;
			}
			set
			{
				this._Contacts = value;
			}
		}
		#endregion
		#region MembersContacted
		public abstract class membersContacted : PX.Data.BQL.BqlInt.Field<membersContacted> { }
		protected Int32? _MembersContacted;
		[PXInt()]
		[PXUIField(DisplayName = "Members Contacted", Enabled = false)]
		[PXDBScalar(typeof(Search5<CRCampaignMembers.contactID,
			InnerJoin<Contact, On<Contact.contactID, Equal<CRCampaignMembers.contactID>>>,
			Where<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>,
				And<CRCampaignMembers.activitiesLogged, Greater<Zero>>>,
			Aggregate<Count<CRCampaignMembers.contactID>>>))]
		public virtual Int32? MembersContacted
		{
			get
			{
				return this._MembersContacted;
			}
			set
			{
				this._MembersContacted = value;
			}
		}
		#endregion
		#region MembersResponded
		public abstract class membersResponded : PX.Data.BQL.BqlInt.Field<membersResponded> { }
		protected Int32? _MembersResponded;
		[PXInt()]
		[PXUIField(DisplayName = "Members Responded", Enabled = false)]
		[PXDBScalar(typeof(Search5<CRCampaignMembers.contactID,
			InnerJoin<Contact, On<Contact.contactID, Equal<CRCampaignMembers.contactID>>>,
			Where<CRCampaignMembers.campaignID, Equal<CRCampaign.campaignID>,
				And<CRCampaignMembers.opportunityCreatedCount, Greater<Zero>>>,
			Aggregate<Count<CRCampaignMembers.contactID>>>))]
		public virtual Int32? MembersResponded
		{
			get
			{
				return this._MembersResponded;
			}
			set
			{
				this._MembersResponded = value;
			}
		}
		#endregion
		#region Opportunities
		public abstract class opportunities : PX.Data.BQL.BqlInt.Field<opportunities> { }
		protected Int32? _Opportunities;
		[PXInt()]
		[PXUIField(DisplayName = "Opportunities", Enabled = false)]
		[PXDBScalar(typeof(Search4<CROpportunity.opportunityID,
			Where<CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>>,
			Aggregate<Count<CROpportunity.opportunityID>>>))]
		public virtual Int32? Opportunities
		{
			get
			{
				return this._Opportunities;
			}
			set
			{
				this._Opportunities = value;
			}
		}
		#endregion
		#region ClosedOpportunities
		public abstract class closedOpportunities : PX.Data.BQL.BqlInt.Field<closedOpportunities> { }
		protected Int32? _ClosedOpportunities;
		[PXInt()]
		[PXUIField(DisplayName = "Won Opportunities", Enabled = false)]
		[PXDBScalar(typeof(Search4<CROpportunity.opportunityID,
                Where<CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>,
                And<CROpportunity.status, Equal<CROpportunityStatusCodes.Won>>>,
                Aggregate<Count<CROpportunity.opportunityID>>>))]
		public virtual Int32? ClosedOpportunities
		{
			get
			{
				return this._ClosedOpportunities;
			}
			set
			{
				this._ClosedOpportunities = value;
			}
		}
		#endregion
		#region OpportunitiesValue
		public abstract class opportunitiesValue : PX.Data.BQL.BqlDecimal.Field<opportunitiesValue> { }
		protected Decimal? _OpportunitiesValue;
		[PXDecimal()]
		[PXUIField(DisplayName = "Opportunities Value", Enabled = false)]
		[PXDBScalar(typeof(Search4<CROpportunity.productsAmount,
				Where<CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>>,
			    Aggregate<Sum<CROpportunity.amount, Sum<CROpportunity.productsAmount, Sum<CROpportunity.discTot>>>>>))]
		public virtual Decimal? OpportunitiesValue
		{
			get
			{
				return this._OpportunitiesValue;
			}
			set
			{
				this._OpportunitiesValue = value;
			}
		}
		#endregion
		#region ClosedOpportunitiesValue
		public abstract class closedOpportunitiesValue : PX.Data.BQL.BqlDecimal.Field<closedOpportunitiesValue> { }
		protected Decimal? _ClosedOpportunitiesValue;
		[PXDecimal()]
		[PXUIField(DisplayName = "Won Opportunities Value", Enabled = false)]
		[PXDBScalar(typeof(Search4<CROpportunity.productsAmount,
                Where<CROpportunity.campaignSourceID, Equal<CRCampaign.campaignID>,
                    And<CROpportunity.status, Equal<CROpportunityStatusCodes.Won>>>,
                Aggregate<Sum<CROpportunity.amount, Sum<CROpportunity.productsAmount, Sum<CROpportunity.discTot>>>>>))]
        public virtual Decimal? ClosedOpportunitiesValue
		{
			get
			{
				return this._ClosedOpportunitiesValue;
			}
			set
			{
				this._ClosedOpportunitiesValue = value;
			}
		}
		#endregion
	}

    public class CROpportunityStatusCodes
    {
        public class Won : PX.Data.BQL.BqlString.Constant<Won>
		{
            public Won()
                : base("W")
            {
            }
        }
    }
}
