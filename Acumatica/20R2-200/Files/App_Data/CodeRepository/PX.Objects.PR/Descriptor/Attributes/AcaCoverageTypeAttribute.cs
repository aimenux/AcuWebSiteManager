using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class AcaCoverageType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { Employee, Spouse, Children },
				new string[]
				{
					Messages.Employee,
					Messages.Spouse,
					Messages.Children,
				})
			{
			}
		}

		public class employee : PX.Data.BQL.BqlString.Constant<employee>
		{
			public employee() : base(Employee) { }
		}

		public class spouse : PX.Data.BQL.BqlString.Constant<spouse>
		{
			public spouse() : base(Spouse) { }
		}

		public class children : PX.Data.BQL.BqlString.Constant<children>
		{
			public children() : base(Children) { }
		}

		public const string Employee = "EMP";
		public const string Spouse = "SPO";
		public const string Children = "CHI";
	}
}
