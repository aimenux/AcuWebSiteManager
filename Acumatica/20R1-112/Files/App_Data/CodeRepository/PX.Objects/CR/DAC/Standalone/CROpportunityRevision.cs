using System.Collections.Generic;
using PX.Common;
using PX.Data.EP;
using System;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.Objects.GL;
using PX.Objects.CT;

namespace PX.Objects.CR.Standalone
{
	public partial class CROpportunityRevision : PX.Data.IBqlTable, IAssign
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(
			DescriptionField = typeof(opportunityID),
			Selector = typeof(opportunityID),
			IsKey = true)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region OpportunityID
		public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }

		public const int OpportunityIDLength = 10;

		[PXDBString(OpportunityIDLength, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Opportunity ID", Visibility = PXUIVisibility.SelectorVisible)]		
		[PXFieldDescription]
		public virtual String OpportunityID { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		private int? _BAccountID;
		[CustomerAndProspect(DisplayName = "Business Account")]
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

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<bAccountID>>>),
			DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		public virtual Int32? LocationID { get; set; }
		#endregion
		
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(Coalesce<
			Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<bAccountID>>, And<Location.locationID, Equal<Current<locationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
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
		
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		protected Int32? _ContactID;
		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search2<Contact.contactID,
			LeftJoin<BAccount, 
				On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
			Where2<Where<
					Contact.contactType, Equal<ContactTypesAttribute.person>,
						Or<Contact.contactType, Equal<ContactTypesAttribute.lead>>>,
					And<Where<BAccount.bAccountID, IsNull, 
						Or<Match<BAccount, Current<AccessInfo.userName>>>>>
				>>),
			DescriptionField = typeof(Contact.displayName),
			Filterable = true,
			DirtyRead = true)]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		public virtual Int32? ContactID
		{
			get { return _ContactID; }
			set { _ContactID = value; }
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
		
		#region OpportunityAddressID
		public abstract class opportunityAddressID : PX.Data.BQL.BqlInt.Field<opportunityAddressID> { }
		protected Int32? _OpportunityAddressID;
		[PXDBInt()]
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

		#region ShippingContactID
		public abstract class shipContactID : PX.Data.IBqlField
		{
		}
		protected Int32? _ShippingContactID;
		[PXDBInt()]
		public virtual Int32? ShipContactID
		{
			get
			{

				return this._ShippingContactID;
			}
			set
			{
				this._ShippingContactID = value;
			}
		}
		#endregion

		#region ShippingAddressID
		public abstract class shipAddressID : PX.Data.IBqlField
		{
		}
		protected Int32? _ShippingAddressID;
		[PXDBInt()]
		public virtual Int32? ShipAddressID
		{
			get
			{
				return this._ShippingAddressID;
			}
			set
			{
				this._ShippingAddressID = value;
			}
		}
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		[CustomerAndProspect(DisplayName = "Parent Account")]
		[PXFormula(typeof(Selector<BAccount.bAccountID, BAccount.parentBAccountID>))]
		public virtual Int32? ParentBAccountID { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }		
		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInCR, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute(typeof(bAccountID))]		
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region QuoteProjectID
		public abstract class quoteProjectID : PX.Data.BQL.BqlInt.Field<quoteProjectID> { }
		[PXUIField(DisplayName = "Project ID")]
		[PXDBInt()]
		[PXSelector(typeof(Search<PMProject.contractID, Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>>>), SubstituteKey = typeof(PMProject.contractCD), DescriptionField = typeof(PMProject.description))]
		public virtual Int32? QuoteProjectID { get; set; }
		#endregion

		#region QuoteProjectCD
		public abstract class quoteProjectCD : PX.Data.BQL.BqlString.Field<quoteProjectCD> { }
	
		[PXDBString()]
		[PXUIField(DisplayName = "Project ID")]
		[PXDimension(ProjectAttribute.DimensionName)]
		public virtual string QuoteProjectCD
		{
			get;
			set;
		}
		#endregion

		#region TemplateID
		public abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }
		[PXUIField(DisplayName = "Template", FieldClass = ProjectAttribute.DimensionName)]
		[PXDimensionSelectorAttribute(ProjectAttribute.DimensionName,
				typeof(Search2<PMProject.contractID,
						LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<PMProject.contractID>>>,
							Where<PMProject.baseType, Equal<CTPRType.projectTemplate>, And<PMProject.isActive, Equal<True>>>>),
				typeof(PMProject.contractCD),
				typeof(PMProject.contractCD),
				typeof(PMProject.description),
				typeof(PMProject.budgetLevel),
				typeof(ContractBillingSchedule.type),
				typeof(PMProject.billingID),
				typeof(PMProject.allocationID),
				typeof(PMProject.approverID),
				DescriptionField = typeof(PMProject.description))]
		[PXDBInt]
		public virtual Int32? TemplateID { get; set; }
		#endregion

		#region ProjectManager
		public abstract class projectManager : PX.Data.BQL.BqlInt.Field<projectManager> { }
			
		[PXDBInt]
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

		[PXDBString(255, IsFixed = true)]
		[PXUIField(DisplayName = "External Ref.")]
		public virtual string ExternalRef { get; set; }
		#endregion

		#region DocumentDate
		public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXMassUpdatableField]
		[PXUIField(DisplayName = "Estimation", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocumentDate { get; set; }
		#endregion
				
		#region CampaignSourceID
		public abstract class campaignSourceID : PX.Data.BQL.BqlString.Field<campaignSourceID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Source Campaign")]
		[PXSelector(typeof(Search3<CRCampaign.campaignID, OrderBy<Desc<CRCampaign.campaignID>>>),
			DescriptionField = typeof(CRCampaign.campaignName), Filterable = true)]
		public virtual String CampaignSourceID { get; set; }
		#endregion		

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXDBInt]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup")]
		[PXMassUpdatableField]
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXDBGuid()]
		[PXOwnerSelector(typeof(workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		[PXMassUpdatableField]
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault(typeof(Search<CRSetup.defaultCuryID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Currency.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXDBLong()]
		[CurrencyInfo(ModuleCode = "CR")]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion

		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal { get; set; }
		#endregion

		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineDiscountTotal))]
		[PXUIField(DisplayName = "Detail Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal { get; set; }
		#endregion

		#region CostTotal
		public abstract class costTotal : PX.Data.BQL.BqlDecimal.Field<costTotal> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CostTotal { get; set; }
		#endregion

		#region CuryCostTotal
		public abstract class curyCostTotal : PX.Data.BQL.BqlDecimal.Field<curyCostTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(costTotal))]
		[PXUIField(DisplayName = "Total Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryCostTotal { get; set; }
		#endregion

		#region ProgressiveTotal
		public abstract class progressiveTotal : PX.Data.BQL.BqlDecimal.Field<progressiveTotal> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ProgressiveTotal { get; set; }
		#endregion

		#region CuryProgressiveTotal
		public abstract class curyProgressiveTotal : PX.Data.BQL.BqlDecimal.Field<curyProgressiveTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(progressiveTotal))]
		[PXUIField(DisplayName = "Total Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryProgressiveTotal { get; set; }
		#endregion

		#region ExtPriceTotal
		public abstract class extPriceTotal : PX.Data.BQL.BqlDecimal.Field<extPriceTotal> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtPriceTotal { get; set; }
		#endregion

		#region CuryExtPriceTotal
		public abstract class curyExtPriceTotal : PX.Data.BQL.BqlDecimal.Field<curyExtPriceTotal> { }

		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Subtotal", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? CuryExtPriceTotal { get; set; }
        #endregion	   

        #region LineDiscountTotal
        public abstract class lineDiscountTotal : PX.Data.BQL.BqlDecimal.Field<lineDiscountTotal> { }

	    [PXDBDecimal(4)]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? LineDiscountTotal { get; set; }
        #endregion

        #region CuryLineDiscountTotal
        public abstract class curyLineDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDiscountTotal> { }

	    [PXDBCurrency(typeof(curyInfoID), typeof(lineDiscountTotal))]
	    [PXUIField(DisplayName = "Detail Discount Total", Enabled = false)]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? CuryLineDiscountTotal { get; set; }
        #endregion

	    #region LineDocDiscountTotal
	    public abstract class lineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<lineDocDiscountTotal> { }

	    [PXDBDecimal(4)]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? LineDocDiscountTotal { get; set; }
	    #endregion

	    #region CuryLineDocDiscountTotal
	    public abstract class curyLineDocDiscountTotal : PX.Data.BQL.BqlDecimal.Field<curyLineDocDiscountTotal> { }

	    [PXDBCurrency(typeof(curyInfoID), typeof(lineDocDiscountTotal))]
	    [PXUIField(Enabled = false)]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? CuryLineDocDiscountTotal { get; set; }
	    #endregion

        #region IsTaxValid
        public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		[PXDBBool()]
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

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal { get; set; }
		#endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(taxTotal))]
		[PXUIField(DisplayName = "Tax Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal { get; set; }
		#endregion

		#region ManualTotal
		public abstract class manualTotalEntry : PX.Data.BQL.BqlBool.Field<manualTotalEntry> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Amount")]
		public virtual Boolean? ManualTotalEntry { get; set; }
		#endregion

		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		private decimal? _amount;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? Amount
		{
			[PXDependsOnFields(typeof(lineTotal), typeof(manualTotalEntry))]
			get { return ManualTotalEntry == true ? _amount : LineTotal; }
			set { _amount = value; }
		}

		#endregion

		#region RawAmount
		public abstract class rawAmount : PX.Data.BQL.BqlDecimal.Field<rawAmount> { }

		[PXBaseCury]
		[PXDBCalced(typeof(Switch<Case<Where<manualTotalEntry, Equal<True>>, amount>, lineTotal>), typeof(decimal))]
		public virtual Decimal? RawAmount
		{
			get;
			set;
		}

		#endregion

		#region CuryAmount
		public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount> { }

		private decimal? _curyAmount;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(amount))]
		[PXDependsOnFields(typeof(manualTotalEntry), typeof(curyLineTotal))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryAmount
		{
			get { return ManualTotalEntry == true ? _curyAmount ?? 0 : CuryLineTotal; }
			set { _curyAmount = value; }
		}

		#endregion

		#region DiscTot
		public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscTot { get; set; }
		#endregion

		#region CuryDiscTot
		public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }

	    private decimal? _curyDiscTot;

        [PXDBCurrency(typeof(curyInfoID), typeof(discTot))]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    [PXUIField(DisplayName = "Discount")]	    
	    public virtual Decimal? CuryDiscTot
	    {
	        get { return _curyDiscTot; }
	        set { _curyDiscTot = value; }
        }
        #endregion

        #region ProductsAmount
        public abstract class productsAmount : PX.Data.BQL.BqlDecimal.Field<productsAmount> { }
		[PXDependsOnFields(typeof(amount), typeof(discTot), typeof(manualTotalEntry), typeof(lineTotal))]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Products Amount")]
		public virtual Decimal? ProductsAmount
		{
			get
			{
				return (Amount ?? 0) - (DiscTot ?? 0) + (TaxTotal ?? 0);
			}
		}
		#endregion

		#region CuryProductsAmount
		public abstract class curyProductsAmount : PX.Data.BQL.BqlDecimal.Field<curyProductsAmount> { }
		[PXDependsOnFields(typeof(curyAmount), typeof(curyDiscTot), typeof(curyTaxTotal))]
		[PXDBCurrency(typeof(curyInfoID), typeof(productsAmount))]
		[PXUIField(DisplayName = "Total", Enabled = false)]
		public virtual Decimal? CuryProductsAmount
		{
			get
			{
				return (CuryAmount ?? 0m) - (CuryDiscTot ?? 0m) + (CuryTaxTotal ?? 0m);
			}
		}
        #endregion        

        #region CuryWgtAmount
        public abstract class curyWgtAmount : PX.Data.BQL.BqlDecimal.Field<curyWgtAmount> { }

		[PXDecimal]
		[PXUIField(DisplayName = "Wgt. Total", Enabled = false)]
		public virtual Decimal? CuryWgtAmount { get; set; }
		#endregion

		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(vatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal { get; set; }
		#endregion

		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal { get; set; }
		#endregion

		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(vatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal { get; set; }
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal { get; set; }
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXFormula(typeof(Default<branchID>))]
		[PXFormula(typeof(Default<locationID>))]
		public virtual String TaxZoneID { get; set; }
        #endregion

		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<Location.cTaxCalcMode, Where<Location.bAccountID, Equal<Current<CROpportunityRevision.bAccountID>>,
			And<Location.locationID, Equal<Current<CROpportunityRevision.locationID>>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode { get; set; }
		#endregion

	    #region QuoteStatus
	    public abstract class quoteStatus : PX.Data.BQL.BqlString.Field<quoteStatus> { }
	    [PXString(IsUnicode = true)]
        [PXUIField(DisplayName = "Quote Status")]
        [CRQuoteStatus]
        public virtual string QuoteStatus { get; set; }
		#endregion

		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion

		#region CuryDocDiscTot
		public abstract class curyDocDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDocDiscTot> { }

		[PXDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CuryDocDiscTot { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		[PXUIField(DisplayName = "Created By")]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "Date Created", Enabled = false)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		[PXUIField(DisplayName = "Last Modified By")]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion	

		#region ProductCntr
		public abstract class productCntr : PX.Data.BQL.BqlInt.Field<productCntr> { }

		[PXDBInt]
		[PXDefault(0)]
		public virtual Int32? ProductCntr { get; set; }

		#endregion

		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		[PXDBInt]
		[PXDefault(0)]
		public virtual Int32? LineCntr { get; set; }

		#endregion

		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual Boolean? Approved { get; set; }
        #endregion

        #region Rejected
        public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual Boolean? Rejected { get; set; }
        #endregion
    }
}
