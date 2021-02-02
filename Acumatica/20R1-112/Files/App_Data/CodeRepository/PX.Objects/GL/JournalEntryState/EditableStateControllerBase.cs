using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.GL.JournalEntryState
{
	public class EditableStateControllerBase : StateControllerBase
	{
		protected EditableStateControllerBase(JournalEntry journalEntry) : base(journalEntry)
		{
		}

		public override void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var batch = e.Row as Batch;

			base.Batch_RowSelected(cache, e);

			PXUIFieldAttribute.SetEnabled<Batch.hold>(cache, batch, (batch.Scheduled != true));

			cache.AllowDelete = true;
			cache.AllowUpdate = true;

			GLTranCache.AllowUpdate = true;
		}
	}
}
