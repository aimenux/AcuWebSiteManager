using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	/// <summary>
	/// Retainage Modes options
	/// </summary>
	public static class RetainageModes
	{
		/// <summary>
		/// Retainage Modes List Attribute
		/// </summary>
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Normal, Messages.Retainage_Normal),
					Pair(Contract, Messages.Retainage_Contract),
					Pair(Line, Messages.Retainage_Line),
				})
			{ }
		}

		public const string Normal = "N";
		public const string Contract = "C";
		public const string Line = "L";

		/// <summary>
		/// Contract level retainage
		/// </summary>
		public class contract : PX.Data.BQL.BqlString.Constant<contract>
		{
			public contract() : base(Contract) {; }
		}

		/// <summary>
		/// Line level retainage
		/// </summary>
		public class line : PX.Data.BQL.BqlString.Constant<line>
		{
			public line() : base(Line) {; }
		}

	}
}
