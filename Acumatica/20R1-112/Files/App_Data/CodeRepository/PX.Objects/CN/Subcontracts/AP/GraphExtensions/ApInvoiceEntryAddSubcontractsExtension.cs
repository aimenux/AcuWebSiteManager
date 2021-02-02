using System;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Subcontracts.AP.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PO;
using PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel;
using ApMessages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages;
using PoMessages = PX.Objects.PO.Messages;

namespace PX.Objects.CN.Subcontracts.AP.GraphExtensions
{
    public class ApInvoiceEntryAddSubcontractsExtension : PXGraphExtension<AddPOOrderLineExtension, AddPOOrderExtension,
        APInvoiceEntry>
    {
        [PXCopyPasteHiddenView]
        [PXViewName(ApMessages.SubcontractViewName)]
        public PXSelect<POOrderRS> Subcontracts;

        [PXCopyPasteHiddenView]
        public PXSelect<POLineRS> SubcontractLines;

        public PXAction<APInvoice> AddSubcontracts;
        public PXAction<APInvoice> AddSubcontract;
        public PXAction<APInvoice> AddSubcontractLines;
        public PXAction<APInvoice> AddSubcontractLine;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
                !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        public IEnumerable poOrdersList()
        {
            return Base1.pOOrderslist().Cast<POOrderRS>()
               .Where(po => po.OrderType != POOrderType.RegularSubcontract);
        }

        public IEnumerable poOrderLinesList()
        {
            return Base2.pOOrderLinesList().Cast<POLineRS>()
                 .Where(line => line.OrderType != POOrderType.RegularSubcontract);
        }

        public IEnumerable subcontracts()
        {
            var subcontracts = Base1.pOOrderslist().Cast<POOrderRS>()
                .Where(x => x.OrderType == POOrderType.RegularSubcontract);
            foreach (var subcontract in subcontracts)
            {
                var extension = PXCache<POOrderRS>.GetExtension<PoOrderRsExt>(subcontract);
                extension.ProjectCD = GetSubcontractProjectName(subcontract);
                yield return subcontract;
            }
        }

        public IEnumerable subcontractLines()
        {
            var extension = PXCache<POOrderFilter>.GetExtension<PoOrderFilterExt>(Base2.orderfilter.Current);
            return Base2.pOOrderLinesList().Cast<POLineRS>()
                .Where(line => line.OrderType == POOrderType.RegularSubcontract &&
                    extension.SubcontractNumber.IsIn(null, line.OrderNbr));
        }

        public virtual void _(Events.RowSelected<APInvoice> args)
        {
            AddSubcontracts.SetEnabled(Base1.addPOOrder.GetEnabled());
            AddSubcontracts.SetVisible(Base1.addPOOrder.GetVisible());
            AddSubcontractLines.SetEnabled(Base2.addPOOrderLine.GetEnabled());
            AddSubcontractLines.SetVisible(Base2.addPOOrderLine.GetVisible());
        }

        [PXButton]
        [PXUIField(DisplayName = ApMessages.AddSubcontracts, FieldClass = ApMessages.FieldClass.Distribution,
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [APMigrationModeDependentActionRestriction(
            restrictInMigrationMode: true,
            restrictForRegularDocumentInMigrationMode: true,
            restrictForUnreleasedMigratedDocumentInNormalMode: true)]
        public virtual IEnumerable addSubcontracts(PXAdapter adapter)
        {
            Base.checkTaxCalcMode();
            if (ShouldAddSubcontracts())
            {
                Base.updateTaxCalcMode();
                return addSubcontract(adapter);
            }
            return adapter.Get();
        }

        [PXButton]
        [PXUIField(DisplayName = ApMessages.AddSubcontract,
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        public virtual IEnumerable addSubcontract(PXAdapter adapter)
        {
            return AddLines(Base1.AddPOOrder2, adapter);
        }

        [PXButton]
        [PXUIField(DisplayName = ApMessages.AddSubcontractLine, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select)]
        [APMigrationModeDependentActionRestriction(
            restrictInMigrationMode: true,
            restrictForRegularDocumentInMigrationMode: true,
            restrictForUnreleasedMigratedDocumentInNormalMode: true)]
        public virtual IEnumerable addSubcontractLines(PXAdapter adapter)
        {
            Base.checkTaxCalcMode();
            return ShouldAddSubcontractLines()
                ? addSubcontractLine(adapter)
                : adapter.Get();
        }

        [PXButton]
        [PXUIField(DisplayName = ApMessages.AddSubcontractLine, MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select, Visible = false)]
        public virtual IEnumerable addSubcontractLine(PXAdapter adapter)
        {
            return AddLines(Base2.AddPOOrderLine2, adapter);
        }

        private static IEnumerable AddLines(Func<PXAdapter, IEnumerable> addLine, PXAdapter adapter)
        {
            try
            {
                return addLine(adapter);
            }
            catch (PXException exception) when (exception.MessageNoPrefix == PoMessages.FailedToAddLine)
            {
                throw new Exception(ApMessages.FailedToAddSubcontractLinesError);
            }
        }

        private bool ShouldAddSubcontracts()
        {
            return IsAdditionSubcontractsAvailable() &&
                Subcontracts.AskExt(AddSubcontractsPanelInitializeHandler, true).IsPositive();
        }

        private bool ShouldAddSubcontractLines()
        {
            return IsAdditionSubcontractsAvailable() &&
                SubcontractLines.AskExt(AddSubcontractLinesPanelInitializeHandler, true).IsPositive();
        }

        private bool IsAdditionSubcontractsAvailable()
        {
            return Base.Document.Current != null &&
                Base.Document.Current.DocType == APDocType.Invoice &&
                Base.Document.Current.Released == false &&
                Base.Document.Current.Prebooked == false;
        }

        private void AddSubcontractLinesPanelInitializeHandler(PXGraph graph, string view)
        {
            ClearViewCache(Base2.orderfilter);
            ClearViewCache(SubcontractLines);
        }

        private void AddSubcontractsPanelInitializeHandler(PXGraph graph, string view)
        {
            var linkLineExtension = Base.GetExtension<LinkLineExtension>();
            ClearViewCache(linkLineExtension.filter);
            ClearViewCache(Subcontracts);
        }

        private static void ClearViewCache(PXSelectBase selectBase)
        {
            selectBase.Cache.ClearQueryCache();
            selectBase.View.Clear();
            selectBase.Cache.Clear();
        }

        private string GetSubcontractProjectName(POOrder subcontract)
        {
            var query = new PXSelectJoin<Contract,
                LeftJoin<POLine, On<POLine.projectID, Equal<Contract.contractID>>>,
                Where<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
                    And<POLine.orderType, Equal<POOrderType.regularSubcontract>>>,
                OrderBy<Asc<POLine.lineNbr>>>(Base);
            return query.SelectSingle(subcontract.OrderNbr)?.ContractCD;
        }
    }
}
