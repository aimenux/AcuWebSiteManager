using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class DocTypeAndRefNbrDisplayNameAttribute : PXStringAttribute, IPXFieldDefaultingSubscriber
	{
		private readonly Type _DocTypeField;
		private readonly Type _RefNbrField;

		public DocTypeAndRefNbrDisplayNameAttribute(Type docTypeField, Type refNbrField) : base(50)
		{
			_DocTypeField = docTypeField;
			_RefNbrField = refNbrField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.RowPersisted.AddHandler(sender.GetItemType(),
				(cache, e) =>
				{
					if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
					{
						cache.SetValue(e.Row, _FieldName, GetDocAndRef(cache, e.Row));
					}
				});
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (e.Row == null)
				return;

			e.ReturnValue = GetDocAndRef(sender, e.Row);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null)
				return;

			e.NewValue = GetDocAndRef(sender, e.Row);
		}

		private string GetDocAndRef(PXCache sender, object row)
		{
			string docType = (string)sender.GetValue(row, _DocTypeField.Name);
			string refNbr = (string)sender.GetValue(row, _RefNbrField.Name);

			if (string.IsNullOrWhiteSpace(docType) || string.IsNullOrWhiteSpace(refNbr))
				return string.Empty;

			return $"{docType},{refNbr}";
		}
	}
}
