using System.Runtime.Serialization;

using PX.Data;

namespace PX.Objects.GL.Exceptions
{
	public class FinancialPeriodWithIdNotFoundException : PXFinPeriodException
	{
		/// <summary>
		/// Gets the financial period ID for which the financial period was not found.
		/// </summary>
		public string FinancialPeriodId
		{
			get;
			private set;
		}

		public FinancialPeriodWithIdNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

		/// <param name="financialPeriodId">
		/// The financial period ID in the internal representation. 
		/// It will automatically be formatted for display in the error message.
		/// </param>
		public FinancialPeriodWithIdNotFoundException(string financialPeriodId)
			: base(string.IsNullOrEmpty(financialPeriodId) 
				? Messages.NoPeriodsDefined 
				: PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodWithId, FinPeriodIDAttribute.FormatForError(financialPeriodId)))
		{
			FinancialPeriodId = financialPeriodId;
		}
	}
}
