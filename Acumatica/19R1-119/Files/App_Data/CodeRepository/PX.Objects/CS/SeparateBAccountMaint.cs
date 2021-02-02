using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.CS
{
	public class SeparateBAccountMaint: BusinessAccountGraphBase<BAccount, BAccount, Where<True, Equal<True>>>
	{
		#region Events Handlers

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		protected virtual void BAccount_DefLocationID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void BAccount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			this.OnBAccountRowInserted(sender, e);
		}

		protected virtual void BAccount_AcctName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.OnBAccountAcctNameFieldUpdated(sender, e);
		}

		#endregion
	}
}
