using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.Common.DataIntegrity;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote
{
	public class ComplianceDocumentRefNoteAttribute : PXDBGuidAttribute, IPXRowPersistedSubscriber, IPXRowDeletedSubscriber
	{
		private const string ClDisplayNameField = "ClDisplayName";

		private readonly Type itemType;
		private readonly ComplianceDocumentEntityHelper complianceDocumentEntityHelper;
		private readonly RefNoteRedirectHelper redirectHelper;

		private EntityHelper entityHelper;
		private string descriptionFieldName;

		public ComplianceDocumentRefNoteAttribute(Type itemType)
		{
			this.itemType = itemType;
			complianceDocumentEntityHelper = new ComplianceDocumentEntityHelper(itemType);
			redirectHelper = new RefNoteRedirectHelper();
		}

		public override void CacheAttached(PXCache cache)
		{
			entityHelper = new EntityHelper(cache.Graph);
			descriptionFieldName = _FieldName + "_Description";
			base.CacheAttached(cache);
			cache.Fields.Add(descriptionFieldName);
			cache.Graph.FieldSelecting.AddHandler(cache.GetItemType(), descriptionFieldName,
				DescriptionFieldSelecting);
			CreateRedirectAction(cache);
		}

		public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
		{
			var graph = cache.Graph;
			var viewName = "_GuidSelector_" + itemType.GetLongName();
			if (!graph.Views.ContainsKey(viewName))
			{
				var selectorView = complianceDocumentEntityHelper.CreateView(graph);
				graph.Views.Add(viewName, selectorView);
			}
			var fieldList = entityHelper.GetFieldList(itemType);
			var headerList = fieldList.Select(GetFieldDisplayName).ToArray();
			var fieldState = GetFieldState(args, viewName, fieldList, headerList);
			fieldState.ValueField = ClDisplayNameField;
			fieldState.DescriptionName = ClDisplayNameField;
			fieldState.SelectorMode = PXSelectorMode.TextModeSearch;
			args.ReturnState = fieldState;
			args.ReturnValue = GetDescription(cache, args.Row);
		}

		public static void SetComplianceDocumentReference<TField>(PXCache cache, ComplianceDocument doc, string docType, string refNumber, Guid? noteId)
			where TField : IBqlField
		{
			cache.GetAttributesReadonly<TField>(doc)
				.OfType<ComplianceDocumentRefNoteAttribute>().First()
				.SetComplianceDocumentReference(cache, doc, docType, refNumber, noteId);
		}

		public virtual void SetComplianceDocumentReference(PXCache cache, ComplianceDocument doc, string docType, string refNumber, Guid? noteId)
		{
			var oldReferenceId = (Guid?)cache.GetValue(doc, _FieldOrdinal);

			var reference = InsertComplianceDocumentReference(cache.Graph, docType, refNumber, noteId);

			cache.SetValue(doc, _FieldName, reference.ComplianceDocumentReferenceId);

			TryDeleteReference(cache, oldReferenceId);
		}

		public override void FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs args)
		{
			if (args.NewValue != null)
			{
				var noteId = complianceDocumentEntityHelper.GetNoteId(cache.Graph, (string)args.NewValue);
				if (noteId != null)
				{
					var oldReferenceId = (Guid?)cache.GetValue(args.Row, _FieldOrdinal);
					var reference = InsertComplianceDocumentReference(cache, args, noteId);
					args.NewValue = reference.ComplianceDocumentReferenceId;
					TryDeleteReference(cache, oldReferenceId);
				}
				else
				{
					ComplianceDocument doc = args.Row as ComplianceDocument;

					args.NewValue = cache.GetValue(doc, _FieldName);
					args.Cancel = true;
				}
			}
			else
			{
				DeleteExistingReference(cache, (ComplianceDocument)args.Row);
			}
		}

		private PXButtonDelegate GetRedirectionDelegate(PXCache cache)
		{
			return adapter =>
			{
				var pxCache = adapter.View.Graph.Caches[cache.BqlTable];
				if (pxCache.Current != null)
				{
					var referenceId = (Guid?)pxCache.GetValue(pxCache.Current, _FieldName);
					if (referenceId.HasValue)
					{
						redirectHelper.Redirect(itemType, referenceId.Value);
					}
				}
				return adapter.Get();
			};
		}

		private PXFieldState GetFieldState(PXFieldSelectingEventArgs args, string viewName, string[] fieldList,
			string[] headerList)
		{
			return PXFieldState.CreateInstance(args.ReturnState, null, null, null,
				null, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null,
				PXUIVisibility.Undefined, viewName, fieldList, headerList);
		}

		private void DescriptionFieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
		{
			var info = GetDescription(cache, args.Row);
			args.ReturnState = PXFieldState.CreateInstance(info, typeof(string), null, null, null, null, null,
				null, descriptionFieldName, null, descriptionFieldName, null, PXErrorLevel.Undefined, false,
				!string.IsNullOrEmpty(info), null, PXUIVisibility.Invisible);
		}

		private string GetDescription(PXCache cache, object row)
		{
			var referenceId = (Guid?)cache.GetValue(row, _FieldOrdinal);
			if (referenceId.HasValue && referenceId.Value != Guid.Empty)
			{
				var reference = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(
					cache.Graph, referenceId);
				if (reference != null)
				{
					return GetFieldValue(reference);
				}
			}
			return null;
		}

		private string GetFieldValue(ComplianceDocumentReference reference)
		{
			return string.Join(", ", ComplianceReferenceTypeHelper.GetValueByKey(itemType, reference.Type),
				reference.ReferenceNumber);
		}

		private void CreateRedirectAction(PXCache cache)
		{
			var buttonDelegate = GetRedirectionDelegate(cache);
			var actionName = cache.GetItemType().Name + "$" + _FieldName + "$Link";
			cache.Graph.Actions[actionName] = (PXAction)Activator
				.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(GetDacOfPrimaryView(cache)), cache.Graph,
					actionName, buttonDelegate, GetEventSubscriberAttributes());
			cache.Graph.Actions[actionName].SetVisible(false);
		}

		private static Type GetDacOfPrimaryView(PXCache cache)
		{
			return cache.Graph.Views.ContainsKey(cache.Graph.PrimaryView)
				? cache.Graph.Views[cache.Graph.PrimaryView].GetItemType()
				: cache.BqlTable;
		}

		private static PXEventSubscriberAttribute[] GetEventSubscriberAttributes()
		{
			return new PXEventSubscriberAttribute[]
			{
				new PXUIFieldAttribute
				{
					MapEnableRights = PXCacheRights.Select
				}
			};
		}

		private string GetFieldDisplayName(string column)
		{
			var attribute = (PXUIFieldAttribute)itemType.GetProperty(column)
				?.GetCustomAttributes(typeof(PXUIFieldAttribute), false).FirstOrDefault();
			return attribute != null
				? attribute.DisplayName
				: column;
		}

		private ComplianceDocumentReference InsertComplianceDocumentReference(PXCache cache,
			PXFieldUpdatingEventArgs args, Guid? noteId)
		{
			var keys = ((string)args.NewValue).Split(',');
			var type = keys[0].Trim();
			var refNumber = keys[1].Trim();
			var reference = InsertComplianceDocumentReference(cache.Graph, ComplianceReferenceTypeHelper.GetKeyByValue(itemType, type), refNumber, noteId);

			return reference;
		}

		private void TryDeleteReference(PXCache cache, Guid? referenceId)
		{
			if (referenceId.HasValue && referenceId.Value != Guid.Empty)
			{
				var reference = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(cache.Graph, referenceId);

				if (reference != null)
				{
					cache.Graph.Caches[typeof(ComplianceDocumentReference)].Delete(reference);
				}
			}
		}

		private void DeleteExistingReference(PXCache cache, ComplianceDocument doc)
		{
			var referenceId = (Guid?)cache.GetValue(doc, _FieldOrdinal);

			TryDeleteReference(cache, referenceId);
		}

		private ComplianceDocumentReference InsertComplianceDocumentReference(PXGraph graph, string docType, string refNumber, Guid? noteId)
		{
			var reference = new ComplianceDocumentReference
			{
				ComplianceDocumentReferenceId = Guid.NewGuid(),
				Type = docType,
				ReferenceNumber = refNumber,
				RefNoteId = noteId,
			};

			return (ComplianceDocumentReference)graph.Caches[typeof(ComplianceDocumentReference)].Insert(reference);
		}

		public void RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			var refCache = cache.Graph.Caches[typeof(ComplianceDocumentReference)];

			if (e.TranStatus == PXTranStatus.Open)
			{
				ComplianceDocument doc = e.Row as ComplianceDocument;

				if (doc == null)
					return;

				var referenceId = (Guid?)cache.GetValue(e.Row, _FieldOrdinal);

				if (referenceId.HasValue && referenceId.Value != Guid.Empty)
				{
					var reference =
						ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(cache.Graph, referenceId);

					if (reference != null)
					{
						reference = (ComplianceDocumentReference)refCache.Locate(reference);

						if (reference != null)
						{
							if (refCache.GetStatus(reference) == PXEntryStatus.Inserted)
							{
								refCache.PersistInserted(reference);
							}
							else if (refCache.GetStatus(reference) == PXEntryStatus.Updated)
							{
								refCache.PersistUpdated(reference);
							}
						}
					}
					else
					{
						reference = (ComplianceDocumentReference)refCache.Locate(new ComplianceDocumentReference()
						{ ComplianceDocumentReferenceId = referenceId });

						if (reference != null && refCache.GetStatus(reference) == PXEntryStatus.Deleted)
						{
							refCache.PersistDeleted(reference);
						}
					}
				}
			}
			else if (e.TranStatus == PXTranStatus.Aborted || e.TranStatus == PXTranStatus.Completed)
			{
				refCache.Persisted(e.TranStatus == PXTranStatus.Aborted);
			}
		}

		public void RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			DeleteExistingReference(cache, (ComplianceDocument)e.Row);
		}
	}
}
