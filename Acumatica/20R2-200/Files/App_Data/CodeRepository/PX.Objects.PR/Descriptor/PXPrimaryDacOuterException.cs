using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PXPrimaryDacOuterException : PXOuterException
	{
		public PXPrimaryDacOuterException(PXOuterException oe, PXCache cache, Type primaryDac) : base(
			new Dictionary<string, string>(),
			oe.GraphType,
			oe.Row,
			oe.Message)
		{
			// If any error message exists for primaryDac, or other unrelated DAC's, discard messages from DAC's inherited by primaryDac
			for (int i = 0; i < oe.InnerFields.Length; i++)
			{
				Type primaryDacField = cache.GetBqlField(oe.InnerFields[i]);
				if (primaryDacField == null || primaryDacField.DeclaringType == primaryDac)
				{
					_InnerExceptions[oe.InnerFields[i]] = oe.InnerMessages[i];
				}
			}

			// If previous filtering cleared all errors, restore original errors
			if (!_InnerExceptions.Any())
			{
				for (int i = 0; i < oe.InnerFields.Length; i++)
				{
					_InnerExceptions[oe.InnerFields[i]] = oe.InnerMessages[i];
				}
			}
		}
	}
}
