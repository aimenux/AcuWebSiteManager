using PX.Data;
using PX.Data.WorkflowAPI;

namespace PX.Objects.CR.Workflows
{
	internal static class WorkflowExtensions
	{
		public static BoundedTo<TGraph, TPrimary>.FieldState.IAllowOptionalConfig IsDisabled<TGraph, TPrimary>(
			this BoundedTo<TGraph, TPrimary>.FieldState.IAllowOptionalConfig config,
			bool disabled)
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
		{
			return disabled ? config.IsDisabled() : config;
		}

		public static BoundedTo<TGraph, TPrimary>.ActionState.IAllowOptionalConfig IsDuplicatedInToolbar<TGraph, TPrimary>(
			this BoundedTo<TGraph, TPrimary>.ActionState.IAllowOptionalConfig config,
			bool duplicated)
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
		{
			return duplicated ? config.IsDuplicatedInToolbar() : config;
		}
	}
}