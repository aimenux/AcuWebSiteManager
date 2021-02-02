using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Repositories;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public interface ISetCardProcessingReadersProvider 
	{
		void SetProvider(ICardProcessingReadersProvider provider);
	}
}