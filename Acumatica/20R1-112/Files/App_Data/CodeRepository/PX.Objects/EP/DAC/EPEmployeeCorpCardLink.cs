using PX.Data;
using PX.Data.BQL;
using PX.Objects.CA;

namespace PX.Objects.EP.DAC
{
	[PXCacheName(Messages.EmployeeCorpCardReference)]
	public class EPEmployeeCorpCardLink : IBqlTable
	{
		[PXDBInt(IsKey = true)]
		[PXEPEmployeeSelector]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>))]
		[PXUIField(DisplayName = "Employee ID")]
		public int? EmployeeID { get; set; }
		public abstract class employeeID : BqlInt.Field<employeeID> { }

		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<CACorpCard, Where<CACorpCard.corpCardID, Equal<Current<corpCardID>>>>))]
		[PXSelector(typeof(Search<CACorpCard.corpCardID>),
			typeof(CACorpCard.corpCardCD), typeof(CACorpCard.name), typeof(CACorpCard.cardNumber), typeof(CACorpCard.cashAccountID),
			SubstituteKey = typeof(CACorpCard.corpCardCD))]
		[PXUIField(DisplayName = "Corporate Card ID")]
		public int? CorpCardID { get; set; }
		public abstract class corpCardID : BqlInt.Field<corpCardID> { }
	}
}