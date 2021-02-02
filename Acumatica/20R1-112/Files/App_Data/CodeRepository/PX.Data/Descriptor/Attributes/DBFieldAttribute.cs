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


using Microsoft.IdentityModel.Clients.ActiveDirectory;
using PX.Api.Soap.Screen;
using PX.Common;
using PX.Common.Mail;
using PX.SM;
using PX.Translation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
//using System.Monads;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using PX.Data.SQLTree;


namespace PX.Data
{
	#region PXDBDefaultAttribute
	/// <summary>Sets the default value for a DAC field. Use to assign a value
	/// from the auto-generated key field.</summary>
	/// <example>
	/// 	<code title="Eaxmple" description="Setting the default value that will be taken from the current POReceipt cache object and reassigned only on insertion of the data record to the database." lang="CS">
	/// public partial class LandedCostTran : PX.Data.IBqlTable
	/// {
	/// ...
	/// [PXDBString(3, IsFixed = true)]
	/// [PXDBDefault(typeof(POReceipt.receiptType),
	/// DefaultForUpdate = false)]
	/// public virtual string POReceiptType { get; set; }
	/// ...
	/// }</code>
	/// 	<code title="Example1" description="Changing the SetDefaultForUpdate property. The method sets the property for the ShipAddressID field in all data records in the cache object associated with the OrderList view." groupname="Example" lang="CS">
	/// XDBDefaultAttribute.SetDefaultForUpdate&lt;SOOrderShipment.shipAddressID&gt;(
	///     OrderList.Cache, null, false);</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
	public class PXDBDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		#region State
		protected Type _SourceType;
		protected Type _OriginSourceType = null;
		protected string _SourceField;
		protected BqlCommand _Select;
		protected bool _DefaultForInsert = true;
		protected bool _DefaultForUpdate = true;
		protected bool _DoubleDefaultAttribute;
		protected PXPersistingCheck _PersistingCheck = PXPersistingCheck.Null;

		internal Type OriginSourceType => _OriginSourceType;

		/// <summary>Gets or sets the <see cref="PXPersistingCheck">PXPersistingCheck</see>
		/// value that defines how to check the field value before saving a data record
		/// to the database. The attribute either checks that the value is not
		/// <tt>null</tt>, checks that the value is <tt>null</tt> or a blank
		/// string (contains only whitespace characters), or doesn't check the
		/// value. If the attribute discovers that the value is in fact
		/// <tt>null</tt> or blank, it will throw the
		/// <tt>PXRowPersistingException</tt> exception. As a result, the save
		/// action will fail and the user will get an error message.</summary>
		public virtual PXPersistingCheck PersistingCheck
		{
			get
			{
				return _PersistingCheck;
			}
			set
			{
				_PersistingCheck = value;
			}
		}

		/// <summary>Gets or sets the value that indicates whether the default
		/// value is reassigned on a database update operation.</summary>
		public bool DefaultForUpdate
		{
			get
			{
				return _DefaultForUpdate;
			}
			set
			{
				_DefaultForUpdate = value;
			}
		}

		/// <summary>Gets or sets the value that indicates whether the default
		/// value is reassigned on a database insert operation.</summary>
		public bool DefaultForInsert
		{
			get
			{
				return _DefaultForInsert;
			}
			set
			{
				_DefaultForInsert = value;
			}
		}
		public bool CanDefault => _SourceType != null || _Select != null;
		
		/// <exclude/>
		protected class FlagHandler
		{
			public bool? Value;
			public Dictionary<object, object> Persisted;
		}
		protected FlagHandler _IsRestriction;
		protected virtual void EnsureIsRestriction(PXCache sender)
		{
			if (_IsRestriction.Value != null || _SourceType == null) return;

			string name = _SourceField ?? _FieldName;
			PXCache parentCache = sender.Graph.Caches[_SourceType];
			if (string.Equals(parentCache.Identity, name, StringComparison.OrdinalIgnoreCase)) {
				_IsRestriction.Value = true;
			}
			else {
				PXCommandPreparingEventArgs.FieldDescription description;
				parentCache.RaiseCommandPreparing(name, null, null, PXDBOperation.Update, null, out description);
				_IsRestriction.Value = description != null && description.IsRestriction;
			}
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXDBDefaultAttribute()
			: this(null)
		{

		}
		/// <summary>
		/// Initializes a new instance of the attribute. Obtains the default value using the provided BQL query.
		/// </summary>
		/// <param name="sourceType">The BQL query that is used to calculate the default value. Accepts the types derived from: <tt>IBqlSearch</tt>IBqlSearch, <tt>IBqlSelect</tt>, <tt>IBqlField</tt>, <tt>IBqlTable</tt>.</param>
		public PXDBDefaultAttribute(Type sourceType)
		{
			_OriginSourceType = sourceType;
			SetSourceType(null, null);
		}
		#endregion

		#region Implementation
		/// <summary>
		/// Provides the default value
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to set the NewValue</param>
		/// <exclude/>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_Select != null) {
				PXView view = sender.Graph.TypedViews.GetView(_Select, false);
				List<object> source = view.SelectMultiBound(new object[] { e.Row });
				if (source != null && source.Count > 0) {
					object item = source[source.Count - 1];
					if (item != null && item is PXResult) {
						item = ((PXResult)item)[_SourceType];
					}
					e.NewValue = sender.Graph.Caches[_SourceType].GetValue(item, _SourceField ?? _FieldName);
					e.Cancel = true;
					return;
				}
			}
			else if (_SourceType != null) {
				PXCache cache = sender.Graph.Caches[_SourceType];
				if (cache.Current != null) {
					e.NewValue = cache.GetValue(cache.Current, _SourceField ?? _FieldName);
					e.Cancel = true;
					return;
				}
			}
		}
		/// <summary>
		/// Re-default the value. Check if the value was set before saving the record to the database
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to retrive the value from the Row</param>
		/// <exclude/>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && _DefaultForUpdate) && _SourceType != null) {
				EnsureIsRestriction(sender);
				if (_IsRestriction.Value == true) {
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert
						&& !_DoubleDefaultAttribute
						&& (key is string && ((string)key).StartsWith(" ", StringComparison.InvariantCultureIgnoreCase)
						|| key is int && ((int)key) < 0
						|| key is long && ((long)key) < 0))
					{
						sender.SetValue(e.Row, _FieldOrdinal, null);
					}
					if (_IsRestriction.Persisted != null && key != null)
					{
						object parent;
						if (_IsRestriction.Persisted.TryGetValue(key, out parent)) {
							key = sender.Graph.Caches[_SourceType].GetValue(parent, _SourceField ?? _FieldName);
							sender.SetValue(e.Row, _FieldOrdinal, key);
							if (key != null) {
								_IsRestriction.Persisted[key] = parent;
							}
						}
					}
				}
				else {
					sender.SetValue(e.Row, _FieldOrdinal, null);
					if (_Select != null) {
						PXView view = sender.Graph.TypedViews.GetView(_Select, false);
						List<object> source = view.SelectMultiBound(new object[] { e.Row });
						if (source != null && source.Count > 0) {
							object result = source[source.Count - 1];
							if (result is PXResult) result = ((PXResult)result)[0];
							sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[_SourceType].GetValue(result, _SourceField ?? _FieldName));
						}
					}
					else if (_SourceType != null) {
						PXCache cache = sender.Graph.Caches[_SourceType];
						if (cache.Current != null) {
							sender.SetValue(e.Row, _FieldOrdinal, cache.GetValue(cache.Current, _SourceField ?? _FieldName));
						}
					}
				}
			}
			if (PersistingCheck != PXPersistingCheck.Nothing &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && _DefaultForUpdate) &&
				sender.GetValue(e.Row, _FieldOrdinal) == null) {
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName)))) {
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
			}
		}

		/// <summary>
		/// Rollback changes if the record was not saved to the database
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to check operation and get the row to process</param>
		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert && e.TranStatus == PXTranStatus.Aborted && _SourceType != null) {
				EnsureIsRestriction(sender);
				if (_IsRestriction.Value == true) {
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					if (_IsRestriction.Persisted != null && key != null) {
						object parent;
						if (_IsRestriction.Persisted.TryGetValue(key, out parent)) {
							sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[_SourceType].GetValue(parent, _SourceField ?? _FieldName));
						}
					}
				}
				else {
					sender.SetValue(e.Row, _FieldOrdinal, null);
					if (_Select != null) {
						PXView view = sender.Graph.TypedViews.GetView(_Select, false);
						List<object> source = view.SelectMultiBound(new object[] { e.Row });
						if (source != null && source.Count > 0) {
							sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[_SourceType].GetValue(source[source.Count - 1], _SourceField ?? _FieldName));
						}
					}
					else if (_SourceType != null) {
						PXCache cache = sender.Graph.Caches[_SourceType];
						if (cache.Current != null) {
							sender.SetValue(e.Row, _FieldOrdinal, cache.GetValue(cache.Current, _SourceField ?? _FieldName));
						}
					}
				}
			}
		}

		public void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			StorePersisted(sender, e.Row);
		}

		public void SourceRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				StorePersisted(sender, e.Row);
			}
		}

		public void StorePersisted(PXCache sender, object row)
		{
			EnsureIsRestriction(sender);
			if (_IsRestriction.Value == true) {
				object key = sender.GetValue(row, _SourceField ?? _FieldName);
				if (key != null) {
					if (_IsRestriction.Persisted == null) {
						_IsRestriction.Persisted = new Dictionary<object, object>();
					}
					if (_Select == null || _Select.Meet(sender, row)) {
						_IsRestriction.Persisted[key] = row;
					}
				}
			}
		}

		protected virtual void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_SourceType == null)
				return;
			if ((e.Operation & PXDBOperation.ReadItem) == PXDBOperation.ReadItem || 
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Select &&
				(e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null))
			{
				PXCache cache = sender.Graph.Caches[_SourceType];
				var field = _SourceField ?? _FieldName;
				if (cache.IsAutoNumber(field))
				{
				PXCommandPreparingEventArgs.FieldDescription descr;
					if (!cache.RaiseCommandPreparing(field, null, e.Value, e.Operation, null, out descr))
					{
					e.DataValue = descr.DataValue;
					e.Cancel = true;
				}
			}
		}
		}
		#endregion

		#region Runtime
		/// <summary>Sets the <tt>DefaultForUpdate</tt> property for a particular
		/// data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBDefault</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="def">The new value for the property.</param>
		/// <example>
		/// The code below changes the <tt>SetDefaultForUpdate</tt> property at
		/// run time. The method sets the property for the <tt>ShipAddressID</tt> field in all data
		/// records in the cache object associated with the <tt>OrderList</tt> data view.
		/// <code>
		/// PXDBDefaultAttribute.SetDefaultForUpdate&lt;SOOrderShipment.shipAddressID&gt;(
		///     OrderList.Cache, null, false);
		/// </code>
		/// </example>
		public static void SetDefaultForUpdate<Field>(PXCache cache, object data, bool def)
			where Field : IBqlField
		{
			if (data == null) {
				cache.SetAltered<Field>(true);
			}
			foreach (PXDBDefaultAttribute attr in cache.GetAttributes<Field>(data).OfType<PXDBDefaultAttribute>()) {
				(attr)._DefaultForUpdate = def;
			}
		}

		/// <summary>Sets the <tt>DefaultForInsert</tt> property for a particular
		/// data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PDBXDefault</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="def">The new value for the property.</param>
		public static void SetDefaultForInsert<Field>(PXCache cache, object data, bool def)
			where Field : IBqlField
		{
			if (data == null) {
				cache.SetAltered<Field>(true);
			}
			foreach (PXDBDefaultAttribute attr in cache.GetAttributes<Field>(data).OfType<PXDBDefaultAttribute>()) {
				(attr)._DefaultForInsert = def;
			}
		}

		/// <exclude/>
		public static void SetSourceType(PXCache cache, string field, Type sourceType)
		{
			cache.SetAltered(field, true);
			foreach (PXDBDefaultAttribute attribute in cache.GetAttributes(field).OfType<PXDBDefaultAttribute>()) {
				attribute.SetSourceType(cache, sourceType);
			}
		}

		/// <exclude/>
		public static void SetSourceType<Field>(PXCache cache, Type sourceType)
			where Field : IBqlField
		{
			SetSourceType(cache, typeof(Field).Name, sourceType);
		}

		/// <exclude/>
		public static void SetSourceType(PXCache cache, object data, string field, Type sourceType)
		{
			if (data == null) {
				cache.SetAltered(field, true);
			}
			foreach (PXDBDefaultAttribute attribute in cache.GetAttributes(data, field).OfType<PXDBDefaultAttribute>()) {
				attribute.SetSourceType(cache, sourceType);
			}
		}

		/// <exclude/>
		public static void SetSourceType<Field>(PXCache cache, object data, Type sourceType)
			where Field : IBqlField
		{
			SetSourceType(cache, data, typeof(Field).Name, sourceType);
		}

		#endregion

		#region Initialization

		protected virtual void SetSourceType(PXCache cache, Type sourceType)
		{
			sourceType = sourceType ?? _OriginSourceType;
			Type oldSourceType = _SourceType;

			//if (sourceType != _OriginSourceType || sourceType == null)
			{
				if (sourceType == null) {
					_Select = null;
					_SourceType = null;
					_SourceField = null;
				}
				else if (typeof(IBqlSearch).IsAssignableFrom(sourceType)) {
					_Select = BqlCommand.CreateInstance(sourceType);
					_SourceType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
					_SourceField = ((IBqlSearch)_Select).GetField().Name;
					_SourceField = char.ToUpper(_SourceField[0]) + _SourceField.Substring(1);
				}
				else if (sourceType.IsNested && typeof(IBqlField).IsAssignableFrom(sourceType)) {
					_SourceType = BqlCommand.GetItemType(sourceType);
					_SourceField = sourceType.Name;
					_SourceField = char.ToUpper(_SourceField[0]) + _SourceField.Substring(1);
				}
				else if (typeof(IBqlTable).IsAssignableFrom(sourceType)) {
					_Select = null;
					_SourceType = sourceType;
					_SourceField = null;
				}
				else {
					throw new PXArgumentException(nameof(sourceType), ErrorMessages.CantCreateForeignKeyReference, sourceType);
				}
			}

			if (cache != null && oldSourceType != _SourceType) {
				if (oldSourceType != null)
					cache.Graph.RowPersisting.RemoveHandler(oldSourceType, SourceRowPersisting);
				if (_SourceType != null)
					cache.Graph.RowPersisting.AddHandler(_SourceType, SourceRowPersisting);
			}
		}

		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (_SourceType != null)
			{
				sender.Graph.RowPersisting.AddHandler(_SourceType, SourceRowPersisting);
				sender.Graph.RowPersisted.AddHandler(_SourceType, SourceRowPersisted);
			}

			_IsRestriction = new FlagHandler();

			if (_Select == null) {
				sender.Graph.CommandPreparing.AddHandler(sender.GetItemType(), _FieldName, Parameter_CommandPreparing);
			}

			_DoubleDefaultAttribute = sender.GetAttributesReadonly(_FieldName, false).Any(_ => this.GetType().IsAssignableFrom(_.GetType()) && !object.ReferenceEquals(this, _));
		}
		#endregion
	}
	#endregion

	#region PXDBFieldAttribute
	/// <summary>The base class for attributes that map DAC fields to database
	/// columns. The attribute should not be used directly.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXDBFieldAttribute))]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDBFieldAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected string _DatabaseFieldName = null;
		protected bool _IsKey = false;
		protected bool _IsImmutable = false;
		/// <summary>Gets or sets the name of the database column that is
		/// represented by the field. By default, equals the field name.</summary>
		public virtual string DatabaseFieldName
		{
			get
			{
				return _DatabaseFieldName ?? _FieldName;
			}
			set
			{
				_DatabaseFieldName = value;
			}
		}
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field. Key fields must uniquely identify a data record. The key
		/// fields defined in the DAC should not necessarily be the same as the
		/// keys in the database.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
				_IsImmutable = IsImmutable || value;
			}
		}
		/// <summary>Gets or sets the values that indicates that the field is
		/// immutable.</summary>
		public virtual bool IsImmutable
		{
			get
			{
				return _IsImmutable;
			}
			set
			{
				_IsImmutable = value;
			}
		}
		/// <summary>Returns <tt>null</tt> on get. Sets the BQL field representing
		/// the field in BQL queries.</summary>
		public virtual Type BqlField
		{
			get
			{
				return null;
			}
			set
			{
				_DatabaseFieldName = char.ToUpper(value.Name[0]) + value.Name.Substring(1);
				if (value.IsNested
					//&& typeof(IBqlTable).IsAssignableFrom(value.DeclaringType)
					) {
						if (value.DeclaringType.IsDefined(typeof(PXTableAttribute), true))
						{
							BqlTable = value.DeclaringType;
						}
						else
						{
							BqlTable = BqlCommand.GetItemType(value);
						}
					}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			PrepareCommandImpl(DatabaseFieldName, e);
			if (e.Expr!=null && e.Expr.GetType() == typeof(SQLTree.Column)) ((SQLTree.Column) e.Expr).SetDBType(e.DataType);
		}

		protected virtual void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			PrepareFieldName(dbFieldName, e);
			e.DataValue = e.Value;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}

		protected virtual void PrepareFieldName(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			if (dbFieldName == null) 
				return;

			e.BqlTable = _BqlTable;
			var table = e.Table == null ? _BqlTable : e.Table;
			e.Expr=new SQLTree.Column(dbFieldName, new SQLTree.SimpleTable(table), e.DataType);
		}

		/// <exclude/>
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				object dbValue = e.Record.GetValue(e.Position);
				sender.SetValue(e.Row, _FieldOrdinal, dbValue);
			}
			e.Position++;
		}
		/// <exclude/>
		public override string ToString()
		{
			return String.Format("{0} {1} {2}", this.GetType().Name, FieldName, this.FieldOrdinal);
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (_DatabaseFieldName == null) {
				_DatabaseFieldName = _FieldName;
			}
			if (IsKey /*&& _BqlTable.IsAssignableFrom(sender.GetItemType())*/) {
				sender.Keys.Add(_FieldName);
			}
			if (IsImmutable) {
				sender.Immutables.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBStringAttribute
	/// <summary>Maps a DAC field of <tt>string</tt> type to the database field of <tt>char</tt>, <tt>varchar</tt>, <tt>nchar</tt>, or <tt>nvarchar</tt> type.</summary>
	/// <remarks>
	///   <para>The attribute is added to the value declaration of a DAC field. The field becomes bound to the database column with the same name.</para>
	///   <para>It is possible to specify the maximum length and input validation mask for the string.</para>
	///   <para>You can modify the <tt>Length</tt> and <tt>InputMask</tt> properties at run time by calling the static methods.</para>
	/// </remarks>
	/// <example>
	///   <code title="Example2" description="The attribute below maps a string field to the database column (defines a bound field) and sets a limit for the value length to 50." lang="CS">
	/// [PXDBString(50)]
	/// public virtual string Fax { get; set; }</code>
	/// 	<code title="Example" description="The attribute below defines a bound field taking as a value strings of any 8 characters. In the user interface, the input control will show the mask that splits the value into four groups separated by dots." lang="CS">
	/// [PXDBString(8, InputMask = "CC.CC.CC.CC")]
	/// public virtual string ReportID { get; set; }</code>
	/// 	<code title="Example3" description="The attribute below defines a bound field taking as a value Unicode strings of 5 uppercase characters that are strictly aphabetical letters." groupname="Example2" lang="CS">
	/// [PXDBString(5, IsUnicode = true, InputMask = "&gt;LLLLL")]
	/// public virtual string CuryID { get; set; }</code>
	/// 	<code title="Example4" description="The example below shows a complex definition of a string key field represented in the user interface by a lookup control. In this example, the RefNbr field is mapped to the nvarchar(15) RefNbr column from the APRegister table." groupname="Example3" lang="CS">
	/// [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
	/// [PXDefault]
	/// [PXUIField(DisplayName = "Reference Nbr.",
	///            Visibility = PXUIVisibility.SelectorVisible,
	///            TabOrder = 1)]
	/// [PXSelector(typeof(
	///     Search&lt;APRegister.refNbr,
	///         Where&lt;APRegister.docType, Equal&lt;Optional&lt;APRegister.docType&gt;&gt;&gt;&gt;),
	///     Filterable = true)]
	/// public virtual string RefNbr { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBStringAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _Length = -1;
		protected string _InputMask = null;
		protected bool _IsUnicode = false;
		protected bool _IsFixed = false;

		/// <summary>Gets the maximum length of the string value. If a string
		/// value exceeds the maximum length, it will be trimmed. If
		/// <tt>IsFixed</tt> is set to <tt>true</tt> and the string length is less
		/// then the maximum, it will be extended with spaces.</summary>
		/// <remarks>The default value is -1 (the string length is not limited). A
		/// different value can be set in the constructor.</remarks>
		public int Length
		{
			get
			{
				return _Length;
			}
		}

		/// <summary>Gets or sets the pattern that indicates the allowed
		/// characters in a field value. The user interface will not allow the
		/// user to enter other characters in the input control associated with
		/// the field.</summary>
		/// <remarks>
		/// <para>The default value for the key fields is
		/// '<tt>&gt;aaaaaa</tt>'.</para>
		/// <para><i>Control characters:</i></para>
		/// <list type="bullet">
		/// <item><description>'&gt;': the following chars to
		/// upper case</description></item>
		/// <item><description>'&lt;': the
		/// following chars to lower
		/// case</description></item>
		/// <item><description>'<tt>&amp;</tt>',
		/// '<tt>C</tt>': any character or a
		/// space</description></item>
		/// <item><description>'<tt>A</tt>',
		/// '<tt>a</tt>': a letter or
		/// digit</description></item>
		/// <item><description>'<tt>L</tt>',
		/// '<tt>?</tt>': a
		/// letter</description></item>
		/// <item><description>'<tt>#</tt>',
		/// '<tt>0</tt>', '<tt>9</tt>': a digit</description></item>
		/// </list>
		/// </remarks>
		/// <example>
		/// The attribute below defines a bound field taking as a value Unicode
		/// strings of 5 uppercase characters that are strictly aphabetical letters.
		/// <code>
		/// [PXDBString(5, IsUnicode = true, InputMask = "&gt;LLLLL")]
		/// public virtual string CuryID { get; set; }
		/// </code>
		/// </example>
		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}

		/// <summary>Gets or sets an indication that the string consists of
		/// Unicode characters. This property should be set to <tt>true</tt> if
		/// the database column has a Unicode string type (<tt>nchar</tt> or
		/// <tt>nvarchar</tt>). The default value is <tt>false</tt>.</summary>
		/// <example>
		/// The example below shows a complex definition of a string key field
		/// represented in the user interface by a lookup control.
		/// In this example, the <tt>RefNbr</tt> field is mapped to the
		/// <tt>nvarchar(15)</tt> column from the <tt>APRegister</tt> table.
		/// <code>
		/// [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		/// [PXDefault]
		/// [PXUIField(DisplayName = "Reference Nbr.",
		///            Visibility = PXUIVisibility.SelectorVisible,
		///            TabOrder = 1)]
		/// [PXSelector(typeof(
		///     Search&lt;APRegister.refNbr,
		///         Where&lt;APRegister.docType, Equal&lt;Optional&lt;APRegister.docType&gt;&gt;&gt;&gt;),
		///     Filterable = true)]
		/// public virtual string RefNbr { get; set; }
		/// </code>
		/// </example>
		public bool IsUnicode
		{
			get
			{
				return _IsUnicode;
			}
			set
			{
				_IsUnicode = value;
			}
		}

		/// <summary>Gets or sets an indication that the string has a fixed
		/// length. This property should be set to <tt>true</tt> if the database
		/// column has a fixed length type (<tt>char</tt> or <tt>nchar</tt>). The
		/// default value is <tt>false</tt>.</summary>
		public bool IsFixed
		{
			get
			{
				return _IsFixed;
			}
			set
			{
				_IsFixed = value;
			}
		}
		/// <exclude/>
		protected enum MaskMode
		{
			Manual,
			Auto,
			Foreign
		}
		protected MaskMode _AutoMask = MaskMode.Manual;
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance of the attribute.</summary>
		public PXDBStringAttribute()
		{
		}

		/// <summary>Initializes a new instance with the given maximum length of a
		/// field value.</summary>
		/// <param name="length">The maximum length value assigned to the
		/// <tt>Length</tt> property.</param>
		/// <example>
		/// The attribute below maps a string field to the database column
		/// (defines a bound field) and sets a limit for the value length to 50.
		/// <code>
		/// [PXDBString(50)]
		/// public virtual string Fax { get; set; }</code>
		/// </example>
		public PXDBStringAttribute(int length)
		{
			_Length = length;
		}
		#endregion

		#region Runtime
		private static void setLength(PXDBStringAttribute attr, int length)
		{
			attr._Length = length;
		}
		/// <summary>Sets the maximum length for the string field with the
		/// specified name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="length">The value that is assigned to the <tt>Length</tt>
		/// property.</param>
		public static void SetLength(PXCache cache, object data, string name, int length)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the maximum length for the specified string
		/// field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="length">The value that is assigned to the <tt>Length</tt>
		/// property.</param>
		public static void SetLength<Field>(PXCache cache, object data, int length)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the maximum length for the string field with the
		/// specified name for all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="length">The value that is assigned to the <tt>Length</tt>
		/// property.</param>
		public static void SetLength(PXCache cache, string name, int length)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the maximum length for the specified string field for
		/// all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="length">The value that is assigned to the <tt>Length</tt>
		/// property.</param>
		public static void SetLength<Field>(PXCache cache, int length)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the input mask for the string field with the specified
		/// name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="mask">The value that is assigned to the
		/// <tt>InputMask</tt> property.</param>
		public static void SetInputMask(PXCache cache, object data, string name, string mask)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		/// <summary>Sets the input mask for the specified string field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="mask">The value that is assigned to the
		/// <tt>InputMask</tt> property.</param>
		public static void SetInputMask<Field>(PXCache cache, object data, string mask)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		/// <summary>Sets the input mask for the string field with the specified
		/// name for all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="mask">The value that is assigned to the
		/// <tt>InputMask</tt> property.</param>
		public static void SetInputMask(PXCache cache, string name, string mask)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		/// <summary>Sets the input mask for the specified string field for all
		/// data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBString</tt> type.</param>
		/// <param name="mask">The value that is assigned to the
		/// <tt>InputMask</tt> property.</param>
		public static void SetInputMask<Field>(PXCache cache, string mask)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null && !e.Cancel)
			{
				var stringValue = e.NewValue as string;
				if (stringValue == null)
					stringValue = Convert.ToString(e.NewValue);
				e.NewValue = stringValue;
				if (!_IsFixed)
				{
					e.NewValue = ((string)e.NewValue).TrimEnd();
				}
				if (_Length >= 0)
				{
					int length = ((string)e.NewValue).Length;
					if (length > _Length)
					{
						e.NewValue = ((string)e.NewValue).Substring(0, _Length);
					}
					else if (_IsFixed && length < _Length)
					{
						StringBuilder bld = new StringBuilder(((string)e.NewValue), _Length);
						for (int i = length; i < _Length; i++)
						{
							bld.Append(' ');
						}
						e.NewValue = bld.ToString();
					}
				}
			}
		}
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = DbType;
			if (_Length >= 0)
			{
				e.DataLength = _Length;
			}
		}

		protected PXDbType DbType
		{
			get
			{
			return _IsFixed ? (_IsUnicode ? PXDbType.NChar : PXDbType.Char) : (_IsUnicode ? PXDbType.NVarChar : PXDbType.VarChar);
			}
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{

			if (e.Row != null)
			{
				string v = e.Record.GetString(e.Position);
				// amazon aws mySQL server won't let us globally change the sql_mode (to include PAD_CHAR_TO_FULL_LENGTH), that's why we need to pad spaces here
				if (v != null)
				{
					int spacesToPad = IsFixed ? Length - v.Length : 0;
					if (spacesToPad > 0)
						v = string.Concat(v, new String(' ', spacesToPad));
				}

				if (sender.Graph.StringTable != null)
					v = sender.Graph.StringTable.Add(v);

				if (_IsKey && !e.IsReadOnly)
				{
					//string key;
					sender.SetValue(e.Row, _FieldOrdinal, v);
					if ((v == null || sender.IsPresent(e.Row)) && sender.Graph.GetType() != typeof(PXGraph) && PXView.LegacyQueryCacheModeEnabled)
					{
						e.Row = null;
					}
				}
				else
				{
					sender.SetValue(e.Row, _FieldOrdinal, v);
				}
			}


			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				if (_AutoMask == MaskMode.Auto)
				{
					_AutoMask = MaskMode.Manual;
					if (sender.Keys.IndexOf(_FieldName) != sender.Keys.Count - 1)
					{
						_InputMask = null;
					}
				}
				else if (_AutoMask == MaskMode.Foreign && !PXContext.GetSlot<bool>(PXSelectorAttribute.selectorBypassInit))
				{
					_AutoMask = MaskMode.Manual;
					if (!_masks.TryGetValue(_BqlTable.Name + "$" + _FieldName, out _InputMask))
					{
						string Mask = null;
						Type ForeignField = null;
						foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(null, _FieldName))
						{
							if (attr is PXSelectorAttribute)
							{
								ForeignField = ((PXSelectorAttribute)attr).Field;
							}
						}
						if (ForeignField != null)
						{
							PXCache ForeignCache = sender.Graph.Caches[BqlCommand.GetItemType(ForeignField)];
							foreach (PXEventSubscriberAttribute attr in ForeignCache.GetAttributes(null, ForeignField.Name))
							{
								if (attr is PXDBStringAttribute)
								{
									Mask = ((PXDBStringAttribute)attr).InputMask;
								}
							}
						}
						InputMask = Mask;
						lock (((ICollection)_masks).SyncRoot)
						{
							_masks[_BqlTable.Name + "$" + _FieldName] = Mask;
						}
					}
				}
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, String.IsNullOrEmpty(_InputMask) ? null : _InputMask, null, null, null, null);
			}
		}
		#endregion

		#region Initialization
		protected static Dictionary<string, string> _masks = new Dictionary<string, string>();
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_InputMask == null)
			{
				if (IsKey)
				{
					StringBuilder sb = new StringBuilder(">");
					for (int i = 0; i < _Length; i++)
					{
						sb.Append("a");
					}
					InputMask = sb.ToString();
					_AutoMask = MaskMode.Auto;
				}
				else if (!_masks.TryGetValue(_BqlTable.Name + "$" + _FieldName, out _InputMask))
				{
					_AutoMask = MaskMode.Foreign;
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDBLocalizableStringAttribute
	/// <summary>Allows you to configure a DAC field to have values in multiple languages.</summary>
	/// <remarks>The <tt>PXDBLocalizableString</tt> attribute works similarly to the <tt>PXDBString</tt> attribute, but unlike the  <tt>PXDBString</tt> attribute, 
	/// the <tt>PXDBLocalizableString</tt> attribute can be used instead of the <tt>PXDBText</tt> and PXString attributes.</remarks>
	/// <example>
	/// 	<code title="Example" description="The general declaration example
	/// " lang="CS">
	/// public abstract class noteID : PX.Data.BQL.BqlGuid.Field&lt;noteID> { }
	///  
	/// [PXNote]
	/// [PXDBLocalizableString(60, IsUnicode = true)]
	/// public virtual Guid? NoteID { get; set; }</code>
	/// 	<code title="Example2" description="If you need to configure a field that has the PXString attribute, which is used in conjunction with the PXDBCalced attribute, replace the PXString attribute with the PXDBLocalizableString attribute and set the value of the NonDB parameter to true, as shown in the following example." groupname="Example" lang="CS">
	/// PXDBLocalizableString(255, IsUnicode = true, NonDB = true, 
	///    BqlField = typeof(PaymentMethod.descr))] 
	/// [PXDBCalced(typeof(Switch&lt;Case&lt;Where&lt;PaymentMethod.descr, IsNotNull&gt;, 
	///    PaymentMethod.descr&gt;, CustomerPaymentMethod.descr&gt;), typeof(string))]</code>
	/// 	<code title="Example3" description="If you want to obtain the value of a multi-language field in the current locale, use the PXDatabase.SelectSingle() or PXDatabase.SelectMulti() method, and pass to it the return value of the PXDBLocalizableStringAttribute.GetValueSelect() static method instead of passing a new PXDataField object to it. " groupname="Example2" lang="CS">
	/// foreach (PXDataRecord record in PXDatabase.SelectMulti&lt;Numbering&gt;(
	///    newPXDataField&lt;Numbering.numberingID&gt;(), 
	///    PXDBLocalizableStringAttribute.GetValueSelect("Numbering", 
	///       "NewSymbol", false),
	///    newPXDataField&lt;Numbering.userNumbering&gt;()))     
	/// {
	///    ...
	/// }</code>
	/// 	<code title="Example4" description="If you want to obtain the value of a multi-language field in a specific language, use the PXDBLocalizableStringAttribute.GetTranslation() method. Pass to the method as input parameters a DAC cache, a DAC instance, a field name, and the ISO code of the language. 
	/// The following code shows an example of use of the PXDBLocalizableStringAttribute.GetTranslation() method." groupname="Example3" lang="CS">
	/// tran.TranDesc =
	///    PXDBLocalizableStringAttribute.GetTranslation(
	///       Caches[typeof(InventoryItem)], item, typeof(InventoryItem.descr).Name,
	///       customer.Current?.LanguageName);</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBLocalizableStringAttribute : PXDBStringAttribute, IPXRowPersistedSubscriber
	{
		internal const string TranslationsPostfix = "Translations";

		#region State
		protected int _PositionInTranslations;
		/// <exclude/>
		public PXDBLocalizableStringAttribute(int length)
			: base(length)
		{
		}
		/// <exclude/>
		public PXDBLocalizableStringAttribute()
			: base()
		{
		}

		public bool NonDB
		{
			get;
			set;
		}

		public bool IsProjection
		{
			get;
			set;
		}

		public bool IsSecondLevelProjection
		{
			get;
			set;
		}

		public static bool IsEnabled
		{
			get
			{
				var def = PXContext.GetSlot<Definition>();
				if (def == null)
				{
					PXContext.SetSlot<Definition>(def = PXDatabase.GetSlot<Definition>("Definition", typeof(Locale)));
				}
				if (def == null || String.IsNullOrEmpty(def.DefaultLocale))
				{
					return false;
				}
				return true;
			}
		}

		public static bool HasMultipleLocales
		{
			get
			{
				var def = PXContext.GetSlot<Definition>();
				if (def == null)
				{
					PXContext.SetSlot<Definition>(def = PXDatabase.GetSlot<Definition>("Definition", typeof(Locale)));
				}
				if (def == null || !def.HasMultipleLocales)
				{
					return false;
				}
				return true;
			}
		}

		public static List<string> EnabledLocales
		{
			get
			{
				var def = PXContext.GetSlot<Definition>();
				if (def == null)
				{
					PXContext.SetSlot<Definition>(def = PXDatabase.GetSlot<Definition>("Definition", typeof(Locale)));
				}
				if (def == null)
				{
					return new List<string>();
				}
				return def.DefaultPlusAlternative;
			}
		}

		/// <exclude/>
		public static void DefaultTranslationsFromMessage(PXCache sender, object row, string fieldName, string message)
		{
			var def = PXContext.GetSlot<Definition>();
			if (def == null)
			{
				PXContext.SetSlot<Definition>(def = PXDatabase.GetSlot<Definition>("Definition", typeof(Locale)));
			}
			if (def == null)
			{
				return;
			}
			string[] languages = sender.GetValueExt(null, fieldName + TranslationsPostfix) as string[];
			if (languages != null)
			{
				string[] translations = new string[languages.Length];
				for (int i = 0; i < translations.Length; i++)
				{
					List<string> locs;
					if (def.LocalesByLanguage.TryGetValue(languages[i], out locs))
					{
						foreach (string l in locs)
						{
							using (new PXLocaleScope(l))
							{
								translations[i] = PXMessages.LocalizeNoPrefix(message);
								if (translations[i] != null && !string.Equals(translations[i], message))
								{
									break;
								}
								else if (languages[i] != def.DefaultLocale)
								{
									translations[i] = null;
								}
							}
						}
					}
				}
				sender.SetValueExt(row, fieldName + TranslationsPostfix, translations);
			}
		}

		/// <summary>
		/// If <paramref name="language"/> is defined, returns the translation of the specified field for the specified record;
		/// otherwise, returns the current value of the field.
		/// </summary>
		/// <param name="cache">The cache object that is used to search for the attributes of
		/// the <see cref="PXDBLocalizableStringAttribute"/> type.</param>
		/// <param name="data">The record from which the translation is retrieved.</param>
		/// <param name="fieldName">The name of the field for which translation is requested.  
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</param>
		/// <param name="language">A predefined System.Globalization.CultureInfo name, System.Globalization.CultureInfo.Name
		/// of an existing System.Globalization.CultureInfo, or Windows-only culture name.
		/// The name is not case-sensitive.</param>
		public static string GetTranslation(PXCache cache, object data, string fieldName, string language)
		{
			if (IsEnabled && !String.IsNullOrWhiteSpace(language))
			{
				language = new CultureInfo(language).TwoLetterISOLanguageName;
				string[] languages = cache.GetValueExt(null, fieldName + TranslationsPostfix) as string[];
				string[] translations = cache.GetValueExt(data, fieldName + TranslationsPostfix) as string[];
				int idx;
				if (languages != null && translations != null && (idx = Array.IndexOf(languages, language)) >= 0
					&& idx < translations.Length && !String.IsNullOrEmpty(translations[idx]))
				{
					return translations[idx];
				}
			}
			return cache.GetValue(data, fieldName) as string;
		}

		/// <summary>
		/// If <paramref name="language"/> is defined, returns the translation of the 
		/// field of the specified type for the specified record;
		/// otherwise, returns the current value of the field.
		/// </summary>
		/// <typeparam name="TField">The type of the field for which translation is requested.
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <param name="cache">The cache object that is used to search for the attributes of
		/// the <see cref="PXDBLocalizableStringAttribute"/> type.</param>
		/// <param name="data">The record from which the translation is retrieved.</param>
		/// <param name="language">A predefined System.Globalization.CultureInfo name, System.Globalization.CultureInfo.Name
		/// of an existing System.Globalization.CultureInfo, or Windows-only culture name.
		/// The name is not case-sensitive.</param>
		public static string GetTranslation<TField>(PXCache cache, object data, string language)
			where TField : IBqlField
		{
			return GetTranslation(cache, data, typeof(TField).Name, language);
		}

		/// <summary>
		/// Returns the translation of the field of the specified type for the specified record for current locale.
		/// </summary>
		/// <typeparam name="TField">The type of the field for which translation is requested.
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <param name="cache">The cache object that is used to search for the attributes of
		/// the <see cref="PXDBLocalizableStringAttribute"/> type.</param>
		/// <param name="data">The record from which the translation is retrieved.</param>
		public static string GetTranslation<TField>(PXCache cache, object data)
			where TField : IBqlField
		{
			return (string)PXFieldState.UnwrapValue(cache.GetValueExt<TField>(data));
		}

		/// <summary>
		/// Copies all translations from the specified field of the source record
		/// to the specified field of the destination record.
		/// </summary>
		/// <typeparam name="TSourceField">The type of the source field.  
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <typeparam name="TDestinationField">The type of the destination field.
		/// Translations are copied to this field.
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <param name="sourceCache">The cache object that is used to search for the attributes of
		/// the <see cref="PXDBLocalizableStringAttribute"/> type  
		/// on the <typeparamref name="TSourceField"/> field.</param>
		/// <param name="sourceData">The record from which translations are retrieved.</param>
		/// <param name="destinationCache">The cache object that is used to search for the attributes of
		/// the <see cref="PXDBLocalizableStringAttribute"/> type 
		/// on the <typeparamref name="TDestinationField"/> field.</param>
		/// <param name="destinationData">The record to which translations are copied.</param>
		public static void CopyTranslations<TSourceField, TDestinationField>(PXCache sourceCache, object sourceData, PXCache destinationCache, object destinationData)
			where TSourceField : IBqlField
			where TDestinationField : IBqlField
		{
			if (IsEnabled)
			{
				string[] translations = new string[] { };
				foreach (PXDBLocalizableStringAttribute attribute in
					sourceCache.GetAttributes<TSourceField>(sourceData).Where(attribute => (attribute is PXDBLocalizableStringAttribute)))
				{
					translations = attribute.GetTranslations(sourceCache, sourceData);
					if (translations != null)
						translations = (string[])translations.Clone();
					break;
				}
				foreach (PXDBLocalizableStringAttribute attribute in
					destinationCache.GetAttributes<TDestinationField>(destinationData).Where(attribute => (attribute is PXDBLocalizableStringAttribute)))
				{
					attribute.SetTranslations(destinationCache, destinationData, translations);
				}
			}
		}

		/// <summary>
		/// Copies all translations from the specified field of the source record
		/// to the specified field of the destination record.
		/// </summary>
		/// <typeparam name="TSourceField">The type of the source field.  
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <typeparam name="TDestinationField">The type of the destination field.
		/// Translations are copied to this field.
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <param name="graph">PXGraph which contains source and destination caches.</param>
		/// <param name="sourceData">The record from which translations are retrieved.</param>
		/// <param name="destinationData">The record to which translations are copied.</param>
		public static void CopyTranslations<TSourceField, TDestinationField>(PXGraph graph, object sourceData, object destinationData)
			where TSourceField : IBqlField
			where TDestinationField : IBqlField
		{            
			PXCache sourceCache = graph.Caches[BqlCommand.GetItemType<TSourceField>()];
			PXCache destinationCache = graph.Caches[BqlCommand.GetItemType<TDestinationField>()];
			CopyTranslations<TSourceField, TDestinationField>(sourceCache, sourceData, destinationCache, destinationData);
		}

		internal static int[] _GetSlotIndexes(PXCache sender)
		{
			List<int> ret = new List<int>();
			if (HasMultipleLocales)
			{
				foreach (var attr in sender.GetAttributesReadonly(null))
				{
					if (attr is PXDBLocalizableStringAttribute)
					{
						ret.Add(((PXDBLocalizableStringAttribute)attr)._PositionInTranslations);
					}
				}
			}
			return ret.ToArray();
		}

		/// <summary>
		/// Fills in translations for the specified field of the specified record. 
		/// Translations are collected from <paramref name="message"/> .
		/// </summary>
		/// <typeparam name="TField">The type of the destination field.
		/// Translations are filled in for this field.
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <param name="cache">The cache object that is used to search for the attributes of
		/// the <see cref="PXDBLocalizableStringAttribute"/> type.</param>
		/// <param name="data">The record for which translations are filled in.</param>
		/// <param name="message">The message from which translations are collected. 
		/// The message should be translated in <see cref="TranslationMaint"/>. </param>
		public static void SetTranslationsFromMessage<TField>(PXCache cache, object data, string message)
			where TField : IBqlField
		{
			SetTranslationsFromMessageFormatNLA<TField>(cache,  data,  message);
		}


		/// <summary>
		/// Fills in translations for the specified field of the specified record. 
		/// Translations are collected from <paramref name="message"/> .
		/// Strings from  <paramref name="args"/> are not translated.
		/// </summary>
		/// <typeparam name="TField">The type of the destination field.
		/// Translations are filled in for this field.
		/// The field must have <see cref="PXDBLocalizableStringAttribute"/>.</typeparam>
		/// <param name="cache">The cache object that is used to search for the attributes of
		/// the <see cref="PXDBLocalizableStringAttribute"/> type.</param>
		/// <param name="data">The record for which translations are filled in.</param>
		/// <param name="message">The message from which translations are collected. 
		/// <param name="args">Parameters that won't be translated. 
		/// The message should be translated in <see cref="TranslationMaint"/>. </param>
		public static void SetTranslationsFromMessageFormatNLA<TField>(PXCache cache, object data, string message, params string[] args)
			where TField : IBqlField
		{
			string[] languages = cache.GetValueExt(null, typeof(TField).Name + TranslationsPostfix) as string[];
			if (languages != null) {
			string[] translations = new string[languages.Length];
			string translatedMessage;
			for (int i = 0; i < languages.Length; i++)
			{
				using (new PXLocaleScope(languages[i]))
				{
					translatedMessage = PXLocalizer.Localize(message);
					try
					{
						translations[i] = String.Format(translatedMessage, args);
					}
					catch (FormatException) {
						PXTrace.WriteError("Following message could not be translated:" + message + ". As it expects more than " + (args.Length).ToString() + " arguments.");
						throw;
					}
				}
			}
			foreach (PXDBLocalizableStringAttribute attribute in
				cache.GetAttributes<TField>(data).Where(attribute => (attribute is PXDBLocalizableStringAttribute)))
			{
				attribute.SetTranslations(cache, data, translations);
			}
		}
		}

		/// <exclude/>
		public static void EnsureTranslations(Func<string, bool> tableMeet)
		{
			var def = PXContext.GetSlot<Definition>();
			if (def == null)
			{
				PXContext.SetSlot<Definition>(def = PXDatabase.GetSlot<Definition>("Definition", typeof(Locale)));
			}
			if (def != null && !String.IsNullOrEmpty(def.DefaultLocale))
			{
				try
				{
					PX.Data.Maintenance.KeyValueExtHelper.CopyKeyValueExtensions<PXDBLocalizableStringAttribute>(
						fieldName => { return fieldName + def.DefaultLocale.ToUpper(); },
						(typeCode, attribute, tableName) =>
						{
							string valueField = null;
							if (typeCode == TypeCode.String)
							{
								if (!((PXDBLocalizableStringAttribute)attribute).NonDB
									&& !((PXDBLocalizableStringAttribute)attribute).IsProjection
									&& tableMeet(tableName))
								{
									valueField = "ValueString";
									if (attribute is PXDBLocalizableStringAttribute
										&& (((PXDBLocalizableStringAttribute)attribute).Length <= 0
										|| ((PXDBLocalizableStringAttribute)attribute).Length > 256))
									{
										valueField = "ValueText";
									}
								}
							}
							return valueField;
						},
						false);
				}
				catch
				{
				}
			}
		}

		/// <exclude/>
		public static PXDataField GetValueSelect(string tableName, string fieldName, bool isLong)
		{
			var def = PXContext.GetSlot<Definition>();
			if (def == null)
			{
				PXContext.SetSlot<Definition>(def = PXDatabase.GetSlot<Definition>("Definition", typeof(Locale)));
			}
			if (def == null || String.IsNullOrEmpty(def.DefaultLocale))
			{
				return new PXDataField(fieldName);
			}
			var sqlDialect = PXDatabase.Provider.SqlDialect;
			PXDatabaseProviderBase provider = PXDatabase.Provider as PXDatabaseProviderBase;
			string currentLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
			string extTable = tableName + SqlDialectBase.KvExtSuffix;
			Column fieldCol = new Column("FieldName", extTable);
			SQLSwitch locSwitch = new SQLSwitch();

			List<string> ordered = def.DefaultPlusAlternative.Where(_ =>
				!String.Equals(_, def.DefaultLocale, StringComparison.OrdinalIgnoreCase)
				&& (currentLanguage == null || !String.Equals(_, currentLanguage, StringComparison.OrdinalIgnoreCase)))
				.ToList();
			ordered.Insert(0, def.DefaultLocale);
			if (!String.IsNullOrEmpty(currentLanguage))
			{
				ordered.Insert(0, currentLanguage);
			}
			for (int i = 0; i < ordered.Count; i++)
			{
				var value = ordered[i].ToUpper();
				locSwitch.Case(fieldCol.EQ(fieldName + value), new SQLConst(i));
			}
			
			SQLExpression restrictionExp = null;
			bool multicompany = false;
			if (provider != null)
			{
				companySetting setting;
				int cid = provider.getCompanyID(tableName, out setting);
				int[] listCompanies;
				if (setting != null && setting.Flag != companySetting.companyFlag.Separate && provider.tryGetSelectableCompanies(cid, out listCompanies))
				{
					SQLExpression seq = new SQLConst(cid);
					foreach (var item in listCompanies)
					{
						seq = seq.Seq(item);
					}
					restrictionExp = new Column("CompanyID", extTable).In(seq);
					multicompany = true;
				}
				else
				{
					restrictionExp = provider.GetRestrictionExpression(extTable, extTable, true);
				}
			}

			var query = new Query();

			SQLExpression orderedSeq = null;
			for (int i = 0; i < ordered.Count; i++)
			{
				var col = new SQLConst(fieldName + ordered[i].ToUpper());
				orderedSeq = i == 0 ? col : orderedSeq.Seq(col);
			}
			
			query.Field(new Column(isLong ? "ValueText" : "ValueString", extTable))
				.From(new SimpleTable(extTable))
				.Where(new Column("RecordID", extTable).EQ(new Column("NoteID", tableName)
					.And(new Column("FieldName", extTable).In(orderedSeq)))
					.And(restrictionExp)
				)
				.OrderAsc(locSwitch);

			if (multicompany) 
			{
				query.OrderDesc(new Column("CompanyID", extTable));
			}
			query.Limit(1);
			
			return new PXDataField(new SubQuery(query));
		}

		/// <exclude/>
		protected class Definition : IPrefetchable
		{
			public string DefaultLocale;
			public List<string> DefaultPlusAlternative;
			public bool HasMultipleLocales;
			public Dictionary<string, List<string>> LocalesByLanguage = new Dictionary<string, List<string>>();

			public void Prefetch()
			{
				DefaultPlusAlternative = new List<string>();
				//try
				//{
				//	if (PXSiteMap.IsPortal)
				//	{
				//		return;
				//	}
				//}
				//catch
				//{
				//}
				int i = 0;
				HashSet<string> all = new HashSet<string>();
				foreach (PXDataRecord record in PXDatabase.SelectMulti<Locale>(
					new PXDataField<Locale.localeName>(),
					new PXDataField<Locale.isDefault>(),
					new PXDataField<Locale.isAlternative>(),
					new PXDataFieldValue<Locale.isActive>(PXDbType.Bit, 1, true),
					new PXDataFieldOrder<Locale.number>()))
				{
					string name = record.GetString(0);
					if (!String.IsNullOrEmpty(name))
					{
						string locname = name;
						i++;
						name = new CultureInfo(name).TwoLetterISOLanguageName;
						if (record.GetBoolean(1) == true)
						{
							DefaultLocale = name;
						}
						else if (record.GetBoolean(2) != true)
						{
							continue;
						}
						if (all.Add(name))
						{
							DefaultPlusAlternative.Add(name);
						}
						List<string> locs;
						if (!LocalesByLanguage.TryGetValue(name, out locs))
						{
							LocalesByLanguage[name] = locs = new List<string>();
						}
						locs.Add(locname);
					}
				}
				HasMultipleLocales = (i > 1);
			}
		}
		protected Definition _Definition;
		protected Dictionary<string, int> _Indexes;
		protected Dictionary<string, int> _FieldIndexes;
		#endregion

		#region Runtime
		/// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			var restore = NonDB 
				? new RestoreScope()
					.RestoreTo(_=>e.Expr=_, e.Expr)
					.RestoreTo(_=>e.BqlTable=_, e.BqlTable)
				: null;

			using (restore)
			{
				if (_Definition != null)
					CommandPreparingLocalizable(sender, e);
				else
					base.CommandPreparing(sender, e);
			}
			}

		private void CommandPreparingLocalizable(PXCache sender, PXCommandPreparingEventArgs e)
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
				{
					base.CommandPreparing(sender, e);
					PXCommandPreparingEventArgs.FieldDescription description = e.GetFieldDescription();
					if (description != null)
					{
						ISqlDialect sqlDialect = sender.Graph.SqlDialect;
						if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External)
						{
							string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
							List<string> ordered = _Definition.DefaultPlusAlternative.Where(_ =>
								!String.Equals(_, _Definition.DefaultLocale, StringComparison.OrdinalIgnoreCase)
								&& (currentLanguage == null || !String.Equals(_, currentLanguage, StringComparison.OrdinalIgnoreCase)))
								.ToList();
							ordered.Insert(0, _Definition.DefaultLocale);
							if (!String.IsNullOrEmpty(currentLanguage) && _Indexes.ContainsKey(currentLanguage))
							{
								ordered.Insert(0, currentLanguage);
							}

							string tableName = description.BqlTable.Name;

							if (IsSecondLevelProjection)
							{
								var parentLocalizableAttribute = sender.Graph.Caches[description.BqlTable]
									.GetAttributes(_DatabaseFieldName).OfType<PXDBLocalizableStringAttribute>().FirstOrDefault();
								if (parentLocalizableAttribute != null) tableName = parentLocalizableAttribute.BqlTable.Name;
							}

							SimpleTable sqlTable = new SimpleTable(tableName + SqlDialectBase.KvExtSuffix);
							Column sqlField = new Column("FieldName", sqlTable);
							SQLExpression locSeq = SQLExpression.None();
							SQLSwitch locSwitch = new SQLSwitch();

							for (int i = 0; i < ordered.Count; i++)
							{
								var value = ordered[i].ToUpper();
								locSwitch.Case(sqlField.EQ((IsProjection ? _DatabaseFieldName : _FieldName) + value), new SQLConst(i));
							}

							Column sqlValueField = null;
							if (_Length <= 0 || _Length > 256)
							{
								sqlValueField = new Column("ValueText", sqlTable);
							}
							else
							{
								sqlValueField = new Column("ValueString", sqlTable);
							}
							for (int i = 0; i < ordered.Count; i++)
							{
							locSeq = locSeq.Seq((IsProjection ? _DatabaseFieldName : _FieldName) + ordered[i].ToUpper());
							}

							Query q = new Query();
							q.Field(sqlValueField).From(sqlTable).Where(
								new Column("RecordID", sqlTable).EQ(new Column(sender._NoteIDName, e.Table ?? description.BqlTable))
								.And(sqlField.In(locSeq))
							).OrderAsc(locSwitch);
							e.Expr = new SubQuery(q);
						}
						else
						{
							if (description.BqlTable.IsAssignableFrom(BqlTable) && !IsSecondLevelProjection)
							{
								Query q = BqlCommand.GetNoteAttributesJoined(null, description.BqlTable, e.Table, e.Operation);
								e.Expr = new SubQuery(q);
							}
						}
						e.BqlTable = sender.BqlTable;
					}
				}
				else
				{
					string[] translation = sender.GetSlot<string[]>(e.Row, _PositionInTranslations);
					string value;
					if (translation != null && (value = translation[_Indexes[_Definition.DefaultLocale]]) != null)
					{
						e.Value = value;
					}
					base.CommandPreparing(sender, e);
				}
			}

		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (!NonDB)
			{
				base.RowSelecting(sender, e);
			}

			if (_Definition != null)
			{
				string firstColumn = sender.GetValue(e.Row, _FieldOrdinal) as string;
				var sqlDialect = sender.Graph.SqlDialect;

				if (sqlDialect.tryExtractAttributes(firstColumn, _FieldIndexes, out string[] translation))
				{
					string value = null;
					sender.SetSlot(e.Row, _PositionInTranslations, translation, true);
					string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

					if (!String.IsNullOrEmpty(currentLanguage) && _Indexes.ContainsKey(currentLanguage))
					{
						value = translation[_Indexes[currentLanguage]];
					}

					if (String.IsNullOrEmpty(value))
					{
						value = translation[_Indexes[_Definition.DefaultLocale]];
					}

					if (String.IsNullOrEmpty(value))
					{
						foreach (KeyValuePair<string, int> pair in _Indexes)
						{
							if ((value = translation[pair.Value]) != null)
							{
								break;
							}
						}
					}

					sender.SetValue(e.Row, _FieldOrdinal, value);
				}
			}
		}

		internal bool TryTranslateValue(PXGraph graph, string nonLocalizedString, out string localizedString)
		{
			graph.ThrowOnNull(nameof(graph));
			localizedString = null;

			if (string.IsNullOrWhiteSpace(nonLocalizedString) || _Definition == null ||
				!graph.SqlDialect.tryExtractAttributes(nonLocalizedString, _FieldIndexes, out string[] translation))
			{
				return false;
			}

			//Search for translation in current locale
			string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

			if (!string.IsNullOrEmpty(currentLanguage) && _Indexes.TryGetValue(currentLanguage, out int currentLanguageIndex))
			{
				localizedString = translation[currentLanguageIndex];
			}

			if (!string.IsNullOrEmpty(localizedString))
				return true;

			//Fallback to translation in default locale
			int defaultLocaleIndex = _Indexes[_Definition.DefaultLocale];
			localizedString = translation[defaultLocaleIndex];

			if (!string.IsNullOrEmpty(localizedString))
				return true;

			//Fallback to a translation in a random existing locale		
			foreach (var languageIndex in _Indexes.Values)
			{
				localizedString = translation[languageIndex];

				if (!string.IsNullOrEmpty(localizedString))
					return true;
			}
			
			return false;
		}

		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_Definition != null)
			{
				string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
				if (String.IsNullOrEmpty(currentLanguage) || !_Indexes.ContainsKey(currentLanguage))
				{
					currentLanguage = _Definition.DefaultLocale;
				}
				string[] translation;
				if (e.Row == null)
				{
					base.FieldSelecting(sender, e);
					if (e.ReturnState is PXStringState)
					{
						((PXStringState)e.ReturnState).Language = currentLanguage;
					}
				}
				else if ((translation = GetTranslations(sender, e.Row)) != null)
				{
					string value = translation[_Indexes[currentLanguage]];
					string key = null;
					if (value != null)
					{
						key = currentLanguage;
						if (sender.Graph.IsCopyPasteContext && !sender.Graph.IsMobile && translation.Count(_ => !String.IsNullOrEmpty(_)) > 1)
						{
							value = PackTranslations(translation);
						}
					}
					else if ((value = translation[_Indexes[_Definition.DefaultLocale]]) != null)
					{
						key = _Definition.DefaultLocale;
						if (sender.Graph.IsCopyPasteContext && !sender.Graph.IsMobile)
						{
							value = PackTranslations(translation);
						}
					}
					else
					{
						foreach (KeyValuePair<string, int> pair in _Indexes)
						{
							if ((value = translation[pair.Value]) != null)
							{
								key = pair.Key;
								break;
							}
						}
						if (sender.Graph.IsCopyPasteContext && !sender.Graph.IsMobile && value != null)
						{
							value = PackTranslations(translation);
						}
					}
					if (key != null)
					{
						e.ReturnValue = value;
						if (!String.Equals(key, currentLanguage, StringComparison.OrdinalIgnoreCase))
						{
							e.IsAltered = true;
						}
					}
					base.FieldSelecting(sender, e);
					if (e.ReturnState is PXStringState)
					{
						((PXStringState)e.ReturnState).Language = key ?? currentLanguage;
					}
				}
				else
				{
					base.FieldSelecting(sender, e);
					if (e.ReturnState is PXStringState)
					{
						((PXStringState)e.ReturnState).Language = currentLanguage;
					}
				}
			}
			else
			{
				base.FieldSelecting(sender, e);
			}
		}

		protected virtual string PackTranslations(string[] translations)
		{
			StringBuilder bld = new StringBuilder("[");
			foreach (KeyValuePair<string, int> pair in _Indexes)
			{
				if (!String.IsNullOrEmpty(translations[pair.Value]))
				{
					if (bld.Length > 1)
					{
						bld.Append(',');
					}
					bld.Append('{');
					bld.Append(pair.Key);
					bld.Append(':');
					bld.Append(translations[pair.Value]);
					bld.Append('}');
				}
			}
			bld.Append(']');
			return bld.ToString();
		}

		protected virtual string[] UnpackTranslations(string value)
		{
			string[] translations = new string[_Indexes.Count];
			value = value.Replace("[{", "").Replace("}]", "");
			string[] input = value.Split(new string[] { "},{" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string val in input)
			{
				int idx = val.IndexOf(':');
				int position;
				if (idx > 1 && idx + 1 < val.Length - 1 && _Indexes.TryGetValue(val.Substring(0, idx), out position))
				{
					translations[position] = val.Substring(idx + 1);
				}
			}
			return translations;
		}

		protected virtual string[] GetTranslations(PXCache sender, object row)
		{
			string[] alternatives = sender.GetSlot<string[]>(row, _PositionInTranslations);
			if (alternatives == null)
			{
				if (sender.GetStatus(row) != PXEntryStatus.Inserted)
				{
					if (OnDemandCommand.GetKeyValues(sender, row, _BqlTable, _FieldIndexes, out alternatives))
					{
						sender.SetSlot(row, _PositionInTranslations, alternatives, true);
					}
				}
				else
				{
					string value = sender.GetValue(row, _FieldOrdinal) as string;
					if (!String.IsNullOrEmpty(value))
					{
						string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
						if (String.IsNullOrEmpty(currentLanguage) || !_Indexes.ContainsKey(currentLanguage))
						{
							currentLanguage = _Definition.DefaultLocale;
						}
						alternatives = new string[_FieldIndexes.Count];
						alternatives[_Indexes[currentLanguage]] = value;
						//sender._SetTranslations(row, _PositionInTranslations, alternatives);
					}
				}
			}
			return alternatives;
		}
		protected virtual void SetTranslations(PXCache sender, object row, string[] translations)
		{
			sender.SetSlot(row, _PositionInTranslations, translations);
		}
		protected int AdjustPosition(int position, string language, string currentLanguage)
		{
			if (String.Equals(language, currentLanguage, StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}
			else if (String.Equals(language, _Definition.DefaultLocale, StringComparison.OrdinalIgnoreCase))
			{
				return 1;
			}
			else if (String.Equals(currentLanguage, _Definition.DefaultLocale, StringComparison.OrdinalIgnoreCase))
			{
				if (position < _Indexes[currentLanguage])
				{
					return position + 1;
				}
				else
				{
					return position;
				}
			}
			else
			{
				if (position < _Indexes[currentLanguage] && position < _Indexes[_Definition.DefaultLocale])
				{
					return position + 2;
				}
				else if (position > _Indexes[currentLanguage] && position > _Indexes[_Definition.DefaultLocale])
				{
					return position;
				}
				else
				{
					return position + 1;
				}
			}
		}

		protected virtual void Translations_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_Definition != null)
			{
				string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
				if (String.IsNullOrEmpty(currentLanguage) || !_Indexes.ContainsKey(currentLanguage))
				{
					currentLanguage = _Definition.DefaultLocale;
				}
				string[] outgoing = new string[_Definition.DefaultPlusAlternative.Count];
				if (e.Row == null)
				{
					foreach (KeyValuePair<string, int> pair in _Indexes)
					{
						outgoing[AdjustPosition(pair.Value, pair.Key, currentLanguage)] = pair.Key;
					}
				}
				else
				{
					string[] translation = GetTranslations(sender, e.Row);
					if (translation != null)
					{
						foreach (KeyValuePair<string, int> pair in _Indexes)
						{
							outgoing[AdjustPosition(pair.Value, pair.Key, currentLanguage)] = translation[pair.Value];
						}
					}
				}
				if (sender.Graph.IsExport && !sender.Graph.IsMobile)
				{
					e.ReturnValue = PackTranslations(outgoing);
				}
				else
				{
					e.ReturnValue = outgoing;
				}
			}
		}

		protected virtual void Translations_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if ( _Definition != null && e.NewValue is string[])
			{
				if (e.Row != null)
				{
					string[] incoming = (string[])e.NewValue;
					string[] translation = GetTranslations(sender, e.Row) ?? new string[_Definition.DefaultPlusAlternative.Count];
					string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
					if (String.IsNullOrEmpty(currentLanguage) || !_Indexes.ContainsKey(currentLanguage))
					{
						currentLanguage = _Definition.DefaultLocale;
					}
					bool updated = false;
					foreach (KeyValuePair<string, int> pair in _Indexes)
					{
						string val = incoming[AdjustPosition(pair.Value, pair.Key, currentLanguage)];
						if (String.IsNullOrEmpty(val))
						{
							val = null;
						}
						updated |= !String.Equals(translation[pair.Value], val, StringComparison.InvariantCultureIgnoreCase);
						translation[pair.Value] = val;
					}
					if (updated)
					{
						sender.SetSlot(e.Row, _PositionInTranslations, translation);
						var status = sender.GetStatus(e.Row);
						sender.SetValue(e.Row, _FieldOrdinal, incoming.FirstOrDefault(_ => _ != null));
						if (status == PXEntryStatus.Inserted)
							sender.SetStatus(e.Row, PXEntryStatus.Inserted);
						if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
						{
							sender.SetStatus(e.Row, PXEntryStatus.Modified);
							sender.IsDirty = true;
						}
					}
				}
			}
		}

		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Row != null && _Definition != null)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((IsProjection || IsSecondLevelProjection) && sender.BqlTable != null && _BqlTable != null &&
						sender.BqlTable.Name != _BqlTable.Name)
					{
						return;
					}

					// some fields can be in PK but don't have IsKey=true property
					var point = PXDatabase.Provider.CreateDbServicesPoint();
					HashSet<string> pkFields = null;
					var tableSchema = point.Schema.GetTable(sender.BqlTable.Name);
					if (tableSchema != null)
					{
						pkFields = tableSchema.GetPrimaryKey().Columns.Select(c => c.Name).ToHashSetAcu(StringComparer.OrdinalIgnoreCase);
					}
					List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
					foreach (string descr in sender.Fields // Not only keys can be restrictions, e.g. PXExtraKeyAttribute
						.Where(fieldName => !sender.GetAttributesReadonly(null, fieldName)
							.OfType<PXDBTimestampAttribute>()
							.Any())) // except timestamps too
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						object val = sender.GetValue(e.Row, descr);
						sender.RaiseCommandPreparing(descr, e.Row, val, PXDBOperation.Update, null, out description);
						if (description != null && description.Expr != null && (description.IsRestriction || pkFields != null && pkFields.Contains(descr)))
						{
							pars.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
						}
					}
					{
						string descr = sender._NoteIDName;
						PXCommandPreparingEventArgs.FieldDescription description;
						object val = sender.GetValue(e.Row, descr);
						object oldval = val;
						if (val == null)
						{
							val = SequentialGuid.Generate();
							sender.SetValue(e.Row, descr, val);
						}
						sender.RaiseCommandPreparing(descr, e.Row, val, PXDBOperation.Update, null, out description);
						if (description != null && description.Expr != null)
						{
							PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue);
							assign.OldValue = oldval;
							assign.IsChanged = (oldval == null);
							pars.Add(assign);
							assign = new PXDataFieldAssign("RecordID", description.DataType, description.DataLength, description.DataValue);
							assign.Storage = StorageBehavior.KeyValueKey;
							assign.OldValue = val;
							assign.IsChanged = false;
							pars.Add(assign);
						}
					}
					if ((e.Operation & PXDBOperation.Delete) == PXDBOperation.Delete)
					{
					}
					else
					{
						string[] translation = GetTranslations(sender, e.Row);
						string[] originals = sender.GetSlot<string[]>(e.Row, _PositionInTranslations, true);
						if (translation != null)
						{
							string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
							if (String.IsNullOrEmpty(currentLanguage) || !_Indexes.ContainsKey(currentLanguage))
							{
								currentLanguage = _Definition.DefaultLocale;
							}
							foreach (KeyValuePair<string, int> pair in _FieldIndexes)
							{
								//if (String.Equals(pair.Key, currentLanguage, StringComparison.OrdinalIgnoreCase)
								//	&& translation[pair.Value] == null)
								//{
								//	string currentval = sender.GetValue(e.Row, _FieldOrdinal) as string;
								//	if (!String.IsNullOrEmpty(currentval))
								//	{
								//		translation[pair.Value] = currentval;
								//	}
								//}
								string val = translation[pair.Value];
								string origval = originals != null ? originals[pair.Value] : null;
								PXDataFieldAssign assign = new PXDataFieldAssign(pair.Key, PXDbType.NVarChar, 256, val);
								if (_Length <= 0 || _Length > 256)
								{
									assign.Storage = StorageBehavior.KeyValueText;
								}
								else
								{
									assign.Storage = StorageBehavior.KeyValueString;
								}
								assign.OldValue = origval;
								assign.IsChanged = !PXLocalesProvider.CollationComparer.Equals(val, origval);
								pars.Add(assign);
								if (String.Equals(_Definition.DefaultLocale, pair.Key.Substring(pair.Key.Length -2), StringComparison.OrdinalIgnoreCase)
										&& !String.IsNullOrEmpty(val))
								{
									PXCommandPreparingEventArgs args = new PXCommandPreparingEventArgs(e.Row, val, PXDBOperation.Update, _BqlTable, sender.Graph.SqlDialect);
									base.CommandPreparing(sender, args);
									PXCommandPreparingEventArgs.FieldDescription description = args.GetFieldDescription();
									if (description != null && description.Expr != null && description.DataValue != null)
									{
										assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue);
										object oldval = sender.GetValue(e.Row, _FieldName);
										assign.OldValue = oldval;
										assign.IsChanged = !PXLocalesProvider.CollationComparer.Equals(val, oldval as string);
										pars.Add(assign);
									}
								}
							}
							PXTransactionScope.SetChildIdentity();
							sender.Graph.ProviderUpdate(_BqlTable, pars.ToArray());
						}
					}
				}
			}
		}

		/// <exclude/>
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (_Definition != null && e.Row != null)
			{
				if (e.NewValue is string && ((string)e.NewValue).StartsWith("[{") && ((string)e.NewValue).EndsWith("}]"))
				{
					string[] incoming = UnpackTranslations((string)e.NewValue);
					sender.SetSlot(e.Row, _PositionInTranslations, incoming);
					sender.SetValue(e.Row, _FieldOrdinal, incoming.FirstOrDefault(_ => _ != null));
					if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
					{
						sender.SetStatus(e.Row, PXEntryStatus.Modified);
						sender.IsDirty = true;
					}
					e.NewValue = sender.GetValue(e.Row, _FieldOrdinal);
				}
				else
				{
					base.FieldUpdating(sender, e);
					string currentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
					if (!String.IsNullOrEmpty(currentLanguage) && _Indexes.ContainsKey(currentLanguage))
					{
						string[] translation = GetTranslations(sender, e.Row) ?? new string[_Definition.DefaultPlusAlternative.Count];
						translation[_Indexes[currentLanguage]] = e.NewValue as string;
						sender.SetSlot(e.Row, _PositionInTranslations, translation);
					}
				}
			}
			else
			{
				base.FieldUpdating(sender, e);
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (NonDB && typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
			{
				subscribers.Remove(this as ISubscriber);
			}
		}
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Definition = PXContext.GetSlot<Definition>();
			if (_Definition == null)
			{
				PXContext.SetSlot<Definition>(_Definition = PXDatabase.GetSlot<Definition>("Definition", typeof(Locale)));
			}
			sender.Graph._RecordCachedSlot(this.GetType(), _Definition, () => PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("Definition", typeof(Locale))));
			if (_Definition != null && String.IsNullOrEmpty(_Definition.DefaultLocale))
			{
				_Definition = null;
			}
			if (_Definition != null)
			{
				_Indexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				_FieldIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < _Definition.DefaultPlusAlternative.Count; i++)
				{
					_Indexes[_Definition.DefaultPlusAlternative[i]] = i;
					_FieldIndexes[(IsProjection ? _DatabaseFieldName : _FieldName) + _Definition.DefaultPlusAlternative[i].ToUpper()] = i;
				}
				if (NonDB)
				{
					sender.RowSelecting += RowSelecting;
				}
				string name = _FieldName + TranslationsPostfix;
				if (!sender.Fields.Contains(name))
				{
					sender.Fields.Add(name);
					name = name.ToLower();
					sender.FieldSelectingEvents[name] += Translations_FieldSelecting;
					sender.FieldUpdatingEvents[name] += Translations_FieldUpdating;
				}
				int cnt = _Definition.DefaultPlusAlternative.Count;
				_PositionInTranslations = sender.SetupSlot<string[]>(
					() => new string[cnt],
					(item, copy) =>
					{
						for (int i = 0; i < cnt && i < item.Length && i < copy.Length; i++)
						{
							item[i] = copy[i];
						}
						return item;
					},
					(item) => (string[])item.Clone()
					);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBTextAttribute
	/// <summary>Maps a DAC field of <tt>string</tt> type to the database
	/// column of <tt>nvarchar</tt> or <tt>varchar</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBText(IsUnicode = true)]
	/// [PXUIField(DisplayName = "Activity Details")]
	/// public virtual string Body { ... }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBTextAttribute : PXDBStringAttribute
	{
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			if (_DatabaseFieldName != null && (e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy) {
				e.Expr = SQLExpression.Null();
			}
		}
	}
	#endregion

	#region PXDBLocalStringAttribute
	/// <summary>Maps a string DAC field to a localized string column in the
	/// database.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database columns that have the culture
	/// information specified in their names. For example, for the
	/// <tt>Description</tt> field, the English-specific column is
	/// <tt>DescriptionenGB</tt>, the Russian-specific column is
	/// <tt>DescriptionruRU</tt>.</remarks>
	/// <example>
	/// <code>
	/// [PXDBLocalString(255, IsUnicode = true)]
	/// [PXUIField(Visibility = PXUIVisibility.SelectorVisible)]
	/// public virtual String Title { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBLocalStringAttribute : PXDBStringAttribute
	{
		protected static Dictionary<string, bool> _FieldForCultureExists = new Dictionary<string, bool>();
		private static System.Threading.ReaderWriterLock rwLock = new System.Threading.ReaderWriterLock();

		#region Ctor
		/// <summary>Initializes a new instance with the default
		/// parameters.</summary>
		public PXDBLocalStringAttribute()
			: base()
		{
		}

		/// <summary>Initializes a new instance with the specified maximum
		/// length.</summary>
		/// <param name="Length">The maximum length of the field value.</param>
		public PXDBLocalStringAttribute(int Length)
			: base(Length)
		{
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			string culturename = Thread.CurrentThread.CurrentCulture.Name.Replace("-", "");
			if ("enUS".Equals(culturename, StringComparison.OrdinalIgnoreCase)) {
				PrepareCommandImpl(_DatabaseFieldName, e);
				return;
			}

			//rwLock.AcquireReaderLock(-1);
			using (var scope = new PXReaderWriterScope(rwLock))
			{
				scope.AcquireReaderLock();

				bool fieldexists;
				if (_FieldForCultureExists.TryGetValue(culturename, out fieldexists)) {
					if (fieldexists) {
						PrepareCommandImpl(_DatabaseFieldName + culturename, e);
					}
					else {
						PrepareCommandImpl(_DatabaseFieldName, e);
					}
				}
				else {

					scope.UpgradeToWriterLock();
					if (_FieldForCultureExists.TryGetValue(culturename, out fieldexists))
					{
						if (fieldexists)
						{
							PrepareCommandImpl(_DatabaseFieldName + culturename, e);
						}
						else
						{
							PrepareCommandImpl(_DatabaseFieldName, e);
						}
					}
					else
					{
						TryPrepareCommand(e, culturename);
					}



	
				}
			}

		}

		private bool TryPrepareCommand(PXCommandPreparingEventArgs e, string culturename)
		{
			try {
				using (PXDatabase.SelectSingle(BqlTable, new PXDataField(_DatabaseFieldName + culturename))) {
				}
			}
			catch (Exception) {
				PrepareCommandImpl(_DatabaseFieldName, e);
				_FieldForCultureExists.Add(culturename, false);
				return false;
			}
			PrepareCommandImpl(_DatabaseFieldName + culturename, e);
			_FieldForCultureExists.Add(culturename, true);
			return true;
		}

		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			if (dbFieldName != null && (e.Operation & PXDBOperation.Command) != PXDBOperation.Insert && (e.Operation & PXDBOperation.Command) != PXDBOperation.Update)
			{
				var table = e.Table ?? _BqlTable;
				e.Expr = new Column(dbFieldName, table).Coalesce(new Column(_DatabaseFieldName, table));
			}
		}
		#endregion
	}
	#endregion

	#region PXDBByteAttribute
	/// <summary>Maps a DAC field of <tt>byte?</tt> type to the database
	/// column of <tt>tinyint</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// 	<code title="Example" description="The code below shows the CacheAttached event handler, which is defined in a graph and replaces attributes of the FilterRow.Condition field within the current graph." lang="CS">
	/// [PXDefault]
	/// [PXDBByte]
	/// [PXUIField(DisplayName = "Condition")]
	/// protected virtual void FilterRow_Condition_CacheAttached(PXCache sender)
	/// {
	/// }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBByteAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _MinValue = byte.MinValue;
		protected int _MaxValue = byte.MaxValue;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		/// <summary>Gets or sets the maximum value for the field.</summary>
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.TinyInt;
			e.DataLength = 1;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetByte(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				byte val;
				if (byte.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(byte), null);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBShortAttribute
	/// <summary>Maps a DAC field of <tt>short?</tt> type to the database
	/// column of <tt>smallint</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBShort(MaxValue = 9, MinValue = 0)]
	/// public virtual short? TaxReportPrecision { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBShortAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _MinValue = short.MinValue;
		protected int _MaxValue = short.MaxValue;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.SmallInt;
			e.DataLength = 2;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt16(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				short val;
				if (short.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(short), null);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBUShortAttribute
	/// <summary>Maps a DAC field of <tt>ushort?</tt> type to the database
	/// column of <tt>int</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBUShort()]
	/// public virtual ushort LineNbr { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBUShortAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _MinValue = ushort.MinValue;
		protected int _MaxValue = ushort.MaxValue;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.Int;
			e.DataLength = 2;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, (ushort?)e.Record.GetInt32(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				ushort val;
				if (ushort.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(ushort), null);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBGuidAttribute
	/// <summary>Map a DAC field of <tt>Guid?</tt> type to the database column
	/// of <tt>uniqueidentifier</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// 	<code title="Example" description="The attribute below binds the field to the unique identifier column and assigns a default value to the field." lang="CS">
	/// [PXDBGuid(true)]
	/// public virtual Guid? SetupID { get; set; }</code>
	/// 	<code title="Example1" description="The attribute below binds the field to the unique identifier column. The field becomes a key field." groupname="Example" lang="CS">
	/// [PXDBGuid(IsKey = true)]
	/// public virtual Guid? SetupID { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBGuidAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXFieldDefaultingSubscriber
	{
		private readonly bool _withDefaulting = false;

		/// <summary>
		/// Initializes a new instance of the attribute.
		/// </summary>
		/// <param name="withDefaulting">Indicates whether the default
		/// value should be set for the field.</param>
		public PXDBGuidAttribute(bool withDefaulting = false)
		{
			_withDefaulting = withDefaulting;
		}

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.UniqueIdentifier;
			e.DataLength = 16;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetGuid(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				Guid val;
				if (GUID.TryParse((string)e.NewValue, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXGuidState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1);
			}
		}
		#endregion

		/// <exclude/>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_withDefaulting) e.NewValue = newGuid();
		}

		protected virtual Guid newGuid()
		{
			return Guid.NewGuid();
		}
	}
	#endregion

	#region PXDBSequentialGuidAttribute
	/// <summary>
	/// GUID, defaulting by "old" algorithm (timestamp + ethernet MAC address)
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBSequentialGuidAttribute : PXDBGuidAttribute
	{
		public PXDBSequentialGuidAttribute() : base(true) { }
		protected override Guid newGuid()
		{
			return SequentialGuid.Generate();
		}
	}
	#endregion

	#region PXDBGuidMaintainDeletedAttribute
	/// <summary>
	/// This attribute is equivalent to the PXDBGuidAttribute but doesn't update Guid when restoring deleted record.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBGuidMaintainDeletedAttribute : PXDBGuidAttribute
	{
		public PXDBGuidMaintainDeletedAttribute() : base() { }

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) != PXDBOperation.Second)
				base.CommandPreparing(sender, e);
		}
	}
	#endregion

	#region PXDBBoolAttribute
	/// <summary>Maps a DAC field of <tt>bool?</tt> type to the database
	/// column of <tt>bit</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBBool()]
	/// [PXDefault(false)]
	/// public virtual Boolean? Scheduled { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBBoolAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.Bit;
			e.DataLength = 1;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetBoolean(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			PXBoolAttribute.ConvertValue(e);
			//if (e.NewValue is string)
			//{
			//    bool val;
			//    if (bool.TryParse((string)e.NewValue, out val))
			//    {
			//        e.NewValue = val;
			//    }
			//    else
			//    {
			//        string newValue = e.NewValue as string;
			//        if (!string.IsNullOrEmpty(newValue))
			//            switch (newValue.Trim())
			//            {
			//                case "1": e.NewValue = true; break;
			//                case "0": e.NewValue = false; break;
			//                default: e.NewValue = null; break;
			//            }
			//        else e.NewValue = null;
			//    }
			//}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), _IsKey, null, -1, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBIntAttribute
	/// <summary>Maps a DAC field of <tt>int?</tt> type to the database column of <tt>int</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field. The field becomes bound to the database column with the same name.</remarks>
	/// <example>
	///   <code title="Example" description="The attribute below maps a field to the database column and explicitly sets the minimum and maximum values for the field." lang="CS">
	/// [PXDBInt(MinValue = 0, MaxValue = 365)]
	/// public virtual int? ReceiptTranDaysBefore { get; set; }</code>
	/// 	<code title="Example2" description="The attribute below maps a field to the database column and sets the properties inherited from the PXDBField attribute." groupname="Example" lang="CS">
	/// [PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.branchID))]
	/// [PXSelector(typeof(Branch.branchID),
	///             SubstituteKey = typeof(Branch.branchCD))]
	/// public virtual int? BranchID { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBIntAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _MinValue = int.MinValue;
		protected int _MaxValue = int.MaxValue;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		/// <example>
		/// The attribute below maps a field to the database column and
		/// explicitly sets the minimum and maximum values for the field.
		/// <code>
		/// [PXDBInt(MinValue = 0, MaxValue = 365)]
		/// public virtual int? ReceiptTranDaysBefore { get; set; }
		/// </code>
		/// </example>
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		/// <summary>Gets or sets the maximum value for the field.</summary>
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.Int;
			e.DataLength = 4;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				if (_IsKey && !e.IsReadOnly)
				{
					int? key;
					sender.SetValue(e.Row, _FieldOrdinal, (key = e.Record.GetInt32(e.Position)));
					if ((key == null || sender.IsPresent(e.Row)) && sender.Graph.GetType() != typeof(PXGraph) && PXView.LegacyQueryCacheModeEnabled)
					{
						e.Row = null;
					}
				}
				else
				{
					sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt32(e.Position));
				}
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				int val;
				if (int.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(int), null);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBLongAttribute
	/// <summary>Maps a DAC field of <tt>int64?</tt> type to the database
	/// column of <tt>bigint</tt> type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBLong()]
	/// public virtual long? CuryInfoID { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBLongAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected Int64 _MinValue = Int64.MinValue;
		protected Int64 _MaxValue = Int64.MaxValue;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public Int64 MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		/// <summary>Gets or sets the maximum value for the field.</summary>
		public Int64 MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.BigInt;
			e.DataLength = 8;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt64(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				Int64 val;
				if (Int64.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXLongState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, typeof(Int64));
			}
		}
		#endregion
	}
	#endregion

	#region PXDBFloatAttribute
	/// <summary>Maps a DAC field of <tt>float?</tt> type to the 4-bytes floating point column in the database.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBFloatAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected float _MinValue = float.MinValue;
		protected float _MaxValue = float.MaxValue;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public float MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		/// <summary>Gets or sets the maximum value for the field.</summary>
		public float MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance with default parameters.</summary>
		public PXDBFloatAttribute()
		{
		}
		/// <summary>Initializes a new instance of the attribute with the given
		/// precision. The precision is the number of digits after the comma. If a
		/// user enters a value with greater number of fractional digits, the
		/// value will be rounded.</summary>
		/// <param name="precision">The value to use as the precision.</param>
		public PXDBFloatAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		/// <exclude/>
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null) {
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name)) {
				if (attr is PXDBFloatAttribute) {
					((PXDBFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <exclude/>
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name)) {
				if (attr is PXDBFloatAttribute) {
					((PXDBFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.Real;
			e.DataLength = 4;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetFloat(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				float val;
				if (float.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
			if (e.NewValue != null) {
				e.NewValue = Convert.ToSingle(Math.Round((float)e.NewValue, _Precision));
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXFloatState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBDoubleAttribute
	/// <summary>Maps a DAC field of <tt>double?</tt> type to the 8-bytes
	/// floating point column in the database.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDoubleAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected double _MinValue = double.MinValue;
		protected double _MaxValue = double.MaxValue;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public double MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		/// <summary>Gets or sets the maximum value for the field.</summary>
		public double MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance of the attribute with default
		/// parameters.</summary>
		public PXDBDoubleAttribute()
		{
		}
		/// <summary>Initializes a new instance of the attribute with the given
		/// precision. The precision is the number of digits after the comma. If a
		/// user enters a value with greater number of fractional digits, the
		/// value will be rounded.</summary>
		/// <param name="precision">The value to use as the precision.</param>
		public PXDBDoubleAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		/// <exclude/>
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null) {
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name)) {
				if (attr is PXDBDoubleAttribute) {
					((PXDBDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <exclude/>
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name)) {
				if (attr is PXDBDoubleAttribute) {
					((PXDBDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.Float;
			e.DataLength = 8;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetDouble(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				double val;
				if (double.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
			if (e.NewValue != null) {
				e.NewValue = Math.Round((Double)e.NewValue, _Precision);
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXDoubleState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBDecimalAttribute
	/// <summary>Maps a DAC field of <tt>decimal?</tt> type to the database
	/// column of <tt>decimal</tt> type.</summary>
	/// <remarks>
	/// 	<para>The attribute is added to the value declaration of a DAC field. The field becomes bound to the database column with the same name. A minimum value, maximum
	/// value, and precision can be specified. The precision can be calculated at runtime using BQL. The default precision is 2.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="Declaration of a DAC field with a specific precision is shown below." lang="CS">
	/// [PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
	/// public virtual decimal? Price { get; set; }</code>
	/// 	<code title="Example1" description="Declaration of a DAC field with a precision calculated at runtime. The BQL query in this example will search for the Currency data record that satisfies the specified Where condition. The field precision will be set to the DecimalPlaces value from this data record." groupname="Example" lang="CS">
	/// [PXDBDecimal(typeof(
	///     Search&lt;Currency.decimalPlaces,
	///         Where&lt;Currency.curyID, Equal&lt;Current&lt;POCreateFilter.vendorID&gt;&gt;&gt;&gt;
	/// ))]
	/// public virtual decimal? OrderTotal { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDecimalAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXRowPersistingSubscriber
	{
		#region State

		/// <exclude/>
		public class DBDecimalProperties
		{
			public int? _scale;
			public int? _precision;
			public decimal? _maxValue;

			public int? Scale
			{
				get
				{
					//_sync.AcquireReaderLock(-1);
					using (var scope = new PXReaderWriterScope(_sync))
					{
						scope.AcquireReaderLock();
						return _scale;
					}

				}
			}

			public int? Precision
			{
				get
				{
					using (var scope = new PXReaderWriterScope(_sync))
					{
						scope.AcquireReaderLock();
						return _precision;
					}
					
				}
			}

			public decimal? MaxValue
			{
				get
				{
					using (var scope = new PXReaderWriterScope(_sync))
					{
						scope.AcquireReaderLock();
						return _maxValue;
					}

				}
			}


			private ReaderWriterLock _sync = new ReaderWriterLock();

			public void Fill(Type table, string field)
			{
				using (var scope = new PXReaderWriterScope(_sync))
				{
					scope.AcquireReaderLock();
					if (_scale != null && _precision != null && _maxValue != null)
						return;

					//var lc = _sync.UpgradeToWriterLock(-1);
					scope.UpgradeToWriterLock();

					if (_scale != null && _precision != null && _maxValue != null)
						return;
					try
					{
						var tableHeader = PXDatabase.Provider.GetTableStructure(table.Name);
						if (tableHeader != null)
						{
							var column = tableHeader
								.Columns.FirstOrDefault(c => string.Equals(c.Name, field, StringComparison.OrdinalIgnoreCase));
							if (column != null)
							{
								_scale = column.Scale;
								_precision = column.Precision;
								if (column.Precision - column.Scale <= 28)
								_maxValue = (decimal?)Math.Pow(10, (double)(column.Precision - column.Scale));
								else
									_maxValue = decimal.MaxValue;
							}
							else
							{
								_scale = 29;
								_precision = 28;
								_maxValue = decimal.MaxValue;
							}
						}
					}
					catch
					{
					}

				}

			}

			public bool IsSet
			{
				get
				{
					using (var scope = new PXReaderWriterScope(_sync))
					{
						scope.AcquireReaderLock();
						return _scale != null && _precision != null && _maxValue != null;
					}
					
				}
			}
		}

		// because attributes instantiating by copying there will be only one instance for all instances of field.
		/// <exclude/>
		public DBDecimalProperties DBProperties { get; private set; }

		protected int? _Precision = 2;
		protected decimal _MinValue = decimal.MinValue;
		protected decimal _MaxValue = decimal.MaxValue;
		protected Type _Type;
		protected BqlCommand _Select;
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public double MinValue
		{
			get
			{
				return (double)_MinValue;
			}
			set
			{
				_MinValue = (decimal)value;
			}
		}
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public double MaxValue
		{
			get
			{
				return (double)_MaxValue;
			}
			set
			{
				_MaxValue = (decimal)value;
			}
		}

		protected internal virtual int? Precision => _Precision;

		protected Type _SignFormula;
		/// <exclude />
		public Type SignFormula
		{
			get { return _SignFormula; }
			set
			{
				if (!typeof(IBqlCreator).IsAssignableFrom(value))
					throw new PXException(ErrorMessages.SignFormulaShouldBeIBqlCreator, GetSignedFieldName());

				if (_IsInitialized)
					throw new PXNotSupportedException(ErrorMessages.SignFormulaShouldBeSetBeforeCacheAttached, GetSignedFieldName());

				_SignFormula = value;
			}
		}

		/// <exclude />
		public const string SignSuffix = "Signed";
		protected bool _IsInitialized = false;

		#endregion

		#region Ctor
		/// <summary>Initializes a new instance with the default precision, which
		/// equals 2.</summary>
		/// <example>
		/// <code>
		/// [PXDBDecimal(MaxValue = 100, MinValue = 0)]
		/// [PXDefault(TypeCode.Decimal, "50.0")]
		/// [PXUIField(DisplayName = "Group/Document Discount Limit (%)")]
		/// public virtual Decimal? DiscountLimit { get; set; }
		/// </code>
		/// </example>
		public PXDBDecimalAttribute()
		{
			DBProperties = new DBDecimalProperties();
		}
		/// <summary>Initializes a new instance with the given
		/// precision.</summary>
		/// <param name="precision">The precision value.</param>
		/// <example>
		///   <code title="" description="" lang="CS">
		/// [PXDBDecimal(4)]
		/// [PXDefault(TypeCode.Decimal, "0.0")]
		/// public virtual Decimal? TaxTotal { get; set; }</code>
		/// </example>
		public PXDBDecimalAttribute(int precision)
			: this()
		{
			_Precision = precision;
		}

		/// <summary>Initializes a new instance with the precision calculated at runtime with a BQL query.</summary>
		/// <param name="type">A BQL query based on a class derived from
		/// <tt>IBqlSearch</tt> or <tt>IBqlField</tt>. For example, the parameter
		/// can be set to <tt>typeof(Search&lt;...&gt;)</tt>, or
		/// <tt>typeof(Table1.field)</tt>.</param>
		/// <example>
		/// The code below shows declaration of a DAC field with a precision calculated at runtime. The BQL query in this example will search for the <tt>Currency</tt>
		/// data record that satisfies the specified <tt>Where</tt> condition. The field precision will be set to the <tt>DecimalPlaces</tt> value from this data record.
		/// <code title="" description="" lang="CS">
		/// [PXDBDecimal(typeof(
		///     Search&lt;Currency.decimalPlaces,
		///         Where&lt;Currency.curyID, Equal&lt;Current&lt;POCreateFilter.vendorID&gt;&gt;&gt;&gt;
		/// ))]
		/// public virtual decimal? OrderTotal { get; set; }</code></example>
		public PXDBDecimalAttribute(Type type)
			: this()
		{
			if (type == null)
			{
				throw new PXArgumentException(nameof(type), ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(type))
			{
				_Select = BqlCommand.CreateInstance(type);
				_Type = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			}
			else if (type.IsNested && typeof(IBqlField).IsAssignableFrom(type))
			{
				_Type = BqlCommand.GetItemType(type);
				_Select = BqlCommand.CreateInstance(typeof(Search<>), type);
			}
			else
			{
				throw new PXArgumentException(nameof(type), ErrorMessages.CantCreateForeignKeyReference, type);
			}
		}

		#endregion

		#region Runtime

		/// <summary>Sets the precision in the attribute instance that marks the
		/// field with the specified name in a particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBDecimal</tt> type.</param>
		/// <param name="data">The data record the method is applied to.</param>
		/// <param name="name">The name of the field that is be marked with the
		/// attribute.</param>
		/// <param name="precision">The new precision value.</param>
		/// <example>
		/// The code below shows the <tt>RowSelected</tt> event handler (used to
		/// configure the UI at run time), in which you set the precision for the
		/// <tt>Qty</tt> field in the provided data record.
		/// <code>
		/// protected virtual void LotSerOptions_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		/// {
		///     LotSerOptions opt = (LotSerOptions)e.Row;
		///     ...
		///     PXDBDecimalAttribute.SetPrecision(sender, opt, "Qty", (opt.IsSerial == true ? 0 : INSetupDecPl.Qty));
		///     ...
		/// }
		/// </code>
		/// </example>
		public static void SetPrecision(PXCache cache, object data, string name, int? precision)
		{
			if (data == null) {
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name)) {
				if (attr is PXDBDecimalAttribute) {
					((PXDBDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <summary>Sets the precision in the attribute instance that marks the
		/// field with the specified name in all data records in the cache
		/// object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBDecimal</tt> type.</param>
		/// <param name="name">The name of the field that is be marked with the
		/// attribute.</param>
		/// <param name="precision">The new precision value.</param>
		public static void SetPrecision(PXCache cache, string name, int? precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name)) {
				if (attr is PXDBDecimalAttribute) {
					((PXDBDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation

		protected string Check(object value)
		{
			if (value is decimal) {
				decimal val = Normalize((decimal)value);

				if (!DBProperties.IsSet) {
					DBProperties.Fill(_BqlTable, _DatabaseFieldName);
				}
				// if can`t read properties - ignoring check.
				if (DBProperties.IsSet) {
					if (Math.Abs(val) >= DBProperties.MaxValue) {
						return PXMessages.LocalizeFormat(ErrorMessages.InvalidDecimalValue, _FieldName);
					}
				}
			}

			return null;
		}

		protected decimal Normalize(decimal value)
		{
			return decimal.Round(value, 28);
		}

		/// <exclude/>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object val = sender.GetValue(e.Row, _FieldOrdinal);
			string error = Check(val);
			if (error != null) {
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(error))) {
					throw new PXRowPersistingException(_FieldName, null, error);
				}
			}
		}

		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = PXDbType.Decimal;
			e.DataLength = 16;
		}

		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetDecimal(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				decimal val;
				if (decimal.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
			if (e.NewValue != null) {
				_ensurePrecision(sender, e.Row);
				if (_Precision != null) {
					e.NewValue = Math.Round((decimal)e.NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
				}
				string error = Check(e.NewValue);
				if (error != null) {
					throw new PXSetPropertyException(error);
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				_ensurePrecision(sender, e.Row);
				e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		protected virtual void _ensurePrecision(PXCache sender, object row)
		{
			if (_Type != null) {
				PXView view = sender.Graph.TypedViews.GetView(_Select, true);
				object item = null;
				try {
					List<object> list = view.SelectMultiBound(new object[] { row });
					if (list.Count > 0) item = list[0];
				}
				catch {
				}
				if (item != null) {
					int? prec = GetItemPrecision(view, item);
					if (prec != null)
						_Precision = prec;
				}
			}
		}

		/// <summary>Retrieves the precision value if it is set by a BQL query specified in the constructor, and sets its to all attribute instances in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBDecimal</tt> type.</param>
		public static void EnsurePrecision(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				PXDBDecimalAttribute decattr = attr as PXDBDecimalAttribute;
				if (decattr != null && decattr.AttributeLevel == PXAttributeLevel.Cache)
				{
					int? oldValue = decattr._Precision;
					try
					{
						decattr._Precision = null;
						decattr._ensurePrecision(cache, null);
						if (decattr._Precision == null)
							continue;
						oldValue = (int)decattr._Precision;

						cache.SetAltered(decattr._FieldName, true);
						decattr._Type = null;
					}
					catch (InvalidOperationException) { }
					finally
					{
						decattr._Precision = oldValue;
					}
				}
			}
		}

		/// <exclude />
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_IsInitialized = true;
			AddVirtualFieldWithSign(sender);
		}
		#endregion

		#region Sign

		protected string GetSignedFieldName() => $"{_FieldName}_{SignSuffix}";

		protected virtual void AddVirtualFieldWithSign(PXCache cache)
		{
			if (SignFormula == null) return;

			var fieldName = GetSignedFieldName();
			if (cache.Fields.Contains(fieldName)) return;
			cache.Fields.Add(fieldName);

			var slotIndex = cache.SetupSlot<decimal?>(
				() => null,
				(item, copy) => copy,
				(item) => item);

			SignAttribute = new PXSignAttribute
			{
				SlotIndex = slotIndex,
				BqlTable = BqlTable,
				FieldName = fieldName
			};
			cache.RowSelecting += SignAttribute.RowSelecting;

			var attrDBCalced  = new PXDBCalcedAttribute(SignFormula, typeof(decimal))
			{
				BqlTable = BqlTable,
				FieldName = fieldName
			};
			var attrDBDecimal = new PXDBDecimalAttribute()
			{
				BqlTable = BqlTable,
				FieldName = fieldName
			};

			cache.FieldSelectingEvents.Add(fieldName, (sender, e) =>
			{
				decimal? value = sender.GetSlot<decimal?>(e.Row, slotIndex);
				var state = PXDecimalState.CreateInstance(value, Precision, fieldName, _IsKey, -1, _MinValue, _MaxValue);
				e.ReturnValue = state;
			});
			cache.CommandPreparingEvents.Add(fieldName, (sender, e) =>
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
				{
					attrDBDecimal.CommandPreparing(sender, e);
					attrDBCalced.CommandPreparing(sender, e);
				}
				else
				{
					decimal? value = sender.GetSlot<decimal?>(e.Row, slotIndex);
					if (value != null)
						e.Value = value;
				}
			});
		}

		private PXSignAttribute SignAttribute;

		/// <exclude />
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber(subscribers);
			if (SignAttribute != null && typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
				subscribers.Add(SignAttribute as ISubscriber);
		}

		private class PXSignAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber
		{
			public int SlotIndex;

			public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
			{
				if (e.Row != null && e.Record != null)
				{
					var val = e.Record.GetDecimal(e.Position);
					sender.SetSlot(e.Row, SlotIndex, val, true);
				}
				e.Position++;
			}
		}
		

		#endregion

		protected virtual int? GetItemPrecision(PXView view, object item)
		{
			if (item is PXResult) item = ((PXResult)item)[0];
			return item != null ? (short?)view.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name) : null;
		}

	}

	#endregion

	#region PXDBDateAttribute
	/// <summary>Maps a DAC field of <tt>DateTime?</tt> type to the database column of <tt>datetime</tt> or <tt>smalldatetime</tt> type, depending on the
	/// <tt>UseSmallDateTime</tt> flag.</summary>
	/// <remarks>
	///   <para>The attribute is added to the value declaration of a DAC field. The field becomes bound to the database column with the same name.</para>
	///   <para>The attribute defines a field represented by a single input control in the user interface.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="The attribute below binds the field to the database column and sets the minimum and maximum values for a field value." lang="CS">
	/// [PXDBDate(MinValue = "01/01/1900")]
	/// public virtual DateTime? OrderDate { get; set; }</code>
	/// 	<code title="Example2" description="The attribute below binds the field to the database column and sets the input and display masks. A field value will be displayed using the long date pattern. That is, for en-US culture the 6/15/2009 1:45:30 PM value will be converted to Monday, June 15, 2009." groupname="Example" lang="CS">
	/// [PXDBDate(InputMask = "d", DisplayMask = "d")]
	/// public virtual DateTime? StartDate { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDateAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected static readonly DateTime _MIN_VALUE = new DateTime(1900, 1, 1);

		protected string _InputMask = null;
		protected string _DisplayMask = null;
		protected DateTime? _MinValue;
		protected DateTime? _MaxValue;
		protected bool _PreserveTime;
		protected bool _UseSmallDateTime = true;
		private bool _useTimeZone = true;

		/// <summary>Gets or sets the format string that defines how a field value
		/// inputted by a user should be formatted.</summary>
		/// <value>If the property is set to a one-character string, the corresponding <see cref="!:https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings">standard date and time
		/// format string</see> is used. If the property value is longer, it is treated as a <see cref="!:https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings">custom date and time format
		/// string</see>. A particular pattern depends on the culture set by the application.</value>
		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		/// <summary>Gets or sets the format string that defines how a field value is displayed in the input control.</summary>
		/// <value>If the property is set to a one-character string, the corresponding <see cref="!:https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings">standard date and time
		/// format string</see> is used. If the property value is longer, it is treated as a <see cref="!:https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings">custom date and time format
		/// string</see>. A particular pattern depends on the culture set by the application.</value>
		/// <example>
		/// The attribute below binds the field to the database column and sets the input and display
		/// masks. A field value will be displayed using the long date pattern. That is, for en-US
		/// culture the <i>6/15/2009 1:45:30 PM</i> value will be converted to <i>Monday, June 15,
		/// 2009</i>.
		/// <code title="" description="" lang="CS">
		/// [PXDBDate(InputMask = "d", DisplayMask = "d")]
		/// public virtual DateTime? StartDate { get; set; }</code></example>
		public string DisplayMask
		{
			get
			{
				return _DisplayMask;
			}
			set
			{
				_DisplayMask = value;
			}
		}
		/// <summary>Gets or sets the minimum value for the field.</summary>
		public string MinValue
		{
			get
			{
				if (_MinValue != null)
				{
					return _MinValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MinValue = DateTime.Parse(value, CultureInfo.InvariantCulture);
				}
				else
				{
					_MinValue = null;
				}
			}
		}
		/// <summary>Gets or sets the maximum value for the field.</summary>
		public string MaxValue
		{
			get
			{
				if (_MaxValue != null)
				{
					return _MaxValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MaxValue = DateTime.Parse(value, CultureInfo.InvariantCulture);
				}
				else
				{
					_MaxValue = null;
				}
			}
		}

		/// <summary>Gets or sets the value that indicates whether the time part
		/// of a field value is preserved. If <tt>false</tt>, the time part is
		/// removed.</summary>
		public virtual bool PreserveTime
		{
			get
			{
				return _PreserveTime;
			}
			set
			{
				_PreserveTime = value;
				_DisplayMask = value && _DisplayMask == null ? "g" : _DisplayMask;
			}
		}

		/// <summary>
		///   <para>Gets or sets the value that indicates the database column data type.</para>
		/// </summary>
		/// <remarks>
		///   <para>The following table shows the difference in using the property for MS SQL and MySQL.</para>
		/// 	<table>
		/// 		<tbody>
		/// 			<tr>
		/// 				<td>
		/// 					<para align="center">
		/// 						<strong>Value</strong>
		/// 					</para>
		/// 				</td>
		/// 				<td>
		/// 					<para align="center">
		/// 						<strong>MS SQL</strong>
		/// 					</para>
		/// 				</td>
		/// 				<td>
		/// 					<para align="center">
		/// 						<strong>MySQL</strong>
		/// 					</para>
		/// 				</td>
		/// 			</tr>
		/// 			<tr>
		/// 				<td>false</td>
		/// 				<td>datetime</td>
		/// 				<td>datetime(6)</td>
		/// 			</tr>
		/// 			<tr>
		/// 				<td>true</td>
		/// 				<td>datetime2(0)</td>
		/// 				<td>datetime(0)</td>
		/// 			</tr>
		/// 		</tbody>
		/// 	</table>
		///   <para></para>
		/// </remarks>
		/// <value>
		/// 	<para>By default, a value is set to <code>true.</code></para>
		/// </value>
		public bool UseSmallDateTime
		{
			get
			{
				return this._UseSmallDateTime;
			}
			set
			{
				this._UseSmallDateTime = value;
			}
		}

		/// <summary>Gets or sets the value that indicates whether the attribute
		/// should convert the time to UTC, using the local time zone. If
		/// <tt>true</tt>, the time is converted. By default,
		/// <tt>true</tt>.</summary>
		public virtual bool UseTimeZone
		{
			get { return _useTimeZone; }
			set { _useTimeZone = value; }
		}

		internal static void SetUseTimeZone(PXCache cache, string field, bool useTimeZone)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field))
			{
				if (attr is PXDBDateAttribute)
				{
					((PXDBDateAttribute)attr).UseTimeZone = useTimeZone;
					break;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			e.DataType = UseSmallDateTime ? PXDbType.SmallDateTime : PXDbType.DateTime;
			if (e.Value != null)
			{
				if (UseTimeZone && _PreserveTime)
				{
					DateTime newDate = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)e.Value, GetTimeZone());
					e.DataValue = newDate;
				}
				else e.DataValue = (DateTime)e.Value;
			}
			if (e.DataValue != null && UseSmallDateTime)
			{
				if ((e.DataValue as DateTime?) > this._MaxValue)
				{
					e.DataValue = this._MaxValue;
				}
				if ((e.DataValue as DateTime?) < this._MinValue)
				{
					e.DataValue = this._MinValue;
				}
			}
			e.DataLength = 4;
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				DateTime? dt;
				try
				{
					dt = e.Record.GetDateTime(e.Position);
				}
				catch (ArgumentOutOfRangeException)
				{
					dt = null; // workaround for MySQL
				}

				if (dt != null)
				{
					if (_PreserveTime)
					{
						if (UseTimeZone) dt = PXTimeZoneInfo.ConvertTimeFromUtc(dt.Value, GetTimeZone());
					}
					else dt = new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day);
				}
				sender.SetValue(e.Row, _FieldOrdinal, dt);
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					if (!_PreserveTime) val = new DateTime(val.Year, val.Month, val.Day);
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (!_PreserveTime && e.NewValue != null)
			{
				DateTime val = (DateTime)e.NewValue;
				if (val != null) e.NewValue = new DateTime(val.Year, val.Month, val.Day);
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, null, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}
		}
		#endregion
		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (_MinValue == null)
			{
				_MinValue = _MIN_VALUE;
			}
			if (_MaxValue == null)
			{
				_MaxValue = new DateTime(9999, 12, 31);
			}

			AddFieldsForDateParts(sender);
		}
		#endregion

		protected virtual PXTimeZoneInfo GetTimeZone()
		{
			return LocaleInfo.GetTimeZone();
		}

		#region Private Methods
		/// <exclude/>
		protected static IEnumerable<PXDBDateAndTimeAttribute> GetAttribute(PXCache cache, object data, string name)
		{
			return cache.GetAttributes(data, name).OfType<PXDBDateAndTimeAttribute>();
		}
		
		private static IConstant<string>[] dateParts = 
		{
			new DatePart.month(),
			new DatePart.day(),
			new DatePart.hour(),
			new DatePart.quarter()
		};
		static string[] datePartsNames = new[] { "_Month", "_Day", "_Hour", "_Quarter" };

		private void AddFieldsForDateParts(PXCache cache)
		{
			for (int i = 0; i < dateParts.Length; i++)
				AddFieldForDatePart(cache, dateParts[i], datePartsNames[i]);
		}

		private void AddFieldForDatePart(PXCache cache, IConstant<string> datePart, string datePartName)
		{
			string partFieldName = _FieldName + datePartName;
			if (cache.Fields.Contains(partFieldName)) return;
			cache.Fields.Add(partFieldName);

			var attr = new PXDBIntAttribute()
			{
				BqlTable = this.BqlTable,
				FieldName = _FieldName
			};

			cache.FieldSelectingEvents.Add(partFieldName, (sender, e) =>
			{
				string result = string.Empty;
				var value = sender.GetValue(e.Row, _FieldName);
				if (e.Row != null && value != null)
				{
					result = value.ToString();
					e.ReturnValue = result;
				}

				var state = PXStringState.CreateInstance(result, null, true, partFieldName, false, 0, null, null, null, null, null);
				state.Visible = false;
				state.Enabled = false;
				state.Visibility = PXUIVisibility.Invisible;
				e.ReturnState = state;
			});

			cache.FieldUpdatingEvents.Add(partFieldName, (sender, e) =>
			{
				if (e.Row != null)
				{
					var newValue =  sender.GetValue(e.Row, _FieldName);
					if (newValue != null)
						e.NewValue = Convert.ToInt32(newValue);
				}
					
			});

			cache.CommandPreparingEvents.Add(partFieldName, (sender, e) =>
			{
				if (e.IsSelect() && (e.Operation & PXDBOperation.Option) == PXDBOperation.External)
				{
					//call CommandPreparing for get correct fieldName. 
					attr.CommandPreparing(sender, e);
					string fieldName = FieldName;

					bool isNeedConvertTime = UseTimeZone && _PreserveTime;
					e.Expr = SQLExpression.DatePartByTimeZone(datePart, new Column(fieldName, e.Table), isNeedConvertTime ? GetTimeZone() : null);
				}
			});
		}

		#endregion

	}
	#endregion

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDateWithTimezoneAttribute : PXDBDateAttribute
	{
		object _timeZone;
		Type _timeZoneType;
		public PXDBDateWithTimezoneAttribute(Type timeZoneType)
		{
			_timeZoneType = timeZoneType;
			UseTimeZone = true;
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			_timeZone = sender.GetValue(e.Row, _timeZoneType.Name);
			base.CommandPreparing(sender, e);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			_timeZone = sender.GetValue(e.Row, _timeZoneType.Name);
			base.RowSelecting(sender, e);
		}
		protected override PXTimeZoneInfo GetTimeZone()
		{
			return GetTimeZoneInt();
		}

		internal PXTimeZoneInfo GetTimeZoneInt()
		{
			return PXTimeZoneInfo.FindSystemTimeZoneById((string)_timeZone);
		}
	}

	#region PXDBTimestampAttribute
	/// <summary>Maps a DAC field of <tt>byte[]</tt> type to the database
	/// column of <tt>timestamp</tt> type.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</para>
	/// <para>The attribute binds the field to a timestamp column in the
	/// database. The database timestamp is a counter that is incremented for
	/// each insert or update operation performed on a table with a
	/// <tt>timestamp</tt> column. The counter tracks a relative time within a
	/// database (not an actual time that can be associated with a clock). You
	/// can use the <tt>timestamp</tt> column of a data record to easily
	/// determine whether any value in the data record has changed since the
	/// last time it was read.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBTimestamp()]
	/// public virtual byte[] tstamp { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBTimestampAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXRowPersistedSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected bool _RecordComesFirst;
		/// <exclude/>
		public virtual bool RecordComesFirst
		{
			get
			{
				return _RecordComesFirst;
			}
			set
			{
				_RecordComesFirst = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				try {
					e.NewValue = Convert.FromBase64String((string)e.NewValue);
				}
				catch {
				}
			}
		}

		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnValue is byte[]) {
				e.ReturnValue = Convert.ToBase64String((byte[])e.ReturnValue);
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, false, _FieldName, false, null, null, null, null, null, null);
				((PXFieldState)e.ReturnState).Visible = false;
				((PXFieldState)e.ReturnState).Enabled = false;
				((PXFieldState)e.ReturnState).Visibility = PXUIVisibility.Invisible;
			}
		}

		// returned results follow the same convention as String.Compare(strA, strB)
		public static int compareTimestamps(byte[] first, byte[] second)
		{
			if (first == null && second == null) return 0;
			if (first == null)  return -1;
			if (second == null) return 1;

			for (int i = 0; i < first.Length && i < second.Length; i++)
			{
				if (first[i] == second[i])
					continue;
				return first[i] > second[i] ? 1 : -1;
			}
			return 0;
		}

		public static byte[] getLatestTimestamp(byte[] first, byte[] second)
		{
			if (first == null && second == null) return null;
			if (first == null) return second;
			if (second == null) return first;

			for (int i = 0; i < first.Length && i < second.Length; i++)
			{
				if (first[i] == second[i])
					continue;
				return first[i] > second[i] ? first : second;
			}
			return first;
		}

		/// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert && (e.Operation & PXDBOperation.Option) != PXDBOperation.Second) {
				object value = null;
				bool isSelect = ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select);
				bool recordComesFirst = this._RecordComesFirst || PXTimeStampScope.GetRecordComesFirst(sender.GetItemType());

				if (!isSelect) {
					byte[] gstamp = sender.Graph.TimeStamp;
					if (sender.Graph._primaryRecordTimeStamp != null && sender.Graph.PrimaryItemType != null && sender.Graph.Caches[sender.Graph.PrimaryItemType] == sender)
					{
						gstamp = sender.Graph._primaryRecordTimeStamp;
					}
					value = gstamp;
					byte[] tstamp = null;
					object[] persisted = PXTimeStampScope.GetPersisted(sender, e.Row);
					if (persisted != null && persisted.Length > 0) {
						tstamp = persisted[0] as byte[];
					}
					if (recordComesFirst)
					{
						value = tstamp ?? e.Value ?? gstamp;
					}
					else if (tstamp != null)
					{
						value = compareTimestamps(tstamp, gstamp) > 0 ? tstamp : gstamp;
					}
					else if (gstamp != null && sender.Graph.PrimaryItemType != null && e.Row != null && sender.Graph.PrimaryItemType.IsAssignableFrom(e.Row.GetType())) {
						TimeSpan ts;
						Exception msg;
						if (!sender.Graph.IsImport && PXLongOperation.GetStatus(sender.Graph.UID, out ts, out msg) == PXLongRunStatus.Completed) {
							tstamp = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
							if (tstamp != null) {
								int cmp = compareTimestamps(tstamp, gstamp);
								if (cmp > 0)
									value = sender.Graph.TimeStamp = tstamp;
							}
						}
					}
				}
				if (value == null)
				{
					string s = e.Value as string;
					if (s != null) {
						try {
							value = Convert.FromBase64String(s);
						}
						catch {}
					}
					else {
						value = e.Value;
					}
				}
				if (value != null || isSelect) {
					PrepareFieldName(_DatabaseFieldName, e);
					e.DataType = PXDbType.Timestamp;
					e.DataValue = value;
					e.DataLength = 8;
					e.IsRestriction = e.IsRestriction || !isSelect;
				}
				else {
					throw new PXTimeStampEmptyException(_FieldName);
				}
			}
		}
		/// <exclude/>
		public class PXTimeStampEmptyException : PXCommandPreparingException
		{
			public PXTimeStampEmptyException(string field)
				: base(field, null, ErrorMessages.FieldIsEmpty, field)
			{
			}
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				byte[] tsFromDb = e.Record.GetTimeStamp(e.Position);
				//if (e.Row.GetType().Name == "SOOrder" && compareTimestamps(sender.GetValue(e.Row, _FieldOrdinal) as byte[], tsFromDb) != 0)
				//	System.Diagnostics.Debug.WriteLine("SOOrder.Timestamp := " + String.Join(" ", tsFromDb) + " @ " + Thread.CurrentThread.ManagedThreadId);
				sender.SetValue(e.Row, _FieldOrdinal, tsFromDb);
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			byte[] gstamp;
			if (e.TranStatus == PXTranStatus.Completed
				&& (e.Operation & PXDBOperation.Command) != PXDBOperation.Delete
				&& (gstamp = sender.Graph.TimeStamp) != null) {
				byte[] tstamp = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
				if (tstamp == null) {
					sender.SetValue(e.Row, _FieldOrdinal, gstamp);
					PXTimeStampScope.PutPersisted(sender, e.Row, gstamp);
				}
				else {
					bool isSet = false;
					for (int i = 0; i < tstamp.Length && i < gstamp.Length; i++) {
						if (gstamp[i] > tstamp[i]) {
							sender.SetValue(e.Row, _FieldOrdinal, gstamp);
							PXTimeStampScope.PutPersisted(sender, e.Row, gstamp);
							isSet = true;
							break;
						}
					}
					if (!isSet) {
						PXTimeStampScope.PutPersisted(sender, e.Row, tstamp);
					}
				}
			}
		}
		#endregion

		/// <exclude/>
		public static string ToString(byte[] tstamp)
		{
			return PXSqlDatabaseProvider.TimestampToString(tstamp);
		}

	}
	#endregion

	#region PXDBTimeAttribute
	/// <summary>Maps a DAC field of <tt>DateTime?</tt> type to the database
	/// column of <tt>smalldatetime</tt> type. The field value holds only time
	/// without date.</summary>
	/// <remarks>
	///   <para>The attribute is added to the value declaration of a DAC field. The field becomes bound to the database column with the same name. </para>
	///   <para>The field values keep only time without date. On the user interface, the field is represented by a control allowing a user to enter only a time value.</para>
	///   <para>The attribute inherits properties of the PXDBDate attribute.</para>
	/// </remarks>
	/// <example>
	///   <code title="Example" description="The code below binds the &lt;tt&gt;SunStartTime&lt;/tt&gt; DAC field to the database column with the same name and sets the default value for the field. Notice the setting of the &lt;tt&gt;DisplayMask&lt;/tt&gt; property inherited from the %PXDBDate:PX.Data.PXDBDateAttribute% attribute." lang="CS">
	/// [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
	/// [PXDefault(TypeCode.DateTime, "01/01/2008 09:00:00")]
	/// public virtual DateTime? SunStartTime { ... }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBTimeAttribute : PXDBDateAttribute
	{
		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXDBTimeAttribute()
		{
			base.PreserveTime = true;
		}

		/// <summary>Gets the value that indicates whether the time part of a
		/// field value is preserved. Since the constructor sets this value to
		/// <tt>true</tt>, this property always returns <tt>true</tt>.</summary>
		public override bool PreserveTime
		{
			get
			{
				return base.PreserveTime;
			}
			set
			{
			}
		}

		/// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			if (e.DataValue != null) e.DataValue = _MIN_VALUE.AddTicks(((DateTime)e.DataValue).TimeOfDay.Ticks);
		}

		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			object val = sender.GetValue(e.Row, _FieldOrdinal);
			if (val != null) sender.SetValue(e.Row, _FieldOrdinal, _MIN_VALUE.AddTicks(((DateTime)val).TimeOfDay.Ticks));
		}

		/// <exclude/>
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			base.FieldUpdating(sender, e);

			if (e.NewValue != null) e.NewValue = _MIN_VALUE.AddTicks(((DateTime)e.NewValue).TimeOfDay.Ticks);
		}
	}

	#endregion

	#region PXDBIdentityAttribute
	/// <summary>Maps an auto-incremented integer DAC field of <tt>int?</tt>
	/// type to the <tt>int</tt> database column.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</para>
	/// <para>The field value is auto-incremented by the attribute.</para>
	/// <para>A field with this attribute typically is a key field. To declare a key field,
	/// set the <tt>IsKey</tt> parameter to <tt>true</tt>.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBIdentity(IsKey = true)]
	/// [PXUIField(DisplayName = "Contact ID", Visible = false)]
	/// public virtual int? ContactID { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBIdentityAttribute : PXDBFieldAttribute, IPXFieldDefaultingSubscriber, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXRowPersistedSubscriber, IPXFieldVerifyingSubscriber, IPXIdentityColumn
	{
		#region State
		protected int? _KeyToAbort;
		/// <exclude/>
		protected class LastDefault
		{
			public int Value;
			public List<object> Rows = new List<object>();
		}
		protected LastDefault _MaximumDefaultValue;
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && _MaximumDefaultValue.Value < 0) {
				foreach (object row in _MaximumDefaultValue.Rows) {
					if (sender.Locate(row) != null) {
						_MaximumDefaultValue.Rows.Clear();
						_MaximumDefaultValue.Value++;
						break;
					}
				}
				e.NewValue = _MaximumDefaultValue.Value;
				_MaximumDefaultValue.Rows.Add(e.Row);
			}
			else {
				int newId = int.MinValue;
				foreach (object data in sender.Cached) {
					object val = sender.GetValue(data, _FieldOrdinal);
					if (val != null) {
						if ((int)val > 0) {
							foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(data, _FieldName)) {
								if (attr is PXDBIdentityAttribute) {
									if (((PXDBIdentityAttribute)attr)._KeyToAbort < 0 && ((PXDBIdentityAttribute)attr)._KeyToAbort > newId) {
										newId = (int)((PXDBIdentityAttribute)attr)._KeyToAbort;
									}
									break;
								}
							}
						}
						else if ((int)val > newId) {
							newId = (int)val;
						}
					}
				}
				newId++;
				e.NewValue = newId;
				if (e.Row != null) {
					_MaximumDefaultValue.Value = newId;
					_MaximumDefaultValue.Rows.Add(e.Row);
				}
			}
			e.Cancel = true;
		}
		/// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert
				&& ((e.Operation & PXDBOperation.Option) != PXDBOperation.Second || _IsKey || e.IsRestriction)
				&& ((e.Operation & PXDBOperation.Command) != PXDBOperation.Update || e.Value is int && ((int)e.Value) >= 0
				|| (e.Operation & PXDBOperation.Option) == PXDBOperation.Second)) {
				
				PrepareFieldName(_DatabaseFieldName, e);
				e.DataType = PXDbType.Int;
				e.DataValue = e.Value;
				e.DataLength = 4;
				e.IsRestriction = true;
			}
		}
		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt32(e.Position));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				int val;
				if (int.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, null, null, null, null, typeof(int), null);
			}
		}

		/// <exclude/>
		public virtual object GetLastInsertedIdentity(object valueFromCache) {
			return getLastInsertedIdentity();
		}

		private int getLastInsertedIdentity() {
			return Convert.ToInt32(PXDatabase.SelectIdentity(_BqlTable, _FieldName));
		}

		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) {
				if (e.TranStatus == PXTranStatus.Open) {
					if (_KeyToAbort == null)
						_KeyToAbort = (int?)sender.GetValue(e.Row, _FieldOrdinal);
					if (_KeyToAbort < 0) {
						assignIdentityValue(sender, e);
					}
					else {
						_KeyToAbort = null;
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null) {
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
					_KeyToAbort = null;
				}
			}
			if (e.TranStatus == PXTranStatus.Completed)
			{
				ClearMaximumDefaultValue();
			}
		}
		public virtual void ClearMaximumDefaultValue()
		{
				_MaximumDefaultValue.Rows.Clear();
				_MaximumDefaultValue.Value = 0;
			}
		protected virtual void assignIdentityValue(PXCache sender, PXRowPersistedEventArgs e)
		{
			int? id = getLastInsertedIdentity();
			if (id.Value == 0m)
			{
				PXDataField[] pars = new PXDataField[sender.Keys.Count + 1];
				pars[0] = new PXDataField(_DatabaseFieldName);
				for (int i = 0; i < sender.Keys.Count; i++)
				{
					string name = sender.Keys[i];
					PXCommandPreparingEventArgs.FieldDescription description = null;
					sender.RaiseCommandPreparing(name, e.Row, sender.GetValue(e.Row, name), PXDBOperation.Select, _BqlTable,
						out description);
					if (description != null && description.Expr != null && description.IsRestriction)
					{
						pars[i + 1] = new PXDataFieldValue(description.Expr, description.DataType, description.DataLength, description.DataValue);
					}
				}
				using (PXDataRecord record = PXDatabase.SelectSingle(_BqlTable, pars))
				{
					if (record != null)
					{
						id = record.GetInt32(0);
					}
				}
			}
			sender.SetValue(e.Row, _FieldOrdinal, id);
			PXTransactionScope.SendIdentity(_DatabaseFieldName, BqlCommand.GetTableName(sender.BqlTable), id.GetValueOrDefault());
		}

		/// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.ExternalCall) {
				int? oldValue = (int?)sender.GetValue(e.Row, _FieldOrdinal);

				if (oldValue != null) {
					e.NewValue = oldValue;
				}
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			_MaximumDefaultValue = new LastDefault();
			sender._Identity = _FieldName;
			sender._RowId = _FieldName;
			sender.Graph.OnClear += (graph_, option) => ClearMaximumDefaultValue();
			base.CacheAttached(sender);
		}
		#endregion
	}
	#endregion

	#region PXDBLongIdentityAttribute
	/// <summary>Maps an 8-byte auto-incremented integer DAC field of
	/// <tt>int64?</tt> type to the <tt>bigint</tt> database column.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same name. The
	/// field value is auto-incremented by the database.</para>
	/// <para>A field of this type is typically declared a key field. To do
	/// this, set the <tt>IsKey</tt> parameter to <tt>true</tt>.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBLongIdentity(IsKey = true)]
	/// public virtual Int64? RecordID { ... }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBLongIdentityAttribute : PXDBFieldAttribute, IPXFieldDefaultingSubscriber, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXRowPersistedSubscriber, IPXFieldVerifyingSubscriber, IPXIdentityColumn
	{
		#region State
		protected long? _KeyToAbort;
		/// <exclude/>
		protected class LastDefault
		{
			public long Value;
			public List<object> Rows = new List<object>();
		}
		protected LastDefault _MaximumDefaultValue;
		#endregion

		#region Implementation
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && _MaximumDefaultValue.Value < 0) {
				foreach (object row in _MaximumDefaultValue.Rows) {
					if (sender.Locate(row) != null) {
						_MaximumDefaultValue.Rows.Clear();
						_MaximumDefaultValue.Value++;
						break;
					}
				}
				e.NewValue = _MaximumDefaultValue.Value;
				_MaximumDefaultValue.Rows.Add(e.Row);
			}
			else {
				long newId = int.MinValue;
				foreach (object data in sender.Cached) {
					object val = sender.GetValue(data, _FieldOrdinal);
					if (val != null) {
						if ((long)val > 0) {
							foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(data, _FieldName)) {
								if (attr is PXDBLongIdentityAttribute) {
									if (((PXDBLongIdentityAttribute)attr)._KeyToAbort < 0 && ((PXDBLongIdentityAttribute)attr)._KeyToAbort > newId) {
										newId = (long)((PXDBLongIdentityAttribute)attr)._KeyToAbort;
									}
									break;
								}
							}
						}
						else if ((long)val > newId) {
							newId = (long)val;
						}
					}
				}
				newId++;
				e.NewValue = newId;
				if (e.Row != null) {
					_MaximumDefaultValue.Value = newId;
					_MaximumDefaultValue.Rows.Add(e.Row);
				}
			}
		}
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert && ((e.Operation & PXDBOperation.Option) != PXDBOperation.Second || _IsKey || e.IsRestriction)) {
				PrepareFieldName(_DatabaseFieldName, e);
				e.DataType = PXDbType.BigInt;
				e.DataValue = e.Value;
				e.DataLength = 8;
				e.IsRestriction = true;
			}
			e.Cancel = true;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt64(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string) {
				long val;
				if (long.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXLongState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, null, null, typeof(long));
			}
		}

		public virtual object GetLastInsertedIdentity(object valueFromCache) {
			return getLastInsertedIdentity(); // native method provides long, native som
		}

		private long getLastInsertedIdentity() {
			return Convert.ToInt64(PXDatabase.SelectIdentity(_BqlTable, _FieldName));
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) {
				if (e.TranStatus == PXTranStatus.Open) {
					_KeyToAbort = (long?)sender.GetValue(e.Row, _FieldOrdinal);
					if (_KeyToAbort < 0) {
						long? id = getLastInsertedIdentity();
						if ((id ?? 0m) == 0m) {
							PXDataField[] pars = new PXDataField[sender.Keys.Count + 1];
							pars[0] = new PXDataField(_DatabaseFieldName);
							for (int i = 0; i < sender.Keys.Count; i++) {
								string name = sender.Keys[i];
								PXCommandPreparingEventArgs.FieldDescription description = null;
								sender.RaiseCommandPreparing(name, e.Row, sender.GetValue(e.Row, name), PXDBOperation.Select, _BqlTable, out description);
								if (description != null && description.Expr != null && description.IsRestriction) {
									pars[i + 1] = new PXDataFieldValue(description.Expr, description.DataType, description.DataLength, description.DataValue);
								}
							}
							using (PXDataRecord record = PXDatabase.SelectSingle(_BqlTable, pars)) {
								if (record != null) {
									id = record.GetInt64(0);
								}
							}
						}
						sender.SetValue(e.Row, _FieldOrdinal, id);
					}
					else {
						_KeyToAbort = null;
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null) {
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
					_KeyToAbort = null;
				}
			}
			if (e.TranStatus == PXTranStatus.Completed)
			{
				ClearMaximumDefaultValue();
			}
		}

		public virtual void ClearMaximumDefaultValue()
		{
				_MaximumDefaultValue.Rows.Clear();
				_MaximumDefaultValue.Value = 0;
			}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.ExternalCall) {
				long? oldValue = (long?)sender.GetValue(e.Row, _FieldOrdinal);

				if (oldValue != null) {
					e.NewValue = oldValue;
				}
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			_MaximumDefaultValue = new LastDefault();
			sender._Identity = _FieldName;
			sender._RowId = _FieldName;
			sender.Graph.OnClear += (graph_, option) => ClearMaximumDefaultValue();
			base.CacheAttached(sender);
		}
		#endregion
	}
	#endregion

	#region PXDBForeignIdentityAttribute

	/// <exclude/>
	public class PXDBForeignIdentityAttribute : PXDBIdentityAttribute, IPXRowPersistingSubscriber
	{
		Type _ForeignIdentity = null;

		public PXDBForeignIdentityAttribute(Type ForeignIdentity)
		{
			_ForeignIdentity = ForeignIdentity;
		}

		public override void CacheAttached(PXCache sender)
		{
			sender._RowId = _FieldName;
			base.CacheAttached(sender);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				try
				{
					PXDatabase.Insert(_ForeignIdentity, PXDataFieldAssign.OperationSwitchAllowed);
				}
				catch (PXDbOperationSwitchRequiredException)
				{
					PXDatabase.Update(_ForeignIdentity);
				}
				_KeyToAbort = (int?)sender.GetValue(e.Row, _FieldOrdinal);
				decimal? identity;
				sender.SetValue(e.Row, _FieldOrdinal, Convert.ToInt32(identity = PXDatabase.SelectIdentity()));
				PXTransactionScope.SetIdentityAudit(_BqlTable.Name, _FieldName, identity);
			}
		}

		public override object GetLastInsertedIdentity(object valueFromCache)
		{
			return valueFromCache;
		}

		protected override void assignIdentityValue(PXCache sender, PXRowPersistedEventArgs e)
		{
			// we have already assigned a value in RowPersising, no need to do it again (this method is called from RowPersisted)
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			PrepareFieldName(_DatabaseFieldName, e);
			e.DataType = PXDbType.Int;
			e.DataValue = e.Value;
			e.DataLength = 4;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
	}

	#endregion

	#region PXDBCalcedAttribute
	/// <summary>Defines the SQL expression that calculates an unbound field
	/// from the fields of the same DAC whose values are taken from the
	/// database.</summary>
	/// <remarks>
	/// 	<para>You should place the attribute on the field that is not bound to
	/// any particular database column.</para>
	/// 	<para>The attribute will translate the provided BQL query into the SQL
	/// code and insert it into the select statement that retrieves data
	/// records of this DAC. In the BQL query, you can reference any bound
	/// field of the same DAC or an unbound field marked with <see cref="PXDBScalarAttribute">PXDBScalar</see>. You can also use BQL
	/// constants, arithmetic operations, equivalents of SQL function (such as
	/// <tt>SUBSTRING</tt> and <tt>REPLACE</tt>), and the <tt>Switch</tt>
	/// expression.</para>
	/// 	<para>If, in contrast, you need to calculate the field on the server
	/// side at run time, use the <see cref="PXFormulaAttribute">PXFormula</see> attribute.</para>
	/// 	<para>Note that you should also annotate the field with an attribute
	/// that indicates an unbound
	/// field of a particular data type. Otherwise, the field may be
	/// displayed incorrectly in the user interface.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="The attribute below defines the expression to calculate the field of decimal type." lang="CS">
	/// [PXDBCalced(typeof(Sub&lt;POLine.curyExtCost, POLine.curyOpenAmt&gt;),
	///             typeof(decimal))]
	/// public virtual decimal? CuryClosedAmt { get; set; }</code>
	/// 	<code title="Example2" description="See the following example with the Switch expression." groupname="Example" lang="CS">
	/// [PXDBCalced(
	///     typeof(Switch&lt;Case&lt;Where&lt;INUnit.unitMultDiv, Equal&lt;MultDiv.divide&gt;&gt;,
	///         Mult&lt;INSiteStatus.qtyOnHand, INUnit.unitRate&gt;&gt;,
	///         Div&lt;INSiteStatus.qtyOnHand, INUnit.unitRate&gt;&gt;),
	///     typeof(decimal))]
	/// public virtual decimal? QtyOnHandExt { get; set; }</code>
	/// 	<code title="Example3" description="See the following example with the more complex BQL expression." groupname="Example2" lang="CS">
	/// [Serializable]
	/// public class Product : PX.Data.IBqlTable
	/// {
	///     ...
	///     [PXDecimal(2)]
	///     [PXDBCalced(typeof(
	///         Minus&lt;Sub&lt;Sub&lt;IsNull&lt;Product.availQty, decimal_0&gt;, 
	///                       IsNull&lt;Product.bookedQty, decimal_0&gt;&gt;, 
	///               Product.minAvailQty&gt;&gt;),
	///         typeof(decimal))]
	///     public virtual decimal? Discrepancy { get; set; }
	///     ...
	/// }</code>
	/// 	<code title="Example4" description="This example also shows the enclosing declaration of the Product DAC. You can retrieve the records from the Product table by executing the following code in some graph." groupname="Example3" lang="CS">
	/// PXSelect&lt;Product&gt;.Select(this);</code>
	/// 	<code title="Example5" description="This BQL statement will be translated into the following SQL query." groupname="Example4" lang="SQL">
	/// SELECT [other fields],
	///         -((ISNULL(Product.AvailQty, .0) - ISNULL(Product.BookedQty, .0))
	///           - Product.MinAvailQty) as Product.Discrepancy
	/// FROM Product</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class)]
	public class PXDBCalcedAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _OperandType;
		protected IBqlCreator _Operand;
		protected Type _Type;
		protected int _DatabaseOrdinal = -1;
		protected bool _Persistent = false;
		protected bool _BypassGroupby;
		public int CastToPrecision;
		public int CastToScale;

		/// <summary>Gets or sets the value that indicates whether the field the
		/// attribute is attached to is updated after a database commit
		/// operation.</summary>
		public virtual bool Persistent
		{
			get
			{
				return _Persistent;
			}
			set
			{
				this._Persistent = value;
			}
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Initializes a new instance that uses the provided BQL expression to
		/// calculate the value of the field.
		/// </summary>
		/// <param name="operand">The BQL query that is translated into SQL code
		/// that retrieves the value of the field. Specify any combination of BQL
		/// functions, constants, and the bound fields of the same DAC.</param>
		/// <param name="type">The data type of the field.</param>
		/// <example>
		/// The code below shows the full definition of a DAC field.
		/// <code>
		/// public abstract class startDate : PX.Data.BQL.BqlString.Field<startDate>
		/// {
		/// }
		/// protected String _StartDate;
		/// [PXDBCalced(typeof(Add&lt;AP1099Year.finYear, string0101&gt;), typeof(string))]
		/// public virtual String StartDate { get; set; }
		/// </code>
		/// </example>
		public PXDBCalcedAttribute(Type operand, Type type)
		{
			_OperandType = operand;
			_Type = type;
			foreach (Type t in BqlCommand.Decompose(operand)) {
				if (typeof(IBqlSearch).IsAssignableFrom(t)) {
					_BypassGroupby = true;
				}
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (_Persistent) {
				sender.Graph.RowPersisted.AddHandler(sender.GetItemType(), RowPersisted);
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (!sender.BypassCalced || sender.BqlSelect != null))
			{
				SQLTree.SQLExpression exp = null;

				List<Type> tables = new List<Type>();
				bool replace = false;
				if (e.Table != null) {
					tables.Add(e.Table);
				}
				else {
					tables.Add(_BqlTable);
					Type successor;
					if (sender.GetExtensionTables() != null && (successor = sender.GetItemType()) != _BqlTable) {
						while (successor != typeof(object))
						{
							if (successor.IsDefined(typeof(PXTableAttribute), false))
							{
								break;
							}
							if (successor == _BqlTable)
							{
								replace = true;
								break;
							}
							successor = successor.BaseType;
						}
					}
				}
				if (!typeof(IBqlCreator).IsAssignableFrom(_OperandType)) {
					if (String.Compare(_OperandType.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0) {
						exp=BqlCommand.GetSingleExpression(_OperandType, sender.Graph, tables, null, BqlCommand.FieldPlace.Select);
					}
					else {
						exp = new Column(_OperandType.Name, new SimpleTable(tables[0]));
					}
				}
				else {
					if (_Operand == null) {
						_Operand = Activator.CreateInstance(_OperandType) as IBqlCreator;
					}
					if (_Operand == null) {
						throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
					}
					Dictionary<PXCache, BqlCommand> selects = new Dictionary<PXCache, BqlCommand>();
					selects[sender] = sender.BqlSelect;

					if (replace)
					{
						Type cacheType = sender.GetItemType();
						while ((cacheType = cacheType.BaseType) != typeof(object)
							&& typeof(IBqlTable).IsAssignableFrom(cacheType))
						{
							PXCache cache = sender.Graph.Caches[cacheType];
							if (!selects.ContainsKey(cache))
							{
								selects[cache] = cache.BqlSelect;
							}
						}
					}
					try {
						foreach (PXCache cache in selects.Keys)
						{
							cache.BqlSelect = null;
						}
						_Operand.AppendExpression(ref exp, sender.Graph, new BqlCommandInfo(false) {Tables = tables}, null);
					}
					finally {
						foreach (KeyValuePair<PXCache, BqlCommand> pair in selects)
						{
							pair.Key.BqlSelect = pair.Value;
						}
					}
				}
				e.BqlTable = _BqlTable;
				if((e.Operation & PXDBOperation.Place) == PXDBOperation.NestedSelectInReport )
				{
					e.Expr = null;
					return;
				}
				bool useFieldText = (e.Operation & PXDBOperation.Option) != PXDBOperation.GroupBy || _Type != typeof(Boolean) && _Type != typeof(Guid) && !_BypassGroupby;
				if( useFieldText )
				{
					if (exp == null)
					{
						e.Expr = null;
					}
					else
					{
						if(_Type == typeof(Decimal) && CastToPrecision > 0) {
							e.Expr = exp?.Embrace().CastAsDecimal(CastToPrecision, CastToScale);
						}
						else {
							if (exp is Column) exp = SQLExpression.Empty().SetLeft(exp);
							e.Expr = exp;	
						}
						e.Expr?.Embrace();
						
					}
				}
				else {
					e.Expr = SQLExpression.Null();
				}
			}
		}
		/// <exclude/>
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null) {
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetValue(e.Position, _Type));
			}
			e.Position++;
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {

				PXFieldState state = e.ReturnState as PXFieldState;
				if (state == null || state.DataType == typeof(object) || state.DataType == _Type)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, _Type, false, true, null, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
				}
			}
		}

		/// <summary>Calculates the field value of the data record using the
		/// formula from the attribute instance that marks the
		/// specified field. </summary>
		/// <param name="Field">The field to calculate.</param>
		/// <param name="sender">The cache object to search for the attributes of
		/// <tt>PXDBCalced</tt> type.</param>
		/// <param name="row">The data record.</param>
		public static void Calculate<Field>(PXCache sender, object row)
		where Field : IBqlField
		{
			foreach (PXDBCalcedAttribute attr in sender.GetAttributesReadonly<Field>(row).OfType<PXDBCalcedAttribute>())
			{
				attr.Calculate(sender, row);
			}
		}
		/// <exclude/>
		public virtual void Calculate(PXCache sender, object row)
		{
			bool? result = null;
			object value = null;

			if (typeof(IBqlField).IsAssignableFrom(_OperandType))
			{
				if (sender.GetItemType() == BqlCommand.GetItemType(_OperandType) && BqlCommand.GetItemType(_OperandType).IsAssignableFrom(sender.GetItemType()))
				{
					value = sender.GetValue(row, _OperandType.Name);
				}
			}
			else {
				if (_Operand == null)
				{
					_Operand = Activator.CreateInstance(_OperandType) as IBqlCreator;
				}
				if (_Operand == null)
				{
					throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
				}
				_Operand.Verify(sender, row, null, ref result, ref value);
			}
			sender.SetValue(row, _FieldName, value);
		}
		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
				Calculate(sender, e.Row);
		}
		#endregion
	}
	#endregion

	#region PXDBCreatedByIDAttribute
	/// <summary>Maps a DAC field to the database column and automatically
	/// sets the field value to the ID of the user who created the data
	/// record.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field data type should be <tt>Guid?</tt>.</para>
	/// <para>The attribute aggregates the <see cref="PXDBGuidAttribute">PXDBGuid</see> and
	/// <tt>PXDisplaySelector</tt> (derives from
	/// <see cref="PXSelectorAttribute">PXSelector</see>).</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBCreatedByID()]
	/// public virtual Guid? CreatedByID { get; set; }
	/// </code>
	/// </example>
	[Serializable]
	[PXDBGuid]
	[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedByID, Enabled = false, Visible = true, IsReadOnly = true)]
	public class PXDBCreatedByIDAttribute : PXAggregateAttribute, IPXRowInsertingSubscriber, IPXFieldVerifyingSubscriber
	{
		internal class AuditUserSelectorAttribute : PXDisplaySelectorAttribute
		{
			public AuditUserSelectorAttribute(Type type) : base(type) {}
			public AuditUserSelectorAttribute(Type type, params Type[] fieldList) : base(type, fieldList) {}

			protected override void EmitColumnForDescriptionField(PXCache sender)
			{
				base.EmitColumnForDescriptionField(sender);
				EmitDescriptionFieldAlias(sender, $"{_FieldName}_{_Type.Name}_Username"); // Backward compatibility support. Do not use this alias in application.
			}

			public override void DescriptionFieldCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
			{
				base.DescriptionFieldCommandPreparing(sender, e);
				Query q = (e.Expr as SubQuery)?.Query();
				if (q!=null && q.GetSelection().Count>0 && (q.GetSelection()[0] as Column)?.Name.Equals(nameof(Creator.displayName), StringComparison.OrdinalIgnoreCase)==true) {
					SimpleTable tbl = new SimpleTable(_Type.Name + "Ext");
					SQLSwitch extcase = new SQLSwitch().Case(
						new Column("FirstName", tbl).IsNull(),
						new SQLSwitch().Case(
							new Column("LastName", tbl).IsNull(),
							new Column("UserName", tbl)
						).Default(new Column("LastName", tbl))
					).Default(
						new SQLSwitch().Case(
							new Column("LastName", tbl).IsNull(),
							new Column("FirstName", tbl)
						).Default(
							new Column("FirstName", tbl)
								.Concat(new SQLConst(" "))
								.Concat(new Column("LastName", tbl))
						)
					);
					q.ClearSelection();
					q.Field(extcase);
					e.Expr = new SubQuery(q);
				}

			}
		}

		/// <exclude/>
		[PXBreakInheritance]
		[Serializable]
		public sealed class Creator : Users
		{
			/// <exclude/>
			public new abstract class pKID : PX.Data.BQL.BqlGuid.Field<pKID> { }

			/// <exclude/>
			public new abstract class username : PX.Data.BQL.BqlString.Field<username> { }

			[PXDBString(64, IsKey = true, IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedByID, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			public override string Username { get; set; }

			/// <exclude/>
			public new abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

			[PXString]
			[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedByID, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			[PXFormula(typeof(IsNull<IsNull<SmartJoin<Space, firstName, lastName>, username>, Empty>))]
			public override string DisplayName { get; set; }
		}

		/// <summary>Returns <tt>null</tt> on get. Sets the BQL field representing
		/// the field in BQL queries.</summary>
		public Type BqlField
		{
			get
			{
				return null;
			}
			set
			{
				if (_DBGuidAttrIndex < 0) return;

				((PXDBGuidAttribute)_Attributes[_DBGuidAttrIndex]).BqlField = value;
				BqlTable = _Attributes[_DBGuidAttrIndex].BqlTable;
			}
		}

		public string DisplayName
		{
			get
			{
				return (_UIFldAttrIndex == -1) ? null : ((PXUIFieldAttribute)_Attributes[_UIFldAttrIndex]).DisplayName;
			}
			set
			{
				if (_UIFldAttrIndex != -1)
					((PXUIFieldAttribute)_Attributes[_UIFldAttrIndex]).DisplayName = value;
			}
		}

		public PXUIVisibility Visibility
		{
			get
			{
				return (_UIFldAttrIndex == -1) ? PXUIVisibility.Undefined : ((PXUIFieldAttribute)_Attributes[_UIFldAttrIndex]).Visibility;
			}
			set
			{
				if (_UIFldAttrIndex != -1)
					((PXUIFieldAttribute)_Attributes[_UIFldAttrIndex]).Visibility = value;
			}
		}

		public bool Visible
		{
			get
			{
				return (_UIFldAttrIndex == -1) || ((PXUIFieldAttribute)_Attributes[_UIFldAttrIndex]).Visible;
			}
			set
			{
				if (_UIFldAttrIndex != -1)
					((PXUIFieldAttribute)_Attributes[_UIFldAttrIndex]).Visible = value;
			}
		}

		protected Guid GetUserID(PXCache sender)
		{
			return PXAccess.GetTrueUserID();
		}

		protected int _DBGuidAttrIndex = -1;
		protected int _DisplSelAttrIndex = -1;
		protected int _UIFldAttrIndex = -1;

		/// <summary>
		/// Initializes a new unparameterized instance of the
		/// <tt>PXDBCreatedByID</tt> attribute.
		/// </summary>
		public PXDBCreatedByIDAttribute()
			: this(typeof(Creator.pKID), typeof(Creator.username), typeof(Creator.displayName),
				typeof(Creator.pKID), typeof(Creator.username))
		{
		}

		internal PXDBCreatedByIDAttribute(Type search, Type substituteKey, Type descriptionField, params Type[] fields)
		{
			_Attributes.Add(new AuditUserSelectorAttribute(search, fields) { DescriptionField = descriptionField, SubstituteKey = substituteKey, CacheGlobal = true });
			_DisplSelAttrIndex = _Attributes.Count - 1;

			_DBGuidAttrIndex = _Attributes.FindIndex(attr => attr is PXDBGuidAttribute);
			_UIFldAttrIndex = _Attributes.FindIndex(attr => attr is PXUIFieldAttribute);
		}

		/// <summary>Gets or sets the value that indicates whether a field update
		/// is allowed after the field value is set for the first time.</summary>
		public bool DontOverrideValue { get; set; }

		void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (!DontOverrideValue || sender.GetValue(e.Row, _FieldOrdinal) == null) 
			{
				sender.SetValue(e.Row, _FieldOrdinal, GetUserID(sender));
			}
		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			try
			{
				if (_DisplSelAttrIndex < 0) return;
				((IPXFieldVerifyingSubscriber)_Attributes[_DisplSelAttrIndex]).FieldVerifying(sender, e);
			}
			catch (PXSetPropertyException) 
			{
				e.NewValue = GetUserID(sender);
			}
		}

		/// <exclude/>
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) && _DisplSelAttrIndex >= 0) 
			{
				subscribers.Remove(_Attributes[_DisplSelAttrIndex] as ISubscriber);
			}
		}
	}
	#endregion

	#region PXDBLastModifiedByIDAttribute
	/// <summary>Maps a DAC field to the database column and automatically
	/// sets the field value to the ID of the user who was the last to modify
	/// the data record.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field data type should be <tt>Guid?</tt>.</remarks>
	/// <example>
	/// <code>
	/// [PXDBLastModifiedByID()]
	/// [PXUIField(DisplayName = "Last Modified By")]
	/// public virtual Guid? LastModifiedByID { get; set; }
	/// </code>
	/// </example>
	[Serializable]
	public class PXDBLastModifiedByIDAttribute : PXDBCreatedByIDAttribute, IPXRowUpdatingSubscriber, IPXRowPersistingSubscriber
	{
		/// <summary>Is used internally to represent the user who modified the data record.</summary>
		[PXBreakInheritance]
		[Serializable]
		public sealed class Modifier : Users
		{
			/// <exclude/>
			public new abstract class pKID : PX.Data.BQL.BqlGuid.Field<pKID> { }

			/// <exclude/>
			public new abstract class username : PX.Data.BQL.BqlString.Field<username> { }

			[PXDBString(64, IsKey = true, IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = DisplayFieldNames.LastModifiedByID, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			public override string Username { get; set; }

			/// <exclude/>
			public new abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

			[PXString]
			[PXUIField(DisplayName = DisplayFieldNames.LastModifiedByID, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			[PXFormula(typeof(IsNull<IsNull<SmartJoin<Space, firstName, lastName>, username>, Empty>))]
			public override string DisplayName { get; set; }
		}

		/// <exclude/>
		[PXLocalizable]
		public static class DisplayFieldNames
		{
			public const string CreatedByID = "Created By";
			public const string CreatedDateTime = "Created On";
			public const string LastModifiedByID = "Last Modified By";
			public const string LastModifiedDateTime = "Last Modified On";
		}

		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXDBLastModifiedByIDAttribute()
			: base(typeof(Modifier.pKID), typeof(Modifier.username), typeof(Modifier.displayName),
				typeof(Modifier.pKID), typeof(Modifier.username))
		{
			if (_UIFldAttrIndex < 0)
			{
				_Attributes.Add(new PXUIFieldAttribute{ Enabled = false, Visible = true, IsReadOnly = true});
				_UIFldAttrIndex = _Attributes.Count - 1;
			}
			PXUIFieldAttribute uiattr = _Attributes[_UIFldAttrIndex] as PXUIFieldAttribute;
			uiattr.DisplayName = DisplayFieldNames.LastModifiedByID;
			
		}

		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			sender.SetValue(e.NewRow, _FieldOrdinal, GetUserID(sender));
		}

		void IPXRowPersistingSubscriber.RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				sender.SetValue(e.Row, _FieldOrdinal, GetUserID(sender));
			}
		}
	}
	#endregion

	#region PXDBCreatedByScreenIDAttribute
	/// <summary>Maps a DAC field to the database column and automatically
	/// sets the field value to the string ID of the application screen that
	/// created the data record.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field data type should be <tt>string</tt>.</remarks>
	/// <example>
	/// <code>
	/// [PXDBCreatedByScreenID()]
	/// public virtual string CreatedByScreenID { get; set; }
	/// </code>
	/// </example>
	public class PXDBCreatedByScreenIDAttribute : PXDBStringAttribute, IPXRowInsertingSubscriber
	{
		/// <summary>
		/// Initializes a new instance of the <tt>PXDBCreatedByScreenID</tt>
		/// attribute. Limits the maximum value length and sets the input mask.
		/// </summary>
		public PXDBCreatedByScreenIDAttribute()
			: base(10)
		{
			InputMask = "aa.aa.aa.aa";
		}

		protected string GetScreenID(PXCache sender)
		{
			if (sender.Graph.Accessinfo != null && sender.Graph.Accessinfo.ScreenID != null) {
				return ((string)sender.Graph.Accessinfo.ScreenID).Replace(".", "");
			}
			else {
				return "00000000";
			}
		}

		void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			sender.SetValue(e.Row, _FieldOrdinal, GetScreenID(sender));
		}
	}
	#endregion

	#region PXDBLastModifiedByScreenIDAttribute
	/// <summary>Maps a DAC field to the database column and automatically
	/// sets the field value to the string ID of the application screen on
	/// which the data record was modified the last time.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field data type should be <tt>string</tt>.</remarks>
	/// <example>
	/// <code>
	/// [PXDBLastModifiedByScreenID()]
	/// public virtual string LastModifiedByScreenID { get; set; }
	/// </code>
	/// </example>
	public class PXDBLastModifiedByScreenIDAttribute : PXDBCreatedByScreenIDAttribute, IPXRowUpdatingSubscriber, IPXRowPersistingSubscriber
	{
		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			sender.SetValue(e.NewRow, _FieldOrdinal, GetScreenID(sender));
		}

		void IPXRowPersistingSubscriber.RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				sender.SetValue(e.Row, _FieldOrdinal, GetScreenID(sender));
			}
		}
	}
	#endregion

	#region PXDBCreatedDateTimeAttribute
	/// <summary>Maps a DAC field to the database column and automatically sets the field value to the data record's creation date and time in UTC.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field data type should be <tt>DateTime?</tt>.</remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXDBCreatedDateTime()]
	/// public virtual DateTime? CreatedDateTime { get; set; }</code>
	/// </example>
	public class PXDBCreatedDateTimeAttribute : PXDBDateAttribute, IPXCommandPreparingSubscriber, IPXRowInsertingSubscriber
	{
		protected virtual DateTime GetDate()
		{
			DateTime? serverDate = PXTransactionScope.GetServerDateTime(true);
			return serverDate.HasValue
				? PXTimeZoneInfo.ConvertTimeFromUtc(serverDate.Value, GetTimeZone())
				: PXTimeZoneInfo.Now;
		}

		/// <summary>
		/// Initializes a new instance of the <tt>PXDBCreatedDateTime</tt> attribute.
		/// </summary>
		public PXDBCreatedDateTimeAttribute()
			: base()
		{
			this.UseSmallDateTime = false;
			PreserveTime = true;
			base.UseTimeZone = true;
		}

		/// <summary>Gets or sets the value that indicates (if set to <tt>true</tt>) that the attribute should convert the time to UTC, using the local time zone.</summary>
		/// <value>
		/// By default, the value is <tt>true</tt>.</value>
		public override bool UseTimeZone
		{
			get { return base.UseTimeZone; }
			set { }
		}

		void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null) sender.SetValue(e.Row, _FieldOrdinal, GetDate());
		}
		void IPXCommandPreparingSubscriber.CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert)
			{
				e.DataLength = 8;
				e.IsRestriction = e.IsRestriction || _IsKey;
				PrepareFieldName(_DatabaseFieldName, e);

				e.DataType = PXDbType.DirectExpression;
				e.DataValue = UseTimeZone ? e.SqlDialect.GetUtcDate : e.SqlDialect.GetDate;

				sender.SetValue(e.Row, _FieldOrdinal, GetDate());
			}
			else base.CommandPreparing(sender, e);
		}
	}
	#endregion

	#region PXDBLastModifiedDateTimeAttribute
	/// <summary>Maps a DAC field to the database column and automatically sets the field value to the data record's last modification date and time in UTC.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field. The field data type should be <tt>DateTime?</tt>.</remarks>
	/// <example>
	///   <code title="Example" description="" lang="CS">
	/// [PXDBLastModifiedDateTime]
	/// [PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
	/// public virtual DateTime? LastModifiedDateTime { get; set; }</code>
	/// </example>
	public class PXDBLastModifiedDateTimeAttribute : PXDBCreatedDateTimeAttribute, IPXCommandPreparingSubscriber, IPXRowUpdatingSubscriber
	{
		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (sender.GetValue(e.NewRow, _FieldOrdinal) == null) sender.SetValue(e.NewRow, _FieldOrdinal, GetDate());
		}
		void IPXCommandPreparingSubscriber.CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert
				|| (e.Operation & PXDBOperation.Update) == PXDBOperation.Update)
			{
				e.DataLength = 8;
				e.IsRestriction = e.IsRestriction || _IsKey;
				PrepareFieldName(_DatabaseFieldName, e);
				e.DataType = PXDbType.DirectExpression;
				e.DataValue = UseTimeZone ? e.SqlDialect.GetUtcDate : e.SqlDialect.GetDate;

				sender.SetValue(e.Row, _FieldOrdinal, GetDate());
			}
			else base.CommandPreparing(sender, e);
		}
	}
	#endregion

	#region PXDBBinaryAttribute
	/// <summary>Maps a DAC field of <tt>byte[]</tt> type to the binary
	/// database column of either fixed or variable length.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBBinary]
	/// [PXUIField(Visible = false)]
	/// public virtual byte[] NewValue { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBBinaryAttribute : PXDBFieldAttribute
	{
		#region State
		protected int _Length = -1;
		protected bool _IsFixed;

		/// <summary>Gets or sets an indication that the binay value has a fixed
		/// length. This property should be set to <tt>true</tt> if the database
		/// column has a fixed length type (<tt>binary</tt>) and to <tt>false</tt>
		/// if the database column has a variable length type
		/// (<tt>varbinary</tt>). The default value is <tt>false</tt>.</summary>
		public bool IsFixed
		{
			get { return _IsFixed; }
			set { _IsFixed = value; }
		}

		/// <summary>Gets the maximum length of the binary value.</summary>
		/// <remarks>The default value is -1 (the length is not limited). A different
		/// value can be set in the constructor.</remarks>
		public int Length => _Length;
		#endregion

		#region Ctor
		/// <summary>Initializes a new unparameterized instance of the attribute.</summary>
		public PXDBBinaryAttribute() { }

		/// <summary>Initializes a new instance with the given maximum
		/// length.</summary>
		/// <param name="length">The maximum length of the field value.</param>
		/// <example>
		/// The code below shows the value definition of a DAC field.
		/// <code>
		/// [PXDBBinary(500)]
		/// public byte[] CommonValues { get; set; }
		/// </code>
		/// </example>
		public PXDBBinaryAttribute(int length) { _Length = length; }
		#endregion

		#region Implementation
		/// <exclude/>
		protected override void PrepareCommandImpl(string dbFieldName, PXCommandPreparingEventArgs e)
		{
			base.PrepareCommandImpl(dbFieldName, e);
			if (_DatabaseFieldName != null && (e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy)
				e.Expr = SQLExpression.Null();
			e.DataType = _IsFixed ? PXDbType.Binary : PXDbType.VarBinary;
			e.DataValue = SerializeValue(e.Value) ?? new byte[0];
			if (_Length > -1)
				e.DataLength = _Length;
		}

		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				byte[] buff;
				if (_Length > -1)
				{
					buff = new byte[_Length];
					e.Record.GetBytes(e.Position, 0, buff, 0, _Length);
				}
				else
				{
					buff = e.Record.GetBytes(e.Position);
				}

				if (buff == null)
					buff = new byte[0];

				object deserializedValue = DeserializeValue(buff);
				sender.SetValue(e.Row, _FieldOrdinal, deserializedValue);
			}
			e.Position++;
		}

		protected virtual object DeserializeValue(Byte[] bytes) => bytes;
		protected virtual Byte[] SerializeValue(object value) => (byte[])value;
		#endregion
	}
	#endregion

	/// <exclude/>
	public class PXDBBinaryStringAttribute : PXDBBinaryAttribute
	{
		protected override Object DeserializeValue(Byte[] bytes) => Encoding.UTF8.GetString(bytes);
		protected override Byte[] SerializeValue(Object value) => ((string)value).With(Encoding.UTF8.GetBytes);
	}

	/// <summary>
	/// Maps a DAC field of <tt>ushort[]</tt> type to the binary database column of variable length, using the One Hot encoding.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBPackedIntegerArrayAttribute : PXDBBinaryAttribute
	{
		protected IEnumerable<ushort> ExtractHotBitPositions(byte[] bytes)
		{
			ushort pass = 0;
			foreach (Byte b in bytes)
			{
				Int32 offset = pass * 8;
				if ((b & 1) == 1) yield return (ushort)(1 + offset);
				if ((b & 2) == 2) yield return (ushort)(2 + offset);
				if ((b & 4) == 4) yield return (ushort)(3 + offset);
				if ((b & 8) == 8) yield return (ushort)(4 + offset);
				if ((b & 16) == 16) yield return (ushort)(5 + offset);
				if ((b & 32) == 32) yield return (ushort)(6 + offset);
				if ((b & 64) == 64) yield return (ushort)(7 + offset);
				if ((b & 128) == 128) yield return (ushort)(8 + offset);
				pass++;
			}
		}

		protected byte ToHotBit(ushort value)
		{
			if (value == 0) return 0;
			if (value == 1) return 1;
			if (value == 2) return 2;
			if (value == 3) return 4;
			if (value == 4) return 8;
			if (value == 5) return 16;
			if (value == 6) return 32;
			if (value == 7) return 64;
			if (value == 8) return 128;
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		protected byte[] PackToHotBits(IEnumerable<ushort> hotBitPositions)
		{
			var sortedPositions = hotBitPositions.OrderBy(p => p).ToArray();

			ushort nearestPowerOfTwo = (ushort)Math.Ceiling(Math.Log(sortedPositions.LastOrDefault()) / Math.Log(2));
			byte[] bytes = new byte[Math.Max(1, (ushort)Math.Pow(2, nearestPowerOfTwo) / 8)];
			ushort pass = 0;
			foreach (ushort pos in sortedPositions)
			{
				while (pos > (pass + 1) * 8)
					pass++;
				bytes[pass] |= ToHotBit((ushort)(pos - pass * 8));
			}

			return bytes;
		}

		protected override Object DeserializeValue(Byte[] bytes)
		{
			return bytes != null
				? ExtractHotBitPositions(bytes).ToArray()
				: null;
		}

		protected override Byte[] SerializeValue(Object value)
		{
			return value != null
				? PackToHotBits((ushort[])value)
				: null;
		}
	}

	#region PXDBGroupMaskAttribute
	/// <summary>Marks a DAC field of <tt>byte[]</tt> type that holds the
	/// group mask value.</summary>
	/// <example>
	/// 	<code title="Example" description="The code below shows definition of a DAC field tha holds a group mask value." lang="CS">
	/// [PXDBGroupMask()]
	/// public virtual Byte[] GroupMask { get; set; }</code>
	/// </example>
	public class PXDBGroupMaskAttribute : PXDBBinaryAttribute
	{
		#region Ctor
		/// <summary>Initializes an instance of the attribute with default
		/// parameters.</summary>
		public PXDBGroupMaskAttribute()
			: base()
		{
		}
		/// <summary>Initializes an instance of the attribute with the specified
		/// maximum length of the value.</summary>
		public PXDBGroupMaskAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void securedFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnValue = false;
			if (e.Row != null) {
				byte[] mask = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
				if (mask != null) {
					foreach (byte b in mask) {
						if (b != 0x00) {
							e.ReturnValue = true;
							break;
						}
					}
				}
			}
			else {
				e.ReturnState = PXFieldState.CreateInstance(null, typeof(bool), false, true, null, null, null, null, "Secured", null, PXMessages.Localize(ActionsMessages.Secured), null, PXErrorLevel.Undefined, false, true, null, PXUIVisibility.Invisible, null, null, null);
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (!sender.Fields.Contains("Secured")) {
				sender.Fields.Add("Secured");
				sender.FieldSelectingEvents["secured"] += securedFieldSelecting;
			}
		}
		#endregion
	}
	#endregion

	#region PXDBVariantAttribute
	/// <summary>Maps a DAC field of <tt>byte[]</tt> type to the database
	/// column of a variant type.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBVariant]
	/// [PXUIField(DisplayName = "Value")]
	/// public virtual byte[] Value { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBVariantAttribute : PXDBBinaryAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region Ctor
		/// <summary>Initializes a new instance of the attribute
		/// with default parameters.</summary>
		public PXDBVariantAttribute()
			: base()
		{
		}

		/// <summary>Initializes a new instance with the given maximum
		/// length.</summary>
		/// <param name="length">The maximum length of the value.</param>
		public PXDBVariantAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null && e.ReturnValue != null && e.ReturnValue.GetType() == typeof(byte[]))
				e.ReturnValue = GetValue((byte[])e.ReturnValue);
		}

		/// <summary>
		/// Gets a typed value from an array of bytes. The first byte
		/// in the array is converted to <tt>Typecode</tt> to determine
		/// the data type of the value.
		/// </summary>
		/// <param name="val">The array of bytes to convert to a typed value.</param>
		/// <returns>The typed value.</returns>
		public static object GetValue(byte[] val)
		{
			if (val.Length > 0) {
				using (System.IO.MemoryStream ms = new System.IO.MemoryStream(val, 1, val.Length - 1))
				using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms, Encoding.Unicode)) {
					switch ((TypeCode)(val[0])) {
						case TypeCode.Boolean:
							return br.ReadBoolean();
						case TypeCode.Byte:
							return br.ReadByte();
						case TypeCode.Char:
							return br.ReadChar();
						case TypeCode.DateTime:
							return new DateTime(br.ReadInt64());
						case TypeCode.Decimal:
							return br.ReadDecimal();
						case TypeCode.Double:
							return br.ReadDouble();
						case TypeCode.Int16:
							return br.ReadInt16();
						case TypeCode.Int32:
							return br.ReadInt32();
						case TypeCode.Int64:
							return br.ReadInt64();
						case TypeCode.SByte:
							return br.ReadSByte();
						case TypeCode.Single:
							return br.ReadSingle();
						case TypeCode.String:
							return br.ReadString();
						case TypeCode.UInt16:
							return br.ReadUInt16();
						case TypeCode.UInt32:
							return br.ReadUInt32();
						case TypeCode.UInt64:
							return br.ReadUInt64();
						default:
							throw new PXException(ErrorMessages.CantRestoreValueFromByteArray);
					}
				}
			}
			return null;
		}

		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null) {
				if (e.NewValue != null && e.NewValue.GetType() != typeof(byte[])) {
					e.NewValue = SetValue(e.NewValue);
				}
			}
		}

		/// <summary>
		/// Convert a typed value to an array of bytes.
		/// </summary>
		/// <param name="value">The data value to convert.</param>
		/// <returns>The array of bytes representing the provided value.</returns>
		public static byte[] SetValue(object value)
		{
			TypeCode c = Type.GetTypeCode(value.GetType());
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
			using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, Encoding.Unicode)) {
				bw.Write((byte)c);
				switch (c) {
					case TypeCode.Boolean:
						bw.Write((Boolean)value);
						return ms.ToArray();
					case TypeCode.Byte:
						bw.Write((Byte)value);
						return ms.ToArray();
					case TypeCode.Char:
						bw.Write((Char)value);
						return ms.ToArray();
					case TypeCode.DateTime:
						bw.Write(((DateTime)value).Ticks);
						return ms.ToArray();
					case TypeCode.Decimal:
						bw.Write((Decimal)value);
						return ms.ToArray();
					case TypeCode.Double:
						bw.Write((Double)value);
						return ms.ToArray();
					case TypeCode.Int16:
						bw.Write((Int16)value);
						return ms.ToArray();
					case TypeCode.Int32:
						bw.Write((Int32)value);
						return ms.ToArray();
					case TypeCode.Int64:
						bw.Write((Int64)value);
						return ms.ToArray();
					case TypeCode.SByte:
						bw.Write((SByte)value);
						return ms.ToArray();
					case TypeCode.Single:
						bw.Write((Single)value);
						return ms.ToArray();
					case TypeCode.String:
						bw.Write((String)value);
						return ms.ToArray();
					case TypeCode.UInt16:
						bw.Write((UInt16)value);
						return ms.ToArray();
					case TypeCode.UInt32:
						bw.Write((UInt32)value);
						return ms.ToArray();
					case TypeCode.UInt64:
						bw.Write((UInt64)value);
						return ms.ToArray();
					default:
						throw new PXException(ErrorMessages.CantConvertValueToByteArray);
				}
			}
		}

		#endregion
	}
	#endregion

	#region PXDBWeblinkAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class PXDBWeblinkAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber
	{
		public PXDBWeblinkAttribute()
			: base(255)
		{
			IsUnicode = true;
			IsFixed = false;
		}
		static readonly Regex regex = new Regex("(http://|https://)?(www\\.)?[\\w-]+(\\.[\\w-]+)+(/[\\w-]+)*(" +
								"\\.(html|htm|cgi|php|aspx|asp|\\w+))?(\\?(\\w+\\=[\\w%+.]+){" +
								"1}(&\\w+\\=[\\w%+.]+)*)?", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static bool isPossibleURL(string url)
		{
			if (string.IsNullOrEmpty(url))
				return false;
			if (url.IndexOf("://", StringComparison.Ordinal) < 0)
				url = "http://" + url;

			Uri uri;
			if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
				return false;
			if (!String.IsNullOrEmpty(uri.Scheme) && !uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase))
				return false;

			return true;
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall && !sender.Graph.IsMobile)
				return;

			var val = e.NewValue as string;
			if (String.IsNullOrEmpty(val))
				return;

			if (val.Length < 4 || val.Length > 255)
				throw new PXSetPropertyException(ErrorMessages.InvalidWeblink);

			if (sender.Graph.IsImport || sender.Graph.IsExport)
			{
				if (isPossibleURL(val))
					return;
			}
			else
			{
				string trimmed = val.Trim();
				string lower = trimmed.ToLower();
				if (regex.IsMatch(trimmed) && !lower.StartsWith("javascript:") && !lower.StartsWith("vbscript:"))
					return;
			}

			throw new PXSetPropertyException(ErrorMessages.InvalidWeblink);

		}
	}
	#endregion

	#region PXDBEmailAttribute
	/// <summary>Maps a <tt>string</tt> DAC field representing email addresses
	/// to the database column of <tt>nvarchar</tt> type.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</para>
	/// <para>The field value must be a Unicode string. The field value length
	/// is limited by 255.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBEmail]
	/// [PXUIField(DisplayName = "Email",
	///            Visibility = PXUIVisibility.SelectorVisible)]
	/// public virtual string Email { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class PXDBEmailAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber
	{
		#region Nested Types

		/// <exclude/>
		public interface IEmailValidator
		{
			bool Validate(string email);
		}

		// Known issues: doesn't validate adresses like "jsmith@gmail."
		/// <exclude/>
		public class DotNetEmailValidator : IEmailValidator
		{
			public bool Validate(string email)
			{
				try
				{
					new System.Net.Mail.MailAddress(email);
					return true;
				}
				catch (FormatException)
				{
					return false;
				}
			}
		}

		#endregion
		
		private const string EMAIL_SEPARATOR = "; ";

		/// <exclude/>
		public static Dictionary<Type, List<string>> FieldList = new Dictionary<Type, List<string>>();
		/// <summary>
		/// Initializes a new instance of the attribute. The maximum string length is set to 255. The string is marked as Unicode.
		/// </summary>
		public PXDBEmailAttribute()
			: base(255)
		{
			IsUnicode = true;
			IsFixed = false;
		}

		/// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
				return;

			var val = e.NewValue as string;
			if (String.IsNullOrEmpty(val))
				return;

			e.NewValue = FormatAddressesWithoutDisplayName(val);
		}

		protected internal override void SetBqlTable(Type bqlTable)
			{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)FieldList).SyncRoot) {
				List<string> list;
				if (!FieldList.TryGetValue(bqlTable, out list)) {
					FieldList[bqlTable] = list = new List<string>();
				}
				if (!list.Contains(base.FieldName)) {
					list.Add(base.FieldName);
			}
		}
		}
		/// <exclude/>
		public static List<string> GetEMailFields(Type table)
		{
			if (ServiceManager.EnsureCachesInstatiated(true)) {
				List<string> list;
				if (FieldList.TryGetValue(table, out list)) {
					return list;
		}
		}
			return new List<string>();
		}

		/// <exclude />
		public static string FormatAddressesWithoutDisplayName(string addresses)
		{
			var addressesList = ParseAddressesList(addresses);

			return string.Join(EMAIL_SEPARATOR, addressesList.Select(_ => _.Address)).TrimEnd(' ');
		}

		/// <exclude />
		public static string FormatAddressesWithSingleDisplayName(string addresses, string displayName)
		{
			var addressesList = ParseAddressesList(addresses);

			return string.Join(EMAIL_SEPARATOR, addressesList.Select(_ => new MailAddress(_.Address, displayName))).TrimEnd(' ');
		}

		/// <exclude />
		public static string AppendAddresses(string addresses1, string addresses2)
		{
			var addressesList1 = ParseAddressesList(addresses1);
			var addressesList2 = ParseAddressesList(addresses2);

			return string.Join(EMAIL_SEPARATOR, addressesList1.Union(addressesList2));
		}

		/// <exclude />
		public static string ToString(IEnumerable<MailAddress> addresses)
		{
			return string.Join(EMAIL_SEPARATOR, addresses);
		}

		/// <exclude />
		public static string ToRFC(string addresses)
		{
			var addressesList = ParseAddressesList(addresses);

			return string.Join(EMAIL_SEPARATOR, addressesList);
		}

		private static List<MailAddress> ParseAddressesList(string addresses)
		{
			if (String.IsNullOrWhiteSpace(addresses))
				return new List<MailAddress>();

			List<MailAddress> addressesList;
			try
		{
				addressesList = EmailParser.ParseAddresses(addresses);
				}
			catch (ArgumentException)
			{
				throw new PXSetPropertyException(ErrorMessages.InvalidEmail, addresses);
			}
			if (addressesList == null || !addressesList.Any())
				throw new PXSetPropertyException(ErrorMessages.InvalidEmail, addresses);
			
			return addressesList;
		}
	}
	#endregion

	#region PXDBCryptStringAttribute
	/// <summary>The attribute is added to the value declaration of a DAC field. The field becomes bound to the database column with the same name.</summary>
	public class PXDBCryptStringAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber, IPXRowUpdatingSubscriber, IPXRowSelectingSubscriber
	{
		#region State
		protected bool isViewDeprypted;
		protected string viewAsString;
		protected Type viewAsField;
		protected bool isEncryptionRequired = true;

		/// <summary>Get, set.</summary>
		public bool IsViewDecrypted
		{
			get { return isViewDeprypted; }
			set { isViewDeprypted = value; }
		}

		/// <summary>Get, set.</summary>
		public string ViewAsString
		{
			get { return this.viewAsString; }
			set { this.viewAsField = null; this.viewAsString = value; }
		}

		/// <summary>Get, set.</summary>
		public Type ViewAsField
		{
			get { return this.viewAsField; }
			set
			{
				this.viewAsField = value; this.viewAsString = null;
			}
		}

		/// <summary>Get, set.</summary>
		public bool IsEncryptionRequired
		{
			get { return isEncryptionRequired; }
			set { isEncryptionRequired = value; }
		}
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance with default parameters.</summary> 
		public PXDBCryptStringAttribute()
		{
		}
		/// <summary>Initializes a new instance with the given maximum
		/// length.</summary>
		public PXDBCryptStringAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Runtime
		/// <summary>Overrides the visible state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetDecrypted(PXCache cache, object data, string field, bool isDecrypted)
		{
			if (data == null) {
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
				}
			}
		}
		/// <summary>Overrides the visible state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetDecrypted(PXCache cache, string field, bool isDecrypted)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
					break;
				}
			}
		}
		/// <summary>Overrides the visible state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetDecrypted<Field>(PXCache cache, object data, bool isDecrypted)
			where Field : IBqlField
		{
			if (data == null) {
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetDecrypted<Field>(PXCache cache, bool isDecrypted)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>()) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
					break;
				}
			}
		}

		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs(PXCache cache, object data, string field, string source)
		{
			if (data == null) {
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs(PXCache cache, string field, string source)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
					break;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs<Field>(PXCache cache, object data, string source)
			where Field : IBqlField
		{
			if (data == null) {
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs<Field>(PXCache cache, string source)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>()) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
					break;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs(PXCache cache, object data, string field, Type sourceField)
		{
			if (data == null) {
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs(PXCache cache, string field, Type sourceField)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
					break;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs<Field>(PXCache cache, object data, Type sourceField)
			where Field : IBqlField
		{
			if (data == null) {
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data)) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
				}
			}
		}
		/// <summary>Overrides the view as state for the particular data
		/// item.</summary>
		/// <param name="cache">Cache containing the data item.</param>
		/// <param name="def">Default value.</param>
		public static void SetViewAs<Field>(PXCache cache, Type sourceField)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>()) {
				if (attr is PXDBCryptStringAttribute) {
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
					break;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (!sender.BypassAuditFields.Contains(this.FieldName))
				sender.BypassAuditFields.Add(this.FieldName);
		}

		/// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!isViewDeprypted && e.Row != null &&
					(string)e.NewValue == ViewString(sender, e.Row))
				e.Cancel = true;
		}

		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);
			if (e.Row == null || sender.GetStatus(e.Row) != PXEntryStatus.Notchanged) return;

			string value = (string)sender.GetValue(e.Row, _FieldOrdinal);
			string result = string.Empty;
			if (!string.IsNullOrEmpty(value)) {
				if (isEncryptionRequired) {
					try {
						result = Encoding.Unicode.GetString(Decrypt(Convert.FromBase64String(value)));
					}
					catch (Exception) {
						try {
							result = Encoding.Unicode.GetString(Convert.FromBase64String(value));
						}
						catch (Exception) {
							result = value;
						}
					}
				}
				else {
					result = value;
				}
			}
			sender.SetValue(e.Row, _FieldOrdinal, result.Replace("\0", string.Empty));
		}

		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!this.isViewDeprypted && e.Row != null)
				e.ReturnValue = ViewString(sender, e.Row);

			base.FieldSelecting(sender, e);

			if (sender._SelectingForAuditExplore && e.ReturnValue is string && sender._EncryptAuditFields != null && sender._EncryptAuditFields.Contains(this.FieldName))
			{
				try
				{
					e.ReturnValue = Encoding.Unicode.GetString(Decrypt(Convert.FromBase64String((string)e.ReturnValue)));
				}
				catch
				{
				}
			}
		}
		/// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) && isEncryptionRequired) {
				string value = (string)sender.GetValue(e.Row, _FieldOrdinal);

				e.Value = !string.IsNullOrEmpty(value)
					?
						Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(value)))
					:
						null;
			}
			base.CommandPreparing(sender, e);
		}

		/// <exclude/>
		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (e.Row == null) return;

			string value = (string)sender.GetValue(e.NewRow, _FieldOrdinal);
			if (!IsViewDecrypted && value == ViewString(sender, e.NewRow)) {
				object oldValue = sender.GetValue(e.Row, _FieldOrdinal);
				sender.SetValue(e.NewRow, _FieldOrdinal, oldValue);
			}
		}

		private void Encrypt(PXCache sender, object row)
		{
			if (row == null) return;
			string value = (string)sender.GetValue(row, _FieldOrdinal);
			string result = string.Empty;

			if (!string.IsNullOrEmpty(value))
				result = Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(value)));

			sender.SetValue(row, _FieldOrdinal, result);
		}

		protected virtual string DefaultViewAsString
		{
			get
			{
				return new string('*', this.Length > 0 && this.Length < 8 ? this.Length : 8);
			}
		}

		protected virtual byte[] Encrypt(byte[] source)
		{
			return source;
		}

		protected virtual byte[] Decrypt(byte[] source)
		{
			return source;
		}

		private string ViewString(PXCache cache, object data)
		{
			if (ViewAsField != null)
				return cache.GetValue(data, viewAsField.Name).ToString();

			return ViewAsString != null ? ViewAsString : DefaultViewAsString;
		}

		#endregion
	}
	#endregion

	#region PXDB3DesCryphStringAttribute

	/// <exclude/>
	public class PXDB3DesCryphStringAttribute : PXDBCryptStringAttribute
	{
		#region Ctor
		/// 
		public PXDB3DesCryphStringAttribute()
		{
		}
		/// <summary>Initializes a new instance with the given maximum
		/// length.</summary>
		public PXDB3DesCryphStringAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Runtime
		/// 
		public static string Encrypt(string source)
		{
			return Convert.ToBase64String(SitePolicy.TripleDESCryptoProvider.Encrypt(Encoding.Unicode.GetBytes(source)));
		}
		#endregion

		#region Implementation
		protected override byte[] Encrypt(byte[] source)
		{
			return SitePolicy.TripleDESCryptoProvider.Encrypt(source);
		}

		protected override byte[] Decrypt(byte[] source)
		{
			return SitePolicy.TripleDESCryptoProvider.Decrypt(source);
		}
		#endregion
	}
	#endregion

	#region PXDBLiteDefaultAttribute
	/// <summary>Provides default value for the property or parameter. The attribute is used for
	/// defaulting from an auto-generated key field.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class PXDBLiteDefaultAttribute : PXDBDefaultAttribute
	{
		#region Ctor
		/// <summary>
		/// Defines the default from the item of the <tt>sourceType</tt> type.
		/// </summary>
		/// <param name="sourceType">The type to get the default value from. If the type implements IBqlField and is nested, the parameter defines the source field as well.</param>
		public PXDBLiteDefaultAttribute(Type sourceType)
			: base(sourceType)
		{
		}
		#endregion
	}
	#endregion

	#region PXDBChildDefaultAttribute
	/// <summary>Indicates that a DAC field references an auto-generated key
	/// field from another table and ensures the DAC field's value is correct
	/// after changes are committed to the database.</summary>
	/// <remarks>The attribute updates the field value once the source field is
	/// assigned a real value by the database.</remarks>
	/// <example>
	/// <code>
	/// [PXDBInt()]
	/// [PXDBChildIdentity(typeof(Address.addressID))]
	/// public virtual int? DefPOAddressID { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBChildIdentityAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		#region State
		protected Type _SourceType;
		protected string _SourceField;
		protected Dictionary<object, object> _Persisted;
		protected Type _SelfType;
		#endregion

		#region Ctor
		/// <summary>
		/// Initializes a new instance that takes the value for the field
		/// the attribute is attached to from the provided source field.
		/// </summary>
		/// <param name="sourceType">The source field type to get the value from, should
		/// be nested (defined in a DAC) and implement <tt>IBqlField</tt>.</param>
		public PXDBChildIdentityAttribute(Type sourceType)
		{
			if (sourceType == null) {
				throw new ArgumentNullException("sourceType");
			}
			if (sourceType.IsNested && typeof(IBqlField).IsAssignableFrom(sourceType)) {
				_SourceType = BqlCommand.GetItemType(sourceType);
				_SourceField = sourceType.Name;

			}
			else {
				throw new ArgumentOutOfRangeException("sourceType", String.Format("Cannot create a foreign key reference from the type '{0}'.", sourceType));
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete) {
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null && Convert.ToInt32(key) < 0) {
					_Persisted[e.Row] = key;
					PXCache cache = sender.Graph.Caches[_SourceType];
					{
						object child;
						if (_Persisted.TryGetValue(key, out child) && _SourceType.IsAssignableFrom(child.GetType())) {
							sender.SetValue(e.Row, _FieldOrdinal, cache.GetValue(child, _SourceField));
						}
					}
					foreach (object child in cache.Inserted) {
						if (object.Equals(key, cache.GetValue(child, _SourceField))) {
							object parents;
							if (!_Persisted.TryGetValue(child, out parents)) {
								_Persisted[child] = new List<object>();
							}
							((List<object>)_Persisted[child]).Add(e.Row);
						}
					}
				}
			}
		}

		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && e.TranStatus == PXTranStatus.Aborted) {
				object key;
				if (_Persisted.TryGetValue(e.Row, out key)) {
					sender.SetValue(e.Row, _FieldOrdinal, key);
				}
			}
		}

		/// <exclude/>
		public void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) {
				object key = sender.GetValue(e.Row, _SourceField);
				if (key != null && Convert.ToInt32(key) < 0) {
					_Persisted[key] = e.Row;
				}
			}
		}

		/// <exclude/>
		public void SourceRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) && e.TranStatus == PXTranStatus.Open) {
				object parents;
				if (_Persisted.TryGetValue(e.Row, out parents) && parents != null && parents is List<object>) {
					foreach (object parent in (List<object>)parents) {
						if (!_SelfType.IsAssignableFrom(parent.GetType())) {
							continue;
						}

						object id = sender.GetValue(e.Row, _SourceField);
						var idintval = id.Return(c => Convert.ToInt32(id), 0);
						if (id != null && idintval > 0) {
							if (sender.Graph.TimeStamp == null) {
								sender.Graph.SelectTimeStamp();
							}
							PXCache cache = sender.Graph.Caches[_SelfType];
							List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								cache.RaiseCommandPreparing(_FieldName, parent, id, PXDBOperation.Update, _BqlTable, out description);
								if (description != null && description.Expr != null) {
									PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, cache.ValueToString(_FieldName, id));
									pars.Add(assign);
								}
							}
							KeysVerifyer kv = new KeysVerifyer(cache);
							foreach (string field in cache.Fields) {
								PXCommandPreparingEventArgs.FieldDescription description;
								cache.RaiseCommandPreparing(field, parent, cache.GetValue(parent, field), PXDBOperation.Update, _BqlTable, out description);
								if (description != null && description.Expr != null)
								{
									if (description.IsRestriction && description.DataValue != null &&
										description.DataType != PXDbType.Timestamp)
									{
										kv.SetRestriction(field, description);
										if (typeof(PXCacheExtension).IsAssignableFrom(_BqlTable) &&
											_BqlTable.IsDefined(typeof(PXTableAttribute), true) &&
											cache.Keys.Contains(field))
										{
											if (
												!PXDatabase.Provider.GetTableStructure(_BqlTable.Name)
													.Columns.Any(
														c =>
															string.Equals(c.Name, field,
																StringComparison.OrdinalIgnoreCase)))
											{
												continue;
											}
										}
										pars.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType,
											description.DataLength, description.DataValue));
									}
								}
							}
							pars.Add(PXDataFieldRestrict.OperationSwitchAllowed);
							kv.Check(_BqlTable);
							pars.Add(PXSelectOriginalsRestrict.SelectOriginalValues);
							PXDatabase.Update(_BqlTable, pars.ToArray());
							cache.SetValue(parent, _FieldOrdinal, id);
						}
					}
				}
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			_Persisted = new Dictionary<object, object>();
			_SelfType = sender.GetItemType();
			sender.Graph.RowPersisting.AddHandler(_SourceType, SourceRowPersisting);
			sender.Graph.RowPersisted.AddHandler(_SourceType, SourceRowPersisted);
		}
		#endregion
	}
	#endregion

	#region PXDBTimeSpanLongAttribute
	/// <summary>Maps a DAC field of the <tt>int?</tt> type that represents the
	/// duration (in minutes) to an <tt>int</tt> database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</remarks>
	/// <example>
	/// <code>
	/// [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
	/// [PXUIField(DisplayName = "Estimation")]
	/// public virtual Int32? TimeEstimated { get; set; }
	/// </code>
	/// </example>
	public class PXDBTimeSpanLongAttribute : PXDBIntAttribute
	{
		#region State

		protected string[] _inputMasks = new string[] { ActionsMessages.TimeSpanMaskDHM, "### d 00:00", ActionsMessages.TimeSpanLongHM, ActionsMessages.TimeSpanHM, "00:00" };
		protected string[] _outputFormats = new string[] { "{0,3}{1:00}{2:00}", "{0,3}{1:00}{2:00}", "{1,4}{2:00}", "{1,2}{2:00}", "{1:00}{2:00}" };
		protected int[] _lengths = new int[] { 7, 7, 6, 4, 4 };
		protected bool _NullIsZero = false;

		protected TimeSpanFormatType _Format = TimeSpanFormatType.DaysHoursMinites;
		/// <summary>Gets or sets the data format type. Possible values are
		/// defined by the <see
		/// cref="TimeSpanFormatType">TimeSpanFormatType</see>
		/// enumeration.</summary>
		public TimeSpanFormatType Format
		{
			get
			{
				return _Format;
			}
			set
			{
				_Format = value;
			}
		}
		/// <summary>Gets or sets the pattern that indicates the allowed
		/// characters in a field value. By default, the property is null, and the
		/// attribute determines the input mask by the <tt>Format</tt>
		/// value.</summary>
		public string InputMask
		{
			get => _inputMask;
			set
			{
				_inputMask = value;
				_maskLength = 0;

				foreach (char c in value) {
					if (c == '#' || c == '0')
						_maskLength += 1;
				}
			}
		}

		private string _inputMask;
		private int _maskLength;


		#endregion

		#region Ctor
		/// <summary>
		/// Initializes a new value with default parameters.
		/// </summary>
		public PXDBTimeSpanLongAttribute()
		{
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation

		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			int index = (int)_Format;

			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) 
			{
				string inputMask = _inputMask ?? _inputMasks[index];
				inputMask = PXMessages.LocalizeNoPrefix(inputMask);
				int length = _inputMask != null 
					? _maskLength 
					: _lengths[index];

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, length, null, _FieldName, _IsKey, null, String.IsNullOrEmpty(inputMask) ? null : inputMask, null, null, null, null);
			}

			if (e.ReturnState == null)
				return;

			int? minutes = e.ReturnValue is int intValue
				? intValue
				: e.ReturnValue is string stringValue && int.TryParse(stringValue, out int parsedValue)
					? parsedValue
					: default;

			if (!minutes.HasValue)
				return;

			TimeSpan span = new TimeSpan(0, 0, minutes.Value, 0);
			int hours = _Format == TimeSpanFormatType.LongHoursMinutes 
				? (span.Days * 24 + span.Hours) 
				: span.Hours;	
			e.ReturnValue = string.Format(_outputFormats[index], span.Days, hours, span.Minutes);		
		}

		/// <exclude/>
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string stringValue) 
			{
				int length = stringValue.Length;
				int index = (int)_Format;
				int maxLength = _lengths[index];

				if (length < maxLength) 
				{
					StringBuilder bld = new StringBuilder(maxLength);

					for (int i = length; i < maxLength; i++) 
					{
						bld.Append('0');
					}

					bld.Append(stringValue);
					e.NewValue = stringValue = bld.ToString();
				}

				if (!string.IsNullOrEmpty(stringValue) && int.TryParse(stringValue.Replace(" ", "0"), out int val)) 
				{
					int minutes = val % 100;
					int hours = ((val - minutes) / 100) % 100;
					int days = (((val - minutes) / 100) - hours) / 100;

					if (Format == TimeSpanFormatType.LongHoursMinutes) 
					{
						hours = (val - minutes) / 100;
						days = 0;
					}

					TimeSpan span = new TimeSpan(days, hours, minutes, 0);
					e.NewValue = (int)span.TotalMinutes;
				}
				else 
				{
					e.NewValue = null;
				}
			}

			if (e.NewValue == null && _NullIsZero)
				e.NewValue = 0;
		}
		#endregion
	}
	#endregion

	#region PXDBDateAndTimeAttribute

	/// <summary>Maps a DAC field of <tt>DateTime?</tt> type to the database
	/// column of <tt>datetime</tt> or <tt>smalldatetime</tt> type. Defines
	/// the DAC field that is represented in the UI by two input controls: one
	/// for date, the other for time.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</para>
	/// <para>Unlike the <see cref="PXDBDateAttribute">PXDBDate</see>
	/// attribute, this attribute defines the field that is represented in the
	/// UI by two input controls to specify date and time values
	/// separately.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBDateAndTime]
	/// [PXUIField(DisplayName = "Start Time")]
	/// public virtual DateTime? StartDate { get; set; }
	/// </code>
	/// </example>
	public class PXDBDateAndTimeAttribute : PXDBDateAttribute
	{
		/// <exclude/>
		public const string DATE_FIELD_POSTFIX = "_Date";
		/// <exclude/>
		public const string TIME_FIELD_POSTFIX = "_Time";

		/// <summary>
		/// Gets the localized additional label for the date part
		/// of the field.
		/// </summary>
		public string DateDisplayNamePostfix
		{
			get
			{
				return string.Format(" ({0})", PXLocalizer.Localize(SM.Messages.Date, typeof(SM.Messages).FullName));
			}
		}

		/// <summary>
		/// Gets the localized additional label for the time part
		/// of the field.
		/// </summary>
		public string TimeDisplayNamePostfix
		{
			get
			{
				return string.Format(" ({0})", PXLocalizer.Localize(SM.Messages.Time, typeof(SM.Messages).FullName));
			}
		}

		protected string _TimeInputMask = "t";
		protected string _TimeDisplayMask = "t";

		protected string _DateInputMask = "d";
		protected string _DateDisplayMask = "d";

		private string _displayNameDate = null;
		private string _displayNameTime = null;

		private bool _isEnabledDate = true;
		private bool _isEnabledTime = true;

		private bool _isVisibleDate = true;
		private bool _isVisibleTime = true;

		#region Ctor
		/// <summary>Initializes a new instance of the attribute with default
		/// parameters.</summary>
		public PXDBDateAndTimeAttribute()
		{
			this.PreserveTime = true;                        
		}
		#endregion

		#region Initialization

		/// <summary>Gets or sets the value that indicates whether the display
		/// names of the input controls for date and time are appended with
		/// <i>(Date)</i> and <i>(Time)</i>, respectively.</summary>
		public virtual bool WithoutDisplayNames { get; set; }

		/// <summary>Gets or sets the display name for the input control that
		/// represents date.</summary>
		public string DisplayNameDate
		{
			get { return _displayNameDate; }
			set { _displayNameDate = value; }
		}

		private string _neutralDisplayNameDate;

		public string NeutralDisplayNameDate
		{
			get
			{
				if (_neutralDisplayNameDate == null && _displayNameDate != null)
				{
					_neutralDisplayNameDate = _displayNameDate;
				}

				return _neutralDisplayNameDate;
			}
		}

		/// <summary>Gets or sets the display name for the input control that
		/// represents time.</summary>
		public string DisplayNameTime
		{
			get { return _displayNameTime; }
			set { _displayNameTime = value; }
		}

		private string _neutralDisplayNameTime;

		public string NeutralDisplayNameTime
		{
			get
			{
				if (_neutralDisplayNameTime == null && _displayNameTime != null)
				{
					_neutralDisplayNameTime = _displayNameTime;
				}

				return _neutralDisplayNameTime;
			}
		}

		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Fields.Add(_FieldName + DATE_FIELD_POSTFIX);
			sender.Fields.Add(_FieldName + TIME_FIELD_POSTFIX);

			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName + DATE_FIELD_POSTFIX, Date_FieldSelecting);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName + DATE_FIELD_POSTFIX, Date_FieldUpdating);

			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName + TIME_FIELD_POSTFIX, Time_FieldSelecting);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName + TIME_FIELD_POSTFIX, Time_FieldUpdating);

			TryLocalize(sender);
		}
		#endregion

		protected virtual void TryLocalize(PXCache sender)
		{
			if (ResourceCollectingManager.IsStringCollecting)
			{
				PXPageRipper.RipDateAndTime(this, sender, CollectResourceSettings.Resource);
			}
			else
			{
				PXLocalizerRepository.DateTimeLocalizer.Localize(this, sender);
			}
		}

		#region Implementation
		/// <exclude/>
		public void Date_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered) 
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + DATE_FIELD_POSTFIX, _IsKey, null, _DateInputMask, _DateDisplayMask, null, null);
				if (!WithoutDisplayNames) state.DisplayName += DateDisplayNamePostfix;
				foreach(PXDBDateAndTimeAttribute attr in GetAttribute(sender, e.Row, _FieldName))
				if (attr != null) 
				{
					if (attr._displayNameDate != null)
							state.DisplayName = PXLocalizer.Localize(attr._displayNameDate, sender.BqlTable.FullName);
					state.Enabled = state.Enabled && attr._isEnabledDate;
					state.Visible = state.Visible && attr._isVisibleDate;
				}
				e.ReturnState = state;
			}

			if (e.ReturnValue != null) {
				if (sender.Graph.IsMobile)
				{
					e.ReturnValue = new DateTime(((DateTime)e.ReturnValue).Year, ((DateTime)e.ReturnValue).Month,
						((DateTime)e.ReturnValue).Day, ((DateTime)e.ReturnValue).Hour, ((DateTime)e.ReturnValue).Minute, 0);
				}
				else
				{
					e.ReturnValue = new DateTime(((DateTime)e.ReturnValue).Year, ((DateTime)e.ReturnValue).Month,
						((DateTime)e.ReturnValue).Day);
				}
			}
		}

		/// <exclude/>
		public virtual void Time_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered) 
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + TIME_FIELD_POSTFIX, _IsKey, null, _TimeInputMask, _TimeDisplayMask, null, null);
				if (!WithoutDisplayNames) state.DisplayName += TimeDisplayNamePostfix;
				foreach (PXDBDateAndTimeAttribute attr in GetAttribute(sender, e.Row, _FieldName))
					if (attr != null) 
				{
					if (attr._displayNameTime != null)
							state.DisplayName = PXLocalizer.Localize(attr._displayNameTime, sender.BqlTable.FullName);
					state.Enabled = state.Enabled && attr._isEnabledTime;
					state.Visible = state.Visible && attr._isVisibleTime && _PreserveTime;
				}
				e.ReturnState = state;
			}

			if (e.ReturnValue != null) {
				if (sender.Graph.IsMobile)
				{
					e.ReturnValue = new DateTime(((DateTime)e.ReturnValue).Year, ((DateTime)e.ReturnValue).Month,
						((DateTime)e.ReturnValue).Day, ((DateTime)e.ReturnValue).Hour, ((DateTime)e.ReturnValue).Minute, 0);
				}
				else
				{
					e.ReturnValue = new DateTime(1900, 1, 1, ((DateTime)e.ReturnValue).Hour, ((DateTime)e.ReturnValue).Minute, 0);
				}
			}
		}

		/// <exclude/>
		public virtual void Date_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Cancel) return;

			if (e.NewValue is string) {
				DateTime val;
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val)) {
					object fieldval = (sender.Graph.IsMobile) ? val : CombineDateTime(val, oldval);
					sender.SetValue(e.Row, _FieldOrdinal, fieldval);
					sender.RaiseFieldUpdated(_FieldName + DATE_FIELD_POSTFIX, e.Row, oldval);
					if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null) {
						sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
					}
				}
				else {
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime) {
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);
				object fieldval = (sender.Graph.IsMobile) ? e.NewValue : CombineDateTime((DateTime)e.NewValue, oldval);
				if (sender.RaiseFieldVerifying(FieldName + DATE_FIELD_POSTFIX, e.Row, ref fieldval)) {
					sender.SetValue(e.Row, _FieldOrdinal, fieldval);
					sender.RaiseFieldUpdated(_FieldName + DATE_FIELD_POSTFIX, e.Row, oldval);
					if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null) {
						sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
					}
				}
			}
		}

		/// <exclude/>
		public void Time_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Cancel) return;

			if (e.NewValue is string) {
				DateTime val;
				DateTime? fieldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);
				if (fieldval != null && DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val)) 
				{
					fieldval = (sender.Graph.IsMobile) ? val : CombineDateTime(fieldval, val);
					sender.SetValueExt(e.Row, _FieldName, fieldval);
				}
				else {
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime) {
				DateTime? fieldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);
				fieldval = (sender.Graph.IsMobile) ? (DateTime)e.NewValue : CombineDateTime(fieldval, (DateTime)e.NewValue);
				sender.SetValueExt(e.Row, _FieldName, fieldval);
			}
		}

		/// <summary>
		/// Combines the provided data and time values in a single <tt>DateTime</tt>
		/// object.
		/// </summary>
		/// <param name="date">The date value.</param>
		/// <param name="time">The time value.</param>
		/// <returns>The combined <tt>DateTime</tt> value or <tt>null</tt> if the
		/// <tt>date</tt> value is <tt>null</tt>.</returns>
		/// <example>
		/// <code>
		/// ...
		/// switch (date.Value.DayOfWeek)
		/// {
		///    case DayOfWeek.Monday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.MonStartTime);
		///    case DayOfWeek.Tuesday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.TueStartTime);
		///    case DayOfWeek.Wednesday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.WedStartTime);
		///    case DayOfWeek.Thursday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.ThuStartTime);
		///    case DayOfWeek.Friday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.FriStartTime);
		///    case DayOfWeek.Saturday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.SatStartTime);
		///    case DayOfWeek.Sunday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.SunStartTime);
		/// }
		/// ...
		/// </code>
		/// </example>
		public static DateTime? CombineDateTime(DateTime? date, DateTime? time)
		{
			if (date != null) {
				if (time != null) {
					return new DateTime(((DateTime)date).Year, ((DateTime)date).Month, ((DateTime)date).Day, ((DateTime)time).Hour, ((DateTime)time).Minute, 0);
				}
				else {
					return new DateTime(((DateTime)date).Year, ((DateTime)date).Month, ((DateTime)date).Day);
				}
			}
			return null;
		}
		#endregion

		#region SetEnabled
		/// <summary>Enables or disables the input control that represents the
		/// date part of the field value. The field is specified as the type
		/// parameter.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		public static void SetDateEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetDateEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		/// <summary>Enables or disables the input control that represents the
		/// date part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		public static void SetDateEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			foreach(PXDBDateAndTimeAttribute attr in GetAttribute(cache, data, name))
			if (attr != null) attr._isEnabledDate = isEnabled;
		}

		/// <summary>Enables or disables the input control that represents the
		/// time part of the field value. The field is specified as the type
		/// parameter.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		public static void SetTimeEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetTimeEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		/// <summary>Enables or disables the input control that represents the
		/// time part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		public static void SetTimeEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			foreach (PXDBDateAndTimeAttribute attr in GetAttribute(cache, data, name))
				if (attr != null) attr._isEnabledTime = isEnabled;
		}
		#endregion

		#region SetDisplayName
		/// <summary>Sets the display name of the input control that represents
		/// the date part of the field value. The field is specified as the type
		/// parameter.</summary>
		/// <typeparam name="Field">The field.</typeparam>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="displayName">The string to set as the display
		/// name.</param>
		/// <example>
		/// The code below shows the constructor of the <tt>CRTaskMaint</tt> graph,
		/// in which you set new display names for the input control representing
		/// the date values of <tt>StartDate</tt> and <tt>EndDate</tt>.
		/// <code>
		/// public CRTaskMaint()
		///     : base()
		/// {
		///     ...
		///     PXDBDateAndTimeAttribute.SetDateDisplayName&lt;EPActivity.startDate&gt;(Tasks.Cache, null, "Start Date");
		///     PXDBDateAndTimeAttribute.SetDateDisplayName&lt;EPActivity.endDate&gt;(Tasks.Cache, null, "Due Date");
		/// }
		/// </code>
		/// </example>
		public static void SetDateDisplayName<Field>(PXCache cache, object data, string displayName)
			where Field : IBqlField
		{
			SetDateDisplayName(cache, data, cache.GetField(typeof(Field)), displayName);
		}

		/// <summary>Sets the display name of the input control that represents
		/// the date part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="displayName">The string to set as the display
		/// name.</param>
		/// <seealso cref="PXDBDateAndTimeAttribute.SetDateDisplayName{T}"/>
		/// <example>
		/// <code>
		/// PXDBDateAndTimeAttribute.SetDateDisplayName(Tasks.Cache, null, "StartDate", "Start Date");
		/// </code>
		/// </example>
		public static void SetDateDisplayName(PXCache cache, object data, string name, string displayName)
		{
			foreach (PXDBDateAndTimeAttribute attr in GetAttribute(cache, data, name))
				if (attr != null) attr._displayNameDate = displayName;
		}

		/// <summary>Sets the display name of the input control that represents
		/// the time part of the field value. The field is specified as the type
		/// parameter.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="displayName">The string to set as the display
		/// name.</param>
		public static void SetTimeDisplayName<Field>(PXCache cache, object data, string displayName)
			where Field : IBqlField
		{
			SetTimeDisplayName(cache, data, cache.GetField(typeof(Field)), displayName);
		}

		/// <summary>Sets the display name of the input control that represents
		/// the time part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="displayName">The string to set as the display
		/// name.</param>
		public static void SetTimeDisplayName(PXCache cache, object data, string name, string displayName)
		{
			foreach (PXDBDateAndTimeAttribute attr in GetAttribute(cache, data, name))
				if (attr != null) attr._displayNameTime = displayName;
		}
		#endregion

		#region SetVisible
		/// <summary>Makes visible or hides the input control that represents the
		/// data part of the field value. The field is specified as the type
		/// parameter.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isVisible">The value indicating whether the input control
		/// is visible on the user interface.</param>
		public static void SetDateVisible<Field>(PXCache cache, object data, bool isVisible)
			where Field : IBqlField
		{
			SetDateVisible(cache, data, cache.GetField(typeof(Field)), isVisible);
		}

		/// <summary>Makes visible or hides the input control that represents the
		/// data part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="isVisible">The value indicating whether the input control
		/// is visible on the user interface.</param>
		public static void SetDateVisible(PXCache cache, object data, string name, bool isVisible)
		{
			foreach (PXDBDateAndTimeAttribute attr in GetAttribute(cache, data, name))
				if (attr != null) attr._isVisibleDate = isVisible;
		}

		/// <summary>Makes visible or hides the input control that represents the
		/// data part of the field value. The field is specified as the type
		/// parameter.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isVisible">The value indicating whether the input control
		/// is visible on the user interface.</param>
		/// <example>
		/// The code below shows the <tt>RowSelected</tt> event handler, in which you
		/// configure the visibility of the input controls that represent time value
		/// of the <tt>StartDate</tt> and <tt>EndDate</tt> fields.
		/// <code>
		/// protected virtual void EPActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		/// {
		///     ...
		///     var showMinutes = EPSetupCurrent.RequireTimes == true;
		///     PXDBDateAndTimeAttribute.SetTimeVisible&lt;EPActivity.startDate&gt;(cache, row, true);
		///     PXDBDateAndTimeAttribute.SetTimeVisible&lt;EPActivity.endDate&gt;(cache, row, showMinutes &amp;&amp; row.TrackTime == true);
		///     ...
		/// }
		/// </code>
		/// </example>
		public static void SetTimeVisible<Field>(PXCache cache, object data, bool isVisible)
			where Field : IBqlField
		{
			SetTimeVisible(cache, data, cache.GetField(typeof(Field)), isVisible);
		}

		/// <summary>Makes visible or hides the input control that represents the
		/// time part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDBDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="isVisible">The value indicating whether the input control
		/// is visible on the user interface.</param>
		/// <seealso cref="PXDBDateAndTimeAttribute.SetTimeVisible{T}"/>
		/// <example>
		/// <code>
		/// PXDBDateAndTimeAttribute.SetTimeVisible(cache, row, "StartDate", true);
		/// </code>
		/// </example>
		public static void SetTimeVisible(PXCache cache, object data, string name, bool isVisible)
		{
			foreach (PXDBDateAndTimeAttribute attr in GetAttribute(cache, data, name))
				if (attr != null) attr._isVisibleTime = isVisible;
		}
		#endregion
	}
	#endregion

	#region PXDBTimeSpanAttribute
	/// <summary>Maps a DAC field of <tt>int?</tt> type to the <tt>int</tt>
	/// database column. The field value represents a date as a number of
	/// minutes passed from 01/01/1900.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</para>
	/// <para>The field value stores a date as a number of minutes. In the UI,
	/// the string is typically represented by a control allowing a selection
	/// from the list of time values with half-hour interval.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBTimeSpan]
	/// [PXUIField(DisplayName = "Run Time")]
	/// public virtual int? RunTime { get; set; ]
	/// </code>
	/// </example>
	public class PXDBTimeSpanAttribute : PXDBIntAttribute
	{
		#region State
		protected string _InputMask = "HH:mm";
		protected string _DisplayMask = "HH:mm";
		protected new DateTime? _MinValue;
		protected new DateTime? _MaxValue;

		/// <summary>Gets or sets the input mask for date and time values that can
		/// be entered as value of the current field. By default, the proprty
		/// equals <i>HH:mm</i>.</summary>
		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		/// <summary>Gets or sets the display mask for date and time values that
		/// can be entered as value of the current field. By default, the proprty
		/// equals <i>HH:mm</i>.</summary>
		public string DisplayMask
		{
			get
			{
				return _DisplayMask;
			}
			set
			{
				_DisplayMask = value;
			}
		}
		/// <summary>Gets or sets the minimum value for the field. The value
		/// should be a valid string representation of a date.</summary>
		public new string MinValue
		{
			get
			{
				if (_MinValue != null) {
					return _MinValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null) {
					_MinValue = DateTime.Parse(value);
				}
				else {
					_MinValue = null;
				}
			}
		}
		/// <summary>Gets or sets the maximum value for the field. The value
		/// should be a valid string representation of a date.</summary>
		public new string MaxValue
		{
			get
			{
				if (_MaxValue != null) {
					return _MaxValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null) {
					_MaxValue = DateTime.Parse(value);
				}
				else {
					_MaxValue = null;
				}
			}
		}

		/// <summary>
		/// The "00:00" constant.
		/// </summary>
		public const string Zero = "00:00";
		/// <summary>
		/// The BQL constant representing string "00:00".
		/// </summary>
		public sealed class zero : PX.Data.BQL.BqlString.Constant<zero> { public zero() : base(Zero) { } }
		#endregion

		#region Ctor
		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXDBTimeSpanAttribute()
		{
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation
		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, null, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}

			if (e.ReturnValue != null && (e.ReturnValue is int || e.ReturnValue is int?)) {
				TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
				e.ReturnValue = new DateTime(1900, 1, 1).Add(span);
				//e.ReturnValue = new DateTime(1900, 1, 1, span.Hours, span.Minutes, 0);
			}
		}

		/// <exclude/>
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.NewValue is int) {
			}
			else if (e.NewValue is string) {
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val)) {
					TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
					e.NewValue = (int)span.TotalMinutes;
				}
				else {
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime) {
				DateTime val = (DateTime)e.NewValue;
				TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
				e.NewValue = (int)span.TotalMinutes;
			}
		}
		#endregion

		/// <summary>Returns the date obtained by adding the specified number of
		/// minutes to 01/01/1900.</summary>
		/// <param name="minutes">The minutes to add to the default date.</param>
		public static DateTime FromMinutes(int minutes)
		{
			TimeSpan span = new TimeSpan(0, 0, minutes, 0);
			return new DateTime(1900, 1, 1).Add(span);
		}
	}
	#endregion

	#region PXDBImageAttribute
	/// <exclude/>
	public class PXDBImageAttribute : PXDBStringAttribute
	{
		/// <summary>Get, set.</summary>
		public string HeaderImage { get; set; }

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				if (_AutoMask == MaskMode.Auto) {
					_AutoMask = MaskMode.Manual;
					if (sender.Keys.IndexOf(_FieldName) != sender.Keys.Count - 1) {
						_InputMask = null;
					}
				}
				e.ReturnState = PXImageState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, String.IsNullOrEmpty(_InputMask) ? null : _InputMask, null, null, null, null, HeaderImage);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBCreatedDateTimeUtcAttribute
	/// <summary>
	///   <para>This attribute is obsolete. Use <see cref="PXDBCreatedDateTimeAttribute">PXDBCreatedDateTimeAttribute</see> instead.</para>
	///   <para>Maps a DAC field to the database column and automatically sets the field value to the data record's creation UTC date and time.</para>
	/// </summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field data type should be <tt>DateTime?</tt>.</remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXDBCreatedDateTimeUtc]
	/// [PXUIField(DisplayName = "Date Created", Enabled = false)]
	/// public virtual DateTime? CreatedDateTime { get; set; }</code>
	/// </example>
	[Obsolete]
	public class PXDBCreatedDateTimeUtcAttribute : PXDBCreatedDateTimeAttribute
	{
		protected override DateTime GetDate()
		{
			return PXTimeZoneInfo.Now;
		}

		/// <summary>
		/// Initializes a new instance of the <tt>PXDBCreatedDateTimeUtc</tt> attribute.
		/// </summary>
		public PXDBCreatedDateTimeUtcAttribute()
			: base()
		{
			UseTimeZone = true;
		}
	}
	#endregion

	#region PXDBLastModifiedDateTimeUtcAttribute
	/// <summary>
	///   <para>This attribute is obsolete. Use <see cref="PXDBLastModifiedDateTimeAttribute">PXDBLastModifiedDateTimeAttribute Class</see> instead.</para>
	///   <para>Maps a DAC field to the database column and automatically sets the field value to the data record's last modification date and time in UTC.</para>
	/// </summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field data type should be <tt>DateTime?</tt>.</remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXDBLastModifiedDateTimeUtc]
	/// [PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
	/// public virtual DateTime? LastModifiedDateTime { get; set; }</code>
	/// </example>
	[Obsolete]
	public class PXDBLastModifiedDateTimeUtcAttribute : PXDBLastModifiedDateTimeAttribute
	{
		protected override DateTime GetDate()
		{
			return PXTimeZoneInfo.Now;
		}

		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXDBLastModifiedDateTimeUtcAttribute()
		{
			UseTimeZone = true;
		}
	}
	#endregion

	#region PXDBDecimalStringAttribute
	/// <summary>Maps a DAC field of <tt>decimal?</tt> type to the database
	/// column of <tt>decimal</tt> type. The mapped DAC field can be
	/// represented in the UI by a dropdown list using the <see
	/// cref="PXDecimalListAttribute">PXDecimalList</see>
	/// attribute.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field becomes bound to the database column with the same
	/// name.</para>
	/// <para>In the UI, the field can be represented by a drop-down list with
	/// specific values. The UI control is configured using the
	/// <tt>PXDecimalList</tt> attribute.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// // A mapping of the DAC field to the database column
	/// [PXDBDecimalString(1)]
	/// // UI control configuration.
	/// // The first list configures values assigned to the field,
	/// // the second one configures displayed labels.
	/// [PXDecimalList(new string[] { "0.1", "0.5", "1.0", "10", "100" },
	///                new string[] { "0.1", "0.5", "1.0", "10", "100" })]
	/// [PXDefault(TypeCode.Decimal, "0.1")]
	/// [PXUIField(DisplayName = "Invoice Amount Precision")]
	/// public virtual decimal? InvoicePrecision { get; set; }
	/// </code>
	/// </example>
	public class PXDBDecimalStringAttribute : PXDBDecimalAttribute
	{
		/// <summary>Initializes a new instance with the default precision, which
		/// equals 2.</summary>
		public PXDBDecimalStringAttribute()
			: base()
		{
		}

		/// <summary>Initializes a new instance with the given decimal value
		/// precision.</summary>
		public PXDBDecimalStringAttribute(int precision)
			: base(precision)
		{
		}


		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered) {
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 10, false, _FieldName, _IsKey, null, null, null, null, null, null);
			}

			if (e.ReturnValue != null && e.ReturnValue is decimal) {
				e.ReturnValue = ((decimal)e.ReturnValue).ToString("0.00", sender.Graph.Culture);
			}
		}

		/// <exclude/>
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null && e.NewValue is string) {
				decimal val;
				if (decimal.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val)) {
					e.NewValue = val;
				}
				else {
					e.NewValue = null;
				}
			}
		}
	}
	#endregion

	/// <exclude/>
	public class PXDBDateStringAttribute : PXDBDateAttribute
	{
		#region State

		public string DateFormat = "d";

		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 10, false, _FieldName, _IsKey, null, null, null, null, null, null);
			}

			if (e.ReturnValue != null && e.ReturnValue is DateTime)
			{
				e.ReturnValue = ((DateTime)e.ReturnValue).ToString(DateFormat, sender.Graph.Culture);
			}
		}
		#endregion
	}

	#region PXDBLastChangeDateTimeAttribute
	/// <summary>Maps a DAC field to the database column and automatically sets the field value to the last modification date and time (in UTC) of the specified field.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field. The field data type should be <tt>DateTime?</tt>.</remarks>
	/// <example>
	///   <code title="Example" description="In the following code, the modification date and time of the &lt;tt&gt;StatusDate&lt;/tt&gt; field is updated each time &lt;tt&gt;CRCase.status&lt;/tt&gt; is updated." lang="CS">
	/// [PXDBLastChangeDateTime(typeof(CRCase.status))]
	/// public virtual DateTime? StatusDate { get; set; }</code>
	/// </example>
	public class PXDBLastChangeDateTimeAttribute : PXDBDateAttribute, IPXRowUpdatingSubscriber
	{
		/// <exclude/>
		[Serializable]
		public class SelectedValue : IBqlTable
		{
			[PXString(IsKey = true)]
			public virtual string FieldName { get; set; }

			public virtual object Value { get; set; }
		}

		private readonly Type _MonitoredField;

		/// <summary>Initializes a new instance of the attribute that monitors the specified field. On each modification of the monitored field, the attribute updates the
		/// modification date and time of the field that is marked with the attribute.</summary>
		/// <param name="monitoredField">The field to monitor. You specify a type that implements IBqlField.</param>
		public PXDBLastChangeDateTimeAttribute(Type monitoredField)
		{
			UseSmallDateTime = false;
			base.PreserveTime = true;
			base.UseTimeZone = true;

			if (monitoredField == null)
			{
				throw new PXArgumentException(nameof(monitoredField), ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlField).IsAssignableFrom(monitoredField))
			{
				_MonitoredField = monitoredField;
			}
			else
			{
				throw new PXArgumentException(nameof(monitoredField), ErrorMessages.ArgumentException);
			}
		}

		/// <exclude />
		public DateTime GetDate()
		{
			DateTime? serverDate = PXTransactionScope.GetServerDateTime(true);
			return serverDate.HasValue
				? PXTimeZoneInfo.ConvertTimeFromUtc(serverDate.Value, GetTimeZone())
				: PXTimeZoneInfo.Now;
		}

		/// <exclude />
		public override bool UseTimeZone
		{
			get { return base.UseTimeZone; }
			set { }
		}

		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (sender.GetValue(e.NewRow, _FieldOrdinal) == null) sender.SetValue(e.NewRow, _FieldOrdinal, GetDate());
		}


		/// <exclude />
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);
			if ((e.Operation & PXDBOperation.Update) == PXDBOperation.Update ||
				(e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert)
			{
				e.DataLength = 8;
				e.IsRestriction = e.IsRestriction || _IsKey;
				PrepareFieldName(_DatabaseFieldName, e);

				var origValue = sender.GetValueOriginal(e.Row, _MonitoredField.Name);
				if (!Equals(origValue, sender.GetValue(e.Row, _MonitoredField.Name)))
				{
					e.DataValue = UseTimeZone ? e.SqlDialect.GetUtcDate : e.SqlDialect.GetDate;
					e.DataType = PXDbType.DirectExpression;
					sender.SetValue(e.Row, _FieldOrdinal, GetDate());
				}
			}
		}
	}
	#endregion

	#region PXDBRevision
	/// <exclude/>
	public class PXDBRevision : PXDBIntAttribute, IPXRowUpdatingSubscriber
	{
		private readonly Type _MonitoredField;

		public PXDBRevision(Type monitoredField)
		{
			if (monitoredField == null) {
				throw new PXArgumentException(nameof(monitoredField), ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlField).IsAssignableFrom(monitoredField)) {
				_MonitoredField = monitoredField;
			}
			else {
				throw new PXArgumentException(nameof(monitoredField), ErrorMessages.ArgumentException);
			}
		}


		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (sender.GetValue(e.NewRow, _FieldOrdinal) == null) sender.SetValue(e.NewRow, _FieldOrdinal, 0);
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			if ((e.Operation & PXDBOperation.Update) == PXDBOperation.Update ||
				(e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert) {
				e.DataLength = 4;
				e.IsRestriction = e.IsRestriction || _IsKey;
				PrepareFieldName(_DatabaseFieldName, e);

				e.DataType = PXDbType.Int;

				int? revision = (int?)sender.GetValue(e.Row, _FieldOrdinal);
				var origValue = sender.GetValueOriginal(e.Row, _MonitoredField.Name);
				if (origValue != null && !Equals(origValue, sender.GetValue(e.Row, _MonitoredField.Name))) {
					e.DataValue = revision + 1;
					sender.SetValue(e.Row, _FieldOrdinal, revision + 1);
				}
				else {
					e.DataValue = revision;
				}
			}
		}
	}
	#endregion

	#region UserAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXUserAttribute : PXEventSubscriberAttribute
	{
		public PXUserAttribute() : base() { }

		public static Dictionary<Type, List<string>> FieldList = new Dictionary<Type, List<string>>();

		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)FieldList).SyncRoot)
			{
				List<string> list;
				if (!FieldList.TryGetValue(bqlTable, out list))
				{
					FieldList[bqlTable] = list = new List<string>();
				}
				if (!list.Contains(base.FieldName))
				{
					list.Add(base.FieldName);
				}
			}
		}
		/// <exclude/>
		public static List<string> GetFields(Type table)
		{
			if (ServiceManager.EnsureCachesInstatiated(true))
			{
				List<string> list;
				if (FieldList.TryGetValue(table, out list))
				{
					return list;
				}
			}
			return new List<string>();
		}
	}
	#endregion

	#region PhoneAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXPhoneAttribute : PXEventSubscriberAttribute
	{
		public PXPhoneAttribute() : base() { }

		public static Dictionary<Type, List<string>> FieldList = new Dictionary<Type, List<string>>();

		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)FieldList).SyncRoot)
			{
				List<string> list;
				if (!FieldList.TryGetValue(bqlTable, out list))
				{
					FieldList[bqlTable] = list = new List<string>();
				}
				if (!list.Contains(base.FieldName))
				{
					list.Add(base.FieldName);
				}
			}
		}
		/// <exclude/>
		public static List<string> GetFields(Type table)
		{
			if (ServiceManager.EnsureCachesInstatiated(true))
			{
				List<string> list;
				if (FieldList.TryGetValue(table, out list))
				{
					return list;
				}
			}
			return new List<string>();
		}
	}
	#endregion
}
