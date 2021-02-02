using PX.Data;
using System;

namespace PX.Objects.PR
{
	internal class ConditionEvaluator : PXBaseConditionAttribute
	{
		public static bool GetResult(PXCache sender, object row, Type conditionType)
		{
			return GetConditionResult(sender, row, conditionType);
		}
	}
}
