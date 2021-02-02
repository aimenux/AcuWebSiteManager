using PX.Data;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRPayGroupPeriodIDAttribute : PeriodIDAttribute, IPXFieldVerifyingSubscriber, IPXFieldUpdatedSubscriber, IPXRowSelectedSubscriber
	{
		private const string PRPeriodIDDisplayMask = "CC-####";

		protected int _SelAttrIndex = -1;

		private Type _PayGroupField;
		private Type _StartDateField;
		private Type _EndDateField;
		private Type _EndDateUIField;
		private Type _TransactionDateField;
		private bool _IsDateChangePossible;
		private Type _TransactionDateEnabledCondition;
		private bool _ApplyConditionToUnsavedRowsOnly;

		#region Ctor
		public PRPayGroupPeriodIDAttribute(
			Type payGroupField, 
			Type startDateField, 
			Type endDateField, 
			Type endDateUIField, 
			Type transactionDateField, 
			bool isDateChangePossible, 
			Type transactionDateEnabledCondition = null, 
			bool applyConditionToUnsavedRowsOnly = false) 
			: this(payGroupField, null)
		{
			_PayGroupField = payGroupField;
			_StartDateField = startDateField;
			_EndDateField = endDateField;
			_EndDateUIField = endDateUIField;
			_TransactionDateField = transactionDateField;
			_IsDateChangePossible = isDateChangePossible;
			_TransactionDateEnabledCondition = transactionDateEnabledCondition;
			_ApplyConditionToUnsavedRowsOnly = applyConditionToUnsavedRowsOnly;
		}

		private PRPayGroupPeriodIDAttribute(Type PayGroup, Type SourceType)
			: base(SourceType, BqlCommand.Compose(
				typeof(Search<,>),
						typeof(PRPayGroupPeriod.finPeriodID),
						typeof(Where<,,>),
						typeof(PRPayGroupPeriod.payGroupID),
						typeof(Equal<>),
						typeof(Current<>),
						PayGroup,
						typeof(And<PRPayGroupPeriod.startDate, LessEqual<Required<PRPayGroupPeriod.startDate>>, And<PRPayGroupPeriod.endDate, Greater<Required<PRPayGroupPeriod.endDate>>>>)))
		{
			base.DisplayMask = PRPeriodIDDisplayMask;

			if (PayGroup != null)
			{
				Type search = BqlCommand.Compose(
					typeof(Search<,,>),
					typeof(PRPayGroupPeriod.finPeriodID),
					typeof(Where<,>),
					typeof(PRPayGroupPeriod.payGroupID),
					typeof(Equal<>),
					typeof(Optional<>),
					PayGroup,
					typeof(OrderBy<Asc<PRPayGroupPeriod.transactionDate, Asc<PRPayGroupPeriod.startDate>>>));

				_Attributes.Add(new PXOrderedSelectorAttribute(search, typeof(PRPayGroupPeriod.finPeriodID), typeof(PRPayGroupPeriod.descr), typeof(PRPayGroupPeriod.transactionDate), typeof(PRPayGroupPeriod.startDate), typeof(PRPayGroupPeriod.endDateUI)));
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}
		#endregion

		#region Initialization
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			try
			{
				if (_SelAttrIndex != -1)
					((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = FormatPeriod((string)e.NewValue);
				throw;
			}
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_SelAttrIndex != -1)
			{
				var payGroupPeriod = GetPayGroupPeriod(sender, e.Row);
				sender.SetValueExt(e.Row, _StartDateField.Name, payGroupPeriod?.StartDate);
				if (_EndDateField != null)
				{
					sender.SetValueExt(e.Row, _EndDateField.Name, payGroupPeriod?.EndDate);
				}
				else if (_EndDateUIField != null)
				{
					object endDateUI = sender.Graph.Caches[typeof(PRPayGroupPeriod)].GetValueExt(payGroupPeriod, nameof(PRPayGroupPeriod.EndDateUI));
					sender.SetValueExt(e.Row, _EndDateUIField.Name, endDateUI);
				}
				sender.SetValueExt(e.Row, _TransactionDateField.Name, payGroupPeriod?.TransactionDate);
			}
		}

		public void PayGroupFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var payGroupPeriod = GetPayGroupPeriod(sender, e.Row);
			if (payGroupPeriod == null)
				sender.SetValueExt(e.Row, _FieldName, null);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			var isPayGroupSet = !string.IsNullOrEmpty(sender.GetValue(e.Row, _PayGroupField.Name) as string);
			var isPayPeriodDateChangeAllowed = IsPayPeriodDateChangeAllowed(sender);

			PXUIFieldAttribute.SetEnabled(sender, e.Row, FieldName, isPayGroupSet);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _StartDateField.Name, isPayGroupSet && isPayPeriodDateChangeAllowed);
			if (_EndDateUIField != null)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, _EndDateUIField.Name, isPayGroupSet && isPayPeriodDateChangeAllowed);
			}

			SetTransactionDateFieldState(sender, e.Row, isPayGroupSet && isPayPeriodDateChangeAllowed);
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _PayGroupField.Name, PayGroupFieldUpdated);

			base.CacheAttached(sender);
		}
		public new static string FormatForError(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForError(period, PRPeriodIDDisplayMask);
		}

		public new static string FormatPeriod(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForDisplay(period);
		}

		public new static string UnFormatPeriod(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForStoring(period);
		}

		public bool IsPayPeriodDateChangeAllowed(PXCache sender)
		{
			if (!_IsDateChangePossible)
			{
				return false;
			}

			PRSetup setup = new PXSetup<PRSetup>(sender.Graph).Select();
			return setup != null && setup.PayPeriodDateChangeAllowed == true;
		}

		private PRPayGroupPeriod GetPayGroupPeriod(PXCache sender, object row)
		{
			return (PRPayGroupPeriod)PXSelectorAttribute.Select(sender, row, _FieldName);
		}

		private void SetTransactionDateFieldState(PXCache sender, object row, bool isEnabledByDefault)
		{
			if (isEnabledByDefault ||
				_ApplyConditionToUnsavedRowsOnly && sender.GetStatus(row) != PXEntryStatus.Inserted ||
				_TransactionDateEnabledCondition == null || 
				!ConditionEvaluator.GetResult(sender, row, _TransactionDateEnabledCondition))
			{
				PXUIFieldAttribute.SetEnabled(sender, row, _TransactionDateField.Name, isEnabledByDefault);
				return;
			}

			DateTime? transactionDate = sender.GetValue(row, _TransactionDateField.Name) as DateTime?;

			if (transactionDate == null)
			{
				PRPayGroupPeriod payGroupPeriod = GetPayGroupPeriod(sender, row);
				transactionDate = payGroupPeriod?.TransactionDate;
			}

			PXUIFieldAttribute.SetEnabled(sender, row, _TransactionDateField.Name, transactionDate != null);
			if (transactionDate != null)
			{
				int year = transactionDate.Value.Year;
				sender.Adjust<PXDBDateAttribute>(row).For(_TransactionDateField.Name, attr =>
				{
					attr.MinValue = new DateTime(year, 01, 01).ToString(CultureInfo.InvariantCulture);
					attr.MaxValue = new DateTime(year, 12, 31).ToString(CultureInfo.InvariantCulture);
				});
			}
		}
		#endregion

		private class PXOrderedSelectorAttribute : PXSelectorAttribute
		{
			public PXOrderedSelectorAttribute(Type type, params Type[] fieldList) : base(type, fieldList) { }

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				PXView view = new PXOrderedSelectorView(sender.Graph, true, _Select);
				sender.Graph.Views[_ViewName] = view;
			}
		}

		private class PXOrderedSelectorView : PXView
		{
			public PXOrderedSelectorView(PXGraph graph, bool isReadOnly, BqlCommand select) : base(graph, isReadOnly, select)
			{
			}

			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				IBqlSortColumn[] bqlSortColumns = BqlSelect.GetSortColumns();

				List<object> searchesList = new List<object>();
				List<string> sortcolumnsList = new List<string>();
				List<bool> descendingsList = new List<bool>();

				for (int i = 0; i < bqlSortColumns.Count(); i++)
				{
					sortcolumnsList.Add(bqlSortColumns[i].GetReferencedType().Name);
					descendingsList.Add(bqlSortColumns[i].IsDescending);
					searchesList.Add(null);
				}

				if (searches != null)
				{
					searchesList.AddRange(searches);
				}
				if (sortcolumns != null)
				{
					sortcolumnsList.AddRange(sortcolumns);
				}
				if (descendings != null)
				{
					descendingsList.AddRange(descendings);
				}

				return base.Select(currents, parameters, searchesList.ToArray(), sortcolumnsList.ToArray(), descendingsList.ToArray(), filters, ref startRow, maximumRows, ref totalRows);
			}
		}
	}
}
