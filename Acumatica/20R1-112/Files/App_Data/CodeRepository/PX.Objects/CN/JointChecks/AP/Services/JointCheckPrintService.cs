using System;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.Services
{
	internal class JointCheckPrintService
	{
		private readonly APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();

		public bool DoJointPayeePaymentsWithPositiveAmountExist(string documentType, string referenceNumber)
		{
			var query = new PXSelect<JointPayeePayment,
				Where<JointPayeePayment.paymentDocType, Equal<Required<JointPayeePayment.paymentDocType>>,
					And<JointPayeePayment.paymentRefNbr, Equal<Required<JointPayeePayment.paymentRefNbr>>,
					And<JointPayeePayment.jointAmountToPay, Greater<decimal0>>>>>(
				paymentEntry);
			return query.Select(documentType, referenceNumber).Any();
		}

		public string GetJointPayeesSingleLine(string documentType, string referenceNumber)
		{
			return GetJointPayees(documentType, referenceNumber, false);
		}

		public string GetJointPayeesMultiline(string documentType, string referenceNumber)
		{
			return GetJointPayees(documentType, referenceNumber, true);
		}

		private string GetJointPayees(string documentType, string referenceNumber, bool isMultiline)
		{
			var jointPayeePayments = GetJointPayeePayments(documentType, referenceNumber);
			var printModel = JointCheckPrintModel.Create(isMultiline);
			foreach (var jointPayeePayment in jointPayeePayments)
			{
				ProcessJointPayeePayment(printModel, jointPayeePayment);
			}
			return printModel.JointPayeeNames;
		}

		private static void ProcessJointPayeePayment(JointCheckPrintModel printModel, PXResult jointPayeePayment)
		{
			var jointPayee = jointPayeePayment.GetItem<JointPayee>();
			var vendor = jointPayeePayment.GetItem<Vendor>();
			var jointPayeeName = GetJointPayeeNameIfNew(printModel, jointPayee, vendor);
			if (jointPayeeName == string.Empty)
			{
				return;
			}
			UpdateJointCheckPrintModelWithNewPayee(printModel, jointPayee, vendor);
			AddJointPayeeName(printModel, jointPayeeName);
		}

		private static void UpdateJointCheckPrintModelWithNewPayee(JointCheckPrintModel printModel,
			JointPayee jointPayee, BAccount vendor)
		{
			if (jointPayee.JointPayeeExternalName.IsNullOrEmpty())
			{
				printModel.InternalJointPayeeIds.Add(vendor.BAccountID);
			}
			else
			{
				printModel.ExternalJointPayeeNames.Add(jointPayee.JointPayeeExternalName);
			}
		}

		private static string GetJointPayeeNameIfNew(JointCheckPrintModel printModel, JointPayee jointPayee,
			BAccount vendor)
		{
			if (jointPayee.JointPayeeExternalName.IsNullOrEmpty())
			{
				return printModel.InternalJointPayeeIds.Any(id => id == vendor.BAccountID)
					? string.Empty
					: vendor.AcctName;
			}
			return printModel.ExternalJointPayeeNames.Any(name => IsSameJointPayeeExternalName(name, jointPayee))
				? string.Empty
				: jointPayee.JointPayeeExternalName;
		}

		private static void AddJointPayeeName(JointCheckPrintModel printModel, string jointPayeeName)
		{
			var updatedNames = $"{printModel.JointPayeeNames}{jointPayeeName} And ";
			printModel.JointPayeeNames = printModel.IsMultilinePrintMode
				? $"{updatedNames}{Environment.NewLine}"
				: updatedNames;
		}

		private static bool IsSameJointPayeeExternalName(string name, JointPayee jointPayee)
		{
			return string.Equals(name.Trim(),
				jointPayee.JointPayeeExternalName.Trim(),
				StringComparison.CurrentCultureIgnoreCase);
		}

		private PXResultset<JointPayeePayment> GetJointPayeePayments(string documentType, string referenceNumber)
		{
			var query = new PXSelectJoin<JointPayeePayment,
				InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>,
				LeftJoin<Vendor, On<Vendor.bAccountID, Equal<JointPayee.jointPayeeInternalId>>>>,
				Where<JointPayeePayment.paymentDocType, Equal<Required<JointPayeePayment.paymentDocType>>,
					And<JointPayeePayment.paymentRefNbr, Equal<Required<JointPayeePayment.paymentRefNbr>>,
					And<JointPayeePayment.jointAmountToPay, Greater<decimal0>>>>>(paymentEntry);
			return query.Select(documentType, referenceNumber);
		}
	}
}