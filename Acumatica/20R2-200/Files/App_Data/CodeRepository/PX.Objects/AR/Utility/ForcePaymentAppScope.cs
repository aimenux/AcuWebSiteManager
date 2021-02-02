using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;

namespace PX.Objects.AR
{
	public class ForcePaymentAppScope : IDisposable
	{
		private readonly static string scopeKey;

		static ForcePaymentAppScope()
		{
			scopeKey = nameof(ARPaymentEntry.ForcePaymentApp);
		}

		public ForcePaymentAppScope()
		{
			PXContext.SetSlot(scopeKey, true);
		}

		void IDisposable.Dispose()
		{
			PXContext.SetSlot(scopeKey, false);
		}

		public static bool IsActive => PXContext.GetSlot<bool>(scopeKey);
	}
}
