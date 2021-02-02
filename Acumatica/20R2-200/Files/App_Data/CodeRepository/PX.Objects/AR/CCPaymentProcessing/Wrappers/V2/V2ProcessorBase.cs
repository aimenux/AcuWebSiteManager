using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public abstract class V2ProcessorBase : ISetCardProcessingReadersProvider
	{
		protected ICardProcessingReadersProvider _provider;
		public V2ProcessorBase()
		{

		}
		protected ICardProcessingReadersProvider GetProvider()
		{
			if(_provider == null)
			{
				throw new PXInvalidOperationException(NotLocalizableMessages.ERR_CardProcessingReadersProviderGetting);
			}
			return _provider;
		}

		public void SetProvider( ICardProcessingReadersProvider provider )
		{
			_provider = provider;
		}
	}
}
