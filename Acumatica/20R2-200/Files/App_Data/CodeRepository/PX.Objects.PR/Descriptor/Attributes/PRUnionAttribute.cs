using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXUIField(DisplayName = "Union Local")]
	public class PRUnionAttribute : PMUnionAttribute
	{
		public PRUnionAttribute() : base(null, null)
		{
		}

		public PRUnionAttribute(Type project, Type employeeSearch) : base(project, employeeSearch)
		{
		}
	}
}
