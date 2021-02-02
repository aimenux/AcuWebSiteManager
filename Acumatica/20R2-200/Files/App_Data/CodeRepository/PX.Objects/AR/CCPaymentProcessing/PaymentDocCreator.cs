using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.Extensions.PaymentTransaction;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.Common.Abstractions;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class PaymentDocCreator
	{
		public class InputParams
		{
			public string TransactionID { get; set; }
			public int? Customer { get; set; }
			public int? CashAccountID { get; set; }
			public int? PMInstanceID { get; set; }
			public string PaymentMethodID { get; set; }
			public string ProcessingCenterID { get; set; }
		}

		protected ICCPaymentProcessingRepository repo;
		protected CCPaymentProcessing paymentProc;

		public PaymentDocCreator()
		{
			PXGraph graph = PXGraph.CreateInstance<CCPaymentHelperGraph>();
			repo = new CCPaymentProcessingRepository(graph);
			paymentProc = new CCPaymentProcessing(repo);
		}

		public PaymentDocCreator(PXGraph graph)
		{
			repo = new CCPaymentProcessingRepository(graph);
			paymentProc = new CCPaymentProcessing(repo);
		}

		public virtual IDocumentKey CreateDoc(InputParams inputParams)
		{
			CheckInput(inputParams);
			TransactionData tranData = paymentProc.GetTransactionById(inputParams.TransactionID, inputParams.ProcessingCenterID);
			ExecValidations(tranData, inputParams);
			IDocumentKey ret = SaveDocWithTransaction(tranData, inputParams);
			return ret;
		}

		protected virtual void CheckInput(InputParams inputParams)
		{
			if (string.IsNullOrEmpty(inputParams.TransactionID))
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, nameof(inputParams.TransactionID));
			}
			if (inputParams.Customer == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, nameof(inputParams.Customer));
			}
			if (string.IsNullOrEmpty(inputParams.PaymentMethodID))
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, nameof(inputParams.PaymentMethodID));
			}
			if (string.IsNullOrEmpty(inputParams.ProcessingCenterID))
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, nameof(inputParams.ProcessingCenterID));
			}
			if (inputParams.CashAccountID == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, nameof(inputParams.CashAccountID));
			}
		}

		protected virtual void ExecValidations(TransactionData tranData, InputParams inputParams)
		{
			TranValidationHelper.CheckRecordedTranStatus(tranData);
			if (tranData.TranType == CCTranType.Void)
			{
				throw new PXException(Messages.ERR_IncorrectVoidTranType, tranData.TranID);
			}
			if (tranData.TranType == CCTranType.Credit)
			{
				throw new PXException(Messages.ERR_IncorrectRefundTranType, tranData.TranID);
			}

			var prms = new TranValidationHelper.AdditionalParams();
			prms.PMInstanceId = inputParams.PMInstanceID;
			prms.ProcessingCenter = inputParams.ProcessingCenterID;
			prms.CustomerID = inputParams.Customer;
			prms.Repo = repo;
			TranValidationHelper.CheckPaymentProfile(tranData, prms);
			TranValidationHelper.CheckTranAlreadyRecorded(tranData, prms);
		}

		protected virtual IDocumentKey SaveDocWithTransaction(TransactionData tranData, InputParams inputParams)
		{
			ARPaymentEntry paymentGraph = PXGraph.CreateInstance<ARPaymentEntry>();
			ARPayment payment = new ARPayment();
			payment.DocType = ARDocType.Payment;
			payment = paymentGraph.Document.Insert(payment);

			payment.CustomerID = inputParams.Customer;
			payment.PaymentMethodID = inputParams.PaymentMethodID;
			payment.CashAccountID = inputParams.CashAccountID;
			payment.CuryOrigDocAmt = tranData.Amount;
			payment = paymentGraph.Document.Update(payment);
			var extension = paymentGraph.GetExtension<ARPaymentEntry.PaymentTransaction>();
			if (inputParams.PMInstanceID > 0)
			{
				payment.PMInstanceID = inputParams.PMInstanceID;
			}
			else
			{
				payment.PMInstanceID = PaymentTranExtConstants.NewPaymentProfile;
				payment.ProcessingCenterID = inputParams.ProcessingCenterID;
			}
			payment = paymentGraph.Document.Update(payment);
			using (var txscope = new PXTransactionScope())
			{
				paymentGraph.Save.Press();
				CCPaymentEntry entry = new CCPaymentEntry(paymentGraph);
				extension.CheckSaveCardOption(tranData);
				extension.RecordTransaction(payment, tranData, entry);

				txscope.Complete();
			}
			return payment;
		}
	}
}
