using System;
using System.Collections.Generic;
using System.Linq;
using PX.Api.Payroll;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PR
{
	public static class TaxUpdateHelpers
	{
		public static bool CheckTaxUpdateTimestamp(PXView updateHistoyView)
		{
			PRTaxUpdateHistory updateHistory = updateHistoyView.SelectSingle() as PRTaxUpdateHistory;

			DateTime utcNow = DateTime.UtcNow;
			DateTime? serverTimestamp = updateHistory?.ServerTaxDefinitionTimestamp;
			if (updateHistory != null &&
				(serverTimestamp == null || updateHistory.LastCheckTime == null || updateHistory.LastCheckTime < utcNow.AddDays(-1)))
			{
				try
				{
					serverTimestamp = new PayrollUpdateClient().GetTaxDefinitionTimestamp();
					updateHistory.ServerTaxDefinitionTimestamp = serverTimestamp;
					updateHistory.LastCheckTime = utcNow;
					updateHistoyView.Cache.Update(updateHistory);

					using (PXTransactionScope ts = new PXTransactionScope())
					{
						updateHistoyView.Cache.PersistUpdated(updateHistory);
						ts.Complete();
					}
					updateHistoyView.Cache.Persisted(false);
				}
				catch { }
			}

			return !(updateHistory == null || updateHistory.LastUpdateTime < serverTimestamp) ;
		}

		[Serializable]
		[PXHidden]
		public class UpdateTaxesWarning : IBqlTable
		{
			#region Message
			public abstract class message : BqlString.Field<message> { }
			[PXString]
			public string Message { get; set; }
			#endregion
		}
	}
}
