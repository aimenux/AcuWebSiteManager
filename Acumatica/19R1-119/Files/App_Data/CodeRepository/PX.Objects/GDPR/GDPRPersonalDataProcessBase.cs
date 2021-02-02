using System.Collections;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using PX.Common;
using PX.Objects.AR;
using PX.Data.Process;

namespace PX.Objects.GDPR
{
	public class GDPRPersonalDataProcessBase : PXGraph<GDPRPersonalDataProcessBase>
	{
		#region ctor

		protected virtual void TopLevelProcessor(string combinedKey, Guid? topParentNoteID, string info) { }
		protected virtual void ChildLevelProcessor(PXGraph graph, Type table, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs, Guid? topParentNoteID) { }

		public Type GetPseudonymizationStatus;
		public int SetPseudonymizationStatus;

		public GDPRPersonalDataProcessBase()
		{
			GetPseudonymizationStatus = typeof(PXPseudonymizationStatusListAttribute.notPseudonymized);
			SetPseudonymizationStatus = PXPseudonymizationStatusListAttribute.NotPseudonymized;
		}

		#endregion

		#region Implementation

		public void ProcessImpl(IEnumerable<IBqlTable> entities, bool topLayer, Guid? topParentNoteID)
		{
			var processingGraph = PXGraph.CreateInstance<GDPRPersonalDataProcessBase>();
			
			foreach (IBqlTable entity in entities)
			{
				var mapping = PXPersonalDataTableAttribute.GetEntitiesMapping(entity.GetType());

				AddCachesIntoGraph(processingGraph, mapping.Keys);

				var entityCache = processingGraph.Caches[entity.GetType()];

				entityCache.Current = entity;

				if (topLayer)
				{
					topParentNoteID = entityCache.GetValue(entity, nameof(INotable.NoteID)) as Guid?;

					var info = GenerateSearchInfo(processingGraph, entity);

					// as no wway to search -> restore
					if (info == null)
						continue;

					var combinedKey = String.Join<string>(PXAuditHelper.SEPARATOR.ToString(), entityCache.Keys.Select(_ => entityCache.GetValue(entity, _).ToString()));

					TopLevelProcessor(combinedKey, topParentNoteID, info);
					
					LogOperation(entity.GetType(), combinedKey);
				}

				foreach (KeyValuePair<Type, List<BqlCommand>> tableLink in mapping)
				{
					var childTable = tableLink.Key;
					var selects = tableLink.Value;

					foreach (BqlCommand select in selects)
					{
						var currentSelect = select;

						var bqlPseudonymizationStatus = processingGraph.Caches[childTable].GetBqlField(nameof(IPseudonymizable.PseudonymizationStatus));
						
						if (bqlPseudonymizationStatus != null)
						{
							if (GetPseudonymizationStatus == typeof(PXPseudonymizationStatusListAttribute.notPseudonymized))
							{
								currentSelect = currentSelect.WhereAnd(BqlCommand.Compose(typeof(Where<,,>), bqlPseudonymizationStatus, typeof(Equal<>), GetPseudonymizationStatus,
									typeof(Or<,>), bqlPseudonymizationStatus, typeof(IsNull))); // for just added columns in cst
							}
							else
							{
								currentSelect = currentSelect.WhereAnd(BqlCommand.Compose(typeof(Where<,>), bqlPseudonymizationStatus, typeof(Equal<>), GetPseudonymizationStatus));
							}
						}

						var view = new PXView(processingGraph, false, currentSelect);

						var result = view.SelectMulti();
						if (result.Count == 0)
							continue;

						var childs = result.Select(_ => _ is PXResult ? (_ as PXResult)[0] : _);

						List<PXPersonalDataFieldAttribute> fields = null;
						var isKeyValueTable = PXPersonalDataFieldAttribute.GetPersonalDataFields(childTable, out fields);
						fields = fields.Where(_ => !(_ is PXPersonalDataTableAnchorAttribute)).ToList();



						if (fields.Count > 0)
						{
							if (!processingGraph.Caches[childTable].Fields.Contains(nameof(INotable.NoteID))
								|| !processingGraph.Caches[childTable].Fields.Contains(nameof(IPseudonymizable.PseudonymizationStatus)))
								continue;

							if (!isKeyValueTable)
							{
								ChildLevelProcessor(processingGraph, childTable, fields, childs, topParentNoteID);
							}
							else
							{
								ChildLevelProcessor(processingGraph, childTable, fields.Where(_ => _ is PXPersonalDataFieldAttribute.Value), childs, topParentNoteID);
							}
						}



						ProcessImpl(childs.Cast<IBqlTable>(), false, topParentNoteID);
					}
				}
			}
		}

		protected static void AddCachesIntoGraph(PXGraph graph, IEnumerable<Type> caches)
		{
			foreach (Type cache in caches)
			{
				graph.EnsureCachePersistence(cache);
				graph.Caches[cache].AllowInsert = graph.Caches[cache].AllowUpdate = graph.Caches[cache].AllowDelete = false;
			}
		}

		protected static string GenerateSearchInfo(PXGraph processingGraph, IBqlTable entity)
		{
			PXSearchableAttribute attr = processingGraph
				.Caches[entity.GetType()]
				.GetAttributes(nameof(INotable.NoteID))
				.OfType<PXSearchableAttribute>()
				.FirstOrDefault();

			if (attr == null && entity.GetType() == typeof(CRContact))
			{
				attr = new PXSearchableAttribute(SM.SearchCategory.CR, Messages.ContactOPTypeForIndex, new Type[] { typeof(CRContact.displayName) },
					new Type[] { typeof(CRContact.email), typeof(CRContact.phone1), typeof(CRContact.phone2), typeof(CRContact.phone3), typeof(CRContact.webSite) })
				{
					Line1Format = "{0}{1}{2}",
					Line1Fields = new Type[] { typeof(CRContact.salutation), typeof(CRContact.phone1), typeof(CRContact.email) }
				};
			}

			return attr?.BuildContent(processingGraph.Caches[entity.GetType()], entity, null);
		}

		protected void LogOperation(Type tableName, string combinedKey)
		{
			PXDatabase.Insert<SMPersonalDataLog>(
				new PXDataFieldAssign<SMPersonalDataLog.tableName>(tableName.FullName),
				new PXDataFieldAssign<SMPersonalDataLog.combinedKey>(combinedKey),
				new PXDataFieldAssign<SMPersonalDataLog.pseudonymizationStatus>(SetPseudonymizationStatus),
				new PXDataFieldAssign<SMPersonalDataLog.createdByID>(PXAccess.GetTrueUserID()),
				new PXDataFieldAssign<SMPersonalDataLog.createdDateTime>(PXTimeZoneInfo.UtcNow)
			);
		}

		#endregion

		#region Decryptors

		private class PXDBBypassStringAttribute : PXDBStringAttribute
		{
			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				if (!sender.BypassAuditFields.Contains(this.FieldName))
					sender.BypassAuditFields.Add(this.FieldName);
			}
		}

		[PXHidden]
		public PXSelect<CustomerPaymentMethodDetail> customerPaymentMethodDetail;

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Value")]
		[PXDBBypassString]
		[PXPersonalDataFieldAttribute.Value]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CustomerPaymentMethodDetail.value> e) { }

		#endregion
	}
}
