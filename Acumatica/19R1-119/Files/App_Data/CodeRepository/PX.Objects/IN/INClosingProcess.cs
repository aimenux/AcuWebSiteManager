using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
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
		[PXUIField(DisplayName = "Unposted to IN Documents", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
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

			throw new PXReportRequiredException(parameters, "IN656500", PXBaseRedirectException.WindowMode.NewWindow, "Unposted IN Documents");
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

		protected static BqlCommand UnpostedDocumentsQuery { get; } =
			PXSelectReadonly2<
				SOOrderShipment,
				LeftJoin<ARRegister,
					On<ARRegister.docType, Equal<SOOrderShipment.invoiceType>,
					And<ARRegister.refNbr, Equal<SOOrderShipment.invoiceNbr>>>,
				LeftJoin<Branch,
					On<ARRegister.branchID, Equal<Branch.branchID>>,
				LeftJoin<INSite,
					On<SOOrderShipment.FK.Site>,
				LeftJoin<INSiteBranch,
					On<INSite.branchID, Equal<INSiteBranch.branchID>>,
				LeftJoin<SOShipLine, 
					On<SOShipLine.shipmentType, Equal<SOOrderShipment.shipmentType>,
					And<SOShipLine.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>,
					And<SOShipLine.origOrderType, Equal<SOOrderShipment.orderType>,
					And<SOShipLine.origOrderNbr, Equal<SOOrderShipment.orderNbr>>>>>,
				LeftJoin<SOLine,
					On<SOLine.orderType, Equal<SOShipLine.origOrderType>,
					And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>,
					And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>,
				LeftJoin<LineBranch,
					On<SOLine.branchID, Equal<LineBranch.branchID>>>>>>>>>,
				Where<SOOrderShipment.confirmed, Equal<True>,
					And<SOOrderShipment.createINDoc, Equal<True>,
					And<SOOrderShipment.invtRefNbr, IsNull,
					And<Where<SOOrderShipment.invoiceNbr, IsNotNull,
							And2<WhereFinPeriodInRange<ARRegister.finPeriodID, Branch.organizationID>,
						Or<SOOrderShipment.invoiceNbr, IsNull,
							And<Where<SOOrderShipment.shipmentType, Equal<INDocType.transfer>,
									And2<WhereDateInRange<SOOrderShipment.shipDate, INSiteBranch.organizationID>,
								Or<SOOrderShipment.shipmentType, NotEqual<INDocType.transfer>,
									And<SOOrderShipment.shipmentType, NotEqual<INDocType.dropShip>,
									And<WhereDateInRange<SOOrderShipment.shipDate, LineBranch.organizationID>>>>>>>>>>>>>>>
				.GetCommand();

		public PXSelectReadonly2<
			SOOrderShipment,
			LeftJoin<ARRegister, 
				On<ARRegister.docType, Equal<SOOrderShipment.invoiceType>, 
				And<ARRegister.refNbr, Equal<SOOrderShipment.invoiceNbr>>>>,
			Where<SOOrderShipment.confirmed, Equal<True>, 
				And<SOOrderShipment.createINDoc, Equal<True>, 
				And<SOOrderShipment.invtRefNbr, IsNull,
				And<Where<SOOrderShipment.invoiceNbr, IsNotNull, 
					And<ARRegister.finPeriodID, Equal<Optional<FinPeriod.finPeriodID>>,
					Or<SOOrderShipment.invoiceNbr, IsNull, 
					And<SOOrderShipment.shipmentType, NotEqual<INDocType.dropShip>,
					And<SOOrderShipment.shipDate, GreaterEqual<Optional<FinPeriod.startDate>>, 
					And<SOOrderShipment.shipDate, Less<Optional<FinPeriod.endDate>>>>>>>>>>>>>
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
