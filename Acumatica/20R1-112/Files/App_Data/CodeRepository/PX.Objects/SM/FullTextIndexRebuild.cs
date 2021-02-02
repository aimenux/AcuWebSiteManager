using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using PX.CS;
using PX.Data;
using PX.Data.Search;
using PX.Data.Wiki.Parser;
using PX.Data.Wiki.Parser.Html;
using PX.Data.Wiki.Parser.PlainTxt;
using PX.Objects.BQLConstants;
using PX.Objects.GL.Overrides.ScheduleProcess;
using PX.SM;
using PX.Web.UI;
using Messages = PX.Objects.CA.Messages;


namespace PX.Objects.SM
{
    public class FullTextIndexRebuild : PXGraph<FullTextIndexRebuild>
    {
        public PXCancel<FullTextIndexRebuildProc.RecordType> Cancel;
        public PXProcessing<FullTextIndexRebuildProc.RecordType> Items;

        public PXSelectJoin<WikiPage,
            InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
            InnerJoin<WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
            Where<WikiRevision.plainText, Equal<EmptyString>, 
            And<WikiRevision.pageRevisionID, Equal<WikiPageLanguage.lastRevisionID>>>> WikiArticles;
		
		/* All revisions:
        public PXSelectJoin<WikiPage,
            InnerJoin<WikiPageLanguage, On<WikiPageLanguage.pageID, Equal<WikiPage.pageID>>,
            InnerJoin<WikiRevision, On<WikiRevision.pageID, Equal<WikiPage.pageID>>>>,
            Where<WikiRevision.plainText, Equal<EmptyString>>> WikiArticles;*/

        public virtual IEnumerable items()
        {
            bool found = false;
            foreach (FullTextIndexRebuildProc.RecordType item in Items.Cache.Inserted)
            {
                found = true;
                yield return item;
            }
            if (found)
                yield break;

            foreach (Type entity in PXSearchableAttribute.GetAllSearchableEntities(this))
            {
                yield return Items.Insert(new FullTextIndexRebuildProc.RecordType() { Entity = entity.FullName, Name = entity.Name, DisplayName = Caches[entity].DisplayName });
            }

            Items.Cache.IsDirty = false;
        }

        public FullTextIndexRebuild()
        {
            Items.SetProcessDelegate<FullTextIndexRebuildProc>(FullTextIndexRebuildProc.BuildIndex);
        }

        #region Actions/Buttons


        public PXAction<FullTextIndexRebuildProc.RecordType> clearAllIndexes;
        [PXUIField(DisplayName = Messages.ClearAllIndexes, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(Tooltip = Messages.ClearAllIndexesTip)]
        public virtual IEnumerable ClearAllIndexes(PXAdapter adapter)
        {
            PXLongOperation.StartOperation(this, delegate()
            {
                PXDatabase.Delete(typeof(SearchIndex));
            });

            return adapter.Get();
        }

        public PXAction<FullTextIndexRebuildProc.RecordType> indexCustomArticles;
        [PXUIField(DisplayName = Messages.IndexCustomArticles, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(Tooltip = Messages.IndexCustomArticles)]
        public virtual IEnumerable IndexCustomArticles(PXAdapter adapter)
        {
			PXLongOperation.StartOperation(this, delegate()
            {
				foreach (var result in WikiArticles.Select())
				{
					string plaintext = null;
                        
					var _wp = (WikiPage)result[typeof(WikiPage)];
					var _wr = (WikiRevision)result[typeof(WikiRevision)];
					var _wl = (WikiPageLanguage) result[typeof (WikiPageLanguage)];

					if (_wp.IsHtml != true)
					{
						WikiReader reader = PXGraph.CreateInstance<WikiReader>();
						PXWikiSettings settings = new PXWikiSettings(new PXPage(), reader);
						PXTxtRenderer renderer = new PXTxtRenderer(settings.Absolute);
						var ctx = new PXDBContext(settings.Absolute);
						ctx.Renderer = renderer;
						plaintext = (_wl.Title ?? "") + Environment.NewLine + PXWikiParser.Parse(_wr.Content, ctx);
					}
					else
					{
						plaintext = (_wl.Title ?? "") + Environment.NewLine + SearchService.Html2PlainText(_wr.Content);
					}


					//Try updating the article in current Company
					if (!PXDatabase.Update<WikiRevision>(
						new PXDataFieldAssign("PlainText", PXDbType.NVarChar, plaintext),
						new PXDataFieldRestrict("PageID", PXDbType.UniqueIdentifier, _wr.PageID),
						new PXDataFieldRestrict("PageRevisionID", PXDbType.Int, _wr.PageRevisionID),
						new PXDataFieldRestrict("Language", PXDbType.VarChar, _wr.Language)
						))
					{
						//Article may be shared. Try updating the article through graph (thus handling the shared record update stratagy)
						//if article is not updatable an exception may be thrown - ignore.
						try
						{
							ArticleUpdater updater = PXGraph.CreateInstance<ArticleUpdater>();
							WikiRevision rev = updater.Revision.Select(_wr.PageID, _wr.PageRevisionID, _wr.Language);
							rev.PlainText = plaintext;
							updater.Revision.Update(rev);
							updater.Persist();
						}
						catch (Exception ex)
						{
							PXTrace.WriteInformation("Plain text field could not be updated for article = {0}. Error Message: {1}", _wr.PageID, ex.Message);
						}
						
					}
				}
			});

			

            return adapter.Get();
        }

        #endregion
    }

	public class ArticleUpdater : PXGraph<ArticleUpdater>
	{
		public PXSelect<WikiRevision, Where<WikiRevision.pageID, Equal<Required<WikiRevision.pageID>>,
			And<WikiRevision.pageRevisionID, Equal<Required<WikiRevision.pageRevisionID>>,
			And<WikiRevision.language, Equal<Required<WikiRevision.language>>>>>> Revision;
		
	}
	

    public class FullTextIndexRebuildProc : PXGraph<FullTextIndexRebuildProc>
    {
        public static void BuildIndex(FullTextIndexRebuildProc graph, RecordType item)
        {
            Debug.Print("Start processing {0}", item.Name);
            Stopwatch sw = new Stopwatch();

            graph.Caches.Clear();
            graph.Clear(PXClearOption.ClearAll);

            PXProcessing<RecordType>.SetCurrentItem(item);
            Type entity = GraphHelper.GetType(item.Entity);

            PXSearchableAttribute searchableAttribute = null;
            var list = graph.Caches[entity].GetAttributes("NoteID");

            foreach (PXEventSubscriberAttribute att in list)
            {
                PXSearchableAttribute attribute = att as PXSearchableAttribute;
                if (attribute != null)
                {
                    searchableAttribute = attribute;
                    break;
                }
            }

            if (searchableAttribute == null)
                return;

            Type viewType;


            Type joinNote = typeof(LeftJoin<Note,On<Note.noteID,Equal<SearchIndex.noteID>>>);

            if (searchableAttribute.SelectForFastIndexing != null)
            {
				Type noteentity = entity;
				if (searchableAttribute.SelectForFastIndexing.IsGenericType)
				{
					Type[] tables = searchableAttribute.SelectForFastIndexing.GetGenericArguments();
					if (tables != null && tables.Length > 0 && typeof(IBqlTable).IsAssignableFrom(tables[0]))
					{
						noteentity = tables[0];
					}
				}
				Type joinSearchIndex = BqlCommand.Compose(
							typeof(LeftJoin<,>),
							typeof(SearchIndex),
							typeof(On<,>),
							typeof(SearchIndex.noteID),
							typeof(Equal<>),
							noteentity.GetNestedType("noteID"));

				viewType = BqlCommand.AppendJoin(searchableAttribute.SelectForFastIndexing, joinSearchIndex);
                viewType = BqlCommand.AppendJoin(viewType, joinNote);
            }
            else
            {
				Type joinSearchIndex = BqlCommand.Compose(
							typeof(LeftJoin<,>),
							typeof(SearchIndex),
							typeof(On<,>),
							typeof(SearchIndex.noteID),
							typeof(Equal<>),
							entity.GetNestedType("noteID"));

				viewType = BqlCommand.Compose(typeof(Select<>), entity);
                viewType = BqlCommand.AppendJoin(viewType, joinSearchIndex);
                viewType = BqlCommand.AppendJoin(viewType, joinNote);
            }

            BqlCommand cmd = BqlCommand.CreateInstance(viewType);

            PXView itemView = new PXView(graph, true, cmd);
            List<object> resultset;

            List<Type> fieldList = new List<Type>(searchableAttribute.GetSearchableFields(graph.Caches[entity]));
			Type entityForNoteId = entity;
			while( typeof(IBqlTable).IsAssignableFrom(entityForNoteId) )
			{
				Type tN = entityForNoteId.GetNestedType("noteID");
				if (null!=tN) fieldList.Add(tN);
				entityForNoteId = entityForNoteId.BaseType;
			}
            fieldList.Add(typeof(SearchIndex.noteID));
            fieldList.Add(typeof(SearchIndex.category));
            fieldList.Add(typeof(SearchIndex.content));
            fieldList.Add(typeof(SearchIndex.entityType));
            fieldList.Add(typeof(Note.noteID));
            fieldList.Add(typeof(Note.noteText));

            sw.Start();

			int batchSize = 50000;
			int startRow = 0;

			do {
				using (new PXFieldScope(itemView, fieldList)) {
					//resultset = itemView.SelectMulti();
					resultset = itemView.SelectWindowed(null, null, null, null, startRow, batchSize);
				}
				sw.Stop();
				Debug.Print("{0} GetResultset in {1} sec. Total records={2}", item.DisplayName, sw.Elapsed.TotalSeconds, resultset.Count);
				sw.Reset();
				sw.Start();

				startRow += batchSize;

				int totalcount = resultset.Count;
				int cx = 0;
				int dx = 0;

				try {
					Dictionary<Guid, SearchIndex> insertDict = new Dictionary<Guid, SearchIndex>(resultset.Count);
					foreach (var res in resultset) {
						cx++;

						bool isSearchable = searchableAttribute.IsSearchable(graph.Caches[entity], ((PXResult)res)[entity]);
						if (isSearchable) {
							dx++;

							Note note = (Note)((PXResult)res)[typeof(Note)];
							SearchIndex si = searchableAttribute.BuildSearchIndex(graph.Caches[entity], ((PXResult)res)[entity],
								(PXResult)res, ExtractNoteText(note));
							SearchIndex searchIndex = (SearchIndex)((PXResult)res)[typeof(SearchIndex)];

							if (searchIndex.NoteID != null && searchIndex.NoteID != si.NoteID) {
								PXSearchableAttribute.Delete(si);
							}

							if (searchIndex.NoteID == null) {
								if (!insertDict.ContainsKey(si.NoteID.Value))
									insertDict.Add(si.NoteID.Value, si);
							}
							else if (si.Content != searchIndex.Content || si.Category != searchIndex.Category
									 || si.EntityType != searchIndex.EntityType) {
								PXSearchableAttribute.Update(si);
							}
						}
					}
					sw.Stop();
					Debug.Print("{0} Content building in {1} sec. Records processed = {2}. Searchable={3}", item.DisplayName, sw.Elapsed.TotalSeconds, totalcount, dx);
					sw.Reset();
					sw.Start();
					PXSearchableAttribute.BulkInsert(insertDict.Values);
					sw.Stop();
					Debug.Print("{0} BulkInsert in {1} sec.", item.DisplayName, sw.Elapsed.TotalSeconds);
				}
				catch (Exception ex) {
					string msg = string.Format(Messages.OutOfProcessed, cx, totalcount, dx, ex.Message);
					throw new Exception(msg, ex);
				}
			} while (resultset.Count>0);

            PXProcessing<RecordType>.SetProcessed();
        }

        private static string ExtractNoteText(Note note)
        {
            String value = note.NoteText;
            if (String.IsNullOrWhiteSpace(value))
                return null;

            String[] parts = value.Split('\0');
            if (parts.Length < 1)
            {
                return null;
            }

            if (String.IsNullOrWhiteSpace(parts[0]))
            {
                return null;
            }
            else
            {
                return parts[0];
            }
        }

        [Serializable]
        public partial class RecordType : IBqlTable
        {
            #region Selected
            public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
            protected bool? _Selected = false;
            [PXBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
            public bool? Selected
            {
                get
                {
                    return _Selected;
                }
                set
                {
                    _Selected = value;
                }
            }
            #endregion

            #region Entity
            public abstract class entity : PX.Data.BQL.BqlString.Field<entity> { }
            protected string _Entity;
            [PXString(250, IsKey = true)]
            [PXUIField(DisplayName = "Entity", Enabled = false)]
            public virtual string Entity
            {
                get { return _Entity; }
                set { _Entity = value; }
            }
            #endregion

            #region Name
            public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
            protected string _Name;
            [PXString(250)]
            [PXUIField(DisplayName = "Entity", Enabled = false)]
            public virtual string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }
            #endregion

            #region DisplayName
            public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
            protected string _DisplayName;
            [PXString(250)]
            [PXUIField(DisplayName = "Name", Enabled = false)]
            public virtual string DisplayName
            {
                get { return _DisplayName; }
                set { _DisplayName = value; }
            }
            #endregion
        }
    }

    public class SearchCategory
    {
        public const int AP = 1;
        public const int AR = 2;
        public const int CA = 4;
        public const int FA = 8;
        public const int GL = 16;
        public const int IN = 32;
        public const int OS = 64;
        public const int PO = 128;
        public const int SO = 256;
        public const int RQ = 512;
        public const int CR = 1024;
        public const int PM = 2048;
		public const int TM = 4096;
        public const int FS = 8192;

        public const int All = 65535;

        public static int Parse(string module)
        {
            switch (module)
            {
                case "AP": return AP;
                case "AR": return AR;
                case "CA": return CA;
                case "FA": return FA;
                case "GL": return GL;
                case "IN": return IN;
                case "OS": return OS;
                case "PO": return PO;
                case "SO": return SO;
                case "RQ": return RQ;
                case "CR": return CR;
                case "PM": return PM;
				case "TM": return TM;
                case "FS": return FS;

                default: return All;
            }
        }
    }

}
