using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR.Utility
{
	public class CalculationResultInfo<TGraph, TTable> : IPXCustomInfo
		where TGraph : PXGraph
		where TTable : class, IBqlTable
	{
		private List<IDeductionCalculationErrorWarning> errorWarnings = new List<IDeductionCalculationErrorWarning>();

		public void Complete(PXLongRunStatus status, PXGraph graph)
		{
			if (graph is TGraph)
			{
				((TGraph)graph).RowSelected.AddHandler<TTable>((sender, e) =>
				{
					TTable row = e.Row as TTable;
					if (row == null)
					{
						return;
					}

					foreach (var errWarn in errorWarnings)
					{
						if (errWarn.MatchesRow(sender, row))
						{
							errWarn.Handle(sender, row);
						}
					}
				});
			}
		}

		public void AddWarning<Field>(TTable row, object value, string message, params object[] args) where Field : IBqlField
		{
			errorWarnings.Add(new DeductionCalculationWarning<Field>(row, value, message, args));
		}

		public void ClearWarning<Field>(TTable row, object value) where Field : IBqlField
		{
			errorWarnings.Add(DeductionCalculationWarning<Field>.ClearWarning(row, value));
		}

		public void AddError<Field>(TTable row, object value, string message, params object[] args) where Field : IBqlField
		{
			var error = new DeductionCalculationError<Field>(row, value, message, args);
			errorWarnings.Add(error);
			throw error;
		}

		public bool HasChildError<TParent>(PXCache childCache, PXCache parentCache, TParent record) where TParent : class, IBqlTable
		{
			foreach (object childObj in PXParentAttribute.SelectChildren(parentCache, record, typeof(TParent)))
			{
				TTable child = childObj as TTable;
				if (child != null && errorWarnings.Any(x => x.IsError && x.MatchesRow(childCache, child)))
				{
					return true;
				}
			}

			return false;
		}

		private interface IDeductionCalculationErrorWarning
		{
			void Handle(PXCache cache, TTable row);
			bool MatchesRow(PXCache cache, TTable matchRow);
			bool IsError { get; }
		}

		private class DeductionCalculationError<Field> : PXSetPropertyException, IDeductionCalculationErrorWarning where Field : IBqlField
		{
			private TTable errorRow;

			public DeductionCalculationError(TTable row, object value, string message, params object[] args)
				: base(message, args)
			{
				errorRow = row;
				ErrorValue = value;
			}

			public virtual void Handle(PXCache cache, TTable row)
			{
				cache.RaiseExceptionHandling<Field>(row, null, this);
			}

			public virtual bool MatchesRow(PXCache cache, TTable matchRow)
			{
				foreach (var key in cache.Keys)
				{
					if (!cache.GetValue(matchRow, key).Equals(cache.GetValue(errorRow, key)))
					{
						return false;
					}
				}
				return true;
			}

			public bool IsError => true;
		}

		private class DeductionCalculationWarning<Field> : IDeductionCalculationErrorWarning where Field : IBqlField
		{
			private TTable warningRow;
			private object warningValue;
			private Exception ex;

			public DeductionCalculationWarning(TTable row, object value, string message, params object[] args)
			{
				warningRow = row;
				ex = new PXSetPropertyException(message, PXErrorLevel.Warning, args);
				warningValue = value;
			}

			private DeductionCalculationWarning(TTable row, object value)
			{
				// A warning with a null exception that will clear previous warning for this field.
				warningRow = row;
				ex = null;
				warningValue = value;
			}

			public static DeductionCalculationWarning<Field> ClearWarning(TTable row, object value)
			{
				return new DeductionCalculationWarning<Field>(row, value);
			}

			public virtual void Handle(PXCache cache, TTable row)
			{
				cache.RaiseExceptionHandling<Field>(row, warningValue, ex);
			}

			public virtual bool MatchesRow(PXCache cache, TTable matchRow)
			{
				foreach (var key in cache.Keys)
				{
					if (!cache.GetValue(matchRow, key).Equals(cache.GetValue(warningRow, key)))
					{
						return false;
					}
				}
				return true;
			}

			public bool IsError => false;
		}
	}
}
