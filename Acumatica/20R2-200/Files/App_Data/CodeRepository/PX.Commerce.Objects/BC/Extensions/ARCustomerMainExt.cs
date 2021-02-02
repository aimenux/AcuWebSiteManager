using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.SM;

namespace PX.Commerce.Objects
{
	public class BCCustomerMaintExt : PXGraphExtension<CustomerMaint>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		//Dimension Key Generator
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[BCCustomNumbering(CustomerRawAttribute.DimensionName, typeof(BCBindingExt.customerTemplate), typeof(BCBindingExt.customerNumberingID),
			typeof(Select2<BCBindingExt,
				InnerJoin<BCBinding, On<BCBinding.bindingID, Equal<BCBindingExt.bindingID>>>,
				Where<BCBinding.connectorType, Equal<Required<BCBinding.connectorType>>,
					And<BCBinding.bindingID, Equal<Required<BCBinding.bindingID>>>>>))]
		public void _(Events.CacheAttached<Customer.acctCD> e) { }

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void Customer_LastModifiedDateTime_CacheAttached(PXCache sender) { }
	}
}
