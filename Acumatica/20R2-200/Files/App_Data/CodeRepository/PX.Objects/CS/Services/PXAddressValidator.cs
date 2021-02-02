using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.AddressValidator;
using PX.Data;
using IAddressBase = PX.CS.Contracts.Interfaces.IAddressBase;

namespace PX.Objects.CS
{
	public static class PXAddressValidator
	{
		public static bool Validate<T>(PXGraph aGraph, T aAddress, bool aSynchronous)
			where T : IAddressBase, IValidatedAddress
		{
			return Validate(aGraph, aAddress, aSynchronous, false);
		}

		public static void Validate<T>(PXGraph aGraph, List<T> aAddresses, bool aSynchronous, bool updateToValidAddress)
			where T : IAddressBase, IValidatedAddress
		{
			foreach (T it in aAddresses)
			{
				Validate<T>(aGraph, it, aSynchronous, updateToValidAddress);
			}
		}

		public static bool Validate<T>(PXGraph aGraph, T aAddress, bool aSynchronous, bool updateToValidAddress)
			where T : IAddressBase, IValidatedAddress
		{
			if (!AddressValidatorPluginMaint.IsActive(aGraph, aAddress.CountryID))
			{
				if (aSynchronous)
				{
					PXCache cache = aGraph.Caches[typeof(T)];
					string countryFieldName = nameof(IAddressBase.CountryID);
					cache.RaiseExceptionHandling(countryFieldName, aAddress, aAddress.CountryID,
						new PXSetPropertyException(Messages.AddressVerificationServiceIsNotSetup,
							PXErrorLevel.Warning, aAddress.CountryID));
					return false;
				}
				else
				{
					throw new PXException(Messages.AddressVerificationServiceIsNotSetup, aAddress.CountryID);
				}
			}

			bool isValid = true;

			Country country = Country.PK.Find(aGraph, aAddress.CountryID);
			updateToValidAddress = updateToValidAddress && country.AutoOverrideAddress == true;

			IAddressValidator service = AddressValidatorPluginMaint.CreateAddressValidator(aGraph, country.AddressValidatorPluginID);
			if (service != null)
			{
				PXCache cache = aGraph.Caches[typeof(T)];
				
				Dictionary<string, string> messages = new Dictionary<string, string>();

				T validAddress = (T)cache.CreateCopy(aAddress);

				try
				{
					isValid = service.ValidateAddress(validAddress, messages);
				}
				catch
				{
					throw new PXException(Messages.UnknownErrorOnAddressValidation);
				}

				if (isValid)
				{
					string[] fields =
					{
						nameof(IAddressBase.AddressLine1),
						nameof(IAddressBase.AddressLine2),
						//nameof(IAddressBase.AddressLine3),
						nameof(IAddressBase.City),
						nameof(IAddressBase.State),
						nameof(IAddressBase.PostalCode)
					};

					var modifiedFields = fields.Select(field => new
					{
						Field = field,
						OriginalValue = ((string)cache.GetValue(aAddress, field)) ?? string.Empty,
						ValidValue = ((string)cache.GetValue(validAddress, field)) ?? string.Empty
					}).Where(x => string.Compare(x.OriginalValue.Trim(), x.ValidValue.Trim(), StringComparison.OrdinalIgnoreCase) != 0).ToArray();

					if (aSynchronous && !updateToValidAddress && modifiedFields.Any())
					{
						var fieldsShouldBeEqual = new HashSet<string>()
						{
							nameof(IAddressBase.PostalCode)
						};

						foreach(var m in modifiedFields)
						{
							cache.RaiseExceptionHandling(m.Field, aAddress, m.OriginalValue,
									new PXSetPropertyException(Messages.AddressVerificationServiceReturnsField, PXErrorLevel.Warning, m.ValidValue));

							if (fieldsShouldBeEqual.Contains(m.Field))
								isValid = false;
						}
					}

					if (isValid)
					{
						T copyToUpdate = (T)cache.CreateCopy(aAddress);
						Action<T> raiseWarnings = null;
						if (aSynchronous && updateToValidAddress)
						{
							foreach (var m in modifiedFields)
							{
								var validValue = m.ValidValue == string.Empty ? null : m.ValidValue;
								cache.SetValue(copyToUpdate, m.Field, validValue);
								raiseWarnings += (a) => cache.RaiseExceptionHandling(m.Field, a, validValue,
									new PXSetPropertyException(Messages.AddressVerificationServiceReplaceValue, PXErrorLevel.Warning, m.OriginalValue));
							}
						}
						copyToUpdate.IsValidated = true;
						aAddress = (T)cache.Update(copyToUpdate);
						raiseWarnings?.Invoke(aAddress);
					}
				}
				else
				{
					string message = string.Empty;
					StringBuilder messageBuilder = new StringBuilder();
					int count = 0;
					foreach (var iMsg in messages)
					{
						if (!aSynchronous)
						{
							if (count > 0) messageBuilder.Append(",");
							messageBuilder.AppendFormat("{0}:{1}", iMsg.Key, iMsg.Value);
							count++;
						}
						else
						{
							object value = cache.GetValue(aAddress, iMsg.Key);
							cache.RaiseExceptionHandling(iMsg.Key, aAddress, value, new PXSetPropertyException(iMsg.Value));
						}
					}
					if (!aSynchronous)
					{
						throw new PXException(messageBuilder.ToString());
					}
				}
			}
			return isValid;
		}

		public static bool IsValidateRequired<T>(PXGraph aGraph, T aAddress)
			where T : IAddressBase
		{
			if (aAddress?.CountryID != null)
			{
				CS.Country country = CS.Country.PK.Find(aGraph, aAddress.CountryID);
				return country.AddressValidatorPluginID != null;
			}

			return false;
		}
	}
}
