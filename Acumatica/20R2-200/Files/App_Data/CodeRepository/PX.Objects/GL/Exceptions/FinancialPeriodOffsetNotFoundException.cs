using System;
using System.Runtime.Serialization;

using PX.Data;

namespace PX.Objects.GL.Exceptions
{
	public class FinancialPeriodOffsetNotFoundException : PXFinPeriodException
	{
		/// <summary>
		/// Gets the financial period ID for which the offset period was not found.
		/// </summary>
		public string FinancialPeriodId
		{
			get;
			private set;
		}

		/// <summary>
		/// The positive or negative number of periods offset from the <see cref="FinancialPeriodId"/>,
		/// for which the financial period was not found.
		/// </summary>
		public int Offset
		{
			get;
			private set;
		}

		public FinancialPeriodOffsetNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

		/// <param name="financialPeriodId">
		/// The financial period ID in the internal representation. 
		/// It will automatically be formatted for display in the error message.
		/// </param>
		public FinancialPeriodOffsetNotFoundException(string financialPeriodId, int offset)
			: base(GetMessage(financialPeriodId, offset))
		{
			FinancialPeriodId = financialPeriodId;
			Offset = offset;
		}

		private static string GetMessage(string financialPeriodId, int offset)
		{
			if (string.IsNullOrEmpty(financialPeriodId)) return Messages.NoPeriodsDefined;
			else switch (offset)
				{
					case 0: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodWithId, FinPeriodIDAttribute.FormatForError(financialPeriodId));
					case -1: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodBefore, FinPeriodIDAttribute.FormatForError(financialPeriodId));
					case 1: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodAfter, FinPeriodIDAttribute.FormatForError(financialPeriodId));
					default: return PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodForOffset,
						Math.Abs(offset),
						offset > 0 ? Messages.AfterLowercase : Messages.BeforeLowercase,
						FinPeriodIDAttribute.FormatForError(financialPeriodId));
				}
		}
	}
}
