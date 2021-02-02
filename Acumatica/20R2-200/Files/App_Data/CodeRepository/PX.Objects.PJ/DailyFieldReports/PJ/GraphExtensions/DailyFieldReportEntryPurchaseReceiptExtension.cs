using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PJ.DailyFieldReports.PO.CacheExtensions;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.PO;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryPurchaseReceiptExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.PurchaseReceipts)]
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportPurchaseReceipt>
            .LeftJoin<POReceipt>
                .On<DailyFieldReportPurchaseReceipt.purchaseReceiptId.IsEqual<POReceipt.receiptNbr>>
            .LeftJoin<Vendor>
                .On<Vendor.bAccountID.IsEqual<POReceipt.vendorID>>
            .Where<DailyFieldReportPurchaseReceipt.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View PurchaseReceipts;

        public PXSetup<APSetup> AccountPayableSetup;

        public PXAction<DailyFieldReport> CreateNewPurchaseReceipt;

        public PXAction<DailyFieldReport> CreateNewPurchaseReturn;

        public PXAction<DailyFieldReport> ViewPurchaseReceipt;

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.PurchaseReceipt, ViewNames.PurchaseReceipts);

	    //Functionality is disabled because it is not ready for use. 
        public static bool IsActive()
        {
            return false;
        }

        [PXButton]
        [PXUIField]
        public virtual void viewPurchaseReceipt()
        {
            var purchaseReceiptEntry = PXGraph.CreateInstance<POReceiptEntry>();
            purchaseReceiptEntry.Document.Current = GetPurchaseReceipt();
            PXRedirectHelper.TryRedirect(purchaseReceiptEntry, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton]
        [PXUIField(DisplayName = "Create New Purchase Receipt")]
        public virtual void createNewPurchaseReceipt()
        {
            CreatePurchaseReceipt(POReceiptType.POReceipt);
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField(DisplayName = "Create New Purchase Return")]
        public virtual void createNewPurchaseReturn()
        {
            CreatePurchaseReceipt(POReceiptType.POReturn);
        }

        public override void _(Events.RowSelected<DailyFieldReport> args)
        {
            base._(args);
            if (args.Row != null)
            {
                var isActionAvailable = IsCreationActionAvailable(args.Row);
                CreateNewPurchaseReceipt.SetEnabled(isActionAvailable);
                CreateNewPurchaseReturn.SetEnabled(isActionAvailable);
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportPurchaseReceipt))
            {
                RelationNumber = typeof(DailyFieldReportPurchaseReceipt.purchaseReceiptId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(PurchaseReceipts);
        }

        private void CreatePurchaseReceipt(string receiptType)
        {
            Base.Actions.PressSave();
            var graph = PXGraph.CreateInstance<POReceiptEntry>();
            InsertPurchaseReceipt(graph, receiptType);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
        }

        private void InsertPurchaseReceipt(POReceiptEntry graph, string receiptType)
        {
            var purchaseReceipt = graph.Document.Insert();
            var dailyFieldReport = Base.DailyFieldReport.Current;
            if (AccountPayableSetup.Current.RequireSingleProjectPerDocument == true)
            {
                purchaseReceipt.ProjectID = dailyFieldReport.ProjectId;
            }
            purchaseReceipt.ReceiptType = receiptType;
            purchaseReceipt.ReceiptDate = dailyFieldReport.Date;
            purchaseReceipt.GetExtension<PoReceiptExtension>().DailyFieldReportId = dailyFieldReport.DailyFieldReportId;
            graph.Document.Cache.SetValueExt<PoReceiptExtension.dailyFieldReportId>(purchaseReceipt,
                dailyFieldReport.DailyFieldReportId);
        }

        private POReceipt GetPurchaseReceipt()
        {
            return Base.Select<POReceipt>()
                .SingleOrDefault(pr => pr.ReceiptNbr == PurchaseReceipts.Current.PurchaseReceiptId);
        }
    }
}