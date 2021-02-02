using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PTOBankMaint : PXGraph<PTOBankMaint, PRPTOBank>
	{
		public SelectFrom<PRPTOBank>.View Bank;
		public SelectFrom<PRPTOBank>.Where<PRPTOBank.bankID.IsEqual<PRPTOBank.bankID.FromCurrent>>.View CurrentBank;
		public SelectFrom<PRPaymentPTOBank>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentPTOBank.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentPTOBank.docType>>>
			.Where<PRPayment.printed.IsEqual<False>
				.And<PRPayment.released.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPaymentPTOBank.bankID.IsEqual<P.AsString>>>.View EditablePaymentPTOBanks;

		public void _(Events.FieldUpdated<PRPTOBank.carryoverType> e)
		{
			var row = (PRPTOBank)e.Row;
			if (row == null)
			{
				return;
			}

			if (row.CarryoverType != CarryoverType.Partial)
			{
				row.CarryoverAmount = 0;
			}
		}

		protected virtual void _(Events.FieldUpdated<PRPTOBank.isActive> e)
		{
			PRPTOBank row = e.Row as PRPTOBank;
			if (row == null)
			{
				return;
			}

			if (!e.NewValue.Equals(true))
			{
				foreach (PRPaymentPTOBank paymentPTOBank in EditablePaymentPTOBanks.Select(row.BankID))
				{
					paymentPTOBank.IsActive = false;
					paymentPTOBank.AccrualAmount = 0m;
					EditablePaymentPTOBanks.Update(paymentPTOBank);
				}
			}
		}
		
		public void _(Events.FieldUpdated<PRPTOBank.allowNegativeBalance> e)
		{
			var row = (PRPTOBank)e.Row;
			if (row == null || e.NewValue.Equals(false))
			{
				return;
			}

			if(row.DisburseFromCarryover == true)
			{
				row.DisburseFromCarryover = false;
				PXUIFieldAttribute.SetWarning<PRPTOBank.disburseFromCarryover>(e.Cache, row, Messages.CantUseSimultaneously);
			}
		}

		public void _(Events.FieldUpdated<PRPTOBank.disburseFromCarryover> e)
		{
			var row = (PRPTOBank)e.Row;
			if (row == null || e.NewValue.Equals(false))
			{
				return;
			}

			if (row.AllowNegativeBalance == true)
			{
				row.AllowNegativeBalance = false;
				PXUIFieldAttribute.SetWarning<PRPTOBank.allowNegativeBalance>(e.Cache, row, Messages.CantUseSimultaneously);
			}
		}

		public void _(Events.RowPersisting<PRPTOBank> e)
		{
			bool? originalValue = (bool?)e.Cache.GetValueOriginal<PRPTOBank.allowNegativeBalance>(e.Row);
			if (e.Row.AllowNegativeBalance == false && e.Operation != PXDBOperation.Delete
				&& originalValue != false)
			{
				IEnumerable<string> negativeBalanceEmployees = GetNegativeBalanceEmployees(e.Row);
				if (negativeBalanceEmployees.Any())
				{
					var errorMessage = string.Format(Messages.NegativePTOBalanceError, string.Join(",", negativeBalanceEmployees));
					PXUIFieldAttribute.SetError<PRPTOBank.allowNegativeBalance>(e.Cache, e.Row, errorMessage);
					e.Row.AllowNegativeBalance = true;
				}
			}
		}

		protected IEnumerable<string> GetNegativeBalanceEmployees(PRPTOBank row)
		{
			var paymentsGroupedByEmployee =
				SelectFrom<PRPaymentPTOBank>
					.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<PRPaymentPTOBank.docType>
						.And<PRPayment.refNbr.IsEqual<PRPaymentPTOBank.refNbr>>>
					.InnerJoin<BAccount>.On<BAccount.bAccountID.IsEqual<PRPayment.employeeID>>
					.Where<PRPaymentPTOBank.bankID.IsEqual<PRPTOBank.bankID.FromCurrent>
						.And<PRPayment.released.IsEqual<True>>
						.And<PRPayment.voided.IsEqual<False>>>
					.OrderBy<PRPayment.transactionDate.Desc>
					.View.Select(this)
					.Cast<PXResult<PRPaymentPTOBank, PRPayment, BAccount>>()
					.GroupBy(x => ((PRPayment)x).EmployeeID);

			var negativeBalanceEmployees = new List<string>();
			foreach (IGrouping<int?, PXResult<PRPaymentPTOBank, PRPayment, BAccount>> employeePayments in paymentsGroupedByEmployee)
			{
				var latestResult = employeePayments.First();
				PRPaymentPTOBank paymentBank = latestResult;
				if (paymentBank.UsedAmount > paymentBank.AvailableAmount)
				{
					BAccount employee = latestResult;
					negativeBalanceEmployees.Add(employee.AcctCD);
				}
			}

			return negativeBalanceEmployees;
		}
	}
}