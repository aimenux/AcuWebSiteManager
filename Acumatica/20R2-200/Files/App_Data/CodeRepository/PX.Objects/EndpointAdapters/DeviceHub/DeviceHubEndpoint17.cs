using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Data;
using System.Linq;
using System;
using PX.Data.BQL.Fluent;
using PX.SM;
using PX.Data.BQL;

namespace PX.Objects.EndpointAdapters
{
	[PXVersion("17.200.001", "DeviceHub")]
	public class DeviceHubEndpoint17 : DefaultEndpointImpl
	{
		private string defaultDeviceHubID = "DEFAULT";

		[FieldsProcessed(new[] {
			"PrinterID",
			"PrinterName",
			"Description",
			"IsActive"
		})]
		protected void Printer_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			var printerName = targetEntity.Fields.SingleOrDefault(f => f.Name == "PrinterName") as EntityValueField;
			var description = targetEntity.Fields.SingleOrDefault(f => f.Name == "Description") as EntityValueField;
			var isActive = targetEntity.Fields.SingleOrDefault(f => f.Name == "IsActive") as EntityValueField;

			if (printerName != null && printerName.Value != null)
			{
				PX.SM.SMPrinterMaint newPrinterGraph = (PX.SM.SMPrinterMaint)PXGraph.CreateInstance(typeof(PX.SM.SMPrinterMaint));

				SMPrinter existingPrinter = SelectFrom<SMPrinter>.Where<SMPrinter.deviceHubID.IsEqual<@P.AsString>.And<SMPrinter.printerName.IsEqual<@P.AsString>>>.View.Select(newPrinterGraph, defaultDeviceHubID, printerName.Value);
				if (existingPrinter != null)
					return;

				PX.SM.SMPrinter printer = new PX.SM.SMPrinter();
				printer.PrinterID = Guid.NewGuid();
				printer.DeviceHubID = defaultDeviceHubID;
				printer.PrinterName = printerName.Value;
				if (description != null)
					printer.Description = description.Value;
				if (isActive != null)
					printer.IsActive = isActive.Value == "true";

				newPrinterGraph.Printers.Insert(printer);
				newPrinterGraph.Save.Press();
			}
		}

		[FieldsProcessed(new[] {
			"PrinterName",
			"Description",
			"IsActive"
		})]
		protected void Printer_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			PX.SM.SMPrinterMaint newPrinterGraph = (PX.SM.SMPrinterMaint)PXGraph.CreateInstance(typeof(PX.SM.SMPrinterMaint));
			var isActive = targetEntity.Fields.SingleOrDefault(f => f.Name == "IsActive") as EntityValueField;
			string printerName = entity.InternalKeys["Printers"]["PrinterName"];

			foreach (PX.SM.SMPrinter existingPrinter in newPrinterGraph.Printers.Select())
			{
				if (existingPrinter.PrinterName == printerName && isActive != null && isActive.Value != null)
				{
					existingPrinter.IsActive = isActive.Value == "true";
					newPrinterGraph.Printers.Update(existingPrinter);
				}
			}
			if (newPrinterGraph.Printers.Cache.IsDirty)
			{
				newPrinterGraph.Save.Press();
			}
		}

		[FieldsProcessed(new[] {
			"JobID",
			"Printer",
			"ReportID",
			"Status"
		})]
		protected void PrintJob_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			PX.SM.SMPrintJobMaint newPrintJobGraph = (PX.SM.SMPrintJobMaint)PXGraph.CreateInstance(typeof(PX.SM.SMPrintJobMaint));
			var status = targetEntity.Fields.SingleOrDefault(f => f.Name == "Status") as EntityValueField;
			int jobID;
			int.TryParse(entity.InternalKeys["Job"]["JobID"], out jobID);

			if (jobID != 0 && status != null && status.Value != null)
			{
				foreach (PX.SM.SMPrintJob existingPrintJob in PXSelect<PX.SM.SMPrintJob, Where<PX.SM.SMPrintJob.jobID, Equal<Required<PX.SM.SMPrintJob.jobID>>>>.Select(newPrintJobGraph, jobID))
				{
					existingPrintJob.Status = status.Value; //status is expected in char form - D, P, F or U
					newPrintJobGraph.Job.Update(existingPrintJob);
				}
				if (newPrintJobGraph.Job.Cache.IsDirty)
				{
					newPrintJobGraph.Save.Press();
				}
			}
		}

	}
}