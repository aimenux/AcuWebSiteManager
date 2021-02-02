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

namespace PX.Data
{
	/// <summary>Represents the cache of modified data records from a
	/// paricular table and the controller for basic operations over these
	/// data records. The type parameter is set to the data access class (DAC)
	/// that represents this table.</summary>
	/// <typeparam name="TNode">The DAC type of data records stored in
	/// the cache object.</typeparam>
	/// <remarks>
	/// <para>The cache objects consists conceptually of two parts:</para>
	/// <list type="bullet">
	/// <item><description>The collections of the data
	/// records that were modified and not yet saved to the database, such as
	/// <see cref="PXCache{T}.Updated">Updated</see>, <see cref="PXCache{T}.Inserted">Inserted</see>,
	/// <see cref="PXCache{T}.Deleted">Deleted</see>, and <see cref="PXCache{T}.Dirty">Dirty</see>.
	/// </description></item>
	/// <item><description>The controller that
	/// executes basic data-related operations through the use of the methods,
	/// such as <see cref="PXCache{T}.Update(object)">Update(object)</see>,
	/// <see cref="PXCache{T}.Insert(object)">Insert(object)</see>,
	/// <see cref="PXCache{T}.Delete(object)">Delete(object)</see>,
	/// <see cref="PXCache{T}.Persist(PXDBOperation)">Persist(PXDBOperation)</see>,
	/// and other methods.</description></item>
	/// </list>
	/// <para>During execution of these methods, the cache raises events. The
	/// graph and attributes can subscribe to these events to implement
	/// business logic. The methods applied to a previously unchanged data
	/// record result in placing of the data record into the cache.</para>
	/// <para>The system creates and destroys <tt>PXCache</tt> instances
	/// (caches) on each request. If the user or the code modifies a data
	/// record, it is placed into the cache. When request execution is
	/// completed, the system serializes the modified records from the caches
	/// to the session. At run time, the cache may also include the unchanged
	/// data records retrieved during request execution. These data records
	/// are discarded once the request is served.</para>
	/// <para>On the next round trip, the modified data records are loaded
	/// from the session to the caches. The cache merges the data retrieved
	/// from the database with the modified data, and the application accesses
	/// the data as if the entire data set has been preserved from the time of
	/// previous request.</para>
	/// <para>The cache maintains the modified data until the changes are
	/// discarded or saved to the database.</para>
	/// <para>The cache is the issuer of all data-related events, which can be
	/// handled by the graph and attributes.</para>
	/// </remarks>
	/// <example>
	/// An instance of <tt>PXCache&lt;&gt;</tt> type, the cache object that initiated
	/// the event, is passed to every event handler as the first argument.
	/// <code>
	/// protected virtual void Vendor_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
	/// {
	///     Vendor row = e.Row as Vendor;
	///     ...
	/// }
	/// </code>
	/// </example>
	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	//[DebuggerDisplay("{ToString()}")]
	[DebuggerTypeProxy(typeof(PXCache<>.PXCacheDebugView))]
	public partial class PXModelExtension<TNode> : PXCache
		where TNode : PXMappedCacheExtension, IBqlTable, new()
	{
		#region Events
		internal override object _Clone(object item)
		{
			return Cache._Clone(getBaseClass(item));
		}

		/// <summary>Checks if the provided data record has any attributes
		/// attached to its fields.</summary>
		/// <param name="data">The data record.</param>
		public override bool HasAttributes(object data)
		{
			return Cache.HasAttributes(getBaseClass(data));
		}

		/// <summary>Returns the cache-level instances of attributes placed on the
		/// specified field in the DAC.</summary>
		/// <remarks>The system maintains instances of attributes on three different
		/// levels. On its instantiation, a cache object copies appropriate
		/// attributes from the global level to the cache level and stores them in
		/// an internal collection. When an attribute needs to be modified for a
		/// particular data record, the cache creates item-level copies of all
		/// attributes and stores them associated with the data record.</remarks>
		/// <param name="name">The name of the field whose attributes are
		/// returned. If <tt>null</tt>, the method returns attributes from all
		/// fields.</param>
		public override List<PXEventSubscriberAttribute> GetAttributesReadonly(string name)
		{
			return Cache.GetAttributesReadonly(tryRemapField(name));
		}

		/// <summary>Returns the cache-level instances of attributes placed on the
		/// specified field in the DAC.</summary>
		/// <remarks>Using this method, you can prevent expanding the aggregate
		/// attributes by setting the second parameter to <tt>false</tt>. Other
		/// overloads of this method always include both the aggregate attributes
		/// and the attributes that comprise such attributes.</remarks>
		/// <param name="name">The data record.</param>
		/// <param name="extractEmmbeddedAttr">The value that indicates whether
		/// the attributes embedded into an aggregate attribute are included into
		/// the list. If <tt>true</tt>, both the aggregate attribute and the
		/// attributes embedded into it are included in the list. Otherwise, only
		/// the aggregate attribute is included. An aggregate attribute is an
		/// attribute that derives from the <tt>PXAggregateAttribute</tt> class.
		/// This class allows combining multiple different attributes in a single
		/// one.</param>
		public override List<PXEventSubscriberAttribute> GetAttributesReadonly(string name, bool extractEmmbeddedAttr)
		{
			return Cache.GetAttributesReadonly(tryRemapField(name), extractEmmbeddedAttr);
		}

		/// <summary>Returns the item-level attribute instances placed on the
		/// specified field, if such instances exist for the provided data record,
		/// or the cache-level instances, otherwise.</summary>
		/// <param name="data">The data record.</param>
		/// <param name="name">The name of the field whose attributes are
		/// returned. If <tt>null</tt>, the method returns attributes from all
		/// fields.</param>
		/// <example>
		/// The code below gets the attributes and places them into a list.
		/// <code>
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
			return Cache.GetAttributesReadonly(getBaseClass(data), tryRemapField(name));
		}

		/// <summary>Returns the item-level instances of attributes placed on the
		/// specified field. If such instances are not exist for the provided data
		/// record, the method creates them by copying all cache-level attributes
		/// and storing them in the internal collection that contains the data
		/// record specific attributes. To avoid cloning cache-level attributes,
		/// use the <see cref="PXCache{T}.GetAttributesReadonly(object,string)">GetAttributesReadonly(object,
		/// string)</see> method.</summary>
		/// <param name="data">The data record.</param>
		/// <param name="name">The name of the field whose attributes are
		/// returned. If <tt>null</tt>, the method returns attributes from all
		/// fields.</param>
		public override IEnumerable<PXEventSubscriberAttribute> GetAttributes(object data, string name)
		{
			return Cache.GetAttributes(getBaseClass(data), tryRemapField(name));
		}

        /// <exclude />
		public override IEnumerable<T> GetAttributesOfType<T>(object data, string name)
		{
			return Cache.GetAttributesOfType<T>(getBaseClass(data), tryRemapField(name));
		}

        /// <exclude />
		public override List<Type> GetExtensionTables()
		{
			return Cache.GetExtensionTables();
		}

		/// <summary>Returns the cach-level instances of attributes placed on the
		/// specified field and all item-level instances currently stored in the
		/// cache.</summary>
		/// <param name="name">The name of the field whose attributes are
		/// returned. If <tt>null</tt>, the method returns attributes from all
		/// fields.</param>
		public override List<PXEventSubscriberAttribute> GetAttributes(string name)
		{
			return Cache.GetAttributes(tryRemapField(name));
		}
		#endregion

		#region Field Manipulation

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
			Cache.SetValue(getBaseClass(data), ordinal, value);
		}
		/// <summary>Sets the value of the field in the provided data record
		/// without raising events.</summary>
		/// <remarks>To set the value, raising the field-related events, use the <see
		/// cref="PXCache{T}.SetValueExt(object,string,object)">SetValueExt(object,
		/// string, object)</see> method.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field that is set to the
		/// value.</param>
		/// <param name="value">The value to set to the field.</param>
		/// <example>
		/// <code>
		/// public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		/// {
		///     bool freeze = ((bool?)sender.GetValue(e.Row, sender.GetField(freezeDisc))) == true;
		///     
		///     if (!freeze &amp;&amp; !sender.Graph.IsImport)
		///     {
		///         IDiscountable row = (IDiscountable)e.Row;
		/// 
		///         if (row.CuryDiscAmt != null &amp;&amp; row.CuryDiscAmt != 0 &amp;&amp; row.CuryExtPrice != 0)
		///         {
		///             row.DiscPct = 100 * row.CuryDiscAmt / row.CuryExtPrice;
		///             sender.SetValue(row, sender.GetField(curyTranAmt), row.CuryExtPrice - row.CuryDiscAmt);
		///             sender.SetValue(e.Row, this.FieldName, true);
		///         }
		///         ...
		///     }
		/// }
		/// </code>
		/// </example>
		public override void SetValue(object data, string fieldName, object value)
		{
			Cache.SetValue(getBaseClass(data), tryRemapField(fieldName), value);
		}

		internal override object GetCopy(object data)
		{
			return Cache.GetCopy(getBaseClass(data));
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
			Cache.SetDefaultExt(getBaseClass(data), tryRemapField(fieldName), value);
		}
		/// <summary>Sets the value of the field in the provided data
		/// record.</summary>
		/// <remarks>The method raises the <tt>FieldUpdating</tt>,
		/// <tt>FieldVerifying</tt>, and <tt>FieldUpdated</tt> events. To set the
		/// value to the field without raising events, use the <see
		/// cref="PXCache{T}.SetValue(object,string,object)">SetValue(object,
		/// string, object)</see> method.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field that is set to the
		/// value.</param>
		/// <param name="value">The value to set to the field.</param>
		public override void SetValueExt(object data, string fieldName, object value)
		{
			Cache.SetValueExt(getBaseClass(data), tryRemapField(fieldName), value);
		}

		/// <summary>Returns the value of the specified field in the given data
		/// record without raising any events. The field is specified by its
		/// index&#8212;see the <see
		/// cref="PXCache{T}.GetFieldOrdinal(string)">GetFieldOrdinal(string)</see>
		/// method.</summary>
		/// <param name="data">The data record.</param>
		/// <param name="ordinal">The index of the field whose value is
		/// returned.</param>
		public override object GetValue(object data, int ordinal)
		{
			return Cache.GetValue(getBaseClass(data), ordinal);
		}
		/// <summary>Returns the value of the specified field in the given data
		/// record without raising any events.</summary>
		/// <remarks>
		/// <para>To get the field of a data record of a known DAC type, you can
		/// use DAC properties. If a type of a data record is unknown (for
		/// example, when it is available as <tt>object</tt>), you can use the
		/// <tt>GetValue()</tt> methods to get a value of a field. These methods
		/// can also be used to get values of fields defined in extensions
		/// (another way is to get the extension data record through the <see
		/// cref="PXCache{T}.GetExtension{E}(T)">GetExtension&lt;&gt;()</see>
		/// method).</para>
		/// <para>The <see
		/// cref="PXCache{T}.GetValuExt(object,string)">GetValueExt()</see>
		/// methods are used to get the value or the field state object and raise
		/// events.</para>
		/// </remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field whose value is
		/// returned.</param>
		/// <example>
		/// The code below iterates over all fields of a specific DAC (including
		/// fields defined in extensions) and checks whether a value is null.
		/// <code>
		/// foreach (string field in sender.Fields)
		/// {
		///     if (sender.GetValue(row, field) == null)
		///         ...
		/// }</code>
		/// </example>
		public override object GetValue(object data, string fieldName)
		{
			return Cache.GetValue(getBaseClass(data), tryRemapField(fieldName));
		}

		/// <exclude/>
		public override object GetValueInt(object data, string fieldName)
		{
			return Cache.GetValueInt(getBaseClass(data), tryRemapField(fieldName));
		}
		/// <exclude/>
		internal override object GetStateInt(object data, string fieldName)
		{
			return Cache.GetStateInt(getBaseClass(data), tryRemapField(fieldName));
		}
		/// <summary>Returns the value or the <tt>PXFieldState</tt> object of the
		/// specified field in the given data record. The <tt>PXFieldState</tt>
		/// object is returned if the field is in the <tt>AlteredFields</tt>
		/// collection.</summary>
		/// <remarks>The method raises the <tt>FieldSelecting</tt> event.</remarks>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field whose value or
		/// <tt>PXFieldState</tt> object is returned.</param>
		/// <returns>The field value or <tt>PXFieldState</tt> object.</returns>
		/// <example>
		/// <code>
		/// string name = _MapErrorTo.Name;
		/// name = char.ToUpper(name[0]) + name.Substring(1);
		/// object val = sender.GetValueExt(e.Row, name);
		/// if (val is PXFieldState)
		/// {
		///     val = ((PXFieldState)val).Value;
		/// }
		/// </code>
		/// </example>
		public override object GetValueExt(object data, string fieldName)
		{
			return Cache.GetValueExt(getBaseClass(data), tryRemapField(fieldName));
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
			return Cache.GetValueInt(getBaseClass(data), tryRemapField(fieldName));
		}
		/// <summary>Returns the value of the specified field for the data record
		/// as it is stored in the database.</summary>
		/// <param name="data">The data record.</param>
		/// <param name="fieldName">The name of the field whose original value is
		/// returned.</param>
		public override object GetValueOriginal(object data, string fieldName)
		{
			return Cache.GetValueOriginal(getBaseClass(data), tryRemapField(fieldName));
		}
        ///<exclude/>
		public override object GetOriginal(object data)
		{
			return Cache.GetOriginal(getBaseClass(data));
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
			return Cache.GetValuePending(getBaseClass(data), tryRemapField(fieldName));
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
			Cache.SetValuePending(getBaseClass(data), tryRemapField(fieldName), value);
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
			Cache.RestoreCopy(getBaseClass(item), getBaseClass(copy));
		}

		private List<object> _copiesStrongRef;
		/// <summary>Creates a clone of the provided data record by initializing a
		/// new data record with the field values get from the provided data
		/// record.</summary>
		/// <param name="item">The data record to copy.</param>
		/// <example>
		/// The code creates a copy of an <tt>Address</tt> data record, modifies
		/// it, and inserts in the cache.
		/// <code>
		/// Address addr = PXCache&lt;Address&gt;.CreateCopy(defAddress);
		/// addr.AddressID = null;
		/// addr.BAccountID = owner.BAccountID;
		/// addr = this.RemitAddress.Insert(addr);
		/// </code>
		/// </example>
		public override object CreateCopy(object item)
		{
			if (item != null)
			{
				var baseCopy = Cache.CreateCopy(getBaseClass(item));
				_copiesStrongRef = _copiesStrongRef ?? new List<object>();
				_copiesStrongRef.Add(baseCopy);
				return Cache.GetExtension<TNode>(baseCopy);
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
			return Cache.ToDictionary(getBaseClass(data));
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
			return Cache.ToXml(getBaseClass(data));
		}

		/// <summary>Initializes the data record from the provided XML
		/// string.</summary>
		/// <remarks>The data record is represented in the XML by the
		/// <i>&lt;Row&gt;</i> element with the <i>type</i> attribute set to the
		/// DAC name. Each field is represented by the <i>&lt;Field&gt;</i>
		/// element with the <i>name</i> attribute holding the field name and the
		/// <i>value</i> attribute holding the field value.</remarks>
		/// <param name="xml">The XML string to parse.</param>
		public override object FromXml(string xml)
		{
			return Cache.GetExtension<TNode>(Cache.FromXml(xml));
		}

		/// <summary>Converts the provided value of the field to string and
		/// returns the resulting value. No events are raised.</summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="val">The field value.</param>
		public override string ValueToString(string fieldName, object val)
		{
			return Cache.ValueToString(tryRemapField(fieldName), val);
		}

		/// <summary>Converts the provided value of the field from a string to the
		/// appropriate type and returns the resulting value. No events are
		/// raised.</summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="val">The string representation of the field
		/// value.</param>
		public override object ValueFromString(string fieldName, string val)
		{
			return Cache.ValueFromString(tryRemapField(fieldName), val);
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
				return Cache.Fields;
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
				return Cache.BqlFields;
			}
		}

		/// <summary>Gets the collection of BQL types that correspond to the key
		/// fields which the DAC defines.</summary>
		public override List<Type> BqlKeys
		{
			get
			{
				return Cache.BqlKeys;
			}
		}

		/// <exclude/>
		public override List<Type> BqlImmutables
		{
			get
			{
				return Cache.BqlImmutables;
			}
		}

		/// <exclude/>
		public override BqlCommand BqlSelect
		{
			get
			{
				return Cache.BqlSelect;
			}
			set
			{
				Cache.BqlSelect = value;
			}
		}

		/// <summary>Gets the DAC the cache is associated with. The DAC is
		/// specified through the type parameter when the cache is
		/// instantiated.</summary>
		public override Type BqlTable
		{
			get
			{
				return Cache.BqlTable;
			}
		}

		public override Type GenericParameter { get { return typeof(TNode); } }

		internal override Type GetFieldType(string fieldName)
		{
			return Cache.GetFieldType(tryRemapField(fieldName));
		}

		internal override Type GetBaseBqlField(string field)
		{
			return Cache.GetBaseBqlField(tryRemapField(field));
		}

		/// <exclude/>
		public override Type[] GetExtensionTypes()
		{
			return Cache.GetExtensionTypes();
		}
       
        #endregion

        #region Object
        /// <summary>Compares two data records by the key fields. Returns
        /// <tt>true</tt> if the values of all key fields in the data records are
        /// equal. Otherwise, returns <tt>false</tt>.</summary>
        /// <param name="a">The first data record to compare.</param>
        /// <param name="b">The second data record to compare.</param>
        public override bool ObjectsEqual(object a, object b)
		{
			return Cache.ObjectsEqual(getBaseClass(a), getBaseClass(b));
		}
		/// <summary>Returns the hash code generated from key field
		/// values.</summary>
		/// <param name="data">The data record.</param>
		public override int GetObjectHashCode(object data)
		{
			return Cache.GetObjectHashCode(getBaseClass(data));
		}
		/// <summary>Returns a string of key fields and their values in the
		/// <i>{key1=value1, key2=value2}</i> format.</summary>
		/// <param name="data">The data record which key fields are written to a
		/// string.</param>
		public override string ObjectToString(object data)
		{
			return Cache.ObjectToString(getBaseClass(data));
		}
		#endregion

		#region Schema
		/// <summary>Gets the instance of the DAC extension of the specified type.
		/// The extension type is specified as the type parameter.</summary>
		/// <param name="item">The standard data record whose extension is
		/// returned.</param>
		/// <example>
		/// The code below gets an extension data record corresponding to the
		/// given instance of the base data record.
		/// <code>
		/// InventoryItem item = cache.Current as InventoryItem;
		/// InventoryItemExtension itemExt = 
		///     cache.GetExtension&lt;InventoryItemExtension&gt;(item);</code>
		/// </example>
		public override Extension GetExtension<Extension>(object item)
		{
            object baseObject = getBaseClass(item);
            PXCacheExtension ext;
            if (typeof(Extension).IsAssignableFrom(typeof(TNode)))
                ext = Cache.GetExtension<TNode>(baseObject);
            else
                ext = Cache.GetExtension<Extension>(baseObject);
            return (Extension)ext;
        }
        /// <exclude />
		public override object GetMain<Extension>(Extension item)
		{
			return getBaseClass(item);
		}

		internal override PXCacheExtension[] GetCacheExtensions(IBqlTable item)
		{
			return Cache.GetCacheExtensions(getBaseClass(item) as IBqlTable);
		}

		/// <summary>Returns the DAC type of the data records in the
		/// cache.</summary>
		public override Type GetItemType()
		{
			return typeof(TNode);
		}

		/// <exclude/>
		public override PXDBInterceptorAttribute Interceptor
		{
			get
			{
				return Cache.Interceptor;
			}
			set
			{
				Cache.Interceptor = value;
			}
		}
        #endregion

        #region Cache
        protected internal override void SetSessionUnmodified(object item, object unmodified)
        {
            Cache.SetSessionUnmodified(getBaseClass(item), getBaseClass(unmodified));
        }

        /// <exclude />
        public override Type GetBqlField(string field)
		{
			return Cache.GetBqlField(field);
		}

		/// <summary>Returns the number of fields and virtual fields which
		/// comprise the <tt>Fields</tt> collection.</summary>
		public override int GetFieldCount()
		{
			return Cache.GetFieldCount();
		}
		/// <summary>Returns the index of the specified field in the internally
		/// kept fields map. The pare</summary>
		public override int GetFieldOrdinal<Field>()
		{
			return Cache.GetFieldOrdinal<Field>();
		}
		/// <summary>Returns the index of the specified field in the internally
		/// kept fields map.</summary>
		/// <param name="field">The name of the field whose index is
		/// returned.</param>
		public override int GetFieldOrdinal(string field)
		{
			return Cache.GetFieldOrdinal(field);
		}
		/// <summary>Recalculates internally stored hash codes. The method should
		/// be called after a key field is modified in a data record from the
		/// cache.</summary>
		public override void Normalize()
		{
			Cache.Normalize();
		}

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
				return Cache.Current != null ? Cache.GetExtension<TNode>(Cache.Current) : null;
			}
			set
			{
				Cache.Current = ((TNode)value)._Base.Target;
			}
		}

		/// <exclude/>
		public override object InternalCurrent
		{
			get { return Cache.GetExtension<TNode>(Cache.InternalCurrent); }
		}
		#endregion

		#region Ctor

		static PXModelExtension()
		{
		}

        private Lazy<PXCache> _cache;
		private PXCache Cache => _cache.Value;
		private Dictionary<string, string> FieldRemapping;

		private string tryRemapField(string fieldName)
		{
			string ret;
			if (fieldName != null && FieldRemapping.TryGetValue(fieldName, out ret))
			{
				return ret;
			}
			return fieldName;
		}

		private static object getBaseClass(object data)
		{
			object ret = data;
			if (data is PXMappedCacheExtension && ((PXMappedCacheExtension)data)._Base != null)
			{
				ret = ((PXMappedCacheExtension)data)._Base.Target;
			}
			return ret;
		}

		/// <exclude/>
		public PXModelExtension(PXGraph graph, IBqlMapping map)
		{
		    _Graph = graph;
			_cache = new Lazy<PXCache>(()=> graph.Caches[map.Table]);
			FieldRemapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (FieldInfo fi in map.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (fi.FieldType == typeof(Type))
				{
					string from = fi.Name;
					string to = ((Type)fi.GetValue(map)).Name;
					FieldRemapping[from] = to;
				}
			}
		}

		#endregion

		#region Data manipulation methods
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
			return Cache.GetExtension<TNode>(Cache.Select(record, ref position, isReadOnly, out wasUpdated));
		}

		internal override object CreateItem(PXDataRecord record, ref int position, bool isReadOnly)
		{
			return Cache.GetExtension<TNode>(Cache.CreateItem(record, ref position, isReadOnly));
		}
		internal override object Select(object item, bool isReadOnly, out bool wasUpdated)
		{
			return Cache.GetExtension<TNode>(Cache.Select(item, isReadOnly, out wasUpdated));
		}


		/// <summary>Returns the status of the provided data record. The <see
		/// cref="PXEntryStatus">PXEntryStatus</see> enumeration
		/// defines the possible status values. For example, the status can
		/// indicate whether the data record has been inserted, updated, or
		/// deleted.</summary>
		/// <param name="item">The data record whose status is requested.</param>
		/// <example>
		/// The code below shows how a status of a data record can be checked in
		/// an event handler.
		/// <code>
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
			return Cache.GetStatus(getBaseClass(item));
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
			Cache.SetStatus(getBaseClass(item), status);
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
			return Cache.GetExtension<TNode>(Cache.Locate(getBaseClass(item)));
		}

        /// <exclude />
        public object UpdateExtensionMapping(object extension)
        {
            return UpdateExtensionMapping(extension, MappingSyncDirection.ExtensionToBase);
        }
        /// <exclude />
        public object UpdateExtensionMapping(object extension, MappingSyncDirection direction)
        {
            object updated = null;
            if (extension is PXMappedCacheExtension && ((PXMappedCacheExtension)extension)._Base != null)
            {
                updated = ((PXMappedCacheExtension)extension)._Base.Target;
                UpdateExtensionMapping(extension, updated, direction);

            }
            return updated;
        }

        private void UpdateExtensionMapping(object extension, object baseObject)
        {
            UpdateExtensionMapping(extension, baseObject, MappingSyncDirection.ExtensionToBase);
        }

        private void UpdateExtensionMapping(object extension, object baseObject, MappingSyncDirection direction)
        {
            if (extension == null || baseObject == null)
                return;
            foreach (var kv in FieldRemapping)
            {
                string extField = kv.Key;
                string origField = kv.Value;
                //TODO: optimize
                SetValue(baseObject, origField,
                    direction == MappingSyncDirection.ExtensionToBase
                        ? typeof(TNode).GetProperty(extField)?.GetValue(extension, null)
                        : Cache.GetValue(baseObject, origField));
            }
        }


        protected internal override bool IsPresent(object item)
		{
			return Cache.IsPresent(getBaseClass(item));
		}
		protected internal override bool IsGraphSpecificField(string fieldName)
		{
			return Cache.IsGraphSpecificField(tryRemapField(fieldName));
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
			return Cache.Locate(keys);
		}

        /// <exclude />
		public override int LocateByNoteID(Guid noteId)
		{
			return Cache.LocateByNoteID(noteId);
		}

		/// <summary>Completely removes the provided data record from the cache
		/// without raising any events.</summary>
		/// <param name="item">The data record to remove from the cache.</param>
		/// <example>
		/// The code below locates a data record in the cache and, if the data
		/// record has not been changed, silently removes it from the cache.
		/// (The <tt>Held</tt> status indicates that a data record has not been
		/// changed but needs to the preserved in the session.
		/// <code>
		/// // Searching the data record by its key fields in the cache
		/// object cached = sender.Locate(item);
		/// 
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
			Cache.Remove(getBaseClass(item));
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
			return 0;
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
            var baseObject = getBaseClass(data);
            if(baseObject != data)
                UpdateExtensionMapping(data, baseObject);
            return Cache.GetExtension<TNode>(Cache.Update(baseObject));
		}

		protected internal override object Update(object data, bool bypassinterceptor)
		{
			return Cache.GetExtension<TNode>(Cache.Update(getBaseClass(data), bypassinterceptor));
		}

		protected internal override void UpdateLastModified(object item, bool inserted)
		{
			Cache.UpdateLastModified(getBaseClass(item), inserted);
		}

		/// <summary>Initializes a new data record using the provided field values
		/// and inserts the data record into the cache. Returns 1 in case of
		/// successful insertion, and 0 otherwise.</summary>
		/// <remarks>
		/// <para>The method raises the following events:
		/// <tt>FieldDefaulting</tt>, <tt>FieldUpdating</tt>,
		/// <tt>FieldVerifying</tt>, <tt>FieldUpdated</tt>, <tt>RowInserting</tt>,
		/// and <tt>RowInserted</tt>. See <a href="Insert.html">Inserting a Data Record</a> for
		/// the events chart.</para>
		/// <para>The method does not check if the data record exists in the
		/// database. The values provided in the dictionary are not readonly and
		/// can be updated during execution of the method. The method is typically
		/// used by the system when the values are received from the user
		/// interface. If the <tt>AllowInsert</tt> property is <tt>false</tt>, the
		/// data record is not inserted and the method returns 0.</para>
		/// <para>In case of successful insertion, the method marks the data
		/// record as <tt>Inserted</tt>, and it becomes accessible through the
		/// <tt>Inserted</tt> collection.</para>
		/// </remarks>
		/// <param name="values">The dictionary with values to initialize the data
		/// record fields. The dictionary keys are field names.</param>
		public override int Insert(IDictionary values)
		{
			return Cache.Insert(values);
		}

		internal override object FillItem(IDictionary values)
		{
			return Cache.GetExtension<TNode>(Cache.FillItem(values));
		}

		/// <summary>Inserts the provided data record into the cache. Returns the
		/// inserted data record or <tt>null</tt> if the data record wasn't
		/// inserted.</summary>
		/// <remarks>
		/// <para>The method raises the following events:
		/// <tt>FieldDefaulting</tt>, <tt>FieldUpdating</tt>,
		/// <tt>FieldVerifying</tt>, <tt>FieldUpdated</tt>, <tt>RowInserting</tt>,
		/// and <tt>RowInserted</tt>. See <a href="Insert.html">Inserting a Data Record</a> for
		/// the events chart.</para>
		/// <para>The method does not check if the data record exists in the
		/// database. The AllowInsert property does not affect this method unlike
		/// the <see
		/// cref="PXCache{T}.Insert(IDictionary)">Insert(IDictionary)</see>
		/// method.</para>
		/// <para>In case of successful insertion, the method marks the data
		/// record as <tt>Inserted</tt>, and it becomes accessible through the
		/// <tt>Inserted</tt> collection.</para>
		/// </remarks>
		/// <param name="data">The data record to insert into the cache.</param>
		/// <example>
		/// The code below initializes a new instance of the <tt>APInvoice</tt>
		/// data record and inserts it into the cache.
		/// <code>
		/// APInvoice newDoc = new APInvoice();
		/// newDoc.VendorID = Document.Current.VendorID;
		/// Document.Insert(newDoc);</code>
		/// </example>
		public override object Insert(object data)
		{
            var baseObject = getBaseClass(data);
            //TODO: optimize
            if (baseObject == data)
            {
                baseObject = Cache.CreateInstance();
            }
            UpdateExtensionMapping(data, baseObject);

            return Cache.GetExtension<TNode>(Cache.Insert(baseObject));
		}

		protected internal override object Insert(object data, bool bypassinterceptor)
		{
			return Cache.GetExtension<TNode>(Cache.Insert(getBaseClass(data), bypassinterceptor));
		}

		internal override object ToChildEntity<Parent>(Parent item)
		{
			return Cache.ToChildEntity<Parent>(item);
		}

		/// <summary>Initializes a data record of the DAC type of the cache from
		/// the provided data record of the base DAC type and inserts the new data
		/// record into the cache. Returns the inserted data record.</summary>
		/// <param name="item">The data record of the base DAC type which field
		/// values are used to initialize the data record.</param>
		/// <example>
		/// See the <see
		/// cref="PXSelectBase{T}.Extend{Parent}(Parent)">Extend&lt;Parent&gt;(Parent)</see>
		/// method of the <tt>PXSelectBase&lt;&gt;</tt> class.
		/// 
		/// </example>
		public override object Extend<Parent>(Parent item)
		{
			return Cache.Extend<Parent>(item);
		}

		/// <summary>Initializes a new data record with default values and inserts
		/// it into the cache by invoking the <see
		/// cref="PXCache{T}.Insert(object)">Insert(object)</see>
		/// method. Returns the new data record inserted into the cache.</summary>
		public override object Insert()
		{
			return Cache.GetExtension<TNode>(Cache.Insert());
		}

		/// <summary>Returns a new data record of the DAC type of the cache. The
		/// method may be used to initialize a data record of the type appropriate
		/// for the <tt>PXCache</tt> instance when its DAC type is
		/// unknown.</summary>
		public override object CreateInstance()
		{
			return Cache.GetExtension<TNode>(Cache.CreateInstance());
		}

		/// <summary>Clears the internal cache of database query
		/// results.</summary>
		public override void ClearQueryCacheObsolete()
		{
			Cache.ClearQueryCacheObsolete();
		}

		public override void ClearQueryCache()
		{
			Cache.ClearQueryCache();
		}

		/// <summary>Initializes the data record with the provided key values and
		/// places it into the cache with the <tt>Deleted</tt> or
		/// <tt>InsertedDeleted</tt> status. The method assigns the
		/// <tt>InsertedDeleted</tt> status to the data record if it has the
		/// <tt>Inserted</tt> status when the method is invoked.</summary>
		/// <remarks>
		/// <para>The method raises the following events: <tt>FieldUpdating</tt>,
		/// <tt>FieldUpdated</tt>, <tt>RowDeleting</tt>, and <tt>RowDeleted</tt>
		/// events. See <a href="Delete.html">Deleting a Data Record</a> for the events
		/// flowchart.</para>
		/// <para>This method is typically used to process deletion initiated from
		/// the user interface. If the <tt>AllowDelete</tt> property is
		/// <tt>false</tt>, the data record is not marked deleted and the method
		/// returns 0. The method returns 1 if the data record is successfully
		/// marked deleted.</para>
		/// </remarks>
		/// <param name="keys">The values of key fields.</param>
		/// <param name="values">The values of all fields. The parameter is not
		/// used in the method.</param>
		public override int Delete(IDictionary keys, IDictionary values)
		{
			return 0;
		}

		/// <summary>Places the data record into the cache with the
		/// <tt>Deleted</tt> or <tt>InsertedDeleted</tt> status. The method
		/// assigns the <tt>InsertedDeleted</tt> status to the data record if it
		/// has the <tt>Inserted</tt> status when the method is invoked.</summary>
		/// <remarks>
		/// <para>The method raises the <tt>RowDeleting</tt> and
		/// <tt>RowDeleted</tt> events. See <a href="Delete.html" />
		/// for the events flowchart.</para>
		/// <para>The <tt>AllowDelete</tt> property does not affect this
		/// method.</para>
		/// </remarks>
		/// <param name="data">The data record to delete.</param>
		/// <example>
		/// The code below deletes the current data records through the
		/// <tt>Address</tt> and <tt>Contact</tt> data views on deletion of an
		/// <tt>INSite</tt> data record.
		/// <code>
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
		/// }
		/// </code>
		/// </example>
		public override object Delete(object data)
		{
			return Cache.GetExtension<TNode>(Cache.Delete(getBaseClass(data)));
		}

		protected internal override object Delete(object data, bool bypassinterceptor)
		{
			return Cache.GetExtension<TNode>(Cache.Delete(getBaseClass(data), bypassinterceptor));
		}

		protected internal override void PlaceNotChanged(object data)
		{
			Cache.PlaceNotChanged(getBaseClass(data));
		}

		protected internal override object PlaceNotChanged(object data, out bool wasUpdated)
		{
			return Cache.PlaceNotChanged(getBaseClass(data), out wasUpdated);
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
				return Cache.Dirty;
			}
		}
		/// <summary>Gets the collection of updated data records that exist in the
		/// database. The collection contains data records with the
		/// <tt>Updated</tt> status.</summary>
		/// <example>
		/// <code>
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
		/// }
		/// </code>
		/// </example>
		public override IEnumerable Updated
		{
			get
			{
                return from object o in Cache.Updated select GetExtension<TNode>(o);
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
                return from object o in Cache.Inserted select GetExtension<TNode>(o);
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
				return from object o in Cache.Cached select GetExtension<TNode>(o);
            }
		}

		/// <summary>Gets the collection of deleted data records that exist in the
		/// database. The collection contains data records with the
		/// <tt>Deleted</tt> status.</summary>
		/// <example>
		/// The code below deletes <tt>EPActivity</tt> data records through
		/// a different graph.
		/// <code>
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
		/// }
		/// </code>
		/// </example>
		public override IEnumerable Deleted
		{
			get {
			    return from object o in Cache.Deleted select GetExtension<TNode>(o);
			}
		}
		internal override int Version
		{
			get
			{
				return Cache.Version;
			}
			set
			{
				Cache.Version = value;
			}
		}

		internal override BqlTablePair GetOriginalObjectContext(object data, bool readItemIfNotExists = false)
        {
            return Cache.GetOriginalObjectContext(getBaseClass(data), readItemIfNotExists);
        }

        /// <summary>Gets the value that indicates if the cache contains modified
        /// data records to be saved to database.</summary>
        public override bool IsInsertedUpdatedDeleted
		{
			get
			{
				return Cache.IsInsertedUpdatedDeleted;
			}
		}
		#endregion

		#region Persistance to the database
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
			return 0;
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
		}

		internal protected override void _AdjustStorage(string name, PXDataFieldParam assign)
		{
		}

		internal protected override void _AdjustStorage(int i, PXDataFieldParam assign)
		{
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
			return false;
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
			return false;
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
			return false;
		}

		/// <exclude/>
		public override void ResetPersisted(object row)
		{
			Cache.ResetPersisted(getBaseClass(row));
		}

		/// <summary>Completes saving changes to the database by raising the
		/// <tt>RowPersisted</tt> event for all persisted data records.</summary>
		/// <param name="isAborted">The value indicating whether the database
		/// operation has been aborted or completed.</param>
		/// <example>
		/// You need to call this method in the application only when you call the
		/// <tt>Persist()</tt>, <tt>PersistInserted()</tt>,
		/// <tt>PersistUpdated()</tt>, or <tt>PersistDeleted()</tt> method, as the
		/// following example shows.
		/// <code>
		/// // Opening a transaction and saving changes to the provided
		/// // new data record
		/// using (PXTransactionScope ts = new PXTransactionScope())
		/// {
		///     cache.PersistInserted(item);
		///     ts.Complete(this);
		/// }
		/// 
		/// // Indicating successful completion of saving changes to the database
		/// cache.Persisted(false);</code>
		/// </example>
		public override void Persisted(bool isAborted)
		{
		}

		protected internal override void Persisted(bool isAborted, Exception exception)
		{
		}
		#endregion

		#region Session state managment methods
		/// <summary>Loads dirty items and other cache state objects from the
		/// session. The application does not typically use this method.</summary>
		public override void Load()
		{
		}

        internal override void LoadFromSession(bool force)
        {
        }

        internal override void ResetToUnmodified()
        {
        }

        /// <summary>Clears the cache from all data.</summary>
        /// <example>
        /// The code below clears the cache of the <tt>POReceipt</tt> data
        /// records.
        /// <code>
        /// // Declaration of a data view in a graph
        /// public PXSelect&lt;POReceipt&gt; poreceiptslist;
        /// ...
        /// // Clearing the cache of POReceipt data records 
        /// poreceiptslist.Cache.Clear();</code>
        /// </example>
        public override void Clear()
		{
			Cache.Clear();
		}
		/// <exclude/>
		public override void ClearItemAttributes()
		{
			Cache.ClearItemAttributes();
		}
        ///<exclude/>
		public override void TrimItemAttributes(object item)
		{
			Cache.TrimItemAttributes(item);
		}
		internal override bool HasChangedKeys()
		{
			return Cache.HasChangedKeys();
		}
		internal override void ClearSession()
		{
			Cache.ClearSession();
		}


		/// <summary>Serializes the cache to the session.</summary>
		public override void Unload()
		{
		}
		#endregion

		#region Debugger support
		/// <summary>Returns the string representing the current cache
		/// object.</summary>
		public override string ToString()
		{
			//throw new Exception();
			return Cache.ToString();
		}
		#endregion

		#region Raise events
        /// <exclude />
		public override bool RaiseCommandPreparing(string name, object row, object value, PXDBOperation operation, Type table, out PXCommandPreparingEventArgs.FieldDescription description)
		{
			if (table == typeof(TNode))
			{
				table = Cache.GetItemType();
			}
			return Cache.RaiseCommandPreparing(tryRemapField(name), getBaseClass(row), value, operation, table, out description);
		}
        /// <exclude />
		public override bool RaiseExceptionHandling(string name, object row, object newValue, Exception exception)
		{
			return Cache.RaiseExceptionHandling(tryRemapField(name), getBaseClass(row), newValue, exception);
		}
        /// <exclude />
		public override bool RaiseFieldDefaulting(string name, object row, out object newValue)
		{
			return Cache.RaiseFieldDefaulting(tryRemapField(name), getBaseClass(row), out newValue);
		}
        /// <exclude />
		public override bool RaiseFieldSelecting(string name, object row, ref object returnValue, bool forceState)
		{
			return Cache.RaiseFieldSelecting(tryRemapField(name), getBaseClass(row), ref returnValue, forceState);
		}
        /// <exclude />
		public override void RaiseFieldUpdated(string name, object row, object oldValue)
		{
			Cache.RaiseFieldUpdated(tryRemapField(name), getBaseClass(row), oldValue);
		}
        /// <exclude />
		public override void RaiseFieldUpdated<Field>(object row, object oldValue)
		{
			string name = typeof(Field).Name;
			Cache.RaiseFieldUpdated(tryRemapField(name), getBaseClass(row), oldValue);
		}
		/// <exclude />
		public override bool RaiseFieldVerifying(string name, object row, ref object newValue)
		{
			return Cache.RaiseFieldVerifying(tryRemapField(name), getBaseClass(row), ref newValue);
		}
        /// <exclude />
		public override void RaiseRowDeleted(object item)
		{
			Cache.RaiseRowDeleted(getBaseClass(item));
		}
        /// <exclude />
		public override bool RaiseRowDeleting(object item)
		{
			return Cache.RaiseRowDeleting(getBaseClass(item));
		}
        /// <exclude />
		public override void RaiseRowInserted(object item)
		{
			Cache.RaiseRowInserted(getBaseClass(item));
		}
        /// <exclude />
		public override bool RaiseRowPersisting(object item, PXDBOperation operation)
		{
			return Cache.RaiseRowPersisting(getBaseClass(item), operation);
		}
        /// <exclude />
		public override void RaiseRowSelected(object item)
		{
			Cache.RaiseRowSelected(getBaseClass(item));
		}
        /// <exclude />
		public override bool RaiseRowSelecting(object item, PXDataRecord record, ref int position, bool isReadOnly)
		{
			return Cache.RaiseRowSelecting(getBaseClass(item), record, ref position, isReadOnly);
		}
        /// <exclude />
		public override void RaiseRowUpdated(object newItem, object oldItem)
		{
			Cache.RaiseRowUpdated(getBaseClass(newItem), getBaseClass(oldItem));
		}
        /// <exclude />
		public override bool RaiseRowUpdating(object item, object newItem)
		{
			return Cache.RaiseRowUpdating(getBaseClass(item), getBaseClass(newItem));
		}
		#endregion
	}

    public enum MappingSyncDirection
    {
        ExtensionToBase,
        BaseToExtension
    }
}
