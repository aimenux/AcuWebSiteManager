using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices
{
    public class LienWaiverValidationService
    {
        private readonly LienWaiverDataProvider lienWaiverDataProvider;
        private readonly LienWaiverWarningMessageService lienWaiverWarningMessageService;

        public LienWaiverValidationService(PXGraph graph, IProjectDataProvider projectDataProvider)
        {
            lienWaiverWarningMessageService = new LienWaiverWarningMessageService(graph, projectDataProvider);
            lienWaiverDataProvider = new LienWaiverDataProvider(graph);
        }

        public void ValidatePrimaryVendor<TField>(PXCache cache, object row, int? vendorId,
            IEnumerable<int?> projectIds, string message, PXErrorLevel errorLevel = PXErrorLevel.Warning)
            where TField : IBqlField
        {
            var doesAnyOutstandingComplianceExist = lienWaiverDataProvider
                .DoesAnyOutstandingComplianceExistForPrimaryVendor(vendorId, projectIds);
            SetWarningIfNeeded<TField>(cache, row, message, doesAnyOutstandingComplianceExist, errorLevel);
        }

        public void ValidateJointPayee(PXCache cache, JointPayee jointPayee, List<int?> projectIds,
            string message, PXErrorLevel errorLevel = PXErrorLevel.Warning)
        {
            if (jointPayee.JointPayeeExternalName == null || jointPayee.JointPayeeInternalId == null)
            {
                var doesAnyOutstandingComplianceExist = lienWaiverDataProvider
                    .DoesAnyOutstandingComplianceExistForJointVendor(jointPayee.JointPayeeInternalId, projectIds);
                SetWarningIfNeeded<JointPayee.jointPayeeInternalId>(cache, jointPayee,
                    message, doesAnyOutstandingComplianceExist, errorLevel);
                doesAnyOutstandingComplianceExist = lienWaiverDataProvider
                    .DoesAnyOutstandingComplianceExistForJointVendor(jointPayee.JointPayeeExternalName, projectIds);
                SetWarningIfNeeded<JointPayee.jointPayeeExternalName>(cache, jointPayee,
                    message, doesAnyOutstandingComplianceExist, errorLevel);
            }
        }

        public void ValidateVendorAndJointPayees(PXCache cache, APAdjust adjustment, List<JointPayee> jointPayees)
        {
            var projectIds = LienWaiverProjectDataProvider.GetProjectIds(adjustment, cache.Graph).ToList();
            var doesAnyOutstandingComplianceExistForJointVendors = jointPayees.Any(jp =>
                lienWaiverDataProvider.DoesAnyOutstandingComplianceExistForJointVendor(jp, projectIds));
            var outstandingCompliances = lienWaiverDataProvider
                .GetOutstandingCompliancesForPrimaryVendor(adjustment.VendorID, projectIds).ToList();
            if (outstandingCompliances.Any() && doesAnyOutstandingComplianceExistForJointVendors)
            {
                ValidateVendorAndJointPayees(cache, jointPayees, adjustment, projectIds,
                    outstandingCompliances);
            }
            else if (outstandingCompliances.Any())
            {
                ValidatePrimaryVendor(cache, adjustment, projectIds, outstandingCompliances);
            }
            else if (doesAnyOutstandingComplianceExistForJointVendors)
            {
                jointPayees.ForEach(jp => ValidateJointPayee(cache, jp, adjustment, projectIds));
            }
            else
            {
                cache.ClearFieldErrors<APAdjust.adjdLineNbr>(adjustment);
            }
        }

        private void ValidatePrimaryVendor(PXCache cache, APAdjust adjustment, IEnumerable<int?> projectIds,
            IEnumerable<ComplianceDocument> outstandingCompliances)
        {
            var warningMessage = lienWaiverWarningMessageService.CreateWarningMessage(outstandingCompliances);
            var formattedWarningMessage = PXMessages.LocalizeFormatNoPrefix(warningMessage);
            ValidatePrimaryVendor<APAdjust.adjdLineNbr>(cache, adjustment, adjustment.VendorID, projectIds,
                formattedWarningMessage, PXErrorLevel.RowWarning);
        }

        private void ValidateJointPayee(PXCache cache, JointPayee jointPayee, APAdjust adjustment,
            List<int?> projectIds)
        {
            var message = string.Concat(ComplianceMessages.LienWaiver.JointPayeeHasOutstandingLienWaiver,
                Constants.WarningMessageSymbols.NewLine);
            var warningMessage = lienWaiverWarningMessageService.CreateWarningMessage(jointPayee, message, projectIds);
            var formattedWarningMessage = PXMessages.LocalizeFormatNoPrefix(warningMessage);
            var doesAnyOutstandingComplianceExistForJointCheck = lienWaiverDataProvider
                .DoesAnyOutstandingComplianceExistForJointVendor(jointPayee, projectIds);
            SetWarningIfNeeded<APAdjust.adjdLineNbr>(cache, adjustment, formattedWarningMessage,
                doesAnyOutstandingComplianceExistForJointCheck, PXErrorLevel.RowWarning);
        }

        private void ValidateVendorAndJointPayees(PXCache cache, IEnumerable<JointPayee> jointPayees,
            APAdjust adjustment, IEnumerable<int?> projectIds,
            IEnumerable<ComplianceDocument> outstandingCompliancesForPrimaryVendor)
        {
            var vendorMessage = lienWaiverWarningMessageService
                .CreateVendorWarningMessage(outstandingCompliancesForPrimaryVendor);
            var warningMessage = string.Concat(ComplianceMessages.LienWaiver.VendorAndJointPayeeHaveOutstandingLienWaiver,
                Constants.WarningMessageSymbols.NewLine, vendorMessage);
            warningMessage = jointPayees.Aggregate(warningMessage,
                (current, jointPayee) => string.Concat(current, lienWaiverWarningMessageService.CreateWarningMessage(
                    jointPayee, Constants.WarningMessageSymbols.NewLine, projectIds.ToList())));
            var formattedWarningMessage = PXMessages.LocalizeFormatNoPrefix(warningMessage);
            SetWarningIfNeeded<APAdjust.adjdLineNbr>(cache, adjustment, formattedWarningMessage, true,
                PXErrorLevel.RowWarning);
        }

        private static void SetWarningIfNeeded<TField>(PXCache cache, object row, string message,
            bool doesAnyOutstandingComplianceExist, PXErrorLevel errorLevel = PXErrorLevel.Warning)
            where TField : IBqlField
        {
            if (doesAnyOutstandingComplianceExist)
            {
                cache.RaiseException<TField>(row, message, errorLevel: errorLevel);
            }
            else
            {
                cache.ClearFieldErrorIfExists<TField>(row, message);
            }
        }
    }
}