using System;

namespace PX.Objects.PR.AUF
{
	public class CspRecord : PaymentItem
	{
		public CspRecord(DateTime checkDate, int pimID) : base(AufRecordType.Csp, checkDate, pimID) { }
	}
}
