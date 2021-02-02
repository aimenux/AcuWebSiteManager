using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.RUTROT
{
	public class ARScheduleProcessRUTROT : PXGraphExtension<ARScheduleProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}

		public delegate ARInvoice InsertDocumentDelegate(ARInvoiceEntry docgraph, ScheduleDet sdet, Customer customer, ARInvoice ardoc, CurrencyInfo info);
		[PXOverride]
		public virtual ARInvoice InsertDocument(ARInvoiceEntry docgraph, ScheduleDet sdet, Customer customer, ARInvoice ardoc, CurrencyInfo info, InsertDocumentDelegate baseMethod)
		{
			ARInvoice new_ardoc = baseMethod(docgraph, sdet, customer, ardoc, info);
			if (PXCache<ARInvoice>.GetExtension<ARInvoiceRUTROT>(ardoc)?.IsRUTROTDeductible == true)
			{
				RUTROT rutrot = PXSelect<RUTROT, 
					Where<RUTROT.docType, Equal<Required<ARInvoice.docType>>, 
					And<RUTROT.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(docgraph, ardoc.DocType, ardoc.RefNbr);
				RUTROT new_rutrot = PXCache<RUTROT>.CreateCopy(rutrot);
				new_rutrot.CuryDistributedAmt = 0m;
				new_rutrot.CuryTotalAmt = 0m;
				new_rutrot.CuryUndistributedAmt = 0m;
				new_rutrot.DistributedAmt = 0m;
				new_rutrot.TotalAmt = 0m;
				new_rutrot.UndistributedAmt = 0m;
				new_rutrot.WorkPrice = 0m;
				new_rutrot.MaterialCost = 0m;
				new_rutrot.OtherCost = 0m;
				new_rutrot.CuryWorkPrice = 0m;
				new_rutrot.CuryMaterialCost = 0m;
				new_rutrot.CuryOtherCost = 0m;
				new_rutrot.RefNbr = null;
				new_rutrot.NoteID = null;
				docgraph.Rutrots.Insert(new_rutrot);
			}
			return new_ardoc;
		}

		public delegate void InsertDetailsDelegate(ARInvoiceEntry docgraph, ARInvoice ardoc, ARInvoice new_ardoc);
		[PXOverride]
		public virtual void InsertDetails(ARInvoiceEntry docgraph, ARInvoice ardoc, ARInvoice new_ardoc, InsertDetailsDelegate baseMethod)
		{
			docgraph.FieldUpdated.RemoveHandler<ARTran.inventoryID>(docgraph.GetExtension<ARInvoiceEntryRUTROT>().ARTran_InventoryID_FieldUpdated);
			baseMethod(docgraph, ardoc, new_ardoc);
			if (PXCache<ARInvoice>.GetExtension<ARInvoiceRUTROT>(ardoc)?.IsRUTROTDeductible == true)
			{
				foreach (RUTROTDistribution rrDetail in PXSelect<RUTROTDistribution, 
					Where<RUTROTDistribution.docType, Equal<Required<ARInvoice.docType>>, 
					And<RUTROTDistribution.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(docgraph, ardoc.DocType, ardoc.RefNbr))
				{
					RUTROTDistribution new_rrDetail = PXCache<RUTROTDistribution>.CreateCopy(rrDetail);
					new_rrDetail.RefNbr = null;
					new_rrDetail.CuryInfoID = null;
					new_rrDetail.LineNbr = null;
					new_rrDetail.CuryAmount = 0m;
					new_rrDetail.Amount = 0m;
					docgraph.RRDistribution.Insert(new_rrDetail);
				}
			}
		}
	}
}