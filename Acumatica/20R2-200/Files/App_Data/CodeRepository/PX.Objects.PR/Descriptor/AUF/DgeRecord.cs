using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class DgeRecord : AufRecord
	{
		public DgeRecord(string name, string ein) : base(AufRecordType.Dge)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new PXException(Messages.AatrixReportCompanyNameMissing);
			}

			if (string.IsNullOrEmpty(ein))
			{
				throw new PXException(Messages.AatrixReportEinMissing);
			}

			Name = name;
			Ein = ein;
		}

		public override string ToString()
		{
			bool isUS = LocationConstants.USCountryCode == CountryAbbr;

			object[] lineData =
			{
				Name,
				AdditionalCompanyName,
				FormatEin(Ein),
				AddressLine1,
				AddressLine2,
				City,
				isUS ? StateAbbr : null,
				isUS ? FormatZipCode(ZipCode) : null,
				isUS ? null : NonUSState,
				isUS ? null : NonUSPostalCode,
				Country,
				isUS ? null : CountryAbbr,
				ContactFirstName,
				FormatPhoneNumber(ContactPhoneNumber),
				ContactPhoneExtension,
				ContactMiddleName,
				ContactLastName,
				ContactNameSuffix
			};

			return FormatLine(lineData);
		}

		public virtual string Name { get; set; }
		public virtual string AdditionalCompanyName { get; set; }
		public virtual string Ein { get; set; }
		public virtual string AddressLine1 { get; set; }
		public virtual string AddressLine2 { get; set; }
		public virtual string City { get; set; }
		public virtual string StateAbbr { get; set; }
		public virtual string ZipCode { get; set; }
		public virtual string NonUSState { get; set; }
		public virtual string NonUSPostalCode { get; set; }
		public virtual string Country { get; set; }
		public virtual string CountryAbbr { get; set; }
		public virtual string ContactFirstName { get; set; }
		public virtual string ContactPhoneNumber { get; set; }
		public virtual string ContactPhoneExtension { get; set; }
		public virtual string ContactMiddleName { get; set; }
		public virtual string ContactLastName { get; set; }
		public virtual string ContactNameSuffix { get; set; }
	}
}
