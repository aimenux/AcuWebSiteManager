using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR.Extensions.CRDuplicateEntities;

namespace PX.Objects.CR
{
	[Obsolete(Common.Messages.WillBeRemovedInAcumatica2020R2)]
	public class CRGramMaint : PXGraph<CRGramMaint, CRGrams>
	{
		#region Selects

		public PXSelect<CRGrams> Grams;

		public PXSetup<CRSetup> Setup;

		#endregion
	}
}
