using System;
using System.Collections.Generic;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.TX;

namespace PX.Objects.AM.Attributes
{
    public class AMSOTaxAttribute : TaxAttribute
    {
		protected virtual short SortOrder
		{
			get
			{
				return 0;
			}
		}

		public AMSOTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
            this.CuryDocBal = null;
            this.CuryTaxTotal = typeof(SOOrder.curyTaxTotal);
            this.DocDate = typeof(SOOrder.orderDate);
            this.CuryLineTotal = typeof(SOOrderExt.aMCuryEstimateTotal);
            this.CuryTranAmt = typeof(AMEstimateReference.curyExtPrice);

            this._Attributes.Add(new PXUnboundFormulaAttribute(null, typeof(SumCalc<SOOrderExt.aMCuryEstimateTotal>)));
        }

		public override int CompareTo(object other)
		{
			return this.SortOrder.CompareTo(((AMSOTaxAttribute)other).SortOrder);
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			decimal val = 0m;
			val = (base.GetCuryTranAmt(sender, row) ?? 0m) * ((decimal?)sender.GetValue(row, _GroupDiscountRate) ?? 1m) * ((decimal?)sender.GetValue(row, _DocumentDiscountRate) ?? 1m);
            return PXDBCurrencyAttribute.Round(sender, row, val, CMPrecision.TRANCURY);
		}

		protected override void CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscountTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);

            //need to include estimate $
            decimal CuryEstimatetotal = (decimal)(ParentGetValue<SOOrderExt.aMCuryEstimateTotal>(sender.Graph) ?? 0m);

            decimal CuryDocTotal = CuryEstimatetotal + CuryLineTotal + CuryMiscTotal + CuryFreightTotal + CuryTaxTotal - CuryInclTaxTotal - CuryDiscountTotal;

			if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<SOOrder.curyOrderTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyOrderTotal>(sender.Graph, CuryDocTotal);
			}
		}

		protected virtual bool IsFreightTaxable(PXCache sender, List<object> taxitems)
		{
			for (int i = 0; i < taxitems.Count; i++)
			{
				if (((SOTax)(PXResult<SOTax>)taxitems[i]).LineNbr == 32000)
				{
					return true;
				}
			}
			return false;
		}

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
			decimal CuryLineTotal = (decimal?)ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m;
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscountTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);

			CuryLineTotal += CuryMiscTotal;

			//24565 do not protate discount if lineamt+freightamt = taxableamt
			//27214 do not prorate discount on freight separated from document lines, i.e. taxable by different tax than document lines
			decimal CuryTaxableFreight = IsFreightTaxable(sender, taxitems) ? CuryFreightTotal : 0m;

			if (CuryLineTotal != 0m && CuryTaxableAmt != 0m)
			{
				if (Math.Abs(CuryTaxableAmt - CuryTaxableFreight - CuryLineTotal) < 0.00005m)
				{
					CuryTaxableAmt -= CuryDiscountTotal;
				}
			}
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<AMEstimateReference.taxLineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override object Insert(PXCache sender, object item)
		{
			return InsertCached(sender, item);
		}

		public override object Delete(PXCache sender, object item)
		{
			return DeleteCached(sender, item);
		}

		protected virtual object InsertCached(PXCache sender, object item)
		{
			List<object> recordsList = getRecordsList(sender);

			PXCache pcache = sender.Graph.Caches[typeof(SOOrder)];
			object OrderType = pcache.GetValue<SOOrder.orderType>(pcache.Current);
			object OrderNbr = pcache.GetValue<SOOrder.orderNbr>(pcache.Current);

			List<PXRowInserted> insertedHandlersList = storeCachedInsertList(sender, recordsList, OrderType, OrderNbr);
			List<PXRowDeleted> deletedHandlersList = storeCachedDeleteList(sender, recordsList, OrderType, OrderNbr);

			try
			{
				return sender.Insert(item);
			}
			finally
			{
				foreach (PXRowInserted handler in insertedHandlersList)
					sender.Graph.RowInserted.RemoveHandler<SOTax>(handler);
				foreach (PXRowDeleted handler in deletedHandlersList)
					sender.Graph.RowDeleted.RemoveHandler<SOTax>(handler);
			}
		}

		protected virtual object DeleteCached(PXCache sender, object item)
		{
			List<object> recordsList = getRecordsList(sender);

			PXCache pcache = sender.Graph.Caches[typeof(SOOrder)];
			object OrderType = pcache.GetValue<SOOrder.orderType>(pcache.Current);
			object OrderNbr = pcache.GetValue<SOOrder.orderNbr>(pcache.Current);

			List<PXRowInserted> insertedHandlersList = storeCachedInsertList(sender, recordsList, OrderType, OrderNbr);
			List<PXRowDeleted> deletedHandlersList = storeCachedDeleteList(sender, recordsList, OrderType, OrderNbr);

			try
			{
				return sender.Delete(item);
			}
			finally
			{
				foreach (PXRowInserted handler in insertedHandlersList)
					sender.Graph.RowInserted.RemoveHandler<SOTax>(handler);
				foreach (PXRowDeleted handler in deletedHandlersList)
					sender.Graph.RowDeleted.RemoveHandler<SOTax>(handler);
			}
		}

		protected List<Object> getRecordsList(PXCache sender)
		{
			List<Object> recordsList = new List<object>();

			recordsList = new List<object>(PXSelect<SOTax,
				Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>.SelectMultiBound(sender.Graph, new object[] { sender.Graph.Caches[typeof(SOTax).Name].Current }).RowCast<SOTax>());

			return recordsList;
		}

		protected List<PXRowInserted> storeCachedInsertList(PXCache sender, List<Object> recordsList, object OrderType, object OrderNbr)
		{
			List<PXRowInserted> handlersList = new List<PXRowInserted>();

			PXRowInserted inserted = delegate (PXCache cache, PXRowInsertedEventArgs e)
			{
				recordsList.Add(e.Row);

				PXSelect<SOTax,
						Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>.StoreCached(sender.Graph, new PXCommandKey(new object[] { OrderType, OrderNbr }), recordsList);
			};
			sender.Graph.RowInserted.AddHandler<SOTax>(inserted);
			handlersList.Add(inserted);

			return handlersList;
		}

		protected List<PXRowDeleted> storeCachedDeleteList(PXCache sender, List<Object> recordsList, object OrderType, object OrderNbr)
		{
			List<PXRowDeleted> handlersList = new List<PXRowDeleted>();

			PXRowDeleted deleted = delegate (PXCache cache, PXRowDeletedEventArgs e)
			{
				recordsList.Remove(e.Row);

				PXSelect<SOTax,
						Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>.StoreCached(sender.Graph, new PXCommandKey(new object[] { OrderType, OrderNbr }), recordsList);
			};

			sender.Graph.RowDeleted.AddHandler<SOTax>(deleted);
			handlersList.Add(deleted);

			return handlersList;
		}

		protected List<object> SelectTaxes<Where, LineNbr>(PXGraph graph, object[] currents, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new()
			where LineNbr : IBqlOperand
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And<TaxRev.taxType, Equal<TaxType.sales>,
					And<Tax.taxType, NotEqual<CSTaxType.withholding>,
					And<Tax.taxType, NotEqual<CSTaxType.use>,
					And<Tax.reverseTax, Equal<boolFalse>,
					And<Current<SOOrder.orderDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			Type fieldLineNbr;
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					int? linenbr = int.MinValue;

					if (currents.Length > 0
						&& currents[0] != null
						&& typeof(LineNbr).IsGenericType
						&& typeof(LineNbr).GetGenericTypeDefinition() == typeof(Current<>)
						&& (fieldLineNbr = typeof(LineNbr).GetGenericArguments()[0]).IsNested
						&& currents[0].GetType() == BqlCommand.GetItemType(fieldLineNbr)
						)
					{
						linenbr = (int?)graph.Caches[BqlCommand.GetItemType(fieldLineNbr)].GetValue(currents[0], fieldLineNbr.Name);

					}

					if (typeof(IConstant<int>).IsAssignableFrom(typeof(LineNbr)))
					{
						linenbr = ((IConstant<int>)Activator.CreateInstance(typeof(LineNbr))).Value;
					}

					foreach (SOTax record in PXSelect<SOTax,
						Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						if (record.LineNbr == linenbr)
						{
							PXResult<Tax, TaxRev> line;
							if (tail.TryGetValue(record.TaxID, out line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0)
									&& String.Compare(((Tax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
									idx--) ;
								Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);
								ret.Insert(idx, new PXResult<SOTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
							}
						}
                    }

					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (SOTax record in PXSelect<SOTax,
						Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						//resultset is always sorted by LineNbr
						if (record.LineNbr == int.MaxValue) break;

						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((SOTax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);
							ret.Insert(idx, new PXResult<SOTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (SOTaxTran record in PXSelect<SOTaxTran,
						Where<SOTaxTran.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOTaxTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<SOTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);
							ret.Insert(idx, new PXResult<SOTaxTran, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.RowInserted(sender, e);

			if (TaxCalc == TaxCalc.ManualLineCalc)
			{
				DefaultTaxes(sender, e.Row);
				CalcTaxes(sender, e.Row, PXTaxCheck.Line);
			}

		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
            base.RowUpdated(sender, e);

			var estReference = (AMEstimateReference) e.Row;
			if (estReference != null && estReference.TaxLineNbr == null)
			{
				return;
			}

			if (TaxCalc == TaxCalc.ManualLineCalc)
			{
				DefaultTaxes(sender, e.Row);
				CalcTaxes(sender, e.Row, PXTaxCheck.Line);
			}
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//Test, confirm delete is working?
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}

		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();

			inserted = new Dictionary<object, object>();
			updated = new Dictionary<object, object>();

			if (sender.Graph is SOOrderEntry)
			{
				base.CacheAttached(sender);
				sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), "AMCuryEstimateTotal", SOOrder_AMCuryEstimateTotal_FieldUpdated);
			}
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

		protected virtual void SOOrder_AMCuryEstimateTotal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (SOOrder)e.Row;
			if (row == null)
			{
				return;
			}

			var rowStatus = sender.GetStatus(row);
			if (rowStatus == PXEntryStatus.Deleted || rowStatus == PXEntryStatus.InsertedDeleted)
			{
				return;
			}

			var oldValue = (decimal)(e.OldValue ?? 0m);
			var newValue = (decimal)(ParentGetValue<SOOrderExt.aMCuryEstimateTotal>(sender.Graph) ?? 0m);

			var diff = newValue - oldValue;

			if (diff == 0)
			{
				return;
			}

            decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
            decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
            _ParentRow = row;
            CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
            _ParentRow = null;
        }
    }
}
