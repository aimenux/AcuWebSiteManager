using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.IN.Matrix.Interfaces;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using PX.Objects.CR;
using PX.Objects.Common.Exceptions;
using PX.Objects.IN.Matrix.DAC;
using PX.Objects.IN.Matrix.Utility;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public abstract class SmartPanelExt<Graph, MainItemType> : MatrixGridExt<Graph, MainItemType>
			where Graph : PXGraph, new()
			where MainItemType : class, IBqlTable, new()
	{
		public override bool AddTotals => true;

		#region Types

		public class InventoryMatrixResult
		{
			public int InventoryID { get; set; }
			public decimal Qty { get; set; }
		}

		#endregion // Types

		#region Views

		public PXSelect<MatrixInventoryItem> MatrixItems;

		public virtual IEnumerable matrixItems()
		{
			return MatrixItems.Cache.Cached;
		}

		#endregion // Views

		public override void Initialize()
		{
			base.Initialize();

			Base.Views.Caches.Remove(typeof(MatrixInventoryItem));
		}

		public PXAction<MainItemType> showMatrixPanel;
		[PXUIField(DisplayName = "Add Matrix Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ShowMatrixPanel(PXAdapter adapter)
		{
			const WebDialogResult LookupViewButtonDialogResult = WebDialogResult.Yes;

			var answer = Header.View.GetAnswer(null);

			if (answer == WebDialogResult.OK)
			{
				AddItemsToOrder(Header.Current.SiteID);
			}
			else if (answer == LookupViewButtonDialogResult)
			{
				Header.View.SetAnswer(null, WebDialogResult.None);

				answer = Matrix.AskExt((g, v) =>
				{
					Header.Current.SmartPanelType = EntryHeader.smartPanelType.Lookup;
					RecalcAttributesGrid();
					RecalcMatrixGrid();
				});
			}
			else if (Matrix.View.GetAnswer(null) == WebDialogResult.OK)
			{
				AddMatrixItemsToOrder(Header.Current.SiteID);
			}
			else
			{
				Header.AskExt((g, v) =>
				{
					Header.Current.SmartPanelType = EntryHeader.smartPanelType.Entry;
					RecalcAttributesGrid();
					RecalcMatrixGrid();
				});
			}

			return adapter.Get();
		}

		protected abstract bool IsDocumentOpen();

		protected abstract void UpdateLine(IMatrixItemLine line);

		protected abstract void CreateNewLine(int? siteID, int? inventoryID, decimal qty);

		protected abstract void CreateNewLine(int? siteID, int? inventoryID, string taxCategoryID, decimal qty);

		protected abstract IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID);

		protected abstract IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID, string taxCategoryID);

		protected virtual bool IsItemStatusDisabled(InventoryItem item)
			=> item?.ItemStatus.IsIn(InventoryItemStatus.Inactive, InventoryItemStatus.MarkedForDeletion) == true;

		#region Entry (Quick) View

		protected override CSAttribute[] GetAdditionalAttributes()
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				return base.GetAdditionalAttributes();
			}

			int? itemClassID = GetItemClassID();

			return new PXSelectReadonly2<CSAttribute,
				InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>(Base)
					.SelectMain(itemClassID);
		}

		protected override void AddFieldToAttributeGrid(PXCache cache, int attributeNumber)
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				base.AddFieldToAttributeGrid(cache, attributeNumber);
				return;
			}

			base.AddFieldToAttributeGrid(MatrixItems.Cache, attributeNumber);
		}

		protected override string GetAttributeValue(object row, int attributeNumber)
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				return base.GetAttributeValue(row, attributeNumber);
			}

			var item = row as MatrixInventoryItem;

			string returnValue = 0 <= attributeNumber && attributeNumber < item?.AttributeValueDescrs?.Length ? item.AttributeValueDescrs[attributeNumber] : null;
			if (string.IsNullOrEmpty(returnValue))
				returnValue = 0 <= attributeNumber && attributeNumber < item?.AttributeValues?.Length ? item.AttributeValues[attributeNumber] : null;

			return returnValue;
		}

		protected override void AttributeValueFieldUpdating(int attributeNumber, PXFieldUpdatingEventArgs e)
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				base.AttributeValueFieldUpdating(attributeNumber, e);
				return;
			}

			var row = e.Row as MatrixInventoryItem;
			if (row == null)
				return;

			string newValue = e.NewValue as string;

			if (attributeNumber < row.AttributeValueDescrs?.Length && row.AttributeValueDescrs[attributeNumber] != newValue)
			{
				var attributeDetail = (CSAttributeDetail)new PXSelect<CSAttributeDetail,
					Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>,
						And<CSAttributeDetail.valueID, Equal<Required<CSAttributeDetail.valueID>>>>>(Base)
					.Select(AdditionalAttributes.Current.AttributeIdentifiers[attributeNumber], newValue);

				if (attributeDetail == null)
					throw new RowNotFoundException(Base.Caches<CSAttributeDetail>(), AdditionalAttributes.Current.AttributeIdentifiers[attributeNumber], newValue);

				row.AttributeValues[attributeNumber] = attributeDetail.ValueID;
				row.AttributeValueDescrs[attributeNumber] = attributeDetail.Description;

				if (AllAttributesArePopulated(row))
				{
					int? inventoryID = FindInventoryItem(row);

					if (inventoryID != null)
					{
						OnExisingItemSelected(row, inventoryID);
					}
					else
					{
						OnNewItemSelected(Header.Current.TemplateItemID, row);
						return;
					}
				}
			}
		}

		protected virtual int? FindInventoryItem(MatrixInventoryItem row)
		{
			int? lastInventoryId = null;
			string[] attributeValues = new string[row.AttributeIDs.Length];

			foreach (PXResult<CSAnswers, CSAttributeGroup, InventoryItem> result in SelectInventoryWithAttributes())
			{
				InventoryItem inventoryItem = result;
				CSAnswers attribute = result;

				if (lastInventoryId != inventoryItem.InventoryID)
				{
					lastInventoryId = inventoryItem.InventoryID;
					for (int attributeIndex = 0; attributeIndex < attributeValues.Length; attributeIndex++)
						attributeValues[attributeIndex] = null;
				}

				for (int attributeIndex = 0; attributeIndex < attributeValues.Length; attributeIndex++)
				{
					if (string.Equals(row.AttributeIDs[attributeIndex], attribute.AttributeID, StringComparison.OrdinalIgnoreCase) &&
						row.AttributeValues[attributeIndex] == attribute.Value)
					{
						attributeValues[attributeIndex] = attribute.Value;
						break;
					}
				}


				if (lastInventoryId != null && attributeValues.All(v => v != null))
				{
					return lastInventoryId;
				}
			}

			return null;
		}

		protected virtual void OnExisingItemSelected(MatrixInventoryItem row, int? inventoryID)
		{
			var inventoryItem = InventoryItem.PK.Find(Base, inventoryID);
			if (inventoryItem == null)
				throw new RowNotFoundException(Base.Caches<InventoryItem>(), inventoryID);

			row.InventoryCD = inventoryItem.InventoryCD;
			row.InventoryID = inventoryItem.InventoryID;
			row.Descr = inventoryItem.Descr;
			row.New = false;
			row.BasePrice = inventoryItem.BasePrice;
			row.TaxCategoryID = inventoryItem.TaxCategoryID;
			row.Exists = false;
			row.Qty = IsItemStatusDisabled(inventoryItem) ? (decimal?)null : 0m;
			MatrixItems.Cache.Normalize();
		}

		protected virtual void OnNewItemSelected(int? templateItemID, MatrixInventoryItem row)
		{
			InventoryItem templateItem = InventoryItem.PK.Find(Base, templateItemID);
			if (templateItem == null)
				throw new RowNotFoundException(Base.Caches<InventoryItem>(), templateItemID);

			var createHelper = GetCreateMatrixItemsHelper(Base);

			createHelper.GetGenerationRules(templateItemID,
				out List<INMatrixGenerationRule> idGenerationRules,
				out List<INMatrixGenerationRule> descrGenerationRules);

			object newCD = createHelper.GenerateMatrixItemID(templateItem, idGenerationRules, row);
			MatrixItems.Cache.RaiseFieldUpdating<MatrixInventoryItem.inventoryCD>(row, ref newCD);
			row.InventoryCD = (string)newCD;
			row.InventoryID = null;
			row.Descr = createHelper.GenerateMatrixItemID(templateItem, descrGenerationRules, row);
			row.New = true;
			row.BasePrice = templateItem.BasePrice;
			row.TaxCategoryID = templateItem.TaxCategoryID;
			row.Exists = (InventoryItem.UK.Find(Base, row.InventoryCD) != null);
			row.Qty = 0m;
			MatrixItems.Cache.Normalize();
		}

		protected virtual CreateMatrixItemsHelper GetCreateMatrixItemsHelper(PXGraph graph)
		{
			return new CreateMatrixItemsHelper(graph);
		}

		protected virtual AttributeGroupHelper GetAttributeGroupHelper(PXGraph graph)
		{
			return new AttributeGroupHelper(graph);
		}

		protected override void RecalcMatrixGrid()
		{
			if (Header.Current.SmartPanelType == EntryHeader.smartPanelType.Lookup)
			{
				base.RecalcMatrixGrid();
				return;
			}

			MatrixItems.Cache.Clear();
		}

		[InventoryRaw(DisplayName = "Inventory ID", IsKey = true)]
		protected virtual void _(Events.CacheAttached<MatrixInventoryItem.inventoryCD> eventArgs)
		{
		}

		[PXInt]
		protected virtual void _(Events.CacheAttached<MatrixInventoryItem.inventoryID> eventArgs)
		{
		}

		protected virtual void _(Events.RowInserting<MatrixInventoryItem> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			eventArgs.Row.AttributeIDs = (string[])AdditionalAttributes.Current.AttributeIdentifiers.Clone();
			eventArgs.Row.AttributeValueDescrs = new string[eventArgs.Row.AttributeIDs.Length];
			eventArgs.Row.AttributeValues = new string[eventArgs.Row.AttributeIDs.Length];
		}

		protected virtual void _(Events.RowSelected<MainItemType> eventArgs)
		{
			showMatrixPanel.SetEnabled(IsDocumentOpen());
		}

		protected virtual void _(Events.RowSelected<MatrixInventoryItem> eventArgs)
		{
			bool allowEdit = IsDocumentOpen() && AdditionalAttributes.Current.AttributeIdentifiers?.Length > 0;
			MatrixItems.AllowInsert = allowEdit;
			MatrixItems.AllowDelete = allowEdit;
			MatrixItems.AllowUpdate = allowEdit;

			if (eventArgs.Row == null)
				return;

			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row).ForAllFields(a => a.Enabled = false);

			bool allowEditRow = AllAttributesArePopulated(eventArgs.Row);
			Exception inventoryException = null;

			if (allowEditRow && eventArgs.Row?.InventoryID != null)
			{
				var item = InventoryItem.PK.Find(Base, eventArgs.Row.InventoryID);
				if (IsItemStatusDisabled(item))
				{
					allowEditRow = false;
					string label = PXStringListAttribute.GetLocalizedLabel<InventoryItem.itemStatus>(Base.Caches<InventoryItem>(), item);
					inventoryException = new PXSetPropertyException(Messages.InventoryItemIsInStatus, PXErrorLevel.Warning, label);
				}
			}

			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row)
				.For<MatrixInventoryItem.qty>(a => a.Enabled = allowEditRow)
				.SameFor<MatrixInventoryItem.taxCategoryID>();

			if (eventArgs.Row.Exists == true)
				inventoryException = new PXSetPropertyException(Messages.InventoryIDExists, PXErrorLevel.Error);
			eventArgs.Cache.RaiseExceptionHandling<MatrixInventoryItem.inventoryCD>(eventArgs.Row, eventArgs.Row.InventoryCD, inventoryException);
		}

		protected virtual bool AllAttributesArePopulated(MatrixInventoryItem row)
			=> row?.AttributeValues?.Any(v => string.IsNullOrEmpty(v)) == false;

		protected override void _(Events.FieldUpdated<EntryHeader, EntryHeader.templateItemID> eventArgs)
		{
			base._(eventArgs);
			if (eventArgs.Row != null)
				eventArgs.Row.Description = InventoryItem.PK.Find(Base, eventArgs.Row.TemplateItemID)?.Descr;
		}

		protected virtual void IncreaseQty(int? siteID, int inventoryID, string taxCategoryID, decimal addQty)
		{
			var line = GetLines(siteID, inventoryID, taxCategoryID).FirstOrDefault();

			if (line != null)
			{
				line.Qty += addQty;
				UpdateLine(line);
			}
			else
				CreateNewLine(siteID, inventoryID, taxCategoryID, addQty);
		}

		protected virtual void AddItemsToOrder(int? siteID)
		{
			var templateItem = InventoryItem.PK.Find(Base, Header.Current.TemplateItemID);
			var itemsToCreate = MatrixItems.Cache.Cached.RowCast<MatrixInventoryItem>().Where(mi => mi.New == true).ToList();

			if (templateItem != null)
			{
				var clone = Base.Clone();

				var inventoryItemGraph = (templateItem.StkItem == true) ?
					(InventoryItemMaintBase)PXGraph.CreateInstance<InventoryItemMaint>() :
					PXGraph.CreateInstance<NonStockItemMaint>();

				var helper = GetCreateMatrixItemsHelper(Base);
				var attributeGroupHelper = GetAttributeGroupHelper(inventoryItemGraph);

				PXLongOperation.StartOperation(Base, delegate ()
				{
					PXLongOperation.SetCustomInfo(clone);

					helper.CreateUpdateMatrixItems(inventoryItemGraph, templateItem, itemsToCreate, true,
						(t, item) => attributeGroupHelper.OnNewItem(templateItem, item, t.AttributeIDs, t.AttributeValues));

					var ext = clone.FindImplementation<SmartPanelExt<Graph, MainItemType>>();
					if (ext == null)
						throw new PXArgumentException(nameof(SmartPanelExt<Graph, MainItemType>));

					foreach (MatrixInventoryItem item in ext.MatrixItems.Cache.Cached)
					{
						if (item.Qty == null)
							continue;

						if (item.InventoryID == null)
						{
							string attributeIDs = string.Join(";", item.AttributeIDs ?? throw new PXArgumentException(nameof(item.AttributeIDs)));
							string values = string.Join(";", item.AttributeIDs ?? throw new PXArgumentException(nameof(item.AttributeValues)));
							throw new RowNotFoundException(MatrixItems.Cache, string.Join("; ", item.AttributeIDs), string.Join(", ", item.AttributeValues));
						}

						
						ext.IncreaseQty(siteID, (int)item.InventoryID, item.TaxCategoryID, (decimal)item.Qty);
					}
				});
			}
		}

		#endregion // Entry (Quick) View

		#region Lookup (Matrix) View

		protected virtual IEnumerable<InventoryMatrixResult> GetResult()
		{
			foreach (var row in Matrix.Cache.Cached)
			{
				var matrix = row as EntryMatrix;
				for (int columnIndex = 0; columnIndex < matrix.Quantities.Length; columnIndex++)
				{
					int? inventoryID = matrix.InventoryIDs[columnIndex];
					decimal? qty = matrix.Quantities[columnIndex];

					if (inventoryID != null)
						yield return new InventoryMatrixResult() { InventoryID = (int)inventoryID, Qty = qty ?? 0m };
				}
			}
		}
		
		protected virtual void AddMatrixItemsToOrder(int? siteID)
		{
			var clone = Base.Clone();

			PXLongOperation.StartOperation(Base, delegate ()
			{
				PXLongOperation.SetCustomInfo(clone);

				var ext = clone.FindImplementation<SmartPanelExt<Graph, MainItemType>>();
				if (ext == null)
					throw new PXArgumentException(nameof(SmartPanelExt<Graph, MainItemType>));

				foreach (var row in ext.GetResult())
				{
					var addQty = row.Qty - ext.GetQty(siteID, row.InventoryID);

					if (addQty > 0)
						ext.IncreaseQty(siteID, row.InventoryID, addQty);
					else if (addQty < 0)
						ext.DecreaseQty(siteID, row.InventoryID, addQty);
				}
			});
		}

		protected virtual void IncreaseQty(int? siteID, int inventoryID, decimal addQty)
		{
			var line = GetLines(siteID, inventoryID).FirstOrDefault();

			if (line != null)
			{
				line.Qty += addQty;
				UpdateLine(line);
			}
			else
				CreateNewLine(siteID, inventoryID, addQty);
		}

		protected virtual void DecreaseQty(int? siteID, int inventoryID, decimal addQty)
		{
			decimal accumQty = addQty * -1;

			foreach (var line in GetLines(siteID, inventoryID))
			{
				if (line.Qty >= accumQty)
				{
					line.Qty -= accumQty;
					UpdateLine(line);
					accumQty = 0;
					break;
				}

				accumQty -= line.Qty ?? 0m;
				line.Qty = 0;
				UpdateLine(line);
			}
		}

		protected override void FillInventoryMatrixItem(EntryMatrix newRow, int colAttributeIndex, InventoryMapValue inventoryValue)
		{
			if (inventoryValue?.InventoryID == null) return;
			var item = InventoryItem.PK.Find(Base, inventoryValue.InventoryID);

			if (!IsItemStatusDisabled(item))
			{
				newRow.InventoryIDs[colAttributeIndex] = inventoryValue.InventoryID;

				try
				{
					newRow.Quantities[colAttributeIndex] = GetQty(Header.Current.SiteID, inventoryValue.InventoryID);
				}
				catch (PXSetPropertyException exception)
				{
					newRow.InventoryIDs[colAttributeIndex] = null;
					newRow.Errors[colAttributeIndex] = exception.Message;
				}
			}
			else
			{
				string label = PXStringListAttribute.GetLocalizedLabel<InventoryItem.itemStatus>(Base.Caches<InventoryItem>(), item);
				newRow.Errors[colAttributeIndex] = PXLocalizer.LocalizeFormat(Messages.InventoryItemIsInStatus, label);
			}
		}

		protected virtual decimal GetQty(int? siteID, int? inventoryID)
		{
			var transactions = GetLines(siteID, inventoryID).ToArray();

			if (transactions.Any(l => l.UOM != transactions.First().UOM))
				throw new PXSetPropertyException(Messages.LinesWithSameInventoryHaveDifferentUOM);

			return transactions.Sum(l => l.Qty) ?? 0m;
		}

		protected override void FieldSelectingImpl(int attributeNumber, PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
			var matrix = e.Row as EntryMatrix;
			int? inventoryId = GetValueFromArray(matrix?.InventoryIDs, attributeNumber);
			decimal? qty = GetValueFromArray(matrix?.Quantities, attributeNumber);
			string error = GetValueFromArray(matrix?.Errors, attributeNumber);

			var state = PXDecimalState.CreateInstance(e.ReturnState, _precision.Value, fieldName, false, 0, 0m, null);
			state.Enabled = inventoryId != null && IsDocumentOpen();
			state.Error = error;
			state.ErrorLevel = string.IsNullOrEmpty(error) ? PXErrorLevel.Undefined : PXErrorLevel.Warning;
			e.ReturnState = state;
			e.ReturnValue = (inventoryId != null || matrix?.IsTotal == true) ? qty : null;

			var firstMatrix = s.Cached.FirstOrDefault_() as EntryMatrix;
			if (attributeNumber < firstMatrix?.ColAttributeValueDescrs?.Length)
			{
				state.DisplayName = firstMatrix.ColAttributeValueDescrs[attributeNumber] ?? firstMatrix.ColAttributeValues[attributeNumber];
				state.Visibility = PXUIVisibility.Visible;
				state.Visible = true;
			}
			else
			{
				state.DisplayName = null;
				state.Visibility = PXUIVisibility.Invisible;
				state.Visible = false;
			}
		}

		protected override void FieldUpdatingImpl(int attributeNumber, PXCache s, PXFieldUpdatingEventArgs e, string fieldName)
		{
			var row = e.Row as EntryMatrix;
			if (row == null)
				return;

			if (attributeNumber < row.Quantities?.Length)
			{
				row.Quantities[attributeNumber] = Convert.ToDecimal(e.NewValue);
				Matrix.View.RequestRefresh();
			}
		}

		protected override void TotalFieldSelecting(PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
			var matrix = e.Row as EntryMatrix;

			var state = PXDecimalState.CreateInstance(e.ReturnState, _precision.Value, fieldName, false, 0, 0m, null);
			e.ReturnState = state;
			state.Enabled = false;

			state.DisplayName = PXLocalizer.Localize(Messages.TotalQty);

			var firstMatrix = s.Cached.FirstOrDefault_() as EntryMatrix;
			if (firstMatrix?.ColAttributeValueDescrs?.Length > 0)
			{
				state.Visibility = PXUIVisibility.Visible;
				state.Visible = true;
			}
			else
			{
				state.Visibility = PXUIVisibility.Invisible;
				state.Visible = false;
			}

			decimal sum = 0;
			for (int columnIndex = 0; columnIndex < matrix?.Quantities?.Length; columnIndex++)
				sum += (matrix.IsTotal == true || matrix.InventoryIDs[columnIndex] != null) ? (matrix.Quantities[columnIndex] ?? 0m) : 0m;

			e.ReturnValue = sum;
		}

		protected override EntryMatrix GenerateTotalRow(IEnumerable<EntryMatrix> rows)
		{
			bool rowsExist = false;
			var totalRow = (EntryMatrix)Matrix.Cache.CreateInstance();

			foreach (EntryMatrix row in Matrix.Cache.Cached)
			{
				rowsExist = true;

				if (totalRow.Quantities == null)
					totalRow.Quantities = new decimal?[row.Quantities.Length];

				for (int columnIndex = 0; columnIndex < row.Quantities.Length; columnIndex++)
				{
					totalRow.Quantities[columnIndex] = totalRow.Quantities[columnIndex] ?? 0m;
					totalRow.Quantities[columnIndex] += row.InventoryIDs[columnIndex] != null ? row.Quantities[columnIndex] : 0m;
				}
			}

			totalRow.RowAttributeValueDescr = PXLocalizer.Localize(Messages.TotalQty);
			totalRow.IsTotal = true;
			totalRow.LineNbr = int.MaxValue;

			return rowsExist ? totalRow : null;
		}

		#region Availability

		protected virtual string GetAvailability(int? siteID, int? inventoryID, decimal? qty)
		{
			if (inventoryID == null)
				return null;

			InventoryItem item = InventoryItem.PK.Find(Base, inventoryID);
			if (item == null)
				throw new Common.Exceptions.RowNotFoundException(Base.Caches<InventoryItem>(), inventoryID);

			int? inventorySiteID = siteID;

			if (inventorySiteID == null)
				inventorySiteID = item.DfltSiteID;

			SiteStatus allocated = new SiteStatus
			{
				InventoryID = inventoryID,
				SubItemID = item.DefaultSubItemID,
				SiteID = inventorySiteID
			};

			allocated = InsertWith(Base, allocated,
				(cache, e) =>
				{
					cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					cache.IsDirty = false;
				});
			allocated = PXCache<SiteStatus>.CreateCopy(allocated);

			INSiteStatus status = INSiteStatus.PK.Find(Base, inventoryID, item.DefaultSubItemID, inventorySiteID);

			if (status != null)
			{
				allocated.QtyOnHand += status.QtyOnHand;
				allocated.QtyAvail += status.QtyAvail;
				allocated.QtyHardAvail += status.QtyHardAvail;
				allocated.QtyActual += status.QtyActual;
				allocated.QtyPOOrders += status.QtyPOOrders;
			}

			foreach (var line in GetLines(inventorySiteID, inventoryID))
				DeductAllocated(allocated, line);

			return GetAvailabilityMessage(inventorySiteID, item, allocated);
		}

		protected abstract string GetAvailabilityMessage(int? siteID, InventoryItem item, SiteStatus allocated);

		protected T InsertWith<T>(PXGraph graph, T row, PXRowInserted handler)
			where T : class, IBqlTable, new()
		{
			graph.RowInserted.AddHandler<T>(handler);
			try
			{
				return PXCache<T>.Insert(graph, row);
			}
			finally
			{
				graph.RowInserted.RemoveHandler<T>(handler);
			}
		}

		protected virtual string FormatQty(decimal? value)
		{
			return (value == null) ? string.Empty : ((decimal)value).ToString("N" + CommonSetupDecPl.Qty.ToString(),
				System.Globalization.NumberFormatInfo.CurrentInfo);
		}

		protected abstract void DeductAllocated(SiteStatus allocated, IMatrixItemLine line);

		protected virtual void _(Events.FieldSelecting<EntryMatrix, EntryMatrix.matrixAvailability> eventArgs)
		{
			eventArgs.ReturnValue = null;

			EntryMatrix row = eventArgs.Row;

			if (row != null && row.SelectedColumn != null && Header.Current?.ShowAvailable == true)
			{
				int columnIndex = (int)row.SelectedColumn;

				eventArgs.ReturnValue = GetAvailability(Header.Current.SiteID,
					GetValueFromArray(row.InventoryIDs, columnIndex),
					GetValueFromArray(row.Quantities, columnIndex));
			}
		}

		protected override void OnMatrixGridCellCahnged()
		{
			// Trigger update of the Availability, which is likely to be different per cell, since each sell may be a different item
			object temp = null;
			Matrix.Cache.RaiseFieldSelecting<EntryMatrix.matrixAvailability>(Matrix.Current, ref temp, true);
		}
		#endregion // Availability

		#endregion // Lookup (Matrix) View
	}
}
