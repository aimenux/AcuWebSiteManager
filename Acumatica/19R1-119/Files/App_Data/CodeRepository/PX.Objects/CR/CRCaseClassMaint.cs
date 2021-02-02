using System;
using PX.Data;
using PX.Objects.CT;

namespace PX.Objects.CR
{
	public class CRCaseClassMaint : PXGraph<CRCaseClassMaint, CRCaseClass>
	{
		[PXViewName(Messages.CaseClass)]
		public PXSelect<CRCaseClass>
			CaseClasses;

		[PXHidden]
		public PXSelect<CRCaseClass, 
			Where<CRCaseClass.caseClassID, Equal<Current<CRCaseClass.caseClassID>>>> 
			CaseClassesCurrent;

		[PXViewName(Messages.CaseClassReaction)]
		public PXSelect<CRClassSeverityTime, 
			Where<CRClassSeverityTime.caseClassID, Equal<Current<CRCaseClass.caseClassID>>>> 
			CaseClassesReaction;

        [PXViewName(Messages.Attributes)]
        public CSAttributeGroupList<CRCaseClass, CRCase> Mapping;

        [PXHidden]
		public PXSelect<CRSetup> 
			Setup;

		public PXSelect<CRCaseClassLaborMatrix, Where<CRCaseClassLaborMatrix.caseClassID, Equal<Current<CRCaseClass.caseClassID>>>>
			LaborMatrix;

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBLiteDefault(typeof(CRCaseClass.caseClassID))]
		protected virtual void CRClassSeverityTime_CaseClassID_CacheAttached(PXCache sender) { }

		public CRCaseClassMaint()
		{
			FieldDefaulting.AddHandler<IN.InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });
		}

		protected virtual void CRCaseClass_DefaultContractID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null) return;

			Contract contract = PXSelectReadonly<Contract,
				Where<Contract.contractID, Equal<Required<CRCase.contractID>>>>.
				Select(this, e.NewValue);

			if (contract != null)
			{
				int daysLeft;
				if (ContractMaint.IsExpired(contract, (DateTime)Accessinfo.BusinessDate))
				{
					e.NewValue = null;
					throw new PXSetPropertyException(Messages.ContractExpired, PXErrorLevel.Error);
				}
				if (ContractMaint.IsInGracePeriod(contract, (DateTime)Accessinfo.BusinessDate, out daysLeft))
				{
					e.NewValue = contract.ContractCD;
					throw new PXSetPropertyException(Messages.ContractInGracePeriod, PXErrorLevel.Warning, daysLeft);
				}
			}
		}

        protected virtual void CRCaseClass_PerItemBilling_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CRCaseClass row = e.Row as CRCaseClass;
            if (row == null) return;

            CRCase crCase = PXSelect<CRCase, Where<CRCase.caseClassID, Equal<Required<CRCaseClass.caseClassID>>, 
                                And<CRCase.isBillable, Equal<True>,
                                And<CRCase.released, Equal<False>>>>>.SelectWindowed(this, 0, 1, row.CaseClassID);

            if (crCase != null)
            {
                throw new PXSetPropertyException(Messages.CurrentClassHasUnreleasedRelatedCases, PXErrorLevel.Error);
            }
        }

		protected virtual void CRCaseClass_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRCaseClass;
			if (row == null) return;

			if (row.IsBillable == true)
			{
				row.RequireCustomer = true;
			}
		}

		protected virtual void CRCaseClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRCaseClass;
			if (row == null) return;

			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void CRCaseClass_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = e.Row as CRCaseClass;
			if (row == null) return;

			CRSetup s = Setup.Select();

			if (s != null && s.DefaultCaseClassID == row.CaseClassID)
			{
				s.DefaultCaseClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void CRCaseClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CRCaseClass;
			if (row == null) return;
			
			if (!CanDelete(row))
			{
				throw new PXException(Messages.RecordIsReferenced);
			}
		}

		[PXDBInt(MinValue = 0, MaxValue = 1440)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void CRCaseClass_ReopenCaseTimeInDays_CacheAttached(PXCache sender) { }

		private bool CanDelete(CRCaseClass row)
		{
			if (row != null)
			{
				CRCase c = PXSelect<CRCase, 
					Where<CRCase.caseClassID, Equal<Required<CRCase.caseClassID>>>>.
					SelectWindowed(this, 0, 1, row.CaseClassID);
				if (c != null)
				{
					return false;
				}
			}

			return true;
		}
	}
}
