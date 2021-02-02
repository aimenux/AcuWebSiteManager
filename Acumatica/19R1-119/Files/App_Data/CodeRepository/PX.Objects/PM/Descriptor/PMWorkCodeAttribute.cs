using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	[PXDBString(PMWorkCode.workCodeID.Length)]
	[PXUIField(DisplayName = "WCC Code", FieldClass = nameof(FeaturesSet.Construction))]
	public class PMWorkCodeAttribute : AcctSubAttribute, IPXFieldDefaultingSubscriber
	{
		protected Type costCodeField;
		
		public PMWorkCodeAttribute(Type costcode)
		{
			this.costCodeField = costcode;
			
			PXSelectorAttribute select = new  PXSelectorAttribute(typeof(PMWorkCode.workCodeID), DescriptionField = typeof(PMWorkCode.description));
			
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? costCodeID = (int?)sender.GetValue(e.Row, costCodeField.Name);

			if (costCodeID != null && costCodeID != CostCodeAttribute.GetDefaultCostCode())
			{
				PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Required<PMCostCode.costCodeID>>>>.Select(sender.Graph, costCodeID);
				if (costCode != null)
				{
					var select = new PXSelect<PMWorkCode, Where<PMWorkCode.costCodeFrom, LessEqual<Required<PMWorkCode.costCodeFrom>>,
						And<PMWorkCode.costCodeTo, GreaterEqual<Required<PMWorkCode.costCodeTo>>>>>(sender.Graph);
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
