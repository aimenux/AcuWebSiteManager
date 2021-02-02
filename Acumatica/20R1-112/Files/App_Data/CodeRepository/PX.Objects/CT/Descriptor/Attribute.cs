using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.CT
{
    public class ContractLineNbr : PXLineNbrAttribute
    {
        public ContractLineNbr(Type sourceType)
            : base(sourceType)
        {
        }

        protected HashSet<object> _justInserted = null;

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            _justInserted = new HashSet<object>();
        }

        public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            base.RowInserted(sender, e);
            _justInserted.Add(e.Row);
        }

        public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            //the idea is not to decrement linecntr on deletion
            if (_justInserted.Contains(e.Row))
            {
                base.RowDeleted(sender, e);
                _justInserted.Remove(e.Row);
            }
        }
    }

    /// <summary>
    /// Contract Selector. Dispalys all contracts.
    /// </summary>
    [PXDBInt()]
    [PXUIField(DisplayName = "Contract", Visibility = PXUIVisibility.Visible)]
    public class ContractAttribute : AcctSubAttribute
    {
        public const string DimensionName = "CONTRACT";

        public ContractAttribute()
        {
            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(
                DimensionName,
                typeof(Search2<Contract.contractID, 
					InnerJoin<ContractBillingSchedule, 
						On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>>, 
					Where<Contract.baseType, Equal<CTPRType.contract>>>)
                , typeof(Contract.contractCD)
                , typeof(Contract.contractCD), typeof(Contract.customerID), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate));

            select.DescriptionField = typeof(Contract.description);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public ContractAttribute(Type WhereType)
        {
            Type SearchType =
                BqlCommand.Compose(
                typeof(Search2<,,,>),
                typeof(Contract.contractID),
                typeof(InnerJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>>),
                typeof(Where<,,>),
                typeof(Contract.baseType),
                typeof(Equal<>),
                typeof(CTPRType.contract),
                typeof(And<>),
                WhereType,
                typeof(OrderBy<Desc<Contract.contractCD>>));

            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Contract.contractCD),
                typeof(Contract.contractCD), typeof(Contract.customerID), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate));

            select.DescriptionField = typeof(Contract.description);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }
    }

    /// <summary>
    /// Contract Template Selector. Displays all Contract Templates.
    /// </summary>
    [PXDBInt()]
    [PXUIField(DisplayName = "Contract Template", Visibility = PXUIVisibility.Visible)]
    public class ContractTemplateAttribute : AcctSubAttribute
    {
        public const string DimensionName = "TMCONTRACT";

        public ContractTemplateAttribute()
        {
            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(
                DimensionName,
                typeof(Search<ContractTemplate.contractID, 
				Where<ContractTemplate.baseType, Equal<CTPRType.contractTemplate>>>)
                , typeof(ContractTemplate.contractCD)
                , typeof(ContractTemplate.contractCD), typeof(ContractTemplate.description), typeof(ContractTemplate.status));

            select.DescriptionField = typeof(ContractTemplate.description);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public ContractTemplateAttribute(Type WhereType)
        {
            Type SearchType =
                BqlCommand.Compose(
                typeof(Search<,>),
                typeof(ContractTemplate.contractID),
                typeof(Where<,,>),
                typeof(ContractTemplate.baseType),
                typeof(Equal<>),
                typeof(CTPRType.contractTemplate),
                typeof(And<>),
                WhereType);

            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(ContractTemplate.contractCD),
                typeof(ContractTemplate.contractCD), typeof(ContractTemplate.description), typeof(ContractTemplate.status));

            select.DescriptionField = typeof(ContractTemplate.description);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }
    }

    /// <summary>
    /// Contract Item Selector. Displays all Contract Items.
    /// </summary>
    [PXDBString(InputMask = "", IsUnicode = true)]
    [PXUIField(DisplayName = "Contract Item", Visibility = PXUIVisibility.Visible)]
    public class ContractItemAttribute : AcctSubAttribute
    {
        public const string DimensionName = "CONTRACTITEM";

        public ContractItemAttribute()
        {
            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(
                DimensionName,
                typeof(Search<ContractItem.contractItemCD>)
                , typeof(ContractItem.contractItemCD)
                , typeof(ContractItem.contractItemCD), typeof(ContractItem.descr), typeof(ContractItem.baseItemID));

            select.DescriptionField = typeof(ContractItem.descr);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public ContractItemAttribute(Type WhereType)
        {
            Type SearchType =
                BqlCommand.Compose(
                typeof(Search<,,>),
                typeof(ContractItem.contractItemCD),
                WhereType,
                typeof(OrderBy<Desc<ContractItem.contractItemCD>>));

            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType,
                typeof(ContractItem.contractItemCD)
                , typeof(ContractItem.contractItemCD), typeof(ContractItem.descr), typeof(ContractItem.baseItemID));

            select.DescriptionField = typeof(ContractItem.descr);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }
    }


	[PXDBInt()]
	[PXUIField(DisplayName = "Contract Item")]
	[PXRestrictor(typeof(Where<InventoryItem.stkItem, NotEqual<True>>), Messages.ContractInventoryItemCantBeStock, typeof(InventoryItem.inventoryCD))]
	[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>), Messages.ContractInventoryItemCantBeUnknown, typeof(InventoryItem.inventoryCD))]
	[PXRestrictor(typeof(Where<InventoryItem.isTemplate, Equal<False>>), IN.Messages.InventoryItemIsATemplate)]
	public class ContractInventoryItemAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INVENTORY";

		public ContractInventoryItemAttribute()
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search5<,,>),
				typeof(InventoryItem.inventoryID),
				typeof(LeftJoin<ARSalesPrice, On<ARSalesPrice.inventoryID, Equal<InventoryItem.inventoryID>,
					And<ARSalesPrice.uOM, Equal<InventoryItem.baseUnit>,
					And<ARSalesPrice.curyID, Equal<Current<ContractItem.curyID>>,
					And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>,
					And<ARSalesPrice.breakQty, Equal<decimal0>,
					And<ARSalesPrice.isPromotionalPrice, Equal<False>,
					And<ARSalesPrice.isFairValue, Equal<False>>>>>>>>>),
				typeof(Aggregate<GroupBy<InventoryItem.inventoryID,
					GroupBy<InventoryItem.stkItem>>>));

			 PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(
				DimensionName,
				SearchType,
				typeof(InventoryItem.inventoryCD)
				);

			select.DescriptionField = typeof(InventoryItem.descr);
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	public class ContractDetailAccumAttribute : PXAccumulatorAttribute
    {
        public ContractDetailAccumAttribute()
        {
            base._SingleRecord = true;
        }
        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            columns.UpdateOnly = true;

            ContractDetailAcum item = (ContractDetailAcum)row;
            columns.Update<ContractDetailAcum.used>(item.Used, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ContractDetailAcum.usedTotal>(item.UsedTotal, PXDataFieldAssign.AssignBehavior.Summarize);

            //columns.Update<ContractDetailAcum.contractID>(item.ContractID, PXDataFieldAssign.AssignBehavior.Initialize);
            //columns.Update<ContractDetailAcum.inventoryID>(item.InventoryID, PXDataFieldAssign.AssignBehavior.Initialize);

            return true;
        }
    }

    public static class GroupType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Contract },
                new string[] { Messages.AttributeEntity_Contract }) { }
        }
        public const string Contract = "CONTRACT";

        public class ContractType : PX.Data.BQL.BqlString.Constant<ContractType>
		{
            public ContractType() : base(GroupType.Contract) { }
        }

    }
	#region GetItemPriceValue
	public class GetItemPriceValue<ContractID, ContractItemID, ItemType, ItemPriceType, ItemID, FixedPrice, SetupPrice, Qty, PriceDate> : BqlFormulaEvaluator<ContractID, ContractItemID, ItemType, ItemPriceType, ItemID, FixedPrice, SetupPrice, Qty, PriceDate>
		where ContractID : IBqlOperand
		where ContractItemID : IBqlOperand
		where ItemType : IBqlOperand
		where ItemPriceType : IBqlOperand
		where ItemID : IBqlOperand
		where FixedPrice : IBqlOperand
		where SetupPrice : IBqlOperand
		where Qty : IBqlOperand
		where PriceDate : IBqlOperand
	{

		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			int? contractID = (int?)pars[typeof(ContractID)];
			string priceOption = (string)pars[typeof(ItemPriceType)];
			string itemType = (string)pars[typeof(ItemType)];
			int? contractItemID = (int?)pars[typeof(ContractItemID)];
			int? itemID = (int?)pars[typeof(ItemID)];
			decimal? fixedPrice = (decimal?)pars[typeof(FixedPrice)];
			decimal? setupPrice = (decimal?)pars[typeof(SetupPrice)];
			decimal? qty = (decimal?)pars[typeof(Qty)];
			DateTime? date = (DateTime?)pars[typeof(PriceDate)];

			// TODO: modify this part.
			// -
			PXResult<Contract, ContractBillingSchedule> customerContract = (PXResult<Contract, ContractBillingSchedule>)PXSelectJoin<
				Contract, 
					LeftJoin<ContractBillingSchedule, 
						On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>>, 
				Where<
					Contract.contractID, Equal<Required<Contract.contractID>>>>
				.Select(cache.Graph, contractID);

			if (customerContract == null) return null;

				Contract contract = customerContract;
				ContractBillingSchedule billingSchedule = customerContract;

				return GetItemPrice(cache, 
					contract.CuryID, 
					billingSchedule.AccountID ?? contract.CustomerID, 
					billingSchedule.LocationID ?? contract.LocationID, 
					contract.Status, 
					contractItemID, 
					itemID, 
					itemType, 
					priceOption, 
					fixedPrice, 
					setupPrice, 
					qty,
					date);
			}

		public virtual decimal GetItemPrice(PXCache sender, string curyID, int? customerID, int? locationID, string contractStatus, int? contractItemID, int? itemID, string itemType, string priceOption, decimal? fixedPrice, decimal? setupPrice, decimal? qty, DateTime? date)
		{
			decimal itemPrice = 0m;
			ContractItem item = PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Required<ContractItem.contractItemID>>>>.Select(sender.Graph, contractItemID);

			if (item != null)
			{
				IN.InventoryItem nonstock = PXSelect<IN.InventoryItem, Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(sender.Graph, itemID);

				CR.Location customerLocation = PXSelect<
					CR.Location, 
					Where<
						CR.Location.bAccountID, Equal<Required<Contract.customerID>>, 
						And<CR.Location.locationID, Equal<Required<Contract.locationID>>>>>
					.Select(sender.Graph, customerID, locationID);

				string customerPriceClass = string.IsNullOrEmpty(customerLocation?.CPriceClassID)
					? ARPriceClass.EmptyPriceClass
					: customerLocation.CPriceClassID;

				if (priceOption == null)
				{
					switch (itemType)
					{
						case ContractDetailType.Setup:
							priceOption = item.BasePriceOption;
							break;
						case ContractDetailType.Renewal:
							priceOption = item.RenewalPriceOption;
							break;
						case ContractDetailType.Billing:
							priceOption = item.FixedRecurringPriceOption;
							break;
						case ContractDetailType.UsagePrice:
							priceOption = item.UsagePriceOption;
							break;
					}
				}

				CM.CurrencyInfo currencyInfo = new CM.CurrencyInfo();
				currencyInfo.BaseCuryID = new PXSetup<Company>(sender.Graph).Current.BaseCuryID;
				currencyInfo.CuryID = curyID;
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(sender.Graph, customerID);
				if (customer != null && customer.CuryRateTypeID != null)
					currencyInfo.CuryRateTypeID = customer.CuryRateTypeID;

				currencyInfo.SetCuryEffDate(sender.Graph.Caches[typeof(CM.CurrencyInfo)], date);

				if (nonstock != null && currencyInfo != null)
				{
					switch (priceOption)
					{
						case PriceOption.ItemPrice:
							itemPrice = ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass, customerID, itemID, currencyInfo, qty, nonstock.BaseUnit, date ?? DateTime.Now, false) ?? 0m;
							break;
						case PriceOption.ItemPercent:
							itemPrice = ((ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass, customerID, itemID, currencyInfo, qty, nonstock.BaseUnit, date ?? DateTime.Now, false) ?? 0m) / 100m * (fixedPrice ?? 0m));
							break;
						case PriceOption.BasePercent:
							itemPrice = (setupPrice ?? 0m) / 100m * (fixedPrice ?? 0m);
							break;
						case PriceOption.Manually:
							itemPrice = fixedPrice ?? 0m;
							break;
					}
				}
			}
			return itemPrice;
		}
	}
	#endregion
}
