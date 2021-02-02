using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;

namespace PX.Objects.FS
{
    public class SMOpenPeriodAttribute : OpenPeriodAttribute
    {
        private Type _TableSourceType;
        private Type _DependenceFieldType;

        public SMOpenPeriodAttribute(Type sourceType,
                                     Type dependenceFieldType,
                                     Type branchSourceType = null,
                                     Type branchSourceFormulaType = null,
                                     Type organizationSourceType = null,
                                     Type useMasterCalendarSourceType = null,
                                     Type defaultType = null,
                                     bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
                                     bool useMasterOrganizationIDByDefault = false)
            : base(typeof(Search<FinPeriod.finPeriodID, 
                          Where<
                              Current<CreateInvoiceFilter.postTo>, NotEqual<FSPostTo.Sales_Order_Module>,
                              And<FinPeriod.status, Equal<FinPeriod.status.open>,
                              Or<Current<CreateInvoiceFilter.postTo>, Equal<FSPostTo.Sales_Order_Module>>>>>),
                   sourceType,
                   branchSourceType: branchSourceType,
                   branchSourceFormulaType: branchSourceFormulaType,
                   organizationSourceType: organizationSourceType,
                   useMasterCalendarSourceType: useMasterCalendarSourceType,
                   defaultType: defaultType,
                   redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
                   useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault)
        {
            _TableSourceType = BqlCommand.GetItemType(sourceType);
            _DependenceFieldType = dependenceFieldType;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            if (_TableSourceType != null)
            {
                sender.Graph.FieldUpdated.AddHandler(_TableSourceType, _DependenceFieldType.Name, SourceFieldUpdated);
            }
        }

        protected void SourceFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            object newValue = (string)cache.GetValue(e.Row, _FieldName);
            cache.SetDefaultExt(e.Row, _FieldName, FormatForDisplay((string)newValue));
        }

        protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
        {
            PeriodValidationResult validationResult = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

            string postTo = (string)sender.GetValue(row, _DependenceFieldType.Name);

            if(postTo == ID.Batch_PostTo.SO)
            {
                return new PeriodValidationResult();
            }

            if (!validationResult.HasWarningOrError && finPeriod.ARClosed == true)
            {
                validationResult = HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: AR.Messages.FinancialPeriodClosedInAR);
            }

            return validationResult;
        }
    }
}
