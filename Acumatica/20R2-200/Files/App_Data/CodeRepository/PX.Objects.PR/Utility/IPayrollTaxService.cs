using PX.Api.Payroll;
using PX.Payroll.Data;
using System.Collections.Generic;
using System.ServiceModel;

namespace PX.Payroll
{
	[ServiceContract]
    public interface IPayrollTaxService : IPayrollBaseService
    {
        [OperationContract]
        IEnumerable<PRPayrollCalculation> Calculate(AppLicenseInfo license, IEnumerable<PRPayroll> payrolls);

        [OperationContract]
        PRLocationCodeDescription GetLocationCode(AppLicenseInfo license, PRAddress address);

		[OperationContract]
		Dictionary<int?, PRLocationCodeDescription> GetLocationCodes(AppLicenseInfo license, IEnumerable<PRAddress> addresses);

		[OperationContract]
        IEnumerable<PRTaxType> GetTaxTypes(AppLicenseInfo license, PRLocationCode locationCode);

		[OperationContract]
		IEnumerable<Payroll.Data.PRTaxType> GetAllLocationTaxTypes(AppLicenseInfo license, IEnumerable<PRLocationCode> locationCodes);

		[OperationContract]
        IEnumerable<PRTaxType> GetSpecificTaxTypes(AppLicenseInfo license, string typeName, PRLocationFinder locationFinder);

        [OperationContract]
        Dictionary<string, SymmetryToAatrixTaxMapping> GetAatrixTaxMapping(AppLicenseInfo license, IEnumerable<PRTaxType> uniqueTaxes);

        [OperationContract]
        byte[] GetTaxMappingFile(AppLicenseInfo license);
    }
}
