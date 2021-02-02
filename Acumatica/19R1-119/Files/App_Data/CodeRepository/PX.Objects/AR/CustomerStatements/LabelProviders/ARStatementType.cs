using System;
using System.Collections.Generic;

using PX.Data;

using PX.Objects.Common;

namespace PX.Objects.AR
{
	public class ARStatementType : ILabelProvider
	{
		public const string OpenItem = "O";
		public const string BalanceBroughtForward = "B";

		public const string CS_OPEN_ITEM = OpenItem;
		public const string CS_BALANCE_BROUGHT_FORWARD = BalanceBroughtForward;

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ OpenItem, Messages.OpenItem },
			{ BalanceBroughtForward, Messages.BalanceBroughtForward },
		};

		public class balanceBroughtForward : PX.Data.BQL.BqlString.Constant<balanceBroughtForward>
		{
			public balanceBroughtForward() : base(BalanceBroughtForward) { }
		}

		public class openItem : PX.Data.BQL.BqlString.Constant<openItem>
		{
			public openItem() : base(OpenItem) { }
		}

		public class balanceFoward : balanceBroughtForward { }
	}

	public class StatementTypes : ARStatementType
	{ }
}
