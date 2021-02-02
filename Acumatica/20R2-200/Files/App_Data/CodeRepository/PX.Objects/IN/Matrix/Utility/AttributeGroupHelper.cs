using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Exceptions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.Matrix.Utility
{
	public class AttributeGroupHelper
	{
		const string Separator = "; ";

		#region Types

		protected class Attribute
		{
			public int AttributeIndex { get; set; }
		}

		protected class StringArrayComparer : IEqualityComparer<string[]>
		{
			public bool Equals(string[] x, string[] y)
			{
				if (x == y)
					return true;

				if (x == null || y == null)
					return false;

				return x.SequenceEqual(y);
			}

			public int GetHashCode(string[] list)
			{
				if (list == null)
					return 0;

				return string.Join(string.Empty, list).GetHashCode();
			}
		}

		#endregion

		protected PXGraph _graph;

		protected InventoryItem _template;
		protected Dictionary<string, Attribute> _templateAttributes;
		protected Dictionary<string[], int> _combinations;
		protected int _numberOfCombination;
		protected int? _lastInventoryID;
		protected string[] _attributeValues;
		protected string _columnAttributeValue;
		protected string _rowAttributeValue;

		public AttributeGroupHelper(PXGraph graph)
		{
			_graph = graph;
		}

		public virtual void Recalculate(InventoryItem template)
		{
			if (template == null)
				throw new PXArgumentException(nameof(template));
			if (template.InventoryID == null)
				throw new PXArgumentException(nameof(template.InventoryID));

			_template = template;

			DeleteOldRows();
			GetAttributes();

			_combinations = new Dictionary<string[], int>(new StringArrayComparer());
			_numberOfCombination = 0;
			_lastInventoryID = null;
			_attributeValues = new string[_templateAttributes.Keys.Count];
			_columnAttributeValue = null;
			_rowAttributeValue = null;

			var select = new PXSelectReadonly2<CSAnswers,
				InnerJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>,
				InnerJoin<InventoryItem, On<CSAnswers.refNoteID, Equal<InventoryItem.noteID>,
					And<CSAttributeGroup.entityClassID, Equal<InventoryItem.itemClassID>>>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
					And<InventoryItem.templateItemID, Equal<Required<InventoryItem.inventoryID>>>>>>,
				OrderBy<Asc<InventoryItem.inventoryID, Asc<CSAttributeGroup.sortOrder, Asc<CSAttributeGroup.attributeID>>>>>(_graph);

			int attributesAreFilled = 0;

			using (new PXFieldScope(select.View, typeof(InventoryItem.inventoryID), typeof(CSAnswers.attributeID), typeof(CSAnswers.value)))
			{
				foreach (PXResult<CSAnswers, CSAttributeGroup, InventoryItem> result in select.Select(_template.InventoryID))
				{
					InventoryItem inventoryItem = result;
					CSAnswers answer = result;

					RecalculateItem(inventoryItem, answer, ref attributesAreFilled);
				}
			}
		}

		protected virtual void RecalculateItem(InventoryItem inventoryItem, CSAnswers answer, ref int attributesAreFilled)
		{
			if (_lastInventoryID != inventoryItem.InventoryID)
			{
				_lastInventoryID = inventoryItem.InventoryID;
				_attributeValues = new string[_templateAttributes.Keys.Count];
				attributesAreFilled = 0;
			}

			if (string.Equals(answer.AttributeID, _template.DefaultColumnMatrixAttributeID, StringComparison.OrdinalIgnoreCase))
			{
				_columnAttributeValue = answer.Value;
			}
			else if (string.Equals(answer.AttributeID, _template.DefaultRowMatrixAttributeID, StringComparison.OrdinalIgnoreCase))
			{
				_rowAttributeValue = answer.Value;
			}
			else if (_templateAttributes.TryGetValue(answer.AttributeID, out Attribute templateAttribute) &&
				_attributeValues[templateAttribute.AttributeIndex] == null)
			{
				attributesAreFilled++;
				_attributeValues[templateAttribute.AttributeIndex] = answer.Value;
			}

			if (attributesAreFilled == _templateAttributes.Keys.Count && _columnAttributeValue != null && _rowAttributeValue != null)
			{
				if (_combinations.TryGetValue(_attributeValues, out int combinationNumber))
				{
					SetInventoryCombinationNumber(inventoryItem.InventoryID, combinationNumber);
				}
				else
				{
					SetInventoryCombinationNumber(inventoryItem.InventoryID, _numberOfCombination);
					_combinations.Add(_attributeValues, _numberOfCombination);
					OnNewCombination();
					_numberOfCombination++;
				}

				_lastInventoryID = null;
			}
		}

		protected virtual void DeleteOldRows()
		{
			var groupSelect = new PXSelect<INAttributeDescriptionGroup,
				Where<INAttributeDescriptionGroup.templateID, Equal<Required<InventoryItem.inventoryID>>>>(_graph);

			groupSelect.SelectMain(_template.InventoryID).ForEach(r => groupSelect.Cache.Delete(r));

			var itemSelect = new PXSelect<INAttributeDescriptionItem,
				Where<INAttributeDescriptionItem.templateID, Equal<Required<InventoryItem.inventoryID>>>>(_graph);

			itemSelect.SelectMain(_template.InventoryID).ForEach(r => itemSelect.Cache.Delete(r));
		}

		protected virtual void GetAttributes()
		{
			CSAttributeGroup[] attributes = new PXSelectReadonly2<CSAttributeGroup,
				InnerJoin<InventoryItem, On<CSAttributeGroup.entityClassID, Equal<InventoryItem.itemClassID>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
					And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
					And<CSAttributeGroup.attributeID, NotIn3<Required<InventoryItem.defaultColumnMatrixAttributeID>,
						Required<InventoryItem.defaultRowMatrixAttributeID>>>>>>>,
				OrderBy<Asc<CSAttributeGroup.sortOrder, Asc<CSAttributeGroup.attributeID>>>>(_graph)
				.SelectMain(_template.InventoryID, _template.DefaultColumnMatrixAttributeID, _template.DefaultRowMatrixAttributeID);

			_templateAttributes = new Dictionary<string, Attribute>(StringComparer.OrdinalIgnoreCase);
			for (int attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
			{
				var attribute = attributes[attributeIndex];
				_templateAttributes.Add(attribute.AttributeID, new Attribute() { AttributeIndex = attributeIndex });
			}
		}

		protected virtual void SetInventoryCombinationNumber(int? inventoryID, int combinationNumber)
		{
			var inventoryItemCache = _graph.Caches<InventoryItem>();

			var item = InventoryItem.PK.Find(_graph, inventoryID);
			if (item == null)
				throw new RowNotFoundException(inventoryItemCache, inventoryID);

			item.AttributeDescriptionGroupID = combinationNumber;
			item.ColumnAttributeValue = _columnAttributeValue;
			item.RowAttributeValue = _rowAttributeValue;
			item = (InventoryItem)inventoryItemCache.Update(item);
		}

		protected virtual void OnNewCombination()
		{
			string[] attributeIdentifiers = _templateAttributes.Keys.ToArray();

			if (attributeIdentifiers == null)
				throw new PXArgumentException(nameof(attributeIdentifiers));
			if (_attributeValues == null)
				throw new PXArgumentException(nameof(_attributeValues));
			if (_attributeValues.Length != attributeIdentifiers.Length)
				throw new PXArgumentException($"{nameof(_attributeValues)}.{nameof(_attributeValues.Length)}");

			PXCache itemCache = _graph.Caches<INAttributeDescriptionItem>();
			var description = new List<string>();

			for (int attributeIndex = 0; attributeIndex < attributeIdentifiers.Length; attributeIndex++)
			{
				var newItem = (INAttributeDescriptionItem)itemCache.CreateInstance();
				newItem.TemplateID = _template.InventoryID;
				newItem.GroupID = _numberOfCombination;
				newItem.AttributeID = attributeIdentifiers[attributeIndex];
				newItem.ValueID = _attributeValues[attributeIndex];
				newItem = (INAttributeDescriptionItem)itemCache.Insert(newItem);

				string valueDescription = CRAttribute.Attributes[newItem.AttributeID].Values
					.Where(v => v.ValueID == newItem.ValueID).FirstOrDefault()?.Description;
				description.Add(valueDescription);
			}

			PXCache groupCache = _graph.Caches<INAttributeDescriptionGroup>();
			var newGroup = (INAttributeDescriptionGroup)groupCache.CreateInstance();
			newGroup.TemplateID = _template.InventoryID;
			newGroup.GroupID = _numberOfCombination;
			newGroup.Description = string.Join(Separator, description);
			newGroup = (INAttributeDescriptionGroup)groupCache.Insert(newGroup);
		}

		public virtual void OnNewItem(InventoryItem templateItem, InventoryItem newItem, string[] newItemAttributeIDs, string[] newItemAttributeValues)
		{
			if (templateItem == null)
				throw new PXArgumentException(nameof(templateItem));
			if (newItem == null)
				throw new PXArgumentException(nameof(newItem));
			if (newItemAttributeIDs == null)
				throw new PXArgumentException(nameof(newItemAttributeIDs));
			if (newItemAttributeValues == null)
				throw new PXArgumentException(nameof(newItemAttributeValues));
			if (newItemAttributeIDs.Length != newItemAttributeValues.Length)
				throw new PXArgumentException($"{nameof(newItemAttributeValues)}.{nameof(newItemAttributeValues.Length)}");
			
			if (templateItem != _template)
			{
				_template = templateItem;
				DeleteOldCombinationsIfAllChildrenDeleted(); // In case user has changed item class (list of attributes)
				GetAttributes();
				GetCombinations();
			}

			_attributeValues = new string[_templateAttributes.Keys.Count];
			_columnAttributeValue = null;
			_rowAttributeValue = null;
			int attributesAreFilled = 0;

			for (int attributeIndex = 0; attributeIndex < newItemAttributeIDs.Length; attributeIndex++ )
			{
				if (newItemAttributeIDs[attributeIndex] == _template.DefaultColumnMatrixAttributeID)
				{
					_columnAttributeValue = newItemAttributeValues[attributeIndex];
				}
				else if (newItemAttributeIDs[attributeIndex] == _template.DefaultRowMatrixAttributeID)
				{
					_rowAttributeValue = newItemAttributeValues[attributeIndex];
				}
				else if (_templateAttributes.TryGetValue(newItemAttributeIDs[attributeIndex], out Attribute templateAttribute) &&
					_attributeValues[templateAttribute.AttributeIndex] == null)
				{
					attributesAreFilled++;
					_attributeValues[templateAttribute.AttributeIndex] = newItemAttributeValues[attributeIndex];
				}
			}

			if (attributesAreFilled == _templateAttributes.Keys.Count)
			{
				if (_combinations.TryGetValue(_attributeValues, out int combinationNumber))
				{
					newItem.AttributeDescriptionGroupID = combinationNumber;
					newItem.ColumnAttributeValue = _columnAttributeValue;
					newItem.RowAttributeValue = _rowAttributeValue;
				}
				else
				{
					newItem.AttributeDescriptionGroupID = _numberOfCombination;
					newItem.ColumnAttributeValue = _columnAttributeValue;
					newItem.RowAttributeValue = _rowAttributeValue;

					_combinations.Add(_attributeValues, _numberOfCombination);
					OnNewCombination();
					_numberOfCombination++;
				}
			}
		}

		protected virtual void GetCombinations()
		{
			int? lastGroupId = null;
			_attributeValues = new string[_templateAttributes.Keys.Count];
			_combinations = new Dictionary<string[], int>(new StringArrayComparer());
			_numberOfCombination = 0;
			int attributesAreFilled = 0;

			if (_templateAttributes.Keys.Count == 0)
			{
				INAttributeDescriptionGroup combination = new PXSelect<INAttributeDescriptionGroup,
					Where<INAttributeDescriptionGroup.templateID, Equal<Required<InventoryItem.inventoryID>>>>(_graph)
					.Select(_template.InventoryID);

				if (combination != null)
				{
					_combinations.Add(_attributeValues, (int)combination.GroupID);
					_numberOfCombination = (int)combination.GroupID + 1;
				}

				return;
			}

			var combinationItems = new PXSelect<INAttributeDescriptionItem,
				Where<INAttributeDescriptionItem.templateID, Equal<Required<InventoryItem.inventoryID>>>>(_graph)
				.SelectMain(_template.InventoryID);

			foreach (INAttributeDescriptionItem item in combinationItems)
			{
				if (lastGroupId != item.GroupID)
				{
					lastGroupId = item.GroupID;
					_attributeValues = new string[_templateAttributes.Keys.Count];
					attributesAreFilled = 0;
				}

				if (_templateAttributes.TryGetValue(item.AttributeID, out Attribute templateAttribute) &&
					_attributeValues[templateAttribute.AttributeIndex] == null)
				{
					attributesAreFilled++;
					_attributeValues[templateAttribute.AttributeIndex] = item.ValueID;
				}

				if (attributesAreFilled == _templateAttributes.Keys.Count)
				{
					if (!_combinations.TryGetValue(_attributeValues, out int combinationNumber))
					{
						_combinations.Add(_attributeValues, (int)item.GroupID);
						_numberOfCombination = Math.Max(_numberOfCombination, (int)item.GroupID + 1);
					}

					lastGroupId = null;
				}
			}
		}

		protected virtual void DeleteOldCombinationsIfAllChildrenDeleted()
		{
			InventoryItem firstChild = (InventoryItem)PXSelect<InventoryItem,
				Where<InventoryItem.templateItemID, Equal<Required<InventoryItem.templateItemID>>>>
				.Select(_graph, _template.InventoryID);

			if (firstChild == null)
				DeleteOldRows();
		}
	}
}
