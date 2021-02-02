using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Factories;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.CA;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCCustomerInformationManager
	{
		private ICardProcessingReadersProvider _readersProvider;

		private IBaseProfileProcessingWrapper _profileProcessingWrapper => BaseProfileProcessingWrapper.GetBaseProfileProcessingWrapper(_pluginObject, _readersProvider);

		private IHostedFromProcessingWrapper _hfProcessor => HostedFromProcessingWrapper.GetHostedFormProcessingWrapper(_pluginObject, _readersProvider);

		private IExtendedProfileProcessingWrapper _extendedProfileProcessor => ExtendedProfileProcessingWrapper.GetExtendedProfileProcessingWrapper(_pluginObject, _readersProvider);

		private object _pluginObject;

		protected CCCustomerInformationManager(ProcessingCardsPluginFactory pluginFactory)
		{
			_pluginObject = pluginFactory.GetPlugin();
		}

		protected virtual string CreateCustomerProfile()
		{
			return _profileProcessingWrapper.CreateCustomerProfile();
		}

		protected virtual string CreatePaymentProfile()
		{
			return _profileProcessingWrapper.CreatePaymentProfile();
		}

		protected virtual CreditCardData GetPaymentProfile()
		{
			return _profileProcessingWrapper.GetPaymentProfile();
		}

		protected virtual void DeletePaymentProfile()
		{
			_profileProcessingWrapper.DeletePaymentProfile();
		}

		protected virtual IEnumerable<CreditCardData> GetAllPaymentProfiles()
		{
			return _extendedProfileProcessor.GetAllPaymentProfiles();
		}

		protected virtual void GetCreatePaymentProfileForm()
		{
			_hfProcessor.GetCreateForm();
		}

		protected virtual IEnumerable<CreditCardData> GetMissingPaymentProfiles()
		{
			return _hfProcessor.GetMissingPaymentProfiles();
		}

		protected virtual void GetManagePaymentProfileForm()
		{
			_hfProcessor.GetManageForm();
		}

		protected virtual TranProfile GetOrCreateCustomerProfileFromTransaction(string tranId, CreateTranPaymentProfileParams cParams)
		{
			TranProfile ret = _extendedProfileProcessor.GetOrCreatePaymentProfileFromTransaction(tranId, cParams);
			return ret;
		}

		protected void SetReadersProvider(ICardProcessingReadersProvider readerProvider)
		{
			_readersProvider = readerProvider;
		}

		public static void GetCreatePaymentProfileForm(PXGraph graph, ICCPaymentProfileAdapter paymentProfileAdapter)
		{
			if (graph == null || paymentProfileAdapter == null)
				return;
			ICCPaymentProfile ccPaymentProfile = paymentProfileAdapter.Current;
			PXCache cache = paymentProfileAdapter.Cache;
			ProcessingCardsPluginFactory pluginFactory = GetProcessingCardsPluginFactory(ccPaymentProfile.CCProcessingCenterID);
			CCCustomerInformationManager cim = GetCustomerInformationManager(pluginFactory);
			CCProcessingContext context = new CCProcessingContext()
			{
				processingCenter = pluginFactory.GetProcessingCenter(),
				aCustomerID = ccPaymentProfile.BAccountID,
				aPMInstanceID = ccPaymentProfile.PMInstanceID,
				callerGraph = graph
			};
			CardProcessingReadersProvider readersProvider = new CardProcessingReadersProvider(context);
			cim.SetReadersProvider(readersProvider);
			string id = ccPaymentProfile.CustomerCCPID;
			if (id == null)
			{
				id = cim.CreateCustomerProfile();
				ICCPaymentProfile cpm = cache.CreateCopy(ccPaymentProfile) as ICCPaymentProfile;
				cpm.CustomerCCPID = id;
				cache.Update(cpm);
			}
			var processingCenter = pluginFactory.GetProcessingCenter();
			if (processingCenter.CreateAdditionalCustomerProfiles == true)
			{
				int customerProfileCount = CCProcessingHelper.CustomerProfileCountPerCustomer(graph,
					ccPaymentProfile.BAccountID,
					ccPaymentProfile.CCProcessingCenterID); // Total customer profile count per customer

				var cardLimit = processingCenter.CreditCardLimit;
				if (cardLimit != null && cardLimit > 0)
				{
					int allPaymentProfileCount = cim.GetAllPaymentProfiles().Count();
					if (CCProcessingHelper.IsCreditCardCountEnough(allPaymentProfileCount, cardLimit.Value))
					{
						context.PrefixForCustomerCD = CCProcessingHelper.BuildPrefixForCustomerCD(customerProfileCount, processingCenter);
						id = cim.CreateCustomerProfile();
						ICCPaymentProfile cpm = cache.CreateCopy(ccPaymentProfile) as ICCPaymentProfile;
						cpm.CustomerCCPID = id;
						cache.Update(cpm);
					}
				}
			}
			cim.GetCreatePaymentProfileForm();
		}

		public static PXResultset<CustomerPaymentMethodDetail> GetAllCustomersCardsInProcCenter(PXGraph graph, int? BAccountID, string CCProcessingCenterID)
		{
			return PXSelectJoin<CustomerPaymentMethodDetail,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
						And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>>>,
					InnerJoin<CustomerPaymentMethod,
						On<CustomerPaymentMethodDetail.pMInstanceID, Equal<CustomerPaymentMethod.pMInstanceID>>>>,
				Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
					And<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
						And<PaymentMethodDetail.isCCProcessingID, Equal<True>>>>>.Select(graph, BAccountID, CCProcessingCenterID);
		}

		public static void GetNewPaymentProfiles(PXGraph graph,
			ICCPaymentProfileAdapter payment,
			ICCPaymentProfileDetailAdapter paymentDetail
			)
		{
			if (graph == null || payment == null || paymentDetail == null)
				return;
			ICCPaymentProfile currentPaymentProfile = payment.Current;
			ProcessingCardsPluginFactory pluginFactory = GetProcessingCardsPluginFactory(currentPaymentProfile.CCProcessingCenterID);
			CCCustomerInformationManager cim = GetCustomerInformationManager(pluginFactory);
			CardProcessingReadersProvider readersProvider = new CardProcessingReadersProvider(new CCProcessingContext()
			{
				processingCenter = pluginFactory.GetProcessingCenter(),
				aCustomerID = currentPaymentProfile.BAccountID,
				aPMInstanceID = currentPaymentProfile.PMInstanceID,
				callerGraph = graph
			});
			cim.SetReadersProvider(readersProvider);

			int attempt = 1;
			CreditCardData newCard = null;
			//AuthorizeNet sometimes failes to process new card in time when using Hosted Form Method
			CCProcessingCenter procCenter = pluginFactory.GetProcessingCenter();
			while ((attempt <= (procCenter.SyncRetryAttemptsNo ?? 0) + 1) && newCard == null)
			{
				Thread.Sleep(procCenter.SyncRetryDelayMs ?? 0);
				List<CreditCardData> newCards = null;

				try
				{
					newCards = cim.GetMissingPaymentProfiles().ToList();
				}
				catch (Exception e)
				{
					throw new PXException(e.Message + ". " + Messages.FailedToSyncCC);
				}

				ICCPaymentProfile customerPaymentMethod = payment.Current;

				if (newCards != null && newCards.Count > 1)
				{
					newCards.Sort(new InterfaceExtensions.CreditCardDataComparer());
					newCard = newCards[0];
				}
				else if (newCards != null && newCards.Count == 1)
				{
					newCard = newCards[0];
				}

				if (newCard != null)
				{
					foreach (Tuple<ICCPaymentProfileDetail, ICCPaymentMethodDetail> det in paymentDetail.Select())
					{
						ICCPaymentProfileDetail cpmd = det.Item1;
						ICCPaymentMethodDetail pmd = det.Item2;
						if (pmd.IsCCProcessingID == true)
						{
							cpmd.Value = newCard.PaymentProfileID;
							paymentDetail.Cache.Update(cpmd);
						}
						else if (pmd.IsIdentifier == true)
						{
							cpmd.Value = newCard.CardNumber;
							paymentDetail.Cache.Update(cpmd);
						}
					}
					//getting unmasked expiration date
					newCard = cim.GetPaymentProfile();
					if (newCard.CardExpirationDate != null)
					{
						payment.Cache.SetValueExt<CustomerPaymentMethod.expirationDate>(customerPaymentMethod, newCard.CardExpirationDate);
						payment.Cache.Update(customerPaymentMethod);
					}
				}
				attempt++;
			}
			if (newCard == null)
			{
				throw new PXException(Messages.FailedToSyncCC);
			}
		}

		public static void GetManagePaymentProfileForm(PXGraph graph, ICCPaymentProfile paymentProfile)
		{
			if (graph == null || paymentProfile == null)
			{
				return;
			}
			CustomerPaymentMethodDetail ccpID = PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail,
					On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>, And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
						And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				Where<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<CustomerPaymentMethodDetail.pMInstanceID,
					Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>>.SelectWindowed(graph, 0, 1, paymentProfile.PMInstanceID);
			if (ccpID != null && !string.IsNullOrEmpty(ccpID.Value))
			{
				ProcessingCardsPluginFactory pluginFactory = GetProcessingCardsPluginFactory(paymentProfile.CCProcessingCenterID);
				CCCustomerInformationManager cim = GetCustomerInformationManager(pluginFactory);
				CCProcessingCenter processingCenter = pluginFactory.GetProcessingCenter();
				CCProcessingContext context = new CCProcessingContext()
				{
					processingCenter = pluginFactory.GetProcessingCenter(),
					aCustomerID = paymentProfile.BAccountID,
					aPMInstanceID = paymentProfile.PMInstanceID,
					callerGraph = graph
				};
				CardProcessingReadersProvider readersProvider = new CardProcessingReadersProvider(context);
				cim.SetReadersProvider(readersProvider);
				cim.GetManagePaymentProfileForm();
			}
		}

		public static void GetOrCreatePaymentProfile(PXGraph graph
				, ICCPaymentProfileAdapter payment
				, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			ICCPaymentProfile paymentProfile = payment.Current;
			bool isHF = CCProcessingHelper.IsHFPaymentMethod(graph, payment.Current.PMInstanceID);
			bool isConverting = false;
			if (paymentProfile is CustomerPaymentMethod)
			{
				isConverting = ((CustomerPaymentMethod)paymentProfile).Selected == true;
			}
			isHF = isHF && !isConverting;
			ICCPaymentProfileDetail CCPIDDet = null;
			bool isIDFilled = false;
			bool isOtherDetsFilled = false;
			foreach (Tuple<ICCPaymentProfileDetail, ICCPaymentMethodDetail> det in paymentDetail.Select())
			{
				ICCPaymentProfileDetail ppd = det.Item1;
				ICCPaymentMethodDetail pmd = det.Item2;
				if (pmd.IsCCProcessingID == true)
				{
					isIDFilled = ppd.Value != null;
					CCPIDDet = ppd;
				}
				else
				{
					isOtherDetsFilled = ppd.Value != null || isOtherDetsFilled;
				}
			}
			if (CCPIDDet == null)
			{
				//something's very wrong
				throw new PXException(Messages.NOCCPID, payment.Current.Descr);
			}
			if (isIDFilled && isOtherDetsFilled)
			{
				return;
			}

			bool tryGetProfile = isIDFilled && !isOtherDetsFilled;
			if ((isIDFilled || isOtherDetsFilled) && !isHF || tryGetProfile)
			{
				var currCpm = payment.Current;
				ProcessingCardsPluginFactory pluginFactory = GetProcessingCardsPluginFactory(currCpm.CCProcessingCenterID);
				CCCustomerInformationManager cim = GetCustomerInformationManager(pluginFactory);
				CCProcessingCenter processingCenter = pluginFactory.GetProcessingCenter();
				CCProcessingContext context = new CCProcessingContext()
				{
					processingCenter = pluginFactory.GetProcessingCenter(),
					aCustomerID = currCpm.BAccountID,
					aPMInstanceID = currCpm.PMInstanceID,
					callerGraph = graph,
					expirationDateConverter = s => CustomerPaymentMethodMaint.ParseExpiryDate(graph, currCpm, s)
				};
				CardProcessingReadersProvider readersProvider = new CardProcessingReadersProvider(context);
				cim.SetReadersProvider(readersProvider);
				string id = currCpm.CustomerCCPID;
				if (currCpm.CustomerCCPID == null)
				{
					id = cim.CreateCustomerProfile();
					paymentProfile.CustomerCCPID = id;
					payment.Cache.Update(paymentProfile);
				}

				if (processingCenter.CreateAdditionalCustomerProfiles == true && !tryGetProfile)
				{
					int customerProfileCount = CCProcessingHelper.CustomerProfileCountPerCustomer(graph,
						currCpm.BAccountID,
						currCpm.CCProcessingCenterID); // Total customer profile count per customer

					var cardLimit = processingCenter.CreditCardLimit;
					if (cardLimit != null && cardLimit > 0)
					{
						int allPaymentProfileCount = cim.GetAllPaymentProfiles().Count();
						if (CCProcessingHelper.IsCreditCardCountEnough(allPaymentProfileCount, cardLimit.Value))
						{
							context.PrefixForCustomerCD = CCProcessingHelper.BuildPrefixForCustomerCD(customerProfileCount, processingCenter);
							id = cim.CreateCustomerProfile();
							paymentProfile.CustomerCCPID = id;
							payment.Cache.Update(paymentProfile);
						}
					}
				}

				if (isOtherDetsFilled)
				{
					string newPMId = cim.CreatePaymentProfile();
					CCPIDDet.Value = newPMId;
					CCPIDDet = paymentDetail.Cache.Update(CCPIDDet) as ICCPaymentProfileDetail;
				}
				CreditCardData cardData = cim.GetPaymentProfile();
				if (cardData != null && !string.IsNullOrEmpty(cardData.PaymentProfileID))
				{
					foreach (Tuple<ICCPaymentProfileDetail, ICCPaymentMethodDetail> det in paymentDetail.Select())
					{
						ICCPaymentProfileDetail ppd = det.Item1;
						ICCPaymentMethodDetail pmd = det.Item2;
						if (ppd.DetailID == CCPIDDet.DetailID)
							continue;
						string detailValue = null;
						if (pmd.IsCCProcessingID != true && pmd.IsIdentifier == true && !string.IsNullOrEmpty(cardData.CardNumber))
						{
							detailValue = cardData.CardNumber;
						}
						ppd.Value = detailValue;
						paymentDetail.Cache.Update(ppd);
					}
					if (cardData.CardExpirationDate != null)
					{
						payment.Cache.SetValueExt(paymentProfile, nameof(paymentProfile.ExpirationDate), cardData.CardExpirationDate);
						payment.Cache.Update(paymentProfile);
					}
				}
				else
				{
					throw new PXException(Messages.CouldntGetPMIDetails, payment.Current.Descr);
				}
			}
		}

		public static void GetPaymentProfile(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			string CCPID = null;
			foreach (Tuple<ICCPaymentProfileDetail, ICCPaymentMethodDetail> det in paymentDetail.Select())
			{
				ICCPaymentProfileDetail ppd = det.Item1;
				ICCPaymentMethodDetail pmd = det.Item2;
				if (pmd.IsCCProcessingID == true)
				{
					CCPID = ppd.Value;
					break;
				}
			}
			if (string.IsNullOrEmpty(CCPID))
			{
				throw new PXException(Messages.CreditCardTokenIDNotFound);
			}
			ICCPaymentProfile paymentProfile = payment.Current;
			string custProcCenterID = paymentProfile.CCProcessingCenterID;
			int? bAccountId = paymentProfile.BAccountID;
			int? pmInstanceId = paymentProfile.PMInstanceID;
			ProcessingCardsPluginFactory pluginFactory = GetProcessingCardsPluginFactory(custProcCenterID);
			CCCustomerInformationManager cim = GetCustomerInformationManager(pluginFactory);
			CardProcessingReadersProvider readersProvider = new CardProcessingReadersProvider(new CCProcessingContext()
			{
				processingCenter = pluginFactory.GetProcessingCenter(),
				aCustomerID = bAccountId,
				aPMInstanceID = pmInstanceId,
				callerGraph = graph
			});
			cim.SetReadersProvider(readersProvider);
			CreditCardData cardData = cim.GetPaymentProfile();
			if (cardData == null)
			{
				throw new PXException(Messages.CreditCardNotFoundInProcCenter, CCPID, paymentProfile.CCProcessingCenterID);
			}
			foreach (Tuple<ICCPaymentProfileDetail, ICCPaymentMethodDetail> det in paymentDetail.Select())
			{
				ICCPaymentProfileDetail ppd = det.Item1;
				ICCPaymentMethodDetail pmd = det.Item2;
				if (pmd.IsCCProcessingID != true && pmd.IsIdentifier == true && !string.IsNullOrEmpty(cardData.CardNumber))
				{
					ppd.Value = cardData.CardNumber;
					paymentDetail.Cache.Update(ppd);
				}
			}
			if (cardData.CardExpirationDate != null)
			{
				payment.Cache.SetValueExt(paymentProfile, nameof(paymentProfile.ExpirationDate), cardData.CardExpirationDate);
				payment.Cache.Update(paymentProfile);
			}
		}

		public static void DeletePaymentProfile(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			IEnumerator cpmEnumerator = payment.Cache.Deleted.GetEnumerator();
			if (cpmEnumerator.MoveNext())
			{
				ICCPaymentProfile current = (ICCPaymentProfile)cpmEnumerator.Current;
				ProcessingCardsPluginFactory pluginFactory = GetProcessingCardsPluginFactory(current.CCProcessingCenterID);
				CCProcessingCenter processingCenter = pluginFactory.GetProcessingCenter();
				if (!string.IsNullOrEmpty(current.CCProcessingCenterID) && processingCenter.SyncronizeDeletion == true)
				{
					CCCustomerInformationManager cim = GetCustomerInformationManager(pluginFactory);
					CardProcessingReadersProvider readersProvider = new CardProcessingReadersProvider(new CCProcessingContext()
					{
						processingCenter = pluginFactory.GetProcessingCenter(),
						aCustomerID = current.BAccountID,
						aPMInstanceID = current.PMInstanceID,
						callerGraph = pluginFactory.GetPaymentProcessingRepository().Graph
					});
					cim.SetReadersProvider(readersProvider);
					ICCPaymentProfileDetail ccpidCPMDet = null;
					PaymentMethodDetail ccpidPMDet = PXSelect<PaymentMethodDetail,
						Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>,
							And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>, And<PaymentMethodDetail.isCCProcessingID, Equal<True>>>>>.Select(graph, current.PaymentMethodID);
					foreach (ICCPaymentProfileDetail deletedDet in paymentDetail.Cache.Deleted)
					{
						if (deletedDet.DetailID == ccpidPMDet.DetailID)
						{
							ccpidCPMDet = deletedDet;
							break;
						}
					}
					if (ccpidCPMDet != null && !string.IsNullOrEmpty(ccpidCPMDet.Value))
					{
						cim.DeletePaymentProfile();
					}
				}
			}
		}
		public static TranProfile GetOrCreatePaymentProfileByTran(PXGraph graph, ICCPaymentProfileAdapter adapter, string tranId)
		{
			ICCPaymentProfile paymentProfile = adapter.Current;
			ProcessingCardsPluginFactory pluginFactory = GetProcessingCardsPluginFactory(paymentProfile.CCProcessingCenterID);
			CCProcessingCenter processingCenter = pluginFactory.GetProcessingCenter();
			CCCustomerInformationManager cim = GetCustomerInformationManager(pluginFactory);

			CardProcessingReadersProvider readersProvider = new CardProcessingReadersProvider(new CCProcessingContext()
			{
				processingCenter = processingCenter,
				aCustomerID = paymentProfile.BAccountID,
				aPMInstanceID = paymentProfile.PMInstanceID,
				callerGraph = graph
			});
			cim.SetReadersProvider(readersProvider);

			Customer customer = new PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(graph).Select(paymentProfile.BAccountID);
			TranProfile ret = null;
			if (paymentProfile.CustomerCCPID == null)
			{
				ret = cim.GetOrCreateCustomerProfileFromTransaction(tranId, new CreateTranPaymentProfileParams() { LocalCustomerId = customer.AcctCD });
				return ret;
			}
			var cardLimit = processingCenter.CreditCardLimit;
			if (processingCenter.CreateAdditionalCustomerProfiles == true && cardLimit != null && cardLimit > 0)
			{
				int customerProfileCount = CCProcessingHelper.CustomerProfileCountPerCustomer(graph,
					paymentProfile.BAccountID,
					paymentProfile.CCProcessingCenterID);

				int allPaymentProfileCount = cim.GetAllPaymentProfiles().Count();
				if (CCProcessingHelper.IsCreditCardCountEnough(allPaymentProfileCount, cardLimit.Value))
				{
					var prefix = CCProcessingHelper.BuildPrefixForCustomerCD(customerProfileCount, processingCenter);
					ret = cim.GetOrCreateCustomerProfileFromTransaction(tranId, new CreateTranPaymentProfileParams() { LocalCustomerId = prefix + customer.AcctCD });
				}
				else
				{
					ret = cim.GetOrCreateCustomerProfileFromTransaction(tranId, new CreateTranPaymentProfileParams() { PCCustomerId = paymentProfile.CustomerCCPID });
				}
			}
			else
			{
				ret = cim.GetOrCreateCustomerProfileFromTransaction(tranId, new CreateTranPaymentProfileParams() { PCCustomerId = paymentProfile.CustomerCCPID });
			}
			return ret;
		}

		private static ProcessingCardsPluginFactory GetProcessingCardsPluginFactory(string processingCenterId)
		{
			ProcessingCardsPluginFactory pluginFactory = new ProcessingCardsPluginFactory(processingCenterId);
			return pluginFactory;
		}

		private static CCCustomerInformationManager GetCustomerInformationManager(ProcessingCardsPluginFactory pluginFactory)
		{
			CCCustomerInformationManager cim = new CCCustomerInformationManager(pluginFactory);
			return cim;
		}
	}
}