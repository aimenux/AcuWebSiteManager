using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Unbound;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public abstract class MatrixGridExt<Graph, MainItemType> : HeaderAndAttributesExt<Graph, MainItemType>
			where Graph : PXGraph
			where MainItemType : class, IBqlTable, new()
	{
		protected const string MatrixFieldName = "AttributeValue";

		protected Lazy<int?> _precision;

		public virtual bool AddTotals => false;

		public virtual bool AddPreliminary => false;

		public override void Initialize()
		{
			base.Initialize();
			_precision = new Lazy<int?>(GetQtyPrecision);
		}

		#region Types

		public class InventoryMapKey : Tuple<string, string>
		{
			public InventoryMapKey(string columnAttributeValue, string rowAttributeValue) : base(columnAttributeValue, rowAttributeValue)
			{ }
			public string ColumnAttributeValue => Item1;
			public string RowAttributeValue => Item2;
		}

		public class InventoryMapValue
		{
			public int? InventoryID { get; set; }
			public INSiteStatus SiteStatus { get; set; }
			public INLocationStatus LocationStatus { get; set; }
		}

		public class MatrixAttributeValues
		{
			public CSAttributeDetail[] ColumnValues { get; set; }
			public CSAttributeDetail[] RowValues { get; set; }
		}

		#endregion

		#region Matrix view

		public PXSelect<EntryMatrix> Matrix;

		public IEnumerable matrix()
		{
			bool preliminaryReturned = false;
			foreach (EntryMatrix row in Matrix.Cache.Cached)
			{
				preliminaryReturned |= row.IsPreliminary == true;
				yield return row;
			}

			if (AddPreliminary && !preliminaryReturned)
			{
				EntryMatrix preliminary = GeneratePreliminaryRow(Matrix.Cache.Cached.Cast<EntryMatrix>());
				if (preliminary != null)
				{
					Matrix.Cache.Hold(preliminary);
					yield return preliminary;
				}
			}

			if (AddTotals)
			{
				EntryMatrix total = GenerateTotalRow(Matrix.Cache.Cached.Cast<EntryMatrix>());
				if (total != null)
				{
					yield return total;
				}
			}
		}

		protected virtual EntryMatrix GeneratePreliminaryRow(IEnumerable<EntryMatrix> rows)
		{
			return null;
		}

		protected virtual EntryMatrix GenerateTotalRow(IEnumerable<EntryMatrix> rows)
		{
			return null;
		}

		protected void _(Events.RowPersisting<EntryMatrix> eventArgs) => eventArgs.Cancel = true;

		protected virtual int? GetQtyPrecision()
		{
			return CommonSetupDecPl.Qty;
		}

		#endregion

		#region Fields

		protected override void AddNeededFields()
		{
			base.AddNeededFields();

			AddPreliminaryField();

			var firstMatrix = this.GetFirstMatrixRow();

			for (int attributeIndex = 0; attributeIndex < firstMatrix?.ColAttributeValueDescrs?.Length; attributeIndex++)
				AddField(attributeIndex);

			AddTotalField();
		}

		protected virtual void AddField(int attributeNumber)
		{
			string fieldName = $"{MatrixFieldName}{attributeNumber}";

			if (!Matrix.Cache.Fields.Contains(fieldName))
			{
				Matrix.Cache.Fields.Add(fieldName);

				Base.FieldSelecting.AddHandler(
					Matrix.Cache.GetItemType(),
					fieldName,
					(s, e) => FieldSelectingImpl(attributeNumber, s, e, fieldName));

				Base.FieldUpdating.AddHandler(
					Matrix.Cache.GetItemType(),
					fieldName,
					(s, e) => FieldUpdatingImpl(attributeNumber, s, e, fieldName));
			}
		}

		protected abstract void FieldSelectingImpl(int attributeNumber, PXCache s, PXFieldSelectingEventArgs e, string fieldName);

		protected abstract void FieldUpdatingImpl(int attributeNumber, PXCache s, PXFieldUpdatingEventArgs e, string fieldName);

		protected virtual void AddPreliminaryField()
		{
			if (!AddPreliminary) return;

			const string fieldName = "Preliminary";

			if (!Matrix.Cache.Fields.Contains(fieldName))
			{
				Matrix.Cache.Fields.Add(fieldName);

				Base.FieldSelecting.AddHandler(
					Matrix.Cache.GetItemType(),
					fieldName,
					(s, e) => PreliminaryFieldSelecting(s, e, fieldName));

				Base.FieldUpdating.AddHandler(
					Matrix.Cache.GetItemType(),
					fieldName,
					(s, e) => PreliminaryFieldUpdating(s, e, fieldName));
			}
		}

		protected virtual void AddTotalField()
		{
			if (!AddTotals) return;

			const string fieldName = "Extra";

			if (!Matrix.Cache.Fields.Contains(fieldName))
			{
				Matrix.Cache.Fields.Add(fieldName);

				Base.FieldSelecting.AddHandler(
					Matrix.Cache.GetItemType(),
					fieldName,
					(s, e) => TotalFieldSelecting(s, e, fieldName));
			}
		}

		protected virtual void PreliminaryFieldSelecting(PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
		}

		protected virtual void PreliminaryFieldUpdating(PXCache s, PXFieldUpdatingEventArgs e, string fieldName)
		{
		}

		protected virtual void TotalFieldSelecting(PXCache s, PXFieldSelectingEventArgs e, string fieldName)
		{
		}

		protected virtual void _(Events.FieldUpdated<EntryHeader, EntryHeader.siteID> eventArgs)
			=> RecalcMatrixGrid();

		#endregion

		#region Overrides

		protected override void RecalcAttributesGrid()
		{
			base.RecalcAttributesGrid();
			RecalcMatrixGrid();
		}

		protected override void AttributeValueFieldUpdating(int attributeNumber, PXFieldUpdatingEventArgs e)
		{
			base.AttributeValueFieldUpdating(attributeNumber, e);
			RecalcMatrixGrid();
		}

		#endregion

		#region Matrix grid

		protected virtual void RecalcMatrixGrid()
		{
			EntryMatrix firstMatrixRow = this.GetFirstMatrixRow();
			int? selectedColumn = firstMatrixRow?.SelectedColumn;

			Matrix.Cache.Clear();

			if (Header.Current.ColAttributeID == null || Header.Current.RowAttributeID == null)
				return;

			IDictionary<InventoryMapKey, InventoryMapValue> inventoryMap = GetInventoryMap();
			FillInventoryMatrix(inventoryMap, selectedColumn);
		}

		/// <summary>
		/// Returns collection to map Inventory Item to row and column attributes values.
		/// Key contains: row attribute value, column attribute value.
		/// Value of dictionary contains: InventoryID or null if for those values inventory doesn't exist.
		/// Method uses MatrixHeader.Current (group id, column attribute, row attribute) to make result.
		/// </summary>
		protected virtual IDictionary<InventoryMapKey, InventoryMapValue> GetInventoryMap()
		{
			var inventoryMatrix = new Dictionary<InventoryMapKey, InventoryMapValue>();

			if (!AllAdditionalAttributesArePopulated())
				return inventoryMatrix;

			int? lastInventoryId = null;
			string rowAttributeValue = null;
			string columnAttributeValue = null;
			string[] additionalAttributeValues = new string[AdditionalAttributes.Current.AttributeIdentifiers.Length];

			foreach (PXResult<CSAnswers, CSAttributeGroup, InventoryItem> result in SelectInventoryWithAttributes())
			{
				InventoryItem inventoryItem = result;
				CSAnswers attribute = result;

				if (lastInventoryId != inventoryItem.InventoryID)
				{
					lastInventoryId = inventoryItem.InventoryID;
					rowAttributeValue = null;
					columnAttributeValue = null;
					for (int attributeIndex = 0; attributeIndex < additionalAttributeValues.Length; attributeIndex++)
						additionalAttributeValues[attributeIndex] = null;
				}

				if (string.Equals(attribute.AttributeID, Header.Current.RowAttributeID, StringComparison.OrdinalIgnoreCase))
				{
					rowAttributeValue = attribute.Value;
				}
				else if (string.Equals(attribute.AttributeID, Header.Current.ColAttributeID, StringComparison.OrdinalIgnoreCase))
				{
					columnAttributeValue = attribute.Value;
				}
				else
				{
					for (int attributeIndex = 0; attributeIndex < additionalAttributeValues.Length; attributeIndex++)
					{
						if (string.Equals(AdditionalAttributes.Current.AttributeIdentifiers[attributeIndex], attribute.AttributeID, StringComparison.OrdinalIgnoreCase) &&
							AdditionalAttributes.Current.Values[attributeIndex] == attribute.Value)
						{
							additionalAttributeValues[attributeIndex] = attribute.Value;
							break;
						}
					}
				}

				if (lastInventoryId != null && rowAttributeValue != null && columnAttributeValue != null &&
					additionalAttributeValues.All(v => v != null))
				{
					inventoryMatrix.Add(
						new InventoryMapKey(columnAttributeValue, rowAttributeValue),
						CreateInventoryMapValue(lastInventoryId, result));
					lastInventoryId = null;
				}
			}

			return inventoryMatrix;
		}

		protected virtual List<PXResult<CSAnswers, CSAttributeGroup, InventoryItem>> SelectInventoryWithAttributes()
		{
			var select = new PXSelectReadonly2<CSAnswers,
				InnerJoin<CSAttributeGroup, On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>,
				InnerJoin<InventoryItem, On<CSAnswers.refNoteID, Equal<InventoryItem.noteID>,
					And<CSAttributeGroup.entityClassID, Equal<InventoryItem.itemClassID>>>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
					And<InventoryItem.templateItemID, Equal<Required<EntryHeader.templateItemID>>>>>>,
				OrderBy<Asc<InventoryItem.inventoryID, Asc<CSAnswers.attributeID>>>>(Base);

			using (new PXFieldScope(select.View, typeof(InventoryItem.inventoryID), typeof(CSAnswers.attributeID), typeof(CSAnswers.value)))
			{
				return select.Select(GetTemplateID()).Cast<PXResult<CSAnswers, CSAttributeGroup, InventoryItem>>().ToList();
			}
		}

		protected virtual int? GetTemplateID()
		{
			return Header.Current.TemplateItemID;
		}

		protected virtual InventoryMapValue CreateInventoryMapValue(int? inventoryID, PXResult<CSAnswers, CSAttributeGroup, InventoryItem> result)
			=> new InventoryMapValue { InventoryID = inventoryID };

		/// <summary>
		/// Inserts matrix rows by inventoryMap and result of GetQty method.
		/// </summary>
		protected virtual void FillInventoryMatrix(IDictionary<InventoryMapKey, InventoryMapValue> inventoryMap, int? selectedColumn)
		{
			MatrixAttributeValues matrixValues = GetMatrixAttributeValues();

			for (int rowAttributeIndex = 0; rowAttributeIndex < matrixValues.RowValues.Length; rowAttributeIndex++)
			{
				var rowAttribute = matrixValues.RowValues[rowAttributeIndex];

				var newRow = new EntryMatrix();

				newRow.LineNbr = rowAttributeIndex;
				newRow.RowAttributeValue = rowAttribute.ValueID;
				newRow.RowAttributeValueDescr = rowAttribute.Description ?? rowAttribute.ValueID;

				newRow.ColAttributeValues = new string[matrixValues.ColumnValues.Length];
				newRow.ColAttributeValueDescrs = new string[matrixValues.ColumnValues.Length];
				newRow.InventoryIDs = new int?[matrixValues.ColumnValues.Length];
				newRow.Quantities = new decimal?[matrixValues.ColumnValues.Length];
				newRow.Errors = new string[matrixValues.ColumnValues.Length];
				newRow.Selected = new bool?[matrixValues.ColumnValues.Length];
				newRow.SelectedColumn = selectedColumn;

				for (int colAttributeIndex = 0; colAttributeIndex < matrixValues.ColumnValues.Length; colAttributeIndex++)
				{
					var colAttribute = matrixValues.ColumnValues[colAttributeIndex];
					newRow.ColAttributeValues[colAttributeIndex] = colAttribute.ValueID;
					newRow.ColAttributeValueDescrs[colAttributeIndex] = colAttribute.Description;

					inventoryMap.TryGetValue(new InventoryMapKey(colAttribute.ValueID, rowAttribute.ValueID), out InventoryMapValue inventoryValue);
					FillInventoryMatrixItem(newRow, colAttributeIndex, inventoryValue);

					if (rowAttributeIndex == 0)
						AddField(colAttributeIndex);
				}

				Matrix.Cache.SetStatus(newRow, PXEntryStatus.Held);
			}
		}

		protected virtual MatrixAttributeValues GetMatrixAttributeValues()
		{
			CSAttributeDetail[] colAttributeValues;
			CSAttributeDetail[] rowAttributeValues;

			var colAttributesSelect = new PXSelectReadonly<CSAttributeDetail,
				Where<CSAttributeDetail.attributeID, Equal<Current<EntryHeader.colAttributeID>>>,
				OrderBy<Asc<CSAttributeDetail.sortOrder>>>(Base);

			if (!ShowDisabledValue)
				colAttributesSelect.WhereAnd<Where<CSAttributeDetail.disabled, Equal<False>>>();

			colAttributeValues = colAttributesSelect.SelectMain();
			var rowAttributesSelect = new PXSelectReadonly<CSAttributeDetail,
				Where<CSAttributeDetail.attributeID, Equal<Current<EntryHeader.rowAttributeID>>>,
				OrderBy<Asc<CSAttributeDetail.sortOrder>>>(Base);

			if (!ShowDisabledValue)
				rowAttributesSelect.WhereAnd<Where<CSAttributeDetail.disabled, Equal<False>>>();

			rowAttributeValues = rowAttributesSelect.SelectMain();

			return new MatrixAttributeValues() { ColumnValues = colAttributeValues, RowValues = rowAttributeValues };
		}

		protected abstract void FillInventoryMatrixItem(EntryMatrix newRow, int colAttributeIndex, InventoryMapValue inventoryValue);

		protected virtual EntryMatrix GetFirstMatrixRow()
		{
			return Matrix.Cache.Cached.Cast<EntryMatrix>().FirstOrDefault(r => r.IsPreliminary != true && r.IsTotal != true);
		}

		protected static TResult GetValueFromArray<TResult>(TResult[] array, int index)
		{
			if (index >= 0 && index < array?.Length)
				return (TResult)array[index];

			return default(TResult);
		}

		public PXAction<MainItemType> MatrixGridCellChanged;
		[PXUIField(Visible = false, Enabled = true, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(CommitChanges = true)]
		public virtual IEnumerable matrixGridCellChanged(PXAdapter adapter)
		{
			string columnName = adapter.CommandArguments?.TrimEnd();

			if (string.IsNullOrEmpty(columnName) ||
				columnName.StartsWith(MatrixFieldName) != true ||
				!int.TryParse(columnName.Substring(MatrixFieldName.Length), out int columnIndex) ||
				columnIndex < 0)
			{
				columnIndex = -1;
			}

			bool changed = false;

			foreach (EntryMatrix matrixRow in Matrix.Cache.Cached)
				if (matrixRow.SelectedColumn != columnIndex)
				{
					changed = true;
					matrixRow.SelectedColumn = columnIndex;
				}

			if (changed)
				OnMatrixGridCellCahnged();

			return adapter.Get();
		}

		protected virtual void OnMatrixGridCellCahnged()
		{
		}

		#endregion
	}
}
