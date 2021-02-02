using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using PX.Data;

using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.SO;
using PX.Common;
using PX.Common.Collection;
using PX.Data.BQL;
using PX.Objects.Common.Discount.Mappers;
using PX.Objects.Common.Extensions;

namespace PX.Objects.Common.Discount
{
	public class DiscountEngine<TLine, TDiscountDetail> : DiscountEngine
		where TLine : class, IBqlTable, new()
		where TDiscountDetail : class, IBqlTable, IDiscountDetail, new()
	{
		protected virtual bool IsDiscountFeatureEnabled(DiscountCalculationOptions calculationOptions = DiscountCalculationOptions.CalculateAll)
		{
			if (calculationOptions.HasFlag(DiscountCalculationOptions.DisableAPDiscountsCalculation) &&
				calculationOptions.HasFlag(DiscountCalculationOptions.DisableARDiscountsCalculation))
				return false;

			if (calculationOptions.HasFlag(DiscountCalculationOptions.DisableAPDiscountsCalculation))
			{
				return PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();
			}
			else if (calculationOptions.HasFlag(DiscountCalculationOptions.DisableARDiscountsCalculation))
			{
				return PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>();
			}
			return true;
		}

		#region Set Line/Group/Document discounts functions
		/// <summary>
		/// Sets best available discount for a given line. Recalculates all group discounts. Sets best available document discount.
		/// </summary>
		/// <typeparam name="TDiscountDetail">DiscountDetails table</typeparam>
		/// <param name="lines">Transaction lines</param>
		/// <param name="line">Current line</param>
		/// <param name="discountDetails">Discount Details</param>
		/// <param name="locationID">Customer or Vendor LocationID</param>
		/// <param name="date">Date</param>
		public virtual void SetDiscounts(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine line,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? branchID,
			int? locationID,
			string curyID,
			DateTime? date,
			RecalcDiscountsParamFilter recalcFilter = null,
			DiscountCalculationOptions discountCalculationOptions = DefaultDiscountCalculationParameters)
		{
			if (branchID == null || locationID == null || date == null || !IsDiscountFeatureEnabled(discountCalculationOptions))
				return;

			UpdateEntityCache();

			DiscountLineFields dLine = DiscountLineFields.GetMapFor(line, cache);
			
			if (IsDiscountCalculationNeeded(cache, line, DiscountType.Line) && ((!cache.Graph.IsImport || discountCalculationOptions.HasFlag(DiscountCalculationOptions.CalculateDiscountsFromImport)) || cache.Graph.IsMobile))
			{
				if (!GetUnitPrice(cache, line, locationID, curyID, date.Value).skipLineDiscount)
				{
					SetLineDiscount(cache, GetDiscountEntitiesDiscounts(cache, line, locationID, true), line, date.Value, discountCalculationOptions, recalcFilter);
				}
				else if (dLine.ManualDisc != true) //clear line discount if customer-specific price found
				{
						ClearLineDiscount(cache, line, dLine);
				}
			}

			if (!discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisableGroupAndDocumentDiscounts) && 
			    ((!cache.Graph.IsImport || discountCalculationOptions.HasFlag(DiscountCalculationOptions.CalculateDiscountsFromImport)) || cache.Graph.IsMobile)
			    && dLine.SkipDisc != true)
			{
				RecalculateGroupAndDocumentDiscounts(cache, lines, line, discountDetails, branchID, locationID, date, discountCalculationOptions, recalcFilter);
			}
		}

		protected override void SetLineDiscountOnlyImpl(
			PXCache cache,
			object line,
			DiscountLineFields dLine,
			string discountID,
			decimal? unitPrice,
			decimal? extPrice,
			decimal? qty,
			int? locationID,
			int? customerID,
			string curyID,
			DateTime? date,
			int? branchID,
			int? inventoryID,
			bool needDiscountID)
		{
			if (locationID == null || date == null || !IsDiscountFeatureEnabled())
				return;

			UpdateEntityCache();

			var tLine = (TLine)line;

			if (!IsDiscountCalculationNeeded(cache, tLine, DiscountType.Line) || GetUnitPrice(cache, tLine, locationID, curyID, date.Value).skipLineDiscount)
				return;

			HashSet<KeyValuePair<object, string>> entities = GetDiscountEntitiesDiscounts(cache, tLine, locationID, true, branchID, inventoryID, customerID);

			GetDiscountTypes();

			if (extPrice == null || qty == null)
				return;

			HashSet<DiscountSequenceKey> discountSequencesByDiscountID = SelectDiscountSequences(discountID);
			HashSet<DiscountSequenceKey> applicableDiscountSequences = SelectApplicableEntityDiscounts(entities, DiscountType.Line, !needDiscountID);

			if (needDiscountID)
				applicableDiscountSequences.IntersectWith(discountSequencesByDiscountID);

			decimal discountTargetPrice;
			if (GetLineDiscountTarget(cache, tLine) == AR.LineDiscountTargetType.SalesPrice)
				discountTargetPrice = (decimal)unitPrice;
			else
				discountTargetPrice = (decimal)extPrice;

			DiscountDetailLine discount = SelectApplicableDiscount(cache, dLine, applicableDiscountSequences, discountTargetPrice, (decimal)qty, DiscountType.Line, date.Value);

			if (discount.DiscountID != null)
			{
				decimal curyDiscountAmt = CalculateDiscount(cache, discount, dLine, discountTargetPrice, (decimal)qty, date.Value, DiscountType.Line);

				DiscountResult dResult = new DiscountResult(discount.DiscountedFor == DiscountOption.Amount ? curyDiscountAmt : discount.Discount, discount.DiscountedFor == DiscountOption.Amount);

				ApplyDiscountToLine(cache, tLine, (decimal)qty, (decimal)unitPrice, (decimal)extPrice, dLine, dResult, 1);
				dLine.DiscountID = discount.DiscountID;
				dLine.DiscountSequenceID = discount.DiscountSequenceID;
				dLine.RaiseFieldUpdated<DiscountLineFields.discountSequenceID>(null);
			}
			else
			{
				ClearLineDiscount(cache, tLine, dLine);
			}
		}

		/// <summary>
		/// Recalculate Prices and Discounts action. Recalculates line discounts. Recalculates all group discounts. Sets best available document discount.
		/// </summary>
		/// <typeparam name="TDiscountDetail">DiscountDetails table</typeparam>
		/// <param name="recalcFilter">Current RecalcDiscountsParamFilter</param>
		public virtual void RecalculatePricesAndDiscounts(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? locationID,
			DateTime? date,
			RecalcDiscountsParamFilter recalcFilter,
			DiscountCalculationOptions discountCalculationOptions)
		{
			try
			{
				UpdateEntityCache();
				recalcFilter.UseRecalcFilter = true;

				if (recalcFilter.RecalcDiscounts == true && recalcFilter.OverrideManualDocGroupDiscounts == true)
				{
					foreach (TDiscountDetail discountDetail in GetDiscountDetailsByType(cache, discountDetails, null))
					{
						if (discountDetail.Type != DiscountType.ExternalDocument)
							discountDetail.IsManual = false;
					}
				}

				if (recalcFilter.RecalcTarget == RecalcDiscountsParamFilter.AllLines)
				{
					List<TLine> documentDetails = GetDocumentDetails(cache, lines);
					for (int i = 0; i < documentDetails.Count(); i++)
					{
						//Group and Document discounts will be calculated for the whole set of lines.
						if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.EnableOptimizationOfGroupAndDocumentDiscountsCalculation) 
						    && i != documentDetails.Count() - 1)
						{
							DiscountLineFields dFields = DiscountLineFields.GetMapFor(documentDetails[i], cache);
							dFields.SkipDisc = true;
						}
							
						RecalculatePricesAndDiscountsOnLine(cache, documentDetails[i], recalcFilter, discountCalculationOptions);
					}
				}
				else if (currentLine != null)
				{
					RecalculatePricesAndDiscountsOnLine(cache, currentLine, recalcFilter, discountCalculationOptions);
				}
				else
					throw new PXException(AR.Messages.NoLineSelected);
			}
			finally
			{
				recalcFilter.UseRecalcFilter = false;
			}
		}

		/// <summary>
		/// Recalculate Prices and Discounts with predefined parameters
		/// </summary>
		/// <typeparam name="TDiscountDetail">DiscountDetails table</typeparam>
		public virtual void AutoRecalculatePricesAndDiscounts(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? locationID,
			DateTime? date,
			DiscountCalculationOptions discountCalculationOptions)
		{
			if (locationID == null || date == null)
				return;

			var recalcFilter = new RecalcDiscountsParamFilter
			{
				RecalcTarget = RecalcDiscountsParamFilter.AllLines,
				OverrideManualDiscounts = false,
				OverrideManualDocGroupDiscounts = false,
				OverrideManualPrices = false,
				RecalcDiscounts = IsDiscountFeatureEnabled(discountCalculationOptions),
				RecalcUnitPrices = !discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisablePriceCalculation)
			};
			try
			{
				IsInternalDiscountEngineCall = true;
				RecalculatePricesAndDiscounts(cache, lines, currentLine, discountDetails, locationID, date, recalcFilter, discountCalculationOptions);
			}
			finally
			{
				IsInternalDiscountEngineCall = false;
			}
		}

		protected virtual void RecalculatePricesAndDiscountsOnLine(PXCache cache, TLine line, RecalcDiscountsParamFilter recalcFilter, DiscountCalculationOptions discountCalculationOptions)
		{
			TLine oldLine = (TLine)cache.CreateCopy(line);
			DiscountLineFields dFields = DiscountLineFields.GetMapFor(line, cache);
			DiscountLineFields oldDFields = DiscountLineFields.GetMapFor(oldLine, cache);
			AmountLineFields lineAmountsFields = AmountLineFields.GetMapFor(line, cache);
			LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(line, cache);
			if ((bool)recalcFilter.OverrideManualPrices)
				dFields.ManualPrice = false;

			if (lineEntities.InventoryID != null
				&& dFields.ManualPrice != true
				&& (recalcFilter.RecalcUnitPrices == true
					|| (lineAmountsFields.CuryUnitPrice == 0m
						&& lineAmountsFields.CuryExtPrice == 0m
						&& recalcFilter.OverrideManualPrices == false)))
			{
				cache.RaiseFieldUpdated(lineAmountsFields.GetField<AmountLineFields.curyUnitPrice>().Name, line, 0m);
				cache.SetDefaultExt(line, lineAmountsFields.GetField<AmountLineFields.curyUnitPrice>().Name);
			}
			if ((bool)recalcFilter.RecalcDiscounts)
			{
				if (recalcFilter.OverrideManualDiscounts == true) dFields.ManualDisc = false;
				if (dFields.ManualDisc != true) dFields.DiscountID = null;
				oldDFields.DiscountID = string.Empty;
				if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.CalculateDiscountsFromImport))
					dFields.CalculateDiscountsOnImport = true;
			}
			cache.IsDirty = true;
			cache.RaiseRowUpdated(line, oldLine);
			cache.MarkUpdated(line);
		}

		/// <summary>
		/// Recalculates all group discounts and sets best available document discount
		/// </summary>
		/// <typeparam name="TDiscountDetail">DiscountDetail table</typeparam>
		public virtual void RecalculateGroupAndDocumentDiscounts(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? branchID,
			int? locationID,
			DateTime? date,
			DiscountCalculationOptions discountCalculationOptions,
			RecalcDiscountsParamFilter recalcFilter = null)
		{
			if (branchID == null || locationID == null || date == null || !IsDiscountFeatureEnabled(discountCalculationOptions))
				return;

			UpdateEntityCache();
			RemoveOrphanDiscountLines(cache, lines, discountDetails);
			SetGroupDiscounts(cache, lines, currentLine, discountDetails, locationID, date.Value, discountCalculationOptions, recalcFilter);
			SetDocumentDiscount(cache, lines, discountDetails, currentLine, branchID, locationID, date.Value, discountCalculationOptions, recalcFilter);
		}

		public virtual void RemoveOrphanDiscountLines(PXCache cache, PXSelectBase<TLine> lines, PXSelectBase<TDiscountDetail> discountDetails)
		{
			TwoWayLookup<TDiscountDetail, TLine> discountDetailsWithLinkedLines = GetListOfLinksBetweenDiscountsAndDocumentLines(cache, lines, discountDetails);

			foreach (TDiscountDetail discountDetail in discountDetailsWithLinkedLines.LeftValues)
			{
				if (discountDetailsWithLinkedLines.RightsFor(discountDetail).Count() == 0)
					discountDetails.Delete(discountDetail);
			}

		}

		/// <summary>
		/// Sets selected manual line discount.
		/// </summary>
		public virtual void UpdateManualLineDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine line,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? branchID,
			int? locationID,
			DateTime? date,
			DiscountCalculationOptions discountCalculationOptions)
		{
			if (branchID == null || locationID == null || date == null || !IsDiscountFeatureEnabled(discountCalculationOptions))
				return;

			UpdateEntityCache();
			SetManualLineDiscount(cache, GetDiscountEntitiesDiscounts(cache, line, locationID, true), line, date.Value);
			RecalculateGroupAndDocumentDiscounts(cache, lines, line, discountDetails, branchID, locationID, date.Value, discountCalculationOptions);
		}

		/// <summary>
		/// Updates document discount.
		/// </summary>
		public virtual void UpdateDocumentDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? branchID,
			int? locationID,
			DateTime? date,
			bool recalcDocDiscount,
			DiscountCalculationOptions discountCalculationOptions)
		{
			if (branchID == null || locationID == null || date == null || !IsDiscountFeatureEnabled(discountCalculationOptions))
				return;

			UpdateEntityCache();
			if (recalcDocDiscount)
			{
				AdjustGroupDiscountRates(cache, lines, discountDetails, locationID, date.Value);
				SetDocumentDiscount(cache, lines, discountDetails, null, branchID, locationID, date.Value, discountCalculationOptions);
				discountDetails.View.RequestRefresh();
			}
			else
				CalculateDocumentDiscountRate(cache, GetDocumentDetails(cache, lines), null, discountDetails);
		}

		/// <summary>
		/// Inserts manual group or document discount.
		/// </summary>
		public virtual void InsertManualDocGroupDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			PXSelectBase<TDiscountDetail> discountDetails,
			TDiscountDetail currentDiscountDetailLine,
			string discountID,
			string discountSequenceID,
			int? branchID,
			int? locationID,
			DateTime? date,
			DiscountCalculationOptions discountCalculationOptions)
		{
			if (branchID == null || locationID == null || date == null || !IsDiscountFeatureEnabled(discountCalculationOptions))
				return;

			UpdateEntityCache();

			currentDiscountDetailLine.IsManual = true;
			SetManualGroupDocDiscount(cache, lines, null, discountDetails, currentDiscountDetailLine, discountID, discountSequenceID, branchID, locationID, date.Value, discountCalculationOptions);
			if (currentDiscountDetailLine.Type != DiscountType.Document)
				UpdateDocumentDiscount(cache, lines, discountDetails, branchID, locationID, date.Value, (currentDiscountDetailLine.Type != null && currentDiscountDetailLine.Type != DiscountType.Document), discountCalculationOptions);
		}

		/// <summary>
		/// Updates existing manual group or document discount.
		/// </summary>
		public virtual void UpdateManualDocGroupDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			PXSelectBase<TDiscountDetail> discountDetails,
			TDiscountDetail currentDiscountDetailLine,
			string discountID,
			string discountSequenceID,
			int? branchID,
			int? locationID,
			DateTime? date,
			DiscountCalculationOptions discountCalculationOptions)
		{
			if (branchID == null || locationID == null || date == null || !IsDiscountFeatureEnabled(discountCalculationOptions))
				return;

			UpdateEntityCache();

			currentDiscountDetailLine.IsManual = true;
			currentDiscountDetailLine.CuryDiscountAmt = 0m;
			currentDiscountDetailLine.DiscountPct = 0m;
			currentDiscountDetailLine.CuryDiscountableAmt = 0m;
			currentDiscountDetailLine.DiscountableQty = 0m;
			Boolean recalcDocDiscount = currentDiscountDetailLine.Type != null && currentDiscountDetailLine.Type != DiscountType.Document;
			UpdateDocumentDiscount(cache, lines, discountDetails, branchID, locationID, date.Value, recalcDocDiscount, discountCalculationOptions);
			SetManualGroupDocDiscount(cache, lines, null, discountDetails, currentDiscountDetailLine, discountID, discountSequenceID, branchID, locationID, date.Value, discountCalculationOptions);
			UpdateDocumentDiscount(cache, lines, discountDetails, branchID, locationID, date.Value, recalcDocDiscount, discountCalculationOptions);
		}

		#endregion

		#region Line Discounts
		//Calculates line-level discounts
		protected virtual void SetLineDiscount(
			PXCache cache,
			HashSet<KeyValuePair<object, string>> entities,
			TLine line,
			DateTime date,
			DiscountCalculationOptions discountCalculationOptions,
			RecalcDiscountsParamFilter recalcFilter = null)
		{
			AmountLineFields documentLine = AmountLineFields.GetMapFor(line, cache);
			DiscountLineFields dLine = DiscountLineFields.GetMapFor(line, cache);

			bool overrideManualDiscount = recalcFilter != null && recalcFilter.UseRecalcFilter == true && recalcFilter.OverrideManualDiscounts == true;

			if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisableAllAutomaticDiscounts) && !overrideManualDiscount)
			{
				dLine.ManualDisc = true;
				return;
			}

			object unitPrice = documentLine.CuryUnitPrice;
			object extPrice = documentLine.CuryExtPrice;
			object qty = documentLine.Quantity;

			if (!dLine.ManualDisc || overrideManualDiscount)
			{
				GetDiscountTypes();

				if (extPrice != null && qty != null)
				{
					decimal discountTargetPrice;
					if (GetLineDiscountTarget(cache, line) == AR.LineDiscountTargetType.SalesPrice)
						discountTargetPrice = (decimal)unitPrice;
					else
						discountTargetPrice = (decimal)extPrice;

					if (overrideManualDiscount)
					{
						dLine.ManualDisc = false;
						dLine.RaiseFieldUpdated<DiscountLineFields.manualDisc>(null);
					}

					DiscountDetailLine discount = SelectBestDiscount(cache, dLine, entities, DiscountType.Line, discountTargetPrice, (decimal)qty, date);
					if (discount.DiscountID != null)
					{
						decimal curyDiscountAmt = CalculateDiscount(cache, discount, dLine, discountTargetPrice, (decimal)qty, date, DiscountType.Line);

						DiscountResult dResult = new DiscountResult(discount.DiscountedFor == DiscountOption.Amount ? curyDiscountAmt : discount.Discount, discount.DiscountedFor == DiscountOption.Amount ? true : false);

						ApplyDiscountToLine(cache, line, (decimal)qty, (decimal)unitPrice, (decimal)extPrice, dLine, dResult, 1);
						dLine.DiscountID = discount.DiscountID;
						//dline.RaiseFieldUpdated<DiscountLineFields.discountID>(null);
						dLine.DiscountSequenceID = discount.DiscountSequenceID;
						if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisableAllAutomaticDiscounts))
							dLine.ManualDisc = true;
						dLine.RaiseFieldUpdated<DiscountLineFields.discountSequenceID>(null);
					}
					else
					{
						DiscountResult dResult = new DiscountResult(0m, true);

						ApplyDiscountToLine(cache, line, (decimal)qty, (decimal)unitPrice, (decimal)extPrice, dLine, dResult, 1);
					}
				}
			}
		}

		/// <summary>
		/// Sets manual line discount
		/// </summary>
		protected virtual void SetManualLineDiscount(PXCache cache, HashSet<KeyValuePair<object, string>> entities, TLine line, DateTime date)
		{
			if (!IsDiscountCalculationNeeded(cache, line, DiscountType.Line)) return;

			AmountLineFields documentLine = AmountLineFields.GetMapFor(line, cache);
			DiscountLineFields dLine = DiscountLineFields.GetMapFor(line, cache);

			object unitPrice = documentLine.CuryUnitPrice;
			object extPrice = documentLine.CuryExtPrice;
			object qty = documentLine.Quantity;

			string discountID = dLine.DiscountID;

			if (extPrice != null && qty != null)
			{
				GetDiscountTypes();

				HashSet<DiscountSequenceKey> discountSequencesByDiscountID = SelectDiscountSequences(discountID);
				HashSet<DiscountSequenceKey> applicableDiscountSequences = SelectApplicableEntityDiscounts(entities, DiscountType.Line, false);

				applicableDiscountSequences.IntersectWith(discountSequencesByDiscountID);

				decimal discountTargetPrice;
				if (GetLineDiscountTarget(cache, line) == AR.LineDiscountTargetType.SalesPrice)
					discountTargetPrice = (decimal)unitPrice;
				else
					discountTargetPrice = (decimal)extPrice;

				DiscountDetailLine discount = SelectApplicableDiscount(cache, dLine, applicableDiscountSequences, discountTargetPrice, (decimal)qty, DiscountType.Line, date);

				dLine.ManualDisc = true;
				dLine.RaiseFieldUpdated<DiscountLineFields.manualDisc>(null);

				if (discount.DiscountID != null)
				{
					decimal curyDiscountAmt = CalculateDiscount(cache, discount, dLine, discountTargetPrice, (decimal)qty, date, DiscountType.Line);

					DiscountResult dResult = new DiscountResult(discount.DiscountedFor == DiscountOption.Amount ? curyDiscountAmt : discount.Discount, discount.DiscountedFor == DiscountOption.Amount);

					ApplyDiscountToLine(cache, line, (decimal)qty, (decimal)unitPrice, (decimal)extPrice, dLine, dResult, 1);

					dLine.DiscountSequenceID = discount.DiscountSequenceID;
					dLine.RaiseFieldUpdated<DiscountLineFields.discountSequenceID>(null);
				}
				else
				{
					DiscountResult dResult = new DiscountResult(0m, true);

					ApplyDiscountToLine(cache, line, (decimal)qty, (decimal)unitPrice, (decimal)extPrice, dLine, dResult, 1);

					if (discountID != null)
					{
						PXUIFieldAttribute.SetWarning<DiscountLineFields.discountID>(cache, line, PXMessages.LocalizeFormatNoPrefixNLA(AR.Messages.NoDiscountFound, discountID));
						if (cache.Graph.IsImport) dLine.DiscountID = null;
					}
				}
			}
		}

		private const string DiscountID = "DiscountID";
		private const string DiscountSequenceID = "DiscountSequenceID";
		private const string TypeFieldName = "Type";
		private const string LineNbrFieldName = "LineNbr";

		/// <summary>
		/// Applies line-level discount to a line
		/// </summary>
		/// <param name="qty">Quantity</param>
		/// <param name="curyUnitPrice">Cury Unit Price</param>
		/// <param name="curyExtPrice">Cury Ext. Price</param>
		/// <param name="dline">Discount will be applied to this line</param>
		/// <param name="discountResult">DiscountResult (discount percent/amount and isAmount flag)</param>
		/// <param name="multInv"></param>
		protected virtual void ApplyDiscountToLine(PXCache sender, TLine line, decimal? qty, decimal? curyUnitPrice, decimal? curyExtPrice, DiscountLineFields dLine, DiscountResult discountResult, int multInv)
		{
			PXCache cache = dLine.Cache;
			object row = dLine.MappedLine;

			decimal qtyVal = qty != null ? Math.Abs(qty.Value) : 0m;

			if (!discountResult.IsEmpty)
			{
				string lineDiscountTarget = GetLineDiscountTarget(sender, line);
				int precision = CommonSetupDecPl.PrcCst;

				if (discountResult.IsAmount)
				{
					decimal discAmt = discountResult.Discount ?? 0;
					if (lineDiscountTarget == AR.LineDiscountTargetType.SalesPrice) discAmt *= qtyVal;

					decimal curyDiscAmt;
					PXCurrencyAttribute.CuryConvCury(cache, row, discAmt, out curyDiscAmt, precision);
					decimal? oldCuryDiscAmt = dLine.CuryDiscAmt;
					dLine.CuryDiscAmt = multInv * PXCurrencyAttribute.Round(cache, row, curyDiscAmt, CMPrecision.TRANCURY); ;
					if (dLine.CuryDiscAmt > Math.Abs(curyExtPrice ?? 0m))
					{
						dLine.CuryDiscAmt = curyExtPrice;
						PXUIFieldAttribute.SetWarning<DiscountLineFields.curyDiscAmt>(sender, line,
							PXMessages.LocalizeFormatNoPrefix(AR.Messages.LineDiscountAmtMayNotBeGreaterExtPrice, AmountLineFields.GetMapFor(line, cache).ExtPriceDisplayName));
					}
					if (dLine.CuryDiscAmt != oldCuryDiscAmt)
					{
						UpdateDiscAmt<DiscountLineFields.curyDiscAmt>(dLine, oldCuryDiscAmt, dLine.CuryDiscAmt);
					}

					if (dLine.CuryDiscAmt != 0 && curyExtPrice.Value != 0)
					{
						decimal? oldValue = dLine.DiscPct;
						decimal discPct = dLine.CuryDiscAmt.Value * 100 / curyExtPrice.Value;
						dLine.DiscPct = Math.Round(discPct, 6, MidpointRounding.AwayFromZero);
						if (dLine.DiscPct != oldValue)
							UpdateDiscPct<DiscountLineFields.discPct>(dLine, oldValue, dLine.DiscPct);
					}
				}
				else
				{
					decimal? oldValue = dLine.DiscPct;
					dLine.DiscPct = Math.Round(discountResult.Discount ?? 0, 6, MidpointRounding.AwayFromZero);
					if (dLine.DiscPct != oldValue)
						UpdateDiscPct<DiscountLineFields.discPct>(dLine, oldValue, dLine.DiscPct);
					decimal? oldCuryDiscAmt = dLine.CuryDiscAmt;
					decimal curyDiscAmt;
					decimal priceSign = curyExtPrice < 0 && curyUnitPrice > 0 ? -1: 1;

					if (lineDiscountTarget == AR.LineDiscountTargetType.SalesPrice)
					{
						decimal salesPriceAfterDiscount = priceSign * ((curyUnitPrice ?? 0) - PXDBPriceCostAttribute.Round((curyUnitPrice ?? 0) * 0.01m * (dLine.DiscPct ?? 0)));

						LineEntitiesFields efields = LineEntitiesFields.GetMapFor(line, cache);
						bool isVendorDiscount = efields != null && efields.VendorID != null && efields.CustomerID == null;
						if ((isVendorDiscount && ApplyQuantityDiscountByBaseUOM(sender.Graph).ForAP) || (!isVendorDiscount && ApplyQuantityDiscountByBaseUOM(sender.Graph).ForAR))
						{
							qtyVal = INUnitAttribute.ConvertFromBase<LineEntitiesFields.inventoryID, AmountLineFields.uOM>(cache, line, qtyVal, INPrecision.QUANTITY);
						}

						decimal extPriceAfterDiscount = qtyVal * PXDBPriceCostAttribute.Round(salesPriceAfterDiscount);
						curyDiscAmt = priceSign * qtyVal * (curyUnitPrice ?? 0) - PXCurrencyAttribute.Round(cache, row, extPriceAfterDiscount, CMPrecision.TRANCURY);
						if (priceSign * curyDiscAmt < 0 && Math.Abs(curyDiscAmt) < 0.01m)//this can happen only due to difference in rounding between unitprice and exprice. ex when Unit price has 4 digits (with value in 3rd place 20.0050) and ext price only 2 
							curyDiscAmt = 0;
					}
					else
					{
						curyDiscAmt = (curyExtPrice ?? 0) * 0.01m * (dLine.DiscPct ?? 0);
					}

					dLine.CuryDiscAmt = multInv * PXCurrencyAttribute.Round(cache, row, curyDiscAmt, CMPrecision.TRANCURY);
					if (Math.Abs(dLine.CuryDiscAmt ?? 0m) > Math.Abs(curyExtPrice ?? 0m))
					{
						dLine.CuryDiscAmt = curyExtPrice;
						PXUIFieldAttribute.SetWarning<DiscountLineFields.curyDiscAmt>(sender, line,
							PXMessages.LocalizeFormatNoPrefix(AR.Messages.LineDiscountAmtMayNotBeGreaterExtPrice, AmountLineFields.GetMapFor(line, cache).ExtPriceDisplayName));
					}
					if (dLine.CuryDiscAmt != oldCuryDiscAmt)
					{
						UpdateDiscAmt<DiscountLineFields.curyDiscAmt>(dLine, oldCuryDiscAmt, dLine.CuryDiscAmt);
					}
				}
			}
			else if (!sender.Graph.IsImport)
			{
				decimal? oldDiscPct = dLine.DiscPct;
				decimal? oldCuryDiscAmt = dLine.CuryDiscAmt;
				string oldDiscountID = dLine.DiscountID;
				dLine.DiscPct = 0;
				if (oldDiscPct != 0)
					dLine.RaiseFieldUpdated<DiscountLineFields.discPct>(oldDiscPct);
				dLine.CuryDiscAmt = 0;
				if (oldCuryDiscAmt != 0)
					dLine.RaiseFieldUpdated<DiscountLineFields.curyDiscAmt>(oldCuryDiscAmt);
				if (dLine.DiscPct == 0m && dLine.CuryDiscAmt == 0m)
				{
					dLine.DiscountID = null;
					dLine.DiscountSequenceID = null;
				}
			}
		}

		/// <summary>
		/// Clears line discount and recalculates all dependent values
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="line">Document line</param>
		protected virtual void ClearLineDiscount(PXCache cache, TLine line, DiscountLineFields dLine)
		{
			ApplyDiscountToLine(cache, line, null, null, null, dLine ?? DiscountLineFields.GetMapFor(line, cache), new DiscountResult(0m, true), 1);
		}

		private void UpdateDiscPct<TField>(DiscountLineFields dLine, decimal? oldValue, decimal? newValue)
			where TField : IBqlField
		{
			object discPct = newValue;
			dLine.RaiseFieldVerifying<TField>(ref discPct);
			dLine.DiscPct = (decimal)discPct;
			dLine.RaiseFieldUpdated<TField>(oldValue);
		}

		private void UpdateDiscAmt<TField>(DiscountLineFields dLine, decimal? oldValue, decimal? newValue)
			where TField : IBqlField
		{
			object curyDiscAmt = newValue;
			dLine.RaiseFieldVerifying<TField>(ref curyDiscAmt);
			dLine.CuryDiscAmt = (decimal)curyDiscAmt;
			dLine.RaiseFieldUpdated<TField>(oldValue);
		}

		#endregion

		#region Group Discounts

		//Collects all applicable group discounts and adds them do Discount Details
		protected virtual bool SetGroupDiscounts(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? locationID,
			DateTime date,
			DiscountCalculationOptions discountCalculationOptions,
			RecalcDiscountsParamFilter recalcFilter = null)
		{
			if (!IsDiscountCalculationNeeded(cache, currentLine, DiscountType.Group))
				return false;

			var newDiscountDetails = new Dictionary<DiscountSequenceKey, TDiscountDetail>();
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			List<TLine> documentDetails = GetDocumentDetails(cache, lines);

			bool skipDocumentDiscounts = false;
			if (documentDetails.Count != 0)
			{
				//If true, all document discounts will be recalculated regardless of DisableAllAutomaticDiscounts option.
				bool recalculateAllFromAction = recalcFilter != null && recalcFilter.UseRecalcFilter == true && recalcFilter.OverrideManualDocGroupDiscounts == true;

				TwoWayLookup<DiscountSequenceKey, TLine> groupDiscountCodesWithApplicableLines = CollectAllGroupDiscountCodesWithApplicableLines(cache, documentDetails, discountDetails, locationID, date, recalculateAllFromAction);

				//Creating new list of discount details.
				foreach (DiscountSequenceKey applicableGroup in groupDiscountCodesWithApplicableLines.LeftValues)
				{
					if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisableFreeItemDiscountsCalculation)
						&& SelectDiscountSequence(cache, applicableGroup.DiscountID, applicableGroup.DiscountSequenceID).DiscountedFor == DiscountOption.FreeItem)
						continue;

					DiscountLineFields discountedLine = DiscountLineFields.GetMapFor(documentDetails[0], cache);

					List<DiscountDetailLine> applicableDiscounts = SelectApplicableDiscounts(
						cache,
						discountedLine,
						new HashSet<DiscountSequenceKey> { applicableGroup },
						(decimal)applicableGroup.CuryDiscountableAmount,
						(decimal)applicableGroup.DiscountableQuantity,
						DiscountType.Group,
						date);

					CreateNewListOfDiscountDetails(
						cache,
						discountedLine,
						applicableDiscounts,
						date,
						newDiscountDetails, //new list of discount details
						(decimal)applicableGroup.CuryDiscountableAmount,
						(decimal)applicableGroup.DiscountableQuantity);
				}

				//Removing old discount details that are not present in new list of discount details and updating discount details with "Skip" flag.
				RemoveUnapplicableDiscountDetails(cache, discountDetails, newDiscountDetails.Keys.ToList(), DiscountType.Group, recalcFilter: recalcFilter);

				//Only discounts that already exist will be recalculated when DisableAllAutomaticDiscounts is enabled. New discounts will not be added.
				if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisableAllAutomaticDiscounts))
				{
					if (recalculateAllFromAction)
					{
						foreach (KeyValuePair<DiscountSequenceKey, TDiscountDetail> dDetail in newDiscountDetails)
						{
							dDetail.Value.IsManual = true;
						}
					}
					else
					{
						List<TDiscountDetail> trace = GetDiscountDetailsByType(cache, discountDetails, DiscountType.Group);

						if (trace.Count == 0)
							newDiscountDetails.Clear();

						if (trace.Count > 0 && newDiscountDetails.Count > 0)
							newDiscountDetails = (from n1 in newDiscountDetails
												  join n2 in trace
												  on new { n1.Key.DiscountID, n1.Key.DiscountSequenceID }
												  equals new { n2.DiscountID, n2.DiscountSequenceID }
												  select n1).ToDictionary();
					}
				}

				//Inserting new discount detail line into Discount Details grid
				foreach (KeyValuePair<DiscountSequenceKey, TDiscountDetail> dDetail in newDiscountDetails)
				{
					TDiscountDetail updatedInsertedDiscountDetail = UpdateInsertDiscountTrace(cache, discountDetails, dDetail.Value);
					if (updatedInsertedDiscountDetail != null
						&& updatedInsertedDiscountDetail.SkipDiscount != true
						&& cachedDiscountTypes[updatedInsertedDiscountDetail.DiscountID].SkipDocumentDiscounts)
					{
						skipDocumentDiscounts = true;
					}
				}

				//Removing Document discount in case Skip Document Discount flag is set for one or more group discount.
				if (skipDocumentDiscounts)
				{
					RemoveUnapplicableDiscountDetails(cache, discountDetails, null, DiscountType.Document, recalcFilter: recalcFilter);
					CalculateDocumentDiscountRate(cache, documentDetails, null, discountDetails);
				}

				CalculateGroupDiscountRate(cache, documentDetails, currentLine, groupDiscountCodesWithApplicableLines, discountDetails, false);
			}
			else
			{
				RemoveUnapplicableDiscountDetails(cache, discountDetails, null, DiscountType.Group, recalcFilter: recalcFilter);
			}
			return skipDocumentDiscounts;
		}

		//Collects all applicable group discounts and updates GroupDiscountRates
		protected virtual TwoWayLookup<DiscountSequenceKey, TLine> AdjustGroupDiscountRates(
			PXCache cache,
			PXSelectBase<TLine> lines,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? locationID,
			DateTime date)
		{
			List<TLine> documentDetails = GetDocumentDetails(cache, lines);
			var cachedDiscountTypes = GetCachedDiscountCodes();
			TwoWayLookup<DiscountSequenceKey, TLine> discountCodesWithApplicableLines = new TwoWayLookup<DiscountSequenceKey, TLine>();

			if (documentDetails.Count != 0)
			{
				discountCodesWithApplicableLines = CollectAllGroupDiscountCodesWithApplicableLines(cache, documentDetails, discountDetails, locationID, date);
				CalculateGroupDiscountRate(cache, documentDetails, null, discountCodesWithApplicableLines, discountDetails, true);
			}
			return discountCodesWithApplicableLines;
		}

		/// <summary>
		/// Stores discounted document lines
		/// </summary>
		public struct DiscountedLines
		{
			public List<TLine> DiscountableLines { get; set; }
			public DiscountableValues DiscountableValues { get; set; }

			internal DiscountedLines(List<TLine> discountableLines, DiscountableValues discountableValues)
			{
				this.DiscountableLines = discountableLines;
				this.DiscountableValues = discountableValues;
			}
		}

		#region Methods to retrieve links between document details and discount details

		/// <summary>
		/// Returns list of discounts already applied to the document with the list of applicable document lines for each of them based on the information stored in DiscountsAppliedToLine field.
		/// Discounts are neither recalculated nor checked if they are still applicable.
		/// </summary>
		public virtual TwoWayLookup<TDiscountDetail, TLine> GetListOfDiscountCodesWithApplicableLines(
			PXCache cache,
			List<TLine> documentDetails,
			PXSelectBase<TDiscountDetail> discountDetails,
			string discountType)
		{
			TwoWayLookup<TDiscountDetail, TLine> discountsWithApplicableLines = new TwoWayLookup<TDiscountDetail, TLine>(leftComparer: new TDiscountDetailComparer());
			List<TDiscountDetail> discountLines = GetDiscountDetailsByType(cache, discountDetails, discountType);

			foreach (TLine line in documentDetails)
			{
				DiscountLineFields discountedLine = DiscountLineFields.GetMapFor(line, cache);
				foreach (TDiscountDetail discountLine in discountLines)
				{
					if (discountLine.LineNbr != null && discountedLine.DiscountsAppliedToLine != null && discountedLine.DiscountsAppliedToLine.Contains(discountLine.LineNbr ?? 0))
					{
						discountsWithApplicableLines.Link(discountLine, line);
					}
				}
			}
			return discountsWithApplicableLines;
		}

		/// <summary>
		/// Returns list of discounts with list of applicable document lines for each of them based on the current discount configuration.
		/// Only applicable discounts will be returned.
		/// </summary>
		protected virtual TwoWayLookup<DiscountSequenceKey, TLine> CollectAllGroupDiscountCodesWithApplicableLines(
			PXCache cache,
			List<TLine> documentDetails,
			PXSelectBase<TDiscountDetail> discountDetails,
			int? locationID,
			DateTime date,
			bool skipManual = false)
		{
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			TwoWayLookup<DiscountSequenceKey, TLine> discountCodesWithLines = new TwoWayLookup<DiscountSequenceKey, TLine>();

			foreach (TLine line in documentDetails)
			{
				DiscountLineFields discountedLine = DiscountLineFields.GetMapFor(line, cache);

				if (discountedLine.DiscountID != null && cachedDiscountTypes[discountedLine.DiscountID].ExcludeFromDiscountableAmt)
					continue;

				HashSet<DiscountSequenceKey> applicableGroupDiscounts = SelectApplicableEntityDiscounts(GetDiscountEntitiesDiscounts(cache, line, locationID, true), DiscountType.Group, skipManual);

				applicableGroupDiscounts = RemoveManualDiscounts(cache, discountDetails, applicableGroupDiscounts, DiscountType.Group);

				CombineDiscountsAndDocumentLines(cache, line, applicableGroupDiscounts, ref discountCodesWithLines);
			}

			return discountCodesWithLines;
		}

		/// <summary>
		/// Updates discountCodesWithLines with two way links between discounts and given document line
		/// </summary>
		protected virtual void CombineDiscountsAndDocumentLines(PXCache cache, TLine line, HashSet<DiscountSequenceKey> applicableGroupDiscounts, ref TwoWayLookup<DiscountSequenceKey, TLine> discountCodesWithLines)
		{
			var discountableValues =
								new DiscountableValues
								{
									CuryDiscountableAmount = AmountLineFields.GetMapFor(line, cache).CuryLineAmount ?? 0m,
									DiscountableQuantity = AmountLineFields.GetMapFor(line, cache).Quantity ?? 0m
								};

			foreach (DiscountSequenceKey dSequence in applicableGroupDiscounts)
			{
				if (discountCodesWithLines.Contains(dSequence))
				{
					DiscountSequenceKey existingDSequence = discountCodesWithLines.LeftValues.First(x => x.Equals(dSequence));
					existingDSequence.CuryDiscountableAmount += discountableValues.CuryDiscountableAmount;
					existingDSequence.DiscountableQuantity += discountableValues.DiscountableQuantity;
					discountCodesWithLines.Link(existingDSequence, line);
				}
				else
				{
					dSequence.CuryDiscountableAmount = discountableValues.CuryDiscountableAmount;
					dSequence.DiscountableQuantity = discountableValues.DiscountableQuantity;
					discountCodesWithLines.Link(dSequence, line);
				}
			}
		}

		public virtual TwoWayLookup<TDiscountDetail, TLine> MatchLinesWithGroupDiscounts(
			PXCache cache,
			TwoWayLookup<DiscountSequenceKey, TLine> discountCodesWithApplicableLines,
			List<TLine> documentDetails,
			PXSelectBase<TDiscountDetail> discountDetails)
		{
			List<TDiscountDetail> newDiscountDetails = GetDiscountDetailsByType(cache, discountDetails, null);

			TwoWayLookup<TDiscountDetail, TLine> discountsWithApplicableLines = new TwoWayLookup<TDiscountDetail, TLine>(leftComparer: new TDiscountDetailComparer());


			foreach (TLine line in documentDetails)
			{
				DiscountLineFields discountFields = DiscountLineFields.GetMapFor(line, cache);

				foreach (TDiscountDetail discount in newDiscountDetails)
				{
					DiscountSequenceKey dsKey = new DiscountSequenceKey(discount.DiscountID, discount.DiscountSequenceID);

					if ((discount.IsOrigDocDiscount == true || discount.Type != DiscountType.Group) && discountFields.DiscountsAppliedToLine != null && discountFields.DiscountsAppliedToLine.Contains(discount.LineNbr ?? 0))
					{
						discountsWithApplicableLines.Link(discount, line);
					}
					else if (discount.IsOrigDocDiscount != true && discountCodesWithApplicableLines.RightValues.Contains(line) &&
						discountCodesWithApplicableLines.LeftsFor(line).Contains(dsKey))
					{
						discountsWithApplicableLines.Link(discount, line);
					}
				}
			}

			return discountsWithApplicableLines;
		}

		public class TDiscountDetailComparer : IEqualityComparer<TDiscountDetail>
		{
			public bool Equals(TDiscountDetail discountDetail1, TDiscountDetail discountDetail2)
			{
				return discountDetail1.DiscountID == discountDetail2.DiscountID &&
					discountDetail1.DiscountSequenceID == discountDetail2.DiscountSequenceID &&
					discountDetail1.Type == discountDetail2.Type &&
					discountDetail1.LineNbr == discountDetail2.LineNbr;
			}

			public int GetHashCode(TDiscountDetail discountDetail)
			{
				int hashCode = 17;
				hashCode = (hashCode * 11) + discountDetail.DiscountID?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.DiscountSequenceID?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.Type?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountDetail.LineNbr?.GetHashCode() ?? 0;
				return hashCode;
			}
		}

		/// <summary>
		/// Creates TwoWayLookup with all links between Document Details lines and Discount Details lines. Accepts PXSelectBase.
		/// </summary>
		public virtual TwoWayLookup<TDiscountDetail, TLine> GetListOfLinksBetweenDiscountsAndDocumentLines(PXCache cache, PXSelectBase<TLine> documentDetailsSelect, PXSelectBase<TDiscountDetail> discountDetailsSelect)
		{
			List<TDiscountDetail> discountDetails = GetDiscountDetailsByType(cache, discountDetailsSelect, null);
			if (!discountDetails.Any())
			{
				return new TwoWayLookup<TDiscountDetail, TLine>(leftComparer: new TDiscountDetailComparer());
			}
			List<TLine> documentDetails = GetDocumentDetails(cache, documentDetailsSelect, null);

			return GetListOfLinksBetweenDiscountsAndDocumentLines(cache, documentDetails, discountDetails);
		}

		/// <summary>
		/// Creates TwoWayLookup with all links between Document Details lines and Discount Details lines.
		/// </summary>
		public virtual TwoWayLookup<TDiscountDetail, TLine> GetListOfLinksBetweenDiscountsAndDocumentLines(PXCache cache, List<TLine> documentDetails, List<TDiscountDetail> discountDetails)
		{
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines = new TwoWayLookup<TDiscountDetail, TLine>(leftComparer: new TDiscountDetailComparer());
			foreach (TLine line in documentDetails)
			{
				DiscountLineFields discountFields = DiscountLineFields.GetMapFor(line, cache);

				foreach (TDiscountDetail dDetail in discountDetails)
				{
					if ((discountFields.DiscountsAppliedToLine != null && discountFields.DiscountsAppliedToLine.Contains(dDetail.LineNbr ?? 0)) ||
						(dDetail.IsOrigDocDiscount != true && dDetail.Type.IsIn(DiscountType.Document, DiscountType.ExternalDocument)))
					{
						discountCodesWithApplicableLines.Link(dDetail, line);
					}
				}
			}

			//Adding discount detail lines not linked to any document line
			if (discountCodesWithApplicableLines.LeftValues.Count() != discountDetails.Count)
			{
				foreach (TDiscountDetail dDetail in discountDetails)
				{
					if (!discountCodesWithApplicableLines.LeftValues.Contains(dDetail))
					{
						discountCodesWithApplicableLines.Add(dDetail);
					}
				}
			}

			return discountCodesWithApplicableLines;
		}

		public virtual TwoWayLookup<TDiscountDetail, TLine> GetListOfDiscountsAppliedToLine(PXCache cache, TLine line, List<TDiscountDetail> discountDetails)
		{
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines = new TwoWayLookup<TDiscountDetail, TLine>(leftComparer: new TDiscountDetailComparer());

			DiscountLineFields discountFields = DiscountLineFields.GetMapFor(line, cache);

			if (discountFields.DiscountsAppliedToLine != null)
			{
				foreach (TDiscountDetail dDetail in discountDetails)
				{
					if (discountFields.DiscountsAppliedToLine.Contains(dDetail.LineNbr ?? 0) ||
						(dDetail.IsOrigDocDiscount != true && dDetail.Type.IsIn(DiscountType.Document, DiscountType.ExternalDocument)))
					{
						discountCodesWithApplicableLines.Link(dDetail, line);
					}
				}
			}
			return discountCodesWithApplicableLines;
		}

		#endregion

		#region Discount rate calculation

		//TODO: Combine with UpdateDocumentDiscount (see ARInvoiceDiscountDetail_RowDeleted)
		/// <summary>
		/// Recalculates document and group discount rates in case discount detail line from ordiginal document has been deleted.
		/// </summary>
		public virtual void UpdateGroupAndDocumentDiscountRatesOnly(PXCache cache,
			PXSelectBase<TLine> lines,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			bool recalculateTaxes,
			bool recalcAll = true)
		{
			List<TLine> documentDetails = GetDocumentDetails(cache, lines);
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines = GetListOfDiscountCodesWithApplicableLines(cache, documentDetails, discountDetails, null);
			CalculateGroupDiscountRate(cache, documentDetails, currentLine, discountCodesWithApplicableLines, recalculateTaxes, recalcAll);
			CalculateDocumentDiscountRate(cache, discountCodesWithApplicableLines, currentLine, lines);
		}

		#region Group discount rate
		/// <summary>
		/// Calculates DocumentDiscountRate and updates array of discounts applied to document details lines (DiscountsAppliedToLine field)
		/// GroupDiscountRate affects Taxes and Commissions calculation.
		/// </summary>
		public virtual void CalculateGroupDiscountRate(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine currentLine,
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines,
			bool recalculateTaxes,
			bool recalcAll = true,
			bool forceFormulaCalculation = false)
		{
			List<TLine> documentDetails = GetDocumentDetails(cache, lines);
			CalculateGroupDiscountRate(cache, documentDetails, currentLine, discountCodesWithApplicableLines, recalculateTaxes, recalcAll, forceFormulaCalculation);
		}

		/// <summary>
		/// Calculates DocumentDiscountRate and updates array of discounts applied to document details lines (DiscountsAppliedToLine field)
		/// GroupDiscountRate affects Taxes and Commissions calculation.
		/// </summary>
		public virtual void CalculateGroupDiscountRate(
			PXCache cache,
			List<TLine> documentDetails,
			TLine currentLine,
			TwoWayLookup<DiscountSequenceKey, TLine> newGroupDiscountCodesWithApplicableLines,
			PXSelectBase<TDiscountDetail> discountDetails,
			bool recalculateTaxes,
			bool recalcAll = true)
		{
			CalculateGroupDiscountRate(cache, documentDetails, currentLine, MatchLinesWithGroupDiscounts(cache, newGroupDiscountCodesWithApplicableLines, documentDetails, discountDetails), recalculateTaxes, recalcAll);
		}

		/// <summary>
		/// Calculates DocumentDiscountRate and updates array of discounts applied to document details lines (DiscountsAppliedToLine field)
		/// GroupDiscountRate affects Taxes and Commissions calculation.
		/// </summary>
		public virtual void CalculateGroupDiscountRate(
			PXCache cache,
			List<TLine> documentDetails,
			TLine currentLine,
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines,
			bool recalculateTaxes,
			bool recalcAll = true,
			bool forceFormulaCalculation = false)
		{
			foreach (TLine line in documentDetails)
			{
				TLine oldCurrentLine = (TLine)cache.CreateCopy(currentLine);

				AmountLineFields lineAmt = AmountLineFields.GetMapFor(line, cache);
				TLine oldLine = (TLine)cache.CreateCopy(line);

				lineAmt.OrigGroupDiscountRate = CalculateGroupDiscountRateOnSpecificLine(cache, line, discountCodesWithApplicableLines, true, recalcAll);
				lineAmt.GroupDiscountRate = CalculateGroupDiscountRateOnSpecificLine(cache, line, discountCodesWithApplicableLines, false, recalcAll);

				UpdateListOfDiscountsAppliedToLine(cache, discountCodesWithApplicableLines, line);

				if (AmountLineFields.GetMapFor(oldLine, cache).GroupDiscountRate != lineAmt.GroupDiscountRate ||
					AmountLineFields.GetMapFor(oldLine, cache).OrigGroupDiscountRate != lineAmt.OrigGroupDiscountRate)
				{
					RecalcTaxes(cache, line, oldLine, oldCurrentLine);

					if (forceFormulaCalculation)
					{
						cache.RaiseRowUpdated(line, oldLine);
					}
				}
			}
		}

		/// <summary>
		/// Calculates DocumentDiscountRate and updates array of discounts applied to document details lines (DiscountsAppliedToLine field)
		/// GroupDiscountRate affects Taxes and Commissions calculation.
		/// </summary>
		/// <param name="discountSelectionMode">
		/// Not specified: use all discounts for calculation; 
		/// True: use discounts with IsOrigDocDiscount == true only; 
		/// False: use discounts with IsOrigDocDiscount == false only
		/// </param>
		public virtual decimal? CalculateGroupDiscountRateOnSpecificLine(
			PXCache cache,
			TLine line,
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines,
			bool? discountSelectionMode = null,
			bool recalcAll = true)
		{
			decimal? groupDiscountRate = 1m;

			AmountLineFields lineAmt = AmountLineFields.GetMapFor(line, cache);
			TLine oldLine = (TLine)cache.CreateCopy(line);
			Decimal lineGroupAmt = recalcAll
				? (decimal)lineAmt.CuryLineAmount
				: (decimal)lineAmt.CuryLineAmount * (decimal)lineAmt.GroupDiscountRate;
			if (lineAmt.CuryLineAmount == null || lineAmt.CuryLineAmount == 0m)
				return groupDiscountRate;

			var discountDetailsValues =
				from dDetail in discountCodesWithApplicableLines.LeftsFor(line).Where(d => d.SkipDiscount != true && d.Type == DiscountType.Group &&
				(discountSelectionMode == null || d.IsOrigDocDiscount == discountSelectionMode))
					where dDetail.CuryDiscountableAmt != null && dDetail.CuryDiscountableAmt != 0
				select lineAmt.CuryLineAmount.Value * dDetail.CuryDiscountAmt.Value / dDetail.CuryDiscountableAmt.Value;

			foreach (decimal value in discountDetailsValues)
					if (lineGroupAmt != 0)
					lineGroupAmt -= value;

			groupDiscountRate = lineGroupAmt / AmountLineFields.GetMapFor(oldLine, cache).CuryLineAmount;
			return RoundDiscountRate(groupDiscountRate ?? 1m);
		}

		#endregion

		#region Document discount rate

		/// <summary>
		/// This method should be called if document discount amount has been updated. DocumentDiscountRate affects Taxes and Commisions calculation.
		/// Calculates DocumentDiscountRate and updates array of discounts applied to document details lines (DiscountsAppliedToLine field)
		/// </summary>
		public virtual void CalculateDocumentDiscountRate(
			PXCache cache,
			PXSelectBase<TLine> documentDetails,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			bool forceFormulaCalculation = false)
		{
			List<TLine> documentDetailsList = GetDocumentDetails(cache, documentDetails);
			CalculateDocumentDiscountRate(cache, documentDetailsList, currentLine, discountDetails, forceFormulaCalculation);
		}

		/// <summary>
		/// This method should be called if document discount amount has been updated. DocumentDiscountRate affects Taxes and Commissions calculation.
		/// Calculates DocumentDiscountRate and updates array of discounts applied to document details lines (DiscountsAppliedToLine field).
		/// Use this method when all the links between discount lines and document details lines are formed outside of the DiscountEngine.
		/// </summary>
		/// <param name="documentDetails"></param>
		public virtual void CalculateDocumentDiscountRate(
			PXCache cache,
			TwoWayLookup<TDiscountDetail, TLine> discountsLinkedToLine,
			TLine currentLine,
			PXSelectBase<TLine> documentDetails = null,
			bool forceFormulaCalculation = false)
		{
			List<TLine> documentDetailsList = discountsLinkedToLine.RightValues.ToList();
			if (documentDetailsList.Count() == 0 && documentDetails != null)
			{
				documentDetailsList = GetDocumentDetails(cache, documentDetails);
			}
			ApplyDocumentDiscountRatesToLine(cache, discountsLinkedToLine, documentDetailsList, currentLine, forceFormulaCalculation);
		}

		private void CalculateDocumentDiscountRate(
			PXCache cache,
			List<TLine> documentDetails,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			bool forceFormulaCalculation = false)
		{
			List<TDiscountDetail> newDiscountDetails = GetDiscountDetailsByType(cache, discountDetails, null);
			CalculateDocumentDiscountRate(cache, documentDetails, currentLine, newDiscountDetails, forceFormulaCalculation);
		}

		private void CalculateDocumentDiscountRate(
		PXCache cache,
		List<TLine> documentDetails,
		TLine currentLine,
		List<TDiscountDetail> discountDetails,
		bool forceFormulaCalculation = false)
		{
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines = GetListOfLinksBetweenDiscountsAndDocumentLines(cache, documentDetails, discountDetails);
			ApplyDocumentDiscountRatesToLine(cache, discountCodesWithApplicableLines, documentDetails, currentLine, forceFormulaCalculation);
		}

		private void ApplyDocumentDiscountRatesToLine(PXCache cache, TwoWayLookup<TDiscountDetail, TLine> discountsLinkedToLine, List<TLine> documentDetails, TLine currentLine, bool forceFormulaCalculation = false)
		{
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			Dictionary<TDiscountDetail, (decimal OrigDocumentDiscountableAmt, decimal DocumentDiscountableAmt)> discountableAmounts = new Dictionary<TDiscountDetail, (decimal OrigDocumentDiscountableAmt, decimal DocumentDiscountableAmt)>();
			
			foreach (TLine line in documentDetails)
			{
				decimal totalOrigDocumentDiscountAmt = 0m;
				decimal totalOrigDocumentDiscountableAmt = 0m;

				decimal totalDocumentDiscountAmt = 0m;
				decimal totalDocumentDiscountableAmt = 0m;

				AmountLineFields lineAmtFields = AmountLineFields.GetMapFor(line, cache);
				DiscountLineFields discountFields = DiscountLineFields.GetMapFor(line, cache);

				decimal totalOrigDocumentDiscountRate = 0m;
				decimal totalDocumentDiscountRate = 0m;

				foreach (TDiscountDetail discount in discountsLinkedToLine.LeftsFor(line).Where(x => x.Type.IsIn(DiscountType.Document, DiscountType.ExternalDocument) && x.SkipDiscount != true))
				{
					totalOrigDocumentDiscountAmt = 0m;
					totalOrigDocumentDiscountableAmt = 0m;
					totalDocumentDiscountAmt = 0m;
					totalDocumentDiscountableAmt = 0m;

					if (discount.IsOrigDocDiscount == true)
					{
						totalOrigDocumentDiscountAmt += (decimal)discount.CuryDiscountAmt;
					}
					else
					{
						totalDocumentDiscountAmt += (decimal)discount.CuryDiscountAmt;
					}

					if (discountableAmounts.TryGetValue(discount, out (decimal OrigDocumentDiscountableAmt, decimal DocumentDiscountableAmt) currentDiscountableAmounts))
					{
						totalOrigDocumentDiscountableAmt = currentDiscountableAmounts.OrigDocumentDiscountableAmt;
						totalDocumentDiscountableAmt = currentDiscountableAmounts.DocumentDiscountableAmt;
					}
					else
					{
					foreach (TLine dline in discountsLinkedToLine.RightsFor(discount))
					{
						DiscountLineFields dlineDiscountFields = DiscountLineFields.GetMapFor(dline, cache);

							if (dlineDiscountFields.DiscountID == null || cachedDiscountTypes.ContainsKey(dlineDiscountFields.DiscountID) &&
								!cachedDiscountTypes[dlineDiscountFields.DiscountID].ExcludeFromDiscountableAmt)
						{
							AmountLineFields dlineAmtFields = AmountLineFields.GetMapFor(dline, cache);

							if (discount.IsOrigDocDiscount == true)
								totalOrigDocumentDiscountableAmt += (dlineAmtFields.CuryLineAmount ?? 0m) * (dlineAmtFields.OrigGroupDiscountRate ?? 0m);
							else
								totalDocumentDiscountableAmt += (dlineAmtFields.CuryLineAmount ?? 0m) * (dlineAmtFields.GroupDiscountRate ?? 0m);
						}
					}

						discountableAmounts.Add(discount, (totalOrigDocumentDiscountableAmt, totalDocumentDiscountableAmt));
					}

					if (discountFields.DiscountID == null || cachedDiscountTypes.ContainsKey(discountFields.DiscountID) && !cachedDiscountTypes[discountFields.DiscountID].ExcludeFromDiscountableAmt)
					{
						if (totalOrigDocumentDiscountAmt != 0m && totalOrigDocumentDiscountableAmt != 0m)
							totalOrigDocumentDiscountRate = totalOrigDocumentDiscountRate + totalOrigDocumentDiscountAmt / totalOrigDocumentDiscountableAmt;
						if (totalDocumentDiscountAmt != 0m && totalDocumentDiscountableAmt != 0m)
							totalDocumentDiscountRate = totalDocumentDiscountRate + totalDocumentDiscountAmt / totalDocumentDiscountableAmt;
					}
				}

				TLine oldLine = (TLine)cache.CreateCopy(line);
				TLine oldCurrentLine = (TLine)cache.CreateCopy(currentLine);

				lineAmtFields.OrigDocumentDiscountRate = 1 - RoundDiscountRate(totalOrigDocumentDiscountRate);
				lineAmtFields.DocumentDiscountRate = 1 - RoundDiscountRate(totalDocumentDiscountRate);

				UpdateListOfDiscountsAppliedToLine(cache, discountsLinkedToLine, line);

				if (AmountLineFields.GetMapFor(oldLine, cache).DocumentDiscountRate != lineAmtFields.DocumentDiscountRate ||
					AmountLineFields.GetMapFor(oldLine, cache).OrigDocumentDiscountRate != lineAmtFields.OrigDocumentDiscountRate)
				{
					RecalcTaxes(cache, line, oldLine, oldCurrentLine);
					if (forceFormulaCalculation)
					{
						cache.RaiseRowUpdated(line, oldLine);
					}
				}
			}
		}


		[Obsolete]
		public virtual void CalculateDocumentDiscountRate(
			PXCache cache,
			PXSelectBase<TLine> lines,
			List<TLine> documentDetailsList,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			decimal lineTotal,
			decimal discountTotal,
			string docType = null,
			string docNbr = null,
			bool alwaysRecalc = false,
			bool overrideDiscountTotal = false,
			bool skipParameters = false)
		{
			List<TLine> documentDetails = documentDetailsList ?? GetDocumentDetails(cache, lines, docType != null && docNbr != null ? new object[] { docType, docNbr } : null);
			Func<string, List<TDiscountDetail>> getDiscountDetailsFor =
				discountType => GetDiscountDetailsByType(
					cache,
					discountDetails,
					discountType,
					skipParameters
						? null
						: (docType != null && docNbr != null
							? new object[] { docType, docNbr, discountType }
							: new object[] { discountType }));

			decimal totalGroupDiscountAmt =
				getDiscountDetailsFor(DiscountType.Group).Where(d => d.SkipDiscount != true).Sum(d => (decimal)d.CuryDiscountAmt);
			decimal totalDocumentDiscountAmt =
				getDiscountDetailsFor(DiscountType.Document).Concat(getDiscountDetailsFor(DiscountType.ExternalDocument)).Where(d => d.SkipDiscount != true).Sum(d => (decimal)d.CuryDiscountAmt);

			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			Decimal fullDiscount =
				Math.Abs(PXCurrencyAttribute.RoundCury(cache, lines, totalGroupDiscountAmt)
						+ PXCurrencyAttribute.RoundCury(cache, lines, totalDocumentDiscountAmt)
						- PXCurrencyAttribute.RoundCury(cache, lines, discountTotal));
			if (fullDiscount <= 0.000005m && (totalGroupDiscountAmt != 0m || totalDocumentDiscountAmt != 0m || discountTotal != 0m) && !alwaysRecalc)
				return;

			if (overrideDiscountTotal)
			{
				totalGroupDiscountAmt = 0m;
				totalDocumentDiscountAmt = discountTotal;
			}

			decimal updLineTotal = 0m;
			foreach (TLine line in documentDetails)
			{
				AmountLineFields lineAmtFields = AmountLineFields.GetMapFor(line, cache);
				DiscountLineFields discountFields = DiscountLineFields.GetMapFor(line, cache);
				if (discountFields.DiscountID == null
					|| cachedDiscountTypes.ContainsKey(discountFields.DiscountID) && cachedDiscountTypes[discountFields.DiscountID].ExcludeFromDiscountableAmt == false)
				{
					updLineTotal += (decimal)lineAmtFields.CuryLineAmount;
				}
			}
			updLineTotal = updLineTotal - totalGroupDiscountAmt;

			foreach (TLine line in documentDetails)
			{
				AmountLineFields lineAmtFields = AmountLineFields.GetMapFor(line, cache);
				DiscountLineFields discountFields = DiscountLineFields.GetMapFor(line, cache);

				TLine oldLine = (TLine)cache.CreateCopy(line);
				TLine oldCurrentLine = (TLine)cache.CreateCopy(currentLine);

				decimal? documentDiscountRate;
				if (updLineTotal != 0m && totalDocumentDiscountAmt != 0m && lineAmtFields.CuryLineAmount != 0m
					&& (discountFields.DiscountID == null || cachedDiscountTypes.ContainsKey(discountFields.DiscountID) && !cachedDiscountTypes[discountFields.DiscountID].ExcludeFromDiscountableAmt))
					documentDiscountRate = 1 - (decimal)lineAmtFields.CuryLineAmount * totalDocumentDiscountAmt / updLineTotal / (decimal)lineAmtFields.CuryLineAmount;
				else
					documentDiscountRate = 1;

				lineAmtFields.DocumentDiscountRate = RoundDiscountRate(documentDiscountRate ?? 1m);

				if (totalGroupDiscountAmt == 0m)
					lineAmtFields.GroupDiscountRate = 1;

				TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines = GetListOfLinksBetweenDiscountsAndDocumentLines(cache, documentDetails, GetDiscountDetailsByType(cache, discountDetails, null));
				//TwoWayLookup<TDiscountDetail, TLine> discountsLinkedToLine = GetListOfDiscountsAppliedToLine(cache, line, GetDiscountDetailsByType(cache, discountDetails, null));
				UpdateListOfDiscountsAppliedToLine(cache, discountCodesWithApplicableLines, line);

				if (AmountLineFields.GetMapFor(oldLine, cache).DocumentDiscountRate != lineAmtFields.DocumentDiscountRate ||
					AmountLineFields.GetMapFor(oldLine, cache).OrigDocumentDiscountRate != lineAmtFields.OrigDocumentDiscountRate)
					RecalcTaxes(cache, line, oldLine, oldCurrentLine);
			}
		}

		#endregion

		protected virtual void UpdateListOfDiscountsAppliedToLine(
			PXCache cache,
			TwoWayLookup<TDiscountDetail, TLine> discountCodesWithApplicableLines,
			TLine line)
		{
			DiscountLineFields discountFields = DiscountLineFields.GetMapFor(line, cache);

			var discountLineNbrs = from dDetail in discountCodesWithApplicableLines.LeftsFor(line)
								   where (dDetail.CuryDiscountableAmt != null && dDetail.CuryDiscountableAmt > 0) || (dDetail.DiscountableQty != null && dDetail.DiscountableQty > 0)
								   select dDetail.LineNbr;


			discountFields.DiscountsAppliedToLine = discountLineNbrs.Where(x => x.HasValue).Select(x => x.Value).ToArray();

			//To check: the code below might not be needed
			foreach (TDiscountDetail discountDetail in discountCodesWithApplicableLines.LeftValues.Where(x => x.LineNbr != null && (x.IsOrigDocDiscount != true && x.Type == DiscountType.Document || x.Type == DiscountType.ExternalDocument)))
			{
				if (!discountFields.DiscountsAppliedToLine.Contains(discountDetail.LineNbr ?? 0))
					discountFields.DiscountsAppliedToLine = discountFields.DiscountsAppliedToLine.Concat(new[] { discountDetail.LineNbr ?? 0 }).ToArray();
			}
		}

		private void RecalcTaxes(PXCache cache, TLine line, TLine oldLine, TLine oldCurrentLine)
		{
			if (!CompareRows(oldLine, oldCurrentLine))
				TX.TaxBaseAttribute.Calculate<AmountLineFields.taxCategoryID>(cache, new PXRowUpdatedEventArgs(line, oldLine, false));
			cache.MarkUpdated(line);
		}

		#endregion

		/// <summary>
		/// Returns new DiscountDetail line
		/// </summary>
		protected virtual TDiscountDetail CreateDiscountDetailsLine(
			PXCache cache,
			DiscountDetailLine discount,
			DiscountLineFields discountedLine,
			decimal curyDiscountedLineAmount,
			decimal discountedLineQty,
			DateTime date,
			string type)
		{
			var newDiscountDetail =
				new TDiscountDetail
				{
					DiscountID = discount.DiscountID,
					DiscountSequenceID = discount.DiscountSequenceID,
					Type = type,
					CuryDiscountableAmt = curyDiscountedLineAmount,
					DiscountableQty = discountedLineQty
				};

			decimal rawValue = CalculateDiscount(cache, discount, discountedLine, curyDiscountedLineAmount, discountedLineQty, date, type);
			newDiscountDetail.CuryDiscountAmt = PXDBCurrencyAttribute.Round(cache, newDiscountDetail, rawValue, CMPrecision.TRANCURY);

			if (discount.DiscountedFor == DiscountOption.Percent)
			{
				newDiscountDetail.DiscountPct = discount.Discount;
			}
			else if (!newDiscountDetail.CuryDiscountableAmt.IsNullOrZero())
			{
				newDiscountDetail.DiscountPct = newDiscountDetail.CuryDiscountAmt / newDiscountDetail.CuryDiscountableAmt * 100;
			}

			newDiscountDetail.FreeItemID = discount.freeItemID;
			newDiscountDetail.FreeItemQty = CalculateFreeItemQuantity(cache, discount, discountedLine, curyDiscountedLineAmount, discountedLineQty, date, type);
			newDiscountDetail.ExtDiscCode = discount.ExtDiscCode;
			newDiscountDetail.Description = discount.Description;

			return newDiscountDetail;
		}

		/// <summary>
		/// Creates new list of Discount Details lines.
		/// </summary>
		protected virtual bool CreateNewListOfDiscountDetails(
			PXCache cache,
			DiscountLineFields discountedLine,
			List<DiscountDetailLine> discounts,
			DateTime date,
			Dictionary<DiscountSequenceKey, TDiscountDetail> newDiscountDetails,
			decimal discountedLineAmount,
			decimal discountedLineQty)
		{
			bool skipDocumentDiscounts = false;
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			foreach (DiscountDetailLine discount in discounts.Where(d => d.DiscountID != null))
			{
				//skip document discounts
				if (cachedDiscountTypes[discount.DiscountID].SkipDocumentDiscounts)
					skipDocumentDiscounts = true;

				var newDiscountDetail = CreateDiscountDetailsLine(cache, discount, discountedLine, discountedLineAmount, discountedLineQty, date, DiscountType.Group);

				var discountSequence = new DiscountSequenceKey(newDiscountDetail.DiscountID, newDiscountDetail.DiscountSequenceID);

				//review
				if (!newDiscountDetails.ContainsKey(discountSequence))
				{
					newDiscountDetails.Add(discountSequence, newDiscountDetail);
				}
				else
				{
					newDiscountDetails[discountSequence].CuryDiscountableAmt = null;
					newDiscountDetails[discountSequence].DiscountableQty = null;
					newDiscountDetails[discountSequence].DiscountPct = null;
					newDiscountDetails[discountSequence].CuryDiscountAmt += newDiscountDetail.CuryDiscountAmt;
				}
			}
			return skipDocumentDiscounts;
		}

		#endregion

		#region Document Discounts

		//Collects Document Details lines, discountedLineAmount and calculates document discount
		protected virtual void SetDocumentDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			PXSelectBase<TDiscountDetail> discountDetails,
			TLine currentLine,
			int? branchID,
			int? locationID,
			DateTime date,
			DiscountCalculationOptions discountCalculationOptions,
			RecalcDiscountsParamFilter recalcFilter = null)
		{
			if (!IsDiscountCalculationNeeded(cache, currentLine, DiscountType.Document)) return;

			List<TLine> documentDetails = GetDocumentDetails(cache, lines);
			decimal totalLineAmount;
			decimal curyTotalLineAmount;
			SumAmounts(cache, documentDetails, out totalLineAmount, out curyTotalLineAmount);
			if (documentDetails.Count != 0)
			{
				SetDocumentDiscount(
					cache,
					documentDetails.First(),
					GetDiscountEntitiesDiscounts(cache, documentDetails.First(), locationID, false, branchID),
					totalLineAmount,
					curyTotalLineAmount,
					discountDetails,
					date,
					discountCalculationOptions,
					recalcFilter);
			}
			else
			{
				RemoveUnapplicableDiscountDetails(cache, discountDetails, null, DiscountType.Document, recalcFilter: recalcFilter);
			}
			CalculateDocumentDiscountRate(cache, documentDetails, currentLine, discountDetails);
		}

		//Calculates document discount
		protected virtual void SetDocumentDiscount(
			PXCache cache,
			TLine line,
			HashSet<KeyValuePair<object, string>> entities,
			decimal totalLineAmount,
			decimal curyTotalLineAmount,
			PXSelectBase<TDiscountDetail> discountDetails,
			DateTime date,
			DiscountCalculationOptions discountCalculationOptions,
			RecalcDiscountsParamFilter recalcFilter = null)
		{
			GetDiscountTypes();
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			List<TDiscountDetail> groupDiscountDetails = GetDiscountDetailsByType(cache, discountDetails, DiscountType.Group);

			bool skipAutomaticDocumentDiscounts = false;
			if (groupDiscountDetails.Any(d => d.SkipDiscount != true && cachedDiscountTypes[d.DiscountID].SkipDocumentDiscounts))
			{
				RemoveUnapplicableDiscountDetails(cache, discountDetails, new List<DiscountSequenceKey>(), DiscountType.Document, recalcFilter: recalcFilter);
				skipAutomaticDocumentDiscounts = true;
			}

			decimal totalGroupDiscountAmount;
			decimal curyTotalGroupDiscountAmount;
			GetDiscountAmountByType(discountDetails.Cache, groupDiscountDetails, DiscountType.Group, out totalGroupDiscountAmount, out curyTotalGroupDiscountAmount);

			decimal curyDocumentDiscountableAmount = curyTotalLineAmount - curyTotalGroupDiscountAmount;

			LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(line, cache);
			decimal discountLimit = GetDiscountLimit(cache, lineEntities.CustomerID, lineEntities.VendorID);
			List<DiscountDetailLine> documentDiscounts = new List<DiscountDetailLine>();
			if (curyTotalLineAmount / 100 * discountLimit > curyTotalGroupDiscountAmount || discountLimit == 0m)
			{
				if (curyTotalLineAmount == 0) return;

				var newDiscountDetails = new List<TDiscountDetail>();
				decimal totalDocumentDiscountAmount = 0m;

				if (!skipAutomaticDocumentDiscounts)
				{
					//Adding one best automatic document discount
					documentDiscounts.Add(SelectBestDiscount(cache, DiscountLineFields.GetMapFor(line, cache), entities, DiscountType.Document, curyDocumentDiscountableAmount, 0, date));

					//If true, all document discounts will be recalculated regardless of DisableAllAutomaticDiscounts option
					bool recalculateAllFromAction = recalcFilter != null && recalcFilter.UseRecalcFilter == true && recalcFilter.OverrideManualDocGroupDiscounts == true;

					if (!recalculateAllFromAction)
					{
						//Adding applicable manual discounts
						List<TDiscountDetail> manualDiscounts = CollectManualDiscounts(cache, discountDetails, DiscountType.Document);

						foreach (TDiscountDetail manualDiscount in manualDiscounts)
						{
							if (manualDiscount.IsOrigDocDiscount == true)
								continue;
							
							//Checking if manual document discount is still applicable
							HashSet<DiscountSequenceKey> applicableDocumentDiscounts = SelectApplicableEntityDiscounts(entities, DiscountType.Document, false);
							DiscountDetailLine newManualDiscount = SelectApplicableDiscount(
								cache,
								DiscountLineFields.GetMapFor(line, cache),
								new DiscountSequenceKey(manualDiscount.DiscountID, manualDiscount.DiscountSequenceID),
								curyDocumentDiscountableAmount,
								0,
								DiscountType.Document,
								date);

							if (applicableDocumentDiscounts.Count(x => x.DiscountID == newManualDiscount.DiscountID && x.DiscountSequenceID == newManualDiscount.DiscountSequenceID) != 0
								&& newManualDiscount.DiscountID == manualDiscount.DiscountID && newManualDiscount.DiscountSequenceID == manualDiscount.DiscountSequenceID)
							{
								newManualDiscount.Discount = manualDiscount.CuryDiscountAmt;
								newManualDiscount.DiscountedFor = DiscountOption.Amount;
								documentDiscounts.Add(newManualDiscount);
							}
						}
					}

					List<DiscountSequenceKey> dKeys = documentDiscounts
						.Where(ds => ds.DiscountID != null && ds.DiscountSequenceID != null)
						.Select(ds => new DiscountSequenceKey(ds.DiscountID, ds.DiscountSequenceID))
						.ToList();
					RemoveUnapplicableDiscountDetails(cache, discountDetails, dKeys, DiscountType.Document, recalcFilter: recalcFilter);

					//Only discounts that already exist will be recalculated when DisableAllAutomaticDiscounts is enabled. New discounts will not be added.
					if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisableAllAutomaticDiscounts) && !recalculateAllFromAction)
					{
						List<TDiscountDetail> trace = GetDiscountDetailsByType(cache, discountDetails, DiscountType.Document);

						if (trace.Count == 0)
							documentDiscounts.Clear();

						if (trace.Count > 0 && documentDiscounts.Count > 0)
							documentDiscounts = (from n1 in documentDiscounts
												 join n2 in trace
												 on new { n1.DiscountID, n1.DiscountSequenceID }
												 equals new { n2.DiscountID, n2.DiscountSequenceID }
												 select n1).ToList();
					}

					//Create new discount details for internal discounts
					foreach (DiscountDetailLine documentDiscount in documentDiscounts)
					{
						if (documentDiscount.DiscountID != null)
						{
							TDiscountDetail newDiscountDetail = CreateDiscountDetailsLine(cache, documentDiscount, DiscountLineFields.GetMapFor(line, cache), curyDocumentDiscountableAmount, 0, date, DiscountType.Document);
							totalDocumentDiscountAmount += newDiscountDetail.CuryDiscountAmt ?? 0m;
							if (discountCalculationOptions.HasFlag(DiscountCalculationOptions.DisableAllAutomaticDiscounts) && recalculateAllFromAction)
								newDiscountDetail.IsManual = true;
							newDiscountDetails.Add(newDiscountDetail);
						}
					}
				}

				//Add all external discounts
				List<TDiscountDetail> externalDocumentDiscounts = GetDiscountDetailsByType(cache, discountDetails, DiscountType.ExternalDocument);
				foreach (TDiscountDetail externalDocumentDiscount in externalDocumentDiscounts)
				{
					if (externalDocumentDiscount.IsOrigDocDiscount == true)
						continue;

					externalDocumentDiscount.CuryDiscountableAmt = curyDocumentDiscountableAmount;
					if (curyDocumentDiscountableAmount > 0)
						externalDocumentDiscount.DiscountPct = externalDocumentDiscount.CuryDiscountAmt * 100 / curyDocumentDiscountableAmount;
					newDiscountDetails.Add(externalDocumentDiscount);
				}

				//Insert new discount detail into Discount Details grid
				foreach (TDiscountDetail newDiscountDetail in newDiscountDetails)
				{
					TDiscountDetail dDetail = UpdateInsertDiscountTrace(cache, discountDetails, newDiscountDetail);
					if ((curyTotalLineAmount / 100 * discountLimit) < (totalDocumentDiscountAmount + curyTotalGroupDiscountAmount))
					{
						SetDiscountLimitException(cache, discountDetails, DiscountType.Document, PXMessages.LocalizeFormatNoPrefix(AR.Messages.DocDiscountExceedLimit, discountLimit));
					}
				}
			}
			else if (curyTotalLineAmount != 0 && curyTotalGroupDiscountAmount > 0)
			{
				SetDiscountLimitException(cache, discountDetails, DiscountType.Group, AR.Messages.GroupDiscountExceedLimit);
			}
			else
			{
				RemoveUnapplicableDiscountDetails(cache, discountDetails, null, DiscountType.Document, recalcFilter: recalcFilter);
			}
		}

		private void SetDiscountLimitException(
			PXCache cache,
			PXSelectBase<TDiscountDetail> discountDetails,
			string discountType,
			string message, TDiscountDetail currentDiscountLine = null)
		{
			List<TDiscountDetail> dDetails = GetDiscountDetailsByType(cache, discountDetails, discountType);
			if (dDetails.Count != 0)
			{
				discountDetails.Cache.RaiseExceptionHandling(DiscountID, dDetails[0], null, new PXSetPropertyException(message, PXErrorLevel.Warning));
			}
			else if (currentDiscountLine != null)
			{
				discountDetails.Cache.RaiseExceptionHandling(DiscountID, currentDiscountLine, null, new PXSetPropertyException(message, PXErrorLevel.Warning));
			}
		}
		#endregion

		#region Manual Group/Document Discounts
		/// <summary>
		/// Sets manual group or document discount
		/// </summary>
		protected virtual void SetManualGroupDocDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			TLine currentLine,
			PXSelectBase<TDiscountDetail> discountDetails,
			TDiscountDetail currentDiscountDetailLine,
			string manualDiscountID,
			string manualDiscountSequenceID,
			int? branchID,
			int? locationID,
			DateTime date,
			DiscountCalculationOptions discountCalculationOptions)
		{
			List<TLine> documentDetails = GetDocumentDetails(cache, lines);
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			if (cachedDiscountTypes.ContainsKey(manualDiscountID) && documentDetails.Count != 0)
			{
				bool isManualDiscountApplicable = false;
				bool isManualDiscountApplied = false;
				decimal totalGroupDiscountAmount;
				decimal curyTotalGroupDiscountAmount;
				decimal totalLineAmount;
				decimal curyTotalLineAmount;
				List<TDiscountDetail> discountDetailsByType = GetDiscountDetailsByType(cache, discountDetails, DiscountType.Group);
				GetDiscountAmountByType(discountDetails.Cache, discountDetailsByType, DiscountType.Group, out totalGroupDiscountAmount, out curyTotalGroupDiscountAmount);
				SumAmounts(cache, documentDetails, out totalLineAmount, out curyTotalLineAmount);
				LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(documentDetails.First(), cache);
				decimal discountLimit = GetDiscountLimit(cache, lineEntities.CustomerID, lineEntities.VendorID);

				if (cachedDiscountTypes[manualDiscountID].Type == DiscountType.Group)
				{
					var discountCodesWithLines = new TwoWayLookup<DiscountSequenceKey, TLine>();
					foreach (TLine line in documentDetails)
					{
						HashSet<DiscountSequenceKey> applicableGroupDiscounts = SelectApplicableEntityDiscounts(GetDiscountEntitiesDiscounts(cache, line, locationID, true), cachedDiscountTypes[manualDiscountID].Type, false);

						HashSet<DiscountSequenceKey> selectedGroupDiscounts = applicableGroupDiscounts
							.Where(sk => manualDiscountID == sk.DiscountID && manualDiscountSequenceID.IsIn(null, sk.DiscountSequenceID))
							.ToHashSet();

						if (!isManualDiscountApplicable)
							isManualDiscountApplicable = selectedGroupDiscounts.Any();

						if (!selectedGroupDiscounts.Any())
							continue;

						DiscountLineFields discountedLine = DiscountLineFields.GetMapFor(line, cache);
						bool excludeFromDiscountableAmount = false;
						if (discountedLine.DiscountID != null)
							excludeFromDiscountableAmount = cachedDiscountTypes[discountedLine.DiscountID].ExcludeFromDiscountableAmt;

						if (!excludeFromDiscountableAmount)
						{
							CombineDiscountsAndDocumentLines(cache, line, selectedGroupDiscounts, ref discountCodesWithLines);
						}
					}

					foreach (DiscountSequenceKey applicableGroup in discountCodesWithLines.LeftValues)
					{
						DiscountLineFields discountedLine = DiscountLineFields.GetMapFor(documentDetails[0], cache);
						List<DiscountDetailLine> discountDetailLines = SelectApplicableDiscounts(
							cache,
							discountedLine,
							new HashSet<DiscountSequenceKey> { applicableGroup },
							(decimal)applicableGroup.CuryDiscountableAmount,
							(decimal)applicableGroup.DiscountableQuantity,
							DiscountType.Group,
							date);

						if (discountDetailLines.Count != 0)
						{
							TDiscountDetail newDiscountDetail = CreateDiscountDetailsLine(
								cache,
								discountDetailLines.First(),
								discountedLine,
								(decimal)applicableGroup.CuryDiscountableAmount,
								(decimal)applicableGroup.DiscountableQuantity,
								date,
								DiscountType.Group);
							newDiscountDetail.CuryDiscountableAmt = applicableGroup.CuryDiscountableAmount;
							newDiscountDetail.DiscountableQty = applicableGroup.DiscountableQuantity;
							currentDiscountDetailLine.DiscountSequenceID = newDiscountDetail.DiscountSequenceID;
							currentDiscountDetailLine.Type = newDiscountDetail.Type;
							currentDiscountDetailLine.CuryDiscountableAmt = newDiscountDetail.CuryDiscountableAmt;
							currentDiscountDetailLine.DiscountableQty = newDiscountDetail.DiscountableQty;
							currentDiscountDetailLine.DiscountPct = newDiscountDetail.DiscountPct;
							currentDiscountDetailLine.CuryDiscountAmt = newDiscountDetail.CuryDiscountAmt;
							currentDiscountDetailLine.FreeItemID = newDiscountDetail.FreeItemID;
							currentDiscountDetailLine.FreeItemQty = newDiscountDetail.FreeItemQty;
							isManualDiscountApplied = true;

							if ((curyTotalLineAmount / 100 * discountLimit) < (newDiscountDetail.CuryDiscountAmt + curyTotalGroupDiscountAmount))
							{
								SetDiscountLimitException(cache, discountDetails, DiscountType.Group, PXMessages.LocalizeFormatNoPrefix(AR.Messages.OnlyGroupDiscountExceedLimit, discountLimit), currentDiscountDetailLine);
							}
						}

						if (cachedDiscountTypes[manualDiscountID].SkipDocumentDiscounts)
						{
							RemoveUnapplicableDiscountDetails(cache, discountDetails, null, DiscountType.Document);
						}
					}

					CalculateGroupDiscountRate(cache, documentDetails, currentLine, discountCodesWithLines, discountDetails, true, false);
				}
				if (cachedDiscountTypes[manualDiscountID].Type == DiscountType.Document)
				{
					HashSet<DiscountSequenceKey> applicableDiscounts = SelectApplicableEntityDiscounts(GetDiscountEntitiesDiscounts(cache, documentDetails.First(), locationID, false, branchID), cachedDiscountTypes[manualDiscountID].Type, false);
					HashSet<DiscountSequenceKey> selectedGroupDiscounts = applicableDiscounts.Where(sk => manualDiscountID == sk.DiscountID && manualDiscountSequenceID.IsIn(null, sk.DiscountSequenceID)).ToHashSet();
					isManualDiscountApplicable = selectedGroupDiscounts.Any();
					isManualDiscountApplied = false;

					if (isManualDiscountApplicable)
					{
						if (discountDetailsByType.Any(detail => detail.SkipDiscount != true && cachedDiscountTypes[detail.DiscountID].SkipDocumentDiscounts))
						{
							discountDetails.Cache.RaiseExceptionHandling(DiscountID, currentDiscountDetailLine, manualDiscountID, new PXSetPropertyException(AR.Messages.DocumentDicountCanNotBeAdded, PXErrorLevel.Error));
							return;
						}

						if (curyTotalLineAmount / 100 * discountLimit > curyTotalGroupDiscountAmount || discountLimit == 0m)
						{
							if (curyTotalLineAmount != 0)
							{
								DiscountDetailLine discount = SelectApplicableDiscount(
									cache, DiscountLineFields.GetMapFor(documentDetails.First(), cache), selectedGroupDiscounts, totalLineAmount - totalGroupDiscountAmount, 0, DiscountType.Document, date);

								if (discount.DiscountID != null)
								{
									TDiscountDetail newDiscountDetail = CreateDiscountDetailsLine(
										cache, discount, DiscountLineFields.GetMapFor(documentDetails.First(), cache), curyTotalLineAmount - curyTotalGroupDiscountAmount, 0, date, DiscountType.Document);
									newDiscountDetail.CuryDiscountableAmt = curyTotalLineAmount - curyTotalGroupDiscountAmount;
									currentDiscountDetailLine.DiscountSequenceID = newDiscountDetail.DiscountSequenceID;
									currentDiscountDetailLine.Type = newDiscountDetail.Type;
									currentDiscountDetailLine.CuryDiscountableAmt = newDiscountDetail.CuryDiscountableAmt;
									currentDiscountDetailLine.DiscountableQty = 0;
									currentDiscountDetailLine.DiscountPct = newDiscountDetail.DiscountPct;
									currentDiscountDetailLine.CuryDiscountAmt = newDiscountDetail.CuryDiscountAmt;
									currentDiscountDetailLine.FreeItemID = null;
									currentDiscountDetailLine.FreeItemQty = 0;
									isManualDiscountApplied = true;

									if (curyTotalLineAmount / 100 * discountLimit < newDiscountDetail.CuryDiscountAmt + curyTotalGroupDiscountAmount)
									{
										SetDiscountLimitException(cache, discountDetails, DiscountType.Document, PXMessages.LocalizeFormatNoPrefix(AR.Messages.DocDiscountExceedLimit, discountLimit), currentDiscountDetailLine);
									}
								}
								else
								{
									RemoveUnapplicableDiscountDetails(cache, discountDetails, null, DiscountType.Document);
								}
							}
						}
						else
						{
							SetDiscountLimitException(cache, discountDetails, DiscountType.Group, AR.Messages.GroupDiscountExceedLimit);
						}
						CalculateDocumentDiscountRate(cache, documentDetails, currentLine, discountDetails);
					}
				}

				if (!isManualDiscountApplicable || !isManualDiscountApplied)
				{
					if (manualDiscountID != null && manualDiscountSequenceID == null)
						discountDetails.Cache.RaiseExceptionHandling(DiscountID, currentDiscountDetailLine, manualDiscountID, new PXSetPropertyException(AR.Messages.NoApplicableSequenceFound, PXErrorLevel.Error));
					if (manualDiscountID != null && manualDiscountSequenceID != null)
					{
						currentDiscountDetailLine.DiscountSequenceID = null;
						discountDetails.Cache.RaiseExceptionHandling(DiscountSequenceID, currentDiscountDetailLine, null, new PXSetPropertyException(AR.Messages.UnapplicableSequence, PXErrorLevel.Error, manualDiscountSequenceID));
					}
				}
			}
		}

		/// <summary>
		/// Sets or updates Discount Total. Adds new line to DocumentDetails if needed.
		/// </summary>
		/// <returns>Returns true if DocumentDiscountRate has been updated. Totals should be recalculated in this case.</returns>
		public virtual void SetTotalDocDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			PXSelectBase<TDiscountDetail> discountDetails,
			decimal? curyDiscountTotal,
			DiscountCalculationOptions discountCalculationOptions)
		{
			if (curyDiscountTotal == null || IsDiscountFeatureEnabled(discountCalculationOptions))
				return;

			List<TLine> documentDetails = GetDocumentDetails(cache, lines);
			foreach (TDiscountDetail discountDetail in GetDiscountDetailsByType(cache, discountDetails, null))
			{
				DeleteDiscountDetail(cache, discountDetails, discountDetail);
			}

			TDiscountDetail newDiscountTotalDetail = new TDiscountDetail
			{
				CuryDiscountAmt = curyDiscountTotal,
				Type = DiscountType.ExternalDocument,
				Description = "Discount Total Adjustment"
			};
			InsertDiscountDetail(cache, discountDetails, newDiscountTotalDetail, false);
		}

		/// <summary>
		/// Sets or updates external document discount
		/// </summary>
		/// <returns>Returns true if DocumentDiscountRate has been updated. Totals should be recalculated in this case.</returns>
		public virtual bool SetExternalManualDocDiscount(
			PXCache cache,
			PXSelectBase<TLine> lines,
			PXSelectBase<TDiscountDetail> discountDetails,
			TDiscountDetail currentDiscountDetailLine,
			TDiscountDetail oldDiscountDetailLine,
			DiscountCalculationOptions discountCalculationOptions)
		{
			List<TLine> documentDetails = GetDocumentDetails(cache, lines);

			if (documentDetails.Count == 0)
				return false;

			if (oldDiscountDetailLine == null)
				oldDiscountDetailLine = new TDiscountDetail();

			if (!((currentDiscountDetailLine.DiscountPct != oldDiscountDetailLine.DiscountPct ||
				currentDiscountDetailLine.CuryDiscountAmt != oldDiscountDetailLine.CuryDiscountAmt)
				&& (currentDiscountDetailLine.Type == DiscountType.ExternalDocument
				|| currentDiscountDetailLine.Type == DiscountType.Document
				|| currentDiscountDetailLine.Type == null)
				&& currentDiscountDetailLine.IsOrigDocDiscount != true))
			{
				return false;
			}

			decimal totalGroupDiscountAmount;
			decimal curyTotalGroupDiscountAmount;

			decimal totalDiscountAmount;
			decimal curyTotalDiscountAmount;

			decimal totalLineAmount;
			decimal curyTotalLineAmount;

			GetDiscountAmountByType(discountDetails.Cache, GetDiscountDetailsByType(cache, discountDetails, DiscountType.Group), DiscountType.Group, out totalGroupDiscountAmount, out curyTotalGroupDiscountAmount);
			GetDiscountAmountByType(discountDetails.Cache, GetDiscountDetailsByType(cache, discountDetails, null), null, out totalDiscountAmount, out curyTotalDiscountAmount);

			SumAmounts(cache, documentDetails, out totalLineAmount, out curyTotalLineAmount);

			if (currentDiscountDetailLine.DiscountID == null)
			{  
				currentDiscountDetailLine.Type = DiscountType.ExternalDocument;
				currentDiscountDetailLine.CuryDiscountableAmt = curyTotalLineAmount - curyTotalGroupDiscountAmount;
			}

			currentDiscountDetailLine.IsManual = true;

			if ((cache.Graph.IsCopyPasteContext || currentDiscountDetailLine.CuryDiscountAmt != oldDiscountDetailLine.CuryDiscountAmt) && 
				currentDiscountDetailLine.CuryDiscountAmt != null && currentDiscountDetailLine.CuryDiscountableAmt != 0m &&
				(oldDiscountDetailLine.CuryDiscountAmt != null || currentDiscountDetailLine.CuryDiscountAmt > 0m))
				currentDiscountDetailLine.DiscountPct = Math.Round(((currentDiscountDetailLine.CuryDiscountAmt ?? 0m) * 100 / currentDiscountDetailLine.CuryDiscountableAmt) ?? 0m, 2, MidpointRounding.AwayFromZero);
			else if (currentDiscountDetailLine.DiscountPct != oldDiscountDetailLine.DiscountPct && currentDiscountDetailLine.DiscountPct != null)
				currentDiscountDetailLine.CuryDiscountAmt = (currentDiscountDetailLine.CuryDiscountableAmt ?? 0m) / 100 * (currentDiscountDetailLine.DiscountPct ?? 0m);

			CalculateDocumentDiscountRate(cache, documentDetails, null, discountDetails);

			LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(documentDetails.First(), cache);
			decimal discountLimit = GetDiscountLimit(cache, lineEntities.CustomerID, lineEntities.VendorID);

			if (curyTotalLineAmount / 100 * discountLimit < currentDiscountDetailLine.CuryDiscountAmt + curyTotalDiscountAmount)
			{
				SetDiscountLimitException(cache, discountDetails, DiscountType.Group, PXMessages.LocalizeFormatNoPrefix(AR.Messages.OnlyGroupDiscountExceedLimit, discountLimit), currentDiscountDetailLine);
			}

			return true;
		}

		#endregion

		#region Prices

		protected virtual UnitPriceVal GetUnitPrice(PXCache cache, TLine line, int? locationID, string curyID, DateTime date)
		{
			ARSetup arsetup = ARSetupSelect.Select(cache.Graph);

			if (arsetup.ApplyLineDiscountsIfCustomerPriceDefined == false || arsetup.ApplyLineDiscountsIfCustomerClassPriceDefined == false)
			{
				AmountLineFields aFields = AmountLineFields.GetMapFor(line, cache);
				LineEntitiesFields eFields = LineEntitiesFields.GetMapFor(line, cache);

				UnitPriceVal unitPriceVal = new UnitPriceVal();
				if (eFields.CustomerID != null && eFields.InventoryID != null)
				{
					string customerPriceClassID = GetCustomerPriceClassID(cache, eFields.CustomerID, locationID);
					var salesPrice = GetSalesPrice(cache, 
						(int)eFields.InventoryID, eFields.SiteID, (int)eFields.CustomerID, customerPriceClassID, curyID, aFields.UOM, (decimal)aFields.Quantity, date,
						aFields.HaveBaseQuantity);
					if (salesPrice == null) return unitPriceVal;

					unitPriceVal.CuryUnitPrice = salesPrice.Price;
					unitPriceVal.isBAccountSpecific = salesPrice.PriceType == PriceTypes.Customer;
					unitPriceVal.isPriceClassSpecific = salesPrice.PriceType == PriceTypes.CustomerPriceClass;
					unitPriceVal.isPromotional = salesPrice.IsPromotionalPrice;
					unitPriceVal.skipLineDiscount = arsetup.ApplyLineDiscountsIfCustomerPriceDefined == false && unitPriceVal.isBAccountSpecific
													|| arsetup.ApplyLineDiscountsIfCustomerClassPriceDefined == false && unitPriceVal.isPriceClassSpecific;
				}
				return unitPriceVal;
			}
			return new UnitPriceVal(false);
		}

		protected virtual void SetUnitPrice(PXCache cache, TLine line, UnitPriceVal unitPriceVal)
		{
			AmountLineFields aFields = AmountLineFields.GetMapFor(line, cache);
			if (unitPriceVal.CuryUnitPrice != null)
			{
				decimal? oldCuryUnitPrice = aFields.CuryUnitPrice;
				aFields.CuryUnitPrice = unitPriceVal.CuryUnitPrice;
				aFields.RaiseFieldUpdated<AmountLineFields.curyUnitPrice>(oldCuryUnitPrice);
			}
		}
		#endregion

		#region Total Discount Amount and total Free Item quantity + prorate
		/// <summary>
		/// Calculates total discount amount. Prorates Amount discounts if needed.
		/// </summary>
		/// <returns>Returns total CuryDiscountAmt</returns>
		protected virtual decimal CalculateDiscount(PXCache cache, DiscountDetailLine discount, DiscountLineFields dLine, decimal curyAmount, decimal quantity, DateTime date, string type)
		{
			decimal totalDiscount = 0m;

			if (discount.DiscountedFor == DiscountOption.Amount)
			{
				if ((bool)discount.Prorate && discount.AmountFrom != null && discount.AmountFrom != 0m)
				{
					DiscountDetailLine intDiscount = discount;
					decimal intCuryLineAmount = curyAmount;
					decimal intLineQty = quantity;
					totalDiscount = 0m;

					var discountSequence = new DiscountSequenceKey(discount.DiscountID, discount.DiscountSequenceID);
					do
					{
						if (discount.BreakBy == BreakdownType.Amount)
						{
							if (intCuryLineAmount < (intDiscount.AmountFrom ?? 0m))
							{
								intDiscount = SelectApplicableDiscount(cache, dLine, discountSequence, intCuryLineAmount, intLineQty, type, date);
								if (intDiscount.DiscountID != null)
								{
									totalDiscount += intDiscount.Discount ?? 0m;
									intCuryLineAmount -= intDiscount.AmountFrom ?? 0m;
								}
								else
								{
									intDiscount = new DiscountDetailLine();
								}
							}
							else
							{
								totalDiscount += intDiscount.Discount ?? 0m;
								intCuryLineAmount -= intDiscount.AmountFrom ?? 0m;
							}

						}
						else
						{
							if (intLineQty < (intDiscount.AmountFrom ?? 0m))
							{
								intDiscount = SelectApplicableDiscount(cache, dLine, discountSequence, intCuryLineAmount, intLineQty, type, date);
								if (intDiscount.DiscountID != null)
								{
									totalDiscount += intDiscount.Discount ?? 0m;
									intLineQty -= intDiscount.AmountFrom ?? 0m;
								}
								else
								{
									intDiscount = new DiscountDetailLine();
								}
							}
							else
							{
								totalDiscount += intDiscount.Discount ?? 0m;
								intLineQty -= intDiscount.AmountFrom ?? 0m;
							}

						}
						if (intDiscount.AmountFrom == 0m) intDiscount = new DiscountDetailLine();
					}
					while (intDiscount.DiscountID != null);
				}
				else
				{
					totalDiscount = discount.Discount ?? 0m;
				}
			}
			else if (discount.DiscountedFor == DiscountOption.Percent)
			{
				totalDiscount = curyAmount / 100 * (discount.Discount ?? 0m);
			}
			return totalDiscount;
		}

		/// <summary>
		/// Calculates total free item quantity. Prorates Free-Item discounts if needed.
		/// </summary>
		/// <returns>Returns total FreeItemQty</returns>
		protected virtual decimal CalculateFreeItemQuantity(PXCache cache, DiscountDetailLine discount, DiscountLineFields dLine, decimal curyAmount, decimal quantity, DateTime date, string type)
		{
			decimal totalFreeItems = 0m;

			if (discount.DiscountedFor == DiscountOption.FreeItem)
			{
				if ((bool)discount.Prorate && discount.AmountFrom != null && discount.AmountFrom != 0m)
				{
					DiscountDetailLine intDiscount = discount;
					decimal intCuryLineAmount = curyAmount;
					decimal intLineQty = quantity;
					totalFreeItems = 0m;

					var discountSequence = new DiscountSequenceKey(discount.DiscountID, discount.DiscountSequenceID);
					do
					{
						if (discount.BreakBy == BreakdownType.Amount)
						{
							if (intCuryLineAmount < (intDiscount.AmountFrom ?? 0m))
							{
								intDiscount = SelectApplicableDiscount(cache, dLine, discountSequence, intCuryLineAmount, intLineQty, type, date);
								if (intDiscount.DiscountID != null)
								{
									totalFreeItems += intDiscount.freeItemQty ?? 0m;
									intCuryLineAmount -= intDiscount.AmountFrom ?? 0m;
								}
								else
								{
									intDiscount = new DiscountDetailLine();
								}
							}
							else
							{
								totalFreeItems += intDiscount.freeItemQty ?? 0m;
								intCuryLineAmount -= intDiscount.AmountFrom ?? 0m;
							}
						}
						else
						{
							if (intLineQty < (intDiscount.AmountFrom ?? 0m))
							{
								intDiscount = SelectApplicableDiscount(cache, dLine, discountSequence, intCuryLineAmount, intLineQty, type, date);
								if (intDiscount.DiscountID != null)
								{
									totalFreeItems += intDiscount.freeItemQty ?? 0m;
									intLineQty -= intDiscount.AmountFrom ?? 0m;
								}
								else
								{
									intDiscount = new DiscountDetailLine();
								}
							}
							else
							{
								totalFreeItems += intDiscount.freeItemQty ?? 0m;
								intLineQty -= intDiscount.AmountFrom ?? 0m;
							}
						}
						if (intDiscount.AmountFrom == 0m) intDiscount = new DiscountDetailLine();
					}
					while (intDiscount.DiscountID != null);
				}
				else
				{
					totalFreeItems = discount.freeItemQty ?? 0m;
				}
			}
			return totalFreeItems;
		}
		#endregion

		#region Utils
		/// <summary>
		/// Checks if discount calculation needed for the given combination of entity line and discount type
		/// </summary>
		protected virtual bool IsDiscountCalculationNeeded(PXCache cache, TLine line, string discountType)
		{
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();
			if (cachedDiscountTypes.Count == 0) return false;

			LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(line, cache);
			switch (discountType)
			{
				case DiscountType.Line:
					DiscountLineFields lineDiscountFields = DiscountLineFields.GetMapFor(line, cache);
					if (lineDiscountFields.DiscountID != null)
						return true;
					break;
				case DiscountType.Group:
					AmountLineFields grLineAmountsFields = AmountLineFields.GetMapFor(line, cache);
					if (grLineAmountsFields.GroupDiscountRate != 1m)
						return true;
					break;
				case DiscountType.Document:
					AmountLineFields docLineAmountsFields = AmountLineFields.GetMapFor(line, cache);
					if (docLineAmountsFields.DocumentDiscountRate != 1m)
						return true;
					break;
			}

			if (discountType == DiscountType.Line)
			{
				DiscountLineFields lineDiscountFields = DiscountLineFields.GetMapFor(line, cache);
				if (lineDiscountFields.DiscountID != null)
					return true;
			}

			foreach (DiscountCode discountCode in cachedDiscountTypes.ValuesExt())
			{
				if (lineEntities.VendorID != null && lineEntities.CustomerID == null)
				{
					if (discountCode.IsVendorDiscount && discountCode.VendorID == lineEntities.VendorID && discountCode.Type == discountType)
						return true;
				}
				else if (discountCode.IsVendorDiscount != true && discountCode.Type == discountType)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns total discount amount by discount type
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="discountDetails"></param>
		/// <param name="type">Set specific discount tyoe or set to null to get Discount Total</param>
		/// <param name="totalDiscountAmount"></param>
		/// <param name="curyTotalDiscountAmt"></param>
		/// <typeparam name="TDiscountDetail"></typeparam>
		protected virtual void GetDiscountAmountByType(PXCache cache, List<TDiscountDetail> discountDetails, string type, out decimal totalDiscountAmount, out decimal curyTotalDiscountAmt)
		{
			totalDiscountAmount = 0m;
			curyTotalDiscountAmt = 0m;
			foreach (TDiscountDetail detail in discountDetails)
			{
				if ((detail.Type == type || type == null) && detail.SkipDiscount != true && detail.IsOrigDocDiscount != true)
				{
					curyTotalDiscountAmt += detail.CuryDiscountAmt ?? 0;
					decimal baseDiscountAmt;
					PXCurrencyAttribute.CuryConvBase(cache, detail, detail.CuryDiscountAmt ?? 0m, out baseDiscountAmt, true);
					totalDiscountAmount += baseDiscountAmt;
				}
			}
		}

		//Collect manual Discounts
		protected virtual List<TDiscountDetail> CollectManualDiscounts(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, string type)
		{
			List<TDiscountDetail> manualDiscounts = GetDiscountDetailsByType(cache, discountDetails, type).Where(x => x.IsManual == true).ToList();
			return manualDiscounts;
		}

		/// <summary>
		/// Removes manual discounts from allApplicableDiscounts. Manual discounts that already applied to the document will be retained.
		/// </summary>
		protected virtual HashSet<DiscountSequenceKey> RemoveManualDiscounts(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, HashSet<DiscountSequenceKey> allApplicableDiscounts, string type)
		{
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();
			List<TDiscountDetail> trace = GetDiscountDetailsByType(cache, discountDetails, type);
			HashSet<DiscountSequenceKey> applicableDiscounts = new HashSet<DiscountSequenceKey>();
			foreach (DiscountSequenceKey discountSequence in allApplicableDiscounts)
			{
				if (cachedDiscountTypes[discountSequence.DiscountID].IsManual)
				{
					foreach (TDiscountDetail discountDetail in trace)
					{
						var discountSequenceKey = new DiscountSequenceKey(discountDetail.DiscountID, discountDetail.DiscountSequenceID);
						if (discountSequence.Equals(discountSequenceKey) && cachedDiscountTypes[discountDetail.DiscountID].IsManual && discountDetail.IsOrigDocDiscount == false)
						{
							applicableDiscounts.Add(discountSequence);
						}
					}
				}
				else
				{
					applicableDiscounts.Add(discountSequence);
				}
			}
			return applicableDiscounts;
		}

		//Call to remove all unapplicable Discount Details
		protected virtual void RemoveUnapplicableDiscountDetails(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, List<DiscountSequenceKey> newDiscountDetails, string type, bool removeManual = true, RecalcDiscountsParamFilter recalcFilter = null)
		{
			List<TDiscountDetail> trace = GetDiscountDetailsByType(cache, discountDetails, type);
			bool removeOrigDiscounts = recalcFilter != null && recalcFilter.UseRecalcFilter == true && recalcFilter.OverrideManualDocGroupDiscounts == true;

			foreach (TDiscountDetail discountDetail in trace)
			{
				if ((discountDetail.IsManual != false && (discountDetail.IsManual != true || !removeManual)) || 
					(!removeOrigDiscounts && discountDetail.IsOrigDocDiscount == true))
					continue;

				if (newDiscountDetails != null)
				{
					var discountSequence = new DiscountSequenceKey(discountDetail.DiscountID, discountDetail.DiscountSequenceID);
					if (!newDiscountDetails.Contains(discountSequence))
					{
						UpdateUnapplicableDiscountLine(cache, discountDetails, discountDetail);
					}
				}
				else
				{
					UpdateUnapplicableDiscountLine(cache, discountDetails, discountDetail);
				}
			}
		}

		private void UpdateUnapplicableDiscountLine(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, TDiscountDetail discountDetail)
		{
			if (discountDetail.SkipDiscount != true)
				DeleteDiscountDetail(cache, discountDetails, discountDetail);
			else
			{
				discountDetail.CuryDiscountableAmt = 0m;
				discountDetail.CuryDiscountAmt = 0m;
				discountDetail.DiscountableQty = 0m;
				discountDetail.DiscountPct = 0m;
				UpdateDiscountDetail(cache, discountDetails, discountDetail);
			}
		}
		//Updates or inserts Discount Detail
		protected virtual TDiscountDetail UpdateInsertOneDiscountTraceLine(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, TDiscountDetail trace, TDiscountDetail newTrace)
		{
			if (trace == null)
				return InsertDiscountDetail(cache, discountDetails, newTrace);

			trace.CuryDiscountableAmt = newTrace.CuryDiscountableAmt;
			trace.DiscountableQty = newTrace.DiscountableQty;
			trace.CuryDiscountAmt = newTrace.CuryDiscountAmt ?? 0;
			trace.DiscountPct = newTrace.DiscountPct;
			trace.FreeItemID = newTrace.FreeItemID;
			trace.FreeItemQty = newTrace.FreeItemQty;

			return UpdateDiscountDetail(cache, discountDetails, trace);
		}

		//Updates or inserts Discount Details
		protected virtual TDiscountDetail UpdateInsertDiscountTrace(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, TDiscountDetail newTrace)
		{
			TDiscountDetail trace = SearchForExistingDiscountDetail(cache, discountDetails, newTrace.DiscountID, newTrace.DiscountSequenceID, newTrace.Type, newTrace.Type == DiscountType.ExternalDocument ? newTrace.RecordID : null);

			if (trace == null)
				return InsertDiscountDetail(cache, discountDetails, newTrace);

			trace.CuryDiscountableAmt = newTrace.CuryDiscountableAmt;
			trace.DiscountableQty = newTrace.DiscountableQty;
			trace.CuryDiscountAmt = newTrace.CuryDiscountAmt ?? 0m;
			trace.DiscountPct = newTrace.DiscountPct;
			trace.FreeItemID = newTrace.FreeItemID;
			trace.FreeItemQty = newTrace.FreeItemQty;
			if (newTrace.IsManual != null)
				trace.IsManual = newTrace.IsManual;

			return UpdateDiscountDetail(cache, discountDetails, trace);
		}

		//Returns dictionary of discountable entities
		private HashSet<KeyValuePair<object, string>> GetDiscountEntitiesDiscounts(PXCache cache, TLine line, int? locationID, bool isLineOrGroupDiscount, int? branchID = null, int? inventoryID = null, int? customerID = null)
		{
			LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(line, cache);

			HashSet<KeyValuePair<object, string>> entities = new HashSet<KeyValuePair<object, string>>();

			if (lineEntities.VendorID != null && lineEntities.CustomerID == null)
			{
				entities.Add(new KeyValuePair<object, string>(lineEntities.VendorID, DiscountTarget.Vendor));

				if (isLineOrGroupDiscount)
				{
					if (locationID != null)
					{
						entities.Add(new KeyValuePair<object, string>(locationID, DiscountTarget.VendorLocation));
					}

					int? entityInventoryID = lineEntities.InventoryID ?? inventoryID;
					if (entityInventoryID != null)
					{
						entities.Add(new KeyValuePair<object, string>(entityInventoryID, DiscountTarget.Inventory));
						AddTemplateInventoryID(cache, line, entityInventoryID, ref entities);
						string itemPriceClassID = GetInventoryPriceClassID(cache, line, entityInventoryID ?? 0);
						if (itemPriceClassID != null)
						{
							entities.Add(new KeyValuePair<object, string>(itemPriceClassID, DiscountTarget.InventoryPrice));
						}
					}
				}
			}
			else
			{
				int? entityCustomerID = lineEntities.CustomerID ?? customerID;
				if (entityCustomerID != null)
				{
					entities.Add(new KeyValuePair<object, string>(entityCustomerID, DiscountTarget.Customer));
					if (locationID != null)
					{
						string customerPriceClassID = GetCustomerPriceClassID(cache, entityCustomerID, locationID);
						if (customerPriceClassID != null)
						{
							entities.Add(new KeyValuePair<object, string>(customerPriceClassID, DiscountTarget.CustomerPrice));
						}
					}
				}

				int? entityBranchID = lineEntities.BranchID ?? branchID;
				if (entityBranchID != null)
					entities.Add(new KeyValuePair<object, string>(entityBranchID, DiscountTarget.Branch));

				if (isLineOrGroupDiscount)
				{
					int? entityInventoryID = lineEntities.InventoryID ?? inventoryID;
					if (entityInventoryID != null)
					{
						entities.Add(new KeyValuePair<object, string>(entityInventoryID, DiscountTarget.Inventory));
						AddTemplateInventoryID(cache, line, entityInventoryID, ref entities);
						string itemPriceClassID = GetInventoryPriceClassID(cache, line, entityInventoryID ?? 0);
						if (itemPriceClassID != null)
						{
							entities.Add(new KeyValuePair<object, string>(itemPriceClassID, DiscountTarget.InventoryPrice));
						}
					}
					if (lineEntities.SiteID != null)
						entities.Add(new KeyValuePair<object, string>(lineEntities.SiteID, DiscountTarget.Warehouse));
				}
			}
			return entities;
		}

		private void AddTemplateInventoryID(PXCache cache, TLine line, int? entityInventoryID, ref HashSet<KeyValuePair<object, string>> entities)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.matrixItem>())
			{
				InventoryItem item = InventoryItem.PK.Find(cache.Graph, entityInventoryID);
				if (item != null && item.TemplateItemID != null)
				{
					entities.Add(new KeyValuePair<object, string>(item.TemplateItemID, DiscountTarget.Inventory));
				}
			}
		}

		//Returns list of Discount Details by type
		protected virtual List<TDiscountDetail> GetDiscountDetailsByType(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, string type, object[] parameters = null)
		{
			List<TDiscountDetail> discountDetailToReturn = new List<TDiscountDetail>();

			if (parameters == null)
			{
				foreach (TDiscountDetail detail in discountDetails.Select())
				{
					if (detail.Type == type || type == null)
						discountDetailToReturn.Add(detail);
				}
			}
			else
			{
				foreach (TDiscountDetail detail in discountDetails.Select(parameters))
				{
					if (detail.Type == type || type == null)
						discountDetailToReturn.Add(detail);
				}
			}
			return discountDetailToReturn;
		}

		//Returns one Discount Details line
		private TDiscountDetail SearchForExistingDiscountDetail(PXCache sender, PXSelectBase<TDiscountDetail> discountDetails, string discountID, string discountSequenceID, string type, int? recordID)
		{
			foreach (TDiscountDetail discount in discountDetails.Select())
			{
				if (discount.IsOrigDocDiscount != true)
				{
					if (recordID != null)
					{
						if (discount.RecordID == recordID)
							return discount;
					}
					else if (discount.DiscountID == discountID && discount.DiscountSequenceID == discountSequenceID && discount.Type == type)
						return discount;
				}
			}
			return null;
		}

		//Returns list of Document Details lines
		private List<TLine> GetDocumentDetails(PXCache cache, PXSelectBase<TLine> documentDetails, object[] parameters = null)
		{
			return documentDetails.Select(parameters)
				.RowCast<TLine>()
				.Where(detail =>
				{
					DiscountLineFields discountedLine = DiscountLineFields.GetMapFor(detail, cache);
					return discountedLine.IsFree != true && discountedLine.LineType != SOLineType.Discount && discountedLine.LineType != SOLineType.Freight && discountedLine.LineType != APLineType.LandedCostAP;
				})
				.ToList();
		}

		/// <summary>
		/// Sums line amounts. Returns modified totalLineAmt and curyTotalLineAmt
		/// </summary>
		protected virtual void SumAmounts(PXCache cache, List<TLine> lines, out decimal totalLineAmt, out decimal curyTotalLineAmt)
		{
			totalLineAmt = 0m;
			curyTotalLineAmt = 0m;

			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();
			foreach (TLine line in lines)
			{
				bool excludeFromDiscountableAmount = false;
				AmountLineFields item = AmountLineFields.GetMapFor(line, cache);
				DiscountLineFields discountedLine = DiscountLineFields.GetMapFor(line, cache);

				if (discountedLine.LineType == null || discountedLine.LineType != SOLineType.Discount || discountedLine.LineType != APLineType.LandedCostAP)
				{
					if (discountedLine.DiscountID != null)
						excludeFromDiscountableAmount = cachedDiscountTypes[discountedLine.DiscountID].ExcludeFromDiscountableAmt;

					decimal curyLineAmt = !excludeFromDiscountableAmount ? (item.CuryLineAmount ?? 0) : 0;

					decimal baseLineAmt;
					PXCurrencyAttribute.CuryConvBase(cache, item, curyLineAmt, out baseLineAmt, true);
					totalLineAmt += baseLineAmt;
					curyTotalLineAmt += curyLineAmt;
				}
			}
		}

		public virtual void ClearDiscount(PXCache cache, TLine line)
		{
			DiscountLineFields discountLine = DiscountLineFields.GetMapFor(line, cache);
			discountLine.DiscountID = null;
			discountLine.DiscountSequenceID = null;
		}

		protected bool CompareRows<TRow>(TRow row1, TRow row2, params string[] ignore) where TRow : class
		{
			if (row1 != null && row2 != null)
			{
				Type type = typeof(TRow);
				List<string> ignoreList = new List<string>(ignore);
				foreach (System.Reflection.PropertyInfo propertyInfo in
					type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
				{
					if (!ignoreList.Contains(propertyInfo.Name))
					{
						object row1Value = type.GetProperty(propertyInfo.Name).GetValue(row1, null);
						object row2Value = type.GetProperty(propertyInfo.Name).GetValue(row2, null);
						if (row1Value != row2Value && (row1Value == null || !row1Value.Equals(row2Value)))
						{
							return false;
						}
					}
				}
				return true;
			}
			return row1 == row2;
		}

		protected virtual string GetLineDiscountTarget(PXCache cache, TLine line)
		{
			LineEntitiesFields eFields = LineEntitiesFields.GetMapFor(line, cache);
			string lineDiscountTarget = AR.LineDiscountTargetType.ExtendedPrice;
			if (eFields?.VendorID != null && eFields.CustomerID == null)
			{
				Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.SelectSingleBound(cache.Graph, null, eFields.VendorID);
				if (vendor != null) lineDiscountTarget = vendor.LineDiscountTarget;
			}
			else
			{
				ARSetup arsetup = PXSelect<ARSetup>.Select(cache.Graph);
				if (arsetup != null) lineDiscountTarget = arsetup.LineDiscountTarget;
			}
			return lineDiscountTarget;
		}

		public virtual void ValidateDiscountDetails(PXSelectBase<TDiscountDetail> discountDetails)
		{
			List<TDiscountDetail> documentDiscounts = GetDiscountDetailsByType(discountDetails.Cache, discountDetails, DiscountType.Document);
			if (documentDiscounts.Count > 1)
			{
				discountDetails.Cache.RaiseExceptionHandling(DiscountID, documentDiscounts.First(), documentDiscounts.First().DiscountID, new PXSetPropertyException(AR.Messages.OnlyOneDocumentDiscountAllowed, PXErrorLevel.Error));
				throw new PXSetPropertyException(AR.Messages.OnlyOneDocumentDiscountAllowed, PXErrorLevel.RowError);
			}
			List<TDiscountDetail> groupDiscounts = GetDiscountDetailsByType(discountDetails.Cache, discountDetails, DiscountType.Group);
			foreach (TDiscountDetail outerDiscount in groupDiscounts)
			{
				foreach (TDiscountDetail innerDiscount in groupDiscounts)
				{
					if (outerDiscount.LineNbr != innerDiscount.LineNbr && outerDiscount.DiscountID == innerDiscount.DiscountID && outerDiscount.DiscountSequenceID == innerDiscount.DiscountSequenceID)
					{
						discountDetails.Cache.RaiseExceptionHandling(DiscountID, innerDiscount, innerDiscount.DiscountID, new PXSetPropertyException(AR.Messages.DuplicateGroupDiscount, PXErrorLevel.RowError));
						throw new PXSetPropertyException(AR.Messages.DuplicateGroupDiscount, PXErrorLevel.RowError);
					}
				}
			}
		}

		/// <summary>
		/// Inserts new line to a Discount Details grid. Sets InternalDEOperation to true by default to disable logic, specified in graph's DiscountDetail_RowInserted event handler.
		/// </summary>
		/// <param name="cache">DiscountDetails cache recommended, but it is acceptable to use any other cache of the current graph (cache.Graph is used later)</param>
		/// <param name="discountDetails">DiscountDetails view</param>
		/// <param name="newTrace">DiscountDetails line that should be inserted</param>
		public virtual TDiscountDetail InsertDiscountDetail(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, TDiscountDetail newTrace, bool setInternalDiscountEngineCall = true)
		{
			try
			{
				IsInternalDiscountEngineCall = setInternalDiscountEngineCall;
				return discountDetails.Insert(newTrace);
			}
			finally
			{
				IsInternalDiscountEngineCall = false;
			}
		}

		/// <summary>
		/// Updates existing line in a Discount Details grid. Sets InternalDEOperation to true by default to disable logic, specified in graph's DiscountDetail_RowUpdated event handler.
		/// </summary>
		/// <param name="cache">DiscountDetails cache recommended, but it is acceptable to use any other cache of the current graph (cache.Graph is used later)</param>
		/// <param name="discountDetails">DiscountDetails view</param>
		/// <param name="trace">DiscountDetails line that should be updated</param>
		public virtual TDiscountDetail UpdateDiscountDetail(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, TDiscountDetail trace, bool setInternalDiscountEngineCall = true)
		{
			try
			{
				IsInternalDiscountEngineCall = setInternalDiscountEngineCall;
				return discountDetails.Update(trace);
			}
			finally
			{
				IsInternalDiscountEngineCall = false;
			}
		}

		/// <summary>
		/// Removes existing line from a Discount Details grid. Sets InternalDEOperation to true by default to disable logic, specified in graph's DiscountDetail_RowDeleted event handler.
		/// </summary>
		/// <param name="cache">DiscountDetails cache recommended, but it is acceptable to use any other cache of the current graph (cache.Graph is used later)</param>
		/// <param name="discountDetails">DiscountDetails view</param>
		/// <param name="traceToDelete">DiscountDetails line that should be deleted</param>
		public virtual TDiscountDetail DeleteDiscountDetail(PXCache cache, PXSelectBase<TDiscountDetail> discountDetails, TDiscountDetail traceToDelete, bool setInternalDiscountEngineCall = true)
		{
			try
			{
				IsInternalDiscountEngineCall = setInternalDiscountEngineCall;
				return discountDetails.Delete(traceToDelete);
			}
			finally
			{
				IsInternalDiscountEngineCall = false;
			}
		}


		/// <summary>
		/// Flag that indicates that the operation is called by the internal logic of the Discount Engine. Replaces 'e.ExternalCall == true' check in DiscountDetail_RowInserted/Updated/Deleted event handlers.
		/// </summary>
		public virtual bool IsInternalDiscountEngineCall
		{
			get { return PXContext.GetSlot<bool>("InternalDEOperation"); }
			set { PXContext.SetSlot("InternalDEOperation", value); }
		}
		#endregion
	}
}