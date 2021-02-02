using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	public class SyncCCProcessingInfoEntry
	{
		public Guid NoteId { get; set; }
		public PXProcessingMessage ProcessingMessage { get; set; }

		public SyncCCProcessingInfoEntry()
		{
		}
	}
}
