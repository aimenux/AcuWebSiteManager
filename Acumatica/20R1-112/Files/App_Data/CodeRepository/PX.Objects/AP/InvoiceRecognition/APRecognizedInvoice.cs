using PX.CloudServices.DocumentRecognition.InvoiceRecognition;
using PX.Data;
using System;

namespace PX.Objects.AP.InvoiceRecognition
{
	public class APRecognizedInvoice : APInvoice
	{
		public InvoiceRecognitionResult RecognitionResult { get; set; }

		[PXGuid]
		public Guid? FileId { get; set; }
		public abstract class fileId : Data.BQL.BqlGuid.Field<fileId> { }

		[PXString]
		[PXUIField(DisplayName = "Feedback", Visible = true)]
		public string DocumentBoundingInfoJson { get; set; }
		public abstract class documentBoundingInfoJson : Data.BQL.BqlString.Field<documentBoundingInfoJson> { }
	}
}
