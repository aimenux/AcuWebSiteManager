using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    public class InventoryPostBatchMaint : PXGraph<InventoryPostBatchMaint, FSPostBatch>
    {
        #region Selects
        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<Customer> Customer;
        [PXHidden]
        public PXSelect<FSServiceOrder> FSServiceOrderDummy;
        public PXSelect<FSPostBatch, Where<FSPostBatch.postTo, Equal<ListField_PostTo.IN>>> BatchRecords;
        [PXHidden]
        public PXSelect<FSPostDet>
               BatchDetails;

        public PXSelectReadonly<InventoryPostingBatchDetail,
                Where<
                    InventoryPostingBatchDetail.batchID, Equal<Current<FSPostBatch.batchID>>>>
                BatchDetailsInfo;
        #endregion

        #region CacheAttached
        #region FSPostBatch_BatchNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(
                           Search<FSPostBatch.batchNbr, 
                                Where<FSPostBatch.postTo, Equal<ListField_PostTo.IN>,
                                    And<FSPostBatch.status, NotEqual<FSPostBatch.status.temporary>>>>),
                            new Type[]
                                        {
                                            typeof(FSPostBatch.batchNbr),
                                            typeof(FSPostBatch.finPeriodID),
                                            typeof(FSPostBatch.cutOffDate),
                                        })]
        [AutoNumber(typeof(Search<FSSetup.postBatchNumberingID>),
                    typeof(AccessInfo.businessDate))]
        protected virtual void FSPostBatch_BatchNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSPostBatch_QtyDoc
        [PXDBInt]
        [PXUIField(DisplayName = "Lines Processed")]
        protected virtual void FSPostBatch_QtyDoc_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSPostBatch_CutOffDate
        [PXDBDate]
        [PXUIField(DisplayName = "Up to Date")]
        protected virtual void FSPostBatch_CutOffDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSPostBatch_FinPeriodID
        [FinPeriodID(BqlField = typeof(FSPostBatch.finPeriodID))]
        [PXUIField(DisplayName = "Document Period", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSPostBatch_FinPeriodID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSPostBatch_InvoiceDate
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Document Date", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSPostBatch_InvoiceDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostingBatchDetail_AppointmentID 
        [PXDBInt(BqlField = typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        [PXSelector(typeof(Search<FSAppointment.appointmentID>), SubstituteKey = typeof(FSAppointment.refNbr))]
        protected virtual void InventoryPostingBatchDetail_AppointmentID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostingBatchDetail_SOID
        [PXDBInt(BqlField = typeof(FSAppointment.sOID))]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(Search<FSServiceOrder.sOID>), SubstituteKey = typeof(FSServiceOrder.refNbr))]
        protected virtual void InventoryPostingBatchDetail_SOID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostingBatchDetail_AcctName
        [PXDBString(60, IsUnicode = true, BqlField = typeof(Customer.acctName))]
        [PXDefault]
        [PXFieldDescription]
        [PXMassMergableField]
        [PXUIField(DisplayName = "Billing Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void InventoryPostingBatchDetail_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
        
        #region Actions
        public PXAction<FSPostBatch> OpenDocument;
        [PXButton]
        [PXUIField(DisplayName = "Open Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openDocument()
        {
            InventoryPostingBatchDetail inventoryPostingBatchDetailRow = BatchDetailsInfo.Current;

            FSPostDet fsPostDetRow = PXSelectJoin<FSPostDet,
                                     InnerJoin<FSPostInfo,
                                         On<FSPostInfo.postID, Equal<FSPostDet.postID>>,
                                     InnerJoin<FSAppointmentDet,
                                         On<FSAppointmentDet.postID, Equal<FSPostInfo.postID>>>>,
                                     Where<
                                         FSPostDet.batchID, Equal<Current<FSPostBatch.batchID>>,
                                         And<FSAppointmentDet.appDetID, Equal<Required<FSAppointmentDet.appDetID>>>>>
                                     .Select(this, inventoryPostingBatchDetailRow.AppointmentInventoryItemID);

            if (fsPostDetRow != null && fsPostDetRow.INPosted == true)
            {
                if (fsPostDetRow.INDocType.Trim() == INDocType.Receipt)
                {
                    INReceiptEntry graphINReceiptEntry = PXGraph.CreateInstance<INReceiptEntry>();
                    graphINReceiptEntry.receipt.Current = graphINReceiptEntry.receipt.Search<INRegister.refNbr>(fsPostDetRow.INRefNbr);
                    throw new PXRedirectRequiredException(graphINReceiptEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else
                {
                    INIssueEntry graphINIssueEntry = PXGraph.CreateInstance<INIssueEntry>();
                    graphINIssueEntry.issue.Current = graphINIssueEntry.issue.Search<INRegister.refNbr>(fsPostDetRow.INRefNbr);
                    throw new PXRedirectRequiredException(graphINIssueEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
        }
        #endregion

        #region Event Handlers

        #region FSPostBatch

        protected virtual void _(Events.RowSelected<FSPostBatch> e)
        {
            PXUIFieldAttribute.SetEnabled(BatchRecords.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<FSPostBatch.batchNbr>(BatchRecords.Cache, e.Row, true);
            BatchRecords.Cache.AllowInsert = false;
        }
        #endregion

        #endregion
    }
}
