using PX.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class CmpRecord : AufRecord
	{
		public CmpRecord() : base(AufRecordType.Cmp)
		{
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Name))
			{
				throw new PXException(Messages.AatrixReportCompanyNameMissing);
			}

			bool isUS = LocationConstants.USCountryCode == CountryAbbr;

			object[] lineData =
			{
				Name,
				TradeName,
				AddressLine1,
				AddressLine2,
				City,
				isUS ? StateAbbr : null,
				County,
				CountyCode,
				isUS ? FormatZipCode(ZipCode) : null,
				Country,
				isUS ? null : CountryAbbr,
				isUS ? null : NonUSPostalCode,
				DoingBusinessAsName,
				BranchName,
				TaxArea,
				FormatPhoneNumber(PhoneNumber),
				PhoneExtension,
				FormatPhoneNumber(FaxNumber),
				IndustryCode,
				isUS ? FormatEin(Ein) : null,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				ContactTitle,
				ContactName,
				FormatPhoneNumber(ContactPhoneNumber),
				ContactPhoneExtension,
				ContactAddress,
				NumberOfEmployees,
				AufConstants.UnusedField,
				ContactEmail,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				TerminationDate,
				isUS ? null : NonUSState,
				AufConstants.ManualInput, // Employment Code
				isUS ? null : NonUSNationalID,
				KindOfEmployer
			};

			StringBuilder builder = new StringBuilder(FormatLine(lineData));
			GtoList?.ForEach(gto => builder.Append(gto.ToString()));
			CjiList?.ForEach(cji => builder.Append(cji.ToString()));
			CfbList?.ForEach(cfb => builder.Append(cfb.ToString()));

			if (Ale != null)
			{
				builder.Append(Ale.ToString());
			}

			AggList?.ForEach(agg => builder.Append(agg.ToString()));

			if (Dge != null)
			{
				builder.Append(Dge.ToString());
			}

			return builder.ToString();
		}

		#region Data
		public virtual string Name { get; set; }
		public virtual string TradeName { get; set; }
		public virtual string AddressLine1 { get; set; }
		public virtual string AddressLine2 { get; set; }
		public virtual string City { get; set; }
		public virtual string StateAbbr { get; set; }
		public virtual string County { get; set; }
		public virtual string CountyCode { get; set; }
		public virtual string ZipCode { get; set; }
		public virtual string Country { get; set; }
		public virtual string CountryAbbr { get; set; }
		public virtual string NonUSPostalCode { get; set; }
		public virtual string DoingBusinessAsName { get; set; }
		public virtual string BranchName { get; set; }
		public virtual string TaxArea { get; set; }
		public virtual string PhoneNumber { get; set; }
		public virtual string PhoneExtension { get; set; }
		public virtual string FaxNumber { get; set; }
		public virtual string IndustryCode { get; set; }
		public virtual string Ein { get; set; }
		public virtual string ContactTitle { get; set; }
		public virtual string ContactName { get; set; }
		public virtual string ContactPhoneNumber { get; set; }
		public virtual string ContactPhoneExtension { get; set; }
		public virtual string ContactAddress { get; set; }
		public virtual int? NumberOfEmployees { get; set; }
		public virtual string ContactEmail { get; set; }
		public virtual DateTime? TerminationDate { get; set; }
		public virtual string NonUSState { get; set; }
		public virtual string NonUSNationalID { get; set; }
		public virtual char? KindOfEmployer { get; set; }
		#endregion Data

		#region Children Records
		public List<GtoRecord> GtoList { private get; set; }
		public List<CjiRecord> CjiList { private get; set; }
		public List<CfbRecord> CfbList { private get; set; }
		public AleRecord Ale { get; set; }
		public List<AggRecord> AggList { private get; set; }
		public DgeRecord Dge { private get; set; }
		#endregion
	}
}
