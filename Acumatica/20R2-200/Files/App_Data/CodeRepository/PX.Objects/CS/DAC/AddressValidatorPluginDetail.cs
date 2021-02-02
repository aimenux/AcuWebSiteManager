using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.AddressValidator;
using PX.Data;
using PX.TaxProvider;

namespace PX.Objects.CS
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.AddressValidatorPluginDetail)]
	public partial class AddressValidatorPluginDetail : PX.Data.IBqlTable, IAddressValidatorSetting
	{
		public const int Text = 1;
		public const int Combo = 2;
		public const int CheckBox = 3;
		public const int Password = 4;

		public const int ValueFieldLength = 1024;

		#region AddressValidatorPluginID
		public abstract class addressValidatorPluginID : PX.Data.BQL.BqlString.Field<addressValidatorPluginID> { }
		protected String _AddressValidatorPluginID;
		[PXParent(typeof(Select<AddressValidatorPlugin, Where<AddressValidatorPlugin.addressValidatorPluginID, Equal<Current<AddressValidatorPluginDetail.addressValidatorPluginID>>>>))]
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(AddressValidatorPlugin.addressValidatorPluginID))]
		public virtual String AddressValidatorPluginID
		{
			get
			{
				return this._AddressValidatorPluginID;
			}
			set
			{
				this._AddressValidatorPluginID = value;
			}
		}
		#endregion
		#region SettingID
		public abstract class settingID : PX.Data.BQL.BqlString.Field<settingID> { }
		protected String _SettingID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "ID", Enabled = false)]
		public virtual String SettingID
		{
			get
			{
				return this._SettingID;
			}
			set
			{
				this._SettingID = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected int? _SortOrder;
		[PXDBInt()]
		[PXDefault()]
		public virtual int? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		protected String _Value;
		[PXDBString(ValueFieldLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		public virtual String Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this._Value = value;
			}
		}
		#endregion
		#region ControlTypeValue
		public abstract class controlTypeValue : PX.Data.BQL.BqlInt.Field<controlTypeValue> { }
		protected Int32? _ControlTypeValue;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Control Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXIntList(new int[] { Text, Combo, CheckBox }, new string[] { "Text", "Combo", "Checkbox" })]
		public virtual Int32? ControlTypeValue
		{
			get
			{
				return this._ControlTypeValue;
			}
			set
			{
				this._ControlTypeValue = value;
			}
		}
		#endregion
		#region ComboValuesStr
		public abstract class comboValuesStr : PX.Data.BQL.BqlString.Field<comboValuesStr> { }
		protected String _ComboValuesStr;
		[PXDBString(4000, IsUnicode = true)]
		public virtual String ComboValuesStr
		{
			get
			{
				return this._ComboValuesStr;
			}
			set
			{
				this._ComboValuesStr = value;
			}
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion

		#region IAddressValidatorDetail Members
		public IDictionary<string, string> ComboValues
		{
			get
			{
				var list = new Dictionary<string, string>();

				string[] parts = _ComboValuesStr.Split(';');
				foreach (string part in parts)
				{
					if (!string.IsNullOrEmpty(part))
					{
						string[] keyval = part.Split('|');

						if (keyval.Length == 2)
						{
							list.Add(keyval[0], keyval[1]);
						}
					}
				}

				return list;
			}
			set
			{
				StringBuilder sb = new StringBuilder();

				foreach (var kv in value)
				{
					sb.AppendFormat("{0}|{1};", kv.Key, kv.Value);
				}

				_ComboValuesStr = sb.ToString();
			}
		}

		public AddressValidatorSettingControlType ControlType
		{
			get
			{
				try
				{
					if (_ControlTypeValue != null)
					{
						var controlType = (AddressValidatorSettingControlType) _ControlTypeValue;
						return controlType;
					}

					return AddressValidatorSettingControlType.Undefined;
				}
				catch
				{
					return AddressValidatorSettingControlType.Undefined;
				}
			}
			set { _ControlTypeValue = (int) value; }
		}
		#endregion
	}
}
