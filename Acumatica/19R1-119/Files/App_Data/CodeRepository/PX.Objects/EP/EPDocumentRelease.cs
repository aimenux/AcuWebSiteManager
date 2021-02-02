using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

using PX.Objects.Common.Exceptions;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.CT;

namespace PX.Objects.EP
{
	[TableDashboardType]
	public class EPDocumentRelease : PXGraph<EPDocumentRelease>
	{
		public PXCancel<EPExpenseClaim> Cancel;

		[PXFilterable]
		public PXProcessing<
			EPExpenseClaim,
			Where<
				EPExpenseClaim.released, Equal<False>,
				And<EPExpenseClaim.approved, Equal<True>>>>
			EPDocumentList;

		public APSetupNoMigrationMode APSetup;

		public EPDocumentRelease()
		{
			APSetup accountsPayableSetup = APSetup.Current;

			EPDocumentList.SetProcessDelegate(ReleaseDoc);
			EPDocumentList.SetProcessCaption(Messages.Release);
			EPDocumentList.SetProcessAllCaption(Messages.ReleaseAll);
			EPDocumentList.SetSelected<EPExpenseClaim.selected>();
		}

		public static void ReleaseDoc(EPExpenseClaim claim)
		{
			PXGraph.CreateInstance<EPReleaseProcess>().ReleaseDocProc(claim);
		}
	}
}