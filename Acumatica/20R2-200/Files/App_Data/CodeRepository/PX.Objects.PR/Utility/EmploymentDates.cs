using System;

namespace PX.Objects.PR
{
	public class EmploymentDates
	{
		public DateTime? InitialHireDate { get; }
		public DateTime? ContinuousHireDate { get; }
		public DateTime? RehireDateAuf { get; }
		public DateTime? TerminationDate { get; }
		public DateTime? TerminationDateAuf { get; }

		public EmploymentDates(DateTime? initialHireDate, DateTime? continuousHireDate, DateTime? rehireDateAuf, DateTime? terminationDate, DateTime? terminationDateAuf)
		{
			InitialHireDate = initialHireDate;
			ContinuousHireDate = continuousHireDate;
			RehireDateAuf = rehireDateAuf;
			TerminationDate = terminationDate;
			TerminationDateAuf = terminationDateAuf;
		}
	}
}
