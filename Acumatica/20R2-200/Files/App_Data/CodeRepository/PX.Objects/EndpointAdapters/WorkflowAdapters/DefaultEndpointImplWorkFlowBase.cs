using PX.Api.ContractBased.Adapters;
using PX.Api.ContractBased.Models;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EndpointAdapters
{
	internal class DefaultEndpointImplWorkFlowBase : IAdapterWithMetadata
	{
		public IEntityMetadataProvider MetadataProvider { protected get; set; }

		protected CbApiWorkflowApplicator.SalesOrderApplicator SalesOrderApplicator { get; }
		protected CbApiWorkflowApplicator.ShipmentApplicator ShipmentApplicator { get; }
		protected CbApiWorkflowApplicator.SalesInvoiceApplicator SalesInvoiceApplicator { get; }

		public DefaultEndpointImplWorkFlowBase(
			CbApiWorkflowApplicator.SalesOrderApplicator salesOrderApplicator,
			CbApiWorkflowApplicator.ShipmentApplicator shipmentApplicator,
			CbApiWorkflowApplicator.SalesInvoiceApplicator salesInvoiceApplicator)
		{
			SalesOrderApplicator = salesOrderApplicator;
			ShipmentApplicator = shipmentApplicator;
			SalesInvoiceApplicator = salesInvoiceApplicator;
		}
	}
}
