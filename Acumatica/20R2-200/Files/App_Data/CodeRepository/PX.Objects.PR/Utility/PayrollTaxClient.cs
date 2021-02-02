using PX.Api.Payroll;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CR;
using PX.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace PX.Objects.PR
{
	public class PayrollTaxClient : PayrollClientBase<IPayrollTaxService>, IPayrollClientTaxService
    {
        private const string ServiceName = "Tax";
        public PayrollTaxClient() : base(ServiceName) { }

        public IEnumerable<PRPayrollCalculation> Calculate(IEnumerable<PRPayroll> payrolls)
        {
			try
			{
				return base.Channel.Calculate(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc(), payrolls);
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
			}
		}

        public PRLocationCodeDescription GetLocationCode(IAddressBase address)
        {
			try
			{ 
				var prAddress = new PRAddress()
				{
					Address1 = address.AddressLine1,
					Address2 = address.AddressLine2,
					City = address.City,
					State = address.State,
					ZipCode = address.PostalCode
				};

				return base.Channel.GetLocationCode(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc(), prAddress);
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
			}
		}

		public Dictionary<int?, PRLocationCodeDescription> GetLocationCodes(IEnumerable<Address> addresses)
		{
			try
			{
				IEnumerable<PRAddress> prAddresses = addresses.Select(address => new PRAddress()
				{
					Address1 = address.AddressLine1,
					Address2 = address.AddressLine2,
					City = address.City,
					State = address.State,
					ZipCode = address.PostalCode,
					AddressID = address.AddressID
				});

				return base.Channel.GetLocationCodes(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc(), prAddresses);
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
			}
		}

		public IEnumerable<Payroll.Data.PRTaxType> GetTaxTypes(string taxLocationCode, string taxMunicipalCode, string taxSchoolCode)
        {
			try
			{
				return base.Channel.GetTaxTypes(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc(), CreateLocationCode(taxLocationCode, taxMunicipalCode, taxSchoolCode));
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
			}
		}

		public IEnumerable<Payroll.Data.PRTaxType> GetAllLocationTaxTypes(IEnumerable<Address> addresses)
		{
			try
			{
				IEnumerable<PRLocationCode> prLocationCodes = addresses.Select(address => CreateLocationCode(address.TaxLocationCode, address.TaxMunicipalCode, address.TaxSchoolCode));
				return base.Channel.GetAllLocationTaxTypes(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc(), prLocationCodes);
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
			}
		}

		public IEnumerable<Payroll.Data.PRTaxType> GetSpecificTaxTypes(string typeName, string locationSearch)
        {
			try
			{
				return base.Channel.GetSpecificTaxTypes(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc(), typeName, new PRLocationFinder() { LocationSearch = locationSearch });
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
			}
		}

        public Dictionary<string, SymmetryToAatrixTaxMapping> GetAatrixTaxMapping(IEnumerable<PX.Payroll.Data.PRTaxType> uniqueTaxes)
        {
			try
			{
				return base.Channel.GetAatrixTaxMapping(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc(), uniqueTaxes);
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
			}
		}

        public byte[] GetTaxMappingFile()
        {
			try
			{
				return base.Channel.GetTaxMappingFile(PayrollServiceLicenseHelper.GetLicenseForPayrollSvc());
			}
			catch (Exception ex)
			{
				throw ProcessWebServiceException(ex);
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


		private Exception ProcessWebServiceException(Exception ex)
		{
			if (ex is EndpointNotFoundException)
			{
				return new PXException(Messages.CantContactWebservice, ex);
			}
			else
			{
				Exception inner = ex.InnerException;

				FaultException<ExceptionDetail> faultException = ex as FaultException<ExceptionDetail>;
				if (faultException != null)
				{
					inner = new FaultExceptionWrapper(faultException);
					ex = new PXException(ex.Message, inner);
				}

				if (inner != null)
				{
					WebServiceErrorTracer tracer = null;
					AppDomain parentAppDomain = AppDomain.CurrentDomain.GetData(PXPayrollAssemblyScope.ParentAppDomainProperty) as AppDomain;
					if (parentAppDomain == null)
					{
						tracer = new WebServiceErrorTracer();
					}
					else
					{
						tracer = (WebServiceErrorTracer)parentAppDomain.CreateInstanceAndUnwrap(typeof(WebServiceErrorTracer).Assembly.FullName, typeof(WebServiceErrorTracer).FullName);
					}

					tracer.TraceException(inner);
				} 
			}

			return ex;
		}

		[Serializable]
		private class FaultExceptionWrapper : PXException, ISerializable
		{
			private string _StackTrace;

			public FaultExceptionWrapper(FaultException<ExceptionDetail> fault)
			{
				_Message = fault.Detail.InnerException.Message;
				_StackTrace = fault.Detail.InnerException.StackTrace;
			}

			private FaultExceptionWrapper(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				_StackTrace = (string)info.GetValue(nameof(_StackTrace), typeof(string));
			}

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				base.GetObjectData(info, context);
				info.AddValue(nameof(_StackTrace), _StackTrace, typeof(string));
			}

			public override string StackTrace => _StackTrace;
		}

		[Serializable]
		private class WebServiceErrorTracer : MarshalByRefObject
		{
			public void TraceException(Exception ex)
			{
				PXTrace.WriteError(ex);
			}
		}
	}
}
