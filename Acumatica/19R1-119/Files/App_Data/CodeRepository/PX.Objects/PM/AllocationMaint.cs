using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CR;
using System.Collections;

namespace PX.Objects.PM
{
	public class AllocationMaint : PXGraph<AllocationMaint, PMAllocation>
	{

		#region Metadata for PMFormulaEditor
		public PXSelect<PMTran> PMTranMetaData;
		public PXSelect<PMProject> PMProjectMetaData;
		public PXSelect<PMTask> PMTaskMetaData;
		public PXSelect<PMAccountGroup> PMAccountGroupMetaData;
		public PXSelect<PMBudget> PMBudget;
		public PXSelect<PX.Objects.EP.EPEmployee> EmployeesMetaData;
		public PXSelect<PX.Objects.AP.Vendor> VendorMetaData;
		public PXSelect<PX.Objects.AR.Customer> CustomerMetaData;
		public PXSelect<PX.Objects.IN.InventoryItem> InventoryItemMetaData;
		#endregion

		#region Views/Selects

		public PXSelect<PMAllocation> Allocations;
		public PXSelect<PMAllocationStep, Where<PMAllocationStep.allocationID, Equal<Current<PMAllocation.allocationID>>>> Steps;
		public PXSelect<PMAllocationStep, Where<PMAllocationStep.allocationID, Equal<Current<PMAllocationStep.allocationID>>, And<PMAllocationStep.stepID, Equal<Current<PMAllocationStep.stepID>>>>> Step;
		public PXSelect<PMAllocationStep, Where<PMAllocationStep.allocationID, Equal<Current<PMAllocationStep.allocationID>>, And<PMAllocationStep.stepID, Equal<Current<PMAllocationStep.stepID>>>>> StepRules;
		public PXSelect<PMAllocationStep, Where<PMAllocationStep.allocationID, Equal<Current<PMAllocationStep.allocationID>>, And<PMAllocationStep.stepID, Equal<Current<PMAllocationStep.stepID>>>>> StepSettings;

		#endregion

		#region Event Handlers

		protected virtual void PMAllocationStep_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.selectOption>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.post>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.dateSource>(sender, e.Row, row.Method == PMMethod.Transaction);

				PXUIFieldAttribute.SetVisible<PMAllocationStep.accountOrigin>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.accountID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.subMask>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.subID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.accountOrigin>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetProjectOrigin>(sender, e.Row, row.UpdateGL != true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetProjectID>(sender, e.Row, row.UpdateGL != true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetTaskOrigin>(sender, e.Row, row.UpdateGL != true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetTaskID>(sender, e.Row, row.UpdateGL != true);

				PXUIFieldAttribute.SetEnabled<PMAllocationStep.projectOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.taskOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);

				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetAccountOrigin>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetAccountID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetSubMask>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetSubID>(sender, e.Row, row.UpdateGL == true);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.offsetProjectOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.offsetTaskOrigin>(sender, e.Row, row.Method == PMMethod.Transaction);

				PXUIFieldAttribute.SetVisible<PMAllocationStep.branchOrigin>(sender, e.Row, ShowBranchOptions());
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.sourceBranchID>(sender, e.Row, row.BranchOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.projectID>(sender, e.Row, row.ProjectOrigin == PMOrigin.Change && row.AccountGroupOrigin != PMOrigin.None);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.taskID>(sender, e.Row, row.TaskOrigin == PMOrigin.Change && row.AccountGroupOrigin != PMOrigin.None);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.accountGroupID>(sender, e.Row, row.AccountGroupOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.accountID>(sender, e.Row, row.AccountOrigin == PMOrigin.Change);

				PXUIFieldAttribute.SetVisible<PMAllocationStep.offsetBranchOrigin>(sender, e.Row, ShowBranchOptions());
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.targetBranchID>(sender, e.Row, row.OffsetBranchOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.offsetProjectID>(sender, e.Row, row.OffsetProjectOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.offsetTaskID>(sender, e.Row, row.OffsetTaskOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.offsetAccountGroupID>(sender, e.Row, row.OffsetAccountGroupOrigin == PMOrigin.Change);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.offsetAccountID>(sender, e.Row, row.OffsetAccountOrigin == PMOrigin.Change);

				PXUIFieldAttribute.SetEnabled<PMAllocationStep.rangeStart>(sender, e.Row, row.SelectOption == PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.rangeEnd>(sender, e.Row, row.SelectOption == PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.accountGroupFrom>(sender, e.Row, row.SelectOption != PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.accountGroupTo>(sender, e.Row, row.SelectOption != PMSelectOption.Step);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.rateTypeID>(sender, e.Row, row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.billableQtyFormula>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.allocateZeroAmount>(sender, e.Row, row.Method != PMMethod.Budget);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.qtyFormula>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.amountFormula>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.descriptionFormula>(sender, e.Row, row.Post == true);

				PXUIFieldAttribute.SetEnabled<PMAllocationStep.groupByDate>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.groupByEmployee>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.groupByItem>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				PXUIFieldAttribute.SetEnabled<PMAllocationStep.groupByVendor>(sender, e.Row, row.Post == true && row.Method == PMMethod.Transaction);
				ValidateWarnings(row);
			}
		}

		public virtual bool ShowBranchOptions()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.branch>())
				return false;

			object[] ids = PXAccess.GetBranches();

			return !(ids == null || ids.Length <= 1);
		}

		protected virtual void PMAllocationStep_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row == null) return;

			PMAllocationStep oldRow = e.OldRow as PMAllocationStep;
			if (oldRow == null) return;

			if (row.SelectOption == PMSelectOption.Step && oldRow.SelectOption != PMSelectOption.Step)
			{
				row.AccountGroupFrom = null;
				row.AccountGroupTo = null;
			}
			else if (row.SelectOption != PMSelectOption.Step && oldRow.SelectOption == PMSelectOption.Step)
			{
				row.RangeStart = null;
				row.RangeEnd = null;
			}

			ValidateWarnings(row);
		}

		protected virtual void PMAllocationStep_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null && e.Operation != PXDBOperation.Delete)
			{
				Validate(row);
			}
		}

		protected virtual void PMAllocationStep_UpdateGL_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.UpdateGL == true)
				{
					sender.SetValueExt<PMAllocationStep.accountOrigin>(row, PMOrigin.Change);
					sender.SetValueExt<PMAllocationStep.offsetAccountOrigin>(row, PMOrigin.Change);
					row.Reverse = PMReverse.OnInvoice;
				}
				else
				{
					sender.SetValueExt<PMAllocationStep.accountGroupOrigin>(row, PMOrigin.Source);
					sender.SetValueExt<PMAllocationStep.offsetAccountGroupOrigin>(row, PMOrigin.Source);
					row.Reverse = PMReverse.OnBilling;
				}
			}
		}

		protected virtual void PMAllocationStep_AccountOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.AccountOrigin == PMOrigin.Change)
				{
					row.AccountGroupOrigin = PMOrigin.FromAccount;
				}
				else
				{
					row.AccountID = null;
				}
			}
		}

		protected virtual void PMAllocationStep_OffsetAccountOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.OffsetAccountOrigin == PMOrigin.Change)
				{
					row.OffsetAccountGroupOrigin = PMOrigin.FromAccount;
				}
				else
				{
					row.OffsetAccountID = null;
				}
			}
		}

		protected virtual void PMAllocationStep_AccountGroupOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.AccountGroupOrigin != PMOrigin.Change)
				{
					row.AccountGroupID = null;
				}

				if (row.AccountGroupOrigin == PMOrigin.None)
				{
					row.ProjectID = null;
					row.TaskID = null;
				}
			}
		}

		protected virtual void PMAllocationStep_OffsetAccountGroupOrigin_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.OffsetAccountGroupOrigin != PMOrigin.Change)
				{
					row.OffsetAccountGroupID = null;
				}
			}
		}

		protected virtual void PMAllocationStep_Method_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.Method == PMMethod.Budget)
				{
					row.RateTypeID = null;
					row.QtyFormula = null;
					row.BillableQtyFormula = null;
					row.AmountFormula = null;
					row.SelectOption = PMSelectOption.Transaction;
					row.AllocateZeroAmount = false;
					row.Post = true;
					row.DateSource = PMDateSource.Allocation;

				}
				else
				{
					if (row.UpdateGL == false && row.AccountGroupOrigin == PMOrigin.None)
					{
						sender.SetDefaultExt<PMAllocationStep.accountGroupOrigin>(e.Row);
					}
				}
			}

		}

		protected virtual void PMAllocationStep_SourceBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void PMAllocationStep_TargetBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}
				
		protected virtual void PMAllocationStep_AccountGroupOrigin_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				List<string> allowedValues = new List<string>();
				List<string> allowedLabels = new List<string>();

				allowedValues.Add(PMOrigin.Source);
				allowedValues.Add(PMOrigin.Change);

				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Source));
				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Change));

				if (row.UpdateGL == true)
				{
					allowedValues.Add(PMOrigin.FromAccount);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_FromAccount));
				}
				else if (row.Method == PMMethod.Budget)
				{
					allowedValues.Add(PMOrigin.None);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_None));
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(PMAllocationStep.accountGroupOrigin).Name, false, 1, null,
													allowedValues.ToArray(), allowedLabels.ToArray(), true, PMOrigin.Source);

				((PXStringState)e.ReturnState).Enabled = row.UpdateGL != true;
			}
		}

		protected virtual void PMAllocationStep_OffsetAccountGroupOrigin_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				List<string> allowedValues = new List<string>();
				List<string> allowedLabels = new List<string>();

				allowedValues.Add(PMOrigin.Source);
				allowedValues.Add(PMOrigin.Change);

				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Source));
				allowedLabels.Add(Messages.GetLocal(Messages.Origin_Change));

				if (row.UpdateGL == true)
				{
					allowedValues.Add(PMOrigin.FromAccount);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_FromAccount));
				}
				else
				{
					allowedValues.Add(PMOrigin.None);
					allowedLabels.Add(Messages.GetLocal(Messages.Origin_None));
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(PMAllocationStep.offsetAccountGroupOrigin).Name, false, 1, null,
													allowedValues.ToArray(), allowedLabels.ToArray(), true, PMOrigin.Source);

				((PXStringState)e.ReturnState).Enabled = row.UpdateGL != true;
			}
		}

		protected virtual void PMAllocationStep_Post_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				sender.SetValueExt<PMAllocationStep.updateGL>(e.Row, row.Post);
			}
		}

		protected virtual void PMAllocationStep_RangeStart_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.RangeStart == row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationStep.rangeStart>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (row.RangeStart > row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationStep.rangeStart>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}
			}
		}

		protected virtual void PMAllocationStep_RangeEnd_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMAllocationStep row = e.Row as PMAllocationStep;
			if (row != null)
			{
				if (row.RangeEnd == row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationStep.rangeEnd>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (row.RangeEnd > row.StepID)
				{
					sender.RaiseExceptionHandling<PMAllocationStep.rangeEnd>(e.Row, e.NewValue, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}
			}
		}

		#endregion

		protected virtual void Validate(PMAllocationStep step)
		{
			if (ValidateErrors(step))
			{
				ValidateWarnings(step);
			}
		}

		/// <summary>
		/// Validate conditions for the given step that raise warnings.
		/// </summary>
		/// <param name="step">Allocation rule</param>
		protected virtual void ValidateWarnings(PMAllocationStep step)
		{
			//PXUIFieldAttribute.SetError<PMAllocationStep.target>(Step.Cache, step, null);

			if (step.UpdateGL == true)
			{
				if (step.AccountID != null && step.OffsetAccountID != null && step.AccountID == step.OffsetAccountID)
				{
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.accountID>(step, null, new PXSetPropertyException(Messages.DebitAccountEqualCreditAccount, PXErrorLevel.RowWarning));
				}
			}
			else
			{
				if (step.AccountGroupID != null && step.OffsetAccountGroupID != null && step.AccountGroupID == step.OffsetAccountGroupID)
				{
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.accountID>(step, null, new PXSetPropertyException(Messages.DebitAccountGroupEqualCreditAccountGroup, PXErrorLevel.RowWarning));
				}
			}
		}

		/// <summary>
		/// Validate conditions for the given step that raise errors.
		/// </summary>
		/// <param name="step">Allocation rule</param>
		/// <returns>True if valid</returns>
		protected virtual bool ValidateErrors(PMAllocationStep step)
		{
			bool valid = true;

			if (step.SelectOption == PMSelectOption.Step)
			{
				if (step.RangeStart == null)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.rangeStart>(step, null, new PXException(ErrorMessages.FieldIsEmpty, typeof(PMAllocationStep.rangeStart).Name));
				}

				if (step.RangeEnd == null)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.rangeEnd>(step, null, new PXException(ErrorMessages.FieldIsEmpty, typeof(PMAllocationStep.rangeEnd).Name));
				}

				if (step.RangeStart == step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.rangeStart>(step, step.RangeStart, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (step.RangeStart > step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.rangeStart>(step, step.RangeStart, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}

				if (step.RangeEnd == step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.rangeEnd>(step, step.RangeEnd, new PXSetPropertyException(Messages.RangeOverlapItself));
				}

				if (step.RangeEnd > step.StepID)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.rangeEnd>(step, step.RangeEnd, new PXSetPropertyException(Messages.RangeOverlapFuture));
				}
				
			}
			else
			{
				if (step.AccountGroupFrom == null)
				{
					valid = false;
					Step.Cache.RaiseExceptionHandling<PMAllocationStep.accountGroupFrom>(step, null, new PXException(ErrorMessages.FieldIsEmpty, typeof(PMAllocationStep.accountGroupFrom).Name));
				}

			}

			if (step.BranchOrigin == PMOrigin.Change && step.SourceBranchID == null)
			{
				valid = false;
				Step.Cache.RaiseExceptionHandling<PMAllocationStep.sourceBranchID>(step, step.SourceBranchID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PMAllocationStep.sourceBranchID).Name));
			}

			if (step.AccountGroupOrigin == PMOrigin.Change && step.AccountGroupID == null)
			{
				valid = false;
				Step.Cache.RaiseExceptionHandling<PMAllocationStep.accountGroupID>(step, step.AccountGroupID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PMAllocationStep.accountGroupID).Name));
			}

			if (step.OffsetBranchOrigin == PMOrigin.Change && step.TargetBranchID == null)
			{
				valid = false;
				Step.Cache.RaiseExceptionHandling<PMAllocationStep.targetBranchID>(step, step.TargetBranchID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PMAllocationStep.targetBranchID).Name));
			}

			if (step.OffsetAccountGroupOrigin == PMOrigin.Change && step.OffsetAccountGroupID == null)
			{
				valid = false;
				Step.Cache.RaiseExceptionHandling<PMAllocationStep.offsetAccountGroupID>(step, step.OffsetAccountGroupID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(PMAllocationStep.offsetAccountGroupID).Name));
			}

			if (step.UpdateGL != true)
			{
				if (step.Method == PMMethod.Transaction)
				{
					if (step.AccountGroupOrigin == PMOrigin.Change && step.AccountGroupID == null)
					{
						valid = false;
						Step.Cache.RaiseExceptionHandling<PMAllocationStep.accountGroupID>(step, step.AccountGroupID,
																					   new PXException(
																						Messages.DebitAccountGroupIsRequired, step.StepID));
					}
				}
				else
				{
					if (step.AccountGroupOrigin == PMOrigin.None && step.OffsetAccountGroupOrigin == PMOrigin.None)
					{
						valid = false;
						Step.Cache.RaiseExceptionHandling<PMAllocationStep.accountGroupID>(step, step.AccountGroupID,
																					   new PXException(
																						Messages.AtleastOneAccountGroupIsRequired, step.StepID));
					}
				}
			}

			return valid;
		}
	}
}
