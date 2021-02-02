using PX.Data;
using PX.Objects.CA;
using System.Collections.Generic;
namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public interface ICCPaymentProcessingRepository
	{
		PXGraph Graph { get; }
		CCProcTran GetCCProcTran(int? tranID);
		CCProcTran InsertCCProcTran(CCProcTran transaction);
		CCProcTran UpdateCCProcTran(CCProcTran transaction);
		CCProcTran InsertOrUpdateTransaction(CCProcTran procTran);
		CCProcTran InsertTransaction(CCProcTran procTran);
		CCProcTran UpdateTransaction(CCProcTran procTran);
		IEnumerable<CCProcTran> GetCCProcTranByTranID(int? transactionId);
		ExternalTransaction GetExternalTransaction(int? tranId);
		CustomerPaymentMethod GetCustomerPaymentMethod(int? pMInstanceId);
		CustomerPaymentMethod UpdateCustomerPaymentMethod(CustomerPaymentMethod paymentMethod);
		CustomerPaymentMethodDetail GetCustomerPaymentMethodDetail(int? pMInstanceId, string detailID);
		void DeletePaymentMethodDetail(CustomerPaymentMethodDetail detail);
		CCProcTran FindReferencedCCProcTran(int? pMInstanceID, string refTranNbr);
		CCProcTran FindVerifyingCCProcTran(int? pMInstanceID);
		CashAccount GetCashAccount(int? cashAccountId);
		CCProcessingCenter GetCCProcessingCenter(string processingCenterID);
		CCProcessingCenter FindProcessingCenter(int? pMInstanceID, string aCuryId);
		PXResultset<CCProcessingCenterDetail> FindAllProcessingCenterDetails(string processingCenterID);
		PXResult<CustomerPaymentMethod, Customer> FindCustomerAndPaymentMethod(int? pMInstanceID);	

		void Save();
	}
}