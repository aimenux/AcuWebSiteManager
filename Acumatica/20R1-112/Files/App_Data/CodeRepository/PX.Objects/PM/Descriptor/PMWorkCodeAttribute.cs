using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Text;

namespace PX.Objects.PM
{
	[PXDBString(PMWorkCode.workCodeID.Length)]
	[PXUIField(DisplayName = "WCC Code", FieldClass = nameof(FeaturesSet.Construction))]
	[PXRestrictor(typeof(Where<PMWorkCode.isActive.IsEqual<True>>), Messages.InactiveWorkCode, typeof(PMWorkCode.workCodeID))]
	public class PMWorkCodeAttribute : AcctSubAttribute
	{
		protected Type costCodeField;

		public PMWorkCodeAttribute()
		{
			PXSelectorAttribute select = new PXSelectorAttribute(typeof(PMWorkCode.workCodeID), DescriptionField = typeof(PMWorkCode.description));
			
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public PMWorkCodeAttribute(Type costcode) : this()
		{
			this.costCodeField = costcode;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (costCodeField != null)
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), FieldName, FieldDefaulting);

			PXDBStringAttribute dbStringAttribute = DBAttribute as PXDBStringAttribute;
			if (dbStringAttribute != null && dbStringAttribute.IsKey)
			{
				// Working with selectors without a SubstituteKey as key fields presents some issues. When the selector is a key field, 
				// the input mask for the field gets cleared in CacheAttached. We need to manually set it back here
				// to ensure the selector field works correctly with free-form input.
				StringBuilder inputMask = new StringBuilder(">");
				for (int i = 0; i < dbStringAttribute.Length; i++)
				{
					inputMask.Append("a");
				}
				PXDBStringAttribute.SetInputMask(sender, _FieldName, inputMask.ToString());
			}
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? costCodeID = (int?)sender.GetValue(e.Row, costCodeField.Name);

			if (costCodeID != null && costCodeID != CostCodeAttribute.GetDefaultCostCode())
			{
				PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Required<PMCostCode.costCodeID>>>>.Select(sender.Graph, costCodeID);
				if (costCode != null)
				{
					var select = new PXSelect<PMWorkCode, Where<PMWorkCode.isActive, Equal<True>, 
						And<PMWorkCode.costCodeFrom, LessEqual<Required<PMWorkCode.costCodeFrom>>,
						And<PMWorkCode.costCodeTo, GreaterEqual<Required<PMWorkCode.costCodeTo>>>>>>(sender.Graph);
					PMWorkCode code = select.Select(costCode.CostCodeCD, costCode.CostCodeCD);
					if (code != null)
					{
						e.NewValue = code.WorkCodeID;
					}
				}
			}
		}
	}
}
