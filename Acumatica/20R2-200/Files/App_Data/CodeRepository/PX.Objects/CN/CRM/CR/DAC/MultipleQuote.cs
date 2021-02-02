using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CR;
using PX.Objects.CS;
using CrMessages = PX.Objects.CR.Messages;

namespace PX.Objects.CN.CRM.CR.DAC
{
    [Serializable]
    [PXCacheName("Multiple Customers")]
    public class MultipleQuote : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? MultipleQuoteID
        {
            get;
            set;
        }

        [PXDBString]
        public virtual string OpportunityID
        {
            get;
            set;
        }

        [CustomerAndProspect(DisplayName = "Business Account")]
        public virtual int? BusinessAccountID
        {
            get;
            set;
        }

        [PXDBInt]
        [PXSelector(typeof(Search2<Contact.contactID,
            LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
            Where<BAccount.bAccountID, Equal<Current<businessAccountID>>,
                Or<Current<businessAccountID>, IsNull>>>),
            DescriptionField = typeof(Contact.displayName), Filterable = true, ValidateValue = false)]
        [PXRestrictor(typeof(Where<
            Where2<Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
                Or<Contact.contactType, Equal<ContactTypesAttribute.lead>>>,
                And<Where<BAccount.type, IsNull,
                Or<BAccount.type, Equal<BAccountType.customerType>,
                Or<BAccount.type, Equal<BAccountType.prospectType>,
                Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>>), CrMessages.ContactBAccountOpp,
            typeof(Contact.displayName), typeof(Contact.contactID))]
        [PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>), CrMessages.ContactInactive,
            typeof(Contact.displayName))]
        [PXDBChildIdentity(typeof(Contact.contactID))]
        [PXUIField(DisplayName = "Contact")]
        public virtual int? ContactID
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quoted Amount")]
        public virtual decimal? QuotedAmount
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Cost Estimate")]
        public virtual decimal? CostEstimate
        {
            get;
            set;
        }

        [PXBaseCury]
        [PXFormula(typeof(Sub<quotedAmount, costEstimate>))]
        [PXUIField(DisplayName = "Gross Margin", Enabled = false)]
        public virtual decimal? GrossMarginAbsolute
        {
            get;
            set;
        }

        [PXDecimal(2)]
        [PXFormula(typeof(
            Switch<
                Case<Where<quotedAmount, NotEqual<decimal0>>,
                    Mult<
                        Div<
                            Sub<quotedAmount, costEstimate>,
                            quotedAmount>,
                    decimal100>>,
            decimal0>))]
        [PXUIField(DisplayName = "Gross Margin, %", Enabled = false)]
        public virtual decimal? GrossMarginPercentage
        {
            get;
            set;
        }

        [PXDBDate]
        [PXUIField(DisplayName = "Quoted On")]
        public virtual DateTime? QuotedOn
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? IsSelected
        {
            get;
            set;
        }

        [PXDBBaseCury]
        [PXFormula(typeof(Default<quotedAmount>))]
        [PXUIField(DisplayName = "Final Amount")]
        public virtual decimal? FinalAmount
        {
            get;
            set;
        }

        [PXBaseCury]
        [PXFormula(typeof(Sub<finalAmount, costEstimate>))]
        [PXUIField(DisplayName = "Final Gross Margin", Enabled = false)]
        public virtual decimal? FinalGrossMarginAbsolute
        {
            get;
            set;
        }

        [PXDecimal(2)]
        [PXFormula(typeof(
            Switch<
                Case<Where<finalAmount, NotEqual<decimal0>>,
                    Mult<
                        Div<
                            Sub<finalAmount, costEstimate>,
                            finalAmount>,
                        decimal100>>,
                decimal0>))]
        [PXUIField(DisplayName = "Final Gross Margin, %", Enabled = false)]
        public virtual decimal? FinalGrossMarginPercentage
        {
            get;
            set;
        }

        [PXDBCreatedByID(Visible = false)]
        public override Guid? CreatedById
        {
            get;
            set;
        }

        [PXDBLastModifiedByID(Visible = false)]
        public override Guid? LastModifiedById
        {
            get;
            set;
        }

        public abstract class multipleQuoteID : IBqlField
        {
        }

        public abstract class opportunityID : IBqlField
        {
        }

        public abstract class businessAccountID : IBqlField
        {
        }

        public abstract class contactID : IBqlField
        {
        }

        public abstract class quotedAmount : IBqlField
        {
        }

        public abstract class costEstimate : IBqlField
        {
        }

        public abstract class grossMarginAbsolute : IBqlField
        {
        }

        public abstract class grossMarginPercentage : IBqlField
        {
        }

        public abstract class quotedOn : IBqlField
        {
        }

        public abstract class isSelected : IBqlField
        {
        }

        public abstract class finalAmount : IBqlField
        {
        }

        public abstract class finalGrossMarginAbsolute : IBqlField
        {
        }

        public abstract class finalGrossMarginPercentage : IBqlField
        {
        }

        public abstract class tstamp : IBqlField
        {
        }

        public abstract class createdByID : IBqlField
        {
        }

        public abstract class createdByScreenID : IBqlField
        {
        }

        public abstract class createdDateTime : IBqlField
        {
        }

        public abstract class lastModifiedByID : IBqlField
        {
        }

        public abstract class lastModifiedByScreenID : IBqlField
        {
        }

        public abstract class lastModifiedDateTime : IBqlField
        {
        }

        public abstract class noteID : IBqlField
        {
        }
    }
}