namespace PX.Objects.AR.CCPaymentProcessing
{
	using PX.Objects.AR.CCPaymentProcessing.Interfaces;
	using CCProcessingBase.Interfaces.V2;
	using PX.Data;
	using System;
	public class CCCustomerInformationManagerGraph : PXGraph<CCCustomerInformationManagerGraph>
	{
		public virtual void GetOrCreatePaymentProfile(PXGraph graph, ICCPaymentProfileAdapter paymentProfileAdapter, ICCPaymentProfileDetailAdapter profileDetailAdapter)
		{
			CCCustomerInformationManager.GetOrCreatePaymentProfile(graph, paymentProfileAdapter, profileDetailAdapter);
		}

		public virtual void GetCreatePaymentProfileForm(PXGraph graph, ICCPaymentProfileAdapter ccPaymentProfileAdapter)
		{
			CCCustomerInformationManager.GetCreatePaymentProfileForm(graph, ccPaymentProfileAdapter);
		}

		public virtual void GetManagePaymentProfileForm(PXGraph graph, ICCPaymentProfile paymentProfile)
		{
			CCCustomerInformationManager.GetManagePaymentProfileForm(graph,paymentProfile);
		}

		public virtual void GetNewPaymentProfiles(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			CCCustomerInformationManager.GetNewPaymentProfiles(graph, payment, paymentDetail);
		}

		public virtual void GetPaymentProfile(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			CCCustomerInformationManager.GetPaymentProfile(graph, payment, paymentDetail);
		}

		public virtual PXResultset<CustomerPaymentMethodDetail> GetAllCustomersCardsInProcCenter(PXGraph graph, int? BAccountID, string CCProcessingCenterID)
		{
			return CCCustomerInformationManager.GetAllCustomersCardsInProcCenter(graph, BAccountID, CCProcessingCenterID);
		}

		public virtual void DeletePaymentProfile(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			CCCustomerInformationManager.DeletePaymentProfile(graph, payment, paymentDetail);
		}

		public virtual TranProfile GetOrCreatePaymentProfileByTran(PXGraph graph, ICCPaymentProfileAdapter payment, string tranId)
		{
			return CCCustomerInformationManager.GetOrCreatePaymentProfileByTran(graph, payment, tranId);
		}
	}
}