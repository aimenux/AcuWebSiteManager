using PX.Data;
using System;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public abstract class AufRecord
	{
		public char DelimiterCharacter;
		public string Endline;

		private AufRecordType _RecordType;

		public AufRecord(AufRecordType recordType, char delimiterCharacter = AufConstants.DefaultDelimiterCharacter, string endline = AufConstants.DefaultEndline)
		{
			DelimiterCharacter = delimiterCharacter;
			Endline = endline;
			_RecordType = recordType;
		}

		public abstract override string ToString();

		protected string FormatLine(params object[] lineData)
		{
			StringBuilder builder = new StringBuilder(AufConstants.RecordNames[_RecordType]);

			foreach (object data in lineData)
			{
				builder.Append(DelimiterCharacter);

				if (data is decimal)
				{
					builder.Append(((decimal)data).ToString("0.00"));
				}
				else if (data is DateTime)
				{
					builder.Append(((DateTime)data).ToString("MM/dd/yyyy"));
				}
				else if (data != null)
				{
					builder.Append(data.ToString());
				}
			}

			builder.Append(Endline);
			return builder.ToString();
		}

		protected static string FormatZipCode(string zipCode)
		{
			if (!string.IsNullOrEmpty(zipCode))
			{
				return new string(zipCode.ToCharArray().Where(x => x >= '0' && x <= '9').ToArray());
			}

			return null;
		}

		protected static string FormatPhoneNumber(string phoneNumber)
		{
			if (!string.IsNullOrEmpty(phoneNumber))
			{
				return new string(phoneNumber.ToCharArray().Where(x => x >= '0' && x <= '9').ToArray());
			}

			return null;
		}

		protected static string FormatEin(string ein)
		{
			if (!string.IsNullOrEmpty(ein))
			{
				string filtered = new string(ein.ToCharArray().Where(x => x >= '0' && x <= '9').ToArray());
				if (!string.IsNullOrEmpty(filtered))
				{
					return filtered;
				} 
			}

			throw new PXException(Messages.AatrixReportEinMissing);
		}

		protected static string FormatSsn(string ssn, string employeeID, bool required = true)
		{
			if (!string.IsNullOrEmpty(ssn))
			{
				string filtered = new string(ssn.ToCharArray().Where(x => x >= '0' && x <= '9').ToArray());
				if (!string.IsNullOrEmpty(filtered))
				{
					if (filtered.Length == 9)
					{
						return filtered;
					}
					else
					{
						throw new PXException(Messages.AatrixReportInvalidSsn, employeeID, ssn);
					}
				}
			}
			else if (required)
			{
				throw new PXException(Messages.AatrixReportSsnNotSet, employeeID);
			}

			return null;
		}
	}
}
