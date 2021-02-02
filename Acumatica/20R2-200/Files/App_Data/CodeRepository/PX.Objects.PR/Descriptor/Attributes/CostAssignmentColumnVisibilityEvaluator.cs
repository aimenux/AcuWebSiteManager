using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class CostAssignmentColumnVisibilityEvaluator
	{
		public abstract class ByProject : BqlFormulaEvaluator, IBqlOperand
		{
			protected static bool IsVisiblePerSetup(PXGraph graph)
			{
				PRSetup payrollPreferences = graph.Caches[typeof(PRSetup)]?.Current as PRSetup ??
					new SelectFrom<PRSetup>.View(graph).SelectSingle();

				return payrollPreferences?.ProjectCostAssignment == ProjectCostAssignmentType.WageLaborBurdenAssigned;
			}
		}

		public class BenefitProject : ByProject
		{
			public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
			{
				return Evaluate(cache.Graph, cache.Graph.Caches[typeof(PRPayment)]?.Current as PRPayment);
			}

			public static bool Evaluate(PXGraph graph, PRPayment currentPayment)
			{
				if (IsVisiblePerSetup(graph))
				{
					return true;
				}

				return CostSplitTypeIsFixed(currentPayment) && CostAssignmentType.GetBenefitSetting(currentPayment.LaborCostSplitType).AssignCostToProject;
			}
		}

		public class TaxProject : ByProject
		{
			public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
			{
				return Evaluate(cache.Graph, cache.Graph.Caches[typeof(PRPayment)]?.Current as PRPayment);
			}

			public static bool Evaluate(PXGraph graph, PRPayment currentPayment)
			{
				if (IsVisiblePerSetup(graph))
				{
					return true;
				}

				return CostSplitTypeIsFixed(currentPayment) && CostAssignmentType.GetTaxSetting(currentPayment.LaborCostSplitType).AssignCostToProject;
			}
		}

		public abstract class ByAcctSubMaskCombo<TExpenseAcctDefault, TExpenseSubMask> : BqlFormulaEvaluator<TExpenseAcctDefault, TExpenseSubMask>, IBqlOperand 
			where TExpenseAcctDefault : IBqlField
			where TExpenseSubMask : IBqlField
		{
			protected static bool IsVisiblePerSetup(PXGraph graph, string compareValue)
			{
				PXCache setupCache = graph.Caches[typeof(PRSetup)];
				PRSetup payrollPreferences = setupCache?.Current as PRSetup ??
					new SelectFrom<PRSetup>.View(graph).SelectSingle();

				if (payrollPreferences == null)
				{
					return false;
				}
				if (compareValue.Equals(setupCache.GetValue(payrollPreferences, typeof(TExpenseAcctDefault).Name)))
				{
					return true;
				}

				if (setupCache != null)
				{
					PRSubAccountMaskAttribute subMaskAttribute = setupCache.GetAttributesOfType<PRSubAccountMaskAttribute>(payrollPreferences, typeof(TExpenseSubMask).Name).FirstOrDefault();
					if (subMaskAttribute != null)
					{
						string subMask = (string)setupCache.GetValue(payrollPreferences, typeof(TExpenseSubMask).Name);
						PRDimensionMaskAttribute dimensionMaskAttribute = subMaskAttribute.GetAttribute<PRDimensionMaskAttribute>();
						if (dimensionMaskAttribute != null)
						{
							return dimensionMaskAttribute.GetSegmentMaskValues(subMask).Contains(compareValue);
						}
					}
				}

				return false;
			}
		}

		public class BenefitEarningType : ByAcctSubMaskCombo<PRSetup.benefitExpenseAcctDefault, PRSetup.benefitExpenseSubMask>
		{
			public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
			{
				return Evaluate(cache.Graph, cache.Graph.Caches[typeof(PRPayment)]?.Current as PRPayment);
			}

			public static bool Evaluate(PXGraph graph, PRPayment currentPayment)
			{
				if (IsVisiblePerSetup(graph, PRBenefitExpenseAcctSubDefault.MaskEarningType))
				{
					return true;
				}

				return CostSplitTypeIsFixed(currentPayment) && CostAssignmentType.GetBenefitSetting(currentPayment.LaborCostSplitType).AssignCostToEarningType;
			}
		}

		public class BenefitLaborItem : ByAcctSubMaskCombo<PRSetup.benefitExpenseAcctDefault, PRSetup.benefitExpenseSubMask>
		{
			public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
			{
				return Evaluate(cache.Graph, cache.Graph.Caches[typeof(PRPayment)]?.Current as PRPayment);
			}

			public static bool Evaluate(PXGraph graph, PRPayment currentPayment)
			{
				if (IsVisiblePerSetup(graph, PRBenefitExpenseAcctSubDefault.MaskLaborItem))
				{
					return true;
				}

				return CostSplitTypeIsFixed(currentPayment) && CostAssignmentType.GetBenefitSetting(currentPayment.LaborCostSplitType).AssignCostToLaborItem;
			}
		}

		public class TaxEarningType : ByAcctSubMaskCombo<PRSetup.taxExpenseAcctDefault, PRSetup.taxExpenseSubMask>
		{
			public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
			{
				return Evaluate(cache.Graph, cache.Graph.Caches[typeof(PRPayment)]?.Current as PRPayment);
			}

			public static bool Evaluate(PXGraph graph, PRPayment currentPayment)
			{
				if (IsVisiblePerSetup(graph, PRTaxExpenseAcctSubDefault.MaskEarningType))
				{
					return true;
				}

				return CostSplitTypeIsFixed(currentPayment) && CostAssignmentType.GetTaxSetting(currentPayment.LaborCostSplitType).AssignCostToEarningType;
			}
		}

		public class TaxLaborItem : ByAcctSubMaskCombo<PRSetup.taxExpenseAcctDefault, PRSetup.taxExpenseSubMask>
		{
			public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
			{
				return Evaluate(cache.Graph, cache.Graph.Caches[typeof(PRPayment)]?.Current as PRPayment);
			}

			public static bool Evaluate(PXGraph graph, PRPayment currentPayment)
			{
				if (IsVisiblePerSetup(graph, PRTaxExpenseAcctSubDefault.MaskLaborItem))
				{
					return true;
				}

				return CostSplitTypeIsFixed(currentPayment) && CostAssignmentType.GetTaxSetting(currentPayment.LaborCostSplitType).AssignCostToLaborItem;
			}
		}

		private static bool CostSplitTypeIsFixed(PRPayment currentPayment) => currentPayment != null &&
			(currentPayment.Printed == true || currentPayment.Released == true) &&
			!string.IsNullOrEmpty(currentPayment.LaborCostSplitType);
	}
}
