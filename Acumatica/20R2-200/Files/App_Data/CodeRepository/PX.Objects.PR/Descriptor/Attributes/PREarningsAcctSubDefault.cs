using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	public class PREarningsAcctSubDefault
	{
		public class AcctListAttribute : CustomListAttribute
		{
			private static Tuple<string, string>[] Pairs => new[]
			{
				Pair(MaskEarningType, PR.Messages.EarningType),
				Pair(MaskEmployee,  PR.Messages.PREmployee),
				Pair(MaskPayGroup,  PR.Messages.PRPayGroup),
				Pair(MaskLaborItem,  PR.Messages.LaborItem),
			};

			public AcctListAttribute() : base(Pairs) { }

			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : CustomListAttribute
		{
			private static Tuple<string, string>[] Pairs => new[]
			{
				Pair(MaskBranch,    PR.Messages.Branch),
				Pair(MaskEmployee,  PR.Messages.PREmployee),
				Pair(MaskPayGroup,  PR.Messages.PRPayGroup),
				Pair(MaskEarningType, PR.Messages.EarningType),
				Pair(MaskLaborItem,  PR.Messages.LaborItem),
			};

			public SubListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskBranch = GLAccountSubSource.Branch;
		public const string MaskEmployee = GLAccountSubSource.Employee;
		public const string MaskPayGroup = GLAccountSubSource.PayGroup;
		public const string MaskEarningType = GLAccountSubSource.EarningType;
		public const string MaskLaborItem = GLAccountSubSource.LaborItem;
	}

	public abstract class CustomListAttribute : PXStringListAttribute
	{
		public string[] AllowedValues => _AllowedValues;
		public string[] AllowedLabels => _AllowedLabels;

		protected CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) { }

		protected abstract Tuple<string, string>[] GetPairs();

		public override void CacheAttached(PXCache sender)
		{
			var pairs = GetPairs();
			_AllowedValues = pairs.Select(t => t.Item1).ToArray();
			_AllowedLabels = pairs.Select(t => t.Item2).ToArray();
			_NeutralAllowedLabels = _AllowedLabels;

			base.CacheAttached(sender);
		}
	}
}
