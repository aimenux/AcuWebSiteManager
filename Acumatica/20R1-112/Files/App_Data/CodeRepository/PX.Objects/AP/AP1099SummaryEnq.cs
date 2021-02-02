using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class AP1099SummaryEnq : PXGraph<AP1099SummaryEnq>
	{
		public PXSelect<AP1099Year> Year_Header;

		public PXCancel<AP1099Year> Cancel;
		public PXFirst<AP1099Year> First;
		public PXPrevious<AP1099Year> Prev;
		public PXNext<AP1099Year> Next;
		public PXLast<AP1099Year> Last;
		public PXAction<AP1099Year> close1099Year;
		public PXAction<AP1099Year> reportsFolder;
		public PXAction<AP1099Year> year1099SummaryReport;
		public PXAction<AP1099Year> year1099DetailReport;
		public PXAction<AP1099Year> open1099PaymentsReport;
		public PXAction<AP1099Year> year1099NECSummaryReport;

		[PXFilterable]
		public PXSelectJoinGroupBy<AP1099Box, 
					LeftJoin<AP1099History, 
						On<AP1099History.boxNbr, Equal<AP1099Box.boxNbr>, 
							And<AP1099History.finYear, Equal<Current<AP1099Year.finYear>>>>,
					LeftJoin<Branch, 
						On<Branch.branchID, Equal<AP1099History.branchID>>>>, 
					Where<Branch.organizationID, Equal<Current<AP1099Year.organizationID>>,
							Or<Branch.organizationID, IsNull>>,
					Aggregate<GroupBy<AP1099Box.boxNbr, Sum<AP1099History.histAmt>>>> 
					Year_Summary;

		protected virtual void AP1099Year_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			AP1099Year year1099 = (AP1099Year)e.Row;
			if (year1099 == null) return;

			close1099Year.SetEnabled(!string.IsNullOrEmpty(year1099.FinYear) && year1099.Status == AP1099Year.status.Open);

			int[] childBranches = PXAccess.GetChildBranchIDs(Year_Header.Current.OrganizationID);

			if (childBranches.Length == 0)
				return;

			bool hasUnappliedPrepayments = PXSelectJoin<APRegister, 
													InnerJoin<Vendor, 
														On<APRegister.vendorID, Equal<Vendor.bAccountID>>,
													InnerJoin<OrganizationFinPeriod, 
														On<APRegister.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>>>>,
													Where<Vendor.vendor1099, Equal<True>,
														And<APRegister.docType, Equal<APDocType.prepayment>,
														And<APRegister.status, NotEqual<APDocStatus.closed>,
														And<OrganizationFinPeriod.finYear, Equal<Current<AP1099Year.finYear>>,
														And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>,
														And<APRegister.branchID, In<Required<APRegister.branchID>>>>>>>>>
													.SelectWindowed(this, 0 , 1, Year_Header.Current.OrganizationID, childBranches)
													.AsEnumerable()
													.Any();

			PXSetPropertyException finYearExeption =
				hasUnappliedPrepayments
					? new PXSetPropertyException(Messages.ExistsUnappliedPayments, PXErrorLevel.Warning, year1099.FinYear)
					: null;

			sender.RaiseExceptionHandling<AP1099Year.finYear>(
				year1099,
				year1099.FinYear,
				finYearExeption);
		}

		protected virtual void AP1099Year_BranchID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AP1099Year year = e.Row as AP1099Year;

			if (year == null)
				return;

			year.FinYear = null;
		}
		
		[PXUIField(DisplayName = Messages.CloseYear, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Close1099Year(PXAdapter adapter)
		{
			PXCache cache = Year_Header.Cache;
			List<AP1099Year> list = adapter.Get().Cast<AP1099Year>().ToList();
			foreach(AP1099Year year in list
				.Where(year => !string.IsNullOrEmpty(year.FinYear) 
						&& year.Status == AP1099Year.status.Open
						&& Year_Header.Current.OrganizationID == year.OrganizationID))
			{
				year.Status = AP1099Year.status.Closed;
				cache.Update(year);
			}
			if (cache.IsInsertedUpdatedDeleted)
			{
				Actions.PressSave();
				PXLongOperation.StartOperation(this, delegate {});
			}
			return list;
		}

		public AP1099SummaryEnq()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled<AP1099Box.boxNbr>(Year_Summary.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<AP1099Box.descr>(Year_Summary.Cache, null, false);

			reportsFolder.MenuAutoOpen = true;
			reportsFolder.AddMenuAction(year1099SummaryReport);
			reportsFolder.AddMenuAction(year1099NECSummaryReport);
			reportsFolder.AddMenuAction(year1099DetailReport);
			reportsFolder.AddMenuAction(open1099PaymentsReport);
		}

		public PXSetup<APSetup> APSetup;

		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Reportsfolder(PXAdapter adapter)
		{
			return adapter.Get();
		}
		
		[PXUIField(DisplayName = Messages.Year1099SummaryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable Year1099SummaryReport(PXAdapter adapter)
		{
			if (Year_Header.Current != null)
			{
				Organization org = OrganizationMaint.FindOrganizationByID(this, Year_Header.Current.OrganizationID);

				if (org?.Reporting1099ByBranches == true)
				{
					throw new PXException(Messages.CannotShowReportForEntireOrganization);
				}

				BAccountR orgBAccount = 
					SelectFrom<BAccountR>
						.Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.FromCurrent>>
						.View.SelectSingleBound(this, new object[] { org });

				Dictionary<string, string> parameters = new Dictionary<string, string>
				{
					["PayerBAccountID"] = orgBAccount?.AcctCD,
					["FinYear"] = Year_Header.Current.FinYear
				};
				throw new PXReportRequiredException(parameters, "AP654000", Messages.Year1099SummaryReport);
			}
			return adapter.Get();
		}

		// Acuminator disable once PX1092 IncorrectAttributesOnActionHandler [Justification]
		[PXUIField(DisplayName = Messages.Year1099NEC_SummaryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable Year1099NECSummaryReport(PXAdapter adapter)
		{
			if (Year_Header.Current != null)
			{
				Organization org = OrganizationMaint.FindOrganizationByID(this, Year_Header.Current.OrganizationID);

				if (org?.Reporting1099ByBranches == true)
				{
					throw new PXException(Messages.CannotShowReportForEntireOrganization);
				}

				BAccountR orgBAccount =
					SelectFrom<BAccountR>
						.Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.FromCurrent>>
						.View.SelectSingleBound(this, new object[] { org });

				Dictionary<string, string> parameters = new Dictionary<string, string>()
				{
					["PayerBAccountID"] = orgBAccount?.AcctCD,
					["FinYear"] = Year_Header.Current.FinYear,
					["Format"] = "NEC"
				};

				throw new PXReportRequiredException(parameters, "AP654000", Messages.Year1099NEC_SummaryReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.Year1099DetailReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable Year1099DetailReport(PXAdapter adapter)
		{
			if (Year_Header.Current != null)
			{
				Organization org = OrganizationMaint.FindOrganizationByID(this, Year_Header.Current.OrganizationID);

				if(org?.Reporting1099ByBranches == true)
				{
					throw new PXException(Messages.CannotShowReportForEntireOrganization);
				}

				BAccountR orgBAccount =
					SelectFrom<BAccountR>
						.Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.FromCurrent>>
						.View.SelectSingleBound(this, new object[] { org });

				Dictionary<string, string> parameters = new Dictionary<string, string>
				{
					["PayerBAccountID"] = orgBAccount?.AcctCD,
					["FinYear"] = Year_Header.Current.FinYear
				};
				throw new PXReportRequiredException(parameters, "AP654500", Messages.Year1099DetailReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Open 1099 Payments", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable Open1099PaymentsReport(PXAdapter adapter)
		{
			if (Year_Header.Current != null)
			{
				Organization org = OrganizationMaint.FindOrganizationByID(this, Year_Header.Current.OrganizationID);

				BAccountR orgBAccount =
					SelectFrom<BAccountR>
						.Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.FromCurrent>>
						.View.SelectSingleBound(this, new object[] { org });

				Dictionary<string, string> parameters = new Dictionary<string, string>
				{
					["OrgBAccountID"] = orgBAccount?.AcctCD,
					["FinYear"] = Year_Header.Current.FinYear
				};
				throw new PXReportRequiredException(parameters, "AP656500", "Open 1099 Payments");
			}
			return adapter.Get();
		}
	}
}
