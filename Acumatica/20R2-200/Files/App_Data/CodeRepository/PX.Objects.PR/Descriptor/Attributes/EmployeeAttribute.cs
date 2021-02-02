using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	[PXDBInt]
	[PXUIField(DisplayName = "Employee")]
	public abstract class EmployeeAttributeBase : AcctSubAttribute
	{
		private int? _EPActiveRestrictorAttributeIndex = null;
		private int? _PRActiveRestrictorAttributeIndex = null;

		public bool FilterActive
		{
			set
			{
				if (_EPActiveRestrictorAttributeIndex != null && _EPActiveRestrictorAttributeIndex < _Attributes.Count)
				{
					DisablableRestrictorAttribute attr = _Attributes[_EPActiveRestrictorAttributeIndex.Value] as DisablableRestrictorAttribute;
					if (attr != null)
					{
						attr.Enabled = value;
					}
				}

				if (_PRActiveRestrictorAttributeIndex != null && _PRActiveRestrictorAttributeIndex < _Attributes.Count)
				{
					DisablableRestrictorAttribute attr = _Attributes[_PRActiveRestrictorAttributeIndex.Value] as DisablableRestrictorAttribute;
					if (attr != null)
					{
						attr.Enabled = value;
					}
				}
			}
		}

		public EmployeeAttributeBase(bool filterActive, Type searchType, params Type[] fieldList)
		{
			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(PX.Objects.EP.EmployeeRawAttribute.DimensionName, searchType, typeof(EPEmployee.acctCD), fieldList));
			attr.DescriptionField = typeof(EPEmployee.acctName);
			_SelAttrIndex = _Attributes.Count - 1;

			if (filterActive)
			{
				DisablableRestrictorAttribute epRestrictor = new DisablableRestrictorAttribute(typeof(Where<EPEmployee.status.IsEqual<BAccount.status.active>>), Messages.InactiveEPEmployee, typeof(EPEmployee.acctName));
				epRestrictor.ShowWarning = true;
				_Attributes.Add(epRestrictor);
				_EPActiveRestrictorAttributeIndex = _Attributes.Count - 1;

				DisablableRestrictorAttribute prRestrictor = new DisablableRestrictorAttribute(typeof(Where<PREmployee.activeInPayroll, Equal<True>>), Messages.InactivePREmployee, typeof(PREmployee.acctName));
				prRestrictor.ShowWarning = true;
				_Attributes.Add(prRestrictor);
				_PRActiveRestrictorAttributeIndex = _Attributes.Count - 1;
			}
			FilterActive = filterActive;

			this.Filterable = true;
		}

		private class DisablableRestrictorAttribute : PXRestrictorAttribute
		{
			public bool Enabled { get; set; } = false;

			public DisablableRestrictorAttribute(Type where, string message, params Type[] pars) :
				base(where, message, pars) { }

			public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (Enabled)
				{
					base.FieldVerifying(sender, e);
				}
			}
		}
	}

	public class EmployeeAttribute : EmployeeAttributeBase
	{
		public EmployeeAttribute(bool filterActive = false) : base(filterActive, typeof(Search2<PREmployee.bAccountID,
			  LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<PREmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>>>),
			typeof(PREmployee.bAccountID), typeof(PREmployee.acctCD), typeof(PREmployee.acctName),
			typeof(PREmployee.status), typeof(PREmployee.employeeClassID), typeof(EPEmployeePosition.positionID), typeof(PREmployee.departmentID))
		{
		}
	}

	public class EmployeeActiveAttribute : EmployeeAttribute
	{
		public EmployeeActiveAttribute() : base(true) { }
	}

	public class EmployeeActiveInPayGroupAttribute : EmployeeAttributeBase
	{
		public EmployeeActiveInPayGroupAttribute() : base(true, typeof(Search2<PREmployee.bAccountID,
			  LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<PREmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>>,
			  Where<Current<PRPayment.payGroupID>, IsNull, Or<Current<PRPayment.payGroupID>, Equal<PREmployee.payGroupID>>>>),
			typeof(PREmployee.bAccountID), typeof(PREmployee.acctCD), typeof(PREmployee.acctName),
			typeof(PREmployee.employeeClassID), typeof(EPEmployeePosition.positionID), typeof(PREmployee.departmentID))
		{
		}
	}
}
