using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.AP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.GL;

namespace PX.Objects.PO.GraphExtensions.APPaymentEntryExt
{
	public class POAdjustExtension : PXGraphExtension<APPaymentEntry>
	{
		#region Views

		[PXViewName(AR.Messages.OrdersToApply)]
		public PXSelectJoin<
			POAdjust,
				LeftJoin<POOrder,
					On<POOrder.orderType, Equal<POAdjust.adjdOrderType>,
					And<POOrder.orderNbr, Equal<POAdjust.adjdOrderNbr>>>>,
			Where<
				POAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
				And<POAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>>
			POAdjustments;

		public PXSelect<POOrderPrepayment,
				Where<POOrderPrepayment.orderType, Equal<Required<POOrderPrepayment.orderType>>,
					And<POOrderPrepayment.orderNbr, Equal<Required<POOrderPrepayment.orderNbr>>,
					And<POOrderPrepayment.aPDocType, Equal<Current<APPayment.docType>>,
					And<POOrderPrepayment.aPRefNbr, Equal<Current<APPayment.refNbr>>>>>>> poOrderPrepayment;

		public PXFilter<LoadOrdersFilter> LoadOrders;

		#endregion

		#region Initialize

		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		public override void Initialize()
		{
			base.Initialize();

			var selectors = new[]
			{
				new { cache = POAdjustments.Cache, fieldName = nameof(POAdjust.adjdOrderNbr) },
				new { cache = LoadOrders.Cache, fieldName = nameof(LoadOrdersFilter.startOrderNbr) },
				new { cache = LoadOrders.Cache, fieldName = nameof(LoadOrdersFilter.endOrderNbr) },
			}.ToList();

			selectors.ForEach(s =>
			{
				var selector = s.cache.GetAttributes(s.fieldName).OfType<PXSelectorAttribute>().First();

				if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
				{
					selector.WhereAnd(s.cache,
						typeof(Where<POOrder.payToVendorID, Equal<Current<APPayment.vendorID>>>));
				}
				else
				{
					selector.WhereAnd(s.cache,
						typeof(Where<POOrder.vendorID, Equal<Current<APPayment.vendorID>>>));
				}
			});
		}

		#endregion

		#region Buttons

		public PXAction<APPayment> loadPOOrders;
		[PXUIField(DisplayName = Messages.LoadOrders, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public virtual IEnumerable LoadPOOrders(PXAdapter adapter)
		{
			if (LoadOrders.AskExt(
				(graph, view) =>
				{
					LoadOrders.Cache.ClearQueryCacheObsolete();
					LoadOrders.View.Clear();
					LoadOrders.Cache.Clear();
					LoadOrders.View.RequestRefresh();
				}, true) == WebDialogResult.Yes)
			{
				LoadPOOrdersByFilter();
			}

			return adapter.Get();
		}

		protected virtual void LoadPOOrdersByFilter()
		{
			var orderSelect = new PXSelect<POOrder,
				Where<POOrder.status, In3<POOrderStatus.open, POOrderStatus.completed>,
					And<POOrder.orderType, In3<POOrderType.regularOrder, POOrderType.dropShip>>>>(Base);

			var filter = LoadOrders.Current;

			if (filter.BranchID != null)
				orderSelect.WhereAnd<Where<POOrder.branchID, Equal<Current<LoadOrdersFilter.branchID>>>>();

			if (filter.FromDate != null)
				orderSelect.WhereAnd<Where<POOrder.orderDate, GreaterEqual<Current<LoadOrdersFilter.fromDate>>>>();

			if (filter.ToDate != null)
				orderSelect.WhereAnd<Where<POOrder.orderDate, LessEqual<Current<LoadOrdersFilter.toDate>>>>();

			if (filter.StartOrderNbr != null)
				orderSelect.WhereAnd<Where<POOrder.orderNbr, GreaterEqual<Current<LoadOrdersFilter.startOrderNbr>>>>();

			if (filter.EndOrderNbr != null)
				orderSelect.WhereAnd<Where<POOrder.orderNbr, LessEqual<Current<LoadOrdersFilter.endOrderNbr>>>>();

			if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
			{
				orderSelect.WhereAnd<Where<POOrder.payToVendorID, Equal<Current<APPayment.vendorID>>>>();
			}
			else
			{
				orderSelect.WhereAnd<Where<POOrder.vendorID, Equal<Current<APPayment.vendorID>>>>();
			}

			orderSelect.WhereAnd<Where<POOrder.curyUnprepaidTotal, Greater<decimal0>>>();

			if (filter.OrderBy == LoadOrdersFilter.orderBy.ByDate)
				orderSelect.OrderByNew<OrderBy<Asc<POOrder.orderDate, Asc<POOrder.orderNbr>>>>();
			else
				orderSelect.OrderByNew<OrderBy<Asc<POOrder.orderNbr>>>();

			var orders = orderSelect.SelectWindowed(0, filter.MaxNumberOfDocuments ?? 0, filter);

			foreach (POOrder order in orders)
			{
				var newAdjustment = new POAdjust()
				{
					AdjgDocType = Base.Document.Current.DocType,
					AdjgRefNbr = Base.Document.Current.RefNbr,
					AdjdOrderType = order.OrderType,
					AdjdOrderNbr = order.OrderNbr,
					AdjdDocType = POAdjust.EmptyApDocType,
					AdjdRefNbr = string.Empty,
					AdjNbr = Base.Document.Current.AdjCntr
				};

				POAdjust oldAdjustment = new PXSelect<POAdjust,
					Where<POAdjust.adjgDocType, Equal<Current<POAdjust.adjgDocType>>,
						And<POAdjust.adjgRefNbr, Equal<Current<POAdjust.adjgRefNbr>>,
						And<POAdjust.adjdOrderType, Equal<Required<POAdjust.adjdOrderType>>,
						And<POAdjust.adjdOrderNbr, Equal<Required<POAdjust.adjdOrderNbr>>,
						And<POAdjust.released, Equal<False>,
						And<POAdjust.isRequest, Equal<False>>>>>>>>(Base)
						.Select(newAdjustment.AdjdOrderType, newAdjustment.AdjdOrderNbr);

				if (oldAdjustment == null)
				{
					newAdjustment = POAdjustments.Insert(newAdjustment);
				}
				else
				{
					POAdjustments.Cache.RaiseFieldDefaulting<POAdjust.curyAdjgAmt>(oldAdjustment, out object newValue);
					if ((decimal?)newValue < oldAdjustment.CuryAdjgAmt)
					{
						var copy = POAdjustments.Cache.CreateCopy(oldAdjustment);
						POAdjustments.Cache.SetValueExt<POAdjust.curyAdjgAmt>(copy, newValue);
						POAdjustments.Cache.Update(copy);
					}
				}
			}
		}

		#endregion

		#region Overrides

		[PXOverride]
		public virtual IEnumerable<AdjustmentStubGroup> GetAdjustmentsPrintList(Func<IEnumerable<AdjustmentStubGroup>> baseMethod)
		{
			if (Base.Document.Current.DocType != APDocType.Prepayment)
				return baseMethod();

			List<AdjustmentStubGroup> result = AddPOAdjustments(baseMethod());
			var outstandingBalanceRow = GetOutstandingBalanceRow();
			if (outstandingBalanceRow != null)
				result.Add(outstandingBalanceRow);

			return result;
		}

		[PXOverride]
		public virtual void SetDocTypeList(object row, Action<Object> baseMethod)
		{
			baseMethod(row);

			if (row is APPayment payment)
			{
				if (IsPrepaymentCheck(payment))
					AddPrepaymentToDocTypeList();
			}
		}

		[PXOverride]
		public virtual void DeleteUnreleasedApplications(Action baseMethod)
		{
			baseMethod();

			if (Base.Document.Current?.DocType != APDocType.Prepayment)
				return;

			new PXSelectJoin<POAdjust,
				LeftJoin<POOrder,
					On<POOrder.orderType, Equal<POAdjust.adjdOrderType>,
					And<POOrder.orderNbr, Equal<POAdjust.adjdOrderNbr>>>>,
			Where<
				POAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
					And<POAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
					And<POAdjust.released, Equal<False>,
					And<POAdjust.isRequest, Equal<False>>>>>>(Base)
				.SelectMain().ForEach(
					poadjust => POAdjustments.Delete(poadjust));
		}

		[PXOverride]
		public virtual void VoidCheckProc(APPayment doc, Action<APPayment> baseMethod)
		{
			baseMethod(doc);
			Base.Document.Current.CuryPOApplAmt = 0m;
			Base.Document.Update(Base.Document.Current);
		}

		#endregion

		#region APPayment events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		[PXFormula(typeof(Sub<APPayment.curyDocBal, Add<APPayment.curyApplAmt,
			Switch<Case<Where<APPayment.docType, Equal<APDocType.prepayment>>, APPayment.curyPOUnreleasedApplAmt>, decimal0>>>))]
		protected virtual void _(Events.CacheAttached<APPayment.curyUnappliedBal> e)
		{
		}

		protected virtual void _(Events.RowSelected<APPayment> eventArgs)
		{
			if (eventArgs.Row == null) return;

			bool allowUpdate = eventArgs.Row.DocType == APDocType.Prepayment &&
				eventArgs.Row.Status.IsIn(APDocStatus.Hold, APDocStatus.Reserved) &&
				eventArgs.Row.IsMigratedRecord != true &&
				IsPrepaymentCheck(Base.Document.Current);

			POAdjustments.Cache.AllowDelete = allowUpdate;
			POAdjustments.Cache.AllowInsert = allowUpdate;
			POAdjustments.Cache.AllowUpdate = allowUpdate;
			loadPOOrders.SetEnabled(allowUpdate);

			POAdjustments.Cache.AllowSelect = eventArgs.Row.DocType.IsIn(APPaymentType.Prepayment, APPaymentType.VoidCheck);

			if (eventArgs.Row.Hold != true &&
				eventArgs.Row.CuryPOFullApplAmt > eventArgs.Row.CuryOrigDocAmt &&
				eventArgs.Row.DocType == APPaymentType.Prepayment)
			{
				eventArgs.Cache.RaiseExceptionHandling<APPayment.curyPOApplAmt>(
					eventArgs.Row, eventArgs.Row.CuryPOFullApplAmt,
					new PXSetPropertyException<APPayment.curyPOApplAmt>(
						Messages.TotalPrepaymentAmountGreaterDocumentAmount, PXErrorLevel.RowError));
			}
			else
			{
				eventArgs.Cache.RaiseExceptionHandling<APPayment.curyPOApplAmt>(
					eventArgs.Row, eventArgs.Row.CuryPOFullApplAmt, null);
			}

			PXUIFieldAttribute.SetVisible<APPayment.curyPOApplAmt>(eventArgs.Cache, eventArgs.Row, eventArgs.Row.DocType == APDocType.Prepayment);

			if (eventArgs.Row.DocType == APDocType.Prepayment && eventArgs.Row.VendorID != null)
			{
				if (POAdjustments.SelectSingle() != null)
				{
					PXUIFieldAttribute.SetEnabled<APPayment.vendorID>(eventArgs.Cache, eventArgs.Row, false);
				}
			}
		}
		#endregion

		#region APAdjust events

		protected virtual void _(Events.FieldUpdated<APAdjust, APAdjust.curyAdjgAmt> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			InsertUpdatePOAdjust(eventArgs.Row);
		}

		protected virtual void _(Events.RowInserted<APAdjust> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			InsertUpdatePOAdjust(eventArgs.Row);
		}

		protected virtual void _(Events.RowDeleted<APAdjust> eventArgs)
		{
			if (Base.Document.Current?.DocType != APDocType.Prepayment || eventArgs.Row?.AdjdDocType != APDocType.Prepayment)
				return;

			var poadjustmentsSelect = new PXSelect<POAdjust,
				Where<POAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
					And<POAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
					And<POAdjust.adjNbr, Equal<Current<APPayment.adjCntr>>,
					And<POAdjust.adjdDocType, Equal<Required<APInvoice.docType>>,
					And<POAdjust.adjdRefNbr, Equal<Required<APInvoice.refNbr>>>>>>>>(Base);

			var poadjustments = poadjustmentsSelect.SelectMain(eventArgs.Row.AdjdDocType, eventArgs.Row.AdjdRefNbr);

			foreach (POAdjust poadjust in poadjustments)
			{
				poadjust.ForceDelete = true;
				POAdjustments.Delete(poadjust);
			}
		}

		#endregion

		#region POAdjust events

		protected virtual void _(Events.FieldVerifying<POAdjust, POAdjust.curyAdjgAmt> eventArgs)
		{
			if (eventArgs.Row == null || eventArgs.Row.IsRequest == true)
				return;

			var order = POOrder.PK.Find(Base, eventArgs.Row.AdjdOrderType, eventArgs.Row.AdjdOrderNbr);
			if (order != null)
			{
				bool exceed;
				decimal newValue = (decimal?)eventArgs.NewValue ?? 0;

				if (order.CuryID == Base.Document.Current.CuryID)
					exceed = order.CuryPrepaidTotal + newValue > order.CuryUnbilledOrderTotal;
				else
				{
					PXCurrencyAttribute.CuryConvBase<POAdjust.adjgCuryInfoID>(POAdjustments.Cache, eventArgs.Row, newValue, out decimal baseCuryValue);
					exceed = order.PrepaidTotal + baseCuryValue > order.UnbilledOrderTotal;
				}

				if (exceed)
					throw new PXSetPropertyException(Messages.PrepaidTotalGreaterUnbilledOrderTotal, order.OrderNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<POAdjust, POAdjust.curyAdjgAmt> eventArgs)
			=> SetPOAdjustAdjdAmt(eventArgs.Row);

		protected virtual void _(Events.RowSelected<POAdjust> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<POAdjust.curyAdjgAmt>(POAdjustments.Cache, eventArgs.Row,
				eventArgs.Row.IsRequest != true && eventArgs.Row.Released != true);

			PXUIFieldAttribute.SetEnabled<POAdjust.adjdOrderType>(POAdjustments.Cache, eventArgs.Row,
				eventArgs.Row.AdjdOrderNbr == null);

			PXUIFieldAttribute.SetEnabled<POAdjust.adjdOrderNbr>(POAdjustments.Cache, eventArgs.Row,
				eventArgs.Row.AdjdOrderNbr == null);
		}

		protected virtual void _(Events.FieldDefaulting<POAdjust, POAdjust.curyAdjgAmt> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			var order = POOrder.PK.Find(Base, eventArgs.Row.AdjdOrderType, eventArgs.Row.AdjdOrderNbr);

			if (order != null)
			{
				if (order.CuryID == Base.Document.Current.CuryID)
				{
					eventArgs.NewValue = order.CuryUnprepaidTotal;
				}
				else
				{
					PXCurrencyAttribute.CuryConvCury(Base.Document.Cache, Base.Document.Current, order.UnprepaidTotal ?? 0, out decimal curyamount);
					eventArgs.NewValue = curyamount;
				}
			}
		}

		protected virtual void _(Events.RowDeleting<POAdjust> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			if (eventArgs.Row.Released == true && eventArgs.Row.ForceDelete != true)
				throw new PXException(Messages.CanNotRemoveReferenceToReleasedPOOrder, eventArgs.Row.AdjdOrderNbr);

			if (eventArgs.Row.IsRequest == true 
				&& eventArgs.Row.AdjgDocType != APPaymentType.VoidCheck 
				&& eventArgs.Row.ForceDelete != true)
				throw new PXException(Messages.CanNotRemoveReferenceToPOOrderRelatedPrepayment, eventArgs.Row.AdjdOrderNbr, eventArgs.Row.AdjdRefNbr);
		}

		protected virtual void _(Events.RowDeleted<POAdjust> eventArgs)
		{
			if (eventArgs.Row == null ||
				eventArgs.Row.AdjdOrderType == null ||
				eventArgs.Row.AdjdOrderNbr == null)
				return;

			POAdjust poadjust = new PXSelect<POAdjust,
				Where<
					POAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
						And<POAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
						And<POAdjust.adjdOrderType, Equal<Required<POAdjust.adjdOrderType>>,
						And<POAdjust.adjdOrderNbr, Equal<Required<POAdjust.adjdOrderNbr>>,
						And<POAdjust.isRequest, NotEqual<True>>>>>>>(Base)
				.Select(eventArgs.Row.AdjdOrderType, eventArgs.Row.AdjdOrderNbr);

			if (poadjust == null)
			{
				var orderPrepayment = poOrderPrepayment.SelectSingle(eventArgs.Row.AdjdOrderType, eventArgs.Row.AdjdOrderNbr);

				if (orderPrepayment != null && orderPrepayment.IsRequest != true)
					poOrderPrepayment.Delete(orderPrepayment);
			}
		}

		protected virtual void _(Events.FieldUpdated<POAdjust, POAdjust.adjdOrderNbr> eventArgs)
		{
			if (eventArgs.Row != null &&
				eventArgs.OldValue != eventArgs.NewValue &&
				eventArgs.Row.AdjdOrderType != null &&
				eventArgs.Row.AdjdOrderNbr != null &&
				Base.Document.Current?.DocType == APDocType.Prepayment &&
				eventArgs.Row.IsRequest != true)
			{
				POAdjustments.Cache.SetDefaultExt<POAdjust.curyAdjgAmt>(eventArgs.Row);

				var orderPrepayment = poOrderPrepayment.SelectSingle(eventArgs.Row.AdjdOrderType, eventArgs.Row.AdjdOrderNbr);

				if (orderPrepayment == null)
				{
					orderPrepayment = poOrderPrepayment.Insert(new POOrderPrepayment()
					{
						OrderType = eventArgs.Row.AdjdOrderType,
						OrderNbr = eventArgs.Row.AdjdOrderNbr,
						APDocType = Base.Document.Current.DocType,
						APRefNbr = Base.Document.Current.RefNbr,
						IsRequest = false,
						CuryAppliedAmt = 0m,
					});
				}
			}
		}

		#endregion

		protected virtual void SetPOAdjustAdjdAmt(POAdjust row)
		{
			if (row == null)
				return;

			var order = POOrder.PK.Find(Base, row.AdjdOrderType, row.AdjdOrderNbr);

			if (order != null)
			{
				if (order.CuryID == Base.Document.Current.CuryID)
				{
					POAdjustments.Cache.SetValueExt<POAdjust.curyAdjdAmt>(row, row.CuryAdjgAmt);
				}
				else
				{
					PXCurrencyAttribute.CuryConvBase<POAdjust.adjgCuryInfoID>(POAdjustments.Cache, row, row.CuryAdjgAmt ?? 0, out decimal baseCuryAmt);
					PXCurrencyAttribute.CuryConvCury(Base.Caches<POOrder>(), order, baseCuryAmt, out decimal orderCuryAmt);
					POAdjustments.Cache.SetValueExt<POAdjust.curyAdjdAmt>(row, orderCuryAmt);
				}
			}

			POAdjustments.Cache.SetValueExt<POAdjust.adjdAmt>(row, row.AdjgAmt);
		}

		protected virtual List<AdjustmentStubGroup> AddPOAdjustments(IEnumerable<AdjustmentStubGroup> adjustmentStubs)
		{
			var poadjustments = POAdjustments.SelectMain();

			var result = adjustmentStubs.Where(r => !poadjustments.Any(
				p => p.AdjdDocType == r.GroupedStubs.Key.AdjdDocType && p.AdjdRefNbr == r.GroupedStubs.Key.AdjdRefNbr))
				.Union(
				poadjustments.GroupBy(adj => new AdjustmentGroupKey
				{
					Source = AdjustmentGroupKey.AdjustmentType.POAdjustment,
					AdjdDocType = adj.AdjdOrderType,
					AdjdRefNbr = adj.AdjdOrderNbr,
					AdjdCuryInfoID = adj.AdjdCuryInfoID
				},
					adj => (IAdjustmentStub)adj).Select(g => new AdjustmentStubGroup() { GroupedStubs = g })).ToList();

			return result;
		}

		protected virtual AdjustmentStubGroup GetOutstandingBalanceRow()
		{
			decimal? outstandingBalance = Base.Document.Current.CuryUnappliedBal;

			if (outstandingBalance != null && outstandingBalance != 0)
			{
				var outstandingBalanceRow = new OutstandingBalanceRow();
				outstandingBalanceRow.CuryOutstandingBalance = outstandingBalance;
				outstandingBalanceRow.OutstandingBalanceDate = Base.Accessinfo.BusinessDate;

				var result = new IAdjustmentStub[] { outstandingBalanceRow }.GroupBy(adj =>
					new AdjustmentGroupKey
					{
						Source = AdjustmentGroupKey.AdjustmentType.OutstandingBalance,
						AdjdDocType = POAdjust.EmptyApDocType,
						AdjdRefNbr = string.Empty,
						AdjdCuryInfoID = Base.Document.Current.CuryInfoID
					}, adj => adj);

				return new AdjustmentStubGroup() { GroupedStubs = result.Single() };
			}

			return null;
		}

		protected virtual void AddPrepaymentToDocTypeList()
		{
			PXStringListAttribute attribute = Base.Adjustments.Cache.GetAttributesReadonly<APAdjust.adjdDocType>(null)
				.OfType<PXStringListAttribute>().FirstOrDefault();

			if (attribute != null)
			{
				var allowedValues = new List<string>(attribute.ValueLabelDic.Keys);
				var allowedLabels = new List<string>(attribute.ValueLabelDic.Values);

				allowedValues.Add(APDocType.Prepayment);
				allowedLabels.Add(AP.Messages.Prepayment);

				PXStringListAttribute.SetList<APAdjust.adjdDocType>(Base.Adjustments.Cache, null, allowedValues.ToArray(), allowedLabels.ToArray());
			}
		}

		protected virtual bool IsPrepaymentCheck(APPayment payment)
		{
			bool prepayment = payment.DocType == APDocType.Prepayment;
			bool fromPO = payment.OrigModule == BatchModule.PO;

			if (prepayment && fromPO)
				return false;

			if (prepayment)
				return !Base.IsRequestPrepayment(payment);

			return false;
		}

		[PXOverride]
		public virtual void RecalcApplAmounts(PXCache sender, APPayment row, bool aReadOnly, Action<PXCache, APPayment, bool> baseMethod)
		{
			PXFormulaAttribute.CalcAggregate<POAdjust.curyAdjgAmt>(POAdjustments.Cache, row, aReadOnly);
			baseMethod?.Invoke(sender, row, aReadOnly);
		}
		
		protected virtual void InsertUpdatePOAdjust(APAdjust apadjust)
		{
			if (Base.Document.Current?.DocType.IsNotIn(APDocType.Prepayment, APDocType.VoidCheck) == true ||
				apadjust?.AdjdDocType != APDocType.Prepayment)
				return;

			var prepaymentRequests = new PXSelectJoin<POOrderPrepayment,
				InnerJoin<POOrder, On<POOrderPrepayment.orderType, Equal<POOrder.orderType>, And<POOrderPrepayment.orderNbr, Equal<POOrder.orderNbr>>>>,
				Where<POOrderPrepayment.aPDocType, Equal<Required<APAdjust.adjdDocType>>,
					And<POOrderPrepayment.aPRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
					And<POOrderPrepayment.aPDocType, Equal<APDocType.prepayment>>>>>(Base)
					.Select(apadjust.AdjdDocType, apadjust.AdjdRefNbr);

			foreach (PXResult<POOrderPrepayment, POOrder> row in prepaymentRequests)
			{
				POOrder order = row;
				POOrderPrepayment prepaymentRequest = row;

				POAdjust poadjustment = new PXSelect<POAdjust,
					Where<POAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
						And<POAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
						And<POAdjust.adjNbr, Equal<Current<APPayment.adjCntr>>,
						And<POAdjust.adjdDocType, Equal<Required<APInvoice.docType>>,
						And<POAdjust.adjdRefNbr, Equal<Required<APInvoice.refNbr>>>>>>>>(Base)
						.Select(apadjust.AdjdDocType, apadjust.AdjdRefNbr);

				if (poadjustment == null)
				{
					poadjustment = POAdjustments.Insert(new POAdjust()
					{
						AdjgDocType = Base.Document.Current.DocType,
						AdjgRefNbr = Base.Document.Current.RefNbr,
						AdjdOrderType = prepaymentRequest.OrderType,
						AdjdOrderNbr = prepaymentRequest.OrderNbr,
						AdjdDocType = apadjust.AdjdDocType,
						AdjdRefNbr = apadjust.AdjdRefNbr,
						AdjNbr = Base.Document.Current.AdjCntr,
						IsRequest = true,
					});
				}

				if (order.CuryID == Base.Document.Current.CuryID)
				{
					POAdjustments.Cache.SetValueExt<POAdjust.curyAdjgAmt>(poadjustment, apadjust.CuryAdjgAmt);
				}
				else
				{
					PXCurrencyAttribute.CuryConvCury(Base.Document.Cache, Base.Document.Current, apadjust.AdjAmt ?? 0, out decimal curyamount);
					POAdjustments.Cache.SetValueExt<POAdjust.curyAdjgAmt>(poadjustment, curyamount);
				}

				POAdjustments.Update(poadjustment);
			}
		}
	}
}
