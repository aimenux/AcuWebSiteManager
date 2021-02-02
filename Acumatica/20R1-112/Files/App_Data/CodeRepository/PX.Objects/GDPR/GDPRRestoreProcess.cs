using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using PX.Common;
using System.Text;
using static PX.Data.BqlCommand;
using System.Text.RegularExpressions;
using PX.Data.Search;
using PX.Data.Process;
using PX.Data.MassProcess;

namespace PX.Objects.GDPR
{
	public class GDPREraseOnRestoreProcess : GDPRRestoreProcess
	{
		protected override void ChildLevelProcessor(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs, Guid? topParentNoteID)
		{
			ErasePseudonymizedData(processingGraph, childTable, childs);
		}

		private void ErasePseudonymizedData(PXGraph processingGraph, Type childTable, IEnumerable<object> childs)
		{
			foreach (object child in childs)
			{
				List<PXDataFieldParam> assignsDB = new List<PXDataFieldParam>();

				assignsDB.Add(new PXDataFieldAssign(nameof(IPseudonymizable.PseudonymizationStatus), SetPseudonymizationStatus));

				List<PXDataFieldParam> restricts = new List<PXDataFieldParam>();

				foreach (string key in processingGraph.Caches[childTable].Keys)
				{
					restricts.Add(new PXDataFieldRestrict(key, processingGraph.Caches[childTable].GetValue(child, key)));
				}

				PXDatabase.Update(
					childTable,
					assignsDB
						.Union(restricts)
						.ToArray()
				);

				var entityNoteID = processingGraph.Caches[childTable].GetValue(child, nameof(INotable.NoteID)) as Guid?;

				PXDatabase.Delete<SMPersonalData>(
					new PXDataFieldRestrict<SMPersonalData.table>(childTable.FullName),
					new PXDataFieldRestrict<SMPersonalData.entityID>(entityNoteID));
			}
		}

	}

	public class GDPRRestoreProcess : GDPRPersonalDataProcessBase
	{
		#region Copy-pastas as-is !!!!!!!!!!!!!!!!!!!!!!!! Sorry :-(
		
		// Copy-paste of PX.Data.Search.PXEntitySearch

		private readonly Lazy<BqlFullTextRenderingMethod> FullTextCapability = new Lazy<BqlFullTextRenderingMethod>(PXDatabase.Provider.GetFullTextSearchCapability<SearchIndex.content>);

		internal static bool IsExactMatch(string query)
		{
			return query.StartsWith("\"") && query.EndsWith("\"") ;
		}

		internal static string ConvertToContainsPatern(string query)
		{
			string result = null;

			string pattern = string.Empty;

			string[] words = Regex.Split(query, @"\W+");//Split on all non-word characters

			if (words.Length > 0)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < words.Length; i++)
				{
					if (words[i].Length > 0)
						list.Add(words[i]);
				}

				if (list.Count > 0)
				{
					for (int i = 0; i < list.Count - 1; i++)
					{
						pattern += string.Format("\"{0}*\" AND ", list[i]);
					}
					pattern += string.Format("\"{0}*\"", list[list.Count - 1]);
				}

			}
			else
			{
				pattern = query;
			}

			if (!string.IsNullOrWhiteSpace(pattern))
				result = pattern;

			return result;
		}

		internal static string ConvertToContainsPaternMySql(string query)
		{
			var words = Regex.Split(query, @"\W+").Where(w => w != string.Empty);
			return words.Count() > 0 ?
				words.Select(w => string.Format("+{0}*", w)).Aggregate((f, w) => f + " " + w) :
				query;
		}

		#endregion
		
		#region DACs
		
		[Serializable]
		[PXHidden]
		public class RestoreType : IBqlTable
		{
			#region DeleteNonRestored
			public abstract class deleteNonRestored : PX.Data.BQL.BqlBool.Field<deleteNonRestored> { }

			[PXBool]
			[PXUIField(DisplayName = "Delete data that cannot be restored")]
			[PXDefault(false)]
			public virtual bool? DeleteNonRestored { get; set; }
			#endregion
		}

		#endregion

		#region Selects

		public PXFilter<RestoreType> Filter;

		public PXFilteredProcessing<SMPersonalDataIndex, RestoreType>
			ObfuscatedItems;

		public virtual IEnumerable obfuscatedItems()
		{
			string query = null;

			foreach (PXFilterRow filter in PXView.Filters)
				query += filter.Value + " ";

			query = query?.Trim();

			if (!String.IsNullOrWhiteSpace(query))
			{
				var searchGraph = new PXEntitySearch();

				if (searchGraph.IsFullText())
				{
					if (!IsExactMatch(query))
					{
						if (FullTextCapability.Value == BqlFullTextRenderingMethod.MySqlMatchAgainst)
						{
							query = ConvertToContainsPaternMySql(query);
						}
						else
						{
							query = ConvertToContainsPatern(query);
						}
					}

					var selectIndexed = new PXSelectReadonly<SMPersonalDataIndex, Where<Contains<SMPersonalDataIndex.content, Required<SMPersonalDataIndex.content>, SMPersonalDataIndex.indexID>>, OrderBy<Desc<RankOf<SMPersonalDataIndex.content>>>>(this);

					return selectIndexed.Select(query);
				}
				else
				{
					var selectNonIndexed = new PXSelectReadonly<SearchIndex, Where<SearchIndex.content, Like<Required<SearchIndex.content>>>>(this);

					return selectNonIndexed.Select(string.Format("%{0}%", query));
				}
			}

			var select = new PXSelectReadonly<SMPersonalDataIndex>(this);

			return select.Select();
		}

		#endregion

		#region ctor

		public GDPRRestoreProcess()
		{
			this.Actions["Schedule"].SetVisible(false);

			ObfuscatedItems.SetSelected<SMPersonalDataIndex.selected>();

			ObfuscatedItems.SetProcessVisible(false);
			ObfuscatedItems.SetProcessAllVisible(false);
		}

		#endregion

		#region Actions

		public PXCancel<RestoreType> Cancel;

		public PXAction<RestoreType> OpenContact;

		[PXButton]
		[PXUIField(DisplayName = Messages.OpenContact, Visible = false)]
		public virtual IEnumerable openContact(PXAdapter adapter)
		{
			var entity = this.Caches[typeof(SMPersonalDataIndex)].Current as SMPersonalDataIndex;

			if (entity == null)
				return adapter.Get();
			
			var primary = RemapToPrimary(new List<SMPersonalDataIndex> { entity });

			foreach (IBqlTable table in primary)
			{
				if (table is Contact)
				{
					PXPrimaryGraphCollection primaryGraph = new PXPrimaryGraphCollection(this);

					PXGraph graph = primaryGraph[table];

					PXRedirectHelper.TryRedirect(graph, table, PXRedirectHelper.WindowMode.New);
				}
				else if (table is CRContact)
				{
					var opp = new PXSelect<
								CROpportunity,
							Where<CROpportunity.opportunityContactID, Equal<Required<CRContact.contactID>>>>(this)
						.SelectSingle((table as CRContact).ContactID);

					var graph = PXGraph.CreateInstance<OpportunityMaint>();
					PXRedirectHelper.TryRedirect(graph, opp, PXRedirectHelper.WindowMode.New);
				}
			}

			return adapter.Get();
		}

		public PXAction<RestoreType> Restore;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.Restore)]
		public virtual IEnumerable restore(PXAdapter adapter)
		{
			var filter = Filter.Current;
			ObfuscatedItems.SetProcessDelegate(delegate (List<SMPersonalDataIndex> entries)
			{
				var graph = PXGraph.CreateInstance<GDPRRestoreProcess>();
				graph.GetPseudonymizationStatus = typeof(PXPseudonymizationStatusListAttribute.pseudonymized);
				graph.SetPseudonymizationStatus = PXPseudonymizationStatusListAttribute.NotPseudonymized;

				RestoreImpl(entries, graph, filter);
			});

			return Actions["Process"].Press(adapter);
		}

		public PXAction<RestoreType> Erase;
		[PXProcessButton]
		[PXUIField(DisplayName = Messages.Erase)]
		public virtual IEnumerable erase(PXAdapter adapter)
		{
			var filter = Filter.Current;
			ObfuscatedItems.SetProcessDelegate(delegate (List<SMPersonalDataIndex> entries)
			{
				var graph = PXGraph.CreateInstance<GDPREraseOnRestoreProcess>();
				graph.GetPseudonymizationStatus = typeof(PXPseudonymizationStatusListAttribute.pseudonymized);
				graph.SetPseudonymizationStatus = PXPseudonymizationStatusListAttribute.Erased;

				RestoreImpl(entries, graph, filter);
			});

			return Actions["Process"].Press(adapter);
		}

		#endregion

		#region Implementation

		private static void RestoreImpl(IEnumerable<SMPersonalDataIndex> entities, GDPRRestoreProcess graph, RestoreType filter)
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				IEnumerable<IBqlTable> processingItems = RemapToPrimary(entities);

				graph.ProcessImpl(processingItems, true, null);

				if (filter?.DeleteNonRestored == true)
					graph.CleanNonRestored(processingItems);

				ts.Complete();
			}
		}

		private static void EraseImpl(IEnumerable<SMPersonalDataIndex> entities, GDPREraseOnRestoreProcess graph)
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				graph.ProcessImpl(RemapToPrimary(entities), true, null);

				ts.Complete();
			}
		}

		protected static IEnumerable<IBqlTable> RemapToPrimary(IEnumerable<SMPersonalDataIndex> entities)
		{
			using (new PXReadDeletedScope())
			{
				var graph = new PXGraph();

				var helper = new EntityHelper(graph);

				foreach (string combinedKey in entities.Select(_ => _.CombinedKey))
				{
					helper.GetEntityRow(typeof(Contact), combinedKey.Split(PXAuditHelper.SEPARATOR));

					var contact = helper.GetEntityRow(typeof(Contact), combinedKey.Split(PXAuditHelper.SEPARATOR)) as IBqlTable;

					if (contact != null)
						yield return contact;
					else
						yield return helper.GetEntityRow(typeof(CRContact), combinedKey.Split(PXAuditHelper.SEPARATOR)) as IBqlTable;
				}
			}
		}

		protected override void TopLevelProcessor(string combinedKey, Guid? topParentNoteID, string info)
		{
			HandleSearchIndex(combinedKey);
		}

		protected override void ChildLevelProcessor(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs, Guid? topParentNoteID)
		{
			RestoreObfuscatedEntries(processingGraph, childTable, fields, childs);
		}
		
		private void HandleSearchIndex(string combinedKey)
		{
			PXDatabase.Delete<SMPersonalDataIndex>(
				new PXDataFieldRestrict<SMPersonalDataIndex.combinedKey>(combinedKey));
		}

		private void RestoreObfuscatedEntries(PXGraph processingGraph, Type childTable, IEnumerable<PXPersonalDataFieldAttribute> fields, IEnumerable<object> childs)
		{
			foreach (object child in childs)
			{
				var entityNoteID = processingGraph.Caches[childTable].GetValue(child, nameof(INotable.NoteID)) as Guid?;

				List<PXDataFieldParam> assignsDB = new List<PXDataFieldParam>();

				foreach (PXDataRecord record in PXDatabase.SelectMulti<SMPersonalData>(
					new PXDataField<SMPersonalData.field>(),
					new PXDataField<SMPersonalData.value>(),
					new PXDataFieldValue<SMPersonalData.table>(childTable.FullName),
					new PXDataFieldValue<SMPersonalData.entityID>(entityNoteID)))
				{
					assignsDB.Add(new PXDataFieldAssign(record.GetString(0), record.GetString(1)));
				}

				List<PXDataFieldParam> assignsEntity = new List<PXDataFieldParam>();

				foreach (var field in fields)
				{
					var defaultAttr = processingGraph.Caches[childTable]
						.GetAttributesOfType<PXDefaultAttribute>(null, field.FieldName)
						?.FirstOrDefault();

					var defaultValue = field.DefaultValue ?? defaultAttr?.Constant;

					assignsEntity.Add(new PXDataFieldAssign(field.FieldName, defaultValue));
				}

				assignsDB.Add(new PXDataFieldAssign(nameof(IPseudonymizable.PseudonymizationStatus), SetPseudonymizationStatus));
				
				List<PXDataFieldParam> restricts = new List<PXDataFieldParam>();
				
				foreach (string key in processingGraph.Caches[childTable].Keys)
				{
					restricts.Add(new PXDataFieldRestrict(key, processingGraph.Caches[childTable].GetValue(child, key)));
				}

				var isSuccess = PXDatabase.Update(
					childTable,
					InterruptRestorationHandler(processingGraph.Caches[childTable], restricts)
						.Union(assignsDB)
						.Union(assignsEntity)
							.Distinct(_ => _.Column.Name.ToLower())
						.Union(restricts)
						.ToArray()
				);

				if (isSuccess)
				{
					PXDatabase.Delete<SMPersonalData>(
						new PXDataFieldRestrict<SMPersonalData.table>(childTable.FullName),
						new PXDataFieldRestrict<SMPersonalData.entityID>(entityNoteID));
				}
			}
		}

		private List<PXDataFieldParam> InterruptRestorationHandler(PXCache cache, List<PXDataFieldParam> restricts)
		{
			var returnAssigns = new List<PXDataFieldParam>();

			var extensions = cache
				.GetExtensionTypes()
				.Where(_ => typeof(IPostRestorable).IsAssignableFrom(_));

			if (extensions == null || extensions.Count() == 0)
				return returnAssigns;

			foreach (var extension in extensions)
			{
				var handler = extension.GetMethod(nameof(IPostRestorable.InterruptRestorationHandler));

				if (handler != null)
				{
					returnAssigns.AddRange(
						handler?.Invoke(Activator.CreateInstance(extension), new object[] { restricts }) as List<PXDataFieldParam>
						?? new List<PXDataFieldParam>()
					);
				}
			}

			return returnAssigns;
		}

		private void CleanNonRestored(IEnumerable<IBqlTable> entities)
		{
			var processingGraph = PXGraph.CreateInstance<GDPRPersonalDataProcessBase>();

			foreach (IBqlTable entity in entities)
			{
				processingGraph.EnsureCachePersistence(entity.GetType());

				var entityCache = processingGraph.Caches[entity.GetType()];

				PXDatabase.Delete<SMPersonalData>(
					new PXDataFieldRestrict<SMPersonalData.topParentNoteID>(entityCache.GetValue(entity, nameof(INotable.NoteID))));
			}
		}

		#endregion
	}
}
