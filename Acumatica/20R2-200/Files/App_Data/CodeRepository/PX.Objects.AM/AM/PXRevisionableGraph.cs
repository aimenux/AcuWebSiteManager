using System.Collections;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Revisionable base graph
    /// </summary>
    public abstract class PXRevisionableGraph<TGraph, TPrimary, TKeyField, TRevisionField> : PXGraph<PXRevisionableGraph<TGraph, TPrimary, TKeyField, TRevisionField>>
        where TGraph : PXRevisionableGraph<TGraph, TPrimary, TKeyField, TRevisionField>, new()
        where TPrimary : class, IBqlTable, new()
        where TKeyField : class, IBqlField
        where TRevisionField : class, IBqlField
    {
        public PXSave<TPrimary> Save;
        public PXRevisionableCancel<TGraph, TPrimary, TKeyField, TRevisionField> Cancel;
        public PXRevisionableInsert<TPrimary> Insert;
        public PXDelete<TPrimary> Delete;

        public PXCopyPasteAction<TPrimary> CopyPaste;

        public PXRevisionableFirst<TPrimary, TKeyField, TRevisionField> First;
        public PXRevisionablePrevious<TPrimary, TKeyField, TRevisionField> Previous;
        public PXRevisionableNext<TPrimary, TKeyField, TRevisionField> Next;
        public PXRevisionableLast<TPrimary, TKeyField, TRevisionField> Last;

        public PXSelectOrderBy<TPrimary,
                   OrderBy<Asc<TKeyField,
                           Desc<TRevisionField>>>> Documents;

        public virtual bool HasAutoNumbering => Rev.KeyAttribute.GetIsAutoNumber<TKeyField>(Documents.Cache);

        /// <summary>
        /// Do we skip the auto creation of a new revision.
        /// Useful when the graph is running as an import or called from a contract endpoint
        /// </summary>
        /// <returns></returns>
        protected virtual bool SkipAutoCreateNewRevision()
        {
            return IsImport || IsContractBasedAPI;
        } 

        public virtual TPrimary CreateNewRevision(PXCache cache, TGraph toGraph, string keyValue, string revisionValue)
        {
            var ret = new TPrimary();

            var newRevisionValue = string.IsNullOrWhiteSpace(revisionValue)
                ? AutoNumberHelper.NextNumber(toGraph?.Documents?.Cache?.GetValue<TRevisionField>(toGraph?.Documents?.Current) as string)
                : revisionValue;

            //if autonumbering is off, need to insert a new record in the cache using new rev number
            TGraph fromGraph = HasAutoNumbering ? GetLastRevision(toGraph, keyValue) : GetLastRevision(toGraph, keyValue, newRevisionValue);

            string createError;
            if(CanCreateNewRevision(fromGraph, toGraph, keyValue, newRevisionValue, out createError))
            {
                CopyRevision(fromGraph, toGraph, keyValue, newRevisionValue);
                ret = toGraph.Documents.Current;
                cache.SetValue<TKeyField>(ret, keyValue);
                cache.SetValue<TRevisionField>(ret, newRevisionValue);

                if (HasAutoNumbering)
                {
                    Rev.KeyAttribute.InsertRevision<TKeyField>(cache);
                }
                cache.IsDirty = true;
            }
            else
            {
                ret = fromGraph?.Documents?.Current;
                cache.RaiseExceptionHandling<TRevisionField>(ret, cache.GetValue<TRevisionField>(ret), new PXSetPropertyException(createError));
                cache.IsDirty = false;
            }

            return ret;
        }

        public abstract bool CanCreateNewRevision(TGraph fromGraph, TGraph toGraph, string keyValue, string revisionValue, out string error);

        public abstract void CopyRevision(TGraph fromGraph, TGraph toGraph, string keyValue, string revisionValue);

        public TGraph GetLastRevision(TGraph toGraph, string keyValue)
        {
            var graphToCopy = PXGraph.CreateInstance<TGraph>();
            graphToCopy.Documents.Current = graphToCopy.Documents.Search<TKeyField>(keyValue);
            var currentCopy = PXCache<TPrimary>.CreateCopy(graphToCopy.Documents.Current);
            toGraph.Documents.Current = toGraph.Documents.Insert(currentCopy);
            return graphToCopy;
        }

        public TGraph GetLastRevision(TGraph toGraph, string keyValue, string newRevisionNumber)
        {
            var graphToCopy = PXGraph.CreateInstance<TGraph>();
            graphToCopy.Documents.Current = graphToCopy.Documents.Search<TKeyField>(keyValue);
            var currentCopy = PXCache<TPrimary>.CreateCopy(graphToCopy.Documents.Current);
            graphToCopy.Documents.Cache.SetValue<TRevisionField>(currentCopy, newRevisionNumber);
            toGraph.Documents.Current = toGraph.Documents.Insert(currentCopy);
            return graphToCopy;
        }
    }

    public class PXRevisionableCancel<TGraph, TPrimary, TKeyField, TRevisionField> : PXAction<TPrimary>
        where TGraph : PXRevisionableGraph<TGraph, TPrimary, TKeyField, TRevisionField>, new()
        where TPrimary : class, IBqlTable, new()
        where TKeyField : class, IBqlField
        where TRevisionField : class, IBqlField
    {
        public PXRevisionableCancel(PXGraph graph, string name)
            : base(graph, name)
        {
        }

        [PXCancelButton]
        [PXUIField(DisplayName = "Cancel", MapEnableRights = PXCacheRights.Select)]
        protected override IEnumerable Handler(PXAdapter adapter)
        {
            PXCache cache = adapter.View.Cache;
            var graph = ToRevisionableGraph(cache.Graph);
            var current = (TPrimary)cache.Current;

            var currentKey = cache.GetValue<TKeyField>(current) as string;
            var currentRev = cache.GetValue<TRevisionField>(current) as string;

            graph.Clear();
            graph.SelectTimeStamp();

            foreach (TPrimary rec in adapter.Get())
            {
                var recKey = cache.GetValue<TKeyField>(rec) as string;
                if (current != null && recKey == currentKey)
                {
                    return new object[] { rec };
                }
                adapter.Searches = new object[] { recKey };
                adapter.Descendings = new bool[] { false, true };
                foreach (TPrimary last in adapter.Get())
                {
                    return new object[] { last };
                }
            }

            var newRec = new TPrimary();
            if (adapter.Searches != null && adapter.Searches.Length == 2)
            {
                var isAutoNumber = graph.HasAutoNumbering;
                var newSymbol = isAutoNumber
                                ? Rev.KeyAttribute.GetNewSymbol<TKeyField>(cache) ?? string.Empty
                                : string.Empty;

                if (adapter.Searches[0] == null || (adapter.Searches[0].ToString().Contains(newSymbol) && isAutoNumber))
                {
                    cache.SetValue<TRevisionField>(newRec, adapter.Searches[1]);
                    newRec = (TPrimary)cache.Insert(newRec);
                    cache.IsDirty = false;
                    return new object[] { newRec };
                }

                if (currentRev?.Equals(adapter.Searches[1]) == true)
                {        
                    adapter.Searches = new object[] { adapter.Searches[0] };
                    adapter.Descendings = new bool[] { false, true };
                    foreach (TPrimary last in adapter.Get())
                    {
                        return new object[] { last };
                    }

                    //Record doesn't exist
                    cache.SetValue<TKeyField>(newRec, adapter.Searches[0]);
                    newRec = (TPrimary)cache.Insert(newRec);
                    cache.IsDirty = true;
                }
                else if(!isAutoNumber)
                {
                    //check to see if the key exists, if so attempt to create a new revision
                    currentKey = adapter.Searches[0].ToString();
                    currentRev = adapter.Searches[1].ToString();
                    adapter.Searches = new object[] { currentKey };
                    adapter.Descendings = new bool[] { false, true };
                    foreach(TPrimary existing in adapter.Get())
                    {
                        newRec = graph.CreateNewRevision(cache, ToRevisionableGraph(adapter.View.Graph), currentKey, currentRev);
                        if (newRec == null)
                        {
                            newRec = new TPrimary();
                        }
                        return new object[] { newRec };
                    }

                    //If key doesn't exist, continue on with the selected revision id
                    cache.SetValue<TKeyField>(newRec, currentKey);
                    cache.SetValue<TRevisionField>(newRec, currentRev);
                    newRec = (TPrimary)cache.Insert(newRec);
                    cache.IsDirty = true;
                }
                else 
                {
                    //Inserting a revision
                    newRec = graph.CreateNewRevision(cache, ToRevisionableGraph(adapter.View.Graph), adapter.Searches[0] as string, adapter.Searches[1] as string);
                }
            }

            if (newRec == null)
            {
                newRec = new TPrimary();
            }
            return new object[] { newRec };
        }

        private static TGraph ToRevisionableGraph(PXGraph graph)
        {
            return (TGraph)graph;
        }
    }

    public class PXRevisionableInsert<TPrimary> : PXAction<TPrimary>
        where TPrimary : class, IBqlTable, new()
    {
        public PXRevisionableInsert(PXGraph graph, string name)
            : base(graph, name)
        {
        }

        [PXInsertButton]
        [PXUIField(DisplayName = "Insert", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
        protected override IEnumerable Handler(PXAdapter adapter)
        {
            if (!adapter.View.Cache.AllowInsert)
            {
                throw new PXException(ErrorMessages.CantInsertRecord);
            }
            _Graph.Clear();
            _Graph.SelectTimeStamp();

            for (int i = 0; i < adapter.Searches.Length; i++)
            {
                adapter.Searches[i] = null;
            }

            Insert(adapter);
            foreach (object ret in adapter.Get())
            {
                yield return ret;
            }
            adapter.View.Cache.IsDirty = false;
        }
    }

    public class PXRevisionableFirst<TPrimary, TKeyField, TRevisionField> : PXAction<TPrimary>
        where TPrimary : class, IBqlTable, new()
        where TKeyField : class, IBqlField
        where TRevisionField : class, IBqlField
    {
        public PXRevisionableFirst(PXGraph graph, string name)
            : base(graph, name)
        {
        }

        [PXFirstButton]
        [PXUIField(DisplayName = "First", MapEnableRights = PXCacheRights.Select)]
        protected override IEnumerable Handler(PXAdapter adapter)
        {
            _Graph.Clear();
            _Graph.SelectTimeStamp();
            TPrimary rec = PXSelectOrderBy<TPrimary,
                                        OrderBy<Asc<TKeyField, Desc<TRevisionField>>>>
                                    .SelectWindowed(_Graph, 0, 1);
            if (rec != null)
            {
                return new object[] { rec };
            }
            return adapter.View.Cache.AllowInsert ?
                new PXRevisionableInsert<TPrimary>(_Graph, "Insert").Press(adapter) :
                new object[0];
        }
    }

    public class PXRevisionablePrevious<TPrimary, TKeyField, TRevisionField> : PXAction<TPrimary>
        where TPrimary : class, IBqlTable, new()
        where TKeyField : class, IBqlField
        where TRevisionField : class, IBqlField
    {
        public PXRevisionablePrevious(PXGraph graph, string name)
            : base(graph, name)
        {
        }

        [PXPreviousButton]
        [PXUIField(DisplayName = "Previous", MapEnableRights = PXCacheRights.Select)]
        protected override IEnumerable Handler(PXAdapter adapter)
        {
            PXCache cache = adapter.View.Cache;
            bool goToLast = cache.GetStatus(cache.Current) == PXEntryStatus.Inserted;
            _Graph.Clear();
            _Graph.SelectTimeStamp();
            TPrimary rec;
            if (goToLast)
            {
                rec = PXSelectOrderBy<TPrimary,
                          OrderBy<Desc<TKeyField, Desc<TRevisionField>>>>
                      .SelectWindowed(_Graph, 0, 1);
                if (rec != null)
                {
                    return new object[] { rec };
                }
                return cache.AllowInsert ? new PXRevisionableInsert<TPrimary>(_Graph, "Insert").Press(adapter) : new object[0];
            }
            rec = PXSelect<TPrimary,
                      Where<TKeyField, Equal<Optional<TKeyField>>,
                          And<TRevisionField, Greater<Optional<TRevisionField>>>>,
                      OrderBy<Desc<TKeyField, Asc<TRevisionField>>>>
                  .SelectWindowed(_Graph, 0, 1, adapter.Searches != null && adapter.Searches.Length > 0 ? adapter.Searches[0] : null,
                                                adapter.Searches != null && adapter.Searches.Length > 1 ? adapter.Searches[1] : null);
            if (rec == null)
            {
                rec = PXSelect<TPrimary,
                          Where<TKeyField, Less<Optional<TKeyField>>>,
                          OrderBy<Desc<TKeyField, Asc<TRevisionField>>>>
                      .SelectWindowed(_Graph, 0, 1, adapter.Searches != null && adapter.Searches.Length > 0 ? adapter.Searches[0] : null);
            }
            if (rec != null)
            {
                return new object[] { rec };
            }
            return cache.AllowInsert ?
                new PXRevisionableInsert<TPrimary>(_Graph, "Insert").Press(adapter) :
                new PXRevisionableLast<TPrimary, TKeyField, TRevisionField>(_Graph, "Last").Press(adapter);
        }
    }

    public class PXRevisionableNext<TPrimary, TKeyField, TRevisionField> : PXAction<TPrimary>
        where TPrimary : class, IBqlTable, new()
        where TKeyField : class, IBqlField
        where TRevisionField : class, IBqlField
    {
        public PXRevisionableNext(PXGraph graph, string name)
            : base(graph, name)
        {
        }

        [PXNextButton]
        [PXUIField(DisplayName = "Next", MapEnableRights = PXCacheRights.Select)]
        protected override IEnumerable Handler(PXAdapter adapter)
        {
            PXCache cache = adapter.View.Cache;
            bool goToFirst = cache.GetStatus(cache.Current) == PXEntryStatus.Inserted;
            _Graph.Clear();
            _Graph.SelectTimeStamp();
            TPrimary rec;
            if (goToFirst)
            {
                rec = PXSelectOrderBy<TPrimary,
                          OrderBy<Asc<TKeyField, Desc<TRevisionField>>>>
                      .SelectWindowed(_Graph, 0, 1);
                if (rec != null)
                {
                    return new object[] { rec };
                }
                return cache.AllowInsert ? new PXRevisionableInsert<TPrimary>(_Graph, "Insert").Press(adapter) : new object[0];
            }
            rec = PXSelect<TPrimary,
                      Where<TKeyField, Equal<Optional<TKeyField>>,
                          And<TRevisionField, Less<Optional<TRevisionField>>>>,
                      OrderBy<Asc<TKeyField, Desc<TRevisionField>>>>
                  .SelectWindowed(_Graph, 0, 1, adapter.Searches != null && adapter.Searches.Length > 0 ? adapter.Searches[0] : null,
                                                adapter.Searches != null && adapter.Searches.Length > 1 ? adapter.Searches[1] : null);
            if (rec == null)
            {
                rec = PXSelect<TPrimary,
                          Where<TKeyField, Greater<Optional<TKeyField>>>,
                          OrderBy<Asc<TKeyField, Desc<TRevisionField>>>>
                      .SelectWindowed(_Graph, 0, 1, adapter.Searches != null && adapter.Searches.Length > 0 ? adapter.Searches[0] : null);
            }
            if (rec != null)
            {
                return new object[] { rec };
            }
            return cache.AllowInsert ?
                new PXRevisionableInsert<TPrimary>(_Graph, "Insert").Press(adapter) :
                new PXRevisionableFirst<TPrimary, TKeyField, TRevisionField>(_Graph, "First").Press(adapter);
        }
    }

    public class PXRevisionableLast<TPrimary, TKeyField, TRevisionField> : PXAction<TPrimary>
        where TPrimary : class, IBqlTable, new()
        where TKeyField : class, IBqlField
        where TRevisionField : class, IBqlField
    {
        public PXRevisionableLast(PXGraph graph, string name)
            : base(graph, name)
        {
        }
        [PXLastButton]
        [PXUIField(DisplayName = "Last", MapEnableRights = PXCacheRights.Select)]
        protected override IEnumerable Handler(PXAdapter adapter)
        {
            _Graph.Clear();
            _Graph.SelectTimeStamp();
            TPrimary rec = PXSelectOrderBy<TPrimary,
                                        OrderBy<Desc<TKeyField, Desc<TRevisionField>>>>
                                    .SelectWindowed(_Graph, 0, 1);
            if (rec != null)
            {
                return new object[] { rec };
            }
            return adapter.View.Cache.AllowInsert ?
                new PXRevisionableInsert<TPrimary>(_Graph, "Insert").Press(adapter) :
                new object[0];
        }
    }
}
