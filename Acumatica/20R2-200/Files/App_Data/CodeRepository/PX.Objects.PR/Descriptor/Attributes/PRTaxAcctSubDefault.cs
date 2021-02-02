using System;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRTaxAcctSubDefault
	{
		public class AcctListAttribute : CustomListAttribute
		{
			protected static Tuple<string, string>[] Pairs => new []
			{
				Pair(MaskTaxCode, Messages.TaxCode),
				Pair(MaskEmployee, Messages.PREmployee),
				Pair(MaskPayGroup, Messages.PRPayGroup),
			};

			public AcctListAttribute() : base(Pairs) { }
			protected AcctListAttribute(Tuple<string, string>[] pairs) : base(pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : CustomListAttribute
		{
			protected static Tuple<string, string>[] Pairs => new []
			{
				Pair(MaskBranch, Messages.Branch),
				Pair(MaskEmployee, Messages.PREmployee),
				Pair(MaskPayGroup, Messages.PRPayGroup),
				Pair(MaskTaxCode, Messages.TaxCode),
			};

			public SubListAttribute() : base(Pairs) { }
			protected SubListAttribute(Tuple<string, string>[] pairs) : base(pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskBranch = GLAccountSubSource.Branch;
		public const string MaskEmployee = GLAccountSubSource.Employee;
		public const string MaskPayGroup = GLAccountSubSource.PayGroup;
		public const string MaskTaxCode = GLAccountSubSource.TaxCode;
	}

	public class PRTaxExpenseAcctSubDefault : PRTaxAcctSubDefault
	{
		public new class AcctListAttribute : PRTaxAcctSubDefault.AcctListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRTaxAcctSubDefault.AcctListAttribute.Pairs.Union(new[]
			{
				Pair(MaskEarningType, Messages.EarningType),
				Pair(MaskLaborItem, Messages.LaborItem),
			}).ToArray();

			public AcctListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public new class SubListAttribute : PRTaxAcctSubDefault.SubListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRTaxAcctSubDefault.SubListAttribute.Pairs.Union(new[]
			{
				Pair(MaskEarningType, Messages.EarningType),
				Pair(MaskLaborItem, Messages.LaborItem),
			}).ToArray();

			public SubListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskEarningType = "T";
		public const string MaskLaborItem = "L";

		public class maskEarningType : PX.Data.BQL.BqlString.Constant<maskEarningType>
		{
			public maskEarningType() : base(MaskEarningType) { }
		}

		public class maskLaborItem : PX.Data.BQL.BqlString.Constant<maskLaborItem>
		{
			public maskLaborItem() : base(MaskLaborItem) { }
		}
	}
}