using System.Collections.Generic;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.AR.CCPaymentProcessing.Common;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public interface ICardTransactionProcessingWrapper
	{
		TranProcessingResult DoTransaction(Common.CCTranType aTranType, TranProcessingInput inputData);
		CCError ValidateSettings(PluginSettingDetail setting);
		TransactionData GetTransaction(string transactionId);
		IEnumerable<TransactionData> GetTransactionsByCustomer(string customerProfileId, TransactionSearchParams searchParams = null);
		IEnumerable<TransactionData> GetUnsettledTransactions(TransactionSearchParams searchParams = null);
		void TestCredentials(APIResponse apiResponse);
		void ExportSettings(IList<PluginSettingDetail> aSettings);
	}
}
