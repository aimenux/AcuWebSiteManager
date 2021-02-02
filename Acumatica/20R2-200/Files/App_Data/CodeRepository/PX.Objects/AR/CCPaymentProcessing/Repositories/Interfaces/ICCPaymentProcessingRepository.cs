using System;
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
		CCProcTran InsertOrUpdateTransaction(CCProcTran procTran, ExternalTransaction extTran);
		CCProcTran InsertTransaction(CCProcTran procTran, ExternalTransaction extTran);
		CCProcTran UpdateTransaction(CCProcTran procTran, ExternalTransaction extTran);
		ExternalTransaction UpdateExternalTransaction(ExternalTransaction extTran);
		IEnumerable<CCProcTran> GetCCProcTranByTranID(int? transactionId);
		ExternalTransaction GetExternalTransaction(int? tranId);
		ExternalTransaction FindCapturedExternalTransaction(int? pMInstanceID, string refTranNbr);
		ExternalTransaction FindCapturedExternalTransaction(string procCenterId, string tranNbr);
		CustomerPaymentMethod GetCustomerPaymentMethod(int? pMInstanceId);
		Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(string procCenter, string custProfileId, string paymentProfileId);
		Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(int? pmInstanceId);
		CustomerPaymentMethod UpdateCustomerPaymentMethod(CustomerPaymentMethod paymentMethod);
		CustomerPaymentMethodDetail GetCustomerPaymentMethodDetail(int? pMInstanceId, string detailID);
		void DeletePaymentMethodDetail(CustomerPaymentMethodDetail detail);
		CCProcTran FindVerifyingCCProcTran(int? pMInstanceID);
		CashAccount GetCashAccount(int? cashAccountId);
		CCProcessingCenter GetCCProcessingCenter(string processingCenterID);
		CCProcessingCenter FindProcessingCenter(int? pMInstanceID, string aCuryId);
		PXResultset<CCProcessingCenterDetail> FindAllProcessingCenterDetails(string processingCenterID);
		PXResult<CustomerPaymentMethod, Customer> FindCustomerAndPaymentMethod(int? pMInstanceID);	

		void Save();
	}
}