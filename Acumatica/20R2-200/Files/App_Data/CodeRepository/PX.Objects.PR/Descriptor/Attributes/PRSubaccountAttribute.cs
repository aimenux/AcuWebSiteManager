using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public abstract class PRSubaccountAttribute : SubAccountAttribute
	{
		public bool CheckIfEmpty { get; set; } = true;
		public Type EmployeeIDField { get; set; } = typeof(PRPayment.employeeID);
		public Type PayGroupIDField { get; set; } = typeof(PRPayment.payGroupID);
		protected abstract Dictionary<Type, string> DefaultingDependencies { get; set; }
		protected abstract Type SetupField { get; }

		#region Data view classes
		protected class LocationAddress : SelectFrom<LocationExtAddress>
				.InnerJoin<GL.Branch>.On<GL.Branch.bAccountID.IsEqual<LocationExtAddress.bAccountID>>
				.Where<GL.Branch.branchID.IsEqual<P.AsInt>>.View
		{
			public LocationAddress(PXGraph graph) : base(graph)
			{
			}
		}

		protected class Employee : SelectFrom<PREmployee>
				.Where<PREmployee.bAccountID.IsEqual<P.AsInt>>.View
		{
			public Employee(PXGraph graph) : base(graph)
			{
			}
		}

		protected class PayGroup : SelectFrom<PRPayGroup>
				.Where<PRPayGroup.payGroupID.IsEqual<P.AsString>>.View
		{
			public PayGroup(PXGraph graph) : base(graph)
			{
			}
		}
		#endregion Data view classes

		#region Constructors
		public PRSubaccountAttribute()
		{
		}

		public PRSubaccountAttribute(Type AccountType) : base(AccountType)
		{
		}

		public PRSubaccountAttribute(Type AccountType, Type BranchType, bool AccountAndBranchRequired = false) : base(AccountType, BranchType, AccountAndBranchRequired)
		{
		}
		#endregion Constructors

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), FieldName, FieldDefaulting);
			sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), RowUpdated);
			sender.Graph.RowInserted.AddHandler(sender.GetItemType(), RowInserted);
			if (CheckIfEmpty)
			{
				sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), FieldName, FieldVerifying);
				sender.Graph.RowSelected.AddHandler(sender.GetItemType(), RowSelected); 
			}
		}

		#region Helpers

		protected static object GetValue<Field>(PXGraph graph, object data)
			where Field : IBqlField
		{
			return GetValue(graph, data, typeof(Field));
		}

		protected static object GetValue(PXGraph graph, object data, Type field)
		{
			return graph.Caches[BqlCommand.GetItemType(field)].GetValue(data, field.Name);
		}

		protected static object GetCurrentValue(PXGraph graph, Type type)
		{
			return graph.Caches[BqlCommand.GetItemType(type)].GetValue(graph.Caches[BqlCommand.GetItemType(type)].Current, type.Name);
		}

		protected void SetDefaultValue(PXCache cache, object row)
		{
			try
			{
				cache.SetDefaultExt(row, FieldName);
			}
			catch (PXSetPropertyException)
			{
				cache.SetValue(row, FieldName, null);
			}
		}

		protected bool HasImpactOnSubDefault(PXCache cache, string fieldName, string mask, object rowA, object rowB)
		{
			// != operator doesn't compare correctly, we need object.Equals
			return !object.Equals(cache.GetValue(rowA, fieldName), cache.GetValue(rowB, fieldName)) && SubMaskContainsValue(cache, mask);
		}

		protected bool SubMaskContainsValue(PXCache cache, string compareValue)
		{
			PRSetup setup = cache.Graph.Caches[typeof(PRSetup)].Current as PRSetup ?? new SelectFrom<PRSetup>.View(cache.Graph).SelectSingle();
			PXCache setupCache = cache.Graph.Caches[typeof(PRSetup)];
			string subMask = setupCache.GetValue(setup, SetupField.Name) as string;
			return PRSetupMaint.SubMaskContainsValue(cache.Graph.Caches[typeof(PRSetup)], setup, SetupField, subMask, compareValue);
		}

		#endregion Helpers

		#region Events
		public override void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(cache, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetError(cache, e.Row, FieldName, null);
				var maskSources = GetMaskSources(cache, e.Row);
				object value = null;
				try
				{
					var setup = (PRSetup)SelectFrom<PRSetup>.View.Select(cache.Graph);
					if (setup != null)
					{
						value = MakeSub(cache.Graph, setup, maskSources);
						cache.RaiseFieldUpdating(FieldName, e.Row, ref value); 
					}
				}
				catch (PXMaskArgumentException)
				{
					value = null;
				}
				catch (PXSetPropertyException)
				{
					value = null;
				}

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

		protected virtual void RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (cache.GetValue(e.Row, FieldName) == null && IsFieldRequired(cache, e.Row))
			{
				PXUIFieldAttribute.SetError(cache, e.Row, this.FieldName, PXMessages.LocalizeFormat(Messages.PostingValueNotFound, this.FieldName));
			}
			else
			{
				PXUIFieldAttribute.SetError(cache, e.Row, FieldName, null);
			}
		}

		protected virtual void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (e.NewValue == null && IsFieldRequired(cache, e.Row))
			{
				PXUIFieldAttribute.SetWarning(cache, e.Row, this.FieldName, PXMessages.LocalizeFormat(Messages.PostingValueNotFound, this.FieldName));
			}
		}

		protected virtual void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			RowInsertedUpdated(cache, e.Row, e.OldRow);
		}

		protected virtual void RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			RowInsertedUpdated(cache, e.Row, null);
		}

		protected virtual void RowInsertedUpdated(PXCache cache, object row, object oldRow)
		{
			if (row == null)
			{
				return;
			}

			if(DefaultingDependencies.Any(pair => HasImpactOnSubDefault(cache, pair.Key.Name, pair.Value, row, oldRow)))
			{
				SetDefaultValue(cache, row);
			}
		}

		#endregion Events

		protected abstract Type[] MaskFieldTypes { get; }
		public abstract object MakeSub(PXGraph graph, PRSetup setup, IEnumerable<object> maskSources);
		protected abstract IEnumerable<object> GetMaskSources(PXCache cache, object row);

		protected virtual bool IsFieldRequired(PXCache cache, object row)
		{
			return true;
		}
	}

	public abstract class ExpenseSubAccountAttribute : PRSubaccountAttribute
	{
		protected Type _EarningTypeField;
		protected Type _LaborItemField;
		protected Type _BranchField;

		public ExpenseSubAccountAttribute(Type accountType, Type branchType, Type earningTypeField, Type laborItemField, bool accountAndBranchRequired = false) : 
			base(accountType, branchType, accountAndBranchRequired)
		{
			_BranchField = branchType;
			_EarningTypeField = earningTypeField;
			_LaborItemField = laborItemField;
		}

		protected ExpenseMaskSources GetCommonMaskSources(PXCache cache, object row)
		{
			var locationAddress = (LocationExtAddress)LocationAddress.Select(cache.Graph, cache.GetValue(row, _BranchField.Name));
			var employee = (PREmployee)Employee.Select(cache.Graph, (int?)GetCurrentValue(cache.Graph, EmployeeIDField));
			var payGroup = (PRPayGroup)PayGroup.Select(cache.Graph, (string)GetCurrentValue(cache.Graph, PayGroupIDField));
			var earningType = PXSelectorAttribute.Select(cache, row, _EarningTypeField.Name);
			var laborItem = PXSelectorAttribute.Select(cache, row, _LaborItemField.Name);

			return new ExpenseMaskSources()
			{
				branchSubID = GetValue<PRLocationExtAddress.cMPPayrollSubID>(cache.Graph, locationAddress),
				employeeSubID = GetValue(cache.Graph, employee, EmployeeSubIDField),
				payGroupSubID = GetValue(cache.Graph, payGroup, PayGroupSubIDField),
				earningTypeSubID = GetValue(cache.Graph, earningType, EarningTypeSubIDField),
				laborItemSubID = GetValue(cache.Graph, laborItem, LaborItemSubIDField)
			};
		}

		protected Type BranchSubIDAttribute => typeof(PRLocationExtAddress.cMPPayrollSubID);
		protected abstract Type EmployeeSubIDField { get; }
		protected abstract Type PayGroupSubIDField { get; }
		protected abstract Type EarningTypeSubIDField { get; }
		protected abstract Type LaborItemSubIDField { get; }

		protected struct ExpenseMaskSources
		{
			public object branchSubID;
			public object employeeSubID;
			public object payGroupSubID;
			public object benefitSubID;
			public object earningTypeSubID;
			public object laborItemSubID;
		}
	}

	public class EarningSubAccountAttribute : ExpenseSubAccountAttribute
	{
		protected override Dictionary<Type, string> DefaultingDependencies { get; set; } = new Dictionary<Type, string>()
		{
			{ typeof(PREarningDetail.branchID), PREarningsAcctSubDefault.MaskBranch },
			{ typeof(PREarningDetail.employeeID), PREarningsAcctSubDefault.MaskEmployee },
			{ typeof(PREarningDetail.typeCD), PREarningsAcctSubDefault.MaskEarningType },
			{ typeof(PREarningDetail.labourItemID), PREarningsAcctSubDefault.MaskLaborItem},
		};

		public EarningSubAccountAttribute(Type AccountType, Type BranchType, bool AccountAndBranchRequired = false) : 
			base(AccountType, BranchType, typeof(PREarningDetail.typeCD), typeof(PREarningDetail.labourItemID), AccountAndBranchRequired)
		{
		}

		protected override Type[] MaskFieldTypes => new[]
		{
			BranchSubIDAttribute,
			EmployeeSubIDField,
			PayGroupSubIDField,
			EarningTypeSubIDField,
			LaborItemSubIDField
		};

		protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
		{
			ExpenseMaskSources sources = GetCommonMaskSources(cache, row);

			//Be careful, this array order needs to match with PREarningsAcctSubDefault.SubListAttribute (used in PREarningsSubAccountMaskAttribute)
			return new object[] { sources.branchSubID, sources.employeeSubID, sources.payGroupSubID, sources.earningTypeSubID, sources.laborItemSubID };
		}

		public override object MakeSub(PXGraph graph, PRSetup setup, IEnumerable<object> maskSources)
		{
			return PREarningsSubAccountMaskAttribute.MakeSub<PRSetup.earningsSubMask>(graph, setup.EarningsSubMask, maskSources.ToArray(), MaskFieldTypes);
		}

		protected override Type EmployeeSubIDField => typeof(PREmployee.earningsSubID);
		protected override Type PayGroupSubIDField => typeof(PRPayGroup.earningsSubID);
		protected override Type EarningTypeSubIDField => typeof(PREarningType.earningsSubID);
		protected override Type LaborItemSubIDField => typeof(PRxInventoryItem.earningsSubID);
		protected override Type SetupField => typeof(PRSetup.earningsSubMask);
	}

	public class DedLiabilitySubAccountAttribute : PRSubaccountAttribute
	{
		protected override Dictionary<Type, string> DefaultingDependencies { get; set; } = new Dictionary<Type, string>()
		{
			{ typeof(PRDeductionDetail.branchID), PRDeductAcctSubDefault.MaskBranch },
			{ typeof(PRDeductionDetail.employeeID), PRDeductAcctSubDefault.MaskEmployee},
			{ typeof(PRDeductionDetail.codeID), PRDeductAcctSubDefault.MaskDeductionCode },
		};

		#region Constructors
		public DedLiabilitySubAccountAttribute()
		{
		}

		public DedLiabilitySubAccountAttribute(Type AccountType) : base(AccountType)
		{
		}

		public DedLiabilitySubAccountAttribute(Type AccountType, Type BranchType, bool AccountAndBranchRequired = false) : base(AccountType, BranchType, AccountAndBranchRequired)
		{
		}
		#endregion Constructors

		protected override Type[] MaskFieldTypes { get; } = {
			typeof(PRLocationExtAddress.cMPPayrollSubID),
			typeof(PREmployee.dedLiabilitySubID),
			typeof(PRPayGroup.dedLiabilitySubID),
			typeof(PRDeductCode.dedLiabilitySubID),
		};

		protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
		{
			var line = row as PRDeductionDetail;
			var locationAddress = (LocationExtAddress)LocationAddress.Select(cache.Graph, line.BranchID);
			var employee = (PREmployee)Employee.Select(cache.Graph, line.EmployeeID);
			var payGroup = (PRPayGroup)PayGroup.Select(cache.Graph, (string)GetCurrentValue(cache.Graph, PayGroupIDField));
			var deductionCode = PXSelectorAttribute.Select<PRDeductionDetail.codeID>(cache, line);

			var branchSubID = GetValue<PRLocationExtAddress.cMPPayrollSubID>(cache.Graph, locationAddress);
			var employeeSubID = GetValue<PREmployee.dedLiabilitySubID>(cache.Graph, employee);
			var payGroupSubID = GetValue<PRPayGroup.dedLiabilitySubID>(cache.Graph, payGroup);
			var dedLiabilitySubID = GetValue<PRDeductCode.dedLiabilitySubID>(cache.Graph, deductionCode);

			//Be careful, this array order needs to match with PRDeductAcctSubDefault.SubListAttribute (used in PRDeductSubAccountMaskAttribute)
			return new object[] { branchSubID, employeeSubID, payGroupSubID, dedLiabilitySubID };
		}

		public override object MakeSub(PXGraph graph, PRSetup setup, IEnumerable<object> maskSources)
		{
			return PRDeductSubAccountMaskAttribute.MakeSub<PRSetup.deductLiabilitySubMask>(graph, setup.DeductLiabilitySubMask, maskSources.ToArray(), MaskFieldTypes);
		}

		protected override Type SetupField => typeof(PRSetup.deductLiabilitySubMask);
	}

	public class BenExpenseSubAccountAttribute : ExpenseSubAccountAttribute
	{
		protected override Dictionary<Type, string> DefaultingDependencies { get; set; } = new Dictionary<Type, string>()
		{
			{ typeof(PRBenefitDetail.branchID), PRBenefitExpenseAcctSubDefault.MaskBranch },
			{ typeof(PRBenefitDetail.employeeID), PRBenefitExpenseAcctSubDefault.MaskEmployee},
			{ typeof(PRBenefitDetail.earningTypeCD), PRBenefitExpenseAcctSubDefault.MaskDeductionCode },
			{ typeof(PRBenefitDetail.codeID), PRBenefitExpenseAcctSubDefault.MaskDeductionCode },
			{ typeof(PRBenefitDetail.labourItemID), PRBenefitExpenseAcctSubDefault.MaskLaborItem},
		};

		public BenExpenseSubAccountAttribute(Type AccountType, Type BranchType, bool AccountAndBranchRequired = false) :
			base(AccountType, BranchType, typeof(PRBenefitDetail.earningTypeCD), typeof(PRBenefitDetail.labourItemID), AccountAndBranchRequired)
		{
		}

		protected override Type[] MaskFieldTypes => new[]
		{
			BranchSubIDAttribute,
			EmployeeSubIDField,
			PayGroupSubIDField,
			typeof(PRDeductCode.benefitExpenseSubID),
			EarningTypeSubIDField,
			LaborItemSubIDField
		};

		protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
		{
			ExpenseMaskSources sources = GetCommonMaskSources(cache, row);

			var line = row as PRBenefitDetail;
			var deductionCode = PXSelectorAttribute.Select<PRBenefitDetail.codeID>(cache, line);
			var benefitSubID = GetValue<PRDeductCode.benefitExpenseSubID>(cache.Graph, deductionCode);

			//Be careful, this array order needs to match with PRBenefitExpenseAcctSubDefault.SubListAttribute (used in PRBenefitExpenseSubAccountMaskAttribute)
			return new object[] { sources.branchSubID, sources.employeeSubID, sources.payGroupSubID, benefitSubID, sources.earningTypeSubID, sources.laborItemSubID };
		}

		public override object MakeSub(PXGraph graph, PRSetup setup, IEnumerable<object> maskSources)
		{
			return PRBenefitExpenseSubAccountMaskAttribute.MakeSub<PRSetup.benefitExpenseSubMask>(graph, setup.BenefitExpenseSubMask, maskSources.ToArray(), MaskFieldTypes);
		}

		protected override Type EmployeeSubIDField => typeof(PREmployee.benefitExpenseSubID);
		protected override Type PayGroupSubIDField => typeof(PRPayGroup.benefitExpenseSubID);
		protected override Type EarningTypeSubIDField => typeof(PREarningType.benefitExpenseSubID);
		protected override Type LaborItemSubIDField => typeof(PRxInventoryItem.benefitExpenseSubID);
		protected override Type SetupField => typeof(PRSetup.benefitExpenseSubMask);
	}

	public class BenLiabilitySubAccountAttribute : PRSubaccountAttribute
	{
		protected Type _IsPayableBenefitField;

		protected override Dictionary<Type, string> DefaultingDependencies { get; set; } = new Dictionary<Type, string>()
		{
			{ typeof(PRBenefitDetail.branchID), PRDeductAcctSubDefault.MaskBranch },
			{ typeof(PRBenefitDetail.employeeID), PRDeductAcctSubDefault.MaskEmployee},
			{ typeof(PRBenefitDetail.codeID), PRDeductAcctSubDefault.MaskDeductionCode },
		};

		#region Constructors
		public BenLiabilitySubAccountAttribute()
		{
		}

		public BenLiabilitySubAccountAttribute(Type AccountType) : base(AccountType)
		{
		}

		public BenLiabilitySubAccountAttribute(Type AccountType, Type BranchType, Type isPayableBenefitField, bool AccountAndBranchRequired = false) : base(AccountType, BranchType, AccountAndBranchRequired)
		{
			_IsPayableBenefitField = isPayableBenefitField;
		}
		#endregion Constructors

		protected override Type[] MaskFieldTypes { get; } = {
			typeof(PRLocationExtAddress.cMPPayrollSubID),
			typeof(PREmployee.benefitLiabilitySubID),
			typeof(PRPayGroup.benefitLiabilitySubID),
			typeof(PRDeductCode.benefitLiabilitySubID)
		};

		protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
		{
			var line = row as PRBenefitDetail;
			var locationAddress = (LocationExtAddress)LocationAddress.Select(cache.Graph, line.BranchID);
			var employee = (PREmployee)Employee.Select(cache.Graph, line.EmployeeID);
			var payGroup = (PRPayGroup)PayGroup.Select(cache.Graph, (string)GetCurrentValue(cache.Graph, PayGroupIDField));
			var deductionCode = PXSelectorAttribute.Select<PRBenefitDetail.codeID>(cache, line);

			var branchSubID = GetValue<PRLocationExtAddress.cMPPayrollSubID>(cache.Graph, locationAddress);
			var employeeSubID = GetValue<PREmployee.benefitLiabilitySubID>(cache.Graph, employee);
			var payGroupSubID = GetValue<PRPayGroup.benefitLiabilitySubID>(cache.Graph, payGroup);
			var benefitSubID = GetValue<PRDeductCode.benefitLiabilitySubID>(cache.Graph, deductionCode);

			//Be careful, this array order needs to match with PRDeductAcctSubDefault.SubListAttribute (used in PRDeductSubAccountMaskAttribute)
			return new object[] { branchSubID, employeeSubID, payGroupSubID, benefitSubID };
		}

		public override object MakeSub(PXGraph graph, PRSetup setup, IEnumerable<object> maskSources)
		{
			return PRDeductSubAccountMaskAttribute.MakeSub<PRSetup.benefitLiabilitySubMask>(graph, setup.BenefitLiabilitySubMask, maskSources.ToArray(), MaskFieldTypes);
		}

		protected override Type SetupField => typeof(PRSetup.benefitLiabilitySubMask);

		protected override bool IsFieldRequired(PXCache cache, object row)
		{
			return cache.GetValue(row, _IsPayableBenefitField.Name) as bool? != true;
		}
	}

	public class TaxExpenseSubAccountAttribute : ExpenseSubAccountAttribute
	{
		protected Type _TaxCategoryField;
		protected override Dictionary<Type, string> DefaultingDependencies { get; set; } = new Dictionary<Type, string>()
		{
			{ typeof(PRTaxDetail.branchID), PRTaxAcctSubDefault.MaskBranch },
			{ typeof(PRTaxDetail.employeeID), PRTaxAcctSubDefault.MaskEmployee },
			{ typeof(PRTaxDetail.taxID), PRTaxAcctSubDefault.MaskTaxCode },
			{ typeof(PRTaxDetail.earningTypeCD), PREarningsAcctSubDefault.MaskEarningType },
			{ typeof(PRTaxDetail.labourItemID), PREarningsAcctSubDefault.MaskLaborItem },
		};

		public TaxExpenseSubAccountAttribute(Type AccountType, Type BranchType, Type taxCategoryField, bool AccountAndBranchRequired = false) :
			base(AccountType, BranchType, typeof(PRTaxDetail.earningTypeCD), typeof(PRTaxDetail.labourItemID), AccountAndBranchRequired)
		{
			_TaxCategoryField = taxCategoryField;
		}

		#region Events
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

		protected override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			if ((string)cache.GetValue(e.Row, _TaxCategoryField.Name) == TaxCategory.EmployerTax)
			{
				base.FieldVerifying(cache, e);
			}
		}

		public override void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
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

		#endregion Events

		protected override Type[] MaskFieldTypes => new[]
		{
			BranchSubIDAttribute,
			EmployeeSubIDField,
			PayGroupSubIDField,
			typeof(PRTaxCode.expenseSubID),
			EarningTypeSubIDField,
			LaborItemSubIDField
		};

		protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
		{
			ExpenseMaskSources sources = GetCommonMaskSources(cache, row);

			var line = row as PRTaxDetail;
			var taxCode = PXSelectorAttribute.Select<PRTaxDetail.taxID>(cache, line);
			var taxSubID = GetValue<PRTaxCode.expenseSubID>(cache.Graph, taxCode);

			//Be careful, this array order needs to match with PRTaxExpenseAcctSubDefault.SubListAttribute (used in PRTaxExpenseSubAccountMaskAttribute)
			return new object[] { sources.branchSubID, sources.employeeSubID, sources.payGroupSubID, taxSubID, sources.earningTypeSubID, sources.laborItemSubID };
		}

		public override object MakeSub(PXGraph graph, PRSetup setup, IEnumerable<object> maskSources)
		{
			return PRTaxExpenseSubAccountMaskAttribute.MakeSub<PRSetup.taxExpenseSubMask>(graph, setup.TaxExpenseSubMask, maskSources.ToArray(), MaskFieldTypes);
		}

		protected override Type EmployeeSubIDField => typeof(PREmployee.payrollTaxExpenseSubID);
		protected override Type PayGroupSubIDField => typeof(PRPayGroup.taxExpenseSubID);
		protected override Type EarningTypeSubIDField => typeof(PREarningType.taxExpenseSubID);
		protected override Type LaborItemSubIDField => typeof(PRxInventoryItem.taxExpenseSubID);
		protected override Type SetupField => typeof(PRSetup.taxExpenseSubMask);
	}

	public class TaxLiabilitySubAccountAttribute : PRSubaccountAttribute
	{
		protected override Dictionary<Type, string> DefaultingDependencies { get; set; } = new Dictionary<Type, string>()
		{
			{ typeof(PRTaxDetail.branchID), PRTaxAcctSubDefault.MaskBranch },
			{ typeof(PRTaxDetail.employeeID), PRTaxAcctSubDefault.MaskEmployee },
			{ typeof(PRTaxDetail.taxID), PRTaxAcctSubDefault.MaskTaxCode },
		};

		#region Constructors
		public TaxLiabilitySubAccountAttribute()
		{
		}

		public TaxLiabilitySubAccountAttribute(Type AccountType) : base(AccountType)
		{
		}

		public TaxLiabilitySubAccountAttribute(Type AccountType, Type BranchType, bool AccountAndBranchRequired = false) : base(AccountType, BranchType, AccountAndBranchRequired)
		{
		}
		#endregion Constructors

		protected override Type[] MaskFieldTypes { get; } = {
			typeof(PRLocationExtAddress.cMPPayrollSubID),
			typeof(PREmployee.payrollTaxLiabilitySubID),
			typeof(PRPayGroup.taxLiabilitySubID),
			typeof(PRTaxCode.liabilitySubID)
		};

		protected override IEnumerable<object> GetMaskSources(PXCache cache, object row)
		{
			var line = row as PRTaxDetail;
			var locationAddress = (LocationExtAddress)LocationAddress.Select(cache.Graph, line.BranchID);
			var employee = (PREmployee)Employee.Select(cache.Graph, line.EmployeeID);
			var payGroup = (PRPayGroup)PayGroup.Select(cache.Graph, (string)GetCurrentValue(cache.Graph, PayGroupIDField));
			var taxCode = PXSelectorAttribute.Select<PRTaxDetail.taxID>(cache, line);

			var branchSubID = GetValue<PRLocationExtAddress.cMPPayrollSubID>(cache.Graph, locationAddress);
			var employeeSubID = GetValue<PREmployee.payrollTaxLiabilitySubID>(cache.Graph, employee);
			var payGroupSubID = GetValue<PRPayGroup.taxLiabilitySubID>(cache.Graph, payGroup);
			var taxSubID = GetValue<PRTaxCode.liabilitySubID>(cache.Graph, taxCode);

			//Be careful, this array order needs to match with PRTaxAcctSubDefault.SubListAttribute (used in PRTaxSubAccountMaskAttribute)
			return new object[] { branchSubID, employeeSubID, payGroupSubID, taxSubID };
		}

		public override object MakeSub(PXGraph graph, PRSetup setup, IEnumerable<object> maskSources)
		{
			return PRTaxSubAccountMaskAttribute.MakeSub<PRSetup.taxLiabilitySubMask>(graph, setup.TaxLiabilitySubMask, maskSources.ToArray(), MaskFieldTypes);
		}
		protected override Type SetupField => typeof(PRSetup.taxLiabilitySubMask);
	}
}