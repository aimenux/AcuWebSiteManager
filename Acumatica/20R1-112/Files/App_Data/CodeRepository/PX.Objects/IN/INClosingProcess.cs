using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods.TableDefinition;

using PX.Objects.SO;

namespace PX.Objects.IN
{
	public class INClosingProcess : FinPeriodClosingProcessBase<INClosingProcess, FinPeriod.iNClosed, FeaturesSet.inventory>
	{
		public PXAction<FinPeriodClosingProcessParameters> ShowUnpostedDocuments;
		[PXUIField(DisplayName = "Documents Not Posted to Inventory", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual IEnumerable showUnpostedDocuments(PXAdapter adapter)
		{
			ShowOpenShipments(SelectedItems);
			return adapter.Get();
		}

		protected virtual void ShowOpenShipments(IEnumerable<FinPeriod> periods)
		{
			ParallelQuery<string> periodIDs = periods.Select(fp => fp.FinPeriodID).AsParallel();

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters["FromPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Min());
			parameters["ToPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(periodIDs.Max());
			Organization org = OrganizationMaint.FindOrganizationByID(this, Filter.Current.OrganizationID);
			parameters["OrgID"] = org?.OrganizationCD;

			throw new PXReportRequiredException(parameters, "IN656500", PXBaseRedirectException.WindowMode.NewWindow, "Documents Not Posted to Inventory");
		}

		protected static BqlCommand OpenDocumentsQuery { get; } =
			PXSelectJoin<
				INRegister,
				LeftJoin<INTran, 
					On<INTran.docType, Equal<INRegister.docType>,
					And<INTran.refNbr, Equal<INRegister.refNbr>>>,
				LeftJoin<Branch,
					On<Branch.branchID, Equal<INRegister.branchID>>,
				LeftJoin<INSiteTo,
					On<INSiteTo.siteID, Equal<INRegister.toSiteID>,
					And<INRegister.transferType, Equal<INTransferType.oneStep>>>,
				LeftJoin<INSiteToBranch,
					On<INSiteToBranch.branchID, Equal<INSiteTo.branchID>>,
				LeftJoin<TranBranch,
					On<TranBranch.branchID, Equal<INTran.branchID>>,
				LeftJoin<TranINSite,
					On<TranINSite.siteID, Equal<INTran.siteID>>,
				LeftJoin<TranINSiteBranch,
					On<TranINSiteBranch.branchID, Equal<TranINSite.branchID>>>>>>>>>,
				Where<INRegister.released, NotEqual<True>,
					And<
					Where2<
						WhereFinPeriodInRange<INRegister.finPeriodID, Branch.organizationID>,
						Or2<WhereFinPeriodInRange<INRegister.finPeriodID, INSiteToBranch.organizationID>,
						Or2<WhereFinPeriodInRange<INRegister.finPeriodID, TranBranch.organizationID>,
						Or<WhereFinPeriodInRange<INRegister.finPeriodID, TranINSiteBranch.organizationID>>>>>>>,
				OrderBy<
					Asc<INRegister.finPeriodID, // sorting, must be redundant relative to the grouping and precede it
					Asc<INRegister.docType, // grouping
					Asc<INRegister.refNbr>>>>> // grouping
				.GetCommand();

		public SelectFrom<SOOrderShipment>
			.LeftJoin<ARRegister>
				.On<SOOrderShipment.FK.ARRegister>
			.LeftJoin<SOOrderTypeOperation>
				.On<SOOrderShipment.FK.OrderTypeOperation>
			.Where<SOOrderShipment.confirmed.IsEqual<True>
				.And<SOOrderShipment.createINDoc.IsEqual<True>
				.And<SOOrderShipment.invtRefNbr.IsNull>
				.And<Where<
					SOOrderTypeOperation.iNDocType.IsNull
					.Or<SOOrderTypeOperation.iNDocType.IsNotEqual<INTranType.noUpdate>>>>
				.And<Where2<
					SOOrderShipment.invoiceNbr.IsNotNull
						.And<ARRegister.finPeriodID.IsEqual<FinPeriod.finPeriodID.AsOptional>>,
					Or<SOOrderShipment.invoiceNbr.IsNull
						.And<SOOrderShipment.shipmentType.IsNotEqual<INDocType.dropShip>
						.And<SOOrderShipment.shipDate.IsGreaterEqual<FinPeriod.startDate.AsOptional> 
						.And<SOOrderShipment.shipDate.IsLess<FinPeriod.endDate.AsOptional>>>>>>>>>.View.ReadOnly 
			UnpostedDocuments;

		protected override UnprocessedObjectsCheckingRule[] CheckingRules { get; } = new UnprocessedObjectsCheckingRule[]
		{
			new UnprocessedObjectsCheckingRule
			{
				ReportID = "IN656600",
				ErrorMessage = AP.Messages.PeriodHasUnreleasedDocs,
				CheckCommand = OpenDocumentsQuery 
			},
			// TODO: Implement the all another needed checkers to select the other open inventory entities.
		};

		protected virtual void _(Events.RowSelected<FinPeriod> e)
		{
			ShowUnpostedDocuments.SetEnabled(SelectedItems.Any());

			if (PXAccess.FeatureInstalled<FeaturesSet.branch>()
			    && !PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
				return;

			FinPeriod row = (FinPeriod)e.Row;
			if (row == null) return;

			bool warnDocsUnposted = (row.Selected == true && UnpostedDocuments.View.SelectSingleBound(new[] { row }) != null);
			Exception warnDocsUnpostedExc = warnDocsUnposted ? new PXSetPropertyException(Messages.UnpostedDocsExist, PXErrorLevel.RowWarning) : null;
			FinPeriods.Cache.RaiseExceptionHandling<FinPeriod.selected>(row, null, warnDocsUnpostedExc);
		}
	}
}
