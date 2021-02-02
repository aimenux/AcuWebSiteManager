using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Descriptor.Attributes;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GDPR;
using CommonMessages = PX.Objects.CN.Common.Descriptor.SharedMessages;

namespace PX.Objects.CN.Common.Services
{
	public class CommonAttributeColumnCreator
	{
		private readonly PXGraph graph;
		private readonly PXSelectBase<CSAnswers> answers;
		private readonly PXSelectBase<CSAttributeGroup> attributeGroups;
		private readonly EntityHelper entityHelper;
		private PXCache documentCache;
		private bool areControlsEnabled;

		public CommonAttributeColumnCreator(PXGraph graph,
			PXSelectBase<CSAttributeGroup> attributeGroups)
		{
			this.graph = graph;
			this.attributeGroups = attributeGroups;
			entityHelper = new EntityHelper(graph);
			answers = new PXSelect<CSAnswers>(graph);
		}

		public void GenerateColumns(PXCache cache, string documentsView, string answerView,
			bool areColumnsEnabled = true)
		{
			areControlsEnabled = areColumnsEnabled;
			documentCache = cache;
			RemoveUnboundFields(cache);
			attributeGroups.Select().FirstTableItems.OrderBy(x => x.SortOrder)
				.ForEach(attributeGroup => ProcessAttributeGroup(cache, documentsView, attributeGroup));
			AddRowEventHandlers(documentsView, answerView);
		}

		private void ProcessAttributeGroup(PXCache cache, string documentsView,
			CSAttributeGroup attributeGroup)
		{
			if (!cache.Fields.Contains(attributeGroup.AttributeID.Trim()))
			{
				AddFieldsAndFieldEventHandlers(cache, documentsView, attributeGroup);
			}
		}

		private void AddFieldsAndFieldEventHandlers(PXCache cache, string documentsView,
			CSAttributeGroup attributeGroup)
		{
			var attributeId = attributeGroup.AttributeID.Trim();
			cache.Fields.Add(attributeId);
			graph.FieldSelecting.AddHandler(documentsView, attributeId,
				(sender, args) => CreateControl(args, attributeGroup, attributeId));
			graph.FieldUpdating.AddHandler(documentsView, attributeId,
				(sender, args) => InsertOrUpdateAnswerValue(args, attributeGroup));
		}

		private void AddRowEventHandlers(string documentsView, string answerView)
		{
			graph.RowPersisting.AddHandler(answerView, ValidateAnswer);
			graph.RowPersisting.AddHandler(documentsView, ValidateDocuments);
			graph.RowDeleted.AddHandler(documentsView, DeleteAnswers);
			graph.RowInserting.AddHandler(documentsView,
				(cache, args) => InsertAnswers(args.Row));
			graph.RowUpdating.AddHandler(documentsView,
				(cache, args) => InsertAnswers(args.Row));
		}

		private void CreateControl(PXFieldSelectingEventArgs arguments, CSAttributeGroup attributeGroup,
			string attributeId)
		{
			var attribute = GetAttribute(attributeGroup.AttributeID);
			switch (attribute.ControlType)
			{
				case CSAttribute.Text:
					CreateStringControl(arguments, attributeGroup);
					break;
				case CSAttribute.CheckBox:
					CreateCheckBoxControl(arguments, attributeGroup);
					break;
				case CSAttribute.Combo:
					CreateComboControl(attributeGroup, arguments, false);
					break;
				case CSAttribute.Datetime:
					CreateDatetimeControl(attributeGroup, arguments);
					break;
				case CSAttribute.MultiSelectCombo:
					CreateComboControl(attributeGroup, arguments, true);
					break;
				default:
					arguments.ReturnState = null;
					break;
			}
			ProcessReturnState(arguments, attributeId, attributeGroup);
		}

		private void ProcessReturnState(PXFieldSelectingEventArgs arguments, string attributeId,
			CSAttributeGroup attributeGroup)
		{
			if (arguments.ReturnState is PXFieldState fieldState)
			{
				if (arguments.Row != null)
				{
					UpdateFieldStateForExistingDocument(attributeId, arguments.Row, fieldState);
				}
				fieldState.SetFieldName(attributeId);
				fieldState.Visible = attributeGroup.IsActive == true;
				fieldState.Visibility = PXUIVisibility.Visible;
				fieldState.DisplayName = attributeGroup.Description.Trim();
				fieldState.Enabled = areControlsEnabled;
			}
		}

		private void UpdateFieldStateForExistingDocument(string attributeId, object document,
			PXFieldState fieldState)
		{
			var result = GetAnswer(attributeId, document);
			if (result != null)
			{
				fieldState.Value = result.Value;
			}
		}

		private void InsertOrUpdateAnswerValue(PXFieldUpdatingEventArgs arguments, CSAttributeGroup group)
		{
			if (arguments.Row != null)
			{
				var answer = GetAnswer(group.AttributeID, arguments.Row);
				if (answer != null)
				{
					UpdateAnswer(arguments.NewValue, answer);
				}
				else
				{
					InsertAnswer(group, arguments.Row, arguments.NewValue.ToString());
				}
			}
		}

		private void ValidateAnswer(PXCache cache, PXRowPersistingEventArgs arguments)
		{
			if (arguments.Row is CSAnswers answer &&
				(arguments.Operation == PXDBOperation.Insert ||
				 arguments.Operation == PXDBOperation.Update))
			{
				var group = attributeGroups.Search<CSAttributeGroup.attributeID>(answer.AttributeID)
					.FirstTableItems.Single();
				if (group.Required.GetValueOrDefault() && string.IsNullOrEmpty(answer.Value))
				{
					RaiseExceptionForEmptyAttributeValue(answer);
				}
			}
		}

		private void RaiseExceptionForEmptyAttributeValue(CSAnswers attribute)
		{
			var message = string.Format(CommonMessages.FieldIsEmpty, attribute.AttributeID);
			var documentType = documentCache.GetItemType();
			var document = entityHelper.GetEntityRow(documentType, attribute.RefNoteID);
			var fieldName = document.GetType().GetProperties()
				.FirstOrDefault(x => x.GetCustomAttributes(typeof(UiInformationFieldAttribute)).Any())?.Name;
			documentCache.RaiseExceptionHandling(fieldName,
				document, true, new PXSetPropertyException(message, PXErrorLevel.RowError));
		}

		private void DeleteAnswers(PXCache cache, PXRowDeletedEventArgs arguments)
		{
			var document = arguments.Row;
			if (document != null)
			{
				var noteId = entityHelper.GetEntityNoteID(document);
				if (noteId.HasValue)
				{
					var answersToDelete = GetAnswersToDelete(document, noteId)
						.Select(x => CastAnswerToViewType(x));
					answers.Cache.DeleteAll(answersToDelete);
				}
			}
		}

		private IEnumerable<CSAnswers> GetAnswersToDelete(object document, Guid? noteId)
		{
			var answerCache = graph.Caches[typeof(CSAnswers)];
			var entityCache = graph.Caches[document.GetType()];
			var status = entityCache.GetStatus(document);
			return status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted
				? GetInsertedAnswers(answerCache, noteId)
				: GetExistingAnswers(noteId);
		}

		private void ValidateDocuments(PXCache cache, PXRowPersistingEventArgs arguments)
		{
			if ((arguments.Operation == PXDBOperation.Insert ||
				arguments.Operation == PXDBOperation.Update) &&
				arguments.Row != null)
			{
				var emptyRequired = new List<string>();
				ValidateDocumentAttributes(arguments.Row, emptyRequired);
				if (emptyRequired.Count > 0)
				{
					throw new PXException(CommonMessages.RequiredAttributesAreEmpty,
						string.Join(", ", emptyRequired.Select(s => $"'{s}'")));
				}
			}
		}

		private void ValidateDocumentAttributes(object document, List<string> emptyRequired)
		{
			var noteId = entityHelper.GetEntityNoteID(document);
			var attributes = answers.Cache.Cached
				.Select<CSAnswers>()
				.Where(x => x.RefNoteID == noteId);
			attributes.ForEach(attribute => ValidateDocumentAttribute(attribute, emptyRequired, document));
		}

		private void ValidateDocumentAttribute(CSAnswers attribute, ICollection<string> emptyRequired,
			object document)
		{
			var group = attributeGroups.Search<CSAttributeGroup.attributeID>(attribute.AttributeID)
				.FirstTableItems.Single();
			if (group.Required.GetValueOrDefault() && string.IsNullOrEmpty(attribute.Value))
			{
				emptyRequired.Add(attribute.AttributeID);
				RaiseExceptionForDocumentAttribute(attribute, document);
			}
		}

		private void RaiseExceptionForDocumentAttribute(CSAnswers attribute,
			object document)
		{
			var message = string.Format(CommonMessages.FieldIsEmpty, attribute.AttributeID);
			var fieldName = document.GetType().GetProperties()
				.FirstOrDefault(x => x.GetCustomAttributes(typeof(UiInformationFieldAttribute)).Any())?.Name;
			documentCache.RaiseExceptionHandling(fieldName,
				document, true, new PXSetPropertyException(message, PXErrorLevel.RowError));
			PXUIFieldAttribute.SetError<CSAnswers.value>(answers.Cache, attribute, message);
		}

		private void InsertAnswers(object document)
		{
			if (document != null)
			{
				attributeGroups.Select().FirstTableItems
					.ForEach(group => InsertAnswer(group, document));
			}
		}

		private void UpdateAnswer(object newValue, CSAnswers updatedAnswer)
		{
			updatedAnswer.Value = newValue?.ToString();
			if (updatedAnswer.GetType() != answers.Cache.GetItemType())
			{
				var castAnswer = CastAnswerToViewType(updatedAnswer);
				castAnswer.NoteId = updatedAnswer.GetExtension<CSAnswersExt>()?.NoteID;
				answers.Cache.Update(castAnswer);
			}
			answers.Cache.Update(updatedAnswer);
		}

		private void InsertAnswer(CSAttributeGroup group, object document, string value = null)
		{
			var noteId = entityHelper.GetEntityNoteID(document);
			var existingAnswer = GetAnswer(group.AttributeID, document);
			if (existingAnswer == null)
			{
				var answer = CreateAnswer(group, value, noteId);
				var castAnswer = CastAnswerToViewType(answer);
				answers.Cache.Insert(castAnswer);
			}
		}

		private dynamic CastAnswerToViewType(CSAnswers answer)
		{
			var type = answers.Cache.GetItemType();
			return answer.Cast(type);
		}

		private CSAnswers GetAnswer(string attributeId, object document)
		{
			var noteId = entityHelper.GetEntityNoteID(document);
			return answers.Search<CSAnswers.attributeID,
				CSAnswers.refNoteID>(attributeId, noteId);
		}

		private void CreateDatetimeControl(CSAttributeGroup attributeGroup, PXFieldSelectingEventArgs args)
		{
			var required = attributeGroup.Required == true
				? 1
				: -1;
			var attribute = GetAttribute(attributeGroup.AttributeID);
			args.ReturnState = PXDateState.CreateInstance(args.ReturnState, nameof(CSAnswers.value),
				false, required, attribute.EntryMask, attribute.EntryMask, null, null);
		}

		private void CreateComboControl(CSAttributeGroup attributeGroup, PXFieldSelectingEventArgs args,
			bool isMultiSelect)
		{
			var details = GetAttributeDetails(attributeGroup.AttributeID);
			var values = details.ToDictionary(detail => detail.ValueID, detail => detail.Description);
			var required = attributeGroup.Required == true
				? 1
				: -1;
			args.ReturnState = PXStringState.CreateInstance(args.ReturnState, 255, null,
				nameof(CSAnswers.value), false, required, null, values.Keys.ToArray(),
				values.Values.ToArray(), true, null);
			((PXStringState)args.ReturnState).MultiSelect = isMultiSelect;
		}

		private IEnumerable<CSAttributeDetail> GetAttributeDetails(string attributeId)
		{
			return PXSelect<CSAttributeDetail,
					Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>,
						And<CSAttributeDetail.disabled, NotEqual<True>>>>
				.Select(graph, attributeId).FirstTableItems;
		}

		private CSAttribute GetAttribute(string attributeId)
		{
			return new PXSelect<CSAttribute,
					Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>(graph)
				.SelectSingle(attributeId);
		}

		private static void RemoveUnboundFields(PXCache cache)
		{
			var customFieldsToRemove = GetCustomFieldsToRemove(cache);
			var listFieldsToRemove = GetListFieldsToRemove(cache, customFieldsToRemove);
			listFieldsToRemove.ForEach(x => cache.Fields.Remove(x));
		}

		private static List<string> GetListFieldsToRemove(PXCache cache, IEnumerable<string> propertyNames)
		{
			return propertyNames
				.Aggregate(
					cache.Fields.Where(x => x.Contains("_")).ToList(),
					(current, propertyName) => current.Except(propertyName + "_Description").ToList());
		}

		private static IEnumerable<string> GetCustomFieldsToRemove(PXCache cache)
		{
			return cache.GetItemType().GetProperties()
				.Where(x => x.GetCustomAttributes(typeof(FieldDescriptionForDynamicColumnsAttribute)).Any())
				.Select(x => x.Name);
		}

		private static void CreateCheckBoxControl(PXFieldSelectingEventArgs args,
			CSAttributeGroup attributeGroup)
		{
			var required = attributeGroup.Required == true
				? 1
				: -1;
			args.ReturnState = PXFieldState.CreateInstance(args.ReturnState, typeof(bool), false, null, required, null,
				null, false, nameof(CSAnswers.value), null, null, null, PXErrorLevel.Undefined, null,
				null, null);
		}

		private static void CreateStringControl(PXFieldSelectingEventArgs args, CSAttributeGroup attributeGroup)
		{
			var required = attributeGroup.Required == true
				? 1
				: -1;
			args.ReturnState = PXStringState.CreateInstance(args.ReturnState, null, null,
				nameof(CSAnswers.value), false, required, null, null, null, false, null);
		}

		private IEnumerable<CSAnswers> GetExistingAnswers(Guid? noteId)
		{
			return PXSelect<CSAnswers,
					Where<CSAnswers.refNoteID,
						Equal<Required<CSAnswers.refNoteID>>>>
				.Select(graph, noteId).FirstTableItems;
		}

		private static CSAnswers CreateAnswer(CSAttributeGroup group, string value, Guid? noteId)
		{
			return new CSAnswers
			{
				AttributeID = group.AttributeID,
				RefNoteID = noteId,
				Value = GetAnswerValue(group, value)
			};
		}

		private static string GetAnswerValue(CSAttributeGroup group, string value)
		{
			var checkboxValue = group.ControlType == CSAttribute.CheckBox
				? Convert.ToInt32(false).ToString()
				: group.DefaultValue;
			return value ?? checkboxValue;
		}

		private static IEnumerable<CSAnswers> GetInsertedAnswers(PXCache cache, Guid? noteId)
		{
			return cache.Inserted.Cast<CSAnswers>()
				.Where(x => x.RefNoteID == noteId);
		}
	}
}