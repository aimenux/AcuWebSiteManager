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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Web.SessionState;
using System.Web;
using PX.Api;
using PX.Api.Services;
using PX.Common;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using PX.Data.PushNotifications;
using PX.Data.SQLTree;
using PX.SM;

namespace PX.Data
{
    /// <exclude/>
	internal sealed class KeysVerifyer
	{
		private HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		public void Check(Type table)
		{
			if (keys.Count > 0)
			{
				throw new PXException(ErrorMessages.AttemptOfWholeTableUpdate, table.Name);
			}
		}
		public KeysVerifyer(PXCache sender)
		{
			for (int i = 0; i < sender.Keys.Count; i++)
			{
				keys.Add(sender.Keys[i]);
			}
		}
		public void SetRestriction(string name, PXCommandPreparingEventArgs.FieldDescription descr)
		{
			if (descr != null && descr.IsRestriction && descr.DataValue != null && descr.Expr != null)
			{
				keys.Remove(name);
			}
		}
	}
    /// <summary>
    /// 	<para>Represents a cache that contains modified data records relating to a particular table along with the controller that allows you to perform basic operations
    /// on such records. A type parameter is set to a data access class (DAC) that represents this table.</para>
    /// </summary>
    /// <typeparam name="TNode">The DAC type of data records stored in the cache object.</typeparam>
    /// <remarks>
    ///   <para>Cache objects consist of two parts:</para>
    ///   <list type="bullet">
    ///     <item>Data records collection that were modified but have not been saved to the database, such
    ///     as <see cref="PXCache{TNode}.Updated">Updated</see>, <see cref="PXCache{TNode}.Inserted">Inserted</see>, <see cref="PXCache{TNode}.Deleted">Deleted</see>, and <see cref="PXCache{TNode}.Dirty">
    ///     Dirty</see>.</item>
    ///     <item>A controller that executes basic data-related operations through the use of the methods, such
    ///     as <see cref="O:PX.Data.PXCache{TNode}.Update">Update()</see>, <see cref="O:PX.Data.PXCache{TNode}.Insert">Insert()</see>, <see cref="O:PX.Data.PXCache{TNode}.Delete">Delete()</see>, <see cref="O:PX.Data.PXCache{TNode}.Persist">Persist()</see>,
    ///     and other methods.</item>
    ///   </list>
    ///   <para>During execution of these methods, the cache object raises events. The graph as well as attributes can subscribe to these events to implement a business
    /// logic. Each method is applied to a previously unchanged data record result in placing of the data record into the cache.</para>
    ///   <para>The system creates and destroys PXCache instances (caches) on each request. If the user or the code modifies a data record, it is placed into the cache.
    /// When request execution is completed, the system serializes the modified records from the caches to the session. At run time, the cache may also include the
    /// unchanged data records retrieved during request execution. These data records are discarded once the request is served.</para>
    ///   <para>On the next round trip, the modified data records are loaded from the session to the caches. The cache merges the data retrieved from the database with the
    /// modified data, and the application accesses the data as if the entire data set has been preserved from the time of previous request.</para>
    ///   <para>The cache maintains the modified data until the changes are discarded or saved to the database.</para>
    ///   <para>The cache is the issuer of all data-related events, which can be handled by the graph and attributes.</para>
    ///   <para></para>
    /// </remarks>
    /// <example>
    /// 	<code title="Example" description="An instance of the PXCache&lt;&gt; type. The cache object that initiated the event is passed to every event handler as the first argument." lang="CS">
    /// protected virtual void Vendor_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
    /// {
    ///     Vendor row = e.Row as Vendor;
    ///     ...
    /// }</code>
    /// </example>
	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	//[DebuggerDisplay("{ToString()}")]
    [DebuggerTypeProxy(typeof(PXCache<>.PXCacheDebugView))]
	public partial class PXCache<TNode> : PXCache
		where TNode : class, IBqlTable, new()
	{
		#region Events
        /// <exclude/>
		private delegate object memberwiseCloneDelegate(TNode item);
		protected delegate PXCacheExtension[] memberwiseCloneExtensionsDelegate(TNode item, PXCacheExtension[] extensions);
		private static memberwiseCloneDelegate memberwiseClone;
		private memberwiseCloneExtensionsDelegate _CloneExtensions;
		internal override object _Clone(object item)
		{
			if (item is TNode)
			{
				if (memberwiseClone != null && _CreateExtensions == null)
				{
					return memberwiseClone((TNode)item);
				}
				return CreateCopy((TNode)item);
			}
			return null;
		}

		private void cloneRowSubscribers(Dictionary<object, object> clones, EventsRowAttr list)
		{
			if (_EventsRowAttr.RowSelecting != null)
			{
				list.RowSelecting = new IPXRowSelectingSubscriber[_EventsRowAttr.RowSelecting.Length];
				for (int i = 0; i < _EventsRowAttr.RowSelecting.Length; i++)
				{
					list.RowSelecting[i] = (IPXRowSelectingSubscriber)clones[_EventsRowAttr.RowSelecting[i]];
				}
			}
			if (_EventsRowAttr.RowSelected != null)
			{
				list.RowSelected = new IPXRowSelectedSubscriber[_EventsRowAttr.RowSelected.Length];
				for (int i = 0; i < _EventsRowAttr.RowSelected.Length; i++)
				{
					list.RowSelected[i] = (IPXRowSelectedSubscriber)clones[_EventsRowAttr.RowSelected[i]];
				}
			}
			if (_EventsRowAttr.RowInserting != null)
			{
				list.RowInserting = new IPXRowInsertingSubscriber[_EventsRowAttr.RowInserting.Length];
				for (int i = 0; i < _EventsRowAttr.RowInserting.Length; i++)
				{
					list.RowInserting[i] = (IPXRowInsertingSubscriber)clones[_EventsRowAttr.RowInserting[i]];
				}
			}
			if (_EventsRowAttr.RowInserted != null)
			{
				list.RowInserted = new IPXRowInsertedSubscriber[_EventsRowAttr.RowInserted.Length];
				for (int i = 0; i < _EventsRowAttr.RowInserted.Length; i++)
				{
					list.RowInserted[i] = (IPXRowInsertedSubscriber)clones[_EventsRowAttr.RowInserted[i]];
				}
			}
			if (_EventsRowAttr.RowUpdating != null)
			{
				list.RowUpdating = new IPXRowUpdatingSubscriber[_EventsRowAttr.RowUpdating.Length];
				for (int i = 0; i < _EventsRowAttr.RowUpdating.Length; i++)
				{
					list.RowUpdating[i] = (IPXRowUpdatingSubscriber)clones[_EventsRowAttr.RowUpdating[i]];
				}
			}
			if (_EventsRowAttr.RowUpdated != null)
			{
				list.RowUpdated = new IPXRowUpdatedSubscriber[_EventsRowAttr.RowUpdated.Length];
				for (int i = 0; i < _EventsRowAttr.RowUpdated.Length; i++)
				{
					list.RowUpdated[i] = (IPXRowUpdatedSubscriber)clones[_EventsRowAttr.RowUpdated[i]];
				}
			}
			if (_EventsRowAttr.RowDeleting != null)
			{
				list.RowDeleting = new IPXRowDeletingSubscriber[_EventsRowAttr.RowDeleting.Length];
				for (int i = 0; i < _EventsRowAttr.RowDeleting.Length; i++)
				{
					list.RowDeleting[i] = (IPXRowDeletingSubscriber)clones[_EventsRowAttr.RowDeleting[i]];
				}
			}
			if (_EventsRowAttr.RowDeleted != null)
			{
				list.RowDeleted = new IPXRowDeletedSubscriber[_EventsRowAttr.RowDeleted.Length];
				for (int i = 0; i < _EventsRowAttr.RowDeleted.Length; i++)
				{
					list.RowDeleted[i] = (IPXRowDeletedSubscriber)clones[_EventsRowAttr.RowDeleted[i]];
				}
			}
			if (_EventsRowAttr.RowPersisting != null)
			{
				list.RowPersisting = new IPXRowPersistingSubscriber[_EventsRowAttr.RowPersisting.Length];
				for (int i = 0; i < _EventsRowAttr.RowPersisting.Length; i++)
				{
					list.RowPersisting[i] = (IPXRowPersistingSubscriber)clones[_EventsRowAttr.RowPersisting[i]];
				}
			}
			if (_EventsRowAttr.RowPersisted != null)
			{
				list.RowPersisted = new IPXRowPersistedSubscriber[_EventsRowAttr.RowPersisted.Length];
				for (int i = 0; i < _EventsRowAttr.RowPersisted.Length; i++)
				{
					list.RowPersisted[i] = (IPXRowPersistedSubscriber)clones[_EventsRowAttr.RowPersisted[i]];
				}
			}
		}
		private Dictionary<string, Subscriber[]> cloneFieldSubscribers<Subscriber>(Dictionary<object, object> clones, Dictionary<string, Subscriber[]> list)
			where Subscriber : class
		{
			Dictionary<string, Subscriber[]> ret = new Dictionary<string, Subscriber[]>(list.Count);
			foreach (KeyValuePair<string, Subscriber[]> p in list)
			{
				List<Subscriber> copy = new List<Subscriber>(p.Value.Length);
				for (int i = 0; i < p.Value.Length; i++)
				{
					var subscriber = p.Value[i];
					//if (clones.ContainsKey(subscriber))
						copy.Add((Subscriber)clones[subscriber]);
				}
				ret[p.Key] = copy.ToArray();
			}
			return ret;
		}

		private void AddAggregatedAttributes(ref Dictionary<object, object> clones, PXEventSubscriberAttribute attr, PXEventSubscriberAttribute clone)
		{
			clones.Add(attr, clone);
			var aggregatedAttr = clone as PXAggregateAttribute;
			if (aggregatedAttr != null)
			{
				PXEventSubscriberAttribute[] oldaggr = ((PXAggregateAttribute)attr).GetAggregatedAttributes();
				PXEventSubscriberAttribute[] newaggr = ((PXAggregateAttribute)clone).GetAggregatedAttributes();
				for (int k = 0; k < oldaggr.Length; k++)
				{
					clones.Add(oldaggr[k], newaggr[k]);
				}
			}
		}

		/// <summary>Checks if the provided data record has any attributes
		/// attached to its fields.</summary>
		/// <param name="data">The data record.</param>
		public override bool HasAttributes(object data)
		{
			if (_ItemAttributes != null && data is TNode)
			{
				return _ItemAttributes.ContainsKey((TNode)data);
			}
			return false;
		}

        /// <summary>Returns the cache-level instances of attributes for the specified field.</summary>
        /// <param name="name">
        /// 	<para>A field name, the attributes of which were returned. The method will return attributes from the entire field collection if <tt>null</tt>.</para>
        /// </param>
        /// <remarks>The system maintains instances of attributes on three different
        /// levels. On its instantiation, a cache object copies appropriate
        /// attributes from the global level to the cache level and stores them in
        /// an internal collection. When an attribute needs to be modified for a
        /// particular data record, the cache creates item-level copies of all
        /// attributes and stores them associated with the data record.</remarks>
		public override List<PXEventSubscriberAttribute> GetAttributesReadonly(string name)
		{
			return GetAttributesReadonly(name, true);
		}

        /// <summary>Returns the cache-level instances of attributes for the specified field.</summary>
        /// <param name="name">The data record.</param>
        /// <param name="extractEmmbeddedAttr">The value that indicates whether
        /// the attributes embedded into an aggregate attribute are included into
        /// the list. If <tt>true</tt>, both the aggregate attribute and the
        /// attributes embedded into it are included in the list. Otherwise, only
        /// the aggregate attribute is included. An aggregate attribute is an
        /// attribute that derives from the <tt>PXAggregateAttribute</tt> class.
        /// This class allows combining multiple different attributes in a single
        /// one.</param>
        /// <remarks>Using this method, you can prevent expanding the aggregate
        /// attributes by setting the second parameter to <tt>false</tt>. Other
        /// overloads of this method always include both the aggregate attributes
        /// and the attributes that comprise such attributes.</remarks>
		public override List<PXEventSubscriberAttribute> GetAttributesReadonly(string name, bool extractEmmbeddedAttr)
		{
			List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			int idx;
			if (name == null)
			{
				foreach (PXEventSubscriberAttribute attr in _CacheAttributes)
				{
					ret.Add(attr);
					if (extractEmmbeddedAttr)
						extractEmbeded(ret, attr);
				}
			}
			else if (_FieldsMap.TryGetValue(name, out idx))
			{
				for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
				{
					ret.Add(_CacheAttributes[i]);
					if (extractEmmbeddedAttr)
						extractEmbeded(ret, _CacheAttributes[i]);
				}
			}
			if (_MeasuringUpdatability)
			{
				for (int i = 0; i < ret.Count; i++)
				{
					PXEventSubscriberAttribute attr = ret[i];
					if (attr is PXUIFieldAttribute)
					{
						ret[i] = new MeasuringUIFieldAttribute(this, attr.FieldName, attr.FieldOrdinal, attr.BqlTable);
					}
				}
			}
			return ret;
		}

        /// <summary>
        /// 	<para>Returns the cache-level instances of attributes for the specified field. If there are no instances to be found, the cache-level instances will be
        /// returned.</para>
        /// </summary>
        /// <param name="data">The data record.</param>
        /// <param name="name">
        /// 	<para>A field name, the attributes of which were returned. The method will return attributes from the entire field collection if <tt>null</tt>.</para>
        /// </param>
        /// <example>
        /// 	<code title="Example" description="The following code snippet gets the attributes and places them into a list collection." lang="CS">
        /// protected virtual void InventoryItem_ValMethod_FieldVerifying(
        ///     PXCache sender, PXFieldVerifyingEventArgs e)
        /// {
        ///     List&lt;PXEventSubscriberAttribute&gt; attrlist = 
        ///         sender.GetAttributesReadonly(e.Row, "ValMethod");
        ///     ...
        /// }</code>
        /// </example>
		public override IEnumerable<PXEventSubscriberAttribute> GetAttributesReadonly(object data, string name)
		{
			if (data == null || _ItemAttributes == null || !_ItemAttributes.ContainsKey((TNode)data))
			{
				return GetAttributesReadonly(name);
			}

            var ret = new DisposableAttributesList(this, data) { NameFilter = name };


			//foreach (var a in _CacheAttributesWithEmbedded)
			//{
			//	if (name != null && !a.FieldName.OrdinalEquals(name))
			//		continue;

			//	ret.SetDirtyState(a);
			//}

			//var buf = GetItemState(data);
			//var ret = new DisposableList { cache = this, Buffer = buf, Capacity = _CacheAttributesWithEmbedded.Length };


			//foreach (var a in _CacheAttributesWithEmbedded)
			//{
			//	if (name != null && !a.FieldName.OrdinalEquals(name))
			//		continue;

			//	SetDirtyState(buf, ret, a);
			//}

			return ret;


			//List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			//int idx;
			//if (name == null)
			//{
			//	//foreach (PXEventSubscriberAttribute attr in _ItemAttributes[(TNode)data])
			//	//{
			//	//	ret.Add(attr);
			//	//	extractEmbeded(ret, attr);
			//	//}
			//	foreach (PXEventSubscriberAttribute attr in _ItemAttributes[(TNode)data].GetItemAttributesWithEmb(this))
			//	{
			//		ret.Add(attr);
			//		//extractEmbeded(ret, attr);
			//	}
			//}
			//else if (_FieldsMap.TryGetValue(name, out idx))
			//{
			//	//for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
			//	//{
			//	//	ret.Add(_ItemAttributes[(TNode)data][i]);
			//	//	extractEmbeded(ret, _ItemAttributes[(TNode)data][i]);
			//	//}

			//	return _ItemAttributes[(TNode) data].GetItemAttributesWithEmb(this).Where(_ => _.FieldName.OrdinalEquals(name)).ToList();
			//	//for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
			//	//{
			//	//	ret.Add(_ItemAttributes[(TNode)data][i]);
			//	//	extractEmbeded(ret, _ItemAttributes[(TNode)data][i]);
			//	//}
			//}
			//return ret;
		}

        /// <summary>Returns item-level instances of attributes for the specified field. In case there are no instances for the provided data record(s), this method will create
        /// them simply by copying all cache-level attributes along with having them placed into the internal collection that contains specific data record attributes. To
        /// avoid cloning cache-level attributes, use the <see cref="PXCache{T}.GetAttributesReadonly(object,string)">GetAttributesReadonly(object, string)</see> method.</summary>
        /// <param name="data">The data record.</param>
        /// <param name="name">
        /// 	<para>A field name, the attributes of which are used. The method will return attributes from the entire field collection if <tt>null</tt>.</para>
        /// </param>
		public override IEnumerable<PXEventSubscriberAttribute> GetAttributes(object data, string name)
		{
			return GetAttributes(data, name, false);
		}

		public override IEnumerable<T> GetAttributesOfType<T>(object data, string name)
		{
			return GetAttributes(data, name, false, typeof(T)).OfType<T>();
		}

		protected internal virtual IEnumerable<PXEventSubscriberAttribute> GetAttributes(object data, string name, bool forceNotCached, Type typeFilter = null)
		{
			if (data == null)
			{
				return GetAttributes(name);
			}
			if (NeverCloneAttributes || Graph.UnattendedMode && Locate(data) == null && !forceNotCached && !object.ReferenceEquals(_Current, data))
			{
				return GetAttributesReadonly(data, name);
			}
            if (_ItemAttributes == null || !_ItemAttributes.ContainsKey((TNode)data))
			{
				if (_ItemAttributes == null)
				{
					InitItemAttributesCollection();
				}

				if (_PendingItems != null)
				{
                    _PendingItems.Add((TNode)data);
				}


				var state = new DirtyItemState
				{
					BoundItem = data,
					//DirtyAttributes = new PXEventSubscriberAttribute[_CacheAttributesWithEmbedded.Length]
				};
                _ItemAttributes.Add((TNode)data, state);

			}
			
            if (typeFilter == typeof(PXEventSubscriberAttribute))
				typeFilter = null;
            var ret = new DisposableAttributesList(this, data) { NameFilter = name, TypeFilter = typeFilter };

			return ret;

		}



		public override List<Type> GetExtensionTables()
		{
			return _ExtensionTables;
		}
		public static Type[] GetExtensionTypesStatic()
		{
			return _Initialize(false)._ExtensionTypes;
		}
		internal static IEnumerable<PXEventSubscriberAttribute> GetAttributesStatic()
		{
			List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in _Initialize(false)._FieldAttributes)
			{
				ret.Add(attr);
				extractEmbeded(ret, attr);
			}
			return ret;
		}

		private static void extractEmbeded(List<PXEventSubscriberAttribute> list, PXEventSubscriberAttribute attr)
		{
			var aggregatedAttr = attr as PXAggregateAttribute;
			if (aggregatedAttr != null)
			{
				list.AddRange(aggregatedAttr.GetAggregatedAttributes());
			}
		}
        /// <summary>
        /// 	<para>Returns the cach-level instances of attributes for the specified field along with the entire collection of item-level instances currently stored in
        /// cache.</para>
        /// </summary>
        /// <param name="name">
        /// 	<para>A field name, the attributes of which are used. The method will return attributes from the entire field collection if <tt>null</tt>.</para>
        /// </param>
		public override List<PXEventSubscriberAttribute> GetAttributes(string name)
		{
			List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			int idx = -1;
			if (name == null)
			{
				foreach (PXEventSubscriberAttribute attr in _CacheAttributes)
				{
					ret.Add(attr);
					extractEmbeded(ret, attr);
				}
			}
			else if (_FieldsMap.TryGetValue(name, out idx))
			{
				for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
				{
					ret.Add(_CacheAttributes[i]);
					extractEmbeded(ret, _CacheAttributes[i]);
				}
			}
			if (_ItemAttributes != null)
			{
				int[] indexes = null;
				int indexesLength = 0;
				if (name != null && _FieldsMap.TryGetValue(name, out idx) && this._AttributesByName.Contains(name))
				{
					indexes = this._AttributesByName[name].Select(_ => _.IndexInClonesArray).Where(index => (_UsableItemAttributes[index / 32] & (1 << (index % 32))) != 0).ToArray();
					indexesLength = indexes.Length;
					if (indexesLength == 0)
						return ret;
				}

				foreach (var dict in _ItemAttributes.Values)
				{
                    if (dict.DirtyAttributes == null)
						continue;

					if (name == null)
					{
						ret.AddRange(dict.DirtyAttributes.Where(_ => _ != null));
						//return dict.GetItemAttributesWithEmb(this).ToList();

						//foreach (PXEventSubscriberAttribute attr in dict.DirtyAttributes)
						//{
						//	ret.Add(attr);
						//	extractEmbeded(ret, attr);
						//}
					}
					else if (idx >= 0)
					{
						if (indexes != null)
						{
							for (int i = 0; i < indexesLength; i++)
							{
								//if (!name.Equals(a.FieldName, StringComparison.OrdinalIgnoreCase))
								//throw new InvalidOperationException("Field name not match");
								var result = dict.DirtyAttributes[indexes[i]];
								if (result != null)
								{
									//if (!result.FieldName.OrdinalEquals(name))
										//throw new InvalidOperationException("Field name not match");
									ret.Add(result);
								}
							}
						}
						else
					{
						ret.AddRange(dict.DirtyAttributes.Where(_ => _ != null && _.FieldName.OrdinalEquals(name)));
						}
						//return dict.GetItemAttributesWithEmb(this).Where(_ => _.FieldName.OrdinalEquals(name)).ToList();

						//for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
						//{
						//	ret.Add(dict[i]);
						//	extractEmbeded(ret, dict[i]);
						//}
					}
				}
			}
			return ret;
		}
		#endregion

		#region Field Manipulation

		private CreateExtensionsDelegate _CreateExtensions;
		private List<Type> _ExtensionTables;
		private Type[] _ExtensionTypes;


		private SetValueByOrdinalDelegate _SetValueByOrdinal;
		private TNode _LastAccessedNode;
		private PXCacheExtension[] _LastAccessedExtensions;
		/// <summary>Sets the value of the field in the provided data record
		/// without raising events. The field is specified by its index in the
		/// field map.</summary>
		/// <remarks>To set the value, raising the field-related events, use the <see
		/// cref="PXCache{T}.SetValueExt(object,string,object)">SetValueExt(object,
		/// string, object)</see> method.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="ordinal">The index of the field in the internally stored
		/// field map. To get the index of a specific field, use the <see
		/// cref="PXCache{T}.GetFieldOrdinal(string)">GetFieldOrdinal(string)</see>
		/// method.</param>
		/// <param name="value">The value to set to the field.</param>
		public override void SetValue(object data, int ordinal, object value)
		{
			TNode node;
			if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
			{
				SetValueByOrdinal(_LastAccessedNode, ordinal, value, _LastAccessedExtensions);
			}
			else if ((node = data as TNode) != null)
			{
				_LastAccessedNode = node;
				if (_Extensions == null)
				{
					SetValueByOrdinal(node, ordinal, value, null);
				}
				else
				{
					PXCacheExtension[] extensions;
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(node, out extensions))
						{
							_Extensions[node] = extensions = _CreateExtensions(node);
						}
					}
					_LastAccessedExtensions = extensions;
					SetValueByOrdinal(node, ordinal, value, extensions);
				}
			}
			else if (ordinal < _ClassFields.Count)
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					dict[_ClassFields[ordinal]] = value;
				}
			}
		}
		/// <summary>Sets the value of the field in the provided data record
		/// without raising events.</summary>
		/// <remarks>
        /// To set the value, raising the field-related events, use the
        /// <see cref="PXCache{T}.SetValueExt(object,string,object)"/> method.
        /// </remarks>
        /// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field that is set to the value.
        /// The parameter is case-insensitive.</param>
        /// <param name="value">The value to set to the field.</param>
        /// <remarks>To set the value, raising the field-related events, use the <see cref="PXCache{T}.SetValueExt(object,string,object)">SetValueExt(object, string, object)</see> method.</remarks>
        /// <example>
        /// 	<code title="" description="" lang="CS">
        /// public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        /// {
        ///     bool freeze = ((bool?)sender.GetValue(e.Row, sender.GetField(freezeDisc))) == true;
        ///     
        ///     if (!freeze &amp;&amp; !sender.Graph.IsImport)
        ///     {
        ///         IDiscountable row = (IDiscountable)e.Row;
        ///         if (row.CuryDiscAmt != null &amp;&amp; row.CuryDiscAmt != 0 &amp;&amp; row.CuryExtPrice != 0)
        ///         {
        ///             row.DiscPct = 100 * row.CuryDiscAmt / row.CuryExtPrice;
        ///             sender.SetValue(row, sender.GetField(curyTranAmt), row.CuryExtPrice - row.CuryDiscAmt);
        ///             sender.SetValue(e.Row, this.FieldName, true);
        ///         }
        ///         ...
        ///     }
        /// }</code>
        /// </example>
		public override void SetValue(object data, string fieldName, object value)
		{
			TNode node;
			if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
			{
				if (_FieldsMap.ContainsKey(fieldName))
				{
					SetValueByOrdinal(_LastAccessedNode, _FieldsMap[fieldName], value, _LastAccessedExtensions);
				}
			}
			else if ((node = data as TNode) != null)
			{
				if (_FieldsMap.ContainsKey(fieldName))
				{
					_LastAccessedNode = node;
					if (_Extensions == null)
					{
						SetValueByOrdinal(node, _FieldsMap[fieldName], value, null);
					}
					else
					{
						PXCacheExtension[] extensions;
						lock (((ICollection)_Extensions).SyncRoot)
						{
							if (!_Extensions.TryGetValue(node, out extensions))
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						_LastAccessedExtensions = extensions;
						SetValueByOrdinal(node, _FieldsMap[fieldName], value, extensions);
					}
				}
			}
			else
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					if (!dict.Contains(fieldName))
					{
						string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
						if (key != null)
						{
							fieldName = key;
						}
					}
					dict[fieldName] = value;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual object GetValueByOrdinal(TNode data, int ordinal, PXCacheExtension[] extensions)
		{
			return _GetValueByOrdinal(data, ordinal, extensions);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void SetValueByOrdinal(TNode data, int ordinal, object value, PXCacheExtension[] extensions)
		{
			_SetValueByOrdinal(data, ordinal, value, extensions);
		}

		internal override object GetCopy(object data)
		{
			object newitem;
			return GetCopy(data, out newitem);
		}

		protected virtual object GetCopy(object data, out object pending)
		{
			object copy = null;
			pending = null;
			if (_PendingValues != null && data is TNode && _PendingValues.TryGetValue((TNode)data, out pending))
			{
				foreach (TNode key in _PendingValues.Keys)
				{
					//try to find copy in _PendingValues[copy]
					if (!object.ReferenceEquals(key, data) && object.ReferenceEquals(_PendingValues[key], pending))
					{
						copy = key;
						break;
					}
				}
			}
			return copy;
		}

		/// <summary>Sets the default value to the field in the provided data
		/// record.</summary>
		/// <remarks>The method raises <tt>FieldDefaulting</tt>,
		/// <tt>FieldUpdating</tt>, <tt>FieldVerifying</tt>, and
		/// <tt>FieldUpdated</tt>.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field to set.</param>
		public override void SetDefaultExt(object data, string fieldName, object value = null)
		{
			if (Fields.Contains(fieldName) == false) return;
			bool externalCall = _PendingValues != null && data is TNode && _PendingValues.ContainsKey((TNode)data) && _PendingValues[(TNode)data] is IDictionary;
			bool eventExternalCall = externalCall;
			ExternalCallMarker marker = value as ExternalCallMarker;
			if (marker != null)
			{
				eventExternalCall = !marker.IsInternalCall;
				value = ExternalCallMarker.Unwrap(value);
			}
			
			object pendingval = GetValuePending(data, fieldName);
			bool valueIsSet = !(pendingval == null || ReferenceEquals(pendingval, NotSetValue));
			if (valueIsSet)
			{
				object newitem;
				object copy = GetCopy(data, out newitem);
				bool valueHasNotChanged = externalCall || copy != null && OnFieldUpdating(fieldName, newitem, ref pendingval) && Equals(GetValue(copy, fieldName), pendingval);
				if (valueHasNotChanged == false)
					return;
			}

				try
				{
					if (data is PXResult)
					{
						data = ((PXResult)data)[0];
					}
					if (value != null || OnFieldDefaulting(fieldName, data, out value))
					{
						OnFieldUpdating(fieldName, data, ref value);
					}
					OnFieldVerifying(fieldName, data, ref value, eventExternalCall);
					object oldValue = null;
					TNode node;
					if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
					{
						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}
						oldValue = GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
						SetValueByOrdinal(_LastAccessedNode, ordinal, value, _LastAccessedExtensions);
					}
					else if ((node = data as TNode) != null)
					{
						_LastAccessedNode = node;
						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}
						if (_Extensions == null)
						{
							oldValue = GetValueByOrdinal(node, ordinal, null);
							SetValueByOrdinal(node, ordinal, value, null);
						}
						else
						{
							PXCacheExtension[] extensions;
							lock (((ICollection)_Extensions).SyncRoot)
							{
								if (!_Extensions.TryGetValue(node, out extensions))
								{
									_Extensions[node] = extensions = _CreateExtensions(node);
								}
							}
							_LastAccessedExtensions = extensions;
							oldValue = GetValueByOrdinal(node, ordinal, extensions);
							SetValueByOrdinal(node, ordinal, value, extensions);
						}
					}
					else
					{
						IDictionary dict = data as IDictionary;
						if (dict != null)
						{
							if (dict.Contains(fieldName))
							{
								oldValue = dict[fieldName];
							}
							else
							{
								string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
								if (key != null)
								{
									oldValue = dict[key];
									fieldName = key;
								}
							}
							dict[fieldName] = value;
						}
					}
					OnFieldUpdated(fieldName, data, oldValue, eventExternalCall);
				}
				catch (PXSetPropertyException e)
				{
					if (OnExceptionHandling(fieldName, data, value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
				}
			}

        /// <summary>
        /// Sets the value of the field in the provided data record.
        /// </summary>
		/// <remarks>
        /// The method raises the <tt>FieldUpdating</tt>,
		/// <tt>FieldVerifying</tt>, and <tt>FieldUpdated</tt> events. To set the
		/// value to the field without raising events, use the
        /// <see cref="PXCache{T}.SetValue(object, string, object)"/> method.
        /// </remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field that is set to the value.
        /// The parameter is case-insensitive.</param>
		/// <param name="value">The value to set to the field.</param>
		public override void SetValueExt(object data, string fieldName, object value)
		{
			if (Fields.Contains(fieldName))
			{
				try
				{
					if (data is PXResult)
					{
						data = ((PXResult)data)[0];
					}

					bool externalCall;
					ExternalCallMarker marker = value as ExternalCallMarker;
					if (marker != null)
					{
						externalCall = !marker.IsInternalCall;
						value = ExternalCallMarker.Unwrap(value);
					}
					else
					{
						externalCall = _PendingValues != null && data is TNode && _PendingValues.ContainsKey((TNode)data) && _PendingValues[(TNode)data] is IDictionary;
					}

					OnFieldUpdating(fieldName, data, ref value);
					OnFieldVerifying(fieldName, data, ref value, externalCall);
					object oldValue = null;
					TNode node;
					if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
					{
						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}
						oldValue = GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
						SetValueByOrdinal(_LastAccessedNode, ordinal, value, _LastAccessedExtensions);
					}
					else if ((node = data as TNode) != null)
					{
						_LastAccessedNode = node;

						if (_Extensions != null)
						{
							PXCacheExtension[] extensions;
							lock (((ICollection)_Extensions).SyncRoot)
							{
								if (!_Extensions.TryGetValue(node, out extensions))
								{
									_Extensions[node] = extensions = _CreateExtensions(node);
								}
							}
							_LastAccessedExtensions = extensions;
						}
						else
						{
							_LastAccessedExtensions = null;
						}

						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}

						oldValue = GetValueByOrdinal(node, ordinal, _LastAccessedExtensions);
						SetValueByOrdinal(node, ordinal, value, _LastAccessedExtensions);
					}
					else
					{
						IDictionary dict = data as IDictionary;
						if (dict != null)
						{
							if (dict.Contains(fieldName))
							{
								oldValue = dict[fieldName];
							}
							else
							{
								string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
								if (key != null)
								{
									oldValue = dict[key];
									fieldName = key;
								}
							}
							dict[fieldName] = value;
						}
					}
					OnFieldUpdated(fieldName, data, oldValue, externalCall);
				}
				catch (PXSetPropertyException e)
				{
					if (OnExceptionHandling(fieldName, data, value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
				}
			}
		}

		private GetValueByOrdinalDelegate _GetValueByOrdinal;
        private object NormalizeData(object data)
        {
            return (data is PXResult) ? PXResult.Unwrap<TNode>(data) : data;
        }
        /// <summary>Returns the value of the specified field in the given data
        /// record without raising any events. The field is specified by its
        /// index—see the <see cref="PXCache{T}.GetFieldOrdinal(string)">GetFieldOrdinal(string)</see>
        /// method.</summary>
        /// <param name="data">The data record.</param>
        /// <param name="ordinal">A field index, value of which is returned.</param>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// if (tran == null || tran.InventoryID == null || !string.IsNullOrEmpty(tran.PONbr))
        /// {
        ///     e.NewValue = sender.GetValue&lt;APTran.curyUnitCost&gt;(e.Row);
        ///     e.Cancel = e.NewValue != null;
        ///     return;
        /// }</code>
        /// </example>
		public override object GetValue(object data, int ordinal)
		{
            data = NormalizeData(data);
            TNode node;
			if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
			{
				return GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
			}
			else if ((node = data as TNode) != null)
			{
				_LastAccessedNode = node;
				if (_Extensions == null)
				{
					return GetValueByOrdinal(node, ordinal, null);
				}
				else
				{
					PXCacheExtension[] extensions;
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(node, out extensions))
						{
							_Extensions[node] = extensions = _CreateExtensions(node);
						}
					}
					_LastAccessedExtensions = extensions;
					return GetValueByOrdinal(node, ordinal, extensions);
				}
			}
			else if (ordinal < _ClassFields.Count)
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					string fieldName = _ClassFields[ordinal];
					if (dict.Contains(fieldName))
					{
						return dict[fieldName];
					}
					else
					{
						string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
						if (key != null)
						{
							return dict[key];
						}
					}
				}
			}
			return null;
		}

        /// <summary>Returns the value of the specified field in the given data
        /// record without raising any events.</summary>
        /// <param name="data">The data record.</param>
        /// <param name="fieldName">The name of the field whose value is
        /// returned.</param>
        /// <remarks>
        /// 	<para>To get a known DAC type data record field, you can use DAC properties. If a type of a data record is unknown (for example, when it is available as
        /// <tt>object</tt>), you can use the <tt>GetValue()</tt> methods to get a value of a field. These methods can also be used to get values of fields defined in
        /// extensions (another way is to get the extension data record through the <see cref="PXCache{T}.GetExtension{E}(T)">GetExtension&lt;&gt;()</see> method).</para>
        /// 	<para>The <see cref="PXCache{T}.GetValuExt(object,string)">GetValueExt()</see> methods are used to get the value or the field state object and raise events.</para>
        /// </remarks>
        /// <example>
        /// 	<code title="Example" description="The code snippet below iterates throughout the entire collection of fields in a specific DAC (including those fields defined in extensions) and checks whether the value is null." lang="CS">
        /// foreach (string field in sender.Fields)
        /// {
        ///     if (sender.GetValue(row, field) == null)
        ///         ...
        /// }</code>
        /// </example>
		public override object GetValue(object data, string fieldName)
		{
			int ordinal;
			if (_FieldsMap.TryGetValue(fieldName, out ordinal))
			{
				TNode node;
				if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
				{
					return GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
				}
				if ((node = data as TNode) != null)
				{
					_LastAccessedNode = node;
					if (_Extensions == null)
					{
						return GetValueByOrdinal(node, ordinal, null);
					}
					else
					{
						PXCacheExtension[] extensions;
						lock (((ICollection)_Extensions).SyncRoot)
						{
							if (!_Extensions.TryGetValue(node, out extensions))
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						_LastAccessedExtensions = extensions;
						return GetValueByOrdinal(node, ordinal, extensions);
					}
				}
				else
				{
					IDictionary dict = data as IDictionary;
					if (dict != null)
					{
						if (dict.Contains(fieldName))
						{
							return dict[fieldName];
						}
						else
						{
							string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
							if (key != null)
							{
								return dict[key];
							}
						}
					}
				}
			}
			else if (fieldName != null)
			{
				int pos = fieldName.IndexOf('.');
				if (pos > 0 && pos < fieldName.Length - 1)
				{
					fieldName = fieldName.Substring(pos + 1);
					if (_FieldsMap.ContainsKey(fieldName))
					{
						TNode node;
						if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
						{
							return GetValueByOrdinal(_LastAccessedNode, _FieldsMap[fieldName], _LastAccessedExtensions);
						}
						if ((node = data as TNode) != null)
						{
							_LastAccessedNode = node;
							if (_Extensions == null)
							{
								return GetValueByOrdinal(node, _FieldsMap[fieldName], null);
							}
							else
							{
								PXCacheExtension[] extensions;
								lock (((ICollection)_Extensions).SyncRoot)
								{
									if (!_Extensions.TryGetValue(node, out extensions))
									{
										_Extensions[node] = extensions = _CreateExtensions(node);
									}
								}
								_LastAccessedExtensions = extensions;
								return GetValueByOrdinal(node, _FieldsMap[fieldName], extensions);
							}
						}
						else
						{
							IDictionary dict = data as IDictionary;
							if (dict != null)
							{
								if (dict.Contains(fieldName))
								{
									return dict[fieldName];
								}
								else
								{
									string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
									if (key != null)
									{
										return dict[key];
									}
								}
							}
						}
					}
				}
			}
			return null;
		}
		/// <exclude/>
		public override object GetValueInt(object data, string fieldName)
		{
			return GetValueInt(data, fieldName, _AlteredFields != null && _AlteredFields.Contains(fieldName.ToLower()), false);
		}
		/// <exclude/>
		internal override object GetStateInt(object data, string fieldName)
		{
			return GetValueInt(data, fieldName, true, false);
		}
        /// <summary>Returns external UI field representation. The <tt>PXFieldState</tt> object is returned if the field is in the <tt>AlteredFields</tt> collection.</summary>
        /// <param name="data">The data record.</param>
        /// <param name="fieldName">The name of the field whose value or
        /// <tt>PXFieldState</tt> object is returned.</param>
        /// <remarks>The method raises the <tt>FieldSelecting</tt> event.</remarks>
        /// <returns>
        /// 	<tt>PXFieldState</tt> object field value.</returns>
        /// <example>
        /// 	<code title="Example" description="" lang="CS">
        /// string name = _MapErrorTo.Name;
        /// name = char.ToUpper(name[0]) + name.Substring(1);
        /// object val = sender.GetValueExt(e.Row, name);
        ///     if (val is PXFieldState)
        ///     {
        ///         val = ((PXFieldState)val).Value;
        ///     }</code>
        /// 	<code title="Example2" description="" groupname="Example" lang="CS">
        /// protected virtual void APPayment_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        /// {
        ///     APPayment payment = (APPayment)e.Row;
        ///     if (payment == null || e.NewValue == null) return;
        ///  
        ///     CashAccount cashAccount = PXSelect&lt;CashAccount, Where&lt;CashAccount.cashAccountID, Equal&lt;Required&lt;CashAccount.cashAccountID&gt;&gt;&gt;&gt;.SelectSingleBound(this, null, e.NewValue);
        ///     if (cashAccount != null)
        ///     {
        ///         foreach (PXResult&lt;APAdjust, APInvoice, Standalone.APRegisterAlias&gt; res in Adjustments_Invoices.Select())
        ///         {
        ///             APAdjust adj = res;
        ///             APInvoice invoice = res;
        ///  
        ///             PXCache&lt;APRegister&gt;.RestoreCopy(invoice, (Standalone.APRegisterAlias)res);
        ///  
        ///             if(adj.AdjdDocType == APDocType.Prepayment
        ///                 &amp;&amp; (adj.AdjgDocType == APDocType.Check || adj.AdjgDocType == APDocType.VoidCheck)
        ///                 &amp;&amp; invoice.CuryID != cashAccount.CuryID)
        ///             {
        ///                 e.NewValue = sender.GetValueExt&lt;APPayment.cashAccountID&gt;(payment);
        ///                 throw new PXSetPropertyException(Messages.CashCuryNotPPCury);
        ///             }
        ///         }
        ///     }
        /// }</code>
        /// </example>
		public override object GetValueExt(object data, string fieldName)
		{
			return GetValueInt(data, fieldName, _AlteredFields != null && _AlteredFields.Contains(fieldName.ToLower()), true);
		}
		/// <summary>Gets the <tt>PXFieldState</tt> object of the specified field
		/// in the given data record.</summary>
		/// <remarks>The method raises the <tt>FieldSelecting</tt> event.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field whose
		/// <tt>PXFieldState</tt> object is created.</param>
		/// <returns>The field state object.</returns>
		public override object GetStateExt(object data, string fieldName)
		{
			return GetValueInt(data, fieldName, true, true);
		}
		/// <summary>Returns the value of the specified field for the data record
		/// as it is stored in the database.</summary>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field whose original value is
		/// returned.</param>
		public override object GetValueOriginal(object data, string fieldName)
		{
			object original = GetOriginal(data);
			if (original != null)
			{
				return GetValue(original, fieldName);
			}
			return null;
		}
        ///<exclude/>
		public override object GetOriginal(object data)
	    {
	        BqlTablePair orig = null;
	        try
	        {
	            orig = GetOriginalObjectContext(data, true);
	        }
	        catch
	        {
	        }
	        if (orig != null && orig.Unchanged != null)
	        {
	            return orig.Unchanged;
	        }
	        return null;
	    }

	    protected Dictionary<TNode, object> _PendingValues;
        protected Dictionary<TNode, List<Exception>> _PendingExceptions;
		/// <summary>Returns the value of the field from the provided data record
		/// when the data record's update or insertion is in progress.</summary>
		/// <remarks>The method raises the <tt>FieldSelecting</tt> event.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The field name.</param>
		public override object GetValuePending(object data, string fieldName)
		{
			object vals;
			if (_PendingValues == null || !(data is TNode) || !_PendingValues.TryGetValue((TNode)data, out vals))
			{
				return null;
			}
			object ret = GetValueExt(vals, fieldName);
			if (ret is PXFieldState)
			{
				ret = ((PXFieldState)ret).Value;
			}
			else if (ret == null && vals is IDictionary)
			{
				var valsDict = (IDictionary)vals;

				if (string.Equals(PXImportAttribute.ImportFlag, fieldName, StringComparison.OrdinalIgnoreCase))
				{
					ret = valsDict[fieldName];
				}
				else
				{
					ret = valsDict.Contains(fieldName) ?
						valsDict[fieldName] :
						NotSetValue;
				}
			}
			return ret;
		}
		/// <summary>Sets the value of the field in the provided data record when
		/// the data record's update or insertion is in process and the field
		/// possibly hasn't been updated in the cache yet. The field is specified
		/// in the type parameter.</summary>
		/// <remarks>The method raises the <tt>FieldUpdating</tt> event.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field that is set to the
		/// value.</param>
		/// <param name="value">The value to set to the field.</param>
		public override void SetValuePending(object data, string fieldName, object value)
		{
			object vals;
			if (_PendingValues != null && data is TNode && _PendingValues.TryGetValue((TNode)data, out vals))
			{
				if (vals is TNode)
				{
					if (!object.ReferenceEquals(value, NotSetValue))
					{
						OnFieldUpdating(fieldName, data, ref value);
						SetValue(vals, fieldName, value);
					}
				}
				else
				{
                    IDictionary values = vals as IDictionary;
                    if (values == null || !values.Contains(PXImportAttribute.ImportFlag) || !object.ReferenceEquals(value, NotSetValue))
                    {
    					SetValue(vals, fieldName, value);
                    }
				}
			}
		}
		protected internal virtual object GetValueInt(object data, string fieldName, bool forceState, bool externalCall)
		{
            data = NormalizeData(data);
            object returnValue = null;
			if (data == null)
            {

                OnFieldSelecting(fieldName, null, ref returnValue, forceState, true);
				string displayName;
				if (returnValue == null && _InactiveFields != null && _InactiveFields.TryGetValue(fieldName, out displayName))
				{
					return PXInactiveFieldState.CreateInstance(fieldName, displayName);
				}
                return returnValue;

            }

		    int ordinal;
			string dn;
			if (_FieldsMap.TryGetValue(fieldName, out ordinal))
			{
				if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
				{
					returnValue = GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
					OnFieldSelecting(fieldName, _LastAccessedNode, ref returnValue, forceState, externalCall);
					return returnValue;
				}
				if (data is IBqlTable)
				{
					TNode node = (TNode)data;
					_LastAccessedNode = node;
					PXCacheExtension[] extensions = null;
					if (_Extensions != null)
					{
						lock (((ICollection)_Extensions).SyncRoot)
						{
							if (!_Extensions.TryGetValue(node, out extensions))
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						_LastAccessedExtensions = extensions;
					}
					returnValue = GetValueByOrdinal(node, ordinal, extensions);
					OnFieldSelecting(fieldName, data as IBqlTable, ref returnValue, forceState, externalCall);
					return returnValue;
				}
				IDictionary dict = (IDictionary)data;
				if (dict.Contains(fieldName))
				{
					return dict[fieldName];
				}
				string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
				if (key != null)
				{
					return dict[key];
				}
				// OnFieldSelecting(fieldName, data, ref returnValue, forceState, true);
				return null;
			}
			else if (Fields.Contains(fieldName) && data is IBqlTable)
			{

				OnFieldSelecting(fieldName, data, ref returnValue, forceState, true);
				return returnValue;
			}
			else if (_InactiveFields != null)
			{
				if (_InactiveFields.TryGetValue(fieldName, out dn))
			{
				return PXInactiveFieldState.CreateInstance(fieldName, dn);
			}
				else if (fieldName.EndsWith("_Date") || fieldName.EndsWith("_Time")
					&& _InactiveFields.TryGetValue(fieldName.Substring(0, fieldName.Length - 5), out dn))
				{
					return PXInactiveFieldState.CreateInstance(fieldName, dn);
				}
			}
			return null;
		}

        /// <summary>
        /// Copy values from dictionary to item with event handling.<br/>
        /// for insert operation raise OnFieldDefaulting, OnFieldUpdating, OnFieldVerifying, OnFieldUpdated<br/>
        /// for update operation raise OnFieldUpdating, OnFieldVerifying, OnFieldUpdated<br/>
        /// for delete operation events raised for key fields, OnFieldUpdating, OnFieldUpdated
        /// returns key updated flag<br/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="copy"></param>
        /// <param name="values"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
		private bool FillWithValues(TNode item, TNode copy, IDictionary values, PXCacheOperation operation)
		{
			return FillWithValues(item, copy, values, operation, true);
		}
		private bool FillWithValues(TNode item, TNode copy, IDictionary values, PXCacheOperation operation, bool externalCall)
		{
			bool keyUpdated = false;
			bool skipFieldUpdated = copy == null && operation == PXCacheOperation.Update;
			if (operation == PXCacheOperation.Insert && copy == null)
			{
				copy = new TNode();
			}
			if (_PendingValues == null)
			{
				_PendingValues = new Dictionary<TNode, object>();
			}
			_PendingValues[item] = values;
			PXCacheExtension[] copyextensions = null;
			PXCacheExtension[] itemextensions = null;
			if (_Extensions != null)
			{
				lock (((ICollection)_Extensions).SyncRoot)
				{
					if (!_Extensions.TryGetValue(item, out itemextensions))
					{
						_Extensions[item] = itemextensions = _CreateExtensions(item);
					}
				}
			}
			if (copy != null)
			{
				_PendingValues[copy] = values;
				if (_Extensions != null)
				{
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(copy, out copyextensions))
						{
							_Extensions[copy] = copyextensions = _CreateExtensions(copy);
						}
					}
				}
			}
			else
			{
				copyextensions = null;
			}
			bool xlsx = Graph.IsContractBasedAPI && operation == PXCacheOperation.Insert || values.Contains(PXImportAttribute.ImportFlag) && !Graph.IsImport && !Graph.IsExport;
			string descr = null;
			try
			{
				for (int i = 0; i < Fields.Count; i++)
				{
					descr = Fields[i];
					int ordinal;
					bool inClass = _FieldsMap.TryGetValue(descr, out ordinal);
                    bool isUDField = IsKvExtAttribute(descr);
					bool contains = values.Contains(descr);
					object newvalue = null;
					if (contains)
					{
						newvalue = values[descr];
						if (newvalue is PXFieldState)
						{
							newvalue = ((PXFieldState)newvalue).Value;
						}
					}
					if ((inClass || isUDField) && (operation == PXCacheOperation.Insert /*|| contains && newvalue == null*/))
					{
						object value;
						if (operation != PXCacheOperation.Insert)
						{
							SetValueByOrdinal(item, ordinal, null, itemextensions);
						}
						if (OnFieldDefaulting(descr, item, out value))
						{
							OnFieldUpdating(descr, item, ref value);
						}
						if (GetValueByOrdinal(item, ordinal, itemextensions) == null)
						{
							SetValueByOrdinal(item, ordinal, value, itemextensions);
						}
						if (copy != null)
						{
							object copyValue;
							if (OnFieldDefaulting(descr, copy, out copyValue))
							{
								OnFieldUpdating(descr, copy, ref copyValue);
							}
							SetValueByOrdinal(copy, ordinal, copyValue, copyextensions);
						}
					}
					if (contains && (newvalue != null || operation != PXCacheOperation.Insert) && !object.ReferenceEquals(newvalue, NotSetValue))
					{
						try
						{
							bool isKeyAndUpdated = false;
							object copyValue = null;
							if (inClass && copy != null)
							{
								copyValue = GetValueByOrdinal(copy, ordinal, copyextensions);
								if (_Keys.Contains(descr))
								{
									//if (copyValue != null && operation != PXCacheOperation.Insert)
									//{
									//    continue;
									//}
									if (newvalue != null)
									{
										isKeyAndUpdated = true;
									}
								}
							}
							Exception ex = null;
							bool skipSetValue = false;
							try
							{
								OnFieldUpdating(descr, item, ref newvalue);
							}
							catch (Exception outer)
							{
								skipSetValue = true;
								if (copy != null)
								{
									ex = outer;
									try
									{
										OnFieldUpdating(descr, copy, ref newvalue);
									}
									catch (Exception)
									{
										throw outer;
									}
								}
							}
							if (copy != null && Object.Equals(newvalue, copyValue) && (!xlsx || Object.Equals(newvalue, GetValueByOrdinal(item, ordinal, itemextensions))))
							{
								continue;
							}
							if (isKeyAndUpdated)
							{
								keyUpdated = true;
								}
							if (ex != null)
							{
								throw ex;
							}
							if (operation != PXCacheOperation.Delete)
							{
								OnFieldVerifying(descr, item, ref newvalue, externalCall);
							}
							if (!inClass || copy != null && Object.Equals(newvalue, copyValue) && (!xlsx || Object.Equals(newvalue, GetValueByOrdinal(item, ordinal, itemextensions))))
							{
								continue;
							}
							if (!skipSetValue)
							{
								object oldValue = GetValueByOrdinal(item, ordinal, itemextensions);
								SetValueByOrdinal(item, ordinal, newvalue, itemextensions);
								if (Graph.IsContractBasedAPI)
								{
									values[descr + _OriginalValue] = oldValue;
								}
								if (!skipFieldUpdated)
								{
									OnFieldUpdated(descr, item, oldValue, externalCall);
								}
							}
						}
						catch (PXSetPropertyException e)
						{
							if (operation == PXCacheOperation.Insert
								&& _Keys.Contains(descr)
								|| OnExceptionHandling(descr, item, newvalue, e))
							{
								throw;
							}
							PXTrace.WriteWarning(e);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (String.IsNullOrEmpty(descr))
				{
					throw;
				}
				if (ex is PXSetPropertyException)
				{
					string dispname = PXUIFieldAttribute.GetDisplayName(this, descr);
					string errortext = ex.Message;

					if (dispname != null && descr != dispname)
					{
						int fid = errortext.IndexOf(descr, StringComparison.OrdinalIgnoreCase);
						if (fid >= 0)
						{
							errortext = errortext.Remove(fid, descr.Length).Insert(fid, dispname);
						}
					}
					else
					{
						dispname = descr;
					}

					if (ex is PXSetupNotEnteredException)
					{
						throw;
					}

					throw new PXFieldProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, errortext);
				}
				if (ex is PXDialogRequiredException)
				{
					throw;
				}
				PXTrace.WriteWarning(ex);
				throw new PXException(ex, ErrorMessages.ErrorFieldProcessing, descr, ex.Message);
			}
			finally
			{
				_PendingValues.Remove(item);
				if (copy != null)
				{
					_PendingValues.Remove(copy);
				}
			}

			return keyUpdated;
		}

        /// <summary>
        /// Create new node, then assigns all default values, then copy non empty fields from item.<br/>
        /// If no exceptions, replace item with copy.
        /// </summary>
        /// <param name="item"></param>
		private void FillWithValues(TNode copy, ref TNode item)
		{
			if (_PendingValues == null)
			{
				_PendingValues = new Dictionary<TNode, object>();
			}
			PXCacheExtension[] itemextensions = null;
			PXCacheExtension[] copyextensions = null;
			if (_Extensions != null)
			{
				lock (((ICollection)_Extensions).SyncRoot)
				{
					if (!_Extensions.TryGetValue(item, out itemextensions))
					{
						_Extensions[item] = itemextensions = _CreateExtensions(item);
					}
					_Extensions[copy] = copyextensions = _CreateExtensions(copy);
				}
			}
			_PendingValues[copy] = item;
			try
			{
				foreach (string descr in _ClassFields)
				{
					object itemValue = null;
					try
					{
						int ordinal = _FieldsMap[descr];
						object defValue;
						if (OnFieldDefaulting(descr, copy, out defValue))
						{
							OnFieldUpdating(descr, copy, ref defValue);
						}
						object copyValue = GetValueByOrdinal(copy, ordinal, copyextensions);
						if (copyValue == null && defValue != null)
						{
							SetValueByOrdinal(copy, ordinal, defValue, copyextensions);
							copyValue = defValue;
						}
						itemValue = GetValueByOrdinal(item, ordinal, itemextensions);
						if (itemValue != null)
						{
							OnFieldVerifying(descr, copy, ref itemValue, false);
							SetValueByOrdinal(copy, ordinal, itemValue, copyextensions);
							OnFieldUpdated(descr, copy, copyValue, false);
						}
					}
					catch (PXSetupNotEnteredException)
					{
						throw;
					}
					catch (PXSetPropertyException ex)
					{
						if (ForceExceptionHandling && !OnExceptionHandling(descr, copy, itemValue, ex))
						{
							continue;
						}

						string dispname = PXUIFieldAttribute.GetDisplayName(this, descr);
						string errortext = ex.Message;

						if (dispname != null && descr != dispname)
						{
							int fid = errortext.IndexOf(descr, StringComparison.OrdinalIgnoreCase);
							if (fid >= 0)
							{
								errortext = errortext.Remove(fid, descr.Length).Insert(fid, dispname);
							}
						}
						else
						{
							dispname = descr;
						} 

						if (itemValue == null)
						{
							ex = new PXFieldProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, errortext);
						}
						else
						{
							object fs = GetStateExt(null, descr);

							if (itemValue is string && fs is PXStringState && !string.IsNullOrEmpty(((PXStringState)fs).InputMask))
							{
								itemValue = PX.Common.Mask.Format(((PXStringState)fs).InputMask, (string)itemValue);
							}

							ex = new PXFieldValueProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, itemValue, errortext);
						}
						if (_Keys.Contains(descr))
						{
							throw ex;
						}
                        List<Exception> pending;
                        if (!_PendingExceptions.TryGetValue(copy, out pending))
                        {
                            _PendingExceptions[copy] = pending = new List<Exception>();
                        }
                        pending.Add(ex);
					}
					catch (PXDialogRequiredException)
					{
						throw;
					}
					catch (Exception ex)
					{
						PXTrace.WriteWarning(ex);
						throw new PXException(ex, ErrorMessages.ErrorFieldProcessing, descr, ex.Message);
					}
				}
				BqlTablePair itemorig;
				BqlTablePair copyorig;
				lock (((ICollection)_Originals).SyncRoot)
				{
					if (_Originals.TryGetValue(item, out itemorig) && itemorig.Slots != null && itemorig.Slots.Count > 0)
					{
						if (!_Originals.TryGetValue(copy, out copyorig))
						{
							_Originals[copy] = copyorig = new BqlTablePair();
						}
						copyorig.Slots = new List<object>(itemorig.Slots.Count);
						copyorig.SlotsOriginal = new List<object>(itemorig.Slots.Count);
						for (int l = 0; l < itemorig.Slots.Count; l++)
						{
							if (itemorig.Slots[l] != null && l < _SlotDelegates.Count)
							{
								copyorig.Slots.Add(((Func<object, object>)_SlotDelegates[l].Item3)(itemorig.Slots[l]));
							}
							else
							{
								copyorig.Slots.Add(null);
							}
							if (itemorig.SlotsOriginal[l] != null && l < _SlotDelegates.Count)
							{
								copyorig.SlotsOriginal.Add(((Func<object, object>)_SlotDelegates[l].Item3)(itemorig.SlotsOriginal[l]));
							}
							else
							{
								copyorig.SlotsOriginal.Add(null);
							}
						}
					}
				}
				item = copy;
			}
			finally
			{
				_PendingValues.Remove(copy);
			}
		}

		private void FillWithValues(TNode item, TNode copy, TNode newitem)
		{
			if (_PendingValues == null)
			{
				_PendingValues = new Dictionary<TNode, object>();
			}
			_PendingValues[item] = newitem;
            if (copy != null)
            {
                _PendingValues[copy] = newitem;
            }
			PXCacheExtension[] itemextensions = null;
			PXCacheExtension[] newitemextensions = null;
			PXCacheExtension[] copyextensions = null;
			if (_Extensions != null)
			{
				lock (((ICollection)_Extensions).SyncRoot)
				{
					if (!_Extensions.TryGetValue(item, out itemextensions))
					{
						_Extensions[item] = itemextensions = _CreateExtensions(item);
					}
					if (!_Extensions.TryGetValue(newitem, out newitemextensions))
					{
						_Extensions[newitem] = newitemextensions = _CreateExtensions(newitem);
					}
					if (!_Extensions.TryGetValue(copy, out copyextensions))
					{
						_Extensions[copy] = copyextensions = _CreateExtensions(copy);
					}
				}
			}
			try
			{
				foreach (string descr in _ClassFields)
				{
					object newvalue = null;
					try
					{
						int ordinal = _FieldsMap[descr];
						object oldvalue = GetValueByOrdinal(copy, ordinal, copyextensions);
						newvalue = GetValueByOrdinal(newitem, ordinal, newitemextensions);
						if (!object.Equals(oldvalue, newvalue))
						{
							OnFieldVerifying(descr, item, ref newvalue, false);
							if (!object.Equals(oldvalue, newvalue))
							{
								SetValueByOrdinal(item, ordinal, newvalue, itemextensions);
								OnFieldUpdated(descr, item, oldvalue, false);
							}
						}
					}
					catch (Exception ex)
					{
						if (ex is PXSetPropertyException)
						{
							if (ForceExceptionHandling && !OnExceptionHandling(descr, item, newvalue, ex))
							{
								continue;
							}

							string dispname = PXUIFieldAttribute.GetDisplayName(this, descr);
							string errortext = ex.Message;

							if (dispname != null && descr != dispname)
							{
								int fid = errortext.IndexOf(descr, StringComparison.OrdinalIgnoreCase);
								if (fid >= 0)
								{
									errortext = errortext.Remove(fid, descr.Length).Insert(fid, dispname);
								}
							}
							else
							{
								dispname = descr;
							}

							if (newvalue == null)
							{
								throw new PXFieldProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, errortext);
							}
							else
							{
								object fs = GetStateExt(null, descr);

								if (newvalue is string && fs is PXStringState && !string.IsNullOrEmpty(((PXStringState)fs).InputMask))
								{
									newvalue = PX.Common.Mask.Format(((PXStringState)fs).InputMask, (string)newvalue);
								}

								throw new PXFieldValueProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, newvalue, errortext);
							}
						}
						if (ex is PXDialogRequiredException)
						{
							throw;
						}
						PXTrace.WriteWarning(ex);
						throw new PXException(ex, ErrorMessages.ErrorFieldProcessing, descr, ex.Message);
					}
				}
			}
			finally
			{
				_PendingValues.Remove(item);
				if (copy != null)
				{
					_PendingValues.Remove(copy);
				}
			}
		}

        /// <summary>Inserts a new row into the cache. Returns inserted row of type <tt>CacheItemType</tt> or null if row was not inserted. Raises events <tt>OnRowInserting</tt>,
        /// <tt>OnRowInserted</tt> and other field related events. Does not check the database for existing row. Flag <tt>AllowInsert</tt> does not affects this method.</summary>
        /// <param name="graph">A Graph object</param>
        /// <param name="item">IBqlTable of type <tt>CacheItemType</tt></param>
		public static TNode Insert(PXGraph graph, TNode item)
		{
			return (TNode)graph.Caches[typeof(TNode)].Insert(item);
		}

        /// <summary>Places a row into the cache with the Updated status. If the row does not exist in the cache, the method looks for it in the database. If the row does not
        /// exist in the database, the method inserts the row with the Inserted status.The method raises the OnRowUpdating, OnRowUpdated, and other events. The
        /// PXCache.AllowUpdate flag does not affect this method.</summary>
		/// <param name="graph">graph object</param>
        /// <param name="item">IBqlTable of type <tt>CacheItemType</tt></param>
		/// <returns></returns>
		public static TNode Update(PXGraph graph, TNode item)
		{
			return (TNode)graph.Caches[typeof(TNode)].Update(item);
		}

        /// <summary>
        /// 	<para>Places the data record into the cache with the <tt>Deleted</tt> or <tt>InsertedDeleted</tt> status. The method assigns the <tt>InsertedDeleted</tt> status to the data record if it has
        /// the Inserted status when the method is invoked. This method raises the <tt>RowDeleting</tt> and <tt>RowDeleted</tt> events. The <tt>AllowDelete</tt> property does not affect this
        /// method.</para>
        /// </summary>
		public static TNode Delete(PXGraph graph, TNode item)
		{
			return (TNode)graph.Caches[typeof(TNode)].Delete(item);
		}

        /// <summary>Creates a new node along with cloning field values.</summary>
        /// <param name="item"></param>
        /// <returns></returns>
		public static TNode CreateCopy(TNode item)
		{
			CacheStaticInfo result = _Initialize(false);
			return CreateCopy(item, result._CreateExtensions, result._CloneExtensions, result._ClassFields, result._GetValueByOrdinal, result._SetValueByOrdinal);
		}
		protected static TNode CreateCopy(TNode item, CreateExtensionsDelegate _CreateExtensions, memberwiseCloneExtensionsDelegate _CloneExtensions, List<string> _ClassFields, GetValueByOrdinalDelegate _GetValueByOrdinal, SetValueByOrdinalDelegate _SetValueByOrdinal)
		{
			if (item == null)
				return null;

			TNode copy = null;
			PXCacheExtension[] copyextensions = null;
			PXCacheExtension[] itemextensions = null;
			PXCacheExtensionCollection dict = null;

			if (_CreateExtensions != null)
			{
				dict = PXContext.GetSlot<PXCacheExtensionCollection>();
				if (dict == null)
				{
					dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}

				lock (((ICollection)dict).SyncRoot)
				{
					if (!dict.TryGetValue(item, out itemextensions))
					{
						dict[item] = itemextensions = _CreateExtensions(item);
					}
				}
			}

			if (memberwiseClone != null)
			{
				copy = (TNode)memberwiseClone(item);

				if (_CreateExtensions != null)
				{
					lock (((ICollection)dict).SyncRoot)
					{
						dict[copy] = copyextensions = _CloneExtensions(copy, itemextensions);
					}
				}
			}
			else
			{
				copy = new TNode();

				if (_CreateExtensions != null)
				{
					lock (((ICollection)dict).SyncRoot)
					{
						dict[copy] = copyextensions = _CreateExtensions(copy);
					}
					for (int i = 0; i < _ClassFields.Count; i++)
					{
						object val = _GetValueByOrdinal(item, i, itemextensions);
						_SetValueByOrdinal(copy, i, val, copyextensions);
					}

				}
				else
				{
					for (int i = 0; i < _ClassFields.Count; i++)
					{
						_SetValueByOrdinal(copy, i, _GetValueByOrdinal(item, i, null), null);
					}
				}
			}
			return copy;
		}

		/// <summary>Copies values of all fields from the second data record to
		/// the first data record.</summary>
		/// <remarks>The data records should have the DAC type of the cache, or the
		/// method does nothing.</remarks>
		/// <param name="item">The data record whose field values are
		/// updated.</param>
		/// <param name="copy">The data record whose field values are
		/// copied.</param>
		public override void RestoreCopy(object item, object copy)
		{
			if (item is TNode && copy is TNode)
			{
				RestoreCopy((TNode)item, (TNode)copy);
			}
		}

        /// <summary>
        ///   <para>Creates a clone of the provided data record by initializing a new data record with the field values get from the provided data record.</para>
        /// </summary>
        /// <param name="item">The data record to copy.</param>
        /// <example>
        /// 	<code title="Example" description="The following example creates a copy of the Address data record, modifies it, and then inserts modified data straight into the cache." lang="CS">
        /// Address addr = PXCache&lt;Address&gt;.CreateCopy(defAddress);
        /// addr.AddressID = null;
        /// addr.BAccountID = owner.BAccountID;
        /// addr = this.RemitAddress.Insert(addr);</code>
        /// </example>
		public override object CreateCopy(object item)
		{
			if (item is TNode)
			{
				return CreateCopy((TNode)item);
			}
			return null;
		}

		/// <summary>Converts the provided data record to the dictionary of field
		/// names and field values. Returns the resulting dictionary
		/// object.</summary>
		/// <remarks>The method raises the <tt>FieldSelecting</tt> event for each
		/// field.</remarks>
		/// <param name="data">The data record to convert to a dictionary.</param>
		public override Dictionary<string, object> ToDictionary(object data)
		{
			Dictionary<string, object> ret = new Dictionary<string, object>();
			object value;

			foreach (string fieldname in _ClassFields)
			{
				ret.Add(fieldname, (value = GetValueExt(data, fieldname)) is PXFieldState ? ((PXFieldState)value).Value : value);
			}
			return ret;
		}

		/// <summary>Returns the XML string representing the provided data
		/// record.</summary>
		/// <remarks>
		/// <para>The data record is represented in the XML by the
		/// <i>&lt;Row&gt;</i> element with the <i>type</i> attribute set to the
		/// DAC name. Each field is represented by the <i>&lt;Field&gt;</i>
		/// element with the <i>name</i> attribute holding the field name and the
		/// <i>value</i> attribute holding the field value.</para>
		/// </remarks>
		/// <param name="data">The data record to convert to XML.</param>
		public override string ToXml(object data)
		{
			TNode node = data as TNode;
			if (node != null)
			{
				PXCacheExtension[] extensions = null;
				if (_Extensions != null)
				{
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(node, out extensions))
						{
							_Extensions[node] = extensions = _CreateExtensions(node);
						}
					}
				}
				StringBuilder bld = new StringBuilder();
				using (System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(new System.IO.StringWriter(bld)))
				{
					xw.Formatting = System.Xml.Formatting.Indented;
					xw.Indentation = 2;
					xw.WriteStartElement("Row");
					xw.WriteAttributeString("type", typeof(TNode).FullName);
					for (int i = 0; i < _ClassFields.Count; i++)
					{
						object val = GetValueByOrdinal(node, _FieldsMap[_ClassFields[i]], extensions);
						if (val != null)
						{
							xw.WriteStartElement("Field");
							xw.WriteAttributeString("name", _ClassFields[i]);
							switch (Type.GetTypeCode(_FieldTypes[i]))
							{
								case TypeCode.String:
									xw.WriteAttributeString("value", (string)val);
									break;
								case TypeCode.Int16:
									xw.WriteAttributeString("value", ((short)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Int32:
									xw.WriteAttributeString("value", ((int)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Int64:
									xw.WriteAttributeString("value", ((long)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Boolean:
									xw.WriteAttributeString("value", ((bool)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Decimal:
									xw.WriteAttributeString("value", ((decimal)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Single:
									xw.WriteAttributeString("value", ((float)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Double:
									xw.WriteAttributeString("value", ((double)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.DateTime:
									xw.WriteAttributeString("value", ((DateTime)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Object:
									if (_FieldTypes[i] == typeof(Guid))
									{
										xw.WriteAttributeString("value", ((Guid)val).ToString(null, System.Globalization.CultureInfo.InvariantCulture));
									}
									else if (_FieldTypes[i] == typeof(byte[]))
									{
										xw.WriteAttributeString("value", Convert.ToBase64String((byte[])val));
									}
									break;
							}
							xw.WriteEndElement();
						}
					}
					xw.WriteEndElement();
				}
				return bld.ToString();
			}
			return null;
		}

        /// <summary>Initializes a data record with the provided XML string.</summary>
        /// <param name="xml">The XML string to parse.</param>
        /// <remarks>A data record is represented in the XML as the <i>&lt;Row&gt;</i> element with the <i>type</i> attribute that is set to the DAC name. Each field is
        /// represented as the <i>&lt;Field&gt;</i> element with the <i>name</i> attribute that holds the field name and the <i>value</i> attribute that holds the
        /// field value.</remarks>
		public override object FromXml(string xml)
		{
			if (xml != null)
			{
				using (System.IO.TextReader tr = new System.IO.StringReader(xml))
				using (System.Xml.XmlTextReader xr = new System.Xml.XmlTextReader(tr))
				{
					string type = null;
					if (xr.ReadToDescendant("Row"))
					{
						type = xr.GetAttribute("type");
					}
					if (type == typeof(TNode).FullName)
					{
						TNode node = new TNode();
						PXCacheExtension[] extensions = null;
						if (_Extensions != null)
						{
							lock (((ICollection)_Extensions).SyncRoot)
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						while (xr.Read())
						{
							if (xr.Name == "Field")
							{
								string name = xr.GetAttribute("name");
								if (!String.IsNullOrEmpty(name))
								{
									string value = xr.GetAttribute("value");
									int i;
									if (value != null && _FieldsMap.TryGetValue(name, out i))
									{
										int k = _ReverseMap[name];
										switch (Type.GetTypeCode(_FieldTypes[k]))
										{
											case TypeCode.String:
												SetValueByOrdinal(node, i, value, extensions);
												break;
											case TypeCode.Int16:
												{
													short val;
													if (short.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Int32:
												{
													int val;
													if (int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Int64:
												{
													long val;
													if (long.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Boolean:
												{
													bool val;
													if (bool.TryParse(value, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Decimal:
												{
													decimal val;
													if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Single:
												{
													float val;
													if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Double:
												{
													double val;
													if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.DateTime:
												{
													DateTime val;
													if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Object:
												if (_FieldTypes[k] == typeof(Guid))
												{
													Guid val;
													if (PX.Common.GUID.TryParse(value, out val))
													{
														SetValueByOrdinal(node, i, val, extensions);
													}
												}
												else if (_FieldTypes[k] == typeof(byte[]))
												{
													SetValueByOrdinal(node, i, Convert.FromBase64String(value), extensions);
												}
												break;
										}
									}
								}
							}
						}
						return node;
					}
				}
			}
			return null;
		}

		internal override string ValueToString(string fieldName, object val, object dbval)
		{
			if (_EncryptAuditFields != null && _EncryptAuditFields.Contains(fieldName) && dbval is string)
			{
				return (string)dbval;
			}
			return base.ValueToString(fieldName, val, dbval);
		}

		/// <summary>Converts the provided value of the field to string and
		/// returns the resulting value. No events are raised.</summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="val">The field value.</param>
		public override string ValueToString(string fieldName, object val)
		{
			if (val == null)
			{
				return null;
			}

			if (_BypassAuditFields != null && _BypassAuditFields.Contains(fieldName))
				return PXDBUserPasswordAttribute.DefaultVeil;
			if (!IsKvExtAttribute(fieldName))
			{
			switch (Type.GetTypeCode(_FieldTypes[_ReverseMap[fieldName]]))
			{
				case TypeCode.String:
					return (string)val;
				case TypeCode.Int16:
					return ((short)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Int32:
					return ((int)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Int64:
					return ((long)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Boolean:
					return ((bool)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Decimal:
					return ((decimal)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Single:
					return ((float)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Double:
					return ((double)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.DateTime:
					return ((DateTime)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Object:
					if (_FieldTypes[_ReverseMap[fieldName]] == typeof(Guid))
					{
						return ((Guid)val).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
					}
					else if (_FieldTypes[_ReverseMap[fieldName]] == typeof(byte[]))
					{
						return Convert.ToBase64String((byte[])val);
					}
					return null;
			}
			}
			else
			{
				switch (_KeyValueAttributeTypes[fieldName])
				{
					case StorageBehavior.KeyValueDate:
						return ((DateTime)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
					case StorageBehavior.KeyValueNumeric:
						return ((int)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
					case StorageBehavior.KeyValueString:
					case StorageBehavior.KeyValueText:
						return (string)val;
					default:
						return null;
				}
			}
			return null;
		}

		/// <summary>Converts the provided value of the field from a string to the
		/// appropriate type and returns the resulting value. No events are
		/// raised.</summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="val">The string representation of the field
		/// value.</param>
		public override object ValueFromString(string fieldName, string val)
		{
            if (FilterVariable.GetVariableType(val) != null || RelativeDatesManager.IsRelativeDatesString(val)) // TODO: refactoring of a relative filters is needed to do this properly
            {
                return val;
            }
			int k = -1;
			Type t = null;
			if (_ReverseMap.TryGetValue(fieldName, out k))
			{
				t = _FieldTypes[k];
			}
			else
			{
				PXFieldState state = GetStateExt(null, fieldName) as PXFieldState;
				if (state != null)
				{
					t = state.DataType;
				}
			}

			if (t == null) return null;

			switch (Type.GetTypeCode(t))
			{
				case TypeCode.String:
					return val;
				case TypeCode.Int16:
					{
						short value;
						if (short.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Int32:
					{
						int value;
						if (int.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Int64:
					{
						long value;
						if (long.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Boolean:
					{
						bool value;
						if (bool.TryParse(val, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Decimal:
					{
						decimal value;
						if (decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Single:
					{
						float value;
						if (float.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Double:
					{
						double value;
						if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.DateTime:
					{
						DateTime value;
						if (DateTime.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Object:
					if (_FieldTypes[k] == typeof(Guid))
					{
						Guid value;
						if (PX.Common.GUID.TryParse(val, out value))
						{
							return value;
						}
					}
					else if (_FieldTypes[k] == typeof(byte[]))
					{
						return Convert.FromBase64String(val);
					}
					break;
			}
			return null;
		}
		/// <summary>
		/// Copies values of all fields from the second data record to the
		/// first data record.
		/// </summary>
		/// <param name="item">The data record whose field values are updated.</param>
		/// <param name="copy">The data record whose field values are copied.</param>
		/// <example>
		/// The code below modifies an <tt>APRegister</tt> data record and copies the
		/// values of all its fields to an <tt>APInvoice</tt> data record.
		/// <code>
		/// APInvoice apdoc = ...
		/// ...
		/// // Modifying the doc data record
		/// doc.OpenDoc = true;
		/// doc.ClosedFinPeriodID = null;
		/// ...
		/// // Copying all fields of doc to apdoc (APInvoince derives from APRegister)
		/// PXCache&lt;APRegister&gt;.RestoreCopy(apdoc, doc);
		/// </code>
		/// </example>
		public static void RestoreCopy(TNode item, TNode copy)
		{
			CacheStaticInfo result = _Initialize(false);

			if (result._CreateExtensions != null)
			{
				PXCacheExtension[] copyextensions;
				PXCacheExtension[] itemextensions;
				PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>();
				if (dict == null)
				{
					dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}
				lock (((ICollection)dict).SyncRoot)
				{
					if (!dict.TryGetValue(copy, out copyextensions))
					{
						dict[copy] = copyextensions = result._CreateExtensions(copy);
					}
					if (!dict.TryGetValue(item, out itemextensions))
					{
						dict[item] = itemextensions = result._CreateExtensions(item);
					}
				}
				for (int i = 0; i < result._ClassFields.Count; i++)
				{
					result._SetValueByOrdinal(item, i, result._GetValueByOrdinal(copy, i, copyextensions), itemextensions);
				}
			}
			else
			{
				for (int i = 0; i < result._ClassFields.Count; i++)
				{
					result._SetValueByOrdinal(item, i, result._GetValueByOrdinal(copy, i, null), null);
				}
			}
		}

        /// <summary>
        /// Used to sync extension objects.
        /// </summary>
        /// <param name="item">The data record whose field values should be synced.</param>
        public static void SyncModel(TNode item)
        {
            CacheStaticInfo result = _Initialize(false);

            if (result._CreateExtensions != null)
            {
                PXCacheExtension[] itemextensions;
                PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>();
                if (dict == null)
                {
                    dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
                }
                lock (((ICollection)dict).SyncRoot)
                {
                    if (!dict.TryGetValue(item, out itemextensions))
                    {
                        dict[item] = itemextensions = result._CreateExtensions(item);
                    }
                }
                for (int i = 0; i < result._ClassFields.Count; i++)
                {
                    result._SetValueByOrdinal(item, i, result._GetValueByOrdinal(item, i, itemextensions), itemextensions);
                }
            }
        }

		internal static Type GetItemTypeInternal()
		{
			return typeof(TNode);
		}

		/// <summary>Gets the collection of names of fields and virtual fields. By
		/// default, the collection includes all public properties of the DAC that
		/// is associated with the cache. The collection may also include the
		/// virtual fields that are injected by attributes (such as the
		/// description field of the <see
		/// cref="PXSelectorAttribute">PXSelector</see> attribute). The
		/// developer can add any field to the collection.</summary>
		public override PXFieldCollection Fields
		{
			get
			{
				if (_Fields == null)
				{
					_Fields = new PXFieldCollection(_ClassFields, _FieldsMap);
				}
				return _Fields;
			}
		}

		/// <summary>Gets the list of classes that implement <tt>IBqlField</tt>
		/// and are nested in the DAC and its base type. These types represent DAC
		/// fields in BQL queries. This list differs from the list that the
		/// <tt>Fields</tt> property returns.</summary>
		public override List<Type> BqlFields
		{
			get
			{
				if (_BqlFields == null)
				{
					_BqlFields = new List<Type>(_BqlFieldsMap.Keys);
				}
				return _BqlFields;
			}
		}

		/// <summary>Gets the collection of BQL types that correspond to the key
		/// fields which the DAC defines.</summary>
		public override List<Type> BqlKeys
		{
			get
			{
				if (_BqlKeys == null)
				{
					_BqlKeys = new List<Type>();
					foreach (string key in Keys)
					{
						foreach (Type t in BqlFields)
						{
							if (String.Compare(t.Name, key, StringComparison.OrdinalIgnoreCase) == 0)
							{
								_BqlKeys.Add(t);
								break;
							}
						}
					}
				}
				return _BqlKeys;
			}
		}

		/// <exclude/>
        public override List<Type> BqlImmutables
        {
            get
            {
                if (_BqlImmutables == null)
                {
                    _BqlImmutables = new List<Type>();
                    foreach (string key in Immutables)
                    {
                        foreach (Type t in BqlFields)
                        {
                            if (String.Compare(t.Name, key, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _BqlImmutables.Add(t);
                                break;
                            }
                        }
                    }
                }
                return _BqlImmutables;
            }
        }

		private BqlCommand _BqlSelect;
		/// <exclude/>
		public override BqlCommand BqlSelect
		{
			get
			{
				return _BqlSelect;
			}
			set
			{
				_BqlSelect = value;
			}
		}

		/// <summary>Gets the DAC the cache is associated with. The DAC is
		/// specified through the type parameter when the cache is
		/// instantiated.</summary>
		public override Type BqlTable
		{
			get
			{
				return _BqlTable;
			}
		}

		public override Type GenericParameter { get { return typeof(TNode); } }

		internal override Type GetFieldType(string fieldName)
		{
			int iField;
			if(_ReverseMap.TryGetValue(fieldName, out iField))
		        return _FieldTypes[iField];
		    if (_KeyValueAttributeTypes != null && _KeyValueAttributeTypes.TryGetValue(fieldName, out var storageBehavior))
		    {
		        switch (storageBehavior)
		        {
		            case StorageBehavior.KeyValueDate:
		                return typeof(DateTime);
		            case StorageBehavior.KeyValueNumeric:
		                return typeof(int);
		            case StorageBehavior.KeyValueString:
		            case StorageBehavior.KeyValueText:
		                return typeof(string);
		            default:
		                return null;
		        }
		    }
		    return null;
		}

        internal override Type GetBaseBqlField(string field)
        {
            if (_BqlFieldsMap != null)
            {
                return _BqlFieldsMap.Keys
                       .Where(f => f.DeclaringType == BqlTable && f.Name.Equals(field, StringComparison.OrdinalIgnoreCase))
                       .FirstOrDefault();
            }
            else
            {
                return base.GetBaseBqlField(field);
            }
        }

		/// <exclude/>
		public override Type[] GetExtensionTypes()
		{
			return _ExtensionTypes;
		}
		#endregion

		#region Object
		private System.Threading.ReaderWriterLock _DelegatesLock;
		private EqualsDelegate _Equals;
		private Dictionary<string, EqualsDelegate> _DelegatesEquals;
		/// <summary>Compares two data records by the key fields. Returns
		/// <tt>true</tt> if the values of all key fields in the data records are
		/// equal. Otherwise, returns <tt>false</tt>.</summary>
		/// <param name="a">The first data record to compare.</param>
		/// <param name="b">The second data record to compare.</param>
		public override bool ObjectsEqual(object a, object b)
		{
			TNode anode = a as TNode;
			TNode bnode = b as TNode;
			if (anode != null && bnode != null)
			{
				return _Equals((TNode)a, (TNode)b);
			}
			else
			{
				IDictionary adict = a as IDictionary;
				IDictionary bdict = b as IDictionary;
				if (adict != null && bdict != null)
				{
					foreach (string key in _Keys)
					{
						if (!adict.Contains(key) || !bdict.Contains(key) || !Object.Equals(adict[key], bdict[key]))
						{
							return false;
						}
						return true;
					}
				}
				else
				{
					return false;
				}
			}
			return false;
		}
		private GetHashCodeDelegate _GetHashCode;
		private Dictionary<string, GetHashCodeDelegate> _DelegatesGetHashCode;
		/// <summary>Returns the hash code generated from key field
		/// values.</summary>
		/// <param name="data">The data record.</param>
		public override int GetObjectHashCode(object data)
		{
			TNode node = data as TNode;
			if (node != null)
			{
				return _GetHashCode((TNode)data);
			}
			else
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					unchecked
					{
						int ret = 13;
						foreach (string key in _Keys)
						{
							object value = null;
							if (!dict.Contains(key) || (value = dict[key]) == null)
							{
								return 0;
							}
							ret = ret * 37 + dict[key].GetHashCode();
						}
						return ret;
					}
				}
			}
			return 0;
		}
		/// <summary>Returns a string of key fields and their values in the
		/// <i>{key1=value1, key2=value2}</i> format.</summary>
		/// <param name="data">The data record which key fields are written to a
		/// string.</param>
		public override string ObjectToString(object data)
		{
			if (data == null)
			{
				return String.Empty;
			}
			if (data is PXResult)
			{
				data = ((PXResult)data)[0];
			}
			TNode item = data as TNode;
			if (item == null)
			{
				return data.ToString();
			}
			StringBuilder bld = new StringBuilder(typeof(TNode).Name);
			bld.Append("{");
			bool first = true;
			foreach (string name in Keys)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					bld.Append(", ");
				}
				bld.Append(name);
				bld.Append(" = ");
				bld.Append(GetValue(data, name));
			}
			bld.Append("}");
			return bld.ToString();
		}
		#endregion

		#region Schema
        /// <summary>Gets a DAC extention instance of the specified type. The extension type is specified as the type parameter.</summary>
        /// <param name="item">A data record whose extension is returned.</param>
        /// <example>
        /// 	<code title="Example" description="The code below gets an extension data record corresponding to the given instance of the base data record." lang="CS">
        /// InventoryItem item = cache.Current as InventoryItem;
        /// InventoryItemExtension itemExt = 
        ///     cache.GetExtension&lt;InventoryItemExtension&gt;(item);</code>
        /// </example>
		public override Extension GetExtension<Extension>(object item)
		{
		    if (item == null) return null;

			int idx = Array.IndexOf(_ExtensionTypes, typeof(Extension));
			if (idx == -1 || _Extensions == null)
			{
				throw new PXException(ErrorMessages.IncorrectExtensionRequested);
			}
			PXCacheExtension[] extensions;
			lock (((ICollection)_Extensions).SyncRoot)
			{
				if (!_Extensions.TryGetValue((TNode)item, out extensions))
				{
					_Extensions[(TNode)item] = extensions = _CreateExtensions((TNode)item);
				}
			}
			return (Extension)extensions[idx];
		}
		public override object GetMain<Extension>(Extension item)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Gets an extension of appropriate type
		/// </summary>
		/// <typeparam name="Extension">
		/// The type of extension requested
		/// </typeparam>
		/// <param name="item">
		/// Parent standard object
		/// </param>
		/// <returns>
		/// Object of type Extension
		/// </returns>
		public static Extension GetExtension<Extension>(TNode item)
			where Extension : PXCacheExtension<TNode>
		{
			CacheStaticInfo result = _Initialize(false);

			int idx = Array.IndexOf(result._ExtensionTypes, typeof(Extension));
			if (idx == -1)
			{
				throw new PXException(ErrorMessages.IncorrectExtensionRequested);
			}
			PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>();
			if (dict == null)
			{
				dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
			}
			PXCacheExtension[] extensions;
			lock (((ICollection)dict).SyncRoot)
			{
				if (!dict.TryGetValue(item, out extensions) || extensions == null)
				{
					dict[item] = extensions = result._CreateExtensions(item);
				}
			}
			return (Extension)extensions[idx];
		}

		internal override PXCacheExtension[] GetCacheExtensions(IBqlTable item)
		{
			PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>() 
				?? PXContext.SetSlot(new PXCacheExtensionCollection());

			PXCacheExtension[] extensions;
			lock (((ICollection)dict).SyncRoot)
			{
				if (!dict.TryGetValue(item, out extensions) || extensions == null)
				{
					CacheStaticInfo result = _Initialize(false);
                    dict[item] = extensions = result._CreateExtensions((TNode)item);
				}
			}

			return extensions;
		}

        /// <summary>
        /// 	<para>Returns a DAC type data records that are stored in the cache.</para>
        /// </summary>
		public override Type GetItemType()
		{
			return typeof(TNode);
		}

		internal static string[] GetKeyNames(PXGraph graph) => GetKeyNames(graph, typeof(TNode));

		internal List<string> _ClassFields;
		private Dictionary<string, int> _ReverseMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		internal List<Type> _FieldTypes;
		private Dictionary<Type, int> _BqlFieldsMap;
		private Dictionary<int, Type> _InverseBqlFieldsMap;
		private static Type _BqlTable;
		private PXDBInterceptorAttribute _Interceptor;
		internal Func<TNode, SQLTree.Query, object[], PXResult<TNode>> _TailAppender;
		internal Dictionary<string, Func<TNode, SQLTree.Query, object[], PXResult>> _SelectorGetter;
		internal override PXResult _InvokeTailAppender(object data, SQLTree.Query query, object[] pars)
		{
			return _TailAppender?.Invoke((TNode)data, query, pars);
		}
		internal override object _InvokeSelectorGetter(object data, string field, PXView view, object[] pars, bool unwrap)
		{
			Func<TNode, SQLTree.Query, object[], PXResult> getter;
			if (!(_Interceptor is Unit.PXUIEmulatorAttribute) || _SelectorGetter == null || !_SelectorGetter.TryGetValue(field, out getter))
			{
				return null;
			}
			object ret = getter((TNode)data, view.BqlSelect.GetQuery(Graph, view), pars);
			if (ret != null && unwrap)
			{
				ret = PXResult.UnwrapMain(ret);
			}
			return ret;
		}
		/// <exclude/>
		public override PXDBInterceptorAttribute Interceptor
		{
			get
			{
				return _Interceptor;
			}
			set
			{
				_Interceptor = value;
				_Interceptor?.CacheAttached(this);
			}
		}
		internal List<PXEventSubscriberAttribute> _FieldAttributes;
		internal EventsRowMap _EventsRowMap;
		internal Dictionary<string, EventsFieldMap> _EventsFieldMap;
		internal int[] _FieldAttributesFirst;
		internal int[] _FieldAttributesLast;
		internal EventPosition[] _EventPositions;


		private List<TNode> _PendingItems;
		private int[] _AttributesFirst;
		private int[] _AttributesLast;
		#endregion




		#region Cache

		public override Type GetBqlField(string field)
		{
			int ordinal;
			Type bqlField;
			if (_FieldsMap.TryGetValue(field, out ordinal) && _InverseBqlFieldsMap.TryGetValue(ordinal, out bqlField))
				return bqlField;
			return null;
		}

		/// <summary>Returns the number of fields and virtual fields which
		/// comprise the <tt>Fields</tt> collection.</summary>
		public override int GetFieldCount()
		{
			return Fields.Count;
		}
        /// <summary>Returns the index of the specified field in the internally kept fields map.</summary>
		public override int GetFieldOrdinal<Field>()
		{
			int ordinal;
			if (_FieldsMap.TryGetValue(typeof(Field).Name, out ordinal))
			{
				return ordinal;
			}
			return -1;
		}
		/// <summary>Returns the index of the specified field in the internally
		/// kept fields map.</summary>
		/// <param name="field">The name of the field whose index is
		/// returned.</param>
		public override int GetFieldOrdinal(string field)
		{
			int ordinal;
			if (_FieldsMap.TryGetValue(field, out ordinal))
			{
				return ordinal;
			}
			return -1;
		}
		/// <summary>Recalculates internally stored hash codes. The method should
		/// be called after a key field is modified in a data record from the
		/// cache.</summary>
		public override void Normalize()
		{
			_Items.Normalize(null);
		}

		private PXCollection<TNode> _Items;
		private TNode _CurrentPlacedIntoCache;
		private bool _ItemsDenormalized;

		protected PXCollection<TNode> Items => _Items;

		/// <summary>Gets or sets the current data record. This property points to
		/// the last data record displayed in the user interface. If the user
		/// selects a data record in a grid, this property points to this data
		/// record. If the user or the application inserts, updates, or deletes a
		/// data record, the property points to this data record. Assigning this
		/// property raises the <tt>RowSelected</tt> event.</summary>
		/// <remarks>You can reference the <tt>Current</tt> data record and its
		/// fields in the <tt>PXSelect</tt> BQL statements by using the
		/// <tt>Current</tt> parameter.</remarks>
		public override object Current
		{
			get
			{
				if (_Current == null)
				{
					if (!stateLoaded)
					{
						Load();
						if (_Current != null)
						{
							return _Current;
						}
					}
					_Current = _Graph.GetDefault<TNode>();
				}
				_CurrentPlacedIntoCache = null;
				return _Current;
			}
			set
			{
				_Current = value as TNode;
				OnRowSelected(_Current);
			}
		}

		/// <exclude/>
		public override object InternalCurrent
		{
			get { return _Current; }
		}
		#endregion

		#region Ctor

		protected static CacheStaticInfo _Initialize(bool ignoredResult)
		{
			CacheStaticInfoBase result = null;
			Dictionary<Type, CacheStaticInfoBase> initialized = PXContext.GetSlot<CacheStaticInfoDictionary>();
			if (initialized == null && !ignoredResult)
			{
				try
				{
					initialized = PXContext.SetSlot<CacheStaticInfoDictionary>(PXDatabase.GetSlot<CacheStaticInfoDictionary>("CacheStaticInfo", typeof(PXGraph.FeaturesSet)));
				}
				catch
				{
				}
			}
			if (initialized != null)
			{
				
			    using (var scope = new PXReaderWriterScope(PXExtensionManager._StaticInfoLock))
			    {
			        scope.AcquireReaderLock();

                    if (initialized.TryGetValue(typeof(TNode), out result))
					{
						return (CacheStaticInfo)result;
					}
				}

			}
			Dictionary<string, string> inactiveFields;
			List<Type> extensions = _GetExtensions(typeof(TNode), initialized != null, out inactiveFields);

			Dictionary<PXExtensionManager.ListOfTypes, CacheStaticInfoBase> multiple;
		    PXExtensionManager.ListOfTypes keyForMultiple = new PXExtensionManager.ListOfTypes(extensions);

            using (var scope = new PXReaderWriterScope(PXExtensionManager._StaticInfoLock))
		    {
		        scope.AcquireReaderLock();
		        

                if (PXExtensionManager._CacheStaticInfo.TryGetValue(typeof(TNode), out multiple)
					&& multiple.TryGetValue(keyForMultiple, out result))
				{
				    scope.UpgradeToWriterLock();
		
						if (initialized != null)
						{
							initialized[typeof(TNode)] = result;
						}
						return (CacheStaticInfo)result;

				}
			}


			//// No luck finding that info in the dictionary
			//if (extensions == null) {
			//	extensions = _GetExtensions(typeof(TNode), initialized != null);
			//}

			result = buildCacheStaticInfo(ignoredResult, keyForMultiple);
			if (ignoredResult)
				return null;
			((CacheStaticInfo)result)._InactiveFields = inactiveFields;

		    using (var scope = new PXReaderWriterScope(PXExtensionManager._StaticInfoLock))
            {
                scope.AcquireWriterLock();
                if (initialized != null)
				{
					initialized[typeof(TNode)] = result;
				}
				if (!PXExtensionManager._CacheStaticInfo.TryGetValue(typeof(TNode), out multiple))
				{
					PXExtensionManager._CacheStaticInfo[typeof(TNode)] = multiple = new Dictionary<PXExtensionManager.ListOfTypes, CacheStaticInfoBase>();
				}
				multiple[keyForMultiple] = result;
			}


			return (CacheStaticInfo)result;
		}

		private PXCacheExtensionCollection _Extensions;
		private int _OriginalsRequested;
		private int _OriginalsReadAhead;

		static PXCache()
		{
			try
			{
				_Initialize(true);
			}
			catch
			{
				/* Prevent any falls in static constructor.*/
				/*Please make sure that you do not use datbase in attribute constructors.*/
			}
		}

		protected HashSet<string> _GraphSpecificFields;
		protected int? _TimestampOrdinal;

		protected void SetAutomationFieldDefaulting(Type cacheType)
		{
			var d = PXAccess.GetDefaultingDelegate(cacheType);
			if (d != null)
			{
				this.AutomationFieldDefaulting = d;
			}
		}
     
        /// <summary>The application does not need to instantiate PXCache directly, as the system creates caches automatically whenever they are needed. A cache instance is always bound to an instance of the business logic controller (graph). The application typically accesses a cache instance through the Cache property of a data view. The property always returns the valid cache instance, even if it didn’t exist before the property was accessed. A cache instance is also available through the Caches property of the graph to which the cache instance is bound.</summary>
		public PXCache(PXGraph graph)
		{
		    var tm = Stopwatch.StartNew();
            CacheStaticInfo result = _Initialize(false);

			_CreateExtensions = result._CreateExtensions;
			_CloneExtensions = result._CloneExtensions;
			_ExtensionTables = result._ExtensionTables;
			_ExtensionTypes = result._ExtensionTypes;
			_SetValueByOrdinal = result._SetValueByOrdinal;
			_GetValueByOrdinal = result._GetValueByOrdinal;
			_DelegatesLock = result._DelegatesLock;
			_DelegatesGetHashCode = result._DelegatesGetHashCode;
			_DelegatesEquals = result._DelegatesEquals;
			_FieldsMap = result._FieldsMap;
			_FieldAttributes = result._FieldAttributes;
			_EventsRowMap = result._EventsRowMap;
			_EventsFieldMap = result._EventsFieldMap;
			_FieldAttributesFirst = result._FieldAttributesFirst;
			_FieldAttributesLast = result._FieldAttributesLast;
			_EventPositions = result._EventPositions;
			_ClassFields = result._ClassFields;
			_ReverseMap = result._ReverseMap;
			_FieldTypes = result._FieldTypes;
			_BqlFieldsMap = result._BqlFieldsMap;
			_InverseBqlFieldsMap = result._InverseBqlFieldsMap;
			_TimestampOrdinal = result._TimestampOrdinal;
			_KeyValueStoredOrdinals = result._KeyValueStoredOrdinals;
			_KeyValueStoredNames = result._KeyValueStoredNames;
			_DBLocalizableNames = result._DBLocalizableNames;
			_FirstKeyValueStored = result._FirstKeyValueStored;
			_InactiveFields = result._InactiveFields;
			SetAutomationFieldDefaulting(typeof(TNode));

			_Graph = graph;
			if (_CreateExtensions != null)
			{
				_Extensions = PXContext.GetSlot<PXCacheExtensionCollection>();
				if (_Extensions == null)
				{
					_Extensions = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}
			}
			_Originals = PXContext.GetSlot<PXCacheOriginalCollection>();
			if (_Originals == null)
			{
				_Originals = PXContext.SetSlot<PXCacheOriginalCollection>(new PXCacheOriginalCollection());
			}

			AlteredDescriptor altered = graph.GetAlteredAttributes(typeof(TNode));
			if (altered != null && altered.Fields != null && altered.Fields.Count > 0)
			{
				_GraphSpecificFields = altered.Fields;
			}
			_Items = new PXCollection<TNode>(this);
			
			FillEventsRowAttr(altered);

			_Graph.Caches.InitCacheMapping(_Graph);

			Type tkey = typeof(TNode);
			while (tkey != typeof(object))
			{
				if (!_Graph.Caches.ContainsKey(tkey) && _Graph.Caches.HasCacheMapping(tkey, typeof(TNode)))
				{
				    if (_Graph.IsInitializing || !_Graph.Caches.CanInitLazyCache())
				    {
				        AutomationHidden = _Graph.AutomationHidden;
				        AutomationInsertDisabled = _Graph.AutomationInsertDisabled;
				        AutomationUpdateDisabled = _Graph.AutomationUpdateDisabled;
				        AutomationDeleteDisabled = _Graph.AutomationDeleteDisabled;
				        _Graph.Caches.Add(tkey, this);
				        _Graph.Caches.AttachHandlers(tkey, this);
				    }
				    else
                        _Graph.Caches.Add(tkey, this);
				}
				if ((tkey == typeof(TNode) || typeof(TNode).IsSubclassOf(tkey)) && tkey == result._BreakInheritanceType
					|| _Graph.GetType() == typeof(PXGraph) || _Graph.GetType() == typeof(PXGenericInqGrph) || _Graph.GetType() == typeof(PX.Data.Maintenance.GI.GenericInquiryDesigner))
				{
					break;
				}

				if (_Graph._ReadonlyCacheCreation)
				{
					_ReadonlyCreatedCache = true;
					break;
				}
				tkey = tkey.BaseType;
			}

			int kvattributepos = -1;
			List<PXEventSubscriberAttribute> secondPassAttributes = new List<PXEventSubscriberAttribute>();

		    foreach (PXEventSubscriberAttribute descr in _CacheAttributes)
		    {
		        if (descr is PXDBAttributeAttribute)
				{
					int fl = String.IsNullOrEmpty(_NoteIDName) ? (_Fields != null ? _Fields.Count : 0) : -1;
		            descr.InvokeCacheAttached(this);
					if (fl >= 0 && !String.IsNullOrEmpty(_NoteIDName))
					{
						kvattributepos = Fields.IndexOf(_NoteIDName) + (_Fields != null ? _Fields.Count : 0) - fl + 1;
					}
				}
				else
				{
		            secondPassAttributes.Add(descr);
				}
		    }


		    foreach (PXEventSubscriberAttribute descr in secondPassAttributes)
		    {
				int fl = String.IsNullOrEmpty(_NoteIDName) ? (_Fields != null ? _Fields.Count : 0) : -1;
		        descr.InvokeCacheAttached(this);
				if (fl >= 0 && !String.IsNullOrEmpty(_NoteIDName))
				{
					kvattributepos = Fields.IndexOf(_NoteIDName) + (_Fields != null ? _Fields.Count : 0) - fl + 1;
				}
		    }

		    if (altered != null && altered._Method != null)
			{
				altered._Method(_Graph, this);
			}

			if (result._TypeInterceptorAttribute != null)
			{
				_Interceptor = ((PXDBInterceptorAttribute)result._TypeInterceptorAttribute).Clone();
				if (_ExtensionTables != null && _ExtensionTables.Count > 0
					&& _ExtensionTables.Last().BaseType.IsGenericType
					&& (_ExtensionTables.Last().BaseType.GetGenericArguments().Last() == typeof(TNode)
					|| _ExtensionTables.Last().BaseType.GetGenericArguments().Last().IsAssignableFrom(typeof(TNode))))
				{
					if (result._ExtensionInterceptorAttribute != null)
					{
						_Interceptor.Child = ((PXDBInterceptorAttribute)result._ExtensionInterceptorAttribute).Clone();
					}
				}
				_Interceptor.CacheAttached(this);
				_BqlSelect = _Interceptor.GetTableCommand();
			}
			else if (_ExtensionTables != null && _ExtensionTables.Count > 0 && result._ExtensionInterceptorAttribute != null)
			{
				_Interceptor = ((PXDBInterceptorAttribute)result._ExtensionInterceptorAttribute).Clone();
				_Interceptor.CacheAttached(this);
				_BqlSelect = _Interceptor.GetTableCommand();
			}

			if (result._ClassAttributes != null)
            {
				for (int i = 0; i < result._ClassAttributes.Length; i++)
                {
					PXClassAttribute attr = ((PXClassAttribute)result._ClassAttributes[i]).Clone();
					attr.CacheAttached(this);
                }
            }

		    StringBuilder bld = new StringBuilder();
			foreach (string key in Keys)
			{
				bld.Append(key);
			}
			string keysequence = bld.ToString();
			
		    using (var scope = new PXReaderWriterScope(_DelegatesLock))
		    {
		        scope.AcquireReaderLock();

                if (!_DelegatesEquals.TryGetValue(keysequence, out _Equals)
					|| !_DelegatesGetHashCode.TryGetValue(keysequence, out _GetHashCode))
				{
				    scope.UpgradeToWriterLock();
                    
					if (!_DelegatesEquals.TryGetValue(keysequence, out _Equals)
						|| !_DelegatesGetHashCode.TryGetValue(keysequence, out _GetHashCode))
					{
						DynamicMethod dm_eq;
						DynamicMethod dm_hash;
						if (!PXGraph.IsRestricted)
						{
							dm_eq = new DynamicMethod("_Equals", typeof(bool), new Type[] { typeof(TNode), typeof(TNode) }, this.GetType());
							dm_hash = new DynamicMethod("_GetHashCode", typeof(int), new Type[] { typeof(TNode) }, this.GetType());
						}
						else
						{
							dm_eq = new DynamicMethod("_Equals", typeof(bool), new Type[] { typeof(TNode), typeof(TNode) });
							dm_hash = new DynamicMethod("_GetHashCode", typeof(int), new Type[] { typeof(TNode) });
						}
						ILGenerator il_eq = dm_eq.GetILGenerator();
						System.Reflection.Emit.Label ret_eq_false = il_eq.DefineLabel();
						System.Reflection.Emit.Label ret_eq = il_eq.DefineLabel();
						ILGenerator il_hash = dm_hash.GetILGenerator();
						il_hash.DeclareLocal(typeof(int));
						il_hash.Emit(OpCodes.Ldc_I4_S, 13);
						il_hash.Emit(OpCodes.Stloc_0);
						il_eq.DeclareLocal(typeof(bool));
						MethodInfo equality = typeof(object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(object) }, null);
						foreach (string key in Keys)
			            {
							PropertyInfo prop = typeof(TNode).GetProperty(key);
							Type propType = prop.PropertyType;
							if (prop.CanRead)
			                {
								MethodInfo getter = prop.GetGetMethod();

		                        il_eq.Emit(OpCodes.Ldarg_0);
		                        il_eq.Emit(OpCodes.Callvirt, getter);
		                        if (propType.IsValueType)
		                        {
		                            il_eq.Emit(OpCodes.Box, propType);
		                        }

		                        il_eq.Emit(OpCodes.Ldarg_1);
		                        il_eq.Emit(OpCodes.Callvirt, getter);
		                        if (propType.IsValueType)
		                        {
		                            il_eq.Emit(OpCodes.Box, propType);
		                        }

		                        il_eq.Emit(OpCodes.Call, equality);
		                        il_eq.Emit(OpCodes.Brfalse, ret_eq_false);


								LocalBuilder local = il_hash.DeclareLocal(propType);
								il_hash.Emit(OpCodes.Ldloc_0);
								il_hash.Emit(OpCodes.Ldc_I4_S, 37);
								il_hash.Emit(OpCodes.Mul);
								il_hash.Emit(OpCodes.Stloc_0);
								il_hash.Emit(OpCodes.Ldarg_0);
								il_hash.Emit(OpCodes.Callvirt, getter);
								il_hash.Emit(OpCodes.Stloc_S, local.LocalIndex);
								if (!propType.IsValueType)
								{
									System.Reflection.Emit.Label hash_next = il_hash.DefineLabel();
									il_hash.Emit(OpCodes.Ldnull);
									il_hash.Emit(OpCodes.Ldloc_S, local.LocalIndex);
									il_hash.Emit(OpCodes.Ceq);
									il_hash.Emit(OpCodes.Brtrue_S, hash_next);
									il_hash.Emit(OpCodes.Ldloc_S, local.LocalIndex);
									il_hash.Emit(OpCodes.Callvirt, propType.GetMethod("GetHashCode", new Type[0]));
									il_hash.Emit(OpCodes.Ldloc_0);
									il_hash.Emit(OpCodes.Add);
									il_hash.Emit(OpCodes.Stloc_0);
									il_hash.MarkLabel(hash_next);
								}
								else
								{
									il_hash.Emit(OpCodes.Ldloca_S, local.LocalIndex);
									il_hash.Emit(OpCodes.Call, propType.GetMethod("GetHashCode", new Type[0]));
									il_hash.Emit(OpCodes.Ldloc_0);
									il_hash.Emit(OpCodes.Add);
									il_hash.Emit(OpCodes.Stloc_0);
								}
							}
						}
						il_eq.Emit(OpCodes.Ldc_I4_1);
						il_eq.Emit(OpCodes.Stloc_0);
						il_eq.Emit(OpCodes.Br, ret_eq);
						il_eq.MarkLabel(ret_eq_false);
						il_eq.Emit(OpCodes.Ldc_I4_0);
						il_eq.Emit(OpCodes.Stloc_0);
						il_eq.MarkLabel(ret_eq);
						il_eq.Emit(OpCodes.Ldloc_0);
						il_eq.Emit(OpCodes.Ret);
						il_hash.Emit(OpCodes.Ldloc_0);
						il_hash.Emit(OpCodes.Ret);
						_DelegatesEquals[keysequence] = _Equals = (EqualsDelegate)dm_eq.CreateDelegate(typeof(EqualsDelegate));
						_DelegatesGetHashCode[keysequence] = _GetHashCode = (GetHashCodeDelegate)dm_hash.CreateDelegate(typeof(GetHashCodeDelegate));
					}
		
				}
			}


		    graph.Caches._cacheLogger.CacheCreated(this.GetItemType(), tm.ElapsedMilliseconds);

            //if (initaliases)
            //{
            //    lock (_FieldAliases)
            //    {
            //        foreach (string key in Keys)
            //        {
            //            _FieldAliases[key] = _BqlTables[0].Name;
            //        }
            //        initaliases = false;
            //    }
            //}

			PX.CS.KeyValueHelper.TableAttribute[] kvattributes = PX.CS.KeyValueHelper.Def?.GetAttributes(typeof(TNode)) ?? new PX.CS.KeyValueHelper.TableAttribute[0];
			int kvlength = kvattributes.Length;
			if (kvlength > 0 && kvattributepos >= 0)
			{
				_KeyValueAttributeNames = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
				_KeyValueAttributeTypes = new Dictionary<string, StorageBehavior>(StringComparer.InvariantCultureIgnoreCase);
				_KeyValueAttributeSlotPosition = SetupSlot<object[]>(
					() => new object[kvlength],
					(item, copy) =>
					{
						for (int i = 0; i < kvlength && i < item.Length && i < copy.Length; i++)
						{
							item[i] = copy[i];
						}
						return item;
					},
					(item) => (object[])item.Clone()
					);

				for (int i = 0; i < kvattributes.Length; i++)
				{
					string fieldName = kvattributes[i].FieldName;
					Fields.Insert(kvattributepos, fieldName);
					kvattributepos++;
					_KeyValueAttributeNames[fieldName] = i;
					_KeyValueAttributeTypes[fieldName] = kvattributes[i].Storage;
					fieldName = fieldName.ToLower();
					int idx = i;
					if (i == 0)
					{
						_FirstKeyValueAttribute = new KeyValuePair<string, int>(kvattributes[i].FieldName, 0);
					}
					kvattributes[idx].CacheAttached?.Invoke(this);
					FieldUpdatingEvents[fieldName] +=
						(PXCache c, PXFieldUpdatingEventArgs e) =>
						{
                            object value = e.NewValue;
                            kvattributes[idx].FieldUpdating?.Invoke(c, e);
                            if (value is bool && e.NewValue is int)
                                e.NewValue = value;
							if (e.Row != null)
							{
								object[] slot = GetSlot<object[]>(e.Row, _KeyValueAttributeSlotPosition);
								if (slot == null && GetStatus(e.Row) != PXEntryStatus.Inserted)
								{
									OnCommandPreparing(_NoteIDName, e.Row, null, PXDBOperation.Select, null, out var description);
									if (OnDemandCommand.GetKeyValues(this, e.Row, description.BqlTable, _KeyValueAttributeNames, out var alternatives))
									{
										slot = convertAttributesFromString(alternatives);
										SetSlot<object[]>(e.Row, _KeyValueAttributeSlotPosition, slot, true);
									}
								}
								if (slot == null)
								{
									slot = new object[kvlength];
									slot[idx] = e.NewValue;
									SetSlot<object[]>(e.Row, _KeyValueAttributeSlotPosition, slot);
								}
								else
								{
									slot[idx] = e.NewValue;
								}
							}
                        };
					FieldSelectingEvents[fieldName] +=
						(PXCache c, PXFieldSelectingEventArgs e) =>
						{
							if (e.Row != null)
							{
								object[] slot = GetSlot<object[]>(e.Row, _KeyValueAttributeSlotPosition);
								if (slot == null && GetStatus(e.Row) != PXEntryStatus.Inserted)
								{
									OnCommandPreparing(_NoteIDName, e.Row, null, PXDBOperation.Select, null, out var description);
									if (OnDemandCommand.GetKeyValues(this, e.Row, description.BqlTable, _KeyValueAttributeNames, out var alternatives))
									{
										slot = convertAttributesFromString(alternatives);
										SetSlot<object[]>(e.Row, _KeyValueAttributeSlotPosition, slot, true);
									}
								}
								if (slot != null)
								{
									e.ReturnValue = slot[idx];
								}
							}
							kvattributes[idx].FieldSelecting?.Invoke(c, e);
						};
					FieldVerifyingEvents[fieldName] +=
						(PXCache c, PXFieldVerifyingEventArgs e) =>
						{
							kvattributes[idx].FieldVerifying?.Invoke(c, e);
						};
					ExceptionHandlingEvents[fieldName] +=
						(PXCache c, PXExceptionHandlingEventArgs e) =>
						{
							kvattributes[idx].ExceptionHandling?.Invoke(c, e);
						};
					CommandPreparingEvents[fieldName] +=
						(PXCache c, PXCommandPreparingEventArgs e) =>
						{
							kvattributes[idx].CommandPreparing?.Invoke(c, e);
						};
				}
			}
        }

        private void FillEventsRowAttr(AlteredDescriptor altered)
		{
			Dictionary<object, object> clones;
			Dictionary<string, EventsFieldMap> eventsFieldMap;
			List<IPXRowSelectingSubscriber> mapRowSelecting;
			List<IPXRowSelectedSubscriber> mapRowSelected;
			List<IPXRowInsertingSubscriber> mapRowInserting;
			List<IPXRowInsertedSubscriber> mapRowInserted;
			List<IPXRowUpdatingSubscriber> mapRowUpdating;
			List<IPXRowUpdatedSubscriber> mapRowUpdated;
			List<IPXRowDeletingSubscriber> mapRowDeleting;
			List<IPXRowDeletedSubscriber> mapRowDeleted;
			List<IPXRowPersistingSubscriber> mapRowPersisting;
			List<IPXRowPersistedSubscriber> mapRowPersisted;
			if (altered != null)
			{
				clones = new Dictionary<object, object>(altered._FieldAttributes.Count);
				_AttributesFirst = altered._FieldAttributesFirst;
				_AttributesLast = altered._FieldAttributesLast;
				_FirstKeyValueStored = altered._FirstKeyValueStored;
				_KeyValueStoredOrdinals = altered._KeyValueStoredOrdinals;
				_CacheAttributes = new List<PXEventSubscriberAttribute>(altered._FieldAttributes.Count);
				foreach (PXEventSubscriberAttribute descr in altered._FieldAttributes)
				{
					PXEventSubscriberAttribute attr = descr.Clone(PXAttributeLevel.Cache);
					_CacheAttributes.Add(attr);
					AddAggregatedAttributes(ref clones, descr, attr);
				}
				eventsFieldMap = altered._EventsFieldMap;
				mapRowSelecting = altered._EventsRowMap.RowSelecting;
				mapRowSelected = altered._EventsRowMap.RowSelected;
				mapRowInserting = altered._EventsRowMap.RowInserting;
				mapRowInserted = altered._EventsRowMap.RowInserted;
				mapRowUpdating = altered._EventsRowMap.RowUpdating;
				mapRowUpdated = altered._EventsRowMap.RowUpdated;
				mapRowDeleting = altered._EventsRowMap.RowDeleting;
				mapRowDeleted = altered._EventsRowMap.RowDeleted;
				mapRowPersisting = altered._EventsRowMap.RowPersisting;
				mapRowPersisted = altered._EventsRowMap.RowPersisted;
			}
			else
			{
				clones = new Dictionary<object, object>(_FieldAttributes.Count);
				_AttributesFirst = _FieldAttributesFirst;
				_AttributesLast = _FieldAttributesLast;
				_CacheAttributes = new List<PXEventSubscriberAttribute>(_FieldAttributes.Count);
				foreach (PXEventSubscriberAttribute descr in _FieldAttributes)
				{
					PXEventSubscriberAttribute attr = descr.Clone(PXAttributeLevel.Cache);
					_CacheAttributes.Add(attr);
					AddAggregatedAttributes(ref clones, descr, attr);
				}
				eventsFieldMap = _EventsFieldMap;
				mapRowSelecting = _EventsRowMap.RowSelecting;
				mapRowSelected = _EventsRowMap.RowSelected;
				mapRowInserting = _EventsRowMap.RowInserting;
				mapRowInserted = _EventsRowMap.RowInserted;
				mapRowUpdating = _EventsRowMap.RowUpdating;
				mapRowUpdated = _EventsRowMap.RowUpdated;
				mapRowDeleting = _EventsRowMap.RowDeleting;
				mapRowDeleted = _EventsRowMap.RowDeleted;
				mapRowPersisting = _EventsRowMap.RowPersisting;
				mapRowPersisted = _EventsRowMap.RowPersisted;
			}

			foreach (KeyValuePair<string, EventsFieldMap> map in eventsFieldMap)
			{
				if (map.Value.CommandPreparing.Count > 0)
				{
					IPXCommandPreparingSubscriber[] arr = new IPXCommandPreparingSubscriber[map.Value.CommandPreparing.Count];
					for (int i = 0; i < arr.Length; i++)
					{
							arr[i] = (IPXCommandPreparingSubscriber)clones[map.Value.CommandPreparing[i]];
					}
					_CommandPreparingEventsAttr[map.Key] = arr;
				}
				if (map.Value.FieldDefaulting.Count > 0)
				{
					IPXFieldDefaultingSubscriber[] arr = new IPXFieldDefaultingSubscriber[map.Value.FieldDefaulting.Count];
					for (int i = 0; i < arr.Length; i++)
					{
							arr[i] = (IPXFieldDefaultingSubscriber)clones[map.Value.FieldDefaulting[i]];
					}
					_FieldDefaultingEventsAttr[map.Key] = arr;
				}
				if (map.Value.FieldUpdating.Count > 0)
				{
					IPXFieldUpdatingSubscriber[] arr = new IPXFieldUpdatingSubscriber[map.Value.FieldUpdating.Count];
					for (int i = 0; i < arr.Length; i++)
					{
							arr[i] = (IPXFieldUpdatingSubscriber)clones[map.Value.FieldUpdating[i]];
					}
					_FieldUpdatingEventsAttr[map.Key] = arr;
				}
				if (map.Value.FieldVerifying.Count > 0)
				{
					IPXFieldVerifyingSubscriber[] arr = new IPXFieldVerifyingSubscriber[map.Value.FieldVerifying.Count];
					for (int i = 0; i < arr.Length; i++)
					{
							arr[i] = (IPXFieldVerifyingSubscriber)clones[map.Value.FieldVerifying[i]];
					}
					_FieldVerifyingEventsAttr[map.Key] = arr;
				}
				if (map.Value.FieldUpdated.Count > 0)
				{
					IPXFieldUpdatedSubscriber[] arr = new IPXFieldUpdatedSubscriber[map.Value.FieldUpdated.Count];
					for (int i = 0; i < arr.Length; i++)
					{
							arr[i] = (IPXFieldUpdatedSubscriber)clones[map.Value.FieldUpdated[i]];
					}
					_FieldUpdatedEventsAttr[map.Key] = arr;
				}
				if (map.Value.FieldSelecting.Count > 0)
				{
					IPXFieldSelectingSubscriber[] arr = new IPXFieldSelectingSubscriber[map.Value.FieldSelecting.Count];
					for (int i = 0; i < arr.Length; i++)
					{
							arr[i] = (IPXFieldSelectingSubscriber)clones[map.Value.FieldSelecting[i]];
					}
					_FieldSelectingEventsAttr[map.Key] = arr;
				}
				if (map.Value.ExceptionHandling.Count > 0)
				{
					IPXExceptionHandlingSubscriber[] arr = new IPXExceptionHandlingSubscriber[map.Value.ExceptionHandling.Count];
					for (int i = 0; i < arr.Length; i++)
					{
							arr[i] = (IPXExceptionHandlingSubscriber)clones[map.Value.ExceptionHandling[i]];
					}
					_ExceptionHandlingEventsAttr[map.Key] = arr;
				}
			}

			if (mapRowSelecting.Count > 0)
			{
				IPXRowSelectingSubscriber[] arr = new IPXRowSelectingSubscriber[mapRowSelecting.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowSelectingSubscriber)clones[mapRowSelecting[i]];
				}
				_EventsRowAttr.RowSelecting = arr;
			}
			if (mapRowSelected.Count > 0)
			{
				IPXRowSelectedSubscriber[] arr = new IPXRowSelectedSubscriber[mapRowSelected.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowSelectedSubscriber)clones[mapRowSelected[i]];
				}
				_EventsRowAttr.RowSelected = arr;
			}
			if (mapRowInserting.Count > 0)
			{
				IPXRowInsertingSubscriber[] arr = new IPXRowInsertingSubscriber[mapRowInserting.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowInsertingSubscriber)clones[mapRowInserting[i]];
				}
				_EventsRowAttr.RowInserting = arr;
			}
			if (mapRowInserted.Count > 0)
			{
				IPXRowInsertedSubscriber[] arr = new IPXRowInsertedSubscriber[mapRowInserted.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowInsertedSubscriber)clones[mapRowInserted[i]];
				}
				_EventsRowAttr.RowInserted = arr;
			}
			if (mapRowUpdating.Count > 0)
			{
				IPXRowUpdatingSubscriber[] arr = new IPXRowUpdatingSubscriber[mapRowUpdating.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowUpdatingSubscriber)clones[mapRowUpdating[i]];
				}
				_EventsRowAttr.RowUpdating = arr;
			}
			if (mapRowUpdated.Count > 0)
			{
				IPXRowUpdatedSubscriber[] arr = new IPXRowUpdatedSubscriber[mapRowUpdated.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowUpdatedSubscriber)clones[mapRowUpdated[i]];
				}
				_EventsRowAttr.RowUpdated = arr;
			}
			if (mapRowDeleting.Count > 0)
			{
				IPXRowDeletingSubscriber[] arr = new IPXRowDeletingSubscriber[mapRowDeleting.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowDeletingSubscriber)clones[mapRowDeleting[i]];
				}
				_EventsRowAttr.RowDeleting = arr;
			}
			if (mapRowDeleted.Count > 0)
			{
				IPXRowDeletedSubscriber[] arr = new IPXRowDeletedSubscriber[mapRowDeleted.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowDeletedSubscriber)clones[mapRowDeleted[i]];
				}
				_EventsRowAttr.RowDeleted = arr;
			}
			if (mapRowPersisting.Count > 0)
			{
				IPXRowPersistingSubscriber[] arr = new IPXRowPersistingSubscriber[mapRowPersisting.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowPersistingSubscriber)clones[mapRowPersisting[i]];
				}
				_EventsRowAttr.RowPersisting = arr;
			}
			if (mapRowPersisted.Count > 0)
			{
				IPXRowPersistedSubscriber[] arr = new IPXRowPersistedSubscriber[mapRowPersisted.Count];
				for (int i = 0; i < arr.Length; i++)
				{
						arr[i] = (IPXRowPersistedSubscriber)clones[mapRowPersisted[i]];
				}
				_EventsRowAttr.RowPersisted = arr;
			}
		}

		#endregion

		#region Data manipulation methods
		//protected static string _TableName;
		//public override string TableName
		//{
		//    get
		//    {
		//        return _TableName;
		//    }
		//    set
		//    {
		//        _TableName = value;
		//    }
		//}
		/// <summary>Creates a data record from the <tt>PXDataRecord</tt> object
		/// and places it into the cache with the <tt>NotChanged</tt> status if
		/// the data record isn't found among the modified data records in the
		/// cache.</summary>
		/// <remarks>
		/// <para>If <tt>isReadOnly</tt> is <tt>false</tt> then:</para>
		/// <list type="bullet">
		/// <item><description>If the cache already contains
		/// the data record with the same keys and the <tt>NotChanged</tt> status,
		/// the method returns this data record updated to the state of
		/// <tt>PXDataRecord</tt>.</description></item>
		/// <item><description>If the
		/// cache contains the same data record with the <tt>Updated</tt> or
		/// <tt>Inserted</tt> status, the method returns this data
		/// record.</description></item>
		/// </list>
		/// <para>In other cases and when <tt>isReadonly</tt> is <tt>true</tt>,
		/// the method returns the data record created from the
		/// <tt>PXDataRecord</tt> object.</para>
		/// <para>If the <tt>AllowSelect</tt> property is <tt>false</tt>, the
		/// methods returns a new empty data record and the logic described above
		/// is not executed.</para>
		/// <para>The method raises the <tt>RowSelecting</tt> event.</para>
		/// </remarks>
		/// <param name="record">The <tt>PXDataRecord</tt> object to convert to
		/// the DAC type of the cache.</param>
		/// <param name="position">The index of the first field to read in
		/// the list of columns comprising the <tt>PXDataRecord</tt>
		/// object.</param>
		/// <param name="isReadOnly">The value indicating if the data record with
		/// the same key fields should be located in the cache and
		/// updated.</param>
		/// <param name="bool">The value indicating whether the data record
		/// with the same keys existed in the cache among the modified data
		/// records.</param>
		public override object Select(PXDataRecord record, ref int position, bool isReadOnly, out bool wasUpdated)
		{
			wasUpdated = false;
			TNode item = new TNode();
			OnRowSelecting(item, record, ref position, isReadOnly || (_Interceptor != null && !_Interceptor.CacheSelected));
			//if (!isKeysFilled(item))
			//{
			//    return null;
			//}
			if (!isReadOnly && (_Interceptor == null || _Interceptor.CacheSelected))
			{
				if (_ItemsDenormalized)
				{
					_Items.Normalize(null);
					_ItemsDenormalized = false;
				}
				object placed;
				if (_ChangedKeys == null || !_ChangedKeys.ContainsKey(item))
				{
					placed = _Items.PlaceNotChanged(item, true, out wasUpdated);
				}
				else
				{
					placed = _Items.PlaceNotChanged(_ChangedKeys[item], true, out wasUpdated);
				}
				if (_CurrentPlacedIntoCache != null && !wasUpdated && object.ReferenceEquals(placed, _CurrentPlacedIntoCache))
				{
					bool restore = true;
					if (_TimestampOrdinal != null)
					{
						byte[] oldstamp = GetValue(placed, _TimestampOrdinal.Value) as byte[];
						byte[] newstamp = GetValue(item, _TimestampOrdinal.Value) as byte[];
						if (newstamp != null)
						{
							if (oldstamp != null)
							{
								restore = false;
								for (int m = 0; m < oldstamp.Length && m < newstamp.Length; m++)
								{
									if (oldstamp[m] != newstamp[m])
									{
										if (oldstamp[m] < newstamp[m])
										{
											restore = true;
										}
										break;
									}
								}
							}
						}
						else
						{
							restore = false;
						}
					}
					if (restore)
					{
						RestoreCopy(placed, item);
						_CurrentPlacedIntoCache = null;
					}
				}
				return placed;
			}
			else
			{
				return item;
			}
		}

		internal override object CreateItem(PXDataRecord record, ref int position, bool isReadOnly)
		{
			TNode item = new TNode();
			OnRowSelecting(item, record, ref position, isReadOnly || (_Interceptor != null && !_Interceptor.CacheSelected));

			if (!isReadOnly && (_Interceptor == null || _Interceptor.CacheSelected))
			{
				PXEntryStatus status;
				if (_ChangedKeys == null || !_ChangedKeys.ContainsKey(item))
				{
					status = _Items.GetStatus(item);
				}
				else
				{
					status = _Items.GetStatus(_ChangedKeys[item]);
				}
				if (status == PXEntryStatus.Deleted || status == PXEntryStatus.InsertedDeleted)
					return null;
			}

			return item;
		}

		internal override object Select(object record, bool isReadOnly, out bool wasUpdated)
		{
			wasUpdated = false;
			TNode item = (TNode)record;
			if (!isReadOnly && (_Interceptor == null || _Interceptor.CacheSelected))
			{
				if (_ItemsDenormalized)
				{
					_Items.Normalize(null);
					_ItemsDenormalized = false;
				}
				object placed;
				if (_ChangedKeys == null || !_ChangedKeys.ContainsKey(item))
				{
					placed = _Items.PlaceNotChanged(item, out wasUpdated);
				}
				else
				{
					placed = _Items.PlaceNotChanged(_ChangedKeys[item], out wasUpdated);
				}
				if (_CurrentPlacedIntoCache != null && !wasUpdated && object.ReferenceEquals(placed, _CurrentPlacedIntoCache))
				{
					bool restore = true;
					if (_TimestampOrdinal != null)
					{
						byte[] oldstamp = GetValue(placed, _TimestampOrdinal.Value) as byte[];
						byte[] newstamp = GetValue(item, _TimestampOrdinal.Value) as byte[];
						if (newstamp != null)
						{
							if (oldstamp != null)
							{
								restore = false;
								for (int m = 0; m < oldstamp.Length && m < newstamp.Length; m++)
								{
									if (oldstamp[m] != newstamp[m])
									{
										if (oldstamp[m] < newstamp[m])
										{
											restore = true;
										}
										break;
									}
								}
							}
						}
						else
						{
							restore = false;
						}
					}
					if (restore)
					{
						RestoreCopy(placed, item);
						_CurrentPlacedIntoCache = null;
					}
				}
				return placed;
			}
			else
			{
				return item;
			}
		}

		/// <summary>Returns the provided data record status. The <see cref="PXEntryStatus">PXEntryStatus</see> enumeration defines the possible status values. For example, the status can indicate
		/// whether the data record has been inserted, updated, or deleted.</summary>
		/// <param name="item">The data record whose status is requested.</param>
		/// <example>
		/// 	<code title="Example" description="The code snippet below shows how to check a data record status in an event handler." lang="CS">
		/// protected virtual void Vendor_RowSelected(PXCache sender,
		///                                           PXRowSelectedEventArgs e)
		/// {
		///     Vendor vend = e.Row as Vendor;
		///     if (vend != null &amp;&amp; sender.GetStatus(vend) == PXEntryStatus.Notchanged)
		///     {
		///         ...
		///     }
		/// }</code>
		/// </example>
		public override PXEntryStatus GetStatus(object item)
		{
			return _Items.GetStatus((TNode)item);
		}
		/// <summary>Sets the status to the provided data record. The <see
		/// cref="PXEntryStatus">PXEntryStatus</see> enumeration
		/// defines the possible status values.</summary>
		/// <param name="item">The data record to set status to.</param>
		/// <param name="status">The new status.</param>
		/// <example>
		/// The code below checks the status of a data record and sets the status
		/// to <tt>Updated</tt> if the status is <tt>Notchanged</tt>.
		/// <code>
		/// if (Transactions.Cache.GetStatus(tran) == PXEntryStatus.Notchanged)
		/// {
		///     Transactions.Cache.SetStatus(tran, PXEntryStatus.Updated);
		/// }</code>
		/// </example>
		public override void SetStatus(object item, PXEntryStatus status)
		{
			if (item != null)
			{
				PXEntryStatus? existing = _Items.SetStatus((TNode)item, status);
				if (status == PXEntryStatus.Updated)
				{
					_FetchOriginals((TNode)item, (TNode)item, existing == PXEntryStatus.Notchanged);
				}
			}
		}
		/// <summary>Searches the cache for a data record that has the same key
		/// fields as the provided data record. If the data record is not found in
		/// the cache, the method retrieves the data record from the database and
		/// places it into the cache with the <tt>NotChanged</tt> status. The
		/// method returns the located or retrieved data record.</summary>
		/// <remarks>The <tt>AllowSelect</tt> property does not affect this method
		/// unlike the <see
		/// cref="PXCache{T}.Locate(IDictionary)">Locate(IDictionary)</see>
		/// method.</remarks>
		/// <param name="item">The data record to locate in the cache.</param>
		/// <example>
		/// <code>
		/// public PXSelectJoin&lt;SOAdjust,
		///     InnerJoin&lt;ARPayment, On&lt;ARPayment.docType, Equal&lt;SOAdjust.adjgDocType&gt;,
		///         And&lt;ARPayment.refNbr, Equal&lt;SOAdjust.adjgRefNbr&gt;&gt;&gt;&gt;&gt; Adjustments;
		/// ...
		/// // The optional delegate of the Adjustment data view to replace the
		/// // output of the Select() method
		/// public virtual IEnumerable adjustments()
		/// {
		///     ...
		///     SOAdjust adj = new SOAdjust();
		/// 
		///     // Setting the key fields
		///     adj.CustomerID = Document.Current.CustomerID;
		///     adj.AdjdOrderType = Document.Current.OrderType;
		///     adj.AdjdOrderNbr = Document.Current.OrderNbr;
		///     adj.AdjgDocType = payment.DocType;
		///     adj.AdjgRefNbr = payment.RefNbr;
		/// 
		///     // Searching the cache for the Adjustment data record with
		///     // the same key fields
		///     if (Adjustments.Cache.Locate(adj) == null)
		///     {
		///         yield return new PXResult&lt;SOAdjust, ARPayment&gt;(Adjustments.Insert(adj), payment);
		///     }
		///     ...
		/// }
		/// </code>
		/// </example>
		public override object Locate(object item)
		{
			return _Items.Locate((TNode)item);
		}
		protected internal override bool IsPresent(object item)
		{
			TNode placed;
			return (IsKeysFilled((TNode)item) && (placed = _Items.Locate((TNode)item)) != null
				&& (_CurrentPlacedIntoCache == null || !object.ReferenceEquals(placed, _CurrentPlacedIntoCache)));
		}
		protected internal override bool IsGraphSpecificField(string fieldName)
		{
			return _GraphSpecificFields != null && fieldName != null && _GraphSpecificFields.Contains(fieldName);
		}
		/// <summary>Searches the cache for a data record that has the same key
		/// fields as in the provided dictionary. If the data record is not found
		/// in the cache, the method initializes a new data record with the
		/// provided values and places it into the cache with the
		/// <tt>NotChanged</tt> status.</summary>
		/// <remarks>Returns 1 if a data record is successfully located or placed
		/// into the cache, and returns 0 if placing into the cache fails or the
		/// <tt>AllowSelect</tt> property is <tt>false</tt>.</remarks>
		/// <param name="keys">The dictionary with values to initialize the data
		/// record fields. The dictionary keys are field names.</param>
		public override int Locate(IDictionary keys)
		{
			if (!_AllowSelect)
			{
				return 0;
			}

			TNode item = (TNode)new TNode();
			FillWithValues(item, null, keys, PXCacheOperation.Update);

            TNode placed = (TNode)Locate(item);

			if (placed == null)
			{
				if (readItem(item) != null)
				{
					bool wasUpdated;
					placed = _Items.PlaceNotChanged(item, out wasUpdated);
				}
			}

			if (placed == null)
			{
				return 0;
			}

			Current = placed;
			return 1;
		}

		protected bool? _NonDBTable;
		protected bool NonDBTable
		{
			get
			{
				if (_NonDBTable == null)
				{
                    _NonDBTable = (PXDatabase.Provider.SchemaCache.GetTableHeader(_BqlTable.Name) == null)                        && (BqlSelect == null || PXDatabase.Provider.SchemaCache.GetTableHeader(GetBqlTable(BqlSelect.GetTables().FirstOrDefault())?.Name) == null);
                }
                return _NonDBTable.Value;
			}
		}

        public override int LocateByNoteID(Guid noteId)
		{
			if (!_AllowSelect)
			{
				return 0;
			}

		    Guid currentGiud;
		    if (Current != null && Guid.TryParse(GetValueExt(Current, "NoteID").ToString(), out currentGiud))
		    {
		        if (currentGiud == noteId)
		        {
		            return 1;
		        }
		    }

		    foreach (var item in _Items)
			{

                Guid guid;
                // if (Current != null && Guid.TryParse(GetValueExt(item, "NoteID").ToString(), out guid)) WTF???
                if (Guid.TryParse(GetValueExt(item, "NoteID").ToString(), out guid))
                {
                    if (guid == noteId)
                    {
                        Current = item;
                        return 1;
                    }
                }
			}

			TNode i = (TNode)new TNode();
			FillWithValues(i, null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "NoteID", noteId.ToString() }
			}, PXCacheOperation.Update);

			TNode placed = readItemByNoteID(i);
			if (placed != null)
			{
				bool wasUpdated;
				placed = _Items.PlaceNotChanged(placed, out wasUpdated);
			}

			if (placed == null)
			{
				return 0;
			}

			Current = placed;
			return 1;
		}

        /// <summary>Completely removes provided data recors from the cache object without raising any events.</summary>
        /// <param name="item">The data record to remove from the cache.</param>
        /// <remarks>Please, keep in mind that this method will not remove any records from the database itself but only from the cache.</remarks>
        /// <example>
        /// 	<code title="Example" description="The code below locates a data record in the cache and, if the data record has not been changed, silently removes it from the cache. (The Held status indicates that a data record has not been changed but needs to the preserved in the session." lang="CS">
        /// // Searching the data record by its key fields in the cache
        /// object cached = sender.Locate(item);
        /// // Checking the status
        /// if (cached != null &amp;&amp; (sender.GetStatus(cached) == PXEntryStatus.Held || 
        ///                        sender.GetStatus(cached) == PXEntryStatus.Notchanged))
        /// {
        ///     // Removing without events
        ///     sender.Remove(cached);
        /// }</code>
        /// </example>
		public override void Remove(object item)
		{
			_Items.Remove((TNode)item);
		}

		/// <summary>
		/// Reads a row from the Database. Raises the <tt>OnRowSelecting</tt> event.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected virtual TNode readItem(TNode item)
		{
			return readItem(item, false);
		}

		protected virtual TNode readItemByNoteID(TNode item)
		{
			return readItem(item, false, true);
		}

		protected virtual TNode readItem(TNode item, bool donotplace, bool byNoteID = false)
		{
			if (this.DisableReadItem)
				return IsKeysFilled(item) ? item : null;

			if (IsKeysFilled(item) || (byNoteID && GetValueExt(item, "NoteID") != null))
			{
				object idtval;
				if (!String.IsNullOrEmpty(Identity)
					&& (idtval = GetValue(item, Identity)) != null
					&& Convert.ToInt64(idtval) < 0L)
				{
					return null;
				}
				if (_Interceptor == null)
				{
					List<PXDataField> pars = new List<PXDataField>();
					foreach (string descr in _ClassFields)
					{
						object val = null;
						var isKeyField = _Keys.Contains(descr);
						bool isNoteID = string.Equals(descr, "NoteID", StringComparison.OrdinalIgnoreCase);
						if (isKeyField || (isNoteID && byNoteID))
						{
							val = GetValue(item, descr);
						}
						if (_KeyValueStoredNames != null || _KeyValueAttributeNames == null)
						{
							isNoteID = false;
						}
						PXCommandPreparingEventArgs.FieldDescription description = null;
						OnCommandPreparing(descr, item, val, PXDBOperation.Select | PXDBOperation.Internal | PXDBOperation.ReadItem, null, out description);

						if (description == null)
							continue;
						
						if (isKeyField)
						{
							if (!byNoteID && description.DataValue == null)
								return null;
						}
						else if (description.IsRestriction)
						{
							val = GetValue(item, descr);
							OnCommandPreparing(descr, item, val, PXDBOperation.Select | PXDBOperation.Internal | PXDBOperation.ReadItem, null, out description);
							//only keys should be skipped as fields marked with IsRestriction are not present in the keys collection in Update(IDictionary, IDictionary)
						}
						if (description != null && description.Expr != null)
						{
							pars.Add(new PXDataField(description.Expr));
							
							if (val != null)
							{
								// HACK to generate right comparison for NoteID attribute
								var fieldExpr = description.Expr.Duplicate();
								var noteIdExprs = fieldExpr.GetExpressionsOfType<NoteIdExpression>();
								foreach (var noteIdExpr in noteIdExprs)
								{
									noteIdExpr.IgnoreNulls = true;
								}

								pars.Add(new PXDataFieldValue(fieldExpr, description.DataType, description.DataLength, description.DataValue));
							}
							if (isNoteID)
							{
								if (prepareKvExtField(_FirstKeyValueAttribute.Value.Key, null, description.BqlTable, PXDBOperation.Select, ref description))
								{
									pars.Add(new PXDataField(description.Expr));
								}
							}
						}
					}
					using (PXDataRecord record = _Graph.ProviderSelectSingle(_BqlTable, pars.ToArray()))
					{
						if (record != null)
						{
							TNode output = new TNode();
							int position = 0;
							OnRowSelecting(output, record, ref position, donotplace);
							if (donotplace)
							{
								return output;
							}
							bool wasUpdated;
							return _Items.PlaceNotChanged(output, out wasUpdated);
						}
					}
				}
				else
				{
                    BqlCommand command;
					List<PXDataValue> pars = new List<PXDataValue>();
                    if (byNoteID)
                    {
				        if (_Interceptor.CanSelectByNoteId)
				        {
				            command = _Interceptor.GetRowByNoteIdCommand();
				            var noteidfield = "noteid";
				            PXCommandPreparingEventArgs.FieldDescription description = null;
				            OnCommandPreparing(noteidfield, item, GetValue(item, noteidfield),
				                PXDBOperation.Select | PXDBOperation.Internal | PXDBOperation.ReadItem,
				                null, out description);
				            pars.Add(new PXDataValue(description.DataType, description.DataValue));
				        }
				        else
				        {
				            return null;
				        }
                    }
				    else
				    {
                        foreach (string descr in _ClassFields)
                        {
                            if (_Keys.Contains(descr))
                            {
                                PXCommandPreparingEventArgs.FieldDescription description = null;
                                OnCommandPreparing(descr, item, GetValue(item, descr), PXDBOperation.Select | PXDBOperation.Internal | PXDBOperation.ReadItem,
                                    null, out description);
								if (description != null && description.DataValue == null)
									return null;
                                pars.Add(new PXDataValue(description.DataType, description.DataLength, description.DataValue));
                            }
                        }
                        command = _Interceptor.GetRowCommand();
                    }
                    IEnumerable<PXDataRecord> records = _Graph.ProviderSelect(command, 1, pars.ToArray());
					foreach (PXDataRecord record in records)
					{
						TNode output = new TNode();
						int position = 0;
						OnRowSelecting(output, record, ref position, donotplace);
						if (donotplace)
						{
							return output;
						}
						bool wasUpdated;
						return _Items.PlaceNotChanged(output, out wasUpdated);
					}
				}
			}
			return null;
		}

	    /// <summary>Updates the data record in the cache with the provided
	    /// values.</summary>
	    /// <remarks>
	    /// <para>The method initalizes a data record with the provided key
	    /// fields. If the data record with such keys does not exist in the cache,
	    /// the method tries to retrieve it from the database. If the data record
	    /// exists in the cache or database, it gets the <tt>Updated</tt> status.
	    /// If the data record does not exist in the database, the method inserts
	    /// a new data record into the cache with the <tt>Inserted</tt>
	    /// status.</para>
	    /// <para>The method raises the following events: <tt>FieldUpdating</tt>,
	    /// <tt>FieldVerifying</tt>, <tt>FieldUpdated</tt>, <tt>RowUpdating</tt>,
	    /// and <tt>RowUpdated</tt>. See <a href="Update.html">Updating a Data Record</a> for
	    /// the events flowchart. If the data record does not exist in the
	    /// database, the method also causes the events of the <see
	    /// cref="PXCache{T}.Insert(object)">Insert(object)</see>
	    /// method.</para>
	    /// <para>If the <tt>AllowUpdate</tt> property is <tt>false</tt>, the data
	    /// record is not updated and the methods returns 0. The method returns 1
	    /// if the data record is successfully updated or inserted.</para>
	    /// </remarks>
	    /// <param name="keys">The values of the key fields of the data record to
	    /// update.</param>
	    /// <param name="values">The new values with which the data record fields
	    /// are updated.</param>
	    public override int Update(IDictionary keys, IDictionary values)
	    {
	        if (!_AllowUpdate)
			{
				return 0;
			}

	        using (new PXPerformanceInfoTimerScope(info => info.TmUpdate))
	        {
	            TNode item = (TNode) new TNode();
	            FillWithValues(item, null, keys, PXCacheOperation.Update);
	            if (Graph.IsContractBasedAPI)
	            {
	                object[] ks = keys.Keys.ToArray<Object>();
	                foreach (string key in ks)
	                {
	                    if (key.EndsWith(_OriginalValue, StringComparison.OrdinalIgnoreCase))
	                    {
	                        keys.Remove(key);
	                    }
	                }
	            }

	            if (_ChangedKeys != null && _ChangedKeys.ContainsKey(item))
	            {
	                throw new PXBadDictinaryException();
	            }

	            TNode placed = _Items.PlaceUpdated(item, false);

	            BqlTablePair orig = null;

	            if (placed == null && Keys.Count == 0)
	            {
	                placed = Current as TNode;
	            }

			if (placed == null)
			{
				TNode output;
				if ((output = readItem(item)) != null)
				{
					placed = _Items.PlaceUpdated(item, true);
					try
					{
						if (placed != null)
						{
							lock (((ICollection)_Originals).SyncRoot)
							{
								List<object> slots = null;
								List<object> slotsoriginal = null;
								if (_Originals.TryGetValue(placed, out orig))
								{
									slots = orig.Slots;
									slotsoriginal = orig.SlotsOriginal;
								}
								_Originals[placed] = orig = new BqlTablePair { Unchanged = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), Slots = slots, SlotsOriginal = slotsoriginal };
							}
						}
					}
					catch
					{
					}
				}
				else
				{
				    if (!_AllowInsert)
				        return 0;
					Dictionary<string, object> inserted = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					bool nokeys = true;
					if (keys != null)
					{
						foreach (DictionaryEntry entry in keys)
						{
                            var keyField = (string)entry.Key;
						    if (!Graph.IsImport || IsFieldEnabled(keyField))
						    {
						        inserted[keyField] = entry.Value;
						        if (entry.Value != null)
						        {
						            nokeys = false;
						        }
						    }
						}
					}
					if (values != null)
					{
						foreach (DictionaryEntry entry in values)
						{
							if (!inserted.ContainsKey((string)entry.Key) || entry.Value != NotSetValue)
							{
								inserted[(string)entry.Key] = entry.Value;
							}
						}
					}
					bool allowinsertswitched = false;
					bool isdirtystored = _IsDirty;
					int ret;
					if (nokeys && !_AllowInsert)
					{
						allowinsertswitched = true;
						_AllowInsert = true;
					}
					try
					{
						ret = Insert(inserted);
					}
					finally
					{
						if (allowinsertswitched)
						{
							_AllowInsert = false;
						}
					}
					if (allowinsertswitched && ret > 0)
					{
						nokeys = false;
						foreach (string key in Keys)
						{
							if (!inserted.ContainsKey(key) || PXFieldState.UnwrapValue(inserted[key]) == null)
							{
								nokeys = true;
							}
						}
						if (nokeys)
						{
							Delete(Current);
							_IsDirty = isdirtystored;
							return 0;
						}
					}
					if (values != null)
					{
						foreach (string key in values.Keys.ToArray<Object>())
						{
							if (ret > 0)
							{
								values[key] = inserted[key];
								if (inserted.ContainsKey(key + _OriginalValue))
								{
									values[key + _OriginalValue] = inserted[key + _OriginalValue];
								}
							}
							else
							{
								PXFieldState fs = inserted[key] as PXFieldState;
								if (fs != null && !String.IsNullOrEmpty(fs.Error))
								{
									values[key] = fs;
								}
								else if (key.EndsWith("_description", StringComparison.OrdinalIgnoreCase))
								{
									values[key] = inserted[key];
								}
							}
						}
					}
					if (keys != null)
					{
						object[] ks = keys.Keys.ToArray<Object>();
						if (Graph.IsContractBasedAPI && ks.Length == 0)
						{
							ks = Keys.ToArray();
						}
						foreach (string key in ks)
						{
						    if (ret > 0)
						    {
						        object val = inserted[key];
						        PXFieldState fs = val as PXFieldState;
						        if (fs != null)
						        {
						            val = fs.Value;
						        }
						        keys[key] = val;
						    }
						}
					}
					return ret;
				}
			}
			else if (!NonDBTable)
			{
				orig = _FetchOriginals(item, placed, true);
			}

	            if (placed == null)
	            {
	                return 0;
	            }

	            Current = placed;
	            TNode copy = CreateCopy(placed);
	            if (orig != null)
	            {
	                orig.LastModified = copy;
	            }
	            SetSessionUnmodified(placed, copy);

	            bool cancel = false;

	            bool normalized = false;
	            bool keysUpdated = false;
	            if (values != null)
	            {
	                if (!_AllowUpdate)
	                {
	                    return 0;
	                }
	                if (!DisableCloneAttributes)
	                    TryDispose(GetAttributes(placed, null));
	                try
	                {
	                    if (FillWithValues(placed, copy, values, PXCacheOperation.Update))
	                    {
	                        try
	                        {
	                            _Items.Normalize(placed);
	                            normalized = true;
	                        }
	                        catch (PXBadDictinaryException)
	                        {
	                            RestoreCopy(placed, copy);
	                            throw;
	                        }
	                        keysUpdated = true;
	                    }
	                }
	                catch (Exception)
	                {
	                    cancel = true;
	                    throw;
	                }
	                finally
	                {
	                    if (cancel)
	                    {
	                        RestoreCopy(placed, copy);
	                    }
	                }
	            }

	            try
	            {
	                cancel = !OnRowUpdating(copy, placed, true);
	            }
	            catch (Exception)
	            {
	                cancel = true;
	                throw;
	            }
	            finally
	            {
	                if (cancel)
	                {
	                    RestoreCopy(placed, copy);
	                    if (normalized)
	                    {
	                        _Items.Normalize(null);
	                    }
	                }
	            }
	            if (cancel)
	            {
	                if (values != null)
	                {
	                    foreach (string key in values.Keys.ToArray<Object>())
	                    {
	                        if (Fields.Contains(key))
	                        {
	                            values[key] = GetValueExt(placed, key);
	                        }
	                    }
	                }
	                if (keys != null)
	                {
	                    foreach (string key in keys.Keys.ToArray<Object>())
	                    {
	                        object val = GetValueExt(placed, key);
	                        PXFieldState fs = val as PXFieldState;
	                        if (fs != null)
	                        {
	                            val = fs.Value;
	                        }
	                        keys[key] = val;
	                    }
	                }
	                return 0;
	            }

	            _IsDirty = true;

	            Current = placed;

	            OnRowUpdated(placed, copy, true);

	            if (values != null)
	            {
	                foreach (string key in values.Keys.ToArray<Object>())
	                {
	                    if (Fields.Contains(key))
	                    {
	                        values[key] = GetValueExt(placed, key);
	                    }
	                }
	            }
	            if (keys != null)
	            {
	                object[] ks = keys.Keys.ToArray<Object>();
	                if (Graph.IsContractBasedAPI && ks.Length == 0)
	                {
	                    ks = Keys.ToArray();
	                }
	                foreach (string key in ks)
	                {
	                    object val = GetValueExt(placed, key);
	                    PXFieldState fs = val as PXFieldState;
	                    if (fs != null)
	                    {
	                        val = fs.Value;
	                    }
	                    keys[key] = val;
	                }
	            }

	            if (orig != null)
	            {
	                try
	                {
	                    orig.LastModified = CreateCopy(placed);
	                    if (keysUpdated && orig.Unchanged is TNode && !object.ReferenceEquals(placed, orig.Unchanged) && !ObjectsEqual(placed, orig.Unchanged))
	                    {
	                        if (_ChangedKeys == null)
	                        {
	                            _ChangedKeys = new Dictionary<TNode, TNode>(new ItemComparer(this));
	                        }
	                        _ChangedKeys[(TNode)orig.Unchanged] = placed;
							ClearQueryCache();
	                    }
	                }
	                catch
	                {
	                }
	            }

	            return 1;
	        }
	    }

	    private bool IsFieldEnabled(string keyField)
	    {
            var state = GetStateExt(null, keyField) as PXFieldState;
	        return state?.Enabled ?? true;
	    }

	    internal bool PlaceNotChangedWithOriginals(TNode item)
	    {
	        if (this.Locate(item) != null)
	            return false;

	        this.PlaceNotChanged(item);

	        lock (((ICollection) _Originals).SyncRoot)
	        {
	            BqlTablePair pair;
	            if (!_Originals.TryGetValue(item, out pair))
	            {
	                _Originals[item] = new BqlTablePair { Unchanged = CreateCopy(item, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), LastModified = CreateCopy(item, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal) };
	            }
	            else
	            {
	                pair.Unchanged = CreateCopy(item, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);
	                pair.LastModified = CreateCopy(item, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);
	            }
            }

	        return true;

	    }

	    private BqlTablePair _FetchOriginals(TNode item, TNode placed, bool ahead)
		{
			BqlTablePair orig = null;
			try
			{
				bool found;
				TNode output = null;
				lock (((ICollection)_Originals).SyncRoot)
				{
					found = _Originals.TryGetValue(placed, out orig) && orig.Unchanged != null;
				}
				if (!found)
				{
					output = readItem(item, true);
				}
				if (!found)
				{
					lock (((ICollection)_Originals).SyncRoot)
					{
						if (!_Originals.TryGetValue(placed, out orig) || orig.Unchanged == null)
						{
							if (ahead)
							{
								_OriginalsRequested++;
							}
							if (output != null)
							{
								if (object.ReferenceEquals(output, placed))
									output = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);

								if (orig == null)
								{
									_Originals[placed] = orig = new BqlTablePair { Unchanged = output };
								}
								else
								{
									orig.Unchanged = output;
								}
							}
							if (ahead && _OriginalsRequested >= 5)
							{
								_OriginalsRequested = 0;
								_OriginalsReadAhead++;
								int i = 0;
								foreach (TNode cached in _Items.NotChanged)
								{
									i++;
									if (_OriginalsReadAhead < 100 && i >= 50)
									{
										break;
									}
									BqlTablePair pair;
									if (!_Originals.TryGetValue(cached, out pair))
									{
										_Originals[cached] = new BqlTablePair { Unchanged = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), LastModified = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal) };
									}
									else
									{
										pair.Unchanged = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);
										pair.LastModified = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);
									}
								}
								if (_OriginalsReadAhead >= 100)
								{
									_OriginalsReadAhead = 0;
								}
							}
						}
					}
				}
			}
			catch
			{
			}
			return orig;
		}
		/// <summary>Updates the provided data record in the cache.</summary>
		/// <remarks>
		/// <para>If the data record does not exist in the cache, the method tries
		/// to retrieve it from the database. If the data record exists in the
		/// cache or database, it gets the <tt>Updated</tt> status. If the data
		/// record does not exist in the database, the method inserts a new data
		/// record into the cache with the <tt>Inserted</tt> status.</para>
		/// <para>The method raises the following events: <tt>FieldUpdating</tt>,
		/// <tt>FieldVerifying</tt>, <tt>FieldUpdated</tt>, <tt>RowUpdating</tt>,
		/// and <tt>RowUpdated</tt>. See <a href="Update.html">Updating a Data Record</a> for
		/// the events flowchart. If the data record does not exist in the
		/// database, the method also causes the events of the <see
		/// cref="PXCache{T}.Insert(object)">Insert(object)</see>
		/// method.</para>
		/// <para>The <tt>AllowUpdate</tt> property does not affect the method
		/// unlike the <see
		/// cref="PXCache{T}.Update(IDictionary,IDictionary)">Update(IDictionary,
		/// IDictionary)</see> method.</para>
		/// </remarks>
		/// <param name="data">The data record to update in the cache.</param>
		/// <example>
		/// The code below modifies an <tt>APRegister</tt> data record and places
		/// it in the cache with the <tt>Updated</tt> status or updates it in the
		/// cache if the data record is already there.
		/// <code>
		/// // Declaring a data view in a graph
		/// public PXSelect&lt;APRegister&gt; APDocument;
		/// ...
		/// 
		/// APRegister apdoc = ...
		/// // Modifying the data record
		/// apdoc.Voided = true;
		/// apdoc.OpenDoc = false;
		/// apdoc.CuryDocBal = 0m;
		/// apdoc.DocBal = 0m;
		/// 
		/// // Updating the data record in the cache
		/// APDocument.Cache.Update(apdoc);</code>
		/// </example>
		public override object Update(object data)
		{
			return Update(data, false);
		}

		protected internal override object Update(object data, bool bypassinterceptor)
		{
		    PXPerformanceInfoTimerScope performanceInfoTimerScope = null;
			bool checkpending = _PendingItems == null;
			try
			{
			    performanceInfoTimerScope = new PXPerformanceInfoTimerScope(info => info.TmUpdate);
				if (checkpending)
				{
					_PendingItems = new List<TNode>();
				}

				if (!bypassinterceptor && _Interceptor != null)
				{
					return _Interceptor.Update(this, data);
				}
				if (data is PXResult)
				{
					data = ((PXResult)data)[0];
				}
				TNode item = data as TNode;
				if (item == null)
				{
					return null;
				}

				TNode placed = _Items.PlaceUpdated(item, false);

				BqlTablePair orig = null;

				if (placed == null)
				{
					TNode output;
					if ((output = readItem(item, true)) != null)
					{
						placed = _Items.PlaceUpdated(item, true);
						try
						{
							if (placed != null)
							{
								lock (((ICollection)_Originals).SyncRoot)
								{
									List<object> slots = null;
									List<object> slotsoriginal = null;
									if (_Originals.TryGetValue(placed, out orig))
									{
										slots = orig.Slots;
										slotsoriginal = orig.SlotsOriginal;
									}
									_Originals[placed] = orig = new BqlTablePair { LastModified = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), Unchanged = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), Slots = slots, SlotsOriginal = slotsoriginal };
								}
							}
						}
						catch
						{
						}
					}
					else
					{
						return Insert(data);
					}
				}
				else if (!NonDBTable)
				{
					try
					{
						bool found;
						TNode output = null;
						lock (((ICollection)_Originals).SyncRoot)
						{
							found = _Originals.TryGetValue(placed, out orig) && orig.Unchanged != null;
						}
						if (!found)
						{
							output = readItem(item, true);
						}
						{
							lock (((ICollection)_Originals).SyncRoot)
							{
								if (!_Originals.TryGetValue(placed, out orig) || orig.Unchanged == null)
								{
									_OriginalsRequested++;
									if (output != null)
									{
										if (object.ReferenceEquals(output, placed))
											output = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);

										if (orig == null)
										{
											_Originals[placed] = orig = new BqlTablePair { LastModified = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), Unchanged = output };
										}
										else
										{
											if (orig.LastModified == null)
											{
												orig.LastModified = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);
											}
											orig.Unchanged = output;
										}
									}
									if (_OriginalsRequested >= 5)
									{
										_OriginalsRequested = 0;
										_OriginalsReadAhead++;
										int i = 0;
										foreach (TNode cached in _Items.NotChanged)
										{
											i++;
											if (_OriginalsReadAhead < 100 && i >= 50)
											{
												break;
											}
											BqlTablePair pair;
											if (!_Originals.TryGetValue(cached, out pair))
											{
												_Originals[cached] = new BqlTablePair { Unchanged = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), LastModified = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal) };
											}
											else
											{
												pair.Unchanged = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);
												pair.LastModified = CreateCopy(cached, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);
											}
										}
										if (_OriginalsReadAhead >= 100)
										{
											_OriginalsReadAhead = 0;
										}
									}
								}
								if (orig != null && orig.LastModified == null)
								{
									orig.LastModified = CreateCopy((TNode)orig.Unchanged, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal);
								}
							}
						}
					}
					catch
					{
					}
				}

				if (placed == null)
				{
					return null;
				}

				bool thesame = Object.ReferenceEquals(placed, item);
				bool keysUpdated = false;

				if (thesame)
				{
					if (orig != null && orig.LastModified is TNode && !Object.ReferenceEquals(orig.LastModified, item))
					{
						item = CreateCopy(item);
						TNode lastModified = (TNode)orig.LastModified;
						foreach (string key in Keys)
						{
							if (keysUpdated = keysUpdated || !object.Equals(GetValue(item, key), GetValue(lastModified, key)))
							{
								break;
							}
						}
						RestoreCopy(placed, orig.LastModified);
						Current = placed;
						if (keysUpdated)
						{
							Normalize();
						}
						thesame = false;
					}
				}

				TNode copy = CreateCopy(placed);

				if (ForceExceptionHandling && !DisableCloneAttributes)
					TryDispose(GetAttributes(placed, null));

				if (!thesame)
				{
					FillWithValues(placed, copy, item);
					if (keysUpdated)
					{
						Normalize();
					}
				}

				bool cancel = false;
				try
				{
					cancel = !OnRowUpdating(copy, placed, false);
				}
				catch (Exception)
				{
					cancel = true;
					throw;
				}
				finally
				{
					if (cancel)
					{
						RestoreCopy(placed, copy);
					}
				}

				if (cancel)
				{
					return null;
				}

				_IsDirty = true;

				Current = placed;

				OnRowUpdated(placed, thesame ? placed : copy, false);

				return placed;
			}
			finally
			{
			    performanceInfoTimerScope?.Dispose();

			    if (checkpending)
				{
					for (int i = 0; i < _PendingItems.Count; i++)
					{
						if (_Items.Locate(_PendingItems[i]) == null)
						{
							_ItemAttributes.Remove(_PendingItems[i]);
						}
					}
					_PendingItems = null;
				}
			}
		}

        protected internal override void UpdateLastModified(object item, bool inserted)
        {
            BqlTablePair orig = null;
            var row = item as TNode;
            if (row == null)
                return;
            TNode placed;
            placed = _Items.Locate(row);
            if (placed == null)
                return;
            lock (((ICollection)_Originals).SyncRoot)
            {
                if (inserted)
                {
					if (_Originals.TryGetValue(placed, out orig) && orig != null && orig.Slots != null)
					{
						List<object> slots = new List<object>(orig.Slots.Count);
						List<object> slotsoriginal = new List<object>(orig.Slots.Count);
						for (int l = 0; l < orig.Slots.Count; l++)
						{
							if (orig.Slots[l] != null && l < _SlotDelegates.Count)
							{
								slots.Add(((Func<object, object>)_SlotDelegates[l].Item3)(orig.Slots[l]));
							}
							else
							{
								slots.Add(null);
							}
							if (orig.SlotsOriginal[l] != null && l < _SlotDelegates.Count)
							{
								slotsoriginal.Add(((Func<object, object>)_SlotDelegates[l].Item3)(orig.SlotsOriginal[l]));
							}
							else
							{
								slotsoriginal.Add(null);
							}
						}
						_Originals[placed] = new BqlTablePair { LastModified = CreateCopy(placed, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), Slots = slots, SlotsOriginal = slotsoriginal, SessionUnmodified = orig.SessionUnmodified };
					}
					else
					{
						_Originals[placed] = new BqlTablePair { LastModified = CreateCopy(placed, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, SetValueByOrdinal), SessionUnmodified = orig?.SessionUnmodified };
					}
				}
                else
                {
                    if (_Originals.TryGetValue(placed, out orig) && orig != null)
                    {
                        orig.LastModified = CreateCopy(row);
                    }
                }
            }
            
        }

        protected internal override void SetSessionUnmodified(
            object item,
            object unmodified)
        {
            SetSessionUnmodified(item, unmodified, GetStatus(unmodified));
        }


        protected internal void SetSessionUnmodified(
            object item, 
            object unmodified,
            PXEntryStatus? status)
		{
			BqlTablePair orig = null;
			var row = item as TNode;
			if (row == null)
				return;
			TNode placed;
			placed = _Items.Locate(row);
			if (placed == null)
				return;
			lock (((ICollection)_Originals).SyncRoot)
			{
                if (!_Originals.TryGetValue(placed, out orig) || orig == null)
                    _Originals[placed] = orig = new BqlTablePair();
                if (orig.SessionUnmodified == null)
					orig.SessionUnmodified = unmodified != null 
                        ? new SessionUnmodifiedPair
                        {
                            Item = CreateCopy((TNode)unmodified),
                            Status = status
                        }
                        : new SessionUnmodifiedPair
                        {
                            Item = null,
                            Status = null
                        };

				}
			}

        /// <summary>Initializes a new data record using the provided field values and inserts the data record into the cache. Returns 1 in case of successful insertion, 0
        /// otherwise.</summary>
        /// <param name="values">The dictionary with values to initialize the data
        /// record fields. The dictionary keys are field names.</param>
        /// <remarks>
        /// 	<para>The method raises the following events:
        /// <tt>FieldDefaulting</tt>, <tt>FieldUpdating</tt>,
        /// <tt>FieldVerifying</tt>, <tt>FieldUpdated</tt>, <tt>RowInserting</tt>,
        /// and <tt>RowInserted</tt>. See <a href="Insert.html">Inserting a Data Record</a> for
        /// the events chart.</para>
        /// 	<para>The method does not check if the data record exists in the
        /// database. The values provided in the dictionary are not readonly and
        /// can be updated during execution of the method. The method is typically
        /// used by the system when the values are received from the user
        /// interface. If the <tt>AllowInsert</tt> property is <tt>false</tt>, the
        /// data record is not inserted and the method returns 0.</para>
        /// 	<para>In case of successful insertion, the method marks the data
        /// record as <tt>Inserted</tt>, and it becomes accessible through the
        /// <tt>Inserted</tt> collection.</para>
        /// </remarks>
		public override int Insert(IDictionary values)
		{
			if (!_AllowInsert)
			{
				return 0;
			}

		    using (new PXPerformanceInfoTimerScope(info => info.TmInsert))
		    {
		        TNode item = (TNode) new TNode();
		        if (!DisableCloneAttributes)
		            TryDispose(GetAttributes(item, null, true));
		        FillWithValues(item, null, values, PXCacheOperation.Insert);

		        if (_ChangedKeys != null && _ChangedKeys.ContainsKey(item))
		        {
		            throw new PXBadDictinaryException();
		        }

                var incoming = _Items.Locate(item);
                var incomingStatus = _Items.GetStatus(item);

		        string[] keys = new string[values.Keys.Count];
		        values.Keys.CopyTo(keys, 0);

		        if (!OnRowInserting(item, true))
		        {
		            foreach (string key in keys)
		            {
		                if (Fields.Contains(key))
		                {
		                    values[key] = GetValueExt(item, key);
		                }
		            }
		            return 0;
		        }

		        //readItem(item);

		        bool deleted;
		        item = _Items.PlaceInserted(item, out deleted);
		        if (deleted)
		        {
					ClearQueryCache();
		        }

		        if (item == null)
		        {
		            return 0;
		        }

		        _IsDirty = true;

		        Current = item;

                SetSessionUnmodified(item, incoming, incomingStatus);

		        OnRowInserted(item, true);

		        foreach (string key in keys)
		        {
		            if (Fields.Contains(key))
		            {
		                values[key] = GetValueExt(item, key);
		            }
		        }
		        foreach (string key in _Keys)
		        {
		            values[key] = GetValueExt(item, key);
		        }

		        return 1;
		    }
		}

		internal override object FillItem(IDictionary values)
		{
			TNode item = (TNode)new TNode();
			if (!DisableCloneAttributes)
                TryDispose(GetAttributes(item, null, true));
			
			FillWithValues(item, null, values, PXCacheOperation.Insert, false);
			return item;
		}

        /// <summary>Inserts the provided data record into the cache. Returns the
        /// inserted data record or <tt>null</tt> if the data record wasn't
        /// inserted.</summary>
        /// <param name="data">The data record to insert into the cache.</param>
        /// <remarks>
        /// 	<para>The method raises the following events:
        /// <tt>FieldDefaulting</tt>, <tt>FieldUpdating</tt>,
        /// <tt>FieldVerifying</tt>, <tt>FieldUpdated</tt>, <tt>RowInserting</tt>,
        /// and <tt>RowInserted</tt>. See <a href="Insert.html">Inserting a Data Record</a> for
        /// the events chart.</para>
        /// 	<para>The method does not check if the data record exists in the
        /// database. The AllowInsert property does not affect this method unlike
        /// the <see cref="PXCache{T}.Insert(IDictionary)">Insert(IDictionary)</see>
        /// method.</para>
        /// 	<para>In case of successful insertion, the method marks the data
        /// record as <tt>Inserted</tt>, and it becomes accessible through the
        /// <tt>Inserted</tt> collection.</para>
        /// </remarks>
        /// <example>
        /// 	<code title="Example" description="The code below initializes a new instance of the APInvoice data record and inserts it into the cache." lang="CS">
        /// APInvoice newDoc = new APInvoice();
        /// newDoc.VendorID = Document.Current.VendorID;
        /// Document.Insert(newDoc);</code>
        /// </example>
		public override object Insert(object data)
		{
			return Insert(data, false);
		}

		protected internal override object Insert(object data, bool bypassinterceptor)
		{
		    PXPerformanceInfoTimerScope performanceInfoTimerScope = null;
			bool checkpending = _PendingItems == null;
			try
			{
			    performanceInfoTimerScope = new PXPerformanceInfoTimerScope(info => info.TmInsert);
				if (checkpending)
				{
					_PendingItems = new List<TNode>();
				}

                if (_PendingExceptions == null)
                {
                    _PendingExceptions = new Dictionary<TNode, List<Exception>>();
                }

				if (!bypassinterceptor && _Interceptor != null)
				{
					return _Interceptor.Insert(this, data);
				}
				if (data is PXResult)
				{
					data = ((PXResult)data)[0];
				}
				TNode item = data as TNode;
				if (item == null)
				{
					return null;
				}

				TNode copy = new TNode();

				if (ForceExceptionHandling && !DisableCloneAttributes)
					TryDispose(GetAttributes(copy, null));

				FillWithValues(copy, ref item);

				if (!OnRowInserting(item, false))
				{
					return null;
				}

                List<Exception> pending;
                if (_PendingExceptions.TryGetValue(item, out pending) && pending.Count > 0)
                {
                    throw pending[0];
                }

				//readItem(item);

                var incoming = _Items.Locate(item);
                var incomingStatus = _Items.GetStatus(item);

				bool deleted;
				TNode placed = _Items.PlaceInserted(item, out deleted);
				if (deleted)
				{
					ClearQueryCache();
				}

				if (placed == null)
				{
					return null;
				}

				_IsDirty = true;

                SetSessionUnmodified(placed, incoming, incomingStatus);

				Current = placed;

				OnRowInserted(placed, data as TNode, false);

				return item;
			}
			finally
			{
			    performanceInfoTimerScope?.Dispose();
				if (checkpending)
				{
					if (_ItemAttributes != null)
					{
						for (int i = 0; i < _PendingItems.Count; i++)
						{
							if (_Items.Locate(_PendingItems[i]) == null)
							{
								_ItemAttributes.Remove(_PendingItems[i]);
							}
						}
					}
					_PendingItems = null;
				}
			}
		}

		private static CacheStaticInfo _GetInitializer()
		{
			return _Initialize(false);
		}

		internal override object ToChildEntity<Parent>(Parent item)
		{
			if (!typeof(TNode).IsSubclassOf(typeof(Parent)))
			{
				throw new PXArgumentException("Parent", ErrorMessages.ArgumentOutOfRangeException);
			}
			TNode data = new TNode();
			PXCacheExtension[] itemextension = null;
			if (PXCache<Parent>._GetInitializer()._CreateExtensions != null)
			{
				PXCacheExtensionCollection dict = _Extensions;
				if (dict == null)
				{
					dict = PXContext.GetSlot<PXCacheExtensionCollection>()
						?? PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}
				lock (((ICollection)dict).SyncRoot)
				{
					if (!dict.TryGetValue(item, out itemextension))
					{
						dict[item] = itemextension = PXCache<Parent>._GetInitializer()._CreateExtensions(item);
					}
				}
			}
			foreach (string name in PXCache<Parent>._GetInitializer()._ClassFields)
			{
				SetValue(data, name, PXCache<Parent>._GetInitializer()._GetValueByOrdinal(item, PXCache<Parent>._GetInitializer()._FieldsMap[name], itemextension));
			}

			return data;
		}

        /// <summary>Initializes a data record of the DAC type of the cache from
        /// the provided data record of the base DAC type and inserts the new data
        /// record into the cache. Returns the inserted data record.</summary>
        /// <param name="item">The data record of the base DAC type, the field values of which are used to initialize the data record.</param>
        /// <example>
        /// See the <see cref="PXSelectBase{T}.Extend{Parent}(Parent)">Extend&lt;Parent&gt;(Parent)</see>
        /// method of the <tt>PXSelectBase&lt;&gt;</tt> class.
        /// <code title="" description="" lang="neutral"></code></example>
		public override object Extend<Parent>(Parent item)
		{
            TNode data = (TNode)ToChildEntity(item);
			TNode loc = (TNode)Locate(data);
			if (loc != null && GetStatus(loc) != PXEntryStatus.Deleted && GetStatus(loc) != PXEntryStatus.InsertedDeleted)
			{
				if (_Extensions == null)
				{
					foreach (string name in _ClassFields)
					{
						if (!PXCache<Parent>._GetInitializer()._ClassFields.Contains(name))
						{
							int ordinal = _FieldsMap[name];
							SetValueByOrdinal(data, ordinal, GetValueByOrdinal(loc, ordinal, null), null);
						}
					}
				}
				else
				{
					PXCacheExtension[] dataextension;
					PXCacheExtension[] locextension;
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(data, out dataextension))
						{
							_Extensions[data] = dataextension = _CreateExtensions(data);
						}
						if (!_Extensions.TryGetValue(loc, out locextension))
						{
							_Extensions[loc] = locextension = _CreateExtensions(loc);
						}
					}
					foreach (string name in _ClassFields)
					{
						if (!PXCache<Parent>._GetInitializer()._ClassFields.Contains(name))
						{
							int ordinal = _FieldsMap[name];
							SetValueByOrdinal(data, ordinal, GetValueByOrdinal(loc, ordinal, locextension), dataextension);
						}
					}
				}
				return Update(data);
			}
			TNode ins = (TNode)Insert(data);
			if (ins != null)
			{
			    var parentOriginal = (Parent)this.Graph.Caches[typeof(Parent)].GetOriginal(item);
                var unchanged = parentOriginal.With(c => (TNode)ToChildEntity(c));
				if (!ObjectsEqual(ins, data))
				{
					TNode lastModified = null;
					lock (((ICollection)_Originals).SyncRoot)
					{
						BqlTablePair orig;
						if (_Originals.TryGetValue(ins, out orig))
						{
							lastModified = (TNode)orig.LastModified;
						    orig.Unchanged = unchanged;
						}
					}
					foreach (string key in Keys)
					{
						object val;
						SetValue(ins, key, val = GetValue(data, key));
						if (lastModified != null)
						{
							SetValue(lastModified, key, val);
						}
					}
					Normalize();
				}
				SetStatus(ins, PXEntryStatus.Updated);
			}
			return ins;
		}

		/// <summary>Initializes a new data record with default values and inserts
		/// it into the cache by invoking the <see
		/// cref="PXCache{T}.Insert(object)">Insert(object)</see>
		/// method. Returns the new data record inserted into the cache.</summary>
		public override object Insert()
		{
			return Insert(new TNode());
		}

        /// <summary>
        /// 	<para>Returns a new <tt>DAC type</tt> data record. This method must be used to initialize a data record that is of an appropriate type for the <tt>PXCache</tt>
        /// instance when its DAC type is unknown.</para>
        /// </summary>
		public override object CreateInstance()
		{
			TNode item = new TNode();

			CacheStaticInfo result = _Initialize(false);

			if (result._CreateExtensions != null)
			{
				PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>();
				if (dict == null)
				{
					dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}
				lock (((ICollection)dict).SyncRoot)
				{
					dict[item] = result._CreateExtensions(item);
				}
			}

			return item;
		}

		/// <summary>Clears the internal cache of database query
		/// results.</summary>
		public override void ClearQueryCacheObsolete()
		{

			//if (_Graph.TypedViews._QueriesLoaded)
			//{
			foreach (var viewQueries in _Graph.QueryCache.Values
										.Concat(_Graph.TypedViews._NonstandardViews))
			{
				if (viewQueries.CacheType == typeof(TNode) || viewQueries.CacheType.IsAssignableFrom(typeof(TNode)) && !Attribute.IsDefined(typeof(TNode), typeof(PXBreakInheritanceAttribute), false))
				{
					viewQueries.Clear();
				}
			}
			//}
			//foreach (KeyValuePair<Type, Dictionary<PXCommandKey, List<object>>> pair in _Graph.TypedViews._NonstandardQueries)
			//{
			//    if (pair.Key == typeof(TNode) || pair.Key.IsAssignableFrom(typeof(TNode)))
			//    {
			//        pair.Value.Clear();
			//    }
			//}
		}

		/// <summary>Clears the internal cache of database query
		/// results.</summary>
		public override void ClearQueryCache()
		{
			foreach (var viewQueries in _Graph.QueryCache.Values.Concat(_Graph.TypedViews._NonstandardViews))
			{
				if ((viewQueries.CacheType == typeof(TNode) || viewQueries.CacheType.IsAssignableFrom(typeof(TNode)) && !Attribute.IsDefined(typeof(TNode), typeof(PXBreakInheritanceAttribute), false)) ||
                    // temporary solution - check for graph.import to prevent continuous query caches invalidation
                    (!_Graph.IsImport && (viewQueries.CacheTypes?.Any(_ => _ == typeof(TNode) || _.IsAssignableFrom(typeof(TNode)) && !Attribute.IsDefined(typeof(TNode), typeof(PXBreakInheritanceAttribute), false)) == true)))
				{
					viewQueries.Clear();
				}
			}
		}


		/// <summary>Initializes the data record with the provided key values and places it into the cache with the Deleted or InsertedDeleted status. The method assigns the
		/// InsertedDeleted status to the data record if it has the Inserted status when the method is invoked.</summary>
		/// <param name="keys">The values of key fields.</param>
		/// <param name="values">The values of all fields. The parameter is not
		/// used in the method.</param>
		/// <remarks>
		/// 	<para>The method raises the following events: <tt>FieldUpdating</tt>,
		/// <tt>FieldUpdated</tt>, <tt>RowDeleting</tt>, and <tt>RowDeleted</tt>
		/// events. See <a href="Delete.html">Deleting a Data Record</a> for the events
		/// flowchart.</para>
		/// 	<para>This method is typically used to process deletion initiated from
		/// the user interface. If the <tt>AllowDelete</tt> property is
		/// <tt>false</tt>, the data record is not marked deleted and the method
		/// returns 0. The method returns 1 if the data record is successfully
		/// marked deleted.</para>
		/// </remarks>
		public override int Delete(IDictionary keys, IDictionary values)
		{
		    using (new PXPerformanceInfoTimerScope(info => info.TmDelete))
		    {
		        TNode item = (TNode) new TNode();
		        FillWithValues(item, null, keys, PXCacheOperation.Delete);

                var incoming = _Items.Locate(item);
                var incomingStatus = _Items.GetStatus(item);

		        TNode placed = _Items.PlaceDeleted(item, false);

		        if (placed == null)
		        {
		            if (readItem(item) != null)
		            {
		                placed = _Items.PlaceDeleted(item, true);
		            }
		        }

		        if (placed == null)
		        {
		            return 0;
		        }

				ClearQueryCache();

		        bool cancel = false;
		        try
		        {
		            cancel = !OnRowDeleting(placed, true);
		        }
		        catch (Exception)
		        {
		            cancel = true;
		            throw;
		        }
		        finally
		        {
		            if (cancel)
		            {
		                bool deleted;
		                _Items.PlaceInserted(placed, out deleted);
		            }
		        }
		        if (cancel)
		        {
		            return 0;
		        }

		        Current = placed;
		        if (!_AllowDelete && _Items.GetStatus(placed) != PXEntryStatus.InsertedDeleted)
		        {
		            bool deleted;
		            _Items.PlaceInserted(placed, out deleted);
		            return 0;
		        }

                SetSessionUnmodified(placed, incoming, incomingStatus);

		        _IsDirty = true;

		        OnRowDeleted(placed, true);

		        _Current = null;

		        return 1;
		    }
		}

        /// <summary>Places the data record into the cache with the Deleted or InsertedDeleted status. The method assigns the InsertedDeleted status to the data record if it has
        /// the Inserted status when the method is invoked.</summary>
        /// <param name="data">The data record to delete.</param>
        /// <remarks>
        ///   <para>The method raises the RowDeleting and RowDeleted events.</para>
        ///   <para>The AllowDelete property does not affect this method.</para>
        /// </remarks>
        /// <example>
        /// The code below deletes the current data records through the
        /// <tt>Address</tt> and <tt>Contact</tt> data views on deletion of an
        /// <tt>INSite</tt> data record.
        /// <code title="" description="" lang="CS">
        /// public PXSelect&lt;Address, Where&lt;Address.bAccountID, Equal&lt;Current&lt;Branch.bAccountID&gt;&gt;,
        ///     And&lt;Address.addressID, Equal&lt;Current&lt;INSite.addressID&gt;&gt;&gt;&gt;&gt; Address;
        /// public PXSelect&lt;Contact, Where&lt;Contact.bAccountID, Equal&lt;Current&lt;Branch.bAccountID&gt;&gt;,
        ///     And&lt;Contact.contactID, Equal&lt;Current&lt;INSite.contactID&gt;&gt;&gt;&gt;&gt; Contact;
        /// ...
        /// protected virtual void INSite_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        /// {
        ///     INSite row = (INSite)e.Row;
        ///     Address.Cache.Delete(Address.Current);
        ///     Contact.Cache.Delete(Contact.Current);
        /// }</code></example>
		public override object Delete(object data)
		{
			return Delete(data, false);
		}

		protected internal override object Delete(object data, bool bypassinterceptor)
		{
		    using (new PXPerformanceInfoTimerScope(info => info.TmDelete))
		    {
		        if (!bypassinterceptor && _Interceptor != null)
		        {
		            return _Interceptor.Delete(this, data);
		        }
		        if (data is PXResult)
		        {
		            data = ((PXResult)data)[0];
		        }
		        TNode item = data as TNode;
		        if (item == null)
		        {
		            return null;
		        }

                var incoming = _Items.Locate(item);
                var incomingState = _Items.GetStatus(item);

		        TNode placed = _Items.PlaceDeleted(item, false);

		        if (placed == null)
		        {
		            if (readItem(item) != null)
		            {
		                placed = _Items.PlaceDeleted(item, true);
		            }
		        }

		        if (placed == null)
		        {
		            return null;
		        }

				ClearQueryCache();

		        bool cancel = false;
		        try
		        {
		            cancel = !OnRowDeleting(placed, false);
		        }
		        catch (Exception)
		        {
		            cancel = true;
		            throw;
		        }
		        finally
		        {
		            if (cancel)
		            {
		                bool deleted;
		                _Items.PlaceInserted(placed, out deleted);
		            }
		        }
		        if (cancel)
		        {
		            return null;
		        }

                SetSessionUnmodified(placed, incoming, incomingState);

		        _IsDirty = true;

		        OnRowDeleted(placed, false);

		        _Current = null;

		        return placed;
		    }
		}

		protected internal override void PlaceNotChanged(object data)
		{
			PlaceNotChanged((TNode)data);
		}

		protected internal override object PlaceNotChanged(object data, out bool wasUpdated)
		{
			return _Items.PlaceNotChanged((TNode)data, out wasUpdated);
		}

		protected void PlaceNotChanged(TNode data)
		{
			bool wasUpdated;
			_Items.PlaceNotChanged(data, out wasUpdated);
		}

		protected void PlaceInserted(TNode data)
		{
			bool wasDeleted;
			_Items.PlaceInserted(data, out wasDeleted);
		}

		protected void PlaceUpdated(TNode data)
		{
			_Items.PlaceUpdated(data, false);
		}

		protected void PlaceDeleted(TNode data)
		{
			_Items.PlaceDeleted(data, false);
		}
		#endregion

		#region Dirty items enumerators
		/// <summary>Gets the collection of updated, inserted, and deleted data
		/// records. The collection contains data records with the
		/// <tt>Updated</tt>, <tt>Inserted</tt>, or <tt>Deleted</tt>
		/// status.</summary>
        public override IEnumerable Dirty
        {
            get
            {
                return (IEnumerable)_Items.Dirty;
            }
        }
        /// <summary>Gets the collection of updated data records that exist in the
        /// database. The collection contains data records with the
        /// <tt>Updated</tt> status.</summary>
        /// <example>
        /// 	<code title="Example" description="The following example shows how to iterate over all updated data records." lang="CS">
        /// // The defition of a data view
        /// public PXProcessing&lt;POReceipt&gt; Orders;
        /// ...
        /// // The optional delegate for the Orders data view
        /// public virtual IEnumerable orders()
        /// {
        ///     // Iterating over all updated POReceipt data records
        ///     foreach (POReceipt order in Orders.Cache.Updated)
        ///     {
        ///         yield return order;
        ///     }
        ///     ...
        /// }</code>
        /// </example>
		public override IEnumerable Updated
		{
			get
			{
				return (IEnumerable)_Items.Updated;
			}
		}

		/// <summary>Gets the collection of inserted data records that does not
		/// exist in the database. The collection contains data records with the
		/// <tt>Inserted</tt> status.</summary>
		/// <example>
		/// The code below modifies inserted <tt>Address</tt> data records on
		/// update of an <tt>EPEmployee</tt> data record.
		/// <code>
		/// protected virtual void EPEmployee_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		/// {
		///     // Checking whether the ParentBAccountID has changed
		///     if (!sender.ObjectsEqual&lt;EPEmployee.parentBAccountID&gt;(e.Row, e.OldRow))
		///     {
		///         // Iterating over all inserted and not saved Address data records
		///         foreach (Address addr in Address.Cache.Inserted)
		///         {
		///             addr.BAccountID = ((EPEmployee)e.Row).ParentBAccountID;
		///             addr.CountryID = company.Current.CountryID;
		///         }
		///         ...
		///     }
		/// }
		/// </code>
		/// </example>
		public override IEnumerable Inserted
		{
			get
			{
				return (IEnumerable)_Items.Inserted;
			}
		}


		/// <summary>Get the collection of all cached data records. The collection
		/// contains data records with any status. The developer should not rely
		/// on the presense of data records with statuses other than
		/// <tt>Updated</tt>, <tt>Inserted</tt>, and <tt>Deleted</tt> in this
		/// collection.</summary>
		public override IEnumerable Cached
		{
			get
			{
				return (IEnumerable)_Items.Cached;
			}
		}

        /// <summary>Gets the collection of deleted data records that exist in the
        /// database. The collection contains data records with the
        /// <tt>Deleted</tt> status.</summary>
        /// <example>
        /// 	<code title="Example" description="The code below deletes EPActivity data records through a different graph." lang="CS">
        /// public override void Persist()
        /// {
        ///     CREmailActivityMaint graph = CreateInstance&lt;CREmailActivityMaint&gt;();
        ///     // Iterating over all deleted EPActivity data records
        ///     foreach (EPActivity item in Emails.Cache.Deleted)
        ///     {
        ///         // Setting the current data record for the CREmailActivityMaint graph
        ///         graph.Message.Current = graph.Message.Search&lt;EPActivity.taskID&gt;(item.TaskID);
        ///         // Invoking the Delete action in the CREmailActivityMaint graph
        ///         graph.Delete.Press();
        ///         Emails.Cache.SetStatus(item, PXEntryStatus.Notchanged);
        ///     }
        ///     base.Persist();
        /// }</code>
        /// </example>
		public override IEnumerable Deleted
		{
			get
			{
				return (IEnumerable)_Items.Deleted;
			}
		}
		internal override int Version
		{
			get
			{
				return _Items.Version;
			}
			set
			{
				_Items.Version = value;
			}
		}

		internal override BqlTablePair GetOriginalObjectContext(object data, bool readItemIfNotExists = false)
	    {
	        TNode item = data as TNode;
	        if (item != null && !NonDBTable)
	        {
	            BqlTablePair orig = null;
	            bool found;
	            TNode output = null;
	            lock (((ICollection)_Originals).SyncRoot)
	            {
	                found = _Originals.TryGetValue(item, out orig) && orig.Unchanged != null;
	            }
                var isExtensionsLost = false;
	            if (_Extensions != null && found)
	            {
	                lock (((ICollection)_Extensions).SyncRoot)
	                {
	                    PXCacheExtension[] dummy;
                        isExtensionsLost = _Extensions != null && !_Extensions.TryGetValue(orig.Unchanged, out dummy);
	                }
	            }
	            if (!found || isExtensionsLost)
	            {
                    output = readItemIfNotExists ? readItem(item, true) : item;
	            }
	            if (output != null)
	            {
	                lock (((ICollection)_Originals).SyncRoot)
	                {
                        if (!_Originals.TryGetValue(item, out orig) || orig.Unchanged == null || isExtensionsLost)
	                    {
							if (object.ReferenceEquals(output, item))
								output = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal, _SetValueByOrdinal);

	                        if (orig == null)
	                        {
	                            _Originals[item] =
	                                orig =
	                                    new BqlTablePair
	                                    {
	                                        LastModified =
	                                            CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields, _GetValueByOrdinal,
	                                                SetValueByOrdinal),
	                                        Unchanged = output
	                                    };
	                        }
	                        else
	                        {
	                            if (orig.LastModified == null)
	                            {
	                                orig.LastModified = CreateCopy(output, _CreateExtensions, _CloneExtensions, _ClassFields,
	                                    _GetValueByOrdinal, _SetValueByOrdinal);
	                            }
	                            orig.Unchanged = output;
	                        }
	                    }
	                }
	            }
	            return orig;
	        }
	        return null;
	    }

	    /// <summary>Gets the value that indicates if the cache contains modified
		/// data records to be saved to database.</summary>
		public override bool IsInsertedUpdatedDeleted
		{
			get
			{
				return _Items.IsDirty;
			}
		}
		#endregion

		#region Persistance to the database
		private Dictionary<TNode, bool> persistedItems;
		private object[] getKeys(TNode node)
		{
			object[] ret = new object[Keys.Count];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = GetValue(node, Keys[i]);
			}
			return ret;
		}
		/// <summary>Saves the modifications of a particular type from the cache
		/// to the database. Returns the number of saved data records.</summary>
		/// <remarks>
		/// <para>Using this method, you can update, delete, or insert all data
		/// records kept by the cache. You can also perform different operations
		/// at once by passing a combination of <tt>PXDBOperation</tt> values,
		/// such as <tt>PXDBOperation.Insert | PXDBOperation.Update</tt>.</para>
		/// <para>The method raises the following events: <tt>RowPersisting</tt>,
		/// <tt>CommandPreparing</tt>, <tt>RowPersisted</tt>,
		/// <tt>ExceptionHandling</tt>.</para>
		/// </remarks>
		/// <param name="operation">The value that indicates the types of database
		/// operations to execute, either one of <tt>PXDBOperation.Insert</tt>,
		/// <tt>PXDBOperation.Update</tt>, and <tt>PXDBOperation.Delete</tt>
		/// values or their bitwise "or" (<tt>|</tt>) combination.</param>
		/// <example>
		/// The code below modifies a <tt>Vendor</tt> data record, updates it in
		/// the cache, saves changes to update <tt>Vendor</tt> data records to the
		/// database, and causes raising of the <tt>RowPersisted</tt> event with
		/// indication that the operation has completed successfully.
		/// <code>
		/// vendor.Status = BAccount.status.Inactive;
		/// Caches[typeof(Vendor)].Update(vendor);
		/// Caches[typeof(Vendor)].Persist(PXDBOperation.Update);
		/// Caches[typeof(Vendor)].Persisted(false);</code>
		/// </example>
		public override int Persist(PXDBOperation operation)
		{
			int ret = 0;

			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			IEnumerable<TNode> list = null;

			switch (operation)
			{
				case PXDBOperation.Update:
					list = _Items.Updated;
					break;
				case PXDBOperation.Insert:
					list = _Items.Inserted;
					break;
				case PXDBOperation.Delete:
					list = _Items.Deleted;
					break;
			}

			if (_PreventDeadlock && Keys.Count > 0)
			{
				list = new List<TNode>(list);
				int[] keys = Keys.Select(_ => _FieldsMap[_]).ToArray();

				Comparison<TNode> comparison = (a, b) =>
				{
					for (int i = 0; i < keys.Length; i++)
					{
						object aVal = GetValue(a, keys[i]);
						object bVal = GetValue(b, keys[i]);
						if (aVal is IComparable && bVal is IComparable)
						{
							int result = ((IComparable)aVal).CompareTo(bVal);
							if (result != 0)
							{
								return result;
							}
						}
						else if (aVal == null)
						{
							if (bVal != null)
							{
								return -1;
							}
						}
						else if (bVal == null)
						{
							return 1;
						}
					}
					return 0;
				};

				if (_CustomDeadlockComparison == null)
				{
					((List<TNode>)list).Sort(comparison);
				}
				else
				{
					((List<TNode>)list).Sort(_CustomDeadlockComparison);
				}
			}

			switch (operation)
			{
				case PXDBOperation.Update:
					foreach (TNode node in list)
					{
						if (PersistUpdated(node))
						{
						ret++;
					}
					}
					break;
				case PXDBOperation.Insert:
					foreach (TNode node in list)
					{
						if (PersistInserted(node))
						{
						ret++;
						}
						_ItemsDenormalized = true;
					}
					_Items.Normalize(null);
					_ItemsDenormalized = false;
					break;
				case PXDBOperation.Delete:
					foreach (TNode node in list)
					{
						if (PersistDeleted(node))
						{
						ret++;
					}
					}
					break;
			}

			return ret;
		}

		/// <summary>Saves the modification of the specified type from the cache
		/// to the database for a particular data record.</summary>
		/// <param name="row">The data record to save to the database.</param>
		/// <param name="operation">The database operation to perform for the data
		/// record, either one of <tt>PXDBOperation.Insert</tt>,
		/// <tt>PXDBOperation.Update</tt>, and <tt>PXDBOperation.Delete</tt>
		/// values or their bitwise "or" (<tt>|</tt>) combination.</param>
		public override void Persist(object row, PXDBOperation operation)
		{
			switch (operation)
			{
				case PXDBOperation.Update:
					PersistUpdated(row);
					break;
				case PXDBOperation.Insert:
					PersistInserted(row);
					break;
				case PXDBOperation.Delete:
					PersistDeleted(row);
					break;
			}
		}

		internal protected override void _AdjustStorage(string name, PXDataFieldParam assign)
		{
			int i;
			if (_FieldsMap.TryGetValue(name, out i))
			{
				int k = _ReverseMap[name];
				_AdjustStorage(k, assign);
			}
			else if (_KeyValueAttributeTypes.TryGetValue(name, out var storage))
			{
				assign.Storage = storage;
			}
		}

		internal protected override void _AdjustStorage(int i, PXDataFieldParam assign)
		{
			if (!IsKvExtField(_ClassFields[i]))
				return;

			assign.Column = new Column(_ClassFields[i]);
			switch (Type.GetTypeCode(_FieldTypes[i]))
			{
				case TypeCode.String:
					if (assign.ValueType == PXDbType.Text || assign.ValueType == PXDbType.NText
						|| assign.ValueLength == null || assign.ValueLength > 256)
					{
						assign.Storage = StorageBehavior.KeyValueText;
					}
					else
					{
						assign.Storage = StorageBehavior.KeyValueString;
					}
					break;
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Boolean:
				case TypeCode.Decimal:
				case TypeCode.Single:
				case TypeCode.Double:
					assign.Storage = StorageBehavior.KeyValueNumeric;
					if (assign.Value != null)
					{
						assign.Value = Convert.ToDecimal(assign.Value);
					}
					break;
				case TypeCode.DateTime:
					assign.Storage = StorageBehavior.KeyValueDate;
					break;
				case TypeCode.Object:
					if (_FieldTypes[i] == typeof(Guid))
					{
						assign.Storage = StorageBehavior.KeyValueString;
						if (assign.Value != null)
						{
							assign.Value = ((Guid)assign.Value).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
						}
					}
					else if (_FieldTypes[i] == typeof(byte[]))
					{
						assign.Storage = StorageBehavior.KeyValueText;
						if (assign.Value != null)
						{
							assign.Value = Convert.ToBase64String((byte[])assign.Value);
						}
					}
					break;
			}
		}

		/// <summary>Updates the provided data record in the database. Returns
		/// <tt>true</tt> if the data record has been updated sucessfully, or
		/// <tt>false</tt> otherwise.</summary>
		/// <remarks>
		/// <para>The method raises the following events: <tt>RowPersisting</tt>,
		/// <tt>CommandPreparing</tt>, <tt>RowPersisted</tt>,
		/// <tt>ExceptionHandling</tt>.</para>
		/// <para>The default behavior can be modified by the
		/// <tt>PXDBInterceptor</tt> attribute.</para>
		/// </remarks>
		/// <param name="row">The data record to update in the database.</param>
		internal protected override bool PersistUpdated(object row, bool bypassInterceptor)
		{
			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			bool cancel;
			if (persistedItems.TryGetValue((TNode)row, out cancel))
			{
				return !cancel;
			}

			if (!DisableCloneAttributes)
				TryDispose(GetAttributes(row, null));
			cancel = true;
			try
			{
				cancel = !OnRowPersisting(row, PXDBOperation.Update);
				if (!cancel && _Interceptor != null && !bypassInterceptor)
				{
					cancel = true;
					cancel = !_Interceptor.PersistUpdated(this, row);
					return !cancel;
				}
			}
			catch (PXCommandPreparingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXRowPersistingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXDatabaseException e)
			{
				if (e.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
				{
					throw GetLockViolationException((TNode)row, PXDBOperation.Insert);
				}
				else if (e is PXDataWouldBeTruncatedException truncatedEx)
				{
					truncatedEx.Operation = PXDBOperation.Update;
					if (!String.IsNullOrEmpty(truncatedEx.CommandText))
						PXTrace.WriteError("{0}{1}SQL query:{1}{2}", truncatedEx.Message, Environment.NewLine, truncatedEx.CommandText);
				}
				e.Keys = getKeys((TNode)row);
				throw;
			}
			finally
			{
				if (!bypassInterceptor)
				{
					persistedItems.Add((TNode)row, cancel);
				}
			}
			if (!cancel)
			{
			    var unchanged = GetOriginal(row);
				List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
				KeysVerifyer kv = new KeysVerifyer(this);
				bool noteIDRequred = _KeyValueStoredNames != null || _KeyValueAttributeNames != null;
				try
				{
					for (int i = 0; i < _ClassFields.Count; i++)
					{
						string descr = _ClassFields[i];
						PXCommandPreparingEventArgs.FieldDescription description = null;
						object val = GetValue(row, descr);
						OnCommandPreparing(descr, row, val, PXDBOperation.Update, null, out description);
						if (description != null && description.Expr != null)
						{
                            var origval = unchanged.With(c => GetValue(unchanged, descr));
						    PXCommandPreparingEventArgs.FieldDescription origdescription=null;
						    if(origval!=null)
						        OnCommandPreparing(descr, unchanged, origval, PXDBOperation.Update, null, out origdescription);
						    if (description.IsRestriction)
							{
								kv.SetRestriction(descr, description);
								if (origval != null && description.DataType != PXDbType.Timestamp
									&& Keys.Contains(descr) && !object.Equals(origval, val))
								{
									var column = (Column) description.Expr;
									if (origdescription != null && origdescription.Expr != null)
									{
										PXDataFieldAssign assign = new PXDataFieldAssign(column, description.DataType, description.DataLength, description.DataValue, ValueToString(descr, val, description.DataValue))
										{
											OldValue = origdescription.DataValue??origval,
										};
										pars.Add(assign);
										pars.Add(new PXDataFieldRestrict((Column)origdescription.Expr, origdescription.DataType, origdescription.DataLength, origdescription.DataValue));
									}
									else
									{
										pars.Add(new PXDataFieldRestrict(column, description.DataType, description.DataLength, description.DataValue));
									}
								}
								else
								{
									pars.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
								}
							}
							else
						    {
							    var column = (Column) description.Expr;
								if (description.IsExcludedFromUpdate)
						        {
						            var param = new PXDummyDataFieldRestrict(column, description.DataType, description.DataLength, origdescription?.DataValue??origval);
						            pars.Add(param);
                                }
						        else
						        {
						            PXDataFieldAssign assign = new PXDataFieldAssign(column, description.DataType,
						                description.DataLength, description.DataValue, null);
								if (unchanged != null)
								{
						                if (assign.IsChanged = !object.Equals(GetValue(row, descr), origval))
									{
										assign.NewValue = ValueToString(descr, val, description.DataValue);
									    assign.OldValue = origdescription == null || PXCache.IsOrigValueNewDate(this, origdescription)
									        ? origval
									        : origdescription.DataValue;

									}
								}
								else assign.IsChanged = false;

									if (noteIDRequred && String.Equals(descr, _NoteIDName, StringComparison.OrdinalIgnoreCase))
									{
										if (assign.Value == null)
										{
											assign.Value = SequentialGuid.Generate();
											SetValue(row, (int)_NoteIDOrdinal, assign.Value);
										}
										PXDataFieldAssign n =
											new PXDataFieldAssign(_NoteIDName, PXDbType.UniqueIdentifier, 16, assign.Value);
										n.Storage = StorageBehavior.KeyValueKey;
										pars.Add(n);
										noteIDRequred = false;

										if (_KeyValueAttributeNames != null)
										{
											object[] vals = GetSlot<object[]>(row, _KeyValueAttributeSlotPosition);
											object[] origs = GetSlot<object[]>(row, _KeyValueAttributeSlotPosition, true);

											if (vals != null)
											{
												foreach (KeyValuePair<string, int> pair in _KeyValueAttributeNames)
												{
													if (pair.Value < vals.Length)
													{
														OnCommandPreparing(pair.Key, row, vals[pair.Value], PXDBOperation.Update, null, out var d);
														PXDataFieldAssign a =
															new PXDataFieldAssign(pair.Key, 
															_KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueDate ? PXDbType.DateTime
															: (_KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueNumeric ? PXDbType.Bit
															: PXDbType.NVarChar), vals[pair.Value]
															);
														a.Storage = _KeyValueAttributeTypes[pair.Key];
														if (a.IsChanged = origs != null && pair.Value < origs.Length && !object.Equals(a.Value, origs[pair.Value]))
														{
															a.NewValue = AttributeValueToString(pair.Key, a.Value);
															a.OldValue = origs[pair.Value];

														}
														pars.Add(a);
													}
												}
											}
										}
									}
								_AdjustStorage(i, assign);
								pars.Add(assign);
							}
						}
					}
				}
				}
				catch (PXCommandPreparingException e)
				{
					if (OnExceptionHandling(e.Name, row, e.Value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
					return false;
				}
				try
				{
					pars.Add(PXDataFieldRestrict.OperationSwitchAllowed);
					kv.Check(_BqlTable);
                    if(unchanged==null)
                        pars.Add(PXSelectOriginalsRestrict.SelectOriginalValues);
					if (!_Graph.ProviderUpdate(_BqlTable, pars.ToArray()))
					{
						throw GetLockViolationException(pars.ToArray(), PXDBOperation.Update);
					}
					try
					{
						OnRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
						lock (((ICollection)_Originals).SyncRoot)
						{
							BqlTablePair pair;
							if (_Originals.TryGetValue((TNode)row, out pair))
							{
								if (_OriginalsRemoved == null)
								{
									_OriginalsRemoved = new PXCacheOriginalCollection();
								}
								_OriginalsRemoved[(TNode)row] = pair;
							}
							_Originals.Remove((TNode)row);
						}
					}
					catch (PXRowPersistedException e)
					{
						OnExceptionHandling(e.Name, row, e.Value, e);
						throw;
					}
				}
                catch (PXDbOperationSwitchRequiredException e)
					{
						List<PXDataFieldAssign> ipars = new List<PXDataFieldAssign>();
						try
						{
							foreach (string descr in _ClassFields)
							{
                                var origval = unchanged.With(c => GetValue(unchanged, descr));
                                PXCommandPreparingEventArgs.FieldDescription description = null;
								OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Insert, null, out description);
								if (description?.Expr != null && !description.IsExcludedFromUpdate)
								{
									var assign = new PXDataFieldAssign((Column) description.Expr, description.DataType, description.DataLength,
										description.DataValue);
									ipars.Add(assign);
									if (assign.IsChanged = !Equals(description.DataValue, origval))
									{
										assign.OldValue = origval;
									}
								}
							}
						}
						catch (PXCommandPreparingException ex)
						{
							if (OnExceptionHandling(ex.Name, row, ex.Value, ex))
							{
								throw;
							}
							PXTrace.WriteWarning(e);
							return false;
						}
					    try
					    {
					        _Graph.ProviderInsert(_BqlTable, ipars.ToArray());
					        try
					        {
					            OnRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
					            lock (((ICollection)_Originals).SyncRoot)
					            {
					                BqlTablePair pair;
					                if (_Originals.TryGetValue((TNode)row, out pair))
					                {
					                    if (_OriginalsRemoved == null)
					                    {
					                        _OriginalsRemoved = new PXCacheOriginalCollection();
					                    }
					                    _OriginalsRemoved[(TNode)row] = pair;
					                }
					                _Originals.Remove((TNode)row);
					            }
					        }
					        catch (PXRowPersistedException ex)
					        {
					            OnExceptionHandling(ex.Name, row, ex.Value, ex);
					            throw;
					        }
					    }
					    catch (PXDatabaseException ex)
					    {
					        if (ex.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
					        {
								throw GetLockViolationException(ipars.ToArray(), PXDBOperation.Insert);
							}
					        ex.Keys = getKeys((TNode)row);
					        throw;
					    }
					}
				catch (PXDatabaseException e)
					{
						e.Keys = getKeys((TNode)row);
						throw;
					}
				}
			return !cancel;
		}
        
	    /// <summary>Inserts the provided data record into the database. Returns
		/// <tt>true</tt> if the data record has been inserted sucessfully, or
		/// <tt>false</tt> otherwise.</summary>
		/// <remarks>
		/// <para>The method throws an exception if the data record with such keys
		/// exists in the database.</para>
		/// <para>The method raises the following events: <tt>RowPersisting</tt>,
		/// <tt>CommandPreparing</tt>, <tt>RowPersisted</tt>,
		/// <tt>ExceptionHandling</tt>.</para>
		/// <para>The default behavior can be modified by the
		/// <tt>PXDBInterceptor</tt> attribute.</para>
		/// </remarks>
		/// <param name="row">The data record to insert into the database.</param>
		internal protected override bool PersistInserted(object row, bool bypassInterceptor)
		{
			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			bool cancel;
			if (persistedItems.TryGetValue((TNode)row, out cancel))
			{
				return !cancel;
			}

			if (!DisableCloneAttributes)
				TryDispose(GetAttributes(row, null));
			cancel = true;
			try
			{
				cancel = !OnRowPersisting(row, PXDBOperation.Insert);
				if (!cancel && _Interceptor != null && !bypassInterceptor)
				{
					cancel = true;
					cancel = !_Interceptor.PersistInserted(this, row);
					return !cancel;
				}
			}
			catch (PXCommandPreparingException e)
			{
				_Items.Normalize(null);
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXRowPersistingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXDatabaseException e)
			{
				if (e.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
				{
					throw GetLockViolationException((TNode)row, PXDBOperation.Insert);
				}
				else if (e is PXDataWouldBeTruncatedException truncatedEx)
				{
					truncatedEx.Operation = PXDBOperation.Insert;
					if (!String.IsNullOrEmpty(truncatedEx.CommandText))
						PXTrace.WriteError("{0}{1}SQL query:{1}{2}", truncatedEx.Message, Environment.NewLine, truncatedEx.CommandText);
				}
				e.Keys = getKeys((TNode)row);
				throw;
			}
			finally
			{
				if (!bypassInterceptor)
				{
					persistedItems.Add((TNode)row, cancel);
				}
			}
			if (!cancel)
			{
				bool audit = false;
				Type table = BqlTable;
				while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
				{
					table = table.BaseType;
				}
				List<PXDataFieldAssign> pars = new List<PXDataFieldAssign>();
				bool noteIDRequred = _KeyValueStoredNames != null || _KeyValueAttributeNames != null;
				try
				{
					for (int i = 0; i < _ClassFields.Count; i++)
					{
						string descr = _ClassFields[i];
						PXCommandPreparingEventArgs.FieldDescription description = null;
						object val = GetValue(row, descr);
						OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Insert, null, out description);
						if (description?.Expr != null && !description.IsExcludedFromUpdate)
						{
							PXDataFieldAssign assign;
							pars.Add(assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, null));
							if (audit && val != null)
							{
								assign.IsChanged = true;
								assign.NewValue = ValueToString(descr, val, description.DataValue);
							}
							else assign.IsChanged = false;

							if (noteIDRequred && String.Equals(descr, _NoteIDName, StringComparison.OrdinalIgnoreCase))
							{
								if (assign.Value == null)
								{
									assign.Value = SequentialGuid.Generate();
									SetValue(row, (int)_NoteIDOrdinal, assign.Value);
								}
								PXDataFieldAssign n = new PXDataFieldAssign(_NoteIDName, PXDbType.UniqueIdentifier, 16, assign.Value);
								n.Storage = StorageBehavior.KeyValueKey;
								pars.Add(n);
								noteIDRequred = false;

								if (_KeyValueAttributeNames != null)
								{
									object[] vals = GetSlot<object[]>(row, _KeyValueAttributeSlotPosition);

									if (vals != null)
									{
										foreach (KeyValuePair<string, int> pair in _KeyValueAttributeNames)
										{
											if (pair.Value < vals.Length)
											{
												OnCommandPreparing(pair.Key, row, vals[pair.Value], PXDBOperation.Insert, null, out var d);
												PXDataFieldAssign a =
													new PXDataFieldAssign(pair.Key,
													_KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueDate ? PXDbType.DateTime
													: (_KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueNumeric ? PXDbType.Bit
													: PXDbType.NVarChar), vals[pair.Value]
													);
												a.Storage = _KeyValueAttributeTypes[pair.Key];
												if (a.IsChanged = audit && a.Value != null)
												{
													a.NewValue = AttributeValueToString(pair.Key, a.Value);

												}
												pars.Add(a);
											}
										}
									}
								}
							}
							_AdjustStorage(i, assign);
						}
					}
				}
				catch (PXCommandPreparingException e)
				{
					_Items.Normalize(null);
					if (OnExceptionHandling(e.Name, row, e.Value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
					return false;
				}
				try
				{
					pars.Add(PXDataFieldAssign.OperationSwitchAllowed);
					_Graph.ProviderInsert(_BqlTable, pars.ToArray());
                    try
					{
						OnRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
						lock (((ICollection)_Originals).SyncRoot)
						{
							BqlTablePair pair;
							if (_Originals.TryGetValue((TNode)row, out pair))
							{
								if (_OriginalsRemoved == null)
								{
									_OriginalsRemoved = new PXCacheOriginalCollection();
								}
								_OriginalsRemoved[(TNode)row] = pair;
							}
							_Originals.Remove((TNode)row);
						}
					}
					catch (PXRowPersistedException e)
					{
						OnExceptionHandling(e.Name, row, e.Value, e);
						throw;
					}
				}

				catch (PXDbOperationSwitchRequiredException e)
				{
					TNode unchanged = null;
					try
					{
						lock (((ICollection)_Originals).SyncRoot)
						{
							BqlTablePair orig;
							if (_Originals.TryGetValue((TNode)row, out orig))
							{
								unchanged = orig.Unchanged as TNode;
							}
						}
					}
					catch
					{
					}
					List<PXDataFieldParam> upars = new List<PXDataFieldParam>();
					KeysVerifyer kv = new KeysVerifyer(this);
					try
					{
						foreach (string descr in _ClassFields)
						{
							IPXIdentityColumn idAttribute = null;
							int iField;
							if (descr.Equals(Identity, StringComparison.Ordinal) && _FieldsMap.TryGetValue(descr, out iField))
							{
								for (int iAttr = _FieldAttributesFirst[iField]; iAttr <= _FieldAttributesLast[iField] && idAttribute == null; iAttr++)
								{
									idAttribute = _FieldAttributes[iAttr] as IPXIdentityColumn;
								}
							}
							PXCommandPreparingEventArgs.FieldDescription description = null;
							object val = GetValue(row, descr);
							OnCommandPreparing(descr, row, val, PXDBOperation.Update | PXDBOperation.Second, null, out description);
							if (description != null && description.Expr != null)
							{
								if (description.IsRestriction)
								{
									kv.SetRestriction(descr, description);
									object dataValue = idAttribute == null ? description.DataValue : idAttribute.GetLastInsertedIdentity(description.DataValue);
									upars.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, dataValue));
								}
								else
								{
								    var origval = unchanged.With(c => GetValue(unchanged, descr));
									var column = (Column) description.Expr;

									if (description.IsExcludedFromUpdate)
								    {
								        if (unchanged != null)
								        {
								            var param = new PXDummyDataFieldRestrict(column, description.DataType,
								                description.DataLength, origval);
								            upars.Add(param);
								        }
								    }
								    else
								    {
								        PXDataFieldAssign assign = new PXDataFieldAssign(column, description.DataType,
								            description.DataLength, description.DataValue, null);
									if (unchanged != null)
									{
								            if (assign.IsChanged = !object.Equals(GetValue(row, descr), origval))
										{
											assign.NewValue = ValueToString(descr, val, description.DataValue);
										}
									}
									else assign.IsChanged = false;
									upars.Add(assign);
								}
							}
						}
					}
					}
					catch (PXCommandPreparingException ex)
					{
						if (OnExceptionHandling(ex.Name, row, ex.Value, ex))
						{
							throw;
						}
						PXTrace.WriteWarning(e);
						return false;
					}
					try
					{
						kv.Check(_BqlTable);
                        if(unchanged==null)
                            upars.Add(PXSelectOriginalsRestrict.SelectOriginalValues);
						if (!_Graph.ProviderUpdate(_BqlTable, upars.ToArray()))
						{
							throw GetLockViolationException(upars.ToArray(), PXDBOperation.Update);
						}
                        try
						{
							OnRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
							lock (((ICollection)_Originals).SyncRoot)
							{
								BqlTablePair pair;
								if (_Originals.TryGetValue((TNode)row, out pair))
								{
									if (_OriginalsRemoved == null)
									{
										_OriginalsRemoved = new PXCacheOriginalCollection();
									}
									_OriginalsRemoved[(TNode)row] = pair;
								}
								_Originals.Remove((TNode)row);
							}
						}
						catch (PXRowPersistedException ex)
						{
							OnExceptionHandling(ex.Name, row, ex.Value, ex);
							throw;
						}
					}
					catch (PXDatabaseException ex)
					{
						if (ex.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
						{
							throw GetLockViolationException(upars.ToArray(), PXDBOperation.Insert);
						}
						ex.Keys = getKeys((TNode)row);
						throw;
					}
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
					{
						throw GetLockViolationException(pars.ToArray(), PXDBOperation.Insert);
					}
					e.Keys = getKeys((TNode)row);
					throw;
				}
			}
			return !cancel;
		}

		/// <summary>Deletes the provided data record from the database by the key
		/// fields. Returns <tt>true</tt> if the data record has been deleted
		/// sucessfully, or <tt>false</tt> otherwise.</summary>
		/// <remarks>
		/// <para>The method raises the following events: <tt>RowPersisting</tt>,
		/// <tt>CommandPreparing</tt>, <tt>RowPersisted</tt>,
		/// <tt>ExceptionHandling</tt>.</para>
		/// <para>The default behavior can be modified by the
		/// <tt>PXDBInterceptor</tt> attribute.</para>
		/// </remarks>
		/// <param name="row">The data record to deleted from the
		/// database.</param>
		internal protected override bool PersistDeleted(object row, bool bypassInterceptor)
		{
			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			bool cancel;
			if (persistedItems.TryGetValue((TNode)row, out cancel))
			{
				return !cancel;
			}
			if (!DisableCloneAttributes)
				TryDispose(GetAttributes(row, null));
			cancel = true;
			try
			{
				cancel = !OnRowPersisting(row, PXDBOperation.Delete);
				if (!cancel && _Interceptor != null && !bypassInterceptor)
				{
					cancel = true;
					cancel = !_Interceptor.PersistDeleted(this, row);
					return !cancel;
				}
			}
			catch (PXCommandPreparingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXRowPersistingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXDatabaseException e)
			{
				e.Keys = getKeys((TNode)row);
				throw;
			}
			finally
			{
				if (!bypassInterceptor)
				{
					persistedItems.Add((TNode)row, cancel);
				}
			}
			if (!cancel)
			{
			    var unchanged = GetOriginal(row) as TNode;
                
                List<PXDataFieldRestrict> pars = new List<PXDataFieldRestrict>();
				bool noteIDRequred = _KeyValueStoredNames != null || _DBLocalizableNames != null || _KeyValueAttributeNames != null;
				try
				{
					for (int i = 0; i < _ClassFields.Count; i++)
					{
						string descr = _ClassFields[i];
						PXCommandPreparingEventArgs.FieldDescription description = null;
						OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Delete, null, out description);
                        if (description != null && description.Expr != null)
                        {
	                        var column = (Column) description.Expr;
							if (description.IsRestriction)
							{
								pars.Add(new PXDataFieldRestrict(column, description.DataType, description.DataLength, description.DataValue));
							}
							else 
							{
                                var origval = unchanged.With(c => GetValue(unchanged, descr));
							    PXCommandPreparingEventArgs.FieldDescription origdescription=null;
							    if(origval!=null)
							        OnCommandPreparing(descr, unchanged, origval, PXDBOperation.Update, null, out origdescription);
                                var assign = IsKvExtField(_ClassFields[i])
                                    ? new PXDataFieldRestrict(column, description.DataType, description.DataLength, description.DataValue)
							        : new PXDummyDataFieldRestrict(column, description.DataType, description.DataLength, unchanged != null ? origdescription?.DataValue??origval : description.DataValue);
							    if (noteIDRequred && String.Equals(descr, _NoteIDName, StringComparison.OrdinalIgnoreCase))
								{
									if (assign.Value == null)
									{
										assign.Value = SequentialGuid.Generate();
										SetValue(row, (int)_NoteIDOrdinal, assign.Value);
									}
									PXDataFieldRestrict n = new PXDataFieldRestrict(_NoteIDName, PXDbType.UniqueIdentifier, 16, assign.Value);
									n.Storage = StorageBehavior.KeyValueKey;
									pars.Add(n);
									noteIDRequred = false;
								}
								_AdjustStorage(i, assign);
								pars.Add(assign);
							}
						}
					}
				}
				catch (PXCommandPreparingException e)
				{
					if (OnExceptionHandling(e.Name, row, e.Value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
					return false;
				}
				try
				{
                    if(unchanged==null)
                        pars.Add(PXSelectOriginalsRestrict.SelectOriginalValues);
					if (!_Graph.ProviderDelete(_BqlTable, pars.ToArray()))
					{
						throw GetLockViolationException(pars.ToArray(), PXDBOperation.Delete);
					}
                    try
					{
						OnRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
					}
					catch (PXRowPersistedException e)
					{
						OnExceptionHandling(e.Name, row, e.Value, e);
						throw;
					}
				}
				catch (PXDatabaseException e)
				{
					if (e is PXDbOperationSwitchRequiredException)
					{
						List<PXDataFieldAssign> ipars = new List<PXDataFieldAssign>();
						try
						{
							foreach (string descr in _ClassFields)
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Insert, null, out description);
								if (description?.Expr != null && !description.IsExcludedFromUpdate)
								{
									ipars.Add(new PXDataFieldAssign((Column) description.Expr, description.DataType, description.DataLength,
										description.DataValue));
								}
							}
						}
						catch (PXCommandPreparingException ex)
						{
							if (OnExceptionHandling(ex.Name, row, ex.Value, ex))
							{
								throw;
							}
							PXTrace.WriteWarning(e);
							return false;
						}
						try
						{
							_Graph.ProviderInsert(_BqlTable, ipars.ToArray());
                            try
							{
								OnRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
							}
							catch (PXRowPersistedException ex)
							{
								OnExceptionHandling(ex.Name, row, ex.Value, ex);
								throw;
							}
						}
						catch (PXDatabaseException ex)
						{
							if (ex.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
							{
								throw GetLockViolationException(ipars.ToArray(), PXDBOperation.Insert);
							}
							ex.Keys = getKeys((TNode)row);
							throw;
						}
					}
					else
					{
						e.Keys = getKeys((TNode)row);
						throw;
					}
				}
			}
			return !cancel;
		}

		/// <exclude/>
		public override void ResetPersisted(object row)
		{
			if (persistedItems != null && row is TNode && persistedItems.ContainsKey((TNode)row))
			{
				persistedItems.Remove((TNode)row);
			}
		}

        /// <summary>Completes saving changes to the database by raising the
        /// <tt>RowPersisted</tt> event for all persisted data records.</summary>
        /// <param name="isAborted">The value indicating whether the database
        /// operation has been aborted or completed.</param>
        /// <example>
        /// 	<code title="Example" description="You need to call this method in the application only when you call the Persist(), PersistInserted(), PersistUpdated(), or PersistDeleted() method, as the following example shows." lang="CS">
        /// // Opening a transaction and saving changes to the provided
        /// // new data record
        /// using (PXTransactionScope ts = new PXTransactionScope())
        /// {
        ///     cache.PersistInserted(item);
        ///     ts.Complete(this);
        /// }
        /// // Indicating successful completion of saving changes to the database
        /// cache.Persisted(false);</code>
        /// </example>
		public override void Persisted(bool isAborted)
		{
			Persisted(isAborted, null);
		}

		protected internal override void Persisted(bool isAborted, Exception exception)
		{
			if (persistedItems == null)
			{
				return;
			}
			if (_OriginalsRemoved != null)
			{
				if (isAborted)
				{
					lock (((ICollection)_Originals).SyncRoot)
					{
						foreach (PXWeakReference t in _OriginalsRemoved.Keys)
						{
							if (t.IsAlive)
							{
								IBqlTable key = t.Target as IBqlTable;
								if (t != null)
								{
									_Originals[key] = _OriginalsRemoved[key];
								}
							}
						}
					}
				}
				_OriginalsRemoved = null;
			}
			List<object> successed = new List<object>();
			foreach (TNode node in _Items.Updated)
			{
				if (persistedItems.ContainsKey(node))
				{
					OnRowPersisted(node, PXDBOperation.Update, isAborted ? PXTranStatus.Aborted : PXTranStatus.Completed, exception);
					if (!isAborted && !persistedItems[node])
					{
						SetStatus(node, PXEntryStatus.Notchanged);
						successed.Add(node);
					}
				}
			}
			foreach (TNode node in _Items.Inserted)
			{
				if (persistedItems.ContainsKey(node))
				{
					OnRowPersisted(node, PXDBOperation.Insert, isAborted ? PXTranStatus.Aborted : PXTranStatus.Completed, exception);
					if (!isAborted && !persistedItems[node])
					{
						SetStatus(node, PXEntryStatus.Notchanged);
						successed.Add(node);
					}
				}
			}
			if (isAborted)
			{
				_Items.Normalize(null);
			}
			foreach (TNode node in _Items.Deleted)
			{
				if (persistedItems.ContainsKey(node))
				{
					OnRowPersisted(node, PXDBOperation.Delete, isAborted ? PXTranStatus.Aborted : PXTranStatus.Completed, exception);
					if (!isAborted && !persistedItems[node])
					{
						SetStatus(node, PXEntryStatus.InsertedDeleted);
					}
				}
			}
			persistedItems = null;
			if (!isAborted)
			{
				_IsDirty = false;
			}
			PXAutomation.StorePersisted(_Graph, _BqlTable, successed);
		}
		#endregion

		#region Session state managment methods
		protected Dictionary<TNode, TNode> _ChangedKeys;
        /// <exclude/>
		protected sealed class ItemComparer : IEqualityComparer<TNode>
		{
			private PXCache _Cache;
			public ItemComparer(PXCache cache)
			{
				_Cache = cache;
			}
			public int GetHashCode(TNode item)
			{
				return _Cache.GetObjectHashCode(item);
			}
			public bool Equals(TNode item1, TNode item2)
			{
				return _Cache.ObjectsEqual(item1, item2);
			}
		}

		private void ClearSessionUnmodified(IEnumerable<TNode> items)
		{
			foreach (var item in items)
			{
				if (item == null) continue;
				BqlTablePair pair;
				if (_Originals.TryGetValue(item, out pair))
				{
					pair.SessionUnmodified = null;
				}
			}
		}

		private bool stateLoaded;
		
	    /// <summary>Loads dirty items and other cache state objects from the
		/// session. The application does not typically use this method.</summary>
		public override void Load()
		{
			if (stateLoaded)
			{
				if (_Graph.stateLoading && _Current != null)
				{
					try
					{
						OnRowSelected((TNode)_Current);
					}
					catch
					{
					}
				}
				return;
			}
			if (!PXContext.Session.IsSessionEnabled)
			{
			    _Graph.Caches.AttachAllHandlers(this);
				return;
			}			
			stateLoaded = true;
			if (Graph.UnattendedMode)
			{
			    _Graph.Caches.AttachAllHandlers(this);
				return;
			}
            LoadFromSession(true);
            _Graph.Caches.AttachAllHandlers(this);
		}

        internal override void LoadFromSession(bool force)
        {
            if (!PXContext.Session.IsSessionEnabled) return;
            if (stateLoaded && !force) return;
			string key = Graph.StatePrefix + String.Format("{0}${1}", _Graph.GetType().FullName, typeof(TNode).FullName);
			object[] bucket = PXContext.Session.CacheInfo[key] as object[];
			if (bucket != null && bucket.Length > 0)
			{
				TNode[] items = bucket[0] as TNode[];
				if (items != null)
				{
					for (int i = 0; i < items.Length; ++i)
					{
						TNode item = _Items.PlaceUpdated(items[i], true);
						if (item != null)
						{
							lock (((ICollection)_Originals).SyncRoot)
							{
								BqlTablePair orig;
								if (_Originals.TryGetValue(item, out orig) && orig.Unchanged is TNode && !object.ReferenceEquals(item, orig.Unchanged) && !ObjectsEqual(item, orig.Unchanged))
								{
									if (_ChangedKeys == null)
									{
										_ChangedKeys = new Dictionary<TNode, TNode>(new ItemComparer(this));
									}
									_ChangedKeys[(TNode)orig.Unchanged] = item;
								}
							}
						}
					}
					if (_ChangedKeys != null)
					{
						ClearQueryCache();
					}
				}
				if (bucket.Length > 1)
				{
					items = bucket[1] as TNode[];
					if (items != null)
					{
						for (int i = 0; i < items.Length; ++i)
						{
							bool deleted;
							_Items.PlaceInserted(items[i], out deleted);
						}
					}
				}
				if (bucket.Length > 2)
				{
					items = bucket[2] as TNode[];
					if (items != null)
					{
						for (int i = 0; i < items.Length; ++i)
						{
							_Items.PlaceDeleted(items[i], true);
						}
					}
				}
				if (bucket.Length > 3)
				{
					items = bucket[3] as TNode[];
					if (items != null)
					{
						bool wasUpdated;
						for (int i = 0; i < items.Length; ++i)
						{
							_Items.PlaceNotChanged(items[i], out wasUpdated);
							_Items.SetStatus(items[i], PXEntryStatus.Held);
						}
					}
				}
				if (bucket.Length > 4)
				{
					TNode item = bucket[4] as TNode;
					if (item != null)
					{
						try
						{
							TNode cached = _Items.Locate(item);
                            TNode currentItem = cached ?? item;

                            if (force) Current = currentItem;
                            else _Current = currentItem;

                            if (cached == null 
                                && bucket.Length > 6 
                                && (bool)bucket[6])
								{
									bool wasUpdated;
                                _Items.PlaceNotChanged(currentItem, out wasUpdated);
                                _CurrentPlacedIntoCache = currentItem;
							}
						}
						catch
						{
						}
					}
				}
				if (bucket.Length > 5)
				{
					_IsDirty = (bool)bucket[5];
				}
				if (bucket.Length > 7)
				{
					_Items.Version = (int)bucket[7];
				}
                if (bucket.Length > 8)
                {
                    LoadErrors(bucket[8] as PXUIErrorInfo[]);
                }
				ClearSessionUnmodified(_Items);
			}
		}

        private void TryGetUnmodified(TNode item, out TNode outItem, out PXEntryStatus? status)
        {
            BqlTablePair pair = null;
            if (item != null
                && _Originals.TryGetValue(item, out pair)
                && pair.SessionUnmodified != null)
            {
                if (pair.SessionUnmodified.Item != null)
                {
                    outItem = (TNode)pair.SessionUnmodified.Item;
                    status = pair.SessionUnmodified.Status;
                }
                else
                {
                    outItem = null;
                    status = null;
                }
            }
            else
            {
                outItem = item;
                status = GetStatus(outItem);
            }
        }

        private void GetUnmodified(IEnumerable<TNode> items,
            out IEnumerable<TNode> updated,
            out IEnumerable<TNode> inserted,
            out IEnumerable<TNode> deleted,
            out IEnumerable<TNode> held)
        {
            var updatedList = new List<TNode>();
            var insertedList = new List<TNode>();
            var deletedList = new List<TNode>();
            var heldList = new List<TNode>();

            foreach (var item in items)
            {
                TNode restored;
                PXEntryStatus? status;
                TryGetUnmodified(item, out restored, out status);
                if (restored != null)
                {
                    switch (status.Value)
                    {
                        case PXEntryStatus.Updated:
                        case PXEntryStatus.Modified:
                            updatedList.Add(restored);
                            break;
                        case PXEntryStatus.Inserted:
                            insertedList.Add(restored);
                            break;
                        case PXEntryStatus.Deleted:
                            deletedList.Add(restored);
                            break;
                        case PXEntryStatus.Held:
                            heldList.Add(restored);
                            break;
                    }
                }
            }

            updated = updatedList;
            inserted = insertedList;
            deleted = deletedList;
            held = heldList;
        }

        internal override void ResetToUnmodified()
        {
            GetUnmodified(_Items.Cached,
                out IEnumerable<TNode> updated,
                out IEnumerable<TNode> inserted,
                out IEnumerable<TNode> deleted,
                out IEnumerable<TNode> held);

            TryGetUnmodified((TNode)_Current,
                out TNode current,
                out PXEntryStatus? status);

            Clear();

            foreach (TNode item in updated)
                _Items.PlaceUpdated(item, true);
            foreach (TNode item in inserted)
                _Items.PlaceInserted(item, out bool del);
            foreach (TNode item in deleted)
                _Items.PlaceDeleted(item, true);
            foreach (TNode item in held)
            {
                _Items.PlaceNotChanged(item, out bool wasUpdated);
                _Items.SetStatus(item, PXEntryStatus.Held);
            }

            _Current = current;
        }

        private void LoadErrors(PXUIErrorInfo[] errors)
        {
            if (errors != null && errors.Length > 0)
            {
                foreach (PXUIErrorInfo errorInfo in errors)
                {
                    if (errorInfo.ErrorLevel == PXErrorLevel.Error)
                    {
                        PXUIFieldAttribute.SetError(this, errorInfo.CacheItem, errorInfo.FieldName, errorInfo.ErrorText, errorInfo.ErrorValue);
                    }
                    else
                    {
                        PXUIFieldAttribute.SetWarning(this, errorInfo.CacheItem, errorInfo.FieldName, errorInfo.ErrorText);
                    }
                }
            }
        }

        /// <summary>Clears the cache completely.</summary>
        /// <example>
        /// 	<code title="Example" description="The following example demonstrates how to clear all POReceipt data records from the cache." lang="CS">
        /// // Declaration of a data view in a graph
        /// public PXSelect&lt;POReceipt&gt; poreceiptslist;
        /// ...
        /// // Clearing the cache of POReceipt data records 
        /// poreceiptslist.Cache.Clear();</code>
        /// </example>
		public override void Clear()
		{
            if (this.DisableCacheClear)
				return;

			//ClearSession();
			_Current = null;
			_CurrentPlacedIntoCache = null;

			_Items = new PXCollection<TNode>(this);
			//if (_AlteredFields != null)
			//{
			//    _AlteredFields.Clear();
			//}
			//_EventsRowItem = null;
			//_CommandPreparingEventsItem = null;
			//_FieldDefaultingEventsItem = null;
			//_FieldUpdatingEventsItem = null;
			//_FieldVerifyingEventsItem = null;
			//_FieldUpdatedEventsItem = null;
			//_FieldSelectingEventsItem = null;
			//_ExceptionHandlingEventsItem = null;
			_ItemAttributes = null;
			_IsDirty = false;
			_ChangedKeys = null;

		}
		/// <exclude/>
		public override void ClearItemAttributes()
		{
			_ItemAttributes = null;

			//_EventsRowItem = null;
			//_CommandPreparingEventsItem = null;
			//_FieldDefaultingEventsItem = null;
			//_FieldUpdatingEventsItem = null;
			//_FieldVerifyingEventsItem = null;
			//_FieldUpdatedEventsItem = null;
			//_FieldSelectingEventsItem = null;
			//_ExceptionHandlingEventsItem = null;
		}
        /// <exclude/>
        public override void TrimItemAttributes(object item)
		{
			if (item != null && _ItemAttributes != null && _ItemAttributes.ContainsKey(item))
			{
				if (_Items == null || !_Items.Contains((TNode)item))
					_ItemAttributes.Remove(item);
			}
		}
		internal override bool HasChangedKeys()
		{
			return _ChangedKeys != null && _ChangedKeys.Count > 0;
		}
		internal override void ClearSession()
		{
			if (PXContext.Session.IsSessionEnabled)
			{
				string key = Graph.StatePrefix + String.Format("{0}${1}", _Graph.GetType().FullName, typeof(TNode).FullName);
				PXContext.Session.Remove(key);
			}
		}

        /// <summary>Serializes cache to the session.</summary>
		public override void Unload()
		{
			if (!AutoSave)
			{
				if (PXContext.Session.IsSessionEnabled)
				{
                    IEnumerable<TNode> updated = new List<TNode>(_Items.Updated);
                    IEnumerable<TNode> inserted = new List<TNode>(_Items.Inserted);
                    IEnumerable<TNode> deleted = new List<TNode>(_Items.Deleted);
                    IEnumerable<TNode> held = new List<TNode>(_Items.Held);
                    List<PXUIErrorInfo> errorInfo = new List<PXUIErrorInfo>();

                    if (Graph.PreserveErrorInfo 
                        || this.Graph.IsMobile 
                        && (updated.Count() > 0 || inserted.Count() > 0) 
                        && this.Graph.PrimaryItemType != null
                        && Graph.Caches[this.Graph.PrimaryItemType] == this)
                    {
                        HoldNotChangedRows();
                        errorInfo = CollectErrorInfo(Graph.PreserveErrorInfo);
                    }

					string key = Graph.StatePrefix + String.Format("{0}${1}", _Graph.GetType().FullName, typeof(TNode).FullName);
					if (updated.Count() > 0 
                        || inserted.Count() > 0 
                        || deleted.Count() > 0 
                        || held.Count() > 0 
                        || _Current != null 
                        || _IsDirty 
                        || _Items.Version > 0)
						{
							PXContext.Session.CacheInfo[key] = new object[]
							{
								updated.ToArray(),
								inserted.ToArray(),
								deleted.ToArray(),
								held.ToArray(),
								_Current as TNode,
								_IsDirty,
								_Current is TNode && _Items.Locate((TNode)_Current) != null && _Items.GetStatus((TNode)_Current) != PXEntryStatus.InsertedDeleted,
								_Items.Version,
								errorInfo.ToArray()
							};

							if (PXContext.Session.CacheInfo[PXGraph.CacheStateOrder] == null)
								PXContext.Session.CacheInfo[PXGraph.CacheStateOrder] = new object[1] { new Dictionary<string, int>() };
							Dictionary<string, int> cacheOrder = (Dictionary<string, int>)PXContext.Session.CacheInfo[PXGraph.CacheStateOrder][0];

							if (!cacheOrder.ContainsKey(key))
								cacheOrder[key] = cacheOrder.Count;
						}
						else
						{
						PXContext.Session.Remove(key);
					}
				}
			}
			else
			{
				using (PXTransactionScope tscope = new PXTransactionScope())
				{
					Persist(PXDBOperation.Insert);
					Persist(PXDBOperation.Update);
					Persist(PXDBOperation.Delete);
					tscope.Complete();
				}
			}
			_CurrentPlacedIntoCache = null;
		}
        #endregion

        private List<PXUIErrorInfo> CollectErrorInfo(bool collectall)
        {
            List<PXUIErrorInfo> errors = new List<PXUIErrorInfo>();

			if (collectall)
			{
            foreach (string field in Fields)
            {
                foreach (object item in Cached)
                {
                    IPXInterfaceField interfaceItemField = GetAttributesReadonly(item, field)
                                                           .OfType<IPXInterfaceField>()
                                                           .FirstOrDefault();
                    if (interfaceItemField != null && !string.IsNullOrEmpty(interfaceItemField.ErrorText))
                    {
                        errors.Add(new PXUIErrorInfo(field, item, interfaceItemField.ErrorText, interfaceItemField.ErrorLevel, interfaceItemField.ErrorValue));
                    }
                }
            }
			}
			else if (_ItemAttributes?.Count > 0)
			{
				foreach (KeyValuePair<object, DirtyItemState> pair in _ItemAttributes)
				{
					PXEventSubscriberAttribute[] list = pair.Value.DirtyAttributes;
					PXEntryStatus status;
					if (list != null && ((status = GetStatus(pair.Key)) == PXEntryStatus.Inserted || status == PXEntryStatus.Updated))
					{
						foreach (PXEventSubscriberAttribute attr in pair.Value.DirtyAttributes)
						{
							if (attr is IPXInterfaceField && !String.IsNullOrEmpty(((IPXInterfaceField)attr).ErrorText))
							{
								errors.Add(new PXUIErrorInfo(attr.FieldName, pair.Key, ((IPXInterfaceField)attr).ErrorText, ((IPXInterfaceField)attr).ErrorLevel, ((IPXInterfaceField)attr).ErrorValue));
							}
						}
					}
				}
			}

            return errors;
        }
        private void HoldNotChangedRows()
        {
            foreach (object item in Cached)
            {
                this.Hold(item);
            }
        }

		private PXLockViolationException GetLockViolationException(TNode row, PXDBOperation operation)
		{
			var keyFields = Keys.ToArray();
			var keyValues = getKeys(row);
			var deletedDatabaseRecord = PXDatabase.IsDeletedDatabaseRecord(_BqlTable, keyFields, keyValues, out var keys);

			return new PXLockViolationException(_BqlTable, operation, keys, deletedDatabaseRecord);
		}

		private PXLockViolationException GetLockViolationException(PXDataFieldParam[] parameters, PXDBOperation operation)
		{
			var deletedDatabaseRecord = PXDatabase.IsDeletedDatabaseRecord(_BqlTable, parameters, out var keyValues);

			return new PXLockViolationException(_BqlTable, operation, keyValues, deletedDatabaseRecord);
		}

		#region Debugger support
		/// <summary>Returns the string representing the current cache
		/// object.</summary>
		public override string ToString()
        {
            //throw new Exception();
			return String.Format("PXCache<{0}>({1})", typeof(TNode).FullName, _Items.CachedCount);
        }

        /// <exclude/>
        internal class PXCacheDebugView
        {
            

            private readonly PXCache<TNode> Src;
            public PXCacheDebugView(PXCache<TNode> src)
            {
                Src = src;

               
            }


            public TNode Current { get { return (TNode)Src.Current; } }
            public TNode[] Inserted { get { return Src.Inserted.OfType<TNode>().ToArray(); } }
            public TNode[] Updated { get { return Src.Updated.OfType<TNode>().ToArray(); } }
            public TNode[] Deleted { get { return Src.Deleted.OfType<TNode>().ToArray(); } }
            public string[] Fields { get { return Src.Fields.ToArray(); } }
            public string[] BqlFields { get { return Src.BqlFields.Select(_ => _.Name).ToArray(); } }
             
        }
           
        #endregion
	}
}
