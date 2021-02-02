using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.CacheExtensions;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.AM.ScanLabor, PX.Objects.AM.ScanLaborHost, PX.Objects.AM.AMBatch, PX.Objects.AM.ScanLabor.Header>;

namespace PX.Objects.AM
{
    public class ScanLaborHost : LaborEntry
    {
        public override Type PrimaryItemType => typeof(ScanLabor.Header);
        public PXFilter<ScanLabor.Header> HeaderView;
    }

    public class ScanLabor : WMSBase
    {
        public class UserSetup : PXUserSetupPerMode<UserSetup, ScanLaborHost, Header, AMScanUserSetup, AMScanUserSetup.userID, AMScanUserSetup.mode, Modes.scanLabor> { }

        #region DACs
        [PXCacheName("MFG Header")]
        [Serializable]
        public class Header : WMSHeader, ILSMaster
        {
            #region BatNbr
            [PXUnboundDefault(typeof(AMBatch.batNbr))]
            [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
            [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<AMDocType.labor>>>))]
            public override string RefNbr { get; set; }
            public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
            #endregion
            #region TranDate
            [PXDate]
            [PXUnboundDefault(typeof(AccessInfo.businessDate))]
            public virtual DateTime? TranDate { get; set; }
            public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
            #endregion
            #region SiteID
            [Site]
            public virtual int? SiteID { get; set; }
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
            #endregion
            #region LocationID
            [Location]
            public virtual int? LocationID { get; set; }
            public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
            #endregion
            #region InventoryID
            public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            #endregion
            #region SubItemID
            public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
            #endregion
            #region LotSerialNbr
            //[AMLotSerialNbr(typeof(inventoryID), typeof(subItemID), typeof(locationID), PersistingCheck = PXPersistingCheck.Nothing)]
            [PXString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
            [PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
            [PXUnboundDefault("")]
            public virtual string LotSerialNbr { get; set; }
            public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
            #endregion
            #region ExpirationDate
            [INExpireDate(typeof(inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual DateTime? ExpireDate { get; set; }
            public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
            #endregion

            #region LotSerTrack
            [PXString(1, IsFixed = true)]
            public virtual String LotSerTrack { get; set; }
            public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
            #endregion
            #region LotSerAssign
            [PXString(1, IsFixed = true)]
            public virtual String LotSerAssign { get; set; }
            public abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }
            #endregion
            #region LotSerTrackExpiration
            [PXBool]
            public virtual Boolean? LotSerTrackExpiration { get; set; }
            public abstract class lotSerTrackExpiration : PX.Data.BQL.BqlBool.Field<lotSerTrackExpiration> { }
            #endregion
            #region AutoNextNbr
            [PXBool]
            public virtual Boolean? AutoNextNbr { get; set; }
            public abstract class autoNextNbr : PX.Data.BQL.BqlBool.Field<autoNextNbr> { }
            #endregion
            #region OrderType
            public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

            protected String _OrderType;
            [PXString(2, IsFixed = true, InputMask = ">aa")]
            [PXUIField(DisplayName = "Order Type")]
            [PXUnboundDefault(typeof(AMPSetup.defaultOrderType))]
            public virtual String OrderType
            {
                get
                {
                    return this._OrderType;
                }
                set
                {
                    this._OrderType = value;
                }
            }
            #endregion
            #region ProdOrdID
            public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

            protected String _ProdOrdID;
            [PXUnboundDefault]
            [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "Production Nbr", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String ProdOrdID
            {
                get
                {
                    return this._ProdOrdID;
                }
                set
                {
                    this._ProdOrdID = value;
                }
            }
            #endregion
            #region OperationID
            public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

            protected int? _OperationID;
            [PXInt]
            [PXUIField(DisplayName = "Operation ID")]
            [PXUnboundDefault(typeof(Search<
                AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<prodOrdID>>>>,
                OrderBy<
                    Asc<AMProdOper.operationCD>>>))]
            [PXSelector(typeof(Search<AMProdOper.operationID,
                    Where<AMProdOper.orderType, Equal<Current<orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Current<prodOrdID>>>>>),
                SubstituteKey = typeof(AMProdOper.operationCD))]
            [PXFormula(typeof(Validate<AMMTran.prodOrdID>))]
            public virtual int? OperationID
            {
                get
                {
                    return this._OperationID;
                }
                set
                {
                    this._OperationID = value;
                }
            }
            #endregion
            #region LastOperationID
            public abstract class lastOperationID : PX.Data.BQL.BqlInt.Field<lastOperationID> { }

            protected int? _LastOperationID;
            [OperationIDField(DisplayName = "Last Operation ID")]
            [PXSelector(typeof(Search<AMProdOper.operationID,
                    Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>,
                        And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>),
                SubstituteKey = typeof(AMProdOper.operationCD), ValidateValue = false)]
            public virtual int? LastOperationID
            {
                get
                {
                    return this._LastOperationID;
                }
                set
                {
                    this._LastOperationID = value;
                }
            }
            #endregion
            #region EmployeeID
            public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

            protected Int32? _EmployeeID;
            [PXInt]
            [ProductionEmployeeSelector]
            [PXDefault(typeof(Search<EPEmployee.bAccountID,
                    Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
                    And<EPEmployeeExt.amProductionEmployee, Equal<True>,
                    And<Current<AMPSetup.defaultEmployee>, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Employee ID")]
            public virtual Int32? EmployeeID
            {
                get
                {
                    return this._EmployeeID;
                }
                set
                {
                    this._EmployeeID = value;
                }
            }
            #endregion
            #region LaborTime
            public abstract class laborTime : PX.Data.BQL.BqlInt.Field<laborTime> { }

            protected Int32? _LaborTime;
            [PXInt]
            [PXTimeList]
            [PXUIField(DisplayName = "Labor Time")]
            public virtual Int32? LaborTime
            {
                get
                {
                    return this._LaborTime;
                }
                set
                {
                    this._LaborTime = value;
                }
            }
            #endregion
            #region LaborType
            public abstract class laborType : PX.Data.BQL.BqlString.Field<laborType> { }

            protected String _LaborType;
            [PXString(1, IsFixed = true)]
            [AMLaborType.List]
            [PXUnboundDefault(AMLaborType.Direct, PersistingCheck = PXPersistingCheck.NullOrBlank)]
            [PXUIField(DisplayName = "Type")]
            public virtual String LaborType
            {
                get
                {
                    return this._LaborType;
                }
                set
                {
                    this._LaborType = value;
                }
            }
            #endregion
            #region LaborCodeID

            public abstract class laborCodeID : PX.Data.BQL.BqlString.Field<laborCodeID> { }

            protected String _LaborCodeID;
            [PXString(15, InputMask = ">AAAAAAAAAAAAAAA")]
            [PXUIField(DisplayName = "Labor Code")]
            public virtual String LaborCodeID
            {
                get
                {
                    return this._LaborCodeID;
                }
                set
                {
                    this._LaborCodeID = value;
                }
            }
            #endregion
            #region ShiftID
            public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }

            protected String _ShiftID;
            [PXString(4)]
            [PXUnboundDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
            [PXUIField(DisplayName = "Shift")]
            [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
            public virtual String ShiftID
            {
                get
                {
                    return this._ShiftID;
                }
                set
                {
                    this._ShiftID = value;
                }
            }
            #endregion

            #region ILSMaster implementation
            public string TranType => string.Empty;
            public short? InvtMult { get => -1; set { } }
            public int? ProjectID { get; set; }
            public int? TaskID { get; set; }
            #endregion
        }
        #endregion
        #region Views
        public override PXFilter<Header> HeaderView => Base.HeaderView;
        public PXSetupOptional<AMScanSetup, Where<AMScanSetup.branchID, Equal<Current<AccessInfo.branchID>>>> Setup;
        public PXSetupOptional<AMPSetup> AMSetup;
        #endregion
        #region Buttons
        public PXAction<Header> ScanRelease;
        [PXButton, PXUIField(DisplayName = "Release")]
        protected virtual IEnumerable scanRelease(PXAdapter adapter) => scanBarcode(adapter, ScanCommands.Release);

        public PXAction<Header> Review;
        [PXButton, PXUIField(DisplayName = "Review")]
        protected virtual IEnumerable review(PXAdapter adapter) => adapter.Get();
        #endregion

        #region Event Handlers
        protected override void _(Events.RowSelected<Header> e)
        {
            base._(e);

            bool notReleaseAndHasLines = Batch?.Released != true && ((AMMTran)Base.transactions.Select()) != null;
            ScanRemove.SetEnabled(notReleaseAndHasLines);
            ScanRelease.SetEnabled(notReleaseAndHasLines);
            ScanModeInReceive.SetEnabled(e.Row != null && e.Row.Mode != Modes.ScanLabor);
            ScanConfirm.SetEnabled(Batch?.Released != true && e.Row?.ScanState == ScanStates.Confirm);

            Review.SetVisible(Base.IsMobile);

            Logs.AllowInsert = Logs.AllowDelete = Logs.AllowUpdate = false;
            Base.transactions.AllowInsert = false;
            Base.transactions.AllowDelete = Base.transactions.AllowUpdate = (Batch == null || Batch.Released != true);
        }

        protected virtual void _(Events.RowSelected<AMMTran> e)
        {
            bool isMobileAndNotReleased = Base.IsMobile && (Batch == null || Batch.Released != true);

            Base.transactions.Cache
            .Adjust<PXUIFieldAttribute>()
            .For<AMMTran.inventoryID>(ui => ui.Enabled = false)
            .SameFor<AMMTran.tranDesc>()
            .SameFor<AMMTran.qty>()
            .SameFor<AMMTran.uOM>()
            .For<AMMTran.lotSerialNbr>(ui => ui.Enabled = isMobileAndNotReleased)
            .SameFor<AMMTran.expireDate>()
            .SameFor<AMMTran.locationID>();
        }

        protected virtual void _(Events.FieldDefaulting<Header, Header.siteID> e) => e.NewValue = IsWarehouseRequired() ? null : DefaultSiteID;

        protected virtual void _(Events.FieldDefaulting<Header, Header.orderType> e) => e.NewValue = !UseDefaultOrderType() ? null : AMSetup.Current.DefaultOrderType;

        protected virtual void _(Events.FieldDefaulting<Header, Header.qty> e) => e.NewValue = Convert.ToDecimal(0);

        protected virtual void _(Events.RowPersisted<Header> e)
        {
            e.Row.RefNbr = Batch?.BatNbr;
            e.Row.TranDate = Batch?.TranDate;
            e.Row.NoteID = Batch?.NoteID;

            Base.transactions.Cache.Clear();
            Base.transactions.Cache.ClearQueryCache();
        }

        protected virtual void _(Events.FieldUpdated<Header, Header.refNbr> e)
        {
            Base.batch.Current = Base.batch.Search<AMBatch.batNbr>(e.Row.RefNbr, AMDocType.Labor);
        }

        protected virtual void _(Events.RowUpdated<AMScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
        protected virtual void _(Events.RowInserted<AMScanUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
        #endregion

        private AMBatch Batch => Base.batch.Current;
        protected override BqlCommand DocumentSelectCommand()
            => new SelectFrom<AMBatch>.
                Where<AMBatch.docType.IsEqual<AMBatch.docType.AsOptional>
                    .And<AMBatch.batNbr.IsEqual<AMBatch.batNbr.AsOptional>>>();

        protected virtual bool UseDefaultOrderType() => Setup.Current.UseDefaultOrderType == true;
        protected virtual bool IsWarehouseRequired() => UserSetup.For(Base).DefaultWarehouse != true || DefaultSiteID == null;
        protected virtual bool IsLotSerialRequired() => UserSetup.For(Base).DefaultLotSerialNumber != true;
        protected virtual bool IsExpirationDateRequired() => UserSetup.For(Base).DefaultExpireDate != true || EnsureExpireDateDefault() == null;
        protected virtual bool IsLastOperation() => (HeaderView.Current.OperationID == HeaderView.Current.LastOperationID);
        protected virtual bool IsShiftSet => HeaderView.Current.ShiftID != null;

        protected override WMSModeOf<ScanLabor, ScanLaborHost> DefaultMode => Modes.ScanLabor;
        public override string CurrentModeName =>
            HeaderView.Current.Mode == Modes.ScanLabor ? Msg.ScanLaborMode :
            Msg.FreeMode;
        protected override string GetModePrompt()
        {
            var header = HeaderView.Current;
            if (header.Mode == Modes.ScanLabor)
            {
                if (header.LaborCodeID != null)
                {
                    if (header.ShiftID == null)
                        return Localize(Msg.ShiftPrompt);                    
                }
                else
                {
                    if(header.OrderType == null)
                        return Localize(Msg.DefaultPrompt);
                    if(header.ProdOrdID == null)
                        return Localize(Msg.DefaultPromptWithType);
                    if (header.OperationID == null)
                        return Localize(Msg.OperationPrompt);
                    if(IsLastOperation())
                    {
                        if (IsWarehouseRequired() && header.SiteID == null)
                            return Localize(Msg.WarehousePrompt);
                        if (header.LocationID == null)
                            return Localize(Msg.LocationPrompt);
                        if (header.LotSerialNbr == null && IsLotSerialRequired())
                            return Localize(Msg.LotSerialPrompt);
                    }
                }

                if (header.LaborTime == null)
                    return Localize(Msg.LaborTimePrompt);

                return Localize(Msg.ConfirmationPrompt);
            }
            return null;
        }

        protected override bool ProcessCommand(string barcode)
        {
            switch (barcode)
            {
                case ScanCommands.Confirm:
                    if (HeaderView.Current.Remove != true) ProcessConfirm();
                    else ProcessConfirmRemove();
                    return true;

                case ScanCommands.Remove:
                    HeaderView.Current.Remove = true;
                    SetScanState(UseDefaultOrderType() ? ScanStates.ProdOrd : ScanStates.OrderType, Msg.RemoveMode);
                    return true;

                case ScanCommands.OrderType:
                    SetScanState(ScanStates.OrderType);
                    return true;

                case ScanCommands.Release:
                    ProcessRelease();
                    return true;
            }
            return false;
        }

        protected override bool ProcessByState(Header doc)
        {
            if (Batch?.Released == true)
            {
                ClearHeaderInfo();
                HeaderView.Current.RefNbr = null;
                Base.batch.Insert();
            }

            switch (doc.ScanState)
            {
                case ScanStates.Default:
                    ProcessDefault(doc.Barcode);
                    return true;
                case ScanStates.OrderType:
                    ProcessOrderType(doc.Barcode);
                    return true;
                case ScanStates.ProdOrd:
                    ProcessProdOrd(doc.Barcode);
                    return true;
                case ScanStates.Operation:
                    ProcessOperation(doc.Barcode);
                    return true;
                case ScanStates.Warehouse:
                    ProcessWarehouse(doc.Barcode);
                    return true;
                case ScanStates.Item:
                    ProcessItemBarcode(doc.Barcode);
                    return true;
                case ScanStates.Shift:
                    ProcessShift(doc.Barcode);
                    return true;
                case ScanStates.LaborTime:
                    ProcessLaborTime(doc.Barcode);
                    return true;
                case ScanStates.Employee:
                    ProcessEmployee(doc.Barcode);
                    return true;
                default:
                    return base.ProcessByState(doc);
            }
        }

        protected virtual void ProcessDefault(string barcode)
        {
            if (HeaderView.Current.EmployeeID == null && AMSetup.Current.DefaultEmployee == true)
            {
                EPEmployee employee = SelectFrom<EPEmployee>.Where<EPEmployee.userID.IsEqual<AccessInfo.userID>
                    .And<EPEmployeeExt.amProductionEmployee.IsEqual<True>>>.View.Select(Base).First();
                if (employee != null)
                    HeaderView.Current.EmployeeID = employee.BAccountID;
            }

            //Default can be an indirect labor code, order type or prod order ID (if order type already defined)
            AMLaborCode laborCode = PXSelect<AMLaborCode, Where<AMLaborCode.laborCodeID, Equal<Required<AMLaborCode.laborCodeID>>,
                And<AMLaborCode.laborType, Equal<AMLaborType.indirect>>>>.Select(Base, barcode);
            if(laborCode != null)
            {
                HeaderView.Current.LaborType = AMLaborType.Indirect;
                HeaderView.Current.LaborCodeID = laborCode.LaborCodeID;
                HeaderView.Current.OrderType = null; 
                SetScanState(ScanStates.Shift);
                return;
            }
            AMOrderType orderType = PXSelectReadonly<AMOrderType,
                Where<AMOrderType.orderType, Equal<Required<Header.barcode>>>>.Select(Base, barcode);
            if(orderType != null)
            {
                HeaderView.Current.OrderType = orderType.OrderType;
                SetScanState(ScanStates.ProdOrd);
                return;
            }
            ProcessProdOrd(barcode);
        }

        protected virtual void ProcessOperation(string barcode)
        {
            AMProdOper oper = PXSelectReadonly<AMProdOper,
                Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                And<AMProdOper.operationCD, Equal<Required<Header.barcode>>>>>>.Select(Base, HeaderView.Current.OrderType, HeaderView.Current.ProdOrdID, barcode);
            if (oper == null)
                ReportError(Msg.OperationMissing, barcode);
            else
            {
                HeaderView.Current.OperationID = oper.OperationID;
                SetDefaultShift(oper);
                ProcessItem();
            }
        }

        protected virtual void SetDefaultShift(AMProdOper oper)
        {
            PXResultset<AMShift> result = PXSelectJoin<AMShift,
        InnerJoin<AMProdOper, On<AMShift.wcID, Equal<AMProdOper.wcID>>>,
        Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
            And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                .Select(Base, oper.OrderType, oper.ProdOrdID, oper.OperationID);

            if (result == null || result.Count != 1)
            {
                return;
            }

            HeaderView.Current.ShiftID = ((AMShift)result[0])?.ShiftID;
            HeaderView.Current.LaborCodeID = ((AMShift)result[0])?.LaborCodeID;
        }

        protected virtual void ProcessProdOrd(string barcode)
        {
            if (HeaderView.Current.OrderType == null)
                ReportError(Msg.OrderTypeMissing, barcode);

            AMProdItem prodOrd = PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Required<Header.barcode>>>>>.Select(Base, HeaderView.Current.OrderType, barcode);
            if (prodOrd == null)
                ReportError(Msg.ProdOrdMissing, barcode);
            else if (!ProductionStatus.IsReleasedTransactionStatus(prodOrd))
                ReportError(Msg.ProdOrdWrongStatus, prodOrd.OrderType, prodOrd.ProdOrdID, ProductionOrderStatus.GetStatusDescription(prodOrd.StatusID));
            else
            {
                HeaderView.Current.ProdOrdID = prodOrd.ProdOrdID;
                HeaderView.Current.InventoryID = prodOrd.InventoryID;
                HeaderView.Current.LastOperationID = prodOrd.LastOperationID;
                HeaderView.Current.LaborType = AMLaborType.Direct;
                SetScanState(ScanStates.Operation);
            }
        }

        protected virtual void ProcessOrderType(string barcode)
        {
            AMOrderType orderType = PXSelectReadonly<AMOrderType,
                Where<AMOrderType.orderType, Equal<Required<Header.barcode>>>>.Select(Base, barcode);
            if (orderType == null)
                ReportError(Msg.OrderTypeMissing, barcode);
            else
            {
                HeaderView.Current.OrderType = orderType.OrderType;
                SetScanState(ScanStates.ProdOrd);
            }

        }

        protected virtual void ProcessWarehouse(string barcode)
        {
            INSite site =
                PXSelectReadonly<INSite,
                Where<INSite.siteCD, Equal<Required<Header.barcode>>>>
                .Select(Base, barcode);

            if (site == null)
            {
                ReportError(Msg.WarehouseMissing, barcode);
            }
            else if (IsValid<Header.siteID>(site.SiteID, out string error) == false)
            {
                ReportError(error);
                return;
            }
            else
            {
                HeaderView.Current.SiteID = site.SiteID;
                SetScanState(ScanStates.Location, Msg.WarehouseReady, site.SiteCD);
            }
        }

        protected override void ProcessLocationBarcode(string barcode)
        {
            INLocation location = ReadLocationByBarcode(HeaderView.Current.SiteID, barcode);
            if (location == null)
                return;

            HeaderView.Current.LocationID = location.LocationID;
            if (IsLotSerialRequired() && HeaderView.Current.LotSerTrack != INLotSerTrack.NotNumbered)
                SetScanState(ScanStates.LotSerial);
            else
                SetScanState(IsShiftSet ? ScanStates.LaborTime : ScanStates.Shift);
        }

        protected virtual void ProcessItem()
        {
            var invtID = HeaderView.Current.InventoryID;
            InventoryItem inventoryItem = PXSelectReadonly<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, invtID);
            if (inventoryItem == null)
            {
                ReportError(Msg.InventoryMissing, invtID);
            }

            ProcessItemBarcode(inventoryItem.InventoryCD);
        }

        protected override void ProcessItemBarcode(string barcode)
        {
            var item = ReadItemByBarcode(barcode);
            if (item == null)
            {
                if (HandleItemAbsence(barcode) == false)
                    ReportError(Msg.InventoryMissing, barcode);
                return;
            }

            INItemXRef xref = item;
            InventoryItem inventoryItem = item;
            INLotSerClass lsclass = item;
            var uom = xref.UOM ?? inventoryItem.SalesUnit;

            if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered &&
                uom != inventoryItem.BaseUnit)
            {
                ReportError(Msg.SerialItemNotComplexQty);
                return;
            }

            HeaderView.Current.InventoryID = xref.InventoryID;
            HeaderView.Current.SubItemID = xref.SubItemID;
            if (HeaderView.Current.UOM == null)
                HeaderView.Current.UOM = uom;
            HeaderView.Current.LotSerTrack = lsclass.LotSerTrack;
            HeaderView.Current.LotSerAssign = lsclass.LotSerAssign;
            HeaderView.Current.LotSerTrackExpiration = lsclass.LotSerTrackExpiration;
            HeaderView.Current.AutoNextNbr = lsclass.AutoNextNbr;

            Report(Msg.InventoryReady, inventoryItem.InventoryCD);

            if (IsLastOperation())
                SetScanState(IsWarehouseRequired() ? ScanStates.Warehouse : ScanStates.Location);
            else
            {
                SetScanState(IsShiftSet ? ScanStates.LaborTime : ScanStates.Shift);
            }
        }

        protected virtual bool HandleItemAbsence(string barcode)
        {
            ProcessLocationBarcode(barcode);
            if (Info.Current.MessageType == WMSMessageTypes.Information)
                return true; // location found

            return false;
        }

        protected override void ProcessLotSerialBarcode(string barcode)
        {
            if (IsValid<Header.lotSerialNbr>(barcode, out string error) == false)
            {
                ReportError(error);
                return;
            }

            HeaderView.Current.LotSerialNbr = barcode;
            Report(Msg.LotSerialReady, barcode);

            if (HeaderView.Current.LotSerTrackExpiration == true && IsExpirationDateRequired())
                SetScanState(ScanStates.ExpireDate);
            else
                SetScanState(IsShiftSet ? ScanStates.LaborTime : ScanStates.Shift);
        }

        protected override void ProcessExpireDate(string barcode)
        {
            if (DateTime.TryParse(barcode.Trim(), out DateTime value) == false)
            {
                ReportError(Msg.LotSerialExpireDateBadFormat);
                return;
            }

            if (IsValid<Header.expireDate>(value, out string error) == false)
            {
                ReportError(error);
                return;
            }

            HeaderView.Current.ExpireDate = value;
            SetScanState(IsShiftSet ? ScanStates.LaborTime : ScanStates.Shift, Msg.LotSerialExpireDateReady, barcode);
        }

        protected override bool ProcessQtyBarcode(string barcode)
        {
            var result = base.ProcessQtyBarcode(barcode);

            if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered &&
                HeaderView.Current.Qty != 1)
            {
                HeaderView.Current.Qty = 1;
                ReportError(Msg.SerialItemNotComplexQty);
            }

            return result;
        }

        protected virtual void ProcessShift(string barcode)
        {
            if (HeaderView.Current.ProdOrdID == null)
            {
                AMShiftMst shiftMst = PXSelect<AMShiftMst, Where<AMShiftMst.shiftID, Equal<Required<AMShiftMst.shiftID>>>>.Select(Base, barcode);
                if (shiftMst == null)
                {
                    ReportError(Msg.ShiftMissing, barcode);
                    return;
                }
                HeaderView.Current.ShiftID = shiftMst.ShiftID;
            }
            else
            {
                AMShift result = PXSelectJoin<AMShift,
                    InnerJoin<AMProdOper, On<AMShift.wcID, Equal<AMProdOper.wcID>>>,
                    Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                        And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>,
                        And<AMShift.shiftID, Equal<Required<AMShift.shiftID>>>>>>>
                        .Select(Base, HeaderView.Current.OrderType, HeaderView.Current.ProdOrdID, HeaderView.Current.OperationID, barcode);
                if(result == null)
                {
                    ReportError(Msg.ShiftMissing, barcode);
                    return;
                }
                HeaderView.Current.ShiftID = result.ShiftID;
                HeaderView.Current.LaborCodeID = result.LaborCodeID;
            }
            SetScanState(ScanStates.LaborTime);
        }

        protected virtual void ProcessLaborTime(string barcode)
        {
            TimeSpan time;

            //if the string contains a colon, parse it as is, otherwise process digits into a time
            if (barcode.Contains(":"))
            {
                if (!TimeSpan.TryParse(barcode, out time))
                {
                    ReportError(Msg.TimeFormatInvalid);
                    return;
                }
            }
            else
            {
                Int32 input;
                if(!Int32.TryParse(barcode, out input))
                {
                    ReportError(Msg.TimeFormatInvalid);
                    return;
                }
                if (input < 24)
                    time = new TimeSpan(input, 0, 0);
                else
                {
                    Int32 hours = barcode.Length <= 2 ? 0 : Convert.ToInt32(barcode.Substring(0, barcode.Length - 2));
                    Int32 minutes = Convert.ToInt32(barcode.Substring(barcode.Length - 2));
                    time = new TimeSpan(hours, minutes, 0);
                }
            }
            HeaderView.Current.LaborTime = Convert.ToInt32(time.TotalMinutes);
            SetScanState(HeaderView.Current.EmployeeID == null ? ScanStates.Employee : ScanStates.Confirm);
        }

        protected virtual void ProcessEmployee(string barcode)
        {
            EPEmployee employee = SelectFrom<EPEmployee>.Where<EPEmployee.acctCD.IsEqual<@P.AsString>>.View.Select(Base, barcode).First();
            if (employee == null)
            {
                ReportError(Msg.EmployeeNotFound, barcode);
                return;
            }
            if(employee.GetExtension<EPEmployeeExt>()?.AMProductionEmployee != true)
            {
                ReportError(Msg.EmployeeNotProduction, barcode);
                return;
            }
            HeaderView.Current.EmployeeID = employee.BAccountID;
            SetScanState(ScanStates.Confirm);
        }

        protected virtual void ProcessConfirm()
        {
            if (!ValidateConfirmation())
            {
                if (ExplicitLineConfirmation == false)
                    ClearHeaderInfo();
                return;
            }

            var header = HeaderView.Current;
            var userLotSerial = header.LotSerialNbr;
            bool isSerialItem = HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered;

            if (Batch == null) Base.batch.Insert();

            AMMTran existTransaction = FindLaborRow(header);

            Action rollbackAction = null;
            decimal? newQty = header.Qty;

            if (existTransaction != null)
            {
                newQty += existTransaction.Qty;

                if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered && newQty != 1)
                {
                    if (ExplicitLineConfirmation == false)
                        ClearHeaderInfo();
                    ReportError(Msg.SerialItemNotComplexQty);
                    return;
                }

                var backup = Base.transactions.Cache.CreateCopy(existTransaction) as AMMTran;

                Base.transactions.Cache.SetValueExt<AMMTran.lotSerialNbr>(existTransaction, userLotSerial);
                if (header.LotSerTrackExpiration == true && header.ExpireDate != null)
                    Base.transactions.Cache.SetValueExt<AMMTran.expireDate>(existTransaction, header.ExpireDate);
                existTransaction = Base.transactions.Update(existTransaction);

                Base.transactions.Cache.SetValueExt<AMMTran.qty>(existTransaction, newQty);
                existTransaction = Base.transactions.Update(existTransaction);

                rollbackAction = () =>
                {
                    Base.transactions.Delete(existTransaction);
                    Base.transactions.Insert(backup);
                };
            }
            else
            {
                existTransaction = Base.transactions.Insert();
                Base.transactions.Cache.SetValueExt<AMMTran.orderType>(existTransaction, header.OrderType);
                Base.transactions.Cache.SetValueExt<AMMTran.prodOrdID>(existTransaction, header.ProdOrdID);
                Base.transactions.Cache.SetValueExt<AMMTran.operationID>(existTransaction, header.OperationID);
                Base.transactions.Cache.SetValueExt<AMMTran.inventoryID>(existTransaction, header.InventoryID);
                Base.transactions.Cache.SetValueExt<AMMTran.siteID>(existTransaction, header.SiteID);
                Base.transactions.Cache.SetValueExt<AMMTran.locationID>(existTransaction, header.LocationID);
                Base.transactions.Cache.SetValueExt<AMMTran.uOM>(existTransaction, header.UOM);
                Base.transactions.Cache.SetValueExt<AMMTran.laborType>(existTransaction, header.LaborType);
                Base.transactions.Cache.SetValueExt<AMMTran.laborCodeID>(existTransaction, header.LaborCodeID);
                Base.transactions.Cache.SetValueExt<AMMTran.shiftID>(existTransaction, header.ShiftID);
                Base.transactions.Cache.SetValueExt<AMMTran.laborTime>(existTransaction, header.LaborTime);
                Base.transactions.Cache.SetValueExt<AMMTran.employeeID>(existTransaction, header.EmployeeID);
                existTransaction = Base.transactions.Update(existTransaction);

                Base.transactions.Cache.SetValueExt<AMMTran.lotSerialNbr>(existTransaction, userLotSerial);
                if (header.LotSerTrackExpiration == true && header.ExpireDate != null)
                    Base.transactions.Cache.SetValueExt<AMMTran.expireDate>(existTransaction, header.ExpireDate);
                existTransaction = Base.transactions.Update(existTransaction);

                Base.transactions.Cache.SetValueExt<AMMTran.qty>(existTransaction, newQty);
                existTransaction = Base.transactions.Update(existTransaction);



                rollbackAction = () => Base.transactions.Delete(existTransaction);
            }
            decimal? dispQty = header.Qty;
            string dispUOM = header.UOM;

            if (!string.IsNullOrEmpty(header.LotSerialNbr))
            {
                foreach (AMMTranSplit split in Base.splits.Select())
                {
                    Base.splits.Cache.SetValueExt<AMMTranSplit.expireDate>(split, header.ExpireDate ?? existTransaction.ExpireDate);
                    Base.splits.Cache.SetValueExt<AMMTranSplit.lotSerialNbr>(split, header.LotSerialNbr);
                    Base.splits.Update(split);
                }
            }

            ClearHeaderInfo();
            SetScanState(ScanStates.Default, Msg.InventoryAdded, Base.transactions.Cache.GetValueExt<AMMTran.inventoryID>(existTransaction), dispQty, dispUOM);

            if (!isSerialItem)
                HeaderView.Current.IsQtyOverridable = true;
        }

        protected virtual bool ValidateConfirmation()
        {
            if (Batch?.Released == true)
            {
                ReportError(PX.Objects.IN.Messages.Document_Status_Invalid);
                return false;
            }
            if(HeaderView.Current.EmployeeID == null)
            {
                ReportError(Msg.EmployeeNotFound, "");
                return false;
            }
            if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered && (HeaderView.Current.Qty != 1 && HeaderView.Current.Qty != 0))
            {
                ReportError(Msg.SerialItemNotComplexQty);
                return false;
            }

            return true;
        }

        protected virtual void ProcessConfirmRemove()
        {
            if (!ValidateConfirmation())
            {
                if (ExplicitLineConfirmation == false)
                    ClearHeaderInfo();
                return;
            }

            var header = HeaderView.Current;

            AMMTran existTransaction = FindLaborRow(header);

            if (existTransaction != null)
            {
                bool isSerialItem = HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered;
                if (existTransaction.Qty == header.Qty)
                {
                    Base.transactions.Delete(existTransaction);
                }
                else
                {
                    var newQty = existTransaction.Qty - header.Qty;

                    if (!IsValid<AMMTran.qty, AMMTran>(existTransaction, newQty, out string error))
                    {
                        if (ExplicitLineConfirmation == false)
                            ClearHeaderInfo();
                        ReportError(error);
                        return;
                    }

                    Base.transactions.Cache.SetValueExt<AMMTran.qty>(existTransaction, newQty);
                    Base.transactions.Update(existTransaction);
                }

                SetScanState(
                    PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item,
                    Msg.InventoryRemoved,
                    Base.transactions.Cache.GetValueExt<AMMTran.inventoryID>(existTransaction), header.Qty, header.UOM);
                ClearHeaderInfo();

                if (!isSerialItem)
                    HeaderView.Current.IsQtyOverridable = true;
            }
            else
            {
                ReportError(Msg.BatchLineMissing, InventoryItem.PK.Find(Base, header.InventoryID).InventoryCD);
                ClearHeaderInfo();
                if (PromptLocationForEveryLine)
                    SetScanState(ScanStates.Location);
                else
                    ProcessItem();
            }
        }

        protected virtual void ProcessRelease()
        {
            if (Batch != null)
            {
                if (Batch.Released == true)
                {
                    ReportError(PX.Objects.IN.Messages.Document_Status_Invalid);
                    return;
                }

                if (Batch.Hold != false) Base.batch.Cache.SetValueExt<AMBatch.hold>(Batch, false);

                Save.Press();

                var clone = Base.Clone();

                WaitFor(
                (wArgs) =>
                {
                    AMDocumentRelease.ReleaseDoc(new List<AMBatch>() { wArgs.Document }, false);
                    PXLongOperation.SetCustomInfo(clone);
                    throw new PXOperationCompletedException(Msg.DocumentIsReleased);
                }, null, new DocumentWaitArguments(Batch), Msg.DocumentReleasing, Base.batch.Current.BatNbr);
            }
        }

        protected override void OnWaitEnd(PXLongRunStatus status, AMBatch primaryRow)
            => OnWaitEnd(status, primaryRow?.Released == true, Msg.DocumentIsReleased, Msg.DocumentReleaseFailed);

        protected virtual AMMTran FindLaborRow(Header header)
        {
            var existTransactions = Base.transactions.SelectMain().Where(t =>
                t.OrderType == header.OrderType &&
                t.ProdOrdID == header.ProdOrdID &&
                t.OperationID == header.OperationID &&
                t.InventoryID == header.InventoryID &&
                t.SiteID == header.SiteID &&
                t.LocationID == (header.LocationID ?? t.LocationID) &&
                t.UOM == header.UOM);

            AMMTran existTransaction = null;

            if (IsLastOperation() && IsLotSerialRequired())
            {
                foreach (var tran in existTransactions)
                {
                    Base.transactions.Current = tran;
                    if (Base.splits.SelectMain().Any(t => (t.LotSerialNbr ?? "") == (header.LotSerialNbr ?? "")))
                    {
                        existTransaction = tran;
                        break;
                    }
                }
            }
            else
            {
                existTransaction = existTransactions.FirstOrDefault();
            }

            return existTransaction;
        }

        protected override void ClearHeaderInfo(bool redirect = false)
        {
            base.ClearHeaderInfo(redirect);

            if (redirect)
            {
                HeaderView.Current.SiteID = null;
            }

            if (redirect || PromptLocationForEveryLine)
            {
                HeaderView.Current.LocationID = null;
            }

            HeaderView.Current.LotSerialNbr = null;
            HeaderView.Current.LotSerTrack = null;
            HeaderView.Current.ExpireDate = null;            
            HeaderView.Current.ProdOrdID = null;
            HeaderView.Current.OperationID = null;
            HeaderView.Current.LaborTime = null;
            HeaderView.Current.LaborCodeID = null;
            HeaderView.Current.ShiftID = null;
            HeaderView.Current.LaborType = null;
            HeaderView.Current.Qty = Convert.ToDecimal(0);
            if (HeaderView.Current.OrderType == null && UseDefaultOrderType())
                HeaderView.Current.OrderType = AMSetup.Current.DefaultOrderType;
        }

        protected override void ApplyState(string state)
        {
            switch (state)
            {
                case ScanStates.Default:
                    if (HeaderView.Current.OrderType == null)
                        Prompt(Msg.DefaultPrompt);
                    else
                        Prompt(Msg.DefaultPromptWithType);
                    break;
                case ScanStates.OrderType:
                    Prompt(Msg.OrderTypePrompt);
                    break;
                case ScanStates.ProdOrd:
                    Prompt(Msg.ProdOrdPrompt);
                    break;
                case ScanStates.Operation:
                    Prompt(Msg.OperationPrompt);
                    break;
                case ScanStates.Warehouse:
                    Prompt(Msg.WarehousePrompt);
                    break;
                case ScanStates.Location:
                    if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                        Prompt(Msg.LocationPrompt);
                    else
                        ProcessItem();
                    break;
                case ScanStates.LotSerial:
                    Prompt(Msg.LotSerialPrompt);
                    break;
                case ScanStates.Shift:
                    Prompt(Msg.ShiftPrompt);
                    break;
                case ScanStates.LaborTime:
                    Prompt(Msg.LaborTimePrompt);
                    break;
                case ScanStates.Employee:
                    Prompt(Msg.EmployeePrompt);
                    break;
                case ScanStates.Confirm:
                    if (ExplicitLineConfirmation)
                        Prompt(Msg.ConfirmationPrompt);
                    else if (HeaderView.Current.Remove == false)
                        ProcessConfirm();
                    else
                        ProcessConfirmRemove();
                    break;
            }
        }

        protected override string GetDefaultState(Header header = null) => ScanStates.Default;

        protected override void ClearMode()
        {
            ClearHeaderInfo();
            SetScanState(ScanStates.Default, Msg.ScreenCleared);
        }

        protected override void ProcessDocumentNumber(string barcode) => throw new NotImplementedException();

        protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();

        protected override bool PerformQtyCorrection(decimal qtyDelta)
        {
            //since we default qty to 0, need to add 1 to the delta
            return base.PerformQtyCorrection(qtyDelta +1);
        }

        private DateTime? EnsureExpireDateDefault() => LSSelect.ExpireDateByLot(Base, HeaderView.Current, null);

        protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQtyInMove != true;
        protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;
        protected override bool DocumentLoaded => Batch != null;
        protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItemInMove == true;


        #region Constants & Messages
        public new abstract class Modes : WMSBase.Modes
        {
            public static WMSModeOf<ScanLabor, ScanLaborHost> ScanLabor { get; } = WMSMode("INRE");

            public class scanLabor : PX.Data.BQL.BqlString.Constant<scanLabor> { public scanLabor() : base(ScanLabor) { } }
        }

        public new abstract class ScanStates : WMSBase.ScanStates
        {
            public const string Default = "DFLT";
            public const string Warehouse = "SITE";
            public const string Confirm = "CONF";
            public const string OrderType = "OTYP";
            public const string ProdOrd = "PROD";
            public const string Operation = "OPER";
            public const string Shift = "SHFT";
            public const string LaborTime = "TIME";
            public const string Employee = "EMPL";

        }

        public new abstract class ScanCommands : WMSBase.ScanCommands
        {
            public const string Release = Marker + "RELEASE*RECEIPT";
            public const string OrderType = Marker + "TYPE";
        }

        [PXLocalizable]
        public new abstract class Msg : WMSBase.Msg
        {
            public const string ScanLaborMode = "Scan Labor";

            public const string ConfirmationPrompt = "Confirm the line, or scan or enter the line quantity.";

            public const string BatchLineMissing = "Line {0} is not found in the batch.";

            public const string DocumentReleasing = "The {0} labor is being released.";
            public const string DocumentIsReleased = "The labor is successfully released.";
            public const string DocumentReleaseFailed = "The labor release failed.";
            public const string DefaultPrompt = "Scan an Order Type or Indirect Labor Code.";
            public const string DefaultPromptWithType = "Scan an Order Type, Production Order ID or Indirect Labor Code.";
            public const string OrderTypePrompt = "Scan the Order Type.";
            public const string ProdOrdPrompt = "Scan the Production Order ID.";
            public const string OperationPrompt = "Scan the Operation ID.";
            public const string ShiftPrompt = "Scan the Shift ID.";
            public const string LaborTimePrompt = "Scan the Labor Time (HH:MM).";
            public const string EmployeePrompt = "Scan the Employee ID.";
            public const string OrderTypeMissing = "The {0} order type is not found.";
            public const string ProdOrdMissing = "The {0} production order is not found.";
            public const string ProdOrdWrongStatus = "The production order {0}, {1} has a status of {2}";
            public const string OperationMissing = "The {0} operation is not found.";
            public const string ShiftMissing = "The {0} shift is not found.";
            public const string TimeFormatInvalid = "Time must be formatted as HH:MM or HHMM";
            public const string EmployeeNotFound = "Employee {0} is not found.";
            public const string EmployeeNotProduction = "Employee {0} is not set as a Production Employee.";
        }
        #endregion
    }
}