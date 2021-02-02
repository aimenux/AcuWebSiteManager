using System;
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.CM.Extensions;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.SO;
using System.Collections.Generic;
using System.Diagnostics;
using PX.Objects.GL;
using PX.Objects.Extensions.MultiCurrency.CR;
using PX.Objects.Extensions.SalesPrice;
using PX.Objects.Extensions.Discount;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.Extensions.ContactAddress;
using Autofac;
using System.Web.Compilation;
using PX.Data.DependencyInjection;
using PX.Objects.Common.Discount;
using PX.Objects.EP;
using PX.Objects.CR.Standalone;
using PX.Objects.CR.DAC;
using PX.LicensePolicy;

namespace PX.Objects.CR
{
    public class QuoteMaint : PXGraph<QuoteMaint>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
    {
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
            [PXDefault()]
            public virtual string OpportunityID{ get; set; }
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
            public virtual bool? RecalculatePrices { get; set; }
            #endregion

            #region OverrideManualPrices
            public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
            [PXBool()]
            [PXUIField(DisplayName = "Override Manual Prices", Enabled = false)]
            public virtual bool? OverrideManualPrices { get; set; }
            #endregion

            #region RecalculateDiscounts
            public abstract class recalculateDiscounts : PX.Data.BQL.BqlBool.Field<recalculateDiscounts> { }
            [PXBool()]
            [PXUIField(DisplayName = "Recalculate Discounts")]
            public virtual bool? RecalculateDiscounts { get; set; }
            #endregion

            #region OverrideManualDiscounts
            public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
            [PXBool()]
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

        [PXViewName(Messages.CRQuote)]
		[PXCopyPasteHiddenFields(typeof(CRQuote.approved), typeof(CRQuote.rejected))]
        public PXSelect<CRQuote,
                Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>, And<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>>>> Quote;

        [PXHidden]
		[PXCopyPasteHiddenFields(typeof(CRQuote.approved), typeof(CRQuote.rejected))]
        public PXSelect<CRQuote,
                Where<CRQuote.opportunityID, Equal<Current<CRQuote.opportunityID>>,
                    And<CRQuote.quoteNbr, Equal<Current<CRQuote.quoteNbr>>>>>
            QuoteCurrent;
  

        [PXHidden]
        public PXSelect<Address>
            Address;

        [PXHidden]
        public PXSetup<Contact, Where<Contact.contactID, Equal<Optional<CRQuote.contactID>>>> Contacts;

        [PXHidden]
        public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<CRQuote.bAccountID>>>> customer;

        [PXViewName(Messages.Activities)]
        [PXFilterable]
        [CRReference(typeof(CRQuote.bAccountID), typeof(CRQuote.contactID))]
        [CRDefaultMailTo(typeof(Select<CRContact, Where<CRContact.contactID, Equal<Current<CRQuote.opportunityContactID>>>>))]
		public CRActivityList<CRQuote> Activities;


        [PXViewName(Messages.QuoteProducts)]
        [PXImport(typeof(CRQuote))]
		public PXOrderedSelect<CRQuote, CROpportunityProducts,
				Where<CROpportunityProducts.quoteID, Equal<Current<CRQuote.quoteID>>>,
				OrderBy<Asc<CROpportunityProducts.sortOrder>>>
			Products;

		[PXHidden]
		public PXSelect<CROpportunityRevision> FakeRevisionCache;

        public PXSelect<CROpportunityTax,
            Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
				And<CROpportunityTax.lineNbr, Less<intMax>>>,
            OrderBy<Asc<CROpportunityTax.taxID>>> TaxLines;

        [PXViewName(Messages.QuoteTax)]
        public PXSelectJoin<CRTaxTran,
            InnerJoin<Tax, On<Tax.taxID, Equal<CRTaxTran.taxID>>>,
            Where<CRTaxTran.quoteID, Equal<Current<CRQuote.quoteID>>>,
            OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>> Taxes;


        public PXSetup<Location,
            Where<Location.bAccountID, Equal<Current<CRQuote.bAccountID>>,
                And<Location.locationID, Equal<Optional<CRQuote.locationID>>>>> location;

        
        [PXViewName(Messages.QuoteContact)]
        public PXSelect<CRContact, Where<CRContact.contactID, Equal<Current<CRQuote.opportunityContactID>>>> Quote_Contact;

        [PXViewName(Messages.QuoteAddress)]
        public PXSelect<CRAddress, Where<CRAddress.addressID, Equal<Current<CRQuote.opportunityAddressID>>>> Quote_Address;

        [PXViewName(Messages.ShippingContact)]
        public PXSelect<CRShippingContact, Where<CRShippingContact.contactID, Equal<Current<CRQuote.shipContactID>>>> Shipping_Contact;

        [PXViewName(Messages.ShippingAddress)]
        public PXSelect<CRShippingAddress, Where<CRShippingAddress.addressID, Equal<Current<CRQuote.shipAddressID>>>> Shipping_Address;

        [PXHidden]
        public PXSelectJoin<Contact,
            LeftJoin<Address, On<Contact.defAddressID, Equal<Address.addressID>>>,
            Where<Contact.contactID, Equal<Current<CRQuote.contactID>>>> CurrentContact;

		[PXHidden]
		public PXSelectJoin<Standalone.CROpportunity, 
                LeftJoin<CROpportunityRevision,
                    On<CROpportunityRevision.noteID, Equal<Standalone.CROpportunity.defQuoteID>>,
				LeftJoin<Standalone.CRQuote, 
					On<Standalone.CRQuote.quoteID, Equal<Standalone.CROpportunityRevision.noteID>>>>,
				Where<Standalone.CROpportunity.opportunityID, Equal<Optional<CRQuote.opportunityID>>>>
            Opportunity;

	    [PXViewName(Messages.Opportunity)]
	    public PXSelect<CROpportunity,			    
			    Where<CROpportunity.opportunityID, Equal<Optional<CRQuote.opportunityID>>>> CurrentOpportunity;

		[PXHidden]
		public PXSelect<AP.Vendor> Vendors;

		[PXViewName(Messages.Approval)]
		public EPApprovalActionExtensionPersistent<
			CRQuote,
			CRQuote.approved,
			CRQuote.rejected,
			CRSetup.quoteApprovalMapID,
			CRSetup.quoteApprovalNotificationID> Approval;

        [PXViewName(Messages.CreateAccount)]
		[PXCopyPasteHiddenView]
        public PXFilter<CopyQuoteFilter> CopyQuoteInfo;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }
		
		public override bool ProviderInsert(Type table, params PXDataFieldAssign[] pars)
		{
			if (table == typeof(CROpportunityRevision))
			{
				foreach (var param in pars)
				{
					var cacheColumn = new Data.SQLTree.Column(Caches[typeof(CROpportunityRevision)].GetBqlField<CROpportunityRevision.noteID>().Name, table.Name);
					if (param.Column.Equals(cacheColumn))
					{
						var noteID = Guid.Parse(param.Value.ToString());
						var revisions = PXSelect<CROpportunityRevision, Where<CROpportunityRevision.noteID, Equal<Required<CROpportunityRevision.noteID>>>>.SelectSingleBound(this, null, noteID);
						if (revisions.Count > 0)
							throw new PXDbOperationSwitchRequiredException(table.Name, "Need update instead of insert");
					}
				}
			}                   
			return base.ProviderInsert(table, pars);
		}

        public override bool ProviderDelete(Type table, params PXDataFieldRestrict[] pars)
        {
            if (table == typeof(CROpportunityRevision))
            {
				var cacheColumn = new Data.SQLTree.Column(Caches[typeof(CROpportunityRevision)].GetBqlField<CROpportunityRevision.opportunityID>().Name, table.Name);
				foreach (var param in pars)
                {
                    if (param.Column.Equals(cacheColumn))
                    {
                        if (param.Value != null && IsSingleQuote(param.Value.ToString()))
                        {
                            return true;
                        }
                    }
                }

                
            }
            return base.ProviderDelete(table, pars);
        }

        #endregion
		
        #region Ctors

        public QuoteMaint()
        {
            if (string.IsNullOrEmpty(Setup.Current.QuoteNumberingID))
            {
                throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
            }

            Activities.GetNewEmailAddress =
                () =>
                {
                    var current = Quote.Current;
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


            var bAccountCache = Caches[typeof(BAccount)];
            var CRQuoteCache = Caches[typeof(CRQuote)];

            CRQuote quotecurrent = CRQuoteCache.Current as CRQuote;            

            actionsFolder.MenuAutoOpen = true;
            actionsFolder.AddMenuAction(copyQuote);
            actionsFolder.AddMenuAction(sendQuote);
            actionsFolder.AddMenuAction(printQuote);
            actionsFolder.AddMenuAction(validateAddresses);
            
            if (quotecurrent != null)
            {
                PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
                PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);

                PXUIFieldAttribute.SetEnabled<CRQuote.branchID>(CRQuoteCache, null, false);
                PXUIFieldAttribute.SetEnabled<CRQuote.projectID>(CRQuoteCache, null, false);

                PXDefaultAttribute.SetPersistingCheck<CRQuote.branchID>(CRQuoteCache, null, PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<CRQuote.projectID>(CRQuoteCache, null, PXPersistingCheck.Nothing);

                PXUIFieldAttribute.SetEnabled<CRQuote.locationID>(CRQuoteCache, quotecurrent, quotecurrent.BAccountID != null);
                PXDefaultAttribute.SetPersistingCheck<CRQuote.locationID>(CRQuoteCache, quotecurrent, quotecurrent.BAccountID == null ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

                Actions[nameof(PrimaryQuote)].SetEnabled(!String.IsNullOrEmpty(quotecurrent.OpportunityID) && quotecurrent.IsPrimary != true);
			}
	        this.Views.Caches.Remove(typeof(Standalone.CROpportunity));
	        this.Views.Caches.Remove(typeof(CROpportunity));

		}
		
		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<CRQuote>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(CROpportunityProducts), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<CROpportunityProducts.quoteID>(((QuoteMaint)graph).Quote.Current?.QuoteID)
						};
					}));
			}
		}

		#endregion

		#region Actions

		public PXSave<CRQuote> Save;
        public PXAction<CRQuote> cancel;
        public PXInsert<CRQuote> insert;        
        public PXDelete<CRQuote> Delete;
        public PXFirst<CRQuote> First;
        public PXPrevious<CRQuote> previous;
        public PXNext<CRQuote> next;
        public PXLast<CRQuote> Last;
        public PXAction<CRQuote> viewOnMap;
        public PXAction<CRQuote> validateAddresses;

        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
            string oppID = Quote.Current != null ? Quote.Current.OpportunityID : null;
            Quote.Cache.Clear();
            foreach (CRQuote quote in (new PXCancel<CRQuote>(this, "Cancel")).Press(adapter))
            {
                return new object[] { quote };
            }
            return new object[0];
        }

        [PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXPreviousButton]
        protected virtual IEnumerable Previous(PXAdapter adapter)
        {
            foreach (CRQuote loc in (new PXPrevious<CRQuote>(this, "Prev")).Press(adapter))
            {
                if (Quote.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
                {
                    return Last.Press(adapter);
                }
                else
                {
                    return new object[] { loc };
                }
            }
            return new object[0];
        }

        [PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXNextButton]
        protected virtual IEnumerable Next(PXAdapter adapter)
        {
            foreach (CRQuote loc in (new PXNext<CRQuote>(this, "Next")).Press(adapter))
            {
                if (Quote.Cache.GetStatus(loc) == PXEntryStatus.Inserted)
                {
                    return First.Press(adapter);
                }
                else
                {
                    return new object[] { loc };
                }
            }
            return new object[0];
        }

        public PXAction<CRQuote> actionsFolder;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable ActionsFolder(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<CRQuote> viewMainOnMap;

        [PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ViewMainOnMap()
        {
            var address = Quote_Address.SelectSingle();
            if (address != null)
            {
                BAccountUtility.ViewOnMap(address);
            }
        }

        public PXAction<CRQuote> ViewShippingOnMap;

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

        public PXAction<CRQuote> primaryQuote;
        [PXUIField(DisplayName = Messages.MarkAsPrimary)]
        [PXButton]
        public virtual IEnumerable PrimaryQuote(PXAdapter adapter)
        {
			IEnumerable<CRQuote> quotes = adapter.Get<CRQuote>().ToArray();

			Save.Press();

			foreach (CRQuote item in quotes)
            {
	            Opportunity.Cache.Clear();
				var rec = (PXResult<Standalone.CROpportunity>)
					this.Opportunity.View.SelectSingleBound(new object[] { item });	            

				this.Opportunity.Current = rec;
	            this.Opportunity.Current.DefQuoteID = item.QuoteID;
                item.DefQuoteID = item.QuoteID;
                Standalone.CROpportunity opudate = Opportunity.Cache.Update(this.Opportunity.Current) as Standalone.CROpportunity;
				this.Views.Caches.Add(typeof(Standalone.CROpportunity));
                CRQuote upitem = Quote.Cache.Update(item) as CRQuote;
				Save.Press();
				yield return upitem;
            }
        }


        public PXAction<CRQuote> copyQuote;
        [PXUIField(DisplayName = Messages.CopyQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable CopyQuote(PXAdapter adapter)
        {
            List<CRQuote> CRQoutes = new List<CRQuote>(adapter.Get().Cast<CRQuote>());
            foreach (CRQuote quote in CRQoutes)
            {
                if (CopyQuoteInfo.View.Answer == WebDialogResult.None)
                {
                    CopyQuoteInfo.Cache.Clear();
                    CopyQuoteFilter filterdata = CopyQuoteInfo.Cache.Insert() as CopyQuoteFilter;
                    filterdata.Description = quote.Subject + Messages.QuoteCopy;
                    filterdata.RecalculatePrices = false;
                    filterdata.RecalculateDiscounts = false;
                    filterdata.OverrideManualPrices = false;
                    filterdata.OverrideManualDiscounts = false;
					filterdata.OverrideManualDocGroupDiscounts = false;
					filterdata.OpportunityID = quote.OpportunityID;
                }

                if (CopyQuoteInfo.AskExt() != WebDialogResult.Yes)
                    return CRQoutes;

                Save.Press();
                PXLongOperation.StartOperation(this, () => CopyToQuote(quote, CopyQuoteInfo.Current));
            }
            return CRQoutes;
        }

        public virtual void CopyToQuote(CRQuote currentquote, CopyQuoteFilter param)
        {
	        this.Quote.Current = currentquote;

			QuoteMaint graph = PXGraph.CreateInstance<QuoteMaint>();
	        graph.SelectTimeStamp();
			var quote = (CRQuote)graph.Quote.Cache.CreateInstance();
	        graph.Opportunity.Current = graph.Opportunity.SelectSingle(param.OpportunityID);
			quote.OpportunityID = param.OpportunityID;
            quote = graph.Quote.Insert(quote);
	        CurrencyInfo info =
		        PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CRQuote.curyInfoID>>>>
			        .Select(this);
	        info.CuryInfoID = null;
	        info = (CurrencyInfo)graph.Caches<CurrencyInfo>().Insert(info);

			foreach (string field in Quote.Cache.Fields)
            {
                if (graph.Quote.Cache.Keys.Contains(field)
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.quoteID))
                    || field == graph.Quote.Cache.GetField(typeof(CRQuote.status))
                    || field == graph.Quote.Cache.GetField(typeof(CRQuote.isPrimary))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.expirationDate))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.approved))
					|| field == graph.Quote.Cache.GetField(typeof(CRQuote.rejected)))
                    continue;

                graph.Quote.Cache.SetValue(quote, field,
                    Quote.Cache.GetValue(currentquote, field));
            }
	        quote.CuryInfoID = info.CuryInfoID;
	        quote.Subject = param.Description;
	        quote.DocumentDate = this.Accessinfo.BusinessDate;	        

			string note = PXNoteAttribute.GetNote(Quote.Cache, currentquote);
            Guid[] files = PXNoteAttribute.GetFileNotes(Quote.Cache, currentquote);

			if (!IsSingleQuote(quote.OpportunityID))
			{
				object quoteID;
				graph.Quote.Cache.RaiseFieldDefaulting<CRQuote.noteID>(quote, out quoteID);
				quote.QuoteID = quote.NoteID = (Guid?)quoteID;
			}

			PXNoteAttribute.SetNote(graph.Quote.Cache, quote, note);
			PXNoteAttribute.SetFileNotes(graph.Quote.Cache, quote, files);

			CloneView(this.Products.View, graph, quote.QuoteID);
			var DiscountExt = this.GetExtension<Discount>();
			CloneView(Views[nameof(DiscountExt.DiscountDetails)], graph, quote.QuoteID);
			CloneView(TaxLines.View, graph, quote.QuoteID);
			CloneView(Taxes.View, graph, quote.QuoteID, nameof(CRTaxTran.RecordID));

			if (currentquote.AllowOverrideContactAddress == true)
            {
				CloneView(Quote_Contact.View, graph, quote.QuoteID);
				quote.OpportunityContactID = graph.Quote_Contact.Current.ContactID;
				CloneView(Quote_Address.View, graph, quote.QuoteID);
				quote.OpportunityAddressID = graph.Quote_Address.Current.AddressID;
            }
	        graph.Quote.Update(quote);
			var Discount = graph.GetExtension<QuoteMaint.Discount>();
	        Discount.recalcdiscountsfilter.Current.OverrideManualDiscounts = param.OverrideManualDiscounts == true;
			Discount.recalcdiscountsfilter.Current.OverrideManualDocGroupDiscounts = param.OverrideManualDocGroupDiscounts == true;
			Discount.recalcdiscountsfilter.Current.OverrideManualPrices = param.OverrideManualPrices == true;
	        Discount.recalcdiscountsfilter.Current.RecalcDiscounts = param.RecalculateDiscounts == true;
	        Discount.recalcdiscountsfilter.Current.RecalcUnitPrices = param.RecalculatePrices == true;
	        graph.Actions[nameof(Discount.RecalculateDiscountsAction)].Press();

			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
        }

		protected virtual string DefaultReportID => "CR604500";

		protected virtual string DefaultNotificationCD => "CRQUOTE";


		public PXAction<CRQuote> sendQuote;
        [PXUIField(DisplayName = "Send Quote")]
        public IEnumerable SendQuote(PXAdapter adapter)
        {
            foreach (CRQuote item in adapter.Get<CRQuote>())
            {
                var parameters = new Dictionary<string, string>();
				parameters[nameof(CRQuote) + "." + nameof(CRQuote.OpportunityID)] = item.OpportunityID;
				parameters[nameof(CRQuote) + "." + nameof(CRQuote.QuoteNbr)] = item.QuoteNbr;
                Activities.SendNotification(CRNotificationSource.BAccount, DefaultNotificationCD, item.BranchID, parameters);
                item.Status = CRQuoteStatusAttribute.Sent;
                Quote.Update(item);
                Save.Press();
                yield return item;
            }                
        }
	  

        public PXAction<CRQuote> printQuote;
        [PXUIField(DisplayName = "Print Quote")]
        public IEnumerable PrintQuote(PXAdapter adapter)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string actualReportID = DefaultReportID;

            foreach (CRQuote item in adapter.Get<CRQuote>())
            {
				parameters[nameof(CRQuote.OpportunityID)] = item.OpportunityID;
				parameters[nameof(CRQuote.QuoteNbr)] = item.QuoteNbr;

                throw new PXReportRequiredException(parameters, actualReportID,"Report " + actualReportID);                
            }
            return adapter.Get();
        }

        [PXUIField(DisplayName = CR.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select/*, FieldClass = CS.Messages.ValidateAddress*/)]
        [PXButton()]
        public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
        {
            foreach (CRQuote current in adapter.Get<CRQuote>())
            {
                bool needSave = false;
                Save.Press();

                if (current != null)
                {
                    CRAddress address = this.Quote_Address.Select();
                    if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
                    {
                        if (PXAddressValidator.Validate<CRAddress>(this, address, true))
                        {
                            needSave = true;
                        }
                    }

                    CRShippingAddress shipAddress = this.Shipping_Address.Select();
                    if (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
                    {
                        if (PXAddressValidator.Validate<CRShippingAddress>(this, shipAddress, true))
                        {
                            needSave = true;
                        }
                    }

                    if (needSave)
                    {
                        this.Save.Press();
                    }
                }
                yield return current;
            }
        }

        #endregion




        #region Contacts

        [CustomerProspectVendor(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual void Contact_BAccountID_CacheAttached(PXCache sender)
        {

        }

        #endregion
		
		#region EPApproval Cache Attached
		[PXDBDate]
		[PXDefault(typeof(CRQuote.documentDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender) { }

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(CRQuote.subject), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender) { }

		[PXDBLong]
		[CurrencyInfo(typeof(CRQuote.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender) { }

		[PXDBDecimal(4)]
		[PXDefault(typeof(CRQuote.curyProductsAmount), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender) { }

		[PXDBDecimal(4)]
		[PXDefault(typeof(CRQuote.productsAmount), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender) { }

		[PXDBInt()]
		[PXDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender) { }

		[PXDBGuid()]
		[PXDefault(typeof(Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<CRQuote.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender) { }
		#endregion

        #region QuoteFilter
        protected virtual void CopyQuoteFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CopyQuoteFilter row = e.Row as CopyQuoteFilter;
            if (row == null) return;

            if (!row.RecalculatePrices == true)
            {
                CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualPrices>(row, false);
            }
            if (!row.RecalculateDiscounts == true)
            {
                CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDiscounts>(row, false);
				CopyQuoteInfo.Cache.SetValue<CopyQuoteFilter.overrideManualDocGroupDiscounts>(row, false);
			}
        }

        protected virtual void CopyQuoteFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CopyQuoteFilter row = e.Row as CopyQuoteFilter;
            if (row == null) return;

            PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualPrices>(sender, row, row.RecalculatePrices == true);
            PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDiscounts>(sender, row, row.RecalculateDiscounts == true);
			PXUIFieldAttribute.SetEnabled<CopyQuoteFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalculateDiscounts == true);
		}
        #endregion

        #region QuoteFilter
        protected virtual void RecalcDiscountsParamFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            RecalcDiscountsParamFilter row = e.Row as RecalcDiscountsParamFilter;
            if (row == null) return;

            if (!(row.RecalcUnitPrices == true))
            {
                CopyQuoteInfo.Cache.SetValue<RecalcDiscountsParamFilter.overrideManualPrices>(row, false);
            }
            if (!(row.RecalcDiscounts == true))
            {
                CopyQuoteInfo.Cache.SetValue<RecalcDiscountsParamFilter.overrideManualDiscounts>(row, false);
				CopyQuoteInfo.Cache.SetValue<RecalcDiscountsParamFilter.overrideManualDocGroupDiscounts>(row, false);
			}
        }

        protected virtual void RecalcDiscountsParamFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            RecalcDiscountsParamFilter row = e.Row as RecalcDiscountsParamFilter;
            if (row == null) return;

            PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualPrices>(sender, row, row.RecalcUnitPrices == true);
            PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualDiscounts>(sender, row, row.RecalcDiscounts == true);
			PXUIFieldAttribute.SetEnabled<RecalcDiscountsParamFilter.overrideManualDocGroupDiscounts>(sender, row, row.RecalcDiscounts == true);
		}
        #endregion


        #region CRQuote        

        [CRMBAccount(BqlField = typeof(Standalone.CROpportunityRevision.bAccountID), Enabled = false)]
        [CustomerAndProspectRestrictor]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<CRQuote.bAccountID> e) { }

        protected virtual void CRQuote_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = e.Row as CRQuote;
            if (row == null) return;

            var customerLocation = (Location)PXSelect<Location,
                    Where<Location.bAccountID, Equal<Required<CRQuote.bAccountID>>,
                        And<Location.locationID, Equal<Required<CRQuote.locationID>>>>>.
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
                    InnerJoin<Branch, On<Branch.branchID, Equal<Current<CRQuote.branchID>>>,
                        InnerJoin<BAccount, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>,
                    Where<Location.locationID, Equal<BAccount.defLocationID>>>.Select(this);
                if (branchLocation != null && branchLocation.VTaxZoneID != null)
                    e.NewValue = branchLocation.VTaxZoneID;
                else
                    e.NewValue = row.TaxZoneID;	            
            }
	        if (sender.GetStatus(e.Row) != PXEntryStatus.Notchanged)
		        e.Cancel = true;
        }

		protected virtual void CRQuote_QuoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var quote = e.Row as CRQuote;
			if (quote == null) return;

			object noteID = quote.NoteID;
			if (noteID == null)
			{
				sender.RaiseFieldDefaulting<CRQuote.noteID>(quote, out noteID);
			}
			e.NewValue = noteID;
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<CRQuote.quoteType, Equal<CRQuoteTypeAttribute.distribution>>), Messages.OnlyDistributionQuotesAvailable)]
		protected virtual void CRQuote_QuoteNbr_CacheAttached(PXCache sender) { }

		#region CROpportunity
		protected virtual void CRQuote_OpportunityID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = e.Row as CRQuote;
            if (row == null) return;            
            Opportunity.Cache.Current = Opportunity.SelectSingle(row.OpportunityID);
	        if (Opportunity.Cache.Current != null)
	        {
		        row.Subject = ((Standalone.CROpportunity) Opportunity.Cache.Current).Subject;
				if (IsSingleQuote(row.OpportunityID))
				{
					var opportunity = CurrentOpportunity.SelectSingle(row.OpportunityID);
					if (opportunity != null)
					{
						row.QuoteID = opportunity.QuoteNoteID;
						row.IsPrimary = true;
					}
				}
			}
        }
        #endregion

        protected virtual void CRQuote_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {            
            PXNoteAttribute.SetTextFilesActivitiesRequired<CROpportunityProducts.noteID>(Products.Cache, null);
            
			CRQuote row = e.Row as CRQuote;
            if (row == null) return;

            PXUIFieldAttribute.SetEnabled<CRQuote.allowOverrideContactAddress>(cache, row, !(row.BAccountID == null && row.ContactID == null));
            Caches[typeof(CRContact)].AllowUpdate = row.AllowOverrideContactAddress == true;
            Caches[typeof(CRAddress)].AllowUpdate = row.AllowOverrideContactAddress == true;

            PXUIFieldAttribute.SetEnabled<CRQuote.bAccountID>(cache, row, false);            
            PXUIFieldAttribute.SetEnabled<CRQuote.curyAmount>(cache, row, row.ManualTotalEntry == true);
			PXUIFieldAttribute.SetEnabled<CRQuote.curyDiscTot>(cache, row, row.ManualTotalEntry == true);

			PXUIFieldAttribute.SetEnabled<CRQuote.locationID>(cache, row, row.BAccountID != null);
            PXDefaultAttribute.SetPersistingCheck<CRQuote.locationID>(cache, row, row.BAccountID == null ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

            PXUIFieldAttribute.SetEnabled<CRQuote.branchID>(cache, row, false);
            PXUIFieldAttribute.SetEnabled<CRQuote.projectID>(cache, row, false);
            
            PXDefaultAttribute.SetPersistingCheck<CRQuote.branchID>(cache, row, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<CRQuote.projectID>(cache, row, PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetVisible<CRQuote.curyID>(cache, row, IsMultyCurrency);
            if (row.ManualTotalEntry == true && row.CuryTaxTotal > 0)
            {
                cache.RaiseExceptionHandling<CRQuote.curyTaxTotal>(row, row.CuryTaxTotal,
                    new PXSetPropertyException(Messages.TaxAmountExcluded, PXErrorLevel.Warning));
            }
            else
            {
                cache.RaiseExceptionHandling<CRQuote.curyTaxTotal>(row, row.CuryTaxTotal, null);
            }

            Actions[nameof(PrimaryQuote)].SetEnabled(!String.IsNullOrEmpty(row.OpportunityID) && row.IsPrimary != true);
			var line = String.Format(Messages.QuoteGridProductText,
				this.Quote.Cache.GetValueExt<CRQuote.curyExtPriceTotal>(row),
				this.Quote.Cache.GetValueExt<CRQuote.curyLineDiscountTotal>(row));
			foreach (CROpportunityProducts product in this.Products.Select())
			{
				product.TextForProductsGrid = line;
				 PXEntryStatus oldstatus = this.Products.Cache.GetStatus(product);
                this.Products.Cache.SetStatus(product, PXEntryStatus.Updated);
                this.Products.Cache.SetStatus(product, oldstatus);
            }

			Approval.AllowSelect = row.IsSetupApprovalRequired.GetValueOrDefault();

			if (row.OpportunityIsActive == false)
			{
				cache.RaiseExceptionHandling<CRQuote.opportunityID>(row, row.OpportunityID, 
					new PXSetPropertyException(Messages.OpportunityIsNotActive, PXErrorLevel.Warning));
			}

			if (!UnattendedMode)
			{
				CRShippingAddress shipAddress = this.Shipping_Address.Select();
				CRAddress contactAddress = this.Quote_Address.Select();
				bool enableAddressValidation = ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
												|| (contactAddress != null && (contactAddress.IsDefaultAddress == false || row.BAccountID == null && row.ContactID == null) && contactAddress.IsValidated == false));
				this.validateAddresses.SetEnabled(enableAddressValidation);
			}
		}


        protected virtual void CRQuote_BAccountID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            CRQuote row = e.Row as CRQuote;
            if (row == null) return;

            if (row.BAccountID < 0)
                e.ReturnValue = "";
        }        
                
		protected virtual void CRQuote_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            var row = e.Row as CRQuote;
            if (row == null) return;

			row.NoteID = row.QuoteID;

            object newContactId = row.ContactID;
            if (newContactId != null && !VerifyField<CRQuote.contactID>(row, newContactId))
                row.ContactID = null;

            if (row.ContactID != null)
            {
                object newCustomerId = row.BAccountID;
                if (newCustomerId == null)
                    FillDefaultBAccountID(row);
            }

            object newLocationId = row.LocationID;
            if (newLocationId == null || !VerifyField<CRQuote.locationID>(row, newLocationId))
            {
                cache.SetDefaultExt<CRQuote.locationID>(row);
            }

            if (row.ContactID == null)
                cache.SetDefaultExt<CRQuote.contactID>(row);

            if (row.TaxZoneID == null)
                cache.SetDefaultExt<CRQuote.taxZoneID>(row);

            foreach (var product in Products.Select().RowCast<CROpportunityProducts>())                
            {
                Products.Cache.Update(product);
            }

            if (IsFirstQuote(row.OpportunityID))
            {
                CROpportunityRevision firstrevision = PXSelect<CROpportunityRevision,
                    Where<CROpportunityRevision.opportunityID, Equal<Required<CROpportunityRevision.opportunityID>>>>.SelectSingleBound(this, null, new object[] { row.OpportunityID });

				if (firstrevision != null)
				{
					cache.SetValueExt(row, typeof(CRQuote.curyInfoID).Name, firstrevision.CuryInfoID);
					var Discount = this.GetExtension<QuoteMaint.Discount>();
					Discount.RefreshTotalsAndFreeItems(Discount.Details.Cache);
				}
            }
        }


        protected virtual void CRQuote_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var oldRow = e.OldRow as CRQuote;
            var row = e.Row as CRQuote;
            if (oldRow == null || row == null) return;

            if (row.ContactID != null && row.ContactID != oldRow.ContactID)
            {
                object newCustomerId = row.BAccountID;
                if (newCustomerId == null)
                    FillDefaultBAccountID(row);
            }

            var customerChanged = row.BAccountID != oldRow.BAccountID;
            object newLocationId = row.LocationID;           
            if (customerChanged && !VerifyField<CRQuote.locationID>(row, newLocationId))
            {
                sender.SetDefaultExt<CRQuote.locationID>(row);
            }

            if (customerChanged)
                sender.SetDefaultExt<CRQuote.taxZoneID>(row);

            var locationChanged = row.LocationID != oldRow.LocationID;
            var docDateChanged = row.DocumentDate != oldRow.DocumentDate;
            var projectChanged = row.ProjectID != oldRow.ProjectID;
            if (locationChanged || docDateChanged || projectChanged || customerChanged)
            {
                var productsCache = Products.Cache;
                foreach (CROpportunityProducts line in SelectProducts(row.QuoteID))
                {
                    var lineCopy = (CROpportunityProducts)productsCache.CreateCopy(line);
                    lineCopy.ProjectID = row.ProjectID;
                    lineCopy.CustomerID = row.BAccountID;
                    productsCache.Update(lineCopy);
                }
                sender.SetDefaultExt<CRQuote.taxCalcMode>(row);
            }

            foreach (CROpportunityProducts product in this.Products.Select())
            {
                product.TextForProductsGrid = String.Format(Messages.QuoteGridProductText, row.CuryExtPriceTotal.ToString(), row.CuryLineDiscountTotal.ToString());
                PXEntryStatus oldstatus = this.Products.Cache.GetStatus(product);
                this.Products.Cache.SetStatus(product, PXEntryStatus.Updated);
                this.Products.Cache.SetStatus(product, oldstatus);
            }
        }

        protected virtual void CRQuote_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            CRQuote row = e.Row as CRQuote;
            if (row == null) return;
            
            bool IsPrimary = (bool)sender.GetValue<CRQuote.isPrimary>(row);
            if (IsPrimary && !IsSingleQuote(row.OpportunityID))
            {
                throw new PXException(ErrorMessages.PrimaryQuote);
            }
        }

        protected virtual void CRQuote_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (CRQuote)e.Row;
            if (row == null) return;

            if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.BAccountID != null && row.LocationID == null)
            {
                sender.RaiseExceptionHandling<CRQuote.locationID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
            }
        }

        protected virtual void CRQuote_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            CRQuote row = e.Row as CRQuote;
            if (row == null) return;

	        if (e.Operation == PXDBOperation.Insert && this.Opportunity.Current != null && e.TranStatus == PXTranStatus.Open)
	        {
		        //Persist counter manually, removed from Views.Caches by multicurrenry.
		        PXDatabase.Update<Standalone.CROpportunity>(
			        new PXDataFieldAssign<Standalone.CROpportunity.defQuoteID>(row.IsPrimary == true ? row.QuoteID : this.Opportunity.Current.DefQuoteID),
					new PXDataFieldRestrict<Standalone.CROpportunity.opportunityID>(PXDbType.VarChar, 255, this.Opportunity.Current.OpportunityID, PXComp.EQ)
		        );
	        }
        }

	    protected void SuppressCascadeDeletion(PXView view, object row)
	    {
		    PXCache cache = this.Caches[row.GetType()];
		    foreach (object rec in view.Cache.Deleted)
		    {
			    if (view.Cache.GetStatus(rec) == PXEntryStatus.Deleted)
			    {					
				    bool own = true;
				    foreach (string key in new[]{typeof(CROpportunity.quoteNoteID).Name})
				    {
					    if (!object.Equals(cache.GetValue(row, key), view.Cache.GetValue(rec, key)))
					    {
						    own = false;
						    break;
					    }
				    }
					if(own)
						view.Cache.SetStatus(rec, PXEntryStatus.Notchanged);
			    }
		    }
	    }
	    protected virtual void CROpportunityRevision_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
	    {
		    //Suppress update revision for quote, should be done by main DAC
		    CROpportunityRevision row = (CROpportunityRevision)e.Row;
		    if (row != null && this.Quote.Current != null &&
		        row.NoteID == this.Quote.Current.QuoteID)
			    e.Cancel = true;
	    }
		#endregion

		#region CROpportunityProducts
	    
	    [PXDBLong]
	    [CurrencyInfo(typeof(CRQuote.curyInfoID))]
	    protected virtual void CROpportunityProducts_CuryInfoID_CacheAttached(PXCache e)
	    {
	    }

		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRQuote.quoteID))]
		[PXParent(typeof(Select<CRQuote,
			 Where<CRQuote.quoteID, Equal<Current<CROpportunityProducts.quoteID>>>>))]
		protected virtual void CROpportunityProducts_QuoteID_CacheAttached(PXCache e)
		{
        }

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts, CROpportunityProducts.vendorID> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.POCreate == false || e.Row.InventoryID == null)
				e.Cancel = true;
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.pOCreate> e)
		{
			if (e.Row == null)
				return;

			e.Cache.SetDefaultExt<CROpportunityProducts.vendorID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<CROpportunityProducts, CROpportunityProducts.inventoryID> e)
		{
			if (e.Row == null)
				return;

			e.Cache.SetValueExt<CROpportunityProducts.pOCreate>(e.Row, false);
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts.projectID> e)
		{
			e.NewValue = QuoteCurrent.Current.ProjectID;
		}

		protected virtual void _(Events.FieldDefaulting<CROpportunityProducts.customerID> e)
		{
			e.NewValue = QuoteCurrent.Current.BAccountID;
		}

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(CRQuote.productCntr))]
        protected virtual void CROpportunityProducts_LineNbr_CacheAttached(PXCache e)
        {           
        }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price", Visible = false)]
        protected virtual void CROpportunityProducts_ManualPrice_CacheAttached(PXCache e)
        {
        }

		[PXDBInt]
		[PXDBDefault(typeof(CRQuote.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void CROpportunityProducts_CustomerID_CacheAttached(PXCache e)
		{
		}

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
        }

        [PopupMessage]
        [PXRestrictor(typeof(Where<
            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
            And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>>>), IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<CROpportunityProducts.inventoryID> e) { }

        protected virtual void CROpportunityProducts_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CROpportunityProducts row = e.Row as CROpportunityProducts;
            if (row == null) return;

            if (row.InventoryID != null && row.IsFree == false)
            {
                Caches[typeof(CROpportunityProducts)].SetDefaultExt<CROpportunityProducts.curyUnitPrice>(row);
            }
        }
        #endregion

        #region CROpportunityDiscountDetail    
        public PXSelect<CROpportunityDiscountDetail,
				Where<CROpportunityDiscountDetail.quoteID, Equal<Current<CRQuote.quoteID>>>,
				OrderBy<Asc<CROpportunityDiscountDetail.lineNbr>>>
			_DiscountDetails;

		[PXDBGuid(IsKey = true)]
	    [PXDBDefault(typeof(CRQuote.quoteID))]
		protected virtual void CROpportunityDiscountDetail_QuoteID_CacheAttached(PXCache sender)
		{
	    }

		[PXDBUShort()]
		[PXLineNbr(typeof(CRQuote))]
		protected virtual void CROpportunityDiscountDetail_LineNbr_CacheAttached(PXCache e)
		{
		}

		#endregion

		#region CROpportunityTax
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRQuote.quoteID))]
		[PXParent(typeof(Select<CRQuote,
			Where<CRQuote.quoteID, Equal<Current<CROpportunityTax.quoteID>>>>))]
		protected virtual void CROpportunityTax_QuoteID_CacheAttached(PXCache sender)
		{
        }

	    [PXDBLong]
	    [CurrencyInfo(typeof(CRQuote.curyInfoID))]
	    protected virtual void CROpportunityTax_CuryInfoID_CacheAttached(PXCache e)
	    {
	    }


		#endregion

		#region CRTaxTran
		[PXDBGuid(IsKey = true)]
		[PXDBDefault(typeof(CRQuote.quoteID))]
		protected virtual void CRTaxTran_QuoteID_CacheAttached(PXCache sender)
		{
        }

	    [PXDBLong]
	    [CurrencyInfo(typeof(CRQuote.curyInfoID))]
	    protected virtual void CRTaxTran_CuryInfoID_CacheAttached(PXCache e)
	    {
	    }

		#endregion

		#region BAccountR

		[PXBool]
        [PXDefault(false)]
        [PXDBCalced(typeof(True), typeof(Boolean))]
        protected virtual void BAccountR_ViewInCrm_CacheAttached(PXCache sender)
        {
        }

        #endregion

        #region Private Methods

        private BAccount SelectAccount(string acctCD)
        {
            if (string.IsNullOrEmpty(acctCD)) return null;
            return (BAccount)PXSelectReadonly<BAccount,
                    Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.
                Select(this, acctCD);
        }

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

        private void FillDefaultBAccountID(CRQuote row)
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
                }
            }

        }

        private bool IsMultyCurrency
        {
            get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
        }

        private bool ProrateDiscount
        {
            get
            {
                SOSetup sosetup = PXSelect<SOSetup>.Select(this);

                if (sosetup == null)
                    return true; //default true

                if (sosetup.ProrateDiscounts == null)
                    return true;

                return sosetup.ProrateDiscounts == true;
            }
        }

        private bool IsSingleQuote(string opportunityId)
        {
            var quote = PXSelect<CRQuote, Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>>>.SelectSingleBound(this, null, opportunityId);
            return (quote.Count == 0);
        }

        private bool IsFirstQuote(string opportunityId)
        {
            var quote = PXSelectReadonly<CRQuote, Where<CRQuote.opportunityID, Equal<Required<CRQuote.opportunityID>>>>.SelectSingleBound(this, null, opportunityId);
            return (quote.Count == 0);
        }

        private CRQuote SelectSingleQuote(string opportunityId)
        {
            if (opportunityId == null) return null;

            var opportunity = (CRQuote)PXSelect<CRQuote,
                    Where<CRQuote.opportunityID, Equal<Required<CRQuote.opportunityID>>>>.
                Select(this, opportunityId);
            return opportunity;
        }

        private IEnumerable SelectProducts(object quoteId)
        {
            if (quoteId == null)
                return new CROpportunityProducts[0];

            return PXSelect<CROpportunityProducts,
                    Where<CROpportunityProducts.quoteID, Equal<Required<CRQuote.quoteID>>>>.
                Select(this, quoteId).
                RowCast<CROpportunityProducts>();
        }

        private IEnumerable SelectDiscountDetails(object quoteId)
        {
            if (quoteId == null)
                return new CROpportunityDiscountDetail[0];

            return PXSelect<CROpportunityDiscountDetail,
                    Where<CROpportunityDiscountDetail.quoteID, Equal<Required<CRQuote.quoteID>>>>.
                Select(this, quoteId).
                RowCast<CROpportunityDiscountDetail>();
        }


        private Contact FillFromOpportunityContact(Contact Contact)
        {
            CRContact _CRContact = Quote_Contact.SelectSingle();

            Contact.FullName = _CRContact.FullName;
            Contact.Title = _CRContact.Title;
            Contact.FirstName = _CRContact.FirstName;
            Contact.LastName = _CRContact.LastName;
            Contact.Salutation = _CRContact.Salutation;
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
            CRAddress _CRAddress = Quote_Address.SelectSingle();

            Address.AddressLine1 = _CRAddress.AddressLine1;
            Address.AddressLine2 = _CRAddress.AddressLine2;
            Address.City = _CRAddress.City;
            Address.CountryID = _CRAddress.CountryID;
            Address.State = _CRAddress.State;
            Address.PostalCode = _CRAddress.PostalCode;
            return Address;
        }

        private bool IsDefaultContactAdress()
        {
            CRAddress _CRAddress = Quote_Address.SelectSingle();
            CRContact _CRContact = Quote_Contact.SelectSingle();

            if (_CRContact != null && _CRAddress != null)
            {
                bool IsDirtya = Quote_Address.Cache.IsDirty;
                bool IsDirtyc = Quote_Contact.Cache.IsDirty;

                CRAddress _etalonCRAddress = Quote_Address.Insert();
                CRContact _etalonCRContact = Quote_Contact.Insert();

                Quote_Address.Cache.SetStatus(_etalonCRAddress, PXEntryStatus.Held);
                Quote_Contact.Cache.SetStatus(_etalonCRContact, PXEntryStatus.Held);

                Quote_Address.Cache.IsDirty = IsDirtya;
                Quote_Contact.Cache.IsDirty = IsDirtyc;

                if (_CRContact.FullName != _etalonCRContact.FullName)
                    return false;
                if (_CRContact.Title != _etalonCRContact.Title)
                    return false;
                if (_CRContact.FirstName != _etalonCRContact.FirstName)
                    return false;
                if (_CRContact.LastName != _etalonCRContact.LastName)
                    return false;
                if (_CRContact.Salutation != _etalonCRContact.Salutation)
                    return false;
                if (_CRContact.Email != _etalonCRContact.Email)
                    return false;
                if (_CRContact.Phone1 != _etalonCRContact.Phone1)
                    return false;
                if (_CRContact.Phone1Type != _etalonCRContact.Phone1Type)
                    return false;
                if (_CRContact.Phone2 != _etalonCRContact.Phone2)
                    return false;
                if (_CRContact.Phone2Type != _etalonCRContact.Phone2Type)
                    return false;
                if (_CRContact.Phone3 != _etalonCRContact.Phone3)
                    return false;
                if (_CRContact.Phone3Type != _etalonCRContact.Phone3Type)
                    return false;
                if (_CRContact.Fax != _etalonCRContact.Fax)
                    return false;
                if (_CRContact.FaxType != _etalonCRContact.FaxType)
                    return false;

                if (_CRAddress.AddressLine1 != _etalonCRAddress.AddressLine1)
                    return false;
                if (_CRAddress.AddressLine2 != _CRAddress.AddressLine2)
                    return false;
                if (_CRAddress.City != _CRAddress.City)
                    return false;
                if (_CRAddress.State != _CRAddress.State)
                    return false;
                if (_CRAddress.CountryID != _CRAddress.CountryID)
                    return false;
                if (_CRAddress.PostalCode != _CRAddress.PostalCode)
                    return false;
            }
            return true;
        }

        private bool IsContactAddressNoChanged(Contact _etalonCRContact, Address _etalonCRAddress)
        {
            if (_etalonCRContact == null || _etalonCRAddress == null)
            {
                return false;
            }

            CRAddress _CRAddress = Quote_Address.SelectSingle();
            CRContact _CRContact = Quote_Contact.SelectSingle();

            if (_CRContact != null && _CRAddress != null)
            {
                if (_CRContact.FullName != _etalonCRContact.FullName)
                    return false;
                if (_CRContact.Title != _etalonCRContact.Title)
                    return false;
                if (_CRContact.LastName != _etalonCRContact.LastName)
                    return false;
                if (_CRContact.FirstName != _etalonCRContact.FirstName)
                    return false;
                if (_CRContact.Salutation != _etalonCRContact.Salutation)
                    return false;
                if (_CRContact.Email != _etalonCRContact.EMail)
                    return false;
                if (_CRContact.Phone1 != _etalonCRContact.Phone1)
                    return false;
                if (_CRContact.Phone1Type != _etalonCRContact.Phone1Type)
                    return false;
                if (_CRContact.Phone2 != _etalonCRContact.Phone2)
                    return false;
                if (_CRContact.Phone2Type != _etalonCRContact.Phone2Type)
                    return false;
                if (_CRContact.Phone3 != _etalonCRContact.Phone3)
                    return false;
                if (_CRContact.Phone3Type != _etalonCRContact.Phone3Type)
                    return false;
                if (_CRContact.Fax != _etalonCRContact.Fax)
                    return false;
                if (_CRContact.FaxType != _etalonCRContact.FaxType)
                    return false;

                if (_CRAddress.AddressLine1 != _etalonCRAddress.AddressLine1)
                    return false;
                if (_CRAddress.AddressLine2 != _etalonCRAddress.AddressLine2)
                    return false;
                if (_CRAddress.City != _etalonCRAddress.City)
                    return false;
                if (_CRAddress.State != _etalonCRAddress.State)
                    return false;
                if (_CRAddress.CountryID != _etalonCRAddress.CountryID)
                    return false;
                if (_CRAddress.PostalCode != _etalonCRAddress.PostalCode)
                    return false;
            }
            else
            {
                return false;
            }
            return true;
        }
		#endregion

		#region Avalara Tax

		public virtual bool IsExternalTax(string taxZoneID)
	    {
		    return false;
	    }

	    public virtual CRQuote CalculateExternalTax(CRQuote quote)
	    {
		    return quote;
	    }

		#endregion


		public override void Persist()
        {
	        foreach (CRQuote quote in this.Quote.Cache.Deleted)
	        {
		        if (IsSingleQuote(quote.OpportunityID))
		        {
					//Suppress cascace deleting
			        SuppressCascadeDeletion(this.Products.View, quote);
			        SuppressCascadeDeletion(this.Taxes.View, quote);
			        SuppressCascadeDeletion(this.TaxLines.View, quote);
			        SuppressCascadeDeletion(this._DiscountDetails.View, quote);
				}
	        }

	        base.Persist();
		}

		#region Implementation of IPXPrepareItems

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            if (string.Compare(viewName, "Products", true) == 0)
            {
                if (values.Contains("opportunityID"))
                    values["opportunityID"] = Quote.Current.OpportunityID;
                else
                    values.Add("opportunityID", Quote.Current.OpportunityID);
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

        //*
        #region Extensions
        public class MultiCurrency : CRMultiCurrencyGraph<QuoteMaint, CRQuote>
        {
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CRQuote)) { DocumentDate = typeof(CRQuote.documentDate) };
            }

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Quote,
					Base.Products,
					Base.TaxLines,
					Base.Taxes
				};
			}
		}

        public class SalesPrice : SalesPriceGraph<QuoteMaint, CRQuote>
        {
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CRQuote)) { CuryInfoID = typeof(CRQuote.curyInfoID) };
            }
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Descr = typeof(CROpportunityProducts.descr) };
            }
            protected override PriceClassSourceMapping GetPriceClassSourceMapping()
            {
                return new PriceClassSourceMapping(typeof(Location)) { PriceClassID = typeof(Location.cPriceClassID) };
            }            
        }

        public class Discount : DiscountGraph<QuoteMaint, CRQuote>
        {
            public override void Initialize()
            {
                base.Initialize();
                if (this.Discounts == null)
                    this.Discounts = new PXSelectExtension<PX.Objects.Extensions.Discount.Discount>(this.DiscountDetails);

                Base.actionsFolder.AddMenuAction(graphRecalculateDiscountsAction);
            }
            protected override DocumentMapping GetDocumentMapping()
            {
				return new DocumentMapping(typeof(CRQuote)) { CuryDiscTot = typeof(CRQuote.curyLineDocDiscountTotal) };
			}
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Quantity = typeof(CROpportunityProducts.quantity) };
            }
            protected override DiscountMapping GetDiscountMapping()
            {
                return new DiscountMapping(typeof(CROpportunityDiscountDetail));
            }

            [PXCopyPasteHiddenView()]
            [PXViewName(Messages.DiscountDetails)]
            public PXSelect<CROpportunityDiscountDetail,
                    Where<CROpportunityDiscountDetail.quoteID, Equal<Current<CRQuote.quoteID>>>,
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
            [CurrencyInfo(typeof(CRQuote.curyInfoID))]            
			public override void Discount_CuryInfoID_CacheAttached(PXCache sender)
            {
            }

            protected override bool AddDocumentDiscount => true;

            protected override void DefaultDiscountAccountAndSubAccount(PX.Objects.Extensions.Discount.Detail det)
            {
            }            

            public PXAction<CRQuote> graphRecalculateDiscountsAction;
            [PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
            [PXButton]
            public virtual IEnumerable GraphRecalculateDiscountsAction(PXAdapter adapter)
            {
                List<CRQuote> CRQoutes = new List<CRQuote>(adapter.Get().Cast<CRQuote>());
                foreach (CRQuote quote in CRQoutes)
                {
                    if (recalcdiscountsfilter.View.Answer == WebDialogResult.None)
                    {
                        recalcdiscountsfilter.Cache.Clear();
                        RecalcDiscountsParamFilter filterdata = recalcdiscountsfilter.Cache.Insert() as RecalcDiscountsParamFilter;
                        filterdata.RecalcUnitPrices = false;
                        filterdata.RecalcDiscounts = false;
                        filterdata.OverrideManualPrices = false;
                        filterdata.OverrideManualDiscounts = false;
						filterdata.OverrideManualDocGroupDiscounts = false;
					}

                    if (recalcdiscountsfilter.AskExt() != WebDialogResult.OK)
                        return CRQoutes;

                    RecalculateDiscountsAction(adapter);                    
                }
                return CRQoutes;              
            }
        }

        public class SalesTax : TaxGraph<QuoteMaint, CRQuote>
        {
            protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }

            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(CRQuote))
                {
                    DocumentDate = typeof(CRQuote.documentDate),
                    CuryDocBal = typeof(CRQuote.curyProductsAmount),
                    CuryDiscountLineTotal = typeof(CRQuote.curyLineDiscountTotal),
					CuryDiscTot = typeof(CRQuote.curyLineDocDiscountTotal),
					TaxCalcMode = typeof(CRQuote.taxCalcMode)
				};
            }
            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(CROpportunityProducts)) { CuryTranAmt = typeof(CROpportunityProducts.curyAmount), CuryTranDiscount = typeof(CROpportunityProducts.curyDiscAmt), CuryTranExtPrice = typeof(CROpportunityProducts.curyExtPrice), DocumentDiscountRate = typeof(CROpportunityProducts.documentDiscountRate), GroupDiscountRate = typeof(CROpportunityProducts.groupDiscountRate) };
            }

            protected override TaxDetailMapping GetTaxDetailMapping()
            {
                return new TaxDetailMapping(typeof(CROpportunityTax), typeof(CROpportunityTax.taxID));
            }
            protected override TaxTotalMapping GetTaxTotalMapping()
            {
                return new TaxTotalMapping(typeof(CRTaxTran), typeof(CRTaxTran.taxID));
            }

            protected virtual void _(Events.FieldUpdated<CRQuote, CRQuote.curyAmount> e)
            {
                if (e.Row != null && e.Row.ManualTotalEntry == true)
                    e.Row.CuryProductsAmount = e.Row.CuryAmount - e.Row.CuryDiscTot;
            }

            protected virtual void _(Events.FieldUpdated<CRQuote, CRQuote.curyDiscTot> e)
            {
                if (e.Row != null && e.Row.ManualTotalEntry == true)
                    e.Row.CuryProductsAmount = e.Row.CuryAmount - e.Row.CuryDiscTot;
            }

            protected virtual void _(Events.FieldUpdated<CRQuote, CRQuote.manualTotalEntry> e)
            {
                if (e.Row != null && e.Row.ManualTotalEntry == false)
                {
                    CalcTotals(null, false);
                }
            }
            protected virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                var row = sender.GetExtension<PX.Objects.Extensions.SalesTax.Document>(e.Row);
                if (row != null && row.TaxCalc == null)
                    row.TaxCalc = TaxCalc.Calc;
            }
            protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
            {
                base.CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);


                CRQuote doc = (CRQuote)this.Documents.Cache.GetMain<PX.Objects.Extensions.SalesTax.Document>(this.Documents.Current);
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

                if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<CRQuote.curyProductsAmount>() ?? 0m)) == false)
                {
                    ParentSetValue<CRQuote.curyProductsAmount>(CuryDocTotal);
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
				IComparer<Tax> taxComparer = GetTaxByCalculationLevelComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

				Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();                
	            var currents = new[]
                {
                    row != null && row is PX.Objects.Extensions.SalesTax.Detail ? Details.Cache.GetMain((PX.Objects.Extensions.SalesTax.Detail)row):null,
					((QuoteMaint)graph).Quote.Current
                };                

                foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                        LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
                            And<TaxRev.outdated, Equal<boolFalse>,
                                And<TaxRev.taxType, Equal<TaxType.sales>,
                                    And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                        And<Tax.taxType, NotEqual<CSTaxType.use>,
                                            And<Tax.reverseTax, Equal<boolFalse>,
                                                And<Current<CRQuote.documentDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
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
                                Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
                                        And <CROpportunityTax.quoteID, Equal<Current<CROpportunityProducts.quoteID>>,
                                            And <CROpportunityTax.lineNbr, Equal<Current<CROpportunityProducts.lineNbr>>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && taxComparer.Compare((PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
                                    idx--) ;

                                Tax adjdTax = AdjustTaxLevel((Tax)line);
                                ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcLine:
                        foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                                Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
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
                                Where<CRTaxTran.quoteID, Equal<Current<CRQuote.quoteID>>>,
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
                            Where<CROpportunityProducts.quoteID, Equal<Current<CRQuote.quoteID>>>>
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

            protected virtual void CROpportunityTax_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
            {
                CROpportunityTax row = e.Row as CROpportunityTax;
                if (row == null) return;
            }


            protected virtual void CRTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
            {
                CRTaxTran row = e.Row as CRTaxTran;
                if (row == null) return;

                if (e.Operation == PXDBOperation.Delete)
                {
                    CROpportunityTax tax = (CROpportunityTax)Base.TaxLines.Cache.Locate(OpportunityMaint.SalesTax.FindCROpportunityTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Deleted ||
                        Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.InsertedDeleted)
                        e.Cancel = true;
                }
                if (e.Operation == PXDBOperation.Update)
                {
                    CROpportunityTax tax = (CROpportunityTax)Base.TaxLines.Cache.Locate(OpportunityMaint.SalesTax.FindCROpportunityTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Updated)
                        e.Cancel = true;
                }
            }
            #endregion
        }

		public class ContactAddress : ContactAddressGraph<QuoteMaint>
		{
			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRQuote))
				{
					DocumentAddressID = typeof(CRQuote.opportunityAddressID),
					DocumentContactID = typeof(CRQuote.opportunityContactID),
					ShipAddressID = typeof(CRQuote.shipAddressID),
					ShipContactID = typeof(CRQuote.shipContactID)
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
		        return Base.Quote_Contact.Cache;
		    }
		    protected override PXCache GetAddressCache()
		    {
		        return Base.Quote_Address.Cache;
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
				var contact = Base.Quote_Contact.SelectSingle();
				return contact;
			}
			protected override IPersonalContact GetEtalonContact()
			{
				bool isDirty = Base.Quote_Contact.Cache.IsDirty;
				var contact = Base.Quote_Contact.Insert();
				Base.Quote_Contact.Cache.SetStatus(contact, PXEntryStatus.Held);
				Base.Quote_Contact.Cache.IsDirty = isDirty;
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
				var address = Base.Quote_Address.SelectSingle();
				return address;
			}
			protected override IAddress GetEtalonAddress()
			{
				bool isDirty = Base.Quote_Address.Cache.IsDirty;
				var address = Base.Quote_Address.Insert();
				Base.Quote_Address.Cache.SetStatus(address, PXEntryStatus.Held);
				Base.Quote_Address.Cache.IsDirty = isDirty;
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

		}

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
        #endregion
        //*/

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
    }
}

