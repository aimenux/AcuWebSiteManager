using System;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CR.Workflows;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.TM;

namespace PX.Objects.CR.BackwardCompatibility
{
	[PXHidden]
	public class DACWithOwnerID : IBqlTable
	{
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		[PXOwnerSelector]
		[PXDBInt]
		[PXUIField(DisplayName = "Owner")]
		public virtual int? OwnerID { get; set; }
		#endregion
	}
}
