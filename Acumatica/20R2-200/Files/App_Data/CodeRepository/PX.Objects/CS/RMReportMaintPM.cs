using System;
using PX.Data;
using PX.SM;
using System.Collections;
using System.Collections.Generic;
using PX.CS;
using PX.Reports.ARm;

namespace PX.Objects.CS
{
	public class RMReportMaintPM : PXGraphExtension<RMReportMaintGL, RMReportMaint>
	{	
		protected void RMReport_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
		{
            if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<RMReportPM.requestStartAccountGroup>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndAccountGroup>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestStartProject>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndProject>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestStartProjectTask>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndProjectTask>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestStartInventory>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			PXUIFieldAttribute.SetVisible<RMReportPM.requestEndInventory>(sender, e.Row, ((RMReport)e.Row).Type == RMType.PM);
			del(sender, e);
		}

		[PXOverride]
		public virtual bool IsFieldVisible(string field, RMReport report, Func<string, RMReport, bool> baseMethod)
		{
			string reportType = report != null ? report.Type : null;
			
			if (reportType == RMType.PM)
			{
				if (
					field.Equals(typeof(RMDataSourceGL.ledgerID).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.accountClassID).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.startAccount).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.endAccount).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.startSub).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.endSub).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.organizationID).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.useMasterCalendar).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.startBranch).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourceGL.endBranch).Name, StringComparison.InvariantCultureIgnoreCase)
					)
				{
					return false;
				}
			}

			return baseMethod(field, report);			
		}

		[PXOverride]
		public virtual void dataSourceFieldSelecting(PXFieldSelectingEventArgs e, string field)
		{
			RMReport report = (RMReport)e.Row;
			RMDataSource dataSource = report != null ? Base.DataSourceByID.Select(report.DataSourceID) : null;
			if (dataSource == null)
			{
				object defValue;
				if (Base.DataSourceByID.Cache.RaiseFieldDefaulting(field, null, out defValue))
				{
					Base.DataSourceByID.Cache.RaiseFieldUpdating(field, null, ref defValue);
				}
				Base.DataSourceByID.Cache.RaiseFieldSelecting(field, null, ref defValue, true);
				e.ReturnState = defValue;

			}
			else
			{
				e.ReturnState = Base.DataSourceByID.Cache.GetStateExt(dataSource, field);
			}

			//Fix AmountType Combo for PM:
			if (report != null && report.Type == RMType.PM && field.Equals(typeof(RMDataSource.amountType).Name, StringComparison.InvariantCultureIgnoreCase))
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnValue, field, false, 0, null, null,
														  new int[] { BalanceType.NotSet, BalanceType.Amount, BalanceType.Quantity, BalanceType.TurnoverAmount, BalanceType.TurnoverQuantity, BalanceType.BudgetAmount, BalanceType.BudgetQuantity, BalanceType.RevisedAmount, BalanceType.RevisedQuantity, BalanceType.OriginalCommittedAmount, BalanceType.OriginalCommittedQuantity, BalanceType.CommittedAmount, BalanceType.CommittedQuantity, BalanceType.CommittedOpenAmount, BalanceType.CommittedOpenQuantity, BalanceType.CommittedReceivedQuantity, BalanceType.CommittedInvoicedAmount, BalanceType.CommittedInvoicedQuantity },
														  new string[]
														  {
															  Messages.GetLocal(Messages.NotSet),
															  Messages.GetLocal(Messages.ActualAmount),
															  Messages.GetLocal(Messages.ActualQuantity),
															  Messages.GetLocal(Messages.AmountTurnover),
															  Messages.GetLocal(Messages.QuantityTurnover),
															  Messages.GetLocal(Messages.BudgetAmount),
															  Messages.GetLocal(Messages.BudgetQuantity),
															  Messages.GetLocal(Messages.RevisedAmount),
															  Messages.GetLocal(Messages.RevisedQuantity),
															  Messages.GetLocal(Messages.OriginalCommittedAmount),
															  Messages.GetLocal(Messages.OriginalCommittedQuantity),
															  Messages.GetLocal(Messages.CommittedAmount),
															  Messages.GetLocal(Messages.CommittedQuantity),
															  Messages.GetLocal(Messages.CommittedOpenAmount),
															  Messages.GetLocal(Messages.CommittedOpenQuantity),
															  Messages.GetLocal(Messages.CommittedReceivedQuantity),
															  Messages.GetLocal(Messages.CommittedInvoicedAmount),
															  Messages.GetLocal(Messages.CommittedInvoicedQuantity)
														  },
														  typeof(short), 0);
                ((PXFieldState)e.ReturnState).DisplayName = Messages.GetLocal(Messages.AmountType);
			}
					
			Base1.dataSourceFieldSelecting(e,field);					

			if (e.ReturnState is PXFieldState)
			{
				((PXFieldState)e.ReturnState).SetFieldName("DataSource" + field);
				((PXFieldState)e.ReturnState).Visible = Base.IsFieldVisible(field, report); 
			}
		}										
	}
}
