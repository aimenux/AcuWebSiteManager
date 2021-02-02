using PX.Common;

namespace PX.Objects.GL
{
	public static class PredefinedRoles
	{
		public static string FinancialSupervisor => WebConfig.GetString("FinancialSupervisor", "Financial Supervisor");
		public static string ProjectAccountant => WebConfig.GetString("ProjectAccountant", "Project Accountant");
	}
}
