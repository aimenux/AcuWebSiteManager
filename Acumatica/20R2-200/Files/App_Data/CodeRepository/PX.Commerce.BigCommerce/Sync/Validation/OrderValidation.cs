using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Commerce.Core;
using PX.Commerce.Objects;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Commerce.BigCommerce.API.REST;
using PX.Objects.SO;
using PX.Commerce.Core.API;
using PX.Objects.CA;

namespace PX.Commerce.BigCommerce
{
	public class OrderValidator : BCBaseValidator, ISettingsValidator, ILocalValidator
	{
		public int Priority { get { return 0; } }

		public virtual void Validate(IProcessor iproc)
		{
			Validate<BCSalesOrderProcessor>(iproc, (processor) =>
			{
				BCEntity entity = processor.GetEntity();
				BCBinding store = processor.GetBinding();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();

				//Order Types
				if (storeExt.OrderType == null)
					throw new PXException(BigCommerceMessages.NoSalesOrderType);

				//Branch
				if (store.BranchID == null)
					throw new PXException(BigCommerceMessages.NoBranch);

				//Numberings
				SOOrderType type = PXSelect<SOOrderType, Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>>>.Select(processor, storeExt.OrderType);
				BCAutoNumberAttribute.CheckAutoNumbering(processor, type.OrderNumberingID);

			});
			Validate<BCPaymentProcessor>(iproc, (processor) =>
			{
				BCEntity entity = processor.GetEntity();
				BCBinding store = processor.GetBinding();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();

				//Branch
				if (store.BranchID == null)
					throw new PXException(BigCommerceMessages.NoBranch);

				//Numberings
				ARSetup type = PXSelect<ARSetup>.Select(processor);
				BCAutoNumberAttribute.CheckAutoNumbering(processor, type.PaymentNumberingID);
			});
		}

		public void Validate(IProcessor iproc, ILocalEntity ilocal)
		{
		}
	}
}
