using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class CfbRecord : AufRecord
	{
		public CfbRecord(string benefitCode, char benefitType) : base(AufRecordType.Cfb)
		{
			BenefitName = benefitCode;
			BenefitID = Math.Abs(benefitCode.GetHashCode());
			BenefitType = benefitType;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				BenefitID,
				BenefitType,
				BenefitName,
				VendorAddress,
				VendorCity,
				VendorState,
				VendorZipCode,
				FormatPhoneNumber(VendorPhone),
				AccountNumber,
				VendorContact,
				'E' // Payment method
			};

			return FormatLine(lineData);
		}

		public virtual int BenefitID { get; set; }
		public virtual char BenefitType { get; set; }
		public virtual string BenefitName { get; set; }
		public virtual string VendorAddress { get; set; }
		public virtual string VendorCity { get; set; }
		public virtual string VendorState { get; set; }
		public virtual string VendorZipCode { get; set; }
		public virtual string VendorPhone { get; set; }
		public virtual string AccountNumber { get; set; }
		public virtual string VendorContact { get; set; }
	}
}
