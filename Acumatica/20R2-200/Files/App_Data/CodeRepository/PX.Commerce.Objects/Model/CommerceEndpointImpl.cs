using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Commerce.Core;
using PX.SM;
using PX.Data;
using PX.Api;
using PX.Api.ContractBased.Models;
using PX.Api.ContractBased;
using PX.Objects.CA;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Commerce.Core.API;
using PX.Objects.SO;
using PX.Commerce.Objects;
using System.Drawing;
using System.Globalization;
using PX.Objects.CS;

namespace PX.Commerce.Objects
{
	[PXVersion("20.200.001", "eCommerce")]
	public class CommerceEndpointImpl20
	{
		[FieldsProcessed(new[] {
			"Freight"
		})]
		protected virtual void Totals_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			Totals_Handler(graph, targetEntity);
		}
		[FieldsProcessed(new[] {
			"Freight"
		})]
		protected virtual void Totals_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			Totals_Handler(graph, targetEntity);
		}
		protected virtual void Totals_Handler(PXGraph graph, EntityImpl targetEntity)
		{
			SOOrderEntry sograph = graph as SOOrderEntry;

			decimal? freight = null;
			EntityValueField freightField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Freight") as EntityValueField;
			if (freightField?.Value != null)
				freight = decimal.Parse(freightField.Value, CultureInfo.InvariantCulture);
			if (freight != null && sograph.Document.Current != null)
			{
				SOOrder order = sograph.Document.Current;
				ShipTerms shipTerms = order.ShipTermsID != null ? PXSelectorAttribute.Select<SOOrder.shipTermsID>(sograph.Document.Cache, order) as ShipTerms : null;

				bool saveToFreight = order.OverrideFreightAmount == true || shipTerms == null || shipTerms.FreightAmountSource == FreightAmountSourceAttribute.OrderBased;
				if (saveToFreight)
				{
					if(order.OverrideFreightAmount != true)
						sograph.Document.Cache.SetValueExt<SOOrder.overrideFreightAmount>(order, true);
					sograph.Document.Cache.SetValueExt<SOOrder.curyFreightAmt>(order, freight);
				}
				else
					sograph.Document.Cache.SetValueExt<SOOrder.curyPremiumFreightAmt>(order, freight);

				order = sograph.Document.Update(order);
			}
		}
	}
}