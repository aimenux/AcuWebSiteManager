using PX.Data;

namespace PX.Objects.GL.JournalEntryState.PartiallyEditable
{
	public class TrialBalanceStateController : PartiallyEditableStateControllerBase
	{
		public TrialBalanceStateController(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var batch = e.Row as Batch;

			base.Batch_RowSelected(cache, e);

			PXUIFieldAttribute.SetEnabled<Batch.description>(cache, batch, true);
		}

		public override void GLTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e, Batch batch)
		{
			var tran = e.Row as GLTran;

			base.GLTran_RowSelected(sender, e, batch);

			PXUIFieldAttribute.SetEnabled<GLTran.tranDesc>(sender, tran, true);
		}
	}
}
