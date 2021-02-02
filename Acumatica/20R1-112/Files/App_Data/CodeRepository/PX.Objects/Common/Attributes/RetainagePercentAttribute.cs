using System;
using PX.Data;
using PX.Objects.CS;


namespace PX.Objects.Common
{
	/// <summary>
	/// Provides a special formula for the retainage percent calculation 
	/// depending on related retainage amount field. 
	/// The attribute will do next things: 
	/// correctly default retainage percent on the FieldDefaulting event; 
	/// calculate retainage percent value only in the case when retainage 
	/// amount has been changed from the UI;
	/// verify retainage percent value and raise an error if it is incorrect.
	/// Note this attribute will affect only retainage percent value,
	/// for the retainage amount field use <see cref="RetainageAmountAttribute"/>.
	/// <param name="retainageApplyField">field from the parent entity, 
	/// indicating retainage calculation should be applied.</param>
	/// <param name="defRetainagePctField">field from the parent entity, 
	/// from which retainage percent should be defaulted.</param>
	/// <param name="retainedAmtFormula">final amount for which retainage
	/// calculation should be applied. Expression or field may be used. Note, all
	/// included fields should be surrounded by <see cref="Current{Field}"/> BQL 
	/// statement.</param>
	/// <param name="curyRetainageAmtField">retainage amount field for which
	/// retainage calculation should be applied in the case when retainage percent
	/// has been changed.</param> 
	/// <param name="retainagePctField">field to which the attribute belongs.</param> 
	/// <example>
	/// [DBRetainagePercent(
	///     typeof(APInvoice.retainageApply), 
	///     typeof(APInvoice.defRetainagePct), 
	///     typeof(Sub<Current<APTran.curyLineAmt>, Current<APTran.curyDiscAmt>>),
	///     typeof(APTran.curyRetainageAmt))]
	/// </example>
	/// </summary>
	[PXUIField(
		DisplayName = "Retainage Percent",
		Visibility = PXUIVisibility.Visible,
		FieldClass = nameof(FeaturesSet.Retainage))]
	public class RetainagePercentAttribute : PXAggregateAttribute
	{
		protected int _UIAttrIndex = -1;
		protected PXUIFieldAttribute UIAttribute => _UIAttrIndex == -1 ? null : (PXUIFieldAttribute)_Attributes[_UIAttrIndex];

		protected Type defaultType;
		protected Type verifyType;
		protected Type formulaType;

		public RetainagePercentAttribute(
			Type retainageApplyField,
			Type defRetainagePctField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainagePctField)
			: base()
		{
			Initialize();

			Type retainageApplyType = typeof(IConstant).IsAssignableFrom(retainageApplyField)
				? retainageApplyField
				: BqlCommand.Compose(typeof(Current<>), retainageApplyField);

			Type defRetainagePctType = typeof(IConstant).IsAssignableFrom(defRetainagePctField)
				? defRetainagePctField
				: BqlCommand.Compose(typeof(Current<>), defRetainagePctField);

			defaultType = BqlCommand.Compose(
				typeof(Switch<,>), typeof(Case<,>), typeof(Where<,>), retainageApplyType, typeof(Equal<True>),
					defRetainagePctType,
					typeof(decimal0));

			formulaType = BqlCommand.Compose(
				typeof(Switch<,>), typeof(Case<,>), typeof(Where<,>), retainageApplyType, typeof(Equal<True>),
					typeof(ExternalValue<>), typeof(Switch<,>), typeof(Case<,>), typeof(Where<,>), retainedAmtFormula, typeof(NotEqual<decimal0>),
						typeof(Mult<,>), typeof(Div<,>), curyRetainageAmtField, retainedAmtFormula, typeof(decimal100),
						typeof(decimal0),
					typeof(decimal0));

			verifyType = BqlCommand.Compose(
				typeof(Where<,,>), retainagePctField, typeof(LessEqual<decimal100>),
					typeof(And<,>), retainagePctField, typeof(GreaterEqual<decimal0>));
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

	public class DBRetainagePercentAttribute : RetainagePercentAttribute
	{
		public DBRetainagePercentAttribute(
			Type retainageApplyField,
			Type defRetainagePctField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainagePctField)
			: base(retainageApplyField, defRetainagePctField, retainedAmtFormula, curyRetainageAmtField, retainagePctField)
		{
			_Attributes.Add(new PXDBDecimalAttribute(6) { MinValue = 0, MaxValue = 100 });
			_Attributes.Add(new PXDefaultAttribute(TypeCode.Decimal, "0.0", defaultType) { PersistingCheck = PXPersistingCheck.Nothing });
			_Attributes.Add(new PXUIVerifyAttribute(verifyType, PXErrorLevel.Error, AP.Messages.IncorrectRetainagePercent));
			_Attributes.Add(new UndefaultFormulaAttribute(formulaType));
		}
	}

	public class UnboundRetainagePercentAttribute : RetainagePercentAttribute
	{
		public UnboundRetainagePercentAttribute(
			Type retainageApplyField,
			Type defRetainagePctField,
			Type retainedAmtFormula,
			Type curyRetainageAmtField,
			Type retainagePctField)
			: base(retainageApplyField, defRetainagePctField, retainedAmtFormula, curyRetainageAmtField, retainagePctField)
		{
			_Attributes.Add(new PXDecimalAttribute(6) { MinValue = 0, MaxValue = 100 });
			_Attributes.Add(new PXUnboundDefaultAttribute(TypeCode.Decimal, "0.0", defaultType));
			_Attributes.Add(new PXUIVerifyAttribute(verifyType, PXErrorLevel.Error, AP.Messages.IncorrectRetainagePercent, true));
			_Attributes.Add(new UndefaultFormulaAttribute(formulaType));
		}
	}

	public class UndefaultFormulaAttribute : PXFormulaAttribute
	{
		public UndefaultFormulaAttribute(Type formulaType)
			: base(formulaType)
		{
		}

		public override void FormulaDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue(e.Row, _FieldName);
		}
	}
}