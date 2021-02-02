using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CA;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public class GenericCustomerPaymentMethodDetailAdapter<T> : ICustomerPaymentMethodDetailAdapter where T : CustomerPaymentMethodDetail,new()
	{
		PXSelectBase<T> dataView;

		public GenericCustomerPaymentMethodDetailAdapter(PXSelectBase<T> dataView)
		{
			this.dataView = dataView;
		}

		public CustomerPaymentMethodDetail Current => dataView.Current;

		public PXCache Cache => dataView.Cache;

		public PXResultset<CustomerPaymentMethodDetail,PaymentMethodDetail> Select( params object[] argumetns )
		{
			PXResultset<T> resultSet =  dataView.Select(argumetns);
			PXResultset<CustomerPaymentMethodDetail,PaymentMethodDetail> retResultSet = new PXResultset<CustomerPaymentMethodDetail, PaymentMethodDetail>();
			foreach(PXResult<T, PaymentMethodDetail> result in resultSet)
			{
				CustomerPaymentMethodDetail cpmd = result;
				PaymentMethodDetail pmd = (PaymentMethodDetail)result;
				retResultSet.Add(new PXResult<CustomerPaymentMethodDetail,PaymentMethodDetail>(cpmd,pmd));
			}
			return retResultSet;
		}
	}
}
