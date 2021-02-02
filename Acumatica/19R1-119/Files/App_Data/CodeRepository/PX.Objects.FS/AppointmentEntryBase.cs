using Autofac;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.GL;
using PX.Objects.TX;
using System;
using System.Collections.Generic;
using System.Web.Compilation;
using System.Linq;

namespace PX.Objects.FS
{
    public class AppointmentEntryBase<TGraph> : PXGraph<TGraph, FSAppointment>
        where TGraph : PXGraph<TGraph, FSAppointment>
    {
        protected bool serviceOrderIsAlreadyUpdated = false;
        protected bool serviceOrderRowPersistedPassedWithStatusAbort = false;
        protected bool insertingServiceOrder = false;
        public bool recalculateCuryID = false;

        public bool RecalculateExternalTaxesSync { get; set; }
        protected virtual void RecalculateExternalTaxes()
        {
        }

        #region Selects / Views

        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        [PXHidden]
        public PXSelect<FSRouteSetup> RouteSetupRecord;

        [PXHidden]
        public PXSelect<BAccount> BAccounts;

        [PXHidden]
        public PXSelect<BAccountSelectorBase> BAccountSelectorBaseView;
        [PXHidden]
        public PXSelect<FSSODet> FSSODets;

        [PXHidden]
        public PXSelect<Vendor> VendorBaseView;
        

        [PXHidden]
        public PXSelect<Vendor> Vendors;

        [PXHidden]
        public PXSetup<FSSelectorHelper> Helper;


        [PXCopyPasteHiddenFields(typeof(FSAppointment.soRefNbr))]
        public AppointmentCore.AppointmentRecords_View AppointmentRecords;

        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_SELECTED)]
        [PXCopyPasteHiddenFields(
            typeof(FSAppointment.soRefNbr),
            typeof(FSAppointment.fullNameSignature),
            typeof(FSAppointment.additionalCommentsCustomer),
            typeof(FSAppointment.additionalCommentsStaff),            
            typeof(FSAppointment.agreementSignature),
            typeof(FSAppointment.CustomerSignaturePath),
            typeof(FSAppointment.serviceContractID),
            typeof(FSAppointment.scheduleID))]
        public AppointmentCore.AppointmentSelected_View AppointmentSelected;

        [PXViewName(TX.FriendlyViewName.Common.SERVICEORDERTYPE_SELECTED)]
        public PXSetup<FSSrvOrdType>.Where<
                    Where<
                        FSSrvOrdType.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>>> ServiceOrderTypeSelected;

        [PXHidden]
        public PXSetup<FSBranchLocation>.Where<
                Where<
                    FSBranchLocation.branchLocationID, Equal<Current<FSServiceOrder.branchLocationID>>>> CurrentBranchLocation;

        [PXHidden]
        public PXSetup<Customer>.Where<
        Where<
            Customer.bAccountID, Equal<Optional<FSServiceOrder.billCustomerID>>>> TaxCustomer;

        [PXHidden]
        public PXSetup<Location>.Where<
        Where<
            Location.bAccountID, Equal<Current<FSServiceOrder.billCustomerID>>,
            And<Location.locationID, Equal<Optional<FSServiceOrder.billLocationID>>>>> TaxLocation;

        [PXHidden]
        public PXSetup<TaxZone>.Where<
        Where<TaxZone.taxZoneID, Equal<Current<FSAppointment.taxZoneID>>>> TaxZone;

        [PXViewName(TX.FriendlyViewName.Appointment.SERVICEORDER_RELATED)]
        public AppointmentCore.ServiceOrderRelated_View ServiceOrderRelated;

        [PXViewName(TX.TableName.FSCONTACT)]
        public ServiceOrderCore.FSContact_View ServiceOrder_Contact;

        [PXViewName(TX.TableName.FSADDRESS)]
        public ServiceOrderCore.FSAddress_View ServiceOrder_Address;

        [PXHidden]
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<FSAppointment.curyInfoID>>>> currencyInfoView;

        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_DET_SERVICES)]
        [PXCopyPasteHiddenFields(typeof(FSAppointmentDetService.sODetID), typeof(FSAppointmentDetService.lotSerialNbr))]
        public AppointmentCore.AppointmentDetServices_View AppointmentDetServices;

        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_DET_PARTS)]
        [PXCopyPasteHiddenFields(typeof(FSAppointmentDetPart.sODetID), typeof(FSAppointmentDetPart.lotSerialNbr))]
        public AppointmentCore.AppointmentDetParts_View AppointmentDetParts;

        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_EMPLOYEES)]
        [PXCopyPasteHiddenFields(
            typeof(FSAppointmentEmployee.lineNbr),
            typeof(FSAppointmentEmployee.lineRef),
            typeof(FSAppointmentEmployee.serviceLineRef))]
        public AppointmentCore.AppointmentEmployees_View AppointmentEmployees;

        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_RESOURCES)]
        public AppointmentCore.AppointmentResources_View AppointmentResources;

        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_ATTENDEES)]
        public AppointmentCore.AppointmentAttendees_View AppointmentAttendees;

        [PXCopyPasteHiddenView]
        [PXViewName(TX.FriendlyViewName.Appointment.PICKUP_DELIVERY_ITEMS)]
        public AppointmentCore.AppointmentPickupDeliveryItems_View PickupDeliveryItems;

        [PXHidden]
        public PXSelect<FSAppointmentDetUNION, Where<FSAppointmentDetUNION.appointmentID, Equal<Current<FSAppointment.appointmentID>>>> TaxSourceLines;

        [PXCopyPasteHiddenView()]
        public PXSelectReadonly2<ARPayment,
                    InnerJoin<FSAdjust,
                        On<ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                            And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
                    Where<FSAdjust.adjdOrderType, Equal<Current<FSServiceOrder.srvOrdType>>,
                        And<FSAdjust.adjdOrderNbr, Equal<Current<FSServiceOrder.refNbr>>>>> Adjustments;

        [PXCopyPasteHiddenView]
        public PXSelectJoinGroupBy<FSPostDet,
                InnerJoin<FSSODet,
                    On<FSSODet.postID, Equal<FSPostDet.postID>>,
                InnerJoin<FSPostBatch,
                    On<FSPostBatch.batchID, Equal<FSPostDet.batchID>>>>,
                Where<
                    FSSODet.sOID, Equal<Current<FSAppointment.sOID>>>,
                Aggregate<
                    GroupBy<FSPostDet.batchID,
                    GroupBy<FSPostDet.aRPosted,
                    GroupBy<FSPostDet.aPPosted,
                    GroupBy<FSPostDet.iNPosted,
                    GroupBy<FSPostDet.sOPosted>>>>>>> ServiceOrderPostedIn;
        #endregion

        #region Avalara Tax


        #region External Tax Provider

        public virtual bool IsExternalTax(string taxZoneID)
        {
            return false;
        }

        public virtual FSAppointment CalculateExternalTax(FSAppointment fsAppointmentRow)
        {
            return fsAppointmentRow;
        }
        #endregion


        public void ClearTaxes(FSAppointment appointmentRow)
        {
            if (appointmentRow == null)
                return;

            if (IsExternalTax(appointmentRow.TaxZoneID))
            {
                foreach (PXResult<FSAppointmentTaxTran, Tax> res in Taxes.View.SelectMultiBound(new object[] { appointmentRow }))
                {
                    FSAppointmentTaxTran taxTran = (FSAppointmentTaxTran)res;
                    Taxes.Delete(taxTran);
                }

                appointmentRow.CuryTaxTotal = 0;
                appointmentRow.CuryDocTotal = GetCuryDocTotal(appointmentRow.CuryBillableLineTotal, appointmentRow.CuryDiscTot,
                                                0, 0);
            }
        }
        #endregion
        #region Overrides
        public virtual void MyPersist()
        {
            serviceOrderIsAlreadyUpdated = false;
            serviceOrderRowPersistedPassedWithStatusAbort = false;
            insertingServiceOrder = false;

            try
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    try
                    {
                        base.Persist(typeof(FSServiceOrder), PXDBOperation.Insert);
                        base.Persist(typeof(FSServiceOrder), PXDBOperation.Update);
                    }
                    catch
                    {
                        Caches[typeof(FSServiceOrder)].Persisted(true);
                        throw;
                    }

                    try
                    {
                        if (RecalculateExternalTaxesSync)
                        {
                            RecalculateExternalTaxes();
                            this.SelectTimeStamp();
                        }

                        base.Persist();

                        if (!RecalculateExternalTaxesSync) //When the calling process is the 'UI' thread.
                            RecalculateExternalTaxes();
                    }
                    catch
                    {
                        if (serviceOrderRowPersistedPassedWithStatusAbort == false)
                        {
                            Caches[typeof(FSServiceOrder)].Persisted(true);
                        }

                        throw;
                    }

                    ts.Complete();
                }
            }
            finally
            {
                serviceOrderIsAlreadyUpdated = false;
                serviceOrderRowPersistedPassedWithStatusAbort = false;
                insertingServiceOrder = false;
            }
        }

        public override void Persist()
        {
            MyPersist();
        }
        #endregion

        #region Tax Extension Views
        public PXSelect<FSAppointmentTax,
                Where<
                    FSAppointmentTax.entityID, Equal<Current<FSAppointment.appointmentID>>,
                    And<FSAppointmentTax.lineNbr, Less<intMax>>>,
                OrderBy<
                    Asc<FSAppointmentTax.taxID>>> TaxLines;

        [PXViewName(TX.Messages.AppointmentTax)]
        public PXSelectJoin<FSAppointmentTaxTran,
                InnerJoin<Tax,
                    On<Tax.taxID, Equal<FSAppointmentTaxTran.taxID>>>,
                Where<
                    FSAppointmentTaxTran.entityID, Equal<Current<FSAppointment.appointmentID>>>,
                OrderBy<
                    Asc<FSAppointmentTaxTran.lineNbr,
                    Asc<FSAppointmentTaxTran.taxID>>>> Taxes;
        #endregion

        #region FSAppointmentDetUNION events and sync methods
        private FSAppointmentDetUNION CreateTaxRow(FSAppointmentDet baseRow)
        {
            var taxRow = new FSAppointmentDetUNION()
            {
                SrvOrdType = baseRow.SrvOrdType,
                RefNbr = baseRow.RefNbr,
                AppointmentID = baseRow.AppointmentID,
                ApptLineNbr = baseRow.ApptLineNbr,
                SODetID = baseRow.SODetID,
                LineRef = baseRow.LineRef,
                AppDetID = baseRow.AppDetID,

                CuryInfoID = baseRow.CuryInfoID,
                TaxCategoryID = baseRow.TaxCategoryID,
                GroupDiscountRate = baseRow.GroupDiscountRate,
                SiteID = baseRow.SiteID,
                SiteLocationID = baseRow.SiteLocationID,
                DocumentDiscountRate = baseRow.DocumentDiscountRate,
                CuryEstimatedTranAmt = baseRow.CuryEstimatedTranAmt,
                CuryBillableTranAmt = baseRow.CuryBillableTranAmt,
            };

            return taxRow;
        }

        protected virtual void _(Events.RowPersisting<FSAppointmentDetUNION> e)
        {
            e.Cancel = true;
        }
        #endregion

        #region Events to initialize ServiceOrderRelated
        protected virtual void FSAppointment_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            if (fsAppointmentRow == null || string.IsNullOrEmpty(fsAppointmentRow.SrvOrdType))
            {
                return;
            }

            InitServiceOrderRelated(fsAppointmentRow);
            SharedFunctions.InitializeNote(cache, e);
        }

        protected virtual void FSAppointmentDetService_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            TaxSourceLines.Insert(CreateTaxRow((FSAppointmentDet)e.Row));
        }

        protected virtual void FSAppointmentDetPart_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            TaxSourceLines.Insert(CreateTaxRow((FSAppointmentDet)e.Row));
        }

        protected virtual void FSAppointmentInventoryItem_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            TaxSourceLines.Insert(CreateTaxRow((FSAppointmentDet)e.Row));
        }

        protected virtual void FSAppointmentDetService_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            TaxSourceLines.Update(CreateTaxRow((FSAppointmentDet)e.Row));
        }

        protected virtual void FSAppointmentDetPart_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            TaxSourceLines.Update(CreateTaxRow((FSAppointmentDet)e.Row));
        }

        protected virtual void FSAppointmentInventoryItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            TaxSourceLines.Update(CreateTaxRow((FSAppointmentDet)e.Row));
        }

        protected virtual void FSAppointmentDetService_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            TaxSourceLines.Delete(CreateTaxRow((FSAppointmentDet)e.Row));
            ClearTaxes(AppointmentSelected.Current);
        }

        protected virtual void FSAppointmentDetPart_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            TaxSourceLines.Delete(CreateTaxRow((FSAppointmentDet)e.Row));
            ClearTaxes(AppointmentSelected.Current);
        }

        protected virtual void FSAppointmentInventoryItem_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            TaxSourceLines.Delete(CreateTaxRow((FSAppointmentDet)e.Row));
            ClearTaxes(AppointmentSelected.Current);
        }

        protected virtual void FSServiceOrder_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {

        }

        protected virtual void FSAppointment_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (!cache.ObjectsEqual<FSAppointment.srvOrdType>(e.Row, e.OldRow) && ((FSAppointment)e.OldRow).SrvOrdType == null)
            {
                InitServiceOrderRelated((FSAppointment)e.Row);
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

           
        }

        protected virtual void FSAppointment_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            DeleteUnpersistedServiceOrderRelated((FSAppointment)e.Row);
        }
        #endregion

        protected virtual void FSServiceOrder_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.TranStatus == PXTranStatus.Aborted)
            {
                serviceOrderRowPersistedPassedWithStatusAbort = true;
            }
        }

        protected virtual void FSServiceOrder_BillCustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            var fsServiceOrderRow = (FSServiceOrder)e.Row;

            try
            {
                if (e.ExternalCall == true)
                    this.recalculateCuryID = true;

                AppointmentSelected.Cache.SetValueExt<FSAppointment.billCustomerID>(AppointmentSelected.Current, fsServiceOrderRow.BillCustomerID);
            }
            finally
            {
                this.recalculateCuryID = false;
            }
        }

        protected virtual void FSServiceOrder_BranchID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            var fsServiceOrderRow = (FSServiceOrder)e.Row;

            AppointmentSelected.Cache.SetValueExt<FSAppointment.branchID>(AppointmentSelected.Current, fsServiceOrderRow.BranchID);
        }

        protected virtual void FSServiceOrder_BillLocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            AppointmentSelected.Cache.SetDefaultExt<FSAppointment.taxZoneID>(AppointmentSelected.Current);
        }

        protected virtual void FSAppointment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            LoadServiceOrderRelated(fsAppointmentRow);
        }

        protected virtual void FSAppointmentDetService_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            // FSAppointmentDet.BillingRule is an Unbound field so we need to calculate it
            if (fsAppointmentDetServiceRow.LineType == ID.LineType_All.SERVICE)
            {
                if (fsAppointmentDetServiceRow.SODetID > 0)
                {
                    using (new PXConnectionScope())
                    {
                        var fsSODetRow = (FSSODet)PXSelectReadonly<FSSODet,
                                        Where<
                                            FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                                        .Select(cache.Graph, fsAppointmentDetServiceRow.SODetID);

                        fsAppointmentDetServiceRow.BillingRule = fsSODetRow?.BillingRule;
                    }
                }
            }
            else if(fsAppointmentDetServiceRow.LineType == ID.LineType_All.NONSTOCKITEM)
            {
                fsAppointmentDetServiceRow.BillingRule = ID.BillingRule.FLAT_RATE;
            }
            else
            {
                fsAppointmentDetServiceRow.BillingRule = ID.BillingRule.NONE;
            }

            // IsFree is an Unbound field so we need to calculate it
            fsAppointmentDetServiceRow.IsFree = ServiceOrderAppointmentHandlers.IsFree(fsAppointmentDetServiceRow.BillingRule, fsAppointmentDetServiceRow.ManualPrice, fsAppointmentDetServiceRow.LineType);
        }

        protected virtual void FSAppointmentDetPart_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;

            // FSAppointmentDet.BillingRule is an Unbound field so we need to calculate it
            if (fsAppointmentDetPartRow.LineType == ID.LineType_All.INVENTORY_ITEM)
            {
                fsAppointmentDetPartRow.BillingRule = ID.BillingRule.FLAT_RATE;
            }
            else
            {
                fsAppointmentDetPartRow.BillingRule = ID.BillingRule.NONE;
            }

            // IsFree is an Unbound field so we need to calculate it
            fsAppointmentDetPartRow.IsFree = ServiceOrderAppointmentHandlers.IsFree(fsAppointmentDetPartRow.BillingRule, fsAppointmentDetPartRow.ManualPrice, fsAppointmentDetPartRow.LineType);

            using (new PXConnectionScope())
            {
                SharedFunctions.SetInventoryItemExtensionInfo(this, fsAppointmentDetPartRow.InventoryID, fsAppointmentDetPartRow);
            }
        }

        protected virtual void FSAppointmentInventoryItem_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var fsAppointmentInventoryItemRow = (FSAppointmentInventoryItem)e.Row;

            // FSAppointmentDet.BillingRule is an Unbound field so we need to calculate it
            fsAppointmentInventoryItemRow.BillingRule = ID.BillingRule.FLAT_RATE;

            // IsFree is an Unbound field so we need to calculate it
            fsAppointmentInventoryItemRow.IsFree = ServiceOrderAppointmentHandlers.IsFree(fsAppointmentInventoryItemRow.BillingRule, fsAppointmentInventoryItemRow.ManualPrice, fsAppointmentInventoryItemRow.LineType);
        }

        #region Protected methods
        protected void InitServiceOrderRelated(FSAppointment fsAppointmentRow)
        {
            // Inserting FSServiceOrder record
            if (fsAppointmentRow.SOID == null)
            {
                var oldServiceOrderDirty = ServiceOrderRelated.Cache.IsDirty;

                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)ServiceOrderRelated.Cache.CreateInstance();
                fsServiceOrderRow.SrvOrdType = fsAppointmentRow.SrvOrdType;
                fsServiceOrderRow.DocDesc = fsAppointmentRow.DocDesc;
                fsServiceOrderRow = ServiceOrderRelated.Insert(fsServiceOrderRow);
                fsAppointmentRow.SOID = fsServiceOrderRow.SOID;

                ServiceOrderRelated.Cache.IsDirty = oldServiceOrderDirty;
            }
            else
            {
                LoadServiceOrderRelated(fsAppointmentRow);
            }
        }

        protected void DeleteUnpersistedServiceOrderRelated(FSAppointment fsAppointmentRow)
        {
            // Deleting unpersisted FSServiceOrder record
            if (fsAppointmentRow.SOID < 0)
            {
                FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.SelectSingle();
                ServiceOrderRelated.Delete(fsServiceOrderRow);
                fsAppointmentRow.SOID = null;
            }
        }

        protected virtual void LoadServiceOrderRelated(FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentRow.SrvOrdType != null && fsAppointmentRow.SOID != null &&
                (ServiceOrderRelated.Current == null 
                    || (ServiceOrderRelated.Current.SOID != fsAppointmentRow.SOID
                            && fsAppointmentRow.SOID > 0)
                )
               )
            {
                ServiceOrderRelated.Current = ServiceOrderRelated.SelectSingle(fsAppointmentRow.SOID);
                fsAppointmentRow.BillCustomerID = ServiceOrderRelated.Current?.BillCustomerID;
                fsAppointmentRow.BranchID = ServiceOrderRelated.Current?.BranchID;
                fsAppointmentRow.CuryID = ServiceOrderRelated.Current?.CuryID;
            }
        }

        protected virtual void VerifyIsAlreadyPosted<Field>(PXCache cache, FSAppointmentDet fsAppointmentDetRow, FSAppointmentInventoryItem fsAppointmentInventoryItemRow, FSBillingCycle billingCycleRow)
            where Field : class, IBqlField
        {
            if ((fsAppointmentDetRow == null && fsAppointmentInventoryItemRow == null) || ServiceOrderRelated.Current == null || billingCycleRow == null)
            {
                return;
            }

            IFSSODetBase row = null;
            int? pivot = -1;

            if (fsAppointmentDetRow != null)
            {
                row = fsAppointmentDetRow;
                pivot = fsAppointmentDetRow.SODetID;
            }
            else if(fsAppointmentInventoryItemRow != null)
            {
                row = fsAppointmentInventoryItemRow;
                pivot = fsAppointmentInventoryItemRow.AppDetID > 0 ? fsAppointmentInventoryItemRow.AppDetID : null;
            }

            PXEntryStatus status = ServiceOrderRelated.Cache.GetStatus(ServiceOrderRelated.Current);
            bool needsVerify = status == PXEntryStatus.Updated || status == PXEntryStatus.Notchanged;
            bool isSOAlreadyPosted = ServiceOrderPostedIn.Select().Count > 0;

            if (needsVerify == true
                    && pivot == null
                        && IsInstructionOrComment(row) == false
                            && billingCycleRow.BillingBy == ID.Billing_By.SERVICE_ORDER
                                && isSOAlreadyPosted == true)
            {
                cache.RaiseExceptionHandling<Field>(row,
                    row.InventoryID,
                    new PXSetPropertyException(
                        PXMessages.LocalizeFormat(TX.Error.CANNOT_ADD_INVENTORY_TYPE_LINES_BECAUSE_SO_POSTED, GetLineType(row.LineType)),
                        PXErrorLevel.RowError));
            }
        }

        protected virtual bool IsInstructionOrComment(object eRow)
        {
            if (eRow == null)
            {
                return false;
            }

            var row = (IFSSODetBase)eRow;

            return row.LineType == ID.LineType_All.COMMENT_PART
                || row.LineType == ID.LineType_All.COMMENT_SERVICE
                || row.LineType == ID.LineType_All.INSTRUCTION_PART
                || row.LineType == ID.LineType_All.INSTRUCTION_SERVICE;
        }

        protected virtual string GetLineType(string lineType)
        {
            switch (lineType)
            {
                case ID.LineType_All.SERVICE: return TX.LineType_ALL.SERVICE.ToLower();
                case ID.LineType_All.NONSTOCKITEM: return TX.LineType_ALL.NONSTOCKITEM.ToLower();
                case ID.LineType_All.INVENTORY_ITEM: return TX.LineType_ALL.INVENTORY_ITEM.ToLower();
                case ID.LineType_All.PICKUP_DELIVERY: return TX.LineType_ALL.PICKUP_DELIVERY.ToLower();
            }

            return TX.LineType_ALL.INVENTORY_ITEM.ToLower();
        }
        #endregion

        public static decimal GetCuryDocTotal(decimal? curyLineTotal, decimal? curyDiscTotal,
                                                decimal? curyTaxTotal, decimal? curyInclTaxTotal)
        {
            return (curyLineTotal ?? 0) - (curyDiscTotal ?? 0) + (curyTaxTotal ?? 0) - (curyInclTaxTotal ?? 0);
        }
    }
}
