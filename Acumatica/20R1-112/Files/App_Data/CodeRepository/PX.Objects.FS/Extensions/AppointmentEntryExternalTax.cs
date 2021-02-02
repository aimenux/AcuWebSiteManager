/// <summary>
/// All the commented code is there just to compare future changes on Opportunity / Sales Order external tax calc changes
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TaxProvider;

namespace PX.Objects.FS
{
    public class AppointmentEntryExternalTax : ExternalTax<AppointmentEntry, FSAppointment>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
        }

        public override bool IsExternalTax(string taxZoneID)
        {
            if (Base.ServiceOrderTypeSelected.Current != null && Base.ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
                return false;

            return IsExternalTax(Base, taxZoneID);
        }

        public override void Initialize()
        {
            base.Initialize();
            Base.menuActions.AddMenuAction(recalcExternalTax);
        }

        public virtual void PublicSkipTaxCalcAndSave()
        {
            SkipTaxCalcAndSave();
        }

        public bool SkipExternalTaxCalcOnSave
        {
            get
            {
                return skipExternalTaxCalcOnSave;
            }
            set
            {
                skipExternalTaxCalcOnSave = value;
            }
        }

        public override FSAppointment CalculateExternalTax(FSAppointment fsAppointment)
        {
            if (IsExternalTax(fsAppointment.TaxZoneID) && skipExternalTaxCalcOnSave == false)
                return CalculateExternalTax(fsAppointment, Base.ServiceOrderRelated.Current, false);

            return fsAppointment;
        }

        public FSAppointment CalculateExternalTax(FSAppointment fsAppointment, FSServiceOrder order, bool forceRecalculate)
        {
            var toAddress = GetToAddress(order);

            var service = TaxProviderFactory(Base, fsAppointment.TaxZoneID);

            GetTaxRequest getRequest = null;
            GetTaxRequest getRequestOpen = null;
            GetTaxRequest getRequestUnbilled = null;
            GetTaxRequest getRequestFreight = null;

            bool isValidByDefault = false;

            FSSrvOrdType srvOrdType = PXSelect<FSSrvOrdType, Where<FSSrvOrdType.srvOrdType, Equal<Required<FSAppointment.srvOrdType>>>>.Select(this.Base, fsAppointment.SrvOrdType);

            if (/*srvOrdType.INDocType != INTranType.Transfer &&*/ !IsNonTaxable(toAddress))
            {
                if (fsAppointment.IsTaxValid != true || forceRecalculate)
                {
                    getRequest = BuildGetTaxRequest(fsAppointment, order);

                    if (getRequest.CartItems.Count > 0)
                    {
                        isValidByDefault = false;
                    }
                    else
                    {
                        getRequest = null;
                    }
                }

                /*if (order.IsOpenTaxValid != true || forceRecalculate)
                {
                    getRequestOpen = BuildGetTaxRequestOpen(order);
                    if (getRequestOpen.CartItems.Count > 0)
                    {
                        isValidByDefault = false;
                    }
                    else
                    {
                        getRequestOpen = null;
                    }
                }*/

                /*if (order.IsUnbilledTaxValid != true || forceRecalculate)
                {
                    getRequestUnbilled = BuildGetTaxRequestUnbilled(order);
                    if (getRequestUnbilled.CartItems.Count > 0)
                    {
                        isValidByDefault = false;
                    }
                    else
                    {
                        getRequestUnbilled = null;
                    }
                }*/

                /*if (order.IsFreightTaxValid != true || forceRecalculate)
                {
                    getRequestFreight = BuildGetTaxRequestFreight(order);
                    if (getRequestFreight.CartItems.Count > 0)
                    {
                        isValidByDefault = false;
                    }
                    else
                    {
                        getRequestFreight = null;
                    }
                }*/
            }

            if (isValidByDefault)
            {
                fsAppointment.CuryTaxTotal = 0;
                //order.CuryOpenTaxTotal = 0;
                //order.CuryUnbilledTaxTotal = 0;
                fsAppointment.IsTaxValid = true;
                //order.IsOpenTaxValid = true;
                //order.IsUnbilledTaxValid = true;
                //order.IsFreightTaxValid = true;

                Base.AppointmentRecords.Update(fsAppointment);

                foreach (FSAppointmentTaxTran item in Base.Taxes.Select())
                {
                    Base.Taxes.Delete(item);
                }

                using (var ts = new PXTransactionScope())
                {
                    Base.Persist(typeof(FSAppointmentTaxTran), PXDBOperation.Delete);
                    Base.Persist(typeof(FSAppointment), PXDBOperation.Update);
                    PXTimeStampScope.PutPersisted(Base.AppointmentRecords.Cache, fsAppointment, PXDatabase.SelectTimeStamp());
                    ts.Complete();
                }
                return fsAppointment;
            }

            GetTaxResult result = null;
            GetTaxResult resultOpen = null;
            GetTaxResult resultUnbilled = null;
            GetTaxResult resultFreight = null;

            bool getTaxFailed = false;
            if (getRequest != null)
            {
                result = service.GetTax(getRequest);

                if (!result.IsSuccess)
                {
                    getTaxFailed = true;
                }
            }
            if (getRequestOpen != null)
            {
                if (getRequest != null && IsSame(getRequest, getRequestOpen))
                {
                    resultOpen = result;
                }
                else
                {
                    resultOpen = service.GetTax(getRequestOpen);

                    if (!resultOpen.IsSuccess)
                    {
                        getTaxFailed = true;
                    }
                }
            }
            if (getRequestUnbilled != null)
            {
                if (getRequest != null && IsSame(getRequest, getRequestUnbilled))
                {
                    resultUnbilled = result;
                }
                else
                {
                    resultUnbilled = service.GetTax(getRequestUnbilled);
                    
                    if (!resultUnbilled.IsSuccess)
                    {
                        getTaxFailed = true;
                    }
                }
            }
            if (getRequestFreight != null)
            {
                resultFreight = service.GetTax(getRequestFreight);
                
                if (!resultFreight.IsSuccess)
                {
                    getTaxFailed = true;
                }
            }

            if (!getTaxFailed)
            {
                try
                {
                    ApplyTax(fsAppointment, result, resultOpen, resultUnbilled, resultFreight);
                }
                catch (PXOuterException ex)
                {
                    string msg = PX.Objects.TX.Messages.FailedToApplyTaxes;
                    foreach (string err in ex.InnerMessages)
                    {
                        msg += Environment.NewLine + err;
                    }

                    throw new PXException(ex, msg);
                }
                catch (Exception ex)
                {
                    string msg = PX.Objects.TX.Messages.FailedToApplyTaxes;
                    msg += Environment.NewLine + ex.Message;

                    throw new PXException(ex, msg);
                }
            }
            else
            {
                ResultBase taxResult = result ?? resultOpen ?? resultUnbilled ?? resultFreight;
                if (taxResult != null)
                    LogMessages(taxResult);

                throw new PXException(PX.Objects.TX.Messages.FailedToGetTaxes);
            }

            return fsAppointment;
        }

        [PXOverride]
        public virtual void RecalculateExternalTaxes()
        {
            if (Base.AppointmentRecords.Current != null && IsExternalTax(Base.AppointmentRecords.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && /*!Base.IsTransferOrder &&*/
                (Base.AppointmentRecords.Current.IsTaxValid != true /*|| Base.AppointmentRecords.Current.IsOpenTaxValid != true || Base.AppointmentRecords.Current.IsUnbilledTaxValid != true*/)
            )
            {
                if (Base.RecalculateExternalTaxesSync)
                {
                    FSAppointment doc = new FSAppointment();
                    doc.SrvOrdType = Base.AppointmentRecords.Current.SrvOrdType;
                    doc.RefNbr = Base.AppointmentRecords.Current.RefNbr;

                    AppointmentExternalTaxCalc.Process(doc);
                }
                else
                {
                    Debug.Print("{0} SOExternalTaxCalc.Process(doc) Async", DateTime.Now.TimeOfDay);
                    PXLongOperation.StartOperation(Base, delegate ()
                    {
                        Debug.Print("{0} Inside PXLongOperation.StartOperation", DateTime.Now.TimeOfDay);
                        FSAppointment doc = new FSAppointment();
                        doc.SrvOrdType = Base.AppointmentRecords.Current.SrvOrdType;
                        doc.RefNbr = Base.AppointmentRecords.Current.RefNbr;
                        AppointmentExternalTaxCalc.Process(doc);
                    });
                }
            }
        }

        public PXAction<FSAppointment> recalcExternalTax;
        [PXUIField(DisplayName = "Recalculate External Tax", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable RecalcExternalTax(PXAdapter adapter)
        {
            if (Base.AppointmentRecords.Current != null && IsExternalTax(Base.AppointmentRecords.Current.TaxZoneID))
            {
                var order = Base.AppointmentRecords.Current;
                CalculateExternalTax(Base.AppointmentRecords.Current, Base.ServiceOrderRelated.Current, true);

                Base.Clear(PXClearOption.ClearAll);
                Base.AppointmentRecords.Current = Base.AppointmentRecords.Search<FSAppointment.refNbr>(order.RefNbr, order.SrvOrdType);

                yield return Base.AppointmentRecords.Current;
            }
            else
            {
                foreach (var res in adapter.Get())
                {
                    yield return res;
                }
            }
        }

        protected virtual void _(Events.RowSelected<FSAppointment> e)
        {
            if (e.Row == null)
                return;

            var isExternalTax = IsExternalTax(e.Row.TaxZoneID);
            bool runningFromExternalControls = Base.Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.WEB_METHOD);

            if (isExternalTax == true && ((FSAppointment)e.Row).IsTaxValid != true
                && runningFromExternalControls == false
            )
            {
                PXUIFieldAttribute.SetWarning<FSAppointment.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
            }/*
            else if (isExternalTax == true && ((FSAppointment)e.Row).IsFreightTaxValid != true && !Base.IsTransferOrder)
                PXUIFieldAttribute.SetWarning<FSAppointment.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);

            if (isExternalTax == true && ((FSAppointment)e.Row).IsOpenTaxValid != true && !Base.IsTransferOrder)
                PXUIFieldAttribute.SetWarning<FSAppointment.curyOpenTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);

            if (isExternalTax == true && ((FSAppointment)e.Row).IsUnbilledTaxValid != true && !Base.IsTransferOrder)
            {
                PXUIFieldAttribute.SetWarning<FSAppointment.curyUnbilledTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
                PXUIFieldAttribute.SetWarning<FSAppointment.curyUnbilledOrderTotal>(e.Cache, e.Row, PX.Objects.SO.Messages.UnbilledBalanceWithoutTaxTaxIsNotUptodate);
            }*/


            Base.Taxes.Cache.AllowInsert = !isExternalTax;
            Base.Taxes.Cache.AllowUpdate = !isExternalTax;
            Base.Taxes.Cache.AllowDelete = !isExternalTax;
        }

        protected virtual void _(Events.RowUpdated<FSAppointment> e)
        {
            if (e.Row == null)
                return;


            //if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
            if (IsExternalTax(e.Row.TaxZoneID)
                && (!e.Cache.ObjectsEqual<FSAppointment.scheduledDateTimeBegin,
                                          FSAppointment.taxZoneID,
                                          FSAppointment.billCustomerID,
                                          FSAppointment.branchID>(e.Row, e.OldRow)))
            {
                e.Row.IsTaxValid = false;
                //e.Row.IsOpenTaxValid = false;
                //e.Row.IsUnbilledTaxValid = false;

                /*if (!e.Cache.ObjectsEqual<FSAppointmentDet.openAmt>(e.Row, e.OldRow))
                {
                    e.Row.IsOpenTaxValid = false;
                }*/

                /*if (!e.Cache.ObjectsEqual<FSAppointmentDet.unbilledAmt>(e.Row, e.OldRow))
                {
                    e.Row.IsUnbilledTaxValid = false;
                }*/

                /*if (!e.Cache.ObjectsEqual<FSAppointment.curyFreightTot, FSAppointment.freightTaxCategoryID>(e.OldRow, e.Row))
                {
                    e.Row.IsFreightTaxValid = false;
                    e.Row.IsTaxValid = false;
                    e.Row.IsOpenTaxValid = false;
                    e.Row.IsUnbilledTaxValid = false;
                }*/
            }
        }

        protected virtual void _(Events.RowUpdated<FSServiceOrder> e)
        {
            //if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
            if (IsExternalTax(e.Row.TaxZoneID)
                & (!e.Cache.ObjectsEqual<FSServiceOrder.branchLocationID,
                                         FSServiceOrder.billCustomerID,
                                         FSServiceOrder.serviceOrderAddressID,
                                         FSServiceOrder.billLocationID>(e.Row, e.OldRow)))
            {
                Base.AppointmentRecords.Current.IsTaxValid = false;
            }
        }

        protected virtual void _(Events.RowInserted<FSAppointmentDet> e)
        {
            if (e.Cache.Graph.IsCopyPasteContext)
            {
                //if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
                InvalidateExternalTax(Base.AppointmentRecords.Current);
            }
        }

        protected virtual void _(Events.RowUpdated<FSAppointmentDet> e)
        {
            //if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
            if (Base.AppointmentRecords.Current != null && IsExternalTax(Base.AppointmentRecords.Current.TaxZoneID))
            {
                if (!e.Cache.ObjectsEqual<
                        FSAppointmentDet.acctID,
                        FSAppointmentDet.inventoryID,
                        FSAppointmentDet.tranDesc,
                        FSAppointmentDet.curyBillableTranAmt,
                        FSAppointmentDet.tranDate,
                        FSAppointmentDet.taxCategoryID,
                        FSAppointmentDet.siteID
                    >(e.Row, e.OldRow))
                {
                    InvalidateExternalTax(Base.AppointmentRecords.Current);
                }

                /*if (!e.Cache.ObjectsEqual<FSAppointmentDet.openAmt>(e.Row, e.OldRow))
                {
                    Base.AppointmentRecords.Current.IsOpenTaxValid = false;
                }*/

                /*if (!e.Cache.ObjectsEqual<FSAppointmentDet.unbilledAmt>(e.Row, e.OldRow))
                {
                    Base.AppointmentRecords.Current.IsUnbilledTaxValid = false;
                }*/
            }
        }

        protected virtual void _(Events.RowDeleted<FSAppointmentDet> e)
        {
            InvalidateExternalTax(Base.AppointmentRecords.Current);
        }

        #region FSAddress Events
        protected virtual void _(Events.RowUpdated<FSAddress> e)
        {
            if (e.Row == null) return;
            if (e.Cache.ObjectsEqual<FSAddress.postalCode, FSAddress.countryID, FSAddress.state>(e.Row, e.OldRow) == false)
                InvalidateExternalTax(Base.AppointmentRecords.Current);
        }

        protected virtual void _(Events.RowInserted<FSAddress> e)
        {
            if (e.Row == null) return;
            InvalidateExternalTax(Base.AppointmentRecords.Current);
        }

        protected virtual void _(Events.RowDeleted<FSAddress> e)
        {
            if (e.Row == null) return;
            InvalidateExternalTax(Base.AppointmentRecords.Current);
        }

        protected virtual void _(Events.FieldUpdating<FSAddress, FSAddress.overrideAddress> e)
        {
            if (e.Row == null) return;
            InvalidateExternalTax(Base.AppointmentRecords.Current);
        }
        #endregion

        protected virtual GetTaxRequest BuildGetTaxRequest(FSAppointment fsAppointment, FSServiceOrder order)
        {
            if (fsAppointment == null)
                throw new PXArgumentException(nameof(fsAppointment));


            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { fsAppointment });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { fsAppointment });

            IAddressBase fromAddress = GetFromAddress(order);
            IAddressBase toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetFromAddressCR);

            if (toAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetToAddressCR);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(fsAppointment.TaxZoneID, fsAppointment.BranchID);
            request.CurrencyCode = fsAppointment.CuryID;
            request.CustomerCode = cust.AcctCD;
            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = string.Format("SO.{0}.{1}", fsAppointment.SrvOrdType, fsAppointment.RefNbr);
            request.DocDate = fsAppointment.ScheduledDateTimeBegin.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(fsAppointment);

            Sign sign = Sign.Plus;

            request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
            if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { fsAppointment });

            /*if (srvOrdType.DefaultOperation == SOOperation.Receipt)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                sign = Sign.Minus;

                PXSelectBase<FSAppointmentDet> selectLineWithInvoiceDate = new PXSelect<FSAppointmentDet,
                Where<FSAppointmentDet.srvOrdType, Equal<Required<FSAppointmentDet.srvOrdType>>, And<FSAppointmentDet.refNbr, Equal<Required<FSAppointmentDet.refNbr>>,
                And<FSAppointmentDet.invoiceDate, IsNotNull>>>>(Base);

                FSAppointmentDet soLine = selectLineWithInvoiceDate.SelectSingle(order.SrvOrdType, order.RefNbr);
                if (soLine != null && soLine.TranDate != null)
                {
                    request.TaxOverride.Reason = PX.Objects.SO.Messages.ReturnReason;
                    request.TaxOverride.TaxDate = soLine.TranDate.Value;
                    request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
                }

            }
            else
            {*/
            request.DocType = TaxDocumentType.SalesOrder;
            /*}*/


            /* We need InnerJoin with InventoryItem instead of LeftJoin */
            /* because of instructions and comments lines */
            PXSelectBase<FSAppointmentDet> select = new PXSelectJoin<FSAppointmentDet,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<FSAppointmentDet.acctID>>>>,
                Where<FSAppointmentDet.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>,
                    And<FSAppointmentDet.refNbr, Equal<Current<FSAppointment.refNbr>>>>,
                OrderBy<Asc<FSAppointmentDet.srvOrdType, Asc<FSAppointmentDet.refNbr, Asc<FSAppointmentDet.lineNbr>>>>>(Base);

            request.Discount = fsAppointment.CuryDiscTot.GetValueOrDefault();

            foreach (PXResult<FSAppointmentDet, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { fsAppointment }))
            {
                FSAppointmentDet tran = (FSAppointmentDet)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;

                var line = new TaxCartItem();
                line.Index = tran.LineNbr ?? 0;

                /*if (srvOrdType.DefaultOperation != tran.Operation)
                    line.Amount = Sign.Minus * sign * tran.CuryLineAmt.GetValueOrDefault();
                else
                    line.Amount = sign * tran.CuryLineAmt.GetValueOrDefault();*/
                line.Amount = sign * tran.CuryBillableTranAmt.GetValueOrDefault();

                line.Description = tran.TranDesc;
                line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
                line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
                line.ItemCode = item.InventoryCD;
                line.Quantity = Math.Abs(tran.Qty.GetValueOrDefault());
                line.Discounted = request.Discount > 0;
                line.RevAcct = salesAccount.AccountCD;

                line.TaxCode = tran.TaxCategoryID;

                request.CartItems.Add(line);
            }

            return request;
        }

        /*protected virtual GetTaxRequest BuildGetTaxRequestOpen(FSAppointment order)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (order == null)
                throw new PXArgumentException(ErrorMessages.ArgumentNullException);

            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { order });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { order });

            IAddressBase fromAddress = GetFromAddress(order);
            IAddressBase toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(Messages.FailedGetFromAddressSO);

            if (toAddress == null)
                throw new PXException(Messages.FailedGetToAddressSO);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
            request.CurrencyCode = order.CuryID;
            request.CustomerCode = cust.AcctCD;
            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = string.Format("SO.{0}.{1}", order.SrvOrdType, order.RefNbr);
            request.DocDate = order.ScheduledDateTimeBegin.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(order);

            int mult = 1;

            if (!string.IsNullOrEmpty(loc.CAvalaraCustomerUsageType))
            {
                request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
            }
            if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order });

            if (srvOrdType.DefaultOperation == SOOperation.Receipt)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                mult = -1;

                PXSelectBase<FSAppointmentDet> selectLineWithInvoiceDate = new PXSelect<FSAppointmentDet,
                Where<FSAppointmentDet.srvOrdType, Equal<Required<FSAppointmentDet.srvOrdType>>, And<FSAppointmentDet.refNbr, Equal<Required<FSAppointmentDet.refNbr>>,
                And<FSAppointmentDet.invoiceDate, IsNotNull>>>>(Base);

                FSAppointmentDet soLine = selectLineWithInvoiceDate.SelectSingle(order.SrvOrdType, order.RefNbr);
                if (soLine != null && soLine.TranDate != null)
                {
                    request.TaxOverride.Reason = Messages.ReturnReason;
                    request.TaxOverride.TaxDate = soLine.TranDate.Value;
                    request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
                }

            }
            else
            {
                request.DocType = TaxDocumentType.SalesOrder;
            }
            request.DocType = TaxDocumentType.SalesOrder;


            PXSelectBase<FSAppointmentDet> select = new PXSelectJoin<FSAppointmentDet,
                LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<FSAppointmentDet.acctID>>>>,
                Where<FSAppointmentDet.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>,
                    And<FSAppointmentDet.refNbr, Equal<Current<FSAppointment.refNbr>>>>,
                OrderBy<Asc<FSAppointmentDet.srvOrdType, Asc<FSAppointmentDet.refNbr, Asc<FSAppointmentDet.lineNbr>>>>>(Base);

            request.Discount = order.CuryDiscTot.GetValueOrDefault();

            foreach (PXResult<FSAppointmentDet, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
            {
                FSAppointmentDet tran = (FSAppointmentDet)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;

                if (tran.OpenAmt >= 0)
                {
                    var line = new TaxCartItem();
                    line.Index = tran.LineNbr ?? 0;
                    if (srvOrdType.DefaultOperation != tran.Operation)
                        line.Amount = -1 * mult * tran.CuryOpenAmt.GetValueOrDefault();
                    else
                        line.Amount = mult * tran.CuryOpenAmt.GetValueOrDefault();
                    line.Description = tran.TranDesc;
                    line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
                    line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
                    line.ItemCode = item.InventoryCD;
                    line.Quantity = Math.Abs(tran.OpenQty.GetValueOrDefault());
                    line.Discounted = request.Discount > 0;
                    line.RevAcct = salesAccount.AccountCD;

                    line.TaxCode = tran.TaxCategoryID;

                    request.CartItems.Add(line);
                }
            }

            sw.Stop();
            Debug.Print("BuildGetTaxRequestOpen() in {0} millisec.", sw.ElapsedMilliseconds);

            return request;
        }
        */
        /*protected virtual GetTaxRequest BuildGetTaxRequestUnbilled(FSAppointment order)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (order == null)
                throw new PXArgumentException(ErrorMessages.ArgumentNullException);

            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { order });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { order });

            IAddressBase fromAddress = GetFromAddress(order);
            IAddressBase toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(Messages.FailedGetFromAddressSO);

            if (toAddress == null)
                throw new PXException(Messages.FailedGetToAddressSO);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
            request.CurrencyCode = order.CuryID;
            request.CustomerCode = cust.AcctCD;
            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = string.Format("{0}.{1}.Open", order.SrvOrdType, order.RefNbr);
            request.DocDate = order.ScheduledDateTimeBegin.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(order);

            int mult = 1;

            if (!string.IsNullOrEmpty(order.AvalaraCustomerUsageType))
            {
                request.CustomerUsageType = order.AvalaraCustomerUsageType;
            }
            if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order });

            if (srvOrdType.DefaultOperation == SOOperation.Receipt)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                mult = -1;

                PXSelectBase<FSAppointmentDet> selectLineWithInvoiceDate = new PXSelect<FSAppointmentDet,
                Where<FSAppointmentDet.srvOrdType, Equal<Required<FSAppointmentDet.srvOrdType>>, And<FSAppointmentDet.refNbr, Equal<Required<FSAppointmentDet.refNbr>>,
                And<FSAppointmentDet.invoiceDate, IsNotNull>>>>(Base);

                FSAppointmentDet soLine = selectLineWithInvoiceDate.SelectSingle(order.SrvOrdType, order.RefNbr);
                if (soLine != null && soLine.TranDate != null)
                {
                    request.TaxOverride.Reason = Messages.ReturnReason;
                    request.TaxOverride.TaxDate = soLine.TranDate.Value;
                    request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
                }

            }
            else
            {
                request.DocType = TaxDocumentType.SalesOrder;
            }


            PXSelectBase<FSAppointmentDet> select = new PXSelectJoin<FSAppointmentDet,
                LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<FSAppointmentDet.salesAcctID>>>>,
                Where<FSAppointmentDet.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>,
                    And<FSAppointmentDet.refNbr, Equal<Current<FSAppointment.refNbr>>>>,
                OrderBy<Asc<FSAppointmentDet.srvOrdType, Asc<FSAppointmentDet.refNbr, Asc<FSAppointmentDet.lineNbr>>>>>(Base);

            request.Discount = order.CuryDiscTot.GetValueOrDefault();

            foreach (PXResult<FSAppointmentDet, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
            {
                FSAppointmentDet tran = (FSAppointmentDet)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;

                if (tran.UnbilledAmt >= 0)
                {
                    var line = new TaxCartItem();
                    line.Index = tran.LineNbr ?? 0;
                    if (srvOrdType.DefaultOperation != tran.Operation)
                        line.Amount = -1 * mult * tran.CuryUnbilledAmt.GetValueOrDefault();
                    else
                        line.Amount = mult * tran.CuryUnbilledAmt.GetValueOrDefault();
                    line.Description = tran.TranDesc;
                    line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
                    line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
                    line.ItemCode = item.InventoryCD;
                    line.Quantity = Math.Abs(tran.UnbilledQty.GetValueOrDefault());
                    line.Discounted = request.Discount > 0;
                    line.RevAcct = salesAccount.AccountCD;

                    line.TaxCode = tran.TaxCategoryID;

                    request.CartItems.Add(line);
                }
            }

            sw.Stop();
            Debug.Print("BuildGetTaxRequestUnbilled() in {0} millisec.", sw.ElapsedMilliseconds);

            return request;
        }
        */
        /*protected virtual GetTaxRequest BuildGetTaxRequestFreight(FSAppointment order)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (order == null)
                throw new PXArgumentException(ErrorMessages.ArgumentNullException);

            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { order });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { order });

            IAddressBase fromAddress = GetFromAddress(order);
            IAddressBase toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetFromAddressCR);

            if (toAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetToAddressCR);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
            request.CurrencyCode = order.CuryID;
            request.CustomerCode = cust.AcctCD;
            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = $"{order.SrvOrdType}.{order.RefNbr}.Freight";
            request.DocDate = order.ScheduledDateTimeBegin.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(order);

            int mult = 1;

            if (!string.IsNullOrEmpty(loc.CAvalaraCustomerUsageType))
            {
                request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
            }
            if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order });

            if (srvOrdType.ARDocType == ARDocType.CreditMemo)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                mult = -1;
            }
            else
            {
                request.DocType = TaxDocumentType.SalesOrder;
            }

            if (order.CuryFreightTot > 0)
            {
                var line = new TaxCartItem();
                line.Index = short.MaxValue;
                line.Amount = mult * order.CuryFreightTot.GetValueOrDefault();
                line.Description = PXMessages.LocalizeNoPrefix(Messages.FreightDesc);
                line.DestinationAddress = request.DestinationAddress;
                line.OriginAddress = request.OriginAddress;
                line.ItemCode = "N/A";
                line.Discounted = false;
                line.TaxCode = order.FreightTaxCategoryID;

                request.CartItems.Add(line);
            }

            sw.Stop();
            Debug.Print("BuildGetTaxRequestFreight() in {0} millisec.", sw.ElapsedMilliseconds);

            return request;
        }
        */
        protected virtual void ApplyTax(FSAppointment order, GetTaxResult result, GetTaxResult resultOpen, GetTaxResult resultUnbilled, GetTaxResult resultFreight)
        {

            TaxZone taxZone = (TaxZone)Base.TaxZone.View.SelectSingleBound(new object[] { order });
            if (taxZone == null)
            {
                throw new PXException(PX.Objects.SO.Messages.TaxZoneIsNotSet);
            }

            AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>>.Select(Base, taxZone.TaxVendorID);

            if (vendor == null)
                throw new PXException(PX.Objects.CR.Messages.ExternalTaxVendorNotFound);

            /*var sign = ((FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order })).DefaultOperation == SOOperation.Receipt
                ? Sign.Minus
                : Sign.Plus;*/
            var sign = Sign.Plus;

            if (result != null)
            {
                //Clear all existing Tax transactions:
                foreach (PXResult<FSAppointmentTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { order }))
                {
                    FSAppointmentTaxTran taxTran = res;
                    Base.Taxes.Delete(taxTran);
                }

                Base.Views.Caches.Add(typeof(Tax));

                decimal freightTax = 0;
                if (resultFreight != null)
                    freightTax = sign * resultFreight.TotalTaxAmount;

                //bool requireControlTotal = Base.ServiceOrderTypeSelected.Current.RequireControlTotal == true;
                /*if (order.Hold != true)
                    Base.ServiceOrderTypeSelected.Current.RequireControlTotal = false;*/

                var taxDetails = new List<PX.TaxProvider.TaxDetail>();
                foreach (TaxProvider.TaxDetail tax in result.TaxSummary.OrderByDescending(e => e.TaxAmount))
                {
                    if (tax.TaxAmount != 0
                        || taxDetails.Find(e => e.TaxName == tax.TaxName) == default(TaxProvider.TaxDetail))
                    {
                        taxDetails.Add(tax);
                    }
                }

                if (resultFreight != null)
                {
                    foreach (TaxProvider.TaxDetail tax in resultFreight.TaxSummary.OrderByDescending(e => e.TaxAmount))
                    {
                        if (tax.TaxAmount != 0
                            || taxDetails.Find(e => e.TaxName == tax.TaxName) == default(TaxProvider.TaxDetail))
                        {
                            taxDetails.Add(tax);
                        }
                    }
                }

                try
                {
                    foreach (var taxDetail in taxDetails)
                    {
                        string taxID = taxDetail.TaxName;

                        if (string.IsNullOrEmpty(taxID))
                            taxID = taxDetail.JurisCode;

                        if (string.IsNullOrEmpty(taxID))
                        {
                            PXTrace.WriteInformation(PX.Objects.SO.Messages.EmptyValuesFromExternalTaxProvider);
                            continue;
                        }
                        
                        CreateTax(Base, taxZone, vendor, taxDetail, taxID);

                        FSAppointmentTaxTran tax = (FSAppointmentTaxTran)Base.Taxes.Cache.CreateInstance(); ;
                        
                        tax.TaxID = taxID;
                        tax.CuryTaxAmt = Math.Abs(taxDetail.TaxAmount);
                        tax.CuryTaxableAmt = Math.Abs(taxDetail.TaxableAmount);
                        tax.TaxRate = Convert.ToDecimal(taxDetail.Rate) * 100;
                        tax.JurisType = taxDetail.JurisType;
                        tax.JurisName = taxDetail.JurisName;

                        Base.Taxes.Insert(tax);
                    }

                    Base.AppointmentSelected.SetValueExt<FSAppointment.curyTaxTotal>(order, sign * result.TotalTaxAmount + freightTax);

                    decimal? СuryDocTotal = AppointmentEntry.GetCuryDocTotal(order.CuryBillableLineTotal, order.CuryLogBillableTranAmountTotal, order.CuryDiscTot, order.CuryTaxTotal, 0);
                    Base.AppointmentSelected.SetValueExt<FSAppointment.curyDocTotal>(order, СuryDocTotal ?? 0m);
                }
                finally
                {
                    //Base.ServiceOrderTypeSelected.Current.RequireControlTotal = requireControlTotal;
                }
            }


            /*if (resultUnbilled != null)
                Base.AppointmentRecords.SetValueExt<FSAppointment.curyUnbilledTaxTotal>(order, sign * resultUnbilled.TotalTaxAmount);

            if (resultOpen != null)
                Base.AppointmentRecords.SetValueExt<FSAppointment.curyOpenTaxTotal>(order, sign * resultOpen.TotalTaxAmount);*/

            order = (FSAppointment)Base.AppointmentSelected.Cache.CreateCopy(order);
            order.IsTaxValid = true;
            Base.AppointmentSelected.Cache.Update(order);

            if (Base.TimeStamp == null)
            {
                Base.SelectTimeStamp();
            }

            SkipTaxCalcAndSave();
        }

        protected virtual bool IsSame(GetTaxRequest x, GetTaxRequest y)
        {
            if (x.CartItems.Count != y.CartItems.Count)
                return false;

            for (int i = 0; i < x.CartItems.Count; i++)
            {
                if (x.CartItems[i].Amount != y.CartItems[i].Amount)
                    return false;
            }

            return true;
        }

		protected override string GetExternalTaxProviderLocationCode(FSAppointment fsAppointment) => GetExternalTaxProviderLocationCode<FSAppointmentDet, FSAppointmentDet.FK.Appointment.SameAsCurrent, FSAppointmentDet.siteID>(fsAppointment);

		protected virtual IAddressBase GetFromAddress(FSServiceOrder order)
        {
            FSAddress returnAdrress = PXSelectJoin<FSAddress,
                                        InnerJoin<
                                            FSBranchLocation,
                                            On<FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>>,
                                        Where<
                                            FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                            .Select(Base, order.BranchLocationID)
                                            .RowCast<FSAddress>()
                                            .FirstOrDefault();

            return returnAdrress;
        }

        protected virtual IAddressBase GetFromAddress(FSServiceOrder order, FSAppointmentDet line)
        {
            IAddressBase returnAddress = null;

            if (line.SiteID != null)
            {
                returnAddress = PXSelectJoin<Address,
                                InnerJoin<INSite, On<Address.addressID, Equal<INSite.addressID>>>,
                                Where<
                                    INSite.siteID, Equal<Required<INSite.siteID>>>>
                                .Select(Base, line.SiteID)
                                .RowCast<Address>()
                                .FirstOrDefault();
            }
            
            if(returnAddress == null)
            {
                returnAddress = GetFromAddress(order);
            }

            return returnAddress;
        }

        protected virtual IAddressBase GetToAddress(FSServiceOrder order)
        {
            return ((FSAddress)PXSelect<FSAddress, Where<FSAddress.addressID, Equal<Required<FSServiceOrder.serviceOrderAddressID>>>>.Select(Base, order.ServiceOrderAddressID)).With(ValidAddressFrom<FSServiceOrder.serviceOrderAddressID>);
        }

        protected virtual IAddressBase GetToAddress(FSServiceOrder order, FSAppointmentDet line)
        {
            /*if (order.WillCall == true && line.SiteID != null && !(line.POCreate == true && line.POSource == INReplenishmentSource.DropShipToOrder))
                return GetFromAddress(order, line); // will call
            else*/
            return GetToAddress(order);
        }

        private IAddressBase ValidAddressFrom<TFieldSource>(IAddressBase address)
            where TFieldSource : IBqlField
        {
            if (!IsEmptyAddress(address)) return address;
            throw new PXException(PickAddressError<TFieldSource>(address));
        }

        private string PickAddressError<TFieldSource>(IAddressBase address)
            where TFieldSource : IBqlField
        {
            if (typeof(TFieldSource) == typeof(FSServiceOrder.serviceOrderAddressID))
                return PXSelectReadonly<FSServiceOrder, Where<FSServiceOrder.serviceOrderAddressID, Equal<Required<FSAddress.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((FSAddress)address).AddressID).First().GetItem<FSAppointment>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<FSServiceOrder>(), new EntityHelper(Base).GetRowID(e)));

            if (typeof(TFieldSource) == typeof(Vendor.defLocationID))
                return PXSelectReadonly<Vendor, Where<Vendor.defLocationID, Equal<Required<Address.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<Vendor>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<Vendor>(), new EntityHelper(Base).GetRowID(e)));

            if (typeof(TFieldSource) == typeof(INSite.addressID))
                return PXSelectReadonly<INSite, Where<INSite.addressID, Equal<Required<Address.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<INSite>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<INSite>(), new EntityHelper(Base).GetRowID(e)));

            if (typeof(TFieldSource) == typeof(BAccountR.defAddressID))
                return PXSelectReadonly<BAccountR, Where<BAccountR.defAddressID, Equal<Required<Address.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<BAccountR>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<BAccountR>(), new EntityHelper(Base).GetRowID(e)));

            throw new ArgumentOutOfRangeException("Unknown address source used");
        }

        protected virtual bool IsCommonCarrier(string carrierID)
        {
            if (string.IsNullOrEmpty(carrierID))
            {
                return false; //pickup;
            }
            else
            {
                Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<Carrier.carrierID>>>>.Select(Base, carrierID);
                if (carrier == null)
                {
                    return false;
                }
                else
                {
                    return carrier.IsCommonCarrier == true;
                }
            }
        }

        private void InvalidateExternalTax(FSAppointment order, bool keepFreight = false)
        {
            if (order == null || !IsExternalTax(order.TaxZoneID)) return;
            order.IsTaxValid = false;
            /*order.IsOpenTaxValid = false;
            order.IsUnbilledTaxValid = false;
            if (keepFreight == false)
                order.IsFreightTaxValid = false;*/
        }
    }
}
