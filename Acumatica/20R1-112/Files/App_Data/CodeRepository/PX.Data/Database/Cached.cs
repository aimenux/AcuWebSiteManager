// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using PX.Common;
using PX.CS;
using PX.Data.Maintenance.GI;
using PX.SM;
using PX.TM;
using System.Text.RegularExpressions;
using PX.Api;
using PX.Api.Export;
using PX.Data.SQLTree;
using System.Runtime.CompilerServices;

namespace PX.Data
{
	/// <summary>
	/// A controller that executes the BQL command and implements interfaces for sorting, searching, merging data with the cached changes, and caching the result set.
	/// </summary>

	/// <summary>A controller that executes the BQL command and implements
	/// interfaces for sorting, searching, merging data with the cached
	/// changes, and caching the result set.</summary>
	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	public class PXView
	{
		/// <summary>A <see cref="PXView" /> that always returns only a predefined set of rows</summary>
		/// <exclude />
		public sealed class Dummy : PXView
		{
			private readonly List<object> _records;
			public Dummy(PXGraph graph, BqlCommand command, List<object> records)
				: base(graph, true, command)
			{
				_records = records;
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				return _records;
			}

			public static Dummy For<TTable>(PXGraph graph) where TTable : class, IBqlTable, new() => For(graph, ((TTable)graph.Caches<TTable>().Current).AsSingleEnumerable());
			public static Dummy For<TTable>(PXGraph graph, IEnumerable<TTable> items) where TTable : class, IBqlTable, new() => new Dummy(graph, new Select<TTable>(), items.ToList<object>());
		}

		[Serializable]
		protected internal sealed class VersionedList : List<object>
		{
			public int Version = -1;
			public bool AnyMerged;

			public VersionedList()
				: base()
			{
			}

			public VersionedList(IEnumerable<object> collection, int Version)
				: base(collection)
			{
				this.Version = Version;
			}
		}
		protected sealed class Context
		{
			public readonly PXView View;
			public PXSearchColumn[] Sorts;
			public PXFilterRow[] Filters;
			public readonly object[] Currents;
			public readonly object[] Parameters;
			public bool ReverseOrder;
			public int StartRow;
			public readonly int MaximumRows;
			public readonly RestrictedFieldsSet RestrictedFields;
			public readonly bool RetrieveTotalRowCount;

			public Context(PXView view, PXSearchColumn[] sorts, PXFilterRow[] filters, object[] currents, object[] parameters, bool reverseOrder, int startRow, int maximumRows,
				bool retrieveTotalRowCount,
				RestrictedFieldsSet restrictedFields)
			{
				View = view;
				Sorts = sorts;
				Filters = filters;
				Currents = currents;
				Parameters = parameters;
				ReverseOrder = reverseOrder;
				StartRow = startRow;
				MaximumRows = maximumRows;
				RetrieveTotalRowCount = retrieveTotalRowCount;
				RestrictedFields = restrictedFields;
			}
		}
		protected static Stack<Context> _Executing
		{
			get
			{
				Stack<Context> context = PX.Common.PXContext.GetSlot<Stack<Context>>();
				if (context == null)
				{
					context = PX.Common.PXContext.SetSlot<Stack<Context>>(new Stack<Context>());
				}
				return context;
			}
		}

		internal RestrictedFieldsSet RestrictedFields = new RestrictedFieldsSet();

		public static RestrictedFieldsSet CurrentRestrictedFields
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					var context = _Executing.Peek();
					if (context.RestrictedFields != null)
					{
						return new RestrictedFieldsSet(context.RestrictedFields);
					}
				}
				return new RestrictedFieldsSet();
			}
		}

		public static bool LegacyQueryCacheModeEnabled
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					var context = _Executing.Peek();
					if (context.View != null)
					{
						return context.View._LegacyQueryCacheMode;
					}
				}
				return true;
			}
		}

		/// <summary>Gets the names of the fields passed to the <tt>Select(...)</tt> method to filter and sort the data set.</summary>
		public static string[] SortColumns
		{
			get
			{
				List<string> ret = new List<string>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add(sc.Column);
						}
					}
				}
				return ret.ToArray();
			}
		}
		/// <summary>Gets the values passed to the <tt>Select(...)</tt> method to indicate whether ordering by the sort columns should be descending or ascending.</summary>
		public static bool[] Descendings
		{
			get
			{
				List<bool> ret = new List<bool>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add(sc.Descending);
						}
					}
				}
				return ret.ToArray();
			}
		}
		/// <summary>Gets the values passed to the <tt>Select(...)</tt> method to filter the data set by them.</summary>
		public static object[] Searches
		{
			get
			{
				List<object> ret = new List<object>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add(sc.OrigSearchValue);
						}
					}
				}
				return ret.ToArray();
			}
			internal set
			{
				if (value == null)
					value = new object[0];
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null && context.Sorts.Length == value.Length)
					{
						for (int i = 0; i < context.Sorts.Length; i++)
						{
							context.Sorts[i].SearchValue = value[i];
							context.Sorts[i].OrigSearchValue = value[i];
						}
					}
				}
			}
		}

		/// <summary>Gets the graph within which the <tt>Select(...)</tt> method was invoked.</summary>
		public static PXGraph CurrentGraph
		{
			get
			{
				Context context;
				return _Executing != null && _Executing.Count > 0 && (context = _Executing.Peek()) != null ? context.View.Graph : null;
			}
		}

		public sealed class PXFilterRowCollection : IEnumerable
		{
			private PXFilterRow[] _Array;
			public IEnumerator GetEnumerator()
			{
				return _Array.GetEnumerator();
			}
			public int Length
			{
				get
				{
					return _Array.Length;
				}
			}
			public static implicit operator PXFilterRow[](PXFilterRowCollection collection)
			{
				return collection._Array;
			}
			public PXFilterRowCollection(PXFilterRow[] source)
			{
				_Array = source;
			}
			public PXFilterRow this[int index]
			{
				get
				{
					return _Array[index];
				}
			}

			public void Add(params PXFilterRow[] filters)
			{
				if (_Executing != null && _Executing.Count > 0 && filters.Length > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						List<PXFilterRow> list = new List<PXFilterRow>();
						if (context.Filters != null && context.Filters.Length > 0)
						{
							foreach (PXFilterRow fr in context.Filters)
							{
								list.Add(new PXFilterRow(fr.DataField, fr.Condition, fr.OrigValue ?? fr.Value, fr.OrigValue2 ?? fr.Value2, fr.Variable));
								list[list.Count - 1].OpenBrackets = fr.OpenBrackets;
								list[list.Count - 1].CloseBrackets = fr.CloseBrackets;
								list[list.Count - 1].OrOperator = fr.OrOperator;
							}
							if (list.Count > 1)
							{
								list[0].OpenBrackets += 1;
								list[list.Count - 1].CloseBrackets += 1;
							}
							list[list.Count - 1].OrOperator = false;
						}
						foreach (PXFilterRow fr in filters)
						{
							list.Add(new PXFilterRow(fr.DataField, fr.Condition, fr.OrigValue ?? fr.Value, fr.OrigValue2 ?? fr.Value2, fr.Variable));
							list[list.Count - 1].OpenBrackets = fr.OpenBrackets;
							list[list.Count - 1].CloseBrackets = fr.CloseBrackets;
							list[list.Count - 1].OrOperator = fr.OrOperator;
						}
						context.Filters = list.ToArray();
					}
				}
			}

			public void Clear()
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						context.Filters = new PXFilterRow[0];
					}
				}
			}
			internal bool PrepareFilters()
			{
				return PrepareFilters(out _);
			}
			internal bool PrepareFilters(out bool skipped)
			{
				skipped = true;
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						skipped = false;
						return context.View.prepareFilters(ref _Array);
					}
				}
				return false;
			}
		}
		/// <summary>Gets the filtering conditions originated on the user interface and passed to the <tt>Select(...)</tt> method.</summary>
		public static PXFilterRowCollection Filters
		{
			get
			{
				List<PXFilterRow> ret = new List<PXFilterRow>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Filters != null)
					{
						foreach (PXFilterRow fr in context.Filters)
						{
							ret.Add(new PXFilterRow(fr.DataField, fr.Condition, fr.OrigValue ?? fr.Value, fr.OrigValue2 ?? fr.Value2, fr.Variable));
							ret[ret.Count - 1].OpenBrackets = fr.OpenBrackets;
							ret[ret.Count - 1].CloseBrackets = fr.CloseBrackets;
							ret[ret.Count - 1].OrOperator = fr.OrOperator;
						}
					}
				}
				return new PXFilterRowCollection(ret.ToArray());
			}
		}
		protected internal sealed class PXSortColumnCollection : IEnumerable
		{
			private PXSearchColumn[] _Array;
			public IEnumerator GetEnumerator()
			{
				return _Array.GetEnumerator();
			}
			public int Length
			{
				get
				{
					return _Array.Length;
				}
			}
			public static implicit operator PXSearchColumn[](PXSortColumnCollection collection)
			{
				return collection._Array;
			}
			public PXSortColumnCollection(PXSearchColumn[] source)
			{
				_Array = source;
			}
			public PXSearchColumn this[int index]
			{
				get
				{
					return _Array[index];
				}
			}
			public void Add(params PXSearchColumn[] sorts)
			{
				if (_Executing != null && _Executing.Count > 0 && sorts.Length > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						List<PXSearchColumn> list = new List<PXSearchColumn>();
						if (context.Filters != null && context.Filters.Length > 0)
						{
							foreach (PXSearchColumn sc in context.Sorts)
							{
								PXSearchColumn copy = new PXSearchColumn(sc.Column, sc.Descending, sc.SearchValue);
								copy.Description = sc.Description;
								copy.OrigSearchValue = sc.OrigSearchValue;
								copy.UseExt = sc.UseExt;
								list.Add(copy);
							}
						}
						foreach (PXSearchColumn sc in sorts)
						{
							PXSearchColumn column = null;
							foreach (PXSearchColumn exist in list)
							{
								if (exist.Column == sc.Column)
								{
									column = exist;
									break;
								}
							}

							if (column == null)
							{
								column = new PXSearchColumn(sc.Column, sc.Descending, null);
								list.Add(column);
							}
							else
							{
								column.Descending = sc.Descending;
							}
							column.UseExt = sc.UseExt;
						}
						context.Sorts = list.ToArray();
					}
				}
			}

			[Obsolete("Use overload with PXSearchColumn[] argument instead.")]
			public void Add(params PXSortColumn[] sorts)
			{
				this.Add(sorts.Select(s => new PXSearchColumn(s.Column, s.Descending, null) { UseExt = s.UseExt }).ToArray());
			}

			internal void Clear()
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						context.Sorts = new PXSearchColumn[0];
					}
				}
			}
		}
		protected internal static PXSortColumnCollection Sorts
		{
			get
			{
				List<PXSearchColumn> ret = new List<PXSearchColumn>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add((PXSearchColumn) sc.Clone());
						}
					}
				}
				return new PXSortColumnCollection(ret.ToArray());
			}
		}

		/// <exclude />
		public static IEnumerable<PXSearchColumn> SearchColumns => PXView.Sorts.Cast<PXSearchColumn>();

		/// <summary>Gets the current data records passed to the <tt>Select(...)</tt> method to process the <tt>Current</tt> and <tt>Optional</tt> parameters.</summary>
		public static object[] Currents
		{
			get
			{
				List<object> ret = new List<object>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Currents != null)
					{
						foreach (object item in context.Currents)
						{
							ret.Add(item);
						}
					}
				}
				return ret.ToArray();
			}
		}
		/// <summary>Gets the values passed to the <tt>Select(...)</tt> method to process such parameters as <tt>Required</tt>, <tt>Optional</tt>, and <tt>Argument</tt>, and
		/// pre-processed by the <tt>Select(...)</tt> method.</summary>
		public static object[] Parameters
		{
			get
			{
				List<object> ret = new List<object>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Parameters != null)
					{
						foreach (object item in context.Parameters)
						{
							ret.Add(item);
						}
					}
				}
				return ret.ToArray();
			}
		}

		/// <summary>Gets or sets the value passed to the <tt>Select(...)</tt> method as the index of the first data record to retrieve.</summary>
		public static int StartRow
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						if (context.ReverseOrder && context.MaximumRows > 0)
						{
							return -1 - context.StartRow;
						}
						else
						{
							return context.StartRow;
						}
					}
				}
				return 0;
			}
			set
			{
				if (_Executing != null)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						context.StartRow = value;
					}
				}
			}
		}
		/// <summary>Gets the value passed to the <tt>Select(...)</tt> method as the number of data records to retrieve.</summary>
		public static int MaximumRows
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						return context.MaximumRows;
					}
				}
				return 0;
			}
		}

		public static bool NeedDefaultPrimaryViewObject => MaximumRows == 1 && (!Searches.Any() || Searches.All(key => key == null));

		/// <summary>Gets the value indicating whether a negative value was passed as the index of the first data record to retrieve.</summary>
		public static bool ReverseOrder
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						return context.ReverseOrder;
					}
				}
				return false;
			}
			set
			{
				if (_Executing != null)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						context.ReverseOrder = value;
					}
				}
			}
		}

		/// <summary>Gets the value indicating whether a view delegate should retrieve total row count.</summary>
		public static bool RetrieveTotalRowCount
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						return context.RetrieveTotalRowCount;
					}
				}
				return false;
			}
		}

		/// <summary>Sort the provided collection of <tt>PXResult&lt;&gt;</tt> instances by the conditions currently stored in the <tt>PXView</tt> context. This context exists only
		/// during execution of the <see cref="PXView.Select">Select(...)</see> method. The <tt>Sort(IEnumerable)</tt> method may be called in the optional method of the data view to sort
		/// by the conditions that were provided to the <tt>Select(...)</tt> method, which invoked the optional method.</summary>
		/// <param name="list">The collection of <tt>PXResult&lt;&gt;</tt>
		/// instances to sort.</param>
		public static IEnumerable Sort(IEnumerable list)
		{
			if (_Executing != null)
			{
				Context context = _Executing.Peek();
				if (context != null)
				{
					if (list is List<object>)
					{
						context.View.SortResult((List<object>)list, context.Sorts, context.ReverseOrder);
						return list;
					}
					else
					{
						List<object> result = list is VersionedList ? new VersionedList() : new List<object>();
						foreach (object item in list)
						{
							result.Add(item);
						}
						context.View.SortResult(result, context.Sorts, context.ReverseOrder);
						if (result is VersionedList)
						{
							((VersionedList)result).Version = ((VersionedList)list).Version;
						}
						return result;
					}
				}
			}
			return list;
		}
		/// <exclude/>
		public static void SortClear()
		{
			PXView.Sorts.Clear();
		}
		/// <exclude/>
		public static IEnumerable Filter(IEnumerable list)
		{
			if (_Executing != null)
			{
				Context context = _Executing.Peek();
				if (context != null)
				{
					if (list is List<object>)
					{
						context.View.FilterResult((List<object>)list, context.Filters);
						return list;
					}
					else
					{
						List<object> result = list is VersionedList ? new VersionedList() : new List<object>();
						foreach (object item in list)
						{
							result.Add(item);
						}
						context.View.FilterResult(result, context.Filters);
						if (result is VersionedList)
						{
							((VersionedList)result).Version = ((VersionedList)list).Version;
						}
						return result;
					}
				}
			}
			return list;
		}
		public static PXView View
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						return context.View;
					}
				}
				return null;
			}
		}
		internal PXGraphExtension[] Extensions;
		protected PXGraphExtension[] GraphExtensions { get { return Extensions; } }
		private Func<PXFilterTuple> _ExternalFilterDelegate = () => new PXFilterTuple();
		internal void StoreExternalFilters(Func<PXFilterTuple> del)
		{
			_ExternalFilterDelegate = del;
		}

		/// <exclude/>
		public PXFilterRow[] GetExternalFilters()
		{
			return _ExternalFilterDelegate().FilterRows;
		}

		internal long? GetExternalFilterID()
		{
			return _ExternalFilterDelegate().FilterID;
		}

		private Func<Tuple<string[], bool[]>> _ExternalSortsDelegate = () => new Tuple<string[], bool[]>(null, null);
		internal void StoreExternalSorts(Func<Tuple<string[], bool[]>> del)
		{
			_ExternalSortsDelegate = del;
		}

		/// <exclude />
		public string[] GetExternalSorts()
		{
			return _ExternalSortsDelegate().Item1;
		}

		/// <exclude />
		public bool[] GetExternalDescendings()
		{
			return _ExternalSortsDelegate().Item2;
		}

		/// <summary>Resets applied filters for all views in the data graph.</summary>
		public virtual void RequestFiltersReset()
		{
			FiltersResetRequired = true;
		}
		internal bool FiltersResetRequired { get; private set; }

		private Type[] _dependToCacheTypes;

		internal void SetDependToCacheTypes(IEnumerable<Type> types)
		{
			_dependToCacheTypes = types.ToArray();
		}


		internal IEnumerable<Type> GetDependToCacheTypes()
		{
			if (_dependToCacheTypes != null)
				return _dependToCacheTypes;
			if (BqlDelegate != null &&
				BqlDelegate.Method?.GetCustomAttribute<PXOptimizationBehaviorAttribute>()?.IgnoreBqlDelegate != true)
			{
				var dependToCacheAttribute = BqlDelegate.Method?.GetCustomAttribute<PXDependToCacheAttribute>();
				_dependToCacheTypes = dependToCacheAttribute?.ViewTypes;
				return _dependToCacheTypes;
			}
			Type previousType = null;
			var result = new List<Type>();
			foreach (var type in BqlCommand.Decompose(this.BqlSelect.GetType()))
			{
				if (previousType == typeof(Current<>) || previousType == typeof(Current2<>) || previousType == typeof(Optional<>) || previousType == typeof(Optional2<>))
				{
					var cacheType = type.DeclaringType;
					if (cacheType != null)
						result.Add(Graph.Caches[cacheType].GenericParameter);
				}
				previousType = type;
			}

			_dependToCacheTypes = result.ToArray();
			return _dependToCacheTypes;
		}


		protected PXGraph _Graph;
		/// <summary>Gets or sets the parent business object.</summary>
		public virtual PXGraph Graph
		{
			get
			{
				return _Graph;
			}
			set
			{
				_Graph = value;
			}
		}

		/// <exclude/>
		public PXViewExtensionAttribute[] Attributes;

		protected bool _IsReadOnly = false;
		/// <summary>Gets or sets the value that indicates (if set to <tt>true</tt>) that placing retrieved data records into the cache and merging them with the cache are allowed.</summary>
		public virtual bool IsReadOnly
		{
			get
			{
				return _IsReadOnly;
			}
			set
			{
				_IsReadOnly = value;
				_SelectQueries = null;
			}
		}

		protected bool _LegacyQueryCacheMode = true;
		/// <summary>Gets or sets the value that indicates (if set to <tt>true</tt>) that placing of the retrieved data records into the cache and merging of them with the cache
		/// are allowed.</summary>
		public virtual bool LegacyQueryCacheMode
		{
			get
			{
				return _LegacyQueryCacheMode;
			}
			set
			{
				_LegacyQueryCacheMode = value;
			}
		}

		private BqlCommand _Select;
		/// <summary>Returns the string with the SQL query corresponding to the underlying BQL command.</summary>
		public override string ToString()
		{
			if (_Select.Context != null && _Select.Context.LastCommandText != null)
				return _Select.Context.LastCommandText;
			return _Select.GetQuery(_Graph, RestrictedFields.Any() ? this : null).ToString();
			//return _Select.GetText(_Graph, RestrictedFields.Any() ? this : null);
		}

		protected Delegate _Delegate;
		/// <summary>Gets the delegate representing the method (called <i>optional method</i> in this reference) which is invoked by the <tt>Select(...)</tt> method to retrieve the
		/// data. If this method is provided to the <tt>PXView</tt> object, the Select(...) method doesn't retrieve data from the database and returns the result returned
		/// by the optional method.</summary>
		public Delegate BqlDelegate
		{
			get
			{
				return _Delegate;
			}
		}

		protected Type _CacheType;
		internal protected Type CacheType
		{
			get
			{
				if (_CacheType == null)
				{
					//_CacheType = _Select.GetTables()[0];
					_CacheType = _Select.GetFirstTable();
				}
				return _CacheType;
			}
			set
			{
				_CacheType = value;
			}
		}
		PXCache _cache;
		Action _init;
		
		protected internal PXCache _Cache
		{
			get
			{
				if (_init != null)
				{
					_init();
					_init = null;

				}

				return _cache;

			}
			set
			{
				if (_init != null)
				{
					_init = null;

				}

				_cache = value;
			}
		}
		/// <summary>Gets the cache corresponding to the first DAC mentioned in the BQL command.</summary>
		public virtual PXCache Cache
		{
			get
			{
				if (_Cache == null || _Graph.stateLoading)
				{
					_Cache = Graph.Caches[CacheType];
				}
				return _Cache;
			}
		}

		/// <summary>Returns the DAC type of the primary cache; that is, the first DAC referenced in the BQL command.</summary>
		public virtual Type GetItemType()
		{
			return CacheType;
		}

		public Type CacheGetItemType()
		{
			if (_Cache != null)
				return _Cache.GetItemType();
			return Graph.Caches.GetRealCacheType(CacheType);
		}

		/// <summary>Gets the underlying BQL command. If the current <tt>PXView</tt> object is associated with a variant of <tt>PXSelect&lt;&gt;</tt> object, the BQL command type
		/// has the the same type parameters as the type of this object, so it represents the same SQL query.</summary>
		public virtual BqlCommand BqlSelect
		{
			get
			{
				return _Select;
			}
		}

		/// <summary>Gets the class that defines the optional method of a data view. Typically, this class is the graph that defines both the data view and its optional method. The
		/// optional method is the method represented by <tt>BqlDelegate</tt> when a data view is defined as a member of a graph.</summary>
		public virtual Type BqlTarget
		{
			get
			{
				if (_Delegate != null)
				{
					return _Delegate.Method.DeclaringType;
				}
				return null;
			}
		}

		/// <summary>Returns all DAC types referenced in the BQL command.</summary>
		public virtual Type[] GetItemTypes()
		{
			return _Select.GetTables();
		}

		protected string[] _ParameterNames;
		/// <summary>Returns the names of the fields referenced by BQL parameters and the names of parameters of the optional method, if it is defined.</summary>
		public virtual string[] GetParameterNames()
		{
			if (_ParameterNames == null)
			{
				var list = EnumParameters();
				_ParameterNames = new string[list.Count];

				foreach (PXViewParameter info in list)
				{
					_ParameterNames[info.Ordinal] = info.Name;
				}

			}

			return _ParameterNames;


		}

		/// <summary>Returns the information on the fields referenced by BQL parameters and parameters of the optional method, if it is defined for the data view.</summary>
		public virtual List<PXViewParameter> EnumParameters()
		{
			var result = new List<PXViewParameter>();
			//if (_ParameterNames == null)
			{
				IBqlParameter[] pars = _Select.GetParameters();
				ParameterInfo[] pi = _Delegate == null ? null : _Delegate.Method.GetParameters();
				int k = 0;
				//var names = new List<string>();
				for (int i = 0; i < pars.Length; i++)
				{
					if (!pars[i].IsVisible)
					{
						continue;
					}
					Type rt = pars[i].GetReferencedType();
					if (typeof(IBqlField).IsAssignableFrom(rt) && rt.IsNested)
					{
						string n = String.Format("{0}.{1}", BqlCommand.GetItemType(rt).Name, rt.Name);
						result.Add(new PXViewParameter { Name = n, Bql = pars[i], Ordinal = result.Count });
					}
					else if (pars[i].IsArgument)
					{
						if (pi == null || k >= pi.Length || rt != pi[k].ParameterType && rt != pi[k].ParameterType.GetElementType())
						{
							throw new PXException(ErrorMessages.DelegateArgsDontMeetSelectionCommandPars);
						}
						string n = pi[k].Name;
						result.Add(new PXViewParameter { Name = n, Bql = pars[i], Argument = pi[k], Ordinal = result.Count });
						k++;
					}
				}
				if (pi != null)
				{
					for (; k < pi.Length; k++)
					{
						string n = pi[k].Name;
						result.Add(new PXViewParameter { Name = n, Argument = pi[k], Ordinal = result.Count });
					}
				}
				//_ParameterNames = names.ToArray();
			}
			return result;
		}

		private protected bool? _isNonStandardView;
		protected bool IsNonStandardView
		{
			get
			{
				return _isNonStandardView ?? (this.GetType() != typeof(PXView) || _Delegate != null);
			}
		}

		PXViewQueryCollection _SelectQueries;
		protected PXViewQueryCollection SelectQueries
		{
			get
			{


				if (_SelectQueries != null)
					return _SelectQueries;


				if (IsNonStandardView && _Delegate == null)
				{
					_SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = _IsReadOnly };
					_Graph.TypedViews._NonstandardViews.Add(_SelectQueries);
					return _SelectQueries;
				}



				_Graph.LoadQueryCache();

				//if(!_Graph.TypedViews._QueriesLoaded)
				//{
				//    _SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = _IsReadOnly };
				//    return _SelectQueries;

				//}


				Type selectType = _Select.GetSelectType();
				PXViewQueryCollection viewQueries;
				var viewKey = new ViewKey(selectType, _IsReadOnly);
				if (!_Graph.QueryCache.TryGetValue(viewKey, out viewQueries))
				{
					_SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = _IsReadOnly };
					_Graph.QueryCache[viewKey] = _SelectQueries;
					return _SelectQueries;

				}


				_SelectQueries = viewQueries;


				if (!IsReadOnly)
				{
					PXCache cache = null;
					foreach (var queryResult in viewQueries.Values)
					{
						if (queryResult.HasPlacedNotChanged)
							continue;

						if (cache == null)
						{
							cache = Cache;
							cache.Normalize();
						}

						for (int i = 0; i < queryResult.Items.Count;)
						{
							PXResult result = null;
							object item = queryResult.Items[i];
							if (item is PXResult)
							{
								result = ((PXResult)item);
								item = result[0];
								if (item == null)
								{
									// LINQ result without DACs
									i++;
									continue;
								}
							}
							try
							{
								bool wasUpdated;
								object newitem = cache.PlaceNotChanged(item, out wasUpdated);
								if (newitem != null)
								{
								if (!object.ReferenceEquals(newitem, item))
								{
									if (result != null)
									{
										result[0] = newitem;
									}
									else
									{
										queryResult.Items[i] = newitem;
									}
								}
									i++;
								}
								else
								{
									queryResult.Items.RemoveAt(i);
								}
							}
							catch (InvalidCastException)
							{
								_SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = false };
								_Graph.QueryCache[viewKey] = _SelectQueries;
								break;
							}
						}

						queryResult.HasPlacedNotChanged = true;
					}
				}


				return _SelectQueries;
			}
		}
		/// <summary>Clears the results of BQL statement execution.</summary>
		public virtual void Clear()
		{
			SelectQueries.Clear();
		}

		/// <summary>Initialize a new cache for storing the results of BQL statement execution.</summary>
		public void DetachCache()
		{
			_SelectQueries = new PXViewQueryCollection();
		}

		/// <summary>Initializes an instance for executing the BQL command.</summary>
		/// <param name="graph">The graph with which the instance is associated.</param>
		/// <param name="isReadOnly">The value that indicates if updating the cache and merging data with the cache are allowed.</param>
		/// <param name="select">The BQL command as an instance of the type derived from the <tt>BqlCommand</tt> class.</param>
		public PXView(PXGraph graph, bool isReadOnly, BqlCommand select)
		{
			Answers = new AnswerIndexer(this);
			_Graph = graph;
			_IsReadOnly = isReadOnly;
			_Select = select is PX.Data.BQL.BqlCommandDecorator decoratedSelect ? decoratedSelect.Unwrap() : select;
			if (!_IsReadOnly)
			{
				if (!graph.Caches.CanInitLazyCache())
				{
					_Cache = graph.Caches[CacheType];
				}
				else
				{
					_init = CacheLazyLoad;
					graph.Caches.ProcessCacheMapping(graph, CacheType);
				}
			}
			PXExtensionManager.InitExtensions(this);
		}

		private void CacheLazyLoad()
		{
			_Cache = Graph.Caches[CacheType];
		}

		/// <summary>Initializes an instance for executing the BQL command using the provided method to retrieve data.</summary>
		/// <param name="graph">The graph with which the instance is associated.</param>
		/// <param name="isReadOnly">The value that indicates if updating the cache and merging data with the cache are allowed.</param>
		/// <param name="select">The BQL command as an instance of the type derived from the <tt>BqlCommand</tt> class.</param>
		/// <param name="handler">Either PXPrepareDelegate or PXSelectDelegate.</param>
		public PXView(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler)
			: this(graph, isReadOnly, select)
		{
			if (handler == null)
			{
				throw new PXArgumentException(nameof(handler), ErrorMessages.ArgumentNullException);
			}
			Type t = handler.GetType();
			if (!t.IsGenericType)
			{
				if (!(handler is PXSelectDelegate) && t != typeof(PXPrepareDelegate))
				{
					throw new PXException(ErrorMessages.InvalidDelegate);
				}
			}
			else
			{
				t = t.GetGenericTypeDefinition();
				if (t != typeof(PXSelectDelegate<>) && t != typeof(PXPrepareDelegate<>) &&
					t != typeof(PXSelectDelegate<,>) && t != typeof(PXPrepareDelegate<,>) &&
					t != typeof(PXSelectDelegate<,,>) && t != typeof(PXPrepareDelegate<,,>) &&
					t != typeof(PXSelectDelegate<,,,>) && t != typeof(PXPrepareDelegate<,,,>) &&
					t != typeof(PXSelectDelegate<,,,,>) && t != typeof(PXPrepareDelegate<,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,,,,>)
				)
				{
					throw new PXException(ErrorMessages.InvalidDelegate);
				}
			}
			_Delegate = handler;

			PXExtensionManager.InitExtensions(this);
		}

		protected bool _AllowSelect = true;
		/// <summary>Gets or sets the value that indicates (if set to <tt>true</tt>) that selecting of data records of the view's main DAC type is allowed in the UI.</summary>
		public bool AllowSelect
		{
			get
			{
				if (_AllowSelect)
				{
					return Cache.AllowSelect;
				}
				return false;
			}
			set
			{
				if (value || !PXGraph.GeneratorIsActive)
				{
					_AllowSelect = value;
				}
			}
		}

		protected bool _AllowInsert = true;
		/// <summary>Gets or sets the value that indicates (if set to <tt>true</tt>) that insertion of data records of the view's main DAC type is allowed in the UI.</summary>
		public bool AllowInsert
		{
			get
			{
				if (_AllowInsert && !IsReadOnly)
				{
					return Cache.AllowInsert;
				}
				return false;
			}
			set
			{
				_AllowInsert = value;
			}
		}

		protected bool _AllowUpdate = true;
		/// <summary>Gets or sets the value that indicates (if set to <tt>true</tt>) that the update of data records of the view's main DAC type is allowed in the UI.</summary>
		public bool AllowUpdate
		{
			get
			{
				if (_AllowUpdate && !IsReadOnly)
				{
					return Cache.AllowUpdate;
				}
				return false;
			}
			set
			{
				_AllowUpdate = value;
			}
		}

		protected bool _AllowDelete = true;
		/// <summary>Gets or sets the value that indicates (if set to <tt>true</tt>) that deletion of data records of the view's main DAC type is allowed in the UI.</summary>
		public bool AllowDelete
		{
			get
			{
				if (_AllowDelete && !IsReadOnly)
				{
					return Cache.AllowDelete;
				}
				return false;
			}
			set
			{
				_AllowDelete = value;
			}
		}

		internal Type ConstructSort(PXSearchColumn[] sorts, List<PXDataValue> pars, bool reverseOrder)
		{
			Type newsort = null;
			List<Type> fields = Cache.BqlFields;
			int parspos = pars.Count;
			for (int i = sorts.Length - 1; i >= 0; i--)
			{
				Type b = null;
				PXCache cache;
				string field;
				bool corrected = CorrectCacheAndField(sorts[i].Column, out cache, out field);

				if (corrected)
				{
					b = sorts[i].SelSort ?? cache.GetBqlField(field);
				}
				else
				{
					for (int j = fields.Count - 1; j >= 0; j--)
					{
						if (String.Compare(fields[j].Name, sorts[i].Column, StringComparison.OrdinalIgnoreCase) == 0)
						{
							b = fields[j];
							break;
						}
					}
				}
				if (!sorts[i].UseExt)
				{
					if (b != null)
					{
						if (reverseOrder ^ sorts[i].Descending)
						{
							if (newsort == null)
							{
								newsort = typeof(Desc<>).MakeGenericType(b);
							}
							else
							{
								newsort = typeof(Desc<,>).MakeGenericType(b, newsort);
							}
						}
						else
						{
							if (newsort == null)
							{
								newsort = typeof(Asc<>).MakeGenericType(b);
							}
							else
							{
								newsort = typeof(Asc<,>).MakeGenericType(b, newsort);
							}
						}
					}
				}
				else if (sorts[i].Description != null && sorts[i].Description.Expr != null)
				{
					PXCommandPreparingEventArgs.FieldDescription descr = null;
					if (b != null && b.IsNested && CacheType != Cache.GetItemType()
						&& (BqlCommand.GetItemType(b) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(b)))
						&& sorts[i].Description.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
					{
						Cache.RaiseCommandPreparing(sorts[i].Column, null, sorts[i].SearchValue, PXDBOperation.Select | PXDBOperation.External, CacheType, out descr);
					}
					if (descr == null || descr.Expr == null)
					{
						descr = sorts[i].Description;
					}
					if (reverseOrder ^ sorts[i].Descending)
					{
						if (newsort == null)
						{
							newsort = typeof(FieldNameDesc);
						}
						else
						{
							newsort = typeof(FieldNameDesc<>).MakeGenericType(newsort);
						}
					}
					else
					{
						if (newsort == null)
						{
							newsort = typeof(FieldNameAsc);
						}
						else
						{
							newsort = typeof(FieldNameAsc<>).MakeGenericType(newsort);
						}
					}
					string fieldName = descr.Expr?.SQLQuery(Graph.SqlDialect.GetConnection()).ToString();
					string exprName = descr.Expr?.ToString();
					SQLTree.SQLExpression fieldExpr = descr.Expr?.Duplicate();
					int idxdot, idxscore;
					if ((idxdot = fieldName.IndexOf('.')) != -1 && fieldName.IndexOf("CASE ") == -1 && fieldName.IndexOf('(') == -1 && (idxscore = sorts[i].Column.IndexOf("__")) != -1)
					{
						fieldName = sorts[i].Column.Substring(0, idxscore) + fieldName.Substring(idxdot);
					}
					if (_Select is IBqlAggregate && !String.IsNullOrEmpty(fieldName))
					{
						if (_Selection == null)
						{
							_Selection = new BqlCommand.Selection();
							SQLTree.Query q=_Select.GetQueryInternal(_Graph, new BqlCommandInfo(false), _Selection);
						}
						SQLTree.SQLExpression newExpr = _Selection.GetExpr(exprName);
						string newname = newExpr?.SQLQuery(Graph.SqlDialect.GetConnection()).ToString();
						if (!String.IsNullOrEmpty(newname) && !String.Equals(newname, fieldName, StringComparison.OrdinalIgnoreCase))
						{
							fieldName = newname;
							if (newExpr != null) fieldExpr = newExpr;
						}
						else
						{
							Cache.RaiseCommandPreparing(sorts[i].Column, null, null, PXDBOperation.Select, Cache.GetItemType(), out descr);
							if (descr == null || descr.Expr == null)
							{
								string baseField = sorts[i].Column.Substring(0, sorts[i].Column.IndexOf('_'));
								Cache.RaiseCommandPreparing(baseField, null, null, PXDBOperation.Select, Cache.GetItemType(), out descr);
							}
							if (descr != null && descr.Expr != null)
							{
								string newfieldName = descr.Expr.SQLQuery(Graph.SqlDialect.GetConnection()).ToString();
								newExpr = _Selection.GetExpr(descr.Expr?.ToString());
								newname = newExpr?.SQLQuery(Graph.SqlDialect.GetConnection()).ToString();
								fieldExpr.substituteNode(descr.Expr, newExpr);
								if (!String.IsNullOrEmpty(newname) && !String.Equals(newname, newfieldName, StringComparison.OrdinalIgnoreCase))
								{
									int pos = fieldName.IndexOf(newfieldName);
									while (pos != -1)
									{
										if ((pos == 0 || !char.IsLetterOrDigit(fieldName[pos - 1]))
											&& (pos + newfieldName.Length == fieldName.Length || !char.IsLetterOrDigit(fieldName[pos + newfieldName.Length])))
										{
											fieldName = fieldName.Substring(0, pos) + newname + (pos + newfieldName.Length == fieldName.Length ? "" : fieldName.Substring(pos + newfieldName.Length));
											pos = pos + newname.Length + 1;
										}
										else
										{
											pos = pos + newfieldName.Length + 1;
										}
										pos = fieldName.IndexOf(newfieldName, pos);
									}
								}
							}
							else if (_Selection.GrouppedBy.Count > 0)// virtual description column from selector, and grouping exists
							{

							}
						}
					}
					pars.Insert(parspos, new PXFieldName(fieldName, fieldExpr));
				}
			}
			if (newsort != null)
			{
				return typeof(OrderBy<>).MakeGenericType(newsort);
			}
			else
			{
				return null;
			}
		}

		protected BqlCommand.Selection _Selection;

		/// <summary>Returns pairs of the names of the fields by which the data view result will be sorted and values indicating if the sort by the field is descending.</summary>
		public virtual KeyValuePair<string, bool>[] GetSortColumns()
		{
			bool nos, search, resetTopCount = false;
			PXSearchColumn[] columns = prepareSorts(null, null, null, 1, out nos, out search, ref resetTopCount, true);
			KeyValuePair<string, bool>[] ret = new KeyValuePair<string, bool>[columns.Length];
			for (int i = 0; i < columns.Length; i++)
			{
				string name = columns[i].Column;
				if (!String.IsNullOrEmpty(name))
				{
					name = char.ToUpper(columns[i].Column[0]) + columns[i].Column.Substring(1);
				}
				ret[i] = new KeyValuePair<string, bool>(name, columns[i].Descending);
			}
			return ret;
		}

		protected bool HasUnboundSort(PXSearchColumn[] sorts)
		{
			for (int i = 0; i < sorts.Length; i++)
			{
				if (sorts[i].Description == null)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Generates sort columns. Analyze Bql command sort columns, override with external sort order, append primary key.
		/// as side effect can override _Select with OrderByNew() method.
		/// </summary>
		/// <param name="sortcolumns">External sort columns</param>
		/// <param name="needOverrideSort">Output if the Bql command need to be composed with the new sort expression</param>
		/// <returns>Sort columns</returns>
		internal PXSearchColumn[] prepareSorts(string[] sortcolumns, bool[] descendings, object[] searches, int topCount, out bool needOverrideSort, out bool anySearch, ref bool resetTopCount, bool externalCall = false)
		{
			string[] defsorts = null;
			bool identitysort = false;
			if (!externalCall && Cache.BqlTable != null && (!String.IsNullOrEmpty(Cache.Identity) && !Cache.Keys.Contains(Cache.Identity)))
			{
				var primary = PXDatabase.Provider.SchemaCache.GetTableHeader(Cache.BqlTable.Name)?.GetPrimaryKey();
				if (primary != null && primary.Columns.Count == 2 
					&& (String.Equals(primary.Columns[0].Name, DbServices.Model.AcumaticaDb.CompanyId, StringComparison.OrdinalIgnoreCase) && String.Equals(primary.Columns[1].Name, Cache.Identity, StringComparison.OrdinalIgnoreCase)
					|| String.Equals(primary.Columns[1].Name, DbServices.Model.AcumaticaDb.CompanyId, StringComparison.OrdinalIgnoreCase) && String.Equals(primary.Columns[0].Name, Cache.Identity, StringComparison.OrdinalIgnoreCase)))
				{
					defsorts = new string[] { Cache.Identity };
				}
			}
			if (defsorts == null)
			{
				defsorts = Cache.Keys.ToArray();
			}
			else
			{
				identitysort = true;
			}

			object row = null;
			IBqlSortColumn[] selsort = _Select.GetSortColumns();
			bool needUpdateSelect = false;
			anySearch = false;

			bool hasNonFieldSorts = false;

			List<Type> selcols;
			if (selsort.Length == 0)
			{
				needOverrideSort = true;
				if (sortcolumns == null || sortcolumns.Length == 0)
				{
					sortcolumns = defsorts;
					needUpdateSelect = defsorts.Length > 0;
					needOverrideSort = false;
				}
				else if (!CompareIgnoreCase.IsListEndingWithSublist(sortcolumns.ToList(), defsorts.ToList()))
				{
					List<string> cols = new List<string>(sortcolumns);
					foreach (string key in defsorts)
					{
						if (!identitysort || Cache.Keys.Count != 1 || !CompareIgnoreCase.IsInList(cols, Cache.Keys[0]))
						{
							cols.Add(key);
						}
					}
					sortcolumns = cols.ToArray();
					if (searches != null)
					{
						List<object> correctSearches = new List<object>();
						List<string> uniqueSorts = new List<string>();
						int searchIndex = 0, searchesLength = searches.Length;
						for (int i = 0; i < sortcolumns.Length; i++)
						{
							string sort = sortcolumns[i];
							if (!uniqueSorts.Contains(sort))
							{
								correctSearches.Add(searchIndex < searchesLength ? searches[searchIndex++] : null);
								uniqueSorts.Add(sort);
							}
							else
							{
								int firstColIndex = cols.IndexOf(sort);
								correctSearches.Add(correctSearches[firstColIndex]);
							}
						}
						searches = correctSearches.ToArray();
					}
				}
				selcols = new List<Type>(new Type[sortcolumns.Length]);
			}
			else
			{
				List<string> cols;
				List<bool> descs;
				if (sortcolumns != null && sortcolumns.Length > 0)
				{
					cols = new List<string>(sortcolumns);
					if (descendings != null)
					{
						descs = new List<bool>(descendings);
					}
					else
					{
						descs = new List<bool>(sortcolumns.Length);
					}
					selcols = new List<Type>(new Type[sortcolumns.Length]);
					for (int i = descs.Count; i < cols.Count; i++)
					{
						descs.Add(false);
					}
					for (int i = 0; i < selsort.Length; i++)
					{
						Type ct = selsort[i].GetReferencedType();
						if (ct != null && typeof(IBqlField).IsAssignableFrom(ct))
						{
							string field;
							if (!(_Select is IBqlJoinedSelect) || BqlCommand.GetItemType(ct) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(ct)))
							{
								field = ct.Name;
							}
							else
							{
								field = BqlCommand.GetItemType(ct).Name + "__" + ct.Name;
							}
							bool exists = false;
							for (int j = 0; j < cols.Count; j++)
							{
								if (String.Equals(cols[j], field, StringComparison.OrdinalIgnoreCase))
								{
									exists = true;
									if (selcols[j] == null)
									{
										selcols[j] = ct;
									}
									break;
								}
							}
							if (!exists)
							{
								cols.Add(field);
								descs.Add(selsort[i].IsDescending);
								selcols.Add(ct);
							}
						}
						else // Most likely an expression (such as Switch<Case<...>>) that cannot be represented with just (String fieldname, boolean isDesc)
							hasNonFieldSorts = true;
					}
					needOverrideSort = true;
				}
				else
				{
					cols = new List<string>();
					descs = new List<bool>();
					selcols = new List<Type>();
					for (int i = 0; i < selsort.Length; i++)
					{
						Type ct = selsort[i].GetReferencedType();
						if (ct != null && typeof(IBqlField).IsAssignableFrom(ct))
						{
							string field;
							if (!(_Select is IBqlJoinedSelect) || BqlCommand.GetItemType(ct) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(ct)))
							{
								field = ct.Name;
							}
							else
							{
								field = BqlCommand.GetItemType(ct).Name + "__" + ct.Name;
							}
							cols.Add(field);
							descs.Add(selsort[i].IsDescending);
							selcols.Add(ct);
						}
						else // Most likely an expression (such as Switch<Case<...>>) that cannot be represented with just (String fieldname, boolean isDesc)
							hasNonFieldSorts = true;
					}
					needOverrideSort = false;
				}
				
				foreach (string key in defsorts)
				{
					if (!CompareIgnoreCase.IsInList(cols, key) && !CompareIgnoreCase.IsInList(cols, Cache.BqlTable.Name + "__" + key)
						&& (!identitysort || Cache.Keys.Count != 1 || !CompareIgnoreCase.IsInList(cols, Cache.Keys[0]) && !CompareIgnoreCase.IsInList(cols, Cache.BqlTable.Name + "__" + Cache.Keys[0])))
					{
						cols.Add(key);
						selcols.Add(null);
						if (!hasNonFieldSorts) // if there are complex expressions, don't try to rebuild the sort clause from string+bool arrays in PXView.GetResult
							needOverrideSort = true;
					}
				}
				sortcolumns = cols.ToArray();
				descendings = descs.ToArray();
			}

			// Fill PXSearchColumn "sorts" array
			PXSearchColumn[] sorts = new PXSearchColumn[sortcolumns.Length];
			bool anyCorrected = false;
			for (int i = 0; i < sortcolumns.Length; i++)
			{
				object val = searches != null && i < searches.Length ? searches[i] : null;
				bool desc = descendings != null && i < descendings.Length && descendings[i];

				string columnName = sortcolumns[i];

				PXCache cache;
				string field;
				bool corrected = CorrectCacheAndField(sortcolumns[i], out cache, out field);
				anyCorrected |= corrected;

				PXSearchColumn searchColumn = sorts[i] = new PXSearchColumn(columnName, desc, val);
				searchColumn.SelSort = selcols[i];

				try
				{
					if (val != null)
					{
						anySearch = true;
						row = RaiseFieldUpdatingForSearchColumn(cache, searchColumn, topCount, field, row, corrected, ref val);
					}
				}
				catch (Exception)
				{
					searchColumn.UseExt = true;
					if (val is string && searchColumn.SearchValue is string
						&& ((string)val).Length > ((string)searchColumn.SearchValue).Length
						&& ((string)val).StartsWith((string)searchColumn.SearchValue))
					{
						searchColumn.SearchValue = searchColumn.OrigSearchValue = val;
					}
				}

				bool topCountNeedsReset = false;

				try
				{
					RaiseCommandPreparingForSearchColumn(cache, searchColumn, topCount, field, row, out topCountNeedsReset);
				}
				catch (Exception) { }
				if (topCountNeedsReset)
					resetTopCount = true;
			}
			if (needUpdateSelect || anyCorrected)
			{
				List<PXDataValue> pars = new List<PXDataValue>();
				Type neworder = ConstructSort(sorts, pars, false);
				if (pars.Count == 0)
				{
					_NewOrder = neworder;
					_Select = _Select.OrderByNew(neworder);
				}
			}
			return sorts;
		}

		private object RaiseFieldUpdatingForSearchColumn(PXCache cache, PXSearchColumn searchColumn, int topCount, string columnName, object row, bool corrected, ref object val)
		{
			cache.RaiseFieldUpdating(columnName, row, ref val);
			if (val == null)
			{
				searchColumn.UseExt = true;
				return row;
			}

			try
			{
				if (!corrected)
				{
					if (row == null)
					{
						row = Cache.CreateInstance();
					}
					Cache.SetValue(row, columnName, val);
				}
			}
			catch
			{
			}
			if (topCount == 1)
			{
				searchColumn.SearchValue = val;
			}
			else
			{
				PXFieldState state = Cache.GetStateExt(row, columnName) as PXFieldState;
				if (state != null && state.DataType == val.GetType())
				{
					if (searchColumn.Descending && val is string && ((string)val).Length < state.Length)
					{
						searchColumn.SearchValue = val + new string((char)0xF8FF, state.Length - ((string)val).Length);
					}
					else
					{
						searchColumn.SearchValue = val;
					}
				}
				else
				{
					searchColumn.UseExt = true;
				}
			}
			return row;
		}

		private void RaiseCommandPreparingForSearchColumn(PXCache cache, PXSearchColumn searchColumn, int topCount, string columnName, object row, out bool resetTopCount)
		{
			resetTopCount = false;

			PXCommandPreparingEventArgs.FieldDescription descr;
			if (topCount == 1 && searchColumn.SearchValue == null)
			{
				cache.RaiseCommandPreparing(columnName, null, null, PXDBOperation.Select, cache.GetItemType(), out descr);
			}
			else
			{
				cache.RaiseCommandPreparing(columnName, null, searchColumn.SearchValue, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
				if (descr != null && descr.Expr != null)
				{
					PXCommandPreparingEventArgs.FieldDescription emptydescr;
					cache.RaiseCommandPreparing(columnName, null, null, PXDBOperation.Select | PXDBOperation.External, Cache.GetItemType(), out emptydescr);
					if (emptydescr == null || emptydescr.Expr == null)
					{
						descr = null;
					}
				}
			}
			if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
			{
				searchColumn.Description = descr;
				// TODO: This logic was saved to keep everything work. It's better to find some other approach to distinct String List and Selector from Calculated columns
				// In case of String List, expression Switch won't be surrounded with brackets.
				// Calculated columns with Switch will have brackets and shouldn't pass this "if".
				if (descr.Expr is SQLTree.SubQuery
					|| searchColumn.SearchValue == null && (descr.Expr is SQLTree.SQLSwitch) && !descr.Expr.IsEmbraced()
					)
				{
					searchColumn.UseExt = true;
					searchColumn.SearchValue = searchColumn.OrigSearchValue;
				}
			}
			else
			{
				PXFieldState state = cache.GetStateExt(row, searchColumn.Column) as PXFieldState;
				if (state == null || searchColumn.OrigSearchValue == null || state.DataType == searchColumn.OrigSearchValue.GetType())
				{
					searchColumn.SearchValue = searchColumn.OrigSearchValue;
				}
				searchColumn.UseExt = true;
				//sorts[i].SearchValue = sorts[i].OrigSearchValue;
				resetTopCount = true;
			}
		}

		/// <summary>Prepares parameters, formats input values, gets default values for the hidden and not supplied parameters. The method returns the values that will replace the
		/// parameters including and the parameters of the custom selection method if it is defined.</summary>
		/// <param name="currents">The objects to use as current data records when
        /// processing <tt>Current</tt> and <tt>Optional</tt> parameters.</param>
		/// <param name="parameters">The explicit values for such parameters as
        /// <tt>Required</tt>, <tt>Optional</tt>, and <tt>Argument</tt>.</param>
		public object[] PrepareParameters(object[] currents, object[] parameters)
		{
			IBqlParameter[] selpars = _Select.GetParameters();
			return PrepareParametersInternal(currents, parameters, selpars);
		}

		protected Type _PrimaryTableType;
		protected virtual PXCache _GetCacheCheckTail(Type ft, out Type ct)
		{
			ct = BqlCommand.GetItemType(ft);
			if (_PrimaryTableType != null && _PrimaryTableType.IsSubclassOf(ct))
			{
				ct = _PrimaryTableType;
			}
			return _Graph.Caches[ct];
		}
		internal object[] PrepareParametersInternal(object[] currents, object[] parameters, IBqlParameter[] selpars)
		{
			List<object> list = new List<object>();

			int pcnt = 0;
			int acnt = 0;
			ParameterInfo[] args = null;
			for (int i = 0; i < selpars.Length; i++)
			{
				object val = null;
				if ((selpars[i].IsVisible || selpars[i] is FieldNameParam) && parameters != null && pcnt < parameters.Length)
				{
					val = parameters[pcnt];
					pcnt++;
				}
				if (val == null)
				{
					if (selpars[i].HasDefault)
					{
						Type ft = selpars[i].GetReferencedType();
						if (ft.IsNested)
						{
							Type ct;
							PXCache cache = _GetCacheCheckTail(ft, out ct);
							bool currfound = false;
							if (currents != null)
							{
								for (int k = 0; k < currents.Length; k++)
								{
									if (currents[k] != null && (currents[k].GetType() == ct || currents[k].GetType().IsSubclassOf(ct)))
									{
										val = cache.GetValue(currents[k], ft.Name);
										currfound = true;
										break;
									}
								}
							}
							if (!currfound && val == null && (cache._Current ?? cache.Current) != null)
							{
								val = cache.GetValue((cache._Current ?? cache.Current), ft.Name);
							}
							if (val == null && selpars[i].TryDefault)
							{
								if (cache.RaiseFieldDefaulting(ft.Name, null, out val))
								{
									cache.RaiseFieldUpdating(ft.Name, null, ref val);
								}
							}
							if (selpars[i].MaskedType != null && !selpars[i].IsArgument)
							{
								object row = (cache._Current ?? cache.Current);
								if (currents != null)
								{
									for (int k = 0; k < currents.Length; k++)
									{
										if (currents[k] != null && (currents[k].GetType() == ct || currents[k].GetType().IsSubclassOf(ct)))
										{
											row = currents[k];
											break;
										}
									}
								}
								val = GroupHelper.GetReferencedValue(cache, row, ft.Name, val, _Graph._ForceUnattended);
							}
						}
					}
					else if (selpars[i].IsArgument && _Delegate != null)
					{
						if (args == null)
						{
							args = _Delegate.Method.GetParameters();
						}
						if (acnt < args.Length)
						{
							object[] attributes = args[acnt].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false);
							foreach (PXEventSubscriberAttribute attr in attributes)
							{
								List<IPXFieldDefaultingSubscriber> del = new List<IPXFieldDefaultingSubscriber>();
								attr.GetSubscriber<IPXFieldDefaultingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldDefaultingEventArgs defs = new PXFieldDefaultingEventArgs(null);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldDefaulting(Cache, defs);
									}
									val = defs.NewValue;
									break;
								}
							}
							foreach (PXEventSubscriberAttribute attr in attributes)
							{
								List<IPXFieldUpdatingSubscriber> del = new List<IPXFieldUpdatingSubscriber>();
								attr.GetSubscriber<IPXFieldUpdatingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldUpdatingEventArgs upds = new PXFieldUpdatingEventArgs(null, val);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldUpdating(Cache, upds);
									}
									val = upds.NewValue;
								}
							}
						}
						acnt++;
					}
					else if (selpars[i].IsContext)
					{
						string contextVal;
						Type ft = selpars[i].GetReferencedType();
						if (ft.IsNested)
						{
							Type ct;
							PXCache cache = _GetCacheCheckTail(ft, out ct);
							if (_Graph.contextValues != null && _Graph.contextValues.TryGetValue(ft.FullName, out contextVal))
							{
								val = contextVal;
								cache.RaiseFieldUpdating(ft.Name, null, ref val);
							}
						}
					}
				}
				else
				{
					if (selpars[i].HasDefault)
					{
						Type ft = selpars[i].GetReferencedType();
						if (ft.IsNested)
						{
							Type ct;
							PXCache cache = _GetCacheCheckTail(ft, out ct);
							object row = (cache._Current ?? cache.Current);
							if (currents != null)
							{
								for (int k = 0; k < currents.Length; k++)
								{
									if (currents[k] != null && (currents[k].GetType() == ct || currents[k].GetType().IsSubclassOf(ct)))
									{
										row = currents[k];
										break;
									}
								}
							}
							using (new PXReadDeletedScope())
								cache.RaiseFieldUpdating(ft.Name, row, ref val);
							if (selpars[i].MaskedType != null && !selpars[i].IsArgument)
							{
								val = GroupHelper.GetReferencedValue(cache, row, ft.Name, val, _Graph._ForceUnattended);
							}
						}
					}
					else if (selpars[i].IsArgument && _Delegate != null)
					{
						if (args == null)
						{
							args = _Delegate.Method.GetParameters();
						}
						if (acnt < args.Length)
						{
							foreach (PXEventSubscriberAttribute attr in args[acnt].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false))
							{
								List<IPXFieldUpdatingSubscriber> del = new List<IPXFieldUpdatingSubscriber>();
								attr.GetSubscriber<IPXFieldUpdatingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldUpdatingEventArgs upds = new PXFieldUpdatingEventArgs(null, val);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldUpdating(Cache, upds);
									}
									val = upds.NewValue;
								}
							}
						}
						acnt++;
					}
				}
				list.Add(val);
			}
			if (parameters != null)
			{
				for (; pcnt < parameters.Length; pcnt++)
				{
					object val = parameters[pcnt];
					if (_Delegate != null)
					{
						if (args == null)
						{
							args = _Delegate.Method.GetParameters();
						}
						if (acnt < args.Length)
						{
							foreach (PXEventSubscriberAttribute attr in args[acnt].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false))
							{
								List<IPXFieldUpdatingSubscriber> del = new List<IPXFieldUpdatingSubscriber>();
								attr.GetSubscriber<IPXFieldUpdatingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldUpdatingEventArgs upds = new PXFieldUpdatingEventArgs(null, val);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldUpdating(Cache, upds);
									}
									val = upds.NewValue;
								}
							}
						}
						acnt++;
					}
					list.Add(val);
				}
			}
			return list.ToArray();
		}

		private bool CorrectCacheAndField(string originField, out PXCache cache, out string field)
		{
			cache = Cache;
			return CorrectCacheAndField(_Graph, _Select, originField, ref cache, out field);
		}

		private static bool CorrectCacheAndField(PXGraph graph, BqlCommand bqlCommand, string originField, ref PXCache cache, out string field)
        {
			field = originField;

			bool corrected = false;
			int idx = originField.IndexOf("__", StringComparison.Ordinal);

			if (idx > -1)
			{
				string fieldName = originField.Substring(idx + 2);
                Type[] tables = bqlCommand.GetTables();
				string tableName = originField.Substring(0, idx);
				PXCache joinedTableCache = tables
										   .Where(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                                           .Select(t => graph.Caches[t]).FirstOrDefault();
				if (joinedTableCache != null)
				{
					cache = joinedTableCache;
					field = fieldName;
					corrected = true;
				}
			}

			return corrected;
		}

		private object GetStateFromCorrectCache(string dataField)
		{
	        return GetStateFromCorrectCache(Graph, _Select, Cache, dataField);
        }

        private static object GetStateFromCorrectCache(PXGraph graph, BqlCommand bqlCommand, PXCache cache, string dataField)
        {
	        PXCache correctedCache = cache;
			string correctedField;
	        CorrectCacheAndField(graph, bqlCommand, dataField, ref correctedCache, out correctedField);
			return correctedCache.GetStateExt(null, correctedField);
		}

		private IEnumerable<PXEventSubscriberAttribute> GetAttributesFromCorrectCache(string dataField)
		{
	        return GetAttributesFromCorrectCache(Graph, _Select, Cache, dataField);
        }

		private static IEnumerable<PXEventSubscriberAttribute> GetAttributesFromCorrectCache(PXGraph graph, BqlCommand bqlCommand, PXCache cache, string dataField)
		{
			PXCache correctedCache = cache;
			string correctedField;
			CorrectCacheAndField(graph, bqlCommand, dataField, ref correctedCache, out correctedField);
			return correctedCache.GetAttributesReadonly(correctedField, true);
		}

		protected internal bool prepareFilters(ref PXFilterRow[] filters, string forceView = null)
		{
			return prepareFilters(Graph, Cache, _Select, ref filters, forceView);
		}

		internal static bool prepareFilters(PXGraph graph, PXCache originalCache, BqlCommand bqlCommand, ref PXFilterRow[] filters, string forceView = null)
		{
			bool anyFailed = false;
			if (filters != null)
			{
				PXCache cache = originalCache;
				int brackets = 0;

				var filtersList = filters.ToList();
				bool filtersChanged = false;
				foreach (var f in filters)
				{
					var state = GetStateFromCorrectCache(graph, bqlCommand, originalCache, f.DataField) as PXFieldState;
					if (state != null)
					{
						var strState = state as PXStringState;
						if (strState != null && strState.MultiSelect && f.Value != null)
						{
							string nullValue = "<null>";
							var values = f.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
							if (f.Condition == PXCondition.LIKE)
							{
								int startPostion, pos;
								startPostion = pos = filtersList.IndexOf(f);
								filtersList.Remove(f);

								filtersChanged = true;
								f.UseExt = true;

								foreach (var v in values)
								{
									var newFilter = f.Clone() as PXFilterRow;
									newFilter.Value = v;
									newFilter.OrOperator = true;
									newFilter.Condition = string.Equals(v, nullValue, StringComparison.OrdinalIgnoreCase) ? 
										PXCondition.ISNULL : PXCondition.LIKE;
									newFilter.OpenBrackets = 0;
									newFilter.CloseBrackets = 0;
									filtersList.Insert(pos, newFilter);
									++pos;
								}
								var last = filtersList.ElementAt(pos - 1);
								var first = filtersList.ElementAt(startPostion);
								first.OpenBrackets = f.OpenBrackets + 1;
								last.OrOperator = f.OrOperator;
								last.CloseBrackets = f.CloseBrackets + 1;
							}
						}

						var dateState = state as PXDateState;
						if (dateState != null)
						{
							var dtAttr = (PXDBDateAttribute) GetAttributesFromCorrectCache(graph, bqlCommand, originalCache, f.DataField)
								.FirstOrDefault(a => a is PXDBDateAttribute);
							if (dtAttr != null && dtAttr.PreserveTime)
							{
								DateTime? dtValue = DateTime.TryParse(f.Value as string, out var val) ? val : f.Value as DateTime?;
								DateTime? dtValue2 = DateTime.TryParse(f.Value2 as string, out val) ? val : f.Value2 as DateTime?;
								if (dtValue != null && dtValue.Value.TimeOfDay == TimeSpan.Zero)
								{
									switch (f.Condition)
									{
										case PXCondition.EQ:
											int ePos = filtersList.IndexOf(f);
											var eqNewFilter = (PXFilterRow) f.Clone();
											eqNewFilter.Condition = PXCondition.BETWEEN;
											eqNewFilter.Value2 = dtValue.Value.AddDays(1).AddTicks(-1);
											filtersList.Remove(f);
											filtersList.Insert(ePos, eqNewFilter);
											filtersChanged = true;
											break;
										case PXCondition.BETWEEN:
											if (dtValue2 != null && dtValue2.Value.TimeOfDay == TimeSpan.Zero)
											{
												int bPos = filtersList.IndexOf(f);
												var bNewFilter = (PXFilterRow) f.Clone();
												bNewFilter.Value2 = dtValue2.Value.AddDays(1).AddTicks(-1);
												filtersList.Remove(f);
												filtersList.Insert(bPos, bNewFilter);
												filtersChanged = true;
											}
											break;
										case PXCondition.NE:
											int nePos = filtersList.IndexOf(f);
											var neNewFilterLt = (PXFilterRow) f.Clone();
											var neNewFilterGt = (PXFilterRow) f.Clone();
											neNewFilterLt.OrOperator = false;
											neNewFilterLt.Condition = PXCondition.LT;
											neNewFilterLt.CloseBrackets = 0;
											neNewFilterGt.Condition = PXCondition.GT;
											neNewFilterGt.OpenBrackets = 0;
											neNewFilterGt.Value = dtValue.Value.AddDays(1).AddTicks(-1);
											filtersList.Remove(f);
											filtersList.Insert(nePos, neNewFilterGt);
											filtersList.Insert(nePos, neNewFilterLt);
											filtersChanged = true;
											break;
									}
								}
							}
						}
					}
				}
				if (filtersChanged)
					filters = filtersList.ToArray();

				foreach (PXFilterRow fr in filters)
				{
					if (string.IsNullOrEmpty(fr.DataField)) continue;
					fr.OrigValue = fr.Value;
					fr.OrigValue2 = fr.Value2;
					if (brackets > 0)
					{
						brackets += fr.OpenBrackets;
						brackets -= fr.CloseBrackets;
						if (cache == originalCache)
						{
							fr.Value = null;
							fr.Value2 = null;
							continue;
						}
					}
					else
					{
						cache = forceView == null ? originalCache : graph.Views[forceView].Cache;
					}

					string fieldName = fr.DataField;
                    PXCache correctedCache = originalCache;
					string correctedField;
                    bool corrected = CorrectCacheAndField(graph, bqlCommand, fr.DataField, ref correctedCache, out correctedField);

					if (corrected)
					{
						cache = correctedCache;
						fieldName = correctedField;
					}

					if (fr.Condition == PXCondition.EQ && fr.Value is bool && !((bool)fr.Value))
					{
						fr.Condition = PXCondition.NE;
						fr.OrigValue = fr.Value = true;
					}
					if (fr.Value is string
							&& (fr.Condition == PXCondition.EQ && string.Equals((string)fr.Value, "False", StringComparison.OrdinalIgnoreCase)
							|| fr.Condition == PXCondition.NE && string.Equals((string)fr.Value, "True", StringComparison.OrdinalIgnoreCase)))
					{
						PXFieldState fs = cache.GetStateExt(null, fieldName) as PXFieldState;
						if (fs != null && fs.DataType == typeof(bool))
						{
							fr.Condition = PXCondition.NE;
							fr.OrigValue = fr.Value = true;
						}
					}
					if (fr.Value == null
						&& (fr.Condition == PXCondition.EQ
						|| fr.Condition == PXCondition.LIKE
						|| fr.Condition == PXCondition.LLIKE
						|| fr.Condition == PXCondition.RLIKE))
					{
						fr.Condition = PXCondition.ISNULL;
					}
					if (fr.Value == null
						&& (fr.Condition == PXCondition.NE
						|| fr.Condition == PXCondition.NOTLIKE))
					{
						fr.Condition = PXCondition.ISNOTNULL;
					}
					PXCommandPreparingEventArgs.FieldDescription descr;
					if (fr.Variable == null)
					{
						fr.Variable = FilterVariable.GetVariableType(fr.Value as string);
					}
					if (fr.Value is string && FilterVariable.GetConditionViolationMessage(fr.Value as string, fr.Condition) != null)
					{
						continue;
					}

					if (fr.Variable != null)
						fr.DataField = fieldName = fr.DataField.RemoveFromEnd("_description", StringComparison.OrdinalIgnoreCase);

					if (RelativeDatesManager.IsRelativeDatesString(fr.Value as string))
					{
						fr.Value = RelativeDatesManager.EvaluateAsDateTime(fr.Value as string);
					}
					if (RelativeDatesManager.IsRelativeDatesString(fr.Value2 as string))
					{
						fr.Value2 = RelativeDatesManager.EvaluateAsDateTime(fr.Value2 as string);
					}

					switch (fr.Condition)
					{
						case PXCondition.NestedSelector:
							var nestedFilters = new[] { (PXFilterRow)fr.Value2 };
							prepareFilters(graph, originalCache, bqlCommand, ref nestedFilters, (string)fr.Value);
							break;
						case PXCondition.IN:
						case PXCondition.NI:
							if (fr.Value is Type)
							{
								fr.Value = ((Type)fr.Value).FullName;
							}
							if (cache != originalCache && fieldName == fr.DataField)
							{
								cache = originalCache;
								fr.Value = null;
								fr.Value2 = null;
							}
							else if (fr.Value is string)
							{
								if (fr.Variable == FilterVariableType.CurrentUserGroups || fr.Variable == FilterVariableType.CurrentUserGroupsTree)
								{
									Type bqlField = cache.BqlFields.FirstOrDefault(f => f.Name.Equals(fr.DataField, StringComparison.InvariantCultureIgnoreCase));
									if (bqlField != null && cache.GetFieldType(bqlField.Name) == typeof(int))
									{
										object ids = fr.Variable == FilterVariableType.CurrentUserGroups
											? UserGroupLazyCache.Current.GetUserGroupIds(PXAccess.GetUserID()).ToArray()
											: UserGroupLazyCache.Current.GetUserWorkTreeIds(PXAccess.GetUserID()).ToArray();
										cache.RaiseCommandPreparing(fr.DataField, null, ids, PXDBOperation.Select, cache.GetItemType(), out descr);
										fr.Description = descr;
									}
									else
									{
										anyFailed = true;
										if (fr.DataField.Contains('_'))
										{
											fr.UseExt = true;
										}
									}
								}
								else if (fr.Variable == null)
								{
									Type field = System.Web.Compilation.PXBuildManager.GetType((string)fr.Value, false);
									if (field != null && typeof(IBqlField).IsAssignableFrom(field)
										&& field.IsNested && typeof(IBqlTable).IsAssignableFrom(BqlCommand.GetItemType(field)))
									{
										fr.Value = field;
										cache = graph.Caches[BqlCommand.GetItemType(field)];
									}
									else
									{
										fr.Value = null;
										fr.Value2 = null;
									}
									brackets = fr.OpenBrackets - fr.CloseBrackets;
								}
							}
							else
							{
								cache = originalCache;
								fr.Value = null;
								fr.Value2 = null;
								brackets = fr.OpenBrackets - fr.CloseBrackets;
							}
							break;
						case PXCondition.ER:
							fr.DataField = typeof(Note.noteID).Name;
							PXCache noteCache = (new PXGraph()).Caches[typeof(Note)];
							noteCache.RaiseCommandPreparing(typeof(Note.externalKey).Name, null, fr.Value, PXDBOperation.Select, typeof(Note), out descr);
							if (descr != null)
							{
								fr.Description = descr;
							}
							break;
						case PXCondition.EQ:
						case PXCondition.NE:
							PXDBOperation operation = PXDBOperation.Select;
							if (PXDBLocalizableStringAttribute.IsEnabled && cache.GetAttributes(fieldName).Any(_ => _ is PXDBLocalizableStringAttribute)
								|| cache._ReportGetFirstKeyValueStored(fieldName) != null
								|| cache._ReportGetFirstKeyValueAttribute(fieldName) != null)
							{
								operation |= PXDBOperation.External;
								fr.UseExt = true;
							}

							if (fr.Variable == FilterVariableType.CurrentUser && !corrected)
							{
								PXCommandPreparingEventArgs.FieldDescription fdescr;
								cache.RaiseCommandPreparing(fieldName, null, null, operation, cache.GetItemType(), out fdescr);
								if (fdescr == null || fdescr.Expr == null || fdescr.DataType != PXDbType.UniqueIdentifier)
									continue;
								fr.Value = PXAccess.GetUserID();

								PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
								if (state != null && state.DataType != fr.Value.GetType())
								{
									object value = fr.Value;
									cache.RaiseFieldSelecting(fieldName, null, ref value, false);
									value = PXFieldState.UnwrapValue(value);
									fr.UseExt = fr.UseExt | value != null && value != fr.Value;
									fr.Value = value ?? PXAccess.GetUserID();
								}
							}
							if (fr.Value != null)
							{
								object valBeforeUpdating = fr.Value;
								bool updatingFailed = false;
								try
								{
									object val = fr.Value;
									cache.RaiseFieldUpdating(fieldName, null, ref val);
									if(val != null)
										fr.Value = val;
								}
								catch (Exception esp)
								{
									fr.UseExt = true;
									updatingFailed = true;
									if (esp is PXSetPropertyException && ((PXSetPropertyException)esp).ErrorValue is string && fr.Value is string)
									{
										fr.Value = ((PXSetPropertyException)esp).ErrorValue;
									}
								}
								if (fr.Value != null)
								{
									try
									{
										descr = null;
										if (!updatingFailed)
										{
											cache.RaiseCommandPreparing(fieldName, null, fr.Value, operation, cache.GetItemType(), out descr);
										}
										if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
										{
											fr.Description = descr;
											fr.UseExt = fr.UseExt || descr.Expr is SQLTree.SubQuery
														|| cache.GetBqlField(fr.DataField) == null && !(descr.Expr is SQLTree.Column);
										}
										else
										{
											cache.RaiseCommandPreparing(fieldName, null, fr.Value, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
											if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL
												&& (!updatingFailed || descr.Expr is SQLTree.SubQuery 
																	|| (cache.GetBqlField(fr.DataField) == null && !(descr.Expr is SQLTree.Column))))
											{
												fr.Description = descr;
												fr.UseExt = true;
											}
											else
											{
												anyFailed = true;
												fr.UseExt = fr.UseExt || (fieldName).IndexOf("_", StringComparison.Ordinal) != -1 || originalCache.GetBqlField(fr.DataField) == null;
											}
										}
									}
									catch
									{
										anyFailed = true;
									}
									if (fr.Variable == FilterVariableType.CurrentUser)
									{
										fr.Value = valBeforeUpdating;
									}
								}
							}
							break;
						case PXCondition.GT:
						case PXCondition.GE:
						case PXCondition.LT:
						case PXCondition.LE:
							if (fr.Value != null)
							{
								object val = fr.Value;
								try
								{
									try
									{
										cache.RaiseFieldUpdating(fieldName, null, ref val);
									}
									catch
									{
										fr.Value = val;
										throw;
									}
									if (val != null)
									{
										PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
										if (state != null && state.DataType == val.GetType())
										{
											fr.Value = val;
										}
										else
										{
											fr.UseExt = true;
											if (state != null)
											{
												cache.RaiseFieldSelecting(fieldName, null, ref val, false);
												if (val is PXFieldState)
												{
													val = ((PXFieldState)val).Value;
												}
												if (val != null && val.GetType() == state.DataType)
												{
													fr.Value = val;
												}
											}

										}
									}
									else
									{
										fr.UseExt = true;
									}
								}
								catch
								{
									fr.UseExt = true;
								}
								try
								{
									cache.RaiseCommandPreparing(fieldName, null, fr.Value, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
									if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
									{
										fr.Description = descr;
										if (!fr.UseExt && (descr.Expr is SQLTree.SubQuery
											|| cache.GetBqlField(fr.DataField) == null && !(descr.Expr is SQLTree.Column)))
										{
											fr.UseExt = true;
										}
									}
									else
									{
										fr.UseExt = true;
										anyFailed = true;
									}
								}
								catch
								{
									anyFailed = true;
								}
							}
							break;
						case PXCondition.LIKE:
						case PXCondition.NOTLIKE:
						case PXCondition.LLIKE:
						case PXCondition.RLIKE:
							if (fr.Value is string s)
							{
								try
								{
									object val;
									var state = cache.GetStateExt(null, fieldName) as PXFieldState;
									var strState = state as PXStringState;
									if (strState != null && !String.IsNullOrEmpty(strState.InputMask) && PX.Common.Mask.IsMasked(s, strState.InputMask, true))
									{
										val = PX.Common.Mask.Parse(strState.InputMask, s);
									}
									else
									{
										val = s;
									}
									cache.RaiseFieldUpdating(fieldName, null, ref val);
									if (val is string)
									{
										if (!(strState != null && strState.Length > 0 && s.Length > strState.Length && ((string)val).Length < s.Length && s.StartsWith((string)val)))
										{
											s = (string)val;
										}
									}
									else //if (state.DataType == typeof (string) || state.DataType == typeof (int) || state.DataType == typeof (long))
									{
										fr.UseExt = true;
									}
								}
								catch
								{
									fr.UseExt = true;
								}
								s = s.TrimEnd();
								fr.Value = s;
								if (fr.Condition == PXCondition.RLIKE)
								{
									s = s.Replace("[", "[[]") + "%";
								}
								else if (fr.Condition == PXCondition.LLIKE)
								{
									s = "%" + s.Replace("[", "[[]");
								}
								else
								{
									s = "%" + s.Replace("[", "[[]") + "%";
								}
								try
								{
									cache.RaiseCommandPreparing(fieldName, null, s, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
									if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
									{
										if (descr.DataLength != null)
										{
											if (fr.Condition == PXCondition.LIKE || fr.Condition == PXCondition.NOTLIKE)
											{
												fr.Description = new PXCommandPreparingEventArgs.FieldDescription(descr.BqlTable, descr.Expr, descr.DataType, ((int)descr.DataLength) + 2, descr.DataValue, descr.IsRestriction);
											}
											else
											{
												fr.Description = new PXCommandPreparingEventArgs.FieldDescription(descr.BqlTable, descr.Expr, descr.DataType, ((int)descr.DataLength) + 1, descr.DataValue, descr.IsRestriction);
											}
										}
										else
										{
											fr.Description = descr;
										}
										if (!fr.UseExt && descr.Expr is SQLTree.SubQuery)
										{
											fr.UseExt = true;
										}
									}
									else
									{
										anyFailed = true;
										fr.UseExt = fr.UseExt || (fieldName).IndexOf("_") != -1;
									}
								}
								catch
								{
									fr.UseExt = true;
									anyFailed = true;
								}
							}
							else
							{
								fr.Value = null;
							}
							break;
						case PXCondition.ISNULL:
						case PXCondition.ISNOTNULL:
							try
							{
								cache.RaiseCommandPreparing(fieldName, null, null, PXDBOperation.Select, cache.GetItemType(), out descr);
								
								if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
								{
									if (CanApplyNullIf(descr.Expr))
									{
										descr.Expr = descr.Expr.NullIf(new SQLTree.SQLConst(string.Empty));
									}

									fr.Description = descr;
									if (!fr.UseExt && descr.Expr is SQLTree.SubQuery)
									{
										fr.UseExt = true;
									}
								}
								else
								{
									cache.RaiseCommandPreparing(fieldName, null, null, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
									if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL
										&& (descr.Expr is SQLTree.SubQuery || cache.GetBqlField(fr.DataField) == null && !(descr.Expr is SQLTree.Column)))
									{
										if (CanApplyNullIf(descr.Expr))
										{
											descr.Expr = descr.Expr.NullIf(new SQLTree.SQLConst(string.Empty));
										}

										fr.Description = descr;
										fr.UseExt = true;
									}
									else
									{
										anyFailed = true;
										fr.UseExt = fr.UseExt || (fieldName).IndexOf("_") != -1;
									}
								}

								bool CanApplyNullIf(SQLTree.SQLExpression v)
								{
									switch (v.GetDBType())
									{
										case PXDbType.NChar:
										case PXDbType.NText:
										case PXDbType.NVarChar:
										case PXDbType.Text:
										case PXDbType.VarChar:
										case PXDbType.Variant:
										case PXDbType.Xml:
											return true;
										default:
											return false;
									}
								}
							}
							catch
							{
								anyFailed = true;
							}
							break;
						case PXCondition.TODAY:
						case PXCondition.TODAY_OVERDUE:
						case PXCondition.OVERDUE:
						case PXCondition.TOMMOROW:	
						case PXCondition.THIS_MONTH:
						case PXCondition.THIS_WEEK:
						case PXCondition.NEXT_WEEK:
						case PXCondition.NEXT_MONTH:
						case PXCondition.BETWEEN:
							if (fr.Condition != PXCondition.BETWEEN)
							{
								DateTime minValue = new DateTime(1900, 1, 1);
								fr.Value = minValue;
								fr.Value2 = minValue;
								object value = PXView.DateTimeFactory.GetDateRange(fr.Condition, graph.Accessinfo.BusinessDate);
								if ((value as DateTime?[])[0] != null)
									fr.Value = (value as DateTime?[])[0].Value;
								if ((value as DateTime?[])[1] != null)
									fr.Value2 = (value as DateTime?[])[1].Value;
							}
							if (fr.Value != null && fr.Value2 != null)
							{
								object val = fr.Value;
								object val2 = fr.Value2;
								try
								{
									Exception ex1 = null;
									try
									{
										cache.RaiseFieldUpdating(fieldName, null, ref val);
									}
									catch (Exception ex)
									{
										fr.Value = val;
										ex1 = ex;
									}

									try
									{
										cache.RaiseFieldUpdating(fieldName, null, ref val2);
									}
									catch
									{
										fr.Value2 = val2;
										throw;
									}
									if (ex1 != null) throw ex1;

									if (val != null && val2 != null)
									{
										PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
										if (state != null && state.DataType == val.GetType() && state.DataType == val2.GetType())
										{
											fr.Value = val;
											fr.Value2 = val2;
										}
										else
										{
											fr.UseExt = true;
											if (state != null)
											{
												cache.RaiseFieldSelecting(fieldName, null, ref val, false);
												if (val is PXFieldState)
												{
													val = ((PXFieldState)val).Value;
												}
												if (val != null && val.GetType() == state.DataType)
												{
													fr.Value = val;
												}
												cache.RaiseFieldSelecting(fieldName, null, ref val2, false);
												if (val2 is PXFieldState)
												{
													val2 = ((PXFieldState)val2).Value;
												}
												if (val2 != null && val2.GetType() == state.DataType)
												{
													fr.Value2 = val2;
												}
											}
										}
									}
									else
									{
										fr.UseExt = true;
									}
								}
								catch
								{
									fr.UseExt = true;
								}
								try
								{
									PXCommandPreparingEventArgs.FieldDescription descr2;
									cache.RaiseCommandPreparing(fieldName, null, fr.Value2, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr2);
									if (descr2 != null && descr2.Expr != null && descr2.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
									{
										cache.RaiseCommandPreparing(fieldName, null, fr.Value, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
										if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
										{
											fr.Description = descr;
											fr.Description2 = descr2;
											fr.UseExt = fr.UseExt | 
												(descr.Expr is SQLTree.SubQuery|| cache.GetBqlField(fr.DataField) == null && !(descr.Expr is SQLTree.Column));
										}
										else
										{
											anyFailed = true;
											fr.UseExt = fr.UseExt || (fieldName).IndexOf("_") != -1;
										}
									}
									else
									{
										fr.UseExt = true;
										anyFailed = true;
									}
								}
								catch
								{
									anyFailed = true;
								}
							}
							break;
					}
					if (fr.Condition != PXCondition.NestedSelector)
					{
					if (fr.Description != null)
					{
						cache.RaiseCommandPreparing(fieldName, null, null, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
						if (descr == null || descr.Expr == null)
						{
							fr.Description = null;
							fr.Description2 = null;
							if (!fr.UseExt || fr.Variable == FilterVariableType.CurrentUser)
							{
								object val = fr.Value;
								PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
								if (state != null)
								{
									if (val != null && state.DataType != val.GetType())
									{
										cache.RaiseFieldSelecting(fieldName, null, ref val, false);
										if (val is PXFieldState)
										{
											val = ((PXFieldState)val).Value;
										}
										if (val != null && val.GetType() == state.DataType)
										{
											fr.Value = val;
										}
										val = fr.Value2;
										if (val != null && state.DataType != val.GetType())
										{
											cache.RaiseFieldSelecting(fieldName, null, ref val, false);
											if (val is PXFieldState)
											{
												val = ((PXFieldState)val).Value;
											}
											if (val != null && val.GetType() == state.DataType)
											{
												fr.Value2 = val;
											}
										}
									}
										else if (fr.Variable != FilterVariableType.CurrentUser && fr.OrigValue != null && state.DataType == fr.OrigValue.GetType())
									{
										fr.Value = fr.OrigValue;
										fr.Value2 = fr.OrigValue2;
									}
								}
							}
							fr.UseExt = true;
							anyFailed = true;
						}
					}
					else if (fr.UseExt && fr.OrigValue != null)
					{
						PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
						if (state != null && state.DataType == fr.OrigValue.GetType())
						{
							fr.Value = fr.OrigValue;
							fr.Value2 = fr.OrigValue2;
						}
					}
				}

				// in case of PXProjection we refer to field in subquery
				if (cache.BqlSelect != null && !fr.UseExt)
				{
					if (fr.Description?.Expr != null)
						fr.Description.Expr = new Column(fieldName, cache.GetItemType());
					if (fr.Description2?.Expr != null)
						fr.Description2.Expr = new Column(fieldName, cache.GetItemType());
				}
			}
			}
			return anyFailed;
		}

		private static void appendUserGroups(object dataValue, Type field, bool inCondition, out Type expression, out PXDataValue parameter)
		{
			parameter = null;
			int[] ids = dataValue as int[];
			if (ids != null && ids.Length > 0)
			{
				parameter = new PXDataValue(PXDbType.DirectExpression, dataValue);

				if (inCondition)
				{
					expression = typeof(Where<,>).MakeGenericType(
						field,
						typeof(In<>).MakeGenericType(
							typeof(Required<>).MakeGenericType(
								field
							)
						)
					);
				}
				else
				{
					expression = typeof(Where<,>).MakeGenericType(
						field,
							typeof(NotIn<>).MakeGenericType(
								typeof(Required<>).MakeGenericType(
									field
								)
							)
					);
				}
			}
			else
			{
				expression = typeof(Where<True, Equal<False>>);
			}
		}

		private BqlCommand appendFilters(PXFilterRow[] filters, List<PXDataValue> pars, BqlCommand cmd, string forceView = null)
		{
			var cache = string.IsNullOrEmpty(forceView) ? Cache:Graph.Views[forceView].Cache;
			return AppendFiltersToCommand(cmd, filters, pars, cache, Graph);
		}

		internal static BqlCommand AppendFiltersToCommand(BqlCommand cmd, PXFilterRow[] filters, List<PXDataValue> pars, PXCache cache, PXGraph graph)
		{
			if (filters != null)
			{
				int brackets = 0;
				foreach (PXFilterRow row in filters)
				{
					brackets += row.OpenBrackets;
					brackets -= row.CloseBrackets;
				}
				if (brackets > 0)
				{
					filters[filters.Length - 1].CloseBrackets += brackets;
				}
				else if (brackets < 0)
				{
					filters[0].OpenBrackets -= brackets;
				}
				List<Type> fields = cache.BqlFields;
				Type[] expressions = new Type[filters.Length];
				int exprCnt = 0;
				brackets = 0;
				PXCache subcache = null;
				for (int i = 0; i < filters.Length; i++)
				{
					PXFilterRow fr = filters[i];
					Type field = null;
					if (brackets > 0)
					{
						field = subcache.GetBqlField(fr.DataField);
						brackets += fr.OpenBrackets;
						brackets -= fr.CloseBrackets;
					}
					else
					{
						for (int j = fields.Count - 1; j >= 0; j--)
						{
							Type bf = fields[j];
							if (String.Compare(bf.Name, fr.DataField, StringComparison.OrdinalIgnoreCase) == 0)
							{
								field = bf;
								break;
							}
						}
					}
					Type required = null;
					if (field != null && !fr.UseExt)
					{
						required = typeof (Required<>).MakeGenericType(field);
					}
					else if (fr.Description != null && fr.Description.Expr != null)
					{
						field = typeof (FieldNameParam);
						required = typeof (Argument<object>);
					}
					bool conditionViolation = fr.Value is string &&
											  FilterVariable.GetConditionViolationMessage(fr.Value as string, fr.Condition) !=
											  null;
					if (field != null && !conditionViolation)
					{
						int parspos = pars.Count;
						switch (fr.Condition)
						{
							case PXCondition.NestedSelector:
								expressions[i] = AppendNestedSelectorFilters(field, fr, pars, cmd, cache, graph);
								break;
							case PXCondition.IN:
							case PXCondition.NI:
								if (fr.Value is Type)
								{
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (Equal<>).MakeGenericType((Type)fr.Value));
									subcache = graph.Caches[BqlCommand.GetItemType((Type)fr.Value)];
									brackets = fr.OpenBrackets - fr.CloseBrackets;
								}
								else if ((fr.Variable == FilterVariableType.CurrentUserGroups ||
										  fr.Variable == FilterVariableType.CurrentUserGroupsTree))
								{
									if (fr.Description != null)
									{
										Type expression;
										PXDataValue parameter;
										appendUserGroups(fr.Description.DataValue, field, fr.Condition == PXCondition.IN,
											out expression, out parameter);
										if (expression != null)
										{
											expressions[i] = expression;
										}
										if (parameter != null)
										{
											pars.Add(parameter);
										}
									}
								}
								break;
							case PXCondition.ER:
								if (string.Compare(field.Name, "noteID", StringComparison.InvariantCultureIgnoreCase) == 0 &&
									!string.IsNullOrEmpty(fr.Value as string))
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
									expressions[i] = typeof (Where<,,>).MakeGenericType(
										field,
										typeof (Equal<>).MakeGenericType(
											typeof (Note.noteID)
											),
										typeof (And<,>).MakeGenericType(
											typeof (Note.externalKey),
											typeof (Like<>).MakeGenericType(
												typeof (Required<>).MakeGenericType(
													typeof (Note.externalKey)
													)
												)
											)
										);
								}
								break;
							case PXCondition.EQ:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (Equal<>).MakeGenericType(required));
								}
								break;
							case PXCondition.NE:
								if (fr.Description != null)
								{
									if (fr.OrigValue is bool && ((bool)fr.OrigValue))
									{
										expressions[i] = typeof (Where<,,>).MakeGenericType(field,
											typeof (NotEqual<>).MakeGenericType(required),
											typeof (Or<,>).MakeGenericType(field, typeof (IsNull)));
									}
									else
									{
										expressions[i] = typeof (Where<,>).MakeGenericType(field,
											typeof (NotEqual<>).MakeGenericType(required));
									}
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
								}
								break;
							case PXCondition.GT:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (Greater<>).MakeGenericType(required));
								}
								break;
							case PXCondition.GE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (GreaterEqual<>).MakeGenericType(required));
								}
								break;
							case PXCondition.LT:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (Less<>).MakeGenericType(required));
								}
								break;
							case PXCondition.LE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (LessEqual<>).MakeGenericType(required));
								}
								break;
							case PXCondition.LIKE:
							case PXCondition.LLIKE:
							case PXCondition.RLIKE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType,
										fr.Description.DataLength <= ((string)fr.Description.DataValue).Length
											? fr.Description.DataLength
											: ((string)fr.Description.DataValue).Length, fr.Description.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (Like<>).MakeGenericType(required));
								}
								break;
							case PXCondition.NOTLIKE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType,
										fr.Description.DataLength <= ((string)fr.Description.DataValue).Length
											? fr.Description.DataLength
											: ((string)fr.Description.DataValue).Length, fr.Description.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (NotLike<>).MakeGenericType(required));
								}
								break;
							case PXCondition.ISNULL:
								if (fr.Description != null)
								{
									expressions[i] = typeof (Where<,>).MakeGenericType(field, typeof (IsNull));
								}
								break;
							case PXCondition.ISNOTNULL:
								if (fr.Description != null)
								{
									expressions[i] = typeof (Where<,>).MakeGenericType(field, typeof (IsNotNull));
								}
								break;
							case PXCondition.OVERDUE:
							case PXCondition.TOMMOROW:
							case PXCondition.THIS_WEEK:
							case PXCondition.TODAY:
							case PXCondition.THIS_MONTH:
							case PXCondition.BETWEEN:
								if (fr.Description != null && fr.Description2 != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength,
										fr.Description.DataValue));
									pars.Add(new PXDataValue(fr.Description2.DataType, fr.Description2.DataLength,
										fr.Description2.DataValue));
									expressions[i] = typeof (Where<,>).MakeGenericType(field,
										typeof (Between<,>).MakeGenericType(required, required));
								}
								break;
							case PXCondition.TODAY_OVERDUE:
							case PXCondition.NEXT_WEEK:
							case PXCondition.NEXT_MONTH:
								if (fr.Description != null)
								{
									DateTime?[] range = DateTimeFactory.GetDateRange(fr.Condition, null);
									var startDate = range[0];
									var endDate = range[1];
									Type condition = null;
									if (startDate != null)
									{
										pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, startDate));
										condition = typeof (GreaterEqual<>).MakeGenericType(required);
									}
									if (endDate != null)
									{
										pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, endDate));
										condition = typeof (LessEqual<>).MakeGenericType(required);
									}
									if (startDate != null && endDate != null)
										condition = typeof (Between<,>).MakeGenericType(required, required);
									if (condition != null)
										expressions[i] = typeof (Where<,>).MakeGenericType(field, condition);
								}
								break;
						}
						if (field == typeof (FieldNameParam) && expressions[i] != null)
						{
							SQLExpression fieldExpr = fr.Description.Expr.Duplicate();
							int idxscore;
							if (fieldExpr is Column fieldCol && fieldCol.Table() != null &&
								fieldCol.Name.IndexOf("__") == -1 && // only take columns with simple name into account (Optimized export can pass compound names)
								(idxscore = filters[i].DataField.IndexOf("__")) != -1)
							{
								fieldExpr = new Column(fieldCol.Name, filters[i].DataField.Substring(0, idxscore));
							}
							string fieldName = fieldExpr.SQLQuery(cache.Graph.SqlDialect.GetConnection()).ToString();
							pars.Insert(parspos, new PXFieldName(fieldName, fieldExpr));
							if (fr.Condition == PXCondition.NE && fr.OrigValue is bool && ((bool)fr.OrigValue))
							{
								pars.Add(new PXFieldName(fieldName, fieldExpr));
							}
						}
					}
					if (expressions[i] == null /*&& (fr.OpenBrackets > 0 || fr.CloseBrackets > 0)*/)
					{
						expressions[i] = typeof (Where<True, Equal<True>>);
					}
					if (expressions[i] != null)
					{
						exprCnt++;
					}
				}
				Type filter = null;
				if (exprCnt > 0)
				{
					List<Type> list = new List<Type>();
					List<int> levels = new List<int>();
					levels.Add(list.Count);
					list.Add(typeof (Where<>));
					for (int j = 0; j < filters[0].OpenBrackets - filters[0].CloseBrackets; j++)
					{
						levels.Add(list.Count);
						list.Add(typeof (Where<>));
					}
					for (int i = 0; i < filters.Length; i++)
					{
						if (expressions[i] != null)
						{
							if (i > 0)
							{
								Type clause = list[levels[levels.Count - 1]];
								if (typeof (Where<>).IsAssignableFrom(clause))
								{
									list[levels[levels.Count - 1]] = typeof (Where2<,>);
								}
								else if (typeof (And<>).IsAssignableFrom(clause))
								{
									list[levels[levels.Count - 1]] = typeof (And2<,>);
								}
								else if (typeof (Or<>).IsAssignableFrom(clause))
								{
									list[levels[levels.Count - 1]] = typeof (Or2<,>);
								}
								levels[levels.Count - 1] = list.Count;
								if (filters[i - 1].OrOperator)
								{
									list.Add(typeof (Or<>));
								}
								else
								{
									list.Add(typeof (And<>));
								}
								for (int j = 0; j < filters[i].OpenBrackets - filters[i].CloseBrackets; j++)
								{
									levels.Add(list.Count);
									list.Add(typeof (Where<>));
								}
								for (int j = 0; j < filters[i].CloseBrackets - filters[i].OpenBrackets; j++)
								{
									levels.RemoveAt(levels.Count - 1);
								}
							}
							bool inAllowed = filters[i].Variable == null &&
											 !(filters[i].Value is string &&
											   FilterVariable.GetConditionViolationMessage(filters[i].Value as string,
												   filters[i].Condition) != null);
							if (filters[i].Condition == PXCondition.IN && inAllowed)
							{
								list.Add(typeof (Exists<>));
								list.Add(typeof (Select<,>));
								list.Add(BqlCommand.GetItemType((Type)filters[i].Value));
							}
							else if (filters[i].Condition == PXCondition.NI && inAllowed)
							{
								list.Add(typeof (NotExists<>));
								list.Add(typeof (Select<,>));
								list.Add(BqlCommand.GetItemType((Type)filters[i].Value));
							}
							else if (filters[i].Condition == PXCondition.ER)
							{
								list.Add(typeof (Exists<>));
								list.Add(typeof (Select<,>));
								list.Add(typeof (Note));
							}
							list.Add(expressions[i]);
						}
					}
					filter = BqlCommand.Compose(list.ToArray());
				}
				if (filter != null)
				{
					cmd = cmd.WhereAnd(filter);
				}
			}
			return cmd;
		}

		private static Type AppendNestedSelectorFilters(Type field, PXFilterRow filter, List<PXDataValue> pars,
			BqlCommand cmd, PXCache cache, PXGraph graph)
		{
			var viewname = (string)filter.Value;
			var view = graph.Views[viewname];

			var nestedFilter = (PXFilterRow)filter.Value2;

			// find tables for params and kill all wheres
			var tables = new List<Type>();
			var decomposed = BqlCommand.Decompose(view.BqlSelect.GetSelectType());

			decomposed = BqlCommand.KillAllWheres(decomposed).ToArray();

			for (var i = 0; i < decomposed.Length; i++)
			{
				if (decomposed[i] == typeof(Current<>))
				{
					tables.Add(decomposed[i + 1].DeclaringType);
				}
			}

			var bql = BqlCommand.CreateInstance(BqlCommand.Compose(decomposed));

			// deparametrize view
			bql = tables.Aggregate(bql, BqlCommand.Deparametrize);

			pars.AddRange(view.PrepareParametersInternal(null, null, bql.GetParameters()).Select(o => new PXDataValue(o)));

			// append filters
			var keyField = ((IBqlSearch)view.BqlSelect).GetField();
			bql = AppendFiltersToCommand(bql, new[] { nestedFilter }, pars, view.Cache, graph);

			return typeof(Where<>).MakeGenericType(typeof(Exists<>).MakeGenericType(
				bql.WhereAnd(typeof(Where<,>).MakeGenericType(keyField,
					typeof(Equal<>).MakeGenericType(field))).GetType()
			));
		}

		internal static class DateTimeFactory
		{
			public static double GetDayOfWeek(DayOfWeek day)
			{
				switch (day)
				{
					case DayOfWeek.Monday: return 0D;
					case DayOfWeek.Tuesday: return 1D;
					case DayOfWeek.Wednesday: return 2D;
					case DayOfWeek.Thursday: return 3D;
					case DayOfWeek.Friday: return 4D;
					case DayOfWeek.Saturday: return 5D;
					case DayOfWeek.Sunday: return 6D;
					default: return 0D;
				}
			}

			public static DateTime?[] GetDateRange(PXCondition condition, DateTime? businessDate)
			{
				businessDate = businessDate ?? DateTime.Today;
				DateTime? startDate;
				DateTime? endDate;
				switch (condition)
				{
					case PXCondition.TODAY:
						startDate = businessDate;
						endDate = startDate.Value.AddDays(1D);
						break;
					case PXCondition.OVERDUE:
						startDate = null;
						endDate = businessDate;
						break;
					case PXCondition.TODAY_OVERDUE:
						startDate = null;
						endDate = ((DateTime)businessDate).AddDays(1D);
						break;
					case PXCondition.TOMMOROW:
						startDate = ((DateTime)businessDate).AddDays(1D);
						endDate = startDate.Value.AddDays(1D);
						break;
					case PXCondition.THIS_WEEK:
						startDate = businessDate;
						startDate = startDate.Value.AddDays(-GetDayOfWeek(startDate.Value.DayOfWeek));
						endDate = startDate.Value.AddDays(7D);
						break;
					case PXCondition.NEXT_WEEK:
						startDate = businessDate;
						startDate = startDate.Value.AddDays(7D - GetDayOfWeek(startDate.Value.DayOfWeek));
						endDate = startDate.Value.AddDays(7D);
						break;
					case PXCondition.THIS_MONTH:
						startDate = businessDate;
						startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
						endDate = startDate.Value.AddMonths(1);
						break;
					case PXCondition.NEXT_MONTH:
						startDate = businessDate;
						startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
						startDate = startDate.Value.AddMonths(1);
						endDate = startDate.Value.AddMonths(1);
						break;
					default: return null;
				}
				if (endDate != null) endDate = endDate.Value.AddSeconds(-1D);
				return new[] { startDate, endDate };
			}
		}

		private BqlCommand appendSearches(PXSearchColumn[] sorts, List<PXDataValue> pars, BqlCommand cmd, ref int topCount, bool reverseOrder)
		{
			if (sorts != null && topCount > 0)
			{
				IBqlSortColumn[] sort = cmd.GetSortColumns();

				Type newsearch = null;
				int lastSearch = pars.Count;
				for (int i = sorts.Length - 1; i >= 0; i--)
				{
					if (i >= sort.Length)
					{
						topCount = 0;
						continue;
					}
					if (sorts[i].SearchValue != null && sorts[i].Description != null)
					{
						PXFieldName name = null;
						Type b = null;
						if (sort[i].GetReferencedType() == null)
						{
							for (int j = Cache.BqlFields.Count - 1; j >= 0; j--)
							{
								if (String.Compare(Cache.BqlFields[j].Name, sorts[i].Column, StringComparison.OrdinalIgnoreCase) == 0)
								{
									b = Cache.BqlFields[j];
									break;
								}
							}
							PXCommandPreparingEventArgs.FieldDescription descr = null;
							if (b != null && b.IsNested && CacheType != Cache.GetItemType()
								&& (BqlCommand.GetItemType(b) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(b)))
								&& sorts[i].Description.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
							{
								Cache.RaiseCommandPreparing(sorts[i].Column, null, sorts[i].SearchValue, PXDBOperation.Select | PXDBOperation.External, CacheType, out descr);
							}
							if (descr == null || descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
							{
								descr = sorts[i].Description;
							}
							string fieldName = descr.Expr.SQLQuery(Graph.SqlDialect.GetConnection()).ToString();
							int idxdot, idxscore;
							if ((idxdot = fieldName.IndexOf('.')) != -1 && fieldName.IndexOf('(') == -1 && (idxscore = sorts[i].Column.IndexOf("__")) != -1)
							{
								fieldName = sorts[i].Column.Substring(0, idxscore) + fieldName.Substring(idxdot);
							}
							name = new PXFieldName(fieldName, descr.Expr);
						}
						PXDataValue val = new PXDataValue(sorts[i].Description.DataType, sorts[i].Description.DataLength, sorts[i].Description.DataValue);
						if (lastSearch != pars.Count && (topCount != 1 || reverseOrder))
						{
							pars.Insert(lastSearch, val);
							if (name != null)
							{
								pars.Insert(lastSearch, name);
							}
						}
						pars.Insert(lastSearch, val);
						if (name != null)
						{
							pars.Insert(lastSearch, name);
						}

						Type ft = sort[i].GetReferencedType();
						Type fb = null;
						if (ft == null)
						{
							ft = typeof(FieldNameParam);
							fb = typeof(Required<>).MakeGenericType(b ?? typeof(FieldNameParam.PXRequiredField));
						}
						else
						{
							fb = typeof(Required<>).MakeGenericType(ft);
						}

							if (newsearch == null)
							{
							Type conditionType;
								if (topCount == 1 && !reverseOrder)
								conditionType = typeof(Equal<>);
								else if (sort[i].IsDescending)
								conditionType = reverseOrder ? typeof(Less<>) : typeof(LessEqual<>);
									else
								conditionType = reverseOrder ? typeof(Greater<>) : typeof(GreaterEqual<>);
							newsearch = typeof(Where<,>).MakeGenericType(ft, conditionType.MakeGenericType(fb));
									}
								else
								{
								if (topCount == 1 && !reverseOrder)
								{
									newsearch = typeof(Where2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Equal<>).MakeGenericType(fb)), typeof(And<>).MakeGenericType(newsearch));
								}
								else if (sort[i].IsDescending)
								{
									newsearch = typeof(Where2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Less<>).MakeGenericType(fb)), typeof(Or2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Equal<>).MakeGenericType(fb)), typeof(And<>).MakeGenericType(newsearch)));
								}
								else
								{
									newsearch = typeof(Where2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Greater<>).MakeGenericType(fb)), typeof(Or2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Equal<>).MakeGenericType(fb)), typeof(And<>).MakeGenericType(newsearch)));
								}
							}
						}
					}
				if (newsearch != null)
				{
					cmd = cmd.WhereAnd(newsearch);
				}
			}
			return cmd;
		}

		/// <summary>Retrieves resultset out of the database</summary>
		/// <param name="parameters">Parameters for the command</param>
		/// <param name="searches">Search values</param>
		/// <param name="reverseOrder">If reversing of the sort expression is required</param>
		/// <param name="topCount">Number of rows required</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="needOverrideSort">If the Bql command needs to be updated with the new sort expression</param>
		/// <returns>Resultset, if there is no empty parameters, otherwise empty list</returns>
		protected virtual List<object> GetResult(
			object[] parameters,
			PXFilterRow[] filters,
			bool reverseOrder,
			int topCount,
			PXSearchColumn[] sorts,
			ref bool overrideSort,
			ref bool extFilter
			)
		{
			var context = new PXSelectOperationContext();
			_Select.Context = context;
			var ret = new VersionedList();
			IBqlParameter[] cmdpars = _Select.GetParameters();

			if (parameters == null && cmdpars.Length > 0)
			{

				context.BadParametersQueryNotExecuted = true;
				return ret;
			}
			if (parameters != null)
			{
				if (cmdpars.Length > parameters.Length)
				{
					context.BadParametersQueryNotExecuted = true;
					return ret;
				}
				for (int i = 0; i < parameters.Length && i < cmdpars.Length; i++)
				{
					if (!cmdpars[i].NullAllowed && parameters[i] == null)
					{
						context.BadParametersQueryNotExecuted = true;
						List<KeyValuePair<Type, Type>> parameterPairs = _Select.GetParameterPairs();
						int badParams = 0;

						for (int k = i; k < parameters.Length && k < cmdpars.Length; k++)
						{
							Type field = cmdpars[k].GetReferencedType();
							bool pairFound = false;

							foreach (KeyValuePair<Type, Type> pair in parameterPairs)
							{
								if (pair.Value == field)
								{
									pairFound = true;
									field = pair.Key;
									PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];

									if (!cmdpars[k].NullAllowed && parameters[k] == null)
									{
										if (cache.Keys.Contains(char.ToUpper(field.Name[0]) + field.Name.Substring(1)))
										{
											return ret;
										}
										badParams++;
									}
									break;
								}
							}
							if (!pairFound)
							{
								return ret;
							}
						}
						if (badParams > 0)
							context.BadParametersSkipMergeCache = true;
						return ret;
					}
				}
			}

			BqlCommand cmd = _Select;

			List<PXDataValue> sortnames = new List<PXDataValue>();
			if (overrideSort)
			{
				Type newsort = ConstructSort(sorts, sortnames, reverseOrder);
				if (newsort != null)
				{
					cmd = cmd.OrderByNew(newsort);
				}
				overrideSort = false;
			}

			List<PXDataValue> pars = new List<PXDataValue>();

			ISqlDialect dialect = _Graph.SqlDialect;

			if (parameters != null)
			{
				int argNbr = 0;
				for (int i = 0; i < cmdpars.Length && i < parameters.Length; i++)
				{
					if (cmdpars[i] is FieldNameParam && parameters[i] is PXFieldName)
					{
						pars.Add((PXFieldName)parameters[i]);
						continue;
					}
					PXCommandPreparingEventArgs.FieldDescription descr = null;
					bool IsPassThroughParameter = (_IsReadOnly || cmdpars[i].NullAllowed);

					Type cross = null;
					if (!cmdpars[i].IsArgument)
					{
						Type field = cmdpars[i].GetReferencedType();
						if (!field.IsNested || BqlCommand.GetItemType(field) == CacheType)
						{
							Cache.RaiseCommandPreparing(field.Name, null, parameters[i], PXDBOperation.Select | (IsPassThroughParameter ? PXDBOperation.ReadOnly : PXDBOperation.Normal), null, out descr);
							if (cmdpars[i].MaskedType != null)
							{
								cross = GroupHelper.GetReferencedType(Cache, field.Name);
							}
						}
						else
						{
							PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];
							cache.RaiseCommandPreparing(field.Name, null, parameters[i], PXDBOperation.Select | (IsPassThroughParameter ? PXDBOperation.ReadOnly : PXDBOperation.Normal), null, out descr);
							if (cmdpars[i].MaskedType != null)
							{
								cross = GroupHelper.GetReferencedType(cache, field.Name);
							}
						}
					}
					else if (_Delegate != null)
					{
						foreach (PXEventSubscriberAttribute attr in _Delegate.Method.GetParameters()[argNbr].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false))
						{
							List<IPXCommandPreparingSubscriber> del = new List<IPXCommandPreparingSubscriber>();
							attr.GetSubscriber<IPXCommandPreparingSubscriber>(del);
							if (del.Count > 0)
							{
								PXCommandPreparingEventArgs preps = new PXCommandPreparingEventArgs(null, parameters[i], PXDBOperation.Select, null, dialect);
								for (int l = 0; l < del.Count; l++)
								{
									del[l].CommandPreparing(Cache, preps);
								}
								descr = preps.GetFieldDescription();
								argNbr++;
								break;
							}
						}
					}
					if (descr == null || descr.DataValue == null && !cmdpars[i].NullAllowed)
					{
						PXCommandPreparingEventArgs.FieldDescription innerDescr = IsPassThroughParameter ? descr : null;

						context.BadParametersQueryNotExecuted = true;
						int badParams = 0;

						List<KeyValuePair<Type, Type>> parameterPairs = _Select.GetParameterPairs();

						for (int k = i; k < parameters.Length && k < cmdpars.Length; k++)
						{
							Type field = cmdpars[k].GetReferencedType();
							bool pairFound = false;

							foreach (KeyValuePair<Type, Type> pair in parameterPairs)
							{
								if (pair.Value == field)
								{
									pairFound = true;
									field = pair.Key;
									PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];

									if (innerDescr == null) Cache.RaiseCommandPreparing(field.Name, null, parameters[k], PXDBOperation.Select | PXDBOperation.ReadOnly, null, out innerDescr);
									if (innerDescr == null || innerDescr.DataValue == null && !cmdpars[k].NullAllowed)
									{
										if (cache.Keys.Contains(char.ToUpper(field.Name[0]) + field.Name.Substring(1)))
										{
											return ret;
										}
										badParams++;
									}
									innerDescr = null;
									break;
								}
							}
							if (!pairFound)
							{
								return ret;
							}
						}
						if (badParams > 0)
							context.BadParametersSkipMergeCache = true;

						return ret;
					}
					if (cmdpars[i].MaskedType == null)
					{
						pars.Add(new PXDataValue(descr.DataType, descr.DataLength, descr.DataValue));
					}
					else if (cmdpars[i].MaskedType == typeof(Array))
					{
						pars.Add(new PXDataValue(PXDbType.DirectExpression, descr.DataValue));
					}
					else if (_Graph.Caches[cmdpars[i].MaskedType].Fields.Contains(GroupHelper.FieldName))
					{
						byte[] mask = descr.DataValue as byte[];
						foreach (GroupHelper.ParamsPair pair in GroupHelper.GetParams(cross, cmdpars[i].MaskedType, mask))
						{
							pars.Add(new PXDataValue(PXDbType.Int, 4, pair.First));
							pars.Add(new PXDataValue(PXDbType.Int, 4, pair.Second));
						}
					}
				}
			}

			cmd = appendSearches(sorts, pars, cmd, ref topCount, reverseOrder);

			cmd = appendFilters(filters, pars, cmd);

			pars.AddRange(sortnames);

			Type[] tables = cmd.GetTables();
			bool hascount = !typeof(IBqlTable).IsAssignableFrom(tables[tables.Length - 1]);
			PXCache[] caches = new PXCache[hascount ? tables.Length - 1 : tables.Length];
			caches[0] = Cache;
			for (int i = 1; i < caches.Length; i++)
			{
				caches[i] = _Graph.Caches[tables[i]];
				if (_Select is IBqlAggregate)
				{
					caches[i]._AggregateSelecting = true;
				}
			}
			EnsureCreateInstance(tables);
			cmd.Context = context;
			if (Cache.SelectInterceptor != null)
			{
				var interceptorResult = Cache.SelectInterceptor.Select(_Graph, cmd, topCount, this, pars.ToArray());
				foreach (var oResult in interceptorResult)
				{
					if (_CreateInstance == null)
					{
						ret.Add(oResult);
					}
					else
					{
						PXResult res = (PXResult)_CreateInstance(new object[] { oResult });
						res.RowCount = 1;
						ret.Add(res);
					}

				}

				return ret;
			}
			var result = _Graph.ProviderSelect(cmd, topCount, this.RestrictedFields.Any() ? this : null, pars.ToArray());
			PXDataRecordMap map = null;
			if (cmd.RecordMapEntries.Any())
			{
				map = new PXDataRecordMap(cmd.RecordMapEntries);
			}
			foreach (PXDataRecord r in result)
			{
				var rec = r;
				if (map != null)
				{
					map.SetRow(r);
					rec = map;
				}
				var res = CreateResult(caches, rec, hascount, ref overrideSort, ref extFilter);
				if (res != null)
					ret.Add(res);
			}
			if (_Select is IBqlAggregate)
			{
				for (int i = 1; i < caches.Length; i++)
				{
					caches[i]._AggregateSelecting = false;
				}
			}
			return ret;
		}

		private protected virtual object CreateResult(PXCache[] caches, PXDataRecord rec, bool hascount, ref bool overrideSort, ref bool extFilter)
		{
			int position = 0;
			object[] items = new object[caches.Length];
			for (int i = 0; i < caches.Length; i++)
			{
				items[i] = CreateItem(caches[i], rec, ref position, _IsReadOnly || i > 0, out var wasUpdated);
				if (wasUpdated)
				{
					overrideSort = true;
					extFilter = true;
				}
			}
			if (items[0] != null)
			{
				if (_CreateInstance == null)
				{
					return items[0];
				}
				else
				{
					PXResult res = (PXResult)_CreateInstance(items);
					if (hascount)
					{
						res.RowCount = rec.GetInt32(position);
					}
					return res;
				}
			}
			return null;
		}

		private protected object CreateItem(PXCache cache, PXDataRecord record, ref int position, bool isReadOnly, out bool wasUpdated)
		{
			wasUpdated = false;
			if (_LegacyQueryCacheMode)
				return cache.Select(record, ref position, isReadOnly, out wasUpdated);
			else
				return cache.CreateItem(record, ref position, isReadOnly);
		}

		private List<object> GetResultWindowedRO(object[] parameters, PXSearchColumn[] sorts, int skip=0, int take=0) {
			var context = new PXSelectOperationContext();
			_Select.Context = context;
			var ret = new VersionedList();
			IBqlParameter[] cmdpars = _Select.GetParameters();

			if (parameters == null && cmdpars.Length > 0) {

				context.BadParametersQueryNotExecuted = true;
				return ret;
			}
			if (parameters != null) {
				if (cmdpars.Length > parameters.Length) {
					context.BadParametersQueryNotExecuted = true;
					return ret;
				}
				for (int i = 0; i < parameters.Length && i < cmdpars.Length; i++) {
					if (!cmdpars[i].NullAllowed && parameters[i] == null) {
						context.BadParametersQueryNotExecuted = true;
						List<KeyValuePair<Type, Type>> parameterPairs = _Select.GetParameterPairs();
						int badParams = 0;

						for (int k = i; k < parameters.Length && k < cmdpars.Length; k++) {
							Type field = cmdpars[k].GetReferencedType();
							bool pairFound = false;

							foreach (KeyValuePair<Type, Type> pair in parameterPairs) {
								if (pair.Value == field) {
									pairFound = true;
									field = pair.Key;
									PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];

									if (!cmdpars[k].NullAllowed && parameters[k] == null) {
										if (cache.Keys.Contains(char.ToUpper(field.Name[0]) + field.Name.Substring(1))) {
											return ret;
										}
										badParams++;
									}
									break;
								}
							}
							if (!pairFound) {
								return ret;
							}
						}
						if (badParams > 0)
							context.BadParametersSkipMergeCache = true;
						return ret;
					}
				}
			}

			BqlCommand cmd = _Select;

			List<PXDataValue> sortnames = new List<PXDataValue>();
			Type newsort = ConstructSort(sorts, sortnames, false);
			if (newsort != null) {
				cmd = cmd.OrderByNew(newsort);
			}

			List<PXDataValue> pars = new List<PXDataValue>();

			ISqlDialect dialect = _Graph.SqlDialect;

			if (parameters != null) {
				int argNbr = 0;
				for (int i = 0; i < cmdpars.Length && i < parameters.Length; i++) {
					if (cmdpars[i] is FieldNameParam && parameters[i] is PXFieldName) {
						pars.Add((PXFieldName)parameters[i]);
						continue;
					}
					PXCommandPreparingEventArgs.FieldDescription descr = null;
					bool IsPassThroughParameter = (_IsReadOnly || cmdpars[i].NullAllowed);

					Type cross = null;
					if (!cmdpars[i].IsArgument) {
						Type field = cmdpars[i].GetReferencedType();
						if (!field.IsNested || BqlCommand.GetItemType(field) == CacheType) {
							Cache.RaiseCommandPreparing(field.Name, null, parameters[i], PXDBOperation.Select | (IsPassThroughParameter ? PXDBOperation.ReadOnly : PXDBOperation.Normal), null, out descr);
							if (cmdpars[i].MaskedType != null) {
								cross = GroupHelper.GetReferencedType(Cache, field.Name);
							}
						}
						else {
							PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];
							cache.RaiseCommandPreparing(field.Name, null, parameters[i], PXDBOperation.Select | (IsPassThroughParameter ? PXDBOperation.ReadOnly : PXDBOperation.Normal), null, out descr);
							if (cmdpars[i].MaskedType != null) {
								cross = GroupHelper.GetReferencedType(cache, field.Name);
							}
						}
					}
					else if (_Delegate != null) {
						foreach (PXEventSubscriberAttribute attr in _Delegate.Method.GetParameters()[argNbr].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false)) {
							List<IPXCommandPreparingSubscriber> del = new List<IPXCommandPreparingSubscriber>();
							attr.GetSubscriber<IPXCommandPreparingSubscriber>(del);
							if (del.Count > 0) {
								PXCommandPreparingEventArgs preps = new PXCommandPreparingEventArgs(null, parameters[i], PXDBOperation.Select, null, dialect);
								for (int l = 0; l < del.Count; l++) {
									del[l].CommandPreparing(Cache, preps);
								}
								descr = preps.GetFieldDescription();
								argNbr++;
								break;
							}
						}
					}
					if (descr == null || descr.DataValue == null && !cmdpars[i].NullAllowed) {
						PXCommandPreparingEventArgs.FieldDescription innerDescr = IsPassThroughParameter ? descr : null;

						context.BadParametersQueryNotExecuted = true;
						int badParams = 0;

						List<KeyValuePair<Type, Type>> parameterPairs = _Select.GetParameterPairs();

						for (int k = i; k < parameters.Length && k < cmdpars.Length; k++) {
							Type field = cmdpars[k].GetReferencedType();
							bool pairFound = false;

							foreach (KeyValuePair<Type, Type> pair in parameterPairs) {
								if (pair.Value == field) {
									pairFound = true;
									field = pair.Key;
									PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];

									if (innerDescr == null) Cache.RaiseCommandPreparing(field.Name, null, parameters[k], PXDBOperation.Select | PXDBOperation.ReadOnly, null, out innerDescr);
									if (innerDescr == null || innerDescr.DataValue == null && !cmdpars[k].NullAllowed) {
										if (cache.Keys.Contains(char.ToUpper(field.Name[0]) + field.Name.Substring(1))) {
											return ret;
										}
										badParams++;
									}
									innerDescr = null;
									break;
								}
							}
							if (!pairFound) {
								return ret;
							}
						}
						if (badParams > 0)
							context.BadParametersSkipMergeCache = true;

						return ret;
					}
					if (cmdpars[i].MaskedType == null) {
						pars.Add(new PXDataValue(descr.DataType, descr.DataLength, descr.DataValue));
					}
					else if (cmdpars[i].MaskedType == typeof(Array)) {
						pars.Add(new PXDataValue(PXDbType.DirectExpression, descr.DataValue));
					}
					else if (_Graph.Caches[cmdpars[i].MaskedType].Fields.Contains(GroupHelper.FieldName)) {
						byte[] mask = descr.DataValue as byte[];
						foreach (GroupHelper.ParamsPair pair in GroupHelper.GetParams(cross, cmdpars[i].MaskedType, mask)) {
							pars.Add(new PXDataValue(PXDbType.Int, 4, pair.First));
							pars.Add(new PXDataValue(PXDbType.Int, 4, pair.Second));
						}
					}
				}
			}

			pars.AddRange(sortnames);
			Type[] tables = cmd.GetTables();
			bool hascount = !typeof(IBqlTable).IsAssignableFrom(tables[tables.Length - 1]);
			PXCache[] caches = new PXCache[hascount ? tables.Length - 1 : tables.Length];
			caches[0] = Cache;
			for (int i = 1; i < caches.Length; i++) {
				caches[i] = _Graph.Caches[tables[i]];
				if (_Select is IBqlAggregate) {
					caches[i]._AggregateSelecting = true;
				}
			}

			EnsureCreateInstance(tables);
			cmd.Context = context;
			var result = PXDatabase.Select(_Graph, cmd, skip, take, this.RestrictedFields.Any() ? this : null, pars.ToArray());

			PXDataRecordMap map = null;
			if (cmd.RecordMapEntries.Any()) {
				map = new PXDataRecordMap(cmd.RecordMapEntries);
			}
			foreach (PXDataRecord r in result) {
				var rec = r;
				if (map != null) {
					map.SetRow(r);
					rec = map;
				}
				int position = 0;
				object[] items = new object[caches.Length];
				for (int i = 0; i < caches.Length; i++) {
					bool wasUpdated;
					items[i] = caches[i].Select(rec, ref position, _IsReadOnly || i > 0, out wasUpdated);
				}
				if (items[0] != null) {
					if (_CreateInstance == null) {
						ret.Add(items[0]);
					}
					else {
						PXResult res = (PXResult)_CreateInstance(items);
						if (hascount) {
							res.RowCount = rec.GetInt32(position);
						}
						ret.Add(res);
					}
				}
			}
			return ret;
		}

		private protected virtual PXCommandKey GetParametersCacheKey(object[] searches, string[] sortcolumns, bool[] descendings, int maximumRows, PXFilterRow[] filters)
		{
			var key = new PXCommandKey(null, searches, sortcolumns, descendings, 0, maximumRows, filters, PXDatabase.ReadBranchRestricted);
			key.Select = _Select.GetSelectType();
			key.CacheType = CacheGetItemType();
			return key;
		}


		internal virtual void StoreCached(PXCommandKey queryKey, List<object> records)
		{
			StoreCached(queryKey, records, null);
		}

		internal virtual void StoreCached(PXCommandKey queryKey, List<object> records, PXSelectOperationContext context)
		{
			List<object> items = (this.IsReadOnly) ? records : new VersionedList(CloneResult(records), Cache.Version);
			SelectQueries.StoreCached(this, queryKey, items, context);
		}

		public List<object> CloneResult(List<object> list)
		{
			if (this._LegacyQueryCacheMode)
				return list;

			List<object> result = null;
			var versionedList = list as VersionedList;
			if (versionedList != null)
			{
				var temp = new VersionedList(list, versionedList.Version);
				temp.AnyMerged = versionedList.AnyMerged;
				result = temp;
			}
			else
			{
				result = new List<object>(list);
			}

			PXCache[] caches = null;
			Type[] tables = null;
			if (list.Count > 0 && list[0] is PXResult)
			{
				caches = new PXCache[((PXResult)list[0]).TableCount];
				tables = ((PXResult)list[0]).Tables;
				EnsureCreateInstance(tables);
			}
			else
				caches = new PXCache[1];

			caches[0] = Cache;
			for (int i = 1; i < caches.Length; i++)
			{
				caches[i] = _Graph.Caches.GetCache(tables[i]);
			}

			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];
				if (item == null)
					continue;
				result[i] = CloneItem(item, caches);
			}

			return result;
		}

		private protected virtual object CloneItem(object item, PXCache[] caches)
		{
			var res = item as PXResult;
			if (res != null)
			{
				int? rowCount = res.RowCount;
				res = (PXResult)_CreateInstance(res.Items);
				res.RowCount = rowCount;
				for (int j = 0; j < res.Items.Length; j++)
				{
					if (res.Items[j] == null)
						continue;
					var clone = caches[j].CreateCopy(res.Items[j]);
					CloneOriginals(caches[j], res.Items[j], clone);
					res.Items[j] = clone;

				}
				return res;
			}
			else
			{
				var clone = caches[0].CreateCopy(item);
				CloneOriginals(caches[0], item, clone);
				return clone;
			}
		}

		/// <summary>Removes a result set from the cache of the results of BQL statement execution.</summary>
		/// <param name="queryKey">Key to search</param>
		/// <returns>true if resultset is found and removed successfully</returns>
		public bool RemoveCached(PXCommandKey queryKey)
		{
			PXQueryResult res;
			return SelectQueries.TryRemove(queryKey, out res);
		}

		private void CloneOriginals(PXCache cache, object oldItem, object newItem)
		{
			if (oldItem is IBqlTable && newItem is IBqlTable)
			{
				lock (((ICollection)cache._Originals).SyncRoot)
				{
					BqlTablePair orig;
					if (cache._Originals.TryGetValue((IBqlTable)oldItem, out orig))
					{
						cache._Originals[(IBqlTable)newItem] = orig;
					}
				}
			}
		}

		/// <summary>Looks for a resultset inside the internal cache, merges updated and deleted items, adjusts top count</summary>
		/// <param name="queryKey">Key to search</param>
		/// <param name="topCount">Number of rows required</param>
		/// <returns>Resultset if found in the cache, otherwise null</returns>
		protected virtual List<object> LookupCache(PXCommandKey queryKey, ref int topCount, ref bool overrideSort, ref bool needFilterResults)
		{
			List<object> list = null;
			if (_IsCommandMutable)
			{
				SelectQueries.IsCommandMutable = _IsCommandMutable;
			}
			//			if (SelectQueries.IsCommandMutable && queryKey.CommandText == null && queryKey.BadParamsQueryNotExecuted) 
			if (SelectQueries.IsCommandMutable && queryKey.CommandText == null && !queryKey.BadParamsQueryNotExecuted)
			{
				queryKey.CommandText = this.ToString();
			}

			PXQueryResult stored = null;
			if (SelectQueries.TryGetValue(queryKey, out stored)
				&& !(PX.Common.WebConfig.IsClusterEnabled && stored.IsExpired(PXDatabase.Provider)))
			{
				list = stored.Items;
				if (!_IsReadOnly && Graph.Caches.ContainsKey(this.CacheType))
				{
					//records deleted only by reference, not by hash code
					foreach (object item in Cache.Deleted)
					{
						int index = list.IndexOf(item);
						if (index >= 0)
						{
							list.RemoveAt(index);
						}
					}

					if (!_IsReadOnly && !_LegacyQueryCacheMode)
					{
						List<object> result;
						var versionedList = list as VersionedList;
						if (versionedList != null)
						{
							var temp = new VersionedList(list, versionedList.Version);
							temp.AnyMerged = versionedList.AnyMerged;
							result = temp;
						}
						else
						{
							result = new List<object>(list);
						}

						PXCache[] caches = null;
						Type[] tables = null;
						if (list.Count > 0 && list[0] is PXResult)
						{
							caches = new PXCache[((PXResult)list[0]).TableCount];
							tables = ((PXResult)list[0]).Tables;
							EnsureCreateInstance(tables);
						}
						else
							caches = new PXCache[1];

						caches[0] = Cache;
						for (int i = 1; i < caches.Length; i++)
						{
							caches[i] = _Graph.Caches.GetCache(tables[i]);
						}

						for (int i = 0; i < list.Count; i++)
						{
							var item = list[i];
							if (item == null)
								continue;
							result[i] = LookupItem(item, caches, ref overrideSort, ref needFilterResults);
						}
						list = result;
					}
				}
			}
			else
			{
				if (stored != null)
				{
					PXQueryResult temp;
					SelectQueries.TryRemove(queryKey, out temp);
				}
				if (!_IsReadOnly && topCount > 0 && Graph.Caches.ContainsKey(this.CacheType))
				{
					foreach (object item in Cache.Deleted)
					{
						topCount++;
					}
					foreach (object item in Cache.Updated)
					{
						topCount++;
					}
				}
			}
			return list;
		}

		private protected virtual object LookupItem(object item, PXCache[] caches, ref bool overrideSort, ref bool needFilterResults)
		{
			object result;
			bool wasUpdated = false;
			var res = item as PXResult;
			if (res != null)
			{
				int? rowCount = res.RowCount;
				res = (PXResult)_CreateInstance(res.Items);
				res.RowCount = rowCount;
				result = res;
				for (int j = 0; j < res.Items.Length; j++)
				{
					if (res.Items[j] == null)
						continue;
					var newItem = caches[j].Locate(res.Items[j]);
					if (newItem != null && (!(_IsReadOnly) && (Cache.Interceptor == null || Cache.Interceptor.CacheSelected)))
						res.Items[j] = newItem;
					else
					{
						var clone = caches[j].CreateCopy(res.Items[j]);
						CloneOriginals(caches[j], res.Items[j], clone);
						res.Items[j] = clone;
						res[j] = caches[j].Select(res[j], _IsReadOnly || j > 0, out wasUpdated); ;
						if (wasUpdated)
						{
							overrideSort = true;
							needFilterResults = true;
						}
						//caches[j].PlaceNotChanged(res.Items[j], out wasUpdated);
					}
				}
			}
			else
			{
				var newItem = caches[0].Locate(item);
				if (newItem != null && (!(_IsReadOnly) && (Cache.Interceptor == null || Cache.Interceptor.CacheSelected)))
					result = newItem;
				else
				{
					var clone = caches[0].CreateCopy(item);
					CloneOriginals(caches[0], item, clone);
					result = clone;
					result = caches[0].Select(result, _IsReadOnly, out wasUpdated); ;
					if (wasUpdated)
					{
						overrideSort = true;
						needFilterResults = true;
					}
					//caches[0].PlaceNotChanged(result[i], out wasUpdated);
				}
			}
			return result;
		}

		protected PXView _TailSelect;
		/// <summary>Selects the data records joined with the provided data record by the underlying BQL command.</summary>
		/// <param name="item">First data item.</param>
		/// <param name="parameters">Parameters.</param>
		/// <returns>The first item plus joined rows.</returns>
		public virtual void AppendTail(object item, List<object> list, params object[] parameters)
		{
			if (!(_Select is IBqlJoinedSelect))
			{
				return;
			}
			if (_TailSelect == null)
			{
				var tailBqlCommand = ((IBqlJoinedSelect)_Select).GetTail();
				if (tailBqlCommand != null)
					_TailSelect = _Graph.TypedViews.GetView(tailBqlCommand, true);
			}

			List<object> tail;
			if (_TailSelect != null)
			{
				_TailSelect._PrimaryTableType = _CacheType;
				tail = _TailSelect.SelectMultiBound(new object[] { item }, parameters);
				_TailSelect._PrimaryTableType = null;
			}
			else
			{
				tail = new List<object>();
			}
			Type[] tables = _Select.GetTables();
			EnsureCreateInstance(tables);
			if (tail.Count == 0)
			{
				if (!(((IBqlJoinedSelect)_Select).IsInner) || ((IBqlJoinedSelect)_Select).GetTail() == null)
				{
					AppendEmptyTail(item, list, tables);
				}
			}
			else if (!(tail[0] is PXResult))
			{
				for (int i = 0; i < tail.Count; i++)
				{
					list.Add(_CreateInstance(new object[] { item, tail[i] }));
				}
			}
			else
			{
				for (int i = 0; i < tail.Count; i++)
				{
					object[] res = new object[((PXResult)tail[i]).TableCount + 1];
					res[0] = item;
					for (int j = 1; j < res.Length; j++)
					{
						res[j] = ((PXResult)tail[i])[j - 1];
					}
					list.Add(_CreateInstance(res));
				}
			}
		}

		private protected virtual void AppendEmptyTail(object item, List<object> list)
		{
			Type[] tables = _Select.GetTables();
			EnsureCreateInstance(tables);
			AppendEmptyTail(item, list, tables);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AppendEmptyTail(object item, List<object> list, Type[] tables)
		{
			object[] res = new object[tables.Length];
			res[0] = item;
			for (int i = 1; i < tables.Length; i++)
			{
				if (typeof(IBqlTable).IsAssignableFrom(tables[i]))
				{
					res[i] = _Graph.Caches[tables[i]].CreateInstance();
				}
			}
			list.Add(_CreateInstance(res));
		}

		private bool? keyReferenceOnly;
		private bool? fullKeyReferenced;
		/// <summary>
		///  This flag keeps joined DACs (tail) during cache merge of updated main DAC instead of retrieving the tail
		/// </summary>
		internal bool SupressTailSelect;
		/// <summary>Merges the result set with the updated/inserted/deleted items, stores the result set in the internal cache.</summary>
		/// <param name="list">Resultset</param>
		/// <param name="parameters">Query parameters</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="reverseOrder">If reverse of the sort expression is required</param>
		/// <param name="queryKey">Key to store the result in the cache</param>
		private protected virtual bool MergeCache(
			List<object> list,
			object[] parameters,
			//	PXSearchColumn[] sorts,
			//	bool reverseOrder,
			//PXCommandKey queryKey,
			//	bool overrideSort,
			bool filterExists,
			ref bool sortReq
			)
		{

			bool anyMerged = false;
			bool emulation = (Cache.Interceptor is Unit.PXUIEmulatorAttribute);
			SQLTree.Query query = null;
			if (_IsReadOnly && !emulation || !Graph.Caches.ContainsKey(CacheType))
				return false;

			int version = Cache.Version;
			if (_Select is IBqlJoinedSelect || !(list is VersionedList) || ((VersionedList)list).Version != version)
			{
				if (fullKeyReferenced == null)
				{
					if (Cache.BqlSelect != null || _Select is IBqlJoinedSelect)
					{
						fullKeyReferenced = false;
					}
					else
					{
						HashSet<Type> keys = new HashSet<Type>(Cache.BqlImmutables);
						foreach (Type field in _Select.GetReferencedFields(true))
						{
							if (!keys.Remove(field))
							{
								fullKeyReferenced = false;
								break;
							}
						}
						if (fullKeyReferenced == null)
						{
							if (keys.Count == 0)
							{
								fullKeyReferenced = true;
							}
							else
							{
								fullKeyReferenced = false;
							}
						}
					}
				}
				if (!(fullKeyReferenced == true && list.Count == 1 && list[0] != null && object.ReferenceEquals(Cache.Locate(list[0]), list[0])))
				{
					HashSet<object> existing = null;
					foreach (object item in Cache.Inserted)
					{
						if (_Select.Meet(Cache, item, parameters))
						{
							if (_Select is IBqlJoinedSelect && SupressTailSelect)
							{
								if (existing == null)
								{
									existing = new HashSet<object>(list.Select(i => i is PXResult res ? res[0] : i));
								}
								if (!existing.Contains(item))
								{
									AppendEmptyTail(item, list);
								}
							}
							else if (_Select is IBqlJoinedSelect)
							{
								bool append = true;
								for (int i = 0; i < list.Count;)
								{
									PXResult res = list[i] as PXResult;
									if (res != null && Cache.ObjectsEqual(item, res[0]))
									{
										if (emulation)
										{
											append = false;
											break;
										}
										else
										{
											list.RemoveAt(i);
										}
									}
									else
									{
										i++;
									}
								}
								if (append)
								{
									if (emulation)
									{
										if (query == null)
										{
											query = _Select.GetQuery(Cache.Graph, this);
										}
										PXResult tail = Cache._InvokeTailAppender(item, query, parameters);
										if (tail != null)
										{
											list.Add(tail);
										}
										else
										{
											AppendTail(item, list, parameters);
										}
									}
									else
									{
										AppendTail(item, list, parameters);
									}
								}
							}
							else
							{
								if (existing == null)
								{
									existing = new HashSet<object>(list);
								}
								if (!existing.Contains(item))
								{
									list.Add(item);
								}
							}
							sortReq = true;
							anyMerged = true;
							if (fullKeyReferenced == true)
							{
								break;
							}
						}
						else if (list.Count > 0 && !emulation)
						{
							if (_Select is IBqlJoinedSelect)
							{
								if (existing == null)
								{
									existing = new HashSet<object>();
								}
								for (int i = 0; i < list.Count; i++)
								{
									PXResult res = list[i] as PXResult;
									if (res != null)
									{
										existing.Add(res[0]);
									}
								}
							}
							else
							{
								if (existing == null)
								{
									existing = new HashSet<object>(list);
								}
							}
							if (existing.Contains(item))
							{
								list.RemoveAll(_ => object.ReferenceEquals(_, item));
							}
						}
					}
				}
			}
			if (keyReferenceOnly != true || filterExists)
			{
				foreach (object item in Cache.Updated)
				{
					if (keyReferenceOnly == null)
					{
						if (Cache.BqlSelect != null)
						{
							keyReferenceOnly = false;
						}
						else if (Cache.HasChangedKeys())
						{
							keyReferenceOnly = false;
						}
						else
						{
							List<Type> keys = Cache.BqlImmutables;
							keyReferenceOnly = true;
							foreach (Type field in _Select.GetReferencedFields(false))
							{
								if ((BqlCommand.GetItemType(field) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(field))) && !keys.Contains(field))
								{
									keyReferenceOnly = false;
									break;
								}
							}
						}
					}
					if (keyReferenceOnly == true && !filterExists)
					{
						break;
					}
					if (_Select.Meet(Cache, item, parameters))
					{
						if (_Select is IBqlJoinedSelect)
						{
							if (SupressTailSelect)
							{
								for (int i = 0; i < list.Count; i++)
								{
									if (list[i] is PXResult res
										&& Cache.ObjectsEqual(item, res[0]))
									{
										res[0] = item;

									}

								}
							}
							else
							{
								for (int i = 0; i < list.Count;)
								{
									PXResult res = list[i] as PXResult;
									if (res != null && Cache.ObjectsEqual(item, res[0]))
									{
										list.RemoveAt(i);
									}
									else
									{
										i++;
									}
								}
								AppendTail(item, list, parameters);
							}

						}
						else if (!list.Contains(item))
						{
							list.Add(item);
						}
						sortReq = true;
						anyMerged = true;
					}
					else
					{
						if (_Select is IBqlJoinedSelect)
						{
							for (int i = 0; i < list.Count;)
							{
								PXResult res = list[i] as PXResult;
								if (res != null && Cache.ObjectsEqual(item, res[0]))
								{
									list.RemoveAt(i);
								}
								else
								{
									i++;
								}
							}
						}
						else if (list.Contains(item))
						{
							list.Remove(item);
						}
					}
				}
			}
			if (list is VersionedList)
			{
				((VersionedList)list).Version = version;
				((VersionedList)list).AnyMerged |= anyMerged;
			}


			//if (queryKey != null)
			//{
			//	SelectQueries.StoreCached(this, queryKey, list);
			//}
			//PX.SM.PXPerformanceMonitor.SetPeakMemory();
			return anyMerged;
		}

		//static readonly Regex ExtractTablesRegex = new Regex(@"((from)|(join))\s+(?<tableName>\w+)\s+", RegexOptions.IgnoreCase|RegexOptions.ExplicitCapture|RegexOptions.CultureInvariant |RegexOptions.Compiled);

		/// <summary>Sorts the result set.</summary>
		/// <param name="list">Resultset to sort</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="reverseOrder">If reversing of the sort order is required</param>
		protected virtual void SortResult(List<object> list, PXSearchColumn[] sorts, bool reverseOrder)
		{
			if (list.Count < 2 || sorts.Length == 0)
			{
				return;
			}

			ISqlDialect dialect = null;
			compareDelegate[] comparisons = new compareDelegate[sorts.Length];
			for (int i = 0; i < sorts.Length; i++)
			{
				string fieldName = sorts[i].Column;
				Type tableType = null;
				PXCache cache = Cache;
				bool descending = sorts[i].Descending;
				bool useExt = sorts[i].UseExt;
				int idx = fieldName.IndexOf("__");
				if (idx != -1)
				{
					tableType = list[0] is PXResult ? (((PXResult)list[0]).GetItemType(fieldName.Substring(0, idx))) : null;
					fieldName = fieldName.Substring(idx + 2);
					if (tableType != null)
					{
						cache = _Graph.Caches[tableType];
					}
				}
				else if (list[0] is PXResult)
				{
					PXResult pxres = (PXResult)list[0];
					tableType = pxres.GetItemType(0);
					if (useExt && !cache.Fields.Contains(fieldName)) // Compare method won't read the value if the field belongs to a diffirent dac
					{
						bool found = false;
						for (int iExt = 1; iExt < pxres.Items.Length; iExt++)
							if (found = (cache = _Graph.Caches[tableType = pxres.GetItemType(iExt)]).Fields.Contains(fieldName))
								break;
						if (!found)
						{ // no luck finding the item, revert
							cache = Cache;
							tableType = pxres.GetItemType(0);
						}
					}
				}

				PXCollationComparer collationCmp = PXLocalesProvider.CollationComparer;
				PXStringState sstate = cache.GetStateExt(null, fieldName) as PXStringState;
				Dictionary<string, string> dict = null;
				if (sstate != null && sstate.AllowedValues != null && sstate.AllowedLabels != null)
				{
					dict = sstate.ValueLabelDic;
				}
				if (!reverseOrder)
				{
					comparisons[i] = (a, b) => CompareMethod(a, b, cache, fieldName, @descending, useExt, tableType, collationCmp, dict, ref dialect);
				}
				else
				{
					comparisons[i] = (a, b) => -CompareMethod(a, b, cache, fieldName, @descending, useExt, tableType, collationCmp, dict, ref dialect);
				}
			}
			list.Sort((a, b) => Compare(a, b, comparisons));
		}

		protected delegate object getPXResultValue(object item);

		private static int compareGuid(Guid a, Guid b, ISqlDialect dialect)
		{
			int[] guidOrder = dialect.GetGuidByteOrder();
			byte[] avals = a.ToByteArray();
			byte[] bvals = b.ToByteArray();
			for (int i = guidOrder.Length - 1; i >= 0; i--)
			{
				int res = avals[guidOrder[i]].CompareTo(bvals[guidOrder[i]]);
				if (res != 0)
				{
					return res;
				}
			}
			return 0;
		}

		protected sealed class HashList : List<object>
		{
			private HashSet<object> _hashset;
			public HashList()
				: base()
			{
				_hashset = new HashSet<object>();
			}
			public HashList(IEnumerable<object> collection)
				: base(collection)
			{
				_hashset = new HashSet<object>(collection);
			}
			public new bool Contains(object item)
			{
				return _hashset.Contains(item);
			}
			public new void Add(object item)
			{
				base.Add(item);
				_hashset.Add(item);
			}
			public new void RemoveAt(int index)
			{
				_hashset.Remove(base[index]);
				base.RemoveAt(index);
			}
		}

		private void ConvertFilterValues(object value, PXFilterRow fr)
		{
			DateTime filterDate;

			if (fr.Value is string && value is DateTime &&
				DateTime.TryParse((string)fr.Value, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out filterDate))
			{
				fr.Value = filterDate;
			}

			if (fr.Value2 is string && value is DateTime &&
				DateTime.TryParse((string)fr.Value2, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out filterDate))
			{
				fr.Value2 = filterDate;
			}
		}

		protected internal virtual void FilterResult(
			List<object> list,
			PXFilterRow[] filters
			)
		{
			if (filters == null || filters.Length == 0 || list.Count == 0)
			{
				return;
			}

			List<KeyValuePair<HashList, bool>> levels = new List<KeyValuePair<HashList, bool>>();
			int currlevel = 0;

			for (int i = 0; i < filters.Length; i++)
			{
				PXFilterRow fr = filters[i];
				if (fr.Condition != PXCondition.ISNULL
					&& fr.Condition != PXCondition.ISNOTNULL
					&& fr.Condition != PXCondition.TODAY
					&& fr.Condition != PXCondition.OVERDUE
					&& fr.Condition != PXCondition.TODAY_OVERDUE
					&& fr.Condition != PXCondition.TOMMOROW
					&& fr.Condition != PXCondition.THIS_WEEK
					&& fr.Condition != PXCondition.NEXT_WEEK
					&& fr.Condition != PXCondition.THIS_MONTH
					&& fr.Condition != PXCondition.NEXT_MONTH
					&& fr.Value == null
					|| fr.Condition == PXCondition.BETWEEN
					&& fr.Value2 == null || string.IsNullOrEmpty(fr.DataField))
				{
					continue;
				}

				int idx = fr.DataField.IndexOf("__");
				Type t = list[0] is PXResult ? (idx != -1 ? ((PXResult)list[0]).GetItemType(fr.DataField.Substring(0, idx)) : ((PXResult)list[0]).GetItemType(0)) : Cache.GetItemType();
				string dataField = idx != -1 ? fr.DataField.Substring(idx + 2) : fr.DataField;
				if (idx != -1 && t == null)
				{
					continue;
				}
				PXCache cache = idx != -1 ? _Graph.Caches[t] : Cache;
				if (String.IsNullOrEmpty(dataField) || !cache.Fields.Contains(dataField))
				{
					continue;
				}
				getPXResultValue getValue;
				if (fr.UseExt)
				{
					getValue = list[0] is PXResult
						? new getPXResultValue(item =>
							{
								object value = PXFieldState.UnwrapValue(cache.GetValueExt(((PXResult)item)[t], dataField));
								ConvertFilterValues(value, fr);
								return value;
							})
						: new getPXResultValue(item =>
							{
								object value = PXFieldState.UnwrapValue(cache.GetValueExt(item, dataField));
								ConvertFilterValues(value, fr);
								return value;
							});
				}
				else
				{
					getValue = list[0] is PXResult
						? new getPXResultValue(item =>
							{
								object value = cache.GetValue(((PXResult)item)[t], dataField);
								ConvertFilterValues(value, fr);
								return value;
							})
						: new getPXResultValue(item =>
							{
								object value = cache.GetValue(item, dataField);
								ConvertFilterValues(value, fr);
								return value;
							});
				} 

				var state = GetStateFromCorrectCache(fr.DataField) as PXStringState;
				if (state != null && state.MultiSelect && fr.Value != null)
					fr.Value = fr.Value.ToString().Trim(new[] { ',' });

				Predicate<object> cmp = null;
				switch (fr.Condition)
				{
					case PXCondition.IN:
					case PXCondition.NI:
						#region Filter variables (@mygroups, @myworktree)
						if (fr.Variable == FilterVariableType.CurrentUserGroups)
						{
							if (fr.UseExt)
							{
								cmp = item =>
								{
									string val = getValue(item) as string;
									if (!string.IsNullOrEmpty(val))
									{
										UserGroupLazyCache lazyCache = UserGroupLazyCache.Current;
										HashSet<string> userGroups = lazyCache.GetUserGroupDescriptions(PXAccess.GetUserID());
										return fr.Condition == PXCondition.IN
											? userGroups.Contains(val)
											: !userGroups.Contains(val);
									}
									return false;
								};
							}
							else
							{
								cmp = item =>
								{
									object val = getValue(item);
									if (val is int)
									{
										UserGroupLazyCache lazyCache = UserGroupLazyCache.Current;
										HashSet<int> userGroups = lazyCache.GetUserGroupIds(PXAccess.GetUserID());
										return fr.Condition == PXCondition.IN
											? userGroups.Contains((int)val)
											: !userGroups.Contains((int)val);
									}
									return false;
								};
							}
						}
						else if (fr.Variable == FilterVariableType.CurrentUserGroupsTree)
						{
							if (fr.UseExt)
							{
								cmp = item =>
								{
									string val = getValue(item) as string;
									if (!string.IsNullOrEmpty(val))
									{
										UserGroupLazyCache lazyCache = UserGroupLazyCache.Current;
										HashSet<string> userGroups = lazyCache.GetUserWorkTreeDescriptions(PXAccess.GetUserID());
										return fr.Condition == PXCondition.IN
											? userGroups.Contains(val)
											: !userGroups.Contains(val);
									}
									return false;
								};
							}
							else
							{
								cmp = item =>
								{
									object val = getValue(item);
									if (val is int)
									{
										UserGroupLazyCache lazyCache = UserGroupLazyCache.Current;
										HashSet<int> userGroups = lazyCache.GetUserWorkTreeIds(PXAccess.GetUserID());
										return fr.Condition == PXCondition.IN
											? userGroups.Contains((int)val)
											: !userGroups.Contains((int)val);
									}
									return false;
								};
							}
						}
						#endregion
						else if (fr.Value is Type)
						{
							PXCache subcache = _Graph.Caches[BqlCommand.GetItemType((Type)fr.Value)];
							List<PXDataField> pars = new List<PXDataField>();
							int brackets = fr.OpenBrackets;
							for (; brackets > 0 && i + 1 < filters.Length; i++)
							{
								brackets += filters[i + 1].OpenBrackets;
								brackets -= filters[i + 1].CloseBrackets;
								if (filters[i + 1].Description != null)
								{
									PXComp comparison;
									switch (filters[i + 1].Condition)
									{
										case PXCondition.EQ:
											comparison = PXComp.EQ;
											break;
										case PXCondition.NE:
											comparison = PXComp.NE;
											break;
										case PXCondition.GE:
											comparison = PXComp.GE;
											break;
										case PXCondition.GT:
											comparison = PXComp.GT;
											break;
										case PXCondition.LE:
											comparison = PXComp.LE;
											break;
										case PXCondition.LT:
											comparison = PXComp.LT;
											break;
										case PXCondition.ISNULL:
											comparison = PXComp.ISNULL;
											break;
										case PXCondition.ISNOTNULL:
											comparison = PXComp.ISNOTNULL;
											break;
										case PXCondition.LIKE:
											comparison = PXComp.LIKE;
											break;
										default:
											continue;
									}
									var fieldExpr = filters[i + 1].Description.Expr;
									fieldExpr = fieldExpr.Duplicate().substituteTableName(subcache.GetItemType().Name, subcache.BqlTable.Name);
									pars.Add(new PXDataFieldValue(
										fieldExpr,
										filters[i + 1].Description.DataType,
										filters[i + 1].Description.DataLength,
										filters[i + 1].Description.DataValue,
										comparison));
								}
							}
							cmp = delegate(object item)
							{
								object val = getValue(item);
								PXCommandPreparingEventArgs.FieldDescription descr;
								subcache.RaiseCommandPreparing(((Type)fr.Value).Name, null, val, PXDBOperation.Select, null, out descr);
								if (descr != null && descr.Expr != null && descr.Expr.Oper() != SQLTree.SQLExpression.Operation.NULL)
								{
									pars.Add(new PXDataFieldValue(descr.Expr, descr.DataType, descr.DataLength, descr.DataValue));
									pars.Add(new PXDataField(descr.Expr));
									using (PXDataRecord record = PXDatabase.SelectSingle(subcache.BqlTable, pars.ToArray()))
									{
										if (fr.Condition == PXCondition.IN && record != null
											|| fr.Condition == PXCondition.NI && record == null)
										{
											return true;
										}
										return false;
									}
								}
								return true;
							};
						}
						break;
					case PXCondition.ER:
							cmp = delegate(object item)
							{
								object val = getValue(item);
								if (!(val is long) || !(fr.Value is string))
								{
									return false;
								}
								OrderedDictionary dict = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
								dict["NoteID"] = val;
								PXCache nc = _Graph.Caches[typeof(Note)];
								if (nc.Locate(dict) > 0 && nc.Current != null && String.Equals(((Note)nc.Current).ExternalKey, (string)fr.Value, StringComparison.InvariantCultureIgnoreCase))
								{
									return true;
								}
								return false;
							};
						break;
					case PXCondition.EQ:
						if (fr.Value != null)
						{
							if (fr.Value is string)
							{
								string sval = (string)fr.Value;

								cmp = delegate(object item)
								{
									string val = getValue(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(val, sval) == 0;
								};
							}
							else
							{
								if (fr.UseExt && fr.Value is bool)
								{
									cmp = delegate(object item)
									{
										try
										{
											return object.Equals(Convert.ToBoolean(getValue(item)), fr.Value);
										}
										catch (FormatException)
										{
											return false;
										}
									};
								}
								else
								{
									cmp = item => object.Equals(getValue(item), fr.Value);
								}
							}
						}
						else
						{
							list.Clear();
							return;
						}
						break;
					case PXCondition.NE:
						if (fr.Value != null)
						{
							if (fr.Value is string)
							{
								string sval = (string)fr.Value;

								cmp = delegate(object item)
								{
									string val = getValue(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(val, sval) != 0;
								};
							}
							else
							{
								cmp = item => !object.Equals(getValue(item), fr.Value);
							}
						}
						break;
					case PXCondition.GT:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;

							cmp = delegate(object item)
							{
								string val = getValue(item) as string;
								return PXLocalesProvider.CollationComparer.Compare(sval, val) < 0;
							};
						}
						else if (fr.Value is IComparable)
						{
							cmp = item => ((IComparable)fr.Value).CompareTo(getValue(item)) < 0;
						}
						break;
					case PXCondition.GE:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;
							cmp = delegate(object item)
							{
								string val = getValue(item) as string;
								return PXLocalesProvider.CollationComparer.Compare(sval, val) <= 0;
							};
						}
						else if (fr.Value is IComparable)
						{
							cmp = item => ((IComparable)fr.Value).CompareTo(getValue(item)) <= 0;
						}
						break;
					case PXCondition.LT:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;
							cmp = delegate(object item)
							{
								string val = getValue(item) as string;
								return PXLocalesProvider.CollationComparer.Compare(sval, val) > 0;
							};
						}
						else if (fr.Value is IComparable)
						{
							cmp = item => ((IComparable) fr.Value).CompareTo(getValue(item)) > 0;
						}
						else
						{
							list.Clear();
							return;
						}
						break;
					case PXCondition.LE:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;
							cmp = delegate(object item)
							{
								string val = getValue(item) as string;
								return PXLocalesProvider.CollationComparer.Compare(sval, val) >= 0;
							};
						}
						else if (fr.Value is IComparable)
						{
							cmp = item => ((IComparable) fr.Value).CompareTo(getValue(item)) >= 0;
						}
						else
						{
							list.Clear();
							return;
						}
						break;

					
					case PXCondition.LIKE:
					case PXCondition.NOTLIKE:
					case PXCondition.LLIKE:
					case PXCondition.RLIKE:
						if (fr.Value is string)
						{
							bool likeInvertResult = false;
							Func<string, bool> matcher = null;
							string fValue = ((string)fr.Value);

							// Filter by label if it is a fast filter search; otherwise, filter by value
							bool isFastFilter = filters.Length > 1 && filters
								.Except(fr)
								.All(f => f.Condition == PXCondition.LIKE && f.Value is string && String.Equals(((string)f.Value).Trim(), fValue?.Trim(), StringComparison.OrdinalIgnoreCase));

							// Using compiled regex for comparison is approximately 2.5x slower, so it is used only for wildcards 
							if (fValue.Contains(cache.Graph.SqlDialect.WildcardAnySingle) || fValue.Contains(cache.Graph.SqlDialect.WildcardAnything))
							{
								string pattern = fValue.Replace(cache.Graph.SqlDialect.WildcardAnything, ".*").Replace(cache.Graph.SqlDialect.WildcardAnySingle, ".{1}");
								switch (fr.Condition)
								{
									case PXCondition.LIKE:
										pattern = ".*" + pattern + ".*";
										likeInvertResult = false;
										break;
									case PXCondition.NOTLIKE:
										pattern = ".*" + pattern + ".*";
										likeInvertResult = true;
										break;
									case PXCondition.LLIKE:
										pattern = ".*" + pattern;
										likeInvertResult = false;
										break;
									case PXCondition.RLIKE:
										pattern = pattern + ".*";
										likeInvertResult = false;
										break;
								}
								Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
								matcher = v => regex.IsMatch(v);
							}
							else
							{
								string lower = fValue.ToLower();
								switch (fr.Condition)
								{
									case PXCondition.LIKE:
										matcher = v => v.ToLower().Contains(lower);
										likeInvertResult = false;
										break;
									case PXCondition.NOTLIKE:
										matcher = v => v.ToLower().Contains(lower);
										likeInvertResult = true;
										break;
									case PXCondition.LLIKE:
										matcher = v => v.EndsWith(fValue, StringComparison.CurrentCultureIgnoreCase);
										likeInvertResult = false;
										break;
									case PXCondition.RLIKE:
										matcher = v => v.StartsWith(fValue, StringComparison.CurrentCultureIgnoreCase);
										likeInvertResult = false;
										break;
								}
							}
							PXStringState sstate = cache.GetStateExt(null, dataField) as PXStringState;
							Dictionary<string, string> valueLabelDic = null;
							if (sstate != null && sstate.ValueLabelDic != null && sstate.ValueLabelDic.Count > 0)
								valueLabelDic = sstate.ValueLabelDic;
							cmp = delegate(object item)
							{
								string val = getValue(item)?.ToString();
								if (val != null && isFastFilter && valueLabelDic != null && valueLabelDic.ContainsKey(val))
								{
									val = valueLabelDic[val];
								}
								return val != null && (likeInvertResult ^ matcher(val));
							};
						}
						break;
						
					case PXCondition.ISNULL:
						cmp = item => getValue(item) == null || (getValue(item) is string s && string.IsNullOrWhiteSpace(s));
						break;
					case PXCondition.ISNOTNULL:
						cmp = item => !(getValue(item) is string s && string.IsNullOrWhiteSpace(s)) && getValue(item) != null;
						break;
					case PXCondition.OVERDUE:
					case PXCondition.TOMMOROW:
					case PXCondition.THIS_WEEK:
					case PXCondition.TODAY:
					case PXCondition.THIS_MONTH:
					case PXCondition.BETWEEN:
						if (fr.Value is string && fr.Value2 is string)
						{
							cmp = delegate(object item)
							{
								object val = getValue(item);
								if (val is string)
								{
									return PXLocalesProvider.CollationComparer.Compare((string)fr.Value, (string)val) <= 0 && PXLocalesProvider.CollationComparer.Compare((string)fr.Value2, (string)val) >= 0;
								}
								return ((IComparable)fr.Value).CompareTo(val) <= 0 && ((IComparable)fr.Value2).CompareTo(val) >= 0;
							};
						}
						else if (fr.Value is IComparable && fr.Value2 is IComparable)
						{
							cmp = delegate(object item)
							{
								object val = getValue(item);
								return ((IComparable)fr.Value).CompareTo(val) <= 0 && ((IComparable)fr.Value2).CompareTo(val) >= 0;
							};
						}
						else
						{
							list.Clear();
							return;
						}
						break;										
					case PXCondition.TODAY_OVERDUE:
					case PXCondition.NEXT_WEEK:
					case PXCondition.NEXT_MONTH:
						DateTime?[] range = DateTimeFactory.GetDateRange(fr.Condition, null);
						DateTime? startDate = range[0];
						DateTime? endDate = range[1];
						cmp = delegate(object item)
						{
							DateTime? val = (DateTime?)getValue(item);
							return val != null &&
								(startDate == null || startDate.Value.CompareTo(val) <= 0) &&
								(endDate == null || endDate.Value.CompareTo(val) >= 0);
						};
						break;
				}
				if (cmp != null)
				{
					currlevel += fr.OpenBrackets;
					if (currlevel < levels.Count)
					{
						if (!levels[currlevel].Value)
						{
							levels[currlevel] = new KeyValuePair<HashList, bool>(new HashList(levels[currlevel].Key.FindAll(cmp)), fr.OrOperator);
						}
						else
						{
							List<object> filtered = list.FindAll(cmp);
							foreach (object found in filtered)
							{
								if (!levels[currlevel].Key.Contains(found))
								{
									levels[currlevel].Key.Add(found);
								}
							}
							levels[currlevel] = new KeyValuePair<HashList, bool>(levels[currlevel].Key, fr.OrOperator);
						}
					}
					else
					{
						for (int k = levels.Count; k <= currlevel; k++)
						{
							levels.Add(new KeyValuePair<HashList, bool>(new HashList(), true));
						}
						levels[currlevel] = new KeyValuePair<HashList, bool>(new HashList(list.FindAll(cmp)), fr.OrOperator);
					}
					for (int k = 0; k < fr.CloseBrackets; k++)
					{
						if (currlevel == 0)
						{
							break;
						}
						if (levels[currlevel - 1].Value)
						{
							foreach (object found in levels[currlevel].Key)
							{
								if (!levels[currlevel - 1].Key.Contains(found))
								{
									levels[currlevel - 1].Key.Add(found);
								}
							}
						}
						else
						{
							for (int l = 0; l < levels[currlevel - 1].Key.Count; )
							{
								if (!levels[currlevel].Key.Contains(levels[currlevel - 1].Key[l]))
								{
									levels[currlevel - 1].Key.RemoveAt(l);
								}
								else
								{
									l++;
								}
							}
						}
						levels[currlevel - 1] = new KeyValuePair<HashList, bool>(levels[currlevel - 1].Key, fr.OrOperator);
						levels.RemoveAt(currlevel);
						currlevel--;
					}
				}
			}
			for (; currlevel > 0; currlevel--)
			{
				if (levels[currlevel - 1].Value)
				{
					foreach (object found in levels[currlevel].Key)
					{
						if (!levels[currlevel - 1].Key.Contains(found))
						{
							levels[currlevel - 1].Key.Add(found);
						}
					}
				}
				else
				{
					for (int l = 0; l < levels[currlevel - 1].Key.Count; )
					{
						if (!levels[currlevel].Key.Contains(levels[currlevel - 1].Key[l]))
						{
							levels[currlevel - 1].Key.RemoveAt(l);
						}
						else
						{
							l++;
						}
					}
				}
			}
			if (levels.Count > 0)
			{
				for (int i = 0; i < list.Count; )
				{
					if (levels[0].Key.Contains(list[i]))
					{
						i++;
					}
					else
					{
						list.RemoveAt(i);
					}
				}
			}
		}

		/// <summary>Cuts the resultset by search values, starting row and number of rows required</summary>
		/// <param name="list">The result set.</param>
		/// <param name="searches">Values to search</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="reverseOrder">If reversing of the sort expression is required</param>
		/// <param name="startRow">Index of row to start with</param>
		/// <param name="maximumRows">Maximum number of rows to return</param>
		/// <param name="totalRows">Total number of rows fetched</param>
		/// <param name="searchFound">If there is an item that meets search values</param>
		/// <returns>Filtered resultset</returns>
		protected virtual List<object> SearchResult(
			List<object> list,
			PXSearchColumn[] sorts,
			bool reverseOrder,
			bool findAll,
			ref int startRow,
			int maximumRows,
			ref int totalRows,
			out bool searchFound
			)
		{
			if (list.Count == 0)
			{
				searchFound = false;
				return list;
			}

			ISqlDialect dialect = null;
			searchFound = false;
			int startOffset = 0;
			List<object> ret = list is VersionedList ? new VersionedList() : new List<object>();

			List<searchDelegate> comparisons = null;
			for (int k = 0; k < sorts.Length; k++)
			{
				if (sorts[k].SearchValue != null)
				{
					if (comparisons == null)
					{
						comparisons = new List<searchDelegate>();
					}
					string fieldName = sorts[k].Column;
					Type tableType = null;
					PXCache cache = Cache;
					bool descending = sorts[k].Descending;
					bool useExt = sorts[k].UseExt;
					int idx = fieldName.IndexOf("__");
					object searchValue = sorts[k].SearchValue;
					if (idx != -1)
					{
						tableType = list[0] is PXResult ? (((PXResult)list[0]).GetItemType(fieldName.Substring(0, idx))) : null;
						if (tableType != null)
						{
							cache = _Graph.Caches[tableType];
						}
						fieldName = fieldName.Substring(idx + 2);
					}
					else if (list[0] is PXResult)
					{
						tableType = ((PXResult)list[0]).GetItemType(0);
					}
					PXStringState sstate = cache.GetStateExt(null, fieldName) as PXStringState;
					Dictionary<string, string> dict = null;
					if (sstate != null && sstate.AllowedValues != null && sstate.AllowedLabels != null && sstate.ValueLabelDic.Count > 0)
					{
						dict = sstate.ValueLabelDic;
					}
					comparisons.Add(delegate (object a)
					{
						return SearchMethod(a, searchValue, cache, fieldName, descending, useExt, tableType, dict, ref dialect);
					});
				}
			}

			if (comparisons != null)
			{
				searchDelegate[] searches = comparisons.ToArray();
				int i = 0;
				if (comparisons.Count > 0)
				{
					while (i < list.Count && (!reverseOrder && Search(list[i], searches) < 0 || reverseOrder && Search(list[i], searches) >= 0))
					{
						i++;
					}
				}
				searchFound = !reverseOrder && i < list.Count && Search(list[i], searches) == 0;

				if (totalRows == -1 && maximumRows > 2)
				{
					if (!reverseOrder)
					{
						i = (i / maximumRows * maximumRows);
					}
					else
					{
						int j = list.Count - i - 1;
						j = ((j / maximumRows + 1) * maximumRows);
						i = list.Count - j;
						if (i < 0)
						{
							maximumRows += i;
							i = 0;
						}
					}
				}

				int k = list.Count;
				if (searchFound && !reverseOrder && findAll)
				{
					k = i + 1;
					while (k < list.Count && Search(list[k], searches) == 0)
					{
						k++;
					}
				}

				for (int j = i; j < k; j++)
				{
					ret.Add(list[j]);
				}

				if (totalRows == -1)
				{
					if (reverseOrder)
					{
						startOffset = list.Count - i - 1;
					}
					else
					{
						startOffset = i;
					}
				}
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					ret.Add(list[i]);
				}
				if (totalRows == -1)
				{
					if (reverseOrder)
					{
						if (maximumRows > 2)
						{
							int j = list.Count - startRow - 1;
							j = ((j / maximumRows + 1) * maximumRows);
							if (j > 0 && j < list.Count && list.Count - j < startRow)
							{
								maximumRows = list.Count - j;
								startRow = maximumRows - 1;
							}
						}
						startOffset = list.Count;
					}
				}
			}

			totalRows = list.Count;

			if (!reverseOrder)
			{
				if (startRow > 0)
				{
					if (startRow <= ret.Count)
					{
						ret.RemoveRange(0, startRow);
					}
					else
					{
						ret.Clear();
					}
				}
			}
			else
			{
				if (maximumRows > 0 && startRow >= ret.Count)
				{
					maximumRows -= (startRow - ret.Count + 1);
					if (maximumRows < 0) maximumRows = 0;
				}
				if (startRow + 1 < ret.Count)
				{
					ret.RemoveRange(startRow + 1, ret.Count - startRow - 1);
				}
				ret.Reverse();
			}

			if (maximumRows >= 0 && maximumRows < ret.Count)
			{
				ret.RemoveRange(maximumRows, ret.Count - maximumRows);
			}

			if (!reverseOrder)
			{
				startRow += startOffset;
			}
			else
			{
				startRow = startOffset - startRow - 1;
			}

			if (ret is VersionedList)
			{
				((VersionedList)ret).Version = ((VersionedList)list).Version;
			}

			return ret;
		}

		public class PXSortColumn
		{
			public string Column;
			public bool Descending;
			public bool UseExt;

			public PXSortColumn(string column, bool descending)
			{
				Column = column;
				Descending = descending;
			}

			public override string ToString()
			{
				return string.Format("{0}: {1} {2} {3}", GetType().Name, Column, Descending ? "DESC" : "ASC", UseExt ? "[EXT]" : string.Empty);
			}
		}
		public sealed class PXSearchColumn : PXSortColumn, ICloneable
		{
			public object SearchValue;
			public object OrigSearchValue;
			public PXCommandPreparingEventArgs.FieldDescription Description;
			public Type SelSort;

			public PXSearchColumn(string column, bool descending, object searchValue)
				: base(column, descending)
			{
				OrigSearchValue = SearchValue = searchValue;
			}

			public object Clone()
			{
				var clone = new PXSearchColumn(Column, Descending, SearchValue);
				clone.OrigSearchValue = OrigSearchValue;
				clone.SelSort = SelSort;
				clone.UseExt = UseExt;
				clone.Description = Description?.Clone() as PXCommandPreparingEventArgs.FieldDescription;
				return clone;
			}
		}


		//protected virtual void BeforeSelect(PXFilterRow[] filters, object[] parameters)
		//{
			
		//}
	
		static readonly ConcurrentDictionary<object, IBqlParameter[]> CommandParamsCache = new ConcurrentDictionary<object, IBqlParameter[]>();
		Type _NewOrder;


		/// <summary>The main selection procedure.</summary>
		/// <param name="currents">Items to replace current values when retrieving Current and Optional parameters</param>
		/// <param name="parameters">Query parameters</param>
		/// <param name="searches">Search values</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="startRow">Index of row to start with</param>
		/// <param name="maximumRows">Maximum rows to return</param>
		/// <param name="totalRows">Total rows fetched</param>
		/// <returns>Resultset requested</returns>
		public virtual List<object> Select(
			object[] currents,
			object[] parameters,
			object[] searches,
			string[] sortcolumns,
			bool[] descendings,
			PXFilterRow[] filters,
			ref int startRow,
			int maximumRows,
			ref int totalRows
			)
		{
			var originalSelect = _Select;
			var originalFilters = filters;

			var perf = PXPerformanceMonitor.CurrentSample;
			if (perf != null)
			{
				perf.SelectCount++;
				perf.SelectTimer.Start();
			}



			int realTotalRows = totalRows;
			if (totalRows == -2) totalRows = 0;
			bool overrideSort;
			bool anySearch;
			bool resetTopCount = false;
			if (FiltersResetRequired)
				filters = null;

			//we need cache parameters if they are expensive

			var key = GetParametersCacheKey(searches, sortcolumns, descendings, maximumRows, filters);

			PXSearchColumn[] sorts;
			bool needFilterResults = false;

			bool paramsFromCache = false;
			SelectCacheEntry entry;
			if (SelectQueries.ParametersCache.TryGetValue(key, out entry))
			{
				paramsFromCache = true;
				sorts = entry.Sorts;

				overrideSort = entry.overrideSort;
				anySearch = entry.anySearch;
				resetTopCount = entry.resetTopCount;

				if (filters != null && filters.Length > 0)
				{
					filters = entry.filters;
					needFilterResults = entry.extFilter;
				}


				if (entry.NewOrder != null)
					_Select = _Select.OrderByNew(entry.NewOrder);

			}
			else
			{
				_NewOrder = null;

				//	Analyze Bql command sort columns, override with external sort order, append primary key.
				// as side effect can override _Select with OrderByNew() method.
				sorts = prepareSorts(sortcolumns, descendings, searches, maximumRows, out overrideSort, out anySearch, ref resetTopCount);

				if (filters != null && filters.Length > 0)
				{
					needFilterResults = prepareFilters(ref filters);
				}


				entry = new SelectCacheEntry
				{
					KeyDigest = key,
					NewOrder = _NewOrder,
					overrideSort = overrideSort,
					anySearch = anySearch,
					resetTopCount = resetTopCount,
					extFilter = needFilterResults,


				};

				if (filters != null)
					entry.filters = (PXFilterRow[])filters.Clone();

				if (sorts != null)
					entry.Sorts = (PXSearchColumn[])sorts.Clone();



				_NewOrder = null;

				SelectQueries.ParametersCache.Add(key, entry);

			}


			//can cache this value per _Select or per View
			//we do not expect this parameters can be changed
			//			IBqlParameter[] selpars = _Select.GetParameters();

			IBqlParameter[] selpars = CommandParamsCache.GetOrAdd(_Select.GetUniqueKey(), type => _Select.GetParameters());


			//can be dependency on graph, if we have some currents in parameters
			parameters = PrepareParametersInternal(currents, parameters, selpars);
			//parameters = PrepareParameters(currents, parameters);




			//BeforeSelect(originalFilters, parameters);



			if (needFilterResults)
			{
				resetTopCount = true;
			}
			int topCount = 0;
			bool reverseOrder = false;
			if (startRow < 0)
			{
				reverseOrder = true;
				overrideSort = true;
				startRow = -1 - startRow;
				if (maximumRows > 0 && totalRows != -1)
				{
					topCount = startRow + 1;
				}
			}
			else
			{
				if (maximumRows > 0 && totalRows != -1)
				{
					topCount = startRow + maximumRows;
				}
			}
			if (maximumRows == 0) maximumRows = -1;
			if (resetTopCount)
			{
				topCount = 0;
			}


			bool windowed = (totalRows != -1 && maximumRows > 0);

			int realTopCount = topCount;
			bool retrieveTotalRowCount = false;

			List<object> list = null;
			IPXDelegateResult delegateOptions = null;
			try
			{
				if (_Executing != null)
				{
					_Executing.Push(new Context(this, sorts, filters, currents, parameters, reverseOrder, windowed ? startRow : 0, windowed ? maximumRows : 0, totalRows == -1, this.RestrictedFields));
				}
				list = InvokeDelegate(parameters);
				delegateOptions = list as IPXDelegateResult;
				if (list is IPXDelegateCacheResult delegateCache && delegateCache.IsResultCachable)
				{
					var cacheKey = new PXCommandKey(delegateCache.CacheKeys);
					var originalList = list;
					list = LookupCache(cacheKey, ref topCount, ref overrideSort, ref needFilterResults);
					if (list == null)
					{
						delegateCache.EmitRows();
						list = originalList;
						StoreCached(cacheKey, list, new PXSelectOperationContext() { LastSqlTables = new List<string>(delegateCache.SqlTables) });
					}

				}


			}
			finally
			{
				if (_Executing != null)
				{
					Context context = _Executing.Pop();
					retrieveTotalRowCount = context.RetrieveTotalRowCount;
					if (list != null && windowed)
					{
						if (reverseOrder && context.StartRow == 0 && startRow != 0)
						{
							startRow = maximumRows - 1;
						}
						else
						{
							startRow = context.StartRow;
						}
					}
					if (!object.ReferenceEquals(context.Filters, filters))
					{
						filters = context.Filters;
						needFilterResults = prepareFilters(ref filters);
					}
					if (!object.ReferenceEquals(context.Sorts, sorts))
					{
						sorts = context.Sorts;
					}
				}
			}

			// If it is result of row count query
			if (retrieveTotalRowCount && list != null && list.Count == 1 && (list[0] as PXResult)?.RowCount != null)
			{
				totalRows = ((PXResult)list[0]).RowCount.Value;
				list.Clear();
			}
			else
			{
				if (list != null)
				{
					needFilterResults = true;

					if (delegateOptions?.IsResultSorted != true)
						SortResult(list, sorts, reverseOrder);
				}

				if (list == null)
				{
					bool isFromCache = false;
					PXCommandKey queryKey = null;
					if (!PXDatabase.ReadDeleted)
					{
						string[] fields = RestrictedFields.Any() ? RestrictedFields.Select(f => f.Field).ToArray() : null;
						queryKey = topCount == 0
							? new PXCommandKey(parameters, searches, sortcolumns, descendings, null, null, filters,
								PXDatabase.ReadBranchRestricted, fields)
							: new PXCommandKey(parameters, searches, sortcolumns, descendings, (reverseOrder ? -1 - startRow : startRow),
								maximumRows, filters, PXDatabase.ReadBranchRestricted, fields);
						////queryKey.Params = key;
						list = LookupCache(queryKey, ref topCount, ref overrideSort, ref needFilterResults);
						isFromCache = list != null;
					}
					if (list == null)
					{
						list = GetResult(parameters, filters, reverseOrder, topCount, sorts, ref overrideSort, ref needFilterResults);

						if (!_LegacyQueryCacheMode)
						{
							PXCache[] caches = null;
							Type[] tables = null;
							if (list.Count > 0 && list[0] is PXResult)
							{
								caches = new PXCache[((PXResult)list[0]).TableCount];
								tables = ((PXResult)list[0]).Tables;
							}
							else
								caches = new PXCache[1];

							caches[0] = Cache;
							for (int i = 1; i < caches.Length; i++)
							{
								caches[i] = _Graph.Caches.GetCache(tables[i]);
							}

							if (queryKey != null)
							{
								SelectQueries.StoreCached(this, queryKey, _IsReadOnly ? list : CloneResult(list), BqlSelect.Context);
							}

							for (int i = 0; i < list.Count; i++)
							{
								var item = list[i];
								if (item is PXResult)
								{
									var res = (PXResult)item;
									bool wasUpdated = false;
									for (int j = 0; j < caches.Length; j++)
									{
										res[j] = caches[j].Select(res[j], _IsReadOnly || j > 0, out wasUpdated); ;
									}
									if (wasUpdated)
									{
										overrideSort = true;
										needFilterResults = true;
									}
								}
								else
								{
									bool wasUpdated = false;
									list[i] = caches[0].Select(item, _IsReadOnly, out wasUpdated);
									if (wasUpdated)
									{
										overrideSort = true;
										needFilterResults = true;
									}

								}
							}
						}
					}
					else
					{
						if (!paramsFromCache)
						{

						}
						overrideSort = false;
						needFilterResults |= !(list is VersionedList) || ((VersionedList)list).AnyMerged;
					}
					bool filterExists = filters != null && filters.Length > 0;
					bool sortReq = overrideSort || HasUnboundSort(sorts);
					bool anyMerged = false;

					PXQueryResult selectQueryResult = null;
					if (queryKey != null)
					{
						SelectQueries.TryGetValue(queryKey, out selectQueryResult);
					}
					// TODO: I think, that code below is incorrect, and should be like this:
					//if (!((BqlSelect.Context != null && BqlSelect.Context.BadParametersSkipMergeCache && !isFromCache) ||
					//      (selectQueryResult != null && selectQueryResult.BadParamsSkipMergeCache)))
					if (!((BqlSelect.Context != null && BqlSelect.Context.BadParametersSkipMergeCache) ||
						  (selectQueryResult != null && selectQueryResult.BadParamsSkipMergeCache)))
						anyMerged = MergeCache(list, parameters, filterExists, ref sortReq);

					needFilterResults |= anyMerged;

					if (sortReq && list.Count > 1 && sorts.Length > 0)
					{
						SortResult(list, sorts, reverseOrder);
					}

					if (queryKey != null && _LegacyQueryCacheMode && (anyMerged || !isFromCache))
					{
						SelectQueries.StoreCached(this, queryKey, list, BqlSelect.Context);
					}
				}


				if (needFilterResults && !FiltersResetRequired && delegateOptions?.IsResultFiltered != true)
				{
					FilterResult(list, filters);
				}

				if (delegateOptions?.IsResultTruncated != true)
				{
					list = SearchResult(list, sorts, reverseOrder, (realTotalRows == -2), ref startRow, maximumRows, ref totalRows,
							out var searchFound);

					bool searchNotFound = !reverseOrder &&
										  (realTotalRows == -2 || realTopCount == 1 || realTopCount == 0 && maximumRows == 1) &&
										  anySearch && !searchFound && list.Count > 0;
					if (searchNotFound)
					{
						list.Clear();
					}
				}


				string viewName;
				bool isAutomationView = _Graph.AutomationView != null
										&& _Graph.ViewNames.TryGetValue(this, out viewName)
										&& viewName == _Graph.AutomationView;

				if (isAutomationView && (maximumRows <= 2 || _Graph._ContextViewDescriptor == null))
					ApplyAutomation(list);
			}

			perf?.SelectTimer.Stop();

			_Select = originalSelect;

			return list;
		}

		public virtual List<object> SelectWindowed(
			object[] currents,
			object[] parameters,
			string[] sortcolumns,
			bool[] descendings,
			int startRow,
			int maximumRows
		) {
			var originalSelect = _Select;

			var perf = PXPerformanceMonitor.CurrentSample;
			if (perf != null) {
				perf.SelectCount++;
				perf.SelectTimer.Start();
			}

			bool overrideSort;
			bool anySearch;
			bool resetTopCount = false;

			PXSearchColumn[] sorts;
			
			//	Analyze Bql command sort columns, override with external sort order, append primary key.
			// as side effect can override _Select with OrderByNew() method.
			sorts = prepareSorts(sortcolumns, descendings, null, maximumRows, out overrideSort, out anySearch, ref resetTopCount);
			parameters = PrepareParameters(currents, parameters);


			List<object> list = null;
			list = GetResultWindowedRO(parameters, sorts, startRow, maximumRows);

			string viewName;
			bool isAutomationView = _Graph.AutomationView != null
									&& _Graph.ViewNames.TryGetValue(this, out viewName)
									&& viewName == _Graph.AutomationView;

			if (isAutomationView && (maximumRows <= 2 || _Graph._ContextViewDescriptor == null))
				ApplyAutomation(list);

			perf?.SelectTimer.Stop();

			_Select = originalSelect;

			return list;
		}

		private void ApplyAutomation(List<object> list)
		{
			//if (_Graph.AutomationView != null)
			//{
			//	string viewName;
			//	if (_Graph.ViewNames.TryGetValue(this, out viewName) && viewName == _Graph.AutomationView)
			//	{
					foreach (object t in list)
					{
						object row = t;
						if (row is PXResult)
							row = ((PXResult) row)[0];

						Cache.Current = row;
					}
					if (!PXAutomation.IsWorkflowExists(_Graph))
						PXAutomation.GetStep(_Graph, list, _Select);

					if (list.Count > 0)
					{
						object row = list[list.Count - 1];
						object item = row;
						if (item is PXResult)
						{
							item = ((PXResult) item)[0];
						}
						Cache.Current = item;
						PXAutomation.ApplyStep(_Graph, row, false);
					}
			//	}
			//}
		}

		private delegate List<object> _InvokeDelegate(Delegate method, object[] parameters);
		private _InvokeDelegate _CustomMethod;
		private delegate object _InstantiateDelegate(object[] parameters);
		private _InstantiateDelegate _CreateInstance;
		private static System.Threading.ReaderWriterLock _CreateInstanceLock = new System.Threading.ReaderWriterLock();
		private static Dictionary<PXCreateInstanceKey, _InstantiateDelegate> _CreateInstanceDict = new Dictionary<PXCreateInstanceKey, _InstantiateDelegate>();

		private void EnsureCreateInstance(Type[] tables)
		{
			if (tables.Length > 1 && _CreateInstance == null)
			{
				
				using (var scope = new PXReaderWriterScope(_CreateInstanceLock))
				{
					scope.AcquireReaderLock();
					PXCreateInstanceKey createkey = new PXCreateInstanceKey(tables);
					if (!_CreateInstanceDict.TryGetValue(createkey, out _CreateInstance))
					{
						scope.UpgradeToWriterLock();
							if (!_CreateInstanceDict.TryGetValue(createkey, out _CreateInstance))
							{
								Type result = null;
								if (!typeof(IBqlTable).IsAssignableFrom(tables[tables.Length - 1]))
								{
									Type[] bqltables = new Type[tables.Length - 1];
									Array.Copy(tables, bqltables, bqltables.Length);
									tables = bqltables;
								}
								switch (tables.Length)
								{
									case 1:
										result = typeof(PXResult<>).MakeGenericType(tables);
										break;
									case 2:
										result = typeof(PXResult<,>).MakeGenericType(tables);
										break;
									case 3:
										result = typeof(PXResult<,,>).MakeGenericType(tables);
										break;
									case 4:
										result = typeof(PXResult<,,,>).MakeGenericType(tables);
										break;
									case 5:
										result = typeof(PXResult<,,,,>).MakeGenericType(tables);
										break;
									case 6:
										result = typeof(PXResult<,,,,,>).MakeGenericType(tables);
										break;
									case 7:
										result = typeof(PXResult<,,,,,,>).MakeGenericType(tables);
										break;
									case 8:
										result = typeof(PXResult<,,,,,,,>).MakeGenericType(tables);
										break;
									case 9:
										result = typeof(PXResult<,,,,,,,,>).MakeGenericType(tables);
										break;
									case 10:
										result = typeof(PXResult<,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 11:
										result = typeof(PXResult<,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 12:
										result = typeof(PXResult<,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 13:
										result = typeof(PXResult<,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 14:
										result = typeof(PXResult<,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 15:
										result = typeof(PXResult<,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 16:
										result = typeof(PXResult<,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 17:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 18:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 19:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 20:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 21:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 22:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 23:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 24:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 25:
										result = typeof(PXResult<,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(tables);
										break;
								}
								DynamicMethod dm;
								if (!PXGraph.IsRestricted)
								{
									dm = new DynamicMethod("_CreateInstance", typeof(object), new Type[] { typeof(object[]) }, typeof(PXView));
								}
								else
								{
									dm = new DynamicMethod("_CreateInstance", typeof(object), new Type[] { typeof(object[]) });
								}
								ILGenerator il = dm.GetILGenerator();
								int[] idx = new int[tables.Length];
								for (int i = 0; i < tables.Length; i++)
								{
									idx[i] = il.DeclareLocal(tables[i]).LocalIndex;
									il.Emit(OpCodes.Ldarg_0);
									il.Emit(OpCodes.Ldc_I4, i);
									il.Emit(OpCodes.Ldelem_Ref);
									if (tables[i].IsValueType)
									{
										il.Emit(OpCodes.Unbox_Any, tables[i]);
									}
									else
									{
										il.Emit(OpCodes.Castclass, tables[i]);
									}
									il.Emit(OpCodes.Stloc, idx[i]);
								}
								for (int i = 0; i < tables.Length; i++)
								{
									il.Emit(OpCodes.Ldloc, idx[i]);
								}
								il.Emit(OpCodes.Newobj, result.GetConstructor(tables));
								il.Emit(OpCodes.Ret);
								_CreateInstanceDict[createkey] = _CreateInstance = (_InstantiateDelegate)dm.CreateDelegate(typeof(_InstantiateDelegate));
							}

						}
				}
	
			}
		}

		private static System.Threading.ReaderWriterLock _InvokeLock = new System.Threading.ReaderWriterLock();
		private static Dictionary<Type, _InvokeDelegate> _InvokeDict = new Dictionary<Type, _InvokeDelegate>();

		private void EnsureDelegate()
		{
			if (_CustomMethod == null)
			{
				
				using (var scope = new PXReaderWriterScope(_InvokeLock))
				{
					scope.AcquireReaderLock();
					Type deltype = _Delegate.GetType();
					if (!_InvokeDict.TryGetValue(deltype, out _CustomMethod))
					{
						scope.UpgradeToWriterLock();

							if (!_InvokeDict.TryGetValue(deltype, out _CustomMethod))
							{
								DynamicMethod dm;
								if (!PXGraph.IsRestricted)
								{
									dm = new DynamicMethod("_CustomMethod", typeof(List<object>), new Type[] { typeof(object), typeof(object[]) }, typeof(PXView), true);
								}
								else
								{
									dm = new DynamicMethod("_CustomMethod", typeof(List<object>), new Type[] { typeof(object), typeof(object[]) }, true);
								}
								ILGenerator il = dm.GetILGenerator();
								ParameterInfo[] pars = _Delegate.Method.GetParameters();
								int[] idx = new int[pars.Length];
								bool byRef = (_Delegate.Method.ReturnType == typeof(void));
								for (int i = 0; i < pars.Length; i++)
								{
									Type pt = byRef ? pars[i].ParameterType.GetElementType() : pars[i].ParameterType;
									idx[i] = il.DeclareLocal(pt).LocalIndex;
									il.Emit(OpCodes.Ldarg_1);
									il.Emit(OpCodes.Ldc_I4, i);
									il.Emit(OpCodes.Ldelem_Ref);
									if (pt.IsValueType)
									{
										il.Emit(OpCodes.Unbox_Any, pt);
									}
									else
									{
										il.Emit(OpCodes.Castclass, pt);
									}
									il.Emit(OpCodes.Stloc, idx[i]);
								}
								LocalBuilder lb = il.DeclareLocal(typeof(List<object>));
								il.Emit(OpCodes.Ldarg_0);
								il.Emit(OpCodes.Castclass, deltype);
								for (int i = 0; i < pars.Length; i++)
								{
									if (byRef)
									{
										il.Emit(OpCodes.Ldloca_S, idx[i]);
									}
									else
									{
										il.Emit(OpCodes.Ldloc_S, idx[i]);
									}
								}
								il.Emit(OpCodes.Callvirt, deltype.GetMethod("Invoke"));
								if (byRef)
								{
									il.Emit(OpCodes.Ldnull);
									il.Emit(OpCodes.Stloc, lb.LocalIndex);
								}
								else
								{
									//LocalBuilder lr = il.DeclareLocal(typeof(IEnumerable));
									//il.Emit(OpCodes.Stloc, lr.LocalIndex);
									//il.Emit(OpCodes.Ldloc, lr.LocalIndex);
									//System.Reflection.Emit.Label retrive = il.DefineLabel();
									//il.Emit(OpCodes.Ldnull);
									//il.Emit(OpCodes.Ceq);
									//il.Emit(OpCodes.Brfalse_S, retrive);
									//il.Emit(OpCodes.Ldnull);
									//il.Emit(OpCodes.Ret);
									//il.MarkLabel(retrive);
									//il.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructor(new Type[0]));
									//il.Emit(OpCodes.Stloc, lb.LocalIndex);
									//LocalBuilder le = il.DeclareLocal(typeof(IEnumerator));
									//LocalBuilder lo = il.DeclareLocal(typeof(object));
									//il.Emit(OpCodes.Ldloc, lr.LocalIndex);
									//il.Emit(OpCodes.Callvirt, typeof(IEnumerable).GetMethod("GetEnumerator"));
									//il.Emit(OpCodes.Stloc, le.LocalIndex);
									//System.Reflection.Emit.Label next = il.DefineLabel();
									//il.Emit(OpCodes.Br_S, next);
									//System.Reflection.Emit.Label load = il.DefineLabel();
									//il.MarkLabel(load);
									//il.Emit(OpCodes.Ldloc, le.LocalIndex);
									//il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetProperty("Current").GetGetMethod());
									//il.Emit(OpCodes.Stloc, lo.LocalIndex);
									//il.Emit(OpCodes.Ldloc, lb.LocalIndex);
									//il.Emit(OpCodes.Ldloc, lo.LocalIndex);
									//il.Emit(OpCodes.Callvirt, typeof(List<object>).GetMethod("Add", new Type[] { typeof(object) }));
									//il.MarkLabel(next);
									//il.Emit(OpCodes.Ldloc, le.LocalIndex);
									//il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
									//il.Emit(OpCodes.Brtrue_S, load);

									il.Emit(OpCodes.Call, typeof(PXView).GetMethod(nameof(ToList), BindingFlags.Static | BindingFlags.NonPublic));
									il.Emit(OpCodes.Stloc, lb.LocalIndex);
								}
								if (byRef)
								{
									for (int i = 0; i < pars.Length; i++)
									{
										Type pt = pars[i].ParameterType.GetElementType();
										il.Emit(OpCodes.Ldarg_1);
										il.Emit(OpCodes.Ldc_I4, i);
										il.Emit(OpCodes.Ldloc, idx[i]);
										if (pt.IsValueType)
										{
											il.Emit(OpCodes.Box, pt);
										}
										il.Emit(OpCodes.Stelem_Ref);
									}
								}
								il.Emit(OpCodes.Ldloc, lb.LocalIndex);
								il.Emit(OpCodes.Ret);
								_InvokeDict[deltype] = _CustomMethod = (_InvokeDelegate)dm.CreateDelegate(typeof(_InvokeDelegate));
							}

						}
				}

			}
		}

		protected static List<object> ToList(IEnumerable src)
		{
			if (src == null)
				return null;
			
			if (src is PXDelegateResult delegateResult)
			{
				PXDelegateResult copy = new PXDelegateResult();
				copy.Capacity = delegateResult.Capacity;
				copy.AddRange(delegateResult);
				copy.IsResultFiltered = delegateResult.IsResultFiltered;
				copy.IsResultSorted = delegateResult.IsResultSorted;
				copy.IsResultTruncated = delegateResult.IsResultTruncated;

				return copy;
			}

			if (src is PXDelegateCacheResult cacheResult)
				return cacheResult;

			if (src is List<object> ret)
				return new List<object>(ret);

			//if (PXView.MaximumRows > 0)
			//	return src.OfType<object>().Take(PXView.MaximumRows).ToList();
			//else
				return src.OfType<object>().ToList();

		}

		/// <summary>Invokes the manual method if provided in the constructor</summary>
		/// <param name="parameters">Query parameters</param>
		/// <returns>Either resultset or null, if the method is intended to override parameters only</returns>
		protected virtual List<object> InvokeDelegate(object[] parameters)
		{
			List<object> list = null;
			if (_Delegate != null)
			{
				ParameterInfo[] pi = _Delegate.Method.GetParameters();
				IBqlParameter[] pb = _Select.GetParameters();
				object[] vals = new object[pi.Length];
				if (parameters != null)
				{
					int j = 0;
					for (int i = 0; i < pb.Length && j < pi.Length; i++)
					{
						if (pb[i].IsArgument)
						{
							if (i < parameters.Length)
							{
								vals[j] = parameters[i];
							}
							else
							{
								vals[j] = null;
							}
							j++;
						}
					}
					int k = pb.Length;
					for (; j < vals.Length; j++)
					{
						if (k < parameters.Length)
						{
							vals[j] = parameters[k];
							k++;
						}
						else
						{
							vals[j] = null;
						}
					}
				}
				EnsureDelegate();
				list = _CustomMethod(_Delegate, vals);
				if (parameters != null)
				{
					int j = 0;
					for (int i = 0; i < pb.Length && j < pi.Length; i++)
					{
						if (pb[i].IsArgument)
						{
							if (i < parameters.Length)
							{
								parameters[i] = vals[j];
							}
							j++;
						}
					}
					int k = pb.Length;
					for (; j < vals.Length; j++)
					{
						if (k < parameters.Length)
						{
							parameters[k] = vals[j];
							k++;
						}
						else
						{
							break;
						}
					}
				}
			}
			return list;
		}

		protected delegate int compareDelegate(object a, object b);
		protected int CompareMethod(object a, object b, PXCache cache, string fieldName, bool descending, bool useExt, Type tableType, PXCollationComparer collationComparer, Dictionary<string, string> valueLabelDic, ref ISqlDialect dialect)
		{
			if (tableType != null)
			{
				a = ((PXResult)a)[tableType];
				b = ((PXResult)b)[tableType];
			}
			object aVal;
			if (!useExt)
			{
				aVal = cache.GetValue(a, fieldName);
			}
			else
			{
				aVal = cache.GetValueExt(a, fieldName);
				if (aVal is PXFieldState)
				{
					aVal = ((PXFieldState)aVal).Value;
				}
				if (valueLabelDic != null && valueLabelDic.Count > 0 && aVal is string)
				{
					string aStrVal;
					if (valueLabelDic.TryGetValue((string) aVal, out aStrVal))
					{
						aVal = aStrVal;
					}
					else
					{
						aVal = null;
					}
				}
			}
			if (!(aVal is IComparable) && aVal != null)
			{
				if (descending)
				{
					return 1;
				}
				return -1;
			}
			object bVal;
			if (!useExt)
			{
				bVal = cache.GetValue(b, fieldName);
			}
			else
			{
				bVal = cache.GetValueExt(b, fieldName);
				if (bVal is PXFieldState)
				{
					bVal = ((PXFieldState)bVal).Value;
				}
				if (valueLabelDic != null && valueLabelDic.Count > 0 && bVal is string)
				{
					string bStrVal;
					if (valueLabelDic.TryGetValue((string) bVal, out bStrVal))
					{
						bVal = bStrVal;
					}
					else
					{
						bVal = null;
					}
				}
			}
			if (aVal == null && bVal == null) return 0;
			// TODO: needs review

			int result;
			if (aVal == null)
				result = -1;
			else if (bVal == null)
				result = 1;
			else if (aVal is string && bVal is String)
				result = collationComparer.Compare((string)aVal, (string)bVal);
			else if (aVal is Guid && bVal is Guid)
			{
				if (dialect == null)
				{
					dialect = cache.Graph.SqlDialect;
				}
				result = compareGuid((Guid)aVal, (Guid)bVal, dialect);
			}
			else
				result = ((IComparable)aVal).CompareTo(bVal);

			return descending ? -result : result;
		}

		protected delegate int searchDelegate(object a);
		protected int SearchMethod(object a, object bVal, PXCache cache, string fieldName, bool descending, bool useExt, Type tableType, Dictionary<string, string> valueLabelDic, ref ISqlDialect dialect)
		{
			if (tableType != null)
			{
				a = ((PXResult)a)[tableType];
			}
			object aVal;
			if (!useExt)
			{
				aVal = cache.GetValue(a, fieldName);
			}
			else
			{
				aVal = cache.GetValueExt(a, fieldName);
				if (aVal is PXFieldState)
				{
					aVal = ((PXFieldState)aVal).Value;
				}
			}
			if (!(aVal is IComparable))
			{
				if (descending)
				{
					return 1;
				}
				return -1;
			}
			int ret;
			if (aVal is string && bVal is String)
			{
				if (useExt && valueLabelDic != null)
				{
					string strVal;
					if (valueLabelDic.TryGetValue((string)aVal, out strVal))
					{
						aVal = strVal;
					}
					if (valueLabelDic.TryGetValue((string)bVal, out strVal))
					{
						bVal = strVal;
					}
				}
				ret = PXLocalesProvider.CollationComparer.Compare((string)aVal, (string)bVal);
			}
			else if (aVal is Guid && bVal is Guid)
			{
				if (dialect == null)
				{
					dialect = cache.Graph.SqlDialect;
				}
				ret = compareGuid((Guid)aVal, (Guid)bVal, dialect);
			}
			else
			{
				ret = ((IComparable)aVal).CompareTo(bVal);
			}
			if (descending && ret != 0)
			{
				ret = -ret;
			}
			return ret;
		}

		/// <summary>Compare two items</summary>
		/// <param name="a">First item, might be a dictionary</param>
		/// <param name="b">Second item, might be a dictionary</param>
		/// <param name="columns">Sort columns</param>
		/// <param name="descendings">Sort descendings</param>
		/// <returns>-1, 0, 1</returns>
		protected virtual int Compare(object a, object b, compareDelegate[] comparisons)
		{
			if (Object.ReferenceEquals(a, b))
			{
				return 0;
			}
			for (int i = 0; i < comparisons.Length; i++)
			{
				int result = comparisons[i](a, b);
				if (result != 0)
				{
					return result;
				}
			}
			return 0;
		}

		protected virtual int Search(object a, searchDelegate[] comparisons)
		{
			for (int i = 0; i < comparisons.Length; i++)
			{
				int result = comparisons[i](a);
				if (result != 0)
				{
					return result;
				}
			}
			return 0;
		}

		/// <summary>Retrieves the whole data set corresponding to the BQL command.</summary>
		/// <param name="parameters">The explicit values for such parameters as <tt>Required</tt>, <tt>Optional</tt>, and <tt>Argument</tt>.</param>
		public virtual List<object> SelectMulti(params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			return Select(null, parameters, null, null, null, null, ref startRow, 0, ref totalRows);
		}

		/// <summary>Retrieves the top data record from the data set corresponding to the BQL command.</summary>
		/// <param name="parameters">The explicit values for such parameters as
        /// <tt>Required</tt>, <tt>Optional</tt>, and <tt>Argument</tt>.</param>
		public virtual object SelectSingle(params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			List<object> list = Select(null, parameters, null, null, null, null, ref startRow, 1, ref totalRows);
			if (list.Count == 0)
			{
				return null;
			}
			return list[0];
		}

		/// <summary>Retrieves the top data record from the data set corresponding to the BQL command.</summary>
		/// <param name="currents">The objects to use as current data records when
        /// processing <tt>Current</tt> and <tt>Optional</tt> parameters.</param>
		/// <param name="parameters">The explicit values for such parameters as
        /// <tt>Required</tt>, <tt>Optional</tt>, and <tt>Argument</tt>.</param>
		/// <returns>The resultset.</returns>
		public virtual object SelectSingleBound(object[] currents, params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			List<object> list = Select(currents, parameters, null, null, null, null, ref startRow, 1, ref totalRows);
			if (list.Count == 0)
			{
				return null;
			}
			return list[0];
		}
		protected bool _IsCommandMutable;
		/// <summary>Retrieves the whole data set corresponding to the BQL command.</summary>
		/// <param name="currents">The objects to use as current data records when
        /// processing <tt>Current</tt> and <tt>Optional</tt> parameters.</param>
		/// <param name="parameters">The explicit values for such parameters as
        /// <tt>Required</tt>, <tt>Optional</tt>, and <tt>Argument</tt>.</param>
		public virtual List<object> SelectMultiBound(object[] currents, params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			return Select(currents, parameters, null, null, null, null, ref startRow, 0, ref totalRows);
		}

		/// <summary>Appends a filtering expression to the underlying BQL command
		/// via the logical "and". The additional filtering expression is provided
		/// in the type parameter.</summary>
		public void WhereAnd<TWhere>()
			where TWhere : IBqlWhere, new()
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereAnd<TWhere>();
		}
		/// <summary>Appends a filtering expression to the underlying BQL command
		/// via the logical "and". The additional filtering expression is provided
		/// in the type parameter.</summary>
		/// <param name="where">The additional filtering expression as the type
		/// derived from <tt>IBqlWhere</tt>.</param>
		public void WhereAnd(Type where)
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereAnd(where);
		}
		/// <summary>Appends a filtering expression to the BQL statement via the
		/// logical "or". The additional filtering expression is provided in the
		/// type parameter.</summary>
		public void WhereOr<TWhere>()
			where TWhere : IBqlWhere, new()
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereOr<TWhere>();
		}
		/// <summary>Appends a filtering expression to the BQL statement via the
		/// logical "or".</summary>
		/// <param name="where">The additional filtering expression as the type
		/// derived from <tt>IBqlWhere</tt>.</param>
		public void WhereOr(Type where)
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereOr(where);
		}
		/// <summary>Replaces the sorting expression with the new sorting
		/// expression. The sorting expressio is specified in the type
		/// parameter.</summary>
		public void OrderByNew<newOrderBy>()
			where newOrderBy : IBqlOrderBy, new()
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_Select = _Select.OrderByNew<newOrderBy>();

			bool needOverrideSort;
			bool anySearch;
			bool resetTopCount = false;
			PXSearchColumn[] sorts = prepareSorts(null, null, null, 0, out needOverrideSort, out anySearch, ref resetTopCount);
			PXView.Sorts.Clear();
			PXView.Sorts.Add(sorts);
		}
		/// <summary>Replaces the sorting expression with the new sorting
		/// expression.</summary>
		/// <param name="newOrderBy">The sorting expression as a type derived from
		/// <tt>IBqlOrderBy</tt>, such as <tt>OrderBy&lt;&gt;</tt>.</param>
		public void OrderByNew(Type newOrderBy)
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_Select = _Select.OrderByNew(newOrderBy);

			bool needOverrideSort;
			bool anySearch;
			bool resetTopCount = false;
			PXSearchColumn[] sorts = prepareSorts(null, null, null, 0, out needOverrideSort, out anySearch, ref resetTopCount);
			PXView.Sorts.Clear();
			PXView.Sorts.Add(sorts);
		}
		/// <summary>Appends the provided join clause to the BQL command. The join
		/// clause is specified in the type parameter.</summary>
		public void Join<join>()
			where join : IBqlJoin, new()
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = BqlCommand.AppendJoin<join>(_Select);
		}
		/// <summary>Appends the provided join clause to the BQL
		/// command.</summary>
		/// <param name="join">The join clause as a type derived from
		/// <tt>IBqlJoin</tt>.</param>
		public void Join(Type join)
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = (BqlCommand)Activator.CreateInstance(BqlCommand.AppendJoin(_Select.GetSelectType(), join));
		}
		/// <summary>Appends or sets the provided join clause to the BQL command.</summary>
		/// <param name="join">The join clause as a type derived from
        /// <tt>IBqlJoin</tt>.</param>
		public void JoinNew(Type join)
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_CreateInstance = null;
			_Select = (BqlCommand)Activator.CreateInstance(BqlCommand.NewJoin(_Select.GetSelectType(), join));
		}
		/// <summary>Adds logical "not" to the whole <tt>Where</tt> clause of the BQL statement, reversing the condition to the opposite.</summary>
		public void WhereNot()
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereNot();
		}
		/// <summary>Replaces the filtering expression in the BQL statement. The
		/// new filtering expression is provided in the type parameter.</summary>
		public void WhereNew<newWhere>()
			where newWhere : IBqlWhere, new()
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereNew<newWhere>();
		}
		/// <summary>Replaces the filtering expression in the BQL
		/// statement.</summary>
		/// <param name="newWhere">The new filtering expression as the type
		/// derived from <tt>IBqlWhere</tt>.</param>
		public void WhereNew(Type newWhere)
		{
			if (IsNonStandardView) Clear(); else _SelectQueries = null;
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereNew(newWhere);
		}

		/// <summary>
		/// The event handler for the <tt>RefreshRequested</tt> event.
		/// </summary>
		/// <exclude/>
		public EventHandler RefreshRequested;
		protected void OnRefreshRequested(object sender, EventArgs e)
		{
			if (RefreshRequested != null)
			{
				RefreshRequested(sender, e);
			}
		}
		/// <summary>Raises the <tt>RequestRefresh</tt> event defined within the
		/// <tt>PXView</tt> object.
		/// The method refreshes only grid control and may be called from RowSelected, RowInserted, RowUpdated and RowDeleted events.
		/// </summary>
		public void RequestRefresh()
		{
			OnRefreshRequested(this, new EventArgs());
		}

		/// <summary>Gets or sets the value indicating user's choice in the dialog window displayed through one of the <tt>Ask()</tt> methods.</summary>
		public WebDialogResult Answer
		{
			get
			{
				return GetAnswer(string.Empty);
			}
			set
			{
				SetAnswer(string.Empty, value);
			}
		}

		/// <summary>Returns the result of the dialog window that was opened through one of the <tt>Ask()</tt> methods and saved in the <tt>PXView</tt> object.</summary>
		/// <param name="key">The identifier of the dialog window that was
        /// provided to the <tt>Ask()</tt> method or the name of the data
        /// view.</param>
		public WebDialogResult GetAnswer(string key)
		{
			return DialogManager.GetAnswer(this, key);
		}

		/// <summary>Saves the result of the dialog window.</summary>
		/// <param name="key">The identifier of the dialog window.</param>
		/// <param name="answer">The result value.</param>
		public void SetAnswer(string key, WebDialogResult answer)
		{
			DialogManager.SetAnswer(this, key, answer);
		}

		/// <summary>Allows to get or set the values indicating user's choice in the dialog window displayed through one of the <tt>Ask()</tt> methods.</summary>
		public AnswerIndexer Answers { get; }
		public class AnswerIndexer
		{
			private readonly PXView _view;
			internal AnswerIndexer(PXView view) { _view = view; }

			/// <summary>The result of the dialog window that was opened
			/// through one of the <tt>Ask()</tt> methods and saved in the
			/// <tt>PXView</tt> object.</summary>
			/// <param name="key">The identifier of the dialog window that was
			/// provided to the <tt>Ask()</tt> method or the name of the data
			/// view.</param>
			public WebDialogResult this[string key]
			{
				get { return _view.GetAnswer(key); }
				set { _view.SetAnswer(key, value); }
			}
		}

		/// <summary>Saves the result of the dialog window.</summary>
		/// <param name="graph">The graph with which the data view is
		/// associated.</param>
		/// <param name="viewName">The name of the data view with which the dialog
		/// window is associated.</param>
		/// <param name="key">The identifier of the dialog window.</param>
		/// <param name="answer">The result value.</param>
		public static void SetAnswer(PXGraph graph, string viewName, string key, WebDialogResult answer)
		{
			DialogManager.SetAnswer(graph, viewName, key, answer);
		}

		/// <summary>Clears the dialog information saved by the graph on last invocation of the <tt>Ask()</tt> method.</summary>
		public void ClearDialog()
		{
			DialogManager.Clear(this.Graph);
		}

		/// <exclude />
		public WebDialogResult Ask(string key, object row, string header, string message, MessageButtons buttons, MessageIcon icon, bool refreshRequired)
		{
			return DialogManager.Ask(this, key, row, header, message, buttons, icon, refreshRequired);
		}

		/// <exclude />
		public WebDialogResult Ask(string key, object row, string header, string message, MessageButtons buttons, MessageIcon icon)
		{
			return Ask(key, row, header, message, buttons, icon, false);
		}

		/// <exclude />
		public WebDialogResult Ask(object row, string header, string message, MessageButtons buttons, MessageIcon icon, bool refreshRequired)
		{
			return Ask(null, row, header, message, buttons, icon, refreshRequired);
		}

		/// <exclude />
		public WebDialogResult Ask(object row, string header, string message, MessageButtons buttons, MessageIcon icon)
		{
			return Ask(null, row, header, message, buttons, icon);
		}

		/// <exclude />
		public WebDialogResult Ask(object row, string header, string message, MessageButtons buttons, IReadOnlyDictionary<WebDialogResult, string> buttonNames, MessageIcon icon)
		{
			return DialogManager.Ask(this, null, row, header, message, buttons, buttonNames, icon, false);
		}

		/// <summary>Displays the dialog box (panel) with single or multiple choices for the user.
		/// </summary>
		/// <param name="key">The identifier of the panel to display.</param>
		/// <param name="message">The string displayed as the message inside the dialog box.</param>
		/// <param name="buttons">The value from the <see cref="MessageButtons">MessageButtons</see> enumeration that indicates which set of buttons to display in the dialog box.</param>
		/// <param name="refreshRequired">The value that indicates whether the dialog box should be repainted or displayed as it was cached. If <tt>true</tt>, the dialog box is repainted.</param>
		public WebDialogResult Ask(string key, string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(key, null, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		/// <summary>Displays the dialog box (panel) with single or multiple choices for the user.</summary>
		/// <param name="key">The identifier of the panel to display.</param>
		/// <param name="message">The string displayed as the message inside the dialog box.</param>
		/// <param name="buttons">The value from the <see cref="MessageButtons">MessageButtons</see> enumeration that indicates which set of buttons to display in the dialog box.</param>
		public WebDialogResult Ask(string key, string message, MessageButtons buttons)
		{
			return Ask(key, null, String.Empty, message, buttons, MessageIcon.None);
		}

		/// <summary>Displays the dialog box with single or multiple choices for the user. The dialog box can be repainted or displayed as it was cached.</summary>
		/// <param name="message">The string displayed as the message inside the dialog box.</param>
		/// <param name="buttons">The value from the <see cref="MessageButtons">MessageButtons</see> enumeration
		/// that indicates which set of buttons to display in the dialog
		/// window.</param>
		/// <param name="refreshRequired">The value that indicates whether the
		/// dialog should be repainted or displayed as it was cached. If
		/// <tt>true</tt>, the dialog is repainted.</param>
		public WebDialogResult Ask(string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(null, null, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		/// <summary>Displays the dialog box with single or multiple choices for the user.</summary>
		/// <param name="message">The string displayed as the message inside the dialog box.</param>
		/// <param name="buttons">The value from the <see cref="MessageButtons">MessageButtons</see> enumeration
		/// that indicates which set of buttons to display in the dialog
		/// window.</param>
		public WebDialogResult Ask(string message, MessageButtons buttons)
		{
			return Ask(null, null, String.Empty, message, buttons, MessageIcon.None);
		}

		/// <exclude />
		public WebDialogResult Ask(string key, object row, string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(key, row, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		/// <exclude />
		public WebDialogResult Ask(string key, object row, string message, MessageButtons buttons)
		{
			return Ask(key, row, String.Empty, message, buttons, MessageIcon.None);
		}

		/// <exclude />
		public WebDialogResult Ask(object row, string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(null, row, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		/// <exclude />
		public WebDialogResult Ask(object row, string message, MessageButtons buttons)
		{
			return Ask(null, row, String.Empty, message, buttons, MessageIcon.None);
		}

		public delegate void InitializePanel(PXGraph graph, string viewName);

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control.</summary>
		/// <param name="key">The identifier of the panel to display.</param>
		/// <param name="refreshRequired">The value that indicates whether the
		/// dialog should be repainted or displayed as it was cached. If
		/// <tt>true</tt>, the dialog is repainted.</param>
		public WebDialogResult AskExt(string key, bool refreshRequired)
		{
			return DialogManager.AskExt(this, key, null, refreshRequired);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control. The method requests repainting of the
		/// panel.</summary>
		/// <param name="key">The identifier of the panel to display.</param>
		public WebDialogResult AskExt(string key)
		{
			return AskExt(key, false);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control. As a key, the method uses the name of
		/// the variable that holds the BQL statement.</summary>
		/// <param name="refreshRequired">The value that indicates whether the
		/// dialog should be repainted or displayed as it was cached. If
		/// <tt>true</tt>, the dialog is repainted.</param>
		public WebDialogResult AskExt(bool refreshRequired)
		{
			return DialogManager.AskExt(this, null, null, refreshRequired);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control. As a key, the method uses the name of
		/// the variable that holds the BQL statement. The method requests
		/// repainting of the panel.</summary>
		public WebDialogResult AskExt()
		{
			return AskExt(false);
		}

		public WebDialogResult AskExtWithHeader(string header, params string[] commitFields)
		{
			return AskExtWithHeader(header, commitFields.ToList());
		}
		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control. Header is displayed as a panel title</summary>
		public WebDialogResult AskExtWithHeader(string header, List<string> commitFields = null)
		{
			return DialogManager.AskExt(this, header, commitFields);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control. Header is displayed as a panel title</summary>
		public WebDialogResult AskExtWithHeader(string header, InitializePanel initializeHandler, List<string> commitFields = null)
		{
			return DialogManager.AskExt(this, header, new DialogManager.InitializePanel(initializeHandler), commitFields);
		}
		
		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control. As a key, the method uses the name of
		/// the variable that holds the BQL statement. The method requests
		/// repainting of the panel.</summary>
		public WebDialogResult AskExt(MessageButtons buttons)
		{
			return DialogManager.AskExt(this, buttons);
		}
		
		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control.</summary>
		/// <param name="key">The identifier of the panel to display.</param>
		/// <param name="initializeHandler">The delegate of the method that is
		/// called before the dialog is displayed.</param>
		/// <param name="refreshRequired">The value that indicates whether the
		/// dialog should be repainted or displayed as it was cached. If
		/// <tt>true</tt>, the dialog is repainted.</param>
		public WebDialogResult AskExt(string key, InitializePanel initializeHandler, bool refreshRequired)
		{
			return DialogManager.AskExt(this, key, new DialogManager.InitializePanel(initializeHandler), refreshRequired);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control.</summary>
		/// <param name="key">The identifier of the panel to display.</param>
		/// <param name="initializeHandler">The delegate of the method that is
		/// called before the dialog is displayed.</param>
		public WebDialogResult AskExt(string key, InitializePanel initializeHandler)
		{
			return AskExt(key, initializeHandler, false);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control.</summary>
		/// <param name="initializeHandler">The delegate of the method that is
		/// called before the dialog is displayed.</param>
		/// <param name="refreshRequired">The value that indicates whether the
		/// dialog should be repainted or displayed as it was cached. If
		/// <tt>true</tt>, the dialog is repainted.</param>
		public WebDialogResult AskExt(InitializePanel initializeHandler, bool refreshRequired)
		{
			return DialogManager.AskExt(this, null, new DialogManager.InitializePanel(initializeHandler), refreshRequired);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control.</summary>
		/// <param name="initializeHandler">The delegate of the method that is
		/// called before the dialog is displayed.</param>
		public WebDialogResult AskExt(InitializePanel initializeHandler)
		{
			return AskExt(initializeHandler, false);
		}

		/// <summary>Displays the dialog window configured by the
		/// <tt>PXSmartPanel</tt> control.</summary>
		/// <param name="graph">The graph where the data view is defined.</param>
		/// <param name="viewName">The name of the data view with which the dialog
		/// is associated.</param>
		/// <param name="key">The identifier of the panel to display.</param>
		/// <param name="initializeHandler">The delegate of the method that is
		/// called before the dialog is displayed.</param>
		public static WebDialogResult AskExt(PXGraph graph, string viewName, string key, InitializePanel initializeHandler)
		{
			return DialogManager.AskExt(graph, viewName, key, new DialogManager.InitializePanel(initializeHandler), false);
		}

		/// <summary>Gets the value of the specified field in the data record from the cache.</summary>
		/// <param name="sender">The cache object.</param>
		/// <param name="data">The data record.</param>
        /// <param name="sourceType">The DAC of the data record. The cache of this
        /// DAC type is obtained through the cache object provided in the
        /// parameter.</param>
		/// <param name="sourceField">The name of the field which value is
        /// returned.</param>
		/// <remarks>The method may raise the <tt>FieldDefaulting</tt> and
		/// <tt>FieldUpdating</tt> events.</remarks>
		public static object FieldGetValue(PXCache sender, object data, Type sourceType, string sourceField)
		{
			object newValue;
			PXCache sourceCache = sender.Graph.Caches[sourceType];

			if (InheritsType(sender.GetItemType(), sourceType))
			{
				newValue = sender.GetValue(data, sourceField);
				if (newValue == null)
				{
					if (sender.RaiseFieldDefaulting(sourceField, data, out newValue))
					{
						sender.RaiseFieldUpdating(sourceField, data, ref newValue);
					}
				}
			}
			else if ((sourceCache._Current ?? sourceCache.Current) == null)
			{
				object newRow = Activator.CreateInstance(sourceType);
				if (sourceCache.RaiseFieldDefaulting(sourceField, newRow, out newValue))
				{
					sourceCache.RaiseFieldUpdating(sourceField, newRow, ref newValue);
				}
			}
			else
			{
				newValue = sourceCache.GetValue((sourceCache._Current ?? sourceCache.Current), sourceField);
			}
			return newValue;
		}
		private static bool InheritsType(Type ChildType, Type BaseType)
		{
			while (ChildType != null && ChildType != BaseType)
			{
				ChildType = ChildType.BaseType;
			}
			return (ChildType == BaseType);
		}

		/// <summary>View name in the graph.</summary>
		public string Name
		{
			get
			{
				string viewName;
				if (Graph != null && Graph.ViewNames.TryGetValue(this, out viewName))
					return viewName;
				return null;
			}
		}

		/// <exclude />
		public object CreateResult(object[] items)
		{
			if (_CreateInstance == null)
				EnsureCreateInstance(_Select.GetTables());
			return _CreateInstance != null
				? (PXResult)_CreateInstance(items)
				: null;
		}
	}

	public class PXViewParameter
	{
		public string Name;
		public int Ordinal;
		public IBqlParameter Bql;
		public ParameterInfo Argument;

	}

	public interface IPXDelegateResult 
	{
		bool IsResultSorted { get; set; }
		bool IsResultFiltered { get; set; }
		bool IsResultTruncated { get; set; }
		
	}

	public class PXDelegateResult : List<object>, IPXDelegateResult
	{
		public bool IsResultSorted { get; set; }
		public bool IsResultFiltered { get; set; }
		public bool IsResultTruncated { get; set; }
	}

	public interface IPXDelegateCacheResult
	{
		bool IsResultCachable { get; set; }
		object[] CacheKeys { get; set; }
		string[] SqlTables { get; set; }
		void EmitRows();
	}

	public class PXDelegateCacheResult : List<object>, IPXDelegateCacheResult
	{
		public bool IsResultCachable { get; set; }

		public Action OnEmitRows;
	   
		void IPXDelegateCacheResult.EmitRows()
		{
			OnEmitRows();
		}

		public object[] CacheKeys { get; set; }
		
		public string[] SqlTables { get; set; }
	}

	public static class SelectHelper
	{
		//private PXView View;
		//public IEnumerable SelectLazy()
		//{
		//    List<object> list = null;
		//    while (true)
		//    {
		//        var last = list.Last();
		//        var searches = last.
		//        list = View.Select(0, 100);
		//    }



		//}


		public static PXDelegateCacheResult ApplyViewConstraints(this PXSelectBase select)
	   
		{
			return new PXDelegateCacheResult();
		}


		/// <summary>
		/// delegate can return the same RowType as internal delegate select( can reffer to view type)
		/// if not -> mapping is used, what can we do?
		/// if yes -> we should ajust internal select in the same way pxview.select does 
		/// -sort, filter, search, 
		/// if mapping is one to one, we can also apply offset and truncate
		/// in case of filter - implement lazy select, delegate enumerator must stop on row count + offset somehow (yeild return is fine) or special exception
		/// also, can create sql side filter based on conditions from delegate
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="selectBase"></param>
		/// <returns></returns>
		//public static PXSelectBase Restrict<T>(this PXSelect<T> selectBase)
		//    where T : class, IBqlTable, new()
		//{
		//    var ret = new PXSelect<T>(selectBase.View.Graph);
		//    var view = new PXView(graph, isReadOnly, bqlCommand);
		//    var startRow = PXView.StartRow;
		//    int totalRows = 0;
		//    if (PXView.CurrentRestrictedFields.Any())
		//    {
		//        view.RestrictedFields = PXView.CurrentRestrictedFields;
		//    }
		//    var list = view.Select(PXView.Currents, parameters ?? PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, filters ?? PXView.Filters,
		//        ref startRow, PXView.MaximumRows, ref totalRows);
		//    PXView.StartRow = 0;
		//    return list;
		//}
	}

	public class GraphWithDelegates : PXGraph<GraphWithDelegates>
	{
		public PXSelect<SYMapping> Items;
		protected virtual IEnumerable items()
		{
			var result = new PXDelegateResult
			{
				IsResultFiltered = true,
				IsResultTruncated = true,
				IsResultSorted = true
			};

			foreach (var r in this.SelectWithinContext(Items.View.BqlSelect))
			{
				result.Add(r);
			}

			return result;

		}


		public PXSelect<SYMapping> ItemsCached;

		protected virtual IEnumerable itemsCached()
		{
			var parameters = new object[] {"XXX"};

			var result = new PXDelegateCacheResult
			{
				IsResultCachable = true,

				CacheKeys = parameters,



			};


			result.OnEmitRows = delegate
			{
				foreach (var r in PXSelect<SYMapping>.Select(this, parameters))
				{
					result.Add(r);
				}

			};


			return result;

		}
	}

	public class PXNonSqlView<TTable, OrderBy> : PXNonSqlView<TTable>
		where TTable : class, IBqlTable, new()
		where OrderBy : IBqlOrderBy, new()
	{
		public PXNonSqlView(PXGraph graph) : base(graph)
		{
			this.View = new PXView(graph, false, GetCommand().OrderByNew(typeof(OrderBy)), (PXSelectDelegate) Getter)
				{CacheType = typeof(TTable)};
		}
	}

	public class PXNonSqlView<TTable> : PXSelect<TTable>, IPXSelectInterceptor
		where TTable : class, IBqlTable, new()
	{
		public Func<bool, IList> GetCollection;

		public IEnumerable<TTable> SelectResult()
		{
			foreach (TTable row in Getter())
			{
				yield return row;
			}
		}
		protected virtual IEnumerable Getter()
		{
			if (GetCollection == null)
			{
				throw new PXException($"PXNonSqlView source is null:{typeof(TTable).FullName}");
			}
			var lst = GetCollection(false);
			if (lst == null)
				yield break;

			foreach (var src in lst)
			{
				if (Cache.Locate(src) == null)
					Cache.SetStatus(src, PXEntryStatus.Held);
			}

			foreach (object row in Cache.Cached)
			{
				PXEntryStatus status = Cache.GetStatus(row);

				if (status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted)
					continue;

				yield return row;
			}

			//foreach (object row in Cache.Cached)
			//{
			//    PXEntryStatus status = Cache.GetStatus(row);

			//    if (status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted)
			//        continue;

			//    yield return row;
			//}
		}

		public PXNonSqlView(PXGraph graph) : base(graph)
		{
			this.View = new PXView(graph, false, GetCommand(), (PXSelectDelegate)Getter) { CacheType = typeof(TTable) };
		  //  graph.OnBeforePersist += copySourcePassToInterceptor;
			
			graph.Caches.SubscribeCacheCreated<TTable>(delegate
			{
				var interceptor= new Interceptor
				{
					// TODO: there is one potential problem - if GetCollection delegate will change original source due to filters or some logic
					// or just because it uses LINQ - than all modify operations will fail, because we would change not original source
					Source = b => this.GetCollection(b), 
					ItemType = GetItemType()
				};
				this.Cache.Interceptor = interceptor;
				this.Cache.SelectInterceptor = this;
				//this.Cache.AllowInsert = false;
				this.Cache.DisableReadItem = true;

				graph.OnBeforeCommit += delegate
				{
					interceptor.Commit?.Invoke();
					interceptor.Commit = null;
				};
			});
		}

		public IEnumerable<object> Select(PXGraph graph, BqlCommand command, int topCount, PXView view, PXDataValue[] pars)
		{
			// TODO: need to verify this code
			return ((IEnumerable<object>)GetCollection(false)).Where(it=>command.Meet(view.Cache, it, pars?.Select(p=>p.Value).ToArray()));
		}
	}

	class Interceptor : PXDBInterceptorAttribute
	{
		public Func<bool, IList> Source;
		public Type ItemType;
		public override BqlCommand GetRowCommand()
		{
			return null;
		}

		public override BqlCommand GetTableCommand()
		{
			return null;
		}

		int GetIndexOf(object item, PXCache cache)
		{
			var src = Source(false);
			if (src == null)
				return -1;

			for (int i = 0; i < src.Count; i++)
			{
				var b = src[i];
				if (cache.ObjectsEqual(b, item))
					return i;
			}

			return -1;
		}

		public Action Commit;

		public override bool PersistInserted(PXCache sender, object row)
		{
			var i = GetIndexOf(row, sender);
			if (i >= 0)
				throw new PXException("Items exists");

			var verify = Verify(sender, row, PXDBOperation.Insert);
			if (verify)
			{
				var clone = sender.CreateCopy(row);
				var lst = Source(true);
				Commit += delegate {lst.Add(clone); };
			}
			else
			{
				GenerateError(sender, row, PXDBOperation.Insert);
			}
			return verify;
		}

		public override bool PersistDeleted(PXCache sender, object row)
		{
			var i = GetIndexOf(row, sender);
			if (i < 0)
				throw new PXException("Items not found");

			var verify = Verify(sender, row, PXDBOperation.Delete);
			if (verify)
			{
				var lst = Source(true);
				var deletedCopy = lst[i];
				Commit += delegate { lst.Remove(deletedCopy); };
			}
			else
			{
				GenerateError(sender, row, PXDBOperation.Delete);
			}
			return verify;
		}

		public override bool PersistUpdated(PXCache sender, object row)
		{
			var i = GetIndexOf(row, sender);
			if (i < 0)
            {
				// in some cases, f.e. for record, that was inserted, than deleted and next added once again to cache it's status will be Updated, however it is, in fact new record
                return PersistInserted(sender, row);
            }

			var verify = Verify(sender, row, PXDBOperation.Update);
			if (!verify)
			{
				GenerateError(sender, row, PXDBOperation.Update);
			}
			else
			{
				//var clone = sender.CreateCopy(row);
				//var lst = Source(true);
				//var updatedCopy = lst[i];
				//Commit += delegate
				//{
				//    lst.Remove(updatedCopy);
				//    lst.Add(clone);
				//};
			}
			return verify;
		}

		private void GenerateError(PXCache cache, object row, PXDBOperation operation)
		{
			string prefix;
			switch (operation)
			{
				case PXDBOperation.Insert:
					prefix = ErrorMessages.GetLocal(ErrorMessages.Inserting);
					break;
				case PXDBOperation.Delete:
					prefix = ErrorMessages.GetLocal(ErrorMessages.Deleting);
					break;
				default:
					prefix = ErrorMessages.GetLocal(ErrorMessages.Updating);
					break;
			}
			throw new PXOuterException(PXUIFieldAttribute.GetErrors(cache, row), cache.Graph.GetType(), row,
				ErrorMessages.RecordRaisedErrors, prefix, PXUIFieldAttribute.GetItemName(cache));
		}

		private bool Verify(PXCache cache, object row, PXDBOperation operation)
		{
			bool result = true;
			PXRowPersistingEventArgs e = new PXRowPersistingEventArgs(operation, row);
			foreach (string field in cache.Fields)
			{
				if (HasError(cache, row, field))
				{
					return false;
				}

				foreach (PXDefaultAttribute defAttr in cache.GetAttributesReadonly(row, field)
					.OfType<PXDefaultAttribute>()
					.Where(defaultAttribute => defaultAttribute.PersistingCheck != PXPersistingCheck.Nothing))
				{
					defAttr.RowPersisting(cache, e);
					bool error = HasError(cache, row, field);
					if (error) 
						result = false;

					if (error)
					{
						cache.RaiseExceptionHandling(field, row, null, null);
						return false;
					}
				}
			}
			return result;
		}

		private bool HasError(PXCache cache, object row, string fieldName)
		{
			return !string.IsNullOrEmpty(PXUIFieldAttribute.GetErrorOnly(cache, row, fieldName));
		}
	}
}
