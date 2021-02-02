using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AP
{
    /// <summary>
    /// This class provide information about state <see cref="APInvoice"></c>
    /// </summary>
    public class APInvoiceState
    {
        public bool DontApprove;
        public bool HasPOLink;
        public bool IsDocumentPrepayment;
        public bool IsDocumentInvoice;
        public bool IsDocumentDebitAdjustment;
        public bool IsDocumentCreditAdjustment;
        public bool IsDocumentOnHold;
        public bool IsDocumentScheduled;
        public bool IsDocumentPrebookedNotCompleted;
        public bool IsDocumentReleasedOrPrebooked;
        public bool IsDocumentVoided;
        public bool IsDocumentRejected;
        public bool RetainageApply;
        public bool IsRetainageDocument;
        public bool IsRetainageDebAdj;
        public bool IsDocumentRejectedOrPendingApproval;
        public bool IsDocumentApprovedBalanced;
        public bool LandedCostEnabled;
        public bool IsFromExpenseClaims;
        public bool AllowAddPOByProject;
		public bool IsCuryEnabled;
		public bool IsFromPO;
		public bool IsPrepaymentRequestFromPO => IsDocumentPrepayment && IsFromPO;
        public bool IsDocumentEditable => !IsDocumentReleasedOrPrebooked && !IsDocumentRejectedOrPendingApproval && !IsDocumentApprovedBalanced;
    }
}
