using System;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	/// <summary>
	/// Represents CashAccount Field with Selector that shows all Cash Accounts.
	/// </summary>
	[PXDBInt]
	public class CashAccountAttribute : CashAccountBaseAttribute
	{
		/// <summary>
		/// Constructor of the new CashAccountAttribute object with all default parameters.
		/// Doesn't filter by branch, doesn't suppress <see cref="CashAccount.Active"/> status verification
		/// </summary>
		public CashAccountAttribute() : this(suppressActiveVerification: false)
		{
		}

		/// <summary>
		/// Constructor of the new CashAccountAttribute object with all default parameters except the <paramref name="search"/>.
		/// Doesn't filter by branch, doesn't suppress <see cref="CashAccount.Active"/> status verification
		/// </summary>
		/// <param name="search">The type of search. Should implement <see cref="IBqlSearch"/> or <see cref="IBqlSelect"/></param>
		public CashAccountAttribute(Type search) : this(suppressActiveVerification: false, search: search)
		{
		}

		/// <summary>
		/// Constructor of the new CashAccountAttribute object. Doesn't filter by branch.
		/// </summary>
		/// <param name="suppressActiveVerification">True to suppress <see cref="CashAccount.Active"/> verification.</param>
		/// <param name="branchID">(Optional) Identifier for the branch.</param>
		/// <param name="search">(Optional) The type of search. Should implement <see cref="IBqlSearch"/> or <see cref="IBqlSelect"/></param>
		public CashAccountAttribute(bool suppressActiveVerification, Type branchID = null, Type search = null) : base(suppressActiveVerification, branchID, search)
		{	
		}

		/// <summary>
		/// Constructor of the new CashAccountAttribute object.
		/// </summary>
		/// <param name="suppressActiveVerification">True to suppress <see cref="CashAccount.Active"/> verification.</param>
		/// <param name="branchID">(Optional) Identifier for the branch.</param>
		/// <param name="search">(Optional) The type of search. Should implement <see cref="IBqlSearch"/> or <see cref="IBqlSelect"/></param>
		public CashAccountAttribute(bool suppressActiveVerification, bool filterBranch, Type branchID = null, Type search = null) : base(suppressActiveVerification, filterBranch, branchID, search)
		{
		}

		/// <summary>
		/// Constructor of the new CashAccountAttribute object. Filter by branch, doesn't suppress <see cref="CashAccount.Active"/> status verification.
		/// </summary>
		/// <param name="branchID">Identifier for the branch.</param>
		/// <param name="search">The type of search. Should implement <see cref="IBqlSearch"/> or <see cref="IBqlSelect"/></param>
		public CashAccountAttribute(Type branchID, Type search) : base(branchID, search)
		{
		}
	}
}
