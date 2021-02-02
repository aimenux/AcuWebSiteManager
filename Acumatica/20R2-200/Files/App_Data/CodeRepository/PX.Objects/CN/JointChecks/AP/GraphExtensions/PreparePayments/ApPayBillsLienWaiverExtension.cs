using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.PreparePaymentsServices;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PreparePayments
{
    public class ApPayBillsLienWaiverExtension : PXGraphExtension<ApPayBillsExt, APPayBills>
    {
	    public PXSetup<LienWaiverSetup> LienWaiverSetup;
        public PXSetup<APSetup> AccountsPayableSetup;

        private LienWaiverDataProvider lienWaiverDataProvider;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public override void Initialize()
        {
            lienWaiverDataProvider = new LienWaiverDataProvider(Base);
        }

        protected virtual void _(Events.RowSelected<APAdjust> args)
        {
            var adjustment = args.Row;
            if (adjustment != null && LienWaiverWarningsCacheService.ShouldShowWarning(adjustment))
            {
                args.Cache.RaiseException<APAdjust.vendorID>(adjustment,
                    ComplianceMessages.LienWaiver.BillHasOneOrMoreOutstandingLienWaivers, errorLevel: PXErrorLevel.RowWarning);
            }

            Base.APDocumentList.SetParametersDelegate(ConfirmProcessing);
        }

        protected virtual void _(Events.RowInserted<APAdjust> args)
        {
            var adjustment = args.Row;
            if (adjustment != null && LienWaiverSetup.Current.ShouldWarnOnPayment == true &&
                DoesAnyOutstandingLienWaiverExist(adjustment))
            {
                LienWaiverWarningsCacheService.Add(adjustment);
            }
        }

        protected virtual bool ConfirmProcessing(List<APAdjust> adjustments)
        {
	        if (LienWaiverSetup.Current.ShouldStopPayments == true &&
	            adjustments.Any(LienWaiverWarningsCacheService.ShouldShowWarning))
	        {
		        var warningMessage = GetWarningMessage();
		        WebDialogResult result = Base.APDocumentList.Ask(warningMessage, MessageButtons.OKCancel);

		        return result == WebDialogResult.OK;
	        }

	        return true;
		}

        private bool DoesAnyOutstandingLienWaiverExist(APAdjust adjustment)
        {
            var projectIds = LienWaiverProjectDataProvider.GetProjectIds(adjustment, Base).ToList();
            return lienWaiverDataProvider
                    .DoesAnyOutstandingComplianceExistForPrimaryVendor(adjustment.VendorID, projectIds)
                || DoesAnyOutstandingLienWaiverExistForJointPayees(adjustment, projectIds);
        }

        private bool DoesAnyOutstandingLienWaiverExistForJointPayees(APAdjust adjustment, List<int?> projectsIds)
        {
            var jointPayeePayments = JointPayeePaymentDataProvider.GetJointPayeePayments(Base, adjustment.AdjdRefNbr,
                adjustment.AdjdDocType, adjustment.AdjdLineNbr);
            var jointPayees = JointPayeeDataProvider.GetJointPayees(Base, jointPayeePayments, adjustment.AdjdLineNbr);
            return jointPayees.Any(jp => lienWaiverDataProvider
                .DoesAnyOutstandingComplianceExistForJointVendor(jp, projectsIds));
        }

        private string GetWarningMessage()
        {
            return AccountsPayableSetup.Current.HoldEntry == true
                ? ComplianceMessages.LienWaiver.BillHasOutstandingLienWaiverStopPayment
                : string.Concat(ComplianceMessages.LienWaiver.BillHasOutstandingLienWaiverStopPayment, Environment.NewLine,
                    Environment.NewLine, ComplianceMessages.LienWaiver.CheckWillBeAssignedOnHoldStatus);
        }
    }
}