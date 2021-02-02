using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class EciRecord : AufRecord
	{
		public EciRecord(string employeeID) : base(AufRecordType.Eci)
		{
			_EmployeeID = employeeID;
		}

		public override string ToString()
		{
			List<object> lineData = new List<object>()
			{
				FormatSsn(SocialSecurityNumber, _EmployeeID, false),
				BirthDate,
				FirstName,
				MiddleName,
				LastName,
				NameSuffix
			};

			for (int i = 0; i < 12; i++)
			{
				lineData.Add(CoverageIndicator[i] == true ? "X" : null);
			}

			return FormatLine(lineData.ToArray());
		}

		public virtual string SocialSecurityNumber { get; set; }
		public virtual DateTime? BirthDate { get; set; }
		public virtual string FirstName { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string NameSuffix { get; set; }
		public virtual bool?[] CoverageIndicator { get; set; } = new bool?[12];

		private string _EmployeeID;
	}
}
