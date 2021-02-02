using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    public class PostBatchMaint : PXGraph<PostBatchMaint, FSPostBatch>
    {
        #region Selects
        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<Customer> Customer;
        [PXHidden]
        public PXSelect<FSAppointment> Appointment;
        [PXHidden]
        public PXSelect<FSServiceOrder> FSServiceOrderDummy;

        public PXSelect<FSPostBatch, 
                    Where<
                        FSPostBatch.postTo, NotEqual<ListField_PostTo.IN>,
                        And<
                            Where<
                                FSPostBatch.status, IsNull,
                                Or<FSPostBatch.status, NotEqual<FSPostBatch.status.temporary>>>>>> BatchRecords;
        [PXHidden]
        public PXSelect<FSPostDet> BatchDetails;

        public PXSelectGroupBy<PostingBatchDetail,
                Where<
                    PostingBatchDetail.batchID, Equal<Current<FSPostBatch.batchID>>>,
               Aggregate<
                    GroupBy<PostingBatchDetail.sOID,
                    GroupBy<PostingBatchDetail.appointmentID,
                    GroupBy<PostingBatchDetail.noteID,
                    GroupBy<PostingBatchDetail.srvOrdType,
                    GroupBy<PostingBatchDetail.apRefNbr,
                    GroupBy<PostingBatchDetail.aRPosted,
                    GroupBy<PostingBatchDetail.aPPosted,
                    GroupBy<PostingBatchDetail.sOPosted,
                    GroupBy<PostingBatchDetail.iNPosted,
                    GroupBy<PostingBatchDetail.sOInvPosted>>>>>>>>>>>>
                BatchDetailsInfo;
        #endregion

        #region CacheAttached 
        #region FSPostBatch_BatchNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSPostBatch.batchNbr, Where<FSPostBatch.postTo, NotEqual<ListField_PostTo.IN>, And<FSPostBatch.status, NotEqual<FSPostBatch.status.temporary>>>>))]
        [AutoNumber(typeof(Search<FSSetup.postBatchNumberingID>),
                    typeof(AccessInfo.businessDate))]
        protected virtual void FSPostBatch_BatchNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostingBatchDetail_AppointmentID
        [PXDBInt(BqlField = typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        [PXSelector(typeof(Search<FSAppointment.appointmentID>), SubstituteKey = typeof(FSAppointment.refNbr))]
        protected virtual void PostingBatchDetail_AppointmentID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostingBatchDetail_SOID
        [PXDBInt(BqlField = typeof(FSServiceOrder.sOID))]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(Search<FSServiceOrder.sOID, Where<FSServiceOrder.srvOrdType, Equal<Current<PostingBatchDetail.srvOrdType>>>>), SubstituteKey = typeof(FSServiceOrder.refNbr))]
        protected virtual void PostingBatchDetail_SOID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region PostingBatchDetail_AcctName
        [PXDBString(60, IsUnicode = true, BqlField = typeof(Customer.acctName))]
        [PXDefault]
        [PXFieldDescription]
        [PXMassMergableField]
        [PXUIField(DisplayName = "Billing Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void PostingBatchDetail_AcctName_CacheAttached(PXCache sender)
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
            PostingBatchDetail postingBatchDetailRow = BatchDetailsInfo.Current;

            if (postingBatchDetailRow.SOPosted == true)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
                    graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(postingBatchDetailRow.SOOrderNbr, postingBatchDetailRow.SOOrderType);
                    throw new PXRedirectRequiredException(graphSOOrderEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            else if (postingBatchDetailRow.ARPosted == true)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(postingBatchDetailRow.ARRefNbr, postingBatchDetailRow.ARDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.SOInvPosted == true)
            {
                SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>(postingBatchDetailRow.SOInvRefNbr, postingBatchDetailRow.SOInvDocType);
                throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.APPosted == true)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(postingBatchDetailRow.APRefNbr, postingBatchDetailRow.APDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region Events
        protected void FSPostBatch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled(BatchRecords.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<FSPostBatch.batchNbr>(BatchRecords.Cache, e.Row, true);
            BatchRecords.Cache.AllowInsert = false;
        }

        protected virtual void PostingBatchDetail_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            PostingBatchDetail postingBatchDetailRow = e.Row as PostingBatchDetail;

            if (postingBatchDetailRow.SOPosted == true)
            {
                using (new PXConnectionScope())
                {
                    var soOrderShipment = (SOOrderShipment)PXSelectReadonly<SOOrderShipment,
                                          Where<
                                              SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
                                          And<
                                              SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>>>>
                                    .Select(cache.Graph, postingBatchDetailRow.SOOrderNbr, postingBatchDetailRow.SOOrderType);

                    postingBatchDetailRow.InvoiceRefNbr = soOrderShipment?.InvoiceNbr;
                }
            }
            else if (postingBatchDetailRow.ARPosted == true || postingBatchDetailRow.SOInvPosted == true)
            {
                postingBatchDetailRow.InvoiceRefNbr = postingBatchDetailRow.Mem_DocNbr;
            }
        }
        #endregion
    }
}
