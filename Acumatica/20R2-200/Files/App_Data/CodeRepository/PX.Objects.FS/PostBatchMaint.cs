using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
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

        [PXHidden]
        public PXSelect<PostingBatchDetail> PostingBatchDetails;

        public PXSelectJoinGroupBy<FSCreatedDoc,
                InnerJoin<PostingBatchDetail,     
                    On<
                        PostingBatchDetail.batchID, Equal<FSCreatedDoc.batchID>,
                        And<
                            Where2<
                                Where<
                                    PostingBatchDetail.iNPosted, Equal<True>,
                                And<
                                    FSCreatedDoc.createdDocType, Equal<PostingBatchDetail.iNDocType>,
                                And<
                                    FSCreatedDoc.createdRefNbr, Equal<PostingBatchDetail.iNRefNbr>>>>,
                            Or<
                                Where2<
                                    Where<
                                        PostingBatchDetail.pMPosted, Equal<True>,
                                    And<
                                        FSCreatedDoc.createdDocType, Equal<PostingBatchDetail.pMDocType>,
                                    And<
                                        FSCreatedDoc.createdRefNbr, Equal<PostingBatchDetail.pMRefNbr>>>>,
                                Or<
                                    Where2<
                                        Where<
                                            PostingBatchDetail.sOPosted, Equal<True>,
                                        And<
                                            FSCreatedDoc.createdDocType, Equal<PostingBatchDetail.sOOrderType>,
                                        And<
                                            FSCreatedDoc.createdRefNbr, Equal<PostingBatchDetail.sOOrderNbr>>>>,
                                Or<
                                    Where2<
                                        Where<
                                            PostingBatchDetail.sOInvPosted, Equal<True>,
                                        And<
                                            FSCreatedDoc.createdDocType, Equal<PostingBatchDetail.sOInvDocType>,
                                        And<
                                            FSCreatedDoc.createdRefNbr, Equal<PostingBatchDetail.sOInvRefNbr>>>>,
                                Or<
                                    Where2<
                                        Where<
                                            PostingBatchDetail.aPPosted, Equal<True>,
                                        And<
                                            FSCreatedDoc.createdDocType, Equal<PostingBatchDetail.apDocType>,
                                        And<
                                            FSCreatedDoc.createdRefNbr, Equal<PostingBatchDetail.apRefNbr>>>>,
                                Or<
                                    Where<
                                        PostingBatchDetail.aRPosted, Equal<True>,
                                    And<
                                        FSCreatedDoc.createdDocType, Equal<PostingBatchDetail.arDocType>,
                                    And<
                                        FSCreatedDoc.createdRefNbr, Equal<PostingBatchDetail.arRefNbr>>>>>>>>>>>>>>>>>,
                Where<
                    FSCreatedDoc.batchID, Equal<Current<FSPostBatch.batchID>>>,
               Aggregate<
                    GroupBy<FSCreatedDoc.recordID,
                    GroupBy<FSCreatedDoc.batchID,
                    GroupBy<FSCreatedDoc.createdDocType,
                    GroupBy<FSCreatedDoc.createdRefNbr,
                    GroupBy<PostingBatchDetail.srvOrdType,
                    GroupBy<PostingBatchDetail.sOID,
                    GroupBy<PostingBatchDetail.appointmentID,
                    GroupBy<PostingBatchDetail.billCustomerID,
                    GroupBy<PostingBatchDetail.branchLocationID>>>>>>>>>>,
                OrderBy<
                    Asc<FSCreatedDoc.recordID,
                    Asc<PostingBatchDetail.sOID,
                    Asc<PostingBatchDetail.appointmentID>>>>>
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
        [PXSelector(typeof(Search<FSServiceOrder.sOID>), SubstituteKey = typeof(FSServiceOrder.refNbr))]
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
            FSCreatedDoc postingBatchDetailRow = BatchDetailsInfo.Current;

            if (postingBatchDetailRow.PostTo == ID.Batch_PostTo.SO)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
                    graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(postingBatchDetailRow.CreatedRefNbr, postingBatchDetailRow.CreatedDocType);
                    throw new PXRedirectRequiredException(graphSOOrderEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            else if (postingBatchDetailRow.PostTo == ID.Batch_PostTo.AR)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(postingBatchDetailRow.CreatedRefNbr, postingBatchDetailRow.CreatedDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.PostTo == ID.Batch_PostTo.SI)
            {
                SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>(postingBatchDetailRow.CreatedRefNbr, postingBatchDetailRow.CreatedDocType);
                throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.PostTo == ID.Batch_PostTo.AP)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(postingBatchDetailRow.CreatedRefNbr, postingBatchDetailRow.CreatedDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.PostTo == ID.Batch_PostTo.IN)
            {
                INIssueEntry graphINIssueEntry = PXGraph.CreateInstance<INIssueEntry>();
                graphINIssueEntry.issue.Current = graphINIssueEntry.issue.Search<INRegister.refNbr>(postingBatchDetailRow.CreatedRefNbr, postingBatchDetailRow.CreatedDocType);
                throw new PXRedirectRequiredException(graphINIssueEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.PostTo == ID.Batch_PostTo.PM)
            {
                RegisterEntry graphRegisterEntry = PXGraph.CreateInstance<RegisterEntry>();
                graphRegisterEntry.Document.Current = graphRegisterEntry.Document.Search<PMRegister.refNbr>(postingBatchDetailRow.CreatedRefNbr, postingBatchDetailRow.CreatedDocType);
                throw new PXRedirectRequiredException(graphRegisterEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region Events Handlers

        #region FSPostbatch

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSPostBatch> e)
        {
            PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
            PXUIFieldAttribute.SetEnabled<FSPostBatch.batchNbr>(BatchRecords.Cache, e.Row, true);
            BatchRecords.Cache.AllowInsert = false;
        }

        protected virtual void _(Events.RowInserting<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSPostBatch> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSPostBatch> e)
        {
        }

        #endregion
        
        #region PostingBatchDetail

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<PostingBatchDetail> e)
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
                                          .Select(e.Cache.Graph, postingBatchDetailRow.SOOrderNbr, postingBatchDetailRow.SOOrderType);

                    postingBatchDetailRow.InvoiceRefNbr = soOrderShipment?.InvoiceNbr;
                }
            }
            else if (postingBatchDetailRow.ARPosted == true || postingBatchDetailRow.SOInvPosted == true)
            {
                postingBatchDetailRow.InvoiceRefNbr = postingBatchDetailRow.Mem_DocNbr;
            }
        }

        protected virtual void _(Events.RowSelected<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowInserting<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowInserted<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowUpdating<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowUpdated<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowDeleting<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowDeleted<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowPersisting<PostingBatchDetail> e)
        {
        }

        protected virtual void _(Events.RowPersisted<PostingBatchDetail> e)
        {
        }

        #endregion
        
        #endregion
    }
}
