using PX.Common;
using PX.Data;
using PX.Objects.Common.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.IN.Matrix.DAC;
using PX.Objects.IN.Matrix.Utility;
using PX.Objects.IN.Matrix.DAC.Projections;
using PX.Objects.PO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.IN.Matrix.DAC.Unbound;
using System;

namespace PX.Objects.IN.Matrix.Graphs
{
	public class TemplateInventoryItemMaint : InventoryItemMaintBase
	{
		const string SampleField = "Sample";

		#region Views

		public PXSelect<INItemBoxEx, Where<INItemBoxEx.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> Boxes;

		public PXSetup<INPostClass>.Where<Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>> postclass;

		public PXSelect<IDGenerationRule,
			Where<IDGenerationRule.templateID, Equal<Current<InventoryItem.inventoryID>>>,
			OrderBy<Asc<IDGenerationRule.sortOrder>>> IDGenerationRules;

		public PXSelect<DescriptionGenerationRule,
			Where<DescriptionGenerationRule.templateID, Equal<Current<InventoryItem.inventoryID>>>,
			OrderBy<Asc<DescriptionGenerationRule.sortOrder>>> DescriptionGenerationRules;

		#endregion // Views

		#region Constructor

		public TemplateInventoryItemMaint()
		{
			Item.View = new PXView(this, false, new Select<InventoryItem,
				Where2<Match<Current<AccessInfo.userName>>,
					And<InventoryItem.isTemplate, Equal<True>>>>());

			Views[nameof(Item)] = Item.View;

			IDGenerationRules.Cache.Fields.Add(SampleField);
			DescriptionGenerationRules.Cache.Fields.Add(SampleField);

			FieldSelecting.AddHandler(typeof(IDGenerationRule), SampleField, SampleIDFieldSelecting);
			FieldSelecting.AddHandler(typeof(DescriptionGenerationRule), SampleField, SampleDescriptionFieldSelecting);
		}

		#endregion // Constructor

		#region Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(IIf<Where<FeatureInstalled<FeaturesSet.inventory>>, True, False>))]
		protected virtual void _(Events.CacheAttached<InventoryItem.stkItem> eventArgs)
		{
		}

		[PXDefault()]
		[TemplateInventoryRaw(IsKey = true, Filterable = true)]
		protected virtual void _(Events.CacheAttached<InventoryItem.inventoryCD> eventArgs)
		{
		}

		[TemplateInventory(IsKey = true, DirtyRead = true)]
		[PXParent(typeof(INItemCategory.FK.InventoryItem))]
		[PXDBLiteDefault(typeof(InventoryItem.inventoryID))]
		protected virtual void _(Events.CacheAttached<INItemCategory.inventoryID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDefault(true)]
		protected virtual void _(Events.CacheAttached<InventoryItem.isTemplate> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[DefaultConditional(typeof(InventoryItem.isTemplate), true)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Required), true)]
		protected virtual void _(Events.CacheAttached<InventoryItem.defaultRowMatrixAttributeID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[DefaultConditional(typeof(InventoryItem.isTemplate), true)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Required), true)]
		protected virtual void _(Events.CacheAttached<InventoryItem.defaultColumnMatrixAttributeID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(INItemTypes.FinishedGood,
			typeof(Search<INItemClass.itemType, Where<INItemClass.itemClassID, Equal<Current<INItemClass.parentItemClassID>>, And<INItemClass.stkItem, Equal<Current<INItemClass.stkItem>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<INItemClass.itemType> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(typeof(Where<INItemClass.stkItem, Equal<Current<InventoryItem.stkItem>>>), Messages.StkItemSettingMustCoincide)]
		protected virtual void _(Events.CacheAttached<InventoryItem.itemClassID> eventArgs)
		{
		}

		[DefaultConditional(typeof(Search<INItemClass.lotSerClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>),
			typeof(InventoryItem.stkItem), true)]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<InventoryItem.lotSerClassID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(INPostClass.postClassID), DescriptionField = typeof(INPostClass.descr))]
		[DefaultConditional(typeof(Search<INItemClass.postClassID, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>),
			typeof(InventoryItem.stkItem), true)]
		protected virtual void _(Events.CacheAttached<InventoryItem.postClassID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search<InventoryItem.stkItem>))]
		protected virtual void _(Events.CacheAttached<INItemClass.stkItem> eventArgs)
		{
		}

		[TemplateInventoryAccount(true, typeof(InventoryItem.invtAcctID), DisplayName = "Inventory Account",
			Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.IN)]
		[PXDefault(typeof(Search<INPostClass.invtAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void _(Events.CacheAttached<InventoryItem.invtAcctID> e)
		{
		}

		[TemplateInventorySubAccount(true, typeof(InventoryItem.invtSubID), typeof(InventoryItem.invtAcctID), DisplayName = "Inventory Sub.",
			Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.invtSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<InventoryItem.invtSubID> e)
		{
		}

		[TemplateInventoryAccount(true, typeof(InventoryItem.cOGSAcctID), DisplayName = "COGS Account",
			Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.cOGSAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<InventoryItem.cOGSAcctID> e)
		{
		}

		[TemplateInventorySubAccount(true, typeof(InventoryItem.cOGSSubID), typeof(InventoryItem.cOGSAcctID),
			DisplayName = "COGS Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<InventoryItem.cOGSSubID> e)
		{
		}

		[TemplateInventoryAccount(false, typeof(InventoryItem.invtAcctID), DisplayName = "Expense Accrual Account",
			DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.invtAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual void _(Events.CacheAttached<InventoryItem.expenseAccrualAcctID> e)
		{
		}

		[TemplateInventorySubAccount(false, typeof(InventoryItem.invtSubID), typeof(InventoryItem.invtAcctID), DisplayName = "Expense Accrual Sub.", DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.invtSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<InventoryItem.expenseAccrualSubID> e)
		{
		}

		[DefaultConditional(typeof(Search<INPostClass.cOGSAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>),
			typeof(InventoryItem.stkItem), false)]
		[TemplateInventoryAccount(false, typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Account",
			DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		protected virtual void _(Events.CacheAttached<InventoryItem.expenseAcctID> e)
		{
		}

		[DefaultConditional(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>),
			typeof(InventoryItem.stkItem), false)]
		[TemplateInventorySubAccount(false, typeof(InventoryItem.cOGSSubID), typeof(InventoryItem.cOGSAcctID),
			DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
		protected virtual void _(Events.CacheAttached<InventoryItem.expenseSubID> e)
		{
		}

		#endregion // Cache Attached

		#region Generation Rule Events

		protected virtual void _(Events.RowInserted<IDGenerationRule> eventArgs)
		{
			GenerationRuleRowInserted(eventArgs.Cache, eventArgs.Row);
			ResetValue<InventoryItem.sampleID>();
		}

		protected virtual void _(Events.RowInserted<DescriptionGenerationRule> eventArgs)
		{
			GenerationRuleRowInserted(eventArgs.Cache, eventArgs.Row);
			ResetValue<InventoryItem.sampleDescription>();
		}

		protected virtual void GenerationRuleRowInserted(PXCache cache, INMatrixGenerationRule row)
		{
			row.SortOrder = row.LineNbr;
		}

		protected virtual void _(Events.RowUpdated<IDGenerationRule> eventArgs)
			=> ResetValue<InventoryItem.sampleID>();

		protected virtual void _(Events.RowUpdated<DescriptionGenerationRule> eventArgs)
			=> ResetValue<InventoryItem.sampleDescription>();

		protected virtual void _(Events.RowDeleted<IDGenerationRule> eventArgs)
			=> ResetValue<InventoryItem.sampleID>();

		protected virtual void _(Events.RowDeleted<DescriptionGenerationRule> eventArgs)
			=> ResetValue<InventoryItem.sampleDescription>();

		public void SampleIDFieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			args.ReturnState = PXStringState.CreateInstance(args.ReturnState, null, true, SampleField, false, 0, null, null, null, null, null);
			args.ReturnValue = GetGenerationRuleSample<InventoryItem.sampleID, IDGenerationRule>(
				IDGenerationRules, Messages.SampleInventoryID);
		}

		public void SampleDescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			args.ReturnState = PXStringState.CreateInstance(args.ReturnState, null, true, SampleField, false, 0, null, null, null, null, null);
			args.ReturnValue = GetGenerationRuleSample<InventoryItem.sampleDescription, DescriptionGenerationRule>(
				DescriptionGenerationRules, Messages.SampleInventoryDescription);
		}

		public PXAction<InventoryItem> IdRowUp;
		[PXUIField(DisplayName = ActionsMessages.RowUp, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp, Tooltip = ActionsMessages.ttipRowUp)]
		public virtual IEnumerable idRowUp(PXAdapter adapter)
		{
			MoveCurrentRow(IDGenerationRules.Cache, IDGenerationRules.SelectMain(), true);
			IDGenerationRules.View.RequestRefresh();
			return adapter.Get();
		}

		public PXAction<InventoryItem> IdRowDown;
		[PXUIField(DisplayName = ActionsMessages.RowDown, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown, Tooltip = ActionsMessages.ttipRowDown)]
		public virtual IEnumerable idRowDown(PXAdapter adapter)
		{
			MoveCurrentRow(IDGenerationRules.Cache, IDGenerationRules.SelectMain(), false);
			IDGenerationRules.View.RequestRefresh();
			return adapter.Get();
		}

		public PXAction<InventoryItem> DescriptionRowUp;
		[PXUIField(DisplayName = ActionsMessages.RowUp, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowUp, Tooltip = ActionsMessages.ttipRowUp)]
		public virtual IEnumerable descriptionRowUp(PXAdapter adapter)
		{
			MoveCurrentRow(DescriptionGenerationRules.Cache, DescriptionGenerationRules.SelectMain(), true);
			DescriptionGenerationRules.View.RequestRefresh();
			return adapter.Get();
		}

		public PXAction<InventoryItem> DescriptionRowDown;
		[PXUIField(DisplayName = ActionsMessages.RowDown, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.ArrowDown, Tooltip = ActionsMessages.ttipRowDown)]
		public virtual IEnumerable descriptionRowDown(PXAdapter adapter)
		{
			MoveCurrentRow(DescriptionGenerationRules.Cache, DescriptionGenerationRules.SelectMain(), false);
			DescriptionGenerationRules.View.RequestRefresh();
			return adapter.Get();
		}

		public void MoveCurrentRow(PXCache cache, IEnumerable<INMatrixGenerationRule> allRows, bool up)
		{
			INMatrixGenerationRule currentLine = (INMatrixGenerationRule)cache.Current;
			if (currentLine == null)
				return;

			INMatrixGenerationRule nextLine;

			if (up)
				nextLine = allRows.Where(r => r.SortOrder < currentLine.SortOrder).OrderByDescending(r => r.SortOrder).FirstOrDefault();
			else
				nextLine = allRows.Where(r => r.SortOrder > currentLine.SortOrder).OrderBy(r => r.SortOrder).FirstOrDefault();

			if (nextLine == null)
				return;

			int? currentLineNbr = currentLine.SortOrder;
			int? nextLineNbr = nextLine.SortOrder;

			nextLine.SortOrder = currentLineNbr;
			currentLine.SortOrder = nextLineNbr;

			nextLine = (INMatrixGenerationRule)cache.Update(nextLine);
			currentLine = (INMatrixGenerationRule)cache.Update(currentLine);

			cache.Current = currentLine;
		}


		protected virtual void _(Events.RowSelected<IDGenerationRule> eventArgs)
			=> GenerationRuleRowSelected(eventArgs.Cache, eventArgs.Row);

		protected virtual void _(Events.RowSelected<DescriptionGenerationRule> eventArgs)
			=> GenerationRuleRowSelected(eventArgs.Cache, eventArgs.Row);

		protected virtual void GenerationRuleRowSelected(PXCache cache, INMatrixGenerationRule row)
		{
			if (row == null)
				return;

			bool isAttriubte = row.SegmentType.IsIn(INMatrixGenerationRule.segmentType.AttributeCaption, IDGenerationRule.segmentType.AttributeValue);
			bool isConstant = row.SegmentType == INMatrixGenerationRule.segmentType.Constant;
			bool isAutonumber = row.SegmentType == INMatrixGenerationRule.segmentType.AutoNumber;

			cache.Adjust<PXUIFieldAttribute>(row)
				.For<INMatrixGenerationRule.attributeID>(a => a.Enabled = isAttriubte)
				.For<INMatrixGenerationRule.constant>(a => a.Enabled = isConstant)
				.For<IDGenerationRule.numberingID>(a => a.Enabled = isAutonumber)
				.For<INMatrixGenerationRule.separator>(a => a.Enabled = row.UseSpaceAsSeparator != true);
		}


		protected virtual void _(Events.FieldUpdated<IDGenerationRule, IDGenerationRule.segmentType> eventArgs)
			=> GenerationRuleSegmentUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void _(Events.FieldUpdated<DescriptionGenerationRule, IDGenerationRule.segmentType> eventArgs)
			=> GenerationRuleSegmentUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void GenerationRuleSegmentUpdated(PXCache cache, INMatrixGenerationRule row)
		{
			if (row == null)
				return;

			switch(row.SegmentType)
			{
				case INMatrixGenerationRule.segmentType.TemplateDescription:
					row.NumberOfCharacters = ItemSettings.Current?.Descr?.Length;
					row.Constant = null;
					row.NumberingID = null;
					row.AttributeID = null;
					break;
				case INMatrixGenerationRule.segmentType.TemplateID:
					row.NumberOfCharacters = ItemSettings.Current?.InventoryCD?.Trim().Length;
					row.Constant = null;
					row.NumberingID = null;
					row.AttributeID = null;
					break;
				case INMatrixGenerationRule.segmentType.AttributeCaption:
				case INMatrixGenerationRule.segmentType.AttributeValue:
					row.Constant = null;
					row.NumberingID = null;
					GenerationRuleAttributeUpdated(cache, row);
					break;
				case INMatrixGenerationRule.segmentType.Constant:
					row.AttributeID = null;
					row.NumberingID = null;
					break;
				case INMatrixGenerationRule.segmentType.AutoNumber:
					row.Constant = null;
					row.AttributeID = null;
					break;
				case INMatrixGenerationRule.segmentType.Space:
					row.NumberOfCharacters = 1;
					row.Constant = null;
					row.NumberingID = null;
					row.AttributeID = null;
					break;
				default:
					row.Constant = null;
					row.NumberingID = null;
					row.AttributeID = null;
					break;
			}
		}

		protected virtual void _(Events.FieldUpdated<IDGenerationRule, IDGenerationRule.attributeID> eventArgs)
			=> GenerationRuleAttributeUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void _(Events.FieldUpdated<DescriptionGenerationRule, IDGenerationRule.attributeID> eventArgs)
			=> GenerationRuleAttributeUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void GenerationRuleAttributeUpdated(PXCache cache, INMatrixGenerationRule row)
		{
			if (row?.AttributeID == null)
				return;

			var attribute = CRAttribute.Attributes[row?.AttributeID];

			int? length = 0;

			length = (row.SegmentType == INMatrixGenerationRule.segmentType.AttributeValue) ?
				attribute.Values.Max(v => v.ValueID?.Length) : attribute.Values.Max(v => v.Description?.Length);

			row.NumberOfCharacters = length;
		}

		protected virtual void _(Events.FieldUpdated<IDGenerationRule, IDGenerationRule.constant> eventArgs)
			=> GenerationRuleConstantUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void _(Events.FieldUpdated<DescriptionGenerationRule, IDGenerationRule.constant> eventArgs)
			=> GenerationRuleConstantUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void GenerationRuleConstantUpdated(PXCache cache, INMatrixGenerationRule row)
		{
			row.NumberOfCharacters = row?.Constant?.Length;
		}

		protected virtual void _(Events.FieldUpdated<IDGenerationRule, IDGenerationRule.numberingID> eventArgs)
			=> GenerationRuleNumberingUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void _(Events.FieldUpdated<DescriptionGenerationRule, IDGenerationRule.numberingID> eventArgs)
			=> GenerationRuleNumberingUpdated(eventArgs.Cache, eventArgs.Row);

		protected virtual void GenerationRuleNumberingUpdated(PXCache cache, INMatrixGenerationRule row)
		{
			if (row?.NumberingID == null)
				return;

			row.NumberOfCharacters = new PXSelect<NumberingSequence,
				Where<NumberingSequence.numberingID, Equal<Required<Numbering.numberingID>>>>(this)
				.SelectMain(row.NumberingID)
				.Max(s => s.EndNbr?.Length);
		}

		#endregion // Generation Rule Events

		#region Overrides

		public override bool IsStockItemFlag => ItemSettings.Current?.StkItem == true;

		#endregion // Overrides

		#region Event Handlers

		protected virtual void _(Events.FieldSelecting<InventoryItem, InventoryItem.hasChild> eventArgs)
		{
			if (eventArgs.Row?.InventoryID > 0 && eventArgs.Row.IsTemplate == true && eventArgs.Row.HasChild == null)
			{
				var childSelect = new PXSelectReadonly<InventoryItem,
					Where<InventoryItem.templateItemID, Equal<Required<InventoryItem.inventoryID>>>>(this);

				using (new PXFieldScope(childSelect.View, typeof(InventoryItem.inventoryID)))
				{
					InventoryItem child = childSelect.Select(eventArgs.Row.InventoryID);
					eventArgs.Row.HasChild = (child != null);
				}
			}

			eventArgs.ReturnValue = eventArgs.Row?.HasChild;
		}

		protected override void InventoryItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.InventoryItem_RowSelected(sender, e);

			InventoryItem row = (InventoryItem)e.Row;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<InventoryItem.cOGSSubID>(sender, row, (postclass.Current != null && postclass.Current.COGSSubFromSales == false));
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstVarAcctID>(sender, row, row?.ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstVarSubID>(sender, row, row?.ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstRevAcctID>(sender, row, row?.ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.stdCstRevSubID>(sender, row, row?.ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.pendingStdCost>(sender, row, row?.ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetEnabled<InventoryItem.pendingStdCostDate>(sender, row, row?.ValMethod == INValMethod.Standard);
			PXUIFieldAttribute.SetVisible<InventoryItem.defaultSubItemOnEntry>(sender, null, insetup.Current.UseInventorySubItem == true);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.isDefault>(this.VendorItems.Cache, null, true);
			INAcctSubDefault.Required(sender, new PXRowSelectedEventArgs(row));

			PXUIFieldAttribute.SetVisible<InventoryItem.defaultSubItemOnEntry>(sender, null, insetup.Current.UseInventorySubItem == true);

			Boxes.Cache.AllowInsert = row.PackageOption != INPackageOption.Manual && PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>();
			Boxes.Cache.AllowUpdate = row.PackageOption != INPackageOption.Manual && PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>();
			Boxes.Cache.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>();

			if (row.PackageOption == INPackageOption.Quantity)
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, false);
			}
			else if (row.PackageOption == INPackageOption.Weight)
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, false);
			}
			else if (row.PackageOption == INPackageOption.WeightAndVolume)
			{
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, true);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, true);

			}
			else if (row.PackageOption == INPackageOption.Manual)
			{
				Boxes.Cache.AllowSelect = false;
				PXUIFieldAttribute.SetEnabled<InventoryItem.packSeparately>(Item.Cache, Item.Current, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.qty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.uOM>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxQty>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxWeight>(Boxes.Cache, null, false);
				PXUIFieldAttribute.SetVisible<INItemBoxEx.maxVolume>(Boxes.Cache, null, false);

			}

			if (PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>())
				ValidatePackaging(row);

			FieldsDependOnStkItemFlag(sender, row);

			var hasChildObject = sender.GetValueExt<InventoryItem.hasChild>(row);
			bool hasChild = ((hasChildObject is PXFieldState s) ? (bool?)s.Value : (bool?)hasChildObject) == true;
			sender.Adjust<PXUIFieldAttribute>().For<InventoryItem.itemClassID>(a => a.Enabled = !hasChild)
				.SameFor<InventoryItem.stkItem>()
				.SameFor<InventoryItem.baseUnit>()
				.SameFor<InventoryItem.decimalBaseUnit>()
				.SameFor<InventoryItem.purchaseUnit>()
				.SameFor<InventoryItem.decimalPurchaseUnit>()
				.SameFor<InventoryItem.salesUnit>()
				.SameFor<InventoryItem.decimalSalesUnit>();
		}

		protected virtual void _(Events.RowPersisting<InventoryItem> eventArgs)
		{
			if (eventArgs.Row?.IsTemplate == true)
			{
				ValidateChangeStkItemFlag();
				ValidateMainFieldsAreNotChanged(eventArgs.Cache, eventArgs.Row);

				if (eventArgs.Row.StkItem != true)
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
					{
						eventArgs.Row.NonStockReceipt = false;
						eventArgs.Row.NonStockShip = false;
					}

					if (eventArgs.Row.NonStockReceipt == true && string.IsNullOrEmpty(eventArgs.Row.PostClassID))
					{
						throw new PXRowPersistingException(nameof(InventoryItem.postClassID),
							eventArgs.Row.PostClassID, ErrorMessages.FieldIsEmpty, nameof(InventoryItem.postClassID));
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> eventArgs)
		{
			var row = eventArgs.Row;
			if (row != null && row.ItemClassID < 0)
			{
				INItemClass ic = ItemClass.Select();
				row.ParentItemClassID = ic?.ParentItemClassID;
			}
			else if (row != null)
			{
				row.ParentItemClassID = row.ItemClassID;
			}

			if (doResetDefaultsOnItemClassChange)
			{
				eventArgs.Cache.SetDefaultExt<InventoryItem.postClassID>(row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.priceClassID>(row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.priceWorkgroupID>(row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.priceManagerID>(row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.markupPct>(row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.minGrossProfitPct>(row);

				INItemClass ic = ItemClass.Select();
				if (ic != null)
				{
					eventArgs.Cache.SetValue<InventoryItem.priceWorkgroupID>(row, ic.PriceWorkgroupID);
					eventArgs.Cache.SetValue<InventoryItem.priceManagerID>(row, ic.PriceManagerID);
				}

				eventArgs.Cache.SetDefaultExt<InventoryItem.lotSerClassID>(row);

				ResetConversionsSettings(eventArgs.Cache, row);

				eventArgs.Cache.SetDefaultExt<InventoryItem.dfltSiteID>(row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.valMethod>(row);

				eventArgs.Cache.SetDefaultExt<InventoryItem.taxCategoryID>(row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.itemType>(row);
			}

			if (row != null && row.ItemClassID != null && eventArgs.ExternalCall)
			{
				Answers.Cache.Clear();
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.postClassID> eventArgs)
		{
			eventArgs.Cache.SetDefaultExt<InventoryItem.invtAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.invtSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.salesAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.salesSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.cOGSAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.cOGSSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.stdCstVarAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.stdCstVarSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.stdCstRevAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.stdCstRevSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.pPVAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.pPVSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.pOAccrualAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.pOAccrualSubID>(eventArgs.Row);

			eventArgs.Cache.SetDefaultExt<InventoryItem.reasonCodeSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.lCVarianceAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.lCVarianceSubID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.deferralAcctID>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<InventoryItem.deferralSubID>(eventArgs.Row);

			if (eventArgs.Row != null && eventArgs.Row.StkItem != true)
			{
				eventArgs.Cache.SetDefaultExt<InventoryItem.expenseAccrualAcctID>(eventArgs.Row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.expenseAccrualSubID>(eventArgs.Row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.expenseAcctID>(eventArgs.Row);
				eventArgs.Cache.SetDefaultExt<InventoryItem.expenseSubID>(eventArgs.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.dfltSiteID> eventArgs)
		{
			INSite site = INSite.PK.Find(this, ((InventoryItem)eventArgs.Row).DfltSiteID);

			eventArgs.Row.DfltShipLocationID = site?.ShipLocationID;
			eventArgs.Row.DfltReceiptLocationID = site?.ReceiptLocationID;
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.stkItem> eventArgs)
		{
			eventArgs.Cache.SetValueExt<InventoryItem.itemType>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.itemClassID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.invtAcctID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.invtSubID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.cOGSAcctID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.cOGSSubID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.expenseAccrualAcctID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.expenseAccrualSubID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.expenseAcctID>(eventArgs.Row, null);
			eventArgs.Cache.SetValueExt<InventoryItem.expenseSubID>(eventArgs.Row, null);
			eventArgs.Cache.SetDefaultExt<InventoryItem.valMethod>(eventArgs.Row);
		}

		#endregion // Event handlers

		#region Methods

		public virtual CreateMatrixItemsHelper GetCreateMatrixItemsHelper()
			=> new CreateMatrixItemsHelper(this);

		public virtual AttributeGroupHelper GetAttributeGroupHelper()
			=> new AttributeGroupHelper(this);

		protected virtual void ValidateChangeStkItemFlag()
		{
			var childWithDifferentStkItemValue = (InventoryItem)new PXSelect<InventoryItem,
				Where<InventoryItem.templateItemID, Equal<Current<InventoryItem.inventoryID>>,
					And<InventoryItem.stkItem, NotEqual<Current<InventoryItem.stkItem>>>>>(this).Select();

			if (childWithDifferentStkItemValue != null)
			{
				throw new PXSetPropertyException<InventoryItem.stkItem>(Messages.ItIsNotAllowedToChangeStkItemFlagIfChildExists);
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.defaultColumnMatrixAttributeID> e)
			=> GetAttributeGroupHelper().Recalculate(e.Row);

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.defaultRowMatrixAttributeID> e)
			=> GetAttributeGroupHelper().Recalculate(e.Row);

		protected virtual void ValidateMainFieldsAreNotChanged(PXCache cache, InventoryItem row)
		{
			bool hasChild = ReloadHasChild();

			if (!hasChild)
				return;
			
			InventoryItem oldRow = PXSelectReadonly<InventoryItem,
				Where<InventoryItem.inventoryID, Equal<Current<InventoryItem.inventoryID>>>>.Select(this);

			if (oldRow != null)
			{
				if (!cache.ObjectsEqual<InventoryItem.itemClassID,
					InventoryItem.baseUnit, InventoryItem.decimalBaseUnit,
					InventoryItem.salesUnit, InventoryItem.decimalSalesUnit,
					InventoryItem.purchaseUnit, InventoryItem.decimalPurchaseUnit>(oldRow, row))
					throw new PXException(Messages.ItIsNotAllowedToChangeMainFieldsIfChildExists);					
			}
		}

		protected virtual void ValidatePackaging(InventoryItem row)
		{
			PXUIFieldAttribute.SetError<InventoryItem.weightUOM>(Item.Cache, row, null);
			PXUIFieldAttribute.SetError<InventoryItem.baseItemWeight>(Item.Cache, row, null);
			PXUIFieldAttribute.SetError<InventoryItem.volumeUOM>(Item.Cache, row, null);
			PXUIFieldAttribute.SetError<InventoryItem.baseItemVolume>(Item.Cache, row, null);

			//validate weight & volume:
			if (row.PackageOption == INPackageOption.Weight || row.PackageOption == INPackageOption.WeightAndVolume)
			{
				if (string.IsNullOrEmpty(row.WeightUOM))
					Item.Cache.RaiseExceptionHandling<InventoryItem.weightUOM>(row, row.WeightUOM, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));

				if (row.BaseItemWeight <= 0)
					Item.Cache.RaiseExceptionHandling<InventoryItem.baseItemWeight>(row, row.BaseItemWeight, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));

				if (row.PackageOption == INPackageOption.WeightAndVolume)
				{
					if (string.IsNullOrEmpty(row.VolumeUOM))
						Item.Cache.RaiseExceptionHandling<InventoryItem.volumeUOM>(row, row.VolumeUOM, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));

					if (row.BaseItemVolume <= 0)
						Item.Cache.RaiseExceptionHandling<InventoryItem.baseItemVolume>(row, row.BaseItemVolume, new PXSetPropertyException(Messages.ValueIsRequiredForAutoPackage, PXErrorLevel.Warning));
				}
			}

			//validate boxes:
			foreach (INItemBoxEx box in Boxes.Select())
			{
				PXUIFieldAttribute.SetError<INItemBoxEx.boxID>(Boxes.Cache, box, null);
				PXUIFieldAttribute.SetError<INItemBoxEx.maxQty>(Boxes.Cache, box, null);

				if ((row.PackageOption == INPackageOption.Weight || row.PackageOption == INPackageOption.WeightAndVolume) && box.MaxWeight.GetValueOrDefault() == 0)
				{
					Boxes.Cache.RaiseExceptionHandling<INItemBoxEx.boxID>(box, box.BoxID, new PXSetPropertyException(Messages.MaxWeightIsNotDefined, PXErrorLevel.Warning));
				}

				if (row.PackageOption == INPackageOption.WeightAndVolume && box.MaxVolume.GetValueOrDefault() == 0)
				{
					Boxes.Cache.RaiseExceptionHandling<INItemBoxEx.boxID>(box, box.BoxID, new PXSetPropertyException(Messages.MaxVolumeIsNotDefined, PXErrorLevel.Warning));
				}

				if ((row.PackageOption == INPackageOption.Weight || row.PackageOption == INPackageOption.WeightAndVolume) &&
					(box.MaxWeight.GetValueOrDefault() < row.BaseItemWeight.GetValueOrDefault() ||
					(box.MaxVolume > 0 && row.BaseItemVolume > box.MaxVolume)))
				{
					Boxes.Cache.RaiseExceptionHandling<INItemBoxEx.boxID>(box, box.BoxID, new PXSetPropertyException(Messages.ItemDontFitInTheBox, PXErrorLevel.Warning));
				}

			}
		}

		protected virtual bool ReloadHasChild()
		{
			var childSelect = new PXSelectReadonly<InventoryItem,
				Where<InventoryItem.templateItemID, Equal<Current<InventoryItem.inventoryID>>>>(this);

			childSelect.Cache.ClearQueryCache();

			using (new PXFieldScope(childSelect.View, typeof(InventoryItem.inventoryID)))
			{
				InventoryItem child = childSelect.Select();
				return (child != null);
			}
		}

		protected virtual void ResetValue<Field>()
			where Field : IBqlField
		{
			Item.Cache.SetValue<Field>(Item.Current, null);
		}

		protected virtual string GetGenerationRuleSample<Field, Rule>(PXSelectBase<Rule> view, string userMessage)
			where Field : IBqlField
			where Rule : INMatrixGenerationRule, new()
		{
			if (Item.Current == null)
				return null;

			string oldValue = (string)Item.Cache.GetValue<Field>(Item.Current);

			if (oldValue != null)
				return oldValue;

			var value = GetGenerationRuleSample(view.SelectMain());
			var userValue = PXLocalizer.LocalizeFormat(userMessage, value);
			Item.Cache.SetValue<Field>(Item.Current, userValue);
			view.View.RequestRefresh();

			return userValue;
		}

		protected virtual string GetGenerationRuleSample(IEnumerable<INMatrixGenerationRule> rules)
		{
			try
			{
				var helper = GetCreateMatrixItemsHelper();

				var tempItem = new MatrixInventoryItem();
				tempItem.TemplateItemID = Item.Current.TemplateItemID;

				tempItem.AttributeIDs = Answers.SelectMain().Select(a => a.AttributeID).ToArray();
				tempItem.AttributeValues = new string[tempItem.AttributeIDs.Length];
				tempItem.AttributeValueDescrs = new string[tempItem.AttributeIDs.Length];

				for (int attributeIndex = 0; attributeIndex < tempItem.AttributeIDs.Length; attributeIndex++)
				{
					var attributeID = tempItem.AttributeIDs[attributeIndex];
					var values = CRAttribute.Attributes[attributeID].Values;
					var value = values.Where(v => !v.Disabled).FirstOrDefault();
					tempItem.AttributeValues[attributeIndex] = value?.ValueID;
					tempItem.AttributeValueDescrs[attributeIndex] = value?.Description;
				}

				return helper.GenerateMatrixItemID(Item.Current, rules.ToList(), tempItem, true);
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}

		#endregion // Methods

		#region Show/Hide Fields by Stock Item Flag

		protected virtual void FieldsDependOnStkItemFlag(PXCache cache, InventoryItem row)
		{
			bool isStock = row.StkItem == true;

			ItemTypeValuesDependOnStkItemFlag(cache, row, isStock);
			GeneralSettingsFieldsDependOnStkItemFlag(cache, row, isStock);
			FulfillmentFieldsDependOnStkItemFlag(cache, row, isStock);
			PriceCostInfoFieldsDependOnStkItemFlag(cache, row, isStock);
			VendorDetailsFieldsDependOnStkItemFlag(row, isStock);
			GLAccountsFieldsDependOnStkItemFlag(cache, row, isStock);
		}

		protected virtual void ItemTypeValuesDependOnStkItemFlag(PXCache cache, InventoryItem row, bool isStock)
		{
			INItemTypes.CustomListAttribute strings = isStock ? 
				(INItemTypes.CustomListAttribute)new INItemTypes.StockListAttribute() : new INItemTypes.NonStockListAttribute();

			PXStringListAttribute.SetList<InventoryItem.itemType>(cache, row, strings.AllowedValues, strings.AllowedLabels);
		}

		protected virtual void GeneralSettingsFieldsDependOnStkItemFlag(PXCache cache, InventoryItem row, bool isStock)
		{
			cache.Adjust<PXUIFieldAttribute>(row).For<InventoryItem.valMethod>(fa => fa.Visible = isStock)
				.SameFor<InventoryItem.lotSerClassID>()
				.SameFor<InventoryItem.countryOfOrigin>()
				.SameFor<InventoryItem.cycleID>()
				.SameFor<InventoryItem.aBCCodeID>()
				.SameFor<InventoryItem.aBCCodeIsFixed>()
				.SameFor<InventoryItem.movementClassID>()
				.SameFor<InventoryItem.movementClassIsFixed>()
				.SameFor<InventoryItem.dfltShipLocationID>()
				.SameFor<InventoryItem.dfltReceiptLocationID>()
				.SameFor<InventoryItem.defaultSubItemID>();

			cache.Adjust<PXUIFieldAttribute>(row).For<InventoryItem.completePOLine>(fa => fa.Visible = !isStock)
				.SameFor<InventoryItem.nonStockReceipt>()
				.SameFor<InventoryItem.nonStockShip>();

			bool isService = row.ItemType == INItemTypes.ServiceItem;
			bool fieldServiceVisible = !isStock && isService && PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();

			cache.Adjust<PXUIFieldAttribute>(row).For("EstimatedDuration", fa => fa.Visible = fieldServiceVisible); // Field Service
			ItemClass.Cache.Adjust<PXUIFieldAttribute>().For("Mem_RouteService", fa => fa.Visible = fieldServiceVisible); // Field Service
		}

		protected virtual void FulfillmentFieldsDependOnStkItemFlag(PXCache cache, InventoryItem row, bool isStock)
		{
			cache.Adjust<PXUIFieldAttribute>(row).For<InventoryItem.hSTariffCode>(fa => fa.Visible = isStock)
				.SameFor<InventoryItem.packageOption>()
				.SameFor<InventoryItem.packSeparately>();
		}

		protected virtual void PriceCostInfoFieldsDependOnStkItemFlag(PXCache cache, InventoryItem row, bool isStock)
		{
			cache.Adjust<PXUIFieldAttribute>(row).For<InventoryItem.accrueCost>(fa => fa.Visible = !isStock)
				.SameFor<InventoryItem.costBasis>()
				.SameFor<InventoryItem.percentOfSalesPrice>()
				.SameFor("DfltEarningType"); // Field Service
		}

		protected virtual void VendorDetailsFieldsDependOnStkItemFlag(InventoryItem row, bool isStock)
		{
			Caches[typeof(CR.Standalone.Location)].Adjust<PXUIFieldAttribute>()
				.For<CR.Standalone.Location.vSiteID>(fa => fa.Visible = isStock)
				.SameFor<CR.Standalone.Location.vLeadTime>();
			
			VendorItems.Cache.Adjust<PXUIFieldAttribute>(VendorItems.Current).For<POVendorInventory.subItemID>(fa => fa.Visible = isStock)
				.SameFor<POVendorInventory.overrideSettings>()
				.SameFor<POVendorInventory.subItemID>()
				.SameFor<POVendorInventory.addLeadTimeDays>()
				.SameFor<POVendorInventory.minOrdFreq>()
				.SameFor<POVendorInventory.minOrdQty>()
				.SameFor<POVendorInventory.maxOrdQty>()
				.SameFor<POVendorInventory.lotSize>()
				.SameFor<POVendorInventory.eRQ>();
		}

		protected virtual void GLAccountsFieldsDependOnStkItemFlag(PXCache cache, InventoryItem row, bool isStock)
		{
			cache.Adjust<PXUIFieldAttribute>(row).For<InventoryItem.stdCstRevAcctID>(fa => fa.Visible = isStock)
				.SameFor<InventoryItem.stdCstRevSubID>()
				.SameFor<InventoryItem.stdCstVarAcctID>()
				.SameFor<InventoryItem.stdCstVarSubID>()
				.SameFor<InventoryItem.lCVarianceAcctID>()
				.SameFor<InventoryItem.lCVarianceSubID>()
				
				.SameFor<InventoryItem.invtAcctID>()
				.SameFor<InventoryItem.invtSubID>()
				.SameFor<InventoryItem.cOGSAcctID>()
				.SameFor<InventoryItem.cOGSSubID>();

			cache.Adjust<PXUIFieldAttribute>(row).For<InventoryItem.expenseAccrualAcctID>(fa => fa.Visible = !isStock)
				.SameFor<InventoryItem.expenseAccrualSubID>()
				.SameFor<InventoryItem.expenseAcctID>()
				.SameFor<InventoryItem.expenseSubID>();
		}

		#endregion // Show/Hide Fields by Stock Item Flag
	}
}
