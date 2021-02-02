using PX.Data;
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.PR
{
	public class CostAssignmentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
			{
				char[] availableDigits = { NoAssignment, ProjectAssignment, LaborItemAssignment, EarningTypeAssignment, ProjectAndLaborItemAssignment, ProjectAndEarningTypeAssignment };
				List<string> valueList = new List<string>();
				foreach (char i in availableDigits)
				{
					foreach (char j in availableDigits)
					{
						foreach (char k in availableDigits)
						{
							valueList.Add(new string(new char[] { i, j, k }));
						}
					}
				}

				_AllowedValues = valueList.ToArray();
			}
		}

		public static string GetCode(CostAssignmentSetting earningSetting, CostAssignmentSetting benefitSetting, CostAssignmentSetting taxSetting)
		{
			StringBuilder sb = new StringBuilder();
			foreach (CostAssignmentSetting setting in new CostAssignmentSetting[] { earningSetting, benefitSetting, taxSetting })
			{
				if (setting.AssignCostToProject)
				{
					if (setting.AssignCostToLaborItem)
					{
						sb.Append(ProjectAndLaborItemAssignment);
					}
					else if (setting.AssignCostToEarningType)
					{
						sb.Append(ProjectAndEarningTypeAssignment);
					}
					else
					{
						sb.Append(ProjectAssignment);
					}
				}
				else if (setting.AssignCostToLaborItem)
				{
					sb.Append(LaborItemAssignment);
				}
				else if (setting.AssignCostToEarningType)
				{
					sb.Append(LaborItemAssignment);
				}
				else
				{
					sb.Append(NoAssignment);
				}
			}

			return sb.ToString();
		}

		public static CostAssignmentSetting GetEarningSetting(string code)
		{
			return GetSetting(code[0]);
		}

		public static CostAssignmentSetting GetBenefitSetting(string code)
		{
			return GetSetting(code[1]);
		}

		public static CostAssignmentSetting GetTaxSetting(string code)
		{
			return GetSetting(code[2]);
		}

		private static CostAssignmentSetting GetSetting(char code)
		{
			bool assignCostToProject = code == ProjectAssignment || code == ProjectAndLaborItemAssignment || code == ProjectAndEarningTypeAssignment;
			bool assignCostToLaborItem = code == LaborItemAssignment || code == ProjectAndLaborItemAssignment;
			bool assignCostToEarningType = code == EarningTypeAssignment || code == ProjectAndEarningTypeAssignment;
			return new CostAssignmentSetting(assignCostToProject, assignCostToLaborItem, assignCostToEarningType);
		}

		public static bool IsProjectSplitEnabled(PXGraph graph, PRPayment currentPayment, DetailType detailType)
		{
			if (detailType == DetailType.Benefit)
			{
				return CostAssignmentColumnVisibilityEvaluator.BenefitProject.Evaluate(graph, currentPayment);
			}
			else
			{
				return CostAssignmentColumnVisibilityEvaluator.TaxProject.Evaluate(graph, currentPayment);
			}
		}

		public static bool IsEarningTypeSplitEnabled(PXGraph graph, PRPayment currentPayment, DetailType detailType)
		{
			if (detailType == DetailType.Benefit)
			{
				return CostAssignmentColumnVisibilityEvaluator.BenefitEarningType.Evaluate(graph, currentPayment);
			}
			else
			{
				return CostAssignmentColumnVisibilityEvaluator.TaxEarningType.Evaluate(graph, currentPayment);
			}
		}

		public static bool IsLaborItemSplitEnabled(PXGraph graph, PRPayment currentPayment, DetailType detailType)
		{
			if (detailType == DetailType.Benefit)
			{
				return CostAssignmentColumnVisibilityEvaluator.BenefitLaborItem.Evaluate(graph, currentPayment);
			}
			else
			{
				return CostAssignmentColumnVisibilityEvaluator.TaxLaborItem.Evaluate(graph, currentPayment);
			}
		}

		private const char NoAssignment = 'N';
		private const char ProjectAssignment = 'P';
		private const char LaborItemAssignment = 'L';
		private const char EarningTypeAssignment = 'T';
		private const char ProjectAndLaborItemAssignment = 'X';
		private const char ProjectAndEarningTypeAssignment = 'Y';

		public enum DetailType
		{
			Benefit,
			Tax
		}
	}

	public class CostAssignmentSetting
	{
		public CostAssignmentSetting(bool assignCostToProject, bool assignCostToLaborItem, bool assignCostToEarningType)
		{
			AssignCostToProject = assignCostToProject;
			AssignCostToLaborItem = assignCostToLaborItem;
			AssignCostToEarningType = assignCostToEarningType;
		}

		public bool AssignCostToProject { get; set; } = false;
		public bool AssignCostToLaborItem
		{
			get => _AssignCostToLaborItem;
			set
			{
				if (value && AssignCostToEarningType)
				{
					throw new PXException(Messages.CantAssignCostToLaborItemAndEarningType);
				}
				_AssignCostToLaborItem = value;
			}
		}
		public bool AssignCostToEarningType
		{
			get => _AssignCostToEarningType;
			set
			{
				if (value && AssignCostToLaborItem)
				{
					throw new PXException(Messages.CantAssignCostToLaborItemAndEarningType);
				}
				_AssignCostToEarningType = value;
			}
		}

		private bool _AssignCostToLaborItem = false;
		private bool _AssignCostToEarningType = false;
	}
}
