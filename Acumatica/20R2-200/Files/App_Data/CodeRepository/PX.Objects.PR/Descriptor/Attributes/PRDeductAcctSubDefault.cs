using System;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRDeductAcctSubDefault
	{
		public class AcctListAttribute : CustomListAttribute
		{
			protected static Tuple<string, string>[] Pairs => new []
			{
				Pair(MaskDeductionCode, Messages.BenefitAndDeductionCode),
				Pair(MaskEmployee, Messages.PREmployee),
				Pair(MaskPayGroup, Messages.PRPayGroup),
			};

			public AcctListAttribute() : base(Pairs) { }
			protected AcctListAttribute(Tuple<string, string>[] pairs) : base(pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : CustomListAttribute
		{
			protected static Tuple<string, string>[] Pairs => new[]
			{
				Pair(MaskBranch, Messages.Branch),
				Pair(MaskEmployee, Messages.PREmployee),
				Pair(MaskPayGroup, Messages.PRPayGroup),
				Pair(MaskDeductionCode, Messages.BenefitAndDeductionCode),
			};

			public SubListAttribute() : base(Pairs) { }
			protected SubListAttribute(Tuple<string, string>[] pairs) : base(pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskBranch = GLAccountSubSource.Branch;
		public const string MaskEmployee = GLAccountSubSource.Employee;
		public const string MaskPayGroup = GLAccountSubSource.PayGroup;
		public const string MaskDeductionCode = GLAccountSubSource.DeductionCode;
	}

	public class PRBenefitExpenseAcctSubDefault : PRDeductAcctSubDefault
	{
		public new class AcctListAttribute : PRDeductAcctSubDefault.AcctListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRDeductAcctSubDefault.AcctListAttribute.Pairs.Union(new []
			{
				Pair(MaskEarningType, Messages.EarningType),
				Pair(MaskLaborItem, Messages.LaborItem),
			}).ToArray();

			public AcctListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public new class SubListAttribute : PRDeductAcctSubDefault.SubListAttribute
		{
			protected static new Tuple<string, string>[] Pairs => PRDeductAcctSubDefault.SubListAttribute.Pairs.Union(new []
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