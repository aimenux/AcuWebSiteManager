using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.TX;

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
            typeof(FSAppointment.CustomerSignaturePath),
            typeof(FSAppointment.serviceContractID),
            typeof(FSAppointment.scheduleID),
            typeof(FSAppointment.logLineCntr))]
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
        public PXSelect<CurrencyInfo,
               Where<CurrencyInfo.curyInfoID, Equal<Current<FSAppointment.curyInfoID>>>> currencyInfoView;

        [PXFilterable]
        [PXImport(typeof(FSAppointment))]
        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_DETAILS)]
        [PXCopyPasteHiddenFields(typeof(FSAppointmentDet.sODetID), 
                                 typeof(FSAppointmentDet.lotSerialNbr),
                                 typeof(FSAppointmentDet.status))]
        public AppointmentCore.AppointmentDetails_View AppointmentDetails;

        [PXFilterable]
        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_EMPLOYEES)]
        [PXCopyPasteHiddenFields(
            typeof(FSAppointmentEmployee.lineNbr),
            typeof(FSAppointmentEmployee.lineRef),
            typeof(FSAppointmentEmployee.serviceLineRef))]
        public AppointmentCore.AppointmentServiceEmployees_View AppointmentServiceEmployees;

        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_RESOURCES)]
        public AppointmentCore.AppointmentResources_View AppointmentResources;

        [PXCopyPasteHiddenView()]
        public PXSelectReadonly2<ARPayment,
               InnerJoin<FSAdjust,
               On<
                   ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                   And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>>,
               Where<
                   FSAdjust.adjdOrderType, Equal<Current<FSServiceOrder.srvOrdType>>,
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

        [PXFilterable]
        [PXImport(typeof(FSAppointment))]
        [PXCopyPasteHiddenView]
        public AppointmentCore.AppointmentLog_View LogRecords;

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
                appointmentRow.CuryDocTotal = GetCuryDocTotal(appointmentRow.CuryBillableLineTotal, appointmentRow.CuryLogBillableTranAmountTotal, appointmentRow.CuryDiscTot,
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
        [PXCopyPasteHiddenView]
        public PXSelect<FSAppointmentTax,
               Where<
                   FSAppointmentTax.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>,
                   And<FSAppointmentTax.refNbr, Equal<Current<FSAppointment.refNbr>>>>,
               OrderBy<
                   Asc<FSAppointmentTax.taxID>>> TaxLines;

        [PXViewName(TX.Messages.AppointmentTax)]
        [PXCopyPasteHiddenView]
        public PXSelectJoin<FSAppointmentTaxTran,
               InnerJoin<Tax,
                   On<Tax.taxID, Equal<FSAppointmentTaxTran.taxID>>>,
               Where<
                   FSAppointmentTaxTran.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>,
                   And<FSAppointmentTaxTran.refNbr, Equal<Current<FSAppointment.refNbr>>>>,
               OrderBy<
                   Asc<FSAppointmentTaxTran.taxID,
                   Asc<FSAppointmentTaxTran.recordID>>>> Taxes;
        #endregion

        #region Events to initialize ServiceOrderRelated

        protected virtual void _(Events.RowInserted<FSAppointment> e)
        {
            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            PXCache cache = e.Cache;

            if (fsAppointmentRow == null || string.IsNullOrEmpty(fsAppointmentRow.SrvOrdType))
            {
                return;
            }

            InitServiceOrderRelated(fsAppointmentRow);
            SharedFunctions.InitializeNote(cache, e.Args);
        }

        protected virtual void _(Events.RowUpdated<FSServiceOrder> e)
        {

        }

        protected virtual void _(Events.RowUpdated<FSAppointment> e)
        {
            if (!e.Cache.ObjectsEqual<FSAppointment.srvOrdType>(e.Row, e.OldRow) && ((FSAppointment)e.OldRow).SrvOrdType == null)
            {
                InitServiceOrderRelated((FSAppointment)e.Row);
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
        }

        protected virtual void _(Events.RowDeleted<FSAppointment> e)
        {
            DeleteUnpersistedServiceOrderRelated((FSAppointment)e.Row);
        }
        #endregion

        protected virtual void _(Events.RowPersisted<FSAppointment> e)
        {
            if (e.TranStatus == PXTranStatus.Aborted)
            {
                serviceOrderRowPersistedPassedWithStatusAbort = true;
            }
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.billCustomerID> e)
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

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.branchID> e)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            var fsServiceOrderRow = (FSServiceOrder)e.Row;

            AppointmentSelected.Cache.SetValueExt<FSAppointment.branchID>(AppointmentSelected.Current, fsServiceOrderRow.BranchID);
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.billLocationID> e)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            AppointmentSelected.Cache.SetDefaultExt<FSAppointment.taxZoneID>(AppointmentSelected.Current);
            AppointmentSelected.Cache.SetDefaultExt<FSAppointment.taxCalcMode>(AppointmentSelected.Current);
        }

        protected virtual void _(Events.RowSelected<FSAppointment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            LoadServiceOrderRelated(fsAppointmentRow);
        }

        #region Unify Tabs

        protected virtual void _(Events.RowSelecting<FSAppointmentDet> e)
        {
            if (e.Row == null)
            {
                return;
            }

            var fsAppointmentDetURow = (FSAppointmentDet)e.Row;

            // FSAppointmentDet.BillingRule is an Unbound field so we need to calculate it
            if (fsAppointmentDetURow.IsService == true)
            {
                if (fsAppointmentDetURow.LineType == ID.LineType_ALL.NONSTOCKITEM)
                {
                    fsAppointmentDetURow.BillingRule = ID.BillingRule.FLAT_RATE;
                }
                else if (fsAppointmentDetURow.SODetID > 0)
                {
                    using (new PXConnectionScope())
                    {
                        var fsSODetRow = PXSelectReadonly<FSSODet,
                                         Where<
                                             FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                                         .Select(e.Cache.Graph, fsAppointmentDetURow.SODetID);

                        fsAppointmentDetURow.BillingRule = ((FSSODet)fsSODetRow)?.BillingRule;
                    }
                }
            }
            else if (fsAppointmentDetURow.IsInventoryItem == true || fsAppointmentDetURow.IsPickupDelivery == true)
            {
                fsAppointmentDetURow.BillingRule = ID.BillingRule.FLAT_RATE;
            }
            else
            {
                fsAppointmentDetURow.BillingRule = ID.BillingRule.NONE;
            }

            // IsFree is an Unbound field so we need to calculate it
            fsAppointmentDetURow.IsFree = ServiceOrderAppointmentHandlers.IsFree(fsAppointmentDetURow.BillingRule, fsAppointmentDetURow.ManualPrice, fsAppointmentDetURow.LineType);

            if (fsAppointmentDetURow.IsInventoryItem)
            {
                using (new PXConnectionScope())
                {
                    SharedFunctions.SetInventoryItemExtensionInfo(this, fsAppointmentDetURow.InventoryID, fsAppointmentDetURow);
                }
            }
        }

        protected virtual void _(Events.RowDeleted<FSAppointmentDet> e)
        {
            ClearTaxes(AppointmentSelected.Current);
        }

        #endregion

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

        protected virtual void VerifyIsAlreadyPosted<Field>(PXCache cache, FSAppointmentDet fsAppointmentDetRow, FSBillingCycle billingCycleRow)
            where Field : class, IBqlField
        {
            if (fsAppointmentDetRow == null || ServiceOrderRelated.Current == null || billingCycleRow == null)
            {
                return;
            }

            IFSSODetBase row = null;
            int? pivot = -1;

            if (fsAppointmentDetRow.IsInventoryItem)
            {
                row = fsAppointmentDetRow;
                pivot = fsAppointmentDetRow.SODetID;
            }
            else if (fsAppointmentDetRow.IsPickupDelivery)
            {
                row = fsAppointmentDetRow;
                pivot = fsAppointmentDetRow.AppDetID > 0 ? fsAppointmentDetRow.AppDetID : null;
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
                                                    new PXSetPropertyException(PXMessages.LocalizeFormat(TX.Error.CANNOT_ADD_INVENTORY_TYPE_LINES_BECAUSE_SO_POSTED, SharedFunctions.GetLineType(row.LineType, true)),
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

            return row.LineType == ID.LineType_ALL.COMMENT
                || row.LineType == ID.LineType_ALL.INSTRUCTION;
        }
        #endregion

        public static decimal GetCuryDocTotal(decimal? curyLineTotal, decimal? curyLogBillableTranAmountTotal, decimal? curyDiscTotal, decimal? curyTaxTotal, decimal? curyInclTaxTotal)
        {
            return (curyLineTotal ?? 0) + (curyLogBillableTranAmountTotal ?? 0) - (curyDiscTotal ?? 0) + (curyTaxTotal ?? 0) - (curyInclTaxTotal ?? 0);
        }
    }
}
