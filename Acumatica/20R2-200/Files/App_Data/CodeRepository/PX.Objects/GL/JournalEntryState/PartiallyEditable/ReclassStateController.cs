using PX.Data;

namespace PX.Objects.GL.JournalEntryState.PartiallyEditable
{
	public class ReclassStateController : PartiallyEditableStateControllerBase
	{
		public ReclassStateController(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var batch = e.Row as Batch;

			base.Batch_RowSelected(cache, e);

			PXUIFieldAttribute.SetEnabled<Batch.dateEntered>(cache, batch, true);
			PXUIFieldAttribute.SetEnabled<Batch.description>(cache, batch, true);
			PXUIFieldAttribute.SetEnabled<Batch.branchID>(cache, batch, true);

			PXUIFieldAttribute.SetVisible<GLTran.origBatchNbr>(GLTranCache, null, true);
			
			JournalEntry.editReclassBatch.SetVisible(true);
		}

		[System.Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		public static bool HasRamainingAmount(bool? hasRemainingAmount, GLTran tran)
		{
			return (hasRemainingAmount ?? false) || (tran.CuryReclassRemainingAmt ?? 0m) != 0m;
		}
		
	}
}
