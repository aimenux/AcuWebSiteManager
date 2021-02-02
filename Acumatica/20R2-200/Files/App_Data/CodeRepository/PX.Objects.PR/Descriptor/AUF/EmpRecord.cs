using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class EmpRecord : AufRecord
	{
		public EmpRecord(string employeeID) : base(AufRecordType.Emp)
		{
			EmployeeID = employeeID;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(FirstName))
			{
				throw new PXException(Messages.AatrixReportFirstNameMissing, EmployeeID);
			}

			if (string.IsNullOrEmpty(LastName))
			{
				throw new PXException(Messages.AatrixReportLastNameMissing, EmployeeID);
			}

			bool isUS = LocationConstants.USCountryCode == CountryAbbr;
			bool isCanada = LocationConstants.CanadaCountryCode == CountryAbbr;
			bool isAlaska = isUS && LocationConstants.AlaskaStateAbbr == StateAbbr;

			object[] lineData =
			{
				FirstName,
				MiddleName,
				LastName,
				NameSuffix,
				isUS ? FormatSsn(SocialSecurityNumber, EmployeeID) : null,
				AddressLine1,
				City,
				County,
				CountyCode,
				isUS ? StateAbbr : null,
				isUS ? FormatZipCode(ZipCode) : null,
				Country,
				isUS ? null : CountryAbbr,
				isUS ? null : NonUSPostalCode,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				IsFemale == true ? "X" : null,
				IsDisabled == true ? "X" : null,
				HireDate,
				RehireDate == null || RehireDate < FireDate ? FireDate : null,
				MedicalCoverageDate,
				BirthDate,
				PayRate,
				FederalExemptions,
				IsHourlyPay == true ? 'H' : 'S',
				IsFullTime == true ? "X" : null,
				Title,
				StateOfHireAbbr,
				WorkType,
				AufConstants.UnusedField,
				HasHealthBenefits == true ? "X" : null,
				FormatPhoneNumber(PhoneNumber),
				IsSeasonal == true ? "X" : null,
				WorkersCompClass,
				WorkersCompSubclass,
				AufConstants.UnusedField,
				MaritalStatus,
				EmployeeID,
				IsStatutoryEmployee == true ? "X" : null,
				HasRetirementPlan == true ? "X" : null,
				HasThirdPartySickPay == true ? "X" : null,
				HasDirectDeposit == true ? "X" : null,
				AddressLine2,
				AufConstants.UnusedField,
				Email,
				HasElectronicW2 == true ? "X" : null,
				isUS ? null : NonUSState,
				RehireDate,
				isUS || isCanada ? EmploymentCode : null,
				isAlaska ? FullOccupationalTitle : null,
				isAlaska ? GeographicCode : null,
				PensionDate,
				isUS ? null : NonUSNationalID,
				AufConstants.UnusedField,
				isCanada && CppExempt == true ? "X" : null,
				isCanada && EmploymentInsuranceExempt == true ? "X" : null,
				isCanada && ProvincialParentalInsurancePlanExempt == true ? "X" : null,
				StateExemptions,
				Ethnicity,
				AufConstants.ManualInput // Sort Text
			};

			StringBuilder builder = new StringBuilder(FormatLine(lineData));
			GenList?.ForEach(gen => builder.Append(gen.ToString()));
			EfbList?.ForEach(efb => builder.Append(efb.ToString()));

			if (Ecv != null)
			{
				builder.Append(Ecv.ToString());
			}

			if (Eci != null)
			{
				builder.Append(Eci.ToString());
			}

			return builder.ToString();
		}

		#region Data
		public virtual string FirstName { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string NameSuffix { get; set; }
		public virtual string SocialSecurityNumber { get; set; }
		public virtual string AddressLine1 { get; set; }
		public virtual string City { get; set; }
		public virtual string County { get; set; }
		public virtual string CountyCode { get; set; }
		public virtual string StateAbbr { get; set; }
		public virtual string ZipCode { get; set; }
		public virtual string Country { get; set; }
		public virtual string CountryAbbr { get; set; }
		public virtual string NonUSPostalCode { get; set; }
		public virtual bool? IsFemale { get; set; }
		public virtual bool? IsDisabled { get; set; }
		public virtual DateTime? HireDate { get; set; }
		public virtual DateTime? FireDate { get; set; }
		public virtual DateTime? MedicalCoverageDate { get; set; }
		public virtual DateTime? BirthDate { get; set; }
		public virtual decimal? PayRate { get; set; }
		public virtual int? FederalExemptions { get; set; }
		public virtual bool? IsHourlyPay { get; set; }
		public virtual bool? IsFullTime { get; set; }
		public virtual string Title { get; set; }
		public virtual string StateOfHireAbbr { get; set; }
		public virtual string WorkType { get; set; }
		public virtual bool? HasHealthBenefits { get; set; }
		public virtual string PhoneNumber { get; set; }
		public virtual bool? IsSeasonal { get; set; }
		public virtual string WorkersCompClass { get; set; }
		public virtual string WorkersCompSubclass { get; set; }
		public virtual string MaritalStatus { get; set; }
		public virtual string EmployeeID { get; set; }
		public virtual bool? IsStatutoryEmployee { get; set; }
		public virtual bool? HasRetirementPlan { get; set; }
		public virtual bool? HasThirdPartySickPay { get; set; }
		public virtual bool? HasDirectDeposit { get; set; }
		public virtual string AddressLine2 { get; set; }
		public virtual string Email { get; set; }
		public virtual bool? HasElectronicW2 { get; set; }
		public virtual string NonUSState { get; set; }
		public virtual DateTime? RehireDate { get; set; }
		public virtual string EmploymentCode { get; set; }
		public virtual string FullOccupationalTitle { get; set; }
		public virtual string GeographicCode { get; set; }
		public virtual DateTime? PensionDate { get; set; }
		public virtual string NonUSNationalID { get; set; }
		public virtual bool? CppExempt { get; set; }
		public virtual bool? EmploymentInsuranceExempt { get; set; }
		public virtual bool? ProvincialParentalInsurancePlanExempt { get; set; }
		public virtual int? StateExemptions { get; set; }
		public virtual string Ethnicity { get; set; }
		#endregion

		#region Children records
		public List<GenRecord> GenList { get; set; }
		public List<EfbRecord> EfbList { private get; set; }
		public EcvRecord Ecv { private get; set; }
		public EciRecord Eci { private get; set; }
		#endregion
	}
}
