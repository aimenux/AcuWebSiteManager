namespace PX.Objects.CN.Compliance.CL.Descriptor
{
    public class Constants
    {
        public const string ComplianceDocumentViewName = "ComplianceDocuments";
        public const string LienWaiverReportFileNamePattern = "{0}\\LW-{1}-{2}-{3:MM-dd-yyyy}{4}";
        public const string LienWaiverReportFileNameSearchPattern = "{0}\\LW-{1}-{2}";

        public class LienWaiverDocumentTypeValues
        {
            public const string ConditionalPartial = "Conditional Partial";
            public const string ConditionalFinal = "Conditional Final";
            public const string UnconditionalPartial = "Unconditional Partial";
            public const string UnconditionalFinal = "Unconditional Final";
        }

        public class LienWaiverReportParameters
        {
            public const string ComplianceDocumentId = "ComplianceDocumentId";
            public const string DeviceHubComplianceDocumentId = "ComplianceDocument.ComplianceDocumentID";
            public const string IsJointCheck = "IsJointCheck";
        }

        public class ComplianceNotification
        {
            public const string LienWaiverNotificationSourceCd = "Vendor";
        }
    }
}