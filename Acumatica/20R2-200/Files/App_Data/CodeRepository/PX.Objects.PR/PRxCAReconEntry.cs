using System.Collections;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public class PRxCAReconEntry : PXGraphExtension<CAReconEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		public delegate IEnumerable ViewDocDelegate(PXAdapter adapter);

		[PXOverride]
		public virtual IEnumerable ViewDoc(PXAdapter adapter, ViewDocDelegate baseMethod)
		{
			CAReconEntry.CATranExt caTransaction = Base.CAReconTranRecords.Current;
			if (caTransaction == null || caTransaction.OrigModule != BatchModule.PR)
				return baseMethod(adapter);

			PRPayment payment =
				SelectFrom<PRPayment>
					.Where<PRPayment.refNbr.IsEqual<P.AsString>.And<PRPayment.docType.IsEqual<P.AsString>>>.View
					.SelectSingleBound(Base, null, caTransaction.OrigRefNbr, caTransaction.OrigTranType);

			if (payment != null)
			{
				PRPayChecksAndAdjustments graph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
				graph.Document.Current = payment;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}
	}
}
