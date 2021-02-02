using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	public static class TaxLocationHelpers
	{
		public static void UpdateAddressLocationCodes(List<Address> addresses, PayrollTaxClient payrollService)
		{
			PXGraph.CreateInstance<UpdateAddressLocationCodesGraph>().UpdateAddressLocationCodes(addresses, payrollService);
		}

		public static void AddressPersisting(Events.RowPersisting<Address> e)
		{
			try
			{
				if (IsAddressedModified(e.Cache, e.Row))
				{
					var payrollService = new PayrollTaxClient();
					PRLocationCodeDescription locationCodes = payrollService.GetLocationCode(e.Row);
					if (locationCodes != null && locationCodes.TaxLocationCode != null)
					{
						e.Row.TaxLocationCode = locationCodes.TaxLocationCode;
						e.Row.TaxMunicipalCode = locationCodes.TaxMunicipalCode;
						e.Row.TaxSchoolCode = locationCodes.TaxSchoolCode;
					}
				}
			}
			catch
			{
				e.Row.TaxLocationCode = null;
				e.Row.TaxMunicipalCode = null;
				e.Row.TaxSchoolCode = null;
			}
		}

		public static bool IsAddressedModified(PXCache cache, Address row)
		{
			return !object.Equals(row.AddressLine1, cache.GetValueOriginal<Address.addressLine1>(row)) ||
				!object.Equals(row.AddressLine2, cache.GetValueOriginal<Address.addressLine2>(row)) ||
				!object.Equals(row.City, cache.GetValueOriginal<Address.city>(row)) ||
				!object.Equals(row.State, cache.GetValueOriginal<Address.state>(row)) ||
				!object.Equals(row.CountryID, cache.GetValueOriginal<Address.countryID>(row));
		}

		public class AddressEqualityComparer : IEqualityComparer<Address>
		{
			public bool Equals(Address x, Address y)
			{
				return x.AddressID == y.AddressID;
			}

			public int GetHashCode(Address obj)
			{
				return obj.AddressID.GetHashCode();
			}
		}

		public class UpdateAddressLocationCodesGraph : PXGraph<UpdateAddressLocationCodesGraph>
		{
			public SelectFrom<Address>.View Addresses;

			public void UpdateAddressLocationCodes(List<Address> addresses, PayrollTaxClient payrollService)
			{
				foreach (KeyValuePair<int?, PRLocationCodeDescription> kvp in payrollService.GetLocationCodes(addresses))
				{
					if (kvp.Value?.TaxLocationCode != null)
					{
						Address address = Addresses.Search<Address.addressID>(kvp.Key);
						if (address != null)
						{
							Addresses.Cache.RestoreCopy(address, addresses.First(x => x.AddressID == kvp.Key));
							address.TaxLocationCode = kvp.Value.TaxLocationCode;
							address.TaxMunicipalCode = kvp.Value.TaxMunicipalCode;
							address.TaxSchoolCode = kvp.Value.TaxSchoolCode;
							Addresses.Update(address);
						}
					}
				}

				Persist();
			}
		}
	}
}
