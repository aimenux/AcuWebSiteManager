using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.Standalone;
using PX.Objects.CS;
using System.Collections;

namespace PX.Objects.DR
{
	public class ARCashSaleEntryASC606 : PXGraphExtension<ARCashSaleEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		public PXAction<ARCashSale> viewSchedule;
		[PXUIField(DisplayName = "View Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			ARTran currentLine = Base.Transactions.Current;

			if (currentLine != null &&
				Base.Transactions.Cache.GetStatus(currentLine) == PXEntryStatus.Notchanged)
			{
				ValidateTransactions();
				Base.Save.Press();
				ViewScheduleForDocument(Base, Base.Document.Current);
			}

			return adapter.Get();
		}

		protected void ValidateTransactions()
		{
			foreach (ARTran tran in Base.Transactions.Select())
			{
				object defCode = tran.DeferredCode;

				try
				{
					Base.Transactions.Cache.RaiseFieldVerifying<ARTran.deferredCode>(tran, ref defCode);
				}
				catch (PXSetPropertyException e)
				{
					Base.Transactions.Cache.RaiseExceptionHandling<ARTran.deferredCode>(tran, defCode, e);
					throw e;
				}
			}
		}

		public static void ViewScheduleForDocument(PXGraph graph, ARCashSale document)
		{
			PXSelectBase<DRSchedule> correspondingScheduleView = new PXSelect<
				DRSchedule,
				Where<
				DRSchedule.module, Equal<GL.BatchModule.moduleAR>,
					And<DRSchedule.docType, Equal<Required<ARTran.tranType>>,
					And<DRSchedule.refNbr, Equal<Required<ARTran.refNbr>>>>>>
				(graph);

			DRSchedule correspondingSchedule = correspondingScheduleView.Select(document.DocType, document.RefNbr);

			if (correspondingSchedule?.LineNbr != null && document.Released == false)
			{
				throw new PXException(Messages.CantCompleteBecauseASC606FeatureIsEnabled);
			}

			if (correspondingSchedule?.IsOverridden != true && document.Released == false)
			{
				var netLinesAmount = ASC606Helper.CalculateNetAmount(graph, document);
				int? defScheduleID = null;

				if (netLinesAmount.Cury != 0m)
				{
					DRSingleProcess process = PXGraph.CreateInstance<DRSingleProcess>();

					process.CreateSingleSchedule(document, netLinesAmount, defScheduleID, true);
					process.Actions.PressSave();

					correspondingScheduleView.Cache.Clear();
					correspondingScheduleView.Cache.ClearQueryCacheObsolete();

					correspondingSchedule = correspondingScheduleView.Select(document.DocType, document.RefNbr);
				}
			}

			if (correspondingSchedule != null)
			{
				PXRedirectHelper.TryRedirect(
					graph.Caches[typeof(DRSchedule)],
					correspondingSchedule,
					"View Schedule",
					PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		protected virtual void _(Events.FieldVerifying<ARTran, ARTran.deferredCode> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.InventoryID == null && e.NewValue != null)
			{
				throw new PXSetPropertyException(AR.Messages.InventoryIDCouldNotBeEmpty);
			}
		}
	}
}
