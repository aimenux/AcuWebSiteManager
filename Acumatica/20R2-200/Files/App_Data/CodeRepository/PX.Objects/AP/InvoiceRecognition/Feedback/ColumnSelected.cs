using System.Collections.Generic;

namespace PX.Objects.AP.InvoiceRecognition.Feedback
{
	internal class ColumnSelected : ColumnUnbound
	{
		public List<short> Columns { get; }

		public ColumnSelected(string detailColumn, List<short> columns)
			: base(detailColumn)
		{
			Columns = columns;
		}
	}
}
