using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    public class PostDocBatchMaint : PXGraph<PostDocBatchMaint, FSPostBatch>
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

        public PXSelectJoin<FSPostDoc,
                LeftJoin<FSServiceOrder,
                    On<FSServiceOrder.sOID, Equal<FSPostDoc.sOID>>,
                LeftJoin<FSAppointment,
                    On<FSAppointment.appointmentID, Equal<FSPostDoc.appointmentID>>,
                LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSServiceOrder.billCustomerID>>,
                InnerJoin<FSAddress,
                    On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                LeftJoin<FSGeoZonePostalCode,
                    On<FSGeoZonePostalCode.postalCode, Equal<FSAddress.postalCode>>,
                LeftJoin<FSGeoZone,
                    On<FSGeoZone.geoZoneID, Equal<FSGeoZonePostalCode.geoZoneID>>>>>>>>,
                Where<
                    FSPostDoc.batchID, Equal<Current<FSPostBatch.batchID>>>>
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
        #region ActualDateTimeBegin
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Start Time", BqlField = typeof(FSAppointment.actualDateTimeBegin))]
        [PXUIField(DisplayName = "Actual Date", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSAppointment_ActualDateTimeBegin_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ActualDateTimeEnd
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "End Time", BqlField = typeof(FSAppointment.actualDateTimeEnd))]
        [PXUIField(DisplayName = "Actual Date End", Visibility = PXUIVisibility.Invisible)]
        protected virtual void FSAppointment_ActualDateTimeEnd_CacheAttached(PXCache sender)
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
            FSPostDoc postingBatchDetailRow = BatchDetailsInfo.Current;

            if (postingBatchDetailRow.PostedTO == ID.Batch_PostTo.SO)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
                    graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(postingBatchDetailRow.PostRefNbr, postingBatchDetailRow.PostDocType);
                    throw new PXRedirectRequiredException(graphSOOrderEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            else if (postingBatchDetailRow.PostedTO == ID.Batch_PostTo.AR)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(postingBatchDetailRow.PostRefNbr, postingBatchDetailRow.PostDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.PostedTO == ID.Batch_PostTo.AP)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(postingBatchDetailRow.PostRefNbr, postingBatchDetailRow.PostDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (postingBatchDetailRow.PostedTO == ID.Batch_PostTo.SI)
            {
                SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>(postingBatchDetailRow.PostRefNbr, postingBatchDetailRow.PostDocType);
                throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
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
