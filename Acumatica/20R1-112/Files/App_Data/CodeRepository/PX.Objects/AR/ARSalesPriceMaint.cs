using System;
using System.Collections.Generic;
using PX.Data;
using System.Collections;
using PX.Objects.AR.Repositories;
using PX.Objects.IN;
using PX.SM;
using PX.TM;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.CM;
using System.Linq;
using PX.Common;
using PX.Objects.CS;
using OwnedFilter = PX.TM.OwnedFilter;
using PX.Objects.Common;

namespace PX.Objects.AR
{
	public class ARSalesPriceMaint : PXGraph<ARSalesPriceMaint>, IPXAuditSource
	{
		private static readonly Lazy<ARSalesPriceMaint> _arSalesPriceMaint = new Lazy<ARSalesPriceMaint>(CreateInstance<ARSalesPriceMaint>);
		public static ARSalesPriceMaint SingleARSalesPriceMaint => _arSalesPriceMaint.Value;

		#region DAC Overrides
		#region ARSalesPrice
		#region PriceType
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[PriceTypes.List]
		[PXUIField(DisplayName = "Price Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void ARSalesPrice_PriceType_CacheAttached(PXCache sender) { }
		#endregion
		#region PriceCode
		[PXMergeAttributes]
		[PXDefault(typeof(ARSalesPriceFilter.priceCode), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCalced(typeof(Switch<
			Case<Where<ARSalesPrice.priceType.IsEqual<PriceTypes.customer>>, Customer.acctCD,
			Case<Where<ARSalesPrice.priceType.IsEqual<PriceTypes.customerPriceClass>>, ARSalesPrice.custPriceClassID>>, Null>), typeof(string))]
		public virtual void ARSalesPrice_PriceCode_CacheAttached(PXCache sender) { }
		#endregion
		#region InventoryID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(typeof(ARSalesPriceFilter.inventoryID))]
		public virtual void ARSalesPrice_InventoryID_CacheAttached(PXCache sender) { }
		#endregion
		#region CustomerCD
		[PXMergeAttributes]
		[PXDBCalced(typeof(Customer.acctCD), typeof(string))]
		[PXFormula(typeof(Selector<ARSalesPrice.customerID, Customer.acctCD>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.customerCD> e) { }
		#endregion
		#region Description
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXStringAttribute))]
		[PXDBLocalizableString(256, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.descr>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.description> e) { }
		#endregion
		#region InventoryCD
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXStringAttribute))]
		[PXDBString(IsUnicode = true, BqlField = typeof(InventoryItem.inventoryCD))]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.inventoryCD>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.inventoryCD> e) { }
		#endregion
		#region ItemStatus
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXStringAttribute))]
		[PXDBString(2, IsFixed = true, BqlField = typeof(InventoryItem.itemStatus))]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.itemStatus>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.itemStatus> e) { }
		#endregion
		#region ItemClassID
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXIntAttribute))]
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.itemClassID>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.itemClassID> e) { }
		#endregion
		#region PriceClassID
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXStringAttribute))]
		[PXDBString(30, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.priceClassID>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.priceClassID> e) { }
		#endregion
		#region PriceWorkgroupID
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXIntAttribute))]
		[PXDBInt(BqlField = typeof(InventoryItem.priceWorkgroupID))]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.priceWorkgroupID>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.priceWorkgroupID> e) { }
		#endregion
		#region PriceManagerID
		[PXMergeAttributes]
		[PXRemoveBaseAttribute(typeof(PXGuidAttribute))]
		[PXDBGuid(BqlField = typeof(InventoryItem.priceManagerID))]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.priceManagerID>))]
		public virtual void _(Events.CacheAttached<ARSalesPrice.priceManagerID> e) { }
		#endregion
		#endregion
		#endregion

		#region Selects/Views
		public PXFilter<ARSalesPriceFilter> Filter;

		[PXFilterable]
		public PXSelectJoin<ARSalesPrice,
			LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<ARSalesPrice.itemClassID>>>,
			Where<ARSalesPrice.itemStatus, NotIn3<INItemStatus.inactive, InventoryItemStatus.unknown, INItemStatus.toDelete>,
			And2<Where<ARSalesPrice.isFairValue, NotEqual<True>, Or<FeatureInstalled<FeaturesSet.aSC606>>>,
			And2<Where<Required<ARSalesPriceFilter.priceType>, Equal<PriceTypes.allPrices>, Or<ARSalesPrice.priceType, Equal<Required<ARSalesPriceFilter.priceType>>>>,
			And2<Where<ARSalesPrice.customerID, Equal<Required<ARSalesPriceFilter.priceCode>>, Or<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPriceFilter.priceCode>>, Or<Required<ARSalesPriceFilter.priceCode>, IsNull>>>,
			And2<Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPriceFilter.inventoryID>>, Or<Required<ARSalesPriceFilter.inventoryID>, IsNull>>,
			And2<Where<ARSalesPrice.siteID, Equal<Required<ARSalesPriceFilter.siteID>>, Or<Required<ARSalesPriceFilter.siteID>, IsNull>>,
			And2<Where2<Where2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPriceFilter.effectiveAsOfDate>>, Or<ARSalesPrice.effectiveDate, IsNull>>,
			And<Where<ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPriceFilter.effectiveAsOfDate>>, Or<ARSalesPrice.expirationDate, IsNull>>>>,
			Or<Required<ARSalesPriceFilter.effectiveAsOfDate>, IsNull>>,
			And<Where2<Where<Required<ARSalesPriceFilter.itemClassCD>, IsNull,
					Or<INItemClass.itemClassCD, Like<Required<ARSalesPriceFilter.itemClassCDWildcard>>>>,
				And2<Where<Required<ARSalesPriceFilter.inventoryPriceClassID>, IsNull,
					Or<Required<ARSalesPriceFilter.inventoryPriceClassID>, Equal<ARSalesPrice.priceClassID>>>,
				And2<Where<Required<ARSalesPriceFilter.ownerID>, IsNull,
					Or<Required<ARSalesPriceFilter.ownerID>, Equal<ARSalesPrice.priceManagerID>>>,
				And2<Where<Required<ARSalesPriceFilter.myWorkGroup>, Equal<False>,
						 Or<ARSalesPrice.priceWorkgroupID, InMember<CurrentValue<ARSalesPriceFilter.currentOwnerID>>>>,
				And<Where<Required<ARSalesPriceFilter.workGroupID>, IsNull,
					Or<Required<ARSalesPriceFilter.workGroupID>, Equal<ARSalesPrice.priceWorkgroupID>>>>>>>>>>>>>>>>,
				OrderBy<Asc<ARSalesPrice.inventoryCD,
						Asc<ARSalesPrice.priceType,
						Asc<ARSalesPrice.uOM, Asc<ARSalesPrice.breakQty, Asc<ARSalesPrice.effectiveDate>>>>>>> Records;

		public PXSetup<Company> Company;
		public PXSetup<ARSetup> arsetup;

		public PXSelect<CR.BAccount,
					Where<CR.BAccount.type, Equal<CR.BAccountType.customerType>,
						Or<CR.BAccount.type, Equal<CR.BAccountType.combinedType>>>> CustomerCode;

		public PXSelect<Customer> Customer;
		public PXSelect<ARPriceClass> CustPriceClassCode;

		protected readonly CustomerRepository CustomerRepository;

		public virtual IEnumerable records()
		{
			if (PXView.MaximumRows == 1 
				&& PXView.SortColumns?.Length > 0 && PXView.SortColumns[0].Equals(nameof(ARSalesPrice.RecordID), StringComparison.OrdinalIgnoreCase)
				&& PXView.Searches?.Length > 0 && PXView.Searches[0] != null)
			{
				var cached = Records.Cache.Locate(new ARSalesPrice { RecordID = Convert.ToInt32(PXView.Searches[0]) });
				if (cached != null)
					return new[] { cached };
			}

			ARSalesPriceFilter filter = Filter.Current;

			var priceCode = ParsePriceCode(this, filter.PriceType, filter.PriceCode);

			return QSelect(
				this,
				Records.View.BqlSelect,
				new object[]
				{
					filter.PriceType, filter.PriceType,
					filter.PriceType == PriceTypes.Customer ? priceCode : null,
					filter.PriceType == PriceTypes.CustomerPriceClass ? priceCode : null,
					priceCode,
					filter.InventoryID, filter.InventoryID,
					filter.SiteID, filter.SiteID,
					filter.EffectiveAsOfDate, filter.EffectiveAsOfDate, filter.EffectiveAsOfDate,
					filter.ItemClassCD, filter.ItemClassCDWildcard,
					filter.InventoryPriceClassID, filter.InventoryPriceClassID,
					filter.OwnerID, filter.OwnerID,
					filter.MyWorkGroup,
					filter.WorkGroupID, filter.WorkGroupID
				});
		}

		public static IEnumerable QSelect(PXGraph graph, BqlCommand bqlCommand, object[] viewParameters)
		{
			var view = new PXView(graph, false, bqlCommand);
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(PXView.Currents, viewParameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		#endregion

		#region Ctors
		public ARSalesPriceMaint()
		{
			CustomerRepository = new CustomerRepository(this);
			bool loadSalesPriceByAlternateID = PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && arsetup.Current.LoadSalesPricesUsingAlternateID == true;
			CrossItemAttribute.SetEnableAlternateSubstitution<ARSalesPrice.inventoryID>(Records.Cache, null, loadSalesPriceByAlternateID);
		}

		#endregion

		#region Buttons/Actions
		public PXSave<ARSalesPriceFilter> Save;
		public PXCancel<ARSalesPriceFilter> Cancel;

		public PXAction<ARSalesPriceFilter> createWorksheet;

        [PXUIField(DisplayName = Messages.CreatePriceWorksheet, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable CreateWorksheet(PXAdapter adapter)
        {
            if (Filter.Current == null)
            {
                return adapter.Get();
            }

            Save.Press();
            string priceCode = ParsePriceCode(this, Filter.Current.PriceType, Filter.Current.PriceCode);
            ARPriceWorksheetMaint graph = CreateInstance<ARPriceWorksheetMaint>();
            ARPriceWorksheet worksheet = new ARPriceWorksheet();
            graph.Document.Insert(worksheet);
            int startRow = PXView.StartRow;
            int totalRows = 0;

            object[] parameters =
            {
                    Filter.Current.PriceType,
                    Filter.Current.PriceType,
                    Filter.Current.PriceType == PriceTypes.Customer ? priceCode : null,
                    Filter.Current.PriceType == PriceTypes.CustomerPriceClass ? priceCode : null,
                    priceCode,
                    Filter.Current.InventoryID,
                    Filter.Current.InventoryID,
                    Filter.Current.SiteID,
                    Filter.Current.SiteID,
                    Filter.Current.EffectiveAsOfDate,
                    Filter.Current.EffectiveAsOfDate,
                    Filter.Current.EffectiveAsOfDate,
                    Filter.Current.ItemClassCD,
                    Filter.Current.ItemClassCDWildcard,
                    Filter.Current.InventoryPriceClassID,
                    Filter.Current.InventoryPriceClassID,
                    Filter.Current.OwnerID,
                    Filter.Current.OwnerID,
                    Filter.Current.MyWorkGroup,
                    Filter.Current.WorkGroupID,
                    Filter.Current.WorkGroupID
                };

            Func<BqlCommand, List<Object>> performSelect = command =>
                new PXView(this, false, command)
                    .Select(
                        PXView.Currents,
                        parameters,
                        PXView.Searches,
                        PXView.SortColumns,
                        PXView.Descendings,
                        Records.View.GetExternalFilters(),
                        ref startRow,
                        PXView.MaximumRows,
                        ref totalRows);

            List<object> allSalesPrices = performSelect(
                PXSelectJoin<ARSalesPrice,
                LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<ARSalesPrice.inventoryID>>,
                LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<ARSalesPrice.itemClassID>>>>,
                Where<ARSalesPrice.itemStatus, NotIn3<INItemStatus.inactive, InventoryItemStatus.unknown, INItemStatus.toDelete>,
                    And2<Where<Required<ARSalesPriceFilter.priceType>, Equal<PriceTypes.allPrices>, Or<ARSalesPrice.priceType, Equal<Required<ARSalesPriceFilter.priceType>>>>,
                    And2<Where<ARSalesPrice.customerID, Equal<Required<ARSalesPriceFilter.priceCode>>, Or<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPriceFilter.priceCode>>, Or<Required<ARSalesPriceFilter.priceCode>, IsNull>>>,
                    And2<Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPriceFilter.inventoryID>>, Or<Required<ARSalesPriceFilter.inventoryID>, IsNull>>,
                    And2<Where<ARSalesPrice.siteID, Equal<Required<ARSalesPriceFilter.siteID>>, Or<Required<ARSalesPriceFilter.siteID>, IsNull>>,
                    And2<Where2<Where2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPriceFilter.effectiveAsOfDate>>, Or<ARSalesPrice.effectiveDate, IsNull>>,
                    And<Where<ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPriceFilter.effectiveAsOfDate>>, Or<ARSalesPrice.expirationDate, IsNull>>>>,
                    Or<Required<ARSalesPriceFilter.effectiveAsOfDate>, IsNull>>,
                    And<Where2<Where<Required<ARSalesPriceFilter.itemClassCD>, IsNull,
                            Or<INItemClass.itemClassCD, Like<Required<ARSalesPriceFilter.itemClassCDWildcard>>>>,
                        And2<Where<Required<ARSalesPriceFilter.inventoryPriceClassID>, IsNull,
                            Or<Required<ARSalesPriceFilter.inventoryPriceClassID>, Equal<ARSalesPrice.priceClassID>>>,
                        And2<Where<Required<ARSalesPriceFilter.ownerID>, IsNull,
                            Or<Required<ARSalesPriceFilter.ownerID>, Equal<ARSalesPrice.priceManagerID>>>,
                        And2<Where<Required<ARSalesPriceFilter.myWorkGroup>, Equal<False>,
                                    Or<ARSalesPrice.priceWorkgroupID, InMember<CurrentValue<ARSalesPriceFilter.currentOwnerID>>>>,
                        And<Where<Required<ARSalesPriceFilter.workGroupID>, IsNull,
                            Or<Required<ARSalesPriceFilter.workGroupID>, Equal<ARSalesPrice.priceWorkgroupID>>>>>>>>>>>>>>>>.GetCommand());

            List<object> groupedSalesPrices = performSelect(
                PXSelectJoinGroupBy<ARSalesPrice,
                LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<ARSalesPrice.inventoryID>>,
                LeftJoin<INItemClass, On<INItemClass.itemClassID, Equal<ARSalesPrice.itemClassID>>>>,
                Where<ARSalesPrice.itemStatus, NotIn3<INItemStatus.inactive, InventoryItemStatus.unknown, INItemStatus.toDelete>,
                    And2<Where<Required<ARSalesPriceFilter.priceType>, Equal<PriceTypes.allPrices>, Or<ARSalesPrice.priceType, Equal<Required<ARSalesPriceFilter.priceType>>>>,
                    And2<Where<ARSalesPrice.customerID, Equal<Required<ARSalesPriceFilter.priceCode>>, Or<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPriceFilter.priceCode>>, Or<Required<ARSalesPriceFilter.priceCode>, IsNull>>>,
                    And2<Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPriceFilter.inventoryID>>, Or<Required<ARSalesPriceFilter.inventoryID>, IsNull>>,
                    And2<Where<ARSalesPrice.siteID, Equal<Required<ARSalesPriceFilter.siteID>>, Or<Required<ARSalesPriceFilter.siteID>, IsNull>>,
                    And2<Where2<Where2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPriceFilter.effectiveAsOfDate>>, Or<ARSalesPrice.effectiveDate, IsNull>>,
                    And<Where<ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPriceFilter.effectiveAsOfDate>>, Or<ARSalesPrice.expirationDate, IsNull>>>>,
                        Or<Required<ARSalesPriceFilter.effectiveAsOfDate>, IsNull>>,
                    And<Where2<Where<Required<ARSalesPriceFilter.itemClassCD>, IsNull,
                            Or<INItemClass.itemClassCD, Like<Required<ARSalesPriceFilter.itemClassCDWildcard>>>>,
                    And2<Where<Required<ARSalesPriceFilter.inventoryPriceClassID>, IsNull,
                        Or<Required<ARSalesPriceFilter.inventoryPriceClassID>, Equal<ARSalesPrice.priceClassID>>>,
                    And2<Where<Required<ARSalesPriceFilter.ownerID>, IsNull,
                        Or<Required<ARSalesPriceFilter.ownerID>, Equal<ARSalesPrice.priceManagerID>>>,
                    And2<Where<Required<ARSalesPriceFilter.myWorkGroup>, Equal<False>,
                        Or<ARSalesPrice.priceWorkgroupID, InMember<CurrentValue<ARSalesPriceFilter.currentOwnerID>>>>,
                    And<Where<Required<ARSalesPriceFilter.workGroupID>, IsNull,
                        Or<Required<ARSalesPriceFilter.workGroupID>, Equal<ARSalesPrice.priceWorkgroupID>>>>>>>>>>>>>>>,
                Aggregate<
                    GroupBy<ARSalesPrice.priceType,
                    GroupBy<ARSalesPrice.customerID,
                    GroupBy<ARSalesPrice.custPriceClassID,
                    GroupBy<ARSalesPrice.inventoryID,
                    GroupBy<ARSalesPrice.uOM,
                    GroupBy<ARSalesPrice.breakQty,
                    GroupBy<ARSalesPrice.curyID,
                    GroupBy<ARSalesPrice.siteID>>>>>>>>>>.GetCommand());

            if (allSalesPrices.Count > groupedSalesPrices.Count)
            {
                throw new PXException(Messages.MultiplePriceRecords);
            }

            CreateWorksheetDetailsFromSalesPrices(graph, groupedSalesPrices);
            throw new PXRedirectRequiredException(graph, Messages.CreatePriceWorksheet);
        }

        /// <summary>
        /// Creates worksheet details from sales prices. Extended in Lexware Price Unit customization.
        /// </summary>
        /// <param name="graph">The ARPriceWorksheetMaint graph.</param>
        /// <param name="groupedSalesPrices">The grouped sales prices.</param>
        protected virtual void CreateWorksheetDetailsFromSalesPrices(ARPriceWorksheetMaint graph, List<object> groupedSalesPrices)
        {
            foreach (PXResult<ARSalesPrice> res in groupedSalesPrices)
            {
                ARSalesPrice price = res;

                var detail = new ARPriceWorksheetDetail
                {
                    RefNbr = graph.Document.Current.RefNbr,
                    PriceType = price.PriceType
                };

                if (detail.PriceType == PriceTypes.Customer)
                {
                    Customer customer = PXSelect<Customer,
                                           Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                       .Select(this, price.CustomerID);

                    if (customer != null)
                        detail.PriceCode = customer.AcctCD;
                }
                else
                {
                    detail.PriceCode = price.CustPriceClassID != ARPriceClass.EmptyPriceClass ? price.CustPriceClassID : null;
                }

                detail.CustomerID = price.CustomerID;
                detail.CustPriceClassID = price.CustPriceClassID != ARPriceClass.EmptyPriceClass ? price.CustPriceClassID : null;

                detail = graph.Details.Insert(detail);

                FillWorksheetDetailFromSalesPriceOnWorksheetCreation(detail, price);

                graph.Details.Update(detail);
            }
        }

        /// <summary>
        /// Fill worksheet detail from sales price on worksheet creation. Extended in Lexware Price Unit customization.
        /// </summary>
        /// <param name="detail">The detail.</param>
        /// <param name="price">The price.</param>
        protected virtual void FillWorksheetDetailFromSalesPriceOnWorksheetCreation(ARPriceWorksheetDetail detail, ARSalesPrice price)
        {
            detail.InventoryID = price.InventoryID;
            detail.UOM = price.UOM;
            detail.BreakQty = price.BreakQty;
            detail.CurrentPrice = price.SalesPrice;
            detail.CuryID = price.CuryID;
            detail.TaxID = price.TaxID;
            detail.SiteID = price.SiteID;
        }
        #endregion

		#region IPXAuditSource

		string IPXAuditSource.GetMainView()
		{
			return nameof(Records);
		}

		IEnumerable<Type> IPXAuditSource.GetAuditedTables()
		{
			yield return typeof(ARSalesPrice);
		}

		#endregion

        #region Event Handlers
        public override void Persist()
		{
			foreach (ARSalesPrice price in Records.Cache.Inserted)
			{
				ARSalesPrice lastPrice = FindLastPrice(this, price);
				if (lastPrice?.EffectiveDate > price.EffectiveDate && price.ExpirationDate == null)
				{
					Records.Cache.RaiseExceptionHandling<ARSalesPrice.expirationDate>(price, price.ExpirationDate, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARSalesPrice.expirationDate>(Records.Cache)));
					throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARSalesPrice.expirationDate>(Records.Cache));
				}
				ValidateDuplicate(this, Records.Cache, price);
			}
			foreach (ARSalesPrice price in Records.Cache.Updated)
			{
				ARSalesPrice lastPrice = FindLastPrice(this, price);
				if (lastPrice?.EffectiveDate > price.EffectiveDate && price.ExpirationDate == null)
				{
					Records.Cache.RaiseExceptionHandling<ARSalesPrice.expirationDate>(price, price.ExpirationDate, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARSalesPrice.expirationDate>(Records.Cache)));
					throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARSalesPrice.expirationDate>(Records.Cache));
				}
				ValidateDuplicate(this, Records.Cache, price);
			}
			base.Persist();
		}

        protected virtual void ARSalesPrice_SalesPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            ARSalesPrice salesPrice = e.Row as ARSalesPrice;

            if (salesPrice == null)
            {
                e.NewValue = 0m;
                return;
            }

			InventoryItem item = InventoryItem.PK.Find(this, salesPrice.InventoryID);

            if (item == null || salesPrice.CuryID != Company.Current.BaseCuryID)
                return;

            e.NewValue = salesPrice.UOM == item.BaseUnit
                ? item.BasePrice
                : INUnitAttribute.ConvertToBase(sender, item.InventoryID, salesPrice.UOM ?? item.SalesUnit, item.BasePrice.Value, INPrecision.UNITCOST);
        }

		protected virtual void ARSalesPrice_PriceType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null)
			{
				if (Filter.Current != null && Filter.Current.PriceType != PriceTypes.AllPrices)
					e.NewValue = Filter.Current.PriceType;
				else
					e.NewValue = PriceTypes.BasePrice;
			}

		}

		protected virtual void ARSalesPrice_SalesPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARSalesPrice row = e.Row as ARSalesPrice;
			if (row != null && MinGrossProfitValidation != MinGrossProfitValidationType.None && row.EffectiveDate != null && PXAccess.FeatureInstalled<CS.FeaturesSet.distributionModule>())
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
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
					INItemCost cost = INItemCost.PK.Find(this, row.InventoryID) ?? new INItemCost();
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

					e.NewValue = MinGrossProfitValidator.Validate<ARSalesPrice.salesPrice>(
						sender, row, item,
						validationMode: MinGrossProfitValidation,
						currentValue: newValue,
						minValue: minPrice,
						newValue: (decimal?) e.NewValue,
						setToMinValue: minPrice,
						target: MinGrossProfitValidator.Target.SalesPrice);
				}
			}
		}

		protected virtual void ARSalesPrice_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARSalesPrice.salesPrice>(e.Row);
		}

		protected virtual void _(Events.RowInserted<ARSalesPrice> e)
		{
			if (!string.IsNullOrEmpty(e.Row?.PriceCode))
				UpdateCustomerAndPriceClassFields(e.Row);
		}

		protected virtual void ARSalesPrice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			ARSalesPrice oldRow = (ARSalesPrice)e.OldRow;
			if (!sender.ObjectsEqual<ARSalesPrice.priceType>(row, oldRow))
				row.PriceCode = null;

			if (!sender.ObjectsEqual<ARSalesPrice.priceCode>(row, oldRow))
			{
				UpdateCustomerAndPriceClassFields(row);
			}
		}

		protected virtual void ARSalesPrice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARSalesPrice.priceType>(sender, row, Filter.Current.PriceType == PriceTypes.AllPrices);
				PXUIFieldAttribute.SetEnabled<ARSalesPrice.inventoryID>(sender, row, Filter.Current.InventoryID == null);
				PXUIFieldAttribute.SetEnabled<ARSalesPrice.priceCode>(sender, row, row.PriceType != PriceTypes.BasePrice);
			}
		}

		protected virtual void ARSalesPrice_PriceCode_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			ARSalesPrice det = (ARSalesPrice)e.Row;
			if (det?.PriceType == null) return;

			if (det.PriceType == PriceTypes.Customer)
			{
				string customerCD = det.PriceCode;
				if (string.IsNullOrEmpty(customerCD))
				{
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, det.CustomerID);
				if (customer != null)
				{
						det.PriceCode 
							= customerCD
							= customer.AcctCD;
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

		protected virtual void ARSalesPrice_PriceCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARSalesPrice det = (ARSalesPrice)e.Row;
			if (det == null) return;
			if (e.NewValue == null)
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.priceCode).Name);

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

		protected virtual void ARSalesPriceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name) == false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, e.Row == null || (bool?)sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name) == false);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.priceCode>(sender, e.Row, (((ARSalesPriceFilter)e.Row).PriceType != PriceTypes.AllPrices && ((ARSalesPriceFilter)e.Row).PriceType != PriceTypes.BasePrice));
		}

		protected virtual void ARSalesPriceFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARSalesPriceFilter filter = (ARSalesPriceFilter)e.Row;
			ARSalesPriceFilter oldFilter = (ARSalesPriceFilter)e.OldRow;
			if (!sender.ObjectsEqual<ARSalesPriceFilter.priceType>(oldFilter, filter))
				filter.PriceCode = null;
		}

		protected virtual void ARSalesPrice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARSalesPrice row = (ARSalesPrice)e.Row;
			if (row.IsPromotionalPrice == true && row.ExpirationDate == null)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.expirationDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.expirationDate).Name));
			}
			if (row.IsPromotionalPrice == true && row.EffectiveDate == null)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.effectiveDate>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.effectiveDate).Name));
			}
			if (row.ExpirationDate < row.EffectiveDate)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.expirationDate>(row, row.ExpirationDate, new PXSetPropertyException(Messages.EffectiveDateExpirationDate, PXErrorLevel.RowError));
			}

			if (row.PriceType != PriceTypes.BasePrice && row.PriceCode == null)
			{
				sender.RaiseExceptionHandling<ARSalesPrice.priceCode>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(ARSalesPrice.priceCode).Name));
				return;
			}

			UpdateCustomerAndPriceClassFields(row);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		protected virtual void ARSalesPriceInventoryIDCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e) { }

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		protected virtual void ARSalesPricePriceTypeCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e) { }

		#endregion

		#region Private Members
		public string GetPriceType(string viewname)
		{
			return viewname.Contains(typeof(ARSalesPriceFilter).Name) && Filter.Current != null
				? Filter.Current.PriceType
				: (viewname.Contains(typeof(ARSalesPrice).Name) && Records.Current != null
					? Records.Current.PriceType
					: PriceTypeList.Customer);
		}

		#region Overrides
		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if ((viewName.Contains(typeof(ARSalesPrice.priceCode).Name) || viewName.Contains("PriceCode")) && ViewNames.ContainsKey(CustomerCode.View) && ViewNames.ContainsKey(CustPriceClassCode.View))
			{
				if (GetPriceType(viewName) == PriceTypeList.Customer)
				{
					viewName = ViewNames[CustomerCode.View];
					if (sortcolumns.Any())
					{
						sortcolumns[0] = typeof(CR.BAccount.acctCD).Name;
					}
					ModifyFilters(filters, typeof(ARSalesPrice.priceCode).Name, typeof(CR.BAccount.acctCD).Name);
					ModifyFilters(filters, typeof(ARSalesPrice.description).Name, typeof(CR.BAccount.acctName).Name);
				}
				else
				{
					viewName = ViewNames[CustPriceClassCode.View];
					if (sortcolumns.Any())
					{
						sortcolumns[0] = typeof(ARPriceClass.priceClassID).Name;
					}
					ModifyFilters(filters, typeof(ARSalesPrice.priceCode).Name, typeof(ARPriceClass.priceClassID).Name);
				}
			}
			IEnumerable ret = base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			return ret;
		}

		public override Type GetItemType(string viewName)
		{
			if ((viewName.Contains(typeof(ARSalesPrice.priceCode).Name) || viewName.Contains("PriceCode")) && this.ViewNames.ContainsKey(CustomerCode.View) && this.ViewNames.ContainsKey(CustPriceClassCode.View))
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

		private static void ModifyFilters(PXFilterRow[] filters, string originalFieldName, string newFieldName)
		{
			if (filters != null)
			{
				foreach (PXFilterRow filter in filters)
				{
					if (String.Equals(filter.DataField, originalFieldName, StringComparison.OrdinalIgnoreCase))
					{
						filter.DataField = newFieldName;
					}
				}
			}
		}

		public override object GetStateExt(string viewName, object data, string fieldName)
		{
			if ((viewName.Contains(typeof(ARSalesPrice.priceCode).Name)
				|| viewName.Contains("PriceCode")) && ViewNames.ContainsKey(CustomerCode.View) && ViewNames.ContainsKey(CustPriceClassCode.View))
			{
				if (GetPriceType(viewName) == PriceTypeList.Customer)
				{
					string customerCodeViewName = ViewNames[CustomerCode.View];
					if (String.Equals(fieldName, typeof(ARSalesPrice.priceCode).Name, StringComparison.OrdinalIgnoreCase))
					{
						return base.GetStateExt(customerCodeViewName, data, typeof(CR.BAccount.acctCD).Name);
					}
					else if (String.Equals(fieldName, typeof(ARSalesPrice.description).Name, StringComparison.OrdinalIgnoreCase))
					{
						return base.GetStateExt(customerCodeViewName, data, typeof(CR.BAccount.acctName).Name);
					}
				}
				else
				{
					string custPriceClassCodViewName = ViewNames[CustPriceClassCode.View];
					if (String.Equals(fieldName, typeof(ARSalesPrice.priceCode).Name, StringComparison.OrdinalIgnoreCase))
					{
						return base.GetStateExt(custPriceClassCodViewName, data, typeof(ARPriceClass.priceClassID).Name);
					}
				}
			}

			return base.GetStateExt(viewName, data, fieldName);
		}

		public override object GetValueExt(string viewName, object data, string fieldName)
		{
			if (viewName.Contains(typeof(ARSalesPrice.priceCode).Name))
			{
				if (GetPriceType(viewName) == PriceTypeList.Customer)
				{
					viewName = ViewNames[CustomerCode.View];
					if (String.Equals(fieldName, PriceCodeInfo.PriceCodeFieldName, StringComparison.OrdinalIgnoreCase))
					{
						fieldName = typeof (CR.BAccount.acctCD).Name;
					}
					else if (String.Equals(fieldName, PriceCodeInfo.PriceCodeDescrFieldName, StringComparison.OrdinalIgnoreCase))
					{
						fieldName = typeof (CR.BAccount.acctName).Name;
					}
					else
					{
							return null;
					}
				}
				else
				{
					viewName = ViewNames[CustPriceClassCode.View];
					if (String.Equals(fieldName, PriceCodeInfo.PriceCodeFieldName, StringComparison.OrdinalIgnoreCase))
					{
						fieldName = typeof (ARPriceClass.priceClassID).Name;
					}
					else if (String.Equals(fieldName, PriceCodeInfo.PriceCodeDescrFieldName, StringComparison.OrdinalIgnoreCase))
					{
						fieldName = typeof (ARPriceClass.description).Name;
					}
					else
					{
							return null;
					}
				}
			}

			return base.GetValueExt(viewName, data, fieldName);
		}
		#endregion

		public static void ValidateDuplicate(PXGraph graph, PXCache sender, ARSalesPrice price)
		{
			PXSelectBase<ARSalesPrice> selectDuplicate =
				new PXSelect<ARSalesPrice,
				Where<
					ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>,
					And2<Where<
						ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>, And<ARSalesPrice.custPriceClassID, IsNull,
						Or<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>, And<ARSalesPrice.customerID, IsNull,
						Or<ARSalesPrice.custPriceClassID, IsNull, And<ARSalesPrice.customerID, IsNull>>>>>>,
					And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
					And2<Where<ARSalesPrice.siteID, Equal<Required<ARSalesPrice.siteID>>, Or<ARSalesPrice.siteID, IsNull, And<Required<ARSalesPrice.siteID>, IsNull>>>,
					And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
					And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>,
					And<ARSalesPrice.isFairValue, Equal<Required<ARSalesPrice.isFairValue>>,
					And<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>,
					And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
					And<ARSalesPrice.recordID, NotEqual<Required<ARSalesPrice.recordID>>>>>>>>>>>>>(graph);

			string priceCode = ParsePriceCode(graph, price.PriceType, price.PriceCode);
			int? customerID = null;
			if (price.PriceType == PriceTypes.Customer && priceCode != null)
			{
				customerID = int.Parse(priceCode);
			}

			PXResultset<ARSalesPrice> duplicates = selectDuplicate.Select(
				price.PriceType,
				customerID,
				price.PriceType == PriceTypes.CustomerPriceClass ? priceCode : null,
				price.InventoryID,
				price.SiteID, price.SiteID,
				price.UOM,
				price.IsPromotionalPrice,
				price.IsFairValue,
				price.BreakQty,
				price.CuryID,
				price.RecordID);

			foreach (ARSalesPrice arPrice in duplicates)
			{
				if (IsOverlapping(arPrice, price))
				{
					PXSetPropertyException exception = new PXSetPropertyException(
						Messages.DuplicateSalesPrice,
						PXErrorLevel.RowError,
						arPrice.SalesPrice,
						arPrice.EffectiveDate?.ToShortDateString() ?? string.Empty,
						arPrice.ExpirationDate?.ToShortDateString() ?? string.Empty);
					sender.RaiseExceptionHandling<ARSalesPrice.uOM>(price, price.UOM, exception);
					throw exception;
				}
			}
		}

		public static ARSalesPrice FindLastPrice(PXGraph graph, ARSalesPrice price)
		{
			string priceCode = ParsePriceCode(graph, price.PriceType, price.PriceCode);
			ARSalesPrice lastPrice = new PXSelect<ARSalesPrice,
				Where<ARSalesPrice.priceType, Equal<Required<ARSalesPrice.priceType>>,
				And2<Where2<
					Where<ARSalesPrice.customerID, Equal<Required<ARSalesPriceFilter.priceCode>>, And<ARSalesPrice.custPriceClassID, IsNull>>,
					Or<Where<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPriceFilter.priceCode>>, And<ARSalesPrice.customerID, IsNull>>>>,
				And<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
				And2<Where<ARSalesPrice.siteID, Equal<Required<ARSalesPrice.siteID>>, Or<ARSalesPrice.siteID, IsNull, And<Required<ARSalesPrice.siteID>, IsNull>>>,
				And<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>,
				And<ARSalesPrice.isPromotionalPrice, Equal<Required<ARSalesPrice.isPromotionalPrice>>,
				And<ARSalesPrice.isFairValue, Equal<Required<ARSalesPrice.isFairValue>>,
				And<ARSalesPrice.breakQty, Equal<Required<ARSalesPrice.breakQty>>,
				And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
				And<ARSalesPrice.recordID, NotEqual<Required<ARSalesPrice.recordID>>>>>>>>>>>>,
				OrderBy<Desc<ARSalesPrice.effectiveDate>>>(graph).SelectSingle(
				price.PriceType,
				price.PriceType == PriceTypes.Customer ? priceCode : null,
				price.PriceType == PriceTypes.CustomerPriceClass ? priceCode : null,
				price.InventoryID,
				price.SiteID, price.SiteID,
				price.UOM,
				price.IsPromotionalPrice,
				price.IsFairValue,
				price.BreakQty,
				price.CuryID,
				price.RecordID);
			return lastPrice;
		}

		private static string ParsePriceCode(PXGraph graph, string priceType, string priceCode)
		{
			if (priceCode != null)
			{
				if (priceType == PriceTypes.Customer)
				{
					var customerRepository = new CustomerRepository(graph);

					Customer customer = customerRepository.FindByCD(priceCode);
					if (customer != null)
					{
						return customer.BAccountID.ToString();
					}
				}
				return priceType == PriceTypes.CustomerPriceClass ? priceCode : null;
			}
			else
				return null;

		}

		private decimal ConvertAmt(string from, string to, string rateType, DateTime effectiveDate, decimal amount, decimal? customRate = 1)
		{
			if (from == to)
			{
				return amount;
			}

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

		private void UpdateCustomerAndPriceClassFields(ARSalesPrice row)
		{
			switch (row.PriceType)
			{
				case PriceTypeList.Customer:
					Customer customer = CustomerRepository.FindByCD(row.PriceCode);
					if (customer != null)
					{
						row.CustomerID = customer.BAccountID;
						row.CustPriceClassID = null;
					}
					break;
				case PriceTypeList.CustomerPriceClass:
					row.CustomerID = null;
					row.CustPriceClassID = row.PriceCode;
					break;
				case PriceTypeList.BasePrice:
					row.CustomerID = null;
					row.CustPriceClassID = null;
					break;
			}
		}

		public static bool IsOverlapping(ARSalesPrice salesPrice1, ARSalesPrice salesPrice2)
		{
			return ((salesPrice1.EffectiveDate != null && salesPrice1.ExpirationDate != null && salesPrice2.EffectiveDate != null && salesPrice2.ExpirationDate != null && (salesPrice1.EffectiveDate <= salesPrice2.EffectiveDate && salesPrice1.ExpirationDate >= salesPrice2.EffectiveDate || salesPrice1.EffectiveDate <= salesPrice2.ExpirationDate && salesPrice1.ExpirationDate >= salesPrice2.ExpirationDate || salesPrice1.EffectiveDate >= salesPrice2.EffectiveDate && salesPrice1.EffectiveDate <= salesPrice2.ExpirationDate))
						|| (salesPrice1.ExpirationDate != null && salesPrice2.EffectiveDate != null && (salesPrice2.ExpirationDate == null || salesPrice1.EffectiveDate == null) && salesPrice2.EffectiveDate <= salesPrice1.ExpirationDate)
						|| (salesPrice1.EffectiveDate != null && salesPrice2.ExpirationDate != null && (salesPrice2.EffectiveDate == null || salesPrice1.ExpirationDate == null) && salesPrice2.ExpirationDate >= salesPrice1.EffectiveDate)
						|| (salesPrice1.EffectiveDate != null && salesPrice2.EffectiveDate != null && salesPrice1.ExpirationDate == null && salesPrice2.ExpirationDate == null)
						|| (salesPrice1.ExpirationDate != null && salesPrice2.ExpirationDate != null && salesPrice1.EffectiveDate == null && salesPrice2.EffectiveDate == null)
						|| (salesPrice1.EffectiveDate == null && salesPrice1.ExpirationDate == null)
						|| (salesPrice2.EffectiveDate == null && salesPrice2.ExpirationDate == null));
		}
		#endregion

		#region Sales Price Calculation

		#region CalculateSalesPrice - Static calls
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
			=> CalculateSalesPrice(sender, custPriceClass, inventoryID, null, currencyinfo, UOM, date);

		/// <summary>
		/// Calculates Sales Price.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="siteID">Warehouse</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <returns>Sales Price.</returns>
		/// <remarks>AlwaysFromBaseCury flag in the SOSetup is considered when performing the calculation.</remarks>
		public static decimal? CalculateSalesPrice(PXCache sender, string custPriceClass, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, string UOM, DateTime date)
			=> SingleARSalesPriceMaint.CalculateSalesPriceInt(sender, custPriceClass, inventoryID, siteID, currencyinfo, UOM, date);

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
			=> CalculateSalesPrice(sender, custPriceClass, customerID, inventoryID, null, currencyinfo, UOM, quantity, date, currentUnitPrice);

		/// <summary>
		/// Calculates Sales Price.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="siteID">Warehouse</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <returns>Sales Price.</returns>
		/// <remarks>AlwaysFromBaseCury flag in the SOSetup is considered when performing the calculation.</remarks>
		public static decimal? CalculateSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitPrice)
			=> SingleARSalesPriceMaint.CalculateSalesPriceInt(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo, UOM, quantity, date, currentUnitPrice);

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
			=> CalculateSalesPrice(sender, custPriceClass, inventoryID, null, currencyinfo, UOM, date, alwaysFromBaseCurrency);

		/// <summary>
		/// Calculates Sales Price.
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="inventoryID">Inventory</param>
		/// <param name="siteID">Warehouse</param>
		/// <param name="curyID">Currency</param>
		/// <param name="UOM">Unit of measure</param>
		/// <param name="date">Date</param>
		/// <param name="alwaysFromBaseCurrency">If true sales price is always calculated (converted) from Base Currency.</param>
		/// <returns>Sales Price.</returns>
		public static decimal? CalculateSalesPrice(PXCache sender, string custPriceClass, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, string UOM, DateTime date, bool alwaysFromBaseCurrency)
			=> SingleARSalesPriceMaint.CalculateSalesPriceInt(sender, custPriceClass, inventoryID, siteID, currencyinfo, UOM, date, alwaysFromBaseCurrency);

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
			=> SingleARSalesPriceMaint.CalculateSalesPriceInt(sender, custPriceClass, customerID, inventoryID, null, currencyinfo, quantity, UOM, date, alwaysFromBaseCurrency);

		/// <summary>
		/// Calculates Fair Value Sales Price.
		/// </summary>
		public static SalesPriceItem CalculateFairValueSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, CurrencyInfo currencyinfo, decimal? quantity, string UOM, DateTime date, bool alwaysFromBaseCurrency)
		{
			var spItem = SingleARSalesPriceMaint.CalculateSalesPriceItem(sender, custPriceClass, customerID, inventoryID, null, currencyinfo, quantity, UOM, date, alwaysFromBaseCurrency, true);

			var newPrice = SingleARSalesPriceMaint.AdjustSalesPrice(sender, spItem, inventoryID, currencyinfo, UOM);

			return new SalesPriceItem(spItem?.UOM, newPrice ?? 0m, spItem?.CuryID, spItem?.PriceType, spItem?.Prorated ?? false);
		}
		#endregion
		#region CalculateSalesPrice - Virtual calls
		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? inventoryID, CurrencyInfo currencyinfo, string UOM, DateTime date)
		{
			return CalculateSalesPriceInt(sender, custPriceClass, inventoryID, null, currencyinfo, UOM, date);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, string UOM, DateTime date)
		{
			bool alwaysFromBase = GetAlwaysFromBaseCurrencySetting(sender);
            return CalculateSalesPriceInt(sender, custPriceClass, null, inventoryID, siteID, currencyinfo, 0m, UOM, date, alwaysFromBase);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitPrice)
		{
			return CalculateSalesPriceInt(sender, custPriceClass, customerID, inventoryID, null, currencyinfo, UOM, quantity, date, currentUnitPrice);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, string UOM, decimal? quantity, DateTime date, decimal? currentUnitPrice)
		{
            bool alwaysFromBase = GetAlwaysFromBaseCurrencySetting(sender);
            decimal? salesPrice = CalculateSalesPriceInt(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo, Math.Abs(quantity ?? 0m), UOM, date, alwaysFromBase);
			return salesPrice ?? 0;
		}

		public virtual bool GetAlwaysFromBaseCurrencySetting(PXCache sender)
        {
            ARSetup arsetup = (ARSetup)sender.Graph.Caches[typeof(ARSetup)].Current ?? PXSelect<ARSetup>.Select(sender.Graph);

            return arsetup != null 
                ? arsetup.AlwaysFromBaseCury == true
                : false;          
        }

		public static void CheckNewUnitPrice<Line, Field>(PXCache sender, Line row, object newValue)
			where Line : class, IBqlTable, new()
			where Field : class, IBqlField

		{
			if (newValue != null && (decimal)newValue != 0m)
				PXUIFieldAttribute.SetWarning<Field>(sender, row, null);
			else
				PXUIFieldAttribute.SetWarning<Field>(sender, row, Messages.NoUnitPriceFound);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? inventoryID, CurrencyInfo currencyinfo, string UOM, DateTime date, bool alwaysFromBaseCurrency)
		{
			return CalculateSalesPriceInt(sender, custPriceClass, inventoryID, null, currencyinfo, UOM, date, alwaysFromBaseCurrency);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, string UOM, DateTime date, bool alwaysFromBaseCurrency)
		{
			return CalculateSalesPriceInt(sender, custPriceClass, null, inventoryID, siteID, currencyinfo, 0m, UOM, date, alwaysFromBaseCurrency);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, CurrencyInfo currencyinfo, decimal? quantity, string UOM, DateTime date, bool alwaysFromBaseCurrency)
		{
			return CalculateSalesPriceInt(sender, custPriceClass, customerID, inventoryID, null, currencyinfo, quantity, UOM, date, alwaysFromBaseCurrency);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, decimal? quantity, string UOM, DateTime date, bool alwaysFromBaseCurrency)
		{
			return CalculateSalesPriceInt(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo, quantity, UOM, date, alwaysFromBaseCurrency, false);
		}

		public virtual decimal? CalculateSalesPriceInt(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, decimal? quantity, string UOM, DateTime date, bool alwaysFromBaseCurrency, bool isFairValue)
		{
			//InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, inventoryID);
			SalesPriceItem spItem = CalculateSalesPriceItem(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo, quantity, UOM, date, alwaysFromBaseCurrency, isFairValue);

			return AdjustSalesPrice(sender, spItem, inventoryID, currencyinfo, UOM);
		}

		public virtual SalesPriceItem CalculateSalesPriceItem(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, CurrencyInfo currencyinfo, decimal? quantity, string UOM, DateTime date, bool alwaysFromBaseCurrency, bool isFairValue)
		{
			//InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, inventoryID);
			SalesPriceItem spItem;
			try
			{
				spItem = FindSalesPrice(sender, custPriceClass, customerID, inventoryID, siteID, currencyinfo.BaseCuryID, alwaysFromBaseCurrency ? currencyinfo.BaseCuryID : currencyinfo.CuryID, Math.Abs(quantity ?? 0m), UOM, date, isFairValue);
			}
			catch (PXUnitConversionException)
			{
				return null;
			}

			return spItem;
		}

		public virtual decimal? AdjustSalesPrice(PXCache sender, SalesPriceItem spItem, int? inventoryID, CurrencyInfo currencyinfo, string uom)
        {
            if (spItem == null || uom == null)
            {
                return null;
            }

            decimal salesPrice = spItem.Price;

            if (spItem.CuryID != currencyinfo.CuryID)
            {
                if (currencyinfo.CuryRate == null)
                    throw new PXSetPropertyException(CM.Messages.RateNotFound, PXErrorLevel.Warning);

                PXCurrencyAttribute.CuryConvCury(sender, currencyinfo, spItem.Price, out salesPrice);
            }
        
            if (spItem.UOM != uom)
            {
                decimal salesPriceInBase = INUnitAttribute.ConvertFromBase(sender, inventoryID, spItem.UOM, salesPrice, INPrecision.UNITCOST);
                salesPrice = INUnitAttribute.ConvertToBase(sender, inventoryID, uom, salesPriceInBase, INPrecision.UNITCOST);
            }

            return salesPrice;
        }
        #endregion

        public virtual SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? inventoryID, string curyID, string UOM, DateTime date)
		{
			return FindSalesPrice(sender, custPriceClass, inventoryID, null, curyID, UOM, date);
		}

		public virtual SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? inventoryID, int? siteID, string curyID, string UOM, DateTime date)
		{
			return FindSalesPrice(sender, custPriceClass, null, inventoryID, siteID, new PXSetup<Company>(sender.Graph).Current.BaseCuryID, curyID, 0m, UOM, date);
		}

		public virtual SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date)
		{
			return FindSalesPrice(sender, custPriceClass, customerID, inventoryID, null, baseCuryID, curyID, quantity, UOM, date);
		}

		public virtual SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date)
		{
			return FindSalesPrice(sender, custPriceClass, customerID, inventoryID, siteID, baseCuryID, curyID, quantity, UOM, date, false);
		}

		public virtual SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date, bool isFairValue)
		{
			var priceListExists = RecordExistsSlot<ARSalesPrice, ARSalesPrice.recordID>.IsRowsExists();
			if (!priceListExists)
				return isFairValue ? null : SelectDefaultItemPrice(sender, inventoryID, baseCuryID);
		
			var salesPriceSelect =
				new SalesPriceSelect(sender, inventoryID, UOM, (decimal) quantity, isFairValue)
				{
					CustomerID = customerID,
					CustPriceClass = custPriceClass,
					CuryID = curyID,
					SiteID = siteID,
					Date = date,
				};

			SalesPriceForCurrentUOM priceForCurrentUOM = salesPriceSelect.ForCurrentUOM();
			SalesPriceForBaseUOM priceForBaseUOM = salesPriceSelect.ForBaseUOM();
			SalesPriceForSalesUOM priceForSalesUOM = salesPriceSelect.ForSalesUOM();

			return priceForCurrentUOM.SelectCustomerPrice()
				?? priceForBaseUOM.SelectCustomerPrice()
				?? priceForCurrentUOM.SelectBasePrice()
				?? priceForBaseUOM.SelectBasePrice()
				?? (isFairValue ? null : SelectDefaultItemPrice(sender, inventoryID, baseCuryID))
				?? priceForSalesUOM.SelectCustomerPrice()
				?? priceForSalesUOM.SelectBasePrice();
		}

        /// <summary>
        /// Creates <see cref="SalesPriceItem"/> data container from sales price. Used by PriceUnit customization for Lexware.
        /// </summary>
        /// <param name="salesPrice">The sales price.</param>
        /// <param name="uom">The unit of measure.</param>
        /// <returns/>      
        protected virtual SalesPriceItem CreateSalesPriceItemFromSalesPrice(ARSalesPrice salesPrice, string uom)
        {
            if (salesPrice == null)
                return null;

            return new SalesPriceItem(uom ?? salesPrice.UOM, salesPrice.SalesPrice ?? 0, salesPrice.CuryID, salesPrice.PriceType, salesPrice.IsProrated ?? false)
			{
				IsPromotionalPrice = salesPrice.IsPromotionalPrice.GetValueOrDefault()
			};
        }

        /// <summary>
        /// Creates default <see cref="SalesPriceItem" /> data container from <see cref="InventoryItem" />. Used by PriceUnit customization for Lexware.
        /// </summary>
        /// <param name="inventoryItem">The inventory item.</param>
        /// <param name="baseCuryID">The base cury identifier.</param>
        /// <returns/>
        protected virtual SalesPriceItem CreateDefaultSalesPriceItemFromInventoryItem(InventoryItem inventoryItem, string baseCuryID)
        {
            if (inventoryItem?.BasePrice == null || inventoryItem.BasePrice == 0)
            {
                return null;
            }

            return new SalesPriceItem(inventoryItem.BaseUnit, inventoryItem.BasePrice.Value, baseCuryID);
        }

		internal virtual SalesPriceItem SelectDefaultItemPrice(PXCache sender, int? inventoryID, string baseCuryID)
		{
			InventoryItem inventoryItem = InventoryItem.PK.Find(sender.Graph, inventoryID);
			return SingleARSalesPriceMaint.CreateDefaultSalesPriceItemFromInventoryItem(inventoryItem, baseCuryID);
		}

		internal class SalesPriceSelect
		{
			protected PXSelectBase<ARSalesPrice> SelectCommand;
			protected readonly PXCache Cache;

			public int? InventoryID { get; }
			public string UOM { get; }
			public decimal Qty { get; }

			public int? CustomerID { get; set; }
			public string CustPriceClass { get; set; }
			public string CuryID { get; set; }
			public int? SiteID { get; set; }
			public DateTime Date { get; set; }
			public bool IsFairValue { get; set; }

			public SalesPriceSelect(PXCache cache, int? inventoryID, string uom, decimal qty)
				: this(cache, inventoryID, uom, qty, false) { }

			public SalesPriceSelect(PXCache cache, int? inventoryID, string uom, decimal qty, bool isFairValue)
			{
				Cache = cache;

				Qty = qty;
				InventoryID = inventoryID;
				UOM = uom;
				IsFairValue = isFairValue;

				SelectCommand =
					new PXSelect<ARSalesPrice,
					Where<
						ARSalesPrice.inventoryID, In<Required<ARSalesPrice.inventoryID>>,
						And<ARSalesPrice.isFairValue, Equal<Required<ARSalesPrice.isFairValue>>,
						And2<
							Where2<Where<ARSalesPrice.priceType, Equal<PriceTypes.customer>,
							And<ARSalesPrice.customerID, Equal<Required<ARSalesPrice.customerID>>>>,
								Or2<Where<ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>,
								And<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>>>,
									Or<Where<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>,
									And<Required<ARSalesPrice.customerID>, IsNull,
									And<Required<ARSalesPrice.custPriceClassID>, IsNull>>>>>>,
						And<ARSalesPrice.curyID, Equal<Required<ARSalesPrice.curyID>>,
						And2<Where<ARSalesPrice.siteID, Equal<Required<ARSalesPrice.siteID>>, Or<ARSalesPrice.siteID, IsNull>>,
						And<Where2<Where<ARSalesPrice.breakQty, LessEqual<Required<ARSalesPrice.breakQty>>>,

						And<Where2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
						And<ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPrice.expirationDate>>>>,
							Or2<Where<ARSalesPrice.effectiveDate, LessEqual<Required<ARSalesPrice.effectiveDate>>,
						And<ARSalesPrice.expirationDate, IsNull>>,
							Or<Where<ARSalesPrice.expirationDate, GreaterEqual<Required<ARSalesPrice.expirationDate>>,
						And<ARSalesPrice.effectiveDate, IsNull,
							Or<ARSalesPrice.effectiveDate, IsNull, And<ARSalesPrice.expirationDate, IsNull>>>>>>>>>>>>>>>,
					OrderBy<Asc<ARSalesPrice.priceType,
							Desc<ARSalesPrice.isPromotionalPrice,
							Desc<ARSalesPrice.siteID,
							Desc<ARSalesPrice.breakQty>>>>>>(cache.Graph);
			}

			protected ARSalesPrice SelectCustomerPrice(decimal quantity, params object[] additionalParams)
			{
				return SelectCommand
					.SelectSingle(
						new object[] { GetInventoryIDs(Cache, InventoryID), IsFairValue, CustomerID, CustPriceClass, CustomerID, CustPriceClass, CuryID, SiteID, quantity, Date, Date, Date, Date }
							.Concat(additionalParams)
							.ToArray());
			}

			protected ARSalesPrice SelectBasePrice(decimal quantity, params object[] additionalParams)
			{
				return SelectCommand
					.SelectSingle(
						new object[] { GetInventoryIDs(Cache, InventoryID), IsFairValue, CustomerID, CustPriceClass, null, null, CuryID, SiteID, quantity, Date, Date, Date, Date }
							.Concat(additionalParams)
							.ToArray());
			}

			/// <summary>
			/// This function is intended for customization purposes and allows to create an array of InventoryIDs to be selected. 
			/// </summary>
			/// <param name="inventoryID">Original InventoryID</param>
			public virtual int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
			{
				return new int?[] { inventoryID };
			}

			protected decimal ConvertToBaseUOM() => INUnitAttribute.ConvertToBase(Cache, InventoryID, UOM, Qty, INPrecision.QUANTITY);

			#region Factories
			public SalesPriceForCurrentUOM ForCurrentUOM() => new SalesPriceForCurrentUOM(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue };
			public SalesPriceForBaseUOM ForBaseUOM() => new SalesPriceForBaseUOM(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue };
			public SalesPriceForSalesUOM ForSalesUOM() => new SalesPriceForSalesUOM(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue }; 
			#endregion
		}

		internal class SalesPriceForCurrentUOM : SalesPriceSelect
		{
			public SalesPriceForCurrentUOM(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				SelectCommand.WhereAnd<Where<ARSalesPrice.uOM, Equal<Required<ARSalesPrice.uOM>>>>(); // additional parameter
			}

			public SalesPriceItem SelectCustomerPrice() => SelectCustomerPrice(Qty, additionalParams: UOM).With(p => SalesPriceItem.FromPrice(p, UOM));
			public SalesPriceItem SelectBasePrice() => SelectBasePrice(Qty, additionalParams: UOM).With(p => SalesPriceItem.FromPrice(p, UOM));
		}

		internal class SalesPriceForBaseUOM : SalesPriceSelect
		{
			private readonly Lazy<decimal> _baseUnitQty;
			public decimal BaseUnitQty => _baseUnitQty.Value;

			public SalesPriceForBaseUOM(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				_baseUnitQty = new Lazy<decimal>(ConvertToBaseUOM);
				SelectCommand.Join<
					InnerJoin<InventoryItem,
						On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>,
						And<InventoryItem.baseUnit, Equal<ARSalesPrice.uOM>>>>>();
			}

			public SalesPriceItem SelectCustomerPrice() => SelectCustomerPrice(BaseUnitQty).With(p => SalesPriceItem.FromPrice(p));
			public SalesPriceItem SelectBasePrice() => SelectBasePrice(BaseUnitQty).With(p => SalesPriceItem.FromPrice(p));
		}

		internal class SalesPriceForSalesUOM : SalesPriceSelect
		{
			private readonly bool _usePriceAdjustmentMultiplier;

			private decimal? _baseUnitQty;
			public decimal BaseUnitQty
			{
				get { return _baseUnitQty ?? (_baseUnitQty = ConvertToBaseUOM()).Value; }
				set { _baseUnitQty = value; }
			}

			private readonly Lazy<Tuple<decimal, string>> _salesInfo;
			private Tuple<decimal, string> GetSalesInfo()
			{
				InventoryItem inventoryItem = InventoryItem.PK.Find(Cache.Graph, InventoryID);
				decimal salesUnitQty = INUnitAttribute.ConvertFromBase(Cache, InventoryID, inventoryItem.SalesUnit, BaseUnitQty, INPrecision.QUANTITY);
				return Tuple.Create(salesUnitQty, inventoryItem.SalesUnit);
			}

			public decimal SalesUnitQty => _salesInfo.Value.Item1;
			public string SalesUOM => _salesInfo.Value.Item2;

			public SalesPriceForSalesUOM(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				_salesInfo = new Lazy<Tuple<decimal, string>>(GetSalesInfo);
				SelectCommand.Join<
					InnerJoin<InventoryItem,
						On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>,
						And<InventoryItem.salesUnit, Equal<ARSalesPrice.uOM>>>>>();

				_usePriceAdjustmentMultiplier = INUnitAttribute.UsePriceAdjustmentMultiplier(cache.Graph);
			}

			public SalesPriceItem SelectCustomerPrice() => _usePriceAdjustmentMultiplier ? SelectCustomerPrice(SalesUnitQty).With(p => SalesPriceItem.FromPrice(p, SalesUOM)) : null;
			public SalesPriceItem SelectBasePrice() => _usePriceAdjustmentMultiplier ? SelectBasePrice(SalesUnitQty).With(p => SalesPriceItem.FromPrice(p, SalesUOM)) : null;
		}

		public class SalesPriceItem
		{
			public string UOM { get; }

			public decimal Price { get; }

			public string CuryID { get; }

			public string PriceType { get; }

			public bool IsPromotionalPrice { get; internal set; }

			public bool Prorated { get; }

			public SalesPriceItem(string uom, decimal price, string curyid, string priceType = null, bool prorated = false)
			{
				UOM = uom;
				Price = price;
				CuryID = curyid;
				PriceType = priceType;
				Prorated = prorated;
			}

            public static SalesPriceItem FromPrice(ARSalesPrice price, string uom = null)
                => SingleARSalesPriceMaint.CreateSalesPriceItemFromSalesPrice(price, uom);  //Calls AR Price singleton to allow extension of SalesPriceItem in customizations            
        }

		#endregion
	}

	public sealed class CustomerPriceClassAttribute : PXSelectorAttribute
	{
		public CustomerPriceClassAttribute() : base(typeof(ARPriceClass.priceClassID))
		{
			DescriptionField = typeof(ARPriceClass.description);
		}
	}

	[Serializable]
	public partial class ARSalesPriceFilter : IBqlTable
	{
		#region PriceType

		public abstract class priceType : PX.Data.BQL.BqlString.Field<priceType> { }

		protected String _PriceType;

		[PXDBString(1, IsFixed = true)]
		[PXDefault(PriceTypes.AllPrices)]
		[PriceTypes.ListWithAll]
		[PXUIField(DisplayName = "Price Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String PriceType
		{
			get { return this._PriceType; }
			set { this._PriceType = value; }
		}

		#endregion
		#region PriceCode

		public abstract class priceCode : PX.Data.BQL.BqlString.Field<priceCode> { }

		protected String _PriceCode;

		[PXDBString(30, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Price Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPriceCodeSelector(typeof(ARSalesPrice.priceCode), new Type[] {typeof(ARSalesPrice.priceCode), typeof(ARSalesPrice.description)},
			ValidateValue = false)]
		public virtual String PriceCode
		{
			get { return this._PriceCode; }
			set { this._PriceCode = value; }
		}

		#endregion
		#region InventoryID

		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected Int32? _InventoryID;

		[InventoryIncludingTemplates(DisplayName = "Inventory ID")]
		public virtual Int32? InventoryID
		{
			get { return this._InventoryID; }
			set { this._InventoryID = value; }
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
			get { return this._EffectiveAsOfDate; }
			set { this._EffectiveAsOfDate = value; }
		}

		#endregion
		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region InventoryPriceClassID
		public abstract class inventoryPriceClassID : PX.Data.BQL.BqlString.Field<inventoryPriceClassID> { }
		protected String _InventoryPriceClassID;
		[PXDBString(10)]
		[PXSelector(typeof(INPriceClass.priceClassID))]
		[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
		public virtual String InventoryPriceClassID
		{
			get { return this._InventoryPriceClassID; }
			set { this._InventoryPriceClassID = value; }
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
		[PXUIField(DisplayName = "Price Manager")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get { return (_MyOwner == true) ? CurrentOwnerID : _OwnerID; }
			set { _OwnerID = value; }
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
			get { return _MyOwner; }
			set { _MyOwner = value; }
		}

		#endregion
		#region WorkGroupID

		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }

		protected Int32? _WorkGroupID;

		[PXDBInt]
		[PXUIField(DisplayName = "Price Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
			SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get { return (_MyWorkGroup == true) ? null : _WorkGroupID; }
			set { _WorkGroupID = value; }
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
			get { return _MyWorkGroup; }
			set { _MyWorkGroup = value; }
		}

		#endregion
		#region FilterSet

		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }

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