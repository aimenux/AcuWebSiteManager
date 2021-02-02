using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.CS;

namespace PX.Objects.AR.BQL
{
	public class HasNonZeroGLAmount<TAdjustment> : IBqlUnary
		where TAdjustment : ARAdjust
	{
		private static IBqlCreator _where;

		private static IBqlCreator Where
		{
			get
			{
				if (_where != null) return _where;

				Dictionary<string, Type> nestedTypes = typeof(TAdjustment)
					.GetNestedTypes()
					.ToDictionary(nestedType => nestedType.Name);

				Type writeOffAmountField1 = nestedTypes[nameof(ARAdjust.adjWOAmt)];
				Type writeOffAmountField2 = nestedTypes[nameof(ARAdjust.curyAdjgWOAmt)];
				Type writeOffAmountField3 = nestedTypes[nameof(ARAdjust.curyAdjdWOAmt)];
				Type cashDiscountAmountField1 = nestedTypes[nameof(ARAdjust.adjDiscAmt)];
				Type cashDiscountAmountField2 = nestedTypes[nameof(ARAdjust.curyAdjgDiscAmt)];
				Type cashDiscountAmountField3 = nestedTypes[nameof(ARAdjust.curyAdjdDiscAmt)];
				Type rgolAmountField = nestedTypes[nameof(ARAdjust.rGOLAmt)];

				Type whereType = BqlCommand.Compose(
					typeof(Where<,,>),
						writeOffAmountField1, typeof(NotEqual<>), typeof(decimal0),
						typeof(Or<,,>), writeOffAmountField2, typeof(NotEqual<>), typeof(decimal0),
						typeof(Or<,,>), writeOffAmountField3, typeof(NotEqual<>), typeof(decimal0),
						typeof(Or<,,>), cashDiscountAmountField1, typeof(NotEqual<>), typeof(decimal0),
						typeof(Or<,,>), cashDiscountAmountField2, typeof(NotEqual<>), typeof(decimal0),
						typeof(Or<,,>), cashDiscountAmountField3, typeof(NotEqual<>), typeof(decimal0),
						typeof(Or<,>), rgolAmountField, typeof(NotEqual<>), typeof(decimal0));

				_where = Activator.CreateInstance(whereType) as IBqlUnary;

				return _where;
			}
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> Where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			=> Where.Verify(cache, item, pars, ref result, ref value);
	}
}
