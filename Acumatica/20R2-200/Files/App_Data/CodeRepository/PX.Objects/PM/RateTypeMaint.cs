using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.PM
{
	public class RateTypeMaint : PXGraph<RateTypeMaint>
	{
		public PXSelect<PMRateType> RateTypes;
        public PXSavePerRow<PMRateType> Save;
        public PXCancel<PMRateType> Cancel;
    }
}
