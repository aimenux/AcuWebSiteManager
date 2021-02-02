using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.EP.GraphExtensions
{
    public class ExpenseClaimDetailEntryExt : CreatedFromDailyFieldReportExtension<ExpenseClaimDetailEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportEmployeeExpense>
            .Where<DailyFieldReportEmployeeExpense.employeeExpenseId.IsEqual<
                EPExpenseClaimDetails.claimDetailCD.FromCurrent>>
            .View DailyFieldReportEmployeeExpense;

        protected override string EntityName => DailyFieldReportEntityNames.EmployeeExpense;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        /// <summary>
        /// Project availabitily logic moved to <see cref="ExpenseClaimDetailEntryProjectAvailabilityExt" />
        /// because another Acumatica Graph Extension overrides our logic.
        /// </summary>
        public override void _(Events.RowSelected<DailyFieldReportRelatedDocument> args)
        {
        }

        protected override DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping()
        {
            return new DailyFieldReportRelatedDocumentMapping(typeof(EPExpenseClaimDetails))
            {
                ReferenceNumber = typeof(EPExpenseClaimDetails.claimDetailCD),
                ProjectId = typeof(EPExpenseClaimDetails.contractID)
            };
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportEmployeeExpense))
            {
                RelationNumber = typeof(DailyFieldReportEmployeeExpense.employeeExpenseId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportEmployeeExpense);
        }
    }
}
