using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.Extensions.Discount;

namespace PX.Objects.Common.Discount.Mappers
{
	public abstract class AmountLineFields : DiscountedLineMapperBase
	{
		public bool HaveBaseQuantity { get; set; }
		public virtual decimal? Quantity { get; set; }
		public virtual decimal? CuryUnitPrice { get; set; }
		public virtual decimal? CuryExtPrice { get; set; }
		public virtual decimal? CuryLineAmount { get; set; }
		public virtual string UOM { get; set; }
		public virtual decimal? OrigGroupDiscountRate { get; set; }
		public virtual decimal? OrigDocumentDiscountRate { get; set; }
		public virtual decimal? GroupDiscountRate { get; set; }
		public virtual decimal? DocumentDiscountRate { get; set; }
		public virtual string TaxCategoryID { get; set; }
		public virtual bool? FreezeManualDisc { get; set; }

		public abstract class quantity : PX.Data.BQL.BqlDecimal.Field<quantity> { }
		private abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		private abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		private abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }
		public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }
		private abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }
		public abstract class curyLineAmount : PX.Data.BQL.BqlDecimal.Field<curyLineAmount> { }
		private abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		public abstract class origGroupDiscountRate : PX.Data.BQL.BqlDecimal.Field<origGroupDiscountRate> { }
		public abstract class origDocumentDiscountRate : PX.Data.BQL.BqlDecimal.Field<origDocumentDiscountRate> { }
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
		public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		public abstract class freezeManualDisc : PX.Data.BQL.BqlBool.Field<freezeManualDisc> { }

		public string ExtPriceDisplayName => GetDisplayName<curyExtPrice>();

		protected AmountLineFields(PXCache cache, object row) : base(cache, row) { }

		private object GetState<T>() where T : IBqlField => Cache.GetStateExt(MappedLine, Cache.GetField(GetField<T>()));

		private string GetDisplayName<T>() where T : IBqlField
		{
			PXFieldState state = (PXFieldState)GetState<T>();
			return state.DisplayName;
		}

		/// <summary>
		/// Get map to amount line fields
		/// </summary>
		/// <remarks>
		/// QuantityField: Quantity
		/// CuryUnitPriceField: Cury Unit Price
		/// CuryExtPriceField: Quantity * Cury Unit Price field
		/// CuryLineAmountField: (Quantity * Cury Unit Price field) - Cury Discount Amount field
		///</remarks>
		public static AmountLineFields GetMapFor<TLine>(TLine line, PXCache cache)
			where TLine : class, IBqlTable
			=> GetMapFor(line, cache, null);

		internal static AmountLineFields GetMapFor<TLine>(TLine line, PXCache cache, bool? applyQuantityDiscountByBaseUOM)
			where TLine : class, IBqlTable
		{
			Type lineType = line?.GetType() ?? typeof(TLine);
			return GetMapFor(line, cache, lineType, applyQuantityDiscountByBaseUOM);
		}

		internal static AmountLineFields GetMapFor(object line, PXCache cache)
		{
			return GetMapFor(line, cache, line.GetType());
		}

		private static AmountLineFields GetMapFor(object line, PXCache cache, Type lineType, bool? applyQuantityDiscountByBaseUOM = null)
		{
			if (lineType == typeof(ARTran))
			{
				return applyQuantityDiscountByBaseUOM ?? DiscountEngine.ApplyQuantityDiscountByBaseUOMForAR(cache.Graph)
					? (AmountLineFields)new RetainedAmountLineFields<ARTran.baseQty, ARTran.curyUnitPrice, ARTran.curyExtPrice, ARTran.curyTranAmt, ARTran.curyRetainageAmt, ARTran.uOM, ARTran.origGroupDiscountRate, ARTran.origDocumentDiscountRate, ARTran.groupDiscountRate, ARTran.documentDiscountRate, ARTran.freezeManualDisc>(cache, line) { HaveBaseQuantity = true }
					: (AmountLineFields)new RetainedAmountLineFields<ARTran.qty, ARTran.curyUnitPrice, ARTran.curyExtPrice, ARTran.curyTranAmt, ARTran.curyRetainageAmt, ARTran.uOM, ARTran.origGroupDiscountRate, ARTran.origDocumentDiscountRate, ARTran.groupDiscountRate, ARTran.documentDiscountRate, ARTran.freezeManualDisc>(cache, line);
			}
			if (lineType == typeof(SOLine))
			{
				return applyQuantityDiscountByBaseUOM ?? DiscountEngine.ApplyQuantityDiscountByBaseUOMForAR(cache.Graph)
					? (AmountLineFields)new AmountLineFields<SOLine.baseOrderQty, SOLine.curyUnitPrice, SOLine.curyExtPrice, SOLine.curyLineAmt, SOLine.uOM, origGroupDiscountRate, origDocumentDiscountRate, SOLine.groupDiscountRate, SOLine.documentDiscountRate, SOLine.freezeManualDisc>(cache, line) { HaveBaseQuantity = true }
					: (AmountLineFields)new AmountLineFields<SOLine.orderQty, SOLine.curyUnitPrice, SOLine.curyExtPrice, SOLine.curyLineAmt, SOLine.uOM, origGroupDiscountRate, origDocumentDiscountRate, SOLine.groupDiscountRate, SOLine.documentDiscountRate, SOLine.freezeManualDisc>(cache, line);
			}
			if (lineType == typeof(SOShipLine))
			{
				return applyQuantityDiscountByBaseUOM ?? DiscountEngine.ApplyQuantityDiscountByBaseUOMForAR(cache.Graph)
					? (AmountLineFields)new AmountLineFields<SOShipLine.baseShippedQty, curyUnitCost, curyLineAmt, curyExtCost, SOShipLine.uOM, origGroupDiscountRate, origDocumentDiscountRate, groupDiscountRate, documentDiscountRate>(cache, line) { HaveBaseQuantity = true }
					: (AmountLineFields)new AmountLineFields<SOShipLine.shippedQty, curyUnitCost, curyLineAmt, curyExtCost, SOShipLine.uOM, origGroupDiscountRate, origDocumentDiscountRate, groupDiscountRate, documentDiscountRate>(cache, line);
			}
			if (lineType == typeof(APTran))
			{
				return applyQuantityDiscountByBaseUOM ?? DiscountEngine.ApplyQuantityDiscountByBaseUOMForAP(cache.Graph)
					? (AmountLineFields)new RetainedAmountLineFields<APTran.baseQty, APTran.curyUnitCost, APTran.curyLineAmt, APTran.curyTranAmt, APTran.curyRetainageAmt, APTran.uOM, APTran.origGroupDiscountRate, APTran.origDocumentDiscountRate, APTran.groupDiscountRate, APTran.documentDiscountRate, APTran.freezeManualDisc>(cache, line) { HaveBaseQuantity = true }
					: (AmountLineFields)new RetainedAmountLineFields<APTran.qty, APTran.curyUnitCost, APTran.curyLineAmt, APTran.curyTranAmt, APTran.curyRetainageAmt, APTran.uOM, APTran.origGroupDiscountRate, APTran.origDocumentDiscountRate, APTran.groupDiscountRate, APTran.documentDiscountRate, APTran.freezeManualDisc>(cache, line);
			}
			if (lineType == typeof(POLine))
			{
				return applyQuantityDiscountByBaseUOM ?? DiscountEngine.ApplyQuantityDiscountByBaseUOMForAP(cache.Graph)
					? (AmountLineFields)new RetainedAmountLineFields<POLine.baseOrderQty, POLine.curyUnitCost, POLine.curyLineAmt, POLine.curyExtCost, POLine.curyRetainageAmt, POLine.uOM, origGroupDiscountRate, origDocumentDiscountRate, POLine.groupDiscountRate, POLine.documentDiscountRate>(cache, line) { HaveBaseQuantity = true }
					: (AmountLineFields)new RetainedAmountLineFields<POLine.orderQty, POLine.curyUnitCost, POLine.curyLineAmt, POLine.curyExtCost, POLine.curyRetainageAmt, POLine.uOM, origGroupDiscountRate, origDocumentDiscountRate, POLine.groupDiscountRate, POLine.documentDiscountRate>(cache, line);
			}
			if (lineType == typeof(CROpportunityProducts))
			{
				return new AmountLineFields<CROpportunityProducts.quantity, CROpportunityProducts.curyUnitPrice, CROpportunityProducts.curyExtPrice, CROpportunityProducts.curyAmount, CROpportunityProducts.uOM, origGroupDiscountRate, origDocumentDiscountRate, CROpportunityProducts.groupDiscountRate, CROpportunityProducts.documentDiscountRate, freezeManualDisc>(cache, line);
			}
			if (lineType == typeof(Detail))
			{
				return new AmountLineFields<Detail.quantity, Detail.curyUnitPrice, Detail.curyExtPrice, Detail.curyLineAmount, Detail.uOM, Detail.origGroupDiscountRate, Detail.origDocumentDiscountRate, Detail.groupDiscountRate, Detail.documentDiscountRate, Detail.freezeManualDisc>(cache, line);
			}
			return applyQuantityDiscountByBaseUOM ?? DiscountEngine.ApplyQuantityDiscountByBaseUOMForAR(cache.Graph)
				? (AmountLineFields)new AmountLineFields<baseOrderQty, curyUnitPrice, curyExtPrice, curyLineAmt, uOM, origGroupDiscountRate, origDocumentDiscountRate, groupDiscountRate, documentDiscountRate, freezeManualDisc>(cache, line) { HaveBaseQuantity = true }
				: (AmountLineFields)new AmountLineFields<orderQty, curyUnitPrice, curyExtPrice, curyLineAmt, uOM, origGroupDiscountRate, origDocumentDiscountRate, groupDiscountRate, documentDiscountRate, freezeManualDisc>(cache, line);
		}
	}

	public class AmountLineFields<QuantityField, CuryUnitPriceField, CuryExtPriceField, CuryLineAmountField, UOMField, OrigGroupDiscountRateField, OrigDocumentDiscountRateField, GroupDiscountRateField, DocumentDiscountRateField>
		: AmountLineFields
		where QuantityField : IBqlField
		where CuryUnitPriceField : IBqlField
		where CuryExtPriceField : IBqlField
		where CuryLineAmountField : IBqlField
		where UOMField : IBqlField
		where OrigGroupDiscountRateField : IBqlField
		where OrigDocumentDiscountRateField : IBqlField
		where GroupDiscountRateField : IBqlField
		where DocumentDiscountRateField : IBqlField
	{
		public AmountLineFields(PXCache cache, object row) : base(cache, row) { }

		public override Type GetField<T>()
		{
			if (typeof(T) == typeof(quantity))
			{
				return typeof(QuantityField);
			}
			if (typeof(T) == typeof(curyUnitPrice))
			{
				return typeof(CuryUnitPriceField);
			}
			if (typeof(T) == typeof(curyExtPrice))
			{
				return typeof(CuryExtPriceField);
			}
			if (typeof(T) == typeof(curyLineAmount))
			{
				return typeof(CuryLineAmountField);
			}
			if (typeof(T) == typeof(uOM))
			{
				return typeof(UOMField);
			}
			if (typeof(T) == typeof(origGroupDiscountRate))
			{
				return typeof(GroupDiscountRateField);
			}
			if (typeof(T) == typeof(origDocumentDiscountRate))
			{
				return typeof(DocumentDiscountRateField);
			}
			if (typeof(T) == typeof(groupDiscountRate))
			{
				return typeof(GroupDiscountRateField);
			}
			if (typeof(T) == typeof(documentDiscountRate))
			{
				return typeof(DocumentDiscountRateField);
			}
			return null;
		}

		public override decimal? Quantity
		{
			get { return (decimal?) Cache.GetValue<QuantityField>(MappedLine); }
			set { Cache.SetValue<QuantityField>(MappedLine, value); }
		}

		public override decimal? CuryUnitPrice
		{
			get { return (decimal?) Cache.GetValue<CuryUnitPriceField>(MappedLine); }
			set { Cache.SetValue<CuryUnitPriceField>(MappedLine, value); }
		}

		public override decimal? CuryExtPrice
		{
			get { return (decimal?) Cache.GetValue<CuryExtPriceField>(MappedLine); }
			set { Cache.SetValue<CuryExtPriceField>(MappedLine, value); }
		}

		public override decimal? CuryLineAmount
		{
			get { return (decimal?) Cache.GetValue<CuryLineAmountField>(MappedLine); }
			set { Cache.SetValue<CuryLineAmountField>(MappedLine, value); }
		}

		public override string UOM
		{
			get { return (string) Cache.GetValue<UOMField>(MappedLine); }
			set { Cache.SetValue<UOMField>(MappedLine, value); }
		}

		public override decimal? OrigGroupDiscountRate
		{
			get { return (decimal?)Cache.GetValue<OrigGroupDiscountRateField>(MappedLine); }
			set { Cache.SetValue<OrigGroupDiscountRateField>(MappedLine, value); }
		}

		public override decimal? OrigDocumentDiscountRate
		{
			get { return (decimal?)Cache.GetValue<OrigDocumentDiscountRateField>(MappedLine); }
			set { Cache.SetValue<OrigDocumentDiscountRateField>(MappedLine, value); }
		}

		public override decimal? GroupDiscountRate
		{
			get { return (decimal?) Cache.GetValue<GroupDiscountRateField>(MappedLine); }
			set { Cache.SetValue<GroupDiscountRateField>(MappedLine, value); }
		}

		public override decimal? DocumentDiscountRate
		{
			get { return (decimal?) Cache.GetValue<DocumentDiscountRateField>(MappedLine); }
			set { Cache.SetValue<DocumentDiscountRateField>(MappedLine, value); }
		}
	}

	public class AmountLineFields<QuantityField, CuryUnitPriceField, CuryExtPriceField, CuryLineAmountField, UOMField, OrigGroupDiscountRateField, OrigDocumentDiscountRateField, GroupDiscountRateField, DocumentDiscountRateField, FreezeManualDiscField>
		: AmountLineFields<QuantityField, CuryUnitPriceField, CuryExtPriceField, CuryLineAmountField, UOMField, OrigGroupDiscountRateField, OrigDocumentDiscountRateField, GroupDiscountRateField, DocumentDiscountRateField>
		where QuantityField : IBqlField
		where CuryUnitPriceField : IBqlField
		where CuryExtPriceField : IBqlField
		where CuryLineAmountField : IBqlField
		where UOMField : IBqlField
		where OrigGroupDiscountRateField : IBqlField
		where OrigDocumentDiscountRateField : IBqlField
		where GroupDiscountRateField : IBqlField
		where DocumentDiscountRateField : IBqlField
		where FreezeManualDiscField : IBqlField
	{
		public AmountLineFields(PXCache cache, object row) : base(cache, row) { }

		public override Type GetField<T>()
		{
			if (typeof(T) == typeof(freezeManualDisc))
			{
				return typeof(FreezeManualDiscField);
			}
			else
			{
				return base.GetField<T>();
			}
		}

		public override bool? FreezeManualDisc
		{
			get { return (bool?)Cache.GetValue<FreezeManualDiscField>(MappedLine); }
			set { Cache.SetValue<FreezeManualDiscField>(MappedLine, value); }
		}
	}

	#region RetainedAmountLineFields
	/// <summary>
	/// A specialized for retainage version of the <see cref="AmountLineFields"/> class
	/// with an additional CuryRetainageAmtField generic parameter.
	/// Discount fields should not be recalculated after retainage fields changing
	/// because they have a higher priority.
	/// </summary>
	public class RetainedAmountLineFields<QuantityField, CuryUnitPriceField, CuryExtPriceField, CuryLineAmountField, CuryRetainageAmtField, UOMField, OrigGroupDiscountRateField, OrigDocumentDiscountRateField, GroupDiscountRateField, DocumentDiscountRateField, FreezeManualDiscField>
		: AmountLineFields<QuantityField, CuryUnitPriceField, CuryExtPriceField, CuryLineAmountField, UOMField, OrigGroupDiscountRateField, OrigDocumentDiscountRateField, GroupDiscountRateField, DocumentDiscountRateField, FreezeManualDiscField>

		where QuantityField : IBqlField
		where CuryUnitPriceField : IBqlField
		where CuryExtPriceField : IBqlField
		where CuryLineAmountField : IBqlField
		where CuryRetainageAmtField : IBqlField
		where UOMField : IBqlField
		where OrigGroupDiscountRateField : IBqlField
		where OrigDocumentDiscountRateField : IBqlField
		where GroupDiscountRateField : IBqlField
		where DocumentDiscountRateField : IBqlField
		where FreezeManualDiscField : IBqlField
	{
		public RetainedAmountLineFields(PXCache cache, object row)
			: base(cache, row)
		{
		}

		public override decimal? CuryLineAmount
		{
			get
			{
				return base.CuryLineAmount +
						(decimal)(Cache.GetValue<CuryRetainageAmtField>(MappedLine) ?? 0m);
			}
			set
			{
				base.CuryLineAmount = value;
			}
		}
	}

	/// <summary>
	/// A specialized for retainage version of the <see cref="AmountLineFields"/> class
	/// with an additional CuryRetainageAmtField generic parameter.
	/// Discount fields should not be recalculated after retainage fields changing
	/// because they have a higher priority.
	/// </summary>
	public class RetainedAmountLineFields<QuantityField, CuryUnitPriceField, CuryExtPriceField, CuryLineAmountField, CuryRetainageAmtField, UOMField, OrigGroupDiscountRateField, OrigDocumentDiscountRateField, GroupDiscountRateField, DocumentDiscountRateField>
		: AmountLineFields<QuantityField, CuryUnitPriceField, CuryExtPriceField, CuryLineAmountField, UOMField, OrigGroupDiscountRateField, OrigDocumentDiscountRateField, GroupDiscountRateField, DocumentDiscountRateField>

		where QuantityField : IBqlField
		where CuryUnitPriceField : IBqlField
		where CuryExtPriceField : IBqlField
		where CuryLineAmountField : IBqlField
		where CuryRetainageAmtField : IBqlField
		where UOMField : IBqlField
		where OrigGroupDiscountRateField : IBqlField
		where OrigDocumentDiscountRateField : IBqlField
		where GroupDiscountRateField : IBqlField
		where DocumentDiscountRateField : IBqlField
	{
		public RetainedAmountLineFields(PXCache cache, object row)
			: base(cache, row)
		{
		}

		public override decimal? CuryLineAmount
		{
			get
			{
				return base.CuryLineAmount +
						(decimal)(Cache.GetValue<CuryRetainageAmtField>(MappedLine) ?? 0m);
			}
			set
			{
				base.CuryLineAmount = value;
			}
		}
	}

	#endregion

}