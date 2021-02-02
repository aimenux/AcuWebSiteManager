using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;

using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Api;
using PX.Objects.Common.Scopes;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AP
{
    [Serializable]
    public class APPriceWorksheetMaint : PXGraph<APPriceWorksheetMaint, APPriceWorksheet>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
    {
        #region Selects/Views

        public PXSelect<APPriceWorksheet> Document;

        [PXImport(typeof(APPriceWorksheet))]
        public PXSelect<APPriceWorksheetDetail,
                Where<APPriceWorksheetDetail.refNbr, Equal<Current<APPriceWorksheet.refNbr>>>,
                OrderBy<
					Asc<APPriceWorksheetDetail.vendorID, 
					Asc<APPriceWorksheetDetail.inventoryCD, 
					Asc<APPriceWorksheetDetail.breakQty>>>>> Details;

        public PXSetup<APSetup> APSetup;
        public PXSelect<APVendorPrice> APVendorPrices;
        public PXSelect<Vendor> Vendor;

        [PXCopyPasteHiddenView]
        public PXFilter<CopyPricesFilter> CopyPricesSettings;
        [PXCopyPasteHiddenView]
        public PXFilter<CalculatePricesFilter> CalculatePricesSettings;
        public PXSelect<CurrencyInfo> CuryInfo;
		[PXCopyPasteHiddenView]
		public PXSelect<INStockItemXRef> StockCrossReferences;
		[PXCopyPasteHiddenView]
		public PXSelect<INNonStockItemXRef> NonStockCrossReferences;
		public PXSetup<Company> company;

		#endregion

		#region Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Default<APPriceWorksheetDetail.vendorID>))]
		protected virtual void APPriceWorksheetDetail_PendingPrice_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault]
		[PXFormula(typeof(IsNull<
			Selector<APPriceWorksheetDetail.vendorID, Vendor.curyID>,
			Current<Company.baseCuryID>>))]
		protected virtual void APPriceWorksheetDetail_CuryID_CacheAttached(PXCache sender) { }

		#endregion
		private readonly bool _loadVendorsPricesUsingAlternateID;

		public APPriceWorksheetMaint()
        {
            APSetup setup = APSetup.Current;
	        _loadVendorsPricesUsingAlternateID = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && APSetup.Current.LoadVendorsPricesUsingAlternateID == true;
			PXUIFieldAttribute.SetVisible<APPriceWorksheetDetail.alternateID>(Details.Cache, null, _loadVendorsPricesUsingAlternateID);

			Details.Cache.Adjust<PXRestrictorAttribute>().For<APPriceWorksheetDetail.inventoryID>(ra => ra.CacheGlobal = true);
		}

        #region Actions

        public PXAction<APPriceWorksheet> ReleasePriceWorksheet;
        [PXUIField(DisplayName = ActionsMessages.Release, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable releasePriceWorksheet(PXAdapter adapter)
        {
            List<APPriceWorksheet> list = new List<APPriceWorksheet>();
            if (Document.Current != null)
            {
                this.Save.Press();
                list.Add(Document.Current);
                PXLongOperation.StartOperation(this, delegate() { ReleaseWorksheet(Document.Current); });
            }
            return list;
        }

        public static void ReleaseWorksheet(APPriceWorksheet priceWorksheet)
        {
            var graph = PXGraph.CreateInstance<APPriceWorksheetMaint>();
			graph.ReleaseWorksheetImpl(priceWorksheet);
        }

		public virtual void ReleaseWorksheetImpl(APPriceWorksheet priceWorksheet)
		{
			Document.Current = priceWorksheet;

			bool isPromotional = priceWorksheet.IsPromotional == true;

			var secondGroupFields = new Type[]
			{
				typeof(APVendorPrice.vendorID),
				typeof(APVendorPrice.siteID),
				typeof(APVendorPrice.uOM),
				typeof(APVendorPrice.curyID),
				typeof(APVendorPrice.breakQty)
			};
			var firstGroupFields = new Type[]
			{
				typeof(APVendorPrice.inventoryID)
			};

			var pricesGroups = new List<APVendorPrice>()
				.SplitBy(this.Caches<APVendorPrice>(), secondGroupFields)
				.SplitBy(firstGroupFields, group => LoadInventoryPrices(group, isPromotional));

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				using (new GroupedCollectionScope<APVendorPrice>(pricesGroups))
				{
					foreach (APPriceWorksheetDetail detail in Details.SelectMain().OrderBy(x => x.InventoryID))
					{
						var inventory = InventoryItem.PK.Find(this, detail.InventoryID);

						var group = CreateSalesPrice(detail, isPromotional, null, null);

						var prices = pricesGroups.GetItems(group)
							.OrderBy(x => x.EffectiveDate)
							.ThenBy(x => x.ExpirationDate);

						var pricesSet = new PXResultset<APVendorPrice>();
						pricesSet.AddRange(prices.Select(x => new PXResult<APVendorPrice>(x)));

						CreateVendorPricesOnWorksheetRelease(detail, pricesSet);
					}
				}

				priceWorksheet.Status = AR.SPWorksheetStatus.Released;
				Document.Update(priceWorksheet);
				Document.Current.Status = AR.SPWorksheetStatus.Released;

				Persist();

				ts.Complete();
			}
		}

		protected virtual IEnumerable<APVendorPrice> LoadInventoryPrices(APVendorPrice group, bool isPromotional)
		{
			return new SelectFrom<APVendorPrice>
				.Where<APVendorPrice.inventoryID.IsEqual<@P.AsInt>
					.And<APVendorPrice.isPromotionalPrice.IsEqual<@P.AsBool>>>
					.View
					.ReadOnly(this)
					.SelectMain(group.InventoryID, isPromotional);
		}

		
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2)]
		protected virtual void CreateVendorPricesOnWorksheetRelease(PXResult<APPriceWorksheetDetail, InventoryItem> row)
		{
			CreateVendorPricesOnWorksheetRelease(row, GetVendorPricesByPriceLineForWorksheetRelease(row));
		}

		protected virtual void CreateVendorPricesOnWorksheetRelease(APPriceWorksheetDetail priceLine, PXResultset<APVendorPrice> salesPrices)
		{
            InventoryItem item = InventoryItem.PK.Find(this, priceLine.InventoryID);
            DateTime? worksheetExpirationDate = Document.Current.ExpirationDate;
            DateTime? worksheetEffectiveDate = Document.Current.EffectiveDate;

            if (Document.Current.IsPromotional != true || worksheetExpirationDate == null)
            {
                ProcessReleaseForNonPromotionalVendorPrices(salesPrices, priceLine);
            }
            else
            {
                ProcessReleaseForPromotionalVendorPrices(salesPrices, priceLine);
            }

            if (APSetup.Current.RetentionType == AR.RetentionTypeList.FixedNumOfMonths && APSetup.Current.NumberOfMonths != 0)
            {
                foreach (APVendorPrice salesPrice in salesPrices)
                {
                    int numberOfMonths = APSetup.Current.NumberOfMonths ?? 0;

                    if (salesPrice.ExpirationDate != null && salesPrice.ExpirationDate.Value.AddMonths(numberOfMonths) < worksheetEffectiveDate)
                    {
                        APVendorPrices.Delete(salesPrice);
                    }
                }
            }

            if (!_loadVendorsPricesUsingAlternateID || priceLine.AlternateID.IsNullOrEmpty())
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

        protected virtual PXResultset<APVendorPrice> GetVendorPricesByPriceLineForWorksheetRelease(APPriceWorksheetDetail priceLine)
        {
            return PXSelect<APVendorPrice,
                     Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                       And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                      And2<
                          Where<APVendorPrice.siteID, Equal<Required<APVendorPrice.siteID>>,
                             Or<APVendorPrice.siteID, IsNull,
                             And<Required<APVendorPrice.siteID>, IsNull>>>,
                     And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
                     And<APVendorPrice.curyID, Equal<Required<APVendorPrice.curyID>>,
                     And<APVendorPrice.breakQty, Equal<Required<APVendorPrice.breakQty>>,
                     And<APVendorPrice.isPromotionalPrice, Equal<Required<APVendorPrice.isPromotionalPrice>>>>>>>>>,
                 OrderBy<
                         Asc<APVendorPrice.effectiveDate,
                         Asc<APVendorPrice.expirationDate>>>>
                   .Select(this,
                           priceLine.VendorID, priceLine.InventoryID,  
                           priceLine.SiteID, priceLine.SiteID,
                           priceLine.UOM, priceLine.CuryID, priceLine.BreakQty, 
                           Document.Current.IsPromotional ?? false);
        }

        protected virtual void ProcessReleaseForNonPromotionalVendorPrices(PXResultset<APVendorPrice> salesPrices, APPriceWorksheetDetail priceLine)
        {
            DateTime? worksheetExpirationDate = Document.Current.ExpirationDate;
            DateTime? worksheetEffectiveDate = Document.Current.EffectiveDate;
            bool insertNewPrice = true;
            const bool isPromotional = false;

            if (salesPrices.Count == 0)
            {
                APVendorPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, worksheetEffectiveDate, worksheetExpirationDate);
                APVendorPrices.Insert(newSalesPrice);
                return;
            }

            foreach (APVendorPrice salesPrice in salesPrices)
            {
                if (APSetup.Current.RetentionType == AR.RetentionTypeList.FixedNumOfMonths)
                {
                    if (Document.Current.OverwriteOverlapping != true &&
                       ((salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate >= worksheetEffectiveDate) ||
                         salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null))
                    {
                        insertNewPrice = false;
                    }

                    ProcessNonPromotionalPriceForFixedNumOfMonthsRetentionType(salesPrice, priceLine, worksheetExpirationDate,
                                                                                                worksheetEffectiveDate);
                }
                else
                {
                    if ((salesPrice.EffectiveDate >= worksheetEffectiveDate) ||
                        (worksheetEffectiveDate != null && salesPrice.ExpirationDate < worksheetEffectiveDate.Value.AddDays(-1)))
                    {
                        APVendorPrices.Delete(salesPrice);
                    }
                    else if (((salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null) ||
                              (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate == null) ||
                              ((salesPrice.EffectiveDate < worksheetEffectiveDate || salesPrice.EffectiveDate == null) &&
                                worksheetEffectiveDate <= salesPrice.ExpirationDate)) && worksheetEffectiveDate != null)
                    {
                        salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                        salesPrice.EffectiveDate = null;
                        APVendorPrices.Update(salesPrice);
                    }
                }
            }

            if (!insertNewPrice)
                return;

            if (Document.Current.OverwriteOverlapping == true || APSetup.Current.RetentionType == AR.RetentionTypeList.LastPrice)
            {
                APVendorPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, worksheetEffectiveDate, worksheetExpirationDate);
                APVendorPrices.Insert(newSalesPrice);
            }
            else
            {
                APVendorPrice minSalesPrice = GetMinVendorPriceForNonPromotionalPricesWorksheetRelease(priceLine, worksheetEffectiveDate);
                DateTime? newSalesPriceExpirationDate = minSalesPrice?.EffectiveDate?.AddDays(-1) ?? worksheetExpirationDate;
                APVendorPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, worksheetEffectiveDate, newSalesPriceExpirationDate);

                APVendorPrices.Insert(newSalesPrice);
            }
        }

        protected virtual void ProcessNonPromotionalPriceForFixedNumOfMonthsRetentionType(APVendorPrice salesPrice, APPriceWorksheetDetail priceLine,
                                                                                          DateTime? worksheetExpirationDate, DateTime? worksheetEffectiveDate)
        {
            if (Document.Current.OverwriteOverlapping == true)
            {
                if ((worksheetExpirationDate == null && salesPrice.EffectiveDate >= worksheetEffectiveDate) ||
                    (worksheetExpirationDate != null && salesPrice.EffectiveDate >= worksheetEffectiveDate && salesPrice.EffectiveDate <= worksheetExpirationDate))
                {
                    APVendorPrices.Delete(salesPrice);
                }
                else if (((salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null) ||
                          (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate == null) ||
                          (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate >= worksheetEffectiveDate) ||
                          (salesPrice.EffectiveDate < worksheetEffectiveDate && worksheetEffectiveDate <= salesPrice.ExpirationDate))
                    && Document.Current.IsPromotional != true && worksheetEffectiveDate != null)
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    APVendorPrices.Update(salesPrice);
                }
            }
            else
            {                
                if (salesPrice.EffectiveDate < worksheetEffectiveDate &&
                    salesPrice.ExpirationDate >= worksheetEffectiveDate && worksheetEffectiveDate != null)
                {
                    APVendorPrice newSalesPrice = (APVendorPrice)APVendorPrices.Cache.CreateCopy(salesPrice);
                    salesPrice.EffectiveDate = worksheetEffectiveDate;

                    UpdateVendorPriceFromPriceLine(salesPrice, priceLine);                    
                    APVendorPrices.Update(salesPrice);

                    newSalesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    newSalesPrice.RecordID = null;
                    APVendorPrices.Insert(newSalesPrice);
                }
                else if (salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate == null && worksheetEffectiveDate != null)
                {
                    const bool isPromotional = false;
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    APVendorPrices.Update(salesPrice);
                    APVendorPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, worksheetEffectiveDate, null);

                    APVendorPrices.Insert(newSalesPrice);
                }
                else if (salesPrice.EffectiveDate == worksheetEffectiveDate && salesPrice.ExpirationDate == worksheetExpirationDate)
                {
                    UpdateVendorPriceFromPriceLine(salesPrice, priceLine);
                    APVendorPrices.Update(salesPrice);
                }
                else if ((salesPrice.EffectiveDate == null && salesPrice.ExpirationDate == null) ||
                         (salesPrice.EffectiveDate == null && salesPrice.ExpirationDate >= worksheetEffectiveDate))
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    APVendorPrices.Update(salesPrice);
                }
            }        
        }

        protected virtual void UpdateVendorPriceFromPriceLine(APVendorPrice salesPrice, APPriceWorksheetDetail priceLine)
        {
            salesPrice.SalesPrice = priceLine.PendingPrice;
        }

        protected virtual APVendorPrice GetMinVendorPriceForNonPromotionalPricesWorksheetRelease(APPriceWorksheetDetail priceLine,
                                                                                                 DateTime? effectiveDate)
        {
            const object[] currents = null;
            return PXSelect<APVendorPrice,
                           Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                             And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                            And2<
                                Where<APVendorPrice.siteID, Equal<Required<APVendorPrice.siteID>>,
                                   Or<APVendorPrice.siteID, IsNull,
                                  And<Required<APVendorPrice.siteID>, IsNull>>>,
                            And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
                            And<APVendorPrice.curyID, Equal<Required<APVendorPrice.curyID>>,
                            And<APVendorPrice.breakQty, Equal<Required<APVendorPrice.breakQty>>,
                            And<APVendorPrice.effectiveDate, IsNotNull,
                            And<
                                Where<APVendorPrice.effectiveDate, GreaterEqual<Required<APVendorPrice.effectiveDate>>>>>>>>>>>,
                        OrderBy<
                                Asc<APVendorPrice.effectiveDate>>>
                        .SelectSingleBound(this, currents, priceLine.VendorID, priceLine.InventoryID,
                                           priceLine.SiteID, priceLine.SiteID, priceLine.UOM, priceLine.CuryID,
                                           priceLine.BreakQty, effectiveDate);
        }

        protected virtual void ProcessReleaseForPromotionalVendorPrices(PXResultset<APVendorPrice> salesPrices, APPriceWorksheetDetail priceLine)
        {
            DateTime? worksheetExpirationDate = Document.Current.ExpirationDate;
            DateTime? worksheetEffectiveDate = Document.Current.EffectiveDate;

            foreach (APVendorPrice salesPrice in salesPrices)
            {              
                if (salesPrice.EffectiveDate >= worksheetEffectiveDate && salesPrice.ExpirationDate <= worksheetExpirationDate)
                {
                    APVendorPrices.Delete(salesPrice);
                }
                else if (salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate <= worksheetExpirationDate &&
                         salesPrice.ExpirationDate >= worksheetEffectiveDate && worksheetEffectiveDate != null)
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    APVendorPrices.Update(salesPrice);
                }
                else if (salesPrice.EffectiveDate >= worksheetEffectiveDate && salesPrice.EffectiveDate < worksheetExpirationDate &&
                         salesPrice.ExpirationDate >= worksheetExpirationDate)
                {
                    salesPrice.EffectiveDate = worksheetExpirationDate.Value.AddDays(1);
                    APVendorPrices.Update(salesPrice);
                }
                else if (salesPrice.EffectiveDate <= worksheetEffectiveDate && salesPrice.ExpirationDate >= worksheetExpirationDate &&
                         salesPrice.ExpirationDate > worksheetEffectiveDate && worksheetEffectiveDate != null)
                {
                    salesPrice.ExpirationDate = worksheetEffectiveDate.Value.AddDays(-1);
                    APVendorPrices.Update(salesPrice);
                }
            }

            const bool isPromotional = true;
            APVendorPrice newSalesPrice = CreateSalesPrice(priceLine, isPromotional, worksheetEffectiveDate, worksheetExpirationDate);
            APVendorPrices.Insert(newSalesPrice);
        }

        protected virtual APVendorPrice CreateSalesPrice(APPriceWorksheetDetail priceLine, bool? isPromotional, DateTime? effectiveDate, DateTime? expirationDate)
        {
            APVendorPrice newSalesPrice = new APVendorPrice
            {
                VendorID = priceLine.VendorID,
                InventoryID = priceLine.InventoryID,
                SiteID = priceLine.SiteID,
                UOM = priceLine.UOM,
                BreakQty = priceLine.BreakQty,
                SalesPrice = priceLine.PendingPrice,
                CuryID = priceLine.CuryID,
                IsPromotionalPrice = isPromotional,
                EffectiveDate = effectiveDate,
                ExpirationDate = expirationDate,
            };

            return newSalesPrice;
        }

        #region SiteStatus Lookup
        [PXCopyPasteHiddenView]
        public PXFilter<AddItemFilter> addItemFilter;

        [PXCopyPasteHiddenView]
        public PXFilter<AddItemParameters> addItemParameters;

        [PXFilterable]
        [PXCopyPasteHiddenView]
        public ARAddItemLookup<ARAddItemSelected, AddItemFilter> addItemLookup;


        public PXAction<APPriceWorksheet> addItem;

        [PXUIField(DisplayName = AR.Messages.AddItem, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable AddItem(PXAdapter adapter)
        {
            if (addItemLookup.AskExt() == WebDialogResult.OK)
            {
                return AddSelItems(adapter);
            }
            return adapter.Get();
        }

        public PXAction<APPriceWorksheet> addSelItems;

        [PXUIField(DisplayName = AR.Messages.Add, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddSelItems(PXAdapter adapter)
        {
            if (addItemParameters.Current.VendorID == null)
            {
                PXSetPropertyException exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
                                                                              PXErrorLevel.Error, typeof(AddItemParameters.vendorID).Name);
                addItemParameters.Cache.RaiseExceptionHandling<AddItemParameters.vendorID>(addItemParameters.Current,
                                                                                           addItemParameters.Current.VendorID, exception);
                return adapter.Get();
            }

            foreach (ARAddItemSelected line in addItemLookup.Cache.Cached)
            {
                if (line.Selected != true)
                    continue;

				InsertOrUpdateARAddItem(line);
			}

            addItemFilter.Cache.Clear();
            addItemLookup.Cache.Clear();
            addItemParameters.Cache.Clear();

            return adapter.Get();
        }

		public PXAction<APPriceWorksheet> addAllItems;

		[PXUIField(DisplayName = AR.Messages.AddAll, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddAllItems(PXAdapter adapter)
		{
			if (addItemParameters.Current.VendorID == null)
			{
				PXSetPropertyException exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
																			  PXErrorLevel.Error, typeof(AddItemParameters.vendorID).Name);
				addItemParameters.Cache.RaiseExceptionHandling<AddItemParameters.vendorID>(addItemParameters.Current,
																						   addItemParameters.Current.VendorID, exception);
				return adapter.Get();
			}

			foreach (ARAddItemSelected line in addItemLookup.Select())
			{
				InsertOrUpdateARAddItem(line);
			}

			addItemFilter.Cache.Clear();
			addItemLookup.Cache.Clear();
			addItemParameters.Cache.Clear();

			return adapter.Get();
		}

		protected virtual void InsertOrUpdateARAddItem (ARAddItemSelected line)
		{
			int? priceCode = addItemParameters.Current.VendorID;

			var salesPrices =
				PXSelectGroupBy<APVendorPrice,
						  Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
							And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
							And<APVendorPrice.curyID, Equal<Required<APVendorPrice.curyID>>>>>,
						Aggregate<
								GroupBy<APVendorPrice.vendorID,
								GroupBy<APVendorPrice.inventoryID,
								GroupBy<APVendorPrice.uOM,
								GroupBy<APVendorPrice.breakQty,
								GroupBy<APVendorPrice.curyID>>>>>>>
			   .SelectMultiBound(this, null, priceCode, line.InventoryID, addItemParameters.Current.CuryID).ToList();

			if (salesPrices.Count > 0)
			{
				salesPrices.Select(price => CreateWorksheetDetailFromVendorPriceOnAddSelItems(price))
						   .ForEach(newWorksheetDetail => Details.Update(newWorksheetDetail));
			}
			else
			{
				APPriceWorksheetDetail newWorksheetDetail = CreateWorksheetDetailWhenPriceNotFoundOnAddSelItems(line);
				Details.Update(newWorksheetDetail);
			}
		}

		protected virtual APPriceWorksheetDetail CreateWorksheetDetailFromVendorPriceOnAddSelItems(APVendorPrice salesPrice)
        {
            return new APPriceWorksheetDetail
            {
                VendorID = addItemParameters.Current.VendorID,
                InventoryID = salesPrice.InventoryID,
                SiteID = addItemParameters.Current.SiteID ?? salesPrice.SiteID,
                UOM = salesPrice.UOM,
                BreakQty = salesPrice.BreakQty,
                CurrentPrice = salesPrice.SalesPrice,
                CuryID = addItemParameters.Current.CuryID
            };
        }

        protected virtual APPriceWorksheetDetail CreateWorksheetDetailWhenPriceNotFoundOnAddSelItems(ARAddItemSelected line)
        {
            return new APPriceWorksheetDetail
            {
                InventoryID = line.InventoryID,
                SiteID = addItemParameters.Current.SiteID,
                CuryID = addItemParameters.Current.CuryID,
                UOM = line.BaseUnit,
                VendorID = addItemParameters.Current.VendorID,
                CurrentPrice = 0m
            };
        }

        public override IEnumerable<PXDataRecord> ProviderSelect(BqlCommand command, int topCount, params PXDataValue[] pars)
        {
            return base.ProviderSelect(command, topCount, pars);
        }

		public override void Persist()
		{
			CheckForDuplicateDetails();

			base.Persist();
		}

		#region AddItemParameters event handlers
		protected virtual void AddItemParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AddItemParameters det = (AddItemParameters)e.Row;
            if (det == null) return;
			PXUIFieldAttribute.SetVisible<AddItemParameters.curyID>(sender, det, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
        }

        protected virtual void AddItemParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            AddItemParameters parameters = (AddItemParameters)e.Row;
            if (parameters == null) return;

            if (!sender.ObjectsEqual<AddItemParameters.vendorID>(e.Row, e.OldRow))
            {
                PXResult<Vendor> vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(this, parameters.VendorID);
                if (vendor != null)
                {
                    if (((Vendor)vendor).CuryID != null)
                        parameters.CuryID = ((Vendor)vendor).CuryID;
                    else
                        sender.SetDefaultExt<AddItemParameters.curyID>(e.Row);
                }
            }
        }
        #endregion

        #endregion

        public PXAction<APPriceWorksheet> copyPrices;
        [PXUIField(DisplayName = AR.Messages.CopyPrices, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CopyPrices(PXAdapter adapter)
        {
            if (CopyPricesSettings.AskExt() == WebDialogResult.OK)
            {
                if (CopyPricesSettings.Current == null)
                {
                    return adapter.Get();
                }
                else if (CopyPricesSettings.Current.SourceVendorID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.sourceVendorID>(adapter);                 
                }
                else if (CopyPricesSettings.Current.SourceCuryID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.sourceCuryID>(adapter);                  
                }
                else if (CopyPricesSettings.Current.EffectiveDate == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.effectiveDate>(adapter);
                }
                else if (CopyPricesSettings.Current.DestinationVendorID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.destinationVendorID>(adapter);                 
                }
                else if(CopyPricesSettings.Current.DestinationCuryID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.destinationCuryID>(adapter);
                }
                else if(CopyPricesSettings.Current.DestinationCuryID != CopyPricesSettings.Current.SourceCuryID && CopyPricesSettings.Current.RateTypeID == null)
                {
                    return SetErrorOnEmptyFieldAndReturn<CopyPricesFilter.rateTypeID>(adapter);
                }

                PXLongOperation.StartOperation(this, delegate { CopyPricesProc(Document.Current, CopyPricesSettings.Current); });
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

        public static void CopyPricesProc(APPriceWorksheet priceWorksheet, CopyPricesFilter copyFilter)
        {
            APPriceWorksheetMaint vendorPriceWorksheetMaint = PXGraph.CreateInstance<APPriceWorksheetMaint>();
            vendorPriceWorksheetMaint.Document.Update((APPriceWorksheet)vendorPriceWorksheetMaint.Document.Cache.CreateCopy(priceWorksheet));          
            vendorPriceWorksheetMaint.CopyPricesInternalProcessing(copyFilter);

            PXRedirectHelper.TryRedirect(vendorPriceWorksheetMaint, PXRedirectHelper.WindowMode.Same);
        }

        protected virtual void CopyPricesInternalProcessing(CopyPricesFilter copyFilter)
        {
            CopyPricesSettings.Current = copyFilter;

            var salesPrices =
                PXSelectJoinGroupBy<APVendorPrice,
                LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APVendorPrice.inventoryID>>>,
                Where<
                    InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
                    And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
                    And<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
                    And<APVendorPrice.curyID, Equal<Required<APVendorPrice.curyID>>,
                    And2<AreSame<APVendorPrice.siteID, Required<APVendorPrice.siteID>>,
                    And<APVendorPrice.isPromotionalPrice, Equal<Required<APVendorPrice.isPromotionalPrice>>,
                    And<Where2<
                        Where<
                            APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
                            And<APVendorPrice.expirationDate, IsNull>>,
                        Or<Where<
                            APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
                            And<APVendorPrice.expirationDate, Greater<Required<APVendorPrice.effectiveDate>>>>>>>>>>>>>,
                Aggregate<
                    GroupBy<APVendorPrice.vendorID,
                        GroupBy<APVendorPrice.inventoryID,
                        GroupBy<APVendorPrice.uOM,
                        GroupBy<APVendorPrice.breakQty,
                    GroupBy<APVendorPrice.curyID,
                    GroupBy<APVendorPrice.siteID>>>>>>>,
                OrderBy<
                        Asc<APVendorPrice.effectiveDate, 
                        Asc<APVendorPrice.expirationDate>>>>
                .Select(this,
                    copyFilter.SourceVendorID,
                    copyFilter.SourceCuryID,
                    copyFilter.SourceSiteID,
                    copyFilter.SourceSiteID,
                    copyFilter.IsPromotional ?? false,
                    copyFilter.EffectiveDate,
                    copyFilter.EffectiveDate,
                    copyFilter.EffectiveDate).AsEnumerable();

            salesPrices.Select(price => CreateWorksheetDetailFromVendorPriceOnCopying(price, copyFilter))
                       .ForEach(newLine => Details.Update(newLine));
          
            Save.Press();
            CopyPricesSettings.Cache.Clear();
        }

        protected virtual APPriceWorksheetDetail CreateWorksheetDetailFromVendorPriceOnCopying(APVendorPrice salesPrice, CopyPricesFilter copyFilter)
        {
            APPriceWorksheetDetail newline = new APPriceWorksheetDetail
            {
                VendorID = copyFilter.DestinationVendorID,
                InventoryID = salesPrice.InventoryID,
                SiteID = copyFilter.DestinationSiteID ?? salesPrice.SiteID,
                UOM = salesPrice.UOM,
                BreakQty = salesPrice.BreakQty,
                CuryID = copyFilter.DestinationCuryID
            };

            if (copyFilter.SourceCuryID == copyFilter.DestinationCuryID)
            {
                newline.CurrentPrice = salesPrice.SalesPrice ?? 0m;
            }
            else 
            {
                newline.CurrentPrice = ConvertSalesPrice(this, copyFilter.RateTypeID, copyFilter.SourceCuryID, copyFilter.DestinationCuryID,
                                                         copyFilter.CurrencyDate, salesPrice.SalesPrice ?? 0m);
            }

            return newline;
        }

        public static decimal ConvertSalesPrice(APPriceWorksheetMaint graph, string curyRateTypeID, string fromCuryID, string toCuryID, DateTime? curyEffectiveDate, decimal salesPrice)
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

        public PXAction<APPriceWorksheet> calculatePrices;
        [PXUIField(DisplayName = AR.Messages.CalcPendingPrices, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
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

            foreach (APPriceWorksheetDetail worksheetDetail in Details.Select())
            {
                                            
                var query = (PXResult<InventoryItem, INItemCost>)
                                PXSelectJoin<InventoryItem,
                                    LeftJoin<INItemCost,
                                          On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>>,
                                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                .SelectWindowed(this, 0, 1, worksheetDetail.InventoryID);

                InventoryItem inventoryItem = query;
                INItemCost itemCost = query;
                KeyValuePair<bool, decimal> calcRes = CalculateCorrectedAmountForPendingPricesCalculation(settings, inventoryItem,
                                                                                                          itemCost, worksheetDetail);
                bool skipUpdate = calcRes.Key;
               
                if (skipUpdate)
                    continue;

                decimal correctedAmt = calcRes.Value;

                if (settings.CorrectionPercent != null)
                {
                    correctedAmt = correctedAmt * (settings.CorrectionPercent.Value * 0.01m);
                }

                if (settings.Rounding != null)
                {
                    correctedAmt = Math.Round(correctedAmt, settings.Rounding.Value, MidpointRounding.AwayFromZero);
                }

                APPriceWorksheetDetail worksheetDetailCopyForUpdate = (APPriceWorksheetDetail)Details.Cache.CreateCopy(worksheetDetail);
                worksheetDetailCopyForUpdate.PendingPrice = correctedAmt;
                Details.Update(worksheetDetailCopyForUpdate);
            }         
        }

        protected virtual KeyValuePair<bool, decimal> CalculateCorrectedAmountForPendingPricesCalculation(CalculatePricesFilter settings, 
                                                                                                          InventoryItem inventoryItem,
                                                                                                          INItemCost itemCost,
                                                                                                          APPriceWorksheetDetail worksheetDetail)
        {
            bool skipUpdate = false;
            decimal correctedAmt = 0m;

            switch (settings.PriceBasis)
            {
                case AR.PriceBasisTypes.LastCost:
                    {
                        skipUpdate = settings.UpdateOnZero != true && (itemCost.LastCost == null || itemCost.LastCost == 0);

                        if (skipUpdate)
                            return new KeyValuePair<bool, decimal>(skipUpdate, correctedAmt);

                        decimal correctedAmtInBaseUnit = itemCost.LastCost ?? 0m;
                        correctedAmt = AdjustByUOM(inventoryItem, worksheetDetail, correctedAmtInBaseUnit);
                        break;
                    }
                case AR.PriceBasisTypes.StdCost:
                    {
                        decimal? costToUse = inventoryItem.ValMethod == INValMethod.Standard
                            ? inventoryItem.StdCost
                            : itemCost.AvgCost;

                        skipUpdate = settings.UpdateOnZero != true && (costToUse == null || costToUse == 0m);
                        decimal correctedAmtInBaseUnit = costToUse ?? 0m;
                        correctedAmt = AdjustByUOM(inventoryItem, worksheetDetail, correctedAmtInBaseUnit);
                        break;
                    }
                case AR.PriceBasisTypes.PendingPrice:
                    skipUpdate = settings.UpdateOnZero != true && (worksheetDetail.PendingPrice == null || worksheetDetail.PendingPrice == 0);
                    correctedAmt = worksheetDetail.PendingPrice ?? 0m;
                    break;
                case AR.PriceBasisTypes.CurrentPrice:
                    skipUpdate = settings.UpdateOnZero != true && (worksheetDetail.CurrentPrice == null || worksheetDetail.CurrentPrice == 0);
                    correctedAmt = worksheetDetail.CurrentPrice ?? 0m;
                    break;
                case AR.PriceBasisTypes.RecommendedPrice:
                    skipUpdate = settings.UpdateOnZero != true && (inventoryItem.RecPrice == null || inventoryItem.RecPrice == 0);
                    correctedAmt = inventoryItem.RecPrice ?? 0m;
                    break;
            }

            return new KeyValuePair<bool, decimal>(skipUpdate, correctedAmt);
        }

       protected virtual decimal AdjustByUOM(InventoryItem inventoryItem, APPriceWorksheetDetail worksheetDetail, decimal correctedAmtInBaseUnit)
        {
            if (inventoryItem.BaseUnit != worksheetDetail.UOM)
            {
                decimal? result;

                if (TryConvertToBase(Caches[typeof(InventoryItem)], inventoryItem.InventoryID, worksheetDetail.UOM,
                                     correctedAmtInBaseUnit, out result))
                {
                    return result.Value;
                }
                else
                {
                    return 0m;
                }
            }
            else
            {
                return correctedAmtInBaseUnit;
            }
        }

        protected virtual void CheckForDuplicateDetails()
		{
			IEnumerable<APPriceWorksheetDetail> worksheetDetails = PXSelect<
				APPriceWorksheetDetail,
				Where<
					APPriceWorksheetDetail.refNbr, Equal<Current<APPriceWorksheetDetail.refNbr>>>>
				.Select(this)
				.RowCast<APPriceWorksheetDetail>()
				.ToArray();

			IEqualityComparer<APPriceWorksheetDetail> duplicateComparer =
				new FieldSubsetEqualityComparer<APPriceWorksheetDetail>(
					Details.Cache,
					typeof(APPriceWorksheetDetail.refNbr),
					typeof(APPriceWorksheetDetail.vendorID),
					typeof(APPriceWorksheetDetail.inventoryID),
					typeof(APPriceWorksheetDetail.siteID),
					typeof(APPriceWorksheetDetail.subItemID),
					typeof(APPriceWorksheetDetail.uOM),
					typeof(APPriceWorksheetDetail.curyID),
					typeof(APPriceWorksheetDetail.breakQty));

			IEnumerable<APPriceWorksheetDetail> duplicates = worksheetDetails
				.GroupBy(detail => detail, duplicateComparer)
				.Where(duplicatesGroup => duplicatesGroup.HasAtLeastTwoItems())
				.Flatten();

			foreach (APPriceWorksheetDetail duplicate in duplicates)
			{
				Details.Cache.RaiseExceptionHandling<APPriceWorksheetDetail.vendorID>(
					duplicate,
					duplicate.VendorID,
					new PXSetPropertyException(
						Messages.DuplicateVendorPrice,
						PXErrorLevel.RowError,
						typeof(APPriceWorksheetDetail.vendorID).Name));
			}
		}

		private decimal GetItemPrice(int? inventoryID, string toCuryID, DateTime? curyEffectiveDate)
        {
			InventoryItem inventoryItem = InventoryItem.PK.Find(this, inventoryID);

            decimal? itemPrice = GetItemPriceFromInventoryItem(inventoryItem);
            return itemPrice == null
                ? 0m
                : ConvertSalesPrice(this, new CMSetupSelect(this).Current.APRateTypeDflt, new PXSetup<GL.Company>(this).Current.BaseCuryID,
                                    toCuryID, curyEffectiveDate, itemPrice.Value);
        }

        /// <summary>
        /// Gets item price from inventory item. The extension point used by Lexware PriceUnit customization to adjust inventory item's price.
        /// </summary>
        /// <param name="inventoryItem">The inventory item.</param>
        protected virtual decimal? GetItemPriceFromInventoryItem(InventoryItem inventoryItem) => inventoryItem?.BasePrice;

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

        #region APPriceWorksheet event handlers

        protected virtual void APPriceWorksheet_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            APPriceWorksheet tran = (APPriceWorksheet)e.Row;
            if (tran == null) return;

            bool allowEdit = (tran.Status == AR.SPWorksheetStatus.Hold || tran.Status == AR.SPWorksheetStatus.Open);
            ReleasePriceWorksheet.SetEnabled(tran.Hold == false && tran.Status == AR.SPWorksheetStatus.Open);
            addItem.SetEnabled(tran.Hold == true && allowEdit);
            copyPrices.SetEnabled(tran.Hold == true && allowEdit);
            calculatePrices.SetEnabled(tran.Hold == true && allowEdit);

            Details.Cache.AllowInsert = allowEdit;
            Details.Cache.AllowDelete = allowEdit;
            Details.Cache.AllowUpdate = allowEdit;

            Document.Cache.AllowDelete = tran.Status != AR.SPWorksheetStatus.Released;

            PXUIFieldAttribute.SetEnabled<APPriceWorksheet.hold>(sender, tran, tran.Status != AR.SPWorksheetStatus.Released);
            PXUIFieldAttribute.SetEnabled<APPriceWorksheet.descr>(sender, tran, allowEdit);
            PXUIFieldAttribute.SetEnabled<APPriceWorksheet.effectiveDate>(sender, tran, allowEdit);
            PXUIFieldAttribute.SetEnabled<APPriceWorksheet.expirationDate>(sender, tran, allowEdit && (APSetup.Current.RetentionType != AR.RetentionTypeList.LastPrice || (APSetup.Current.RetentionType == AR.RetentionTypeList.LastPrice && tran.IsPromotional == true)));
            PXUIFieldAttribute.SetEnabled<APPriceWorksheet.isPromotional>(sender, tran, allowEdit);
            PXUIFieldAttribute.SetEnabled<APPriceWorksheet.overwriteOverlapping>(sender, tran, allowEdit && tran.IsPromotional != true && APSetup.Current.RetentionType != AR.RetentionTypeList.LastPrice);

            if (APSetup.Current.RetentionType == AR.RetentionTypeList.LastPrice || tran.IsPromotional == true) tran.OverwriteOverlapping = true;
            if (APSetup.Current.RetentionType == AR.RetentionTypeList.LastPrice && tran.IsPromotional != true) tran.ExpirationDate = null;
        }

        protected virtual void APPriceWorksheet_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            APPriceWorksheet doc = (APPriceWorksheet)e.Row;
            if (doc == null) return;

            doc.Status = doc.Hold == false ? AR.SPWorksheetStatus.Open : AR.SPWorksheetStatus.Hold;
        }

        protected virtual void APPriceWorksheet_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            APPriceWorksheet doc = (APPriceWorksheet)e.Row;
            if (doc == null) return;

            if (doc.IsPromotional == true && doc.ExpirationDate == null)
                sender.RaiseExceptionHandling<APPriceWorksheet.expirationDate>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPriceWorksheet.expirationDate).Name));
        }

        protected virtual void APPriceWorksheet_EffectiveDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            APPriceWorksheet doc = (APPriceWorksheet)e.Row;
            if (doc == null) return;

            if (e.NewValue == null)
                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPriceWorksheet.effectiveDate).Name);

            if (doc.IsPromotional == true && doc.ExpirationDate != null && doc.ExpirationDate < (DateTime)e.NewValue)
            {
                throw new PXSetPropertyException(PXMessages.LocalizeFormat(AR.Messages.ExpirationLessThanEffective));
            }
        }

        protected virtual void APPriceWorksheet_ExpirationDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            APPriceWorksheet doc = (APPriceWorksheet)e.Row;
            if (doc == null) return;

            if (doc.IsPromotional == true && e.NewValue == null)
                throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPriceWorksheet.expirationDate).Name);

            if (doc.IsPromotional == true && doc.EffectiveDate != null && doc.EffectiveDate > (DateTime)e.NewValue)
            {
                throw new PXSetPropertyException(PXMessages.LocalizeFormat(AR.Messages.ExpirationLessThanEffective));
            }
        }

		protected virtual void _(Events.FieldDefaulting<APPriceWorksheetDetail, APPriceWorksheetDetail.subItemID> args)
		{
			var inventory = InventoryItem.PK.Find(this, args.Row?.InventoryID);
			if (inventory?.DefaultSubItemOnEntry == true)
				args.NewValue = inventory.DefaultSubItemID;
		}

		#endregion

		#region APPriceWorksheetDetail event handlers

		protected virtual void APPriceWorksheetDetail_BreakQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            //add verification
            APPriceWorksheetDetail det = (APPriceWorksheetDetail)e.Row;
            if (det == null) return;
        }

        protected virtual void APPriceWorksheetDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            APPriceWorksheetDetail det = (APPriceWorksheetDetail)e.Row;

            if (det == null)
                return;

            if (e.ExternalCall && det.VendorID != null && det.InventoryID != null && det.CuryID != null && Document.Current != null &&
                Document.Current.EffectiveDate != null && det.CurrentPrice == 0m)
            {
                det.CurrentPrice = GetItemPrice(det.InventoryID, det.CuryID, Document.Current.EffectiveDate);
            }
			if (IsImportFromExcel && DuplicateFinder != null)
				DuplicateFinder.AddItem(det);

        }

        protected virtual void APPriceWorksheetDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            APPriceWorksheetDetail det = (APPriceWorksheetDetail)e.Row;
            if (det == null) return;

            if (e.ExternalCall && (!sender.ObjectsEqual<APPriceWorksheetDetail.uOM>(e.Row, e.OldRow) || !sender.ObjectsEqual<APPriceWorksheetDetail.inventoryID>(e.Row, e.OldRow)
                || !sender.ObjectsEqual<APPriceWorksheetDetail.curyID>(e.Row, e.OldRow)))
            {
                det.CurrentPrice = 0m;
            }
        }

        protected virtual void APPriceWorksheetDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            APPriceWorksheetDetail det = (APPriceWorksheetDetail)e.Row;
            if (det == null) return;

			if (Document.Current?.Hold != true && det.PendingPrice == null)
			{
				sender.RaiseExceptionHandling<APPriceWorksheetDetail.pendingPrice>(
					det, 
					det.PendingPrice,
					new PXSetPropertyException(
						ErrorMessages.FieldIsEmpty, 
						PXErrorLevel.Error, 
						typeof(APPriceWorksheetDetail.pendingPrice).Name));

				return;
			}

			if (det.VendorID == null)
            {
                sender.RaiseExceptionHandling<APPriceWorksheetDetail.vendorID>(det, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPriceWorksheetDetail.vendorID).Name));
                return;
            }

            if (Document.Current != null && Document.Current.Hold != true && det.PendingPrice == null)
                sender.RaiseExceptionHandling<APPriceWorksheetDetail.pendingPrice>(det, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APPriceWorksheetDetail.pendingPrice).Name));
        }
        #endregion

        #region CopyPricesFilter event handlers

        protected virtual void CopyPricesFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CopyPricesFilter filter = (CopyPricesFilter)e.Row;
            if (filter == null) return;

            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.rateTypeID>(sender, filter, filter.SourceCuryID != filter.DestinationCuryID);
            PXUIFieldAttribute.SetEnabled<CopyPricesFilter.currencyDate>(sender, filter, filter.SourceCuryID != filter.DestinationCuryID);

	        bool MCFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
            PXUIFieldAttribute.SetVisible<CopyPricesFilter.sourceCuryID>(sender, filter, MCFeatureInstalled);
            PXUIFieldAttribute.SetVisible<CopyPricesFilter.destinationCuryID>(sender, filter, MCFeatureInstalled);
            PXUIFieldAttribute.SetVisible<CopyPricesFilter.currencyDate>(sender, filter, MCFeatureInstalled);
            PXUIFieldAttribute.SetVisible<CopyPricesFilter.rateTypeID>(sender, filter, MCFeatureInstalled);
        }

        protected virtual void CopyPricesFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            CopyPricesFilter parameters = (CopyPricesFilter)e.Row;
            if (parameters == null) return;

            if (!sender.ObjectsEqual<CopyPricesFilter.sourceVendorID>(e.Row, e.OldRow))
            {
                PXResult<Vendor> vendor = PXSelect<Vendor, Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>>>.Select(this, parameters.SourceVendorID);
                if (vendor != null)
                {
                    parameters.SourceCuryID = ((Vendor)vendor).CuryID;
                }
            }
            if (!sender.ObjectsEqual<CopyPricesFilter.destinationVendorID>(e.Row, e.OldRow))
            {
                PXResult<Vendor> vendor = PXSelect<Vendor, Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>>>.Select(this, parameters.DestinationVendorID);
                if (vendor != null)
                {
                    parameters.DestinationCuryID = ((Vendor)vendor).CuryID;
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

        #region #Find of duplicates

        protected virtual Type[] GetAlternativeKeyFields()
		{
			return new[]
			{
				typeof(APPriceWorksheetDetail.vendorID),
				typeof(APPriceWorksheetDetail.inventoryID),
				typeof(APPriceWorksheetDetail.uOM),
				typeof(APPriceWorksheetDetail.siteID),
				typeof(APPriceWorksheetDetail.curyID),
				typeof(APPriceWorksheetDetail.taxID)
			};
		}

		public DuplicatesSearchEngine<APPriceWorksheetDetail> DuplicateFinder { get; set; }

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
			if (viewName.Equals(Details.View.Name, StringComparison.InvariantCultureIgnoreCase) && !DontUpdateExistRecords)
			{
				if (DuplicateFinder == null)
				{
					var details = PXSelect<APPriceWorksheetDetail,
						Where<APPriceWorksheetDetail.refNbr, Equal<Current<APPriceWorksheetDetail.refNbr>>>>.Select(this).RowCast<APPriceWorksheetDetail>().ToList();
					DuplicateFinder = new DuplicatesSearchEngine<APPriceWorksheetDetail>(Details.Cache, GetAlternativeKeyFields(), details);
				}
				var duplicate = DuplicateFinder.Find(values);
				if (duplicate != null)
				{
					if (keys.Contains(nameof(APPriceWorksheetDetail.LineID)))
						keys[nameof(APPriceWorksheetDetail.LineID)] = duplicate.LineID;
					else
						keys.Add(nameof(APPriceWorksheetDetail.LineID), duplicate.LineID);
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
        [PXUIField(DisplayName = "Product Manager")]
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
        [PXUIField(DisplayName = "Product  Workgroup")]
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
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [Vendor]
        [PXDefault]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
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
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[NullableSite]
		public virtual Int32? SiteID { get; set; }
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
        //[PXCalcCurrency(typeof(SOSiteStatusSelected.curyInfoID), typeof(SOSiteStatusSelected.baseUnitPrice))]
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

		#region ProductWorkgroupID
		public abstract class productWorkgroupID : PX.Data.BQL.BqlInt.Field<productWorkgroupID> { }
		/// <summary>
		/// The workgroup that is responsible for the item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPCompanyTree.WorkGroupID"/> field.
		/// </value>
		[PXDBInt(BqlField = typeof(InventoryItem.productWorkgroupID))]
		[EP.PXWorkgroupSelector]
		[PXUIField(DisplayName = "Product Workgroup")]
		public virtual Int32? ProductWorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region ProductManagerID
		public abstract class productManagerID : PX.Data.BQL.BqlGuid.Field<productManagerID> { }
		/// <summary>
		/// The <see cref="EPEmployee">product manager</see> responsible for this item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEmployee.PKID"/> field.
		/// </value>
		[PXDBGuid(BqlField = typeof(InventoryItem.productManagerID))]
		[TM.PXOwnerSelector(typeof(productWorkgroupID))]
		[PXUIField(DisplayName = "Product Manager")]
		public virtual Guid? ProductManagerID
		{
			get;
			set;
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
						Or<Current<AddItemFilter.ownerID>, Equal<ARAddItemSelected.productManagerID>>>>();

			view.WhereAnd<Where<Current<AddItemFilter.myWorkGroup>, Equal<boolFalse>,
						 Or<ARAddItemSelected.productWorkgroupID, TM.InMember<CurrentValue<AddItemFilter.currentOwnerID>>>>>();

			view.WhereAnd<Where<Current<AddItemFilter.workGroupID>, IsNull,
						Or<Current<AddItemFilter.workGroupID>, Equal<ARAddItemSelected.productWorkgroupID>>>>();

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
        #region SourceVendorID
        public abstract class sourceVendorID : PX.Data.BQL.BqlInt.Field<sourceVendorID> { }
        protected Int32? _SourceVendorID;
        [Vendor(DisplayName="Source Vendor")]
        [PXDefault]
        public virtual Int32? SourceVendorID
        {
            get
            {
                return this._SourceVendorID;
            }
            set
            {
                this._SourceVendorID = value;
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

        #region DestinationVendorID
        public abstract class destinationVendorID : PX.Data.BQL.BqlInt.Field<destinationVendorID> { }
        protected Int32? _DestinationVendorID;
        [Vendor(DisplayName = "Destination Vendor")]
        [PXDefault]
        public virtual Int32? DestinationVendorID
        {
            get
            {
                return this._DestinationVendorID;
            }
            set
            {
                this._DestinationVendorID = value;
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
        [PXDefault()]
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
