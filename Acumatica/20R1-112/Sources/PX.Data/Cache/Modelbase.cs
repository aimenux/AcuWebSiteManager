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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using PX.Api;
using PX.Common;

namespace PX.Data
{
    /// <summary>
    /// Untyped interface to access <see cref="PXCache{T}">PXCache&lt;TNode&gt;</see>
    /// without knowledge of the <tt>TNode</tt> type.</summary>
    /// <seealso cref="PXCache{T}"/>
	public abstract partial class PXCache
	{
		#region Ctors

		public PXCache()
		{
            var att = Attribute.GetCustomAttributes(GetItemType(), typeof(PXCacheNameAttribute), true).FirstOrDefault() as PXCacheNameAttribute;
			if (att != null)
				DisplayName = att.Name;
		}

		static PXCache()
		{
			_ExtensionTables = new Dictionary<string, List<KeyValuePair<string, List<string>>>>(StringComparer.OrdinalIgnoreCase);
			_AvailableExtensions = FindExtensionTypes(out _mapping, null, _ExtensionTables);
		}

		internal static Type[] FindExtensionTypes(List<Exception> errorLog = null, Dictionary<string, List<KeyValuePair<string, List<string>>>> extensionTables = null)
		{
			Mapping mapping;
			return FindExtensionTypes(out mapping, errorLog, extensionTables);
		}

		internal static Type[] FindExtensionTypes(out Mapping mapping, List<Exception> errorLog = null, Dictionary<string, List<KeyValuePair<string, List<string>>>> extensionTables = null)
	    {
		    List<Type> ret = new List<Type>();
            mapping = new Mapping();

            foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
		    {
			    // ignore some assemblies including dynamic ones
			    if (!PXSubstManager.IsSuitableTypeExportAssembly(a, false))
				    continue;
			    try
			    {
				    Type[] types = null;
				    try
				    {
					    if (!a.IsDynamic)
						    types = a.GetExportedTypes();
				    }
				    catch (System.Reflection.ReflectionTypeLoadException te)
				    {
					    types = te.Types;
				    }
				    if (types == null)
				    {
					    continue;
				    }
				    // iterate through assembly types
				    foreach (Type t in types)
				    {
						// processing extension objects
						if (typeof(PXCacheExtension).IsAssignableFrom(t))
						{
							ret.Add(t);
							saveTables(t, extensionTables);
						}
						else if (typeof(PXGraphExtension).IsAssignableFrom(t))
						{
							Type bt = PXExtensionManager.GetFirstExtensionParent(t);
							if (bt != t.BaseType)
							{
								foreach (MethodInfo mi in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
								{
									if (typeof(IBqlMapping).IsAssignableFrom(mi.ReturnType) && t.ContainsGenericParameters == false)
									{
										try
										{
											IBqlMapping map = (IBqlMapping)mi.Invoke(Activator.CreateInstance(t), new object[0]);
                                            if (map != null)
                                                mapping.AddMap(t, map);
                                            else
                                                mapping.AddDisabledGraphExtension(t);
                                        }
										catch
										{
										}
									}
								}
							}
						}
					}
			    }
			    catch(Exception ex)
			    {
					if(errorLog!=null)
						errorLog.Add(new Exception("Failed to load types from the assembly: " + a.FullName + "\nWith message: " + ex.Message, ex));
			    }
		    }
			Type[] dynamic = _DynamicExtensions;
			if (dynamic == null)
			{
				dynamic = PXCodeDirectoryCompiler.GetCompiledTypes<PXCacheExtension>().ToArray();
			}
			if (dynamic != null)
			{
				foreach (Type t in dynamic)
				{
					saveTables(t, extensionTables);
				}
			}
			ret.Sort((a, b) => String.Compare(a.FullName, b.FullName));
		    return ret.ToArray();
	    }

		private static void saveTables(Type t, Dictionary<string, List<KeyValuePair<string, List<string>>>> extensionTables)
		{
			if (extensionTables == null) return;
			if (t.IsDefined(typeof(PXDBInterceptorAttribute), true)
				&& t.BaseType.IsGenericType)
			{
				PXDBInterceptorAttribute inter =
					(PXDBInterceptorAttribute)t.GetCustomAttributes(typeof(PXDBInterceptorAttribute), true)[0];
				Type table = t.BaseType.GetGenericArguments()[t.BaseType.GetGenericArguments().Length - 1];
				while (table != typeof(object))
				{
					if ((table.BaseType == typeof(object) || !typeof(IBqlTable).IsAssignableFrom(table.BaseType)) &&
						typeof(IBqlTable).IsAssignableFrom(table) || table.IsDefined(typeof(PXTableAttribute), false))
					{
						List<KeyValuePair<string, List<string>>> extensions;
						if (!extensionTables.TryGetValue(table.Name, out extensions))
						{
							extensionTables[table.Name] = extensions = new List<KeyValuePair<string, List<string>>>();
						}
						extensions.Add(new KeyValuePair<string, List<string>>(t.Name, inter.Keys));
					}
					table = table.BaseType;
				}
			}
		}

		private static Dictionary<string, List<KeyValuePair<string, List<string>>>> _ExtensionTables;

		internal static List<KeyValuePair<string, List<string>>> GetExtensionTables(string table)
		{
			List<KeyValuePair<string, List<string>>> ret;
			Dictionary<string, List<KeyValuePair<string, List<string>>>> extensionTables = _ExtensionTables;
			if (extensionTables == null)
			{
				extensionTables = new Dictionary<string, List<KeyValuePair<string, List<string>>>>();
				FindExtensionTypes(null, extensionTables);
				_ExtensionTables = extensionTables;
			}
			extensionTables.TryGetValue(table, out ret);
			return ret;
		}

		#endregion

		#region Attributes

		/// <summary>
		/// The flag controlled by the
		/// <see cref="PXDisableCloneAttributesAttribute">PXDisableCloneAttributes</see>
		/// attribute.
		/// </summary>
		public bool DisableCloneAttributes;
#if DEBUG
	    public Stopwatch SelectorTimer = new Stopwatch();
#endif

        internal bool NeverCloneAttributes;

		internal bool DisableCacheClear;
        /// <exclude />
		public bool DisableReadItem;

		protected List<PXEventSubscriberAttribute> _CacheAttributes;
		protected Dictionary<object, DirtyItemState> _ItemAttributes;
		protected PXEventSubscriberAttribute[] _CacheAttributesWithEmbedded;
		protected PXEventSubscriberAttribute[] _ReusableItemAttributes;
		protected int[] _UsableItemAttributes;
		protected ILookup<Type, PXEventSubscriberAttribute> _AttributesByType;
		protected ILookup<string, PXEventSubscriberAttribute> _AttributesByName;
		Func<object, object, bool>[] _AttributeComparers;





		/// <summary>
		/// Returns the cach-level instances of attributes placed on the
		/// specified field and all item-level instances currently stored
		/// in the cache.
		/// </summary>
		/// <param name="name">The name of the field whose attributes are returned.
		/// If <tt>null</tt>, the method returns attributes from all fields.</param>
		public abstract List<PXEventSubscriberAttribute> GetAttributes(string name);
        /// <summary>Returns the cach-level instances of attributes placed on the specified field and all item-level instances currently stored in the cache as read-only instances.</summary>
		public abstract List<PXEventSubscriberAttribute> GetAttributesReadonly(string name);
        /// <exclude />
		public abstract List<PXEventSubscriberAttribute> GetAttributesReadonly(string name, bool extractEmmbeddedAttr);

        /// <summary>Returns a collection of row-specific attributes as read-only instances.</summary>
		public abstract IEnumerable<PXEventSubscriberAttribute> GetAttributesReadonly(object data, string name);
        /// <summary>Returns a collection of row specific attributes. This method creates a copy of all cache level attributes and stores these clones to internal collection that
        /// contains row specific attributes. To avoid cloning cache level attributes, use the GetAttributesReadonly method.</summary>
		public abstract IEnumerable<PXEventSubscriberAttribute> GetAttributes(object data, string name);

        /// <exclude />
	    public virtual IEnumerable<T> GetAttributesOfType<T>(object data, string name)
		    where T : PXEventSubscriberAttribute
	    {
		    return GetAttributes(data, name).OfType<T>();
	    }

        /// <exclude />
	    public abstract bool HasAttributes(object data);
        /// <exclude />
		public virtual bool HasAttribute<T>()
			where T : PXEventSubscriberAttribute
		{
			foreach (PXEventSubscriberAttribute attr in _CacheAttributes)
			{
				if (attr is T)
				{
					return true;
				}
				else
				{
					var aggregatedAttr = attr as PXAggregateAttribute;
					if (aggregatedAttr != null)
					{
						var aggrAttrs = aggregatedAttr.GetAggregatedAttributes();
						foreach (PXEventSubscriberAttribute aggrAttr in aggrAttrs)
						{
							if (aggrAttr is T)
								return true;
						}
					}
				}
			}

			return false;
		}
        /// <summary>
        /// Returns the cach-level instances of attributes placed on the
        /// specified field and all item-level instances currently stored
        /// in the cache.
        /// </summary>
        /// <typeparam name="Field">The DAC field.</typeparam>
		public List<PXEventSubscriberAttribute> GetAttributes<Field>()
			where Field : IBqlField
		{
			return GetAttributes(typeof(Field).Name);
		}
        /// <summary>Returns the cache-level read-only instances of attributes placed on the specified field in the DAC. The field is specified as the type parameter.</summary>
        /// <typeparam name="Field">The DAC field.</typeparam>
		public List<PXEventSubscriberAttribute> GetAttributesReadonly<Field>()
			where Field : IBqlField
		{
			return GetAttributesReadonly(typeof(Field).Name);
		}
        /// <summary>Returns the read-only item-level instances of attributes placed on the specified field if such instances exist for the provided data record, or the cache-level
        /// instances otherwise. The field is specified as the type parameter.</summary>
        /// <typeparam name="Field">The DAC field.</typeparam>
        /// <param name="data">The data record.</param>
		public IEnumerable<PXEventSubscriberAttribute> GetAttributesReadonly<Field>(object data)
			where Field : IBqlField
		{
			return GetAttributesReadonly(data, typeof(Field).Name);
		}
        /// <summary>
        /// Returns the item-level instances of attributes placed on the
        /// specified field. If such instances are not exist for the
        /// provided data record, the method creates them by copying all
        /// cache-level attributes and storing them in the internal collection
        /// that contains the data record specific attributes.</summary>
        /// <remarks>
        /// To avoid cloning cache-level attributes, use the
        /// <see cref="PXCache{T}.GetAttributesReadonly(object,string)">GetAttributesReadonly(object, string)</see>
        /// method. The field is specified as the type parameter.
        /// </remarks>
        /// <typeparam name="Field">The DAC field.</typeparam>
        /// <param name="data">The data record.</param>
        /// <example>
        /// <code>
        /// foreach (PXEventSubscriberAttribute attr in sender.GetAttributes&lt;Field&gt;(data))
        /// {
        ///     if (attr is PXUIFieldAttribute)
        ///     {
        ///         // Doing something
        ///     }
        /// }
        /// </code>
        /// </example>
		public IEnumerable<PXEventSubscriberAttribute> GetAttributes<Field>(object data)
			where Field : IBqlField
		{
			return GetAttributes(data, typeof(Field).Name);
		}
        /// <exclude />
		public abstract List<Type> GetExtensionTables();

		internal abstract PXCacheExtension[] GetCacheExtensions(IBqlTable item);





	    /// <summary>
	    ///initializes internal collections: _ItemAttributes, _CacheAttributesWithEmbedded, _ReusableItemAttributes, _AttributesByType
	    /// </summary>
        /// <exclude />
	    protected void InitItemAttributesCollection()
	    {
		    if (_ItemAttributes != null)
			    return;

		    _ItemAttributes = new Dictionary<object, DirtyItemState>();

		    if (_CacheAttributesWithEmbedded == null) //_ItemAttributes cleared sometimes
			{

			    _CacheAttributesWithEmbedded = GetAttributesReadonly(null, true).ToArray();
			    for (int i = 0; i < _CacheAttributesWithEmbedded.Length; i++)
			    {
				    _CacheAttributesWithEmbedded[i].IndexInClonesArray = i;
			    }
			    _ReusableItemAttributes = new PXEventSubscriberAttribute[_CacheAttributesWithEmbedded.Length];
				_UsableItemAttributes = new int[_CacheAttributesWithEmbedded.Length > 0 ? (((_CacheAttributesWithEmbedded.Length - 1) / 32) + 1) : 0];


			    var q = from a in _CacheAttributesWithEmbedded
				    from t in a.GetType().CreateList(_ => _.BaseType)
				    where typeof (PXEventSubscriberAttribute).IsAssignableFrom(t) && t != typeof (PXEventSubscriberAttribute)
				    select new {t, a};

			    this._AttributesByType = q.ToLookup(_ => _.t, _ => _.a);


			    this._AttributesByName = _CacheAttributesWithEmbedded.ToLookup(_ => _.FieldName, _ => _,
				    StringComparer.OrdinalIgnoreCase);



			    _AttributeComparers = _CacheAttributesWithEmbedded.Select(GetAttributeComparer).ToArray();

		    }
	    }

	    /// <summary>
		/// Ensures cleanup of item attributes after foreach statement
		/// </summary>
		protected class DisposableAttributesList : IPXAttributeList
		{
			public DisposableAttributesList(PXCache c, object data)
			{
				this.cache = c;

				this.Buffer = c.GetItemState(data);
				if (this.Buffer.RefCnt > 1)
				{
					Buffer.RefCnt--;
					_disposed = true;
				}
			}

			public string NameFilter;
			public PXCache cache;

			public DirtyItemState Buffer;

			private bool _disposed;
			internal Type TypeFilter;


			public void Dispose()
			{
				if (_disposed)
					return;
				_disposed = true;
				cache.CompressItemState(Buffer);

			}



			public IEnumerator<PXEventSubscriberAttribute> GetEnumerator()
			{
				return EnumItems().GetEnumerator();

			}



			bool IsMatchFilters(PXEventSubscriberAttribute a)
			{
				if (NameFilter != null && !NameFilter.Equals(a.FieldName, StringComparison.OrdinalIgnoreCase))
					return false;

				if (TypeFilter != null && !TypeFilter.IsInstanceOfType(a))
					return false;

				return true;
			}
			IEnumerable<PXEventSubscriberAttribute> EnumItems()
			{
				using (this)
				{
					var d = Buffer.DirtyAttributes;
					var p = cache._CacheAttributesWithEmbedded;
					if (NameFilter != null)
					{

						if (this.cache._AttributesByName.Contains(NameFilter))
						{
							foreach (var a in this.cache._AttributesByName[NameFilter])
							{
								if (!IsMatchFilters(a))
									continue;
								var i = a.IndexInClonesArray;
								var result = cache.CheckoutItemAttribute(Buffer, i);

								yield return result;
							}

						}

					}
					else if (TypeFilter != null)
					{
						if (this.cache._AttributesByType.Contains(TypeFilter))
						{
							foreach (var a in this.cache._AttributesByType[TypeFilter])
							{
								if (!IsMatchFilters(a))
									continue;
								var i = a.IndexInClonesArray;
								var result = cache.CheckoutItemAttribute(Buffer, i);

								yield return result;
							}

						}

					}
					else
					{
						for (int i = 0; i < d.Length; i++)
						{
							var result = cache.CheckoutItemAttribute(Buffer, i);
							yield return result;
						}
					}
				}

			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				//return new Enumerator(this);
				return this.EnumItems().GetEnumerator();

			}



		}


		/// <summary>
		/// Compares attributes by field values
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		bool AttributesEqual(PXEventSubscriberAttribute a, PXEventSubscriberAttribute b)
		{
			if (object.ReferenceEquals(a, b))
				return true;

			bool eq = _AttributeComparers[a.IndexInClonesArray](a, b);

			return eq;
		}
		static Func<object, object, bool> GetAttributeComparer(PXEventSubscriberAttribute a)
		{
			var r = _Comparers.GetOrAdd(a.GetType(), type => new CompiledAttributeComparer(type));
			return r.CompiledMethod;
		}
		static readonly ConcurrentDictionary<Type, CompiledAttributeComparer> _Comparers = new ConcurrentDictionary<Type, CompiledAttributeComparer>();

		class CompiledAttributeComparer
		{
			public CompiledAttributeComparer(Type t)
			{
				T = t;
			}
			readonly Type T;
			Func<object, object, bool> _CompiledMethod;

			public Func<object, object, bool> CompiledMethod
			{
				get
				{
					if (_CompiledMethod == null)
					{
						lock (this)
						{
							if (_CompiledMethod == null)
								_CompiledMethod = CompileAttributeComparer();
						}

					}

					return _CompiledMethod;
				}
			}

			Func<object, object, bool> CompileAttributeComparer()
			{
				var flist = new List<FieldInfo>();
				for (var t = T; t != typeof(object); t = t.BaseType)
				{

					if (t == typeof(PXEventSubscriberAttribute))
						break;

					var flds = t.GetFields(BindingFlags.Instance
										   | BindingFlags.Public
										   | BindingFlags.NonPublic
										   | BindingFlags.DeclaredOnly
						)
						.OrderBy(_ => _.Name).ToArray();

					if (t == typeof(PXAggregateAttribute))
						flds = flds.Where(_ => _.Name != "_Attributes").ToArray();

					flist.AddRange(flds);
				}

				var aParam = Expression.Parameter(typeof(object), "a");
				var bParam = Expression.Parameter(typeof(object), "b");

				//var bodyList = new List<Expression>();
				Expression ret = Expression.Constant(true);

				foreach (var fieldInfo in flist)
				{

					var cmp = Expression.Equal(
						Expression.Field(Expression.Convert(aParam, fieldInfo.DeclaringType), fieldInfo),
						Expression.Field(Expression.Convert(bParam, fieldInfo.DeclaringType), fieldInfo)

						);

					ret = Expression.And(cmp, ret);

				}

				var result = Expression.Lambda<Func<object, object, bool>>(ret, aParam, bParam).Compile();
				return result;

			}

		}


		/// <summary>
		/// Container for dirty attributes in _ItemAttributes collection
		/// </summary>
		protected class DirtyItemState
		{
			public PXEventSubscriberAttribute[] DirtyAttributes;
			//public Dictionary<int, PXEventSubscriberAttribute> DirtyAttributes;
			public int RefCnt;
			public object BoundItem;


		}




		public static void TryDispose(object obj)
		{
			var d = obj as IDisposable;
			d?.Dispose();
		}

		/// <summary>
		/// Clones attribute to per item collection
		/// </summary>
		/// <param name="state"></param>
		/// <param name="idx"></param>
		/// <returns></returns>
		PXEventSubscriberAttribute CheckoutItemAttribute(DirtyItemState state, int idx)
		{
			var result = state.DirtyAttributes[idx];
			if (result == null)
			{

				result = _ReusableItemAttributes[idx];
				if (result != null)
				{
					_ReusableItemAttributes[idx] = null;

					//sanity check
					if (!AttributesEqual(result, result.Prototype))
						throw new PXException(MsgNotLocalizable.NewItemAttributeDirty);

					if (!AttributesEqual(result, _CacheAttributesWithEmbedded[idx]))
						result = null;
				}


				if (result == null)
				{
					result = _CacheAttributesWithEmbedded[idx].Clone(PXAttributeLevel.Item);
					result.Prototype = _CacheAttributesWithEmbedded[idx].Clone(PXAttributeLevel.Item);



				}

				state.DirtyAttributes[idx] = result;

			}
			var aggr = result as PXAggregateAttribute;
			if (aggr != null)
			{
				var list = aggr.InternalAttributesAccessor;
				for (int i = 0; i < list.Count; i++)
				{
					var a = list[i];
					//if (a == null || a.AttributeLevel == PXAttributeLevel.Item)
					//	continue;
					list[i] = CheckoutItemAttribute(state, a.IndexInClonesArray);
				}
			}


			return result;
		}

	    private PXEventSubscriberAttribute[] _reusableContainer;
        /// <exclude />
		protected DirtyItemState GetItemState(object row)
		{
			if (row == null || _ItemAttributes == null)
				return null;



			DirtyItemState state;
			if (_ItemAttributes.TryGetValue(row, out state))
			{
				state.RefCnt++;

				if (state.DirtyAttributes == null)
				{
					if (_reusableContainer != null)
					{
						state.DirtyAttributes = _reusableContainer;
						_reusableContainer = null;
					}
					else
					{
						state.DirtyAttributes = new PXEventSubscriberAttribute[_ReusableItemAttributes.Length];
					}
				}
			}



			return state;

		}


        /// <summary>Checks if a subscriber attribute has an item related dirty clone.</summary>
        /// <exclude />
		/// <typeparam name="T"></typeparam>
		/// <param name="buffer"></param>
		/// <param name="subscriber"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		protected T GetDirtyAttribute<T>(DirtyItemState buffer, T subscriber)
			where T : class
		{
			var a = subscriber as PXEventSubscriberAttribute;

			if (buffer == null)
			{
				if (a.AttributeLevel != PXAttributeLevel.Cache)
					throw new PXException(MsgNotLocalizable.GetDirtyAttributeLevel);
				//a.Version++;
				return subscriber;
			}


			var bound = CheckoutItemAttribute(buffer, a.IndexInClonesArray);


			return bound as T;
		}


        /// <summary>Cleans up unmodified attributes from the item state.</summary>
        /// <exclude />
	    /// <param name="buffer"></param>
	    protected void CompressItemState(DirtyItemState buffer)
	    {
		    if (buffer == null)
			    return;
		    buffer.RefCnt--;
		    if (buffer.RefCnt > 0)
			    return;

		    if (buffer.RefCnt < 0)
			    throw new PXException(MsgNotLocalizable.ReleaseItemStateRefCntLessZero);

		    bool hasDirty = false;
		    for (int index = 0; index < buffer.DirtyAttributes.Length; index++)
		    {
			    var attribute = buffer.DirtyAttributes[index];
			    if (attribute == null)
				    continue;

			    bool isDirty = attribute.IsDirty;
			    if (!isDirty)
			    {
				    isDirty = attribute.IsDirty = !AttributesEqual(attribute, attribute.Prototype);
			    }
			    if (isDirty)
			    {
				    hasDirty = true;
					_UsableItemAttributes[index / 32] |= (1 << (index % 32));
				    attribute.Prototype = null;
				    continue;
			    }

			    _ReusableItemAttributes[index] = attribute;
			    buffer.DirtyAttributes[index] = null;

		    }
		    if (!hasDirty)
		    {
			    _reusableContainer = buffer.DirtyAttributes;
			    buffer.DirtyAttributes = null;
		    }

	    }







	    #endregion

		#region Field Manipulation
		internal Dictionary<string, int> _FieldsMap;
		protected HashSet<string> _BypassAuditFields;

		/// <summary>
		/// Bypass collecting values for audit. May be used for passwords ans so on.
		/// </summary>
        /// <exclude/>
		public virtual HashSet<string> BypassAuditFields
		{
			get
			{
				if (_BypassAuditFields == null)
				{
					_BypassAuditFields = new HashSet<string>();
				}
				return _BypassAuditFields;
			}
		}

		protected internal HashSet<string> _EncryptAuditFields;

		/// <summary>
		/// Bypass collecting values for audit. May be used for passwords ans so on.
		/// </summary>
		/// <exclude/>
		public virtual HashSet<string> EncryptAuditFields
		{
			get
			{
				if (_EncryptAuditFields == null)
				{
					_EncryptAuditFields = new HashSet<string>();
				}
				return _EncryptAuditFields;
			}
		}

		internal bool _SelectingForAuditExplore;

		protected HashSet<string> _AlteredFields;
		protected HashSet<string> _SecuredFields;
		protected HashSet<string> _SelectingFields;
		protected HashSet<int> _KeyValueStoredOrdinals;
		protected internal Dictionary<string, int> _KeyValueStoredNames;
		protected internal Dictionary<string, int> _DBLocalizableNames;
		protected KeyValuePair<string, int>? _FirstKeyValueStored;
		protected Dictionary<string, string> _InactiveFields;
		protected internal Dictionary<string, int> _KeyValueAttributeNames;
		internal Dictionary<string, StorageBehavior> _KeyValueAttributeTypes;
		protected internal int _KeyValueAttributeSlotPosition;
		protected KeyValuePair<string, int>? _FirstKeyValueAttribute;
		internal virtual HashSet<string> SelectingFields
		{
			get
			{
				if (_SelectingFields == null)
				{
					_SelectingFields = new HashSet<string>();
				}
				return _SelectingFields;
			}
		}
		internal virtual HashSet<string> SecuredFields
		{
			get
			{
				if (_SecuredFields == null)
				{
					_SecuredFields = new HashSet<string>();
				}
				return _SecuredFields;
			}
		}

		protected internal object[] convertAttributesFromString(string[] alternatives)
		{
			object[] slot = new object[alternatives.Length];
			foreach (KeyValuePair<string, int> pair in _KeyValueAttributeNames)
			{
				slot[pair.Value] = alternatives[pair.Value];
				if (_KeyValueAttributeTypes.TryGetValue(pair.Key, out var behavior))
				{
					if (behavior == StorageBehavior.KeyValueDate)
					{
						if (DateTime.TryParse(alternatives[pair.Value], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
						{
							slot[pair.Value] = dt;
						}
					}
					else if (behavior == StorageBehavior.KeyValueNumeric)
					{
						if (int.TryParse(alternatives[pair.Value], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var iv))
						{
							slot[pair.Value] = iv;
						}
					}
				}
			}
			return slot;
		}

		/// <exclude />
		public ISet<string> GetFieldsWithChangingUpdatability()
		{
			HashSet<string> ret = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> preserve = _AlteredFields;
			_AlteredFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				object item = CreateInstance();
				_MeasuringUpdatability = true;
				RaiseRowSelected(item);
			}
			catch
			{
			}
			finally
			{
				ret.AddRange(_AlteredFields);
				_AlteredFields = preserve;
				_MeasuringUpdatability = false;
			}
			return ret;
		}
		protected bool _MeasuringUpdatability;
		internal protected const string _OriginalValue = "_OriginalValue";
		protected class MeasuringUIFieldAttribute : PXUIFieldAttribute
		{
			PXCache _Cache;
			public MeasuringUIFieldAttribute(PXCache cache, string fieldName, int fieldOrdinal, Type bqlTable)
			{
				_Cache = cache;
				_FieldName = fieldName;
				_FieldOrdinal = fieldOrdinal;
				_BqlTable = bqlTable;
			}
			public override bool Enabled
			{
				get
				{
					return base.Enabled;
				}

				set
				{
					base.Enabled = value;
					_Cache.SetAltered(_FieldName, true);
				}
			}
		}

        /// <summary>Gets the collection of field names. Placing the field name in this collection forces calculation of the <tt>PXFieldState</tt> object in the <tt>GetValueExt</tt>, <tt>GetValueInt</tt>
        /// methods.</summary>
        /// <remarks>This property internally calls the OnFieldSelecting event with the IsAltered flag.</remarks>
		public virtual ISet<string> AlteredFields
		{
			get
			{
				if (_AlteredFields == null)
				{
					_AlteredFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				}
				return _AlteredFields;
			}
		}
        /// <summary>
        /// Adds or removes the specified field to/from the <tt>AlteredFields</tt> list.
        /// </summary>
        /// <typeparam name="Field">The DAC field.</typeparam>
        /// <param name="isAltered">The value indicating whether the field is added
        /// (<tt>true</tt>) or removed (<tt>false</tt>) from the <tt>AlteredFields</tt> list.</param>
        /// <example>
        /// <code>
        /// Items.Cache.SetAltered&lt;FlatPriceItem.inventoryID&gt;(true);
        /// </code>
        /// </example>
		public virtual void SetAltered<Field>(bool isAltered)
			where Field : IBqlField
		{
			SetAltered(typeof(Field).Name, isAltered);
		}
        /// <summary>
        /// Adds or removes the specified field to/from the <tt>AlteredFields</tt> list.
        /// </summary>
        /// <param name="field">The name of the DAC field.</param>
        /// <param name="isAltered">The value indicating whether the field is added
        /// (<tt>true</tt>) or removed (<tt>false</tt>) from the <tt>AlteredFields</tt> list.</param>
		public virtual void SetAltered(string field, bool isAltered)
		{
			if (!String.IsNullOrEmpty(field))
			{
				if (!isAltered)
				{
					if (_AlteredFields != null)
					{
						_AlteredFields.Remove(field.ToLower());
					}
				}
				else 
				{
					AlteredFields.Add(field.ToLower());
				}
			}
		}

        /// <summary>
        /// Sets value without any validation or event handling
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="ordinal"></param>
        /// <param name="value"></param>
		public abstract void SetValue(object data, int ordinal, object value);
        /// <exclude />
		public abstract object GetValue(object data, int ordinal);

        /// <summary>
        /// sets value without any validation or event handling
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
		public abstract void SetValue(object data, string fieldName, object value);

        /// <summary>Gets the field value by the field name without raising any events.</summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
		public abstract object GetValue(object data, string fieldName);

        /// <summary>
        /// Set field value. <br/>
        /// Raises events: OnFieldUpdating, OnFieldVerifying, OnFieldUpdated. <br/>
        /// If exception - OnExceptionHandling. <br/>
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName">The name of the field. The parameter is case-insensitive.</param>
        /// <param name="value"></param>
		public abstract void SetValueExt(object data, string fieldName, object value);

		internal abstract object GetCopy(object data);
        /// <summary>
        /// Raises OnFieldUpdating event to set field value
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName"></param>
		public abstract void SetDefaultExt(object data, string fieldName, object value = null);
        /// <summary>
        /// Sets the value of the field in the provided data record
        /// without raising events.
        /// </summary>
        /// <typeparam name="Field">The field that is set to the
        /// value.</typeparam>
        /// <param name="data">The data record.</param>
        /// <param name="value">The value to set to the field.</param>
        /// <example>
        /// The code below shows an event handler that sets values to
        /// three fields of the <tt>ARTran</tt> data record on update
        /// of the <tt>UOM</tt> field: two fields are assigned their
        /// default values and the <tt>UnitCost</tt> field is set
        /// directly.
        /// <code>
        /// protected virtual void APTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        /// {
        ///     APTran tran = (APTran)e.Row;
        ///     sender.SetDefaultExt&lt;APTran.unitCost&gt;(tran);
        ///     sender.SetDefaultExt&lt;APTran.curyUnitCost&gt;(tran);
        ///     sender.SetValue&lt;APTran.unitCost&gt;(tran, null);
        /// }
        /// </code>
        /// </example>
		public void SetValue<Field>(object data, object value)
			where Field : IBqlField
		{
			SetValue(data, typeof(Field).Name, value);
		}

		public class ExternalCallMarker
		{
			public readonly object Value;
			public bool IsInternalCall { get; set; } = false;

			public ExternalCallMarker(object value)
			{
				Value = value;
			}

			public static object Unwrap(object value)
			{
				ExternalCallMarker marker = value as ExternalCallMarker;
				return marker != null ? marker.Value : value;
			}
		}

        /// <summary>Sets the value of the field in the provided data
        /// record.</summary>
        /// <remarks>The method raises the <tt>FieldUpdating</tt>,
        /// <tt>FieldVerifying</tt>, and <tt>FieldUpdated</tt> events. To set the
        /// value to the field without raising events, use the <see
        /// cref="PXCache{T}.SetValue(object,string,object)">SetValue(object,
        /// string, object)</see> method.</remarks>
        /// <typeparam name="Field">The field that is set to the
        /// value.</typeparam>
        /// <param name="data">The data record.</param>
        /// <param name="value">The value to set to the field.</param>
        /// <example>
        /// <code>
        /// protected virtual void APInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        /// {
        ///     APInvoice doc = e.Row as APInvoice;
        /// 
        ///     if (doc.Released != true &amp;&amp; doc.Prebooked != true)
        ///     {
        ///         if (doc.CuryDocBal != doc.CuryOrigDocAmt)
        ///         {
        ///             if (doc.CuryDocBal != null &amp;&amp; doc.CuryDocBal != 0)
        ///                 sender.SetValueExt&lt;APInvoice.curyOrigDocAmt&gt;(doc, doc.CuryDocBal);
        ///             else
        ///                 sender.SetValueExt&lt;APInvoice.curyOrigDocAmt&gt;(doc, 0m);
        ///         }
        ///         ...
        ///     }
        /// }
        /// </code>
        /// </example>
		public void SetValueExt<Field>(object data, object value)
			where Field : IBqlField
		{
			SetValueExt(data, typeof(Field).Name, value);
		}
        /// <summary>Sets the default value to the field in the provided data
        /// record.</summary>
        /// <remarks>The method raises <tt>FieldDefaulting</tt>,
        /// <tt>FieldUpdating</tt>, <tt>FieldVerifying</tt>, and
        /// <tt>FieldUpdated</tt>.</remarks>
        /// <typeparam name="Field">The field to set.</typeparam>
        /// <param name="data">The data record.</param>
        /// <example>
        /// The code below sets default values to the fields of a
        /// <tt>Location</tt> data record.
        /// <code>
        /// // The data view to select Location data records
        /// public PXSelect&lt;Location,
        ///     Where&lt;Location.bAccountID, Equal&lt;Current&lt;BAccount.bAccountID&gt;&gt;&gt;,
        ///     OrderBy&lt;Asc&lt;Location.locationID&gt;&gt;&gt; IntLocations;
        /// ...
        /// public virtual void InitVendorLocation(Location aLoc, string aLocationType)
        /// {
        ///     this.IntLocations.Cache.SetDefaultExt&lt;Location.vCarrierID&gt;(aLoc);
        ///     this.IntLocations.Cache.SetDefaultExt&lt;Location.vFOBPointID&gt;(aLoc);
        ///     this.IntLocations.Cache.SetDefaultExt&lt;Location.vLeadTime&gt;(aLoc);
        ///     this.IntLocations.Cache.SetDefaultExt&lt;Location.vShipTermsID&gt;(aLoc);
        ///     ...
        /// }
        /// </code>
        /// </example>
		public void SetDefaultExt<Field>(object data)
			where Field : IBqlField
		{
			SetDefaultExt(data, typeof(Field).Name);
		}

        /// <summary>Gets the value by the field name. The method raises the OnFieldSelecting event.</summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName"></param>
        /// <returns>Returns the field value or PXFieldState (if the field is in the <see cref="AlteredFields">AlteredFields</see> list).</returns>
		public abstract object GetValueExt(object data, string fieldName);


        /// <summary>
        /// The same as GetValueExt
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        /// <exclude/>
		public abstract object GetValueInt(object data, string fieldName);

	    /// <exclude/>
	    internal abstract object GetStateInt(object data, string fieldName);

        /// <summary>
        /// Gets the field state by the field name.
        /// </summary>
        /// <remarks>Raises the <tt>FieldSelecting</tt> event.</remarks>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName"></param>
        /// <returns>A <tt>PXFieldState</tt> object generated in the <tt>FieldSelecting</tt> event</returns>
		public abstract object GetStateExt(object data, string fieldName);
        /// <summary>
        /// Gets the value of the specified field in the provided data record.
        /// </summary>
        /// <typeparam name="Field">The field.</typeparam>
        /// <param name="data">The data record.</param>
        /// <returns>The field value.</returns>
        /// <example>
        /// The code below shows a <tt>FieldDefaulting</tt> event handler which
        /// gets the default value for the <tt>LocationExtAddress.VPaymentByType</tt> field
        /// from the current <tt>VendorClass</tt> data record.
        /// <code>
        /// protected virtual void LocationExtAddress_VPaymentByType_FieldDefaulting(
        ///     PXCache sender, PXFieldDefaultingEventArgs e)
        /// {
        ///     if (VendorClass.Current != null)
        ///     {
        ///         e.NewValue = VendorClass.Cache.GetValue&lt;VendorClass.paymentByType&gt;(VendorClass.Current);
        ///         e.Cancel = true;
        ///     }
        /// }
        /// </code>
        /// </example>
		public object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return GetValue(data, typeof(Field).Name);
		}
        /// <summary>Returns the value or the <tt>PXFieldState</tt> object of the
        /// specified field in the given data record. The <tt>PXFieldState</tt>
        /// object is returned if the field is in the <tt>AlteredFields</tt>
        /// collection.</summary>
        /// <param name="data">The data record.</param>
        /// <typeparam name="Field">The field whose value or
        /// <tt>PXFieldState</tt> object is returned.</typeparam>
        /// <remarks>The method raises the <tt>FieldSelecting</tt> event.</remarks>
        /// <returns>Returns the field value or PXFieldState (if the field is in the <see cref="AlteredFields">AlteredFields</see> list).</returns>
        /// <example>
        /// The code below shows a <tt>RowDeleted</tt> event handler in which you
        /// get the value (or field state) of the <tt>IsDefault</tt> field
        /// of the <tt>POVendorInventory</tt> data record.
        /// <code title="" description="" lang="CS">
        /// protected virtual void POVendorInventory_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        /// {
        ///     POVendorInventory vendor = e.Row as POVendorInventory;
        ///     object isdefault = cache.GetValueExt&lt;POVendorInventory.isDefault&gt;(e.Row);
        ///     if (isdefault is PXFieldState)
        ///     {
        ///         isdefault = ((PXFieldState)isdefault).Value;
        ///     }
        ///     if ((bool?)isdefault == true)
        ///         ...
        /// }</code></example>
		public object GetValueExt<Field>(object data)
			where Field : IBqlField
		{
			return GetValueExt(data, typeof(Field).Name);
		}
        /// <exclude/>
		public object GetValueInt<Field>(object data)
			where Field : IBqlField
		{
			return GetValueInt(data, typeof(Field).Name);
		}
        /// <summary>
        /// Gets the <tt>PXFieldState</tt> object of the specified field
        /// in the given data record.
        /// </summary>
        /// <remarks>The method raises the <tt>FieldSelecting</tt> event.</remarks>
        /// <typeparam name="Field">The field whose
        /// <tt>PXFieldState</tt> object is created.</typeparam>
        /// <param name="data">The data record.</param>
        /// <returns>The field state object.</returns>
        /// <example>
        /// The code below shows a part of the <tt>SOOrderEntry</tt> graph
        /// constructor. The field state is created for the
        /// <tt>SOLine.InventoryID</tt> field.
        /// <code>
        /// public SOOrderEntry()
        /// {
        ///     ...
        ///     PXFieldState state = (PXFieldState)this.Transactions.Cache.GetStateExt&lt;SOLine.inventoryID&gt;(null);
        ///     viewInventoryID = state != null ? state.ViewName : null;
        ///     ...
        /// }
        /// </code>
        /// </example>
		public object GetStateExt<Field>(object data)
			where Field : IBqlField
		{
			return GetStateExt(data, typeof(Field).Name);
		}

        /// <summary>
        /// Specified value stored in internal dictionary associated with data row,  <br/>
        /// and can be retrived later by GetValuePending method.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
		public abstract void SetValuePending(object data, string fieldName, object value);

        /// <summary>
        /// returns value stored by SetValuePending
        /// </summary>
        /// <exclude />
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
		public abstract object GetValuePending(object data, string fieldName);
        /// <exclude />
		public object GetValuePending<Field>(object data)
			where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			return GetValuePending(data, char.ToUpper(fieldName[0]).ToString() + fieldName.Substring(1));
		}
		public void SetValuePending<Field>(object data, object value)
			where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			SetValuePending(data, char.ToUpper(fieldName[0]).ToString() + fieldName.Substring(1), value);
		}

		/// <summary>
		/// returns value from instance copy stored in database
		/// </summary>
        /// <exclude />
		/// <param name="data"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public abstract object GetValueOriginal(object data, string fieldName);
        /// <exclude />
		public object GetValueOriginal<Field>(object data)
			where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			return GetValueOriginal(data, char.ToUpper(fieldName[0]).ToString() + fieldName.Substring(1));
		}
        /// <exclude />
		public abstract object GetOriginal(object data);
		private static Dictionary<Type, Dictionary<Type, string[]>> _keys = new Dictionary<Type, Dictionary<Type, string[]>>();
		internal static string[] GetKeyNames(PXGraph graph, Type cacheType)
		{
			Dictionary<Type, string[]> dict;
			string[] ret;
			lock (((ICollection)_keys).SyncRoot)
			{
				if (!_keys.TryGetValue(cacheType, out dict))
				{
					_keys[cacheType] = dict = new Dictionary<Type, string[]>();
				}
				dict.TryGetValue(graph.GetType(), out ret);
			}
			if (ret == null)
			{
				PXCache cache = graph.Caches[cacheType];
				ret = cache.Keys.ToArray();
				lock (((ICollection)_keys).SyncRoot)
				{
					dict[graph.GetType()] = ret;
				}
			}
			return ret;
		}

		protected KeysCollection _Keys;
        /// <summary>
        /// Gets the collection of fied names that form the primary key
        /// of the data record. The collection is usually composed of
        /// the field that have attributes with the <tt>IsKey</tt> property
        /// set to <tt>true</tt>.
        /// </summary>
		public virtual KeysCollection Keys
		{
			get
			{
				if (_Keys == null)
				{
					_Keys = new KeysCollection();
				}
				return _Keys;
			}
		}

		internal protected string _Identity;

		/// <summary>
		/// When set to <c>true</c> makes the cache sort the accumulator records
		/// by keys before executing update/insert commands, which allows to
		/// avoid deadlocks in case of multithreaded update of the same accumulator table.
		/// </summary>
		[Obsolete("Use CustomDeadlockComparison instead.", error: false)]
		public bool PreventDeadlock
		{
			get { return _PreventDeadlock; }
			set { _PreventDeadlock = value; }
		}
		internal protected bool _PreventDeadlock;

		/// <summary>
		/// When set, makes the cache sort the accumulator records
		/// by custom comparison before executing update/insert commands, which allows to
		/// avoid deadlocks in case of multithreaded update of the same accumulator table.
		/// </summary>
		public Comparison<object> CustomDeadlockComparison
		{
			get { return _CustomDeadlockComparison; }
			set { _CustomDeadlockComparison = value; }
		}
		internal Comparison<object> _CustomDeadlockComparison;

		/// <summary>
        /// Gets the name of the identity field if the DAC defines it.
		/// </summary>
		public virtual string Identity
		{
			get
			{
				return _Identity;
			}
		}

		internal protected bool _AggregateSelecting;
		internal protected bool _SingleTableSelecting;
		internal protected string _RowId;
		/// <summary>
		/// Gets the name of the Row ID (identity, guid or foreign identity) field if the DAC defines it.
		/// </summary>
		public virtual string RowId
		{
			get
			{
				return _RowId;
			}
		}

		internal protected int? _NoteIDOrdinal;
		internal protected string _NoteIDName;
		protected List<Tuple<Delegate, Delegate, Delegate>> _SlotDelegates;
		public virtual int SetupSlot<TSlot>(Func<TSlot> create, Func<TSlot, TSlot, TSlot> update, Func<TSlot, TSlot> clone)
		{
			if (_SlotDelegates == null)
			{
				_SlotDelegates = new List<Tuple<Delegate, Delegate, Delegate>>();
			}
			_SlotDelegates.Add(new Tuple<Delegate, Delegate, Delegate>(
				(Func<object>)(() => create()),
				(Func<object, object, object>)((item, copy) => update((TSlot)item, (TSlot)copy)),
				(Func<object, object>)((item) => clone((TSlot)item))));
			return _SlotDelegates.Count - 1;
		}

        internal virtual int SlotsCount => _SlotDelegates?.Count ?? 0;
		internal protected abstract void _AdjustStorage(int i, PXDataFieldParam assign);
		internal protected abstract void _AdjustStorage(string name, PXDataFieldParam assign);
		internal protected virtual bool _HasKeyValueStored()
		{
			return _KeyValueStoredNames != null || _KeyValueAttributeNames != null;
		}
		internal protected virtual string _ReportGetFirstKeyValueStored(string fieldName)
		{
			return IsKvExtField(fieldName) ? _FirstKeyValueStored.Value.Key : null;
		}
		internal protected virtual string _ReportGetFirstKeyValueAttribute(string fieldName)
		{
			return IsKvExtAttribute(fieldName) ? _FirstKeyValueAttribute.Value.Key : null;
		}

		public bool IsKvExtField(string fieldName)
		{
			return _KeyValueStoredNames != null && _KeyValueStoredNames.ContainsKey(fieldName);
		}

		public virtual bool IsKvExtAttribute(string fieldName)
		{
			return _KeyValueAttributeNames != null && _KeyValueAttributeNames.ContainsKey(fieldName);
		}

		protected List<string> _Immutables;
        /// <summary>
        /// Gets the list of names of the fields that are considered as immutable.
        /// Immutable fields are the fields that have attributes with the
        /// <tt>IsImmutable</tt> property set to <tt>true</tt>.
        /// </summary>
        public virtual List<string> Immutables
        {
            get
            {
                if (_Immutables == null)
                {
                    _Immutables = new List<string>();
                }
                return _Immutables;
            }
        }

		protected internal HashSet<string> _AutoNumberFields;

		/// <summary>
		/// Return true if the <paramref name="field"/> value is auto number
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public bool IsAutoNumber(string field)
		{
			return !string.IsNullOrEmpty(field) && _AutoNumberFields != null && _AutoNumberFields.Contains(field);
		}

		/// <summary>
		/// Set the <paramref name="field"/> as auto number
		/// </summary>
		/// <param name="field"></param>
		public void SetAutoNumber(string field)
		{
			if (!string.IsNullOrEmpty(field) && Fields.Contains(field))
			{
				if (_AutoNumberFields == null)
					_AutoNumberFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				_AutoNumberFields.Add(field);
			}
		}

		protected PXFieldCollection _Fields;

        /// <summary>Gets the collection of names of fields and virtual fields.</summary>
        /// <value>By default, the collection includes all public properties of the DAC that is associated with the cache. The collection may also include the virtual fields that
        /// are injected by attributes (such as the description field of the PXSelector attribute). The developer can add any field to the collection.</value>
		public virtual PXFieldCollection Fields
		{
			get
			{
				if (_Fields == null)
				{
					_Fields = new PXFieldCollection(new string[0], new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
				}
				return _Fields;
			}
		}

		internal virtual PXResult _InvokeTailAppender(object data, SQLTree.Query query, object[] pars)
		{
			return null;
		}

		internal virtual object _InvokeSelectorGetter(object data, string field, PXView view, object[] pars, bool unwrap)
		{
			return null;
		}

		/// <summary>Allows you to intercept the database operations (insertion, update and removal of a row in the database).</summary>
		public abstract PXDBInterceptorAttribute Interceptor
		{
			get;
			set;
		}
        public IPXSelectInterceptor SelectInterceptor { get; set; }
		protected List<Type> _BqlFields;

        /// <summary>Gets the list of classes that implement IBqlField and are nested in the DAC and its base type. These types represent DAC fields in BQL queries. This list
        /// differs from the list that the Fields property returns.</summary>
		public virtual List<Type> BqlFields
		{
			get
			{
				if (_BqlFields == null)
				{
					_BqlFields = new List<Type>();
				}
				return _BqlFields;
			}
		}

        /// <summary>Searchs for the specified BQL field in the <see cref="Fields">Fields</see> collection.</summary>
        /// <param name="bqlField">A BQL field.</param>
        /// <returns></returns>
		public string GetField(Type bqlField)
		{
			string bqlFieldName = bqlField.Name;
			foreach (string field in Fields)
				if (string.Equals(field, bqlFieldName, StringComparison.OrdinalIgnoreCase)) return field;
			return null;
		}


        /// <summary>
        /// Searches for the specified field in the <see cref="PXCache{T}.BqlFields">BqlFields</see>
        /// collection.
        /// </summary>
        /// <param name="field">The field to find.</param>
        /// <returns>The abstract type implementing <tt>IBqlField</tt>.</returns>
		public virtual Type GetBqlField(string field)
		{
			foreach (Type bqlField in BqlFields)
				if (string.Equals(field, bqlField.Name, StringComparison.OrdinalIgnoreCase)) return bqlField;
			return null;
		}

        internal virtual Type GetBaseBqlField(string field)
        {
            return GetBqlField(field);
        }

        /// <exclude />
		protected internal string GetFieldName(string name, bool needBql)
		{
			if (needBql)
			{
				for (int j = BqlFields.Count - 1; j >= 0; j--)
				{
					if (String.Compare(BqlFields[j].Name, name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return BqlFields[j].Name;
					}
				}
			}
			else
			{
				for (int j = 0; j < Fields.Count; j++)
				{
					if (String.Compare(Fields[j], name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return Fields[j];
					}
				}
			}
			return null;
		}

		protected List<Type> _BqlKeys;
        /// <summary>Gets the collection of BQL types that correspond to the key fields that the DAC defines.</summary>
		public virtual List<Type> BqlKeys
		{
			get
			{
				if (_BqlKeys == null)
				{
					_BqlKeys = new List<Type>();
				}
				return _BqlKeys;
			}
		}

        protected List<Type> _BqlImmutables;
        public virtual List<Type> BqlImmutables
        {
            get
            {
                if (_BqlImmutables == null)
                {
                    _BqlImmutables = new List<Type>();
                }
                return _BqlImmutables;
            }
        }
        /// <summary>Looks like obsolete property. The value of this property is not used.</summary>
        /// <exclude />
		public abstract BqlCommand BqlSelect
		{
			get;
			set;
		}
		internal bool BypassCalced;
		internal bool SingleExtended;
        /// <summary>Gets the DAC the cache is associated with. The DAC is specified through the type parameter when the cache is instantiated.</summary>
		public abstract Type BqlTable
		{
			get;
		}

		public virtual Type GenericParameter { get { return null; } }

        /// <exclude />
		public static Type GetBqlTable(Type dac)
		{
			var result = dac;
			while (result != null && 
				typeof(IBqlTable).IsAssignableFrom(result.BaseType) && 
				!result.IsDefined(typeof(PXTableAttribute), false) &&
				(!result.IsDefined(typeof(PXTableNameAttribute), false) || !((PXTableNameAttribute)result.GetCustomAttributes(typeof(PXTableNameAttribute), false)[0]).IsActive))
			{
				result = result.BaseType;
			}
			return result;
		}

        /// <summary>
        /// Save row field values to Dictionary 
        /// </summary>
        /// <param name="data">IBqlTable</param>
        /// <returns></returns>
		public abstract Dictionary<string, object> ToDictionary(object data);

		public abstract string ToXml(object data);

        /// <exclude />
		public abstract object FromXml(string xml);

        /// <exclude />
		public abstract string ValueToString(string fieldName, object val);

		internal virtual string ValueToString(string fieldName, object val, object dbval)
		{
			return ValueToString(fieldName, val);
		}

		internal virtual string AttributeValueToString(string fieldName, object val)
		{
			if (val == null)
			{
				return null;
			}

			if (val is string || _KeyValueAttributeTypes[fieldName] == StorageBehavior.KeyValueString || _KeyValueAttributeTypes[fieldName] == StorageBehavior.KeyValueText)
			{
				return val.ToString();
			}

			if (_KeyValueAttributeTypes[fieldName] == StorageBehavior.KeyValueDate && val is DateTime)
			{
				return ((DateTime)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
			}

			if (_KeyValueAttributeTypes[fieldName] == StorageBehavior.KeyValueNumeric && val is bool)
			{
				return ((bool)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
			}

			return val.ToString();
		}

		/// <exclude />
		public abstract object ValueFromString(string fieldName, string val);

		//protected abstract Dictionary<string, string> FieldAliases
		//{
		//    get;
		//}

        /// <exclude />
		public abstract int GetFieldCount();
        /// <exclude />
		public abstract int GetFieldOrdinal(string field);
        /// <exclude />
		public abstract int GetFieldOrdinal<Field>()
			where Field : IBqlField;

    	internal abstract Type GetFieldType(string fieldName);

        /// <summary>
        /// Repair internal hashtable, if user has changed the key values of stored data rows.
        /// </summary>
		public abstract void Normalize();
		#endregion

		#region External References
		protected PXGraph _Graph;
        /// <summary>Gets or sets the business logic controller the cache is related to.</summary>
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
		#endregion

		#region Access Rights
		protected internal object _CacheSecurity;

		protected bool _SelectRights = true;
		protected internal virtual bool SelectRights
		{
			get
			{
				return _SelectRights;
			}
			set
			{
				_SelectRights = value;
				if (!value)
				{
					_AllowSelect = false;
				}
			}
		}
		protected internal bool _AllowSelect = true;
		protected internal bool _AllowSelectChanged;
		internal bool AutomationHidden;
		internal bool AutomationInsertDisabled;
		internal bool AutomationUpdateDisabled;
		internal bool AutomationDeleteDisabled;
        /// <summary>
        /// Gets or sets the value that indicates whether the cache
        /// allows selection of data records from the user interface.
        /// </summary>
		public virtual bool AllowSelect
		{
			get
			{
				if (!AutomationHidden)
				{
					return _AllowSelect;
				}
				return false;
			}
			set
			{
				if (_SelectRights)
				{
					if (_AllowSelect != value)
					{
						_AllowSelectChanged = true;
					}
					_AllowSelect = value;
				}
			}
		}
		protected bool _UpdateRights = true;
		protected internal virtual bool UpdateRights
		{
			get
			{
				return _UpdateRights;
			}
			set
			{
				_UpdateRights = value;
				if (!value)
				{
					_AllowUpdate = false;
				}
			}
		}
		protected internal bool _AllowUpdate = true;

        /// <summary>
        /// Gets or sets the value that indicates whether the cache allows
        /// update of data records from the user interface. This value does
        /// not affect the ability to update a data record via the methods.
        /// By default, the property equals true.
        /// </summary>
		public virtual bool AllowUpdate
		{
			get
			{
				if (!AutomationUpdateDisabled)
				{
					return _AllowUpdate;
				}
				return false;
			}
			set
			{
				if (_UpdateRights)
				{
					_AllowUpdate = value;
				}
			}
		}
		protected bool _InsertRights = true;
		protected internal virtual bool InsertRights
		{
			get
			{
				return _InsertRights;
			}
			set
			{
				_InsertRights = value;
				if (!value)
				{
					_AllowInsert = false;
				}
			}
		}
		protected internal bool _AllowInsert = true;

        /// <summary>
        /// Gets or sets the value that indicates whether the cache allows insertion
        /// of data records from the user interface. This value does not affect the
        /// ability to insert a data record via the methods. By default, the property
        /// equals <tt>true</tt>.
        /// </summary>
        public virtual bool AllowInsert
		{
			get
			{
				if (!AutomationInsertDisabled)
				{
					return _AllowInsert;
				}
				return false;
			}
			set
			{
				if (_InsertRights)
				{
					_AllowInsert = value;
				}
			}
		}
		protected bool _DeleteRights = true;
		protected internal virtual bool DeleteRights
		{
			get
			{
				return _DeleteRights;
			}
			set
			{
				_DeleteRights = value;
				if (!value)
				{
					_AllowDelete = false;
				}
			}
		}
		protected internal bool _AllowDelete = true;
		
        /// <summary>
        /// Gets or sets the value that indicates whether the cache allows
        /// deletion of data records from the user interface. This value does
        /// not affect the ability to delete a data record via the methods.
        /// By default, the property equals <tt>true</tt>.
        /// </summary>
        public virtual bool AllowDelete
		{
			get
			{
				if (!AutomationDeleteDisabled)
				{
					return _AllowDelete;
				}
				return false;
			}
			set
			{
				if (_DeleteRights)
				{
					_AllowDelete = value;
				}
			}
		}
		#endregion

		#region Commands
		internal abstract object _Clone(object item);
		//public abstract string TableName
		//{
		//    get;
		//    set;
		//}
		internal bool _ReadonlyCreatedCache;
		protected bool _AutoSave;
		public virtual bool AutoSave
		{
			get
			{
				return _AutoSave;
			}
			set
			{
				_AutoSave = value;
			}
		}
		#endregion

		#region Item manipulation methods
        /// <exclude />
		public bool ForceExceptionHandling;
        /// <exclude />
		public static List<Type> getBqlTableAndParents(Type bqlTable)
		{
			List<Type> tables = new List<Type>();
			while (typeof(IBqlTable).IsAssignableFrom(bqlTable))
			{
				tables.Add(bqlTable);
				bqlTable = bqlTable.BaseType;
			}
			return tables;
		}
		internal class IndirectMappingScope : IDisposable
		{
			private Type _extendedGraph;

			internal static Type Get()
			{
				var instance = PXContext.GetSlot<IndirectMappingScope>();
				if (null == instance)
					return null;
				return instance._extendedGraph;
			}
			public IndirectMappingScope(Type graph)
			{
				_extendedGraph = graph;
				PXContext.SetSlot(this);
			}
			public void Dispose()
			{
				PXContext.SetSlot<IndirectMappingScope>(null);
			}
		}
		internal class Mapping
        {
			internal class DefaultMapping : IBqlMapping
			{
				Type _table;
				Type _extension;
				public Type Table => _table;
				public Type Extension => _extension;
				public DefaultMapping(Type table, Type extension)
				{
					_table = table;
					_extension = extension;
				}
			}
			//T1: extended graph, T2: extending mappedcacheextension, there can be multiple mappings in case of N Dacs -> 1 mappedcacheextension
			//TODO: optimize
			private Dictionary<Tuple<Type, Type>, Tuple<Type, List<IBqlMapping>>> _maps =
                                            new Dictionary<Tuple<Type, Type>, Tuple<Type, List<IBqlMapping>>>();
            private HashSet<Type> _disabledGraphExtensions = new HashSet<Type>();
            private Tuple<Type, Type> getKey(Type extendedGraph, Type mappedCacheExtension)
            {
				return Tuple.Create(CustomizedTypeManager.GetTypeNotCustomized(extendedGraph), GetFirstExtensionImplementation(mappedCacheExtension));
            }

            private List<IBqlMapping> FindMaps(Type extendedGraph, Type mappedCacheExtension)
            {
                var key = getKey(extendedGraph, mappedCacheExtension);
                Tuple<Type,List<IBqlMapping>> maps;
                if (_maps.TryGetValue(key, out maps) && maps.Item2.Count > 0)
                    return maps.Item2;
                return null;
            }

            private List<IBqlMapping> FindMaps(Type extendedGraph, Type extendingGraph, string mappedCacheExtensionName)
            {
                //TODO: optimize, although we shouldn't use this method at all as all generic extensions use new event syntax
                extendedGraph = CustomizedTypeManager.GetTypeNotCustomized(extendedGraph);
                var maps = _maps.Where(_ => _.Key.Item1 == extendedGraph &&
                                     (_.Value.Item1.IsAssignableFrom(extendingGraph)
									 || (!_.Value.Item1.BaseType.IsGenericType || !String.Equals(_.Value.Item1.BaseType.GetGenericSimpleName(), typeof(PXGraphExtension).Name, StringComparison.OrdinalIgnoreCase))
									 && _.Value.Item1.BaseType.IsAssignableFrom(extendingGraph)) &&
                                     String.Compare(_.Key.Item2.GetGenericSimpleName(), mappedCacheExtensionName, StringComparison.OrdinalIgnoreCase) == 0).
                               Select(_ => _.Value.Item2).
                               SingleOrDefault();

                if (maps != null && maps.Count > 0)
                    return maps;
                return null;
            }

            private List<IBqlMapping> FindMapsForInheritedGraph(Type extendedGraph, Type mappedCacheExtension)
            {
                List<IBqlMapping> maps = null;
                extendedGraph = CustomizedTypeManager.GetTypeNotCustomized(extendedGraph);
                do
                {
                    maps = FindMaps(extendedGraph, mappedCacheExtension);
                    extendedGraph = extendedGraph.BaseType;
                } while (maps == null &&
                         extendedGraph != typeof(PXGraph));
                return maps;
            }

            private List<IBqlMapping> FindMapsForInheritedGraph(Type extendedGraph, Type extendingGraph, string mappedCacheExtensionName)
            {
                List<IBqlMapping> maps = null;
                extendedGraph = CustomizedTypeManager.GetTypeNotCustomized(extendedGraph);
                do
                {
                    maps = FindMaps(extendedGraph, extendingGraph, mappedCacheExtensionName);
                    extendedGraph = extendedGraph.BaseType;
                } while (maps == null &&
                         extendedGraph != typeof(PXGraph));
                return maps;
            }

            private void AddMap(Type extendedGraph, Type extendingGraph, Type mappedCacheExtension, IBqlMapping mapping)
            {
                var key = getKey(extendedGraph, mappedCacheExtension);
                Tuple<Type, List<IBqlMapping>> maps;
                if (!_maps.TryGetValue(key, out maps))
                {
                    _maps[key] = maps = Tuple.Create(extendingGraph, new List<IBqlMapping>());
                }
                maps.Item2.Add(mapping);
            }

			private static Type GetFirstExtensionImplementation(Type extension)
			{
				Type actualExt = extension;
				while (actualExt.BaseType != null)
				{
					string name = actualExt.BaseType.Name;
					int index = name.IndexOf('`');
					if (index != -1)
						name = name.Substring(0, index);
					if (name == typeof(PXMappedCacheExtension).Name)
						break;
					actualExt = actualExt.BaseType;
				}
				return actualExt;
			}

			public void AddMap(Type extendingGraph, IBqlMapping mapping)
            {
                bool secondLevel;
                Type extendedGraph = PXExtensionManager.GetExtendedGraphType(extendingGraph, out secondLevel);
                AddMap(extendedGraph, extendingGraph, mapping.Extension, mapping);
            }

            public bool TryGetMap(Type extendedGraph, Type mappedCacheExtension, out IBqlMapping map)
            {
                map = FindMapsForInheritedGraph(extendedGraph, mappedCacheExtension)?.FirstOrDefault();
                return map != null;
            }

            public bool TryGetMap(Type extendedGraph, Type mappedCacheExtension, Type extendedType, out IBqlMapping map)
            {
                map = FindMapsForInheritedGraph(extendedGraph, mappedCacheExtension)?.FirstOrDefault(_ => _.Table == extendedType);
                return map != null;
            }

            public bool TryGetMap(Type extendedGraph, Type extendingGraph, string mappedCacheExtensionName, out IBqlMapping map)
            {
                map = FindMapsForInheritedGraph(extendedGraph, extendingGraph, mappedCacheExtensionName)?.FirstOrDefault();
                return map != null;
            }

            public IBqlMapping GetMap(Type extendedGraph, Type mappedCacheExtension)
            {
				IBqlMapping map;
                if (TryGetMap(extendedGraph, mappedCacheExtension, out map))
                    return map;

				var graph = IndirectMappingScope.Get();
				if (graph != null && TryGetMap(graph, mappedCacheExtension, out map))
					return map;

				var pxmce = GetFirstExtensionImplementation(mappedCacheExtension).BaseType;
				if (pxmce.IsGenericType)
				{
					var dac = pxmce.GenericTypeArguments[0];
					return new DefaultMapping(dac, mappedCacheExtension);
				}

				throw new KeyNotFoundException(
                    string.Format("Requested unavailable mapping {0} for graph {1}",
                    mappedCacheExtension.FullName, extendedGraph.FullName));
            }

            public IBqlMapping GetMap(Type extendedGraph, Type mappedCacheExtension, Type extendedType)
            {
				IBqlMapping map;
                if (TryGetMap(extendedGraph, mappedCacheExtension, extendedType, out map))
                    return map;

				var graph = IndirectMappingScope.Get();
				if (graph != null && TryGetMap(graph, mappedCacheExtension, out map))
					return map;

				var pxmce = GetFirstExtensionImplementation(mappedCacheExtension).BaseType;
				if (pxmce.IsGenericType)
				{
					var dac = pxmce.GenericTypeArguments[0];
					return new DefaultMapping(dac, mappedCacheExtension);
				}

				throw new KeyNotFoundException(
                    string.Format("Requested unavailable mapping {0} for dac {1} used in graph {2}",
                    mappedCacheExtension.FullName, extendedType.FullName, extendedGraph.FullName));
            }

            public List<IBqlMapping> GetAllMaps()
            {
                return _maps.Values.SelectMany(_ => _.Item2).ToList();
            }

            public PXCache CreateModelExtension(IBqlMapping map, PXGraph graph)
            {
                return (PXCache)Activator.CreateInstance(typeof(PXModelExtension<>).
                    MakeGenericType(map.Extension), graph, map);
            }

            public PXCache CreateModelExtension(Type mce, PXGraph graph)
            {
                IBqlMapping map = GetMap(graph.GetType(), mce);
                return CreateModelExtension(map, graph);
            }

            public PXCache CreateModelExtension(Type mce, Type et, PXGraph graph)
            {
                IBqlMapping map = GetMap(graph.GetType(), mce, et);
                return CreateModelExtension(map, graph);
            }

            internal void AddDisabledGraphExtension(Type t)
            {
                if(typeof(PXGraphExtension).IsAssignableFrom(t))
                    _disabledGraphExtensions.Add(t);
            }

            internal bool IsDisabled(Type extType)
            {
                return _disabledGraphExtensions.Contains(extType);
            }
        }

        /// <exclude />
		public abstract Type[] GetExtensionTypes();
		static readonly Type[] _AvailableExtensions;
        internal static readonly Mapping _mapping;
        static Type[] _DynamicExtensions;
		static bool _initDynamicExtensionsWatcher;

		public static bool IsActiveExtension(Type extension)
		{
			foreach (Type t in GetAllExtensions())
			{
				if (t == extension)
				{
					try
					{
						System.Reflection.MethodInfo info = t.GetMethod("IsActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase, null, new Type[0], null);
						object res;
						if (info != null && info.ReturnType == typeof(bool) && (res = info.Invoke(null, new object[0])) is bool && !(bool)res)
						{
							return false;
						}
						return true;
					}
					catch
					{
						return false;
					}
				}
			}
			return false;
		}

		static IEnumerable<Type> GetAllExtensions()
		{
			if (WebConfig.DisableExtensions)
				return new Type[0];

			if (_DynamicExtensions == null)
			{
				_DynamicExtensions = PXCodeDirectoryCompiler.GetCompiledTypes<PXCacheExtension>().ToArray();
			}

			if (!_initDynamicExtensionsWatcher)
			{
				_initDynamicExtensionsWatcher = true;
				PXCodeDirectoryCompiler.NotifyOnChange(ClearCaches);
				PXCacheExtensionCollection.ClearCacheExtensionsDelegate = ClearCaches;
			}


			return _AvailableExtensions.Union(_DynamicExtensions);


		}

		internal class CacheStaticInfoDictionary : Dictionary<Type, CacheStaticInfoBase>, IPXCompanyDependent
		{
		}

        static void ClearCaches()
        {
            _DynamicExtensions = null;
            _ExtensionTables = null;
			PXDatabase.ResetSlotForAllCompanies(typeof(AuditSetup).FullName, typeof(PX.SM.AUAuditSetup), typeof(PX.SM.AUAuditTable), typeof(PX.SM.AUAuditField));
			PXDatabase.ResetSlotForAllCompanies("CacheStaticInfo", typeof(PXGraph.FeaturesSet));
            lock (PXExtensionManager._StaticInfoLock)
            {
                PXExtensionManager._CacheStaticInfo.Clear();
            }
        }

		internal static List<Type> _GetExtensions(Type tnode, bool checkActive)
		{
			Dictionary<string, string> inactiveFields;
			return _GetExtensions(tnode, checkActive, out inactiveFields);
		}

        private static bool ExtensionIsActive(Type extensionType)
        {
            System.Reflection.MethodInfo info = extensionType.GetMethod("IsActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase, null, new Type[0], null);
            object res;
            return info == null ||
                   info.ReturnType != typeof(bool) ||
                   !((res = info.Invoke(null, new object[0])) is bool) ||
                   (bool)res;
        }

		internal protected static List<Type> _GetExtensions(Type tnode, bool checkActive, out Dictionary<string, string> inactiveFields)
		{
			inactiveFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			List<Type> tables = getBqlTableAndParents(tnode);
			List<Type> ret = new List<Type>();
			for (int i = tables.Count - 1; i >= 0; i--)
			{
				Type extendedTable = tables[i];
				List<Type> ext = new List<Type>();
				foreach (Type t in GetAllExtensions())
				{
                    // processing extension objects
                    Type[] args;
                    if (typeof(PXCacheExtension).IsAssignableFrom(t) && 
                        t.BaseType.IsGenericType &&
                        (args = t.BaseType.GetGenericArguments()).Length > 0 && 
                        args[args.Length - 1] == extendedTable)
                    {
                        if (!checkActive || ExtensionIsActive(t))
                        {
                            ext.Add(t);
                        }
                        else
                        { 
                            try
                            {
                                foreach (PropertyInfo pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                {
                                    IPXInterfaceField ui = pi.GetCustomAttributes(true).Where(_ => _ is IPXInterfaceField).FirstOrDefault() as IPXInterfaceField;
                                    string dn = pi.Name;
                                    if (ui != null && !String.IsNullOrWhiteSpace(ui.DisplayName))
                                    {
                                        dn = ui.DisplayName;
                                    }
                                    inactiveFields[pi.Name] = dn;
                                }
                            }
                            catch
                            {
                            }
                        }
                        
                    }
				}
                foreach (IBqlMapping map in _mapping.GetAllMaps().
                                                Where(m => m.Table == extendedTable &&
                                                           !ret.Contains(m.Extension)))
                    ext.Add(map.Extension);

                ret.AddRange(PXExtensionManager.Sort(ext));
            }
            //ExtraFieldsDefinition definition = _GetAttributeDefinition(tnode);
            //if (definition != null) {
            //	Type[] genericParams = definition.GetExtensionGenerics(tnode);
            //	Type cacheExt = ExtraFieldsCacheExtensionHelper.allExtensions[genericParams.Length-1];
            //	ret.Insert(0, cacheExt.MakeGenericType(genericParams));
            //}

            return ret;
		}

        /// <summary>Gets an extension of appropriate type.</summary>
		/// <typeparam name="Extension">
		/// The type of extension requested
		/// </typeparam>
		/// <param name="item">
		/// Parent standard object
		/// </param>
		/// <returns>
		/// Object of type Extension
		/// </returns>
		public abstract Extension GetExtension<Extension>(object item)
			where Extension : PXCacheExtension;

        /// <exclude />
		public abstract object GetMain<Extension>(Extension item)
			where Extension : PXCacheExtension;

		public static readonly object NotSetValue = new object();
		public static readonly string IsNewRow = Guid.NewGuid().ToString();

		/// <summary>
		/// Gets the type of data rows in the cache.
		/// </summary>
		/// <returns>Containing type.</returns>
		public abstract Type GetItemType();

		internal object _Current;
        /// <summary>
        ///   <para>Gets or sets the current data record. This property can point to the last data record displayed in the user interface. If the user selects a data record in
        /// a grid, this property points to this data record. If the user or the application inserts, updates, or deletes a data record, the property points to this data
        /// record. Insertion and update of records through the API methods also assign this property.</para>
        /// </summary>
        /// <remarks>
        ///   <para>When this property is assigned, the RowSelected event is raised.</para>
        ///   <para>You can reference the Current data record and its fields in the PXSelect BQL statements by using the Current parameter.</para>
        /// </remarks>
        /// <value>Contains value of type IBqlTable.</value>
		public abstract object Current
		{
			get;
			set;
		}

		/// <summary>
		/// Allow to pass active row from business logic back to UI
		/// Current can not be used for this because its value can be unpredictable changed
		/// </summary>
		public IBqlTable ActiveRow;

		/// <summary>
		/// Allows to pass data key of the row before which new row should be inserted.
		/// </summary>
		public Dictionary<string, object> InsertPosition;

		/// <summary>
		/// If true you should insert 'RowsToMove' after specified 'InsertPosition'.
		/// </summary>
		public bool InsertPositionMode = false;

		/// <summary>
		/// Allows to pass data keys of the rows that need to be moved.
		/// </summary>
		public List<Dictionary<string, object>> RowsToMove;

		public abstract object InternalCurrent
		{
			get;
		}
        /// <summary>Compares the values of the key fields. The list of key fields is taken from the cache.</summary>
        /// <param name="a">IBqlTable or IDictionary</param>
        /// <param name="b">IBqlTable or IDictionary</param>
        /// <returns>Returns true if the values of the key fields are equal.</returns>
        public abstract bool ObjectsEqual(object a, object b);

        /// <summary>Compares the values of the specified fields.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The field to compare.</typeparam>
        /// <returns>Returns true if the values of the specified fields in the specified rows are equal.</returns>
        /// <example>
        ///   <code title="Example" description="To check whether a field has been changed, you use the ObjectsEqual&lt;&gt;() method of the cache as follows." lang="CS">
        /// if (!sender.ObjectsEqual&lt;ShipmentLine.lineQty&gt;(newLine, oldLine)
        ///     ...</code>
        /// </example>
        public bool ObjectsEqual<Field1>(object a, object b)
            where Field1 : IBqlField
        {
            return object.Equals(GetValue<Field1>(a), GetValue<Field1>(b));
        }
        /// <summary>Compares the values of two fields in the specified records.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The first field to compare.</typeparam>
        /// <typeparam name="Field2">The second field to compare.</typeparam>
        /// <returns>If the values of any of the fields you specify differ, the data records are considered different and the method returns false.</returns>
        /// <example>
        ///   <code title="Example" description="You can specify up to eight fields as type parameters of the ObjectsEqual&lt;&gt;() method to compare two data records by using these fields." lang="CS">
        /// if (!sender.ObjectsEqual&lt;ShipmentLine.lineQty,
        ///                          ShipmentLine.cancelled&gt;(newLine, oldLine)
        ///     ...</code>
        /// </example>
        public bool ObjectsEqual<Field1, Field2>(object a, object b)
            where Field1 : IBqlField
            where Field2 : IBqlField
        {
            return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2>(a, b);
        }

        /// <summary>Compares the values of three fields in the specified records.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The first field to compare.</typeparam>
        /// <typeparam name="Field3">The third field to compare.</typeparam>
        /// <typeparam name="Field2">The second field to compare.</typeparam>
        /// <returns>If the values of any of the fields you specify differ, the data records are considered different and the method returns false.</returns>
        public bool ObjectsEqual<Field1, Field2, Field3>(object a, object b)
            where Field1 : IBqlField
            where Field2 : IBqlField
            where Field3 : IBqlField
        {
            return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3>(a, b);
        }
        /// <summary>Compares the values of four fields in the specified records.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The first field to compare.</typeparam>
        /// <typeparam name="Field4">The fourth field to compare.</typeparam>
        /// <typeparam name="Field3">The third field to compare.</typeparam>
        /// <typeparam name="Field2">The second field to compare.</typeparam>
        /// <returns>If the values of any of the fields you specify differ, the data records are considered different and the method returns false.</returns>
        public bool ObjectsEqual<Field1, Field2, Field3, Field4>(object a, object b)
            where Field1 : IBqlField
            where Field2 : IBqlField
            where Field3 : IBqlField
            where Field4 : IBqlField
        {
            return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4>(a, b);
        }
        /// <summary>Compares the values of five fields in the specified records.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The first field to compare.</typeparam>
        /// <typeparam name="Field5">The fifth field to compare.</typeparam>
        /// <typeparam name="Field4">The fourth field to compare.</typeparam>
        /// <typeparam name="Field3">The third field to compare.</typeparam>
        /// <typeparam name="Field2">The second field to compare.</typeparam>
        /// <returns>If the values of any of the fields you specify differ, the data records are considered different and the method returns false.</returns>
        public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5>(object a, object b)
            where Field1 : IBqlField
            where Field2 : IBqlField
            where Field3 : IBqlField
            where Field4 : IBqlField
            where Field5 : IBqlField
        {
            return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5>(a, b);
        }
        /// <summary>Compares the values of six fields in the specified records.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The first field to compare.</typeparam>
        /// <typeparam name="Field6">The sixth field to compare.</typeparam>
        /// <typeparam name="Field5">The fifth field to compare.</typeparam>
        /// <typeparam name="Field4">The fourth field to compare.</typeparam>
        /// <typeparam name="Field3">The third field to compare.</typeparam>
        /// <typeparam name="Field2">The second field to compare.</typeparam>
        /// <returns>If the values of any of the fields you specify differ, the data records are considered different and the method returns false.</returns>
        public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5, Field6>(object a, object b)
            where Field1 : IBqlField
            where Field2 : IBqlField
            where Field3 : IBqlField
            where Field4 : IBqlField
            where Field5 : IBqlField
            where Field6 : IBqlField
        {
            return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5, Field6>(a, b);
        }
        /// <summary>Compares the values of seven fields in the specified records.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The first field to compare.</typeparam>
        /// <typeparam name="Field7">The seventh field to compare.</typeparam>
        /// <typeparam name="Field6">The sixth field to compare.</typeparam>
        /// <typeparam name="Field5">The fifth field to compare.</typeparam>
        /// <typeparam name="Field4">The fourth field to compare.</typeparam>
        /// <typeparam name="Field3">The third field to compare.</typeparam>
        /// <typeparam name="Field2">The second field to compare.</typeparam>
        /// <returns>If the values of any of the fields you specify differ, the data records are considered different and the method returns false.</returns>
        public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5, Field6, Field7>(object a, object b)
            where Field1 : IBqlField
            where Field2 : IBqlField
            where Field3 : IBqlField
            where Field4 : IBqlField
            where Field5 : IBqlField
            where Field6 : IBqlField
            where Field7 : IBqlField
        {
            return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5, Field6, Field7>(a, b);
        }
        /// <summary>Compares the values of eight fields in the specified records.</summary>
        /// <param name="a">The first row to compare.</param>
        /// <param name="b">The second row to compare.</param>
        /// <typeparam name="Field1">The first field to compare.</typeparam>
        /// <typeparam name="Field2">The second field to compare.</typeparam>
        /// <typeparam name="Field3">The third field to compare.</typeparam>
        /// <typeparam name="Field4">The fourth field to compare.</typeparam>
        /// <typeparam name="Field5">The fifth field to compare.</typeparam>
        /// <typeparam name="Field6">The sixth field to compare.</typeparam>
        /// <typeparam name="Field7">The seventh field to compare.</typeparam>
        /// <typeparam name="Field8">The eighth field to compare.</typeparam>
        /// <returns>If the values of any of the fields you specify differ, the data records are considered different and the method returns false.</returns>
        public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8>(object a, object b)
            where Field1 : IBqlField
            where Field2 : IBqlField
            where Field3 : IBqlField
            where Field4 : IBqlField
            where Field5 : IBqlField
            where Field6 : IBqlField
            where Field7 : IBqlField
            where Field8 : IBqlField
        {
            return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5, Field6, Field7, Field8>(a, b);
        }
        /// <summary>Returns the hash code generated from the key field values.</summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <returns></returns>
		public abstract int GetObjectHashCode(object data);

        /// <summary>
        /// Displays key fields in format {k1=v1, k2=v2}
        /// </summary>
        /// <param name="data">IBqlTable or PXResult</param>
        /// <returns></returns>
		public abstract string ObjectToString(object data);

        /// <summary>Looks for an object in the cache and returns the status of the object. The item is located by the values of the key fields.</summary>
        /// <param name="item">Cache item to test, IBqlTable</param>
        /// <returns></returns>
		public abstract PXEntryStatus GetStatus(object item);

        /// <summary>
        /// Looks for an item in the cache and sets the status of the item.
        /// If the item is not in the cache, the item is inserted.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="status"></param>
		public abstract void SetStatus(object item, PXEntryStatus status);

        /// <summary>
        /// Searches data row in the cache.<br/>
        /// Returns located row.
        /// </summary>
        /// <param name="item">IBqlRow</param>
        /// <returns></returns>
		public abstract object Locate(object item);

		protected internal abstract bool IsPresent(object item);

		protected internal abstract bool IsGraphSpecificField(string fieldName);

        /// <summary>
        /// Searches data row in the cache. <br/>
        /// If row not found in cache, <br/>
        /// reads it from the database and places into the cache with status NotChanged.<br/>
        /// </summary>
		public abstract int Locate(IDictionary keys);


        /// <summary>
        /// Remove entry from cache items hashtable.
        /// </summary>
        /// <param name="item"></param>
        public abstract void Remove(object item);

		/// <summary>
        /// Places row into the cache with status Updated.<br/>
        /// If row does not exists in the cache, looks for it in database.<br/>
        /// If row does not exists in database, inserts row with status Inserted.<br/>
        /// Raise events OnRowUpdating, OnRowUpdated and other events.<br/>
        /// This method is used to update row from user interface.<br/>
        /// Flag AllowUpdate may cancel this method.<br/>
        /// returns 1 if updated successfully, otherwise 0.
		/// </summary>
		/// <param name="keys">Primary key of the item.</param>
		/// <param name="values">New field values to update the item with.</param>
		/// <returns>1 if updated successfully, otherwise 0.</returns>
		public abstract int Update(IDictionary keys, IDictionary values);

        /// <summary>
        /// Places row into the cache with status Updated.<br/>
        /// If row does not exists in the cache, looks for it in database.<br/>
        /// If row does not exists in database, inserts row with status Inserted.<br/>
        /// Raise events OnRowUpdating, OnRowUpdated and other events.
        /// Flag AllowUpdate does not affects this method.
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
        /// <returns></returns>
		public abstract object Update(object item);

        /// <summary>
        /// Place row into the cache with status Updated.<br/>
        /// If row does not exists in the cache, looks for it in database.<br/>
        /// If row does not exists in database, inserts row with status Inserted.<br/>
        /// Raise events OnRowUpdating, OnRowUpdated and other events.
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
        /// <param name="bypassinterceptor">whether to ignore PXDBInterceptorAttribute</param>
        protected internal abstract object Update(object item, bool bypassinterceptor);

		/// <summary>
        /// Updates Last Modified state of the row into the cache. <br />
        /// Called at the end of RowUpdated, RowInserted event. <br />
        /// Last modified is used to track properties changes on RowUpdate. <br />
        /// Has influence on Accumulator Attributes <br />
        /// </summary>
        /// <param name="item">Row updated/inserted</param>
        /// <param name="inserted">Flag insert/update</param>
        protected internal abstract void UpdateLastModified(object item, bool inserted);

        protected internal abstract void SetSessionUnmodified(object item, object unmodified);
        /// <summary>
        /// Inserts  new row into the cache. <br/>
        /// Raises events OnRowInserting, OnRowInserted and other field related events.<br/>
        /// Does not check the database for existing row.<br/>
        /// Values of dictionary are not readonly and can be updated during method call.<br/>
        /// Flag AllowInsert may cancel this method.<br/>
        /// Returns 1 if inserted successfully, otherwise 0.<br/>
		/// </summary>
		/// <param name="values">Field values to populate the item before inserting.</param>
		/// <returns>1 if inserted successfully, otherwise 0.</returns>
		public abstract int Insert(IDictionary values);
		internal abstract object FillItem(IDictionary values);


        /// <summary>
        /// Inserts  new row into the cache. <br/>
        /// Returns inserted row of type [CacheItemType] or null if row was not inserted.<br/>
        /// Raises events OnRowInserting, OnRowInserted and other field related events.<br/>
        /// Does not check the database for existing row.
        /// Flag AllowInsert does not affects this method.
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
		public abstract object Insert(object item);

        /// <summary>
        /// Inserts new row into the cache. <br/>
        /// Returns inserted row of type [CacheItemType] or null if row was not inserted.<br/>
        /// Raises events OnRowInserting, OnRowInserted and other field related events.<br/>
        /// Does not check the database for existing row.<br/>
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
        /// <param name="bypassinterceptor">whether to ignore PXDBInterceptorAttribute</param>
        /// <returns></returns>
        protected internal abstract object Insert(object item, bool bypassinterceptor);

        /// <summary>
        /// Place new row into the cache.
        /// </summary>
        /// <returns></returns>
        public abstract object Insert();

        /// <summary>Returns a new TNode();</summary>
        /// <exclude />
		public abstract object CreateInstance();

		internal abstract object ToChildEntity<Parent>(Parent item)
			where Parent : class, IBqlTable, new();

        /// <summary>Inserts a row into the cache if the row type distincts from the cache item type.</summary>
        /// <typeparam name="Parent"></typeparam>
        /// <param name="item"></param>
        /// <returns>Returns the inserted row.</returns>
		public abstract object Extend<Parent>(Parent item)
			where Parent : class, IBqlTable, new();

        /// <summary>Places the item into the cache with the Deleted status. The method raises the OnRowDeleting, OnRowDeleted events. This method is used to delete a row from
        /// the user interface. The <see cref="AllowDelete">AllowDelete</see> property can cancel this method.</summary>
        /// <param name="keys">The primary key of the item.</param>
        /// <param name="values">The parameter is not used. The value can be null.</param>
        /// <returns>Returns 1 if deleted successfully, otherwise returns 0.</returns>
        public abstract int Delete(IDictionary keys, IDictionary values);

        /// <summary>Places the item into the cache with the Deleted status. The method raises the OnRowDeleting, OnRowDeleted events. The <see cref="AllowDelete">AllowDelete</see> property
        /// does not affect this method.</summary>
        /// <param name="item">The item to be placed in the cache.</param>
		public abstract object Delete(object item);


        /// <exclude />
        protected internal abstract object Delete(object item, bool bypassinterceptor);
		/// <summary>
        /// Looks for a row in the cache collection. If the row has the deleted or insertedDeleted status, the method returns null.
        /// Returns the row inserted to the cache.
		/// </summary>
		internal protected abstract void PlaceNotChanged(object data);

		/// <summary>
        /// Looks for a row in the cache collection. If the row has the deleted or insertedDeleted status, the method returns null.
        /// If the status of the row is inserted or updated, the wasUpdated flag is true.
        /// Returns the row inserted to cache.
		/// </summary>
		internal protected abstract object PlaceNotChanged(object data, out bool wasUpdated);

        /// <summary>Creates a data record from the PXDataRecord object and places it into the cache with the NotChanged status if the data record isn't found among the modified
        /// data records in the cache.</summary>
        /// <param name="record">The PXDataRecord object to convert to the DAC type of the cache.</param>
        /// <param name="position">The index of the first field to read in the list of columns comprising the PXDataRecord object.</param>
        /// <param name="isReadOnly">The value that indicates (if true) that the data record with the same key fields should be located in the cache and updated.</param>
        /// <param name="wasUpdated">The value that indicates (if true) that the data record with the same keys existed in the cache among the modified data records.</param>
        /// <remarks>
        ///   <para>If isReadOnly is false then:</para>
        ///   <list type="bullet">
        ///     <item>If the cache already contains the data record with the same keys and the NotChanged status, the method returns this data record updated to the state of
        ///     PXDataRecord.</item>
        ///     <item>If the cache contains the same data record with the Updated or Inserted status, the method returns this data record.</item>
        ///   </list>
        ///   <para>In other cases and when isReadonly is true, the method returns the data record created from the PXDataRecord object.</para>
        ///   <para>If the AllowSelect property is false, the methods returns a new empty data record and the logic described above is not executed.</para>
        ///   <para>The method raises the RowSelecting event.</para>
        /// </remarks>
		public abstract object Select(PXDataRecord record, ref int position, bool isReadOnly, out bool wasUpdated);

		internal abstract object CreateItem(PXDataRecord record, ref int position, bool isReadOnly);
		internal abstract object Select(object record, bool isReadOnly, out bool wasUpdated);

		/// <summary>Initializes a new data record with the field values from the provided data record.</summary>
		/// <param name="item">The data record to copy.</param>
		/// <returns></returns>
		/// <example>
		///   <code title="Example" description="The code below creates a copy of the Current data record of a data view." lang="CS">
		/// public PXSelect&lt;APInvoice, ... &gt; Document;
		/// ...
		/// APInvoice newdoc = PXCache&lt;APInvoice&gt;.CreateCopy(Document.Current);</code>
		/// </example>
		public abstract object CreateCopy(object item);

        /// <summary>
        /// Copy field values from copy to item.
        /// </summary>
        /// <param name="item">IBqlTable</param>
        /// <param name="copy">IBqlTable</param>
		public abstract void RestoreCopy(object item, object copy);
		#endregion

		#region Session state managment methods
		/// <summary>
		/// Loads data rows and other cache state objects from the session.
		/// </summary>
		public abstract void Load();
        internal abstract void LoadFromSession(bool force = false);
        internal abstract void ResetToUnmodified();

		/// <summary>
        /// Saves dirty data rows and other cache state objects into the session.
		/// </summary>
		public abstract void Unload();

        /// <summary>Clears all information stored in the session previously.</summary>
		public abstract void Clear();
        /// <exclude />
		public abstract void ClearItemAttributes();
        /// <exclude />
		public abstract void TrimItemAttributes(object item);

        /// <summary>Clears all cached query result for a given table.</summary>
		public abstract void ClearQueryCacheObsolete();

		public abstract void ClearQueryCache();
		#endregion

		#region Dirty items enumerators
		/// <summary>Gets the collection of updated, inserted, and deleted data records. The collection contains data records with the Updated, Inserted, or Deleted status.</summary>
		public abstract IEnumerable Dirty
        {
            get;
        }

        /// <summary>Gets the collection of deleted data records that exist in the database. The collection contains data records with the Deleted status.</summary>
		public abstract IEnumerable Deleted
		{
			get;
		}

        /// <summary>Gets the collection of updated data records that exist in the database. The collection contains data records with the Updated status.</summary>
		public abstract IEnumerable Updated
		{
			get;
		}

        /// <summary>Gets the collection of inserted data records that does not exist in the database. The collection contains data records with the Inserted status.</summary>
		public abstract IEnumerable Inserted
		{
			get;
		}
        /// <summary>Get the collection of all cached data records. The collection contains data records with any status. The developer should not rely on the presence of data
        /// records with statuses other than Updated, Inserted, and Deleted in this collection.</summary>
        /// <remarks>The collection contains data records with any status. The developer should not rely on the presence of data records with statuses other than Updated, Inserted,
        /// and Deleted in this collection.</remarks>
		public abstract IEnumerable Cached
		{
			get;
		}
		internal abstract int Version
		{
			get; set;
		}
		internal PXCacheOriginalCollection _Originals;

        internal abstract BqlTablePair GetOriginalObjectContext(object item, bool readItemIfNotExists = false);

		internal Tuple<string, int?, int?, string> _GetOriginalCounts(object item)
		{
			lock (((ICollection)_Originals).SyncRoot)
			{
				BqlTablePair orig;
				if (item is IBqlTable && _Originals.TryGetValue((IBqlTable)item, out orig))
				{
					return new Tuple<string, int?, int?, string>(orig.NoteText, orig.FilesCount, orig.ActivitiesCount, orig.NotePopupText);
				}
				return new Tuple<string, int?, int?, string>(null, null, null, null);
			}
		}
		public virtual void SetSlot<TSlot>(object item, int idx, TSlot slot, bool isOriginal = false)
		{
			if (item is IBqlTable)
			{
				lock (((ICollection)_Originals).SyncRoot)
				{
					BqlTablePair orig;
					if (!_Originals.TryGetValue((IBqlTable)item, out orig))
					{
						_Originals[(IBqlTable)item] = orig = new BqlTablePair();
					}
					if (orig.Slots == null)
					{
						orig.Slots = new List<object>(_SlotDelegates.Count);
						orig.SlotsOriginal = new List<object>(_SlotDelegates.Count);
					}
					for (int i = orig.Slots.Count; i <= idx; i++)
					{
						orig.Slots.Add(null);
						orig.SlotsOriginal.Add(null);
					}
					orig.Slots[idx] = slot;
					if (isOriginal && idx < _SlotDelegates.Count)
					{
						orig.SlotsOriginal[idx] = ((Func<object, object>)_SlotDelegates[idx].Item3)(slot);
					}
				}
			}
		}
		public virtual TSlot GetSlot<TSlot>(object item, int idx, bool isOriginal = false)
		{
			if (item is IBqlTable)
			{
				lock (((ICollection)_Originals).SyncRoot)
				{
					BqlTablePair orig;
					if (_Originals.TryGetValue((IBqlTable)item, out orig))
					{
						if (!isOriginal)
						{
							if (orig.Slots != null && idx < orig.Slots.Count)
							{
								return (TSlot)orig.Slots[idx];
							}
						}
						else
						{
							if (orig.SlotsOriginal != null && idx < orig.SlotsOriginal.Count)
							{
								return (TSlot)orig.SlotsOriginal[idx];
							}
						}
					}
				}
			}
			return default(TSlot);
		}
		internal void _SetOriginalCounts(object item, string noteText, int? filesCount, int? activitiesCount, string notePopupText)
		{
			if (item is IBqlTable)
			{
				lock (((ICollection)_Originals).SyncRoot)
				{
					BqlTablePair orig;
					if (!_Originals.TryGetValue((IBqlTable)item, out orig))
					{
						_Originals[(IBqlTable)item] = orig = new BqlTablePair();
					}
					if (noteText != null)
					{
						orig.NoteText = noteText;
					}
					if (filesCount != null)
					{
						orig.FilesCount = filesCount;
					}
					if (activitiesCount != null)
					{
						orig.ActivitiesCount = activitiesCount;
					}
                    if (notePopupText != null)
                    {
                        orig.NotePopupText = notePopupText;
                    }
				}
			}
		}
		internal void _ResetOriginalCounts(object item, bool resetText, bool resetFiles, bool resetActivities)
		{
			if (item is IBqlTable)
			{
				lock (((ICollection)_Originals).SyncRoot)
				{
					BqlTablePair orig;
					if (!_Originals.TryGetValue((IBqlTable)item, out orig))
					{
						_Originals[(IBqlTable)item] = orig = new BqlTablePair();
					}
					if (resetText)
					{
						orig.NoteText = null;
                        orig.NotePopupText = null;
					}
					if (resetFiles)
					{
						orig.FilesCount = null;
					}
					if (resetActivities)
					{
						orig.ActivitiesCount = null;
					}
				}
			}
		}
		internal PXCacheOriginalCollection _OriginalsRemoved;
		#endregion

		#region Persistance to the database
		/// <summary>
        /// Saves changed data rows to the database.<br/>
        /// Raise events OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling
		/// </summary>
		/// <returns>The first item saved.</returns>
		public abstract int Persist(PXDBOperation operation);


        /// <summary>
        /// executes PersistUpdated or PersistInserted or PersistDeleted depends on operation
        /// </summary>
        /// <param name="row"></param>
        /// <param name="operation"></param>
		public abstract void Persist(object row, PXDBOperation operation);


        /// <summary>Saves the updated data record to the database.</summary>
        /// <remarks>
        ///   <para>The method raises the OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling events.</para>
        ///   <para>The default behavior can be modified by PXDBInterceptorAttribute.</para>
        /// </remarks>
		public virtual bool PersistUpdated(object row)
		{
			return PersistUpdated(row, false);
		}
		internal protected abstract bool PersistUpdated(object row, bool bypassInterceptor);


        /// <summary>Inserts the provided data record into the database.<br /></summary>
        /// <remarks>
        ///   <para>The exception is thrown if a row with such keys exists in the database.</para>
        ///   <para>The method raises the OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling events.</para>
        ///   <para>The default behavior can be modified by PXDBInterceptorAttribute.</para>
        /// </remarks>
		public virtual bool PersistInserted(object row)
		{
			return PersistInserted(row, false);
		}
		internal protected abstract bool PersistInserted(object row, bool bypassInterceptor);


        /// <summary>Deletes the provided data record from the database by the key fields</summary>
        /// <remarks>The method raises the OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling events.<br />
        ///  The default behavior can be modified by PXDBInterceptorAttribute.</remarks>
		public virtual bool PersistDeleted(object row)
		{
			return PersistDeleted(row, false);
		}
		internal protected abstract bool PersistDeleted(object row, bool bypassInterceptor);

		/// <summary>
		/// For each persisted row - raise OnRowPersisted, SetStatus(Notchanged)
		/// </summary>
		public abstract void Persisted(bool isAborted);

        /// <summary>
        /// For each persisted row - raise OnRowPersisted, SetStatus(Notchanged)
        /// </summary>
		protected internal abstract void Persisted(bool isAborted, Exception exception);

        /// <summary>
        /// remove row from list of persited items
        /// </summary>
        /// <param name="row"></param>
		public abstract void ResetPersisted(object row);


		protected bool _IsDirty;

        /// <summary>Gets or sets the value that indicates whether the cache contains the modified data records.</summary>
		public virtual bool IsDirty
		{
			get
			{
				if (!AutoSave)
				{
					return _IsDirty;
				}
				return false;
			}
			set
			{
				_IsDirty = value;
			}
		}

        /// <summary>Gets the value that indicates if the cache contains modified data records to be saved to database.</summary>
        public abstract bool IsInsertedUpdatedDeleted
        {
            get;
        }
		#endregion

		#region Events
		internal protected sealed class EventsRow
		{
			internal List<PXRowSelecting> _RowSelectingList = new List<PXRowSelecting>();
			private PXRowSelecting _RowSelectingDelegate;
			public PXRowSelecting RowSelecting
			{
				get
				{
					if (_RowSelectingList != null && _RowSelectingList.Count > 0)
					{
						_RowSelectingDelegate = (PXRowSelecting)Delegate.Combine(_RowSelectingList.ToArray());
						_RowSelectingList = null;
					}
					return _RowSelectingDelegate;
				}
				set
				{
					_RowSelectingDelegate = value;
				}
			}
			internal List<PXRowSelected> _RowSelectedList = new List<PXRowSelected>();
			private PXRowSelected _RowSelectedDelegate;
			public PXRowSelected RowSelected
			{
				get
				{
					if (_RowSelectedList != null && _RowSelectedList.Count > 0)
					{
						_RowSelectedDelegate = (PXRowSelected)Delegate.Combine(_RowSelectedList.ToArray());
						_RowSelectedList = null;
					}
					return _RowSelectedDelegate;
				}
				set
				{
					_RowSelectedDelegate = value;
				}
			}
			internal List<PXRowInserting> _RowInsertingList = new List<PXRowInserting>();
			private PXRowInserting _RowInsertingDelegate;
			public PXRowInserting RowInserting
			{
				get
				{
					if (_RowInsertingList != null && _RowInsertingList.Count > 0)
					{
						_RowInsertingDelegate = (PXRowInserting)Delegate.Combine(_RowInsertingList.ToArray());
						_RowInsertingList = null;
					}
					return _RowInsertingDelegate;
				}
				set
				{
					_RowInsertingDelegate = value;
				}
			}
			internal List<PXRowInserted> _RowInsertedList = new List<PXRowInserted>();
			private PXRowInserted _RowInsertedDelegate;
			public PXRowInserted RowInserted
			{
				get
				{
					if (_RowInsertedList != null && _RowInsertedList.Count > 0)
					{
						_RowInsertedDelegate = (PXRowInserted)Delegate.Combine(_RowInsertedList.ToArray());
						_RowInsertedList = null;
					}
					return _RowInsertedDelegate;
				}
				set
				{
					_RowInsertedDelegate = value;
				}
			}
			internal List<PXRowUpdating> _RowUpdatingList = new List<PXRowUpdating>();
			private PXRowUpdating _RowUpdatingDelegate;
			public PXRowUpdating RowUpdating
			{
				get
				{
					if (_RowUpdatingList != null && _RowUpdatingList.Count > 0)
					{
						_RowUpdatingDelegate = (PXRowUpdating)Delegate.Combine(_RowUpdatingList.ToArray());
						_RowUpdatingList = null;
					}
					return _RowUpdatingDelegate;
				}
				set
				{
					_RowUpdatingDelegate = value;
				}
			}
			internal List<PXRowUpdated> _RowUpdatedList = new List<PXRowUpdated>();
			private PXRowUpdated _RowUpdatedDelegate;
			public PXRowUpdated RowUpdated
			{
				get
				{
					if (_RowUpdatedList != null && _RowUpdatedList.Count > 0)
					{
						_RowUpdatedDelegate = (PXRowUpdated)Delegate.Combine(_RowUpdatedList.ToArray());
						_RowUpdatedList = null;
					}
					return _RowUpdatedDelegate;
				}
				set
				{
					_RowUpdatedDelegate = value;
				}
			}
			internal List<PXRowDeleting> _RowDeletingList = new List<PXRowDeleting>();
			private PXRowDeleting _RowDeletingDelegate;
			public PXRowDeleting RowDeleting
			{
				get
				{
					if (_RowDeletingList != null && _RowDeletingList.Count > 0)
					{
						_RowDeletingDelegate = (PXRowDeleting)Delegate.Combine(_RowDeletingList.ToArray());
						_RowDeletingList = null;
					}
					return _RowDeletingDelegate;
				}
				set
				{
					_RowDeletingDelegate = value;
				}
			}
			internal List<PXRowDeleted> _RowDeletedList = new List<PXRowDeleted>();
			private PXRowDeleted _RowDeletedDelegate;
			public PXRowDeleted RowDeleted
			{
				get
				{
					if (_RowDeletedList != null && _RowDeletedList.Count > 0)
					{
						_RowDeletedDelegate = (PXRowDeleted)Delegate.Combine(_RowDeletedList.ToArray());
						_RowDeletedList = null;
					}
					return _RowDeletedDelegate;
				}
				set
				{
					_RowDeletedDelegate = value;
				}
			}
			internal List<PXRowPersisting> _RowPersistingList = new List<PXRowPersisting>();
			private PXRowPersisting _RowPersistingDelegate;
			public PXRowPersisting RowPersisting
			{
				get
				{
					if (_RowPersistingList != null && _RowPersistingList.Count > 0)
					{
						_RowPersistingDelegate = (PXRowPersisting)Delegate.Combine(_RowPersistingList.ToArray());
						_RowPersistingList = null;
					}
					return _RowPersistingDelegate;
				}
				set
				{
					_RowPersistingDelegate = value;
				}
			}
			internal List<PXRowPersisted> _RowPersistedList = new List<PXRowPersisted>();
			private PXRowPersisted _RowPersistedDelegate;
			public PXRowPersisted RowPersisted
			{
				get
				{
					if (_RowPersistedList != null && _RowPersistedList.Count > 0)
					{
						_RowPersistedDelegate = (PXRowPersisted)Delegate.Combine(_RowPersistedList.ToArray());
						_RowPersistedList = null;
					}
					return _RowPersistedDelegate;
				}
				set
				{
					_RowPersistedDelegate = value;
				}
			}
		}
		//internal protected sealed class EventsField
		//{
		//    public PXCommandPreparing CommandPreparing;
		//    public PXFieldDefaulting FieldDefaulting;
		//    public PXFieldUpdating FieldUpdating;
		//    public PXFieldVerifying FieldVerifying;
		//    public PXFieldUpdated FieldUpdated;
		//    public PXFieldSelecting FieldSelecting;
		//    public PXExceptionHandling ExceptionHandling;
		//}
		protected sealed class EventsRowAttr
		{
			public IPXRowSelectingSubscriber[] RowSelecting;
			public IPXRowSelectedSubscriber[] RowSelected;
			public IPXRowInsertingSubscriber[] RowInserting;
			public IPXRowInsertedSubscriber[] RowInserted;
			public IPXRowUpdatingSubscriber[] RowUpdating;
			public IPXRowUpdatedSubscriber[] RowUpdated;
			public IPXRowDeletingSubscriber[] RowDeleting;
			public IPXRowDeletedSubscriber[] RowDeleted;
			public IPXRowPersistingSubscriber[] RowPersisting;
			public IPXRowPersistedSubscriber[] RowPersisted;
		}
		//protected sealed class EventsFieldAttr
		//{
		//    public IPXCommandPreparingSubscriber[] CommandPreparing;
		//    public IPXFieldDefaultingSubscriber[] FieldDefaulting;
		//    public IPXFieldUpdatingSubscriber[] FieldUpdating;
		//    public IPXFieldVerifyingSubscriber[] FieldVerifying;
		//    public IPXFieldUpdatedSubscriber[] FieldUpdated;
		//    public IPXFieldSelectingSubscriber[] FieldSelecting;
		//    public IPXExceptionHandlingSubscriber[] ExceptionHandling;
		//}
		
		protected internal sealed class EventsFieldMap
		{
			public List<IPXCommandPreparingSubscriber> CommandPreparing = new List<IPXCommandPreparingSubscriber>();
			public List<IPXFieldDefaultingSubscriber> FieldDefaulting = new List<IPXFieldDefaultingSubscriber>();
			public List<IPXFieldUpdatingSubscriber> FieldUpdating = new List<IPXFieldUpdatingSubscriber>();
			public List<IPXFieldVerifyingSubscriber> FieldVerifying = new List<IPXFieldVerifyingSubscriber>();
			public List<IPXFieldUpdatedSubscriber> FieldUpdated = new List<IPXFieldUpdatedSubscriber>();
			public List<IPXFieldSelectingSubscriber> FieldSelecting = new List<IPXFieldSelectingSubscriber>();
			public List<IPXExceptionHandlingSubscriber> ExceptionHandling = new List<IPXExceptionHandlingSubscriber>();
		}
		internal class EventPosition
		{
			public int RowSelectingFirst = -1;
			public int RowSelectingLength;
			public int RowSelectedFirst = -1;
			public int RowSelectedLength;
			public int RowInsertingFirst = -1;
			public int RowInsertingLength;
			public int RowInsertedFirst = -1;
			public int RowInsertedLength;
			public int RowUpdatingFirst = -1;
			public int RowUpdatingLength;
			public int RowUpdatedFirst = -1;
			public int RowUpdatedLength;
			public int RowDeletingFirst = -1;
			public int RowDeletingLength;
			public int RowDeletedFirst = -1;
			public int RowDeletedLength;
			public int RowPersistingFirst = -1;
			public int RowPersistingLength;
			public int RowPersistedFirst = -1;
			public int RowPersistedLength;
			public int CommandPreparingFirst = -1;
			public int CommandPreparingLength;
		}
        /// <summary>The internal delegate for the <tt>CacheAttached</tt> event.</summary>
        /// <param name="graph">(Required). The <see cref="PXGraph" /> object to which the cache is attached.</param>
        /// <param name="sender">(Required). The <see cref="PXCache" /> object that raised the event.</param>
        /// <remarks>
        ///   <para>The <tt>CacheAttached</tt> handler is used to override data access class (DAC) field attributes declared directly within the DAC. By declaring a
        /// <tt>CacheAttached</tt> handler and attaching <span>appropriate attributes</span> to the handler within a graph, the developer forces the framework to
        /// completely override DAC field attributes within this graph.</para>
        ///   <para>The execution order for the <tt>CacheAttached</tt> event handlers is the following:</para>
        ///   <list type="number">
        ///     <item>Attribute event handlers are executed.</item>
        ///     <item>Graph event handlers are executed.</item>
        ///   </list>
        ///   <para></para>
        /// </remarks>
        /// <example>
        ///   <code title="Example" description="You should define a graph event handler as follows. Event handlers in particular graphs do not have the graph parameter." lang="CS">
        /// [DAC_Field_Attribute_1]
        ///  ...
        ///  [DAC_Field_Attribute_N]
        ///  protected virtual void DACName_FieldName_CacheAttached(PXCache sender)
        ///  {
        ///  ...
        ///  }</code>
        ///   <code title="Example2" description="The code below overrides DAC field attributes within a graph." lang="CS">
        /// public class DimensionMaint : PXGraph&lt;DimensionMaint, Dimension&gt;
        /// {
        ///     ...
        ///     
        ///     [PXDBString(15, IsUnicode = true, IsKey = true)]
        ///     [PXDefault(typeof(Dimension.dimensionID))]
        ///     [PXUIField(DisplayName = "Dimension ID", Visibility = 
        ///     PXUIVisibility.Invisible, Visible = false)]
        ///     [PXSelector(typeof(Dimension.dimensionID), DirtyRead = true)]
        ///     protected virtual void Segment_DimensionID_CacheAttached(PXCache sender)
        ///     {
        ///  
        ///     }
        ///     
        ///     ...
        /// }</code>
        /// </example>
        internal delegate void _CacheAttachedDelegate(PXGraph graph, PXCache sender);
        internal protected EventsRow _EventsRow = new EventsRow();
		protected EventsRowAttr _EventsRowAttr = new EventsRowAttr();
		//protected Dictionary<object, EventsRowAttr> _EventsRowItem;

		#endregion
		#region Public Properties
		/// <summary>
        /// Gets or sets the user-friendly name set by the
        /// <see cref="PXCacheNameAttribute">PXCacheName</see> attribute.
        /// </summary>
		public string DisplayName { get; set; }

		#endregion

    	internal abstract void ClearSession();

		internal virtual bool HasChangedKeys()
		{
			return false;
		}

        /// <exclude />
	    public virtual bool IsKeysFilled(object item)
	    {
#pragma warning disable 618
		    return isKeysFilled(item);
#pragma warning restore 618
	    }

	    internal static bool IsOrigValueNewDate(PXCache sender, PXCommandPreparingEventArgs.FieldDescription origdescription)
	    {
	        return origdescription.DataType==PXDbType.DirectExpression&&(Equals(origdescription.DataValue, sender.Graph.SqlDialect.GetDate)||Equals(origdescription.DataValue, sender.Graph.SqlDialect.GetUtcDate));
	    }

		[Obsolete("This method has been renamed to IsKeysFilled")]
	    internal bool isKeysFilled(object item)
	    {
		    foreach (string key in _Keys)
		    {
			    if (GetValue(item, key) == null)
			    {
				    return false;
			    }
		    }
		    return true;
	    }

	    public abstract int LocateByNoteID(Guid noteId);

		internal protected static PropertyInfo[] _GetProperties(Type type, List<Type> extensions, out Dictionary<string, List<PropertyInfo>> nameToFieldPropertyInfo, out int origLength)
		{
			PropertyInfo[] orig = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.OrderBy(f => f, new PXGraph.FieldInfoComparer()).ToArray();
			PropertyInfo[] info = new PropertyInfo[orig.Length];
			int ii = 0;
			while (ii < info.Length)
			{
				int oi = orig.Length - ii - 1;
				while (oi > 0 && orig[orig.Length - ii - 1].DeclaringType == orig[oi - 1].DeclaringType)
				{
					oi--;
				}
				Array.Copy(orig, oi, info, ii, orig.Length - ii - oi);
				ii = orig.Length - oi;
			}

			nameToFieldPropertyInfo = null;

			if (extensions.Count > 0)
			{
				HashSet<string> baseproperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				List<PropertyInfo> addproperties = new List<PropertyInfo>();
				foreach (PropertyInfo basepi in info)
				{
					baseproperties.Add(basepi.Name);
				}
				nameToFieldPropertyInfo = new Dictionary<string, List<PropertyInfo>>(StringComparer.OrdinalIgnoreCase);
				foreach (Type ext in extensions)
				{
					foreach (PropertyInfo extpi in ext.GetProperties(BindingFlags.Public | BindingFlags.Instance))
					{
						List<PropertyInfo> found;
						if (!nameToFieldPropertyInfo.TryGetValue(extpi.Name, out found))
						{
							nameToFieldPropertyInfo[extpi.Name] = found = new List<PropertyInfo>();
						}
						found.Insert(0, extpi);
					}
				}

                //optimize
                foreach (var map in _mapping.GetAllMaps().
                    Where(m => m.Table.IsAssignableFrom(type) &&
                               extensions.Contains(m.Extension)))
                {
                    var mapProperties = map.Extension.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (FieldInfo fi in map.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                    {
                        List<PropertyInfo> propertyInfoList;
                        string fvname = ((Type)fi.GetValue(map)).Name;
                        //extension field name differs from original name
                        if (fi.FieldType == typeof(Type) &&
                            !string.Equals(fvname, fi.Name, StringComparison.OrdinalIgnoreCase) &&
                            nameToFieldPropertyInfo.TryGetValue(fi.Name, out propertyInfoList))
                        {
                            List<PropertyInfo> existingPropertyInfoList;
                            PropertyInfo extensionPI = propertyInfoList.FirstOrDefault(p => mapProperties.Contains(p));
                            propertyInfoList.Remove(extensionPI);
                            if (propertyInfoList.Count == 0)
                                nameToFieldPropertyInfo.Remove(fi.Name);
                            if (nameToFieldPropertyInfo.TryGetValue(fvname, out existingPropertyInfoList))
                                existingPropertyInfoList.Add(extensionPI);
                            else
                                nameToFieldPropertyInfo[fvname] = new List<PropertyInfo> { extensionPI };
                        }
                    }
                }

                {
					foreach (List<PropertyInfo> extpi in nameToFieldPropertyInfo.Values)
					{
						if (!baseproperties.Contains(extpi[0].Name) && 
                            !addproperties.Any(_ => _.Name == extpi[0].Name))
						{
							addproperties.Add(extpi[0]);
						}
					}
				}
				if (addproperties.Count > 0)
				{
					Array.Resize(ref info, info.Length + addproperties.Count);
					Array.Copy(addproperties.ToArray(), 0, info, info.Length - addproperties.Count, addproperties.Count);
				}
			}
			origLength = orig.Length;
			return info;
		}

		/// <remarks>
		/// See <a href="https://wiki.acumatica.com/display/TND/Primary+and+Foreign+Key+API">Foreign Key API</a> for more details.
		/// </remarks>
		private protected static void CollectForeignKeys(Type type, IEnumerable<Type> extensions)
		{
			void CollectForeignKeysImpl(Type t)
			{
				foreach (var nestedType in t.GetNestedTypes())
				{
					if (nestedType.IsAbstract && nestedType.IsSealed) // if static
					{
						CollectForeignKeysImpl(nestedType);
					}

					var current = nestedType;
					while (current != null && current != typeof(object))
					{
						if (current.IsGenericType &&
						    typeof(ReferentialIntegrity.Attributes.KeysRelation<,,>).IsAssignableFrom(
							    current.GetGenericTypeDefinition()))
						{
							var method = current.GetMethod("CollectReference", BindingFlags.Static | BindingFlags.NonPublic);
							method?.Invoke(null, Array<object>.Empty);
							break;
						}

						current = current.BaseType;
					}
				}
			}

			while (type != null && type != typeof(object))
			{
				CollectForeignKeysImpl(type);
				type = type.BaseType;
			}

			foreach (Type extType in extensions)
				CollectForeignKeysImpl(extType);
		}
	}

    public interface IPXSelectInterceptor
    {
        IEnumerable<object> Select(PXGraph graph, BqlCommand command, int topCount, PXView view, PXDataValue[] pars);
    }

    public static class PXCacheEx
	{
		public static IEqualityComparer<object> GetComparer(this PXCache cache) => new CacheItemComparer<object>(cache);
		public static IEqualityComparer<TNode> GetComparer<TNode>(this PXCache<TNode> cache) where TNode : class, IBqlTable, new() => new CacheItemComparer<TNode>(cache);
		internal class CacheItemComparer<T> : IEqualityComparer<T>
		{
			private readonly PXCache _cache;
			public CacheItemComparer(PXCache cache) { _cache = cache; }
			int IEqualityComparer<T>.GetHashCode(T item) => _cache.GetObjectHashCode(item);
			bool IEqualityComparer<T>.Equals(T x, T y) => _cache.ObjectsEqual(x, y);
		}

		public static TExtension GetExtension<TExtension>(this IBqlTable item)
			where TExtension : PXCacheExtension
		{
			if (item == null)
				return null;

			var dict = PXContext.GetSlot<PXCacheExtensionCollection>();
			if (dict == null)
			{
				dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
			}


			lock (((ICollection) dict).SyncRoot)
			{
				PXCacheExtension[] itemextensions;

				if (dict.TryGetValue(item, out itemextensions))
				{
					return (TExtension) itemextensions.FirstOrDefault((x) => x is TExtension && x.GetType() == typeof (TExtension));
				}
			}


			throw new PXException(MsgNotLocalizable.GetItemExtensionFailed);


		}

		public static TExtension GetExtension<TBqlTable, TExtension>(this TBqlTable item)
			where TExtension : PXCacheExtension<TBqlTable>
			where TBqlTable : class, IBqlTable, new()
		{
			return PXCache<TBqlTable>.GetExtension<TExtension>(item);
		}

		public static PXCacheExtension[] GetExtensions(this IBqlTable item)
		{
			if (item == null)
				return null;

			var dict = PXContext.GetSlot<PXCacheExtensionCollection>();
			if (dict == null)
			{
				dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
			}


			lock (((ICollection) dict).SyncRoot)
			{
				PXCacheExtension[] itemextensions;

				if (dict.TryGetValue(item, out itemextensions))
				{
					return itemextensions;
				}
			}


			return null;


		}

		/// <summary>
		/// Provides an object that allows to adjust the state of a <see cref="PXAttributeLevel.Item"/>-attribute of the <see cref="PXUIFieldAttribute"/> type, that is attached to the cache, in runtime.
		/// </summary>
		/// <param name="cache">Cache which the attribute is attached to.</param>
		/// <param name="row">Row which the attribute should be adjusted for.</param>
		public static AttributeAdjuster<PXUIFieldAttribute> AdjustUI(this PXCache cache, object row = null) => new AttributeAdjuster<PXUIFieldAttribute>(cache, row, false);

		/// <summary>
		/// Provides an object that allows to adjust the state of a <see cref="PXAttributeLevel.Item"/>-attribute of a specific type, that is attached to the cache, in runtime.
		/// </summary>
		/// <typeparam name="TAttribute">Type of the adjusted attribute.</typeparam>
		/// <param name="cache">Cache which the attribute is attached to.</param>
		/// <param name="row">Row which the attribute should be adjusted for.</param>
		public static AttributeAdjuster<TAttribute> Adjust<TAttribute>(this PXCache cache, object row = null)
			where TAttribute : PXEventSubscriberAttribute => new AttributeAdjuster<TAttribute>(cache, row, false);

		/// <summary>
		/// Provides an object that allows to adjust the state of a <see cref="PXAttributeLevel.Cache"/>-attribute of the <see cref="PXUIFieldAttribute"/> type, that is attached to the cache, in runtime.
		/// </summary>
		/// <param name="cache">Cache which the attribute is attached to.</param>
		/// <param name="row">Row which the attribute should be adjusted for.</param>
		public static AttributeAdjuster<PXUIFieldAttribute> AdjustUIReadonly(this PXCache cache, object row = null) => new AttributeAdjuster<PXUIFieldAttribute>(cache, row, true);

		/// <summary>
		/// Provides an object that allows to adjust the state of a <see cref="PXAttributeLevel.Cache"/>-attribute of a specific type, that is attached to the cache, in runtime.
		/// </summary>
		/// <typeparam name="TAttribute">Adjusted attribute type.</typeparam>
		/// <param name="cache">Cache which the attribute is attached to.</param>
		/// <param name="row">Row which the attribute should be adjusted for.</param>
		public static AttributeAdjuster<TAttribute> AdjustReadonly<TAttribute>(this PXCache cache, object row = null)
			where TAttribute : PXEventSubscriberAttribute => new AttributeAdjuster<TAttribute>(cache, row, true);

		[ImmutableObject(true)]
		public readonly struct AttributeAdjuster<TAttribute>
			where TAttribute : PXEventSubscriberAttribute
		{
			private readonly bool _readonly;
			private readonly PXCache _cache;
			private readonly object _row;
			internal AttributeAdjuster([NotNull] PXCache cache, Object row, Boolean @readonly)
			{
				if (cache == null) throw new ArgumentNullException(nameof(cache));
				_cache = cache;
				_readonly = @readonly;
				_row = row;
			}

			/// <summary>
			/// Adjust attributes attached to any fields by applying the <paramref name="adjustment"/> action.
			/// </summary>
			/// <param name="adjustment">An attribute adjustment action</param>
			public AttributeAdjuster<TAttribute> ForAllFields([NotNull] Action<TAttribute> adjustment)
			{
				Adjust(null, adjustment);
				return this;
			}

			/// <summary>
			/// Adjust attributes attached to <typeparamref name="TField"/> by applying the <paramref name="adjustment"/> action,
			/// and memorize <paramref name="adjustment"/> action for further chaining.
			/// </summary>
			/// <param name="adjustment">An attribute adjustment action</param>
			public Chained For<TField>([NotNull] Action<TAttribute> adjustment) where TField : IBqlField => For(typeof(TField).Name, adjustment);

			/// <summary>
			/// Adjust attributes attached to <paramref name="fieldName"/> by applying the <paramref name="adjustment"/> action,
			/// and memorize <paramref name="adjustment"/> action for further chaining.
			/// </summary>
			/// <param name="adjustment">An attribute adjustment action</param>
			public Chained For(string fieldName, [NotNull] Action<TAttribute> adjustment)
			{
				Adjust(fieldName, adjustment);
				return new Chained(this, adjustment);
			}

			private void Adjust(String fieldName, Action<TAttribute> adjustment)
			{
				if (adjustment == null) throw new ArgumentNullException(nameof(adjustment));

				if (_row == null)
					_cache.SetAltered(fieldName, true);
				var attributes = _readonly
					? _cache.GetAttributesReadonly(_row, fieldName)
					: _cache.GetAttributes(_row, fieldName);
				foreach (var attr in attributes.OfType<TAttribute>())
					adjustment(attr);
			}

			[ImmutableObject(true)]
			public readonly struct Chained
			{
				private readonly AttributeAdjuster<TAttribute> _previousAdjuster;
				private readonly Action<TAttribute> _previousAction;

				internal Chained(AttributeAdjuster<TAttribute> previousAdjuster, Action<TAttribute> previousAction)
				{
					_previousAdjuster = previousAdjuster;
					_previousAction = previousAction;
				}

				/// <summary>
				/// Adjust attributes attached to any fields by applying the <paramref name="adjustment"/> action.
				/// </summary>
				/// <param name="adjustment">An attribute adjustment action</param>
				public AttributeAdjuster<TAttribute> ForAllFields([NotNull] Action<TAttribute> adjustment) => _previousAdjuster.ForAllFields(adjustment);

				/// <summary>
				/// Adjust attributes attached to <typeparamref name="TField"/> by applying the <paramref name="adjustment"/> action,
				/// and override previous memorized <paramref name="adjustment"/> action.
				/// </summary>
				/// <param name="adjustment">An attribute adjustment action</param>
				public Chained For<TField>([NotNull] Action<TAttribute> adjustment) where TField : IBqlField => _previousAdjuster.For<TField>(adjustment);

				/// <summary>
				/// Adjust attributes attached to <paramref name="fieldName"/> by applying the <paramref name="adjustment"/> action,
				/// and override previous memorized <paramref name="adjustment"/> action.
				/// </summary>
				/// <param name="adjustment">An attribute adjustment action</param>
				public Chained For(string fieldName, [NotNull] Action<TAttribute> adjustment) => _previousAdjuster.For(fieldName, adjustment);

				/// <summary>
				/// Adjust attributes attached to <typeparamref name="TField"/> by applying the previously memorized adjustment action.
				/// </summary>
				public Chained SameFor<TField>() where TField : IBqlField => SameFor(typeof(TField).Name);

				/// <summary>
				/// Adjust attributes attached to <paramref name="fieldName"/> by applying the previously memorized adjustment action.
				/// </summary>
				public Chained SameFor(string fieldName) => _previousAdjuster.For(fieldName, _previousAction);
			}
		}

		public static ValueSetter<TEntity> GetSetterFor<TEntity>(this PXGraph graph, TEntity entity)
			where TEntity : class, IBqlTable, new()
		{
			return GetSetterFor(graph?.Caches<TEntity>(), entity);
		}
		public static ValueSetter<TEntity> GetSetterFor<TEntity>(this PXSelectBase<TEntity> select, TEntity entity)
			where TEntity : class, IBqlTable, new()
		{
			return GetSetterFor(select?.Cache, entity);
		}
		public static ValueSetter<TEntity> GetSetterForCurrent<TEntity>(this PXSelectBase<TEntity> select)
			where TEntity : class, IBqlTable, new()
		{
			return GetSetterFor(select?.Cache, select?.Current);
		}
		public static ValueSetter<TEntity> GetSetterFor<TEntity>(this PXCache cache, TEntity entity)
			where TEntity : class, IBqlTable, new()
		{
			ValidateParameters(cache, entity);
			return new ValueSetter<TEntity>(cache, entity, false);
		}
		private static void ValidateParameters<TEntity>(PXCache cache, TEntity entity)
			where TEntity : class, IBqlTable, new()
		{
			cache.ThrowOnNull(nameof(cache));
			entity.ThrowOnNull(nameof(entity));
		}
	}

	[ImmutableObject(true)]
	public readonly struct ValueSetter<TEntity>
			where TEntity : class, IBqlTable, new()
	{
		private readonly PXCache _cache;
		private readonly TEntity _entity;
		private readonly bool _fireFieldEvents;
		internal ValueSetter(PXCache cache, TEntity entity, bool fireFieldEvents)
		{
			_cache = cache;
			_entity = entity;
			_fireFieldEvents = fireFieldEvents;
		}
		public void Set<TValue>(Expression<Func<TEntity, TValue>> fieldSelector, TValue value)
		{
			if (_fireFieldEvents)
				_cache.SetValueExt(_entity, ((MemberExpression)fieldSelector.Body).Member.Name, value);
			else
				_cache.SetValue(_entity, ((MemberExpression)fieldSelector.Body).Member.Name, value);
		}
		public ValueSetter<TEntity> WithEventFiring => new ValueSetter<TEntity>(_cache, _entity, true);
	}
}
