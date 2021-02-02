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
	[PXDBInt]
	[PXUIField(DisplayName = "Labor Item")]
	public class PMLaborItemAttribute : AcctSubAttribute, IPXFieldDefaultingSubscriber
	{
		protected Type projectField;
		protected Type earningTypeField;
		protected Type employeeSearch;

		public PMLaborItemAttribute(Type project, Type earningType, Type employeeSearch)
		{
			this.projectField = project;
			this.earningTypeField = earningType;
			this.employeeSearch = employeeSearch;

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD));
			
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPEmployee employee = null;

			if (employeeSearch != null)
			{
				BqlCommand cmd = BqlCommand.CreateInstance(employeeSearch);
				PXView view = new PXView(sender.Graph, false, cmd);

				employee = view.SelectSingle() as EPEmployee;
			}

			if (employee != null)
			{
				int? projectID = (int?)sender.GetValue(e.Row, projectField.Name);
				string earningType = (string)sender.GetValue(e.Row, earningTypeField.Name);

				e.NewValue = GetDefaultLaborItem(sender.Graph, employee, earningType, projectID);
			}
		}

		public virtual int? GetDefaultLaborItem(PXGraph graph, EPEmployee employee, string earningType, int? projectID)
		{
			if (employee == null)
				return null;

			int? result = null;

			if (ProjectDefaultAttribute.IsProject(graph, projectID))
			{
				result = EPContractRate.GetProjectLaborClassID(graph, projectID.Value, employee.BAccountID.Value, earningType);
			}

			if (result == null)
			{
				result = EPEmployeeClassLaborMatrix.GetLaborClassID(graph, employee.BAccountID, earningType);
			}

			if (result == null)
			{
				result = employee.LabourItemID;
			}

			return result;
		}
	}
}
