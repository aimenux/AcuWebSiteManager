using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.GL
{
	public class GLVoucherEntryHelper
	{
		public static void ReverseGLVoucher(PXGraph graph, PXAction actionSave, Action baseReverse, PXCache voucherBatchView)
		{
			GLWorkBook wb = PXSelect<GLWorkBook, Where<GLWorkBook.workBookID, Equal<Required<GLVoucherBatch.workBookID>>>>.Select(graph, graph.GetContextValue<GLVoucherBatch.workBookID>());

			if (wb?.ReversingWorkBookID == null)
			{
				throw new PXException(Messages.ReversingWorkbookIDisnotDefined);
			}
			GLWorkBook reversingWB = PXSelect<GLWorkBook,
					Where<GLWorkBook.workBookID, Equal<Required<GLVoucherBatch.workBookID>>,
					And<GLWorkBook.status, Equal<WorkBookStatus.active>>>>.Select(graph, wb.ReversingWorkBookID);
			if (reversingWB == null)
			{
				throw new PXException(Messages.ReversingWorkbookIDisnotFound);
			}

			GLVoucherBatch vb = PXSelect<GLVoucherBatch, Where<GLVoucherBatch.workBookID, Equal<Required<GLWorkBook.workBookID>>, And<GLVoucherBatch.released, Equal<False>>>,
									OrderBy<Asc<GLVoucherBatch.voucherBatchNbr>>>.Select(graph, reversingWB.WorkBookID);

			if (vb == null)
			{
				var batch = new GLVoucherBatch
				{
					WorkBookID = reversingWB.WorkBookID,
					Descr = Messages.ReversingVoucherBatchDesc
				};

				vb = (GLVoucherBatch)voucherBatchView.Insert(batch);
				voucherBatchView.Current = batch;
				actionSave.Press();
			}

			graph.contextValues[typeof(GLVoucherBatch.workBookID).FullName] = reversingWB.WorkBookID;
			graph.contextValues[typeof(GLVoucherBatch.voucherBatchNbr).FullName] = vb.VoucherBatchNbr;

			baseReverse.Invoke();
			throw new PXRedirectWithinContextException(graph, graph, Messages.VoucherEdit, typeof(GLVoucherBatch.workBookID), typeof(GLVoucherBatch.voucherBatchNbr));
		}
	}
}
