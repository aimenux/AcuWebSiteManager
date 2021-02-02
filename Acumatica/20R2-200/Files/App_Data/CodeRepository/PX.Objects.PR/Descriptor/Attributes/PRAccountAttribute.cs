using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using System;

namespace PX.Objects.PR
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class PRAccountAttribute : AccountAttribute
	{
		protected Type _CodeField;
		protected Type _EmployeeField;
		public Type PayGroupField { get; set; }
		public bool CheckIfEmpty { get; set; } = true;

		protected abstract int? GetAccountValue(PXCache cache, object setupFieldValue, object row);
		protected abstract Type SetupField { get; }
		protected abstract int? GetAcctIDFromEmployee(PREmployee employee);
		protected abstract int? GetAcctIDFromPayGroup(PRPayGroup payGroup);

		protected virtual bool IsFieldRequired(PXCache cache, object row)
		{
			return true;
		}


		#region Constructor
		public PRAccountAttribute(Type codeField, Type employeeField, Type payGroupField) : this(null, codeField, employeeField, payGroupField)
		{
		}

		public PRAccountAttribute(Type branchID, Type codeField, Type employeeField, Type payGroupField) : base(branchID)
		{
			_CodeField = codeField;
			_EmployeeField = employeeField;
			PayGroupField = payGroupField;
		}

		public PRAccountAttribute(Type branchID, Type SearchType, Type codeField, Type employeeField, Type payGroupField) : base(branchID, SearchType)
		{
			_CodeField = codeField;
			_EmployeeField = employeeField;
			PayGroupField = payGroupField;
		}
		#endregion Constructor

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), FieldName, FieldDefaulting);
			sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), RowUpdated);
			sender.Graph.RowInserted.AddHandler(sender.GetItemType(), RowInserted);
			if (CheckIfEmpty)
			{
				sender.Graph.RowSelected.AddHandler(sender.GetItemType(), RowSelected);
				sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), FieldName, OptionalFieldVerifying);
			}
		}

		#region Events

		protected virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetError(sender, e.Row, FieldName, null);
				var setupFieldValue = GetCurrentValue(sender.Graph, SetupField);
				switch (setupFieldValue)
				{
					case PREarningsAcctSubDefault.MaskEmployee:
						var employeeID = sender.GetValue(e.Row, _EmployeeField.Name);
						var employee = (PREmployee)SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<P.AsInt>>.View.Select(sender.Graph, employeeID);
						if (employee == null)
						{
							return;
						}
						e.NewValue = GetAcctIDFromEmployee(employee);
						e.Cancel = true;
						break;
					case PREarningsAcctSubDefault.MaskPayGroup:
						var payGroupID = GetCurrentValue(sender.Graph, PayGroupField);
						var payGroup = (PRPayGroup)SelectFrom<PRPayGroup>.Where<PRPayGroup.payGroupID.IsEqual<P.AsString>>.View.Select(sender.Graph, payGroupID);
						if (payGroup == null)
						{
							return;
						}
						e.NewValue = GetAcctIDFromPayGroup(payGroup);
						e.Cancel = true;
						break;
					default:
						var value = GetAccountValue(sender, setupFieldValue, e.Row);
						if (value != null)
						{
							e.NewValue = value;
							e.Cancel = true;
						}
						break;
				}
			}
		}

		protected virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			RowInsertedUpdated(sender, e.Row, e.OldRow);
		}

		protected virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			RowInsertedUpdated(sender, e.Row, null);
		}

		protected virtual void RowInsertedUpdated(PXCache sender, object row, object oldRow)
		{
			if (row == null)
			{
				return;
			}

			if (sender.GetValue(row, _CodeField.Name) != sender.GetValue(oldRow, _CodeField.Name))
			{
				sender.SetDefaultExt(row, FieldName);
			}
		}

		protected virtual void RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (cache.GetValue(e.Row, _CodeField.Name) != null && cache.GetValue(e.Row, FieldName) == null && IsFieldRequired(cache, e.Row))
			{
				PXUIFieldAttribute.SetError(cache, e.Row, FieldName, PXMessages.LocalizeFormat(Messages.PostingValueNotFound, this.FieldName));
			}
			else
			{
				PXUIFieldAttribute.SetError(cache, e.Row, FieldName, null);
			}
		}

		public virtual void OptionalFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(cache, e);
			if (e.Row == null)
			{
				return;
			}

			if (e.NewValue == null && IsFieldRequired(cache, e.Row))
			{
				PXUIFieldAttribute.SetWarning(cache, e.Row, this.FieldName, PXMessages.LocalizeFormat(Messages.PostingValueNotFound, this.FieldName));
			}
		}

		#endregion Events


		protected object GetCurrentValue(PXGraph graph, Type type)
		{
			return graph.Caches[BqlCommand.GetItemType(type)].GetValue(graph.Caches[BqlCommand.GetItemType(type)].Current, type.Name);
		}
	}

	public abstract class PRExpenseAccountAttribte : PRAccountAttribute
	{
		protected Type _EarningCodeField;
		protected Type _LaborItemField;

		#region Constructors
		public PRExpenseAccountAttribte(Type branchID, Type codeField, Type employeeField, Type payGroupField, Type earningCodeField, Type laborItemField) :
			base(branchID, codeField, employeeField, payGroupField)
		{
			_EarningCodeField = earningCodeField;
			_LaborItemField = laborItemField;
		}

		public PRExpenseAccountAttribte(Type branchID, Type SearchType, Type codeField, Type employeeField, Type payGroupField, Type earningCodeField, Type laborItemField) :
			base(branchID, SearchType, codeField, employeeField, payGroupField)
		{
			_EarningCodeField = earningCodeField;
			_LaborItemField = laborItemField;
		}
		#endregion Constructors

		protected abstract Type EarningTypeAccountField { get; }
		protected abstract Type InventoryItemAccountField { get; }

		protected override int? GetAccountValue(PXCache cache, object setupFieldValue, object row)
		{
			switch (setupFieldValue)
			{
				case PREarningsAcctSubDefault.MaskEarningType:
					var earningTypeCD = cache.GetValue(row, _EarningCodeField.Name);
					var earningTypeView = new SelectFrom<EPEarningType>.Where<EPEarningType.typeCD.IsEqual<P.AsString>>.View(cache.Graph);
					EPEarningType earning = earningTypeView.SelectSingle(earningTypeCD);
					if (earning == null)
					{
						return null;
					}

					return earningTypeView.Cache.GetValue(earning, EarningTypeAccountField.Name) as int?;
				case PREarningsAcctSubDefault.MaskLaborItem:
					var laborItem = (InventoryItem)PXSelectorAttribute.Select(cache, row, _LaborItemField.Name);
					if (laborItem == null)
					{
						return null;
					}

					PXCache inventoryItemCache = cache.Graph.Caches[typeof(InventoryItem)];
					return inventoryItemCache.GetValue(laborItem, InventoryItemAccountField.Name) as int?;
			}

			return null;
		}

		protected override void RowInsertedUpdated(PXCache sender, object row, object oldRow)
		{
			if (row == null)
			{
				return;
			}

			object setupFieldValue = GetCurrentValue(sender.Graph, SetupField);
			if ((sender.GetValue(row, _EarningCodeField.Name) != sender.GetValue(oldRow, _EarningCodeField.Name) && setupFieldValue.Equals(PREarningsAcctSubDefault.MaskEarningType)) ||
				(sender.GetValue(row, _LaborItemField.Name) != sender.GetValue(oldRow, _LaborItemField.Name) && setupFieldValue.Equals(PREarningsAcctSubDefault.MaskLaborItem)))
			{
				sender.SetDefaultExt(row, FieldName);
			}
		}
	}

	public class EarningsAccountAttribute : PRExpenseAccountAttribte
	{
		#region Constructors
		public EarningsAccountAttribute(Type codeField, Type employeeField, Type payGroupField, Type earningCodeField, Type laborItemField) :
			this(null, codeField, employeeField, payGroupField, earningCodeField, laborItemField)
		{ }

		public EarningsAccountAttribute(Type branchID, Type codeField, Type employeeField, Type payGroupField, Type earningCodeField, Type laborItemField) :
			base(branchID, codeField, employeeField, payGroupField, earningCodeField, laborItemField)
		{ }

		public EarningsAccountAttribute(Type branchID, Type SearchType, Type codeField, Type employeeField, Type payGroupField, Type earningCodeField, Type laborItemField) :
			base(branchID, SearchType, codeField, employeeField, payGroupField, earningCodeField, laborItemField)
		{ }
		#endregion Constructors

		protected override Type SetupField => typeof(PRSetup.earningsAcctDefault);
		protected override int? GetAcctIDFromEmployee(PREmployee employee) => employee.EarningsAcctID;
		protected override int? GetAcctIDFromPayGroup(PRPayGroup payGroup) => payGroup.EarningsAcctID;
		protected override Type EarningTypeAccountField => typeof(PREarningType.earningsAcctID);
		protected override Type InventoryItemAccountField => typeof(PRxInventoryItem.earningsAcctID);
	}

	public class DedLiabilityAccountAttribute : PRAccountAttribute
	{
		protected Type _DeductionCodeIDField;

		#region Constructors
		public DedLiabilityAccountAttribute(Type codeField, Type employeeField, Type payGroupField, Type deductionCodeIDField) : this(null, codeField, employeeField, payGroupField, deductionCodeIDField)
		{
		}

		public DedLiabilityAccountAttribute(Type branchID, Type codeField, Type employeeField, Type payGroupField, Type deductionCodeIDField) : base(branchID, codeField, employeeField, payGroupField)
		{
			_DeductionCodeIDField = deductionCodeIDField;
		}

		public DedLiabilityAccountAttribute(Type branchID, Type SearchType, Type codeField, Type employeeField, Type payGroupField, Type deductionCodeIDField) : base(branchID, SearchType, codeField, employeeField, payGroupField)
		{
			_DeductionCodeIDField = deductionCodeIDField;
		}
		#endregion Constructors

		protected override Type SetupField => typeof(PRSetup.deductLiabilityAcctDefault);
		protected override int? GetAcctIDFromEmployee(PREmployee employee) => employee.DedLiabilityAcctID;
		protected override int? GetAcctIDFromPayGroup(PRPayGroup payGroup) => payGroup.DedLiabilityAcctID;

		protected override int? GetAccountValue(PXCache cache, object setupFieldValue, object row)
		{
			switch (setupFieldValue)
			{
				case PRDeductAcctSubDefault.MaskDeductionCode:
					var deductionCodeID = cache.GetValue(row, _DeductionCodeIDField.Name);
					var deductionBenefitCode = (PRDeductCode)SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View.Select(cache.Graph, deductionCodeID);
					return deductionBenefitCode?.DedLiabilityAcctID;
			}

			return null;
		}
	}

	public class BenExpenseAccountAttribute : PRExpenseAccountAttribte
	{
		protected Type _BenefitCodeIDField;

		public BenExpenseAccountAttribute(Type branchID, Type benefitCodeIDField, Type employeeField, Type payGroupField, Type earningCodeField, Type laborItemField) :
			base(branchID, benefitCodeIDField, employeeField, payGroupField, earningCodeField, laborItemField)
		{
			_BenefitCodeIDField = benefitCodeIDField;
		}

		protected override Type SetupField => typeof(PRSetup.benefitExpenseAcctDefault);
		protected override int? GetAcctIDFromEmployee(PREmployee employee) => employee.BenefitExpenseAcctID;
		protected override int? GetAcctIDFromPayGroup(PRPayGroup payGroup) => payGroup.BenefitExpenseAcctID;
		protected override Type EarningTypeAccountField => typeof(PREarningType.benefitExpenseAcctID);
		protected override Type InventoryItemAccountField => typeof(PRxInventoryItem.benefitExpenseAcctID);

		protected override int? GetAccountValue(PXCache cache, object setupFieldValue, object row)
		{
			switch (setupFieldValue)
			{
				case PRDeductAcctSubDefault.MaskDeductionCode:
					var deductionCodeID = cache.GetValue(row, _BenefitCodeIDField.Name);
					var deductionBenefitCode = (PRDeductCode)SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View.Select(cache.Graph, deductionCodeID);
					return deductionBenefitCode?.BenefitExpenseAcctID;
			}

			return base.GetAccountValue(cache, setupFieldValue, row);
		}

		protected override void RowInsertedUpdated(PXCache sender, object row, object oldRow)
		{
			if (row == null)
			{
				return;
			}

			object setupFieldValue = GetCurrentValue(sender.Graph, SetupField);
			if (sender.GetValue(row, _BenefitCodeIDField.Name) != sender.GetValue(oldRow, _BenefitCodeIDField.Name) && setupFieldValue.Equals(PRDeductAcctSubDefault.MaskDeductionCode))
			{
				sender.SetDefaultExt(row, FieldName);
			}

			base.RowInsertedUpdated(sender, row, oldRow);
		}
	}

	public class BenLiabilityAccountAttribute : PRAccountAttribute
	{
		protected Type _BenefitCodeIDField;
		protected Type _IsPayableBenefitField;

		#region Constructors
		public BenLiabilityAccountAttribute(Type branchID, Type codeField, Type employeeField, Type payGroupField, Type benefitCodeIDField, Type isPayableBenefitField) : base(branchID, codeField, employeeField, payGroupField)
		{
			_BenefitCodeIDField = benefitCodeIDField;
			_IsPayableBenefitField = isPayableBenefitField;
		}

		public BenLiabilityAccountAttribute(Type branchID, Type SearchType, Type codeField, Type employeeField, Type payGroupField, Type benefitCodeIDField, Type isPayableBenefitField) : base(branchID, SearchType, codeField, employeeField, payGroupField)
		{
			_BenefitCodeIDField = benefitCodeIDField;
			_IsPayableBenefitField = isPayableBenefitField;
		}
		#endregion Constructors

		protected override Type SetupField => typeof(PRSetup.benefitLiabilityAcctDefault);
		protected override int? GetAcctIDFromEmployee(PREmployee employee) => employee.BenefitLiabilityAcctID;
		protected override int? GetAcctIDFromPayGroup(PRPayGroup payGroup) => payGroup.BenefitLiabilityAcctID;

		protected override bool IsFieldRequired(PXCache cache, object row)
		{
			return cache.GetValue(row, _IsPayableBenefitField.Name) as bool? != true;
		}

		protected override int? GetAccountValue(PXCache cache, object setupFieldValue, object row)
		{
			switch (setupFieldValue)
			{
				case PRDeductAcctSubDefault.MaskDeductionCode:
					var deductionCodeID = cache.GetValue(row, _BenefitCodeIDField.Name);
					var deductionBenefitCode = (PRDeductCode)SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View.Select(cache.Graph, deductionCodeID);
					return deductionBenefitCode?.BenefitLiabilityAcctID;
			}

			return null;
		}
	}

	public class TaxExpenseAccountAttribute : PRExpenseAccountAttribte
	{
		protected Type _TaxCodeIDField;
		protected Type _TaxCategoryField;

		public TaxExpenseAccountAttribute(Type branchID, Type taxCodeIDField, Type employeeField, Type payGroupField, Type taxCategoryField, Type earningCodeField, Type laborItemField) :
			base(branchID, taxCodeIDField, employeeField, payGroupField, earningCodeField, laborItemField)
		{
			_TaxCodeIDField = taxCodeIDField;
			_TaxCategoryField = taxCategoryField;
		}

		protected override Type SetupField => typeof(PRSetup.taxExpenseAcctDefault);
		protected override int? GetAcctIDFromEmployee(PREmployee employee) => employee.PayrollTaxExpenseAcctID;
		protected override int? GetAcctIDFromPayGroup(PRPayGroup payGroup) => payGroup.TaxExpenseAcctID;
		protected override Type EarningTypeAccountField => typeof(PREarningType.taxExpenseAcctID);
		protected override Type InventoryItemAccountField => typeof(PRxInventoryItem.taxExpenseAcctID);

		protected override int? GetAccountValue(PXCache cache, object setupFieldValue, object row)
		{
			switch (setupFieldValue)
			{
				case PRTaxAcctSubDefault.MaskTaxCode:
					var taxCodeID = cache.GetValue(row, _TaxCodeIDField.Name);
					var taxCode = (PRTaxCode)SelectFrom<PRTaxCode>.Where<PRTaxCode.taxID.IsEqual<P.AsInt>>.View.Select(cache.Graph, taxCodeID);
					return taxCode?.ExpenseAcctID;
			}

			return base.GetAccountValue(cache, setupFieldValue, row);
		}

		#region Events
		protected override void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			//Only Employer Tax needs to default the Expense Account, otherwise it should stay null.
			if ((string)cache.GetValue(e.Row, _TaxCategoryField.Name) == TaxCategory.EmployerTax)
			{
				base.FieldDefaulting(cache, e);
			}
			else
			{
				e.Cancel = true;
			}
		}

		protected override void RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			if ((string)cache.GetValue(e.Row, _TaxCategoryField.Name) == TaxCategory.EmployerTax)
			{
				base.RowSelected(cache, e);
			}
		}

		public override void OptionalFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(cache, e);
			if (e.Row == null)
			{
				return;
			}

			if ((string)cache.GetValue(e.Row, _TaxCategoryField.Name) == TaxCategory.EmployerTax)
			{
				base.OptionalFieldVerifying(cache, e);
			}
		}

		protected override void RowInsertedUpdated(PXCache sender, object row, object oldRow)
		{
			if (row == null)
			{
				return;
			}

			object setupFieldValue = GetCurrentValue(sender.Graph, SetupField);
			if (sender.GetValue(row, _TaxCodeIDField.Name) != sender.GetValue(oldRow, _TaxCodeIDField.Name) && setupFieldValue.Equals(PRTaxAcctSubDefault.MaskTaxCode))
			{
				sender.SetDefaultExt(row, FieldName);
			}

			base.RowInsertedUpdated(sender, row, oldRow);
		}
		#endregion Events
	}

	public class TaxLiabilityAccountAttribute : PRAccountAttribute
	{
		protected Type _TaxCodeIDField;

		#region Constructors
		public TaxLiabilityAccountAttribute(Type codeField, Type employeeField, Type payGroupField, Type taxCodeIDField) : this(null, codeField, employeeField, payGroupField, taxCodeIDField)
		{
		}

		public TaxLiabilityAccountAttribute(Type branchID, Type codeField, Type employeeField, Type payGroupField, Type taxCodeIDField) : base(branchID, codeField, employeeField, payGroupField)
		{
			_TaxCodeIDField = taxCodeIDField;
		}

		public TaxLiabilityAccountAttribute(Type branchID, Type SearchType, Type codeField, Type employeeField, Type payGroupField, Type taxCodeIDField) : base(branchID, SearchType, codeField, employeeField, payGroupField)
		{
			_TaxCodeIDField = taxCodeIDField;
		}
		#endregion Constructors

		protected override Type SetupField => typeof(PRSetup.taxLiabilityAcctDefault);
		protected override int? GetAcctIDFromEmployee(PREmployee employee) => employee.PayrollTaxLiabilityAcctID;
		protected override int? GetAcctIDFromPayGroup(PRPayGroup payGroup) => payGroup.TaxLiabilityAcctID;

		protected override int? GetAccountValue(PXCache cache, object setupFieldValue, object row)
		{
			switch (setupFieldValue)
			{
				case PRTaxAcctSubDefault.MaskTaxCode:
					var taxCodeID = cache.GetValue(row, _TaxCodeIDField.Name);
					var taxCode = (PRTaxCode)SelectFrom<PRTaxCode>.Where<PRTaxCode.taxID.IsEqual<P.AsInt>>.View.Select(cache.Graph, taxCodeID);
					return taxCode?.LiabilityAcctID;
			}

			return null;
		}
	}
}