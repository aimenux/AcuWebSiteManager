using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Objects.Common.Discount;
using PX.Objects.EP;
using static PX.Objects.PM.PMQuoteMaint;

namespace PX.Objects.PM
{
	public class PMQuoteMaintExt : PXGraphExtension<PMDiscount, PMQuoteMaint>
	{
		public override void Initialize()
		{
			Base.actionsFolder.AddMenuAction(Base.Actions[nameof(Base.Approval.Approve)]);
			Base.actionsFolder.AddMenuAction(Base.Actions[nameof(Base.Approval.Reject)]);

			Base.Actions[nameof(Base.Approval.Submit)].SetCaption(CR.Messages.SummitForApproval);

			var sender = Base.Quote.Cache;
			var row = sender.Current as PMQuote;

			if (row != null)
			{
				VisibilityHandler(sender, row);
			}

			Base.Actions.Move(nameof(Base.actionsFolder), nameof(Base.Approval.Submit), true);
			Base.Actions.Move(nameof(Base.actionsFolder), nameof(EditQuote), true);

			Base.Approval.StatusHandler =
				(item) =>
				{
					if (item.SubmitCancelled == true) return;

					switch (Base.Approval.GetResult(item))
					{
						case ApprovalResult.Approved:
							item.Status = PMQuoteStatusAttribute.Approved;
							break;

						case ApprovalResult.Rejected:
							item.Status = PMQuoteStatusAttribute.Rejected;
							break;

						case ApprovalResult.PendingApproval:
							item.Status = PMQuoteStatusAttribute.PendingApproval;
							break;

						case ApprovalResult.Submitted:
							item.Status = PMQuoteStatusAttribute.Approved;
							break;
					}
				};
		}

		public PXAction<PMQuote> EditQuote;
		[PXUIField(DisplayName = CR.Messages.EditQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable editQuote(PXAdapter adapter)
		{
			var row = Base.Quote.Current;

			if (row == null) return adapter.Get();

			Base.Approval.Reset(row);

			row.Approved = false;
			row.Rejected = false;
			row.Status = PMQuoteStatusAttribute.Draft;

			Base.Quote.Cache.Update(row);

			Base.Save.Press();

			return adapter.Get();
		}

		protected virtual void PMQuote_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected sel)
		{
			sel?.Invoke(sender, e);

			var row = e.Row as PMQuote;
			if (row == null) return;

			VisibilityHandler(sender, row);
		}

		private void VisibilityHandler(PXCache sender, PMQuote row)
		{
			CR.Standalone.CROpportunityRevision revisionInDb = PXSelectReadonly<CR.Standalone.CROpportunityRevision,
				Where<CR.Standalone.CROpportunityRevision.noteID, Equal<Required<CR.Standalone.CROpportunityRevision.noteID>>>>.Select(Base, row.QuoteID).FirstOrDefault();
			CR.Standalone.CROpportunity opportunityInDb = (revisionInDb == null) ? null : PXSelectReadonly<CR.Standalone.CROpportunity,
				Where<CR.Standalone.CROpportunity.opportunityID, Equal<Required<CR.Standalone.CROpportunity.opportunityID>>>>.Select(Base, revisionInDb.OpportunityID).FirstOrDefault();

			CR.Standalone.CROpportunity opportunity = PXSelect<CR.Standalone.CROpportunity,
				Where<CR.Standalone.CROpportunity.opportunityID, Equal<Required<CR.Standalone.CROpportunity.opportunityID>>>>.Select(Base, row.OpportunityID).FirstOrDefault();

			var opportunityIsClosed = opportunity?.IsActive == false;

			bool allowUpdate = row.IsDisabled != true && !opportunityIsClosed && row.Status != PMQuoteStatusAttribute.Closed;

			if (opportunityInDb?.OpportunityID == opportunity?.OpportunityID)
				Base.Caches[typeof(PMQuote)].AllowUpdate = allowUpdate;
			else
			{
				var quoteCache = Base.Caches[typeof(PMQuote)];
				foreach (var field in quoteCache.Fields)
				{
					if (!quoteCache.Keys.Contains(field) && 
						field != quoteCache.GetField(typeof(PMQuote.opportunityID)) &&
						field != quoteCache.GetField(typeof(PMQuote.isPrimary)))
						PXUIFieldAttribute.SetEnabled(sender, row, field, allowUpdate);
				}
			}

			PXUIFieldAttribute.SetEnabled<PMQuote.bAccountID>(sender, row, row.OpportunityID == null);

			Base.Caches[typeof(PMQuote)].AllowDelete = !opportunityIsClosed;
			foreach (var type in new[]
			{
					typeof(CR.CROpportunityDiscountDetail),
					typeof(CR.CROpportunityProducts),
					typeof(CR.CRTaxTran),
					typeof(CR.CRAddress),
					typeof(CR.CRContact),
					typeof(CR.CRPMTimeActivity),
					typeof(PM.PMQuoteTask)
				})
			{
				Base.Caches[type].AllowInsert = Base.Caches[type].AllowUpdate = Base.Caches[type].AllowDelete = allowUpdate;
			}

			Base.Caches[typeof(CopyQuoteFilter)].AllowUpdate = true;

			Base.Caches[typeof(RecalcDiscountsParamFilter)].AllowUpdate = true;

			Base.Actions[nameof(Base.Approval.Submit)]
				.SetVisible(row.Status == PMQuoteStatusAttribute.Draft);

			Base.actionsFolder
				.SetVisible(nameof(Base.Approval.Approve), Base.Actions[nameof(Base.Approval.Approve)].GetVisible());

			Base.actionsFolder
				.SetVisible(nameof(Base.Approval.Reject), Base.Actions[nameof(Base.Approval.Reject)].GetVisible());

			Base.Actions[nameof(EditQuote)]
				.SetVisible(row.Status != PMQuoteStatusAttribute.Draft);

			Base.Actions[nameof(Base.Approval.Submit)]
				.SetEnabled(row.Status == PMQuoteStatusAttribute.Draft && !opportunityIsClosed);

			Base.Actions[nameof(Base.Approval.Approve)]
				.SetEnabled(row.Status == PMQuoteStatusAttribute.PendingApproval);

			Base.Actions[nameof(Base.Approval.Reject)]
				.SetEnabled(row.Status == PMQuoteStatusAttribute.PendingApproval);

			Base.Actions[nameof(EditQuote)]
				.SetEnabled(row.Status != PMQuoteStatusAttribute.Draft && row.Status != PMQuoteStatusAttribute.Closed);

			Base.Actions[nameof(Base.CopyQuote)]
				.SetEnabled(Base.Caches[typeof(PMQuote)].AllowInsert);

			Base.Actions[nameof(PMDiscount.GraphRecalculateDiscountsAction)]
				.SetEnabled((row.Status == PMQuoteStatusAttribute.Draft));

			Base.Actions[nameof(Base.PrimaryQuote)].SetEnabled(!String.IsNullOrEmpty(row.OpportunityID) && row.IsPrimary == false && row.Status != PMQuoteStatusAttribute.Closed);
			Base.Actions[nameof(Base.SendQuote)].SetEnabled(row.Status.IsIn<string>(PMQuoteStatusAttribute.Approved, PMQuoteStatusAttribute.Sent, PMQuoteStatusAttribute.Closed));
			Base.Actions[nameof(Base.PrintQuote)].SetEnabled(true);
			Base.convertToProject.SetEnabled( (row.OpportunityID == null || row.IsPrimary == true) && row.QuoteProjectID == null && row.Status.IsIn<string>(PMQuoteStatusAttribute.Approved, PMQuoteStatusAttribute.Sent));

			PXUIFieldAttribute.SetEnabled<PMQuote.subject>(sender, row, true);
			PXUIFieldAttribute.SetEnabled<PMQuote.status>(sender, row, false);
		}
	}
}
