using System;
using System.Collections.Generic;
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

			CS.Country country = CS.Country.PK.Find(aGraph, aAddress.CountryID);

			IAddressValidator service = AddressValidatorPluginMaint.CreateAddressValidator(aGraph, country.AddressValidatorPluginID);
			if (service != null)
			{
				PXCache cache = aGraph.Caches[typeof(T)];
				
				Dictionary<string, string> messages = new Dictionary<string, string>();

				T copy = (T)cache.CreateCopy(aAddress);

				try
				{
					isValid = service.ValidateAddress(copy, messages);
				}
				catch
				{
					throw new PXException(Messages.UnknownErrorOnAddressValidation);
				}

				if (isValid)
				{
					if (!updateToValidAddress && aSynchronous)
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

						var fieldsShouldBeEqual = new HashSet<string>()
						{
							nameof(IAddressBase.PostalCode)
						};

						foreach (string iFld in fields)
						{
							string ourValue = ((string)cache.GetValue(aAddress, iFld)) ?? String.Empty;
							string extValue = ((string)cache.GetValue(copy, iFld)) ?? String.Empty;

							if (String.Compare(ourValue.Trim(), extValue.Trim(), StringComparison.OrdinalIgnoreCase) != 0)
							{
								cache.RaiseExceptionHandling(iFld, aAddress, ourValue,
									new PXSetPropertyException(Messages.AddressVerificationServiceReturnsField, PXErrorLevel.Warning, extValue));

								if (fieldsShouldBeEqual.Contains(iFld)) isValid = false;
							}
						}
					}

					if (isValid)
					{
						T copyToUpdate = copy;
						if (!updateToValidAddress)
						{
							copyToUpdate = (T)cache.CreateCopy(aAddress);//Clear changes made by ValidateAddress
						}
						copyToUpdate.IsValidated = true;
						aAddress = (T)cache.Update(copyToUpdate);
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
