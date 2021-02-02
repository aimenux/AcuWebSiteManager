using PX.Data;
using PX.Data.BQL.Fluent;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class ProjectCostAssignmentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { NoCostAssigned, WageCostAssigned, WageLaborBurdenAssigned },
				new string[] { Messages.NoCostAssigned, Messages.WageCostAssigned, Messages.WageLaborBurdenAssigned })
			{ }
		}

		public class noCostAssigned : PX.Data.BQL.BqlString.Constant<noCostAssigned>
		{
			public noCostAssigned() : base(NoCostAssigned) { }
		}

		public class wageCostAssigned : PX.Data.BQL.BqlString.Constant<wageCostAssigned>
		{
			public wageCostAssigned() : base(WageCostAssigned) { }
		}

		public class wageLaborBurdenAssigned : PX.Data.BQL.BqlString.Constant<wageLaborBurdenAssigned>
		{
			public wageLaborBurdenAssigned() : base(WageLaborBurdenAssigned) { }
		}

		public const string NoCostAssigned = "NCA";
		public const string WageCostAssigned = "WCA";
		public const string WageLaborBurdenAssigned = "WLB";
	}
}
