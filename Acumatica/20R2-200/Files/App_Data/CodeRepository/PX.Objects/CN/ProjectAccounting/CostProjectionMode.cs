using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class ProjectionMode
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Auto, Manual, ManualQuantity, ManualCost },
				new string[] { Messages.ProjectionModeAuto, Messages.ProjectionModeManual, Messages.ProjectionModeManualQuantity, Messages.ProjectionModeManualCost })
			{
			}
		}
		public const string Auto = "A";
		public const string Manual = "M";
		public const string ManualQuantity = "Q";
		public const string ManualCost = "C";
	}
}
