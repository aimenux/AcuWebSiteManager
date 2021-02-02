using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class UpdateInventoryPost : PXGraph<UpdateInventoryPost>
    {
        #region Select
        // BAccount Workaround
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;
        [PXHidden]
        public PXSetup<FSRouteSetup> RouteSetupRecord;
        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<FSAppointment> Appointment;
        [PXHidden]
        public PXSelect<Customer> Customer;
        public PXFilter<UpdateInventoryFilter> Filter;
        public PXCancel<UpdateInventoryFilter> Cancel;
        [PXFilterable]
        [PXViewDetailsButton(typeof(UpdateInventoryFilter))]
        public PXFilteredProcessingJoin<FSAppointmentDet, UpdateInventoryFilter,
                InnerJoin<FSAppointment,
                    On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                InnerJoin<FSServiceOrder,
                    On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                InnerJoin<Customer,
                    On<Customer.bAccountID, Equal<FSServiceOrder.billCustomerID>>,
                InnerJoin<FSSrvOrdType,
                    On<FSSrvOrdType.srvOrdType, Equal<FSAppointment.srvOrdType>>,
                LeftJoin<FSPostInfo,
                    On<FSPostInfo.postID, Equal<FSAppointmentDet.postID>>,
                InnerJoin<FSAddress,
                    On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                LeftJoin<FSGeoZonePostalCode,
                    On<FSGeoZonePostalCode.postalCode, Equal<FSAddress.postalCode>>,
                LeftJoin<FSGeoZone,
                    On<FSGeoZone.geoZoneID, Equal<FSGeoZonePostalCode.geoZoneID>>>>>>>>>>,
            Where<
                FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                And<FSAppointment.status, Equal<ListField_Status_Appointment.Closed>,
                And<FSSrvOrdType.enableINPosting, Equal<True>,
                And2<
                    Where<FSPostInfo.postID, IsNull, 
                        Or<FSPostInfo.iNPosted, Equal<False>>>,
                And2<
                    Where<Current<UpdateInventoryFilter.cutOffDate>, IsNull,
                        Or<FSAppointment.executionDate, LessEqual<Current<UpdateInventoryFilter.cutOffDate>>>>,
                And2<
                    Where<Current<UpdateInventoryFilter.routeDocumentID>, IsNull,
                        Or<FSAppointment.routeDocumentID, Equal<Current<UpdateInventoryFilter.routeDocumentID>>>>,
                And<
                    Where<Current<UpdateInventoryFilter.appointmentID>, IsNull,
                        Or<FSAppointment.appointmentID, Equal<Current<UpdateInventoryFilter.appointmentID>>>>>>>>>>>> Appointments;

        [PXDBString(60, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Billing Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void Customer_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region CacheAttached
        #region FSAppointment_RefNbr
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        [PXSelector(typeof(Search<FSAppointment.refNbr>))]
        protected virtual void FSAppointment_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_SORefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(typeof(Search<FSServiceOrder.refNbr>))]
        protected virtual void FSAppointment_SORefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Actions
        #region ViewInvoice
        public PXAction<UpdateInventoryFilter> viewPostBatch;
        [PXUIField(DisplayName = "")]
        public virtual IEnumerable ViewPostBatch(PXAdapter adapter)
        {
            if (Appointments.Current != null)
            {
                FSAppointmentDet fsAppointmentInventoryItemRow = Appointments.Current;

                if (!string.IsNullOrEmpty(fsAppointmentInventoryItemRow.Mem_BatchNbr))
                {
                    graphUpdatePostBatchMaint.BatchRecords.Current = graphUpdatePostBatchMaint.BatchRecords.Search<FSPostBatch.batchNbr>(fsAppointmentInventoryItemRow.Mem_BatchNbr);
                    throw new PXRedirectRequiredException(graphUpdatePostBatchMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }

            return adapter.Get();
        }

        #endregion
        #endregion

        private PostInfoEntry graphPostInfoEntry;
        private InventoryPostBatchMaint graphUpdatePostBatchMaint;

        public UpdateInventoryPost()
        {
            graphPostInfoEntry = PXGraph.CreateInstance<PostInfoEntry>();
            graphUpdatePostBatchMaint = PXGraph.CreateInstance<InventoryPostBatchMaint>();

            UpdateInventoryFilter filter = Filter.Current;

            Appointments.SetProcessDelegate(
                delegate(List<FSAppointmentDet> fsAppointmentInventoryItem)
                {
                    CreateDocuments(fsAppointmentInventoryItem, filter);
                });
        }

        #region Virtual Methods
        /// <summary>
        /// Gets the information of the Appointment and AppointmentInventoryItem using as reference the [appointmentID] and [appointmentInventoryItemID].
        /// </summary>
        /// 

        // AC-143262 Review this method
        public virtual SharedClasses.AppointmentInventoryItemInfo GetAppointmentInventoryItemInfo(int? appointmentID, int? appDetID, int index)
        {
            //PXResult<FSAppointment, FSServiceOrder, FSSrvOrdType, FSAppointmentDet, FSAppointmentDet> bqlResult =
            //                                                                            (PXResult<FSAppointment, FSServiceOrder, FSSrvOrdType, FSAppointmentDet, FSAppointmentDet>)
            //                                                                            PXSelectReadonly2<FSAppointment,
            //                                                                            InnerJoin<FSServiceOrder,
            //                                                                                On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>,
            //                                                                                    And<FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>,
            //                                                                            InnerJoin<FSSrvOrdType,
            //                                                                                On<FSSrvOrdType.srvOrdType, Equal<FSAppointment.srvOrdType>>,
            //                                                                            InnerJoin<FSAppointmentDet,
            //                                                                                On<FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>,
            //                                                                                    And<FSAppointmentDet.appDetID, Equal<Required<FSAppointmentDet.appDetID>>>>,
            //                                                                            InnerJoin<FSAppointmentDet,
            //                                                                                On<FSAppointmentDet.sODetID, Equal<FSAppointmentDet.sODetID>>>>>>>
            //                                                                            .Select(this, appointmentID, appDetID);

            SharedClasses.AppointmentInventoryItemInfo appointmentInfoToReturn = new SharedClasses.AppointmentInventoryItemInfo();

            //if (bqlResult != null)
            //{
            //    FSAppointment fsAppointmentRow                              = (FSAppointment)bqlResult;
            //    FSServiceOrder fsServiceOrderRow                            = (FSServiceOrder)bqlResult;
            //    FSSrvOrdType fsSrvOrdTypeRow                                = (FSSrvOrdType)bqlResult;
            //    FSAppointmentDet fsAppointmentInventoryItemRow    = (FSAppointmentDet)bqlResult;
            //    FSAppointmentDet fsAppointmentDetRow                        = (FSAppointmentDet)bqlResult;

            //    appointmentInfoToReturn = new SharedClasses.AppointmentInventoryItemInfo(fsAppointmentRow, fsServiceOrderRow, fsSrvOrdTypeRow, fsAppointmentDetRow, fsAppointmentInventoryItemRow, index);
            //}

            return appointmentInfoToReturn;
        }

        #endregion

        #region Event Handlers
        protected virtual void UpdateInventoryFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            UpdateInventoryFilter createInvoiceFilterRow = (UpdateInventoryFilter)e.Row;

            string errorMessage = PXUIFieldAttribute.GetErrorOnly<UpdateInventoryFilter.finPeriodID>(cache, createInvoiceFilterRow);
            bool enableProcessButtons = string.IsNullOrEmpty(errorMessage) == true;

            Appointments.SetProcessAllEnabled(enableProcessButtons);
            Appointments.SetProcessEnabled(enableProcessButtons);
        }
        #endregion

        #region ProcessHandler

        /// <summary>
        /// Group the Appointment List [fsAppointmentRows] to determine how to post them.
        /// </summary>
        public virtual void CreateDocuments(List<FSAppointmentDet> fsAppointmentInventoryItemRows, UpdateInventoryFilter filter)
        {
            List<SharedClasses.AppointmentInventoryItemInfo> appointmentList = fsAppointmentInventoryItemRows.Select((n, i) => GetAppointmentInventoryItemInfo(n.AppointmentID, n.AppDetID, i)).ToList();

            List<SharedClasses.AppointmentInventoryItemGroup> listGroupToUpdateInIN = null;

            if (RouteSetupRecord.Current != null)
            {
                if (RouteSetupRecord.Current.GroupINDocumentsByPostingProcess == true)
                {
                    listGroupToUpdateInIN =
                                        appointmentList.GroupBy(
                                            u => new { u.ServiceType },
                                            (key, group) => new
                                            {
                                                Key = key,
                                                Group = (List<SharedClasses.AppointmentInventoryItemInfo>)group.ToList()
                                            })
                                        .Select(List => new SharedClasses.AppointmentInventoryItemGroup((int)(List.Key.ServiceType == ID.Service_Action_Type.PICKED_UP_ITEMS ? 1 : 0), List.Key.ServiceType, List.Group))
                                        .OrderBy(List => List.Pivot)
                                        .ToList();
                }
                else
                {
                    listGroupToUpdateInIN =
                                        appointmentList.GroupBy(
                                            u => new { u.AppointmentID, u.ServiceType },
                                            (key, group) => new
                                            {
                                                Key = key,
                                                Group = (List<SharedClasses.AppointmentInventoryItemInfo>)group.ToList()
                                            })
                                        .Select(List => new SharedClasses.AppointmentInventoryItemGroup((int)List.Key.AppointmentID, List.Key.ServiceType, List.Group))
                                        .OrderBy(List => List.Pivot)
                                        .ToList();
                }

                if (listGroupToUpdateInIN != null && listGroupToUpdateInIN.Count > 0)
                {
                    CreateDocumentByGroup(fsAppointmentInventoryItemRows, listGroupToUpdateInIN, filter);
                }
            }
        }

        /// <summary>
        /// Defines where the AppointmentInventoryItems are going to be posted, depending of the ServiceType (Pickup or Delivery).
        /// </summary>
        /// <param name="fsAppointmentInventoryItemRows"> Items to be posted (Original List in the screen).</param>
        /// <param name="listGroupToInvoice"> Items to be posted (Groups to be posted after grouping rules).</param>
        /// <param name="filter"> Header of the screen (Filters).</param>
        public virtual void CreateDocumentByGroup(List<FSAppointmentDet> fsAppointmentInventoryItemRows, List<SharedClasses.AppointmentInventoryItemGroup> listGroupToInvoice, UpdateInventoryFilter filter)
        {
            FSPostBatch fsPostBatchRow = CreateFSPostBatch(fsAppointmentInventoryItemRows.Count, ID.Batch_PostTo.IN, filter.CutOffDate, filter.FinPeriodID, filter.DocumentDate);
            int documentsPosted = 0;

            documentsPosted = CreateDocumentsInIN(fsPostBatchRow, fsAppointmentInventoryItemRows, listGroupToInvoice, filter);

            if (documentsPosted > 0)
            {
                fsPostBatchRow.QtyDoc = documentsPosted;
                UpdateFSPostBatch(fsPostBatchRow);
            }
            else
            {
                DeleteFSPostBatch(fsPostBatchRow);
            }
        }
        
        /// <summary>
        /// Creates one or more documents in Inventory depending of the number of FSAppointmentInventoryItem in the list [fsAppointmentInventoryItemRows].
        /// </summary>
        public virtual int CreateDocumentsInIN(FSPostBatch fsPostBatchRow, List<FSAppointmentDet> fsAppointmentInventoryItemRows, List<SharedClasses.AppointmentInventoryItemGroup> listGroupToUpdateInInventory, UpdateInventoryFilter filter)
        {
            INReceiptEntry graphINReceiptEntry  = PXGraph.CreateInstance<INReceiptEntry>();
            INIssueEntry graphINIssueEntry      = PXGraph.CreateInstance<INIssueEntry>();
            int linesPosted = 0;

            foreach (SharedClasses.AppointmentInventoryItemGroup groupToUpdateInInventory in listGroupToUpdateInInventory)
            {
                string iNRefNbr = null;
                string iNDocType = null;

                foreach (SharedClasses.AppointmentInventoryItemInfo appointmentInventoryItemInfoRow in groupToUpdateInInventory.AppointmentInventoryItems)
                {
                    using (var ts_GroupToUpdateInInventory = new PXTransactionScope())
                    {
                        try
                        {
                            if (groupToUpdateInInventory.ServiceType == ID.Service_Action_Type.PICKED_UP_ITEMS)
                            {
                                // create receipt
                                CreateDocumentReceipt(graphINReceiptEntry,
                                                      appointmentInventoryItemInfoRow,
                                                      fsAppointmentInventoryItemRows[appointmentInventoryItemInfoRow.Index],
                                                      filter.DocumentDate,
                                                      filter.FinPeriodID,
                                                      fsPostBatchRow,
                                                      ref iNRefNbr,
                                                      ref iNDocType);
                            }
                            else if (groupToUpdateInInventory.ServiceType == ID.Service_Action_Type.DELIVERED_ITEMS)
                            {
                                // create issue
                                CreateDocumentIssue(graphINIssueEntry,
                                                    appointmentInventoryItemInfoRow,
                                                    fsAppointmentInventoryItemRows[appointmentInventoryItemInfoRow.Index],
                                                    filter.DocumentDate,
                                                    filter.FinPeriodID,
                                                    fsPostBatchRow,
                                                    ref iNRefNbr,
                                                    ref iNDocType);
                            } 
                            else if (groupToUpdateInInventory.ServiceType == ID.Service_Action_Type.NO_ITEMS_RELATED)
                            {
                                PXProcessing<FSAppointmentDet>.SetError(appointmentInventoryItemInfoRow.Index, TX.Error.APPOINTMENT_ITEM_CANNOT_BE_POSTED_TO_IN_NO_ITEMS_RELATED);
                            }

                            PXProcessing<FSAppointmentDet>.SetInfo(appointmentInventoryItemInfoRow.Index, TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
                            linesPosted++;
                            ts_GroupToUpdateInInventory.Complete();
                        }
                        catch (Exception e)
                        {
                            Exception latestException = ExceptionHelper.GetExceptionWithContextMessage(PXMessages.Localize(TX.Messages.COULD_NOT_PROCESS_RECORD), e);
                            PXProcessing<FSAppointmentDet>.SetError(appointmentInventoryItemInfoRow.Index, latestException);
                            
                            ts_GroupToUpdateInInventory.Dispose();
                            graphINReceiptEntry.Actions.PressCancel();
                        }
                    }
                }                
            }

            return linesPosted;
        }

        /// <summary>
        /// Creates a Posting Batch that will be used in every Posting Process.
        /// </summary>
        public virtual FSPostBatch CreateFSPostBatch(int qtyDoc, string postTo, DateTime? cutOffDate, string invoicePeriodID = null, DateTime? invoiceDate = null, int? billingCycleID = null, DateTime? upToDate = null)
        {
            FSPostBatch fsPostBatchRow = new FSPostBatch();

            fsPostBatchRow.QtyDoc = qtyDoc;
            fsPostBatchRow.BillingCycleID = billingCycleID;
            fsPostBatchRow.InvoiceDate = invoiceDate.HasValue == true ? new DateTime(invoiceDate.Value.Year, invoiceDate.Value.Month, invoiceDate.Value.Day, 0, 0, 0) : invoiceDate;
            fsPostBatchRow.PostTo = postTo;
            fsPostBatchRow.UpToDate = upToDate.HasValue == true ? new DateTime(upToDate.Value.Year, upToDate.Value.Month, upToDate.Value.Day, 0, 0, 0) : upToDate;
            fsPostBatchRow.CutOffDate = new DateTime(cutOffDate.Value.Year, cutOffDate.Value.Month, cutOffDate.Value.Day, 0, 0, 0);
            fsPostBatchRow.FinPeriodID = invoicePeriodID;

            graphUpdatePostBatchMaint.BatchRecords.Insert(fsPostBatchRow);
            graphUpdatePostBatchMaint.Save.Press();

            return graphUpdatePostBatchMaint.BatchRecords.Current;
        }

        /// <summary>
        /// Update a Posting Batch that will be used in every Posting Process.
        /// </summary>
        public virtual FSPostBatch UpdateFSPostBatch(FSPostBatch fsPostBatchRow)
        {
            graphUpdatePostBatchMaint.BatchRecords.Current = graphUpdatePostBatchMaint.BatchRecords.Search<FSPostBatch.batchID>(fsPostBatchRow.BatchID);
            graphUpdatePostBatchMaint.BatchRecords.Update(fsPostBatchRow);
            graphUpdatePostBatchMaint.Save.Press();

            return graphUpdatePostBatchMaint.BatchRecords.Current;
        }

        /// <summary>
        /// Deletes a Posting Batch record.
        /// </summary>
        public virtual void DeleteFSPostBatch(FSPostBatch fsPostBatchRow)
        {
            graphUpdatePostBatchMaint.BatchRecords.Current = graphUpdatePostBatchMaint.BatchRecords.Search<FSPostBatch.batchID>(fsPostBatchRow.BatchID);
            graphUpdatePostBatchMaint.BatchRecords.Delete(fsPostBatchRow);
            graphUpdatePostBatchMaint.Save.Press();
        }

        /// <summary>
        /// Creates a Receipt document using the parameters [fsAppointmentRow], [fsServiceOrderRow], [fsServiceOrderTypeRow] and its posting information.
        /// </summary>
        public virtual void CreateDocumentReceipt(INReceiptEntry graphINReceiptEntry, SharedClasses.AppointmentInventoryItemInfo appointmentInventoryItemInfoRow, FSAppointmentDet fsAppointmentInventoryItemRow, DateTime? documentDate, string documentPeriod, FSPostBatch fsPostBatchRow, ref string inRefNbr, ref string inDocType)
        {
            if (appointmentInventoryItemInfoRow != null)
            {
                INRegister inRegisterRow;
                #region IN Receipt Header
                if (string.IsNullOrEmpty(inRefNbr))
                {
                    inRegisterRow = new INRegister();
                    
                    inRegisterRow.DocType = INDocType.Receipt;
                    inRegisterRow.TranDate = documentDate;
                    inRegisterRow.FinPeriodID = documentPeriod;
                    inRegisterRow.TranDesc = appointmentInventoryItemInfoRow.FSAppointmentRow.DocDesc;
                    inRegisterRow.Hold = false;
                    inRegisterRow.BranchID = appointmentInventoryItemInfoRow.FSServiceOrderRow.BranchID;
                    inRegisterRow = graphINReceiptEntry.receipt.Current = graphINReceiptEntry.receipt.Insert(inRegisterRow);
                    
                    inRegisterRow = graphINReceiptEntry.receipt.Update(inRegisterRow);
                }
                else
                {
                    inRegisterRow = graphINReceiptEntry.receipt.Current = graphINReceiptEntry.receipt.Search<INRegister.refNbr>(inRefNbr);
                }
                #endregion
                INTran inTranRow;

                inTranRow = new INTran();
                inTranRow.TranType = INTranType.Receipt;
                inTranRow = graphINReceiptEntry.transactions.Current = graphINReceiptEntry.transactions.Insert(inTranRow);

                graphINReceiptEntry.transactions.Cache.SetValueExt<INTran.inventoryID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.InventoryID);

                if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                {
                    graphINReceiptEntry.transactions.Cache.SetValueExt<INTran.subItemID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.SubItemID);
                }

                graphINReceiptEntry.transactions.Cache.SetValueExt<INTran.siteID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.SiteID);
                graphINReceiptEntry.transactions.Cache.SetValueExt<INTran.qty>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.Qty);
                graphINReceiptEntry.transactions.Cache.SetValueExt<INTran.unitCost>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.UnitPrice);
                graphINReceiptEntry.transactions.Cache.SetValueExt<INTran.projectID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.ProjectID);
                graphINReceiptEntry.transactions.Cache.SetValueExt<INTran.taskID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.ProjectTaskID);

                FSxINTran fsxINTranRow = graphINReceiptEntry.transactions.Cache.GetExtension<FSxINTran>(graphINReceiptEntry.transactions.Current);

                fsxINTranRow.Source = ID.Billing_By.APPOINTMENT;
                fsxINTranRow.SOID = appointmentInventoryItemInfoRow.FSAppointmentRow.SOID;
                fsxINTranRow.AppointmentID = appointmentInventoryItemInfoRow.FSAppointmentRow.AppointmentID;
                fsxINTranRow.AppointmentDate = new DateTime(appointmentInventoryItemInfoRow.FSAppointmentRow.ActualDateTimeBegin.Value.Year,
                                                            appointmentInventoryItemInfoRow.FSAppointmentRow.ActualDateTimeBegin.Value.Month,
                                                            appointmentInventoryItemInfoRow.FSAppointmentRow.ActualDateTimeBegin.Value.Day,
                                                            0,
                                                            0,
                                                            0);

                fsxINTranRow.AppDetID = (int)appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.AppDetID;

                inTranRow = graphINReceiptEntry.transactions.Update(inTranRow);

                graphINReceiptEntry.Save.Press();

                if (string.IsNullOrEmpty(inRefNbr))
                {
                    inRefNbr = graphINReceiptEntry.receipt.Current.RefNbr;
                    inDocType = graphINReceiptEntry.receipt.Current.DocType;
                }

                UpdateReceiptPostInfo(graphINReceiptEntry, graphUpdatePostBatchMaint, graphPostInfoEntry, fsAppointmentInventoryItemRow, appointmentInventoryItemInfoRow, fsPostBatchRow);
            }
            else
            {
                throw new PXException(TX.Error.NOTHING_TO_BE_POSTED);
            }
        }

        /// <summary>
        /// Creates an Issue document using the parameters <c>fsAppointmentRow</c>, <c>fsServiceOrderRow</c>, <c>fsServiceOrderTypeRow</c> and its posting information.
        /// </summary>
        protected virtual void CreateDocumentIssue(INIssueEntry graphINIssueEntry, SharedClasses.AppointmentInventoryItemInfo appointmentInventoryItemInfoRow, FSAppointmentDet fsAppointmentInventoryItemRow, DateTime? documentDate, string documentPeriod, FSPostBatch fsPostBatchRow, ref string inRefNbr, ref string inDocType)
        {
            if (appointmentInventoryItemInfoRow != null)
            {
                INRegister inRegisterRow;
                #region IN Issue Header
                if (string.IsNullOrEmpty(inRefNbr))
                {
                    inRegisterRow = new INRegister();

                    inRegisterRow.DocType = INDocType.Issue;
                    inRegisterRow.TranDate = documentDate;
                    inRegisterRow.FinPeriodID = documentPeriod;
                    inRegisterRow.TranDesc = appointmentInventoryItemInfoRow.FSAppointmentRow.DocDesc;
                    inRegisterRow.Hold = false;
                    inRegisterRow = graphINIssueEntry.issue.Current = graphINIssueEntry.issue.Insert(inRegisterRow);

                    inRegisterRow = graphINIssueEntry.issue.Update(inRegisterRow);
                }
                else
                {
                    inRegisterRow = graphINIssueEntry.issue.Current = graphINIssueEntry.issue.Search<INRegister.refNbr>(inRefNbr);
                }
                #endregion
                INTran inTranRow;

                inTranRow = new INTran();
                inTranRow.TranType = INTranType.Issue;

                inTranRow = graphINIssueEntry.transactions.Current = graphINIssueEntry.transactions.Insert(inTranRow);

                graphINIssueEntry.transactions.Cache.SetValueExt<INTran.inventoryID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.InventoryID);

                if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
                {
                    graphINIssueEntry.transactions.Cache.SetValueExt<INTran.subItemID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.SubItemID);
                }

                graphINIssueEntry.transactions.Cache.SetValueExt<INTran.siteID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.SiteID);
                graphINIssueEntry.transactions.Cache.SetValueExt<INTran.qty>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.Qty);
                graphINIssueEntry.transactions.Cache.SetValueExt<INTran.unitPrice>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.UnitPrice);
                graphINIssueEntry.transactions.Cache.SetValueExt<INTran.tranAmt>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.TranAmt);
                graphINIssueEntry.transactions.Cache.SetValueExt<INTran.projectID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.ProjectID);
                graphINIssueEntry.transactions.Cache.SetValueExt<INTran.taskID>(inTranRow, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.ProjectTaskID);

                FSxINTran fsxINTranRow = graphINIssueEntry.transactions.Cache.GetExtension<FSxINTran>(graphINIssueEntry.transactions.Current);

                fsxINTranRow.Source = ID.Billing_By.APPOINTMENT;
                fsxINTranRow.SOID = appointmentInventoryItemInfoRow.FSAppointmentRow.SOID;
                fsxINTranRow.BillCustomerID = appointmentInventoryItemInfoRow.FSServiceOrderRow.BillCustomerID;
                fsxINTranRow.CustomerLocationID = appointmentInventoryItemInfoRow.FSServiceOrderRow.BillLocationID;
                fsxINTranRow.AppointmentID = appointmentInventoryItemInfoRow.FSAppointmentRow.AppointmentID;
                fsxINTranRow.AppointmentDate = new DateTime(appointmentInventoryItemInfoRow.FSAppointmentRow.ActualDateTimeBegin.Value.Year,
                                                            appointmentInventoryItemInfoRow.FSAppointmentRow.ActualDateTimeBegin.Value.Month,
                                                            appointmentInventoryItemInfoRow.FSAppointmentRow.ActualDateTimeBegin.Value.Day,
                                                            0,
                                                            0,
                                                            0);

                fsxINTranRow.AppDetID = (int)appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.AppDetID;

                inTranRow = graphINIssueEntry.transactions.Update(inTranRow);

                graphINIssueEntry.Save.Press();

                if (string.IsNullOrEmpty(inRefNbr))
                {
                    inRefNbr = graphINIssueEntry.issue.Current.RefNbr;
                    inDocType = graphINIssueEntry.issue.Current.DocType;
                }

                UpdateIssuePostInfo(graphINIssueEntry, graphUpdatePostBatchMaint, graphPostInfoEntry, fsAppointmentInventoryItemRow, appointmentInventoryItemInfoRow, fsPostBatchRow);
            }
            else
            {
                throw new PXException(TX.Error.NOTHING_TO_BE_POSTED);
            }
        }

        /// <summary>
        /// Update the references in <c>FSPostInfo</c> and <c>FSPostDet</c> when the posting process of every AppointmentInventoryItem is complete in IN.
        /// </summary>
        public virtual void UpdateReceiptPostInfo(INReceiptEntry graphINReceiptEntry, InventoryPostBatchMaint graphInventoryPostBatchMaint, PostInfoEntry graphPostInfoEntry, FSAppointmentDet fsAppointmentInventoryItemRow, SharedClasses.AppointmentInventoryItemInfo appointmentInventoryItemInfoRow, FSPostBatch fsPostBatchRow)
        {
            //Create | Update Post info
            FSPostInfo fsPostInfoRow;
            FSPostDet fsPostDet;

            fsPostBatchRow = graphInventoryPostBatchMaint.BatchRecords.Current = graphInventoryPostBatchMaint.BatchRecords.Search<FSPostBatch.batchID>(fsPostBatchRow.BatchID);

            fsPostInfoRow = PXSelect<FSPostInfo,
                            Where<
                                FSPostInfo.postID, Equal<Required<FSPostInfo.postID>>>>
                            .Select(this, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.PostID);

            if (fsPostInfoRow == null || fsPostInfoRow.PostID == null)
            {
                fsPostInfoRow = new FSPostInfo();
                fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current = graphPostInfoEntry.PostInfoRecords.Insert(fsPostInfoRow);
            }
            else
            {
                fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current = graphPostInfoEntry.PostInfoRecords.Search<FSPostInfo.postID>(fsPostInfoRow.PostID);
            }

            fsPostInfoRow.INPosted = true;
            fsPostInfoRow.INDocType = graphINReceiptEntry.receipt.Current.DocType;
            fsPostInfoRow.INRefNbr = graphINReceiptEntry.receipt.Current.RefNbr;

            foreach (INTran inTranRowLocal in graphINReceiptEntry.transactions.Select())
            {
                FSxINTran fsxINTranRow = graphINReceiptEntry.transactions.Cache.GetExtension<FSxINTran>(inTranRowLocal);

                if (fsxINTranRow != null && appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.AppDetID == fsxINTranRow.AppDetID)
                {
                    fsPostInfoRow.INLineNbr = inTranRowLocal.LineNbr;
                    break;
                }
            }

            fsPostInfoRow.AppointmentID = appointmentInventoryItemInfoRow.FSAppointmentRow.AppointmentID;
            fsPostInfoRow.SOID = appointmentInventoryItemInfoRow.FSAppointmentRow.SOID;

            fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Update(fsPostInfoRow);

            graphPostInfoEntry.Save.Press();
            fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current;

            fsPostDet = new FSPostDet();

            fsPostDet.PostID = fsPostInfoRow.PostID;
            fsPostDet.INPosted = fsPostInfoRow.INPosted;
            fsPostDet.INDocType = fsPostInfoRow.INDocType;
            fsPostDet.INRefNbr = fsPostInfoRow.INRefNbr;
            fsPostDet.INLineNbr = fsPostInfoRow.INLineNbr;

            graphInventoryPostBatchMaint.BatchDetails.Insert(fsPostDet);
            graphInventoryPostBatchMaint.Save.Press();

            fsAppointmentInventoryItemRow.Mem_BatchNbr = fsPostBatchRow.BatchNbr;

            fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current;

            PXUpdate<
                Set<FSAppointmentDet.postID, Required<FSAppointmentDet.postID>>,
            FSAppointmentDet,
            Where<
                FSAppointmentDet.appDetID, Equal<Required<FSAppointmentDet.appDetID>>>>
            .Update(this, fsPostInfoRow.PostID, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.AppDetID);
        }

        /// <summary>
        /// Update the references in <c>FSPostInfo</c> and <c>FSPostDet</c> when the posting process of every AppointmentInventoryItem is complete in IN.
        /// </summary>
        public virtual void UpdateIssuePostInfo(INIssueEntry graphINIssueEntry, InventoryPostBatchMaint graphInventoryPostBatchMaint, PostInfoEntry graphPostInfoEntry, FSAppointmentDet fsAppointmentInventoryItemRow, SharedClasses.AppointmentInventoryItemInfo appointmentInventoryItemInfoRow, FSPostBatch fsPostBatchRow)
        {
            //Create | Update Post info
            FSPostInfo fsPostInfoRow;
            FSPostDet fsPostDet;

            fsPostBatchRow = graphInventoryPostBatchMaint.BatchRecords.Current = graphInventoryPostBatchMaint.BatchRecords.Search<FSPostBatch.batchID>(fsPostBatchRow.BatchID);

            fsPostInfoRow = PXSelect<FSPostInfo,
                            Where<
                                FSPostInfo.postID, Equal<Required<FSPostInfo.postID>>>>
                            .Select(this, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.PostID);

            if (fsPostInfoRow == null || fsPostInfoRow.PostID == null)
            {
                fsPostInfoRow = new FSPostInfo();
                fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current = graphPostInfoEntry.PostInfoRecords.Insert(fsPostInfoRow);
            }
            else
            {
                fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current = graphPostInfoEntry.PostInfoRecords.Search<FSPostInfo.postID>(fsPostInfoRow.PostID);
            }

            fsPostInfoRow.INPosted = true;
            fsPostInfoRow.INDocType = graphINIssueEntry.issue.Current.DocType;
            fsPostInfoRow.INRefNbr = graphINIssueEntry.issue.Current.RefNbr;

            foreach (INTran inTranRowLocal in graphINIssueEntry.transactions.Select())
            {
                FSxINTran fsxINTranRow = graphINIssueEntry.transactions.Cache.GetExtension<FSxINTran>(inTranRowLocal);

                if (fsxINTranRow != null && appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.AppDetID == fsxINTranRow.AppDetID)
                {
                    fsPostInfoRow.INLineNbr = inTranRowLocal.LineNbr;
                    break;
                }
            }

            fsPostInfoRow.AppointmentID = appointmentInventoryItemInfoRow.FSAppointmentRow.AppointmentID;
            fsPostInfoRow.SOID = appointmentInventoryItemInfoRow.FSAppointmentRow.SOID;

            fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Update(fsPostInfoRow);

            graphPostInfoEntry.Save.Press();
            fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current;

            fsPostDet = new FSPostDet();

            fsPostDet.PostID = fsPostInfoRow.PostID;
            fsPostDet.INPosted = fsPostInfoRow.INPosted;
            fsPostDet.INDocType = fsPostInfoRow.INDocType;
            fsPostDet.INRefNbr = fsPostInfoRow.INRefNbr;
            fsPostDet.INLineNbr = fsPostInfoRow.INLineNbr;

            graphInventoryPostBatchMaint.BatchDetails.Insert(fsPostDet);
            graphInventoryPostBatchMaint.Save.Press();

            fsAppointmentInventoryItemRow.Mem_BatchNbr = fsPostBatchRow.BatchNbr;

            fsPostInfoRow = graphPostInfoEntry.PostInfoRecords.Current;

            PXUpdate<
                Set<FSAppointmentDet.postID, Required<FSAppointmentDet.postID>>,
            FSAppointmentDet,
            Where<
                FSAppointmentDet.appDetID, Equal<Required<FSAppointmentDet.appDetID>>>>
            .Update(this, fsPostInfoRow.PostID, appointmentInventoryItemInfoRow.FSAppointmentInventoryItem.AppDetID);
        }

        #endregion
    }
}
