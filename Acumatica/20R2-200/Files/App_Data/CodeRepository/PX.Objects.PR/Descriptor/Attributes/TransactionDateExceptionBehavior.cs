using PX.Data;

namespace PX.Objects.PR
{
	public class TransactionDateExceptionBehavior
	{
		public class ListAttribute : PXStringListAttribute, IPXFieldDefaultingSubscriber
		{
			public ListAttribute()
				: base(
				new string[] { Before, After },
				new string[] { Messages.TransactionDayBefore, Messages.TransactionDayAfter })
			{ }

			public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
				e.NewValue = Before;
			}
		}

		public const string Before = "BEF";
		public const string After = "AFT";
	}
}
