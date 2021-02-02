using PX.Data;
using PX.SM;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class ExchangeEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			CRSMEmail activity = package.Message;
			EMailAccount account = package.Account;
			PXGraph graph = package.Graph;

			if (account.EmailAccountType != EmailAccountTypesAttribute.Exchange) return false;


			foreach (EPEmployee employee in PXSelectJoin<EPEmployee,
				InnerJoin<EMailSyncAccount, On<EPEmployee.bAccountID, Equal<EMailSyncAccount.employeeID>>>,
				Where<EMailSyncAccount.emailAccountID, Equal<Required<EMailSyncAccount.emailAccountID>>>>.Select(graph, account.EmailAccountID))
			{
				if (employee.UserID != null)
				{
					activity.OwnerID = employee.UserID;
					return true;
				}
			}

			return false;
		}
	}
}
