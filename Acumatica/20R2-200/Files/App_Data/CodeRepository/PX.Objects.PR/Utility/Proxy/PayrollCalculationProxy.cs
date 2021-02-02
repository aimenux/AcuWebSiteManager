using PX.Api.Payroll;
using PX.Data;
using PX.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PX.Objects.PR
{
	[Serializable]
	public class PayrollCalculationProxy : PayrollBaseProxy<PayrollCalculationProxy>
	{
		private Dictionary<string, Payroll> _Payrolls;

		public PayrollCalculationProxy()
		{
			_Payrolls = new Dictionary<string, Payroll>();
		}

		public string AddPayroll(PRPayrollBase payrollBase, List<PRWage> wages, List<PRBenefit> benefits)
		{
			if (!DynamicAssemblies.Any())
			{
				throw new PXException(Messages.CantFindTaxDefinitionAssembly);
			}

			var payroll = new Payroll(DynamicAssemblies,
									  payrollBase,
									  wages,
									  benefits);

			_Payrolls.Add(payrollBase.ReferenceNbr, payroll);
			return payrollBase.ReferenceNbr;
		}

		public void AddTaxSetting(
			string referenceNbr,
			string taxUniqueCode,
			decimal periodAmount,
			decimal wtdAmount,
			decimal mtdAmount,
			decimal qtdAmount,
			decimal ytdAmount,
			decimal taxableWagesYtd,
			decimal mostRecentWH,
			IPRReflectiveSettingMapper<TaxTypeAttribute> taxTypeSetting,
			Dictionary<string, string> settingDictionary)
		{
			_Payrolls[referenceNbr].AddTaxSetting(taxUniqueCode, periodAmount, wtdAmount, mtdAmount, qtdAmount, ytdAmount, taxableWagesYtd, mostRecentWH, taxTypeSetting, settingDictionary);
		}

		public void AddEmployeeSetting(string referenceNbr, IPRReflectiveSettingMapper<EmployeeLocationSettingsAttribute> employeeSettingMapper, Dictionary<string, string> settingDictionary)
		{
			_Payrolls[referenceNbr].AddEmployeeSetting(employeeSettingMapper, settingDictionary);
		}

		public void SetEmployeeResidenceLocationCode(string referenceNbr, string residenceLocationCode)
		{
			_Payrolls[referenceNbr].EmployeeResidenceLocationCode = residenceLocationCode;
		}

		public IEnumerable<PRPayrollCalculation> CalculatePayroll()
		{
			var payrollClientService = new PayrollTaxClient();
			return payrollClientService.Calculate(_Payrolls.Values.Select(pr => (PRPayroll)pr));
		}

		[Serializable]
		private class Payroll
		{
			private IEnumerable<Assembly> _Assemblies;

			private PRPayrollBase _PayrollBase;

			private List<PRWage> _Wages;
			private List<PRBenefit> _Benefits;
			private List<PRTax> _TaxSettings;
			private List<PX.Payroll.Data.PREmployee> _EmployeeSettings;

			public string EmployeeResidenceLocationCode { get; set; }

			internal Payroll(IEnumerable<Assembly> assemblies, PRPayrollBase payrollBase, List<PRWage> wages, List<PRBenefit> benefits)
			{
				_Assemblies = assemblies;

				_PayrollBase = payrollBase;
				_Wages = wages;
				_Benefits = benefits;
				_TaxSettings = new List<PRTax>();
				_EmployeeSettings = new List<PX.Payroll.Data.PREmployee>();
			}

			public void AddTaxSetting(
				string taxUniqueCode,
				decimal periodAmount,
				decimal wtdAmount,
				decimal mtdAmount,
				decimal qtdAmount,
				decimal ytdAmount,
				decimal taxableWagesYtd,
				decimal mostRecentWH,
				IPRReflectiveSettingMapper<TaxTypeAttribute> taxTypeSetting,
				Dictionary<string, string> settingDictionary)
			{
				var taxSetting = PRTax.Create(_Assemblies, taxTypeSetting, settingDictionary);
				if (taxSetting == null)
				{
					throw new PXException(Messages.InvalidTaxDefinitionAssembly);
				}

				taxSetting.TaxUniqueCode = taxUniqueCode;
				taxSetting.PeriodWH = periodAmount;
				taxSetting.WTDAmount = wtdAmount;
				taxSetting.MTDAmount = mtdAmount;
				taxSetting.QTDAmount = qtdAmount;
				taxSetting.YTDAmount = ytdAmount;
				taxSetting.TaxableWagesYtd = taxableWagesYtd;
				taxSetting.MostRecentWH = mostRecentWH;
				_TaxSettings.Add(taxSetting);
			}

			public void AddEmployeeSetting(IPRReflectiveSettingMapper<EmployeeLocationSettingsAttribute> employeeSettingMapper, Dictionary<string, string> settingDictionary)
			{
				var employeeSetting = PX.Payroll.Data.PREmployee.Create(_Assemblies, employeeSettingMapper, settingDictionary);
				if (employeeSetting == null)
				{
					throw new PXException(Messages.InvalidTaxDefinitionAssembly);
				}
				_EmployeeSettings.Add(employeeSetting);
			}

			public static explicit operator PRPayroll(Payroll payroll)
			{
				var prPayroll = new PRPayroll(payroll._PayrollBase);

				prPayroll.Wages = payroll._Wages;
				prPayroll.Benefits = payroll._Benefits;
				prPayroll.TaxSettings = payroll._TaxSettings;
				prPayroll.EmployeeSettings = payroll._EmployeeSettings;
				prPayroll.EmployeeResidenceLocationCode = payroll.EmployeeResidenceLocationCode;

				return prPayroll;
			}
		}
	}
}
