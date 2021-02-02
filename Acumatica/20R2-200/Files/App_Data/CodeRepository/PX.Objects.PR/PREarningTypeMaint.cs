using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	public class PREarningTypeMaint : EPEarningTypesSetup
	{
		#region Views
		public PXSelectJoin<PREarningTypeDetail,
							InnerJoin<PRTaxCode, On<PREarningTypeDetail.taxID, Equal<PRTaxCode.taxID>>>,
							Where<PREarningTypeDetail.typecd, Equal<Current<EPEarningType.typeCD>>>> EarningTypeTaxes;

		public PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Current<EPEarningType.typeCD>>>> EarningSettings;

		public PXSetup<PRSetup> Preferences;
		#endregion

		#region Actions
		//Save and Cancel are already defined by the base class
		public PXInsert<EPEarningType> Insert;
		public PXDelete<EPEarningType> Delete;
		public PXCopyPasteAction<EPEarningType> CopyPaste;
		public PXFirst<EPEarningType> First;
		public PXPrevious<EPEarningType> Previous;
		public PXNext<EPEarningType> Next;
		public PXLast<EPEarningType> Last;
		#endregion

		#region Cache Attached
		//Standard earning screen doesn't require a selector, so we override it for payroll only.
		[PXDefault()]
		[PXDBString(2, IsUnicode = true, IsKey = true, InputMask = ">LL", IsFixed = true)]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(EPEarningType.typeCD))]
		public virtual void EPEarningType_TypeCD_CacheAttached(PXCache sender) { }


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDefault(true)]
		protected virtual void _(Events.CacheAttached<EPEarningType.isActive> e) { }
		#endregion

		#region Row Event Handlers
		protected virtual void EPEarningType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPEarningType row = (EPEarningType)e.Row;
			if (row == null)
				return;

			PREarningType prRow = sender.GetExtension<PREarningType>(row);
			EarningTypeTaxes.Cache.AllowInsert = SubjectToTaxes.IsFromList(prRow.IncludeType);

			bool requireRegularTypeCD = row.IsOvertime == true || prRow.IsPTO == true;
			PXDefaultAttribute.SetPersistingCheck<PREarningType.regularTypeCD>(sender, row, requireRegularTypeCD ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<PREarningType.regularTypeCD>(sender, requireRegularTypeCD);
		}
		#endregion

		#region Field Event Handlers
		protected override void EPEarningType_IsActive_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				base.EPEarningType_IsActive_FieldUpdated(sender, e);
			}
			catch (PXException exception)
			{
				sender.SetValue<EPEarningType.isActive>(e.Row, e.OldValue);
				throw new PXSetPropertyException<EPEarningType.isActive>(exception, PXErrorLevel.Error, exception.Message);
			}
		}

		protected virtual void EPEarningType_IncludeType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPEarningType row = (EPEarningType)e.Row;
			if (e.Row == null)
				return;

			PREarningType prRow = sender.GetExtension<PREarningType>((EPEarningType)e.Row);

			if (prRow.IncludeType == SubjectToTaxes.All || prRow.IncludeType == SubjectToTaxes.None)
			{
				foreach (PREarningTypeDetail item in EarningTypeTaxes.Select())
				{
					EarningTypeTaxes.Delete(item);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<EPEarningType.isActive> e)
		{
			if (e.NewValue as bool? == true)
				return;

			EPEarningType currentEarningType = e.Row as EPEarningType;
			if (string.IsNullOrWhiteSpace(currentEarningType?.TypeCD))
				return;

			string displayName = PXUIFieldAttribute.GetDisplayName(e.Cache, nameof(EPEarningType.isActive));
			CheckEarningTypeUsage(displayName, currentEarningType.TypeCD, false);
		}

		protected virtual void _(Events.FieldVerifying<PREarningType.earningTypeCategory> e)
		{
			if (Equals(e.OldValue, e.NewValue))
				return;

			EPEarningType currentEarningType = e.Row as EPEarningType;
			if (string.IsNullOrWhiteSpace(currentEarningType?.TypeCD))
				return;

			string displayName = PXUIFieldAttribute.GetDisplayName(e.Cache, nameof(PREarningType.earningTypeCategory));
			CheckEarningTypeUsage(displayName, currentEarningType.TypeCD, true);
		}
		#endregion

		private void CheckEarningTypeUsage(string displayName, string typeCD, bool checkExistingEarningDetails)
		{
			CheckEarningTypesWithCurrentRegularEarningType(displayName, typeCD);
			CheckEmployeesWithCurrentEarningType(displayName, typeCD);
			if (checkExistingEarningDetails)
			{
				CheckPaymentsWithCurrentEarningType(displayName, typeCD);
				CheckPayrollBatchesWithCurrentEarningType(displayName, typeCD);
			}
			CheckPreferencesWithCurrentEarningType(displayName, typeCD);
			CheckOvertimeRulesWithCurrentEarningType(displayName, typeCD);
			CheckPTOBanksWithCurrentEarningType(displayName, typeCD);
		}

		private void CheckEarningTypesWithCurrentRegularEarningType(string displayName, string typeCD)
		{
			EPEarningType earningTypeWithCurrentRegularEarningType =
				SelectFrom<EPEarningType>.
				Where<EPEarningType.isActive.IsEqual<True>.
					And<PREarningType.regularTypeCD.IsEqual<P.AsString>>>.View.
				SelectSingleBound(this, null, typeCD);

			if (earningTypeWithCurrentRegularEarningType == null)
				return;

			throw new PXSetPropertyException(Messages.CannotChangeFieldStateEarningType, PXErrorLevel.Error, displayName, typeCD, earningTypeWithCurrentRegularEarningType.TypeCD);
		}

		private void CheckEmployeesWithCurrentEarningType(string displayName, string typeCD)
		{
			EPEmployee employeeWithCurrentEarningType =
				SelectFrom<EPEmployee>.InnerJoin<PREmployeeEarning>.
					On<EPEmployee.bAccountID.IsEqual<PREmployeeEarning.bAccountID>>.
				Where<PREmployeeEarning.typeCD.IsEqual<P.AsString>.
					And<PREmployeeEarning.isActive.IsEqual<True>>>.View.
				SelectSingleBound(this, null, typeCD);

			if (employeeWithCurrentEarningType == null)
				return;

			throw new PXSetPropertyException(Messages.CannotChangeFieldStateEmployee, PXErrorLevel.Error, displayName, typeCD, employeeWithCurrentEarningType.AcctName);
		}

		private void CheckPaymentsWithCurrentEarningType(string displayName, string typeCD)
		{
			PRPayment paymentsWithCurrentEarningType =
				SelectFrom<PRPayment>.InnerJoin<PREarningDetail>.
					On<PRPayment.refNbr.IsEqual<PREarningDetail.paymentRefNbr>.
						And<PRPayment.docType.IsEqual<PREarningDetail.paymentDocType>>>.
				Where<PREarningDetail.typeCD.IsEqual<P.AsString>.
					And<PRPayment.printed.IsNotEqual<True>>.
					And<PRPayment.released.IsNotEqual<True>>>.View.SelectSingleBound(this, null, typeCD);

			if (paymentsWithCurrentEarningType == null)
				return;

			throw new PXSetPropertyException(Messages.CannotChangeFieldStatePayment, PXErrorLevel.Error, displayName, typeCD, paymentsWithCurrentEarningType.PaymentDocAndRef);
		}

		private void CheckPayrollBatchesWithCurrentEarningType(string displayName, string typeCD)
		{
			PRBatch payrollBatchWithCurrentEarningType =
				SelectFrom<PRBatch>.InnerJoin<PREarningDetail>.
					On<PRBatch.batchNbr.IsEqual<PREarningDetail.batchNbr>>.
				Where<PREarningDetail.typeCD.IsEqual<P.AsString>.
					And<PRBatch.open.IsNotEqual<True>>.
					And<PRBatch.closed.IsNotEqual<True>>>.View.SelectSingleBound(this, null, typeCD);

			if (payrollBatchWithCurrentEarningType == null)
				return;

			throw new PXSetPropertyException(Messages.CannotChangeFieldStatePayrollBatch, PXErrorLevel.Error, displayName, typeCD, payrollBatchWithCurrentEarningType.BatchNbr);
		}

		private void CheckPreferencesWithCurrentEarningType(string displayName, string typeCD)
		{
			PRSetup preferences = Preferences.Current;

			string settingName = null;

			if (typeCD == preferences.RegularHoursType)
				settingName = PXUIFieldAttribute.GetDisplayName(Preferences.Cache, nameof(PRSetup.regularHoursType));
			else if (typeCD == preferences.HolidaysType)
				settingName = PXUIFieldAttribute.GetDisplayName(Preferences.Cache, nameof(PRSetup.holidaysType));
			else if (typeCD == preferences.CommissionType)
				settingName = PXUIFieldAttribute.GetDisplayName(Preferences.Cache, nameof(PRSetup.commissionType));

			if (settingName == null)
				return;

			throw new PXSetPropertyException(Messages.CannotChangeFieldStateSetup, PXErrorLevel.Error, displayName, typeCD, settingName);
		}

		private void CheckOvertimeRulesWithCurrentEarningType(string displayName, string typeCD)
		{
			PROvertimeRule overtimeRule = SelectFrom<PROvertimeRule>.
				Where<PROvertimeRule.disbursingTypeCD.IsEqual<P.AsString>.
					And<PROvertimeRule.isActive.IsEqual<True>>>.
				View.SelectSingleBound(this, null, typeCD);

			if (overtimeRule == null)
				return;

			throw new PXSetPropertyException(Messages.CannotChangeFieldStateOvertime, PXErrorLevel.Error, displayName, typeCD, overtimeRule.OvertimeRuleID);
		}

		private void CheckPTOBanksWithCurrentEarningType(string displayName, string typeCD)
		{
			PRPTOBank ptoBank = SelectFrom<PRPTOBank>.
				Where<PRPTOBank.earningTypeCD.IsEqual<P.AsString>.
					And<PRPTOBank.isActive.IsEqual<True>>>.
				View.SelectSingleBound(this, null, typeCD);

			if (ptoBank == null)
				return;

			throw new PXSetPropertyException(Messages.CannotChangeFieldStatePTOBanks, PXErrorLevel.Error, displayName, typeCD, ptoBank.BankID);
		}
	}
}