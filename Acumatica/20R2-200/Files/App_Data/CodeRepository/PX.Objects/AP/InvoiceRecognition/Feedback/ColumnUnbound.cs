using System;

namespace PX.Objects.AP.InvoiceRecognition.Feedback
{
	internal class ColumnUnbound
	{
		public string DetailColumn { get; }
		public DateTime Created { get; }

		public ColumnUnbound(string detailColumn)
		{
			DetailColumn = detailColumn;
			Created = DateTime.Now;
		}
	}
}
