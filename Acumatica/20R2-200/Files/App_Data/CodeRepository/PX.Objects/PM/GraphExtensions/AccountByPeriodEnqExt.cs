using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System.Collections;

namespace PX.Objects.PM.GraphExtensions
{
	/// <summary>
	/// AccountByPeriodEnq extension
	/// </summary>
	public class AccountByPeriodEnqExt : PXGraphExtension<AccountByPeriodEnq>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		#region Actions/Buttons

		public PXAction<AccountByPeriodFilter> ViewPMTran;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewPMTran(PXAdapter adapter)
		{
			GLTranR tran = Base.GLTranEnq.Current;

			if (tran?.PMTranID != null)
			{
				var graph = PXGraph.CreateInstance<TransactionInquiry>();
				var filter = graph.Filter.Insert();
				filter.TranID = tran.PMTranID;

				throw new PXRedirectRequiredException(graph, true, "ViewPMTran") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}

			return Base.Filter.Select();
		}

		#endregion
	}
}