using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	public class SubAccountMaint : PXGraph<SubAccountMaint>
	{
        public PXSavePerRow<Sub, Sub.subID> Save;
		public PXCancel<Sub> Cancel;
		[PXImport(typeof(Sub))]
		[PXFilterable]
		public PXSelectOrderBy<Sub, OrderBy<Asc<Sub.subCD>>> SubRecords;

		public PXSetup<Branch> Company;

		public SubAccountMaint()
		{
			viewRestrictionGroups.SetVisible(PXAccess.FeatureInstalled<FeaturesSet.rowLevelSecurity>());
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}

		#region Repository methods

		public static int? FindSubIDByCD(PXGraph graph, string subCD)
		{
			Sub sub = PXSelect<Sub,
				Where<Sub.subCD, Equal<Required<Sub.subCD>>>>.
				Select(graph, subCD);

			if (sub == null)
				return null;

			return sub.SubID;
		}

		#endregion

		public PXAction<Sub> viewRestrictionGroups;
		[PXUIField(DisplayName = Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (SubRecords.Current != null)
			{
				GLAccessBySub graph = CreateInstance<GLAccessBySub>();
				graph.Sub.Current = graph.Sub.Search<Sub.subCD>(SubRecords.Current.SubCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}
	}
}
