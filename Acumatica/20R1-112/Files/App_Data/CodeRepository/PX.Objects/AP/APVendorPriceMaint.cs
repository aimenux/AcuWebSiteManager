using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.SM;
using PX.TM;
using PX.Objects.PO;
using PX.Common;
using PX.Data.BQL;

namespace PX.Objects.AP
{
	public class APVendorPriceMaint : PXGraph<APVendorPriceMaint>, IPXAuditSource //, PXImportAttribute.IPXPrepareItems
	{

		private static readonly Lazy<APVendorPriceMaint> _apVendorPriceMaint = new Lazy<APVendorPriceMaint>(CreateInstance<APVendorPriceMaint>);
		public static APVendorPriceMaint SingleAPVendorPriceMaint => _apVendorPriceMaint.Value;

		#region DAC overrides
		#region APVendorPrice
		#region VendorID
		[Vendor]
		[PXDefault(typeof(APVendorPriceFilter.vendorID))]
		public virtual void APVendorPrice_VendorID_CacheAttached(PXCache sender) { }
		#endregion
		#region InventoryID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(typeof(APVendorPriceFilter.inventoryID))]
		public virtual void APVendorPrice_InventoryID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault]
		[PXFormula(typeof(IsNull<
			Selector<APVendorPrice.vendorID, Vendor.curyID>,
			Current<Company.baseCuryID>>))]
		public virtual void APVendorPrice_CuryID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Default<APVendorPrice.vendorID>))]
		public virtual void APVendorPrice_SalesPrice_CacheAttached(PXCache sender) { }
		#endregion
		#endregion
		#endregion
		#region Selects/Views

		public PXSave<APVendorPriceFilter> Save;
		public PXCancel<APVendorPriceFilter> Cancel;

		public PXFilter<APVendorPriceFilter> Filter;

		[PXFilterable]
		public PXSelectJoin<APVendorPrice,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APVendorPrice.inventoryID>>,
				LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>,
				LeftJoin<Vendor, On<APVendorPrice.vendorID, Equal<Vendor.bAccountID>>>>>,
				Where<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
				And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
				And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
				And2<Where<APVendorPrice.vendorID, Equal<Current<APVendorPriceFilter.vendorID>>, Or<Current<APVendorPriceFilter.vendorID>, IsNull>>, 
				And2<Where<APVendorPrice.inventoryID, Equal<Current<APVendorPriceFilter.inventoryID>>, Or<Current<APVendorPriceFilter.inventoryID>, IsNull>>,
				And2<Where<APVendorPrice.siteID, Equal<Current<APVendorPriceFilter.siteID>>, Or<Current<APVendorPriceFilter.siteID>, IsNull>>,
				And2<Where2<Where2<Where<APVendorPrice.effectiveDate, LessEqual<Current<APVendorPriceFilter.effectiveAsOfDate>>, Or<APVendorPrice.effectiveDate, IsNull>>, 
				And<Where<APVendorPrice.expirationDate, GreaterEqual<Current<APVendorPriceFilter.effectiveAsOfDate>>, Or<APVendorPrice.expirationDate, IsNull>>>>,
				Or<Current<APVendorPriceFilter.effectiveAsOfDate>, IsNull>>,
					And<Where2<Where<Current<APVendorPriceFilter.itemClassCD>, IsNull,
							Or<INItemClass.itemClassCD, Like<Current<APVendorPriceFilter.itemClassCDWildcard>>>>,
					And2<Where<Current<APVendorPriceFilter.ownerID>, IsNull,
						Or<Current<APVendorPriceFilter.ownerID>, Equal<InventoryItem.productManagerID>>>,
					And2<Where<Current<APVendorPriceFilter.myWorkGroup>, Equal<boolFalse>,
						 Or<InventoryItem.productWorkgroupID, InMember<CurrentValue<APVendorPriceFilter.currentOwnerID>>>>,
					And<Where<Current<APVendorPriceFilter.workGroupID>, IsNull,
						Or<Current<APVendorPriceFilter.workGroupID>, Equal<InventoryItem.productWorkgroupID>>>>>>>>>>>>>>>,
				OrderBy<Asc<APVendorPrice.inventoryID,
					Asc<APVendorPrice.uOM, Asc<APVendorPrice.breakQty, Asc<APVendorPrice.effectiveDate>>>>>> Records;

		public PXSetup<Company> Company;
		public PXSetup<APSetup> APSetup;
		#endregion
		
		#region Ctor
		public APVendorPriceMaint() 
		{
			FieldDefaulting.AddHandler<CR.BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = CR.BAccountType.VendorType; });
			bool loadVendorsPriceByAlternateID = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && APSetup.Current.LoadVendorsPricesUsingAlternateID == true;
			CrossItemAttribute.SetEnableAlternateSubstitution<APVendorPrice.inventoryID>(Records.Cache, null, loadVendorsPriceByAlternateID);
		}
		#endregion

		#region IPXAuditSource

		string IPXAuditSource.GetMainView()
		{
			return nameof(Records);
		}

		IEnumerable<Type> IPXAuditSource.GetAuditedTables()
		{
			yield return typeof(APVendorPrice);
		}

		#endregion

		public PXAction<APVendorPriceFilter> createWorksheet;

        [PXUIField(DisplayName = AR.Messages.CreatePriceWorksheet, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable CreateWorksheet(PXAdapter adapter)
        {
            if (Filter.Current == null)
            {
                return adapter.Get();
            }

            this.Save.Press();
            APPriceWorksheetMaint graph = PXGraph.CreateInstance<APPriceWorksheetMaint>();
            APPriceWorksheet worksheet = new APPriceWorksheet();
            graph.Document.Insert(worksheet);
            int startRow = PXView.StartRow;
            int totalRows = 0;

            object[] parameters =
            {
                    Filter.Current.VendorID,
                    Filter.Current.VendorID,
                    Filter.Current.InventoryID,
                    Filter.Current.InventoryID,
                    Filter.Current.SiteID,
                    Filter.Current.SiteID,
                    Filter.Current.EffectiveAsOfDate,
                    Filter.Current.EffectiveAsOfDate,
                    Filter.Current.EffectiveAsOfDate,
                    Filter.Current.ItemClassCD,
                    Filter.Current.ItemClassCDWildcard,
                    Filter.Current.OwnerID,
                    Filter.Current.OwnerID,
                    Filter.Current.MyWorkGroup,
                    Filter.Current.WorkGroupID,
                    Filter.Current.WorkGroupID
                };

            Func<BqlCommand, List<Object>> performSelect = command =>
                new PXView(this, false, command).Select(
                    PXView.Currents,
                    parameters,
                    PXView.Searches,
                    PXView.SortColumns,
                    PXView.Descendings,
                    Records.View.GetExternalFilters(),
                    ref startRow,
                    PXView.MaximumRows,
                    ref totalRows);

            List<Object> allVendorPrices = performSelect(
                    PXSelectJoin<APVendorPrice,
                    LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APVendorPrice.inventoryID>>,
                    LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>,
                    LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>>>,
                    Where<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
                        And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
                        And2<Where<APVendorPrice.vendorID, Equal<Required<APVendorPriceFilter.vendorID>>, Or<Required<APVendorPriceFilter.vendorID>, IsNull>>,
                        And2<Where<APVendorPrice.inventoryID, Equal<Required<APVendorPriceFilter.inventoryID>>, Or<Required<APVendorPriceFilter.inventoryID>, IsNull>>,
                        And2<Where<APVendorPrice.siteID, Equal<Required<APVendorPriceFilter.siteID>>, Or<Required<APVendorPriceFilter.siteID>, IsNull>>,
                        And2<Where2<Where2<Where<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPriceFilter.effectiveAsOfDate>>, Or<APVendorPrice.effectiveDate, IsNull>>,
                        And<Where<APVendorPrice.expirationDate, GreaterEqual<Required<APVendorPriceFilter.effectiveAsOfDate>>, Or<APVendorPrice.expirationDate, IsNull>>>>,
                        Or<Required<APVendorPriceFilter.effectiveAsOfDate>, IsNull>>,
                        And<Where2<Where<Required<APVendorPriceFilter.itemClassCD>, IsNull,
                                Or<INItemClass.itemClassCD, Like<Required<APVendorPriceFilter.itemClassCDWildcard>>>>,
                        And2<Where<Required<APVendorPriceFilter.ownerID>, IsNull,
                            Or<Required<APVendorPriceFilter.ownerID>, Equal<InventoryItem.productManagerID>>>,
                        And2<Where<Required<APVendorPriceFilter.myWorkGroup>, Equal<False>,
                            Or<InventoryItem.productWorkgroupID, InMember<CurrentValue<APVendorPriceFilter.currentOwnerID>>>>,
                        And<Where<Required<APVendorPriceFilter.workGroupID>, IsNull,
                            Or<Required<APVendorPriceFilter.workGroupID>, Equal<InventoryItem.productWorkgroupID>>>>>>>>>>>>>>,
                    OrderBy<
                        Asc<APVendorPrice.inventoryID,
                        Asc<APVendorPrice.uOM,
                        Asc<APVendorPrice.breakQty,
                        Desc<APVendorPrice.effectiveDate>>>>>>.GetCommand());

            List<Object> groupedVendorPrices = performSelect(
                    PXSelectJoinGroupBy<APVendorPrice,
                    LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APVendorPrice.inventoryID>>,
                    LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>>,
                    LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>>>,
                    Where<InventoryItem.itemStatus, NotEqual<INItemStatus.inactive>,
                        And<InventoryItem.itemStatus, NotEqual<INItemStatus.toDelete>,
                        And2<Where<APVendorPrice.vendorID, Equal<Required<APVendorPriceFilter.vendorID>>, Or<Required<APVendorPriceFilter.vendorID>, IsNull>>,
                        And2<Where<APVendorPrice.inventoryID, Equal<Required<APVendorPriceFilter.inventoryID>>, Or<Required<APVendorPriceFilter.inventoryID>, IsNull>>,
                        And2<Where<APVendorPrice.siteID, Equal<Required<APVendorPriceFilter.siteID>>, Or<Required<APVendorPriceFilter.siteID>, IsNull>>,
                        And2<Where2<Where2<Where<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPriceFilter.effectiveAsOfDate>>, Or<APVendorPrice.effectiveDate, IsNull>>,
                        And<Where<APVendorPrice.expirationDate, GreaterEqual<Required<APVendorPriceFilter.effectiveAsOfDate>>, Or<APVendorPrice.expirationDate, IsNull>>>>,
                        Or<Required<APVendorPriceFilter.effectiveAsOfDate>, IsNull>>,
                        And<Where2<Where<Required<APVendorPriceFilter.itemClassCD>, IsNull,
                            Or<INItemClass.itemClassCD, Like<Required<APVendorPriceFilter.itemClassCDWildcard>>>>,
                        And2<Where<Required<APVendorPriceFilter.ownerID>, IsNull,
                            Or<Required<APVendorPriceFilter.ownerID>, Equal<InventoryItem.productManagerID>>>,
                        And2<Where<Required<APVendorPriceFilter.myWorkGroup>, Equal<False>,
                            Or<InventoryItem.productWorkgroupID, InMember<CurrentValue<APVendorPriceFilter.currentOwnerID>>>>,
                        And<Where<Required<APVendorPriceFilter.workGroupID>, IsNull,
                            Or<Required<APVendorPriceFilter.workGroupID>, Equal<InventoryItem.productWorkgroupID>>>>>>>>>>>>>>,
                    Aggregate<
                        GroupBy<APVendorPrice.vendorID,
                        GroupBy<APVendorPrice.inventoryID,
                        GroupBy<APVendorPrice.uOM,
                        GroupBy<APVendorPrice.breakQty,
                        GroupBy<APVendorPrice.curyID,
                        GroupBy<APVendorPrice.siteID>>>>>>>,
                    OrderBy<
                        Asc<APVendorPrice.inventoryID,
                        Asc<APVendorPrice.uOM,
                        Asc<APVendorPrice.breakQty,
                        Desc<APVendorPrice.effectiveDate>>>>>>.GetCommand());

            if (allVendorPrices.Count > groupedVendorPrices.Count)
            {
                throw new PXException(Messages.MultiplePriceRecords);
            }

            CreateWorksheetDetailsFromVendorPrices(graph, groupedVendorPrices);
            throw new PXRedirectRequiredException(graph, AR.Messages.CreatePriceWorksheet);
        }

        /// <summary>
        /// Creates worksheet details from vendor prices. Extended in Lexware Price Unit customization.
        /// </summary>
        /// <param name="graph">The APPriceWorksheetMaint graph.</param>
        /// <param name="groupedVendorPrices">The grouped vendor prices.</param>
        protected virtual void CreateWorksheetDetailsFromVendorPrices(APPriceWorksheetMaint graph, List<object> groupedVendorPrices)
        {
            foreach (PXResult<APVendorPrice> res in groupedVendorPrices)
            {
                APVendorPrice price = res;
                var detail = new APPriceWorksheetDetail
                {
                    RefNbr = graph.Document.Current.RefNbr,
                    VendorID = price.VendorID
                };

                detail = graph.Details.Insert(detail);
                FillWorksheetDetailFromVendorPriceOnWorksheetCreation(detail, price);
                graph.Details.Update(detail);
            }
        }

        /// <summary>
        /// Fill worksheet detail from vendor price on worksheet creation. Extended in Lexware Price Unit customization.
        /// </summary>
        /// <param name="detail">The detail.</param>
        /// <param name="price">The price.</param>
        protected virtual void FillWorksheetDetailFromVendorPriceOnWorksheetCreation(APPriceWorksheetDetail detail, APVendorPrice price)
        {
            detail.InventoryID = price.InventoryID;
            detail.UOM = price.UOM;
            detail.BreakQty = price.BreakQty;
            detail.CurrentPrice = price.SalesPrice;
            detail.CuryID = price.CuryID;
            detail.SiteID = price.SiteID;
        }

		public virtual void APVendorPriceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(TM.OwnedFilter.ownerID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(TM.OwnedFilter.myOwner).Name) == false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(TM.OwnedFilter.workGroupID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(TM.OwnedFilter.myWorkGroup).Name) == false);
		}

		protected virtual void APVendorPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APVendorPrice row = (APVendorPrice)e.Row;
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
			{
				sender.RaiseExceptionHandling<APVendorPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APVendorPrice.expirationDate).Name));
			}
			if (row.IsPromotionalPrice == true && row.EffectiveDate == null)
			{
				sender.RaiseExceptionHandling<APVendorPrice.effectiveDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(APVendorPrice.effectiveDate).Name));
			}
			if (row.ExpirationDate < row.EffectiveDate)
			{
				sender.RaiseExceptionHandling<APVendorPrice.effectiveDate>(row, row.ExpirationDate, new PXSetPropertyException(AR.Messages.EffectiveDateExpirationDate, PXErrorLevel.RowError));
			}
		}

		protected virtual void APVendorPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			APVendorPrice row = e.Row as APVendorPrice;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<APVendorPrice.vendorID>(sender, row, Filter.Current.VendorID == null);
				PXUIFieldAttribute.SetEnabled<APVendorPrice.inventoryID>(sender, row, Filter.Current.InventoryID == null);
			}
		}

		public override void Persist()
		{
			foreach (APVendorPrice price in Records.Cache.Inserted)
			{
				APVendorPrice lastPrice = FindLastPrice(this, price);
				if (lastPrice != null)
				{
					if (lastPrice.EffectiveDate > price.EffectiveDate && price.ExpirationDate == null)
					{
						Records.Cache.RaiseExceptionHandling<APVendorPrice.expirationDate>(price, price.ExpirationDate, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APVendorPrice.expirationDate>(Records.Cache)));
						throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APVendorPrice.expirationDate>(Records.Cache));
					}
				}
				ValidateDuplicate(this, Records.Cache, price);
			}
			foreach (APVendorPrice price in Records.Cache.Updated)
			{
				APVendorPrice lastPrice = FindLastPrice(this, price);
				if (lastPrice != null)
				{
					if (lastPrice.EffectiveDate > price.EffectiveDate && price.ExpirationDate == null)
					{
						Records.Cache.RaiseExceptionHandling<APVendorPrice.expirationDate>(price, price.ExpirationDate, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APVendorPrice.expirationDate>(Records.Cache)));
						throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APVendorPrice.expirationDate>(Records.Cache));
					}
				}
				ValidateDuplicate(this, Records.Cache, price);
			}
			base.Persist();
			Records.Cache.Clear();
		}

		public static void ValidateDuplicate(PXGraph graph, PXCache sender, APVendorPrice price)
		{
			PXSelectBase<APVendorPrice> selectDuplicate = 
				new PXSelect<APVendorPrice, 
				Where<
					APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
					And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
					And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
					And<APVendorPrice.isPromotionalPrice, Equal<Required<APVendorPrice.isPromotionalPrice>>,
					And<APVendorPrice.breakQty, Equal<Required<APVendorPrice.breakQty>>,
					And<APVendorPrice.curyID, Equal<Required<APVendorPrice.curyID>>,
					And2<Where<APVendorPrice.siteID, Equal<Required<APVendorPrice.siteID>>, Or<APVendorPrice.siteID, IsNull, And<Required<APVendorPrice.siteID>, IsNull>>>,
					And<APVendorPrice.recordID, NotEqual<Required<APVendorPrice.recordID>>>>>>>>>>>(graph);
			var duplicates = selectDuplicate.Select(price.VendorID, price.InventoryID, price.UOM, price.IsPromotionalPrice, price.BreakQty, price.CuryID, price.SiteID, price.SiteID, price.RecordID);
			foreach (APVendorPrice apPrice in duplicates)
			{
				if (IsOverlapping(apPrice, price))
				{
					sender.RaiseExceptionHandling<APVendorPrice.uOM>(price, price.UOM, new PXSetPropertyException(AR.Messages.DuplicateSalesPrice, PXErrorLevel.RowError, apPrice.SalesPrice, apPrice.EffectiveDate.HasValue ? apPrice.EffectiveDate.Value.ToShortDateString() : string.Empty, apPrice.ExpirationDate.HasValue ? apPrice.ExpirationDate.Value.ToShortDateString() : string.Empty));
					throw new PXSetPropertyException(AR.Messages.DuplicateSalesPrice, PXErrorLevel.RowError, apPrice.SalesPrice, apPrice.EffectiveDate.HasValue ? apPrice.EffectiveDate.Value.ToShortDateString() : string.Empty, apPrice.ExpirationDate.HasValue ? apPrice.ExpirationDate.Value.ToShortDateString() : string.Empty);
				}
			}
		}

		public static bool IsOverlapping(APVendorPrice vendorPrice1, APVendorPrice vendorPrice2)
		{
			return ((vendorPrice1.EffectiveDate != null && vendorPrice1.ExpirationDate != null && vendorPrice2.EffectiveDate != null && vendorPrice2.ExpirationDate != null && (vendorPrice1.EffectiveDate <= vendorPrice2.EffectiveDate && vendorPrice1.ExpirationDate >= vendorPrice2.EffectiveDate || vendorPrice1.EffectiveDate <= vendorPrice2.ExpirationDate && vendorPrice1.ExpirationDate >= vendorPrice2.ExpirationDate || vendorPrice1.EffectiveDate >= vendorPrice2.EffectiveDate && vendorPrice1.EffectiveDate <= vendorPrice2.ExpirationDate))
						|| (vendorPrice1.ExpirationDate != null && vendorPrice2.EffectiveDate != null && (vendorPrice2.ExpirationDate == null || vendorPrice1.EffectiveDate == null) && vendorPrice2.EffectiveDate <= vendorPrice1.ExpirationDate)
						|| (vendorPrice1.EffectiveDate != null && vendorPrice2.ExpirationDate != null && (vendorPrice2.EffectiveDate == null || vendorPrice1.ExpirationDate == null) && vendorPrice2.ExpirationDate >= vendorPrice1.EffectiveDate)
						|| (vendorPrice1.EffectiveDate != null && vendorPrice2.EffectiveDate != null && vendorPrice1.ExpirationDate == null && vendorPrice2.ExpirationDate == null)
						|| (vendorPrice1.ExpirationDate != null && vendorPrice2.ExpirationDate != null && vendorPrice1.EffectiveDate == null && vendorPrice2.EffectiveDate == null)
						|| (vendorPrice1.EffectiveDate == null && vendorPrice1.ExpirationDate == null)
						|| (vendorPrice2.EffectiveDate == null && vendorPrice2.ExpirationDate == null));
		}

		#region Cost Calculation

		#region CalculateUnitCost - Static calls
		/// <summary>
		/// Calculates Unit Cost.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="siteID">Warehouse</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <returns>Unit Cost</returns>
		public static decimal? CalculateUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost, bool alwaysFromBaseCurrency = false)
			=> CalculateUnitCost(sender, vendorID, vendorLocationID, inventoryID, null, currencyinfo, UOM, quantity, date, currentUnitCost, alwaysFromBaseCurrency);

        /// <summary>
        /// Calculates Unit Cost.
        /// </summary>
        /// <param name="sender">Cache.</param>
        /// <param name="vendorID">The vendor ID.</param>
        /// <param name="vendorLocationID">The vendor location ID.</param>
        /// <param name="inventoryID">Inventory.</param>
        /// <param name="siteID">Warehouse.</param>
        /// <param name="currencyinfo">The currency info.</param>
        /// <param name="UOM">Unit of measure.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="date">Date.</param>
        /// <param name="currentUnitCost">The current unit cost.</param>
        /// <param name="alwaysFromBaseCurrency">(Optional) True to always from base currency.</param>
        /// <returns>
        /// Unit Cost.
        /// </returns>
		public static decimal? CalculateUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, 
                                                 int? siteID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date,
                                                 decimal? currentUnitCost, bool alwaysFromBaseCurrency = false)
			=> SingleAPVendorPriceMaint.CalculateUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, siteID, currencyinfo, UOM, quantity, date, currentUnitCost, alwaysFromBaseCurrency);

		/// <summary>
		/// Calculates Unit Cost in a given currency only.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <returns>Unit Cost</returns>
		public static decimal? CalculateCuryUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, string curyID, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost)
			=> CalculateCuryUnitCost(sender, vendorID, vendorLocationID, inventoryID, null, curyID, UOM, quantity, date, currentUnitCost);

		/// <summary>
		/// Calculates Unit Cost in a given currency only.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="siteID">Warehouse</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <returns>Unit Cost</returns>
		public static decimal? CalculateCuryUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, int? siteID, string curyID, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost)
			=> SingleAPVendorPriceMaint.CalculateCuryUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, siteID, curyID, UOM, quantity, date, currentUnitCost); 
		#endregion
		#region CalculateUnitCost - Virtual calls
		public virtual decimal? CalculateUnitCostInt(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost, bool alwaysFromBaseCurrency = false)
			=> CalculateUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, null, currencyinfo, UOM, quantity, date, currentUnitCost, alwaysFromBaseCurrency);

		public virtual decimal? CalculateUnitCostInt(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, int? siteID,
                                                     CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost,
                                                     bool alwaysFromBaseCurrency = false)
		{
            string curyID = alwaysFromBaseCurrency 
                ? currencyinfo.BaseCuryID 
                : currencyinfo.CuryID;

            decimal preparedQuantity = Math.Abs(quantity ?? 0m);
            UnitCostItem ucItem = FindUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, siteID, currencyinfo.BaseCuryID,
                curyID, preparedQuantity, UOM, date);

			return AdjustUnitCostInt(sender, ucItem, inventoryID, currencyinfo, UOM, currentUnitCost);
		}

		public virtual decimal? CalculateCuryUnitCostInt(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, string curyID, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost)
			=> CalculateCuryUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, null, curyID, UOM, quantity, date, currentUnitCost);

		public virtual decimal? CalculateCuryUnitCostInt(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, int? siteID, string curyID, string UOM, decimal? quantity, DateTime date, decimal? currentUnitCost)
		{
			UnitCostItem ucItem = FindUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, siteID, curyID, curyID, Math.Abs(quantity ?? 0m), UOM, date);
			return AdjustUnitCostInt(sender, ucItem, inventoryID, null, UOM, currentUnitCost);
		} 
		#endregion

		#region FindUnitCost - Static calls
		public static UnitCostItem FindUnitCost(PXCache sender, int? inventoryID, string curyID, string UOM, DateTime date)
			=> FindUnitCost(sender, inventoryID, null, curyID, UOM, date);
		public static UnitCostItem FindUnitCost(PXCache sender, int? inventoryID, int? siteID, string curyID, string UOM, DateTime date)
			=> SingleAPVendorPriceMaint.FindUnitCostInt(sender, inventoryID, siteID, curyID, UOM, date);
		public static UnitCostItem FindUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date)
			=> FindUnitCost(sender, vendorID, vendorLocationID, inventoryID, null, baseCuryID, curyID, quantity, UOM, date);
		public static UnitCostItem FindUnitCost(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date)
			=> SingleAPVendorPriceMaint.FindUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, siteID, baseCuryID, curyID, quantity, UOM, date);
		#endregion
		# region FindUnitCost - Virtual calls
		public virtual UnitCostItem FindUnitCostInt(PXCache sender, int? inventoryID, string curyID, string UOM, DateTime date)
			=> FindUnitCostInt(sender, null, null, inventoryID, null, curyID, curyID, 0m, UOM, date);
		public virtual UnitCostItem FindUnitCostInt(PXCache sender, int? inventoryID, int? siteID, string curyID, string UOM, DateTime date)
			=> FindUnitCostInt(sender, null, null, inventoryID, siteID, curyID, curyID, 0m, UOM, date);
		public virtual UnitCostItem FindUnitCostInt(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date)
			=> FindUnitCostInt(sender, vendorID, vendorLocationID, inventoryID, null, baseCuryID, curyID, quantity, UOM, date);

		/// <summary>
		/// Creates unit cost select command
		/// </summary>
		/// <param name="isBaseUOM">Adds inner join to InventoryItem table with (InventoryItem.baseUnit, Equal(APVendorPrice.uOM))) condition and removes (APVendorPrice.uOM, Equal(Required(APVendorPrice.uOM)))</param>
		public virtual BqlCommand CreateUnitCostSelectCommand(bool isBaseUOM)
		{
			BqlCommand command = new Select<APVendorPrice,
				Where<APVendorPrice.inventoryID, In<Required<APVendorPrice.inventoryID>>,
					And<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
					And<APVendorPrice.curyID, Equal<Required<APVendorPrice.curyID>>,
					And2<
						Where<APVendorPrice.siteID, Equal<Required<APVendorPrice.siteID>>,
						   Or<APVendorPrice.siteID, IsNull>>,
					And<
					  Where2<
						 Where<APVendorPrice.breakQty, LessEqual<Required<APVendorPrice.breakQty>>>,
						   And<
							   Where2<
									Where<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
									 And<APVendorPrice.expirationDate, GreaterEqual<Required<APVendorPrice.expirationDate>>>>,
								Or2<
									Where<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
									  And<APVendorPrice.expirationDate, IsNull>>,
						Or<
						   Where<APVendorPrice.expirationDate, GreaterEqual<Required<APVendorPrice.expirationDate>>,
							 And<APVendorPrice.effectiveDate, IsNull,
							  Or<APVendorPrice.effectiveDate, IsNull,
							 And<APVendorPrice.expirationDate, IsNull>>>>>>>>>>>>>>,

				OrderBy<Desc<APVendorPrice.isPromotionalPrice,
						Desc<APVendorPrice.siteID,
						Desc<APVendorPrice.vendorID,
						Desc<APVendorPrice.breakQty>>>>>>();

			if (isBaseUOM)
			{
				command = BqlCommand.AppendJoin<InnerJoin<InventoryItem,
					   On<InventoryItem.inventoryID, Equal<APVendorPrice.inventoryID>,
					  And<InventoryItem.baseUnit, Equal<APVendorPrice.uOM>>>>>(command);
			}
			else
			{
				command = command.WhereAnd(typeof(Where<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>>));
			}

			return command;
		}

		public virtual UnitCostItem FindUnitCostInt(PXCache sender, int? vendorID, int? vendorLocationID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date)
		{
			int?[] inventoryIDs = GetInventoryIDs(sender, inventoryID);

			PXView view = new PXView(sender.Graph, true, CreateUnitCostSelectCommand(false));
			APVendorPrice vendorPrice = view.SelectSingle(inventoryIDs, vendorID, curyID, siteID, quantity, date, date, date, date, UOM).With(_ => PXResult.Unwrap<APVendorPrice>(_));

			if (vendorPrice != null)
				return CreateUnitCostFromVendorPrice(vendorPrice, UOM);

			decimal baseUnitQty = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, (decimal)quantity, INPrecision.QUANTITY);

			view = new PXView(sender.Graph, true, CreateUnitCostSelectCommand(true));
			vendorPrice = view.SelectSingle(inventoryIDs, vendorID, curyID, siteID, baseUnitQty, date, date, date, date).With(_ => PXResult.Unwrap<APVendorPrice>(_));

			if (vendorPrice != null)
				return CreateUnitCostFromVendorPrice(vendorPrice, vendorPrice.UOM);

			return null;
		}

		/// <summary>
		/// This function is intended for customization purposes and allows to create an array of InventoryIDs to be selected. 
		/// </summary>
		/// <param name="inventoryID">Original InventoryID</param>
		public virtual int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
		{
			return new int?[] { inventoryID };
		}

		/// <summary>
		/// Creates <see cref="UnitCostItem"/> data container from vendor price. Used in PriceUnit customization for Lexware.
		/// </summary>
		/// <param name="vendorPrice">The vendor price.</param>
		/// <param name="uom">The unit of measure.</param>
		/// <returns/>      
		protected internal virtual UnitCostItem CreateUnitCostFromVendorPrice(APVendorPrice vendorPrice, string uom)
        {
            if (vendorPrice == null)
            {
                return null;
            }

            return new UnitCostItem(uom, vendorPrice.SalesPrice ?? 0, vendorPrice.CuryID);
        }
        #endregion

        public static decimal? AdjustUnitCost(PXCache sender, UnitCostItem ucItem, int? inventoryID, CurrencyInfo currencyinfo, string UOM, decimal? currentUnitCost)
			=> SingleAPVendorPriceMaint.AdjustUnitCostInt(sender, ucItem, inventoryID, currencyinfo, UOM, currentUnitCost);

        public virtual decimal? AdjustUnitCostInt(PXCache sender, UnitCostItem ucItem, int? inventoryID, CurrencyInfo currencyinfo, string UOM, decimal? currentUnitCost)
        {
            if (ucItem == null || UOM == null)
            {
                return null;
            }

            decimal unitCost = ucItem.Cost;

            if (currencyinfo != null && ucItem.CuryID != currencyinfo.CuryID)
            {
                PXCurrencyAttribute.CuryConvCury(sender, currencyinfo, ucItem.Cost, out unitCost);
            }

            if (ucItem.UOM == UOM)
            {
                return unitCost;
            }

            decimal salesPriceInBase = INUnitAttribute.ConvertFromBase(sender, inventoryID, ucItem.UOM, unitCost, INPrecision.UNITCOST);
            return INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, salesPriceInBase, INPrecision.UNITCOST);
        }
        
		public static void CheckNewUnitCost<Line, Field>(PXCache sender, Line row, object newValue)
			where Line : class, IBqlTable, new()
			where Field : class, IBqlField

		{
			if (newValue != null && (decimal)newValue != 0m)
				PXUIFieldAttribute.SetWarning<Field>(sender, row, null);
			else
				PXUIFieldAttribute.SetWarning<Field>(sender, row, Messages.NoUnitCostFound);
		}

		public class UnitCostItem
		{
			public string UOM { get; }

			public decimal Cost { get; }

			public string CuryID { get; }

			public UnitCostItem(string uom, decimal cost, string curyid)
			{
				this.UOM = uom;
				this.Cost = cost;
				this.CuryID = curyid;
			}
		}

		#endregion

		public static APVendorPrice FindLastPrice(PXGraph graph, APVendorPrice price)
		{
			APVendorPrice lastPrice = 
				new PXSelect<APVendorPrice,
				Where<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>,
					And<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
					And2<Where<APVendorPrice.siteID, Equal<Required<APVendorPrice.siteID>>, Or<APVendorPrice.siteID, IsNull, And<Required<APVendorPrice.siteID>, IsNull>>>,
					And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
					And<APVendorPrice.isPromotionalPrice, Equal<Required<APVendorPrice.isPromotionalPrice>>,
					And<APVendorPrice.breakQty, Equal<Required<APVendorPrice.breakQty>>,
					And<APVendorPrice.curyID, Equal<Required<APVendorPrice.curyID>>,
					And<APVendorPrice.recordID, NotEqual<Required<APVendorPrice.recordID>>>>>>>>>>,
				OrderBy<Desc<APVendorPrice.effectiveDate>>>(graph).SelectSingle(
					price.VendorID,
					price.InventoryID,
					price.SiteID, price.SiteID,
					price.UOM,
					price.IsPromotionalPrice,
					price.BreakQty,
					price.CuryID,
					price.RecordID);
			return lastPrice;
		}
	}

	[Serializable]
	public partial class APVendorPriceFilter : IBqlTable
	{
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXUIField(DisplayName = "Vendor")]
		[VendorNonEmployeeActive()]
		[PXParent(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APVendorPriceFilter.vendorID>>>>))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[InventoryIncludingTemplates(DisplayName = "Inventory ID")]
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
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXString(50, IsUnicode = true, InputMask = "")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Alternate ID")]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region EffectiveAsOfDate
		public abstract class effectiveAsOfDate : PX.Data.BQL.BqlDateTime.Field<effectiveAsOfDate> { }
		private DateTime? _EffectiveAsOfDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective As Of", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? EffectiveAsOfDate
		{
			get
			{
				return this._EffectiveAsOfDate;
			}
			set
			{
				this._EffectiveAsOfDate = value;
			}
		}
		#endregion
		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), ValidComboRequired = true)]
		public virtual string ItemClassCD
		{
			get { return this._ItemClassCD; }
			set { this._ItemClassCD = value; }
		}
		#endregion
		#region ItemClassCDWildcard
		public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
		[PXString(IsUnicode = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXDimension(INItemClass.Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCDWildcard
		{
			get { return ItemClassTree.MakeWildcard(ItemClassCD); }
			set { }
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[NullableSite]
		public virtual Int32? SiteID
		{
			get { return this._SiteID; }
			set { this._SiteID = value; }
		}
		#endregion
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }

		[PXDBGuid]
		[CR.CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
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
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Product Workgroup")]
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
}