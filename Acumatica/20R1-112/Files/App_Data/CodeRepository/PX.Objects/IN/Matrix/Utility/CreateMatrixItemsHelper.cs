using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.IN.Matrix.DAC;
using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.PO;

namespace PX.Objects.IN.Matrix.Utility
{
	public class CreateMatrixItemsHelper
	{
		protected PXGraph _graph;

		public CreateMatrixItemsHelper(PXGraph graph)
		{
			_graph = graph;
		}

		public virtual MatrixInventoryItem CreateMatrixItemFromTemplate(
			EntryMatrix row, int attributeNumber, InventoryItem template,
			List<INMatrixGenerationRule> idGenRules, List<INMatrixGenerationRule> descrGenRules)
		{
			int? inventoryId = GetValueFromArray(row?.InventoryIDs, attributeNumber);
			bool? selected = GetValueFromArray(row?.Selected, attributeNumber);
			if (inventoryId != null || selected != true)
				return null;

			var currentHeader = (EntryHeader)_graph.Caches[typeof(EntryHeader)].Current;
			var currentAdditionalAttr = (AdditionalAttributes)_graph.Caches[typeof(AdditionalAttributes)].Current;
			var matrixCache = _graph.Caches[typeof(MatrixInventoryItem)];

			var newItem = PropertyTransfer.Transfer(template, new MatrixInventoryItem());

			List<string> attributes = new List<string>(currentAdditionalAttr?.AttributeIdentifiers);
			attributes.Add(currentHeader?.RowAttributeID);
			attributes.Add(currentHeader?.ColAttributeID);
			List<string> attrValues = new List<string>(currentAdditionalAttr?.Values);
			attrValues.Add(row.RowAttributeValue);
			attrValues.Add(GetValueFromArray(row?.ColAttributeValues, attributeNumber));
			List<string> attrValueDescrs = new List<string>(currentAdditionalAttr?.Descriptions);
			attrValueDescrs.Add(row.RowAttributeValueDescr);
			attrValueDescrs.Add(GetValueFromArray(row?.ColAttributeValueDescrs, attributeNumber));

			newItem.AttributeIDs = attributes.ToArray();
			newItem.AttributeValues = attrValues.ToArray();
			newItem.AttributeValueDescrs = attrValueDescrs.ToArray();

			object newCD = GenerateMatrixItemID(template, idGenRules, newItem);
			matrixCache.RaiseFieldUpdating<MatrixInventoryItem.inventoryCD>(newItem, ref newCD);
			newItem.InventoryCD = (string)newCD;

			if (PXDBLocalizableStringAttribute.IsEnabled)
			{
				PXCache templateCache = _graph.Caches<InventoryItem>();

				DBMatrixLocalizableDescriptionAttribute.SetTranslations<InventoryItem.descr, MatrixInventoryItem.descr>
					(templateCache, template, matrixCache, newItem, (locale) =>
				{
					object newTranslation = GenerateMatrixItemID(template, descrGenRules, newItem, false, locale);
					matrixCache.RaiseFieldUpdating<MatrixInventoryItem.descr>(newItem, ref newTranslation);
					return (string)newTranslation;
				});
			}
			else
			{
				object newDescr = GenerateMatrixItemID(template, descrGenRules, newItem);
				matrixCache.RaiseFieldUpdating<MatrixInventoryItem.descr>(newItem, ref newDescr);
				newItem.Descr = (string)newDescr;
			}

			newItem.Exists = (InventoryItem.UK.Find(_graph, newItem.InventoryCD) != null);
			newItem.Duplicate = matrixCache.Cached.RowCast<MatrixInventoryItem>().Any(mi => mi.InventoryCD == newItem.InventoryCD);
			newItem.Selected = (newItem.Exists != true && newItem.Duplicate != true);
			newItem.IsTemplate = false;

			return newItem;
		}

		public virtual void GetGenerationRules(int? templateItemID, out List<INMatrixGenerationRule> idGenerationRules, out List<INMatrixGenerationRule> descrGenerationRules)
		{
			var generationRules =
				PXSelectReadonly<INMatrixGenerationRule,
					Where<INMatrixGenerationRule.templateID, Equal<Required<INMatrixGenerationRule.templateID>>>,
					OrderBy<Asc<INMatrixGenerationRule.sortOrder>>>
				.Select(_graph, templateItemID)
				.RowCast<INMatrixGenerationRule>()
				.ToList();
			idGenerationRules = generationRules.Where(r => r.Type == INMatrixGenerationRule.type.ID).ToList();
			descrGenerationRules = generationRules.Where(r => r.Type == INMatrixGenerationRule.type.Description).ToList();
		}

		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2020R2)]
		public virtual string GenerateMatrixItemID(
			InventoryItem template, List<INMatrixGenerationRule> genRules, MatrixInventoryItem newItem, bool useLastAutoNumberValue = false)
		{
			return GenerateMatrixItemID(template, genRules, newItem, useLastAutoNumberValue, null);
		}

		public virtual string GenerateMatrixItemID(
			InventoryItem template, List<INMatrixGenerationRule> genRules, MatrixInventoryItem newItem, bool useLastAutoNumberValue, string locale)
		{
			StringBuilder res = new StringBuilder();

			for (int i = 0; i < genRules.Count; i++)
			{
				bool isLastSegment = (i == genRules.Count - 1);

				if (string.IsNullOrEmpty(locale))
				{
					AppendMatrixItemIDSegment(res, template, genRules[i], isLastSegment, newItem, useLastAutoNumberValue);
				}
				else
				{
					AppendMatrixItemIDSegment(res, template, genRules[i], isLastSegment, newItem, useLastAutoNumberValue, locale);
				}
			}

			return res.ToString();
		}

		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2020R2)]
		protected virtual void AppendMatrixItemIDSegment(
			StringBuilder res, InventoryItem template,
			INMatrixGenerationRule genRule, bool isLastSegment, MatrixInventoryItem newItem, bool useLastAutoNumberValue)
		{
			AppendMatrixItemIDSegment(res, template, genRule, isLastSegment, newItem, useLastAutoNumberValue, null);
		}

		protected virtual void AppendMatrixItemIDSegment(
			StringBuilder res, InventoryItem template,
			INMatrixGenerationRule genRule, bool isLastSegment, MatrixInventoryItem newItem, bool useLastAutoNumberValue, string locale)
		{
			string segValue = string.Empty;
			switch (genRule.SegmentType)
			{
				case INMatrixGenerationRule.segmentType.TemplateID:
					segValue = template.InventoryCD;
					break;
				case INMatrixGenerationRule.segmentType.TemplateDescription:
					segValue = GetTemplateDescription(template, locale);
					break;
				case INMatrixGenerationRule.segmentType.AttributeCaption:
					segValue = GetAttributeCaption(genRule.AttributeID, newItem, locale);
					break;
				case INMatrixGenerationRule.segmentType.AttributeValue:
					for (int i = 0; i < newItem.AttributeIDs.Length; i++)
					{
						if (newItem.AttributeIDs[i].Equals(genRule.AttributeID, StringComparison.OrdinalIgnoreCase))
						{
							segValue = newItem.AttributeValues[i];
							break;
						}
					}
					break;
				case INMatrixGenerationRule.segmentType.Constant:
					segValue = genRule.Constant;
					break;
				case INMatrixGenerationRule.segmentType.Space:
					segValue = " ";
					break;
				case INMatrixGenerationRule.segmentType.AutoNumber when !useLastAutoNumberValue:
					segValue = AutoNumberAttribute.GetNextNumber(_graph.Caches[typeof(InventoryItem)], null, genRule.NumberingID, _graph.Accessinfo.BusinessDate);
					break;
				case INMatrixGenerationRule.segmentType.AutoNumber when useLastAutoNumberValue:
					var numberingSequence = AutoNumberAttribute.GetNumberingSequence(genRule.NumberingID, _graph.Accessinfo.BranchID, _graph.Accessinfo.BusinessDate);
					segValue = numberingSequence?.LastNbr;
					if (string.IsNullOrEmpty(segValue))
						segValue = AutoNumberAttribute.GetNextNumber(_graph.Caches[typeof(InventoryItem)], null, genRule.NumberingID, _graph.Accessinfo.BusinessDate);
					break;

				default:
					throw new PXArgumentException(nameof(INMatrixGenerationRule));
			}

			segValue = segValue ?? string.Empty;

			if (segValue.Length > genRule.NumberOfCharacters)
			{
				segValue = segValue.Substring(0, (int)genRule.NumberOfCharacters);
			}
			else if (segValue.Length < genRule.NumberOfCharacters)
			{
				segValue = segValue.PadRight((int)genRule.NumberOfCharacters);
			}

			res.Append(segValue);

			if (!isLastSegment)
			{
				if (genRule.UseSpaceAsSeparator == true)
				{
					res.Append(' ');
				}
				else
				{
					res.Append(genRule.Separator);
				}
			}
		}

		protected virtual string GetTemplateDescription(InventoryItem template, string locale)
		{
			string segValue;
			if (!string.IsNullOrEmpty(locale))
			{
				segValue = PXDBLocalizableStringAttribute.GetTranslation<InventoryItem.descr>
					(_graph.Caches[typeof(InventoryItem)], template, locale);

				if (string.IsNullOrEmpty(segValue))
					segValue = template.Descr;
			}
			else
			{
				segValue = template.Descr;
			}

			return segValue;
		}

		protected virtual string GetAttributeCaption(string attributeID, MatrixInventoryItem newItem, string locale)
		{
			string segValue = string.Empty;

			for (int i = 0; i < newItem.AttributeIDs.Length; i++)
			{
				if (newItem.AttributeIDs[i].Equals(attributeID, StringComparison.OrdinalIgnoreCase))
				{
					if (!string.IsNullOrEmpty(locale))
					{
						string valueID = newItem.AttributeValues[i];

						var attribute = CSAttributeDetail.PK.Find(_graph, attributeID, valueID);

						segValue = PXDBLocalizableStringAttribute.GetTranslation<CSAttributeDetail.description>
							(_graph.Caches[typeof(CSAttributeDetail)], attribute, locale);

						if (string.IsNullOrEmpty(segValue))
							segValue = newItem.AttributeValueDescrs[i];
					}
					else
					{
						segValue = newItem.AttributeValueDescrs[i];
					}
					break;
				}
			}

			return segValue;
		}

		public virtual void CreateUpdateMatrixItems(InventoryItemMaintBase graph, InventoryItem templateItem, IEnumerable<MatrixInventoryItem> itemsToCreateUpdate, bool create,
			Action<MatrixInventoryItem, InventoryItem> beforeSave = null)
		{
			Dictionary<string, string> templateAttrValues =
				PXSelectReadonly<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<InventoryItem.noteID>>>>
				.Select(graph, templateItem.NoteID)
				.RowCast<CSAnswers>()
				.ToDictionary(a => a.AttributeID, a => a.Value, StringComparer.OrdinalIgnoreCase);
			IEnumerable<POVendorInventory> templateVendorInvs =
				graph.VendorItems.View.SelectMultiBound(new[] { templateItem })
				.RowCast<POVendorInventory>()
				.ToArray();
			IEnumerable<INUnit> templateItemConvs =
				graph.itemunits.View.SelectMultiBound(new[] { templateItem })
				.RowCast<INUnit>()
				.ToArray();
			IEnumerable<INItemCategory> templateItemCategs =
				graph.Category.View.SelectMultiBound(new[] { templateItem })
				.RowCast<INItemCategory>()
				.ToArray();
			IEnumerable<INItemBoxEx> templateBoxes = null;
			InventoryItemMaint stockItemGraph = null;
			if (templateItem.StkItem == true)
			{
				stockItemGraph = (InventoryItemMaint)graph;
				templateBoxes = stockItemGraph.Boxes.View.SelectMultiBound(new[] { templateItem })
					.RowCast<INItemBoxEx>()
					.ToArray();
			}

			foreach (MatrixInventoryItem itemToCreateUpdate in itemsToCreateUpdate)
			{
				graph.Clear();

				InventoryItem item;
				if (create)
				{
					item = new InventoryItem
					{
						InventoryCD = itemToCreateUpdate.InventoryCD
					};
					item = graph.Item.Insert(item);
				}
				else
				{
					item = graph.Item.Current = graph.Item.Search<InventoryItem.inventoryCD>(itemToCreateUpdate.InventoryCD);
				}
				if (item == null)
				{
					throw new PXInvalidOperationException();
				}

				if (create)
				{
					item = AssignInventoryField<InventoryItem.descr>(graph, item, itemToCreateUpdate.Descr);
					PXDBLocalizableStringAttribute.CopyTranslations<MatrixInventoryItem.descr, InventoryItem.descr>(graph, itemToCreateUpdate, item);
				}
				item = AssignInventoryField<InventoryItem.itemClassID>(graph, item, templateItem.ItemClassID);
				item = AssignInventoryField<InventoryItem.postClassID>(graph, item, templateItem.PostClassID);
				AssignConversionsSettings(graph, item, templateItem);
				item = AssignRestInventoryFields(graph, item, templateItem);
				item = AssignInventoryField<InventoryItem.templateItemID>(graph, item, templateItem.InventoryID);

				AssignInventoryAttributes(graph, itemToCreateUpdate, templateAttrValues);
				AssignVendorInventory(graph, templateVendorInvs);
				AssignInventoryConversions(graph, templateItemConvs);
				AssignInventoryCategories(graph, templateItemCategs);
				if (templateItem.StkItem == true)
					AssignInventoryBoxes(stockItemGraph, templateBoxes);

				beforeSave?.Invoke(itemToCreateUpdate, item);

				graph.Save.Press();

				itemToCreateUpdate.InventoryID = item.InventoryID;
			}
		}

		protected virtual InventoryItem AssignInventoryField<TField>(InventoryItemMaintBase graph, InventoryItem item, object value)
			where TField : IBqlField
		{
			var copy = (InventoryItem)graph.Item.Cache.CreateCopy(item);
			graph.Item.Cache.SetValue<TField>(copy, value);
			return graph.Item.Update(copy);
		}
		
		protected virtual void AssignConversionsSettings(InventoryItemMaintBase graph, InventoryItem item, InventoryItem templateItem)
		{
			//sales and purchase units must be cleared not to be added to item unit conversions on base unit change.
			PXCache cache = graph.Item.Cache;
			cache.SetValueExt<InventoryItem.baseUnit>(item, null);
			cache.SetValue<InventoryItem.salesUnit>(item, null);
			cache.SetValue<InventoryItem.purchaseUnit>(item, null);

			cache.SetValueExt<InventoryItem.baseUnit>(item, templateItem.BaseUnit);
			cache.SetValueExt<InventoryItem.salesUnit>(item, templateItem.SalesUnit);
			cache.SetValueExt<InventoryItem.purchaseUnit>(item, templateItem.PurchaseUnit);

			cache.SetValueExt<InventoryItem.decimalBaseUnit>(item, templateItem.DecimalBaseUnit);
			cache.SetValueExt<InventoryItem.decimalSalesUnit>(item, templateItem.DecimalSalesUnit);
			cache.SetValueExt<InventoryItem.decimalPurchaseUnit>(item, templateItem.DecimalPurchaseUnit);
		}

		protected virtual InventoryItem AssignRestInventoryFields(InventoryItemMaintBase graph, InventoryItem item, InventoryItem templateItem)
		{
			var copy = (InventoryItem)graph.Item.Cache.CreateCopy(item);
			graph.Item.Cache.RestoreCopy(copy, templateItem);

			var excludeFields = new Type[]
			{
				typeof(InventoryItem.inventoryID),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr),
				typeof(InventoryItem.preferredVendorID),
				typeof(InventoryItem.preferredVendorLocationID),
				typeof(InventoryItem.templateItemID),
				typeof(InventoryItem.isTemplate),
				typeof(InventoryItem.Tstamp),
				typeof(InventoryItem.createdByID),
				typeof(InventoryItem.createdByScreenID),
				typeof(InventoryItem.createdDateTime),
				typeof(InventoryItem.lastModifiedByID),
				typeof(InventoryItem.lastModifiedByScreenID),
				typeof(InventoryItem.lastModifiedDateTime),
				typeof(InventoryItem.noteID),
			};

			foreach (Type excludeField in excludeFields)
			{
				graph.Item.Cache.SetValue(copy, excludeField.Name,
					graph.Item.Cache.GetValue(item, excludeField.Name));
			}

			return graph.Item.Update(copy);
		}

		protected virtual void AssignInventoryAttributes(InventoryItemMaintBase graph, MatrixInventoryItem itemToCreateUpdate, Dictionary<string, string> templateAttrValues)
		{
			CSAnswers[] answers = graph.Answers.Select().RowCast<CSAnswers>().ToArray();
			foreach (CSAnswers answer in answers)
			{
				string value = null;
				for (int i = 0; i < itemToCreateUpdate.AttributeIDs.Length; i++)
				{
					if (itemToCreateUpdate.AttributeIDs[i].Equals(answer.AttributeID, StringComparison.OrdinalIgnoreCase))
					{
						value = itemToCreateUpdate.AttributeValues[i];
						break;
					}
				}
				if (value == null)
				{
					templateAttrValues.TryGetValue(answer.AttributeID, out value);
				}

				answer.Value = value;
				graph.Answers.Update(answer);
			}
		}

		protected virtual void AssignVendorInventory(InventoryItemMaintBase graph, IEnumerable<POVendorInventory> templateVendorInvs)
		{
			POVendorInventory[] vendorInvs = graph.VendorItems.Select().RowCast<POVendorInventory>().ToArray();
			foreach (POVendorInventory templateVendorInv in templateVendorInvs)
			{
				POVendorInventory vendorInv =
					vendorInvs.FirstOrDefault(vi
						=> vi.SubItemID == templateVendorInv.SubItemID
						&& vi.VendorID == templateVendorInv.VendorID
						&& vi.VendorLocationID == templateVendorInv.VendorLocationID
						&& vi.PurchaseUnit == templateVendorInv.PurchaseUnit)
					?? graph.VendorItems.Insert();
				var copy = (POVendorInventory)graph.VendorItems.Cache.CreateCopy(vendorInv);

				graph.VendorItems.Cache.RestoreCopy(copy, templateVendorInv);

				var excludeFields = new Type[]
				{
					typeof(POVendorInventory.recordID),
					typeof(POVendorInventory.inventoryID),
					typeof(POVendorInventory.Tstamp),
					typeof(POVendorInventory.createdByID),
					typeof(POVendorInventory.createdByScreenID),
					typeof(POVendorInventory.createdDateTime),
					typeof(POVendorInventory.lastModifiedByID),
					typeof(POVendorInventory.lastModifiedByScreenID),
					typeof(POVendorInventory.lastModifiedDateTime),
				};

				foreach (Type excludeField in excludeFields)
				{
					graph.VendorItems.Cache.SetValue(copy, excludeField.Name,
						graph.VendorItems.Cache.GetValue(vendorInv, excludeField.Name));
				}

				vendorInv = graph.VendorItems.Update(copy);
			}
		}

		protected virtual void AssignInventoryConversions(InventoryItemMaintBase graph, IEnumerable<INUnit> templateItemConvs)
		{
			INUnit[] itemConvs = graph.itemunits.Select().RowCast<INUnit>().ToArray();
			foreach (INUnit templateItemConv in templateItemConvs)
			{
				INUnit itemConv =
					itemConvs.FirstOrDefault(ic
						=> ic.FromUnit == templateItemConv.FromUnit
						&& ic.ToUnit == templateItemConv.ToUnit)
					?? graph.itemunits.Insert(new INUnit { FromUnit = templateItemConv.FromUnit });
				var copy = (INUnit)graph.itemunits.Cache.CreateCopy(itemConv);

				graph.itemunits.Cache.RestoreCopy(copy, templateItemConv);

				var excludeFields = new Type[]
				{
					typeof(INUnit.recordID),
					typeof(INUnit.inventoryID),
					typeof(INUnit.Tstamp),
					typeof(INUnit.createdByID),
					typeof(INUnit.createdByScreenID),
					typeof(INUnit.createdDateTime),
					typeof(INUnit.lastModifiedByID),
					typeof(INUnit.lastModifiedByScreenID),
					typeof(INUnit.lastModifiedDateTime),
				};

				foreach (Type excludeField in excludeFields)
				{
					graph.itemunits.Cache.SetValue(copy, excludeField.Name,
						graph.itemunits.Cache.GetValue(itemConv, excludeField.Name));
				}

				itemConv = graph.itemunits.Update(copy);
			}
		}

		protected virtual void AssignInventoryCategories(InventoryItemMaintBase graph, IEnumerable<INItemCategory> templateItemCategs)
		{
			INItemCategory[] itemCategs = graph.Category.Select().RowCast<INItemCategory>().ToArray();
			foreach (INItemCategory templateItemCateg in templateItemCategs)
			{
				INItemCategory itemCateg =
					itemCategs.FirstOrDefault(ic => ic.CategoryID == templateItemCateg.CategoryID)
					?? graph.Category.Insert(new INItemCategory { CategoryID = templateItemCateg.CategoryID });
			}
		}

		protected virtual void AssignInventoryBoxes(InventoryItemMaint graph, IEnumerable<INItemBoxEx> templateItemBoxes)
		{
			INItemBoxEx[] itemBoxes = graph.Boxes.Select().RowCast<INItemBoxEx>().ToArray();
			foreach (INItemBoxEx templateItemBox in templateItemBoxes)
			{
				INItemBoxEx itemBox =
					itemBoxes.FirstOrDefault(ic => ic.BoxID == templateItemBox.BoxID)
					?? graph.Boxes.Insert(new INItemBoxEx { BoxID = templateItemBox.BoxID });
				var copy = (INItemBoxEx)graph.Boxes.Cache.CreateCopy(itemBox);

				graph.Boxes.Cache.RestoreCopy(copy, templateItemBox);

				var excludeFields = new Type[]
				{
					typeof(INItemBoxEx.inventoryID),
					typeof(INItemBoxEx.Tstamp),
					typeof(INItemBoxEx.createdByID),
					typeof(INItemBoxEx.createdByScreenID),
					typeof(INItemBoxEx.createdDateTime),
					typeof(INItemBoxEx.lastModifiedByID),
					typeof(INItemBoxEx.lastModifiedByScreenID),
					typeof(INItemBoxEx.lastModifiedDateTime),
					typeof(INItemBoxEx.noteID),
				};

				foreach (Type excludeField in excludeFields)
				{
					graph.Boxes.Cache.SetValue(copy, excludeField.Name,
						graph.Boxes.Cache.GetValue(itemBox, excludeField.Name));
				}

				itemBox = graph.Boxes.Update(copy);
			}
		}

		protected static TResult GetValueFromArray<TResult>(TResult[] array, int index)
		{
			if (index >= 0 && index < array?.Length)
				return (TResult)array[index];

			return default(TResult);
		}
	}
}
