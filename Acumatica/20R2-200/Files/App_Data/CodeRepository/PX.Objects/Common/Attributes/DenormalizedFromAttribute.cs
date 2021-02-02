using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;

namespace PX.Objects.Common.Attributes
{
	/// <summary>
	/// The attribute handles the fields which need to be denormalized from parent to children.
	/// </summary>
	public class DenormalizedFromAttribute : PXEventSubscriberAttribute
	{
		protected Type[] _parentFieldTypes;
		protected string[] _childFieldNames;
		protected object[] _defaultValues;
		protected Type _childToParentLinkField;

		protected string GetChildFieldName(int i) => _childFieldNames[i] ?? FieldName;

		/// <summary>
		/// Constructor of the attribute.
		/// </summary>
		/// <param name="parentFieldType">The field from parent which needs to be denormalized.</param>
		/// <param name="childToParentLinkField">The field which is the link from child to parent.</param>
		public DenormalizedFromAttribute(Type parentFieldType, Type childToParentLinkField = null)
		{
			if (!typeof(IBqlField).IsAssignableFrom(parentFieldType)
				|| !typeof(IBqlTable).IsAssignableFrom(BqlCommand.GetItemType(parentFieldType)))
			{
				throw new PXArgumentException(nameof(parentFieldType));
			}
			_parentFieldTypes = new[] { parentFieldType };
			_childFieldNames = new string[] { null };
			// TODO: SOCCPaymentEntry: check if this field can be obtained from child-parent relation
			_childToParentLinkField = childToParentLinkField;
		}

		/// <summary>
		/// Constructor of the attribute.
		/// </summary>
		/// <param name="parentTypes">The collection of the fields from parent which needs to be denormalized.</param>
		/// <param name="childTypes">The corresponding collection of the fields in children.</param>
		/// <param name="defaultValues">Default values for children in case if parent is not found.</param>
		/// <param name="childToParentLinkField">The field which is the link from child to parent.</param>
		public DenormalizedFromAttribute(
			Type[] parentTypes,
			Type[] childTypes,
			object[] defaultValues = null,
			Type childToParentLinkField = null)
		{
			if (parentTypes == null || parentTypes.Length == 0)
			{
				throw new PXArgumentException(nameof(parentTypes));
			}
			if (childTypes == null || childTypes.Length != parentTypes.Length)
			{
				throw new PXArgumentException(nameof(childTypes));
			}
			if (defaultValues != null && defaultValues.Length != parentTypes.Length)
			{
				throw new PXArgumentException(nameof(defaultValues));
			}

			_parentFieldTypes = parentTypes;
			_childFieldNames = childTypes.Select(t => t.Name).ToArray();
			_defaultValues = defaultValues;
			// TODO: SOCCPaymentEntry: check if this field can be obtained from child-parent relation
			_childToParentLinkField = childToParentLinkField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			Type parentField0 = _parentFieldTypes[0];
			sender.Graph.RowUpdated.AddHandler(BqlCommand.GetItemType(parentField0), Parent_RowUpdated);

			for (int i = 0; i < _childFieldNames.Length; i++)
			{
				int index = i;
				sender.Graph.FieldDefaulting.AddHandler(this.BqlTable, GetChildFieldName(index),
					(cache, e) => Child_FieldDefaulting(index, cache, e));
			}

			if (_childToParentLinkField != null)
			{
				sender.Graph.FieldUpdated.AddHandler(
					BqlCommand.GetItemType(_childToParentLinkField),
					_childToParentLinkField.Name,
					Child_ParentLinkUpdated);
			}
		}

		protected virtual object GetParentValue(Type parentField, PXCache sender, object child)
		{
			Type parentType = BqlCommand.GetItemType(parentField);
			object parent = PXParentAttribute.SelectParent(sender, child, parentType);
			PXCache parentCache = sender.Graph.Caches[parentType];
			return parentCache.GetValue(parent, parentField.Name);
		}

		public virtual void Child_ParentLinkUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			for (int i = 0; i < _parentFieldTypes.Length; i++)
			{
				sender.SetValueExt(e.Row, GetChildFieldName(i), GetParentValue(_parentFieldTypes[i], sender, e.Row));
			}
		}

		public virtual void Child_FieldDefaulting(int index, PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Type parentField = _parentFieldTypes[index];
			e.NewValue = GetParentValue(parentField, sender, e.Row) ?? _defaultValues?[index];
			e.Cancel = true;
		}

		public virtual void Parent_RowUpdated(PXCache parentCache, PXRowUpdatedEventArgs e)
		{
			bool fieldChanged = false;
			for (int i = 0; i < _parentFieldTypes.Length; i++)
			{
				if (!object.Equals(
						parentCache.GetValue(e.Row, _parentFieldTypes[i].Name),
						parentCache.GetValue(e.OldRow, _parentFieldTypes[i].Name)))
				{
					fieldChanged = true;
					break;
				}
			}

			if (fieldChanged)
			{
				PXCache childCache = parentCache.Graph.Caches[this.BqlTable];
				foreach (object child in PXParentAttribute.SelectChildren(childCache, e.Row, parentCache.GetItemType()))
				{
					object childCopy = childCache.CreateCopy(child);
					for (int i = 0; i < _parentFieldTypes.Length; i++)
					{
						object newValue = parentCache.GetValue(e.Row, _parentFieldTypes[i].Name);
						childCache.SetValue(childCopy, GetChildFieldName(i), newValue);
					}
					childCopy = childCache.Update(childCopy);
				}
			}
		}
	}
}
