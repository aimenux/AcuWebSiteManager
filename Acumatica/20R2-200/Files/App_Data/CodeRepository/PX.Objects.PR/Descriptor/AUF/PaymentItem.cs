using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR.AUF
{
	public abstract class PaymentItem : AufRecord
	{
		protected PaymentItem(AufRecordType recordType, DateTime checkDate, int pimID) : base(recordType)
		{
			CheckDate = checkDate;
			PimID = pimID;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				State,
				CheckDate,
				PimID,
				PaymentAmount,
				PaymentDate
			};

			return FormatLine(lineData);
		}

		public virtual string State { get; set; }
		public virtual DateTime CheckDate { get; set; }
		public virtual int PimID { get; set; }
		public virtual decimal? PaymentAmount { get; set; }
		public virtual DateTime? PaymentDate { get; set; }
	}
}
