using PX.Objects.AR.CCPaymentProcessing;
using System.Collections.Generic;
using PX.CCProcessingBase.Interfaces.V2;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public interface ICardTransactionProcessingWrapper
	{
		CCProcessingBase.ProcessingResult DoTransaction(CCProcessingBase.CCTranType aTranType, CCProcessingBase.ProcessingInput inputData);
		CCProcessingBase.CCErrors ValidateSettings(CCProcessingBase.ISettingsDetail setting);
		TransactionData GetTransaction(string transactionId);
		IEnumerable<TransactionData> GetTransactionsByCustomer(string customerProfileId, TransactionSearchParams searchParams = null);
		IEnumerable<TransactionData> GetUnsettledTransactions(TransactionSearchParams searchParams = null);
		void TestCredentials(CCProcessingBase.APIResponse apiResponse);
		void ExportSettings(IList<CCProcessingBase.ISettingsDetail> aSettings);
	}
}
