using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.AP.DAC.ReportParameters
{
	public class AP1099ReportParameters: IBqlTable
	{
		#region OrganizationID

		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false)]
		public virtual int? OrganizationID { get; set; }
		
		#endregion

		#region FinYear

		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }

		[PXDBString(4, IsFixed = true)]
		[PXUIField(DisplayName = "1099 Year")]
		[PXSelector(typeof(Search4<AP1099Year.finYear,
			Where<AP1099Year.organizationID, Equal<Optional2<AP1099ReportParameters.organizationID>>,
				Or<Optional2<AP1099ReportParameters.organizationID>, IsNull>>,
			Aggregate<GroupBy<AP1099Year.finYear>>>))]
		public virtual String FinYear { get; set; }

		#endregion
	}
}
