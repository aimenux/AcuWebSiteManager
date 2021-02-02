using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.Common.Discount.Mappers;

namespace PX.Objects.Common.Discount.Attributes
{
	public enum DiscountFeatureType
	{
		Unknown,
		VendorDiscount,
		CustomerDiscount
	}

	/// <summary>
	/// Sets ManualDisc flag based on the values of the depending fields. Manual Flag is set when user
	/// overrides the discount values.
	/// This attribute also updates the relative fields. Ex: Updates Discount Amount when Discount Pct is modified.
	/// </summary>
	public class ManualDiscountMode : PXEventSubscriberAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber
	{
		private Type curyDiscAmtT;
		private Type curyTranAmtT;
		private Type freezeDiscT;
		private Type discPctT;
		private DiscountFeatureType discountFeatureTypeE;

		#region BqlFields
		private abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
		private abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
		private abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
		#endregion

		public ManualDiscountMode(Type curyDiscAmt, Type curyTranAmt, Type discPct, Type freezeManualDisc, DiscountFeatureType discountType)
			: this(curyDiscAmt, curyTranAmt, discPct, freezeManualDisc)
		{
			this.discountFeatureTypeE = discountType;
		}

		public ManualDiscountMode(Type curyDiscAmt, Type curyTranAmt, Type discPct, Type freezeManualDisc)
			: this(curyDiscAmt, curyTranAmt, discPct)
		{
			if (curyDiscAmt == null)
				throw new ArgumentNullException();

			if (curyTranAmt == null)
				throw new ArgumentNullException();

			this.freezeDiscT = freezeManualDisc;
			this.curyTranAmtT = curyTranAmt;
		}

		public ManualDiscountMode(Type curyDiscAmt, Type curyTranAmt, Type discPct, DiscountFeatureType discountType)
			: this(curyDiscAmt, curyTranAmt, discPct)
		{
			this.discountFeatureTypeE = discountType;
		}

		public ManualDiscountMode(Type curyDiscAmt, Type curyTranAmt, Type discPct)
			: this(curyDiscAmt, discPct)
		{
			if (curyDiscAmt == null)
				throw new ArgumentNullException();

			if (curyTranAmt == null)
				throw new ArgumentNullException();

			this.curyTranAmtT = curyTranAmt;
		}

		public ManualDiscountMode(Type curyDiscAmt, Type discPct, DiscountFeatureType discountType)
			: this(curyDiscAmt, discPct)
		{
			this.discountFeatureTypeE = discountType;
		}

		public ManualDiscountMode(Type curyDiscAmt, Type discPct)
		{
			if (curyDiscAmt == null)
				throw new ArgumentNullException("curyDiscAmt");
			if (discPct == null)
				throw new ArgumentNullException("discPct");

			this.curyDiscAmtT = curyDiscAmt;
			this.discPctT = discPct;
		}

		public static AmountLineFields GetDiscountDocumentLine(PXCache sender, object line) => AmountLineFields.GetMapFor((IBqlTable)line, sender, false);

		//Returns line fields to be updated
		public static DiscountLineFields GetDiscountedLine(PXCache sender, object line) => DiscountLineFields.GetMapFor((IBqlTable)line, sender);

		public static string GetLineDiscountTarget(PXCache sender, LineEntitiesFields efields)
		{
			string LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
			if (efields != null && efields.VendorID != null && efields.CustomerID == null)
			{
				AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.SelectSingleBound(sender.Graph, null, efields.VendorID);
				if (vendor != null) LineDiscountTarget = vendor.LineDiscountTarget;
			}
			else
			{
				ARSetup arsetup = PXSelect<ARSetup>.Select(sender.Graph);
				if (arsetup != null) LineDiscountTarget = arsetup.LineDiscountTarget;
			}
			return LineDiscountTarget;
		}

		private void UpdateLineAmt<TField>(AmountLineFields aLine, decimal? oldValue, decimal? newValue)
			where TField : IBqlField
		{
			object curyLineAmt = newValue;
			aLine.RaiseFieldUpdating<TField>(ref curyLineAmt);
			aLine.RaiseFieldVerifying<TField>(ref curyLineAmt);
			aLine.CuryLineAmount = (decimal?)curyLineAmt;
			aLine.RaiseFieldUpdated<TField>(oldValue);
		}

		#region IPXRowUpdatedSubscriber Members

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			AmountLineFields lineAmountsFields = GetDiscountDocumentLine(sender, e.Row);
			if (lineAmountsFields.FreezeManualDisc == true)
			{
				lineAmountsFields.FreezeManualDisc = false;
				return;
			}

			DiscountLineFields lineDiscountFields = GetDiscountedLine(sender, e.Row);
			if (lineDiscountFields.LineType == SOLineType.Discount)
				return;

			AmountLineFields oldLineAmountsFields = GetDiscountDocumentLine(sender, e.OldRow);
			DiscountLineFields oldLineDiscountFields = GetDiscountedLine(sender, e.OldRow);

			LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(e.Row, sender);
			LineEntitiesFields oldLineEntities = LineEntitiesFields.GetMapFor(e.OldRow, sender);

			bool manualMode = false;//by default AutoMode.
			bool useDiscPct = false;//by default value in DiscAmt has higher priority than DiscPct when both are modified.
			bool keepDiscountID = true;//should be set to true if user changes discount code code manually
			bool manualDiscUnchecked = false;

			//Force Auto Mode 
			if (lineDiscountFields.ManualDisc == false && oldLineDiscountFields.ManualDisc == true)
			{
				manualMode = false;
				manualDiscUnchecked = true;
			}

			//Change to Manual Mode based on fields changed:
			if (lineDiscountFields.ManualDisc == true || sender.Graph.IsCopyPasteContext)
				manualMode = true;

			//if (row.IsFree == true && oldRow.IsFree != true)
			//    manualMode = true;

			bool isQtyChangedToZero = lineAmountsFields.Quantity != oldLineAmountsFields.Quantity && lineAmountsFields.Quantity == 0m;
			if (lineDiscountFields.DiscPct != oldLineDiscountFields.DiscPct && lineEntities.InventoryID == oldLineEntities.InventoryID && !isQtyChangedToZero)
			{
				manualMode = true;
				useDiscPct = true;
			}

			//use DiscPct when only Quantity/CuryUnitPrice/CuryExtPrice was changed
			if (lineDiscountFields.DiscPct == oldLineDiscountFields.DiscPct && lineDiscountFields.CuryDiscAmt == oldLineDiscountFields.CuryDiscAmt &&
				(lineAmountsFields.Quantity != oldLineAmountsFields.Quantity || lineAmountsFields.CuryUnitPrice != oldLineAmountsFields.CuryUnitPrice || lineAmountsFields.CuryExtPrice != oldLineAmountsFields.CuryExtPrice))
			{
				useDiscPct = true;
			}

			if (lineDiscountFields.CuryDiscAmt != oldLineDiscountFields.CuryDiscAmt &&
				lineAmountsFields.Quantity == oldLineAmountsFields.Quantity && lineAmountsFields.CuryUnitPrice == oldLineAmountsFields.CuryUnitPrice)
			{
				manualMode = true;
				useDiscPct = false;
			}

			if (e.ExternalCall && (((Math.Abs((lineDiscountFields.CuryDiscAmt ?? 0m) - (oldLineDiscountFields.CuryDiscAmt ?? 0m)) > 0.0000005m) || (Math.Abs((lineDiscountFields.DiscPct ?? 0m) - (oldLineDiscountFields.DiscPct ?? 0m)) > 0.0000005m)) && lineDiscountFields.DiscountID == oldLineDiscountFields.DiscountID))
			{
				keepDiscountID = false;
			}

			//if only CuryLineAmt (Ext.Price) was changed for a line with DiscoutAmt<>0
			//for Contracts Qty * UnitPrice * Prorate(<>1) = ExtPrice
			if (lineAmountsFields.CuryLineAmount != oldLineAmountsFields.CuryLineAmount && lineAmountsFields.Quantity == oldLineAmountsFields.Quantity && lineAmountsFields.CuryUnitPrice == oldLineAmountsFields.CuryUnitPrice && lineAmountsFields.CuryExtPrice == oldLineAmountsFields.CuryExtPrice && lineDiscountFields.DiscPct == oldLineDiscountFields.DiscPct && lineDiscountFields.CuryDiscAmt == oldLineDiscountFields.CuryDiscAmt && lineDiscountFields.CuryDiscAmt != 0)
			{
				manualMode = true;
			}

			//Set ManualDisc flag to true when feature is disabled
			if ((discountFeatureTypeE == DiscountFeatureType.CustomerDiscount && !PXAccess.FeatureInstalled<CS.FeaturesSet.customerDiscounts>())
				|| (discountFeatureTypeE == DiscountFeatureType.VendorDiscount && !PXAccess.FeatureInstalled<CS.FeaturesSet.vendorDiscounts>()))
				manualMode = true;

			decimal? validLineAmtRaw;
			decimal? validLineAmt = null;
			if (lineAmountsFields.CuryLineAmount != oldLineAmountsFields.CuryLineAmount)
			{
				if (useDiscPct)
				{
					decimal val = lineAmountsFields.CuryExtPrice ?? 0;

					decimal disctAmt;
					if (GetLineDiscountTarget(sender, lineEntities) == LineDiscountTargetType.SalesPrice)
					{
						disctAmt = PXCurrencyAttribute.Round(sender, lineAmountsFields, (lineAmountsFields.Quantity ?? 0m) * (lineAmountsFields.CuryUnitPrice ?? 0m), CMPrecision.TRANCURY)
									- PXCurrencyAttribute.Round(sender, lineAmountsFields, (lineAmountsFields.Quantity ?? 0m) * PXDBPriceCostAttribute.Round((lineAmountsFields.CuryUnitPrice ?? 0m) * (1 - (lineDiscountFields.DiscPct ?? 0m) * 0.01m)), CMPrecision.TRANCURY);
					}
					else
					{
						disctAmt = val * (lineDiscountFields.DiscPct ?? 0m) * 0.01m;
						disctAmt = PXCurrencyAttribute.Round(sender, lineDiscountFields, disctAmt, CMPrecision.TRANCURY);
					}

					validLineAmtRaw = lineAmountsFields.CuryExtPrice - disctAmt;
					validLineAmt = PXCurrencyAttribute.Round(sender, lineAmountsFields, validLineAmtRaw ?? 0, CMPrecision.TRANCURY);

				}
				else
				{
					if (lineDiscountFields.CuryDiscAmt > lineAmountsFields.CuryExtPrice)
					{
						validLineAmtRaw = lineAmountsFields.CuryExtPrice;
					}
					else
					{
						validLineAmtRaw = lineAmountsFields.CuryExtPrice - lineDiscountFields.CuryDiscAmt;
					}
					validLineAmt = PXCurrencyAttribute.Round(sender, lineAmountsFields, validLineAmtRaw ?? 0, CMPrecision.TRANCURY);
				}

				if (lineAmountsFields.CuryLineAmount != validLineAmt && lineDiscountFields.DiscPct != oldLineDiscountFields.DiscPct)
					manualMode = true;
			}

			sender.SetValue(e.Row, this.FieldName, manualMode);

			//Process only Manual Mode:
			if (manualMode || sender.Graph.IsCopyPasteContext)
			{
				if (manualMode && !keepDiscountID && !sender.Graph.IsImport)
				{
					lineDiscountFields.DiscountID = null;
					lineDiscountFields.DiscountSequenceID = null;
				}

				//Update related fields:
				if (lineAmountsFields.Quantity == 0 && oldLineAmountsFields.Quantity != 0)
				{
					sender.SetValueExt(e.Row, sender.GetField(typeof(curyDiscAmt)), 0m);
					sender.SetValueExt(e.Row, sender.GetField(typeof(discPct)), 0m);
				}
				else if (lineAmountsFields.CuryLineAmount != oldLineAmountsFields.CuryLineAmount && !useDiscPct)
				{
					decimal? extAmt = lineAmountsFields.CuryExtPrice ?? 0;
					decimal? lineAmt = lineAmountsFields.CuryLineAmount ?? 0;
					if (extAmt - lineAmountsFields.CuryLineAmount >= 0)
					{
						if (lineDiscountFields.CuryDiscAmt > Math.Abs(extAmt ?? 0m))
						{
							sender.SetValueExt(e.Row, sender.GetField(typeof(curyDiscAmt)), lineAmountsFields.CuryExtPrice);
							PXUIFieldAttribute.SetWarning<DiscountLineFields.curyDiscAmt>(sender, e.Row,
								PXMessages.LocalizeFormatNoPrefix(AR.Messages.LineDiscountAmtMayNotBeGreaterExtPrice, lineAmountsFields.ExtPriceDisplayName));
						}
						else
							sender.SetValueExt(e.Row, sender.GetField(typeof(curyDiscAmt)), extAmt - lineAmountsFields.CuryLineAmount);
						if (extAmt != 0 && !sender.Graph.IsCopyPasteContext)
						{
							decimal? pct = CalcDiscountPercent(lineAmountsFields, lineDiscountFields);
							sender.SetValueExt(e.Row, sender.GetField(typeof(discPct)), pct);
						}
					}
					else if (extAmt != 0 && !sender.Graph.IsCopyPasteContext)
					{
						if (lineDiscountFields.CuryDiscAmt != oldLineDiscountFields.CuryDiscAmt)
						{
							decimal? pct = CalcDiscountPercent(lineAmountsFields, lineDiscountFields);
							sender.SetValueExt(e.Row, sender.GetField(typeof(discPct)), (pct ?? 0m) < -100m ? -100m : pct);
							if ((pct ?? 0m) < -100m)
							{
								sender.SetValueExt(e.Row, sender.GetField(typeof(curyDiscAmt)), -lineAmountsFields.CuryExtPrice);
								PXUIFieldAttribute.SetWarning<DiscountLineFields.curyDiscAmt>(sender, e.Row,
									PXMessages.LocalizeFormatNoPrefix(AR.Messages.LineDiscountAmtMayNotBeGreaterExtPrice, lineAmountsFields.ExtPriceDisplayName));
							}
						}
						else
						{
							sender.SetValueExt(e.Row, sender.GetField(typeof(discPct)), 0m);
							sender.SetValueExt(e.Row, sender.GetField(typeof(curyDiscAmt)), 0m);
							UpdateLineAmt<AmountLineFields.curyLineAmount>(lineAmountsFields, oldLineAmountsFields.CuryLineAmount, lineAmt);
						}
					}
				}
				else if (lineDiscountFields.CuryDiscAmt != oldLineDiscountFields.CuryDiscAmt)
				{
					if (lineAmountsFields.CuryExtPrice != 0 && !sender.Graph.IsCopyPasteContext) if (lineAmountsFields.CuryExtPrice != 0 && !sender.Graph.IsCopyPasteContext)
					{
						decimal? pct = CalcDiscountPercent(lineAmountsFields, lineDiscountFields);
						sender.SetValueExt(e.Row, sender.GetField(typeof(discPct)), pct);
					}
				}
				else if (lineDiscountFields.DiscPct != oldLineDiscountFields.DiscPct
					|| validLineAmt != null && lineAmountsFields.CuryLineAmount != validLineAmt)
				{
					decimal amt = CalcDiscountAmount(sender, GetLineDiscountTarget(sender, lineEntities),
						lineAmountsFields, lineDiscountFields);
					sender.SetValueExt(e.Row, sender.GetField(typeof(curyDiscAmt)), amt);
				}
			}
			else if (manualDiscUnchecked && lineDiscountFields.DiscountID == null)
			{
				sender.SetValueExt(e.Row, sender.GetField(typeof(discPct)), 0m);
				sender.SetValueExt(e.Row, sender.GetField(typeof(curyDiscAmt)), 0m);
			}
		}

		#endregion

		#region IPXRowInsertedSubscriber Members

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			//When a new row is inserted there is 2 possible ways of handling it:
			//1. Sync the Discounts fields DiscAmt and DiscPct and calculate LineAmt as ExtPrice - DiscAmt. If DiscAmt <> 0 then ManualDisc flag is set.
			//2. Add line as is without changing any of the fields.
			//First Mode is typically executed when a user adds a line to Invoice from UI. Moreover the user enters Only Ext.Amt on the UI.
			//Second Mode is when a line from SOOrder is added to SOInvoice - in this case all discounts must be freezed and line must be added as is.

			//Set ManualDisc flag to true when feature is disabled
			if ((discountFeatureTypeE == DiscountFeatureType.CustomerDiscount && !PXAccess.FeatureInstalled<CS.FeaturesSet.customerDiscounts>())
				|| (discountFeatureTypeE == DiscountFeatureType.VendorDiscount && !PXAccess.FeatureInstalled<CS.FeaturesSet.vendorDiscounts>()))
				sender.SetValue(e.Row, this.FieldName, true);

			if (!sender.Graph.IsCopyPasteContext)
			{
				AmountLineFields lineAmountsFields = GetDiscountDocumentLine(sender, e.Row);
				if (lineAmountsFields.FreezeManualDisc == true)
				{
					lineAmountsFields.FreezeManualDisc = false;
					return;
				}
				DiscountLineFields lineDiscountFields = GetDiscountedLine(sender, e.Row);
				if (lineDiscountFields.LineType == SOLineType.Discount)
					return;

				if (lineDiscountFields.CuryDiscAmt != null && lineDiscountFields.CuryDiscAmt != 0 && lineAmountsFields.CuryExtPrice != 0)
				{
					decimal? revCuryDiscAmt = null;
					if (lineDiscountFields.DiscPct != null)
					{
						revCuryDiscAmt = PXDBCurrencyAttribute.RoundCury(sender, e.Row, (decimal)((lineAmountsFields.CuryExtPrice ?? 0m) / 100 * lineDiscountFields.DiscPct));
					}
					if (revCuryDiscAmt != null && revCuryDiscAmt != lineDiscountFields.CuryDiscAmt)
					{
						lineDiscountFields.DiscPct = 100 * lineDiscountFields.CuryDiscAmt / lineAmountsFields.CuryExtPrice;
					}
					if (sender.Graph.IsContractBasedAPI)
					{
						sender.SetValue(e.Row, lineDiscountFields.GetField<DiscountLineFields.manualDisc>().Name, true);
						sender.SetValueExt(e.Row, lineAmountsFields.GetField<AmountLineFields.curyLineAmount>().Name, lineAmountsFields.CuryExtPrice - lineDiscountFields.CuryDiscAmt);
					}
					else
					{
						sender.SetValue(e.Row, lineAmountsFields.GetField<AmountLineFields.curyLineAmount>().Name, lineAmountsFields.CuryExtPrice - lineDiscountFields.CuryDiscAmt);
					}
				}
				else if (lineDiscountFields.DiscPct != null && lineDiscountFields.DiscPct != 0)
				{
					decimal curyDiscAmt = (lineAmountsFields.CuryExtPrice ?? 0) * (lineDiscountFields.DiscPct ?? 0) * 0.01m;
					sender.SetValueExt(e.Row, lineDiscountFields.GetField<DiscountLineFields.curyDiscAmt>().Name, PXCurrencyAttribute.Round(sender, lineDiscountFields, curyDiscAmt, CMPrecision.TRANCURY));
					if (sender.Graph.IsContractBasedAPI)
					{
						sender.SetValue(e.Row, lineDiscountFields.GetField<DiscountLineFields.manualDisc>().Name, true);
						sender.SetValueExt(e.Row, lineAmountsFields.GetField<AmountLineFields.curyLineAmount>().Name, lineAmountsFields.CuryExtPrice - lineDiscountFields.CuryDiscAmt);
					}
					else
					{
						sender.SetValue(e.Row, lineAmountsFields.GetField<AmountLineFields.curyLineAmount>().Name, lineAmountsFields.CuryExtPrice - lineDiscountFields.CuryDiscAmt);
					}
				}
				else if (lineAmountsFields.CuryExtPrice != null && lineAmountsFields.CuryExtPrice != 0m && (lineAmountsFields.CuryLineAmount == null || lineAmountsFields.CuryLineAmount == 0m))
				{
					if (sender.Graph.IsContractBasedAPI)
					{
						sender.SetValueExt(e.Row, lineAmountsFields.GetField<AmountLineFields.curyLineAmount>().Name, lineAmountsFields.CuryExtPrice);
					}
					else
					{
						sender.SetValue(e.Row, lineAmountsFields.GetField<AmountLineFields.curyLineAmount>().Name, lineAmountsFields.CuryExtPrice);
					}
				}
			}
		}

		#endregion

		protected virtual decimal CalcDiscountAmount(PXCache sender, string lineDiscountTarget,
			AmountLineFields lineAmountsFields, DiscountLineFields lineDiscountFields)
		{
			if (lineDiscountTarget == AP.LineDiscountTargetType.SalesPrice
				&& lineAmountsFields.CuryUnitPrice != 0 && lineAmountsFields.Quantity != 0) // If sales price is available.
			{
				decimal extPrice = PXCurrencyAttribute.Round(sender, lineAmountsFields, (lineAmountsFields.Quantity ?? 0m)
					* (lineAmountsFields.CuryUnitPrice ?? 0m), CMPrecision.TRANCURY);

				decimal lineAmt = PXCurrencyAttribute.Round(sender, lineAmountsFields, (lineAmountsFields.Quantity ?? 0m)
					* ((lineAmountsFields.CuryUnitPrice ?? 0m) - PXDBPriceCostAttribute.Round((lineAmountsFields.CuryUnitPrice ?? 0m)
					* (lineDiscountFields.DiscPct ?? 0m) * 0.01m)), CMPrecision.TRANCURY);

				return extPrice - lineAmt;
			}
			else
			{
				return (lineAmountsFields.CuryExtPrice ?? 0m) * (lineDiscountFields.DiscPct ?? 0m) * 0.01m;
			}
		}

		protected virtual decimal? CalcDiscountPercent(AmountLineFields lineAmountsFields, DiscountLineFields lineDiscountFields)
		{
			return (lineDiscountFields.CuryDiscAmt ?? 0) * 100 / lineAmountsFields.CuryExtPrice;
		}
	}
}