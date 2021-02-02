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

using PX.Api.Soap.Screen;
using PX.Common;
using System.Collections.Concurrent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using PX.Common.Collection;
using PX.Data.ReferentialIntegrity;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.ReferentialIntegrity.Inspecting;
using PX.Data.SQLTree;

namespace PX.Data
{
	#region PXSelectorAttribute
    /// <summary>Configures the lookup control for a DAC field that references a data record from a particular table by holding its key field.See</summary>
    /// <remarks>
    ///   <para>The attribute configures the input control for a DAC field that references a data record from a particular table. Such field holds a key value that
    /// identifies the data record in this table.</para>
    ///   <para>The input control will be of "lookup" type (may also be called a "selector"). A user can either input the value for the field manually or select from the
    /// list of the data records. If a value is inserted manually, the attribute checks if it is included in the list. You can specify a complex BQL query to define
    /// the set of data records that appear in the list.</para>
    ///   <para>The key field usually represents a database identity column that may not be user-friendly (surrogate key). It is possible to substitute its value with the
    /// value of another field from the same data record (natural key). This field should be specified in the <tt>SubstituteKey</tt> property. In this case, the table,
    /// and the DAC, have two fields that uniquely identify a data record from this table. For example, the <tt>Account</tt> table may have the numeric
    /// <tt>AccountID</tt> field and the user-friendly string <tt>AccountCD</tt> field. On a field that references <tt>Account</tt> data records in another DAC, you
    /// should place the <tt>PXSelector</tt> attribute as follows.</para>
    ///   <code>[PXSelector(typeof(Search&lt;Account.accountID&gt;), SubstituteKey =
    /// typeof(Account.accountCD))]</code>
    ///   <para>The attribute will automatically convert the stored numeric value to the displayed string value and back. Note that only the <tt>AccountCD</tt> property
    /// should be marked with <tt>IsKey</tt> property set to <tt>true</tt>.</para>
    ///   <para>It is also possible to define the list of columns to display. You can use an appropriated constructor and specify the types of the fields. By default, all
    /// fields that have the <tt>PXUIField</tt> attribute's <tt>Visibility</tt> property set to <tt>PXUIVisibility.SelectorVisible</tt>.</para>
    ///   <para>Along with a key, some other field can be displayed as the description of the key. This field should be specified in the <tt>DescriptionField</tt> property.
    /// The way the description is displayed in the lookup control is configured in the webpage layout through the <tt>DisplayMode</tt> property of the
    /// <tt>PXSelector</tt> control. The default display format is <i>ValueField – DescriptionField</i>. It can be changed to display the description only.</para>
    ///   <para>To achieve better performance, the attribute can be configured to cache the displayed data records.</para>
    /// </remarks>
    /// <example>
    /// 	<para></para>
    /// 	<code title="Example" description="The example below shows the simplest PXSelector attribute declaration. All Category data records will be available for selection. Their CategoryCD field values will be inserted without conversion." lang="CS">
    /// [PXSelector(typeof(Category.categoryCD))]
    /// public virtual string CategoryCD { get; set; }</code>
    /// 	<code title="Example2" description="The attribute below configures the lookup control to let the user select from the Customer data records retrieved by the Search BQL query. The displayed columns are specified explicitly: AccountCD and CompanyName." groupname="Example" lang="CS">
    /// [PXSelector(
    ///     typeof(Search&lt;Customer.accountCD, 
    ///                Where&lt;Customer.companyType, Equal&lt;CompanyType.customer&gt;&gt;&gt;),
    ///     new Type[] 
    ///     {
    ///         typeof(Customer.accountCD),
    ///         typeof(Customer.companyName)
    ///     })]
    /// public virtual string AccountCD { get; set; }</code>
    ///   <code title="Example3" description="The Customer.accountCD field data will be inserted as a value without conversion. The attribute below let the user select from the Branch data records. The attribute displays the Branch.BranchCD field value in the user interface, but actually assigns the Branch.BranchID field value to the field." groupname="Example2" lang="CS">
    /// [PXSelector(typeof(Branch.branchID),
    ///             SubstituteKey = typeof(Branch.branchCD))]
    /// public virtual int? BranchID { get; set; }</code>
    /// 	<code title="Example4" description="The example below shows the PXSelector attribute in combination with other attributes. Here, the PXSelector attribute configures a lookup field that will let a user select from the data set defined by the Search query. The lookup control will display descriptions the data records, taking them from CRLeadClass.description field. The attribute will cache records in memory to reduce the number of database calls." groupname="Example3" lang="CS">
    /// [PXDBString(10, IsUnicode = true, InputMask = "&gt;aaaaaaaaaa")]
    /// [PXUIField(DisplayName = "Class ID")]
    /// [PXSelector(
    ///     typeof(Search&lt;CRLeadClass.cRLeadClassID,
    ///                Where&lt;CRLeadClass.isActive, Equal&lt;True&gt;&gt;&gt;),
    ///     DescriptionField = typeof(CRLeadClass.description),
    ///     CacheGlobal = true)]
    /// public virtual string ClassID { get; set; }</code>
    /// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXSelectorAttribute))]
	public class PXSelectorAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber, IPXFieldSelectingSubscriber, IPXDependsOnFields
	{
		#region Nested types

	    private class PXSelectorFilterView : PXFilterView
	    {
		    private readonly string _alias;

		    public PXSelectorFilterView(PXGraph graph, PXSelectorAttribute selector) 
				: base(graph, FilterHeader.SelectorScreenID, GetViewName(GetAlias(selector)))
		    {
				graph.CommandPreparing.AddHandler<FilterRow.dataField>(FilterRow_DataField_CommandPreparing);
			    _alias = GetAlias(selector);
		    }

		    private static string GetAlias(PXSelectorAttribute selector)
		    {
			    return (selector._FilterEntity ?? selector._Type).Name;
		    }

		    private static string GetViewName(string alias)
		    {
			    return String.Concat("_", alias, "_");
		    }

		    private void FilterRow_DataField_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		    {
				string oldValue = e.Value as string;
			    if (e.Row != null && !String.IsNullOrEmpty(oldValue) && !oldValue.Contains("__"))
			    {
                    FilterHeader parent = PXParentAttribute.SelectParent(sender, e.Row, typeof(FilterHeader)) as FilterHeader;
				    if (parent != null && String.Equals(parent.ViewName, GetViewName(_alias), StringComparison.Ordinal))
				    {
					    e.Value = String.Concat(_alias, "__", oldValue);
				    }
			    }
		    }
	    }

		#endregion

		#region State
		public bool IsPrimaryViewCompatible { get; set; }
		protected Type _Type;
		protected Type _BqlType;
	    protected Type _FilterEntity;
		protected Type _CacheType;
		protected BqlCommand _Select;
		/// <summary>
		/// Returns Bql command used for selection of referenced records.
		/// </summary>
        /// <exclude/>
		public virtual BqlCommand GetSelect() => _Select;
		protected int _ParsCount;
		protected int _ParsSimpleCount;
		protected BqlCommand _PrimarySelect;
		protected internal BqlCommand _PrimarySimpleSelect;
		protected BqlCommand _OriginalSelect;
		protected BqlCommand _NaturalSelect;
		protected BqlCommand _LookupSelect;
		protected BqlCommand _UnconditionalSelect;
		protected string[] _FieldList;
		protected string[] _HeaderList;
		protected string _ViewName;
		protected Type _DescriptionField;
		protected Type _SubstituteKey;
		protected bool _DirtyRead;
		protected bool _Filterable;
		protected bool _CacheGlobal;
		protected bool _ViewCreated;
		protected bool _IsOwnView;
		protected UIFieldRef _UIFieldRef;

		private Type _originalBqlTable;
		[InjectDependencyOnTypeLevel]
		internal ITableReferenceInspector TableReferenceInspector { get; set; }
		[InjectDependencyOnTypeLevel]
		internal SelectorToReferenceConverter ReferenceConverter { get; set; }

		public virtual string CustomMessageElementDoesntExist { get; set; }
		public virtual string CustomMessageValueDoesntExist { get; set; }
		public virtual string CustomMessageElementDoesntExistOrNoRights { get; set; }
		public virtual string CustomMessageValueDoesntExistOrNoRights { get; set; }

		protected Delegate _ViewHandler;

        /// <summary>Gets or sets the value that indicates whether the attribute
        /// should cache the data records retrieved from the database to show in
        /// the lookup control. By default, the attribute does not cache the data
        /// records.</summary>
		public virtual bool CacheGlobal
		{
	        get { return _CacheGlobal; }
	        set
			{
                if (_NaturalSelect != null && _CacheGlobal != value)
                {
					Type substituteKey = _NaturalSelect.GetParameters().Last().GetReferencedType();
					_NaturalSelect = BuildNaturalSelect(value, substituteKey);
				}
				_CacheGlobal = value;
			}
		}

		/// <summary>Gets or sets the field from the referenced table that
        /// contains the description.</summary>
        /// <example>
        /// In the code below, the <apiname>PXSelector</apiname> attribute configures
        /// a lookup field that will let a user select from the data set defined
        /// by the <tt>Search</tt> query. The lookup control will display descriptions
        /// of the data records taken from <tt>CRLeadClass.description</tt> field.
        /// <code>
        /// [PXDBString(10, IsUnicode = true, InputMask = "&gt;aaaaaaaaaa")]
        /// [PXUIField(DisplayName = "Class ID")]
        /// [PXSelector(
        ///     typeof(Search&lt;CRLeadClass.cRLeadClassID,
        ///                Where&lt;CRLeadClass.isActive, Equal&lt;True&gt;&gt;&gt;),
        ///     DescriptionField = typeof(CRLeadClass.description))]
        /// public virtual string ClassID { get; set; }
        /// </code>
        /// </example>
		public virtual Type DescriptionField
		{
	        get { return _DescriptionField; }
	        set
			{
                if (value == null || typeof(IBqlField).IsAssignableFrom(value) && value.IsNested)
                {
					_DescriptionField = value;
				}
                else
                {
					throw new PXException(ErrorMessages.CantSetDescriptionField, value);
				}
			}
		}

        public virtual bool ShowPopupWarning { get; set; }
        public virtual bool ShowPopupMessage { get; set; }

        /// <summary>
        /// Gets or sets the type that is used as a key for saved filters.
        /// </summary>
        public virtual Type FilterEntity
	    {
			get { return _FilterEntity; }
		    set
		    {
                if (value == null || typeof(IBqlTable).IsAssignableFrom(value))
				    _FilterEntity = value;
			    else
				    throw new PXException(ErrorMessages.CantSetFilterEntity, value);
		    }
	    }

        /// <summary>Gets or sets the field from the referenced table that
        /// substitutes the key field used as internal value and is displayed as a
        /// value in the user interface (natural key).</summary>
        /// <example>
        /// The attribute below let the user select from the <tt>Branch</tt> data records.
        /// The attribute displays the <tt>Branch.BranchCD</tt> field value in the user
        /// interface, but actually assigns the <tt>Branch.BranchID</tt> field value to the
        /// field.
        /// <code>
        /// [PXSelector(typeof(Branch.branchID),
        ///             SubstituteKey = typeof(Branch.branchCD))]
        /// public virtual int? BranchID { get; set; }
        /// </code>
        /// </example>
		public virtual Type SubstituteKey
		{
	        get { return _SubstituteKey; }
	        set
			{
                if (value != null && typeof(IBqlField).IsAssignableFrom(value) && value.IsNested)
                {
					_SubstituteKey = value;
					_NaturalSelect = BuildNaturalSelect(_CacheGlobal, value);
				}
                else
                {
					throw new PXException(ErrorMessages.CantSubstituteKey, value);
				}
			}
		}
        /// <summary>Gets the field that identifies a referenced data record
        /// (surrogate key) and is assigned to the field annotated with the
        /// <tt>PXSelector</tt> attribute. Typically, it is the first parameter of
        /// the BQL query passed to the attribute constructor.</summary>
		public virtual Type Field => ForeignField;
        protected Type ForeignField => ((IBqlSearch)_Select).GetField();

		/// <summary>Gets or sets a value that indicates whether the attribute
        /// should take into account the unsaved modifications when displaying
        /// data records in control. If <tt>false</tt>, the data records are taken
        /// from the database and not merged with the cache object. If
        /// <tt>true</tt>, the data records are merged with the modification
        /// stored in the cache object.</summary>
		public virtual bool DirtyRead
		{
	        get { return _DirtyRead; }
	        set { _DirtyRead = value; }
        }

		/// <summary>
		/// Allows to control validation process.
		/// </summary>
		public bool ValidateValue = true;

        /// <summary>Gets or sets the value that indicates whether the filters
        /// defined by the user should be stored in the database.</summary>
		public virtual bool Filterable
		{
	        get { return _Filterable; }
	        set { _Filterable = value; }
        }
        /// <summary>Gets or sets the list of labels for column headers that are
        /// displayed in the lookup control. By default, the attribute uses
        /// display names of the fields.</summary>
		public virtual string[] Headers
		{
	        get { return _HeaderList; }
	        set
			{
                if (_FieldList == null || value != null && value.Length != _FieldList.Length)
                {
					throw new PXArgumentException("Headers", ErrorMessages.HeadersNotMeetColList);
				}
				_HeaderList = value;
			}
		}

		protected Type _ValueField;
        /// <summary>
        ///  Gets the referenced data record field whose value is
        ///  assigned to the current field (marked with the <tt>PXSelector</tt>
        ///  attribute).
        /// </summary>
		public Type ValueField => _ValueField;

		protected PXSelectorMode _SelectorMode;
        /// <summary>
        /// Gets or sets the value that determines the value displayed by
        /// the selector control in the UI and some aspects of
        /// attribute's behavior. You can assign a combination of
        /// <see cref="PXSelectorMode">PXSelectorMode</see> values joined
        /// by bitwise or ("|").
        /// </summary>
        /// <example>
        /// In the following example, the <tt>SelectorMode</tt> property
        /// is used to disable autocompletion in the selector control.
        /// <code>
        /// ...
        /// [PXSelector(
        ///     typeof(FinPeriod.finPeriodID), 
        ///     DescriptionField = typeof(FinPeriod.descr),
        ///     SelectorMode = PXSelectorMode.NoAutocomplete)]
        /// public virtual String FinPeriodID { get; set; }
        /// </code>
        /// </example>
		public virtual PXSelectorMode SelectorMode
		{
			get
			{
				return _SelectorMode & ~PXSelectorMode.NoAutocomplete;
			}
	        set { this._SelectorMode = value; }
        }

		/// <summary>
		/// Exclude <see cref="PXSelectorAttribute"/> from using it in <see cref="Reference"/> generating process.
		/// </summary>
		public virtual bool ExcludeFromReferenceGeneratingProcess { get; set; }

        /// <summary>Gets the BQL query that is used to retrieve data records to
        /// show to the user.</summary>
        /// <remarks>This select contains condition by ID to retrieve a specific record by key.</remarks>
		public BqlCommand PrimarySelect => _PrimarySelect;

		/// <summary>Gets the BQL query that was passed to the attribute on it's creation.</summary>
		public BqlCommand OriginalSelect
	    {
		    get { return _OriginalSelect; }
	    }

		/// <exclude/>
		public int ParsCount => _ParsCount;

		/// <exclude/>
		internal Type Type => _Type;

		protected Boolean IsSelfReferencing => _CacheType != null && _Type != null && (_CacheType == _Type || _CacheType.IsSubclassOf(_Type));

		/// <exclude />
		public virtual bool SuppressUnconditionalSelect { get; set; }
		#endregion

		#region Ctor
		static PXSelectorAttribute()
		{
			Type t = System.Web.Compilation.PXBuildManager.GetType("PX.Objects.CS.FeaturesSet", false);
			if (t != null)
			{
				PXDatabase.Subscribe(t, () =>
					{
						FieldHeaderDictionaryIndependant fieldsheaders = PXDatabase.GetSlot<FieldHeaderDictionaryIndependant>(nameof(FieldHeaderDictionaryIndependant));
						fieldsheaders._fields.Clear();
						fieldsheaders._headers.Clear();
					});
			}
		}
        /// <summary>Initializes a new instance that will use the specified BQL
        /// query to retrieve the data records to select from. The list of
        /// displayed columns is created automatically and consists of all columns
        /// from the referenced table with the <tt>Visibility</tt> property of the
        /// <see cref="PXUIFieldAttribute">PXUIField</see> attribute set to
        /// <tt>PXUIVisibility.SelectorVisible</tt>.</summary>
        /// <param name="type">A BQL query that defines the data set that is shown
        /// to the user along with the key field that is used as a value. Set to a
        /// field (type part of a DAC field) to select all data records from the
        /// referenced table. Set to a BQL command of <tt>Search</tt> type to
        /// specify a complex select statement.</param>
		public PXSelectorAttribute(Type type)
		{
            if (type == null)
            {
				throw new PXArgumentException(nameof(type), ErrorMessages.ArgumentNullException);
			}
            if (typeof(IBqlSearch).IsAssignableFrom(type))
            {
				_Select = BqlCommand.CreateInstance(type);
				_Type = BqlCommand.GetItemType(ForeignField);
			}
            else if (type.IsNested && typeof(IBqlField).IsAssignableFrom(type))
            {
				_Select = BqlCommand.CreateInstance(typeof(Search<>), type);
				_Type = BqlCommand.GetItemType(type);
			}
            else
            {
				throw new PXArgumentException(nameof(type), ErrorMessages.CantCreateForeignKeyReference, type);
			}

			_BqlType = GetDBTableType(_Type);
			
			_ValueField = ForeignField;
			_LookupSelect = BqlCommand.CreateInstance(_Select.GetType());
			_PrimarySelect = _Select.WhereAnd(BqlCommand.Compose(typeof(Where<,>), _ValueField, typeof(Equal<>), typeof(Required<>), _ValueField));
            _PrimarySimpleSelect = _Select.WhereAnd(BqlCommand.Compose(typeof(Where<,>), _ValueField, typeof(Equal<>), typeof(Required<>), _ValueField)); ;
			_OriginalSelect = BqlCommand.CreateInstance(_Select.GetSelectType());
			_UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<,>), _ValueField, typeof(Where<,>), _ValueField, typeof(Equal<>), typeof(Required<>), _ValueField);
			_ViewName = GenerateViewName();
		}

		/// <summary>Initializes a new instance that will use the specified BQL
        /// query to retrieve the data records to select from, and display the
        /// provided set of columns.</summary>
        /// <param name="type">A BQL query that defines the data set that is shown
        /// to the user along with the key field that is used as a value. Set to a
        /// field (type part of a DAC field) to select all data records from the
        /// referenced table. Set to a BQL command of <tt>Search</tt> type to
        /// specify a complex select statement.</param>
        /// <param name="fieldList">Fields to display in the control.</param>
        /// <example>
        /// The attribute below configures the lookup control to let the user select from the
        /// <tt>Customer</tt> data records retrieved by the <tt>Search</tt> BQL
        /// query. The displayed columns are specified explicitly: <tt>AccountCD</tt> and
        /// <tt>CompanyName</tt>. The <tt>Customer.accountCD</tt> field data will be
        /// inserted as a value without conversion.
        /// <code>
        /// [PXSelector(
        ///     typeof(Search&lt;Customer.accountCD, 
        ///                Where&lt;Customer.companyType, Equal&lt;CompanyType.customer&gt;&gt;&gt;),
        ///     new Type[] 
        ///     {
        ///         typeof(Customer.accountCD),
        ///         typeof(Customer.companyName)
        ///     })]
        /// public virtual string AccountCD { get; set; }
        /// </code>
        /// </example>
		public PXSelectorAttribute(Type type, params Type[] fieldList)
			: this(type)
        {
	        SetFieldList(fieldList);
        }

		public PXSelectorAttribute(Type type, Type lookupJoin, bool cacheGlobal, Type[] fieldList)
			: this(type)
		{
			if (lookupJoin == null || !typeof(IBqlJoin).IsAssignableFrom(lookupJoin))
				throw new PXArgumentException(nameof(lookupJoin), "Unsupported value {0}", lookupJoin);

			CacheGlobal = cacheGlobal;
			_LookupSelect = BqlCommand.CreateInstance(BqlCommand.AppendJoin(_LookupSelect.GetType(), lookupJoin));
			_PrimarySelect = BqlCommand.CreateInstance(BqlCommand.AppendJoin(_PrimarySelect.GetType(), lookupJoin));
			_ViewName = GenerateViewName();
			SetFieldList(fieldList);
		}

		internal void SetFieldList(Type[] fieldList)
		{
			fieldList = ExpandFieldList(fieldList);
			_FieldList = new string[fieldList.Length];
			Type[] tables = _LookupSelect.GetTables();
			for (int i = 0; i < fieldList.Length; i++)
			{
				if (!fieldList[i].IsNested || !typeof(IBqlField).IsAssignableFrom(fieldList[i]))
					throw new PXArgumentException(nameof(fieldList), ErrorMessages.InvalidSelectorColumn);

				_FieldList[i] = tables.Length <= 1 || BqlCommand.GetItemType(fieldList[i]).IsAssignableFrom(tables[0])
					? fieldList[i].Name
					: BqlCommand.GetItemType(fieldList[i]).Name + "__" + fieldList[i].Name;
			}
		}

		private Type[] ExpandFieldList(Type[] fieldList)
		{
			var fields = new List<Type>();
			foreach (Type field in fieldList)
			{
				if (BqlFields.IsTypeArray(field))
					fields.AddRange(BqlFields.CheckAndExtract(field));
				else
					fields.Add(field);
			}
			return fields.ToArray();
		}

		private static Type GetDBTableType(Type tableType)
		{
			Type dbTableType = tableType;
			while (typeof(IBqlTable).IsAssignableFrom(dbTableType.BaseType)
					&& !dbTableType.IsDefined(typeof(PXTableAttribute), false)
					&& (!dbTableType.IsDefined(typeof(PXTableNameAttribute), false) || !((PXTableNameAttribute)dbTableType.GetCustomAttributes(typeof(PXTableNameAttribute), false)[0]).IsActive))
			{
				dbTableType = dbTableType.BaseType;
			}
			return dbTableType;
		}

        /// <exclude/>
		public interface IPXAdjustableView { }

        /// <exclude/>
		public class PXAdjustableView : PXView, IPXAdjustableView
		{
			public PXAdjustableView(PXGraph graph, bool isReadOnly, BqlCommand @select, Delegate handler)
				: base(graph, isReadOnly, @select, handler)
			{
			}
		}

		private static readonly ConcurrentDictionary<Tuple<Type, Type>, Func<BqlCommand>> WhereAndFactories = new ConcurrentDictionary<Tuple<Type, Type>, Func<BqlCommand>>();
		private BqlCommand WhereAnd(BqlCommand select, Type Where)
		{
			if (WebConfig.EnablePageOpenOptimizations)
			{
				Func<BqlCommand> factory;
				var types = Tuple.Create(select.GetType(), Where);
				if (!WhereAndFactories.TryGetValue(types, out factory))
				{
					var result = select.WhereAnd(Where);
					var type = result.GetType();
					factory = Expression.Lambda<Func<BqlCommand>>(Expression.New(type)).Compile();
					WhereAndFactories.TryAdd(types, factory);
					return result;
				}
				return factory();
			}

			return select.WhereAnd(Where);
		}
		/// <exclude/>
		public BqlCommand WhereAnd(PXCache sender, Type whr)
		{
			if (!typeof(IBqlWhere).IsAssignableFrom(whr)) return _PrimarySelect;

			_Select = WhereAnd(_Select, whr);
			_LookupSelect = WhereAnd(_LookupSelect, whr);

            if (_ViewHandler == null)
            {
				_ViewHandler = new PXSelectDelegate(
                    delegate
                    {
						int startRow = PXView.StartRow;
						int totalRows = 0;

                        if (PXView.MaximumRows == 1)
                        {
							IBqlParameter[] selpars = _Select.GetParameters();
							object[] parameters = PXView.Parameters;
							List<object> pars = new List<object>();

                            for (int i = 0; i < selpars.Length && i < parameters.Length; i++)
                            {
                                if (selpars[i].MaskedType != null)
                                {
									break;
								}
                                if (selpars[i].IsVisible)
                                {
									pars.Add(parameters[i]);
								}
							}

							return new PXView(sender.Graph, !_DirtyRead, _OriginalSelect).Select(PXView.Currents, pars.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
						}

						return null;
					});
			}

            if (_ViewCreated)
            {
				// recreate selector view
				CreateView(sender);
			}
			
			return WhereAnd(_PrimarySelect, whr);
		}

		/// <summary>
		/// Generates default view name. View name is used by UI controls when selecting list of records available for selection.
		/// </summary>
		/// <returns>A string that references a PXView instance which will be used to retrive a list of records.</returns>
		protected virtual string GenerateViewName()
		{
			if (!(_Select is IBqlSearch)) return null;

			var simpleParameters = _Select.GetParameters();
			if (simpleParameters != null) _ParsSimpleCount = simpleParameters.Count(p => p.IsVisible);

			var parameters = _LookupSelect.GetParameters();
			if (parameters != null) _ParsCount = parameters.Count(p => p.IsVisible);

			//			var parameters = _Select.GetParameters();
			//			var bld = new StringBuilder("_");
			//			bld.Append(_Type.Name);
			//			if (parameters != null)
			//				foreach (var par in parameters)
			//				{
			//					if (!par.HasDefault) throw new PXArgumentException("sourceType", ErrorMessages.NotCurrentOrOptionalParameter);
			//					if (par.IsVisible) _ParsCount++;
			//					var t = par.GetReferencedType();
			//					bld.Append('_');
			//					bld.Append(BqlCommand.GetItemType(t).Name);
			//					bld.Append('.');
			//					bld.Append(t.Name);
			//				}
			//			bld.Append('_');
			//			return bld.ToString();
			return string.Format("_{0}_", ForeignField.FullName);
		}

		protected virtual BqlCommand BuildNaturalSelect(Boolean cacheGlobal, Type substituteKey)
		{
			Type surrogateCondition = BqlCommand.Compose(typeof(Where<,>), substituteKey, typeof(Equal<>), typeof(Required<>), substituteKey);
			return cacheGlobal
				? BqlCommand.CreateInstance(typeof(Search<,>), ForeignField, surrogateCondition)
				: _Select.WhereAnd(surrogateCondition);
		}

		#endregion

		#region Runtime
		/// <summary>
		/// A wrapper to PXView.SelectMultiBound() method, extracts the first table in a row if a result of a join is returned.<br/>
		/// While we are looking for a single record here, we still call SelectMulti() for performance reason, to hit cache and get the result of previously executed queries if any.<br/>
		/// 'Bound' means we will take omitted parameters from explicitly defined array of rows, not from current records set in the graph.
		/// </summary>
		/// <param name="view">PXView instance to be called for a selection result</param>
		/// <param name="currents">List of rows used as a source for omitted parameter values</param>
		/// <param name="pars">List of parameters to be passed to the query</param>
		/// <returns>Foreign record retrieved</returns>
		internal static object SelectSingleBound(PXView view, object[] currents, params object[] pars)
		{
			List<object> ret = view.SelectMultiBound(currents, pars);
			return ret.Count > 0 ? PXResult.UnwrapMain(ret[0]) : null;
		}
		/// <summary>
		/// A wrapper to PXView.SelectSingleBound() method, extracts the first table in a row if a result of a join is returned.<br/>
		/// </summary>
		/// <param name="view">PXView instance to be called for a selection result</param>
		/// <param name="pars">List of parameters to be passed to the query</param>
		/// <returns>Foreign record retrieved</returns>
		internal static object SelectSingle(PXView view, params object[] pars)
		{
			List<object> ret = view.SelectMulti(pars);
			return ret.Count > 0 ? PXResult.UnwrapMain(ret[0]) : null;
		}

        internal static object SelectSingle(PXCache cache, object data, string field, object value)
        {
            foreach (PXSelectorAttribute attr in cache.GetAttributesReadonly(field).OfType<PXSelectorAttribute>())
            {
	            var view = attr.GetViewWithParameters(cache, value, includeLookupJoins: true);
                List<object> ret = view.SelectMultiBound(data);
	            return ret.FirstOrDefault();
            }
            return null;
        }

		internal static object SelectSingle(PXCache cache, object data, string field)
		{
            object value = cache.GetValue(data, field);
            return SelectSingle(cache, data, field, value);
		}
		/// <summary>
		/// Returns cached typed view, can be ovirriden to substitute a view with a delegate instead.
		/// </summary>
		/// <param name="cache">PXCache instance, used to retrive a graph object</param>
		/// <param name="select">Bql command to be searched</param>
		/// <param name="dirtyRead">Flag to separate result sets either merged with not saved changes or not</param>
		/// <returns></returns>
		protected virtual PXView GetView(PXCache cache, BqlCommand select, bool isReadOnly) => cache.Graph.TypedViews.GetView(@select, isReadOnly);
		protected virtual PXView GetUnconditionalView(PXCache cache) => cache.Graph.TypedViews.GetView(_UnconditionalSelect, !DirtyRead);

		protected object[] MakeParameters(object lastParameter, bool includeLookupJoins = false)
		{
			int parsCount = includeLookupJoins ? _ParsCount : _ParsSimpleCount;
			var pars = new object[parsCount + 1];
			pars[pars.Length - 1] = lastParameter;
			return pars;
		}

		protected struct ViewWithParameters
		{
			public ViewWithParameters(PXView view, Object[] parameters)
			{
				View = view;
				Parameters = parameters;
			}

			public PXView View { get; }
			public object[] Parameters { get; }
			public PXCache Cache => View.Cache;
            public object SelectSingleBound(object current) => PXSelectorAttribute.SelectSingleBound(View, new[] { current }, Parameters);
            public List<object> SelectMultiBound(object current) => View.SelectMultiBound(new[] { current }, Parameters);
            public object[] PrepareParameters(object current) => View.PrepareParameters(new[] { current }, Parameters);
		}

		protected ViewWithParameters GetViewWithParameters(PXCache cache, object lastParameter, bool includeLookupJoins = false)
		{
			return new ViewWithParameters(
				GetView(cache, includeLookupJoins ? _PrimarySelect : _PrimarySimpleSelect, !DirtyRead),
				MakeParameters(lastParameter, includeLookupJoins));
		}

		/// <summary>Returns the data record referenced by the attribute instance
		/// that marks the field with the specified name in a particular data
		/// record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXSelector</tt> type.</param>
		/// <param name="data">The data record the method is applied to.</param>
		/// <param name="field">The name of the field that is be marked with the
		/// attribute.</param>
		public static object Select(PXCache cache, object data, string field)
		{
			object value = cache.GetValue(data, field);
			return Select(cache, data, field, value);
		}

        /// <summary>Returns the first data record retrieved by the attribute
        /// instance that marks the specified field in a particular data
        /// record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static object SelectFirst<Field>(PXCache cache, object data)
			where Field : IBqlField => SelectFirst(cache, data, typeof(Field).Name);

		/// <summary>Returns the first data record retrieved by the attribute
        /// instance that marks the field with the specified name in a particular
        /// data record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="field">The name of the field that is be marked with the
        /// attribute.</param>
		public static object SelectFirst(PXCache cache, object data, string field) => SelectSpecific(cache, data, field, 0);

		/// <summary>Returns the last data record retrieved by the attribute
        /// instance that marks the specified field in a particular data
        /// record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static object SelectLast<Field>(PXCache cache, object data)
			where Field : IBqlField => SelectLast(cache, data, typeof(Field).Name);

		/// <summary>Returns the last data record retrieved by the attribute
        /// instance that marks the field with the specified name in a particular
        /// data record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="field">The name of the field that is be marked with the
        /// attribute.</param>
		public static object SelectLast(PXCache cache, object data, string field) => SelectSpecific(cache, data, field, -1);

		private static Object SelectSpecific(PXCache cache, Object data, String field, Int32 rowPosition)
		{
			foreach (PXSelectorAttribute attr in cache.GetAttributesReadonly(field).OfType<PXSelectorAttribute>())
			{
				PXView view = cache.Graph.TypedViews.GetView(attr._Select, !attr._DirtyRead);
				int startRow = rowPosition;
				int totalRows = 0;
                List<object> source = view.Select(new object[] { data }, null, null, null, null, null, ref startRow, 1, ref totalRows);
				if (source != null && source.Count > 0)
				{
					object item = PXResult.UnwrapMain(source[source.Count - 1]);
					return item;
				}
				return null;
			}
			return null;
		}

		/// <summary>Returns the referenced data record that holds the specified
        /// value. The data record should be referenced by the attribute instance
        /// that marks the field with the specified in a particular data
        /// record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="field">The name of the field that is be marked with the
        /// attribute.</param>
        /// <param name="value">The value to search the referenced table
        /// for.</param>
        /// <returns>Foreign record.</returns>
		public static object Select(PXCache cache, object data, string field, object value)
		{
			return cache.GetAttributesReadonly(field)
						.OfType<PXSelectorAttribute>()
						.Select(attr => GetItem(cache, attr, data, value))
						.FirstOrDefault();
		}

		private sealed class CSAttributeGroup : IBqlTable { }
		private sealed class FeaturesSet : IBqlTable { }
		protected class FieldHeaderDictionaryIndependant
		{
			public ConcurrentDictionary<string, string[]> _fields = new ConcurrentDictionary<string, string[]>();
			public ConcurrentDictionary<string, string[]> _headers = new ConcurrentDictionary<string, string[]>();
		}
		protected sealed class FieldHeaderDictionaryDependant : FieldHeaderDictionaryIndependant, IPXCompanyDependent { }
		
		internal GlobalDictionary GetGlobalCache() => GlobalDictionary.GetOrCreate(_Type, _BqlType, KnownForeignKeysCount);
		/// <exclude/>
		internal sealed class GlobalDictionary : IPXCompanyDependent
        {
            internal const string SlotPrefix = "GlobalDictionary$";
			public static GlobalDictionary GetOrCreate(Type foreignTable, Type watchedTable) => GetOrCreate(foreignTable, watchedTable, 1);

			public static GlobalDictionary GetOrCreate(Type foreignTable, Type watchedTable, byte keysCount)
			{
				String slotName = _GetSlotName(foreignTable, keysCount);
				var dict = PXContext.GetSlot<GlobalDictionary>(slotName);
				if (dict == null)
                    PXContext.SetSlot(slotName, dict = PXDatabase.GetLocalizableSlot<GlobalDictionary>(slotName, watchedTable));
				return dict;
			}

			public static void ClearFor(Type table) => ClearFor(table, 1);
			public static void ClearFor(Type table, byte keysCount)
			{
				var slotName = _GetSlotName(table, keysCount);
				PXDatabase.ResetLocalizableSlot<GlobalDictionary>(slotName, table);
				PXContext.SetSlot(slotName, null);
			}

            /// <exclude/>
			internal struct CacheValue
			{
				public object Item;
				public bool IsDeleted;
				public PXCacheExtension[] Extensions;
			}

			readonly Dictionary<object, CacheValue> Items = new Dictionary<object, CacheValue>();

            public object SyncRoot => ((ICollection)Items).SyncRoot;

			public bool TryGetValue(object key, out CacheValue cacheValue)
			{
				if (!Items.TryGetValue(key, out cacheValue))
				{
					cacheValue = default(CacheValue);
					return false;
				}
				
				if (cacheValue.Extensions != null)
				{
					var r = cacheValue.Item as IBqlTable;
					
					var ext = PXCacheExtensionCollection.GetSlot(true);
					lock (ext.SyncRoot)
					{
						ext[r] = cacheValue.Extensions;
					}
				}

				return true;
			}

			public void Set(object key, object row, bool deleted)
			{
                var store = new CacheValue { Item = row, IsDeleted = deleted };
				var r = row as IBqlTable;
				if (r != null)
				{
					store.Extensions = r.GetExtensions();
				}

				Items[key] = store;
			}

		}

		protected virtual void AppendOtherValues(Dictionary<string, object> values, PXCache cache, object row) { }
		protected virtual object CreateGlobalCacheKey(PXCache cache, object row, object keyValue) => keyValue;
		protected virtual byte KnownForeignKeysCount => 1;
		protected bool CanCacheGlobal(PXCache foreignCache) => foreignCache.Keys.Count <= KnownForeignKeysCount;

		/// <summary>Returns the foreign data record by the specified
		/// key.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXSelector</tt> type.</param>
		/// <param name="attr">The instance of the <tt>PXSelector</tt> attribute
		/// to query for a data record.</param>
		/// <param name="data">The data record that contains a reference to the
		/// foreign data record.</param>
		/// <param name="key">The key value of the referenced data record.</param>
		public static object GetItem(PXCache cache, PXSelectorAttribute attr, object data, object key)
		{
			return GetItem(cache, attr, data, key, false);
		}
		internal static object GetItem(PXCache cache, PXSelectorAttribute attr, object data, object key, bool unconditionally)
		{
		    GlobalDictionary globalDictionary;
		    var row = LookupGlobalDictionary(cache, attr, data, key, out globalDictionary);
		    if (row == null && (key == null || key.GetType() == cache.GetFieldType(attr._FieldName)))
			{
				var view = attr.GetViewWithParameters(cache, key);
				row = view.SelectSingleBound(data);
				if (row == null)
				{
					if (!unconditionally || attr.SuppressUnconditionalSelect)
						return null;

					var select = attr.GetUnconditionalView(cache);
					row = cache._InvokeSelectorGetter(data, attr.FieldName, select, new object[] { key }, true) ?? SelectSingleBound(select, new object[] { data }, key);
					return row;
				}
				attr.cacheOnReadItem(globalDictionary, view.Cache, attr.ForeignField.Name, row, key, false);
			}
			return row;
		}

	    internal static object GetItemUnconditionally(PXCache cache, PXSelectorAttribute attr, object key)
	    {
            if (key == null)
                return null;
            if (attr.SuppressUnconditionalSelect)
                return null;
            var row = LookupGlobalDictionary(cache, attr, null, key, out _);
	        var fieldType = cache.GetFieldType(attr._FieldName);
            if (row == null && fieldType == null || key.GetType() == fieldType)
	        {
	            PXView select = attr.GetView(cache, attr._UnconditionalSelect, !attr._DirtyRead);
	            row = cache._InvokeSelectorGetter(null, attr.FieldName, select, new object[] { key }, true) ?? SelectSingleBound(select, new object[0], key);
	            return row;
            }
	        return row;
	    }

	    private static object LookupGlobalDictionary(PXCache cache, PXSelectorAttribute attr, object data, object key, out GlobalDictionary globalDictionary)
	    {
	        object row = null;
	        globalDictionary = null;
	        if (attr._CacheGlobal && key != null)
	        {
	            globalDictionary = attr.GetGlobalCache();
	            lock (globalDictionary.SyncRoot)
	            {
	                GlobalDictionary.CacheValue cacheValue;
	                if (globalDictionary.TryGetValue(attr.CreateGlobalCacheKey(cache, data, key), out cacheValue) && !cacheValue.IsDeleted && !(cacheValue.Item is IDictionary))
	                {
	                    row = cacheValue.Item;
	                }
	            }
	        }
	        return row;
	    }

        /// <summary>Clears the internal cache of the <tt>PXSelector</tt>
		/// attribute, removing the data records retrieved from the specified
		/// table. Typically, you don't need to call this method, because the
		/// attribute subscribes on the change notifications related to the table
		/// and drops the cache automatically.</summary>
		public static void ClearGlobalCache<Table>() where Table : IBqlTable => ClearGlobalCache<Table>(1);
		public static void ClearGlobalCache<Table>(byte keysCount)
			where Table : IBqlTable => GlobalDictionary.ClearFor(typeof(Table), keysCount);

		/// <summary>Clears the internal cache of the <tt>PXSelector</tt>
        /// attribute, removing the data records retrieved from the specified
        /// table. Typically, you don't need to call this method, because the
        /// attribute subscribes on the change notifications related to the table
        /// and drops the cache automatically.</summary>
        /// <param name="table">The DAC to drop from the attribute's
        /// cache.</param>
		public static void ClearGlobalCache(Type table) => ClearGlobalCache(table, 1);
		public static void ClearGlobalCache(Type table, byte keysCount)
		{
            if (table == null)
            {
				throw new PXArgumentException(nameof(table), ErrorMessages.ArgumentNullException);
			}
			GlobalDictionary.ClearFor(table, keysCount);
		}
        /// <summary>Returns a value of the field from a foreign data
        /// record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record that contains a reference to the
        /// foreign data record.</param>
        /// <param name="field">The name of the field holding the referenced data
        /// record key value.</param>
        /// <param name="value">The key value of the referenced data
        /// record.</param>
        /// <param name="foreignField">The name of the referenced data record
        /// field whose value is returned by the method.</param>
		public static object GetField(PXCache cache, object data, string field, object value, string foreignField)
		{
            foreach (PXSelectorAttribute attr in cache.GetAttributesReadonly(data, field).OfType<PXSelectorAttribute>())
            {
				object row = null;
				GlobalDictionary dict = null;
                if (attr._CacheGlobal && value != null)
                {
					dict = GlobalDictionary.GetOrCreate(attr._Type, cache.Graph.Caches[attr._Type].BqlTable, attr.KnownForeignKeysCount);
                    lock (dict.SyncRoot)
                    {
						GlobalDictionary.CacheValue cacheValue;
                        if (dict.TryGetValue(attr.CreateGlobalCacheKey(cache, data, value), out cacheValue) && !cacheValue.IsDeleted)
                        {
							row = cacheValue.Item;
						}
					}
				}
                if (row == null)
                {
					var view = attr.GetViewWithParameters(cache, value);
					row = view.SelectSingleBound(data);
                    if (row == null)
						return null;
					attr.cacheOnReadItem(dict, view.Cache, attr.ForeignField.Name, row, value, false);
				}
				return cache.Graph.Caches[attr._Type].GetValue(row, foreignField) ?? new byte[0];
			}
			return null;
		}

		internal static void CheckIntegrityAndPutGlobal(GlobalDictionary globalDictionary, PXCache foreignCache, String foreignField, Object foreignRow, Object ownKey, Boolean isRowDeleted, Boolean putSingle = false)
		{
			var compositeKey = ownKey as Composite;
			var otherKeys = Array<object>.Empty;
			if (compositeKey != null)
			{
				ownKey = compositeKey.Last();
				otherKeys = compositeKey.Take(compositeKey.ComponentsCount - 1).ToArray();
			}
			bool putBoth = false;
			object val = foreignCache.GetValue(foreignRow, foreignField);
			if ((object.Equals(ownKey, val) || (putBoth = !putSingle && ownKey is string && val is string && string.Equals(((string)ownKey).TrimEnd(), ((string)val).TrimEnd())))
				&& (foreignCache.Keys.Count == 0
				|| foreignCache.Keys.Contains(foreignField)
				|| foreignCache.Identity == null
				|| String.Equals(foreignCache.Identity, foreignField, StringComparison.OrdinalIgnoreCase)))
			{
				lock (globalDictionary.SyncRoot)
				{
					globalDictionary.Set(compositeKey ?? ownKey, foreignRow, isRowDeleted);
					if (putBoth)
					{
						Object valKey = compositeKey == null ? val : Composite.Create(otherKeys.Append(val));
						globalDictionary.Set(valKey, foreignRow, isRowDeleted);
					}
				}
			}
		}

		/// <summary>Returns the data access class referenced by the attribute
			/// instance that marks the field with specified name.</summary>
			/// <param name="cache">The cache object to search for the attributes of
			/// <tt>PXSelector</tt> type.</param>
			/// <param name="field">The name of the field that marked with the
			/// attribute.</param>
			public static Type GetItemType(PXCache cache, string field)
        {
	        return cache.GetAttributesReadonly(field)
						.OfType<PXSelectorAttribute>()
						.Select(a => a._Type)
						.FirstOrDefault();
        }
        /// <summary>Returns all data records kept by the attribute instance the
        /// marks the specified field in a particular data record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static List<object> SelectAll<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return SelectAll(cache, typeof(Field).Name, data);
		}
        /// <summary>Returns all data records kept by the attribute instance the
        /// marks the field with the specified name in a particular data
        /// record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="fieldname">The name of the field that should be marked
        /// with the attribute.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static List<object> SelectAll(PXCache cache, string fieldname, object data)
        {
	        return cache
		        .GetAttributesReadonly(fieldname)
		        .OfType<PXSelectorAttribute>()
		        .Select(attr => attr.GetView(cache, attr._LookupSelect, !attr._DirtyRead))
                .Select(select => select.SelectMultiBound(new object[] { data }))
		        .FirstOrDefault();
        }
        /// <summary>Returns the data record referenced by the attribute instance
        /// that marks the specified field in a particular data record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
		public static object Select<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return Select(cache, data, typeof(Field).Name);
		}
        /// <summary>Returns the referenced data record that holds the specified
        /// value. The data record is searched among the ones referenced by the
        /// attribute instance that marks the specified field in a particular data
        /// record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="value">The value to search the referenced table
        /// for.</param>
		public static object Select<Field>(PXCache cache, object data, object value)
			where Field : IBqlField
		{
			return Select(cache, data, typeof(Field).Name, value);
		}
        /// <summary>Sets the list of columns and column headers to display for
        /// the attribute instance that marks the field with the specified name in
        /// a particular data record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to. If
        /// <tt>null</tt>, the method is applied to all data records kept in the
        /// cache object.</param>
        /// <param name="field">The name of the field marked with the
        /// attribute.</param>
        /// <param name="fieldList">The new list of field names.</param>
        /// <param name="headerList">The new list of column headers.</param>
		public static void SetColumns(PXCache cache, object data, string field, string[] fieldList, string[] headerList)
		{
            if (data == null)
            {
				cache.SetAltered(field, true);
			}
			foreach (PXSelectorAttribute attr in cache.GetAttributes(data, field).OfType<PXSelectorAttribute>())
			{
				attr._FieldList = fieldList;
				attr._HeaderList = headerList;
			}
		}
        /// <summary>Sets the list of columns and column headers for all attribute
        /// instances that mark the field with the specified name in all data
        /// records in the cache object.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="field">The name of the field marked with the
        /// attribute.</param>
        /// <param name="fieldList">The new list of field names.</param>
        /// <param name="headerList">The new list of column headers.</param>
		public static void SetColumns(PXCache cache, string field, string[] fieldList, string[] headerList)
		{
			cache.SetAltered(field, true);
			foreach (PXSelectorAttribute attr in cache.GetAttributes(field).OfType<PXSelectorAttribute>())
			{
				attr._FieldList = fieldList;
				attr._HeaderList = headerList;
			}
		}
		/// <summary>Sets the list of columns and column headers for an attribute
		/// instance.</summary>
		/// <param name="fieldList">The new list of field names.</param>
		/// <param name="headerList">The new list of column headers.</param>
		public virtual void SetColumns(string[] fieldList, string[] headerList)
		{
			this._FieldList = fieldList;
			this._HeaderList = headerList;
		}
        /// <summary>Sets the list of columns and column headers to display for
        /// the attribute instance that marks the specified field in a particular
        /// data record.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="data">The data record the method is applied to.</param>
        /// <param name="fieldList">The new list of field names.</param>
        /// <param name="headerList">The new list of column headers.</param>
		public static void SetColumns<Field>(PXCache cache, object data, Type[] fieldList, string[] headerList)
			where Field : IBqlField
		{
			if (data == null)
				cache.SetAltered<Field>(true);

			foreach (PXSelectorAttribute attr in cache.GetAttributes<Field>(data).OfType<PXSelectorAttribute>())
				attr.SetColumns(fieldList, headerList);
		}
        /// <summary>Sets the list of columns and column headers for all attribute
        /// instances that mark the specified field in all data records in the
        /// cache object.</summary>
        /// <param name="cache">The cache object to search for the attributes of
        /// <tt>PXSelector</tt> type.</param>
        /// <param name="fieldList">The new list of field names.</param>
        /// <param name="headerList">The new list of column headers.</param>
		public static void SetColumns<Field>(PXCache cache, Type[] fieldList, string[] headerList)
			where Field : IBqlField
		{
			SetColumns<Field>(cache, null, fieldList, headerList);
		}

		private void SetColumns(Type[] fieldList, String[] headerList)
		{
			SetFieldList(fieldList);
			_HeaderList = headerList;
		}

		/// <exclude/>
		public static void StoreCached<Field>(PXCache cache, object data, object item)
			where Field : IBqlField
		{
			StoreCached<Field>(cache, data, item, false);
		}
		/// <exclude/>
		public static void StoreCached<Field>(PXCache cache, object data, object item, bool clearCache)
			where Field : IBqlField
		{
			foreach (PXSelectorAttribute attr in cache.GetAttributesReadonly<Field>().OfType<PXSelectorAttribute>())
			{
				var view = attr.GetViewWithParameters(cache, cache.GetValue(data, attr._FieldOrdinal), includeLookupJoins: true);
				object[] preparedPars = view.PrepareParameters(data);
				if (clearCache)
				{
					view.View.Clear();
				}
                view.View.StoreCached(new PXCommandKey(preparedPars), new List<object> { item });
				return;
			}
		}
		/// <summary>
		/// Checks foreign keys and raises exception on violation. Works only if foreing key feild has PXSelectorAttribute
		/// </summary>
		/// <param name="Row">Current record</param>
		/// <param name="fieldType">BQL type of foreing key</param>
		/// <param name="searchType">Optional additional BQL statement to be checked</param>
		/// <param name="customMessage">Optional custom message to be displayed to user. Must either have {0} placeholder for name of current table 
		/// and {1} placeholder for foreign key table name, or no format placeholders at all</param>
		public static void CheckAndRaiseForeignKeyException(PXCache sender, object Row, Type fieldType, Type searchType = null, string customMessage = null)
        {
			var checker = new ForeignKeyChecker(sender, Row, fieldType, searchType);
	        if (!string.IsNullOrEmpty(customMessage))
		        checker.CustomMessage = customMessage;
	        checker.DoCheck();
		}

        /// <exclude />
		public virtual ISet<Type> GetDependencies(PXCache sender)
		{
			var result = new HashSet<Type>();
			Type cacheType = sender.GetItemType();
			if ((cacheType == _Type || cacheType.IsSubclassOf(_Type)) && _DescriptionField != null)
			{
				result.Add(_DescriptionField);
			}

			return result;
		}

        #endregion

        #region Implementation
        /// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            if (e.NewValue == null || !ValidateValue || _BypassFieldVerifying.Value)
            {
				return;
			}

            if (sender.Keys.Count == 0 || _FieldName != sender.Keys[sender.Keys.Count - 1])
            {
				object item = null;
				Verify(sender, e, ref item);
			}
		}

		protected virtual void Verify(PXCache sender, PXFieldVerifyingEventArgs e, ref object item)
		{
			if (item == null)
			{
				var view = GetViewWithParameters(sender, e.NewValue);
				try
				{
					item = sender._InvokeSelectorGetter(e.Row, _FieldName, view.View, view.Parameters, true) ?? view.SelectSingleBound(e.Row);
				}
				catch (FormatException) { } // thrown by SqlServer
				catch (InvalidCastException) { } // thrown by MySql
			}
			if (item == null)
			{
				if (_SubstituteKey != null)
				{
					if (e.ExternalCall)
					{
						object incoming = sender.GetValuePending(e.Row, _FieldName);
						if (incoming != null)
						{
							e.NewValue = incoming;
						}
					}
					else if (object.Equals(e.NewValue, sender.GetValue(e.Row, _FieldOrdinal)))
					{
						try
						{
							object incoming = sender.GetValueExt(e.Row, _FieldName);
							if (incoming is PXFieldState)
							{
								e.NewValue = ((PXFieldState)incoming).Value;
							}
							else if (incoming != null)
							{
								e.NewValue = incoming;
							}
						}
						catch
						{
						}
					}
					else
					{
						try
						{
							object incoming = e.NewValue;
							sender.RaiseFieldSelecting(_FieldName, e.Row, ref incoming, false);
							if (incoming is PXFieldState)
							{
								e.NewValue = ((PXFieldState)incoming).Value;
							}
							else if (incoming != null)
							{
								e.NewValue = incoming;
							}
						}
						catch
						{
						}
				}
				}
				throwNoItem(hasRestrictedAccess(sender, _PrimarySimpleSelect, e.Row), e.ExternalCall, e.NewValue);
			}
			else if (ShowPopupMessage)
			{
				string popupText = PXNoteAttribute.GetPopupNote(sender.Graph.Caches[_Type], item);

				if (!string.IsNullOrEmpty(popupText))
				{
					PopupNoteManager.RegisterText(sender, e.Row, _FieldName, popupText);
				}
			}
		}

		protected internal static string[] hasRestrictedAccess(PXCache sender, BqlCommand command, object row)
		{
			List<string> descr = new List<string>();
			foreach (IBqlParameter par in command.GetParameters().Where(p => p.MaskedType != null))
			{
				Type ft = par.GetReferencedType();
				if (ft.IsNested)
				{
					Type ct = ft.DeclaringType;
					PXCache cache = sender.Graph.Caches[ct];
					object val = null;
					bool currfound = false;
					if (row != null && (row.GetType() == ct || row.GetType().IsSubclassOf(ct)))
					{
						val = cache.GetValue(row, ft.Name);
						currfound = true;
					}
					if (!currfound && val == null && cache.Current != null)
					{
						val = cache.GetValue(cache.Current, ft.Name);
					}
					if (val == null && par.TryDefault)
					{
						if (cache.RaiseFieldDefaulting(ft.Name, null, out val))
						{
							cache.RaiseFieldUpdating(ft.Name, null, ref val);
						}
					}

					if (val != null)
					{
						descr.Add($"{ft.Name.ToCapitalized()}={val}");
					}
				}
			}

			return descr.Count > 0 ? descr.ToArray() : null;
		}

		protected void throwNoItem(string[] restricted, bool external, object value)
		{
			PXTrace.WriteInformation("The item {0} is not found (restricted:{1},external:{2},value:{3})",
				this.FieldName, restricted != null ? string.Join(",", restricted) : false.ToString(), external, value);

            if (restricted == null)
            {
                if (external || value == null)
                {
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(CustomMessageElementDoesntExist ?? ErrorMessages.ElementDoesntExist, _FieldName));
				}
                else
                {
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(CustomMessageValueDoesntExist ?? ErrorMessages.ValueDoesntExist, _FieldName, value));
				}
			}
            else
            {
                if (external || value == null)
                {
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(CustomMessageElementDoesntExistOrNoRights ?? ErrorMessages.ElementDoesntExistOrNoRights, _FieldName));
				}
                else
                {
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(CustomMessageValueDoesntExistOrNoRights ?? ErrorMessages.ValueDoesntExistOrNoRights, _FieldName, value));
				}
			}
		}
        /// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			bool deleted = false;
			if (_SubstituteKey == null && e.ReturnValue != null && IsReadDeletedSupported && sender.Graph.GetType() != typeof(PXGraph) &&
                (!_BqlTable.IsAssignableFrom(_BqlType) || sender.Keys.Count == 0 || String.Compare(sender.Keys[sender.Keys.Count - 1], _FieldName, StringComparison.OrdinalIgnoreCase) != 0))
            {
				object key = e.ReturnValue;
				GlobalDictionary dict = null;
				object item = null;
                if (_CacheGlobal)
                {
					dict = GetGlobalCache();
                    lock (dict.SyncRoot)
                    {
						GlobalDictionary.CacheValue cacheValue;
                        if (dict.TryGetValue(CreateGlobalCacheKey(sender, e.Row, key), out cacheValue))
                        {
							item = cacheValue.Item;
							deleted = cacheValue.IsDeleted;
						}
					}
				}
                if (item == null)
				{
					var view = GetViewWithParameters(sender, key);
					item = sender._InvokeSelectorGetter(e.Row, _FieldName, view.View, view.Parameters, true) ?? view.SelectSingleBound(e.Row);
					if (item == null)
					{
                        using (new PXReadDeletedScope())
                        {
							item = view.SelectSingleBound(e.Row);
							deleted = item != null;
						}
					}
					cacheOnReadItem(dict, view.Cache, ForeignField.Name, item, key, deleted);
				}
            }
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                if (_HeaderList == null)
                {
					populateFields(sender, PXContext.GetSlot<bool>(selectorBypassInit));
				}

                PXErrorLevel errorLevel = deleted ? PXErrorLevel.Warning : PXErrorLevel.Undefined;
                string errorText = deleted ? ErrorMessages.ForeignRecordDeleted : null;

                if (ShowPopupWarning && e.Row != null)
                {
                    object val = sender.GetValue(e.Row, _FieldOrdinal);
                    ViewWithParameters view = GetViewWithParameters(sender, val);
                    object item = sender._InvokeSelectorGetter(e.Row, _FieldName, view.View, view.Parameters, true) ?? view.SelectSingleBound(e.Row);
                    string popupText = PXNoteAttribute.GetPopupNote(view.Cache, item);

                    if (!string.IsNullOrEmpty(popupText) && errorLevel < PXErrorLevel.Warning)
                    {
                        errorLevel = PXErrorLevel.Warning;
                        errorText = popupText;
                    }
                }

                e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, _FieldName, _DescriptionField != null ? _DescriptionField.Name : null, null, errorText, errorLevel, null, null, null, PXUIVisibility.Undefined, _ViewName, _FieldList, _HeaderList);
				((PXFieldState)e.ReturnState).ValueField = _SubstituteKey == null ? ValueField.Name : _SubstituteKey.Name;
				((PXFieldState)e.ReturnState).SelectorMode = sender.IsAutoNumber(_FieldName) ? SelectorMode : _SelectorMode;
            }
            else if (deleted)
            {
				e.ReturnState = sender.GetStateExt(e.Row, _FieldName);
			}
			//if (_HeaderList == null || _HeaderList.Length == 0 || _FieldList == null || _FieldList.Length == 0)
			//{
			//	PXFirstChanceExceptionLogger.LogMessage("Empty selector columns detected " +
			//		(_HeaderList == null ? "headers are null " : (_HeaderList.Length == 0 ? "headers are empty " : "")) +
			//		(_FieldList == null ? "fields are null " : (_FieldList.Length == 0 ? "fields are empty " : "")) +
			//		"for the field " + _FieldName +
			//		" flag state " + PXContext.GetSlot<bool>(selectorBypassInit).ToString());
			//}
		}
		

		private bool? _isReadDeletedSupported;
		protected virtual bool IsReadDeletedSupported
		{
			get
			{
				if (_isReadDeletedSupported == null)
					_isReadDeletedSupported = PXDatabase.IsReadDeletedSupported(_BqlType);
				return _isReadDeletedSupported.Value;
			}
		}

        /// <exclude/>
		public virtual void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
		{
			bool deleted = false;
            if (e.Row != null)
            {
				object key = sender.GetValue(e.Row, _FieldOrdinal);
                if (key != null)
                {
					object item = null;
					GlobalDictionary dict = null;
                    if (_CacheGlobal)
                    {
						dict = GetGlobalCache();
                        lock (dict.SyncRoot)
                        {
							GlobalDictionary.CacheValue cacheValue;
                            if (dict.TryGetValue(CreateGlobalCacheKey(sender, e.Row, key), out cacheValue))
                            {
								item = cacheValue.Item;
								deleted = cacheValue.IsDeleted;
							}
						}
					}
					if (item == null)
					{
						PXCache itemCache = sender;
						readItem(itemCache, e.Row, key, out itemCache, out item, ref deleted);
						cacheOnReadItem(dict, itemCache, ForeignField.Name, item, key, deleted);
					}
					if (item != null)
					{
						PXCache itemCache = sender.Graph.Caches[_Type];
						e.ReturnValue = itemCache.GetValue(item, _DescriptionField.Name);
						if (e.ReturnValue == null)
						{
							readItem(itemCache, e.Row, key, out itemCache, out item, ref deleted);
							if (item != null)
							{
								cacheOnReadItem(dict, itemCache, ForeignField.Name, item, key, deleted);
								e.ReturnValue = itemCache.GetValue(item, _DescriptionField.Name);
							}
						}
					}
				}
			}
            if (e.Row == null || e.IsAltered || deleted)
            {
				int? length;
				string displayname = getDescriptionName(sender, out length);
                if (_UIFieldRef != null && _UIFieldRef.UIFieldAttribute == null)
				{
					_UIFieldRef.UIFieldAttribute = sender.GetAttributes(FieldName)
												   .OfType<PXUIFieldAttribute>()
												   .FirstOrDefault();
				}
				bool isVisible = true;
				PXUIVisibility visibility = PXUIVisibility.Visible;
				if (_UIFieldRef?.UIFieldAttribute != null)
				{
					isVisible = _UIFieldRef.UIFieldAttribute.Visible;
					visibility = _UIFieldRef.UIFieldAttribute.Visibility;
					if (!_UIFieldRef.UIFieldAttribute.ViewRights)
					{
						visibility = PXUIVisibility.HiddenByAccessRights;
					}
					else if (((visibility & PXUIVisibility.SelectorVisible) == PXUIVisibility.SelectorVisible)
						&& (!sender.Keys.Contains(_FieldName) || !String.Equals(alias, _UIFieldRef.UIFieldAttribute.FieldName + "_description", StringComparison.OrdinalIgnoreCase)))
					{
						visibility = PXUIVisibility.Visible;
					}
				}
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, null, null, length, null,
					alias//_FieldName + "_" + _Type.Name + "_" + _DescriptionField.Name
					, null, displayname, deleted ? ErrorMessages.ForeignRecordDeleted : null, deleted ? PXErrorLevel.Warning : PXErrorLevel.Undefined, false, isVisible, null, visibility, null, null, null);
			}
		}

		protected void readItem(PXCache sender, object row, object key, out PXCache itemCache, out object item, ref bool deleted)
		{
			itemCache = sender;
			item = null;
			if (_UnconditionalSelect.GetTables()[0] == _PrimarySimpleSelect.GetTables()[0])
			{
				var view = GetViewWithParameters(sender, key);
				itemCache = view.Cache;
				using (new PXReadBranchRestrictedScope())
				{
					try
					{
						item = sender._InvokeSelectorGetter(row, _FieldName, view.View, view.Parameters, true) ?? view.SelectSingleBound(row);
						if (item == null && IsReadDeletedSupported)
						{
                            using (new PXReadDeletedScope())
                            {
								item = view.SelectSingleBound(row);
								deleted = item != null;
							}
						}
					}
					catch
					{
					}
				}
			}
			if (item == null && !SuppressUnconditionalSelect)
			{
				var view = GetUnconditionalView(sender);
				itemCache = view.Cache;
				using (new PXReadBranchRestrictedScope())
				{
					try
					{
						item = sender._InvokeSelectorGetter(row, _FieldName, view, new[] { key }, true) ?? SelectSingleBound(view, new object[] { row }, key);
						if (item == null && IsReadDeletedSupported)
						{
                            using (new PXReadDeletedScope())
                            {
								item = SelectSingleBound(view, new object[] { row }, key);
								deleted = item != null;
							}
						}
					}
					catch (FormatException) // thrown by MS SQL
					{
					}
					catch (InvalidCastException) // thrown by MySQL
					{
					}
				}
			}
		}

		private bool CanCacheItem(GlobalDictionary dict, PXCache foreignCache, object foreignItem)
			=> _CacheGlobal
				&& dict != null && foreignItem != null
				&& CanCacheGlobal(foreignCache)
				&& foreignCache.GetItemType().IsInstanceOfType(foreignItem)
				&& !PXDatabase.ReadDeleted
				&& foreignCache.GetStatus(foreignItem) == PXEntryStatus.Notchanged;

		internal void cacheOnReadItem(GlobalDictionary dict, PXCache foreignCache, object foreignItem, bool isItemDeleted = false)
		{
			if (!CanCacheItem(dict, foreignCache, foreignItem))
				return;
			var fieldValue = foreignCache.GetValue(foreignItem, ForeignField.Name);
			if (fieldValue == null)
				return;
			CheckIntegrityAndPutGlobal(dict, foreignCache, ForeignField.Name, foreignItem, CreateGlobalCacheKey(foreignCache, foreignItem, fieldValue), isItemDeleted);

			if (_SubstituteKey != null && (fieldValue = foreignCache.GetValue(foreignItem, _SubstituteKey.Name)) != null)
				CheckIntegrityAndPutGlobal(dict, foreignCache, _SubstituteKey.Name, foreignItem, CreateGlobalCacheKey(foreignCache, foreignItem, fieldValue), isItemDeleted);

			OnItemCached(foreignCache, foreignItem, isItemDeleted);
		}

		private void cacheOnReadItem(GlobalDictionary dict, PXCache foreignCache, String foreignField, Object foreignItem, Object ownKey, Boolean isItemDeleted)
	    {
			if (!CanCacheItem(dict, foreignCache, foreignItem))
				return;
			CheckIntegrityAndPutGlobal(dict, foreignCache, foreignField, foreignItem, CreateGlobalCacheKey(foreignCache, foreignItem, ownKey), isItemDeleted);
			OnItemCached(foreignCache, foreignItem, isItemDeleted);
		}

		protected virtual void OnItemCached(PXCache foreignCache, object foreignItem, bool isItemDeleted)
		{

		}

		protected class SubstituteKeyInfo : Tuple<string, int?, bool?>
		{
			public SubstituteKeyInfo() : this(null, null, null) { }
			public SubstituteKeyInfo(String mask, Int32? length, Boolean? isUnicode) : base(mask, length, isUnicode) { }
			public String Mask => Item1;
			public Int32? Length => Item2;
			public Boolean? IsUnicode => Item3;
		}
		protected static ConcurrentDictionary<Type, SubstituteKeyInfo> _substitutekeys = new ConcurrentDictionary<Type, SubstituteKeyInfo>();
		protected SubstituteKeyInfo getSubstituteKeyMask(PXCache sender)
		{
			SubstituteKeyInfo substituteKeyInfo = null;
			if (_SubstituteKey != null && !_substitutekeys.TryGetValue(_SubstituteKey, out substituteKeyInfo))
			{
				PXCache cache = sender.Graph._GetReadonlyCache(_Type);
				int? length = null;
				bool? isUnicode = null;
				string mask = null;
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_SubstituteKey.Name))
				{
					if (attr is PXDBStringAttribute)
					{
                        length = ((PXDBStringAttribute)attr).Length;
                        isUnicode = ((PXDBStringAttribute)attr).IsUnicode;
                        mask = ((PXDBStringAttribute)attr).InputMask;
					}
					else if (attr is PXStringAttribute)
					{
                        length = ((PXStringAttribute)attr).Length;
                        isUnicode = ((PXStringAttribute)attr).IsUnicode;
                        mask = ((PXStringAttribute)attr).InputMask;
					}
					if (mask != null)
						break;
				}

				if (cache.BqlTable.IsAssignableFrom(_Type))
				{
					_substitutekeys[_SubstituteKey] = substituteKeyInfo = new SubstituteKeyInfo(mask, length, isUnicode);
				}
			}
			return substituteKeyInfo ?? new SubstituteKeyInfo();
		}

        protected string getDescriptionName(PXCache sender, out int? length)
		{
			const string descriptionFieldFullName = "_DescriptionFieldFullName$";
			length = null;
			string displayname = null;
			KeyValuePair<string, int?> pair;
			Dictionary<string, KeyValuePair<string, int?>> descriptions = PXContext.GetSlot<Dictionary<string, KeyValuePair<string, int?>>>(descriptionFieldFullName + System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            if (descriptions == null)
            {
				PXContext.SetSlot(descriptionFieldFullName + System.Threading.Thread.CurrentThread.CurrentCulture.Name, 
                    descriptions = PXDatabase.GetSlot<Dictionary<string, KeyValuePair<string, int?>>>(descriptionFieldFullName + System.Threading.Thread.CurrentThread.CurrentCulture.Name, _BqlType));
			}
            var key = sender.Graph.GetType().FullName + "$" + _DescriptionField.FullName;
            var found = false;
            lock (((ICollection)descriptions).SyncRoot)
            {
                found = descriptions.TryGetValue(key, out pair);
            }
            if (!found)
            {
				PXCache cache = sender.Graph._GetReadonlyCache(_Type);
                foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_DescriptionField.Name))
                {
                    if (attr is PXUIFieldAttribute)
                    {
						displayname = ((PXUIFieldAttribute)attr).DisplayName;
					}
                    else if (attr is PXDBStringAttribute)
                    {
						length = ((PXDBStringAttribute)attr).Length;
					}
                    else if (attr is PXStringAttribute)
                    {
						length = ((PXStringAttribute)attr).Length;
					}
                    if (displayname != null && length != null)
                    {
						break;
					}
				}
                if (displayname == null)
                {
					displayname = _DescriptionField.Name;
				}
                if (cache.BqlTable.IsAssignableFrom(_Type))
                {
                    lock (((ICollection)descriptions).SyncRoot)
                    {
						descriptions[key] = new KeyValuePair<string, int?>(displayname, length);
					}
				}
			}
            else
            {
				displayname = pair.Key;
				length = pair.Value;
			}
            if (_FieldList != null && _HeaderList != null && _FieldList.Length == _HeaderList.Length)
            {
                for (int i = 0; i < _FieldList.Length; i++)
                {
					if (_FieldList[i] == _DescriptionField.Name)
						return _HeaderList[i];
				}
			}
			return displayname;
		}

		protected static string _GetSlotName(Type type, byte keysCount) => GlobalDictionary.SlotPrefix + $"{keysCount}keys_" + type.FullName;

		/// <exclude/>
		public virtual void SelfRowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (sender.RowId == null || !CanCacheGlobal(sender) || sender._AggregateSelecting || sender._SingleTableSelecting || e.Record?._isTableChanging == true) return;

			var dict = GetGlobalCache();
			lock (dict.SyncRoot)
			{
				object key = sender.GetValue(e.Row, sender.RowId);
				GlobalDictionary.CacheValue cacheValue;
				Object cacheKey = CreateGlobalCacheKey(sender, e.Row, key);
				if (key != null && !dict.TryGetValue(cacheKey, out cacheValue))
				{
					object row;
					if (sender.Graph.GetType() == typeof(PXGenericInqGrph) || PXView.CurrentRestrictedFields.Any() || PXFieldScope.IsScoped)
					{
                        var values = new Dictionary<string, object> { { _FieldName, sender.GetValue(e.Row, _FieldName) } };
						if (!string.Equals(_FieldName, sender.RowId, StringComparison.OrdinalIgnoreCase))
							values.Add(sender.RowId, key);
						if (_DescriptionField != null)
							values.Add(_DescriptionField.Name, sender.GetValue(e.Row, _DescriptionField.Name));
						AppendOtherValues(values, sender, e.Row);
						row = values;
					}
					else
					{
						row = e.Row;
					}
					dict.Set(cacheKey, row, false);
				}
			}
		}	
        /// <exclude/>
		public virtual void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			bool deleted = false;
            if (e.ReturnValue != null)
            {
				object item = null;
				GlobalDictionary dict = null;
                if (_CacheGlobal)
                {
					dict = GetGlobalCache();
                    lock (dict.SyncRoot)
                    {
						GlobalDictionary.CacheValue cacheValue;
                        if (dict.TryGetValue(CreateGlobalCacheKey(sender, e.Row, e.ReturnValue), out cacheValue))
                        {
							item = cacheValue.Item;
							deleted = cacheValue.IsDeleted;
						}
					}
				}
                if (item == null)
                {
                    if (e.ReturnValue.GetType() == sender.GetFieldType(_FieldName))
                    {
						PXCache itemCache = sender;
						readItem(itemCache, e.Row, e.ReturnValue, out itemCache, out item, ref deleted);
                        if (item != null)
                        {
							cacheOnReadItem(dict, itemCache, item, deleted);
							e.ReturnValue = itemCache.GetValue(item, _SubstituteKey.Name);
						}
					}
				}
                else
                {
					PXCache itemCache = sender.Graph.Caches[_Type];
					object prevReturnValue = e.ReturnValue;
					e.ReturnValue = itemCache.GetValue(item, _SubstituteKey.Name);
					if (e.ReturnValue == null)
					{
						readItem(itemCache, e.Row, prevReturnValue, out itemCache, out item, ref deleted);
						if (item != null)
                        {
							e.ReturnValue = itemCache.GetValue(item, _SubstituteKey.Name);
							if (e.ReturnValue != null)
								cacheOnReadItem(dict, itemCache, item, deleted);
						}
					}
				}
			}
            if (!e.IsAltered)
            {
				e.IsAltered = deleted || sender.HasAttributes(e.Row);
			}
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
				var keyInfo = getSubstituteKeyMask(sender);
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, keyInfo.Length, null, _FieldName, null, null, keyInfo.Mask, null, null, null, null);
				if (e.ReturnValue != null && e.ReturnValue.GetType() != typeof(string))
				{
					e.ReturnValue = e.ReturnValue.ToString();
				}
                if (deleted)
                {
					((PXFieldState)e.ReturnState).Error = ErrorMessages.ForeignRecordDeleted;
					((PXFieldState)e.ReturnState).ErrorLevel = PXErrorLevel.Warning;
					((PXFieldState)e.ReturnState).SelectorMode = sender.IsAutoNumber(_FieldName) ? SelectorMode : _SelectorMode;
                }
			}

			//if (e.ReturnState is PXFieldState )
			//{
			//    var returnState = (PXFieldState)e.ReturnState;
			//    returnState.ValueField = _SubstituteKey;
			//}
		}
        protected internal ObjectRef<bool> _BypassFieldVerifying;
        /// <exclude/>
		public virtual void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
	        e.NewValue = PXMacroManager.Default.TryResolveExt(e.NewValue, sender, FieldName, e.Row);
            if (!e.Cancel && e.NewValue != null)
            {
				object item = null;
				GlobalDictionary dict = null;
                if (_CacheGlobal)
                {
					dict = GetGlobalCache();
                    lock (dict.SyncRoot)
                    {
						GlobalDictionary.CacheValue cacheValue;
                        if (dict.TryGetValue(CreateGlobalCacheKey(sender, e.Row, e.NewValue), out cacheValue))
                        {
                            if (cacheValue.IsDeleted && !PXDatabase.ReadDeleted)
                            {
								throw new PXForeignRecordDeletedException();
							}
							item = cacheValue.Item;
						}
					}
				}
				if (item == null)
                {
					PXView select = GetView(sender, _NaturalSelect, !_DirtyRead);
					object[] pars = MakeParameters(e.NewValue, includeLookupJoins: true);
					bool bypass = e.NewValue.GetType() != select.Cache.GetFieldType(_SubstituteKey.Name) && e.NewValue.GetType() == sender.GetFieldType(_FieldName);
                    Func<object> readItem = () => SelectSingleBound(@select, new[] { e.Row }, _CacheGlobal ? new[] { e.NewValue } : pars);
                    if (!bypass)
                    {
						item = sender._InvokeSelectorGetter(e.Row, _FieldName, select, pars, true) ?? readItem();
					}
                    if (item != null)
                    {
						cacheOnReadItem(dict, @select.Cache, item);
						e.NewValue = select.Cache.GetValue(item, ForeignField.Name);
					}
                    else
                    {
                        using (new PXReadBranchRestrictedScope())
                        {
                            if (!bypass)
                            {
								item = readItem();
                                if (item == null && IsReadDeletedSupported)
                                {
                                    using (new PXReadDeletedScope())
                                    {
										item = readItem();
                                        if (item != null)
                                        {
											cacheOnReadItem(dict, @select.Cache, item);
											throw new PXForeignRecordDeletedException();
										}
									}
								}
							}
                            if (e.NewValue.GetType() == sender.GetFieldType(_FieldName))
                            {
								var view = GetViewWithParameters(sender, e.NewValue);
								item = null;
                                try
                                {
									item = view.SelectSingleBound(e.Row);
								}
                                catch (FormatException)
                                {
								}
                                if (item != null)
                                {
									return;
								}
							}
							_BypassFieldVerifying.Value = true;
                            try
                            {
								object val = e.NewValue;
								sender.OnFieldVerifying(_FieldName, e.Row, ref val, true);

                                if (val != null && val.GetType() == sender.GetFieldType(_FieldName))
                                {
									e.NewValue = val;
									return;
								}
							}
                            catch (Exception ex)
                            {
                                if (ex is PXSetPropertyException)
                                {
									throw PXException.PreserveStack(ex);
								}
							}
                            finally
                            {
								_BypassFieldVerifying.Value = false;
							}
							string[] restricted = item != null ? new string[] { true.ToString() } : hasRestrictedAccess(sender, _NaturalSelect, e.Row);
							throwNoItem(restricted, true, e.NewValue);
						}
					}
				}
                else
                {
					PXCache cache = sender.Graph.Caches[_Type];
					object p = e.NewValue;
					e.NewValue = cache.GetValue(item, ForeignField.Name);
                    if (e.NewValue == null)
                    {
						PXView select = GetView(sender, _NaturalSelect, !_DirtyRead);
						item = sender._InvokeSelectorGetter(e.Row, _FieldName, select, new object[] { p }, true) ?? SelectSingleBound(select, new object[] { e.Row }, p);
                        if (item != null)
                        {
							e.NewValue = select.Cache.GetValue(item, ForeignField.Name);
						}
                        else
                        {
							throwNoItem(hasRestrictedAccess(sender, _NaturalSelect, e.Row), true, p);
						}
					}
				}
			}
		}
        /// <exclude/>
		public virtual void SubstituteKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            var isInnerSubselect = e.Operation.Option() == PXDBOperation.SubselectForExport;
            if (ShouldPrepareCommandForSubstituteKey(e))
            {
				e.Cancel = true;
				foreach (PXDBFieldAttribute attr in sender.GetAttributes(_FieldName).OfType<PXDBFieldAttribute>())
				{
		            e.BqlTable = _BqlTable;
		            string extTable = _Type.Name + "Ext";

					SQLExpression outerColumn = null;
					if (attr is PXDBScalarAttribute)
					{
						var outerArgs =
							new PXCommandPreparingEventArgs(e.Row, e.Value, PXDBOperation.Select, e.Table, e.SqlDialect);
						attr.CommandPreparing(sender, outerArgs);
						outerColumn = outerArgs.Expr;
					}

					if (outerColumn == null)
					{
						outerColumn = new Column(sender.BqlSelect == null ? attr.DatabaseFieldName : _FieldName,
							e.Table ?? (isInnerSubselect ? attr.BqlTable : _BqlTable));
					}
								
					SimpleTable extSQLTable = new SimpleTable(extTable);
					Query q = new Query();
					q.Field(new Column(_SubstituteKey.Name, extSQLTable))
						.From(BqlCommand.GetSQLTable(_Type, sender.Graph).As(extTable))
						.Where(
							new Column(ForeignField.Name, extSQLTable)
							.EQ(outerColumn)
						);
					e.Expr = new SubQuery(q);

		            if (e.Value != null)
		            {
			            e.DataValue = e.Value;
			            e.DataType = PXDbType.NVarChar;
                        e.DataLength = ((string)e.Value).Length;
		            }
		            break;
	            }
            }
        }

	    protected static bool ShouldPrepareCommandForSubstituteKey(PXCommandPreparingEventArgs e)
	    {
	        bool isInnerSubselect = (e.Operation & PXDBOperation.Option) == PXDBOperation.SubselectForExport;
            return e.Operation.Command() == PXDBOperation.Select &&
	               (e.Operation.Option() == PXDBOperation.External || isInnerSubselect) &&
	               (e.Value == null || e.Value is string);
	    }

	    /// <exclude/>
		public virtual void DescriptionFieldCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Select &&
				e.Operation.Option() == PXDBOperation.External &&
				(e.Value == null || e.Value is string))
			{
				e.Cancel = true;
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(_FieldName))
				{
					if (attr is PXDBFieldAttribute)
					{
						e.BqlTable = _BqlTable;
						string extTable = _Type.Name + "Ext";
						
						Type descrTable = BqlCommand.GetItemType(_DescriptionField);
						PXCache descrCache = sender.Graph.Caches[descrTable];
						var descrFieldName = descrCache.GetFieldName(_DescriptionField.Name, false);

						SQLTree.Table sqlTable = BqlCommand.GetSQLTable(_Type, sender.Graph).As(extTable);
						SQLExpression sqlField = new Column(descrFieldName, sqlTable);

						//if (!String.IsNullOrEmpty(tableName) && !tableName.Trim().StartsWith(BqlCommand.SubSelect)) // do not expand description field to subselect in case when description field is being selected from another expanded subselect
						if (sqlTable is SimpleTable)
						{
							PXDBOperation operation = PXDBOperation.Select;
							if (PXDBLocalizableStringAttribute.IsEnabled && descrCache.GetAttributes(_DescriptionField.Name).Any(_ => _ is PXDBLocalizableStringAttribute))
							{
								operation |= PXDBOperation.External;
							}
							descrCache.RaiseCommandPreparing(_DescriptionField.Name, null, null, operation, descrTable, out var descrFieldDescription);

							if (descrFieldDescription?.Expr != null)
							{
								sqlField = descrFieldDescription?.Expr.substituteTableName(_Type.Name, extTable).substituteTableName(descrTable.Name, extTable);
							}
						}

						Query sq = new Query();
						sq.Field(sqlField).From(sqlTable).Where(
							new Column(ForeignField.Name, extTable).EQ(new Column(sender.BqlSelect == null ? ((PXDBFieldAttribute)attr).DatabaseFieldName : _FieldName, e.Table ?? _BqlTable))
						);
						e.Expr = new SubQuery(sq);

						if (e.Value != null)
						{
							e.DataValue = e.Value;
							e.DataType = PXDbType.NVarChar;
							e.DataLength = ((string)e.Value).Length;
						}
						break;
					}
				}
			}
		}
        /// <exclude/>
		public virtual void ForeignTableRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
            if (e.TranStatus == PXTranStatus.Completed)
            {
				ClearGlobalCache(_Type, KnownForeignKeysCount);
			}
		}
        /// <exclude/>
		public virtual void ReadDeletedFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null ||
				!ValidateValue ||
				e.Cancel ||
                _BqlTable.IsAssignableFrom(_BqlType) && sender.Keys.Count > 0 && String.Compare(sender.Keys[sender.Keys.Count - 1], _FieldName, StringComparison.OrdinalIgnoreCase) == 0)
            {
				return;
			}
			GlobalDictionary dict = null;
			object key = e.NewValue;
            if (_CacheGlobal)
            {
				dict = GetGlobalCache();
                lock (dict.SyncRoot)
                {
					GlobalDictionary.CacheValue cacheValue;
                    if (dict.TryGetValue(CreateGlobalCacheKey(sender, e.Row, key), out cacheValue))
                    {
                        if (cacheValue.IsDeleted && !PXDatabase.ReadDeleted)
                        {
							throw new PXForeignRecordDeletedException();
						}
                        else
                        {
							return;
						}
					}
				}
			}
			var view = GetViewWithParameters(sender, key);
			bool deleted = false;
			object item = view.SelectSingleBound(e.Row);
			if (item == null)
			{
                using (new PXReadDeletedScope())
                {
					item = view.SelectSingleBound(e.Row);
					deleted = true;
				}
			}
            if (item != null)
            {
                cacheOnReadItem(dict, view.Cache, ForeignField.Name, item, key, deleted);
                if (deleted)
					throw new PXForeignRecordDeletedException();
			}
		}
		#endregion

		#region Initialization
		protected static Dictionary<Type, List<KeyValuePair<string, Type>>> _SelectorFields = new Dictionary<Type, List<KeyValuePair<string, Type>>>();

		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);

            lock (((ICollection)_SelectorFields).SyncRoot)
            {
				List<KeyValuePair<string, Type>> list;
                if (!_SelectorFields.TryGetValue(bqlTable, out list))
                {
					_SelectorFields[bqlTable] = list = new List<KeyValuePair<string, Type>>();
				}
				bool found = list.Any(pair => pair.Key == base.FieldName);
				if (!found)
				{
					Type field = ForeignField;
					Type table = BqlCommand.GetItemType(field);
					if (table == null || !bqlTable.IsAssignableFrom(table) 
						|| !String.Equals(field.Name, base.FieldName, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(new KeyValuePair<string, Type>(base.FieldName, ForeignField));
					}
				}
			}

			_originalBqlTable = bqlTable;

			if (_AttributeLevel != PXAttributeLevel.Type) return;
			if (TableReferenceInspector == null || ReferenceConverter == null)
			{
				PXTrace.WriteWarning("Reference collection for {ReferenceOrigins} references is turned off because either " + nameof(ITableReferenceInspector) + " or " + nameof(SelectorToReferenceConverter) + " is not registered or attribute-level DI is not enabled", new[] { ReferenceOrigin.SelectorAttribute });
				return;
			}
			if (TableReferenceInspector.AllReferencesAreCollected) return;

			var reference = ReferenceConverter.CreateReference(this, _originalBqlTable);
			if (reference != null)
				TableReferenceInspector.CollectReference(reference);
		}

		/// <exclude/>
		public static List<KeyValuePair<string, Type>> GetSelectorFields(Type table)
		{
	        if (ServiceManager.EnsureCachesInstatiated(true))
	        {
                lock (((ICollection)_SelectorFields).SyncRoot)
		        {
			        List<KeyValuePair<string, Type>> list;
			        if (_SelectorFields.TryGetValue(table, out list))
			        {
						HashSet<string> distinct = null;
						List<KeyValuePair<string, Type>> ret = list;
						while ((table = table.BaseType) != typeof(object))
						{
							List<KeyValuePair<string, Type>> toMerge;
							if (_SelectorFields.TryGetValue(table, out toMerge))
							{
								if (distinct == null)
								{
									distinct = new HashSet<string>(list.Select(_ => _.Key));
									ret = new List<KeyValuePair<string, Type>>(list);
								}
								int cnt = ret.Count;
								ret.AddRange(toMerge.Where(_ => !distinct.Contains(_.Key)));
								ret.GetRange(cnt, ret.Count - cnt).ForEach(_ => distinct.Add(_.Key));
							}
						}
						return ret;
			        }
		        }
	        }
	        return new List<KeyValuePair<string, Type>>();
		}

		protected internal const string selectorBypassInit = "selectorBypassInit";

		protected void populateFields(PXCache sender, bool bypassInit)
		{
			string key;
		    var cacheType = sender.Graph.Caches.GetCacheType(_Type);
			if (_FieldList == null) 
			{
				Type t = _Type;
				if (cacheType != null && cacheType != _Type
					|| cacheType == null && (t = PXSubstManager.Substitute(_Type, sender.Graph.GetType())) != _Type)
				{
					if (cacheType != null)
					{
						key = cacheType.FullName + "$" + sender.Graph.GetType().FullName;
					}
					else
					{
						key = t.FullName + "$" + sender.Graph.GetType().FullName;
					}
				}
				else
				{
					key = _Type.FullName;
					if (sender.Graph.HasGraphSpecificFields(_Type))
					{
						key = key + "$" + sender.Graph.GetType().FullName;
					}
				}
			}
            else
            {
				key = sender.GetItemType().FullName + "$" + _FieldName;
				if (sender.IsGraphSpecificField(_FieldName)
					|| sender.Graph.HasGraphSpecificFields(_Type)
					|| cacheType != null && cacheType != _Type
					|| cacheType == null && PXSubstManager.Substitute(_Type, sender.Graph.GetType()) != _Type)
				{
					key = key + "$" + sender.Graph.GetType().FullName;
				}
			}
			string culture = key + "@" + System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
			FieldHeaderDictionaryIndependant fieldsheaders;
			if (PXDBAttributeAttribute._BqlTablesUsed.ContainsKey(_BqlType))
			{
				fieldsheaders = PXDatabase.GetSlotWithContextCache<FieldHeaderDictionaryDependant>(nameof(FieldHeaderDictionaryDependant), typeof(CSAttributeGroup), typeof(FeaturesSet));
			}
			else
			{
				fieldsheaders = PXDatabase.GetSlotWithContextCache<FieldHeaderDictionaryIndependant>(nameof(FieldHeaderDictionaryIndependant));
			}
			string[] headerspecified = _HeaderList;
			if (_FieldList == null)
			{
				_FieldList = fieldsheaders._fields.GetOrAddOrUpdate(key,
					(fieldkey) =>
					{
						if (bypassInit)
						{
							return null;
						}
						findFieldsHeaders(sender);
                        if (_FieldList?.Length == 0)
                        {
                            return null;
                        }
						return _FieldList;
					},
					(fieldkey, fieldvalue) =>
					{
						if (fieldvalue != null)
						{
							return fieldvalue;
						}
						if (bypassInit)
						{
							return null;
						}
						findFieldsHeaders(sender);
                        if (_FieldList?.Length == 0)
                        {
                            return null;
                        }
						return _FieldList;
					});
			}
			if (_FieldList != null)
			{
				_HeaderList = fieldsheaders._headers.GetOrAddOrUpdate(culture + "$" + string.Join(",", _FieldList),
					(headerkey) =>
					{
						if (bypassInit)
						{
							return null;
						}
						if (headerspecified != null)
						{
							_HeaderList = headerspecified;
							for (int i = 0; i < _HeaderList.Length; i++)
							{
								string msgprefix;
								_HeaderList[i] = PXMessages.Localize(_HeaderList[i], out msgprefix);
							}
						}
						else
						{
							findFieldsHeaders(sender);
						}
                        if (_HeaderList?.Length == 0)
                        {
                            return null;
                        }
						return _HeaderList;
					},
					(headerkey, headervalue) =>
					{
						if (headervalue != null)
						{
							return headervalue;
						}
						if (bypassInit)
						{
							return null;
						}
						if (headerspecified != null)
						{
							_HeaderList = headerspecified;
							for (int i = 0; i < _HeaderList.Length; i++)
							{
								string msgprefix;
								_HeaderList[i] = PXMessages.Localize(_HeaderList[i], out msgprefix);
							}
						}
						else
						{
							findFieldsHeaders(sender);
						}
                        if (_HeaderList?.Length == 0)
                        {
                            return null;
                        }
						return _HeaderList;
					});
			}
			if (_HeaderList == null && headerspecified != null)
			{
				_HeaderList = headerspecified;
				if (_FieldList != null)
				{
					for (int i = 0; i < _HeaderList.Length; i++)
					{
						string msgprefix;
						_HeaderList[i] = PXMessages.Localize(_HeaderList[i], out msgprefix);
					}
					fieldsheaders._fields.TryAdd(key, _FieldList);
					fieldsheaders._headers.TryAdd(culture + "$" + string.Join(",", _FieldList), _HeaderList);
				}
			}
		}
		
		protected void findFieldsHeaders(PXCache sender)
		{
			_HeaderList = new string[0];
			List<string> fields = new List<string>();
			List<string> headers = new List<string>();
			PXCache cache = sender.GetItemType() == _Type || sender.GetItemType().IsSubclassOf(_Type) && !Attribute.IsDefined(sender.GetItemType(), typeof(PXBreakInheritanceAttribute), false) ? sender : sender.Graph._GetReadonlyCache(_Type);
			PXContext.SetSlot<bool>(selectorBypassInit, true);
			try
			{
				if (_FieldList == null)
				{
					foreach (string name in cache.Fields)
					{
						PXFieldState st = cache.GetStateExt(null, name) as PXFieldState;
						if (st != null &&
							(st.Visibility.HasFlag(PXUIVisibility.SelectorVisible) ||
							st.Visibility.HasFlag(PXUIVisibility.Dynamic)))
						{
							fields.Add(st.Name);
							headers.Add(st.DisplayName);
						}
					}
				}
				else
				{
					for (int i = 0; i < _FieldList.Length; i++)
					{
						bool found = false;
						{
							PXFieldState st = cache.GetStateExt(null, _FieldList[i]) as PXFieldState;
							if (st != null)
							{
								fields.Add(_FieldList[i]);
								headers.Add(st.DisplayName);
								found = true;
							}
						}
						int idx;
						if (!found)
						{
							if ((idx = _FieldList[i].IndexOf("__")) > 0)
							{
								if (idx + 2 < _FieldList[i].Length)
								{
									string tname = _FieldList[i].Substring(0, idx);
									foreach (Type table in _LookupSelect.GetTables())
									{
										if (table.Name == tname)
										{
											string fname = _FieldList[i].Substring(idx + 2, _FieldList[i].Length - idx - 2);
											PXCache tcache = sender.Graph._GetReadonlyCache(table);
											{
												PXFieldState st = tcache.GetStateExt(null, fname) as PXFieldState;
												if (st != null)
												{
													fields.Add(_FieldList[i]);
													headers.Add(st.DisplayName);
												}
											}
											break;
										}
									}
								}
							}
							else if ((idx = _FieldList[i].IndexOf('_')) > 0)
							{
								string fname = _FieldList[i].Substring(0, idx);
								foreach (PXSelectorAttribute attr in cache.GetAttributes(fname).OfType<PXSelectorAttribute>())
								{
									if (attr._DescriptionField == null)
										continue;

									fields.Add(attr.FieldName);
									int? length;
									headers.Add(attr.getDescriptionName(sender, out length));
									break;
								}
							}
						}
					}
				}
			}
			catch (Exception)
			{
				_HeaderList = null;
#pragma warning disable CS0618 // Type or member is obsolete
				PXFirstChanceExceptionLogger.LogMessage("Failed to retrieve selector columns");
#pragma warning restore CS0618 // Type or member is obsolete
				throw;
			}
			finally
			{
				PXContext.SetSlot<bool>(selectorBypassInit, false);
			}
			_FieldList = fields.ToArray();
			_HeaderList = headers.ToArray();
		}

	    protected virtual void CreateFilter(PXGraph graph)
	    {
            var filterView = new PXSelectorFilterView(graph, this);
            PXFilterableAttribute.AddFilterView(graph, filterView, _ViewName);

            var detailView = new PXFilterDetailView(graph, _ViewName, new Type[0]);
            PXFilterableAttribute.AddFilterDetailView(graph, detailView, _ViewName);
        }

        /// <exclude/>
		public void CreateView(PXCache sender)
		{
			_ViewName = string.Format("_Cache#{0}_{1}{2}", sender.GetItemType().FullName, _FieldName, _ViewName);
			PXView view;
            if (sender.Graph.Views.TryGetValue(_ViewName, out view))
            {
                if (view.BqlSelect.GetType() != _LookupSelect.GetType())
                {
                    // as we have field name in viewname we don`t need random viewnames
					//if (!_IsOwnView) {
					//	_ViewName = Guid.NewGuid().ToString();
					//}
					view = null;
				}
			}
            if (view == null)
            {
                if (_ViewHandler != null)
                {
					if (_CacheGlobal) view = new adjustableViewGlobal(sender.Graph, true, _LookupSelect, _ViewHandler, sender, _FieldName);
					else view = new PXAdjustableView(sender.Graph, true, _LookupSelect, _ViewHandler);
				}
                else
                {
					view = _CacheGlobal ? new viewGlobal(sender.Graph, true, _LookupSelect, sender, _FieldName) : new PXView(sender.Graph, true, _LookupSelect);
				}
				sender.Graph.Views[_ViewName] = view;
				_IsOwnView = true;
                if (_DirtyRead)
                {
					view.IsReadOnly = false;
				}
				if (_Filterable)
				{
                    CreateFilter(sender.Graph);
                }
			}
		}

		private static readonly ConcurrentDictionary<Type, WeakSet<PXGraph>> SelfSelectingTables = new ConcurrentDictionary<Type, WeakSet<PXGraph>>();
        /// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			_UIFieldRef = new UIFieldRef();
			_CacheType = sender.GetItemType();
			_BypassFieldVerifying = new ObjectRef<bool>();
            if (_CacheGlobal)
            {
                if (IsSelfReferencing)
                {
					sender.RowPersisted += ForeignTableRowPersisted;
					var dict = GetGlobalCache();
				}
			}
			populateFields(sender, true);
			CreateView(sender);
			_ViewCreated = true;

			bool isSameField = String.Equals(_FieldName, ForeignField.Name, StringComparison.OrdinalIgnoreCase);
			if (IsSelfReferencing && isSameField)
			{
				SelectorMode |= PXSelectorMode.NoAutocomplete;

                if (sender.Graph.GetType() != typeof(PXGraph))
                {
					if ((SelfSelectingTables.ContainsKey(_Type) && SelfSelectingTables[_Type].Contains(sender.Graph)) == false)
					{
						sender.RowSelecting += SelfRowSelecting;
						SelfSelectingTables.GetOrAdd(_Type, key => new WeakSet<PXGraph>()).Add(sender.Graph);
					}
				}
			}
			else
			{
				EmitColumnForDescriptionField(sender);
			}

            if (_SubstituteKey != null)
            {
				string name = _FieldName.ToLower();
				sender.FieldSelectingEvents[name] += SubstituteKeyFieldSelecting;
				sender.FieldUpdatingEvents[name] += SubstituteKeyFieldUpdating;
                if (String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)
                {
					sender.CommandPreparingEvents[name] += SubstituteKeyCommandPreparing;
				}
			}
            else if (IsReadDeletedSupported)
            {
				sender.FieldVerifyingEvents[_FieldName.ToLower()] += ReadDeletedFieldVerifying;
				_CacheGlobal = true;
			}
		}

		protected void EmitDescriptionFieldAlias(PXCache sender, string alias)
		{
			if (_DescriptionField == null || sender.Fields.Contains(alias)) return;
			var lowerAlias = alias.ToLower();
			sender.Fields.Add(alias);
			sender.FieldSelectingEvents[lowerAlias] += (cache, e) => DescriptionFieldSelecting(cache, e, alias); 
			sender.CommandPreparingEvents[lowerAlias] += DescriptionFieldCommandPreparing;
		}

		protected virtual void EmitColumnForDescriptionField(PXCache sender)
		{
			if (_DescriptionField == null) return;
			EmitDescriptionFieldAlias(sender, $"{_FieldName}_{_Type.Name}_{_DescriptionField.Name}");
			EmitDescriptionFieldAlias(sender, $"{_FieldName}_description");
		}

		#endregion

        /// <exclude/>
		private sealed class adjustableViewGlobal : viewGlobal, IPXAdjustableView
		{
			public adjustableViewGlobal(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler, PXCache sender, string fieldName)
				: base(graph, isReadOnly, select, handler, sender, fieldName) { }
		}

        /// <exclude/>
		private class viewGlobal : PXView
		{
			private BqlCommand _select;
			private readonly PXCache _sender;
			private readonly string _fieldName;
			private PXSearchColumn[] _sorts;
			public viewGlobal(PXGraph graph, bool isReadOnly, BqlCommand select, PXCache sender, string fieldName)
				: base(graph, isReadOnly, select)
			{
				_select = select;
				_sender = sender;
				_fieldName = fieldName;
			}
			public viewGlobal(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler, PXCache sender, string fieldName)
				: base(graph, isReadOnly, select, handler)
			{
				_select = select;
				_sender = sender;
				_fieldName = fieldName;
			}

			protected override List<object> InvokeDelegate(object[] parameters)
			{
				_sorts = PXView._Executing.Peek().Sorts;
				return base.InvokeDelegate(parameters);
			}

			//protected override PXSearchColumn[] prepareSorts(string[] sortcolumns, bool[] descendings, object[] searches, int topCount, out bool needOverrideSort, out bool anySearch, ref bool resetTopCount)
			//{
			//	_sorts = base.prepareSorts(sortcolumns, descendings, searches, topCount, out needOverrideSort, out anySearch, ref resetTopCount);
			//	return _sorts;
			//}

			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				List<object> ret = null;
                if (startRow == 0 && maximumRows == 1 && searches != null && searches.Length == 1 && searches[0] != null)
                {
					var key = searches[0];
					Boolean cacheGlobal = base.Cache.Keys.Count <= 1;
					GlobalDictionary dict = null;

                    if (cacheGlobal)
                    {
						dict = GlobalDictionary.GetOrCreate(Cache.GetItemType(), Cache.BqlTable, 1);
						lock (dict.SyncRoot)
						{
							GlobalDictionary.CacheValue cacheValue;
                            if (dict.TryGetValue(searches[0], out cacheValue))
                            {
                                ret = new List<object> { cacheValue.Item };
							}
						} 
					}

                    if (ret == null)
                    {
						ret = base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
						bool deleted = false;
                        if ((ret == null || ret.Count == 0) && PXDatabase.IsReadDeletedSupported(Cache.BqlTable))
                        {
                            using (new PXReadDeletedScope())
                            {
								ret = base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
							}
							deleted = true;
						}
						if (ret != null && ret.Count == 1 && !PXDatabase.ReadDeleted && cacheGlobal)
						{
							PXSearchColumn col;
							if (sortcolumns.Length == 1 && !String.IsNullOrEmpty(sortcolumns[0])
								&& (col = _sorts.FirstOrDefault(_ => String.Equals(_.Column, sortcolumns[0], StringComparison.OrdinalIgnoreCase))) != null
								&& col.SearchValue != null)
							{
								object row = PXResult.UnwrapMain(ret[0]);
								CheckIntegrityAndPutGlobal(dict, base.Cache, sortcolumns[0], row, col.SearchValue, deleted, true);
								if (col.SearchValue is string && key is string && !String.Equals((string)col.SearchValue, (string)key) && String.Equals(((string)col.SearchValue).TrimEnd(), ((string)key).TrimEnd()))
								{
									lock (dict.SyncRoot)
									{
										dict.Set(key, row, deleted);
									}
								}
							}
						}
					}
				}
                if (ret == null)
                {
					ret = base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
				}
				return ret;
			}
		}


	    public class WithCachingByCompositeKeyAttribute : PXSelectorAttribute
	    {
			/// <summary>
			/// Indicate that the search query has only conditions with a key fields for item identification.
			/// </summary>
			public bool OnlyKeyConditions { get; set; }

			public WithCachingByCompositeKeyAttribute(Type search, Type additionalKeysRelation)
	            : base(search)
	        {
	            CacheGlobal = true;
	            AdditionalKeyRelations = additionalKeysRelation;
	        }
	        public WithCachingByCompositeKeyAttribute(Type search, Type additionalKeysRelation, Type[] fieldList)
	            : base(search, fieldList)
	        {
	            CacheGlobal = true;
	            AdditionalKeyRelations = additionalKeysRelation;
	        }

	        public WithCachingByCompositeKeyAttribute(Type search, Type additionalKeysRelation, Type lookupJoin, Type[] fieldList)
	            : base(search, lookupJoin, true, fieldList)
	        {
	            AdditionalKeyRelations = additionalKeysRelation;
	        }

			public sealed override Boolean CacheGlobal
	        {
	            get { return base.CacheGlobal; }
	            set { base.CacheGlobal = value; }
	        }

	        public IReadOnlyCollection<IFieldsRelation> AdditionalKeysRelationsArray { get; private set; }
	        private Type _additionalKeyRelations;
	        private Type AdditionalKeyRelations
	        {
	            get { return _additionalKeyRelations; }
	            set
	            {
	                if (FieldRelationArray.IsTypeArrayOrElement(value))
	                {
	                    _additionalKeyRelations = FieldRelationArray.EmptyOrSingleOrSelf(value);
	                    AdditionalKeysRelationsArray = FieldRelationArray.CheckAndExtractInstances(_additionalKeyRelations);
	                }
	                else
	                {
	                    throw new PXArgumentException(nameof(value), $"Unsupported value {value}.");
	                }
	            }
	        }

	        protected override BqlCommand BuildNaturalSelect(Boolean cacheGlobal, Type substituteKey)
	        {
	            Type additionalKeyCondition = AdditionalKeysRelationsArray.ToWhere();
	            Type surrogateCondition = BqlCommand.Compose(typeof(Where<,>), substituteKey, typeof(Equal<>), typeof(Required<>), substituteKey);
	            Type naturalCondition = additionalKeyCondition == null
	                ? surrogateCondition
	                : BqlCommand.Compose(typeof(Where2<,>), additionalKeyCondition, typeof(And<>), surrogateCondition);

	            return cacheGlobal
	                ? BqlCommand.CreateInstance(typeof(Search<,>), ForeignField, naturalCondition)
	                : _Select.WhereAnd(naturalCondition);
	        }

	        protected override void AppendOtherValues(Dictionary<String, Object> values, PXCache cache, Object row)
	        {
	            var additionalPairs = AdditionalKeysRelationsArray
	                .Select(f => new
	                             {
	                                 FieldName = f.FieldOfParentTable.Name,
	                                 Value = cache.GetValue(row, f.FieldOfParentTable.Name)
	                             });
	            foreach (var pair in additionalPairs)
	                values.Add(pair.FieldName, pair.Value);
	        }

			protected override void Verify(PXCache sender, PXFieldVerifyingEventArgs e, ref object item)
			{
				bool itemFromCache = false;
				GlobalDictionary dict = null;
				if (_CacheGlobal && OnlyKeyConditions)
				{
					dict = GetGlobalCache();
					lock (dict.SyncRoot)
					{
						GlobalDictionary.CacheValue cacheValue;
						if (dict.TryGetValue(CreateGlobalCacheKey(sender, e.Row, e.NewValue), out cacheValue) && !cacheValue.IsDeleted)
						{
							item = cacheValue.Item;
							itemFromCache = true;
						}
					}
				}
				base.Verify(sender, e, ref item);
				if (dict != null && item != null && !itemFromCache)
					cacheOnReadItem(dict, sender.Graph.Caches[_Type], item);
			}

			protected override Byte KnownForeignKeysCount => (byte)(1 + AdditionalKeysRelationsArray.Count);

	        protected override Object CreateGlobalCacheKey(PXCache cache, Object row, Object keyValue)
	            => CreateCompositeKey(cache, row, keyValue, AdditionalKeysRelationsArray, _Type.IsAssignableFrom(cache.GetItemType()));

	        internal static Composite CreateCompositeKey(PXCache cache, Object row, Object keyValue, IEnumerable<IFieldsRelation> additionalFieldsRelations, bool isForeign)
	            => Composite.Create(new List<object>(additionalFieldsRelations.Select(f => cache.GetValue(row, isForeign ? f.FieldOfParentTable.Name : f.FieldOfChildTable.Name))) { keyValue }.ToArray());
	    }
    }
	#endregion

	#region PXCustomSelectorAttribute
    /// <summary>The base class for custom selector attributes. To create a custom selector attribute, you derive a class from this class and implement the
    /// <tt>GetRecords()</tt> method, as shown in the example below.</summary>
    /// <remarks>You can also override the <see cref="PXCustomSelectorAttribute.CacheAttached">CacheAttached</see> method to add initialization logic and override
    /// the <see cref="PXCustomSelectorAttribute.FieldVerifying">FieldVerifying</see> method to redefine the verification logic for the field value.</remarks>
    /// <example>
    ///   <code title="" description="" lang="CS">
    /// public class MyCustomSelector : PXCustomSelectorAttribute
    /// {
    ///     public MyCustomSelector(Type type)
    ///         : base(type) { }
    ///     
    ///     public virtual IEnumerable GetRecords()
    ///     {
    ///         ...
    ///     }
    /// }</code>
    /// </example>
	public class PXCustomSelectorAttribute : PXSelectorAttribute
	{
		readonly long createDate;
		protected PXGraph _Graph;
		private bool _raiseFieldSelecting = true;

		#region Ctor
        /// <summary>Initializes a new instance with the specified BQL query for selecting the data records to show to the user.</summary>
        /// <param name="type">A BQL query that defines the data set that is shown to the user along with the key field that is used as a value. Set to a field (type part of a DAC field) to
        /// select all data records from the referenced table. Set to a BQL command of <tt>Search</tt> type to specify a complex select statement.</param>
		public PXCustomSelectorAttribute(Type type)
			: base(type)
		{
			this.createDate = DateTime.Now.Ticks;
		}

        /// <summary>Initializes a new instance that will use the specified BQL query to retrieve the data records to select from and display the provided set of columns.</summary>
        /// <param name="type">A BQL query that defines the data set that is shown to the user along with the key field that is used as a value. Set to a field (type part of a DAC field) to
        /// select all data records from the referenced table. Set to a BQL command of <tt>Search</tt> type to specify a complex select statement.</param>
        /// <param name="fieldList">Fields to display in the control.</param>
		public PXCustomSelectorAttribute(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
			this.createDate = DateTime.Now.Ticks;
		}

        /// <exclude/>
		protected sealed class FilteredView : PXView
		{
			private PXView _OuterView;
			private PXView _TemplateView;
			private PXCustomSelectorAttribute _attribute;

			public FilteredView(PXView outerView, PXView templateView)
				: base(templateView.Graph, templateView.IsReadOnly, templateView.BqlSelect)
			{
				_OuterView = outerView;
				_TemplateView = templateView;
			}

			public FilteredView(PXView outerView, PXView templateView, PXCustomSelectorAttribute attribute)
				: base(templateView.Graph, templateView.IsReadOnly, templateView.BqlSelect)
			{
				_OuterView = outerView;
				_TemplateView = templateView;
				_attribute = attribute;
			}

			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
                if (parameters != null && parameters.Length > 0)
                {
					string[] names = _TemplateView.GetParameterNames();
					int idx;
                    if (names.Length > 0 && !String.IsNullOrEmpty(names[names.Length - 1]) && (idx = names[names.Length - 1].LastIndexOf('.')) != -1 && idx + 1 < names[names.Length - 1].Length)
                    {
						string field = names[names.Length - 1].Substring(idx + 1);
						object val = parameters[parameters.Length - 1];
						bool _topRaiseFieldSelecting = true;
						if (_attribute != null)
							_topRaiseFieldSelecting = _attribute._raiseFieldSelecting;
                        try
                        {
							if (_topRaiseFieldSelecting)
							{
								if (_attribute != null)
								_attribute._raiseFieldSelecting = false;
							Cache.RaiseFieldSelecting(field, currents != null && currents.Length > 0 ? currents[0] : null, ref val, false);
							val = PXFieldState.UnwrapValue(val);
						}
						}
                        catch
                        {
						}
						finally
						{
							if (_attribute != null && _topRaiseFieldSelecting)
								_attribute._raiseFieldSelecting = true;
						}
                        if (val == null)
                        {
							val = parameters[parameters.Length - 1];
						}
						PXFilterRow filter = new PXFilterRow(field, PXCondition.EQ, val);
                        if (filters == null || filters.Length == 0)
                        {
							filters = new PXFilterRow[] { filter };
						}
                        else
                        {
							filters = filters.Append(filter).ToArray();
                            if (filters.Length > 2)
                            {
								filters[0].OpenBrackets++;
								filters[filters.Length - 2].CloseBrackets++;
							}
							filters[filters.Length - 2].OrOperator = false;
						}
						Array.Resize(ref parameters, parameters.Length - 1);
					}
				}
				return _OuterView.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}
		}
		protected override PXView GetView(PXCache cache, BqlCommand select, bool isReadOnly)
		{
			return new FilteredView(cache.Graph.Views[_ViewName], base.GetView(cache, select, isReadOnly), this);
		}

		protected override PXView GetUnconditionalView(PXCache cache) => GetView(cache, _UnconditionalSelect, !DirtyRead);

		#endregion

		#region Implementation

		public override bool ExcludeFromReferenceGeneratingProcess { get; set; } = true;

        /// <summary>
        /// The handler of the <tt>FieldVerifying</tt> event.
        /// </summary>
        /// <param name="sender">The cache object that has raised the event.</param>
        /// <param name="e">The event arguments.</param>
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!ValidateValue)
				return;

            if (e.NewValue == null)
            {
				return;
			}

            if (sender.Keys.Count == 0 || _FieldName != sender.Keys[sender.Keys.Count - 1])
            {
				List<object> records = sender.Graph.Views[_ViewName].SelectMultiBound(new object[] { e.Row });
				PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(ForeignField)];
                foreach (object rec in records)
                {
					object item = PXResult.UnwrapMain(rec);
					object val = cache.GetValue(item, ForeignField.Name);
					if (Equals(val, e.NewValue))
						return;
					if (val is Array && e.NewValue is Array
						&& ((Array)val).Length == ((Array)e.NewValue).Length)
					{
						bool meet = true;
						for (int i = 0; i < ((Array)val).Length; i++)
						{
							if (!(meet = meet && Equals(((Array)val).GetValue(i), ((Array)e.NewValue).GetValue(i))))
							{
								break;
							}
						}
						if (meet)
						{
							return;
						}
					}
				}
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, _FieldName));
			}
		}

		private string GetHash()
		{
			return this.GetType().FullName + this.createDate.ToString();
		}
		#endregion

		#region Initialization
		private static readonly Type[] SelectDelegateMap =
		{
			typeof(PXSelectDelegate),
			typeof(PXSelectDelegate<>),
			typeof(PXSelectDelegate<,>),
			typeof(PXSelectDelegate<,,>),
			typeof(PXSelectDelegate<,,,>),
			typeof(PXSelectDelegate<,,,,>),
			typeof(PXSelectDelegate<,,,,,>),
			typeof(PXSelectDelegate<,,,,,,>),
			typeof(PXSelectDelegate<,,,,,,,>),
			typeof(PXSelectDelegate<,,,,,,,,>),
			typeof(PXSelectDelegate<,,,,,,,,,>),
			typeof(PXSelectDelegate<,,,,,,,,,,>),
		};
		
        /// <exclude/>
		private delegate PXView CreateViewDelegate(PXCustomSelectorAttribute attr, PXGraph graph, bool IsReadOnly, BqlCommand select);
		// Dictionary of createView delegates for each user-defined class which are derived from PXCustomSelector, dictionary key - is a type of derived class
		private static readonly Dictionary<string, CreateViewDelegate> createView = new Dictionary<string, CreateViewDelegate>();
		private static readonly object _vlock = new object();


		[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]

		private static CreateViewDelegate CreateDelegate(PXCustomSelectorAttribute attr)
		{
			DynamicMethod dm;
            if (!PXGraph.IsRestricted)
            {
				dm = new DynamicMethod("InitView", typeof(PXView), new Type[] { typeof(PXCustomSelectorAttribute), typeof(PXGraph), typeof(bool), typeof(BqlCommand) }, typeof(PXCustomSelectorAttribute), true);
			}
            else
            {
				dm = new DynamicMethod("InitView", typeof(PXView), new Type[] { typeof(PXCustomSelectorAttribute), typeof(PXGraph), typeof(bool), typeof(BqlCommand) }, true);
			}
			MethodInfo del = attr.GetType().GetMethod("GetRecords", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (del == null)
				return null;

			Type tdel = null;
			ParameterInfo[] pars = del.GetParameters();
			if (typeof(IEnumerable).IsAssignableFrom(del.ReturnType))
			{
				if (pars.Length <= SelectDelegateMap.Length)
				{
					tdel = SelectDelegateMap[pars.Length];
					if (pars.Length > 0)
						tdel = tdel.MakeGenericType(pars.Select(p => p.ParameterType).ToArray());
				}
			}
			else if (pars.Length == 0)
			{
				tdel = typeof(PXPrepareDelegate);
			}
			if (tdel == null)
				return null;

			ILGenerator ilgen = dm.GetILGenerator();
			LocalBuilder result = ilgen.DeclareLocal(typeof(PXView));
			ilgen.Emit(OpCodes.Nop);
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.Emit(OpCodes.Ldarg_2);
			ilgen.Emit(OpCodes.Ldarg_3);
			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Castclass, attr.GetType());
			ilgen.Emit(OpCodes.Ldftn, del);
			ilgen.Emit(OpCodes.Newobj, tdel.GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			ilgen.Emit(OpCodes.Newobj, typeof(PXView).GetConstructor(new Type[] { typeof(PXGraph), typeof(bool), typeof(BqlCommand), typeof(Delegate) }));
			ilgen.Emit(OpCodes.Stloc, result.LocalIndex);
			ilgen.Emit(OpCodes.Ldloc, result.LocalIndex);
			ilgen.Emit(OpCodes.Ret);

			return (CreateViewDelegate)dm.CreateDelegate(typeof(CreateViewDelegate));
		}

		protected bool writeLog = false;
        /// <summary>
        /// The method executed when the attribute is copied to the cache level.
        /// </summary>
        /// <param name="sender">The cache object that has raised the event.</param>
        public override void CacheAttached(PXCache sender)
		{
			_CacheType = sender.GetItemType();
			_Graph = sender.Graph;
			_BypassFieldVerifying = new ObjectRef<bool>();

            string hashCode = this.GetHash();

            lock (_vlock)
            {
				if (!createView.ContainsKey(hashCode))
					createView.Add(hashCode, CreateDelegate(this));
			}

			populateFields(sender, true);

			PXView view;
			_ViewName = string.Format("_{0}{1}{2}", sender.GetItemType().Name, _FieldName, _ViewName);
			if (!sender.Graph.Views.TryGetValue(_ViewName, out view) || view.BqlTarget != this.GetType())
			{
				lock (_vlock)
				{
					view = createView[hashCode](this, sender.Graph, !_DirtyRead, _Select);
				}
				sender.Graph.Views[_ViewName] = view;
				if (_Filterable)
				{
                    CreateFilter(sender.Graph);
				}
			}

            if (!IsSelfReferencing)
            {
				EmitColumnForDescriptionField(sender);
			}

            if (_SubstituteKey != null)
            {
				string name = _FieldName.ToLower();
				sender.FieldSelectingEvents[name] += SubstituteKeyFieldSelecting;
				sender.FieldUpdatingEvents[name] += SubstituteKeyFieldUpdating;
                if (String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    sender.CommandPreparingEvents[name] += SubstituteKeyCommandPreparing;
		        }
			}
		}
		#endregion
	}
	#endregion

	public class PXCustomSelectorFromGraphViewAttribute : PXCustomSelectorAttribute
	{
		readonly string _viewName;

		public PXCustomSelectorFromGraphViewAttribute(Type selectorField, string viewName) : base(selectorField)
		{
			_viewName = viewName;
		}

		protected virtual IEnumerable GetRecords()
		{
			var v = this._Graph.Views[_viewName];
			return v.SelectMulti();
			
			//var m = this._Graph.GetType().GetMethod(_delegateName,
			//    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			//if (m == null)
			//    yield break;

			//if (!typeof(IEnumerable).IsAssignableFrom(m.ReturnType))
			//    throw new PXException("Invalid delegate");



			//var rows = (IEnumerable)m.Invoke(this._Graph, null);


			//foreach (var ret in rows)
			//{
			//    yield return ret;
			//}



		}
	}

	#region PXSelectorByMethodAttribute
	/// <summary>
	/// Selector that extracts records by calling provided static method of a provided type. Method must take no parameters and return IEnumerable implementor
	/// </summary>
	public class PXSelectorByMethodAttribute : PXCustomSelectorAttribute
	{
		/// <summary>
		/// Caches compiled functions. Later the compiled function will be avaliable by the function key which is a tuple of function name and the type than contains it
		/// </summary>
		private static readonly ConcurrentDictionary<Tuple<Type, string>, Func<IEnumerable>> FunctionCache = new ConcurrentDictionary<Tuple<Type, string>, Func<IEnumerable>>();
		/// <summary>
		/// Key to get the data providing function from the function cache
		/// </summary>
		private readonly Tuple<Type, string> _functionCacheKey;

		/// <summary>
		/// Initialize a new instance of a selector which retrieves records by a provided method call
		/// </summary>
		/// <param name="dataProviderType">Type, containing record-providing method</param>
		/// <param name="dataProvidingMethodName">Name of a <b>static<b/> record-providing method. Signature of the method should be "IEnumerable MethodName()"</param>
		/// <param name="selectingField">BQL field whose value should be selected</param>
		/// <param name="displayingFieldList">List of BQL fields which should be displayed in selector's grid</param>
		public PXSelectorByMethodAttribute(Type dataProviderType, string dataProvidingMethodName, Type selectingField, params Type[] displayingFieldList)
			: base(selectingField, displayingFieldList)
		{
			if (dataProviderType == null)
				throw new ArgumentNullException(nameof(dataProviderType));
			if (dataProvidingMethodName == null)
				throw new ArgumentNullException(nameof(dataProvidingMethodName));

			_functionCacheKey = Tuple.Create(dataProviderType, dataProvidingMethodName);
			if (FunctionCache.ContainsKey(_functionCacheKey) == false)
			{
				var dataProvidingMethod =
					dataProviderType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
									.FirstOrDefault(m => m.Name == dataProvidingMethodName
														 && typeof(IEnumerable).IsAssignableFrom(m.ReturnType)
														 && m.GetParameters().Any() == false);
				if (dataProvidingMethod == null)
					throw new ArgumentException(
						$"Static method \"IEnumerable {dataProvidingMethodName}()\" does not exist in {dataProviderType.FullName} type",
						nameof(dataProvidingMethodName));

				// Compile method call to avoid reflection usage
				var getRecords = Expression.Lambda<Func<IEnumerable>>(
					Expression.Call(null, dataProvidingMethod),
					Enumerable.Empty<ParameterExpression>())
										   .Compile();

				FunctionCache.TryAdd(_functionCacheKey, getRecords);
			}
		}

		protected virtual IEnumerable GetRecords() => FunctionCache[_functionCacheKey].Invoke();
	} 
	#endregion
	
    /// <exclude/>
	public class UIFieldRef
	{
		public PXUIFieldAttribute UIFieldAttribute;
	}

    /// <exclude/>
	public class ForeignKeyChecker
	{
		private PXCache _sender;
		private object _row;
		private Type _fieldType, _searchType;
		public string CustomMessage = null;

		public ForeignKeyChecker(PXCache sender, object row, Type fieldType, Type searchType)
		{
			_sender = sender;
			_row = row;
			_fieldType = fieldType;
			_searchType = searchType;
		}

		public void DoCheck()
		{
			string foreingTableName;
			string currentTableName;
			if (isExists(out currentTableName, out foreingTableName))
			{
				string message = String.IsNullOrEmpty(CustomMessage) ? ErrorMessages.ExtRefError : CustomMessage;
				if (message.Contains("{0}") && message.Contains("{1}"))
				{
					message = PXLocalizer.LocalizeFormat(message, currentTableName, foreingTableName);
				}
				else
				{
					message = PXLocalizer.Localize(message);
				}
				throw new PXException(message);
			}
		}

		private bool isExists(out string currentTableName, out string foreingTableName)
		{
			if (_searchType != null && !typeof(IBqlSearch).IsAssignableFrom(_searchType))
			{
				throw new PXArgumentException("selectType", ErrorMessages.ArgumentException);
			}
			Type currentTableType = _row.GetType();
			Type cmd;
			Type tableType = BqlCommand.GetItemType(_fieldType);
			foreingTableName = getTableName(tableType);
			currentTableName = getTableName(currentTableType);
			Type currentFieldType = getCurrentFieldType(_sender, _fieldType);
			if (currentFieldType == null)
			{
				return false;
			}
			if (_searchType == null)
			{
				cmd = BqlCommand.Compose(
					typeof(Search<,>),
					_fieldType,
					typeof(Where<,>),
					_fieldType,
					typeof(Equal<>), typeof(Current<>), currentFieldType);
			}
			else
			{
				IBqlSearch Select = (IBqlSearch)Activator.CreateInstance(_searchType);
				if (Select.GetType() != _searchType)
				{
					throw new PXArgumentException("selectType", ErrorMessages.ArgumentException);
				}
				List<Type> args = new List<Type> { _searchType.GetGenericTypeDefinition() };
				args.AddRange(_searchType.GetGenericArguments());
				int j = args.FindIndex(arg => typeof(IBqlWhere).IsAssignableFrom(arg));
				if (j == -1)
				{
					throw new PXArgumentException("selectType", ErrorMessages.ArgumentException);
				}
				args[j] = BqlCommand.Compose(
					typeof(Where2<,>),
					typeof(Where<,>),
					_fieldType,
					typeof(Equal<>), typeof(Current<>), currentFieldType,
					typeof(And<>),
					args[j]);
				cmd = BqlCommand.Compose(args.ToArray());
			}
			PXView view = new PXView(_sender.Graph, false, BqlCommand.CreateInstance(cmd));
			object refObject = view.SelectSingleBound(new object[] { _row });
			return refObject != null;
		}

		private string getTableName(Type TableType)
		{
			if (TableType.IsDefined(typeof(PXCacheNameAttribute), true))
			{
				var attr = (PXCacheNameAttribute)TableType.GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0];
				return attr.GetName();
			}
			return TableType.Name;
		}

		private Type getCurrentFieldType(PXCache sender, Type foreignFieldType)
		{
			return sender.Graph
						.Caches[BqlCommand.GetItemType(foreignFieldType)]
						.GetAttributesReadonly(foreignFieldType.Name)
						.OfType<PXSelectorAttribute>()
						.Select(s => s.Field)
						.FirstOrDefault();
		}
	}

    public class PopupMessageAttribute : PXEventSubscriberAttribute
    {
        public override void CacheAttached(PXCache sender)
        {
            IEnumerable<PXSelectorAttribute> selectors = sender.GetAttributes(_FieldName)
                                                         .OfType<PXSelectorAttribute>();
            foreach (PXSelectorAttribute s in selectors)
            {
                s.ShowPopupMessage = true;
            }
        }
    }
}
