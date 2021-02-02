using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.AP.GraphExtensions
{
    public class ApSetupMaintExt : PXGraphExtension<APSetupMaint>
    {
        public PXSelect<POOrder,
            Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>>> Subcontracts;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public virtual void _(Events.RowPersisting<APSetup> args)
        {
            if (args.Row?.RequireSingleProjectPerDocument == true)
            {
                UpdateSubcontractProjects();
            }
        }

        private void UpdateSubcontractProjects()
        {
            var subcontracts = Subcontracts.Select().FirstTableItems;
            foreach (var subcontract in subcontracts)
            {
                UpdateSubcontractIfRequired(subcontract);
            }
            Subcontracts.Cache.Persist(PXDBOperation.Update);
        }

        private void UpdateSubcontractIfRequired(POOrder subcontract)
        {
            var subcontractLines = GetSubcontractLines(subcontract);
            if (IsProjectUpdateRequired(subcontract, subcontractLines))
            {
                UpdateSubcontractProject(subcontract, subcontractLines);
            }
        }

        private void UpdateSubcontractProject(POOrder subcontract, IEnumerable<POLine> subcontractLines)
        {
            subcontract.ProjectID = subcontractLines.First().ProjectID;
            Subcontracts.Cache.Update(subcontract);
        }

        private static bool IsProjectUpdateRequired(POOrder subcontract, IReadOnlyCollection<POLine> subcontractLines)
        {
            var nonZeroOpenAmount = subcontract.LineTotal > 0;
            var nonZeroNumberOfLines = subcontractLines.Count > 0;
            var allLinesWithTheSameProject = subcontractLines.GroupBy(x => x.ProjectID).Count() == 1;
            return nonZeroOpenAmount && nonZeroNumberOfLines && allLinesWithTheSameProject;
        }

        private List<POLine> GetSubcontractLines(POOrder subcontract)
        {
            var query = new PXSelect<POLine,
                Where<POLine.orderType, Equal<POOrderType.regularSubcontract>,
                    And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>>>>(Base);
            return query.Select(subcontract.OrderNbr).FirstTableItems.ToList();
        }
    }
}