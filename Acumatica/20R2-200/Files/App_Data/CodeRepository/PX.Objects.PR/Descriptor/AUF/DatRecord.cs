using System;

namespace PX.Objects.PR.AUF
{
	public class DatRecord : AufRecord
	{
		public DatRecord(DateTime start, DateTime end) : base(AufRecordType.Dat)
		{
			FirstDate = start;
			LastDate = end;
			Year = end.Year;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				Year,
				Quarter,
				Month,
				_FirstDate,
				_LastDate
			};

			return FormatLine(lineData);
		}

		public virtual int Year { get; set; }
		public virtual int? Quarter { get; set; }
		public virtual int? Month { get; set; }

		private DateTime _FirstDate;
		public virtual DateTime FirstDate
		{
			get
			{
				return _FirstDate.Date;
			}
			set
			{
				_FirstDate = value;
			}
		}
		private DateTime _LastDate;
		public virtual DateTime LastDate
		{
			get
			{
				return _LastDate.Date.AddDays(1).AddTicks(-1);
			}
			set
			{
				_LastDate = value;
			}
		}
	}
}
