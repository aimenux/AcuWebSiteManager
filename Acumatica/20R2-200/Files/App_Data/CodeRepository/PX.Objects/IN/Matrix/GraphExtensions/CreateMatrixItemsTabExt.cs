using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.IN.Matrix.Graphs;
using PX.Objects.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.IN.Matrix.Utility;
using PX.Objects.IN.Matrix.DAC;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public class CreateMatrixItemsTabExt : CreateMatrixItemsExt<TemplateInventoryItemMaint, InventoryItem>
	{
		public override void Initialize()
		{
			Base.Views.Caches.Remove(typeof(AdditionalAttributes));
			Base.Views.Caches.Remove(typeof(EntryMatrix));
			Base.Views.Caches.Remove(typeof(MatrixInventoryItem));

			base.Initialize();
		}

		public PXAction<InventoryItem> createUpdate;
		[PXUIField(DisplayName = "Confirmation", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public override IEnumerable CreateUpdate(PXAdapter adapter)
		{
			Base.Save.Press();
			return base.CreateUpdate(adapter);
		}

		protected override int? GetItemClassID()
		{
			return Base.Item.Current?.ItemClassID;
		}

		protected override int? GetTemplateID()
		{
			return Base.Item.Current?.InventoryID;
		}

		protected override InventoryItem GetTemplateItem()
		{
			return Base.Item.Current;
		}

		protected override void GetGenerationRules(CreateMatrixItemsHelper helper, out List<INMatrixGenerationRule> idGenerationRules, out List<INMatrixGenerationRule> descrGenerationRules)
		{
			idGenerationRules = Base.IDGenerationRules.SelectMain().Select(s => (INMatrixGenerationRule)s).ToList();
			descrGenerationRules = Base.DescriptionGenerationRules.SelectMain().Select(s => (INMatrixGenerationRule)s).ToList();
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> eventArgs)
		{
			RecalcAttributesGrid();
		}

		protected virtual void _(Events.RowSelected<EntryHeader> eventArgs)
		{
			PXUIFieldAttribute.SetEnabled<EntryHeader.rowAttributeID>(eventArgs.Cache, eventArgs.Row, Base.Item.Current?.DefaultRowMatrixAttributeID != null);
			PXUIFieldAttribute.SetEnabled<EntryHeader.colAttributeID>(eventArgs.Cache, eventArgs.Row, Base.Item.Current?.DefaultColumnMatrixAttributeID != null);
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.defaultColumnMatrixAttributeID> eventArgs)
		{
			Header.SetValueExt<EntryHeader.colAttributeID>(Header.Current, eventArgs.NewValue);
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.defaultRowMatrixAttributeID> eventArgs)
		{
			Header.SetValueExt<EntryHeader.rowAttributeID>(Header.Current, eventArgs.NewValue);
		}

		protected override void _(Events.RowSelected<InventoryItem> eventArgs)
		{
			base._(eventArgs);

			if (eventArgs.Row?.IsTemplate == true && Header.Current != null &&
				Header.Current.TemplateItemID != eventArgs.Row.InventoryID)
			{
				Header.Current.TemplateItemID = eventArgs.Row?.InventoryID;
				RecalcAttributesGrid();
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXDefault(typeof(InventoryItem.defaultColumnMatrixAttributeID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CSAttributeGroup.attributeID,
			Where<CSAttributeGroup.isActive, Equal<True>,
				And<CSAttributeGroup.entityClassID, Equal<Current<InventoryItem.itemClassID>>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>),
			typeof(CSAttributeGroup.attributeID), DescriptionField = typeof(CSAttributeGroup.description))]
		protected virtual void _(Events.CacheAttached<EntryHeader.colAttributeID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXDefault(typeof(InventoryItem.defaultRowMatrixAttributeID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CSAttributeGroup.attributeID,
			Where<CSAttributeGroup.isActive, Equal<True>,
				And<CSAttributeGroup.entityClassID, Equal<Current<InventoryItem.itemClassID>>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>),
			typeof(CSAttributeGroup.attributeID), DescriptionField = typeof(CSAttributeGroup.description))]
		protected virtual void _(Events.CacheAttached<EntryHeader.rowAttributeID> eventArgs)
		{
		}
	}
}
