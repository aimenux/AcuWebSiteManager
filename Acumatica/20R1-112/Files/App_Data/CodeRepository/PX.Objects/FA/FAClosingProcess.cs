using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.FA
{

	public class FAClosingProcess : FinPeriodClosingProcessBase<FAClosingProcess, FinPeriod.fAClosed, FeaturesSet.fixedAsset>
	{
		protected static BqlCommand OpenDocumentsQuery { get; } = 
			PXSelectJoin<
				FARegister,
				InnerJoin<FATran, 
					On<FARegister.refNbr, Equal<FATran.refNbr>>,
				InnerJoin<FABook, 
					On<FATran.bookID, Equal<FABook.bookID>,
					And<FABook.updateGL, Equal<True>>>,
				LeftJoin<Branch, 
					On<FATran.branchID, Equal<Branch.branchID>>,
				LeftJoin<TranBranch,
					On<FATran.srcBranchID, Equal<TranBranch.branchID>>>>>>,
				Where<FARegister.released, NotEqual<True>,
					And<
						Where2<
							WhereFinPeriodInRange<FATran.finPeriodID, Branch.organizationID>,
							Or<WhereFinPeriodInRange<FATran.finPeriodID, TranBranch.organizationID>>>>>>
				.GetCommand();

		protected static BqlCommand NonDepreciatedAssetsQuery { get; } =
			PXSelectJoin<FABookBalance,
				LeftJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
				LeftJoin<Branch, On<FixedAsset.branchID, Equal<Branch.branchID>>,
				LeftJoin<FABook, On<FABookBalance.bookID, Equal<FABook.bookID>>,
				LeftJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>>>>>,
				Where<FABookBalance.deprFromPeriod, LessEqual<Current<UnprocessedObjectsQueryParameters.toFinPeriodID>>,
					And<FABookBalance.deprToPeriod, GreaterEqual<Current<UnprocessedObjectsQueryParameters.fromFinPeriodID>>,
					And2<Where<Branch.organizationID, Equal<Current<UnprocessedObjectsQueryParameters.organizationID>>,
						Or<Current<UnprocessedObjectsQueryParameters.organizationID>, IsNull>>,
					And<FABookBalance.updateGL, Equal<True>,
					And<FixedAsset.suspended, NotEqual<True>,
					And<FADetails.hold, NotEqual<True>,
					And<FABookBalance.initPeriod, IsNotNull,
					And<Where<FABookBalance.currDeprPeriod, IsNull,
							And<FABookBalance.status, Equal<FixedAssetStatus.active>,
						Or<FABookBalance.currDeprPeriod, LessEqual<Current<UnprocessedObjectsQueryParameters.toFinPeriodID>>>>>>>>>>>>>>
				.GetCommand();

		protected override UnprocessedObjectsCheckingRule[] CheckingRules { get; } = new UnprocessedObjectsCheckingRule[]
		{
			new UnprocessedObjectsCheckingRule
			{
				ReportID = "FA651100",
				ErrorMessage = AP.Messages.PeriodHasUnreleasedDocs,
				CheckCommand = OpenDocumentsQuery
			},
			new UnprocessedObjectsCheckingRule
			{
				ReportID = "FA652100",
				ErrorMessage = Messages.AssetNotDepreciatedInPeriod,
				MessageParameters = new System.Type[]{ typeof(FixedAsset.assetCD), typeof(FABook.bookCode) },
				CheckCommand = NonDepreciatedAssetsQuery
			}
		};

        protected override string EmptyReportMessage => Messages.UnreleasedDocumentsOrFixedAssets;
    }
}
