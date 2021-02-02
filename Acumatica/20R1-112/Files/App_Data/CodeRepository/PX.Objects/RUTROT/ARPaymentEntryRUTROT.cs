using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;


namespace PX.Objects.RUTROT
{
	public class ARPaymentEntryRUTROT : PXGraphExtension<ARPaymentEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}

		public PXSelectJoin<ARAdjust, LeftJoin<ARInvoice,
			On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>, And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
			LeftJoin<RUTROT, On<RUTROT.docType, Equal<ARInvoice.docType>, And<RUTROT.refNbr, Equal<ARInvoice.refNbr>>>>>,
			Where<ARAdjust.adjgDocType, Equal<Current<ARPayment.docType>>,
				And<ARAdjust.adjgRefNbr, Equal<Current<ARPayment.refNbr>>,
					And<ARAdjust.adjNbr, Equal<Current<ARPayment.adjCntr>>>>>> Adjustments;

		protected void SetPaymentAmount(decimal? amount)
		{
		    if (amount == 0m)
		    {
		        return;
		    }

			Base.Document.Current.CuryOrigDocAmt = amount;
			Base.Document.Update(Base.Document.Current);

			var adj = Base.Adjustments.SelectSingle();
			adj.CuryAdjgAmt = amount;
			Base.Adjustments.Cache.Update(adj);
		}
		[PXOverride]
		public virtual void CreatePayment(ARInvoice ardoc)
		{
			Base.CreatePayment(ardoc, null, null, null, true);
			RUTROT rowRR = PXSelect<RUTROT, Where<RUTROT.docType, Equal<Required<ARInvoice.docType>>,
			And<RUTROT.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this.Base, ardoc.DocType, ardoc.RefNbr);
			if (PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>() 
				&& ardoc != null 
				&& PXCache<ARInvoice>.GetExtension<ARInvoiceRUTROT>(ardoc)?.IsRUTROTDeductible == true
                && Base.Document.Current.DocType == ARDocType.Payment)
			{
			    bool isInvoiceBalanced = rowRR.BalancingCreditMemoRefNbr != null && rowRR.BalancingDebitMemoRefNbr != null;
			    
                if (rowRR.IsClaimed != true)
			    {
					decimal curyDistributedAmt = 0m;

					if (isInvoiceBalanced == false)
					{
						PXCache rUTROTCache = Base.Caches[typeof(RUTROT)];
						CM.PXCurrencyAttribute.CuryConvCury<ARPayment.curyInfoID>(Base.Document.Cache, Base.Document.Current, rowRR.DistributedAmt ?? 0m, out curyDistributedAmt);
						rUTROTCache.SetValue<RUTROT.curyDistributedAmt>(rowRR, curyDistributedAmt);
					}

					SetPaymentAmount(Math.Max((Base.Document.Current.CuryDocBal - curyDistributedAmt) ?? 0m, 0m));
			    }
			}
		}
		[PXDecimal]
		protected virtual void ARInvoice_CuryRUTROTUndistributedAmt_CacheAttached(PXCache sender)
		{
		}

		protected virtual void ARPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARPayment doc = e.Row as ARPayment;

			Branch branch = Base.CurrentBranch.SelectSingle(doc.BranchID);
			var adjustments = Adjustments.Select().AsEnumerable();
			List<ARInvoice> invoices = adjustments.Select(d => d.GetItem<ARInvoice>()).ToList();
			List<RUTROT> rutrots = adjustments.Select(d => d.GetItem<RUTROT>()).ToList();
			bool hasRutRotInvoices = invoices.Any(d => (d != null && PXCache<ARInvoice>.GetExtension<ARInvoiceRUTROT>(d)?.IsRUTROTDeductible == true));
			bool someInvoiceClaimed = rutrots.Any(d => (d != null && d.IsClaimed == true));

			PXUIFieldAttribute.SetVisible<ARPaymentRUTROT.isRUTROTPayment>(Base.Document.Cache, e.Row, branch != null
																		&& PXCache<Branch>.GetExtension<BranchRUTROT>(branch).AllowsRUTROT == true
																		&& DocTypeSuits(doc));
			PXUIFieldAttribute.SetEnabled<ARPaymentRUTROT.isRUTROTPayment>(Base.Document.Cache, e.Row, branch != null
																		&& PXCache<Branch>.GetExtension<BranchRUTROT>(branch).RUTROTCuryID == doc.CuryID
																		&& doc.Released != true
																		&& (someInvoiceClaimed == true || hasRutRotInvoices == false));
		}

		private bool DocTypeSuits(ARPayment payment)
		{
			if (payment == null)
			{
				return true;
			}

			return payment.DocType == ARDocType.Payment || payment.DocType == ARDocType.VoidPayment || payment.DocType == ARDocType.CreditMemo;
		}
	}
}
