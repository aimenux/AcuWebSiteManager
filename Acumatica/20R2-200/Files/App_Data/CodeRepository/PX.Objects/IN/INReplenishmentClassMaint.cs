using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.IN
{
	public class INReplenishmentClassMaint : PXGraph<INReplenishmentClassMaint>
	{
		public PXSelect<INReplenishmentClass> Classes;
        public PXSavePerRow<INReplenishmentClass> Save;
        public PXCancel<INReplenishmentClass> Cancel;
    }
}
