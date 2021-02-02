using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	public class TXSetupMaint : PXGraph<TXSetupMaint>
	{
		public PXSelect<TXSetup> TXSetupRecord;
		public PXSave<TXSetup> Save;
		public PXCancel<TXSetup> Cancel;

		public TXSetupMaint()
		{
		}
	}
}
