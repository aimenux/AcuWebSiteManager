using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.AR.CCPaymentProcessing
{
	class GenericCCPaymentProfileAdapter<T> : ICCPaymentProfileAdapter where T : class, IBqlTable, ICCPaymentProfile, new()
	{
		PXSelectBase<T> dataView;
		public GenericCCPaymentProfileAdapter(PXSelectBase<T> dataView)
		{
			this.dataView = dataView;
		}

		public ICCPaymentProfile Current => dataView.Current;

		public PXCache Cache => dataView.Cache;
	}
}
