using PX.Data;

namespace PX.Objects.Common.Discount
{
	public static class DiscountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Line, AR.Messages.Line),
					Pair(Group, AR.Messages.Group),
					Pair(Document, AR.Messages.Document),
					Pair(ExternalDocument, AR.Messages.ExternalDocument)
				}) { }
		}

		public const string Line = "L";
		public const string Group = "G";
		public const string Document = "D";
		public const string ExternalDocument = "B";
		public const string Flat = "F";

		public class LineDiscount : PX.Data.BQL.BqlString.Constant<LineDiscount> { public LineDiscount() : base(Line) { } }
		public class GroupDiscount : PX.Data.BQL.BqlString.Constant<GroupDiscount> { public GroupDiscount() : base(Group) { } }
		public class DocumentDiscount : PX.Data.BQL.BqlString.Constant<DocumentDiscount> { public DocumentDiscount() : base(Document) { } }
		public class ExternalDocumentDiscount : PX.Data.BQL.BqlString.Constant<ExternalDocumentDiscount> { public ExternalDocumentDiscount() : base(ExternalDocument) { } }
	}
}