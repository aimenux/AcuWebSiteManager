using PX.CloudServices.DAC;
using PX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AP.InvoiceRecognition.DAC
{
	[PXInternalUseOnly]
	public class APRecognizedInvoiceRecognitionStatusListAttribute : RecognizedRecordStatusListAttribute
	{
		public const string PendingFile = "F";
		private const string PendingFileLabel = "New";

		public APRecognizedInvoiceRecognitionStatusListAttribute()
			: base(new[] { PendingFile }, new[] { PendingFileLabel })
		{
		}
	}
}
