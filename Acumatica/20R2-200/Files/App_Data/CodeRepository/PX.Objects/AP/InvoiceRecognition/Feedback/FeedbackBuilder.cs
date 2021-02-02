using Newtonsoft.Json;
using PX.CloudServices.DocumentRecognition;
using PX.Common;
using PX.Data;
using PX.Data.Search;
using PX.Objects.CM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AP.InvoiceRecognition.Feedback
{
	internal class FeedbackBuilder
	{
		private const char ViewFieldNameSeparator = '.';
		private const string DateStringFormat = "yyyy-MM-dd";
		private const string _primaryViewName = nameof(APInvoiceRecognitionEntry.Document);

		private readonly string _vendorInvoiceFieldName = GetDocumentFieldName(nameof(APInvoiceRecognitionEntry.Document),
			nameof(APInvoice.VendorID));

		private static readonly string[] _primaryReadonlyFieldsToBypass =
		{
			CurrencyInfoAttribute._CuryViewField,
			nameof(APInvoice.CuryLineTotal),
		};

		private readonly HashSet<string> _primaryFields;
		private readonly HashSet<string> _detailFields;

		private static readonly HashSet<Type> _numericFieldTypes = new HashSet<Type>
		{
			typeof(byte),
			typeof(short),
			typeof(int),
			typeof(long),
			typeof(decimal),
			typeof(float),
			typeof(double)
		};

		private readonly List<TableContext> _tableContextDump = new List<TableContext>();
		private TableContext _currentTableContext;

		public FeedbackBuilder(PXCache recognizedInvoiceCache, HashSet<string> primaryFields, HashSet<string> detailFields)
		{
			recognizedInvoiceCache.ThrowOnNull(nameof(recognizedInvoiceCache));
			primaryFields.ThrowOnNull(nameof(primaryFields));
			detailFields.ThrowOnNull(nameof(detailFields));

			_primaryFields = primaryFields;
			_detailFields = detailFields;

			var fieldsToBypass = new HashSet<string>(recognizedInvoiceCache.Fields, StringComparer.OrdinalIgnoreCase);
			var invoiceCache = new PXGraph().Caches[typeof(APInvoice)];
			fieldsToBypass.ExceptWith(invoiceCache.Fields);
			fieldsToBypass.UnionWith(_primaryReadonlyFieldsToBypass);

			_primaryFields.ExceptWith(fieldsToBypass);
		}

		private static string GetDocumentFieldName(string viewName, string fieldName)
		{
			return $"{viewName}{ViewFieldNameSeparator}{fieldName}";
		}

		public void ProcessCellBound(string cellBoundJson)
		{
			if (string.IsNullOrEmpty(cellBoundJson))
			{
				return;
			}

			CellBound cellBound;

			try
			{
				cellBound = JsonConvert.DeserializeObject<CellBound>(cellBoundJson);
			}
			catch (JsonSerializationException e)
			{
				PXTrace.WriteError("Malformed cell bound json:\n{error}\n{cellBoundJson}", e.Message, cellBoundJson);

				return;
			}

			if (string.IsNullOrWhiteSpace(cellBound.DetailColumn))
			{
				PXTrace.WriteError("Inconsistent cell bound json:\n{cellBoundJson}", cellBoundJson);

				return;
			}

			if (_currentTableContext != null && !_currentTableContext.CanBeBounded(cellBound))
			{
				_tableContextDump.Add(_currentTableContext);
				_currentTableContext = null;
			}

			RegisterCellBound(cellBound);
		}

		public void DumpTableFeedback()
		{
			if (_currentTableContext == null)
			{
				return;
			}

			_tableContextDump.Add(_currentTableContext);
			_currentTableContext = null;
		}

		private void RegisterCellBound(CellBound cellBound)
		{
			if (_currentTableContext == null)
			{
				_currentTableContext = new TableContext(cellBound.Page, cellBound.Table);
			}

			if (cellBound.Columns.Count == 1 && cellBound.Columns[0] == -1)
			{
				_currentTableContext.RegisterColumnUnbound(cellBound.DetailColumn);

				return;
			}

			_currentTableContext.RegisterColumnSelected(cellBound.DetailColumn, cellBound.Columns);
			_currentTableContext.RegisterRowBound(cellBound.DetailRow, cellBound.Row);
		}

		public List<VersionedFeedback> ToTableFeedbackList(string detailViewName)
		{
			var feedbackList = new List<VersionedFeedback>();

			DumpTableFeedback();

			foreach (var tableContext in _tableContextDump)
			{
				feedbackList.Add(GetTableSelectedFeedback(detailViewName, tableContext.Page, tableContext.Table));

				var columnSelectedFeedback = tableContext.ColumnSelected
					.Select(columnSelected => new
					{
						Value = GetColumnBoundFeedback(detailViewName, tableContext.Page, tableContext.Table,
							columnSelected.DetailColumn, columnSelected.Columns),
						columnSelected.Created
					});

				var columnUnboundFeedback = tableContext.ColumnUnbound
					.Select(columnUnbound => new
					{
						Value = GetColumnUnboundFeedback(detailViewName, columnUnbound.DetailColumn),
						columnUnbound.Created
					});

				var colunnFeedback = columnSelectedFeedback
					.Concat(columnUnboundFeedback)
					.OrderBy(feedback => feedback.Created)
					.Select(feedback => feedback.Value);

				feedbackList.AddRange(colunnFeedback);

				var rowByDetailRow = tableContext.RowByDetailRow.ToList();
				if (rowByDetailRow.Count != 0)
				{
					feedbackList.Add(GetRowsBoundFeedback(detailViewName, tableContext.Page, tableContext.Table, rowByDetailRow));
				}
			}

			_tableContextDump.Clear();

			return feedbackList;
		}

		private VersionedFeedback GetRowsBoundFeedback(string detailViewName, short page, short table,
			IEnumerable<KeyValuePair<short, short>> rowByDetailRow)
		{
			var detail = new Detail
			{
				Value = new List<DetailValue>()
			};

			var prevRowIndex = -1;

			foreach (var rowPair in rowByDetailRow)
			{
				var rowIndexDelta = rowPair.Key - prevRowIndex;
				if (rowIndexDelta > 1)
				{
					for (var i = 0; i < rowIndexDelta - 1; i++)
					{
						detail.Value.Add(null);
					}
				}

				prevRowIndex = rowPair.Key;

				var detailValue = new DetailValue
				{
					BoundingBoxes = new List<BoundingBoxDetail>
					{
						new BoundingBoxDetail
						{
							Page = page,
							Table = table,
							Row = rowPair.Value
						}
					}
				};

				detail.Value.Add(detailValue);
			}

			return CreateTableVersionedFeedback(detailViewName, detail);
		}

		private VersionedFeedback GetTableSelectedFeedback(string detailViewName, short page, short table)
		{
			var detail = new Detail
			{
				Ocr = new List<DetailOcr>
				{
					{
						new DetailOcr
						{
							Page = page,
							Table = table
						}
					}
				}
			};

			return CreateTableVersionedFeedback(detailViewName, detail);
		}

		private VersionedFeedback GetColumnBoundFeedback(string detailViewName, short page, short table, string columnName, List<short> columnIndexes)
		{
			var detail = new Detail
			{
				Ocr = new List<DetailOcr>
				{
					new DetailOcr
					{
						Page = page,
						Table = table,
						FieldColumns = new Dictionary<string, List<short>>
						{
							[columnName] = columnIndexes
						}
					}
				}
			};

			return CreateTableVersionedFeedback(detailViewName, detail);
		}

		private VersionedFeedback GetColumnUnboundFeedback(string detailViewName, string columnName)
		{
			var detail = new Detail
			{
				Ocr = new List<DetailOcr>
				{
					new DetailOcr
					{
						FieldColumns = new Dictionary<string, List<short>>
						{
							[columnName] = new List<short>()
						}
					}
				}
			};

			return CreateTableVersionedFeedback(detailViewName, detail);
		}

		private VersionedFeedback CreateTableVersionedFeedback(string detailViewName, Detail detail)
		{
			var feedback = new VersionedFeedback
			{
				Documents = new List<Document>()
			};

			var document = new Document
			{
				Details = new Dictionary<string, Detail>()
			};

			feedback.Documents.Add(document);
			document.Details.Add(detailViewName, detail);

			return feedback;
		}

		public VersionedFeedback ToFieldBoundFeedback(string documentJson)
		{
			if (string.IsNullOrWhiteSpace(documentJson))
			{
				return null;
			}

			Dictionary<string, Field> documentFields;

			try
			{
				documentFields = JsonConvert.DeserializeObject<Dictionary<string, Field>>(documentJson);
			}
			catch (JsonSerializationException e)
			{
				PXTrace.WriteError("Malformed document json:\n{error}\n{documentJson}", e.Message, documentJson);

				return null;
			}

			var fieldKeyValuePair = documentFields.First();

			var viewNameFieldName = fieldKeyValuePair.Key;
			var (viewName, fieldName) = InvoiceDataLoader.GetFieldInfo(viewNameFieldName);
			if (viewName == null || fieldName == null)
			{
				return null;
			}

			var bypass = !viewName.Equals(_primaryViewName, StringComparison.OrdinalIgnoreCase) || !_primaryFields.Contains(fieldName);
			if (bypass)
			{
				return null;
			}

			var feedback = new VersionedFeedback
			{
				Documents = new List<Document>(new[]
				{
					new Document
					{
						Fields = documentFields
					}
				})
			};

			return feedback;
		}

		public VersionedFeedback ToRecordSavedFeedback(PXView primaryView, APInvoice primaryRow,
			PXView detailView, IEnumerable<APTran> detailRows, IEntitySearchService entitySearchService)
		{
			primaryRow.ThrowOnNull(nameof(primaryRow));
			detailRows.ThrowOnNull(nameof(detailRows));
			entitySearchService.ThrowOnNull(nameof(entitySearchService));

			var recordSavedFeedback = new VersionedFeedback
			{
				Documents = new List<Document>()
			};

			var document = new Document();
			recordSavedFeedback.Documents.Add(document);

			FillDocumentFields(document, primaryView, primaryRow, entitySearchService);
			FillDocumentDetails(document, detailView, detailRows);

			return recordSavedFeedback;
		}

		private void FillDocumentFields(Document document, PXView view, APInvoice row, IEntitySearchService entitySearchService)
		{
			var trackedFields = view.Cache.Fields
				.Where(f => _primaryFields.Contains(f))
				.ToList();
			if (trackedFields.Count == 0)
			{
				return;
			}

			document.Fields = new Dictionary<string, Field>();

			FillRowTrackedFields(view, row, trackedFields, document.Fields, entitySearchService);
		}

		private void FillRowTrackedFields(PXView view, object row, IEnumerable<string> trackedFields, Dictionary<string, Field> fieldContainer,
			IEntitySearchService entitySearchService)
		{
			foreach (var field in trackedFields)
			{
				var documentFieldName = GetDocumentFieldName(view.Name, field);
				var documentField = new TypedField();

				fieldContainer[documentFieldName] = documentField;

				var isVendorInvoiceField = documentFieldName.Equals(_vendorInvoiceFieldName, StringComparison.OrdinalIgnoreCase);
				var (type, value) = GetFieldInfo(view.Cache, field, row, isVendorInvoiceField);
				var fullTextTerm = isVendorInvoiceField ?
					GetVendorFullTextTerm(view.Cache, row, value, entitySearchService) :
					null;

				FillDocumentField(documentField, value, type, fullTextTerm);
			}
		}

		private (FieldTypes? Type, object Value) GetFieldInfo(PXCache cache, string fieldName, object row, bool isVendorInvoiceField)
		{
			var value = PXFieldState.UnwrapValue(cache.GetValueExt(row, fieldName));

			if (isVendorInvoiceField)
			{
				return (FieldTypes.SearchTerms, null);
			}

			var state = cache.GetStateExt(row, fieldName) as PXFieldState;

			if (state.DataType == typeof(string))
			{
				return (FieldTypes.String, ((string)value)?.TrimEnd());
			}

			if (_numericFieldTypes.Contains(state.DataType))
			{
				return (FieldTypes.Number, value);
			}

			if (state.DataType == typeof(DateTime))
			{
				var cacheAttributes = cache.GetAttributesReadonly(row, fieldName);

				var isUnboundDateAttribute = cacheAttributes
					.OfType<PXDateAttribute>()
					.Any();
				if (isUnboundDateAttribute)
				{
					return (FieldTypes.DateTime, value);
				}

				var boundDateAttribute = cacheAttributes
					.OfType<PXDBDateAttribute>()
					.FirstOrDefault();
				if (boundDateAttribute == null || boundDateAttribute is PXDBTimeAttribute)
				{
					return (null, value);
				}

				if (boundDateAttribute.PreserveTime)
				{
					return (FieldTypes.DateTime, value);
				}

				var dateValue = value as DateTime?;
				if (dateValue != null)
				{
					value = dateValue.Value.ToString(DateStringFormat);
				}

				return (FieldTypes.Date, value);
			}

			return (null, value);
		}

		private void FillDocumentField(TypedField field, object value, FieldTypes? type, FullTextTerm fullTextTerm)
		{
			if (fullTextTerm == null)
			{
				field.Value = value;
			}
			else
			{
				field.FullTextTerms = new List<FullTextTerm>(new[] { fullTextTerm });
			}

			field.Type = type;
		}

		private FullTextTerm GetVendorFullTextTerm(PXCache cache, object row, object value, IEntitySearchService entitySearchService)
		{
			if (entitySearchService == null)
			{
				return null;
			}

			var vendorId = cache.GetValue<APInvoice.vendorID>(row) as int?;
			if (vendorId == null)
			{
				return null;
			}

			var vendor = Vendor.PK.Find(cache.Graph, vendorId);
			if (vendor == null)
			{
				return null;
			}

			var noteId = vendor.NoteID;
			if (noteId == null)
			{
				return null;
			}

			var searchContent = entitySearchService.GetSearchIndexContent(noteId.Value);
			if (searchContent == null)
			{
				return null;
			}

			return new FullTextTerm { Text = searchContent };
		}

		private void FillDocumentDetails(Document document, PXView view, IEnumerable<APTran> rows)
		{
			var trackedFields = view.Cache.Fields
				.Where(f => _detailFields.Contains(f))
				.ToList();
			if (trackedFields.Count == 0)
			{
				return;
			}

			document.Details = new Dictionary<string, Detail>();

			var detail = new Detail
			{
				Value = new List<DetailValue>()
			};

			document.Details[view.Name] = detail;

			foreach (var r in rows)
			{
				var detailValue = new DetailValue()
				{
					Fields = new Dictionary<string, Field>()
				};

				detail.Value.Add(detailValue);

				FillRowTrackedFields(view, r, trackedFields, detailValue.Fields, null);
			}
		}
	}
}
