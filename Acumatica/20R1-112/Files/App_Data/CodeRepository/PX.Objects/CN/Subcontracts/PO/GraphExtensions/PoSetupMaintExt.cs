using System.Collections.Generic;
using PX.Data;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CN.Subcontracts.PO.DAC;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PO;
using PoMessages = PX.Objects.CN.Subcontracts.PO.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PO.GraphExtensions
{
    public class PoSetupMaintExt : PXGraphExtension<POSetupMaint>
    {
        public PXFilter<PurchaseOrderTypeFilter> Filter;

        public override void Initialize()
        {
            base.Initialize();
            ApplyBaseTypeFiltering();
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<EPAssignmentMap.graphType, Equal<Current<PurchaseOrderTypeFilter.graph>>>),
            PoMessages.InvalidAssignmentMap)]
        protected virtual void POSetupApproval_AssignmentMapID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.FieldUpdating<POSetup.orderRequestApproval> args)
        {
            ChangeSetupApprovalStatuses(args.NewValue as bool?, false);
        }

        public virtual void POSetup_SubcontractRequestApproval_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            ChangeSetupApprovalStatuses(e.NewValue as bool?, true);
		}

        private void ChangeSetupApprovalStatuses(bool? isActive, bool forSubcontractsOnly)
        {
            foreach (var setupApproval in GetSetupApprovals(forSubcontractsOnly))
            {
                setupApproval.IsActive = isActive;
                Base.SetupApproval.Update(setupApproval);
            }
        }

        private IEnumerable<POSetupApproval> GetSetupApprovals(bool forSubcontractsOnly)
        {
            var query = forSubcontractsOnly
                ? (PXSelectBase<POSetupApproval>) new PXSelect<POSetupApproval,
                    Where<POSetupApproval.orderType, Equal<POOrderType.regularSubcontract>>>(Base)
                : new PXSelect<POSetupApproval,
                    Where<POSetupApproval.orderType, NotEqual<POOrderType.regularSubcontract>>>(Base);
            return query.Select().FirstTableItems;
        }

        private void ApplyBaseTypeFiltering()
        {
            if (IsSubcontractScreen())
            {
                Base.SetupApproval
                    .WhereAnd<Where<POSetupApproval.orderType, Equal<POOrderType.regularSubcontract>>>();
                Filter.Current.Graph = typeof(SubcontractEntry).FullName;
            }
            else
            {
                Base.SetupApproval
                    .WhereAnd<Where<POSetupApproval.orderType, NotEqual<POOrderType.regularSubcontract>>>();
                PXDefaultAttribute.SetPersistingCheck<PoSetupExt.subcontractNumberingID>(
                    Base.Setup.Cache, null, PXPersistingCheck.Nothing);
                Filter.Current.Graph = typeof(POOrderEntry).FullName;
            }
        }

        private bool IsSubcontractScreen()
        {
            return Base.GetType() == typeof(SubcontractSetupMaint);
        }
    }
}