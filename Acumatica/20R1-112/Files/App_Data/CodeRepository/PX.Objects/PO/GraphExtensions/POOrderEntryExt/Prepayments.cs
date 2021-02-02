using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.TX;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	public class Prepayments : PXGraphExtension<POOrderEntry>
	{
		public PXSelectJoin<POOrderPrepayment,
			InnerJoin<APRegister, On<APRegister.docType, Equal<POOrderPrepayment.aPDocType>, And<APRegister.refNbr, Equal<POOrderPrepayment.aPRefNbr>>>>,
			Where<POOrderPrepayment.orderType, Equal<Current<POOrder.orderType>>, And<POOrderPrepayment.orderNbr, Equal<Current<POOrder.orderNbr>>>>>
			PrepaymentDocuments;

		public PXAction<POOrder> createPrepayment;

		[PXUIField(DisplayName = "Create Prepayment Request", MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual void CreatePrepayment()
		{
			if (Base.Document.Current != null)
			{
				Base.Save.Press();
				var target = PXGraph.CreateInstance<APInvoiceEntry>();
				var prepaymentExt = target.GetExtension<APInvoiceEntryExt.Prepayments>();
				prepaymentExt.AddPOOrderProc(Base.Document.Current, true);

				throw new PXPopupRedirectException(target, "New Prepayment", true);
			}
		}

		protected virtual void _(Events.RowSelected<POOrder> e)
		{
			createPrepayment.SetEnabled(e.Row?.CuryUnprepaidTotal > 0m);
		}

		protected virtual void _(Events.FieldSelecting<POOrderPrepayment.statusText> e)
		{
			if (Base.Document.Current == null || e.Row == null)
				return;

			var query =
				new PXSelectJoinGroupBy<POOrderPrepayment,
					InnerJoin<APRegister, On<APRegister.docType, Equal<POOrderPrepayment.aPDocType>, And<APRegister.refNbr, Equal<POOrderPrepayment.aPRefNbr>>>>,
					Where<POOrderPrepayment.orderType, Equal<Current<POOrder.orderType>>, And<POOrderPrepayment.orderNbr, Equal<Current<POOrder.orderNbr>>>>,
					Aggregate<Sum<POOrderPrepayment.curyAppliedAmt, Sum<APRegister.curyDocBal>>>>(Base);

			using (new PXFieldScope(query.View,
				typeof(POOrderPrepayment.orderType), typeof(POOrderPrepayment.orderNbr), typeof(POOrderPrepayment.aPDocType), typeof(POOrderPrepayment.aPRefNbr),
				typeof(APRegister.docType), typeof(APRegister.refNbr),
				typeof(POOrderPrepayment.curyAppliedAmt), typeof(APRegister.curyDocBal)))
			{
				var prepaymentTotal = (PXResult<POOrderPrepayment, APRegister>)query.SelectWindowed(0, 1);

				e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.StatusTotalPrepayments,
					Base.FormatQty(((POOrderPrepayment)prepaymentTotal)?.CuryAppliedAmt ?? 0),
					Base.FormatQty(((APRegister)prepaymentTotal)?.CuryDocBal ?? 0));
			}
		}
	}
}
