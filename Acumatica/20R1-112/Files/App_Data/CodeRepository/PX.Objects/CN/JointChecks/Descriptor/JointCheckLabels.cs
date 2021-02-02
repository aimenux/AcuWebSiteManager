using PX.Common;

namespace PX.Objects.CN.JointChecks.Descriptor
{
    [PXLocalizable]
    public static class JointCheckLabels
    {
	    public const string JointPaidAmount = "Joint Paid Amount";
        public const string JointAmountToPay = "Joint Amount To Pay";

        public const string VendorBalance = "Balance";
        public const string MaxAvailableAmount = "Max Available Amount";

        public const string BillLineNumber = "Bill Line Nbr.";
        public const string ApBillLineNumber = "AP Bill Line Nbr.";
        public const string ApBillNbr = "AP Bill Nbr.";

        [PXLocalizable]
        public static class BillLine
        {
	        public const string TransactionDescription = "Transaction Desc.";
            public const string Project = "Project";
            public const string ProjectTask = "Project Task";
            public const string CostCode = "Cost Code";
            public const string Account = "Account";
            public const string Balance = "Balance";
            public const string LineNumber = "Line Nbr.";
        }
    }
}