using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;

namespace PX.Objects.Common
{
	public abstract class OverrideAttributePropertyScope<TAttribute, TProperty> : IDisposable
		where TAttribute : PXEventSubscriberAttribute
	{
		/// <summary>
		/// The cache object used to obtain attributes of <typeparamref name="TAttribute"/> type.
		/// </summary>
		public PXCache Cache { get; }

		/// <summary>
		/// The record fields on which this scope acts.
		/// </summary>
		public IEnumerable<Type> Fields { get; }

		/// <summary>
		/// Delegate function used to assign the required property of the 
		/// specified attribute.
		/// </summary>
		protected Action<TAttribute, TProperty> _setAttributeProperty;

		/// <summary>
		/// Delegate function used to retrieve the required property value from 
		/// the specified attribute.
		/// </summary>
		protected Func<TAttribute, TProperty> _getAttributeProperty;

		/// <summary>
		/// Stores the old property values (to be restored upon scope disposal)
		/// for each attribute of type <typeparamref name="TAttribute"/> residing
		/// on a given field.
		/// </summary>
		protected Dictionary<string, TProperty> _oldAttributePropertyValuesByField;

		protected OverrideAttributePropertyScope(
			PXCache cache, 
			IEnumerable<Type> fields,
			Action<TAttribute, TProperty> setAttributeProperty,
			Func<TAttribute, TProperty> getAttributeProperty,
			TProperty overridePropertyValue)
		{
			if (cache == null) throw new ArgumentNullException(nameof(cache));
			if (setAttributeProperty == null) throw new ArgumentNullException(nameof(setAttributeProperty));
			if (getAttributeProperty == null) throw new ArgumentNullException(nameof(getAttributeProperty));

			bool anyFieldsSpecified = fields?.Any() == true;

			if (!anyFieldsSpecified)
			{
				fields = cache.BqlFields;
			}

			this.Cache = cache;
			this.Fields = fields;

			_setAttributeProperty = setAttributeProperty;
			_getAttributeProperty = getAttributeProperty;

			_oldAttributePropertyValuesByField = new Dictionary<string, TProperty>();

			foreach (Type field in fields)
			{
				if (!typeof(IBqlField).IsAssignableFrom(field))
				{
					throw new PXException($"The type {field.FullName} is not a BQL field.");
				}

				// The old value to restore upon scope disposal is 
				// always taken from the cache-level attribute.
				// -
				IEnumerable<TAttribute> attributesOfType = this.Cache
					.GetAttributesReadonly(field.Name)
					.OfType<TAttribute>();

				if (!attributesOfType.Any())
				{
					if (anyFieldsSpecified)
					{
						throw new PXException($"There are no attributes of type {nameof(TAttribute)} residing on {field.FullName}.");
					}
					else
					{
						continue;
					}
				}
				else if (!attributesOfType.IsSingleElement())
				{
					throw new PXException($"There is more than one attribute of type {nameof(TAttribute)} residing on {field.FullName}.");
				}
				
				attributesOfType.ForEach(attribute => 
				{
					TProperty oldPropertyValue = _getAttributeProperty(attribute);
					_oldAttributePropertyValuesByField[field.Name] = oldPropertyValue;

					_setAttributeProperty(attribute, overridePropertyValue);
				});
			}
		}

		void IDisposable.Dispose()
		{
			foreach (string fieldName in _oldAttributePropertyValuesByField.Keys)
			{
				TProperty oldPropertyValue = _oldAttributePropertyValuesByField[fieldName];

				// The attributes could have been cloned to item level since scope creation, so 
				// the old value should be restored to all attribute instances, hence the
				// GetAttributes (vs. GetAttributesReadonly).
				// -
				IEnumerable<TAttribute> attributesOfType = this.Cache
					.GetAttributes(fieldName)
					.OfType<TAttribute>();

				foreach (TAttribute attribute in attributesOfType)
				{
					_setAttributeProperty(attribute, oldPropertyValue);
				}
			}
		}
	}
}
