using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.Data.Maintenance;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.IN
{
	public class INItemClassMaint : PXGraph<INItemClassMaint, INItemClass>, IGraphWithInitialization
	{
		public class ImmediatelyChangeID : PXImmediatelyChangeID<INItemClass, INItemClass.itemClassCD>
		{
			public ImmediatelyChangeID(PXGraph graph, String name) : base(graph, name) {}
			public ImmediatelyChangeID(PXGraph graph, Delegate handler) : base(graph, handler) {}

			protected override void Initialize()
			{
				base.Initialize();
				DuplicatedKeyMessage = Messages.DuplicateItemClassID;
			}
		}

		public class GoTo : IBqlTable
		{
			public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
            [PXInt]
            public Int32? ItemClassID { get; set; }
		}


		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INParentItemClass(defaultStkItemFromParent: true)]
		protected virtual void INItemClass_ParentItemClassID_CacheAttached(PXCache sender) { }

		[PXDBString(255)]
		[PXUIField(DisplayName = "Specific Type")]
		[PXStringList(new string[] { "PX.Objects.CS.SegmentValue", "PX.Objects.IN.InventoryItem" },
			new string[] { "Subitem", "Inventory Item Restriction" })]
		protected virtual void RelationGroup_SpecificType_CacheAttached(PXCache sender) { }

		[PXDefault]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(InventoryItem.inventoryCD), typeof(InventoryItem.inventoryCD))]
		protected virtual void InventoryItem_InventoryCD_CacheAttached(PXCache sender) { }

		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXUIRequiredAttribute))]
		protected virtual void InventoryItem_ItemClassID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Lot/Serial Class")]
		[PXDBString(10, IsUnicode = true)]
		protected virtual void InventoryItem_LotSerClassID_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Posting Class")]
		[PXDBString(10, IsUnicode = true)]
		protected virtual void InventoryItem_PostClassID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes]
		[PXParent(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<CSAttributeGroup.entityClassID>>>>), LeaveChildren = true)]
		[PXDBLiteDefault(typeof(INItemClass.itemClassStrID))]
		protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender) { } 
		#endregion

		#region Selects
		[PXHidden]
		public PXSelect<INLotSerClass> lotSerClass;
		public PXSetup<INSetup> inSetup;
		public PXSelect<INItemClass> itemclass;
		public PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Current<INItemClass.itemClassID>>>> itemclasssettings;
		public PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Current<INItemClass.itemClassID>>>> TreeViewAndPrimaryViewSynchronizationHelper;
		public PXSetup<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Current<INItemClass.lotSerClassID>>>> lotserclass;
		public INUnitSelect2<INUnit, INItemClass.itemClassID, INItemClass.salesUnit, INItemClass.purchaseUnit, INItemClass.baseUnit, INItemClass.lotSerClassID> classunits;
		public PXSelect<INItemClassSubItemSegment, Where<INItemClassSubItemSegment.itemClassID, Equal<Current<INItemClass.itemClassID>>>, OrderBy<Asc<INItemClassSubItemSegment.segmentID>>> segments;
		protected virtual IEnumerable Segments() => GetSegments();
		public CSAttributeGroupList<INItemClass.itemClassID, InventoryItem> Mapping;
		public PXSelect<INItemClassRep, Where<INItemClassRep.itemClassID, Equal<Optional<INItemClass.itemClassID>>>> replenishment;
		public PXSelect<SegmentValue> segmentvalue;
		public PXSelect<RelationGroup> Groups;
		protected IEnumerable groups() => GetRelationGroups();

		[PXCopyPasteHiddenView]
		public PXSelectReadonly3<ItemClassTree.INItemClass, OrderBy<Asc<ItemClassTree.INItemClass.itemClassCD>>> ItemClassNodes;
		protected virtual IEnumerable itemClassNodes([PXInt] int? itemClassID) => ItemClassTree.EnrollNodes(itemClassID);
		public PXSelect<InventoryItem, Where<InventoryItem.itemClassID, Equal<Current<INItemClass.itemClassID>>>> Items;
		public PXFilter<GoTo> goTo;
		#endregion

		#region Actions

		public ImmediatelyChangeID ChangeID;

		public PXAction<INItemClass> viewGroupDetails;
		[PXUIField(DisplayName = Messages.ViewRestrictionGroup, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewGroupDetails(PXAdapter adapter)
		{
			if (Groups.Current != null)
			{
				RelationGroups graph = CreateInstance<RelationGroups>();
				graph.HeaderGroup.Current = graph.HeaderGroup.Search<RelationHeader.groupName>(Groups.Current.GroupName);
				throw new PXRedirectRequiredException(graph, false, Messages.ViewRestrictionGroup);
			}
			return adapter.Get();
		}

		public PXAction<INItemClass> viewRestrictionGroups;
		[PXUIField(DisplayName = GL.Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (itemclass.Current != null)
			{
				INAccessDetailByClass graph = CreateInstance<INAccessDetailByClass>();
				graph.Class.Current = graph.Class.Search<INItemClass.itemClassID>(itemclass.Current.ItemClassID);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		public PXAction<INItemClass> resetGroup;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.ApplyRestrictionSettings)]
		protected virtual IEnumerable ResetGroup(PXAdapter adapter)
		{
			if (itemclass.Ask(Messages.Warning, CS.Messages.GroupUpdateConfirm, MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				Save.Press();
				int? classID = itemclass.Current.ItemClassID;
				PXLongOperation.StartOperation(this, delegate()
				{
					Reset(classID);
				});
			}
			return adapter.Get();
		}
		protected static void Reset(int? classID)
		{
			INItemClassMaint graph = PXGraph.CreateInstance<INItemClassMaint>();
			INItemClass itemclass = graph.itemclass.Current = graph.itemclass.Search<INItemClass.itemClassID>(classID);
			if (itemclass != null)
			{
				PXDatabase.Update<InventoryItem>(new PXDataFieldRestrict<InventoryItem.itemClassID>(itemclass.ItemClassID),
					new PXDataFieldAssign<InventoryItem.groupMask>(itemclass.GroupMask));
			}
		}

		public PXAction<INItemClass> applyToChildren;
		[PXUIField(DisplayName = Messages.ApplyToChildren, Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual IEnumerable ApplyToChildren(PXAdapter adapter)
		{
			if (itemclass.Current != null &&
				itemclass.Ask(Messages.Confirmation, Messages.ConfirmItemClassApplyToChildren, MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				Actions.PressSave();
				int? itemclassID = itemclass.Current.ItemClassID;
				PXLongOperation.StartOperation(this, delegate() { UpdateChildren(itemclassID); });
			}
			return adapter.Get();
		}

		[PXUIField]
		[PXDeleteButton]
		protected virtual IEnumerable delete(PXAdapter adapter)
		{
			if (itemclass.Current != null)
			{
				var tree = ItemClassTree.Instance;
				var parent = tree.GetParentsOf(itemclass.Current.ItemClassID.Value).FirstOrDefault();
				IEnumerable<INItemClass> children = tree.GetAllChildrenOf(itemclass.Current.ItemClassID.Value);
				bool deleteChildren = children.Any() &&
									itemclass.Ask(Messages.Confirmation, Messages.ConfirmItemClassDeleteKeepChildren, MessageButtons.YesNo) == WebDialogResult.No;
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (deleteChildren)
					{
						var graph = PXGraph.CreateInstance<INItemClassMaint>();
						foreach (INItemClass child in children)
						{
							graph.itemclass.Current = graph.itemclass.Search<INItemClass.itemClassID>(child.ItemClassID);
							graph.itemclass.Delete(graph.itemclass.Current);
						}
						graph.Actions.PressSave();
					}
					itemclass.Delete(itemclass.Current);
					Actions.PressSave();

					ts.Complete();
				}

				if (parent == null)
				{
					if (itemclass.AllowInsert)
					{
						itemclass.Insert();
						itemclass.Cache.IsDirty = false;
					}
				}
				else
				{
					itemclass.Current = parent;
				}

				SelectTimeStamp();
				yield return itemclass.Current;
			}
			else
			{
				foreach (var row in adapter.Get())
					yield return row;
			}
		}

		#region MyButtons (MMK)
		public PXAction<INItemClass> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter) => adapter.Get();
		#endregion

		public PXAction<INItemClass> GoToNodeSelectedInTree;
		[PXButton, PXUIField(MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		protected virtual IEnumerable goToNodeSelectedInTree(PXAdapter adapter)
		{
			if (itemclass.Cache.IsDirty && itemclass.View.Answer == WebDialogResult.None)
				goTo.Current.ItemClassID = ItemClassNodes.Current?.ItemClassID;

			if (itemclass.Cache.IsDirty == false || itemclass.AskExt() == WebDialogResult.OK)
			{
				Int32? goToItemClassID = itemclass.Cache.IsDirty
					? goTo.Current.ItemClassID
					: ItemClassNodes.Current?.ItemClassID;
				Actions.PressCancel();

				_forbidToSyncTreeCurrentWithPrimaryViewCurrent = true;
				itemclass.Current = PXSelect<INItemClass>.Search<INItemClass.itemClassID>(this, goToItemClassID);
				SetTreeCurrent(goToItemClassID);
			}
			else
			{
				SetTreeCurrent(itemclass.Current.ItemClassID);
			}

			return new[] {itemclass.Current};
		}

		#endregion

		private readonly Lazy<bool> _timestampSelected = new Lazy<bool>(() => { PXDatabase.SelectTimeStamp(); return true; });

		public INItemClassMaint()
		{
			PXUIFieldAttribute.SetVisible<INUnit.toUnit>(classunits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INUnit.toUnit>(classunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INUnit.sampleToUnit>(classunits.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INUnit.sampleToUnit>(classunits.Cache, null, false);

			PXUIFieldAttribute.SetEnabled(Groups.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<PX.SM.RelationGroup.included>(Groups.Cache, null, true);

			action.AddMenuAction(applyToChildren);
			action.AddMenuAction(ChangeID);
			action.AddMenuAction(resetGroup);

			if (!PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
			{
				action.SetVisible(false);
				resetGroup.SetVisible(false);
				PXUIFieldAttribute.SetVisible(replenishment.Cache, null, false);
				PXUIFieldAttribute.SetVisible(itemclass.Cache, null, false);
				PXUIFieldAttribute.SetVisible(Groups.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemClass.itemClassCD>(itemclass.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemClass.descr>(itemclass.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemClass.itemType>(itemclass.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemClass.taxCategoryID>(itemclass.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemClass.baseUnit>(itemclass.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemClass.priceClassID>(itemclass.Cache, null, true);
			}
		}

		public virtual void Initialize()
		{
			OnBeforeCommit += (graph) => ValidateINUnit(itemclass.Current);
		}
		
		private bool _allowToSyncTreeCurrentWithPrimaryViewCurrent;
		private bool _forbidToSyncTreeCurrentWithPrimaryViewCurrent;
		public override IEnumerable ExecuteSelect(String viewName, Object[] parameters, Object[] searches, String[] sortcolumns, Boolean[] descendings, PXFilterRow[] filters, ref Int32 startRow, Int32 maximumRows, ref Int32 totalRows)
		{
			if (viewName == nameof(TreeViewAndPrimaryViewSynchronizationHelper))
				_allowToSyncTreeCurrentWithPrimaryViewCurrent = true;
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		#region INItemClass events
		protected virtual void INItemClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var itemClass = (INItemClass)e.Row;
			bool StockItem = e.Row == null || itemClass.StkItem == true;

			INItemTypes.CustomListAttribute stringlist;

			if (StockItem)
			{
				stringlist = new INItemTypes.StockListAttribute();
			}
			else
			{
				stringlist = new INItemTypes.NonStockListAttribute();
			}
			PXUIFieldAttribute.SetVisible<INItemClass.taxCalcMode>(itemclass.Cache, e.Row, ((INItemClass)e.Row).ItemType == INItemTypes.ExpenseItem);
			_timestampSelected.Init();

			PXStringListAttribute.SetList<INItemClass.itemType>(itemclass.Cache, e.Row, stringlist.AllowedValues, stringlist.AllowedLabels);

			PXUIFieldAttribute.SetEnabled<INItemClass.stkItem>(itemclass.Cache, e.Row, !IsDefaultItemClass(itemClass));

			PXDefaultAttribute.SetPersistingCheck<INItemClass.availabilitySchemeID>(itemclass.Cache, e.Row, StockItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<INItemClass.availabilitySchemeID>(itemclass.Cache, StockItem);
			PXUIFieldAttribute.SetEnabled<INItemClass.availabilitySchemeID>(itemclass.Cache, e.Row, StockItem);
			PXUIFieldAttribute.SetEnabled<INItemClass.lotSerClassID>(itemclass.Cache, e.Row, StockItem);

			SyncTreeCurrentWithPrimaryViewCurrent(itemClass);
		}

		protected virtual void INItemClass_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null && (!PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() || !PXAccess.FeatureInstalled<FeaturesSet.inventory>()))
			{
				sender.SetValueExt<INItemClass.stkItem>(e.Row, false);
			}
			ItemClassNodes.Current = null;
			ItemClassNodes.Cache.ActiveRow = null;
		}

		protected virtual void INItemClass_StkItem_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INItemClass row = e.Row as INItemClass;
			if (row == null)
				return;

			bool? newValue = (bool?)e.NewValue;

			if (row.StkItem != newValue)
			{
				var inventoryItem = GetFirstItem(row);
				if (inventoryItem != null)
					throw new PXSetPropertyException<INItemClass.stkItem>(Messages.StkItemValueCanNotBeChangedBecauseItIsUsedInInventoryItem, inventoryItem.InventoryCD);
			}
		}

		protected virtual void INItemClass_StkItem_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INItemClass.parentItemClassID>(e.Row);
			sender.SetDefaultExt<INItemClass.itemType>(e.Row);
			sender.SetDefaultExt<INItemClass.valMethod>(e.Row);
			sender.SetDefaultExt<INItemClass.accrueCost>(e.Row);
			
			if (((INItemClass)e.Row).StkItem != true)
			{
				sender.SetValueExt<INItemClass.availabilitySchemeID>(e.Row, null);
				sender.SetValueExt<INItemClass.lotSerClassID>(e.Row, null);
			}

			if (sender.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				sender.SetDefaultExt<INItemClass.negQty>(e.Row);
				if (((INItemClass)e.Row).StkItem == true)
					sender.SetDefaultExt<INItemClass.availabilitySchemeID>(e.Row);
				//sales and purchase units must be cleared not to be added to item unit conversions on base unit change
				sender.SetValueExt<INItemClass.baseUnit>(e.Row, null);
				sender.SetValue<INItemClass.salesUnit>(e.Row, null);
				sender.SetValue<INItemClass.purchaseUnit>(e.Row, null);
				sender.SetDefaultExt<INItemClass.baseUnit>(e.Row);
				sender.SetDefaultExt<INItemClass.salesUnit>(e.Row);
				sender.SetDefaultExt<INItemClass.purchaseUnit>(e.Row);
				sender.SetDefaultExt<INItemClass.postClassID>(e.Row);
				if (((INItemClass)e.Row).StkItem == true)
					sender.SetDefaultExt<INItemClass.lotSerClassID>(e.Row);
				sender.SetDefaultExt<INItemClass.taxCategoryID>(e.Row);
				sender.SetDefaultExt<INItemClass.deferredCode>(e.Row);
				sender.SetDefaultExt<INItemClass.priceClassID>(e.Row);
				sender.SetDefaultExt<INItemClass.priceWorkgroupID>(e.Row);
				sender.SetDefaultExt<INItemClass.priceManagerID>(e.Row);
				sender.SetDefaultExt<INItemClass.dfltSiteID>(e.Row);
				sender.SetDefaultExt<INItemClass.minGrossProfitPct>(e.Row);
				sender.SetDefaultExt<INItemClass.markupPct>(e.Row);
				sender.SetDefaultExt<INItemClass.demandCalculation>(e.Row);
				sender.SetDefaultExt<INItemClass.groupMask>(e.Row);
				InitDetailsFromParentItemClass(e.Row as INItemClass);
			}
		}

		protected virtual void INItemClass_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			InitDetailsFromParentItemClass(e.Row as INItemClass);
		}

		protected virtual void INItemClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (IsDefaultItemClass(e.Row as INItemClass))
			{
				throw new PXException(Messages.ThisItemClassCanNotBeDeletedBecauseItIsUsedInInventorySetup);
			}

			InventoryItem inventoryItemRec = GetFirstItem(e.Row as INItemClass);
			if (inventoryItemRec != null)
			{
				throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ThisItemClassCanNotBeDeletedBecauseItIsUsedInInventoryItem, inventoryItemRec.InventoryCD));
			}

			INLocation iNLocationRec = (INLocation)PXSelect<INLocation, Where<INLocation.primaryItemClassID, Equal<Current<INItemClass.itemClassID>>>>.SelectWindowed(this, 0, 1);
			if (iNLocationRec != null)
			{
				INSite iNSiteRec = INSite.PK.Find(this, iNLocationRec.SiteID);
				throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ThisItemClassCanNotBeDeletedBecauseItIsUsedInWarehouseLocation, (iNSiteRec.SiteCD ?? ""), iNLocationRec.LocationCD));
			}

		}

		protected virtual void INItemClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (((INItemClass)e.Row).ValMethod == INValMethod.Specific && lotserclass.Current != null && (lotserclass.Current.LotSerTrack == INLotSerTrack.NotNumbered || lotserclass.Current.LotSerAssign != INLotSerAssign.WhenReceived))
				{
					if (sender.RaiseExceptionHandling<INItemClass.valMethod>(e.Row, INValMethod.Specific, new PXSetPropertyException(Messages.SpecificOnlyNumbered)))
					{
						throw new PXRowPersistingException(typeof(INItemClass.valMethod).Name, INValMethod.Specific, Messages.SpecificOnlyNumbered, typeof(INItemClass.valMethod).Name);
					}
				}
			}
		}

		protected virtual void INItemClass_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PXDBOperation command = e.Operation & PXDBOperation.Command;
			if (e.TranStatus == PXTranStatus.Completed && (command == PXDBOperation.Insert || command == PXDBOperation.Delete || command == PXDBOperation.Update))
			{
				SelectTimeStamp(); // needed to reload slot with item class tree
			}
		}
		#endregion

		#region INItemClassRep events
		protected virtual void INItemClassRep_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INItemClassRep row = (INItemClassRep)e.Row;
			if (row != null)
			{
				bool isTransfer = (row.ReplenishmentSource == INReplenishmentSource.Transfer);
				bool isFixedReorder = (row.ReplenishmentMethod == INReplenishmentMethod.FixedReorder);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.replenishmentMethod>(sender, row, row.ReplenishmentSource != INReplenishmentSource.PurchaseToOrder && row.ReplenishmentSource != INReplenishmentSource.DropShipToOrder);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.replenishmentSourceSiteID>(sender, row, row.ReplenishmentSource == INReplenishmentSource.PurchaseToOrder || row.ReplenishmentSource == INReplenishmentSource.DropShipToOrder || row.ReplenishmentSource == INReplenishmentSource.Transfer || row.ReplenishmentSource == INReplenishmentSource.Purchased);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.transferLeadTime>(sender, e.Row, isTransfer);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.transferERQ>(sender, e.Row, isTransfer && isFixedReorder && row.ReplenishmentMethod != INReplenishmentMethod.None);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.forecastModelType>(sender, e.Row, row.ReplenishmentMethod != INReplenishmentMethod.None);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.forecastPeriodType>(sender, e.Row, row.ReplenishmentMethod != INReplenishmentMethod.None);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.historyDepth>(sender, e.Row, row.ReplenishmentMethod != INReplenishmentMethod.None);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.launchDate>(sender, e.Row, row.ReplenishmentMethod != INReplenishmentMethod.None);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.terminationDate>(sender, e.Row, row.ReplenishmentMethod != INReplenishmentMethod.None);
				PXUIFieldAttribute.SetEnabled<INItemClassRep.serviceLevelPct>(sender, e.Row, row.ReplenishmentMethod != INReplenishmentMethod.None);
				PXDefaultAttribute.SetPersistingCheck<INItemClassRep.transferLeadTime>(sender, e.Row, isTransfer ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<INItemClassRep.transferERQ>(sender, e.Row, (isTransfer && isFixedReorder) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			}
		}

		protected virtual void INItemClassRep_ReplenishmentSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INItemClassRep r = e.Row as INItemClassRep;
			if (r == null) return;
			if (r.ReplenishmentSource == INReplenishmentSource.PurchaseToOrder || r.ReplenishmentSource == INReplenishmentSource.DropShipToOrder)
			{
				sender.SetValueExt<INItemClassRep.replenishmentMethod>(r, INReplenishmentMethod.None);
			}
			if (r.ReplenishmentSource != INReplenishmentSource.PurchaseToOrder && r.ReplenishmentSource != INReplenishmentSource.DropShipToOrder && r.ReplenishmentSource != INReplenishmentSource.Transfer)
			{
				sender.SetDefaultExt<INItemClassRep.replenishmentSourceSiteID>(r);
			}
			sender.SetDefaultExt<INItemClassRep.transferLeadTime>(e.Row);

		}
		#endregion

		#region RelationGroup events
		protected virtual void RelationGroup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RelationGroup group = e.Row as RelationGroup;
			if (itemclass.Current != null && group != null && Groups.Cache.GetStatus(group) == PXEntryStatus.Notchanged)
			{
				group.Included = UserAccess.IsIncluded(itemclass.Current.GroupMask, group);
			}
		}

		protected virtual void RelationGroup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region INItemClassSubItemSegment events
		protected virtual void INItemClassSubItemSegment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				INItemClassSubItemSegment seg = (INItemClassSubItemSegment)e.Row;

				if (seg.IsActive == false && string.IsNullOrEmpty(seg.DefaultValue))
				{
					if (sender.RaiseExceptionHandling<INItemClassSubItemSegment.defaultValue>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(INItemClassSubItemSegment.defaultValue).Name)))
					{
						throw new PXRowPersistingException(typeof(INItemClassSubItemSegment.defaultValue).Name, null, ErrorMessages.FieldIsEmpty, typeof(INItemClassSubItemSegment.defaultValue).Name);
					}
				}

				SegmentValue val = (SegmentValue)PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>, And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>, And<SegmentValue.isConsolidatedValue, Equal<boolTrue>>>>>.Select(this, seg.SegmentID);

				if (val == null)
				{
					val = (SegmentValue)PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>, And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>, And<SegmentValue.value, Equal<Required<SegmentValue.value>>>>>>.Select(this, seg.SegmentID, seg.DefaultValue);

					if (val != null)
					{
						val.IsConsolidatedValue = true;
						segmentvalue.Cache.SetStatus(val, PXEntryStatus.Updated);
						segmentvalue.Cache.IsDirty = true;
					}
				}
			}
		}
		#endregion


		public override void Persist()
		{
			if (itemclass.Current != null && itemclass.Current.StkItem == true && string.IsNullOrEmpty(itemclass.Current.LotSerClassID) && !PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>())
			{
				itemclass.Current.LotSerClassID = INLotSerClass.GetDefaultLotSerClass(this);
			}

			if (itemclass.Current != null && Groups.Cache.IsDirty)
			{
				PX.SM.UserAccess.PopulateNeighbours<INItemClass>(itemclass, Groups, typeof(SegmentValue));
				PXSelectorAttribute.ClearGlobalCache<INItemClass>();
			}
			base.Persist();
			Groups.Cache.Clear();
			GroupHelper.Clear();
		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			if (cacheType == typeof(INUnit) && operation == PXDBOperation.Update)
			{
				base.Persist(cacheType, PXDBOperation.Insert);
			}
			return base.Persist(cacheType, operation);
		}

		protected virtual InventoryItem GetFirstItem(INItemClass itemClass)
		{
			if (itemClass == null)
				return null;

			return (InventoryItem)PXSelect<InventoryItem,
				Where<InventoryItem.itemClassID, Equal<Required<INItemClass.itemClassID>>>>.SelectWindowed(this, 0, 1, itemClass.ItemClassID);
		}

		protected virtual bool IsDefaultItemClass(INItemClass itemClass)
		{
			INSetup inSetupRec = inSetup.Select();
			return itemClass != null && inSetupRec != null
				&& (itemClass.ItemClassID == inSetupRec.DfltNonStkItemClassID || itemClass.ItemClassID == inSetupRec.DfltStkItemClassID);
		}

		protected virtual void InitDetailsFromParentItemClass(INItemClass itemClass)
		{
			if (itemClass == null || itemClass.ParentItemClassID == null) return;

			using (new ReadOnlyScope(classunits.Cache, replenishment.Cache, Mapping.Cache))
			{
				classunits.Cache.ClearQueryCache();
				classunits.Cache.Clear();
				foreach (INUnit conv in classunits.Select(itemClass.ParentItemClassID))
				{
					var convCopy = classunits.Cache.CreateCopy(conv);
					classunits.Cache.SetValue<INUnit.recordID>(convCopy, null);
					classunits.Cache.SetValue<INUnit.itemClassID>(convCopy, itemClass.ItemClassID);
					convCopy = classunits.Cache.Insert(convCopy);
					if (convCopy == null)
						throw new PXException(Messages.CopyingSettingsFailed);
				}
				replenishment.Cache.ClearQueryCache();
				replenishment.Cache.Clear();
				foreach (INItemClassRep rep in replenishment.Select(itemClass.ParentItemClassID))
				{
					var repCopy = replenishment.Cache.CreateCopy(rep);
					replenishment.Cache.SetValue<INItemClassRep.itemClassID>(repCopy, itemClass.ItemClassID);
					repCopy = replenishment.Cache.Insert(repCopy);
					if (repCopy == null)
						throw new PXException(Messages.CopyingSettingsFailed);
				}
				Mapping.Cache.ClearQueryCache();
				Mapping.Cache.Clear();
				foreach (CSAttributeGroup attr in SelectCSAttributeGroupRecords(itemClass.ParentItemClassID.ToString()))
				{
					var attrCopy = Mapping.Cache.CreateCopy(attr);
					Mapping.Cache.SetValue<CSAttributeGroup.entityClassID>(attrCopy, itemClass.ItemClassStrID);
					attrCopy = Mapping.Cache.Insert(attrCopy);
					if (attrCopy == null)
						throw new PXException(Messages.CopyingSettingsFailed);
				}
			}
		}

		protected static void UpdateChildren(int? itemClassID)
		{
			var graph = PXGraph.CreateInstance<INItemClassMaint>();
			graph.itemclass.Current = graph.itemclass.Search<INItemClass.itemClassID>(itemClassID);
			if (graph.itemclass.Current != null)
			{
				var tree = ItemClassTree.Instance;
				IEnumerable<INItemClass> children = tree.GetAllChildrenOf(graph.itemclass.Current.ItemClassID.Value);
				if (children.Any())
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						var replenishmentTemplate = graph.replenishment.Select().RowCast<INItemClassRep>();
						var unitConversionsTemplate = graph.classunits.Select().RowCast<INUnit>();
						var attributesTemplate = graph.Mapping.Select().RowCast<CSAttributeGroup>();
						foreach (INItemClass child in children)
						{
							graph.UpdateChildItemClass(child);
							graph.MergeReplenishment(child, replenishmentTemplate);
							graph.MergeUnitConversions(child, unitConversionsTemplate);
							graph.MergeAttributes(child, attributesTemplate);
						}
						ts.Complete();
					}
				}
			}
		}

		protected virtual void UpdateChildItemClass(INItemClass child)
		{
			if (child.StkItem != itemclass.Current.StkItem)
			{
				var inventoryItem = GetFirstItem(child);
				if (inventoryItem != null)
				{
					throw new PXSetPropertyException<INItemClass.stkItem>(Messages.ChildStkItemValueCanNotBeChangedBecauseItIsUsedInInventoryItem,
						child.ItemClassCD, inventoryItem.InventoryCD);
				}
			}

			PXDatabase.Update<INItemClass>(
				new PXDataFieldRestrict<INItemClass.itemClassID>(child.ItemClassID),
				new PXDataFieldAssign<INItemClass.stkItem>(itemclass.Current.StkItem),
				new PXDataFieldAssign<INItemClass.negQty>(itemclass.Current.NegQty),
				new PXDataFieldAssign<INItemClass.accrueCost>(itemclass.Current.AccrueCost),
				new PXDataFieldAssign<INItemClass.availabilitySchemeID>(itemclass.Current.AvailabilitySchemeID),
				new PXDataFieldAssign<INItemClass.valMethod>(itemclass.Current.ValMethod),
				new PXDataFieldAssign<INItemClass.baseUnit>(itemclass.Current.BaseUnit),
				new PXDataFieldAssign<INItemClass.salesUnit>(itemclass.Current.SalesUnit),
				new PXDataFieldAssign<INItemClass.purchaseUnit>(itemclass.Current.PurchaseUnit),
				new PXDataFieldAssign<INItemClass.postClassID>(itemclass.Current.PostClassID),
				new PXDataFieldAssign<INItemClass.lotSerClassID>(itemclass.Current.LotSerClassID),
				new PXDataFieldAssign<INItemClass.taxCategoryID>(itemclass.Current.TaxCategoryID),
				new PXDataFieldAssign<INItemClass.deferredCode>(itemclass.Current.DeferredCode),
				new PXDataFieldAssign<INItemClass.itemType>(itemclass.Current.ItemType),
				new PXDataFieldAssign<INItemClass.priceClassID>(itemclass.Current.PriceClassID),
				new PXDataFieldAssign<INItemClass.priceWorkgroupID>(itemclass.Current.PriceWorkgroupID),
				new PXDataFieldAssign<INItemClass.priceManagerID>(itemclass.Current.PriceManagerID),
				new PXDataFieldAssign<INItemClass.dfltSiteID>(itemclass.Current.DfltSiteID),
				new PXDataFieldAssign<INItemClass.minGrossProfitPct>(itemclass.Current.MinGrossProfitPct),
				new PXDataFieldAssign<INItemClass.markupPct>(itemclass.Current.MarkupPct),
				new PXDataFieldAssign<INItemClass.demandCalculation>(itemclass.Current.DemandCalculation)
			);
		}

		protected virtual void MergeReplenishment(INItemClass child, IEnumerable<INItemClassRep> replenishmentTemplate)
		{
			if (!replenishmentTemplate.Any()) return;

			var replenishmentExisting = replenishment.Select(child.ItemClassID).RowCast<INItemClassRep>();
			foreach (INItemClassRep rep in replenishmentTemplate)
			{
				var crudParamsTemplate = new PXDataFieldAssign[]
				{
					new PXDataFieldAssign<INItemClassRep.replenishmentPolicyID>(rep.ReplenishmentPolicyID),
					new PXDataFieldAssign<INItemClassRep.replenishmentMethod>(rep.ReplenishmentMethod),
					new PXDataFieldAssign<INItemClassRep.replenishmentSource>(rep.ReplenishmentSource),
					new PXDataFieldAssign<INItemClassRep.replenishmentSourceSiteID>(rep.ReplenishmentSourceSiteID),
					new PXDataFieldAssign<INItemClassRep.launchDate>(rep.LaunchDate),
					new PXDataFieldAssign<INItemClassRep.terminationDate>(rep.TerminationDate),
					new PXDataFieldAssign<INItemClassRep.forecastModelType>(rep.ForecastModelType),
					new PXDataFieldAssign<INItemClassRep.forecastPeriodType>(rep.ForecastPeriodType),
					new PXDataFieldAssign<INItemClassRep.historyDepth>(rep.HistoryDepth),
					new PXDataFieldAssign<INItemClassRep.transferLeadTime>(rep.TransferLeadTime),
					new PXDataFieldAssign<INItemClassRep.transferERQ>(rep.TransferERQ),
					new PXDataFieldAssign<INItemClassRep.serviceLevel>(rep.ServiceLevel),
					new PXDataFieldAssign<INItemClassRep.eSSmoothingConstantL>(rep.ESSmoothingConstantL),
					new PXDataFieldAssign<INItemClassRep.eSSmoothingConstantT>(rep.ESSmoothingConstantT),
					new PXDataFieldAssign<INItemClassRep.eSSmoothingConstantS>(rep.ESSmoothingConstantS),
					new PXDataFieldAssign<INItemClassRep.autoFitModel>(rep.AutoFitModel),
					new PXDataFieldAssign<INItemClassRep.createdByID>(rep.CreatedByID),
					new PXDataFieldAssign<INItemClassRep.createdByScreenID>(rep.CreatedByScreenID),
					new PXDataFieldAssign<INItemClassRep.createdDateTime>(rep.CreatedDateTime),
					new PXDataFieldAssign<INItemClassRep.lastModifiedByID>(rep.LastModifiedByID),
					new PXDataFieldAssign<INItemClassRep.lastModifiedByScreenID>(rep.LastModifiedByScreenID),
					new PXDataFieldAssign<INItemClassRep.lastModifiedDateTime>(rep.LastModifiedDateTime),
				};

				if (replenishmentExisting.Any(r => r.ReplenishmentClassID == rep.ReplenishmentClassID))
				{
					var updateParams = new List<PXDataFieldParam>(crudParamsTemplate);
					updateParams.Add(new PXDataFieldRestrict<INItemClassRep.itemClassID>(child.ItemClassID));
					updateParams.Add(new PXDataFieldRestrict<INItemClassRep.replenishmentClassID>(rep.ReplenishmentClassID));

					PXDatabase.Update<INItemClassRep>(updateParams.ToArray());
				}
				else
				{
					var insertParams = new List<PXDataFieldAssign>(crudParamsTemplate);
					insertParams.Add(new PXDataFieldAssign<INItemClassRep.itemClassID>(child.ItemClassID));
					insertParams.Add(new PXDataFieldAssign<INItemClassRep.replenishmentClassID>(rep.ReplenishmentClassID));

					PXDatabase.Insert<INItemClassRep>(insertParams.ToArray());
				}
			}
		}

		protected virtual void MergeUnitConversions(INItemClass child, IEnumerable<INUnit> unitConversionsTemplate)
		{
			if (!unitConversionsTemplate.Any()) return;

			var unitConversionsExisting = classunits.Select(child.ItemClassID).RowCast<INUnit>();
			foreach (INUnit conv in unitConversionsTemplate)
			{
				var crudParamsTemplate = new PXDataFieldAssign[]
				{
					new PXDataFieldAssign<INUnit.unitMultDiv>(conv.UnitMultDiv),
					new PXDataFieldAssign<INUnit.unitRate>(conv.UnitRate),
					new PXDataFieldAssign<INUnit.priceAdjustmentMultiplier>(conv.PriceAdjustmentMultiplier),
					new PXDataFieldAssign<INUnit.createdByID>(conv.CreatedByID),
					new PXDataFieldAssign<INUnit.createdByScreenID>(conv.CreatedByScreenID),
					new PXDataFieldAssign<INUnit.createdDateTime>(conv.CreatedDateTime),
					new PXDataFieldAssign<INUnit.lastModifiedByID>(conv.LastModifiedByID),
					new PXDataFieldAssign<INUnit.lastModifiedByScreenID>(conv.LastModifiedByScreenID),
					new PXDataFieldAssign<INUnit.lastModifiedDateTime>(conv.LastModifiedDateTime),
				};

				if (unitConversionsExisting.Any(r => r.UnitType == conv.UnitType && r.FromUnit == conv.FromUnit && r.ToUnit == conv.ToUnit))
				{
					var updateParams = new List<PXDataFieldParam>(crudParamsTemplate);
					updateParams.Add(new PXDataFieldRestrict<INUnit.itemClassID>(child.ItemClassID));
					updateParams.Add(new PXDataFieldRestrict<INUnit.inventoryID>(conv.InventoryID));
					updateParams.Add(new PXDataFieldRestrict<INUnit.unitType>(conv.UnitType));
					updateParams.Add(new PXDataFieldRestrict<INUnit.fromUnit>(conv.FromUnit));
					updateParams.Add(new PXDataFieldRestrict<INUnit.toUnit>(conv.ToUnit));

					PXDatabase.Update<INUnit>(updateParams.ToArray());
				}
				else
				{
					var insertParams = new List<PXDataFieldAssign>(crudParamsTemplate);
					insertParams.Add(new PXDataFieldAssign<INUnit.itemClassID>(child.ItemClassID));
					insertParams.Add(new PXDataFieldAssign<INUnit.inventoryID>(conv.InventoryID));
					insertParams.Add(new PXDataFieldAssign<INUnit.unitType>(conv.UnitType));
					insertParams.Add(new PXDataFieldAssign<INUnit.fromUnit>(conv.FromUnit));
					insertParams.Add(new PXDataFieldAssign<INUnit.toUnit>(conv.ToUnit));
		

					PXDatabase.Insert<INUnit>(insertParams.ToArray());
				}
			}
		}

		protected virtual void MergeAttributes(INItemClass child, IEnumerable<CSAttributeGroup> attributesTemplate)
		{
			if (!attributesTemplate.Any()) return;

			var attributesExisting = SelectCSAttributeGroupRecords(child.ItemClassStrID);
			foreach (CSAttributeGroup attr in attributesTemplate)
			{
				var existingAttribute = attributesExisting.FirstOrDefault(
					r => r.AttributeID == attr.AttributeID && r.EntityType == attr.EntityType);

				MergeAttribute(child, existingAttribute, attr);
			}
		}

		protected virtual void MergeAttribute(INItemClass child, CSAttributeGroup existingAttribute, CSAttributeGroup parentAttribute)
		{
			var crudParamsTemplate = new PXDataFieldAssign[]
			{
					new PXDataFieldAssign<CSAttributeGroup.isActive>(parentAttribute.IsActive),
					new PXDataFieldAssign<CSAttributeGroup.sortOrder>(parentAttribute.SortOrder),
					new PXDataFieldAssign<CSAttributeGroup.required>(parentAttribute.Required),
					new PXDataFieldAssign<CSAttributeGroup.defaultValue>(parentAttribute.DefaultValue),
					new PXDataFieldAssign<CSAttributeGroup.attributeCategory>(parentAttribute.AttributeCategory),
					new PXDataFieldAssign<CSAttributeGroup.createdByID>(parentAttribute.CreatedByID),
					new PXDataFieldAssign<CSAttributeGroup.createdByScreenID>(parentAttribute.CreatedByScreenID),
					new PXDataFieldAssign<CSAttributeGroup.createdDateTime>(parentAttribute.CreatedDateTime),
					new PXDataFieldAssign<CSAttributeGroup.lastModifiedByID>(parentAttribute.LastModifiedByID),
					new PXDataFieldAssign<CSAttributeGroup.lastModifiedByScreenID>(parentAttribute.LastModifiedByScreenID),
					new PXDataFieldAssign<CSAttributeGroup.lastModifiedDateTime>(parentAttribute.LastModifiedDateTime),
			};

			if (existingAttribute != null)
			{
				var updateParams = new List<PXDataFieldParam>(crudParamsTemplate);
				updateParams.Add(new PXDataFieldRestrict<CSAttributeGroup.entityClassID>(child.ItemClassStrID));
				updateParams.Add(new PXDataFieldRestrict<CSAttributeGroup.attributeID>(parentAttribute.AttributeID));
				updateParams.Add(new PXDataFieldRestrict<CSAttributeGroup.entityType>(parentAttribute.EntityType));

				PXDatabase.Update<CSAttributeGroup>(updateParams.ToArray());
			}
			else
			{
				var insertParams = new List<PXDataFieldAssign>(crudParamsTemplate);
				insertParams.Add(new PXDataFieldAssign<CSAttributeGroup.entityClassID>(child.ItemClassStrID));
				insertParams.Add(new PXDataFieldAssign<CSAttributeGroup.attributeID>(parentAttribute.AttributeID));
				insertParams.Add(new PXDataFieldAssign<CSAttributeGroup.entityType>(parentAttribute.EntityType));

				PXDatabase.Insert<CSAttributeGroup>(insertParams.ToArray());
			}
		}

		protected virtual void ValidateINUnit(INItemClass itemClass)
		{
			if (itemClass == null)
				return;

			using (PXDataRecord record = PXDatabase.SelectSingle<INUnit>(new PXDataField<INUnit.toUnit>(),
				new PXDataFieldValue<INUnit.unitType>(INUnitType.ItemClass),
				new PXDataFieldValue<INUnit.itemClassID>(itemClass.ItemClassID),
				new PXDataFieldValue<INUnit.toUnit>(PXDbType.NVarChar, 6, itemClass.BaseUnit, PXComp.NE)))
			{
				if (record != null)
					throw new PXException(Messages.WrongItemClassToUnitValue, record.GetString(0), itemClass.ItemClassCD, itemClass.BaseUnit);
			}
		}

		private IEnumerable<CSAttributeGroup> SelectCSAttributeGroupRecords(string itemClassID)
		{
			var resultSet = new PXSelectJoin<CSAttributeGroup,
				InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				Where<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
					And<CSAttributeGroup.entityType, Equal<Required<CSAttributeGroup.entityType>>>>>(this)
				.Select(itemClassID, typeof(InventoryItem).FullName).AsEnumerable();

			return resultSet
				.Select(r => PXResult.Unwrap<CSAttributeGroup>(r))
				.Where(r => r != null);
		}
		
		private IEnumerable<RelationGroup> GetRelationGroups()
		{
			foreach (RelationGroup group in PXSelect<RelationGroup>.Select(this))
			{
				if (group.SpecificModule == null || group.SpecificModule == typeof(InventoryItem).Namespace
					|| itemclass.Current != null && UserAccess.IsIncluded(itemclass.Current.GroupMask, group))
				{
					Groups.Current = group;
					yield return group;
				}
			}
		}
		
		private IEnumerable<INItemClassSubItemSegment> GetSegments()
		{
			foreach (INItemClassSubItemSegment seg in segments.Cache.Updated)
			{
				yield return seg;
			}

			if (itemclass.Current == null || itemclass.Current.ItemClassID.GetValueOrDefault() == 0)
			{
				yield break;
			}

			var segs = PXSelectJoin<Segment,
				LeftJoin<SegmentValue,
					On<SegmentValue.dimensionID, Equal<Segment.dimensionID>,
						And<SegmentValue.segmentID, Equal<Segment.segmentID>, And<SegmentValue.isConsolidatedValue, Equal<True>>>>,
					LeftJoin<INItemClassSubItemSegment,
						On<INItemClassSubItemSegment.itemClassID, Equal<Current<INItemClass.itemClassID>>,
							And<INItemClassSubItemSegment.segmentID, Equal<Segment.segmentID>>>>>,
				Where<Segment.dimensionID, Equal<SubItemAttribute.dimensionName>>>
				.Select(this);

			foreach (PXResult<Segment, SegmentValue, INItemClassSubItemSegment> res in segs)
			{
				INItemClassSubItemSegment seg = (INItemClassSubItemSegment)res;
				if (seg.SegmentID == null)
				{
					seg.SegmentID = ((Segment)res).SegmentID;
					seg.ItemClassID = itemclass.Current.ItemClassID;
					seg.IsActive = true;
				}

				seg.DefaultValue = ((SegmentValue)res).Value;
				PXUIFieldAttribute.SetEnabled<INItemClassSubItemSegment.defaultValue>(segments.Cache, seg, string.IsNullOrEmpty(seg.DefaultValue));

				INItemClassSubItemSegment cached;

				if ((cached = (INItemClassSubItemSegment)segments.Cache.Locate(seg)) == null || segments.Cache.GetStatus(cached) == PXEntryStatus.Notchanged)
				{
					yield return seg;
				}
			}

			segments.Cache.IsDirty = false;
		}

		private void SyncTreeCurrentWithPrimaryViewCurrent(INItemClass primaryViewCurrent)
		{
			if (_allowToSyncTreeCurrentWithPrimaryViewCurrent && !_forbidToSyncTreeCurrentWithPrimaryViewCurrent
				&& primaryViewCurrent != null && (ItemClassNodes.Current == null || ItemClassNodes.Current.ItemClassID != primaryViewCurrent.ItemClassID))
			{
				SetTreeCurrent(primaryViewCurrent.ItemClassID);
			}
		}

		private void SetTreeCurrent(Int32? itemClassID)
		{
			ItemClassTree.INItemClass current = ItemClassTree.Instance.GetNodeByID(itemClassID ?? 0);
			ItemClassNodes.Current = current;
			ItemClassNodes.Cache.ActiveRow = current;
		}
	}

	public class ItemClassTree 
		: DimensionTree<
			ItemClassTree,
			ItemClassTree.INItemClass,
			INItemClass.dimension,
			ItemClassTree.INItemClass.itemClassCD,
			ItemClassTree.INItemClass.itemClassID>
	{
		[Serializable]
		public class INItemClass : IN.INItemClass
		{
			public new abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
			public new abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
			public new abstract class stkItem : PX.Data.BQL.BqlBool.Field<stkItem> { }

			public virtual string SegmentedClassCD { get; set; }
		}
		
		public string GetFullItemClassDescription(string itemClassCD)
		{
			if (itemClassCD == null) return null;
			itemClassCD = itemClassCD.TrimEnd();
			var itemClassNode = GetNodeByCD(itemClassCD);
			return itemClassNode != null 
				? String.Join(" / ", GetParentsOf(itemClassCD).Reverse().Append(itemClassNode).Select(node => node.Descr ?? " ")) 
				: null;
		}

		protected override void PrepareElement(INItemClass original)
		{
			int length = 0;
			Segment[] segments = Segments;
			string segmentedClassCD = PadKey(original.ItemClassCD.Replace(' ', '*'), segments.Sum(s => s.Length.Value));
			foreach (Segment segment in segments.Take(segments.Length - 1))
			{
				segmentedClassCD = segmentedClassCD.Insert(length + segment.Length.Value, segment.Separator);
				length += segment.Length.Value + segment.Separator.Length;
			}
			original.SegmentedClassCD = segmentedClassCD + " " + original.Descr;
		}
	}
}
