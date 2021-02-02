using PX.CloudServices.DocumentRecognition;
using PX.Common;
using PX.Data;
using PX.Data.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.AP.InvoiceRecognition
{
	internal class FullTextComparer : IEqualityComparer<FullTextTerm>
	{
		private readonly StringComparer _stringComparer = StringComparer.CurrentCultureIgnoreCase;

		public bool Equals(FullTextTerm x, FullTextTerm y)
		{
			return _stringComparer.Equals(x?.Text, y?.Text);
		}

		public int GetHashCode(FullTextTerm obj)
		{
			return _stringComparer.GetHashCode(obj?.Text);
		}
	}

	internal class InvoiceDataLoader
	{
		private const string DateTimeStringFormat = "yyyy-MM-dd";
		private const char ViewPlusFieldNameSeparator = '.';

		private readonly Document _recognizedDocument;
		private readonly APInvoiceRecognitionEntry _graph;
		private readonly IEntitySearchService _entitySearchService;
		private readonly PXCache _vendorCache;

		public FullTextTerm VendorTerm { get; private set; }

		public InvoiceDataLoader(DocumentRecognitionResult recognitionResult, APInvoiceRecognitionEntry graph)
		{
			recognitionResult.ThrowOnNull(nameof(recognitionResult));
			graph.ThrowOnNull(nameof(graph));

			_recognizedDocument = recognitionResult?.Documents?.Count > 0 ?
				recognitionResult.Documents[0] :
				null;
			_graph = graph;
			_entitySearchService = graph.EntitySearchService;
			_vendorCache = graph.Vendors.Cache;
		}

		public void Load(object primaryRow)
		{
			primaryRow.ThrowOnNull(nameof(primaryRow));

			LoadPrimary(primaryRow);
			LoadDetails();
		}

		private void LoadPrimary(object row)
		{
			row.ThrowOnNull(nameof(row));

			var areFieldsRecognized = _recognizedDocument?.Fields != null;
			if (!areFieldsRecognized)
			{
				return;
			}

			foreach (var fieldPair in _recognizedDocument.Fields)
			{
				var viewPlusFieldName = fieldPair.Key;
				if (string.IsNullOrWhiteSpace(viewPlusFieldName))
				{
					continue;
				}

				var (viewName, fieldName) = GetFieldInfo(fieldPair.Key);
				if (string.IsNullOrWhiteSpace(viewName) || string.IsNullOrWhiteSpace(fieldName))
				{
					continue;
				}

				var cache = _graph.Views[viewName].Cache;
				LoadPrimaryField(cache, row, viewName, fieldName, fieldPair.Value);
			}
		}

		internal static (string ViewName, string FieldName) GetFieldInfo(string viewNameFieldName)
		{
			var names = viewNameFieldName.Split(ViewPlusFieldNameSeparator);

			if (names.Length != 2)
			{
				return (null, null);
			}

			return (names[0], names[1]);
		}

		private void LoadPrimaryField(PXCache cache, object row, string viewName, string fieldName, Field field)
		{
			if (fieldName.Equals(nameof(APInvoice.VendorID), StringComparison.OrdinalIgnoreCase) &&
				field.Value == null && field.FullTextTerms != null)
			{
				var searchResult = FindVendor(field.FullTextTerms);

				field.Value = searchResult.Name;
				VendorTerm = searchResult.Term;
			}

			SetFieldExtValue(cache, row, fieldName, field);
		}

		private (string Name, FullTextTerm Term) FindVendor(IList<FullTextTerm> fullTextTerms)
		{
			var fullTextTermsDistinct = fullTextTerms.Distinct(new FullTextComparer());
			var ventorEntityType = typeof(VendorR).FullName;
			var rankedSearchResults = new Dictionary<Guid?, Tuple<EntitySearchResult, int, FullTextTerm>>();

			foreach (var term in fullTextTermsDistinct)
			{
				var searchResult = _entitySearchService.Search(term.Text, 0, 10)
					.Where(r => r.EntityType == ventorEntityType);

				foreach (var s in searchResult)
				{
					if (rankedSearchResults.ContainsKey(s.NoteID))
					{
						var rankedResult = rankedSearchResults[s.NoteID];
						rankedSearchResults[s.NoteID] = new Tuple<EntitySearchResult, int, FullTextTerm>(rankedResult.Item1, rankedResult.Item2 + 1, term);
					}
					else
					{
						rankedSearchResults.Add(s.NoteID, new Tuple<EntitySearchResult, int, FullTextTerm>(s, 1, term));
					}
				}
			}

			if (rankedSearchResults.Count == 0)
			{
				return (null, null);
			}

			var relevantResult = rankedSearchResults.First();

			foreach (var result in rankedSearchResults.Skip(1))
			{
				if (result.Value.Item2 > relevantResult.Value.Item2)
				{
					relevantResult = result;
				}
			}

			if (_vendorCache.LocateByNoteID(relevantResult.Key.Value) != 1)
			{
				return (null, null);
			}

			var vendorRow = (VendorR)_vendorCache.Current;

			return (vendorRow.AcctCD, relevantResult.Value.Item3);
		}

		private void LoadDetails()
		{
			var areDetailsRecognized = _recognizedDocument?.Details != null;
			if (!areDetailsRecognized)
			{
				return;
			}

			foreach (var detailPair in _recognizedDocument.Details)
			{
				var rows = detailPair.Value?.Value;
				if (rows == null)
				{
					continue;
				}

				LoadDetailsRows(rows);
			}
		}

		private void LoadDetailsRows(IList<DetailValue> rows)
		{
			foreach (var row in rows)
			{
				var fields = row.Fields;
				if (fields == null)
				{
					continue;
				}

				LoadDetailsRow(fields);
			}
		}

		private void LoadDetailsRow(Dictionary<string, Field> fields)
		{
			object row = null;

			foreach (var fieldPair in fields)
			{
				var viewPlusFieldName = fieldPair.Key;
				if (string.IsNullOrWhiteSpace(viewPlusFieldName))
				{
					continue;
				}

				var (viewName, fieldName) = GetFieldInfo(fieldPair.Key);
				if (string.IsNullOrWhiteSpace(viewName) || string.IsNullOrWhiteSpace(fieldName))
				{
					continue;
				}

				var cache = _graph.Views[viewName].Cache;

				if (row == null)
				{
					row = cache.Insert();
				}

				LoadDetailsField(cache, row, viewName, fieldName, fieldPair.Value);
			}
		}

		private void LoadDetailsField(PXCache cache, object row, string viewName, string fieldName, Field field)
		{
			SetFieldExtValue(cache, row, fieldName, field);

			// To update parent's Details Total
			cache.Update(row);
		}

		private void SetFieldExtValue(PXCache cache, object row, string fieldName, Field field)
		{
			if (field == null)
			{
				return;
			}

			var fieldState = cache.GetStateExt(null, fieldName) as PXFieldState;
			if (fieldState == null || fieldState.DataType == null)
			{
				return;
			}

			var fieldType = fieldState.DataType;
			var extValue = ParseExtValue(fieldType, field.Value, field.Ocr?.Text);
			if (extValue == null)
			{
				return;
			}

			var valueType = extValue.GetType();
			var needToConvertValue = !fieldType.IsAssignableFrom(valueType);
			if (needToConvertValue)
			{
				if (valueType == typeof(string) && fieldType == typeof(DateTime?) &&
					DateTime.TryParseExact(extValue as string, DateTimeStringFormat, null, DateTimeStyles.None, out var dateExtValue))
				{
					extValue = (DateTime?)dateExtValue;
				}
				else
				{
					extValue = Convert.ChangeType(extValue, fieldType);
				}
			}

			try
			{
				cache.SetValueExt(row, fieldName, extValue);
			}
			catch (PXSetPropertyException e)
			{
				cache.RaiseExceptionHandling(fieldName, row, extValue, e);
			}
		}

		private object ParseExtValue(Type fieldType, object fieldValue, string fieldTextValue)
		{
			if (fieldValue != null)
			{
				return fieldValue;
			}

			if (string.IsNullOrEmpty(fieldTextValue))
			{
				return null;
			}

			if (fieldType == typeof(string))
			{
				return fieldTextValue;
			}

			if (fieldType == typeof(int?) && int.TryParse(fieldTextValue, out var intExtValue))
			{
				return (int?)intExtValue;
			}

			if (fieldType == typeof(decimal?) && decimal.TryParse(fieldTextValue, out var decimalExtValue))
			{
				return (decimal?)decimalExtValue;
			}

			if (fieldType == typeof(DateTime?) &&
				DateTime.TryParseExact(fieldTextValue, DateTimeStringFormat, null, DateTimeStyles.None, out var dateExtValue))
			{
				return (DateTime?)dateExtValue;
			}

			return null;
		}
	}
}
