using PX.Api.Payroll;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Payroll.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PX.Objects.PR
{
	public class PRSetupMaint : PXGraph<PRSetupMaint>
	{
		#region Actions
		public PXSave<PRSetup> Save;
		public PXCancel<PRSetup> Cancel;
		#endregion

		#region Views
		public PXSelect<PRSetup> Setup;

		public SelectFrom<PRTransactionDateException>
			.OrderBy<PRTransactionDateException.date.Asc>.View TransactionDateExceptions;

		public SelectFrom<PRPayment>
			.Where<PRPayment.printed.IsEqual<False>
				.And<PRPayment.released.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>>.View EditablePayments;

		public SelectFrom<PRBenefitDetail>
			.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<PRBenefitDetail.paymentDocType>
				.And<PRPayment.refNbr.IsEqual<PRBenefitDetail.paymentRefNbr>>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRBenefitDetail.codeID>>
			.Where<PRPayment.printed.IsEqual<False>
				.And<PRPayment.released.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>>.View EditableBenefitDetails;

		public SelectFrom<PRTaxDetail>
			.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<PRTaxDetail.paymentDocType>
				.And<PRPayment.refNbr.IsEqual<PRTaxDetail.paymentRefNbr>>>
			.Where<PRPayment.printed.IsEqual<False>
				.And<PRPayment.released.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>>.View EditableTaxDetails;
		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(BenExpenseAccountAttribute), nameof(BenExpenseAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRBenefitDetail.expenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(BenExpenseSubAccountAttribute), nameof(BenExpenseSubAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRBenefitDetail.expenseSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxExpenseAccountAttribute), nameof(TaxExpenseAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.expenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxExpenseSubAccountAttribute), nameof(TaxExpenseSubAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.expenseSubID> e) { }
		#endregion CacheAttached

		#region Events
		protected virtual void _(Events.RowPersisting<PRTransactionDateException> e)
		{
			PRTransactionDateException row = e.Row as PRTransactionDateException;
			if (row == null)
			{
				return;
			}

			// Check that each date has at most one record.
			// We can't use a SQL unique index to check for this condition because PRTransactionDateException uses
			// a CompanyMask.
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Update || (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) &&
				new SelectFrom<PRTransactionDateException>
					.Where<PRTransactionDateException.date.IsEqual<P.AsDateTime>
						.And<PRTransactionDateException.recordID.IsNotEqual<P.AsInt>>>.View(this).SelectSingle(row.Date, row.RecordID) != null)
			{
				throw new PXException(Messages.DuplicateExceptionDate);
			}
		}

		protected virtual void _(Events.FieldVerifying<PRSetup.enablePieceworkEarningType> e)
		{
			if (e.Row == null || Convert.ToBoolean(e.NewValue))
				return;

			CheckPieceworkEarningType();
			CheckEmployeesWithMiscEarningType();
			CheckPaymentsWithMiscEarningType();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.projectCostAssignment> e)
		{
			PRSetup row = e.Row as PRSetup;
			if (row == null)
			{
				return;
			}

			if (e.OldValue.Equals(ProjectCostAssignmentType.WageLaborBurdenAssigned))
			{
				DeleteDetails<PRSetup.projectCostAssignment>(e.Cache, row, CostAssignmentType.DetailType.Benefit);
				DeleteDetails<PRSetup.projectCostAssignment>(e.Cache, row, CostAssignmentType.DetailType.Tax);
				UncalculateEditablePayments();
			}
			else if (row.ProjectCostAssignment == ProjectCostAssignmentType.WageLaborBurdenAssigned)
			{
				UncalculateEditablePayments();
			}
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.benefitExpenseAcctDefault> e)
		{
			PRSetup row = e.Row as PRSetup;
			if (row == null)
			{
				return;
			}

			if ((e.OldValue.Equals(PRBenefitExpenseAcctSubDefault.MaskEarningType) && !e.NewValue.Equals(PRBenefitExpenseAcctSubDefault.MaskEarningType) &&
					!SubMaskContainsValue<PRSetup.benefitExpenseSubMask>(e.Cache, row, row.BenefitExpenseSubMask, PRBenefitExpenseAcctSubDefault.MaskEarningType)) ||
				(e.OldValue.Equals(PRBenefitExpenseAcctSubDefault.MaskLaborItem) && !e.NewValue.Equals(PRBenefitExpenseAcctSubDefault.MaskLaborItem) &&
					!SubMaskContainsValue<PRSetup.benefitExpenseSubMask>(e.Cache, row, row.BenefitExpenseSubMask, PRBenefitExpenseAcctSubDefault.MaskLaborItem)))
			{
				DeleteDetails<PRSetup.benefitExpenseAcctDefault>(e.Cache, row, CostAssignmentType.DetailType.Benefit);
			}

			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.benefitExpenseSubMask> e)
		{
			PRSetup row = e.Row as PRSetup;
			if (row == null)
			{
				return;
			}

			if ((SubMaskContainsValue<PRSetup.benefitExpenseSubMask>(e.Cache, row, (string)e.OldValue, PRBenefitExpenseAcctSubDefault.MaskEarningType) &&
					!SubMaskContainsValue<PRSetup.benefitExpenseSubMask>(e.Cache, row, (string)e.NewValue, PRBenefitExpenseAcctSubDefault.MaskEarningType) &&
					row.BenefitExpenseAcctDefault != PRBenefitExpenseAcctSubDefault.MaskEarningType) ||
				(SubMaskContainsValue<PRSetup.benefitExpenseSubMask>(e.Cache, row, (string)e.OldValue, PRBenefitExpenseAcctSubDefault.MaskLaborItem) &&
					!SubMaskContainsValue<PRSetup.benefitExpenseSubMask>(e.Cache, row, (string)e.NewValue, PRBenefitExpenseAcctSubDefault.MaskLaborItem) &&
					row.BenefitExpenseAcctDefault != PRBenefitExpenseAcctSubDefault.MaskLaborItem))
			{
				DeleteDetails<PRSetup.benefitExpenseSubMask>(e.Cache, row, CostAssignmentType.DetailType.Benefit);
			}

			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.taxExpenseAcctDefault> e)
		{
			PRSetup row = e.Row as PRSetup;
			if (row == null)
			{
				return;
			}

			if ((e.OldValue.Equals(PRTaxExpenseAcctSubDefault.MaskEarningType) && !e.NewValue.Equals(PRTaxExpenseAcctSubDefault.MaskEarningType) &&
					!SubMaskContainsValue<PRSetup.taxExpenseSubMask>(e.Cache, row, row.TaxExpenseSubMask, PRTaxExpenseAcctSubDefault.MaskEarningType)) ||
				(e.OldValue.Equals(PRTaxExpenseAcctSubDefault.MaskLaborItem) && !e.NewValue.Equals(PRTaxExpenseAcctSubDefault.MaskLaborItem) &&
					!SubMaskContainsValue<PRSetup.taxExpenseSubMask>(e.Cache, row, row.TaxExpenseSubMask, PRTaxExpenseAcctSubDefault.MaskLaborItem)))
			{
				DeleteDetails<PRSetup.taxExpenseAcctDefault>(e.Cache, row, CostAssignmentType.DetailType.Tax);
			}

			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.taxExpenseSubMask> e)
		{
			PRSetup row = e.Row as PRSetup;
			if (row == null)
			{
				return;
			}

			if ((SubMaskContainsValue<PRSetup.taxExpenseSubMask>(e.Cache, row, (string)e.OldValue, PRTaxExpenseAcctSubDefault.MaskEarningType) &&
					!SubMaskContainsValue<PRSetup.taxExpenseSubMask>(e.Cache, row, (string)e.NewValue, PRTaxExpenseAcctSubDefault.MaskEarningType) &&
					row.TaxExpenseAcctDefault != PRTaxExpenseAcctSubDefault.MaskEarningType) ||
				(SubMaskContainsValue<PRSetup.taxExpenseSubMask>(e.Cache, row, (string)e.OldValue, PRTaxExpenseAcctSubDefault.MaskLaborItem) &&
					!SubMaskContainsValue<PRSetup.taxExpenseSubMask>(e.Cache, row, (string)e.NewValue, PRTaxExpenseAcctSubDefault.MaskLaborItem) &&
					row.TaxExpenseAcctDefault != PRTaxExpenseAcctSubDefault.MaskLaborItem))
			{
				DeleteDetails<PRSetup.taxExpenseSubMask>(e.Cache, row, CostAssignmentType.DetailType.Tax);
			}

			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.deductLiabilityAcctDefault> e)
		{
			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.deductLiabilitySubMask> e)
		{
			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.benefitLiabilityAcctDefault> e)
		{
			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.benefitLiabilitySubMask> e)
		{
			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.taxLiabilityAcctDefault> e)
		{
			UncalculateEditablePayments();
		}

		protected virtual void _(Events.FieldUpdated<PRSetup.taxLiabilitySubMask> e)
		{
			UncalculateEditablePayments();
		}
		#endregion Events

		#region Helpers
		private void CheckPieceworkEarningType()
		{
			EPEarningType pieceworkEarningType = 
				SelectFrom<EPEarningType>.
				Where<PREarningType.isPiecework.IsEqual<True>>.View.SelectSingleBound(this, null);

			if (pieceworkEarningType == null)
				return;

			throw new PXSetPropertyException(Messages.CannotDeactivatePieceworkEarningType, PXErrorLevel.Error, 
				pieceworkEarningType.TypeCD);
		}

		private void CheckEmployeesWithMiscEarningType()
		{
			EPEmployee employeeWithMiscEarningType =
				SelectFrom<EPEmployee>.
				InnerJoin<PREmployeeEarning>.
					On<EPEmployee.bAccountID.IsEqual<PREmployeeEarning.bAccountID>>.
				InnerJoin<EPEarningType>.
					On<PREmployeeEarning.typeCD.IsEqual<EPEarningType.typeCD>>.
				Where<PREarningType.isPiecework.IsEqual<True>.
					Or<PREmployeeEarning.unitType.IsEqual<UnitType.misc>>>.View.SelectSingleBound(this, null);

			if (employeeWithMiscEarningType == null)
				return;

			throw new PXSetPropertyException(Messages.CannotDeactivatePieceworkEarningTypeEmployee, PXErrorLevel.Error,
				Messages.Misc, employeeWithMiscEarningType.AcctName);
		}

		private void CheckPaymentsWithMiscEarningType()
		{
			PRPayment paymentsWithMiscEarningType =
				SelectFrom<PRPayment>.
				InnerJoin<PREarningDetail>.
					On<PRPayment.refNbr.IsEqual<PREarningDetail.paymentRefNbr>.
						And<PRPayment.docType.IsEqual<PREarningDetail.paymentDocType>>>.
				InnerJoin<EPEarningType>.
					On<PREarningDetail.typeCD.IsEqual<EPEarningType.typeCD>>.
				Where<PREarningType.isPiecework.IsEqual<True>.
					Or<PREarningDetail.unitType.IsEqual<UnitType.misc>>>.View.SelectSingleBound(this, null);

			if (paymentsWithMiscEarningType == null)
				return;

			throw new PXSetPropertyException(Messages.CannotDeactivatePieceworkEarningTypePayment, PXErrorLevel.Error,
				Messages.Misc, string.Format("{0},{1}", paymentsWithMiscEarningType.DocType, paymentsWithMiscEarningType.RefNbr));
		}

		private bool SubMaskContainsValue<TSubMaskField>(PXCache cache, PRSetup setup, string subMask, string compareValue) where TSubMaskField : IBqlField
		{
			return SubMaskContainsValue(cache, setup, typeof(TSubMaskField), subMask, compareValue);
		}

		public static bool SubMaskContainsValue(PXCache cache, PRSetup setup, Type subMaskField, string subMask, string compareValue)
		{
			PRSubAccountMaskAttribute subMaskAttribute = cache.GetAttributesOfType<PRSubAccountMaskAttribute>(setup, subMaskField.Name).FirstOrDefault();
			if (subMaskAttribute != null)
			{
				PRDimensionMaskAttribute dimensionMaskAttribute = subMaskAttribute.GetAttribute<PRDimensionMaskAttribute>();
				if (dimensionMaskAttribute != null)
				{
					List<string> maskValues = dimensionMaskAttribute.GetSegmentMaskValues(subMask).ToList();
					return maskValues.Contains(compareValue);
				}
			}

			return false;
		}

		private void DeleteDetails<TUpdatedField>(PXCache cache, PRSetup row, CostAssignmentType.DetailType detailType) where TUpdatedField : IBqlField
		{
			HashSet<string> affectedAdjChecks = new HashSet<string>();
			if (detailType == CostAssignmentType.DetailType.Benefit)
			{
				foreach (PXResult<PRBenefitDetail, PRPayment> result in EditableBenefitDetails.Select().Select(x => (PXResult<PRBenefitDetail, PRPayment>)x))
				{
					PRBenefitDetail benefitDetail = result;
					PRPayment payment = result;
					EditableBenefitDetails.Delete(benefitDetail);

					if (payment.DocType == PayrollType.Adjustment)
					{
						affectedAdjChecks.Add(EditablePayments.Cache.GetValueExt<PRPayment.paymentDocAndRef>(payment).ToString());
					}
				}
			}
			else
			{
				foreach (PXResult<PRTaxDetail, PRPayment> result in EditableTaxDetails.Select().Select(x => (PXResult<PRTaxDetail, PRPayment>)x))
				{
					PRTaxDetail taxDetail = result;
					PRPayment payment = result;
					EditableTaxDetails.Delete(taxDetail);

					if (payment.DocType == PayrollType.Adjustment)
					{
						affectedAdjChecks.Add(EditablePayments.Cache.GetValueExt<PRPayment.paymentDocAndRef>(payment).ToString());
					}
				}
			}

			if (affectedAdjChecks.Any())
			{
				string detailTypeMessage = detailType == CostAssignmentType.DetailType.Benefit ? Messages.Benefit : Messages.Tax;
				PXUIFieldAttribute.SetWarning<TUpdatedField>(
					cache,
					row,
					PXMessages.LocalizeFormat(Messages.AdjustmentDetailsWillBeDeleted, detailTypeMessage));

				StringBuilder sb = new StringBuilder(PXMessages.LocalizeFormatNoPrefix(Messages.AdjustmentListWithDeletedDetails, detailTypeMessage));
				sb.AppendLine();
				affectedAdjChecks.ForEach(x => sb.AppendLine(x));
				PXTrace.WriteWarning(sb.ToString());
			}
		}

		private void UncalculateEditablePayments()
		{
			foreach (PRPayment payment in EditablePayments.Select())
			{
				payment.Calculated = false;
				EditablePayments.Update(payment);
			}
		}
		#endregion Helpers
	}
}
