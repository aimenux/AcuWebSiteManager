using PX.Data;

namespace PX.Objects.GL.JournalEntryState
{
	public class CommonTypeStateController : EditableStateControllerBase
	{
		public CommonTypeStateController(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (JournalEntry.UnattendedMode)
				return;

			var batch = e.Row as Batch;

			base.Batch_RowSelected(cache, e);

			PXUIFieldAttribute.SetEnabled(cache, batch, true);
			PXUIFieldAttribute.SetEnabled<Batch.status>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.curyCreditTotal>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.curyDebitTotal>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.origBatchNbr>(cache, batch, false);
			PXUIFieldAttribute.SetEnabled<Batch.autoReverse>(cache, batch, (batch.AutoReverseCopy != true));
			PXUIFieldAttribute.SetEnabled<Batch.autoReverseCopy>(cache, batch, false);

			GLTranCache.AllowInsert = true;
			GLTranCache.AllowDelete = true;

			bool isAutoReverse = batch.AutoReverseCopy == true || batch.AutoReverse == true;
			bool createTaxTranEnabled = IsTaxTranCreationAllowed(batch) && isAutoReverse != true;

			PXUIFieldAttribute.SetEnabled<Batch.createTaxTrans>(cache, batch, createTaxTranEnabled);
		}
	}
}
