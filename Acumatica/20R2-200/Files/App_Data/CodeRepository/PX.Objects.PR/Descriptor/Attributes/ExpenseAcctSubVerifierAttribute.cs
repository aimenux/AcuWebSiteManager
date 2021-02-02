using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class ExpenseAcctSubVerifierAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
	{
		private Type _SubMaskField;
		private string _MaskEarningType;
		private string _MaskLaborItem;

		public ExpenseAcctSubVerifierAttribute(Type subMaskField, string maskEarningType, string maskLaborItem)
		{
			_SubMaskField = subMaskField;
			_MaskEarningType = maskEarningType;
			_MaskLaborItem = maskLaborItem;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), _SubMaskField.Name, (cache, e) =>
			{
				FieldVerifying(cache, e, true);
			});
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FieldVerifying(sender, e, false);
		}

		private void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, bool isSub)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				return;
			}

			string acctDefault = (string)(isSub ? sender.GetValue(e.Row, _FieldOrdinal) : e.NewValue);
			string prohibitedSubMaskValue = null;

			if (acctDefault == _MaskEarningType)
			{
				prohibitedSubMaskValue = _MaskLaborItem;
			}
			else if (acctDefault == _MaskLaborItem)
			{
				prohibitedSubMaskValue = _MaskEarningType;
			}

			PRSubAccountMaskAttribute subMaskAttribute = sender.GetAttributesOfType<PRSubAccountMaskAttribute>(e.Row, _SubMaskField.Name).FirstOrDefault();
			if (subMaskAttribute != null)
			{
				PRDimensionMaskAttribute dimensionMaskAttribute = subMaskAttribute.GetAttribute<PRDimensionMaskAttribute>();
				if (dimensionMaskAttribute != null)
				{
					string subMask = (string)(isSub ? e.NewValue : sender.GetValue(e.Row, _SubMaskField.Name));
					if (subMask == null)
					{
						return;
					}

					List<string> segmentMaskValues = dimensionMaskAttribute.GetSegmentMaskValues(subMask).ToList();
					if ((!string.IsNullOrEmpty(prohibitedSubMaskValue) && segmentMaskValues.Contains(prohibitedSubMaskValue)) ||
						(segmentMaskValues.Contains(_MaskEarningType) && segmentMaskValues.Contains(_MaskLaborItem)))
					{
						throw new PXSetPropertyException(Messages.EarningTypeAndLaborItemInAcctSub,
							sender.GetAttributesOfType<PXUIFieldAttribute>(e.Row, _FieldName).FirstOrDefault()?.DisplayName ?? _FieldName,
							sender.GetAttributesOfType<PXUIFieldAttribute>(e.Row, _SubMaskField.Name).FirstOrDefault()?.DisplayName ?? _SubMaskField.Name);
					}
				}
			}
		}
	}
}
