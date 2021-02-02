using Autofac;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Discount;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.Extensions.ContactAddress;
using PX.Objects.Extensions.Discount;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.MultiCurrency.CR;
using PX.Objects.Extensions.SalesPrice;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
namespace PX.Objects.CR
{
    public class OpportunityMaint : PXGraph<OpportunityMaint>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
	{
		#region Filters
		
		#region CreateSalesOrderFilter
		[Serializable()]
		[PXHidden]
		public partial class CreateSalesOrderFilter : IBqlTable
		{
			#region OrderType

			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

            [PXRestrictor(typeof(Where<SOOrderType.active, Equal<boolTrue>>), Messages.OrderTypeIsNotActive, typeof(SOOrderType.descr))]
			[PXDBString(2, IsFixed = true, InputMask = ">aa")]
            [PXDefault(typeof(Search2<SOOrderType.orderType,
                    InnerJoin<SOSetup, On<SOOrderType.orderType, Equal<SOSetup.defaultOrderType>>>,
                    Where<SOOrderType.active, Equal<boolTrue>>>))]
			[PXSelector(typeof (Search<SOOrderType.orderType>),
				DescriptionField = typeof (SOOrderType.descr))]
			[PXUIField(DisplayName = "Order Type")]
			public virtual String OrderType { get; set; }

			#endregion
			
			#region RecalcDiscounts

			public abstract class recalcDiscount : PX.Data.BQL.BqlBool.Field<recalcDiscount> { }

			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Recalculate Prices and Discounts")]
			[Obsolete]
			public virtual bool? RecalcDiscounts { get; set; }
			#endregion

		    #region RecalculatePrices
		    public abstract class recalculatePrices : PX.Data.BQL.BqlBool.Field<recalculatePrices> { }
		    [PXBool()]
		    [PXDefault(false)]
		    [PXUIField(DisplayName = "Recalculate Prices")]
		    public virtual bool? RecalculatePrices { get; set; }
		    #endregion

		    #region Override Manual Prices
		    public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
		    [PXBool()]
		    [PXDefault(false)]
		    [PXUIField(DisplayName = "Override Manual Prices")]
		    public virtual bool? OverrideManualPrices { get; set; }
		    #endregion

		    #region Recalculate Discounts
		    public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
		    [PXBool()]
			[PXDefault(false)]
		    [PXUIField(DisplayName = "Recalculate Discounts")]
		    public virtual bool? RecalculateDiscounts { get; set; }
		    #endregion

		    #region Override Manual Discounts
		    public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
		    [PXBool()]
		    [PXDefault(false)]
		    [PXUIField(DisplayName = "Override Manual Line Discounts")]
		    public virtual bool? OverrideManualDiscounts { get; set; }
			#endregion

			#region OverrideManualDocGroupDiscounts
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
			public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
			public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
			#endregion

			#region ConfirmManualAmount
			public abstract class confirmManualAmount : PX.Data.BQL.BqlBool.Field<confirmManualAmount> { }
			[PXBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Create a Sales Order Regardless of the Specified Manual Amount")]
			public virtual bool? ConfirmManualAmount { get; set; }
			#endregion
		}
        #endregion

        #region CreateQuotesFilter
        [Serializable]
	    public partial class CreateQuotesFilter : IBqlTable
	    {
            #region QuoteType
            public abstract class quoteType : PX.Data.BQL.BqlString.Field<quoteType> { }
            [PXDBString(1, IsFixed = true)]
            [PXUIField(DisplayName = "Quote Type")]
            [CRQuoteType()]
            [PXDefault(CRQuoteTypeAttribute.Distribution)]
            public virtual string QuoteType { get; set; }
            #endregion

            #region AddProductsFromOpportunity
            public abstract class addProductsFromOpportunity : PX.Data.BQL.BqlBool.Field<addProductsFromOpportunity> { }
	        [PXBool()]
            [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
	        [PXUIField(DisplayName = "Add Details From Opportunity")]
	        public virtual bool? AddProductsFromOpportunity { get; set; }
            #endregion

            #region MakeNewQuotePrimary
            public abstract class makeNewQuotePrimary : PX.Data.BQL.BqlBool.Field<makeNewQuotePrimary> { }
	        [PXBool()]
            [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
	        [PXUIField(DisplayName = "Make New Quote Primary")]
	        public virtual bool? MakeNewQuotePrimary { get; set; }
            #endregion

            #region RecalculatePrices
            public abstract class recalculatePrices : PX.Data.BQL.BqlBool.Field<recalculatePrices> { }
	        [PXBool()]
	        [PXDefault(false)]
            [PXUIField(DisplayName = "Recalculate Prices")]
	        public virtual bool? RecalculatePrices { get; set; }
            #endregion

            #region Override Manual Prices
            public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
	        [PXBool()]
	        [PXDefault(false)]
	        [PXUIField(DisplayName = "Override Manual Prices")]
	        public virtual bool? OverrideManualPrices { get; set; }
            #endregion

            #region Recalculate Discounts
	        public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
            [PXBool()]
	        [PXDefault(false)]
	        [PXUIField(DisplayName = "Recalculate Discounts")]
            public virtual bool? RecalculateDiscounts { get; set; }
            #endregion

	        #region Override Manual Discounts
	        public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
	        [PXBool()]
	        [PXDefault(false)]
	        [PXUIField(DisplayName = "Override Manual Line Discounts")]
	        public virtual bool? OverrideManualDiscounts { get; set; }
	        #endregion

			#region OverrideManualDocGroupDiscounts
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
			public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
			public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
			#endregion
        }
        #endregion

        #region CreateInvoicesFilter
        [Serializable]
	    public partial class CreateInvoicesFilter : IBqlTable
	    {
	        #region RecalculatePrices
	        public abstract class recalculatePrices : PX.Data.BQL.BqlBool.Field<recalculatePrices> { }
	        [PXBool()]
	        [PXDefault(false)]
	        [PXUIField(DisplayName = "Recalculate Prices")]
	        public virtual bool? RecalculatePrices { get; set; }
	        #endregion

	        #region Override Manual Prices
	        public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
	        [PXBool()]
	        [PXDefault(false)]
	        [PXUIField(DisplayName = "Override Manual Prices")]
	        public virtual bool? OverrideManualPrices { get; set; }
	        #endregion

	        #region Recalculate Discounts
	        public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
	        [PXBool()]
	        [PXDefault(false)]
	        [PXUIField(DisplayName = "Recalculate Discounts")]
	        public virtual bool? RecalculateDiscounts { get; set; }
	        #endregion

	        #region Override Manual Discounts
	        public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
	        [PXBool()]
	        [PXDefault(false)]
	        [PXUIField(DisplayName = "Override Manual Line Discounts")]
	        public virtual bool? OverrideManualDiscounts { get; set; }
	        #endregion

			#region OverrideManualDocGroupDiscounts
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
			public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
			public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
			#endregion

			#region ConfirmManualAmount
			public abstract class confirmManualAmount : PX.Data.BQL.BqlBool.Field<confirmManualAmount> { }
			[PXBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Create an Invoice for the Specified Manual Amount")]
			public virtual bool? ConfirmManualAmount { get; set; }
			#endregion
        }
        #endregion

        #region CopyQuoteFilter
        [Serializable]
        public partial class CopyQuoteFilter : IBqlTable
        {
            #region OpportunityID
            public abstract class opportunityId : PX.Data.BQL.BqlString.Field<opportunityId> { }

            [PXString()]
            [PXUIField(DisplayName = "Opportunity ID", Visible = false)]
            [PXSelector(typeof(Search2<CROpportunity.opportunityID,
                LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
                    LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>>>,
                Where<CROpportunity.isActive.IsEqual<True>>,
                OrderBy<Desc<CROpportunity.opportunityID>>>),
            new[] { typeof(CROpportunity.opportunityID),
                typeof(CROpportunity.subject),
                typeof(CROpportunity.status),
                typeof(CROpportunity.stageID),
                typeof(CROpportunity.classID),
                typeof(BAccount.acctName),
                typeof(Contact.displayName),
                typeof(CROpportunity.subject),
                typeof(CROpportunity.externalRef),
                typeof(CROpportunity.closeDate) },
            Filterable = true)]
            [PXDefault(typeof(CROpportunity.opportunityID))]
            public virtual string OpportunityID { get; set; }
            #endregion

            #region Description
            public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
            protected string _Description;
            [PXDefault()]
            [PXString(60, IsUnicode = true)]
            [PXUIField(DisplayName = "Description", Required = true)]
            public virtual string Description
            {
                get { return _Description; }
                set { _Description = value; }
            }
            #endregion

            #region RecalculatePrices
            public abstract class recalculatePrices : PX.Data.BQL.BqlBool.Field<recalculatePrices> { }
            [PXBool()]
            [PXUIField(DisplayName = "Recalculate Prices")]
            [PXDefault(false)]
            public virtual bool? RecalculatePrices { get; set; }
            #endregion

            #region OverrideManualPrices
            public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
            [PXBool()]
            [PXUIField(DisplayName = "Override Manual Prices", Enabled = false)]
            [PXDefault(false)]
            public virtual bool? OverrideManualPrices { get; set; }
            #endregion

            #region RecalculateDiscounts
            public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
            [PXBool()]
            [PXUIField(DisplayName = "Recalculate Discounts")]
            [PXDefault(false)]
            public virtual bool? RecalculateDiscounts { get; set; }
            #endregion

            #region OverrideManualDiscounts
            public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
            [PXBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Override Manual Line Discounts", Enabled = false)]
            public virtual bool? OverrideManualDiscounts { get; set; }
            #endregion

			#region OverrideManualDocGroupDiscounts
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
			public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
			public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
			#endregion
        }
        #endregion
		
		#endregion

		#region Selects / Views
        [PXHidden()]
        public PXSelect<BAccount> BAccounts;

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;

        [PXHidden]
        public PXSelect<BAccountR>
            bAccountRBasic;

        [PXHidden]
		public PXSetupOptional<SOSetup>
			sosetup;

		[PXHidden]
		public PXSetup<CRSetup>
			Setup;

		[PXCopyPasteHiddenFields(typeof(CROpportunity.details), typeof(CROpportunity.stageID), typeof(CROpportunity.resolution))]
		[PXViewName(Messages.Opportunity)]
		public PXSelectJoin<CROpportunity,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>>,
				Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			Opportunity;

		[PXHidden]
        public PXSelect<CROpportunityRevision>
            OpportunityRevision;

        [PXHidden]
		public PXSelect<CRLead> Leads;

		[PXCopyPasteHiddenFields(typeof(CROpportunity.details), typeof(CROpportunity.stageID), typeof(CROpportunity.resolution))]
		public PXSelect<CROpportunity,
			Where<CROpportunity.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
			OpportunityCurrent;

		[PXHidden]
		public PXSelectReadonly<CRQuote, Where<CRQuote.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>> PrimaryQuoteQuery;

		[PXHidden]
		public PXSelect<CROpportunityProbability,
			Where<CROpportunityProbability.stageCode, Equal<Current<CROpportunity.stageID>>>>
			ProbabilityCurrent;

		[PXHidden]
		public PXSelect<Address>
			Address;

		[PXHidden]
		public PXSetup<Contact, Where<Contact.contactID, Equal<Optional<CROpportunity.contactID>>>> Contacts;

		[PXHidden]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<CROpportunity.bAccountID>>>> customer;


		[PXViewName(Messages.Answers)]
		public CRAttributeSourceList<CROpportunity, CROpportunity.contactID>
			Answers;

        public PXSetup<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.classID>>>> OpportunityClass;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
        [CRReference(typeof(CROpportunity.bAccountID),
			typeof(Select<Contact, 
				Where<Current<CROpportunity.allowOverrideContactAddress>, NotEqual<True>, 
					And<Contact.contactID, Equal<Current<CROpportunity.contactID>>>>>))]
        public OpportunityActivities Activities;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics>
			ActivityStatistics;

		public virtual IEnumerable activityStatistics()
		{
			var opp = ActivityOpportunityStatistics.SelectSingle();
			var qt = ActivityQuoteStatistics.SelectSingle();
			var lastIn = opp?.LastIncomingActivityDate > qt?.LastIncomingActivityDate ? opp : qt;
			var lastOut = opp?.LastOutgoingActivityDate > qt?.LastOutgoingActivityDate ? opp : qt;

			yield return (opp != null && qt != null)
				? new CRActivityStatistics()
				{
					LastActivityDate = lastIn.LastActivityDate > lastOut.LastActivityDate ? lastIn.LastActivityDate : lastOut.LastActivityDate,
					LastIncomingActivityDate = lastIn.LastIncomingActivityDate,
					LastIncomingActivityNoteID = lastIn.LastIncomingActivityNoteID,
					LastOutgoingActivityDate = lastOut.LastOutgoingActivityDate,
					LastOutgoingActivityNoteID = lastOut.LastOutgoingActivityNoteID
				}
				: opp ?? qt;
		}

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<CROpportunity.noteID>>>>
			ActivityOpportunityStatistics;

		[PXCopyPasteHiddenView]
		public PXSelect<CRActivityStatistics,
				Where<CRActivityStatistics.noteID, Equal<Current<CRQuote.noteID>>>>
			ActivityQuoteStatistics;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<CROpportunity.noteID>
			Relations;

	    [PXCopyPasteHiddenView]	    
	    [PXFilterable]
	    public PXSelect<CRQuote, Where<CRQuote.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
            Quotes;

	    [PXViewName(Messages.CopyQuote)]
		[PXCopyPasteHiddenView]
        public PXFilter<CopyQuoteFilter> CopyQuoteInfo;

		[PXViewName(Messages.OpportunityProducts)]
		[PXImport(typeof(CROpportunity))]
		public PXOrderedSelect<CROpportunity, CROpportunityProducts,
			Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
			OrderBy<Asc<CROpportunityProducts.sortOrder>>>
			Products;
		
	    public PXSelect<CROpportunityTax,
			Where<CROpportunityTax.quoteID, Equal<Current<CROpportunity.quoteNoteID>>,
			  And<CROpportunityTax.lineNbr, Less<intMax>>>,
            OrderBy<Asc<CROpportunityTax.taxID>>> TaxLines;

		 [PXViewName(Messages.OpportunityTax)]
		 public PXSelectJoin<CRTaxTran,
			 InnerJoin<Tax, On<Tax.taxID, Equal<CRTaxTran.taxID>>>,
			 Where<CRTaxTran.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
			 OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>> Taxes;

		[PXViewName(Messages.CreateSalesOrder)]
		[PXCopyPasteHiddenView]
		public CRPopupFilter<CreateSalesOrderFilter>
			CreateOrderParams;

	    public PXSetup<Location, 
            Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>,
              And<Location.locationID, Equal<Optional<CROpportunity.locationID>>>>> location;

	    [PXViewName(Messages.CreateQuote)]
		[PXCopyPasteHiddenView]
	    public PXFilter<CreateQuotesFilter> QuoteInfo;

	    [PXViewName(Messages.CreateInvoice)]
		[PXCopyPasteHiddenView]
	    public CRPopupFilter<CreateInvoicesFilter> InvoiceInfo;

        [PXViewName(Messages.OpportunityContact)]
        public PXSelect<CRContact, Where<CRContact.contactID, Equal<Current<CROpportunity.opportunityContactID>>>> Opportunity_Contact;

        [PXViewName(Messages.OpportunityAddress)]
        public PXSelect<CRAddress, Where<CRAddress.addressID, Equal<Current<CROpportunity.opportunityAddressID>>>> Opportunity_Address;

        [PXViewName(Messages.ShippingContact)]
        public PXSelect<CRShippingContact, Where<CRShippingContact.contactID, Equal<Current<CROpportunity.shipContactID>>>> Shipping_Contact;

        [PXViewName(Messages.ShippingAddress)]
        public PXSelect<CRShippingAddress, Where<CRShippingAddress.addressID, Equal<Current<CROpportunity.shipAddressID>>>> Shipping_Address;

        [PXHidden]
        public PXSelectJoin<Contact,
            LeftJoin<Address, On<Contact.defAddressID, Equal<Address.addressID>>>,  
            Where<Contact.contactID, Equal<Current<CROpportunity.contactID>>>> CurrentContact;       
		
        [PXHidden]
        public PXSelect<SOBillingContact> CurrentSOBillingContact;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<SOOrder,
			InnerJoin<CRRelation, On<CRRelation.refNoteID, Equal<SOOrder.noteID>>>,
			Where<CRRelation.targetNoteID, Equal<Current<CROpportunity.noteID>>, And<CRRelation.role, Equal<CRRoleTypeList.source>>>> SalesOrders;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<ARInvoice,
			InnerJoin<CRRelation, On<CRRelation.refNoteID, Equal<ARInvoice.noteID>>>,
			Where<CRRelation.targetNoteID, Equal<Current<CROpportunity.noteID>>, And<CRRelation.role, Equal<CRRoleTypeList.source>>>> Invoices;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<
			CROpportunityProducts,
			InnerJoin<InventoryItem,
				On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>>,
			Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunity.quoteNoteID>>,
				And<Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>>>
			ProductsByQuoteIDAndInventoryCD;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }
		#endregion

		#region Ctors

		public OpportunityMaint()
		{
			var crsetup = Setup.Current;

			if (string.IsNullOrEmpty(Setup.Current.OpportunityNumberingID))
			{
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
			}

			//Have to be remnoved for multicurrency.
			this.Views.Caches.Remove(typeof(CRQuote));

			Activities.GetNewEmailAddress =
				() =>
				{
					var current = Opportunity.Current;
					if (current != null)
					{
						var contact = current.OpportunityContactID.
							With(_ => (CRContact)PXSelect<CRContact,
								Where<CRContact.contactID, Equal<Required<CRContact.contactID>>>>.
							Select(this, _.Value));
						if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
							return PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(contact.Email, contact.DisplayName);
					}
					return String.Empty;
				};

		    if (!PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
		    {
		        createSalesOrder.SetEnabled(false);
		    }

		    actionsFolder.MenuAutoOpen = true;

			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);

			Caches[typeof(SOOrder)].AllowUpdate = Caches[typeof(SOOrder)].AllowInsert = Caches[typeof(SOOrder)].AllowDelete = false;
			Caches[typeof(ARInvoice)].AllowUpdate = Caches[typeof(ARInvoice)].AllowInsert = Caches[typeof(ARInvoice)].AllowDelete = false;
		}
		
		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<CROpportunity>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(CROpportunityProducts), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<CROpportunityProducts.quoteID>(((OpportunityMaint)graph).Opportunity.Current?.QuoteNoteID)
						};
					}));
			}
		}

		#endregion

		#region Actions

		public PXSave<CROpportunity> Save;
		public PXCancel<CROpportunity> Cancel;
		public PXInsert<CROpportunity> Insert;
		public PXCopyPasteAction<CROpportunity> CopyPaste;
		public PXDelete<CROpportunity> Delete;
		public PXFirst<CROpportunity> First;
		public PXPrevious<CROpportunity> Previous;
		public PXNext<CROpportunity> Next;
		public PXLast<CROpportunity> Last;

		public PXAction<CROpportunity> createQuote;
		[PXUIField(DisplayName = Messages.CreateQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual IEnumerable CreateQuote(PXAdapter adapter)
		{
			foreach (CROpportunity opportunity in adapter.Get<CROpportunity>())
			{
				if (QuoteInfo.View.Answer == WebDialogResult.None)
				{
					QuoteInfo.Cache.Clear();
					QuoteInfo.Cache.Insert();
				}

				WebDialogResult result = QuoteInfo.AskExt();

				if (result == WebDialogResult.Cancel)
					yield return opportunity;

				Opportunity.Current = opportunity;
				Actions.PressSave();

				if (QuoteInfo.Current.QuoteType == CRQuoteTypeAttribute.Project)
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && OpportunityCurrent.Current.CuryID != (Accessinfo.BaseCuryID ?? new PXSetup<PX.Objects.GL.Company>(this).Current?.BaseCuryID))
					throw new PXException(Messages.CannotCreateProjectQuoteBecauseOfCury);

					if (PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() && OpportunityCurrent.Current.TaxCalcMode != TaxCalculationMode.TaxSetting)
						throw new PXException(Messages.CannotCreateProjectQuoteBecauseOfTaxCalcMode);
				}

				if ((QuoteInfo.Current.QuoteType == CRQuoteTypeAttribute.Distribution))
					PXLongOperation.StartOperation(this, () => CreateNewQuote(opportunity, QuoteInfo.Current, result));
				else
					PXLongOperation.StartOperation(this, () => CreateNewProjectQuote(opportunity, QuoteInfo.Current, result));

				yield return opportunity;
			}
		}

	    public PXAction<CROpportunity> actionsFolder;

        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable ActionsFolder(PXAdapter adapter)
        {
            return adapter.Get();
        }

		public PXAction<CROpportunity> createInvoice;
		[PXUIField(DisplayName = Messages.CreateInvoice, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable CreateInvoice(PXAdapter adapter)
		{
			CROpportunity opportunity = this.Opportunity.Current;

		    if (opportunity.BAccountID == null)
		    {
		        WebDialogResult result = Opportunity.View.Ask(opportunity, Messages.AskConfirmation, Messages.InvoiceRequiredCustomerAccount, MessageButtons.YesNo, MessageIcon.Question);
		        if (result == WebDialogResult.Yes)
		        {
		            Actions[nameof(CreateBothAccountAndContactFromOpportunityGraphExt.CreateBothContactAndAccount)].Press();
		        }

                return adapter.Get();
		    }

			Customer customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>.Select(this);
			if (customer == null)
			{
				throw new PXException(Messages.ProspectNotCustomer);
			}

			var nonStockItems = PXSelectJoin<InventoryItem,
				InnerJoin<CROpportunityProducts, On<CROpportunityProducts.inventoryID, Equal<InventoryItem.inventoryID>>>,
				Where<InventoryItem.stkItem, Equal<False>,
					And<CROpportunityProducts.quoteID, Equal<Required<CROpportunity.quoteNoteID>>>>>.
				Select(this, opportunity.QuoteNoteID);
			if (nonStockItems.Count == 0)
			{
				throw new PXException(Messages.InvoiceHasOnlyNonStockLines);
			}

			if (opportunity.BAccountID != null)
		    {
		        var baccountQuery = new PXSelectJoin<BAccount, LeftJoin<Contact,
		                On<Contact.contactID, Equal<BAccount.defContactID>>>,
		            Where<BAccount.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>(this);
                  var baccount = (BAccount)baccountQuery.SelectSingle();
		        if (baccount.Type == BAccountType.VendorType || baccount.Type == BAccountType.ProspectType)
		        {
		            WebDialogResult result = Opportunity.View.Ask(opportunity, Messages.AskConfirmation, Messages.InvoiceRequiredConvertBusinessAccountToCustomerAccount, MessageButtons.YesNo, MessageIcon.Question);
		            if (result == WebDialogResult.Yes)
		            {
		                PXLongOperation.StartOperation(this, () => ConvertToCustomerAccount(baccount, opportunity));
		            }
		            
		            return adapter.Get();
                }
            }

			if (customer != null)
			{
			    if (InvoiceInfo.View.Answer == WebDialogResult.None)
			    {
			        InvoiceInfo.Cache.Clear();
                    InvoiceInfo.Cache.Insert();
			    }
				
				if (InvoiceInfo.AskExtFullyValid((graph, viewName) => { }, DialogAnswerType.Positive))
				{
					Actions.PressSave();
					PXLongOperation.StartOperation(this, delegate()
					{
						var grapph = PXGraph.CreateInstance<OpportunityMaint>();
						grapph.Opportunity.Current = opportunity;
						grapph.DoCreateInvoice(InvoiceInfo.Current);
					});
				}
			}

			return adapter.Get();
		}

        protected virtual void DoCreateInvoice(CreateInvoicesFilter param)
        {
	        bool recalcAny = param.RecalculatePrices == true ||
	                         param.RecalculateDiscounts == true ||
	                         param.OverrideManualDiscounts == true ||
							 param.OverrideManualDocGroupDiscounts == true ||
	                         param.OverrideManualPrices == true;

            var opportunity = this.Opportunity.Current;
            ARInvoiceEntry docgraph = PXGraph.CreateInstance<ARInvoiceEntry>();

            Customer customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>.Select(this);
            docgraph.customer.Current = customer;

            CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>.Select(this);
            info.CuryInfoID = null;
            info = CurrencyInfo.GetEX(docgraph.currencyinfo.Insert(info.GetCM()));

            ARInvoice invoice = new ARInvoice();
            invoice.DocType = ARDocType.Invoice;
            invoice.CuryID = info.CuryID;
            invoice.CuryInfoID = info.CuryInfoID;
            invoice.DocDate = opportunity.CloseDate;
            invoice.Hold = true;
            invoice.BranchID = opportunity.BranchID;
            invoice.CustomerID = opportunity.BAccountID;
            invoice = PXCache<ARInvoice>.CreateCopy(docgraph.Document.Insert(invoice));

            invoice.TermsID = customer.TermsID;
            invoice.InvoiceNbr = opportunity.OpportunityID;
            invoice.DocDesc = opportunity.Subject;
            invoice.CustomerLocationID = opportunity.LocationID ?? customer.DefLocationID;

			CRContact _CRContact = Opportunity_Contact.SelectSingle();
			CRAddress _CRAddress = Opportunity_Address.SelectSingle();
			// Insert

			if (_CRAddress != null)
			{
				ARAddress _billingAddress = docgraph.Billing_Address.Select();

				if (_billingAddress != null)
				{
					_billingAddress.AddressLine1 = _CRAddress.AddressLine1;
					_billingAddress.AddressLine2 = _CRAddress.AddressLine2;
					_billingAddress.City = _CRAddress.City;
					_billingAddress.CountryID = _CRAddress.CountryID;
					_billingAddress.State = _CRAddress.State;
					_billingAddress.IsValidated = _CRAddress.IsValidated;
					_billingAddress.PostalCode = _CRAddress.PostalCode;
					_billingAddress.IsDefaultAddress = _CRAddress.IsDefaultAddress;
					_billingAddress = docgraph.Billing_Address.Update(_billingAddress);
				}
			}

			if (_CRContact != null)
			{
				ARContact _billingContact = docgraph.Billing_Contact.Select();

				if (_billingContact != null)
				{
					_billingContact.FullName = _CRContact.FullName;
					_billingContact.Salutation = _CRContact.Salutation;
					_billingContact.Attention = _CRContact.Attention;
					_billingContact.Phone1 = _CRContact.Phone1;
					_billingContact.Email = _CRContact.Email;
					_billingContact.IsDefaultContact = _CRContact.IsDefaultContact;
					_billingContact = docgraph.Billing_Contact.Update(_billingContact);
				}
			}

			CRShippingAddress _crShippingAddress = Shipping_Address.SelectSingle();
			ARShippingAddress _shippingAddress = docgraph.Shipping_Address.Select();
			if (_shippingAddress != null && _crShippingAddress != null)
			{
				_shippingAddress.IsValidated = _crShippingAddress.IsValidated;
				_shippingAddress.AddressLine1 = _crShippingAddress.AddressLine1;
				_shippingAddress.AddressLine2 = _crShippingAddress.AddressLine2;
				_shippingAddress.City = _crShippingAddress.City;
				_shippingAddress.CountryID = _crShippingAddress.CountryID;
				_shippingAddress.State = _crShippingAddress.State;
				_shippingAddress.PostalCode = _crShippingAddress.PostalCode;
				_shippingAddress = docgraph.Shipping_Address.Update(_shippingAddress);
				_shippingAddress.IsDefaultAddress = _crShippingAddress.IsDefaultAddress;
				_shippingAddress = docgraph.Shipping_Address.Update(_shippingAddress);
			}

			CRShippingContact _crShippingContact = Shipping_Contact.SelectSingle();
			ARShippingContact _shippingContact = docgraph.Shipping_Contact.Select();
			if (_shippingContact != null && _crShippingContact != null)
			{
				_shippingContact.FullName = _crShippingContact.FullName;
				_shippingContact.Attention = _crShippingContact.Attention;
				_shippingContact.Phone1 = _crShippingContact.Phone1;
				_shippingContact.Phone1Type = _crShippingContact.Phone1Type;
				_shippingContact.Phone2 = _crShippingContact.Phone2;
				_shippingContact.Phone2Type = _crShippingContact.Phone2Type;
				_shippingContact.Email = _crShippingContact.Email;
				_shippingContact = docgraph.Shipping_Contact.Update(_shippingContact);
				_shippingContact.IsDefaultContact = _crShippingContact.IsDefaultContact;
				_shippingContact = docgraph.Shipping_Contact.Update(_shippingContact);
			}

	        if (Opportunity.Current.ManualTotalEntry == true)
		        recalcAny = false;

            if (opportunity.TaxZoneID != null)
            {
                invoice.TaxZoneID = opportunity.TaxZoneID;   
                if(!recalcAny && Opportunity.Current.ManualTotalEntry != true)	                
                    TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
            }
            invoice.TaxCalcMode = opportunity.TaxCalcMode;
            invoice.ProjectID = opportunity.ProjectID;
            invoice = docgraph.Document.Update(invoice);

			var campaignRelation = docgraph.RelationsLink.Insert();
			campaignRelation.RefNoteID = invoice.NoteID;
			campaignRelation.Role = CRRoleTypeList.Source;
			campaignRelation.TargetType = CRTargetEntityType.CROpportunity;
			campaignRelation.TargetNoteID = opportunity.NoteID;
			campaignRelation.DocNoteID = opportunity.NoteID;
			campaignRelation.EntityID = opportunity.BAccountID;
			campaignRelation.ContactID = opportunity.ContactID;
			docgraph.RelationsLink.Update(campaignRelation);
            
            if (Opportunity.Current.ManualTotalEntry == true)
            {
                ARTran tran = new ARTran();
                tran.Qty = 1;
                tran.CuryUnitPrice = Opportunity.Current.CuryAmount;
                tran = docgraph.Transactions.Insert(tran);
                if (tran != null)
                {
                    tran.CuryDiscAmt = opportunity.CuryDiscTot;

                    using (new PXLocaleScope(customer.LocaleName))
					{
						tran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.ManualAmount);
					}
                }
                tran = docgraph.Transactions.Update(tran);
            }
            else
            {
				foreach (PXResult<CROpportunityProducts, InventoryItem> res in PXSelectJoin<CROpportunityProducts,
					LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>>,
					Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>, 
                    And<InventoryItem.stkItem, Equal<False>>>>
                .Select(this))
				{
					CROpportunityProducts product = (CROpportunityProducts)res;
					InventoryItem item = (InventoryItem)res;

					ARTran tran = new ARTran();
					tran = docgraph.Transactions.Insert(tran);
					if (tran != null)
					{
						tran.InventoryID = product.InventoryID;
						using (new PXLocaleScope(customer.LocaleName))
						{
							tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(this.Caches[typeof(CROpportunityProducts)],
														  product, typeof(CROpportunityProducts.descr).Name, this.Culture.Name);
						}

						tran.Qty = product.Quantity;
						tran.UOM = product.UOM;
						tran.CuryUnitPrice = product.CuryUnitPrice;
						tran.IsFree = product.IsFree;
						tran.SortOrder = product.SortOrder;

						tran.CuryTranAmt = product.CuryAmount;
						tran.TaxCategoryID = product.TaxCategoryID;
						tran.ProjectID = product.ProjectID;
						tran.TaskID = product.TaskID;
						tran.CostCodeID = product.CostCodeID;

						if (param.RecalculatePrices != true)
						{
							tran.ManualPrice = true;
						}
						else
						{
							if (param.OverrideManualPrices != true)
								tran.ManualPrice = product.ManualPrice;
							else
								tran.ManualPrice = false;
						}

						if (param.RecalculateDiscounts != true)
						{
							tran.ManualDisc = true;
						}
						else
						{
							if (param.OverrideManualDiscounts != true)
								tran.ManualDisc = product.ManualDisc;
							else
								tran.ManualDisc = false;
						}

						tran.CuryDiscAmt = product.CuryDiscAmt;
						tran.DiscAmt = product.DiscAmt;
						tran.DiscPct = product.DiscPct;

						if (item.Commisionable.HasValue)
						{
							tran.Commissionable = item.Commisionable;
						}
					}

					tran = docgraph.Transactions.Update(tran);

					PXNoteAttribute.CopyNoteAndFiles(Products.Cache, product, docgraph.Transactions.Cache, tran, Setup.Current);
				}
            }
            PXNoteAttribute.CopyNoteAndFiles(Opportunity.Cache, opportunity, docgraph.Document.Cache, invoice, Setup.Current);

			//Skip all customer dicounts
			if (param.RecalculateDiscounts != true && param.OverrideManualDiscounts != true)
			{
				var discounts = new Dictionary<string, ARInvoiceDiscountDetail>();
				foreach (ARInvoiceDiscountDetail discountDetail in docgraph.ARDiscountDetails.Select())
				{
					docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.skipDiscount>(discountDetail, true);
					string key = discountDetail.Type + ':' + discountDetail.DiscountID + ':' + discountDetail.DiscountSequenceID;
					discounts.Add(key, discountDetail);
				}
				Discount ext = this.GetExtension<Discount>();
				foreach (CROpportunityDiscountDetail discountDetail in ext.DiscountDetails.Select())
				{
					ARInvoiceDiscountDetail detail;
					string key = discountDetail.Type + ':' + discountDetail.DiscountID + ':' + discountDetail.DiscountSequenceID;
					if (discounts.TryGetValue(key, out detail))
					{
						docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.skipDiscount>(detail, false);
						if (discountDetail.IsManual == true && discountDetail.Type == DiscountType.Document)
						{
							docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.extDiscCode>(detail, discountDetail.ExtDiscCode);
							docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.description>(detail, discountDetail.Description);
							docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.isManual>(detail, discountDetail.IsManual);
							docgraph.ARDiscountDetails.SetValueExt<ARInvoiceDiscountDetail.curyDiscountAmt>(detail, discountDetail.CuryDiscountAmt);
						}
					}
					else
					{
						detail = (ARInvoiceDiscountDetail)docgraph.ARDiscountDetails.Cache.CreateInstance();
						detail.Type = discountDetail.Type;
						detail.DiscountID = discountDetail.DiscountID;
						detail.DiscountSequenceID = discountDetail.DiscountSequenceID;
						detail.ExtDiscCode = discountDetail.ExtDiscCode;
						detail.Description = discountDetail.Description;
						detail = (ARInvoiceDiscountDetail)docgraph.ARDiscountDetails.Cache.Insert(detail);
						if (discountDetail.IsManual == true && (discountDetail.Type == DiscountType.Document || discountDetail.Type == DiscountType.ExternalDocument))
						{
							detail.CuryDiscountAmt = discountDetail.CuryDiscountAmt;
							detail.IsManual = discountDetail.IsManual;
							docgraph.ARDiscountDetails.Cache.Update(detail);
						}
					}
				}
				ARInvoice old_row = PXCache<ARInvoice>.CreateCopy(docgraph.Document.Current);
				docgraph.Document.Cache.SetValueExt<ARInvoice.curyDiscTot>(docgraph.Document.Current, DiscountEngineProvider.GetEngineFor<ARTran, ARInvoiceDiscountDetail>().GetTotalGroupAndDocumentDiscount(docgraph.ARDiscountDetails));
				docgraph.Document.Cache.RaiseRowUpdated(docgraph.Document.Current, old_row);
				invoice = docgraph.Document.Update(invoice);
			}

	        if (opportunity.TaxZoneID != null && !recalcAny)
	        {
		        foreach (CRTaxTran tax in PXSelect<CRTaxTran,
					Where<CRTaxTran.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>>.Select(this))
            {
                if (opportunity.TaxZoneID == null)
                {
                    this.OpportunityCurrent.Cache.RaiseExceptionHandling<CROpportunity.taxZoneID>(
                        opportunity, null,
                        new PXSetPropertyException<CROpportunity.taxZoneID>(ErrorMessages.FieldIsEmpty,
                            typeof(CROpportunity.taxZoneID).Name));

                }

                ARTaxTran new_artax = new ARTaxTran();
                new_artax.TaxID = tax.TaxID;

                new_artax = docgraph.Taxes.Insert(new_artax);

                if (new_artax != null)
                {
                    new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
                    new_artax.TaxRate = tax.TaxRate;
                    new_artax.TaxBucketID = 0;
                    new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
                    new_artax.CuryTaxAmt = tax.CuryTaxAmt;
                    new_artax = docgraph.Taxes.Update(new_artax);
                }
            }
	        }

	        if (recalcAny)
	        {
		        docgraph.recalcdiscountsfilter.Current.OverrideManualPrices = param.OverrideManualPrices == true;
		        docgraph.recalcdiscountsfilter.Current.RecalcDiscounts = param.RecalculateDiscounts == true;
		        docgraph.recalcdiscountsfilter.Current.RecalcUnitPrices = param.RecalculatePrices == true;
		        docgraph.recalcdiscountsfilter.Current.OverrideManualDiscounts = param.OverrideManualDiscounts == true;
				docgraph.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = param.OverrideManualDocGroupDiscounts == true;

		        docgraph.Actions[nameof(Discount.RecalculateDiscountsAction)].Press();
	        }

            invoice.CuryOrigDocAmt = invoice.CuryDocBal;
            invoice.Hold = true;
            docgraph.Document.Update(invoice);

            using (PXConnectionScope cs = new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    this.Opportunity.Cache.MarkUpdated(opportunity);
                    this.Save.Press();
                    ts.Complete();
                }
            }

			docgraph.customer.Current.CreditRule = customer.CreditRule;

	        if (!this.IsContractBasedAPI)
				throw new PXRedirectRequiredException(docgraph, "");

	        docgraph.Save.Press();
        }

	    private void ConvertToCustomerAccount(BAccount account, CROpportunity opportunity)
	    {
            BusinessAccountMaint accountMaint = CreateInstance<BusinessAccountMaint>();
            accountMaint.BAccount.Insert(account);
            accountMaint.BAccount.Current = account;
	        accountMaint.BAccount.Cache.SetStatus(accountMaint.BAccount.Current, PXEntryStatus.Updated);

            accountMaint.Actions[nameof(BusinessAccountMaint.ConverToCustomer)].Press();
        }

        public PXAction<CROpportunity> addNewContact;
        [PXUIField(DisplayName = Messages.AddNewContact, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable AddNewContact(PXAdapter adapter)
        {
            if (Opportunity.Current != null && Opportunity.Current.BAccountID != null)
            {
                ContactMaint target = PXGraph.CreateInstance<ContactMaint>();
                target.Clear();

				Contact maincontact = target.Contact.Insert();

                maincontact.BAccountID = Opportunity.Current.BAccountID;

				CRContactClass ocls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>
					.SelectSingleBound(this, new object[] { maincontact });
				if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
				{
					maincontact.WorkgroupID = Opportunity.Current.WorkgroupID;
					maincontact.OwnerID = Opportunity.Current.OwnerID;
				}

				maincontact = target.Contact.Update(maincontact);

                throw new PXRedirectRequiredException(target, true, "Contact") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            return adapter.Get();
        }

		public PXAction<CROpportunity> createSalesOrder;
		[PXUIField(DisplayName = Messages.CreateSalesOrder, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable CreateSalesOrder(PXAdapter adapter)
		{
			foreach (CROpportunity opportunity in adapter.Get<CROpportunity>())
			{
				Customer customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>
					.SelectSingleBound(this, new object[] { opportunity });
				if (customer == null)
				{
					throw new PXException(Messages.ProspectNotCustomer);
				}

				if (CreateOrderParams.AskExtFullyValid((graph, viewName) => { }, DialogAnswerType.Positive))
				{
					Actions.PressSave();
					PXLongOperation.StartOperation(this, delegate()
					{
						var grapph = PXGraph.CreateInstance<OpportunityMaint>();
						grapph.Opportunity.Current = opportunity;
						grapph.CreateOrderParams.Current = CreateOrderParams.Current;
						grapph.DoCreateSalesOrder(CreateOrderParams.Current);
					});
				}
				yield return opportunity;
			}

		}

        protected virtual void DoCreateSalesOrder(CreateSalesOrderFilter param)
        {
	        bool recalcAny = param.RecalculatePrices == true ||
	                         param.RecalculateDiscounts == true ||
	                         param.OverrideManualDiscounts == true ||
							 param.OverrideManualDocGroupDiscounts == true ||
	                         param.OverrideManualPrices == true;

            var opportunity = this.Opportunity.Current;
            Customer customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>.Select(this);

            SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();

            CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>.Select(this);
            info.CuryInfoID = null;
            info = CurrencyInfo.GetEX(docgraph.currencyinfo.Insert(info.GetCM()));

            SOOrder doc = new SOOrder();
            doc.OrderType = CreateOrderParams.Current.OrderType ?? SOOrderTypeConstants.SalesOrder;
            doc = docgraph.Document.Insert(doc);
            doc = PXCache<SOOrder>.CreateCopy(docgraph.Document.Search<SOOrder.orderNbr>(doc.OrderNbr));

            doc.CuryInfoID = info.CuryInfoID;
            doc = PXCache<SOOrder>.CreateCopy(docgraph.Document.Update(doc));

            doc.CuryID = info.CuryID;
            doc.OrderDate = Accessinfo.BusinessDate;
            doc.OrderDesc = opportunity.Subject;
            doc.TermsID = customer.TermsID;
            doc.CustomerID = opportunity.BAccountID;
            doc.CustomerLocationID = opportunity.LocationID ?? customer.DefLocationID;

			doc = docgraph.Document.Update(doc);

			CRContact _CRContact = Opportunity_Contact.SelectSingle();
			CRAddress _CRAddress = Opportunity_Address.SelectSingle();
			// Insert
			if (_CRContact != null)
			{
				SOBillingContact _billingContact = docgraph.Billing_Contact.Select();

				if (_billingContact != null)
				{
					_billingContact.FullName = _CRContact.FullName;
					_billingContact.Salutation = _CRContact.Salutation;
					_billingContact.Attention = _CRContact.Attention;
					_billingContact.Phone1 = _CRContact.Phone1;
					_billingContact.Email = _CRContact.Email;

					_billingContact = docgraph.Billing_Contact.Update(_billingContact);
					_billingContact.IsDefaultContact = _CRContact.IsDefaultContact;
					_billingContact = docgraph.Billing_Contact.Update(_billingContact);
				}
			}

			if (_CRAddress != null)
			{
				SOBillingAddress _billingAddress = docgraph.Billing_Address.Select();

				if (_billingAddress != null)
				{
					_billingAddress.AddressLine1 = _CRAddress.AddressLine1;
					_billingAddress.AddressLine2 = _CRAddress.AddressLine2;
					_billingAddress.City = _CRAddress.City;
					_billingAddress.CountryID = _CRAddress.CountryID;
					_billingAddress.State = _CRAddress.State;
					_billingAddress.IsValidated = _CRAddress.IsValidated;
					_billingAddress.PostalCode = _CRAddress.PostalCode;
					_billingAddress = docgraph.Billing_Address.Update(_billingAddress);
					_billingAddress.IsDefaultAddress = _CRAddress.IsDefaultAddress;
					_billingAddress = docgraph.Billing_Address.Update(_billingAddress);
				}
			}

			CRShippingContact _crShippingContact = Shipping_Contact.SelectSingle();
			SOShippingContact _shippingContact = docgraph.Shipping_Contact.Select();
			if (_shippingContact != null && _crShippingContact != null)
			{
				_shippingContact.FullName = _crShippingContact.FullName;
				_shippingContact.Attention = _crShippingContact.Attention;
				_shippingContact.Phone1 = _crShippingContact.Phone1;
				_shippingContact.Phone1Type = _crShippingContact.Phone1Type;
				_shippingContact.Phone2 = _crShippingContact.Phone2;
				_shippingContact.Phone2Type = _crShippingContact.Phone2Type;
				_shippingContact.Email = _crShippingContact.Email;
				_shippingContact = docgraph.Shipping_Contact.Update(_shippingContact);
				_shippingContact.IsDefaultContact = _crShippingContact.IsDefaultContact;
				_shippingContact = docgraph.Shipping_Contact.Update(_shippingContact);
			}

			CRShippingAddress _crShippingAddress = Shipping_Address.SelectSingle();
			SOShippingAddress _shippingAddress = docgraph.Shipping_Address.Select();
			if (_shippingAddress != null && _crShippingAddress != null)
			{
				_shippingAddress.IsValidated = _crShippingAddress.IsValidated;
				_shippingAddress.AddressLine1 = _crShippingAddress.AddressLine1;
				_shippingAddress.AddressLine2 = _crShippingAddress.AddressLine2;
				_shippingAddress.City = _crShippingAddress.City;
				_shippingAddress.CountryID = _crShippingAddress.CountryID;
				_shippingAddress.State = _crShippingAddress.State;
				_shippingAddress.PostalCode = _crShippingAddress.PostalCode;
				_shippingAddress = docgraph.Shipping_Address.Update(_shippingAddress);
				_shippingAddress.IsDefaultAddress = _crShippingAddress.IsDefaultAddress;
				_shippingAddress = docgraph.Shipping_Address.Update(_shippingAddress);
			}

            if (opportunity.TaxZoneID != null)
            {
            doc.TaxZoneID = opportunity.TaxZoneID;
	            if (!recalcAny)
	            {
                SOTaxAttribute.SetTaxCalc<SOLine.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
		            SOTaxAttribute.SetTaxCalc<SOOrder.freightTaxCategoryID>(docgraph.Document.Cache, null,
			            TaxCalc.ManualCalc);
	            }
            }
            doc.TaxCalcMode = opportunity.TaxCalcMode;
            doc.ProjectID = opportunity.ProjectID;
            doc.BranchID = opportunity.BranchID;
            doc = docgraph.Document.Update(doc);
			docgraph.customer.Current.CreditRule = customer.CreditRule;

			var campaignRelation = docgraph.RelationsLink.Insert();
			campaignRelation.RefNoteID = doc.NoteID;
			campaignRelation.Role = CRRoleTypeList.Source;
			campaignRelation.TargetType = CRTargetEntityType.CROpportunity;
			campaignRelation.TargetNoteID = opportunity.NoteID;
			campaignRelation.DocNoteID = opportunity.NoteID;
			campaignRelation.EntityID = opportunity.BAccountID;
			campaignRelation.ContactID = opportunity.ContactID;
			docgraph.RelationsLink.Update(campaignRelation);

            bool failed = false;
			foreach (CROpportunityProducts product in PXSelectJoin<CROpportunityProducts,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>>,
					Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>>
				.Select(this))
			{
				if (product.SiteID == null)
				{
					InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<CROpportunityProducts.inventoryID>(Products.Cache, product);
					if (item != null && item.NonStockShip == true)
					{
						Products.Cache.RaiseExceptionHandling<CROpportunityProducts.siteID>(product, null,
							new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CROpportunityProducts.siteID).Name));
						failed = true;
					}
				}

				SOLine tran = new SOLine();
				tran = docgraph.Transactions.Insert(tran);
				if (tran != null)
				{
					tran.InventoryID = product.InventoryID;
					tran.SubItemID = product.SubItemID;
					tran.TranDesc = product.Descr;
					tran.OrderQty = product.Quantity;
					tran.UOM = product.UOM;
					tran.CuryUnitPrice = product.CuryUnitPrice;
					tran.CuryExtPrice = product.CuryExtPrice;
					tran.TaxCategoryID = product.TaxCategoryID;
					tran.SiteID = product.SiteID;
					tran.IsFree = product.IsFree;
					tran.ProjectID = product.ProjectID;
					tran.TaskID = product.TaskID;
					tran.CostCodeID = product.CostCodeID;
					tran.ManualPrice = true;
                    tran.ManualDisc = true;
                    tran.CuryDiscAmt = product.CuryDiscAmt;
					tran.DiscAmt = product.DiscAmt;
                    tran.DiscPct = product.DiscPct;
                    tran.POCreate = product.POCreate;
                    tran.VendorID = product.VendorID;
					tran.SortOrder = product.SortOrder;

					if (param.RecalculatePrices != true)
					{
						tran.ManualPrice = true;
					}
					else
					{
						if (param.OverrideManualPrices != true)
							tran.ManualPrice = product.ManualPrice;
						else
							tran.ManualPrice = false;
					}

					if (param.RecalculateDiscounts != true)
					{
						tran.ManualDisc = true;
					}
					else
					{
						if (param.OverrideManualDiscounts != true)
							tran.ManualDisc = product.ManualDisc;
						else
							tran.ManualDisc = false;
					}

					tran.CuryDiscAmt = product.CuryDiscAmt;
					tran.DiscAmt = product.DiscAmt;
					tran.DiscPct = product.DiscPct;
				}

				tran = docgraph.Transactions.Update(tran);
				PXNoteAttribute.CopyNoteAndFiles(Products.Cache, product, docgraph.Transactions.Cache, tran,
					Setup.Current);
			}
			
            PXNoteAttribute.CopyNoteAndFiles(Opportunity.Cache, opportunity, docgraph.Document.Cache, doc, Setup.Current);

            if (failed)
                throw new PXException(Messages.SiteNotDefined);

			//Skip all customer dicounts
			if (param.RecalculateDiscounts != true && param.OverrideManualDiscounts != true)
			{
				var discounts = new Dictionary<string, SOOrderDiscountDetail>();
				foreach (SOOrderDiscountDetail discountDetail in docgraph.DiscountDetails.Select())
				{
					docgraph.DiscountDetails.SetValueExt<SOOrderDiscountDetail.skipDiscount>(discountDetail, true);
					string key = discountDetail.Type + ':' + discountDetail.DiscountID + ':' + discountDetail.DiscountSequenceID;
					discounts.Add(key, discountDetail);
				}
				Discount ext = this.GetExtension<Discount>();
				foreach (CROpportunityDiscountDetail discountDetail in ext.DiscountDetails.Select())
				{
					SOOrderDiscountDetail detail;
					string key = discountDetail.Type + ':' + discountDetail.DiscountID + ':' + discountDetail.DiscountSequenceID;
					if (discounts.TryGetValue(key, out detail))
					{
						docgraph.DiscountDetails.SetValueExt<SOOrderDiscountDetail.skipDiscount>(detail, false);
						if (discountDetail.IsManual == true && discountDetail.Type == DiscountType.Document)
						{
							docgraph.DiscountDetails.SetValueExt<SOOrderDiscountDetail.extDiscCode>(detail, discountDetail.ExtDiscCode);
							docgraph.DiscountDetails.SetValueExt<SOOrderDiscountDetail.description>(detail, discountDetail.Description);
							docgraph.DiscountDetails.SetValueExt<SOOrderDiscountDetail.isManual>(detail, discountDetail.IsManual);
							docgraph.DiscountDetails.SetValueExt<SOOrderDiscountDetail.curyDiscountAmt>(detail, discountDetail.CuryDiscountAmt);
						}
					}
					else
					{
						detail = (SOOrderDiscountDetail)docgraph.DiscountDetails.Cache.CreateInstance();
						detail.Type = discountDetail.Type;
						detail.DiscountID = discountDetail.DiscountID;
						detail.DiscountSequenceID = discountDetail.DiscountSequenceID;
						detail.ExtDiscCode = discountDetail.ExtDiscCode;
						detail.Description = discountDetail.Description;
						detail = (SOOrderDiscountDetail)docgraph.DiscountDetails.Cache.Insert(detail);
						if (discountDetail.IsManual == true && (discountDetail.Type == DiscountType.Document || discountDetail.Type == DiscountType.ExternalDocument))
						{
							detail.CuryDiscountAmt = discountDetail.CuryDiscountAmt;
							detail.IsManual = discountDetail.IsManual;
							docgraph.DiscountDetails.Cache.Update(detail);
						}
					}
				}
				SOOrder old_row = PXCache<SOOrder>.CreateCopy(docgraph.Document.Current);
				docgraph.Document.Cache.SetValueExt<SOOrder.curyDiscTot>(docgraph.Document.Current, DiscountEngineProvider.GetEngineFor<SOLine, SOOrderDiscountDetail>().GetTotalGroupAndDocumentDiscount(docgraph.DiscountDetails));
				docgraph.Document.Cache.RaiseRowUpdated(docgraph.Document.Current, old_row);
			}

            doc = docgraph.Document.Update(doc);

			if (opportunity.TaxZoneID != null && !recalcAny)
            {
				foreach (CRTaxTran tax in PXSelect<CRTaxTran, Where<CRTaxTran.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>>.Select(this))
                {
                    SOTaxTran newtax = new SOTaxTran();
					newtax.OrderType = doc.OrderType;
					newtax.OrderNbr = doc.OrderNbr;
					newtax.TaxID = tax.TaxID;
					newtax.TaxRate = 0m;

					foreach (SOTaxTran existingTaxTran in docgraph.Taxes.Cache.Cached.RowCast<SOTaxTran>()
																.Where(a =>
																	!docgraph.Taxes.Cache.GetStatus(a).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)
																	&& docgraph.Taxes.Cache.ObjectsEqual<SOTaxTran.orderNbr, SOTaxTran.orderType, SOTaxTran.taxID>(newtax, a)
																)
					)
					{
						docgraph.Taxes.Delete(existingTaxTran);
					}

					newtax = docgraph.Taxes.Insert(newtax);
                }
            }
			
	        if (recalcAny)
	        {
		        docgraph.recalcdiscountsfilter.Current.OverrideManualPrices = param.OverrideManualPrices == true;
		        docgraph.recalcdiscountsfilter.Current.RecalcDiscounts = param.RecalculateDiscounts == true;
		        docgraph.recalcdiscountsfilter.Current.RecalcUnitPrices = param.RecalculatePrices == true;
		        docgraph.recalcdiscountsfilter.Current.OverrideManualDiscounts = param.OverrideManualDiscounts == true;
				docgraph.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = param.OverrideManualDocGroupDiscounts == true;

				docgraph.Actions[nameof(Discount.RecalculateDiscountsAction)].Press();
			}

			if (!this.IsContractBasedAPI)
				throw new PXRedirectRequiredException(docgraph, "");

	        docgraph.Save.Press();
        }
		
	    public PXAction<CROpportunity> sendQuote;
	    [PXUIField(DisplayName = "Send Quote")]
	    public IEnumerable SendQuote(PXAdapter adapter)
	    {
	        CRQuote currentQuote = Quotes.Cache.Current as CRQuote;
            if (currentQuote == null)
                return adapter.Get();

            QuoteMaint target = PXGraph.CreateInstance<QuoteMaint>();
	        target.Clear();

	        target.Opportunity.Select(currentQuote.OpportunityID);
			target.Quote.Current = target.Quote.Search<CRQuote.quoteNbr>(currentQuote.QuoteNbr, currentQuote.OpportunityID);
			target.Actions[nameof(QuoteMaint.SendQuote)].Press();
		    Quotes.Cache.Clear();

	        return adapter.Get();
        }

	    public PXAction<CROpportunity> printQuote;
	    [PXUIField(DisplayName = "Print Quote")]
	    public IEnumerable PrintQuote(PXAdapter adapter)
	    {
  	        CRQuote currentQuote = Quotes.Cache.Current as CRQuote;
	        if (currentQuote == null)
	            return adapter.Get();

			if (currentQuote.QuoteType == CRQuoteTypeAttribute.Project)
			{
				var target = PXGraph.CreateInstance<PM.PMQuoteMaint>();
				target.Clear();
				target.Opportunity.Select(currentQuote.OpportunityID);
				target.Quote.Current = target.Quote.Search<CRQuote.quoteNbr>(currentQuote.QuoteNbr);
				target.Actions[nameof(PM.PMQuoteMaint.PrintQuote)].Press();
			}
			else
			{
            QuoteMaint target = PXGraph.CreateInstance<QuoteMaint>();
	        target.Clear();
	        target.Opportunity.Select(currentQuote.OpportunityID);
			target.Quote.Current = target.Quote.Search<CRQuote.quoteNbr>(currentQuote.QuoteNbr, currentQuote.OpportunityID);
			target.Actions[nameof(QuoteMaint.PrintQuote)].Press();
			}
	        return adapter.Get();
	    }

        public PXAction<CROpportunity> submitQuote;
        [PXUIField(DisplayName = "Submit  Quote")]
        public IEnumerable SubmitQuote(PXAdapter adapter)
        {
	        UpdateQuoteStatus(false);
			return adapter.Get();
        }

        public PXAction<CROpportunity> editQuote;
	    [PXUIField(DisplayName = "Edit Quote")]
	    public IEnumerable EditQuote(PXAdapter adapter)
	    {			
		    UpdateQuoteStatus(true);
			return adapter.Get();
	    }

		private void UpdateQuoteStatus(bool hold)
		{
			this.Save.Press();
			CRQuote primaryQuote = PrimaryQuoteQuery.SelectSingle();
			if (primaryQuote != null && primaryQuote.QuoteType == CRQuoteTypeAttribute.Distribution)
			{
				QuoteMaint target = PXGraph.CreateInstance<QuoteMaint>();				
				target.Opportunity.Select(primaryQuote.OpportunityID);
				target.Quote.Current = target.Quote.Search<CRQuote.quoteNbr>(primaryQuote.QuoteNbr, primaryQuote.OpportunityID);
				QuoteMaintExt ext = target.GetExtension<QuoteMaintExt>();

				if (hold)				
					ext.EditQuote.Press();				
				else
					target.Actions[nameof(target.Approval.Submit)].Press();
				primaryQuote.Status = target.Quote.Current.Status;
				target.Save.Press();
			}
			else if (primaryQuote != null && primaryQuote.QuoteType == CRQuoteTypeAttribute.Project)
			{
				var target = PXGraph.CreateInstance<PM.PMQuoteMaint>();
				target.Opportunity.Select(primaryQuote.OpportunityID);
				target.Quote.Current = target.Quote.Search<PM.PMQuote.quoteNbr>(primaryQuote.QuoteNbr);
				var ext = target.GetExtension<PM.PMQuoteMaintExt>();

				if (hold)
					ext.EditQuote.Press();
				else
					target.Actions[nameof(target.Approval.Submit)].Press();
				primaryQuote.Status = target.Quote.Current.Status;
				target.Save.Press();
			}
			else
			{
				throw new PXException(Messages.UnsupportedQuoteType);
			}
			this.Quotes.Cache.Clear();
			this.Quotes.View.Clear();
			this.PrimaryQuoteQuery.View.Clear();
		}
		public PXAction<CROpportunity> updateClosingDate;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable UpdateClosingDate(PXAdapter adapter)
		{
			var opportunity = Opportunity.Current;
			if (opportunity != null)
			{
				opportunity.ClosingDate = Accessinfo.BusinessDate;
				Opportunity.Cache.Update(opportunity);
				Save.Press();
			}
			return adapter.Get();
		}

        public PXAction<CROpportunity> viewMainOnMap;

        [PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ViewMainOnMap()
        {
            var address = Opportunity_Address.SelectSingle();
            if (address != null)
            {
                BAccountUtility.ViewOnMap(address);
            }
        }

        public PXAction<CROpportunity> ViewShippingOnMap;

        [PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void viewShippingOnMap()
        {
            var address = Shipping_Address.SelectSingle();
            if (address != null)
            {
                BAccountUtility.ViewOnMap(address);
            }
        }

		public PXAction<CROpportunity> primaryQuote;
		[PXUIField(DisplayName = Messages.MarkAsPrimary)]
		[PXButton]
		public virtual IEnumerable PrimaryQuote(PXAdapter adapter)
		{
			foreach (CROpportunity opp in adapter.Get<CROpportunity>())
			{
				if (Quotes.Current?.IsPrimary != true)
				{
                    var selectExistingPrimary = new PXSelect<CRQuote, Where<CRQuote.quoteID, Equal<Required<CRQuote.quoteID>>>>(this);

                    CRQuote primary = selectExistingPrimary.Select(opp.DefQuoteID);
                    if (primary != null && primary.QuoteID != Quotes.Current.QuoteID && primary.Status == PM.PMQuoteStatusAttribute.Closed)
                    {
                        throw new PXException(PM.Messages.QuoteIsClosed, opp.OpportunityID, primary.QuoteNbr);
                    }


					var quoteID = Quotes.Current.QuoteID;
					var opportunityID = this.Opportunity.Current.OpportunityID;
                    this.Persist();
					PXDatabase.Update<Standalone.CROpportunity>(
						new PXDataFieldAssign<Standalone.CROpportunity.defQuoteID>(quoteID),
						new PXDataFieldRestrict<Standalone.CROpportunity.opportunityID>(PXDbType.VarChar, 255, opportunityID, PXComp.EQ)
						);
					this.Cancel.Press();
					CROpportunity rec = this.Opportunity.Search<CROpportunity.opportunityID>(opportunityID);
                    yield return rec;
                }
                yield return opp;
            }
        }

        public virtual void CreateNewQuote(CROpportunity opportunity, CreateQuotesFilter param, WebDialogResult result)
	    {
            QuoteMaint graph = CreateInstance<QuoteMaint>();
		    graph.SelectTimeStamp();
			graph.Opportunity.Current = graph.Opportunity.SelectSingle(opportunity.OpportunityID);
	        var quote = (CRQuote)graph.Quote.Cache.CreateInstance();
            quote.OpportunityID = opportunity.OpportunityID;

            quote.OpportunityAddressID = opportunity.OpportunityAddressID;
            quote.OpportunityContactID = opportunity.OpportunityContactID;
            quote.ShipAddressID = opportunity.ShipAddressID;
            quote.ShipContactID = opportunity.ShipContactID;

            quote = graph.Quote.Insert(quote);

			foreach (string field in Opportunity.Cache.Fields)
			{
				if (graph.Quote.Cache.Keys.Contains(field)
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.status))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.approved))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.rejected))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.opportunityAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.opportunityContactID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.shipAddressID))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.shipContactID)))
					continue;

				graph.Quote.Cache.SetValue(quote, field,
					Opportunity.Cache.GetValue(opportunity, field));
			}
			graph.Quote.Cache.SetDefaultExt<CRQuote.termsID>(quote);
			quote.DocumentDate = this.Accessinfo.BusinessDate;

			if (IsSingleQuote(quote.OpportunityID))
			{
				quote.QuoteID = quote.NoteID = opportunity.QuoteNoteID;
			}
			else
			{
				object quoteID;
				graph.Quote.Cache.RaiseFieldDefaulting<CRQuote.noteID>(quote, out quoteID);
				quote.QuoteID = quote.NoteID = (Guid?)quoteID;
			}

			CurrencyInfo info =
			    PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>
				    .Select(this);

		    info.CuryInfoID = null;

		    info = (CurrencyInfo)graph.Caches<CurrencyInfo>().Insert(info);
		    quote.CuryInfoID = info.CuryInfoID;
		    quote.Subject = opportunity.Subject;
		    quote.DocumentDate = this.Accessinfo.BusinessDate;
		    quote.IsPrimary = param.MakeNewQuotePrimary;

			if (param.MakeNewQuotePrimary == true)
				quote.DefQuoteID = quote.QuoteID;

			if (param.AddProductsFromOpportunity == true && !IsSingleQuote(quote.OpportunityID))
		    {
				CloneView(Products.View, graph, quote.QuoteID);
		    }
		    else
		    {			    
				graph.Quote.Cache.SetDefaultExt<CRQuote.curyDiscTot>(quote);
				graph.Quote.Cache.SetDefaultExt<CRQuote.curyLineDocDiscountTotal>(quote);
				graph.Quote.Cache.SetDefaultExt<CRQuote.curyProductsAmount>(quote);
			}



		    string note = PXNoteAttribute.GetNote(Opportunity.Cache, opportunity);
	        Guid[] files = PXNoteAttribute.GetFileNotes(Opportunity.Cache, opportunity);

	        PXNoteAttribute.SetNote(graph.Quote.Cache, quote, note);
	        PXNoteAttribute.SetFileNotes(graph.Quote.Cache, quote, files);

	        if (param.AddProductsFromOpportunity == true && !IsSingleQuote(quote.OpportunityID))
	        {
	            var DiscountExt = this.GetExtension<Discount>();
				CloneView(TaxLines.View, graph, quote.QuoteID);
				CloneView(Taxes.View, graph, quote.QuoteID, nameof(CRTaxTran.RecordID));
				CloneView(Views[nameof(DiscountExt.DiscountDetails)], graph, quote.QuoteID);
			}
	        

	        if (opportunity.AllowOverrideContactAddress == true)
	        {
				CloneView(Opportunity_Contact.View, graph, quote.QuoteID, nameof(CRContact.ContactID));
	            quote.OpportunityContactID = graph.Quote_Contact.Current.ContactID;
				CloneView(Opportunity_Address.View, graph, quote.QuoteID, nameof(CRAddress.AddressID));
	            quote.OpportunityAddressID = graph.Quote_Address.Current.AddressID;
	        }
			if (opportunity.AllowOverrideShippingContactAddress == true)
			{
				CloneView(Shipping_Contact.View, graph, quote.QuoteID, nameof(CRShippingContact.ContactID));
				quote.ShipContactID = graph.Shipping_Contact.Current.ContactID;
				CloneView(Shipping_Address.View, graph, quote.QuoteID, nameof(CRShippingAddress.AddressID));
				quote.ShipAddressID = graph.Shipping_Address.Current.AddressID;
			}

            graph.Quote.Update(quote);
		    var Discount = graph.GetExtension<QuoteMaint.Discount>();
			Discount.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = param.OverrideManualDocGroupDiscounts == true;
		    Discount.recalcdiscountsfilter.Current.OverrideManualDiscounts = param.OverrideManualDiscounts == true;
		    Discount.recalcdiscountsfilter.Current.OverrideManualPrices = param.OverrideManualPrices == true;
		    Discount.recalcdiscountsfilter.Current.RecalcDiscounts = param.RecalculateDiscounts == true;
		    Discount.recalcdiscountsfilter.Current.RecalcUnitPrices = param.RecalculatePrices == true;				
			graph.Actions[nameof(Discount.RecalculateDiscountsAction)].Press();
			Discount.RefreshTotalsAndFreeItems(Discount.Details.Cache);


			if (result == WebDialogResult.Yes)
            {
                PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
            }
			else if (result == WebDialogResult.No)
			{
				graph.Actions.PressSave();
			}
        }

		public void CreateNewProjectQuote(CROpportunity opportunity, CreateQuotesFilter param, WebDialogResult result)
		{
			var graph = CreateInstance<PM.PMQuoteMaint>();
			graph.SelectTimeStamp();
			graph.Opportunity.Current = graph.Opportunity.SelectSingle(opportunity.OpportunityID);
			var quote = (PM.PMQuote)graph.Quote.Cache.CreateInstance();
			quote.OpportunityID = opportunity.OpportunityID;
			quote.OpportunityAddressID = opportunity.OpportunityAddressID;
			quote.OpportunityContactID = opportunity.OpportunityContactID;
			quote.ShipAddressID = opportunity.ShipAddressID;
			quote.ShipContactID = opportunity.ShipContactID;

			if (IsSingleQuote(quote.OpportunityID))
			{
				quote.QuoteID = quote.NoteID = opportunity.QuoteNoteID;
			}
			else
			{
				object quoteID;
				graph.Quote.Cache.RaiseFieldDefaulting<PM.PMQuote.noteID>(quote, out quoteID);
				quote.QuoteID = quote.NoteID = (Guid?)quoteID;
			}

			quote = graph.Quote.Insert(quote);

			foreach (string field in Opportunity.Cache.Fields)
			{
				if (graph.Quote.Cache.Keys.Contains(field)
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.quoteProjectID))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.manualTotalEntry))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyAmount))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyDiscTot))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyExtPriceTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyProductsAmount))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyTaxTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyLineTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyVatExemptTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.curyVatTaxableTotal))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.status))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.approved))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.rejected))
					|| field == graph.Quote.Cache.GetField(typeof(PM.PMQuote.noteID)))
					continue;

				graph.Quote.Cache.SetValue(quote, field, Opportunity.Cache.GetValue(opportunity, field));
			}
			graph.Quote.Cache.SetDefaultExt<PM.PMQuote.termsID>(quote);
			quote.DocumentDate = this.Accessinfo.BusinessDate;

			CurrencyInfo info =
				PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>
					.Select(this);

			info.CuryInfoID = null;

			info = (CurrencyInfo)graph.Caches<CurrencyInfo>().Insert(info);
			quote.CuryInfoID = info.CuryInfoID;
			quote.Subject = opportunity.Subject;
			quote.DocumentDate = this.Accessinfo.BusinessDate;
			quote.IsPrimary = param.MakeNewQuotePrimary;

			if (param.MakeNewQuotePrimary == true)
				quote.DefQuoteID = quote.QuoteID;

			graph.Quote.Cache.SetDefaultExt<CRQuote.curyDiscTot>(quote);
			graph.Quote.Cache.SetDefaultExt<CRQuote.curyLineDocDiscountTotal>(quote);
			graph.Quote.Cache.SetDefaultExt<CRQuote.curyProductsAmount>(quote);

			string note = PXNoteAttribute.GetNote(Opportunity.Cache, opportunity);
			Guid[] files = PXNoteAttribute.GetFileNotes(Opportunity.Cache, opportunity);

			PXNoteAttribute.SetNote(graph.Quote.Cache, quote, note);
			PXNoteAttribute.SetFileNotes(graph.Quote.Cache, quote, files);

			if (opportunity.AllowOverrideContactAddress == true)
			{
				CloneView(Opportunity_Contact.View, graph, quote.QuoteID, nameof(CRContact.ContactID));
				quote.OpportunityContactID = graph.Quote_Contact.Current.ContactID;
				CloneView(Opportunity_Address.View, graph, quote.QuoteID, nameof(CRAddress.AddressID));
				quote.OpportunityAddressID = graph.Quote_Address.Current.AddressID;
			}
			if (opportunity.AllowOverrideShippingContactAddress == true)
			{
				CloneView(Shipping_Contact.View, graph, quote.QuoteID, nameof(CRShippingContact.ContactID));
				quote.ShipContactID = graph.Shipping_Contact.Current.ContactID;
				CloneView(Shipping_Address.View, graph, quote.QuoteID, nameof(CRShippingAddress.AddressID));
				quote.ShipAddressID = graph.Shipping_Address.Current.AddressID;
			}

			graph.Quote.Update(quote);

			foreach(var product in graph.Products.Select())
			{
				graph.Products.Delete(product);
			}
			foreach (var tax in graph.TaxLines.Select())
			{
				graph.TaxLines.Delete(tax);
			}
			foreach (var discount in graph._DiscountDetails.Select())
			{
				graph._DiscountDetails.Delete(discount);
			}

			if (result == WebDialogResult.Yes)
			{
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
			else if (result == WebDialogResult.No)
			{
				graph.Actions.PressSave();
			}
		}

        public PXAction<CROpportunity> copyQuote;
        [PXUIField(DisplayName = Messages.CopyQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual IEnumerable CopyQuote(PXAdapter adapter)
        {
            CRQuote currentQuote = Quotes.Cache.Current as CRQuote;
            if (currentQuote == null)
                return adapter.Get();

            foreach (CROpportunity opportunity in adapter.Get<CROpportunity>())
            {
                if (CopyQuoteInfo.View.Answer == WebDialogResult.None)
                {
                    CopyQuoteInfo.Cache.Clear();
                    CopyQuoteFilter filterdata = CopyQuoteInfo.Cache.Insert() as CopyQuoteFilter;
                    if (filterdata != null)
                    {
                        filterdata.Description = currentQuote.Subject + Messages.QuoteCopy;
                    }
                }

                if (CopyQuoteInfo.AskExt() != WebDialogResult.Yes)
                    return adapter.Get();

				switch (currentQuote.QuoteType)
				{
					case CRQuoteTypeAttribute.Distribution:
                QuoteMaint.CopyQuoteFilter quoteFilterData = new QuoteMaint.CopyQuoteFilter()
		{
                    OpportunityID = CopyQuoteInfo.Current.OpportunityID,
                    Description = CopyQuoteInfo.Current.Description,
                    RecalculatePrices = CopyQuoteInfo.Current.RecalculatePrices,
                    RecalculateDiscounts = CopyQuoteInfo.Current.RecalculateDiscounts,
                    OverrideManualPrices = CopyQuoteInfo.Current.OverrideManualPrices,
							OverrideManualDiscounts = CopyQuoteInfo.Current.OverrideManualDiscounts,
							OverrideManualDocGroupDiscounts = CopyQuoteInfo.Current.OverrideManualDocGroupDiscounts
                };
	            
				PXLongOperation.StartOperation(this, () => PXGraph.CreateInstance<QuoteMaint>().CopyToQuote(currentQuote, quoteFilterData));
						break;

					case CRQuoteTypeAttribute.Project:
						PM.PMQuoteMaint.CopyQuoteFilter pmQuoteFilterData = new PM.PMQuoteMaint.CopyQuoteFilter()
						{
							Description = CopyQuoteInfo.Current.Description,
							RecalculatePrices = CopyQuoteInfo.Current.RecalculatePrices,
							RecalculateDiscounts = CopyQuoteInfo.Current.RecalculateDiscounts,
							OverrideManualPrices = CopyQuoteInfo.Current.OverrideManualPrices,
							OverrideManualDiscounts = CopyQuoteInfo.Current.OverrideManualDiscounts,
							OverrideManualDocGroupDiscounts = CopyQuoteInfo.Current.OverrideManualDocGroupDiscounts
						};

						var pmGraph = PXGraph.CreateInstance<PM.PMQuoteMaint>();
						var pmQuote = pmGraph.Quote.Search<PM.PMQuote.quoteID>(currentQuote.QuoteID);
						PXLongOperation.StartOperation(this, () => pmGraph.CopyToQuote(pmQuote, pmQuoteFilterData));
						break;

					default:
						throw new PXException(Messages.UnsupportedQuoteType);
				}
            }

            return adapter.Get();
		}

		private void CloneView(PXView view, PXGraph graph, Guid? quoteId, string keyField = null)
	    {
	        Type cacheType = view.Cache.GetItemType();
			var cache = graph.Caches[view.Cache.GetItemType()];
			cache.Clear();
			foreach (object rec in view.SelectMulti())
            {
	            object orig = PXResult.Unwrap(rec, cacheType);
	            object item = view.Cache.CreateCopy(orig);

				view.Cache.SetValue<CROpportunityProducts.quoteID>(item, quoteId);

				if (view.Cache.Fields.Contains(nameof(INotable.NoteID)))
					view.Cache.SetValue(item, nameof(INotable.NoteID), Guid.NewGuid());

				if (!string.IsNullOrEmpty(keyField) && view.Cache.Fields.Contains(keyField))
				{
					view.Cache.SetValue(item, keyField, null);
					item = cache.Insert(item);
				}
				else
				{
					cache.SetStatus(item, PXEntryStatus.Inserted);
				}
				cache.Current = item;

	            if (PXNoteAttribute.GetNoteIDIfExists(view.Cache, orig) != null)
	            {
		            string note = PXNoteAttribute.GetNote(view.Cache, orig);
		            Guid[] files = PXNoteAttribute.GetFileNotes(view.Cache, orig);
		            PXNoteAttribute.SetNote(cache, item, note);
		            PXNoteAttribute.SetFileNotes(cache, item, files);
	            }
            }
		}

		public PXAction<CROpportunity> ViewQuote;

		[PXUIField(DisplayName = Messages.ViewQuote, Visible = false)]
		[PXButton]
		public virtual IEnumerable viewQuote(PXAdapter adapter)
		{
			if (this.Quotes.Current != null)
		{
				var quote = this.Quotes.Current;
				this.Persist();
				
				switch (quote.QuoteType)
				{
					case CRQuoteTypeAttribute.Distribution:
						var quoteMaint = PXGraph.CreateInstance<QuoteMaint>();
						quoteMaint.Quote.Current = quoteMaint.Quote.Search<CRQuote.quoteNbr>(quote.QuoteNbr, quote.OpportunityID);
						if (quoteMaint.Quote.Current != null)
							PXRedirectHelper.TryRedirect(quoteMaint, PXRedirectHelper.WindowMode.InlineWindow);
						break;

					case CRQuoteTypeAttribute.Project:
						var pmQuoteMaint = PXGraph.CreateInstance<PM.PMQuoteMaint>();
						pmQuoteMaint.Quote.Current = pmQuoteMaint.Quote.Search<PM.PMQuote.quoteNbr>(quote.QuoteNbr);
						if (pmQuoteMaint.Quote.Current != null)
							PXRedirectHelper.TryRedirect(pmQuoteMaint, PXRedirectHelper.WindowMode.InlineWindow);
						break;

					default:
						throw new PXException(Messages.UnsupportedQuoteType);
				}
			}
			return adapter.Get();
		}

		public PXAction<CROpportunity> ViewProject;
		[PXUIField(DisplayName = Messages.ViewQuote, Visible = false)]
		[PXButton]
		public virtual IEnumerable viewProject(PXAdapter adapter)
		{
            int? projectID = this.Quotes.Current?.QuoteProjectID;

            if (projectID != null)
			{
				this.Persist();
				var service = PXGraph.CreateInstance<PM.ProjectAccountingService>();
				service.NavigateToProjectScreen(projectID, PXRedirectHelper.WindowMode.InlineWindow);
			}
			return adapter.Get();
		}

		public PXAction<CROpportunity> ViewOrder;
		[PXUIField(DisplayName = "View Order", Visible = false)]
		[PXButton]
		public virtual IEnumerable viewOrder(PXAdapter adapter)
		{
			if (SalesOrders.Current != null)
			{
				var graph = PXGraph.CreateInstance<SOOrderEntry>();
				PXRedirectHelper.TryRedirect(graph, SalesOrders.Current, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<CROpportunity> ViewInvoice;
		[PXUIField(DisplayName = "View Invoice", Visible = false)]
		[PXButton]
		public virtual IEnumerable viewInvoice(PXAdapter adapter)
		{
			if (Invoices.Current != null)
			{
				var graph = PXGraph.CreateInstance<ARInvoiceEntry>();
				PXRedirectHelper.TryRedirect(graph, Invoices.Current, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<CROpportunity> validateAddresses;
        [PXUIField(DisplayName = CR.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select/*, FieldClass = CS.Messages.ValidateAddress*/)]
        [PXButton()]
        public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
        {
            foreach (CROpportunity current in adapter.Get<CROpportunity>())
            {
                if (current != null)
                {
                    CRAddress address = this.Opportunity_Address.Select();
					if (address != null && current.AllowOverrideContactAddress == true && address.IsValidated == false)
                        {
						PXAddressValidator.Validate<CRAddress>(this, address, true);
                    }

                    CRShippingAddress shipAddress = this.Shipping_Address.Select();
					if (shipAddress != null && current.BAccountID == null && current.ContactID == null && current.AllowOverrideShippingContactAddress != true)
                    {
						shipAddress.IsValidated = address.IsValidated;
						Shipping_Address.Cache.MarkUpdated(shipAddress);
                    }

					if (shipAddress != null && current.AllowOverrideShippingContactAddress == true && shipAddress.IsValidated == false)
                    {
						PXAddressValidator.Validate<CRShippingAddress>(this, shipAddress, true);
                    }
                }
                yield return current;
            }
        }

		#endregion

		#region Event Handlers

        #region CROpportunity

		[CRMBAccount(BqlField = typeof(Standalone.CROpportunityRevision.bAccountID))]
		[CustomerAndProspectRestrictor]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CROpportunity.bAccountID> e) { }

		protected virtual void CROpportunity_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null) return;

			var customerLocation = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<CROpportunity.bAccountID>>,
					And<Location.locationID, Equal<Required<CROpportunity.locationID>>>>>.
				Select(this, row.BAccountID, row.LocationID);
            if (customerLocation != null)
            {
                if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
                {
                    e.NewValue = customerLocation.CTaxZoneID;
                }
                else
                {
                    var address = (Address)PXSelect<Address,
                        Where<Address.addressID, Equal<Required<Address.addressID>>>>.
                        Select(this, customerLocation.DefAddressID);
                    if (address != null && !string.IsNullOrEmpty(address.PostalCode))
                    {
                        e.NewValue = TaxBuilderEngine.GetTaxZoneByZip(this, address.PostalCode);
                    }
                }
            }
            if (e.NewValue == null)
            {
                var branchLocation = (Location)PXSelectJoin<Location,
                    InnerJoin<Branch, On<Branch.branchID, Equal<Current<CROpportunity.branchID>>>,
                    InnerJoin<BAccount, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>,
                        Where<Location.locationID, Equal<BAccount.defLocationID>>>.Select(this);
                if (branchLocation != null && branchLocation.VTaxZoneID != null)
                    e.NewValue = branchLocation.VTaxZoneID;
                else
                    e.NewValue = row.TaxZoneID;
            }
		}

		protected virtual void CROpportunity_ClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Setup.Current.DefaultOpportunityClassID;
		}

		protected virtual void CROpportunity_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null || row.BAccountID == null) return;

			var baccount = (BAccount)PXSelect<BAccount, 
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, row.BAccountID);

			if (baccount != null)
			{
				e.NewValue = baccount.DefLocationID;
				e.Cancel = true;
			}
		}

		protected virtual void CROpportunity_DefQuoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = (CROpportunity)e.Row;
			if (row == null) return;
			e.NewValue = row.QuoteNoteID;
			e.Cancel = true;
		}

        protected virtual void CROpportunity_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CROpportunity row = (CROpportunity)e.Row;
			if (row != null && row.CampaignSourceID != null)
			{
				CRCampaign campaign =
					(CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);

				if (campaign != null && campaign.ProjectID != null)
				{
					e.NewValue = campaign.ProjectID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void CROpportunity_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			UpdateProductsTasks();
		}
	    protected virtual void CROpportunity_CampaignSourceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
	        CROpportunity row = (CROpportunity)e.Row;
	        if (row != null && row.ProjectID != null)
	        {
	            PXResultset<CRCampaign> result = 
                    PXSelect<CRCampaign, 
                    Where<CRCampaign.projectID, Equal<Required<CROpportunity.projectID>>>>
	                .SelectWindowed(this,0,2, row.ProjectID);

	            if (result.Count == 1)
	            {
	                CRCampaign rec = result[0];
	                e.NewValue = rec.CampaignID;
	            }
	        }
	    }
	    protected virtual void CROpportunity_CampaignSourceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
			UpdateProductsTasks();
		}
		protected virtual void UpdateProductsTasks()
        {
			if (this.Opportunity.Current?.CampaignSourceID != null)
			{
				CRCampaign campaign = (CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);
				if (campaign != null && campaign.ProjectID == this.Opportunity.Current.ProjectID)
				{
					foreach (var product in Products.Select())
					{
						Products.Cache.SetDefaultExt<CROpportunityProducts.projectID>(product);
						Products.Cache.SetDefaultExt<CROpportunityProducts.taskID>(product);
					}
				}
			}
		}

		protected virtual void UpdateProductsCostCodes(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = this.Opportunity.Current;
			if (row == null) return;

			foreach (CROpportunityProducts prod in Products.Select())
			{
				bool isUpdated = prod.ProjectID != row.ProjectID;

				try
				{
					PM.PMProject project;
					if (PXAccess.FeatureInstalled<FeaturesSet.costCodes>() && PM.ProjectDefaultAttribute.IsProject(this, row.ProjectID, out project))
					{
						if (project.BudgetLevel == PM.BudgetLevels.Task)
						{
							int CostCodeID = PM.CostCodeAttribute.GetDefaultCostCode();
							isUpdated = isUpdated || prod.CostCodeID != CostCodeID;
							prod.CostCodeID = CostCodeID;
						}
					}

					prod.ProjectID = row.ProjectID;

					if (isUpdated)
					{
						Products.Update(prod);
					}
				}
				catch (PXException ex)
				{
					PXFieldState projectIDState = (PXFieldState)sender.GetStateExt<CROpportunity.projectID>(row);
					Products.Cache.RaiseExceptionHandling<CROpportunityProducts.projectID>(prod, projectIDState.Value, ex);
				}
			}
		}

        protected virtual void CROpportunity_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            CROpportunity row = e.Row as CROpportunity;
            if (row == null) return;

            BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>.SelectSingleBound(this, null);
            if (bAccount != null)
			{
                Opportunity.Cache.SetValueExt<CROpportunity.taxZoneID>(row, bAccount.TaxZoneID);
			}

            Opportunity.Cache.SetDefaultExt<CROpportunity.locationID>(row);
            Opportunity.Cache.SetDefaultExt<CROpportunity.taxCalcMode>(row);

            foreach (var opportunityRevision in PXSelect<CROpportunityRevision, Where<CROpportunityRevision.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
                                                            .Select(this).RowCast<CROpportunityRevision>())
            {
                OpportunityRevision.Cache.SetValueExt<CROpportunityRevision.bAccountID>(opportunityRevision, row.BAccountID);
                if (bAccount != null)
                {
                    OpportunityRevision.Cache.SetValueExt<CROpportunityRevision.taxZoneID>(opportunityRevision, bAccount.TaxZoneID);
                }
                OpportunityRevision.Cache.SetDefaultExt<CROpportunityRevision.locationID>(opportunityRevision);
            }

			var allowOverrideContactAddress = (row.AllowOverrideContactAddress == true) || (row.BAccountID == null && row.ContactID == null);
			Opportunity.Cache.SetValueExt<CROpportunity.allowOverrideContactAddress>(row, allowOverrideContactAddress);
		}

        protected virtual void CROpportunity_ContactID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

            var opportunityRevisionsquery = new PXSelectJoin<CROpportunityRevision, InnerJoin<CRQuote, On<CRQuote.quoteID, Equal<CROpportunityRevision.noteID>>>,
                Where<CROpportunityRevision.opportunityID, Equal<Current<CROpportunity.opportunityID>>, And<CRQuote.quoteID, Equal<CRQuote.defQuoteID>>>>(this);

            CROpportunityRevision opportunityRevision = opportunityRevisionsquery.SelectSingle();
            if (opportunityRevision != null)
			{
                OpportunityRevision.Cache.SetValueExt<CROpportunityRevision.contactID>(opportunityRevision, row.ContactID);
			}

			if (Opportunity.Cache.GetStatus(row) == PXEntryStatus.Updated)
			{
				var allowOverrideContactAddress = (row.AllowOverrideContactAddress == true) || (row.BAccountID == null && row.ContactID == null);
				Opportunity.Cache.SetValueExt<CROpportunity.allowOverrideContactAddress>(row, allowOverrideContactAddress);
			}
		}

		protected virtual void CROpportunity_CloseDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

			if(PrimaryQuoteQuery.SelectSingle() == null)
			{
                Opportunity.Cache.SetValueExt<CROpportunity.documentDate>(row, row.CloseDate);
			}
		}
			
        protected virtual void CROpportunity_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXNoteAttribute.SetTextFilesActivitiesRequired<CROpportunityProducts.noteID>(Products.Cache, null);

			CROpportunity row = e.Row as CROpportunity;
            if (row == null) return;

			CRQuote primaryQt = PrimaryQuoteQuery.SelectSingle();

			if (row.PrimaryQuoteType == null)
				row.PrimaryQuoteType = primaryQt?.QuoteType ?? CRQuoteTypeAttribute.Distribution;

			if (primaryQt?.IsDisabled == true)
			{
				sender.RaiseExceptionHandling<CROpportunity.opportunityID>(row, row.OpportunityID,
					new PXSetPropertyException(Messages.QuoteSubmittedReadonly, PXErrorLevel.Warning));
			}

			foreach (var type in new[]
			{
				typeof(CROpportunityDiscountDetail),
				typeof(CROpportunityProducts),
				typeof(CRTaxTran),
				typeof(CSAnswers)
			})
			{
				Caches[type].AllowInsert = Caches[type].AllowUpdate = Caches[type].AllowDelete = primaryQt?.IsDisabled != true;
			}
			if (row.PrimaryQuoteType == CRQuoteTypeAttribute.Project)
				row.ManualTotalEntry = false;

			PXUIFieldAttribute.SetEnabled<CROpportunity.manualTotalEntry>(sender, row, row.PrimaryQuoteType != CRQuoteTypeAttribute.Project);

			PXUIFieldAttribute.SetEnabled<CROpportunity.classID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyID>(sender, row, primaryQt?.IsDisabled != true && row.PrimaryQuoteType != CRQuoteTypeAttribute.Project);
			PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.locationID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyAmount>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyDiscTot>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.branchID>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.taxZoneID>(sender, row, primaryQt?.IsDisabled != true);
			
			PXUIFieldAttribute.SetEnabled<CROpportunity.taxCalcMode>(sender, row, primaryQt?.IsDisabled != true);
			PXUIFieldAttribute.SetVisible<CROpportunity.taxCalcMode>(sender, row, row.PrimaryQuoteType != CRQuoteTypeAttribute.Project);

			if (row.ManualTotalEntry == true && row.CuryTaxTotal > 0)
			{
				sender.RaiseExceptionHandling<CROpportunity.curyTaxTotal>(row, row.CuryTaxTotal,
					new PXSetPropertyException(Messages.TaxAmountExcluded, PXErrorLevel.Warning));
			}
			else
			{
				sender.RaiseExceptionHandling<CROpportunity.curyTaxTotal>(row, row.CuryTaxTotal, null);
			}

			Caches[typeof(CRContact)].AllowUpdate = row.AllowOverrideContactAddress == true;
			PXUIFieldAttribute.SetEnabled(Caches[typeof(CRContact)], null, Caches[typeof(CRContact)].AllowUpdate);

			Caches[typeof(CRAddress)].AllowUpdate = row.AllowOverrideContactAddress == true;
			PXUIFieldAttribute.SetEnabled(Caches[typeof(CRAddress)], null, Caches[typeof(CRAddress)].AllowUpdate);

			CROpportunityClass source = PXSelectorAttribute.Select<CROpportunity.classID>(sender, e.Row) as CROpportunityClass;
			if (source != null)
			{
				Activities.DefaultEMailAccountId = source.DefaultEMailAccountID;
		}

			bool hasProducts = Products.SelectSingle(row.OpportunityID) != null;

			
			var products = Products.View.SelectMultiBound(new object[] { row }).RowCast<CROpportunityProducts>();
			bool allProductsHasNoInventoryID = products.Any(_ => _.InventoryID == null) && !products.Any(_ => _.InventoryID != null);

				if (row.BAccountID != null && hasProducts)
			{
				PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(sender, row, false);
			}
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyAmount>(sender, e.Row, row.ManualTotalEntry == true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.curyDiscTot>(sender, e.Row, row.ManualTotalEntry == true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.opportunityID>(sender, e.Row, true);
			PXUIFieldAttribute.SetEnabled<CROpportunity.allowOverrideContactAddress>(sender, row, !(row.BAccountID == null && row.ContactID == null));

			PXUIFieldAttribute.SetEnabled<CROpportunity.projectID>(sender, e.Row, row.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution);

			PXUIFieldAttribute.SetVisible<CROpportunity.curyID>(sender, row, IsMultyCurrency);

            decimal? curyWgtAmount = null;
			var oppProbability = row.StageID.
				With(_ => (CROpportunityProbability)PXSelect<CROpportunityProbability,
					Where<CROpportunityProbability.stageCode, Equal<Required<CROpportunityProbability.stageCode>>>>.
				Select(this, _));
			if (oppProbability != null && oppProbability.Probability != null)
                curyWgtAmount = oppProbability.Probability * (this.Accessinfo.CuryViewState ? row.ProductsAmount : row.CuryProductsAmount) / 100;
			row.CuryWgtAmount = curyWgtAmount;

            if (row.ContactID != null && row.BAccountID != null)
			{
                Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.SelectSingleBound(this, null, row.ContactID);
				if (contact != null && contact.BAccountID != row.BAccountID)
				{
					Opportunity.Cache.RaiseExceptionHandling<CROpportunity.contactID>(row, row.ContactID, new PXSetPropertyException(Messages.ContractBAccountDiffer, PXErrorLevel.Warning));
				}
				else
				{
					Opportunity.Cache.RaiseExceptionHandling<CROpportunity.contactID>(row, null, null);
				}
			}



			bool hasQuotes = primaryQt != null;

			createQuote
				.SetEnabled(Opportunity.Current.IsActive == true);

			primaryQuote
				.SetEnabled(hasQuotes && Opportunity.Current.IsActive == true);

			copyQuote
				.SetEnabled(hasQuotes && Opportunity.Current.IsActive == true);

			createInvoice
				.SetEnabled(hasProducts && !allProductsHasNoInventoryID
				&& (!hasQuotes || primaryQt.Status == CRQuoteStatusAttribute.Sent || primaryQt.Status == CRQuoteStatusAttribute.Approved)
				&& (!hasQuotes || primaryQt.QuoteType == CRQuoteTypeAttribute.Distribution));

			createSalesOrder
				.SetEnabled(hasProducts && !allProductsHasNoInventoryID
				&& (!hasQuotes || primaryQt.QuoteType == CRQuoteTypeAttribute.Distribution));

		    editQuote
				.SetEnabled(primaryQt != null && primaryQt.Status != CRQuoteStatusAttribute.Draft);

            submitQuote
				.SetEnabled(primaryQt != null && primaryQt.Status == CRQuoteStatusAttribute.Draft && Opportunity.Current.IsActive == true);

			sendQuote
				.SetEnabled(primaryQt != null && (
					primaryQt.QuoteType == CRQuoteTypeAttribute.Distribution && primaryQt.Status.IsIn(CRQuoteStatusAttribute.Approved, CRQuoteStatusAttribute.Sent)
					||
					primaryQt.QuoteType == CRQuoteTypeAttribute.Project && primaryQt.Status.IsIn(PM.PMQuoteStatusAttribute.Approved, PM.PMQuoteStatusAttribute.Sent, PM.PMQuoteStatusAttribute.Closed)
					));

			printQuote
				.SetEnabled(primaryQt != null && (
				primaryQt.QuoteType == CRQuoteTypeAttribute.Project ||
				primaryQt.Status == CRQuoteStatusAttribute.Approved ||
				primaryQt.Status == CRQuoteStatusAttribute.Sent));

            if (!UnattendedMode)
            {
                CRShippingAddress shipAddress = this.Shipping_Address.Select();
                CRAddress contactAddress = this.Opportunity_Address.Select();
                bool enableAddressValidation = (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
                                                || (contactAddress != null && (contactAddress.IsDefaultAddress == false || row.BAccountID == null && row.ContactID == null) && contactAddress.IsValidated == false);
                validateAddresses
					.SetEnabled(enableAddressValidation);
            }

            PXUIFieldAttribute.SetVisible<CROpportunityProducts.curyUnitCost>(this.Products.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>() && PXAccess.FeatureInstalled<FeaturesSet.inventory>());
            PXUIFieldAttribute.SetVisible<CROpportunityProducts.vendorID>(this.Products.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.inventory>());
            PXUIFieldAttribute.SetVisible<CROpportunityProducts.pOCreate>(this.Products.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.inventory>());
        }

		protected virtual void CROpportunityRevision_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
			//Suppress update revision for opportunity, should be done by main DAC
			CROpportunityRevision row = (CROpportunityRevision)e.Row;
			if (row != null && this.Opportunity.Current != null &&
			    row.OpportunityID == this.Opportunity.Current.OpportunityID &&
			    row.NoteID == this.Opportunity.Current.DefQuoteID)
				e.Cancel = true;
		}

		protected virtual void CROpportunity_BAccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

			if (row.BAccountID < 0)
				e.ReturnValue = "";
		}
		protected virtual void CROpportunity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null) return;

			object revisionNoteID;
			this.Caches[typeof(CROpportunity)].RaiseFieldDefaulting<CROpportunity.quoteNoteID>(row, out revisionNoteID);
			if (revisionNoteID != null)
				row.DefQuoteID = (Guid?)revisionNoteID;

			object newContactId = row.ContactID;
			if (newContactId != null && !VerifyField<CROpportunity.contactID>(row, newContactId))
				row.ContactID = null;

			if (row.ContactID != null)
			{
				object newCustomerId = row.BAccountID;
				if (newCustomerId == null)
					FillDefaultBAccountID(cache, row);
			}

			object newLocationId = row.LocationID;
			if (newLocationId == null || !VerifyField<CROpportunity.locationID>(row, newLocationId))
			{
				cache.SetDefaultExt<CROpportunity.locationID>(row);
			}

			if (row.ContactID == null)
				cache.SetDefaultExt<CROpportunity.contactID>(row);

			if (row.TaxZoneID == null) 
				cache.SetDefaultExt<CROpportunity.taxZoneID>(row);
		}


		protected virtual void CROpportunity_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
            var oldRow = e.OldRow as CROpportunity;
            var row = e.Row as CROpportunity;
            if (oldRow == null || row == null) return;

            if (row.ContactID != null && row.ContactID != oldRow.ContactID)
            {
                object newCustomerId = row.BAccountID;
                if (newCustomerId == null)
                    FillDefaultBAccountID(sender, row);
            }

            var customerChanged = row.BAccountID != oldRow.BAccountID;
			object newLocationId = row.LocationID;
			bool locationChanged = !sender.ObjectsEqual<CROpportunity.locationID>(e.Row, e.OldRow);
			if (locationChanged && (newLocationId == null || !VerifyField<CROpportunity.locationID>(row, newLocationId)))
			{
				sender.SetDefaultExt<CROpportunity.locationID>(row);
			}		

			if (customerChanged)
				sender.SetDefaultExt<CROpportunity.taxZoneID>(row);

		    bool campaignChanged = !sender.ObjectsEqual<CROpportunity.campaignSourceID>(e.Row, e.OldRow);
		    bool projectChanged = !sender.ObjectsEqual<CROpportunity.projectID>(e.Row, e.OldRow);

		    if (campaignChanged)
		    {
		        CRCampaign campaign =
		            (CRCampaign) PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(
                        this.Opportunity.Cache,
		                this.Opportunity.Current);

		        if (!projectChanged || campaign.ProjectID != row.ProjectID)
		            sender.SetDefaultExt<CROpportunity.projectID>(row);
		        projectChanged = sender.ObjectsEqual<CROpportunity.projectID>(e.Row, e.OldRow);
            }
		    else if(projectChanged)
		    {
		        CRCampaign campaign =
		            (CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);
		        if (campaign == null || campaign.ProjectID != row.ProjectID)
		        {
		            sender.SetDefaultExt<CROpportunity.campaignSourceID>(row);
                }		        
		    }
		    
			var closeDateChanged = row.CloseDate != oldRow.CloseDate;
			
			if (locationChanged || closeDateChanged || projectChanged || customerChanged)
			{
				var productsCache = Products.Cache;
				foreach (CROpportunityProducts line in SelectProducts(row.QuoteNoteID))
				{
					var lineCopy = (CROpportunityProducts)productsCache.CreateCopy(line);
					lineCopy.ProjectID = row.ProjectID;
					lineCopy.CustomerID = row.BAccountID;
					productsCache.Update(lineCopy);
				}

				sender.SetDefaultExt<CROpportunity.taxCalcMode>(row);
			}

			if (row.OwnerID == null)
			{
				row.AssignDate = null;
			}
			else if (oldRow.OwnerID == null)
			{
				row.AssignDate = PXTimeZoneInfo.Now;
			}

		    if (IsExternalTax(Opportunity.Current.TaxZoneID) && (!sender.ObjectsEqual<CROpportunity.contactID, CROpportunity.taxZoneID, CROpportunity.branchID, CROpportunity.locationID,
                                                       CROpportunity.curyAmount, CROpportunity.shipAddressID>(e.Row, e.OldRow) || 
               (PrimaryQuoteQuery.SelectSingle() == null && !sender.ObjectsEqual<CROpportunity.closeDate>(e.Row, e.OldRow))))
		    {
                row.IsTaxValid = false;
		    }
        }

		protected virtual void CROpportunity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (CROpportunity)e.Row;
			if (row == null) return;

			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.BAccountID != null)
			{
				PXDefaultAttribute.SetPersistingCheck<CROpportunity.locationID>(sender, e.Row, PXPersistingCheck.NullOrBlank);				
			}
		}

		protected virtual void CROpportunity_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

			if (e.Operation == PXDBOperation.Delete && e.TranStatus == PXTranStatus.Open)
			{
				var quoteIDs = new List<Guid?>();
				quoteIDs.Add(row.QuoteNoteID);
				foreach (CRQuote quote in this.Quotes.View.SelectMultiBound(new object[] {row}))
				{
					PXDatabase.Delete<EP.EPApproval>(new PXDataFieldRestrict<EP.EPApproval.refNoteID>(quote.NoteID));
					quoteIDs.Add(quote.QuoteID);
				}
				foreach (var quoteId in quoteIDs)
				{
					PXDatabase.Delete<Standalone.CROpportunityRevision>(new PXDataFieldRestrict<Standalone.CROpportunityRevision.noteID>(quoteId));
					PXDatabase.Delete<Standalone.CRQuote>(new PXDataFieldRestrict<Standalone.CRQuote.quoteID>(quoteId));
				}
			}
		}
		#endregion

		#region CROpportunityProducts

		protected virtual void CROpportunityProducts_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CROpportunityProducts;
			if (row == null) return;

			bool autoFreeItem = row.ManualDisc != true && row.IsFree == true;

			if (autoFreeItem)
			{
				PXUIFieldAttribute.SetEnabled<CROpportunityProducts.taxCategoryID>(sender, e.Row);
				PXUIFieldAttribute.SetEnabled<CROpportunityProducts.descr>(sender, e.Row);
			}

            PXUIFieldAttribute.SetEnabled<CROpportunityProducts.vendorID>(sender, row, row.POCreate == true);
        }
		protected virtual void CROpportunityProducts_TaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Opportunity.Current.CampaignSourceID != null)
			{
				CRCampaign campaign =
					(CRCampaign)PXSelectorAttribute.Select<CROpportunity.campaignSourceID>(this.Opportunity.Cache, this.Opportunity.Current);
				if (campaign != null && campaign.ProjectID == this.Opportunity.Current.ProjectID)
				{
					e.NewValue = campaign.ProjectTaskID;
					e.Cancel = true;
				}
			}
		}		
        protected virtual void CROpportunityProducts_VendorID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
                return;

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;

            if (crOpportunityProductsRow.POCreate == false || crOpportunityProductsRow.InventoryID == null)
            {
                e.Cancel = true;
            }
        }
        protected virtual void CROpportunityProducts_POCreate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
                return;

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;

            sender.SetDefaultExt<CROpportunityProducts.vendorID>(crOpportunityProductsRow);
        }
		protected virtual void CROpportunityProducts_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
		    if (IsExternalTax(Opportunity.Current.TaxZoneID))
		    {
		        Opportunity.Current.IsTaxValid = false;
		        Opportunity.Update(Opportunity.Current);
            }
		}
		protected virtual void CROpportunityProducts_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
		    if (IsExternalTax(Opportunity.Current.TaxZoneID))
		    {
		        Opportunity.Current.IsTaxValid = false;
		        Opportunity.Update(Opportunity.Current);
            }
        }
	    protected virtual void CROpportunityProducts_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        if (!IsExternalTax(Opportunity.Current.TaxZoneID)) return;
	        
	        Opportunity.Cache.SetValue(Opportunity.Current, typeof(CROpportunity.isTaxValid).Name, false);
	    }
        protected virtual void CROpportunityProducts_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CROpportunityProducts row = e.Row as CROpportunityProducts;
            if (row == null) return;

            if (row.InventoryID != null && row.IsFree == false)
            {
                Caches[typeof(CROpportunityProducts)].SetDefaultExt<CROpportunityProducts.curyUnitPrice>(row);
            }
        }

        [PopupMessage]
        [PXRestrictor(typeof(Where<
            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>>>), IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<CROpportunityProducts.inventoryID> e) { }

        protected virtual void CROpportunityProducts_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
                return;

            CROpportunityProducts crOpportunityProductsRow = (CROpportunityProducts)e.Row;

            sender.SetValueExt<CROpportunityProducts.pOCreate>(crOpportunityProductsRow, false);
        }

        [PXUIField(DisplayName = "Manual Price", Visible = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void CROpportunityProducts_ManualPrice_CacheAttached(PXCache e)
        {
        }

		protected virtual void _(Events.FieldDefaulting<ARTran, CROpportunityProducts.costCodeID> e)
		{
			PM.PMProject project;
			if (PM.CostCodeAttribute.UseCostCode() && PM.ProjectDefaultAttribute.IsProject(this, e.Row.ProjectID, out project))
			{
				if (project.BudgetLevel == PM.BudgetLevels.Task)
				{
					e.NewValue = PM.CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}
		#endregion

		#region CRRelation
		[PXDBGuid]
		[PXDBDefault(typeof(CROpportunity.noteID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void CRRelation_RefNoteID_CacheAttached(PXCache sender)
		{
		}
		#endregion

        #region CreateQuoteFilter

	    protected virtual void CreateQuotesFilter_AddProductsFromOpportunity_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
	    {
	        var row = e.Row as CreateQuotesFilter;
	        if (row == null) return;

            e.NewValue = Products.SelectSingle() != null && row.QuoteType == CRQuoteTypeAttribute.Distribution && Opportunity.Current.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution;
            e.Cancel = true;
	    }

		protected virtual void CreateQuotesFilter_QuoteType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CreateQuotesFilter;
			if (row == null) return;
			e.NewValue = (SalesQuotesInstalled) ? CRQuoteTypeAttribute.Distribution : CRQuoteTypeAttribute.Project;
            e.Cancel = true;
	    }

        protected virtual void CreateQuotesFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        CreateQuotesFilter row = e.Row as CreateQuotesFilter;
	        if (row == null) return;

            if (!row.RecalculatePrices.GetValueOrDefault())
	        {
	            QuoteInfo.Cache.SetValue<CreateQuotesFilter.overrideManualPrices>(row, false);
	        }
	        if (!row.RecalculateDiscounts.GetValueOrDefault())
	        {
	            QuoteInfo.Cache.SetValue<CreateQuotesFilter.overrideManualDiscounts>(row, false);
                QuoteInfo.Cache.SetValue<CreateQuotesFilter.overrideManualDocGroupDiscounts>(row, false);
	        }
	        if (row.QuoteType == CRQuoteTypeAttribute.Project)
	        {
	            QuoteInfo.Cache.SetValue<CreateQuotesFilter.addProductsFromOpportunity>(row, false);
	        }
	    }

        protected virtual void CreateQuotesFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	    {
	        CreateQuotesFilter row = e.Row as CreateQuotesFilter;
	        if (row == null) return;

	        bool hasProducts = Products.SelectSingle() != null;
	        bool hasQuotes = Quotes.SelectSingle() != null;
			var isDistributionQuote = row.QuoteType == CRQuoteTypeAttribute.Distribution;
			var isProjectQuote = row.QuoteType == CRQuoteTypeAttribute.Project;
			var currentQuoteIsDistribution = Opportunity.Current.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution;

			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.quoteType>(sender, row, ProjectQuotesInstalled && SalesQuotesInstalled);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.addProductsFromOpportunity>(sender, row, hasProducts && hasQuotes && isDistributionQuote && currentQuoteIsDistribution);
	        PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.makeNewQuotePrimary>(sender, row, hasQuotes);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.recalculatePrices>(sender, row, isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true && isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.recalculateDiscounts>(sender, row, isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true && isDistributionQuote);
			PXUIFieldAttribute.SetEnabled<CreateQuotesFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);

			if (row.QuoteType == CRQuoteTypeAttribute.Project)
			{
				if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && OpportunityCurrent.Current.CuryID != (Accessinfo.BaseCuryID ?? new PXSetup<PX.Objects.GL.Company>(this).Current?.BaseCuryID))
				sender.RaiseExceptionHandling<CreateQuotesFilter.quoteType>(row, row.QuoteType,
					new PXSetPropertyException(Messages.CannotCreateProjectQuoteBecauseOfCury, PXErrorLevel.Error));

				if (PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() && OpportunityCurrent.Current.TaxCalcMode != TaxCalculationMode.TaxSetting)
					sender.RaiseExceptionHandling<CreateQuotesFilter.quoteType>(row, row.QuoteType,
						new PXSetPropertyException(Messages.CannotCreateProjectQuoteBecauseOfTaxCalcMode, PXErrorLevel.Error));

				if (Opportunity.Current.ManualTotalEntry == true)
					sender.RaiseExceptionHandling<CreateQuotesFilter.quoteType>(row, row.QuoteType,
						new PXSetPropertyException(Messages.ManualAmountWillBeCleared, PXErrorLevel.Warning));
			}
			else    
			{
				sender.RaiseExceptionHandling<CreateQuotesFilter.quoteType>(row, row.QuoteType, null);
			}
        }
        #endregion

        #region CreateInvoicesFilter

	    protected virtual void CreateInvoicesFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        CreateInvoicesFilter row = e.Row as CreateInvoicesFilter;
            if (row == null) return;

	        if (!row.RecalculatePrices.GetValueOrDefault())
	        {
	            InvoiceInfo.Cache.SetValue<CreateInvoicesFilter.overrideManualPrices>(row, false);
	        }
	        if (!row.RecalculateDiscounts.GetValueOrDefault())
	        {
	            InvoiceInfo.Cache.SetValue<CreateInvoicesFilter.overrideManualDiscounts>(row, false);
				InvoiceInfo.Cache.SetValue<CreateInvoicesFilter.overrideManualDocGroupDiscounts>(row, false);
	        }
	    }

        protected virtual void CreateInvoicesFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	    {
	        CreateInvoicesFilter row = e.Row as CreateInvoicesFilter;
			CROpportunity opportunity = Opportunity.Current;
	        if (row == null || opportunity == null) return;

	        PXUIFieldAttribute.SetEnabled<CreateInvoicesFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true);
	        PXUIFieldAttribute.SetEnabled<CreateInvoicesFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetEnabled<CreateInvoicesFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetVisible<CreateInvoicesFilter.confirmManualAmount>(sender, row, opportunity.ManualTotalEntry == true);
	    }

		protected virtual void CreateInvoicesFilter_ConfirmManualAmount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CreateInvoicesFilter row = e.Row as CreateInvoicesFilter;
			CROpportunity opportunity = Opportunity.Current;
			if (row == null || opportunity == null) return;

			if (opportunity.ManualTotalEntry == true && e.NewValue as bool? != true)
			{
				sender.RaiseExceptionHandling<CreateInvoicesFilter.confirmManualAmount>(row, e.NewValue,
					new PXSetPropertyException(Messages.ConfirmInvoiceCreation, PXErrorLevel.Error));

				throw new PXSetPropertyException(Messages.ConfirmInvoiceCreation, PXErrorLevel.Error);
			}
		}

		protected virtual void CreateInvoicesFilter_ConfirmManualAmount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CreateInvoicesFilter row = e.Row as CreateInvoicesFilter;
			if (row == null) return;

			sender.RaiseExceptionHandling<CreateInvoicesFilter.confirmManualAmount>(row, null, null);
		}

        #endregion

	    #region CreateSalesOrderFilter

	    protected virtual void CreateSalesOrderFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        CreateSalesOrderFilter row = e.Row as CreateSalesOrderFilter;
	        if (row == null) return;

	        if (!row.RecalculatePrices.GetValueOrDefault())
	        {
	            InvoiceInfo.Cache.SetValue<CreateSalesOrderFilter.overrideManualPrices>(row, false);
	        }
	        if (!row.RecalculateDiscounts.GetValueOrDefault())
	        {
	            InvoiceInfo.Cache.SetValue<CreateSalesOrderFilter.overrideManualDiscounts>(row, false);
				InvoiceInfo.Cache.SetValue<CreateSalesOrderFilter.overrideManualDocGroupDiscounts>(row, false);
	        }
	    }

	    protected virtual void CreateSalesOrderFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	    {
	        CreateSalesOrderFilter row = e.Row as CreateSalesOrderFilter;
			CROpportunity opportunity = Opportunity.Current;
			if (row == null || opportunity == null) return;

	        PXUIFieldAttribute.SetEnabled<CreateSalesOrderFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true);
	        PXUIFieldAttribute.SetEnabled<CreateSalesOrderFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetEnabled<CreateSalesOrderFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetVisible<CreateSalesOrderFilter.confirmManualAmount>(sender, row, opportunity.ManualTotalEntry == true);
	    }

		protected virtual void CreateSalesOrderFilter_ConfirmManualAmount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CreateSalesOrderFilter row = e.Row as CreateSalesOrderFilter;
			CROpportunity opportunity = Opportunity.Current;
			if (row == null || opportunity == null) return;

			if (opportunity.ManualTotalEntry == true && e.NewValue as bool? != true)
			{
				sender.RaiseExceptionHandling<CreateSalesOrderFilter.confirmManualAmount>(row, e.NewValue,
					new PXSetPropertyException(Messages.ConfirmSalesOrderCreation, PXErrorLevel.Error));

				throw new PXSetPropertyException(Messages.ConfirmSalesOrderCreation, PXErrorLevel.Error);
			}
		}

		protected virtual void CreateSalesOrderFilter_ConfirmManualAmount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CreateSalesOrderFilter row = e.Row as CreateSalesOrderFilter;
			if (row == null) return;

			sender.RaiseExceptionHandling<CreateSalesOrderFilter.confirmManualAmount>(row, null, null);
		}

		#endregion

        #region QuoteFilter
        protected virtual void CopyQuoteFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
	    {
	        CopyQuoteFilter row = e.Row as CopyQuoteFilter;
	        if (row == null) return;

	        if (row.RecalculatePrices != true)
	        {
	            CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualPrices>(row, false);
	        }
	        if (row.RecalculateDiscounts != true)
	        {
	            CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDiscounts>(row, false);
				CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDocGroupDiscounts>(row, false);
	        }
	    }

	    protected virtual void CopyQuoteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	    {
	        CopyQuoteFilter row = e.Row as CopyQuoteFilter;
	        if (row == null) return;

			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.recalculatePrices>(sender, row, Quotes.Current?.QuoteType == CRQuoteTypeAttribute.Distribution);
	        PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true);
			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.recalculateDiscounts>(sender, row, Quotes.Current?.QuoteType == CRQuoteTypeAttribute.Distribution);
	        PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);
	    }
	    #endregion
		
        #endregion

        #region CacheAttached

		[PXUIField(DisplayName = "Order Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void SOOrder_OrderDate_CacheAttached(PXCache sender) { }

		[CustomerProspectVendor(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender) { }

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void CROpportunity_BAccountID_CacheAttached(PXCache sender) { }

	    [LocationID(typeof(Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>>),
	        DisplayName = "Location",
	        DescriptionField = typeof(Location.descr),
	        BqlField = typeof(Standalone.CROpportunityRevision.locationID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
	    public virtual void CROpportunity_LocationID_CacheAttached(PXCache sender) { }

        [PXDefault(false)]
        [PXDBCalced(typeof(True), typeof(Boolean))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void BAccountR_ViewInCrm_CacheAttached(PXCache sender) { }

	    [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.opportunityContactID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CRQuote_OpportunityContactID_CacheAttached(PXCache sender) { }

	    [PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CROpportunityRevision_OpportunityContactID_CacheAttached(PXCache sender) { }

	    [PXDBInt(BqlField = typeof(Standalone.CROpportunityRevision.opportunityAddressID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CRQuote_OpportunityAddressID_CacheAttached(PXCache sender) { }

	    [PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
        public virtual void CROpportunityRevision_OpportunityAddressID_CacheAttached(PXCache sender) { }

		[PXDBUShort()]
		[PXLineNbr(typeof(CROpportunity))]
		protected virtual void CROpportunityDiscountDetail_LineNbr_CacheAttached(PXCache e)
		{
		}

        #endregion

        #region Private Methods

		private bool VerifyField<TField>(object row, object newValue)
			where TField : IBqlField
		{
			if (row == null) return true;

			var result = false;
			var cache = Caches[row.GetType()];
			try
			{
				result = cache.RaiseFieldVerifying<TField>(row, ref newValue);
			}
			catch (StackOverflowException) { throw; }
			catch (OutOfMemoryException) { throw; }
			catch (Exception) { }

			return result;
		}

		private void FillDefaultBAccountID(PXCache cache, CROpportunity row)
		{
			if (row == null) return;

			if (row.ContactID != null)
			{
				var contact = (Contact)PXSelectReadonly<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(this, row.ContactID);
				if (contact != null)
				{
					row.BAccountID = contact.BAccountID;
					row.ParentBAccountID = contact.ParentBAccountID;
					cache.SetDefaultExt<CROpportunity.locationID>(row);
				}
			}

		}

		private bool IsMultyCurrency
		{
			get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
		}

		private IEnumerable SelectProducts(object quoteId)
		{
			if (quoteId == null)
				return new CROpportunityProducts[0];

			return PXSelect<CROpportunityProducts,
				Where<CROpportunityProducts.quoteID, Equal<Required<CROpportunity.quoteNoteID>>>>.
				Select(this, quoteId).
				RowCast<CROpportunityProducts>();
		}

        private Contact FillFromOpportunityContact(Contact Contact)
        {
            CRContact _CRContact = Opportunity_Contact.SelectSingle();

            Contact.FullName = _CRContact.FullName;
            Contact.Title = _CRContact.Title;
            Contact.FirstName = _CRContact.FirstName;
            Contact.LastName = _CRContact.LastName;
            Contact.Salutation = _CRContact.Salutation;
            Contact.Attention = _CRContact.Attention;
            Contact.EMail = _CRContact.Email;
            Contact.WebSite = _CRContact.WebSite;
            Contact.Phone1 = _CRContact.Phone1;
            Contact.Phone1Type = _CRContact.Phone1Type;
            Contact.Phone2 = _CRContact.Phone2;
            Contact.Phone2Type = _CRContact.Phone2Type;
            Contact.Phone3 = _CRContact.Phone3;
            Contact.Phone3Type = _CRContact.Phone3Type;
            Contact.Fax = _CRContact.Fax;
            Contact.FaxType = _CRContact.FaxType;
            return Contact;
        }
        private Address FillFromOpportunityAddress(Address Address)
        {
            CRAddress _CRAddress = Opportunity_Address.SelectSingle();

            Address.AddressLine1 = _CRAddress.AddressLine1;
            Address.AddressLine2 = _CRAddress.AddressLine2;
            Address.City = _CRAddress.City;
            Address.CountryID = _CRAddress.CountryID;
            Address.State = _CRAddress.State;
            Address.PostalCode = _CRAddress.PostalCode;
            return Address;
        }

		private bool IsSingleQuote(string opportunityId)
		{
			var quote = PXSelect<CRQuote, Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>>>.SelectSingleBound(this, null, opportunityId);
			return (quote.Count == 0);
		}

        #endregion

		#region External Tax Provider

		public virtual bool IsExternalTax(string taxZoneID)
		{
			return false;
		}

		public virtual CROpportunity CalculateExternalTax(CROpportunity opportunity)
		{
			return opportunity;
		}

		#endregion

        public override void Persist()
		{
		    base.Persist();
			this.Quotes.Cache.Clear();
			this.Quotes.View.Clear();
			this.Quotes.View.RequestRefresh();
		}

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (nameof(this.Products).Equals(viewName, StringComparison.OrdinalIgnoreCase))
			{
				// if Insert New Rows specified - just set available value
				if (PX.Common.PXExecutionContext.Current.Bag
						.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS, out object dontUpdateObj)
					&& dontUpdateObj is true)
				{
					keys[nameof(CROpportunityProducts.lineNbr)] = Opportunity.Current.ProductCntr + 1;
				}
				// Only if "Inventory ID" column imported
				else if (values.Contains(nameof(CROpportunityProducts.inventoryID)))
				{
					Guid? quoteId = Opportunity.Current.QuoteNoteID;
					string inventoryCD = (string)values[nameof(CROpportunityProducts.inventoryID)];
					CROpportunityProducts product = null;

					// Find first product already added with same inventory code
					// and use its keys to update
					if (quoteId != null && !String.IsNullOrEmpty(inventoryCD))
					{
						product = ProductsByQuoteIDAndInventoryCD.SelectSingle(quoteId, inventoryCD);
					}
					if (product != null)
					{
						keys[nameof(CROpportunityProducts.quoteID)] = product.QuoteID;
						keys[nameof(CROpportunityProducts.lineNbr)] = product.LineNbr;
					}
					else
					{
						keys[nameof(CROpportunityProducts.quoteID)] = null;
						keys[nameof(CROpportunityProducts.lineNbr)] = null;
					}
				}
			}

			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		#endregion

        #region Extensions
        /// <exclude/>
        public class MultiCurrency : CRMultiCurrencyGraph<OpportunityMaint, CROpportunity>
	    {	       
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CROpportunity)) { DocumentDate = typeof(CROpportunityRevision.documentDate) };
	        }

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Opportunity,
					Base.Products,
					Base.TaxLines,
					Base.Taxes,
					Base.GetExtension<Discount>().DiscountDetails
				};
			}

			protected override CurySource CurrentSourceSelect()
			{
				var primaryQt = Base.PrimaryQuoteQuery.SelectSingle();
				if (primaryQt == null || primaryQt.QuoteType != CRQuoteTypeAttribute.Project)
				{
					return base.CurrentSourceSelect();
				}

				var result = new CurySource();

				if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>() && Base.customer.Current != null)
				{
					result.CuryID = Base.customer.Current.CuryID;
					result.CuryRateTypeID = Base.customer.Current.CuryRateTypeID;
				}

				if (primaryQt.Status == CRQuoteStatusAttribute.Draft)
				{
					if (Base.customer.Current != null)
					{
						result.AllowOverrideCury = Base.customer.Current.AllowOverrideCury;
						result.AllowOverrideRate = Base.customer.Current.AllowOverrideRate;
					}
					else
					{
						result.AllowOverrideCury = true;
						result.AllowOverrideRate = true;
					}
				}
				else
				{
					result.AllowOverrideCury = false;
					result.AllowOverrideRate = false;
				}

				return result;
			}
		}

        /// <exclude/>
        public class SalesPrice : SalesPriceGraph<OpportunityMaint, CROpportunity>
	    {
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CROpportunity));
            }
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Descr = typeof(CROpportunityProducts.descr)};
            }
            protected override PriceClassSourceMapping GetPriceClassSourceMapping()
            {
                return new PriceClassSourceMapping(typeof(Location)) {PriceClassID = typeof(Location.cPriceClassID)};
            }
        }

        /// <exclude/>
        public class Discount : DiscountGraph<OpportunityMaint, CROpportunity>
        {
            public override void Initialize()
            {
                base.Initialize();
                if(this.Discounts == null)
                    this.Discounts = new PXSelectExtension<PX.Objects.Extensions.Discount.Discount>(this.DiscountDetails);
            }
            protected override DocumentMapping GetDocumentMapping()
            {
				return new DocumentMapping(typeof(CROpportunity)){ CuryDiscTot = typeof(CROpportunity.curyLineDocDiscountTotal), DocumentDate = typeof(CROpportunity.documentDate) };
			}
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Quantity = typeof(CROpportunityProducts.quantity)};
            }
            protected override DiscountMapping GetDiscountMapping()
            {
                return new DiscountMapping(typeof(CROpportunityDiscountDetail));
            }
           
            
            [PXViewName(Messages.DiscountDetails)]
            public PXSelect<CROpportunityDiscountDetail,
				Where<CROpportunityDiscountDetail.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
				OrderBy<Asc<CROpportunityDiscountDetail.lineNbr>>>
                      DiscountDetails;

            [PXSelector(typeof(Search<ARDiscount.discountID, 
                Where<ARDiscount.type, NotEqual<DiscountType.LineDiscount>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouse>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomer>,
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomerPrice>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventory>, 
                  And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventoryPrice>>>>>>>>))]
            [PXMergeAttributes]
            public override void Discount_DiscountID_CacheAttached(PXCache sender)
            {
            }
            
			[PXMergeAttributes]
            [CurrencyInfo(typeof(CROpportunity.curyInfoID))]
            public override void Discount_CuryInfoID_CacheAttached(PXCache sender)
            {
            }

            protected virtual void Discount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Opportunity.Current.TaxZoneID))
                {
                    Base.Opportunity.Current.IsTaxValid = false;
                }
            }
            protected virtual void Discount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Opportunity.Current.TaxZoneID))
                {
                    Base.Opportunity.Current.IsTaxValid = false;
                }
            }
            protected virtual void Discount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
            {
                if (Base.IsExternalTax(Base.Opportunity.Current.TaxZoneID))
                {
                    Base.Opportunity.Current.IsTaxValid = false;
                }
            }

            protected override bool AddDocumentDiscount => true;

            protected override void DefaultDiscountAccountAndSubAccount(PX.Objects.Extensions.Discount.Detail det)
            {                
            }

            public PXAction<CROpportunity> recalculatePrices;

            [PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
            [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
            public virtual IEnumerable RecalculatePrices(PXAdapter adapter)
            {
                List<CROpportunity> opportunities = new List<CROpportunity>(adapter.Get<CROpportunity>());
                foreach (CROpportunity opportunity in opportunities)
                {
                    if (recalcdiscountsfilter.View.Answer == WebDialogResult.None)
                    {
                        recalcdiscountsfilter.Cache.Clear();
                        RecalcDiscountsParamFilter filterdata = recalcdiscountsfilter.Cache.Insert() as RecalcDiscountsParamFilter;
                        if (filterdata != null)
                        {
                            filterdata.RecalcUnitPrices = false;
                            filterdata.RecalcDiscounts = false;
                            filterdata.OverrideManualPrices = false;
                            filterdata.OverrideManualDiscounts = false;
							filterdata.OverrideManualDocGroupDiscounts = false;
                        }
                    }

                    if (recalcdiscountsfilter.AskExt() != WebDialogResult.OK)
                        return opportunities;

                    RecalculateDiscountsAction(adapter);
                }

                return opportunities;
            }

			protected virtual void _(Events.RowSelected<PX.Objects.Extensions.Discount.Document> e)
			{
				var opportunity = e.Cache.GetMain(e.Row) as CROpportunity;
				CRQuote primaryQt = Base.PrimaryQuoteQuery.SelectSingle();

				this.recalculatePrices.SetEnabled(opportunity.IsActive == true
					&& opportunity.PrimaryQuoteType == CRQuoteTypeAttribute.Distribution
					&& primaryQt?.IsDisabled != true);
			}
		} 

        /// <exclude/>
	    public class SalesTax : TaxGraph<OpportunityMaint, CROpportunity>
	    {
            protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }
            protected override DocumentMapping GetDocumentMapping()
            {
	            return new DocumentMapping(typeof(CROpportunity))
	            {
	                DocumentDate = typeof(CROpportunity.documentDate),
                    CuryDocBal = typeof(CROpportunity.curyProductsAmount),
                    CuryDiscountLineTotal = typeof(CROpportunity.curyLineDiscountTotal),
					CuryDiscTot = typeof(CROpportunity.curyLineDocDiscountTotal),
					TaxCalcMode = typeof(CROpportunity.taxCalcMode)
				};
            }
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryTranAmt = typeof(CROpportunityProducts.curyAmount), DocumentDiscountRate = typeof(CROpportunityProducts.documentDiscountRate), GroupDiscountRate = typeof(CROpportunityProducts.groupDiscountRate) };
            }

	        protected override TaxDetailMapping GetTaxDetailMapping()
	        {
	            return new TaxDetailMapping(typeof(CROpportunityTax), typeof(CROpportunityTax.taxID));
	        }
            protected override TaxTotalMapping GetTaxTotalMapping()
            {
                return new TaxTotalMapping(typeof(CRTaxTran), typeof(CRTaxTran.taxID));
            }	       

			protected virtual void _(Events.FieldUpdated<CROpportunity, CROpportunity.curyAmount> e)
			{
				if (e.Row != null && e.Row.ManualTotalEntry == true)
					e.Row.CuryProductsAmount = e.Row.CuryAmount - e.Row.CuryDiscTot;				
			}

			protected virtual void _(Events.FieldUpdated<CROpportunity, CROpportunity.curyDiscTot> e)
		    {
			    if (e.Row != null && e.Row.ManualTotalEntry == true)
				    e.Row.CuryProductsAmount = e.Row.CuryAmount - e.Row.CuryDiscTot;
		    }

	        protected virtual void _(Events.FieldUpdated<CROpportunity, CROpportunity.manualTotalEntry> e)
	        {
	            if (e.Row != null && e.Row.ManualTotalEntry == false)
	            {
	                CalcTotals(null, false);
	            }
	        }

            protected virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
	        {
	            var row = sender.GetExtension<PX.Objects.Extensions.SalesTax.Document>(e.Row);
                if(row == null)
                    return;

                if(row.TaxCalc == null)
                    row.TaxCalc = TaxCalc.Calc;	            
	        }
            protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
            {
                base.CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
                CROpportunity doc = (CROpportunity )this.Documents.Cache.GetMain<PX.Objects.Extensions.SalesTax.Document>(this.Documents.Current);
	            bool manualEntry = doc.ManualTotalEntry == true;
				decimal CuryManualAmount = (decimal)(doc.CuryAmount ?? 0m);
                decimal CuryManualDiscTot = (decimal)(doc.CuryDiscTot ?? 0m);
                decimal CuryLineTotal = (decimal)(ParentGetValue<CROpportunity.curyLineTotal>() ?? 0m);
	            decimal CuryDiscAmtTotal = (decimal)(ParentGetValue<CROpportunity.curyLineDiscountTotal>() ?? 0m);
	            decimal CuryExtPriceTotal = (decimal)(ParentGetValue<CROpportunity.curyExtPriceTotal>() ?? 0m);
				decimal CuryDiscTotal = (decimal)(ParentGetValue<CROpportunity.curyLineDocDiscountTotal>() ?? 0m);

				decimal CuryDocTotal =
	                manualEntry 
					? CuryManualAmount - CuryManualDiscTot
                    : CuryLineTotal - CuryDiscTotal + CuryTaxTotal - CuryInclTaxTotal;

                if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<CROpportunity.curyProductsAmount>() ?? 0m)) == false)
                {
                    ParentSetValue<CROpportunity.curyProductsAmount>(CuryDocTotal);
                }
            }

            protected override string GetExtCostLabel(PXCache sender, object row)
            {
                return ((PXDecimalState)sender.GetValueExt<CROpportunityProducts.curyExtPrice>(row)).DisplayName;
            }

            protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
            {
                var row = child as PX.Data.PXResult<PX.Objects.Extensions.SalesTax.Detail>;
                if (row != null)
                {
                    var det = PXResult.Unwrap<PX.Objects.Extensions.SalesTax.Detail>(row);
                    var line = (CROpportunityProducts)det.Base;
                    line.CuryExtPrice = value;
                    sender.Update(row);
                }
            }

            protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
            {
                Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
                var currents = new[]
	            {
		            row != null && row is PX.Objects.Extensions.SalesTax.Detail ? Details.Cache.GetMain((PX.Objects.Extensions.SalesTax.Detail)row):null,
					((OpportunityMaint)graph).Opportunity.Current
				};

				IComparer<Tax> taxComparer = GetTaxByCalculationLevelComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

                foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                    LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
                        And<TaxRev.outdated, Equal<boolFalse>,
                            And<TaxRev.taxType, Equal<TaxType.sales>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                            And<Tax.taxType, NotEqual<CSTaxType.use>,
                            And<Tax.reverseTax, Equal<boolFalse>,
                            And<Current<CROpportunity.documentDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
                    Where>
                    .SelectMultiBound(graph, currents, parameters))
                {
                    tail[((Tax)record).TaxID] = record;
                }
                List<object> ret = new List<object>();
                switch (taxchk)
                {
                    case PXTaxCheck.Line:
                        foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                            Where<CROpportunityTax.quoteID, Equal<Current<CROpportunity.quoteNoteID>>,
                                And<CROpportunityTax.lineNbr, Equal<Current<CROpportunityProducts.lineNbr>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0) && taxComparer.Compare((PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcLine:
                        foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                            Where<CROpportunityTax.quoteID, Equal<Current<CROpportunity.quoteNoteID>>,
                                And<CROpportunityTax.lineNbr, Less<intMax>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && ((CROpportunityTax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
                                    && taxComparer.Compare((PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcTotals:
                        foreach (CRTaxTran record in PXSelect<CRTaxTran,
                            Where<CRTaxTran.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
                            OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (record.TaxID != null && tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && taxComparer.Compare((PXResult<CRTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CRTaxTran, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    default:
                        return ret;
                }
            }

            protected override List<Object> SelectDocumentLines(PXGraph graph, object row)
            {
                var res = PXSelect<CROpportunityProducts,
                            Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>>
                            .SelectMultiBound(graph, new object[] { row })
                            .RowCast<CROpportunityProducts>()
                            .Select(_ => (object)_)
                            .ToList();
                return res;
            }

            #region CRTaxTran
            protected virtual void CRTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                    return;

                PXUIFieldAttribute.SetEnabled<CRTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
            }
            #endregion

            #region CROpportunityTax
            protected virtual void CRTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
            {
                CRTaxTran row = e.Row as CRTaxTran;
                if (row == null) return;

                if (e.Operation == PXDBOperation.Delete)
                {
                    CROpportunityTax tax = (CROpportunityTax)Base.TaxLines.Cache.Locate(FindCROpportunityTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Deleted ||
                         Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.InsertedDeleted)
                        e.Cancel = true;
                }
                if (e.Operation == PXDBOperation.Update)
                {
                    CROpportunityTax tax = (CROpportunityTax)Base.TaxLines.Cache.Locate(FindCROpportunityTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Updated)
                        e.Cancel = true;
                }
            }
            internal static CROpportunityTax FindCROpportunityTax(CRTaxTran tran)
            {
                var list = PXSelect<CROpportunityTax,
                    Where<CROpportunityTax.quoteID, Equal<Required<CROpportunityTax.quoteID>>,
                        And<CROpportunityTax.lineNbr, Equal<Required<CROpportunityTax.lineNbr>>,
                        And<CROpportunityTax.taxID, Equal<Required<CROpportunityTax.taxID>>>>>>
                        .SelectSingleBound(new PXGraph(), new object[] { }).RowCast<CROpportunityTax>();
                return list.FirstOrDefault();
            }
            #endregion
        }

        /// <exclude/>
		public class ContactAddress : ContactAddressGraph<OpportunityMaint>
		{
			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity))
				{
					DocumentAddressID = typeof(CROpportunity.opportunityAddressID),
					DocumentContactID = typeof(CROpportunity.opportunityContactID),
					ShipAddressID = typeof(CROpportunity.shipAddressID),
					ShipContactID = typeof(CROpportunity.shipContactID)
				};
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRContact)) { EMail = typeof(CRContact.email) };
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(CRAddress));
			}
		    protected override PXCache GetContactCache()
		    {
		        return Base.Opportunity_Contact.Cache;
		    }
		    protected override PXCache GetAddressCache()
		    {
		        return Base.Opportunity_Address.Cache;
		    }
			protected override PXCache GetShippingContactCache()
			{
				return Base.Shipping_Contact.Cache;
			}
			protected override PXCache GetShippingAddressCache()
			{
				return Base.Shipping_Address.Cache;
			}
            protected override IPersonalContact GetCurrentContact()
			{
				var contact = Base.Opportunity_Contact.SelectSingle();
				return contact;
			}
            protected override IPersonalContact GetCurrentShippingContact()
			{
				var contact = Base.Shipping_Contact.SelectSingle();
				return contact;
			}
			protected override IAddress GetCurrentShippingAddress()
			{
				var address = Base.Shipping_Address.SelectSingle();
				return address;
			}

			protected override IPersonalContact GetEtalonContact()
			{
				bool isDirty = Base.Opportunity_Contact.Cache.IsDirty;
				var contact = Base.Opportunity_Contact.Insert();
				Base.Opportunity_Contact.Cache.SetStatus(contact, PXEntryStatus.Held);
				Base.Opportunity_Contact.Cache.IsDirty = isDirty;
				return contact;
			}

			protected override IPersonalContact GetEtalonShippingContact()
			{
				bool isDirty = Base.Shipping_Contact.Cache.IsDirty;
				var contact = Base.Shipping_Contact.Insert();
				Base.Shipping_Contact.Cache.SetStatus(contact, PXEntryStatus.Held);
				Base.Shipping_Contact.Cache.IsDirty = isDirty;
				return contact;
			}

			protected override IAddress GetCurrentAddress()
			{
				var address = Base.Opportunity_Address.SelectSingle();
				return address;
			}
            protected override IAddress GetEtalonAddress()
			{
				bool isDirty = Base.Opportunity_Address.Cache.IsDirty;
				var address = Base.Opportunity_Address.Insert();
				Base.Opportunity_Address.Cache.SetStatus(address, PXEntryStatus.Held);
				Base.Opportunity_Address.Cache.IsDirty = isDirty;
				return address;
			}
			protected override IAddress GetEtalonShippingAddress()
			{
				bool isDirty = Base.Shipping_Address.Cache.IsDirty;
				var address = Base.Shipping_Address.Insert();
				Base.Shipping_Address.Cache.SetStatus(address, PXEntryStatus.Held);
				Base.Shipping_Address.Cache.IsDirty = isDirty;
				return address;
			}
		}

		/// <exclude/>
		public class DefaultOpportunityOwner : CRDefaultDocumentOwner<
			OpportunityMaint, CROpportunity,
			CROpportunity.classID, CROpportunity.ownerID, CROpportunity.workgroupID>
		{ }

		/// <exclude/>
		public class CreateBothAccountAndContactFromOpportunityGraphExt : CRCreateBothContactAndAccountAction<OpportunityMaint, CROpportunity, CreateAccountFromOpportunityGraphExt, CreateContactFromOpportunityGraphExt> { }

		/// <exclude/>
		public class CreateAccountFromOpportunityGraphExt : CRCreateAccountAction<OpportunityMaint, CROpportunity>
		{
			protected override string TargetType => CRTargetEntityType.CROpportunity;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity)) { RefContactID = typeof(CROpportunity.contactID) };
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRContact));
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(CRAddress));
			}

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.Activities;

			protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.accountClass> e)
			{
				if (ExistingAccount.SelectSingle() is BAccount existingAccount)
				{
					e.NewValue = existingAccount.ClassID;
					e.Cancel = true;
					return;
				}

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity == null) return;

				CROpportunityClass cls = PXSelect<
						CROpportunityClass,
					Where<
						CROpportunityClass.cROpportunityClassID,
						Equal<Required<CROpportunity.classID>>>>
					.Select(Base, opportunity.ClassID);

				if (cls?.TargetBAccountClassID != null)
				{
					e.NewValue = cls.TargetBAccountClassID;
				}
				else
				{
					e.NewValue = Base.Setup.Current?.DefaultCustomerClassID;
				}

				e.Cancel = true;
			}

			protected override void _(Events.RowSelected<AccountsFilter> e)
			{
				base._(e);

				AccountsFilter row = e.Row as AccountsFilter;
				if (row == null)
					return;

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity.ContactID != null)
				{
					PXUIFieldAttribute.SetVisible<AccountsFilter.linkContactToAccount>(e.Cache, row, true);
					Contact contact = Base.CurrentContact.Current ?? Base.CurrentContact.SelectSingle();
					if (contact == null)
					{
						PXUIFieldAttribute.SetEnabled<AccountsFilter.linkContactToAccount>(e.Cache, row, false);
					}
					else
					{
						if (contact.BAccountID != null)
						{
							PXUIFieldAttribute.SetWarning<AccountsFilter.linkContactToAccount>(e.Cache, row, Messages.AccountContactValidation);
						}
						else
						{
							PXUIFieldAttribute.SetEnabled<AccountsFilter.linkContactToAccount>(e.Cache, row, true);
						}
					}
				}
			}

			protected virtual void _(Events.FieldDefaulting<AccountsFilter, AccountsFilter.linkContactToAccount> e)
			{
				AccountsFilter row = e.Row as AccountsFilter;
				if (row == null)
					return;

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity.ContactID != null)
				{
					Contact contact = Base.CurrentContact.Current ?? Base.CurrentContact.SelectSingle();
					if (contact == null)
					{
						e.NewValue = false;
					}
					else
					{
						if (contact.BAccountID != null)
						{
							e.NewValue = false;
						}
						else
						{
							e.NewValue = true;
						}
					}
				}
				else
				{
					e.NewValue = false;
				}

				e.Cancel = true;
			}

			protected override BAccount CreateMaster(BusinessAccountMaint graph, AccountConversionOptions config)
			{
				BAccount account = base.CreateMaster(graph, config);

				CR.Location location = graph.DefLocation.Select();
				if (location.CTaxZoneID != null)
				{
					location.CTaxZoneID = null;
					graph.DefLocation.Update(location);
				}

				return account;
			}

		}

		/// <exclude/>
		public class CreateContactFromOpportunityGraphExt : CRCreateContactAction<OpportunityMaint, CROpportunity>
		{
			protected override string TargetType => CRTargetEntityType.CROpportunity;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CROpportunity)) { RefContactID = typeof(CROpportunity.contactID) };
			}
			protected override DocumentContactMapping GetDocumentContactMapping()
			{
				return new DocumentContactMapping(typeof(CRContact));
			}
			protected override DocumentContactMethodMapping GetDocumentContactMethodMapping()
			{
				return new DocumentContactMethodMapping(typeof(CRContact));
			}
			protected override DocumentAddressMapping GetDocumentAddressMapping()
			{
				return new DocumentAddressMapping(typeof(CRAddress));
			}

			protected override PXSelectBase<CRPMTimeActivity> Activities => Base.Activities;

			protected virtual void _(Events.FieldDefaulting<ContactFilter, ContactFilter.contactClass> e)
			{
				if (ExistingContact.SelectSingle() is Contact existingContact)
				{
					e.NewValue = existingContact.ClassID;
					e.Cancel = true;
					return;
				}

				CROpportunity opportunity = Base.Opportunity.Current;
				if (opportunity == null) return;

				CROpportunityClass cls = PXSelect<
						CROpportunityClass,
					Where<
						CROpportunityClass.cROpportunityClassID,
						Equal<Required<CROpportunity.classID>>>>
					.Select(Base, opportunity.ClassID);

				if (cls?.TargetContactClassID != null)
				{
					e.NewValue = cls.TargetContactClassID;
				}
				else
				{
					e.NewValue = Base.Setup.Current?.DefaultContactClassID;
				}

				e.Cancel = true;
			}
		}

		[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2019R1+ " Use SortExtensionsBy<> instead.")]
        public class ServiceRegistration : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.ActivateOnApplicationStart<ExtensionSorting>();
            }
            private class ExtensionSorting
            {
                private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
                {
					{typeof(ContactAddress), 1},
                    {typeof(MultiCurrency), 2},
                    {typeof(SalesPrice), 3},
                    {typeof(Discount), 4},
                    {typeof(SalesTax), 5},
                };
                public ExtensionSorting()
                {
					PXBuildManager.SortExtensions += (list) => PXBuildManager.PartialSort(list, _order);
                }
            }
        }

		// uncomment when ServiceRegistration is removed
		//public class ExtensionSort : SortExtensionsBy<ExtensionOrderFor<OpportunityMaint>.FilledWith<ContactAddress, MultiCurrenty, SalesPrice, Discount, SalesTax>> { }
		#endregion
		public static bool ProjectQuotesInstalled { get { return PXAccess.FeatureInstalled<FeaturesSet.projectQuotes>(); } }
		public static bool SalesQuotesInstalled { get { return PXAccess.FeatureInstalled<FeaturesSet.salesQuotes>(); } }
	}
}
