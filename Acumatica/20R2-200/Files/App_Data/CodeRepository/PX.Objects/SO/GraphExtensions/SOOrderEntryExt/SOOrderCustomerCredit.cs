using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class SOOrderCustomerCredit : SOOrderCustomerCreditExtension<SOOrderEntry>
	{
		protected override ARSetup GetARSetup()
			=> Base.arsetup.Current;
	}
}
