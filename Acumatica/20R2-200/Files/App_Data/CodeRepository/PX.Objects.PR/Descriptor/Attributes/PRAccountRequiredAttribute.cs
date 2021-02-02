using PX.Data;
using PX.Data.BQL.Fluent;
using System;

namespace PX.Objects.PR
{
	public abstract class PRAccountRequiredAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber, IPXRowPersistingSubscriber
	{
		protected string _SetupFieldName;
		protected string _ValueWhenRequired;
		protected Type _RequiredCondition;

		public PRAccountRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
		{
			_ValueWhenRequired = valueWhenRequired;
			_RequiredCondition = requiredCondition;
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetRequired(sender, this.FieldName, IsRequired(sender, e.Row));
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete && sender.GetValue(e.Row, this.FieldName) == null && IsRequired(sender, e.Row))
			{
				PXUIFieldAttribute.SetError(sender, e.Row, this.FieldName, PXMessages.LocalizeFormatNoPrefix(Messages.CantBeEmpty, PXUIFieldAttribute.GetDisplayName(sender, this.FieldName)));
			}
		}

		private bool IsRequired(PXCache sender, object row)
		{
			PXCache prSetupCache = sender.Graph.Caches[typeof(PRSetup)];
			PRSetup setupRecord = prSetupCache?.Current as PRSetup ?? SelectFrom<PRSetup>.View.Select(sender.Graph);
			if (setupRecord == null)
			{
				return false;
			}

			if (row != null && _RequiredCondition != null && !ConditionEvaluator.GetResult(sender, row, _RequiredCondition))
			{
				return false;
			}

			var setupValue = prSetupCache.GetValue(setupRecord, _SetupFieldName) as string;
			return setupValue == _ValueWhenRequired;
		}
	}

	public class PREarningAccountRequiredAttribute : PRAccountRequiredAttribute
	{
		public PREarningAccountRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldName = typeof(PRSetup.earningsAcctDefault).Name;
		}
	}

	public class PRDedLiabilityAccountRequiredAttribute : PRAccountRequiredAttribute
	{
		public PRDedLiabilityAccountRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldName = typeof(PRSetup.deductLiabilityAcctDefault).Name;
		}
	}

	public class PRBenExpenseAccountRequiredAttribute : PRAccountRequiredAttribute
	{
		public PRBenExpenseAccountRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldName = typeof(PRSetup.benefitExpenseAcctDefault).Name;
		}
	}

	public class PRBenLiabilityAccountRequiredAttribute : PRAccountRequiredAttribute
	{
		public PRBenLiabilityAccountRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldName = typeof(PRSetup.benefitLiabilityAcctDefault).Name;
		}
	}

	public class PRTaxExpenseAccountRequiredAttribute : PRAccountRequiredAttribute
	{
		public PRTaxExpenseAccountRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldName = typeof(PRSetup.taxExpenseAcctDefault).Name;
		}
	}

	public class PRTaxLiabilityAccountRequiredAttribute : PRAccountRequiredAttribute
	{
		public PRTaxLiabilityAccountRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldName = typeof(PRSetup.taxLiabilityAcctDefault).Name;
		}
	}
}