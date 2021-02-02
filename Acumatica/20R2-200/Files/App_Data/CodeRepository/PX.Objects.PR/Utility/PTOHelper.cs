using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PTOHelper
	{
		public static void GetPTOBankYear(DateTime targetDate, DateTime bankStartDate, out DateTime startDate, out DateTime endDate)
		{
			startDate = new DateTime(targetDate.Year, bankStartDate.Month, bankStartDate.Day);
			if (startDate > targetDate)
			{
				startDate = startDate.AddYears(-1);
			}
			endDate = startDate.AddYears(1).AddTicks(-1);
		}

		public static IPTOBank GetSourceBank(PRPTOBank bank, PREmployeeClassPTOBank classBank, PREmployeePTOBank employeeBank)
		{
			IPTOBank sourceBank;
			if (employeeBank != null)
			{
				if (employeeBank.UseClassDefault == true)
				{
					sourceBank = classBank;
				}
				else
				{
					sourceBank = employeeBank;
				}
			}
			else
			{
				sourceBank = bank;
			}

			return sourceBank;
		}

		/// <summary>
		/// Returns amount that should be carried over from last year's PTO.
		/// </summary>
		public static decimal CalculateAmountToCarryover(PXGraph graph, int? employeeID, IPTOBank sourceBank, DateTime startDate, DateTime endDate)
		{
			IEnumerable<PRPaymentPTOBank> pastYearHistory;
			decimal? carryoverAmount = null;
			switch (sourceBank.CarryoverType)
			{
				case CarryoverType.Total:
				case CarryoverType.PaidOnTimeLimit:
					pastYearHistory = PTOHelper.EmployeePTOHistorySelect.View.Select(graph, employeeID, startDate.AddYears(-1), endDate.AddYears(-1), sourceBank.BankID).FirstTableItems;
					carryoverAmount = pastYearHistory.Sum(x => x.TotalAccrual.GetValueOrDefault() - x.TotalDisbursement.GetValueOrDefault());
					break;
				case CarryoverType.Partial:
					pastYearHistory = PTOHelper.EmployeePTOHistorySelect.View.Select(graph, employeeID, startDate.AddYears(-1), endDate.AddYears(-1), sourceBank.BankID).FirstTableItems;
					decimal acucmulatedPTO = pastYearHistory.Sum(x => x.TotalAccrual.GetValueOrDefault() - x.TotalDisbursement.GetValueOrDefault());
					carryoverAmount = Math.Min(acucmulatedPTO, sourceBank.CarryoverAmount.GetValueOrDefault());
					break;
				case CarryoverType.None:
				default:
					break;
			}

			return carryoverAmount ?? 0;
		}

		/// <summary>
		/// Calculate accumulated, used and available PTO amount at specified time, for an employee and a bank.
		/// Make sure you pass the right IPTOBank for your needs, either the PTO bank itself, the Class bank or the Employee Bank.
		/// </summary>
		public static void GetPTOHistory(PXGraph graph, DateTime targetDate, int employeeID, IPTOBank bank, out decimal accumulated, out decimal used, out decimal available)
		{
			VerifyBankIsValid(bank);

			accumulated = 0;
			used = 0;
			available = 0;
			PTOHelper.GetPTOBankYear(targetDate, bank.StartDate.Value, out DateTime startDate, out DateTime endDate);
			IEnumerable<PRPaymentPTOBank> history = EmployeePTOHistorySelect.View.Select(graph, employeeID, startDate, endDate, bank.BankID).FirstTableItems;

			accumulated = history.Sum(x => x.TotalAccrual.GetValueOrDefault());
			used = history.Sum(x => x.TotalDisbursement.GetValueOrDefault());
			if (bank.DisburseFromCarryover == true)
			{
				available = history.Sum(x => x.CarryoverAmount.GetValueOrDefault());
			}
			else
			{
				available = accumulated;
			}

			available -= used;
		}

		public static PTOYearSummary GetPTOYearSummary(PXGraph graph, DateTime targetDate, int employeeID, IPTOBank bank)
		{
			GetPTOBankYear(targetDate, bank.StartDate.Value, out DateTime startDate, out DateTime endDate);
			var results = EmployeePTOHistorySelect.View.Select(graph, employeeID, startDate, endDate, bank.BankID);
			var history = results.Select(x => (PXResult<PRPaymentPTOBank, PRPayment>)x).ToList();

			var summary = new PTOYearSummary();
			summary.StartDate = startDate;
			summary.EndDate = endDate;
			summary.AccrualAmount = history.Sum(x => ((PRPaymentPTOBank)x).AccrualAmount.GetValueOrDefault());
			summary.DisbursementAmount = history.Sum(x => ((PRPaymentPTOBank)x).DisbursementAmount.GetValueOrDefault());
			summary.FrontLoadingAmount = history.Sum(x => ((PRPaymentPTOBank)x).FrontLoadingAmount.GetValueOrDefault());
			summary.CarryoverAmount = history.Sum(x => ((PRPaymentPTOBank)x).CarryoverAmount.GetValueOrDefault());
			summary.PaidCarryoverAmount = history.Sum(x => ((PRPaymentPTOBank)x).PaidCarryoverAmount.GetValueOrDefault());

			summary.ProcessedFrontLoading = history.Any(x => ((PRPaymentPTOBank)x).ProcessedFrontLoading == true && ((PRPayment)x).Voided == false && ((PRPayment)x).DocType != PayrollType.VoidCheck);
			summary.ProcessedCarryover = history.Any(x => ((PRPaymentPTOBank)x).ProcessedCarryover == true && ((PRPayment)x).Voided == false && ((PRPayment)x).DocType != PayrollType.VoidCheck);
			summary.ProcessedPaidCarryover = history.Any(x => ((PRPaymentPTOBank)x).ProcessedPaidCarryover == true && ((PRPayment)x).Voided == false && ((PRPayment)x).DocType != PayrollType.VoidCheck);

			return summary;
		}

		protected static void VerifyBankIsValid(IPTOBank bank)
		{
			if (bank.StartDate == null)
			{
				throw new PXException(Messages.InvalidBankStartDate);
			}
		}

		public static bool IsFirstRegularPaycheckOfYear(PXGraph graph, DateTime startDate, DateTime endDate)
		{
			return SelectFrom<PRPayment>.Where<PRPayment.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
					.And<PRPayment.docType.IsEqual<PayrollType.regular>>
					.And<PRPayment.transactionDate.IsBetween<P.AsDateTime, P.AsDateTime>>>.View
					.Select(graph, startDate, endDate).Count == 1;
		}

		public class EmployeePTOHistorySelect : SelectFrom<PRPaymentPTOBank>
		.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<PRPaymentPTOBank.docType>
			.And<PRPayment.refNbr.IsEqual<PRPaymentPTOBank.refNbr>>>
		.Where<PRPayment.employeeID.IsEqual<P.AsInt>
			.And<PRPayment.transactionDate.IsGreaterEqual<P.AsDateTime>>
			.And<PRPayment.transactionDate.IsLess<P.AsDateTime>>
			.And<PRPaymentPTOBank.bankID.IsEqual<P.AsString>>
			.And<PRPayment.released.IsEqual<True>>>
		{ }

		public class PTOBankSelect : SelectFrom<PRPTOBank>
			.InnerJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<P.AsInt>>
			.LeftJoin<PREmployeeClassPTOBank>.On<PREmployeeClassPTOBank.bankID.IsEqual<PRPTOBank.bankID>
				   .And<PREmployeeClassPTOBank.employeeClassID.IsEqual<PREmployee.employeeClassID>>>
			.LeftJoin<PREmployeePTOBank>.On<PREmployeePTOBank.bankID.IsEqual<PRPTOBank.bankID>
				   .And<PREmployeePTOBank.bAccountID.IsEqual<PREmployee.bAccountID>>>
			.Where<PRPTOBank.bankID.IsEqual<P.AsString>>
		{ }

		public class PTOYearSummary : IPTOHistory
		{
			public DateTime StartDate { get; set; }

			public DateTime EndDate { get; set; }

			public decimal? AccrualAmount { get; set; }

			public decimal? DisbursementAmount { get; set; }

			public bool? ProcessedFrontLoading { get; set; }

			public decimal? FrontLoadingAmount { get; set; }

			public bool? ProcessedCarryover { get; set; }

			public decimal? CarryoverAmount { get; set; }

			public bool? ProcessedPaidCarryover { get; set; }

			public decimal? PaidCarryoverAmount { get; set; }

			public decimal TotalIncreasedAmount => AccrualAmount.GetValueOrDefault() + FrontLoadingAmount.GetValueOrDefault() + CarryoverAmount.GetValueOrDefault();

			public decimal TotalDecreasedAmount => DisbursementAmount.GetValueOrDefault() + PaidCarryoverAmount.GetValueOrDefault();

			public decimal Balance => TotalIncreasedAmount - TotalDecreasedAmount;
		}

		public interface IPTOHistory
		{
			decimal? AccrualAmount { get; set; }

			decimal? DisbursementAmount { get; set; }

			bool? ProcessedFrontLoading { get; set; }

			decimal? FrontLoadingAmount { get; set; }

			bool? ProcessedCarryover { get; set; }

			decimal? CarryoverAmount { get; set; }

			bool? ProcessedPaidCarryover { get; set; }

			decimal? PaidCarryoverAmount { get; set; }
		}
	}
}
