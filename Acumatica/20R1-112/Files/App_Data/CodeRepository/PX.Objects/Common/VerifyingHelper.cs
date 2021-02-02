using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common
{
	public class VerifyingHelper
	{
		public static object GetNewValueByIncoming(PXCache cache, object row, string fieldName, bool externalCall)
		{
			object incoming = null;

			if (externalCall)
			{
				incoming = cache.GetValuePending(row, fieldName);

				if (incoming != null)
				{
					return incoming;
				}
			}

			try
			{
				incoming = cache.GetValueExt(row, fieldName);

				return PXFieldState.UnwrapValue(incoming);
			}
			catch
			{
			}

			return cache.GetValue(row, fieldName);
		}
	}
}
