using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;

using PX.Objects.AR.Repositories;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.Common.Scopes;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AR
{
    [Serializable]
    public class ARPriceWorksheetMaint : PXGraph<ARPriceWorksheetMaint, ARPriceWorksheet>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		#region Selects/Views

		public PXSelect<ARPriceWorksheet> Document;
        [PXImport(typeof(ARPriceWorksheet))]
        public PXSelect<ARPriceWorksheetDetail,
                Where<ARPriceWorksheetDetail.refNbr, Equal<Current<ARPriceWorksheet.refNbr>>>,
                OrderBy<
					Asc<ARPriceWorksheetDetail.priceType, 
					Asc<ARPriceWorksheetDetail.priceCode, 
					Asc<ARPriceWorksheetDetail.inventoryCD, 
					Asc<ARPriceWorksheetDetail.breakQty>>>>>> Details;
        public PXSetup<ARSetup> ARSetup;
        public PXSelect<ARSalesPrice> ARSalesPrices;
		public PXSelect<BAccount,
					Where2<Where<BAccount.type, Equal<BAccountType.customerType>,
						Or<BAccount.type, Equal<BAccountType.combinedType>>>,
						And<Match<Current<AccessInfo.userName>>>>> CustomerCode;
        public PXSelect<Customer> Customer;
        public PXSelect<ARPriceClass> CustPriceClassCode;
		[PXCopyPasteHiddenView]
		public PXSelect<INStockItemXRef> StockCrossReferences;
		[PXCopyPasteHiddenView]
		public PXSelect<INNonStockItemXRef> NonStockCrossReferences;

		[PXCopyPasteHiddenView]
        public PXFilter<CopyPricesFilter> CopyPricesSettings;
        [PXCopyPasteHiddenView]
        public PXFilter<CalculatePricesFilter> CalculatePricesSettings;
        public PXSelect<CurrencyInfo> CuryInfo;
		public PXSetup<Company> company;

		protected readonly CustomerRepository CustomerRepository;

		#endregion
		private readonly bool _loadSalesPricesUsingAlternateID;

		public ARPriceWorksheetMaint()
        {
            ARSetup setup = ARSetup.Current;
			CustomerRepository = new CustomerRepository(this);
	        _loadSalesPricesUsingAlternateID = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && ARSetup.Current.LoadSalesPricesUsingAlternateID == true;

			Details.Cache.Adjust<PXRestrictorAttribute>().For<ARPriceWorksheetDetail.inventoryID>(ra => ra.CacheGlobal = true);
		}

        public string GetPriceType(string viewname)
        {
            string priceType = PriceTypeList.Customer;
            if (viewname.Contains(typeof(ARPriceWorksheetDetail).Name) && Details.Current != null)
                priceType = Details.Current.PriceType;
            if (viewname.Contains(typeof(AddItemParameters).Name) && addItemParameters.Current != null)
                priceType = addItemParameters.Current.PriceType;
            if (viewname.Contains(typeof(CopyPricesFilter).Name) && CopyPricesSettings.Current != null)
                if (viewname.Contains(typeof(CopyPricesFilter.sourcePriceCode).Name.First().ToString().ToUpper()
                    + String.Join(String.Empty, typeof(CopyPricesFilter.sourcePriceCode).Name.Skip(1))))
                    priceType = CopyPricesSettings.Current.SourcePriceType;
                else
                    priceType = CopyPricesSettings.Current.DestinationPriceType;
            return priceType;
        }

        #region Overrides
		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if ((viewName.Contains(typeof(ARPriceWorksheetDetail.priceCode).Name) || viewName.Contains("PriceCode")) && this.ViewNames.ContainsKey(CustomerCode.View) && this.ViewNames.ContainsKey(CustPriceClassCode.View))
			{
				if (GetPriceType(viewName) == PriceTypeList.Customer)
				{
					viewName = this.ViewNames[CustomerCode.View];
					sortcolumns = new string[] { typeof(CR.BAccount.acctCD).Name };
					ModifyFilters(filters, typeof(ARPriceWorksheetDetail.priceCode).Name, typeof(CR.BAccount.acctCD).Name);
					ModifyFilters(filters, typeof(ARPriceWorksheetDetail.description).Name, typeof(CR.BAccount.acctName).Name);
				}
				else
				{
					viewName = this.ViewNames[CustPriceClassCode.View];
					sortcolumns = new string[] { typeof(ARPriceClass.priceClassID).Name };
					ModifyFilters(filters, typeof(ARPriceWorksheetDetail.priceCode).Name, typeof(ARPriceClass.priceClassID).Name);
					ModifyFilters(filters, typeof(ARPriceWorksheetDetail.description).Name, typeof(ARPriceClass.description).Name);
				}
			}
			IEnumerable ret = base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			return ret;
		}

		public override Type GetItemType(string viewName)
		{
			if ((viewName.Contains(typeof(ARPriceWorksheetDetail.priceCode).Name) || viewName.Contains("PriceCode")) && this.ViewNames.ContainsKey(CustomerCode.View) && this.ViewNames.ContainsKey(CustPriceClassCode.View))
			{
				if (GetPriceType(viewName) == PriceTypeList.Customer)
				{
					viewName = this.ViewNames[CustomerCode.View];
				}
				else
				{
					viewName = this.ViewNames[CustPriceClassCode.View];
				}
			}
			return base.GetItemType(viewName);
		}

		private PXFilterRow[] ModifyFilters(PXFilterRow[] filters, string originalFieldName, string newFieldName)
		{
			if (filters != null)
				foreach (PXFilterRow filter in filters)
				{
					if (string.Compare(filter.DataField, originalFieldName, true) == 0)
						filter.DataField = newFieldName;
				}
			return filters;
		}

        public override object GetValueExt(string viewName, object data, string fieldName)
        {
            if (viewName.Contains(typeof(ARPriceWorksheetDetail.priceCode).Name))
            {
                if (GetPriceType(viewName) == PriceTypeList.Customer)
                {
                    viewName = this.ViewNames[CustomerCode.View];
                    if (fieldName == PriceCodeInfo.PriceCodeFieldName)
                        fieldName = typeof(CR.BAccount.acctCD).Name;
                    else if (fieldName == PriceCodeInfo.PriceCodeDescrFieldName)
                        fieldName = typeof(CR.BAccount.acctName).Name;
                    else
                        return null;
                }
                else
                {
                    viewName = this.ViewNames[CustPriceClassCode.View];
                    if (fieldName == PriceCodeInfo.PriceCodeFieldName)
                        fieldName = typeof(ARPriceClass.priceClassID).Name;
                    else if (fieldName == PriceCodeInfo.PriceCodeDescrFieldName)
                        fieldName = typeof(ARPriceClass.description).Name;
                    else
                        return null;
                }
            }
            return base.GetValueExt(viewName, data, fieldName);
        }

		public override void Persist()
		{
			CheckForEmptyPendingPriceDetails();
			CheckForDuplicateDetails();

			base.Persist();
		}

        #endregion

        #region Actions

        public PXAction<ARPriceWorksheet> ReleasePriceWorksheet;
        [PXUIField(DisplayName = ActionsMessages.Release, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable releasePriceWorksheet(PXAdapter adapter)
        {
            List<ARPriceWorksheet> list = new List<ARPriceWorksheet>();
            if (Document.Current != null)
            {
                this.Save.Press();
                list.Add(Document.Current);
                PXLongOperation.StartOperation(this, delegate() { ReleaseWorksheet(Document.Current); });
            }
            return list;
        }

        public static void ReleaseWorksheet(ARPriceWorksheet priceWorksheet)
        {
            ARPriceWorksheetMaint salesPriceWorksheetMaint = PXGraph.CreateInstance<ARPriceWorksheetMaint>();
            salesPriceWorksheetMaint.ReleaseWorksheetImpl(priceWorksheet);
        }

		public virtual void ReleaseWorksheetImpl(ARPriceWorksheet priceWorksheet)
		{
			Document.Current = priceWorksheet;

			bool isPromotional = priceWorksheet.IsPromotional == true;
			bool isFairValue = priceWorksheet?.IsFairValue == true;

			var secondGroupFields = new Type[]
			{
				typeof(ARSalesPrice.priceType),
				typeof(ARSalesPrice.customerID),
				typeof(ARSalesPrice.custPriceClassID),
				typeof(ARSalesPrice.siteID),
				typeof(ARSalesPrice.uOM),
				typeof(ARSalesPrice.curyID),
				typeof(ARSalesPrice.breakQty)
			};
			var firstGroupFields = new Type[]
			{
				typeof(ARSalesPrice.inventoryID),
			};

			var pricesGroups = new List<ARSalesPrice>()
				.SplitBy(this.Caches<ARSalesPrice>(), secondGroupFields)
				.SplitBy(firstGroupFields, group => LoadInventoryPrices(group, isPromotional, isFairValue));

            using (PXTransactionScope ts = new PXTransactionScope())
            {
				using (new GroupedCollectionScope<ARSalesPrice>(pricesGroups))
				{
					foreach (ARPriceWorksheetDetail detail in Details.SelectMain().OrderBy(x => x.InventoryID))
					{
						var group = CreateSalesPrice(detail, isPromotional, null, null);

						var prices = pricesGroups.GetItems(group)
							.OrderBy(x => x.EffectiveDate)
							.ThenBy(x => x.ExpirationDate);

						var pricesSet = new PXResultset<ARSalesPrice>();
						pricesSet.AddRange(prices.Select(x => new PXResult<ARSalesPrice>(x)));
						
						CreateSalesPricesOnWorksheetRelease(detail, pricesSet);
					}
                }

                priceWorksheet.Status = SPWorksheetStatus.Released;
				Document.Update(priceWorksheet);
				Document.Current.Status = SPWorksheetStatus.Released;

				Persist();

                ts.Complete();
            }
        }

		protected virtual IEnumerable<ARSalesPrice> LoadInventoryPrices(ARSalesPrice group, bool isPromotional, bool isFairValue)
		{
			return new SelectFrom<ARSalesPrice>
				.Where<ARSalesPrice.inventoryID.IsEqual<@P.AsInt>
					.And<ARSalesPrice.isPromotionalPrice.IsEqual<@P.AsBool>
					.And<ARSalesPrice.isFairValue.IsEqual<@P.AsBool>>>>.View.ReadOnly(this).SelectMain(group.InventoryID, isPromotional, isFairValue);
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2)]
        protected virtual void CreateSalesPricesOnWorksheetRelease(PXResult<ARPriceWorksheetDetail, InventoryItem> row)
			=> CreateSalesPricesOnWorksheetRelease(row, GetSalesPricesByPriceLineForWorksheetRelease(row));

		protected virtual void CreateSalesPricesOnWorksheetRelease(ARPriceWorksheetDetail priceLine, PXResultset<ARSalesPrice> salesPrices)
        {
			InventoryItem item = InventoryItem.PK.Find(this, priceLine.InventoryID);
            DateTime? worksheetExpirationDate = Document.Current.ExpirationDate;
            DateTime? worksheetEffectiveDate = Document.Current.EffectiveDate;

            if (Document.Current.IsPromotional != true || worksheetExpirationDate == null)
            {
                ProcessReleaseForNonPromotionalSalesPrices(salesPrices, priceLine);
            }
            else
            {
                ProcessReleaseForPromotionalSalesPrices(salesPrices, priceLine);
            }

            if (ARSetup.Current.RetentionType == RetentionTypeList.FixedNumOfMonths && ARSetup.Current.NumberOfMonths != 0)
            {
                foreach (ARSalesPrice salesPrice in salesPrices)
                {
                    int numberOfMonths = ARSetup.Current.NumberOfMonths ?? 0;

                    if (salesPrice.ExpirationDate != null && salesPrice.ExpirationDate.Value.AddMonths(numberOfMonths) < worksheetEffectiveDate)
                    {
                        ARSalesPrices.Delete(salesPrice);
                    }
                }
            }

            if (!_loadSalesPricesUsingAlternateID || priceLine.AlternateID.IsNullOrEmpty())
                return;

            bool xRefExists = PriceWorksheetAlternateItemAttribute.XRefsExists(Details.Cache, priceLine);

            if (xRefExists)
                return;

            PXCache xRefCache = item.StkItem == true
                ? StockCrossReferences.Cache
                : NonStockCrossReferences.Cache;

            INItemXRef newXRef = (INItemXRef)xRefCache.CreateInstance();
            newXRef.InventoryID = priceLine.InventoryID;
            newXRef.AlternateType = INAlternateType.Global;
            newXRef.AlternateID = priceLine.AlternateID;
            newXRef.UOM = priceLine.UOM;
            newXRef.BAccountID = 0;
            newXRef.SubItemID = priceLine.SubItemID ?? item.DefaultSubItemID;

            newXRef = (INItemXRef)xRefCache.Insert(newXRef);
        }

        protected virtual PXResultset<ARSalesPrice> GetSalesPricesByPriceLineForWorksheetRelease(ARPriceWorksheetDetail priceLine)
        {
           return PXSelect<ARSalesPrice,
                     Where<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>,
                     And2<
                        Where2<
                            Where<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>,
                              And<ARSalesPrice.priceType, Equal<PriceTypes.customer>>>,
                            Or2<
                                Where<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>,
                                  And<ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>>>,
                                Or<
                                   Where<ARSalesPrice.custPriceClassID, IsNull,
                                     And<ARSalesPrice.customerID, IsNull,
                                     And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>>>>>>>,
                    And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
                    And2<
                        Where<ARSalesPrice.siteID, Equal<Required<ARSalesPrice.siteID>>,
                           Or<ARSalesPrice.siteID, IsNull,
                          And<Required<ARSalesPrice.siteID>, IsNull>>>,
                    And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
                    And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
                    And<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>,
                    And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>,
					And<ARSalesPrice.isFairValue, Equal<Required<ARSalesPrice.isFairValue>>>>>>>>>>>,
                OrderBy<
                        Asc<ARSalesPrice.effectiveDate,
                        Asc<ARSalesPrice.expirationDate>>>>
                .Select(this,
                        priceLine.PriceType,
                        priceLine.CustomerID,
                        priceLine.CustPriceClassID,
                        priceLine.InventoryID,
                        priceLine.SiteID, priceLine.SiteID,
                        priceLine.UOM,
                        priceLine.CuryID,
                        priceLine.BreakQty,
                        Document.Current.IsPromotional ?? false,
						Document.Current.IsFairValue ?? false);
        }

        protected virtual void ProcessReleaseForNonPromotionalSalesPrices(PXResultset<ARSalesPrice> salesPrices, ARPriceWorksheetDetail priceLine)
        {
            bool insertNewPrice = true;
            DateTime? worksheetExpirationDate = Document.Current.ExpirationDate;
            DateTime? worksheetEffectiveDate = Document.Current.EffectiveDate;
            const bool isPromotional = false;
			bool? isFairValue = Document.Current.IsFairValue;
			bool? isProrated = Document.Current.IsProrated;

			if (salesPrices.Count == 0)
            {
                ARSalesPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, isFairValue, isProrated, worksheetEffectiveDate, worksheetExpirationDate);
                ARSalesPrices.Insert(newSalesPrice);
                return;
            }

            foreach (ARSalesPrice salesPrice in salesPrices)
            {
                if (ARSetup.Current.RetentionType == RetentionTypeList.FixedNumOfMonths)
                {
                    if (Document.Current.OverwriteOverlapping != true &&
                        ((salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate >= worksheetEffectiveDate) ||
                          salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null))
                    {
                        insertNewPrice = false;
                    }

					ProcessNonPromotionalPriceForFixedNumOfMonthsRetentionType(salesPrice, priceLine, worksheetExpirationDate,
																				worksheetEffectiveDate, isFairValue, isProrated);
                }
                else
                {
                    if (salesPrice.EffectiveDate >= worksheetEffectiveDate ||
                        (worksheetEffectiveDate.Value != null && salesPrice.ExpirationDate < worksheetEffectiveDate.Value.AddDays(-1)))
                    {
                        ARSalesPrices.Delete(salesPrice);
                    }
                    else if (((salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null) ||
                              (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate == null) || 
                              ((salesPrice.EffectiveDate < worksheetEffectiveDate || salesPrice.EffectiveDate == null) && worksheetEffectiveDate <= salesPrice.ExpirationDate)) &&
                              worksheetEffectiveDate.Value != null)
                    {
                        salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                        salesPrice.EffectiveDate = null;
                        ARSalesPrices.Update(salesPrice);
                    }
                }
            }

            if (!insertNewPrice)
                return;

            if (Document.Current.OverwriteOverlapping == true || ARSetup.Current.RetentionType == RetentionTypeList.LastPrice)
            {
                ARSalesPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, isFairValue, isProrated, worksheetEffectiveDate, worksheetExpirationDate);
                ARSalesPrices.Insert(newSalesPrice);
            }
            else
            {
                ARSalesPrice minSalesPrice = GetMinSalesPriceForNonPromotionalPricesWorksheetRelease(priceLine, worksheetEffectiveDate);
                DateTime? newSalesPriceExpirationDate = minSalesPrice?.EffectiveDate?.AddDays(-1) ?? worksheetExpirationDate;
                ARSalesPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, isFairValue, isProrated, worksheetEffectiveDate, newSalesPriceExpirationDate);

                ARSalesPrices.Insert(newSalesPrice);
            }
        }

        protected virtual void ProcessNonPromotionalPriceForFixedNumOfMonthsRetentionType(ARSalesPrice salesPrice, ARPriceWorksheetDetail priceLine,
                                                                                          DateTime? worksheetExpirationDate, DateTime? worksheetEffectiveDate)
			=> ProcessNonPromotionalPriceForFixedNumOfMonthsRetentionType(salesPrice, priceLine, worksheetExpirationDate, worksheetEffectiveDate, false, false);

		protected virtual void ProcessNonPromotionalPriceForFixedNumOfMonthsRetentionType(ARSalesPrice salesPrice, ARPriceWorksheetDetail priceLine,
																							DateTime? worksheetExpirationDate, DateTime? worksheetEffectiveDate,
																							bool? isFairValue, bool? isProrated)
		{
            if (Document.Current.OverwriteOverlapping == true)
            {
                if ((worksheetExpirationDate == null && salesPrice.EffectiveDate >= worksheetEffectiveDate) ||
                    (worksheetExpirationDate != null && salesPrice.EffectiveDate >= worksheetEffectiveDate && salesPrice.EffectiveDate <= worksheetExpirationDate))
                {
                    ARSalesPrices.Delete(salesPrice);
                }
                else if (((salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null) ||
                          (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate == null) ||
                          (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate >= worksheetEffectiveDate) ||
                          (salesPrice.EffectiveDate < worksheetEffectiveDate && worksheetEffectiveDate <= salesPrice.ExpirationDate)) && worksheetEffectiveDate.Value != null)
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    ARSalesPrices.Update(salesPrice);
                }
            }
            else
            {               
                if (salesPrice.EffectiveDate < worksheetEffectiveDate && salesPrice.ExpirationDate >= worksheetEffectiveDate && worksheetEffectiveDate.Value != null)
                {
                    ARSalesPrice newSalesPrice = (ARSalesPrice)ARSalesPrices.Cache.CreateCopy(salesPrice);
                    salesPrice.EffectiveDate = worksheetEffectiveDate;

                    UpdateSalesPriceFromPriceLine(salesPrice, priceLine);
                    ARSalesPrices.Update(salesPrice);

                    newSalesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    newSalesPrice.RecordID = null;
                    ARSalesPrices.Insert(newSalesPrice);
                }
                else if (salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null && worksheetEffectiveDate.Value != null)
                {
                    const bool isPromotional = false;
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    ARSalesPrices.Update(salesPrice);

                    ARSalesPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, isFairValue, isProrated, worksheetEffectiveDate, worksheetExpirationDate);
                    ARSalesPrices.Insert(newSalesPrice);
                }
                else if (salesPrice.EffectiveDate == worksheetEffectiveDate && salesPrice.ExpirationDate == worksheetExpirationDate)
                {
                    UpdateSalesPriceFromPriceLine(salesPrice, priceLine);
                    ARSalesPrices.Update(salesPrice);
                }
                else if ((salesPrice.EffectiveDate == null && salesPrice.ExpirationDate == null) || (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate >= worksheetEffectiveDate))
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    ARSalesPrices.Update(salesPrice);
                }
            }
        }

        protected virtual void UpdateSalesPriceFromPriceLine(ARSalesPrice salesPrice, ARPriceWorksheetDetail priceLine)
        {
            salesPrice.SalesPrice = priceLine.PendingPrice;
        }

        protected virtual ARSalesPrice GetMinSalesPriceForNonPromotionalPricesWorksheetRelease(ARPriceWorksheetDetail priceLine, DateTime? effectiveDate)
        {
            const object[] currents = null;
            return PXSelect<ARSalesPrice,
                      Where<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>,
                       And2<
                           Where2<
                              Where<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>,
                                And<ARSalesPrice.custPriceClassID, IsNull>>,
                              Or2<
                                  Where<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>,
                                    And<ARSalesPrice.customerID, IsNull>>,
                                  Or<
                                     Where<ARSalesPrice.custPriceClassID, IsNull,
                                       And<ARSalesPrice.customerID, IsNull>>>>>,
                           And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
                           And2<
                               Where<ARSalesPrice.siteID, Equal<Required<ARSalesPrice.siteID>>,
                                  Or<ARSalesPrice.siteID, IsNull,
                                 And<Required<ARSalesPrice.siteID>, IsNull>>>,
                                 And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
                                 And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
                                 And<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>,
                                 And<ARSalesPrice.effectiveDate, IsNotNull,
                                 And<
                                     Where<ARSalesPrice.effectiveDate, GreaterEqual<Required<ARSalesPrice.effectiveDate>>>>>>>>>>>>,
                    OrderBy<
                            Asc<ARSalesPrice.effectiveDate>>>
                    .SelectSingleBound(this, currents,
                                       priceLine.PriceType,
                                       priceLine.CustomerID,
                                       priceLine.CustPriceClassID,
                                       priceLine.InventoryID,
                                       priceLine.SiteID, priceLine.SiteID,
                                       priceLine.UOM,
                                       priceLine.CuryID,
                                       priceLine.BreakQty,
                                       effectiveDate);
        }

        protected virtual void ProcessReleaseForPromotionalSalesPrices(PXResultset<ARSalesPrice> salesPrices, ARPriceWorksheetDetail priceLine)
        {
            DateTime? worksheetExpirationDate = Document.Current.ExpirationDate;
            DateTime? worksheetEffectiveDate = Document.Current.EffectiveDate;

            foreach (ARSalesPrice salesPrice in salesPrices)
            {
                if (salesPrice.EffectiveDate >= worksheetEffectiveDate && salesPrice.ExpirationDate <= worksheetExpirationDate)
                {
                    ARSalesPrices.Delete(salesPrice);
                }
                else if (salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate <= worksheetExpirationDate
                    && salesPrice.ExpirationDate >= worksheetEffectiveDate && worksheetEffectiveDate.Value != null)
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    ARSalesPrices.Update(salesPrice);
                }
                else if (salesPrice.EffectiveDate >= worksheetEffectiveDate && salesPrice.EffectiveDate < worksheetExpirationDate
                    && salesPrice.ExpirationDate >= worksheetExpirationDate && worksheetExpirationDate.Value != null)
                {
                    salesPrice.EffectiveDate = worksheetExpirationDate.Value.AddDays(1);
                    ARSalesPrices.Update(salesPrice);
                }
                else if (salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate >= worksheetExpirationDate
                    && salesPrice.ExpirationDate > worksheetEffectiveDate && worksheetEffectiveDate.Value != null)
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    ARSalesPrices.Update(salesPrice);
                }
            }

            const bool isPromotional = true;
            ARSalesPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, worksheetEffectiveDate, worksheetExpirationDate);
            ARSalesPrices.Insert(newSalesPrice);
        }

		protected virtual ARSalesPrice CreateSalesPrice(ARPriceWorksheetDetail priceLine, bool? isPromotional, DateTime? effectiveDate, DateTime? expirationDate)
			=> CreateSalesPrice(priceLine, isPromotional, false, false, effectiveDate, expirationDate);

		protected virtual ARSalesPrice CreateSalesPrice(ARPriceWorksheetDetail priceLine, bool? isPromotional, bool? isFairValue, bool? isProrated, DateTime? effectiveDate, DateTime? expirationDate)
        {
            ARSalesPrice newSalesPrice = new ARSalesPrice
            {
                PriceType = priceLine.PriceType,
                CustomerID = priceLine.CustomerID,
                CustPriceClassID = priceLine.CustPriceClassID,
                InventoryID = priceLine.InventoryID,
                SiteID = priceLine.SiteID,
                UOM = priceLine.UOM,
                BreakQty = priceLine.BreakQty,
                SalesPrice = priceLine.PendingPrice,
                CuryID = priceLine.CuryID,
                TaxID = priceLine.TaxID,
                IsPromotionalPrice = isPromotional,
				IsFairValue = isFairValue,
				IsProrated = isProrated,
				EffectiveDate = effectiveDate,
                ExpirationDate = expirationDate
            };

            return newSalesPrice;
        }

        #region AddItem Lookup
        [PXCopyPasteHiddenView]
        public PXFilter<AddItemFilter> addItemFilter;

        [PXCopyPasteHiddenView]
        public PXFilter<AddItemParameters> addItemParameters;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        public ARAddItemLookup<ARAddItemSelected, AddItemFilter> addItemLookup;

		public PXAction<ARPriceWorksheet> addItem;

        [PXUIField(DisplayName = Messages.AddItem, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable AddItem(PXAdapter adapter)
        {
            if (addItemLookup.AskExt() == WebDialogResult.OK)
            {
                return AddSelItems(adapter);
            }

            return adapter.Get();
        }

        public PXAction<ARPriceWorksheet> addSelItems;

        [PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddSelItems(PXAdapter adapter)
        {
            if ((addItemParameters.Current.PriceType != PriceTypes.BasePrice && addItemParameters.Current.PriceCode != null) ||
                (addItemParameters.Current.PriceType == PriceTypes.BasePrice && addItemParameters.Current.PriceCode == null))
            {
                foreach (ARAddItemSelected line in addItemLookup.Cache.Cached)
                {
                    if (line.Selected != true)
                        continue;

                    ARPriceWorksheetDetail newDetail = CreateWorksheetDetailOnAddSelItems(line);
                    Details.Update(newDetail);
                }

                addItemFilter.Cache.Clear();
                addItemLookup.Cache.Clear();
                addItemParameters.Cache.Clear();
            }
            else if (string.IsNullOrEmpty(addItemParameters.Current.PriceCode))
            {
                var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, typeof(AddItemParameters.priceCode).Name);
                addItemParameters.Cache.RaiseExceptionHandling<AddItemParameters.priceCode>(addItemParameters.Current, 
                                                                                            addItemParameters.Current.PriceCode,
                                                                                        exception);
            }

            return adapter.Get();
        }

		public PXAction<ARPriceWorksheet> addAllItems;

		[PXUIField(DisplayName = Messages.AddAll, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddAllItems(PXAdapter adapter)
		{
			if ((addItemParameters.Current.PriceType != PriceTypes.BasePrice && addItemParameters.Current.PriceCode != null) ||
				(addItemParameters.Current.PriceType == PriceTypes.BasePrice && addItemParameters.Current.PriceCode == null))
			{
				foreach (ARAddItemSelected line in addItemLookup.Select())
				{
					if (line.InventoryID != (PMInventorySelectorAttribute.EmptyInventoryID))
					{
						ARPriceWorksheetDetail newDetail = CreateWorksheetDetailOnAddSelItems(line);
						Details.Update(newDetail);
					}
				}

				addItemFilter.Cache.Clear();
				addItemLookup.Cache.Clear();
				addItemParameters.Cache.Clear();
			}
			else if (string.IsNullOrEmpty(addItemParameters.Current.PriceCode))
			{
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, typeof(AddItemParameters.priceCode).Name);
				addItemParameters.Cache.RaiseExceptionHandling<AddItemParameters.priceCode>(addItemParameters.Current,
																							addItemParameters.Current.PriceCode,
																						exception);
			}

			return adapter.Get();
		}

		protected virtual ARPriceWorksheetDetail CreateWorksheetDetailOnAddSelItems(ARAddItemSelected line)
        {
            string priceCode = addItemParameters.Current.PriceCode;

            if (addItemParameters.Current.PriceType == PriceTypeList.Customer)
            {
                Customer customer = CustomerRepository.FindByCD(addItemParameters.Current.PriceCode);

                if (customer != null)
                {
                    priceCode = customer.BAccountID.ToString();
                }
            }

            ARPriceWorksheetDetail newWorksheetDetail = new ARPriceWorksheetDetail
            {
                InventoryID = line.InventoryID,
                SiteID = addItemParameters.Current.SiteID,
                CuryID = addItemParameters.Current.CuryID,
                UOM = line.BaseUnit,
                PriceType = addItemParameters.Current.PriceType
            };

            newWorksheetDetail.CurrentPrice = GetItemPrice(this, addItemParameters.Current.PriceType, priceCode, newWorksheetDetail.InventoryID,
                                                           newWorksheetDetail.CuryID, Document.Current.EffectiveDate);

            if (addItemParameters.Current.PriceType == PriceTypes.Customer)
                newWorksheetDetail.CustomerID = Convert.ToInt32(priceCode);
            else
                newWorksheetDetail.CustPriceClassID = priceCode;

            newWorksheetDetail.PriceCode = addItemParameters.Current.PriceCode;
            return newWorksheetDetail;
        }

        public override IEnumerable<PXDataRecord> ProviderSelect(BqlCommand command, int topCount, params PXDataValue[] pars)
        {
            return base.ProviderSelect(command, topCount, pars);
        }
        #endregion

        public PXAction<ARPriceWorksheet> copyPrices;

        [PXUIField(DisplayName = Messages.CopyPrices, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CopyPrices(PXAdapter adapter)
        {
            if (CopyPricesSettings.AskExt() == WebDialogResult.OK)
            {
                if (CopyPricesSettings.Current == null)
                {
                    return adapter.Get();
                }
                else if (CopyPricesSettings.Current.SourcePriceType != PriceTypes.BasePrice && CopyPricesSettings.Current.SourcePriceCode == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.sourcePriceCode>(adapter);
                }
                else if (CopyPricesSettings.Current.SourceCuryID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.sourceCuryID>(adapter);
                }
                else if (CopyPricesSettings.Current.EffectiveDate == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.effectiveDate>(adapter);                   
                }
                else if (CopyPricesSettings.Current.DestinationPriceType != PriceTypes.BasePrice && CopyPricesSettings.Current.DestinationPriceCode == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.destinationPriceCode>(adapter);                   
                }
                else if (CopyPricesSettings.Current.DestinationCuryID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.destinationCuryID>(adapter);                   
                }
                else if (CopyPricesSettings.Current.DestinationCuryID != CopyPricesSettings.Current.SourceCuryID && CopyPricesSettings.Current.RateTypeID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.rateTypeID>(adapter);                    
                }

                PXLongOperation.StartOperation(this, delegate () { CopyPricesProc(Document.Current, CopyPricesSettings.Current); });
            }

            return adapter.Get();
        }

        private IEnumerable SetErrorOnEmptyFieldAndReturn<TField>(PXAdapter adapter)
        where TField : IBqlField
        {
            PXSetPropertyException e = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(TField).Name);
            CopyPricesSettings.Cache.RaiseExceptionHandling<TField>(CopyPricesSettings.Current, null, e);
            return adapter.Get();
        }

        public static void CopyPricesProc(ARPriceWorksheet priceWorksheet, CopyPricesFilter copyFilter)
		{
			ARPriceWorksheetMaint salesPriceWorksheetMaint = PXGraph.CreateInstance<ARPriceWorksheetMaint>();
			salesPriceWorksheetMaint.Document.Update((ARPriceWorksheet)salesPriceWorksheetMaint.Document.Cache.CreateCopy(priceWorksheet));
            salesPriceWorksheetMaint.CopyPricesInternalProcessing(copyFilter);

            PXRedirectHelper.TryRedirect(salesPriceWorksheetMaint, PXRedirectHelper.WindowMode.Same);
		}

        protected virtual void CopyPricesInternalProcessing(CopyPricesFilter copyFilter)
        {
            CopyPricesSettings.Current = copyFilter;
            string sourcePriceCode = copyFilter.SourcePriceCode;

            if (copyFilter.SourcePriceType == PriceTypeList.Customer)
            {
                Customer customer = CustomerRepository.FindByCD(copyFilter.SourcePriceCode);

                if (customer != null)
                {
                    sourcePriceCode = customer.BAccountID.ToString();
                }
            }

            string destinationPriceCode = copyFilter.DestinationPriceCode;

            if (copyFilter.DestinationPriceType == PriceTypeList.Customer)
            {
                Customer customer = CustomerRepository.FindByCD(copyFilter.DestinationPriceCode);

                if (customer != null)
                {
                    destinationPriceCode = customer.BAccountID.ToString();
                }
            }

            PXResultset<ARSalesPrice> salesPrices = GetPricesForCopying(copyFilter, sourcePriceCode);

            salesPrices.AsEnumerable().Select(price => CreateWorksheetDetailFromSalesPriceOnCopying(price, copyFilter, destinationPriceCode))
                       .ForEach(newLine => Details.Update(newLine));
            
            Save.Press();
            CopyPricesSettings.Cache.Clear();
        }

        protected virtual PXResultset<ARSalesPrice> GetPricesForCopying(CopyPricesFilter copyFilter, string sourcePriceCode)
        {
            PXResultset<ARSalesPrice> salesPrices =
                PXSelectJoinGroupBy<ARSalesPrice,
                    LeftJoin<InventoryItem,
                        On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>>>,
                    Where<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
                        And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
                        And<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>,
                    And2<
                        Where2<
                            Where<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>,
                              And<ARSalesPrice.custPriceClassID, IsNull>>,
                            Or2<
                                Where<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>,
                                  And<ARSalesPrice.customerID, IsNull>>,
                                Or<
                                   Where<ARSalesPrice.custPriceClassID, IsNull,
                                     And<ARSalesPrice.customerID, IsNull>>>>>,
                        And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
                        And2<
                            AreSame<ARSalesPrice.siteID, Required<ARSalesPrice.siteID>>,
                                And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>,
								And<ARSalesPrice.isFairValue, Equal<Required<ARSalesPrice.isFairValue>>,
								And<ARSalesPrice.isProrated, Equal<Required<ARSalesPrice.isProrated>>,
								And<
                                    Where2<
                                           Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
                                             And<ARSalesPrice.expirationDate, IsNull>>,
                                           Or<
                                              Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
                                                And<ARSalesPrice.expirationDate, Greater<Required<ARSalesPrice.effectiveDate>>>>>>>>>>>>>>>>,
                    Aggregate<
                              GroupBy<ARSalesPrice.priceType,
                              GroupBy<ARSalesPrice.customerID,
                              GroupBy<ARSalesPrice.custPriceClassID,
                              GroupBy<ARSalesPrice.inventoryID,
                              GroupBy<ARSalesPrice.uOM,
                              GroupBy<ARSalesPrice.breakQty,
                              GroupBy<ARSalesPrice.curyID,
                              GroupBy<ARSalesPrice.siteID>>>>>>>>>,
                    OrderBy<
                            Asc<ARSalesPrice.effectiveDate,
                            Asc<ARSalesPrice.expirationDate>>>>
                .Select(this,
                    copyFilter.SourcePriceType,
                    copyFilter.SourcePriceType == PriceTypes.Customer ? sourcePriceCode : null,
                    copyFilter.SourcePriceType == PriceTypes.CustomerPriceClass ? sourcePriceCode : null,
                    copyFilter.SourceCuryID,
                    copyFilter.SourceSiteID,
                    copyFilter.SourceSiteID,
                    copyFilter.IsPromotional ?? false,
					copyFilter.IsFairValue ?? false,
					copyFilter.IsProrated ?? false,
					copyFilter.EffectiveDate,
                    copyFilter.EffectiveDate,
                    copyFilter.EffectiveDate);

            return salesPrices;
        }

        protected virtual ARPriceWorksheetDetail CreateWorksheetDetailFromSalesPriceOnCopying(ARSalesPrice salesPrice, CopyPricesFilter copyFilter,
                                                                                              string destinationPriceCode)
        {
            ARPriceWorksheetDetail newLine = new ARPriceWorksheetDetail
            {
                PriceType = copyFilter.DestinationPriceType,
                PriceCode = copyFilter.DestinationPriceCode,
                InventoryID = salesPrice.InventoryID,
                SiteID = copyFilter.DestinationSiteID ?? salesPrice.SiteID,
                UOM = salesPrice.UOM,
                BreakQty = salesPrice.BreakQty,
                CuryID = copyFilter.DestinationCuryID,
                TaxID = salesPrice.TaxID
            };

            if (copyFilter.SourceCuryID == copyFilter.DestinationCuryID)
            {
                newLine.CurrentPrice = salesPrice.SalesPrice ?? 0m;
            }
            else
            {
                newLine.CurrentPrice = ConvertSalesPrice(this, copyFilter.RateTypeID, copyFilter.SourceCuryID, copyFilter.DestinationCuryID,
                                                         copyFilter.CurrencyDate, salesPrice.SalesPrice ?? 0m);
            }

            if (CopyPricesSettings.Current.DestinationPriceType == PriceTypes.Customer)
                newLine.CustomerID = Convert.ToInt32(destinationPriceCode);
            else
                newLine.CustPriceClassID = destinationPriceCode;

            return newLine;
        }

        public static decimal ConvertSalesPrice(ARPriceWorksheetMaint graph, string curyRateTypeID, string fromCuryID, string toCuryID, DateTime? curyEffectiveDate, decimal salesPrice)
        {
            decimal result = salesPrice;

            if (curyRateTypeID == null || curyRateTypeID == null || curyEffectiveDate == null)
                return result;

            CurrencyInfo info = new CurrencyInfo();
            info.BaseCuryID = fromCuryID;
            info.CuryID = toCuryID;
            info.CuryRateTypeID = curyRateTypeID;

            info = (CurrencyInfo)graph.CuryInfo.Cache.Update(info);
            info.SetCuryEffDate(graph.CuryInfo.Cache, curyEffectiveDate);
            graph.CuryInfo.Cache.Update(info);

            PXCurrencyAttribute.CuryConvCury(graph.CuryInfo.Cache, info, salesPrice, out result);
            graph.CuryInfo.Cache.Delete(info);
            return result;
        }

        public PXAction<ARPriceWorksheet> calculatePrices;
        [PXUIField(DisplayName = Messages.CalcPendingPrices, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CalculatePrices(PXAdapter adapter)
        {
            if (CalculatePricesSettings.AskExt() == WebDialogResult.OK)
            {
                CalculatePendingPrices(CalculatePricesSettings.Current);
            }
            SelectTimeStamp();
            return adapter.Get();
        }

        protected readonly string viewPriceCode;

		private void CalculatePendingPrices(CalculatePricesFilter settings)
		{
			if (settings == null)
                return;
			
            foreach (ARPriceWorksheetDetail worksheetDetail in Details.Select())
            {
                var r = (PXResult<InventoryItem, INItemCost>)
                    PXSelectJoin<InventoryItem,
                        LeftJoin<INItemCost, 
                            On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                        .SelectWindowed(this, 0, 1, worksheetDetail.InventoryID);

                InventoryItem item = r;
                INItemCost itemCost = r;

                INItemSite iis = PXSelect<INItemSite, 
                                    Where<INItemSite.inventoryID, Equal<Required<INItemSite.inventoryID>>,
                                      And<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>>
                                .SelectWindowed(this, 0, 1, worksheetDetail.InventoryID, worksheetDetail.SiteID);

                
                KeyValuePair<bool, decimal> calcRes = CalculateCorrectedAmountForPendingPricesCalculation(settings, item, itemCost,
                                                                                                          iis, worksheetDetail);
                bool skipUpdate = calcRes.Key;

                if (skipUpdate)
                    continue;

                decimal correctedAmt = calcRes.Value;

                if (settings.CorrectionPercent != null)
                {
                    correctedAmt = correctedAmt * settings.CorrectionPercent.Value * 0.01m;
                }

                if (settings.Rounding != null)
                {
                    correctedAmt = Math.Round(correctedAmt, settings.Rounding.Value, MidpointRounding.AwayFromZero);
                }

                var worksheetDetailCopyForUpdate = (ARPriceWorksheetDetail)Details.Cache.CreateCopy(worksheetDetail);
                worksheetDetailCopyForUpdate.PendingPrice = correctedAmt;
                Details.Update(worksheetDetailCopyForUpdate);

            }
		}

        protected virtual KeyValuePair<bool, decimal> CalculateCorrectedAmountForPendingPricesCalculation(CalculatePricesFilter settings, 
                                                                                                          InventoryItem item, 
                                                                                                          INItemCost itemCost,
                                                                                                          INItemSite itemSite,
                                                                                                          ARPriceWorksheetDetail worksheetDetail)
        {
            decimal? lastCost = itemSite?.LastCost ?? itemCost.LastCost;
            decimal? avgCost = itemSite?.AvgCost ?? itemCost.AvgCost;
            decimal? stdCost = itemSite?.StdCost ?? item.StdCost;
            decimal? recCost = itemSite?.RecPrice ?? item.RecPrice;
            decimal markup = (itemSite?.MarkupPct ?? item.MarkupPct ?? 0m) * 0.01m;

            Func<decimal?, bool> getSkipUpdate = cost => settings.UpdateOnZero != true && (cost == null || cost == 0);

            Func<decimal, decimal> applyMarkUp = cost => cost + markup * cost;

            Func<decimal, decimal> applyConversion =
                amt =>
                {
                    decimal? result;
                    return item.BaseUnit != worksheetDetail.UOM && 
                           TryConvertToBase(Caches[typeof(InventoryItem)], item.InventoryID, worksheetDetail.UOM, amt, out result)
                        ? result.Value
                        : amt;
                };
          
            bool skipUpdate = false;
            decimal correctedAmt = 0;

            switch (settings.PriceBasis)
            {
                case PriceBasisTypes.LastCost:
                    {
                        skipUpdate = getSkipUpdate(lastCost);

                        if (skipUpdate)
                        {
                            return new KeyValuePair<bool, decimal>(skipUpdate, correctedAmt);
                        }

                        decimal correctedAmtInBaseUnit = applyMarkUp(lastCost ?? 0);
                        correctedAmt = applyConversion(correctedAmtInBaseUnit);                       
                        break;
                    }
                case PriceBasisTypes.StdCost:
                    {
                        decimal? costToUse = item.ValMethod == INValMethod.Standard ? stdCost : avgCost;
                        skipUpdate = getSkipUpdate(costToUse);
                        decimal correctedAmtInBaseUnit = applyMarkUp(costToUse ?? 0m);                      
                        correctedAmt = applyConversion(correctedAmtInBaseUnit);
                        break;
                    }
                case PriceBasisTypes.PendingPrice:
                    skipUpdate = getSkipUpdate(worksheetDetail.PendingPrice);
                    correctedAmt = worksheetDetail.PendingPrice ?? 0m;
                    break;
                case PriceBasisTypes.CurrentPrice:
                    skipUpdate = getSkipUpdate(worksheetDetail.CurrentPrice);
                    correctedAmt = worksheetDetail.CurrentPrice ?? 0;
                    break;
                case PriceBasisTypes.RecommendedPrice:
                    skipUpdate = getSkipUpdate(recCost);
                    correctedAmt = recCost ?? 0;
                    break;
            }

            return new KeyValuePair<bool, decimal>(skipUpdate, correctedAmt);
        }

        private bool TryConvertToBase(PXCache cache, int? inventoryID, string uom, decimal value, out decimal? result)
        {
            result = null;

            try
            {
                result = INUnitAttribute.ConvertToBase(cache, inventoryID, uom, value, INPrecision.UNITCOST);
                return true;
            }
            catch (PXUnitConversionException)
            {
                return false;
            }
        }
        #endregion

        #region ARPriceWorksheet event handlers

        protected virtual void ARPriceWorksheet_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            ARPriceWorksheet tran = (ARPriceWorksheet)e.Row;
            if (tran == null) return;

            bool allowEdit = (tran.Status == SPWorksheetStatus.Hold || tran.Status == SPWorksheetStatus.Open);

            ReleasePriceWorksheet.SetEnabled(tran.Hold == false && tran.Status == SPWorksheetStatus.Open);
            addItem.SetEnabled(tran.Hold == true && tran.Status == SPWorksheetStatus.Hold);
            copyPrices.SetEnabled(tran.Hold == true && tran.Status == SPWorksheetStatus.Hold);
            calculatePrices.SetEnabled(tran.Hold == true && tran.Status == SPWorksheetStatus.Hold);

            Details.Cache.AllowInsert = allowEdit;
            Details.Cache.AllowDelete = allowEdit;
            Details.Cache.AllowUpdate = allowEdit;

            Document.Cache.AllowDelete = tran.Status != SPWorksheetStatus.Released;

            PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.hold>(sender, tran, tran.Status != SPWorksheetStatus.Released);
            PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.descr>(sender, tran, allowEdit);
            PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.effectiveDate>(sender, tran, allowEdit);
            PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.expirationDate>(sender, tran, allowEdit && (ARSetup.Current.RetentionType != AR.RetentionTypeList.LastPrice || (ARSetup.Current.RetentionType == AR.RetentionTypeList.LastPrice && tran.IsPromotional == true)));
            PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.isPromotional>(sender, tran, allowEdit);
			PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.isFairValue>(sender, tran, allowEdit);
			PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.isProrated>(sender, tran, allowEdit && tran.IsFairValue == true);
			PXUIFieldAttribute.SetEnabled<ARPriceWorksheet.overwriteOverlapping>(sender, tran, allowEdit && tran.IsPromotional != true && ARSetup.Current.RetentionType != AR.RetentionTypeList.LastPrice);

            if (ARSetup.Current.RetentionType == AR.RetentionTypeList.LastPrice || tran.IsPromotional == true) tran.OverwriteOverlapping = true;
            if (ARSetup.Current.RetentionType == AR.RetentionTypeList.LastPrice && tran.IsPromotional != true) tran.ExpirationDate = null;
		}

		protected virtual void ARPriceWorksheet_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            ARPriceWorksheet doc = (ARPriceWorksheet)e.Row;
            if (doc == null) return;

            doc.Status = doc.Hold == false ? SPWorksheetStatus.Open : SPWorksheetStatus.Hold;
        }

        protected virtual void ARPriceWorksheet_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            ARPriceWorksheet doc = (ARPriceWorksheet)e.Row;
            if (doc == null) return;

            if (doc.IsPromotional == true && doc.ExpirationDate == null)
                sender.RaiseExceptionHandling<ARPriceWorksheet.expirationDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARPriceWorksheet.expirationDate).Name));
        }

        protected virtual void ARPriceWorksheet_EffectiveDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            ARPriceWorksheet doc = (ARPriceWorksheet)e.Row;
            if (doc == null) return;

            if (e.NewValue == null)
                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARPriceWorksheet.effectiveDate).Name);

            if (doc.IsPromotional == true && doc.ExpirationDate != null && doc.ExpirationDate < (DateTime)e.NewValue)
            {
                throw new PXSetPropertyException(PXMessages.LocalizeFormat(Messages.ExpirationLessThanEffective));
            }
        }

        protected virtual void ARPriceWorksheet_ExpirationDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            ARPriceWorksheet doc = (ARPriceWorksheet)e.Row;
            if (doc == null) return;

            if (doc.IsPromotional == true && e.NewValue == null)
                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARPriceWorksheet.expirationDate).Name);

            if (doc.IsPromotional == true && doc.EffectiveDate != null && doc.EffectiveDate > (DateTime)e.NewValue)
            {
                throw new PXSetPropertyException(PXMessages.LocalizeFormat(Messages.ExpirationLessThanEffective));
            }
        }

        #endregion

        #region ARPriceWorksheetDetail event handlers

	    protected virtual void ARPriceWorksheetDetail_PriceCode_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            ARPriceWorksheetDetail det = (ARPriceWorksheetDetail)e.Row;

            if (det != null && det.PriceType != null)
            {
                if (det.PriceType == PriceTypes.Customer)
                {
					string customerCD = det.PriceCode;
					if (string.IsNullOrEmpty(customerCD))
					{
						if (det.CustomerID != null)
						{
					var customer = CustomerRepository.FindByID(det.CustomerID);
                    if (customer != null)
                    {
								customerCD = customer.AcctCD;
								det.PriceCode = customerCD;
							}
                    }
                }
					e.ReturnState = customerCD;
				}
                else
                {
                    if (e.ReturnState == null)
                        e.ReturnState = det.CustPriceClassID;
                    if (det.PriceCode == null)
                        det.PriceCode = det.CustPriceClassID;
                }
            }
        }

        protected virtual void ARPriceWorksheetDetail_PriceCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            ARPriceWorksheetDetail det = (ARPriceWorksheetDetail)e.Row;
            if (det == null) return;
            if (e.NewValue == null)
                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARPriceWorksheetDetail.priceCode).Name);

            if (det.PriceType == PriceTypeList.Customer)
            {
	            CustomerRepository.GetByCD(e.NewValue.ToString());
            }
            if (det.PriceType == PriceTypeList.CustomerPriceClass)
            {
                if (PXSelect<ARPriceClass, Where<ARPriceClass.priceClassID, Equal<Required<ARPriceClass.priceClassID>>>>.Select(this, e.NewValue.ToString()).Count == 0)
                    throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, Messages.CustomerPriceClass, e.NewValue.ToString()));
            }
        }

        protected virtual void ARPriceWorksheetDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            ARPriceWorksheetDetail det = (ARPriceWorksheetDetail)e.Row;
	        if (det != null)
	        {
		        PXUIFieldAttribute.SetEnabled<ARPriceWorksheetDetail.priceCode>(sender, det, det.PriceType != PriceTypeList.BasePrice);
			}
		}

		protected virtual void _(Events.FieldDefaulting<ARPriceWorksheetDetail, ARPriceWorksheetDetail.subItemID> args)
		{
			var inventory = InventoryItem.PK.Find(this, args.Row?.InventoryID);
			if (inventory?.DefaultSubItemOnEntry == true)
				args.NewValue = inventory.DefaultSubItemID;
		}

        protected virtual void ARPriceWorksheetDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            ARPriceWorksheetDetail det = (ARPriceWorksheetDetail)e.Row;
            if (det == null) return;

            if (det.PriceType != null && det.PriceCode != null)
            {
                if (det.PriceType == PriceTypeList.Customer)
                {
	                var customer = CustomerRepository.FindByCD(det.PriceCode);
                    if (customer != null)
                    {
                        det.CustomerID = customer.BAccountID;
                        det.CustPriceClassID = null;
                    }
                }
                else
                {
                    det.CustomerID = null;
                    det.CustPriceClassID = det.PriceCode;
                }
                if (e.ExternalCall && det.InventoryID != null && det.CuryID != null && Document.Current != null && Document.Current.EffectiveDate != null && det.CurrentPrice == 0m)
                    det.CurrentPrice = GetItemPrice(this, det.PriceType, det.PriceCode, det.InventoryID, det.CuryID, Document.Current.EffectiveDate);
            }
			if (IsImportFromExcel && DuplicateFinder != null)
				DuplicateFinder.AddItem(det);
        }

        protected virtual void ARPriceWorksheetDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            ARPriceWorksheetDetail det = (ARPriceWorksheetDetail)e.Row;

            if (det == null)
                return;

            if (!sender.ObjectsEqual<ARPriceWorksheetDetail.priceType>(e.Row, e.OldRow))
                det.PriceCode = null;

            if (e.ExternalCall && 
					(!sender.ObjectsEqual<ARPriceWorksheetDetail.priceCode>(e.Row, e.OldRow) || 
					!sender.ObjectsEqual<ARPriceWorksheetDetail.uOM>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<ARPriceWorksheetDetail.inventoryID>(e.Row, e.OldRow) || 
					!sender.ObjectsEqual<ARPriceWorksheetDetail.curyID>(e.Row, e.OldRow)))
            {
                det.CurrentPrice = GetItemPrice(this, det.PriceType, det.PriceCode, det.InventoryID, det.CuryID, det.UOM, Document.Current.EffectiveDate);
            }

            if (!sender.ObjectsEqual<ARPriceWorksheetDetail.priceCode>(e.Row, e.OldRow))
            {
                if (det.PriceType == PriceTypeList.Customer)
                {
	                var customer = CustomerRepository.FindByCD(det.PriceCode);

					if (customer != null)
                    {
						det.CustomerID = customer.BAccountID;
                        det.CustPriceClassID = null;
                    }
                }
                else
                {
                    det.CustomerID = null;
                    det.CustPriceClassID = det.PriceCode;
                }
            }
        }

		private void CheckForEmptyPendingPriceDetails()
		{
			if (Document.Current?.Hold == true) return;

			IEnumerable<ARPriceWorksheetDetail> emptyPendingPriceDetails = PXSelect<
				ARPriceWorksheetDetail,
				Where<
					ARPriceWorksheetDetail.refNbr, Equal<Current<ARPriceWorksheetDetail.refNbr>>,
					And<ARPriceWorksheetDetail.pendingPrice, IsNull>>>
				.Select(this)
				.RowCast<ARPriceWorksheetDetail>()
				.ToArray();

			foreach (ARPriceWorksheetDetail emptyPendingPriceDetail in emptyPendingPriceDetails)
			{
				Details.Cache.RaiseExceptionHandling<ARPriceWorksheetDetail.pendingPrice>(
					emptyPendingPriceDetail,
					emptyPendingPriceDetail.PendingPrice,
					new PXSetPropertyException(
						ErrorMessages.FieldIsEmpty,
						PXErrorLevel.Error,
						typeof(ARPriceWorksheetDetail.pendingPrice).Name));

				// The user could have just cleared header Hold without modifying any details.
				// Thus, we need to force detail persist to abort the save process, otherwise,
				// the platform will ignore the error.
				// -
				Details.Cache.MarkUpdated(emptyPendingPriceDetail);
			}
		}

        protected virtual void CheckForDuplicateDetails()
		{
			IEnumerable<ARPriceWorksheetDetail> worksheetDetails = PXSelect<
				ARPriceWorksheetDetail,
				Where<
					ARPriceWorksheetDetail.refNbr, Equal<Current<ARPriceWorksheetDetail.refNbr>>>>
				.Select(this)
				.RowCast<ARPriceWorksheetDetail>()
				.ToArray();

			IEqualityComparer<ARPriceWorksheetDetail> duplicateComparer =
				new FieldSubsetEqualityComparer<ARPriceWorksheetDetail>(
					Details.Cache,
					typeof(ARPriceWorksheetDetail.refNbr),
					typeof(ARPriceWorksheetDetail.priceType),
					typeof(ARPriceWorksheetDetail.customerID),
					typeof(ARPriceWorksheetDetail.custPriceClassID),
					typeof(ARPriceWorksheetDetail.inventoryID),
					typeof(ARPriceWorksheetDetail.siteID),
					typeof(ARPriceWorksheetDetail.subItemID),
					typeof(ARPriceWorksheetDetail.uOM),
					typeof(ARPriceWorksheetDetail.curyID),
					typeof(ARPriceWorksheetDetail.breakQty));

			IEnumerable<ARPriceWorksheetDetail> duplicates = worksheetDetails
				.GroupBy(detail => detail, duplicateComparer)
				.Where(duplicatesGroup => duplicatesGroup.HasAtLeastTwoItems())
				.Flatten();
			
			foreach (ARPriceWorksheetDetail duplicate in duplicates)
			{
				Details.Cache.RaiseExceptionHandling<ARPriceWorksheetDetail.priceCode>(
					duplicate,
					duplicate.PriceCode,
					new PXSetPropertyException(
						Messages.DuplicateSalesPriceWS,
						PXErrorLevel.RowError,
						typeof(ARPriceWorksheetDetail.priceCode).Name));
			}
		}
		private decimal GetItemPrice(PXGraph graph, string priceType, string priceCode, int? inventoryID, string toCuryID, string uom, DateTime? curyEffectiveDate)
		{
			InventoryItem inventoryItem = InventoryItem.PK.Find(this, inventoryID);

			decimal? itemPrice = GetItemPriceFromInventoryItem(inventoryItem, uom);
			return itemPrice == null
				? 0m
				: ConvertSalesPrice(this, new CMSetupSelect(this).Current.ARRateTypeDflt, new PXSetup<GL.Company>(this).Current.BaseCuryID,
									toCuryID, curyEffectiveDate, itemPrice.Value);
		}
		private decimal GetItemPrice(PXGraph graph, string priceType, string priceCode, int? inventoryID, string toCuryID, DateTime? curyEffectiveDate)
        {
			return GetItemPrice(graph, priceType, priceCode, inventoryID, toCuryID, null, curyEffectiveDate);
        }

		/// <summary>
		/// Gets item price from inventory item. The extension point used by Lexware PriceUnit customization to adjust inventory item's price.
		/// </summary>
		/// <param name="inventoryItem">The inventory item.</param>
		protected virtual decimal? GetItemPriceFromInventoryItem(InventoryItem inventoryItem)
			=> GetItemPriceFromInventoryItem(inventoryItem, inventoryItem?.BaseUnit);

		protected virtual decimal? GetItemPriceFromInventoryItem(InventoryItem inventoryItem, string uom)
		{
			if (inventoryItem == null)
			{
				return null;
			}
			if (string.IsNullOrEmpty(uom) || uom == inventoryItem.BaseUnit)
			{
				return inventoryItem.BasePrice;
			}
			else
			{
				return INUnitAttribute.ConvertToBase(Caches[typeof(InventoryItem)], //Price should be converted contrariwise quantity conversion
					inventoryItem.InventoryID,
					uom,
					inventoryItem.BasePrice ?? 0m,
					INPrecision.NOROUND);
			}
		}

		protected virtual void ARPriceWorksheetDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            ARPriceWorksheetDetail det = (ARPriceWorksheetDetail)e.Row;

            if (det == null) return;

			if ((det.PriceType == PriceTypes.Customer && det.CustomerID == null) || (det.PriceType == PriceTypes.CustomerPriceClass && det.CustPriceClassID == null))
			{
                sender.RaiseExceptionHandling<ARPriceWorksheetDetail.priceCode>(
					det, 
					null, 
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARPriceWorksheetDetail.priceCode).Name));

				return;
			}

            if (det.PriceType == PriceTypeList.Customer)
            {
	            var customer = CustomerRepository.FindByCD(det.PriceCode);
                if (customer != null)
                {
                    det.CustomerID = customer.BAccountID;
                    det.CustPriceClassID = null;
                }
            }
            else
            {
                det.CustomerID = null;
                det.CustPriceClassID = det.PriceCode;
            }
        }
        #endregion

        #region CopyPricesFilter event handlers

        protected virtual void CopyPricesFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CopyPricesFilter filter = (CopyPricesFilter)e.Row;
            if (filter == null) return;

            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.sourceCuryID>(sender, filter, ARSetup.Current.AlwaysFromBaseCury == false);
            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.destinationCuryID>(sender, filter, ARSetup.Current.AlwaysFromBaseCury == false);

            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.rateTypeID>(sender, filter, filter.SourceCuryID != filter.DestinationCuryID);
            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.currencyDate>(sender, filter, filter.SourceCuryID != filter.DestinationCuryID);

            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.sourcePriceCode>(sender, filter, filter.SourcePriceType != PriceTypeList.BasePrice);
            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.destinationPriceCode>(sender, filter, filter.DestinationPriceType != PriceTypeList.BasePrice);

			bool mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<CopyPricesFilter.sourceCuryID>(sender, filter, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CopyPricesFilter.destinationCuryID>(sender, filter, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CopyPricesFilter.currencyDate>(sender, filter, mcFeatureInstalled);
			PXUIFieldAttribute.SetVisible<CopyPricesFilter.rateTypeID>(sender, filter, mcFeatureInstalled);
        }

		protected virtual void CopyPricesFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CopyPricesFilter parameters = (CopyPricesFilter)e.Row;
            if (parameters == null) return;

            if (!sender.ObjectsEqual<CopyPricesFilter.sourcePriceType>(e.Row, e.OldRow))
                parameters.SourcePriceCode = null;
            if (!sender.ObjectsEqual<CopyPricesFilter.destinationPriceType>(e.Row, e.OldRow))
                parameters.DestinationPriceCode = null;

            if (!sender.ObjectsEqual<CopyPricesFilter.sourcePriceCode>(e.Row, e.OldRow))
            {
                if (parameters.SourcePriceType == PriceTypes.Customer)
                {
                    PXResult<Customer> customer = PXSelect<AR.Customer, Where<AR.Customer.acctCD, Equal<Required<AR.Customer.acctCD>>>>.Select(this, parameters.SourcePriceCode);
                    if (customer != null)
                    {
                        parameters.SourceCuryID = ((Customer)customer).CuryID;
                    }
                }
            }
            if (!sender.ObjectsEqual<CopyPricesFilter.destinationPriceCode>(e.Row, e.OldRow))
            {
                if (parameters.DestinationPriceType == PriceTypes.Customer)
                {
                    PXResult<Customer> customer = PXSelect<AR.Customer, Where<AR.Customer.acctCD, Equal<Required<AR.Customer.acctCD>>>>.Select(this, parameters.DestinationPriceCode);
                    if (customer != null)
                    {
                        parameters.DestinationCuryID = ((Customer)customer).CuryID;
                    }
                }
            }
        }
        #endregion

        #region AddItemParameters event handlers
        protected virtual void AddItemParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AddItemParameters filter = (AddItemParameters)e.Row;
            if (filter == null) return;
			PXUIFieldAttribute.SetVisible<AddItemParameters.curyID>(sender, filter, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
            PXUIFieldAttribute.SetEnabled<AddItemParameters.priceCode>(sender, filter, filter.PriceType != PriceTypeList.BasePrice);
        }

        protected virtual void AddItemParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            AddItemParameters parameters = (AddItemParameters)e.Row;
            if (parameters == null) return;

            if (!sender.ObjectsEqual<AddItemParameters.priceType>(e.Row, e.OldRow))
                parameters.PriceCode = null;

            if (!sender.ObjectsEqual<AddItemParameters.priceCode>(e.Row, e.OldRow))
            {
                if (parameters.PriceType == PriceTypes.Customer)
                {
                    PXResult<Customer> customer = PXSelect<AR.Customer, Where<AR.Customer.acctCD, Equal<Required<AR.Customer.acctCD>>>>.Select(this, parameters.PriceCode);
                    if (customer != null)
                    {
                        if (((Customer)customer).CuryID != null)
                            parameters.CuryID = ((Customer)customer).CuryID;
                        else
                            sender.SetDefaultExt<AddItemParameters.curyID>(e.Row);
                    }
                }
            }
        }
		#endregion
		
		#region INItemXRef event handlers
		protected virtual void INNonStockItemXRef_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void INNonStockItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			=> VerifyINItemXRefBAccountID(e);

		protected virtual void INStockItemXRef_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		    => VerifyINItemXRefBAccountID(e);

		private void VerifyINItemXRefBAccountID(PXFieldVerifyingEventArgs e)
		{
			if (((INItemXRef)e.Row).AlternateType != INAlternateType.VPN && ((INItemXRef)e.Row).AlternateType != INAlternateType.CPN)
			{
				e.Cancel = true;
			}
		}
        #endregion

        #region Find of duplicates

        protected virtual Type[] GetAlternativeKeyFields()
		{
			List<Type> types = new List<Type>()
			{
				typeof(ARPriceWorksheetDetail.priceType),
				typeof(ARPriceWorksheetDetail.priceCode),
				typeof(ARPriceWorksheetDetail.inventoryID),
				typeof(ARPriceWorksheetDetail.uOM),
				typeof(ARPriceWorksheetDetail.siteID),
				typeof(ARPriceWorksheetDetail.curyID),
				typeof(ARPriceWorksheetDetail.taxID)
			};

			if (PXAccess.FeatureInstalled<FeaturesSet.supportBreakQty>())
				types.Add(typeof(ARPriceWorksheetDetail.breakQty));

			return types.ToArray();
		}

		public DuplicatesSearchEngine<ARPriceWorksheetDetail> DuplicateFinder { get; set; }

		private bool DontUpdateExistRecords
		{
			get
			{
				object dontUpdateExistRecords;
				return IsImportFromExcel && PXExecutionContext.Current.Bag.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS, out dontUpdateExistRecords) &&
					true.Equals(dontUpdateExistRecords);
			}
		}

		#endregion

		#region PXImportAttribute.IPXPrepareItems and PXImportAttribute.IPXProcess implementations

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if(viewName.Equals(Details.View.Name, StringComparison.InvariantCultureIgnoreCase) && !DontUpdateExistRecords)
			{
				if (DuplicateFinder == null)
				{
					var details = PXSelect<ARPriceWorksheetDetail,
						Where<ARPriceWorksheetDetail.refNbr, Equal<Current<ARPriceWorksheetDetail.refNbr>>>>.Select(this).RowCast<ARPriceWorksheetDetail>().ToList();
					DuplicateFinder = new DuplicatesSearchEngine<ARPriceWorksheetDetail>(Details.Cache, GetAlternativeKeyFields(), details);
				}
				var duplicate = DuplicateFinder.Find(values);
				if (duplicate != null)
				{
					if (keys.Contains(nameof(ARPriceWorksheetDetail.LineID)))
						keys[nameof(ARPriceWorksheetDetail.LineID)] = duplicate.LineID;
					else
						keys.Add(nameof(ARPriceWorksheetDetail.LineID), duplicate.LineID);
				}
			}
			return true;
		}

		public virtual bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public virtual bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			DuplicateFinder = null;
		}
		#endregion
	}

	public static class PriceCodeInfo
    {
        public const string PriceCodeFieldName = "PriceCode";
        public const string PriceCodeDescrFieldName = "Description";
    }

    [Serializable]
    public partial class AddItemFilter : INSiteStatusFilter
    {
        #region Inventory
        public new abstract class inventory : PX.Data.BQL.BqlString.Field<inventory> { }
        #endregion
        #region PriceClassID
        public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
        protected String _PriceClassID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Price Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<INPriceClass.priceClassID>), DescriptionField = typeof(INItemClass.descr))]
        public virtual String PriceClassID
        {
            get
            {
                return this._PriceClassID;
            }
            set
            {
                this._PriceClassID = value;
            }
        }
        #endregion
        #region CurrentOwnerID
        public abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }

        [PXDBGuid]
        [CR.CRCurrentOwnerID]
        public virtual Guid? CurrentOwnerID { get; set; }
        #endregion
        #region MyOwner
        public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
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
        #region OwnerID
        public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
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
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
        protected Int32? _WorkGroupID;
        [PXDBInt]
        [PXUIField(DisplayName = "Price  Workgroup")]
        [PXSelector(typeof(Search<TM.EPCompanyTree.workGroupID,
            Where<TM.EPCompanyTree.workGroupID, TM.Owned<Current<AccessInfo.userID>>>>),
         SubstituteKey = typeof(TM.EPCompanyTree.description))]
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
        public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
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
    }

    [Serializable]
    public partial class AddItemParameters : IBqlTable
    {
        #region PriceType
        public abstract class priceType : PX.Data.BQL.BqlString.Field<priceType> { }
        protected String _PriceType;
        [PXString(1)]
        [PXDefault(PriceTypeList.CustomerPriceClass)]
        [PriceTypeList.List()]
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
        #region PriceCode
        public abstract class priceCode : PX.Data.BQL.BqlString.Field<priceCode> { }
        protected String _PriceCode;
        [PXString(30, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price Code", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [PXSelector(typeof(ARPriceWorksheetDetail.priceCode), new Type[] { typeof(ARPriceWorksheetDetail.priceCode), typeof(ARPriceWorksheetDetail.description) }, ValidateValue = false)]
        public virtual String PriceCode
        {
            get
            {
                return this._PriceCode;
            }
            set
            {
                this._PriceCode = value;
            }
        }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        protected string _CuryID;
        [PXString(5)]
        [PXDefault(typeof(Search<GL.Company.baseCuryID>))]
        [PXSelector(typeof(CM.Currency.curyID), CacheGlobal = true)]
        [PXUIField(DisplayName = "Currency")]
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
		#region SiteID
		public  abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[NullableSite]
		public Int32? SiteID { get; set; }
		#endregion
    }

    [System.SerializableAttribute()]
    [PXProjection(typeof(Select2<InventoryItem,
        LeftJoin<INItemClass,
                        On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>,
        LeftJoin<INPriceClass,
                        On<INPriceClass.priceClassID, Equal<InventoryItem.priceClassID>>,
        LeftJoin<INUnit,
                    On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>,
                 And<INUnit.fromUnit, Equal<InventoryItem.salesUnit>,
                 And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>
							>>>>,
		Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
			And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
			And<CurrentMatch<InventoryItem, AccessInfo.userName>>>>>>), Persistent = false)]
	public partial class ARAddItemSelected : IBqlTable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion

        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [Inventory(BqlField = typeof(InventoryItem.inventoryID), IsKey = true)]
        [PXDefault()]
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

		#region InventoryCD
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
        protected string _InventoryCD;
        [PXDefault()]
        [InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
        public virtual String InventoryCD
        {
            get
            {
                return this._InventoryCD;
            }
            set
            {
                this._InventoryCD = value;
            }
        }
        #endregion

        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected string _Descr;
        [PXDBLocalizableString(60, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion

        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
        protected int? _ItemClassID;
        [PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class ID", Visible = true)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), ValidComboRequired = true)]
        public virtual int? ItemClassID
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

		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;
		[PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCD
        {
            get
            {
				return this._ItemClassCD;
            }
            set
            {
				this._ItemClassCD = value;
            }
        }
        #endregion

        #region ItemClassDescription
        public abstract class itemClassDescription : PX.Data.BQL.BqlString.Field<itemClassDescription> { }
        protected String _ItemClassDescription;
        [PXDBLocalizableString(Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INItemClass.descr), IsProjection = true)]
        [PXUIField(DisplayName = "Item Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
        public virtual String ItemClassDescription
        {
            get
            {
                return this._ItemClassDescription;
            }
            set
            {
                this._ItemClassDescription = value;
            }
        }
        #endregion

        #region PriceClassID
        public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }

        protected string _PriceClassID;
        [PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
        [PXUIField(DisplayName = "Price Class ID", Visible = true)]
        public virtual String PriceClassID
        {
            get
            {
                return this._PriceClassID;
            }
            set
            {
                this._PriceClassID = value;
            }
        }
        #endregion

        #region PriceClassDescription
        public abstract class priceClassDescription : PX.Data.BQL.BqlString.Field<priceClassDescription> { }
        protected String _PriceClassDescription;
        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INPriceClass.description))]
        [PXUIField(DisplayName = "Price Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
        public virtual String PriceClassDescription
        {
            get
            {
                return this._PriceClassDescription;
            }
            set
            {
                this._PriceClassDescription = value;
            }
        }
        #endregion

        #region BaseUnit
        public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }

        protected string _BaseUnit;
        [INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(InventoryItem.baseUnit))]
        public virtual String BaseUnit
        {
            get
            {
                return this._BaseUnit;
            }
            set
            {
                this._BaseUnit = value;
            }
        }
        #endregion

        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        protected String _CuryID;
        [PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String CuryID
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

        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        protected Int64? _CuryInfoID;
        [PXLong()]
        [CM.CurrencyInfo()]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion

        #region CuryUnitPrice
        public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
        protected Decimal? _CuryUnitPrice;
        [PXUIField(DisplayName = "Last Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryUnitPrice
        {
            get
            {
                return this._CuryUnitPrice;
            }
            set
            {
                this._CuryUnitPrice = value;
            }
        }
        #endregion
        #region PriceWorkgroupID
        public abstract class priceWorkgroupID : PX.Data.BQL.BqlInt.Field<priceWorkgroupID> { }
        protected Int32? _PriceWorkgroupID;
        [PXDBInt(BqlField = typeof(InventoryItem.priceWorkgroupID))]
        [EP.PXWorkgroupSelector]
        [PXUIField(DisplayName = "Price Workgroup")]
        public virtual Int32? PriceWorkgroupID
        {
            get
            {
                return this._PriceWorkgroupID;
            }
            set
            {
                this._PriceWorkgroupID = value;
            }
        }
        #endregion

        #region PriceManagerID
        public abstract class priceManagerID : PX.Data.BQL.BqlGuid.Field<priceManagerID> { }
        protected Guid? _PriceManagerID;
        [PXDBGuid(BqlField = typeof(InventoryItem.priceManagerID))]
        [TM.PXOwnerSelector(typeof(InventoryItem.priceWorkgroupID))]
        [PXUIField(DisplayName = "Price Manager")]
        public virtual Guid? PriceManagerID
        {
            get
            {
                return this._PriceManagerID;
            }
            set
            {
                this._PriceManagerID = value;
            }
        }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(InventoryItem.noteID))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
	}

	public class ARAddItemLookup<Status, StatusFilter> : INSiteStatusLookup<Status, StatusFilter>
        where Status : class, IBqlTable, new()
        where StatusFilter : AddItemFilter, new()
    {
        #region Ctor
        public ARAddItemLookup(PXGraph graph)
            : base(graph)
        {
            graph.RowSelecting.AddHandler(typeof(ARAddItemSelected), OnRowSelecting);
        }

        public ARAddItemLookup(PXGraph graph, Delegate handler)
            : base(graph, handler)
        {
            graph.RowSelecting.AddHandler(typeof(ARAddItemSelected), OnRowSelecting);
        }
        #endregion
        protected virtual void OnRowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            //remove
        }

        protected override void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.OnFilterSelected(sender, e);
            AddItemFilter filter = (AddItemFilter)e.Row;
            PXCache status = sender.Graph.Caches[typeof(ARAddItemSelected)];
            PXUIFieldAttribute.SetVisible<ARAddItemSelected.curyID>(status, null, true);
			PXUIFieldAttribute.SetEnabled<AddItemFilter.workGroupID>(sender, e.Row, filter?.MyWorkGroup != true);
			PXUIFieldAttribute.SetEnabled<AddItemFilter.ownerID>(sender, e.Row, filter?.MyOwner != true);
		}

		protected override PXView CreateIntView(PXGraph graph)
		{
			var view = base.CreateIntView(graph);

			view.WhereAnd<Where<Current<AddItemFilter.ownerID>, IsNull,
						Or<Current<AddItemFilter.ownerID>, Equal<ARAddItemSelected.priceManagerID>>>>();

			view.WhereAnd<Where<Current<AddItemFilter.myWorkGroup>, Equal<boolFalse>,
						 Or<ARAddItemSelected.priceWorkgroupID, TM.InMember<CurrentValue<AddItemFilter.currentOwnerID>>>>>();

			view.WhereAnd<Where<Current<AddItemFilter.workGroupID>, IsNull,
						Or<Current<AddItemFilter.workGroupID>, Equal<ARAddItemSelected.priceWorkgroupID>>>>();

			return view;
        }
    }
    [Serializable]
    public partial class CalculatePricesFilter : IBqlTable
    {
        #region CorrectionPercent
        public abstract class correctionPercent : PX.Data.BQL.BqlDecimal.Field<correctionPercent> { }

        protected Decimal? _CorrectionPercent;

        [PXDefault("100.00")]
        [PXDecimal(6, MinValue = 0, MaxValue = 1000)]
        [PXUIField(DisplayName = "% of Original Price", Visibility = PXUIVisibility.Visible)]
        public virtual Decimal? CorrectionPercent
        {
            get
            {
                return this._CorrectionPercent;
            }
            set
            {
                this._CorrectionPercent = value;
            }
        }
        #endregion

        #region Rounding
        public abstract class rounding : PX.Data.BQL.BqlShort.Field<rounding> { }

        protected Int16? _Rounding;
		[PXDefault((short)2, typeof(Search<CommonSetup.decPlPrcCst>))]
        [PXDBShort(MinValue = 0, MaxValue = 6)]
        [PXUIField(DisplayName = "Decimal Places", Visibility = PXUIVisibility.Visible)]
        public virtual Int16? Rounding
        {
            get
            {
                return this._Rounding;
            }
            set
            {
                this._Rounding = value;
            }
        }
        #endregion

        #region PriceBasis
        public abstract class priceBasis : PX.Data.BQL.BqlString.Field<priceBasis> { }
        protected String _PriceBasis;
        [PXString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Price Basis")]
        [PriceBasisTypes.List()]
        [PXDefault(PriceBasisTypes.CurrentPrice)]
        public virtual String PriceBasis
        {
            get
            {
                return this._PriceBasis;
            }
            set
            {
                this._PriceBasis = value;
            }
        }
        #endregion

        #region UpdateOnZero
        public abstract class updateOnZero : PX.Data.BQL.BqlBool.Field<updateOnZero> { }
        protected bool? _UpdateOnZero;
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Update with Zero Price when Basis is Zero", Visibility = PXUIVisibility.Service)]
        public virtual bool? UpdateOnZero
        {
            get
            {
                return _UpdateOnZero;
            }
            set
            {
                _UpdateOnZero = value;
            }
        }
        #endregion
    }

    public static class PriceBasisTypes
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { LastCost, StdCost, CurrentPrice, PendingPrice, RecommendedPrice },
                new string[] { Messages.LastCost, Messages.StdCost, Messages.CurrentPrice, Messages.PendingPrice, Messages.RecommendedPrice }) { ; }
        }
        public const string LastCost = "L";
        public const string StdCost = "S";
        public const string CurrentPrice = "P";
        public const string PendingPrice = "N";
        public const string RecommendedPrice = "R";
    }

    [Serializable]
    public partial class CopyPricesFilter : IBqlTable
    {
        #region SourcePriceType
        public abstract class sourcePriceType : PX.Data.BQL.BqlString.Field<sourcePriceType> { }
        protected String _SourcePriceType;
        [PXString(1, IsFixed = true)]
        [PXDefault(PriceTypeList.CustomerPriceClass)]
        [PriceTypeList.List()]
        [PXUIField(DisplayName = "Price Type", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String SourcePriceType
        {
            get
            {
                return this._SourcePriceType;
            }
            set
            {
                this._SourcePriceType = value;
            }
        }
        #endregion
        #region SourcePriceCode
        public abstract class sourcePriceCode : PX.Data.BQL.BqlString.Field<sourcePriceCode> { }
        protected String _SourcePriceCode;
        [PXString(30, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [PXDefault()]
        [PXUIField(DisplayName = "Price Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(ARPriceWorksheetDetail.priceCode), new Type[] { typeof(ARPriceWorksheetDetail.priceCode), typeof(ARPriceWorksheetDetail.description) }, ValidateValue = false)]
        public virtual String SourcePriceCode
        {
            get
            {
                return this._SourcePriceCode;
            }
            set
            {
                this._SourcePriceCode = value;
            }
        }
        #endregion
        #region SourceCuryID
        public abstract class sourceCuryID : PX.Data.BQL.BqlString.Field<sourceCuryID> { }
        protected string _SourceCuryID;
        [PXString(5)]
        [PXDefault(typeof(Search<GL.Company.baseCuryID>))]
        [PXSelector(typeof(CM.Currency.curyID))]
        [PXUIField(DisplayName = "Source Currency", Required = true)]
        public virtual string SourceCuryID
        {
            get
            {
                return this._SourceCuryID;
            }
            set
            {
                this._SourceCuryID = value;
            }
        }
        #endregion
		#region SourceSiteID
		public abstract class sourceSiteID : PX.Data.BQL.BqlInt.Field<sourceSiteID> { }
		protected Int32? _SourceSiteID;
		[NullableSite]
		public virtual Int32? SourceSiteID
		{
			get { return this._SourceSiteID; }
			set { this._SourceSiteID = value; }
		}
		#endregion

        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
        protected DateTime? _EffectiveDate;
        [PXDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Effective As Of", Required = true)]
        public virtual DateTime? EffectiveDate
        {
            get
            {
                return this._EffectiveDate;
            }
            set
            {
                this._EffectiveDate = value;
            }
        }
        #endregion
        #region IsPromotional
        public abstract class isPromotional : PX.Data.BQL.BqlBool.Field<isPromotional> { }
        protected bool? _IsPromotional;
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Promotional Price")]
        public virtual bool? IsPromotional
        {
            get
            {
                return _IsPromotional;
            }
            set
            {
                _IsPromotional = value;
            }
        }
		#endregion
		#region IsFairValue
		public abstract class isFairValue : IBqlField { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fair Value", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsFairValue { get; set; }
		#endregion
		#region IsProrated
		public abstract class isProrated : IBqlField { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Prorated", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsProrated { get; set; }
		#endregion

		#region DestinationPriceType
		public abstract class destinationPriceType : PX.Data.BQL.BqlString.Field<destinationPriceType> { }

        protected String _DestinationPriceType;
        [PXString(1, IsFixed = true)]
        [PXDefault(PriceTypeList.CustomerPriceClass)]
        [PriceTypeList.List()]
        [PXUIField(DisplayName = "Price Type", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String DestinationPriceType
        {
            get
            {
                return this._DestinationPriceType;
            }
            set
            {
                this._DestinationPriceType = value;
            }
        }
        #endregion
        #region DestinationPriceCode
        public abstract class destinationPriceCode : PX.Data.BQL.BqlString.Field<destinationPriceCode> { }
        protected String _DestinationPriceCode;
        [PXString(30, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [PXDefault()]
        [PXUIField(DisplayName = "Price Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(ARPriceWorksheetDetail.priceCode), new Type[] { typeof(ARPriceWorksheetDetail.priceCode), typeof(ARPriceWorksheetDetail.description) }, ValidateValue = false)]
        public virtual String DestinationPriceCode
        {
            get
            {
                return this._DestinationPriceCode;
            }
            set
            {
                this._DestinationPriceCode = value;
            }
        }
        #endregion
        #region DestinationCuryID
        public abstract class destinationCuryID : PX.Data.BQL.BqlString.Field<destinationCuryID> { }
        protected string _DestinationCuryID;
        [PXString(5)]
        [PXDefault(typeof(Search<GL.Company.baseCuryID>))]
        [PXSelector(typeof(CM.Currency.curyID))]
        [PXUIField(DisplayName = "Destination Currency", Required = true)]
        public virtual string DestinationCuryID
        {
            get
            {
                return this._DestinationCuryID;
            }
            set
            {
                this._DestinationCuryID = value;
            }
        }
		#endregion
		#region DestinationSiteID
		public abstract class destinationSiteID : PX.Data.BQL.BqlInt.Field<destinationSiteID> { }
		protected Int32? _DestinationSiteID;
		[NullableSite]
		public virtual Int32? DestinationSiteID
		{
			get { return this._DestinationSiteID; }
			set { this._DestinationSiteID = value; }
		}
		#endregion

		#region RateTypeID
		public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
        protected String _RateTypeID;
        [PXString(6)]
        [PXDefault(typeof(ARSetup.defaultRateTypeID))]
        [PXSelector(typeof(PX.Objects.CM.CurrencyRateType.curyRateTypeID))]
        [PXUIField(DisplayName = "Rate Type")]
        public virtual String RateTypeID
        {
            get
            {
                return this._RateTypeID;
            }
            set
            {
                this._RateTypeID = value;
            }
        }
        #endregion
        #region CurrencyDate
        public abstract class currencyDate : PX.Data.BQL.BqlDateTime.Field<currencyDate> { }
        protected DateTime? _CurrencyDate;
        [PXDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Currency Effective Date")]
        public virtual DateTime? CurrencyDate
        {
            get
            {
                return this._CurrencyDate;
            }
            set
            {
                this._CurrencyDate = value;
            }
        }
        #endregion
        #region CustomRate
        public abstract class customRate : PX.Data.BQL.BqlDecimal.Field<customRate> { }
        protected Decimal? _CustomRate;
        [PXDefault("1.00")]
        [PXDecimal(6, MinValue = 0)]
        [PXUIField(DisplayName = "Currency Rate", Visibility = PXUIVisibility.Visible)]
        public virtual Decimal? CustomRate
        {
            get
            {
                return this._CustomRate;
            }
            set
            {
                this._CustomRate = value;
            }
        }
		#endregion
	}
}
