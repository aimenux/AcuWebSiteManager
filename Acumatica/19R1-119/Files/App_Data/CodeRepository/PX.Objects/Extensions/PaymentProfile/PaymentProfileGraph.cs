using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.SO;
using System.Reflection;
using PX.Common;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using System.Runtime.CompilerServices;
namespace PX.Objects.Extensions.PaymentProfile
{
	public abstract class PaymentProfileGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TPrimary : class,IBqlTable, new()
	{

		public PXSelectExtension<CustomerPaymentMethod> CustomerPaymentMethod;
		public PXSelectExtension<CustomerPaymentMethodDetail> CustomerPaymentMethodDetail;
		public PXSelectExtension<PaymentMethodDetail> PaymentMethodDetail;

		public PXAction<TPrimary> createCCPaymentMethodHF;
		public PXAction<TPrimary> syncCCPaymentMethods;
		public PXAction<TPrimary> manageCCPaymentMethodHF;

		[PXUIField(DisplayName = "Create New", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CreateCCPaymentMethodHF(PXAdapter adapter)
		{
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			CustomerPaymentMethod currentPaymentMethod = CustomerPaymentMethod.Current;
			if (currentPaymentMethod.CCProcessingCenterID == null)
			{
				CustomerPaymentMethod.Cache.SetDefaultExt<CustomerPaymentMethod.cCProcessingCenterID>(currentPaymentMethod);
				CustomerPaymentMethod.Cache.SetDefaultExt<CustomerPaymentMethod.customerCCPID>(currentPaymentMethod);
			}
			var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
			graph.GetCreatePaymentProfileForm(this.Base, new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(CustomerPaymentMethod));
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Sync", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable SyncCCPaymentMethods(PXAdapter adapter)
		{
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			CustomerPaymentMethod currentPaymentMethod = CustomerPaymentMethod.Current;
			PXTrace.WriteInformation($"{methodName}. CCProcessingCenterID:{currentPaymentMethod.CCProcessingCenterID}; UserName:{this.Base.Accessinfo.UserName}");
			IEnumerable ret = adapter.Get();

			bool isCancel = false;
			System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
			var cancelStr = request.Form.Get("__CLOSECCHFORM");
			bool.TryParse(cancelStr, out isCancel);
			if(isCancel)
			{
				return ret;
			}

			if (currentPaymentMethod.CCProcessingCenterID == null)
			{
				CustomerPaymentMethod.Cache.SetDefaultExt<CustomerPaymentMethod.cCProcessingCenterID>(currentPaymentMethod);
				CustomerPaymentMethod.Cache.SetDefaultExt<CustomerPaymentMethod.customerCCPID>(currentPaymentMethod);
			}
			var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();

			ICCPaymentProfileAdapter paymentProfile = new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(CustomerPaymentMethod);
			ICCPaymentProfileDetailAdapter profileDetail = new GenericCCPaymentProfileDetailAdapter<CustomerPaymentMethodDetail, PaymentMethodDetail>(CustomerPaymentMethodDetail, PaymentMethodDetail);
			bool isIDFilled = CCProcessingHelper.IsCCPIDFilled(this.Base, CustomerPaymentMethod.Current.PMInstanceID);
			if (!isIDFilled)
			{
				graph.GetNewPaymentProfiles(this.Base, paymentProfile, profileDetail);
			}
			else
			{
				graph.GetPaymentProfile(this.Base, paymentProfile, profileDetail);
			}
			this.Base.Persist();
			return ret;
		}

		[PXUIField(DisplayName = "Edit", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ManageCCPaymentMethodHF(PXAdapter adapter)
		{
			ICCPaymentProfile currentPaymentMethod = CustomerPaymentMethod.Current;
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			PXTrace.WriteInformation($"{methodName}; CCProcessingCenterID:{currentPaymentMethod.CCProcessingCenterID}.");
			var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
			graph.GetManagePaymentProfileForm(this.Base, currentPaymentMethod);
			return adapter.Get();
		}

		protected virtual void RefreshCreatePaymentAction(bool enable, bool visible)
		{
			createCCPaymentMethodHF.SetVisible(visible);
			createCCPaymentMethodHF.SetEnabled(enable);
		}
		protected virtual void RefreshSyncPaymentAction(bool enable)
		{
			//syncCCPaymentMethods.SetVisible(visible);
			syncCCPaymentMethods.SetEnabled(enable);
		}

		private string GetClassMethodName([CallerMemberName] string methodName="")
		{
			return this.GetType().Name + "." + methodName;
		}

		protected virtual void RefreshManagePaymentAction(bool enable, bool visible)
		{
			manageCCPaymentMethodHF.SetVisible(visible);
			manageCCPaymentMethodHF.SetEnabled(enable);
		}

		[PXOverride]
		public virtual void Persist(Action @base)
		{
			bool isCPMDeleting = CustomerPaymentMethod.Cache.Deleted.Count() != 0;
			bool isDetailsInserting = CustomerPaymentMethodDetail.Cache.Inserted.Count() != 0;

			var graph = PXGraph.CreateInstance<CCCustomerInformationManagerGraph>();
			ICCPaymentProfileAdapter paymentProfile = new GenericCCPaymentProfileAdapter<CustomerPaymentMethod>(CustomerPaymentMethod);
			ICCPaymentProfileDetailAdapter paymentProfileDetail = new GenericCCPaymentProfileDetailAdapter<CustomerPaymentMethodDetail, 
				PaymentMethodDetail>(CustomerPaymentMethodDetail, PaymentMethodDetail);
			if (!isCPMDeleting && isDetailsInserting && !string.IsNullOrEmpty(CustomerPaymentMethod.Current.CCProcessingCenterID)
				&& CCProcessingHelper.IsTokenizedPaymentMethod(this.Base, CustomerPaymentMethod.Current.PMInstanceID, true))
			{
				graph.GetOrCreatePaymentProfile(this.Base, paymentProfile, paymentProfileDetail);
			}
			//assuming only one record can be deleted from promary view of this graph at a time
			else if (isCPMDeleting && CCProcessingHelper.IsTokenizedPaymentMethod(this.Base, null, true))
			{
				graph.DeletePaymentProfile(this.Base, paymentProfile, paymentProfileDetail);
			}
			@base();
		}

		public override void Initialize()
		{
			base.Initialize();
			MapViews(this.Base);
		}

		protected virtual void RowSelected(Events.RowSelected<TPrimary> e)
		{

		}

		protected abstract CustomerPaymentMethodMapping GetCustomerPaymentMethodMapping();

		protected abstract CustomerPaymentMethodDetailMapping GetCusotmerPaymentMethodDetailMapping();

		protected abstract PaymentmethodDetailMapping GetPaymentMethodDetailMapping();

		protected virtual void MapViews(TGraph graph)
		{

		}

		protected class CustomerPaymentMethodMapping : IBqlMapping
		{
			public Type CCProcessingCenterID = typeof(CustomerPaymentMethod.cCProcessingCenterID);
			public Type CustomerCCPID = typeof(CustomerPaymentMethod.customerCCPID);
			public Type BAccountID = typeof(CustomerPaymentMethod.bAccountID);
			public Type PMInstanceID = typeof(CustomerPaymentMethod.pMInstanceID);
			public Type PaymentMethodID = typeof(CustomerPaymentMethod.paymentMethodID);
			public Type CashAccountID = typeof(CustomerPaymentMethod.cashAccountID);
			public Type Descr = typeof(CustomerPaymentMethod.descr);
			public Type ExpirationDate = typeof(CustomerPaymentMethod.expirationDate);

			public Type Extension => typeof(CustomerPaymentMethod);
			public Type Table { get; private set; }

			public CustomerPaymentMethodMapping(Type table)
			{
				Table = table;
			}
		}

		protected class PaymentmethodDetailMapping : IBqlMapping
		{
			public Type PaymentMethodID = typeof(PaymentMethodDetail.paymentMethodID);
			public Type UseFor = typeof(PaymentMethodDetail.useFor);
			public Type DetailID = typeof(PaymentMethodDetail.detailID);
			public Type Descr = typeof(PaymentMethodDetail.descr);
			public Type IsEncrypted = typeof(PaymentMethodDetail.isEncrypted);
			public Type IsRequired = typeof(PaymentMethodDetail.isRequired);
			public Type IsIdentifier = typeof(PaymentMethodDetail.isIdentifier);
			public Type IsExpirationDate = typeof(PaymentMethodDetail.isExpirationDate);
			public Type IsOwnerName = typeof(PaymentMethodDetail.isOwnerName);
			public Type IsCCProcessingID = typeof(PaymentMethodDetail.isCCProcessingID);
			public Type IsCVV = typeof(PaymentMethodDetail.isCVV);
			public Type Table { get; private set; }
			public Type Extension => typeof(PaymentMethodDetail);

			public PaymentmethodDetailMapping(Type table)
			{
				Table = table;
			}
		}

		protected class CustomerPaymentMethodDetailMapping : IBqlMapping
		{
			public Type PMInstanceID = typeof(CustomerPaymentMethodDetail.pMInstanceID);
			public Type PaymentMethodID = typeof(CustomerPaymentMethodDetail.paymentMethodID);
			public Type DetailID = typeof(CustomerPaymentMethodDetail.detailID);
			public Type Value = typeof(CustomerPaymentMethodDetail.value);
			public Type IsIdentifier = typeof(CustomerPaymentMethodDetail.isIdentifier);
			public Type IsCCProcessingID = typeof(CustomerPaymentMethodDetail.isCCProcessingID);
			public Type Extension => typeof(CustomerPaymentMethodDetail);
			public Type Table { get; private set; }
			public CustomerPaymentMethodDetailMapping(Type table)
			{
				Table = table;
			}
		}
	}
}
