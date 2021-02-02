using System;

namespace PX.Objects.PR.AUF
{
	public class ClpRecord : PaymentItem
	{
		public ClpRecord(DateTime checkDate, int pimID) : base(AufRecordType.Clp, checkDate, pimID) { }
	}
}
