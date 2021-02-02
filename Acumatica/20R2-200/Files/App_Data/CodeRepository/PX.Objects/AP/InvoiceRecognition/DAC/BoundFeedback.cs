using PX.Common;
using PX.Data;
using PX.Data.BQL;
using static PX.Api.SYImportCondition;

namespace PX.Objects.AP.InvoiceRecognition.DAC
{
	[PXInternalUseOnly]
	[PXHidden]
	public class BoundFeedback : IBqlTable
	{
		[PXString]
		[PXUIField(DisplayName = "Field Bound", Visible = false)]
		public virtual string FieldBound { get; set; }
		public abstract class fieldBound : BqlString.Field<value> { }

		[PXString]
		[PXUIField(DisplayName = "Table Related", Visible = false)]
		public virtual string TableRelated { get; set; }
		public abstract class tableRelated : BqlString.Field<tableRelated> { }
	}
}
