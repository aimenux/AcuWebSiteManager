using PX.Data;
using PX.Objects.CN.Subcontracts.PO.DAC;
using PX.Objects.CN.Subcontracts.PO.Descriptor.Attributes;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.GraphExtensions
{
    public class PoOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public PXFilter<PurchaseOrderTypeFilter> TypeFilter;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public override void Initialize()
        {
            base.Initialize();
            ApplyBaseTypeFiltering();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PurchaseOrderTypeRestrictor]
        protected virtual void POOrder_OrderNbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PurchaseOrderTypeRestrictor]
        protected virtual void POLine_PONbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PurchaseOrderTypeRestrictor]
        protected virtual void SOLineSplit_PONbr_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type1> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.Blanket;
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type2> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.DropShip;
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type3> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.RegularOrder;
        }

        protected virtual void _(Events.FieldDefaulting<PurchaseOrderTypeFilter.type4> args)
        {
            args.NewValue = IsSubcontractScreen()
                ? POOrderType.RegularSubcontract
                : POOrderType.StandardBlanket;
        }

        private void ApplyBaseTypeFiltering()
        {
            if (IsSubcontractScreen())
            {
                AddSubcontractFilters();
            }
            else
            {
                AddPurchaseOrderFilters();
            }
        }

        private void AddPurchaseOrderFilters()
        {
            Base.Document.WhereAnd<Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>();
            Base.SetupApproval.WhereAnd<Where<POSetupApproval.orderType, NotEqual<POOrderType.regularSubcontract>>>();
        }

        private void AddSubcontractFilters()
        {
            Base.Document.WhereAnd<Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>>>();
            Base.SetupApproval.WhereAnd<Where<POSetupApproval.orderType, Equal<POOrderType.regularSubcontract>>>();
        }

        private bool IsSubcontractScreen()
        {
            // In case multiple extensions are available Acumatica create dynamic BaseClass
            // (for example "Cst_SubcontractEntry" instead of "SubcontractEntry") with BaseType equal current class.
            return Base.GetType() == typeof(SubcontractEntry)
                || Base.GetType().BaseType == typeof(SubcontractEntry);
        }
    }
}