using System;
using System.Collections;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Specific;
using System.Linq;

namespace PX.Objects.CA
{
	public class PaymentMethodConverter : PXGraph<PaymentMethodConverter>
	{
		public PXFilter<Filter> filter;
		public PXFilteredProcessing<CustomerPaymentMethod, Filter> CustomerPaymentMethodList;
		public IEnumerable customerPaymentMethodList()
		{
			var query = new PXSelectJoinGroupBy<CustomerPaymentMethod,
				LeftJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethodDetail.pMInstanceID, Equal<CustomerPaymentMethod.pMInstanceID>>>, 
				Where<CustomerPaymentMethod.paymentMethodID, Equal<Current<Filter.oldPaymentMethodID>>,
					And<CustomerPaymentMethod.cCProcessingCenterID, Equal<Current<Filter.oldCCProcessingCenterID>>,
					And<CustomerPaymentMethodDetail.value, IsNotNull>>>,
				Aggregate<GroupBy<CustomerPaymentMethod.pMInstanceID>>>(this);
			foreach (PXResult<CustomerPaymentMethod,CustomerPaymentMethodDetail> items in query.Select())
			{
				CustomerPaymentMethod cpm = (CustomerPaymentMethod)items;
				yield return cpm;
			}
		}

		public PXSelect<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Optional<Filter.newCCProcessingCenterID>>>> NewProcessingCenter;
		public PXSelect<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Optional<Filter.oldCCProcessingCenterID>>>> OldProcessingCenter;
		public PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Optional<CustomerPaymentMethod.pMInstanceID>>>> NewCustomerPM;
		public PXSelectJoin<CustomerPaymentMethodDetail, LeftJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
					And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>, 
					Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Optional<CustomerPaymentMethod.pMInstanceID>>>> CustomerPMDetails;
		string[] knownPlugins = new string[] { AuthnetConstants.AIMPluginFullName, AuthnetConstants.CIMPluginFullName, AuthnetConstants.APIPluginFullName };
		[Serializable]
		public partial class Filter : IBqlTable
		{
			#region OldPaymentMethodID
			public abstract class oldPaymentMethodID : PX.Data.BQL.BqlString.Field<oldPaymentMethodID> { }
			protected String _OldPaymentMethodID;
			[PXDBString(10, IsUnicode = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Old Payment Method ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof (Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>>>))]
			public virtual String OldPaymentMethodID
			{
				get { return this._OldPaymentMethodID; }
				set { this._OldPaymentMethodID = value; }
			}
			#endregion
			#region OldCCProcessingCenterID
			public abstract class oldCCProcessingCenterID : PX.Data.BQL.BqlString.Field<oldCCProcessingCenterID> { }
			[PXDBString(10, IsUnicode = true)]
			[ProcCenterByPluginTypesSelector(typeof(Search2<CCProcessingCenterPmntMethod.processingCenterID,
				InnerJoin<PaymentMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
				InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>>,
				Where<PaymentMethod.paymentMethodID, Equal<Current<Filter.oldPaymentMethodID>>>>),
				typeof(CCProcessingCenterPmntMethod.processingCenterID),
				new string[] { AuthnetConstants.AIMPluginFullName, AuthnetConstants.CIMPluginFullName, PluginConstants.V1PluginBaseFullName })]
			[PXUIField(DisplayName = "Old Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string OldCCProcessingCenterID { get; set; }
			#endregion
			#region NewCCProcessingCenterID
			public abstract class newCCProcessingCenterID : PX.Data.BQL.BqlString.Field<newCCProcessingCenterID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXDefault()]
			[ProcCenterByPluginTypesSelector(typeof(Search<CCProcessingCenter.processingCenterID>),
				typeof(CCProcessingCenter.processingCenterID),
				new string[] { PluginConstants.V2PluginInterface })]
			[PXUIField(DisplayName = "New Proc. Center ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string NewCCProcessingCenterID { get; set; }
			#endregion
			#region ProcessExpiredcards
			public abstract class processExpiredCards : PX.Data.BQL.BqlString.Field<processExpiredCards> { }
			[PXBool]
			[PXUnboundDefault(false)]
			[PXUIField(DisplayName = "Process Expired Cards")]
			public virtual bool? ProcessExpiredCards { get; set; }
			#endregion
		}

		public PXCancel<Filter> Cancel;

		protected virtual void Filter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			Filter row = (Filter)e.Row;
			bool allDataIsFilled = !string.IsNullOrEmpty(row.OldPaymentMethodID) 
				&& !string.IsNullOrEmpty(row.OldCCProcessingCenterID) 
				&& !string.IsNullOrEmpty(row.NewCCProcessingCenterID);
			CustomerPaymentMethodList.SetProcessEnabled(allDataIsFilled);
			CustomerPaymentMethodList.SetProcessAllEnabled(allDataIsFilled);
			if (allDataIsFilled)
			{
				CCProcessingCenter newCCPC = NewProcessingCenter.Select();
				string oldProcCenterId = OldProcessingCenter.SelectSingle()?.ProcessingCenterID;
				string newProcCenterId = NewProcessingCenter.SelectSingle()?.ProcessingCenterID;
				CustomerPaymentMethodList.SetParametersDelegate(cpm =>
				{
					WebDialogResult result = filter.Ask(PXMessages.LocalizeFormatNoPrefix(Messages.PaymentMethodConverterWarning, oldProcCenterId, newProcCenterId), MessageButtons.OKCancel);
					return result == WebDialogResult.OK ? true : false;
				});
				CustomerPaymentMethodList.SetProcessDelegate(cpm => ConvertCustomerPaymentMethod(row, cpm, newCCPC));
			}

			ShowUnknownPluginWarningIfNeeded(row, OldProcessingCenter.Select(), nameof(Filter.OldCCProcessingCenterID));
			ShowUnknownPluginWarningIfNeeded(row, NewProcessingCenter.Select(), nameof(Filter.NewCCProcessingCenterID));
		}

		protected virtual void Filter_ProcessExpiredCards_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Filter row = e.Row as Filter;
			if (row?.ProcessExpiredCards == true)
			{
				sender.RaiseExceptionHandling<Filter.processExpiredCards>(row, row.ProcessExpiredCards,
					new PXSetPropertyException(Messages.ProcessExpiredCardWarning, PXErrorLevel.Warning));
			}
		}

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center ID")]
		protected virtual void CustomerPaymentMethod_CCProcessingCenterID_CacheAttached(PXCache sender)
		{
		}

		private static void ConvertCustomerPaymentMethod(Filter filter, CustomerPaymentMethod cpm, CCProcessingCenter newCCPC)
		{
			if (newCCPC == null)
			{
				throw new PXException(Messages.NotSetProcessingCenter);
			}

			PaymentMethodUpdater updaterGraph = PXGraph.CreateInstance<PaymentMethodUpdater>();
			updaterGraph.ConvertCustomerPaymentMethod(filter.ProcessExpiredCards.GetValueOrDefault(), cpm, newCCPC);
		}

		protected virtual void Filter_OldPaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Filter filter = e.Row as Filter;
			if (filter == null) return;
			cache.SetDefaultExt<Filter.oldCCProcessingCenterID>(filter);
		}

		protected virtual PaymentMethodDetail FindSameTypeTemplate(PaymentMethod targetPM, PaymentMethodDetail baseDetail)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
				And<PaymentMethodDetail.isIdentifier, Equal<Required<PaymentMethodDetail.isIdentifier>>,
				And<PaymentMethodDetail.isExpirationDate, Equal<Required<PaymentMethodDetail.isExpirationDate>>,
				And<PaymentMethodDetail.isOwnerName, Equal<Required<PaymentMethodDetail.isOwnerName>>,
				And<PaymentMethodDetail.isCCProcessingID, Equal<Required<PaymentMethodDetail.isCCProcessingID>>>>>>>>>.
					Select(this, targetPM.PaymentMethodID, baseDetail.IsIdentifier, baseDetail.IsExpirationDate, baseDetail.IsOwnerName, baseDetail.IsCCProcessingID);
			return res;
		}

		protected virtual PaymentMethodDetail FindCCPID(PaymentMethod pm)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.isCCProcessingID, Equal<True>>>>.Select(this, pm.PaymentMethodID);
			return res;
		}

		private void ShowUnknownPluginWarningIfNeeded(Filter row, CCProcessingCenter procCenter, string fName)
		{
			if (procCenter != null)
			{
				if (procCenter != null && !IsKnownPlugun(procCenter.ProcessingTypeName))
				{
					filter.Cache.RaiseExceptionHandling(fName, row, row.OldCCProcessingCenterID, new PXSetPropertyException(Messages.IncompatiblePluginForCardProcessing, PXErrorLevel.Warning));
				}
				else
				{
					filter.Cache.RaiseExceptionHandling(fName, row, row.OldCCProcessingCenterID, null);
				}
			}
		}

		private bool IsKnownPlugun(string pluginType)
		{
			return knownPlugins.Contains(pluginType);
		}
	}

	public class PaymentMethodUpdater : PXGraph<PaymentMethodUpdater, CustomerPaymentMethod>
	{
		public PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Optional<CustomerPaymentMethod.pMInstanceID>>>> CustomerPM;
		public PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
					And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
					Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Optional<CustomerPaymentMethod.pMInstanceID>>>> CustomerPMDetails;

		public PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>>> PM;
		public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<CustomerPaymentMethod.paymentMethodID>>>> PMDetails;
		public PXSelect<CCProcessingCenterPmntMethod> ProcessingCenterPM;

		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<CCProcessingCenterPmntMethod.processingCenterID, Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<CustomerPaymentMethod.paymentMethodID>>>>), DirtyRead = true)]
		protected virtual void CustomerPaymentMethod_CCProcessingCenterID_CacheAttached(PXCache sender)
		{
		}

		[PXDBForeignIdentity(typeof(PMInstance), IsKey = true)]
		protected virtual void CustomerPaymentMethod_PMInstanceID_CacheAttached(PXCache sender)
		{
		}

		public void ConvertCustomerPaymentMethod(bool processExpiredCards, CustomerPaymentMethod cpm, CCProcessingCenter newCCPC)
		{
			CustomerPM.Current = cpm;
			DateTime expiredDate;
			if (processExpiredCards && CheckCardIsExpired(cpm, out expiredDate) && !ProcCenterSupportTokenizing(cpm.CCProcessingCenterID))
			{
				foreach (CustomerPaymentMethodDetail cpmd in CustomerPMDetails.Select().RowCast<CustomerPaymentMethodDetail>())
				{
					CustomerPMDetails.Delete(cpmd);
				}
				cpm.IsActive = false;
				CustomerPM.Update(cpm);
			}
			else
			{
				DoConvert(cpm, newCCPC);
			}
			this.Save.Press();
		}

		private void DoConvert(CustomerPaymentMethod cpm, CCProcessingCenter newCCPC)
		{
			CCProcessingCenterPmntMethod newProcessingCenterPM = PXSelect<CCProcessingCenterPmntMethod,
				Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Required<CCProcessingCenterPmntMethod.paymentMethodID>>,
				And<CCProcessingCenterPmntMethod.processingCenterID, Equal<Required<CCProcessingCenterPmntMethod.processingCenterID>>>>>.Select(this, cpm.PaymentMethodID, newCCPC.ProcessingCenterID);
			if (newProcessingCenterPM == null)
			{
				newProcessingCenterPM = (CCProcessingCenterPmntMethod)ProcessingCenterPM.Cache.CreateInstance();
				newProcessingCenterPM.PaymentMethodID = cpm.PaymentMethodID;
				newProcessingCenterPM.ProcessingCenterID = newCCPC.ProcessingCenterID;
				ProcessingCenterPM.Insert(newProcessingCenterPM);
			}

			CustomerPaymentMethod currCPM = (CustomerPaymentMethod)CustomerPM.Cache.CreateCopy(cpm);
			var oldCCProcessingCenterID = currCPM.CCProcessingCenterID;
			currCPM.CCProcessingCenterID = newCCPC.ProcessingCenterID;
			if (currCPM.CustomerCCPID == null)
			{
				CustomerPM.Cache.SetDefaultExt<CustomerPaymentMethod.customerCCPID>(currCPM);
			}
			currCPM.Selected = true;
			currCPM = CustomerPM.Update(currCPM);
			CustomerPM.Current = currCPM;

			PXResultset<PaymentMethodDetail> oldDetails = PMDetails.Select(currCPM.PaymentMethodID);
			foreach (PaymentMethodDetail oldDetail in oldDetails)
			{
				PaymentMethodDetail newDetail = (PaymentMethodDetail)PMDetails.Cache.CreateCopy(oldDetail);
				newDetail.ValidRegexp = null;
				PMDetails.Update(newDetail);
			}

			PaymentMethod CurrPM = PM.Select();
			PaymentMethodDetail CCPID = FindCCPID(CurrPM);

			if (CCPID == null)
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PaymentMethodDetail res;
					CCPID = (PaymentMethodDetail)PMDetails.Cache.CreateInstance();
					CCPID.PaymentMethodID = currCPM.PaymentMethodID;
					CCPID.UseFor = PaymentMethodDetailUsage.UseForARCards;
					CCPID.DetailID = "CCPID";
					CCPID.Descr = Messages.PaymentProfileID;
					CCPID.IsCCProcessingID = true;
					CCPID.IsRequired = true;
					res = PMDetails.Insert(CCPID);
					if (res == null)
					{
						throw new PXException(Messages.CouldNotInsertPMDetail);
					}
					else
					{
						PMDetails.Cache.Persist(PXDBOperation.Insert);
					}
					ts.Complete();
				}
			}

			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>
					.Select(this, oldCCProcessingCenterID);
			bool oldProcCenterSupportTokenizing = ProcCenterSupportTokenizing(oldCCProcessingCenterID);
			bool newProcCenterSupportTokenizing = ProcCenterSupportTokenizing(newCCPC.ProcessingCenterID);

			if (!oldProcCenterSupportTokenizing && newProcCenterSupportTokenizing)
			{
				CustomerPaymentMethodDetail newCCPIDPM = PXSelect<CustomerPaymentMethodDetail,
					Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Required<CustomerPaymentMethodDetail.pMInstanceID>>,
						And<CustomerPaymentMethodDetail.paymentMethodID, Equal<Required<CustomerPaymentMethodDetail.paymentMethodID>>,
						And<CustomerPaymentMethodDetail.detailID, Equal<Required<CustomerPaymentMethodDetail.detailID>>>>>>
							.Select(this, currCPM.PMInstanceID, currCPM.PaymentMethodID, CCPID.DetailID);
				if (newCCPIDPM != null)
				{
					newCCPIDPM.Value = null;
					CustomerPMDetails.Update(newCCPIDPM);
				}
				else
				{
					newCCPIDPM = new CustomerPaymentMethodDetail
					{
						PMInstanceID = currCPM.PMInstanceID,
						PaymentMethodID = currCPM.PaymentMethodID,
						DetailID = CCPID.DetailID
					};
					CustomerPMDetails.Insert(newCCPIDPM);
				}
				var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
				ICCPaymentProfileAdapter paymentProfile = new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(CustomerPM);
				ICCPaymentProfileDetailAdapter profileDetail = new GenericCCPaymentProfileDetailAdapter<CustomerPaymentMethodDetail,
					PaymentMethodDetail>(CustomerPMDetails, PMDetails);
				DateTime expiredDate;
				if (CheckCardIsExpired(currCPM, out expiredDate))
				{
					Customer cust =  new PXSelect<Customer, 
						Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>(this)
							.SelectSingle(currCPM.BAccountID);
					throw new PXException(AR.Messages.ERR_CCCreditCardHasExpired, expiredDate.ToString("d"), cust.AcctCD);
				}
				graph.GetOrCreatePaymentProfile(this, paymentProfile, profileDetail);
			}

			if (newProcCenterSupportTokenizing)
			{
				if (currCPM.CustomerCCPID == null)
				{
					currCPM.CustomerCCPID = cpm.CustomerCCPID;
				}
				CustomerProcessingCenterID newCustomerProcessingCenterID = new CustomerProcessingCenterID
				{
					CCProcessingCenterID = newCCPC.ProcessingCenterID,
					BAccountID = cpm.BAccountID,
					CustomerCCPID = currCPM.CustomerCCPID
				};
				AddCustomerProcessingCenterIfNeeded(newCustomerProcessingCenterID);
			}
			currCPM = CustomerPM.Update(currCPM);
		}

		private bool CheckCardIsExpired(CustomerPaymentMethod cpm, out DateTime expiredDate)
		{
			expiredDate = DateTime.MinValue;
			bool ret = false;
			if (cpm.ExpirationDate != null)
			{
				DateTime dt = DateTime.Now.Date;
				if (cpm.ExpirationDate <= dt)
				{
					expiredDate = cpm.ExpirationDate.Value;
					ret = true;
				}
			}
			else
			{
				string expDateDetailId = CustomerPMDetails.Select().RowCast<PaymentMethodDetail>()
					.Where(i => i.IsExpirationDate == true).Select(i => i.DetailID).FirstOrDefault();
				string expDateStr = CustomerPMDetails.Select().RowCast<CustomerPaymentMethodDetail>()
						.Where(i => i.DetailID == expDateDetailId).Select(i => i.Value).FirstOrDefault();
				if (expDateStr != null)
				{
					DateTime? expDate = CustomerPaymentMethodMaint.ParseExpiryDate(this, cpm.CCProcessingCenterID, expDateStr);
					if (expDate != null)
					{
						expDate = expDate.Value.AddMonths(1);
						if (expDate <= DateTime.Now.Date)
						{
							expiredDate = expDate.Value;
							ret = true;
						}
					}
				}
			}
			return ret;
		}

		private void AddCustomerProcessingCenterIfNeeded(CustomerProcessingCenterID newCustomerProcessingCenterID)
		{
			CustomerProcessingCenterID customerProcessingCenter = PXSelect<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CustomerProcessingCenterID.cCProcessingCenterID>>,
					And<CustomerProcessingCenterID.bAccountID, Equal<Required<CustomerProcessingCenterID.bAccountID>>,
					And<CustomerProcessingCenterID.customerCCPID, Equal<Required<CustomerProcessingCenterID.customerCCPID>>>>>>
						.Select(this, newCustomerProcessingCenterID.CCProcessingCenterID, newCustomerProcessingCenterID.BAccountID, newCustomerProcessingCenterID.CustomerCCPID);

			if (customerProcessingCenter == null)
			{
				PXCache cache = Caches[typeof(CustomerProcessingCenterID)];
				newCustomerProcessingCenterID = cache.Insert(newCustomerProcessingCenterID) as CustomerProcessingCenterID;
				cache.PersistInserted(newCustomerProcessingCenterID);
			}
		}

		protected virtual PaymentMethodDetail FindSameTypeTemplate(PaymentMethod targetPM, PaymentMethodDetail baseDetail)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
				And<PaymentMethodDetail.isIdentifier, Equal<Required<PaymentMethodDetail.isIdentifier>>,
				And<PaymentMethodDetail.isExpirationDate, Equal<Required<PaymentMethodDetail.isExpirationDate>>,
				And<PaymentMethodDetail.isOwnerName, Equal<Required<PaymentMethodDetail.isOwnerName>>,
				And<PaymentMethodDetail.isCCProcessingID, Equal<Required<PaymentMethodDetail.isCCProcessingID>>>>>>>>>.
					Select(this, targetPM.PaymentMethodID, baseDetail.IsIdentifier, baseDetail.IsExpirationDate, baseDetail.IsOwnerName, baseDetail.IsCCProcessingID);
			return res;
		}

		protected virtual PaymentMethodDetail FindCCPID(PaymentMethod pm)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
				And<PaymentMethodDetail.isCCProcessingID, Equal<True>>>>.Select(this, pm.PaymentMethodID);
			return res;
		}

		private bool ProcCenterSupportTokenizing(string procCenterId)
		{
			var query = new PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>(this);
			CCProcessingCenter procCenter = query.Select(procCenterId);
			string typeName = procCenter.ProcessingTypeName;
			if (typeName == AuthnetConstants.AIMPluginFullName)
			{
				return false;
			}
			if (typeName == AuthnetConstants.CIMPluginFullName || 
				typeName == AuthnetConstants.APIPluginFullName)
			{
				return true;
			}
			Type type = CCPluginTypeHelper.GetPluginType(procCenter.ProcessingTypeName);
			if (CCPluginTypeHelper.CheckImplementInterface(type, PluginConstants.V1TokenizedInterface))
			{
				return true;
			}
			if (CCProcessingFeatureHelper.IsFeatureSupported(type, CCProcessingFeature.ProfileManagement))
			{
				return true;
			}
			return false;
		}
	}
}