using PX.CCProcessingBase;
using System.Collections.Generic;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR;
namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public interface ICardProcessingReadersProvider
	{
		ICreditCardDataReader GetCardDataReader();
		IEnumerable<ICreditCardDataReader> GetCustomerCardsDataReaders();
		ICustomerDataReader GetCustomerDataReader();
		IDocDetailsDataReader GetDocDetailsDataReader();
		IProcessingCenterSettingsStorage GetProcessingCenterSettingsStorage();
		String2DateConverterFunc GetExpirationDateConverter();
	}
}
