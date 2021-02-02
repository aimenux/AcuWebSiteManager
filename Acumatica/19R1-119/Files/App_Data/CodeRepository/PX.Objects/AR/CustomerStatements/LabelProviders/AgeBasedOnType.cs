using System.Collections.Generic;

using PX.Objects.Common;

using PX.Data;

namespace PX.Objects.AR
{
	public class AgeBasedOnType : ILabelProvider
	{
		public const string DueDate = "U";
		public const string DocDate = "O";

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ DueDate, Messages.DueDate },
			{ DocDate, Messages.DocDate }
		};

		public class dueDate : PX.Data.BQL.BqlString.Constant<dueDate>
		{
			public dueDate() : base(DueDate) { }
		}

		public class docDate : PX.Data.BQL.BqlString.Constant<docDate>
		{
			public docDate() : base(DocDate) { }
		}
	}
}