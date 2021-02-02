using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	public class EarningTypeLaborItemOverrideAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _EmployeeIDField;
		private Type _LaborItemField;

		public EarningTypeLaborItemOverrideAttribute(Type employeeIDField, Type laborItemField)
		{
			_EmployeeIDField = employeeIDField;
			_LaborItemField = laborItemField;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object employeeID = sender.GetValue(e.Row, _EmployeeIDField.Name);
			object earningType = sender.GetValue(e.Row, _FieldName);

			EPEmployeeClassLaborMatrix laborItemOverride = SelectFrom<EPEmployeeClassLaborMatrix>
				.Where<EPEmployeeClassLaborMatrix.employeeID.IsEqual<P.AsInt>
					.And<EPEmployeeClassLaborMatrix.earningType.IsEqual<P.AsString>>>.View.Select(sender.Graph, employeeID, earningType).FirstTableItems.FirstOrDefault();
			if (laborItemOverride != null)
			{
				sender.SetValue(e.Row, _LaborItemField.Name, laborItemOverride.LabourItemID);
			}
		}
	}
}
