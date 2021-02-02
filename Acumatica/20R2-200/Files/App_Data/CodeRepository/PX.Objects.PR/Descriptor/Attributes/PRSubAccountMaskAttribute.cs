using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public class PRSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		Type _AttributeType;

		public PRSubAccountMaskAttribute(Type attributeType, string maskName, string defaultValue)
		{
			_AttributeType = attributeType;
			var subListAttribute = (CustomListAttribute)Activator.CreateInstance(_AttributeType);
			PXDimensionMaskAttribute attr = new PRDimensionMaskAttribute(_DimensionName, maskName, defaultValue, subListAttribute);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			subscribers.Remove(_Attributes.FirstOrDefault(x => x.GetType().IsAssignableFrom(_AttributeType)) as ISubscriber);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			var stringlist = (CustomListAttribute)_Attributes.First(x => x.GetType() == _AttributeType);
			var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.ElementAt(_SelAttrIndex);
			dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, Type attributeType, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			var subListAttribute = (CustomListAttribute)Activator.CreateInstance(attributeType);
			try
			{
				//In MakeSub, -1 is used to raise an error instead of defaulting value, otherwise should be an index from sources[]
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, subListAttribute.AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(subListAttribute.AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	public class PRDimensionMaskAttribute : PXDimensionMaskAttribute
	{
		public PRDimensionMaskAttribute(string dimensionName, string maskName, string defaultValue, CustomListAttribute subListAttribute) :
			base(dimensionName, maskName, defaultValue, subListAttribute.AllowedValues, subListAttribute.AllowedLabels)
		{ }

		public IEnumerable<string> GetSegmentMaskValues(string mask)
		{
			for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
			{
				string input = mask.Substring(0, _Definition.Dimensions[_Dimension][i].Length);

				string matchVal = null;
				foreach (string val in _allowedValues)
				{
					if (new string(char.Parse(val), input.Length).Equals(input))
					{
						matchVal = val;
						break;
					}
				}

				if (!string.IsNullOrEmpty(matchVal))
				{
					yield return matchVal;
					mask = mask.Substring(_Definition.Dimensions[_Dimension][i].Length);
					continue;
				}

				throw new PXException(PXMessages.LocalizeFormat(ErrorMessages.ElementOfFieldDoesntExist, input, _FieldName));
			}
		}
	}

	[PREarningsAcctSubDefault.SubList]
	public sealed class PREarningsSubAccountMaskAttribute : PRSubAccountMaskAttribute
	{
		public PREarningsSubAccountMaskAttribute()
			: base(typeof(PREarningsAcctSubDefault.SubListAttribute), PRSubAccountMaskConstants.EarningMaskName, PREarningsAcctSubDefault.MaskEarningType)
		{
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			return MakeSub<Field>(graph, mask, typeof(PREarningsAcctSubDefault.SubListAttribute), sources, fields);
		}
	}

	[PRDeductAcctSubDefault.SubList]
	public sealed class PRDeductSubAccountMaskAttribute : PRSubAccountMaskAttribute
	{
		public PRDeductSubAccountMaskAttribute()
			: base(typeof(PRDeductAcctSubDefault.SubListAttribute), PRSubAccountMaskConstants.DeductionMaskName, PRDeductAcctSubDefault.MaskDeductionCode)
		{
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			return MakeSub<Field>(graph, mask, typeof(PRDeductAcctSubDefault.SubListAttribute), sources, fields);
		}
	}

	[PRBenefitExpenseAcctSubDefault.SubList]
	public sealed class PRBenefitExpenseSubAccountMaskAttribute : PRSubAccountMaskAttribute
	{
		public PRBenefitExpenseSubAccountMaskAttribute()
			: base(typeof(PRBenefitExpenseAcctSubDefault.SubListAttribute), PRSubAccountMaskConstants.BenefitExpenseMaskName, PRDeductAcctSubDefault.MaskDeductionCode)
		{
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			return MakeSub<Field>(graph, mask, typeof(PRBenefitExpenseAcctSubDefault.SubListAttribute), sources, fields);
		}
	}

	[PRTaxAcctSubDefault.SubList]
	public sealed class PRTaxSubAccountMaskAttribute : PRSubAccountMaskAttribute
	{
		public PRTaxSubAccountMaskAttribute()
			: base(typeof(PRTaxAcctSubDefault.SubListAttribute), PRSubAccountMaskConstants.TaxMaskName, PRTaxAcctSubDefault.MaskTaxCode)
		{
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			return MakeSub<Field>(graph, mask, typeof(PRTaxAcctSubDefault.SubListAttribute), sources, fields);
		}
	}

	[PRTaxExpenseAcctSubDefault.SubList]
	public sealed class PRTaxExpenseSubAccountMaskAttribute : PRSubAccountMaskAttribute
	{
		public PRTaxExpenseSubAccountMaskAttribute()
			: base(typeof(PRTaxExpenseAcctSubDefault.SubListAttribute), PRSubAccountMaskConstants.TaxExpenseMaskName, PRTaxAcctSubDefault.MaskTaxCode)
		{
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			return MakeSub<Field>(graph, mask, typeof(PRTaxExpenseAcctSubDefault.SubListAttribute), sources, fields);
		}
	}
}