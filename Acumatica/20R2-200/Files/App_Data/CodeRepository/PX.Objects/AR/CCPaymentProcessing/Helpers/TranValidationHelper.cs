using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects;
using PX.Data;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.Common;
using PX.Objects.AR;
namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	public static class TranValidationHelper
	{
		public class AdditionalParams
		{
			public int? PMInstanceId { get; set; }
			public int? CustomerID { get; set; }
			public string ProcessingCenter { get; set; }
			public ICCPaymentProcessingRepository Repo { get; set; }
		}

		public static void CheckRecordedTranStatus(TransactionData tranData)
		{
			if (tranData.TranStatus == CCTranStatus.Declined)
			{
				throw new PXException(Messages.ERR_IncorrectDeclinedTranStatus, tranData.TranID);
			}

			if (tranData.TranStatus == CCTranStatus.Expired)
			{
				throw new PXException(Messages.ERR_IncorrectExpiredTranStatus, tranData.TranID);
			}

			if (tranData.TranStatus == CCTranStatus.Error || tranData.TranStatus == CCTranStatus.Unknown)
			{
				throw new PXException(Messages.ERR_IncorrectTranStatus, tranData.TranID);
			}
			
		}

		public static void CheckCustomer(TransactionData tranData, int? customer, CustomerPaymentMethod cpm)
		{
			if (cpm != null && customer != null && cpm.BAccountID != customer)
			{
				throw new PXException(Messages.ERR_TranIsUsedForAnotherCustomer, tranData.TranID);
			}
		}

		public static void CheckPmInstance(TransactionData tranData, Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> cpmData)
		{
			if (tranData != null && cpmData != null)
			{
				CustomerPaymentMethod cpm = cpmData.Item1;
				CustomerPaymentMethodDetail cpmd = cpmData.Item2;
				if (tranData.CustomerId != cpm.CustomerCCPID || tranData.PaymentId != cpmd.Value)
				{
					throw new PXException(Messages.ERR_IncorrectPmInstanceId, tranData.TranID);
				}
			}
		}

		public static void CheckPaymentProfile(TransactionData tranData, AdditionalParams prms)
		{
			CustomerPaymentMethod cpm = null;
			Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> res = null;
			ICCPaymentProcessingRepository repo = prms.Repo;
			if (prms.PMInstanceId > 0)
			{
				res = repo.GetCustomerPaymentMethodWithProfileDetail(prms.PMInstanceId);
				if (res == null)
				{
					throw new PXException(AR.Messages.CreditCardWithID_0_IsNotDefined, prms.PMInstanceId);
				}
				else
				{
					CheckPmInstance(tranData, res);
				}
			}
			else
			{
				res = repo.GetCustomerPaymentMethodWithProfileDetail(prms.ProcessingCenter, tranData.CustomerId, tranData.PaymentId);
				if (res != null)
				{
					cpm = res.Item1;
					CheckCustomer(tranData, prms.CustomerID, cpm);
				}
			}
		}

		public static void CheckTranAlreadyRecorded(TransactionData tranData, AdditionalParams inputParams)
		{
			var query = new PXSelect<CCProcTran,
				Where<CCProcTran.pCTranNumber, Equal<Required<CCProcTran.pCTranNumber>>,
					And<CCProcTran.processingCenterID, Equal<Required<CCProcTran.processingCenterID>>>>,
				OrderBy<Desc<CCProcTran.tranNbr>>>(inputParams.Repo.Graph);
			CCProcTran storedTran = query.SelectSingle(tranData.TranID, inputParams.ProcessingCenter);
			if (storedTran != null)
			{
				throw new PXException(GenerateTranAlreadyRecordedErrMsg(tranData.TranID, storedTran, inputParams));
			}
		}

		public static string GenerateTranAlreadyRecordedErrMsg(string tranId, CCProcTran procTran, AdditionalParams inputParams)
		{
			string ret = null;
			string docName = PXMessages.LocalizeNoPrefix(Messages.Document);
			string doc = null;
			
			if (procTran.DocType != null)
			{
				doc = procTran.DocType + procTran.RefNbr;
				ARDocType arDocType = new ARDocType();
				var res = arDocType.ValueLabelPairs.FirstOrDefault(i=>i.Value == procTran.DocType);
				if (res.Label != null)
				{
					docName = PXMessages.LocalizeNoPrefix(res.Label).ToLower();
				}
			}
			else
			{
				doc = procTran.OrigDocType + procTran.OrigRefNbr;
			}

			if (inputParams.PMInstanceId > 0)
			{
				var repo = inputParams.Repo;
				CustomerPaymentMethod cpm = inputParams.Repo.GetCustomerPaymentMethod(inputParams.PMInstanceId);
				ret = PXMessages.LocalizeFormatNoPrefix(Messages.ERR_TransactionWithCpmIsRecordedForOtherDoc, tranId, cpm.Descr, doc, docName);
			}
			else
			{
				ret = PXMessages.LocalizeFormatNoPrefix(Messages.ERR_TransactionIsRecordedForOtherDoc, tranId, inputParams.ProcessingCenter, doc, docName);
			}
			return ret;
		}
	}
}
