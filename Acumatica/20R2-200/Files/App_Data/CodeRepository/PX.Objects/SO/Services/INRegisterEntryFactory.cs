using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.SO.Services
{
	public class INRegisterEntryFactory
	{
		protected SOShipmentEntry _shipGraph;

		protected string _lastShipmentType;
		protected INRegisterEntryBase _lastCreatedGraph;

		public INRegisterEntryFactory(SOShipmentEntry shipGraph)
		{
			_shipGraph = shipGraph;
		}

		public virtual INRegisterEntryBase GetOrCreateINRegisterEntry(SOShipment shipment)
		{
			if (shipment.ShipmentType == _lastShipmentType)
			{
				_lastCreatedGraph.Clear();
				return _lastCreatedGraph;
			}
			
			var ie = (shipment.ShipmentType == SOShipmentType.Transfer)
				? (INRegisterEntryBase)PXGraph.CreateInstance<INTransferEntry>()
				: (INRegisterEntryBase)PXGraph.CreateInstance<INIssueEntry>();

			_shipGraph.MergeCachesWithINRegisterEntry(ie);

			_lastShipmentType = shipment.ShipmentType;
			_lastCreatedGraph = ie;

			return ie;
		}
	}
}
