using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using System.Diagnostics;
using PX.Objects.IN;
using PX.TM;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.AR
{
	public class ARSalesPriceMaint : PXGraph<ARSalesPriceMaint>
	{
		#region DAC Overrides
		#region ARSalesPrice
		#region PriceType
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[PriceTypes.List]
		[PXUIField(DisplayName = "Price Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void ARSalesPrice_PriceType_CacheAttached(PXCache sender) { }
		#endregion
		#region InventoryID
		[Inventory(DisplayName = "Inventory ID")]
		[PXDefault(typeof(ARSalesPriceFilter.inventoryID))]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<ARSalesPrice.inventoryID>>>>))]
		public virtual void ARSalesPrice_InventoryID_CacheAttached(PXCache sender) { }
		#endregion
		#endregion
		#endregion

		#region Selects/Views
		public PXFilter<ARSalesPriceFilter> Filter;

		[PXFilterable]
		public PXSelectJoin<ARSalesPrice,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>>,
			LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>>,
			Where<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
			And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
			And2<Where<Current<ARSalesPriceFilter.priceType>,Equal<PriceTypes.allPrices>, Or<ARSalesPrice.priceType, Equal<Current<ARSalesPriceFilter.priceType>>>>,
			And2<Where<ARSalesPrice.inventoryID, Equal<Current<ARSalesPriceFilter.inventoryID>>, Or<Current<ARSalesPriceFilter.inventoryID>, IsNull>>,
			And2<Where<ARSalesPrice.effectiveDate, GreaterEqual<Current<ARSalesPriceFilter.minDate>>, Or<Current<ARSalesPriceFilter.minDate>, IsNull>>,
			And<Where2<Where<Current<ARSalesPriceFilter.itemClassID>, IsNull,
					Or<Current<ARSalesPriceFilter.itemClassID>, Equal<InventoryItem.itemClassID>>>,
				And2<Where<Current<ARSalesPriceFilter.inventoryPriceClassID>, IsNull,
					Or<Current<ARSalesPriceFilter.inventoryPriceClassID>, Equal<InventoryItem.priceClassID>>>,
				And2<Where<Current<ARSalesPriceFilter.ownerID>, IsNull,
					Or<Current<ARSalesPriceFilter.ownerID>, Equal<InventoryItem.priceManagerID>>>,
				And2<Where<Current<ARSalesPriceFilter.myWorkGroup>, Equal<False>,
						 Or<InventoryItem.priceWorkgroupID, InMember<CurrentValue<ARSalesPriceFilter.currentOwnerID>>>>,
				And<Where<Current<ARSalesPriceFilter.workGroupID>, IsNull,
					Or<Current<ARSalesPriceFilter.workGroupID>, Equal<InventoryItem.priceWorkgroupID>>>>>>>>>>>>>>,
			OrderBy<Asc<ARSalesPrice.inventoryID,
					Asc<ARSalesPrice.uOM>>>> Records;

		public PXSetup<Company> Company;
		public PXSetup<ARSetup> arsetup;
		#endregion

		#region Ctors
		public ARSalesPriceMaint()
		{
		}
		#endregion

		#region Buttons/Actions
		public PXSave<ARSalesPriceFilter> Save;
		public PXCancel<ARSalesPriceFilter> Cancel;
		#endregion

		#region Event Handlers
		public override void Persist()
		{
			foreach (ARSalesPrice price in Records.Cache.Inserted)
			{
				ValidateDuplicate(this, Records.Cache, price);
			}
			foreach (ARSalesPrice price in Records.Cache.Updated)
			{
				ValidateDuplicate(this, Records.Cache, price);
			}
			base.Persist();
		}

		protected virtual void ARSalesPrice_SalesPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
				if (item != null)
				{
					if (row.CuryID == Company.Current.BaseCuryID)
					{
						if (row.UOM == item.BaseUnit)
						{
							e.NewValue = item.BasePrice;
						}
						else
						{
							e.NewValue = INUnitAttribute.ConvertToBase(sender, item.InventoryID, row.UOM ?? item.SalesUnit, item.BasePrice.Value, INPrecision.UNITCOST);
						}
					}
				}
			}
			else
			{
				e.NewValue = 0m;
			}

		}

		protected virtual void ARSalesPrice_PriceType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (Filter.Current != null && Filter.Current.PriceType != PriceTypes.AllPrices)
					e.NewValue = Filter.Current.PriceType;
				else
					e.NewValue = PriceTypes.Customer;
			}

		}

		protected virtual void ARSalesPrice_SalesPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null && MinGrossProfitValidation != MinGrossProfitValidationType.None && row.EffectiveDate != null)
			{
				var r = (PXResult<InventoryItem, INItemCost>)
				PXSelectJoin<InventoryItem,
					LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.SelectWindowed(this, 0, 1, row.InventoryID);

				InventoryItem item = r;
				INItemCost cost = r;
				if (item != null)
				{
					decimal newValue = (decimal)e.NewValue;
					if (row.UOM != item.BaseUnit)
					{
						try
						{
							newValue = INUnitAttribute.ConvertFromBase(sender, item.InventoryID, row.UOM, newValue, INPrecision.UNITCOST);
						}
						catch (PXUnitConversionException)
						{
							sender.RaiseExceptionHandling<ARSalesPrice.salesPrice>(row, e.NewValue, new PXSetPropertyException(SO.Messages.FailedToConvertToBaseUnits, PXErrorLevel.Warning));
							return;
						}
					}

					decimal minPrice = PXPriceCostAttribute.MinPrice(item, cost);
					if (row.CuryID != Company.Current.BaseCuryID)
					{
						ARSetup arsetup = PXSetup<ARSetup>.Select(this);

						if (string.IsNullOrEmpty(arsetup.DefaultRateTypeID))
						{
							throw new PXException(SO.Messages.DefaultRateNotSetup);
						}

						minPrice = ConvertAmt(Company.Current.BaseCuryID, row.CuryID, arsetup.DefaultRateTypeID, row.EffectiveDate.Value, minPrice);
					}


					if (newValue < minPrice)
					{
						switch (MinGrossProfitValidation)
						{
							case MinGrossProfitValidationType.Warning:
								sender.RaiseExceptionHandling<ARSalesPrice.salesPrice>(row, e.NewValue, new PXSetPropertyException(SO.Messages.GrossProfitValidationFailed, PXErrorLevel.Warning));
								break;
							case MinGrossProfitValidationType.SetToMin:
								e.NewValue = minPrice;
								sender.RaiseExceptionHandling<ARSalesPrice.salesPrice>(row, e.NewValue, new PXSetPropertyException(SO.Messages.GrossProfitValidationFailedAndPriceFixed, PXErrorLevel.Warning));
								break;
							default:
								break;
						}
					}
				}
			}
		}

		protected virtual void ARSalesPrice_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARSalesPrice.uOM>(e.Row);
		}

		protected virtual void ARSalesPrice_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARSalesPrice.salesPrice>(e.Row);
		}

		protected virtual void ARSalesPrice_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.IsPromotionalPrice != true)
			{
				row.ExpirationDate = null;
			}
		}

		protected virtual void ARSalesPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARSalesPrice.priceType>(sender, row, Filter.Current.PriceType == PriceTypes.AllPrices);
				PXUIFieldAttribute.SetEnabled<ARSalesPrice.inventoryID>(sender, row, Filter.Current.InventoryID == null);
				PXUIFieldAttribute.SetEnabled<ARSalesPrice.expirationDate>(sender, row, row.IsPromotionalPrice == true);
			}
		}

		protected virtual void ARSalesPriceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name) == false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name) == false);

			//ARSalesPriceFilter row = (ARSalesPriceFilter)e.Row;
			//if (row.FilterType == FilterTypes.Inventory)
			//{
			//	PXUIFieldAttribute.SetVisible<ARSalesPriceFilter.inventoryID>(sender, row, false);
			//}
			//if (row.FilterType == FilterTypes.Customer)
			//{
			//	PXUIFieldAttribute.SetVisible<ARSalesPriceFilter.priceType>(sender, row, false);
			//}
		}

		protected virtual void ARSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.expirationDate).Name));
			}
			if (row.IsPromotionalPrice == true && row.ExpirationDate < row.EffectiveDate)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.expirationDate>(row, row.ExpirationDate, new PXSetPropertyException(Messages.EffectiveDateExpirationDate, PXErrorLevel.RowError));
			}
			//if (row.CuryID == Company.Current.BaseCuryID && row.CustPriceClassID == AR.ARPriceClass.EmptyPriceClass && row.IsPromotionalPrice != true)
			//{
			//	InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);

			//	if (item != null && sender.GetStatus(e.Row) == PXEntryStatus.Inserted || sender.GetStatus(e.Row) == PXEntryStatus.Updated)
			//	{
			//		if (SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit)
			//		{
			//			if (row.UOM == item.BaseUnit)
			//			{
			//				List<PXDataFieldParam> updatedFields = new List<PXDataFieldParam>();
							
			//				if (row.PendingPrice != null)
			//				{
			//					updatedFields.Add(new PXDataFieldAssign("PendingBasePrice", PXDbType.Decimal, row.PendingPrice ?? 0));
			//					updatedFields.Add(new PXDataFieldAssign("PendingBasePriceDate", PXDbType.DateTime, row.EffectiveDate));
			//					updatedFields.Add(new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, DateTime.Now));
			//				}

			//				updatedFields.Add(new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID));
			//				PXDatabase.Update<InventoryItem>(updatedFields.ToArray());
			//			}
			//		}
			//		else
			//		{
			//			if (row.UOM == item.SalesUnit)
			//			{
			//				List<PXDataFieldParam> updatedFields = new List<PXDataFieldParam>();
							
			//				if (row.PendingPrice != null)
			//				{
			//					decimal pendingBasePrice = INUnitAttribute.ConvertFromBase(sender, item.InventoryID, row.UOM, row.PendingPrice ?? 0, INPrecision.UNITCOST);
			//					updatedFields.Add(new PXDataFieldAssign("PendingBasePrice", PXDbType.Decimal, pendingBasePrice));
			//					updatedFields.Add(new PXDataFieldAssign("PendingBasePriceDate", PXDbType.DateTime, row.EffectiveDate));
			//					updatedFields.Add(new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, DateTime.Now));
			//				}

			//				updatedFields.Add(new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID));
			//				PXDatabase.Update<InventoryItem>(updatedFields.ToArray());
			//			}
			//		}
			//	}
			//}
		}

		#endregion

		#region Private Members
		public static void ValidateDuplicate(PXGraph graph, PXCache sender, ARSalesPrice price)
		{
			//PXSelectBase<ARSalesPrice> selectDuplicate = new PXSelect<ARSalesPrice, Where<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>, And<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>, And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>, And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>, And<ARSalesPrice.recordID, NotEqual<Required<ARSalesPrice.recordID>>, And<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>, And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>>>>>>>>>(graph);
			//ARSalesPrice duplicate;
			//if (price.IsPromotionalPrice == true)
			//{
			//	//selectDuplicate.WhereAnd<Where2<Where<Required<ARSalesPrice.lastDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>>, Or<Required<ARSalesPrice.expirationDate>, Between<ARSalesPrice.lastDate, ARSalesPrice.expirationDate>, Or<ARSalesPrice.lastDate, Between<Required<ARSalesPrice.lastDate>, Required<ARSalesPrice.expirationDate>>, Or<ARSalesPrice.expirationDate, Between<Required<ARSalesPrice.lastDate>, Required<ARSalesPrice.expirationDate>>>>>>>();
			//	//duplicate = selectDuplicate.SelectSingle(price.CuryID, price.CustPriceClassID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate, price.LastDate, price.ExpirationDate);
			//}
			//else
			//{
			//	//duplicate = selectDuplicate.SelectSingle(price.CuryID, price.CustPriceClassID, price.InventoryID, price.UOM, price.RecordID, price.PendingBreakQty, price.BreakQty, price.IsPromotionalPrice);
			//}
			//if (duplicate != null)
			//{
			//	sender.RaiseExceptionHandling<ARSalesPrice.uOM>(price, price.UOM, new PXSetPropertyException(SO.Messages.DuplicateSalesPrice, PXErrorLevel.RowError));
			//}
		}

		private decimal ConvertAmt(string from, string to, string rateType, DateTime effectiveDate, decimal amount, decimal? customRate = 1)
		{
			if (from == to)
				return amount;

			CurrencyRate rate = getCuryRate(from, to, rateType, effectiveDate);

			if (rate == null)
			{
				return amount * customRate ?? 1;
			}
			else
			{
				return rate.CuryMultDiv == "M" ? amount * rate.CuryRate ?? 1 : amount / rate.CuryRate ?? 1;
			}
		}

		private CurrencyRate getCuryRate(string from, string to, string curyRateType, DateTime curyEffDate)
		{
			return PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
							And<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
							And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.SelectWindowed(this, 0, 1, to, from, curyRateType, curyEffDate);
		}

		private string MinGrossProfitValidation
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);
				if (sosetup != null)
				{
					if (string.IsNullOrEmpty(sosetup.MinGrossProfitValidation))
						return MinGrossProfitValidationType.Warning;
					else
						return sosetup.MinGrossProfitValidation;
				}
				else
					return MinGrossProfitValidationType.Warning;

			}
		}
		#endregion

		#region Sales Price Calculation

		/// <summary>
		/// Calculates Sales Price.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <returns>Sales Price.</returns>
		/// <remarks>AlwaysFromBaseCury flag in the SOSetup is considered when performing the calculation.</remarks>
		public static decimal? CalculateSalesPrice(PXCache sender, string custPriceClass, int? inventoryID, CurrencyInfo currencyinfo, string UOM, DateTime date)
		{
			bool alwaysFromBase = false;

			ARSetup arsetup = (ARSetup)sender.Graph.Caches[typeof(ARSetup)].Current ?? PXSelect<ARSetup>.Select(sender.Graph);
			if (arsetup != null)
			{
				alwaysFromBase = arsetup.AlwaysFromBaseCury == true;
			}

			return ARSalesPriceMaint.CalculateSalesPrice(sender, custPriceClass, null, inventoryID, currencyinfo, 0m, UOM, date, alwaysFromBase);
		}

		/// <summary>
		/// Calculates Sales Price.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <returns>Sales Price.</returns>
		/// <remarks>AlwaysFromBaseCury flag in the SOSetup is considered when performing the calculation.</remarks>
		public static decimal? CalculateSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitPrice)
		{
			bool alwaysFromBase = false;

			ARSetup arsetup = (ARSetup)sender.Graph.Caches[typeof(ARSetup)].Current ?? PXSelect<ARSetup>.Select(sender.Graph);
			if (arsetup != null)
			{
				alwaysFromBase = arsetup.AlwaysFromBaseCury == true;
			}

			decimal? salesPrice = ARSalesPriceMaint.CalculateSalesPrice(sender, custPriceClass, customerID, inventoryID, currencyinfo, Math.Abs(quantity ?? 0m), UOM, date, alwaysFromBase);
			if ((salesPrice == null || salesPrice == 0) && currentUnitPrice != null && currentUnitPrice != 0m)
				return currentUnitPrice;
			else
				return salesPrice;
		}

		/// <summary>
		/// Calculates Sales Price.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <param name="alwaysFromBaseCurrency">If true sales price is always calculated (converted) from Base Currency.</param>
		/// <returns>Sales Price.</returns>
		public static decimal? CalculateSalesPrice(PXCache sender, string custPriceClass, int? inventoryID, CurrencyInfo currencyinfo, string UOM, DateTime date, bool alwaysFromBaseCurrency)
		{
			return CalculateSalesPrice(sender, custPriceClass, null, inventoryID, currencyinfo, 0m, UOM, date, alwaysFromBaseCurrency);
		}

		/// <summary>
		/// Calculates Sales Price.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <param name="alwaysFromBaseCurrency">If true sales price is always calculated (converted) from Base Currency.</param>
		/// <returns>Sales Price.</returns>
		public static decimal? CalculateSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, CurrencyInfo currencyinfo, decimal? quantity, string UOM, DateTime date, bool alwaysFromBaseCurrency)
		{
			//InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, inventoryID);
			SalesPriceItem spItem;
			try
			{
				spItem = FindSalesPrice(sender, custPriceClass, customerID, inventoryID, alwaysFromBaseCurrency ? currencyinfo.BaseCuryID : currencyinfo.CuryID, Math.Abs(quantity ?? 0m), UOM, date);
			}
			catch (PXUnitConversionException)
			{
				return null;
			}

			if (spItem != null)
			{
				decimal salesPrice = spItem.Price;

				if (spItem.CuryID != currencyinfo.CuryID)
				{
					PXCurrencyAttribute.CuryConvCury(sender, currencyinfo, spItem.Price, out salesPrice);
				}

				if (UOM == null)
				{
					return null;
				}

				if (spItem.UOM != UOM)
				{
					decimal salesPriceInBase = INUnitAttribute.ConvertFromBase(sender, inventoryID, spItem.UOM, salesPrice, INPrecision.UNITCOST);
					salesPrice = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, salesPriceInBase, INPrecision.UNITCOST);
				}

				return salesPrice;
			}

			return null;
		}

		public static SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? inventoryID, string curyID, string UOM, DateTime date)
		{
			return FindSalesPrice(sender, custPriceClass, null, inventoryID, curyID, 0m, UOM, date);
		}

		public static SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, string curyID, decimal? quantity, string UOM, DateTime date)
		{
			PXSelectBase<ARSalesPrice> salesPrice = new PXSelect<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
            And2<Where2<Where<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>, And<ARSalesPrice.priceCode, Equal<Required<ARSalesPrice.priceCode>>>>,
            Or<Where<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>, And<ARSalesPrice.priceCode, Equal<Required<ARSalesPrice.priceCode>>>>>>,
			And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
			And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,

			And<Where2<Where<ARSalesPrice.breakQty, LessEqual<Required<ARSalesPrice.breakQty>>>,
			And<Where2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
            And<ARSalesPrice.expirationDate, Greater<Required<ARSalesPrice.expirationDate>>>>,
			Or2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
			And<ARSalesPrice.expirationDate, IsNull>>,
            Or<Where<ARSalesPrice.expirationDate, Greater<Required<ARSalesPrice.expirationDate>>,
			And<ARSalesPrice.effectiveDate, IsNull>>>>>>>>>>>>,

			OrderBy<Asc<ARSalesPrice.priceType, Desc<ARSalesPrice.isPromotionalPrice, Desc<ARSalesPrice.breakQty>>>>>(sender.Graph);

			PXSelectBase<ARSalesPrice> selectWithBaseUOM = new PXSelectJoin<ARSalesPrice, InnerJoin<InventoryItem, 
                On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>,
                And<InventoryItem.baseUnit, Equal<ARSalesPrice.uOM>>>>, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
            And2<Where2<Where<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>, And<ARSalesPrice.priceCode, Equal<Required<ARSalesPrice.priceCode>>>>,
            Or<Where<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>, And<ARSalesPrice.priceCode, Equal<Required<ARSalesPrice.priceCode>>>>>>,
            And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
            And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,

            And<Where2<Where<ARSalesPrice.breakQty, LessEqual<Required<ARSalesPrice.breakQty>>>,
            And<Where2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
            And<ARSalesPrice.expirationDate, Greater<Required<ARSalesPrice.expirationDate>>>>,
            Or2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
            And<ARSalesPrice.expirationDate, IsNull>>,
            Or<Where<ARSalesPrice.expirationDate, Greater<Required<ARSalesPrice.expirationDate>>,
            And<ARSalesPrice.effectiveDate, IsNull>>>>>>>>>>>>,

			OrderBy<Desc<ARSalesPrice.customerID, Desc<ARSalesPrice.isPromotionalPrice, Desc<ARSalesPrice.breakQty>>>>>(sender.Graph);

            ARSalesPrice item = salesPrice.SelectWindowed(0, 1, inventoryID, PriceTypes.Customer, PriceCodeInfo.CustomerPrefix + customerID, PriceTypes.CustomerPriceClass, PriceCodeInfo.CustomerPriceClassPrefix + custPriceClass, curyID, UOM, quantity, date, date, date, date);

			string uomFound = null;

			if (item == null)
			{
				decimal baseUnitQty = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, (decimal)quantity, INPrecision.QUANTITY);
                item = selectWithBaseUOM.Select(inventoryID, PriceTypes.Customer, PriceCodeInfo.CustomerPrefix + customerID, PriceTypes.CustomerPriceClass, PriceCodeInfo.CustomerPriceClassPrefix + custPriceClass, curyID, UOM, quantity, date, date, date, date);

				if (item == null)
				{
					item = salesPrice.SelectWindowed(0, 1, inventoryID, PriceTypes.Customer, PriceCodeInfo.CustomerPrefix + customerID, PriceTypes.CustomerPriceClass, PriceCodeInfo.CustomerPriceClassPrefix + AR.ARPriceClass.EmptyPriceClass, curyID, UOM, quantity, date, date, date, quantity, date);
					if (item == null)
					{
						item = selectWithBaseUOM.Select(inventoryID, PriceTypes.Customer, PriceCodeInfo.CustomerPrefix + customerID, PriceTypes.CustomerPriceClass, PriceCodeInfo.CustomerPriceClassPrefix + AR.ARPriceClass.EmptyPriceClass, curyID, UOM, quantity, date, date, date, quantity, date);

						if (item == null)
						{
							return null;
						}
						else
						{
							uomFound = item.UOM;
						}
					}
					else
					{
						uomFound = UOM;
					}

				}
				else
				{
					uomFound = item.UOM;
				}
			}
			else
			{
				uomFound = UOM;
			}
			return new SalesPriceItem(uomFound, (item.SalesPrice ?? 0), item.CuryID);
		}

		public class SalesPriceItem
		{
			private string uom;

			public string UOM
			{
				get { return uom; }
			}

			private decimal price;

			public decimal Price
			{
				get { return price; }
			}

			private string curyid;
			public string CuryID
			{
				get { return curyid; }
			}

			public SalesPriceItem(string uom, decimal price, string curyid)
			{
				this.uom = uom;
				this.price = price;
				this.curyid = curyid;
			}

		}

		#endregion

	}

	public class CustomerPriceClassAttribute : PXCustomSelectorAttribute
	{

		public CustomerPriceClassAttribute()
			: base(typeof(ARPriceClass.priceClassID))
		{
			this.DescriptionField = typeof(ARPriceClass.description);
		}

		protected virtual IEnumerable GetRecords()
		{
			//ARPriceClass epc = new ARPriceClass();
			//epc.PriceClassID = ARPriceClass.EmptyPriceClass;
			//epc.Description = Messages.BasePriceClassDescription;

			//yield return epc;

			foreach (ARPriceClass pc in PXSelect<ARPriceClass>.Select(this._Graph))
			{
				yield return pc;
			}
		}
	}

	[Serializable]
	public partial class ARSalesPriceFilter : IBqlTable
	{
		#region PriceType
		public abstract class priceType : PX.Data.IBqlField
		{
		}
		protected String _PriceType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PriceTypes.AllPrices)]
		[PriceTypes.ListWithAll]
		[PXUIField(DisplayName = "Price Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String PriceType
		{
			get
			{
				return this._PriceType;
			}
			set
			{
				this._PriceType = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[Inventory(DisplayName = "Inventory ID")]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region CustPriceClassID
		public abstract class custPriceClassID : PX.Data.IBqlField
		{
		}
		protected String _CustPriceClassID;
		[PXDBString(10, InputMask = ">aaaaaaaaaa")]
		[PXDefault(AR.ARPriceClass.EmptyPriceClass)]
		[PXUIField(DisplayName = "Customer Price Class", Visibility = PXUIVisibility.SelectorVisible)]
		[CustomerPriceClass]
		public virtual String CustPriceClassID
		{
			get
			{
				return this._CustPriceClassID;
			}
			set
			{
				this._CustPriceClassID = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.IBqlField
		{
		}
		protected Int32? _CustomerID;
		[PXDBInt()]
		[Customer]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARSalesPriceFilter.customerID>>>>))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region MinDate
		public abstract class minDate : PX.Data.IBqlField
		{
		}
		private DateTime? _MinDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Min. Effective Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? MinDate
		{
			get
			{
				return this._MinDate;
			}
			set
			{
				this._MinDate = value;
			}
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected String _ItemClassID;
		[PXDBString(10)]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(INItemClass.itemClassID), DescriptionField = typeof(INItemClass.descr))]
		public virtual String ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
		#region InventoryPriceClassID
		public abstract class inventoryPriceClassID : PX.Data.IBqlField
		{
		}
		protected String _InventoryPriceClassID;
		[PXDBString(10)]
		[PXSelector(typeof(INPriceClass.priceClassID))]
		[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
		public virtual String InventoryPriceClassID
		{
			get
			{
				return this._InventoryPriceClassID;
			}
			set
			{
				this._InventoryPriceClassID = value;
			}
		}
		#endregion
		

		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.IBqlField
		{
		}

		[PXDBGuid]
		[CR.CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.IBqlField
		{
		}
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Price Manager")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Price Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.IBqlField
		{
		}
		[PXDefault(false)]
		[PXDBBool]
		public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true;
			}
		}
		#endregion
	}
}
