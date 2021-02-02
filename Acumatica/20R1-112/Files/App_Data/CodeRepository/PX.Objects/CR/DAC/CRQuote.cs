using System.Collections.Generic;
using PX.Common;
using PX.Data.EP;
using System;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.AR;
using PX.Objects.CR.Standalone;
using PX.Objects.PM;
using PX.Objects.GL;

namespace PX.Objects.CR
{
	/// <exclude/>
    [SerializableAttribute()]
    [PXCacheName(Messages.CRQuote)]
    [CRQuotePrimaryGraph]
    [CREmailContactsView(typeof(Select2<Contact,
        LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
        Where2<Where<Optional<CRQuote.bAccountID>, IsNull, And<Contact.contactID, Equal<Optional<CRQuote.contactID>>>>,
            Or2<Where<Optional<CRQuote.bAccountID>, IsNotNull, And<Contact.bAccountID, Equal<Optional<CRQuote.bAccountID>>>>,
                Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>))]
    [PXEMailSource]//NOTE: for assignment map
    [PXQuoteProjection(typeof(Select2<Standalone.CRQuote,        
        InnerJoin<Standalone.CROpportunityRevision,
            On<Standalone.CROpportunityRevision.noteID, Equal<Standalone.CRQuote.quoteID>>,
        LeftJoin<Standalone.CROpportunity,
            On<Standalone.CROpportunity.opportunityID, Equal<CROpportunityRevision.opportunityID>>>>>))]
    [PXBreakInheritance]
    public partial class CRQuote : IBqlTable, IAssign, IPXSelectable
    {     

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
        public virtual bool? Selected { get; set; }
        #endregion

		#region QuoteID
		public abstract class quoteID : PX.Data.BQL.BqlGuid.Field<quoteID> { }
		[PXDBGuid(BqlField = typeof(Standalone.CRQuote.quoteID))]
		[PXFormula(typeof(noteID))]
		public virtual Guid? QuoteID { get; set; }
		#endregion

        #region OpportunityID
        public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }

        [PXDBString(CR.Standalone.CROpportunity.OpportunityIDLength, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField=typeof(Standalone.CROpportunityRevision.opportunityID))]
        [PXUIField(DisplayName = "Opportunity ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search2<CROpportunity.opportunityID,
                LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
                    LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>>>,
                Where<True, Equal<True>>,
                OrderBy<Desc<CROpportunity.opportunityID>>>),
            new[] { typeof(CROpportunity.opportunityID),
                typeof(CROpportunity.subject),
                typeof(CROpportunity.status),
                typeof(CROpportunity.stageID),
                typeof(CROpportunity.classID),
                typeof(BAccount.acctName),
                typeof(Contact.displayName),
                typeof(CROpportunity.externalRef),
                typeof(CROpportunity.closeDate) },
            Filterable = true)]
        [PXFieldDescription]
        [PXDefault()]
		public virtual String OpportunityID { get; set; }
		#endregion

		#region QuoteNbr
		public abstract class quoteNbr : PX.Data.BQL.BqlString.Field<quoteNbr> { }
		protected String _QuoteNbr;
        [AutoNumber(typeof(CRSetup.quoteNumberingID), typeof(AccessInfo.businessDate))]
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(Standalone.CRQuote.quoteNbr))]
        [PXSelector(typeof(Search2<CRQuote.quoteNbr,
                    LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRQuote.bAccountID>>,
                    LeftJoin<Contact, On<Contact.contactID, Equal<CRQuote.contactID>>>>,
                Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>,
                    Or<CRQuote.opportunityID, IsNull>>,
                OrderBy<Desc<CRQuote.opportunityID>>>),
            new[] { typeof(CRQuote.quoteNbr),
	            typeof(CRQuote.isPrimary),
				typeof(CRQuote.status),
	            typeof(CRQuote.subject),
				typeof(BAccount.acctCD),	            
				typeof(CRQuote.documentDate),
                typeof(CRQuote.expirationDate),
                typeof(CRQuote.externalRef),	            
			 },
            Filterable = true)]
        [PXUIField(DisplayName = "Quote Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PX.Data.EP.PXFieldDescription]
        public virtual String QuoteNbr
        {
            get
            {
                return this._QuoteNbr;
            }
            set
            {
                this._QuoteNbr = value;
            }
        }
        #endregion

        #region QuoteType
        public abstract class quoteType : PX.Data.BQL.BqlString.Field<quoteType> { }

        [PXDBString(1, IsFixed = true, BqlField = typeof(Standalone.CRQuote.quoteType))]
        [PXUIField(DisplayName = "Type")]
        [PXMassUpdatableField]
        [CRQuoteType()]
        [PXDefault(CRQuoteTypeAttribute.Distribution)]
        public virtual string QuoteType { get; set; }
        #endregion

        #region DefQuoteID
        public abstract class defQuoteID : PX.Data.BQL.BqlGuid.Field<defQuoteID> { }
        [PXDBGuid(BqlField = typeof(Standalone.CROpportunity.defQuoteID))]
        public virtual Guid? DefQuoteID { get; set; }
        #endregion

        #region IsPrimary
        public abstract class isPrimary : PX.Data.BQL.BqlBool.Field<isPrimary> { }
        [PXBool()]        
        [PXUIField(DisplayName = "Primary", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<quoteID, Equal<defQuoteID>>, True>, False>))]
        public virtual Boolean? IsPrimary
        {
            get;
            set;
        }
        #endregion

        #region ExternalRef
        public abstract class externalRef : PX.Data.BQL.BqlString.Field<externalRef> { }

        [PXDBString(255, IsFixed = true, BqlField = typeof(Standalone.CROpportunity.externalRef))]
        [PXUIField(DisplayName = "External Ref.", Enabled = false)]
        public virtual string ExternalRef { get; set; }
		#endregion

		#region ManualTotal
	    public abstract class manualTotalEntry : PX.Data.BQL.BqlBool.Field<manualTotalEntry> { }

	    [PXDBBool(BqlField = typeof(Standalone.CROpportunityRevision.manualTotalEntry))]
	    [PXDefault(false)]
	    [PXUIField(DisplayName = "Manual Amount")]
	    public virtual Boolean? ManualTotalEntry { get; set; }
	    #endregion

		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
        protected String _TermsID;
        /// <summary>
        /// The identifier of the default <see cref="Terms">terms</see>, 
        /// which are applied to the documents of the customer.
        /// </summary>
        [PXDBString(10, IsUnicode = true, BqlField = typeof(Standalone.CRQuote.termsID))]
        [PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
        [PXDefault(
			typeof(Coalesce<
	        Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<CRQuote.bAccountID>>>>,
			Search<CustomerClass.termsID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<CRQuote.bAccountID>))]
        [PXUIField(DisplayName = "Payment Terms")]
        public virtual String TermsID
        {
            get
            {
                return this._TermsID;
            }
            set
            {
                this._TermsID = value;
            }
        }
        #endregion

        #region DocumentDate
        public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }

        [PXDBDate(BqlField = typeof(Standalone.CROpportunityRevision.documentDate))]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXMassUpdatableField]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DocumentDate { get; set; }
        #endregion

        #region ExpirationDate
        public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }

        [PXDBDate(BqlField = typeof(Standalone.CRQuote.expirationDate))]
        [PXMassUpdatableField]
        [PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ExpirationDate { get; set; }
        #endregion

        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

        [PXDBString(1, IsFixed = true, BqlField = typeof(Standalone.CRQuote.status))]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMassUpdatableField]
        [PMQuoteStatus()]
        [PXDefault(CRQuoteStatusAttribute.Draft)]
        public virtual string Status { get; set; }
        #endregion        

        #region OpportunityAddressID
        public abstract class opportunityAddressID : PX.Data.BQL.BqlInt.Field<opportunityAddressID> { }
        protected Int32? _OpportunityAddressID;
        [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.opportunityAddressID))]
        [CROpportunityAddress(typeof(Select<Address,
            Where<True, Equal<False>>>))]
        public virtual Int32? OpportunityAddressID
        {
            get
            {
                return this._OpportunityAddressID;
            }
            set
            {
                this._OpportunityAddressID = value;
            }
        }
        #endregion

        #region OpportunityContactID
        public abstract class opportunityContactID : PX.Data.BQL.BqlInt.Field<opportunityContactID> { }
        protected Int32? _OpportunityContactID;
        [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.opportunityContactID))]
        [CROpportunityContact(typeof(Select<Contact,
            Where<True, Equal<False>>>))]
        public virtual Int32? OpportunityContactID
        {
            get
            {

                return this._OpportunityContactID;
            }
            set
            {
                this._OpportunityContactID = value;
            }
        }
        #endregion  

        #region AllowOverrideContactAddress
        public abstract class allowOverrideContactAddress : PX.Data.BQL.BqlBool.Field<allowOverrideContactAddress> { }
        protected Boolean? _AllowOverrideContactAddress;
        [PXDBBool(BqlField = typeof(Standalone.CROpportunityRevision.allowOverrideContactAddress))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? AllowOverrideContactAddress
        {
            get
            {
                return this._AllowOverrideContactAddress;
            }
            set
            {
                this._AllowOverrideContactAddress = value;
            }
        }
        #endregion

        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        private int? _BAccountID;
        [CustomerAndProspect(DisplayName = "Business Account", BqlField = typeof(Standalone.CROpportunityRevision.bAccountID), Enabled = false)]
        [PXDefault(typeof(Search<CROpportunity.bAccountID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? BAccountID
        {
            get
            {
                return _BAccountID;
            }
            set
            {
                _BAccountID = value;
            }
        }
        #endregion

        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>>),
            DisplayName = "Location",
            DescriptionField = typeof(Location.descr),
            BqlField = typeof(Standalone.CROpportunityRevision.locationID))]
        // add check for features
        [PXDefault(typeof(Search<CROpportunity.locationID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? LocationID { get; set; }
        #endregion
        
        #region ShipContactID
        public abstract class shipContactID : PX.Data.IBqlField
        {
        }
        protected Int32? _ShipContactID;
        [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.shipContactID))]
        [CRShippingContact(typeof(Select2<Contact,
                    InnerJoin<Location, On<Location.bAccountID, Equal<Contact.bAccountID>,
                                And<Contact.contactID, Equal<Location.defContactID>,
                                And<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
                                And<Location.locationID, Equal<Current<CRQuote.locationID>>>>>>,
                    LeftJoin<CRShippingContact, On<CRShippingContact.bAccountID, Equal<Contact.bAccountID>,
                                And<CRShippingContact.bAccountContactID, Equal<Contact.contactID>,
                                And<CRShippingContact.revisionID, Equal<Contact.revisionID>,
                                And<CRShippingContact.isDefaultContact, Equal<True>>>>>>>,
                    Where<True, Equal<True>>>))]
        public virtual Int32? ShipContactID
        {
            get
            {
                return this._ShipContactID;
            }
            set
            {
                this._ShipContactID = value;
            }
        }
        #endregion

        #region ShipAddressID
        public abstract class shipAddressID : PX.Data.IBqlField
        {
        }
        protected Int32? _ShipAddressID;
        [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.shipAddressID))]
        [CRShippingAddress(typeof(Select2<Address,
                    InnerJoin<Location, On<Location.bAccountID, Equal<Address.bAccountID>,
                                And<Address.addressID, Equal<Location.defAddressID>,
                                And<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
                                And<Location.locationID, Equal<Current<CRQuote.locationID>>>>>>,
                    LeftJoin<CRShippingAddress, On<CRShippingAddress.bAccountID, Equal<Address.bAccountID>,
                                And<CRShippingAddress.bAccountAddressID, Equal<Address.addressID>,
                                And<CRShippingAddress.revisionID, Equal<Address.revisionID>,
                                And<CRShippingAddress.isDefaultAddress, Equal<True>>>>>>>,
                    Where<True, Equal<True>>>))]
        public virtual Int32? ShipAddressID
        {
            get
            {
                return this._ShipAddressID;
            }
            set
            {
                this._ShipAddressID = value;
            }
        }
		#endregion

        #region AllowOverrideShippingContactAddress
        public abstract class allowOverrideShippingContactAddress : PX.Data.IBqlField
        {
        }
        protected Boolean? _AllowOverrideShippingContactAddress;
        [PXDBBool(BqlField = typeof(Standalone.CROpportunityRevision.allowOverrideShippingContactAddress))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Override Shipping Info")]
        public virtual Boolean? AllowOverrideShippingContactAddress
        {
            get
            {
                return this._AllowOverrideShippingContactAddress;
            }
            set
            {
                this._AllowOverrideShippingContactAddress = value;
            }
        }
        #endregion

        #region ContactID
        public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

        protected Int32? _ContactID;
        [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.contactID))]
        [PXUIField(DisplayName = "Contact")]
        [PXSelector(typeof(Search2<Contact.contactID,
                LeftJoin<BAccount, 
                    On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
                Where<Contact.contactType, Equal<ContactTypesAttribute.person>>>),
            DescriptionField = typeof(Contact.displayName), Filterable = true)]
        [PXRestrictor(typeof(Where2<Where2<
                Where<
                    Contact.contactType, Equal<ContactTypesAttribute.person>>,
                And<
                    Where<BAccount.type, IsNull,
                        Or<BAccount.type, Equal<BAccountType.customerType>,
                        Or<BAccount.type, Equal<BAccountType.prospectType>,
                        Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>,
                And<WhereEqualNotNull<BAccount.bAccountID, CRQuote.bAccountID>>>),
            Messages.ContactBAccountOpp,
            typeof(Contact.displayName),
            typeof(Contact.contactID))]
        [PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>), Messages.ContactInactive, typeof(Contact.displayName))]
        [PXDBChildIdentity(typeof(Contact.contactID))]
        [PXDefault(typeof(Search<CROpportunity.contactID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? ContactID
        {
            get { return _ContactID; }
            set { _ContactID = value; }
        }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		[PXDBString(255, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Subject { get; set; }
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
        [CustomerAndProspect(DisplayName = "Parent Account", BqlField = typeof(Standalone.CROpportunityRevision.parentBAccountID))]
        [PXFormula(typeof(Selector<CROpportunity.bAccountID, BAccount.parentBAccountID>))]
        [PXDefault(typeof(Search<CROpportunity.parentBAccountID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? ParentBAccountID { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
        [Branch(typeof(Coalesce<
                Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>, And<Location.locationID, Equal<Current<CROpportunity.locationID>>>>>,
                Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false,
            BqlField = typeof(Standalone.CROpportunityRevision.branchID))]
        [PXDefault(typeof(Search<CROpportunity.branchID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
	    [ProjectDefault(BatchModule.CR,
		    typeof(Search<Location.cDefProjectID,
			    Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
				    And<Location.locationID, Equal<Current<CRQuote.locationID>>>>>))]
	    [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
	    [PXRestrictor(typeof(Where<PMProject.visibleInCR, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
	    [ProjectBaseAttribute(typeof(CRQuote.bAccountID), BqlField = typeof(Standalone.CROpportunityRevision.projectID))]		
        public virtual Int32? ProjectID { get; set; }
		#endregion

		#region QuoteProjectID
		public abstract class quoteProjectID : PX.Data.BQL.BqlInt.Field<quoteProjectID> { }

		[PXUIField(DisplayName = "Project ID")]
		[PXDBInt(BqlField = typeof(CROpportunityRevision.quoteProjectID))]
		[PXSelector(typeof(Search<PMProject.contractID, Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>>>), SubstituteKey = typeof(PMProject.contractCD), DescriptionField = typeof(PMProject.description))]
		public virtual int? QuoteProjectID
		{
			get;
			set;
		}
		#endregion

		#region CampaignSourceID
		public abstract class campaignSourceID : PX.Data.BQL.BqlString.Field<campaignSourceID> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(Standalone.CROpportunityRevision.campaignSourceID))]
        [PXUIField(DisplayName = "Source Campaign")]
        [PXSelector(typeof(Search3<CRCampaign.campaignID, OrderBy<Desc<CRCampaign.campaignID>>>),
            DescriptionField = typeof(CRCampaign.campaignName), Filterable = true)]
        [PXDefault(typeof(Search<CROpportunity.campaignSourceID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String CampaignSourceID { get; set; }
        #endregion
         
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

        [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.workgroupID))]
        [PXCompanyTreeSelector]
        [PXUIField(DisplayName = "Workgroup")]
        [PXMassUpdatableField]
        public virtual int? WorkgroupID { get; set; }
        #endregion

        #region OwnerID
        public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

        [PXDBGuid(BqlField = typeof(Standalone.CROpportunityRevision.ownerID))]
        [PXOwnerSelector(typeof(CROpportunity.workgroupID))]
        [PXUIField(DisplayName = "Owner")]
        [PXMassUpdatableField]
        [PXDefault(typeof(Search<CROpportunity.ownerID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Guid? OwnerID { get; set; }
        #endregion

        #region Approved
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }

        [PXDBBool(BqlField = typeof(Standalone.CROpportunityRevision.approved))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual Boolean? Approved { get; set; }
        #endregion
        #region Rejected

        public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }

        [PXDBBool(BqlField = typeof(Standalone.CROpportunityRevision.rejected))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual Boolean? Rejected { get; set; }
        #endregion

        #region IsSetupApprovalRequired
        public abstract class isSetupApprovalRequired : PX.Data.BQL.BqlBool.Field<isSetupApprovalRequired> { }
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<Current<CRSetup.quoteApprovalMapID>, IsNotNull>, True>, False>))]
        [PXUIField(DisplayName = "Approvable Setup", Visible = false, Enabled = false)]
        public virtual bool? IsSetupApprovalRequired { get; set; }
        #endregion     
		
		#region IsDisabled
		public abstract class isDisabled : PX.Data.BQL.BqlBool.Field<isDisabled> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Disabled", Visible = false)]
		public virtual bool? IsDisabled => 
			this.Status == CRQuoteStatusAttribute.PendingApproval ||
			this.Status == CRQuoteStatusAttribute.Approved ||
			this.Status == CRQuoteStatusAttribute.Rejected ||
			this.Status == CRQuoteStatusAttribute.Sent;

		#endregion

        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(Standalone.CROpportunityRevision.curyID))]
        [PXDefault(typeof(Search<CRSetup.defaultCuryID>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Currency.curyID))]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String CuryID { get; set; }
        #endregion

        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        [PXDBLong(BqlField = typeof(Standalone.CROpportunityRevision.curyInfoID))]
        [CurrencyInfo]
        public virtual Int64? CuryInfoID { get; set; }
		#endregion
		
	    #region ExtPriceTotal
	    public abstract class extPriceTotal : PX.Data.BQL.BqlDecimal.Field<extPriceTotal> { }
	    [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.extPriceTotal))]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? ExtPriceTotal { get; set; }
	    #endregion
	
		#region CuryExtPriceTotal
		public abstract class curyExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyExtPriceTotal> { }
        [PXUIField(DisplayName = "Subtotal", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBCurrency(typeof(curyInfoID), typeof(extPriceTotal), BqlField = typeof(Standalone.CROpportunityRevision.curyExtPriceTotal))]
        public virtual Decimal? CuryExtPriceTotal { get; set; }
		#endregion

	    #region LineTotal
	    public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

	    [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.lineTotal))]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? LineTotal { get; set; }
	    #endregion

	    #region CuryLineTotal
	    public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }

	    [PXDBCurrency(typeof(curyInfoID), typeof(lineTotal), BqlField = typeof(Standalone.CROpportunityRevision.curyLineTotal))]
	    [PXUIField(DisplayName = "Detail Total", Enabled = false)]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? CuryLineTotal { get; set; }
		#endregion
	    
		#region LineDiscountTotal
	    public abstract class lineDiscountTotal : PX.Data.BQL.BqlDecimal.Field<lineDiscountTotal> { }

	    [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.lineDiscountTotal))]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? LineDiscountTotal { get; set; }
	    #endregion

		#region CuryLineDiscountTotal
		public abstract class curyLineDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDiscountTotal> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(lineDiscountTotal), BqlField = typeof(Standalone.CROpportunityRevision.curyLineDiscountTotal))]
        [PXUIField(DisplayName = "Discount", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryLineDiscountTotal { get; set; }
        #endregion

        #region LineDocDiscountTotal
        public abstract class lineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<lineDocDiscountTotal> { }

        [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.lineDocDiscountTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? LineDocDiscountTotal { get; set; }
        #endregion

        #region CuryLineDocDiscountTotal
        public abstract class curyLineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDocDiscountTotal> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(lineDocDiscountTotal),  BqlField = typeof(Standalone.CROpportunityRevision.curyLineDocDiscountTotal))]
        [PXUIField(Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryLineDocDiscountTotal { get; set; }
        #endregion

        #region TextForProductsGrid
        public abstract class textForProductsGrid : PX.Data.BQL.BqlString.Field<textForProductsGrid> { }

	    [PXUIField(DisplayName = "  ", Enabled = false)]
	    [PXString()]
	    public virtual String TextForProductsGrid
	    {
		    get
		    {
			    return String.Format(Messages.QuoteGridProductText, CuryExtPriceTotal.ToString(), CuryLineDiscountTotal.ToString());
		    }
	    }
	    #endregion

		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
        [PXDBBool(BqlField = typeof(Standalone.CROpportunityRevision.isTaxValid))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
        public virtual Boolean? IsTaxValid
        {
            get;
            set;
        }
        #endregion

        #region TaxTotal
        public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }

        [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.taxTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxTotal { get; set; }
        #endregion

        #region CuryTaxTotal
        public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

        [PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.taxTotal), BqlField = typeof(Standalone.CROpportunityRevision.curyTaxTotal))]
        [PXUIField(DisplayName = "Tax Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryTaxTotal { get; set; }
        #endregion        

		#region Amount
	    public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

	    private decimal? _amount;
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    [PXDBBaseCury(BqlField = typeof(Standalone.CROpportunityRevision.amount))]
	    public virtual Decimal? Amount
	    {
		    get { return _amount; }
		    set { _amount = value; }
	    }

		#endregion

	    #region CuryAmount
	    public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount> { }

	    private decimal? _curyAmount;

	    [PXDefault(TypeCode.Decimal, "0.0")]
	    [PXDBCurrency(typeof(curyInfoID), typeof(amount), BqlField = typeof(Standalone.CROpportunityRevision.curyAmount))]
	    [PXFormula(typeof(Switch<Case<Where<manualTotalEntry, Equal<True>>, curyAmount>, curyLineTotal>))]
        [PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
	    public virtual Decimal? CuryAmount
	    {
		    get { return _curyAmount; }
		    set { _curyAmount = value; }
	    }

	    #endregion

		#region DiscTot
		public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }

        [PXDBBaseCury(BqlField = typeof(Standalone.CROpportunityRevision.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscTot { get; set; }
        #endregion

        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

        [PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.discTot), BqlField = typeof(Standalone.CROpportunityRevision.curyDiscTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount")]
        [PXFormula(typeof(Switch<Case<Where<manualTotalEntry, Equal<True>>, curyDiscTot>, curyLineDocDiscountTotal>))]
        public virtual Decimal? CuryDiscTot { get; set; }
        #endregion
        
        #region CuryProductsAmount
        public abstract class curyProductsAmount : PX.Data.BQL.BqlDecimal.Field<curyProductsAmount> { }

	    private decimal? _CuryProductsAmount;
		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.productsAmount), BqlField = typeof(Standalone.CROpportunityRevision.curyProductsAmount))]
        [PXUIField(DisplayName = "Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryProductsAmount
        {
			set { _CuryProductsAmount = value; }
			get{ return _CuryProductsAmount; }
        }
		#endregion

		#region ProductsAmount
	    public abstract class productsAmount : PX.Data.BQL.BqlDecimal.Field<productsAmount> { }

	    private decimal? _ProductsAmount;	    
	    [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.productsAmount))]
	    public virtual Decimal? ProductsAmount
	    {
		    set { _ProductsAmount = value; }
		    get
		    {
			    return _ProductsAmount;
		    }
	    }
	    #endregion

		#region CuryWgtAmount
		public abstract class curyWgtAmount : PX.Data.BQL.BqlDecimal.Field<curyWgtAmount> { }

        [PXDecimal()]
        [PXUIField(DisplayName = "Wgt. Total", Enabled = false)]
        public virtual Decimal? CuryWgtAmount { get; set; }
        #endregion

        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

        [PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.vatExemptTotal), BqlField = typeof(Standalone.CROpportunityRevision.curyVatExemptTotal))]
        [PXUIField(DisplayName = "VAT Exempt Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatExemptTotal { get; set; }
        #endregion

        #region VatExemptTaxTotal
        public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

        [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.vatExemptTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatExemptTotal { get; set; }
        #endregion

        #region CuryVatTaxableTotal
        public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

        [PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.vatTaxableTotal), BqlField = typeof(Standalone.CROpportunityRevision.curyVatTaxableTotal))]
        [PXUIField(DisplayName = "VAT Taxable Total", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatTaxableTotal { get; set; }
        #endregion

        #region VatTaxableTotal
        public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

        [PXDBDecimal(4, BqlField = typeof(Standalone.CROpportunityRevision.vatTaxableTotal))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatTaxableTotal { get; set; }
        #endregion

        #region TaxZoneID
        public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(Standalone.CROpportunityRevision.taxZoneID))]
        [PXUIField(DisplayName = "Tax Zone")]
        [PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
        [PXFormula(typeof(Default<CRQuote.branchID>))]
        [PXFormula(typeof(Default<CRQuote.locationID>))]
        [PXDefault(typeof(Search<CROpportunity.taxZoneID, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxZoneID { get; set; }
        #endregion      

        #region TaxCalcMode
        public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
        [PXDBString(1, IsFixed = true, BqlField = typeof(Standalone.CROpportunityRevision.taxCalcMode))]
        [PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CROpportunity.taxCalcMode, Where<CROpportunity.opportunityID, Equal<Current<CRQuote.opportunityID>>>>))]
        [TaxCalculationMode.List]
        [PXUIField(DisplayName = "Tax Calculation Mode")]
        public virtual string TaxCalcMode { get; set; }
        #endregion

        #region NoteID
        public  abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXExtraKey()]
		[PXSearchable(SM.SearchCategory.CR, CR.Messages.SalesQuotesSearchTitle, new Type[] { typeof(quoteNbr), typeof(bAccountID), typeof(BAccount.acctName) },
            new Type[] { typeof(subject) },
            NumberFields = new Type[] { typeof(quoteNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(documentDate), typeof(status), typeof(externalRef) },
            Line2Format = "{0}", Line2Fields = new Type[] { typeof(subject) }
        )]
        [PXNote(
            DescriptionField = typeof(quoteNbr),
			Selector = typeof(Search2<CRQuote.quoteNbr,
					LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRQuote.bAccountID>>,
					LeftJoin<Contact, On<Contact.contactID, Equal<CRQuote.contactID>>>>,
				Where<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>>,
				OrderBy<Desc<CRQuote.quoteNbr>>>),
			BqlField = typeof(Standalone.CROpportunityRevision.noteID),
			ShowInReferenceSelector = true,
			FieldList = new[] { typeof(CRQuote.quoteNbr),
				typeof(CRQuote.status),
				typeof(CRQuote.subject),
				typeof(BAccount.acctCD),
				typeof(CRQuote.documentDate),
				typeof(CRQuote.expirationDate),
				typeof(CRQuote.externalRef),
			 })]
		[PXDefault(typeof(Standalone.CROpportunityRevision.noteID))]
        public virtual Guid? NoteID { get; set; }
        #endregion
       

        #region Attributes
        public abstract class attributes : BqlAttributes.Field<attributes> { }

        [CRAttributesField(typeof(CROpportunity.classID))]
        public virtual string[] Attributes { get; set; }
        #endregion
        
        #region ProductCntr
        public abstract class productCntr : PX.Data.BQL.BqlInt.Field<productCntr> { }

        [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.productCntr))]
        [PXDefault(0)]
        public virtual Int32? ProductCntr { get; set; }

		#endregion

		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		[PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.lineCntr))]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LineCntr { get; set; }

		#endregion

		#region RefOpportunityID
		public abstract class refOpportunityID : PX.Data.BQL.BqlString.Field<refOpportunityID> { }

        [PXDBString(CR.Standalone.CROpportunity.OpportunityIDLength, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(Standalone.CROpportunity.opportunityID))]
        [PXExtraKey()]
        public virtual String RefOpportunityID { get { return OpportunityID; }  }
        #endregion
		
		#region ClassID
		public abstract class opportunityClassID : PX.Data.BQL.BqlString.Field<opportunityClassID> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa", BqlField = typeof(Standalone.CROpportunity.classID))]
		[PXUIField(DisplayName = "Opportunity Class ID")]
		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID),
			DescriptionField = typeof(CROpportunityClass.description), CacheGlobal = true)]
		[PXMassUpdatableField]
		public virtual String OpportunityClassID { get; set; }
		#endregion

		#region StageChangedDate
		public abstract class opportunityStageChangedDate : PX.Data.BQL.BqlDateTime.Field<opportunityStageChangedDate> { }

		[PXDBDate(PreserveTime = true, BqlField = typeof(Standalone.CROpportunity.stageChangedDate))]
		[PXUIField(DisplayName = "Opportunity Stage Change Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? OpportunityStageChangedDate { get; set; }
		#endregion

		#region StageID
		public abstract class opportunityStageID : PX.Data.BQL.BqlString.Field<opportunityStageID> { }

		[PXDBString(2, BqlField = typeof(Standalone.CROpportunity.stageID))]
		[PXUIField(DisplayName = "Opportunity Stage")]
		[CROpportunityStages(typeof(opportunityClassID), typeof(opportunityStageChangedDate), OnlyActiveStages = true)]
		[PXMassUpdatableField]
		public virtual String OpportunityStageID { get; set; }
		#endregion
		
		#region IsActive
		public abstract class opportunityIsActive : PX.Data.BQL.BqlInt.Field<opportunityIsActive> { }
		
		[PXDBBool(BqlField = typeof(Standalone.CROpportunity.isActive))]
		[PXUIField(Visible = false, DisplayName = "Opportunity Is Active")]
		public virtual bool? OpportunityIsActive { get; set; }
		#endregion

		#region OpportunityStatus
		public abstract class opportunityStatus : PX.Data.BQL.BqlString.Field<opportunityStatus> { }

		[PXDBString(1, IsFixed = true, BqlField = typeof(Standalone.CROpportunity.status))]
		[PXUIField(DisplayName = "Opportunity Status", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[0], new string[0])]
		[PXMassUpdatableField]
		public virtual string OpportunityStatus { get; set; }
		#endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp(BqlField = typeof(Standalone.CRQuote.Tstamp))]
        public virtual Byte[] tstamp { get; set; }
        #endregion

        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID(BqlField = typeof(Standalone.CRQuote.createdByScreenID))]
        public virtual String CreatedByScreenID { get; set; }
        #endregion

        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID(BqlField = typeof(Standalone.CRQuote.createdByID))]
        [PXUIField(DisplayName = "Created By")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion

        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime(BqlField = typeof(Standalone.CRQuote.createdDateTime))]
        [PXUIField(DisplayName = "Date Created", Enabled = false)]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion

        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID(BqlField = typeof(Standalone.CRQuote.lastModifiedByID))]
        [PXUIField(DisplayName = "Last Modified By")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion

        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID(BqlField = typeof(Standalone.CRQuote.lastModifiedByScreenID))]
        public virtual String LastModifiedByScreenID { get; set; }
        #endregion

        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime(BqlField = typeof(Standalone.CRQuote.lastModifiedDateTime))]
        [PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion        


        #region RCreatedByID
        public abstract class rCreatedByID : PX.Data.BQL.BqlGuid.Field<rCreatedByID> { }
        [PXDBCreatedByID(BqlField = typeof(Standalone.CROpportunityRevision.createdByID))]
        public virtual Guid? RCreatedByID
        {
            get;
            set;
        }
        #endregion

        #region RCreatedByScreenID
        public abstract class rCreatedByScreenID : PX.Data.BQL.BqlString.Field<rCreatedByScreenID> { }
        [PXDBCreatedByScreenID(BqlField = typeof(Standalone.CROpportunityRevision.createdByScreenID))]
        public virtual String RCreatedByScreenID
        {
            get;
            set;
        }
        #endregion

        #region RCreatedDateTime
        public abstract class rCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<rCreatedDateTime> { }
        [PXDBCreatedDateTime(BqlField = typeof(Standalone.CROpportunityRevision.createdDateTime))]
        public virtual DateTime? RCreatedDateTime
        {
            get;
            set;
        }
        #endregion

        #region RLastModifiedByID
        public abstract class rLastModifiedByID : PX.Data.BQL.BqlGuid.Field<rLastModifiedByID> { }
        [PXDBLastModifiedByID(BqlField = typeof(Standalone.CROpportunityRevision.lastModifiedByID))]
        public virtual Guid? RLastModifiedByID
        {
            get;
            set;
        }
        #endregion

        #region RLastModifiedByScreenID
        public abstract class rLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<rLastModifiedByScreenID> { }
        [PXDBLastModifiedByScreenID(BqlField = typeof(Standalone.CROpportunityRevision.lastModifiedByScreenID))]
        public virtual String RLastModifiedByScreenID
        {
            get;
            set;
        }
        #endregion

        #region RLastModifiedDateTime
        public abstract class rLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<rLastModifiedDateTime> { }
        [PXDBLastModifiedDateTime(BqlField = typeof(Standalone.CROpportunityRevision.lastModifiedDateTime))]
        public virtual DateTime? RLastModifiedDateTime
        {
            get;
            set;
        }
        #endregion        
    }
}
