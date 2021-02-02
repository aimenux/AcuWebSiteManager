using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.FA
{
	public class DeleteDocsProcess : PXGraph<DeleteDocsProcess>
	{
		public PXCancel<FARegister> Cancel;

		[PXFilterable]
		[PX.SM.PXViewDetailsButton(typeof(FARegister.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXProcessing<FARegister, Where<FARegister.released, NotEqual<True>>> Docs;

		public DeleteDocsProcess()
		{
			Docs.SetProcessCaption(Messages.DeleteProc);
			Docs.SetProcessAllCaption(Messages.DeleteAllProc);
			Docs.SetProcessDelegate(delegate(List<FARegister> list)
			{
				bool failed = false;
				TransactionEntry entryGraph = CreateInstance<TransactionEntry>();
				foreach (FARegister register in list)
				{
					PXProcessing.SetCurrentItem(register);
					try
					{
						entryGraph.Clear();
						entryGraph.Document.Current = entryGraph.Document.Search<FARegister.refNbr>(register.RefNbr);
						entryGraph.Delete.Press();
						PXProcessing.SetProcessed();
					}
					catch (Exception e)
					{
						failed = true;
						PXProcessing.SetError(e);
					}
				}
				if (failed)
				{
					throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
				}
			});
		}
	}
}