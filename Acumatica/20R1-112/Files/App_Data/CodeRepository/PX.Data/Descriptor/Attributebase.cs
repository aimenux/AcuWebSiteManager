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
using System.Monads;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using PX.Data.PushNotifications;
using PX.DbServices;
using PX.Data.SQLTree;
using PX.SM;
using static PX.Data.DependencyInjection.InjectMethods;

namespace PX.Data
{
	[System.Diagnostics.DebuggerDisplay("{ToString()}")]
	public abstract class PXEventSubscriberAttribute : Attribute
	{
		internal PXGraphExtension[] Extensions;
		
		protected PXGraphExtension[] GraphExtensions
		{
			get { return Extensions; }
		}



		public override int GetHashCode()
		{
			return _FieldOrdinal;
		}
		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}
		protected Type _BqlTable = null;
		protected string _FieldName = null;
		protected int _FieldOrdinal = -1;
		internal int IndexInClonesArray = -1;
		public bool IsDirty;
		internal PXEventSubscriberAttribute Prototype;

		protected PXEventSubscriberAttribute()
		{
			PXExtensionManager.InitExtensions(this);
		}



		protected PXAttributeLevel _AttributeLevel = PXAttributeLevel.Type;

		public PXAttributeLevel AttributeLevel
		{
			get
			{
				return _AttributeLevel;
			}
		}
		public virtual PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			//if (!IsMutable && attributeLevel == PXAttributeLevel.Item)
			//	return this;

			PXEventSubscriberAttribute attr = (PXEventSubscriberAttribute)MemberwiseClone();
			attr._AttributeLevel = attributeLevel;
			return attr;
		}
		public virtual Type BqlTable
		{
			get
			{
				return _BqlTable;
			}
			set
			{
				if (_BqlTable != null && _AttributeLevel != PXAttributeLevel.Type)
				{
					throw new PXException(ErrorMessages.BaseAttributeStateOverride);
				}
				_BqlTable = value;
			}
		}

		public virtual Type CacheExtensionType { get; set; }

		protected internal virtual void SetBqlTable(Type bqlTable)
		{
			if (!typeof(PXCacheExtension).IsAssignableFrom(bqlTable))
			{
				_BqlTable = bqlTable;
				Type baseType;
				while ((typeof(IBqlTable).IsAssignableFrom(baseType = _BqlTable.BaseType)
					|| baseType.IsDefined(typeof(PXTableAttribute), false)
					|| baseType.IsDefined(typeof(PXTableNameAttribute), false) && ((PXTableNameAttribute)baseType.GetCustomAttributes(typeof(PXTableNameAttribute), false)[0]).IsActive)
					&& ((_FieldName != null
					&& baseType.GetProperty(_FieldName) != null
					|| !_BqlTable.IsDefined(typeof(PXTableAttribute), false))
					&& (!_BqlTable.IsDefined(typeof(PXTableNameAttribute), false) || !((PXTableNameAttribute)_BqlTable.GetCustomAttributes(typeof(PXTableNameAttribute), false)[0]).IsActive)))
				{
					_BqlTable = baseType;
				}
			}
			else
			{
				_BqlTable = bqlTable;
			}
		}
		public virtual string FieldName
		{
			get
			{
				return _FieldName;
			}
			set
			{
				if (_AttributeLevel != PXAttributeLevel.Type)
				{
					throw new PXException(ErrorMessages.BaseAttributeStateOverride);
				}
				_FieldName = value;
			}
		}
		public virtual int FieldOrdinal
		{
			get
			{
				return _FieldOrdinal;
			}
			set
			{
				if (_AttributeLevel != PXAttributeLevel.Type)
				{
					throw new PXException(ErrorMessages.BaseAttributeStateOverride);
				}
				_FieldOrdinal = value;
			}
		}

		/// <summary>
		/// Injects dependencies of a certain level inside the attribute's instance using registered <see cref="IDependencyInjector">dependency injector</see>.
		/// </summary>
		/// <param name="cache"><see cref="PXCache"/> instance that is used to resolve
		/// <see cref="PXCache"/>- and <see cref="PXGraph"/>-bound parameters during the injection into cache-level attributes;
		/// <see langword="null"/> for dependencies that are injected into type-level attributes.</param>
		internal virtual void InjectAttributeDependencies(PXCache cache)
		{
            InjectDependencies(this, cache);
		}

	    [MethodImpl(MethodImplOptions.NoInlining)]
	    public void InvokeCacheAttached(PXCache cache)
	    {
		    InjectAttributeDependencies(cache);

		    if (this is PXAggregateAttribute)
		    {
			    this.CacheAttached(cache);
		    }
		    else
		    {
			    using (new PXPerformanceInfoTimerScope(info => info.TmCacheAttached))
			    {
				    this.CacheAttached(cache);
			    }
		    }
	    }

	    public virtual void CacheAttached(PXCache sender)
		{
		}
		public virtual void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
			where ISubscriber : class
		{
			if (this is ISubscriber)
			{
				subscribers.Add(this as ISubscriber);
			}
		}

		public static T CreateInstance<T>( params object[] constructorArgs)
			where T: PXEventSubscriberAttribute
		{
			return (T) CreateInstance(typeof (T), constructorArgs);
		}

		public static PXEventSubscriberAttribute CreateInstance(Type t, params object[] constructorArgs)
		{
			t = PXExtensionManager.GetWrapperType(t);
			if (constructorArgs == null || constructorArgs.Length == 0)
				return (PXEventSubscriberAttribute)Activator.CreateInstance(t);

			var c = t.GetConstructor(constructorArgs.Select(_ => _.GetType()).ToArray());
			return (PXEventSubscriberAttribute)c.Invoke(constructorArgs);
		}


		protected internal class ObjectRef<T>
		{
			public T Value;

			public ObjectRef()
			{
			}

			public ObjectRef(T defaultValue)
			{
				Value = defaultValue;
			}
		}

		public override string ToString()
		{
			if (BqlTable == null || FieldName == null)
				return base.ToString();
			else
				return base.ToString() + $" on {BqlTable.FullName}+{FieldName}";
		}
	}

	public interface IPXAttributeList : IEnumerable<PXEventSubscriberAttribute>, IDisposable
	{
		
	}
	public static class PXEventSubscriberAttributeExtensions
	{
		internal static void prepare(this PXEventSubscriberAttribute attr, string fieldName, int fieldOrdinal, Type itemType)
		{
			attr.FieldName = fieldName;
			attr.FieldOrdinal = (int)fieldOrdinal;
			if (attr.BqlTable == null) {
				attr.SetBqlTable(itemType);
			}
		}

		internal static void prepare(this PXEventSubscriberAttribute attr, string fieldName, int fieldOrdinal, Type itemType, Type extensionType)
		{
			attr.FieldName = fieldName;
			attr.FieldOrdinal = (int)fieldOrdinal;
			attr.CacheExtensionType = extensionType;
			if (attr.BqlTable == null)
			{
				attr.SetBqlTable(itemType);
			}
		}
	}

    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>CommandPreparing</tt> event.
    /// </summary>
    public interface IPXCommandPreparingSubscriber
	{
		void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowSelecting</tt> event.
    /// </summary>
    public interface IPXRowSelectingSubscriber
	{
		void RowSelecting(PXCache sender, PXRowSelectingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowSelected</tt> event.
    /// </summary>
    public interface IPXRowSelectedSubscriber
	{
		void RowSelected(PXCache sender, PXRowSelectedEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowInserting</tt> event.
    /// </summary>
	public interface IPXRowInsertingSubscriber
	{
		void RowInserting(PXCache sender, PXRowInsertingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowInserted</tt> event.
    /// </summary>
	public interface IPXRowInsertedSubscriber
	{
		void RowInserted(PXCache sender, PXRowInsertedEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowUpdating</tt> event.
    /// </summary>
	public interface IPXRowUpdatingSubscriber
	{
		void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowUpdated</tt> event.
    /// </summary>
	public interface IPXRowUpdatedSubscriber
	{
		void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowDeleting</tt> event.
    /// </summary>
	public interface IPXRowDeletingSubscriber
	{
		void RowDeleting(PXCache sender, PXRowDeletingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowDeleted</tt> event.
    /// </summary>
	public interface IPXRowDeletedSubscriber
	{
		void RowDeleted(PXCache sender, PXRowDeletedEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowPersisting</tt> event.
    /// </summary>
    public interface IPXRowPersistingSubscriber
	{
		void RowPersisting(PXCache sender, PXRowPersistingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>RowPersisted</tt> event.
    /// </summary>
    public interface IPXRowPersistedSubscriber
	{
		void RowPersisted(PXCache sender, PXRowPersistedEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>FieldDefaulting</tt> event.
    /// </summary>
	public interface IPXFieldDefaultingSubscriber
	{
		void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>FieldUpdating</tt> event.
    /// </summary>
	public interface IPXFieldUpdatingSubscriber
	{
		void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>FieldVerifying</tt> event.
    /// </summary>
	public interface IPXFieldVerifyingSubscriber
	{
		void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>FieldUpdated</tt> event.
    /// </summary>
	public interface IPXFieldUpdatedSubscriber
	{
		void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implements this interface subscribes
    /// to the <tt>FieldSelecting</tt> event.
    /// </summary>
	public interface IPXFieldSelectingSubscriber
	{
		void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e);
	}
    /// <summary>
    /// Indicates that an attribute that implemets this interface subscribes
    /// to the <tt>ExceptionHandling</tt> event.
    /// </summary>
	public interface IPXExceptionHandlingSubscriber
	{
		void ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e);
	}
	public interface IPXInterfaceField
	{
		string DisplayName
		{
			get;
			set;
		}
		PXUIVisibility Visibility
		{
			get;
			set;
		}
		bool Enabled
		{
			get;
			set;
		}
		bool Visible
		{
			get;
			set;
		}
		string ErrorText
		{
			get;
			set;
		}
		object ErrorValue
		{
			get;
			set;
		}
		PXErrorLevel ErrorLevel
		{
			get;
			set;
		}
		int TabOrder
		{
			get;
			set;
		}
		PXCacheRights MapEnableRights
		{
			get;
			set;
		}
		PXCacheRights MapViewRights
		{
			get;
			set;
		}
		bool ViewRights
		{
			get;
		}
		void ForceEnabled();
	}

	public interface IPXDependsOnFields
	{
		ISet<Type> GetDependencies(PXCache cache);
	}

	public interface IPXIdentityColumn {
		object GetLastInsertedIdentity(object valueFromRow);
	}

	/// <summary>The base class for classes that combine multiple attributes
	/// into one. By creating a derived class, you can reuse the same set of attributes
	/// with similar properties of different classes.</summary>
	/// <remarks>Some DACs contain the same fields, which have the same attributes.
	/// You can combine multiple attributes into one by creating a class
	/// derived from <tt>PXAggregateAttribute</tt> and assigning these attributes
	/// to the created class.</remarks>
	/// <example>
	/// <code description="The original definitions of the &lt;tt&gt;ARInvoice&lt;/tt&gt; and &lt;tt&gt;SOOrder&lt;/tt&gt;
	/// classes contain long definitions of the &lt;tt&gt;CustomerID&lt;/tt&gt; fields." lang="CS">
	/// public class ARInvoice : IBqlTable
	/// {
	///     [PXDBInt()]
	///     [PXUIField(DisplayName = "Customer")]
	///     [PXDefault()]
	///     [PXSelector(typeof(Search&lt;BAccountR.bAccountID,
	///         Where&lt;BAccountR.status, Equal&lt;BAccount.status.active&gt;&gt;&gt;))]
	///     public override Int32? CustomerID
	/// }
	/// public class SOOrder : IBqlTable
	/// {
	///     [PXDBInt()]
	///     [PXUIField(DisplayName = "Customer")]
	///     [PXDefault()]
	///     [PXSelector(typeof(Search&lt;BAccountR.bAccountID,
	///         Where&lt;BAccountR.status, Equal&lt;BAccount.status.active&gt;&gt;&gt;))]
	///     public override Int32? CustomerID
	/// }</code>
	/// <code description="A set of attributes is combined into a single class as follows." lang="CS">
	/// [PXDBInt()]
	/// [PXUIField(DisplayName = "Customer")]
	/// [PXDefault()]
	/// [PXSelector(typeof(Search&lt;BAccountR.bAccountID,
	///     Where&lt;BAccountR.status, Equal&lt;BAccount.status.active&gt;&gt;&gt;))]
	/// public class CustomerActiveAttribute : PXAggregateAttribute { }
	/// 
	/// public class ARInvoice : IBqlTable
	/// {
	///     [CustomerActive]
	///     public override Int32? CustomerID { get; set; }
	/// }
	/// 
	/// public class SOOrder : IBqlTable
	/// {
	///     [CustomerActive]
	///     public override Int32? CustomerID { get; set; }
	/// }
	/// </code></example>
	public class PXAggregateAttribute : PXEventSubscriberAttribute
	{
		protected class AggregatedAttributesCollection : List<PXEventSubscriberAttribute>
		{
			private PXEventSubscriberAttribute[] _AggregatedAttributes = null;

			public AggregatedAttributesCollection() : base()
			{ }

			public AggregatedAttributesCollection(int capacity) : base(capacity)
			{ }

			public AggregatedAttributesCollection(IEnumerable<PXEventSubscriberAttribute> collection) : base(collection)
			{ }

			public new virtual PXEventSubscriberAttribute this[int index]
			{
				get
				{
					return base[index];
				}
				set
				{
					base[index] = value;
					_AggregatedAttributes = null;
				}
			}

			public new virtual void Add(PXEventSubscriberAttribute item)
			{
				base.Add(item);
				_AggregatedAttributes = null;
			}
			public new virtual void AddRange(IEnumerable<PXEventSubscriberAttribute> collection)
			{
				base.AddRange(collection);
				_AggregatedAttributes = null;
			}
			public new virtual void Remove(PXEventSubscriberAttribute item)
			{
				base.Remove(item);
				_AggregatedAttributes = null;
			}
			public new virtual void RemoveAt(int index)
			{
				base.RemoveAt(index);
				_AggregatedAttributes = null;
			}

			public PXEventSubscriberAttribute[] AggregatedAttributes
			{
				get
				{
					if (_AggregatedAttributes == null)
					{
						BuildAggregatedAttributes();
					}
					return _AggregatedAttributes;
				}
			}

			internal void BuildAggregatedAttributes()
			{
				var aggrAttributes = new List<PXEventSubscriberAttribute>();
				foreach (PXEventSubscriberAttribute embeded in this)
				{
					aggrAttributes.Add(embeded);
					var aggrAttr = embeded as PXAggregateAttribute;
					if (aggrAttr != null)
						aggrAttributes.AddRange(aggrAttr.GetAggregatedAttributes());
				}
				_AggregatedAttributes = aggrAttributes.ToArray();
			}

			public new void Clear()
			{
				base.Clear();
				_AggregatedAttributes = null;
			}
		}

        /// <summary>The collection of the attributes combined in the current attribute.</summary>
        protected AggregatedAttributesCollection _Attributes;

        internal List<PXEventSubscriberAttribute> InternalAttributesAccessor
	    {
		    get { return _Attributes; }
	    } 
        /// <summary>
        /// Initializes a new instance of the attribute; pulls the
        /// <tt>PXEventSubscriberAttribute</tt>-derived attributes placed on the
        /// current attribute and adds them to the collection of aggregated attributes.
        /// </summary>
		public PXAggregateAttribute()
		{
			_Attributes = new AggregatedAttributesCollection();
			foreach(PXEventSubscriberAttribute attr in this.GetType().GetCustomAttributes(typeof(PXEventSubscriberAttribute), true))
			{
				_Attributes.Add(attr);
			}
		}
        /// <exclude/>
		public PXEventSubscriberAttribute[] GetAttributes()
		{
			return _Attributes.ToArray();
		}
        /// <exclude />
        public PXEventSubscriberAttribute[] GetAggregatedAttributes()
        {
            return _Attributes.AggregatedAttributes;
        }

        /// <exclude/>
		public T GetAttribute<T>() where T : class
		{
			foreach (PXEventSubscriberAttribute attr in _Attributes)
				if (attr is T)
					return attr as T;
			return null;
		}
        /// <exclude/>
		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			PXAggregateAttribute attr = (PXAggregateAttribute)base.Clone(attributeLevel);



	        if (attributeLevel == PXAttributeLevel.Item)
	        {
				_Attributes = new AggregatedAttributesCollection(_Attributes);
		        return attr;
	        }

			attr._Attributes = new AggregatedAttributesCollection(_Attributes.Count);

			foreach (PXEventSubscriberAttribute subscr in _Attributes)
			{
				attr._Attributes.Add(subscr.Clone(attributeLevel));
			}
			return attr;
		}
		/// <exclude/>
		internal override void InjectAttributeDependencies(PXCache cache)
		{
			base.InjectAttributeDependencies(cache);
            foreach (PXEventSubscriberAttribute subscr in _Attributes)
            {
                subscr.InjectAttributeDependencies(cache);
            }
		}
        /// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			foreach (PXEventSubscriberAttribute subscr in _Attributes)
			{
				subscr.CacheAttached(sender);
			}
		}
        /// <summary>Gets or sets the DAC associated with the attribute. The
        /// setter also sets the provided value as <tt>BqlTable</tt> in all
        /// attributes combined in the current attribute.</summary>
		public override Type BqlTable
		{
			get
			{
				return base.BqlTable;
			}
			set
			{
				base.BqlTable = value;
				foreach (PXEventSubscriberAttribute subscr in _Attributes)
				{
					subscr.BqlTable = value;
				}
			}
		}
        /// <exclude />
        protected internal override void SetBqlTable(Type bqlTable)
        {
            base.SetBqlTable(bqlTable);
            foreach (PXEventSubscriberAttribute subscr in _Attributes)
            {
                subscr.SetBqlTable(bqlTable);
            }
        }
        /// <summary>Gets or sets the name of the field associated with the attribute. The setter also sets the provided value as <tt>FieldName</tt> in all attributes combined in
        /// the current attribute.</summary>
        public override string FieldName
        {
            get
            {
                return base.FieldName;
            }
            set
            {
                base.FieldName = value;
                foreach (PXEventSubscriberAttribute subscr in _Attributes)
                {
                    subscr.FieldName = value;
                }
            }
        }
        /// <summary>Gets or sets the index of the field associtated with the
        /// attribute. The setter also sets the provided value as
        /// <tt>FieldOrdinal</tt> in all attributes combined in the current
        /// attribute.</summary>
		public override int FieldOrdinal
		{
			get
			{
				return base.FieldOrdinal;
			}
			set
			{
				base.FieldOrdinal = value;
				foreach (PXEventSubscriberAttribute subscr in _Attributes)
				{
					subscr.FieldOrdinal = value;
				}
			}
		}
        /// <exclude/>
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			foreach (PXEventSubscriberAttribute subscr in _Attributes)
			{
				subscr.GetSubscriber<ISubscriber>(subscribers);
			}
		}
	}

	public abstract class PXDBInterceptorAttribute : Attribute
	{
		protected bool tableMeet(PXCommandPreparingEventArgs.FieldDescription description, Type table, ISqlDialect dialect)
		{
			var st = (description.Expr as SQLTree.Column)?.Table() as SimpleTable;
			if (st == null) return false;
			return (st.AliasOrName()) == table.Name;
		}
        protected bool fieldMeet(string databaseFieldName, string fieldName, ISqlDialect dialect)
        {
            return databaseFieldName.EndsWith("." + fieldName) || databaseFieldName.EndsWith("." + dialect.quoteDbIdentifier(fieldName));
        }
        protected static object[] getKeys(PXCache sender, object node)
		{
			object[] ret = new object[sender.Keys.Count];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = sender.GetValue(node, sender.Keys[i]);
			}
			return ret;
		}
		public PXDBInterceptorAttribute Child
		{
			get;
			set;
		}
		internal virtual List<string> Keys
		{
			get
			{
				return null;
			}
		}
		public abstract BqlCommand GetRowCommand();

	    public virtual BqlCommand GetRowByNoteIdCommand()
	    {
	        throw new NotSupportedException("Table does not support select by noteid");
	    }
		public abstract BqlCommand GetTableCommand();

	    public virtual bool CanSelectByNoteId
	    {
	        get { return false; }
	    }
		internal virtual BqlCommand GetTableCommand(PXCache sender)
		{
			return GetTableCommand();
		}
		public virtual void CacheAttached(PXCache sender)
		{
		}

		public virtual Type[] GetTables()
		{
			BqlCommand cmd = GetTableCommand();
			return cmd == null ? new Type[0] : cmd.GetTables();
		}

		public virtual bool PersistInserted(PXCache sender, object row)
		{
			return false;
		}
		public virtual bool PersistUpdated(PXCache sender, object row)
		{
			return false;
		}
		public virtual bool PersistDeleted(PXCache sender, object row)
		{
			return false;
		}
        public virtual object Insert(PXCache sender, object row)
        {
            return sender.Insert(row, true);
        }
        public virtual object Update(PXCache sender, object row)
        {
            return sender.Update(row, true);
        }
        public virtual object Delete(PXCache sender, object row)
        {
            return sender.Delete(row, true);
        }
		public virtual bool CacheSelected
		{
			get
			{
				return true;
			}
		}
		public virtual PXDBInterceptorAttribute Clone()
		{
			PXDBInterceptorAttribute attr = (PXDBInterceptorAttribute)MemberwiseClone();
			return attr;
		}

		protected static PXLockViolationException GetLockViolationException(Type table, PXDataFieldParam[] parameters, PXDBOperation operation)
		{
			var deletedDatabaseRecord = PXDatabase.IsDeletedDatabaseRecord(table, parameters, out var keyValues);

			return new PXLockViolationException(table, operation, keyValues, deletedDatabaseRecord);
		}
	}

	public interface IPXExtensibleTableAttribute
	{
		string[] Keys
		{
			get;
		}
	}

    /// <summary>Binds a DAC that derives from another DAC to the table having
    /// the name of the derived DAC. Without the attribute, the derived DAC
    /// will be bound to the same table as the DAC that starts the inheritance
    /// hierarchy.</summary>
    /// <remarks>
    /// 	<para>The attribute is placed on the declaration of a DAC.</para>
    /// 	<para>The attribute can be used in customizations. You place it on the
    /// declaration of a DAC extension to indicate that the extension fields
    /// are bound to a separate table.</para>
    /// </remarks>
    /// <example>
    /// 	<code title="Example" description="The PXTable attribute below indicates that the APInvoice DAC is bound to the APInvoice table. Without the attribute, it would be bound to the APRegister table." lang="CS">
    /// [System.SerializableAttribute()]
    /// [PXTable()]
    /// public partial class APInvoice : APRegister, IInvoice
    /// {
    ///     ...
    /// }</code>
    /// 	<code title="Example2" description="The PXTable attribute below indicates that the FSxLocation extension of the Location DAC is bound to a separate table and the Location DAC can include data records that do not have the corresponding data records in the extension table. Here, you specify the key fields of the Location DAC, because it includes a surrogate-natural pair of key fields, LocationID (which is the database key as well) and LocationCD (human-readable value). In the PXTable attribute, you specify the surrogate LocationID field.
    /// " groupname="Example" lang="CS">
    /// [PXTable(typeof(Location.bAccountID),
    ///          typeof(Location.locationID),
    ///          IsOptional = true)]
    /// public class FSxLocation : PXCacheExtension&lt;Location&gt;
    /// {
    ///     ...
    /// }</code>
    /// </example>
	[AttributeUsage(AttributeTargets.Class)]
	public class PXTableAttribute : PXDBInterceptorAttribute
	{
		protected BqlCommand rowselection;
		protected BqlCommand tableselection;
	    protected BqlCommand rowSelectionByNoteId;
	    protected bool canSelectByNoteId = false;

		protected List<string> keys;
		protected Type[] _bypassOnDelete; // tables that should be bypassed on delete operation
        /// <summary>Gets or sets the value that indicates whether the base DAC
        /// data record can exist without the extension DAC data record. This
        /// situation corresponds to the use of the attribute on the extension DAC
        /// that is bound to a separate database table. By default, the value is
        /// <tt>false</tt>, and the data record in the extension table is always
        /// created for a data record of the base table.</summary>
		public bool IsOptional
		{
			get;
			set;
		}
        /// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			rowselection = new BqlRowSelection(sender, true, keys, this);
			tableselection = new BqlRowSelection(sender, false, keys, this);
            var noteIdField = sender.GetBqlField("noteid");
            if (noteIdField != null)
            {
                rowSelectionByNoteId = new BqlRowSelection(sender, true, keys, this) { ByNoteId = true };
                canSelectByNoteId = true;
            }
            

			if (keys != null && keys.Count == 1 && sender.Keys.Count == 1)
			{
				sender.CommandPreparingEvents[sender.Keys[0].ToLower()] += KeysCommandPreparing;
			}
		}
        /// <exclude/>
		public override BqlCommand GetRowCommand()
		{
			return rowselection;
		}

        /// <exclude />
	    public override BqlCommand GetRowByNoteIdCommand()
	    {
	        return rowSelectionByNoteId;
	    }

        /// <exclude/>
		public override BqlCommand GetTableCommand()
		{
			return tableselection;
		}

        public override bool CanSelectByNoteId
        {
            get
            {
                return canSelectByNoteId;
            }
        }

		internal override BqlCommand GetTableCommand(PXCache sender)
		{
			if (tableselection == null)
			{
				rowselection = new BqlRowSelection(sender, true, keys, this);
				tableselection = new BqlRowSelection(sender, false, keys, this);
			}
			return tableselection;
		}
        /// <exclude/>
		public virtual void KeysCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Update || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) && 
				e.Table != null && e.Row != null
				)
			{
				Type[] tables = GetTables();
				if (tables[tables.Length - 1] != e.Table &&
					object.Equals(sender.GetValue(e.Row, sender.Keys[0]), e.Value)
					)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(keys[0], e.Row, sender.GetValue(e.Row, keys[0]), PXDBOperation.Select, e.Table, out description);
					if (description != null && description.Expr != null)
					{
						e.BqlTable = description.BqlTable;
						e.Expr = description.Expr;
						e.DataType = description.DataType;
						e.DataLength = description.DataLength;
						e.DataValue = description.DataValue;
						e.IsRestriction = true;
						e.Cancel = true;
					}
				}
			}
		}
        /// <summary>Initializes a new instance of the attribute.</summary>
		public PXTableAttribute()
		{
		}
        /// <summary>Initializes a new instance of the attribute when the base DAC
        /// has a pair of surrogate and natural keys. In this case, in the
        /// parameters, you should specify all key fields of the base DAC. From
        /// the pair of the surrogate and natural keys, you include only the
        /// surrogate key.</summary>
        /// <param name="links">The list of key fields of the base DAC.</param>
        /// <example>
        /// <para>The <tt>PXTable</tt> attribute below indicates that the <tt>FSxLocation</tt> extension
        /// of the <tt>Location</tt> DAC is bound to a separate table and the <tt>Location</tt> DAC
        /// can include data records that do not have the corresponding data record
        /// in the extension table.</para>
        /// <para>You specify the key fields of the <tt>Location</tt> DAC, because it
        /// includes a surrogate-natural pair of key fields, <tt>LocationID</tt> (which is the
        /// database key as well) and <tt>LocationCD</tt> (human-readable value). In the
        /// <tt>PXTable</tt> attribute, you specify the surrogate
        /// <tt>LocationID</tt> field.</para>
        /// <code>
        /// [PXTable(typeof(Location.bAccountID),
        ///          typeof(Location.locationID),
        ///          IsOptional = true)]
        /// public class FSxLocation : PXCacheExtension&lt;Location&gt;
        /// {
        ///     ...
        /// }
        /// </code>
        /// </example>
		public PXTableAttribute(params Type[] links)
			: this()
		{
			keys = new List<string>();
			foreach (Type key in links)
			{
				keys.Add(char.ToUpper(key.Name[0]) + key.Name.Substring(1));
			}
		}

        /// <param name="tables">Tables that should be bypassed on delete operation.</param>
        /// <exclude/>
		public void BypassOnDelete(params Type[] tables)
		{
			_bypassOnDelete = tables;
		}

		internal override List<string> Keys
		{
			get
			{
				return keys;
			}
		}
        /// <exclude/>
		public override bool PersistInserted(PXCache sender, object row)
		{
			ISqlDialect dialect = sender.Graph.SqlDialect;
			Type[] tables = GetTables();
			List<Type> extensions = sender.GetExtensionTables();
			if (extensions != null)
			{
				extensions = new List<Type>(extensions);
				extensions.AddRange(tables);
				tables = extensions.ToArray();
			}
			if (keys == null)
			{
				keys = new List<string>(sender.Keys);
			}
			List<PXDataFieldAssign>[] pars = new List<PXDataFieldAssign>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldAssign>();
			}
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			PrepareParametersForInsert(sender, row, tables, dialect, audit, pars);
            try
			{
				pars[tables.Length - 1].Add(PXDataFieldAssign.OperationSwitchAllowed);
				sender.Graph.ProviderInsert(tables[tables.Length - 1], pars[tables.Length - 1].ToArray());
			}
			catch (PXDbOperationSwitchRequiredException)
			{
				List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
				foreach (string field in sender.Fields)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
					if (description?.Expr != null && !description.IsExcludedFromUpdate)
					{
						if (tableMeet(description, tables[tables.Length - 1], dialect))
						{
							if (description.IsRestriction)
							{
								upd.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
							else
							{
								upd.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
				}
				sender.Graph.ProviderUpdate(tables[tables.Length - 1], upd.ToArray());
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					BqlTablePair pair;
					if (sender._Originals.TryGetValue((IBqlTable)row, out pair))
					{
						if (sender._OriginalsRemoved == null)
						{
							sender._OriginalsRemoved = new PXCacheOriginalCollection();
						}
						sender._OriginalsRemoved[(IBqlTable)row] = pair;
					}
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			for (int j = 0; j < tables.Length - 1; j++)
			{
				foreach (string field in keys)
				{
					object val = sender.GetValue(row, field);
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, tables[j], out description);
					if (description == null || description.Expr == null)
					{
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[j], out description);
					}
					if (description?.Expr != null && !description.IsExcludedFromUpdate)
					{
						PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, null);
						if (audit && val != null)
						{
							assign.IsChanged = true;
							assign.NewValue = sender.ValueToString(field, val, description.DataValue);
						}
						else assign.IsChanged = false;
						pars[j].Add(assign);
					}
				}
			}
			for (int i = tables.Length - 2; i >= 0; i--)
			{
				try
				{
					pars[i].Add(PXDataFieldAssign.OperationSwitchAllowed);
					sender.Graph.ProviderInsert(tables[i], pars[i].ToArray());
				}
				catch (PXDbOperationSwitchRequiredException)
				{
					List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
					foreach (string field in sender.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
						if (description?.Expr != null && !description.IsExcludedFromUpdate)
						{
							if (tableMeet(description, tables[i], dialect))
							{
								if (description.IsRestriction)
								{
									upd.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
								}
								else
								{
									upd.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
					}
					foreach (string field in keys)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
						if (description == null || description.Expr == null)
						{
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
						}
						if (description != null && description.Expr != null)
						{
							upd.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
						}
					}
					sender.Graph.ProviderUpdate(tables[i], upd.ToArray());
				}
			}
			return true;
		}

        private void PrepareParametersForInsert(PXCache sender, object row, Type[] tables, ISqlDialect dialect, bool audit,
            List<PXDataFieldAssign>[] pars)
        {
            bool noteIDRequred = sender._HasKeyValueStored();
            foreach (string field in sender.Fields)
            {
                object val = sender.GetValue(row, field);
                PXCommandPreparingEventArgs.FieldDescription description;
                sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
                if (description?.Expr != null && !description.IsExcludedFromUpdate)
                {
                    for (int j = 0; j < tables.Length; j++)
                    {
                        if (tableMeet(description, tables[j], dialect))
                        {
                            PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType,
                                description.DataLength, description.DataValue, null);
                            if (audit && val != null)
                            {
                                assign.IsChanged = true;
                                assign.NewValue = sender.ValueToString(field, val, description.DataValue);
                            }
                            else assign.IsChanged = false;
                            if (noteIDRequred && String.Equals(field, sender._NoteIDName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (assign.Value == null)
                                {
                                    assign.Value = SequentialGuid.Generate();
                                    sender.SetValue(row, (int)sender._NoteIDOrdinal, assign.Value);
                                }
                                PXDataFieldAssign n = new PXDataFieldAssign(sender._NoteIDName, PXDbType.UniqueIdentifier, 16,
                                    assign.Value);
                                n.Storage = StorageBehavior.KeyValueKey;
                                pars[j].Add(n);
                                noteIDRequred = false;
								if (sender._KeyValueAttributeNames != null)
								{
									object[] vals = sender.GetSlot<object[]>(row, sender._KeyValueAttributeSlotPosition);

									if (vals != null)
									{
										foreach (KeyValuePair<string, int> pair in sender._KeyValueAttributeNames)
										{
											if (pair.Value < vals.Length)
											{
												PXDataFieldAssign a =
													new PXDataFieldAssign(pair.Key,
													sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueDate ? PXDbType.DateTime
													: (sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueNumeric ? PXDbType.Bit
													: PXDbType.NVarChar), vals[pair.Value]
													);
												a.Storage = sender._KeyValueAttributeTypes[pair.Key];
												if (a.IsChanged = audit && a.Value != null)
												{
													a.NewValue = sender.AttributeValueToString(pair.Key, a.Value);

												}
												pars[j].Add(a);
											}
										}
									}
								}
							}
							sender._AdjustStorage(field, assign);
                            pars[j].Add(assign);
                            break;
                        }
                    }
                }
            }
        }

        /// <exclude/>
		public override bool PersistUpdated(PXCache sender, object row)
		{
			ISqlDialect dialect = sender.Graph.SqlDialect;
			Type[] tables = GetTables();
			List<Type> extensions = sender.GetExtensionTables();
			if (extensions != null)
			{
				extensions = new List<Type>(extensions);
				extensions.AddRange(tables);
				tables = extensions.ToArray();
			}
			if (keys == null)
			{
				keys = new List<string>(sender.Keys);
			}
			var unchanged = sender.GetOriginal(row);
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			List<PXDataFieldParam>[] pars = new List<PXDataFieldParam>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldParam>();
			}
			PrepareParametersForUpdate(sender, row, tables, dialect, unchanged, pars);
            for (int j = 0; j < tables.Length - 1; j++)
			{
				foreach (string field in keys)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					object val = sender.GetValue(row, field);
					sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[j], out description);
					if (description == null || description.Expr == null)
					{ sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[j], out description); }
					if (description != null && description.Expr != null)
					{
						object origval;
						if (description.IsRestriction && unchanged != null && description.DataType != PXDbType.Timestamp
						&& sender.Keys.Contains(field) && !object.Equals((origval = sender.GetValue(unchanged, field)), val)
						&& origval != null)
						{
							PXCommandPreparingEventArgs.FieldDescription origdescription;
							sender.RaiseCommandPreparing(field, unchanged, origval, PXDBOperation.Select, tables[j], out origdescription);
						    if (origdescription != null && origdescription.Expr != null)
						    {
						        PXDataFieldAssign assign = new PXDataFieldAssign(
							        (Column)description.Expr,
						            description.DataType, description.DataLength, description.DataValue,
						            sender.ValueToString(field, val, description.DataValue));
						        pars[j].Add(assign);
						        pars[j].Add(new PXDataFieldRestrict(
							        (Column)origdescription.Expr,
						            origdescription.DataType, origdescription.DataLength, origdescription.DataValue));
						    }
						    else
						    {
						        pars[j].Add(new PXDataFieldRestrict(
							        (Column)description.Expr,
						            description.DataType, description.DataLength, description.DataValue));
						    }
						}
					    else
					    {
					        pars[j].Add(new PXDataFieldRestrict(
						        (Column)description.Expr,
					            description.DataType, description.DataLength, description.DataValue));
					    }
					}
				}
			}
			for (int i = tables.Length - 1; i >= 0 ; i--)
			{
				bool success;
				try
				{
					pars[i].Add(PXDataFieldRestrict.OperationSwitchAllowed);
                    if (i < tables.Length - 1)
                    {
                        companySetting settings;
                        PXDatabase.Provider.getCompanyID(tables[i].Name, out settings);
                        if (settings != null && settings.Deleted != null)
                        {
                            pars[i].Add(new PXDataFieldRestrict(settings.Deleted, PXDbType.Bit, 1, false));
                        }
                    }
                    if(unchanged==null)
                        pars[i].Add(PXSelectOriginalsRestrict.SelectOriginalValues);
					success = sender.Graph.ProviderUpdate(tables[i], pars[i].ToArray());
				}
				catch (PXDbOperationSwitchRequiredException)
				{
					List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
					foreach (string field in sender.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
						if (description?.Expr != null && !description.IsExcludedFromUpdate)
						{
							if (tableMeet(description, tables[i], dialect))
							{
								ins.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					if (i < tables.Length - 1)
					{
						foreach (string field in keys)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
							if (description?.Expr == null || description.IsExcludedFromUpdate)
							{
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
							}
							if (description != null && description.Expr != null)
							{
								ins.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					sender.Graph.ProviderInsert(tables[i], ins.ToArray());
					success = true;
				}
				if (!success)
				{
					if (i == tables.Length - 1)
					{
						throw GetLockViolationException(tables[i], pars[i].ToArray(), PXDBOperation.Update);
					}
					else
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							object val = sender.GetValue(row, field);
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
							if (description?.Expr != null && !description.IsExcludedFromUpdate)
							{
								if (tableMeet(description, tables[i], dialect))
								{
									PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, null);
									if (audit && val != null)
									{
										assign.IsChanged = true;
										assign.NewValue = sender.ValueToString(field, val, description.DataValue);
									}
									else assign.IsChanged = false;
									ins.Add(assign);
								}
							}
						}
						foreach (string field in keys)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
							if (description == null || description.Expr == null)
							{
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
							}
							if (description != null && description.Expr != null)
							{
								ins.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
						try
						{
							ins.Add(PXDataFieldAssign.OperationSwitchAllowed);
							sender.Graph.ProviderInsert(tables[i], ins.ToArray());
						}
						catch (PXDbOperationSwitchRequiredException)
						{
							List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
							foreach (string field in sender.Fields)
							{
								PXCommandPreparingEventArgs.FieldDescription description;
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
								if (description != null && description.Expr != null)
								{
									if (tableMeet(description, tables[i], dialect))
									{
										if (description.IsRestriction)
										{
											upd.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
										}
										else if(!description.IsExcludedFromUpdate)
										{
											upd.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
										}
									}
								}
							}
							foreach (string field in keys)
							{
								PXCommandPreparingEventArgs.FieldDescription description;
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
								if (description == null || description.Expr == null)
								{
									sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
								}
								if (description != null && description.Expr != null)
								{
									upd.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
								}
							}
							sender.Graph.ProviderUpdate(tables[i], upd.ToArray());
						}
					}
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					BqlTablePair pair;
					if (sender._Originals.TryGetValue((IBqlTable)row, out pair))
					{
						if (sender._OriginalsRemoved == null)
						{
							sender._OriginalsRemoved = new PXCacheOriginalCollection();
						}
						sender._OriginalsRemoved[(IBqlTable)row] = pair;
					}
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}

        private void PrepareParametersForUpdate(PXCache sender, object row, Type[] tables, ISqlDialect dialect, object unchanged,
            List<PXDataFieldParam>[] pars)
        {
			KeysVerifyer kv = new KeysVerifyer(sender);
			bool noteIDRequred = sender._HasKeyValueStored();
            foreach (string field in sender.Fields)
            {
                object val = sender.GetValue(row, field);
                PXCommandPreparingEventArgs.FieldDescription description;
                sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, null, out description);
                if (description != null && description.Expr != null)
                {
                    for (int j = 0; j < tables.Length; j++)
                    {
                        if (tableMeet(description, tables[j], dialect))
                        {
                            var origval = unchanged.With(c=>sender.GetValue(unchanged, field));
                            PXCommandPreparingEventArgs.FieldDescription origdescription=null;
                            if(origval!=null)
                                sender.RaiseCommandPreparing(field, unchanged, origval, PXDBOperation.Update, null, out origdescription);
                            if (description.IsRestriction)
                            {
                                if (origval != null && description.DataType != PXDbType.Timestamp
                                    && sender.Keys.Contains(field) &&
                                    !object.Equals(origval, val))
                                {
                                    if (origdescription != null && !String.IsNullOrEmpty(origdescription.Expr.SQLQuery(sender.Graph.SqlDialect.GetConnection()).ToString()))
                                    {
                                        PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr,
                                            description.DataType, description.DataLength, description.DataValue,
                                            sender.ValueToString(field, val, description.DataValue))
                                        {
                                            OldValue = origdescription.DataValue
                                        };
                                        pars[j].Add(assign);
                                        pars[j].Add(new PXDataFieldRestrict((Column)origdescription.Expr,
                                            origdescription.DataType,
                                            origdescription.DataLength, origdescription.DataValue));
                                    }
                                    else
                                    {
                                        pars[j].Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType,
                                            description.DataLength, description.DataValue));
                                    }
                                }
                                else
                                {
                                    pars[j].Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType,
                                        description.DataLength, description.DataValue));
                                }
								kv.SetRestriction(field, description);
							}
							else
                            {
                                if (description.IsExcludedFromUpdate)
                                {
                                    if (unchanged != null)
                                    {
                                        var param = new PXDummyDataFieldRestrict((Column)description.Expr,
                                            description.DataType,
                                            description.DataLength, origdescription?.DataValue??origval);
                                        pars[j].Add(param);
                                    }
                                }
                                else
                                {
                                    PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr,
                                        description.DataType,
                                    description.DataLength, description.DataValue, null);
                                if (unchanged != null)
                                {
                                        if (assign.IsChanged = !object.Equals(sender.GetValue(row, field), origval))
                                    {
                                        assign.NewValue = sender.ValueToString(field, val, description.DataValue);
                                        assign.OldValue =
                                            origdescription == null || PXCache.IsOrigValueNewDate(sender, origdescription)
                                                ? origval
                                                : origdescription.DataValue;
                                    }
                                }
                                else assign.IsChanged = false;
                                if (noteIDRequred &&
                                    String.Equals(field, sender._NoteIDName, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (assign.Value == null)
                                    {
                                        assign.Value = SequentialGuid.Generate();
                                        sender.SetValue(row, (int)sender._NoteIDOrdinal, assign.Value);
                                    }
                                        PXDataFieldAssign n = new PXDataFieldAssign(sender._NoteIDName,
                                            PXDbType.UniqueIdentifier,
                                        16, assign.Value);
                                    n.Storage = StorageBehavior.KeyValueKey;
                                    pars[j].Add(n);
                                    noteIDRequred = false;
										if (sender._KeyValueAttributeNames != null)
										{
											object[] vals = sender.GetSlot<object[]>(row, sender._KeyValueAttributeSlotPosition);
											object[] origs = sender.GetSlot<object[]>(row, sender._KeyValueAttributeSlotPosition, true);

											if (vals != null)
											{
												foreach (KeyValuePair<string, int> pair in sender._KeyValueAttributeNames)
												{
													if (pair.Value < vals.Length)
													{
														PXDataFieldAssign a =
															new PXDataFieldAssign(pair.Key,
															sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueDate ? PXDbType.DateTime
															: (sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueNumeric ? PXDbType.Bit
															: PXDbType.NVarChar), vals[pair.Value]
															);
														a.Storage = sender._KeyValueAttributeTypes[pair.Key];
														if (a.IsChanged = origs != null && pair.Value < origs.Length && !object.Equals(a.Value, origs[pair.Value]))
														{
															a.NewValue = sender.AttributeValueToString(pair.Key, a.Value);
															a.OldValue = origs[pair.Value];

														}
														pars[j].Add(a);
													}
												}
											}
										}
									}
									sender._AdjustStorage(field, assign);
                                pars[j].Add(assign);
                            }
                            }
                            break;
                        }
                    }
                }
            }
			kv.Check(sender.BqlTable);
		}

		/// <exclude/>
		public override bool PersistDeleted(PXCache sender, object row)
		{
			ISqlDialect dialect = sender.Graph.SqlDialect;
            Type[] tables = GetTables();
			List<Type> extensions = sender.GetExtensionTables();
			if (extensions != null)
			{
				extensions = new List<Type>(extensions);
				extensions.AddRange(tables);
				tables = extensions.ToArray();
			}

			if (keys == null)
			{
				keys = new List<string>(sender.Keys);
			}
			List<PXDataFieldRestrict>[] pars = new List<PXDataFieldRestrict>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldRestrict>();
			}
			// Preparing restrictions
			PrepareRestrictionsForDelete(sender, row, tables, dialect, pars, out var unchanged);
            for (int j = 0; j < tables.Length - 1; j++)
			{
				foreach (string field in keys)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Delete, tables[j], out description);
					if (description == null || description.Expr == null)
					{
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[j], out description);
					}
					if (description != null && description.Expr != null)
					{
						pars[j].Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
					}
				}
			}
			// Removing bypassed tables from the list
			if (_bypassOnDelete != null)
				tables = tables.Except(_bypassOnDelete).ToArray();
			for (int i = 0; i < tables.Length; i++)
			{
                if(unchanged == null)
                    pars[i].Add(PXSelectOriginalsRestrict.SelectOriginalValues);
				try
				{
					if (!sender.Graph.ProviderDelete(tables[i], pars[i].ToArray()) && i == tables.Length - 1)
					{
						throw GetLockViolationException(tables[i], pars[i].ToArray(), PXDBOperation.Delete);
					}
				}
				catch (PXDbOperationSwitchRequiredException)
				{
					List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
					foreach (string field in sender.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
						if (description?.Expr != null&&!description.IsExcludedFromUpdate)
						{
							if (tableMeet(description, tables[i], dialect))
							{
								ins.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					if (i < tables.Length - 1)
					{
						foreach (string field in keys)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
							if (description?.Expr == null || description.IsExcludedFromUpdate)
							{
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
							}
							if (description != null && description.Expr != null)
							{
								ins.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					sender.Graph.ProviderInsert(tables[i], ins.ToArray());
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}

        private void PrepareRestrictionsForDelete(PXCache sender, object row, Type[] tables, ISqlDialect dialect, List<PXDataFieldRestrict>[] pars, out object unchanged)
        {
            unchanged = sender.GetOriginal(row);
            bool noteIDRequred = sender._HasKeyValueStored();
            foreach (string field in sender.Fields)
            {
                PXCommandPreparingEventArgs.FieldDescription description;
                sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Delete, null,
                    out description);
                if (description != null && description.Expr != null)
                {
                    if (description.IsRestriction)
                    {
                        for (int j = 0; j < tables.Length; j++)
                        {
                            if (tableMeet(description, tables[j], dialect))
                            {
                                pars[j].Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType,
                                    description.DataLength, description.DataValue));
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < tables.Length; j++)
                        {
                            if (tableMeet(description, tables[j], dialect))
                            {
                                var origval = unchanged.With(c => sender.GetValue(c, field));
                                PXCommandPreparingEventArgs.FieldDescription origdescription=null;
                                if(origval!=null)
                                    sender.RaiseCommandPreparing(field, unchanged, origval, PXDBOperation.Update, null, out origdescription);
                                var assign = sender.IsKvExtField(field) || sender.IsKvExtAttribute(field)
								    ? new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength,
                                        description.DataValue)
                                    : new PXDummyDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength,
                                        unchanged != null ? origdescription?.DataValue??origval : description.DataValue);
                                if (noteIDRequred &&
                                    String.Equals(field, sender._NoteIDName, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (assign.Value == null)
                                    {
                                        assign.Value = SequentialGuid.Generate();
                                        sender.SetValue(row, (int)sender._NoteIDOrdinal, assign.Value);
                                    }
                                    PXDataFieldRestrict n = new PXDataFieldRestrict(sender._NoteIDName,
                                        PXDbType.UniqueIdentifier, 16, assign.Value);
                                    n.Storage = StorageBehavior.KeyValueKey;
                                    pars[j].Add(n);
                                    noteIDRequred = false;
                                }
                                sender._AdjustStorage(field, assign);
                                pars[j].Add(assign);
                                break;
                            }
                        }
                    }
                }
            }
        }

		/// <exclude/>
		internal bool IsBypassedOnDelete(Type table)
		{
			return _bypassOnDelete?.Contains(table) == true;
		}

        private sealed class BqlRowSelection : BqlCommand, IPXExtensibleTableAttribute
		{
            public bool ByNoteId { get; set; }
			private void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e, Type table)
			{
				if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Select) return;

				string fieldName = this.links[0];
				Type origTable;
				if (e.Table == null || sender.BqlSelect == null || sender.BqlSelect.GetType() == this.GetType()) {
					origTable = e.Table ?? table;
				} else {
					origTable = e.Table;
					fieldName = table.Name + "_" + fieldName;
				}
				e.BqlTable = table;
				e.Expr = new Column(fieldName, new SimpleTable(origTable));
			}
			private void RowSelecting(PXCache sender, PXRowSelectingEventArgs e, Type table)
			{
				if (!object.ReferenceEquals(sender, attributecache))
					return;
				if (e.Record != null && !(e.Record is PXDataRecordMap) && e.Record.IsDBNull(e.Position))
				{
					using (new PXConnectionScope())
					{
						if (this.fields[table] == null)
							return;

						foreach (var field in this.fields[table])
							{
								object newValue;
							if (sender.RaiseFieldDefaulting(field, e.Row, out newValue))
								{
								sender.RaiseFieldUpdating(field, e.Row, ref newValue);
							}
							sender.SetValue(e.Row, field, newValue);
						}
					}
				}
				e.Position++;
			}
			string[] IPXExtensibleTableAttribute.Keys
			{
				get
				{
					return links.ToArray();
				}
			}
			public BqlRowSelection(PXCache cache, bool single, List<string> links)
				: this(cache, single, links, null)
			{
			}
			public BqlRowSelection(PXCache cache, bool single, List<string> links, PXDBInterceptorAttribute parent)
			{
				this.attributecache = cache;
				this.single = single;
				this.mainTable = cache.GetItemType();
				this.links = links != null ? new List<string>(links) : new List<string>(cache.Keys);
				this.tables = new List<Type>();
				Type table = mainTable;
				while (table != typeof(object))
				{
					if ((table.BaseType == typeof(object)
						|| !typeof(IBqlTable).IsAssignableFrom(table.BaseType))
						&& typeof(IBqlTable).IsAssignableFrom(table)
						|| table.IsDefined(typeof(PXTableAttribute), false))
					{
						this.tables.Add(table);
						if (table.IsDefined(typeof(PXTableAttribute), false))
						{
							foreach (PXTableAttribute attr in table.GetCustomAttributes(typeof(PXTableAttribute), false))
							{
								if (attr.IsOptional)
								{
									this.optional.Add(table);
								}
							}
						}
					}
					table = table.BaseType;
				}
				List<Type> extensions = cache.GetExtensionTables();
				if (extensions != null)
				{
					if (extensions.Count > 0 && this.tables.Count == 1 && object.ReferenceEquals(cache.Interceptor, parent))
					{
						cache.SingleExtended = true;
					}
					foreach (Type ext in extensions)
					{
						if (ext.IsDefined(typeof(PXTableAttribute), false))
						{
							foreach (PXTableAttribute attr in ext.GetCustomAttributes(typeof(PXTableAttribute), false))
							{
								if (attr.IsOptional)
								{
									this.optional.Add(ext);
								}
							}
						}
					}
				}
				if (single && optional.Count > 0 && this.links.Count > 0)
				{
					this.fields = new Dictionary<Type, List<string>>();
					foreach (Type t in optional)
					{
						string fn = t.Name + "_" + this.links[0];
						if (!cache.Fields.Contains(fn))
						{
							cache.Fields.Add(fn);
							cache.Graph.CommandPreparing.AddHandler(cache.GetItemType(), fn, (sender, e) => CommandPreparing(sender, e, t));
							cache.Graph.RowSelecting.AddHandler(cache.GetItemType(), (sender, e) => RowSelecting(sender, e, t));

							var fieldsList = new List<string>();
							this.fields.Add(t, fieldsList);
							string fieldProcessed = null;
							foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
							{
								if (fieldProcessed == attr.FieldName)
								{
									continue;
								}
								if (attr.BqlTable == t)
								{
									fieldsList.Add(attr.FieldName);
									fieldProcessed = attr.FieldName;
								}
							}

						}
					}
				}
			}
			private List<Type> tables;
			private List<Type> optional = new List<Type>();
			private List<string> links;
			private Dictionary<Type, List<string>> fields;
			private readonly Type mainTable;
			private bool single;
			private PXCache attributecache;


			public override SQLTree.Query GetQueryInternal(PXGraph graph, BqlCommandInfo info, Selection selection) 
			{
				bool status = true;
				Query query = new Query();

				if (info.Tables != null && single) {
					info.Tables.Add(mainTable);
				}
				else if (info.Tables != null)
					info.Tables.AddRange(this.tables);

				if (graph == null) return query;
				Query outerQuery = single ? new Query() : null;

				PXCache cache = graph.Caches[mainTable];
				var sqlDialect = graph.SqlDialect;
				var preparedColumnsByAlias = new Dictionary<string, SQLExpression>(StringComparer.OrdinalIgnoreCase);
				List<Type> extensions = cache.GetExtensionTables();
				foreach (string field in cache.Fields) {
					if (selection.Restrict && !IsFieldRestricted(cache, selection, field)) continue;
					PXCommandPreparingEventArgs.FieldDescription description;
					cache.RaiseCommandPreparing(field, null, null, PXDBOperation.Select, null, out description);
					if (description?.Expr == null) continue;

					bool shouldWriteFieldName = 
						!(description.Expr is SQLTree.Column) ||
						description.BqlTable == null || 
						description.BqlTable.IsAssignableFrom(mainTable) || (
							typeof(PXCacheExtension).IsAssignableFrom(description.BqlTable) &&
							description.BqlTable.BaseType.IsGenericType &&
							description.BqlTable.BaseType.GetGenericArguments().Last().IsAssignableFrom(mainTable));
					if(!shouldWriteFieldName) description.Expr=SQLExpression.Null();

					if (single) {
						outerQuery.GetSelection().Add(new Column(field, mainTable));
						preparedColumnsByAlias.Add(field, description.Expr.Duplicate());
					}
					selection.AddExpr(field, description.Expr.Duplicate());
					query.GetSelection().Add(description.Expr.SetAlias(field));
				}
				List<Type> joinedTables = new List<Type>();
				for (int i = 0; i < this.tables.Count; i++) {
					Joiner.JoinType jt = Joiner.JoinType.INNER_JOIN;
					if(i==0) jt = Joiner.JoinType.MAIN_TABLE;
					else if(optional.Contains(this.tables[i])) jt=Joiner.JoinType.LEFT_JOIN;

					TableChangingScope.AddUnchangedRealName(this.tables[i].Name);
                    Joiner j = new Joiner(jt, TableChangingScope.GetSQLTable(() => new SQLTree.SimpleTable(this.tables[i]), this.tables[i].Name), query);					

					joinedTables.Add(this.tables[i]);
					if (jt != Joiner.JoinType.MAIN_TABLE) {
						if (this.links.Count > 0) j.On(MakeWhere(joinedTables, cache));
						else j.On(new SQLConst(1).EQ(1));
						joinedTables.Remove(this.tables[i]);
					}
					query.AddJoin(j);
				}
				if (extensions != null) {
					foreach (Type ext in extensions) {
                        TableChangingScope.AddUnchangedRealName(ext.Name);
						Joiner.JoinType jt = Joiner.JoinType.INNER_JOIN;
						if (optional.Contains(ext)) jt = Joiner.JoinType.LEFT_JOIN;
                        Joiner j = new Joiner(jt, TableChangingScope.GetSQLTable(() => new SQLTree.SimpleTable(ext), ext.Name), query);
						joinedTables.Add(ext);
						if (this.links.Count > 0) j.On(MakeWhere(joinedTables, cache));
						else j.On(new SQLConst(1).EQ(1));
						joinedTables.Remove(ext);

						query.AddJoin(j);
					}
				}

				if (joinedTables.Count > 1) query.Where(MakeWhere(joinedTables, cache));

				if (query.GetWhere()!=null) {

				}

				if (single) {
					string[] wherefields;
					if (ByNoteId) {
						wherefields = new[] { "noteid" };
					}
					else {
						wherefields = cache.Keys.ToArray();
					}
					SQLExpression innerWhere = SQLExpression.None();
					SQLExpression outerWhere = SQLExpression.None();
					for (int i = 0; i < wherefields.Length; i++) {
						SQLExpression expressionThere;
						if (preparedColumnsByAlias.TryGetValue(wherefields[i], out expressionThere)) {
							innerWhere=innerWhere.And(expressionThere.EQ(Literal.NewParameter(i)));
						}
						outerWhere=outerWhere.And(new Column(wherefields[i], mainTable).EQ(Literal.NewParameter(i)));
					}

					outerQuery.Where(outerWhere);
					if(query.GetWhere()==null) query.Where(innerWhere);
					else query.Where(query.GetWhere().And(innerWhere));

					query.Alias = mainTable.Name;
					outerQuery.From(query);
					query = outerQuery;
				}

				if (!status) query.NotOK();
				return query;
			}

			private SQLExpression MakeWhere(List<Type> bound, PXCache cache) {
				SQLExpression exp = SQLExpression.None();
				for (int i = 0; i < links.Count; i++) {
					for (int j = 0; j < bound.Count; j++) {
						PXCommandPreparingEventArgs.FieldDescription pdescription;
						cache.RaiseCommandPreparing(links[i], null, null, PXDBOperation.Select, bound[j], out pdescription);
						if (pdescription?.Expr == null) continue;
						int k = j + 1;
						if (k < bound.Count) {
							PXCommandPreparingEventArgs.FieldDescription description;
							cache.RaiseCommandPreparing(links[i], null, null, PXDBOperation.Select, bound[k], out description);
							if (description?.Expr == null) continue;
							exp = exp.And(pdescription.Expr.EQ(description.Expr));
							TableChangingScope.AppendRestrictionsOnIsNew(ref exp, cache.Graph, new List<Type> { bound[j], bound[k] }, new Selection(), true);
						}
					}
				}

				return exp;
			}
			
			public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
			}
			public override BqlCommand OrderByNew<newOrderBy>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand OrderByNew(Type newOrderBy)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereAnd<where>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereAnd(Type where)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereNew<newWhere>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereNew(Type newWhere)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereNot()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereOr<where>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereOr(Type where)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PXOfflineAttribute : PXDBInterceptorAttribute
	{
		protected class int0 : PX.Data.BQL.BqlInt.Constant<int0>
		{
			public int0()
				: base(0)
			{
			}
		}
		protected BqlCommand _Command;
		protected List<object> _Inserted;
		protected internal virtual List<object> Inserted
		{
			get
			{
				return _Inserted;
			}
		}
		protected List<object> _Updated;
		protected internal virtual List<object> Updated
		{
			get
			{
				return _Updated;
			}
		}
		protected List<object> _Deleted;
		protected internal virtual List<object> Deleted
		{
			get
			{
				return _Deleted;
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Command = BqlCommand.CreateInstance(typeof(Select<,>), sender.BqlTable, typeof(Where<int0, NotEqual<int0>>));
		}
		public override BqlCommand GetRowCommand()
		{
			return _Command;
		}
		public override BqlCommand GetTableCommand()
		{
			return _Command;
		}
		public override bool PersistDeleted(PXCache sender, object row)
		{
			sender.SetStatus(row, PXEntryStatus.Notchanged);
			return true;
		}
		public override bool PersistInserted(PXCache sender, object row)
		{
			sender.SetStatus(row, PXEntryStatus.Notchanged);
			return true;
		}
		public override bool PersistUpdated(PXCache sender, object row)
		{
			sender.SetStatus(row, PXEntryStatus.Notchanged);
			return true;
		}
		public override object Delete(PXCache sender, object row)
		{
			sender.Current = null;
			if (sender.Graph.Defaults.ContainsKey(sender.GetItemType()))
			{
				sender.Graph.Defaults.Remove(sender.GetItemType());
			}
			if (_Deleted == null)
			{
				_Deleted = new List<object>();
			}
			_Deleted.Add(row);
			return row;
		}
		public override object Insert(PXCache sender, object row)
		{
			sender.Current = null;
			sender.Graph.Defaults[sender.GetItemType()] = delegate() { return row; };
			if (_Inserted == null)
			{
				_Inserted = new List<object>();
			}
			_Inserted.Add(row);
			return row;
		}
		public override object Update(PXCache sender, object row)
		{
			sender.Current = null;
			sender.Graph.Defaults[sender.GetItemType()] = delegate() { return row; };
			if (_Updated == null)
			{
				_Updated = new List<object>();
			}
			_Updated.Add(row);
			return row;
		}
	}

	/// <summary>Binds the DAC to an arbitrary data set defined by the
	/// <tt>Select</tt> command. The attribute thus defines a named view, but
	/// implemented by the server side rather then the database.</summary>
	/// <remarks>
	/// 	<para>You can place the attribute on the DAC declaration. The
	/// framework doesn't bind such DAC to a database table (that is,
	/// doesn't select data from the table having the same name as the DAC).
	/// Instead, you specify an arbitrary BQL <tt>Select</tt> command that is
	/// executed to retrieve data for the DAC. The <tt>Select</tt> command can
	/// select data from one or several comands and include any BQL
	/// clauses.</para>
	/// 	<para>By default, the projection is readonly, but you can make it
	/// updatable by setting the <tt>Persistent</tt> property to
	/// <tt>true</tt>. The attribute will use the <tt>Select</tt> command to
	/// determine which tables needs updating. However, only the first table
	/// referenced by the <tt>Select</tt> command is updated by default. If
	/// the data should be committed not only into main table, but also to the
	/// joined tables, the fields that connect the tables must be marked with
	/// the <see cref="PXExtraKeyAttribute">PXExtraKey</see> attribute.
	/// Additionally, you can use the constructor with two parameters to
	/// provide the list of table explicitly. This list should include the
	/// tables referenced in the <tt>Select</tt> command. This constructor
	/// will also set the <tt>Persistent</tt> property to
	/// <tt>true</tt>.</para>
	/// 	<para>You should explicitly map the projection fields to the column
	/// retrieved by the <tt>Select</tt> command. To map a field, set the
	/// <tt>BqlField</tt> property of the attribute that binds the field to
	/// the database (such as <tt>PXDBString</tt> and <tt>PXDBDecimal</tt>) to
	/// the type that represents the column, as follows.</para>
	/// 	<code>[PXDBString(15, IsUnicode = true, BqlField = typeof(Supplier.accountCD))]</code>
	/// </remarks>
	/// <seealso cref="PXExtraKeyAttribute"></seealso>
	/// <example>
	/// 	<code title="Example" description="In the following example, the attribute joins data from two table and projects it to the single DAC." lang="CS">
	/// [Serializable]
	/// [PXProjection(typeof(
	///     Select2&lt;Supplier,
	///         InnerJoin&lt;SupplierProduct,
	///             On&lt;SupplierProduct.accountID, Equal&lt;Supplier.accountID&gt;&gt;&gt;&gt;))]
	/// public partial class SupplierPrice : IBqlTable
	/// {
	///     public abstract class accountID : PX.Data.BQL.BqlInt.Field&lt;accountID>
	///     {
	///     }
	///     // The field mapped to the Supplier field (through setting of BqlField)
	///     [PXDBInt(IsKey = true, BqlField = typeof(Supplier.accountID))]
	///     public virtual int? AccountID { get; set; }
	///     public abstract class productID : PX.Data.BQL.BqlInt.Field&lt;productID>
	///     {
	///     }
	///     // The field mapped to the SupplierProduct field
	///     // (through setting of BqlField)
	///     [PXDBInt(IsKey = true, BqlField = typeof(SupplierProduct.productID))]
	///     [PXUIField(DisplayName = "Product ID")]
	///     public virtual int? ProductID { get; set; }
	///     ...
	/// }</code>
	/// 	<code title="Example2" description="Note how the DAC declares the fields. The projection defined in the example is readonly. To make it updatable, you should set the Persistent property to true, changing the attribute declaration to the following one." groupname="Example" lang="CS">
	/// [PXProjection(
	///     typeof(Select2&lt;Supplier,
	///         InnerJoin&lt;SupplierProduct,
	///             On&lt;SupplierProduct.accountID, Equal&lt;Supplier.accountID&gt;&gt;&gt;&gt;),
	///     Persistent = true
	/// )]</code>
	/// 	<code title="Example3" description="If the projection should be able to update both tables, you should place the PXExtraKey attribute on the field that relates the tables (the AccountID property) as follows." groupname="Example2" lang="CS">
	/// [PXDBInt(IsKey = true, BqlField = typeof(Supplier.accountID))]
	/// [PXExtraKey]
	/// public virtual int? AccountID { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Class)]
	public class PXProjectionAttribute : PXDBInterceptorAttribute
	{
		protected Type _select;
		internal Type Select
		{
			get { return _select; }
		}
		protected Type[] _tables = null;
		protected bool _persistent;
        /// <summary>Gets or sets the value that indicates whether the instances
        /// of the DAC that represents the projection can be saved to the
        /// database. If the property equals <tt>true</tt>, the attribute will
        /// parse the <tt>Select</tt> command and determine the tables that should
        /// be updated. Alternatively, you can specify the list of tables in the
        /// constructor. If the property equals <tt>false</tt>, the DAC is
        /// readonly.</summary>
        /// <example>
        /// The projection defined below can update the <tt>Supplier</tt> table.
        /// <code>
        /// [Serializable]
        /// [PXProjection(
        ///     typeof(Select2&lt;Supplier,
        ///         InnerJoin&lt;SupplierProduct,
        ///             On&lt;SupplierProduct.accountID, Equal&lt;Supplier.accountID&gt;&gt;&gt;&gt;),
        ///     Persistent = true
        /// )]
        /// public partial class SupplierPrice : IBqlTable
        /// { ... }
        /// </code>
        /// </example>
		public bool Persistent
		{
			get
			{
				return _persistent;
			}
			set
			{
				_persistent = value;
			}
		}
		
        /// <summary>Initializes a new instance that binds the DAC to the data set
        /// defined by the provided <tt>Select</tt> command.</summary>
        /// <param name="select">The BQL command that defines the data set, based
        /// on the <tt>Select</tt> class or any other class that implements
        /// <tt>IBqlSelect</tt>.</param>
		public PXProjectionAttribute(Type select)
		{
			_select = select;
		}

        /// <summary>Initializes a new instance that binds the DAC to the
        /// specified data set and enables update saving of the DAC instances to
        /// the database. The tables that should be updated during update of the
        /// current DAC.</summary>
        /// <param name="select">The BQL command that defines the data set, based
        /// on the <tt>Select</tt> class or any other class that implements
        /// <tt>IBqlSelect</tt>.</param>
        /// <param name="persistent">The list of DACs that represent the tables to
        /// update during update of the current DAC.</param>
		public PXProjectionAttribute(Type select, Type[] persistent)
			: this(select)
		{
			_tables = persistent;
			Persistent = true;
		}

		protected BqlCommand rowselection;
		protected BqlCommand tableselection;
	    protected BqlCommand rowSelectionByNoteId;
	    protected bool canSelectByNoteId = false;

        /// <exclude/>
		public override Type[] GetTables()
		{
			Type[] tables = base.GetTables();
			return Array.FindAll(tables, a => _tables == null || Array.IndexOf(_tables, a) >= 0);
		}

		protected virtual Type GetSelect(PXCache sender)
		{
			return _select;
		}

        /// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			tableselection = (BqlCommand)Activator.CreateInstance(GetSelect(sender));
			Type where = null;
			foreach (Type key in sender.BqlKeys)
			{
				@where = @where == null 
					? BqlCommand.Compose(typeof(Where<,>), key, typeof(Equal<>), typeof(Required<>), key) 
					: BqlCommand.Compose(typeof(Where2<,>), @where, typeof(And<>), typeof(Where<,>), key, typeof(Equal<>), typeof(Required<>), key);
			}
			rowselection = @where == null 
				? BqlCommand.CreateInstance(typeof(Select<>), sender.GetItemType()) 
				: BqlCommand.CreateInstance(typeof(Select<,>), sender.GetItemType(), @where);
			
		    var noteIdField = sender.GetBqlField("noteid");
            if (noteIdField!=null)
            {
                rowSelectionByNoteId = BqlCommand.CreateInstance(typeof(Select<,>), sender.GetItemType(),
                    BqlCommand.Compose(typeof(Where<,>), noteIdField, typeof(Equal<>), typeof(Required<>), noteIdField));
                canSelectByNoteId = true;
            }
			
			if (Child == null) return;

			tableselection = new BqlRowSelection(sender, Child, tableselection);
			rowselection = new BqlRowSelection(sender, this, rowselection);
            rowSelectionByNoteId = new BqlRowSelection(sender, this, rowSelectionByNoteId);

		}
		protected class BqlRowSelection : BqlCommand, IPXExtensibleTableAttribute
		{
			protected BqlCommand _Command;
			protected string[] _Links;
			protected Type _Alias;
			protected PXDBInterceptorAttribute _Child;
			string[] IPXExtensibleTableAttribute.Keys
			{
				get
				{
					return _Links;
				}
			}
			public BqlRowSelection(PXCache cache, PXDBInterceptorAttribute child, BqlCommand command)
			{
				_Child = child;
				_Command = command;
				_Alias = cache.GetItemType();
				BqlCommand cmd = _Child.GetTableCommand(cache);
				if (cmd is IPXExtensibleTableAttribute)
				{
					_Links = ((IPXExtensibleTableAttribute)cmd).Keys;
				}
				if (_Links == null)
				{
					_Links = cache.Keys.ToArray();
				}
			}

			public override SQLTree.Query GetQueryInternal(PXGraph graph, BqlCommandInfo info, Selection selection)
			{
				Query query = new Query();

				if (graph == null || _Child == null)
				{
					if (!info.IsEmpty)
					{
						// Just need to collect info
						_Command.GetQueryInternal(graph, info, selection);
					}
					return query;
				}

				PXCache cache = graph.Caches[_Alias];
				BqlCommand cmd = cache.BqlSelect;
				cache.BqlSelect = _Child.GetTableCommand(cache);
				var cacheList = new List<PXCache>();
				if (!(_Child is PXProjectionAttribute)) {
					cache.BypassCalced = true;
					Type cacheType = cache.GetItemType();
					while ((cacheType = cacheType.BaseType) != typeof(object)
							&& typeof(IBqlTable).IsAssignableFrom(cacheType)) {
						PXCache parentCache = graph.Caches[cacheType];
						if (!cacheList.Contains(parentCache)) {
							cacheList.Add(parentCache);
							parentCache.BypassCalced = true;
						}
					}
				}
				query = _Command.GetQueryInternal(graph, info, selection);
				cache.BqlSelect = cmd;
				cache.BypassCalced = false;
				foreach (var parentCache in cacheList) {
					parentCache.BypassCalced = false;
				}
				return query;
			}

			public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
				Verify(cache, item, pars, ref result, ref value);
			}
			public override BqlCommand OrderByNew<newOrderBy>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand OrderByNew(Type newOrderBy)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereAnd<where>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereAnd(Type where)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereNew<newWhere>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereNew(Type newWhere)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereNot()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereOr<where>()
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
			public override BqlCommand WhereOr(Type where)
			{
				throw new PXException(MsgNotLocalizable.MethodOrOperationNotImplemented);
			}
		}
        /// <exclude/>
		public override BqlCommand GetRowCommand()
		{
			return rowselection;
		}

        /// <exclude/>
	    public override BqlCommand GetRowByNoteIdCommand()
	    {
	        return rowSelectionByNoteId;
	    }

        /// <exclude/>
	    public override bool CanSelectByNoteId
        {
            get
            {
                return canSelectByNoteId;
            }
        }

        /// <exclude/>
		public override BqlCommand GetTableCommand()
		{
			return tableselection;
		}
        /// <exclude/>
		public override bool PersistInserted(PXCache sender, object row)
		{
			ISqlDialect dialect = sender.Graph.SqlDialect;
			if (!_persistent)
			{
				return base.PersistInserted(sender, row);
			}
			Type[] tables = GetTables();
			var baseTables = new Dictionary<Type, int>();
			List<PXDataFieldAssign>[] pars = new List<PXDataFieldAssign>[tables.Length];
			List<string>[] keys = new List<string>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldAssign>();
				keys[i] = new List<string>();
			}
			int length = tables.Length;
			for (int i = 0; i < length; i++)
			{
				Type t = tables[i];
				PXCache tc = sender.Graph.Caches[tables[i]];
				tables[i] = tc.BqlTable;
				List<Type> extensions = tc.GetExtensionTables();
				if (extensions != null)
				{
					if (i > 0)
						extensions.ForEach(_ => baseTables.Add(_, i));
					int oldlength = extensions.Count;
					extensions = new List<Type>(extensions);
					extensions.InsertRange(0, tables);
					tables = extensions.ToArray();
					Array.Resize<List<string>>(ref keys, extensions.Count);
					Array.Resize<List<PXDataFieldAssign>>(ref pars, extensions.Count);
					if (tc.BqlSelect is IPXExtensibleTableAttribute)
					{
						for (int j = keys.Length - oldlength; j < keys.Length; j++)
						{
							keys[j] = new List<string>(((IPXExtensibleTableAttribute)tc.BqlSelect).Keys);
							pars[j] = new List<PXDataFieldAssign>();
						}
					}
					else
					{
						for (int j = keys.Length - oldlength; j < keys.Length; j++)
						{
							keys[j] = new List<string>(tc.Keys);
							pars[j] = new List<PXDataFieldAssign>();
						}
					}
				}
			}
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			bool noteIDRequred = sender._HasKeyValueStored();
			foreach (string field in sender.Fields)
            {
                object val = sender.GetValue(row, field);
                PXCommandPreparingEventArgs.FieldDescription description;
                sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
                if (description?.Expr != null && !description.IsExcludedFromUpdate)
                {
                    for (int j = 0; j < tables.Length; j++)
                    {
                        if (tableMeet(description, tables[j], dialect))
                        {
                            if (pars[j] != null)
                            {
                                if (j > 0 && description.IsRestriction)
                                {
                                    if (description.DataValue == null)
                                    {
                                        pars[j] = null;
                                        for (int k = length; k < tables.Length; k++)
                                        {
                                            if (tables[k].BaseType.GetGenericArguments()[tables[k].BaseType.GetGenericArguments().Length - 1].IsAssignableFrom(tables[j]))
                                            {
                                                pars[k] = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        keys[j].Add(field);
                                    }
                                }
                                else
                                {
                                    PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, null);
                                    if (audit && val != null)
                                    {
                                        assign.IsChanged = true;
                                        assign.NewValue = sender.ValueToString(field, val, description.DataValue);
                                    }
                                    else assign.IsChanged = false;

									if (noteIDRequred && String.Equals(field, sender._NoteIDName, StringComparison.OrdinalIgnoreCase))
									{
										if (assign.Value == null)
										{
											assign.Value = SequentialGuid.Generate();
											sender.SetValue(row, (int)sender._NoteIDOrdinal, assign.Value);
										}
										PXDataFieldAssign n = new PXDataFieldAssign(sender._NoteIDName, PXDbType.UniqueIdentifier, 16,
											assign.Value);
										n.Storage = StorageBehavior.KeyValueKey;
										pars[j].Add(n);
										noteIDRequred = false;
									}
									sender._AdjustStorage(field, assign);

									pars[j].Add(assign);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < length; i++)
            {
				if (pars[i] != null && pars[i].Count > 0)
				{
					PXCache tc = sender.Graph.Caches[tables[i]];
					if (tc._NoteIDName != null)
					{
						PXDataFieldAssign note;
						if ((note = pars[i].FirstOrDefault(p => String.Equals(p.Column.Name, tc._NoteIDName, StringComparison.OrdinalIgnoreCase))) == null)
						{
							object val = SequentialGuid.Generate();
							PXCommandPreparingEventArgs.FieldDescription description;
							tc.RaiseCommandPreparing(tc._NoteIDName, null, val, PXDBOperation.Insert, null, out description);
							if (description?.Expr != null && !description.IsExcludedFromUpdate && !description.IsRestriction)
							{
								if (tableMeet(description, tables[i], dialect))
								{
									PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, null);
									if (audit && val != null)
									{
										assign.IsChanged = true;
										assign.NewValue = sender.ValueToString(tc._NoteIDName, val);
									}
									else assign.IsChanged = false;
									pars[i].Add(assign);
									note = assign;
								}
							}
						}
						if (note?.Value != null && sender._KeyValueAttributeNames != null)
						{
							PXDataFieldAssign n = new PXDataFieldAssign(tc._NoteIDName, PXDbType.UniqueIdentifier, 16,
								note.Value);
							n.Storage = StorageBehavior.KeyValueKey;
							pars[i].Add(n);

							object[] vals = sender.GetSlot<object[]>(row, sender._KeyValueAttributeSlotPosition);
							object[] origs = sender.GetSlot<object[]>(row, sender._KeyValueAttributeSlotPosition, true);

							if (vals != null)
							{
								foreach (KeyValuePair<string, int> pair in sender._KeyValueAttributeNames)
								{
									if (pair.Value < vals.Length)
									{
										PXDataFieldAssign a =
											new PXDataFieldAssign(pair.Key,
											sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueDate ? PXDbType.DateTime
											: (sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueNumeric ? PXDbType.Bit
											: PXDbType.NVarChar), vals[pair.Value]
											);
										a.Storage = sender._KeyValueAttributeTypes[pair.Key];
										if (a.IsChanged = origs != null && pair.Value < origs.Length && !object.Equals(a.Value, origs[pair.Value]))
										{
											a.NewValue = sender.AttributeValueToString(pair.Key, a.Value);
											a.OldValue = origs[pair.Value];

										}
										pars[i].Add(a);
									}
								}
							}
						}
					}
				}
            }
			try
			{
				pars[0].Add(PXDataFieldAssign.OperationSwitchAllowed);
				sender.Graph.ProviderInsert(tables[0], pars[0].ToArray());
			}
			catch (PXDbOperationSwitchRequiredException)
			{
				List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
				foreach (string field in sender.Fields)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
					if (description?.Expr != null && !description.IsExcludedFromUpdate)
					{
						if (tableMeet(description, tables[0], dialect))
						{
							if (description.IsRestriction)
							{
								upd.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
							else
							{
								upd.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
				}
				sender.Graph.ProviderUpdate(tables[0], upd.ToArray());
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					BqlTablePair pair;
					if (sender._Originals.TryGetValue((IBqlTable)row, out pair))
					{
						if (sender._OriginalsRemoved == null)
						{
							sender._OriginalsRemoved = new PXCacheOriginalCollection();
						}
						sender._OriginalsRemoved[(IBqlTable)row] = pair;
					}
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			for (int i = 1; i < tables.Length; i++)
			{
				if (pars[i] != null && pars[i].Count > 0)
				{
					try
					{
						foreach (string field in keys[i])
						{
							object val = sender.GetValue(row, field);
							if (baseTables.ContainsKey(tables[i]))
							{
								PXDataFieldAssign baseKey;
								int tableIndex = baseTables[tables[i]];
								if ((baseKey = pars[tableIndex].FirstOrDefault(p => String.Equals(p.Column.Name, field, StringComparison.OrdinalIgnoreCase))) != null)
								{
									val = baseKey.Value;
								}
							}
							PXCommandPreparingEventArgs.FieldDescription description;
							if (i < length)
							{
								sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
							}
							else
							{
								sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
							}
							if (description?.Expr != null && !description.IsExcludedFromUpdate)
							{
								PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, null);
								if (audit && val != null)
								{
									assign.IsChanged = true;
									assign.NewValue = sender.ValueToString(field, val, description.DataValue);
								}
								else assign.IsChanged = false;
								pars[i].Add(assign);
							}
						}

						pars[i].Add(PXDataFieldAssign.OperationSwitchAllowed);
						sender.Graph.ProviderInsert(tables[i], pars[i].ToArray());
					}
					catch (PXDbOperationSwitchRequiredException)
					{
						List<PXDataFieldParam>[] upd = new List<PXDataFieldParam>[tables.Length];
						foreach (string field in sender.Fields)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
							if (description?.Expr == null || description.IsExcludedFromUpdate)
								continue;

							for (int j = 0; j < tables.Length; j++)
							{
								if (!tableMeet(description, tables[j], dialect)) 
									continue;

								PXDataFieldParam fieldParam = description.IsRestriction
									? new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue) as PXDataFieldParam
									: new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue);
								if (upd[j] == null)
									upd[j] = new List<PXDataFieldParam>();
								upd[j].Add(fieldParam);

								if (fieldParam is PXDataFieldAssign)
									break;
							}
						}
						// Is there a reason to update other tables? (If we are inserting, their records might not exist yet)
						int tablesUpdated = 0;
						bool sharedDelete = PXTransactionScope.GetSharedDelete();
						for (int j = 0; j < tables.Length; j++)
							if ((!sharedDelete || j == i) && upd[j] != null && sender.Graph.ProviderUpdate(tables[j], upd[j].ToArray()))
								tablesUpdated++;
					}
				}
			}
			return true;
		}
        /// <exclude/>
		public override bool PersistUpdated(PXCache sender, object row)
		{
			ISqlDialect dialect = sender.Graph.SqlDialect;
			if (!_persistent)
			{
				return base.PersistUpdated(sender, row);
			}
			Type[] tables = GetTables();
			var baseTables = new Dictionary<Type, int>();
			string[][] links = new string[tables.Length][];
			List<PXDataFieldParam>[] pars = new List<PXDataFieldParam>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldParam>();
			}
			int length = tables.Length;
			for (int i = 0; i < length; i++)
			{
				Type t = tables[i];
				PXCache tc = sender.Graph.Caches[tables[i]];
				tables[i] = tc.BqlTable;
				List<Type> extensions = tc.GetExtensionTables();
				if (extensions != null)
				{
					if (i > 0)
						extensions.ForEach(_ => baseTables.Add(_, i));
					int oldlength = extensions.Count;
					extensions = new List<Type>(extensions);
					extensions.InsertRange(0, tables);
					tables = extensions.ToArray();
					Array.Resize<string[]>(ref links, extensions.Count);
					Array.Resize<List<PXDataFieldParam>>(ref pars, extensions.Count);
					if (tc.BqlSelect is IPXExtensibleTableAttribute)
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldParam>();
							links[j] = ((IPXExtensibleTableAttribute)tc.BqlSelect).Keys;
						}
					}
					else
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldParam>();
							links[j] = tc.Keys.ToArray();
						}
					}
				}
			}
		    object unchanged = sender.GetOriginal(row);
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			int timestamptable = -1;
			bool noteIDRequred = sender._HasKeyValueStored();
			foreach (string field in sender.Fields)
			{
				object val = sender.GetValue(row, field);
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, null, out description);
				if (description?.Expr != null && !description.IsExcludedFromUpdate)
				{
					if (timestamptable >= 0 && description.DataType == PXDbType.Timestamp
						&& sender.Graph._primaryRecordTimeStamp != null && sender.Graph.PrimaryItemType != null && sender.Graph.Caches[sender.Graph.PrimaryItemType] == sender)
					{
						byte[] saved = sender.Graph._primaryRecordTimeStamp;
						sender.Graph._primaryRecordTimeStamp = null;
						try
						{
							sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, null, out description);
						}
						finally
						{
							sender.Graph._primaryRecordTimeStamp = saved;
						}
					}

					for (int j = 0; j < tables.Length; j++)
					{
						if (tableMeet(description, tables[j], dialect))
						{
						    var origval = unchanged.With(c => sender.GetValue(unchanged, field));
						    PXCommandPreparingEventArgs.FieldDescription origdescription=null;
						    if(origval!=null)
						        sender.RaiseCommandPreparing(field, unchanged, origval, PXDBOperation.Select, tables[j], out origdescription);
							if (description.IsRestriction)
							{
								if (description.DataType == PXDbType.Timestamp)
								{
									timestamptable = j;
								}
								else if (description.DataValue != null && unchanged != null && !object.Equals(origval, val) && origval != null)
								{
									string fn = null;
									if (((description.Expr as Column)?.Table() as SimpleTable)?.Name == tables[j].Name)
									{
										fn = (description.Expr as Column).Name;
									}
									if (fn != null && sender.Graph.Caches[tables[j]].Keys.Contains(fn))
									{
										if (origdescription != null && origdescription.Expr != null)
										{
											PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, sender.ValueToString(field, val, description.DataValue));
										    assign.OldValue = origdescription.DataValue??origval;
                                            assign.IsChanged = true;
											pars[j].Add(assign);
											description = origdescription;
										}
									}
								}
								if (pars[j] != null)
								{
									if (j > 0 && description.DataValue == null)
									{
										pars[j] = null;
									}
									else
									{
										pars[j].Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
									}
								}
							}
							else if (pars[j] != null)
							{
							    if (description.IsExcludedFromUpdate)
							    {
							        if (unchanged != null)
							        {
							            var param = new PXDummyDataFieldRestrict((Column)description.Expr, description.DataType,
							                description.DataLength, origval);
							            pars[j].Add(param);
							        }
							    }
							    else
							    {
							        PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType,
							            description.DataLength, description.DataValue, null);
								if (unchanged != null)
								{
							            if (assign.IsChanged = !object.Equals(sender.GetValue(row, field), origval))
									{
										assign.NewValue = sender.ValueToString(field, val, description.DataValue);
									    assign.OldValue = origdescription == null || PXCache.IsOrigValueNewDate(sender, origdescription)
                                                ? origval
                                                : origdescription.DataValue;;
									}
								}
								else assign.IsChanged = false;

								if (noteIDRequred &&
									String.Equals(field, sender._NoteIDName, StringComparison.OrdinalIgnoreCase))
								{
									if (assign.Value == null)
									{
										assign.Value = SequentialGuid.Generate();
										sender.SetValue(row, (int)sender._NoteIDOrdinal, assign.Value);
									}
									PXDataFieldAssign n = new PXDataFieldAssign(sender._NoteIDName,
										PXDbType.UniqueIdentifier,
									16, assign.Value);
									n.Storage = StorageBehavior.KeyValueKey;
									pars[j].Add(n);
									noteIDRequred = false;
								}
								sender._AdjustStorage(field, assign);

								pars[j].Add(assign);
							}
							}
							break;
						}
					}
				}
			}
			for (int i = length; i < tables.Length; i++)
			{
				if (pars[i] != null && pars[i].Count > 0)
				{
					foreach (string field in links[i])
					{
						object val = sender.GetValue(row, field);
						if (baseTables.ContainsKey(tables[i]))
						{
							PXDataFieldParam baseLink;
							int tableIndex = baseTables[tables[i]];
							if ((baseLink = pars[tableIndex].FirstOrDefault(p => String.Equals(p.Column.Name, field, StringComparison.OrdinalIgnoreCase))) != null)
							{
								val = baseLink.Value;
							}
						}
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
						if (description?.Expr != null && !description.IsExcludedFromUpdate)
						{
							if (description.DataValue == null)
							{
								pars[i] = null;
								break;
							}
							PXDataFieldRestrict p = new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue);
							pars[i].Add(p);
						}
					}
				}
			}
			for (int i = 0; i < length; i++)
			{
				if (pars[i] != null && pars[i].Count > 0)
				{
					PXCache tc = sender.Graph.Caches[tables[i]];
					if (tc._NoteIDName != null)
					{
						PXDataFieldParam note = pars[i].FirstOrDefault(p => String.Equals(p.Column.Name, tc._NoteIDName, StringComparison.OrdinalIgnoreCase));
						if (note?.Value != null && sender._KeyValueAttributeNames != null)
						{
							PXDataFieldAssign n = new PXDataFieldAssign(tc._NoteIDName, PXDbType.UniqueIdentifier, 16,
								note.Value);
							n.Storage = StorageBehavior.KeyValueKey;
							pars[i].Add(n);

							object[] vals = sender.GetSlot<object[]>(row, sender._KeyValueAttributeSlotPosition);

							if (vals != null)
							{
								foreach (KeyValuePair<string, int> pair in sender._KeyValueAttributeNames)
								{
									if (pair.Value < vals.Length)
									{
										PXDataFieldAssign a =
											new PXDataFieldAssign(pair.Key,
											sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueDate ? PXDbType.DateTime
											: (sender._KeyValueAttributeTypes[pair.Key] == StorageBehavior.KeyValueNumeric ? PXDbType.Bit
											: PXDbType.NVarChar), vals[pair.Value]
											);
										a.Storage = sender._KeyValueAttributeTypes[pair.Key];
										if (a.IsChanged = audit && a.Value != null)
										{
											a.NewValue = sender.AttributeValueToString(pair.Key, a.Value);

										}
										pars[i].Add(a);
									}
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < tables.Length; i++)
			{
				if (pars[i] == null || pars[i].Count == 0)
				{
					continue;
				}
				bool success;
				try
				{
					pars[i].Add(PXDataFieldRestrict.OperationSwitchAllowed);
                    if(unchanged==null)
                        pars[i].Add(PXSelectOriginalsRestrict.SelectOriginalValues);
					success = sender.Graph.ProviderUpdate(tables[i], pars[i].ToArray());
				}
				catch (PXDbOperationSwitchRequiredException)
				{
					List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
					foreach (string field in sender.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
						if (description?.Expr != null && !description.IsExcludedFromUpdate)
						{
							if (tableMeet(description, tables[i], dialect))
							{
								ins.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					sender.Graph.ProviderInsert(tables[i], ins.ToArray());
					success = true;
				}
				if (!success)
				{
					if (i == 0 || i == timestamptable && tables[timestamptable].IsAssignableFrom(sender.GetItemType()))
					{
						throw GetLockViolationException(tables[i], pars[i].ToArray(), PXDBOperation.Update);
					}
					else
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							object val = sender.GetValue(row, field);
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
							if (description?.Expr != null && !description.IsExcludedFromUpdate)
							{
								if (tableMeet(description, tables[i], dialect))
								{
									if (ins != null)
									{
										if (description.IsRestriction && description.DataValue == null)
										{
											ins = null;
										}
										else
										{
											PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue, null);
											if (audit && val != null)
											{
												assign.IsChanged = true;
												assign.NewValue = sender.ValueToString(field, val, description.DataValue);
											}
											else assign.IsChanged = false;
											ins.Add(assign);
										}
									}
								}
							}
						}
						if (ins != null && i >= length && links[i] != null)
						{
							foreach (string field in links[i])
							{
								object val = sender.GetValue(row, field);
								if (baseTables.ContainsKey(tables[i]))
								{
									PXDataFieldParam baseLink;
									int tableIndex = baseTables[tables[i]];
									if ((baseLink = pars[tableIndex].FirstOrDefault(p => String.Equals(p.Column.Name, field, StringComparison.OrdinalIgnoreCase))) != null)
									{
										val = baseLink.Value;
									}
								}
								PXCommandPreparingEventArgs.FieldDescription description;
								sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
								if (description?.Expr != null && !description.IsExcludedFromUpdate)
								{
									if (description.DataValue == null)
									{
										ins = null;
										break;
									}
									PXDataFieldAssign assign = new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue);
									ins.Add(assign);
								}
							}
						}
						if (ins != null)
						{
							sender.Graph.ProviderInsert(tables[i], ins.ToArray());
						}
					}
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					BqlTablePair pair;
					if (sender._Originals.TryGetValue((IBqlTable)row, out pair))
					{
						if (sender._OriginalsRemoved == null)
						{
							sender._OriginalsRemoved = new PXCacheOriginalCollection();
						}
						sender._OriginalsRemoved[(IBqlTable)row] = pair;
					}
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}
        /// <exclude/>
		public override bool PersistDeleted(PXCache sender, object row)
		{
			ISqlDialect dialect = sender.Graph.SqlDialect;
			if (!_persistent)
			{
				return base.PersistDeleted(sender, row);
			}
			Type[] tables = GetTables();
			string[][] links = new string[tables.Length][];
            int?[] parents = new int?[tables.Length];
			List<PXDataFieldRestrict>[] pars = new List<PXDataFieldRestrict>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldRestrict>();
			}
			int length = tables.Length;
			for (int i = 0; i < length; i++)
			{
				Type t = tables[i];
				PXCache tc = sender.Graph.Caches[tables[i]];
				tables[i] = tc.BqlTable;
				List<Type> extensions = tc.GetExtensionTables();
				if (extensions != null)
				{
					int oldlength = extensions.Count;
					extensions = new List<Type>(extensions);
					extensions.InsertRange(0, tables);
					tables = extensions.ToArray();
					Array.Resize<string[]>(ref links, extensions.Count);
                    Array.Resize<int?>(ref parents, extensions.Count);
					Array.Resize<List<PXDataFieldRestrict>>(ref pars, extensions.Count);
					if (tc.BqlSelect is IPXExtensibleTableAttribute)
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldRestrict>();
							links[j] = ((IPXExtensibleTableAttribute)tc.BqlSelect).Keys;
                            parents[j] = i;
						}
					}
					else
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldRestrict>();
							links[j] = tc.Keys.ToArray();
						}
					}
				}
			}
            var unchanged = sender.GetOriginal(row);
			bool noteIDRequred = sender._HasKeyValueStored();
			foreach (string field in sender.Fields)
			{
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Delete, null, out description);
				if (description != null && description.Expr != null )
				{
					for (int j = 0; j < tables.Length; j++)
					{
						if (tableMeet(description, tables[j], dialect))
						{
						    if (description.IsRestriction)
						    {
						        if (pars[j] != null)
						        {
						            if (j > 0 && description.DataValue == null)
						            {
						                pars[j] = null;
						            }
						            else
						            {
						                pars[j].Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType,
						                    description.DataLength, description.DataValue));
						            }
						        }
						    }
						    else
						    {
						        var origval = unchanged.With(c => sender.GetValue(c, field));
						        PXCommandPreparingEventArgs.FieldDescription origdescription=null;
						        if(origval!=null)
						            sender.RaiseCommandPreparing(field, unchanged, origval, PXDBOperation.Update, null, out origdescription);

						        if (pars[j] != null)
						        {
						            var assign = sender.IsKvExtField(field)
						                ? new PXDataFieldRestrict(
						                (Column)description.Expr,
						                    description.DataType, description.DataLength,
						                    description.DataValue)

						                : new PXDummyDataFieldRestrict(
						                (Column)description.Expr,
						                    description.DataType, description.DataLength,
						                    unchanged != null ? origdescription?.DataValue??origval : description.DataValue);

						            if (noteIDRequred &&
						                String.Equals(field, sender._NoteIDName, StringComparison.OrdinalIgnoreCase))
						            {
						                if (assign.Value == null)
						                {
						                    assign.Value = SequentialGuid.Generate();
						                    sender.SetValue(row, (int) sender._NoteIDOrdinal, assign.Value);
						                }

						                PXDataFieldRestrict n = new PXDataFieldRestrict(sender._NoteIDName,
						                    PXDbType.UniqueIdentifier, 16, assign.Value);
						                n.Storage = StorageBehavior.KeyValueKey;
						                pars[j].Add(n);
						                noteIDRequred = false;
						            }

						            sender._AdjustStorage(field, assign);

						            pars[j].Add(assign);
						        }
						    }

						    break;
						}
					}
				}
			}
			for (int i = length; i < tables.Length; i++)
			{
				if (pars[i] != null && (parents[i] == null || pars[(int)parents[i]] != null && pars[(int)parents[i]].Count(_ => !(_ is PXDummyDataFieldRestrict)) > 0))
				{
					foreach (string field in links[i])
					{
						object val = sender.GetValue(row, field);
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
						if (description?.Expr != null&&!description.IsExcludedFromUpdate)
						{
							if (description.DataValue == null)
							{
								pars[i] = null;
								break;
							}
							PXDataFieldRestrict p = new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue);
							pars[i].Add(p);
						}
					}
				}
			}
			for (int i = tables.Length - 1; i >= 0; i--)
			{
				if (pars[i] == null || pars[i].Count(_ => !(_ is PXDummyDataFieldRestrict)) == 0)
				{
					continue;
				}
                if(unchanged==null)
                    pars[i].Add(PXSelectOriginalsRestrict.SelectOriginalValues);
				try
				{
					if (!sender.Graph.ProviderDelete(tables[i], pars[i].ToArray()) && i == 0)
					{
						throw GetLockViolationException(tables[i], pars[i].ToArray(), PXDBOperation.Delete);
					}
				}
				catch (PXDbOperationSwitchRequiredException)
				{
					List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
					foreach (string field in sender.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
						if (description?.Expr != null && !description.IsExcludedFromUpdate)
						{
							if (tableMeet(description, tables[i], dialect))
							{
								ins.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					sender.Graph.ProviderInsert(tables[i], ins.ToArray());
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}		
	}

    /// <summary>Updates values of a data record in the database according to
    /// specified policies. You can derive a custom attribute from this
    /// attribute and override the <tt>PrepareInsert()</tt> method to set
    /// other assignment behavior for target values (such as taking the
    /// maximum instead of summarizing).</summary>
    /// <remarks>
    ///   <para>You can use the attribute on its own or derive a custom attribute. Both a successor of <tt>PXAccumulator</tt> and the <tt>PXAccumulator</tt> attribute
    /// itself should be placed on the definition of a DAC.</para>
    ///   <para>To define custom policy for fields of the specified DAC, you should derive a custom class from this attribute and override the <tt>PrepareInsert()</tt>
    /// method. The method is called within the <tt>PersistInserted()</tt> method of the <tt>PXAccumulator</tt>. You can override the <tt>PersistInserted()</tt> method
    /// as well.</para>With default settings, the attribute doesn't work with tables that contain an identity column. To use the attribute on these tables, you should set
    /// to true the <tt>UpdateOnly</tt> property of the <tt>columns</tt> parameter in the <tt>PrepareInsert()</tt> method.
    /// <para>The logic of the <tt>PXAccumulator</tt> attribute works on saving of the inserted data records to the database. This process is implemented in the
    /// PersistInserted() method of the cache. This methods detects the <tt>PXAccumulator</tt>-derived attribute and calls the <tt>PersistInserted()</tt> method
    /// defined in this attribute.</para><para>When you update a data record using the attribute, you typically initialize a new instance of the DAC, set the key fields to the key values of the data
    /// record you need to update, and insert it into the cache. When a user saves changes on the webpage, or you save changes from code, your custom attribute
    /// processes these inserted data records in its own way, updating database records instead of inserting new records and applying the policies you specify.</para><para>By deriving from this attribute, you can implement an attribute that will prevent certain fields from further updates once they are initialized with
    /// values.</para></remarks>
    /// <example>
    ///   <code title="Example" description="The code below shows how the attribute can be used directly. When a data record is saved, value of every field from the first array will be added to the previously saved value of the corresponding field from the second array. That is, FinYtdBalance values will be accumulated in the FinBegBalance value, TranYtdBalance values in the TranBegBalance value, and so on." lang="CS">
    /// [PXAccumulator(
    ///     new Type[] {
    ///         typeof(CuryAPHistory.finYtdBalance),
    ///         typeof(CuryAPHistory.tranYtdBalance),
    ///         typeof(CuryAPHistory.curyFinYtdBalance),
    ///         typeof(CuryAPHistory.curyTranYtdBalance)
    ///     },
    ///     new Type[] {
    ///         typeof(CuryAPHistory.finBegBalance),
    ///         typeof(CuryAPHistory.tranBegBalance),
    ///         typeof(CuryAPHistory.curyFinBegBalance),
    ///         typeof(CuryAPHistory.curyTranBegBalance)
    ///     }
    /// )]
    /// [Serializable]
    /// public partial class CuryAPHist : CuryAPHistory
    /// { ... }</code>
    ///   <code title="Example2" description="In the next xample, the class derived from PXAccumulatorAttribute overrides the PrepareInsert() method and specifies the assignment behavior for several fields." lang="CS">
    /// public class SupplierDataAccumulatorAttribute : PXAccumulatorAttribute
    /// {
    ///     public SupplierDataAccumulatorAttribute()
    ///     {
    ///         base._SingleRecord = true;
    ///     }
    ///  
    ///     protected override bool PrepareInsert(PXCache sender, object row,
    ///                                           PXAccumulatorCollection columns)
    ///     {
    ///         if (!base.PrepareInsert(sender, row, columns))
    ///             return false;
    ///  
    ///         SupplierData bal = (SupplierData)row;
    ///         columns.Update&lt;SupplierData.supplierPrice&gt;(
    ///             bal.SupplierPrice, PXDataFieldAssign.AssignBehavior.Initialize);
    ///         columns.Update&lt;SupplierData.supplierUnit&gt;(
    ///             bal.SupplierUnit, PXDataFieldAssign.AssignBehavior.Initialize);
    ///         columns.Update&lt;SupplierData.conversionFactor&gt;(
    ///             bal.ConversionFactor, PXDataFieldAssign.AssignBehavior.Initialize);
    ///         columns.Update&lt;SupplierData.lastSupplierPrice&gt;(
    ///             bal.LastSupplierPrice, PXDataFieldAssign.AssignBehavior.Replace);
    ///         columns.Update&lt;SupplierData.lastPurchaseDate&gt;(
    ///             bal.LastPurchaseDate, PXDataFieldAssign.AssignBehavior.Replace);
    ///  
    ///         return true;
    ///     }
    /// }
    /// ...
    /// // Applying the custom attribute to a DAC
    /// [System.SerializableAttribute()]
    /// [SupplierDataAccumulator]
    /// public class SupplierData : PX.Data.IBqlTable
    /// { ... }</code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class PXAccumulatorAttribute : PXDBInterceptorAttribute
    {
        /// <exclude />
        protected BqlCommand rowselection;
        /// <exclude />
        protected Type[] _Source;
        /// <exclude />
        protected Type[] _Destination;
        /// <exclude />
        protected bool _SingleRecord;
        /// <exclude />
        protected Type _BqlTable = null;
        /// <exclude/>
		public override bool CacheSelected
        {
            get
            {
                return false;
            }
        }
        /// <summary>The value that indicates (if set to <tt>true</tt>) that the attribute always updates only a single data record.</summary>
        public virtual bool SingleRecord
        {
            get
            {
                return _SingleRecord;
            }
            set
            {
                _SingleRecord = value;
            }
        }
        /// <summary>The DAC the cache is associated with.</summary>
        public virtual Type BqlTable
        {
            get
            {
                return _BqlTable;
            }
            set
            {
                _BqlTable = value;
            }
        }
        /// <summary>Empty default constructor.</summary>
        public PXAccumulatorAttribute()
        {
        }
        /// <summary>The constructor that initializes an instance of the attribute with the source fields and destination fields.</summary>
        /// <param name="source">Fields whose values are summarized in the
        /// corresponding destination fields.</param>
        /// <param name="destination">Fields that store sums of source fields from
        /// the data records inserted into the database previously to the current
        /// data record.</param>
        /// <remarks>For example, a source field may be the transaction amount and
        /// the destination field the current balance.</remarks>
        public PXAccumulatorAttribute(Type[] source, Type[] destination)
        {
            _Source = source;
            _Destination = destination;
        }
        /// <exclude/>
        public override void CacheAttached(PXCache sender)
        {
            Type[] pars;
            if (sender.BqlKeys.Count == 0)
            {
                pars = new Type[2];
                pars[0] = typeof(Select<>);
                pars[1] = sender.GetItemType();
            }
            else if (sender.BqlKeys.Count == 1)
            {
                pars = new Type[7];
                pars[0] = typeof(Select<,>);
                pars[1] = sender.GetItemType();
                pars[2] = typeof(Where<,>);
                pars[3] = sender.BqlKeys[0];
                pars[4] = typeof(Equal<>);
                pars[5] = typeof(Required<>);
                pars[6] = sender.BqlKeys[0];
            }
            else
            {
                pars = new Type[7 + (sender.BqlKeys.Count - 1) * 5];
                pars[0] = typeof(Select<,>);
                pars[1] = sender.GetItemType();
                pars[2] = typeof(Where<,,>);
                for (int i = 0; i < sender.BqlKeys.Count; i++)
                {
                    pars[3 + 5 * i] = sender.BqlKeys[i];
                    pars[3 + 5 * i + 1] = typeof(Equal<>);
                    pars[3 + 5 * i + 2] = typeof(Required<>);
                    pars[3 + 5 * i + 3] = sender.BqlKeys[i];
                    if (i < sender.BqlKeys.Count - 2)
                    {
                        pars[3 + 5 * i + 4] = typeof(And<,,>);
                    }
                    else if (i < sender.BqlKeys.Count - 1)
                    {
                        pars[3 + 5 * i + 4] = typeof(And<,>);
                    }
                }
            }
            rowselection = BqlCommand.CreateInstance(pars);
            sender._PreventDeadlock = true;
        }
        /// <exclude/>
        public override BqlCommand GetRowCommand()
        {
            return rowselection;
        }
        /// <exclude/>
        public override BqlCommand GetTableCommand()
        {
            return null;
        }
        /// <summary>
        /// The method to override in a successor of the <tt>PXAccumulator</tt>
        /// attribute and set policies for fields.
        /// </summary>
        /// <param name="sender">The cache object into which the data record is inserted.</param>
        /// <param name="row">The data record to insert into the cache.</param>
        /// <param name="columns">The object representing columns.</param>
        /// <remarks>
        ///   <para>The method is invoked by the <tt>PersistInserted(...)</tt> method
        /// of the <tt>PXAccumulator</tt> attribute.</para>
        ///   <para>Typically, when you override this method, you call the base version
        /// of the method and set the policies for fields by calling the
        /// <tt>Update&lt;&gt;()</tt> method of the columns parameter.</para>
        /// </remarks>
        protected virtual bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
	        var getAttributes = PX.Common.Func.Memorize((string fieldName) => sender.GetAttributesReadonly(row, fieldName).ToArray());
            foreach (string descr in sender.Fields)
            {
                columns.Add(descr);
                object val = sender.GetValue(row, descr);
                if (val == null)
                {
                    continue;
                }
                int idx = sender.Keys.IndexOf(descr);
                if (idx == sender.Keys.Count - 1)
                {
                    columns.Restrict(descr, PXComp.EQ, val);
                    if (!_SingleRecord)
                    {
                        columns.RestrictPast(descr, PXComp.LT, val);
                        columns.RestrictFuture(descr, PXComp.GT, val);
                        columns.OrderBy(descr, false);
                    }
                    else
                    {
                        columns.RestrictPast(descr, PXComp.EQ, val);
                    }
                    columns.InitializeWith(descr, val);
                }
                else if (idx != -1)
                {
                    columns.Restrict(descr, PXComp.EQ, val);
                    columns.RestrictPast(descr, PXComp.EQ, val);
                    if (!_SingleRecord)
                    {
                        columns.RestrictFuture(descr, PXComp.EQ, val);
                    }
                    columns.InitializeWith(descr, val);
                }
                else if (val is decimal)
                {
                    columns.InitializeWith(descr, val);
                    if ((decimal)val != 0m)
                    {
                        columns.Update(descr, val, PXDataFieldAssign.AssignBehavior.Summarize);
                    }
                }
                else if (val is double)
                {
                    columns.InitializeWith(descr, val);
                    if ((double)val != 0.0)
                    {
                        columns.Update(descr, val, PXDataFieldAssign.AssignBehavior.Summarize);
                    }
                }
                else if (val is DateTime && getAttributes(descr).Any(a => a is PXDBLastModifiedDateTimeAttribute || a is PXDBLastChangeDateTimeAttribute)
                         || val is string && getAttributes(descr).Any(a => a is PXDBLastModifiedByScreenIDAttribute)
                         || val is Guid && getAttributes(descr).Any(a => a is PXDBLastModifiedByIDAttribute))
                {
                    columns.InitializeWith(descr, val);
                    columns.Update(descr, val, PXDataFieldAssign.AssignBehavior.Replace);
                }
                else
                {
                    columns.InitializeWith(descr, val);
                }
            }
            if (!_SingleRecord && _Source != null && _Destination != null)
            {
                for (int i = 0; i < _Source.Length && i < _Destination.Length; i++)
                {
                    columns.InitializeFrom(_Destination[i], _Source[i]);
                    columns.UpdateFuture(_Destination[i], sender.GetValue(row, _Source[i].Name));
                }
            }
            return true;
        }
        /// <summary>
        /// The method that will be executed by the cache instead of the cache's
        /// <see cref="PXCache{T}.PersistInserted(object)">PersistInserted(object)</see> method.
        /// If the attribute is attached to the cache, the cache will discover
        /// that a successor of the <tt>PXInterceptor</tt> attribute is attached,
        /// invoke the attribute's method from the standard method, and quit the
        /// standard method.
        /// </summary>
        /// <remarks>
        /// If you only need to set insertion policies for some DAC field, you should override only the
        /// <tt>PrepareInsert()</tt> method. Overriding the <tt>PersistInserted()</tt>
        /// method is needed to tweak the persist operation � for example, to catch and
        /// process errors.
        /// </remarks>
        /// <param name="sender">The cache object into which the data record is inserted.</param>
        /// <param name="row">The inserted data record to be saved to the database.</param>
        public override bool PersistInserted(PXCache sender, object row)
        {
            PXAccumulatorCollection columns = new PXAccumulatorCollection();
            if (!PrepareInsert(sender, row, columns))
            {
                return false;
            }
            List<PXAccumulatorItem> list = new List<PXAccumulatorItem>();
            int keyspos = 0;
            int initpos = 0;
            foreach (PXAccumulatorItem item in columns.Values)
            {
                if (item.OrderBy != null || item.CurrentComparison.Length > 0 || item.PastComparison.Length > 0 || item.FutureComparison.Length > 0)
                {
                    list.Add(item);
                }
                else if (item.Initializer != null && !String.IsNullOrEmpty(item.Initializer.Value.Key))
                {
                    list.Insert(initpos, item);
                    keyspos++;
                }
                else
                {
                    list.Insert(0, item);
                    keyspos++;
                    initpos++;
                }
            }
            List<PXDataFieldAssign> values = new List<PXDataFieldAssign>();
            List<PXDataField> pars = new List<PXDataField>();
            List<PXDataFieldParam> single = new List<PXDataFieldParam>();
            List<PXDataFieldParam> mass = new List<PXDataFieldParam>();
            List<PXDataField> checkfield = null;
            List<PXDataFieldParam> checkparam = null;
            List<KeyValuePair<int, KeyValuePair<int, int>>> checkindex = null;
            bool anyfuture = false;
            try
            {
                int lastkey = -1;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    PXAccumulatorItem item = list[i];
                    if (i >= keyspos)
                    {
                        if (!columns.UpdateOnly)
                        {
                            {
                                PXCommandPreparingEventArgs.FieldDescription description = null;
                                sender.RaiseCommandPreparing(item.Field, row, item.Initializer != null ? item.Initializer.Value.Value : null, PXDBOperation.Insert, null, out description);
								if (description?.Expr != null && !description.IsExcludedFromUpdate)
                                {
                                    values.Add(new PXDataFieldAssign((description.Expr as SQLTree.Column).Name, description.DataType, description.DataLength, description.DataValue));
                                }
                            }
                            bool orderset = false;
                            foreach (KeyValuePair<PXComp, object> restr in item.PastComparison)
                            {
                                PXCommandPreparingEventArgs.FieldDescription description = null;
                                sender.RaiseCommandPreparing(item.Field, row, restr.Value, PXDBOperation.Select, null, out description);
                                if (description != null && description.Expr != null)
                                {
	                                string name = (description.Expr as Column).Name;
									pars.Add(new PXDataFieldValue(name, description.DataType, description.DataLength, description.DataValue, restr.Key));
                                    if (item.OrderBy != null && !orderset)
                                    {
                                        pars.Add(new PXDataFieldOrder(name, !((bool)item.OrderBy)));
                                        orderset = true;
                                    }
                                }
                            }
                        }
                        foreach (KeyValuePair<PXComp, object> restr in item.CurrentComparison)
                        {
                            PXCommandPreparingEventArgs.FieldDescription description = null;
                            sender.RaiseCommandPreparing(item.Field, row, restr.Value, PXDBOperation.Update, null, out description);
                            if (description?.Expr != null&& !description.IsExcludedFromUpdate)
                            {
                                single.Add(new PXDataFieldRestrict((description.Expr as Column).Name, description.DataType, description.DataLength, description.DataValue, restr.Key));
                            }
                        }
                        foreach (KeyValuePair<PXComp, object> restr in item.FutureComparison)
                        {
                            PXCommandPreparingEventArgs.FieldDescription description = null;
                            sender.RaiseCommandPreparing(item.Field, row, restr.Value, PXDBOperation.Update, null, out description);
                            if (description?.Expr != null && !description.IsExcludedFromUpdate)
                            {
                                mass.Add(new PXDataFieldRestrict((description.Expr as Column).Name, description.DataType, description.DataLength, description.DataValue, restr.Key));
                            }
                        }
                    }
                    else if (!columns.UpdateOnly && item.Initializer != null)
                    {
                        if (lastkey == -1)
                        {
                            lastkey = values.Count;
                        }
                        PXCommandPreparingEventArgs.FieldDescription description = null;
                        sender.RaiseCommandPreparing(item.Field, row, item.Initializer.Value.Value, PXDBOperation.Insert, null, out description);
                        if (description?.Expr != null && !description.IsExcludedFromUpdate
							&& tableMeet(description, sender.BqlTable, sender.Graph.SqlDialect))
                        {
                            if (!description.IsRestriction)
                            {
                                values.Insert(lastkey, new PXDataFieldAssign((description.Expr as Column).Name, description.DataType, description.DataLength, description.DataValue));
                                if (i >= initpos)
                                {
                                    description = null;
                                    sender.RaiseCommandPreparing(item.Initializer.Value.Key, row, item.Initializer.Value.Value, PXDBOperation.Insert, null, out description);
                                    if (description?.Expr != null && !description.IsExcludedFromUpdate)
                                    {
	                                    pars.Insert(0, new PXDataField(description.Expr));
                                        if (item.CurrentUpdate != null || item.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Nullout)
                                        {
                                            values[lastkey].Behavior = item.CurrentUpdateBehavior;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!columns.InsertOnly && (item.CurrentUpdate != null || item.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Nullout))
                    {
                        PXCommandPreparingEventArgs.FieldDescription description = null;
                        sender.RaiseCommandPreparing(item.Field, row, item.CurrentUpdate, PXDBOperation.Update, null, out description);
                        if (description?.Expr != null && !description.IsExcludedFromUpdate)
                        {
                            if (!description.IsRestriction)
                            {
                                PXDataFieldAssign s = new PXDataFieldAssign((description.Expr as Column).Name, description.DataType, description.DataLength, description.DataValue);
                                s.Behavior = item.CurrentUpdateBehavior;
                                single.Add(s);
                            }
                        }
                    }
                    if (item.FutureUpdate != null)
                    {
                        PXCommandPreparingEventArgs.FieldDescription description = null;
                        sender.RaiseCommandPreparing(item.Field, row, item.FutureUpdate, PXDBOperation.Update, null, out description);
                        if (description?.Expr != null && !description.IsExcludedFromUpdate)
                        {
                            if (!description.IsRestriction)
                            {
                                PXDataFieldAssign m = new PXDataFieldAssign((description.Expr as Column).Name, description.DataType, description.DataLength, description.DataValue);
                                m.Behavior = item.FutureUpdateBehavior;
                                mass.Add(m);
                                anyfuture = true;
                            }
                        }
                    }
                }
                if (columns.Exceptions != null && columns.Exceptions.Count > 0)
                {
                    checkparam = new List<PXDataFieldParam>();
                    checkfield = new List<PXDataField>();
                    checkindex = new List<KeyValuePair<int, KeyValuePair<int, int>>>();
                    int start = 0;
                    for (int i = 0; i < columns.Exceptions.Count; i++)
                    {
                        int finish = start;
                        for (int j = 0; j < columns.Exceptions[i].Value.Length; j++)
                        {
                            PXCommandPreparingEventArgs.FieldDescription description = null;
                            sender.RaiseCommandPreparing(columns.Exceptions[i].Value[j].FieldName, row, columns.Exceptions[i].Value[j].Value, PXDBOperation.Update, null, out description);
							if (description?.Expr != null && !description.IsExcludedFromUpdate)
							{
								var col = (Column) description.Expr;
                                PXDataFieldRestrict r = new PXDataFieldRestrict(col.Name, description.DataType, description.DataLength, description.DataValue, columns.Exceptions[i].Value[j].Comp);
                                if (j == 0)
                                {
                                    r.OpenBrackets++;
                                }
                                else
                                {
                                    r.OrOperator = true;
                                }
                                if (j == columns.Exceptions[i].Value.Length - 1)
                                {
                                    r.CloseBrackets++;
                                }
                                r.CheckResultOnly = true;
                                checkparam.Add(r);
								// using col.Name (column name without enqoute) enstead of r.FieldName (has enqouted column name).
								PXDataFieldValue v = new PXDataFieldValue(col.Name, r.ValueType, r.ValueLength, r.Value, r.Comp);
                                v.OpenBrackets = r.OpenBrackets;
                                v.CloseBrackets = r.CloseBrackets;
                                v.OrOperator = r.OrOperator;
                                v.CheckResultOnly = true;
                                checkfield.Add(v);
                                finish++;
                            }
                        }
                        if (start != finish)
                        {
                            checkindex.Add(new KeyValuePair<int, KeyValuePair<int, int>>(i, new KeyValuePair<int, int>(start, finish - 1)));
                        }
                        start = finish;
                    }
                    if (checkparam.Count == 0)
                    {
                        checkparam = null;
                    }
                }
            }
            catch (PXCommandPreparingException e)
            {
                if (sender.RaiseExceptionHandling(e.Name, row, e.Value, e))
                {
                    throw;
                }
                return false;
            }
            PXDBOperation oper = PXDBOperation.Insert;
            try
            {
                int parscount = pars.Count;
                int singlecount = single.Count;
                if (checkparam != null)
                {
                    pars.AddRange(checkfield);
                }
                if ((parscount == 0 || !sender.Graph.ProviderEnsure(_BqlTable ?? sender.BqlTable, values.ToArray(), pars.ToArray())) && single.Count > 0)
                {
                    oper = PXDBOperation.Update;
                    if (checkparam != null)
                    {
                        single.AddRange(checkparam);
                    }
					if (!sender.Graph.ProviderUpdate(_BqlTable ?? sender.BqlTable, single.Concat(new[] { PXSelectOriginalsRestrict.SelectOriginalValues }).ToArray()))
                    {
                        if (checkparam != null)
                        {
                            for (int i = 1; i <= checkindex.Count; i++)
                            {
                                for (int j = 0; j < Math.Pow(2.0, checkindex.Count) && j < int.MaxValue - 1; j++)
                                {
                                    int m = j;
                                    int count = 0;
                                    {
                                        for (int k = 0; k < checkindex.Count; k++)
                                        {
                                            if ((m & 1) == 0)
                                            {
                                                count++;
                                            }
                                            m = m >> 1;
                                        }
                                    }
                                    if (count == i)
                                    {
                                        pars = pars.GetRange(0, parscount);
                                        single = single.GetRange(0, singlecount);
                                        int errornbr = -1;
                                        m = j;
                                        for (int k = 0; k < checkindex.Count; k++)
                                        {
                                            if ((m & 1) == 1)
                                            {
                                                pars.AddRange(checkfield.GetRange(checkindex[k].Value.Key, checkindex[k].Value.Value - checkindex[k].Value.Key + 1));
                                                single.AddRange(checkparam.GetRange(checkindex[k].Value.Key, checkindex[k].Value.Value - checkindex[k].Value.Key + 1));
                                            }
                                            else if (errornbr == -1)
                                            {
                                                errornbr = k;
                                            }
                                            m = m >> 1;
                                        }
                                        if (sender.Graph.ProviderEnsure(_BqlTable ?? sender.BqlTable, values.ToArray(), pars.ToArray())
											|| single.Count > 0 && sender.Graph.ProviderUpdate(_BqlTable ?? sender.BqlTable, single.Concat(new[] { PXSelectOriginalsRestrict.SelectOriginalValues }).ToArray()))
                                        {
                                            throw new PXRestrictionViolationException(columns.Exceptions[checkindex[errornbr].Key].Key, getKeys(sender, row), checkindex[errornbr].Key);
                                        }
                                    }
                                }
                            }
                        }
						throw GetLockViolationException(_BqlTable ?? sender.BqlTable, single.ToArray(), PXDBOperation.Update);
					}
                }
                sender.RaiseRowPersisted(row, oper, PXTranStatus.Open, null);
                if (anyfuture)
                {
                    sender.Graph.ProviderUpdate(_BqlTable ?? sender.BqlTable, mass.Concat(new[] { PXSelectOriginalsRestrict.SelectOriginalValues }).ToArray());
                }
            }
            catch (PXDatabaseException e)
            {
                e.Keys = getKeys(sender, row);
                if (e.ErrorCode == PXDbExceptions.Timeout)
                {
                    e.Retry = true;
                }
                throw;
            }
            return true;
        }
        /// <exclude/>
        public override bool PersistUpdated(PXCache sender, object row)
        {
            List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
            try
            {
                foreach (string descr in sender.Fields)
                {
                    PXCommandPreparingEventArgs.FieldDescription description = null;
                    sender.RaiseCommandPreparing(descr, row, sender.GetValue(row, descr), PXDBOperation.Update, null, out description);
                    if (description?.Expr != null&& !description.IsExcludedFromUpdate)
                    {
                        if (description.IsRestriction)
                        {
                            pars.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
                        }
                        else
                        {
                            pars.Add(new PXDataFieldAssign((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
                        }
                    }
                }
            }
            catch (PXCommandPreparingException e)
            {
                if (sender.RaiseExceptionHandling(e.Name, row, e.Value, e))
                {
                    throw;
                }
                return false;
            }
            try
            {
                if (!sender.Graph.ProviderUpdate(sender.BqlTable, pars.Concat(new[] { PXSelectOriginalsRestrict.SelectOriginalValues }).ToArray()))
                {
					throw GetLockViolationException(sender.BqlTable, pars.ToArray(), PXDBOperation.Update);
				}
            }
            catch (PXDatabaseException e)
            {
                e.Keys = getKeys(sender, row);
                throw;
            }
            return true;
        }
        /// <exclude/>
        public override bool PersistDeleted(PXCache sender, object row)
        {
            List<PXDataFieldRestrict> pars = new List<PXDataFieldRestrict>();
            try
            {
                foreach (string descr in sender.Fields)
                {
                    PXCommandPreparingEventArgs.FieldDescription description = null;
                    sender.RaiseCommandPreparing(descr, row, sender.GetValue(row, descr), PXDBOperation.Delete, null, out description);
                    if (description != null && description.Expr != null)
                    {
                        if (description.IsRestriction)
                        {
                            pars.Add(new PXDataFieldRestrict((Column)description.Expr, description.DataType, description.DataLength, description.DataValue));
                        }
                    }
                }
            }
            catch (PXCommandPreparingException e)
            {
                if (sender.RaiseExceptionHandling(e.Name, row, e.Value, e))
                {
                    throw;
                }
                return false;
            }
            try
            {
                if (!sender.Graph.ProviderDelete(sender.BqlTable, pars.Concat(new[] { PXSelectOriginalsRestrict.SelectOriginalValues }).ToArray()))
                {
					throw GetLockViolationException(sender.BqlTable, pars.ToArray(), PXDBOperation.Delete);
				}
                sender.RaiseRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
            }
            catch (PXDatabaseException e)
            {
                e.Keys = getKeys(sender, row);
                throw;
            }
            return true;
        }
        /// <exclude/>
        public override object Insert(PXCache sender, object row)
        {
            object existing = sender.Locate(row);
            if (existing != null)
            {
                if (sender.GetStatus(existing) == PXEntryStatus.Inserted)
                {
                    sender.Current = existing;
                    return existing;
                }
                sender.Remove(existing);
                return base.Insert(sender, row);
            }
            return base.Insert(sender, row);
        }
    }

    public sealed class PXAccumulatorItem
    {
        private string _Field;
        private List<KeyValuePair<PXComp, object>> _PastComparison;
        private List<KeyValuePair<PXComp, object>> _CurrentComparison;
        private List<KeyValuePair<PXComp, object>> _FutureComparison;
        private string _InitializeFrom;
		private bool _HasInitializeValue;
        private object _InitializeWith;
        private bool? _OrderPast;
        private object _CurrentUpdate;
        private PXDataFieldAssign.AssignBehavior _CurrentUpdateBehavior;
        private object _FutureUpdate;
        private PXDataFieldAssign.AssignBehavior _FutureUpdateBehavior;
        public PXAccumulatorItem(string field)
        {
            _Field = field;
        }
        public void RestrictPast(PXComp comparison, object value)
        {
            if (_PastComparison == null)
            {
                _PastComparison = new List<KeyValuePair<PXComp, object>>();
            }
            _PastComparison.Add(new KeyValuePair<PXComp, object>(comparison, value));
        }
        public void OrderPast(bool ascending)
        {
            _OrderPast = ascending;
        }
        public void RestrictCurrent(PXComp comparison, object value)
        {
            if (_CurrentComparison == null)
            {
                _CurrentComparison = new List<KeyValuePair<PXComp, object>>();
            }
            _CurrentComparison.Add(new KeyValuePair<PXComp, object>(comparison, value));
        }
        public void RestrictFuture(PXComp comparison, object value)
        {
            if (_FutureComparison == null)
            {
                _FutureComparison = new List<KeyValuePair<PXComp, object>>();
            }
            _FutureComparison.Add(new KeyValuePair<PXComp, object>(comparison, value));
        }
        public void InitializeFrom(string field)
        {
            _InitializeFrom = field;
        }
        public void InitializeWith(object value)
        {
            _InitializeWith = value;
			_HasInitializeValue = true;
        }
        public void UpdateCurrent(object value)
        {
            UpdateCurrent(value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void UpdateCurrent(object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            _CurrentUpdate = value;
			_CurrentUpdateBehavior = value != null || behavior != PXDataFieldAssign.AssignBehavior.Replace ? behavior : PXDataFieldAssign.AssignBehavior.Nullout;
        }
        public void UpdateFuture(object value)
        {
            _FutureUpdate = value;
            _FutureUpdateBehavior = PXDataFieldAssign.AssignBehavior.Summarize;
        }
        public void UpdateFuture(object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            _FutureUpdate = value;
			_FutureUpdateBehavior = value != null || behavior != PXDataFieldAssign.AssignBehavior.Replace ? behavior : PXDataFieldAssign.AssignBehavior.Nullout;
		}
        public string Field
        {
            get
            {
                return _Field;
            }
        }
        public KeyValuePair<PXComp, object>[] PastComparison
        {
            get
            {
                if (_PastComparison == null || _PastComparison.Count == 0)
                {
                    return new KeyValuePair<PXComp, object>[0];
                }
                return _PastComparison.ToArray();
            }
        }
        public KeyValuePair<PXComp, object>[] CurrentComparison
        {
            get
            {
                if (_CurrentComparison == null || _CurrentComparison.Count == 0)
                {
                    return new KeyValuePair<PXComp, object>[0];
                }
                return _CurrentComparison.ToArray();
            }
        }
        public KeyValuePair<PXComp, object>[] FutureComparison
        {
            get
            {
                if (_FutureComparison == null || _FutureComparison.Count == 0)
                {
                    return new KeyValuePair<PXComp, object>[0];
                }
                return _FutureComparison.ToArray();
            }
        }
        public KeyValuePair<string, object>? Initializer
        {
            get
            {
                if (_InitializeFrom == null && !_HasInitializeValue)
                {
                    return null;
                }
                return new KeyValuePair<string, object>(_InitializeFrom, _InitializeWith);
            }
        }
        public bool? OrderBy
        {
            get
            {
                return _OrderPast;
            }
        }
        public object CurrentUpdate
        {
            get
            {
                return _CurrentUpdate;
            }
        }
        public object FutureUpdate
        {
            get
            {
                return _FutureUpdate;
            }
        }
        public PXDataFieldAssign.AssignBehavior CurrentUpdateBehavior
        {
            get
            {
                return _CurrentUpdateBehavior;
            }
        }
        public PXDataFieldAssign.AssignBehavior FutureUpdateBehavior
        {
            get
            {
                return _FutureUpdateBehavior;
            }
        }
    }

	public class PXAccumulatorRestriction
	{
		public readonly string FieldName;
		public readonly PXComp Comp;
		public readonly object Value;
		public PXAccumulatorRestriction(string fieldName, PXComp comp, object value)
		{
			FieldName = fieldName;
			Comp = comp;
			Value = value;
		}
	}

	public class PXAccumulatorRestriction<Field> : PXAccumulatorRestriction
		where Field : IBqlField
	{
		public PXAccumulatorRestriction(PXComp comp, object value)
			: base(typeof(Field).Name, comp, value)
		{
		}
	}

    /// <summary>Represents a collection of settings for individual fields
    /// processed by the <see cref="PXAccumulatorAttribute">PXAccumulator</see>
    /// attribute.</summary>
    /// <remarks>The type is used by the <tt>PXAccumulator</tt> attribute in the <see cref="PXAccumulatorAttribute.PrepareInsert(PXCache, object, PXAccumulatorCollection)"> PrepareInsert</see>
    /// method. You can use the columns parameters to set updating policies,</remarks>
    public sealed class PXAccumulatorCollection : Dictionary<string, PXAccumulatorItem>
    {
        /// <exclude />
        private List<KeyValuePair<string, PXAccumulatorRestriction[]>> _Exceptions;
        public List<KeyValuePair<string, PXAccumulatorRestriction[]>> Exceptions
        {
            get
            {
                return _Exceptions;
            }
        }
        /// <exclude />
        private bool _InsertOnly;
        /// <summary>Gets or sets the value that indicates whether the attribute
        /// is allowed only to insert new data records in the database table and
        /// is not allowed to update them.</summary>
        public bool InsertOnly
        {
            get
            {
                return _InsertOnly;
            }
            set
            {
                _InsertOnly = value;
            }
        }
        /// <exclude />
        private bool _UpdateOnly;
        /// <summary>Gets or sets the value that indicates whether the attribute
        /// is allowed only to update existing data records in the database table
        /// and is not allowed to insert new.</summary>
		public bool UpdateOnly
        {
            get
            {
                return _UpdateOnly;
            }
            set
            {
                _UpdateOnly = value;
            }
        }
        public PXAccumulatorCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        /// <summary>Adds a node for the specified field into the collection. The
        /// field is specified through the type parameter.</summary>
        /// <param name="bqlField">The BQL type of the field.</param>
        public void Add<Field>()
            where Field : IBqlField
        {
            Add(typeof(Field).Name);
        }
        /// <summary>Adds a node for the specified field into the
        /// collection.</summary>
        /// <param name="bqlField">The BQL type of the field.</param>
        public void Add(Type bqlField)
        {
            Add(bqlField.Name);
        }
        /// <summary>Adds a node for the specified field into the
        /// collection.</summary>
        /// <param name="bqlField">The BQL type of the field.</param>
        public void Add(string field)
        {
            base.Add(field, new PXAccumulatorItem(field));
        }
        /// <summary>Remove the setting for the specified field from the
        /// collection. The field is specified through the type
        /// parameter.</summary>
        public void Remove<Field>()
            where Field : IBqlField
        {
            Remove(typeof(Field).Name);
        }
        /// <summary>Remove the setting for the specified field from the
        /// collection.</summary>
        /// <param name="bqlField">The BQL type of the field.</param>
        public void Remove(Type bqlField)
        {
            Remove(bqlField.Name);
        }
        /// <summary>Remove the setting for the specified field from the
        /// collection.</summary>
        /// <param name="field">The name of the field.</param>
        public new void Remove(string field)
        {
            base.Remove(field);
        }
        /// <summary>Configures update of the specified field as addition of the
        /// new value to the value kept in the database. The field is specified
        /// through the type parameter.</summary>
        /// <param name="value">The new value of the field.</param>
        public void Update<Field>(object value)
            where Field : IBqlField
        {
            Update(typeof(Field).Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        /// <summary>The field is specified through the type parameter.</summary>
        /// <param name="value">The new value of the field.</param>
        /// <param name="behavior">The <see cref="PXDataFieldAssign.AssignBehavior"/>
        /// value that specifies how the new value of the field is combined with
        /// the database value.</param>
        public void Update<Field>(object value, PXDataFieldAssign.AssignBehavior behavior)
            where Field : IBqlField
        {
            Update(typeof(Field).Name, value, behavior);
        }
        /// <summary>Configures update of the specified field as addition of the
        /// new value to the value kept in the database.</summary>
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="value">The new value of the field.</param>
        public void Update(Type bqlField, object value)
        {
            Update(bqlField.Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="value">The new value of the field.</param>
        /// <param name="behavior">The <see cref="PXDataFieldAssign.AssignBehavior"/>
        /// value that specifies how the new value of the field is combined with
        /// the database value.</param>
        public void Update(Type bqlField, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            Update(bqlField.Name, value, behavior);
        }
        /// <summary>Configures update of the specified field as addition of the
        /// new value to the value kept in the database.</summary>
        /// <param name="field">The name of the field.</param>
        /// <param name="value">The new value of the field.</param>
        public void Update(string field, object value)
        {
            Update(field, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="value">The new value of the field.</param>
        /// <param name="behavior">The <see cref="PXDataFieldAssign.AssignBehavior"/>
        /// value that specifies how the new value of the field is combined with
        /// the database value.</param>
        public void Update(string field, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            PXAccumulatorItem item = base[field];
            item.UpdateCurrent(value, behavior);
        }
        /// <param name="value">The new value of the field.</param>
        public void UpdateFuture<Field>(object value)
            where Field : IBqlField
        {
            UpdateFuture(typeof(Field).Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        /// <param name="value">The new value of the field.</param>
        /// <param name="behavior">The <see cref="PXDataFieldAssign.AssignBehavior" />
        /// value that specifies how the new value of the field is combined with
        /// the database value.</param>
        public void UpdateFuture<Field>(object value, PXDataFieldAssign.AssignBehavior behavior)
            where Field : IBqlField
        {
            UpdateFuture(typeof(Field).Name, value, behavior);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="value">The new value of the field.</param>
        public void UpdateFuture(Type bqlField, object value)
        {
            UpdateFuture(bqlField.Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="value">The new value of the field.</param>
        /// <param name="behavior">The <see cref="PXDataFieldAssign.AssignBehavior"/>
        /// value that specifies how the new value of the field is combined with
        /// the database value.</param>
        public void UpdateFuture(Type bqlField, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            UpdateFuture(bqlField.Name, value, behavior);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="value">The new value of the field.</param>
        public void UpdateFuture(string field, object value)
        {
            UpdateFuture(field, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="value">The new value of the field.</param>
        /// <param name="behavior">The <see cref="PXDataFieldAssign.AssignBehavior"/>
        /// value that specifies how the new value of the field is combined with
        /// the database value.</param>
        public void UpdateFuture(string field, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            PXAccumulatorItem item = base[field];
            item.UpdateFuture(value, behavior);
        }
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void Restrict<Field>(PXComp comparison, object value)
            where Field : IBqlField
        {
            Restrict(typeof(Field).Name, comparison, value);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="comparison">The PXComp value that specifies the type of
        /// comparison in the condition.</param>
        /// <param name="value">The new value of the field.</param>
        public void Restrict(Type bqlField, PXComp comparison, object value)
        {
            Restrict(bqlField.Name, comparison, value);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void Restrict(string field, PXComp comparison, object value)
        {
            PXAccumulatorItem item = base[field];
            item.RestrictCurrent(comparison, value);
        }
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void RestrictPast<Field>(PXComp comparison, object value)
            where Field : IBqlField
        {
            RestrictPast(typeof(Field).Name, comparison, value);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void RestrictPast(Type bqlField, PXComp comparison, object value)
        {
            RestrictPast(bqlField.Name, comparison, value);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void RestrictPast(string field, PXComp comparison, object value)
        {
            PXAccumulatorItem item = base[field];
            item.RestrictPast(comparison, value);
        }
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void RestrictFuture<Field>(PXComp comparison, object value)
            where Field : IBqlField
        {
            RestrictFuture(typeof(Field).Name, comparison, value);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void RestrictFuture(Type bqlField, PXComp comparison, object value)
        {
            RestrictFuture(bqlField.Name, comparison, value);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="comparison"></param>
        /// <param name="value">The new value of the field.</param>
        public void RestrictFuture(string field, PXComp comparison, object value)
        {
            PXAccumulatorItem item = base[field];
            item.RestrictFuture(comparison, value);
        }
        /// <summary>The targer field and the source fields are specified through
        /// the type parameters.</summary>
        public void InitializeFrom<Field, Source>()
            where Field : IBqlField
            where Source : IBqlField
        {
            InitializeFrom(typeof(Field).Name, typeof(Source).Name);
        }
        /// <summary>The field is specified through the type parameter.</summary>
        /// <param name="bqlField">The BQL type of the field.</param>
        public void InitializeFrom<Field>(Type source)
            where Field : IBqlField
        {
            InitializeFrom(typeof(Field).Name, source.Name);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="source"></param>
        public void InitializeFrom(Type bqlField, Type source)
        {
            InitializeFrom(bqlField.Name, source.Name);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="source"></param>
        public void InitializeFrom(string field, string source)
        {
            PXAccumulatorItem item = base[field];
            item.InitializeFrom(source);
        }
        /// <summary>The field is specified through the type parameter.</summary>
        /// <param name="value">The new value.</param>
        public void InitializeWith<Field>(object value)
            where Field : IBqlField
        {
            InitializeWith(typeof(Field).Name, value);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="value">The new value.</param>
        public void InitializeWith(Type bqlField, object value)
        {
            InitializeWith(bqlField.Name, value);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="value">The new value.</param>
        public void InitializeWith(string field, object value)
        {
            PXAccumulatorItem item = base[field];
            item.InitializeWith(value);
        }
        /// <param name="ascending">The value indicating whether data records are
        /// sorted in the ascending order.</param>
        public void OrderBy<Field>(bool ascending)
            where Field : IBqlField
        {
            OrderBy(typeof(Field).Name, ascending);
        }
        /// <param name="bqlField">The BQL type of the field.</param>
        /// <param name="ascending">The value indicating whether data records are
        /// sorted in the ascending order.</param>
        public void OrderBy(Type bqlField, bool ascending)
        {
            OrderBy(bqlField.Name, ascending);
        }
        /// <param name="field">The name of the field.</param>
        /// <param name="ascending">The value indicating whether data records are
        /// sorted in the ascending order.</param>
        public void OrderBy(string field, bool ascending)
        {
            PXAccumulatorItem item = base[field];
            item.OrderPast(ascending);
        }
        /// <param name="message"></param>
        /// <param name="params exception"></param>
		public void AppendException(string message, params PXAccumulatorRestriction[] exception)
        {
            if (exception.Length > 0)
            {
                if (_Exceptions == null)
                {
                    _Exceptions = new List<KeyValuePair<string, PXAccumulatorRestriction[]>>();
                }
                _Exceptions.Add(new KeyValuePair<string, PXAccumulatorRestriction[]>(message, exception));
            }
        }
    }

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class PXDynamicAggregateAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXRowSelectedSubscriber, 
		IPXRowUpdatingSubscriber, IPXRowUpdatedSubscriber, IPXRowInsertingSubscriber, IPXRowInsertedSubscriber, 
		IPXRowDeletingSubscriber, IPXRowDeletedSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, 
		IPXFieldDefaultingSubscriber, IPXFieldSelectingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldUpdatedSubscriber, 
		IPXFieldVerifyingSubscriber
	{
		public delegate IEnumerable<PXEventSubscriberAttribute> GetAttributes(string fieldName);

		private readonly GetAttributes _getAttributesHandler;

		public PXDynamicAggregateAttribute(GetAttributes handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			_getAttributesHandler = handler;
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowSelectingSubscriber>())
				attribute.RowSelecting(sender, e);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowSelectedSubscriber>())
				attribute.RowSelected(sender, e);
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowUpdatingSubscriber>())
				attribute.RowUpdating(sender, e);
		}

		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowUpdatedSubscriber>())
				attribute.RowUpdated(sender, e);
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowInsertingSubscriber>())
				attribute.RowInserting(sender, e);
		}

		public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowInsertedSubscriber>())
				attribute.RowInserted(sender, e);
		}

		public void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowDeletingSubscriber>())
				attribute.RowDeleting(sender, e);
		}

		public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowDeletedSubscriber>())
				attribute.RowDeleted(sender, e);
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowPersistingSubscriber>())
				attribute.RowPersisting(sender, e);
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowPersistedSubscriber>())
				attribute.RowPersisted(sender, e);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldDefaultingSubscriber>())
				attribute.FieldDefaulting(sender, e);
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldSelectingSubscriber>())
				attribute.FieldSelecting(sender, e);
		}

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldUpdatingSubscriber>())
				attribute.FieldUpdating(sender, e);
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldUpdatedSubscriber>())
				attribute.FieldUpdated(sender, e);
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldVerifyingSubscriber>())
				attribute.FieldVerifying(sender, e);
		}

		private IEnumerable<T> Attributes<T>() 
			where T : class
		{
			foreach (var attribute in _getAttributesHandler(_FieldName))
			{
				var typedAtt = attribute as T;
				if (typedAtt != null) yield return typedAtt;
			}
		}
    }

    #region PXClassAttribute
    public abstract class PXClassAttribute : Attribute
    {
        public virtual void CacheAttached(PXCache sender)
        { 
        }

		public virtual PXClassAttribute Clone()
		{
			PXClassAttribute attr = (PXClassAttribute)MemberwiseClone();
			return attr;
		}
	}
    #endregion

    #region PXDisableCloneAttributesAttribute
    /// <summary>
    /// Disables cloning of the cache-level attributes for a DAC.
    /// </summary>
    /// <remarks>
    /// <para>The attribute is placed on a DAC to prevent creation of
    /// item-level attributes of a cache. The cache creates item-level
    /// attributes by copying cache-level attributes, for example,
    /// when an attribute is modified for a specific data record.</para>
    /// <para>The attribute is not used with DACs whose instances
    /// (data records) can be modified in the UI. Typically, you place
    /// the attribute on DACs representing history and status tables
    /// used in processing operations and accumulator attributes.</para>
    /// </remarks>
    /// <example>
    /// The code below shows the usage of the <tt>PXDisableCloneAttributes</tt>
    /// attribute on a DAC.
    /// <code>
    /// [ItemStatsAccumulator()]
    /// [PXDisableCloneAttributes()]
    /// [Serializable]
    /// public partial class ItemStats : INItemStats
    /// { ... }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class PXDisableCloneAttributesAttribute : PXClassAttribute
    {
        /// <exclude/>
        public override void CacheAttached(PXCache sender)
        {
            sender.DisableCloneAttributes = true;
        }
    }
    #endregion

	#region PXTableNameAttribute
	[AttributeUsage(AttributeTargets.Class)]
	public class PXTableNameAttribute : Attribute
	{
		public virtual bool IsActive
		{
			get
			{
				return true;
			}
		}
	}
	#endregion

	#region IPXLocalizableList
	//Can be inherited by attributes which are used on properties of IBqlTable class
	//If IsLocalizable is false list values won't be collected for localization
	public interface IPXLocalizableList
	{
		bool IsLocalizable { get; set; }
	}
	#endregion

	#region PXKeyValuePairAttribute
	public class PXKeyValueStorageAttribute : Attribute
	{
	}
	#endregion
}

