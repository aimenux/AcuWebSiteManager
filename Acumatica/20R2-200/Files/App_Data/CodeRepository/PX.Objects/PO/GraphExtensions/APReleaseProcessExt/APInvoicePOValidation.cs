using PX.Data;
using PX.Objects.Common;
using PX.Objects.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using POLineDTO = PX.Objects.PO.GraphExtensions.APInvoiceEntryExt.APInvoicePOValidation.POLineDTO;
using PX.Common;

namespace PX.Objects.PO.GraphExtensions.APReleaseProcessExt
{
	public class APInvoicePOValidation : PXGraphExtension<APReleaseProcess>
	{
		public delegate POLineUOpen updatePOLineDelegate(APTran tran, APInvoice apdoc, POLine srcLine, POOrder srcDoc, POLineUOpen updLine,
			HashSet<Tuple<string, string>> orderCheckClosed, bool isPrebooking);

		public PXSelect<POLineBillingRevision> POLineRevision;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.distributionModule>();
		}

		private APInvoicePOValidationService _validationService;
		public virtual APInvoicePOValidationService GetValidationService(Lazy<POSetup> poSetup)
		{
			if (_validationService == null)
				_validationService = new APInvoicePOValidationService(poSetup);
			return _validationService;
		}

		[PXOverride]
		public virtual POLineUOpen UpdatePOLine(APTran tran, APInvoice apdoc, POLine srcLine, POOrder srcDoc, POLineUOpen updLine,
			HashSet<Tuple<string, string>> orderCheckClosed, bool isPrebooking, updatePOLineDelegate baseMethod)
		{
			if (!Base.IsIntegrityCheck && !isPrebooking)
			{
				APInvoicePOValidationService validationService = GetValidationService(Lazy.By(() => Base.posetup));
				POLineDTO poLine = new POLineDTO(srcLine, srcDoc.CuryID);

				if (validationService.ShouldCreateRevision(Base.Caches[typeof(APTran)], tran, apdoc.CuryID, poLine))
					SavePOLineRevision(apdoc, poLine);
			}

			return baseMethod(tran, apdoc, srcLine, srcDoc, updLine, orderCheckClosed, isPrebooking);
		}

		[PXOverride]
		public virtual void ExtensionsPersist()
		{
			POLineRevision.Cache.Persist(PXDBOperation.Insert);
		}

		[PXOverride]
		public virtual void ExtensionsPersisted()
		{
			POLineRevision.Cache.Persisted(false);
		}

		public virtual void SavePOLineRevision(APInvoice apdoc, POLineDTO poLine)
		{
			var revision = new POLineBillingRevision();
			revision.APDocType = apdoc.DocType;
			revision.APRefNbr = apdoc.RefNbr;
			revision.OrderType = poLine.OrderType;
			revision.OrderNbr = poLine.OrderNbr;
			revision.OrderLineNbr = poLine.OrderLineNbr;
			revision.CuryID = poLine.CuryID;
			revision.UOM = poLine.UOM;
			revision.OrderQty = poLine.OrderQty;
			revision.BaseOrderQty = poLine.BaseOrderQty;
			revision.ReceivedQty = poLine.ReceivedQty;
			revision.BaseReceivedQty = poLine.BaseReceivedQty;
			revision.RcptQtyMax = poLine.RcptQtyMax;
			revision.UnbilledQty = poLine.UnbilledQty;
			revision.BaseUnbilledQty = poLine.BaseUnbilledQty;
			revision.CuryUnbilledAmt = poLine.CuryUnbilledAmt;
			revision.UnbilledAmt = poLine.UnbilledAmt;
			revision.CuryUnitCost = poLine.CuryUnitCost;
			revision.UnitCost = poLine.UnitCost;

			// Bill may contain two receipts of the same PO Order.
			if (POLineRevision.Locate(revision) == null)
			{
				POLineRevision.Insert(revision);
			}
		}
	}
}
