using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CS;

namespace PX.Objects.PO.LandedCosts
{
	public class LandedCostDocNumberingAttribute : AutoNumberAttribute
	{
		public LandedCostDocNumberingAttribute()
			: base(typeof(POSetup.landedCostDocNumberingID), typeof(POLandedCostDoc.docDate))
		{
			
		}
	}
}
