using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common
{
	public static class UIState
	{
		public static void RaiseOrHideError<T>(PXCache cache, object row, bool isIncorrect, string message, PXErrorLevel errorLevel, params object[] parameters)
			where T : IBqlField
		{
			if (isIncorrect)
			{
				cache.RaiseExceptionHandling<T>(row, PXFieldState.UnwrapValue(cache.GetValueExt<T>(row)), new PXSetPropertyException(message, errorLevel, parameters));
			}
			else
			{
				cache.RaiseExceptionHandling<T>(row, PXFieldState.UnwrapValue(cache.GetValueExt<T>(row)), null);
			}
		}

		public static void RaiseOrHideErrorByErrorLevelPriority<T>(PXCache cache, object row, bool isIncorrect, string message, PXErrorLevel errorLevel, params object[] parameters)
			where T : IBqlField
		{
			if (IsHigherErrorLevelExist<T>(cache, row, errorLevel))
			{
				return;
			}

			RaiseOrHideError<T>(cache, row, isIncorrect, message, errorLevel, parameters);
		}

		public static bool IsHigherErrorLevelExist<T>(PXCache cache, object row, PXErrorLevel errorLevel)
			where T : IBqlField
		{
			PXFieldState state = (PXFieldState)cache.GetStateExt<T>(row);
			if (state.ErrorLevel > errorLevel)
			{
				return true;
			}

			return false;
		}
	}
}
