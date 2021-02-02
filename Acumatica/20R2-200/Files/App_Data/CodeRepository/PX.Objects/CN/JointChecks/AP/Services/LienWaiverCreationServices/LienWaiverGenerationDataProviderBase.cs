using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.AP.CacheExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.PO;
using ApTranExt = PX.Objects.CN.Subcontracts.AP.CacheExtensions.ApTranExt;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public class LienWaiverGenerationDataProviderBase
    {
        protected readonly APPaymentEntry Graph;
        protected readonly ILienWaiverTransactionRetriever TransactionRetriever;
        protected List<APTran> Transactions;

		private LienWaiverSetup lienWaiverSetup;
		[Obsolete(Objects.Common.Messages.PropertySetIsObsoleteAndWillBeRemoved2021R1)]
		protected LienWaiverSetup LienWaiverSetup
		{
			get
			{
				if (lienWaiverSetup == null)
				{
					lienWaiverSetup = Graph.GetExtension<ApPaymentEntryLienWaiverExtension>().LienWaiverSetup.Current;
				}
				return lienWaiverSetup;
			}
			set
			{
				lienWaiverSetup = value;
			}
		}
		public LienWaiverGenerationDataProviderBase(PXGraph graph)
        {
            Graph = (APPaymentEntry) graph;
            LienWaiverDataProvider = graph.GetService<ILienWaiverDataProvider>();
            TransactionRetriever = graph.GetService<ILienWaiverTransactionRetriever>();
        }

        public ILienWaiverDataProvider LienWaiverDataProvider
        {
            get;
            set;
        }

        protected bool AreThereAnyLienWaiverRecipientsForVendorClass(int? vendorId, int? projectId)
        {
            var vendor = VendorDataProvider.GetVendor(Graph, vendorId);
            var lienWaiverRecipient =
                LienWaiverRecipientDataProvider.GetLienWaiverRecipient(Graph, vendor.VendorClassID, projectId);
            return PXCache<Vendor>.GetExtension<VendorExtension>(vendor).ShouldGenerateLienWaivers == true &&
                lienWaiverRecipient != null;
        }

        protected bool DoesCommitmentAmountExceedMinimumCommitmentAmount(int? vendorId, APTran transaction)
        {
            var commitment = GetCommitment(transaction);
            if (commitment == null)
            {
                return false;
            }
            var vendor = VendorDataProvider.GetVendor(Graph, vendorId);
            var projectId = LienWaiverProjectDataProvider.GetProjectId(Graph, transaction);
            var lienWaiverRecipient = LienWaiverRecipientDataProvider.GetLienWaiverRecipient(Graph,
                vendor.VendorClassID, projectId);
            return commitment.CuryOrderTotal > lienWaiverRecipient.MinimumCommitmentAmount;
        }

        protected POOrder GetCommitment(APTran transaction)
        {
            var originalTransaction = TransactionDataProvider.GetOriginalTransaction(Graph, transaction);
            var transactionExtension = PXCache<APTran>.GetExtension<ApTranExt>(originalTransaction);
            return transactionExtension.SubcontractNbr != null
                ? CommitmentDataProvider.GetCommitment(Graph, transactionExtension.SubcontractNbr,
                    POOrderType.RegularSubcontract)
                : originalTransaction.PONbr != null
                    ? CommitmentDataProvider.GetCommitment(Graph, originalTransaction.PONbr, POOrderType.RegularOrder)
                    : null;
        }
    }
}