using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public abstract class RegularAmountAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber, 
		IPXFieldDefaultingSubscriber, IPXFieldUpdatedSubscriber
	{
		private bool _UpdateEarningDetailsRate;
		private readonly string _EarningDetailsViewName;
		protected readonly Type _SalariedNonExemptField;
		protected readonly Type _ManualRegularAmountField;
		
		protected RegularAmountAttribute(Type salariedNonExemptField, Type manualRegularAmountField, string earningDetailsViewName)
		{
			_UpdateEarningDetailsRate = true;
			_SalariedNonExemptField = salariedNonExemptField;
			_ManualRegularAmountField = manualRegularAmountField;
			_EarningDetailsViewName = earningDetailsViewName;
		}

		public static void EnforceEarningDetailUpdate<Field>(PXCache cache, object record, bool enforceUpdate) where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			foreach (RegularAmountAttribute attribute in cache.GetAttributesReadonly(fieldName).OfType<RegularAmountAttribute>())
			{
				attribute._UpdateEarningDetailsRate = enforceUpdate;
				if (enforceUpdate)
					attribute.OnRegularAmountUpdated(cache, record);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(_SalariedNonExemptField.DeclaringType, _SalariedNonExemptField.Name, ApplyRegularAmount);
			sender.Graph.FieldUpdated.AddHandler(_ManualRegularAmountField.DeclaringType, _ManualRegularAmountField.Name, ManualUpdateRegularAmount);
			
			sender.Graph.FieldUpdated.AddHandler<PREarningDetail.hours>(EarningDetailHoursUpdated);
			sender.Graph.FieldUpdated.AddHandler<PREarningDetail.typeCD>(EarningDetailTypeUpdated);
			sender.Graph.RowInserted.AddHandler<PREarningDetail>(EarningDetailInserted);
			sender.Graph.RowDeleted.AddHandler<PREarningDetail>(EarningDetailDeleted);
		}
		
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;
			
			bool isVisible = IsRegularRateRequired(sender, e.Row);
			PXUIFieldAttribute.SetVisible(sender, e.Row, _FieldName, isVisible);
			PXUIFieldAttribute.SetVisible(sender, e.Row, _ManualRegularAmountField.Name, isVisible);

			string errorMessage = PXUIFieldAttribute.GetError(sender, e.Row, FieldName);
			if (!string.IsNullOrWhiteSpace(errorMessage))
				sender.RaiseExceptionHandling(FieldName, e.Row, sender.GetValue(e.Row, FieldName), new PXSetPropertyException(errorMessage, PXErrorLevel.Error));
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			OnRegularAmountUpdated(sender, e.Row);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && IsRegularRateRequired(sender, e.Row))
			{
				decimal? regularAmount = GetRegularAmount(sender, e.Row, out string errorMessage);
				e.NewValue = regularAmount;
				PXUIFieldAttribute.SetError(sender, e.Row, FieldName, errorMessage);
			}
		}

		protected virtual void OnRegularAmountUpdated(PXCache sender, object currentRecord)
		{
			if (currentRecord == null)
				return;

			bool isRegularRateRequired = IsRegularRateRequired(sender);
			if (!isRegularRateRequired)
			{
				ApplyStandardPayRate(sender);
				return;
			}

			decimal? newRegularAmount = sender.GetValue(currentRecord, FieldName) as decimal?;
			decimal? standardRegularAmount = GetRegularAmount(sender, currentRecord, out string errorMessage);

			if (standardRegularAmount != null)
			{
				int precision = PRCurrencyAttribute.GetPrecision(sender, currentRecord, FieldName) ?? 2;
				standardRegularAmount = Math.Round(standardRegularAmount.Value, precision, MidpointRounding.AwayFromZero);
			}

			if (standardRegularAmount != newRegularAmount)
				sender.SetValue(currentRecord, _ManualRegularAmountField.Name, true);

			ApplyRegularNonExemptRate(sender, newRegularAmount);
		}

		protected abstract bool IsRegularRateRequired(PXCache sender, object currentRecord = null);

		protected abstract decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage);

		protected abstract decimal? GetStoredRegularAmount(PXCache cache);

		protected virtual void UpdateRegularAmount(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			if (args.Row != null)
				sender.SetDefaultExt(args.Row, FieldName);
		}

		protected virtual decimal? GetRegularAmount(PXCache cache, int? employeeID, string payGroupID, DateTime? startDate, DateTime? endDate, out string errorMessage)
		{
			errorMessage = null;
			if (employeeID == null || payGroupID == null || startDate == null || endDate == null)
				return null;

			PRPayGroupYearSetup payGroupYearSetup =
				SelectFrom<PRPayGroupYearSetup>
					.Where<PRPayGroupYearSetup.payGroupID.IsEqual<P.AsString>>.View
					.SelectSingleBound(cache.Graph, null, payGroupID);
			short numberOfPayPeriods = payGroupYearSetup?.FinPeriods ?? 0;

			if (numberOfPayPeriods < 1)
			{
				errorMessage = PXMessages.LocalizeFormat(Messages.IncorrectNumberOfPayPeriodsInPayGroup, payGroupYearSetup?.PayGroupID);
				return null;
			}

			PRSetup setup = GetPayrollPreferences(cache.Graph);
			string regularHoursType = setup?.RegularHoursType;

			if (string.IsNullOrWhiteSpace(regularHoursType))
			{
				errorMessage = Messages.RegularHoursTypeIsNotSetUpInPayrollPreferences;
				return null;
			}

			PXResult<PREmployeeEarning, PREmployee> employeeEarningSelectResult =
				(PXResult<PREmployeeEarning, PREmployee>)
				SelectFrom<PREmployeeEarning>
					.InnerJoin<PREmployee>.On<PREmployeeEarning.bAccountID.IsEqual<PREmployee.bAccountID>>
					.InnerJoin<EPEarningType>.On<PREmployeeEarning.typeCD.IsEqual<EPEarningType.typeCD>>
					.Where<PREmployeeEarning.isActive.IsEqual<True>
						.And<PREmployeeEarning.bAccountID.IsEqual<P.AsInt>>
						.And<PREmployeeEarning.typeCD.IsEqual<P.AsString>>
						.And<EPEarningType.isOvertime.IsNotEqual<True>>
						.And<PREarningType.isPiecework.IsNotEqual<True>>
						.And<PREarningType.isAmountBased.IsNotEqual<True>>
						.And<PREmployeeEarning.payRate.IsNotNull>
						.And<PREmployeeEarning.startDate.IsLessEqual<P.AsDateTime>>
						.And<PREmployeeEarning.endDate.IsNull
							.Or<PREmployeeEarning.endDate.IsGreaterEqual<P.AsDateTime>>>>.View
					.SelectSingleBound(cache.Graph, null, employeeID, regularHoursType, startDate, endDate);

			if (employeeEarningSelectResult == null)
			{
				errorMessage = Messages.SuitableEmployeeEarningNotFound;
				return null;
			}

			PREmployeeEarning employeeEarning = employeeEarningSelectResult;
			PREmployee currentEmployee = employeeEarningSelectResult;

			decimal prorateCoefficient = 1m;
			if (currentEmployee.StdWeeksPerYear != DateConstants.WeeksPerYear && currentEmployee.StdWeeksPerYear > 0)
				prorateCoefficient = (decimal) DateConstants.WeeksPerYear / currentEmployee.StdWeeksPerYear.Value;

			decimal employeePayRate = employeeEarning.PayRate.GetValueOrDefault();
			switch (employeeEarning.UnitType)
			{
				case UnitType.Year:
					return employeePayRate * prorateCoefficient / numberOfPayPeriods;
				case UnitType.Hour:
					decimal hoursPerYear = GetEmployeeHoursPerYear(cache, currentEmployee);
					return employeePayRate * hoursPerYear * prorateCoefficient / numberOfPayPeriods;
				default:
					errorMessage = Messages.SuitableEmployeeEarningNotFound;
					return null;
			}
		}

		protected virtual void ApplyRegularAmount(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			if (args.Row == null)
				return;

			if (sender.GetValue(args.Row, _SalariedNonExemptField.Name) as bool? == true)
			{
				sender.SetDefaultExt(args.Row, FieldName);
			}
			else
			{
				sender.SetValueExt(args.Row, _ManualRegularAmountField.Name, false);
				sender.SetValueExt(args.Row, FieldName, null);
			}
		}

		protected virtual void ManualUpdateRegularAmount(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			if (args.Row == null || !IsRegularRateRequired(sender) || args.OldValue as bool? != true)
				return;

			sender.SetDefaultExt(args.Row, FieldName);
		}

		protected virtual void EarningDetailHoursUpdated(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			PREarningDetail row = args.Row as PREarningDetail;
			if (row == null || row.IsRegularRate != true || !IsRegularRateRequired(sender))
				return;

			decimal? oldRegularHoursQty = args.OldValue as decimal?;
			decimal? newRegularHoursQty = row.Hours;

			if (newRegularHoursQty == oldRegularHoursQty)
				return;

			OnEarningDetailsModification(sender, row.RecordID);
		}

		protected virtual void EarningDetailTypeUpdated(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			PREarningDetail row = args.Row as PREarningDetail;
			if (row == null || !IsRegularRateRequired(sender))
				return;

			bool? oldIsRegularRate = row.IsRegularRate;
			row.IsRegularRate = IsRegularRateEarningDetail(row);

			if (oldIsRegularRate == row.IsRegularRate)
				return;

			OnEarningDetailsModification(sender, row.RecordID);
		}

		protected virtual void EarningDetailInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PREarningDetail row = e.Row as PREarningDetail;
			if (row == null || !IsRegularRateRequired(sender))
				return;

			row.IsRegularRate = IsRegularRateEarningDetail(row);

			if (row.IsRegularRate == true)
				OnEarningDetailsModification(sender);
		}

		protected virtual void EarningDetailDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			PREarningDetail row = e.Row as PREarningDetail;
			if (row == null || !IsRegularRateRequired(sender))
				return;

			if (row.IsRegularRate == true)
				OnEarningDetailsModification(sender);
		}

		protected virtual void ApplyStandardPayRate(PXCache sender)
		{
			PXView earningDetailsView = GetEarningDetailsView(sender);
			if (earningDetailsView == null)
				return;

			PXCache earningDetailsCache = earningDetailsView.Cache;
			foreach (object record in earningDetailsView.SelectMulti())
			{
				PREarningDetail earningDetail = GetEarningDetailFromViewRecord(record);

				if (earningDetail == null || earningDetail.IsRegularRate == false)
					continue;

				earningDetail.IsRegularRate = false;
				earningDetailsCache.Update(earningDetail);
			}
		}

		protected virtual void ApplyRegularNonExemptRate(PXCache sender, decimal? newRegularAmount)
		{
			UpdateRegularEarningDetails(sender, true, newRegularAmount);
		}

		protected virtual void OnEarningDetailsModification(PXCache sender, int? recordIDToSkip = null)
		{
			UpdateRegularEarningDetails(sender, false, null, recordIDToSkip);
		}

		protected virtual void UpdateRegularEarningDetails(PXCache sender, bool regularRateUpdated, decimal? newRegularAmount, int? recordIDToSkip = null)
		{
			if (!_UpdateEarningDetailsRate)
				return;
			
			PXView earningDetailsView = GetEarningDetailsView(sender);
			if (earningDetailsView == null)
				return;

			List<PREarningDetail> earningDetailsToUpdate = new List<PREarningDetail>();
			decimal regularAmount = (regularRateUpdated ? newRegularAmount : GetStoredRegularAmount(sender)).GetValueOrDefault();
			decimal regularHoursQty = 0;
			int lastRecordWithNonZeroHoursIndex = -1;

			foreach (var record in earningDetailsView.SelectMulti())
			{
				PREarningDetail earningDetail = GetEarningDetailFromViewRecord(record);

				if (earningDetail != null &&
					(regularRateUpdated && IsRegularRateEarningDetail(earningDetail) ||
					!regularRateUpdated && earningDetail.IsRegularRate == true))
				{
					decimal earningDetailHours = earningDetail.Hours.GetValueOrDefault();
					
					regularHoursQty += earningDetailHours;
					earningDetailsToUpdate.Add(earningDetail);

					if (earningDetailHours > 0)
						lastRecordWithNonZeroHoursIndex = earningDetailsToUpdate.Count - 1;
				}
			}

			decimal hourlyRate = (regularHoursQty != 0) ? regularAmount / regularHoursQty : 0m;

			for (var index = 0; index < earningDetailsToUpdate.Count; index++)
			{
				PREarningDetail earningDetail = earningDetailsToUpdate[index];

				if (regularRateUpdated)
					earningDetail.IsRegularRate = true;

				//Last record with non-zero hours gets the reminder to ensure that TotalAmount is equal to the sum of all regular EarningDetail.Amounts
				if (index == lastRecordWithNonZeroHoursIndex)
					hourlyRate = regularAmount / earningDetail.Hours.GetValueOrDefault();

				earningDetail.Rate = hourlyRate;

				if (earningDetail.RecordID != recordIDToSkip)
					earningDetailsView.Cache.Update(earningDetail);
				else
					sender.SetValueExt<PREarningDetail.rate>(earningDetail, hourlyRate);

				regularAmount -= earningDetail.Amount.GetValueOrDefault();
			}

			if (!regularRateUpdated)
				earningDetailsView.RequestRefresh();
		}

		protected virtual PXView GetEarningDetailsView(PXCache sender)
		{
			sender.Graph.Views.TryGetValue(_EarningDetailsViewName, out PXView earningDetailsView);
			return earningDetailsView;
		}

		protected virtual bool IsRegularRateEarningDetail(PREarningDetail row)
		{
			return row.TypeCD != null && row.IsOvertime != true && row.IsPiecework != true && row.IsAmountBased != true && row.IsFringeRateEarning != true;
		}

		protected virtual PREarningDetail GetEarningDetailFromViewRecord(object viewRecord)
		{
			switch (viewRecord)
			{
				case PREarningDetail earningDetailRecord:
					return earningDetailRecord;
				case PXResult<PREarningDetail> pxResultRecord:
					return pxResultRecord[0] as PREarningDetail;
			}

			return null;
		}

		protected virtual PRSetup GetPayrollPreferences(PXGraph graph)
		{
			return graph.Caches[typeof(PRSetup)]?.Current as PRSetup ?? new SelectFrom<PRSetup>.View(graph).SelectSingle();
		}

		protected virtual decimal GetEmployeeHoursPerYear(PXCache cache, PREmployee currentEmployee)
		{
			cache.Graph.Caches[typeof(PREmployee)].Locate(currentEmployee);
			object hoursPerYearField = cache.Graph.Caches[typeof(PREmployee)].GetValueExt<PREmployee.hoursPerYear>(currentEmployee);
			decimal hoursPerYear = hoursPerYearField is PXFieldState hoursPerYearFieldState
				? hoursPerYearFieldState.Value as decimal? ?? 0m
				: hoursPerYearField as decimal? ?? 0m;

			return hoursPerYear;
		}
	}

	public class BatchEmployeeRegularAmountAttribute : RegularAmountAttribute
	{
		public BatchEmployeeRegularAmountAttribute() : 
			base(typeof(PRBatchEmployee.salariedNonExempt), typeof(PRBatchEmployee.manualRegularAmount), nameof(PRPayBatchEntry.EarningDetails))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler<PRBatchEmployee.employeeID>(UpdateRegularAmount);
		}

		protected override bool IsRegularRateRequired(PXCache sender, object currentRecord = null)
		{
			PRBatchEmployee batchEmployee = currentRecord != null
				? currentRecord as PRBatchEmployee
				: sender.Graph.Caches[typeof(PRBatchEmployee)].Current as PRBatchEmployee;

			return batchEmployee?.EmpType == EmployeeType.SalariedNonExempt && batchEmployee.SalariedNonExempt == true;
		}

		protected override decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage)
		{
			int? employeeID = cache.GetValue<PRBatchEmployee.employeeID>(currentRecord) as int?;
			PRBatch payrollBatch = cache.Graph.Caches[typeof(PRBatch)].Current as PRBatch ?? PXParentAttribute.SelectParent<PRBatch>(cache, currentRecord);
			string payGroupID = payrollBatch?.PayGroupID;
			DateTime? startDate = payrollBatch?.StartDate;
			DateTime? endDate = payrollBatch?.EndDate;

			return GetRegularAmount(cache, employeeID, payGroupID, startDate, endDate, out errorMessage);
		}

		protected override decimal? GetStoredRegularAmount(PXCache cache)
		{
			return (cache.Graph.Caches[typeof(PRBatchEmployee)].Current as PRBatchEmployee)?.RegularAmount;
		}
	}

	public class PaymentRegularAmountAttribute : RegularAmountAttribute
	{
		public PaymentRegularAmountAttribute(string earningDetailsViewName) : 
			base(typeof(PRPayment.salariedNonExempt), typeof(PRPayment.manualRegularAmount), earningDetailsViewName)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler<PRPayment.employeeID>(UpdateRegularAmount);
			sender.Graph.FieldUpdated.AddHandler<PRPayment.payGroupID>(UpdateRegularAmount);
			sender.Graph.FieldUpdated.AddHandler<PRPayment.payPeriodID>(UpdateRegularAmount);
		}

		protected override bool IsRegularRateRequired(PXCache sender, object currentRecord = null)
		{
			PRPayment payment = GetPayment(sender, currentRecord);

			return payment?.EmpType == EmployeeType.SalariedNonExempt && 
					payment.SalariedNonExempt == true &&
					payment.DocType != PayrollType.VoidCheck;
		}

		protected override decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage)
		{
			PRPayment payment = GetPayment(cache, currentRecord);
			int? employeeID = payment?.EmployeeID;
			string payGroupID = payment?.PayGroupID;
			DateTime? startDate = payment?.StartDate;
			DateTime? endDate = payment?.EndDate;

			return GetRegularAmount(cache, employeeID, payGroupID, startDate, endDate, out errorMessage);
		}

		protected override decimal? GetStoredRegularAmount(PXCache cache)
		{
			return GetPayment(cache)?.RegularAmount;
		}

		protected virtual PRPayment GetPayment(PXCache cache, object currentRecord = null)
		{
			return currentRecord as PRPayment ?? cache.Graph.Caches[typeof(PRPayment)].Current as PRPayment;
		}
	}
}
