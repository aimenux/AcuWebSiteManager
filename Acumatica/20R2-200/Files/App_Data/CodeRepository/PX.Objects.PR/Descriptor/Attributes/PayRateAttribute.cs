using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PayRateAttribute : PXBaseConditionAttribute, IPXRowSelectedSubscriber, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber, IPXFieldVerifyingSubscriber, IPXFieldSelectingSubscriber
	{
		private const decimal ZeroRate = 0m;
		private const int RegularRateDecimalPlaces = 6;

		private Type _EnableCondition;

		public PayRateAttribute(Type enableCondition)
		{
			_EnableCondition = enableCondition;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			PRSetup setup = GetPayrollPreferences(sender.Graph);
			if (setup?.PayRateDecimalPlaces != null)
				PXDBDecimalAttribute.SetPrecision(sender, FieldName, setup?.PayRateDecimalPlaces);
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PREarningDetail currentRecord = e.Row as PREarningDetail;

			if (currentRecord == null || currentRecord.IsRegularRate != true)
				return;

			e.ReturnValue = null;
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetWarning<PREarningDetail.rate>(sender, e.Row, null);
			PREarningDetail currentRecord = e.Row as PREarningDetail;
			if (currentRecord == null || currentRecord.IsAmountBased == true)
				return;

			decimal? payRate = e.NewValue as decimal?;
			if (payRate > 0)
				return;

			string errorMessage;
			if (payRate == 0)
				errorMessage = Messages.ZeroPayRate;
			else if (payRate < 0)
				errorMessage = Messages.NegativePayRate;
			else
				errorMessage = Messages.EmptyPayRate;

			sender.RaiseExceptionHandling<PREarningDetail.rate>(e.Row, e.NewValue, 
				new PXSetPropertyException(errorMessage, PXErrorLevel.Warning));
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PREarningDetail row = e.Row as PREarningDetail;
			
			if (row == null)
				return;

			int? decimalPlaces = row.IsRegularRate == true ? RegularRateDecimalPlaces : GetPayrollPreferences(sender.Graph)?.PayRateDecimalPlaces;

			if (decimalPlaces != null)
				PXDBDecimalAttribute.SetPrecision(sender, row, FieldName, decimalPlaces);
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!GetConditionResult(sender, e.Row, _EnableCondition))
			{
				return;
			}

			PREarningDetail oldRow = e.OldRow as PREarningDetail;
			PREarningDetail newRow = e.Row as PREarningDetail;

			if (newRow == null)
			{
				return;
			}

			if (e.ExternalCall && !sender.ObjectsEqual<PREarningDetail.rate>(oldRow, newRow))
			{
				if (newRow.IsRegularRate != true)
				{
				newRow.ManualRate = true;
				return;
			}

				newRow.ManualRate = false;
			}

			//The Rate should not be updated if the fields the Rate depends on were not updated.
			if (oldRow != null &&
				sender.ObjectsEqual<
					PREarningDetail.manualRate, 
					PREarningDetail.employeeID, 
					PREarningDetail.typeCD, 
					PREarningDetail.date,
					PREarningDetail.labourItemID, 
					PREarningDetail.projectID,
					PREarningDetail.projectTaskID,
					PREarningDetail.unionID>(oldRow, newRow) &&
				sender.ObjectsEqual<
					PREarningDetail.isRegularRate>(oldRow, newRow))
			{
				return;
			}

			SetRate(sender, newRow);
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (!GetConditionResult(sender, e.Row, _EnableCondition))
			{
				return;
			}

			SetRate(sender, e.Row as PREarningDetail);
		}

		private PRSetup GetPayrollPreferences(PXGraph graph)
		{
            PXCache setupCache = graph.Caches[typeof(PRSetup)];
            PRSetup payrollPreferences = setupCache?.Current as PRSetup ??
                new SelectFrom<PRSetup>.View(graph).SelectSingle();

            return payrollPreferences;
		}

		private void SetRate(PXCache sender, PREarningDetail currentRecord)
		{
			if (currentRecord == null || currentRecord.ManualRate == true || currentRecord.PaymentDocType == PayrollType.VoidCheck)
				return;

			if (currentRecord.SourceType == EarningDetailSourceType.SalesCommission)
				return;

			if (currentRecord.IsAmountBased != true)
			{
				if (currentRecord.IsRegularRate != true) 
				sender.SetValueExt<PREarningDetail.rate>(currentRecord, GetMaxRate(sender.Graph, currentRecord));
				sender.SetDefaultExt<PREarningDetail.amount>(currentRecord);
			}
			else
			{
				currentRecord.Rate = null;
			}
		}

		private decimal GetMaxRate(PXGraph graph, PREarningDetail currentRecord)
		{
			decimal employeeEarningRate = GetEmployeeEarningRate(graph, currentRecord);
			if (currentRecord.UnitType == UnitType.Misc)
				return employeeEarningRate;

			decimal employeeLaborCostRate = GetEmployeeLaborCostRate(graph, currentRecord);
			decimal unionLocalRate = GetUnionLocalRate(graph, currentRecord);
			decimal certifiedProjectRate = GetCertifiedProjectRate(graph, currentRecord);

			return Math.Max(
				Math.Max(employeeEarningRate, employeeLaborCostRate),
				Math.Max(unionLocalRate, certifiedProjectRate));
		}

		private decimal GetEmployeeEarningRate(PXGraph graph, PREarningDetail earningDetailRecord)
		{
			string earningTypeCD = earningDetailRecord.TypeCD;
			EPEarningType earningDetailType = GetEarningTypeRecord(graph, earningTypeCD);
			PREarningType prEarningDetailType = earningDetailType?.GetExtension<PREarningType>();

			if (earningDetailType?.IsOvertime == true || prEarningDetailType?.IsPTO == true)
			{
				if (prEarningDetailType == null)
					PXTrace.WriteWarning(Messages.EarningTypeNotFound, earningTypeCD, typeof(PREarningType).Name);

				if (string.IsNullOrWhiteSpace(prEarningDetailType?.RegularTypeCD))
					return ZeroRate;

				earningTypeCD = prEarningDetailType.RegularTypeCD;
			}
			
			PXResult<PREmployeeEarning, PREmployee> employeeEarningQuery = 
				(PXResult<PREmployeeEarning, PREmployee>)
				SelectFrom<PREmployeeEarning>.
				InnerJoin<PREmployee>.On<PREmployeeEarning.bAccountID.IsEqual<PREmployee.bAccountID>>.
				Where<PREmployeeEarning.isActive.IsEqual<True>.
					And<PREmployeeEarning.bAccountID.IsEqual<PREarningDetail.employeeID.FromCurrent>>.
					And<PREmployeeEarning.typeCD.IsEqual<P.AsString>>.
					And<PREmployeeEarning.startDate.IsLessEqual<PREarningDetail.date.FromCurrent>.
						And<PREmployeeEarning.endDate.IsNull.
							Or<PREmployeeEarning.endDate.IsGreaterEqual<PREarningDetail.date.FromCurrent>>>>>.
				OrderBy<PREmployeeEarning.startDate.Desc>.View.
				SelectSingleBound(graph, new object[] { earningDetailRecord }, earningTypeCD);

			PREmployeeEarning employeeEarning = employeeEarningQuery;

			if (employeeEarning == null || employeeEarning.PayRate == null)
				return ZeroRate;

			decimal currentPayRate = employeeEarning.PayRate.Value;

			if (earningDetailType?.IsOvertime == true && earningDetailType?.OvertimeMultiplier > 0)
				currentPayRate *= earningDetailType.OvertimeMultiplier.Value;

			if (employeeEarning.UnitType != UnitType.Year)
				return currentPayRate;

			PREmployee currentEmployee = employeeEarningQuery;

			decimal hoursPerYear = currentEmployee?.HoursPerYear ?? 0m;
			if (hoursPerYear == 0)
				return ZeroRate;

			return currentPayRate / hoursPerYear;
		}

		private decimal GetEmployeeLaborCostRate(PXGraph graph, PREarningDetail earningDetail)
		{
			PMLaborCostRate employeeLaborCostRate =
				SelectFrom<PMLaborCostRate>.
				Where<PMLaborCostRate.employeeID.IsEqual<PREarningDetail.employeeID.FromCurrent>.
					And<PMLaborCostRate.type.IsEqual<PMLaborCostRateType.employee>>.
					And<PMLaborCostRate.inventoryID.IsNull.Or<PMLaborCostRate.inventoryID.IsEqual<PREarningDetail.labourItemID.FromCurrent>>>.
					And<PMLaborCostRate.effectiveDate.IsLessEqual<PREarningDetail.date.FromCurrent>>>.
				OrderBy<PMLaborCostRate.effectiveDate.Desc>.View.
				SelectSingleBound(graph, new object[] { earningDetail });

			if (employeeLaborCostRate?.Rate == null)
				return ZeroRate;

			return employeeLaborCostRate.Rate.Value * (GetOvertimeMultiplier(graph, earningDetail) ?? 1);
		}

		private decimal GetUnionLocalRate(PXGraph graph, PREarningDetail earningDetail)
		{
			if (earningDetail.UnionID == null || earningDetail.LabourItemID == null)
				return ZeroRate;

			PMLaborCostRate unionLocalRate =
				SelectFrom<PMLaborCostRate>.
				Where<PMLaborCostRate.inventoryID.IsEqual<PREarningDetail.labourItemID.FromCurrent>.
					And<PMLaborCostRate.effectiveDate.IsLessEqual<PREarningDetail.date.FromCurrent>>.
					And<PMLaborCostRate.employeeID.IsNull.Or<PMLaborCostRate.employeeID.IsEqual<PREarningDetail.employeeID.FromCurrent>>>.
					And<PMLaborCostRate.unionID.IsEqual<PREarningDetail.unionID.FromCurrent>>>.
				OrderBy<PMLaborCostRate.effectiveDate.Desc>.View.
				SelectSingleBound(graph, new object[] { earningDetail });

			if (unionLocalRate?.Rate == null)
				return ZeroRate;

			return unionLocalRate.Rate.Value * (GetOvertimeMultiplier(graph, earningDetail) ?? 1);
		}

		private decimal GetCertifiedProjectRate(PXGraph graph, PREarningDetail earningDetail)
		{
			if (earningDetail.ProjectID == null || earningDetail.CertifiedJob != true || earningDetail.LabourItemID == null)
				return ZeroRate;

			PREmployee employee =
				SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<PREarningDetail.employeeID.FromCurrent>>.View.
				SelectSingleBound(graph, new object[] { earningDetail });

			if (employee?.ExemptFromCertifiedReporting == true)
				return ZeroRate;

			PMLaborCostRate certifiedProjectRate =
				SelectFrom<PMLaborCostRate>.
				Where<PMLaborCostRate.inventoryID.IsEqual<PREarningDetail.labourItemID.FromCurrent>.
					And<PMLaborCostRate.effectiveDate.IsLessEqual<PREarningDetail.date.FromCurrent>>.
					And<PMLaborCostRate.employeeID.IsNull.Or<PMLaborCostRate.employeeID.IsEqual<PREarningDetail.employeeID.FromCurrent>>>.
					And<PMLaborCostRate.projectID.IsEqual<PREarningDetail.projectID.FromCurrent>.
					And<PMLaborCostRate.type.IsEqual<PMLaborCostRateType.certified>>.
					And<PMLaborCostRate.taskID.IsNull.Or<PMLaborCostRate.taskID.IsEqual<PREarningDetail.projectTaskID.FromCurrent>>>>>.
				OrderBy<PMLaborCostRate.effectiveDate.Desc>.View.
				SelectSingleBound(graph, new[] { earningDetail });

			if (certifiedProjectRate?.Rate == null)
				return ZeroRate;

			return certifiedProjectRate.Rate.Value * (GetOvertimeMultiplier(graph, earningDetail) ?? 1);
		}

		private decimal? GetOvertimeMultiplier(PXGraph graph, PREarningDetail earningDetail)
		{
			if (earningDetail.IsOvertime != true)
			{
				return null;
			}

			EPEarningType overTimeEarningType = GetEarningTypeRecord(graph, earningDetail.TypeCD);

			return overTimeEarningType?.OvertimeMultiplier;
		}

		private EPEarningType GetEarningTypeRecord(PXGraph graph, string typeCD)
		{
			EPEarningType record = SelectFrom<EPEarningType>.
				Where<EPEarningType.isActive.IsEqual<True>.
					And<EPEarningType.typeCD.IsEqual<P.AsString>>>.View.SelectSingleBound(graph, null, typeCD);

			if (record == null)
			{
				PXTrace.WriteWarning(Messages.EarningTypeNotFound, typeCD, typeof(EPEarningType).Name);
			}

			return record;
		}
	}
}
