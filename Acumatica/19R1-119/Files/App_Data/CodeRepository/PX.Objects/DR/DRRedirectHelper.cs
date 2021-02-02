using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL;

namespace PX.Objects.DR
{
	public static class DRRedirectHelper
	{
		/// <summary>
		/// Tries to perform a redirect to a deferral schedule by its ID.
		/// Does nothing if the provided ID value is <c>null</c>.
		/// </summary>
		/// <param name="sourceGraph">
		/// A graph through which the redirect will be processed.
		/// </param>
		/// <param name="scheduleID">
		/// The unique identifier of a <see cref="DRSchedule"/> record which 
		/// the user should be redirected to.</param>
		public static void NavigateToDeferralSchedule(PXGraph sourceGraph, int? scheduleID)
		{
			DRSchedule deferralSchedule = PXSelect<
				DRSchedule,
				Where<
					DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>
				.Select(sourceGraph, scheduleID);

			if (deferralSchedule != null)
			{
				PXRedirectHelper.TryRedirect(
					sourceGraph.Caches[typeof(DRSchedule)],
					deferralSchedule,
					"ViewDocument",
					PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		/// <summary>
		/// Tries to perform a redirect to the original AR or AP document of a given
		/// <see cref="DRSchedule"/> record.
		/// </summary>
		/// <param name="sourceGraph">
		/// A graph through which the redirect will be processed.
		/// </param>
		/// <param name="scheduleDetail">
		/// The <see cref="DRSchedule"/> record containing the document type and 
		/// document reference number necessary for the redirect.
		/// </param>
		public static void NavigateToOriginalDocument(
			PXGraph sourceGraph, 
			DRSchedule schedule)
		{
			IBqlTable originalDocument = null;

			if (schedule.Module == BatchModule.AR)
			{
				originalDocument = (ARRegister)
				PXSelect<
					ARRegister, 
					Where<	ARRegister.docType, Equal<Required<DRSchedule.docType>>, 
							And<ARRegister.refNbr, Equal<Required<DRSchedule.refNbr>>>>>
					.Select(sourceGraph, schedule.DocType, schedule.RefNbr);
			}
			else if (schedule.Module == BatchModule.AP)
			{
				originalDocument = (APRegister)
				PXSelect<
					APRegister, 
					Where<	APRegister.docType, Equal<Required<DRSchedule.docType>>, 
							And<APRegister.refNbr, Equal<Required<DRSchedule.refNbr>>>>>
					.Select(sourceGraph, schedule.DocType, schedule.RefNbr);
			}

			if (originalDocument != null)
			{
				PXRedirectHelper.TryRedirect(
					sourceGraph.Caches[originalDocument.GetType()], 
					originalDocument, 
					"ViewDocument",
					PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		/// <summary>
		/// Tries to perform a redirect to the original AR or AP document of a given
		/// <see cref="DRScheduleDetail"/>.
		/// </summary>
		/// <param name="sourceGraph">
		/// A graph through which the redirect will be processed.
		/// </param>
		/// <param name="scheduleDetail">
		/// The <see cref="DRScheduleDetail"/> object containing the document type and 
		/// document reference number necessary for the redirect.
		/// </param>
		public static void NavigateToOriginalDocument(
			PXGraph sourceGraph, 
			DRScheduleDetail scheduleDetail)
		{
			DRSchedule correspondingSchedule = PXSelect<
				DRSchedule,
				Where<DRSchedule.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>
				.Select(sourceGraph, scheduleDetail.ScheduleID);

			NavigateToOriginalDocument(sourceGraph, correspondingSchedule);
		}

		/// <summary>
		/// Tries to perform a redirect to the original AR or AP document referenced by
		/// the <see cref="DRSchedule"/> that owns the given <see cref="DRScheduleTran"/>.
		/// </summary>
		/// <param name="sourceGraph">
		/// A graph through which the redirect will be processed.
		/// </param>
		/// <param name="scheduleTransaction">
		/// The <see cref="DRScheduleDetail"/> object from which the corresponding schedule
		/// will be selected.
		/// </param>
		public static void NavigateToOriginalDocument(
			PXGraph sourceGraph,
			DRScheduleTran scheduleTransaction)
		{
			DRSchedule correspondingSchedule = PXSelect<
				DRSchedule,
				Where<DRSchedule.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>>>
				.Select(sourceGraph, scheduleTransaction.ScheduleID);

			NavigateToOriginalDocument(sourceGraph, correspondingSchedule);
		}
	}
}
