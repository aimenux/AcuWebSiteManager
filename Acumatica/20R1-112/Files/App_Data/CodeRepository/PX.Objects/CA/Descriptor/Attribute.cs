using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Data;
using PX.CCProcessingBase.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	#region DynamicValueValidationAttribute

	/// <summary>
	/// This attribute allows to provide a dynamic validation rules for the field.<br/>
	/// The rule is defined as regexp and may be stored in some external field.<br/>
	/// In the costructor, one should provide a search method for this rule. <br/>
	/// </summary>
	/// <example>
	/// <code>
	/// [DynamicValueValidation(typeof(Search&lt;PaymentMethodDetail.validRegexp, 
	///		Where&lt;PaymentMethodDetail.paymentMethodID, Equal&lt;Current&lt;VendorPaymentMethodDetail.paymentMethodID&gt;&gt;,
	///		And&lt;PaymentMethodDetail.detailID, Equal&lt;Current&lt;VendorPaymentMethodDetail.detailID&gt;&gt;&gt;&gt;&gt;))]
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class DynamicValueValidationAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
	{
		#region State
		protected Type _RegexpSearch;
		protected Type _SourceType;
		protected string _SourceField;
		protected BqlCommand _Select;
		protected int _Length;
		#endregion

		#region Ctor
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="aRegexpSearch">Must be IBqlSearch returning a validation regexp.</param>
		public DynamicValueValidationAttribute(Type aRegexpSearch)
		{
			if (aRegexpSearch == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (!typeof(IBqlSearch).IsAssignableFrom(aRegexpSearch))
			{
				throw new PXArgumentException("aSearchRegexp", Messages.ValueValidationNotValid);
			}
			_RegexpSearch = aRegexpSearch;
			_Select = BqlCommand.CreateInstance(aRegexpSearch);
			_SourceType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			_SourceField = ((IBqlSearch)_Select).GetField().Name;

		}

		#endregion

		#region Implementation

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string value = (string)e.NewValue;
			if (!string.IsNullOrEmpty(value))
			{
				int? controlType;
				string regexp = this.FindValidationRegexp(sender, e.Row, out controlType);
				if (!this.ValidateValue(value, regexp))
				{
					throw new PXSetPropertyException(Messages.ValueIsNotValid);

				}
			}
		}

		protected virtual void MultiSelectFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			int? controlType;
			if (e.NewValue is string)
			{
				if (this.FindValidationRegexp(sender, e.Row, out controlType) == null)
				{
					if (controlType == 6 && ((string)e.NewValue).TrimEnd().Length > _Length)
					{
						throw new PXSetPropertyException(ErrorMessages.StringLengthExceeded, _FieldName);
					}
					else if (controlType == 5 && sender.Graph.IsImport && !sender.Graph.IsCopyPasteContext)
					{
						DateTime dt;
						if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out dt))
						{
							e.NewValue = dt.ToString("M/d/yyyy");
						}
					}
				}
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			foreach (PXDBStringAttribute attr in sender.GetAttributesReadonly(_FieldName).Where(attr => attr is PXDBStringAttribute).Cast<PXDBStringAttribute>())
			{
				_Length = attr.Length;
			}
			if (_Length > 0)
			{
				sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName.ToLower(), MultiSelectFieldUpdating);
			}
		}

		protected virtual string FindValidationRegexp(PXCache sender, object row, out int? controlType)
		{
			controlType = null;
			if (_Select != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, false);
				object item = view.SelectSingleBound(new object[] { row });
				if (item != null && item is PXResult)
				{
					item = ((PXResult)item)[_SourceType];
				}
				string result = (string)sender.Graph.Caches[_SourceType].GetValue(item, _SourceField == null ? _FieldName : _SourceField);
				if (typeof(CSAttribute).IsAssignableFrom(_SourceType) && item != null)
				{
					controlType = ((int?)sender.Graph.Caches[_SourceType].GetValue<CSAttribute.controlType>(item));
				}
				return result;
			}
			return null;
		}

		protected virtual bool ValidateValue(string val, string regex)
		{
			if (val == null || regex == null)
			{
				return true;
			}
			System.Text.RegularExpressions.Regex regexobject = new System.Text.RegularExpressions.Regex(regex);
			return regexobject.IsMatch(val);
		}
		#endregion

	}
	#endregion

	#region PXDBStringWithMaskAttribute
	/// <summary>
	/// This attribute defines a PXDBStringAttribute with a dynamically created entry mask <br/>
	/// Should be used, when the entry mask must be set during a runtime, rather then at compile-time <br/>
	/// May be combined with the DynamicValueValidationAttribute <br/>
	/// </summary>
	/// <example>
	/// <code>
	/// [PXDBStringWithMask(255, typeof(Search&lt;PaymentMethodDetail.entryMask, 
	///		Where&lt;PaymentMethodDetail.paymentMethodID, Equal&lt;Current&lt;VendorPaymentMethodDetail.paymentMethodID&gt;&gt;,
	///		And&lt;PaymentMethodDetail.detailID, Equal&lt;Current&lt;VendorPaymentMethodDetail.detailID&gt;&gt;&gt;&gt;&gt;),IsUnicode = true)]
	///	</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBStringWithMaskAttribute : PXDBStringAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _MaskSearch;
		protected Type _SourceType;
		protected string _SourceField;
		protected BqlCommand _Select;
		#endregion

		#region Ctor

		/// <summary>        
		/// Calls the default constructor of the PXDBString
		/// </summary>
		/// <param name="length">Length of the string in the database. Passed to PXDBString.</param>
		/// <param name="aMaskSearch">Must be a IBqlSearch type returning a valid mask expression</param>
		public PXDBStringWithMaskAttribute(int length, Type aMaskSearch) : base(length)
		{
			AssignMaskSearch(aMaskSearch);
		}

		/// <summary>Calls the default constructor of the PXDBString</summary>
		/// <param name="aMaskSearch">Must be a IBqlSearch type returning a valid mask expression</param>
		public PXDBStringWithMaskAttribute(Type aMaskSearch)
			: base()
		{
			AssignMaskSearch(aMaskSearch);
		}
		#endregion
		protected virtual void AssignMaskSearch(Type aMaskSearch)
		{
			if (aMaskSearch == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (!typeof(IBqlSearch).IsAssignableFrom(aMaskSearch))
			{
				throw new PXArgumentException("aMaskSearch", Messages.ValueMaskNotValid);
			}
			this._MaskSearch = aMaskSearch;
			this._Select = BqlCommand.CreateInstance(aMaskSearch);
			this._SourceType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			this._SourceField = ((IBqlSearch)_Select).GetField().Name;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				string mask = this.FindMask(sender, e.Row);
				if (!string.IsNullOrEmpty(mask))
				{
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, _Length, null, _FieldName, _IsKey, null, mask, null, null, null, null);
				}
				else
				{
					base.FieldSelecting(sender, e);
				}
			}
			else
			{
				base.FieldSelecting(sender, e);
			}
		}

		protected virtual string FindMask(PXCache sender, object row)
		{
			if (_Select != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, false);
				object item = view.SelectSingleBound(new object[] { row });
				if (item != null && item is PXResult)
				{
					item = ((PXResult)item)[_SourceType];
				}
				string result = (string)sender.Graph.Caches[_SourceType].GetValue(item, _SourceField == null ? _FieldName : _SourceField);
				return result;
			}
			return null;
		}

	}
	#endregion

	#region PXRSACryptStringWithMaskAttribute
	/// <summary>
	/// This as a specialized version of PXRSACryptStringAttribute<br/>
	/// which allows to define entry mask dynamically. Works identically to the PXDBStringWithMask <br/>
	/// and PXRSACryptString attributes - namely providing run-time entry mask definition for the crypted strings.<br/> 
	/// </summary>
	/// <example>
	/// <code>
	/// [PXRSACryptStringWithMaskAttribute(1028, 
	///     typeof(Search&lt;PaymentMethodDetail.entryMask, 
	///         Where&lt;PaymentMethodDetail.paymentMethodID, Equal&lt;Current&lt;CustomerPaymentMethodDetail.paymentMethodID&gt;&gt;,
	///			And&lt;PaymentMethodDetail.detailID, Equal&lt;Current&lt;CustomerPaymentMethodDetail.detailID&gt;&gt;&gt;&gt;&gt;), 
	///		IsUnicode = true)]
	/// </code>
	/// </example>	
	public class PXRSACryptStringWithMaskAttribute : PXRSACryptStringAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _MaskSearch;
		protected Type _SourceType;
		protected string _SourceField;
		protected BqlCommand _Select;
		#endregion
		#region Ctor

		/// <summary>        
		/// Calls the default constructor of the PXDBString
		/// </summary>
		/// <param name="length">Length of the string in the database. Passed to PXDBString.</param>
		/// <param name="aMaskSearch">Must be a IBqlSearch type returning a valid mask expression</param>		
		public PXRSACryptStringWithMaskAttribute(int length, Type aMaskSearch)
			: base(length)
		{
			AssignMaskSearch(aMaskSearch);
		}

		/// <summary>Calls the default constructor of the PXDBString</summary>
		/// <param name="aMaskSearch">Must be a IBqlSearch type returning a valid mask expression</param>
		public PXRSACryptStringWithMaskAttribute(Type aMaskSearch)
			: base()
		{
			AssignMaskSearch(aMaskSearch);
		}
		#endregion
		protected virtual void AssignMaskSearch(Type aMaskSearch)
		{
			if (aMaskSearch == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (!typeof(IBqlSearch).IsAssignableFrom(aMaskSearch))
			{
				throw new PXArgumentException("aMaskSearch", Messages.ValueMaskNotValid);
			}
			this._MaskSearch = aMaskSearch;
			this._Select = BqlCommand.CreateInstance(aMaskSearch);
			this._SourceType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			this._SourceField = ((IBqlSearch)_Select).GetField().Name;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				string mask = this.FindMask(sender, e.Row);
				if (!string.IsNullOrEmpty(mask))
				{
					if (!string.IsNullOrEmpty((string)e.ReturnValue))
					{
						string viewAs = mask.Replace("#", "0").Replace("-", "").Replace("/", "");
						this.ViewAsString = viewAs;
						base.FieldSelecting(sender, e);

						PXStringState state = e.ReturnState as PXStringState;
						if (state != null)
						{
							e.ReturnState = PXStringState.CreateInstance(e.ReturnState, _Length, null, _FieldName, _IsKey, null, mask, null, null, null, null);
						}
					}
					else
					{
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, _Length, null, _FieldName, _IsKey, null, mask, null, null, null, null);
					}
				}
				else
				{
					base.FieldSelecting(sender, e);
				}
			}
			else
			{
				base.FieldSelecting(sender, e);
			}
		}

		protected virtual string FindMask(PXCache sender, object row)
		{
			if (_Select != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, false);
				object item = view.SelectSingleBound(new object[] { row });
				if (item != null && item is PXResult)
				{
					item = ((PXResult)item)[_SourceType];
				}
				string result = (string)sender.Graph.Caches[_SourceType].GetValue(item, _SourceField == null ? _FieldName : _SourceField);
				return result;
			}
			return null;
		}
	}
	#endregion
	#region PXRSACryptStringWithConditionalAttribute
	/// <summary>
	/// Works very much like PXRSACryptStringAttribute. Encryption is used conditionally, depending on value of EncryptionRequiredField.
	/// isEncryptedField is used to store current state of field - encrypted or not.
	/// </summary>
	/// <param name="EncryptionRequiredField">BQL field</param>
	/// <param name="isEncryptedField">BQL field</param>
	public class PXRSACryptStringWithConditionalAttribute : PXRSACryptStringAttribute, IPXRowPersistingSubscriber
	{
		protected Type _EncryptionRequiredField;
		protected Type _isEncryptedField;

		public PXRSACryptStringWithConditionalAttribute(Type encryptionRequiredField, Type isEncryptedField)
			: base()
		{
			checkParams(encryptionRequiredField, isEncryptedField);
			_EncryptionRequiredField = encryptionRequiredField;
			_isEncryptedField = isEncryptedField;
		}

		public PXRSACryptStringWithConditionalAttribute(int length, Type encryptionRequiredField, Type isEncryptedField)
			: base(length)
		{
			checkParams(encryptionRequiredField, isEncryptedField);
			_EncryptionRequiredField = encryptionRequiredField;
			_isEncryptedField = isEncryptedField;
		}

		private void checkParams(Type encryptionRequiredField, Type isEncryptedField)
		{
			if (encryptionRequiredField == null)
			{
				throw new PXArgumentException("EncryptionRequiredField", ErrorMessages.ArgumentNullException);
			}
			if (isEncryptedField == null)
			{
				throw new PXArgumentException("isEncryptedField", ErrorMessages.ArgumentNullException);
			}
			if (!typeof(IBqlField).IsAssignableFrom(encryptionRequiredField))
			{
				throw new PXArgumentException("EncryptionRequiredField", Messages.ShouldContainBQLField);
			}
			if (!typeof(IBqlField).IsAssignableFrom(isEncryptedField))
			{
				throw new PXArgumentException("isEncryptedField", Messages.ShouldContainBQLField);
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
				bool? isEncrypted = (sender.GetValue(e.Row, _isEncryptedField.Name) as bool?) ?? false;
				isEncryptionRequired = (isEncrypted == true);
			}
			base.RowSelecting(sender, e);
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			bool? encryptionRequired = sender.GetValue(e.Row, _EncryptionRequiredField.Name) as bool?;
			sender.SetValue(e.Row, _isEncryptedField.Name, encryptionRequired == true);
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				bool? encryptionRequired = sender.GetValue(e.Row, _EncryptionRequiredField.Name) as bool?;
				isEncryptionRequired = (encryptionRequired == true);
			}
			base.CommandPreparing(sender, e);
		}

		public override bool EncryptOnCertificateReplacement(PXCache cache, object row)
		{
			bool? isEncrypted = cache.GetValue(row, _isEncryptedField.Name) as bool?;
			return isEncrypted == true;
		}
	}
	#endregion

	#region CATranRefNbr
	[Serializable]
	public partial class CATranRefNbr : IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Ref. Number")]
		[PXSelector(typeof(Search<CATran.origRefNbr, Where<CATran.origModule, Equal<GL.BatchModule.moduleCA>,
			And<CATran.origTranType, Like<Optional<CATran.origTranType>>>>>),
			typeof(CATran.origRefNbr), typeof(CATran.origTranType), typeof(CATran.tranDate), typeof(CATran.finPeriodID))]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
	}
	#endregion

	#region PXProviderTypeSelectorAttribute

	public class PXProviderTypeSelectorAttribute : PXCustomSelectorAttribute
	{
		private Type[] _providerInterfaceType;

		[Serializable]
		[PXHidden]
		public partial class ProviderRec : IBqlTable
		{
			#region TypeName
			public abstract class typeName : PX.Data.BQL.BqlString.Field<typeName> { }
			[PXString(255, InputMask = "", IsKey = true)]
			[PXUIField(DisplayName = "Type Name", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string TypeName { get; set; }
			#endregion
			#region DisplayTypeName
			public abstract class displayTypeName : PX.Data.BQL.BqlString.Field<displayTypeName> { }
			[PXString(255, InputMask = "", IsKey = true)]
			[PXUIField(DisplayName = "Plug-in Name", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string DisplayTypeName { get; set; }
			#endregion

		}

		public PXProviderTypeSelectorAttribute(params Type[] providerType)
			: base(typeof(ProviderRec.typeName))
		{
			this._providerInterfaceType = providerType;
		}

		protected virtual IEnumerable GetRecords()
		{
			return GetProviderRecs(_providerInterfaceType);
		}

		public static IEnumerable<ProviderRec> GetProviderRecs(params Type[] providerInterfaceTypes)
		{
			if (providerInterfaceTypes == null)
			{
				yield break;
			}
			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (PXSubstManager.IsSuitableTypeExportAssembly(ass, false))
				{
					Type[] types = null;
					try
					{
						if (!ass.IsDynamic)
						{
							types = ass.GetExportedTypes();
						}
					}
					catch (ReflectionTypeLoadException te)
					{
						types = te.Types;
					}
					catch
					{
						continue;
					}
					if (types != null)
					{
						foreach (var type in types
							.Where(assemblyType => providerInterfaceTypes
								.Any(interfaceType => interfaceType.IsAssignableFrom(assemblyType) && assemblyType != interfaceType)))
						{
							Attribute attribute = type.GetCustomAttributes().FirstOrDefault(a => a is PXDisplayTypeNameAttribute);
							string displayName = (attribute as PXDisplayTypeNameAttribute)?.Name ?? type.FullName;
							yield return new ProviderRec { TypeName = type.FullName, DisplayTypeName = displayName };
						}
					}
				}
			}

		}
	}
	#endregion
}
