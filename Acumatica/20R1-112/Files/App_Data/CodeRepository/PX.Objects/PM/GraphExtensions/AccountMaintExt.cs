using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM.GraphExtensions
{
	public class AccountMaintExt : PXGraphExtension<AccountMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}

		protected virtual void _(Events.RowDeleting<Account> e)
		{
			if (e.Row.AccountGroupID != null)
			{
				PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(Base, e.Row.AccountGroupID);
				if (accountGroup != null)
				{
					throw new PXException(Messages.CannotDeleteAccountMappedToAG, e.Row.AccountCD, accountGroup.GroupCD);
				}
				
			}
		}
	}
}
