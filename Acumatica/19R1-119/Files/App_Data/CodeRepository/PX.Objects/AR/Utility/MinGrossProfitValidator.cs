using System;
using PX.Common;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.SO;

namespace PX.Objects.AR
{
	[PXLocalizable]
	public static class MinGrossProfitMsg
	{
		public const string ValidationFailed = "Minimum Gross Profit requirement is not satisfied.";
		public const string ValidationFailedAndDiscountFixed = "Minimum Gross Profit requirement is not satisfied. Discount was reduced to maximum valid value.";
		public const string ValidationFailedAndSalesPriceFixed = "Minimum Gross Profit requirement is not satisfied. Sales price was set to minimum valid value.";
		public const string ValidationFailedAndUnitPriceFixed = "Minimum Gross Profit requirement is not satisfied. Unit price was set to minimum valid value.";
		public const string ValidationFailedNoCost = "No Average or Last cost found to determine minimum valid Unit price.";
	}

	public static class MinGrossProfitValidator
	{
		public static Decimal? Validate<TField>(PXCache sender, object line, InventoryItem inItem, String validationMode, Decimal? currentValue, Decimal? minValue, Decimal? newValue, Decimal? setToMinValue, Target target)
			where TField : IBqlField
		{
			if (currentValue < minValue)
			{
				switch (validationMode)
				{
					case MinGrossProfitValidationType.Warning:
						sender.RaiseExceptionHandling(typeof(TField).Name, line, newValue, new PXSetPropertyException(MinGrossProfitMsg.ValidationFailed, PXErrorLevel.Warning));
						break;
					case MinGrossProfitValidationType.SetToMin:
						newValue = setToMinValue;
						sender.RaiseExceptionHandling(typeof(TField).Name, line, newValue, new PXSetPropertyException(target.ToFixWarning(), PXErrorLevel.Warning));
						break;
				}
			}
			else if (validationMode != MinGrossProfitValidationType.None && minValue == 0m && inItem.ValMethod != INValMethod.Standard)
			{
				sender.RaiseExceptionHandling(typeof(TField).Name, line, newValue, new PXSetPropertyException(MinGrossProfitMsg.ValidationFailedNoCost, PXErrorLevel.Warning));
			}

			return newValue;
		}

		public enum Target
		{
			Discount,
			SalesPrice,
			UnitPrice
		}
	}

	public static class MinGrossProfitValidator<TLine>
		where TLine : class, IBqlTable, IHasMinGrossProfit, new()
	{
		private abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		private abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
		private abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }

		public static decimal CalculateMinPrice<InfoKeyField, inventoryIDField, uOMField>(PXCache sender, TLine line, InventoryItem inventoryItem, INItemCost inItemCost)
			where InfoKeyField : IBqlField
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			decimal minPrice = CalculateMinPrice<inventoryIDField, uOMField>(sender, line, inventoryItem, inItemCost);

			if (sender.GetValue<InfoKeyField>(line) != null)
			{
				try
				{
					PXDBCurrencyAttribute.CuryConvCury<InfoKeyField>(sender, line, minPrice, out minPrice, true);
				}
				catch (PXRateNotFoundException)
				{
					return minPrice;
				}
			}

			return minPrice;
		}

		public static decimal CalculateMinPrice<inventoryIDField, uOMField>(PXCache sender, TLine line, InventoryItem inventoryItem, INItemCost inItemCost)
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			if (line == null || inventoryItem == null || inItemCost == null) return decimal.Zero;
			decimal minPrice = INUnitAttribute.ConvertToBase<inventoryIDField, uOMField>(sender, line, PXPriceCostAttribute.MinPrice(inventoryItem, inItemCost), INPrecision.UNITCOST);
			return minPrice;
		}

		public static decimal? ValidateUnitPrice<InfoKeyField, inventoryIDField, uOMField>(PXCache sender, TLine line, decimal? curyNewUnitPrice)
			where InfoKeyField : IBqlField
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			if (sender.Graph.UnattendedMode)
				return curyNewUnitPrice;

			SOSetup sosetup = SOSetupSelect.Select(sender.Graph);

			if (sosetup.MinGrossProfitValidation == MinGrossProfitValidationType.None)
				return curyNewUnitPrice;

			if (line != null && line.InventoryID != null && line.UOM != null && curyNewUnitPrice >= 0 && line.IsFree != true)
			{
				var r = (PXResult<InventoryItem, INItemCost>)
					PXSelectJoin<InventoryItem,
					LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
					Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>
					.Select(sender.Graph, line.InventoryID);
				InventoryItem item = r;
				INItemCost cost = r;

				curyNewUnitPrice = ValidateUnitPrice<InfoKeyField, inventoryIDField, uOMField>(sender, line, item, cost, curyNewUnitPrice, sosetup.MinGrossProfitValidation);
			}

			return curyNewUnitPrice;
		}
		public static decimal? ValidateUnitPrice<InfoKeyField, inventoryIDField, uOMField>(PXCache sender, TLine line, InventoryItem inItem, INItemCost inItemCost, decimal? curyNewUnitPrice, string MinGrossProfitValidation)
			where InfoKeyField : IBqlField
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			if (inItem == null || inItemCost == null) return curyNewUnitPrice;
			decimal minPrice = CalculateMinPrice<InfoKeyField, inventoryIDField, uOMField>(sender, line, inItem, inItemCost);
			return MinGrossProfitValidator.Validate<curyUnitPrice>(sender, line, inItem, MinGrossProfitValidation, curyNewUnitPrice, minPrice, curyNewUnitPrice, minPrice, MinGrossProfitValidator.Target.UnitPrice);
		}

		/// <summary>
		/// Validates Discount %. The Unit Price with discount for an item cannot be less then StdCost plus Minimal Gross Profit.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="line">Target row</param>
		/// <param name="unitPrice">UnitPrice in base currency</param>
		/// <param name="newDiscPct">new Discount %</param>
		public static decimal? ValidateDiscountPct<inventoryIDField, uOMField>(PXCache sender, TLine line, decimal? unitPrice, decimal? newDiscPct)
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			if (sender.Graph.UnattendedMode)
				return newDiscPct;

			SOSetup sosetup = SOSetupSelect.Select(sender.Graph);

			if (sosetup.MinGrossProfitValidation == MinGrossProfitValidationType.None)
				return newDiscPct;

			if (line != null && unitPrice > 0 && newDiscPct > 0 && line.IsFree != true)
			{
				var r = (PXResult<InventoryItem, INItemCost>)
					PXSelectJoin<InventoryItem,
					LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
					Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>
					.Select(sender.Graph, line.InventoryID);
				InventoryItem item = r;
				INItemCost cost = r;

				newDiscPct = ValidateDiscountPct<inventoryIDField, uOMField>(sender, line, item, cost, unitPrice, newDiscPct, sosetup.MinGrossProfitValidation);
			}

			return newDiscPct;
		}
		public static decimal? ValidateDiscountPct<inventoryIDField, uOMField>(PXCache sender, TLine line, InventoryItem inItem, INItemCost inItemCost, decimal? unitPrice, decimal? newDiscPct, string MinGrossProfitValidation)
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			if (inItem == null || inItemCost == null) return newDiscPct;
			decimal minPrice = CalculateMinPrice<inventoryIDField, uOMField>(sender, line, inItem, inItemCost);
			decimal? unitPriceAfterDiscount = unitPrice - (newDiscPct * unitPrice * 0.01m);
			decimal? maxDiscPct = (unitPrice == null || unitPrice == decimal.Zero) ? decimal.Zero : (unitPrice - minPrice) * 100 / unitPrice;
			return MinGrossProfitValidator.Validate<discPct>(sender, line, inItem, MinGrossProfitValidation, unitPriceAfterDiscount, minPrice, newDiscPct, maxDiscPct, MinGrossProfitValidator.Target.Discount);
		}

		/// <summary>
		/// Validates Discount Amount. The Unit Price with discount for an item cannot be less then StdCost plus Minimal Gross Profit.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="line">Target row</param>
		/// <param name="unitPrice">UnitPrice in base currency</param>
		/// <param name="newDiscPct">new Discount amount</param>
		public static decimal? ValidateDiscountAmt<inventoryIDField, uOMField>(PXCache sender, TLine line, decimal? unitPrice, decimal? newDisc)
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			if (sender.Graph.UnattendedMode)
				return newDisc;

			SOSetup sosetup = SOSetupSelect.Select(sender.Graph);

			if (sosetup.MinGrossProfitValidation == MinGrossProfitValidationType.None)
				return newDisc;

			if (line != null && Math.Abs(line.Qty.GetValueOrDefault()) > 0 && newDisc > 0 && unitPrice > 0 && line.IsFree != true)
			{
				var r = (PXResult<InventoryItem, INItemCost>)
					PXSelectJoin<InventoryItem,
					LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
					Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>
					.Select(sender.Graph, line.InventoryID);
				InventoryItem item = r;
				INItemCost cost = r;

				newDisc = ValidateDiscountAmt<inventoryIDField, uOMField>(sender, line, item, cost, unitPrice, newDisc, sosetup.MinGrossProfitValidation);
			}

			return newDisc;
		}
		public static decimal? ValidateDiscountAmt<inventoryIDField, uOMField>(PXCache sender, TLine line, InventoryItem inItem, INItemCost inItemCost, decimal? unitPrice, decimal? newDisc, string MinGrossProfitValidation)
			where inventoryIDField : IBqlField
			where uOMField : IBqlField
		{
			if (inItem == null || inItemCost == null) return newDisc;
			decimal minPrice = CalculateMinPrice<inventoryIDField, uOMField>(sender, line, inItem, inItemCost);
			decimal? unitPriceAfterDiscount = unitPrice - newDisc / Math.Abs(line.Qty.Value);
			decimal? maxDisc = Math.Abs(line.Qty.Value) * (unitPrice - minPrice);
			return MinGrossProfitValidator.Validate<curyDiscAmt>(sender, line, inItem, MinGrossProfitValidation, unitPriceAfterDiscount, minPrice, newDisc, maxDisc, MinGrossProfitValidator.Target.Discount);
		}
	}

	internal static class MinGrossProfitTargetExt
	{
		public static string ToFixWarning(this MinGrossProfitValidator.Target target)
		{
			switch (target)
			{
				case MinGrossProfitValidator.Target.Discount:
					return MinGrossProfitMsg.ValidationFailedAndDiscountFixed;
				case MinGrossProfitValidator.Target.SalesPrice:
					return MinGrossProfitMsg.ValidationFailedAndSalesPriceFixed;
				case MinGrossProfitValidator.Target.UnitPrice:
					return MinGrossProfitMsg.ValidationFailedAndUnitPriceFixed;
				default:
					throw new ArgumentOutOfRangeException(nameof(target), target, null);
			}
		}
	}

	public interface IHasMinGrossProfit
	{
		int? InventoryID { get; }
		string UOM { get; }
		bool? IsFree { get; }
		decimal? Qty { get; }
	}
}