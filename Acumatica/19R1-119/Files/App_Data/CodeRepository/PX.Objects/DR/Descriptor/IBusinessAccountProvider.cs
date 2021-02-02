using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;

namespace PX.Objects.DR.Descriptor
{
	public interface IBusinessAccountProvider
	{
		EPEmployee GetEmployee(int? employeeID);

		SalesPerson GetSalesPerson(int? salesPersonID); 

		/// <summary>
		/// Retrieve the location matching the given business account ID
		/// and business account location ID.
		/// </summary>
		Location GetLocation(int? businessAccountID, int? businessAccountLocationId);
	}
}
