using System;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.CT
{
	public class ContractAR677000 : IBqlTable
	{
		#region ContractCD
		[PXDimensionSelector(ContractAttribute.DimensionName,
			typeof(Search2<Contract.contractCD, 
				InnerJoin<ContractBillingSchedule, 
					On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>, 
				LeftJoin<Customer, 
					On<Customer.bAccountID, Equal<Contract.customerID>>>>, 
				Where<Contract.baseType, Equal<CTPRType.contract>,
					And<Where<Contract.templateID, Equal<Optional<Contract.templateID>>,
						Or<Optional<Contract.templateID>, IsNull>>>>>),
			typeof(Contract.contractCD),
			typeof(Contract.contractCD), typeof(Contract.customerID), 
			typeof(Customer.acctName), typeof(Contract.locationID), 
			typeof(Contract.description), typeof(Contract.status), 
			typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), 
			typeof(ContractBillingSchedule.nextDate), DescriptionField = typeof(Contract.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible)]
		public String ContractCD { get; set; }
		#endregion
	}
}
