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
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using DotNetOpenAuth.Messaging;
using PX.Common;
using PX.SM;
using PX.Api;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.ReferentialIntegrity;
using PX.Translation;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using PX.Data.BQL;
using PX.Data.SQLTree;

namespace PX.Data
{
	#region PXDefaultAttribute
	/// <summary>Defines different ways the <tt>PXDefault</tt> attribute
	/// checks the field value before a data record with this field is saved
	/// to the database.</summary>
	public enum PXPersistingCheck
	{
		/// <summary>Check that the field value is not <tt>null</tt>.</summary>
		/// <remarks>Note that the user interface (UI) trims string values, so for
		/// fields displayed in the UI, values containing only whitespace
		/// characters will also be rejected.</remarks>
		Null,
		/// <summary>Check that the field value is not <tt>null</tt> and is not a
		/// string that contains only whitespace characters.</summary>
		NullOrBlank,
		/// <summary>Do not check the field value.</summary>
		Nothing
	}

	/// <summary>Allows you to have a multi-language field with a default value in a specific language.</summary>
	/// <remarks>Use this attribute instead of the <tt>PXDefault</tt> attribute and specify in its second parameter either a BQL field or a BQL select that provides language
	/// selection.</remarks>
	/// <example>
	/// 	<code title="Example" description="For example, in Acumatica ERP, the SOLine line description defaulted to the appropriate InventoryItem description based on the language that is set for a customer. The TransactionDesr field of the SOLine DAC has the PXLocalizableDefault attribute with a second parameter that specifies the language as follows: typeof(Customer.languageName). " lang="CS">
	/// PXLocalizableDefault(typeof(Search&lt;InventoryItem.descr,
	///    Where&lt;InventoryItem.inventoryID, 
	///    Equal&lt;Current&lt;SOLine.inventoryID&gt;&gt;&gt;&gt;),
	///    typeof(Customer.languageName), 
	///    PersistingCheck = PXPersistingCheck.Nothing)]</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXDefaultAttribute))]
	public class PXLocalizableDefaultAttribute : PXDefaultAttribute
	{
		#region State
		protected Type _LanguageSourceType;
		protected string _LanguageSourceField;
		protected BqlCommand _LanguageSelect;
		#endregion

		#region Ctor
		/// <exclude/>
		public PXLocalizableDefaultAttribute(Type sourceType, Type languageSourceType)
			: base(sourceType)
		{
			if (languageSourceType == null)
			{
				throw new PXArgumentException("languageType", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(languageSourceType))
			{
				_LanguageSelect = BqlCommand.CreateInstance(languageSourceType);
				_LanguageSourceType = BqlCommand.GetItemType(((IBqlSearch)_LanguageSelect).GetField());
				_LanguageSourceField = ((IBqlSearch)_LanguageSelect).GetField().Name;
			}
			else if (typeof(IBqlSelect).IsAssignableFrom(languageSourceType))
			{
				_LanguageSelect = BqlCommand.CreateInstance(languageSourceType);
				_LanguageSourceType = _LanguageSelect.GetTables()[0];
			}
			else if (languageSourceType.IsNested && typeof(IBqlField).IsAssignableFrom(languageSourceType))
			{
				_LanguageSourceType = BqlCommand.GetItemType(languageSourceType);
				_LanguageSourceField = languageSourceType.Name;
			}
			else if (typeof(IBqlTable).IsAssignableFrom(languageSourceType))
			{
				_LanguageSourceType = languageSourceType;
			}
			else
			{
				throw new PXArgumentException("languageType", ErrorMessages.CantCreateForeignKeyReference, sourceType);
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			string lang = null;
			if (e.NewValue == null && PXDBLocalizableStringAttribute.IsEnabled)
			{
				if (_LanguageSelect != null && !_SearchOnDefault && e.Row == null)
				{
				}
				else if (_LanguageSelect != null)
				{
					PXView view = sender.Graph.TypedViews.GetView(_LanguageSelect, true);
					object row = view.SelectSingleBound(new object[] { e.Row });
					if (row is PXResult)
					{
						row = ((PXResult)row)[_LanguageSourceType];
					}
					if (row != null)
					{
						lang = sender.Graph.Caches[_LanguageSourceType].GetValue(row, _LanguageSourceField ?? _FieldName) as string;
					}
				}
				else if (_LanguageSourceType != null)
				{
					PXCache cache = sender.Graph.Caches[_LanguageSourceType];
					if (cache.Current != null)
					{
						lang = cache.GetValue(cache.Current, _LanguageSourceField == null ? _FieldName : _LanguageSourceField) as string;
					}
				}
			}
			if (String.IsNullOrWhiteSpace(lang))
			{
				base.FieldDefaulting(sender, e);
			}
			else
			{
				string fieldName = (_SourceField == null ? _FieldName : _SourceField);
				if (_Select != null)
				{
					Tuple<object, PXCache> pair = PXDefaultAttribute.SelectRow(sender, this, e.Row);
					if (pair != null && pair.Item1 != null)
					{
						e.NewValue = PXDBLocalizableStringAttribute.GetTranslation(pair.Item2, pair.Item1, fieldName, lang);
						if (e.NewValue != null)
						{
							e.Cancel = true;
							return;
						}
					}
				}
				else if (_SourceType != null)
				{
					PXCache cache = sender.Graph.Caches[_SourceType];
					if (cache.Current != null)
					{
						e.NewValue = PXDBLocalizableStringAttribute.GetTranslation(cache, cache.Current, fieldName, lang);
						if (e.NewValue != null)
						{
							e.Cancel = true;
							return;
						}
					}
				}
				else if (_Formula != null)
				{
					bool? result = null;
					object value = null;
					BqlFormula.Verify(sender, e.Row, _Formula, ref result, ref value);
					e.NewValue = value;
				}

				if (_Constant != null)
				{
					e.NewValue = _Constant;
				}
			}
		}
		#endregion
	}


	/// <summary>Sets the default value for a DAC field.</summary>
	/// <remarks>
	///   <para>The <tt>PXDefault</tt> attribute provides the default value for a DAC field. The default value is assigned to the field when the cache raises the
	/// <tt>FiedlDefaulting</tt> event. This happens when the a new row is inserted in code or through the user interface.</para>
	///   <para>A value specified as default can be a constant or the result of a BQL query. If you provide a BQL query, the attribute will execute it on the
	/// <tt>FieldDefaulting</tt> event. You can specify both, in which case the attribute first executes the BQL query and uses the constant if the BQL query returns
	/// an empty set. If you provide a DAC field as the BQL query, the attribute takes the value of this field from the cache object's <tt>Current</tt> property. The
	/// attribute uses the cache object of the DAC type in which the field is defined.</para>
	///   <para>The <tt>PXDefault</tt> attribute also checks that the field value is not <tt>null</tt> before saving to the database. You can adjust this behavior using the
	/// <tt>PersistingCheck</tt> property. Its value indicates whether the attribute should check that the value is not <tt>null</tt>, check that the value is not
	/// <tt>null</tt> or a blank string, or not check.</para>
	///   <para>The attribute can redirect the error that happened on the field to another field if you set the <tt>MapErrorTo</tt> property.</para>
	///   <para>You can use the static methods to change the attribute properties for a particular data record in the cache or for all data record in the cache.</para>
	/// </remarks>
	/// <example>
	/// The attribute below sets a constant as the default value.
	/// <code title="" description="" lang="CS">
	/// [PXDefault(false)]
	/// public virtural bool? IsActive { get; set; }</code><code title="Example" description="The attribute below provides a string constants that is converted to the default value of the specific type." lang="CS">
	/// [PXDefault(TypeCode.Decimal, "0.0")]
	/// public virtual Decimal? AdjDiscAmt { get; set; }</code><code title="Example2" description="The attribute below will take the default value from the &lt;tt&gt;ARPayment&lt;/tt&gt; cache object and won't check the field value on saving of the changes to the database." lang="CS">
	/// [PXDefault(typeof(ARPayment.adjDate),
	///            PersistingCheck = PXPersistingCheck.Nothing)]
	/// public virtual DateTime? TillDate { get; set; }</code><code title="Example3" description="The attribute below only prevents the field from being null and does not set a default value." lang="CS">
	/// [PXDefault]
	/// public virtual string BAccountAcctCD { get; set; }</code><code title="Example4" description="The attribute below will execute the Search BQL query and take the &lt;tt&gt;CAEntryType.ReferenceID&lt;/tt&gt; field value from the result." lang="CS">
	/// [PXDefault(typeof(
	///     Search&lt;CAEntryType.referenceID,
	///         Where&lt;CAEntryType.entryTypeId,
	///             Equal&lt;Current&lt;AddTrxFilter.entryTypeID&gt;&gt;&gt;&gt;))]
	/// public virtual int? ReferenceID { get; set; }</code><code title="Example5" description="The attribute below will execute the &lt;tt&gt;Select&lt;/tt&gt; BQL query and take the &lt;tt&gt;VendorClass.AllowOverrideRate&lt;/tt&gt; field value from the result or will use &lt;tt&gt;false&lt;/tt&gt; as the default value if the BQL query returns an empty set." lang="CS">
	/// [PXDefault(
	///     false,
	///     typeof(
	///         Select&lt;VendorClass,
	///             Where&lt;VendorClass.vendorClassID,
	///                   Equal&lt;Current&lt;Vendor.vendorClassID&gt;&gt;&gt;&gt;),
	///     SourceField = typeof(VendorClass.allowOverrideRate))]
	/// public virtual Boolean? AllowOverrideRate { get; set; }</code><code title="Example6" description="The following example illustrates setting of a new default value to a field at run time." lang="CS">
	/// // The view declaration in a graph
	/// public PXSelect&lt;ARAdjust&gt; Adjustments;
	/// ...
	/// // The code executed in some graph method
	/// PXDefaultAttribute.SetDefault&lt;ARAdjust.adjdDocType&gt;(Adjustments.Cache, "CRM");</code><code title="Example7" description="The following code shows how to change the way the attribute checks the field value on saving of the changes to the database." lang="CS">
	/// protected virtual void ARPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
	/// {
	///     ARPayment doc = e.Row as ARPayment;
	///     ...
	///     PXDefaultAttribute.SetPersistingCheck&lt;ARPayment.depositAfter&gt;(
	///         cache, doc,
	///         isPayment &amp;&amp; (doc.DepositAsBatch == true)?
	///             PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
	///     ...
	/// }</code></example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXDefaultAttribute))]
	public class PXDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected bool _CacheGlobal;
		protected Type _ReferencedField;
		protected BqlCommand _NeutralSelect;
		protected object _Constant;
		protected Type _SourceType;
		protected string _SourceField;
		protected BqlCommand _Select;
		protected IBqlCreator _Formula;
		protected PXPersistingCheck _PersistingCheck = PXPersistingCheck.Null;
		protected Type _MapErrorTo;
		protected bool _SearchOnDefault = true;


		public virtual Type Formula
		{
			get { return _Formula?.GetType(); }
			set
			{
				if (typeof(IBqlWhere).IsAssignableFrom(value))
					value = BqlCommand.MakeGenericType(typeof(Switch<,>), typeof(Case<,>), value, typeof(True), typeof(False));
				_Formula = PXFormulaAttribute.InitFormula(value);
			}
		}

		/// <summary>Gets or sets the value that indicates whether the BQL query
		/// specified calculate the default value is executed or ignored. By
		/// default, is <tt>true</tt> (the BQL query is executed).</summary>
		public virtual bool SearchOnDefault
		{
			get
			{
				return this._SearchOnDefault;
			}
			set
			{
				this._SearchOnDefault = value;
			}
		}

		/// <summary>Gets or sets the <see cref="PXPersistingCheck">PXPersistingCheck</see> value
		/// that defines how to check the field value for null before saving a
		/// data record to the database. If a value doesn't pass a check, the
		/// attribute will throw the <tt>PXRowPersistingException</tt> exception.
		/// As a result, the save action will fail and the user will get an error
		/// message.</summary>
		/// <remarks>By default, the property equals <tt>PXPersistingCheck.Null</tt>, which disallows null values. Note that for fields that are displayed in the user interface,
		/// this setting also disallows blank values (containing only whitespce characters).</remarks>
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

		/// <summary>Gets or sets the value that redirects the error from the
		/// field the attribute is placed on (source field) to another field. If
		/// an error happens on the source field, the error message will be
		/// displayed over the input control of the other field. The property can
		/// be set to a type derived from <tt>IBqlField</tt>. The BQL query is set
		/// in a constructor.</summary>
		public virtual Type MapErrorTo
		{
			get
			{
				return _MapErrorTo;
			}
			set
			{
				_MapErrorTo = value;
			}
		}

		/// <summary>Gets or sets a constant value that will be used as the
		/// default value.</summary>
		public virtual object Constant
		{
			get { return this._Constant; }
			set { this._Constant = value; }
		}

		/// <summary>Gets or sets the field whose value will be taken from the BQL query result and used as the default value. (The BQL query is set in a constructor.)</summary>
		/// <value>The property can be set to a type derived from <tt>IBqlField</tt>.</value>
		/// <example>
		///   <code title="Example" description="" lang="CS">
		/// [PXDefault(
		///     typeof(
		///         Select&lt;VendorClass,
		///             Where&lt;VendorClass.vendorClassID,
		///                   Equal&lt;Current&lt;Vendor.vendorClassID&gt;&gt;&gt;&gt;),
		///     SourceField = typeof(VendorClass.allowOverrideRate))]
		/// public virtual Boolean? AllowOverrideRate { get; set; }</code>
		/// </example>
		public virtual Type SourceField
		{
			get
			{
				return null;
			}
			set
			{
				if (value != null && typeof(IBqlField).IsAssignableFrom(value) && value.IsNested)
				{
					_SourceType = BqlCommand.GetItemType(value);
					_SourceField = value.Name;
				}
			}
		}

		public virtual bool CacheGlobal
		{
			get
			{
				return this._CacheGlobal;
			}
			set
			{
				this._CacheGlobal = value;
			}
		}

		public bool CanDefault => _Constant != null || _SourceType != null || _Select != null;
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance that calculates the default value
		/// using the provided BQL query.</summary>
		/// <param name="sourceType">The BQL query that is used to calculate the
		/// default value. Accepts the types derived from: <tt>IBqlSearch</tt>,
		/// <tt>IBqlSelect</tt>, <tt>IBqlField</tt>, <tt>IBqlTable</tt>.</param>
		/// <example>
		/// The attribute below will take the default value from the <tt>ARPayment</tt>
		/// cache object and won't check the field value on saving of the changes to
		/// the database. In the second example, the attribute executes the <tt>Search</tt>
		/// BQL query and takes the <tt>CAEntryType.ReferenceID</tt> value from the result.
		/// <code>
		/// [PXDefault(typeof(ARPayment.adjDate),
		///            PersistingCheck = PXPersistingCheck.Nothing)]
		/// public virtual DateTime? TillDate { get; set; }
		/// </code>
		/// <code>
		/// [PXDefault(typeof(
		///     Search&lt;CAEntryType.referenceID,
		///         Where&lt;CAEntryType.entryTypeId,
		///             Equal&lt;Current&lt;AddTrxFilter.entryTypeID&gt;&gt;&gt;&gt;))]
		/// public virtual int? ReferenceID { get; set; }
		/// </code>
		/// </example>
		public PXDefaultAttribute(Type sourceType)
		{
			if (sourceType == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(sourceType))
			{
				_Select = BqlCommand.CreateInstance(sourceType);
				_SourceType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
				_SourceField = ((IBqlSearch)_Select).GetField().Name;

				Type[] refs = _Select.GetReferencedFields(false);
				if (refs.Length == 1 && _Select.GetSelectType().GetGenericTypeDefinition() == typeof(Select<,>))
				{
					_ReferencedField = refs[0];
					_NeutralSelect = BqlCommand.CreateInstance(typeof(Search<,>), _ReferencedField, typeof(Where<,>), _ReferencedField, typeof(Equal<>), typeof(Required<>), _ReferencedField);
				}
			}
			else if (typeof(IBqlSelect).IsAssignableFrom(sourceType))
			{
				_Select = BqlCommand.CreateInstance(sourceType);
				_SourceType = _Select.GetTables()[0];

				Type[] refs = _Select.GetReferencedFields(false);
				if (refs.Length == 1 && _Select.GetSelectType().GetGenericTypeDefinition() == typeof(Select<,>))
				{
					_ReferencedField = refs[0];
					_NeutralSelect = BqlCommand.CreateInstance(typeof(Search<,>), _ReferencedField, typeof(Where<,>), _ReferencedField, typeof(Equal<>), typeof(Required<>), _ReferencedField);
				}

			}
			else if (sourceType.IsNested && typeof(IBqlField).IsAssignableFrom(sourceType))
			{
				_SourceType = BqlCommand.GetItemType(sourceType);
				_SourceField = sourceType.Name;
			}
			else if (typeof(IBqlTable).IsAssignableFrom(sourceType))
			{
				_SourceType = sourceType;
			}
			else if (typeof(IConstant).IsAssignableFrom(sourceType))
			{
				_Constant = ((IConstant)Activator.CreateInstance(sourceType)).Value;
			}
			else if (typeof(IBqlCreator).IsAssignableFrom(sourceType))
			{
				Formula = sourceType;
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, sourceType);
			}
		}
		/// <summary>Initializes a new instance that defines the default value as
		/// a constant value.</summary>
		/// <param name="constant">Constant value that is used as the default
		/// value.</param>
		public PXDefaultAttribute(object constant)
		{
			_Constant = constant;
		}


		/// <summary>Initializes a new instance that calculates the default value
		/// using the provided BQL query and uses the constant value if the BQL
		/// query returns nothing. If the BQL query is of <tt>Select</tt> type,
		/// you should also explicitly set the <tt>SourceField</tt> property. If
		/// the BQL query is a DAC field, the attribute will take the value from
		/// the <tt>Current</tt> property of the cache object corresponding to the
		/// DAC.</summary>
		/// <param name="constant">Constant value that is used as the default
		/// value.</param>
		/// <param name="sourceType">The BQL query that is used to calculate the
		/// default value. Accepts the types derived from: <tt>IBqlSearch</tt>,
		/// <tt>IBqlSelect</tt>, <tt>IBqlField</tt>, <tt>IBqlTable</tt>.</param>
		/// <example>
		/// The attribute below will execute the <tt>Select</tt> BQL query and take the
		/// <tt>VendorClass.AllowOverrideRate</tt> field value from the result or will use
		/// <tt>false</tt> as the default value if the BQL query returns an empty set.
		/// <code>
		/// [PXDefault(
		///     false,
		///     typeof(
		///         Select&lt;VendorClass,
		///             Where&lt;VendorClass.vendorClassID,
		///                   Equal&lt;Current&lt;Vendor.vendorClassID&gt;&gt;&gt;&gt;),
		///     SourceField = typeof(VendorClass.allowOverrideRate))]
		/// public virtual Boolean? AllowOverrideRate { get; set; }
		/// </code>
		/// </example>
		public PXDefaultAttribute(object constant, Type sourceType)
			: this(sourceType)
		{
			_Constant = constant;
		}

		///// <param name="constant">Constant default value</param>
		///// <param name="sourceType">
		///// Bql query that is used to calculate default value.<br/>
		///// Accepts following types: IBqlSearch, IBqlSelect, IBqlField, IBqlTable
		///// </param>
		//public PXDefaultAttribute(object constant, Type sourceType, Type selectType)
		//    : this(sourceType, selectType)
		//{
		//    _Constant = constant;
		//}


		/// <summary>Initializes a new instance that does not provide the default
		/// value, but checks whether the field value is not null before saving to
		/// the database.</summary>
		/// <example>
		/// The attribute below only prevents the field from being null and does
		/// not set a default value.
		/// <code>
		/// [PXDefault]
		/// public virtual string BAccountAcctCD { get; set; }
		/// </code>
		/// </example>
		public PXDefaultAttribute()
		{
		}
		/// <summary>Converts the provided string to a specific type and
		/// Initializes a new instance that uses the conversion result as the
		/// default value.</summary>
		/// <param name="converter">The type code that specifies the type to
		/// covert the string to.</param>
		/// <param name="constant">The string representation of the constant used
		/// as the default value.</param>
		/// <example>
		/// The attribute below provides a string constants that is converted to
		/// the default value of the specific type.
		/// <code>
		/// [PXDefault(TypeCode.Decimal, "0.0")]
		/// public virtual Decimal? AdjDiscAmt { get; set; }
		/// </code>
		/// </example>
		public PXDefaultAttribute(TypeCode converter, string constant)
		{
			switch (converter)
			{
				case TypeCode.Boolean:
					_Constant = Boolean.Parse(constant);
					break;
				case TypeCode.Byte:
					_Constant = Byte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Char:
					_Constant = Char.Parse(constant);
					break;
				case TypeCode.DateTime:
					_Constant = DateTime.Parse(constant, CultureInfo.InvariantCulture, DateTimeStyles.None);
					break;
				case TypeCode.Decimal:
					_Constant = Decimal.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Double:
					_Constant = Double.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int16:
					_Constant = Int16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int32:
					_Constant = Int32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int64:
					_Constant = Int64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.SByte:
					_Constant = SByte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Single:
					_Constant = Single.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.String:
					_Constant = (String)constant;
					break;
				case TypeCode.UInt16:
					_Constant = UInt16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt32:
					_Constant = UInt32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt64:
					_Constant = UInt64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
			}
		}
		/// <summary>Initializes a new instance that determines the default value
		/// using either the provided BQL query or the constant if the BQL query
		/// returns nothing.</summary>
		/// <param name="converter">The type code that specifies the type to
		/// convert the string constant to.</param>
		/// <param name="constant">The string representation of the constant used
		/// as the default value if the BQL query returns nothing.</param>
		/// <param name="sourceType">The BQL command that is used to calculate the
		/// default value. Accepts the types derived from: <tt>IBqlSearch</tt>,
		/// <tt>IBqlSelect</tt>, <tt>IBqlField</tt>, <tt>IBqlTable</tt>.</param>
		public PXDefaultAttribute(TypeCode converter, string constant, Type sourceType)
			: this(sourceType)
		{
			switch (converter)
			{
				case TypeCode.Boolean:
					_Constant = Boolean.Parse(constant);
					break;
				case TypeCode.Byte:
					_Constant = Byte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Char:
					_Constant = Char.Parse(constant);
					break;
				case TypeCode.DateTime:
					_Constant = DateTime.Parse(constant, CultureInfo.InvariantCulture, DateTimeStyles.None);
					break;
				case TypeCode.Decimal:
					_Constant = Decimal.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Double:
					_Constant = Double.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int16:
					_Constant = Int16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int32:
					_Constant = Int32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int64:
					_Constant = Int64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.SByte:
					_Constant = SByte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Single:
					_Constant = Single.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.String:
					_Constant = (String)constant;
					break;
				case TypeCode.UInt16:
					_Constant = UInt16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt32:
					_Constant = UInt32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt64:
					_Constant = UInt64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
			}
		}
		#endregion

		#region Runtime
		/// <summary>Sets the new default value of the field with the specified
		/// name for a particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDefault</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="field">The name of the field to set the default value
		/// to.</param>
		/// <param name="def">The new default value.</param>
		public static void SetDefault(PXCache cache, object data, string field, object def)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		/// <summary>Sets the new default value of the field with the specified
		/// name for all data records in the cache.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDefault</tt> type.</param>
		/// <param name="field">The name of the field to set the default value
		/// to.</param>
		/// <param name="def">The new default value.</param>
		public static void SetDefault(PXCache cache, string field, object def)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		/// <summary>Sets the new default value of the specified field for a
		/// particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDefault</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="def">The new default value.</param>
		public static void SetDefault<Field>(PXCache cache, object data, object def)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		/// <summary>Sets the <tt>PersistingCheck</tt> property for the specifed
		/// field in a particular data record.</summary>
		/// <typeparam name="Field">The field whose attribute is affected.</typeparam>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDefault</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="check">The value that is set to the property.</param>
		/// <example>
		/// The code below changes the way the attribute checks the field value
		/// on saving of the changes to the database.
		/// <code>
		/// protected virtual void ARPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		/// {
		///     ARPayment doc = e.Row as ARPayment;
		///     ...
		///     PXDefaultAttribute.SetPersistingCheck&lt;ARPayment.depositAfter&gt;(
		///         cache, doc,
		///         isPayment &amp;&amp; (doc.DepositAsBatch == true)?
		///             PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		///     ...
		/// }
		/// </code>
		/// </example>
		public static void SetPersistingCheck<Field>(PXCache cache, object data, PXPersistingCheck check)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._PersistingCheck = check;
				}
			}
		}
		/// <summary>Sets the <tt>PersistingCheck</tt> property for the field with
		/// the specified name in a particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDefault</tt> type.</param>
		/// <param name="field">The field name.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="check">The value that is set to the property.</param>
		public static void SetPersistingCheck(PXCache cache, string field, object data, PXPersistingCheck check)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._PersistingCheck = check;
				}
			}
		}
		/// <summary>Sets the new default value of the specified field for all
		/// data records in the cache.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDefault</tt> type.</param>
		/// <param name="def">The new default value.</param>
		/// <example>
		/// The code below sets a new default value to a field at run time.
		/// <code>
		/// // The view declaration in a graph
		/// public PXSelect&lt;ARAdjust&gt; Adjustments;
		/// ...
		/// // The code executed in some graph method
		/// PXDefaultAttribute.SetDefault&lt;ARAdjust.adjdDocType&gt;(Adjustments.Cache, "CRM");
		/// </code>
		/// </example>
		public static void SetDefault<Field>(PXCache cache, object def)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		/// 
		public static object Select(PXCache cache, PXDefaultAttribute attr, object data)
		{
			Tuple<object, PXCache> ret = SelectRow(cache, attr, data);
			if (ret == null)
			{
				return null;
			}
			return ret.Item2.GetValue(ret.Item1, attr._SourceField ?? attr._FieldName);
		}
		protected static Tuple<object, PXCache> SelectRow(PXCache cache, PXDefaultAttribute attr, object data)
		{
			PXView view;
			object row = null;
			object key = null;

			if (attr._CacheGlobal && attr._ReferencedField != null && (key = cache.GetValue(data, attr._ReferencedField.Name)) != null)
			{
				Type type = BqlCommand.GetItemType(attr._ReferencedField);
				var dict = PXSelectorAttribute.GlobalDictionary.GetOrCreate(type, cache.Graph.Caches[type].BqlTable, 1);
				lock ((dict).SyncRoot)
				{
					PXSelectorAttribute.GlobalDictionary.CacheValue cacheValue;
					if (dict.TryGetValue(key, out cacheValue) && !cacheValue.IsDeleted && !(cacheValue.Item is IDictionary))
					{
						row = cacheValue.Item;
					}
				}

				view = cache.Graph.TypedViews.GetView(attr._NeutralSelect, false);
				if (row == null)
				{
					List<object> records = view.SelectMulti(new object[] { key });
					if (records.Count == 0)
					{
						return null;
					}

					if (records[0] is PXResult)
					{
						row = ((PXResult)records[0])[0];
					}
					else
					{
						row = records[0];
					}

					if (view.Cache.GetStatus(row) == PXEntryStatus.Notchanged && !PXDatabase.ReadDeleted && view.Cache.Keys.Count <= 1)
					{
						PXSelectorAttribute.CheckIntegrityAndPutGlobal(dict, view.Cache, attr._ReferencedField.Name, row, key, false);
					}
				}
				return new Tuple<object, PXCache>(row, view.Cache);
			}

			view = cache.Graph.TypedViews.GetView(attr._Select, false);

			row = view.SelectSingleBound(new object[] { data });

			if (row == null)
			{
				return null;
			}
			if (row is PXResult)
			{
				row = ((PXResult)row)[attr._SourceType];
			}
			return new Tuple<object, PXCache>(row, cache.Graph.Caches[attr._SourceType]);
		}

		public static object Select(PXGraph graph, BqlCommand Select, Type sourceType, string sourceField, object row)
		{
			PXView view = graph.TypedViews.GetView(Select, false);

			object item = view.SelectSingleBound(new object[] { row });
	
			if (item == null)
			{
				return null;
			}
			if (item != null && item is PXResult)
			{
				item = ((PXResult)item)[sourceType];
			}

			return graph.Caches[sourceType].GetValue(item, sourceField);
		}

		/// <summary>
		/// Provides the default value
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to set the NewValue</param>
		/// <exclude/>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.NewValue == null)
			{
				if (_Select != null && !_SearchOnDefault && e.Row == null)
				{
				}
				else if (_Select != null)
				{
					List<BqlCommand> cmds = new List<BqlCommand>();
					if (_Select is IBqlCoalesce)
					{
						((IBqlCoalesce)_Select).GetCommands(cmds);

						foreach (BqlCommand Select in cmds)
						{
							Type SourceType = BqlCommand.GetItemType(((IBqlSearch)Select).GetField());
							string SourceField = ((IBqlSearch)Select).GetField().Name;

							e.NewValue = PXDefaultAttribute.Select(sender.Graph, Select, SourceType, SourceField, e.Row);
							if (e.NewValue != null)
							{
								e.Cancel = true;
								return;
							}
						}
					}
					else
					{
						e.NewValue = PXDefaultAttribute.Select(sender, this, e.Row);
						if (e.NewValue != null)
						{
							e.Cancel = true;
							return;
						}
					}
				}
				else if (_SourceType != null)
				{
					PXCache cache = sender.Graph.Caches[_SourceType];
					if (cache.Current != null)
					{
						e.NewValue = cache.GetValue(cache.Current, _SourceField == null ? _FieldName : _SourceField);
						if (e.NewValue != null)
						{
							e.Cancel = true;
							return;
						}
					}
				}
				else if (_Formula != null)
				{
					bool? result = null;
					object value = null;
					BqlFormula.Verify(sender, e.Row, _Formula, ref result, ref value);
					e.NewValue = value;
					if (e.NewValue != null)
					{
						e.Cancel = true;
						return;
					}
				}

				if (_Constant != null)
				{
					e.NewValue = _Constant;
				}
			}
		}
		/// <summary>
		/// Check if the value was set before saving the record to the database
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to retrive the value from the Row</param>
		/// <exclude/>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object val;
			if (PersistingCheck != PXPersistingCheck.Nothing &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
				((val = sender.GetValue(e.Row, _FieldOrdinal)) == null ||
				PersistingCheck == PXPersistingCheck.NullOrBlank && val is string && ((string)val).Trim() == String.Empty))
			{
				if (_MapErrorTo == null)
				{
					if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
					{
						throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
					}
				}
				else
				{
					string name = _MapErrorTo.Name;
					name = char.ToUpper(name[0]) + name.Substring(1);
					val = sender.GetValueExt(e.Row, name);
					if (val is PXFieldState)
					{
						val = ((PXFieldState)val).Value;
					}
					if (sender.RaiseExceptionHandling(name, e.Row, val, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.IncorrectValueResultedEmptyField, name, _FieldName))))
					{
						throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
					}
				}
			}
		}
		/// <summary>
		/// Provides the default value as a part of the field state
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to set the ReturnState</param>
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, _Constant == null, _PersistingCheck == PXPersistingCheck.Nothing ? (int?)null : 1, null, null, _Constant, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		#endregion

		//#region Initialization
		//public override void CacheAttached(PXCache sender)
		//{
		//    base.CacheAttached(sender);
		//    if (_Features != 0L && _PersistingCheck != PXPersistingCheck.Nothing)
		//    {
		//        sender.GetFieldType(_FieldOrdinal);
		//    }
		//}
		//#endregion
	}
	#endregion

	#region PXUnboundDefaultAttribute
	/// <summary>Sets the default value to an unbound DAC field. The value is
	/// assigned to the field when the data record is retrieved from the
	/// database.</summary>
	/// <remarks>This attributes is similar to the <tt>PXDefault</tt> attribute,
	/// but, unlike the <tt>PXDefault</tt> attribute, it assigns the provided
	/// default value to the field when a data record is retrieved from the
	/// database (on the <tt>RowSelecting</tt> event). The <tt>PXDefault</tt>
	/// attribute assigns the default value to the field when a data record is
	/// inserted to the cache object.</remarks>
	/// <example>
	/// The examples below show the definitions of two DAC fields.
	/// <code>
	/// [PXDecimal(4)]
	/// [PXUnboundDefault(TypeCode.Decimal, "0.0")]
	/// public virtual Decimal? DocBal { get; set; }
	/// </code>
	/// <code>
	/// [PXBool]
	/// [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
	/// [PXUIField(DisplayName = "Included")]
	/// public virtual bool? Included { get; set; }
	/// </code>
	/// </example>
	public class PXUnboundDefaultAttribute : PXDefaultAttribute, IPXRowSelectingSubscriber
	{
		#region Ctor
		/// <summary>Initializes a new instance that calculates the default value
		/// using the provided BQL query.</summary>
		/// <param name="sourceType">The BQL query that is used to calculate the
		/// default value. Accepts the types derived from: <tt>IBqlSearch</tt>,
		/// <tt>IBqlSelect</tt>, <tt>IBqlField</tt>, <tt>IBqlTable</tt>.</param>
		public PXUnboundDefaultAttribute(Type sourceType)
			: base(sourceType)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>Initializes a new instance that defines the default value as
		/// a constant value.</summary>
		/// <param name="constant">Constant value that is used as the default
		/// value.</param>
		/// <example>
		/// <code>
		/// [PXBool]
		/// [PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		/// [PXUIField(DisplayName = "Included")]
		/// public virtual bool? Included { get; set; }
		/// </code>
		/// </example>
		public PXUnboundDefaultAttribute(object constant)
			: base(constant)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>Initializes a new instance that calculates the default value
		/// using the provided BQL query and uses the constant value if the BQL
		/// query returns nothing. If the BQL query is of <tt>Select</tt> type,
		/// you should also explicitly set the <tt>SourceField</tt> property. If
		/// the BQL query is a DAC field, the attribute will take the value from
		/// the <tt>Current</tt> property of the cache object corresponding to the
		/// DAC.</summary>
		/// <param name="constant">Constant value that is used as the default
		/// value.</param>
		/// <param name="sourceType">The BQL query that is used to calculate the
		/// default value. Accepts the types derived from: <tt>IBqlSearch</tt>,
		/// <tt>IBqlSelect</tt>, <tt>IBqlField</tt>, <tt>IBqlTable</tt>.</param>
		public PXUnboundDefaultAttribute(object constant, Type sourceType)
			: base(constant, sourceType)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>Initializes a new instance that does not provide the default
		/// value, but checks whether the field value is not null before saving to
		/// the database.</summary>
		public PXUnboundDefaultAttribute()
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>Converts the provided string to a specific type and
		/// initializes a new instance that uses the conversion result as the
		/// default value.</summary>
		/// <param name="converter">The type code that specifies the type to
		/// covert the string to..</param>
		/// <param name="constant">The string representation of the constant used
		/// as the default value.</param>
		/// <example>
		/// <code>
		/// [PXDecimal(4)]
		/// [PXUnboundDefault(TypeCode.Decimal, "0.0")]
		/// public virtual Decimal? DocBal { get; set; }
		/// </code>
		/// </example>
		public PXUnboundDefaultAttribute(TypeCode converter, string constant)
			: base(converter, constant)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>Initializes a new instance that determines the default value
		/// using either the provided BQL query or the constant if the BQL query
		/// returns nothing.</summary>
		/// <param name="converter">The type code that specifies the type to
		/// convert the string constant to.</param>
		/// <param name="constant">The string representation of the constant used
		/// as the default value if the BQL query returns nothing.</param>
		/// <param name="sourceType">The BQL command that is used to calculate the
		/// default value. Accepts the types derived from: <tt>IBqlSearch</tt>,
		/// <tt>IBqlSelect</tt>, <tt>IBqlField</tt>, <tt>IBqlTable</tt>.</param>
		public PXUnboundDefaultAttribute(TypeCode converter, string constant, Type sourceType)
			: base(converter, constant, sourceType)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		#endregion

		#region Implementation
		/// <summary>
		/// Provides the default value
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments with a row reading</param>
		/// <exclude/>
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				object val = null;
				using (PXConnectionScope ts = new PXConnectionScope())
				{
					bool res = sender.RaiseFieldDefaulting(_FieldName, e.Row, out val);
					if (res)
					{
						sender.RaiseFieldUpdating(_FieldName, e.Row, ref val);
					}
				}
				sender.SetValue(e.Row, _FieldOrdinal, val);
			}
		}
		#endregion
			}
		#endregion

	#region PXExtensionAttribute
	/// <summary>Not used.</summary>
	public class PXExtensionAttribute : PXSelectorAttribute
	{
		#region Ctor
		/// <summary>
		/// Creates an extension
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXExtensionAttribute(Type type)
			: base(type)
		{
		}
		#endregion
	}
	#endregion

	#region PXVirtualSelectorAttribute
	/// <summary>Suppress GUI selector, used in formula.</summary>
	public class PXVirtualSelectorAttribute : PXSelectorAttribute
	{
		#region Ctor
		/// <summary>
		/// Creates an virtual selector
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXVirtualSelectorAttribute(Type type)
			: base(type)
		{
			ValidateValue = false;
		}
		#endregion

		public override bool ExcludeFromReferenceGeneratingProcess { get; set; } = true;

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			PXFieldState state = e.ReturnState as PXFieldState;
			if (state != null && state.ViewName != _ViewName)
				state.ViewName = null;
		}
	}
	#endregion

	#region PXParentAttribute
	/// <summary>Creates a reference to the parent record, establishing a parent-child relationship between two tables.</summary>
	/// <remarks>
	/// 	<para>You can place the attribute on any field of the child DAC. The
	/// primary goal of the attribute is to perform cascade deletion of the
	/// child data records once a parent data record is deleted.</para>
	/// 	<para>The parent data record is defined by a BQL query of
	/// <tt>Select&lt;&gt;</tt> type. Typically, the query includes a
	/// <tt>Where</tt> clause that adds conditions for the parent's key fields
	/// to equal child's key fields. In this case, the values of child data
	/// record key fields are specified using the <tt>Current</tt> parameter.
	/// The business logic controller that provides the interface for working
	/// with these parent and child data records should define a view
	/// selecting parent data records and a view selecting child data records.
	/// These views will againg be connected using the <tt>Current</tt>
	/// parameter.</para>
	/// 	<para>You can use the static methods to retrieve a particular parent
	/// data record or child data records, or get and set some attribute
	/// parameters.</para>
	/// 	<para>Once the <tt>PXParent</tt> attribute is added to some DAC field,
	/// you can use the <see cref="PXFormulaAttribute">PXFormula</see>
	/// attribute to define set calculations for parent data record fields
	/// from child data record fields.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="The attribute below specifies a query for selecting the parent Document data record for a given child DocTransaction data record." lang="CS">
	/// [PXParent(typeof(
	///     Select&lt;Document,
	///         Where&lt;Document.docNbr, Equal&lt;Current&lt;DocTransaction.docNbr&gt;&gt;,
	///             And&lt;Document.docType, Equal&lt;Current&lt;DocTransaction.docType&gt;&gt;&gt;&gt;&gt;))]
	/// public virtual string DocNbr { get; set; }</code>
	/// 	<code title="Example2" description="Another example is given below." groupname="Example" lang="CS">
	/// [PXParent(typeof(
	///     Select&lt;ARTran,
	///         Where&lt;ARTran.tranType, Equal&lt;Current&lt;ARFinChargeTran.tranType&gt;&gt;,
	///             And&lt;ARTran.refNbr, Equal&lt;Current&lt;ARFinChargeTran.refNbr&gt;&gt;,
	///             And&lt;ARTran.lineNbr, Equal&lt;Current&lt;ARFinChargeTran.lineNbr&gt;&gt;&gt;&gt;&gt;&gt;))]
	/// public virtual short? LineNbr { get; set; }</code>
	/// 	<code title="Example3" description="The following code obtains the parent data record at run time." groupname="Example2" lang="CS">
	/// CR.Location child = (CR.Location)e.Row;
	/// BAccount parent =
	///     (BAccount)PXParentAttribute.SelectParent(sender, child, typeof(BAccount));</code>
	/// 	<code title="Example4" description="The following example sets the parent data record at run time." groupname="Example3" lang="CS">
	/// // Views definitions in a graph
	/// public PXSelect&lt;INRegister&gt; inregister;
	/// public PXSelect&lt;INTran&gt; intranselect;
	/// ...
	/// // Code executed in some graph method
	/// INTran tran = (INTran)res;
	/// PXParentAttribute.SetParent(
	///     intranselect.Cache, tran, typeof(INRegister), inregister.Current);</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
	public class PXParentAttribute : PXForeignReferenceAttribute, IPXRowInsertedSubscriber, IPXRowPersistedSubscriber
	{
		#region State
		protected Type _ChildType;
		protected Type _ParentType;
		protected BqlCommand _SelectParent;
		protected BqlCommand _SelectChildren;
		protected bool _UseCurrent;
		protected bool _LeaveChildren;
		protected bool _ParentCreate;

		/// <summary>Gets or sets the value that permits or forbids creation of
		/// the parent through the <see
		/// cref="PXParentAttribute.CreateParent(PXCache,object,Type)">CreateParent(PXCache, object,
		/// Type)</see> method. In particular, the <tt>PXFormula</tt> attribute
		/// tries to create a parent data record if it doesn't exist, by invoking
		/// this method. By default, the property equals <tt>false</tt>.</summary>
		public virtual bool ParentCreate
		{
			get
			{
				return _ParentCreate;
			}
			set
			{
				_ParentCreate = value;
			}
		}

		/// <summary>Gets or sets the value that indicates whether the child data
		/// records are left or deleted on parent data record deletion. By
		/// default, the property equals <tt>false</tt>, which means that child
		/// data records are deleted.</summary>
		public virtual bool LeaveChildren
		{
			get
			{
				return _LeaveChildren;
			}
			set
			{
				_LeaveChildren = value;
			}
		}

		/// <summary>Gets the DAC type of the parent data record. The type is
		/// determined in the constructor as the first table referenced in the
		/// <tt>Select</tt> query.</summary>
		public virtual Type ParentType
		{
			get
			{
				return this._ParentType;
			}
		}

		private protected override bool ForceNoAction => LeaveChildren;

		protected ParentCollection _Currents = null;
		protected ParentCollection _Pendings = null;

		/// <summary>Returns the value of the <tt>ParentCreate</tt> property from
		/// the attribute instance that references the provided parent type or a
		/// type derived from it.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="ParentType">The DAC type of the parent data
		/// record.</param>
		public static bool GetParentCreate(PXCache cache, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
						break;
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				return ((PXParentAttribute)parents[0])._ParentCreate;
			}
			return false;
		}

		/// <summary>Creates the parent for the provided child data record for the
		/// attribute instance that references the provided parent type or a type
		/// derived from it. Does nothing if <tt>ParentCreate</tt> equals
		/// <tt>false</tt> in this attribute instance. If the parent is created,
		/// it is inserted into the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="row">The child data record for which the parent is
		/// created.</param>
		/// <param name="ParentType">The DAC type of the parent data
		/// record.</param>
		public static object CreateParent(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXParentAttribute attr in cache.GetAttributesReadonly(null).OfType<PXParentAttribute>())
			{
				if (attr._ParentType == ParentType)
				{
					parents.Insert(0, attr);
					break;
				}
				else if (ParentType.IsSubclassOf(attr._ParentType))
				{
					parents.Add(attr);
				}
			}

			if (parents.Count > 0 && ((PXParentAttribute)parents[0])._ParentCreate == true)
			{
				object parent = CreateParentInt(cache, row, ParentType, (PXParentAttribute)parents[0]);

				if (parent != null)
				{
					return cache.Graph.Caches[ParentType].Insert(parent);
				}
			}

			return null;
		}

		/// <summary>Locates the parent for the provided child data record for the
		/// attribute instance that references the provided parent type or a type
		/// derived from it.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="row">The child data record for which the parent is
		/// created.</param>
		/// <param name="ParentType">The DAC type of the parent data
		/// record.</param>
		public static object LocateParent(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXParentAttribute attr in cache.GetAttributesReadonly(null).OfType<PXParentAttribute>())
			{
				if (attr._ParentType == ParentType)
					{
						parents.Insert(0, attr);
						break;
					}
				else if (ParentType.IsSubclassOf(attr._ParentType))
					{
						parents.Add(attr);
					}
				}

			if (parents.Count > 0)
			{
				object parent = CreateParentInt(cache, row, ParentType, (PXParentAttribute)parents[0]);

				if (parent != null)
				{
					return cache.Graph.Caches[ParentType].Locate(parent);
				}
			}

			return null;
		}

		internal static object CreateParentInt(PXCache cache, object row, Type ParentType, PXParentAttribute attr)
			{
				object parent;

				PXView parentView = attr.GetParentSelect(cache);
				BqlCommand selectParent = parentView.BqlSelect;

				IBqlParameter[] pars = selectParent.GetParameters();
				Type[] refs = selectParent.GetReferencedFields(false);

				PXCache parentcache = cache.Graph.Caches[ParentType];
				parent = parentcache.CreateInstance();

				object val;

				for (int i = 0; i < refs.Length; i++)
				{
					Type partype = pars[i].GetReferencedType();
					if ((val = cache.GetValue(row, partype.Name)) == null)
					{
					return null;
					}
					parentcache.SetValue(parent, refs[i].Name, val);
				}

				List<object> vals = new List<object>();

				for (int i = 0; i < pars.Length; i++)
				{
					Type partype = pars[i].GetReferencedType();

					if ((val = cache.GetValue(row, partype.Name)) == null)
					{
					return null;
					}
					vals.Add(val);
				}

				object value = null;
				bool? ret = null;

				selectParent.Verify(parentcache, parent, vals, ref ret, ref value);

				if (ret == true)
				{
				return parent;
			}

			return null;
		}

		/// <summary>Enables or disables cascade deletion of child data records
		/// for the attribute instance in a paricular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isLeaveChildren">The new value for the
		/// <tt>LeaveChildren</tt> property. If <tt>true</tt>, enables cascade
		/// deletion. Otherwise, disables it.</param>
		public static void SetLeaveChildren<Field>(PXCache cache, object data, bool isLeaveChildren)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXParentAttribute)
				{
					((PXParentAttribute)attr).LeaveChildren = isLeaveChildren;
					if (isLeaveChildren == false)
					{
						cache.Graph.RowDeleted.AddHandler(((PXParentAttribute)attr)._ParentType, ((PXParentAttribute)attr).RowDeleted);
					}
					else
					{
						cache.Graph.RowDeleted.RemoveHandler(((PXParentAttribute)attr)._ParentType, ((PXParentAttribute)attr).RowDeleted);
					}
				}
			}
		}

		/// <summary>Enables or disables cascade deletion of child data records
		/// for the attribute instance.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="isLeaveChildren">The new value for the
		/// <tt>LeaveChildren</tt> property. If <tt>true</tt>, enables cascade
		/// deletion. Otherwise, disables it.</param>
		/// <param name="parentType">The parent type to search for the attribues.</param>
		public static void SetLeaveChildren(PXCache cache, bool isLeaveChildren, Type parentType)
		{
			foreach (PXParentAttribute attr in cache.GetAttributes(null)
				.OfType<PXParentAttribute>()
				.Where(a => a.ParentType == parentType))
			{
				cache.SetAltered(attr.FieldName, true);
				attr.LeaveChildren = isLeaveChildren;
				if (isLeaveChildren == false)
				{
					cache.Graph.RowDeleted.AddHandler(attr._ParentType, attr.RowDeleted);
				}
				else
				{
					cache.Graph.RowDeleted.RemoveHandler(attr._ParentType, attr.RowDeleted);
				}
			}
		}

		/// <exclude/>
		public virtual PXView GetParentSelect(PXCache sender)
		{
			return sender.Graph.TypedViews.GetView(_SelectParent, false);
		}

		/// <exclude/>
		public virtual PXView GetChildrenSelect(PXCache sender)
		{
			if (_SelectChildren == null)
			{
				_initialize(sender);
			}
			return sender.Graph.TypedViews.GetView(_SelectChildren, false);
		}

		/// <summary>Gets or sets the value that indicates at run time whether to
		/// take the parent data record from the <tt>Current</tt> property or
		/// retrieve it from the database. In both cases the attribute uses the
		/// view corresponding to the <tt>Select</tt> query provided in the
		/// constructor.</summary>
		public virtual bool UseCurrent
		{
			get
			{
				return _UseCurrent;
			}
			set
			{
				_UseCurrent = value;
			}
		}
		#endregion

		#region Ctor
		public PXParentAttribute(Type fieldsRelation, Type additionalCondition)
			: base(fieldsRelation, ReferenceOrigin.ParentAttribute, ReferenceBehavior.Cascade, true)
		{
			Type selectParent = ToParentSelect(fieldsRelation);

			if (!typeof(IBqlSelect).IsAssignableFrom(selectParent) || selectParent.GetGenericArguments().Length == 0)
			{
				throw new PXArgumentException("selectParent", ErrorMessages.PXParentAllowsOnlyBQLSelections);
			}

			if (additionalCondition == null)
				throw new PXArgumentException(nameof(additionalCondition), ErrorMessages.ArgumentNullException);
			if (!typeof(IBqlWhere).IsAssignableFrom(additionalCondition) || additionalCondition.GetGenericArguments().Length == 0)
				throw new PXArgumentException(nameof(additionalCondition), SM.Messages.AssignedTypeMustImplementInterface, nameof(IBqlWhere));

			_SelectParent = BqlCommand.CreateInstance(selectParent).WhereAnd(additionalCondition);
			_ParentType = _SelectParent.GetTables()[0];
		}

		/// <summary>
		/// Initializes a new instance that defines the parent data record using the
		/// provided BQL query. To provide parameters to the BQL query, use <tt>Current</tt>
		/// to pass the values from the child data record that is <tt>Current</tt> for the
		/// cache object.
		/// </summary>
		/// <param name="selectParent">The BQL query that selects the parent record. Should be
		/// based on a class derived from <tt>IBqlSelect</tt>, such as <tt>Select&lt;&gt;</tt>.</param>
		/// <example>
		/// <code>
		/// [PXParent(typeof(
		///     Select&lt;ARTran,
		///         Where&lt;ARTran.tranType, Equal&lt;Current&lt;ARFinChargeTran.tranType&gt;&gt;,
		///             And&lt;ARTran.refNbr, Equal&lt;Current&lt;ARFinChargeTran.refNbr&gt;&gt;,
		///             And&lt;ARTran.lineNbr, Equal&lt;Current&lt;ARFinChargeTran.lineNbr&gt;&gt;&gt;&gt;&gt;&gt;))]
		/// public virtual short? LineNbr { get; set; }
		/// </code>
		/// </example>
		public PXParentAttribute(Type selectParent)
			:base(selectParent, ReferenceOrigin.ParentAttribute, ReferenceBehavior.Cascade, true)
		{
			if (selectParent == null)
			{
				throw new PXArgumentException(nameof(selectParent), ErrorMessages.ArgumentNullException);
			}

			selectParent = ToParentSelect(selectParent);

			if (!typeof(IBqlSelect).IsAssignableFrom(selectParent) || selectParent.GetGenericArguments().Length == 0)
			{
				throw new PXArgumentException(nameof(selectParent), ErrorMessages.PXParentAllowsOnlyBQLSelections);
			}
			_SelectParent = BqlCommand.CreateInstance(selectParent);
			_ParentType = _SelectParent.GetTables()[0];
		}

		private static Type ToParentSelect(Type selectOrFieldsRelation)
		{
			if (typeof(IBqlSelect).IsAssignableFrom(selectOrFieldsRelation))
				return selectOrFieldsRelation;

			if (FieldRelationArray.IsTypeArrayOrElement(selectOrFieldsRelation))
			{
				var fieldRelations = FieldRelationArray.CheckAndExtractInstances(FieldRelationArray.EmptyOrSingleOrSelf(selectOrFieldsRelation));

				var childFields = fieldRelations.Select(r => r.FieldOfChildTable).ToArray();
				var parentFields = fieldRelations.Select(r => r.FieldOfParentTable).ToArray();

				var reference = new Reference(
					new TableWithKeys(parentFields.First().DeclaringType, parentFields),
					new TableWithKeys(childFields.First().DeclaringType, childFields),
					ReferenceOrigin.ParentAttribute,
					ReferenceBehavior.Cascade,
					typeof(Current<>)
					);

				return reference.ParentSelect;
			}

			throw new PXArgumentException("selectParent");
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.NewRow != null)
			{
				object parent;
				if (_Pendings.TryGetValue(e.Row, false, out parent) || _Pendings.TryGetValue(e.NewRow, false, out parent))
				{
					_Currents[e.Row] = parent;
				} 

				_Pendings.Remove(e.Row);
			} 
		}

		/// <exclude/>
		public virtual void SelfRowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_Currents.ContainsKey(e.Row))
				_Currents.Remove(e.Row);
		}

		/// <exclude/>
		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_SelectChildren == null)
			{
				_initialize(sender);
			}
			foreach (object item in sender.Graph.TypedViews.GetView(_SelectChildren, false).SelectMultiBound(new object[] { e.Row }))
			{
				sender.Graph.Caches[_ChildType].Delete(item);
			}
			sender.Graph.TypedViews.GetView(_SelectChildren, false).Clear();
		}

		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				_Pendings.Clear();
				_Currents.Clear();
			}
		}

		/// <summary>Returns the child data records that have the same parent as
		/// the provided child data record. Returns an array of zero length if
		/// fails to retrieve the parent. Uses the first attribute instance found
		/// in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="row">The child data record for which the data records
		/// having the same parent are retrieved.</param>
		public static object[] SelectSiblings(PXCache cache, object row)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					parents.Add(attr);
				}
			}

			if (parents.Count > 1)
			{
				throw new PXException(ErrorMessages.UnconditionalUniqueParent, cache.GetItemType().ToString());
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];
				PXView parentview;

				object parent = SelectParent(cache, row, null, null, attr);

				if (parent != null || (parentview = attr.GetParentSelect(cache)).Cache.Current != null && parentview.Cache.GetStatus(parentview.Cache.Current) != PXEntryStatus.Deleted && parentview.Cache.GetStatus(parentview.Cache.Current) != PXEntryStatus.InsertedDeleted)
				{
					PXView view = attr.GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { parent }).ToArray();
				}
			}
			return new object[0];
		}

		/// <summary>Returns the child data records that have the same parent as
		/// the provided child data record. Returns an array of zero length if
		/// fails to retrieve the parent. Uses the first attribute instance that
		/// references the provided parent type or a type derived from
		/// it.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="row">The child data record for which the data records
		/// having the same parent are retrieved.</param>
		/// <param name="ParentType">The DAC type of the parent data
		/// record.</param>
		public static object[] SelectSiblings(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (attr is PXParentAttribute)
					{
						if (((PXParentAttribute)attr)._ParentType == ParentType)
						{
							parents.Insert(0, attr);
						}
						else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
						{
							parents.Add(attr);
						}
					}
				}
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];
				PXView parentview;

				object parent = SelectParent(cache, row, null, ParentType, attr);

				if (parent != null || (parentview = attr.GetParentSelect(cache)).Cache.Current != null && parentview.Cache.GetStatus(parentview.Cache.Current) != PXEntryStatus.Deleted && parentview.Cache.GetStatus(parentview.Cache.Current) != PXEntryStatus.InsertedDeleted)
				{
					PXView view = attr.GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { parent }).ToArray();
				}
			}

			return new object[0];
		}

		/// <summary>Returns child data records of the specified parent record.</summary>
		/// <param name="ParentType">The DAC that is used to access the parent record.</param>
		/// <param name="cache">The cache object.</param>
		/// <param name="parent">A parent data record.</param>
		/// <example>
		/// 	<code title="Example2" description="In the example below you iterate through child data records whose parent type is SOLine. Then you validate required conditions and call the SetValueExt method to set the value for the SOLine.shipComplete field." groupname="Example" lang="CS">
		/// foreach (SOLineSplit split in PXParentAttribute.SelectChildren(splits.Cache, e.Row, typeof(SOLine)))
		/// {
		///     if (split.ShipmentNbr != null || split.ShippedQty &gt; 0m)
		///         sender.SetValueExt&lt;SOLine.shipComplete&gt;(e.Row, SOShipComplete.BackOrderAllowed);
		/// }</code>
		/// </example>
		public static object[] SelectChildren(PXCache cache, object parent, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
						if (((PXParentAttribute)attr)._ParentType == ParentType)
						{
							parents.Insert(0, attr);
						}
						else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
						{
							parents.Add(attr);
						}
					}
				}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];
				
				PXView view = attr.GetChildrenSelect(cache);
				return view.SelectMultiBound(new object[] { parent }).ToArray();
			}

			return new object[0];
		}

		/// <summary>Returns the parent type of the first attribute instance found
		/// in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		public static Type GetParentType(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					return ((PXParentAttribute)attr)._ParentType;
				}
			}
			return null;
		}

		/// <summary>Sets the provided data record of parent type as the parent of
		/// the child data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="row">The child data record for which the parent data
		/// record is set. Must not be <tt>null</tt>.</param>
		/// <param name="ParentType">The DAC type of the parent data
		/// record.</param>
		/// <param name="parent">The new parent data record.</param>
		/// <example>
		/// The code below sets the parent data record at run time.
		/// <code>
		/// // Views definitions in a graph
		/// public PXSelect&lt;INRegister&gt; inregister;
		/// public PXSelect&lt;INTran&gt; intranselect;
		/// ...
		/// // Code executed in some graph method
		/// INTran tran = (INTran)res;
		/// PXParentAttribute.SetParent(
		///     intranselect.Cache, tran, typeof(INRegister), inregister.Current);
		/// </code>
		/// </example>
		public static void SetParent(PXCache cache, object row, Type ParentType, object parent)
		{
			if (row == null)
			{
				throw new PXArgumentException();
			}

			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];

				if (parent == null)
				{
					if (attr._Currents.ContainsKey(row))
					{
						attr._Currents.Remove(row);
					}
				}
				else
				{
					object cached;
					if ((cached = cache.Locate(row)) == null || cache.GetStatus(row) == PXEntryStatus.InsertedDeleted || cache.GetStatus(row) == PXEntryStatus.Deleted)
					{
						attr._Pendings[row] = parent;

						if (attr._Currents.ContainsKey(row))
							attr._Currents.Remove(row);
					}
					else
					{
						attr._Currents[cached] = parent;
					}
				}
			}

		}

		/// <summary>Makes the parent of the provided data record be the parent of
		/// the other provided data record. Uses the first attribute instance that
		/// references the provided parent type or a type derived from
		/// it.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="item">The child data record whose parent data record is
		/// made the parent of another data record.</param>
		/// <param name="copy">The data record that becomes the child of the
		/// provided data record's parent.</param>
		/// <param name="ParentType">The DAC type of the parent data
		/// record.</param>
		[Obsolete]
		public static void CopyParent(PXCache cache, object item, object copy, Type ParentType)
		{
			if (item == null)
			{
				throw new PXArgumentException();
			}

			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];

				object parent;
				if (attr._Currents.TryGetValue(item, out parent))
				{
					attr._Currents[copy] = parent;
				}
			}
		}

		/// <summary>Returns the parent data record of the provided child data
		/// record. Uses the first attribute instance found in the cache
		/// object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="row">The child data record whose parent data record is
		/// retireved.</param>
		public static object SelectParent(PXCache cache, object row)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					parents.Add(attr);
				}
			}

			if (parents.Count > 1)
			{
				throw new PXException(ErrorMessages.UnconditionalUniqueParent, cache.GetItemType().ToString());
			}

			if (parents.Count > 0)
			{
				return SelectParent(cache, row, null, null, (PXParentAttribute)parents[0]);
			}

			return null;
		}
		/// <summary>Returns the parent data record of the provided child data
		/// record. Uses the first attribute instance that references the provided
		/// parent type or a type derived from it.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXParent</tt> type.</param>
		/// <param name="row">The child data record whose parent data record is
		/// retireved.</param>
		/// <param name="ParentType">The DAC type of the parent data
		/// record.</param>
		/// <example>
		/// The code below obtains the parent data record at run time.
		/// <code>
		/// CR.Location child = (CR.Location)e.Row;
		/// BAccount parent =
		///     (BAccount)PXParentAttribute.SelectParent(sender, child, typeof(BAccount));
		/// </code>
		/// </example>
		public static object SelectParent(PXCache cache, object row, Type ParentType)
		{
			return SelectParent(cache, row, null, ParentType);
		}
		public static TParentTable SelectParent<TParentTable>(PXCache cache, object row) where TParentTable : IBqlTable => (TParentTable)SelectParent(cache, row, typeof(TParentTable));

		internal static object SelectParent(PXCache cache, object row, object newRow, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				return SelectParent(cache, row, newRow, ParentType, (PXParentAttribute)parents[0]);
			}
			return null;
		}

		private static object SelectParent(PXCache cache, object row, object newRow, Type ParentType, PXParentAttribute attr)
		{
			PXView parentview = attr.GetParentSelect(cache);

			if (!(ParentType == null || parentview.CacheGetItemType() == ParentType || ParentType.IsAssignableFrom(parentview.CacheGetItemType())))
			{
				return null;
			}

			if (ParentType == null)
			{
				ParentType = parentview.CacheGetItemType();
			}
			
			object parent;
			if (attr._UseCurrent || 
				(ParentType.IsAssignableFrom(cache.Graph.PrimaryItemType)
				&& parentview.BqlSelect.Meet(parentview.Cache, parentview.Cache.Current, parentview.PrepareParameters(new object[] { row }, new object[0]))))
			{
				parent = parentview.Cache.Current;
				return !(parentview.Cache.GetStatus(parent) == PXEntryStatus.Deleted  || parentview.Cache.GetStatus(parent) == PXEntryStatus.InsertedDeleted) ? parent : null;
			}
			else
			{
				if (row != null)
				{
					if (attr._Currents.TryGetValue(row, out parent))
					{
						return parent;
					}

					if (newRow != null && attr._Pendings.TryGetValue(newRow, out parent))
					{
						attr._Currents[row] = parent;
						attr._Pendings.Remove(newRow);
					}

					if (parent != null)
					{
						return parent;
					}
				}
				return PXSelectorAttribute.SelectSingleBound(parentview, new object[] { row });
			}
		}
		#endregion

		#region Initialization

		protected static Dictionary<Type, BqlCommand> _selects = new Dictionary<Type, BqlCommand>();
		protected static object _slock = new object();
		protected void _initialize(PXCache sender)
		{
			BqlCommand selectchild;
			lock (_slock)
			{
				if (!_selects.TryGetValue(_SelectParent.GetType(), out selectchild))
				{
					selectchild = BqlCommand.CreateInstance(Inverse(_SelectParent.GetType(), _ParentType, _BqlTable));
					_selects.Add(_SelectParent.GetType(), selectchild);
				}
			}
			_SelectChildren = selectchild;
		}

		/// <exclude />
		public static Type Inverse(Type parentSelectCommand, Type parentTable, Type childTable) => _inverse(parentSelectCommand, parentTable, ref childTable);

		protected static Type _inverse(Type command, Type parent, ref Type child)
        {
            command = BqlCommandDecorator.Unwrap(command);
			if (!command.IsGenericType)
			{
				if (command == parent)
				{
					return child;
				}
				if (typeof(IBqlField).IsAssignableFrom(command) && command.IsNested && BqlCommand.GetItemType(command) == parent)
				{
					return BqlCommand.Compose(typeof(Current<>), command);
				}
				return command;
			}
			else
			{
				Type[] args = command.GetGenericArguments();
				if ((command.GetGenericTypeDefinition() == typeof(Current<>) || command.GetGenericTypeDefinition() == typeof(Current2<>)) && typeof(IBqlField).IsAssignableFrom(args[0]) && args[0].IsNested && child.IsAssignableFrom(BqlCommand.GetItemType(args[0])))
				{
					child = BqlCommand.GetItemType(args[0]);
					return args[0];
				}
				bool anyChanged = false;
				for (int i = args.Length - 1; i >= 0; i--)
				{
					Type t = _inverse(args[i], parent, ref child);
					if (t != args[i])
					{
						args[i] = t;
						anyChanged = true;
					}
				}
				if (!anyChanged)
				{
					return command;
				}
				Type[] pars = new Type[args.Length + 1];
				pars[0] = command.GetGenericTypeDefinition();
				for (int i = 1; i < pars.Length; i++)
				{
					pars[i] = args[i - 1];
				}
				return BqlCommand.Compose(pars);
			}
		}
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();
			sender.Graph.RowDeleted.AddHandler(_ChildType, SelfRowDeleted);
			if (!_LeaveChildren /*&& sender.Graph.Caches.ContainsKey(_ParentType)*/)
			{
				sender.Graph.RowDeleted.AddHandler(_ParentType, this.RowDeleted);
			}

			_Currents = (ParentCollection)Activator.CreateInstance(typeof(CurrentCollection<>).MakeGenericType(_ChildType), sender, this);
			_Pendings = (ParentCollection)Activator.CreateInstance(typeof(PendingCollection<>).MakeGenericType(_ChildType), sender, this);
		}
		#endregion

		/// <exclude/>
		public abstract class ParentCollection
		{
			public abstract bool TryGetValue(object key, bool remove, out object value);
			public abstract void Clear();
			public abstract bool ContainsKey(object key);
			public abstract bool Remove(object key);

			public abstract object this[object key]
			{
				get;
				set;
			}

			public bool TryGetValue(object key, out object value)
			{
				return this.TryGetValue(key, true, out value);
			}
		}

		/// <exclude/>
		public class PendingCollection<TNode> : ParentCollection, IEqualityComparer<TNode>
			where TNode : class, IBqlTable
		{
			protected PXCache _cache;
			protected PXParentAttribute _owner;
			protected Dictionary<TNode, KeyValuePair<TNode, object>> _dict = null;

			public PendingCollection(PXCache cache, PXParentAttribute owner)
			{
				_cache = cache;
				_owner = owner;
				_dict = new Dictionary<TNode, KeyValuePair<TNode, object>>(this);
	}

			public override bool TryGetValue(object key, bool remove, out object value)
			{
				KeyValuePair<TNode, object> pair;
				bool found = _dict.TryGetValue((TNode)key, out pair);
				if (found)
				{
					//if key is exact reference of dictionary key 
					if (object.ReferenceEquals(key, pair.Key))
					{
						value = pair.Value;
						return _owner.GetParentSelect(_cache).Cache.GetStatus(value) != PXEntryStatus.Deleted && _owner.GetParentSelect(_cache).Cache.GetStatus(value) != PXEntryStatus.InsertedDeleted;
					}
					//remove expired values
					if (remove) _dict.Remove(pair.Key);
				}
				value = null;
				return false;
			}

			public override object this[object key]
			{
				get
				{
					return _dict[(TNode)key].Value;
				}
				set
				{
					_dict[(TNode)key] = new KeyValuePair<TNode,object>((TNode)key, value);
				}
			}

			public override void Clear()
			{
				_dict.Clear();
			}

			public bool Equals(TNode a, TNode b)
			{
				return _cache.ObjectsEqual(a, b);
			}

			public int GetHashCode(TNode a)
			{
				return _cache.GetObjectHashCode(a);
			}

			public override bool ContainsKey(object key)
			{
				return _dict.ContainsKey((TNode)key);
			}

			public override bool Remove(object key)
			{
				return _dict.Remove((TNode)key);
			}
		}

		/// <exclude/>
		public class CurrentCollection<TNode> : PendingCollection<TNode>
			where TNode : class, IBqlTable
		{
			public CurrentCollection(PXCache cache, PXParentAttribute owner)
				: base(cache, owner)
			{
			}

			public override bool TryGetValue(object key, bool remove, out object value)
			{
				KeyValuePair<TNode, object> pair;
				bool found = _dict.TryGetValue((TNode)key, out pair);
				if (found)
				{
					//if key is exact reference of dictionary key or key is a copy of dictionary key stored in cache 
					if (object.ReferenceEquals(key, pair.Key) || object.ReferenceEquals(_cache.Locate(pair.Key), pair.Key))
					{
						value = pair.Value;
						return _owner.GetParentSelect(_cache).Cache.GetStatus(value) != PXEntryStatus.Deleted && _owner.GetParentSelect(_cache).Cache.GetStatus(value) != PXEntryStatus.InsertedDeleted;
					}
					//remove expired values
					if (remove) _dict.Remove(pair.Key);
				}
				value = null;
				return false;
			}
		}
	}
	#endregion

	#region PXUIFieldAttribute
	/// <summary>Configures the properties of the input control representing a DAC field in the user interface, or the button representing an action. The attribute is mandatory
	/// for all DAC fields that are displayed in the user interface.</summary>
	/// <remarks>
	///   <para>You can use the static methods (such as <tt>SetEnabeled</tt>, <tt>SetRequired</tt>) to set the properties of the attribute at run time. The
	/// <tt>PXUIFieldAttribute</tt> static methods can be called either in the business logic container constructor or the <tt>RowSelected</tt> event handlers.</para>
	///   <para>If you want to modify the <tt>Visible</tt>, <tt>Enabled</tt>, and <tt>Required</tt> properties for all detail rows in a grid, you use the
	/// <tt>RowSelected</tt> event handler of the primary view DAC. If you wan to set the Enabled property of a field in particular row in a grid, you use the
	/// <tt>RowSelected</tt> event handler of the DAC that includes this field.</para>
	///   <para>If the grid column layout is configured at run time, you set the <tt>data</tt> parameter of the corresponding method to <tt>null</tt>. This indicates that
	/// the property should be set for all data records shown in the grid. If a specific data record is passed to the method rather than <tt>null</tt>, the method
	/// invocation has no effect.</para>
	///   <para>If you want to change the <tt>Visible</tt> or <tt>Enabled</tt> property of <tt>PXUIFieldAttribute</tt> for a button at run time, you use the corresponding
	/// static methods of <tt>PXAction</tt>. You usually use these methods in the <tt>RowSelected</tt> event handler of the primary view DAC.</para>
	/// </remarks>
	/// <example>
	///   <code title="Example1" description="The code below shows configuration of the input control for a DAC field." lang="CS">
	/// [PXDBDecimal(2)]
	/// [PXUIField(DisplayName = "Documents Total",
	///            Visibility = PXUIVisibility.SelectorVisible,
	///            Enabled = false)]
	/// public virtual decimal? CuryDocsTotal { get; set; }</code>
	///   <code title="Example2" description="The example below shows how to change layout configuration at run time. Note in the &lt;tt&gt;SetEnabled&lt;/tt&gt; method, the first parameter is set to the cache variable. This is a &lt;tt&gt;PXCache&lt;/tt&gt; object keeping &lt;tt&gt;APInvoice&lt;/tt&gt; data records. The second parameter is set to such a data record obtained from &lt;tt&gt;e.Row&lt;/tt&gt;.&#xD;&#xA;On the other hand, the &lt;tt&gt;SetVisible&lt;/tt&gt; method is called for the &lt;tt&gt;APTran&lt;/tt&gt; DAC field, and therefore a different cache object should be passed to the method. The appropriate cache is specified using the &lt;tt&gt;Cache&lt;/tt&gt; property of the &lt;tt&gt;Transactions&lt;/tt&gt; view." lang="CS">
	/// protected virtual void APInvoice_RowSelected(PXCache cache,
	///                                              PXRowSelectedEventArgs)
	/// {
	///     APInvoice doc = e.Row as APInvoice;
	/// 
	///     // Disable the field input control
	///     PXUIFieldAttribute.SetEnabled&lt;APInvoice.taxZoneID&gt;(
	///         cache, doc, false);
	/// 
	///     // Showing or hiding a 'required' mark beside a field input control
	///     PXUIFieldAttribute.SetRequired&lt;APInvoice.dueDate&gt;(
	///         cache, (doc.DocType != APDocType.DebitAdj)); 
	/// 
	///     // Making a field visible.
	///     // The data parameter is set to null to set the property for all
	///     // APTran data records. The cache object is obtained through the
	///     // Transactions data view
	///     PXUIFieldAttribute.SetVisible&lt;APTran.projectID&gt;(
	///         Transactions.Cache, null, true);
	/// }
	/// 
	/// // Definition of the Transactions data view in the same graph
	/// public PXSelect&lt;APTran,
	///     Where&lt;APTran.tranType, Equal&lt;Current&lt;APInvoice.docType&gt;&gt;,
	///         And&lt;APTran.refNbr, Equal&lt;Current&lt;APInvoice.refNbr&gt;&gt;&gt;&gt;&gt;
	///     Transactions;</code>
	///   <code title="Example3" description="The following example shows how to use the attribute to configure actions." lang="CS">
	/// // The action declaration
	/// public PXAction&lt;APDocumentFilter&gt; viewDocument;
	/// // The action method declaration
	/// [PXUIField(DisplayName = "View Document",
	///            MapEnableRights = PXCacheRights.Select,
	///            MapViewRights = PXCacheRights.Select)]
	/// [PXButton]
	/// public virtual IEnumerable ViewDocument(PXAdapter adapter)
	/// {
	///     ...
	/// }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
	[PXAttributeFamily(typeof(PXUIFieldAttribute))]
	public class PXUIFieldAttribute : PXEventSubscriberAttribute, IPXInterfaceField, IPXExceptionHandlingSubscriber, IPXCommandPreparingSubscriber, IPXFieldSelectingSubscriber, IPXFieldVerifyingSubscriber
	{
		#region State
		protected string _ErrorText = null;
		protected object _ErrorValue = null;
		protected PXErrorLevel _ErrorLevel = PXErrorLevel.Undefined;
		protected bool _Enabled = true;
		protected bool _Visible = true;
		protected bool _ReadOnly = false;
		protected string _DisplayName = null;
		protected bool _Filterable = true;
		protected PXUIVisibility _Visibility = PXUIVisibility.Visible;
		protected int _TabOrder = -1;
		protected bool _ViewRights = true;
		protected bool _EnableRights = true;
		protected PXCacheRights _MapViewRights = PXCacheRights.Select;
		protected PXCacheRights _MapEnableRights = PXCacheRights.Update;
		protected PXErrorHandling _ErrorHandling = PXErrorHandling.WhenVisible;
		protected bool? _Required;
		protected object _RestoredValue;
		protected string _NeutralDisplayName = null;
		protected string _FieldClass;
		protected Type _MapErrorTo = null;

		/// <summary>Gets or sets the field to which the system should map 
		/// the error related to the field with the attribute. </summary>
		/// <value>If the value of the property is set to a field type, 
		/// the error is mapped to the specified field and the field with the attribute.
		/// If the value is <tt>null</tt>, the error is mapped to only the field with the attribute.
		/// By default, the value is <tt>null</tt>.
		/// </value>
		public Type MapErrorTo
		{
			get
			{
				return _MapErrorTo;
			}
			set
			{
				_MapErrorTo = value;
			}
		}

		/// <summary>Gets or sets the value that indicates whether the field is
		/// shown or hidded depending on the features enabled or disabled. By
		/// default, the property is set to the segmented field name.</summary>
		public virtual string FieldClass
		{
			get
			{
				return _FieldClass;
			}
			set
			{
				_FieldClass = value;
			}
		}
		/// <summary>Gets or sets the value that indicates whether an asterisk
		/// sign is shown beside the field in the user interface. Note that this
		/// property <i>does not</i> check that the field value is specified and
		/// add any restriction of this kind. This is done by the <see
		/// cref="PXDefaultAttribute">PXDefault</see> attribute.</summary>
		/// <remarks>The default value is <tt>false</tt>.</remarks>
		public virtual bool Required
		{
			get
			{
				return _Required == true;
			}
			set
			{
				_Required = value;
			}
		}
		/// <summary>Gets or sets the <see
		/// cref="PXErrorHandling">PXErrorHandling</see> value that
		/// specifies the way the attribute treats an error related to the field.
		/// The error is either indicated only when the field is visible, always
		/// indicated, or never indicated.</summary>
		/// <remarks>The default value is
		/// <tt>PXErrorHandling.WhenVisible</tt>.</remarks>
		public virtual PXErrorHandling ErrorHandling
		{
			get
			{
				return _ErrorHandling;
			}
			set
			{
				_ErrorHandling = value;
			}
		}

        [PX.Common.PXInternalUseOnly]
        public PXErrorLevel ErrorLevel
        {
			get
			{
				return _ErrorLevel;
			}
		}

		
		protected internal virtual bool ViewRights
		{
			get
			{
				return _ViewRights;
			}
			set
			{
				_ViewRights = value;
				if (!value)
				{
					_Visible = false;
				}
			}
		}
		protected internal virtual bool EnableRights
		{
			get
			{
				return _EnableRights;
			}
			set
			{
				_EnableRights = value;
				if (!value)
				{
					_Enabled = false;
				}
			}
		}
		/// <summary>Gets or sets the <see
		/// cref="PXCacheRights">PXCacheRights</see> value that
		/// specifies the access on a cache for a cache to see the button in the
		/// user interface. The property is used when the <tt>PXUIField</tt>
		/// configures an action button.</summary>
		public virtual PXCacheRights MapViewRights
		{
			get
			{
				return _MapViewRights;
			}
			set
			{
				_MapViewRights = value;
			}
		}
		/// <summary>Gets or sets the <see
		/// cref="PXCacheRights">PXCacheRights</see> value that specifies
		/// the access rights on a cache to click the button in the user
		/// interface. The property is used when the <tt>PXUIField</tt> configures
		/// an action button.</summary>
		public virtual PXCacheRights MapEnableRights
		{
			get
			{
				return _MapEnableRights;
			}
			set
			{
				_MapEnableRights = value;
			}
		}
		public PXUIFieldAttribute()
		{
			if (_FieldSourceType == null)
			{
				_FieldSourceType = new FieldSourceType();
			}
		}
		string IPXInterfaceField.ErrorText
		{
			get
			{
				return _ErrorText;
			}
			set
			{
				_ErrorText = value;
			}
		}
		object IPXInterfaceField.ErrorValue
		{
			get
			{
				return _ErrorValue;
			}
			set
			{
				_ErrorValue = value;
			}
		}
		PXErrorLevel IPXInterfaceField.ErrorLevel
		{
			get
			{
				return _ErrorLevel;
			}
			set
			{
				_ErrorLevel = value;
			}
		}
		void IPXInterfaceField.ForceEnabled()
		{
			_Enabled = true;
		}
		bool IPXInterfaceField.ViewRights
		{
			get
			{
				return _ViewRights;
			}
		}

		/// <summary>Gets or sets the value that indicates whether the field input
		/// control is enabled. If the field is disabled, the control does not
		/// allow the user to edit and select the field value. Compare to the
		/// <tt>IsReadOnly</tt> property.</summary>
		/// <remarks>The default value is <tt>true</tt>.</remarks>
		public virtual bool Enabled
		{
			get
			{
				return _Enabled;
			}
			set
			{
				if (_EnableRights || !value)
				{
					_Enabled = value;
				}
			}
		}

		/// <summary>Get, set. Allows to show/hide field edit control or grid
		/// column in user interface. To control, whether form designer should
		/// generate template for this field, use Visibility property
		/// instead.</summary>
		/// <remarks>The default value is <tt>true</tt>.</remarks>
		public virtual bool Visible
		{
			get
			{
				return _Visible;
			}
			set
			{
				if (_ViewRights || !value)
				{
					_Visible = value;
				}
			}
		}

		/// <summary>Gets or sets the value that indicates whether the field input
		/// control allows editing. If the property is set to <tt>true</tt>, the
		/// user cannot edit the value, but can still select and copy the value.
		/// Compare to the <tt>Enabled</tt> property.</summary>
		/// <remarks>The default value is <tt>false</tt>.</remarks>
		public virtual bool IsReadOnly
		{
			get
			{
				return _ReadOnly;
			}
			set
			{
				_ReadOnly = value;
			}
		}

		/// <summary>Gets or sets the field name displayed in the user interface.
		/// This name is rendered as the input control label on a form or as the
		/// grid column header.</summary>
		/// <remarks>The default value is the field name.</remarks>
		public virtual string DisplayName
		{
			get
			{
				if (_DisplayName == null)
				{
					return _DisplayName = _FieldName;
				}
				return _DisplayName;
			}
			set
			{
				_DisplayName = value;
			}
		}

		/// <summary>Gets or sets the value that indicates whether the labels used
		/// by the attribute are localizable.</summary>
		public virtual bool IsLocalizable { get; set; } = true;

		/// <summary>Gets or sets the <see
		/// cref="PXUIVisibility">PXUIVisibility</see> value that
		/// indicates whether the webpage layout designer should generate a
		/// template for this field. You can specify whether the template is
		/// generated for a form and grid, is generated for a form, grid, and
		/// lookup controls, or never appear in the user interface. The default
		/// value is <tt>PXUIVisibility.Visible</tt>.</summary>
		public virtual PXUIVisibility Visibility
		{
			get
			{
				return _Visibility;
			}
			set
			{
				_Visibility = value;
			}
		}
		/// <summary>Gets or sets the order in which the field input control gets
		/// the focus when the user moves it by pressing the TAB key.</summary>
		public virtual int TabOrder
		{
			get
			{
				if (_TabOrder == -1)
				{
					return _FieldOrdinal;
				}
				return _TabOrder;
			}
			set
			{
				_TabOrder = value;
			}
		}
		internal string NeutralDisplayName
		{
			get
			{
				if (_NeutralDisplayName == null)
				{
					_NeutralDisplayName = _DisplayName ?? _FieldName;
				}
				return _NeutralDisplayName;
			}
		}
		#endregion

		#region Runtime
		/// <exclude/>
		public void ChangeNeutralDisplayName(string newValue)
		{
			_NeutralDisplayName = newValue;
		}

		/// <summary>Finds all fields with non-empty error strings and returns a
		/// dictionary with field names as the keys and error messages as the
		/// values.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record whose fields are checked for error
		/// strings. If <tt>null</tt>, the method takes into account all data
		/// records in the cache object.</param>
		public static Dictionary<string, string> GetErrors(PXCache cache, object data, params PXErrorLevel[] errorLevels)
		{
			Dictionary<string, string> ret = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (IPXInterfaceField attr in cache.GetAttributes(data, null).OfType<IPXInterfaceField>())
			{
				string err = attr.ErrorText;
				if (!String.IsNullOrEmpty(err) 
					&& (errorLevels == null || errorLevels.Length == 0 || errorLevels.Contains(attr.ErrorLevel)))
				{
					ret[((PXEventSubscriberAttribute)attr).FieldName] = err;
				}
			}
			return ret;
		}

		public static void SetError(PXCache cache, object data, string name, string error, string errorvalue, bool resetErrorValue, PXErrorLevel level)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (IPXInterfaceField attr in cache.GetAttributes(data, name).OfType<IPXInterfaceField>())
			{
				attr.ErrorText = PXMessages.LocalizeNoPrefix(error);
				attr.ErrorLevel = level;
				attr.ErrorValue = resetErrorValue && errorvalue == null ? null : errorvalue;
			}
		}

		/// <summary>Sets the error string to display as a tooltip for the field
		/// with the specified name.
		/// The value of the field will be cleared at UI.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="error">The string that is set as the error message
		/// string.</param>
		public static void SetError(PXCache cache, object data, string name, string error)
		{
			SetError(cache, data, name, error, null, false, error == null ? PXErrorLevel.Undefined : PXErrorLevel.Error);
		}

		/// <summary>Sets the error string to display as a tooltip for the
		/// specified field.
		/// The value of the field will be cleared at UI.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="error">The error string displayed as a tooltip on the
		/// field input control.</param>
		public static void SetError<Field>(PXCache cache, object data, string error)
			where Field : IBqlField
		{
			SetError(cache, data, typeof(Field).Name, error);
		}

		/// <summary>Sets the error string to display as a tooltip and the error
		/// value to display in the input control for the field with the specified
		/// name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="error">The error string displayed as a tooltip on the
		/// field input control.</param>
		/// <param name="errorValue">The string displayed in the field input
		/// control (is not assigned to the field).</param>
		public static void SetError(PXCache cache, object data, string name, string error, string errorvalue)
		{
			SetError(cache, data, name, error, errorvalue, true, error == null ? PXErrorLevel.Undefined : PXErrorLevel.Error);
		}

		/// <summary>Sets the error string to display as a tooltip and the error
		/// value to display in the input control for the specified field. The
		/// error level is set to <tt>PXErrorLevel.Error</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="error">The error string displayed as a tooltip on the
		/// field input control.</param>
		/// <param name="errorValue">The string displayed in the field input
		/// control (is not assigned to the field).</param>
		public static void SetError<Field>(PXCache cache, object data, string error, string errorvalue)
			where Field : IBqlField
		{
			SetError(cache, data, typeof(Field).Name, error, errorvalue);
		}

		/// <summary>Sets the error string to display as a tooltip for the field
		/// with the specified name. The error level is set to
		/// <tt>PXErrorLevel.Warning</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="error">The error string displayed as a tooltip on the
		/// field input control.</param>
		public static void SetWarning(PXCache cache, object data, string name, string error)
		{
			SetError(cache, data, name, error, null, true, PXErrorLevel.Warning);
		}

		/// <summary>Sets the error string to display as a tooltip for the
		/// specified field. The error level is set to
		/// <tt>PXErrorLevel.Warning</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="error">The error string displayed as a tooltip on the
		/// field input control.</param>
		public static void SetWarning<Field>(PXCache cache, object data, string error)
			where Field : IBqlField
		{
			SetWarning(cache, data, typeof(Field).Name, error);
		}

		/// <summary>Returns the error string displayed for the specified
		/// field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		public static string GetError<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetError(cache, data, typeof(Field).Name);
		}

		/// <summary>Returns the error string displayed for the field with the
		/// specified name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		public static string GetError(PXCache cache, object data, string name)
		{
			return cache.GetAttributes(data, name)
				.OfType<IPXInterfaceField>()
				.FirstOrDefault()
				.With(a => a.ErrorText);
		}

		/// <summary>Returns the error string (if error level is Error or RowError) displayed for the specified
		/// field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		public static string GetErrorOnly<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetErrorOnly(cache, data, typeof(Field).Name);
		}

		/// <summary>Returns the error string (if error level is Error or RowError) displayed for the field with the
		/// specified name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		public static string GetErrorOnly(PXCache cache, object data, string name)
		{
			return cache.GetAttributes(data, name)
				.OfType<IPXInterfaceField>()
				.Where(a => a.ErrorLevel == PXErrorLevel.Error || a.ErrorLevel == PXErrorLevel.RowError)
				.FirstOrDefault()
				.With(a => a.ErrorText);
		}


		/// <summary>Returns the warning string displayed for the specified
		/// field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		public static string GetWarning<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetWarning(cache, data, typeof(Field).Name);
		}

		/// <summary>Returns the warning string displayed for the field with the
		/// specified name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		public static string GetWarning(PXCache cache, object data, string name)
		{
			return cache.GetAttributes(data, name)
				.OfType<IPXInterfaceField>()
				.Where(a => a.ErrorLevel == PXErrorLevel.Warning || a.ErrorLevel == PXErrorLevel.RowWarning)
				.FirstOrDefault()
				.With(a => a.ErrorText);
		}

		public static (string errorMessage, PXErrorLevel errorLevel) GetErrorWithLevel<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetErrorWithLevel(cache, data, typeof(Field).Name);
		}

		public static (string errorMessage, PXErrorLevel errorLevel) GetErrorWithLevel(PXCache cache, object data, string name)
		{
			return cache.GetAttributes(data, name)
				.OfType<IPXInterfaceField>()
				.FirstOrDefault()
				.With(a => (a.ErrorText, a.ErrorLevel));
		}

		/// <summary>Enables or disables the input control for the field with the
		/// specified name by setting the <tt>Enabled</tt> property.
		/// If <tt>Enabled</tt> is set to <tt>false</tt>, the method also clears 
		/// the required mark of the field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="isEnabled">The value that is assigned to the
		/// <tt>Enabled</tt> property.</param>
		public static void SetEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
				}
			}
		}
		/// <summary>Enables or disables the input controls for all fields in the
		/// specific data record or all data records by setting the
		/// <tt>Enabled</tt> property.
		/// If <tt>Enabled</tt> is set to <tt>false</tt>, the method also clears 
		/// the required mark of the field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isEnabled">The value that is assigned to the
		/// <tt>Enabled</tt> property.</param>
		public static void SetEnabled(PXCache cache, object data, bool isEnabled)
		{
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, null))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
					if (data == null)
					{
						cache.SetAltered(attr.FieldName, true);
					}
				}
			}
		}
		/// <summary>Enables or disables the input control for the specified field
		/// by setting the <tt>Enabled</tt> property.
		/// If <tt>Enabled</tt> is set to <tt>false</tt>, the method also clears 
		/// the required mark of the field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isEnabled">The value that is assigned to the
		/// <tt>Enabled</tt> property.</param>
		public static void SetEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, typeof(Field).Name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
				}
			}
		}
		/// <summary>Enables the input control for the field with the specified
		/// name by setting the <tt>Enabled</tt> property to
		/// <tt>true</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		public static void SetEnabled(PXCache cache, object data, string name)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = true;
				}
			}
		}
		/// <summary>Enables the specified field of the specific data record in
		/// the cache object by setting the <tt>Enabled</tt> property to
		/// <tt>true</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		public static void SetEnabled<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, typeof(Field).Name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = true;
				}
			}
		}

		#region SetVisible

		/// <summary>Makes the input control for the specified field visible in
		/// the user interface by setting the <tt>Visible</tt> property to
		/// <tt>true</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		public static void SetVisible<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, typeof(Field).Name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = true;
				}
			}
		}
		/// <summary>Shows or hides the input control for the specified field in
		/// the user interface by setting the <tt>Visible</tt> property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isVisible">The value that is assigned to the
		/// <tt>Visible</tt> property.</param>
		public static void SetVisible<Field>(PXCache cache, object data, bool isVisible)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, typeof(Field).Name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = isVisible;
				}
			}
		}
		/// <summary>Shows or hides the input control for the field with the
		/// specified name in the user interface by setting the <tt>Visible</tt>
		/// property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="isVisible">The value that is assigned to the
		/// <tt>Enabled</tt> property.</param>
		public static void SetVisible(PXCache cache, object data, string name, bool isVisible)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = isVisible;
				}
			}
		}
		/// <summary>Makes the input control for the field with the specified name
		/// visible in the user interface by setting the <tt>Visible</tt> property
		/// to <tt>true</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		public static void SetVisible(PXCache cache, object data, string name)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (var attr in cache.GetAttributesOfType<PXUIFieldAttribute>(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = true;
				}
			}
		}

		#endregion

		#region SetReadOnly

		/// <summary>Makes the input control for the specified field read-only by
		/// setting the <tt>IsReadOnly</tt> property to <tt>true</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		public static void SetReadOnly<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			SetReadOnly<Field>(cache, data, true);
		}
		/// <summary>Makes the input control for the field with the specified name
		/// read- only by setting the <tt>IsReadOnly</tt> property to
		/// <tt>true</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		public static void SetReadOnly(PXCache cache, object data, string name)
		{
			SetReadOnly(cache, data, name, true);
		}
		/// <summary>Makes the input controls for all fields read-only by setting
		/// the <tt>IsReadOnly</tt> property to <tt>true</tt>.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		public static void SetReadOnly(PXCache cache, object data)
		{
			SetReadOnly(cache, data, true);
		}
		/// <summary>Makes the input controls for all field read-only or not read-
		/// only by setting the <tt>IsReadOnly</tt> property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isReadOnly">The value that is assigned to the
		/// <tt>IsReadOnly</tt> property.</param>
		public static void SetReadOnly(PXCache cache, object data, bool isReadOnly)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, null))
			{
				var uiAtt = attr as PXUIFieldAttribute;
				if (uiAtt == null) continue;

				uiAtt.IsReadOnly = isReadOnly;
				if (data == null) cache.SetAltered(attr.FieldName, true);
			}
		}
		/// <summary>Makes the input control for the specified field read-only or
		/// not-read- only by setting the <tt>IsReadOnly</tt> property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isReadOnly">The value that is assigned to the
		/// <tt>IsReadOnly</tt> property.</param>
		public static void SetReadOnly<Field>(PXCache cache, object data, bool isReadOnly)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ReadOnly = isReadOnly;
				}
			}
		}
		/// <summary>Makes the input control for the field with the specified name
		/// read- only or not-read-only by setting the <tt>IsReadOnly</tt>
		/// property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="isReadOnly">The value that is assigned to the
		/// <tt>IsReadOnly</tt> property.</param>
		public static void SetReadOnly(PXCache cache, object data, string name, bool isReadOnly)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ReadOnly = isReadOnly;
				}
			}
		}

		#endregion

		/// <summary>Enables or disables the input control for the field with the
		/// specified name by setting the <tt>Enabled</tt> property.
		/// If <tt>Enabled</tt> is set to <tt>false</tt>, the method also clears 
		/// the required mark of the field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="isEnabled">The value that is assigned to the
		/// <tt>Enabled</tt> property.</param>
		public static void SetEnabled(PXCache cache, string name, bool isEnabled)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXUIFieldAttribute)
				{
					if (name == null)
						cache.SetAltered(attr.FieldName, true);
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
				}
			}
		}
		/// <summary>Shows or hides the input control for the field with the
		/// specified name in the user interface for all data record by setting
		/// the <tt>Visible</tt> property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="isVisible">The value that is assigned to the
		/// <tt>Enabled</tt> property.</param>
		public static void SetVisible(PXCache cache, string name, bool isVisible)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = isVisible;
				}
			}
		}
		/// <summary>Sets the visibility status of the input control for the
		/// specified field by setting the <tt>Visibility</tt> property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="visibility">The value that is assigned to the
		/// <tt>Visibility</tt> property.</param>
		public static void SetVisibility<Field>(PXCache cache, object data, PXUIVisibility visibility)
			where Field : IBqlField
		{
			if (visibility == PXUIVisibility.Undefined)
			{
				return;
			}
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Visibility = visibility;
				}
			}
		}
		/// <summary>Sets the visibility status of the input control for the field
		/// with the specified name by setting the <tt>Visibility</tt>
		/// property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The field name.</param>
		/// <param name="visibility">The value that is assigned to the
		/// <tt>Visibility</tt> property.</param>
		public static void SetVisibility(PXCache cache, object data, string name, PXUIVisibility visibility)
		{
			if (visibility == PXUIVisibility.Undefined)
			{
				return;
			}
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Visibility = visibility;
				}
			}
		}
		/// <summary>Sets the visibility status of the input control for the field
		/// with the specified name by setting the <tt>Visibility</tt>
		/// property.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="visibility">The value that is assigned to the
		/// <tt>Enabled</tt> property.</param>
		public static void SetVisibility(PXCache cache, string name, PXUIVisibility visibility)
		{
			if (visibility == PXUIVisibility.Undefined)
			{
				return;
			}
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Visibility = visibility;
				}
			}
		}
		/// <summary>Returns the value of the <tt>DisplayName</tt> property for
		/// the specified field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		public static string GetDisplayName<Field>(PXCache cache)
			where Field : IBqlField
		{
			return GetDisplayName(cache, typeof(Field).Name);
		}
		/// <summary>Returns the value of the <tt>DisplayName</tt> property for
		/// the field with the specified name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="name">The field name.</param>
		public static string GetDisplayName(PXCache cache, string fieldName)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					return ((PXUIFieldAttribute)attr).DisplayName;
				}
			}
			return fieldName;
		}
		/// <exclude/>
		public static string GetNeutralDisplayName(PXCache cache, string fieldName)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					return ((PXUIFieldAttribute)attr).NeutralDisplayName;
				}
			}
			return fieldName;
		}
		/// <exclude />
		public static string SetNeutralDisplayName(PXCache cache, string fieldName, string neutralDisplaydName)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).ChangeNeutralDisplayName(neutralDisplaydName);
					break;
				}
			}
			return fieldName;
		}
		/// <summary>Sets the display name of the specified field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="displayName">The new display name.</param>
		public static void SetDisplayName<Field>(PXCache cache, string displayName)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXUIFieldAttribute)
				{
					string msgprefix;
					string newDisplayName = PXMessages.Localize(displayName, out msgprefix);
					((PXUIFieldAttribute)attr).DisplayName = newDisplayName;
					((PXUIFieldAttribute)attr).ChangeNeutralDisplayName(displayName);
					break;
				}
			}
		}
		/// <summary>Sets the display name of the field with the specified
		/// name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="displayName">The new display name.</param>
		public static void SetDisplayName(PXCache cache, string fieldName, string displayName)
		{
			cache.SetAltered(fieldName, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					string msgprefix;
					string newDisplayName = PXMessages.Localize(displayName, out msgprefix);
					((PXUIFieldAttribute)attr).DisplayName = newDisplayName;
					((PXUIFieldAttribute)attr).ChangeNeutralDisplayName(displayName);
					((PXUIFieldAttribute) attr).IsLocalizable = true;
					break;
				}
			}
		}
		/// <summary>Sets the localized display name of the field with the specified
		/// name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="displayName">The new display name.</param>
		public static void SetDisplayNameLocalized(PXCache cache, string fieldName, string displayName)
		{
			cache.SetAltered(fieldName, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					string msgprefix;
					string newDisplayName = PXMessages.Localize(displayName, out msgprefix);
					((PXUIFieldAttribute)attr).DisplayName = newDisplayName;
					((PXUIFieldAttribute)attr).IsLocalizable = false;
					break;
				}
			}
		}
		/// <summary>Sets the <tt>Required</tt> property for the specified field
		/// for all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="required">The value assigned to the <tt>Required</tt>
		/// property.</param>
		public static void SetRequired<Field>(PXCache cache, bool required)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			cache.GetAttributes<Field>().OfType<PXUIFieldAttribute>().ForEach(attr => attr._Required = required);
		}

		/// <summary>Sets the <tt>Required</tt> property for the field with the
		/// specified name for all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXUIField</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="required">The value assigned to the <tt>Required</tt>
		/// property.</param>
		public static void SetRequired(PXCache cache, string name, bool required)
		{
			cache.SetAltered(name, true);
			foreach (PXUIFieldAttribute attr in cache.GetAttributes(name).OfType<PXUIFieldAttribute>())
			{
				attr._Required = required;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item && e.Row != null)
			{
				if (e.Exception != null)
				{
					if (_RestoredValue != null && e.NewValue == _RestoredValue)
					{
						e.Cancel = true;
						return;
					}
					if (!String.IsNullOrEmpty(_ErrorText) && _ErrorLevel != PXErrorLevel.Warning && _ErrorLevel != PXErrorLevel.RowWarning && _ErrorLevel != PXErrorLevel.Undefined)
					{
						if (e.Exception is PXSetPropertyKeepPreviousException)
						{
							e.Cancel = true;
							return;
						}
						if (e.Exception is PXRowPersistingException)
						{
							return;
						}
					}
					_ErrorValue = e.NewValue;
					_ErrorText = e.Exception.Message;
					_ErrorLevel = PXErrorLevel.Error;
					if (e.Exception is PXSetPropertyException)
					{
						_ErrorLevel = ((PXSetPropertyException)e.Exception).ErrorLevel;
						_ErrorText = ((PXSetPropertyException)e.Exception).MessageNoPrefix;
						if (((PXSetPropertyException)e.Exception).ErrorValue != null)
						{
							_ErrorValue = ((PXSetPropertyException)e.Exception).ErrorValue;
						}
					}

					e.Cancel = e.Exception is PXSetPropertyException && (_ErrorHandling == PXErrorHandling.Always || Visible && _ErrorHandling == PXErrorHandling.WhenVisible);

					int fid = _ErrorText.IndexOf("{0}", StringComparison.OrdinalIgnoreCase);
					if (fid >= 0)
					{
						_ErrorText = _ErrorText.Remove(fid, 3).Insert(fid, _DisplayName ?? _FieldName);
						if (e.Exception is PXOverridableException)
						{
							((PXOverridableException)e.Exception).SetMessage(_ErrorText);
						}
					}

					if (_DisplayName != null && _FieldName != _DisplayName)
					{
						fid = _ErrorText.IndexOf(_FieldName, StringComparison.OrdinalIgnoreCase);
						if (fid >= 0)
						{
							_ErrorText = _ErrorText.Remove(fid, _FieldName.Length).Insert(fid, _DisplayName);

							if (e.Exception is PXOverridableException)
							{
								((PXOverridableException)e.Exception).SetMessage(_ErrorText);
							}
						}
					}
				}
				else
				{
					_ErrorValue = null;
					_ErrorText = null;
					_ErrorLevel = PXErrorLevel.Undefined;
					e.Cancel = true;
				}

				if (MapErrorTo != null)
				{
					foreach (var uiFieldAttribute in sender.GetAttributes(e.Row, MapErrorTo.Name).OfType<PXUIFieldAttribute>())
					{
						uiFieldAttribute._ErrorText = _ErrorText;
						uiFieldAttribute._ErrorLevel = _ErrorLevel;
						uiFieldAttribute._ErrorValue = sender.GetValue(e.Row, MapErrorTo.Name);
					}
				}
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
				!String.IsNullOrEmpty(_ErrorText) && 
				_ErrorLevel != PXErrorLevel.Warning && _ErrorLevel != PXErrorLevel.RowWarning &&
				_ErrorLevel != PXErrorLevel.RowInfo)
			{
				string prefix;
				switch (e.Operation)
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
				throw new PXOuterException(GetErrors(sender, e.Row), sender.Graph.GetType(), e.Row,
					ErrorMessages.RecordRaisedErrors, prefix, GetItemName(sender));
			}
		}

		/// <summary>Returns the user-friendly name of the specified cache object.
		/// The name is set using the <see
		/// cref="PXCacheNameAttribute">PXCacheName</see> attribute.</summary>
		/// <param name="cache">The cache object the method is applied to.</param>
		public static string GetItemName(PXCache sender)
		{
			var entityType = sender.GetItemType();
			object[] cachename = entityType.GetCustomAttributes(typeof(PXCacheNameAttribute), false);
			if (cachename != null)
				foreach (PXCacheNameAttribute att in cachename)
				{
					var localName = att.GetName();
					if (!string.IsNullOrEmpty(localName)) return localName;
				}
			return entityType.Name;
		}

		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{

			var state = e.ReturnState as PXFieldState;
			if (!_ViewRights)
			{
				if (!e.ExternalCall)
				{
					e.ReturnValue = null;
				}
				else
				{
					if (state != null)
					{
						state.Visibility = PXUIVisibility.HiddenByAccessRights;
					}
				}
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				if (!String.IsNullOrEmpty(_ErrorText) && _ErrorLevel == PXErrorLevel.Error)
				{
					e.ReturnValue = _ErrorValue;
				}
				var visibility = state?.Visibility == PXUIVisibility.HiddenByAccessRights
					? PXUIVisibility.HiddenByAccessRights
					: _ViewRights 
						? _Visibility 
						: PXUIVisibility.Invisible;
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, sender.Keys.Contains(_FieldName), null,
					_Required == null ? (e.Row == null && !_Enabled ? -3 : 0) : (_Required == true ? 3 : -3),
					null, null, null, _FieldName, null, _DisplayName, _ErrorText, _ErrorLevel, _Enabled, Visible, _ReadOnly,
				visibility, null, null, null);
			}
		}
		/// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!_EnableRights && e.ExternalCall)
			{
				object valpending = sender.GetValuePending(e.Row, _FieldName);
				object valexisting = sender.GetValue(e.Row, _FieldOrdinal);
				if (e.NewValue == valpending && valpending != valexisting
					&& (valexisting != null || !sender.Keys.Contains(_FieldName)))
				{
					if (_ViewRights)
					{
						throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.NotEnoughRights, _FieldName));
					}
					else
					{
						_RestoredValue = valexisting;
						e.NewValue = _RestoredValue;
					}
				}
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			PXAccess.Secure(sender, this);

			TryLocalize(sender);
		}

		private class FieldSourceType
		{
			public bool IsSet { get; set; }
			public UIFieldSourceType SourceType { get; set; }
		}
		private FieldSourceType _FieldSourceType = null;

		/// <exclude/>
		public UIFieldSourceType GetFieldSourceType(PXCache fieldCache)
		{
			UIFieldSourceType sourceType = UIFieldSourceType.Undefined;

			if (WebConfig.EnablePageOpenOptimizations && this._FieldSourceType.IsSet)
			{
				return this._FieldSourceType.SourceType;
			}

			if (fieldCache.Graph != null && FieldOrdinal == -1)
			{
				sourceType = _FieldName.Contains(Localizers.PXUIFieldLocalizer.AUTOMATION_BUTTON_SYMBOL) ? UIFieldSourceType.AutomationButtonName : UIFieldSourceType.ActionName;
			}
			else
			{
				Type classType = fieldCache.GetItemType();
				PropertyInfo prop = classType.GetProperty(_FieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

				if (prop != null)
				{
					sourceType = UIFieldSourceType.DacFieldName;
				}
				else
				{
					Type extention = PXPageRipper.GetExtentionWithProperty(fieldCache.GetExtensionTypes(), fieldCache.GetItemType(), FieldName);

					if (extention != null)
					{
						sourceType = UIFieldSourceType.CacheExtensionFieldName;
					}
					else if (classType.BaseType != null)
					{
						prop = classType.BaseType.GetProperty(_FieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
						if (prop != null)
						{
							sourceType = UIFieldSourceType.ParentDacFieldName;
						}
					}
				}
			}

			if (WebConfig.EnablePageOpenOptimizations && !this._FieldSourceType.IsSet)
			{
				this._FieldSourceType.SourceType = sourceType;
				this._FieldSourceType.IsSet = true;
			}

			return sourceType;
		}

		protected void TryLocalize(PXCache sender)
		{
			if (!IsLocalizable)
				return; 

			if (ResourceCollectingManager.IsStringCollecting)
				{
					PXPageRipper.RipUIField(this, sender, GetFieldSourceType(sender), CollectResourceSettings.Resource);
			}
			else
			{
				PXLocalizerRepository.UIFieldLocalizer.Localize(this, _BqlTable.FullName, sender, GetFieldSourceType(sender));
			}
		}
		#endregion
	}
	#endregion

	#region PXExtraKeyAttribute
	/// <summary>Indicates that the field implements a relationship between
	/// two tables in a projection. The use of this attribute enables update
	/// of the referenced table on update of the projection.</summary>
	/// <remarks>You can place the attribute on the field declaration in the DAC
	/// that represents a <see cref="PXProjectionAttribute">projection</see>.
	/// The attribute is required when the projection combines data from
	/// joined tables and more than one table needs to be updated on update of
	/// the projection. In this case the attribute should be placed on all
	/// fields that implement the relationship between the main and the joined
	/// tables.</remarks>
	/// <example>
	/// The following example shows the declaration of a projection that can
	/// update data in two tables.
	/// Note that the <tt>Select</tt> commands retrieves data from two tables,
	/// <tt>CRCampaignMembers</tt> and <tt>Contact</tt>. To make the
	/// projection updatable, you set the <tt>Persistent</tt> property to
	/// <tt>true</tt>. The projection field that implements relationship between the
	/// tables is marked with the <tt>PXExtraKey</tt> attribute.
	/// <code>
	/// // Projection declaration
	/// [PXProjection(
	/// typeof(
	///     Select2&lt;CRCampaignMembers,
	///         RightJoin&lt;Contact,
	///             On&lt;Contact.contactID, Equal&lt;CRCampaignMembers.contactID&gt;&gt;&gt;&gt;
	/// ),
	/// Persistent = true)]
	/// [Serializable]
	/// public partial class SelCampaignMembers : CRCampaignMembers, IPXSelectable
	/// {
	///     ...
	///     // The field connecting the current DAC with the Contact DAC
	///     [PXDBInt(BqlField = typeof(Contact.contactID))]
	///     [PXExtraKey]
	///     public virtual int? ContactContactID { get; set; }
	///     ...
	/// }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXExtraKeyAttribute : PXEventSubscriberAttribute, IPXCommandPreparingSubscriber
	{
		#region Implementation
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			e.IsRestriction = true;
		}
		#endregion
	}
	#endregion

	#region PXUnboundKeyAttribute
	/// <summary>Marks the property as a key one.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class PXUnboundKeyAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region Initialization
		/// <exclude />
		public override void CacheAttached(PXCache sender)
		{
			sender.Keys.Add(FieldName);
		}
		#endregion
		/// <exclude />
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, true, null, null, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
				}
		#endregion

	#region PXStringAttribute
	/// <summary>Indicates a DAC field of <tt>string</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>
	/// <para>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</para>
	/// <para>It is possible to specify the maximum length and input
	/// validation mask for the string.</para>
	/// <para>You can modify the <tt>Length</tt> and <tt>InputMask</tt>
	/// properties at run time by calling the static methods.</para>
	/// </remarks>
	/// <example>
	/// The attribute below defines an unbound field taking as a value Unicode
	/// strings of 5 uppercase characters that are strictly aphabetical
	/// letters.
	/// <code>
	/// [PXString(5, IsUnicode = true, InputMask = "&gt;LLLLL")]
	/// public virtual String FinChargeCuryID { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXStringAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _Length = -1;
		protected string _InputMask = null;
		protected bool _IsUnicode = true;
		protected bool _IsFixed = false;
		protected bool _IsKey = false;

		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get { return _IsKey; }
			set { _IsKey = value; }
		}

		/// <summary>Gets the maximum length of the string value. If a string
		/// value exceeds the maximum length, it will be trimmed. If
		/// <tt>IsFixed</tt> is set to <tt>true</tt> and the string length is less
		/// then the maximum, it will be extended with spaces. By default, the
		/// property is &#8211;1, which means that the string length is not
		/// limited.</summary>
		public int Length
		{
			get { return _Length; }
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
		public string InputMask
		{
			get { return _InputMask; }
			set { _InputMask = value; }
		}

		/// <summary>Gets or sets an indication that the string has a fixed
		/// length. This property should be set to <tt>true</tt> if the database
		/// column has a fixed length type (<tt>char</tt> or <tt>nchar</tt>). The
		/// default value is <tt>false</tt>.</summary>
		public bool IsFixed
		{
			get { return _IsFixed; }
			set { _IsFixed = value; }
		}

		/// <summary>Gets or sets an indication that the string consists of
		/// Unicode characters. This property should be set to <tt>true</tt> if
		/// the database column has a Unicode string type (<tt>nchar</tt> or
		/// <tt>nvarchar</tt>). The default value is <tt>false</tt>.</summary>
		public bool IsUnicode
		{
			get { return this._IsUnicode; }
			set { this._IsUnicode = value; }
		}
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance with default parameters.</summary>
		public PXStringAttribute()
		{
		}
		/// <summary>Initializes a new instance with the given maximum length of a
		/// field value.</summary>
		/// <param name="length">The maximum length value assigned to the
		/// <tt>Length</tt> property.</param>
		public PXStringAttribute(int length)
		{
			_Length = length;
		}
		#endregion

		#region Runtime
		private static void setLength(PXStringAttribute attr, int length)
		{
			attr._Length = length;
		}
		/// <summary>Sets the maximum length for the string field with the
		/// specified name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
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
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the maximum length for the specified string
		/// field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
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
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the maximum length for the string field with the
		/// specified name for all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="length">The value that is assigned to the <tt>Length</tt>
		/// property.</param>
		public static void SetLength(PXCache cache, string name, int length)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the maximum length for the specified string field for
		/// all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
		/// <param name="length">The value that is assigned to the <tt>Length</tt>
		/// property.</param>
		public static void SetLength<Field>(PXCache cache, int length)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		/// <summary>Sets the input mask for the string field with the specified
		/// name.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
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
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		/// <summary>Sets the input mask for the specified string field.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
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
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		/// <summary>Sets the input mask for the string field with the specified
		/// name for all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
		/// <param name="name">The field name.</param>
		/// <param name="mask">The value that is assigned to the
		/// <tt>InputMask</tt> property.</param>
		public static void SetInputMask(PXCache cache, string name, string mask)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		/// <summary>Sets the input mask for the specified string field for all
		/// data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXString</tt> type.</param>
		/// <param name="mask">The value that is assigned to the
		/// <tt>InputMask</tt> property.</param>
		public static void SetInputMask<Field>(PXCache cache, string mask)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
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
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, _InputMask, null, null, null, null);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if(e.Expr == null) e.Expr = new Column(_FieldName, _BqlTable);

				e.DataType = _IsFixed ? (_IsUnicode ? PXDbType.NChar : PXDbType.Char) : (_IsUnicode ? PXDbType.NVarChar : PXDbType.VarChar);
				e.DataValue = e.Value;
				if (_Length >= 0)
				{
					e.DataLength = _Length;
				}
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXShortAttribute
	/// <summary>Indicates a DAC field of <tt>short?</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// <code>
	/// [PXShort()]
	/// [PXDefault((short)0)]
	/// [PXUIField(DisplayName = "Overdue Days", Enabled = false)]
	/// public virtual short? OverdueDays { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXShortAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _MinValue = short.MinValue;
		protected int _MaxValue = short.MaxValue;
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				short val;
				if (short.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
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
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(short), null);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.SmallInt;
				e.DataValue = e.Value;
				e.DataLength = 2;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXByteAttribute
	/// <summary>Indicates a DAC field of <tt>byte?</tt> type that is not mapped
	/// to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// The code below shows how the <tt>PXByte</tt> attribute is used in the
	/// definition of a DAC field.
	/// <code>
	/// [PXByte()]
	/// public virtual byte? MyByteField { get; set;}
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXByteAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _MinValue = byte.MinValue;
		protected int _MaxValue = byte.MaxValue;
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				byte val;
				if (byte.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
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
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(byte), null);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.TinyInt;
				e.DataValue = e.Value;
				e.DataLength = 1;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXGuidAttribute
	/// <summary>Indicates a DAC field of <tt>Guid?</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// <code>
	/// [PXGuid]
	/// [PXSelector(typeof(EPEmployee.userID),
	///             SubstituteKey = typeof(EPEmployee.acctCD),
	///             DescriptionField = typeof(EPEmployee.acctName))]
	/// [PXUIField(DisplayName = "Custodian")]
	/// public virtual Guid? Custodian { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXGuidAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				Guid val;
				if (GUID.TryParse((string)e.NewValue, out val))
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
				e.ReturnState = PXGuidState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.UniqueIdentifier;
				e.DataValue = e.Value;
				e.DataLength = 16;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXBoolAttribute
	/// <summary>Indicates a DAC field of <tt>bool?</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// <code>
	/// [PXBool()]
	/// [PXDefault(false)]
	/// public virtual bool? Selected { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXBoolAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			ConvertValue(e);
		}

		/// <summary>
		/// Converts the <tt>e.NewValue</tt> property of the parameter from string
		/// to boolean and sets <tt>e.NewValue</tt> to the converted value.
		/// </summary>
		/// <param name="e">Event arguments of the <tt>FieldUpdating</tt> event.</param>
		public static void ConvertValue(PXFieldUpdatingEventArgs e)
		{
			e.NewValue = ConvertValue(e.NewValue);
		}

		public static object ConvertValue(object newValue)
		{
			if (newValue is string)
			{
				bool val;
				if (bool.TryParse((string)newValue, out val))
				{
					newValue = val;
				}
				else
				{
					string str = newValue as string;
					if (!string.IsNullOrEmpty(str))
						switch (str.Trim())
						{
							case "1":
								newValue = true;
								break;
							case "0":
								newValue = false;
								break;
							default:
								newValue = null;
								break;
						}
					else newValue = null;
				}
			}

			return newValue;
		}

		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), _IsKey, null, -1, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.Bit;
				e.DataValue = e.Value;
				e.DataLength = 1;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion

		#region Runtime
		/// <exclude/>
		public static bool CheckSingleRow(PXCache cache, PXView view, object item, string fieldName)
		{
			bool? val = (bool?)cache.GetValue(item, fieldName);
			if (val == true)
			{
				foreach (object o in cache.Cached)
				{
					bool matching = true;
					foreach (string key in cache.Keys)
						if (!object.Equals(cache.GetValue(o, key), cache.GetValue(item, key)))
						{
							matching = false;
							break;
						}
					if (matching)
						continue;

					val = (bool?)cache.GetValue(o, fieldName);
					if (val == true)
					{
						cache.SetValue(o, fieldName, false);
						cache.Update(o);
					}
				}
				view.RequestRefresh();
				cache.IsDirty = false;
				return true;
			}
			return false;
		}

		/// <exclude/>
		public static bool CheckSingleRow<T>(PXCache cache, PXView view, object item) where T : IBqlField
		{
			return CheckSingleRow(cache, view, item, typeof(T).Name);
		}
		#endregion
	}
	#endregion

	#region PXIntAttribute
	/// <summary>Indicates a DAC field of <tt>int?</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// <code>
	/// [PXInt()]
	/// [PXUIField(DisplayName = "Documents", Visible = true)]
	/// public virtual int? DocCount { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXIntAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _MinValue = int.MinValue;
		protected int _MaxValue = int.MaxValue;
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.Int;
				e.DataValue = e.Value;
				e.DataLength = 4;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXLongAttribute
	/// <summary>Indicates a DAC field of <tt>long?</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// <code>
	/// [PXLong(IsKey = true)]
	/// [PXUIField(DisplayName = "Transaction Num.")]
	/// public virtual Int64? TranID { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXLongAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected Int64 _MinValue = Int64.MinValue;
		protected Int64 _MaxValue = Int64.MaxValue;
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				Int64 val;
				if (Int64.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
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
				e.ReturnState = PXLongState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, typeof(long));
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.BigInt;
				e.DataValue = e.Value;
				e.DataLength = 8;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDateAttribute
	/// <summary>Indicates a DAC field of <tt>DateTime?</tt> type that is not mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field. The field is not bound to a table column.</remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXDate()]
	/// public virtual DateTime? NextEffDate { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected string _InputMask = null;
		protected string _DisplayMask = null;
		protected DateTime? _MinValue;
		protected DateTime? _MaxValue;
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
					_MinValue = DateTime.Parse(value);
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
					_MaxValue = DateTime.Parse(value);
				}
				else
				{
					_MaxValue = null;
				}
			}
		}
		#endregion

		/// <summary>Gets or sets the value that indicates whether the attribute
		/// should convert the time to UTC, using the local time zone. If
		/// <tt>true</tt>, the time is converted.</summary>
		public bool UseTimeZone { get; set; }

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
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
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.DateTime;
				if (UseTimeZone)
				{
					DateTime newDate = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)e.Value, LocaleInfo.GetTimeZone());
					e.DataValue = newDate;
				}
				else e.DataValue = e.Value;
				e.DataLength = 4;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
				sender.Keys.Add(_FieldName);

			if (_MinValue == null)
				_MinValue = new DateTime(1900, 1, 1);

			if (_MaxValue == null)
				_MaxValue = new DateTime(9999, 12, 31);
			}
		#endregion
	}
	#endregion

	#region PXDoubleAttribute
	/// <summary>Indicates a DAC field of <tt>double?</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// <code>
	/// [PXDouble]
	/// [PXUIField(Visible = false)]
	/// public virtual Double? OriginalShift { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDoubleAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected double _MinValue = double.MinValue;
		protected double _MaxValue = double.MaxValue;
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
		public PXDoubleAttribute()
		{
		}
		/// <summary>Initializes a new instance of the attribute with the given
		/// precision. The precision is the number of digits after the comma. If a
		/// user enters a value with greater number of fractional digits, the
		/// value will be rounded.</summary>
		/// <param name="precision">The value to use as the precision.</param>
		public PXDoubleAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		/// <summary>
		/// Sets the precision in the attribute instance that marks the
		/// field with the specified name in a particular data record.
		/// </summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDouble</tt> type.</param>
		/// <param name="data">The data record the method is applied to.</param>
		/// <param name="name">The name of the field that is marked with the
		/// attribute.</param>
		/// <param name="precision">The new precision value.</param>
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <summary>
		/// Sets the precision in the attribute instance that marks the
		/// specified field in a particular data record.
		/// </summary>
		/// <typeparam name="Field">The field that is marked with the attribute.</typeparam>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDouble</tt> type.</param>
		/// <param name="data">The data record the method is applied to.</param>
		/// <param name="precision">The new precision value.</param>
		public static void SetPrecision<Field>(PXCache cache, object data, int precision)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <summary>Sets the precision in the attribute instance that marks the
		/// field with the specified name in all data records in the cache
		/// object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDouble</tt> type.</param>
		/// <param name="name">The name of the field that is marked with the
		/// attribute.</param>
		/// <param name="precision">The new precision value.</param>
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <summary>Sets the precision in the attribute instance that marks the
		/// specified field in all data records in the cache
		/// object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDouble</tt> type.</param>
		/// <param name="precision">The new precision value.</param>
		/// <typeparam name="Field">The field that is marked with the attribute.</typeparam>
		public static void SetPrecision<Field>(PXCache cache, int precision)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				double val;
				if (double.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				e.NewValue = Math.Round((Double)e.NewValue, _Precision);
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDoubleState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.Float;
				e.DataValue = e.Value;
				e.DataLength = 8;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXFloatAttribute
	/// <summary>Indicates a DAC field of <tt>float?</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXFloatAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected float _MinValue = float.MinValue;
		protected float _MaxValue = float.MaxValue;
		protected bool _IsKey = false;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
		/// <summary>Initializes a new instance of the attribute with default
		/// parameters.</summary>
		public PXFloatAttribute()
		{
		}
		/// <summary>Initializes a new instance of the attribute with the given
		/// precision. The precision is the number of digits after the comma. If a
		/// user enters a value with greater number of fractional digits, the
		/// value will be rounded.</summary>
		/// <param name="precision">The value to use as the precision.</param>
		public PXFloatAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		/// <exclude/>
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <exclude/>
		public static void SetPrecision<Field>(PXCache cache, object data, int precision)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <exclude/>
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <exclude/>
		public static void SetPrecision<Field>(PXCache cache, int precision)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				float val;
				if (float.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				e.NewValue = Convert.ToSingle(Math.Round((float)e.NewValue, _Precision));
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFloatState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.Real;
				e.DataValue = e.Value;
				e.DataLength = 4;
			}
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDecimalAttribute
	/// <summary>Indicates a DAC field of <tt>decimal?</tt> type that is not mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field. The field is not bound to a table column.</remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXDecimal(0)]
	/// [PXUIField(DisplayName = "SignBalance")]
	/// public virtual Decimal? SignBalance { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDecimalAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int? _Precision = 2;
		protected decimal _MinValue = decimal.MinValue;
		protected decimal _MaxValue = decimal.MaxValue;
		protected bool _IsKey = false;
		protected Type _Type;
		protected BqlCommand _Select;
		/// <summary>Gets or sets the value that indicates whether the field is a
		/// key field.</summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
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
		/// <summary>Gets or sets the maximum value for the field.</summary>
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
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance with the default precision, which
		/// equals 2.</summary>
		public PXDecimalAttribute()
		{
		}
		/// <summary>Initializes a new instance with the given
		/// precision.</summary>
		public PXDecimalAttribute(int precision)
		{
			_Precision = precision;
		}
		/// <summary>Initializes a new instance with the precision calculated at
		/// runtime using a BQL query.</summary>
		/// <param name="type">A BQL query based on a class derived from
		/// <tt>IBqlSearch</tt> or <tt>IBqlField</tt>. For example, the parameter
		/// can be set to <tt>typeof(Search&lt;...&gt;)</tt>, or
		/// <tt>typeof(Table1.field)</tt>.</param>
		public PXDecimalAttribute(Type type)
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
				_Type = type.DeclaringType;
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
		/// <tt>PXDecimal</tt> type.</param>
		/// <param name="data">The data record the method is applied to.</param>
		/// <param name="name">The name of the field that is be marked with the
		/// attribute.</param>
		/// <param name="precision">The new precision value.</param>
		public static void SetPrecision(PXCache cache, object data, string name, int? precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <summary>Sets the precision in the attribute instance that marks the
		/// specified field in a particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBDecimal</tt> type.</param>
		/// <param name="data">The data record the method is applied to.</param>
		/// <param name="precision">The new precision value.</param>
		public static void SetPrecision<Field>(PXCache cache, object data, int? precision)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
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
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		/// <summary>Sets the precision in the attribute instance that marks the
		/// specified field in all data records in the cache object.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXDBDecimal</tt> type.</param>
		/// <param name="precision">The new precision value.</param>
		public static void SetPrecision<Field>(PXCache cache, int? precision)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				decimal val;
				if (decimal.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				_ensurePrecision(sender, e.Row);
				if (_Precision != null)
				{
					e.NewValue = Math.Round((decimal)e.NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
				}
			}
		}
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				_ensurePrecision(sender, e.Row);
				e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		/// <exclude/>
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				if (e.Expr == null)
					e.Expr = new SQLTree.Column(_FieldName, e.BqlTable);
				e.DataType = PXDbType.Decimal;
				e.DataValue = e.Value;
				e.DataLength = 16;
			}
		}
		protected virtual void _ensurePrecision(PXCache sender, object row)
		{
			if (_Type != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, true);
				object item = null;
				try
				{
					List<object> list = view.SelectMultiBound(new object[] { row });
					if (list.Count > 0) item = list[0];
				}
				catch
				{
				}
				if (item != null)
				{
					int? prec = GetItemPrecision(view, item);
					if (prec != null)
						_Precision = prec;
				}
			}
		}

		protected virtual int? GetItemPrecision(PXView view, object item)
		{
			if (item is PXResult) item = ((PXResult)item)[0];
			return item != null ? (short?)view.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name) : null;
		}
		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXStringListAttribute
	/// <summary>Sets a drop-down list as the input control for a DAC field.
	/// In this control, a user selects from a fixed set of strings or
	/// enters a value manually.</summary>
	/// <remarks>
	/// 	<para>The attribute configures a drop-down list that represents the
	/// DAC field in the user interface. You should provide the list of
	/// possible string values and the list of the corresponding labels in the
	/// attribute constructor.</para>
	/// 	<para>You can reconfigure the drop-down list at run time by calling the
	/// static methods. You can set a different list of values or labels or
	/// extend the list.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="The attribute is added to the DAC field definition as follows." lang="CS">
	/// [PXStringList(
	///     new[] { "N", "P", "I", "F" },
	///     new[] { "New", "Prepared", "Processed", "Partially Processed" }
	/// )]
	/// [PXDefault("N")]
	/// public virtual string Status { get; set; }</code>
	/// 	<code title="Example2" description="The attribute below obtains the list of values from the provided string." groupname="Example" lang="CS">
	/// [PXStringList("Dr.,Miss,Mr,Mrs,Prof.")]
	/// public virtual string TitleOfCourtesy { get; set; }</code>
	/// 	<code title="Example3" description="The attribute below obtains the lists of values and labels from the provided string. The user will select from Import and Export. While the field will be set to I or E." groupname="Example2" lang="CS">
	/// [PXStringList("I;Import,E;Export")]
	/// public virtual string TitleOfCourtesy { get; set; }</code>
	/// 	<code title="Example4" description="The example below demonstrates an invocation of a PXStringListAttribute static method. The method called in the example will set the new lists of values and labels for all data records in the cache object that the Schedule.Cache variable references. The method will assign the lists to the PXStringList attribute instances attached to the ActionName field." groupname="Example3" lang="CS">
	/// List&lt;string&gt; values = new List&lt;string&gt;();
	/// List&lt;string&gt; labels = new List&lt;string&gt;();
	/// ... // Fill the values and labels lists
	/// // Specify as arrays of values and labels of the drop-down list
	/// PXStringListAttribute.SetList&lt;AUSchedule.actionName&gt;(
	///     Schedule.Cache, null, values.ToArray(), labels.ToArray());</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXBaseListAttribute))]
	public class PXStringListAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXLocalizableList
	{
		#region State
		protected string[] _AllowedValues;
		protected string[] _AllowedLabels;
		protected string[] _NeutralAllowedLabels;
		protected bool _ExclusiveValues = true;
		protected string _DatabaseFieldName;
		protected int _ExplicitCnt;
		protected string _Locale;
		/// <summary>Gets or sets the value that indicates whether the values and
		/// labels used by the attribute are localizable.</summary>
		public virtual bool IsLocalizable { get; set; }
		public virtual bool SortByValues { get; set; }
		public virtual bool MultiSelect { get; set; }
		protected int _Length;
		/// <summary>Gets or sets the value that indicates whether a user can
		/// input a value not present in the list of allowed values. If
		/// <tt>true</tt>, it is prohibited. By default, the property is set to
		/// <tt>true</tt>, which means that the user can select only from the
		/// values in the dropdown list.</summary>
		public bool ExclusiveValues
		{
			get
			{
				return _ExclusiveValues;
			}
			set
			{
				_ExclusiveValues = value;
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
					)
				{
					BqlTable = BqlCommand.GetItemType(value);
				}
			}
		}
		/// <summary>Gets the dictionary of allowed value-label pairs.</summary>
		public Dictionary<string, string> ValueLabelDic
		{
			get
			{
				if (_AllowedValues == null || _AllowedLabels == null) return null;
				Dictionary<string, string> result = new Dictionary<string, string>(_AllowedValues.Length);
				for (int index = 0; index < _AllowedValues.Length; index++)
					if (_AllowedValues[index] != null)
						result.Add(_AllowedValues[index], _AllowedLabels[index]);
				return result;
			}
		}

		/// <summary>Retrieves the list of allowed values from cache.</summary>
		/// <param name="cache">The cache object to search for the attributes of <tt>PXStringList</tt> type.</param>
		public string[] GetAllowedValues(PXCache cache)
		{
			if (!_disabledValuesRemoved)
			{
				RemoveDisabledValues(cache.GetType().GetGenericArguments()[0].FullName);
				_disabledValuesRemoved = true;
			}
			return _AllowedValues;
		}

		protected class DBStringProperties
		{
			public bool? _nullable;
			public bool? Nullable
			{
				get
				{
					using (var scope = new PXReaderWriterScope(_sync))
					{
						scope.AcquireReaderLock();
						return _nullable;
					}
				}
			}
			private ReaderWriterLock _sync = new ReaderWriterLock();
			public void Fill(Type table, string field)
			{
				using (var scope = new PXReaderWriterScope(_sync))
				{
					scope.AcquireReaderLock();
					if (_nullable != null)
						return;
					scope.UpgradeToWriterLock();
					if (_nullable != null)
						return;
					if (table == null)
					{
						_nullable = true;
						return;
					}
					try
					{
						var tableHeader = PXDatabase.Provider.GetTableStructure(table.Name);
						if (tableHeader != null)
						{
							var column = tableHeader
								.Columns.FirstOrDefault(c => string.Equals(c.Name, field, StringComparison.OrdinalIgnoreCase));
							if (column != null)
							{
								_nullable = column.IsNullable;
							}
							else
							{
								_nullable = true;
							}
						}
						else
						{
							_nullable = true;
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
						return _nullable != null;
					}

				}
			}
		}
		// because attributes instantiating by copying there will be only one instance for all instances of field.
		/// <exclude/>
		protected DBStringProperties DBProperties { get; private set; }
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance with empty lists of possible values
		/// and labels.</summary>
		public PXStringListAttribute()
		{
			DBProperties = new DBStringProperties();
			IsLocalizable = true;
			_AllowedValues = new string[] { null };
			_AllowedLabels = new string[] { "" };

			CreateNeutralLabels();
		}
		/// <summary>Initializes a new instance with the specified lists of
		/// possible values and corresponding labels. The two lists must be of the same
		/// length.</summary>
		/// <param name="allowedValues">The list of values assigned to the field
		/// when a user selects the corresponding labels.</param>
		/// <param name="allowedLabels">The list of labels displayed in the user
		/// interface when a user expands the control.</param>
		public PXStringListAttribute(string[] allowedValues, string[] allowedLabels)
		{
			DBProperties = new DBStringProperties();
			CheckValuesAndLabels(allowedValues, allowedLabels);
			IsLocalizable = true;
			_AllowedValues = allowedValues;
			_AllowedLabels = allowedLabels;
			CreateNeutralLabels();
		}

		/// <summary>Initializes a new instance with the list of possible values
		/// obtained from the provided string. The string should contain either
		/// values separated by a comma, or value-label pairs where the value and
		/// label are separated by a semicolon and different pairs are separated
		/// by a comma. In the first case, labels are set to value
		/// strings.</summary>
		/// <param name="list">The string that contains the list of values or value-
		/// label pairs.</param>
		/// <example>
		/// In the code pieces below, the attribute obtains the list of values from
		/// the provided string. In the second example, the user will select from <i>Import</i>
		/// and <i>Export</i>. While the field will be set to <i>I</i> or <i>E</i>.
		/// <code>
		/// [PXStringList("Dr.,Miss,Mr,Mrs,Prof.")]
		/// public virtual string TitleOfCourtesy { get; set; }
		/// </code>
		/// <code>
		/// [PXStringList("I;Import,E;Export")]
		/// public virtual string TitleOfCourtesy { get; set; }
		/// </code>
		/// </example>
		public PXStringListAttribute(string list)
		{
			DBProperties = new DBStringProperties();
			IsLocalizable = true;
			string[] items = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			_AllowedValues = new string[items.Length];
			_AllowedLabels = new string[items.Length];
			for (int i = 0; i < items.Length; i++)
			{
				int pos = items[i].IndexOf(';');
				if (pos >= 0)
				{
					_AllowedValues[i] = items[i].Substring(0, pos);
					if (pos + 1 < items[i].Length)
					{
						_AllowedLabels[i] = items[i].Substring(pos + 1);
					}
					else
					{
						_AllowedLabels[i] = _AllowedValues[i];
					}
				}
				else
				{
					_AllowedValues[i] = items[i];
					_AllowedLabels[i] = items[i];
				}
			}
			CreateNeutralLabels();
		}

		/// <summary>Initializes a new instance with the specified list of
		/// tuples of possible values and corresponding labels. </summary>
		/// <param name="valuesToLabels">The list of tuples. In each tuple, 
		/// the first item is a value assigned to the field when a user selects the corresponding label,
		/// the second item is a label displayed in the user interface when a user expands the control.</param>
		protected PXStringListAttribute(params Tuple<string, string>[] valuesToLabels)
			: this(
				valuesToLabels.Select(t => t.Item1).ToArray(),
				valuesToLabels.Select(t => t.Item2).ToArray()) {}

		/// <summary>
		/// Initializes a new instance with the specified list of pairs of possible values and corresponding labels. 
		/// </summary>
		/// <param name="valuesToLabels">
		/// The list of pairs. The first item in each pair is a value assigned to the field when a user selects the corresponding labels.
		/// The second item in each pair is a label displayed in the user interface when a user expands the control.
		/// </param>
		public PXStringListAttribute(params (string Value, string Label)[] valuesToLabels) : 
								this(valuesToLabels.Select(pair => pair.Value).ToArray(),
									 valuesToLabels.Select(pair => pair.Label).ToArray()) { }
		#endregion

		#region Runtime

		/// <summary>Retrieves the localized label from the attribute instance that marks the specified field in a particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of <tt>PXStringList</tt> type.</param>
		/// <param name="row">The data record the method is applied to. If <tt>null</tt>, the method is applied to all data records kept in the cache object.</param>
		/// <typeparam name="TField">The field of the data record.</typeparam>
		public static string GetLocalizedLabel<TField>(PXCache cache, object row)
			where TField : IBqlField
		{
			var pxStringListAttr = cache.GetAttributesReadonly<TField>(row).OfType<PXStringListAttribute>().Single();

			return PXMessages.LocalizeNoPrefix(pxStringListAttr.ValueLabelDic[(string)cache.GetValue<TField>(row)]);
		}

		/// <summary>Retrieves the localized label for the specified value from the attribute instance that marks the specified field in a particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of <tt>PXStringList</tt> type.</param>
		/// <param name="row">The data record the method is applied to. If <tt>null</tt>, the method is applied to all data records kept in the cache object.</param>
		/// <typeparam name="TField">The field of the data record.</typeparam>
		public static string GetLocalizedLabel<TField>(PXCache cache, object row, string value)
			where TField : IBqlField
		{
			var pxStringListAttr = cache.GetAttributesReadonly<TField>(row).OfType<PXStringListAttribute>().Single();

			return PXMessages.LocalizeNoPrefix(pxStringListAttr.ValueLabelDic[value]);
		}

		/// <summary>Sets the value of the <see cref="PX.Data.PXStringListAttribute.IsLocalizable">IsLocalizable</see> property of the attribute instance that marks the specified field in a
		/// particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If <tt>null</tt>, the method is applied to all data records kept in the cache object.</param>
		/// <param name="isLocalizable">A Boolean value that indicates (if set to true) that the attribute labels are localizable.</param>
		/// <typeparam name="Field">The field of the data record.</typeparam>
		public static void SetLocalizable<Field>(PXCache cache, object data, bool isLocalizable)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes<Field>(data).OfType<PXStringListAttribute>();
			foreach (PXStringListAttribute attr in attributes)
			{
				attr.IsLocalizable = isLocalizable;
			}
		}

		private static void CheckValuesAndLabels(string[] values, string [] labels)
		{
			if ((values == null && labels != null) ||
				(values != null && labels == null) ||
				(values != null && labels != null && values.Length != labels.Length))
			{
				throw new PXArgumentException("allowedLabels", ErrorMessages.IncorrectValueArrayLength);
			}
		}

		/// <summary>Assigns the possible values and labels from the specified
		/// attribute instance to the attribute instance that marks the specified field in a
		/// particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="listSource">The attribute instance from which the lists
		/// of possible values and labels are obtained.</param>
		/// <typeparam name="Field">The field of the data record.</typeparam>
		public static void SetList<Field>(PXCache cache, object data, PXStringListAttribute listSource)
				where Field : IBqlField
		{
			SetList<Field>(cache, data, listSource._AllowedValues, listSource._AllowedLabels);
		}

		/// <summary>Assigns the specified lists of possible values and labels
		/// to the attribute instance that marks the specified field in a
		/// particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="allowedValues">The new list of values.</param>
		/// <param name="allowedLabels">The new list of labels.</param>
		/// <typeparam name="Field">The field of the data record.</typeparam>
		/// <example>
		/// The code below shows how to modify the lists of values and labels
		/// of a drop-down control at run time.
		/// The method sets the new lists of values and labels for all data
		/// records in the cache object that the <tt>Schedule.Cache</tt> variable references. The
		/// method assigns the lists to the <tt>PXStringList</tt> attribute instances
		/// attached to the <tt>ActionName</tt> field.
		/// <code title="" description="" lang="CS">
		/// List&lt;string&gt; values = new List&lt;string&gt;();
		/// List&lt;string&gt; labels = new List&lt;string&gt;();
		/// // Fill the values and labels lists
		/// // Specify as arrays of values and labels of the drop-down list
		/// PXStringListAttribute.SetList&lt;AUSchedule.actionName&gt;(
		///     Schedule.Cache, null, values.ToArray(), labels.ToArray());</code></example>
		public static void SetList<Field>(PXCache cache, object data, string[] allowedValues, string[] allowedLabels)
			where Field : IBqlField
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes<Field>(data).OfType<PXStringListAttribute>();
			SetListInternal(attributes, allowedValues, allowedLabels, cache);
		}

		/// <summary>Assigns the specified list of tuples of possible values and labels to the attribute
		/// instance that marks the field with the specified name in a particular
		/// data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="valuesToLabels">The new list of pairs. In this list, 
		/// the first item is a value assigned to the field when a user selects the corresponding labels, 
		/// the second item is a label displayed in the user interface when a user expands the control.</param>
		/// <typeparam name="Field">The field of the data record.</typeparam>
		public static void SetList<Field>(PXCache cache, object data, params Tuple<string, string>[] valuesToLabels)
			where Field : IBqlField
		{
			SetList<Field>(cache, data, valuesToLabels.Select(t => t.Item1).ToArray(), valuesToLabels.Select(t => t.Item2).ToArray());
		}

		/// <summary>
		/// Assigns the specified list of pairs of possible values and labels to the attribute instance that marks the field with the specified name in a particular data record.
		/// </summary>
		/// <param name="cache">The cache object to search for the attributes of <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. 
		/// If <tt>null</tt>, the method is applied to all data records kept in the cache object.</param>
		/// <param name="valuesToLabels">
		/// The new list of pairs. The first item in each pair is a value assigned to the field when a user selects the corresponding labels. 
		/// The second item in each pair is a label displayed in the user interface when a user expands the control.
		/// </param>
		/// <typeparam name="Field">The field of the data record.</typeparam>
		/// <code title="" description="" lang="CS">
		/// PXStringListAttribute.SetList&lt;Dac.field>(cache, null,
		///     (Value: Val1, Label: "Value 1"), (Value: Val2, Label: "Value 2"));</code></example>
		public static void SetList<Field>(PXCache cache, object data, params (string Value, string Label)[] valuesToLabels)
			where Field : IBqlField
		{
			SetList<Field>(cache, data, valuesToLabels.Select(pair => pair.Value).ToArray(), 
										valuesToLabels.Select(pair => pair.Label).ToArray());
		}

		/// <summary>Assigns the specified lists of possible values and labels to the attribute
		/// instance that marks the field with the specified name in a particular
		/// data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="field">The name of the field that is be marked with the
		/// attribute.</param>
		/// <param name="allowedValues">The new list of values.</param>
		/// <param name="allowedLabels">The new list of labels.</param>
		public static void SetList(PXCache cache, object data, string field, string[] allowedValues, string[] allowedLabels)
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered(field, true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes(data, field).OfType<PXStringListAttribute>();
			SetListInternal(attributes, allowedValues, allowedLabels, cache);
		}

		/// <summary>Assigns the possible values and labels from the specified
		/// attribute instance to the attribute instance that marks the field with the
		/// specified name in a particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="field">The name of the field that is marked with the
		/// attribute.</param>
		/// <param name="listSource">The attribute instance from which the lists
		/// of possible values and labels are obtained.</param>
		public static void SetList(PXCache cache, object data, string field, PXStringListAttribute listSource)
		{
			SetList(cache, data, field, listSource._AllowedValues, listSource._AllowedLabels);
		}

		/// <summary>Assigns the specified list of tuples of possible values and labels to the attribute
		/// instance that marks the field with the specified name in a particular
		/// data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="field">The name of the field that is marked with the
		/// attribute.</param>
		/// <param name="valuesToLabels">The new list of pairs. In each pair, 
		/// the first item is a value assigned to the field when a user selects the corresponding labels, 
		/// the second item is a label displayed in the user interface when a user expands the control.</param>
		public static void SetList(PXCache cache, object data, string field, params Tuple<string, string>[] valuesToLabels)
		{
			SetList(cache, data, field, valuesToLabels.Select(t => t.Item1).ToArray(), valuesToLabels.Select(t => t.Item2).ToArray());
		}

		/// <summary>
		/// Assigns the specified list of pairs of possible values and labels to the attribute instance that marks the field with the specified name in a particular data record.
		/// </summary>
		/// <param name="cache">The cache object to search for the attributes of <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. 
		/// If <tt>null</tt>, the method is applied to all data records kept in the cache object.</param>
		/// <param name="field">The name of the field that is marked with the attribute.</param>
		/// <param name="valuesToLabels">
		/// The new list of pairs. In each pair, the first item is a value assigned to the field when a user selects the corresponding labels, 
		/// the second item is a label displayed in the user interface when a user expands the control.
		/// </param>
		public static void SetList(PXCache cache, object data, string field, params (string Value, string Label)[] valuesToLabels)
		{
			SetList(cache, data, field, valuesToLabels.Select(pair => pair.Value).ToArray(),
										valuesToLabels.Select(pair => pair.Label).ToArray());
		}

		protected static void SetListInternal(IEnumerable<PXStringListAttribute> attributes, string[] allowedValues, string[] allowedLabels, PXCache cache)
		{
			foreach (PXStringListAttribute attr in attributes)
			{
				attr._AllowedValues = (string[]) allowedValues?.Clone();
				attr._AllowedLabels = (string[]) allowedLabels?.Clone();
				attr._NeutralAllowedLabels = null;
				attr.CreateNeutralLabels();

				if (attr._AllowedLabels != null)
				{
					attr.TryLocalize(cache);
				}
			}
		}

		/// <summary>
		/// In the attribute instance that marks the specified field in a particular data record, extends the lists of possible values and labels with the specified pairs of possible values and labels.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If <tt>null</tt>, the method is applied to all data records kept in the cache object.</param>
		/// <param name="valuesToLabels">
		/// The list of pairs that are appended to the existing list. In each pair, the first item is a value assigned to the field when a user selects the corresponding labels,
		/// the second item is a label displayed in the user interface when a user expands the control.
		/// </param>
		/// <typeparam name="Field">The field of the data record.</typeparam>
		public static void AppendList<Field>(PXCache cache, object data, params (string Value, string Label)[] valuesToLabels)
		where Field : IBqlField
		{
			AppendList<Field>(cache, data, valuesToLabels.Select(pair => pair.Value).ToArray(), 
										   valuesToLabels.Select(pair => pair.Label).ToArray());
		}

		/// <summary>In the
		/// attribute instance that marks the specified field in a particular data
		/// record, extends the lists of possible values and labels with the specified lists of possible values and labels.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="allowedValues">The list of values that is appended to the
		/// existing list of values.</param>
		/// <param name="allowedLabels">The list of labels that is appended to the
		/// existing list of labels.</param>
		/// <typeparam name="Field">The field of the data record.</typeparam>
		public static void AppendList<Field>(PXCache cache, object data, string[] allowedValues, string[] allowedLabels)
			where Field : IBqlField
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes<Field>(data).OfType<PXStringListAttribute>();
			AppendListInternal(attributes, allowedValues, allowedLabels, cache);
		}

		/// <summary>In the
		/// attribute instance that marks the field with the specified name in a
		/// particular data record, extends the lists of possible values and labels with the specified lists of possible values and labels.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXStringList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="allowedValues">The list of values that is appended to the
		/// existing list of values.</param>
		/// <param name="allowedLabels">The list of labels that is appended to the
		/// existing list of labels.</param>
		public static void AppendList(PXCache cache, object data, string field, string[] allowedValues, string[] allowedLabels)
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered(field, true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes(data, field).OfType<PXStringListAttribute>();
			AppendListInternal(attributes, allowedValues, allowedLabels, cache);
		}

		private static void AppendListInternal(IEnumerable<PXStringListAttribute> attributes, string[] allowedValues, string[] allowedLabels, PXCache cache)
		{
			foreach (PXStringListAttribute attr in attributes)
			{
				if (allowedValues == null)
				{
					attr._AllowedValues = null;
					attr._AllowedLabels = null;
					attr._NeutralAllowedLabels = null;
				}
				else
				{
					if (attr._AllowedValues == null)
					{
						attr._AllowedValues = allowedValues;
						attr._AllowedLabels = allowedLabels;
						attr._NeutralAllowedLabels = null;
						attr.CreateNeutralLabels();
					}
					else
					{
						int destIndex = attr._AllowedValues.Length;

						Array.Resize(ref attr._AllowedValues, attr._AllowedValues.Length + allowedValues.Length);
						Array.Copy(allowedValues, 0, attr._AllowedValues, destIndex, allowedValues.Length);

						Array.Resize(ref attr._AllowedLabels, attr._AllowedValues.Length);
						Array.Copy(allowedLabels, 0, attr._AllowedLabels, destIndex, allowedLabels.Length);

						Array.Resize(ref attr._NeutralAllowedLabels, attr._AllowedValues.Length);
						Array.Copy(allowedLabels, 0, attr._NeutralAllowedLabels, destIndex, allowedLabels.Length);
					}

					attr.TryLocalize(cache);
				}
			}
		}
		#endregion

		private bool _disabledValuesRemoved = false;

		#region Implementation
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string currentLocale = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
				if (_Locale != null && !String.Equals(_Locale, currentLocale))
				{
					_Locale = currentLocale;
					TryLocalize(sender);
				}
				if (!_disabledValuesRemoved)
				{
					RemoveDisabledValues(sender.GetType().GetGenericArguments()[0].FullName);
					_disabledValuesRemoved = true;
				}
                
                string[] values = _AllowedValues;
				if (_ExplicitCnt > 0 && sender.Graph.AutomationView == null && !PXGraph.ProxyIsActive && sender.Graph.GetType() != typeof(PXGraph))
				{
					Array.Resize<string>(ref values, values.Length - _ExplicitCnt);
				}
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, values, _AllowedLabels, _ExclusiveValues, null, _NeutralAllowedLabels);
				((PXStringState)e.ReturnState).MultiSelect = MultiSelect;
				if (!DBProperties.IsSet)
				{
					DBProperties.Fill(_BqlTable, _DatabaseFieldName);
				}
				// if can`t read properties - ignoring.
				if (DBProperties.IsSet && DBProperties.Nullable == true)
				{
					((PXStringState)e.ReturnState).EmptyPossible = true;
				}
            }
		}

		/// <exclude/>
		public virtual void OrderByCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.External) == PXDBOperation.External && e.Value == null && _AllowedValues.Length > 0)
			{
				var allowedValues = _AllowedValues;
				try
				{
					_AllowedValues = new string[0];
					PXCommandPreparingEventArgs.FieldDescription descr;
					sender.RaiseCommandPreparing(_FieldName, e.Row, e.Value, e.Operation, e.Table, out descr);
					if (descr != null && descr.Expr != null)
					{
						var fieldExpr = descr.Expr;
						if (sender.BqlSelect != null && fieldExpr is Column c && !c.Name.Equals(_FieldName, StringComparison.OrdinalIgnoreCase))
						{
							// in case of PXProjection with different field name
							fieldExpr = c.Duplicate();
							((Column)fieldExpr).Name = _FieldName;
						}
						e.Cancel = true;
						SQLSwitch swexp = new SQLSwitch();
						var sqlDialect = e.SqlDialect;
						// prepare parameters to use a common "switch" function (from the set of functions used by reports)
						for (int i = 0; i < allowedValues.Length; i++)
						{
							var value = allowedValues[i];
							SQLExpression whenexp = null;
							if (value == null) {
								whenexp = fieldExpr?.IsNull();
							}
							else {
								whenexp = fieldExpr.EQ(new SQLConst(value));
							}

							swexp.Case(whenexp, new SQLConst(_AllowedLabels[i]));
						}
						e.Expr = swexp;
						e.BqlTable = descr.BqlTable;
					}
				}
				finally
				{
					_AllowedValues = allowedValues;
				}
			}
		}
		#endregion

		#region Initialization
		/// <exclude />
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (_DatabaseFieldName == null)
			{
				_DatabaseFieldName = _FieldName;
			}
			if (_AllowedLabels != null)
			{
				string[] allowedLabels = _AllowedLabels;
				_AllowedLabels = new string[allowedLabels.Length];
				Array.Copy(allowedLabels, 0, _AllowedLabels, 0, allowedLabels.Length);
			}
			_ExplicitCnt = PXAutomation.AppendCombos(BqlTable, _DatabaseFieldName, ref _AllowedValues, ref _AllowedLabels);

            // if old automation is active - we do not need update values from new workflow
            if (_ExplicitCnt == 0 )
                AUWorkflowEngine.ApplyCombos(sender.Graph, BqlTable, _DatabaseFieldName, ref _AllowedValues, ref _AllowedLabels);

			AppendNeutral();
			_Locale = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
			TryLocalize(sender);
			if (SortByValues == false)
			sender.CommandPreparingEvents[_FieldName.ToLower()] += OrderByCommandPreparing;
			if (MultiSelect)
			{
				foreach (PXDBStringAttribute attr in sender.GetAttributesReadonly(_FieldName).Where(attr => attr is PXDBStringAttribute).Cast<PXDBStringAttribute>())
				{
					_Length = attr.Length;
				}
				if (_Length > 0)
				{
					sender.FieldUpdatingEvents[_FieldName.ToLower()] += MultiSelectFieldUpdating;
				}
			}
		}
		protected void MultiSelectFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string && ((string)e.NewValue).TrimEnd().Length > _Length)
			{
				throw new PXSetPropertyException(ErrorMessages.StringLengthExceeded, _FieldName);
			}
		}
		#endregion

		internal void RemoveDisabledValues(string cacheName)
		{
			if (_AllowedLabels != null && _AllowedValues != null && _BqlTable != null && _DatabaseFieldName != null)
			{
				var pairs =
					_AllowedValues.Zip(_AllowedLabels, (v, l) => new Tuple<string, string>(v, l))
					.Where(t => !PXAccess.IsStringListValueDisabled(cacheName, _DatabaseFieldName, t.Item1));
				_AllowedValues = pairs.Select(p => p.Item1).ToArray();
				_AllowedLabels = pairs.Select(p => p.Item2).ToArray();
			}
		}

		private void AppendNeutral()
		{
			if (_AllowedLabels != null && (_NeutralAllowedLabels == null || _NeutralAllowedLabels.Length != _AllowedLabels.Length))
			{
				_NeutralAllowedLabels = new string[_AllowedLabels.Length];
				_AllowedLabels.CopyTo(_NeutralAllowedLabels, 0);
			}
		}

		protected virtual void TryLocalize(PXCache sender)
		{
			if (IsLocalizable)
			{
				if (ResourceCollectingManager.IsStringCollecting)
				{
						PXPageRipper.RipList(FieldName, sender, _NeutralAllowedLabels, CollectResourceSettings.Resource);
					}
				else
				{
					if (!PXInvariantCultureScope.IsSet())
					{
					var manager = PX.Common.PXContext.GetSlot<PXDictionaryManager>();
					var allowedLabels = sender.Graph.Prototype.Memoize(delegate
					{
					PXLocalizerRepository.ListLocalizer.Localize(FieldName, sender, _NeutralAllowedLabels, _AllowedLabels);
						return _AllowedLabels;

					}, sender.GetItemType(), this.FieldName, System.Threading.Thread.CurrentThread.CurrentCulture.Name, _NeutralAllowedLabels.GetHashCodeOfSequence(), manager == null ? 0 : manager.GetHashCode());

					_AllowedLabels = allowedLabels;
					}
				}
			}
		}

		protected virtual void RipDynamicLabels(string[] dynamicAllowedLabels, PXCache sender)
		{
			if (IsLocalizable)
			{
				if (ResourceCollectingManager.IsStringCollecting)
				{
					PXPageRipper.RipList(FieldName, sender, dynamicAllowedLabels, CollectResourceSettings.Resource);
				}
			}
		}

		private void CreateNeutralLabels()
		{
			if (_AllowedLabels != null && _AllowedLabels.Length > 0 && _NeutralAllowedLabels == null)
			{
				_NeutralAllowedLabels = new string[_AllowedLabels.Length];
				_AllowedLabels.CopyTo(_NeutralAllowedLabels, 0);
			}
		}

		/// <summary>Pairs the value with its label.</summary>
		/// <exclude />
		/// <param name="value">The value.</param>
		/// <param name="label">The label.</param>
		protected static Tuple<string, string> Pair(string value, string label) => Tuple.Create(value, label);
	}
	#endregion

	#region PXImagesListAttribute
	/// <summary>
	/// Sets a drop-down list as the input control for a DAC field.
	/// In this control, a user selects a value from a fixed set of strings.
	/// Every possible value is accompanied by an image in the drop-down list.
	/// </summary>
	public class PXImagesListAttribute : PXStringListAttribute
	{
		#region State
		protected string[] _AllowedImages;
		/// 
		public override bool IsLocalizable { get { return true; } }
		#endregion

		#region Ctor
		/// 
		public PXImagesListAttribute()
		{
		}
		/// <summary>
		/// Creates a drop-down list with images displayed for each item.
		/// </summary>
		/// <param name="allowedValues">Specifies possible values to select.</param>
		/// <param name="allowedLabels">Specifies the text labels of the list items.</param>
		/// <param name="allowedImages">Specifies the images for the list items as the members
		/// of the <tt>PX.Web.UI.Sprite</tt> class.</param>
		public PXImagesListAttribute(string[] allowedValues, string[] allowedLabels, string[] allowedImages)
			: base(allowedValues, allowedLabels)
		{
			if (allowedValues.Length != allowedImages.Length)
				throw new PXArgumentException(nameof(allowedImages), ErrorMessages.IncorrectValueArrayLength);
			_AllowedImages = allowedImages;
		}
		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			PXStringState state = e.ReturnState as PXStringState;
			if (state != null && _AllowedImages != null) state.AllowedImages = _AllowedImages;
		}
		#endregion
	}
	#endregion

	#region PXAutomationMenuAttribute
	/// <exclude/>
	[PXDBString]
	[PXUIField(DisplayName = "Action")]
	[PXDefault]
	[PXStringList(IsLocalizable = false)]
	public class PXAutomationMenuAttribute : PXAggregateAttribute, IPXRowSelectedSubscriber, IPXFieldSelectingSubscriber
	{
		#region Ctor
		public PXAutomationMenuAttribute()
			: base()
		{
		}
		#endregion

		#region State
		protected List<string> _AllowedValues;
		/// <summary>Get, set.</summary>
		public string DisplayName
		{
			get
			{
				return this.GetAttribute<PXUIFieldAttribute>().DisplayName;
			}
			set
			{
				this.GetAttribute<PXUIFieldAttribute>().DisplayName = value;
			}
		}

		/// <summary>Get, set.</summary>
		public bool Visible
		{
			get
			{
				return this.GetAttribute<PXUIFieldAttribute>().Visible;
			}
			set
			{
				this.GetAttribute<PXUIFieldAttribute>().Visible = value;
			}
		}
		#endregion

		#region Implementation
		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			string val = sender.GetValue(e.Row, this._FieldName) as string;
			if (String.IsNullOrEmpty(val))
				sender.SetValue(e.Row, this._FieldName, GetDefaultValue());
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string val = e.ReturnValue as string;
			if (!String.IsNullOrEmpty(val) && val != Undefined && _AllowedValues != null && !val.Contains('$'))
			{
				val = val + "$";
				string ret = _AllowedValues.FirstOrDefault(s => !String.IsNullOrEmpty(s) && s.StartsWith(val));
				if (ret != null)
				{
					e.ReturnValue = ret;
				}
			}
		}

		private string GetDefaultValue()
		{
			return _AllowedValues == null || _AllowedValues.Count > 1 ? Undefined : _AllowedValues[0];
		}
		#endregion

		/// <exclude/>
		private class AutomationActionMenuItem
		{
			/// <exclude/>
			public class Comparer : IEqualityComparer<AutomationActionMenuItem>
			{
				public bool Equals(AutomationActionMenuItem x, AutomationActionMenuItem y)
				{
					if (x == null && y == null)
					{
						return true;
					}

					if (x == null || y == null)
					{
						return false;
					}

					return (String.Equals(x.Menu, y.Menu, StringComparison.OrdinalIgnoreCase)) &&
						(x.Fills.Count() == y.Fills.Count()) &&
						!(x.Fills.Except(y.Fills, new Fill.Comparer()).Any() || y.Fills.Except(x.Fills, new Fill.Comparer()).Any());
				}

				public int GetHashCode(AutomationActionMenuItem obj)
				{
					var comparer = new Fill.Comparer();
					return obj.Fills.OrderBy(f => f.Name).Select(
						f => string.Concat(f.Name, f.Value, f.Delayed, f.Relative, f.Ignore))
						.JoinToString("").GetHashCode() ^ obj.Menu.GetHashCode();
				}
			}

			public readonly PXProcessingStep ParentStep;
			public readonly PXProcessingAction ParentAction;
			public readonly string Menu;
			public readonly IEnumerable<Fill> Fills;

			public AutomationActionMenuItem(PXProcessingStep parentStep, PXProcessingAction parentAction, string menu, IEnumerable<Fill> fills)
			{
				ParentStep = parentStep;
				ParentAction = parentAction;
				Menu = menu;
				Fills = fills;
			}
		}

		#region Initialization

		private string GetLabel(PXCache sender, IEnumerable<AutomationActionMenuItem> itemGroup)
		{
			string menu = itemGroup.First().Menu;
			var info = itemGroup.SelectMany(item => GraphHelper.GetActions(item.ParentStep.GraphName, item.ParentStep.CacheName));
			var action = info.FirstOrDefault(_ => String.Equals(_.Name, menu, StringComparison.OrdinalIgnoreCase));
			var label = string.Empty;
			if (action != null)
			{
				label = action.DisplayName;
			}
			else
			{
				label = menu;
				PXLocalizerRepository.AutomationLocalizer.Localize(ref label, menu, sender);
			}
			return label;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			List<string> values = new List<string>();
			List<string> labels = new List<string>();
			PXProcessingStep[] steps = PXAutomation.GetProcessingSteps(sender.Graph);

			List<PXProcessingStep> secured = new List<PXProcessingStep>();
			string screenID = null;
			string graphName = null;
			Type cacheType = null;
			PXCacheRights rights;
			List<string> disabled = null;
			List<string> invisible = null;
			foreach (PXProcessingStep s in steps)
			{
				if (!String.Equals(s.ScreenID, screenID, StringComparison.InvariantCultureIgnoreCase)
					|| !String.Equals(s.GraphName, graphName, StringComparison.OrdinalIgnoreCase)
					|| cacheType == null
					|| !String.Equals(cacheType.FullName, s.CacheName, StringComparison.OrdinalIgnoreCase))
				{
					screenID = s.ScreenID;
					graphName = s.GraphName;
					cacheType = System.Web.Compilation.PXBuildManager.GetType(s.CacheName, false);
					if (!String.IsNullOrEmpty(screenID) && !String.IsNullOrEmpty(graphName) && cacheType != null)
					{
						PXAccess.GetRights(screenID, graphName, cacheType, out rights, out invisible, out disabled);
					}
				}
				if ((invisible == null || invisible.Count == 0) && (disabled == null || disabled.Count == 0))
				{
					secured.Add(s);
				}
				else
				{
					List<PXProcessingAction> actions = new List<PXProcessingAction>();
					foreach (PXProcessingAction a in s.Actions)
					{
						if (String.IsNullOrEmpty(a.Name))
						{
							continue;
						}
						if (a.Menus == null || a.Menus.Length == 0)
						{
							if ((disabled == null || !disabled.Contains(a.Name, StringComparer.InvariantCultureIgnoreCase))
								&& (invisible == null || !invisible.Contains(a.Name, StringComparer.InvariantCultureIgnoreCase)))
							{
								actions.Add(a);
							}
						}
						else
						{
							List<MenuItem> menus = new List<MenuItem>();
							foreach (MenuItem m in a.Menus)
							{
								if (String.IsNullOrEmpty(m.Menu))
								{
									continue;
								}
								if ((disabled == null || !disabled.Contains(m.Menu + "@" + a.Name, StringComparer.InvariantCultureIgnoreCase))
									&& (invisible == null || !invisible.Contains(m.Menu + "@" + a.Name, StringComparer.InvariantCultureIgnoreCase)))
								{
									menus.Add(m);
								}
							}
							if (menus.Count > 0)
							{
								actions.Add(new PXProcessingAction(a.Name, menus.ToArray()));
							}
						}
					}
					if (actions.Count > 0)
					{
						secured.Add(new PXProcessingStep(s.ScreenID, s.Name, s.GraphName, s.CacheName, actions.ToArray(), s.Description));
					}
				}
			}
			steps = secured.ToArray();

			var comparer = new AutomationActionMenuItem.Comparer();
			var menuGroups = steps.SelectMany(
				step => step.Actions.SelectMany(
					action => action.Menus.Select(
						menuItem => new AutomationActionMenuItem(step, action, menuItem.Menu, menuItem.Fills))))
				.GroupBy(comparer.GetHashCode)
				.GroupBy(g => g.First().Menu);

			foreach (var menuGroup in menuGroups)
			{
				bool firstProcessed = false;
				foreach (var menuItem in menuGroup.OrderByDescending(m => m.Count()))
				{
					if (!firstProcessed)
					{
						labels.Add(GetLabel(sender, menuItem));
						values.Add(menuItem.First().Menu + "$" + menuItem.Select(i => i.ParentStep.Name).JoinToString("$"));
						firstProcessed = true;
					}
					else
					{
						labels.Add(GetLabel(sender, menuItem) + " - " + menuItem.Select(i => TryLocalize(i.ParentStep.Description, sender)).JoinToString(", "));
						values.Add(menuItem.First().Menu + "$" + menuItem.Select(i => i.ParentStep.Name).JoinToString("$"));
					}
				}
			}

			if (values.Count == 0)
				return;
			if (values.Count > 1)
			{
				labels.Insert(0, SM.Messages.GetLocal(Undefined));
				values.Insert(0, Undefined);
			}
			PXStringListAttribute.SetList(sender, null, this._FieldName, values.ToArray(), labels.ToArray());

			this._AllowedValues = values;

			this.GetAttribute<PXDefaultAttribute>().Constant = GetDefaultValue();
		}

		private string TryLocalize(string message, PXCache sender)
		{
			string result = String.Empty;
			if (ResourceCollectingManager.IsStringCollecting)
			{
				PXPageRipper.RipDescription(message, sender);
			}
			else
			{
				result = PXCommonLocalizer.Localize(message, sender.GetItemType().FullName);
			}
			return result;
		}
		#endregion

		public const string Undefined = "<SELECT>";

		/// <exclude/>
		public class undefinded : PX.Data.BQL.BqlString.Constant<undefinded>
		{
			public undefinded()
				: base(Undefined)
			{
			}
		}
	}
	#endregion

	#region PXIntListAttribute
	/// <summary>Sets a drop-down list as the input control for a DAC field.
	/// In this control, a user selects from a fixed set of integer values, which are
	/// represented in the drop-down list by string labels.</summary>
	/// <remarks>
	/// <para>The attribute configures a drop-down list that represents the
	/// DAC field in the user interface. In the attribute constructor, you should provide the list of
	/// possible integer values and the list of the corresponding labels.</para>
	/// <para>You can reset the lists of values and labels at run time by
	/// calling the <tt>SetList&lt;&gt;</tt> static method.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXIntList(
	///     new int[] { 0, 1 },
	///     new string[] { "Apply Credit Hold", "Release Credit Hold" })]
	/// public virtual int? Action { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXBaseListAttribute))]
	public class PXIntListAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXLocalizableList
	{
		#region State
		protected int[] _AllowedValues;
		protected string[] _AllowedLabels;
		protected string[] _NeutralAllowedLabels;
		protected bool _ExclusiveValues = true;
		protected string _Locale;
		/// <summary>Gets or sets the value that indicates whether the labels used
		/// by the attribute are localizable.</summary>
		public virtual bool IsLocalizable { get; set; }

		/// <summary>Gets or sets the value that indicates whether a user can
		/// input a value not available in the list of allowed values. By default, the property is set to
		/// <tt>true</tt>, which means that the user can select only from the
		/// values in the drop-down list.</summary>
		public bool ExclusiveValues
		{
			get
			{
				return _ExclusiveValues;
			}
			set
			{
				_ExclusiveValues = value;
			}
		}
		/// <summary>Gets the dictionary of allowed value-label pairs.</summary>
		public Dictionary<int, string> ValueLabelDic
		{
			get
			{
				Dictionary<int, string> result = new Dictionary<int, string>(_AllowedValues.Length);
				for (int index = 0; index < _AllowedValues.Length; index++)
					result.Add(_AllowedValues[index], _AllowedLabels[index]);
				return result;
			}
		}
		protected class DBIntProperties
		{
			public bool? _nullable;
			public bool? Nullable
			{
				get
				{
					using (var scope = new PXReaderWriterScope(_sync))
					{
						scope.AcquireReaderLock();
						return _nullable;
					}
				}
			}
			private ReaderWriterLock _sync = new ReaderWriterLock();
			public void Fill(Type table, string field)
			{
				using (var scope = new PXReaderWriterScope(_sync))
				{
					scope.AcquireReaderLock();
					if (_nullable != null)
						return;
					scope.UpgradeToWriterLock();
					if (_nullable != null)
						return;
					if (table == null)
					{
						_nullable = true;
						return;
					}
					try
					{
						var tableHeader = PXDatabase.Provider.GetTableStructure(table.Name);
						if (tableHeader != null)
						{
							var column = tableHeader
								.Columns.FirstOrDefault(c => string.Equals(c.Name, field, StringComparison.OrdinalIgnoreCase));
							if (column != null)
							{
								_nullable = column.IsNullable;
							}
							else
							{
								_nullable = true;
							}
						}
						else
						{
							_nullable = true;
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
						return _nullable != null;
					}

				}
			}
		}
		// because attributes instantiating by copying there will be only one instance for all instances of field.
		/// <exclude/>
		protected DBIntProperties DBProperties { get; private set; }
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance with empty lists of possible values
		/// and labels.</summary>
		public PXIntListAttribute()
		{
			DBProperties = new DBIntProperties();
			IsLocalizable = true;
		}
		/// <summary>Initializes a new instance with the list of possible values
		/// obtained from the provided string. The string should contain either
		/// values separated by a comma, or value-label pairs where the value and
		/// label are separated by a semicolon and different pairs are separated
		/// by a comma. In the first case, labels are set to value strings. Values
		/// are converted from strings to integers.</summary>
		/// <param name="list">The string that contains the list of values separated
		/// by comma.</param>
		public PXIntListAttribute(string list)
			: this()
		{
			string[] items = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			_AllowedValues = new int[items.Length];
			_AllowedLabels = new string[items.Length];
			for (int i = 0; i < items.Length; i++)
			{
				int pos = items[i].IndexOf(';');
				if (pos >= 0)
				{
					_AllowedValues[i] = int.Parse(items[i].Substring(0, pos));
					if (pos + 1 < items[i].Length)
					{
						_AllowedLabels[i] = items[i].Substring(pos + 1);
					}
					else
					{
						_AllowedLabels[i] = items[i].Substring(0, pos);
					}
				}
				else
				{
					_AllowedValues[i] = int.Parse(items[i]);
					_AllowedLabels[i] = items[i];
				}
			}

			CreateNeutralLabels();
		}
		/// <summary>Initializes a new instance with the specified lists of
		/// possible values and corresponding labels. The two lists must be of the same
		/// length.</summary>
		/// <param name="allowedValues">The list of values assigned to the field
		/// when a user selects the corresponding labels..</param>
		/// <param name="allowedLabels">The list of labels displayed in the user
		/// interface when a user expands the control.</param>
		public PXIntListAttribute(int[] allowedValues, string[] allowedLabels)
			: this()
		{
			if (allowedValues.Length != allowedLabels.Length)
				throw new PXArgumentException(nameof(allowedValues), ErrorMessages.IncorrectValueArrayLength);
			_AllowedValues = allowedValues;
			_AllowedLabels = allowedLabels;

			CreateNeutralLabels();
		}
		/// <summary>Initializes a new instance, extracting the list of possible
		/// values and labels from the provided enumeration. Uses the enumeration
		/// values as possible values and enumeration values names as the
		/// corresponding labels.</summary>
		/// <param name="enumType">The <tt>enum</tt> type that defines the lists
		/// of possible values and labels.</param>
		public PXIntListAttribute(Type enumType, bool byteValues = false)
			: this()
		{
			Array values = Enum.GetValues(enumType);

			_AllowedValues = byteValues
				? Array.ConvertAll((byte[])values, b => (int)b)
				: (int[])values;

			_AllowedLabels = Enum.GetNames(enumType);

			CreateNeutralLabels();
		}

		public PXIntListAttribute(Type enumType, string[] allowedLabels)
			: this()
		{
			_AllowedValues = (int[])Enum.GetValues(enumType);
			if (_AllowedValues.Length != allowedLabels.Length)
				throw new PXArgumentException(nameof(allowedLabels), ErrorMessages.IncorrectLabelArrayLength);
			_AllowedLabels = allowedLabels;

			CreateNeutralLabels();
		}

		/// <summary>Initializes a new instance with the specified collection of tuples
		/// of possible values and corresponding labels.</summary>
		/// <param name="valuesToLabels">The list of pairs where 
		/// first item is a value assigned to the field when a user selects the corresponding labels, and
		/// second item is a label displayed in the user interface when a user expands the control.</param>
		protected PXIntListAttribute(params Tuple<int, string>[] valuesToLabels)
			: this(
				valuesToLabels.Select(t => t.Item1).ToArray(),
				valuesToLabels.Select(t => t.Item2).ToArray()) {}

		/// <summary>
		/// Initializes a new instance with the specified list of pairs of allowed values and corresponding labels. 
		/// </summary>
		/// <param name="valuesToLabels">
		/// The list of pairs where first item is a value assigned to the field when a user selects the corresponding labels, 
		/// and second item is a label displayed in the user interface when a user expands the control.
		/// </param>
		public PXIntListAttribute(params (int Value, string Label)[] valuesToLabels) : 
							 this(valuesToLabels.Select(pair => pair.Value).ToArray(),
								  valuesToLabels.Select(pair => pair.Label).ToArray())
		{ }
		#endregion

		#region Runtime
		/// <summary>Assigns the provided lists of possible values and labels
		/// to the attribute instance that marks the specified field in a
		/// particular data record.</summary>
		/// <param name="cache">The cache object to search for the attributes of
		/// <tt>PXIntList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records kept in the
		/// cache object.</param>
		/// <param name="allowedValues">The new list of values.</param>
		/// <param name="allowedLabels">The new list of labels.</param>
		public static void SetList<Field>(PXCache cache, object data, int[] allowedValues, string[] allowedLabels)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXIntListAttribute)
				{
					PXIntListAttribute intList = (PXIntListAttribute)attr;

					intList._AllowedValues = (int[]) allowedValues?.Clone();
					intList._AllowedLabels = (string[]) allowedLabels?.Clone();
					intList._NeutralAllowedLabels = null;
					intList.CreateNeutralLabels();
					intList.TryLocalize(cache);
				}
			}
		}

		/// <summary>
		/// Assigns the list of pairs of possible values and labels to the attribute instance 
		/// that marks the specified field in a particular data record.
		/// </summary>
		/// <param name="cache">The cache object to search for the attributes of <tt>PXIntList</tt> type.</param>
		/// <param name="data">The data record the method is applied to. 
		/// If <tt>null</tt>, the method is applied to all data records kept in the cache object.</param>
		/// <param name="valuesToLabels">
		/// The new list of pairs. The first item in each pair is a value assigned to the field when a user selects the corresponding label. 
		/// The second item in each pair is a label displayed in the user interface when a user expands the control.
		/// </param>
		public static void SetList<Field>(PXCache cache, object data, params (int Value, string Label)[] valuesToLabels)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}

			foreach (PXIntListAttribute intListAttribute in cache.GetAttributes<Field>(data).OfType<PXIntListAttribute>())
			{
				intListAttribute._AllowedValues = valuesToLabels.Select(pair => pair.Value).ToArray();
				intListAttribute._AllowedLabels = valuesToLabels.Select(pair => pair.Label).ToArray();
				intListAttribute._NeutralAllowedLabels = null;

				intListAttribute.CreateNeutralLabels();
				intListAttribute.TryLocalize(cache);
			}
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			if (_AllowedLabels != null)
			{
				string[] allowedLabels = _AllowedLabels;
				_AllowedLabels = new string[allowedLabels.Length];
				Array.Copy(allowedLabels, 0, _AllowedLabels, 0, allowedLabels.Length);
			}
			AppendNeutral();
			_Locale = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
			TryLocalize(sender);
		}

		private void AppendNeutral()
		{
			if (_NeutralAllowedLabels != null && _AllowedLabels != null && _NeutralAllowedLabels.Length != _AllowedLabels.Length)
			{
				_NeutralAllowedLabels = new string[_AllowedLabels.Length];
				_AllowedLabels.CopyTo(_NeutralAllowedLabels, 0);
			}
		}

		private void TryLocalize(PXCache sender)
		{
			if (IsLocalizable)
			{
				if (ResourceCollectingManager.IsStringCollecting)
				{
						PXPageRipper.RipList(FieldName, sender, _NeutralAllowedLabels, CollectResourceSettings.Resource);
					}
				else
				{
					PXLocalizerRepository.ListLocalizer.Localize(FieldName, sender, _NeutralAllowedLabels, _AllowedLabels);
				}
			}
		}

		private void CreateNeutralLabels()
		{
			if (_AllowedLabels != null && _AllowedLabels.Length > 0 && _NeutralAllowedLabels == null)
			{
				_NeutralAllowedLabels = new string[_AllowedLabels.Length];
				_AllowedLabels.CopyTo(_NeutralAllowedLabels, 0);
			}
		}

		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string currentLocale = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
				if (_Locale != null && !String.Equals(_Locale, currentLocale))
				{
					_Locale = currentLocale;
					TryLocalize(sender);
				}
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, null, null, null, null, _AllowedValues, _AllowedLabels, _ExclusiveValues, null, null, _NeutralAllowedLabels);
				if (!DBProperties.IsSet)
				{
					DBProperties.Fill(_BqlTable, _FieldName);
				}
				// if can`t read properties - ignoring.
				if (DBProperties.IsSet && DBProperties.Nullable == true)
				{
					((PXIntState)e.ReturnState).EmptyPossible = true;
				}
			}
		}
		#endregion

		/// <summary>
		/// Pairs value to its label
		/// </summary>
		protected static Tuple<int, string> Pair(int value, string label) => Tuple.Create(value, label);
	}
	#endregion

	#region PXDisplaySelectorAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal class PXDisplaySelectorAttribute : PXSelectorAttribute
	{
		public PXDisplaySelectorAttribute(Type type)
			: base(type)
		{
		}

		public PXDisplaySelectorAttribute(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

		public override void ReadDeletedFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

		public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			e.NewValue = PXMacroManager.Default.TryResolveExt(e.NewValue, sender, FieldName, e.Row);
			if (!e.Cancel && e.NewValue != null)
			{
				object item = null;
				GlobalDictionary dict = null;
				if (_CacheGlobal)
				{
					dict = GetGlobalCache();
					lock ((dict).SyncRoot)
					{
						GlobalDictionary.CacheValue cacheValue;
						if (dict.TryGetValue(e.NewValue, out cacheValue))
						{
							if (cacheValue.IsDeleted)
							{
								PXView select = sender.Graph.TypedViews.GetView(_NaturalSelect, !_DirtyRead);
								object ret = select.Cache.GetValue(cacheValue.Item, ForeignField.Name);
								e.NewValue = ret;
								return;
							}
							item = cacheValue.Item;
						}
					}
				}
				if (item == null)
				{
					PXView select = sender.Graph.TypedViews.GetView(_NaturalSelect, !_DirtyRead);
					object[] pars = MakeParameters(e.NewValue, true);
					Func<object> readItem = () => SelectSingleBound(@select, new[] {e.Row}, _CacheGlobal ? new[] {e.NewValue} : pars);
					item = sender._InvokeSelectorGetter(e.Row, _FieldName, @select, _CacheGlobal ? new[] { e.NewValue } : pars, true) ?? readItem();
					if (item != null)
					{
						object ret = select.Cache.GetValue(item, ForeignField.Name);
						if (select.Cache.Keys.Count <= 1)
							cacheOnReadItem(dict, @select.Cache, item);
						e.NewValue = ret;
					}
					else
					{
						var found = false;
						if (PXDatabase.IsReadDeletedSupported(_BqlType))
						{
							using (new PXReadDeletedScope())
							{
								item = readItem();
								if (item != null)
								{
									object ret = select.Cache.GetValue(item, ForeignField.Name);
									if (select.Cache.Keys.Count <= 1)
										cacheOnReadItem(dict, @select.Cache, item, true);
									e.NewValue = ret;
									found = true;
								}
							}
						}
						if (!found) throwNoItem(hasRestrictedAccess(sender, _NaturalSelect, e.Row), true, e.NewValue);
					}
				}
				else
				{
					PXCache cache = sender.Graph.Caches[_Type];
					object p = e.NewValue;
					e.NewValue = cache.GetValue(item, ForeignField.Name);
					if (e.NewValue == null && !cache.GetItemType().IsAssignableFrom(item.GetType()))
					{
						PXView select = sender.Graph.TypedViews.GetView(_NaturalSelect, !_DirtyRead);
						item = sender._InvokeSelectorGetter(e.Row, _FieldName, select, new[] { p }, true) ?? SelectSingleBound(select, new object[] { e.Row }, p);
						if (item != null)
						{
							e.NewValue = select.Cache.GetValue(item, ForeignField.Name);
						}
					}
				}
			}
		}
	}
	#endregion

	#region PXRestrictorAttribute
	/// <summary>Adds a restriction to a BQL command that selects data for a
	/// lookup control and displays the error message when the value entered
	/// does not fit the restriction.</summary>
	/// <remarks>
	/// <para>The attribute is used on DAC fields represented by lookup
	/// controls in the user interface. For example, such fields can have the
	/// <see cref="PXSelectorAttribute">PXSelector</see> attribute attached
	/// to them. The attribute adds the <tt>Where&lt;&gt;</tt> clause to the
	/// BQL command that selects data for the control. As a result, the
	/// control lists the data records that satisfy the BQL command and the
	/// new restriction. If the user enters a value that is not in the list,
	/// the error message configured by the attribute is displayed.</para>
	/// <para>A typical example of attribute's usage is specifiying condition
	/// that checks whether a referenced data record is active. This condition
	/// could be specified in the <tt>PXSelector</tt> attribute. But in this
	/// case, if an active data record once selected through the lookup
	/// control becomes inactive, saving the data record that includes this
	/// lookup field will result in an error. Adding the condition through
	/// <tt>PXRestrictor</tt> attribute prevents this error. The lookup field
	/// can still hold a reference to the inactive data record. However, the
	/// new value can be selected only among the active data records.</para>
	/// </remarks>
	/// <example>
	/// The code below shows the use of the attribute on a lookup field.
	/// Notice that the message includes <i>{0}</i> that is replaced
	/// with the value of the <tt>TaxCategoryID</tt> field when the error message
	/// is displayed.
	/// <code>
	/// [PXDBString(10, IsUnicode = true)]
	/// [PXUIField(DisplayName = "Tax Category")]
	/// [PXSelector(
	///     typeof(TaxCategory.taxCategoryID),
	///     DescriptionField = typeof(TaxCategory.descr))]
	/// [PXRestrictor(
	///     typeof(Where&lt;TaxCategory.active, Equal&lt;True&gt;&gt;),
	///     "Tax Category '{0}' is inactive",
	///     typeof(TaxCategory.taxCategoryID))]
	/// public virtual string TaxCategoryID { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true)]
	public class PXRestrictorAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
	{
		protected const string PlaceholderSign = "[";
		protected const string ScreenNamePlaceholder = "[ScreenName]";
		protected const string ScreenIDPlaceholder = "[ScreenID]";

		protected string _Message;
		protected Type[] _MsgParams;
		protected Type _Where;

		protected BqlCommand _OriginalCmd;
		protected BqlCommand _AlteredCmd;
		protected List<Type> _AlteredParams = null;
		protected Type _ValueField;

		protected Type _Type;
		protected bool _CacheGlobal;
		protected int _ParsCount;
		protected bool _DirtyRead;
		protected Type _SubstituteKey;
		protected bool _ShowWarning;

		protected int _RestrictLevel;
		protected int _ReplaceLevel = -1;

		private int? _TablesCount;

		public IReadOnlyCollection<IFieldsRelation> AdditionalKeysRelationsArray { get; private set; }

		/// <summary>Gets or sets the value indicating whether the current
		/// <tt>PXRestrictor</tt> attribute should override the inherited
		/// <tt>PXRestrictor</tt> attributes.</summary>
		public bool ReplaceInherited
		{
			get { return _ReplaceLevel >= 0; }
			set
			{
				if (value)
				{
				_ReplaceLevel = 0;
				_RestrictLevel = 1;
			}
		}
		}

		public bool CacheGlobal
		{
			get { return _CacheGlobal; }
			set { _CacheGlobal = value; }
		}

		public string Message 
		{
			get { return _Message; }
			set { _Message = value; }
		}

		/// <summary>Gets or sets the value indicating whether 
		/// warning message will be displayed on a field if the
		/// referenced record does not satisfy <tt>PXRestrictor</tt> 
		/// attribute condition.</summary>
		public bool ShowWarning
		{
			get { return _ShowWarning; }
			set { _ShowWarning = value; }
		}

		public Type RestrictingCondition => _Where;


		/// <summary>
		/// Initializes a new instance of the attribute.
		/// </summary>
		/// <param name="where">The <tt>Where&lt;&gt;</tt> BQL clause used as the
		/// additional restriction for a BQL command.</param>
		/// <param name="message">The error message that is displayed when a value
		/// violating the restriction is entered. The error message can reference the
		/// fields specified in the third parameter, as <i>{0}�{N}</i>. The attribute
		/// will take the values of these fields from the data record whose identifier
		/// was entered as the value of the current field.</param>
		/// <param name="pars">The types of fields that are referenced by the error message.</param>
		public PXRestrictorAttribute(Type where, string message, params Type[] pars)
		{
			if (where == null)
			{
				throw new PXArgumentException(nameof(where), ErrorMessages.ArgumentNullException);
			}
			else if (typeof(IBqlWhere).IsAssignableFrom(where))
			{
				_Where = where;
			}
			else
			{
				throw new PXArgumentException(nameof(where), ErrorMessages.ArgumentException);
			}

			_Message = message;

			if (pars.Any(par => !typeof(IBqlField).IsAssignableFrom(par)))
			{
				throw new PXArgumentException("params", ErrorMessages.ArgumentException);
			}
			_MsgParams = pars;
		}

		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			var attributes = sender.GetAttributesReadonly(_FieldName);
			if (attributes.OfType<PXRestrictorAttribute>().Any(r => r._ReplaceLevel >= this._RestrictLevel))
					return;

			foreach (PXSelectorAttribute selattr in attributes.OfType<PXSelectorAttribute>())
			{
				_OriginalCmd = selattr.PrimarySelect;
				_AlteredCmd = WhereAnd(sender, selattr, _Where);
				_ValueField = selattr.ValueField;
				_ParsCount = selattr.ParsCount;
				_DirtyRead = selattr.DirtyRead;
				_Type = selattr.Type;
				AdditionalKeysRelationsArray = (selattr as PXSelectorAttribute.WithCachingByCompositeKeyAttribute)?.AdditionalKeysRelationsArray ?? Array<IFieldsRelation>.Empty;

				var tablesCount = sender.Graph.Prototype.Memoize(GetTablesCount, sender.GetItemType(), this.FieldName);

				if (tablesCount < 2)
				{
					_CacheGlobal = selattr.CacheGlobal;
				}
				_SubstituteKey = selattr.SubstituteKey;

				AlterCommand(sender);
				break;
			}

			if (_ShowWarning)
			{
				sender.FieldSelectingEvents[_FieldName.ToLower()] += FieldSelecting;
			}

			if (_Message != null && _Message.Contains(PlaceholderSign))
			{
				string screenID = PXContext.GetScreenID();
				PXSiteMapNode node;
				if (!String.IsNullOrEmpty(screenID) && (node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID)) != null && !String.IsNullOrEmpty(node.Title))
				{
					_Message = _Message.Replace(ScreenNamePlaceholder, node.Title).Replace(ScreenIDPlaceholder, screenID);
				}
			}
		}

		private int GetTablesCount()
		{
			int tablesCount = 0;
			if (WebConfig.EnablePageOpenOptimizations)
			{
				if (_TablesCount == null)
					_TablesCount = _OriginalCmd.GetTables().Length;
				tablesCount = this._TablesCount.Value;
			}
			else
			{
				tablesCount = _OriginalCmd.GetTables().Length;
			}

			return tablesCount;
		}

		protected Dictionary<Type, Tuple<BqlCommand, BqlCommand, List<Type>>> _altered = new Dictionary<Type, Tuple<BqlCommand, BqlCommand, List<Type>>>();

		private static ConcurrentDictionary<Tuple<Type, Type, Type>, Func<BqlCommand>> dict = new ConcurrentDictionary<Tuple<Type, Type, Type>, Func<BqlCommand>>();

		/// <exclude />
		protected virtual BqlCommand WhereAnd(PXCache sender, PXSelectorAttribute selattr, Type Where)
		{
			selattr.WhereAnd(sender, Where);

			if (WebConfig.EnablePageOpenOptimizations)
			{
				Func<BqlCommand> factory = null;
				var types = Tuple.Create(_OriginalCmd.GetType(), Where, selattr.ValueField);
				if (!dict.TryGetValue(types, out factory))
				{
					var result = _OriginalCmd.WhereNew(BqlCommand.Compose(typeof(Where<,>), selattr.ValueField, typeof(Equal<>), typeof(Required<>), selattr.ValueField)).WhereAnd(Where);
					var type = result.GetType();
					factory = Expression.Lambda<Func<BqlCommand>>(Expression.New(type)).Compile();
					dict.TryAdd(types, factory);
					return result;
				}
				return factory();
			}

			return _OriginalCmd.WhereNew(BqlCommand.Compose(typeof(Where<,>), selattr.ValueField, typeof(Equal<>), typeof(Required<>), selattr.ValueField)).WhereAnd(Where);
		}

		protected virtual void AlterCommand(PXCache sender)
		{
			if (_AlteredCmd == null) return;

			Type key = typeof(PXGraph);
			if (sender.IsGraphSpecificField(_FieldName))
			{
				key = sender.Graph.GetType();
			}

			Tuple<BqlCommand, BqlCommand, List<Type>> altered;
			lock (((ICollection)_altered).SyncRoot)
			{
				if (_altered.TryGetValue(key, out altered) && _AlteredCmd.GetSelectType() == altered.Item1.GetSelectType())
				{
					_AlteredCmd = altered.Item2;
					_AlteredParams = altered.Item3;
					return;
				}
			}
			PXView view = sender.Graph.TypedViews.GetView(_OriginalCmd, !_DirtyRead);

			Type[] tables = _AlteredCmd.GetTables();
			Type[] bql = BqlCommand.Decompose(_AlteredCmd.GetSelectType());
			IBqlParameter[] parameters = _AlteredCmd.GetParameters();

			int _AlteredParsCount = 0;
			int _ValueParamIndex = 0;
			if (parameters != null)
				foreach (IBqlParameter par in parameters)
				{
					if (par.IsVisible) _AlteredParsCount++;
					if (par.IsVisible && !par.HasDefault && par.GetReferencedType() == _ValueField) _ValueParamIndex = _AlteredParsCount - 1;
				}

			int j = 0;
			_AlteredParams = new List<Type>(new Type[_AlteredParsCount]) { [_ValueParamIndex] = _ValueField };

			for (int i = 0; i < bql.Length; i++)
			{
				if (bql[i] == typeof(Aggregate<>))
				{
					bool hasOrderBy = bql.Skip(i).Contains(typeof(OrderBy<>));
					Type[] dummy;
					if (hasOrderBy)
						dummy = new Type[2] { typeof(BqlNone), typeof(BqlNone) };
					else
						dummy = new Type[1] { typeof(BqlNone) };
					bql = bql.Take(i).Concat(dummy).ToArray();
					break;
				}
				if (typeof(IBqlParameter).IsAssignableFrom(bql[i]) && bql[i].GetGenericTypeDefinition().IsIn(typeof(Optional<>), typeof(Optional2<>), typeof(Required<>)))
				{
					j++;
				}

				if (typeof(IBqlField).IsAssignableFrom(bql[i]) && !typeof(IBqlParameter).IsAssignableFrom(bql[i - 1]))
				{
					Type currentType = BqlCommand.GetItemType(bql[i]);
					bool readonlycache = sender.Graph._ReadonlyCacheCreation;
					try
					{
						sender.Graph._ReadonlyCacheCreation = true;
					if (!(view.CacheGetItemType() == currentType || currentType.IsAssignableFrom(view.CacheGetItemType())))
					{
						if (Array.IndexOf(tables, currentType) > -1)
						{
							_AlteredParams.Insert(j++, bql[i]);
							bql[i] = typeof(Required<>).MakeGenericType(bql[i]);
						}
					}
				}
					finally
					{
						sender.Graph._ReadonlyCacheCreation = readonlycache;
					}
				}
			}

			BqlCommand OriginalAlteredCmd = _AlteredCmd;
			_AlteredCmd = BqlCommand.CreateInstance(BqlCommand.Compose(bql));
			lock (((ICollection)_altered).SyncRoot)
			{
				_altered[key] = new Tuple<BqlCommand, BqlCommand, List<Type>>(OriginalAlteredCmd, _AlteredCmd, _AlteredParams);
			}
		}

		/// <exclude />
		public static object GetItem(PXCache cache, PXRestrictorAttribute attr, object data, object key)
		{
			object row = null;
			PXSelectorAttribute.GlobalDictionary dict = null;
			Byte keysCount = (byte) (1 + attr.AdditionalKeysRelationsArray.Count);
			if (attr._CacheGlobal && key != null)
			{
				dict = PXSelectorAttribute.GlobalDictionary.GetOrCreate(attr._Type, cache.Graph.Caches[attr._Type].BqlTable, keysCount);
				lock ((dict).SyncRoot)
				{
					PXSelectorAttribute.GlobalDictionary.CacheValue cacheValue;
					object cacheKey = keysCount == 1
						? key
						: PXSelectorAttribute.WithCachingByCompositeKeyAttribute.CreateCompositeKey(cache, data, key, attr.AdditionalKeysRelationsArray, false);
					if (dict.TryGetValue(cacheKey, out cacheValue) && !cacheValue.IsDeleted && !(cacheValue.Item is IDictionary))
					{
						row = cacheValue.Item;
					}
				}
			}
			if (row == null && (key == null || key.GetType() == cache.GetFieldType(attr._FieldName)))
			{
				PXView select = cache.Graph.TypedViews.GetView(attr._OriginalCmd, !attr._DirtyRead);
				object[] pars = new object[attr._ParsCount + 1];
				pars[pars.Length - 1] = key;
				object item;
				row = cache._InvokeSelectorGetter(data, attr.FieldName, select, pars, false);
				if (row != null)
				{
					item = PXResult.UnwrapMain(row);
				}
				else
				{
					List<object> rows = select.SelectMultiBound(new object[] { data }, pars);
					if (rows.Count == 0)
					{
						return null;
					}

					item = PXResult.UnwrapMain(row = rows[0]);
				}

				if (attr._CacheGlobal && key != null && select.Cache.GetStatus(item) == PXEntryStatus.Notchanged && !PXDatabase.ReadDeleted && select.Cache.Keys.Count <= keysCount)
				{
					PXSelectorAttribute.CheckIntegrityAndPutGlobal(dict, @select.Cache, ((IBqlSearch)attr._OriginalCmd).GetField().Name, item, key, false);
				}
			}
			return row;
		}

		/// <exclude />
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object value = sender.GetValue(e.Row, _FieldOrdinal);
			PXFieldVerifyingEventArgs args = new PXFieldVerifyingEventArgs(e.Row, value, e.ExternalCall);

			string message = TryVerify(sender, args, false)?.MessageNoPrefix;

			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, _FieldName, null, null, message, message != null ? PXErrorLevel.Warning : PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}

		/// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Verify(sender, e, true);
		}

		/// <exclude/>
		public virtual void Verify(PXCache sender, PXFieldVerifyingEventArgs e, bool IsErrorValueRequired)
		{
			var ex = TryVerify(sender, e, IsErrorValueRequired);
			if (ex != null) throw ex;
		}

		protected virtual PXException TryVerify(PXCache sender, PXFieldVerifyingEventArgs e, Boolean IsErrorValueRequired)
		{
			if (_AlteredCmd == null || _AlteredParams == null || _OriginalCmd == null || _Message == null) return null;

			object item, itemres;
			try
			{
				itemres = item = GetItem(sender, this, e.Row, e.NewValue);
			}
			catch (PXSetPropertyException ex)
			{
				return ex;
			}

			if (item != null)
			{
				object[] pars = new object[_AlteredParams.Count];
				for (int i = 0; i < _AlteredParams.Count; i++)
				{
					if (_AlteredParams[i] != null)
					{
						Type currentType = BqlCommand.GetItemType(_AlteredParams[i]);
						object current = PXResult.Unwrap(item, currentType);
						if (current != null)
						{
							pars[i] = sender.Graph.Caches[currentType].GetValue(current, _AlteredParams[i].Name);
						}
					}
				}

				PXView view = sender.Graph.TypedViews.GetView(_AlteredCmd, !_DirtyRead);
				pars = view.PrepareParameters(new object[] { e.Row }, pars);

				item = PXResult.UnwrapMain(item);

				if (!_AlteredCmd.Meet(view.Cache, item, pars))
				{
					if (_SubstituteKey != null && IsErrorValueRequired)
					{
						object errorValue = e.NewValue;
						sender.RaiseFieldSelecting(_FieldName, e.Row, ref errorValue, false);
						PXFieldState state = errorValue as PXFieldState;
						e.NewValue = state != null ? state.Value : errorValue;
					}

					return new PXSetPropertyException(
						_Message, 
						_MsgParams
							.Select(param => sender.Graph.Caches[BqlCommand.GetItemType(param)].GetStateExt(PXResult.Unwrap(itemres, BqlCommand.GetItemType(param)), param.Name))
							.ToArray());
				}
			}

			return null;
		}
	}
	#endregion

	#region PXFormulaAttribute
	/// <summary>Calculates the value from the child data record fields and
	/// computes the aggregation of such values over all child data
	/// records.</summary>
	/// <remarks>
	/// 	<para>Unlike the <tt>PXFormula</tt> attribute, this attribute does not
	/// assign the computed value to the field the attribute is attached to.
	/// The value is only used for aggregated calculation of the parent data
	/// record field. Hence, you can place this attribute on declaration of
	/// any child DAC field.</para>
	/// 	<para>The <tt>PXParent</tt> attribute must be added to some field of
	/// the child DAC.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="" lang="CS">
	/// [PXUnboundFormula(
	///     typeof(Mult&lt;APAdjust.adjgBalSign, APAdjust.curyAdjgAmt&gt;),
	///     typeof(SumCalc&lt;APPayment.curyApplAmt&gt;))]
	/// public virtual decimal? CuryAdjgAmt { get; set; }</code>
	/// 	<code title="Example2" description="Several UnboundFormula attributes can be placed on the same DAC field definition, as shown in the example below." groupname="Example" lang="CS">
	/// [PXUnboundFormula(
	///     typeof(Switch&lt;
	///         Case&lt;WhereExempt&lt;APTaxTran.taxID&gt;, APTaxTran.curyTaxableAmt&gt;,
	///         decimal0&gt;),
	///     typeof(SumCalc&lt;APInvoice.curyVatExemptTotal&gt;))]
	/// [PXUnboundFormula(
	///     typeof(Switch&lt;
	///         Case&lt;WhereTaxable&lt;APTaxTran.taxID&gt;, APTaxTran.curyTaxableAmt&gt;,
	///         decimal0&gt;),
	///     typeof(SumCalc&lt;APInvoice.curyVatTaxableTotal&gt;))]
	/// public override Decimal? CuryTaxableAmt { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
	public class PXUnboundFormulaAttribute : PXFormulaAttribute
	{
		#region State
		/// <summary>Gets the name of the field the attribute is attached
		/// to.</summary>
		public override string FormulaFieldName
		{
			get
			{
				return null;
			}
		}
		#endregion

		#region Ctor

		/// <summary>
		/// Initializes a new instance that calculates the value of the field
		/// the attribute is atached to and sets an aggregate function to calculate
		/// the value of a field in the parent data record. The aggregation
		/// function is applied to the values calculated by the first parameter
		/// for all child data records.
		/// </summary>
		/// <param name="formulaType">The formula to calculate the field value from
		/// other fields of the same data record; an expression built from BQL functions
		/// such as <tt>Add</tt>, <tt>Sub</tt>, <tt>Mult</tt>, <tt>Div</tt>, <tt>Switch</tt> and other functions.
		/// If null, the aggregation function takes into account the field value
		/// inputted by the user.</param>
		/// <param name="aggregateType">The aggregation formula to calculate the parent
		/// data record field from the child data records fields.</param>
		public PXUnboundFormulaAttribute(Type formulaType, Type aggregateType)
			: base(formulaType, aggregateType)
		{
		}

		protected override void InitDependencies(PXCache sender)
		{
		}

		protected override void InitAggregate(Type aggregateType)
		{
			if (aggregateType != null)
			{
				Type[] args = aggregateType.GetGenericArguments();

				_ParentFieldType = args[0];
				if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
					throw new PXArgumentException(nameof(_ParentFieldType), ErrorMessages.CantGetParentField, _ParentFieldType.Name);

				if (!typeof(IBqlUnboundAggregateCalculator).IsAssignableFrom(aggregateType))
					throw new PXArgumentException("_Aggregate", ErrorMessages.CantFindAggregateType, aggregateType.Name);
				_Aggregate = (IBqlUnboundAggregateCalculator)Activator.CreateInstance(aggregateType);
			}
			else
			{
				_ParentFieldType = null;
				_Aggregate = null;
			}
		}

		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			ISwitch formula = _Formula as ISwitch;
			if (formula != null)
			{
				formula.OuterField = null;
			}
		}
		#endregion

		#region Implementation

		protected override object CalcAggregate(PXCache cache, object row, object oldrow, int digit)
		{
			return ((IBqlUnboundAggregateCalculator)_Aggregate).Calculate(cache, row, oldrow, _Formula, digit);
		}

		protected override object CalcAggregate(PXCache cache, object row, object[] records, int digit)
		{
			return ((IBqlUnboundAggregateCalculator)_Aggregate).Calculate(cache, row, _Formula, records, digit);
		}

		/// <exclude/>
		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_Formula != null)
			{
				EnsureParent(sender, e.Row, null);
				UpdateParent(sender, e.Row, e.OldRow);
				EnsureChildren(sender, e.OldRow);
			}
		}

		/// <exclude/>
		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_Formula != null)
			{
				EnsureParent(sender, e.Row, null);
				UpdateParent(sender, e.Row, null);
			}
		}

		/// <exclude/>
		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_Formula != null)
			{
				UpdateParent(sender, null, e.Row);
				EnsureChildren(sender, e.Row);
			}
		}
		#endregion
	}
	#endregion

	#region PXFormulaAttribute
	/// <summary>Calculates a field from other fields of the same data record
	/// and sets an aggregation formula to calculate a parent data record
	/// field from child data record fields.</summary>
	/// <remarks>
	/// 	<para>The attribute assigns the computed value to the field the
	/// attribute is attached to. The value is also used for aggregated
	/// calculation of the parent data record field (if the aggregate
	/// expression has been specified in the attribute parameter).</para>
	/// 	<para>The <tt>PXParent</tt> attribute must be added to some field of
	/// the child DAC.</para>
	/// 	<para>The attribute can be used on both bound and unbound DAC fields.
	/// For unbound fields, the attribute dynamically adds the <tt>RowSelecting</tt>
	/// event handler which tries to calculate the field value when the data
	/// record is retrieved from the database. For bound fields, the attribute
	/// doesn't calculate the field value when the data record is retrieved
	/// from the database. Also, if the <tt>Persistent</tt> property is set
	/// to <tt>true</tt>, the attribute recalculates the field value on
	/// the <tt>RowPersisted</tt> event (for bound and unbound fields).</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="The attribute below sums two fields and assigns it the field the attribute is attached to. The second example shows a more complex calculation with the Switch class." lang="CS">
	/// [PXFormula(typeof(
	///     Add&lt;SOOrder.curyPremiumFreightAmt, SOOrder.curyFreightAmt&gt;))]
	/// public virtual Decimal? CuryFreightTot { get; set; }</code>
	/// 	<code title="Example2" description="The attribute below performs more complex calculation." groupname="Example" lang="CS">
	/// [PXFormula(typeof(
	///     Switch&lt;
	///         Case&lt;Where&lt;Add&lt;SOOrder.releasedCntr, SOOrder.billedCntr&gt;,
	///                    Equal&lt;short0&gt;&gt;,
	///             SOOrder.curyOrderTotal&gt;,
	///         Add&lt;SOOrder.curyUnbilledOrderTotal, SOOrder.curyFreightTot&gt;&gt;))]
	/// public decimal? CuryDocBal { get; set; }</code>
	/// 	<code title="Example3" description="The attribute below multiplies the TranQty and UnitPrice fields and assigns the result to the ExtPrice field. The attribute also calculates the sum of the computed ExtPrice values over all child DocTransaction data records and assigns the result to the parent's TotalAmt field." groupname="Example2" lang="CS">
	/// [PXUIField(DisplayName = "Line Total", Enabled = false)]
	/// [PXFormula(
	///     typeof(Mult&lt;DocTransaction.tranQty, DocTransaction.unitPrice&gt;),
	///     typeof(SumCalc&lt;Document.totalAmt&gt;))]
	/// public virtual decimal? ExtPrice { get; set; }</code>
	/// 	<code title="Example4" description="A common practice is to disable the input control for a calculated field. In the example above, the control is disabled using the PXUIField attribute.
	/// The attribute below does not provide a formula for calculating the TranQty property. The value inputted by a user is assigned to the field. The attribute only sets the formula to calculate the TotalQty field in the parent data record as the sum of TranQty values over all related child data records." groupname="Example3" lang="CS">
	/// [PXFormula(null, typeof(SumCalc&lt;Document.totalQty&gt;))]
	/// public virtual decimal? TranQty { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
	public class PXFormulaAttribute : PXEventSubscriberAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber, IPXRowDeletedSubscriber, IPXDependsOnFields
	{
		#region State
		protected IBqlCreator _Formula;
		protected Type _ParentFieldType;
		protected object _Aggregate;
		protected HashSet<Type> _Dependencies; 

		/// <summary>Get the name of the field the attribute is attached
		/// to.</summary>
		public virtual string FormulaFieldName
		{
			get
			{
				return _FieldName;
			}
		}

		/// <summary>Gets or sets the BQL query that is used to calculate the
		/// value of the field the attribute is attached to. The value should
		/// derive from <tt>Constant&lt;&gt;</tt>, <tt>IBqlField</tt>, or
		/// <tt>IBqlCreator</tt>.</summary>
		public virtual Type Formula
		{
			get
			{
				return _Formula.GetType();
			}
			set
			{
				_Formula = InitFormula(value);
			}
		}

		/// <summary>Gets or sets the parent data record field the aggregation
		/// result is assigned to. The value should derive from
		/// <tt>IBqlField</tt>.</summary>
		public virtual Type ParentField
		{
			get
			{
				return _ParentFieldType;
			}
			set
			{
				_ParentFieldType = value;
			}
		}

		/// <summary>Gets or sets the BQL query that represents the aggregation
		/// formula used to calculate the parent data record field from the child
		/// data records fields. The value should derive from
		/// <tt>IBqlAggregateCalculator</tt>.</summary>
		public virtual Type Aggregate
		{
			get
			{
				return _Aggregate?.GetType();
			}
			set
			{
				InitAggregate(value);
			}

		}

		protected bool _Persistent = false;
		/// <summary>Gets or sets the value that indicates whether the attribute
		/// recalculates the formula for the child field after a saving of changes
		/// to the database. You may need recalculation if the fields the formula
		/// depends on are updated on the <tt>RowPersisting</tt> event. By
		/// default, the property equals <tt>false</tt>.</summary>
		public virtual bool Persistent
		{
			get { return _Persistent; }
			set { _Persistent = value; }
		}


		protected string _FieldClass = null;
		public virtual string FieldClass
		{
			get
			{
				return _FieldClass;
			}
			set
			{
				_FieldClass = value;
			}
		}

		protected bool IsRestricted
		{
			get
			{
				string[] resricted = PXAccess.Provider.FieldClassRestricted;
				return resricted != null && resricted.Length != 0 && !string.IsNullOrEmpty(_FieldClass) && Array.IndexOf(resricted, _FieldClass) >= 0;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the formula should
		/// not recalculate during defaulting, insertion, dependent
		/// field updating etc.
		/// </summary>
		public bool CancelCalculation
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the formula should
		/// not raise RowUpdated event on updating parent record.
		/// </summary>
		public bool CancelParentUpdate
		{
			get;
			set;
		}

		/// <exclude/>
		public virtual ISet<Type> GetDependencies(PXCache cache)
		{			
			if (_Dependencies == null)
			{
				_Dependencies = new HashSet<Type>();
				if (_Formula != null)
				{
					List<Type> fields = new List<Type>();
					//_Formula.Parse(cache.Graph, null, null, fields, null, null, null);
					var exp = SQLExpression.None();
					_Formula.AppendExpression(ref exp, cache.Graph, new BqlCommandInfo(false) { Fields = fields, BuildExpression = false}, new BqlCommand.Selection());
					_Dependencies.AddRange(fields);					
				}
			}
			return _Dependencies;
		}

		protected ObjectRef<bool> _recursion;
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance that calculates the value of the
		/// field the attribute is atached to, by the provided formula.</summary>
		/// <param name="formulaType">The formula to calculate the field value
		/// from other fields of the same data record. The formula can be an
		/// expression built from BQL functions such as <tt>Add</tt>,
		/// <tt>Sub</tt>, <tt>Mult</tt>, <tt>Div</tt>, <tt>Switch</tt> and <a
		/// href="BQL_Functions_For_Formulas.html">other
		/// functions</a>.</param>
		public PXFormulaAttribute(Type formulaType)
		{
			_Formula = InitFormula(formulaType);
		}

		/// <summary>Initializes a new instance that calculates the value of the
		/// field the attribute is atached to and sets an aggregate function to
		/// calculate the value of a field in the parent data record. The
		/// aggregation function is applied to the values calculated by the first
		/// parameter for all child data records.</summary>
		/// <param name="formulaType">The formula to calculate the field value
		/// from other fields of the same data record. The formula can be an
		/// expression built from BQL functions such as <tt>Add</tt>,
		/// <tt>Sub</tt>, <tt>Mult</tt>, <tt>Div</tt>, <tt>Switch</tt> and <a
		/// href="BQL_Functions_For_Formulas.html">other functions</a>. If
		/// <tt>null</tt>, the aggregation function takes into account the field
		/// value inputted by the user.</param>
		/// <param name="aggregateType">The aggregation formula to calculate the
		/// parent data record field from the child data records fields. Use an <a
		/// href="BQL_Formulas.html">aggregation function</a> such as
		/// <tt>SumCalc</tt>, <tt>CountCalc</tt>, <tt>MinCalc</tt>, and
		/// <tt>MaxCalc</tt>.</param>
		/// <example>
		/// <para>The attribute below multiplies the <tt>TranQty</tt> and
		/// <tt>UnitPrice</tt> fields and assigns the result to the
		/// <tt>ExtPrice</tt> field. The attribute also calculates the sum of the computed
		/// <tt>ExtPrice</tt> values over all child <tt>DocTransaction</tt> data
		/// records and assigns the result to the parent's <tt>TotalAmt</tt> field.
		/// A common practice is to disable the input control for a calculated field. In the example
		/// below, the control is disabled using the <see cref="PXUIFieldAttribute">PXUIField</see>
		/// attribute.</para>
		/// <para>In the second example, the attribute does not provide a formula for calculating
		/// the <tt>TranQty</tt> property. The value inputted by a user is assigned to the field.
		/// The attribute only sets the formula to calculate the <tt>TotalQty</tt> field in the
		/// parent data record as the sum of <tt>TranQty</tt> values over all related child data
		/// records.</para>
		/// <code>
		/// [PXUIField(DisplayName = "Line Total", Enabled = false)]
		/// [PXFormula(
		///     typeof(Mult&lt;DocTransaction.tranQty, DocTransaction.unitPrice&gt;),
		///     typeof(SumCalc&lt;Document.totalAmt&gt;))]
		/// public virtual decimal? ExtPrice { get; set; }
		/// </code>
		/// <code>
		/// [PXFormula(null, typeof(SumCalc&lt;Document.totalQty&gt;))]
		/// public virtual decimal? TranQty { get; set; }
		/// </code>
		/// </example>
		public PXFormulaAttribute(Type formulaType, Type aggregateType)
		{
			_Formula = InitFormula(formulaType);
			InitAggregate(aggregateType);
		}

		/// <exclude/>
		public static IBqlCreator InitFormula(Type formulaType)
		{
			if (formulaType != null)
			{
				Type type = formulaType;
				if (typeof(IBqlField).IsAssignableFrom(formulaType))
				{
					type = typeof(Row<>);
					type = type.MakeGenericType(formulaType);
				}
				else if (typeof(IConstant).IsAssignableFrom(formulaType))
				{
					type = typeof(Const<>);
					type = type.MakeGenericType(formulaType);
				}
				else if (typeof(IBqlWhere).IsAssignableFrom(formulaType))
				{
					type = BqlCommand.MakeGenericType(typeof(Switch<,>), typeof(Case<,>), formulaType, typeof(True), typeof(False));
				}
				else if (typeof(IBqlUnary).IsAssignableFrom(formulaType))
				{
					type = BqlCommand.MakeGenericType(typeof(Switch<,>), typeof(Case<,>), typeof(Where<>), formulaType, typeof(True), typeof(False));
				}
				else if (!typeof(IBqlCreator).IsAssignableFrom(formulaType))
					throw new PXArgumentException(nameof(formulaType), ErrorMessages.CantCreateFormula, formulaType.Name);
				return (IBqlCreator)Activator.CreateInstance(type);
			}
			return null;
		}

		protected virtual void InitAggregate(Type aggregateType)
		{
			if (aggregateType != null)
			{
				Type[] args = aggregateType.GetGenericArguments();

				_ParentFieldType = args[0];
				if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
					throw new PXArgumentException(nameof(_ParentFieldType), ErrorMessages.CantGetParentField, _ParentFieldType.Name);

				if (!typeof(IBqlAggregateCalculator).IsAssignableFrom(aggregateType))
					throw new PXArgumentException("_Aggregate", ErrorMessages.CantFindAggregateType, aggregateType.Name);
				_Aggregate = (IBqlAggregateCalculator)Activator.CreateInstance(aggregateType);
			}
			else
			{
				_ParentFieldType = null;
				_Aggregate = null;
			}
		}
		#endregion

		#region Runtime
		/// <summary>Sets the new aggregation formula in the attribute instances
		/// that mark the specified field, for all data records in the cache
		/// object.</summary>
		/// <param name="sender">The cache object to search for the attributes of
		/// <tt>PXFormula</tt> type.</param>
		/// <param name="aggregateType">The new aggregation formula that will be
		/// used to calculate the parent data record field from the child data
		/// records fields.</param>
		public static void SetAggregate<Field>(PXCache sender, Type aggregateType)
			where Field : IBqlField
		{
			foreach (PXFormulaAttribute attr in sender.GetAttributes<Field>().OfType<PXFormulaAttribute>())
			{
				attr.Aggregate = aggregateType;
			}
		}

		/// <summary>Calculates the fields of the parent data record using the
		/// aggregation formula from the attribute instance that marks the
		/// specified field. The calculation is applied to the child data records
		/// merged with the modifications kept in the session.</summary>
		/// <param name="sender">The cache object to search for the attributes of
		/// <tt>PXFormula</tt> type.</param>
		/// <param name="parent">The parent data record.</param>
		public static void CalcAggregate<Field>(PXCache sender, object parent)
			where Field : IBqlField
		{
			CalcAggregate<Field>(sender, parent, false);
		}

		/// <summary>Calculates the fields of the parent data record using the
		/// aggregation formula from the attribute instance that marks the
		/// specified field. The calculation is applied to the child data records
		/// that are either taken directly from the database or merged with the
		/// modifications kept in the session.</summary>
		/// <param name="sender">The cache object to search for the attributes of
		/// <tt>PXFormula</tt> type.</param>
		/// <param name="parent">The parent data record.</param>
		/// <param name="IsReadOnly">If <tt>true</tt>, the child data records are
		/// not merged with the unsaved modification accessible through the cache
		/// object. Otherwise, the child data records are merged with the
		/// modifications.</param>
		/// <typeparam name="Field">The field marked with the <tt>PXFormula</tt> attribute.</typeparam>
		/// <example>
		/// The code below shows a part of <tt>RowSelected</tt> event handler defined in
		/// a graph.
		/// <code>
		/// protected virtual void APPayment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		/// {
		///     ...
		///     if (e.Row != null && ((APPayment)e.Row).CuryApplAmt == null)
		///     {
		///         // Checking the status of the parent data record
		///         bool IsReadOnly = (cache.GetStatus(e.Row) == PXEntryStatus.Notchanged);
		///         // Executing the formula
		///         PXFormulaAttribute.CalcAggregate&lt;APAdjust.curyAdjgAmt&gt;(Adjustments.Cache, e.Row, IsReadOnly);
		///         // Raising the FieldUpdated event on the parent data record's field
		///         cache.RaiseFieldUpdated&lt;APPayment.curyApplAmt&gt;(e.Row, null);
		///     }
		///     ...
		/// }
		/// </code>
		/// </example>
		public static void CalcAggregate<Field>(PXCache sender, object parent, bool IsReadOnly)
			where Field : IBqlField
		{
			List<PXFormulaAttribute> formulas = sender.GetAttributesReadonly<Field>()
				.OfType<PXFormulaAttribute>()
				.Where(attribute => attribute._Aggregate != null).ToList();

			Type ParentType = parent.GetType();

			foreach (PXView view in sender.GetAttributesReadonly(null)
				.OfType<PXParentAttribute>()
				.Where(parentAttribute => parentAttribute.ParentType == ParentType || ParentType.IsSubclassOf(parentAttribute.ParentType))
				.Select(parentAttribute => parentAttribute.GetChildrenSelect(sender)))
			{
				if (view.IsReadOnly != IsReadOnly)
				{
					view.Clear();
				}
				view.IsReadOnly = IsReadOnly;
				List<object> records = view.SelectMultiBound(new object[] { parent });
					foreach (object row in records)
					{
						sender.RaiseRowSelected(row);
					}

				foreach (PXFormulaAttribute f in formulas)
				{
					f.CalcAggregate(sender, records.ToArray(), parent);
				}
				return;
			}
		}

		public static object Evaluate<Field>(PXCache sender, object row)
			where Field : IBqlField
		{
			PXFormulaAttribute formula = sender.GetAttributesReadonly<Field>()
				.OfType<PXFormulaAttribute>()
				.FirstOrDefault();

			if (formula != null)
			{
				using (new PXConnectionScope())
				{
					PXFieldDefaultingEventArgs de = new PXFieldDefaultingEventArgs(row);
					formula.FormulaDefaulting(sender, de);
					return de.NewValue;
				}
			}
			return null;
		}

		#endregion

		#region Implementation
		protected virtual object EnsureParent(PXCache cache, object Row, object NewRow)
		{
			if (_ParentFieldType == null)
			{
				return null;
			}
			if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
			{
				throw new PXArgumentException(nameof(_ParentFieldType), ErrorMessages.InvalidField, _ParentFieldType.Name);
			}

			Type parentType = BqlCommand.GetItemType(_ParentFieldType);
			object parent = PXParentAttribute.SelectParent(cache, Row, NewRow, parentType);

			if (parent == null)
			{
				PXParentAttribute.CreateParent(cache, Row, parentType);
				return null;
			}
			return parent;
		}

		protected virtual void EnsureChildren(PXCache cache, object Row)
		{
			if (_ParentFieldType == null || !(_Aggregate is ICountCalc))
			{
				return;
			}

			if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
			{
				throw new PXArgumentException(nameof(_ParentFieldType), ErrorMessages.InvalidField, _ParentFieldType.Name);
			}

			Type parentType = BqlCommand.GetItemType(_ParentFieldType);
			PXCache parentcache = cache.Graph.Caches[parentType];

			if (PXParentAttribute.GetParentCreate(cache, parentType))
			{
				object parent = PXParentAttribute.SelectParent(cache, Row, parentType);
				if (parent != null)
				{
					object val = parentcache.GetValue(parent, _ParentFieldType.Name);

					if (val is int && (int)val <= 0 || val is short && (short)val <= 0)
					{
						parentcache.Delete(parent);
					}
				}
			}
		}

		/// <exclude/>
		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			using (new PXPerformanceInfoTimerScope(info => info.TmFormulaCalculations))
			{
			object parent = EnsureParent(sender, e.Row, null);

			UpdateParent(sender, e.Row, e.OldRow, parent);

			EnsureChildren(sender, e.OldRow);
		}
		}

		/// <exclude/>
		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			using (new PXPerformanceInfoTimerScope(info => info.TmFormulaCalculations))
			{
			bool? result = null;
			object value = null;

				if (_Formula != null && _CancelDefaulting != true && !CancelCalculation &&
					sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				BqlFormula.Verify(sender, e.Row, _Formula, ref result, ref value);
				if (value != PXCache.NotSetValue)
				{
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref value);
					sender.SetValue(e.Row, _FieldOrdinal, value);
				}
			}

				if (_Formula == null && _CancelDefaulting != true && !CancelCalculation &&
					sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				foreach (PXFormulaAttribute attribute in sender.GetAttributesReadonly(_FieldName)
					.OfType<PXFormulaAttribute>()
					.Where(attribute => attribute._Formula != null && attribute.FormulaFieldName != null))
				{
					BqlFormula.Verify(sender, e.Row, attribute._Formula, ref result, ref value);
						sender.RaiseFieldUpdating(_FieldName, e.Row, ref value);
						sender.SetValue(e.Row, _FieldOrdinal, value);

						break;
					}
				}

			object parent = EnsureParent(sender, e.Row, e.NewRow);

			UpdateParent(sender, e.Row, null, parent);
		}
		}

		/// <exclude/>
		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			using (new PXPerformanceInfoTimerScope(info => info.TmFormulaCalculations))
			{
			UpdateParent(sender, null, e.Row);

			EnsureChildren(sender, e.Row);
		}
		}

		protected bool _CancelDefaulting = false;

		/// <exclude/>
		public virtual void FormulaDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CancelCalculation) return;

			bool? result = null;
			object value = null;

			if (e.Row != null)
			{
				_CancelDefaulting = e.Cancel;
			}

			if (e.Row != null && e.Cancel != true)
			{
				if (_recursion.Value)
				{
					throw new PXException(string.Format(ErrorMessages.CircularReferenceInFormula, _BqlTable.FullName, _FieldName, _Formula.GetType().Name));
				}
				_recursion.Value = true;
				try
				{
					BqlFormula.Verify(sender, e.Row, _Formula, ref result, ref value);
				}
				finally
				{
					_recursion.Value = false;
				}

				if ((value != null && value != PXCache.NotSetValue) || result == true)
				{
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref value);
					e.NewValue = value;
					e.Cancel = true;
				}
			}
		}

		protected virtual void dependentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, Type dependentField)
		{
			if (CancelCalculation) return;
			
			IBqlTrigger trigger = _Formula as IBqlTrigger;
			if (trigger != null)
			{
				trigger.Verify(sender, _FieldName, e.Row);
			}
			else
			{
				bool isex = false;
				bool? result = null;
				object value = null;
				BqlFormula.ItemContainer item = new BqlFormula.ItemContainer(sender.CreateCopy(e.Row), dependentField, e.ExternalCall);
				try
				{
					BqlFormula.Verify(sender, item, _Formula, ref result, ref value);
				}
				catch (PXSetPropertyException ex)
				{
					sender.SetValue(e.Row, dependentField.Name, e.OldValue);
					sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldName), ex);
					isex = true;
				}

				if (!isex && value != PXCache.NotSetValue && item.InvolvedFields.Contains(dependentField))
				{
					sender.SetValueExt(e.Row, _FieldName, new PXCache.ExternalCallMarker(value) {IsInternalCall = true});
				}
			}
		}

		protected virtual object CalcAggregate(PXCache cache, object row, object oldrow, int digit)
		{
			return ((IBqlAggregateCalculator)_Aggregate).Calculate(cache, row, oldrow, _FieldOrdinal, -1);
		}

		protected virtual object CalcAggregate(PXCache cache, object row, object[] records, int digit)
		{
			return ((IBqlAggregateCalculator)_Aggregate).Calculate(cache, row, _FieldOrdinal, records, -1);
		}

		protected virtual void UpdateParent(PXCache sender, object row, object oldrow)
		{
			UpdateParent(sender, row, oldrow, null);
		}

		protected virtual void UpdateParent(PXCache sender, object row, object oldrow, object newparent)
		{
			if (_ParentFieldType == null || _Aggregate == null)
				return;

			if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
				throw new PXArgumentException(nameof(_ParentFieldType), ErrorMessages.InvalidField, _ParentFieldType.Name);

			Type ParentType = BqlCommand.GetItemType(_ParentFieldType);

			object oldparent = null;

			if (row != null && newparent == null)
			{
				newparent = PXParentAttribute.SelectParent(sender, row, ParentType);
			}

			if (oldrow != null && !object.ReferenceEquals(row, oldrow))
			{
					oldparent = PXParentAttribute.SelectParent(sender, oldrow, ParentType);

				if (!object.ReferenceEquals(oldparent, newparent) && newparent != null)
				{
					UpdateParent(sender, null, oldrow);
					UpdateParent(sender, row, null);

					return;
				}
			}

			if (newparent == null)
			{
				row = null;
			}

			object erow = row ?? oldrow;
			object foreignrecord = newparent ?? oldparent;

			if (foreignrecord != null)
			{
				PXCache parentcache = sender.Graph.Caches[ParentType];
				object val = null;
				bool curyviewstate = sender.Graph.Accessinfo.CuryViewState;
				object state;
				try
				{
					sender.Graph.Accessinfo.CuryViewState = false;
					state = parentcache.GetStateExt(foreignrecord, _ParentFieldType.Name);
				}
				finally
				{
					sender.Graph.Accessinfo.CuryViewState = curyviewstate;
				}
				TypeCode tc = TypeCode.Empty;
				if (state is PXFieldState)
				{
					tc = Type.GetTypeCode(((PXFieldState)state).DataType);
					state = ((PXFieldState)state).Value;
				}
				else if (state != null)
				{
					tc = Type.GetTypeCode(state.GetType());
				}

				if (tc == TypeCode.String)
				{
					state = parentcache.GetValue(foreignrecord, _ParentFieldType.Name);
					if (state != null)
						tc = Type.GetTypeCode(state.GetType());
					if (tc == TypeCode.String) return;
				}

				if (state != null)
				{
					val = CalcAggregate(sender, row, oldrow, -1);
				}
				if (val == null)
				{
					object[] records = PXParentAttribute.SelectSiblings(sender, erow, ParentType);
					val = CalcAggregate(sender, row, records, -1);
				}
				else
				{
					switch (tc)
					{
						case TypeCode.Int16:
							val = Convert.ToInt16(state) + Convert.ToInt16(val);
							break;
						case TypeCode.Int32:
							val = Convert.ToInt32(state) + Convert.ToInt32(val);
							break;
						case TypeCode.Double:
							val = Convert.ToDouble(state) + Convert.ToDouble(val);
							break;
						case TypeCode.Decimal:
							val = Convert.ToDecimal(state) + Convert.ToDecimal(val);
							break;
						case TypeCode.DateTime:
							val = Convert.ToDateTime(state) +
								  new TimeSpan(0, 0, Convert.ToInt32(val), 0);
							break;
						default:
							break;
					}
				}

				object result = ConvertValue(tc, val);

				object copy = parentcache.CreateCopy(foreignrecord);
				if (!object.Equals(foreignrecord, parentcache.Current))
				{
					parentcache.RaiseFieldUpdating(_ParentFieldType.Name, copy, ref result);
					parentcache.SetValue(copy, _ParentFieldType.Name, result);
					parentcache.Update(copy);
				}
				else
				{
					parentcache.SetValueExt(foreignrecord, _ParentFieldType.Name, result);
					parentcache.MarkUpdated(foreignrecord);
					if (!CancelParentUpdate)
					{
						parentcache.RaiseRowUpdated(foreignrecord, copy);
					}
				}
			}
		}

		/// <exclude/>
		public void CalcAggregate(PXCache sender, object[] records, object parent)
		{
			Type parenttype = BqlCommand.GetItemType(_ParentFieldType);
			PXCache parentcache = sender.Graph.Caches[parenttype];
			object parentrow = parenttype.IsAssignableFrom(parent.GetType()) ? parent : null;
			PXFieldState fieldstate = (PXFieldState)parentcache.GetStateExt(parentrow, _ParentFieldType.Name);
			object val = CalcAggregate(sender, null, records, -1);
			TypeCode tc = Type.GetTypeCode(fieldstate.DataType);
			parentcache.SetValue(parent, _ParentFieldType.Name, ConvertValue(tc, val));
		}

		private static object ConvertValue(TypeCode tc, object val)
		{
			switch (tc)
			{
				case TypeCode.Int16:
					return Convert.ToInt16(val);
				case TypeCode.Int32:
					return Convert.ToInt32(val);
				case TypeCode.Double:
					return Convert.ToDouble(val);
				case TypeCode.Decimal:
					return Convert.ToDecimal(val);
				case TypeCode.String:
					return Convert.ToString(val);
			}
			return val;
		}
		#endregion

		#region Initialization

		protected virtual void InitDependencies(PXCache sender)
		{
			if (_Formula != null)
			{
				List<Type> fields = new List<Type>();
				bool readonlycache = sender.Graph._ReadonlyCacheCreation;
				try
				{
					sender.Graph._ReadonlyCacheCreation = true;
					//_Formula.Parse(sender.Graph, null, null, fields, null, null, null);
					var exp = SQLExpression.None();
					_Formula.AppendExpression(ref exp, sender.Graph, new BqlCommandInfo(false) { Fields = fields, BuildExpression = false}, new BqlCommand.Selection());
				}
				finally
				{
					sender.Graph._ReadonlyCacheCreation = readonlycache;
				}
				_Dependencies = new HashSet<Type>(fields);
				foreach (Type t in _Dependencies)
				{
					if (t.IsNested && (BqlCommand.GetItemType(t) == sender.GetItemType() || sender.GetItemType().IsSubclassOf(BqlCommand.GetItemType(t))))
					{
						if (!t.Name.Equals(_FieldName, StringComparison.OrdinalIgnoreCase))
						{
							Type dependentFld = t;
							sender.FieldUpdatedEvents[t.Name.ToLower()] += delegate(PXCache cache, PXFieldUpdatedEventArgs e)
							{
								dependentFieldUpdated(cache, e, dependentFld);
							};
						}
					}
				}
				if (!(_Formula is IBqlTrigger))
				{
					sender.FieldDefaultingEvents[_FieldName.ToLower()] += FormulaDefaulting;
				}
			}
		}

		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_recursion = new ObjectRef<bool>();

			InitDependencies(sender);

			ISwitch formula = _Formula as ISwitch;
			if (formula != null)
			{
				formula.OuterField = sender.GetBqlField(_FieldName);
			}
			if (!sender.GetAttributesReadonly(_FieldName).Any(attr => attr is PXDBFieldAttribute || attr is PXDBCalcedAttribute)  && _Formula as IBqlTrigger == null)
			{
				sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), (cache, e) =>
				{
					// Prevent formula calculation for the same child DAC
					if (_recursion.Value && e.IsReadOnly) return;

					// Prevent formula calculation when the field is not used in the selection
					if (e.Record == null || e.Record.IsFieldRestricted(sender, _FieldName))
					{
						SetFormulaValue(cache, e.Row);
					}
				});
			}
			if (_Persistent)
			{
				sender.Graph.RowPersisted.AddHandler(sender.GetItemType(), Graph_RowPersisted);
			}

			if (IsRestricted)
			{
				Aggregate = null;
			}
		}

		protected void SetFormulaValue(PXCache sender, object row)
		{
			if (_Formula != null && !(this is PXUnboundFormulaAttribute))
				using (new PXConnectionScope())
				{
					PXFieldDefaultingEventArgs de = new PXFieldDefaultingEventArgs(row);
					FormulaDefaulting(sender, de);
					sender.SetValue(row, _FieldName, de.NewValue);
				}
		}

		/// <exclude/>
		public virtual void Graph_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				 (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				SetFormulaValue(sender, e.Row);
			}
		}
		#endregion

	}
	#endregion

	#region PXRateSyncAttribute
	/// <summary>Synchronizes CuryRateID with the field to which this
	/// attribute is applied.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXRateSyncAttribute : PXEventSubscriberAttribute, IPXRowInsertingSubscriber, IPXRowSelectedSubscriber
	{
		#region State
		#endregion

		#region Ctor
		#endregion

		#region Implementation
		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			sender.SetValue(e.Row, _FieldOrdinal, Convert.ToInt32(sender.Graph.Accessinfo.CuryRateID));
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			sender.Graph.Accessinfo.CuryRateID = Convert.ToInt32(sender.GetValue(e.Row, _FieldOrdinal));
		}
		#endregion
	}
	#endregion

	#region PXVariantAttribute
	/// <summary>Indicates a DAC field of <tt>byte[]</tt> type that is not
	/// mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// The code belows the definition of a data field in the DAC.
	/// <code>
	/// [PXVariant]
	/// [PXUIField(Visible = false)]
	/// public object TargetTable { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXVariantAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region Ctor
		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXVariantAttribute() : base() { }
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null && e.ReturnValue != null && e.ReturnValue.GetType() == typeof(byte[]))
				e.ReturnValue = GetValue((byte[])e.ReturnValue);
		}

		/// <exclude/>
		public static object GetValue(byte[] val)
		{
			if (val.Length > 0)
			{
				using (System.IO.MemoryStream ms = new System.IO.MemoryStream(val, 1, val.Length - 1))
				using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms, Encoding.Unicode))
				{
					switch ((TypeCode)(val[0]))
					{
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
			if (e.Row != null)
			{
				if (e.NewValue != null && e.NewValue.GetType() != typeof(byte[]))
				{
					TypeCode c = Type.GetTypeCode(e.NewValue.GetType());
					using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
					using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, Encoding.Unicode))
					{
						bw.Write((byte)c);
						switch (c)
						{
							case TypeCode.Boolean:
								bw.Write((Boolean)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Byte:
								bw.Write((Byte)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Char:
								bw.Write((Char)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.DateTime:
								bw.Write(((DateTime)e.NewValue).Ticks);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Decimal:
								bw.Write((Decimal)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Double:
								bw.Write((Double)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Int16:
								bw.Write((Int16)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Int32:
								bw.Write((Int32)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Int64:
								bw.Write((Int64)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.SByte:
								bw.Write((SByte)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Single:
								bw.Write((Single)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.String:
								bw.Write((String)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.UInt16:
								bw.Write((UInt16)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.UInt32:
								bw.Write((UInt32)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.UInt64:
								bw.Write((UInt64)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							default:
								throw new PXException(ErrorMessages.CantConvertValueToByteArray);
						}
			}
			}
		}
		}
		#endregion
			}
		#endregion

	#region PXPhoneValidationAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class PXPhoneValidationAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _PhoneValidationField;
		protected Type _CountryIdField;
		protected Type _ForeignIdField;
		protected string _PhoneMask = "";
		protected Definition _Definition;

		/// <summary>Get, set.</summary>
		public virtual Type PhoneValidationField
		{
			get
			{
				return _PhoneValidationField;
			}
			set
			{
				_PhoneValidationField = value;
			}
		}

		/// <summary>Get, set.</summary>
		public virtual string PhoneMask
		{
			get
			{
				return _PhoneMask;
			}
			set
			{
				_PhoneMask = value;
			}
		}

		/// <summary>Get, set.</summary>
		public virtual Type CountryIdField
		{
			get
			{
				return _CountryIdField;
			}
			set
			{
				_CountryIdField = value;
			}
		}
		private Type _CacheType;

		/// 
		public static void Clear<Table>()
			where Table : IBqlTable
		{
			PXDatabase.ResetSlot<Definition>("PhoneDefinitions", typeof(Table));
		}
		#endregion

		#region Ctor
		public PXPhoneValidationAttribute(Type phoneValidationField)
		{
			if (phoneValidationField != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(phoneValidationField))
					throw new PXArgumentException(nameof(PhoneValidationField), ErrorMessages.PhoneValidationFieldNotValid);
				_PhoneValidationField = phoneValidationField;
			}
		}
		#endregion

		#region Implementation
		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string id, phonenumber;

			if (sender.GetItemType().Name == BqlCommand.GetItemType(_CountryIdField).Name)
				id = (string)sender.GetValue(e.Row, _CountryIdField.Name);
			else
			{
				PXCache othercache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)];
				id = (string)othercache.GetValue(othercache.Current, _CountryIdField.Name);
			}
			phonenumber = (string)sender.GetValue(e.Row, _FieldName);

			if (id == null || !_Definition.CountryIdMask.TryGetValue(id, out _PhoneMask))
				_PhoneMask = "+#(###) ###-####";

			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, null, _PhoneMask, null, null, null, null);
		}
		private void countryIDRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[_CacheType];
			cache.SetAltered(_FieldName, true);
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (_CountryIdField != null)
			{
				PXCache cache;
				if (sender.BqlFields.Contains(_CountryIdField))
				{
					cache = sender;
				}
				else
				{
					cache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)];
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_CountryIdField.Name))
				{
					if (attr is PXSelectorAttribute)
					{
						_ForeignIdField = ((PXSelectorAttribute)attr).Field;
						break;
					}
				}
			}
			else
			{
				foreach (Type field in sender.BqlFields)
				{
					foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(field.Name))
					{
						if (attr is PXSelectorAttribute)
						{
							_ForeignIdField = ((PXSelectorAttribute)attr).Field;
							_CountryIdField = field;
							break;
						}
					}
				}
			}
			if (_ForeignIdField == null)
			{
				throw new PXException(ErrorMessages.CountryIdWithSelectorNotFound);
			}

			_Definition = PXContext.GetSlot<Definition>();
			if (_Definition == null)
			{
				PXContext.SetSlot<Definition>(_Definition = PXDatabase.GetSlot<Definition, PXPhoneValidationAttribute>("PhoneDefinitions", this, BqlCommand.GetItemType(_ForeignIdField)));
			}

			_CacheType = sender.GetItemType();
			sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)].RowSelected += countryIDRowSelected;
		}

		/// <exclude/>
		protected class Definition : IPrefetchable<PXPhoneValidationAttribute>
		{
			public Dictionary<string, string> CountryIdMask = new Dictionary<string, string>();
			public void Prefetch(PXPhoneValidationAttribute attr)
			{
				try
				{
					foreach (PXDataRecord record in PXDatabase.SelectMulti(BqlCommand.GetItemType(attr._PhoneValidationField),
																			new PXDataField(attr._ForeignIdField.Name),
																			new PXDataField(attr._PhoneValidationField.Name)))
						CountryIdMask[record.GetString(0)] = record.GetString(1);
				}
				catch
				{
					CountryIdMask.Clear();
					throw;
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXZipValidationAttribute
	/// <summary>Implements validation of a value for DAC fields that hold a
	/// ZIP postal code.</summary>
	/// <example>
	/// The code below shows a typical usage of the attribute. The constructor
	/// with two parameters, which are set to the fields from the
	/// <tt>Country</tt> DAC, is used. The <tt>CountryIdField</tt> property is
	/// set to a field from the <tt>ARAddress</tt> DAC where the
	/// <tt>PostalCode</tt> is defined.
	/// <code>
	/// [PXDBString(20)]
	/// [PXUIField(DisplayName = "Postal Code")]
	/// [PXZipValidation(typeof(Country.zipCodeRegexp),
	/// typeof(Country.zipCodeMask),
	/// CountryIdField = typeof(ARAddress.countryID))]
	/// public virtual string PostalCode { ... }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXZipValidationAttribute : PXAggregateAttribute,
		IPXFieldVerifyingSubscriber,
		IPXFieldSelectingSubscriber,
		IPXRowPersistingSubscriber
	{
		#region State
		protected Type _ZipValidationField;
		protected Type _ZipMaskField;
		protected Type _CountryIdField;
		protected Type _ForeignIdField;
		protected DefinitionZip _DefinitionZip;
		protected DefinitionMask _DefinitionMask;

		/// <summary>Gets or sets the DAC field that holds the ZIP validation
		/// information in a country data record.</summary>
		public virtual Type ZipValidationField
		{
			get
			{
				return _ZipValidationField;
			}
			set
			{
				_ZipValidationField = value;
			}
		}

		/// <summary>
		/// Gets or sets the DAC field that holds the identifier of a
		/// country data record. In case this field is set, zip code verification
		/// will also be performed each time the country ID field is updated.
		/// </summary>
		[Obsolete(
			"The property CountryIdField is deprecated and will be removed in the future versions. " +
			"Please switch to using constructor overload as soon as possible.", false)]
		public virtual Type CountryIdField
		{
			get
			{
				return _CountryIdField;
			}
			set
			{
				if (_CountryIdField != null)
				{
					throw new InvalidOperationException(ErrorMessages.CountryIdFieldNonReassignable);
				}

				_CountryIdField = value;

				Type formulaType = BqlCommand.Compose(typeof(Validate<>), _CountryIdField);
				this._Attributes.Add(new PXFormulaAttribute(formulaType));
			}
		}

		/// <summary>Clears the internal slots that are used to keep ZIP
		/// definitions and ZIP mask definitions.</summary>
		public static void Clear<Table>()
			where Table : IBqlTable
		{
			PXDatabase.ResetSlot<DefinitionZip>("ZipDefinitions", typeof(Table));
			PXDatabase.ResetSlot<DefinitionMask>("MaskDefinitions", typeof(Table));
		}
		#endregion

		#region Ctor
		/// <summary>Initializes a new instance of the attribute that does not
		/// know the field that holds the ZIP mask.</summary>
		/// <param name="zipValidationField">The field that holds country's ZIP validation information.</param>
		public PXZipValidationAttribute(Type zipValidationField)
			: this(zipValidationField, null, null)
		{
		}

		/// <summary>Initializes a new instance of the attribute that does not
		/// know the field that holds the ZIP mask.</summary>
		/// <param name="zipValidationField">The field that holds country's ZIP validation information.</param>
		/// <param name="zipMaskField">The field that holds the country's ZIP mask.</param>
		public PXZipValidationAttribute(Type zipValidationField, Type zipMaskField)
			: this(zipValidationField, zipMaskField, null)
		{
		}

		/// <summary>Initializes a new instance of the attribute that uses the
		/// specified fields to retrieve the ZIP validation information and ZIP
		/// masks per country, and optionally performs re-validation of ZIP code
		/// upon each update of the specified Country ID field.</summary>
		/// <param name="zipMaskField">The field that holds the country's ZIP mask.</param>
		/// <param name="zipValidationField">The field that holds the country's ZIP validation information.</param>
		/// <param name="countryIdField">
		/// Optional field holding the Country ID. If not <c>null</c>, the <see cref="PXZipValidationAttribute"/> 
		/// will re-validate the ZIP code each time the <paramref name="countryIdField"/> is updated.
		/// </param>
		public PXZipValidationAttribute(Type zipValidationField, Type zipMaskField, Type countryIdField = null)
		{
			if (zipValidationField != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(zipValidationField))
					throw new PXArgumentException(nameof(zipValidationField), ErrorMessages.ZipValidationFieldNotValid);
				_ZipValidationField = zipValidationField;
			}
			if (zipMaskField != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(zipMaskField))
					throw new PXArgumentException(nameof(zipMaskField), ErrorMessages.ZipMaskFieldNotValid);
				_ZipMaskField = zipMaskField;
			}
			if (countryIdField != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(countryIdField))
					throw new PXArgumentException(nameof(countryIdField), ErrorMessages.CountryIdFieldNotValid);
				_CountryIdField = countryIdField;

				Type formulaType = BqlCommand.Compose(typeof(Validate<>), _CountryIdField);
				this._Attributes.Add(new PXFormulaAttribute(formulaType));
			}
		}
		#endregion

		#region Implementation

		/// <exclude/>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
			if (!sender.Graph.IsContractBasedAPI
				|| !(sender.GetValue(e.Row, FieldOrdinal) is string zipCode))
					return;
			zipCode = zipCode.Trim();
			if (!ProcessZipValidation(sender, e.Row, zipCode))
				throw new PXRowPersistingException(FieldName, zipCode, ErrorMessages.ZipCodeDoesntMatch);
			}

		/// <exclude/>
		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
			// skip for copy paste at all
			if (sender.Graph.IsContractBasedAPI || sender.Graph.IsCopyPasteContext)
				return;

			if (!ProcessZipValidation(sender, e.Row, e.NewValue as string))
				throw new PXSetPropertyException(ErrorMessages.ZipCodeDoesntMatch);

				sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, null);
			}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (sender.Graph.IsImport || sender.Graph.IsContractBasedAPI || sender.Graph.IsCopyPasteContext)
				return;
			if ((_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
				&& TryGetInputMask(sender, e.Row, out string mask))
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, null, mask, null, null, null, null);
			}
		}

		protected bool TryGetInputMask(PXCache sender, object row, out string mask)
		{
			if (_ZipMaskField == null || _DefinitionMask == null)
			{
				mask = null;
				return false;
			}

			string id;
			if (sender.GetItemType().Name == BqlCommand.GetItemType(_CountryIdField).Name)
				id = (string)sender.GetValue(row, _CountryIdField.Name);
			else
			{
				PXCache othercache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)];
				object foreignrow = PXSelectorAttribute.Select(othercache, row, _CountryIdField.Name);
				if (foreignrow == null)
				{
					mask = null;
					return false;
				}
				id = (string)sender.Graph.Caches[foreignrow.GetType()].GetValue(foreignrow, _CountryIdField.Name);
			}

			if (id != null
				&& _DefinitionMask.CountryIdMask.TryGetValue(id, out mask)
				&& mask != null)
				return true;

			mask = null;
			return false;
		}

		protected virtual bool ProcessZipValidation(PXCache sender, object row, string zipCode)
				{
			string id;

					if (sender.GetItemType().Name == BqlCommand.GetItemType(_CountryIdField).Name)
				id = (string)sender.GetValue(row, _CountryIdField.Name);
					else
					{
						PXCache othercache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)];
				object foreignrow = PXSelectorAttribute.Select(othercache, row, _CountryIdField.Name);
						if (foreignrow == null)
					return true;
						id = (string)sender.Graph.Caches[foreignrow.GetType()].GetValue(foreignrow, _CountryIdField.Name);
					}

			if (id == null || !_DefinitionZip.CountryIdRegex.TryGetValue(id, out string regex))
				return true;

			return ValidateZip(zipCode, regex);
		}

		protected bool ValidateZip(string val, string regex)
		{
			if (String.IsNullOrEmpty(val) || String.IsNullOrEmpty(regex))
				return true;

			System.Text.RegularExpressions.Regex regexobject = new System.Text.RegularExpressions.Regex(regex);
			return regexobject.IsMatch(val);
		}

		#endregion

		#region Initialization
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_CountryIdField != null)
			{
				PXCache cache;
				if (sender.BqlFields.Contains(_CountryIdField))
				{
					cache = sender;
				}
				else
				{
					cache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)];
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_CountryIdField.Name))
				{
					if (attr is PXSelectorAttribute)
					{
						_ForeignIdField = ((PXSelectorAttribute)attr).Field;
						break;
					}
				}
			}
			else
			{
				foreach (Type field in sender.BqlFields)
				{
					foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(field.Name))
					{
						if (attr is PXSelectorAttribute)
						{
							_ForeignIdField = ((PXSelectorAttribute)attr).Field;
							_CountryIdField = field;
							break;
						}
					}
				}
			}
			if (_ForeignIdField == null)
			{
				throw new PXException(ErrorMessages.CountryIdWithSelectorNotFound);
			}

			_DefinitionZip = PXContext.GetSlot<DefinitionZip>();
			if (_DefinitionZip == null)
			{
				PXContext.SetSlot<DefinitionZip>(_DefinitionZip = PXDatabase.GetSlot<DefinitionZip, PXZipValidationAttribute>("ZipDefinitions", this, BqlCommand.GetItemType(_ForeignIdField)));
			}
			if (_ZipMaskField != null)
			{
				_DefinitionMask = PXContext.GetSlot<DefinitionMask>();
				if (_DefinitionMask == null)
					PXContext.SetSlot<DefinitionMask>(_DefinitionMask = PXDatabase.GetSlot<DefinitionMask, PXZipValidationAttribute>("MaskDefinitions", this, BqlCommand.GetItemType(_ForeignIdField)));
			}
		}


		/// <exclude/>
		protected class DefinitionZip : IPrefetchable<PXZipValidationAttribute>
		{
			public Dictionary<string, string> CountryIdRegex = new Dictionary<string, string>();

			public void Prefetch(PXZipValidationAttribute attr)
			{
				try
				{

					foreach (PXDataRecord record in PXDatabase.SelectMulti(BqlCommand.GetItemType(attr._ZipValidationField),
																		   new PXDataField(attr._ForeignIdField.Name),
																		   new PXDataField(attr._ZipValidationField.Name)))
					{
						CountryIdRegex[record.GetString(0)] = record.GetString(1);
					}

				}
				catch
				{
					CountryIdRegex.Clear();
					throw;
				}
			}
		}
		/// <exclude/>
		protected class DefinitionMask : IPrefetchable<PXZipValidationAttribute>
		{
			public Dictionary<string, string> CountryIdMask = new Dictionary<string, string>();

			public void Prefetch(PXZipValidationAttribute attr)
			{
				try
				{

					foreach (PXDataRecord record in PXDatabase.SelectMulti(BqlCommand.GetItemType(attr._ZipValidationField),
																																 new PXDataField(attr._ForeignIdField.Name),
																																 new PXDataField(attr._ZipMaskField.Name)))
					{
						CountryIdMask[record.GetString(0)] = record.GetString(1);
					}

				}
				catch
				{
					CountryIdMask.Clear();
					throw;
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDynamicMaskAttribute

	/// <summary>Indicates that the input values of the field are restricted by the dynamic mask that is specified in <see cref="BqlCommand" /> in constructor. For the UI, the attribute
	/// restricts entering of the symbols that doesn't match the mask. For the contact-based API, the mask could be disabled, enabled for field selecting, or checked
	/// during row persisting as specified by the <see cref="CBApiValidationType" /> property.</summary>
	/// <example>
	///   <code title="Example" description="" lang="C#">
	/// [PXDBString(20)] 
	/// [PXUIField(DisplayName = "Postal Code")] 
	/// [PXDynamicMask(typeof(Search&lt;Country.zipCodeMask, Where&lt;Country.countryID, Equal&lt;Current&lt;Address.countryID&gt;&gt;&gt;&gt;))] 
	/// public virtual string PostalCode { get; set; }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	public class PXDynamicMaskAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXRowPersistingSubscriber
	{
		#region State

		/// <summary>
		/// Specifies how the mask validation works for the contact-based API import
		/// (that is, for the graphs with <see cref="PXGraph.IsContractBasedAPI"/> set to <see langword="true"/>)
		/// or for the copy-paste context (that is, for the graphs with <see cref="PXGraph.IsCopyPasteContext"/> set to <see langword="true"/>).
		/// </summary>
		public enum ValidationType
		{
			/// <summary>
			/// The mask validation doesn't work.
			/// </summary>
			None = 0,
			/// <summary>
			/// The mask validation works during <see cref="IPXFieldSelectingSubscriber.FieldSelecting(PXCache, PXFieldSelectingEventArgs)"/>.
			/// </summary>
			FieldSelecting = 1,
			/// <summary>
			/// The mask validation works during <see cref="IPXRowPersistingSubscriber.RowPersisting(PXCache, PXRowPersistingEventArgs)"/>.
			/// </summary>
			RowPersisting = 2,
		}

		protected BqlCommand _MaskSearch;
		protected string _DefaultMask = string.Empty;

		/// <summary>Gets or sets the default mask that is used if the dynamic mask returns nothing.</summary>
		/// <value>The default value is <see cref="string.Empty" />, which means that no mask is applied.</value>
		public virtual string DefaultMask
		{
			get => _DefaultMask;
			set => _DefaultMask = value;
		}

		/// <summary>Specifies how the mask validation works for the contact-based API import (that is, for the graphs with <see cref="PXGraph.IsContractBasedAPI" /> set to <see langword="true"></see>).</summary>
		/// <value>The default value is <see cref="ValidationType.RowPersisting" />.</value>
		public ValidationType CBApiValidationType { get; set; } = ValidationType.RowPersisting;

		/// <summary>Specifies how the mask validation works for the copied-and-pasted values (that is, for the graphs with <see cref="PXGraph.IsCopyPasteContext" /> set to <see langword="true"></see>).</summary>
		/// <value>The default value is <see cref="ValidationType.None" />.</value>
		public ValidationType CopyPasteValidationType { get; set; } = ValidationType.None;

		#endregion

		#region Ctor
		/// <summary>Creates an instance of the attribute.</summary>
		/// <param name="maskSearch">A search BQL command that specifies a mask for the attribute.</param>
		public PXDynamicMaskAttribute(Type maskSearch)
		{
			if (maskSearch == null || !typeof(IBqlSearch).IsAssignableFrom(maskSearch))
				throw new PXArgumentException(nameof(maskSearch), ErrorMessages.CantCreateForeignKeyReference, maskSearch);
			_MaskSearch = BqlCommand.CreateInstance(maskSearch);
		}
		#endregion

		#region Implementation

		/// <exclude />
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel != PXAttributeLevel.Item && !e.IsAltered
				|| sender.Graph.IsContractBasedAPI
					&& CBApiValidationType != ValidationType.FieldSelecting
				|| sender.Graph.IsCopyPasteContext
					&& CopyPasteValidationType != ValidationType.FieldSelecting)
				return;

			TryGetMask(sender.Graph, e.Row, out string mask);
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, null, mask, null, null, null, null);
		}

		/// <exclude />
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (sender.Graph.IsContractBasedAPI
					&& CBApiValidationType != ValidationType.RowPersisting
				|| sender.Graph.IsCopyPasteContext
					&& CopyPasteValidationType != ValidationType.FieldSelecting)
				return;

			TryGetMask(sender.Graph, e.Row, out string mask);
			if (mask != null
				&& mask != string.Empty
				&& sender.GetValue(e.Row, FieldOrdinal) is string value
				&& !PX.Common.Mask.Format(mask, value).Trim().OrdinalEquals(value))
				throw new PXRowPersistingException(FieldName, value,
					ErrorMessages.StringDoesntMatchInputMask, value, mask);
		}

		/// <summary>Tries to get the mask for the specified field.</summary>
		/// <param name="graph">A graph instance.</param>
		/// <param name="row"></param>
		/// <param name="mask">The found validation mask, or <see cref="DefaultMask" />, depending on the returned value.</param>
		/// <returns>Returns <see langword="true"></see> if the mask is found, otherwise returns <see langword="false"></see>, which means that <see cref="DefaultMask" /> is used.</returns>
		public bool TryGetMask(PXGraph graph, object row, out string mask)
		{
			var view = graph.TypedViews.GetView(_MaskSearch, true);
			if (view.SelectSingleBound(new object[] { row }) is object country)
			{
				Type field = ((IBqlSearch)_MaskSearch).GetField();
				if (country is PXResult pxr)
					country = pxr[BqlCommand.GetItemType(field)];
				mask = PXFieldState.UnwrapValue(view.Cache.GetValueExt(country, field.Name)) as string;
				if (mask != null)
					return true;
			}
			mask = DefaultMask;
			return false;
		}

		#endregion

		#region Initialization
		/// <exclude />
		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}
		#endregion
	}
	#endregion

	#region PXEntityNameAttribute
	/// <exclude/>
	public class PXEntityNameAttribute : PXStringListAttribute
	{

		#region Initialization
		public PXEntityNameAttribute(Type refNoteID)
		{
			this.refNoteID = refNoteID;
		}
		private readonly Type refNoteID;
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			helper = new EntityHelper(sender.Graph);
		}
		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[refNoteID.DeclaringType];
			Guid? id = (Guid?)cache.GetValue(cache == sender ? e.Row : cache.Current, refNoteID.Name);
			e.ReturnValue = helper.GetFriendlyEntityName(id);
		}
		private EntityHelper helper;
		#endregion
	}
	#endregion

	#region PXScalarAttribute
	/// <summary>Defines the SQL subrequest that will be used to retrieve the
	/// value for the DAC field.</summary>
	/// <remarks>
	/// <para>You should place the attribute on the field that is not bound to
	/// any particular database column.</para>
	/// <para>The attribute will translate the provided BQL <tt>Search</tt>
	/// command into the SQL subrequest and insert it into the select
	/// statement that retrieves data records of this DAC. In the BQL command,
	/// you can reference any bound field of any DAC.</para>
	/// <para>Note that you should also annotate the field with an attribute
	/// that indicates an unbound
	/// field of a particular data type. Otherwise, the field may be
	/// displayed incorretly in the user interface.</para>
	/// <para>You should not use fields marked with the <tt>PXDBScalar</tt>
	/// attribute in BQL parameters (<tt>Current</tt>, <tt>Optional</tt>, and
	/// <tt>Required</tt>).</para>
	/// </remarks>
	/// <example>
	/// The attribute below selects the <tt>AcctName</tt> value from the
	/// <tt>Vendor</tt> table as the <tt>VendorName</tt> value.
	/// <code>
	/// [PXString(50, IsUnicode = true)]
	/// [PXDBScalar(typeof(
	/// Search&lt;Vendor.acctName,
	/// Where&lt;Vendor.bAccountID, Equal&lt;RQRequestLine.vendorID&gt;&gt;&gt;))]
	/// public virtual string VendorName { get; set; }</code>
	/// </example>
	[PXAttributeFamily(typeof(PXDBFieldAttribute))]
	public class PXDBScalarAttribute : PXDBFieldAttribute
	{
		protected BqlCommand _Search;
		protected Dictionary<CacheKey, SQLTree.SQLExpression> dictExpr = new Dictionary<CacheKey, SQLTree.SQLExpression>();
		protected Type typeOfProperty;
		private bool _recursiveCall;
		/// <exclude/>
		protected sealed class CacheKey
		{
			private Type _GraphType;
			private Type _CacheType;
			private Type _TableType;
			private int? _HashCode;
			public CacheKey(Type graphType, Type cacheType, Type tableType)
			{
				_GraphType = graphType;
				_CacheType = cacheType;
				_TableType = tableType;
			}
			public override bool Equals(object obj)
			{
				CacheKey that = obj as CacheKey;
				if (that == null)
				{
					return false;
				}
				return _GraphType == that._GraphType && _CacheType == that._CacheType && _TableType == that._TableType;
			}
			public override int GetHashCode()
			{
				unchecked
				{
					if (_HashCode == null)
					{
						int result = 13;
						result = 37 * result;
						result += _GraphType.GetHashCode();
						result = 37 * result;
						result += _CacheType.GetHashCode();
						result = 37 * result;
						if (_TableType != null)
						{
							result += _TableType.GetHashCode();
						}
						_HashCode = result;
					}
					return (int)_HashCode;
				}
			}
		}
		/// <summary>
		/// Initializes a new instance that uses the provided <tt>Search</tt> command
		/// to retrieve the value of the field the attribute is attached to.
		/// </summary>
		/// <param name="search">The BQL query based on the <tt>Search</tt> class or
		/// other class derived from <tt>IBqlSearch</tt>.</param>
		public PXDBScalarAttribute(Type search)
		{
			_Search = BqlCommand.CreateInstance(search);
			BqlField = ((IBqlSearch)_Search).GetField();
		}
		/// <exclude/>
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && 
				(!sender.BypassCalced || sender.BqlSelect != null) &&
				!_recursiveCall)
			{
				if (((e.Operation & PXDBOperation.Option) == PXDBOperation.Normal ||
				(e.Operation & PXDBOperation.Option) == PXDBOperation.Internal) ||
				(e.Operation & PXDBOperation.Option) == PXDBOperation.External)
				{
					CacheKey key = new CacheKey(sender.Graph.GetType(), sender.GetItemType(), e.Table);
					lock (((ICollection)dictExpr).SyncRoot)
					{
						SQLTree.SQLExpression selectExpr = null;
						if (dictExpr.TryGetValue(key, out selectExpr))
						{
							e.BqlTable = _BqlTable;
							e.Expr = selectExpr.Duplicate();
							return;
						}
					}
					
					SQLTree.Query query = null;
					List<Type> tables = null;
					if (sender.GetItemType().IsAssignableFrom(BqlCommand.GetItemType(((IBqlSearch)_Search).GetField())))
					{
						tables = new List<Type>(new Type[] { sender.GetItemType() });
					}
					else if (e.Table != null)
					{
						tables = new List<Type>(new Type[] { e.Table });
					}
					else
					{
						tables = new List<Type>();
					}

					bool isMutable;
					bool prevNull = true;
					int prevCount;
					var selection = new BqlCommand.Selection()
					{
						_Command = _Search,
						Restrict = false,
						RestrictedFields = null
					};
					try
					{
						var list = PXContext.GetSlot<PXMutableCollection>();
						if (prevNull = (list == null))
						{
							list = new PXMutableCollection();
							PX.Common.PXContext.SetSlot<PXMutableCollection>(list);
						}

						prevCount = list.Count;

						List<Type> tables2 = new List<Type>();
						tables2.AddRange(tables);

						//prevent raising PXDBScalar.CommandPreparing from internal selection.Columns collection
						_recursiveCall = true;

						query =_Search.GetQueryInternal(sender.Graph, new BqlCommandInfo(false) { Tables = tables2}, selection);

						isMutable = list.Count > prevCount;
					}
					finally
					{
						_recursiveCall = false;
						if (prevNull)
						{
							PX.Common.PXContext.SetSlot<PXMutableCollection>(null);
						}
					}

					Type field = ((IBqlSearch)_Search).GetField();
					if (field.DeclaringType.IsAssignableFrom(sender.GetItemType()) &&
						string.Equals(_FieldName, field.Name, StringComparison.OrdinalIgnoreCase))
					{
						e.Expr = new SQLTree.Column(_FieldName, tables.First());
					}
					else
					{
						//SQLTree.Query q=new SQLTree.Query(); 
						//if  there is no aggregate in GetAggregates, we assume that new field was added in _aggregate.Parse(...). Count(*) for example
						if (_Search is IBqlAggregate &&
							((IBqlAggregate) _Search).GetAggregates().All(f => f.GetField() != field)) {
							query.ClearSelection();
							query.Field(selection.ColExprs.Last());
						}
						else {
							query.ClearSelection();
							query.Field(BqlCommand.GetSingleExpression(field, sender.Graph, new List<Type>(), selection, BqlCommand.FieldPlace.Select));
							//BqlCommand.AppendSingleField(query, field, sender.Graph, selection);
						}

						e.Expr=new SQLTree.SubQuery(query);
					}

					e.BqlTable = _BqlTable;
					if (!isMutable)
					{
						lock (((ICollection)dictExpr).SyncRoot)
						{
							dictExpr[key] = e.Expr.Duplicate();
						}
					}
				}
				else if ((e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy)
				{
					e.Expr = SQLExpression.Null();
				}
			}
		}

		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			typeOfProperty = sender.GetFieldType(this._FieldName);
		}

		/// <exclude/>
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				object dbValue;

				// mySQL returns LONG from scalar selects, shorts when querying tinyint, so we should take destination field type into consideration.
				if (typeOfProperty == typeof(int) || typeOfProperty == typeof(int?))
					dbValue = e.Record.GetInt32(e.Position);
				else if (typeOfProperty == typeof(short) || typeOfProperty == typeof(short?))
					dbValue = e.Record.GetInt16(e.Position);
				else if (typeOfProperty == typeof(long) || typeOfProperty == typeof(long?))
					dbValue = e.Record.GetInt64(e.Position);
				else if (typeOfProperty == typeof(bool) || typeOfProperty == typeof(bool?))
					dbValue = e.Record.GetBoolean(e.Position);
				else
					dbValue = e.Record.GetValue(e.Position);

				SetValue(sender, e.Row, dbValue);
			}
			e.Position++;
		}

		protected virtual void SetValue(PXCache cache, Object row, Object value)
			{
			cache.SetValue(row, _FieldOrdinal, value);
		}
	}
	#endregion

	#region PXDBDatetimeScalarAttribute
	/// <exclude/>
	[PXAttributeFamily(typeof (PXDBFieldAttribute))]
	public class PXDBDatetimeScalarAttribute : PXDBScalarAttribute
		{
		public virtual bool PreserveTime { get; set;}
		public virtual bool UseTimeZone { get; set; }

		public PXDBDatetimeScalarAttribute(Type search)
			: base(search)
				{
			typeOfProperty = typeof (DateTime);
		}

		protected override void SetValue(PXCache cache, object row, object value)
		{
			DateTime? date = value as DateTime?;
			if (date != null)
			{
				if (PreserveTime)
			{
					if (UseTimeZone) value = PXTimeZoneInfo.ConvertTimeFromUtc(date.Value, LocaleInfo.GetTimeZone());
		}
				else value = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
	}
			base.SetValue(cache, row, value);
		}
		}
	#endregion

	#region PXRSACryptStringAttribute
	/// <exclude/>
	public class PXRSACryptStringAttribute : PXDBCryptStringAttribute
	{
		#region Ctor
		/// 
		public PXRSACryptStringAttribute()
		{
		}
		/// 
		public PXRSACryptStringAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Runtime
#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		/// 
		public static string Encrypt(string source)
		{
			return SitePolicy.RSACryptoProvider.IsEncryptEnable
					?
						Convert.ToBase64String(SitePolicy.RSACryptoProvider.Encrypt(Encoding.Unicode.GetBytes(source)))
					:
						source;
		}
#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.StorePermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Net.WebPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Net.DnsPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		internal static string Decrypt(string source)
		{
			byte[] bSource = null;
			if (source == null)
				return null;
			try
			{
				bSource = Convert.FromBase64String(source);
			}
			catch (Exception)
			{
				return source;
			}
			return SitePolicy.RSACryptoProvider.IsDecryptEnable
							?
								Encoding.Unicode.GetString(SitePolicy.RSACryptoProvider.Decrypt(bSource))
							:
								Encoding.Unicode.GetString(bSource);
		}

		public virtual bool EncryptOnCertificateReplacement(PXCache cache, object row)
		{
			return true;
		}
		#endregion

		#region Implementation
		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
		}

#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		protected override byte[] Encrypt(byte[] source)
		{
			return SitePolicy.RSACryptoProvider.IsEncryptEnable ? SitePolicy.RSACryptoProvider.Encrypt(source) : source;
		}

#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.StorePermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Net.WebPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		protected override byte[] Decrypt(byte[] source)
		{
			return SitePolicy.RSACryptoProvider.IsDecryptEnable ? SitePolicy.RSACryptoProvider.Decrypt(source) : source;
		}
		#endregion
		}
		#endregion

	#region PXRSACryptDataProviderPasswordParameterAttribute
	/// <summary>
	/// Encryption is used for value of Password parameter for Data Providers.
	/// isEncryptedField is used to store current state of field - encrypted or not.
	/// </summary>
	/// <param name="nameField">BQL field</param>
	/// <param name="isEncryptedField">BQL field</param>
	public class PXRSACryptDataProviderPasswordParameterAttribute : PXRSACryptStringAttribute, IPXRowPersistingSubscriber
	{
		protected string _parameterNameForValueEncription = "Password";
		protected Type _nameField;
		protected Type _isEncryptedField;

		public PXRSACryptDataProviderPasswordParameterAttribute(Type nameField, Type isEncryptedField)
			: base()
		{
			checkParams(nameField, isEncryptedField);
			_nameField = nameField;
			_isEncryptedField = isEncryptedField;
		}

		public PXRSACryptDataProviderPasswordParameterAttribute(int length, Type nameField, Type isEncryptedField)
			: base(length)
		{
			checkParams(nameField, isEncryptedField);
			_nameField = nameField;
			_isEncryptedField = isEncryptedField;
		}

		private void checkParams(Type nameField, Type isEncryptedField)
		{
			if (nameField == null)
			{
				throw new PXArgumentException("nameField", ErrorMessages.ArgumentNullException);
			}
			if (isEncryptedField == null)
			{
				throw new PXArgumentException("isEncryptedField", ErrorMessages.ArgumentNullException);
			}
			if (!typeof(IBqlField).IsAssignableFrom(nameField))
			{
				throw new PXArgumentException("nameField", ErrorMessages.InvalidIBqlField);
			}
			if (!typeof(IBqlField).IsAssignableFrom(isEncryptedField))
			{
				throw new PXArgumentException("isEncryptedField", ErrorMessages.InvalidIBqlField);
			}
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				bool? isEncrypted = (sender.GetValue(e.Row, _isEncryptedField.Name) as bool?) ?? false;
				isViewDeprypted = (isEncrypted == false);
			}
			base.FieldSelecting(sender, e);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				// Make sure that isEncrypted field is before the Value filed in the DAC
				bool? isEncrypted = (sender.GetValue(e.Row, _isEncryptedField.Name) as bool?) ?? false;
				var nameFieldValue = (sender.GetValue(e.Row, _nameField.Name) as string);
				isEncryptionRequired = (nameFieldValue?.Equals(_parameterNameForValueEncription, StringComparison.InvariantCultureIgnoreCase) ?? false) && (isEncrypted == true);
			}
			base.RowSelecting(sender, e);
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var nameFieldValue = (sender.GetValue(e.Row, _nameField.Name) as string);
			bool encryptionRequired = nameFieldValue?.Equals(_parameterNameForValueEncription, StringComparison.InvariantCultureIgnoreCase) ?? false;

			sender.SetValue(e.Row, _isEncryptedField.Name, encryptionRequired);
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				var nameFieldValue = (sender.GetValue(e.Row, _nameField.Name) as string);
				isEncryptionRequired = nameFieldValue?.Equals(_parameterNameForValueEncription, StringComparison.InvariantCultureIgnoreCase) ?? false;
			}
			base.CommandPreparing(sender, e);

			if (isEncryptionRequired && e.Value == null)
			{
				e.DataValue = string.Empty;
			}
		}

		public override bool EncryptOnCertificateReplacement(PXCache cache, object row)
		{
			bool? isEncrypted = cache.GetValue(row, _isEncryptedField.Name) as bool?;
			return isEncrypted == true;
		}
	}
	#endregion

	#region PXEnumDescriptionAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class PXEnumDescriptionAttribute : Attribute
	{
		private string _displayName;
		private string _displayNameKey;
		private Type _enumType;
		private string _field;
		private string _category;

		private static object _syncObj = new object();

		private static Hashtable _enumsInfoNotLocalizable = new Hashtable();
		private static Hashtable GetEnumsInfo(bool localize)
		{
			if (localize)
				return PXDatabase.GetSlot<Hashtable>("PXEnumDescriptionAttribute$EnumsInfo$" + System.Threading.Thread.CurrentThread.CurrentUICulture.Name, typeof(LocalizationTranslation));
			else
				return _enumsInfoNotLocalizable;
		}
		public PXEnumDescriptionAttribute(string displayName, Type keyType)
			: base()
		{
			_displayName = displayName;
			_displayNameKey = keyType.ToString();
		}

		/// <summary>Get, set.</summary>
		public string Category
		{
			get
			{
				return _category;
			}
			set
			{
				_category = value;
			}
		}

		/// <summary>Get, set.</summary>
		public Type EnumType
		{
			get
			{
				return _enumType;
			}
			set
			{
				if (_enumType == null || !typeof(Enum).IsAssignableFrom(_enumType))
					throw new PXException(ErrorMessages.BadEnumType);
				_enumType = value;
			}
		}

		/// <summary>Get, set.</summary>
		public string Field
		{
			get
			{
				return _field;
			}
			set
			{
				if (string.IsNullOrEmpty(_field))
					throw new PXException(ErrorMessages.BadEnumField);
				_field = value;
			}
		}

		/// <summary>Get.</summary>
		public string DisplayName
		{
			get
			{
				string result = null;
				if (_enumType != null && !string.IsNullOrEmpty(_field) &&
					!System.Globalization.CultureInfo.InvariantCulture.Equals(System.Threading.Thread.CurrentThread.CurrentCulture))
				{
					result = PXLocalizer.Localize(_displayName, _displayNameKey);
				}
				return string.IsNullOrEmpty(result) ? _displayName : result;
			}
		}

		/// 
		public static string[] GetNames(Type @enum)
		{
			return new List<string>(GetValueNamePairs(@enum).Values).ToArray();
		}

		public static IDictionary<object, string> GetValueNamePairs(Type @enum, bool localize = true)
		{
			return GetValueNamePairs(@enum, null, localize);
		}

		public static IDictionary<object, string> GetValueNamePairs(Type @enum, string categoryName, bool localize = true)
		{
			var key = string.Concat(@enum.FullName, "__", categoryName);
			var _enumsInfo = GetEnumsInfo(localize);
			lock (_syncObj)
			{
				if (_enumsInfo.ContainsKey(key)) return _enumsInfo[key] as IDictionary<object, string>;

				var returnAll = string.IsNullOrEmpty(categoryName);
				var list = new Dictionary<object, string>();
				foreach (var info in GetFullInfoUnSafelly(@enum, localize))
				{
					var categories = info.Value.Key.Split(',');
					if (returnAll || Array.Find(categories, s => string.Compare(s, categoryName, true) == 0) != null)
						list.Add(info.Key, info.Value.Value);
				}
				_enumsInfo.Add(key, list);
				return list;
			}
		}

		public static IDictionary<object, KeyValuePair<string, string>> GetFullInfo(Type @enum, bool localize = false)
		{
			lock (_syncObj)
			{
				return GetFullInfoUnSafelly(@enum, localize);
			}
		}

		private static IDictionary<object, KeyValuePair<string, string>> GetFullInfoUnSafelly(Type @enum, bool localize = true)
		{
			var key = @enum.FullName + "_";
			var _enumsInfo = GetEnumsInfo(localize);
			if (_enumsInfo.ContainsKey(key)) return _enumsInfo[key] as IDictionary<object, KeyValuePair<string, string>>;

			var list = new Dictionary<object, KeyValuePair<string, string>>();
			foreach (var field in
				@enum.GetFields(BindingFlags.Static | BindingFlags.GetField |
								BindingFlags.Public))
			{
				var value = Enum.Parse(@enum, field.Name);
				var info = new KeyValuePair<string, string>(null, field.Name);
				var att = GetCustomAttribute(field, typeof(PXEnumDescriptionAttribute)) as PXEnumDescriptionAttribute;
				if (att != null)
				{
					att._enumType = @enum;
					att._field = field.Name;
					info = new KeyValuePair<string, string>(att._category, localize ? att.DisplayName : att._displayName);
				}
				list.Add(value, info);
			}
			_enumsInfo.Add(key, list);
			return list;
		}

		/// 
		public static KeyValuePair<string, string> GetInfo(Type @enum, object value)
		{
			var name = Enum.GetName(@enum, value);
			if (!string.IsNullOrEmpty(name))
			{
				var att = GetCustomAttribute(@enum.GetField(name), typeof(PXEnumDescriptionAttribute)) as PXEnumDescriptionAttribute;
				if (att != null) return new KeyValuePair<string, string>(att.Category, att.DisplayName);
			}
			return new KeyValuePair<string, string>();
		}
	}
	#endregion

	#region PXDBDataLengthAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDataLengthAttribute : PXEventSubscriberAttribute, IPXCommandPreparingSubscriber, IPXRowSelectingSubscriber
	{
		#region State
		private string _TargetFieldName;
		#endregion

		#region Ctor
		/// 
		public PXDBDataLengthAttribute(Type targetField)
		{
			this._TargetFieldName = targetField.Name;
		}

		/// 
		public PXDBDataLengthAttribute(string targetFieldName)
		{
			this._TargetFieldName = targetFieldName;
		}
		#endregion

		#region Implementation
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			Type table = e.Table ?? this._BqlTable;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
			{
				e.BqlTable = _BqlTable;
				e.Expr = new SQLTree.Column(this._TargetFieldName, table).BinaryLength();
			}
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetValue(e.Position));
			}
			e.Position++;
		}
		#endregion
	}
	#endregion

	#region DashboardTypeAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DashboardTypeAttribute : Attribute
	{
		/// <exclude/>
		public enum Type
		{
			Default = 0,
			WikiArticle = 1,
			Task = 2,
			Announcements = 3,
			Chart = 4,
		}

		public readonly int[] Types;

		public DashboardTypeAttribute(params int[] type)
		{
			Types = type;
		}
	}
	#endregion

	#region DashboardVisibleAttribute

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DashboardVisibleAttribute : PXEventSubscriberAttribute
	{
		private readonly bool _visible;

		/// 
		public DashboardVisibleAttribute(bool visible)
		{
			_visible = visible;
		}

		/// 
		public DashboardVisibleAttribute() : this(true) { }

		/// <summary>Get.</summary>
		public bool Visible
		{
			get { return _visible; }
		}
	}

	#endregion

	#region PXEMailAccountIDSelectorAttribute
	/// <exclude/>
	public class PXEMailAccountIDSelectorAttribute : PXSelectorAttribute
	{
		public PXEMailAccountIDSelectorAttribute(Type where)
			: base(GetCommand(where), 
			typeof(EMailAccount.address), 
			typeof(EMailAccount.description),
			typeof(EMailAccount.emailAccountType),
			typeof(EMailAccount.replyAddress))
		{
			base.DescriptionField = typeof(EMailAccount.description);
		}
		public PXEMailAccountIDSelectorAttribute()
			: this(null)
		{
		}

		private static Type GetCommand(Type additional)
		{
			Type join = typeof (LeftJoin<PreferencesEmail, On<PreferencesEmail.defaultEMailAccountID, Equal<EMailAccount.emailAccountID>>,
				LeftJoin<EMailSyncAccount, On<EMailSyncAccount.emailAccountID, Equal<EMailAccount.emailAccountID>>,
				LeftJoin<EMailSyncServer, On<EMailSyncServer.accountID, Equal<EMailSyncAccount.serverID>>>>>);
			Type where = typeof(Where2<Match<Current<AccessInfo.userName>>, And<Where<EMailAccount.emailAccountType, NotEqual<EmailAccountTypesAttribute.exchange>, Or<EMailSyncServer.isActive, Equal<True>>>>>);
			Type order = typeof (OrderBy<Asc<EMailAccount.description>>);

			if (additional != null) where = BqlCommand.Compose(typeof(Where2<,>), where, typeof(And<>), additional);

			Type command = BqlCommand.Compose(typeof(Search2<,,,>), typeof(EMailAccount.emailAccountID), join, where, order);
			return command;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXSelect<EMailAccount, Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>
				.SelectWindowed(sender.Graph, 0, 1, e.NewValue) != null)
				e.Cancel = true;
			else
				base.FieldVerifying(sender, e);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			this.SelectorMode = PXSelectorMode.DisplayModeHint;
		}
	}
	#endregion
	 
	#region PXCustomizationAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Class)]
	public class PXCustomizationAttribute : Attribute { }
	#endregion

	#region PXDateAndTimeAttribute

	/// <summary>Indicates a DAC field of <tt>DateTime?</tt> type that is not mapped to a database column and is represented in the user interface by two controls to input date
	/// and time values separately.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field. The field is not bound to a table column.</remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXDateAndTime]
	/// public virtual DateTime? StartDate { get; set; }</code>
	/// </example>
	public class PXDateAndTimeAttribute : PXDateAttribute
	{
		/// <exclude/>
		public const string DATE_FIELD_POSTFIX = "_Date";
		/// <exclude/>
		public const string TIME_FIELD_POSTFIX = "_Time";

		protected string _TimeInputMask = "t";
		protected string _TimeDisplayMask = "t";

		protected string _DateInputMask = "d";
		protected string _DateDisplayMask = "d";

		private bool? _isEnabledDate = null;
		private bool? _isEnabledTime = null;

		#region Initialization
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
		}
		#endregion
		#region Implementation
		/// <exclude/>
		public void Date_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered)
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + DATE_FIELD_POSTFIX, _IsKey, null, _DateInputMask, _DateDisplayMask, null, null);
				PXDateAndTimeAttribute attr = GetAttribute(sender, e.Row, _FieldName);
				if (attr != null && attr._isEnabledDate.HasValue) state.Enabled = attr._isEnabledDate.Value;
				e.ReturnState = state;
			}
			
			if (e.ReturnValue != null)
			{
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
		public void Time_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered)
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + TIME_FIELD_POSTFIX, _IsKey, null, _TimeInputMask, _TimeDisplayMask, null, null);
				PXDateAndTimeAttribute attr = GetAttribute(sender, e.Row, _FieldName);
				if (attr != null && attr._isEnabledTime.HasValue) state.Enabled = attr._isEnabledTime.Value;
				e.ReturnState = state;
			}

			if (e.ReturnValue != null)
			{
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
			if (e.NewValue is string)
			{
				DateTime val;
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					object fieldval = (sender.Graph.IsMobile) ? val : CombineDateTime(val, oldval);
					sender.SetValue(e.Row, _FieldOrdinal, fieldval);
					if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null)
					{
						sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
					}
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);
				object fieldval = (sender.Graph.IsMobile) ? e.NewValue : CombineDateTime((DateTime)e.NewValue, oldval);
				sender.SetValue(e.Row, _FieldOrdinal, fieldval);
				if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null)
				{
					sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
				}
			}
		}

		/// <exclude/>
		public void Time_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				DateTime val;
				DateTime? fieldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				if (fieldval != null && DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					fieldval = (sender.Graph.IsMobile) ? val : CombineDateTime(fieldval, val);
					sender.SetValueExt(e.Row, _FieldName, fieldval);
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
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
		/// return PXDateAndTimeAttribute.CombineDateTime(date, new DateTime(2008, 1, 1, 9, 0, 0));
		/// </code>
		/// </example>
		public static DateTime? CombineDateTime(DateTime? date, DateTime? time)
		{
			if (date != null)
			{
				if (time != null)
				{
					return new DateTime(((DateTime)date).Year, ((DateTime)date).Month, ((DateTime)date).Day, ((DateTime)time).Hour, ((DateTime)time).Minute, 0);
				}
				else
				{
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
		/// <tt>PXDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		/// <typeparam name="Field">The field the attribute is attached to.</typeparam>
		public static void SetDateEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetDateEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		/// <summary>Enables or disables the input control that represents the
		/// date part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		public static void SetDateEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			PXDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isEnabledDate = isEnabled;
		}

		/// <summary>Enables or disables the input control that represents the
		/// time part of the field value. The field is specified as the type
		/// parameter.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		/// <typeparam name="Field">The field the attribute is attached to.</typeparam>
		public static void SetTimeEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetTimeEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		/// <summary>Enables or disables the input control that represents the
		/// time part of the field value.</summary>
		/// <param name="cache">The cache object to search for
		/// <tt>PXDateAndTime</tt> attributes.</param>
		/// <param name="data">The data record the method is applied to. If
		/// <tt>null</tt>, the method is applied to all data records in the cache
		/// object.</param>
		/// <param name="name">The name of the field the attribute is attached
		/// to.</param>
		/// <param name="isEnabled">The value indicating whether the input control
		/// is enabled.</param>
		public static void SetTimeEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			PXDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isEnabledTime = isEnabled;
		}
		#endregion

		#region Private Methods
		/// <exclude/>
		public static PXDateAndTimeAttribute GetAttribute(PXCache cache, object data, string name)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				var dtAttr = attr as PXDateAndTimeAttribute;
				if (dtAttr != null) return dtAttr;
			}
			return null;
		}
		#endregion

		/// <summary>Represents the current local date and time in BQL.</summary>
		public class now : PX.Data.BQL.BqlDateTime.Constant<now>
		{
			public now() : base(DateTime.MinValue) { }
			public override DateTime Value => PXTimeZoneInfo.Now;
		}

	}
	#endregion

	#region PXVerifyEndDateAttribute

	/// <summary>Verifies the integrity of a date range. That is, the start date of the range must be less than or equal to the end date.</summary>
	/// <remarks>
	/// <para>It can be applied to only the DAC-field that defines the end date of the range. This field should be of the <tt>DateTime</tt> type.</para>
	/// <para>When a user changes the range incorrectly (sets the start date greater than the end date), the attribute either 
	/// displays an error message or the start date is set equal to the end date. The behavior depends on the attribute settings.</para>
	/// <para>It is assumed that the field of start date is declared in the DAC-class before the field of the end date. 
	/// Otherwise, the attribute may work incorrectly.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDBDate]
	/// [PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.SelectorVisible)]
	/// [PXVerifyEndDate(typeof(Contract.startDate), AllowAutoChange = false)]
	/// public virtual DateTime? ExpireDate { get; set; }
	/// </code>
	/// </example>
	public class PXVerifyEndDateAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertedSubscriber
	{
		private readonly string _startDateField;

		/// <summary>
		/// Gets or sets the flag of the date range autocorrection.
		/// If this flag is set, the start date becomes equal to the end date after it was changed by a user incorrectly. 
		/// Otherwise, an error message appears on the changed field.
		/// </summary>
		public bool AllowAutoChange { get; set; }

		/// <summary>
		/// Gets or sets the flag of information about the autocorrection.
		/// If this flag is set, a warning message appears on the changed field.
		/// </summary>
		public bool AutoChangeWarning { get; set; }

		#region Ctor

		/// <summary>
		/// Initializes a new instance of the attribute.
		/// </summary>
		/// <param name="startDateField">Type of the start date DAC-field. This field should be of the <tt>DateTime</tt> type 
		/// and declared before current (the end date) field in the DAC-class.</param>
		public PXVerifyEndDateAttribute(Type startDateField)
		{
			_startDateField = startDateField.Name;
			AllowAutoChange = true;
			AutoChangeWarning = false;
		}

		#endregion

		#region Implementation

		/// <exclude />
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), _startDateField, StartDateVerifyning);
		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DateTime? startDate = (DateTime?)sender.GetValue(e.Row, _startDateField);
			DateTime? endDate = (DateTime?)e.NewValue;

			Verifying(sender, e.Row, startDate, endDate, _startDateField, endDate);
		}

		private void StartDateVerifyning(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			object endDatePending = sender.GetValuePending(e.Row, _FieldName);
			if(endDatePending == null || endDatePending == PXCache.NotSetValue) // end date not changed
			{
				DateTime? startDate = (DateTime?)e.NewValue;
				DateTime? endDate = (DateTime?)sender.GetValue(e.Row, _FieldName);
				Verifying(sender, e.Row, startDate, endDate, _FieldName, startDate);
			}
		}

		void IPXRowInsertedSubscriber.RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DateTime? startDate = (DateTime?)sender.GetValue(e.Row, _startDateField);
			DateTime? endDate = (DateTime?)sender.GetValue(e.Row, _FieldName);

			try
			{
				Verifying(sender, e.Row, startDate, endDate, _startDateField, endDate);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling(_FieldName, e.Row, endDate, ex);
			}
		}

		private void Verifying(PXCache sender, object row, DateTime? startDate, DateTime? endDate, string fieldName, DateTime? newValue)
		{
			if (startDate != null && endDate != null && startDate > endDate)
			{
				if (AllowAutoChange)
				{
					sender.SetValueExt(row, fieldName, newValue);
					if (AutoChangeWarning)
						sender.RaiseExceptionHandling(fieldName, row, endDate, new PXSetPropertyException(InfoMessages.ChangedAutomatically, PXErrorLevel.Warning, fieldName, newValue));
				}
				else if (fieldName == _FieldName) // start date changed
				{
					throw new PXSetPropertyException(ErrorMessages.StartDateGreaterThanEndDate, PXUIFieldAttribute.GetDisplayName(sender, _startDateField), _FieldName, endDate);
				}
				else
				{
					throw new PXSetPropertyException(ErrorMessages.EndDateLessThanStartDate, _FieldName, PXUIFieldAttribute.GetDisplayName(sender, _startDateField), startDate);
				}
			}
		}

		#endregion

	}
	#endregion

	#region TimeSpanFormatType

	/// <summary>Defines data format types for the
	/// <see cref="PXDBTimeSpanLongAttribute">PXDBTimeSpanLong</see>
	/// and <see cref="PXTimeSpanLongAttribute">PXTimeSpanLong</see>
	/// attributes.</summary>
	public enum TimeSpanFormatType
	{
		/// <summary>
		/// Time span in format "dddhhmm", where "ddd" represents days, "hh" represents
		/// hours (with leading zeros), "mm" represents minutes (with leading zeros).
		/// </summary>
		DaysHoursMinites = 0,
		/// <summary>
		/// Time span in format "dddhhmm", where "ddd" represents days, "hh" represents
		/// hours (with leading zeros), "mm" represents minutes (with leading zeros).
		/// </summary>
		DaysHoursMinitesCompact,
		/// <summary>
		/// Time span in format "HHHHmm", where "HHHH" represents
		/// hours (recalculated to include days), "mm" represents minutes (with leading zeros).
		/// </summary>
		LongHoursMinutes,
		/// <summary>
		/// Time span in format "hhmm", where "hh" represents
		/// hours, "mm" represents minutes (with leading zeros).
		/// </summary>
		ShortHoursMinutes,
		/// <summary>
		/// Time span in format "hhmm", where "hh" represents
		/// hours (with leading zeros), "mm" represents minutes (with leading zeros).
		/// </summary>
		ShortHoursMinutesCompact,
	}

	#endregion

	#region PXTimeSpanLongAttribute
	/// <summary>Indicates a DAC field of the <tt>int?</tt> type that represents the duration (in minutes)
	/// and that is not mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field. The field is not bound to a table column.</remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
	/// public virtual int? InitResponse { get; set; }</code>
	/// </example>
	public class PXTimeSpanLongAttribute : PXIntAttribute
	{
		#region State
		protected string[] _inputMasks = new string[] { ActionsMessages.DurationInputMask, "### d 00:00", ActionsMessages.TimeSpanLongHM, ActionsMessages.TimeSpanHM, "00:00" };
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
			get { return inputMask; }
			set
			{
				inputMask = value;
				_maskLenght = 0;

				foreach (char c in value)
				{
					if (c == '#' || c == '0')
						_maskLenght += 1;
				}
			}
		}

		private string inputMask;
		private int _maskLenght;

		#endregion

		#region Ctor
		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXTimeSpanLongAttribute()
		{
			inputMask = null;
			_maskLenght = 0;
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation

		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string inputMask = this.inputMask ?? _inputMasks[(int)this._Format];
				int lenght = this.inputMask != null ? _maskLenght : _lengths[(int)this._Format];
				inputMask = PXMessages.LocalizeNoPrefix(inputMask);
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, lenght, null, _FieldName, _IsKey, null, String.IsNullOrEmpty(inputMask) ? null : inputMask, null, null, null, null);
			}

			if (e.ReturnValue != null)
			{
				TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
				int hours = (this._Format == TimeSpanFormatType.LongHoursMinutes) ? span.Days * 24 + span.Hours : span.Hours;
				var returnValue = string.Format(_outputFormats[(int)this._Format], span.Days, hours, span.Minutes);
				e.ReturnValue = returnValue.Length < _maskLenght ? (new String(' ', _maskLenght - returnValue.Length)) + returnValue : returnValue;
			}
		}

		/// <exclude/>
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				int length = ((string)e.NewValue).Length;
				int maxLength = this._lengths[(int)this._Format];
				if (length < maxLength)
				{
					StringBuilder bld = new StringBuilder(maxLength);
					for (int i = length; i < maxLength; i++)
					{
						bld.Append('0');
					}
					bld.Append((string)e.NewValue);
					e.NewValue = bld.ToString();
				}

				int val = 0;
				if (!string.IsNullOrEmpty((string)e.NewValue) && int.TryParse(((string)e.NewValue).Replace(" ", "0"), out val))
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
			if (e.NewValue == null && this._NullIsZero)
				e.NewValue = (int)0;
		}
		#endregion
	}
	#endregion

	#region PXTimeSpanAttribute
	/// <summary>Indicates a DAC field of <tt>int?</tt> type that represents a date value as minutes passed from 01/01/1900 and that is not mapped to a database column.</summary>
	/// <remarks>The attribute is added to the value declaration of a DAC field.
	/// The field is not bound to a table column.</remarks>
	/// <example>
	/// The code below shows a full definition of a DAC field not bound to any database columns.
	/// <code title="" description="" lang="CS">
	/// public abstract class timeOnly : PX.Data.BQL.BqlInt.Field&lt;timeOnly&gt; { }
	/// [PXUIField(DisplayName = "Time")]
	/// [PXTimeSpan]
	/// public virtual int? TimeOnly
	/// {
	///     get
	///     {
	///         return (int?)Date.Value.TimeOfDay.TotalMinutes;
	///     }
	/// }</code></example>
	public class PXTimeSpanAttribute : PXIntAttribute
	{
		#region State
		protected string _InputMask = "HH:mm";
		protected string _DisplayMask = "HH:mm";
		protected new DateTime? _MinValue;
		protected new DateTime? _MaxValue;
		/// <summary>Gets or sets the pattern that indicates the allowed
		/// characters in a field value. The user interface will not allow the
		/// user to enter other characters in the input control associated with
		/// the field.</summary>
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
		/// <summary>Get, set.</summary>
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
		public new string MinValue
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
					_MinValue = DateTime.Parse(value);
				}
				else
				{
					_MinValue = null;
				}
			}
		}
		/// <summary>Gets or sets the maximum value for the field.</summary>
		public new string MaxValue
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
					_MaxValue = DateTime.Parse(value);
				}
				else
				{
					_MaxValue = null;
				}
			}
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Initializes a new instance with default parameters.
		/// </summary>
		public PXTimeSpanAttribute()
		{
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation
		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, null, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}

			if (e.ReturnValue != null)
			{
				TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
				e.ReturnValue = new DateTime(1900, 1, 1, span.Hours, span.Minutes, 0);
			}
		}

		/// <exclude/>
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.NewValue is int)
			{
			}
			else if (e.NewValue is string)
			{
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
					e.NewValue = (int)span.TotalMinutes;
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime val = (DateTime)e.NewValue;
				TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
				e.NewValue = (int)span.TotalMinutes;
			}
		}
		#endregion
	}
	#endregion

	#region PXImageAttribute
	/// <exclude/>
	public class PXImageAttribute : PXStringAttribute
	{
		/// <summary>Get, set.</summary>
		public string HeaderImage { get; set; }

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXImageState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, _InputMask, null, null, null, null, HeaderImage);
			}
		}
		#endregion
	}
	#endregion

	#region PXImageHeaderAttribute	
	/// <exclude/>
	public class PXHeaderImageAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		protected string _HeaderImage = "";

		#endregion
		
		#region Ctor
		public PXHeaderImageAttribute(string headerImage)
		{
			_HeaderImage = headerImage;
		}
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXHeaderImageState.CreateInstance(e.ReturnState, FieldName, _HeaderImage);
			}
		}
		#endregion
	}
	#endregion
	#region ReadOnlyScope
	/// <exclude/>
	public class ReadOnlyScope : IDisposable
	{
		private PXCache[] _caches;
		private bool[] _isDirty;

		public ReadOnlyScope(params PXCache[] caches)
		{
			_caches = caches;
			_isDirty = new bool[_caches.Length];

			for (int i = 0; i < _caches.Length; i++)
			{
				_isDirty[i] = _caches[i].IsDirty;
			}
		}

		void IDisposable.Dispose()
		{
			for (int i = 0; i < _caches.Length; i++)
			{
				_caches[i].IsDirty = _isDirty[i];
			}
		}
	}
	#endregion

	#region KeepCurrentScope
	/// <exclude/>
	public class ReplaceCurrentScope : IDisposable
	{
		private List<PXCache> _caches;
		private List<object> _currents;

		public ReplaceCurrentScope(IEnumerable<KeyValuePair<PXCache, object>> caches)
		{
			_caches = new List<PXCache>();
			_currents = new List<object>();

			foreach (var item in caches)
			{
				_caches.Add(item.Key);
				_currents.Add(item.Key.Current);
				item.Key.Current = item.Value;
			}
		}

		void IDisposable.Dispose()
		{
			for (int i = 0; i < _caches.Count; i++)
			{
				_caches[i].Current = _currents[i];
			}
		}
	}
	#endregion

	#region RestoreScope
	/// <exclude/>
	public class RestoreScope : IDisposable
	{
		private interface IRestorable
		{
			void Restore();
		}
		private class Restorable<T> : IRestorable
		{
			public T restoreTo;
			public Action<T> setter;

			public void Restore()
			{
				setter(restoreTo);
			}
		}

		public RestoreScope RestoreTo<T>(Action<T> set, T to)
		{
			var restorable = new Restorable<T>();
			restorable.setter = set;
			restorable.restoreTo = to;
			_ro.Add(restorable);
			return this;
		}

		void IDisposable.Dispose()
		{
			foreach (var o in _ro)
				o.Restore();
		}

		List<IRestorable> _ro = new List<IRestorable>();
	}
	#endregion

	#region PXSuppressUIUpdateAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property)]
	public class PXNoUpdateAttribute : PXEventSubscriberAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.FieldVerifyingEvents[_FieldName.ToLower()] += delegate(PXCache cache, PXFieldVerifyingEventArgs e)
			{
				if (e.Row != null && e.ExternalCall)
				{
					e.NewValue = cache.GetValue(e.Row, _FieldOrdinal);
					e.Cancel = true;
				}
			};
		}
	}
	#endregion

	#region PXCheckUnique
	/// <summary>Ensures that a DAC field has distinct values in all data records in a given context.</summary>
	/// <remarks>
	///   <para>The attribute is placed on the declaration of a DAC field, and ensures that this field has a unique value within the current context.</para>
	///   <para>The functionality of the attribute can be implemented through other ways. The use of the attribute for imposing constraint of a key field is obsolete. You
	/// should use the <tt>IsKey</tt> property of the data type attribute for this purpose.</para>
	/// </remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXDBString(30, IsKey = true)]
	/// [PXUIField(DisplayName = "Mailing ID")]
	/// [PXCheckUnique]
	/// public override string NotificationCD { get; set; }</code>
	/// </example>
	public class PXCheckUnique : PXEventSubscriberAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber, IPXRowPersistingSubscriber
	{
		/// <summary>
		/// The additional <tt>Where</tt> clause that filters the data records
		/// that are selected to check uniqueness of the field value among them.
		/// </summary>
		public Type Where;

		protected string[] _UniqueFields;
		protected PXView _View;

		private const string DefaultErrorMessage = ErrorMessages.DuplicateEntryAdded;

		/// <summary>Initializes a new instance of the attribute.</summary>
		/// <param name="fields">Fields. The parameter is optional.</param>
		public PXCheckUnique(params Type[] fields)
		{
			_UniqueFields = new string[fields.Length + 1];
			for (int i = 0; i < fields.Length; i++)
				_UniqueFields[i] = fields[i].Name;
		}

		/// <exclude/>
		public bool IgnoreNulls { get; set; } = true;

		public bool UniqueKeyIsPartOfPrimaryKey { get; set; }
		public bool IgnoreDuplicatesOnCopyPaste { get; set; }

		/// <summary>
		/// Gets of sets the value that indicates whether the field value
		/// is cleared when it duplicates a value in another data record.
		/// By default, the property equals <tt>true</tt>.
		/// </summary>
		public bool ClearOnDuplicate { get; set; } = true;


		private string _errorMessage;
		/// <summary>
		/// Gets or sets the value of custom error message.
		/// If message is not set, then default message will be shown.
		/// </summary>
		public string ErrorMessage
		{
			get { return _errorMessage ?? DefaultErrorMessage; }
			set { _errorMessage = value; }
		}

		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, OnFieldDefaulting);
			_UniqueFields[_UniqueFields.Length - 1] = _FieldName;
			Type sourceType = sender.GetItemType();
			Type where = Where ?? typeof(Where<True, Equal<True>>);

			for (int i = 0; i < _UniqueFields.Length; i++)
			{
				Type field = sender.GetBqlField(_UniqueFields[i]);
				where = BqlCommand.Compose(
					typeof(Where2<,>),
						typeof(Where<,,>),
						field, typeof(IsNull), typeof(And<,,>),
						typeof(Current<>), field, typeof(IsNull), typeof(Or<,>),
						field, typeof(Equal<>), typeof(Current<>), field,
					typeof(And<>), where
					);
			}
			Type command = BqlCommand.Compose(typeof(Select<,>), sourceType, where);

			_View = new PXView(sender.Graph, false,
				BqlCommand.CreateInstance(command));
		}

		#region IPXRowInsertingSubscriber Members

		/// <exclude/>
		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null)
			{
				if (!IgnoreNulls && _UniqueFields.Any(field => sender.GetValue(e.Row, field) == null))
					return;

				e.Cancel = !ValidateDuplicates(sender, e.Row, null);
			}
		}

		#endregion

		#region IPXRowUpdatingSubscriber Members

		/// <exclude/>
		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			ClearErrors(sender, e.NewRow);
			if (e.Row != null && e.NewRow != null && CheckUpdated(sender, e.Row, e.NewRow))
				e.Cancel = !ValidateDuplicates(sender, e.NewRow, e.Row);

			if (ClearOnDuplicate && CheckEquals(sender.GetValue(e.Row, _FieldName), sender.GetValue(e.NewRow, _FieldName)) && e.Cancel)
			{
				ClearErrors(sender, e.NewRow);
				sender.SetValue(e.NewRow, _FieldName, null);
				e.Cancel = !ValidateDuplicates(sender, e.NewRow, e.Row);
			}
		}

		#endregion

		#region IPXRowPersistingSubscriber Members

		/// <exclude/>
		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && e.Operation != PXDBOperation.Delete)
				e.Cancel = !ValidateDuplicates(sender, e.Row, null);
		}

		#endregion

		private void ClearErrors(PXCache sender, object row)
		{
			foreach (string field in _UniqueFields)
			{
				string errorText = PXUIFieldAttribute.GetError(sender, row, field);
				if (!string.IsNullOrEmpty(errorText) && CanClearError(errorText))
				{
					PXUIFieldAttribute.SetError(sender, row, field, null);
				}
			}
		}

		protected virtual Boolean CanClearError(String errorText) => PXMessages.Localize(ErrorMessage).EndsWith(errorText);

		protected virtual void OnFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (callInprocess) return;

			if (e.Cancel != true)
			{
				callInprocess = true;
				object value = null;
				sender.RaiseFieldDefaulting(_FieldName, e.Row, out value);
				if (value != null)
				{
					object copy = sender.CreateCopy(e.Row);
					sender.SetValue(copy, _FieldName, value);
					e.NewValue = (ValidateDuplicates(sender, copy, null) || !ClearOnDuplicate) ? value : null;
					e.Cancel = true;
				}
				callInprocess = false;
			}
		}

		private bool callInprocess;

		private bool CheckUpdated(PXCache sender, object row, object newRow)
		{
			foreach (string field in _UniqueFields)
			{
				if (!CheckEquals(sender.GetValue(row, field), sender.GetValue(newRow, field)))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Checks whether the provided objects are equal, ignoring the case
		/// if the provided objects are strings.
		/// </summary>
		/// <param name="v1">The first object to compare.</param>
		/// <param name="v2">The second object to compare.</param>
		/// <returns></returns>
		public static bool CheckEquals(object v1, object v2)
		{
			return (v1 is string || v2 is string) ?
				string.Compare((string)v1, (string)v2, true) == 0 :
				Object.Equals(v1, v2);
		}

		private bool CheckDefaults(PXCache sender, object row)
		{
			foreach (string field in _UniqueFields)
			{
				bool isnull = false;
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(row, field))
				{
					if (attr is PXDefaultAttribute && ((PXDefaultAttribute)attr).PersistingCheck != PXPersistingCheck.Nothing)
					{
						isnull = sender.GetValue(row, field) == null;
						break;
					}
				}
				if (isnull)
					return false;
			}
			return true;
		}

		private bool ValidateDuplicates(PXCache sender, object row, object oldRow)
		{
			if (!IgnoreNulls || CheckDefaults(sender, row) && sender.GetValue(row, _FieldOrdinal) != null)
				foreach (object sibling in _View.SelectMultiBound(new object[] { row }))
				{
					var prepareMsgLazy = Lazy.By(() => PrepareMessage(sender, row, sibling));
					if (sender.ObjectsEqual(sibling, row) == false || ReferenceEquals(sibling, row) == false && UniqueKeyIsPartOfPrimaryKey && sender.GetStatus(row) != PXEntryStatus.Inserted)
					{
						foreach (string field in _UniqueFields)
						{
							if (oldRow == null ||
								!CheckEquals(sender.GetValue(row, field), sender.GetValue(oldRow, field)))
							{
								PXFieldState state = sender.GetValueExt(row, field) as PXFieldState;
								sender.RaiseExceptionHandling(
									field,
									row, state != null ? state.Value : sender.GetValue(row, field),
									new PXSetPropertyException(prepareMsgLazy.Value));
							}
						}
						return IgnoreDuplicatesOnCopyPaste && sender.Graph.IsCopyPasteContext;
					}
				}
			return true;
		}

		protected virtual String PrepareMessage(PXCache cache, object currentRow, object duplicateRow) => ErrorMessage;
	}
	#endregion

	#region PXShortCutAttribute

	/// <exclude/>
	public struct HotKeyInfo
	{
		private readonly bool _ctrlKey;
		private readonly bool _shiftKey;
		private readonly bool _altKey;
		private readonly int[] _charCodes;
		private readonly int _keyCode;

		private string _toString;

		public static readonly HotKeyInfo Empty;

		static HotKeyInfo()
		{
			Empty = new HotKeyInfo();
		}

		public HotKeyInfo(bool ctrl, bool shift, bool alt, int keyCode, int[] charCodes)
		{
			if (!ctrl && !shift && !alt)
				throw new ArgumentException("At least one of special keys (Ctrl, Shift, Alt) must be set.");
			if (keyCode == 0 && (charCodes == null || charCodes.Length == 0))
				throw new ArgumentException("A shortcut must contain a functional key or at least one char.");
			_ctrlKey = ctrl;
			_shiftKey = shift;
			_altKey = alt;
			_charCodes = charCodes ?? new int[0];
			_keyCode = keyCode;

			_toString = null;
		}

		public bool CtrlKey
		{
			get { return _ctrlKey; }
		}

		public bool ShiftKey
		{
			get { return _shiftKey; }
		}

		public bool AltKey
		{
			get { return _altKey; }
		}

		public int[] CharCodes
		{
			get { return _charCodes; }
		}

		public int KeyCode
		{
			get { return _keyCode; }
		}

		public override string ToString()
		{
			if (_toString == null && _charCodes != null)
			{
				var sb = new StringBuilder(_charCodes.Length * 4 + 5);
				if (CtrlKey) sb.Append("Ctrl");
				if (AltKey)
				{
					if (sb.Length > 0) sb.Append(" + ");
					sb.Append("Alt");
				}
				if (ShiftKey)
				{
					if (sb.Length > 0) sb.Append(" + ");
					sb.Append("Shift");
				}
				if (KeyCode > 0)
				{
					if (sb.Length > 0) sb.Append(" + ");
					var name = Enum.GetName(typeof(PX.Export.KeyCodes), KeyCode);
					sb.Append(string.IsNullOrEmpty(name) ? KeyCode.ToString() : name);
				}
				foreach (char c in _charCodes)
				{
					sb.Append(" + ");
					sb.Append(c);
				}
				_toString = sb.ToString();
			}
			return _toString;
		}

		public static string ConvertCharCodes(string str)
		{
			var sb = new StringBuilder(str.Length * 4);
			foreach (char c in str)
			{
				if (sb.Length > 0) sb.Append(" + ");
				sb.Append(c);
			}
			return sb.ToString();
		}

		public bool Equals(HotKeyInfo other)
		{
			return other._ctrlKey.Equals(_ctrlKey) && other._shiftKey.Equals(_shiftKey) &&
				other._altKey.Equals(_altKey) && Equals(other._charCodes, _charCodes) &&
				other._keyCode == _keyCode && Equals(other._toString, _toString);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof(HotKeyInfo)) return false;
			return Equals((HotKeyInfo)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _ctrlKey.GetHashCode();
				result = (result * 397) ^ _shiftKey.GetHashCode();
				result = (result * 397) ^ _altKey.GetHashCode();
				result = (result * 397) ^ (_charCodes != null ? _charCodes.GetHashCode() : 0);
				result = (result * 397) ^ _keyCode;
				result = (result * 397) ^ (_toString != null ? _toString.GetHashCode() : 0);
				return result;
			}
		}

		public static bool operator ==(HotKeyInfo left, HotKeyInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HotKeyInfo left, HotKeyInfo right)
		{
			return !left.Equals(right);
		}

		public static int[] ConvertChars(char[] data)
		{
			var result = new int[data.Length];
			for (int i = 0; i < data.Length; i++)
				result[i] = (int)data[i];
			return result;
		}
	}

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class PXShortCutAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private static readonly IDictionary<Type, IDictionary<string, PXShortCutAttribute>> _commands;

		private readonly HotKeyInfo _shortcut;

		static PXShortCutAttribute()
		{
			_commands = new Dictionary<Type, IDictionary<string, PXShortCutAttribute>>();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					// ignore some assemlies, prevent from raising exceptions on dynamic assemblies as well
					if (!PXSubstManager.IsSuitableTypeExportAssembly(assembly) || assembly.FullName.StartsWith("App_SubCode_"))
						continue;

					Type[] types = null;
					try
					{
						if (!assembly.IsDynamic)
							types = assembly.GetExportedTypes();
					}
					catch (ReflectionTypeLoadException te)
					{
						types = te.Types;
					}
					if (types != null)
					{
						foreach (Type type in types)
						{
							if (type != null && (type.IsGenericType || type.IsAbstract || !typeof(PXGraph).IsAssignableFrom(type))) continue;

							IDictionary<string, PXShortCutAttribute> typeCommands = null;
							foreach (MethodInfo method in type.GetMethods())
							{
								ParameterInfo[] parameters;
								if (!typeof(IEnumerable).IsAssignableFrom(method.ReturnType) ||
									(parameters = method.GetParameters()) == null ||
									parameters.Length == 0 ||
									!typeof(PXAdapter).IsAssignableFrom(parameters[0].ParameterType))
								{
									continue;
								}
								var atts = method.GetCustomAttributes(typeof(PXShortCutAttribute), false);
								if (atts == null || atts.Length == 0) continue;
								if (typeCommands == null)
								{
									if (!_commands.TryGetValue(type, out typeCommands))
										_commands.Add(type, typeCommands = new Dictionary<string, PXShortCutAttribute>());
									typeCommands.Clear();
								}
								foreach (PXShortCutAttribute attribute in atts)
								{
									typeCommands.Remove(method.Name);
									typeCommands.Add(method.Name, attribute);
								}
							}
						}
					}
				}
				catch (StackOverflowException) { throw; }
				catch (OutOfMemoryException) { throw; }
				catch { }
			}
		}

		private PXShortCutAttribute(bool ctrl, bool shift, bool alt, int keyCode, int[] charCodes)
		{
			_shortcut = new HotKeyInfo(ctrl, shift, alt, keyCode, charCodes);
		}

		/// 
		public PXShortCutAttribute(bool ctrl, bool shift, bool alt, PX.Export.KeyCodes key)
			: this(ctrl, shift, alt, (int)key, null) { }

		/// 
		public PXShortCutAttribute(bool ctrl, bool shift, bool alt, params char[] chars)
			: this(ctrl, shift, alt, 0, HotKeyInfo.ConvertChars(chars)) { }

		public static IEnumerable<KeyValuePair<string, PXShortCutAttribute>> GetDeclared(string graphName)
		{
			IDictionary<string, PXShortCutAttribute> typeCommands;
			var type = System.Web.Compilation.PXBuildManager.GetType(graphName, false);
			if (type != null && _commands.TryGetValue(type, out typeCommands))
				foreach (KeyValuePair<string, PXShortCutAttribute> command in typeCommands)
					yield return command;
		}

		#region IPXFieldSelectingSubscriber Members

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXButtonState.CreateInstance(e.ReturnState, null, null, null, null, null, null,
				PXConfirmationType.Unspecified, null, null, null, null, null, null, null, null, null, null, this, null);
		}

		#endregion

		/// <summary>Get.</summary>
		public HotKeyInfo HotKey
		{
			get { return _shortcut; }
		}
		}

	#endregion

	#region PXDefaultValidateAttribute
	/// <exclude/>
	public class PXDefaultValidateAttribute : PXDefaultAttribute
	{
		public PXDefaultValidateAttribute(Type sourceType, Type validateExists)
			: base(sourceType)
		{
			this.validateExists = BqlCommand.CreateInstance(validateExists);
		}
		private readonly BqlCommand validateExists;

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);
			if (e.NewValue != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(validateExists, false);
				int startRow = -1;
				int totalRows = 0;
				List<object> source = view.Select(
					new object[] { e.Row },
					new object[] { e.NewValue },
					null,
					null,
					null,
					null,
					ref startRow,
					1,
					ref totalRows);
				if (source != null && source.Count > 0)
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			}
		}
	}
	#endregion

	#region PXCustomStringListAttribute
	/// <exclude/>
	public class PXCustomStringListAttribute : PXStringListAttribute
	{
		/// <summary>Get.</summary>
		public string[] AllowedValues
		{
			get
			{
				return _AllowedValues;
			}
		}

		/// <summary>Get.</summary>
		public string[] AllowedLabels
		{
			get
			{
				return _AllowedLabels;
			}
		}

		public PXCustomStringListAttribute(string[] AllowedValues, string[] AllowedLabels)
			: base(AllowedValues, AllowedLabels)
		{
		}
	}
	#endregion

	#region PXDependsOnFieldsAttribute
	/// <summary>Used for calculated DAC fields that contain references to other fields in their property getters. The attribute allows such fields to work properly in reports
	/// and Integration Services.</summary>
	/// <example>
	/// The code below shows definition of a calculated DAC field. The property getter involves two fields, <tt>DocBal</tt> and <tt>TaxWheld</tt>. These two fields
	/// should be specified as parameters of the <tt>PXDependsOnFields</tt> attribute.
	/// <code title="" description="" lang="CS">
	/// [PXDefault(TypeCode.Decimal, "0.0")]
	/// [PXUIField(DisplayName = "Balance")]
	/// public virtual Decimal? ActualBalance
	/// {
	///     [PXDependsOnFields(typeof(docBal), typeof(taxWheld))]
	///     get
	///     {
	///         return this.DocBal - this.TaxWheld;
	///     }
	/// }</code><code title="Example" description="The following code displays two values in one box in the UI: the number of rejected requests and the percent of this number in all requests." lang="CS">
	/// [PXString]
	/// [PXUIField(DisplayName = "Nbr. of Declined Web Services API Requests (% of All)")]
	/// [PXDependsOnFields(typeof(UsrSMLicenseStatistic.rejectedAPICount), typeof(UsrSMLicenseStatistic.rejectedAPICountP))]
	/// public virtual string RejectedAPICountPW
	/// {
	///     get
	///     {
	///         return (string.Format("{0} ({1}%)", RejectedAPICount, RejectedAPICountP));
	/// }
	/// }</code></example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class PXDependsOnFieldsAttribute : Attribute
	{
		private Type[] _fields;
		/// <summary>
		/// Initializes an instance of the attribute that makes the field the
		/// attribute is attached to depend on the provided DAC fields.
		/// </summary>
		/// <param name="fields">The fields to depend to.</param>
		public PXDependsOnFieldsAttribute(params Type[] fields)
		{
            foreach(var field in fields)
                if(!typeof(IBqlField).IsAssignableFrom(field))
                    throw new PXArgumentException(nameof(field), ErrorMessages.InvalidField, field.Name);
			_fields = fields;
		}

		internal static HashSet<string> GetDependsRecursive(PXCache table, string field)
		{
			var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			AddDependsRecursive(table, field, result);
			return result;

		}
		static void AddDependsRecursive(PXCache table, string field, HashSet<string> result)
		{
			if (result.Contains(field))
				return;

            var itemType = table.GetItemType();
			var p = itemType.GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
			if (p == null)
			{
				foreach (Type extension in table.GetExtensionTypes())
				{
					p = p ?? extension.GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
				}
				if (p == null)
					return;
			}

			result.Add(p.Name);
			PXDependsOnFieldsAttribute attr = (PXDependsOnFieldsAttribute)Attribute.GetCustomAttribute(p, typeof(PXDependsOnFieldsAttribute));
			if (attr == null)
			{

				var getter = p.GetGetMethod();
				attr = (PXDependsOnFieldsAttribute)Attribute.GetCustomAttribute(getter, typeof(PXDependsOnFieldsAttribute));

			}
			List<Type> fields = new List<Type>();
			if (attr != null)
				fields.AddRange(attr._fields);

			foreach (PXEventSubscriberAttribute fattr in table.GetAttributesReadonly(p.Name, true))
			{
				IPXDependsOnFields fDepAttr = fattr as IPXDependsOnFields;
				if (fDepAttr != null)
					fields.AddRange(fDepAttr.GetDependencies(table));
			}
            fields
                .Distinct()
                .Where(ft => BqlCommand.GetItemType(ft).IsAssignableFrom(itemType))
                .ForEach(ft => AddDependsRecursive(table, ft.Name, result));
		}
	}

	#endregion

	#region IPXReportRequiredField
	/// <exclude/>
	public interface IPXReportRequiredField { }

	#endregion

	/// <summary>Sets a dropdown list as the input control for a DAC field of
	/// <tt>decimal</tt> type.</summary>
	/// <remarks>
	/// <para>The user will be able to select a value from the predefined
	/// values list. Values are specified in the constructor as strings,
	/// because the attribute derives from <tt>PXStringList</tt>. The
	/// attribute converts a selected value to the <tt>decimal</tt> type that
	/// is assigned to the field.</para>
	/// <para>The DAC field data type must be defined using the <see
	/// cref="PXDBDecimalAttribute">PXDBDecimalString</see>
	/// attribute.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// [PXDecimalList(
	///     new string[] { "0.1", "0.5", "1.0", "10", "100" },
	///     new string[] { "0.1", "0.5", "1.0", "10", "100" })]
	/// public virtual decimal? InvoicePrecision { get; set; }
	/// </code>
	/// </example>
	public class PXDecimalListAttribute : PXStringListAttribute
	{
		/// <summary>
		/// Initializes a new instance with the provided lists of allowed values
		/// and labels. When a user selects a label in the user interface, the
		/// corresponding value is converted to <tt>decimal</tt> type and assigned to the
		/// field marked by the instance. The two lists must be of the same length.
		/// </summary>
		/// <param name="allowedValues">The array of string values the user will be able
		/// to select from. A string value is converted by the attribute to the decimal
		/// value.</param>
		/// <param name="allowedLabels">The array of labels corresponding to values and
		/// displayed in the user interface.</param>
		public PXDecimalListAttribute(string[] allowedValues, string[] allowedLabels)
			: base(allowedValues, allowedLabels)
		{
			IsLocalizable = false;
		}

		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			//if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string[] values = Array.ConvertAll<string, string>(_AllowedValues, delegate(string a)
				{
					decimal val = Decimal.Parse(a, NumberStyles.Any, CultureInfo.InvariantCulture);
					return val.ToString("F2", sender.Graph.Culture);
				}
				);

				string[] labels = Array.ConvertAll<string, string>(_AllowedLabels, delegate(string a)
				{
					decimal val = Decimal.Parse(a, NumberStyles.Any, CultureInfo.InvariantCulture);
					return val.ToString("F2", sender.Graph.Culture);
				}
				);

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, values, labels, _ExclusiveValues, null);
			}
		}
	}

	/// <exclude/>
	public class PXDateListAttribute : PXStringListAttribute
	{
		public PXDateListAttribute(string[] allowedValues, string[] allowedLabels)
			: base(allowedValues, allowedLabels)
		{
		}
		
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
				string[] values = Array.ConvertAll<string, string>(_AllowedValues, delegate(string a)
				{
					DateTime val = DateTime.Parse(a, CultureInfo.InvariantCulture, DateTimeStyles.None);
				return val.ToString("d", CultureInfo.InvariantCulture);
			});

			// The culture to use for labels: on Import/Export API process, the 
			// graph culture is not invariant, so use that. During normal interaction
			// process, the graph culture is always invarian (2015.12), so use the current UI 
			// culture instead, which knows about localisations / user preferences 
			// - 
			CultureInfo labelsCulture = Equals(sender.Graph.Culture, CultureInfo.InvariantCulture)
				? LocaleInfo.GetUICulture()
				: sender.Graph.Culture;

				string[] labels = Array.ConvertAll<string, string>(_AllowedLabels, delegate(string a)
				{
					DateTime val = DateTime.Parse(a, CultureInfo.InvariantCulture, DateTimeStyles.None);
				return val.ToString("d", labelsCulture);
			});

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, values, labels, _ExclusiveValues, null);
			}
		}


	#region PXAttributeFamilyAttribute
	/// <summary>Allows to specify rules, which attributes can not be combined
	/// together.</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class PXAttributeFamilyAttribute : Attribute
	{
		/// 
		public PXAttributeFamilyAttribute(Type rootType)
		{
			RootType = rootType;

		}
		public readonly Type RootType;
		private static readonly Type[] Empty = new Type[0];

		/// 
		public static Type[] GetRoots(Type t)
		{


			foreach (var parent in t.CreateList(_ => _.BaseType))
			{
				var list = (PXAttributeFamilyAttribute[])Attribute.GetCustomAttributes(parent, typeof(PXAttributeFamilyAttribute));
				if (list.Any())
					return list.Select(_ => _.RootType).ToArray();

			}



			return Empty;

		}

		public static bool IsSameFamily(Type a, Type b)
		{
			var aRoots = GetRoots(a);
			if (aRoots.IsNullOrEmpty())
				return false;

			var bRoots = GetRoots(b);
			if (bRoots.IsNullOrEmpty())
				return false;

			return aRoots.Any(bRoots.Contains);


		}

		/// 
		public static PXAttributeFamilyAttribute FromType(Type t)
		{
			foreach (var parent in t.CreateList(_ => _.BaseType))
			{
				var a = (PXAttributeFamilyAttribute)Attribute.GetCustomAttribute(parent, typeof(PXAttributeFamilyAttribute));
				if (a != null)
					return a;

			}

			return null;

		}

		/// 
		public static void CheckAttributes(PropertyInfo prop, PXEventSubscriberAttribute[] attributes)
		{
			var groups = (from a in attributes
						  from t in GetRoots(a.GetType())
						  select new { t, a })
				.ToLookup(_ => _.t, _ => _.a);

			//var groups = attributes.ToLookup(a => GetFamilyRoot(a.GetType()));
			foreach (var g in groups)
			{
				if (g.Key == null)
					continue;

				if (g.Count() > 1)
				{
					PXValidationWriter.AddTypeError(prop.DeclaringType, "Incompatible attributes have been detected. Family: {0}, Property {1}::{2}, Attributes: {3}", g.Key.Name, prop.DeclaringType, prop.Name, g.Select(_ => _.GetType().Name).JoinToString(","));
				}


			}
		}
	}


	/// <exclude/>
	public class PXValidationWriter
	{
		public static readonly bool? PageValidation = GetPageValidation();

		static bool? GetPageValidation()
		{
			string ValidationPolicyConfig = System.Web.Configuration.WebConfigurationManager.AppSettings["PageValidation"];
			if (String.IsNullOrEmpty(ValidationPolicyConfig))
				return null;

			return Convert.ToBoolean(ValidationPolicyConfig);

		}

		//public static readonly PXValidationWriter Current = new PXValidationWriter();



		public static readonly Dictionary<Type, HashSet<string>> TypeErrors = new Dictionary<Type, HashSet<string>>();
		public static void AddTypeError(Type t, string format, params object[] args)
		{
			if (PageValidation == false)
				return;
			if (!TypeErrors.ContainsKey(t))
				TypeErrors.Add(t, new HashSet<string>());

			TypeErrors[t].Add(String.Format(format, args));

		}

	}
	#endregion

	#region PXFeatureAttribute
	/// <exclude/>
	public class PXFeatureAttribute : Attribute
	{
		public readonly Type Feature;
		public PXFeatureAttribute(Type feature)
		{
			this.Feature = feature;
		}
	}
	#endregion

	#region PXDynamicButtonAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Class)]
	public class PXDynamicButtonAttribute : Attribute
		{
		public string[] buttonNames;
		public string[] displayNames;
		public Type TranslationKeyType { get; set; }

		public PXDynamicButtonAttribute(string[] dynamicButtonNames, string[] dynamicButtonDisplayNames)
		{
			buttonNames = dynamicButtonNames;
			displayNames = dynamicButtonDisplayNames;
		}

		public List<PXActionInfo> DynamicActions
		{
			get
		{
				List<PXActionInfo> actions = new List<PXActionInfo>();
				if (buttonNames != null)
				{
					for (int i = 0; i < buttonNames.Length; i++)
			{
						string actionName = buttonNames[i];
						if (!string.IsNullOrEmpty(actionName))
			{
							string displayName = GetActionDisplayName(i);
							PXActionInfo newAction = new PXActionInfo(actionName, displayName);
							actions.Add(newAction);
			}
			}
		}
				return actions;
			}
		}

		public virtual List<PXActionInfo> GetDynamicActions(Type graphType, Type viewType)
		{
			return DynamicActions;
		}

		private string GetActionDisplayName(int buttonNameIndex)
		{
			string displayName = buttonNames[buttonNameIndex];

			if (displayNames != null && buttonNameIndex <= displayNames.Length - 1 && !string.IsNullOrEmpty(displayNames[buttonNameIndex]))
				{
				displayName = TranslationKeyType == null ? PXMessages.Localize(displayNames[buttonNameIndex]) : PXLocalizer.Localize(displayNames[buttonNameIndex], TranslationKeyType.FullName);
				}

			return displayName;
		}
	}
	#endregion

	#region PXUniqueAttribute

	/// <summary>
	/// Applies application-side unique constraint on a field.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class PXUniqueAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public PXUniqueAttribute()
		{
		}

		public PXUniqueAttribute(Type sourceType)
		{
			if (typeof(IBqlTable).IsAssignableFrom(sourceType))
			{
				_sourceType = sourceType;
			}
			else
			{
				throw new PXArgumentException("sourceType", ErrorMessages.CantCreateForeignKeyReference, sourceType);
			}
		}

		private PXView _allRecordsView;
		private Type _sourceType;

		/// <summary>
		/// Determines if need to skip nulls. False to treat null as unique value.
		/// True by default.
		/// </summary>
		private bool _allowNulls = true;

		public bool AllowNulls
		{
			get { return _allowNulls; }
			set { _allowNulls = value; }
		}

		private string _errorMessage = ErrorMessages.ValueIsNotUnique;

		public string ErrorMessage
		{
			get { return _errorMessage; }
			set { _errorMessage = value; }
		}

		/// <exclude />
		public override void CacheAttached(PXCache sender)
		{
			if (_sourceType != null)
				return;

			string typeName = char.ToLower(_FieldName[0]) + _FieldName.Substring(1);
			var fieldType = BqlTable.GetNestedType(typeName);
			if (fieldType == null)
				throw new PXException(MsgNotLocalizable.NoNestedClassInDacForThisField);
			if (!typeof(IBqlField).IsAssignableFrom(fieldType))
				throw new PXException(string.Format(MsgNotLocalizable.TypeNotImplementIBqlFieldInterface, typeName));

			BqlCommand command = BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), BqlTable,
					typeof(Where<,>), fieldType, typeof(Equal<>), typeof(Required<>), fieldType));

			_allRecordsView = new PXView(sender.Graph, false, command);
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null
				&& ((e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert
					|| (e.Operation & PXDBOperation.Update) == PXDBOperation.Update))
			{
				object search = sender.GetValue(e.Row, _FieldName);
				if (search != null || !AllowNulls)
				{
					IEnumerable<object> records = null;
					if (_sourceType == null) 
					{
						int startRow = 0, maxRows = 2, totalRows = 0;
						records = _allRecordsView.Select(null, new[] { search }, null, null, null, null, ref startRow, maxRows, ref totalRows);
					}
					else
					{
						records = PXParentAttribute.SelectSiblings(sender, e.Row, _sourceType);
					}	
					
					bool errorOccured = false;
					foreach (object record in records)
					{
						var fieldValue = sender.GetValue(record, _FieldName);
						if (!object.Equals(search, fieldValue))
							continue;

						if (e.Row != record) // if it's not the same record
						{
							errorOccured = true;
							break;
						}
					}
					if (errorOccured)
						sender.RaiseExceptionHandling(_FieldName, e.Row, search, new PXSetPropertyException(ErrorMessage, _FieldName));
				}
			}
		}
	}

	#endregion

	#region PXPutButtonAttribute
	/// <summary>
	/// When we generate controls based on the DAC fields we can use this attribute to establish controls order. 
	/// Fields marked with PXGenerateAfterAttribute will come right after the field passed as an argument to PXGenerateAfterAttribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class PXGenerateAfterAttribute : Attribute
	{
		public readonly string FieldToFollow;
		public PXGenerateAfterAttribute(string fieldName)
		{
			if (string.IsNullOrEmpty(fieldName)) throw new PXArgumentException(fieldName, Messages.FieldNameMustBeSet);
			FieldToFollow = fieldName;
		}
		public static List<string> GetSortedFieldList(PXCache cache)
		{
			var regularList = new List<string>();
			var afterMap = new List<Tuple<string, string>>();
			var itemType = cache.GetItemType();
			var result = new List<string>();
			foreach (string fieldName in cache.Fields)
			{
				string followField = itemType.GetProperty(fieldName)?.GetCustomAttribute<PXGenerateAfterAttribute>()?.FieldToFollow;

				if (string.IsNullOrEmpty(followField)) result.Add(fieldName);
				else {
					if (cache.Fields.Contains(followField))
						afterMap.Add(Tuple.Create(followField, fieldName));
					else
						throw new PXArgumentException(fieldName, Messages.NoSuchDacFieldHasBeenFound);
				}
			}
			while (afterMap.Count > 0)
			{
				var newAfterMap = new List<Tuple<string, string>>();
				for (var i = afterMap.Count - 1; i >= 0; i--)
				{
					var tuple = afterMap[i];
					int index = result.IndexOf(tuple.Item1);
					if (index > -1) result.Insert(index + 1, tuple.Item2);
					else newAfterMap.Add(tuple);
				}
				afterMap = newAfterMap;
			}
			return result;
		}
	}
	#endregion
}
