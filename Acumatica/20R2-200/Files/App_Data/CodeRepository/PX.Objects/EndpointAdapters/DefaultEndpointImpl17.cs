using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Common;
using PX.Data;

namespace PX.Objects.EndpointAdapters
{
	[PXInternalUseOnly]
	[PXVersion("5.30.001", "Default")]
	[PXVersion("6.00.001", "Default")]
	[PXVersion("17.200.001", "Default")]
	public class DefaultEndpointImpl17 : DefaultEndpointImpl
	{
		//there will be something about AC-110938 (AC-107928)


		[FieldsProcessed(new[] {
			"AttributeID",
			"Value"
		})]
		protected void AttributeValue_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			AttributeBase_Insert(graph, entity, targetEntity, "AttributeID");
		}

		[FieldsProcessed(new[] {
			"Attribute",
			"Value"
		})]
		protected void AttributeDetail_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			// TODO: merge AttributeDetail and AttributeValue entities in new endpoint version (2019r..)
			AttributeBase_Insert(graph, entity, targetEntity, "Attribute");
		}

	}
}
