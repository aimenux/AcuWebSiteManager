using PX.Data;
using PX.Data.BQL;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR.Standalone
{
	/// <summary>
	/// Standalone DAC related to PR.Objects.PR.PREmployee />
	/// </summary>
	[PXCacheName("Payroll Employee")]
	[Serializable]
	public class PREmployee : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }
		/// <summary>
		/// Key field used to retrieve an Employee
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(EPEmployee.bAccountID))]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<PREmployee.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		#endregion
	}
}