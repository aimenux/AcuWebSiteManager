using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CR
{
	public class CRGramMaint : PXGraph<CRGramMaint, CRGrams>
	{
		#region Selects

		public PXSelect<CRGrams> Grams;

		public PXSetup<CRSetup> Setup;

		#endregion
	}
}
