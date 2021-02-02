using System.Collections;
using System.Linq;

using PX.Data;

using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL;

namespace PX.Objects.DR
{
	public class DRSchedulePrimary : PXGraph<DRSchedulePrimary>
	{
		[PXFilterable]
		public PXSelect<DRSchedule> Items;
		public PXSetup<DRSetup> Setup;

		public DRSchedulePrimary()
		{
			DRSetup setup = Setup.Current;
		}

		#region CRUD Actions

		public PXAction<DRSchedule> create;
		[PXUIField(DisplayName = "")]
		[PXButton(SpecialType = PXSpecialButtonType.Insert, Tooltip = Messages.AddNewDeferralSchedule, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		[PXEntryScreenRights(typeof(DRSchedule), nameof(DraftScheduleMaint.Insert))]
		protected virtual void Create()
		{
			using (new PXPreserveScope())
			{
			DraftScheduleMaint scheduleMaintenanceGraph = PXGraph.CreateInstance<DraftScheduleMaint>();

			scheduleMaintenanceGraph.Clear(PXClearOption.ClearAll);

			scheduleMaintenanceGraph.Schedule.Insert();
			scheduleMaintenanceGraph.Schedule.Cache.IsDirty = false;

			PXRedirectHelper.TryRedirect(scheduleMaintenanceGraph, PXRedirectHelper.WindowMode.InlineWindow);
		}
		}

		public PXAction<DRSchedule> viewDoc;
		[PXUIField(DisplayName = "")]
		[PXButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				DRRedirectHelper.NavigateToOriginalDocument(this, Items.Current);
			}
			return adapter.Get();
		}

		public PXAction<DRSchedule> viewSchedule;
		[PXUIField(DisplayName = "")]
		[PXButton]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				PXRedirectHelper.TryRedirect(
					Caches[typeof(DRSchedule)],
					Items.Current,
					"ViewSchedule",
					PXRedirectHelper.WindowMode.Same);
			}
			return adapter.Get();
		}
		#endregion

		protected virtual void DRSchedule_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			var schedule = e.Row as DRSchedule;
			if (schedule == null)
				return;

			if (schedule.IsDraft == true)
			{
				schedule.Status = DRScheduleStatus.Draft;
			}
			else
			{
				using (new PXConnectionScope())
				{
					var details = PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, schedule.ScheduleID);
					schedule.Status = details.RowCast<DRScheduleDetail>().Any(d => d.IsOpen == true) ? DRScheduleStatus.Open : DRScheduleStatus.Closed;
				}
			}
		}

		protected virtual void DRSchedule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;

			if (row != null)
			{
				row.DocumentType = DRScheduleDocumentType.BuildDocumentType(row.Module, row.DocType);

				if (row.Module == BatchModule.AR)
				{
					row.BAccountType = CR.BAccountType.CustomerType;

					ARTran tran = PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<ARTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>.Select(this);

					if (tran != null)
					{
						row.OrigLineAmt = tran.TranAmt;
					}
				}
				else
				{
					row.BAccountType = CR.BAccountType.VendorType;

					APTran tran = PXSelect<APTran, Where<APTran.tranType, Equal<Current<DRSchedule.docType>>,
						And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>,
						And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>.Select(this);

					if (tran != null)
					{
						row.OrigLineAmt = tran.TranAmt;
					}
				}

				PXUIFieldAttribute.SetVisible<DRSchedule.origLineAmt>(sender, row, row.IsCustom != true);
			}
		}
	}
}
