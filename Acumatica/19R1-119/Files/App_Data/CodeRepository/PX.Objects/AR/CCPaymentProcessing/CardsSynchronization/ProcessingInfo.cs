using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	internal class ProcessingInfo
	{
		public const string processingKey = "PXCCProcessingState";
		public static void AppendProcessingInfo(Guid noteId, string info)
		{
			List<SyncCCProcessingInfoEntry> list = PXLongOperation.GetCustomInfoForCurrentThread(processingKey) as List<SyncCCProcessingInfoEntry>;

			if (list != null)
			{
				PXProcessingMessage msg = new PXProcessingMessage() { ErrorLevel = PXErrorLevel.RowInfo, Message = info };
				list.Add(new SyncCCProcessingInfoEntry() { NoteId = noteId, ProcessingMessage = msg });
			}
		}

		public static void AppendProcessingError(Guid noteId, string error)
		{
			List<SyncCCProcessingInfoEntry> list = PXLongOperation.GetCustomInfoForCurrentThread(processingKey) as List<SyncCCProcessingInfoEntry>;

			if (list != null)
			{
				PXProcessingMessage msg = new PXProcessingMessage() { ErrorLevel = PXErrorLevel.RowError, Message = error };
				list.Add(new SyncCCProcessingInfoEntry() { NoteId = noteId, ProcessingMessage = msg });
			}
		}

		public static void ClearProcessingRows()
		{
			List<SyncCCProcessingInfoEntry> list = PXLongOperation.GetCustomInfoForCurrentThread(processingKey) as List<SyncCCProcessingInfoEntry>;
			list.Clear();
		}
	}
}
