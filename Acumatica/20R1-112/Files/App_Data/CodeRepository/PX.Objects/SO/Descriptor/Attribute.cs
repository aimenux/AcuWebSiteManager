using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Common;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.GL;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;
using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using System.Globalization;
using PX.Objects.CM;
using PX.Objects.CA;
using System.Linq;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Data.BQL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.SO
{
	public class Round<Field, CuryKeyField> : BqlFormulaEvaluator<Field, CuryKeyField>, IBqlOperand
		where Field : IBqlOperand
		where CuryKeyField : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			decimal? val = (decimal?)pars[typeof(Field)];
			return val != null ? (object) PXDBCurrencyAttribute.RoundCury<CuryKeyField>(cache, item, (decimal) val) : null;
		}
	}

	public static class Constants
	{
		public const string NoShipmentNbr = "<NEW>";

		public class noShipmentNbr : PX.Data.BQL.BqlString.Constant<noShipmentNbr>
		{
			public noShipmentNbr() : base(NoShipmentNbr) { ;}
		}

		public const int MaxNumberOfPaymentsAndMemos = 200;
	}

	public class SOSetupSelect : PXSetupSelect<SOSetup>
	{
		public SOSetupSelect(PXGraph graph)
			:base(graph)
		{
		}

		protected override void FillDefaultValues(SOSetup record)
		{
			record.MinGrossProfitValidation = MinGrossProfitValidationType.Warning;
		}
	}

	public class OrderSiteSelectorAttribute : PXSelectorAttribute
	{
		protected string _InputMask = null;

		public OrderSiteSelectorAttribute()
			: base(typeof(Search2<SOOrderSite.siteID, 
				InnerJoin<INSite, 
					On<SOOrderSite.FK.Site>>,
				Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>, 
				And<Match<INSite, Current<AccessInfo.userName>>>>>>),
				typeof(INSite.siteCD), typeof(INSite.descr), typeof(INSite.replenishmentClassID)
			)
		{
			this.DirtyRead = true;
			this.SubstituteKey = typeof(INSite.siteCD);
			this.DescriptionField = typeof(INSite.descr);
			this._UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteID, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>));
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			PXDimensionAttribute attr = new PXDimensionAttribute(SiteAttribute.DimensionName);
			attr.CacheAttached(sender);
			attr.FieldName = _FieldName;
			PXFieldSelectingEventArgs e = new PXFieldSelectingEventArgs(null, null, true, false);
			attr.FieldSelecting(sender, e);

			_InputMask = ((PXSegmentedState)e.ReturnState).InputMask;
		}

		public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.SubstituteKeyFieldSelecting(sender, e);
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, null, null, null, _InputMask, null, null, null, null);
			}
		}
	}

	/// <summary>
	/// Specialized for SOInvoice version of the InvoiceNbrAttribute.<br/>
	/// The main purpose of the attribute is poviding of the uniqueness of the RefNbr <br/>
	/// amoung  a set of  documents of the specifyed types (for example, each RefNbr of the ARInvoice <br/>
	/// the ARInvoices must be unique across all ARInvoices and AR Debit memos)<br/>
	/// This may be useful, if user has configured a manual numberin for SOInvoices  <br/>
	/// or needs  to create SOInvoice from another document (like SOOrder) allowing to type RefNbr <br/>
	/// for the to-be-created Invoice manually. To store the numbers, system using ARInvoiceNbr table, <br/>
	/// keyed uniquelly by DocType and RefNbr. A source document is linked to a number by NoteID.<br/>
	/// Attributes checks a number for uniqueness on FieldVerifying and RowPersisting events.<br/>
	/// </summary>
	public class SOInvoiceNbrAttribute : InvoiceNbrAttribute
	{
		public SOInvoiceNbrAttribute()
			: base(typeof(SOOrder.aRDocType), typeof(SOOrder.noteID))
		{
		}

		protected override bool DeleteOnUpdate(PXCache sender, PXRowPersistedEventArgs e)
		{
			return base.DeleteOnUpdate(sender, e) || (bool?)sender.GetValue<SOOrder.cancelled>(e.Row) == true;
		}
	}

	/// <summary>
	/// Automatically tracks and Updates Cash discounts in accordance with Customer's Credit Terms. 
	/// </summary>
	public class SOInvoiceTermsAttribute : TermsAttribute
	{
		public SOInvoiceTermsAttribute()
			: base(typeof(ARInvoice.docDate), typeof(ARInvoice.dueDate), typeof(ARInvoice.discDate), null, null)
		{ 
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			SubscribeCalcDisc(sender);
			sender.Graph.FieldVerifying.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyOrigDiscAmt).Name, VerifyDiscount<ARInvoice.curyOrigDocAmt>);
			sender.Graph.FieldVerifying.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyOrigDiscAmt).Name, VerifyDiscount<ARInvoice.curyDocBal>);

			_CuryDiscBal = typeof(ARInvoice.curyOrigDiscAmt);
		}

		public override void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			base.FieldUpdated(sender, e);
			CalcDisc_CuryOrigDocAmt(sender, e);
			CalcDisc_CuryDocBal(sender, e);
		}

		protected override void UnsubscribeCalcDisc(PXCache sender)
		{
			sender.Graph.FieldUpdated.RemoveHandler(typeof(ARInvoice), typeof(ARInvoice.curyOrigDocAmt).Name, CalcDisc_CuryOrigDocAmt);
			sender.Graph.FieldUpdated.RemoveHandler(typeof(ARInvoice), typeof(ARInvoice.curyDocBal).Name, CalcDisc_CuryDocBal);
		}

		protected override void SubscribeCalcDisc(PXCache sender)
			{
			sender.Graph.FieldUpdated.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyOrigDocAmt).Name, CalcDisc_CuryOrigDocAmt);
			sender.Graph.FieldUpdated.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyDocBal).Name, CalcDisc_CuryDocBal);
		}

		public void CalcDisc_CuryOrigDocAmt(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            if (((ARInvoice)e.Row).DocType != ARDocType.CashSale && ((ARInvoice)e.Row).DocType != ARDocType.CashReturn)
			{
			_CuryDocBal = typeof(ARInvoice.curyOrigDocAmt);
			}

			try
			{
				base.CalcDisc(sender, e);
			}
			finally
			{
				_CuryDocBal = null;
			}
			}

		public void CalcDisc_CuryDocBal(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            if (((ARInvoice)e.Row).DocType == ARDocType.CashSale || ((ARInvoice)e.Row).DocType == ARDocType.CashReturn)
			{
			_CuryDocBal = typeof(ARInvoice.curyDocBal);
			}

			try
			{
				base.CalcDisc(sender, e);
			}
			finally
			{
				_CuryDocBal = null;
			}
		}

		public void VerifyDiscount<Field>(PXCache sender, PXFieldVerifyingEventArgs e)
			where Field : IBqlField
		{
			if (((ARInvoice)e.Row).DocType == ARDocType.CashSale && typeof(Field) == typeof(ARInvoice.curyDocBal) ||
				((ARInvoice)e.Row).DocType != ARDocType.CashSale && typeof(Field) == typeof(ARInvoice.curyOrigDocAmt))
			{
				_CuryDocBal = typeof(Field);
			}

			try
			{
				base.VerifyDiscount(sender, e);
			}
			finally
			{
				_CuryDocBal = null;
			}
		}
	}


	public class SOShipExpireDateAttribute : INExpireDateAttribute
	{
		public SOShipExpireDateAttribute(Type InventoryType)
			:base(InventoryType)
		{ 
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null) return;

			if (((INLotSerClass) item).LotSerTrack != INLotSerTrack.NotNumbered)
			{
				bool? isConfirmed = (bool?) sender.GetValue<SOShipLineSplit.confirmed>(e.Row);
				if (((INLotSerClass)item).LotSerAssign != INLotSerAssign.WhenUsed || isConfirmed == true)
			{
				base.RowPersisting(sender, e);
				}
			}
		}
	}

	public class DirtyFormulaAttribute : PXAggregateAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber
	{
		protected Dictionary<object, object> inserted = null;
		protected Dictionary<object, object> updated = null;

		public DirtyFormulaAttribute(Type formulaType, Type aggregateType)
		{
			this._Attributes.Add(new PXFormulaAttribute(formulaType, aggregateType));
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			inserted = new Dictionary<object, object>();
			updated = new Dictionary<object, object>();
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			object copy;
			if (!inserted.TryGetValue(e.Row, out copy))
			{
				inserted[e.Row] = sender.CreateCopy(e.Row);
			} 
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			object copy;
			if (!updated.TryGetValue(e.Row, out copy))
			{
				updated[e.Row] = sender.CreateCopy(e.Row);
			}
		}

		public static void RaiseRowInserted<Field>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(e.Row))
			{
				if (attr is DirtyFormulaAttribute)
				{
					object copy;
					if (((DirtyFormulaAttribute)attr).inserted.TryGetValue(e.Row, out copy))
					{
						List<IPXRowUpdatedSubscriber> subs = new List<IPXRowUpdatedSubscriber>();
						((PXAggregateAttribute)attr).GetSubscriber<IPXRowUpdatedSubscriber>(subs);
						foreach (IPXRowUpdatedSubscriber ru in subs)
						{
							ru.RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						}
						((DirtyFormulaAttribute)attr).inserted.Remove(e.Row);
					}
				}
			}
		}

		public static void RaiseRowUpdated<Field>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(e.Row))
			{
				if (attr is DirtyFormulaAttribute)
				{
					object copy;
					if (((DirtyFormulaAttribute)attr).updated.TryGetValue(e.Row, out copy))
					{
						List<IPXRowUpdatedSubscriber> subs = new List<IPXRowUpdatedSubscriber>();
						((PXAggregateAttribute)attr).GetSubscriber<IPXRowUpdatedSubscriber>(subs);
						foreach (IPXRowUpdatedSubscriber ru in subs)
						{
							ru.RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						}
						((DirtyFormulaAttribute)attr).updated.Remove(e.Row);
					}
				}
			}
		}
	}

	public sealed class OpenLineCalc<Field> : IBqlAggregateCalculator
	where Field : IBqlField
	{
		#region IBqlAggregateCalculator Members

		public object Calculate(PXCache cache, object row, object oldrow, int fieldordinal, int digit)
		{
			if (row != null && !typeof(SOLine).IsAssignableFrom(row.GetType()) ||
				oldrow != null && !typeof(SOLine).IsAssignableFrom(oldrow.GetType()))
			{
				return null;
			}

			if (object.ReferenceEquals(row, oldrow))
			{
				return null;
			}

			if (row != null && ((SOLine)row).OpenLine == true && (((SOLine)row).IsFree != true || ((SOLine)row).ManualDisc == true) && (oldrow == null || ((SOLine)oldrow).OpenLine != true || ((SOLine)oldrow).IsFree == true && ((SOLine)oldrow).ManualDisc == false))
			{
				return 1;
			}

			if (oldrow != null && ((SOLine)oldrow).OpenLine == true && (((SOLine)oldrow).IsFree != true || ((SOLine)oldrow).ManualDisc == true) && (row == null || ((SOLine)row).OpenLine != true || ((SOLine)row).IsFree == true && ((SOLine)row).ManualDisc == false))
			{
				return -1;
			}

			return 0;
		}

		public object Calculate(PXCache cache, object row, int fieldordinal, object[] records, int digit)
		{
			short count = 0;
			foreach (object record in records)
			{
				if (((SOLine)record).OpenLine == true && (((SOLine)record).IsFree != true || ((SOLine)record).ManualDisc == true))
				{
					count++;
				}
			}
			return count;
		}

		#endregion
	}

	public class DateMinusDaysNotLessThenDate<V1, V2, V3> : IBqlCreator
		where V1 : IBqlOperand
		where V2 : IBqlOperand
		where V3 : IBqlOperand
	{
		IBqlCreator _formula = new Switch<Case<Where<Sub<V1,V2>, Less<V3>, Or<Sub<V1,V2>, IsNull>>, V3>, Sub<V1,V2>>();

		public bool AppendExpression(ref PX.Data.SQLTree.SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> _formula.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			_formula.Verify(cache, item, pars, ref result, ref value);
		}	
	}

	public interface IFreightBase
	{
		string ShipTermsID { get; }
		string ShipVia { get; }
		string ShipZoneID { get; }
		decimal? LineTotal { get; }
		decimal? OrderWeight { get; }
		decimal? PackageWeight { get; }
		decimal? OrderVolume { get; }
		decimal? FreightAmt { get; set;}
		decimal? FreightCost { get; set; }
	}

	/// <summary>
	/// Calculates Freight Cost and Freight Terms
	/// </summary>
	public class FreightCalculator
	{
		protected PXGraph graph;

		public FreightCalculator(PXGraph graph)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");

			this.graph = graph;
		}

		/*
		Freight Calculation

		1. Per Unit

		FreightInBase = BaseRate + Rate * Weight

		Select FreightRate.Rate 
		FreightRate.Volume <= Order.Volume && 
		FreightRate.Weight <= Order.Weight && 
		FreightRate.Zone = Order.ShippingZone 
		ORDER BY FreightRate.Volume Asc, FreightRate.Weight Asc, FreightRate.Rate Asc

		IF not found, then

		Select ShipTerms.Rate 
		FreightRate.Volume <= Order.Volume && 
		FreightRate.Weight <= Order.Weight 
		ORDER BY FreightRate.Volume Asc, FreightRate.Weight Asc, FreightRate.Rate Desc
		(MAX Rate for search criteria)

		2. Net

		FreightInBase = BaseRate + Rate
		--------------------------------------------------------------------------------
											 FreightCost%			   InvoiceAmount%
		FreightFinalInBase = FreightInBase * ------------ + LineTotal * --------------
												 100						  100
		*/

		/// <summary>
		/// First calculates and sets CuryFreightCost, then applies the Terms and updates CuryFreightAmt.
		/// </summary>
		public virtual void CalcFreight<T, CuryFreightCostField, CuryFreightAmtField>(PXCache sender, T data, int? linesCount)
			where T : class, IFreightBase, new()
			where CuryFreightAmtField : IBqlField
			where CuryFreightCostField : IBqlField
		{
			CalcFreightCost<T, CuryFreightCostField>(sender, data);
			ApplyFreightTerms<T, CuryFreightAmtField>(sender, data, linesCount);
		}

		/// <summary>
		/// Calculates and sets CuryFreightCost
		/// </summary>
		public virtual void CalcFreightCost<T, CuryFreightCostField>(PXCache sender, T data)
			where T : class, IFreightBase, new()
			where CuryFreightCostField : IBqlField
		{
			data.FreightCost = CalculateFreightCost<T>(data);
			CM.PXCurrencyAttribute.CuryConvCury<CuryFreightCostField>(sender, data);
		}

		/// <summary>
		/// Applies the Terms and updates CuryFreightAmt.
		/// </summary>
		public virtual void ApplyFreightTerms<T, CuryFreightAmtField>(PXCache sender, T data, int? linesCount)
			where T : class, IFreightBase, new()
			where CuryFreightAmtField : IBqlField
		{
			ShipTermsDetail shipTermsDetail = GetFreightTerms(data.ShipTermsID, data.LineTotal);

			if (shipTermsDetail != null)
			{
				data.FreightAmt = ((data.FreightCost * (shipTermsDetail.FreightCostPercent) / 100) ?? 0) +
						(((data.LineTotal) * (shipTermsDetail.InvoiceAmountPercent) / 100) ?? 0) + (shipTermsDetail.ShippingHandling ?? 0) + ((linesCount * shipTermsDetail.LineHandling) ?? 0);
			}
			else
			{
				data.FreightAmt = data.FreightCost;
			}

			CM.PXCurrencyAttribute.CuryConvCury<CuryFreightAmtField>(sender, data);
		}

		protected virtual decimal CalculateFreightCost<T>(T data)
			where T : class, IFreightBase, new()
		{
			Carrier carrier = Carrier.PK.Find(graph, data.ShipVia);

			if (carrier == null)
				return 0;

            if (carrier.CalcMethod != CarrierCalcMethod.Manual)
            {
                decimal freightCostAmt = 0;
                if (data.OrderVolume == null || data.OrderVolume == 0)
                {
                    //Get Freight Rate based only on weight.
                    FreightRate freightRateOnWeight = GetFreightRateBasedOnWeight(carrier.CarrierID, data.ShipZoneID, (data.PackageWeight == null || data.PackageWeight == 0) ? data.OrderWeight : data.PackageWeight);

                    if (carrier.CalcMethod == CarrierCalcMethod.Net)
                        freightCostAmt = freightRateOnWeight.Rate ?? 0;
                    else
                        if (data.PackageWeight == null || data.PackageWeight == 0)
                            freightCostAmt = (freightRateOnWeight.Rate ?? 0m) * (data.OrderWeight ?? 0m);
                        else
                            freightCostAmt = (freightRateOnWeight.Rate ?? 0m) * (data.PackageWeight ?? 0m);
                }
                else if (data.PackageWeight == null || data.PackageWeight == 0)
                {
                    //Get Freight Rate based only on Volume
                    FreightRate freightRateOnVolume = GetFreightRateBasedOnVolume(carrier.CarrierID, data.ShipZoneID, data.OrderVolume);

                    if (carrier.CalcMethod == CarrierCalcMethod.Net)
                        freightCostAmt = freightRateOnVolume.Rate ?? 0;
                    else
                        freightCostAmt = (freightRateOnVolume.Rate ?? 0m) * (data.OrderVolume ?? 0m);
                }
                else
                {
                    FreightRate freightRateOnWeight = GetFreightRateBasedOnWeight(carrier.CarrierID, data.ShipZoneID, (data.PackageWeight == null || data.PackageWeight == 0) ? data.OrderWeight : data.PackageWeight);
                    FreightRate freightRateOnVolume = GetFreightRateBasedOnVolume(carrier.CarrierID, data.ShipZoneID, data.OrderVolume);

                    decimal freightCostByWeight = 0;
                    decimal freightCostByVolume = 0;


                    if (carrier.CalcMethod == CarrierCalcMethod.Net)
                    {
                        freightCostByWeight = freightRateOnWeight.Rate ?? 0;
                        freightCostByVolume = freightRateOnVolume.Rate ?? 0;
                    }
                    else
                    {
                        freightCostByWeight = (freightRateOnWeight.Rate ?? 0m) * (data.PackageWeight ?? 0m);
                        freightCostByVolume = (freightRateOnVolume.Rate ?? 0m) * (data.OrderVolume ?? 0m);
                    }

                    freightCostAmt = Math.Max(freightCostByWeight, freightCostByVolume);
                }

                return (carrier.BaseRate ?? 0m) + freightCostAmt;
            }
            else
            {
                return data.FreightCost ?? 0m;
            }
		}

		protected virtual ShipTermsDetail GetFreightTerms(string shipTermsID, decimal? lineTotal)
		{
			return PXSelect<ShipTermsDetail,
				Where<ShipTermsDetail.shipTermsID, Equal<Required<SOOrder.shipTermsID>>,
				And<ShipTermsDetail.breakAmount, LessEqual<Required<SOOrder.lineTotal>>>>,
				OrderBy<Desc<ShipTermsDetail.breakAmount>>>.Select(graph, shipTermsID, lineTotal);

		}

		protected virtual FreightRate GetFreightRateBasedOnWeight(string carrierID, string shipZoneID, decimal? weight)
		{
			FreightRate freightRate = PXSelect<FreightRate,
				Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
				And<FreightRate.weight, LessEqual<Required<SOOrder.orderWeight>>,
				And<FreightRate.zoneID, Equal<Required<SOOrder.shipZoneID>>>>>,
				OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Asc<FreightRate.rate>>>>>.
				Select(graph, carrierID, weight, shipZoneID);

			if (freightRate == null)
			{
				freightRate = PXSelect<FreightRate,
					Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
					And<FreightRate.weight, LessEqual<Required<FreightRate.weight>>>>,
					OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Desc<FreightRate.rate>>>>>.
					Select(graph, weight);
			}

			return freightRate ?? new FreightRate();
		}

		protected virtual FreightRate GetFreightRateBasedOnVolume(string carrierID, string shipZoneID, decimal? volume)
		{
			FreightRate freightRate = PXSelect<FreightRate,
				Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
				And<FreightRate.volume, LessEqual<Required<FreightRate.volume>>,
				And<FreightRate.zoneID, Equal<Required<FreightRate.zoneID>>>>>,
				OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Asc<FreightRate.rate>>>>>.
				Select(graph, carrierID, volume, shipZoneID);

			if (freightRate == null)
			{
				freightRate = PXSelect<FreightRate,
					Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
					And<FreightRate.weight, LessEqual<Required<FreightRate.weight>>>>,
					OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Desc<FreightRate.rate>>>>>.
					Select(graph, volume);
			}

			return freightRate ?? new FreightRate();
		}

	}

	public class SOLineSplitPlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentOrderDate;
		protected Lazy<Dictionary<object[], HashSet<Guid?>>> _processingSets;
		#endregion
		#region Ctor
		public SOLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry)
		{
			_ParentOrderDate = ParentOrderDate;
		}
		#endregion
		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_processingSets = Lazy.By(() => new Dictionary<object[], HashSet<Guid?>>());
            sender.Graph.FieldDefaulting.AddHandler<SiteStatus.negAvailQty>(SiteStatus_NegAvailQty_FieldDefaulting);
		}

        protected virtual void SiteStatus_NegAvailQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);
            if (e.Cancel == false && ordertype != null && ordertype.RequireAllocation == true)
            {
                e.NewValue = false;
                e.Cancel = true;
            }
        }

		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			PXView view;
			WebDialogResult answer = sender.Graph.Views.TryGetValue("Document", out view) ? view.Answer : WebDialogResult.None;

			bool DatesUpdated = !sender.ObjectsEqual<SOOrder.shipDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed);
			bool RequestOnUpdated = !sender.ObjectsEqual<SOOrder.requestDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed);
			bool CreditHoldApprovedUpdated = !sender.ObjectsEqual<SOOrder.creditHold>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOOrder.approved>(e.Row, e.OldRow);
			bool CustomerUpdated = !sender.ObjectsEqual<SOOrder.customerID>(e.Row, e.OldRow);

			if (CustomerUpdated || DatesUpdated || RequestOnUpdated || CreditHoldApprovedUpdated
				|| !sender.ObjectsEqual<SOOrder.hold, SOOrder.cancelled, SOOrder.completed, SOOrder.backOrdered, SOOrder.shipComplete>(e.Row, e.OldRow))
			{
				DatesUpdated |= !sender.ObjectsEqual<SOOrder.shipComplete>(e.Row, e.OldRow) && ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;
				RequestOnUpdated |= !sender.ObjectsEqual<SOOrder.shipComplete>(e.Row, e.OldRow) && ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;

				bool Cancelled = (bool?)sender.GetValue<SOOrder.cancelled>(e.Row) == true;
				bool Completed = (bool?)sender.GetValue<SOOrder.completed>(e.Row) == true;
                bool? BackOrdered = (bool?)sender.GetValue<SOOrder.backOrdered>(e.Row);

				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				PXCache splitcache = sender.Graph.Caches[typeof(SOLineSplit)];
				PXCache linecache = sender.Graph.Caches[typeof(SOLine)];

				SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);
				var unlinkedLines = new Dictionary<int?, bool>();
				var splitsByPlan = new Dictionary<long?, SOLineSplit>();
				foreach (IEnumerable<SOLineSplit> lineSplits in PXSelect<SOLineSplit,
					Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>, 
					And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
					.SelectMultiBound(sender.Graph, new[] { e.Row }).RowCast<SOLineSplit>().GroupBy(x => x.LineNbr).ToDictionary(x => x.Key, x => x.ToList()).Values)
                {
					bool clearPOLinks = false;
					if (Cancelled && lineSplits.Any(x => x.Completed != true && !string.IsNullOrEmpty(x.PONbr)))
					{
						clearPOLinks = true;
						unlinkedLines.Add(lineSplits.First().LineNbr, !lineSplits.Any(x => x.Completed == true && !string.IsNullOrEmpty(x.PONbr)));
					}

					foreach (SOLineSplit split in lineSplits)
					{
						if (Cancelled || Completed)
						{
							if (split.Completed != true)
							{
								plancache.Inserted.RowCast<INItemPlan>()
									.Where(_ => _.PlanID == split.PlanID)
									.ForEach(_ => plancache.Delete(_));
								if(clearPOLinks)
								{
									split.POCreate = false;
									if (!string.IsNullOrEmpty(split.PONbr))
									{
										split.ClearPOReferences();
										split.RefNoteID = null;
									}
								}
								split.PlanID = null;
								split.Completed = true;

								splitcache.MarkUpdated(split);
							}
						}
						else
						{
							if ((bool?)sender.GetValue<SOOrder.cancelled>(e.OldRow) == true)
							{
								if (string.IsNullOrEmpty(split.ShipmentNbr) && split.POCompleted == false)
								{
									split.Completed = false;
								}

								INItemPlan plan = DefaultValues(splitcache, split);
								if (plan != null)
								{
									plan = (INItemPlan)sender.Graph.Caches[typeof(INItemPlan)].Insert(plan);
									split.PlanID = plan.PlanID;
								}

								splitcache.MarkUpdated(split);
							}

							if (DatesUpdated)
							{
								split.ShipDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
								splitcache.MarkUpdated(split);
							}

							if (split.PlanID != null)
							{
								splitsByPlan[split.PlanID] = split;
							}
						}
					}
				}

				foreach (SOLine line in PXSelect<SOLine,
					Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>, And<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>>>
					.SelectMultiBound(sender.Graph, new[] { e.Row }))
				{
					if (Cancelled || Completed)
					{
						if (line.Completed != true)
						{
							SOLine old_row = PXCache<SOLine>.CreateCopy(line);
							line.UnbilledQty -= line.OpenQty;
							line.OpenQty = 0m;
							linecache.RaiseFieldUpdated<SOLine.unbilledQty>(line, 0m);
							linecache.RaiseFieldUpdated<SOLine.openQty>(line, 0m);

							bool clearPOCreated;
							if(unlinkedLines.TryGetValue(line.LineNbr, out clearPOCreated))
							{
								if (clearPOCreated)
									line.POCreated = false;
								if (line.POCreate == true)
								{
									line.POCreate = false;
									linecache.RaiseFieldUpdated<SOLine.pOCreate>(line, true);
								}
							}
							line.Completed = true;
							LSSOLine.ResetAvailabilityCounters(line);

							//SOOrderEntry_SOOrder_RowUpdated should execute later to correctly update balances
							TaxAttribute.Calculate<SOLine.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

							linecache.MarkUpdated(line);
						}
					}
					else
					{
						if ((bool?)sender.GetValue<SOOrder.cancelled>(e.OldRow) == true)
						{
							SOLine old_row = PXCache<SOLine>.CreateCopy(line);
							line.OpenQty = line.OrderQty - line.ShippedQty;
							line.UnbilledQty += line.OpenQty;
							object value = line.UnbilledQty;
							linecache.RaiseFieldVerifying<SOLine.unbilledQty>(line, ref value);
							linecache.RaiseFieldUpdated<SOLine.unbilledQty>(line, value);

							value = line.OpenQty;
							linecache.RaiseFieldVerifying<SOLine.openQty>(line, ref value);
							linecache.RaiseFieldUpdated<SOLine.openQty>(line, value);

							linecache.SetValueExt<SOLine.completed>(line, false);

							TaxAttribute.Calculate<SOLine.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

							linecache.MarkUpdated(line);
						}
						if (DatesUpdated)
						{
							line.ShipDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
							linecache.MarkUpdated(line);
					}
						if (RequestOnUpdated)
						{
							line.RequestDate = (DateTime?)sender.GetValue<SOOrder.requestDate>(e.Row);
							linecache.MarkUpdated(line);
				}
						if (CreditHoldApprovedUpdated || !sender.ObjectsEqual<SOOrder.hold>(e.Row, e.OldRow))
						{
                            LSSOLine.ResetAvailabilityCounters(line);
						}
					}
				}

				if (Cancelled || Completed)
				{
					PXFormulaAttribute.CalcAggregate<SOLine.unbilledQty>(linecache, e.Row);
					PXFormulaAttribute.CalcAggregate<SOLine.openQty>(linecache, e.Row);
				}

				PXSelectBase<INItemPlan> cmd = new PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<SOOrder.noteID>>>>(sender.Graph);

				//BackOrdered is tri-state
				if (BackOrdered == true && sender.GetValue<SOOrder.lastSiteID>(e.Row) != null && sender.GetValue<SOOrder.lastShipDate>(e.Row) != null)
				{
					cmd.WhereAnd<Where<INItemPlan.siteID, Equal<Current<SOOrder.lastSiteID>>, And<INItemPlan.planDate, LessEqual<Current<SOOrder.lastShipDate>>>>>();
				}

				if (BackOrdered == false)
				{
					sender.SetValue<SOOrder.lastSiteID>(e.Row, null);
					sender.SetValue<SOOrder.lastShipDate>(e.Row, null);
				}

				foreach (INItemPlan plan in cmd.View.SelectMultiBound(new [] { e.Row }))
				{
					if (Cancelled || Completed)
					{
						plancache.Delete(plan);
					}
					else
					{
						INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

						if (DatesUpdated)
						{
							plan.PlanDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
						}
						if (CustomerUpdated)
						{
							plan.BAccountID = (int?)sender.GetValue<SOOrder.customerID>(e.Row);
						}
						plan.Hold = IsOrderOnHold((SOOrder)e.Row);

						// We should skip allocated plans. In general we should process only "normal" plans.
						bool initPlanType = (ordertype.RequireAllocation != true)
							&& plan.PlanType.IsIn(INPlanConstants.Plan60, INPlanConstants.Plan62, INPlanConstants.Plan68, INPlanConstants.Plan69);
						if (initPlanType)
						{
						SOLineSplit split;
						if (splitsByPlan.TryGetValue(plan.PlanID, out split))
						{
							plan.PlanType = CalcPlanType(sender, plan, (SOOrder)e.Row, split, BackOrdered);

						if (!string.Equals(copy.PlanType, plan.PlanType))
						{
							plancache.RaiseRowUpdated(plan, copy);
						}
						}
						}

						plancache.MarkUpdated(plan);
					}
				}
				// SOOrder.BackOrdered value should be handled only single time and only in this method
				sender.SetValue<SOOrder.backOrdered>(e.Row, null);
			}
		}

		public override void Plan_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			// crutch for mass processing CreateShipment & PXProcessing<Table>.TryPersistPerRow
			if (e.Operation == PXDBOperation.Update)
			{
				INItemPlan row = e.Row as INItemPlan;
				PXCache cache = sender.Graph.Caches[typeof(SOOrder)];
				SOOrder order = cache.Current as SOOrder;

				if (row != null && order != null
					&& row.RefNoteID != order.NoteID
					&& PXLongOperation.GetCustomInfo(sender.Graph.UID, PXProcessing.ProcessingKey, out object[] processingList) != null
					&& processingList != null)
				{
					if (!_processingSets.Value.TryGetValue(processingList, out HashSet<Guid?> processingSet))
					{
						processingSet = processingList.Select(x => ((SOOrder)x).NoteID).ToHashSet();
						_processingSets.Value[processingList] = processingSet;
					}

					if (processingSet.Contains(row.RefNoteID))
					e.Cancel = true;
				}
			}
			base.Plan_RowPersisting(sender, e);
		}

		bool InitPlan = false;
		bool InitVendor = false;
		bool ResetSupplyPlanID = false;
		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//respond only to GUI operations
		    var IsLinked = IsLineLinked((SOLineSplit)e.Row);

		    InitPlan = InitPlanRequired(sender, e) && !IsLinked;
			InitVendor = !sender.ObjectsEqual<SOLineSplit.siteID, SOLineSplit.subItemID, SOLineSplit.vendorID, SOLineSplit.pOCreate>(e.Row, e.OldRow) &&
				!IsLinked;
			ResetSupplyPlanID = !IsLinked;

			try
			{
				base.RowUpdated(sender, e);
			}
			finally
			{
				InitPlan = false;
				ResetSupplyPlanID = false;
			}
		}

	    protected virtual bool InitPlanRequired(PXCache cache, PXRowUpdatedEventArgs e)
	    {
	        return !cache
	            .ObjectsEqual<SOLineSplit.isAllocated, 
	                SOLineSplit.siteID, 
	                SOLineSplit.pOCreate, 
	                SOLineSplit.pOSource,
	                SOLineSplit.operation>(e.Row, e.OldRow);
	    }

	    protected virtual bool IsLineLinked(SOLineSplit soLineSplit)
	    {
	        return soLineSplit != null && (soLineSplit.PONbr != null || soLineSplit.SOOrderNbr != null && soLineSplit.IsAllocated == true);
	    }

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			if (((SOLineSplit)orig_Row).Completed == true || ((SOLineSplit)orig_Row).POCompleted == true || ((SOLineSplit)orig_Row).LineType == SOLineType.MiscCharge || ((SOLineSplit)orig_Row).LineType == SOLineType.NonInventory && ((SOLineSplit)orig_Row).RequireShipping == false)
			{
				return null;
			}
			SOLine parent = (SOLine)PXParentAttribute.SelectParent(sender, orig_Row, typeof(SOLine));
			SOOrder order = (SOOrder)PXParentAttribute.SelectParent(sender, orig_Row, typeof(SOOrder));

			SOLineSplit split_Row = (SOLineSplit)orig_Row;			

			if (string.IsNullOrEmpty(plan_Row.PlanType) || InitPlan)
			{
				plan_Row.PlanType = CalcPlanType(sender, plan_Row, order, split_Row);
				if (split_Row.POCreate == true)
				{
					plan_Row.FixedSource = INReplenishmentSource.Purchased;
					if (split_Row.POType != PO.POOrderType.Blanket && split_Row.POType != PO.POOrderType.DropShip && split_Row.POSource == INReplenishmentSource.PurchaseToOrder)
						plan_Row.SourceSiteID = order.DestinationSiteID ?? split_Row.SiteID;
					else
						plan_Row.SourceSiteID = split_Row.SiteID;
				}
				else
				{
                    plan_Row.Reverse = (split_Row.Operation == SOOperation.Receipt);

					plan_Row.FixedSource = (split_Row.SiteID != split_Row.ToSiteID ? INReplenishmentSource.Transfer : INReplenishmentSource.None);
                    plan_Row.SourceSiteID = split_Row.SiteID;
				}
			}

			if (ResetSupplyPlanID)
			{
				plan_Row.SupplyPlanID = null;
			}

			plan_Row.VendorID = split_Row.VendorID;

			if (InitVendor || split_Row.POCreate == true && plan_Row.VendorID != null && plan_Row.VendorLocationID == null)
			{
				plan_Row.VendorLocationID =
					PX.Objects.PO.POItemCostManager.FetchLocation(
					sender.Graph,
					split_Row.VendorID,
					split_Row.InventoryID,
					split_Row.SubItemID,
					split_Row.SiteID);
			}

			plan_Row.BAccountID = parent == null ? null : parent.CustomerID;
			plan_Row.InventoryID = split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.LocationID = split_Row.LocationID;
			plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
			if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanDate = split_Row.ShipDate;
			plan_Row.PlanQty = (split_Row.POCreate == true ? split_Row.BaseUnreceivedQty - split_Row.BaseShippedQty : split_Row.BaseQty);

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			plan_Row.RefNoteID = (Guid?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = IsOrderOnHold(order);

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}

		protected virtual bool IsOrderOnHold(SOOrder order)
	    {
			return (order != null) && ((order.Hold ?? false) || (order.CreditHold ?? false) || (!order.Approved ?? false));
		}

		protected virtual string CalcPlanType(PXCache sender, INItemPlan plan, SOOrder order, SOLineSplit split, bool? backOrdered = null)
		{
			if (split.POCreate == true)
			{
				if (split.POType == PO.POOrderType.Blanket)
	        {
					return (split.POSource == INReplenishmentSource.DropShipToOrder)
						? INPlanConstants.Plan6E
						: INPlanConstants.Plan6B;
				}
				else
				{
					return (split.POSource == INReplenishmentSource.DropShipToOrder)
						? INPlanConstants.Plan6D
						: INPlanConstants.Plan66;
				}
			}

			SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);
			bool isAllocation = (split.IsAllocated == true) || INPlanConstants.IsAllocated(plan.PlanType) || INPlanConstants.IsFixed(plan.PlanType);
			bool isOrderOnHold = IsOrderOnHold(order) && ordertype.RequireAllocation != true;

			string calcedPlanType = CalcPlanType(plan, split, ordertype, isOrderOnHold);
			bool putOnSOPrepared = (calcedPlanType == INPlanConstants.Plan69);

			if (!InitPlan && !putOnSOPrepared && !isAllocation)
			{
				if (backOrdered == true || backOrdered == null && plan.PlanType == INPlanConstants.Plan68)
				{
					return INPlanConstants.Plan68;
				}
	        }

			return calcedPlanType;
	    }

		protected virtual string CalcPlanType(INItemPlan plan, SOLineSplit split, SOOrderType ordertype, bool isOrderOnHold)
		{
			if (ordertype == null || ordertype.RequireShipping == true)
			{
				return (split.IsAllocated == true) ? split.AllocatedPlanType
					: isOrderOnHold ? INPlanConstants.Plan69
					: (split.RequireAllocation != true || split.IsStockItem != true) ? split.PlanType : split.BackOrderPlanType;
			}
			else
			{
				return (isOrderOnHold != true || split.IsStockItem != true) ? split.PlanType : INPlanConstants.Plan69;
			}
		}
		#endregion
    }

	public abstract class SOShipLineSplitPlanIDBaseAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentOrderDate;
		#endregion
		#region Ctor
		public SOShipLineSplitPlanIDBaseAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry)
		{
			_ParentOrderDate = ParentOrderDate;
		}
		#endregion
		#region Implementation

		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			if (!sender.ObjectsEqual<SOShipment.shipDate, SOShipment.hold>(e.Row, e.OldRow))
			{
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				HashSet<long?> shipmentPlans = CollectShipmentPlans(sender);
				foreach (INItemPlan plan in plancache.Inserted)
				{
					if (shipmentPlans.Contains(plan.PlanID))
					{
						plan.Hold = (bool?)sender.GetValue<SOShipment.hold>(e.Row);
						plan.PlanDate = (DateTime?)sender.GetValue<SOShipment.shipDate>(e.Row);
					}
				}

				UpdatePlansOnParentUpdated(sender, e.Row);
			}
		}

		protected virtual void UpdatePlansOnParentUpdated(PXCache sender, object parent)
		{
		}

		protected abstract HashSet<long?> CollectShipmentPlans(PXCache sender, string shipmentNbr = null);

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			if ((bool?)sender.GetValue<SOShipLineSplit.released>(orig_Row) == true
				|| (bool?)sender.GetValue<SOShipLineSplit.isStockItem>(orig_Row) == false)
			{
				return null;
			}
			else if (plan_Row.IsTemporary != true && (bool?)sender.GetValue<SOShipLineSplit.confirmed>(orig_Row) == true)
			{
				return plan_Row;
			}
			SOShipLine parent = plan_Row.IsTemporary == true ? null : (SOShipLine)PXParentAttribute.SelectParent(sender, orig_Row, typeof(SOShipLine));

			if (plan_Row.IsTemporary != true && parent == null) return null;

			plan_Row.BAccountID = parent?.CustomerID;
			plan_Row.PlanType = (string)sender.GetValue<SOShipLineSplit.planType>(orig_Row);
			plan_Row.OrigPlanType = (string)sender.GetValue<SOShipLineSplit.origPlanType>(orig_Row);
			plan_Row.IgnoreOrigPlan = (bool?)sender.GetValue<SOShipLineSplit.isComponentItem>(orig_Row) == true;
			plan_Row.InventoryID = (int?)sender.GetValue<SOShipLineSplit.inventoryID>(orig_Row);
			plan_Row.Reverse = (string)sender.GetValue<SOShipLineSplit.operation>(orig_Row) == SOOperation.Receipt;
			plan_Row.SubItemID = (int?)sender.GetValue<SOShipLineSplit.subItemID>(orig_Row);
			plan_Row.SiteID = (int?)sender.GetValue<SOShipLineSplit.siteID>(orig_Row);
			plan_Row.LocationID = (int?)sender.GetValue<SOShipLineSplit.locationID>(orig_Row);
			plan_Row.LotSerialNbr = (string)sender.GetValue<SOShipLineSplit.lotSerialNbr>(orig_Row);

			string assignedNbr = (string)sender.GetValue<SOShipLineSplit.assignedNbr>(orig_Row);
			if (string.IsNullOrEmpty(assignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(assignedNbr, plan_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanDate = (DateTime?)sender.GetValue<SOShipLineSplit.shipDate>(orig_Row);
			plan_Row.PlanQty = (decimal?)sender.GetValue<SOShipLineSplit.baseQty>(orig_Row);

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			plan_Row.RefNoteID = (Guid?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(cache.Current, _ParentHoldEntry.Name);

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}

            if (parent != null)
            {
				if (plan_Row.OrigNoteID == null)
				{
					SOOrder doc = SOOrder.PK.Find(sender.Graph, parent.OrigOrderType, parent.OrigOrderNbr);
				plan_Row.OrigNoteID = doc?.NoteID;
				}

	            SOLineSplit split = SOLineSplit.PK.Find(sender.Graph, parent.OrigOrderType, parent.OrigOrderNbr, parent.OrigLineNbr ?? 0, parent.OrigSplitLineNbr ?? 0);
                if (split != null)
                {
                    plan_Row.OrigPlanLevel = (!string.IsNullOrEmpty(split.LotSerialNbr) ? INPlanLevel.LotSerial : INPlanLevel.Site);
                    plan_Row.OrigPlanID = split.PlanID;
					plan_Row.IgnoreOrigPlan |= !string.IsNullOrEmpty(split.LotSerialNbr) &&
						!string.Equals(plan_Row.LotSerialNbr, split.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase);
                }
            }

			return plan_Row;
		}	

		#endregion
	}

	public class SOShipLineSplitPlanIDAttribute : SOShipLineSplitPlanIDBaseAttribute
	{
		#region State
		protected Lazy<Dictionary<object[], HashSet<Guid?>>> _processingSets;
		#endregion
		#region Ctor
		public SOShipLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry, ParentOrderDate)
		{
		}
		#endregion
		#region Implementation

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_processingSets = Lazy.By(() => new Dictionary<object[], HashSet<Guid?>>());
		}

		protected override void UpdatePlansOnParentUpdated(PXCache sender, object parent)
		{
			PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
			foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<SOShipment.noteID>>>>.Select(sender.Graph))
			{
				plan.Hold = (bool?)sender.GetValue<SOShipment.hold>(parent);
				plan.PlanDate = (DateTime?)sender.GetValue<SOShipment.shipDate>(parent);

				plancache.MarkUpdated(plan);
			}
		}

		protected override HashSet<long?> CollectShipmentPlans(PXCache sender, string shipmentNbr = null)
		{
			object[] pars = (shipmentNbr == null) ? new object[] { } : new object[] { shipmentNbr };
			return new HashSet<long?>(
				PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Optional<SOShipment.shipmentNbr>>>>
				.Select(sender.Graph, pars)
				.Select(s => ((SOShipLineSplit)s).PlanID));
		}

		public override void Plan_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			// crutch for mass processing CreateShipment & PXProcessing<Table>.TryPersistPerRow
			if (e.Operation == PXDBOperation.Update)
			{
				INItemPlan row = e.Row as INItemPlan;
				PXCache cache = sender.Graph.Caches[typeof(SOShipment)];
				SOShipment shipment = cache.Current as SOShipment;

				if (row != null && shipment != null
					&& row.RefNoteID != shipment.NoteID
					&& PXLongOperation.GetCustomInfo(sender.Graph.UID, PXProcessing.ProcessingKey, out object[] processingList) != null
					&& processingList != null)
				{
					if (!_processingSets.Value.TryGetValue(processingList, out HashSet<Guid?> processingSet))
					{
						processingSet = processingList.Select(x => ((SOShipment)x).NoteID).ToHashSet();
						_processingSets.Value[processingList] = processingSet;
					}

					if (processingSet.Contains(row.RefNoteID))
					e.Cancel = true;
				}
			}
			base.Plan_RowPersisting(sender, e);
		}
		#endregion
	}

	public class SOUnassignedShipLineSplitPlanIDAttribute : SOShipLineSplitPlanIDBaseAttribute
	{
		#region Ctor
		public SOUnassignedShipLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry, ParentOrderDate)
		{
		}
		#endregion
		#region Implementation
		protected override HashSet<long?> CollectShipmentPlans(PXCache sender, string shipmentNbr = null)
		{
			object[] pars = (shipmentNbr == null) ? new object[] { } : new object[] { shipmentNbr };
			return new HashSet<long?>(
				PXSelect<Unassigned.SOShipLineSplit, Where<Unassigned.SOShipLineSplit.shipmentNbr, Equal<Optional<SOShipment.shipmentNbr>>>>
				.Select(sender.Graph, pars)
				.Select(s => ((Unassigned.SOShipLineSplit)s).PlanID));
		}
		#endregion
	}

	public abstract class LSSelectSOBase<TLSMaster, TLSDetail, Where> : LSSelect<TLSMaster, TLSDetail, Where>
		where TLSMaster : class, IBqlTable, ILSPrimary, new()
		where TLSDetail : class, IBqlTable, ILSDetail, new()
		where Where : IBqlWhere, new()
	{
		#region Ctor

		public LSSelectSOBase(PXGraph graph)
			: base(graph)
		{
		}

		#endregion

		#region Implementation

		protected virtual bool MemoAvailabilityCheckQty(
			PXCache sender, int? inventoryID,
			string arDocType, string arRefNbr, int? arTranLineNbr,
			string orderType, string orderNbr, int? orderLineNbr)
		{
			bool hasRefToOrigSOLine = (orderType != null && orderNbr != null && orderLineNbr != null);
			bool hasRefToOrigARTran = (arDocType != null && arRefNbr != null && arTranLineNbr != null);
			if (!hasRefToOrigSOLine && !hasRefToOrigARTran)
				return true;

			SOInvoicedRecords invoiced = SelectInvoicedRecords(arDocType, arRefNbr);

			//return SO lines (including current document, excluding cancelled orders):
			//Note: SOOrder is LeftJoined instead of InnerJoin for current unsaved document lines to be included in the result.
			PXSelectBase<SOLine> selectReturnSOLines = new PXSelectJoin<SOLine,
				LeftJoin<SOOrder, On<SOOrder.orderType, Equal<SOLine.orderType>, And<SOOrder.orderNbr, Equal<SOLine.orderNbr>>>>,
				Where<SOLine.invoiceType, Equal<Required<SOLine.invoiceType>>,
				And<SOLine.invoiceNbr, Equal<Required<SOLine.invoiceNbr>>,
				And<Where<SOLine.behavior, Equal<SOOrderTypeConstants.rmaOrder>,
				Or<SOLine.behavior, Equal<SOOrderTypeConstants.creditMemo>, And<SOOrder.cancelled, Equal<False>>>>>>>>(_Graph);
			var returnSOLines = selectReturnSOLines.Select(arDocType, arRefNbr).RowCast<SOLine>();

			//return direct AR Transactions (including current document):
			PXSelectBase<ARTran> selectReturnARTrans = new PXSelect<ARTran,
				Where<ARTran.sOOrderNbr, IsNull,
				And<ARTran.origInvoiceType, Equal<Required<ARTran.origInvoiceType>>,
				And<ARTran.origInvoiceNbr, Equal<Required<ARTran.origInvoiceNbr>>,
				And<Where<ARTran.invtMult, Equal<short1>, And<ARTran.qty, Greater<decimal0>,
				Or<ARTran.invtMult, Equal<shortMinus1>, And<ARTran.qty, Less<decimal0>>>>>>>>>>(_Graph);
			var returnARTrans = selectReturnARTrans.Select(arDocType, arRefNbr).RowCast<ARTran>();

			bool success = true;
			if (hasRefToOrigSOLine)
			{
				var invoicedFromSOLine = invoiced.Records.Where(r => r.SOLine.OrderType == orderType && r.SOLine.OrderNbr == orderNbr && r.SOLine.LineNbr == orderLineNbr);
				var returnedFromSOLine = returnSOLines.Where(l => l.OrigOrderType == orderType && l.OrigOrderNbr == orderNbr && l.OrigLineNbr == orderLineNbr)
					.Select(ReturnRecord.FromSOLine);
				success = CheckInvoicedAndReturnedQty(sender, inventoryID, invoicedFromSOLine, returnedFromSOLine);
			}
			if (success == true && hasRefToOrigARTran)
			{
				var invoicedFromOrigARTran = invoiced.Records.Where(r => r.ARTran.LineNbr == arTranLineNbr);
				var returnedFromOrigARTran = returnARTrans.Where(t => t.OrigInvoiceLineNbr == arTranLineNbr).Select(ReturnRecord.FromARTran)
					.Concat(returnSOLines.Where(l => l.InvoiceLineNbr == arTranLineNbr).Select(ReturnRecord.FromSOLine));
				success = CheckInvoicedAndReturnedQty(sender, inventoryID, invoicedFromOrigARTran, returnedFromOrigARTran);
			}
			return success;
		}

		public virtual SOInvoicedRecords SelectInvoicedRecords(string arDocType, string arRefNbr)
		{
			return SelectInvoicedRecords(arDocType, arRefNbr, includeDirectLines: false);
		}

		protected virtual SOInvoicedRecords SelectInvoicedRecords(string arDocType, string arRefNbr, bool includeDirectLines)
		{
			SOInvoicedRecords splits = new SOInvoicedRecords(new Comparer<ARTran>(_Graph));

			PXSelectBase<ARTran> cmd = new PXSelectJoin<ARTran
				, InnerJoin<InventoryItem
					, On<InventoryItem.inventoryID, Equal<ARTran.inventoryID>
						>
				, LeftJoin<SOLine
					, On<SOLine.orderType, Equal<ARTran.sOOrderType>
						, And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>
							, And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>
							>
						>
				, LeftJoin<INTran
					, On<INTran.aRDocType, Equal<ARTran.tranType>, And<INTran.aRRefNbr, Equal<ARTran.refNbr>, And<INTran.aRLineNbr, Equal<ARTran.lineNbr>>>>
				, LeftJoin<INTranSplit
					, On<INTranSplit.FK.Tran>
				, LeftJoin<INLotSerialStatus
					, On<INLotSerialStatus.lotSerTrack, Equal<INLotSerTrack.serialNumbered>
						, And<INLotSerialStatus.inventoryID, Equal<INTranSplit.inventoryID>
							, And<INLotSerialStatus.lotSerialNbr, Equal<INTranSplit.lotSerialNbr>
								, And<
									Where<
										INLotSerialStatus.qtyOnHand, Greater<decimal0>
										, Or<INLotSerialStatus.qtyINReceipts, Greater<decimal0>
											, Or<INLotSerialStatus.qtySOShipping, Less<decimal0>
												, Or<INLotSerialStatus.qtySOShipped, Less<decimal0>>
												>
											>
										>
									>
								>
							>
						>
				, LeftJoin<SOSalesPerTran
					, On<SOSalesPerTran.orderType, Equal<SOLine.orderType>, And<SOSalesPerTran.orderNbr, Equal<SOLine.orderNbr>, And<SOSalesPerTran.salespersonID, Equal<SOLine.salesPersonID>>>>>>>>>>,
				Where<ARTran.tranType, Equal<Required<AddInvoiceFilter.docType>>, And<ARTran.refNbr, Equal<Required<AddInvoiceFilter.refNbr>>,
				And<Where2<Where<INTran.released, Equal<boolTrue>, And<INTran.qty, Greater<decimal0>,
				And<Where<INTran.tranType, Equal<INTranType.issue>, Or<INTran.tranType, Equal<INTranType.debitMemo>, Or<INTran.tranType, Equal<INTranType.invoice>>>>>>>,
				Or<Where<INTran.released, IsNull, And<Where<ARTran.lineType, Equal<SOLineType.miscCharge>, Or<ARTran.lineType, Equal<SOLineType.nonInventory>>>>>>>>>>,
				OrderBy<Asc<ARTran.inventoryID, Asc<INTranSplit.subItemID>>>>(_Graph);

			if (!includeDirectLines)
			{
				cmd.WhereAnd<Where<ARTran.lineType, Equal<SOLine.lineType>, And<SOLine.orderNbr, IsNotNull>>>();
			}

			foreach (PXResult<ARTran, InventoryItem, SOLine, INTran, INTranSplit, INLotSerialStatus, SOSalesPerTran> res in
				cmd.Select(arDocType, arRefNbr))
			{
				splits.Add(res);
			}

			return splits;
		}

		protected virtual bool CheckInvoicedAndReturnedQty(PXCache sender, int? returnInventoryID, IEnumerable<SOInvoicedRecords.Record> invoiced, IEnumerable<ReturnRecord> returned)
		{
			if (returnInventoryID == null)
				return true;

			int origInventoryID = 0;
			decimal totalInvoicedQty = 0;
			Dictionary<int, decimal> totalInvoicedQtyByComponent = new Dictionary<int, decimal>();
			Dictionary<int, decimal> componentsInAKit = new Dictionary<int, decimal>();

			//invoiced are always either KIT or a regular item
			foreach (SOInvoicedRecords.Record record in invoiced)
			{
				origInventoryID = record.SOLine.InventoryID ?? record.ARTran.InventoryID.Value;
				totalInvoicedQty += INUnitAttribute.ConvertToBase(sender.Graph.Caches[typeof(ARTran)], record.ARTran.InventoryID, record.ARTran.UOM, (decimal)record.ARTran.Qty, INPrecision.QUANTITY);

				foreach (SOInvoicedRecords.INTransaction intran in record.Transactions.Values)
				{
					if (!totalInvoicedQtyByComponent.ContainsKey(intran.Transaction.InventoryID.Value))
						totalInvoicedQtyByComponent[intran.Transaction.InventoryID.Value] = 0;

					totalInvoicedQtyByComponent[intran.Transaction.InventoryID.Value] += INUnitAttribute.ConvertToBase(sender.Graph.Caches[typeof(INTran)], intran.Transaction.InventoryID, intran.Transaction.UOM, (decimal)intran.Transaction.Qty, INPrecision.QUANTITY);
				}
			}

			foreach (KeyValuePair<int, decimal> kv in totalInvoicedQtyByComponent)
			{
				componentsInAKit[kv.Key] = kv.Value / totalInvoicedQty;
			}

			//returned can be a regular item or a kit or a component of a kit. 
			foreach (var ret in returned)
			{
				if (ret.InventoryID == origInventoryID || totalInvoicedQtyByComponent.Count == 0)//regular item or a kit
				{
					decimal returnedQty = INUnitAttribute.ConvertToBase(sender, ret.InventoryID, ret.UOM, (decimal)ret.Qty, INPrecision.QUANTITY);
					totalInvoicedQty -= returnedQty;

					InventoryItem item = ReadInventoryItem(sender, ret.InventoryID);
					if (item.KitItem == true)
					{
						foreach (KeyValuePair<int, decimal> kv in componentsInAKit)
						{
							totalInvoicedQtyByComponent[kv.Key] -= componentsInAKit[kv.Key] * returnedQty;
						}
					}
				}
				else //component of a kit. 
				{
					totalInvoicedQtyByComponent[ret.InventoryID.Value] -= INUnitAttribute.ConvertToBase(sender, ret.InventoryID, ret.UOM, (decimal)ret.Qty, INPrecision.QUANTITY);
				}
			}

			bool success = true;
			if (returnInventoryID == origInventoryID)
			{
				if (totalInvoicedQty < 0m || totalInvoicedQtyByComponent.Values.Any(v => v < 0m))
				{
					success = false;
				}
			}
			else
			{
				if (totalInvoicedQty < 0m)
				{
					success = false;
				}

				decimal qtyByComponent;
				if (totalInvoicedQtyByComponent.TryGetValue(returnInventoryID.Value, out qtyByComponent) && qtyByComponent < 0)
				{
					success = false;
				}
			}
			return success;
		}

		public class Comparer<T> : IEqualityComparer<T>
		{
			protected PXCache _cache;
			public Comparer(PXGraph graph)
			{
				_cache = graph.Caches[typeof(T)];
			}

			public bool Equals(T a, T b)
			{
				return _cache.ObjectsEqual(a, b);
			}

			public int GetHashCode(T a)
			{
				return _cache.GetObjectHashCode(a);
			}
		}

		[PXInternalUseOnly]
		public abstract class ReturnRecord
		{
			public abstract int? InventoryID { get; }
			public abstract string UOM { get; }
			public abstract decimal Qty { get; }
			public abstract string DocumentNbr { get; }

			public static ReturnRecord FromSOLine(SOLine l) => new ReturnSOLine(l);

			public static ReturnRecord FromARTran(ARTran t) => new ReturnARTran(t);

			public class ReturnSOLine : ReturnRecord
			{
				public SOLine Line { get; }
				public override string DocumentNbr => Line.OrderNbr;
				public override int? InventoryID => Line.InventoryID;
				public override string UOM => Line.UOM;
				public override decimal Qty => (Line.RequireShipping == true && Line.Completed == true) ? (Line.ShippedQty ?? 0) : (Line.OrderQty ?? 0);

				public ReturnSOLine(SOLine line)
				{
					Line = line;
				}
			}

			public class ReturnARTran : ReturnRecord
			{
				public ARTran Tran { get; }
				public override string DocumentNbr => Tran.RefNbr;
				public override int? InventoryID => Tran.InventoryID;
				public override string UOM => Tran.UOM;
				public override decimal Qty => Math.Abs(Tran.Qty ?? 0m);

				public ReturnARTran(ARTran tran)
				{
					Tran = tran;
				}
			}
		}

		#endregion
	}

	public class LSSOLine : LSSelectSOBase<SOLine, SOLineSplit, 
		Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>,
        And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
	{
		#region State
        public bool IsLocationEnabled
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);
                if (ordertype == null || (ordertype.RequireShipping == false && ordertype.RequireLocation == true && ordertype.INDocType != INTranType.NoUpdate))
                    return true;
                else
                    return false;
            }
        }

		public bool IsLSEntryEnabled
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);
                return (ordertype == null || ordertype.RequireLocation == true || ordertype.RequireLotSerial == true);
			}
		}

        public bool IsLotSerialRequired
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);
                return (ordertype == null || ordertype.RequireLotSerial == true);
			}
		}

		public bool IsAllocationEntryEnabled
		{
			get
			{
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);
				return (ordertype == null || ordertype.RequireShipping == true);
			}
		}

        public bool IsAllocationRequired
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);
                return (ordertype == null || ordertype.RequireAllocation == true);
            }
        }
		#endregion
		#region Ctor
		public LSSOLine(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof(SOLine.orderQty);
			graph.FieldDefaulting.AddHandler<SOLineSplit.subItemID>(SOLineSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOLineSplit.locationID>(SOLineSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOLineSplit.invtMult>(SOLineSplit_InvtMult_FieldDefaulting);
			graph.RowSelected.AddHandler<SOOrder>(Parent_RowSelected);
			graph.RowUpdated.AddHandler<SOOrder>(SOOrder_RowUpdated);
			graph.RowSelected.AddHandler<SOLineSplit>(SOLineSplit_RowSelected);
			graph.RowPersisting.AddHandler<SOLineSplit>(SOLineSplit_RowPersisting);
		}
		#endregion

		#region Implementation
		public override IEnumerable BinLotSerial(PXAdapter adapter)
		{
			if (IsLSEntryEnabled || IsAllocationEntryEnabled)
			{
				if (MasterCache.Current != null && ((IsLSEntryEnabled && ((SOLine)MasterCache.Current).LineType != SOLineType.Inventory) || ((SOLine)MasterCache.Current).LineType == SOLineType.MiscCharge))
				{
					throw new PXSetPropertyException(Messages.BinLotSerialInvalid);
				}

				View.AskExt(true);
			}
			return adapter.Get();
		}

		protected virtual void SOOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (IsLSEntryEnabled && !sender.ObjectsEqual<SOOrder.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<SOOrder.hold>(e.Row) == false)
			{
				PXCache cache = sender.Graph.Caches[typeof(SOLine)];

				foreach (SOLine item in PXParentAttribute.SelectSiblings(cache, null, typeof(SOOrder)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
					{
						cache.RaiseExceptionHandling<SOLine.orderQty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						cache.MarkUpdated(item);
					}
				}
			}
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				var row = (SOLine)e.Row;
				if ((row.LineType == SOLineType.Inventory || row.LineType == SOLineType.NonInventory && row.InvtMult == (short)-1) && row.TranType != INTranType.NoUpdate && row.BaseQty < 0m)
				{
					if (sender.RaiseExceptionHandling<SOLine.orderQty>(e.Row, ((SOLine)e.Row).Qty, new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString())))
					{
						throw new PXRowPersistingException(typeof(SOLine.orderQty).Name, ((SOLine)e.Row).Qty, CS.Messages.Entry_GE, ((int)0).ToString());
					}
					return;
				}

				if (IsLSEntryEnabled)
				{
					PXCache cache = sender.Graph.Caches[typeof(SOOrder)];
					object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(SOOrder)) ?? cache.Current;

					bool? OnHold = (bool?)cache.GetValue<SOOrder.hold>(doc);

					if (OnHold == false && Math.Abs((decimal)((SOLine)e.Row).BaseQty) >= 0.0000005m && (((SOLine)e.Row).UnassignedQty >= 0.0000005m || ((SOLine)e.Row).UnassignedQty <= -0.0000005m))
					{
						if (sender.RaiseExceptionHandling<SOLine.orderQty>(e.Row, ((SOLine)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
						{
							throw new PXRowPersistingException(typeof(SOLine.orderQty).Name, ((SOLine)e.Row).Qty, Messages.BinLotSerialNotAssigned);
						}
					}
				}
			}

			//for normal orders there are only when received numbers which do not require any additional processing
			if (!IsLSEntryEnabled)
			{
                if (((SOLine)e.Row).TranType == INTranType.Transfer && DetailCounters.ContainsKey((SOLine)e.Row))
                {
                    //keep Counters when adding splits to Transfer order
                    DetailCounters[(SOLine)e.Row].UnassignedNumber = 0;
                }
                else
                {
				DetailCounters[(SOLine)e.Row] = new Counters { UnassignedNumber = 0 };
			}
			}

			base.Master_RowPersisting(sender, e);
		}
		
		public bool AvailabilityFetching { get; private set; }
		public override IQtyAllocated AvailabilityFetch(PXCache sender, ILSMaster Row, AvailabilityFetchMode fetchMode)
		{
			try
			{
				AvailabilityFetching = true;
				return AvailabilityFetchImpl(sender, Row, fetchMode);
			}
			finally
			{
				AvailabilityFetching = false;
			}
		}

		public virtual IQtyAllocated AvailabilityFetchImpl(PXCache sender, ILSMaster Row, AvailabilityFetchMode fetchMode)
		{
			if (Row != null)
			{
				SOLineSplit copy = Row as SOLineSplit;
				if (copy == null)
				{
					copy = Convert(Row as SOLine);

					PXParentAttribute.SetParent(DetailCache, copy, typeof(SOLine), Row);

					if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
					{
						DefaultLotSerialNbr(sender.Graph.Caches[typeof(SOLineSplit)], copy);
					}

					if (fetchMode.HasFlag(AvailabilityFetchMode.TryOptimize) && _detailsRequested++ == 5)
						{ 
						foreach (PXResult<SOLine, INUnit, INSiteStatus> res in
							PXSelectReadonly2<SOLine,
							InnerJoin<INUnit, On<
								INUnit.inventoryID, Equal<SOLine.inventoryID>,
								And<INUnit.fromUnit, Equal<SOLine.uOM>>>,
							InnerJoin<INSiteStatus, On<
								SOLine.inventoryID, Equal<INSiteStatus.inventoryID>,
								And<SOLine.subItemID, Equal<INSiteStatus.subItemID>,
								And<SOLine.siteID, Equal<INSiteStatus.siteID>>>>>>,
							Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
								And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
							.Select(sender.Graph))
							{
								INSiteStatus status = res;
								INUnit unit = res;

								INUnit.UK.ByInventory.StoreCached(sender.Graph, unit);
							INSiteStatus.PK.StoreCached(sender.Graph, status);
							}

							foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<SOOrder.noteID>>>>.Select(this._Graph))
							{
								PXSelect<INItemPlan, 
								Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
								.StoreCached(this._Graph, new PXCommandKey(new object[] { plan.PlanID }), new List<object> { plan });
							}
						}

					if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
					{
						IQtyAllocated result = AvailabilityFetch(sender, copy, AvailabilityFetchMode.None);
						return DeductAllocated(sender, (SOLine)Row, result);
					}
				}

				return AvailabilityFetch(sender, copy, fetchMode);
			}
			return null;
		}

		public virtual IQtyAllocated DeductAllocated(PXCache sender, SOLine soLine, IQtyAllocated result)
		{
			if (result == null) return null;
			decimal? lineQtyAvail = (decimal?) sender.GetValue<SOLine.lineQtyAvail>(soLine);
			decimal? lineQtyHardAvail = (decimal?) sender.GetValue<SOLine.lineQtyHardAvail>(soLine);

			if (lineQtyAvail == null || lineQtyHardAvail == null)
			{
				lineQtyAvail = 0m;
				lineQtyHardAvail = 0m;

				foreach (SOLineSplit split in SelectDetail(DetailCache, soLine))
				{
					SOLineSplit detail = split;
					if (detail.PlanID != null)
					{
						INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this._Graph, detail.PlanID);
						if (plan != null)
						{
							detail = PXCache<SOLineSplit>.CreateCopy(detail);
							detail.PlanType = plan.PlanType;
						}
					}

					PXParentAttribute.SetParent(DetailCache, detail, typeof(SOLine), soLine);

					decimal signQtyAvail;
					decimal signQtyHardAvail;
					INItemPlanIDAttribute.GetInclQtyAvail<SiteStatus>(DetailCache, detail, out signQtyAvail, out signQtyHardAvail);

					if (signQtyAvail != 0m)
					{
						lineQtyAvail -= signQtyAvail * (detail.BaseQty ?? 0m);
					}

					if (signQtyHardAvail != 0m)
					{
						lineQtyHardAvail -= signQtyHardAvail * (detail.BaseQty ?? 0m);
					}
				}

				sender.SetValue<SOLine.lineQtyAvail>(soLine, lineQtyAvail);
				sender.SetValue<SOLine.lineQtyHardAvail>(soLine, lineQtyHardAvail);
			}

			result.QtyAvail += lineQtyAvail;
			result.QtyHardAvail += lineQtyHardAvail;
			result.QtyNotAvail = -lineQtyAvail;

			return result;
		}

		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var fetchMode = ((SOLine) e.Row)?.Completed == true
				? AvailabilityFetchMode.None
				: AvailabilityFetchMode.ExcludeCurrent;
			IQtyAllocated availability = AvailabilityFetch(sender, (SOLine)e.Row, fetchMode | AvailabilityFetchMode.TryOptimize);

			if (availability != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((SOLine)e.Row).InventoryID);

				decimal unitRate = INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, 1m, INPrecision.NOROUND);
				availability.QtyOnHand = PXDBQuantityAttribute.Round((decimal)availability.QtyOnHand * unitRate);
				availability.QtyAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyAvail * unitRate);
				availability.QtyNotAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyNotAvail * unitRate);
				availability.QtyHardAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyHardAvail * unitRate);

				if(IsAllocationEntryEnabled)
				{
					Decimal? allocated = PXDBQuantityAttribute.Round((decimal)(((SOLine)e.Row).LineQtyHardAvail ?? 0m) * unitRate); ;
					e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_AllocatedInfo,
							sender.GetValue<SOLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));
				}
				else
					e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_Info,
							sender.GetValue<SOLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));


				AvailabilityCheck(sender, (SOLine)e.Row, availability);
			}
			else
			{
				//handle missing UOM
				INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}

		protected SOOrder _LastSelected;

		protected virtual void Parent_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (_LastSelected == null || !object.ReferenceEquals(_LastSelected, e.Row))
			{
                PXUIFieldAttribute.SetRequired<SOLine.locationID>(this.MasterCache, IsLocationEnabled);
                PXUIFieldAttribute.SetVisible<SOLine.locationID>(this.MasterCache, null, IsLocationEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.lotSerialNbr>(this.MasterCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.expireDate>(this.MasterCache, null, IsLSEntryEnabled);

                PXUIFieldAttribute.SetVisible<SOLineSplit.inventoryID>(this.DetailCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<SOLineSplit.locationID>(this.DetailCache, null, IsLocationEnabled);
				//PXUIFieldAttribute.SetVisible<SOLineSplit.lotSerialNbr>(this.DetailCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.expireDate>(this.DetailCache, null, IsLSEntryEnabled);

				PXUIFieldAttribute.SetVisible<SOLineSplit.shipDate>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.isAllocated>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.completed>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.shippedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.shipmentNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.pOType>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.pONbr>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.pOReceiptNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.pOSource>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.pOCreate>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.receivedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<SOLineSplit.refNoteID>(this.DetailCache, null, IsAllocationEntryEnabled);

                PXView view;
                if (sender.Graph.Views.TryGetValue(Prefixed("lotseropts"), out view))
                {
                    view.AllowSelect = IsLSEntryEnabled;
                }

				if (e.Row is SOOrder)
				{
					_LastSelected = (SOOrder)e.Row;
				}
			}
			this.SetEnabled(IsLSEntryEnabled || IsAllocationEntryEnabled);
		}

		protected virtual void IssueAvailable(PXCache sender, SOLine Row, decimal? BaseQty)
		{
			IssueAvailable(sender, Row, BaseQty, false);
		}

		protected virtual void IssueAvailable(PXCache sender, SOLine Row, decimal? BaseQty, bool isUncomplete)
		{
			DetailCounters.Remove(Row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			foreach (INSiteStatus avail in PXSelectReadonly<INSiteStatus,
				Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
				And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
				And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>,
				OrderBy<Asc<INLocation.pickPriority>>>.Select(this._Graph, Row.InventoryID, Row.SubItemID, Row.SiteID))
			{
				SOLineSplit split = (SOLineSplit)Row;
                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }
				split.SplitLineNbr = null;
				split.IsAllocated = Row.RequireAllocation;
                split.SiteID = Row.SiteID;

                object newval;
                DetailCache.RaiseFieldDefaulting<SOLineSplit.allocatedPlanType>(split, out newval);
                DetailCache.SetValue<SOLineSplit.allocatedPlanType>(split, newval);

                DetailCache.RaiseFieldDefaulting<SOLineSplit.backOrderPlanType>(split, out newval);
                DetailCache.SetValue<SOLineSplit.backOrderPlanType>(split, newval);

				decimal SignQtyAvail;
				decimal SignQtyHardAvail;
				INItemPlanIDAttribute.GetInclQtyAvail<SiteStatus>(DetailCache, split, out SignQtyAvail, out SignQtyHardAvail);

				if (SignQtyHardAvail < 0m)
				{
					SiteStatus accumavail = new SiteStatus();
					PXCache<INSiteStatus>.RestoreCopy(accumavail, avail);

					accumavail = (SiteStatus)this._Graph.Caches[typeof(SiteStatus)].Insert(accumavail);

					decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

					if (AvailableQty <= 0m)
					{
						continue;
					}

					if (AvailableQty < BaseQty)
					{
						split.BaseQty = AvailableQty;
						split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)AvailableQty, INPrecision.QUANTITY);
						DetailCache.Insert(split);

						BaseQty -= AvailableQty;
					}
					else
					{
						split.BaseQty = BaseQty;
						split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)BaseQty, INPrecision.QUANTITY);
						DetailCache.Insert(split);

						BaseQty = 0m;
						break;
					}
				}
			}

			if (BaseQty > 0m && Row.InventoryID != null && Row.SiteID != null && (Row.SubItemID != null || (Row.SubItemID == null && Row.IsStockItem != true && Row.IsKit == true) || Row.LineType == SOLineType.NonInventory))
			{
				SOLineSplit split = (SOLineSplit)Row;
                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }
				split.SplitLineNbr = null;
				split.IsAllocated = false;
				split.BaseQty = BaseQty;
				split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

				BaseQty = 0m;

				if (isUncomplete)
				{
					split.POCreate = false;
					split.POSource = null;
				}

				DetailCache.Insert(PXCache<SOLineSplit>.CreateCopy(split));
			}
		}

		public override void UpdateParent(PXCache sender, SOLine Row)
		{
			if (Row != null && Row.RequireShipping == true && !IsSplitRequired(sender, Row))
			{
				decimal BaseQty;
				UpdateParent(sender, Row, null, null, out BaseQty);
			}
			else
			{
				base.UpdateParent(sender, Row);
			}
		}

		public override void UpdateParent(PXCache sender, SOLineSplit Row, SOLineSplit OldRow)
		{
			SOLine parent = (SOLine)LSParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(SOLine));

			if (parent != null && parent.RequireShipping == true)
			{
				if ((Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
				{
					SOLine oldrow = PXCache<SOLine>.CreateCopy(parent);
					decimal BaseQty;

					UpdateParent(sender, parent, (Row != null && Row.Completed == false ? Row : null), (OldRow != null && OldRow.Completed == false ? OldRow : null), out BaseQty);

					using (InvtMultScope<SOLine> ms = new InvtMultScope<SOLine>(parent))
					{
                        if (IsLotSerialRequired && Row != null)
                        {
                            parent.UnassignedQty = 0m;
                            if (IsLotSerialItem(sender, Row))
                            {
                                object[] splits = SelectDetail(sender, Row);
                                foreach (SOLineSplit split in splits)
                                {
                                    if (split.LotSerialNbr == null)
                                    {
                                        parent.UnassignedQty += split.BaseQty;
                                    }
                                }
                            }
                        }
						parent.BaseQty = BaseQty + parent.BaseClosedQty;
						parent.Qty = INUnitAttribute.ConvertFromBase(sender, parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
					}

					sender.Graph.Caches[typeof(SOLine)].MarkUpdated(parent);

					sender.Graph.Caches[typeof(SOLine)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
					if (sender.Graph.Caches[typeof(SOLine)].RaiseRowUpdating(oldrow, parent))
					{
						sender.Graph.Caches[typeof(SOLine)].RaiseRowUpdated(parent, oldrow);
					}
					else
					{
						sender.Graph.Caches[typeof(SOLine)].RestoreCopy(parent, oldrow);
					}
				}
			}
			else
			{
				base.UpdateParent(sender, Row, OldRow);
			}
		}

        public static void ResetAvailabilityCounters(SOLine row)
        {
            row.LineQtyAvail = null;
            row.LineQtyHardAvail = null;
        }

		public override void UpdateParent(PXCache sender, SOLine Row, SOLineSplit Det, SOLineSplit OldDet, out decimal BaseQty)
		{
            ResetAvailabilityCounters(Row);

			bool counted = DetailCounters.ContainsKey(Row);

			base.UpdateParent(sender, Row, Det, OldDet, out BaseQty);

			if (!counted && OldDet != null)
			{
				Counters counters;
				if (DetailCounters.TryGetValue(Row, out counters))
				{
					if (OldDet.POCreate == true)
					{
						counters.BaseQty += (decimal)OldDet.BaseReceivedQty + (decimal)OldDet.BaseShippedQty;
					}
					//if (OldDet.ShipmentNbr != null)
					//{
					//    counters.BaseQty += (decimal)(OldDet.BaseQty - OldDet.BaseShippedQty);
					//}
					BaseQty = counters.BaseQty;
				}
			}
		}

		protected override void UpdateCounters(PXCache sender, Counters counters, SOLineSplit detail)
		{
			base.UpdateCounters(sender, counters, detail);

			if (detail.POCreate == true)
			{
                //base shipped qty in context of purchase for so is meaningless and equals zero, so it's appended for dropship context
				counters.BaseQty -= (decimal)detail.BaseReceivedQty + (decimal)detail.BaseShippedQty;
			}

			if (IsAllocationEntryEnabled)
			{
				counters.LotSerNumbersNull = -1;
				counters.LotSerNumber = null;
				counters.LotSerNumbers.Clear();
			}

			//if (detail.ShipmentNbr != null)
			//{
			//    counters.BaseQty -= (decimal)(detail.BaseQty - detail.BaseShippedQty);
			//}
		}

		protected int _detailsRequested = 0;

		protected override object[] SelectDetail(PXCache sender, SOLineSplit row)
		{
			return SelectDetail(sender, row, true);
		}

		protected virtual object[] SelectDetail(PXCache sender, SOLineSplit row, bool ExcludeCompleted = true)
		{
			object[] ret;
			if (_detailsRequested > 5)
			{
				ret = PXParentAttribute.SelectSiblings(sender, row, typeof(SOOrder));

				return Array.FindAll(ret, a =>
					SameInventoryItem((SOLineSplit)a, row) && ((SOLineSplit)a).LineNbr == row.LineNbr && (((SOLineSplit)a).Completed == false || ExcludeCompleted == false && ((SOLineSplit)a).PONbr == null && ((SOLineSplit)a).SOOrderNbr == null));
			}

			ret = base.SelectDetail(sender, row);
			return Array.FindAll<object>(ret, a => (((SOLineSplit)a).Completed == false || ExcludeCompleted == false && ((SOLineSplit)a).PONbr == null && ((SOLineSplit)a).SOOrderNbr == null));
		}


		protected override object[] SelectDetail(PXCache sender, SOLine row)
		{
			object[] ret;
			if (_detailsRequested > 5)
			{
				ret = PXParentAttribute.SelectSiblings(sender, Convert(row), typeof(SOOrder));

				return Array.FindAll(ret, a =>
					SameInventoryItem((SOLineSplit)a, row) && ((SOLineSplit)a).LineNbr == row.LineNbr && ((SOLineSplit)a).Completed == false);
			}

			ret = base.SelectDetail(sender, row);
			return Array.FindAll<object>(ret, a => ((SOLineSplit)a).Completed == false);
		}

		protected override object[] SelectDetailOrdered(PXCache sender, SOLineSplit row)
		{
			return SelectDetailOrdered(sender, row, true);
		}

		protected virtual object[] SelectDetailOrdered(PXCache sender, SOLineSplit row, bool ExcludeCompleted=true)
		{
			object[] ret = SelectDetail(sender, row, ExcludeCompleted);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aIsAllocated = ((SOLineSplit)a).Completed == true ? 0 : ((SOLineSplit)a).IsAllocated == true ? 1 : 2;
				object bIsAllocated = ((SOLineSplit)b).Completed == true ? 0 : ((SOLineSplit)b).IsAllocated == true ? 1 : 2;

				int res = ((IComparable)aIsAllocated).CompareTo(bIsAllocated);

				if (res != 0)
				{
					return res;
				}

				object aSplitLineNbr = ((SOLineSplit)a).SplitLineNbr;
				object bSplitLineNbr = ((SOLineSplit)b).SplitLineNbr;

				return ((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		protected override object[] SelectDetailReversed(PXCache sender, SOLineSplit row)
		{
			return SelectDetailReversed(sender, row, true);
		}

		protected virtual object[] SelectDetailReversed(PXCache sender, SOLineSplit row, bool ExcludeCompleted=true)
		{
			object[] ret = SelectDetail(sender, row, ExcludeCompleted);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aIsAllocated = ((SOLineSplit)a).Completed == true ? 0 : ((SOLineSplit)a).IsAllocated == true ? 1 : 2;
				object bIsAllocated = ((SOLineSplit)b).Completed == true ? 0 : ((SOLineSplit)b).IsAllocated == true ? 1 : 2;

				int res = -((IComparable)aIsAllocated).CompareTo(bIsAllocated);

				if (res != 0)
				{
					return res;
				}

				object aSplitLineNbr = ((SOLineSplit)a).SplitLineNbr;
				object bSplitLineNbr = ((SOLineSplit)b).SplitLineNbr;

				return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		public virtual void UncompleteSchedules(PXCache sender, SOLine Row)
		{
			DetailCounters.Remove(Row);

			decimal? UnshippedQty = Row.BaseOpenQty;

			foreach (object detail in SelectDetailReversed(DetailCache, Row, false))
			{
				if (((SOLineSplit)detail).ShipmentNbr == null)
				{
					UnshippedQty -= ((SOLineSplit)detail).BaseQty;

					SOLineSplit newdetail = PXCache<SOLineSplit>.CreateCopy((SOLineSplit)detail);
					newdetail.Completed = false;

					DetailCache.Update(newdetail);
				}
			}

			IssueAvailable(sender, Row, (decimal)UnshippedQty, true);
		}

		public virtual void CompleteSchedules(PXCache sender, SOLine Row)
		{
			DetailCounters.Remove(Row);

			string LastShipmentNbr = null;
			decimal? LastUnshippedQty = 0m;
			foreach (object detail in SelectDetailReversed(DetailCache, Row, false))
			{
				if (LastShipmentNbr == null && ((SOLineSplit)detail).ShipmentNbr != null)
				{
					LastShipmentNbr = ((SOLineSplit)detail).ShipmentNbr;
				}

				if (LastShipmentNbr != null && ((SOLineSplit)detail).ShipmentNbr == LastShipmentNbr)
				{
					LastUnshippedQty += ((SOLineSplit)detail).BaseOpenQty;
				}
			}

			TruncateSchedules(sender, Row, (decimal)LastUnshippedQty);

			foreach (object detail in SelectDetailReversed(DetailCache, Row))
			{
				SOLineSplit newdetail = PXCache<SOLineSplit>.CreateCopy((SOLineSplit)detail);
				newdetail.Completed = true;

				DetailCache.Update(newdetail);
			}
		}

		public virtual void TruncateSchedules(PXCache sender, SOLine Row, decimal BaseQty)
		{
			DetailCounters.Remove(Row);
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

			foreach (object detail in SelectDetailReversed(DetailCache, Row))
			{
				if (BaseQty >= ((ILSDetail)detail).BaseQty)
				{
					BaseQty -= (decimal)((ILSDetail)detail).BaseQty;
					DetailCache.Delete(detail);
				}
				else
				{
					SOLineSplit newdetail = PXCache<SOLineSplit>.CreateCopy((SOLineSplit)detail);
					newdetail.BaseQty -= BaseQty;
					newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

					DetailCache.Update(newdetail);
					break;
				}
			}
		}

		protected virtual void IssueAvailable(PXCache sender, SOLine Row)
		{
			IssueAvailable(sender, Row, Row.BaseOpenQty);
		}

		protected override void _Master_RowInserted(PXCache sender, PXRowInsertedEventArgs<SOLine> e)
		{
			SOLine row = e.Row as SOLine;
			if (row == null) return;

			if (IsSplitRequired(sender, row))
			{
				base._Master_RowInserted(sender, e);
			}
			else
			{
				sender.SetValue<SOLine.locationID>(e.Row, null);
				sender.SetValue<SOLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<SOLine.expireDate>(e.Row, null);

                if (IsAllocationEntryEnabled && e.Row != null && e.Row.BaseOpenQty != 0m)
				{
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

					//if (e.Row.InvtMult == -1 && item != null && (e.Row.LineType == SOLineType.Inventory || e.Row.LineType == SOLineType.NonInventory))
                    if (item != null && (e.Row.LineType == SOLineType.Inventory || e.Row.LineType == SOLineType.NonInventory))
                    {
						IssueAvailable(sender, e.Row);

					}
				}
				AvailabilityCheck(sender, e.Row);
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			try
			{
				using (ResolveNotDecimalUnitErrorRedirectorScope<SOLineSplit.qty>(e.Row))
					base.Master_RowUpdated(sender, e);
			}
			catch (PXUnitConversionException ex)
			{
				if (!PXUIFieldAttribute.GetErrors(sender, e.Row, PXErrorLevel.Error).Keys.Any(a => string.Compare(a, typeof(SOLine.uOM).Name, StringComparison.InvariantCultureIgnoreCase) == 0))
					sender.RaiseExceptionHandling<SOLine.uOM>(e.Row, null, ex);
			}
		}

		protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<SOLine> e)
		{
			SOLine row = e.Row as SOLine;
            if (row == null) return;

			if (IsSplitRequired(sender, row, out InventoryItem ii)) //check condition
			{
				base._Master_RowUpdated(sender, e);

				if (ii != null && (ii.KitItem == true || ii.StkItem == true ))
				{
					AvailabilityCheck(sender, (SOLine)e.Row);
				}
			}
			else 
			{
				sender.SetValue<SOLine.locationID>(e.Row, null);
				sender.SetValue<SOLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<SOLine.expireDate>(e.Row, null);

				if (IsAllocationEntryEnabled)
				{
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

					if (e.OldRow != null && (e.OldRow.InventoryID != e.Row.InventoryID || e.OldRow.SiteID != e.Row.SiteID || e.OldRow.SubItemID != e.Row.SubItemID || e.OldRow.InvtMult != e.Row.InvtMult || e.OldRow.UOM != e.Row.UOM))
					{
						RaiseRowDeleted(sender, e.OldRow);
						RaiseRowInserted(sender, e.Row);
					}
					//else if (e.Row.InvtMult == -1 && item != null && (e.Row.LineType == SOLineType.Inventory || e.Row.LineType == SOLineType.NonInventory))
                    else if (item != null && (e.Row.LineType == SOLineType.Inventory || e.Row.LineType == SOLineType.NonInventory))
					{
                        // prevent setting null to quantity from mobile app
                        if (this._Graph.IsMobile && e.Row.OrderQty == null)
                        {
                            e.Row.OrderQty = e.OldRow.OrderQty;
                        }

                        //ConfirmShipment(), CorrectShipment() use SuppressedMode and never end up here.
                        //OpenQty is calculated via formulae, ExternalCall is used to eliminate duplicating formula arguments here
                        //direct OrderQty for AddItem()
                        if (e.Row.OrderQty != e.OldRow.OrderQty || e.Row.Completed != e.OldRow.Completed)
						{
							e.Row.BaseOpenQty = INUnitAttribute.ConvertToBase(sender, e.Row.InventoryID, e.Row.UOM, (decimal)e.Row.OpenQty, e.Row.BaseOpenQty, INPrecision.QUANTITY);

							//mimic behavior of Shipment Confirmation where at least one schedule will always be present for processed line
							//but additional schedules will never be created and thus should be truncated when ShippedQty > 0
							if (e.Row.Completed == true && e.OldRow.Completed == false)
							{
								CompleteSchedules(sender, e.Row);
								UpdateParent(sender, e.Row);
							}
							else if (e.Row.Completed == false && e.OldRow.Completed == true)
							{
								UncompleteSchedules(sender, e.Row);
								UpdateParent(sender, e.Row);
							}
							else if (e.Row.BaseOpenQty > e.OldRow.BaseOpenQty)
							{
								IssueAvailable(sender, e.Row, (decimal)e.Row.BaseOpenQty - (decimal)e.OldRow.BaseOpenQty);
								UpdateParent(sender, e.Row);
							}
							else if (e.Row.BaseOpenQty < e.OldRow.BaseOpenQty)
							{
								TruncateSchedules(sender, e.Row, (decimal)e.OldRow.BaseOpenQty - (decimal)e.Row.BaseOpenQty);
								UpdateParent(sender, e.Row);
							}
						}
						
						if (!sender.ObjectsEqual<SOLine.pOCreate, SOLine.pOSource, SOLine.vendorID, SOLine.pOSiteID>(e.Row, e.OldRow))
						{
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                SOLineSplit split = detail as SOLineSplit;
                                if (split.IsAllocated == false && split.Completed == false && split.PONbr == null)
                                {
                                    split.POCreate = row.POCreate;
                                    split.POSource = row.POSource;
                                    split.VendorID = row.VendorID;
                                    split.POSiteID = row.POSiteID;

                                    DetailCache.Update(split);
                                }
                            }
						}

                        if (!sender.ObjectsEqual<SOLine.shipDate>(e.Row, e.OldRow) || 
							(!sender.ObjectsEqual<SOLine.shipComplete>(e.Row, e.OldRow) && ((SOLine)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed))
                        {
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                SOLineSplit split = detail as SOLineSplit;
                                split.ShipDate = row.ShipDate;

                                DetailCache.Update(split);
                            }
                        }
					}
				}
				else
				{
					if (e.OldRow != null && e.OldRow.InventoryID != e.Row.InventoryID)
					{
						RaiseRowDeleted(sender, e.OldRow);
					}
				}

				AvailabilityCheck(sender, (SOLine)e.Row);
			}
		}

		protected virtual bool IsSplitRequired(PXCache sender, SOLine row)
			=> IsSplitRequired(sender, row, out InventoryItem item);

		protected virtual bool IsSplitRequired(PXCache sender, SOLine row, out InventoryItem item)
		{
			if (row == null)
			{
				item = null;
				return false;
			}

			bool skipSplitCreating = false;
			item = InventoryItem.PK.Find(sender.Graph, row.InventoryID);

			if (IsLocationEnabled && item != null && item.StkItem == false && item.KitItem == false && item.NonStockShip == false)
			{
				skipSplitCreating = true;
			}

			if (item != null && item.StkItem == false && item.KitItem == true && row.Behavior != SOBehavior.CM && row.Behavior != SOBehavior.IN)
			{
				skipSplitCreating = true;
			}

			return !skipSplitCreating && (IsLocationEnabled || (IsLotSerialRequired && row.POCreate != true && IsLotSerialItem(sender, row)));
		}

		protected virtual bool SchedulesEqual(SOLineSplit a, SOLineSplit b)
		{
			if (a != null && b != null)
			{
				return (a.InventoryID == b.InventoryID &&
								a.SubItemID == b.SubItemID &&
								a.SiteID == b.SiteID &&
                                a.ToSiteID == b.ToSiteID &&
								a.ShipDate == b.ShipDate &&
								a.IsAllocated == b.IsAllocated && 
                                a.IsMergeable != false && b.IsMergeable != false &&
								a.ShipmentNbr == b.ShipmentNbr &&
								a.Completed == b.Completed &&
								a.POCreate == b.POCreate &&
								a.POCompleted == b.POCompleted &&
								a.PONbr == b.PONbr &&
								a.POLineNbr == b.POLineNbr &&
								a.SOOrderType == b.SOOrderType &&
								a.SOOrderNbr == b.SOOrderNbr &&
								a.SOLineNbr == b.SOLineNbr &&
								a.SOSplitLineNbr == b.SOSplitLineNbr);
			}
			else
			{
				return (a != null);
			}
		}

		protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (IsLSEntryEnabled)
			{
				if (e.ExternalCall)
				{
					if (((SOLineSplit)e.Row).LineType != SOLineType.Inventory)
					{
						throw new PXSetPropertyException(ErrorMessages.CantInsertRecord);
					}
				}

				base.Detail_RowInserting(sender, e);

                if (e.Row != null && !IsLocationEnabled && ((SOLineSplit)e.Row).LocationID != null)
                {
                    ((SOLineSplit)e.Row).LocationID = null;
                }
			}
			else if (IsAllocationEntryEnabled)
			{
				SOLineSplit a = (SOLineSplit)e.Row;

				if (!e.ExternalCall && _Operation == PXDBOperation.Update)
				{
					foreach (object item in SelectDetail(sender, (SOLineSplit)e.Row))
					{
						SOLineSplit detailitem = (SOLineSplit)item;

						if (SchedulesEqual((SOLineSplit)e.Row, detailitem))
						{
							object old_item = PXCache<SOLineSplit>.CreateCopy(detailitem);
							detailitem.BaseQty += ((SOLineSplit)e.Row).BaseQty;
							detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseQty, INPrecision.QUANTITY);

							detailitem.BaseUnreceivedQty += ((SOLineSplit)e.Row).BaseQty;
							detailitem.UnreceivedQty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseUnreceivedQty, INPrecision.QUANTITY);

							sender.Current = detailitem;
							sender.RaiseRowUpdated(detailitem, old_item);
							sender.MarkUpdated(detailitem);
							PXDBQuantityAttribute.VerifyForDecimal(sender, detailitem);
							e.Cancel = true;
							break;
						}
					}
				}

				if (((SOLineSplit)e.Row).InventoryID == null || string.IsNullOrEmpty(((SOLineSplit)e.Row).UOM))
				{
					e.Cancel = true;
				}

				if (!e.Cancel)
				{
				}
			}
		}

		protected virtual bool Allocated_Updated(PXCache sender, EventArgs e)
		{
			SOLineSplit split = (SOLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
			SiteStatus accum = new SiteStatus();
			accum.InventoryID = split.InventoryID;
			accum.SiteID = split.SiteID;
			accum.SubItemID = split.SubItemID;

			accum = (SiteStatus)sender.Graph.Caches[typeof(SiteStatus)].Insert(accum);
			accum = PXCache<SiteStatus>.CreateCopy(accum);

			INSiteStatus stat = INSiteStatus.PK.Find(sender.Graph, split.InventoryID, split.SubItemID, split.SiteID);
			if (stat != null)
			{
				accum.QtyAvail += stat.QtyAvail;
				accum.QtyHardAvail += stat.QtyHardAvail;
			}

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);
			if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult))
            {
                if (split.LotSerialNbr != null)
                {
                    LotSerialNbr_Updated(sender, e);
                    return true;
                }
            }
            else
            {
			if (accum.QtyHardAvail < 0m)
			{
				SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(split);
				if (split.BaseQty + accum.QtyHardAvail > 0m)
				{
					split.BaseQty += accum.QtyHardAvail;
					split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
						sender.RaiseFieldUpdated(sender.GetField(typeof(SOLineSplit.qty)), split, split.Qty);
				}
				else
				{
					split.IsAllocated = false;
					sender.RaiseExceptionHandling<SOLineSplit.isAllocated>(split, true, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
				}

				sender.RaiseFieldUpdated(sender.GetField(typeof(SOLineSplit.isAllocated)), split, copy.IsAllocated);
				sender.RaiseRowUpdated(split, copy);

				if (split.IsAllocated == true)
				{
					copy.SplitLineNbr = null;
					copy.PlanID = null;
					copy.IsAllocated = false;
					copy.BaseQty = -accum.QtyHardAvail;
					copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
						copy.OpenQty = null;
						copy.BaseOpenQty = null;
						copy.UnreceivedQty = null;
						copy.BaseUnreceivedQty = null;

					sender.Insert(copy);
				}
				RefreshView(sender);
				
				return true;
			}
            }
			return false;
		}

        protected virtual bool LotSerialNbr_Updated(PXCache sender, EventArgs e)
        {

            SOLineSplit split = (SOLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            SOLineSplit oldsplit = (SOLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).OldRow : ((PXRowInsertedEventArgs)e).Row);

            SiteLotSerial accum = new SiteLotSerial();

            accum.InventoryID = split.InventoryID;
            accum.SiteID = split.SiteID;
            accum.LotSerialNbr = split.LotSerialNbr;

            accum = (SiteLotSerial)sender.Graph.Caches[typeof(SiteLotSerial)].Insert(accum);
            accum = PXCache<SiteLotSerial>.CreateCopy(accum);

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);

            INSiteLotSerial siteLotSerial = PXSelectReadonly<INSiteLotSerial, Where<INSiteLotSerial.inventoryID, Equal<Required<INSiteLotSerial.inventoryID>>, And<INSiteLotSerial.siteID, Equal<Required<INSiteLotSerial.siteID>>,
                And<INSiteLotSerial.lotSerialNbr, Equal<Required<INSiteLotSerial.lotSerialNbr>>>>>>.Select(sender.Graph, split.InventoryID, split.SiteID, split.LotSerialNbr);

            if (siteLotSerial != null)
            {
                accum.QtyAvail += siteLotSerial.QtyAvail;
                accum.QtyHardAvail += siteLotSerial.QtyHardAvail;
            }

			Lazy<bool> externalCall = Lazy.By(() =>
			{
				bool extCall = false;
				if (e is PXRowUpdatedEventArgs) extCall = ((PXRowUpdatedEventArgs)e).ExternalCall;
				if (e is PXRowInsertedEventArgs) extCall = ((PXRowInsertedEventArgs)e).ExternalCall;
				return extCall;
			});

            //Serial-numbered items
            if (INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null)
            {
                SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(split);
                if (siteLotSerial != null && siteLotSerial.QtyAvail > 0 && siteLotSerial.QtyHardAvail > 0)
                {
                    if (split.Operation != SOOperation.Receipt)
                    {
                        split.BaseQty = 1;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        split.IsAllocated = true;
                    }
                    else
                    {
                        split.IsAllocated = false;
                        sender.RaiseExceptionHandling<SOLineSplit.lotSerialNbr>(split, null, new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(IN.Messages.SerialNumberAlreadyReceived, ((InventoryItem)item).InventoryCD, split.LotSerialNbr)));
                    }
                }
                else
                {

                    if (split.Operation != SOOperation.Receipt)
                    {
						if (externalCall.Value)
                        {
                            split.IsAllocated = false;
                            split.LotSerialNbr = null;
                            sender.RaiseExceptionHandling<SOLineSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
                            if (split.IsAllocated == true)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        split.BaseQty = 1;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                    }
                }

                sender.RaiseFieldUpdated(sender.GetField(typeof(SOLineSplit.isAllocated)), split, copy.IsAllocated);
                sender.RaiseRowUpdated(split, copy);

                if (copy.BaseQty - 1 > 0m)
                {
                    if (split.IsAllocated == true || (split.IsAllocated != true && split.Operation == SOOperation.Receipt))
                    {
                        copy.SplitLineNbr = null;
                        copy.PlanID = null;
                        copy.IsAllocated = false;
                        copy.LotSerialNbr = null;
                        copy.BaseQty -= 1;
                        copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
                        sender.Insert(copy);
                    }
                    RefreshView(sender);
                    return true;
                }
            }
            //Lot-numbered items
            else if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null && !INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult))
            {
				if (split.BaseQty > 0m)
				{
					//Lot/Serial Nbr. selected on non-allocated line. Trying to allocate line first. Verification of Qty. available for allocation will be performed on the next pass-through
					if (split.IsAllocated == false)
					{
						if (siteLotSerial == null || (((siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail <= 0m) || siteLotSerial.QtyOnHand <= 0m) && split.Operation != SOOperation.Receipt))
						{
							if (externalCall.Value)
							{
							return NegativeInventoryError(sender, split);
						}
						}
						else
						{
						SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(split);
						split.IsAllocated = true;
						sender.RaiseFieldUpdated(sender.GetField(typeof(SOLineSplit.isAllocated)), split, copy.IsAllocated);
						sender.RaiseRowUpdated(split, copy);
						}
						return true;
					}

					//Lot/Serial Nbr. selected on allocated line. Available Qty. verification procedure 
					if (split.IsAllocated == true)
					{
						SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(split);
						if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail >= 0m && split.Operation != SOOperation.Receipt)
						{
							split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
						}
						else if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail < 0m && split.Operation != SOOperation.Receipt)
						{
							split.BaseQty += accum.QtyHardAvail;
							if (split.BaseQty <= 0m)
							{
								if (NegativeInventoryError(sender, split)) return false;
							}
							split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
						}
						else if (siteLotSerial == null || (siteLotSerial.QtyOnHand <= 0m && split.Operation != SOOperation.Receipt))
						{
							if (NegativeInventoryError(sender, split)) return false;
						}

						INItemPlanIDAttribute.RaiseRowUpdated(sender, split, copy);

						if ((copy.BaseQty - split.BaseQty) > 0m && split.IsAllocated == true)
						{
							_InternallCall = true;
							try
							{
								copy.SplitLineNbr = null;
								copy.PlanID = null;
								copy.IsAllocated = false;
								copy.LotSerialNbr = null;
								copy.BaseQty -= split.BaseQty;
								copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
								copy = (SOLineSplit)sender.Insert(copy);
								if (copy.LotSerialNbr != null && copy.IsAllocated != true)
								{
									sender.SetValue<SOLineSplit.lotSerialNbr>(copy, null);
								}
							}
							finally
							{
								_InternallCall = false;
							}
						}
						RefreshView(sender);

						return true;
					}
				}
            }
            return false;
        }

		protected virtual bool NegativeInventoryError(PXCache sender, SOLineSplit split)
		{
			split.IsAllocated = false;
			split.LotSerialNbr = null;
			sender.RaiseExceptionHandling<SOLineSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
			if (split.IsAllocated == true)
			{
				return true;
			}
			return false;
		}

		private void RefreshView(PXCache sender)
		{
			foreach (KeyValuePair<string, PXView> pair in sender.Graph.Views)
			{
				PXView view = pair.Value;
				if (view.IsReadOnly == false && view.GetItemType() == sender.GetItemType())
				{
					view.RequestRefresh();
				}
			}
		}

		protected override void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.Detail_RowInserted(sender, e);

			if ((_InternallCall == false || !string.IsNullOrEmpty(((SOLineSplit)e.Row).LotSerialNbr) && ((SOLineSplit)e.Row).IsAllocated != true) && IsAllocationEntryEnabled)
			{
                if (((SOLineSplit)e.Row).IsAllocated == true || (!string.IsNullOrEmpty(((SOLineSplit)e.Row).LotSerialNbr) && ((SOLineSplit)e.Row).IsAllocated != true))
				{
					Allocated_Updated(sender, e);

					sender.RaiseExceptionHandling<SOLineSplit.qty>(e.Row, null, null);
					AvailabilityCheck(sender, (SOLineSplit)e.Row);
				}
			}
		}

		protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Detail_RowUpdated(sender, e);

			if (_InternallCall == false && IsAllocationEntryEnabled)
			{
                if (!sender.ObjectsEqual<SOLineSplit.isAllocated>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLineSplit.pOLineNbr>(e.Row, e.OldRow) && ((SOLineSplit)e.Row).POLineNbr == null && ((SOLineSplit)e.Row).IsAllocated == false)
				{
					if (((SOLineSplit)e.Row).IsAllocated == true)
					{
						Allocated_Updated(sender, e);

						sender.RaiseExceptionHandling<SOLineSplit.qty>(e.Row, null, null);
						AvailabilityCheck(sender, (SOLineSplit) e.Row);
					}
					else
					{
						//clear link to created transfer
						SOLineSplit row = (SOLineSplit)e.Row;
						row.ClearSOReferences();

						foreach (SOLineSplit s in this.SelectDetailReversed(sender,(SOLineSplit) e.Row))
						{
							if(s.SplitLineNbr != ((SOLineSplit) e.Row).SplitLineNbr &&
								SchedulesEqual(s, (SOLineSplit)e.Row))
							{
								((SOLineSplit)e.Row).Qty += s.Qty;
								((SOLineSplit)e.Row).BaseQty += s.BaseQty;

								((SOLineSplit)e.Row).UnreceivedQty += s.Qty;
								((SOLineSplit)e.Row).BaseUnreceivedQty += s.BaseQty;

                                if (((SOLineSplit)e.Row).LotSerialNbr != null)
                                {
                                    SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy((SOLineSplit)e.Row);
                                    ((SOLineSplit)e.Row).LotSerialNbr = null;
                                    //sender.RaiseFieldUpdated(sender.GetField(typeof(SOLineSplit.isAllocated)), s, copy.IsAllocated);
                                    sender.RaiseRowUpdated((SOLineSplit)e.Row, copy);
                                }
								sender.SetStatus(s, sender.GetStatus(s) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                sender.ClearQueryCache();

								PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
								INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, ((SOLineSplit)e.Row).PlanID);
								if (plan != null)
								{
									plan.PlanQty += s.BaseQty;
									if (cache.GetStatus(plan) != PXEntryStatus.Inserted)
									{
										cache.SetStatus(plan, PXEntryStatus.Updated);
									}
								}

								INItemPlan old_plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, s.PlanID);
								if (old_plan != null)
								{
									cache.SetStatus(old_plan, cache.GetStatus(old_plan) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                    cache.ClearQueryCacheObsolete();

								}
								RefreshView(sender);								
							}
                            else if (s.SplitLineNbr == ((SOLineSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (SOLineSplit)e.Row) && ((SOLineSplit)e.Row).LotSerialNbr != null)
                            {
                                SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy((SOLineSplit)e.Row);
                                ((SOLineSplit)e.Row).LotSerialNbr = null;
                                sender.RaiseRowUpdated((SOLineSplit)e.Row, copy);
                            }
                        }
                    }
                }

                if (!sender.ObjectsEqual<SOLineSplit.lotSerialNbr>(e.Row, e.OldRow))
                {
                    if (((SOLineSplit)e.Row).LotSerialNbr != null)
                    {
                        LotSerialNbr_Updated(sender, e);

                        sender.RaiseExceptionHandling<SOLineSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (SOLineSplit)e.Row); //???
                    }
                    else
                    {
                        foreach (SOLineSplit s in this.SelectDetailReversed(sender, (SOLineSplit)e.Row))
                        {
                            if (s.SplitLineNbr == ((SOLineSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (SOLineSplit)e.Row))
                            {
                                SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(s);
                                ((SOLineSplit)e.Row).IsAllocated = false;
                                sender.RaiseFieldUpdated(sender.GetField(typeof(SOLineSplit.isAllocated)), (SOLineSplit)e.Row, ((SOLineSplit)e.Row).IsAllocated);
                                //sender.RaiseFieldUpdated(sender.GetField(typeof(SOLineSplit.isAllocated)), s, copy.IsAllocated);
                                sender.RaiseRowUpdated(s, copy);
                            }
						}
					}
				}

			}
		}

		public override void Detail_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSDetail)e.Row).InventoryID);

			if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				e.NewValue = ((InventoryItem)item).BaseUnit;
				e.Cancel = true;
			}
			else if (!IsAllocationEntryEnabled)
			{
				base.Detail_UOM_FieldDefaulting(sender, e);
			}
		}

		public override void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!IsAllocationEntryEnabled)
			{
				base.Detail_Qty_FieldVerifying(sender, e);
			}
			else
			{
				VerifySNQuantity(sender, e, (ILSDetail)e.Row, typeof(SOLineSplit.qty).Name);
			}
		}



		public override SOLineSplit Convert(SOLine item)
		{
			using (InvtMultScope<SOLine> ms = new InvtMultScope<SOLine>(item))
			{
				SOLineSplit ret = (SOLineSplit)item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;

				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}

		public virtual void SOLineSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOLineSplit split = e.Row as SOLineSplit;

			if (split != null)
			{
                bool isLineTypeInventory = (split.LineType == SOLineType.Inventory);
                object val = sender.GetValueExt<SOLineSplit.isAllocated>(e.Row);
                bool isAllocated = split.IsAllocated == true || (bool?)PXFieldState.UnwrapValue(val) == true;
                bool isCompleted = split.Completed == true;
                bool isIssue = split.Operation == SOOperation.Issue;
				bool IsLinked = split.PONbr != null || split.SOOrderNbr != null && split.IsAllocated == true;
				bool isPOSchedule = split.POCreate == true || split.POCompleted == true;

				SOLine parent = (SOLine)PXParentAttribute.SelectParent(sender, split, typeof(SOLine));
				PXUIFieldAttribute.SetEnabled<SOLineSplit.subItemID>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.completed>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shippedQty>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shipmentNbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<SOLineSplit.isAllocated>(sender, e.Row, isLineTypeInventory && isIssue && !isCompleted && !isPOSchedule);
                PXUIFieldAttribute.SetEnabled<SOLineSplit.siteID>(sender, e.Row, isLineTypeInventory && isAllocated && !IsLinked);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.qty>(sender, e.Row, !isCompleted && !IsLinked);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shipDate>(sender, e.Row, !isCompleted && parent?.ShipComplete == SOShipComplete.BackOrderAllowed);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.pONbr>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.pOReceiptNbr>(sender, e.Row, false);

                if (split.Completed == true)
                {
                    PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
                }
			}
		}

		public virtual void SOLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				bool RequireLocationAndSubItem = ((SOLineSplit)e.Row).RequireLocation == true && ((SOLineSplit)e.Row).IsStockItem == true && ((SOLineSplit)e.Row).BaseQty != 0m;

				PXDefaultAttribute.SetPersistingCheck<SOLineSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<SOLineSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}

		public virtual void SOLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOLine)];
			if (cache.Current != null && (e.Row == null || ((SOLine)cache.Current).LineNbr == ((SOLineSplit)e.Row).LineNbr && ((SOLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOLine)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void SOLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOLine)];
			if (cache.Current != null && (e.Row == null || ((SOLine)cache.Current).LineNbr == ((SOLineSplit)e.Row).LineNbr && ((SOLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOLine)cache.Current).LocationID;
                e.Cancel = (_InternallCall == true || e.NewValue != null || !IsLocationEnabled);
			}
		}

		public virtual void SOLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOLine)];
			if (cache.Current != null && (e.Row == null || ((SOLine)cache.Current).LineNbr == ((SOLineSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<SOLine> ms = new InvtMultScope<SOLine>((SOLine)cache.Current))
				{
					e.NewValue = ((SOLine)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
		{
			if (row is SOLine)
			{
				sender.RaiseExceptionHandling<SOLine.orderQty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<SOLine.inventoryID>(row), sender.GetStateExt<SOLine.subItemID>(row), sender.GetStateExt<SOLine.siteID>(row), sender.GetStateExt<SOLine.locationID>(row), sender.GetValue<SOLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<SOLineSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<SOLineSplit.inventoryID>(row), sender.GetStateExt<SOLineSplit.subItemID>(row), sender.GetStateExt<SOLineSplit.siteID>(row), sender.GetStateExt<SOLineSplit.locationID>(row), sender.GetValue<SOLineSplit.lotSerialNbr>(row)));
			}
		}

		protected void RaiseMemoQtyExceptionHanding<Target>(PXCache sender, SOLine Row, ILSMaster Split, Exception e)
			where Target : class, ILSMaster
		{
			if (typeof(Target) == typeof(SOLine))
			{
				sender.Graph.Caches[typeof(SOLine)].RaiseExceptionHandling<SOLine.orderQty>(Row, Row.OrderQty, new PXSetPropertyException(e.Message, sender.GetValueExt<SOLine.inventoryID>(Row), sender.GetValueExt<SOLine.subItemID>(Row), sender.GetValueExt<SOLine.invoiceNbr>(Row), sender.GetValueExt<SOLine.lotSerialNbr>(Row)));
			}
			else
			{
				PXCache cache = sender.Graph.Caches[typeof(SOLineSplit)];
				cache.RaiseExceptionHandling<SOLineSplit.qty>(Split, ((SOLineSplit)Split).Qty, new PXSetPropertyException(e.Message, sender.GetValueExt<SOLine.inventoryID>(Row), cache.GetValueExt<SOLineSplit.subItemID>(Split), sender.GetValueExt<SOLine.invoiceNbr>(Row), cache.GetValueExt<SOLineSplit.lotSerialNbr>(Split)));
			}
		}

		public virtual bool MemoAvailabilityCheck(PXCache sender, SOLine Row)
			=> MemoAvailabilityCheck(sender, Row, false);

		public virtual bool MemoAvailabilityCheck(PXCache sender, SOLine Row, bool persisting)
		{
			if (Row.Operation == SOOperation.Issue)
				return true;
			bool success = false;
			try
			{
				returnRecords = null;
				success = MemoAvailabilityCheckQty(sender, Row);
				if(!success)
				{
					RaiseException<SOLine.orderQty>(sender, Row, persisting,
						Messages.InvoiceCheck_DecreaseQty,
						sender.GetValueExt<SOLine.invoiceNbr>(Row),
						sender.GetValueExt<SOLine.inventoryID>(Row),
						returnRecords == null ? string.Empty : string.Join(", ", returnRecords.Select(x => x.DocumentNbr)));
				}
			}
			finally
			{
				returnRecords = null;
			}
			return success;
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		private ReturnRecord[] returnRecords;

		protected override bool CheckInvoicedAndReturnedQty(PXCache sender, int? returnInventoryID, IEnumerable<SOInvoicedRecords.Record> invoiced, IEnumerable<ReturnRecord> returned)
		{
			var success = base.CheckInvoicedAndReturnedQty(sender, returnInventoryID, invoiced, returned);
			if (!success)
			{
				string currentOrderNbr = ((SOLine)sender.Current)?.OrderNbr;
				returnRecords = returned.Where(x => x.DocumentNbr != currentOrderNbr).ToArray();
			}
			return success;
		}

		protected virtual void RaiseException<TField>(PXCache sender, object row, bool persisting, string errorMessage, params object[] args)
			where TField: IBqlField
		{
			object value = sender.GetValue<TField>(row);
			if (sender.RaiseExceptionHandling<TField>(row, value, new PXSetPropertyException(errorMessage, args)) && persisting)
				throw new PXRowPersistingException(typeof(TField).Name, value, errorMessage, args);
		}

		protected virtual bool MemoAvailabilityCheckQty(PXCache sender, SOLine row)
		{
			return MemoAvailabilityCheckQty(sender, row.InventoryID, row.InvoiceType, row.InvoiceNbr, row.InvoiceLineNbr, row.OrigOrderType, row.OrigOrderNbr, row.OrigLineNbr);
		}

		protected virtual bool MemoAvailabilityCheck<Target>(PXCache sender, SOLine Row, ILSMaster Split)
			where Target : class, ILSMaster
		{
			bool success = true;
			if (Row.InvoiceNbr != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Split.InventoryID);

				if (item != null && ((INLotSerClass)item).LotSerTrack != INLotSerTrack.NotNumbered && Split.SubItemID != null && string.IsNullOrEmpty(Split.LotSerialNbr) == false)
				{
					PXResult<INTran> orig_line = PXSelectJoinGroupBy<INTran,
					InnerJoin<INTranSplit, 
						On<INTranSplit.FK.Tran>>,
					Where<INTran.sOOrderType, Equal<Optional<SOLine.origOrderType>>, 
					And<INTran.sOOrderNbr, Equal<Optional<SOLine.origOrderNbr>>, 
					And<INTran.sOOrderLineNbr, Equal<Optional<SOLine.origLineNbr>>, 
					And<INTran.aRDocType, Equal<Optional<SOLine.invoiceType>>,
					And<INTran.aRRefNbr, Equal<Optional<SOLine.invoiceNbr>>,
					And<INTranSplit.inventoryID, Equal<Optional<SOLineSplit.inventoryID>>,
					And<INTranSplit.subItemID, Equal<Optional<SOLineSplit.subItemID>>,
					And<INTranSplit.lotSerialNbr, Equal<Optional<SOLineSplit.lotSerialNbr>>>>>>>>>>,
					Aggregate<GroupBy<INTranSplit.inventoryID, GroupBy<INTranSplit.subItemID, GroupBy<INTranSplit.lotSerialNbr, Sum<INTranSplit.baseQty>>>>>>.SelectSingleBound(sender.Graph, new object[] { Row, Split as SOLineSplit });

					PXResult<SOLine> memo_line = PXSelectJoinGroupBy<SOLine,
					InnerJoin<SOLineSplit, On<SOLineSplit.orderType, Equal<SOLine.orderType>, And<SOLineSplit.orderNbr, Equal<SOLine.orderNbr>, And<SOLineSplit.lineNbr, Equal<SOLine.lineNbr>>>>>,
					Where2<Where<SOLine.orderType, NotEqual<Optional<SOLine.orderType>>, Or<SOLine.orderNbr, NotEqual<Optional<SOLine.orderNbr>>>>, 
					And<SOLine.origOrderType, Equal<Optional<SOLine.origOrderType>>, 
					And<SOLine.origOrderNbr, Equal<Optional<SOLine.origOrderNbr>>, 
					And<SOLine.origLineNbr, Equal<Optional<SOLine.origLineNbr>>, 
					And<SOLine.invoiceType, Equal<Optional<SOLine.invoiceType>>,
					And<SOLine.invoiceNbr, Equal<Optional<SOLine.invoiceNbr>>,
					And<SOLine.operation, Equal<SOOperation.receipt>,
					And<SOLineSplit.inventoryID, Equal<Optional<SOLineSplit.inventoryID>>,
					And<SOLineSplit.subItemID, Equal<Optional<SOLineSplit.subItemID>>, 
					And<SOLineSplit.lotSerialNbr, Equal<Optional<SOLineSplit.lotSerialNbr>>,
					And<Where<
						SOLine.baseBilledQty, Greater<decimal0>, 
						Or2<Where<SOLine.baseShippedQty, Greater<decimal0>, And<SOLineSplit.baseShippedQty, Greater<decimal0>>>, 
						Or<Where<SOLine.completed, NotEqual<True>, And<SOLineSplit.completed, NotEqual<True>>>>>>>>>>>>>>>>>,
					Aggregate<GroupBy<SOLineSplit.inventoryID, GroupBy<SOLineSplit.subItemID, GroupBy<SOLineSplit.lotSerialNbr, Sum<SOLineSplit.baseQty, Sum<SOLineSplit.baseShippedQty>>>>>>>.SelectSingleBound(sender.Graph, new object[] { Row, Split as SOLineSplit });

					if (orig_line == null)
					{
						if (Split is SOLineSplit && string.IsNullOrEmpty(((SOLineSplit)Split).AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(((SOLineSplit)Split).AssignedNbr, ((SOLineSplit)Split).LotSerialNbr))
						{
							((SOLineSplit)Split).AssignedNbr = null;
							((SOLineSplit)Split).LotSerialNbr = null;
						}
						else
						{
							RaiseMemoQtyExceptionHanding<Target>(sender, Row, Split, new PXSetPropertyException(Messages.InvoiceCheck_LotSerialInvalid));
							success = false;
						}
						return success;
					}

					decimal? QtyInvoicedLotBase = ((INTranSplit)(PXResult<INTran, INTranSplit>)orig_line).BaseQty;

					if (memo_line != null)
					{
						if (((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
						{
							RaiseMemoQtyExceptionHanding<Target>(sender, Row, Split, new PXSetPropertyException(Messages.InvoiceCheck_SerialAlreadyReturned));
							success = false;
						}
						else
						{
							decimal returnedQty = ((SOLineSplit)(PXResult<SOLine, SOLineSplit>)memo_line).BaseShippedQty ?? 0m;
							if (returnedQty == 0)
								returnedQty = ((SOLineSplit)(PXResult<SOLine, SOLineSplit>)memo_line).BaseQty ?? 0m;

							QtyInvoicedLotBase -= returnedQty;
						}
					}

					if (Split is SOLine)
					{
						QtyInvoicedLotBase -= Split.BaseQty;
					}
					else
					{
                        foreach (SOLineSplit split in PXParentAttribute.SelectSiblings(sender.Graph.Caches[typeof(SOLineSplit)], Split, typeof(SOLine)))
						{
							if (object.Equals(split.SubItemID, Split.SubItemID) && object.Equals(split.LotSerialNbr, Split.LotSerialNbr))
							{
								QtyInvoicedLotBase -= split.BaseQty;
							}
						}
					}

					if (QtyInvoicedLotBase < 0m)
					{
						RaiseMemoQtyExceptionHanding<Target>(sender, Row, Split, new PXSetPropertyException(Messages.InvoiceCheck_QtyLotSerialNegative));
						success = false;
					}
				}
			}
			return success;
		}

		public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
		{
			base.AvailabilityCheck(sender, Row);

			if (Row is SOLine)
			{
				MemoAvailabilityCheck(sender, (SOLine)Row);

				SOLineSplit copy = Convert(Row as SOLine);

				if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
				{
					DefaultLotSerialNbr(sender.Graph.Caches[typeof(SOLineSplit)], copy);
				}

				MemoAvailabilityCheck<SOLine>(sender, (SOLine)Row, copy);

				if (copy.LotSerialNbr == null)
				{
					Row.LotSerialNbr = null;
				}
			}
			else
			{
				object parent = PXParentAttribute.SelectParent(sender, Row, typeof(SOLine));
				MemoAvailabilityCheck(sender.Graph.Caches[typeof(SOLine)], (SOLine)parent);
				MemoAvailabilityCheck<SOLineSplit>(sender.Graph.Caches[typeof(SOLine)], (SOLine)parent, Row);
			}
		}

		public override void DefaultLotSerialNbr(PXCache sender, SOLineSplit row)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

			if (item != null)
			{
				if (IsAllocationEntryEnabled && ((INLotSerClass)item).LotSerAssign == INLotSerAssign.WhenUsed)
					return;
				else
					base.DefaultLotSerialNbr(sender, row);
			}
		}
		#endregion

		protected override PXSelectBase<INLotSerialStatus> GetSerialStatusCmdBase(PXCache sender, SOLine Row, PXResult<InventoryItem, INLotSerClass> item)
		{
            if (!IsLocationEnabled && IsLotSerialRequired)
			{
				return new PXSelectJoin<INLotSerialStatus,
				InnerJoin<INLocation, 
					On<INLotSerialStatus.FK.Location>, 
                InnerJoin<INSiteLotSerial, On<INSiteLotSerial.inventoryID, Equal<INLotSerialStatus.inventoryID>,
						And<INSiteLotSerial.siteID, Equal<INLotSerialStatus.siteID>,
						And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>>>,
                Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
                And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>, 
                And<INLotSerialStatus.qtyOnHand, Greater<decimal0>, 
                And<INSiteLotSerial.qtyHardAvail, Greater<decimal0>>>>>>(sender.Graph);
			}
			else
			{
				return base.GetSerialStatusCmdBase(sender, Row, item);
			}
		}

		protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatus> cmd, SOLine Row, INLotSerClass lotSerClass)
		{
			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				switch (Row.TranType)
				{
					case INTranType.Transfer:
						cmd.WhereAnd<Where<INLocation.transfersValid, Equal<boolTrue>>>();
						break;
					default:
						cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
						break;
				}
			}

			if (lotSerClass.IsManualAssignRequired == true)
			{
				if (string.IsNullOrEmpty(Row.LotSerialNbr))
					cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
				else
					cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}
		}

		public virtual bool IsLotSerialItem(PXCache sender, ILSMaster line)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, line.InventoryID);

			if (item == null)
				return false;

			return INLotSerialNbrAttribute.IsTrack(item, line.TranType, line.InvtMult);
		}
	}

	#region SOLotSerialNbrAttribute

	public class SOLotSerialNbrAttribute : INLotSerialNbrAttribute
	{
		private SOLotSerialNbrAttribute() { }

		public SOLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType)
		{
			var itemType = BqlCommand.GetItemType(InventoryType);
			if (!typeof(ILSMaster).IsAssignableFrom(itemType))
			{
				throw new PXArgumentException(IN.Messages.TypeMustImplementInterface, itemType.GetLongName(), typeof(ILSMaster).GetLongName());
			}

			_InventoryType = InventoryType;
			_SubItemType = SubItemType;
			_LocationType = LocationType;

			Type SearchType;
			PXSelectorAttribute attr;

			SearchType = BqlTemplate.OfCommand<Search2<INLotSerialStatus.lotSerialNbr,
				InnerJoin<INSiteLotSerial, On<INLotSerialStatus.inventoryID, Equal<INSiteLotSerial.inventoryID>, And<INLotSerialStatus.siteID, Equal<INSiteLotSerial.siteID>,
				 And<INLotSerialStatus.lotSerialNbr, Equal<INSiteLotSerial.lotSerialNbr>>>>>,
			Where<INLotSerialStatus.inventoryID, Equal<Optional<BqlPlaceholder.A>>,
				And<INLotSerialStatus.subItemID, Equal<Optional<BqlPlaceholder.B>>,
				And2<Where<INLotSerialStatus.locationID, Equal<Optional<BqlPlaceholder.C>>,
					Or<Optional<BqlPlaceholder.C>, IsNull>>, 
				And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>>>
				.Replace<BqlPlaceholder.A>(InventoryType)
				.Replace<BqlPlaceholder.B>(SubItemType)
				.Replace<BqlPlaceholder.C>(LocationType).ToType();


				attr = new PXSelectorAttribute(SearchType,
																	 typeof(INLotSerialStatus.lotSerialNbr),
																	 typeof(INLotSerialStatus.siteID),
																	 typeof(INLotSerialStatus.qtyOnHand),
																	 typeof(INSiteLotSerial.qtyAvail),
																	 typeof(INLotSerialStatus.expireDate));

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public SOLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType)
			: this(InventoryType, SubItemType, LocationType)
		{
			_Attributes[_DefAttrIndex] = new PXDefaultAttribute(ParentLotSerialNbrType) { PersistingCheck = PXPersistingCheck.NullOrBlank };
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) || typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
			{
				base.GetSubscriber<ISubscriber>(subscribers);

				subscribers.Remove(this as ISubscriber);
				subscribers.Add(this as ISubscriber);
				subscribers.Reverse();
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public class SOAllocationLotSerialNbrAttribute : SOLotSerialNbrAttribute
		{
			public SOAllocationLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type SiteType, Type LocationType)
			{
				var itemType = BqlCommand.GetItemType(InventoryType);
				if (!typeof(ILSMaster).IsAssignableFrom(itemType))
				{
					throw new PXArgumentException(nameof(itemType), IN.Messages.TypeMustImplementInterface, itemType.GetLongName(), typeof(ILSMaster).GetLongName());
				}

				_InventoryType = InventoryType;
				_SubItemType = SubItemType;
				_LocationType = LocationType;

				Type SearchType;
				PXSelectorAttribute attr;

				SearchType = BqlTemplate.OfCommand<Search2<INLotSerialStatus.lotSerialNbr,
					InnerJoin<INSiteLotSerial, On<INLotSerialStatus.inventoryID, Equal<INSiteLotSerial.inventoryID>, And<INLotSerialStatus.siteID, Equal<INSiteLotSerial.siteID>,
					 And<INLotSerialStatus.lotSerialNbr, Equal<INSiteLotSerial.lotSerialNbr>>>>>,
				Where<INLotSerialStatus.inventoryID, Equal<Optional<BqlPlaceholder.A>>,
					And<INLotSerialStatus.subItemID, Equal<Optional<BqlPlaceholder.B>>,
					And2<Where<INLotSerialStatus.siteID, Equal<Optional<BqlPlaceholder.C>>,
						Or<Optional<BqlPlaceholder.C>, IsNull>>,
					And2 <Where<INLotSerialStatus.locationID, Equal<Optional<BqlPlaceholder.D>>,
						Or<Optional<BqlPlaceholder.D>, IsNull>>, 
					And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>>>>
					.Replace<BqlPlaceholder.A>(InventoryType)
					.Replace<BqlPlaceholder.B>(SubItemType)
					.Replace<BqlPlaceholder.C>(SiteType)
					.Replace<BqlPlaceholder.D>(LocationType).ToType();


				attr = new PXSelectorAttribute(SearchType,
																	 typeof(INLotSerialStatus.lotSerialNbr),
																	 typeof(INLotSerialStatus.siteID),
																	 typeof(INLotSerialStatus.qtyOnHand),
																	 typeof(INSiteLotSerial.qtyAvail),
																	 typeof(INLotSerialStatus.expireDate));

				_Attributes.Add(attr);
				_SelAttrIndex = _Attributes.Count - 1;
			}

			public SOAllocationLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type SiteType, Type LocationType, Type ParentLotSerialNbrType)
				: this(InventoryType, SubItemType, SiteType, LocationType)
			{
				_Attributes[_DefAttrIndex] = new PXDefaultAttribute(ParentLotSerialNbrType) { PersistingCheck = PXPersistingCheck.NullOrBlank };
			}
		}

	}

	#endregion

	#region SOShipLotSerialNbrAttribute

	public class SOShipLotSerialNbrAttribute : INLotSerialNbrAttribute
	{
        public SOShipLotSerialNbrAttribute(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType)
		{
			var itemType = BqlCommand.GetItemType(InventoryType);
			if (!typeof(ILSMaster).IsAssignableFrom(itemType))
			{
				throw new PXArgumentException(nameof(itemType), IN.Messages.TypeMustImplementInterface, itemType.GetLongName(), typeof(ILSMaster).GetLongName());
			}

			_InventoryType = InventoryType;
			_SubItemType = SubItemType;
			_LocationType = LocationType;

			Type SearchType = BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(INLotSerialStatus.lotSerialNbr),
				typeof(InnerJoin<INSiteLotSerial, On<INLotSerialStatus.inventoryID, Equal<INSiteLotSerial.inventoryID>, And<INLotSerialStatus.siteID, Equal<INSiteLotSerial.siteID>,
				   And<INLotSerialStatus.lotSerialNbr, Equal<INSiteLotSerial.lotSerialNbr>>>>>),
				typeof(Where<,,>),
				typeof(INLotSerialStatus.inventoryID),
				typeof(Equal<>),
				typeof(Optional<>),
				InventoryType,
				typeof(And<,,>),
				typeof(INLotSerialStatus.siteID),
				typeof(Equal<>),
				typeof(Optional<>),
				SiteID,
				typeof(And<,,>),
				typeof(INLotSerialStatus.subItemID),
				typeof(Equal<>),
				typeof(Optional<>),
				SubItemType,
				typeof(And2<,>),
				typeof(Where<,,>),
				typeof(Optional<>),
				LocationType,
				typeof(IsNotNull),
				typeof(And<,,>),
				typeof(INLotSerialStatus.locationID),
				typeof(Equal<>),
				typeof(Optional<>),
				LocationType,
				typeof(Or<,>),
				typeof(Optional<>),
				LocationType,
				typeof(IsNull),
				typeof(And<,>),
				typeof(INLotSerialStatus.qtyOnHand),
				typeof(Greater<>),
				typeof(decimal0)
				);

			{
				PXSelectorAttribute attr = new PXSelectorAttribute(SearchType,
																	 typeof(INLotSerialStatus.lotSerialNbr),
																	 typeof(INLotSerialStatus.siteID),
																	 typeof(INLotSerialStatus.locationID),
																	 typeof(INLotSerialStatus.qtyOnHand),
																	 typeof(INSiteLotSerial.qtyAvail),
																	 typeof(INLotSerialStatus.expireDate));
				_Attributes.Add(attr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}

        public SOShipLotSerialNbrAttribute(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType)
            : this(SiteID, InventoryType, SubItemType, LocationType)
		{
			_Attributes[_DefAttrIndex] = new PXDefaultAttribute(ParentLotSerialNbrType) { PersistingCheck = PXPersistingCheck.Null };
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) || typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
			{
				base.GetSubscriber<ISubscriber>(subscribers);

				subscribers.Remove(this as ISubscriber);
				subscribers.Add(this as ISubscriber);
				subscribers.Reverse();
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler<SOShipLineSplit.lotSerialNbr>(LotSerialNumberUpdated);
		}
		protected virtual void LotSerialNumberUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOShipLineSplit row = e.Row as SOShipLineSplit;
			if (row == null) return;
			if (string.IsNullOrEmpty(row.LotSerialNbr)) return;
			SOShipLine parentLine = PXParentAttribute.SelectParent(sender, e.Row, typeof(SOShipLine)) as SOShipLine;
			if (parentLine == null || parentLine.IsUnassigned != true)
				return;

			if (row.LocationID != null)
				return;

			PXResultset<INLotSerialStatus> res = PXSelect<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
				 And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
				 And<INLotSerialStatus.siteID, Equal<Required<INLotSerialStatus.siteID>>,
				 And<INLotSerialStatus.lotSerialNbr, Equal<Required<INLotSerialStatus.lotSerialNbr>>,
				 And<INLotSerialStatus.qtyHardAvail, Greater<Zero>>>>>>>.SelectWindowed(sender.Graph, 0, 1, row.InventoryID, row.SubItemID, row.SiteID, row.LotSerialNbr);
			if (res.Count == 1)
			{
				sender.SetValueExt<SOShipLineSplit.locationID>(row, ((INLotSerialStatus)res).LocationID);
			}
		}
	}

	#endregion

	public class LSSOShipLine : LSSelectSOBase<SOShipLine, SOShipLineSplit,
		Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>
	{
		#region State

		public bool IsLocationEnabled
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);
				if (ordertype == null || (ordertype.RequireShipping == false && ordertype.RequireLocation == true && ordertype.INDocType != INTranType.NoUpdate)) return true;
				else return false;
			}
		}

		public bool IsLotSerialRequired
		{
			get
			{
				SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);
				return (ordertype == null || ordertype.RequireLotSerial == true);
			}
		}
		#endregion
		#region Ctor
		public LSSOShipLine(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof(SOShipLine.shippedQty);
			graph.FieldDefaulting.AddHandler<SOShipLineSplit.subItemID>(SOShipLineSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOShipLineSplit.locationID>(SOShipLineSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOShipLineSplit.invtMult>(SOShipLineSplit_InvtMult_FieldDefaulting);
			graph.RowPersisting.AddHandler<SOShipLine>(SOShipLine_RowPersisting);
			graph.RowPersisting.AddHandler<SOShipLineSplit>(SOShipLineSplit_RowPersisting);

			graph.RowUpdated.AddHandler<SOShipment>(SOShipment_RowUpdated);
			graph.RowUpdating.AddHandler<SOShipLineSplit>(Detail_RowUpdating);
            graph.RowSelected.AddHandler<SOShipLine>(SOShipLine_RowSelected);
		}

		#endregion
		public override IEnumerable BinLotSerial(PXAdapter adapter)
		{
			View.AskExt(true);
			return adapter.Get();
		}

		#region Implementation

        private void SOShipLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (SOShipLine)e.Row;
            if (row == null)
                return;
            bool unassignederror = false;
            if (row.InventoryID != null)
            {
                if (Math.Abs(row.BaseQty ?? 0m) >= 0.0000005m && Math.Abs(row.UnassignedQty ?? 0m) >= 0.0000005m)
                {
                    unassignederror = true;
                    sender.RaiseExceptionHandling<SOShipLine.unassignedQty>(row, null,
                        new PXSetPropertyException(Messages.LineBinLotSerialNotAssigned, PXErrorLevel.Warning, sender.GetValueExt<SOShipLine.inventoryID>(row)));
                }
            }

            if (unassignederror == false)
                sender.RaiseExceptionHandling<SOShipLine.unassignedQty>(row, null, null);
        }

		protected override bool IsLotSerOptionsEnabled(PXCache sender, LotSerOptions opt)
		{
			return base.IsLotSerOptionsEnabled(sender, opt) &&
				((SOShipment)sender.Graph.Caches<SOShipment>().Current)?.Confirmed != true;
		}

		public override SOShipLine CloneMaster(SOShipLine item)
		{
			SOShipLine copy = base.CloneMaster(item);
			copy.OrigOrderType = null;
			copy.OrigOrderNbr = null;
			copy.OrigLineNbr = null;
			copy.OrigSplitLineNbr = null;
			copy.IsClone = true;

			return copy;
		}

		protected virtual void SOShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<SOShipment.confirmed>(e.Row, e.OldRow) && (bool?)sender.GetValue<SOShipment.confirmed>(e.Row) == true)
			{
				PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];

				foreach (SOShipLine item in PXParentAttribute.SelectSiblings(cache, null, typeof(SOShipment)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
					{
						cache.RaiseExceptionHandling<SOShipLine.unassignedQty>(item, item.UnassignedQty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

                            //this code is placed to obligate platform call command preparing for current row and as result get an error. Normally it's not necessary, but in this case the code could be called from Unnatended mode
						cache.MarkUpdated(item);
					}
				}
			}
		}

		protected virtual void OrderAvailabilityCheck(PXCache sender, SOShipLine Row)
		{
			if (UnattendedMode)
				return;

			if (Row.OrigOrderNbr != null)
			{
				SOLineSplit2 split = PXSelect<SOLineSplit2,
					Where<SOLineSplit2.orderType, Equal<Current<SOShipLine.origOrderType>>,
						And<SOLineSplit2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>,
						And<SOLineSplit2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>,
						And<SOLineSplit2.splitLineNbr, Equal<Current<SOShipLine.origSplitLineNbr>>>>>>>
					.SelectSingleBound(_Graph, new object[] { Row });

				SOLine2 soLine = PXSelect<SOLine2,
					Where<SOLine2.orderType, Equal<Current<SOShipLine.origOrderType>>,
						And<SOLine2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>,
						And<SOLine2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>>>>>
					.SelectSingleBound(_Graph, new object[] { Row });

				if (split != null && soLine != null)
				{
					if (split.IsAllocated == true && split.Qty * soLine.CompleteQtyMax / 100 < split.ShippedQty)
						throw new PXSetPropertyException(Messages.OrderSplitCheck_QtyNegative,
							sender.GetValueExt<SOShipLine.inventoryID>(Row),
							sender.GetValueExt<SOShipLine.subItemID>(Row),
							sender.GetValueExt<SOShipLine.origOrderType>(Row),
							sender.GetValueExt<SOShipLine.origOrderNbr>(Row));

					if (PXDBPriceCostAttribute.Round((decimal)(soLine.OrderQty * soLine.CompleteQtyMax / 100m - soLine.ShippedQty)) < 0m &&
						PXDBPriceCostAttribute.Round((decimal)(split.Qty * soLine.CompleteQtyMax / 100m - split.ShippedQty)) < 0m)
					{
						throw new PXSetPropertyException(Messages.OrderCheck_QtyNegative, sender.GetValueExt<SOShipLine.inventoryID>(Row), sender.GetValueExt<SOShipLine.subItemID>(Row), sender.GetValueExt<SOShipLine.origOrderType>(Row), sender.GetValueExt<SOShipLine.origOrderNbr>(Row));
					}
				}
			}
		}

		public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
		{
			base.AvailabilityCheck(sender, Row);

			if (Row is SOShipLine)
			{
				try
				{
					OrderAvailabilityCheck(sender, (SOShipLine)Row);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOShipLine.shippedQty>(Row, ((SOShipLine)Row).ShippedQty, ex);
				}
			}
			else
			{
				object parent = PXParentAttribute.SelectParent(sender, Row, typeof(SOShipLine));
				try
				{
					OrderAvailabilityCheck(sender.Graph.Caches[typeof(SOShipLine)], (SOShipLine)parent);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOShipLineSplit.qty>(Row, ((SOShipLineSplit)Row).Qty, ex);
				}
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			using (ResolveNotDecimalUnitErrorRedirectorScope<SOShipLineSplit.qty>(e.Row))
				base.Master_RowUpdated(sender, e);
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOShipLine row = (SOShipLine)e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(SOShipment)];
				object doc = PXParentAttribute.SelectParent(sender, row, typeof(SOShipment)) ?? cache.Current;

				bool? Confirmed = (bool?)cache.GetValue<SOShipment.confirmed>(doc);
				if (Confirmed == true)
				{
					if (Math.Abs((decimal)row.BaseQty) >= 0.0000005m && row.UnassignedQty >= 0.0000005m || row.UnassignedQty <= -0.0000005m)
					{
						if (sender.RaiseExceptionHandling<SOShipLine.unassignedQty>(row, row.UnassignedQty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
						{
							throw new PXRowPersistingException(typeof(SOShipLine.unassignedQty).Name, row.UnassignedQty, Messages.BinLotSerialNotAssigned);
						}
					}
				}

			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				try
				{
					OrderAvailabilityCheck(sender, row);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOShipLine.shippedQty>(row, row.ShippedQty, ex);
				}
			}
			base.Master_RowPersisting(sender, e);
		}

		public int? lastComponentID = null;
		protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<SOShipLine> e)
		{
			if (lastComponentID == e.Row.InventoryID)
			{
				return;
			}
			base._Master_RowUpdated(sender, e);
		}

		protected virtual void UpdateKit(PXCache sender, SOShipLineSplit row)
		{
			SOShipLine newline = SelectMaster(sender, row);

			if (newline == null)
			{
				return;
			}

			decimal KitQty = (decimal)newline.BaseQty;
			if (newline.InventoryID != row.InventoryID && row.IsStockItem == true)
			{
				foreach (PXResult<INKitSpecStkDet, InventoryItem> res in PXSelectJoin<INKitSpecStkDet, 
					InnerJoin<InventoryItem, 
						On<INKitSpecStkDet.FK.CompInventoryItem>>, 
					Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Search<INKitSpecStkDet.compInventoryID>(sender.Graph, row.InventoryID, newline.InventoryID))
				{
					INKitSpecStkDet kititem = res;
					decimal ComponentQty = INUnitAttribute.ConvertToBase<SOShipLineSplit.inventoryID>(sender, row, kititem.UOM, KitQty * (decimal)kititem.DfltCompQty, INPrecision.NOROUND);

					SOShipLine copy = CloneMaster(newline);
					copy.InventoryID = row.InventoryID;

					Counters counters;
					if (!DetailCounters.TryGetValue(copy, out counters))
					{
						DetailCounters[copy] = counters = new Counters();
						foreach (SOShipLineSplit detail in SelectDetail(sender, copy))
						{
							UpdateCounters(sender, counters, detail);
						}
					}

					if (ComponentQty != 0m && (decimal)counters.BaseQty != ComponentQty)
					{
						KitQty = PXDBQuantityAttribute.Round(KitQty * (decimal)counters.BaseQty / ComponentQty);
						lastComponentID = kititem.CompInventoryID;
					}
				}
			}
			else if (newline.InventoryID != row.InventoryID)
			{
				foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in PXSelectJoin<INKitSpecNonStkDet, 
					InnerJoin<InventoryItem, 
						On<INKitSpecNonStkDet.FK.CompInventoryItem>>,
					Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>>>.Search<INKitSpecNonStkDet.compInventoryID>(sender.Graph, row.InventoryID, newline.InventoryID))
				{
					INKitSpecNonStkDet kititem = res;

					decimal ComponentQty = INUnitAttribute.ConvertToBase<SOShipLineSplit.inventoryID>(sender, row, kititem.UOM, (decimal)kititem.DfltCompQty, INPrecision.NOROUND);

					if (ComponentQty != 0m && row.BaseQty != ComponentQty)
					{
						KitQty = PXDBQuantityAttribute.Round((decimal)row.BaseQty / ComponentQty);
						lastComponentID = kititem.CompInventoryID;
					}
				}
			}

			if (lastComponentID != null)
			{
				SOShipLine copy = PXCache<SOShipLine>.CreateCopy(newline);
				copy.ShippedQty = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(MasterCache, newline, newline.UOM, KitQty, INPrecision.QUANTITY);
				try
				{
					MasterCache.Update(copy);
				}
				finally
				{
					lastComponentID = null;
				}

				if (sender.Graph is SOShipmentEntry)
				{
					((SOShipmentEntry)sender.Graph).splits.View.RequestRefresh();
				}
			}
		}

		protected void Detail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			SOShipLineSplit row = (SOShipLineSplit)e.Row;

            if (!_InternallCall && !UnattendedMode)
			{
				UpdateKit(sender, row);
			}
		}

		protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Detail_RowUpdated(sender, e);

			SOShipLineSplit row = (SOShipLineSplit)e.Row;

			if (!sender.ObjectsEqual<SOShipLineSplit.lotSerialNbr>(e.Row, e.OldRow) && ((SOShipLineSplit)e.Row).LotSerialNbr != null && ((SOShipLineSplit)e.Row).Operation == SOOperation.Issue)
			{
				LotSerialNbr_Updated(sender, e);
			}

			if (!sender.ObjectsEqual<SOShipLineSplit.locationID>(e.Row, e.OldRow) && ((SOShipLineSplit)e.Row).LotSerialNbr != null && e.ExternalCall)
			{
				Location_Updated(sender, e);
			}

			if (!_InternallCall && !UnattendedMode)
			{
				UpdateKit(sender, row);
			}
		}

		protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			SOShipLineSplit row = (SOShipLineSplit)e.Row;

			PXResult<InventoryItem, INLotSerClass> res = ReadInventoryItem(sender, row.InventoryID);
			InventoryItem item = (InventoryItem)res;
			bool NonStockKit = item.KitItem == true && item.StkItem == false;

			if (NonStockKit)
			{
				row.InventoryID = null;
			}

			base.Detail_RowInserting(sender, e);
		}

		protected override void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.Detail_RowInserted(sender, e);

			SOShipLineSplit row = (SOShipLineSplit)e.Row;
			if (!_InternallCall && !UnattendedMode)
			{
				UpdateKit(sender, row);
			}
		}

		protected override void Detail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			base.Detail_RowDeleted(sender, e);

			SOShipLineSplit row = (SOShipLineSplit)e.Row;
			if (!_InternallCall && !UnattendedMode)
			{
				UpdateKit(sender, row);
			}
		}

		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			IQtyAllocated availability = AvailabilityFetch(sender, (SOShipLine)e.Row, AvailabilityFetchMode.ExcludeCurrent | AvailabilityFetchMode.TryOptimize);

			if (availability != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((SOShipLine)e.Row).InventoryID);

				decimal unitRate = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID, SOShipLine.uOM>(sender, e.Row, 1m, INPrecision.NOROUND);
				availability.QtyOnHand = PXDBQuantityAttribute.Round((decimal)availability.QtyOnHand * unitRate);
				availability.QtyAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyAvail * unitRate);
				availability.QtyNotAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyNotAvail * unitRate);
				availability.QtyHardAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyHardAvail * unitRate);

				e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(IN.Messages.Availability_Info,
						sender.GetValue<SOShipLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));

				AvailabilityCheck(sender, (SOShipLine)e.Row, availability);
			}
			else
			{
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}

		protected int _detailsRequested = 0;

		protected override IQtyAllocated AvailabilityFetch<TNode>(ILSDetail Row, IQtyAllocated allocated, IStatus status, AvailabilityFetchMode fetchMode)
		{
			if (status != null)
			{
				allocated.QtyOnHand += status.QtyOnHand;
				allocated.QtyHardAvail += status.QtyHardAvail;
			}
			allocated.QtyAvail = allocated.QtyHardAvail;

			if (fetchMode.HasFlag(AvailabilityFetchMode.TryOptimize) && _detailsRequested++ == 5)
			{
				foreach (PXResult<SOShipLine, INSiteStatus, INLocationStatus, INLotSerialStatus> res in
						PXSelectReadonly2<SOShipLine,
						InnerJoin<INSiteStatus,
						On<INSiteStatus.inventoryID, Equal<SOShipLine.inventoryID>,
						And<INSiteStatus.subItemID, Equal<SOShipLine.subItemID>,
							And<INSiteStatus.siteID, Equal<SOShipLine.siteID>>>>,
						LeftJoin<INLocationStatus,
						On<INLocationStatus.inventoryID, Equal<SOShipLine.inventoryID>,
						And<INLocationStatus.subItemID, Equal<SOShipLine.subItemID>,
						And<INLocationStatus.siteID, Equal<SOShipLine.siteID>,
						And<INLocationStatus.locationID, Equal<SOShipLine.locationID>>>>>,
					LeftJoin<INLotSerialStatus,
						On<INLotSerialStatus.inventoryID, Equal<SOShipLine.inventoryID>,
						And<INLotSerialStatus.subItemID, Equal<SOShipLine.subItemID>,
						And<INLotSerialStatus.siteID, Equal<SOShipLine.siteID>,
						And<INLotSerialStatus.locationID, Equal<SOShipLine.locationID>,
						And<INLotSerialStatus.lotSerialNbr, Equal<SOShipLine.lotSerialNbr>>>>>>>>>,
						Where<SOShipLine.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>
						.Select(this._Graph))
					{
						SOShipLine line = res;
						INSiteStatus siteStatus = res;
						INLocationStatus locStatus = res;
					INLotSerialStatus lotSerStatus = res;

					INSiteStatus.PK.StoreCached(this._Graph, siteStatus);

						if (locStatus.LocationID != null)
						INLocationStatus.PK.StoreCached(this._Graph, locStatus);

					if (lotSerStatus?.LotSerialNbr != null)
						IN.INLotSerialStatus.PK.StoreCached(this._Graph, lotSerStatus);
					}
				}

			if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
			{
				decimal SignQtyAvail;
				decimal SignQtyHardAvail;
				INItemPlanIDAttribute.GetInclQtyAvail<TNode>(DetailCache, Row, out SignQtyAvail, out SignQtyHardAvail);

				if (SignQtyHardAvail != 0)
				{
					allocated.QtyAvail -= SignQtyHardAvail * (Row.BaseQty ?? 0m);
					allocated.QtyNotAvail += SignQtyHardAvail * (Row.BaseQty ?? 0m);
					allocated.QtyHardAvail -= SignQtyHardAvail * (Row.BaseQty ?? 0m);
				}
				//Exclude Unassigned
				foreach (Unassigned.SOShipLineSplit detail in SelectUnassignedDetails(DetailCache, Row))
				{
					if (SignQtyHardAvail != 0 && (Row.LocationID == null || Row.LocationID == detail.LocationID) &&
						(Row.LotSerialNbr == null || string.IsNullOrEmpty(detail.LotSerialNbr) || string.Equals(Row.LotSerialNbr, detail.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase)))
					{
						allocated.QtyAvail -= SignQtyHardAvail * (detail.BaseQty ?? 0m);
						allocated.QtyHardAvail -= SignQtyHardAvail * (detail.BaseQty ?? 0m);
					}
				}
			}
			return allocated;
		}

		protected virtual bool LotSerialNbr_Updated(PXCache sender, EventArgs e)
		{
			SOShipLineSplit split = (SOShipLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);
			INSiteLotSerial siteLotSerial = PXSelect<INSiteLotSerial, 
				Where<INSiteLotSerial.inventoryID, Equal<Required<INSiteLotSerial.inventoryID>>,
				And<INSiteLotSerial.siteID, Equal<Required<INSiteLotSerial.siteID>>,
				And<INSiteLotSerial.lotSerialNbr, Equal<Required<INSiteLotSerial.lotSerialNbr>>>>>>.Select(sender.Graph, split.InventoryID, split.SiteID, split.LotSerialNbr);
			
			if (INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null && siteLotSerial != null && siteLotSerial.LotSerAssign != INLotSerAssign.WhenUsed)
			{
				decimal qtyHardAvail = siteLotSerial.QtyHardAvail.GetValueOrDefault();
				
				//Exclude unasigned
				foreach (Unassigned.SOShipLineSplit detail in SelectUnassignedDetails(DetailCache, split))
				{
					if ( (split.LocationID == null || split.LocationID == detail.LocationID) &&
						( string.IsNullOrEmpty(detail.LotSerialNbr) || split.LotSerialNbr == null || string.Equals(split.LotSerialNbr, detail.LotSerialNbr, StringComparison.InvariantCultureIgnoreCase)))
					{
						qtyHardAvail += split.BaseQty.GetValueOrDefault();
					}
				
				}
				
				if (qtyHardAvail < split.BaseQty)
				{
					split.LotSerialNbr = null;
                    sender.RaiseExceptionHandling<SOShipLineSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
					return false;
				}
			}
			return true;
		}

		protected virtual void Location_Updated(PXCache sender, EventArgs e)
		{
			SOShipLineSplit split = (SOShipLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);

			SOShipLine line = SelectMaster(sender, split);
            if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null)
			{
				INLotSerialStatus res = PXSelect<INLotSerialStatus, Where<INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
				 And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
				 And<INLotSerialStatus.siteID, Equal<Required<INLotSerialStatus.siteID>>,
				 And<INLotSerialStatus.lotSerialNbr, Equal<Required<INLotSerialStatus.lotSerialNbr>>,
				 And<INLotSerialStatus.locationID, Equal<Required<INLotSerialStatus.locationID>>>>>>>>.Select(sender.Graph, split.InventoryID, split.SubItemID, split.SiteID, split.LotSerialNbr, split.LocationID);
				if (res == null)
				{
				split.LotSerialNbr = null;
			}
		}
		}

		public override SOShipLineSplit Convert(SOShipLine item)
		{
			using (InvtMultScope<SOShipLine> ms = new InvtMultScope<SOShipLine>(item))
			{
				SOShipLineSplit ret = item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				ret.LotSerialNbr = string.Empty;
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}
		public virtual void SOShipLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && AdvancedAvailCheck(sender, e.Row) &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((SOShipLine)e.Row).BaseQty != 0m)
				{
					AvailabilityCheck(sender, (SOShipLine)e.Row);
				}
			}
		}

		public virtual void SOShipLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				bool RequireLocationAndSubItem = ((SOShipLineSplit)e.Row).IsStockItem == true && ((SOShipLineSplit)e.Row).BaseQty != 0m;

				PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				if (AdvancedAvailCheck(sender, e.Row) && ((SOShipLineSplit)e.Row).BaseQty != 0m)
				{
					AvailabilityCheck(sender, (SOShipLineSplit)e.Row);
				}
			}
		}

		public virtual void SOShipLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];
			if (cache.Current != null && (e.Row == null || ((SOShipLine)cache.Current).LineNbr == ((SOShipLineSplit)e.Row).LineNbr && ((SOShipLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOShipLine)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void SOShipLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];
			if (cache.Current != null && ((SOShipLine)cache.Current).IsUnassigned != true &&(e.Row == null || ((SOShipLine)cache.Current).LineNbr == ((SOShipLineSplit)e.Row).LineNbr && ((SOShipLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOShipLine)cache.Current).LocationID;
				e.Cancel = (_InternallCall == true || e.NewValue != null);
			}
		}

		public virtual void SOShipLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];
			if (cache.Current != null && (e.Row == null || ((SOShipLine)cache.Current).LineNbr == ((SOShipLineSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<SOShipLine> ms = new InvtMultScope<SOShipLine>((SOShipLine)cache.Current))
				{
					e.NewValue = ((SOShipLine)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
		{
			PXErrorLevel level = AdvancedAvailCheck(sender, row) ? PXErrorLevel.Error : PXErrorLevel.Warning;
			if (row is SOShipLine)
			{
				sender.RaiseExceptionHandling<SOShipLine.shippedQty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, level, sender.GetStateExt<SOShipLine.inventoryID>(row), sender.GetStateExt<SOShipLine.subItemID>(row), sender.GetStateExt<SOShipLine.siteID>(row), sender.GetStateExt<SOShipLine.locationID>(row), sender.GetValue<SOShipLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<SOShipLineSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, level, sender.GetStateExt<SOShipLineSplit.inventoryID>(row), sender.GetStateExt<SOShipLineSplit.subItemID>(row), sender.GetStateExt<SOShipLineSplit.siteID>(row), sender.GetStateExt<SOShipLineSplit.locationID>(row), sender.GetValue<SOShipLineSplit.lotSerialNbr>(row)));
			}
		}

		protected bool AdvancedAvailCheck(PXCache sender, object row)
		{
			SOSetup setup = (SOSetup)sender.Graph.Caches[typeof(SOSetup)].Current;
			if (setup != null && setup.AdvancedAvailCheck == true)
			{
				if (_advancedAvailCheck != null) return _advancedAvailCheck == true;
			}
			return false;
		}

		public void OverrideAdvancedAvailCheck(bool checkRequired)
		{
			_advancedAvailCheck = checkRequired;
		}
		private bool? _advancedAvailCheck;
		#endregion

		protected override PXSelectBase<INLotSerialStatus> GetSerialStatusCmdBase(PXCache sender, SOShipLine Row, PXResult<InventoryItem, INLotSerClass> item)
		{
			if (!IsLocationEnabled && IsLotSerialRequired)
			{
				return new PXSelectJoin<INLotSerialStatus,
				InnerJoin<INLocation, 
					On<INLotSerialStatus.FK.Location>,
				InnerJoin<INSiteLotSerial, On<INSiteLotSerial.inventoryID, Equal<INLotSerialStatus.inventoryID>,
						And<INSiteLotSerial.siteID, Equal<INLotSerialStatus.siteID>,
						And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>>>,
				Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
				And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>,
				And<INLotSerialStatus.qtyOnHand, Greater<decimal0>,
				And<INSiteLotSerial.qtyHardAvail, Greater<decimal0>>>>>>(sender.Graph);
			}
			else
			{
				return base.GetSerialStatusCmdBase(sender, Row, item);
			}
		}

		protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatus> cmd, SOShipLine Row, INLotSerClass lotSerClass)
		{
			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				switch (Row.TranType)
				{
					case INTranType.Transfer:
						cmd.WhereAnd<Where<INLocation.transfersValid, Equal<boolTrue>>>();
						break;
					default:
						cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
						break;
				}
			}

			if (lotSerClass.IsManualAssignRequired == true)
			{
				if (string.IsNullOrEmpty(Row.LotSerialNbr))
			{
					cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
			}
				else
					cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}
		}

		public override void AvailabilityCheck(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			base.AvailabilityCheck(sender, Row, availability);
			if (Row.InvtMult == (short)-1 && Row.BaseQty > 0m && availability != null)
			{
				SOShipment doc = (SOShipment)sender.Graph.Caches[typeof(SOShipment)].Current;
				if (availability.QtyOnHand - Row.Qty < 0m && doc != null && doc.Confirmed == false)
				{
					if (availability is LotSerialStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(IN.Messages.StatusCheck_QtyLotSerialOnHandNegative));
					}
					else if (availability is LocationStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(IN.Messages.StatusCheck_QtyLocationOnHandNegative));
					}
					else if (availability is SiteStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(IN.Messages.StatusCheck_QtyOnHandNegative));
					}
				}
			}
		}
		private void RaiseQtyRowExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			PXErrorLevel level = AdvancedAvailCheck(sender, row) ? PXErrorLevel.Error : PXErrorLevel.Warning;
			if (row is SOShipLine)
			{
				sender.RaiseExceptionHandling<SOShipLine.shippedQty>(row, newValue,
					e == null ? e : new PXSetPropertyException(e.Message, level, sender.GetStateExt<SOShipLine.inventoryID>(row), sender.GetStateExt<SOShipLine.subItemID>(row), sender.GetStateExt<SOShipLine.siteID>(row), sender.GetStateExt<SOShipLine.locationID>(row), sender.GetValue<SOShipLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<SOShipLineSplit.qty>(row, newValue,
					e == null ? e : new PXSetPropertyException(e.Message, level, sender.GetStateExt<SOShipLineSplit.inventoryID>(row), sender.GetStateExt<SOShipLineSplit.subItemID>(row), sender.GetStateExt<SOShipLineSplit.siteID>(row), sender.GetStateExt<INTranSplit.locationID>(row), sender.GetValue<SOShipLineSplit.lotSerialNbr>(row)));
			}
		}
		
		/// <summary>
		/// Inserts SOShipLine into cache without adding the splits.
		/// The Splits have to be added manually.
		/// </summary>
		/// <param name="line">Master record.</param>
		public virtual SOShipLine InsertMasterWithoutSplits(SOShipLine line)
		{
			_InternallCall = true;
			try
			{
				var row = (SOShipLine)MasterCache.Insert(line);
				DetailCounters.Remove(row);
				return row;
			}
			finally
			{
				_InternallCall = false;
			}
		}

		protected virtual List<Unassigned.SOShipLineSplit> SelectUnassignedDetails(PXCache sender, ILSDetail row)
		{
			Unassigned.SOShipLineSplit unassignedRow = new Unassigned.SOShipLineSplit();
			unassignedRow.ShipmentNbr = ((SOShipLineSplit) row).ShipmentNbr;
			unassignedRow.LineNbr = ((SOShipLineSplit)row).LineNbr;
			unassignedRow.SplitLineNbr = ((SOShipLineSplit)row).SplitLineNbr;
			object[] ret = PXParentAttribute.SelectSiblings(sender.Graph.Caches[typeof(Unassigned.SOShipLineSplit)], unassignedRow,
				(_detailsRequested > 5) ? typeof(SOShipment) : typeof(SOShipLine));
			List<Unassigned.SOShipLineSplit> list = new List<Unassigned.SOShipLineSplit>(ret.Cast<Unassigned.SOShipLineSplit>());
			return list.FindAll(a => SameInventoryItem(a, row) && a.LineNbr == ((SOShipLineSplit)row).LineNbr);
		}
	}

	public abstract class SOContactAttribute : ContactAttribute
	{
		#region State
		BqlCommand _DuplicateSelect = BqlCommand.CreateInstance(typeof(Select<SOContact, Where<SOContact.customerID, Equal<Required<SOContact.customerID>>, And<SOContact.customerContactID, Equal<Required<SOContact.customerContactID>>, And<SOContact.revisionID, Equal<Required<SOContact.revisionID>>, And<SOContact.isDefaultContact, Equal<boolTrue>>>>>>));
		#endregion
		#region Ctor
		public SOContactAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
			: base(AddressIDType, IsDefaultAddressType, SelectType)
		{
		}
		#endregion
		#region Implementation
		public override void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((SOContact)e.Row).IsDefaultContact == true)
			{
				PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
				view.Clear();

				SOContact prev_address = (SOContact)view.SelectSingle(((SOContact)e.Row).CustomerID, ((SOContact)e.Row).CustomerContactID, ((SOContact)e.Row).RevisionID);
				if (prev_address != null)
				{
					_KeyToAbort = sender.GetValue(e.Row, _RecordID);
					object newkey = sender.Graph.Caches[typeof(SOContact)].GetValue(prev_address, _RecordID);

					PXCache cache = sender.Graph.Caches[_ItemType];

					foreach (object data in cache.Updated)
					{
						object datakey = cache.GetValue(data, _FieldOrdinal);
						if (Equals(_KeyToAbort, datakey))
						{
							cache.SetValue(data, _FieldOrdinal, newkey);
						}
					}

					_KeyToAbort = null;
					e.Cancel = true;
					return;
				}
			}
			base.Record_RowPersisting(sender, e);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null)
			{
				PXCache cache = sender.Graph.Caches[_RecordType];
				if (Convert.ToInt32(key) < 0)
				{
					foreach (object data in cache.Inserted)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							if (((SOContact)data).IsDefaultContact == true)
							{
								PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
								view.Clear();

								SOContact prev_address = (SOContact)view.SelectSingle(((SOContact)data).CustomerID, ((SOContact)data).CustomerContactID, ((SOContact)data).RevisionID);

								if (prev_address != null)
								{
									_KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
									object id = sender.Graph.Caches[typeof(SOContact)].GetValue(prev_address, _RecordID);
									sender.SetValue(e.Row, _FieldOrdinal, id);
								}
							}
							break;
						}
					}
				}
			}
			base.RowPersisting(sender, e);
		}
		#endregion
	}

	public class SOBillingContactAttribute : SOContactAttribute
	{
		public SOBillingContactAttribute(Type SelectType)
			: base(typeof(SOBillingContact.contactID), typeof(SOBillingContact.isDefaultContact), SelectType)
		{ 
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOBillingContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<SOBillingContact, SOBillingContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<SOBillingContact, SOBillingContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<SOBillingContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<SOBillingContact.overrideContact>(sender, e.Row, true);
			}
		}
	}

	/// <summary>
	/// Shipping contact for the Sales Order document.
	/// </summary>
	public class SOShippingContactAttribute : SOContactAttribute
	{
		public SOShippingContactAttribute(Type SelectType)
			: base(typeof(SOShippingContact.contactID), typeof(SOShippingContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShippingContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<SOShippingContact, SOShippingContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<SOShippingContact, SOShippingContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<SOShippingContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<SOShippingContact.overrideContact>(sender, e.Row, true);
			}
		}

        public override void DefaultContact<TContact, TContactID>(PXCache sender, object DocumentRow, object AddressRow)
		{
            int? destinationSiteID = (int?)sender.GetValue<SOOrder.destinationSiteID>(DocumentRow);
            PXView view;
            bool issitebranch = false;
            if (destinationSiteID != null)
            {
                issitebranch = true;
                BqlCommand altSelect = BqlCommand.CreateInstance(typeof(
                    Select2<Contact,
                        InnerJoin<INSite,
                            On2<INSite.FK.Contact,
                            And<INSite.siteID, Equal<Current<SOOrder.destinationSiteID>>>>,
                        LeftJoin<SOShippingContact,
                            On<SOShippingContact.customerID, Equal<Contact.bAccountID>,
                            And<SOShippingContact.customerContactID, Equal<Contact.contactID>,
                            And<SOShippingContact.revisionID, Equal<Contact.revisionID>,
                            And<SOShippingContact.isDefaultContact, Equal<True>>>>>>>,
                        Where<True, Equal<True>>>));
                view = sender.Graph.TypedViews.GetView(altSelect, false);
            }
            else
			{
                view = sender.Graph.TypedViews.GetView(_Select, false);
            }
            int startRow = -1;
            int totalRows = 0;
            bool contactFound = false;

            foreach (PXResult res in view.Select(new object[] { DocumentRow }, null, null, null, null, null, ref startRow, 1, ref totalRows))
			{
                contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, AddressRow, res);
                break;
			}

            if (!contactFound && !_Required)
                this.ClearRecord(sender, DocumentRow);

            if(!contactFound && _Required && issitebranch)
                throw new SharedRecordMissingException();
		}
	}

	public class SOShipmentContactAttribute : ContactAttribute
	{
		public SOShipmentContactAttribute(Type SelectType)
			: base(typeof(SOShipmentContact.contactID), typeof(SOShipmentContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShipmentContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
            SOShipment shipment = DocumentRow as SOShipment;
            if (shipment != null && shipment.ShipmentType == SOShipmentType.Transfer)
            {
	            PXResult contactRecord = null;
				using(new PXReadBranchRestrictedScope())
	            {
	            contactRecord = PXSelectJoin<Contact,
                    InnerJoin<INSite,
                          On<INSite.FK.Contact>,
                    LeftJoin<SOShipmentContact, On<SOShipmentContact.customerID, Equal<Contact.bAccountID>,
                        And<SOShipmentContact.customerContactID, Equal<Contact.contactID>,
                        And<SOShipmentContact.revisionID, Equal<Contact.revisionID>,
                        And<SOShipmentContact.isDefaultContact, Equal<True>>>>>>>,
                    Where<INSite.siteID, Equal<Current<SOShipment.destinationSiteID>>>>.SelectMultiBound(sender.Graph, new object[] { DocumentRow });
				}
                DefaultContact<SOShipmentContact, SOShipmentContact.contactID>(sender, FieldName, DocumentRow, Row, contactRecord);

            }
            else
            {
                DefaultContact<SOShipmentContact, SOShipmentContact.contactID>(sender, DocumentRow, Row);
            }
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<SOShipmentContact, SOShipmentContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<SOShipmentContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<SOShipmentContact.overrideContact>(sender, e.Row, true);
			}
		}
	}

	public abstract class SOAddressAttribute : AddressAttribute
	{
		#region State
		BqlCommand _DuplicateSelect = BqlCommand.CreateInstance(typeof(Select<SOAddress, Where<SOAddress.customerID, Equal<Required<SOAddress.customerID>>, And<SOAddress.customerAddressID, Equal<Required<SOAddress.customerAddressID>>, And<SOAddress.revisionID, Equal<Required<SOAddress.revisionID>>, And<SOAddress.isDefaultAddress, Equal<boolTrue>>>>>>));
		#endregion
		#region Ctor
		public SOAddressAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
			: base(AddressIDType, IsDefaultAddressType, SelectType)
		{
		}
		#endregion
		#region Implementation
		public override void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((SOAddress)e.Row).IsDefaultAddress == true)
			{
				PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
				view.Clear();

				SOAddress prev_address = (SOAddress)view.SelectSingle(((SOAddress)e.Row).CustomerID, ((SOAddress)e.Row).CustomerAddressID, ((SOAddress)e.Row).RevisionID);
				if (prev_address != null)
				{
					_KeyToAbort = sender.GetValue(e.Row, _RecordID);
					object newkey = sender.Graph.Caches[typeof(SOAddress)].GetValue(prev_address, _RecordID);

					PXCache cache = sender.Graph.Caches[_ItemType];

					foreach (object data in cache.Updated)
					{
						object datakey = cache.GetValue(data, _FieldOrdinal);
						if (Equals(_KeyToAbort, datakey))
						{
							cache.SetValue(data, _FieldOrdinal, newkey);
						}
					}

					_KeyToAbort = null;
					e.Cancel = true;
					return;
				}
			}
			base.Record_RowPersisting(sender, e);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null)
			{
				PXCache cache = sender.Graph.Caches[_RecordType];
				if (Convert.ToInt32(key) < 0)
				{
					foreach (object data in cache.Inserted)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							if (((SOAddress)data).IsDefaultAddress == true)
							{
								PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
								view.Clear();

								SOAddress prev_address = (SOAddress)view.SelectSingle(((SOAddress)data).CustomerID, ((SOAddress)data).CustomerAddressID, ((SOAddress)data).RevisionID);

								if (prev_address != null)
								{
									_KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
									object id = sender.Graph.Caches[typeof(SOAddress)].GetValue(prev_address, _RecordID);
									sender.SetValue(e.Row, _FieldOrdinal, id);
								}
							}
							break;
						}
					}
				}
			}
			base.RowPersisting(sender, e);
		}
		#endregion
	}

	public class SOBillingAddressAttribute : SOAddressAttribute
	{
		public SOBillingAddressAttribute(Type SelectType)
			: base(typeof(SOBillingAddress.addressID), typeof(SOBillingAddress.isDefaultAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOBillingAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<SOBillingAddress, SOBillingAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<SOBillingAddress, SOBillingAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<SOBillingAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<SOBillingAddress.overrideAddress>(sender, e.Row, true);
				PXUIFieldAttribute.SetEnabled<SOBillingAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	public class SOShippingAddressAttribute : SOAddressAttribute
	{
		public SOShippingAddressAttribute(Type SelectType)
			: base(typeof(SOShippingAddress.addressID), typeof(SOShippingAddress.isDefaultAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShippingAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<SOShippingAddress, SOShippingAddress.addressID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<SOShippingAddress, SOShippingAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<SOShippingAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<SOShippingAddress.overrideAddress>(sender, e.Row, true);
			}
		}

		public override void DefaultAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object AddressRow)
		{
            int? destinationSiteID = (int?)sender.GetValue<SOOrder.destinationSiteID>(DocumentRow);
            PXView view;
            bool issitebranch = false;
            if (destinationSiteID != null)
			{
                issitebranch = true;
                BqlCommand altSelect = BqlCommand.CreateInstance(
                    typeof(Select2<Address,
                                    InnerJoin<INSite,
                                        On2<INSite.FK.Address,
                                        And<INSite.siteID, Equal<Current<SOOrder.destinationSiteID>>>>,
                                    LeftJoin<SOShippingAddress,
                                        On<SOShippingAddress.customerID, Equal<Address.bAccountID>,
                                        And<SOShippingAddress.customerAddressID, Equal<Address.addressID>,
                                        And<SOShippingAddress.revisionID, Equal<Address.revisionID>,
                                        And<SOShippingAddress.isDefaultAddress, Equal<True>>>>>>>,
                                    Where<True, Equal<True>>>));
                view = sender.Graph.TypedViews.GetView(altSelect, false);
			}
            else
            {
                view = sender.Graph.TypedViews.GetView(_Select, false);
		}

            int startRow = -1;
            int totalRows = 0;
            bool addressFind = false;
            foreach (PXResult res in view.Select(new object[] { DocumentRow }, null, null, null, null, null, ref startRow, 1, ref totalRows))
            {
                addressFind = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, res);
                break;
            }

            if (!addressFind && !_Required)
                this.ClearRecord(sender, DocumentRow);

            if (!addressFind && _Required && issitebranch)
                throw new SharedRecordMissingException();
        }
	}

	public class SOShipmentAddressAttribute : AddressAttribute
	{
		public SOShipmentAddressAttribute(Type SelectType)
			: base(typeof(SOShipmentAddress.addressID), typeof(SOShipmentAddress.isDefaultAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShipmentAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
            SOShipment shipment = DocumentRow as SOShipment;
            if (shipment != null && shipment.ShipmentType == SOShipmentType.Transfer)
            {
	            PXResult addressRecord;
	            using (new PXReadBranchRestrictedScope())
	            {
	            addressRecord = PXSelectJoin<Address,
                    InnerJoin<INSite,
                                On<INSite.FK.Address>,
                    LeftJoin<SOShipmentAddress, On<SOShipmentAddress.customerID, Equal<Address.bAccountID>,
                            And<SOShipmentAddress.customerAddressID, Equal<Address.addressID>,
                            And<SOShipmentAddress.revisionID, Equal<Address.revisionID>,
                            And<SOShipmentAddress.isDefaultAddress, Equal<True>>>>>>>,
                    Where<INSite.siteID, Equal<Current<SOShipment.destinationSiteID>>>>.SelectMultiBound(sender.Graph, new object[] { DocumentRow });

                AddressAttribute.DefaultAddress<SOShipmentAddress, SOShipmentAddress.addressID>(sender, FieldName, DocumentRow, Row, addressRecord);
            }
			}
            else
            {
                DefaultAddress<SOShipmentAddress, SOShipmentAddress.addressID>(sender, DocumentRow, Row);
            }
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<SOShipmentAddress, SOShipmentAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<SOShipmentAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<SOShipmentAddress.overrideAddress>(sender, e.Row, true);
				PXUIFieldAttribute.SetEnabled<SOShipmentAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	public class SOUnbilledTax4Attribute : SOUnbilledTax2Attribute
	{
		public SOUnbilledTax4Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryExemptedAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryTaxAmt = typeof(SOTax.curyUnbilledTaxAmt).Name;

			this.CuryTranAmt = typeof(SOLine4.curyUnbilledAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine4.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOLine4.curyUnbilledAmt>, Data.Minus<SOLine4.curyUnbilledAmt>>),
				typeof(SumCalc<SOOrder.curyUnbilledLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Mult<
				Mult<Switch<Case<Where<SOLine4.operation, Equal<Parent<SOOrder.defaultOperation>>>, decimal1>, Data.Minus<decimal1>>, SOLine4.curyUnbilledAmt>,
				Sub<decimal1, Mult<SOLine4.groupDiscountRate, SOLine4.documentDiscountRate>>>),
				typeof(SumCalc<SOOrder.curyUnbilledDiscTotal>)));
			
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine4.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}
	}

	public class SOUnbilledTax2Attribute : SOUnbilledTaxAttribute
	{
		public SOUnbilledTax2Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryExemptedAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryTaxAmt = typeof(SOTax.curyUnbilledTaxAmt).Name;

			this.CuryTranAmt = typeof(SOLine2.curyUnbilledAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine2.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOLine2.curyUnbilledAmt>, Data.Minus<SOLine2.curyUnbilledAmt>>),
				typeof(SumCalc<SOOrder.curyUnbilledLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Mult<
				Mult<Switch<Case<Where<SOLine2.operation, Equal<Parent<SOOrder.defaultOperation>>>, decimal1>, Data.Minus<decimal1>>, SOLine2.curyUnbilledAmt>,
				Sub<decimal1, Mult<SOLine2.groupDiscountRate, SOLine2.documentDiscountRate>>>),
				typeof(SumCalc<SOOrder.curyUnbilledDiscTotal>)));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine2.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			TaxCalc = TaxCalc.Calc;
			//sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryUnbilledAmt_FieldUpdated);
		}

		//public virtual void CuryUnbilledAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		//{
		//	sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
		//	CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		//}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}
	}

	/// <summary>
	/// Extends <see cref="SOTaxAttribute"/> and calculates CuryUnbilledOrderTotal and OpenDoc for the Parent(Header) SOOrder.
	/// </summary>
	/// <example>
	/// [SOUnbilledTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>
	public class SOUnbilledTaxAttribute : SOTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 2;
			}
		}

		public SOUnbilledTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryExemptedAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryTaxAmt = typeof(SOTax.curyUnbilledTaxAmt).Name;

			this.CuryDocBal = null;
			this.CuryLineTotal = typeof(SOOrder.curyUnbilledLineTotal);
			this.CuryTaxTotal = typeof(SOOrder.curyUnbilledTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
			this.CuryTranAmt = typeof(SOLine.curyUnbilledAmt);

            this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyUnbilledAmt>, Data.Minus<SOLine.curyUnbilledAmt>>>, decimal0>),
				typeof(SumCalc<SOOrder.curyUnbilledLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine.lineType, Equal<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyUnbilledAmt>, Data.Minus<SOLine.curyUnbilledAmt>>>, decimal0>),
				typeof(SumCalc<SOOrder.curyUnbilledMiscTot>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Mult<
				Mult<Switch<Case<Where<SOLine.operation, Equal<Parent<SOOrder.defaultOperation>>>, decimal1>, Data.Minus<decimal1>>, SOLine.curyUnbilledAmt>,
				Sub<decimal1, Mult<SOLine.groupDiscountRate, SOLine.documentDiscountRate>>>),
				typeof(SumCalc<SOOrder.curyUnbilledDiscTotal>)));
		}

		#region Per Unit Taxes
		protected override string TaxableQtyFieldNameForTaxDetail => nameof(SOTax.UnbilledTaxableQty);
		#endregion

		protected override void CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);

			decimal CuryUnbilledLineTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledLineTotal>(sender.Graph) ?? 0m);
			decimal CuryUnbilledMiscTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledMiscTot>(sender.Graph) ?? 0m);
			decimal CuryUnbilledFreightTotal = CalcUnbilledFreightTotal(sender, CuryFreightTotal, CuryLineTotal, CuryUnbilledLineTotal);
			decimal CuryUnbilledDiscTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledDiscTotal>(sender.Graph) ?? 0m);

			CuryLineTotal += CuryMiscTotal;
			CuryUnbilledLineTotal += CuryUnbilledMiscTotal;

			decimal CuryUnbilledDocTotal = CuryUnbilledLineTotal + CuryUnbilledFreightTotal + CuryTaxTotal - CuryInclTaxTotal - CuryUnbilledDiscTotal;

			if (object.Equals(CuryUnbilledDocTotal, (decimal)(ParentGetValue<SOOrder.curyUnbilledOrderTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyUnbilledOrderTotal>(sender.Graph, CuryUnbilledDocTotal);
				ParentSetValue<SOOrder.openDoc>(sender.Graph, (CuryUnbilledDocTotal > 0m)); 
			}
		}

		protected virtual decimal CalcUnbilledFreightTotal(PXCache sender, decimal curyFreightTot, decimal curyLineTot, decimal curyUnbilledLineTot)
		{
			int? releasedCntr = (int?)ParentGetValue<SOOrder.releasedCntr>(sender.Graph);
			int? billedCntr = (int?)ParentGetValue<SOOrder.billedCntr>(sender.Graph);
			if (releasedCntr + billedCntr > 0)
			{
				SOSetup sosetup = PXSetup<SOSetup>.Select(sender.Graph);
				return (sosetup.FreightAllocation == FreightAllocationList.FullAmount || curyLineTot == 0m) ? 0m
					: PXCurrencyAttribute.RoundCury(ParentCache(sender.Graph), ParentRow(sender.Graph), curyFreightTot * curyUnbilledLineTot / curyLineTot);
			}
			else
			{
				return curyFreightTot;
			}
		}

		protected override bool IsFreightTaxable(PXCache sender, List<object> taxitems)
		{
			if (taxitems.Count > 0)
			{
				List<object> items = base.SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, null, PXTaxCheck.RecalcLine, ((SOTax)(PXResult<SOTax>)taxitems[0]).TaxID);

				return base.IsFreightTaxable(sender, items);
			}
			else
			{
				return false;
			}
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		protected override decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType = "I")
		{
			var extPrice = (decimal?)sender.GetValue(row, typeof(SOLine.curyUnbilledAmt).Name);
			var docDiscount = (decimal?)sender.GetValue(row, typeof(SOLine.documentDiscountRate).Name);
			var groupDiscount = (decimal?)sender.GetValue(row, typeof(SOLine.groupDiscountRate).Name);
			var value = extPrice * docDiscount * groupDiscount;
			return value;
		}

		public override object Insert(PXCache sender, object item)
		{
			return InsertCached(sender, item);
		}

		public override object Delete(PXCache sender, object item)
		{
			return DeleteCached(sender, item);
		}

		//Only base attribute should re-default taxes
		protected override void ReDefaultTaxes(PXCache cache, List<object> details)
		{
		}
		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
		}
	}

	public class SOOpenTax4Attribute : SOOpenTaxAttribute
	{
		public SOOpenTax4Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(SOTax.curyUnshippedTaxableAmt).Name;
			this._CuryTaxAmt = typeof(SOTax.curyUnshippedTaxAmt).Name;

			this.CuryTranAmt = typeof(SOLine4.curyOpenAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine4.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOLine4.curyOpenAmt>, Data.Minus<SOLine4.curyOpenAmt>>),
				typeof(SumCalc<SOOrder.curyOpenLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Mult<
				Mult<Switch<Case<Where<SOLine4.operation, Equal<Parent<SOOrder.defaultOperation>>>, decimal1>, Data.Minus<decimal1>>, SOLine4.curyOpenAmt>,
				Sub<decimal1, Mult<SOLine4.groupDiscountRate, SOLine4.documentDiscountRate>>>),
				typeof(SumCalc<SOOrder.curyOpenDiscTotal>)));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine4.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryUnbilledAmt_FieldUpdated);
		}

		public virtual void CuryUnbilledAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}
	}


	/// <summary>
	/// Extends <see cref="SOTaxAttribute"/> and calculates CuryOpenOrderTotal for the Parent(Header) SOOrder.
	/// </summary>
	/// <example>
	/// [SOOpenTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>
	public class SOOpenTaxAttribute : SOTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 1;
			}
		}

		public SOOpenTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(SOTax.curyUnshippedTaxableAmt).Name;
			this._CuryExemptedAmt = typeof(SOTax.curyUnshippedTaxableAmt).Name;
			this._CuryTaxAmt = typeof(SOTax.curyUnshippedTaxAmt).Name;

			this.CuryDocBal = typeof(SOOrder.curyOpenOrderTotal);
			this.CuryLineTotal = typeof(SOOrder.curyOpenLineTotal);
			this.CuryTaxTotal = typeof(SOOrder.curyOpenTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
			this.CuryTranAmt = typeof(SOLine.curyOpenAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyOpenAmt>, Data.Minus<SOLine.curyOpenAmt>>),
				typeof(SumCalc<SOOrder.curyOpenLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Mult<
				Mult<Switch<Case<Where<SOLine.operation, Equal<Parent<SOOrder.defaultOperation>>>, decimal1>, Data.Minus<decimal1>>, SOLine.curyOpenAmt>,
				Sub<decimal1, Mult<SOLine.groupDiscountRate, SOLine.documentDiscountRate>>>),
				typeof(SumCalc<SOOrder.curyOpenDiscTotal>)));
		}

		#region Per Unit Taxes
		protected override string TaxableQtyFieldNameForTaxDetail => nameof(SOTax.UnshippedTaxableQty);
		#endregion

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		protected override decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType = "I")
		{
			var extPrice = (decimal?)sender.GetValue(row, typeof(SOLine.curyOpenAmt).Name);
			var docDiscount = (decimal?)sender.GetValue(row, typeof(SOLine.documentDiscountRate).Name);
			var groupDiscount = (decimal?)sender.GetValue(row, typeof(SOLine.groupDiscountRate).Name);
			var value = extPrice * docDiscount * groupDiscount;
			return value;
		}

		public override object Insert(PXCache sender, object item)
		{
			return InsertCached(sender, item);
		}

		public override object Delete(PXCache sender, object item)
		{
			return DeleteCached(sender, item);
		}

		protected override void CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryOpenDiscTotal = (decimal)(ParentGetValue<SOOrder.curyOpenDiscTotal>(sender.Graph) ?? 0m);

			CuryLineTotal += CuryMiscTotal + CuryFreightTotal;

			decimal CuryOpenLineTotal = (decimal)(ParentGetValue<SOOrder.curyOpenLineTotal>(sender.Graph) ?? 0m);
			decimal CuryOpenDocTotal = CuryOpenLineTotal + CuryTaxTotal - CuryInclTaxTotal - CuryOpenDiscTotal;

			if (object.Equals(CuryOpenDocTotal, (decimal)(ParentGetValue<SOOrder.curyOpenOrderTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyOpenOrderTotal>(sender.Graph, CuryOpenDocTotal);
			}
		}

		protected override bool IsFreightTaxable(PXCache sender, List<object> taxitems)
		{
			if (taxitems.Count > 0)
			{
				List<object> items = base.SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, null, PXTaxCheck.RecalcLine, ((SOTax)(PXResult<SOTax>)taxitems[0]).TaxID);

				return base.IsFreightTaxable(sender, items);
			}
			else
			{
				return false;
			}
		}

		//Only base attribute should re-default taxes
		protected override void ReDefaultTaxes(PXCache cache, List<object> details)
		{
		}
		protected override void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
		}
	}

	/// <summary>
	/// Extends <see cref="SOTaxAttribute"/> and calculates CuryOrderTotal and CuryTaxTotal for the Parent(Header) SOOrder.
	/// This Attribute overrides some of functionality of <see cref="SOTaxAttribute"/>. 
	/// This Attribute is applied to the TaxCategoryField of SOOrder instead of SO Line.
	/// </summary>
	/// <example>
	/// [SOOrderTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>
	public class SOOrderTaxAttribute : SOTaxAttribute
	{
		public SOOrderTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: this(ParentType, TaxType, TaxSumType, null)
		{
		}

		public SOOrderTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type TaxCalculationMode)
			: base(ParentType, TaxType, TaxSumType, TaxCalculationMode)
		{
			CuryTranAmt = typeof(SOOrder.curyFreightTot);
			TaxCategoryID = typeof(SOOrder.freightTaxCategoryID);

			this._Attributes.Clear();
		}

		protected override object InitializeTaxDet(object data)
		{
			object new_data =  base.InitializeTaxDet(data);
			if (new_data.GetType() == _TaxType)
			{
				((SOTax)new_data).LineNbr = 32000;
			}

			return new_data;
		}

        protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, int32000>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		protected override void CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscountTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);

			decimal CuryDocTotal = CuryLineTotal + CuryMiscTotal + CuryFreightTotal + CuryTaxTotal - CuryInclTaxTotal - CuryDiscountTotal;

			if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<SOOrder.curyOrderTotal>(sender.Graph) ?? 0m)) == false ||
				object.Equals(CuryTaxTotal, (decimal)(ParentGetValue<SOOrder.curyTaxTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyOrderTotal>(sender.Graph, CuryDocTotal);
				ParentSetValue<SOOrder.curyTaxTotal>(sender.Graph, CuryTaxTotal);
			}
		}

		protected override void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
		}

		protected override void Tax_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if ((_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc) && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				object det = ParentRow(sender.Graph);
				{
					ITaxDetail taxzonedet = MatchesCategory(cache, det, (ITaxDetail)e.Row);
					AddOneTax(taxcache, det, taxzonedet);
				}
				_NoSumTotals = (_TaxCalc == TaxCalc.ManualCalc && e.ExternalCall == false);

				PXRowDeleting del = delegate(PXCache _sender, PXRowDeletingEventArgs _e) { _e.Cancel |= object.ReferenceEquals(e.Row, _e.Row); };
				sender.Graph.RowDeleting.AddHandler(_TaxSumType, del);
				try
				{
					CalcTaxes(cache, null);
				}
				finally
				{
					sender.Graph.RowDeleting.RemoveHandler(_TaxSumType, del);
				}
				_NoSumTotals = false;
			}
		}

		protected override void Tax_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if ((_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc) && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				object det = ParentRow(sender.Graph);
				{
					DelOneTax(taxcache, det, e.Row);
				}
				CalcTaxes(cache, null);
			}
		}
		
		protected override void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (!CompareZone(sender.Graph, (string)e.OldValue, (string)sender.GetValue(e.Row, _TaxZoneID)))
				{
					Preload(sender);

					ReDefaultTaxes(sender, e.Row, e.Row, false);

					_ParentRow = e.Row;
					CalcTaxes(sender, e.Row);
					_ParentRow = null;
				}
			}
		}

		protected override void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				Preload(sender);

				ReDefaultTaxes(sender, e.Row, e.Row, false);

				_ParentRow = e.Row;
				CalcTaxes(sender, e.Row);
				_ParentRow = null;
			}
		}

	    protected override bool ShouldUpdateFinPeriodID(PXCache sender, object oldRow, object newRow)
	    {
	        return false;
	    }

        public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ParentRow = e.Row;
			base.RowUpdated(sender, e);
			_ParentRow = null;
		}

		#region Per Unit Taxes Override Stubs	
		/// <summary>
		/// Fill tax details for line for per unit taxes. Do nothing for retained tax.
		/// </summary>
		protected override void TaxSetLineDefaultForPerUnitTaxes(PXCache rowCache, object row, Tax tax, TaxRev taxRevision, TaxDetail taxDetail)
		{
		}

		/// <summary>
		/// Fill aggregated tax detail for per unit tax. Do nothing for retained tax.
		/// </summary>
		protected override TaxDetail FillAggregatedTaxDetailForPerUnitTax(PXCache rowCache, object row, Tax tax, TaxRev taxRevision,
																		  TaxDetail aggrTaxDetail, List<object> taxItems)
		{
			return aggrTaxDetail;
		}
		#endregion
	}

	public class SOUnbilledMiscTax2Attribute : SOUnbilledTaxAttribute
	{
		public SOUnbilledMiscTax2Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this._CuryTaxableAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryExemptedAmt = typeof(SOTax.curyUnbilledTaxableAmt).Name;
			this._CuryTaxAmt = typeof(SOTax.curyUnbilledTaxAmt).Name;

			this.CuryDocBal = null;
			this.CuryLineTotal = typeof(SOOrder.curyUnbilledMiscTot);
			this.CuryTaxTotal = typeof(SOOrder.curyUnbilledTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
			this.CuryTranAmt = typeof(SOMiscLine2.curyUnbilledAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOMiscLine2.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOMiscLine2.curyUnbilledAmt>, Data.Minus<SOMiscLine2.curyUnbilledAmt>>),
				typeof(SumCalc<SOOrder.curyUnbilledMiscTot>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Mult<
				Mult<Switch<Case<Where<SOMiscLine2.operation, Equal<Parent<SOOrder.defaultOperation>>>, decimal1>, Data.Minus<decimal1>>, SOMiscLine2.curyUnbilledAmt>,
				Sub<decimal1, Mult<SOMiscLine2.groupDiscountRate, SOMiscLine2.documentDiscountRate>>>),
				typeof(SumCalc<SOOrder.curyUnbilledDiscTotal>)));
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOMiscLine2.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			TaxCalc = TaxCalc.Calc;
			//sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryUnbilledAmt_FieldUpdated);
		}

		//public virtual void CuryUnbilledAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		//{
		//	sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
		//	CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		//}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}

	}

	/// <summary>
	/// Specialized for SO version of the TaxAttribute. <br/>
	/// Provides Tax calculation for SOLine, by default is attached to SOLine (details) and SOOrder (header) <br/>
	/// Normally, should be placed on the TaxCategoryID field. <br/>
	/// Internally, it uses SOOrder graphs, otherwise taxes are not calculated (tax calc Level is set to NoCalc).<br/>
	/// As a result of this attribute work a set of SOTax tran related to each SOLine  and to their parent will created <br/>
	/// May be combined with other attrbibutes with similar type - for example, SOOpenTaxAttribute <br/>
	/// </summary>
	/// <example>
	/// [SOTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>    
	public class SOTaxAttribute : TaxAttribute
	{
		protected virtual short SortOrder
		{
			get
			{
				return 0;
			}
		}
		
		protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }

		public SOTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: this(ParentType, TaxType, TaxSumType, null)
		{
		}

		public SOTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type TaxCalculationMode)
			: base(ParentType, TaxType, TaxSumType, TaxCalculationMode)
		{
			this.CuryDocBal = null;
			this.CuryLineTotal = typeof(SOOrder.curyLineTotal);
			this.CuryTaxTotal = typeof(SOOrder.curyTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
			this.CuryTranAmt = typeof(SOLine.curyLineAmt);
            this.GroupDiscountRate = typeof(SOLine.groupDiscountRate);
			TaxCalcMode = typeof(SOOrder.taxCalcMode);

			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyLineAmt>, Data.Minus<SOLine.curyLineAmt>>>, decimal0>), 
				typeof(SumCalc<SOOrder.curyLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine.lineType, Equal<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyLineAmt>, Data.Minus<SOLine.curyLineAmt>>>, decimal0>),
				typeof(SumCalc<SOOrder.curyMiscTot>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(
				typeof(Switch<Case<Where<SOLine.commissionable, Equal<True>>, Mult<Mult<SOLine.curyLineAmt, SOLine.groupDiscountRate>, SOLine.documentDiscountRate>>, decimal0>),
				typeof(SumCalc<SOSalesPerTran.curyCommnblAmt>)));
		}

		public override int CompareTo(object other)
		{
 			 return this.SortOrder.CompareTo(((SOTaxAttribute)other).SortOrder);
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}
		
        protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
            decimal val = 0m;
            val = (base.GetCuryTranAmt(sender, row) ?? 0m) * 
				  ((decimal?)sender.GetValue(row, _GroupDiscountRate) ?? 1m) * 
				  ((decimal?)sender.GetValue(row, _DocumentDiscountRate) ?? 1m);

			PXCache cache = sender.Graph.Caches[typeof(SOOrder)];
			
			if (row != null && !object.Equals(sender.GetValue<SOLine.operation>(row), cache.GetValue<SOOrder.defaultOperation>(cache.Current)))
			{
                return -PXDBCurrencyAttribute.Round(sender, row, val, CMPrecision.TRANCURY);
			}

            return PXDBCurrencyAttribute.Round(sender, row, val, CMPrecision.TRANCURY);
		}

		/// <summary>
		/// Check if per-unit tax amount sign should be inverted. For <see cref="SOTaxAttribute"/> checks the same conditions for sign invertion
		/// as the <see cref="SOTaxAttribute.GetCuryTranAmt(PXCache, object, string)"/> method override.
		/// </summary>
		/// <param name="rowCache">The row cache.</param>
		/// <param name="row">The row.</param>
		/// <param name="tax">The tax.</param>
		/// <param name="taxRevison">The tax revison.</param>
		/// <param name="taxDetailCache">The tax detail cache.</param>
		/// <param name="taxDetail">The tax detail.</param>
		/// <returns/>
		protected override bool InvertPerUnitTaxAmountSign(PXCache rowCache, object row, Tax tax, TaxRev taxRevison,
														   PXCache taxDetailCache, TaxDetail taxDetail)
		{
			if (row == null)
				return base.InvertPerUnitTaxAmountSign(rowCache, row, tax, taxRevison, taxDetailCache, taxDetail);

			PXCache ordersCache = rowCache.Graph.Caches[typeof(SOOrder)];
			string lineOperation = rowCache.GetValue<SOLine.operation>(row) as string;
			string defaultOperation = ordersCache.GetValue<SOOrder.defaultOperation>(ordersCache.Current) as string;

			return defaultOperation != lineOperation
				? true
				: base.InvertPerUnitTaxAmountSign(rowCache, row, tax, taxRevison, taxDetailCache, taxDetail);
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
			
			//if (this.GetType() == typeof(SOTaxAttribute) && CuryLineTotal < 0m)
			//{
			//	CuryLineTotal = -CuryLineTotal;
			//	CuryTaxTotal = -CuryTaxTotal;
			//	CuryDiscountTotal = -CuryDiscountTotal;
			//}

			decimal CuryDocTotal = CuryLineTotal + CuryMiscTotal + CuryFreightTotal + CuryTaxTotal - CuryInclTaxTotal - CuryDiscountTotal;

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

		//PXParent attributes pointing on SOLine in SOTax DAC don't work because SOTax cache is initialized before SOLine cache
 		protected override object SelectParent(PXCache sender, object row)
		{
			if (row.GetType() == typeof(SOTax) && ((SOTax)row).LineNbr == 32000) //freight lines
			{
				if (_ChildType != typeof(SOOrder))
			{
					return null; 
				}
				return (SOOrder)PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Current<SOTax.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOTax.orderNbr>>>>>.Select(sender.Graph, row);
			}
			else
			{
				return base.SelectParent(sender, row);
			}
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
			return SelectTaxes<Where, Current<SOLine.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
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

			PXRowInserted inserted = delegate(PXCache cache, PXRowInsertedEventArgs e)
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

			PXRowDeleted deleted = delegate(PXCache cache, PXRowDeletedEventArgs e)
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
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

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
							if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<SOTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
									idx--) ;

								Tax adjdTax = AdjustTaxLevel(graph, (Tax)line);
								ret.Insert(idx, new PXResult<SOTax, Tax, TaxRev>(record, adjdTax, (TaxRev)line));
							}
						}

						//resultset is always sorted by LineNbr
						//if (record.LineNbr > linenbr) break;
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

						if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((SOTax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& taxByCalculationLevelComparer.Compare((PXResult<SOTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
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
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& taxByCalculationLevelComparer.Compare((PXResult<SOTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
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

		protected override List<object> SelectDocumentLines(PXGraph graph, object row)
		{
			List<object> ret = PXSelect<SOLine,
								Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
									And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
									.SelectMultiBound(graph, new object[] { row })
									.RowCast<SOLine>()
									.Select(_ => (object)_)
									.ToList();
			return ret;
		}

		protected override decimal? GetDocLineFinalAmtNoRounding(PXCache sender, object row, string TaxCalcType = "I")
		{
			var extPrice = (decimal?)sender.GetValue(row, typeof(SOLine.curyExtPrice).Name);
			var discAmount = (decimal?)sender.GetValue(row, typeof(SOLine.curyDiscAmt).Name);
			var docDiscount = (decimal?)sender.GetValue(row, typeof(SOLine.documentDiscountRate).Name);
			var groupDiscount = (decimal?)sender.GetValue(row, typeof(SOLine.groupDiscountRate).Name);
			var value = (extPrice - discAmount ) * docDiscount * groupDiscount;
			return value;
		}



		protected override void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			if (SortOrder == 0)
				base.DefaultTaxes(sender, row, DefaultExisting);
		}

		protected override void ClearTaxes(PXCache sender, object row)
		{
			if (SortOrder == 0)
				base.ClearTaxes(sender, row);
		}

		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();

            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>();

			if (sender.Graph is SOOrderEntry)
			{
                base.CacheAttached(sender);

				sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), typeof(SOOrder.curyDiscTot).Name, SOOrder_CuryDiscTot_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), typeof(SOOrder.curyFreightTot).Name, SOOrder_CuryFreightTot_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), _CuryTaxTotal, SOOrder_CuryTaxTot_FieldUpdated);
			}
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

		protected virtual void SOOrder_CuryDiscTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool calc = true;
			TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
			if (taxZone != null && taxZone.IsExternal == true)
				calc = false;
			
			PXCache cache = sender.Graph.Caches[typeof(SOLine)];
			
			this._ParentRow = e.Row;
			CalcTotals(sender, e.Row, calc);
			this._ParentRow = null;
		}

		protected virtual void SOOrder_CuryFreightTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool calc = true;
			TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
			if (taxZone != null && taxZone.IsExternal == true)
				calc = false;

			this._ParentRow = e.Row;
			CalcTotals(sender, e.Row, calc);
			this._ParentRow = null;
		}

        protected virtual void SOOrder_CuryTaxTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
            if (taxZone != null && taxZone.IsExternal == true)
            {
                decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
                decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
                CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
            }
        }
    }

	public class SOInvoiceTaxAttribute : ARTaxAttribute
	{
        public SOInvoiceTaxAttribute()
            : base(typeof(PX.Objects.AR.ARInvoice), typeof(ARTax), typeof(ARTaxTran))
        {
            _Attributes.Clear();
            _Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, NotEqual<SOLineType.miscCharge>, And<ARTran.lineType, NotEqual<SOLineType.freight>, And<ARTran.lineType, NotEqual<SOLineType.discount>>>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARInvoice.curyGoodsTotal>)));
            _Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, Equal<SOLineType.miscCharge>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARInvoice.curyMiscTot>)));
            _Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, Equal<SOLineType.freight>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARInvoice.curyFreightTot>)));
			_Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<
				Case<Where<ARTran.commissionable, Equal<True>, And<Where<ARTran.lineType, NotEqual<SOLineType.discount>,
					And<Where<ARTran.tranType, Equal<ARDocType.creditMemo>, Or<ARTran.tranType, Equal<ARDocType.cashReturn>>>>>>>,
						Data.Minus<Sub<Sub<ARTran.curyTranAmt, Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.origGroupDiscountRate>, ARTran.origDocumentDiscountRate>>>,
						Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.groupDiscountRate>, ARTran.documentDiscountRate>>>>,
				Case<Where<ARTran.commissionable, Equal<True>, And<Where<ARTran.lineType, NotEqual<SOLineType.discount>>>>,
						Sub<Sub<ARTran.curyTranAmt, Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.origGroupDiscountRate>, ARTran.origDocumentDiscountRate>>>,
						Sub<ARTran.curyTranAmt, Mult<Mult<ARTran.curyTranAmt, ARTran.groupDiscountRate>, ARTran.documentDiscountRate>>>>>,
				decimal0>),
				typeof(SumCalc<ARSalesPerTran.curyCommnblAmt>)));
        }

		protected override void Tax_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc && e.ExternalCall)
			{
				base.Tax_RowDeleted(sender, e);
			}

			if (_TaxCalc == TaxCalc.ManualCalc)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				//Preload detail taxes into PXCache
				SelectTaxes(sender, null, PXTaxCheck.RecalcLine);
			}
		}
        protected override bool ConsiderEarlyPaymentDiscount(PXCache sender, object parent, Tax tax)
        {
			var doc = (ARInvoice)parent;
			if (doc.DocType != ARDocType.CashSale && doc.DocType != ARDocType.CashReturn)
                return base.ConsiderEarlyPaymentDiscount(sender, parent, tax);
            else
                return
                    (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
                    || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
                                &&
                    tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment;
        }
        protected override bool ConsiderInclusiveDiscount(PXCache sender, object parent, Tax tax)
        {
			var doc = (ARInvoice)parent;
			if (doc.DocType != ARDocType.CashSale && doc.DocType != ARDocType.CashReturn)
				return base.ConsiderInclusiveDiscount(sender, parent, tax);
            else
                return (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment);
        }

		protected override object SelectParent(PXCache cache, object row)
		{
			if (_TaxCalc == TaxCalc.ManualCalc)
			{
				//locate parent in cache
				object detrow = TaxParentAttribute.LocateParent(cache, row, _ChildType);

				if (FilterParent(cache, detrow))
				{
					return null;
				}
				return detrow;
			}
			else
			{
				return base.SelectParent(cache, row);
			}
			}

		protected virtual bool FilterParent(PXCache cache, object detrow)
		{
			if (detrow == null || cache.Graph.Caches[_ChildType].GetStatus(detrow) == PXEntryStatus.Notchanged)
				return true;

			if (_ChildType == typeof(ARTran))
			{
				ARTran tran = (ARTran)detrow;
				PXCache orderShipmentCache = cache.Graph.Caches[typeof(SOOrderShipment)];
				SOOrderShipment orderShipment = (SOOrderShipment)orderShipmentCache?.Current;
				return orderShipment != null && (orderShipment.OrderType != tran.SOOrderType || orderShipment.OrderNbr != tran.SOOrderNbr
					|| orderShipment.ShipmentType != tran.SOShipmentType || orderShipment.ShipmentNbr != tran.SOShipmentNbr);
			}
			return false;
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOInvoice)];
			SOInvoice current = (SOInvoice)PXParentAttribute.SelectParent(sender, row, typeof(SOInvoice)); 

			decimal CuryLineTotal = (decimal?)cache.GetValue<ARInvoice.curyGoodsTotal>(current) ?? 0m;
			decimal CuryMiscTotal = (decimal?)cache.GetValue<ARInvoice.curyMiscTot>(current) ?? 0m;
			decimal CuryFreightTotal = (decimal?)cache.GetValue<ARInvoice.curyFreightTot>(current) ?? 0m;
            decimal CuryDiscountTotal = (decimal?)cache.GetValue<ARInvoice.curyDiscTot>(current) ?? 0m;

			if (CuryLineTotal + CuryMiscTotal + CuryFreightTotal != 0m && CuryTaxableAmt != 0m)
			{
                if (Math.Abs(CuryTaxableAmt - CuryLineTotal - CuryMiscTotal) < 0.00005m)
                {
                    CuryTaxableAmt -= CuryDiscountTotal;
                }
			}
		}

		protected override void CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			PXCache cache = sender.Graph.Caches[typeof(ARInvoice)];

			ARInvoice doc = null;
			if ( row is ARInvoice )
			{
				doc = (ARInvoice) row;
			}
			else
			{
				doc = (ARInvoice)PXParentAttribute.SelectParent(sender, row, typeof(ARInvoice));	
			}
			

			decimal CuryLineTotal = ((decimal?)cache.GetValue<ARInvoice.curyGoodsTotal>(doc) ?? 0m)
									+((decimal?)cache.GetValue<ARInvoice.curyMiscTot>(doc) ?? 0m)
									+ ((decimal?)cache.GetValue<ARInvoice.curyFreightTot>(doc) ?? 0m);

            decimal CuryDiscTotal = ((decimal?)cache.GetValue<ARInvoice.curyDiscTot>(doc) ?? 0m);

            decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal + CuryTaxDiscountTotal - CuryInclTaxTotal - CuryDiscTotal;

			decimal doc_CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);
			decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);
            decimal doc_CuryDiscTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot) ?? 0m);

			if (object.Equals(CuryLineTotal, doc_CuryLineTotal) == false ||
				object.Equals(CuryTaxTotal, doc_CuryTaxTotal) == false ||
                object.Equals(CuryDiscTotal, doc_CuryDiscTotal) == false)
			{
				ParentSetValue(sender.Graph, _CuryLineTotal, CuryLineTotal);
				ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);
                ParentSetValue(sender.Graph, _CuryDiscTot, CuryDiscTotal);
				if (!string.IsNullOrEmpty(_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
					return;
				}
			}

            if (!string.IsNullOrEmpty(_CuryTaxDiscountTotal))
            {
                ParentSetValue(sender.Graph, _CuryTaxDiscountTotal, CuryTaxDiscountTotal);
            }

			if (!string.IsNullOrEmpty(_CuryDocBal))
			{
				decimal doc_CuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);

				if (object.Equals(CuryDocTotal, doc_CuryDocBal) == false)
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
				}
			}
		}

		protected override void IsTaxSavedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice invoice = e.Row as ARInvoice;

			if (invoice != null)
			{
				decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
				decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
				CalcDocTotals(sender, invoice, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
			}
		}

		protected override DateTime? GetDocDate(PXCache sender, object row)
		{
			bool isCancellationInvoice = (bool?)ParentGetValue<ARRegister.isCancellation>(sender.Graph) == true;
			DateTime? origDocDate = (DateTime?)ParentGetValue<ARRegister.origDocDate>(sender.Graph);
			if (isCancellationInvoice && origDocDate != null)
			{
				return origDocDate;
			}
			else
			{
				return base.GetDocDate(sender, row);
			}
		}
	}


	public abstract class SOCustomListAttribute : PXStringListAttribute
			{
		public string[] AllowedValues => _AllowedValues;
		public string[] AllowedLabels => _AllowedLabels;

		protected SOCustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) { }

		protected abstract Tuple<string, string>[] GetPairs();

            public override void CacheAttached(PXCache sender)
            {
			var pairs = GetPairs();
			_AllowedValues = pairs.Select(t => t.Item1).ToArray();
			_AllowedLabels = pairs.Select(t => t.Item2).ToArray();
                _NeutralAllowedLabels = _AllowedLabels;

                base.CacheAttached(sender);
            }

		protected static string MaskLocationLabel
			=> !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>()
				? AR.Messages.MaskCustomer
				: AR.Messages.MaskLocation;
		}

	public class SOSalesAcctSubDefault
	{
		public class AcctListAttribute : SOCustomListAttribute
		{
			private static Tuple<string, string>[] Pairs =>
				new[]
			{
					Pair(MaskItem, IN.Messages.MaskItem),
					Pair(MaskSite, IN.Messages.MaskSite),
					Pair(MaskClass, IN.Messages.MaskClass),
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskReasonCode, IN.Messages.MaskReasonCode),
				};

			public AcctListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
			}

		public class SubListAttribute : SOCustomListAttribute
            {
			private static Tuple<string, string>[] Pairs =>
				new[]
				{
					Pair(MaskItem, IN.Messages.MaskItem),
					Pair(MaskSite, IN.Messages.MaskSite),
					Pair(MaskClass, IN.Messages.MaskClass),
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskEmployee, AR.Messages.MaskEmployee),
					Pair(MaskCompany, AR.Messages.MaskCompany),
					Pair(MaskSalesPerson, AR.Messages.MaskSalesPerson),
					Pair(MaskReasonCode, IN.Messages.MaskReasonCode),
				};

			public SubListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskItem = "I";
		public const string MaskSite = "W";
		public const string MaskClass = "P";
		public const string MaskReasonCode = "R";

		public const string MaskLocation = "L";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
		public const string MaskSalesPerson = "S";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    [SOSalesAcctSubDefault.SubList]
	public sealed class SOSalesSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZA";
		public SOSalesSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOSalesAcctSubDefault.MaskItem, new SOSalesAcctSubDefault.SubListAttribute().AllowedValues, new SOSalesAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<SOSalesAcctSubDefault.SubListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (SOSalesAcctSubDefault.SubListAttribute)_Attributes.First(x => x.GetType() == typeof(SOSalesAcctSubDefault.SubListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOSalesAcctSubDefault.SubListAttribute().AllowedValues, 3, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOSalesAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}


	public class SOMiscAcctSubDefault
	{
		public class AcctListAttribute : SOCustomListAttribute
		{
			private static Tuple<string, string>[] Pairs =>
				new[]
			{
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskItem, AR.Messages.MaskItem),
				};

			public AcctListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : SOCustomListAttribute
			{
			private static Tuple<string, string>[] Pairs =>
				new[]
            {
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskItem, AR.Messages.MaskItem),
					Pair(MaskEmployee, AR.Messages.MaskEmployee),
					Pair(MaskCompany, AR.Messages.MaskCompany),
				};

			public SubListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskItem = "I";
		public const string MaskLocation = "L";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    [SOMiscAcctSubDefault.SubList]
	public sealed class SOMiscSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZB";
		public SOMiscSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOMiscAcctSubDefault.MaskItem, new SOMiscAcctSubDefault.SubListAttribute().AllowedValues, new SOMiscAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<SOMiscAcctSubDefault.SubListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (SOMiscAcctSubDefault.SubListAttribute)_Attributes.First(x => x.GetType() == typeof(SOMiscAcctSubDefault.SubListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOMiscAcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOMiscAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}


	public class SOFreightAcctSubDefault
	{
		public class AcctListAttribute : SOCustomListAttribute
		{
			private static Tuple<string, string>[] Pairs =>
				new[]
			{
					Pair(OrderType, Messages.OrderType),
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskShipVia, Messages.ShipVia),
				};

			public AcctListAttribute() : base(Pairs) {}
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : SOCustomListAttribute
		{
			private static Tuple<string, string>[] Pairs =>
				new[]
			{
					Pair(OrderType, Messages.OrderType),
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskShipVia, Messages.ShipVia),
					Pair(MaskCompany, AR.Messages.MaskCompany),
					Pair(MaskEmployee, AR.Messages.MaskEmployee),
				};

			public SubListAttribute() : base(Pairs) { }
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskShipVia = "V";
		public const string MaskLocation = "L";
		public const string OrderType = "T";
        public const string MaskCompany = "C";
		public const string MaskEmployee = "E";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    [SOFreightAcctSubDefault.SubList]
	public sealed class SOFreightSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZC";
		public SOFreightSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOFreightAcctSubDefault.MaskLocation, new SOFreightAcctSubDefault.SubListAttribute().AllowedValues, new SOFreightAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<SOFreightAcctSubDefault.SubListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (SOFreightAcctSubDefault.SubListAttribute)_Attributes.First(x => x.GetType() == typeof(SOFreightAcctSubDefault.SubListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField 
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOFreightAcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOFreightAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}




	public class SODiscAcctSubDefault
	{
		public class AcctListAttribute : SOCustomListAttribute
			{
			private static Tuple<string, string>[] Pairs =>
				new[]
				{
					Pair(OrderType, Messages.OrderType),
					Pair(MaskLocation, MaskLocationLabel),
				};

			public AcctListAttribute() : base(Pairs) {}
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : SOCustomListAttribute
		{
			private static Tuple<string, string>[] Pairs =>
				new[]
			{
					Pair(OrderType, Messages.OrderType),
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskCompany, AR.Messages.MaskCompany),
				};

			public SubListAttribute() : base(Pairs) {}
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string OrderType = "T";
		public const string MaskLocation = "L";
        public const string MaskCompany = "C";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    [SODiscAcctSubDefault.SubList]
	public sealed class SODiscSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZD";
		public SODiscSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SODiscAcctSubDefault.MaskLocation, new SODiscAcctSubDefault.SubListAttribute().AllowedValues, new SODiscAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<SODiscAcctSubDefault.SubListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (SODiscAcctSubDefault.SubListAttribute)_Attributes.First(x => x.GetType() == typeof(SODiscAcctSubDefault.SubListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField 
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SODiscAcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SODiscAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	public class SOCOGSAcctSubDefault
	{
		public class AcctListAttribute : SOCustomListAttribute
		{
			private static Tuple<string, string>[] Pairs =>
				new[]
			{
					Pair(MaskItem, IN.Messages.MaskItem),
					Pair(MaskSite, IN.Messages.MaskSite),
					Pair(MaskClass, IN.Messages.MaskClass),
					Pair(MaskLocation, MaskLocationLabel),
				};

			public AcctListAttribute() : base(Pairs) {}
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public class SubListAttribute : SOCustomListAttribute
			{
			private static Tuple<string, string>[] Pairs =>
				new[]
            {
					Pair(MaskItem, IN.Messages.MaskItem),
					Pair(MaskSite, IN.Messages.MaskSite),
					Pair(MaskClass, IN.Messages.MaskClass),
					Pair(MaskLocation, MaskLocationLabel),
					Pair(MaskEmployee, AR.Messages.MaskEmployee),
					Pair(MaskCompany, AR.Messages.MaskCompany),
					Pair(MaskSalesPerson, AR.Messages.MaskSalesPerson),
				};

			public SubListAttribute() : base(Pairs) {}
			protected override Tuple<string, string>[] GetPairs() => Pairs;
		}

		public const string MaskItem = "I";
		public const string MaskSite = "W";
		public const string MaskClass = "P";

		public const string MaskLocation = "L";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
		public const string MaskSalesPerson = "S";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    [SOCOGSAcctSubDefault.SubList]
	public sealed class SOCOGSSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZE";
		public SOCOGSSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOCOGSAcctSubDefault.MaskItem, new SOCOGSAcctSubDefault.SubListAttribute().AllowedValues, new SOCOGSAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<SOCOGSAcctSubDefault.SubListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (SOCOGSAcctSubDefault.SubListAttribute)_Attributes.First(x => x.GetType() == typeof(SOCOGSAcctSubDefault.SubListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOCOGSAcctSubDefault.SubListAttribute().AllowedValues, 3, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOCOGSAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	public class SOSiteStatusLookup<Status, StatusFilter> : INSiteStatusLookup<Status, StatusFilter>
		where Status : class, IBqlTable, new()
		where StatusFilter:  SOSiteStatusFilter, new()
	{
		#region Ctor
		public SOSiteStatusLookup(PXGraph graph)
			:base(graph)
		{
			graph.RowSelecting.AddHandler(typeof(SOSiteStatusSelected), OnRowSelecting);
		}

		public SOSiteStatusLookup(PXGraph graph, Delegate handler)
			:base(graph, handler)
		{
			graph.RowSelecting.AddHandler(typeof(SOSiteStatusSelected), OnRowSelecting);
		}
		#endregion
		protected virtual void OnRowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (sender.Fields.Contains(typeof(SOSiteStatusSelected.curyID).Name) &&
					sender.GetValue<SOSiteStatusSelected.curyID>(e.Row) == null)
			{
				PXCache orderCache = sender.Graph.Caches[typeof(SOOrder)];
				sender.SetValue<SOSiteStatusSelected.curyID>(e.Row,
					orderCache.GetValue<SOOrder.curyID>(orderCache.Current));
				sender.SetValue<SOSiteStatusSelected.curyInfoID>(e.Row,
					orderCache.GetValue<SOOrder.curyInfoID>(orderCache.Current));				
			}
		}

		protected override void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.OnFilterSelected(sender, e);
			SOSiteStatusFilter filter = (SOSiteStatusFilter)e.Row;
			PXUIFieldAttribute.SetVisible<SOSiteStatusFilter.historyDate>(sender, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<SOSiteStatusFilter.dropShipSales>(sender, null, filter.Mode == SOAddItemMode.ByCustomer);

			PXCache status = sender.Graph.Caches[typeof (SOSiteStatusSelected)];
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.qtyLastSale>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.curyID>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.curyUnitPrice>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.lastSalesDate>(status, null, filter.Mode == SOAddItemMode.ByCustomer);

			status.Adjust<PXUIFieldAttribute>()
				.For<SOSiteStatusSelected.dropShipLastBaseQty>(x => { x.Visible = filter.DropShipSales == true; })
				.SameFor<SOSiteStatusSelected.dropShipLastQty>()
				.SameFor<SOSiteStatusSelected.dropShipLastUnitPrice>()
				.SameFor<SOSiteStatusSelected.dropShipCuryUnitPrice>()
				.SameFor<SOSiteStatusSelected.dropShipLastDate>();

			if (filter.HistoryDate == null)
				filter.HistoryDate =  sender.Graph.Accessinfo.BusinessDate.GetValueOrDefault().AddMonths(-3);
		}
	}
	#region SOOpenPeriod
	/// <summary>
	/// Specialized version of the selector for SO Open Financial Periods.<br/>
	/// Displays a list of FinPeriods, having flags Active = true and  ARClosed = false and INClosed = false.<br/>
	/// </summary>
	public class SOOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public SOOpenPeriodAttribute(Type sourceType,
			Type branchSourceType,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			bool useMasterOrganizationIDByDefault = false,
			Type masterFinPeriodIDType = null)
			: base(
				typeof(Search<FinPeriod.finPeriodID, 
							Where2<Where<Current<SOInvoice.createINDoc>, Equal<False>, 
											Or<FinPeriod.iNClosed, Equal<False>>>,
									And<FinPeriod.aRClosed, Equal<False>, 
									And<Where<FinPeriod.status, Equal<FinPeriod.status.open>>>>>>),
				sourceType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault,
				masterFinPeriodIDType: masterFinPeriodIDType)
		{
		}

		public SOOpenPeriodAttribute(Type SourceType)
			: this(SourceType, null)
		{
		}

		public SOOpenPeriodAttribute()
			: this(null)
		{
		}
		#endregion

		#region Implementation

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_ValidatePeriod != PeriodValidation.Nothing)
			{
				OpenPeriodVerifying(sender, e);
			}
		}

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError)
			{
				if (finPeriod.ARClosed == true)
				{
					result.Aggregate(HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: AR.Messages.FinancialPeriodClosedInAR));
			}

				if (finPeriod.INClosed == true)
			{
					bool? createINDoc = ((SOInvoice)sender.Graph.Caches<SOInvoice>().Current)?.CreateINDoc;

				if (createINDoc == true)
				{
						result.Aggregate(HandleErrorThatPeriodIsClosed(sender, finPeriod, errorMessage: IN.Messages.FinancialPeriodClosedInIN));
				}
			}
		}
			
			return result;
		}

		#endregion
	}
	#endregion

	#region SOFinPeriodAttribute
	public class SOFinPeriodAttribute : OpenPeriodAttribute
	{
		public SOFinPeriodAttribute()
			: this(null)
		{
		}

		public SOFinPeriodAttribute(Type SourceType)
			: this(SourceType, null)
		{
		}

		public SOFinPeriodAttribute(Type sourceType,
			Type branchSourceType,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			bool useMasterOrganizationIDByDefault = false)
			: base(
				typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.aRClosed, Equal<False>>>),
				sourceType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault)
		{
		}

		public static void DefaultFirstOpenPeriod(PXCache sender, string FieldName)
		{
			AROpenPeriodAttribute.DefaultFirstOpenPeriod(sender, FieldName);
		}

		public static void DefaultFirstOpenPeriod<Field>(PXCache sender)
			where Field : IBqlField
		{
			DefaultFirstOpenPeriod(sender, typeof(Field).Name);
		}

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult validationResult = new PeriodValidationResult();

			if (finPeriod.Status == FinPeriod.status.Closed || finPeriod.ARClosed == true)
			{
				validationResult = HandleErrorThatPeriodIsClosed(sender, finPeriod);

				return validationResult;
			}

			if (finPeriod.Status == FinPeriod.status.Locked)
			{
				validationResult.AddMessage(
					PXErrorLevel.Warning,
					ExceptionType.Locked,
					GL.Messages.FinPeriodIsLockedInCompany,
					FormatForError(finPeriod.FinPeriodID),
					PXAccess.GetOrganizationCD(finPeriod.OrganizationID));
				
				return validationResult;
			}

			if (finPeriod.Status == FinPeriod.status.Inactive)
			{
				validationResult.AddMessage(
					PXErrorLevel.Warning,
					ExceptionType.Inactive,
					GL.Messages.FinPeriodIsInactiveInCompany,
					FormatForError(finPeriod.FinPeriodID),
					PXAccess.GetOrganizationCD(finPeriod.OrganizationID));

				return validationResult;
			}

			return validationResult;
		}
	}
	#endregion

	public class SOCashSaleCashTranIDAttribute : CashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			SOInvoice soinvoice = (SOInvoice)orig_Row;
			if (soinvoice.Released == true || soinvoice.CuryPaymentAmt == null || soinvoice.CuryPaymentAmt == 0m)
			{
				return null;
			}

			catran_Row.OrigModule = BatchModule.AR;
			catran_Row.OrigTranType = soinvoice.DocType;
			catran_Row.OrigRefNbr = soinvoice.RefNbr;
			catran_Row.ExtRefNbr = soinvoice.ExtRefNbr;
			catran_Row.CashAccountID = soinvoice.CashAccountID;
			catran_Row.CuryInfoID = soinvoice.CuryInfoID;
			catran_Row.CuryID = soinvoice.CuryID;

			switch (soinvoice.DocType)
			{
				case ARDocType.CashSale:
					catran_Row.CuryTranAmt = soinvoice.CuryPaymentAmt;
					catran_Row.DrCr = DrCr.Debit;
					break;
				case ARDocType.CashReturn:
					catran_Row.CuryTranAmt = -soinvoice.CuryPaymentAmt;
					catran_Row.DrCr = DrCr.Credit;
					break;
				default:
					return null;
			}

			catran_Row.TranDate = soinvoice.AdjDate;
			catran_Row.TranDesc = string.Empty;
		    SetPeriodsByMaster(sender, catran_Row, soinvoice.AdjTranPeriodID);
            catran_Row.ReferenceID = soinvoice.CustomerID;
			catran_Row.Released = false;
			catran_Row.Hold = soinvoice.Hold;
			catran_Row.Cleared = soinvoice.Cleared;
			catran_Row.ClearDate = soinvoice.ClearDate;

			return catran_Row;
		}
	}

	/// <summary>
	/// Extends <see cref="LocationAvailAttribute"/> and shows the availability of Inventory Item for the given location.
	/// </summary>
	/// <example>
	/// [SOLocationAvail(typeof(SOLine.inventoryID), typeof(SOLine.subItemID), typeof(SOLine.siteID), typeof(SOLine.tranType), typeof(SOLine.invtMult))]
	/// </example>
	public class SOLocationAvailAttribute : LocationAvailAttribute
	{ 
		public SOLocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type TranType, Type InvtMultType)
			: base(InventoryType, SubItemType, SiteIDType, null, null, null)
		{
			_IsSalesType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.issue>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.invoice>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.debitMemo>)
				);

			_IsReceiptType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.receipt>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.return_>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.creditMemo>)
				);

			_IsTransferType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.transfer>),
				typeof(And<,,>),
				InvtMultType,
				typeof(Equal<short1>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.transfer>),
				typeof(And<,>),
				InvtMultType,
				typeof(Equal<shortMinus1>)
				);

			_IsStandardCostAdjType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.standardCostAdjustment>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.negativeCostAdjustment>)
				);

			this._Attributes.Add(new PrimaryItemRestrictorAttribute(InventoryType, _IsReceiptType, _IsSalesType, _IsTransferType));
			this._Attributes.Add(new LocationRestrictorAttribute(_IsReceiptType, _IsSalesType, _IsTransferType));
		}
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOShipLine parentLine = PXParentAttribute.SelectParent(sender, e.Row, typeof (SOShipLine)) as SOShipLine;
			if (parentLine != null && parentLine.IsUnassigned == true)
				return;
			base.FieldDefaulting(sender, e);
		}
	}

	/// <summary>
	/// Specialized for SOLine version of the CrossItemAttribute.<br/> 
	/// Providing an Inventory ID selector for the field, it allows also user <br/>
	/// to select both InventoryID and SubItemID by typing AlternateID in the control<br/>
	/// As a result, if user type a correct Alternate id, values for InventoryID, SubItemID, <br/>
	/// and AlternateID fields in the row will be set.<br/>
	/// In this attribute, InventoryItems with a status inactive, markedForDeletion,<br/>
	/// noSale and noRequest are filtered out. It also fixes  INPrimaryAlternateType parameter to CPN <br/>    
	/// This attribute may be used in combination with AlternativeItemAttribute on the AlternateID field of the row <br/>
	/// <example>
	/// [SOLineInventoryItem(Filterable = true)]
	/// </example>
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>,
							Or<Where<Current<SOOrderType.behavior>, Equal<SOOrderTypeConstants.salesOrder>,
								And<Current<SOOrderType.iNDocType>, Equal<INTranType.transfer>>>>>), IN.Messages.ItemCannotSale)]
	public class SOLineInventoryItemAttribute : CrossItemAttribute
	{

		/// <summary>
		/// Default ctor
		/// </summary>
		public SOLineInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), INPrimaryAlternateType.CPN)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method , AllowMultiple = false)]
	public class SOShipmentEntryActionsAttribute : PXIntListAttribute
	{
		public const int ConfirmShipment = 1;
		public const int CreateInvoice = 2;
		public const int PostInvoiceToIN = 3;
		public const int ApplyAssignmentRules = 4;
		public const int CorrectShipment = 5;
		public const int CreateDropshipInvoice = 6;
		public const int PrintLabels = 7;
		public const int GetReturnLabels = 8;
		public const int CancelReturn = 9;
		public const int PrintPickList = 10;

		public SOShipmentEntryActionsAttribute() : base(
			new[]
			{
				Pair(ConfirmShipment, Messages.ConfirmShipment),
				Pair(CreateInvoice, Messages.CreateInvoice),
				Pair(PostInvoiceToIN, Messages.PostInvoiceToIN),
				Pair(ApplyAssignmentRules, Messages.ApplyAssignmentRules),
				Pair(CorrectShipment, Messages.CorrectShipment),
				Pair(CreateDropshipInvoice, Messages.CreateDropshipInvoice),
				Pair(PrintLabels, Messages.PrintLabels),
				Pair(GetReturnLabels, Messages.GetReturnLabels),
				Pair(CancelReturn, Messages.CancelReturn),
				Pair(PrintPickList, Messages.PrintPickList),
			}) {}
		
		
		[PXLocalizable]
		public class Messages
		{
			public const string ConfirmShipment = "Confirm Shipment";
			public const string CreateInvoice = "Create Invoice";
			public const string PostInvoiceToIN = "Post Invoice to IN";
			public const string ApplyAssignmentRules = "Apply Assignment Rules";
			public const string CorrectShipment = "Correct Shipment";
			public const string CreateDropshipInvoice = "Create Drop-Ship Invoice";
			public const string PrintLabels = "Print Labels";
			public const string GetReturnLabels = "Get Return Labels";
			public const string CancelReturn = "Cancel Return";
			public const string PrintPickList = "Print Pick List";
			public const string PrintShipmentConfirmation = "Print Shipment Confirmation";
		}
	}

	public class SOReports : PX.SM.ReportUtils
	{
		public const string PrintLabels = "SO645000";
		public const string PrintPickList = "SO644000";
		public const string PrintShipmentConfirmation = "SO642000";
		public const string PrintInvoiceReport = "SO643000";
		public const string PrintSalesOrder = "SO641010";
		public const string PrintPackSlipBatch = "SO644005";
		public const string PrintPackSlipWave = "SO644007";
		public const string PrintPickerPickList = "SO644006";

		public static string GetReportID(int? actionID, string actionName)
		{
			if (actionID != null)
			{
				switch (actionID)
				{
					case SOShipmentEntryActionsAttribute.PrintLabels:
						return PrintLabels;
					case SOShipmentEntryActionsAttribute.PrintPickList:
						return PrintPickList;
				}
			}
			if (actionName != null)
			{
				switch (actionName)
				{
					case SOShipmentEntryActionsAttribute.Messages.PrintLabels:
						return PrintLabels;
					case SOShipmentEntryActionsAttribute.Messages.PrintPickList:
						return PrintPickList;
					case SOShipmentEntryActionsAttribute.Messages.PrintShipmentConfirmation:
						return PrintShipmentConfirmation;
				}
			}
			return null;
		}

	}

	/// <summary>
	/// PXSelector Marker to be used in PXFormula when a field cannot be decorated with PXSelector Attribute (Example: A field is already defined with StringList attribute to render a Combo box).
	/// </summary>
	public class PXSelectorMarkerAttribute : PXSelectorAttribute
	{
		public PXSelectorMarkerAttribute(Type type) : base(type)
		{
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber) || typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				//do nothing.
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}
	}
}
