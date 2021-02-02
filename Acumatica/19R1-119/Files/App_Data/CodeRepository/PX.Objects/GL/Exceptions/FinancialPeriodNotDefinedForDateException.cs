using System;
using System.Runtime.Serialization;

using PX.Data;

namespace PX.Objects.GL.Exceptions
{
	public class FinancialPeriodNotDefinedForDateException : PXFinPeriodException
	{
		/// <summary>
		/// Gets the date for which the financial period was not found.
		/// </summary>
		public DateTime? Date
		{
			get;
			private set;
		}

		public FinancialPeriodNotDefinedForDateException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

		public FinancialPeriodNotDefinedForDateException(DateTime? date)
			: base(date == null 
				? Messages.NoPeriodsDefined 
				: PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodForDate, date?.ToShortDateString()))
		{
			Date = date;
		}
	}
}
