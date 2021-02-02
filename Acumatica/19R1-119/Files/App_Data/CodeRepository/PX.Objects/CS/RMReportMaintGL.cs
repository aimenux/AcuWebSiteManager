using System;
using PX.Data;
using PX.SM;
using System.Collections;
using System.Collections.Generic;
using PX.CS;
using PX.Reports.ARm;

namespace PX.Objects.CS
{
	public class RMReportMaintGL : PXGraphExtension<RMReportMaint>
	{
		[PXOverride]
		public virtual bool IsFieldVisible(string field, RMReport report)
		{
			string reportType = report != null ? report.Type : null;
			if(field.Equals(typeof(RMDataSourceGL.useMasterCalendar).Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>();
			}
			if (reportType == RMType.GL)
			{
				if (
					field.Equals(typeof(RMDataSourcePM.startAccountGroup).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endAccountGroup).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.startProject).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endProject).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.startProjectTask).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endProjectTask).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.startInventory).Name, StringComparison.InvariantCultureIgnoreCase) ||
					field.Equals(typeof(RMDataSourcePM.endInventory).Name, StringComparison.InvariantCultureIgnoreCase)
					)
				{
					return false;
				}
			}	
			return true;
		}
		
		protected void RMReport_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
		{
            if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<RMReportGL.requestAccountClassID>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestEndAccount>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestEndSub>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestLedgerID>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestStartAccount>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestStartSub>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
            PXUIFieldAttribute.SetVisible<RMReportGL.requestStartBranch>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
            PXUIFieldAttribute.SetVisible<RMReportGL.requestEndBranch>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestOrganizationID>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL);
			PXUIFieldAttribute.SetVisible<RMReportGL.requestUseMasterCalendar>(sender, e.Row, ((RMReport)e.Row).Type == RMType.GL && 
			                                                                                  PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>());
			del(sender, e);
		}

		[PXOverride]
		public virtual void dataSourceFieldSelecting(PXFieldSelectingEventArgs e, string field)
		{			
			RMReport report = (RMReport)e.Row;
			if (report != null && report.Type == RMType.GL && field.Equals(typeof(RMDataSource.amountType).Name, StringComparison.InvariantCultureIgnoreCase))
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnValue, field, false, 0, null, null,
														  new int[] { BalanceType.NotSet, BalanceType.Turnover, BalanceType.Credit, BalanceType.Debit, BalanceType.BeginningBalance, BalanceType.EndingBalance,
                                                                                            BalanceType.CuryTurnover, BalanceType.CuryCredit, BalanceType.CuryDebit, BalanceType.CuryBeginningBalance, BalanceType.CuryEndingBalance},
														  new string[]
														  {
															  Messages.GetLocal(Messages.NotSet),
															  Messages.GetLocal(Messages.Turnover),
															  Messages.GetLocal(Messages.Credit),
															  Messages.GetLocal(Messages.Debit),
															  Messages.GetLocal(Messages.BegBalance),
															  Messages.GetLocal(Messages.EndingBalance),
                                                              Messages.GetLocal(Messages.CuryTurnover),
															  Messages.GetLocal(Messages.CuryCredit),
															  Messages.GetLocal(Messages.CuryDebit),
															  Messages.GetLocal(Messages.CuryBegBalance),
															  Messages.GetLocal(Messages.CuryEndingBalance),
														  },
														  typeof(short), BalanceType.NotSet);
				
					
                ((PXFieldState)e.ReturnState).DisplayName = Messages.GetLocal(Messages.AmountType);
			}			
		}						
	}
}
