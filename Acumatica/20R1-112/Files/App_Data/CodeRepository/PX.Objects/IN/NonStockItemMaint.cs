using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PX.Api.Models;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.RUTROT;
using PX.Objects.SO;
using PX.Objects.Common.Discount;
using PX.SM;
using PX.Web.UI;
using CRLocation = PX.Objects.CR.Standalone.Location;
using ItemStats = PX.Objects.IN.Overrides.INDocumentRelease.ItemStats;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;

namespace PX.Objects.IN
{
	public class NonStockItemMaint : InventoryItemMaintBase
	{
		public override bool IsStockItemFlag => false;

		public NonStockItemMaint()
		{
			Item.View = new PXView(this, false, new Select<InventoryItem,
				Where<InventoryItem.stkItem, Equal<False>,
				And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
				And<InventoryItem.isTemplate, Equal<False>,
				And<Match<Current<AccessInfo.userName>>>>>>>());

			this.Views[nameof(Item)] = Item.View;

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });

			action.AddMenuAction(ChangeID);
		}

		#region Cache Attached

		#region InventoryItem
		#region InventoryCD
		[PXDefault()]
		[InventoryRaw(typeof(Where<InventoryItem.stkItem, Equal<False>>), IsKey = true, DisplayName = "Inventory ID", Filterable = true)]
		public virtual void InventoryItem_InventoryCD_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region PostClassID
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
		[PXUIField(DisplayName = "Posting Class", Required = true)]
		[PXDefault(typeof(Search<INItemClass.postClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void InventoryItem_PostClassID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region LotSerClassID
		[PXDBString(10, IsUnicode = true)]
		public virtual void InventoryItem_LotSerClassID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ItemType
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INItemTypes.NonStockItem, typeof(Search<INItemClass.itemType, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		[INItemTypes.NonStockList()]
		public virtual void InventoryItem_ItemType_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ValMethod
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.Standard)]
		public virtual void InventoryItem_ValMethod_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region InvtAcctID
		[Account(DisplayName = "Expense Accrual Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.invtAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void InventoryItem_InvtAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region InvtSubID
		[SubAccount(typeof(InventoryItem.invtAcctID), DisplayName = "Expense Accrual Sub.", DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.invtSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void InventoryItem_InvtSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region COGSAcctID
		[PXDefault(typeof(Search<INPostClass.cOGSAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
		[Account(DisplayName = "Expense Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual void InventoryItem_COGSAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region COGSSubID
		[PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
		[SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
		public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region StkItem
		[PXDBBool()]
		[PXDefault(false)]
		public virtual void InventoryItem_StkItem_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ItemClassID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(typeof(Where<INItemClass.stkItem, NotEqual<True>>), Messages.ItemClassIsStock)]
		protected virtual void InventoryItem_ItemClassID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region KitItem
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is a Kit")]
		public virtual void InventoryItem_KitItem_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region DefaultSubItemOnEntry
		[PXDBBool()]
		[PXDefault(false)]
		public virtual void InventoryItem_DefaultSubItemOnEntry_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region DeferredCode
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa", BqlField = typeof(InventoryItem.deferredCode))]
		[PXUIField(DisplayName = "Deferral Code")]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
		[PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
		[PXDefault(typeof(Search<INItemClass.deferredCode, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void InventoryItem_DeferredCode_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region IsSplitted
		[PXDBBool(BqlField = typeof(InventoryItem.isSplitted))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Split into Components")]
		public virtual void InventoryItem_IsSplitted_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UseParentSubID
		[PXDBBool(BqlField = typeof(InventoryItem.useParentSubID))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Component Subaccounts")]
		public virtual void InventoryItem_UseParentSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region TotalPercentage
		[PXDecimal()]
		[PXUIField(DisplayName = "Total Percentage", Enabled = false)]
		public virtual void InventoryItem_TotalPercentage_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region NonStockReceipt
		[PXDBBool(BqlField = typeof(InventoryItem.nonStockReceipt))]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Require Receipt")]
		public virtual void InventoryItem_NonStockReceipt_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region NonStockShip
		[PXDBBool(BqlField = typeof(InventoryItem.nonStockShip))]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Require Shipment")]
		public virtual void InventoryItem_NonStockShip_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region INItemClass

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(false)]
		protected virtual void INItemClass_StkItem_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(INItemTypes.NonStockItem,
			typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<INItemClass.parentItemClassID>>, And<INItemClass.stkItem, Equal<boolFalse>>>>),
			SourceField = typeof(INItemClass.itemType), PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
		protected virtual void INItemClass_ItemType_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region INComponent
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(INComponent.FK.InventoryItem))]
		[PXDBInt(IsKey = true)]
		protected virtual void INComponent_InventoryID_CacheAttached(PXCache sender)
		{			
		}
		[PXDefault()]
		[Inventory(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), Filterable = true, IsKey = true, DisplayName = "Inventory ID")]
		protected virtual void INComponent_ComponentID_CacheAttached(PXCache sender)
		{			
		}
		#endregion

		#region POVendorInventory
        [PXDBInt()]
        [PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<POVendorInventory.inventoryID>>>>))]
        [PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
        protected virtual void POVendorInventory_InventoryID_CacheAttached(PXCache sender)
        {
        }

		[SubItem(typeof(POVendorInventory.inventoryID), DisplayName = "Subitem")]
		protected virtual void POVendorInventory_SubItemID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region INItemXRef
		[PXCustomizeBaseAttribute(typeof(SubItemAttribute), nameof(SubItemAttribute.Disabled), true)]
        public virtual void INItemXRef_SubItemID_CacheAttached(PXCache sender) {}
		#endregion

        #region INItemCategory
        [NonStockItem(IsKey = true, DirtyRead = true)]
        [PXParent(typeof(INItemCategory.FK.InventoryItem))]
        [PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
        protected virtual void INItemCategory_InventoryID_CacheAttached(PXCache sender) { }
		#endregion

		#endregion

		#region Selects

		public PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>> CurrentBranch;

		#endregion

		#region InventoryItem Event Handlers

		protected override void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.InventoryItem_RowSelected(sender, e);

			var item = e.Row as InventoryItem;
            if (item == null)
				return;

            //Multiple Components are not supported for CashReceipt Deferred Revenue:
			DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<InventoryItem.deferredCode>>>>.Select(this);
            PXUIFieldAttribute.SetEnabled<POVendorInventory.isDefault>(this.VendorItems.Cache, null, true);
			
			//Initial State for Components:
			Components.Cache.AllowDelete = false;
			Components.Cache.AllowInsert = false;
			Components.Cache.AllowUpdate = false;

			SetDefaultTermControlsState(sender, item);

			if (item.IsSplitted == true)
			{
				Components.Cache.AllowDelete = true;
				Components.Cache.AllowInsert = true;
				Components.Cache.AllowUpdate = true;
				item.TotalPercentage = SumComponentsPercentage();
				PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(sender, item, true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(sender, item, false);
				item.UseParentSubID = false;
				item.TotalPercentage = 100;
			}
			if (item.NonStockReceipt == true)
			{
				PXUIFieldAttribute.SetRequired<InventoryItem.postClassID>(sender, true);
			}
			else
			{
				PXUIFieldAttribute.SetRequired<InventoryItem.postClassID>(sender, false);
			}
			InventoryHelper.CheckZeroDefaultTerm<InventoryItem.deferredCode, InventoryItem.defaultTerm>(sender, item);
			PXUIFieldAttribute.SetVisible<InventoryItem.taxCalcMode>(sender, item, item.ItemType == INItemTypes.ExpenseItem);

			sender.Adjust<PXUIFieldAttribute>(item).For<InventoryItem.completePOLine>(fa => fa.Enabled = (item.TemplateItemID == null))
				.SameFor<InventoryItem.nonStockReceipt>()
				.SameFor<InventoryItem.nonStockShip>();
		}

		protected override void InventoryItem_ItemClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			base.InventoryItem_ItemClassID_FieldUpdated(sender, e);

			if (doResetDefaultsOnItemClassChange)
			{
				sender.SetDefaultExt<InventoryItem.accrueCost>(e.Row);
			}
		}

		protected virtual void InventoryItem_PostClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<InventoryItem.invtAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.invtSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.salesAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.salesSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.cOGSAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.cOGSSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pOAccrualAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pOAccrualSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pPVAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.pPVSubID>(e.Row);
			sender.SetDefaultExt<InventoryItem.deferralAcctID>(e.Row);
			sender.SetDefaultExt<InventoryItem.deferralSubID>(e.Row);
		}

		protected virtual void InventoryItem_KitItem_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (InventoryItem)e.Row;
			if (row != null && row.KitItem == true)
			{
				sender.SetValueExt<InventoryItem.accrueCost>(row, false);
			}
		}

		protected virtual void InventoryItem_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			((InventoryItem)e.Row).TotalPercentage = 100;
		}

		protected override void InventoryItem_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			base.InventoryItem_RowPersisting(sender, e);

			InventoryItem row = e.Row as InventoryItem;

			if (row.IsSplitted == true)
			{
				if (string.IsNullOrEmpty(row.DeferredCode))
				{
					if (sender.RaiseExceptionHandling<InventoryItem.deferredCode>(e.Row, row.DeferredCode, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.deferredCode).Name)))
					{
						throw new PXRowPersistingException(typeof(InventoryItem.deferredCode).Name, row.DeferredCode, Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.deferredCode).Name);
					}
				}

				var components = Components.Select().RowCast<INComponent>().ToList();

				VerifyComponentPercentages(sender, row, components);
				VerifyOnlyOneResidualComponent(sender, row, components);
				CheckSameTermOnAllComponents(Components.Cache, components);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() || !PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				row.NonStockReceipt = false;
				row.NonStockShip = false;
			}

			if (row.NonStockReceipt == true)
			{
				if (string.IsNullOrEmpty(row.PostClassID))
				{
					throw new PXRowPersistingException(typeof(InventoryItem.postClassID).Name, row.PostClassID, Data.ErrorMessages.FieldIsEmpty, typeof(InventoryItem.postClassID).Name);
				}
			}

			if (e.Operation == PXDBOperation.Delete)
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
                    PXDatabase.Delete<CSAnswers>(new PXDataFieldRestrict("RefNoteID", PXDbType.UniqueIdentifier, ((InventoryItem)e.Row).NoteID));
                    ts.Complete(this);
				}
			}
		}

		protected virtual void InventoryItem_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			DiscountEngine.RemoveFromCachedInventoryPriceClasses(((InventoryItem)e.Row).InventoryID);
		}
		#endregion

		#region Actions
		public PXAction<InventoryItem> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Update)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 1, 2, 3 }, new string[] 
			{ 
					"Update Price", 
					"Update Cost",
          "View Restriction Groups"                    
			})]
			int? actionID
			)
		{
			switch (actionID)
			{
				case 2:
					if (ItemSettings.Current != null && ItemSettings.Current.PendingStdCostDate != null)
					{
						ItemSettings.Current.LastStdCost = ItemSettings.Current.StdCost;
						ItemSettings.Current.StdCostDate = ItemSettings.Current.PendingStdCostDate.GetValueOrDefault((DateTime)this.Accessinfo.BusinessDate);
						ItemSettings.Current.StdCost = ItemSettings.Current.PendingStdCost;
						ItemSettings.Current.PendingStdCost = 0;
						ItemSettings.Current.PendingStdCostDate = null;
						ItemSettings.Update(ItemSettings.Current);

						this.Save.Press();
					}

					break;
				case 3:
					if (Item.Current != null)
					{
						INAccessDetailByItem graph = CreateInstance<INAccessDetailByItem>();
						graph.Item.Current = graph.Item.Search<InventoryItem.inventoryCD>(Item.Current.InventoryCD);
						throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
					}
					break;
            }
            return adapter.Get();
        }

        public PXAction<InventoryItem> inquiry;
        [PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.InquiriesFolder)]
        protected virtual IEnumerable Inquiry(PXAdapter adapter,
            [PXInt]
			[PXIntList(new int[] { 1, 2 }, new string[] 
			{ 
					"Sales Prices",
					"Vendor Prices"
			})]
			int? inquiryID
            )
        {
            switch (inquiryID)
            {
                case 1:
                    if (Item.Current != null)
                    {
                        ARSalesPriceMaint graph = PXGraph.CreateInstance<ARSalesPriceMaint>();
                        graph.Filter.Current.InventoryID = Item.Current.InventoryID;
                        throw new PXRedirectRequiredException(graph, "Sales Prices");
                    }
                    break;
                case 2:
                    if (Item.Current != null)
                    {
                        APVendorPriceMaint graph = PXGraph.CreateInstance<APVendorPriceMaint>();
                        graph.Filter.Current.InventoryID = Item.Current.InventoryID;
                        throw new PXRedirectRequiredException(graph, "Vendor Prices");
                    }
                    break;

			}
			return adapter.Get();
		}
		#endregion

		#region INItemXRef Event Handlers

		protected virtual void INItemXRef_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

		#endregion
	}
}
