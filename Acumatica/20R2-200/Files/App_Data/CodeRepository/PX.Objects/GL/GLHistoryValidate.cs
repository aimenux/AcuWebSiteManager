using System;

using PX.Data;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL
{
	[TableAndChartDashboardType]
	public class GLHistoryValidate : PXGraph<GLHistoryValidate>
	{
		#region Internal Types Definition
		[Serializable]
		public class GLIntegrityCheckFilter : IBqlTable
		{
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

			[FinPeriodNonLockedSelector()]			
			[PXUIField(DisplayName = Messages.FinPeriod)]
			public virtual string FinPeriodID
			{
				get;
				set;
			}
		}
		#endregion

		public GLHistoryValidate()
		{
			GLSetup setup = glsetup.Current;
			GLIntegrityCheckFilter filter = Filter.Current;

			LedgerList.SetProcessCaption(Messages.ProcValidate);
			LedgerList.SetProcessAllCaption(Messages.ProcValidateAll);
			LedgerList.SuppressMerge = true;
			LedgerList.SuppressUpdate = true;
			LedgerList.SetProcessDelegate((PostGraph postGraph, Ledger ledger) => Validate(postGraph, ledger, filter));

			PXUIFieldAttribute.SetEnabled<Ledger.selected>(LedgerList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<Ledger.ledgerCD>(LedgerList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<Ledger.descr>(LedgerList.Cache, null, false);
		}

		public PXFilter<GLIntegrityCheckFilter> Filter;
		public PXCancel<GLIntegrityCheckFilter> Cancel;

		[PXFilterable]
		public PXFilteredProcessing<
			Ledger, 
			GLIntegrityCheckFilter,
			Where<
				Ledger.balanceType, Equal<LedgerBalanceType.actual>, 
				Or<Ledger.balanceType,Equal<LedgerBalanceType.report>>>> 
			LedgerList;

		public PXSetup<GLSetup> glsetup;

		protected virtual void GLIntegrityCheckFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool errorsOnForm = PXUIFieldAttribute.GetErrors(sender,null, PXErrorLevel.Error, PXErrorLevel.RowError).Count > 0;
			LedgerList.SetProcessEnabled(!errorsOnForm);
			LedgerList.SetProcessAllEnabled(!errorsOnForm);
		}

		private static void Validate(PostGraph graph, Ledger ledger, GLIntegrityCheckFilter filter)
		{
			if (string.IsNullOrEmpty(filter.FinPeriodID))
			{
				throw new PXException(Messages.Prefix + ": " + Messages.ProcessingRequireFinPeriodID);
			}

			while (RunningFlagScope<PostGraph>.IsRunning)
			{
				System.Threading.Thread.Sleep(10);
			}

            using (new RunningFlagScope<GLHistoryValidate>())
            {
                graph.Clear();
                graph.IntegrityCheckProc(ledger, filter.FinPeriodID);
                graph = PXGraph.CreateInstance<PostGraph>();
                graph.PostBatchesRequiredPosting();
            }
		}		
	}
}
