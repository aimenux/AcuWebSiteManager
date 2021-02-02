using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class PayrollClient : ClientBase<IPayrollService>, IPayrollClientService
	{
		public IEnumerable<PRPayrollCalculation> Calculate(IEnumerable<PRPayroll> payrolls)
		{
			try
			{
				return base.Channel.Calculate(GetLicenseForPayrollSvc(), payrolls);
			}
			catch (FaultException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		public PRLocationCodeDescription GetLocationCode(PRAddress address)
		{
			try
			{
				return base.Channel.GetLocationCode(GetLicenseForPayrollSvc(), address);
			}
			catch (FaultException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		public PRLocationCodeDescription GetLocationCode(IAddressBase address)
		{
			var prAddress = new PRAddress()
			{
				Address1 = address.AddressLine1,
				Address2 = address.AddressLine2,
				City = address.City,
				State = address.State,
				ZipCode = address.PostalCode
			};

			return GetLocationCode(prAddress);
		}

		public IEnumerable<Payroll.Data.PRTaxType> GetTaxTypes(PRLocationCode locationCode)
		{
			try
			{
				return base.Channel.GetTaxTypes(GetLicenseForPayrollSvc(), locationCode);
			}
			catch (FaultException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		public IEnumerable<Payroll.Data.PRTaxType> GetTaxTypes(string taxLocationCode, string taxMunicipalCode, string taxSchoolCode)
		{
			return GetTaxTypes(CreateLocationCode(taxLocationCode, taxMunicipalCode, taxSchoolCode));
		}

		public IEnumerable<Payroll.Data.PRTaxType> GetSpecificTaxTypes(string typeName, PRLocationFinder locationFinder)
		{
			try
			{
				return base.Channel.GetSpecificTaxTypes(GetLicenseForPayrollSvc(), typeName, locationFinder);
			}
			catch (FaultException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		public IEnumerable<Payroll.Data.PRTaxType> GetSpecificTaxTypes(string typeName, string locationSearch)
		{
			return GetSpecificTaxTypes(typeName, new PRLocationFinder() { LocationSearch = locationSearch });
		}

		public Dictionary<string, SymmetryToAatrixTaxMapping> GetAatrixTaxMapping(IEnumerable<PX.Payroll.Data.PRTaxType> uniqueTaxes)
		{
			try
			{
				return base.Channel.GetAatrixTaxMapping(GetLicenseForPayrollSvc(), uniqueTaxes);
			}
			catch (FaultException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		private PRLocationCode CreateLocationCode(string taxLocationCode, string taxMunicipalCode, string taxSchoolCode)
		{
			return new PRLocationCode()
			{
				TaxLocationCode = taxLocationCode,
				TaxMunicipalCode = taxMunicipalCode,
				TaxSchoolCode = taxSchoolCode
			};
		}

		public byte[] GetTaxDefinitionAssembly()
		{
			try
			{
				return base.Channel.GetTaxDefinitionAssembly(GetLicenseForPayrollSvc());
			}
			catch (FaultException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		public byte[] GetTaxMappingFile()
		{
			try
			{
				return base.Channel.GetTaxMappingFile(GetLicenseForPayrollSvc());
			}
			catch (FaultException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		public static AppLicenseInfo GetLicenseForPayrollSvc()
		{
			PXLicense license = PXLicenseHelper.License;
			return new AppLicenseInfo()
			{
				Restriction = license?.Restriction,
				Signature = license?.Signature,
				Licensed = license?.Licensed ?? false
			};
		}
	}

}
