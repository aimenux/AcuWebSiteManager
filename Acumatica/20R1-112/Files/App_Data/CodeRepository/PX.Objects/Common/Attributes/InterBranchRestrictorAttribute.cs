using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.Common
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true)]
	public class InterBranchRestrictorAttribute : PXRestrictorAttribute
	{
		static readonly Type EmptyWhere = typeof(Where<True, Equal<True>>);

		protected Type _interBranchWhere;

		public InterBranchRestrictorAttribute(Type where)
			: base(EmptyWhere, Messages.InterBranchFeatureIsDisabled)
		{
			_interBranchWhere = where;
		}

		protected override BqlCommand WhereAnd(PXCache sender, PXSelectorAttribute selattr, Type Where)
		{
			Type newWhere = IsReportOrInterBranchFeatureEnabled(sender) ? EmptyWhere : _interBranchWhere;

			return base.WhereAnd(sender, selattr, newWhere);
		}

		private bool IsReportOrInterBranchFeatureEnabled(PXCache sender)
		{
			return IsReportGraph(sender.Graph) || PXAccess.FeatureInstalled<FeaturesSet.interBranch>();
		}

		protected bool IsReportGraph(PXGraph graph) => graph.GetType() == typeof(PXGraph);
	}
}
