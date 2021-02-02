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
using static PX.Objects.CR.QuoteMaint;

namespace PX.Objects.CR
{
    public class QuoteMaintExt : PXGraphExtension<Discount, QuoteMaint>
    {
        public override void Initialize()
        {
			Base.actionsFolder.AddMenuAction(Base.Actions[nameof(Base.Approval.Approve)]);
			Base.actionsFolder.AddMenuAction(Base.Actions[nameof(Base.Approval.Reject)]);

			Base.Actions[nameof(Base.Approval.Submit)].SetCaption(Messages.SummitForApproval);

			PXCache sender = Base.Quote.Cache;     
            var row = sender.Current as CRQuote;    

            if (row != null)
			{
				VisibilityHandler(sender, row);
			}

            Base.Actions.Move(nameof(Base.actionsFolder), nameof(Base.Approval.Submit), true);
            Base.Actions.Move(nameof(Base.actionsFolder), nameof(EditQuote), true);

			Base.Approval.StatusHandler =
				(item) =>
				{
					switch (Base.Approval.GetResult(item))
					{
						case ApprovalResult.Approved:
							item.Status = CRQuoteStatusAttribute.Approved;
							break;

						case ApprovalResult.Rejected:
							item.Status = CRQuoteStatusAttribute.Rejected;
							break;

						case ApprovalResult.PendingApproval:
							item.Status = CRQuoteStatusAttribute.PendingApproval;
							break;

						case ApprovalResult.Submitted:
							item.Status = CRQuoteStatusAttribute.Approved;
							break;
					}
				};
        }
		
        public PXAction<CRQuote> EditQuote;
        [PXUIField(DisplayName = Messages.EditQuote, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable editQuote(PXAdapter adapter)
        {
            CRQuote row = Base.Quote.Current;

			if (row == null) return adapter.Get();

			Base.Approval.Reset(row);

			row.Approved = false;
			row.Rejected = false;
			row.Status = CRQuoteStatusAttribute.Draft;
                
			Base.Quote.Cache.Update(row); 
				
			Base.Save.Press();

			return adapter.Get();
        }                  

        protected virtual void CRQuote_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected sel)
        {            
			sel?.Invoke(sender, e);

            CRQuote row = e.Row as CRQuote;
			if (row == null) return;
                
			VisibilityHandler(sender, row);
		}

		private void VisibilityHandler(PXCache sender, CRQuote row)
		{
			Standalone.CROpportunity opportunity = PXSelect<Standalone.CROpportunity,
				Where<Standalone.CROpportunity.opportunityID, Equal<Required<Standalone.CROpportunity.opportunityID>>>>.Select(Base, row.OpportunityID).FirstOrDefault();

			if (opportunity != null)
			{
				bool allowUpdate = row.IsDisabled != true && opportunity.IsActive == true;
				Base.Caches[typeof(CRQuote)].AllowUpdate = allowUpdate;

				Base.Caches[typeof(CRQuote)].AllowInsert = Base.Caches[typeof(CRQuote)].AllowDelete =
					 opportunity.IsActive == true;
				
				foreach (var type in new[]
				{
					typeof(CROpportunityDiscountDetail),
					typeof(CROpportunityProducts),
					typeof(CRTaxTran),
					typeof(CRAddress),
					typeof(CRContact)
				})
				{
					Base.Caches[type].AllowInsert = Base.Caches[type].AllowUpdate = Base.Caches[type].AllowDelete = allowUpdate;
				}

				Base.Caches[typeof(CopyQuoteFilter)].AllowUpdate = true;
				Base.Caches[typeof(RecalcDiscountsParamFilter)].AllowUpdate = true;
				


				Base.Actions[nameof(Base.Approval.Submit)]
					.SetVisible(row.Status == CRQuoteStatusAttribute.Draft);
				
				Base.actionsFolder
					.SetVisible(nameof(Base.Approval.Approve), Base.Actions[nameof(Base.Approval.Approve)].GetVisible());
				
				Base.actionsFolder
					.SetVisible(nameof(Base.Approval.Reject), Base.Actions[nameof(Base.Approval.Reject)].GetVisible());

				Base.Actions[nameof(EditQuote)]
					.SetVisible(row.Status != CRQuoteStatusAttribute.Draft);


				
				Base.Actions[nameof(Base.Approval.Submit)]
					.SetEnabled(row.Status == CRQuoteStatusAttribute.Draft && opportunity.IsActive == true);
				
				Base.Actions[nameof(Base.Approval.Approve)]
					.SetEnabled(row.Status == CRQuoteStatusAttribute.PendingApproval);
				
				Base.Actions[nameof(Base.Approval.Reject)]
					.SetEnabled(row.Status == CRQuoteStatusAttribute.PendingApproval);
				
				Base.Actions[nameof(EditQuote)]
					.SetEnabled(row.Status != CRQuoteStatusAttribute.Draft);
				
				Base.Actions[nameof(Base.SendQuote)]
					.SetEnabled((row.Status == CRQuoteStatusAttribute.Approved || row.Status == CRQuoteStatusAttribute.Sent) && !String.IsNullOrEmpty(row.OpportunityID));
				
				Base.Actions[nameof(Base.PrintQuote)]
					.SetEnabled((row.Status == CRQuoteStatusAttribute.Approved || row.Status == CRQuoteStatusAttribute.Sent) && !String.IsNullOrEmpty(row.OpportunityID));
				
				Base.Actions[nameof(Base.CopyQuote)]
					.SetEnabled(Base.Caches[typeof(CRQuote)].AllowInsert);
				
				Base.Actions[nameof(Discount.GraphRecalculateDiscountsAction)]
					.SetEnabled((row.Status == CRQuoteStatusAttribute.Draft) && !String.IsNullOrEmpty(row.OpportunityID));



				PXUIFieldAttribute.SetEnabled<CRQuote.subject>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<CRQuote.status>(sender, row, false);
			}
		}
	}
}
