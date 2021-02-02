using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public abstract class PRSubaccountRequiredAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber, IPXRowPersistingSubscriber
	{
		protected List<string> _SetupFieldNameList = new List<string>();
		protected string _ValueWhenRequired;
		protected Type _RequiredCondition;

		public PRSubaccountRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
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

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete && sender.GetValue(e.Row, this.FieldName) == null && IsRequired(sender, e.Row))
			{
				PXUIFieldAttribute.SetError(sender, e.Row, this.FieldName, PXMessages.LocalizeFormatNoPrefix(Messages.CantBeEmpty, PXUIFieldAttribute.GetDisplayName(sender, this.FieldName)));
			}
		}

		public virtual bool IsRequired(PXCache sender, object row)
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

			foreach (var fieldName in _SetupFieldNameList)
			{
				PRSubAccountMaskAttribute maskAttribute = prSetupCache.GetAttributesOfType<PRSubAccountMaskAttribute>(setupRecord, fieldName).SingleOrDefault();
				if (maskAttribute != null)
				{
					var dimensionAttribute = maskAttribute.GetAttribute<PRDimensionMaskAttribute>();
					if (dimensionAttribute != null)
					{
						IEnumerable<string> segments = dimensionAttribute.GetSegmentMaskValues(prSetupCache.GetValue(setupRecord, fieldName) as string);
						if (segments.Any(x => x == _ValueWhenRequired))
						{
							return true;
						}
					}
				}
			}

			return false;
		}
	}

	public class PREarningSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PREarningSubRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.earningsSubMask).Name);
		}
	}

	public class PRDedLiabilitySubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRDedLiabilitySubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.deductLiabilitySubMask).Name);
		}
	}

	public class PRBenExpenseSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRBenExpenseSubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.benefitExpenseSubMask).Name);
		}
	}

	public class PRBenLiabilitySubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRBenLiabilitySubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.benefitLiabilitySubMask).Name);
		}
	}

	public class PRTaxExpenseSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRTaxExpenseSubRequiredAttribute(string valueWhenRequired, Type requiredCondition = null)
			: base(valueWhenRequired, requiredCondition)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.taxExpenseSubMask).Name);
		}
	}

	public class PRTaxLiabilitySubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRTaxLiabilitySubRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.taxLiabilitySubMask).Name);
		}
	}

	public class PRBranchSubRequiredAttribute : PRSubaccountRequiredAttribute
	{
		public PRBranchSubRequiredAttribute(string valueWhenRequired)
			: base(valueWhenRequired)
		{
			_SetupFieldNameList.Add(typeof(PRSetup.earningsSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.deductLiabilitySubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.benefitExpenseSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.benefitLiabilitySubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.taxExpenseSubMask).Name);
			_SetupFieldNameList.Add(typeof(PRSetup.taxLiabilitySubMask).Name);
		}
	}
}
