using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.PO
{
	public class POLandedCostDocType : ILabelProvider
	{
		public const string LandedCost = "L";
		public const string Correction = "C";
		public const string Reversal = "R";

		protected static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ LandedCost, "Landed Cost" },
			{ Correction, "Correction" },
			{ Reversal, "Reversal" }
		};

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public class ListAttribute : LabelListAttribute
		{
			public ListAttribute() : base(_valueLabelPairs)
			{ }
		}

		public class landedCost : PX.Data.BQL.BqlString.Constant<landedCost>
		{
			public landedCost() : base(LandedCost) { }
		}

		public class correction : PX.Data.BQL.BqlString.Constant<correction>
		{
			public correction() : base(Correction) { }
		}

		public class reversal : PX.Data.BQL.BqlString.Constant<reversal>
		{
			public reversal() : base(Reversal) { }
		}
	}
}
