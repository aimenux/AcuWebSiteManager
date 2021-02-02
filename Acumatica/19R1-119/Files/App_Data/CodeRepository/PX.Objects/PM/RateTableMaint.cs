using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.PM
{
	public class RateTableMaint : PXGraph<RateTableMaint>
	{
		public PXSelect<PMRateTable> RateTables;
        public PXSavePerRow<PMRateTable> Save;
        public PXCancel<PMRateTable> Cancel;
    }
}
