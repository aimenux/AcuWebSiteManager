using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CM;
using System.Collections;
using PX.Objects.GL;

namespace PX.Objects.SO
{
	[PXGraphName(Messages.SOSalesPriceMaintenance, typeof(SOSalesPrice))]
	public class SOSalesPriceEntry : PXGraph<SOSalesPriceEntry>
	{
		#region Selects/Views

		public PXFilter<SalesPriceFilter> Filter;
		public PXSelectJoin<SOSalesPrice,
			InnerJoin<InventoryItem, On<SOSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>>,
			Where<SOSalesPrice.curyID, Equal<Current<SalesPriceFilter.curyID>>>> Records;

		public IEnumerable records()
		{
			SalesPriceFilter filter = this.Filter.Current;
			if (filter != null)
			{
				PXSelectBase<SOSalesPrice> select = new PXSelectJoin<SOSalesPrice,
					InnerJoin<InventoryItem, On<SOSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>>>,
					Where<SOSalesPrice.curyID, Equal<Current<SalesPriceFilter.curyID>>>>(this);

				if (!string.IsNullOrEmpty(filter.InventoryPriceClassID))
				{
					select.WhereAnd<Where<InventoryItem.priceClassID, Equal<Current<SalesPriceFilter.inventoryPriceClassID>>>>();
				}

				if (filter.InventoryID != null)
				{
					select.WhereAnd<Where<SOSalesPrice.inventoryID, Equal<Current<SalesPriceFilter.inventoryID>>>>();
				}

				return select.Select();
			}
			else
				return null;
		}

		public PXSetup<Company> Company;
		public PXSetup<SOSetup> Setup;
	
		#endregion
		
		#region Buttons/Actions
		public PXCancel<SalesPriceFilter> Cancel;
		public PXSave<SalesPriceFilter> Save;

		#endregion

		#region Event Handlers
		
		protected virtual void SOSalesPrice_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOSalesPrice row = e.Row as SOSalesPrice;
			if (row != null)
			{
				if (Filter.Current != null)
				{
					row.CuryID = Filter.Current.CuryID;
				}
			}
		}

		protected virtual void SOSalesPrice_SalesPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOSalesPrice row = e.Row as SOSalesPrice;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
				if (item != null)
				{
					if (IsBaseCurrency)
					{
						if (row.UOM == item.BaseUnit)
						{
							e.NewValue = item.BasePrice;
						}
						else
						{
							e.NewValue = INUnitAttribute.ConvertFromBase(sender, item.InventoryID, item.SalesUnit, item.BasePrice.Value, INPrecision.UNITCOST);
						}
					}
				}
			}
		}

		protected virtual void SOSalesPrice_PendingPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOSalesPrice row = e.Row as SOSalesPrice;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
				if (item != null)
				{
					if (IsBaseCurrency)
					{
						if (row.UOM == item.BaseUnit)
						{
							e.NewValue = item.PendingBasePrice;
						}
						else
						{
							e.NewValue = INUnitAttribute.ConvertFromBase(sender, item.InventoryID, item.SalesUnit, item.PendingBasePrice.Value, INPrecision.UNITCOST);
						}
					}
				}
			}
		}

		protected virtual void SOSalesPrice_LastPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOSalesPrice row = e.Row as SOSalesPrice;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
				if (item != null)
				{
					if (IsBaseCurrency)
					{
						if (row.UOM == item.BaseUnit)
						{
							e.NewValue = item.LastBasePrice;
						}
						else
						{
							e.NewValue = INUnitAttribute.ConvertFromBase(sender, item.InventoryID, item.SalesUnit, item.LastBasePrice.Value, INPrecision.UNITCOST);
						}
					}
				}
			}
		}

        protected virtual void SOSalesPrice_EffectiveDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            SOSalesPrice row = e.Row as SOSalesPrice;
            if (row != null)
            {
                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
                if (item != null)
                {
                    e.NewValue = item.PendingBasePriceDate;
                }
            }
        }

		protected virtual void SOSalesPrice_LastDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOSalesPrice row = e.Row as SOSalesPrice;
			if (row != null)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);
				if (item != null)
				{
					e.NewValue = item.BasePriceDate;
				}
			}
		}

		protected virtual void SOSalesPrice_SalesPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOSalesPrice row = e.Row as SOSalesPrice;
			if (sender.GetStatus(row) == PXEntryStatus.Updated)
			{
				if (row.SalesPrice != Convert.ToDecimal(e.OldValue))
				{
					row.LastPrice = Convert.ToDecimal(e.OldValue);
					row.LastDate = Accessinfo.BusinessDate;
				}
			}
		}

		protected virtual void SalesPriceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Records.Cache.AllowInsert = false;
			Records.Cache.AllowUpdate = false;
			Records.Cache.AllowDelete = false;

			SalesPriceFilter row = e.Row as SalesPriceFilter;
			if (row != null && row.CuryID != null)
			{
				Records.Cache.AllowInsert = true;
				Records.Cache.AllowUpdate = true;
				Records.Cache.AllowDelete = true;
			}

            if (Setup.Current.AlwaysFromBaseCury == true)
            {
                PXUIFieldAttribute.SetEnabled<SalesPriceFilter.curyID>(sender, e.Row, false);
            }
		}

        protected virtual void SalesPriceFilter_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (Setup.Current.AlwaysFromBaseCury == true)
            {
                e.NewValue = Company.Current.BaseCuryID;
            }
        }

		protected virtual void SOSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOSalesPrice row = (SOSalesPrice)e.Row;

			#region Validations

			if (row.PendingPrice != null && row.EffectiveDate == null)
			{
				if (sender.RaiseExceptionHandling<SOSalesPrice.effectiveDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty)))
				{
					throw new PXRowPersistingException(typeof(SOSalesPrice.effectiveDate).Name, null, ErrorMessages.FieldIsEmpty);
				}
			}

			#endregion

			if (IsBaseCurrency)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>>>.Select(this, row.InventoryID);

				if (sender.GetStatus(e.Row) == PXEntryStatus.Inserted || sender.GetStatus(e.Row) == PXEntryStatus.Updated)
				{
					if (Setup.Current.SalesPriceUpdateUnit == SalesPriceUpdateUnitType.BaseUnit)
					{
						if (row.UOM == item.BaseUnit)
						{
							List<PXDataFieldParam> updatedFields = new List<PXDataFieldParam>();
							updatedFields.Add(new PXDataFieldAssign("LastBasePrice", PXDbType.DirectExpression, "=BasePrice"));
							updatedFields.Add(new PXDataFieldAssign("BasePrice", PXDbType.Decimal, row.SalesPrice));

							if (row.PendingPrice != null)
							{
								updatedFields.Add(new PXDataFieldAssign("PendingBasePrice", PXDbType.Decimal, row.PendingPrice));
								updatedFields.Add(new PXDataFieldAssign("PendingBasePriceDate", PXDbType.DateTime, row.EffectiveDate));
							}

							updatedFields.Add(new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID));
							PXDatabase.Update<InventoryItem>(updatedFields.ToArray());
						}
					}
					else
					{
						if (row.UOM == item.SalesUnit)
						{
							List<PXDataFieldParam> updatedFields = new List<PXDataFieldParam>();
							updatedFields.Add(new PXDataFieldAssign("LastBasePrice", PXDbType.DirectExpression, "=BasePrice"));

							decimal basePrice = INUnitAttribute.ConvertToBase(sender, item.InventoryID, row.UOM, row.SalesPrice.Value, INPrecision.UNITCOST);
							updatedFields.Add(new PXDataFieldAssign("BasePrice", PXDbType.Decimal, basePrice));

							if (row.PendingPrice != null)
							{
								decimal pendingBasePrice = INUnitAttribute.ConvertToBase(sender, item.InventoryID, row.UOM, row.PendingPrice.Value, INPrecision.UNITCOST);
								updatedFields.Add(new PXDataFieldAssign("PendingBasePrice", PXDbType.Decimal, pendingBasePrice));
								updatedFields.Add(new PXDataFieldAssign("PendingBasePriceDate", PXDbType.DateTime, row.EffectiveDate));
							}

							updatedFields.Add(new PXDataFieldRestrict("InventoryID", PXDbType.Int, item.InventoryID));
							PXDatabase.Update<InventoryItem>(updatedFields.ToArray());
						}
					}
				}
			}
		}

		#endregion
		
		private bool IsBaseCurrency
		{
			get 
			{
				return Filter.Current.CuryID == Company.Current.BaseCuryID;
			}
		}


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
        public static decimal? CalculateSalesPrice(PXCache sender, int inventoryID, string curyID, string UOM, DateTime date)
        {
            bool alwaysFromBase = false;
            
            SOSetup sosetup = PXSelectReadonly<SOSetup>.Select(sender.Graph);
            if (sosetup != null)
            {
                alwaysFromBase = sosetup.AlwaysFromBaseCury == true;
            }

            return SOSalesPriceEntry.CalculateSalesPrice(sender, inventoryID, curyID, UOM, date, alwaysFromBase); 
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
        public static decimal? CalculateSalesPrice(PXCache sender, int inventoryID, string curyID, string UOM, DateTime date, bool alwaysFromBaseCurrency)
        {
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, inventoryID);
            decimal? salesPrice = null;

            #region Search for Sales Price in PriceList

            if (alwaysFromBaseCurrency == false)
            {
                SalesPriceItem spItem = FindSalesPrice(sender, inventoryID, curyID, UOM, date);

                if (spItem != null)
                {
                    if (spItem.UOM != UOM)
                    {
                        decimal salesPriceInBase = INUnitAttribute.ConvertToBase(sender, inventoryID, spItem.UOM, spItem.Price, INPrecision.UNITCOST);
                        salesPrice = INUnitAttribute.ConvertFromBase(sender, inventoryID, UOM, salesPriceInBase, INPrecision.UNITCOST);
                    }
                    else
                        salesPrice = spItem.Price;
                }
            }

            #endregion

            if (salesPrice == null)
            {
                #region Calculate from Base Price

                if (item.BasePriceDate != null)
                {
                    decimal salesPriceInBase;

                    if (date < item.BasePriceDate.Value)
                    {
                        //use last price
                        PXCurrencyAttribute.CuryConvCury(sender, null, item.LastBasePrice.Value, out salesPriceInBase);
                    }
                    else
                    {
                        //use current price
                        PXCurrencyAttribute.CuryConvCury(sender, null, item.BasePrice.Value, out salesPriceInBase);
                    }

                    salesPrice = INUnitAttribute.ConvertFromBase(sender, inventoryID, UOM, salesPriceInBase, INPrecision.UNITCOST);
                }

                #endregion
            }

            return salesPrice;
        }

        private static SalesPriceItem FindSalesPrice(PXCache sender, int inventoryID, string curyID, string UOM, DateTime date)
        {
            SOSalesPrice item = PXSelect<SOSalesPrice, Where<SOSalesPrice.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>,
                And<SOSalesPrice.curyID, Equal<SOSalesPrice.curyID>,
                And<SOSalesPrice.uOM, Equal<SOSalesPrice.uOM>>>>>.Select(sender.Graph, inventoryID, curyID, UOM);

            string uomFound = null;

            if (item == null || item.LastDate == null)
            {
                item = PXSelect<SOSalesPrice, Where<SOSalesPrice.inventoryID, Equal<Required<SOSalesPrice.inventoryID>>,
                And<SOSalesPrice.curyID, Equal<SOSalesPrice.curyID>>>>.Select(sender.Graph, inventoryID, curyID);

                if (item == null || item.LastDate == null)
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


            if (date < item.LastDate.Value)
                return new SalesPriceItem(uomFound, item.LastPrice.Value);
            else
                return new SalesPriceItem(uomFound, item.SalesPrice.Value);
        }

        private class SalesPriceItem
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

            public SalesPriceItem(string uom, decimal price)
            {
                this.uom = uom;
                this.price = price;
            }

        }
        
        #endregion
	

		#region Local Types
		[Serializable]
		public class SalesPriceFilter : IBqlTable
		{
			#region InventoryID
			public abstract class inventoryID : PX.Data.IBqlField
			{
			}
			protected Int32? _InventoryID;
			[Inventory()]
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
			#region CuryID
			public abstract class curyID : PX.Data.IBqlField
			{
			}
			protected string _CuryID;
			[PXDBString(5)]
			[PXDefault()]
			[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Currency ID", Required=true)]
			public virtual string CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
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
			[PXUIField(DisplayName = "Price Class ID", Visibility = PXUIVisibility.Visible)]
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

		} 
		#endregion
	}
}
