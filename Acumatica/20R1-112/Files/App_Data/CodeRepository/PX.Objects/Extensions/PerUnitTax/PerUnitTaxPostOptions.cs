using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.TX;

namespace PX.Objects.Extensions.PerUnitTax
{
	/// <summary>
	/// A set of post options for per unit tax amount during the release of the document.
	/// </summary>
	public class PerUnitTaxPostOptions
	{
		/// <summary>
		/// Post per-unit tax amount to the document line account.
		/// </summary>
		public const string LineAccount = "L";

		/// <summary>
		/// Post per-unit tax amount to the account specified in tax settings.
		/// </summary>
		public const string TaxAccount = "T";

		/// <summary>
		/// Post per-unit tax amount to the document line account.
		/// </summary>
		public class lineAccount : BqlString.Constant<lineAccount>
		{
			public lineAccount() : base(LineAccount) {; }
		}

		/// <summary>
		/// Post per-unit tax amount to the account specified in tax settings.
		/// </summary>
		public class taxAccount : BqlString.Constant<taxAccount>
		{
			public taxAccount() : base(TaxAccount) {; }
		}

		/// <summary>
		/// String list attribute with a list of per unit tax post options. 
		/// </summary>
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { LineAccount, TaxAccount },
				new string[] { Messages.PostPerUnitTaxToLineAccount, Messages.PostPerUnitTaxToTaxAccount })
			{ }
		}
	}
}
