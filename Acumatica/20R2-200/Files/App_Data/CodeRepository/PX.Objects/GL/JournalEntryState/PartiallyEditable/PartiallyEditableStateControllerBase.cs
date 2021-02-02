using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.GL.JournalEntryState.PartiallyEditable
{
	public abstract class PartiallyEditableStateControllerBase : EditableStateControllerBase
	{
		protected PartiallyEditableStateControllerBase(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var batch = e.Row as Batch;

			PXUIFieldAttribute.SetEnabled(cache, batch, false);
			GLTranCache.AllowInsert = false;
			GLTranCache.AllowDelete = false;

			base.Batch_RowSelected(cache, e);
		}

		public override void GLTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e, Batch batch)
		{
			var tran = e.Row as GLTran;

			base.GLTran_RowSelected(sender, e, batch);

			PXUIFieldAttribute.SetReadOnly(sender, tran, true);
		}
	}
}
