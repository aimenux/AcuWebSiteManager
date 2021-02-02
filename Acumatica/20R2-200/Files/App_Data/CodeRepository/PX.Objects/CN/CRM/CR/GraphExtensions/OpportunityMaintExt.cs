using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.CRM.CR.CacheExtensions;
using PX.Objects.CN.CRM.CR.DAC;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CR.Workflows;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CN.CRM.CR.GraphExtensions
{
    public class OpportunityMaintExt : PXGraphExtension<OpportunityMaint>
    {
        [PXViewDetailsButton(typeof(Contact), typeof(Select<Contact,
            Where<Contact.contactID, Equal<MultipleQuote.contactID>>>))]
        [PXViewDetailsButton(typeof(BAccount), typeof(Select<BAccount,
            Where<BAccount.bAccountID, Equal<Current<MultipleQuote.businessAccountID>>>>))]
        public PXSelect<MultipleQuote,
            Where<MultipleQuote.opportunityID, Equal<Current<CROpportunity.opportunityID>>,
                Or<MultipleQuote.opportunityID, IsNull>>> MultipleQuotes;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIEnabled(typeof(CrOpportunityExt.multipleAccounts.IsEqual<False>))]
        protected virtual void CROpportunity_BAccountID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIEnabled(typeof(CrOpportunityExt.multipleAccounts.IsEqual<False>))]
        protected virtual void CROpportunity_ContactID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.RowSelected<CROpportunity> args)
        {
            ValidateQuoteContacts();
            var opportunity = args.Row;
            if (opportunity == null)
            {
                return;
            }
            SetMultipleQuotesTabAvailability(opportunity);
            SetQuoteRelatedFieldsEnabled(args.Cache, opportunity);
            SetOpportunityAmountSource(opportunity);

            PXUIFieldAttribute.SetVisible<CROpportunity.curyAmount>(args.Cache, args.Row, false);
            PXUIFieldAttribute.SetVisible<CROpportunity.curyProductsAmount>(args.Cache, args.Row, false);
        }

        protected virtual void _(Events.RowInserting<CROpportunity> args)
        {
            var opportunity = args.Row;
            if (opportunity != null)
            {
                var opportunityExtension = opportunity.GetExtension<CrOpportunityExt>();
                if (opportunityExtension != null)
                {
                    opportunityExtension.Cost = 0;
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<CROpportunity, CrOpportunityExt.multipleAccounts> args)
        {
            var opportunity = args.Row;
            if (opportunity == null)
            {
                return;
            }
            var opportunityExtension = opportunity.GetExtension<CrOpportunityExt>();
            var isMultipleAccountsChecked = opportunityExtension.MultipleAccounts == true;
            bool oldValue = (bool)args.OldValue == true;
            if (oldValue == isMultipleAccountsChecked) return; // to suppress cleaning the opportunity when OldValue is null and new value is false

            if (isMultipleAccountsChecked)
            {
                args.Cache.SetValue<CROpportunity.manualTotalEntry>(opportunity, true);
                opportunity.CuryDiscTot = 0m;
                if (DoesOpportunityContainQuoteRelatedInfo(opportunityExtension))
                {
                    AddNewQuote(opportunity, opportunityExtension);
                }
                PopulateOpportunityBasedOnSelectedQuote(opportunity, opportunityExtension);
            }
            else
            {
                ClearOpportunity(opportunity, opportunityExtension);
                var selectedQuote = MultipleQuotes.Search<MultipleQuote.isSelected>(true).FirstTableItems
                    .SingleOrDefault();
                if (selectedQuote != null)
                {
                    selectedQuote.IsSelected = false;
                    MultipleQuotes.Update(selectedQuote);
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<CROpportunity, CrOpportunityExt.quotedAmount> args)
        {
            var opportunity = args.Row;
            var opportunityExtension = opportunity?.GetExtension<CrOpportunityExt>();
            if (opportunityExtension != null)
            {
                opportunityExtension.TotalAmount =
                    (opportunityExtension.QuotedAmount ?? 0) - (opportunity.CuryDiscTot ?? 0);
            }
        }

        protected virtual void _(Events.RowUpdated<MultipleQuote> args)
        {
            var quote = args.Row;
            if (quote != null && quote.IsSelected == true)
            {
                UpdateOpportunityAfterQuoteSelection(quote);
            }
        }

        protected virtual void _(Events.RowDeleted<MultipleQuote> args)
        {
            var quote = args.Row;
            var opportunity = Base.Opportunity.Current;
            var opportunityExtension = opportunity?.GetExtension<CrOpportunityExt>();
            if (quote != null && quote.IsSelected == true && opportunityExtension != null)
            {
                ClearOpportunity(opportunity, opportunityExtension);
            }
        }

        protected virtual void _(Events.FieldDefaulting<MultipleQuote.finalAmount> args)
        {
            if (args.Row is MultipleQuote quote)
            {
                args.NewValue = quote.QuotedAmount;
            }
        }

        protected virtual void _(Events.RowSelected<MultipleQuote> args)
        {
            var quote = args.Row;
            MultipleQuotes.Cache.ClearItemAttributes();
            ValidateContact(quote);
        }

        protected virtual void _(Events.FieldUpdated<MultipleQuote.contactID> args)
        {
            var quote = args.Row as MultipleQuote;
            if (quote?.ContactID != null && quote.BusinessAccountID == null)
            {
                var contact = new PXSelectReadonly<Contact,
                    Where<Contact.contactID, Equal<Required<Contact.contactID>>>>(Base).SelectSingle(quote.ContactID);
                quote.BusinessAccountID = contact?.BAccountID;
            }
        }

        protected virtual void _(Events.FieldUpdated<MultipleQuote.isSelected> args)
        {
            if (args.Row is MultipleQuote quote)
            {
                ClearQuotesSelection(quote);
                UpdateOpportunityAfterQuoteSelection(quote);
            }
        }

        protected virtual void _(Events.RowPersisting<MultipleQuote> args)
        {
            var quote = args.Row;
            var opportunity = Base.Opportunity.Current;
            if (opportunity != null && quote != null)
            {
                quote.OpportunityID = opportunity.OpportunityID;
            }
        }

        private void ValidateQuoteContacts()
        {
            using (new PXConnectionScope())
            {
                var quotes = GetQuotes();
                quotes.ForEach(ValidateContact);
            }
        }

        private void SetMultipleQuotesTabAvailability(CROpportunity opportunity)
        {
            var opportunityExtension = opportunity.GetExtension<CrOpportunityExt>();
            var expectedStatus = opportunity.Status.IsIn(OpportunityWorkflow.States.New, OpportunityWorkflow.States.Open);
            var isMultipleAccountsChecked = opportunityExtension.MultipleAccounts == true && expectedStatus;
            MultipleQuotes.Enable(isMultipleAccountsChecked);
        }

        private static void SetOpportunityAmountSource(CROpportunity opportunity)
        {
            var opportunityExtension = opportunity.GetExtension<CrOpportunityExt>();
            if (opportunityExtension != null)
            {
                if (opportunity.ManualTotalEntry != true && opportunityExtension.MultipleAccounts != true)
                {
                    opportunityExtension.QuotedAmount = opportunity.CuryAmount;
                    opportunityExtension.TotalAmount = opportunity.CuryProductsAmount;
                    opportunityExtension.GrossMarginAbsolute = 0;
                    opportunityExtension.GrossMarginPercentage = 0;
                    opportunityExtension.Cost = 0;
                }
                else
                {
                    opportunity.CuryAmount = opportunityExtension.QuotedAmount.GetValueOrDefault();
                    opportunity.CuryProductsAmount = opportunityExtension.TotalAmount.GetValueOrDefault();
                }
            }
        }

        private void UpdateOpportunityAfterQuoteSelection(MultipleQuote quote)
        {
            var opportunity = Base.Opportunity.Current;
            var opportunityExtension = opportunity?.GetExtension<CrOpportunityExt>();
            if (opportunityExtension == null)
            {
                return;
            }
            if (quote.IsSelected == true)
            {
                if (opportunityExtension.MultipleAccounts == true)
                {
                    PopulateOpportunityBasedOnQuote(opportunity, opportunityExtension, quote);
                }
            }
            else
            {
                ClearOpportunity(opportunity, opportunityExtension);
            }
        }

        private void ClearQuotesSelection(MultipleQuote selectedQuote)
        {
            var otherQuotes = GetQuotes().Except(selectedQuote);
            foreach (var quote in otherQuotes)
            {
                MultipleQuotes.Cache.SetValue<MultipleQuote.isSelected>(quote, false);
                if (MultipleQuotes.Cache.GetStatus(quote) != PXEntryStatus.Inserted)
                {
                    MultipleQuotes.Cache.SetStatus(quote, PXEntryStatus.Updated);
                }
            }
            MultipleQuotes.View.RequestRefresh();
        }

        private void SetQuoteRelatedFieldsEnabled(PXCache cache, CROpportunity opportunity)
        {
            var isManualAmount = opportunity.ManualTotalEntry == true;
            var isMultipleAccountsChecked = false;
            var opportunityExtension = opportunity.GetExtension<CrOpportunityExt>();
            if (opportunityExtension != null)
            {
                isMultipleAccountsChecked = opportunityExtension.MultipleAccounts == true;
            }
            PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(cache, opportunity, !isMultipleAccountsChecked);
            PXUIFieldAttribute.SetEnabled<CROpportunity.contactID>(cache, opportunity, !isMultipleAccountsChecked);
            PXUIFieldAttribute.SetEnabled<CrOpportunityExt.quotedAmount>(cache, opportunity,
                !(isMultipleAccountsChecked || !isManualAmount));
            PXUIFieldAttribute.SetEnabled<CrOpportunityExt.cost>(cache, opportunity,
                !(isMultipleAccountsChecked || !isManualAmount));
            PXUIFieldAttribute.SetEnabled<CROpportunity.manualTotalEntry>(cache, opportunity,
                !isMultipleAccountsChecked);

            PXUIFieldAttribute.SetVisible<CrOpportunityExt.cost>(cache, opportunity, isMultipleAccountsChecked);
            PXUIFieldAttribute.SetVisible<CrOpportunityExt.grossMarginAbsolute>(cache, opportunity, isMultipleAccountsChecked);
            PXUIFieldAttribute.SetVisible<CrOpportunityExt.grossMarginPercentage>(cache, opportunity, isMultipleAccountsChecked);
        }

        private void ValidateContact(MultipleQuote quote)
        {
            if (quote?.ContactID != null && quote.BusinessAccountID != null &&
                !DoesContactCorrespondsToBusinessAccount(quote))
            {
                var warning = new PXSetPropertyException(
                    PX.Objects.CR.Messages.ContractBAccountDiffer, PXErrorLevel.Warning);
                MultipleQuotes.Cache.RaiseExceptionHandling<MultipleQuote.contactID>(quote, quote.ContactID, warning);
            }
        }

        private bool DoesContactCorrespondsToBusinessAccount(MultipleQuote quote)
        {
            var contact = new PXSelect<Contact,
                Where<Contact.contactID, Equal<Required<Contact.contactID>>>>(Base).SelectSingle(quote.ContactID);
            return contact == null || contact.BAccountID == quote.BusinessAccountID;
        }

        private IEnumerable<MultipleQuote> GetQuotes()
        {
            return Base.Opportunity.Current != null
                ? new PXSelect<MultipleQuote,
                        Where<MultipleQuote.opportunityID, Equal<Current<CROpportunity.opportunityID>>,
                            Or<MultipleQuote.opportunityID, IsNull>>>(Base)
                    .Select(Base.Opportunity.Current.OpportunityID).FirstTableItems
                : Enumerable.Empty<MultipleQuote>();
        }

        private MultipleQuote GetSelectedQuote()
        {
            return new PXSelect<MultipleQuote,
                Where<MultipleQuote.opportunityID, Equal<Current<CROpportunity.opportunityID>>,
                    And<MultipleQuote.isSelected, Equal<True>>>>(Base).SelectSingle();
        }

        private void AddNewQuote(CROpportunity opportunity, CrOpportunityExt opportunityExtension)
        {
            var newQuote = new MultipleQuote
            {
                BusinessAccountID = opportunity.BAccountID,
                ContactID = opportunity.ContactID,
                QuotedAmount = opportunityExtension.QuotedAmount,
                FinalAmount = opportunityExtension.QuotedAmount,
                CostEstimate = opportunityExtension.Cost,
                GrossMarginAbsolute = CalculateGrossMarginAbsolute(opportunityExtension),
                GrossMarginPercentage = CalculateGrossMargin(opportunityExtension),
                QuotedOn = Base.Accessinfo.BusinessDate,
                FinalGrossMarginAbsolute = opportunityExtension.GrossMarginAbsolute,
                FinalGrossMarginPercentage = opportunityExtension.GrossMarginPercentage
            };
            MultipleQuotes.Insert(newQuote);
        }

        private static bool DoesOpportunityContainQuoteRelatedInfo(CrOpportunityExt opportunityExtension)
        {
            return opportunityExtension.QuotedAmount.HasValue && opportunityExtension.QuotedAmount != 0
                || opportunityExtension.Cost.HasValue && opportunityExtension.Cost != 0;
        }

        private void PopulateOpportunityBasedOnSelectedQuote(CROpportunity opportunity,
            CrOpportunityExt opportunityExtension)
        {
            var selectedQuote = GetSelectedQuote();
            if (selectedQuote != null)
            {
                PopulateOpportunityBasedOnQuote(opportunity, opportunityExtension, selectedQuote);
            }
            else
            {
                ClearOpportunity(opportunity, opportunityExtension);
            }
        }

        private void PopulateOpportunityBasedOnQuote(CROpportunity opportunity, CrOpportunityExt opportunityExtension,
            MultipleQuote selectedQuote)
        {
            Base.Opportunity.SetValueExt<CROpportunity.contactID>(opportunity, selectedQuote.ContactID);
            Base.Opportunity.SetValueExt<CROpportunity.bAccountID>(opportunity, selectedQuote.BusinessAccountID);
            opportunityExtension.QuotedAmount = selectedQuote.FinalAmount;
            opportunityExtension.Cost = selectedQuote.CostEstimate;
            opportunityExtension.GrossMarginAbsolute = CalculateGrossMarginAbsolute(opportunityExtension);
            opportunityExtension.GrossMarginPercentage = CalculateGrossMargin(opportunityExtension);
            opportunityExtension.TotalAmount = opportunityExtension.QuotedAmount - (opportunity.CuryDiscTot ?? 0);
        }

        private void ClearOpportunity(CROpportunity opportunity, CrOpportunityExt opportunityExtension)
        {
            Base.Opportunity.SetValueExt<CROpportunity.bAccountID>(opportunity, null);
            Base.Opportunity.SetValueExt<CROpportunity.contactID>(opportunity, null);
            Base.Opportunity.Cache.ClearFieldErrors<CROpportunity.contactID>(opportunity);
            opportunityExtension.QuotedAmount = 0;
            opportunityExtension.Cost = 0;
            opportunityExtension.GrossMarginAbsolute = 0;
            opportunityExtension.GrossMarginPercentage = 0;
            opportunityExtension.TotalAmount = -opportunity.CuryDiscTot;
        }

        private static decimal? CalculateGrossMargin(CrOpportunityExt opportunityExtension)
        {
            return opportunityExtension.QuotedAmount != 0
                ? (opportunityExtension.QuotedAmount - opportunityExtension.Cost) /
                opportunityExtension.QuotedAmount * 100
                : 0;
        }

        private static decimal? CalculateGrossMarginAbsolute(CrOpportunityExt opportunityExtension)
        {
            return opportunityExtension.QuotedAmount - opportunityExtension.Cost;
        }
    }
}