using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.PM;


namespace PX.Objects.EP
{
	public class EPEarningTypesSetup : PXGraph<EPEarningTypesSetup>
	{
		#region Selects

		public PXSelect<EPEarningType> EarningTypes;
		public PXSetup<EPSetup> Setup;

		#endregion

		#region Actions

		public PXSave<EPEarningType> Save;
		public PXCancel<EPEarningType> Cancel;

		#endregion

		protected void EPEarningType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPEarningType row = (EPEarningType)e.Row;
			if (row == null) return;

			EPSetup setup = PXSelect<
				EPSetup
				, Where<EPSetup.regularHoursType, Equal<Required<EPEarningType.typeCD>>
					, Or<EPSetup.holidaysType, Equal<Required<EPEarningType.typeCD>>
						, Or<EPSetup.vacationsType, Equal<Required<EPEarningType.typeCD>>>
						>
					>
				>.Select(this, row.TypeCD, row.TypeCD, row.TypeCD);
			if (setup != null)
				throw new PXException(Messages.CannotDeleteInUse);

			CRCaseClassLaborMatrix caseClassLabor = PXSelect<CRCaseClassLaborMatrix, Where<CRCaseClassLaborMatrix.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (caseClassLabor != null)
				throw new PXException(Messages.CannotDeleteInUse);

			EPContractRate contractRate = PXSelect<EPContractRate, Where<EPContractRate.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (contractRate != null)
				throw new PXException(Messages.CannotDeleteInUse);

			EPEmployeeClassLaborMatrix employeeLabor = PXSelect<EPEmployeeClassLaborMatrix, Where<EPEmployeeClassLaborMatrix.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (employeeLabor != null)
				throw new PXException(Messages.CannotDeleteInUse);

			PMTimeActivity activity = PXSelect<PMTimeActivity, Where<PMTimeActivity.earningTypeID, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (activity != null)
				throw new PXException(Messages.CannotDeleteInUse);

			PMTran pmTran = PXSelect<PMTran, Where<PMTran.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (pmTran != null)
				throw new PXException(Messages.CannotDeleteInUse);

		}

		protected virtual void EPEarningType_IsActive_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPEarningType row = e.Row as EPEarningType;
			if (row == null) return;

			if (row.IsActive == false)
			{
				if (Setup.Current.RegularHoursType == row.TypeCD)
				{
					throw new PXException(String.Format(EP.Messages.EarningTypeDeactivate, row.TypeCD, PXUIFieldAttribute.GetDisplayName<EPSetup.regularHoursType>(Setup.Cache)));
				}
				if (Setup.Current.HolidaysType == row.TypeCD)
				{
					throw new PXException(String.Format(EP.Messages.EarningTypeDeactivate, row.TypeCD, PXUIFieldAttribute.GetDisplayName<EPSetup.holidaysType>(Setup.Cache)));
				}
				if (Setup.Current.VacationsType == row.TypeCD)
				{
					throw new PXException(String.Format(EP.Messages.EarningTypeDeactivate, row.TypeCD, PXUIFieldAttribute.GetDisplayName<EPSetup.vacationsType>(Setup.Cache)));
				}
			}
		}
	}
}