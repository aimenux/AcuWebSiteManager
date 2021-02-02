using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common.Abstractions;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
	public class ApPaymentEntryVoidExtension : PaymentRevertBaseExtension
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public void VoidCheckProc(APPayment payment, Action<APPayment> baseHandler)
        {
            VoidAutomaticallyCreatedLienWaivers();
            baseHandler(payment);
            VoidJointPayeePayments(payment);
        }

        private void VoidJointPayeePayments(IDocumentKey check)
        {
            var checkAdjustments = GetAdjustments(check).ToList();
            var voidCheckAdjustments = GetAdjustments(Base.Document.Current);
            foreach (var voidCheckAdjustment in voidCheckAdjustments)
            {
                var checkAdjustment = checkAdjustments.Single(adj => IsSameAdjustment(adj, voidCheckAdjustment));
                AddRevertingJointPayeePayments(checkAdjustment, voidCheckAdjustment);
            }
        }

        private void VoidAutomaticallyCreatedLienWaivers()
        {
            var lienWaiverVoidService = new LienWaiverVoidService();
            var paymentEntryExtension = Base.GetExtension<PX.Objects.CN.Compliance.AP.GraphExtensions.ApPaymentEntryExt>();
            var complianceDocuments = paymentEntryExtension.ComplianceDocuments.SelectMain()
                .Where(cd => cd.IsCreatedAutomatically == true).ToList();
            if (complianceDocuments.Any() && lienWaiverVoidService.IsVoidOfAutomaticallyGeneratedLienWaiverConfirmed(
                Base, LienWaiverReferencedDocument.ApCheck))
            {
                lienWaiverVoidService.VoidAutomaticallyCreatedLienWaivers(
                    paymentEntryExtension.ComplianceDocuments.Cache, complianceDocuments);
                paymentEntryExtension.ComplianceDocuments.Cache.Persist(PXDBOperation.Update);
            }
        }

        private static bool IsSameAdjustment(APAdjust checkAdjustment, APAdjust voidCheckAdjustment)
        {
            return checkAdjustment.AdjdRefNbr == voidCheckAdjustment.AdjdRefNbr &&
                checkAdjustment.AdjdDocType == voidCheckAdjustment.AdjdDocType &&
                checkAdjustment.AdjdLineNbr == voidCheckAdjustment.AdjdLineNbr;
        }

        private IEnumerable<APAdjust> GetAdjustments(IDocumentKey payment)
        {
            return SelectFrom<APAdjust>
                .Where<APAdjust.adjgDocType.IsEqual<P.AsString>
                    .And<APAdjust.adjgRefNbr.IsEqual<P.AsString>>>.View
                .Select(Base, payment.DocType, payment.RefNbr).FirstTableItems;
        }
    }
}