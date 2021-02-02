using System;

namespace PX.Objects.PR.AUF
{
	public class EjwRecord : AufRecord
	{
		public EjwRecord(int jobID, DateTime weekEndDate) : base(AufRecordType.Ejw)
		{
			JobID = jobID;
			WeekEndDate = weekEndDate;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				JobID,
				WeekEndDate,
				WorkClassification,
				JobGross,
				TotalGross,
				AufConstants.UnusedField,
				FederalWithholding,
				StateWithholding,
				Sui,
				Sdi,
				OtherDeductions,
				JobNet,
				RegularHourlyRate,
				CashFringeRegularHourlyRate,
				RegularHoursDay1,
				RegularHoursDay2,
				RegularHoursDay3,
				RegularHoursDay4,
				RegularHoursDay5,
				RegularHoursDay6,
				RegularHoursDay7,
				OvertimeHourlyRate,
				OvertimeHoursDay1,
				OvertimeHoursDay2,
				OvertimeHoursDay3,
				OvertimeHoursDay4,
				OvertimeHoursDay5,
				OvertimeHoursDay6,
				OvertimeHoursDay7,
				AufConstants.UnusedField, // Double Time Hourly Rate
				AufConstants.UnusedField, // Doube Time Hours Day 1
				AufConstants.UnusedField, // Doube Time Hours Day 2
				AufConstants.UnusedField, // Doube Time Hours Day 3
				AufConstants.UnusedField, // Doube Time Hours Day 4
				AufConstants.UnusedField, // Doube Time Hours Day 5
				AufConstants.UnusedField, // Doube Time Hours Day 6
				AufConstants.UnusedField, // Doube Time Hours Day 7
				SSWithholding,
				MedicareWithholding,
				ClassLevel,
				ClassPercent,
				CashFringeOvertimeHourlyRate,
				AufConstants.UnusedField, // Cash Fringe Double Time Hourly Rate
				AufConstants.ManualInput, // Fringe Benefits Hours Day 1
				AufConstants.ManualInput, // Fringe Benefits Hours Day 2
				AufConstants.ManualInput, // Fringe Benefits Hours Day 3
				AufConstants.ManualInput, // Fringe Benefits Hours Day 4
				AufConstants.ManualInput, // Fringe Benefits Hours Day 5
				AufConstants.ManualInput, // Fringe Benefits Hours Day 6
				AufConstants.ManualInput, // Fringe Benefits Hours Day 7
				CheckNumber,
				WorkClassificationCode,
				AufConstants.UnusedField,
				IsPrevailingWage == false ? "X" : null,
				CheckDate,
				TotalHoursDay1,
				TotalHoursDay2,
				TotalHoursDay3,
				TotalHoursDay4,
				TotalHoursDay5,
				TotalHoursDay6,
				TotalHoursDay7,
				UnionDues
			};

			return FormatLine(lineData);
		}

		public virtual int JobID { get; set; }
		public virtual DateTime WeekEndDate { get; set; }
		public virtual string WorkClassification { get; set; }
		public virtual decimal? JobGross { get; set; }
		public virtual decimal? TotalGross { get; set; }
		public virtual decimal? FederalWithholding { get; set; }
		public virtual decimal? StateWithholding { get; set; }
		public virtual decimal? Sui { get; set; }
		public virtual decimal? Sdi { get; set; }
		public virtual decimal? OtherDeductions { get; set; }
		public virtual decimal? JobNet { get; set; }
		public virtual decimal? CashFringeRegularHourlyRate { get; set; }
		public virtual decimal? RegularHourlyRate { get; set; }
		public virtual decimal? RegularHoursDay1 { get; set; }
		public virtual decimal? RegularHoursDay2 { get; set; }
		public virtual decimal? RegularHoursDay3 { get; set; }
		public virtual decimal? RegularHoursDay4 { get; set; }
		public virtual decimal? RegularHoursDay5 { get; set; }
		public virtual decimal? RegularHoursDay6 { get; set; }
		public virtual decimal? RegularHoursDay7 { get; set; }
		public virtual decimal? OvertimeHourlyRate { get; set; }
		public virtual decimal? OvertimeHoursDay1 { get; set; }
		public virtual decimal? OvertimeHoursDay2 { get; set; }
		public virtual decimal? OvertimeHoursDay3 { get; set; }
		public virtual decimal? OvertimeHoursDay4 { get; set; }
		public virtual decimal? OvertimeHoursDay5 { get; set; }
		public virtual decimal? OvertimeHoursDay6 { get; set; }
		public virtual decimal? OvertimeHoursDay7 { get; set; }
		public virtual decimal? SSWithholding { get; set; }
		public virtual decimal? MedicareWithholding { get; set; }
		public virtual char? ClassLevel { get; set; }
		public virtual int? ClassPercent { get; set; }
		public virtual decimal? CashFringeOvertimeHourlyRate { get; set; }
		public virtual string CheckNumber { get; set; }
		public virtual string WorkClassificationCode { get; set; }
		public virtual bool? IsPrevailingWage { get; set; }
		public virtual DateTime? CheckDate { get; set; }
		public virtual decimal? TotalHoursDay1 { get; set; }
		public virtual decimal? TotalHoursDay2 { get; set; }
		public virtual decimal? TotalHoursDay3 { get; set; }
		public virtual decimal? TotalHoursDay4 { get; set; }
		public virtual decimal? TotalHoursDay5 { get; set; }
		public virtual decimal? TotalHoursDay6 { get; set; }
		public virtual decimal? TotalHoursDay7 { get; set; }
		public virtual decimal? UnionDues { get; set; }
	}
}
