using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.CS;
using Company = PX.Objects.GL.Company;

namespace PX.Objects.CT
{
	public class ContractItemMaint : PXGraph<ContractItemMaint, ContractItem>
	{
		public PXSelect<ContractItem> ContractItems;
		public PXSelect<ContractItem, Where<ContractItem.contractItemID, Equal<Current<ContractItem.contractItemID>>>> CurrentContractItem;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<ContractTemplate, 
			InnerJoin<ContractDetail, 
				On<ContractTemplate.contractID, Equal<ContractDetail.contractID>>>, 
			Where<ContractDetail.contractItemID, Equal<Current<ContractItem.contractItemID>>, 
				And<ContractTemplate.baseType, Equal<CTPRType.contractTemplate>>>> CurrentTemplates;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<Contract, 
			InnerJoin<ContractDetail, 
				On<Contract.contractID, Equal<ContractDetail.contractID>>>, 
			Where<ContractDetail.contractItemID, Equal<Current<ContractItem.contractItemID>>, 
				And<Contract.baseType, Equal<CTPRType.contract>, 
				And<Contract.status, NotEqual<Contract.status.canceled>, 
				And<Contract.status, NotEqual<Contract.status.expired>, 
				And<Contract.status, NotEqual<Contract.status.draft>>>>>>> CurrentContracts;

		public CMSetupSelect CMSetup;
		public PXSetup<Company> Company;

		public ContractItemMaint()
		{
			CurrentTemplates.Cache.AllowInsert =
			CurrentTemplates.Cache.AllowUpdate =
			CurrentTemplates.Cache.AllowDelete = false;
			CurrentContracts.Cache.AllowInsert =
			CurrentContracts.Cache.AllowUpdate =
			CurrentContracts.Cache.AllowDelete = false;
			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });

			if (!PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				PXUIFieldAttribute.SetVisible<ContractItem.curyID>(ContractItems.Cache, null, false);
				FieldDefaulting.AddHandler<ContractItem.curyID>((sender, e) =>
				{
					e.NewValue = Company.Current.BaseCuryID;
					e.Cancel = true;
				});
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, NotEqual<True>>), CT.Messages.CannotUseKit)]
		protected virtual void ContractItem_BaseItemID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, NotEqual<True>>), CT.Messages.CannotUseKit)]
		protected virtual void ContractItem_RenewalItemID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, NotEqual<True>>), CT.Messages.CannotUseKit)]
		protected virtual void ContractItem_RecurringItemID_CacheAttached(PXCache sender) { }

		public virtual void ContractItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ContractItem row = (ContractItem)e.Row;
			if (row != null)
			{
				SetControlsState(cache, row);
				ContractDetail detail = PXSelect<ContractDetail, Where<ContractDetail.contractItemID, Equal<Current<ContractItem.contractItemID>>>>.SelectWindowed(this, 0, 1);
				ContractItems.Cache.AllowDelete = detail == null ;
				PXUIFieldAttribute.SetEnabled<ContractItem.curyID>(cache, row, detail == null);

				PXDefaultAttribute.SetPersistingCheck<ContractItem.recurringItemID>(cache, row, row.RecurringType == RecurringOption.None ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
				PXUIFieldAttribute.SetRequired<ContractItem.recurringItemID>(cache, row.RecurringType != RecurringOption.None);
				string message = null;
				if (row.BaseItemID != null)
				{
					PXUIFieldAttribute.SetWarning<ContractItem.baseItemID>(cache, row, !IsValidBasePrice(this, row, ref message) ? Messages.ItemNotPrice : null);
				}
				if (row.RecurringItemID != null)
				{
					PXUIFieldAttribute.SetWarning<ContractItem.recurringItemID>(cache, row, !IsValidRecurringPrice(this, row, ref message) ? Messages.ItemNotPrice : null);
				}
				if (row.RenewalItemID != null)
				{
					PXUIFieldAttribute.SetWarning<ContractItem.renewalItemID>(cache, row, !IsValidRenewalPrice(this, row, ref message) ? Messages.ItemNotPrice : null);
				}
			}
		}

		public virtual void ContractItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			ContractItem item = (ContractItem)e.Row;
			if (item != null)
			{
				string uomUsed = null;
				if (item.BaseItemID != null)
				{
					InventoryItem nonStock = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.BaseItemID);
					uomUsed = nonStock.BaseUnit;
				}
				if (item.RecurringItemID != null && item.RecurringType != RecurringOption.None)
				{
					InventoryItem nonStock = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RecurringItemID);
					if (uomUsed == null)
					{
						uomUsed = nonStock.BaseUnit;
					}
					else if (uomUsed != nonStock.BaseUnit)
					{
						cache.RaiseExceptionHandling<ContractItem.recurringItemID>(item, nonStock.InventoryCD, new PXSetPropertyException(Messages.ItemsUOMConflict));
					}
				}
				if (item.RenewalItemID != null)
				{
					InventoryItem nonStock = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, item.RenewalItemID);
					if (uomUsed == null)
					{
						uomUsed = nonStock.BaseUnit;
					}
					else if (uomUsed != nonStock.BaseUnit)
					{
						cache.RaiseExceptionHandling<ContractItem.renewalItemID>(item, nonStock.InventoryCD, new PXSetPropertyException(Messages.ItemsUOMConflict));
					}
				}
			}
		}

		public virtual void ContractItem_BasePriceOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem item = e.Row as ContractItem;

			if (item != null && item.BasePriceOption == PriceOption.ItemPercent && item.BasePrice == 0m)
			{
				sender.SetValueExt<ContractItem.basePrice>(e.Row, 100m);
			}
			if (item != null && item.BasePriceOption == PriceOption.ItemPrice)
			{
				sender.SetDefaultExt<ContractItem.basePrice>(item);
			}
		}

		public virtual void ContractItem_RenewalPriceOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem item = e.Row as ContractItem;

			if (item != null && item.RenewalPriceOption == PriceOption.ItemPercent && item.RenewalPrice == 0m)
			{
				sender.SetValueExt<ContractItem.renewalPrice>(e.Row, 100m);
			}

			if (item != null && item.RenewalPriceOption == PriceOption.ItemPrice)
			{
				sender.SetDefaultExt<ContractItem.renewalPrice>(item);
			}
		}

		public virtual void ContractItem_FixedRecurringPriceOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem item = e.Row as ContractItem;

			if (item != null && item.FixedRecurringPriceOption == PriceOption.ItemPercent && item.FixedRecurringPrice == 0m)
			{
				sender.SetValueExt<ContractItem.fixedRecurringPrice>(e.Row, 100m);
			}

			if (item != null && item.FixedRecurringPriceOption == PriceOption.ItemPrice)
			{
				sender.SetDefaultExt<ContractItem.fixedRecurringPrice>(item);
			}
		}

		public virtual void ContractItem_RecurringType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem item = e.Row as ContractItem;

			if (item != null && item.RecurringType == RecurringOption.None)
			{
				item.ResetUsageOnBilling = false;
				sender.SetDefaultExt<ContractItem.recurringItemID>(item);
				sender.SetDefaultExt<ContractItem.fixedRecurringPriceOption>(item);
				sender.SetDefaultExt<ContractItem.fixedRecurringPrice>(item);
				sender.SetDefaultExt<ContractItem.usagePriceOption>(item);
				sender.SetDefaultExt<ContractItem.usagePrice>(item);
				sender.SetDefaultExt<ContractItem.depositItemID>(item);
			}
		}


		public virtual void ContractItem_UsagePriceOption_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem item = e.Row as ContractItem;

			if (item != null && item.UsagePriceOption == PriceOption.ItemPercent && item.UsagePrice == 0m)
			{
				sender.SetValueExt<ContractItem.usagePrice>(e.Row, 100m);
			}

			if (item != null && item.UsagePriceOption == PriceOption.ItemPrice)
			{
				sender.SetDefaultExt<ContractItem.usagePrice>(item);
			}
		}

		public virtual void ContractItem_Deposit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem item = e.Row as ContractItem;

			if (item != null && item.Deposit == true)
			{
				sender.SetDefaultExt<ContractItem.recurringType>(item);
				sender.SetDefaultExt<ContractItem.recurringItemID>(item);
				sender.SetDefaultExt<ContractItem.resetUsageOnBilling>(item);
				sender.SetDefaultExt<ContractItem.fixedRecurringPriceOption>(item);
				sender.SetDefaultExt<ContractItem.fixedRecurringPrice>(item);
				sender.SetDefaultExt<ContractItem.usagePriceOption>(item);
				sender.SetDefaultExt<ContractItem.usagePrice>(item);
				sender.SetDefaultExt<ContractItem.depositItemID>(item);

				sender.SetDefaultExt<ContractItem.collectRenewFeeOnActivation>(item);
				sender.SetDefaultExt<ContractItem.renewalPriceOption>(item);
				sender.SetDefaultExt<ContractItem.renewalPrice>(item);
				sender.SetDefaultExt<ContractItem.renewalItemID>(item);
				sender.SetDefaultExt<ContractItem.recurringItemID>(item);
			}
		}

		public virtual void ContractItem_DepositItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem item = e.Row as ContractItem;

			if (item != null && item.DepositItemID != null)
			{
				sender.SetDefaultExt<ContractItem.resetUsageOnBilling>(item);
				sender.SetDefaultExt<ContractItem.maxQty>(item);
				sender.SetDefaultExt<ContractItem.minQty>(item);
				sender.SetDefaultExt<ContractItem.defaultQty>(item);
			}
		}
		public virtual void ContractItem_BaseItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem row = (ContractItem)e.Row;
			if (row != null)
			{
				if (row.BaseItemID == null)
				{
					sender.SetDefaultExt<ContractItem.basePriceOption>(row);
					sender.SetDefaultExt<ContractItem.basePrice>(row);
					sender.SetDefaultExt<ContractItem.deposit>(row);
					row.ProrateSetup = false;
				}
				SetValueRefundable(sender, row);
			}
		}

		public virtual void ContractItem_RenewalItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem row = (ContractItem)e.Row;
			if (row != null)
			{
				if (row.RenewalItemID == null)
				{
					sender.SetDefaultExt<ContractItem.renewalPriceOption>(row);
					sender.SetDefaultExt<ContractItem.renewalPrice>(row);
					row.CollectRenewFeeOnActivation = false;
				}
				SetValueRefundable(sender, row);
			}
		}

		public virtual void ContractItem_RecurringItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ContractItem row = (ContractItem)e.Row;
			if (row != null)
			{
				if (row.RecurringItemID == null)
				{
					sender.SetDefaultExt<ContractItem.fixedRecurringPriceOption>(row);
					sender.SetDefaultExt<ContractItem.fixedRecurringPrice>(row);
					sender.SetDefaultExt<ContractItem.usagePriceOption>(row);
					sender.SetDefaultExt<ContractItem.usagePrice>(row);
				}
				SetValueRefundable(sender, row);
			}
		}

		private void SetValueRefundable(PXCache sender, ContractItem item)
		{
			if (item.BaseItemID == null && item.RenewalItemID == null && item.RecurringItemID == null)
			{
				item.Refundable = false;
			}
		}

		public override int ExecuteUpdate(string viewName, System.Collections.IDictionary keys, System.Collections.IDictionary values, params object[] parameters)
		{
			if (viewName == "CurrentContractItem" && values != null)
			{
				values["BasePriceVal"] = PXCache.NotSetValue;
				values["RenewalPriceVal"] = PXCache.NotSetValue;
				values["FixedRecurringPriceVal"] = PXCache.NotSetValue;
				values["UsagePriceVal"] = PXCache.NotSetValue;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		private void SetControlsState(PXCache cache, ContractItem row)
		{
			ContractDetail activeDetail = PXSelectJoin<ContractDetail, InnerJoin<Contract, On<ContractDetail.contractID, Equal<Contract.contractID>>>,
					Where<ContractDetail.contractItemID, Equal<Required<ContractItem.contractItemID>>,
					And2<Where<Contract.status, Equal<Contract.status.active>, Or<Contract.status, Equal<Contract.status.inUpgrade>>>, 
					And<Contract.baseType, Equal<CTPRType.contract>>>>>.SelectWindowed(this, 0, 1, row.ContractItemID);
			bool used = activeDetail != null;

			PXUIFieldAttribute.SetEnabled<ContractItem.recurringType>(cache, row, !used);
			PXUIFieldAttribute.SetEnabled<ContractItem.recurringItemID>(cache, row, row.RecurringType != RecurringOption.None && !used && row.Deposit != true);

			PXUIFieldAttribute.SetEnabled<ContractItem.basePriceOption>(cache, row, row.BaseItemID != null);
			PXUIFieldAttribute.SetEnabled<ContractItem.basePrice>(cache, row, row.BaseItemID != null && row.BasePriceOption != "I");
			PXUIFieldAttribute.SetEnabled<ContractItem.renewalPriceOption>(cache, row, row.RenewalItemID != null);
			PXUIFieldAttribute.SetEnabled<ContractItem.renewalPrice>(cache, row, row.RenewalItemID != null && row.RenewalPriceOption != "I");
			PXUIFieldAttribute.SetEnabled<ContractItem.fixedRecurringPriceOption>(cache, row, row.RecurringType != RecurringOption.None && row.RecurringItemID != null && !used);
			PXUIFieldAttribute.SetEnabled<ContractItem.fixedRecurringPrice>(cache, row, row.RecurringType != RecurringOption.None && row.RecurringItemID != null && row.FixedRecurringPriceOption != "I" && !used);
			PXUIFieldAttribute.SetEnabled<ContractItem.usagePriceOption>(cache, row, row.RecurringType != RecurringOption.None && row.RecurringItemID != null);
			PXUIFieldAttribute.SetEnabled<ContractItem.usagePrice>(cache, row, row.RecurringType != RecurringOption.None && row.RecurringItemID != null && row.UsagePriceOption != "I");
			PXUIFieldAttribute.SetEnabled<ContractItem.depositItemID>(cache, row, row.RecurringType != RecurringOption.None);

			PXUIFieldAttribute.SetVisible<ContractItem.collectRenewFeeOnActivation>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.renewalPriceOption>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.renewalPrice>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.renewalItemID>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.recurringItemID>(cache, row, row.Deposit != true);

			PXUIFieldAttribute.SetVisible<ContractItem.recurringType>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.resetUsageOnBilling>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.fixedRecurringPriceOption>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.fixedRecurringPrice>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.usagePriceOption>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.usagePrice>(cache, row, row.Deposit != true);
			PXUIFieldAttribute.SetVisible<ContractItem.depositItemID>(cache, row, row.Deposit != true);

			PXUIFieldAttribute.SetEnabled<ContractItem.deposit>(cache, row, row.BaseItemID != null && !(used && row.RecurringItemID != null));
			PXUIFieldAttribute.SetEnabled<ContractItem.prorateSetup>(cache, row, row.BaseItemID != null);
			PXUIFieldAttribute.SetEnabled<ContractItem.collectRenewFeeOnActivation>(cache, row, row.RenewalItemID != null);

			PXUIFieldAttribute.SetEnabled<ContractItem.resetUsageOnBilling>(cache, row, row.RecurringType != RecurringOption.None && row.DepositItemID == null);
		}

		protected virtual void ValidateQuantity(ContractItem row, decimal? defaultQty)
		{
			decimal included = defaultQty ?? 0;
			decimal max = row.MaxQty ?? 0;
			decimal min = row.MinQty ?? 0;

			if (max < min || included < min || included > max)
			{
				throw new PXSetPropertyException(Messages.QtyError);
			}
		}

		public virtual void ContractItem_DefaultQty_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row != null && !this.IsImport)
			{
				ContractItem item = (ContractItem)e.Row;
				ValidateQuantity(item, (decimal?)e.NewValue);
				if (item.DepositItemID != null && (decimal) e.NewValue > 0m)
				{
					throw new PXSetPropertyException(Messages.QuantityUnderDeposit, typeof(ContractItem.defaultQty).Name);
				}
			}
		}
		
		public virtual void ContractItem_MaxQty_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			ContractItem item = (ContractItem)e.Row;
			if (item != null && !IsImport)
			{
				if (item.DepositItemID != null && (decimal) e.NewValue > 0m)
				{
					throw new PXSetPropertyException(Messages.QuantityUnderDeposit, typeof (ContractItem.maxQty).Name);
				}
			}
		}

		public virtual void ContractItem_MinQty_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			ContractItem item = (ContractItem)e.Row;
			if (item != null && !IsImport && item.DepositItemID != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(Messages.QuantityUnderDeposit, typeof(ContractItem.minQty).Name);
			}
		}

		public static bool IsValidItemPrice(PXGraph graph, ContractItem item, out string message)
		{
			message = null;
			return (IsValidBasePrice(graph, item, ref message) && IsValidRecurringPrice(graph, item, ref message) && IsValidRenewalPrice(graph, item, ref message));
		}

		public static bool IsValidItemPrice(PXGraph graph, ContractItem item)
		{
			string message = null;
			return (IsValidBasePrice(graph, item, ref message) && IsValidRecurringPrice(graph, item, ref message) && IsValidRenewalPrice(graph, item, ref message));
		}

		private static bool IsValidBasePrice(PXGraph graph, ContractItem item, ref string message)
		{
			if (item != null && item.BaseItemID != null && item.BasePriceVal == null)
			{
				message = Messages.SetupPriceMessage;
				return false;
			}
			else
				return true;
		}

		private static bool IsValidRecurringPrice(PXGraph graph, ContractItem item, ref string message)
		{
			if (item != null && item.RecurringItemID != null && (item.FixedRecurringPriceVal == null || item.UsagePriceVal == null))
			{
				message = Messages.RecurringPriceMessage;
				return false;
			}
			else
				return true;
		}

		private static bool IsValidRenewalPrice(PXGraph graph, ContractItem item, ref string message)
		{
			if (item != null && item.RenewalItemID != null && item.RenewalPriceVal == null)
			{
				message = Messages.RenewalPriceMessage;
				return false;
			}
			else
				return true;
		}
	}
}
