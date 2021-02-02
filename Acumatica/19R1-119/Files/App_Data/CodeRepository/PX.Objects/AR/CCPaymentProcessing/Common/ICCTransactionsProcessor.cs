using PX.CCProcessingBase;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public interface ICCTransactionsProcessor
	{
		void ProcessCCTransaction(ICCPayment aDoc, CCProcTran refTran, CCTranType aTranType);
		void ProcessCCTransaction(ICCPayment aDoc, ICCPaymentTransaction refTran, CCTranType aTranType);
	}
}