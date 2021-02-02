using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Repositories;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public abstract class V1ProcessorBase
	{
		protected CCProcessingBase.ICCPaymentProcessing _plugin;
		protected Repositories.ICardProcessingReadersProvider _provider;

		public V1ProcessorBase(CCProcessingBase.ICCPaymentProcessing v1Plugin)
		{
			_plugin = v1Plugin;
		}

		public V1ProcessorBase(CCProcessingBase.ICCPaymentProcessing v1Plugin, Repositories.ICardProcessingReadersProvider provider) : this(v1Plugin)
		{
			_provider = provider;
		}

		protected void ProcessAPIResponse(CCProcessingBase.APIResponse apiResponse)
		{
			if (!apiResponse.isSucess && apiResponse.ErrorSource != CCProcessingBase.CCErrors.CCErrorSource.None)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> kvp in apiResponse.Messages)
				{
					stringBuilder.Append(kvp.Key);
					stringBuilder.Append(": ");
					stringBuilder.Append(kvp.Value);
					stringBuilder.Append(". ");
				}
				throw new PXException(Messages.CardProcessingError, CCProcessingBase.CCErrors.GetDescription(apiResponse.ErrorSource), stringBuilder.ToString());
			}
		}

		protected string GetExpirationDateFormat()
		{
			if (_plugin is CCProcessingBase.ICCProcessingExpDateFormatting)
			{
				return ((CCProcessingBase.ICCProcessingExpDateFormatting) _plugin).GetExpirationDateFormat();
			}
			return null;
		}

		protected List<CCProcessingBase.Interfaces.V2.CreditCardData> GetCardData(CCProcessingBase.SyncPMResponse syncResponse)
		{
			if (syncResponse == null) throw new ArgumentNullException(nameof(syncResponse));

			CCProcessingBase.ICreditCardDataReader cardDataReader = _provider.GetCardDataReader();
			List<CCProcessingBase.Interfaces.V2.CreditCardData> cardDataList = new List<CCProcessingBase.Interfaces.V2.CreditCardData>();

			foreach (var cardInfo in syncResponse.PMList)
			{
				CCProcessingBase.Interfaces.V2.CreditCardData cardData = new CCProcessingBase.Interfaces.V2.CreditCardData()
				{
					PaymentProfileID = cardInfo.Key
				};
				string cardNumber;
				if (cardInfo.Value.TryGetValue(cardDataReader.Key_CardNumber, out cardNumber))
				{
					cardData.CardNumber = cardNumber;
				}
				string expirationDate;
				if (cardInfo.Value.TryGetValue(cardDataReader.Key_CardExpiryDate, out expirationDate))
				{
					string dateTimeFormat = GetExpirationDateFormat();
					if (!string.IsNullOrEmpty(dateTimeFormat))
					{
						DateTime expirationDateTime;
						if (DateTime.TryParseExact(expirationDate, 
							dateTimeFormat, 
							null, 
							System.Globalization.DateTimeStyles.None, 
							out expirationDateTime))
						{
							cardData.CardExpirationDate = expirationDateTime;
						}
					}
				}

				cardDataList.Add(cardData);
			}
			return cardDataList;
		}

		protected ICardProcessingReadersProvider GetProvider()
		{
			if(_provider == null)
			{
				throw new PXInvalidOperationException(NotLocalizableMessages.ERR_CardProcessingReadersProviderGetting);
			}
			return _provider;
		}
	}
}