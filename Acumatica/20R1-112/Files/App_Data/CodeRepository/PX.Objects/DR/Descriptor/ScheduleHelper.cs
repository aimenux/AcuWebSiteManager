using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.DR.Descriptor;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.DR
{
	public static class ScheduleHelper
	{
		/// <summary>
		/// Checks if deferral code has been changed or removed from the line.
		/// If so, ensures the removal of any associated deferral schedules.
		/// </summary>
		public static void DeleteAssociatedScheduleIfDeferralCodeChanged(
			PXGraph graph,
			IDocumentLine documentLine)
		{
			IDocumentLine oldLine;
 
 			// Obtain the document line last saved into the database
 			// to check if the new line's deferral code differs from it.
 			// -
			switch (documentLine.Module)
			{
				case GL.BatchModule.AR:
					oldLine = GetOriginal<ARTran>(graph.Caches[typeof(ARTran)], documentLine as ARTran);
					break;
				case GL.BatchModule.AP:
					oldLine = GetOriginal<APTran>(graph.Caches[typeof(APTran)], documentLine as APTran);
					break;
				default:
				throw new PXException(Messages.UnexpectedDocumentLineModule);
			}

			DeleteAssociatedScheduleIfDeferralCodeChanged(graph, documentLine, oldLine);
		}


		/// <summary>
		/// Checks if deferral code has been changed or removed from the line.
		/// If so, ensures the removal of any associated deferral schedules.
		/// </summary>
		public static void DeleteAssociatedScheduleIfDeferralCodeChanged(
			PXCache cache,
			ARTran documentLine)
		{
			// Obtain the document line last saved into the database
			// to check if the new line's deferral code differs from it.
			// -
			ARTran oldLine = GetOriginal<ARTran>(cache, documentLine);

			DeleteAssociatedScheduleIfDeferralCodeChanged(cache.Graph, documentLine, oldLine);
		}

		/// <summary>
		/// Checks if deferral code has been changed or removed from the line.
		/// If so, ensures the removal of any associated deferral schedules.
		/// </summary>
		public static void DeleteAssociatedScheduleIfDeferralCodeChanged(
			PXCache cache,
			APTran documentLine)
		{
			// Obtain the document line last saved into the database
			// to check if the new line's deferral code differs from it.
			// -
			APTran oldLine = GetOriginal<APTran>(cache, documentLine);

			DeleteAssociatedScheduleIfDeferralCodeChanged(cache.Graph, documentLine, oldLine);
		}

		private static T GetOriginal<T>(PXCache cache, object row)
			where T : class, IBqlTable, new()
		{
			if (cache.GetStatus(row) == PXEntryStatus.Inserted || cache.GetStatus(row) == PXEntryStatus.InsertedDeleted)
			{
				return null;
			}

			T original = new T();
			cache.Keys.ForEach(s => cache.SetValue(original, s, cache.GetValueOriginal(row, s)));

			if (cache.IsKeysFilled(original))
			{
				cache.Fields.Except(cache.Keys).ForEach(s => cache.SetValue(original, s, cache.GetValueOriginal(row, s)));
				return original;
			}
			return null;
		}

		private static void DeleteAssociatedScheduleIfDeferralCodeChanged(
			PXGraph graph,
			IDocumentLine documentLine,
			IDocumentLine oldLine)
		{
			bool deferralCodeRemoved = documentLine.DeferredCode == null;

			bool deferralCodeChanged =
				oldLine?.DeferredCode != null &&
				documentLine.DeferredCode != null &&
				oldLine.DeferredCode != documentLine.DeferredCode;

			if (deferralCodeRemoved || deferralCodeChanged)
			{
				DRSchedule correspondingSchedule = PXSelect<
					DRSchedule,
					Where<DRSchedule.module, Equal<Required<DRSchedule.module>>,
						And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
						And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
						And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>>
					.Select(
						graph,
						documentLine.Module,
						documentLine.TranType,
						documentLine.RefNbr,
						documentLine.LineNbr);

				if (correspondingSchedule == null) return;

				DraftScheduleMaint scheduleGraph = PXGraph.CreateInstance<DraftScheduleMaint>();
				scheduleGraph.Schedule.Delete(correspondingSchedule);
				scheduleGraph.Save.Press();
			}
		}
	}
}
