using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CA
{
    public class CashAccountScalarAttribute : CashAccountBaseAttribute
	{
		public CashAccountScalarAttribute() : base() { }

		public CashAccountScalarAttribute(Type branchID, Type searchType) : base(branchID, searchType) { }
    }
}
