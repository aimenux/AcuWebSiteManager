using PX.Common;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.GraphExtensions.INItemClassMaint
{
	public class VariantAttributesExt : PXGraphExtension<IN.INItemClassMaint>
	{
		protected Lazy<bool> _hasTemplateWithChild;
		protected Lazy<bool> _childItemClassHasTemplateWithItems;

		public override void Initialize()
		{
			_hasTemplateWithChild = new Lazy<bool>(HasTemplateWithChild);
			base.Initialize();
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.matrixItem>();
		}
		
		protected virtual void _(Events.RowSelected<CSAttributeGroup> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<CSAttributeGroup.attributeCategory>(eventArgs.Cache, eventArgs.Row,
				eventArgs.Row.ControlType == CSAttribute.Combo);

			PXUIFieldAttribute.SetEnabled<CSAttributeGroup.required>(eventArgs.Cache, eventArgs.Row,
				eventArgs.Row.AttributeCategory != CSAttributeGroup.attributeCategory.Variant);
		}

		protected virtual void _(Events.FieldUpdated<CSAttributeGroup, CSAttributeGroup.attributeID> eventArgs)
		{
			eventArgs.Cache.SetDefaultExt<CSAttributeGroup.attributeCategory>(eventArgs.Row);
		}

		protected virtual void _(Events.FieldUpdated<CSAttributeGroup, CSAttributeGroup.attributeCategory> eventArgs)
		{
			if (eventArgs.Row?.AttributeCategory == CSAttributeGroup.attributeCategory.Variant)
			{
				eventArgs.Row.Required = false;
			}
		}

		protected virtual void _(Events.RowPersisting<CSAttributeGroup> eventArgs)
		{
			var row = eventArgs.Row;
			if (row == null)
				return;

			var command = eventArgs.Operation & PXDBOperation.Command;

			switch(command)
			{
				case PXDBOperation.Insert:
					ValidateInsert(eventArgs.Cache, _hasTemplateWithChild, row);
					break;
				case PXDBOperation.Update:
					ValidateUpdate(eventArgs.Cache, _hasTemplateWithChild, row);
					break;
				case PXDBOperation.Delete:
					ValidateDelete(eventArgs.Cache, _hasTemplateWithChild, row, true);
					break;
			}
		}

		protected virtual void ValidateInsert(PXCache cache, Lazy<bool> hasTemplateWithChild, CSAttributeGroup row, bool throwException = false)
		{
			if (row.AttributeCategory == CSAttributeGroup.attributeCategory.Variant && row.IsActive == true && hasTemplateWithChild.Value)
			{
				var exception = new PXSetPropertyException<CSAttributeGroup.attributeCategory>(Messages.CantAddVariantAttributeForMatrixItem, row.AttributeID);

				if (!throwException)
					cache.RaiseExceptionHandling<CSAttributeGroup.attributeCategory>(row, row.AttributeCategory, exception);
				else
					throw exception;
			}
		}
		
		protected virtual void ValidateUpdate(PXCache cache, Lazy<bool> hasTemplateWithChild, CSAttributeGroup row, bool throwException = false, CSAttributeGroup oldRow = null)
		{
			string templateIDs = string.Empty;

			if (oldRow == null)
				oldRow = CSAttributeGroup.PK.Find(Base, row.AttributeID, row.EntityClassID, row.EntityType);

			if (oldRow != null && oldRow.AttributeCategory != row.AttributeCategory &&
				row.IsActive == true && (hasTemplateWithChild.Value || IsAttributeDefaultRowColumnAttribute(row, out templateIDs)))
			{
				var exception = hasTemplateWithChild.Value ?
					new PXSetPropertyException<CSAttributeGroup.attributeCategory>(Messages.CantChangeAttributeCategoryForMatrixItem) :
					new PXSetPropertyException<CSAttributeGroup.attributeCategory>(Messages.CantChangeAttributeCategoryForMatrixTemplate, templateIDs);

				if (!throwException)
					cache.RaiseExceptionHandling<CSAttributeGroup.attributeCategory>(row, row.AttributeCategory, exception);
				else
					throw exception;
			}

			if (oldRow?.AttributeCategory == CSAttributeGroup.attributeCategory.Variant &&
				oldRow.IsActive != row.IsActive && (hasTemplateWithChild.Value || IsAttributeDefaultRowColumnAttribute(row, out templateIDs)))
			{
				var exception = hasTemplateWithChild.Value ?
					new PXSetPropertyException<CSAttributeGroup.isActive>(Messages.CantChangeAttributeIsActiveFlagForMatrixItem) :
					new PXSetPropertyException<CSAttributeGroup.isActive>(Messages.CantChangeAttributeIsActiveFlagForMatrixTemplate, templateIDs);

				if (!throwException)
					cache.RaiseExceptionHandling<CSAttributeGroup.isActive>(row, row.IsActive, exception);
				else
					throw exception;
			}
		}

		protected virtual void ValidateDelete(PXCache cache, Lazy<bool> hasTemplateWithChild, CSAttributeGroup row, bool throwException = false)
		{
			string templateIDs = string.Empty;

			var oldRow = CSAttributeGroup.PK.Find(Base, row.AttributeID, row.EntityClassID, row.EntityType);

			if (oldRow?.AttributeCategory == CSAttributeGroup.attributeCategory.Variant && oldRow.IsActive == true &&
				(hasTemplateWithChild.Value || IsAttributeDefaultRowColumnAttribute(row, out templateIDs)))
			{
				var exception = hasTemplateWithChild.Value ?
					new PXSetPropertyException<CSAttributeGroup.attributeCategory>(Messages.CantDeleteVariantAttributeForMatrixItem, row.AttributeID) :
					new PXSetPropertyException<CSAttributeGroup.attributeCategory>(Messages.CantDeleteVariantAttributeForMatrixTemplate, row.AttributeID, templateIDs);

				if (!throwException)
					cache.RaiseExceptionHandling<CSAttributeGroup.attributeCategory>(row, row.AttributeCategory, exception);
				else
					throw exception;
			}
		}

		protected virtual bool IsAttributeDefaultRowColumnAttribute(CSAttributeGroup attributeGroup, out string templateIDs)
		{
			const int MaxTemplates = 10;
			const string Separator = ", ";

			if (attributeGroup?.EntityType != typeof(InventoryItem).FullName)
			{
				templateIDs = null;
				return false;
			}

			PXResultset<InventoryItem> template = PXSelect<InventoryItem,
				Where<InventoryItem.isTemplate, Equal<True>, And<InventoryItem.itemClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
				And<Where<InventoryItem.defaultColumnMatrixAttributeID, Equal<Required<CSAttributeGroup.attributeID>>,
					Or<InventoryItem.defaultRowMatrixAttributeID, Equal<Required<CSAttributeGroup.attributeID>>>>>>>>
				.SelectWindowed(Base, 0, MaxTemplates, attributeGroup.EntityClassID, attributeGroup.AttributeID, attributeGroup.AttributeID);

			if (template.Count == 0)
			{
				templateIDs = null;
				return false;
			}

			templateIDs = string.Join(Separator, template.RowCast<InventoryItem>().Select(s => s.InventoryCD));

			return true;
		}

		protected virtual void _(Events.RowDeleting<CSAttributeGroup> eventArgs)
		{
			string templateIDs = string.Empty;

			var row = eventArgs.Row;
			if (row == null)
				return;

			var oldRow = CSAttributeGroup.PK.Find(Base, row.AttributeID, row.EntityClassID, row.EntityType);
			if (oldRow?.AttributeCategory == CSAttributeGroup.attributeCategory.Variant)
			{
				if (_hasTemplateWithChild.Value)
					throw new PXSetPropertyException<CSAttributeGroup.attributeCategory>(Messages.CantDeleteVariantAttributeForMatrixItem, row.AttributeID);

				if (IsAttributeDefaultRowColumnAttribute(row, out templateIDs))
					throw new PXSetPropertyException<CSAttributeGroup.attributeCategory>(Messages.CantDeleteVariantAttributeForMatrixTemplate, row.AttributeID, templateIDs);
			}
		}

		protected virtual bool HasTemplateWithChild(int? itemClassID, bool clearQueryCache)
		{
			var childItemSelect = new PXSelectReadonly<InventoryItem,
				Where<InventoryItem.itemClassID, Equal<Required<IN.InventoryItem.itemClassID>>,
					And<InventoryItem.isTemplate, Equal<False>,
					And<InventoryItem.templateItemID, IsNotNull>>>>(Base);

			if (clearQueryCache)
				childItemSelect.Cache.ClearQueryCache();

			InventoryItem childItem = childItemSelect.SelectSingle(itemClassID);

			return childItem != null;
		}

		protected virtual bool HasTemplateWithChild()
			=> HasTemplateWithChild(Base.itemclass.Current.ItemClassID, true);

		[PXOverride]
		public virtual void MergeAttributes(INItemClass child, IEnumerable<CSAttributeGroup> attributesTemplate,
			Action<INItemClass, IEnumerable<CSAttributeGroup>> baseMethod)
		{
			int? childItemClassID = child.ItemClassID;
			_childItemClassHasTemplateWithItems = new Lazy<bool>(() => HasTemplateWithChild(childItemClassID, false));

			baseMethod(child, attributesTemplate);
		}

		[PXOverride]
		public virtual void MergeAttribute(INItemClass child, CSAttributeGroup existingAttribute, CSAttributeGroup attr,
			Action<INItemClass, CSAttributeGroup, CSAttributeGroup> baseMethod)
		{
			if (existingAttribute == null)
			{
				ValidateInsert(Base.Mapping.Cache, _childItemClassHasTemplateWithItems, attr, true);
			}
			else
			{
				ValidateUpdate(Base.Mapping.Cache, _childItemClassHasTemplateWithItems, attr, true, existingAttribute);
			}

			baseMethod.Invoke(child, existingAttribute, attr);
		}
	}
}
