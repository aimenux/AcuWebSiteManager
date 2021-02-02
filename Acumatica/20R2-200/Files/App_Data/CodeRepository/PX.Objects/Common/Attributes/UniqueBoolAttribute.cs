//This class was moved from \WebSites\Pure\PX.Objects\CA\Descriptor\Attribute.cs 
using System;
using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// Attribute ensures that there is only one row with "true" value of given field within scope restricted by key value
	/// Works only on boolean fields. Key field must have PXParent Attribute on it
	/// <param name="key">Key field with PXParentAttribute on it</param>
	/// <param name="groupByField">Field for grouping</param>
	/// </summary>
	public class UniqueBoolAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber
	{
		private Type _key;
		private Type _groupByField;
		private PXParentAttribute _parentAttribute;
		#region Ctor
		public UniqueBoolAttribute(Type key)
		{
			if (key == null)
			{
				throw new PXArgumentException("scope", Messages.ParameterShouldNotNull);
			}
			if (!(key.IsNested && typeof(IBqlField).IsAssignableFrom(key)))
			{
				throw new PXArgumentException("scope", PXMessages.LocalizeFormatNoPrefixNLA(Messages.IsNotBqlField, key.Name));
			}
			_key = key;
		}

		public UniqueBoolAttribute(Type key, Type groupByField) : this(key)
		{
			if (!(groupByField.IsNested && typeof(IBqlField).IsAssignableFrom(groupByField)))
			{
				throw new PXArgumentException("scope", PXMessages.LocalizeFormatNoPrefixNLA(Messages.IsNotBqlField, groupByField.Name));
			}
			_groupByField = groupByField;
		}
		#endregion

		#region Implementation
		private void FindParentAttribute(PXCache sender, object row)
		{
			if (_parentAttribute != null) return;
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(row, _key.Name))
			{
				if (attr is PXParentAttribute)
				{
					_parentAttribute = (PXParentAttribute)attr;
					break;
				}
			}
		}

		private void UpdateOtherSiblings(PXCache sender, object row)
		{
			bool needRefresh = false;
			if (_parentAttribute == null) return;
			PXFieldState state = (PXFieldState)sender.GetStateExt(row, _FieldName);
			if (state.DataType != typeof(bool)) return;
			if ((bool?)sender.GetValue(row, _FieldName) == false) return;
			foreach (object sibling in PXParentAttribute.SelectSiblings(sender, row, _parentAttribute.ParentType))
			{
				if (!sender.ObjectsEqual(sibling, row)
					&& (bool?)sender.GetValue(sibling, _FieldName) == true
					&& (_groupByField == null ||
						Equals(sender.GetValue(row, _groupByField.Name), sender.GetValue(sibling, _groupByField.Name))))
				{
						sender.SetValue(sibling, _FieldName, false);
						sender.Update(sibling);
						needRefresh = true;
				}
			}

			if (needRefresh)
			{
				foreach (var kvp in sender.Graph.Views)
				{
					PXView view = kvp.Value;
					if (_BqlTable.IsAssignableFrom(view.GetItemType()))
					{
						view.RequestRefresh();
					}
				}
			}
		}

		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			FindParentAttribute(sender, e.Row);
			UpdateOtherSiblings(sender, e.Row);
		}

		public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			FindParentAttribute(sender, e.Row);
			UpdateOtherSiblings(sender, e.Row);
		}
		#endregion
	}
}
