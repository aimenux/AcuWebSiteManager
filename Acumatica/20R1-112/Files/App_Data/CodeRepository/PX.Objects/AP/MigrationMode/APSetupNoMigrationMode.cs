using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.AP.MigrationMode
{
	/// <summary>
	/// An <see cref="PXSetup{APSetup}"/> descendant. In addition to checking the
	/// presence of the setup record, also checks that <see cref="APSetup.MigrationMode">
	/// migration mode</see> is not enabled in the module.
	/// </summary>
	public class APSetupNoMigrationMode : PXSetup<APSetup>
	{
		private PXGraph Graph
		{
			get;
			set;
		}

		public APSetupNoMigrationMode(PXGraph graph) : base(graph)
		{
			Graph = graph;
		}

		public override APSetup Current
		{
			get
			{
				APSetup setup = base.Current;

				EnsureMigrationModeDisabled(Graph, setup);

				return setup;
			}
			set
			{
				base.Current = value;
			}
		}

		public static void EnsureMigrationModeDisabled(PXGraph graph, APSetup setup = null)
		{
			setup = setup ?? PXSelectReadonly<APSetup>.Select(graph);

			if (setup != null)
				setup.MigrationMode = CurrentConfiguration.ActualAPSetup.isMigrationModeEnabled;

			if (setup?.MigrationMode != true) return;

			throw new PXSetupNotEnteredException(
				Messages.MigrationModeIsActivated, 
				typeof(APSetup), 
				PXMessages.LocalizeNoPrefix(Messages.APSetup));
		}
	}
}
