using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.IN.Matrix.DAC;
using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.IN.Matrix.Utility;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public abstract class CreateMatrixItemsExt<TGraph, TMainItem> : MatrixGridExt<TGraph, TMainItem>
		where TGraph : PXGraph, new()
		where TMainItem : class, IBqlTable, new()
	{
		public override bool AddPreliminary => true;

		public override bool ShowDisabledValue => false;

		public PXSelect<MatrixInventoryItem> MatrixItemsForCreation;

		[PXGuid]
		public virtual void _(Events.CacheAttached<MatrixInventoryItem.noteID> e)
		{
		}

		public PXAction<TMainItem> createMatrixItems;
		[PXUIField(DisplayName = "Create Matrix Items", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CreateMatrixItems(PXAdapter adapter)
		{
			if (Base.IsImport || Base.IsContractBasedAPI)
			{
				PrepareMatrixItemList();
				return CreateUpdate(adapter);
			}

			if (this.MatrixItemsForCreation.AskExt((g, v) =>
			{
				PrepareMatrixItemList();
			}) == WebDialogResult.OK)
			{
				return CreateUpdate(adapter);
			}
			return adapter.Get();
		}

		public PXAction<TMainItem> createUpdate;
		[PXUIField(DisplayName = "Confirmation", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CreateUpdate(PXAdapter adapter)
		{
			var templateItem = GetTemplateItem();
			var itemsToCreate = MatrixItemsForCreation.Cache.Cached.RowCast<MatrixInventoryItem>().Where(mi => mi.Selected == true).ToList();

			if (templateItem != null && itemsToCreate.Any())
			{
				var clone = Base.Clone();
				var graph = (templateItem.StkItem == true)
					? (InventoryItemMaintBase)PXGraph.CreateInstance<InventoryItemMaint>()
					: PXGraph.CreateInstance<NonStockItemMaint>();
				var helper = this.GetHelper(graph);
				var attributeGroupHelper = this.GetAttributeGroupHelper(graph);
				PXLongOperation.StartOperation(Base, delegate()
				{
					helper.CreateUpdateMatrixItems(graph, templateItem, itemsToCreate, true,
						(t, item) => attributeGroupHelper.OnNewItem(templateItem, item, t.AttributeIDs, t.AttributeValues));
					var ext = clone.FindImplementation<CreateMatrixItemsExt<TGraph, TMainItem>>();
					ext.RecalcMatrixGrid();
					Base.SelectTimeStamp();
					PXLongOperation.SetCustomInfo(clone);
				});
			}

			return adapter.Get();
		}

		protected virtual InventoryItem GetTemplateItem()
		{
			return InventoryItem.PK.Find(Base, Header.Current.TemplateItemID);
		}

		protected virtual IEnumerable matrixItemsForCreation()
		{
			MatrixItemsForCreation.Cache.IsDirty = false;
			return MatrixItemsForCreation.Cache.Cached;
		}

		public override void Initialize()
		{
			base.Initialize();

			for (int i = 0; i < MatrixItemsForCreation.Current?.AttributeIDs?.Length; i++)
				AddItemAttributeColumn(i);

			PXUIFieldAttribute.SetVisible<EntryHeader.showAvailable>(Header.Cache, null, false);
			PXUIFieldAttribute.SetVisible<EntryHeader.siteID>(Header.Cache, null, false);
		}

		protected virtual void AddItemAttributeColumn(int attributeNumber)
		{
			string fieldName = $"AttributeValue{attributeNumber}";

			if (!MatrixItemsForCreation.Cache.Fields.Contains(fieldName))
			{
				MatrixItemsForCreation.Cache.Fields.Add(fieldName);

				Base.FieldSelecting.AddHandler(
					MatrixItemsForCreation.Cache.GetItemType(),
					fieldName,
					(s, e) => ItemAttributeValueFieldSelecting(attributeNumber, e, fieldName));
			}
		}

		protected virtual void ItemAttributeValueFieldSelecting(int attributeNumber, PXFieldSelectingEventArgs e, string fieldName)
		{
			var row = (MatrixInventoryItem)e.Row;
			PXStringState state = (PXStringState)PXStringState.CreateInstance(e.ReturnState, null, true, fieldName, false, null, null, null, null, null, null);
			state.Enabled = false;
			e.ReturnState = state;

			if (attributeNumber < MatrixItemsForCreation.Current?.AttributeIDs?.Length)
			{
				e.ReturnValue = 0 <= attributeNumber && attributeNumber < row?.AttributeValues?.Length ? row.AttributeValues[attributeNumber] : null;
				state.Visible = true;
				state.Visibility = PXUIVisibility.Visible;

				if (CRAttribute.Attributes.TryGetValue(MatrixItemsForCreation.Current.AttributeIDs[attributeNumber],
					out CRAttribute.Attribute attribute))
				{
					state.DisplayName = attribute.Description;
					state.AllowedValues = attribute.Values.Select(v => v.ValueID).ToArray();
					state.AllowedLabels = attribute.Values.Select(v => v.Description).ToArray();
				}
			}
			else
			{
				state.Value = null;
				e.ReturnValue = null;
				state.DisplayName = null;
				state.Visible = false;
				state.Visibility = PXUIVisibility.Invisible;
			}
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R1)]
		protected virtual void _(Events.RowSelected<TMainItem> e)
		{
		}

		protected virtual void _(Events.RowSelected<EntryMatrix> e)
		{
			createMatrixItems.SetEnabled(Matrix.Cache.IsDirty);
		}

		protected override void FieldSelectingImpl(int attributeNumber, PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
			var matrix = (EntryMatrix)e.Row;
			var ret = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), fieldName: fieldName);

			int? inventoryId = GetValueFromArray(matrix?.InventoryIDs, attributeNumber);
			bool? selected = GetValueFromArray(matrix?.Selected, attributeNumber);
			string error = GetValueFromArray(matrix?.Errors, attributeNumber);

			ret.Enabled = (inventoryId == null && AllAdditionalAttributesArePopulated());
			ret.Error = error;
			ret.ErrorLevel = string.IsNullOrEmpty(error) ? PXErrorLevel.Undefined : PXErrorLevel.Warning;

			var anyMatrixRow = matrix ?? this.GetFirstMatrixRow();
			if (attributeNumber < anyMatrixRow?.ColAttributeValueDescrs?.Length)
			{
				ret.DisplayName = anyMatrixRow.ColAttributeValueDescrs[attributeNumber] ?? anyMatrixRow.ColAttributeValues[attributeNumber];
				ret.Visibility = PXUIVisibility.Visible;
				ret.Visible = true;
			}
			else
			{
				ret.DisplayName = null;
				ret.Visibility = PXUIVisibility.Invisible;
				ret.Visible = false;
			}

			e.ReturnState = ret;
			e.ReturnValue = selected;
		}

		protected override void FieldUpdatingImpl(int attributeNumber, PXCache s, PXFieldUpdatingEventArgs e, string fieldName)
		{
			var matrix = (EntryMatrix)e.Row;
			if (matrix == null) return;

			if (attributeNumber < matrix.Selected?.Length)
			{
				bool? newValue = (bool?)e.NewValue;
				matrix.Selected[attributeNumber] = newValue;

				if (matrix.IsPreliminary == true)
				{
					foreach (EntryMatrix row in Matrix.Cache.Cached.Cast<EntryMatrix>().Where(row => row.IsPreliminary != true))
					{
						if (row.InventoryIDs[attributeNumber] == null)
						{
							row.Selected[attributeNumber] = newValue;
						}
					}
					Matrix.View.RequestRefresh();
				}
			}
		}

		protected override void FillInventoryMatrixItem(EntryMatrix newRow, int colAttributeIndex, InventoryMapValue inventoryValue)
		{
			newRow.InventoryIDs[colAttributeIndex] = inventoryValue?.InventoryID;
			newRow.Selected[colAttributeIndex] = (inventoryValue?.InventoryID != null);
		}

		protected virtual void PrepareMatrixItemList()
		{
			MatrixItemsForCreation.Cache.Clear();
			var templateItem = GetTemplateItem();
			int cnt = 0;
			var helper = GetHelper(Base);
			List<INMatrixGenerationRule> idGenerationRules, descrGenerationRules;
			GetGenerationRules(helper, out idGenerationRules, out descrGenerationRules);
			foreach (EntryMatrix row in this.Matrix.Cache.Cached.Cast<EntryMatrix>().Where(row => row.IsPreliminary != true))
			{
				for (int i = 0; i < row.InventoryIDs.Length; i++)
				{
					MatrixInventoryItem newItem = helper.CreateMatrixItemFromTemplate(row, i, templateItem, idGenerationRules, descrGenerationRules);
					if (newItem != null)
					{
						bool firstItem = (cnt == 0);
						if (firstItem)
						{
							for (int j = 0; j < newItem.AttributeIDs.Length; j++)
								AddItemAttributeColumn(j);
						}
						newItem.InventoryID = ++cnt;
						MatrixItemsForCreation.Cache.Hold(newItem);
						if (firstItem)
						{
							MatrixItemsForCreation.Current = newItem;
						}
					}
				}
			}
		}

		protected virtual void GetGenerationRules(CreateMatrixItemsHelper helper, out List<INMatrixGenerationRule> idGenerationRules, out List<INMatrixGenerationRule> descrGenerationRules)
		{
			helper.GetGenerationRules(Header.Current.TemplateItemID, out idGenerationRules, out descrGenerationRules);
		}

		protected virtual CreateMatrixItemsHelper GetHelper(PXGraph graph)
		{
			return new CreateMatrixItemsHelper(graph);
		}
		
		protected virtual AttributeGroupHelper GetAttributeGroupHelper(PXGraph graph)
		{
			return new AttributeGroupHelper(graph);
		}

		protected override EntryMatrix GeneratePreliminaryRow(IEnumerable<EntryMatrix> rows)
		{
			var preliminaryRow = (EntryMatrix)Matrix.Cache.CreateInstance();

			preliminaryRow.RowAttributeValueDescr = PXLocalizer.Localize(Messages.SelectColumn);
			preliminaryRow.IsPreliminary = true;
			preliminaryRow.LineNbr = -1;
			var firstRow = rows.FirstOrDefault();
			preliminaryRow.Selected = new bool?[firstRow?.InventoryIDs.Length ?? 0];
			preliminaryRow.InventoryIDs = new int?[firstRow?.InventoryIDs.Length ?? 0];

			return preliminaryRow;
		}

		protected override void PreliminaryFieldSelecting(PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
			var matrix = (EntryMatrix)e.Row;
			var ret = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), fieldName: fieldName);

			ret.Enabled = AllAdditionalAttributesArePopulated();
			ret.DisplayName = PXLocalizer.Localize(Messages.SelectRow);
			ret.Visibility = PXUIVisibility.Visible;
			ret.Visible = true;

			e.ReturnState = ret;
			e.ReturnValue = matrix?.AllSelected;
		}

		protected override void PreliminaryFieldUpdating(PXCache s, PXFieldUpdatingEventArgs e, string fieldName)
		{
			var matrix = (EntryMatrix)e.Row;
			if (matrix == null) return;
			bool? newValue = (bool?)e.NewValue;
			matrix.AllSelected = newValue;
			for (int i = 0; i < matrix.InventoryIDs?.Length; i++)
			{
				if (matrix.InventoryIDs[i] == null)
				{
					matrix.Selected[i] = newValue;
				}
			}
			if (matrix.IsPreliminary == true)
			{
				foreach (EntryMatrix row in Matrix.Cache.Cached.Cast<EntryMatrix>().Where(row => row.IsPreliminary != true))
				{
					Matrix.Cache.SetValueExt(row, fieldName, newValue);
				}
				Matrix.View.RequestRefresh();
			}
		}
	}
}
