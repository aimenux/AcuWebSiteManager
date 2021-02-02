using System;
using System.Diagnostics;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.Common;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.Common.MigrationMode;
using PX.Objects.TX;

namespace PX.Objects.AR.Standalone
{
	[PXHidden]
	[Serializable]
	public partial class ARRegister2 : AR.ARRegister
	{
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		public override long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region ClosedFinPeriodID
		public new abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		#endregion
		#region ClosedTranPeriodID
		public new abstract class closedTranPeriodID : PX.Data.BQL.BqlString.Field<closedTranPeriodID> { }
		#endregion
	}
}
