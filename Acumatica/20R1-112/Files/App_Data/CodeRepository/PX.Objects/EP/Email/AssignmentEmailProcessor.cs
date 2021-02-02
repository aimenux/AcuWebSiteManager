using PX.Data;
using PX.SM;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class AssignmentEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			CRSMEmail activity = package.Message;
			EMailAccount account = package.Account;

			if (activity.IsIncome != true
			    || activity.OwnerID != null
			    || activity.WorkgroupID != null
			    || activity.ClassID == CRActivityClass.EmailRouting)
				return false;

			bool assigned = false;
			if (account.DefaultEmailAssignmentMapID != null)
			{
				var processor = PXGraph.CreateInstance<EPAssignmentProcessor<CRSMEmail>>();
				assigned = processor.Assign(activity, account.DefaultEmailAssignmentMapID);
			}
			if (!assigned)
			{
				activity.OwnerID = account.DefaultOwnerID;
				activity.WorkgroupID = account.DefaultWorkgroupID;
			}

			return true;
		}
	}
}
