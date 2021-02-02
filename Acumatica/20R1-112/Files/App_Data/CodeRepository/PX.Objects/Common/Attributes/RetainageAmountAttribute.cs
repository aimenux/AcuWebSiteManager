using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CM;


namespace PX.Objects.Common
{
	/// Provides a special formula for the retainage amount calculation 
	/// depending on related retainage percent field. 
	/// The attribute will do next things: 
	/// correctly default retainage amount on the FieldDefaulting event; 
	/// calculate retainage amount value once any dependent field has been updated;
	/// verify retainage amount value and raise an error if it is incorrect.
	/// Note this attribute will affect only retainage amount value,
	/// for the retainage percent field use <see cref="RetainagePercentAttribute"/>.
	/// <param name="curyInfoIDField">CurrencyInfo field for the current line.</param>
	/// <param name="retainedAmtFormula">final amount for which retainage
	/// calculation should be applied. Expression or field may be used. Note, all
	/// included fields should NOT be surrounded by <see cref="Current{Field}"/> BQL 
	/// statement.</param>
	/// <param name="curyRetainageAmtField">field to which the attribute belongs
	/// (Presented in the currency of the document, see <see cref="APRegister.CuryID"/>).</param> 
	/// <param name="retainageAmtField">field to which the attribute belongs.
	/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>).</param> 
	/// <param name="retainagePctField">retainage percent field.</param> 
	/// <example>
	/// [DBRetainageAmount(
	///    typeof(APTran.curyInfoID), 
	///     typeof(Sub<APTran.curyLineAmt, APTran.curyDiscAmt>),
	///     typeof(APTran.curyRetainageAmt),
	///     typeof(APTran.retainageAmt),
	///     typeof(APTran.retainagePct))]
	/// </example>
	/// </summary>
	[PXUIField(
		DisplayName = "Retainage Amount",
		Visibility = PXUIVisibility.Visible,
		FieldClass = nameof(FeaturesSet.Retainage))]
	public class RetainageAmountAttribute : PXAggregateAttribute
	{
		protected int _UIAttrIndex = -1;
		protected PXUIFieldAttribute UIAttribute => _UIAttrIndex == -1 ? null : (PXUIFieldAttribute)_Attributes[_UIAttrIndex];

		protected Type formulaType;
		protected Type verifyType;

		public RetainageAmountAttribute(
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainagePctField)
			: base()
		{
			Initialize();

			formulaType = BqlCommand.Compose(
				typeof(Mult<,>), retainedAmtFormula, typeof(Div<,>), typeof(IsNull<,>), retainagePctField, typeof(decimal0), typeof(decimal100));

			verifyType = BqlCommand.Compose(
				typeof(Where<,,>), retainedAmtFormula, typeof(GreaterEqual<decimal0>),
					typeof(And<,,>), curyRetainageAmtField, typeof(GreaterEqual<decimal0>),
					typeof(And<,,>), curyRetainageAmtField, typeof(LessEqual<>), retainedAmtFormula,
					typeof(Or<,,>), retainedAmtFormula, typeof(LessEqual<decimal0>),
					typeof(And<,,>), curyRetainageAmtField, typeof(LessEqual<decimal0>),
					typeof(And<,>), curyRetainageAmtField, typeof(GreaterEqual<>), retainedAmtFormula);
		}

		protected virtual void Initialize()
		{
			_UIAttrIndex = -1;

			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXUIFieldAttribute)
				{
					_UIAttrIndex = _Attributes.IndexOf(attr);
				}
			}
		}

		public string DisplayName
		{
			get
			{
				return UIAttribute?.DisplayName;
			}
			set
			{
				if (UIAttribute != null)
				{
					UIAttribute.DisplayName = value;
				}
			}
		}
	}

	public class DBRetainageAmountAttribute : RetainageAmountAttribute
	{
		public DBRetainageAmountAttribute(
			Type curyInfoIDField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainageAmtField,
			Type retainagePctField)
			: base(retainedAmtFormula, curyRetainageAmtField, retainagePctField)
		{
			_Attributes.Add(new PXDBCurrencyAttribute(curyInfoIDField, retainageAmtField));
			_Attributes.Add(new PXDefaultAttribute(TypeCode.Decimal, "0.0") { PersistingCheck = PXPersistingCheck.Nothing });
			_Attributes.Add(new PXFormulaAttribute(formulaType));
			_Attributes.Add(new PXUIVerifyAttribute(verifyType, PXErrorLevel.Error, AP.Messages.IncorrectRetainageAmount));
		}
	}

	public class UnboundRetainageAmountAttribute : RetainageAmountAttribute
	{
		public UnboundRetainageAmountAttribute(
			Type curyInfoIDField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainageAmtField,
			Type retainagePctField)
			: base(retainedAmtFormula, curyRetainageAmtField, retainagePctField)
		{
			_Attributes.Add(new PXCurrencyAttribute(curyInfoIDField, retainageAmtField));
			_Attributes.Add(new PXUnboundDefaultAttribute(TypeCode.Decimal, "0.0"));
			_Attributes.Add(new PXFormulaAttribute(formulaType));
			_Attributes.Add(new PXUIVerifyAttribute(verifyType, PXErrorLevel.Error, AP.Messages.IncorrectRetainageAmount, true));
		}
	}
}