using PX.Data;

namespace PX.Objects.GL.JournalEntryState
{
	public class ReadonlyStateController : StateControllerBase
	{
		public ReadonlyStateController(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var batch = e.Row as Batch;

			base.Batch_RowSelected(cache, e);

			PXUIFieldAttribute.SetEnabled(cache, batch, false);

			GLTranCache.AllowDelete = false;
			GLTranCache.AllowInsert = false;
			GLTranCache.AllowUpdate = false;

			cache.AllowDelete = false;
			cache.AllowUpdate = false;
		}

		public override void GLTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e, Batch batch)
		{
			var tran = e.Row as GLTran;

			base.GLTran_RowSelected(sender, e, batch);

			PXUIFieldAttribute.SetReadOnly(sender, tran, true);
		}
	}
}
