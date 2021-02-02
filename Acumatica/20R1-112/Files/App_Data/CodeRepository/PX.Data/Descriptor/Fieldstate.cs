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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Web.UI.Design;
using System.Collections.Concurrent;

namespace PX.Data
{
    /// <summary>Is used to define the visibility of input controls.</summary>
    /// <remarks>This enumeration is used to define the following:
    /// <list type="bullet">
    /// <item><description>The visibility of an input
    /// control or a grid column at runtime</description></item>
    /// <item><description>A hint as to which columns you should
    /// include in the grid while selecting them in the webpage
    /// layout designer</description></item>
    /// <item><description>The default set of
    /// columns displayed in the pop-up of the <tt>PXSelector</tt> input
    /// control</description></item>
    /// <item><description>The set of columns
    /// automatically added to the <tt>PXGrid</tt> control with the
    /// <tt>AutoGenerateColumns</tt> property set to <tt>AppendDynamic</tt>
    /// if no appropriate columns are defined for the <tt>PXGrid</tt>
    /// control</description></item>
    /// </list>
    /// </remarks>
    [Flags]
    public enum PXUIVisibility
    {
        /// <exclude />
        Undefined = 0,
        /// <summary>The field input control or column is hidden.</summary>
        /// <remarks>Also, when you select columns for a grid
        /// in the webpage layout designer, the field column is not recommended to
        /// be included.</remarks>
        Invisible = 1,
        /// <summary>The field input control or column is visible.</summary>
        /// <remarks>Also, when you select columns for a grid
        /// in the webpage layout designer, the field column is recommended to
        /// be included.</remarks>
        Visible = 2 | Invisible,
        /// <summary>The field column is
        /// added to the <tt>PXSelector</tt> lookup control when the
        /// <tt>PXSelector</tt> attribute does not define the set of columns
        /// explicitly.</summary>
        SelectorVisible = 4 | Visible,
        /// <summary>
        /// The field column is automatically added to the <tt>PXGrid</tt> control
        /// if the <tt>AutoGenerateColumns</tt> property of the grid is set to <tt>AppendDynamic</tt>.
        /// </summary>
        Dynamic = 8 | Invisible,
        /// <exclude />
        Service = 16 | Visible,
        /// <exclude />
        HiddenByAccessRights = 32 | Invisible,
    }

	/// <exclude/>
	[Flags]
	public enum PXSelectorMode
	{
		Undefined = 0,
		NoAutocomplete = 1,
		MaskAutocomplete = 32,
		AutocompleteMode = MaskAutocomplete | NoAutocomplete,
		TextModeReadonly = 2,
		TextModeEditable = 4,
		TextModeSearch = 6,
		TextMode = TextModeSearch,
		DisplayModeHint = 0,
		DisplayModeValue = 8,
		DisplayModeText = 16,
		DisplayMode = DisplayModeValue | DisplayModeText
	}

	/// <summary>This enumeration specifies the level of the <tt>PXSetPropertyException</tt> exception. Depending on the level, different error or warning signs are attached to
	/// UI controls associated with particular fields or rows.</summary>
	public enum PXErrorLevel
	{
		/// <summary>The <tt>Error</tt> sign is attached to the input controls or cells of the DAC fields whose <tt>PXFieldState</tt><tt>Error</tt> property values are not null.</summary>
		Undefined,
		/// <summary>The <tt>Information</tt> sign is attached to a DAC row within the <tt>PXGrid</tt> control.</summary>
		RowInfo,
		/// <summary>The <tt>Warning</tt> sign is attached to a DAC field input control or cell.</summary>
		Warning,
		/// <summary>The <tt>Warning</tt> sign is attached to a DAC row within the <tt>PXGrid</tt> control.</summary>
		RowWarning,
		/// <summary>The <tt>Error</tt> sign is attached to a DAC field input control or a cell.</summary>
		Error,
		/// <summary>The <tt>Error</tt> sign is attached to a DAC row within the <tt>PXGrid</tt> control.</summary>
		RowError
	}

	/// <summary>This enumeration is used in the <see cref="PXUIFieldAttribute">PXUIField</see> attribute to specify when to handle the <tt>PXSetPropertyException</tt> exception related to the
	/// field. If the exception is handled, the user gets a message box with the error description, and the field input control is marked as causing an error.</summary>
	public enum PXErrorHandling
	{
		/// <summary>The exception is reported only when the <tt>PXUIField</tt> attribute with the <tt>Visible</tt> property set to <tt>true</tt> is attached to a DAC field.</summary>
		WhenVisible,
		/// <summary>The exception is always reported by the <tt>PXUIField</tt> attribute attached to a DAC field.</summary>
		Always,
		/// <summary>The exception is never reported by the <tt>PXUIField</tt> attribute attached to a DAC field.</summary>
		Never
	}

	/// <summary>Provides data to set up the presentation of the input control for the DAC field.</summary>
	[DebuggerDisplay("[{DisplayName}] [{ViewName}] {DataType} {Value}")]
	public class PXFieldState : IDataSourceFieldSchema, ICloneable
	{
		object ICloneable.Clone()
		{
			return MemberwiseClone();
		}

		/// <summary>Gets the type of data stored in the field.</summary>
		public virtual Type DataType { get { return _DataType; } }
		/// <summary>Gets the value that indicates (if set to true) that the field is mapped to an identity column in a database table.</summary>
		public virtual bool Identity { get { return false; } }

		/// <summary>Gets the value that indicates (if set to true) that the field is read-only.</summary>
		public virtual bool IsReadOnly
		{
			get { return _IsReadOnly || !_Enabled; }
			internal set { _IsReadOnly = value; }
		}
		/// <summary>Gets the indication of a uniqueness constraint on the field.</summary>
		public virtual bool IsUnique { get { return false; } }
		/// <summary>Gets or sets the storage size of the field.</summary>
		public virtual int Length
		{
			get
			{
				return _Length;
			}
			set
			{
				_Length = value;

			}
		}
		/// <summary>Gets the name of the field.</summary>
		public virtual string Name { get { return _FieldName; } }
		/// <exclude/>
		public virtual void SetAliased(string prefix)
		{
			_FieldName = String.Format("{0}__{1}", prefix, _FieldName);
			_IsKey = false;
		}
		/// <summary>Gets the value that indicates (if set to true) that the field can store the <tt>null</tt> value.</summary>
		public virtual bool Nullable { get { return _Nullable; } internal set { _Nullable = value; } }
		/// <summary>Gets the maximum number of digits used to represent a numeric value stored in the field.</summary>
		public virtual int Precision { get { return _Precision; } }
		/// <summary>Gets the value that indicates (if set to true) that the field is marked as a key field.</summary>
		public virtual bool PrimaryKey { get { return _IsKey; } internal set { _IsKey = value; } }
		/// <summary>Gets the number of digits to the right of the decimal point used to represent a numeric value stored in the field.</summary>
		public virtual int Scale { get { return -1; } }

		/// <summary>Gets or sets the value that indicates (if set to true) that the value of the field is required.</summary>
		public virtual bool? Required
		{
			get
			{
				if (_Required < 0) return false;
				if (_Required > 0) return true;
				return null;
			}
			set
			{
				if (value == true) _Required = 1;
				else if (value == false) _Required = -1;
				else _Required = 0;
			}
		}

		protected object _Value = null;
		protected string _Error = null;
		protected PXErrorLevel _ErrorLevel = PXErrorLevel.Undefined;
		protected bool _Enabled = true;
		protected bool _Visible = true;
		protected bool _IsReadOnly = false;
		protected string _FieldName = null;
		protected string _DescriptionName = null;
		protected internal bool _IsKey = false;
		protected bool _Nullable = true;
		protected int _Precision = -1;
		protected string _DisplayName = null;
		protected PXUIVisibility _Visibility = PXUIVisibility.Invisible;
		protected Type _DataType = typeof(object);
		protected object _DefaultValue = null;
		protected string _ViewName = null;
		protected string[] _FieldList = null;
		protected string[] _HeaderList = null;
		protected int _Length = -1;
		protected int _Required = 0;
		protected string _ValueField;
		protected PXSelectorMode _SelectorMode;

		protected PXFieldState(object value)
		{
			PXFieldState state = value as PXFieldState;
			if (state == null)
			{
				_Value = value;
			}
			else
			{
				_Value = state._Value;
				_DataType = state._DataType;
				_IsKey = state._IsKey;
				_Nullable = state._Nullable;
				_Precision = state._Precision;
				_Length = state._Length;
				_DefaultValue = state._DefaultValue;
				_FieldName = state._FieldName;
				_DescriptionName = state._DescriptionName;
				DisplayName = state._DisplayName;
				_Error = state._Error;
				_ErrorLevel = state._ErrorLevel;
				_Enabled = state._Enabled;
				_Visible = state._Visible;
				_IsReadOnly = state._IsReadOnly;
				_Visibility = state._Visibility;
				_ViewName = state._ViewName;
				_FieldList = state._FieldList;
				_HeaderList = state._HeaderList;
				_Required = state._Required;
				_ValueField = state._ValueField;
				_SelectorMode = state._SelectorMode;
			}
		}

		/// <summary>Gets or sets the value that is stored in the field.</summary>
		public virtual object Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
			}
		}

		/// <summary>Gets or sets the error text assigned to the field.</summary>
		public virtual string Error
		{
			get
			{
				return _Error;
			}
			set
			{
				_Error = value;
			}
		}

		/// <summary>Gets or sets the value that indicates (if set to true) whether the field is marked with the <tt>Warning</tt> sign.</summary>
		public virtual bool IsWarning
		{
			get
			{
				return _ErrorLevel == PXErrorLevel.Warning;
			}
			set
			{
				if (value)
				{
					_ErrorLevel = PXErrorLevel.Warning;
				}
				else if (_ErrorLevel == PXErrorLevel.Warning)
				{
					_ErrorLevel = PXErrorLevel.Error;
				}
			}
		}

		/// <summary>Gets or sets the error level assigned to the field.</summary>
		public virtual PXErrorLevel ErrorLevel
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

		/// <summary>Gets or sets the value that indicates (if set to true) that the current field input control will respond to a user's interaction.</summary>
		public virtual bool Enabled
		{
			get
			{
				return _Enabled;
			}
			set
			{
				_Enabled = value;
			}
		}

		/// <summary>Gets or sets the value that indicates (if set to true) that the current field input control is displayed.</summary>
		public virtual bool Visible
		{
			get
			{
				return _Visible;
			}
			set
			{
				_Visible = value;
			}
		}

		/// <summary>Gets or sets the display name for the field.</summary>
		public virtual string DisplayName
		{
			get
			{
				if (_DisplayName != null)
				{
					return _DisplayName;
				}
				return _FieldName;
			}
			set
			{
				_DisplayName = value;
			}
		}

		/// <summary>Gets or sets the name of a DAC field displayed in the <tt>PXSelector</tt> control of the field if the <tt>DisplayMode</tt> property is set to <tt>Text</tt>. If
		/// the <tt>DisplayMode</tt> property is set to <tt>Hint</tt>, the name is displayed in the <tt>ValueField</tt> - <tt>DescriptionName</tt> format. By default,
		/// <tt>DisplayMode</tt> is set to <tt>Hint</tt>.</summary>
		public virtual string DescriptionName
		{
			get
			{
				return _DescriptionName;
			}
			set
			{
				_DescriptionName = value;
			}
		}

		/// <summary>Gets or sets the <tt>PXUIVisibility</tt> object for the field.</summary>
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

		/// <summary>Gets or sets the default value that is displayed in the field's cell for a new record that is not yet committed to the <tt>PXGraph</tt> instance.</summary>
		public virtual object DefaultValue
		{
			get
			{
				return _DefaultValue;
			}
			set
			{
				_DefaultValue = value;
			}
		}

		/// <summary>Gets or sets the name for the <tt>PXView</tt> object bound to the <tt>PXSelector</tt> field control.</summary>
		public virtual string ViewName
		{
			get
			{
				return _ViewName;
			}
			set
			{
				_ViewName = value;
			}
		}

		/// <summary>Gets or sets the array of DAC fields for the <tt>PXSelector</tt> field control.</summary>
		public virtual string[] FieldList
		{
			get
			{
				return _FieldList;
			}
			set
			{
				_FieldList = value;
			}
		}

		/// <summary>Gets or sets the array of field display names for the <tt>PXSelector</tt> field control.</summary>
		public virtual string[] HeaderList
		{
			get
			{
				if (_HeaderList != null && _FieldList != null && _HeaderList.Length == _FieldList.Length)
				{
					return _HeaderList;
				}
				return _FieldList;
			}
			set
			{
				_HeaderList = value;
			}
		}

		/// <summary>Gets or sets the name of a DAC field, which is: 
		/// <list type="bullet"><item>Displayed in the <tt>PXSelector</tt> field control on focus.</item><item>Used to locate the selected record in the <tt>PXSelector</tt> field control.</item><item>Displayed in the <tt>PXSelector</tt> field control when the <tt>DisplayMode</tt> property is set to <tt>Value</tt>.</item></list></summary>
		public virtual string ValueField
		{
			get { return _ValueField; }
			set { _ValueField = value; }
		}

		/// <summary>Gets or sets the mode of the PXSelector control.</summary>
		public virtual PXSelectorMode SelectorMode
		{
			get { return _SelectorMode; }
			set { _SelectorMode = value; }
		}

		/// <summary>Returns the string representation of the data value held by the state object.</summary>
		/// <returns>The string representation of the value.</returns>
		public override string ToString()
		{
			if (Value == null)
			{
				return String.Empty;
			}
			return _Value.ToString();
		}

		/// <summary>Compares the provided object with the data value.</summary>
		/// <param name="obj">The object to compare with the data value.</param>
		/// <returns>
		///   <tt>true</tt> if the provided object is equal to the data value and <tt>false</tt> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (_Value == null)
			{
				return false;
			}
			return _Value.Equals(obj);
		}

		public override int GetHashCode()
		{
			if (_Value == null)
			{
				return 0;
			}
			return _Value.GetHashCode();
		}

		public void SetFieldName(string name)
		{
			_FieldName = name;
		}

		/// <summary>Configures a field state with the specified value and type of the field from the provided parameters.</summary>
		/// <param name="precision">The maximum number of digits used to represent a numeric value stored in the field.</param>
		/// <param name="isKey">The value that indicates (if set to true) that the field is marked as a key field.</param>
		/// <param name="value">The value that is stored in the field.</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="dataType">The type of data stored in the field.</param>
		/// <param name="defaultValue">The default value that is displayed in the field's cell for a new record that is not yet committed to the <tt>PXGraph</tt> instance.</param>
		/// <param name="nullable">The value that indicates (if set to true) that the field can store the <tt>null</tt> value.</param>
		/// <param name="required">The value that indicates (if set to true) that the value of the field is required.</param>
		/// <param name="length">The storage size of the field.</param>
		/// <param name="displayName">The display name for the field.</param>
		/// <param name="error">The error text assigned to the field.</param>
		/// <param name="errorLevel">The error level assigned to the field.</param>
		/// <param name="enabled">The value that indicates (if set to true) that the current field input control will respond to a user's interaction.</param>
		/// <param name="visible">The value that indicates (if set to true) that the current field input control is displayed.</param>
		/// <param name="readOnly">The value that indicates (if set to true) that the field is read-only.</param>
		/// <param name="visibility">The <tt>PXUIVisibility</tt> object for the field.</param>
		/// <param name="viewName">The name for the <tt>PXView</tt> object bound to the <tt>PXSelector</tt> field control.</param>
		/// <param name="fieldList">The array of DAC fields for the <tt>PXSelector</tt> field control.</param>
		/// <param name="headerList">The array of field display names for the <tt>PXSelector</tt> field control.</param>
		/// <param name="descriptionName">The name of a DAC field displayed in the <tt>PXSelector</tt> control of the field if the <tt>DisplayMode</tt> property is set to <tt>Text</tt>. If the
		/// <tt>DisplayMode</tt> property is set to <tt>Hint</tt>, the name is displayed in the <tt>ValueField</tt> - <tt>DescriptionName</tt> format.</param>
		public static PXFieldState CreateInstance(
					object value,
					Type dataType,
					bool? isKey = null,
					bool? nullable = null,
					int? required = null,
					int? precision = null,
					int? length = null,
					object defaultValue = null,
					string fieldName = null,
					string descriptionName = null,
					//string description = null, 
					string displayName = null,
					string error = null,
					PXErrorLevel errorLevel = PXErrorLevel.Undefined,
					bool? enabled = null,
					bool? visible = null,
					bool? readOnly = null,
					PXUIVisibility visibility = PXUIVisibility.Undefined,
					string viewName = null,
					string[] fieldList = null,
					string[] headerList = null
					)
		{
			var state = value as PXFieldState ?? new PXFieldState(value);

			FillValues(state, false, dataType, isKey, nullable, required, precision, length, defaultValue, fieldName,
				descriptionName, displayName, errorLevel, error, enabled, visible, readOnly, visibility,
				viewName, fieldList, headerList);

			return state;
		}

		/// <summary>Configures a field state with the specified type of the field from the provided parameters.</summary>
		/// <param name="precision">The maximum number of digits used to represent a numeric value stored in the field.</param>
		/// <param name="isKey">The value that indicates (if set to true) that the field is marked as a key field.</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="dataType">The type of data stored in the field.</param>
		/// <param name="defaultValue">The default value that is displayed in the field's cell for a new record that is not yet committed to the <tt>PXGraph</tt> instance.</param>
		/// <param name="nullable">The value that indicates (if set to true) that the field can store the <tt>null</tt> value.</param>
		/// <param name="required">The value that indicates (if set to true) that the value of the field is required.</param>
		/// <param name="length">The storage size of the field.</param>
		/// <param name="displayName">The display name for the field.</param>
		/// <param name="error">The error text assigned to the field.</param>
		/// <param name="errorLevel">The error level assigned to the field.</param>
		/// <param name="enabled">The value that indicates (if set to true) that the current field input control will respond to a user's interaction.</param>
		/// <param name="visible">The value that indicates (if set to true) that the current field input control is displayed.</param>
		/// <param name="readOnly">The value that indicates (if set to true) that the field is read-only.</param>
		/// <param name="visibility">The <tt>PXUIVisibility</tt> object for the field.</param>
		/// <param name="viewName">The name for the <tt>PXView</tt> object bound to the <tt>PXSelector</tt> field control.</param>
		/// <param name="fieldList">The array of DAC fields for the <tt>PXSelector</tt> field control.</param>
		/// <param name="headerList">The array of field display names for the <tt>PXSelector</tt> field control.</param>
		/// <param name="descriptionName">The name of a DAC field displayed in the <tt>PXSelector</tt> control of the field if the <tt>DisplayMode</tt> property is set to <tt>Text</tt>. If the
		/// <tt>DisplayMode</tt> property is set to <tt>Hint</tt>, the name is displayed in the <tt>ValueField</tt> - <tt>DescriptionName</tt> format.</param>
		public PXFieldState CreateInstance(
					Type dataType,
					bool? isKey,
					bool? nullable,
					int? required,
					int? precision,
					int? length,
					object defaultValue,
					string fieldName,
					string descriptionName,
					//string description, 
					string displayName,
					string error,
					PXErrorLevel errorLevel,
					bool? enabled,
					bool? visible,
					bool? readOnly,
					PXUIVisibility visibility,
					string viewName,
					string[] fieldList,
					string[] headerList
					)
		{
			var state = ((ICloneable)this).Clone() as PXFieldState;

			FillValues(state, true, dataType, isKey, nullable, required, precision, length, defaultValue, fieldName,
				descriptionName, displayName, errorLevel, error, enabled, visible, readOnly, visibility,
				viewName, fieldList, headerList);

			return state;
		}

		private static void FillValues(PXFieldState state, bool @override, Type dataType, bool? isKey, bool? nullable, int? required,
			int? precision, int? length, object defaultValue, string fieldName, string descriptionName, string displayName,
			PXErrorLevel errorLevel, string error, bool? enabled, bool? visible, bool? readOnly, PXUIVisibility visibility,
			string viewName, string[] fieldList, string[] headerList)
		{
			if (dataType != null) state._DataType = dataType;
			if (isKey != null) state._IsKey = (bool)isKey;
			if (nullable != null) state._Nullable = (bool)nullable;
			if (required != null) state._Required += (int)required;
			if (precision != null) state._Precision = (int)precision;
			if (length != null) state._Length = (int)length;
			if (defaultValue != null) state._DefaultValue = defaultValue;
			if (fieldName != null) state._FieldName = fieldName;
			if (descriptionName != null) state._DescriptionName = descriptionName;
			if (displayName != null) state.DisplayName = displayName;
			if (errorLevel >= state._ErrorLevel)
			{
				state._ErrorLevel = errorLevel;
				if (error != null) state._Error = error;
			}
			else if (state._Error == null && error != null)
			{
				state._Error = error;
				if (state._ErrorLevel == PXErrorLevel.Undefined) state._ErrorLevel = PXErrorLevel.Error;
			}
			if (enabled != null && (@override || state._Enabled)) state._Enabled = (bool)enabled;
			if (visible != null && (@override || state._Visible)) state._Visible = (bool)visible;
			if (readOnly != null && (@override || !state._IsReadOnly)) state._IsReadOnly = (bool)readOnly;
			if (visibility != PXUIVisibility.Undefined) state._Visibility = visibility;
			if (viewName != null) state._ViewName = viewName;
			if (fieldList != null) state._FieldList = fieldList;
			if (headerList != null) state._HeaderList = headerList;
		}

		/// <summary>Initializes a field state from another field state.</summary>
		/// <param name="state">A field state to initialize the field state from.</param>
		public static PXFieldState CreateInstance(IDataSourceFieldSchema state)
		{

			PXFieldState result = new PXFieldState(state);

			result._DataType = state.DataType;

			result._Nullable = state.Nullable;
			result._Precision = state.Precision;
			result._Length = state.Length;
			result._FieldName = state.Name;


			return result;
		}

		public static string GetStringValue(PXFieldState state, string fFormat, string dFormat)
		{
			if (state == null || state.Value == null)
			{
				return string.Empty;
			}
			else
			{
				if (state.DataType == typeof(string))
				{
					PXStringState st = state as PXStringState;
					string v = (string)state.Value;
					if (st != null)
					{
						if (st.AllowedValues != null && st.AllowedLabels != null)
						{
							for (int j = 0; j < st.AllowedValues.Length; j++)
							{
								if (j >= st.AllowedLabels.Length)
								{
									break;
								}
								if (String.Compare(v, st.AllowedValues[j], StringComparison.OrdinalIgnoreCase) == 0)
								{
									v = st.AllowedLabels[j];
									break;
								}
							}
						}
						else if (!String.IsNullOrEmpty(st.InputMask))
						{
							v = PX.Common.Mask.Format(st.InputMask, v);
						}
					}
					return v;
				}
				else if (state.DataType == typeof(byte)
					|| state.DataType == typeof(short)
					|| state.DataType == typeof(int))
				{
					PXIntState st = state as PXIntState;
					string v = state.Value.ToString();
					if (st != null && st.AllowedValues != null && st.AllowedLabels != null)
					{
						for (int j = 0; j < st.AllowedValues.Length; j++)
						{
							if (j >= st.AllowedLabels.Length)
							{
								break;
							}
							if ((int)state.Value == st.AllowedValues[j])
							{
								v = st.AllowedLabels[j];
								break;
							}
						}
					}
					return v;
				}
				else if (state.DataType == typeof(Guid)
					|| state.DataType == typeof(bool)
					|| state.DataType == typeof(long))
				{
					return state.Value.ToString();
				}
				else if (state.DataType == typeof(float))
				{
					return ((float)state.Value).ToString(fFormat + state.Precision.ToString());
				}
				else if (state.DataType == typeof(double))
				{
					return ((double)state.Value).ToString(fFormat + state.Precision.ToString());
				}
				else if (state.DataType == typeof(decimal))
				{
					return ((decimal)state.Value).ToString(fFormat + state.Precision.ToString());
				}
				else if (state.DataType == typeof(DateTime))
				{
					return ((DateTime)state.Value).ToString(dFormat);
				}
				else
				{
					return state.Value.ToString();
				}
			}
		}

		/// <exclude/>
		private class FieldInfo
		{
			public readonly PXFieldState State;
			public readonly string ForeignTableName;
			public FieldInfo(PXFieldState state, string foreignTableName)
			{
				State = state;
				ForeignTableName = foreignTableName;
			}
		}

		public static PXFieldState[] GetFields(PXGraph graph, Type[] tables, bool designMode)
		{
			List<PXFieldState> ret = new List<PXFieldState>();
			Dictionary<string, FieldInfo> displayNames = new Dictionary<string, FieldInfo>();
			for (int i = 0; i < tables.Length; i++)
			{
				string friendly = tables[i].Name;
				if (Attribute.IsDefined(tables[i], typeof(PXCacheNameAttribute)))
				{
					friendly = graph.Caches[tables[i]].GetName();
				}
				PXCache cache = graph.Caches[tables[i]];
				for (int j = 0; j < cache.Fields.Count; j++)
				{
					string name = cache.Fields[j];
					PXFieldState value = cache.GetStateExt(null, name) as PXFieldState;
					if (i > 1)
					{
						name = tables[i].Name + "__" + name;
					}
					string foreignTableName = null;
					if (value == null)
					{
						PropertyInfo descr = tables[i].GetProperty(name);
						value = PXFieldState.CreateInstance(null, descr != null ? descr.PropertyType : typeof(object),
							null, null, null, null, null, null, name, null, name, null, PXErrorLevel.Undefined,
							false, false, null, PXUIVisibility.Invisible, null, null, null);
					}
					else
					{
						if (designMode)
						{
							CorrectViewName(graph, value);
						}
						if (i > 0)
						{
							value.SetAliased(tables[i].Name);
							foreignTableName = friendly;
							FieldInfo fieldInfo;
							if (!string.IsNullOrEmpty(value.DisplayName) && displayNames.TryGetValue(value.DisplayName, out fieldInfo))
							{
								value.DisplayName = friendly + '-' + value.DisplayName;
								if (!string.IsNullOrEmpty(fieldInfo.ForeignTableName))
								{
									displayNames.Remove(fieldInfo.State.DisplayName);
									fieldInfo.State.DisplayName = fieldInfo.ForeignTableName + '-' + fieldInfo.State.DisplayName;
									if (!displayNames.ContainsKey(fieldInfo.State.DisplayName))
										displayNames.Add(fieldInfo.State.DisplayName, fieldInfo);
								}
							}
						}
					}
					ret.Add(value);
					if (!string.IsNullOrEmpty(value.DisplayName) && !displayNames.ContainsKey(value.DisplayName))
						displayNames.Add(value.DisplayName, new FieldInfo(value, foreignTableName));
				}
			}
			List<string> newDisplayNames = new List<string>();
			foreach (KeyValuePair<string, FieldInfo> info in displayNames)
			{
				string fieldName = info.Key;
				string leftSeparator = "-";
				int startBracketIndex = fieldName.IndexOf("-$");
				if (startBracketIndex < 0)
				{
					startBracketIndex = fieldName.IndexOf('$');
					leftSeparator = string.Empty;
				}
				if (startBracketIndex > -1 && startBracketIndex < (fieldName.Length - 1))
				{
					int endBracketIndex = fieldName.IndexOf("$-", startBracketIndex + 1);
					if (endBracketIndex > -1)
					{
						string leftStr = fieldName.Substring(0, startBracketIndex);
						string contStr = fieldName.Substring(startBracketIndex + leftSeparator.Length + 1,
							endBracketIndex - startBracketIndex - leftSeparator.Length - 1);
						string rightStr = fieldName.Substring(endBracketIndex + 2);

						string newDisplayName = string.Concat(leftStr, leftSeparator, rightStr);
						if (displayNames.ContainsKey(newDisplayName) || newDisplayNames.Contains(newDisplayName))
							newDisplayName = string.Concat(leftStr, leftSeparator, contStr, "-", rightStr);
						info.Value.State.DisplayName = newDisplayName;
						newDisplayNames.Add(newDisplayName);
					}
				}
			}
			return ret.ToArray();
		}

		public static void CorrectViewName(PXGraph graph, object val)
		{
			PXFieldState state = val as PXFieldState;
			if (state != null && !String.IsNullOrEmpty(state.ViewName) && state.ViewName[0] != '_')
			{
				PXView view;
				if (graph.Views.TryGetValue(state.ViewName, out view))
				{
					IBqlParameter[] parameters = view.BqlSelect.GetParameters();
					System.Text.StringBuilder bld = new System.Text.StringBuilder();
					foreach (Type type in view.BqlSelect.GetTables())
					{
						bld.Append('_');
						bld.Append(type.Name);
					}
					if (parameters != null)
					{
						foreach (IBqlParameter par in parameters)
						{
							if (!par.HasDefault)
							{
								continue;
							}
							Type t = par.GetReferencedType();
							bld.Append('_');
							bld.Append(BqlCommand.GetItemType(t).Name);
							bld.Append('.');
							bld.Append(t.Name);
						}
					}
					bld.Append('_');
					if (!graph.Views.ContainsKey(bld.ToString()))
					{
						graph.Views[bld.ToString()] = graph.Views[state.ViewName];
						graph._viewNames = null;
					}
					state.ViewName = bld.ToString();
				}
			}
		}
		public static object UnwrapValue(object stateOrValue)
		{
			PXFieldState state = stateOrValue as PXFieldState;
			return state != null ? state.Value : stateOrValue;
		}

		public static implicit operator string(PXFieldState val)
		{
			return val.Value as string;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>string</tt> DAC field.</summary>
	public class PXStringState : PXFieldState
	{
		#region Constructor
		protected PXStringState(object value)
			: base(value)
		{
			PXStringState state = value as PXStringState;
			if (state != null)
			{
				_Length = state._Length;
			}
		}
		#endregion

		#region Variables
		protected string _InputMask;
		protected string[] _AllowedValues;
		protected string[] _AllowedLabels;
		protected string[] _AllowedImages;
		internal string[] _NeutralLabels;
		protected bool _ExclusiveValues = false;
		protected bool _EmptyPossible = false;
		protected bool _IsUnicode = true;
		protected bool _MultiSelect = false;
		protected string _Language;
		#endregion

		#region Properties
		/// <summary>Gets or sets the value specifying how users enter data and how data is displayed.</summary>
		public virtual string InputMask
		{
			get { return _InputMask; }
			set { _InputMask = value; }
		}

		/// <summary>Gets or sets the list of values for the <tt>PXDropDown</tt> field input control.</summary>
		public virtual string[] AllowedValues
		{
			get { return _AllowedValues; }
			set { _AllowedValues = value; }
		}
		/// <summary>Gets or sets the list of labels for the <tt>PXDropDown</tt> field input control.</summary>
		public virtual string[] AllowedLabels
		{
			get
			{
				if (_AllowedLabels != null && _AllowedValues != null && _AllowedLabels.Length >= _AllowedValues.Length)
				{
					return _AllowedLabels;
				}
				return _AllowedValues;
			}
			set { _AllowedLabels = value; }
		}
		/// <summary>Gets or sets the list of images for the <tt>PXDropDown</tt> field input control.</summary>
		public virtual string[] AllowedImages
		{
			get
			{
				if (_AllowedImages != null && _AllowedValues != null && _AllowedImages.Length >= _AllowedValues.Length)
				{
					return _AllowedImages;
				}
				if (_AllowedValues == null)
				{
					return null;
				}
				return _AllowedImages = new string[_AllowedValues.Length];
			}
			set { _AllowedImages = value; }
		}


		/// <summary>Gets a value that enables (if set to true) editing of the value in the <tt>PXDropDown</tt> field input control.</summary>
		public virtual bool ExclusiveValues
		{
			get { return _ExclusiveValues; }
		}

		/// <summary>Gets or sets a value that enables (if set to true) filtering by the empty value in the <tt>PXDropDown</tt> field input control.</summary>
		public virtual bool EmptyPossible
		{
			get { return _EmptyPossible; }
			set { _EmptyPossible = value; }
		}

		/// <summary>Gets or sets a value indicating whether Unicode string content is supported.</summary>
		public virtual bool IsUnicode
		{
			get { return _IsUnicode; }
			set { _IsUnicode = value; }
		}

		public virtual bool MultiSelect
		{
			get { return _MultiSelect; }
			set { _MultiSelect = value; }
		}

		public virtual string Language
		{
			get { return _Language; }
			set { _Language = value; }
		}

		/// <summary>Gets the collection of values and labels for the field PXDropDown input control.</summary>
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
		#endregion

		#region Methods
		/// <exclude />
		public override string ToString()
		{
			if (_Value == null)
			{
				return String.Empty;
			}
			else if (!string.IsNullOrEmpty(_InputMask) && _Value is string)
			{
				return PX.Common.Mask.Format(_InputMask, (string)_Value);
			}
			else if (!(_AllowedValues == null || _AllowedLabels == null) && _Value is string)
			{
				string descr;
				if (ValueLabelDic.TryGetValue((string)_Value, out descr))
				{
					return descr;
				}
			}
			return _Value.ToString();
		}

		/// <exclude />
		public static PXFieldState CreateInstance(object value, int? length, bool? isUnicode, string fieldName, bool? isKey, int? required, string inputMask, string[] allowedValues, string[] allowedLabels, bool? exclusiveValues, string defaultValue, string[] neutralLabels = null)
		{
			PXStringState state = value as PXStringState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(string))
				{
					return field;
				}
				state = new PXStringState(value);
			}
			state._DataType = typeof(string);
			if (length != null)
			{
				state._Length = (int)length;
			}
			if (isUnicode != null)
			{
				state._IsUnicode = (bool)isUnicode;
			}
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (required != null)
			{
				state._Required += (int)required;
			}
			if (inputMask != null)
			{
				state._InputMask = inputMask;
			}
			if (allowedValues != null)
			{
				state._AllowedValues = allowedValues;
			}
			if (allowedLabels != null)
			{
				state._AllowedLabels = allowedLabels;
			}
			if (neutralLabels != null)
			{
				state._NeutralLabels = neutralLabels;
			}
			if (exclusiveValues != null)
			{
				state._ExclusiveValues = (bool)exclusiveValues;
			}
			if (defaultValue != null)
			{
				state._DefaultValue = defaultValue;
			}
			return state;
		}
		#endregion
	}

	/// <exclude />
	public class PXBranchSelectorState : PXFieldState
	{
		public string DACName { get; set; }
		public PXBranchSelectorState(object value) : base(value) { }
		public string SelectionMode { get; set; }
	}

	/// <exclude/>
	public class PXImageState : PXStringState
	{
		private string _headerImage;

		#region Constructor
		protected PXImageState(object value)
			: base(value)
		{
		}

		#endregion

		#region Methods

		public string HeaderImage
		{
			get { return _headerImage; }
		}

		public static PXFieldState CreateInstance(object value, int? length, bool? isUnicode, string fieldName, bool? isKey, int? required, string inputMask, string[] allowedValues, string[] allowedLabels, bool? exclusiveValues, string defaultValue, string headerImage)
		{
			PXImageState state = value as PXImageState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(string))
				{
					return field;
				}
				state = new PXImageState(value);
			}
			state._DataType = typeof(string);
			if (length != null)
				state._Length = (int)length;

			if (isUnicode != null)
				state._IsUnicode = (bool)isUnicode;

			if (fieldName != null)
				state._FieldName = fieldName;

			if (isKey != null)
				state._IsKey = (bool)isKey;

			if (required != null)
				state._Required += (int)required;

			if (defaultValue != null)
				state._DefaultValue = defaultValue;

			if (headerImage != null)
				state._headerImage = headerImage;
			return state;
		}
		#endregion
	}
	/// <exclude/>
	public class PXHeaderImageState : PXFieldState
	{
		private string _headerImage;

		#region Constructor
		protected PXHeaderImageState(object value): base(value)
		{}
		#endregion

		#region Methods

		public string HeaderImage
		{
			get { return _headerImage; }
		}

		public static PXFieldState CreateInstance(object value, string fieldName, string headerImage)
		{
			PXHeaderImageState state = value as PXHeaderImageState;
			if (state == null)
			{
				state = new PXHeaderImageState(value);
			}
			if (headerImage != null)
				state._headerImage = headerImage;
			if (fieldName != null)
				state._FieldName = fieldName;
			return state;
		}
		#endregion

	}

	/// <summary>Provides data to set up the presentation of a single segment of a segmented field input control.</summary>
	public class PXSegment
	{
		/// <summary>The input mask for the segment:
		/// <list type="bullet"><item>C: <tt>MaskType.Ascii</tt></item><item>a: <tt>MaskType.AlphaNumeric</tt></item><item>9: <tt>MaskType.Numeric</tt></item><item>?: <tt>MaskType.Alpha</tt></item></list></summary>
		public readonly char EditMask;
		/// <summary>The character to fill the value.</summary>
		public readonly char FillCharacter;
		/// <summary>The character to prompt for a value.</summary>
		public readonly char PromptCharacter;
		/// <summary>The number of characters in the segment.</summary>
		public readonly short Length;
		/// <summary>The value that indicates (if set to true) that the new specified segment value should be validated.</summary>
		public readonly bool Validate;
		/// <summary>The value that specifies whether the letters in the segment are converted to uppercase or lowercase:
		/// <list type="bullet"><item>0: <tt>NotSet</tt></item><item>1: <tt>Upper</tt></item><item>2: <tt>Lower</tt></item></list></summary>
		public readonly short CaseConvert;
		/// <summary>The text alignment type in the segment:
		/// <list type="bullet"><item>1: <tt>Left</tt></item><item>2: <tt>Right</tt></item></list></summary>
		public readonly short Align;
		/// <summary>The character that is used to separate the segment from the previous one.</summary>
		public readonly char Separator;
		/// <summary>The value that indicates (if set to true) that the contents of the segment cannot be changed.</summary>
		public readonly bool ReadOnly;
		/// <summary>The description of the segment.</summary>
		public readonly string Descr;
		/// <summary>Creates an instance of the <tt>PXSegment</tt> class using the provided values.</summary>
		/// <param name="editMask">The input mask for the segment:
		/// <list type="bullet"><item>C: <tt>MaskType.Ascii</tt></item><item>a: <tt>MaskType.AlphaNumeric</tt></item><item>9: <tt>MaskType.Numeric</tt></item><item>?: <tt>MaskType.Alpha</tt></item></list></param>
		/// <param name="fillCharacter">The character to fill the value.</param>
		/// <param name="length">The number of characters in the segment.</param>
		/// <param name="validate">The value that indicates (if set to true) that the new specified segment value should be validated.</param>
		/// <param name="caseConverter">The value that specifies whether the letters in the segment are converted to uppercase or lowercase:
		/// <list type="bullet"><item>0: <tt>NotSet</tt></item><item>1: <tt>Upper</tt></item><item>2: <tt>Lower</tt></item></list></param>
		/// <param name="align">The text alignment type in the segment:
		/// <list type="bullet"><item>1: <tt>Left</tt></item><item>2: <tt>Right</tt></item></list></param>
		/// <param name="separator">The character that is used to separate the segment from the previous one.</param>
		/// <param name="readOnly">The value that indicates (if set to true) that the contents of the segment cannot be changed.</param>
		/// <param name="descr">The description of the segment.</param>
		/// <param name="promptCharacter">The character to prompt for a value.</param>
		public PXSegment(char editMask, char fillCharacter, short length, bool validate, short caseConverter, short align, char separator, bool readOnly, string descr, char promptCharacter = '_')
		{
			EditMask = editMask;
			FillCharacter = fillCharacter;
			PromptCharacter = promptCharacter;
			Length = length;
			Validate = validate;
			CaseConvert = caseConverter;
			Align = align;
			Separator = separator;
			ReadOnly = readOnly;
			Descr = descr;
		}
	}

	/// <summary>Provides data to set up the presentation of the segmented DAC field input control.</summary>
	/// <example>
	///   <code title=" " description="The code below gets the field state object of the SubItemID." lang="CS">
	/// PXSegmentedState subItem =
	///     this.ResultRecords.Cache.GetValueExt&lt;InventoryTranDetEnqResult.subItemID&gt;
	///     (this.ResultRecords.Current) as PXSegmentedState;</code>
	/// </example>
	public class PXSegmentedState : PXStringState
	{
		protected PXSegmentedState(object value)
			: base(value)
		{
		}
		protected PXSegment[] _Segments;
		/// <summary>Gets or sets the list of segments for the segmented field input control.</summary>
		public PXSegment[] Segments
		{
			get
			{
				return _Segments;
			}
			set
			{
				_Segments = value;
			}
		}
		protected bool _ValidCombos;
		/// <summary>Gets or sets the value indicating whether the segmented field input control displays a single lookup or a separate lookup for each segment</summary>
		public bool ValidCombos
		{
			get
			{
				return _ValidCombos;
			}
			set
			{
				_ValidCombos = value;
			}
		}

		protected DimensionLookupMode _LookupMode;

		public DimensionLookupMode LookupMode
		{
			get { return _LookupMode; }
			set { _LookupMode = value; }
		}

		protected string _SegmentValueField;

		public string SegmentValueField
		{
			get { return _SegmentValueField; }
			set { _SegmentValueField = value; }
		}

		protected string _SegmentDescriptionName;

		public string SegmentDescriptionName
		{
			get { return _SegmentDescriptionName; }
			set { _SegmentDescriptionName = value; }
		}

		protected string _SegmentViewName;

		public string SegmentViewName
		{
			get { return _SegmentViewName; }
			set { _SegmentViewName = value; }
		}

		protected string[] _SegmentFieldList;

		public string[] SegmentFieldList
		{
			get { return _SegmentFieldList; }
			set { _SegmentFieldList = value; }
		}

		private static ConcurrentDictionary<string, string[]> _segmentHeaderListCached = new ConcurrentDictionary<string, string[]>();
		protected string[] _SegmentHeaderList;

		public string[] SegmentHeaderList
		{
			get { return _SegmentHeaderList; }
			set { _SegmentHeaderList = value; }
		}

		protected string _Wildcard;

		/// <summary>Gets or sets the collection of characters allowed to be specified within each segment in addition to the <tt>Mask</tt> property of <tt>PXSegment.</tt></summary>
		public string Wildcard
		{
			get
			{
				return _Wildcard;
			}
			set
			{
				_Wildcard = value;
			}
		}

		internal static string GetEditMaskForSegment(PXSegment seg)
		{
			switch (seg.EditMask)
			{
				case 'C':
				case 'c':
					return new string('C', seg.Length);
				case 'A':
				case 'a':
					return new string('A', seg.Length);
				case '9':
					return new string('#', seg.Length);
				case '?':
					return new string('L', seg.Length);
				default:
					return String.Empty;
			}
		}

		/// <exclude />
		public static PXFieldState CreateInstance(object value, string fieldName, PXSegment[] segments, string segmentViewName, DimensionLookupMode? lookupMode, bool? validCombos, string wildcard)
		{
			PXSegmentedState state = value as PXSegmentedState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(string))
				{
					return field;
				}
				state = new PXSegmentedState(value);
			}
			state._DataType = typeof(string);
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (segments != null)
			{
				state._Segments = segments;
			}
			if (lookupMode != null)
			{
				state._LookupMode = lookupMode.Value;
			}
			if (validCombos != null)
			{
				state._ValidCombos = (bool)validCombos;
			}
			if (wildcard != null)
			{
				state._Wildcard = wildcard;
			}
			if (String.IsNullOrEmpty(state._SegmentViewName))
			{
				state._SegmentFieldList = new string[] { "Value", "Descr" };
				string culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
				string[] _headers = _segmentHeaderListCached.GetOrAdd(culture, (key) => new[] { PXMessages.LocalizeNoPrefix("Value"), PXMessages.LocalizeNoPrefix("Description") });
				state._SegmentHeaderList = _headers;
				state._SegmentValueField = "Value";
				state._SegmentDescriptionName = "Descr";
				state._SegmentViewName = segmentViewName;
			}
			if (segments != null && segments.Length > 0)
			{
				System.Text.StringBuilder bld = new System.Text.StringBuilder();
				short prevCase = 0;
				for (int i = 0; i < segments.Length; i++)
				{
					PXSegment s = segments[i];
					if (s.Length > 0)
					{
						if (s.CaseConvert != prevCase)
						{
							switch (s.CaseConvert)
							{
								case 0:
									bld.Append(prevCase == 1 ? '>' : '<');
									break;
								case 1:
									bld.Append('>');
									break;
								case 2:
									bld.Append('<');
									break;
							}
							prevCase = s.CaseConvert;
						}
						bld.Append(GetEditMaskForSegment(s));
					}
					if (i < segments.Length - 1)
					{
						bld.Append(s.Separator);
					}
				}
				state._InputMask = bld.ToString();
			}
			return state;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>double</tt> DAC field.</summary>
	public class PXDoubleState : PXFieldState
	{
		protected PXDoubleState(object value)
			: base(value)
		{
			PXDoubleState state = value as PXDoubleState;
			if (state != null)
			{
				_Precision = state._Precision;
			}
		}
		protected double _MinValue = Double.MinValue;
		protected double _MaxValue = Double.MaxValue;
		/// <summary>Gets or sets the minimum value that can be set in the field input control.</summary>
		public virtual double MinValue
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
		/// <summary>Gets or sets the maximum value that can be set in the field input control.</summary>
		public virtual double MaxValue
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
		/// <summary>Configures a double field state from the provided parameters.</summary>
		/// <param name="maxValue">The maximum value that can be set in the field input control.</param>
		/// <param name="minValue">The minimum value that can be set in the field input control.</param>
		/// <param name="required">The value that indicates (if set to true) that the value of the field is required.</param>
		/// <param name="precision">The maximum number of digits used to represent a numeric value stored in the field.</param>
		/// <param name="isKey">The value that indicates (if set to true) that the field is marked as a key field.</param>
		/// <param name="value">The value that is stored in the field.</param>
		/// <param name="fieldName">The name of the field.</param>
		public static PXFieldState CreateInstance(object value, int? precision, string fieldName, bool? isKey, int? required, double? minValue, double? maxValue)
		{
			PXDoubleState state = value as PXDoubleState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(double))
				{
					return field;
				}
				state = new PXDoubleState(value);
			}
			state._DataType = typeof(double);
			if (precision != null)
			{
				state._Precision = (int)precision;
			}
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (required != null)
			{
				state._Required += (int)required;
			}
			if (minValue != null)
			{
				state._MinValue = (double)minValue;
			}
			if (maxValue != null)
			{
				state._MaxValue = (double)maxValue;
			}
			return state;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>float</tt> DAC field.</summary>
	public class PXFloatState : PXFieldState
	{
		protected PXFloatState(object value)
			: base(value)
		{
			PXFloatState state = value as PXFloatState;
			if (state != null)
			{
				_Precision = state._Precision;
			}
		}
		protected float _MinValue = float.MinValue;
		protected float _MaxValue = float.MaxValue;
		/// <summary>Gets or sets the minimum value that could be set in the field input control.</summary>
		public virtual float MinValue
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
		/// <summary>Gets or sets the maximum value that could be set in the field input control.</summary>
		public virtual float MaxValue
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
		/// <summary>Configures a float field state from the provided parameters.</summary>
		/// <param name="value">The value that is stored in the field.</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="precision">The maximum number of digits used to represent a numeric value stored in the field.</param>
		/// <param name="isKey">The value that indicates (if set to true) that the field is marked as a key field.</param>
		/// <param name="required">The value that indicates (if set to true) that the value of the field is required.</param>
		/// <param name="minValue">The minimum value that can be set in the field input control.</param>
		/// <param name="maxValue">The maximum value that can be set in the field input control.</param>
		public static PXFieldState CreateInstance(object value, int? precision, string fieldName, bool? isKey, int? required, float? minValue, float? maxValue)
		{
			PXFloatState state = value as PXFloatState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(float))
				{
					return field;
				}
				state = new PXFloatState(value);
			}
			state._DataType = typeof(float);
			if (required != null)
			{
				state._Required += (int)required;
			}
			if (precision != null)
			{
				state._Precision = (int)precision;
			}
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (minValue != null)
			{
				state._MinValue = (float)minValue;
			}
			if (maxValue != null)
			{
				state._MaxValue = (float)maxValue;
			}
			return state;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>decimal</tt> DAC field.</summary>
	public class PXDecimalState : PXFieldState
	{
		protected PXDecimalState(object value)
			: base(value)
		{
			PXDecimalState state = value as PXDecimalState;
			if (state != null)
			{
				_Precision = state._Precision;
			}
		}
		protected decimal _MinValue = decimal.MinValue;
		protected decimal _MaxValue = decimal.MaxValue;
		/// <summary>Gets or sets the minimum value that can be set in the field input control.</summary>
		public virtual decimal MinValue
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
		/// <summary>Gets or sets the maximum value that can be set in the field input control.</summary>
		public virtual decimal MaxValue
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
		/// <summary>Configures a decimal field state from the provided parameters.</summary>
		/// <param name="value">The value that is stored in the field.</param>
		/// <param name="precision">The maximum number of digits used to represent a numeric value stored in the field.</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="isKey">The value that indicates (if set to true) that the field is marked as a key field.</param>
		/// <param name="required">The value that indicates (if set to true) that the value of the field is required.</param>
		/// <param name="minValue">The minimum value that can be set in the field input control.</param>
		/// <param name="maxValue">The maximum value that can be set in the field input control.</param>
		/// <returns></returns>
		public static PXFieldState CreateInstance(object value, int? precision, string fieldName, bool? isKey, int? required, decimal? minValue, decimal? maxValue)
		{
			PXDecimalState state = value as PXDecimalState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(decimal))
				{
					return field;
				}
				state = new PXDecimalState(value);
			}
			state._DataType = typeof(decimal);
			if (required != null)
			{
				state._Required += (int)required;
			}
			if (precision != null)
			{
				state._Precision = (int)precision;
			}
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (minValue != null)
			{
				state._MinValue = (decimal)minValue;
			}
			if (maxValue != null)
			{
				state._MaxValue = (decimal)maxValue;
			}
			return state;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>DateTime</tt> DAC field.</summary>
	public class PXDateState : PXFieldState
	{
		protected PXDateState(object value)
			: base(value)
		{
			PXDateState state = value as PXDateState;
			if (state != null)
			{
			}
		}
		protected string _InputMask = null;
		protected string _DisplayMask = null;
		protected DateTime _MinValue = new DateTime(1900, 1, 1);
		protected DateTime _MaxValue = new DateTime(9999, 12, 31);
		/// <summary>Gets or sets the value that specifies how users enter the date.</summary>
		public virtual string InputMask
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
		/// <summary>Gets or sets the value that specifies how the date is displayed.</summary>
		public virtual string DisplayMask
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
		/// <summary>Gets or sets the minimum value that can be set in the field input control.</summary>
		public virtual DateTime MinValue
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
		/// <summary>Gets or sets the maximum value that can be set in the field input control.</summary>
		public virtual DateTime MaxValue
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
		/// <summary>Configures a date field state from the provided parameters.</summary>
		/// <param name="minValue">The minimum value that can be set in the field input control.</param>
		/// <param name="maxValue">The maximum value that can be set in the field input control.</param>
		/// <param name="inputMask">The value that specifies how users enter the date.</param>
		/// <param name="displayMask">The value that specifies how the date is displayed.</param>
		/// <param name="required">The value that indicates (if set to true) that the value of the field is required.</param>
		/// <param name="value">The value that is stored in the field.</param>
		/// <param name="isKey">The value that indicates (if set to true) that the field is marked as a key field.</param>
		/// <param name="fieldName">The name of the field.</param>
		public static PXFieldState CreateInstance(object value, string fieldName, bool? isKey, int? required, string inputMask, string displayMask, DateTime? minValue, DateTime? maxValue)
		{
			PXDateState state = value as PXDateState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(DateTime))
				{
					return field;
				}
				state = new PXDateState(value);
			}
			state._DataType = typeof(DateTime);
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (required != null)
			{
				state._Required += (int)required;
			}
			if (inputMask != null)
			{
				state._InputMask = inputMask;
			}
			if (displayMask != null)
			{
				state._DisplayMask = displayMask;
			}
			if (minValue != null)
			{
				state._MinValue = (DateTime)minValue;
			}
			if (maxValue != null)
			{
				state._MaxValue = (DateTime)maxValue;
			}
			return state;
		}
	}

	public class PXTimeState : PXIntState
	{
		protected PXTimeState(object value)
			: base(value)
		{
		}

		public static PXFieldState CreateInstance(PXIntState value, int[] allowedValues, string[] allowedLabels)
		{
			if (value == null) return null;
			PXTimeState timeState = new PXTimeState(value);

			timeState._AllowedImages = value.AllowedImages;
			timeState._AllowedLabels = allowedLabels;
			timeState._AllowedValues = allowedValues;
			timeState._ExclusiveValues = value.ExclusiveValues;
			timeState._EmptyPossible = value.EmptyPossible;
			timeState._MaxValue = value.MaxValue;
			timeState._MinValue = value.MinValue;
			timeState._NeutralLabels = value._NeutralLabels;

			return timeState;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>integer</tt> DAC field.</summary>
	public class PXIntState : PXFieldState
	{
		protected PXIntState(object value)
			: base(value)
		{
			_DataType = typeof(int);
			PXIntState state = value as PXIntState;
			if (state != null)
			{
			}
		}
		protected int _MinValue = int.MinValue;
		protected int _MaxValue = int.MaxValue;
		protected int[] _AllowedValues;
		internal string[] _NeutralLabels;
		protected string[] _AllowedLabels;
		protected string[] _AllowedImages;
		protected bool _ExclusiveValues = false;
		protected bool _EmptyPossible = false;

		/// <summary>Gets or sets the minimum value that could be set in the field input control</summary>
		public virtual int MinValue
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
		/// <summary>Gets or sets the maximum value that could be set in the field input control.</summary>
		public virtual int MaxValue
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
		/// <summary>Gets or sets the list of values for the field input control of the <tt>PXDropDown</tt> type.</summary>
		public virtual int[] AllowedValues
		{
			get
			{
				return _AllowedValues;
			}
			set { _AllowedValues = value; }
		}
		/// <summary>Gets or sets the list of labels for the field input control of the <tt>PXDropDown</tt> type.</summary>
		public virtual string[] AllowedLabels
		{
			get
			{
				if (_AllowedLabels != null && _AllowedValues != null && _AllowedLabels.Length >= _AllowedValues.Length)
				{
					return _AllowedLabels;
				}
				if (_AllowedValues == null)
				{
					return null;
				}
				string[] allowedLabels = new string[_AllowedValues.Length];
				for (int i = 0; i < _AllowedValues.Length; i++)
				{
					allowedLabels[i] = _AllowedValues[i].ToString();
				}
				return allowedLabels;
			}
			set { _AllowedLabels = value; }
		}
		/// <summary>Gets or sets the list of images for the field input control of the <tt>PXDropDown</tt> type.</summary>
		public virtual string[] AllowedImages
		{
			get
			{
				if (_AllowedImages != null && _AllowedValues != null && _AllowedImages.Length >= _AllowedValues.Length)
				{
					return _AllowedImages;
				}
				if (_AllowedValues == null)
				{
					return null;
				}
				return _AllowedImages = new string[_AllowedValues.Length];
			}
		}

		/// <summary>Gets a value that enables (if set to true) editing of the value in the <tt>PXDropDown</tt> field input control.</summary>
		public virtual bool ExclusiveValues
		{
			get { return _ExclusiveValues; }
		}

		/// <summary>Gets or sets a value that enables (if set to true) filtering by the empty value in the <tt>PXDropDown</tt> field input control.</summary>
		public virtual bool EmptyPossible
		{
			get { return _EmptyPossible; }
			set { _EmptyPossible = value; }
		}

		/// <summary>Gets the collection of values and labels for the field PXDropDown input control.</summary>
		public Dictionary<int, string> ValueLabelDic
		{
			get
			{
				if (_AllowedValues == null || _AllowedLabels == null) return null;
				Dictionary<int, string> result = new Dictionary<int, string>(_AllowedValues.Length);
				for (int index = 0; index < _AllowedValues.Length; index++)
					result.Add(_AllowedValues[index], _AllowedLabels[index]);
				return result;
			}
		}

		/// <exclude />
		public static PXFieldState CreateInstance(object value, string fieldName, bool? isKey, int? required, int? minValue, int? maxValue, int[] allowedValues, string[] allowedLabels, Type dataType, int? defaultValue, string[] neutralLabels)
		{
			return CreateInstance(value, fieldName, isKey, required, minValue, maxValue, allowedValues, allowedLabels, dataType, defaultValue, neutralLabels, null);
		}

		/// <exclude />
		public static PXFieldState CreateInstance(object value, string fieldName, bool? isKey, int? required, int? minValue, int? maxValue, int[] allowedValues, string[] allowedLabels, Type dataType, int? defaultValue, string[] neutralLabels = null, bool? nullable = null)
		{
			return CreateInstance(value, fieldName, isKey, required, minValue, maxValue, allowedValues, allowedLabels, false, dataType, defaultValue, neutralLabels, nullable);
		}

		/// <exclude />
		public static PXFieldState CreateInstance(object value, string fieldName, bool? isKey, int? required, int? minValue, int? maxValue, int[] allowedValues, string[] allowedLabels, bool? exclusiveValues, Type dataType, int? defaultValue, string[] neutralLabels = null, bool? nullable = null)
		{
			PXIntState state = value as PXIntState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (dataType == null && field != null && field.DataType != typeof(object))
					dataType = field.DataType;
				if (field != null && field.DataType != typeof(object) && field.DataType != dataType)
				{
					return field;
				}
				state = new PXIntState(value);
			}
			if (dataType != null)
			{
				state._DataType = dataType;
			}
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (required != null)
			{
				state._Required += (int)required;
			}
			if (minValue != null)
			{
				state._MinValue = (int)minValue;
			}
			if (maxValue != null)
			{
				state._MaxValue = (int)maxValue;
			}
			if (allowedValues != null)
			{
				state._AllowedValues = allowedValues;
			}
			if (allowedLabels != null)
			{
				state._AllowedLabels = allowedLabels;
			}
			if (neutralLabels != null)
			{
				state._NeutralLabels = neutralLabels;
			}
			if (exclusiveValues != null)
			{
				state._ExclusiveValues = (bool)exclusiveValues;
			}
			if (defaultValue != null)
			{
				state._DefaultValue = Convert.ToInt32(defaultValue);
			}
			if (nullable != null)
			{
				state._Nullable = (bool)nullable;
			}
			return state;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>Guid</tt> DAC field.</summary>
	public class PXGuidState : PXFieldState
	{
		protected PXGuidState(object value)
			: base(value)
		{
			_DataType = typeof(Guid);
			PXGuidState state = value as PXGuidState;
			if (state != null)
			{
			}
		}
		/// <summary>Configures a guid field state from the provided parameters.</summary>
		/// <param name="value">The value that is stored in the field.</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="isKey">The value that indicates (if set to true) that the field is marked as a key field.</param>
		/// <param name="required">The value that indicates (if set to true) that the value of the field is required.</param>
		public static PXFieldState CreateInstance(object value, string fieldName, bool? isKey, int? required)
		{
			PXGuidState state = value as PXGuidState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(Guid))
				{
					return field;
				}
				state = new PXGuidState(value);
			}
			state._DataType = typeof(Guid);
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (required != null)
			{
				state._Required += (int)required;
			}
			return state;
		}
	}

	/// <summary>Provides data to set up the presentation of the input control for the <tt>long</tt> DAC field.</summary>
	public class PXLongState : PXFieldState
	{
		protected PXLongState(object value)
			: base(value)
		{
			_DataType = typeof(long);
			PXLongState state = value as PXLongState;
			if (state != null)
			{
			}
		}
		protected long _MinValue = long.MinValue;
		protected long _MaxValue = long.MaxValue;
		/// <summary>Gets or sets the minimum value that could be set in the field input control.</summary>
		public virtual long MinValue
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
		/// <summary>Gets or sets the maximum value that could be set in the field input control.</summary>
		public virtual long MaxValue
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
		/// <exclude />
		public static PXFieldState CreateInstance(object value, string fieldName, bool? isKey, int? required, long? minValue, long? maxValue, Type dataType)
		{
			PXLongState state = value as PXLongState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != dataType)
				{
					return field;
				}
				state = new PXLongState(value);
			}
			if (dataType != null)
			{
				state._DataType = dataType;
			}
			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (isKey != null)
			{
				state._IsKey = (bool)isKey;
			}
			if (required != null)
			{
				state._Required += (int)required;
			}
			if (minValue != null)
			{
				state._MinValue = (long)minValue;
			}
			if (maxValue != null)
			{
				state._MaxValue = (long)maxValue;
			}
			return state;
		}
	}

	/// <exclude/>
	public class PXNoteState : PXFieldState
	{
		protected string _NoteIDField;
		protected string _NoteDocsField;
		protected string _NoteTextField;
		protected string _NoteActivityField;
		protected bool _AddFileEnabled;

		protected string _NoteTextExistsField;
		protected string _NoteFilesExistsField;

		protected PXNoteState(object value)
			: base(value)
		{
			_DataType = typeof(string);
			_Nullable = false;
			PXNoteState state = value as PXNoteState;
			if (state != null)
			{
			}
		}

		public virtual string NoteIDField
		{
			get { return _NoteIDField; }
		}

		public virtual string NoteDocsField
		{
			get { return _NoteDocsField; }
		}

		public virtual string NoteTextField
		{
			get { return _NoteTextField; }
		}

		public virtual string NoteActivityField
		{
			get { return _NoteActivityField; }
		}

		public virtual string NoteTextExistsField
		{
			get { return _NoteTextExistsField; }
		}

		public virtual string NoteFilesExistsField
		{
			get { return _NoteFilesExistsField; }
		}

		public static PXFieldState CreateInstance(object value, string fieldName, string noteIDField, string noteTextField, string noteDocsField, string noteActivityField, string noteTextExistsField = null, string noteFilesExistsField = null, string noteTextDisplayName = null)
		{
			PXNoteState state = value as PXNoteState;
			if (state == null)
			{
				PXFieldState field = value as PXFieldState;
				if (field != null && field.DataType != typeof(object) && field.DataType != typeof(string))
				{
					return field;
				}
				state = new PXNoteState(value);
			}

			if (noteTextDisplayName != null)
			{
				state._DisplayName = noteTextDisplayName;
			}

			if (fieldName != null)
			{
				state._FieldName = fieldName;
			}
			if (noteIDField != null)
			{
				state._NoteIDField = noteIDField;
			}
			if (noteTextField != null)
			{
				state._NoteTextField = noteTextField;
			}
			if (noteDocsField != null)
			{
				state._NoteDocsField = noteDocsField;
			}
			if (noteActivityField != null)
			{
				state._NoteActivityField = noteActivityField;
			}
			if (noteTextExistsField != null)
			{
				state._NoteTextExistsField = noteTextExistsField;
			}
			if (noteFilesExistsField != null)
			{
				state._NoteFilesExistsField = noteFilesExistsField;
			}
			return state;
		}
	}

	/// <exclude/>
	public class PXColorState : PXStringState
	{
		protected PXColorState(object value)
			: base(value)
		{
		}

		public static PXFieldState CreateInstance(PXFieldState value)
		{
			PXStringState state = value as PXStringState;
			if (state == null) return value;
			PXColorState c = new PXColorState(state)
			{
				_AllowedLabels = state.AllowedLabels,
				_AllowedValues = state.AllowedValues,
				_InputMask = state.InputMask,
				_NeutralLabels = state._NeutralLabels,
				_ExclusiveValues = state.ExclusiveValues,
				_EmptyPossible = state.EmptyPossible,
				_MultiSelect = state.MultiSelect
			};
			return c;
		}
	}

	/// <exclude/>
	public class PXInactiveFieldState : PXFieldState
	{
		protected PXInactiveFieldState(object value)
			: base(value)
		{
		}

		public static PXFieldState CreateInstance(string fieldName, string displayName)
		{
			return new PXInactiveFieldState(PXFieldState.CreateInstance(null, typeof(object), false, true, null, null, null, null, fieldName, null, displayName, null, PXErrorLevel.Undefined, false, false, true, PXUIVisibility.Invisible, null, null, null));
		}
	}

	/// <summary>Provides data to set up the presentation of a <tt>PXSelector</tt> input control for a DAC field.</summary>
	public class PXSelectorState : PXStringState
	{
		/// <summary>The DAC that contains the field whose values the system uses as the selector values.</summary>
		/// <value>The full name of the DAC, such as <tt>PX.Objects.SO.SOOrder</tt>.</value>
		public string SchemaObject { get; protected set; }
		/// <summary>The field whose values the system uses as the selector values.</summary>
		/// <value>The name of the field, such as <tt>OrderType</tt>.</value>
		public string SchemaField { get; protected set; }
		protected PXSelectorState(object value)
			: base(value)
		{
		}

		/// <summary>Configures a PXSelector field state from the provided parameters.</summary>
		/// <param name="schemeField">The field whose values the system uses as the selector values.</param>
		/// <param name="schemeObj">The DAC that contains the field whose values the system uses as the selector values.</param>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="value">A field state.</param>
		public static PXFieldState CreateInstance(PXFieldState value, string schemeObj, string schemeField, string fieldName)
		{
			PXStringState state = value as PXStringState;
			if (state == null) return new PXSelectorState(value) { SchemaObject = schemeObj, SchemaField = schemeField, _FieldName = fieldName };
			PXSelectorState s = new PXSelectorState(state)
			{
				_AllowedLabels = state.AllowedLabels,
				_AllowedValues = state.AllowedValues,
				_InputMask = state.InputMask,
				_NeutralLabels = state._NeutralLabels,
				_ExclusiveValues = state.ExclusiveValues,
				_EmptyPossible = state.EmptyPossible,
				_MultiSelect = state.MultiSelect,
				SchemaObject = schemeObj,
				SchemaField = schemeField,
				_ValueField = schemeField,
				_FieldName = fieldName
			};
			return s;
		}
	}

}
