using PX.CS.Contracts.Interfaces;
using PX.Objects.CR;
using PX.Payroll.Data;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public interface IPayrollClientTaxService
	{
		IEnumerable<PRPayrollCalculation> Calculate(IEnumerable<PRPayroll> payrolls);
		PRLocationCodeDescription GetLocationCode(IAddressBase address);
		Dictionary<int?, PRLocationCodeDescription> GetLocationCodes(IEnumerable<Address> addresses);
		IEnumerable<Payroll.Data.PRTaxType> GetTaxTypes(string taxLocationCode, string taxMunicipalCode, string taxSchoolCode);
		IEnumerable<Payroll.Data.PRTaxType> GetAllLocationTaxTypes(IEnumerable<Address> addresses);
		IEnumerable<Payroll.Data.PRTaxType> GetSpecificTaxTypes(string typeName, string locationSearch);
		Dictionary<string, SymmetryToAatrixTaxMapping> GetAatrixTaxMapping(IEnumerable<Payroll.Data.PRTaxType> uniqueTaxes);
		byte[] GetTaxMappingFile();
	}
}
