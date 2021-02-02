using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRAcaReportingMaint : PXGraph<PRAcaReportingMaint>
	{
		public PRAcaReportingMaint()
		{
			SelectedEmployeeRows = new PXView(this, false, FilteredEmployeeInformation.View.BqlSelect);
			SelectedEmployeeRows.WhereAnd(typeof(Where<PRAcaEmployeeMonthlyInformation.selected.IsEqual<True>>));

			CompanyMonthlyInformationPerMonth = new PXView(this, false, CompanyMonthlyInformation.View.BqlSelect);
			CompanyMonthlyInformationPerMonth.WhereAnd(typeof(Where<PRAcaCompanyMonthlyInformation.month.IsEqual<P.AsInt>>));

			SelectedCompanyRows = new PXView(this, false, CompanyMonthlyInformation.View.BqlSelect);
			SelectedCompanyRows.WhereAnd(typeof(Where<PRAcaCompanyMonthlyInformation.selected.IsEqual<True>>));

			MissingEmployees = new PXView(this, false, Employees.View.BqlSelect);
			MissingEmployees.WhereAnd(typeof(Where<PRAcaEmployeeMonthlyInformation.year.IsNull>));
		}

		#region Views
		public SelectFrom<PRAcaCompanyYearlyInformation>.View CompanyYearlyInformation;

		public PXFilter<MonthFilter> EmployeeMonthFilter;

		public SelectFrom<PRAcaEmployeeMonthlyInformation>
			.InnerJoin<EPEmployee>.On<EPEmployee.bAccountID.IsEqual<PRAcaEmployeeMonthlyInformation.employeeID>>
			.Where<PRAcaEmployeeMonthlyInformation.orgBAccountID.IsEqual<PRAcaCompanyYearlyInformation.orgBAccountID.FromCurrent>
				.And<PRAcaEmployeeMonthlyInformation.year.IsEqual<PRAcaCompanyYearlyInformation.year.FromCurrent>>
				.And<MonthFilter.month.FromCurrent.IsNull
					.Or<PRAcaEmployeeMonthlyInformation.month.IsEqual<MonthFilter.month.FromCurrent>>>>
			.OrderBy<PRAcaEmployeeMonthlyInformation.month.Asc, EPEmployee.acctCD.Asc>.View FilteredEmployeeInformation;

		public SelectFrom<PRAcaCompanyMonthlyInformation>
			.Where<PRAcaCompanyMonthlyInformation.orgBAccountID.IsEqual<PRAcaCompanyYearlyInformation.orgBAccountID.FromCurrent>
				.And<PRAcaCompanyMonthlyInformation.year.IsEqual<PRAcaCompanyYearlyInformation.year.FromCurrent>>>
			.OrderBy<PRAcaCompanyMonthlyInformation.month.Asc>.View CompanyMonthlyInformation;

		public SelectFrom<PRAcaAggregateGroupMember>
			.Where<PRAcaAggregateGroupMember.orgBAccountID.IsEqual<PRAcaCompanyYearlyInformation.orgBAccountID.FromCurrent>
				.And<PRAcaAggregateGroupMember.year.IsEqual<PRAcaCompanyYearlyInformation.year.FromCurrent>>>.View AggregateGroupInformation;

		public SelectFrom<EPEmployee>
			.LeftJoin<PRAcaEmployeeMonthlyInformation>.On<PRAcaEmployeeMonthlyInformation.employeeID.IsEqual<EPEmployee.bAccountID>
				.And<PRAcaEmployeeMonthlyInformation.orgBAccountID.IsEqual<PRAcaCompanyYearlyInformation.orgBAccountID.FromCurrent>>
				.And<PRAcaEmployeeMonthlyInformation.year.IsEqual<PRAcaCompanyYearlyInformation.year.FromCurrent>>>
			.LeftJoin<PRPayment>.On<PRPayment.employeeID.IsEqual<EPEmployee.bAccountID>
				.And<PRPayment.released.IsEqual<True>>
				.And<Where<PRPayment.branchID, Inside<PRAcaCompanyYearlyInformation.orgBAccountID.FromCurrent>>>>
			.LeftJoin<PREarningDetail>.On<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr>>
				.And<PREarningDetail.date.IsGreaterEqual<P.AsDateTime>>
				.And<PREarningDetail.date.IsLessEqual<P.AsDateTime>>>
			.InnerJoin<Branch>.On<Branch.bAccountID.IsEqual<EPEmployee.parentBAccountID>>
			.Where<Where<Branch.branchID, Inside<PRAcaCompanyYearlyInformation.orgBAccountID.FromCurrent>>>
			.AggregateTo<GroupBy<EPEmployee.bAccountID>>.View Employees;

		public SelectFrom<EPEmployeePosition>
			.Where<EPEmployeePosition.employeeID.IsEqual<P.AsInt>
				.And<Brackets<EPEmployeePosition.endDate.IsGreaterEqual<P.AsDateTime>>
					.Or<EPEmployeePosition.endDate.IsNull>>
				.And<EPEmployeePosition.startDate.IsLessEqual<P.AsDateTime>>>.View EmployeeActivePositions;

		public SelectFrom<PREmployeeDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PREmployeeDeduct.codeID>>
			.LeftJoin<PRAcaDeductCoverageInfo>.On<PRAcaDeductCoverageInfo.deductCodeID.IsEqual<PRDeductCode.codeID>>
			.Where<PREmployeeDeduct.bAccountID.IsEqual<P.AsInt>
				.And<PREmployeeDeduct.startDate.IsLessEqual<P.AsDateTime>>
				.And<Brackets<PREmployeeDeduct.endDate.IsGreaterEqual<P.AsDateTime>
					.Or<PREmployeeDeduct.endDate.IsNull>>>
				.And<PREmployeeDeduct.isActive.IsEqual<True>>
				.And<PRDeductCode.isActive.IsEqual<True>>
				.And<PRAcaDeductCode.acaApplicable.IsEqual<True>>>.View EmployeeAcaDeductions;

		public SelectFrom<PREarningDetail>
			.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<PREarningDetail.paymentDocType>
				.And<PRPayment.refNbr.IsEqual<PREarningDetail.paymentRefNbr>>>
			.Where<PRPayment.employeeID.IsEqual<P.AsInt>
				.And<PREarningDetail.date.IsGreaterEqual<P.AsDateTime>>
				.And<PREarningDetail.date.IsLessEqual<P.AsDateTime>>
				.And<Where<PRPayment.branchID, Inside<PRAcaCompanyYearlyInformation.orgBAccountID.FromCurrent>>>
				.And<PRPayment.released.IsEqual<True>>
				.And<PREarningDetail.isFringeRateEarning.IsEqual<False>>>.View EmployeeMonthlyEarnings;

		public PXFilter<PRAcaUpdateEmployeeFilter> EmployeeUpdate;
		public PXFilter<PRAcaUpdateCompanyMonthFilter> CompanyUpdate;

		private PXView SelectedEmployeeRows;
		private PXView CompanyMonthlyInformationPerMonth;
		private PXView SelectedCompanyRows;
		private PXView MissingEmployees;

		public SelectFrom<PRAcaCompanyYearlyInformation>
			.Where<PRAcaCompanyYearlyInformation.orgBAccountID.IsEqual<P.AsInt>
				.And<PRAcaCompanyYearlyInformation.year.IsEqual<P.AsString>>>.View CompanyYearlyInformationQuery;

		public SelectFrom<PRAcaAggregateGroupMember>
			.Where<PRAcaAggregateGroupMember.orgBAccountID.IsEqual<P.AsInt>
				.And<PRAcaAggregateGroupMember.year.IsEqual<P.AsString>>>.View AggregateGroupInformationQuery;
		#endregion Views

		#region View delegates
		protected IEnumerable filteredEmployeeInformation()
		{
			if (PXSelectorAttribute.Select<PRAcaCompanyYearlyInformation.year>(CompanyYearlyInformation.Cache, CompanyYearlyInformation.Current) != null)
			{
				if (!string.IsNullOrEmpty(CompanyYearlyInformation.Current?.Year) && !FilteredEmployeeInformation.Cache.Inserted.Any_())
				{
					HashSet<int> insertedMonths = new HashSet<int>();
					HashSet<int?> payrollEmployeeIds = SelectFrom<PREmployee>.View.Select(this).FirstTableItems.Select(x => x.BAccountID).ToHashSet();
					for (int month = 1; month <= 12; month++)
					{
						DateTime monthStart = new DateTime(int.Parse(CompanyYearlyInformation.Current.Year), month, 1);
						DateTime monthEnd = monthStart.AddMonths(1).AddTicks(-1);

						foreach (PXResult<EPEmployee, PRAcaEmployeeMonthlyInformation, PRPayment, PREarningDetail> missing in MissingEmployees.SelectMulti(monthStart, monthEnd)
							.Select(x => (PXResult<EPEmployee, PRAcaEmployeeMonthlyInformation, PRPayment, PREarningDetail>)x)
							.Where(x => payrollEmployeeIds.Contains(((EPEmployee)x).BAccountID)))
						{
							EPEmployee employee = (EPEmployee)missing;
							PREarningDetail earning = (PREarningDetail)missing;

							if (IsEmployeeActiveInMonth(employee, CompanyYearlyInformation.Current.Year, month, earning))
							{
								var newRecord = new PRAcaEmployeeMonthlyInformation()
								{
									OrgBAccountID = CompanyYearlyInformation.Current.OrgBAccountID,
									Year = CompanyYearlyInformation.Current.Year,
									Month = month,
									EmployeeID = employee.BAccountID
								};
								CalculateEmployeeRow(newRecord);
								FilteredEmployeeInformation.Insert(newRecord);
								insertedMonths.Add(month);
							}
						}
					}

					foreach (int month in insertedMonths)
					{
						PRAcaCompanyMonthlyInformation companyRecord = CompanyMonthlyInformationPerMonth.SelectSingle(month) as PRAcaCompanyMonthlyInformation;
						if (companyRecord != null)
						{
							CalculateCompanyRow(companyRecord);
							Caches[typeof(PRAcaCompanyMonthlyInformation)].Update(companyRecord);
						}
					}
				}
			}

			return null;
		}

		protected IEnumerable companyMonthlyInformation()
		{
			if (PXSelectorAttribute.Select<PRAcaCompanyYearlyInformation.year>(CompanyYearlyInformation.Cache, CompanyYearlyInformation.Current) != null)
			{
				if (!string.IsNullOrEmpty(CompanyYearlyInformation.Current?.Year) && !CompanyMonthlyInformation.Cache.Inserted.Any_())
				{
					for (int month = 1; month <= 12; month++)
					{
						PRAcaCompanyMonthlyInformation record = CompanyMonthlyInformationPerMonth.SelectSingle(month) as PRAcaCompanyMonthlyInformation;
						if (record == null)
						{
							record = new PRAcaCompanyMonthlyInformation()
							{
								OrgBAccountID = CompanyYearlyInformation.Current.OrgBAccountID,
								Year = CompanyYearlyInformation.Current.Year,
								Month = month
							};

							CalculateCompanyRow(record);
							CompanyMonthlyInformation.Insert(record);
						}
					}
				}
			}

			return null;
		}
		#endregion View delegates

		#region Actions
		public PXSave<PRAcaCompanyYearlyInformation> Save;
		public PXCancel<PRAcaCompanyYearlyInformation> Cancel;
		public PXInsert<PRAcaCompanyYearlyInformation> Insert;
		public PXFirst<PRAcaCompanyYearlyInformation> First;
		public PXPrevious<PRAcaCompanyYearlyInformation> Prev;
		public PXNext<PRAcaCompanyYearlyInformation> Next;
		public PXLast<PRAcaCompanyYearlyInformation> Last;

		public PXAction<PRAcaCompanyYearlyInformation> UpdateSelectedEmployees;
		[PXButton]
		[PXUIField(DisplayName = "Update")]
		protected virtual void updateSelectedEmployees()
		{
			IEnumerable recordsToUpdate = SelectedEmployeeRows.SelectMulti();
			if (recordsToUpdate.Any_())
			{
				UpdateEmployees(recordsToUpdate);
			}
		}

		public PXAction<PRAcaCompanyYearlyInformation> UpdateAllEmployees;
		[PXButton]
		[PXUIField(DisplayName = "Update All")]
		protected virtual void updateAllEmployees()
		{
			UpdateEmployees(FilteredEmployeeInformation.Select());
		}

		private void UpdateEmployees(IEnumerable recordsToUpdate)
		{
			if (EmployeeUpdate.View.Answer == WebDialogResult.None)
			{
				EmployeeUpdate.Cache.Clear();
				EmployeeUpdate.Cache.Insert();
			}

			if (EmployeeUpdate.AskExt() == WebDialogResult.Yes)
			{
				HashSet<int> updatedMonths = new HashSet<int>();

				foreach (PRAcaEmployeeMonthlyInformation record in recordsToUpdate.Cast<PXResult<PRAcaEmployeeMonthlyInformation>>())
				{
					if (EmployeeUpdate.Current.UpdateAcaFTStatus == true)
					{
						record.FTStatus = EmployeeUpdate.Current.AcaFTStatus;
					}

					if (EmployeeUpdate.Current.UpdateOfferOfCoverage == true)
					{
						record.OfferOfCoverage = EmployeeUpdate.Current.OfferOfCoverage;
					}

					if (EmployeeUpdate.Current.UpdateSection4980H == true)
					{
						record.Section4980H = EmployeeUpdate.Current.Section4980H;
					}

					if (EmployeeUpdate.Current.UpdateMinimumIndividualContribution == true)
					{
						record.MinimumIndividualContribution = EmployeeUpdate.Current.MinimumIndividualContribution;
					}

					CalculateEmployeeRow(record, EmployeeUpdate.Current);
					updatedMonths.Add(record.Month.Value);
					FilteredEmployeeInformation.Update(record);
				}

				// Update company records that correspond to the months for which at least one employee record was updated
				foreach (PRAcaCompanyMonthlyInformation companyRecord in CompanyMonthlyInformation.Select())
				{
					if (updatedMonths.Contains(companyRecord.Month.Value))
					{
						CalculateCompanyRow(companyRecord);
						CompanyMonthlyInformation.Update(companyRecord);
					}
				}
			}
		}

		public PXAction<PRAcaCompanyYearlyInformation> UpdateSelectedCompanyMonths;
		[PXButton]
		[PXUIField(DisplayName = "Update")]
		protected virtual void updateSelectedCompanyMonths()
		{
			IEnumerable<PRAcaCompanyMonthlyInformation> recordsToUpdate = SelectedCompanyRows.SelectMulti().Select(x => (PRAcaCompanyMonthlyInformation)x);
			if (recordsToUpdate.Any())
			{
				UpdateCompanyMonths(recordsToUpdate);
			}
		}

		public PXAction<PRAcaCompanyYearlyInformation> UpdateAllCompanyMonths;
		[PXButton]
		[PXUIField(DisplayName = "Update All")]
		protected virtual void updateAllCompanyMonths()
		{
			UpdateCompanyMonths(CompanyMonthlyInformation.Select().Select(x => (PRAcaCompanyMonthlyInformation)x));
		}

		private void UpdateCompanyMonths(IEnumerable<PRAcaCompanyMonthlyInformation> recordsToUpdate)
		{
			if (CompanyUpdate.View.Answer == WebDialogResult.None)
			{
				CompanyUpdate.Cache.Clear();
				CompanyUpdate.Cache.Insert();
			}

			if (CompanyUpdate.AskExt() == WebDialogResult.Yes)
			{
				foreach (PRAcaCompanyMonthlyInformation record in recordsToUpdate)
				{
					if (CompanyUpdate.Current.UpdateCertificationOfEligibility == true)
					{
						record.CertificationOfEligibility = CompanyUpdate.Current.CertificationOfEligibility;
					}

					if (CompanyUpdate.Current.UpdateSelfInsured == true)
					{
						record.SelfInsured = CompanyUpdate.Current.SelfInsured;
					}

					CalculateCompanyRow(record);
					CompanyMonthlyInformation.Update(record);
				}
			}
		}
		#endregion Actions

		#region Events
		protected virtual void _(Events.RowUpdated<PRAcaEmployeeMonthlyInformation> e)
		{
			if (e.OldRow == null || e.Row == null)
			{
				return;
			}

			if (e.ExternalCall)
			{
				// If only the selected checkbox is modified, don't dirty the cache
				bool onlySelectedModified = true;
				foreach (string field in e.Cache.Fields.Except(e.Cache.GetField(typeof(PRAcaEmployeeMonthlyInformation.selected))))
				{
					object newValue = e.Cache.GetValue(e.Row, field);
					object oldValue = e.Cache.GetValue(e.OldRow, field);
					if ((newValue == null) != (oldValue == null) ||
						newValue != null && !newValue.Equals(oldValue))
					{
						onlySelectedModified = false;
						break;
					}
				}

				if (onlySelectedModified)
				{
					e.Cache.IsDirty = false;
					return;
				} 
			}

			// Code 1G (self-insured coverage was offered to the employee who is not full time at any point in the year)
			// can't be used if employee us Full Time
			if (e.Row.FTStatus == AcaFTStatus.FullTime && e.Row.OfferOfCoverage == AcaOfferOfCoverage.Code1G)
			{
				e.Cache.SetValue<PRAcaEmployeeMonthlyInformation.offerOfCoverage>(e.Row, AcaOfferOfCoverage.Code1H);
			}

			if (e.ExternalCall)
			{
				PRAcaCompanyMonthlyInformation companyRow = CompanyMonthlyInformationPerMonth.SelectSingle(e.Row.Month) as PRAcaCompanyMonthlyInformation;
				if (companyRow != null)
				{
					CalculateCompanyRow(companyRow);
					Caches[typeof(PRAcaCompanyMonthlyInformation)].Update(companyRow);
				}
			}
		}

		protected virtual void _(Events.RowUpdated<PRAcaCompanyMonthlyInformation> e)
		{
			if (e.OldRow == null || e.Row == null)
			{
				return;
			}

			// If only the selected checkbox is modified, don't dirty the cache
			foreach (string field in e.Cache.Fields.Except(e.Cache.GetField(typeof(PRAcaCompanyMonthlyInformation.selected))))
			{
				object newValue = e.Cache.GetValue(e.Row, field);
				object oldValue = e.Cache.GetValue(e.OldRow, field);
				if ((newValue == null) != (oldValue == null) ||
					newValue != null && !newValue.Equals(oldValue))
				{
					return;
				}
			}

			e.Cache.IsDirty = false;
		}

		protected virtual void _(Events.RowSelected<PRAcaCompanyYearlyInformation> e)
		{
			PRAcaCompanyYearlyInformation row = e.Row as PRAcaCompanyYearlyInformation;
			if (row == null)
			{
				return;
			}

			if (row.OrgBAccountID != null && string.IsNullOrEmpty(row.Ein))
			{
				e.Cache.RaiseExceptionHandling<PRAcaCompanyYearlyInformation.ein>(
					row,
					null,
					new PXSetPropertyException(Messages.AcaEinMissing, PXErrorLevel.Warning));
			}
		}

		protected virtual void _(Events.RowInserting<PRAcaCompanyYearlyInformation> e)
		{
			PRAcaCompanyYearlyInformation row = e.Row as PRAcaCompanyYearlyInformation;
			if (row == null)
			{
				return;
			}

			if (row.OrgBAccountID != null && !string.IsNullOrEmpty(row.Year))
			{
				string previousYear = (int.Parse(row.Year) - 1).ToString();
				PRAcaCompanyYearlyInformation previousYearRecord = CompanyYearlyInformationQuery.SelectSingle(row.OrgBAccountID, previousYear);
				if (previousYearRecord != null)
				{
					row.IsPartOfAggregateGroup = previousYearRecord.IsPartOfAggregateGroup;
					row.IsAuthoritativeTransmittal = previousYearRecord.IsAuthoritativeTransmittal;

					foreach (PRAcaAggregateGroupMember previousYearMember in AggregateGroupInformationQuery.Select(row.OrgBAccountID, previousYear))
					{
						AggregateGroupInformation.Insert(new PRAcaAggregateGroupMember()
						{
							OrgBAccountID = row.OrgBAccountID,
							Year = row.Year,
							MemberCompanyName = previousYearMember.MemberCompanyName,
							MemberEin = previousYearMember.MemberEin
						});
					}
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<PRAcaEmployeeMonthlyInformation.hoursWorked> e)
		{
			PXDBDecimalAttribute.SetPrecision(e.Cache, e.Cache.GetField(typeof(PRAcaEmployeeMonthlyInformation.hoursWorked)), 0);
		}

		protected virtual void _(Events.FieldUpdating<PRAcaEmployeeMonthlyInformation.hoursWorked> e)
		{
			PXDBDecimalAttribute.SetPrecision(e.Cache, e.Cache.GetField(typeof(PRAcaEmployeeMonthlyInformation.hoursWorked)), null);
		}

		protected virtual void _(Events.FieldVerifying<PRAcaCompanyYearlyInformation.year> e)
		{
			PRAcaCompanyYearlyInformation row = e.Row as PRAcaCompanyYearlyInformation;
			if (row == null || string.IsNullOrEmpty(e.NewValue?.ToString()))
			{
				return;
			}

			if (!SelectFrom<PRPayGroupYear>.Where<PRPayGroupYear.year.IsEqual<P.AsString>>.View.Select(this, e.NewValue).Any_())
			{
				e.Cache.RaiseExceptionHandling<PRAcaCompanyYearlyInformation.year>(
					row,
					e.NewValue,
					new PXSetPropertyException(Messages.YearNotSetUp, e.NewValue.ToString()));
			}
		}
		#endregion Events

		#region Helpers
		private void CalculateEmployeeRow(PRAcaEmployeeMonthlyInformation row, PRAcaUpdateEmployeeFilter updateFilter = null)
		{
			if (row == null || string.IsNullOrEmpty(row.Year))
			{
				return;
			}

			(DateTime monthStart, DateTime monthEnd) = GetMonthStartEndDates(row.Year, row.Month.Value);
			string bestOfferOfCoverage = AcaOfferOfCoverage.GetDefault();
			PRAcaDeductCode bestMatchingDeduction = null;
			foreach (IGrouping<int, PXResult<PREmployeeDeduct, PRDeductCode, PRAcaDeductCoverageInfo>> deduction in EmployeeAcaDeductions
				.Select(row.EmployeeID, monthStart, monthEnd)
				.Select(x => (PXResult<PREmployeeDeduct, PRDeductCode, PRAcaDeductCoverageInfo>)x)
				.GroupBy(x => ((PRDeductCode)x).CodeID.Value))
			{
				string offerOfCoverage = GetDeductionOfferOfCoverage(deduction.Select(x => (PRAcaDeductCoverageInfo)x), row.EmployeeID.Value, row.Year);
				if (AcaOfferOfCoverage.Compare(bestOfferOfCoverage, offerOfCoverage) < 0)
				{
					bestOfferOfCoverage = offerOfCoverage;
					bestMatchingDeduction = ((PRDeductCode)deduction.First()).GetExtension<PRAcaDeductCode>();
				}
				else if (AcaOfferOfCoverage.Compare(bestOfferOfCoverage, offerOfCoverage) == 0)
				{
					PRAcaDeductCode currentDeduction = ((PRDeductCode)deduction.First()).GetExtension<PRAcaDeductCode>();
					if (bestMatchingDeduction == null || currentDeduction.MinimumIndividualContribution < bestMatchingDeduction.MinimumIndividualContribution)
					{
						bestOfferOfCoverage = offerOfCoverage;
						bestMatchingDeduction = ((PRDeductCode)deduction.First()).GetExtension<PRAcaDeductCode>();
					}
				}
			}

			if (updateFilter == null || updateFilter.UpdateAcaFTStatus != true)
			{
				row.FTStatus = GetFTStatus(row.EmployeeID.Value, row.Year, row.Month.Value);
			}

			if (updateFilter == null || updateFilter.UpdateOfferOfCoverage != true)
			{
				row.OfferOfCoverage = bestOfferOfCoverage;
			}

			if (updateFilter == null || updateFilter.UpdateMinimumIndividualContribution != true)
			{
				row.MinimumIndividualContribution = bestMatchingDeduction?.MinimumIndividualContribution;
			}

			row.HoursWorked = 0m;
			foreach (PXResult<PREarningDetail> earning in EmployeeMonthlyEarnings.Select(row.EmployeeID.Value, monthStart, monthEnd))
			{
				row.HoursWorked += ((PREarningDetail)earning).Hours ?? 0m;
			}
		}

		private void CalculateCompanyRow(PRAcaCompanyMonthlyInformation row)
		{
			decimal totalHours = 0m;
			int mecEmployees = 0;
			int fullTimeRecords = 0;
			int ptesWithAcaDeduction = 0;
			(DateTime monthStart, DateTime monthEnd) = GetMonthStartEndDates(row.Year, row.Month.Value);

			MonthFilter oldFilter = EmployeeMonthFilter.Current;
			EmployeeMonthFilter.Current = new MonthFilter() { Month = row.Month };

			row.NumberOfEmployees = Employees.Select(monthStart, monthEnd)
				.Select(x => (PXResult<EPEmployee, PRAcaEmployeeMonthlyInformation, PRPayment, PREarningDetail>)x)
				.Count(x => IsEmployeeActiveInMonth(x, row.Year, row.Month.Value, x));

			foreach (PRAcaEmployeeMonthlyInformation employeeRecord in FilteredEmployeeInformation.Select())
			{
				decimal employeeHours = employeeRecord.HoursWorked ?? 0m;
				if (employeeRecord.FTStatus == AcaFTStatus.FullTime)
				{
					totalHours += employeeHours;
					fullTimeRecords++;
					if (AcaOfferOfCoverage.MeetsMinimumCoverageRequirement(employeeRecord.OfferOfCoverage))
					{
						mecEmployees++;
					}
				}
				else
				{
					totalHours += Math.Min(AcaFTStatus.PartTimeMaxHoursImputed, employeeHours);
					if (EmployeeAcaDeductions.SelectSingle(employeeRecord.EmployeeID, monthStart, monthEnd) != null)
					{
						ptesWithAcaDeduction++;
					}
				}
			}

			row.NumberOfFte = (int)Math.Round(totalHours / AcaFTStatus.FteHours);
			row.PctEmployeesCoveredByMec = fullTimeRecords == 0 ? 0 : (decimal)mecEmployees / fullTimeRecords * 100;
			row.Numberof1095C = fullTimeRecords + ptesWithAcaDeduction;

			EmployeeMonthFilter.Current = oldFilter;
		}

		private string GetDeductionOfferOfCoverage(IEnumerable<PRAcaDeductCoverageInfo> healthPlanTypes, int employeeID, string year)
		{
			AcaOfferOfCoverage.EmployeeAlwaysPartTimeDelegate partTimeDelegate = () =>
			{
				for (int i = 1; i <= 12; i++)
				{
					if (GetFTStatus(employeeID, year, i) == AcaFTStatus.FullTime)
					{
						return false;
					}
				}

				return true;
			};

			return AcaOfferOfCoverage.GetDeductionOfferOfCoverage(healthPlanTypes, partTimeDelegate);
		}

		private string GetFTStatus(int employeeID, string year, int month)
		{
			decimal totalHours = 0m;

			(DateTime startDate, DateTime endDate) = GetMonthStartEndDates(year, month);
			foreach(PXResult<PREarningDetail> earning in EmployeeMonthlyEarnings.Select(employeeID, startDate, endDate))
			{
				totalHours += ((PREarningDetail)earning).Hours ?? 0m;
			}

			decimal weeklyAverage = totalHours / ((decimal)DateTime.DaysInMonth(startDate.Year, month) / 7);
			if (totalHours >= AcaFTStatus.FullTimeMonthlyHourThreshold || weeklyAverage >= AcaFTStatus.FullTimeWeeklyHourThreshold)
			{
				return AcaFTStatus.FullTime;
			}
			return AcaFTStatus.PartTime;
		}

		private (DateTime startDate, DateTime endDate) GetMonthStartEndDates(string year, int month)
		{
			if (!int.TryParse(year, out int yearAsInt))
			{
				throw new PXException(Messages.CantParseYear, year);
			}

			DateTime monthStart = new DateTime(yearAsInt, month, 1);
			DateTime monthEnd = monthStart.AddMonths(1).AddTicks(-1);
			return (monthStart, monthEnd);
		}

		private bool IsEmployeeActiveInMonth(EPEmployee employee, string year, int month, PREarningDetail monthEarning)
		{
			(DateTime monthStart, DateTime monthEnd) = GetMonthStartEndDates(year, month);
			return monthEarning?.RecordID != null || EmployeeActivePositions.Select(employee.BAccountID, monthStart, monthEnd).Any_();
		}
		#endregion Helpers
	}

	[PXHidden]
	public class MonthFilter : IBqlTable
	{
		#region Month
		public abstract class month : PX.Data.BQL.BqlInt.Field<month> { }
		[PXInt]
		[PXUIField(DisplayName = Messages.Month)]
		[Month.List]
		public virtual int? Month { get; set; }
		#endregion
	}
}
