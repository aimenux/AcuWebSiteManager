using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CS;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class PRAcaDedBenCodeMaint : PXGraphExtension<PRDedBenCodeMaint>
	{
		protected virtual void _(Events.RowUpdating<PRDeductCode> e)
		{
			PRDeductCode row = e.NewRow;
			if (row == null)
			{
				return;
			}

			if (row.IsWorkersCompensation == true)
			{
				PRAcaDeductCode acaExt = PXCache<PRDeductCode>.GetExtension<PRAcaDeductCode>(row);
				acaExt.AcaApplicable = false;
			}
		}
	}
}