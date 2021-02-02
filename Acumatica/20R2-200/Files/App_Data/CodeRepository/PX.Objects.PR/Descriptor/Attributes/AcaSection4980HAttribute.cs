using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class AcaSection4980H
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { "2A", "2B", "2C", "2D", "2E", "2F", "2G", "2H" },
				new string[] { "2A", "2B", "2C", "2D", "2E", "2F", "2G", "2H" })
			{
			}
		}
	}
}
