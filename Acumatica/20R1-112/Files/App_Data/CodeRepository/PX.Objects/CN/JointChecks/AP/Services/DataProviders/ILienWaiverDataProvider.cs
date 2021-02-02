using System;
using System.Collections.Generic;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Models;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
    public interface ILienWaiverDataProvider
    {
        bool DoesAnyOutstandingComplianceExistForPrimaryVendor(int? vendorId, IEnumerable<int?> projectIds);

        bool DoesAnyOutstandingComplianceExistForJointVendor(string externalName, IEnumerable<int?> projectIds);

        bool DoesAnyOutstandingComplianceExistForJointVendor(int? internalId, IEnumerable<int?> projectIds);

        bool DoesAnyOutstandingComplianceExistForJointVendor(JointPayee jointPayee, List<int?> projectIds);

        bool DoesAnyOutstandingComplianceExist(APRegister payment);

        IEnumerable<ComplianceDocument> GetOutstandingCompliancesForPrimaryVendor(int? vendorId,
            IEnumerable<int?> projectIds);

        IEnumerable<ComplianceDocument> GetOutstandingCompliancesForJointVendor(string externalName,
            IEnumerable<int?> projectIds);

        IEnumerable<ComplianceDocument> GetOutstandingCompliancesForJointVendor(int? internalId,
            IEnumerable<int?> projectIds);

        IEnumerable<ComplianceDocument> GetOutstandingCompliancesForJointVendor(JointPayee jointPayee,
            List<int?> projectIds);

        IEnumerable<ComplianceDocument> GetLienWaivers(Guid? checkNoteId);

        IEnumerable<ComplianceDocument> GetNotVoidedLienWaivers(LienWaiverGenerationKey generationKey);

        ComplianceAttribute GetComplianceAttribute(string complianceAttributeValue);
    }
}