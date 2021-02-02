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
using PX.Objects.CR;
using PX.Objects.CR.DAC.Standalone;
using PX.Objects.CT;

namespace PX.Objects.PM
{
	/// <exclude/>
	[SerializableAttribute()]
	[PXCacheName(CR.Messages.PMQuote)]
	[PXPrimaryGraph(typeof(PMQuoteMaint))]
	[CREmailContactsView(typeof(Select2<Contact,
		LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
		Where2<Where<Optional<PMQuote.bAccountID>, IsNull, And<Contact.contactID, Equal<Optional<PMQuote.contactID>>>>,
			Or2<Where<Optional<PMQuote.bAccountID>, IsNotNull, And<Contact.bAccountID, Equal<Optional<PMQuote.bAccountID>>>>,
				Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>))]
	[PXEMailSource]//NOTE: for assignment map
	[PXQuoteProjection(typeof(Select2<CR.Standalone.CRQuote,
		InnerJoin<CROpportunityRevision,
			On<CROpportunityRevision.noteID, Equal<CR.Standalone.CRQuote.quoteID>>,
		LeftJoin<CR.Standalone.CROpportunity,
			On<CR.Standalone.CROpportunity.opportunityID, Equal<CROpportunityRevision.opportunityID>>>>,
		Where<CR.Standalone.CRQuote.quoteType, Equal<CRQuoteTypeAttribute.project>>>))]
	[PXBreakInheritance]
	public partial class PMQuote : IBqlTable, IAssign, IPXSelectable
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
		[PXDBGuid(BqlField = typeof(CR.Standalone.CRQuote.quoteID))]
		[PXFormula(typeof(noteID))]
		public virtual Guid? QuoteID { get; set; }
		#endregion

		#region QuoteNbr
		public abstract class quoteNbr : PX.Data.BQL.BqlString.Field<quoteNbr> { }
		[AutoNumber(typeof(PMSetup.quoteNumberingID), typeof(AccessInfo.businessDate))]
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(CR.Standalone.CRQuote.quoteNbr))]
		[PXSelector(typeof(Search2<PMQuote.quoteNbr,
					LeftJoin<BAccount, On<BAccount.bAccountID, Equal<PMQuote.bAccountID>>,
					LeftJoin<Contact, On<Contact.contactID, Equal<PMQuote.contactID>>>>,
				Where<PMQuote.quoteType, Equal<CRQuoteTypeAttribute.project>>,
				OrderBy<Desc<PMQuote.quoteNbr>>>),
			new[] {
				typeof(PMQuote.quoteNbr),
				typeof(PMQuote.status),
				typeof(PMQuote.subject),
				typeof(BAccount.acctCD),
				typeof(BAccount.acctName),
				typeof(PMQuote.documentDate),
				typeof(PMQuote.expirationDate)
			 },
			Filterable = true, DescriptionField = typeof(PMQuote.subject))]
		[PXUIField(DisplayName = "Quote Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String QuoteNbr { get; set; }
		#endregion

		#region QuoteType
		public abstract class quoteType : PX.Data.BQL.BqlString.Field<quoteType> { }
		
		[PXDBString(1, IsFixed = true, BqlField = typeof(CR.Standalone.CRQuote.quoteType))]
		[PXUIField(DisplayName = "Type", Visible = false)]
		[PXMassUpdatableField]
		[CRQuoteType()]
		[PXDefault(CRQuoteTypeAttribute.Project)]
		public virtual string QuoteType { get; set; }
		#endregion

		#region OpportunityID
		public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }
		[PXDBString(CR.Standalone.CROpportunity.OpportunityIDLength, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(CROpportunityRevision.opportunityID))]
		[PXUIField(DisplayName = "Opportunity ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXSelector(typeof(Search2<CR.CROpportunity.opportunityID,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CR.CROpportunity.bAccountID>>,
					LeftJoin<Contact, On<Contact.contactID, Equal<CR.CROpportunity.contactID>>>>,
				Where<True, Equal<True>>,
				OrderBy<Desc<CR.CROpportunity.opportunityID>>>),
			new[] {
				typeof(CR.CROpportunity.opportunityID),
				typeof(CR.CROpportunity.subject),
				typeof(CR.CROpportunity.status),
				typeof(CR.CROpportunity.stageID),
				typeof(CR.CROpportunity.classID),
				typeof(BAccount.acctName),
				typeof(Contact.displayName),
				typeof(CR.CROpportunity.externalRef),
				typeof(CR.CROpportunity.closeDate) },
			Filterable = true)]
		[PXFieldDescription]
		[PXRestrictor(typeof(Where<CR.CROpportunity.bAccountID, Equal<Current<bAccountID>>, Or<Current<bAccountID>, IsNull>>), Messages.OpportunityBAccount)]
		[PXRestrictor(typeof(Where<CR.CROpportunity.isActive.IsEqual<True>>), Messages.QuoteCannotBeLinkedToNotActiveOpportunity)]
		public virtual String OpportunityID { get; set; }
		#endregion

		#region IsActive
		public abstract class opportunityIsActive : PX.Data.BQL.BqlInt.Field<opportunityIsActive> { }

		[PXDBBool(BqlField = typeof(CR.Standalone.CROpportunity.isActive))]
		[PXUIField(Visible = false, DisplayName = "Opportunity Is Active")]
		public virtual bool? OpportunityIsActive { get; set; }
		#endregion

		#region DefQuoteID
		public abstract class defQuoteID : PX.Data.BQL.BqlGuid.Field<defQuoteID> { }
		[PXDBGuid(BqlField = typeof(CR.Standalone.CROpportunity.defQuoteID))]
		public virtual Guid? DefQuoteID { get; set; }
		#endregion

		#region IsPrimary
		public abstract class isPrimary : PX.Data.BQL.BqlBool.Field<isPrimary> { }
		[PXBool()]
		[PXUIField(DisplayName = "Primary", Enabled = false, FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXFormula(typeof(Switch<Case<Where<defQuoteID, IsNotNull, And<quoteID, Equal<defQuoteID>>>, True>, False>))]
		public virtual Boolean? IsPrimary
		{
			get;
			set;
		}
		#endregion

		#region ManualTotal
		public abstract class manualTotalEntry : PX.Data.BQL.BqlBool.Field<manualTotalEntry> { }

		[PXDBBool(BqlField = typeof(CROpportunityRevision.manualTotalEntry))]
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
		[PXDBString(10, IsUnicode = true, BqlField = typeof(CR.Standalone.CRQuote.termsID))]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		[PXDefault(
			typeof(Coalesce<
			Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<PMQuote.bAccountID>>>>,
			Search<CustomerClass.termsID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<PMQuote.bAccountID>))]
		[PXUIField(DisplayName = "Credit Terms")]
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

		[PXDBDate(BqlField = typeof(CROpportunityRevision.documentDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXMassUpdatableField]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocumentDate { get; set; }
		#endregion

		#region ExpirationDate
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }

		[PXDBDate(BqlField = typeof(CR.Standalone.CRQuote.expirationDate))]
		[PXMassUpdatableField]
		[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? ExpirationDate { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, IsFixed = true, BqlField = typeof(CR.Standalone.CRQuote.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassUpdatableField]
		[PMQuoteStatusAttribute()]
		[PXDefault(CRQuoteStatusAttribute.Draft)]
		public virtual string Status { get; set; }
		#endregion

		#region OpportunityAddressID
		public abstract class opportunityAddressID : PX.Data.BQL.BqlInt.Field<opportunityAddressID> { }
		protected Int32? _OpportunityAddressID;
		[PXDBInt(BqlField = typeof(CROpportunityRevision.opportunityAddressID))]
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
		[PXDBInt(BqlField = typeof(CROpportunityRevision.opportunityContactID))]
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
		[PXDBBool(BqlField = typeof(CROpportunityRevision.allowOverrideContactAddress))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override Contact and Address")]
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
		[CustomerAndProspect(DisplayName = "Business Account", BqlField = typeof(CROpportunityRevision.bAccountID))]
		[PXDefault(typeof(Search<CROpportunityRevision.bAccountID, Where<CROpportunityRevision.noteID, Equal<Current<PMQuote.quoteID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[LocationID(typeof(Where<CR.Location.bAccountID, Equal<Current<PMQuote.bAccountID>>, Or<Current<PMQuote.bAccountID>, IsNull>>),
			DisplayName = "Location",
			DescriptionField = typeof(CR.Location.descr),
			BqlField = typeof(CROpportunityRevision.locationID))]
		// add check for features
		[PXDefault(typeof(Search<CR.CROpportunity.locationID, Where<CR.CROpportunity.opportunityID, Equal<Current<PMQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		protected Int32? _ContactID;
		[PXDBInt(BqlField = typeof(CROpportunityRevision.contactID))]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search2<Contact.contactID,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
				Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
					Or<Contact.contactType, Equal<ContactTypesAttribute.lead>>>>),
			DescriptionField = typeof(Contact.displayName), Filterable = true)]
		[PXRestrictor(typeof(Where2<Where2<
				Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
					Or<Contact.contactType, Equal<ContactTypesAttribute.lead>>>,
				And<
					Where<BAccount.type, IsNull,
						Or<BAccount.type, Equal<BAccountType.customerType>,
							Or<BAccount.type, Equal<BAccountType.prospectType>,
								Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>,
			And<WhereEqualNotNull<BAccount.bAccountID, PMQuote.bAccountID>>>), CR.Messages.ContactBAccountOpp, typeof(Contact.displayName), typeof(Contact.contactID))]
		[PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>), CR.Messages.ContactInactive, typeof(Contact.displayName))]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXDefault(typeof(Search<CR.CROpportunity.contactID, Where<CR.CROpportunity.opportunityID, Equal<Current<PMQuote.opportunityID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ContactID
		{
			get { return _ContactID; }
			set { _ContactID = value; }
		}
		#endregion

		#region ShipContactID
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID>
		{
		}
		protected Int32? _ShipContactID;
		[PXDBInt(BqlField = typeof(CROpportunityRevision.shipContactID))]
		[CRShippingContact(typeof(Select2<Contact,
					InnerJoin<CR.Location, On<CR.Location.bAccountID, Equal<Contact.bAccountID>,
								And<Contact.contactID, Equal<CR.Location.defContactID>,
								And<CR.Location.bAccountID, Equal<Current<PMQuote.bAccountID>>,
								And<CR.Location.locationID, Equal<Current<PMQuote.locationID>>>>>>,
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
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID>
		{
		}
		protected Int32? _ShipAddressID;
		[PXDBInt(BqlField = typeof(CROpportunityRevision.shipAddressID))]
		[CRShippingAddress(typeof(Select2<Address,
					InnerJoin<CR.Location, On<CR.Location.bAccountID, Equal<Address.bAccountID>,
								And<Address.addressID, Equal<CR.Location.defAddressID>,
								And<CR.Location.bAccountID, Equal<Current<PMQuote.bAccountID>>,
								And<CR.Location.locationID, Equal<Current<PMQuote.locationID>>>>>>,
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
		public abstract class allowOverrideShippingContactAddress : PX.Data.BQL.BqlBool.Field<allowOverrideShippingContactAddress>
		{
		}
		protected Boolean? _AllowOverrideShippingContactAddress;
		[PXDBBool(BqlField = typeof(CR.Standalone.CROpportunityRevision.allowOverrideShippingContactAddress))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Override")]
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

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		[PXDBString(255, IsUnicode = true, BqlField = typeof(CR.Standalone.CRQuote.subject))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Subject { get; set; }
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		[CustomerAndProspect(DisplayName = "Parent Account", BqlField = typeof(CROpportunityRevision.parentBAccountID))]
		[PXFormula(typeof(Selector<CROpportunityRevision.bAccountID, BAccount.parentBAccountID>))]
		[PXDefault(typeof(Search<CROpportunityRevision.parentBAccountID, Where<CROpportunityRevision.noteID, Equal<Current<PMQuote.quoteID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ParentBAccountID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(Coalesce<
				Search<CR.Location.cBranchID, Where<CR.Location.bAccountID, Equal<Current<CROpportunityRevision.bAccountID>>, And<CR.Location.locationID, Equal<Current<CROpportunityRevision.locationID>>>>>,
				Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false,
			BqlField = typeof(CROpportunityRevision.branchID))]
		[PXDefault(typeof(Search<CROpportunityRevision.branchID, Where<CROpportunityRevision.noteID, Equal<Current<PMQuote.quoteID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDBInt(BqlField = typeof(CROpportunityRevision.projectID))]
		[PXDefault(0)]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region QuoteProjectID
		public abstract class quoteProjectID : PX.Data.BQL.BqlInt.Field<quoteProjectID> { }
		[PXUIField(DisplayName = "Project ID")]
		[PXDBInt(BqlField = typeof(CROpportunityRevision.quoteProjectID))]
		[PXDimensionSelector(ProjectAttribute.DimensionName, 
			typeof(Search<PMProject.contractID, Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>>>), typeof(PMProject.contractCD) , DescriptionField = typeof(PMProject.description))]
		public virtual Int32? QuoteProjectID { get; set; }
		#endregion

		#region QuoteProjectCD
		public abstract class quoteProjectCD : PX.Data.BQL.BqlString.Field<quoteProjectCD> { }

		[PXDBString(BqlField = typeof(CROpportunityRevision.quoteProjectCD))]
		[PXUIField(DisplayName = "New Project ID")]
		[PXDimension(ProjectAttribute.DimensionName)]
		public virtual string QuoteProjectCD
		{
			get;
			set;
		}
		#endregion

		#region TemplateID
		public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
		[PXUIField(DisplayName = "Project Template", FieldClass = ProjectAttribute.DimensionName)]
		[PXDefault(typeof(Search<PMSetup.quoteTemplateID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDimensionSelector(ProjectAttribute.DimensionName,
				typeof(Search2<PMProject.contractID,
						LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<PMProject.contractID>>>,
							Where<PMProject.baseType, Equal<CTPRType.projectTemplate>, And<PMProject.isActive, Equal<True>>>>),
				typeof(PMProject.contractCD),
				typeof(PMProject.contractCD),
				typeof(PMProject.description),
				typeof(PMProject.budgetLevel),
				typeof(PMProject.billingID),
				typeof(ContractBillingSchedule.type),
				typeof(PMProject.approverID),
				DescriptionField = typeof(PMProject.description))]
		[PXDBInt(BqlField = typeof(CROpportunityRevision.templateID))]
		public virtual Int32? TemplateID { get; set; }
		#endregion
		

		#region ProjectManager
		public abstract class projectManager : PX.Data.BQL.BqlInt.Field<projectManager> { }

		[PXDBInt(BqlField = typeof(CROpportunityRevision.projectManager))]
		[EP.PXEPEmployeeSelector]
		[PXUIField(DisplayName = "Project Manager")]
		public virtual Int32? ProjectManager
		{
			get;
			set;
		}
		#endregion

		#region ExternalRef
		public abstract class externalRef : PX.Data.BQL.BqlString.Field<externalRef> { }

		[PXDBString(255, IsFixed = true, BqlField = typeof(CROpportunityRevision.externalRef))]
		[PXUIField(DisplayName = "External Ref.")]
		public virtual string ExternalRef { get; set; }
		#endregion
				
		#region CampaignSourceID
		public abstract class campaignSourceID : PX.Data.BQL.BqlString.Field<campaignSourceID> { }

		[PXDBString(10, IsUnicode = true, BqlField = typeof(CROpportunityRevision.campaignSourceID))]
		[PXUIField(DisplayName = "Source Campaign")]
		[PXSelector(typeof(Search3<CR.CRCampaign.campaignID, OrderBy<Desc<CR.CRCampaign.campaignID>>>),
			DescriptionField = typeof(CR.CRCampaign.campaignName), Filterable = true)]
		[PXDefault(typeof(Search<CROpportunityRevision.campaignSourceID, Where<CROpportunityRevision.noteID, Equal<Current<PMQuote.quoteID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String CampaignSourceID { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXDBInt(BqlField = typeof(CROpportunityRevision.workgroupID))]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup")]
		[PXMassUpdatableField]
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXDBGuid(BqlField = typeof(CROpportunityRevision.ownerID))]
		[PXOwnerSelector(typeof(CROpportunityRevision.workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		[PXMassUpdatableField]
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }

		[PXDBBool(BqlField = typeof(CROpportunityRevision.approved))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Boolean? Approved { get; set; }
		#endregion
		#region Rejected

		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }

		[PXDBBool(BqlField = typeof(CROpportunityRevision.rejected))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Boolean? Rejected { get; set; }
		#endregion
		#region SubmitCancelled
		public abstract class submitCancelled : PX.Data.BQL.BqlBool.Field<submitCancelled> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? SubmitCancelled { get; set; }
		#endregion

		#region IsSetupApprovalRequired
		public abstract class isSetupApprovalRequired : PX.Data.BQL.BqlBool.Field<isSetupApprovalRequired> { }
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<Current<PMSetup.quoteApprovalMapID>, IsNotNull>, True>, False>))]
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

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(CROpportunityRevision.curyID))]
		[PXDefault(typeof(Search<CRSetup.defaultCuryID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Currency.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXDBLong(BqlField = typeof(CROpportunityRevision.curyInfoID))]
		[CurrencyInfo]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion

		#region ExtPriceTotal
		public abstract class extPriceTotal : PX.Data.BQL.BqlDecimal.Field<extPriceTotal> { }
		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.extPriceTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtPriceTotal { get; set; }
		#endregion

		#region CuryExtPriceTotal
		public abstract class curyExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyExtPriceTotal> { }
		[PXUIField(DisplayName = "Subtotal", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(curyInfoID), typeof(extPriceTotal), BqlField = typeof(CROpportunityRevision.curyExtPriceTotal))]
		public virtual Decimal? CuryExtPriceTotal { get; set; }
		#endregion

		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal { get; set; }
		#endregion

		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineTotal), BqlField = typeof(CROpportunityRevision.curyLineTotal))]
		[PXUIField(DisplayName = "Detail Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal { get; set; }
		#endregion

		#region CostTotal
		public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }

		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.costTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CostTotal { get; set; }
		#endregion

		#region CuryCostTotal
		public abstract class curyCostTotal : PX.Data.BQL.BqlDecimal.Field<curyCostTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(costTotal), BqlField = typeof(CROpportunityRevision.curyCostTotal))]
		[PXUIField(DisplayName = "Total Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryCostTotal { get; set; }
		#endregion
			

		#region GrossMarginAmount
		public abstract class grossMarginAmount : PX.Data.BQL.BqlDecimal.Field<grossMarginAmount> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin")]
		public virtual Decimal? GrossMarginAmount
		{
			[PXDependsOnFields(typeof(amount), typeof(costTotal))]
			get
			{
				return Amount - CostTotal;
			}

		}
		#endregion

		#region CuryGrossMarginAmount
		public abstract class curyGrossMarginAmount : PX.Data.BQL.BqlDecimal.Field<curyGrossMarginAmount> { }
		[PXCurrency(typeof(curyInfoID), typeof(grossMarginAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin")]
		public virtual Decimal? CuryGrossMarginAmount
		{
			[PXDependsOnFields(typeof(curyAmount), typeof(curyCostTotal))]
			get
			{ 
				return CuryAmount - CuryCostTotal;
			}

		}
		#endregion
		
		#region GrossMarginPct
		public abstract class grossMarginPct : PX.Data.BQL.BqlDecimal.Field<grossMarginPct> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Gross Margin %")]
		public virtual Decimal? GrossMarginPct
		{
			[PXDependsOnFields(typeof(amount), typeof(costTotal))]
			get
			{
				if (Amount != 0)
				{
					return 100 * (Amount - CostTotal) / Amount;
				}
				else
					return 0;
			}
		}
		#endregion

		#region QuoteTotal
		public abstract class quoteTotal : PX.Data.BQL.BqlDecimal.Field<quoteTotal> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Quote Total")]
		public virtual Decimal? QuoteTotal
		{
			[PXDependsOnFields(typeof(amount), typeof(taxTotal))]
			get
			{
				return Amount + TaxTotal;
			}

		}
		#endregion

		#region CuryQuoteTotal
		public abstract class curyQuoteTotal : PX.Data.BQL.BqlDecimal.Field<curyQuoteTotal> { }
		[PXCurrency(typeof(curyInfoID), typeof(quoteTotal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Quote Total")]
		public virtual Decimal? CuryQuoteTotal
		{
			[PXDependsOnFields(typeof(curyAmount), typeof(curyTaxTotal))]
			get
			{
				return CuryAmount + CuryTaxTotal;
			}

		}
		#endregion

		#region LineDiscountTotal
		public abstract class lineDiscountTotal : PX.Data.BQL.BqlDecimal.Field<lineDiscountTotal> { }

		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.lineDiscountTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineDiscountTotal { get; set; }
		#endregion

		#region CuryLineDiscountTotal
		public abstract class curyLineDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDiscountTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineDiscountTotal), BqlField = typeof(CROpportunityRevision.curyLineDiscountTotal))]
		[PXUIField(DisplayName = "Discount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineDiscountTotal { get; set; }
		#endregion

		#region LineDocDiscountTotal
		public abstract class lineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<lineDocDiscountTotal> { }

		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.lineDocDiscountTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineDocDiscountTotal { get; set; }
		#endregion

		#region CuryLineDocDiscountTotal
		public abstract class curyLineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDocDiscountTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineDocDiscountTotal), BqlField = typeof(CROpportunityRevision.curyLineDocDiscountTotal))]
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
				return String.Format(CR.Messages.QuoteGridProductText, CuryExtPriceTotal.ToString(), CuryLineDiscountTotal.ToString());
			}
		}
		#endregion

		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		[PXDBBool(BqlField = typeof(CROpportunityRevision.isTaxValid))]
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

		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.taxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal { get; set; }
		#endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

		[PXDBCurrency(typeof(CROpportunityRevision.curyInfoID), typeof(CROpportunityRevision.taxTotal), BqlField = typeof(CROpportunityRevision.curyTaxTotal))]
		[PXUIField(DisplayName = "Tax Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal { get; set; }
		#endregion

		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		private decimal? _amount;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury(BqlField = typeof(CROpportunityRevision.amount))]
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
		[PXDBCurrency(typeof(curyInfoID), typeof(amount), BqlField = typeof(CROpportunityRevision.curyAmount))]
		[PXFormula(typeof(Switch<Case<Where<manualTotalEntry, Equal<True>>, curyAmount>, curyLineTotal>))]
		[PXUIField(DisplayName = "Total Sales", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryAmount
		{
			get { return _curyAmount; }
			set { _curyAmount = value; }
		}

		#endregion

		#region DiscTot
		public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }

		[PXDBBaseCury(BqlField = typeof(CROpportunityRevision.discTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscTot { get; set; }
		#endregion

		#region CuryDiscTot
		public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

		[PXDBCurrency(typeof(CROpportunityRevision.curyInfoID), typeof(CROpportunityRevision.discTot), BqlField = typeof(CROpportunityRevision.curyDiscTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Discount")]
		[PXFormula(typeof(Switch<Case<Where<manualTotalEntry, Equal<True>>, curyDiscTot>, curyLineDocDiscountTotal>))]
		public virtual Decimal? CuryDiscTot { get; set; }
		#endregion

		#region CuryProductsAmount
		public abstract class curyProductsAmount : PX.Data.BQL.BqlDecimal.Field<curyProductsAmount> { }

		private decimal? _CuryProductsAmount;
		[PXDBCurrency(typeof(CROpportunityRevision.curyInfoID), typeof(CROpportunityRevision.productsAmount), BqlField = typeof(CROpportunityRevision.curyProductsAmount))]
		[PXUIField(DisplayName = "Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryProductsAmount
		{
			set { _CuryProductsAmount = value; }
			get { return _CuryProductsAmount; }
		}
		#endregion

		#region ProductsAmount
		public abstract class productsAmount : PX.Data.BQL.BqlDecimal.Field<productsAmount> { }

		private decimal? _ProductsAmount;
		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.productsAmount))]
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

		[PXDBCurrency(typeof(CROpportunityRevision.curyInfoID), typeof(CROpportunityRevision.vatExemptTotal), BqlField = typeof(CROpportunityRevision.curyVatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal { get; set; }
		#endregion

		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.vatExemptTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal { get; set; }
		#endregion

		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

		[PXDBCurrency(typeof(CROpportunityRevision.curyInfoID), typeof(CROpportunityRevision.vatTaxableTotal), BqlField = typeof(CROpportunityRevision.curyVatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal { get; set; }
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

		[PXDBDecimal(4, BqlField = typeof(CROpportunityRevision.vatTaxableTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal { get; set; }
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		[PXDBString(10, IsUnicode = true, BqlField = typeof(CROpportunityRevision.taxZoneID))]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXFormula(typeof(Default<PMQuote.branchID>))]
		[PXFormula(typeof(Default<PMQuote.locationID>))]
		[PXDefault(typeof(Search<CROpportunityRevision.taxZoneID, Where<CROpportunityRevision.noteID, Equal<Current<PMQuote.quoteID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxZoneID { get; set; }
		#endregion

		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(CROpportunityRevision.taxCalcMode))]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CROpportunityRevision.taxCalcMode, Where<CROpportunityRevision.opportunityID, Equal<Current<PMQuote.opportunityID>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXExtraKey()]
		[PXSearchable(SM.SearchCategory.PM, CR.Messages.ProjectQuotesSearchTitle, new Type[] { typeof(quoteNbr), typeof(bAccountID), typeof(BAccount.acctName) },
			new Type[] { typeof(subject) },
			NumberFields = new Type[] { typeof(quoteNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(documentDate), typeof(status), typeof(externalRef) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(subject) }
		)]
		[PXNote(
			DescriptionField = typeof(quoteNbr),
			Selector = typeof(quoteNbr),
			BqlField = typeof(CR.Standalone.CROpportunityRevision.noteID),
			ShowInReferenceSelector = true)]
		[PXDefault(typeof(CR.Standalone.CROpportunityRevision.noteID))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }
		/// <summary>
		/// Gets or sets entity attributes.
		/// </summary>
		[CRAttributesField(typeof(PMProject.classID), typeof(quoteID))]
		public virtual string[] Attributes { get; set; }

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		/// <summary>
		/// Gets ClassID for the attributes. Always returns current <see cref="GroupTypes.Project"/>
		/// </summary>
		[PXString(20)]
		public virtual string ClassID
		{
			get { return GroupTypes.Project; }
		}

		#endregion

		#endregion

		#region ProductCntr
		public abstract class productCntr : PX.Data.BQL.BqlInt.Field<productCntr> { }

		[PXDBInt(BqlField = typeof(CROpportunityRevision.productCntr))]
		[PXDefault(0)]
		public virtual Int32? ProductCntr { get; set; }

		#endregion

		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		[PXDBInt(BqlField = typeof(CROpportunityRevision.lineCntr))]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? LineCntr { get; set; }
		#endregion

		#region RefOpportunityID
		public abstract class refOpportunityID : PX.Data.BQL.BqlString.Field<refOpportunityID> { }

		[PXDBString(CR.Standalone.CROpportunity.OpportunityIDLength, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(CR.Standalone.CROpportunity.opportunityID))]
		[PXExtraKey()]
		public virtual String RefOpportunityID { get { return OpportunityID; } }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(BqlField = typeof(CR.Standalone.CRQuote.Tstamp))]
		public virtual Byte[] tstamp { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID(BqlField = typeof(CR.Standalone.CRQuote.createdByScreenID))]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID(BqlField = typeof(CR.Standalone.CRQuote.createdByID))]
		[PXUIField(DisplayName = "Created By")]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime(BqlField = typeof(CR.Standalone.CRQuote.createdDateTime))]
		[PXUIField(DisplayName = "Date Created", Enabled = false)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID(BqlField = typeof(CR.Standalone.CRQuote.lastModifiedByID))]
		[PXUIField(DisplayName = "Last Modified By")]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID(BqlField = typeof(CR.Standalone.CRQuote.lastModifiedByScreenID))]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(CR.Standalone.CRQuote.lastModifiedDateTime))]
		[PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion


		#region RCreatedByID
		public abstract class rCreatedByID : PX.Data.BQL.BqlGuid.Field<rCreatedByID> { }
		[PXDBCreatedByID(BqlField = typeof(CROpportunityRevision.createdByID))]
		public virtual Guid? RCreatedByID
		{
			get;
			set;
		}
		#endregion

		#region RCreatedByScreenID
		public abstract class rCreatedByScreenID : PX.Data.BQL.BqlString.Field<rCreatedByScreenID> { }
		[PXDBCreatedByScreenID(BqlField = typeof(CROpportunityRevision.createdByScreenID))]
		public virtual String RCreatedByScreenID
		{
			get;
			set;
		}
		#endregion

		#region RCreatedDateTime
		public abstract class rCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<rCreatedDateTime> { }
		[PXDBCreatedDateTime(BqlField = typeof(CROpportunityRevision.createdDateTime))]
		public virtual DateTime? RCreatedDateTime
		{
			get;
			set;
		}
		#endregion

		#region RLastModifiedByID
		public abstract class rLastModifiedByID : PX.Data.BQL.BqlGuid.Field<rLastModifiedByID> { }
		[PXDBLastModifiedByID(BqlField = typeof(CROpportunityRevision.lastModifiedByID))]
		public virtual Guid? RLastModifiedByID
		{
			get;
			set;
		}
		#endregion

		#region RLastModifiedByScreenID
		public abstract class rLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<rLastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID(BqlField = typeof(CROpportunityRevision.lastModifiedByScreenID))]
		public virtual String RLastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion

		#region RLastModifiedDateTime
		public abstract class rLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<rLastModifiedDateTime> { }
		[PXDBLastModifiedDateTime(BqlField = typeof(CROpportunityRevision.lastModifiedDateTime))]
		public virtual DateTime? RLastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}

	public class PMQuoteStatusAttribute : CRQuoteStatusAttribute
	{
		public const string Closed = "C";

		public PMQuoteStatusAttribute()
			: base(new[] { Draft, Approved, Sent, PendingApproval, Rejected, Closed },
					new[] { CR.Messages.Draft, CR.Messages.Prepared, CR.Messages.Sent, CR.Messages.PendingApproval, CR.Messages.Rejected, Messages.Closed })
		{ }
				
		public sealed class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) { }
		}
	}
}
