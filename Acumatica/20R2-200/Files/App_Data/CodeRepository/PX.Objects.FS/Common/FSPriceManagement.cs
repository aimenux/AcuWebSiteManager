using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    /// <summary>
    /// Contains the information related to a price for an inventory item.
    /// </summary>
    public class SalesPriceSet
    {
        public SalesPriceSet(string priceCode, decimal? price, string priceType, int? customerID, string errorCode)
        {
            this.PriceCode = priceCode;
            this.Price = price;
            this.PriceType = priceType;
            this.CustomerID = customerID;
            this.ErrorCode = errorCode;
        }

        public string PriceCode { get; set; }

        public decimal? Price { get; set; }

        public string PriceType { get; set; }

        public int? CustomerID { get; set; }

        public string ErrorCode { get; set; }
    }

    public class FSPriceManagement : PXGraph<FSPriceManagement>
    {
        private static readonly Lazy<FSPriceManagement> _fsPriceManagement = new Lazy<FSPriceManagement>(CreateInstance<FSPriceManagement>);

        public static FSPriceManagement SingleFSPriceManagement => _fsPriceManagement.Value;

        public string errorCode;

        /// <summary>
        /// Determine the PriceCode value depending of PriceType of the calculated price.
        /// </summary>
        private static void DeterminePriceCode(PXCache cache, ref SalesPriceSet salesPriceSet)
        {
            if (salesPriceSet.PriceType == ID.PriceType.CUSTOMER)
            {
                List<object> args = new List<object>();

                PXView appointmentRecordsView;

                BqlCommand customerBql = new Select<Customer,
                                             Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>();

                appointmentRecordsView = new PXView(cache.Graph, true, customerBql);

                args.Add(salesPriceSet.CustomerID);

                Customer customerRow = (Customer)appointmentRecordsView.SelectSingle(args.ToArray());

                salesPriceSet.PriceCode = customerRow.AcctCD;
            }
            else if (salesPriceSet.PriceType == ID.PriceType.BASE || salesPriceSet.PriceType == ID.PriceType.DEFAULT)
            {
                salesPriceSet.PriceCode = string.Empty;
            }
        }

        /// <summary>
        /// Calculates the price retrieving the correct price depending of the price set for that item.
        /// </summary>
        public static SalesPriceSet CalculateSalesPriceWithCustomerContract(PXCache cache, 
                                                                            int? serviceContractID,
                                                                            int? billServiceContractID,
                                                                            int? billContractPeriodID,
                                                                            int? customerID,
                                                                            int? customerLocationID,
                                                                            bool? lineRelatedToContract,
                                                                            int? inventoryID,
                                                                            int? siteID,
                                                                            decimal? quantity,
                                                                            string uom, DateTime date,
                                                                            decimal? currentUnitPrice,
                                                                            bool alwaysFromBaseCurrency,
                                                                            CurrencyInfo currencyInfo,
                                                                            bool catchSalesPriceException)
        {
            decimal? salesPrice;

            // It is necessary to set at midnight the time because the Effective Date nad Expiration Date are stored at midnight
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            if (currencyInfo == null)
            {
                // Get the currency info in the company to retrieve the prices
                currencyInfo = new CurrencyInfo();
                currencyInfo.BaseCuryID = cache.Graph.Accessinfo.BaseCuryID;
                currencyInfo.CuryID = cache.Graph.Accessinfo.BaseCuryID;
                currencyInfo.CuryRate = 1;
                currencyInfo.CuryMultDiv = CuryMultDivType.Mult;
                currencyInfo.CuryEffDate = cache.Graph.Accessinfo.BusinessDate;
            }

            // If the item has a price from the Contract then retrieves that price
            salesPrice = GetCustomerContractPrice(cache, currencyInfo, serviceContractID, billServiceContractID, billContractPeriodID, lineRelatedToContract, inventoryID);

            SalesPriceSet salesPriceSet;

            if (salesPrice == null)
            {
                // Calculates/Retrieves the price depending of the price set into Acumatica
                salesPriceSet = FSPriceManagement.CalculateSalesPrice(cache, customerID, customerLocationID, inventoryID, siteID, currencyInfo, quantity, uom, date, currentUnitPrice, alwaysFromBaseCurrency, catchSalesPriceException);
                DeterminePriceCode(cache, ref salesPriceSet);
            }
            else
            {
                salesPriceSet = new SalesPriceSet(string.Empty, salesPrice, ID.PriceType.CONTRACT, customerID, ID.PriceErrorCode.OK);
            }

            return salesPriceSet;
        }

        /// <summary>
        /// Gets the price for the item in the contract if it exists.
        /// </summary>
        private static decimal? GetCustomerContractPrice(PXCache cache, 
                                                         int? serviceContractID,
                                                         int? billServiceContractID,
                                                         int? billContractPeriodID,
                                                         bool? lineRelatedToContract,
                                                         int? inventoryID)
        {
            if (serviceContractID == null && billServiceContractID == null)
            {
                return null;
            }

            decimal? salesPrice = null;

            FSServiceContract fsServiceContractRow = PXSelect<FSServiceContract,
                                                     Where<
                                                         FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                     .Select(cache.Graph, serviceContractID);

            if (fsServiceContractRow == null)
            {
                fsServiceContractRow = PXSelect<FSServiceContract,
                                       Where<
                                           FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                       .Select(cache.Graph, billServiceContractID);
            }

            if (fsServiceContractRow != null
                    && fsServiceContractRow.BillingType == ID.Contract_BillingType.AS_PERFORMED_BILLINGS
                    && fsServiceContractRow.SourcePrice == ID.SourcePrice.CONTRACT)
            {
                FSSalesPrice fsSalesPriceRow = PXSelect<FSSalesPrice,
                                               Where<
                                                   FSSalesPrice.serviceContractID, Equal<Required<FSSalesPrice.serviceContractID>>,
                                               And<
                                                   FSSalesPrice.inventoryID, Equal<Required<FSSalesPrice.inventoryID>>>>>
                                               .Select(cache.Graph, serviceContractID, inventoryID);

                if (fsSalesPriceRow != null)
                {
                    salesPrice = fsSalesPriceRow.UnitPrice;
                }
            }
            else if (fsServiceContractRow != null
                    && fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS
                    && lineRelatedToContract == true)
            {
                FSContractPeriodDet fsContractPeriodDetRow = PXSelect<FSContractPeriodDet,
                                                             Where<
                                                                 FSContractPeriodDet.serviceContractID, Equal<Required<FSContractPeriodDet.serviceContractID>>,
                                                             And<
                                                                 FSContractPeriodDet.contractPeriodID, Equal<Required<FSContractPeriodDet.contractPeriodID>>,
                                                             And<
                                                                 FSContractPeriodDet.inventoryID, Equal<Required<FSContractPeriodDet.inventoryID>>>>>>
                                                             .Select(cache.Graph, billServiceContractID, billContractPeriodID,inventoryID);

                if (fsContractPeriodDetRow != null)
                {
                    salesPrice = fsContractPeriodDetRow.RecurringUnitPrice;
                }
            }

            return salesPrice;
        }

        private static decimal? GetCustomerContractPrice(
            PXCache cache,
            CurrencyInfo info,
            int? serviceContractID,
            int? billServiceContractID,
            int? billContractPeriodID,
            bool? lineRelatedToContract,
            int? inventoryID)
        {
            decimal? contractSalesPrice = GetCustomerContractPrice(cache, serviceContractID, billServiceContractID, billContractPeriodID, lineRelatedToContract, inventoryID);

            if (contractSalesPrice != null 
                && info != null) 
            {
                decimal curyval;
                PXDBCurrencyAttribute.CuryConvCury(cache, info, (decimal)contractSalesPrice, out curyval);
                return curyval;
            }

            return contractSalesPrice;
        }

        /// <summary>
        /// Calculates/Retrieves the price for an item depending on the price set for it.
        /// </summary>
        private static SalesPriceSet CalculateSalesPrice(PXCache cache,
                                                         int? customerID,
                                                         int? customerLocationID,
                                                         int? inventoryID,
                                                         int? siteID,
                                                         CurrencyInfo currencyinfo,
                                                         decimal? quantity,
                                                         string uom,
                                                         DateTime date,
                                                         decimal? currentUnitPrice,
                                                         bool alwaysFromBaseCurrency,
                                                         bool catchSalesPriceException)
        {
            string custPriceClass = ARPriceClass.EmptyPriceClass;
            string errorCode = ID.PriceErrorCode.OK;

            // Check if it exists a price by customerID
            string priceType = CheckPriceByPriceType(cache, 
                                                     custPriceClass, 
                                                     customerID, 
                                                     inventoryID,
                                                     currencyinfo.BaseCuryID, 
                                                     alwaysFromBaseCurrency ? currencyinfo.BaseCuryID : currencyinfo.CuryID,
                                                     Math.Abs(quantity ?? 0m), 
                                                     uom, 
                                                     date, 
                                                     ID.PriceType.CUSTOMER,
                                                     ref errorCode);

            // If it does not exist a price by customerID, then verify if it exists at least a Customer Price Class
            if (priceType == null)
            {
                custPriceClass = SingleFSPriceManagement.DetermineCustomerPriceClass(cache, customerID, customerLocationID, inventoryID, currencyinfo, quantity, uom, date, alwaysFromBaseCurrency);

                // If it does not exist a price by Customer Price Class for the item, then try to verify the BASE price or DEFAULT price
                if (custPriceClass == ARPriceClass.EmptyPriceClass)
                {
                    priceType = CheckPriceByPriceType(cache, 
                                                      custPriceClass, 
                                                      customerID, 
                                                      inventoryID, 
                                                      currencyinfo.BaseCuryID, 
                                                      alwaysFromBaseCurrency ? currencyinfo.BaseCuryID : currencyinfo.CuryID, 
                                                      Math.Abs(quantity ?? 0m), 
                                                      uom, 
                                                      date, 
                                                      ID.PriceType.BASE,
                                                      ref errorCode);
                }
                else
                {
                    priceType = ID.PriceType.PRICE_CLASS;
                }
            }

            decimal? price = null;

            try
            {
                if (alwaysFromBaseCurrency == true)
                {
                    price = ARSalesPriceMaint.CalculateSalesPrice(cache, custPriceClass, customerID, inventoryID, currencyinfo, quantity, uom, date, alwaysFromBaseCurrency: true);
                }
                else
                {
                    price = ARSalesPriceMaint.CalculateSalesPrice(cache, custPriceClass, customerID, inventoryID, siteID, currencyinfo, uom, quantity, date, currentUnitPrice);
                }
            }
            catch (PXUnitConversionException exception)
            {
                if (catchSalesPriceException == true)
                {
                    return new SalesPriceSet(custPriceClass, price, priceType, customerID, ID.PriceErrorCode.UOM_INCONSISTENCY);
                }
                else
                {
                    throw exception;
                }
            }

            SalesPriceSet salesPriceSet = new SalesPriceSet(custPriceClass, price, priceType, customerID, errorCode);

            return salesPriceSet;
        }

        /// <summary>
        /// Determines if an item has a Customer Price Class defined depending on the Customer Location.
        /// </summary>
        public virtual string DetermineCustomerPriceClass(PXCache cache,
                                                          int? customerID,
                                                          int? customerLocationID,
                                                          int? inventoryID,
                                                          CurrencyInfo currencyinfo,
                                                          decimal? quantity,
                                                          string uom,
                                                          DateTime date,
                                                          bool alwaysFromBaseCurrency)
        {
            // Sets to BASE by default
            string defaultCustomerPriceClass = ARPriceClass.EmptyPriceClass;
            errorCode = ID.PriceErrorCode.OK;

            if (customerLocationID == null || customerID == null)
            {
                return defaultCustomerPriceClass;
            }

            Location locationRow = PXSelect<Location,
                                   Where<
                                       Location.bAccountID, Equal<Required<Location.bAccountID>>,
                                   And<
                                       Location.locationID, Equal<Required<Location.locationID>>>>>
                                   .Select(cache.Graph, customerID, customerLocationID);

            if (locationRow == null)
            {
                return defaultCustomerPriceClass;
            }

            // Checks if the first Customer Price Class is defined or specified for the item
            if (string.IsNullOrEmpty(locationRow.CPriceClassID) == false &&
                    CheckPriceByPriceType(cache,
                                          locationRow.CPriceClassID,
                                          customerID,
                                          inventoryID,
                                          currencyinfo.BaseCuryID,
                                          alwaysFromBaseCurrency ? currencyinfo.BaseCuryID : currencyinfo.CuryID,
                                          Math.Abs(quantity ?? 0m),
                                          uom,
                                          date,
                                          ID.PriceType.PRICE_CLASS,
                                          ref errorCode) != null)
            {
                return locationRow.CPriceClassID;
            }

            return defaultCustomerPriceClass;
        }

        /// <summary>
        /// Returns true if for the Customer and/or Customer Price Class there is a price defined for the item.
        /// </summary>
        private static string CheckPriceByPriceType(PXCache cache, string custPriceClass, int? customerID, int? inventoryID, string baseCuryID, string curyID, decimal? quantity, string uom, DateTime date, string type, ref string errorCode)
        {
            // These BQLs are necessary to evaluate if the item has a price defined for a Customer o Customer Price Class specified
            PXSelectBase<ARSalesPrice> salesPrice = new PXSelect<ARSalesPrice,
                                                        Where<
                                                            ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
                                                        And2<
                                                            Where2<
                                                                Where<
                                                                    ARSalesPrice.priceType, Equal<PriceTypes.customer>,
                                                                    And<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>, 
                                                                    And<Required<ARSalesPrice.custPriceClassID>, IsNull>>>,
                                                                Or2<
                                                                    Where<
                                                                        ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>,
                                                                        And<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>, 
                                                                        And<Required<ARSalesPrice.customerID>, IsNull>>>,
                                                                    Or<
                                                                        Where<
                                                                            ARSalesPrice.priceType, Equal<PriceTypes.basePrice>,
                                                                            And<Required<ARSalesPrice.customerID>, IsNull,
                                                                            And<Required<ARSalesPrice.custPriceClassID>, IsNull>>>>>>,
                                                            And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
                                                            And<
                                                                Where2<
                                                                    Where<
                                                                        ARSalesPrice.breakQty, LessEqual<Required<ARSalesPrice.breakQty>>>,
                                                                And<
                                                                    Where2<
                                                                        Where<
                                                                            ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
                                                                            And<ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPrice.expirationDate>>>>,
                                                                        Or2<
                                                                            Where<
                                                                                ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
                                                                                And<ARSalesPrice.expirationDate, IsNull>>,
                                                                            Or<
                                                                                Where<
                                                                                    ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPrice.expirationDate>>,
                                                                                    And<ARSalesPrice.effectiveDate, IsNull,
                                                                                    Or<ARSalesPrice.effectiveDate, IsNull, And<ARSalesPrice.expirationDate, IsNull>>>>>>>>>>>>>,
                                                        OrderBy<
                                                            Asc<ARSalesPrice.priceType, 
                                                            Desc<ARSalesPrice.isPromotionalPrice, 
                                                            Desc<ARSalesPrice.breakQty>>>>>(cache.Graph);

            PXSelectBase<ARSalesPrice> selectWithBaseUOM = new PXSelectJoin<ARSalesPrice,
                                                               InnerJoin<InventoryItem,
                                                                    On<
                                                                       InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>,
                                                                       And<InventoryItem.baseUnit, Equal<ARSalesPrice.uOM>>>>,
                                                               Where<
                                                                   ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
                                                                   And2<
                                                                       Where2<
                                                                           Where<
                                                                               ARSalesPrice.priceType, Equal<PriceTypes.customer>,
                                                                               And<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>, 
                                                                               And<Required<ARSalesPrice.custPriceClassID>, IsNull>>>,
                                                                           Or2<
                                                                               Where<
                                                                                   ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>,
                                                                                   And<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>, 
                                                                                   And<Required<ARSalesPrice.customerID>, IsNull>>>,
                                                                               Or<
                                                                                   Where<
                                                                                       ARSalesPrice.priceType, Equal<PriceTypes.basePrice>,
                                                                                       And<Required<ARSalesPrice.customerID>, IsNull,
                                                                                       And<Required<ARSalesPrice.custPriceClassID>, IsNull>>>>>>,
                                                                       And<
                                                                           ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
                                                                           And<
                                                                               Where2<
                                                                                   Where<
                                                                                       ARSalesPrice.breakQty, LessEqual<Required<ARSalesPrice.breakQty>>>,
                                                                                   And<
                                                                                       Where2<
                                                                                           Where<
                                                                                               ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
                                                                                               And<ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPrice.expirationDate>>>>,
                                                                                           Or2<
                                                                                               Where<
                                                                                                   ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
                                                                                                   And<ARSalesPrice.expirationDate, IsNull>>,
                                                                                               Or<
                                                                                                   Where<
                                                                                                       ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPrice.expirationDate>>,
                                                                                                       And<ARSalesPrice.effectiveDate, IsNull,
                                                                                                       Or<ARSalesPrice.effectiveDate, IsNull, And<ARSalesPrice.expirationDate, IsNull>>>>>>>>>>>>>,
                                                               OrderBy<
                                                                   Asc<ARSalesPrice.priceType, 
                                                                   Desc<ARSalesPrice.isPromotionalPrice, 
                                                                   Desc<ARSalesPrice.breakQty>>>>>(cache.Graph);

            string priceType = null;
            ARSalesPrice item;

            switch (type)
            { 
                case ID.PriceType.CUSTOMER:

                    item = salesPrice.SelectWindowed(0, 1, inventoryID, customerID, null, custPriceClass, customerID, customerID, custPriceClass, curyID, quantity, date, date, date, date);
                    errorCode = CheckInventoryItemUOM(cache, item, inventoryID, uom);

                    if (item == null && errorCode == ID.PriceErrorCode.OK)
                    {
                        decimal baseUnitQty = INUnitAttribute.ConvertToBase(cache, inventoryID, uom, (decimal)quantity, INPrecision.QUANTITY);
                        item = selectWithBaseUOM.Select(inventoryID, customerID, null, custPriceClass, customerID, customerID, custPriceClass, curyID, baseUnitQty, date, date, date, date);
                    }

                    if (item != null && errorCode == ID.PriceErrorCode.OK)
                    {
                        priceType = ID.PriceType.CUSTOMER;
                    }

                    break;

                case ID.PriceType.PRICE_CLASS:

                    item = salesPrice.SelectWindowed(0, 1, inventoryID, customerID, custPriceClass, custPriceClass, null, customerID, custPriceClass, curyID, quantity, date, date, date, date);
                    errorCode = CheckInventoryItemUOM(cache, item, inventoryID, uom);

                    if (item == null && errorCode == ID.PriceErrorCode.OK)
                    {
                        decimal baseUnitQty = INUnitAttribute.ConvertToBase(cache, inventoryID, uom, (decimal)quantity, INPrecision.QUANTITY);
                        item = selectWithBaseUOM.Select(inventoryID, customerID, custPriceClass, custPriceClass, null, customerID, custPriceClass, curyID, baseUnitQty, date, date, date, date);
                    }

                    if (item != null && errorCode == ID.PriceErrorCode.OK)
                    {
                        priceType = ID.PriceType.PRICE_CLASS;
                    }

                    break;

                default:

                    item = salesPrice.SelectWindowed(0, 1, inventoryID, customerID, custPriceClass, custPriceClass, customerID, null, null, curyID, quantity, date, date, date, date);
                    errorCode = CheckInventoryItemUOM(cache, item, inventoryID, uom);

                    if (item == null)
                    {
                        decimal baseUnitQty = INUnitAttribute.ConvertToBase(cache, inventoryID, uom, (decimal)quantity, INPrecision.QUANTITY);
                        item = selectWithBaseUOM.Select(inventoryID, customerID, custPriceClass, custPriceClass, customerID, null, null, curyID, baseUnitQty, date, date, date, date);

                        if (item == null)
                        {
                            priceType = ID.PriceType.DEFAULT;
                        }
                    }

                    if (item != null && errorCode == ID.PriceErrorCode.OK)
                    {
                        priceType = ID.PriceType.BASE;
                    }

                    break;
            }

            return priceType;
        }

        /// <summary>
        /// Verifies whether or not the system can convert the InventoryItem's <c>UOM</c> to the one defined in the Sales Price screen.
        /// If <c>arSalesPriceRow != null</c> means that the price from Sale Price applies.
        /// If <c>arSalesPriceRow.UOM != uom</c> means that the <c>UOM</c> conversion applies.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="arSalesPriceRow"><c>ARSalesPrice</c> instance.</param>
        /// <param name="inventoryID">Inventory Item ID.</param>
        /// <param name="uom">Unit of measure required.</param>
        /// <returns>Returns an errorCode status.</returns>
        private static string CheckInventoryItemUOM(PXCache cache, ARSalesPrice arSalesPriceRow, int? inventoryID, string uom)
        {
            string errorCode = ID.PriceErrorCode.OK;

            if (arSalesPriceRow != null && arSalesPriceRow.UOM != uom)
            {
                try
                {
                    decimal value = 0m;
                    decimal salesprice = INUnitAttribute.ConvertFromBase(cache, inventoryID, arSalesPriceRow.UOM, value, INPrecision.NOROUND);
                }
                catch (PXException e)
                {
                    //The error code that can be thrown by ConvertFromBase routine is PX.Objects.IN.Messages.MissingUnitConversion, though,
                    //the error message is captured in case it does not match it for unknown reasons.
                    errorCode = e.Message;

                    //At this point it's ensured that e.Message corresponds to MissingUnitConversion,
                    //changing the errorCode value to a more explanatory message.
                    if (e.Message.IndexOf(PX.Objects.IN.Messages.MissingUnitConversion) != -1)
                    {
                        errorCode = ID.PriceErrorCode.UOM_INCONSISTENCY;
                    }
                }
            }

            return errorCode;
        }
    }
}
