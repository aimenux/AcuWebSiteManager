using PX.Common;
using PX.Data;
using PX.Objects.AP.InvoiceRecognition.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AP.InvoiceRecognition.DAC
{
	[PXInternalUseOnly]
	[PXHidden]
	public class FeedbackParameters : IBqlTable
	{
		internal FeedbackBuilder FeedbackBuilder { get; set; }

		internal Dictionary<string, Uri> Links { get; set; }

		internal virtual Guid? FeedbackFileID { get; set; }
	}
}
