using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing
{ 
	public class GenericCustomerPaymentMethodAdaper<T> : ICustomerPaymentMethodAdapter where T : CustomerPaymentMethod,new()
	{
		PXSelectBase<T> dataView;

		public GenericCustomerPaymentMethodAdaper(PXSelectBase<T> dataView)
		{
			this.dataView = dataView;
		}

		public CustomerPaymentMethod Current => dataView.Current;

		public PXCache Cache => dataView.Cache;
	}
}
