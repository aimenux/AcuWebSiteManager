using PX.Data;

namespace PX.Objects.PR
{
	public class PRCurrencyAttribute : PXDBDecimalAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber
	{
		public PRCurrencyAttribute() { }
		public PRCurrencyAttribute(int precision) : base(precision) { }

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			UpdateField(sender, e.Row);
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			UpdateField(sender, e.Row);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			UpdateField(sender, e.Row);

			base.RowPersisting(sender, e);
		}

		private void UpdateField(PXCache sender, object row)
		{
			object newValue = sender.GetValue(row, _FieldName);
			sender.RaiseFieldUpdating(_FieldName, row, ref newValue);
			sender.SetValue(row, _FieldName, newValue);
		}

		public static int? GetPrecision(PXCache cache, object data, string name)
		{
			foreach (PXEventSubscriberAttribute attribute in cache.GetAttributes(data, name))
			{
				if (attribute is PRCurrencyAttribute prCurrencyAttribute)
					return prCurrencyAttribute._Precision;
			}

			return null;
		}
	}
}
