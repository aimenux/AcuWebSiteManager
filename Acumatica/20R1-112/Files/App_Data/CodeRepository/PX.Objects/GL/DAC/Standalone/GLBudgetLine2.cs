using System;
using PX.Data;
using PX.Objects.GL.DAC;

namespace PX.Objects.GL.Standalone
{
	[Serializable]
	[PXHidden]
	public partial class GLBudgetLine2 : PX.Objects.GL.GLBudgetLine
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		public new abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		public new abstract class groupID : PX.Data.BQL.BqlGuid.Field<groupID> { }
		public new abstract class parentGroupID : PX.Data.BQL.BqlGuid.Field<parentGroupID> { }
	}
}
