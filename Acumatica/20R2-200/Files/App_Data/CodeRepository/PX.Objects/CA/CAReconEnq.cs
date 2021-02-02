using PX.Data;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.CA
{
	[TableAndChartDashboardType]
	public class CAReconEnq : PXGraph<CAReconEnq>
	{
		#region Internal Type Definitions
		[Serializable]
		[PXHidden]
		public partial class CashAccountFilter : PX.Data.IBqlTable
		{
			#region CashAccountID
			public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

			[PXDefault()]
			[CashAccount(search: typeof(Search<CashAccount.cashAccountID,
				Where<CashAccount.active, Equal<True>,
					And<CashAccount.reconcile, Equal<True>>>>),
				DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
			public virtual int? CashAccountID { get; set; }

			#endregion
		}
		#endregion
		#region Buttons Definition
		#region Button Cancel
		public PXAction<CAEnqFilter> cancel;
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			CAReconRecords.Cache.Clear();
			Filter.Cache.Clear();
			TimeStamp = null;
			PXLongOperation.ClearStatus(this.UID);
			return adapter.Get();
		}
		#endregion
		#region Button ViewDoc
		public PXAction<CAEnqFilter> viewDoc;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			CARecon curRecon = CAReconRecords.Current;
			CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();
			graph.CAReconRecords.Current = curRecon;
			throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}
		#endregion
		#region Button Voided
		public PXAction<CAEnqFilter> voided;
		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
		[PXProcessButton]
		public virtual IEnumerable Voided(PXAdapter adapter)
		{
			CARecon recon = CAReconRecords.Current;
			if (recon != null)
			{
				PXLongOperation.StartOperation(this, delegate () { CAReconEntry.VoidCARecon(recon); });
				CAReconRecords.View.RequestRefresh();
			}
			return adapter.Get();
		}
		#endregion
		#region Button CreateRecon
		public PXAction<CAEnqFilter> createRecon;
		[PXUIField(DisplayName = "Create Reconciliation", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable CreateRecon(PXAdapter adapter)
		{
			if (this.Views.ContainsKey("cashAccountFilter"))
			{
				CashAccountFilter createReconFilter = cashAccountFilter.Current;
				WebDialogResult result = this.Views["cashAccountFilter"].AskExt();
				if (result == WebDialogResult.OK)
				{
					CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<AddTrxFilter.cashAccountID>>>>.Select(this, createReconFilter.CashAccountID);
					CAReconEntry.ReconCreate(acct);
					CAReconRecords.View.RequestRefresh();
				}
			}

			return adapter.Get();
		}
		#endregion
		#endregion

		#region Variables
		public PXFilter<CAEnqFilter> Filter;
		[PXFilterable]
		public PXSelect<CARecon> CAReconRecords;
		public PXFilter<CashAccountFilter> cashAccountFilter;
		public PXSetup<CASetup> casetup;
		public PXSelect<CashAccount> cashAccount;
		#endregion

		#region Execute Select
		protected virtual IEnumerable careconrecords()
		{
			List<CAReconMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as List<CAReconMessage>;
			CAEnqFilter filter = Filter.Current;
			foreach (CARecon recon in PXSelectJoin<CARecon,
				InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CARecon.cashAccountID>>,
				InnerJoin<Account, On<Account.accountID, Equal<CashAccount.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
				InnerJoin<Sub, On<Sub.subID, Equal<CashAccount.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>>>>,
				Where2<Where<CARecon.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
																  Or<Required<CAEnqFilter.accountID>, IsNull>>,
															And2<Where<CARecon.reconDate, GreaterEqual<Required<CAEnqFilter.startDate>>,
																	Or<Required<CAEnqFilter.startDate>, IsNull>>,
															And<Where<CARecon.reconDate, LessEqual<Required<CAEnqFilter.endDate>>,
																	Or<Required<CAEnqFilter.endDate>, IsNull>>>>>,
													   OrderBy<Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>
					.Select(this, filter.AccountID, filter.AccountID, filter.StartDate, filter.StartDate, filter.EndDate, filter.EndDate))
			{
				TimeSpan timespan;
				Exception ex;
				if ((PXLongOperation.GetStatus(UID, out timespan, out ex) == PXLongRunStatus.Aborted || PXLongOperation.GetStatus(UID, out timespan, out ex) == PXLongRunStatus.Completed) &&
					listMessages != null && listMessages.Count > 0)
					for (int i = 0; i < listMessages.Count; i++)
					{
						CAReconMessage message = (CAReconMessage)listMessages[i];
						if (message.KeyCashAccount == recon.CashAccountID && message.KeyReconNbr == recon.ReconNbr)
						{
							CAReconRecords.Cache.RaiseExceptionHandling<CARecon.reconNbr>(recon, recon.ReconNbr, new PXSetPropertyException(message.Message, message.ErrorLevel));
						}
					}
				yield return recon;
			}
		}
		#endregion

		#region CAEnqFilter Events
		protected virtual void CAEnqFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CAEnqFilter filter = (CAEnqFilter)e.Row;
			if (filter == null) return;
			PXCache reconCache = CAReconRecords.Cache;
			reconCache.AllowInsert = false;
			reconCache.AllowUpdate = false;
			reconCache.AllowDelete = false;
			CashAccountFilter reconCreateFilter = cashAccountFilter.Current;
			cashAccountFilter.Cache.RaiseRowSelected(reconCreateFilter);
		}
		#endregion

		#region CashAccountFilter Events
		protected virtual void CashAccountFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CashAccountFilter reconCreateFilter = (CashAccountFilter)e.Row;
			cache.AllowUpdate = true;
			PXUIFieldAttribute.SetEnabled(cache, reconCreateFilter, false);
			PXUIFieldAttribute.SetEnabled<CashAccountFilter.cashAccountID>(cache, reconCreateFilter, true);
		}
		protected virtual void CashAccountFilter_CashAccountID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			CashAccountFilter createReconFilter = (CashAccountFilter)e.Row;
			if (createReconFilter == null) return;
			CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, (string)e.NewValue);
			if (acct != null && acct.Reconcile != true)
			{
                throw new PXSetPropertyException(Messages.CashAccounNotReconcile);
            }
		}
		#endregion

	}
}
