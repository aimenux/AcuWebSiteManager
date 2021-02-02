using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class WageBaseInclusion
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { AllWages, StateSpecific, LocationSpecific },
				new string[] { Messages.AllWages, Messages.StateSpecific, Messages.LocationSpecific })
			{ }
		}

		public const string AllWages = "ALL";
		public const string StateSpecific = "STE";
		public const string LocationSpecific = "LCL";

	}
}
