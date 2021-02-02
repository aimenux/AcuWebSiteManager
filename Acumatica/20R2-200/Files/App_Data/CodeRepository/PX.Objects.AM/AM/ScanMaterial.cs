using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WMSBase = PX.Objects.IN.WarehouseManagementSystemGraph<PX.Objects.AM.ScanMaterial, PX.Objects.AM.ScanMaterialHost, PX.Objects.AM.AMBatch, PX.Objects.AM.ScanMaterial.Header>;

namespace PX.Objects.AM
{
    public class ScanMaterialHost : MaterialEntry
    {
        public override Type PrimaryItemType => typeof(ScanMaterial.Header);
        public PXFilter<ScanMaterial.Header> HeaderView;
    }

    public class ScanMaterial : WMSBase
    {
        public class UserSetup : PXUserSetupPerMode<UserSetup, ScanMaterialHost, Header, AMScanUserSetup, AMScanUserSetup.userID, AMScanUserSetup.mode, Modes.scanMaterial> { }

        #region DACs
        [PXCacheName("MFG Header")]
        [Serializable]
        public class Header : WMSHeader, ILSMaster
        {
            #region BatNbr
            [PXUnboundDefault(typeof(AMBatch.batNbr))]
            [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
            [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<AMDocType.material>>>))]
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
            [PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
            [PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
            [PXDefault("")]
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
            #region QtyRemaining  (Unbound)
            public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

            protected Decimal? _QtyRemaining;
            [PXQuantity]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Qty Remaining", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
            public virtual Decimal? QtyRemaining
            {
                get
                {
                    return this._QtyRemaining;
                }
                set
                {
                    this._QtyRemaining = value;
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

        [PXButton, PXUIField(DisplayName = "Set Qty")]
        protected override IEnumerable scanQty(PXAdapter adapter)
        {
            IsSetQty = true;
            return base.scanQty(adapter);
        }
        #endregion

        #region Event Handlers
        protected override void _(Events.RowSelected<Header> e)
        {
            base._(e);

            bool notReleaseAndHasLines = Batch?.Released != true && ((AMMTran)Base.transactions.Select()) != null;
            ScanRemove.SetEnabled(notReleaseAndHasLines);
            ScanRelease.SetEnabled(notReleaseAndHasLines);
            ScanModeIssue.SetEnabled(e.Row != null && e.Row.Mode != Modes.ScanMaterial);
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
            Base.batch.Current = Base.batch.Search<AMBatch.batNbr>(e.Row.RefNbr, AMDocType.Material);
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

        protected override WMSModeOf<ScanMaterial, ScanMaterialHost> DefaultMode => Modes.ScanMaterial;
        public override string CurrentModeName =>
            HeaderView.Current.Mode == Modes.ScanMaterial ? Msg.ScanMaterialMode :
            Msg.FreeMode;
        protected override string GetModePrompt()
        {
            if (HeaderView.Current.Mode == Modes.ScanMaterial)
            {
                if (HeaderView.Current.OrderType == null)
                    return Localize(Msg.OrderTypePrompt);
                if (HeaderView.Current.ProdOrdID == null)
                    return Localize(Msg.ProdOrdPrompt);
                if (HeaderView.Current.OperationID == null)
                    return Localize(Msg.OperationPrompt);
                if (HeaderView.Current.SiteID == null)
                    return Localize(Msg.WarehousePrompt);
                if (HeaderView.Current.LocationID == null)
                    return Localize(Msg.LocationPrompt);
                if (HeaderView.Current.InventoryID == null)
                    return Localize(Msg.InventoryPrompt);
                if (HeaderView.Current.LotSerialNbr == null && IsLotSerialRequired())
                    return Localize(Msg.LotSerialPrompt);
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

                case ScanCommands.Release:
                    ProcessRelease();
                    return true;

                case ScanCommands.OrderType:
                    SetScanState(ScanStates.OrderType);
                    return true;

                case ScanCommands.Prod:
                    SetScanState(ScanStates.ProdOrd);
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
                default:
                    return base.ProcessByState(doc);
            }
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
                SetScanState(IsWarehouseRequired() ? ScanStates.Warehouse : ScanStates.Location);
            }
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
            else if (prodOrd.Function == OrderTypeFunction.Disassemble)
                ReportError(Msg.ProdOrdWrongType, prodOrd.ProdOrdID);
            else if (!ProductionStatus.IsReleasedTransactionStatus(prodOrd))
                ReportError(Msg.ProdOrdWrongStatus, prodOrd.OrderType, prodOrd.ProdOrdID, ProductionOrderStatus.GetStatusDescription(prodOrd.StatusID));
            else
            {
                HeaderView.Current.ProdOrdID = prodOrd.ProdOrdID;
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
            SetScanState(ScanStates.Item,
                Msg.LocationReady, location.LocationCD);
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

            if (UseRemainingQty)
            {
                var matls = SelectFrom<AMProdMatl>
                    .Where<AMProdMatl.orderType.IsEqual<@P.AsString>
                    .And<AMProdMatl.prodOrdID.IsEqual<@P.AsString>>
                    .And<AMProdMatl.operationID.IsEqual<@P.AsInt>>
                    .And<AMProdMatl.inventoryID.IsEqual<@P.AsInt>>>.View.ReadOnly
                    .Select(Base, HeaderView.Current.OrderType, HeaderView.Current.ProdOrdID,
                    HeaderView.Current.OperationID, inventoryItem.InventoryID);
                foreach (AMProdMatl matl in matls)
                {
                    if(matl.QtyRemaining > 0)
                    {
                        HeaderSetter.Set(x => x.QtyRemaining, matl.QtyRemaining);
                        if(!IsSetQty)
                            HeaderSetter.Set(x => x.Qty, matl.QtyRemaining);
                        continue;
                    }
                }
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

            if (IsLotSerialRequired() && lsclass.LotSerTrack != INLotSerTrack.NotNumbered)
                SetScanState(ScanStates.LotSerial);
            else
                SetScanState(ScanStates.Confirm);
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

            if (HeaderView.Current.LotSerAssign == INLotSerAssign.WhenUsed && HeaderView.Current.LotSerTrackExpiration == true && IsExpirationDateRequired())
                SetScanState(ScanStates.ExpireDate);
            else
                SetScanState(ScanStates.Confirm);
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
            SetScanState(ScanStates.Confirm, Msg.LotSerialExpireDateReady, barcode);
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

            AMMTran existTransaction = FindIssueRow(header);

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
                Base.IsImport = true;
                existTransaction = Base.transactions.Insert();
                Base.transactions.Cache.SetValueExt<AMMTran.orderType>(existTransaction, header.OrderType);
                Base.transactions.Cache.SetValueExt<AMMTran.prodOrdID>(existTransaction, header.ProdOrdID);
                Base.transactions.Cache.SetValueExt<AMMTran.operationID>(existTransaction, header.OperationID);
                Base.transactions.Cache.SetValueExt<AMMTran.inventoryID>(existTransaction, header.InventoryID);
                Base.transactions.Cache.SetValueExt<AMMTran.siteID>(existTransaction, header.SiteID);
                Base.transactions.Cache.SetValueExt<AMMTran.locationID>(existTransaction, header.LocationID);
                Base.transactions.Cache.SetValueExt<AMMTran.uOM>(existTransaction, header.UOM);
                existTransaction = Base.transactions.Update(existTransaction);

                Base.transactions.Cache.SetValueExt<AMMTran.lotSerialNbr>(existTransaction, userLotSerial);
                if (header.LotSerTrackExpiration == true && header.ExpireDate != null)
                    Base.transactions.Cache.SetValueExt<AMMTran.expireDate>(existTransaction, header.ExpireDate);                
                existTransaction = Base.transactions.Update(existTransaction);

                Base.transactions.Cache.SetValueExt<AMMTran.qty>(existTransaction, newQty);
                existTransaction = Base.transactions.Update(existTransaction);
                Base.IsImport = false;

                rollbackAction = () => Base.transactions.Delete(existTransaction);
            }
            decimal? dispQty = header.Qty;
            string dispUOM = header.UOM;

            if (!EnsureSplitsLotSerial(userLotSerial, header, rollbackAction))
            {
                if (ExplicitLineConfirmation == false)
                    ClearHeaderInfo();
                return;
            }
            if (!EnsureSplitsLocation(header.LocationID, header, rollbackAction))
            {
                if (ExplicitLineConfirmation == false)
                    ClearHeaderInfo();
                return;
            }

            ClearHeaderInfo();

            SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item, Msg.InventoryAdded, Base.transactions.Cache.GetValueExt<AMMTran.inventoryID>(existTransaction),dispQty, dispUOM);

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
            if (!HeaderView.Current.InventoryID.HasValue)
            {
                ReportError(Msg.InventoryNotSet);
                return false;
            }
            if (HeaderView.Current.LotSerTrack == INLotSerTrack.SerialNumbered && HeaderView.Current.Qty != 1)
            {
                ReportError(Msg.SerialItemNotComplexQty);
                return false;
            }

            return true;
        }

        protected virtual bool EnsureSplitsLotSerial(string userLotSerial, Header header, Action rollbackAction)
        {
            if (!string.IsNullOrEmpty(userLotSerial) &&
                Base.splits.SelectMain().Any(s => s.LotSerialNbr != userLotSerial))
            {
                ReportError(Msg.QtyIssueExceedsQtyOnLot, userLotSerial,
                    HeaderView.Cache.GetValueExt<Header.inventoryID>(header));

                rollbackAction();

                ClearHeaderInfo();
                SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);

                return false;
            }
            return true;
        }

        protected virtual bool EnsureSplitsLocation(int? userLocationID, Header header, Action rollbackAction)
        {
            if (IsLocationRequired(header) &&
                Base.splits.SelectMain().Any(s => s.LocationID != userLocationID))
            {
                ReportError(Msg.QtyIssueExceedsQtyOnLocation,
                    HeaderView.Cache.GetValueExt<Header.locationID>(header),
                    HeaderView.Cache.GetValueExt<Header.inventoryID>(header));

                rollbackAction();

                ClearHeaderInfo();
                SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);

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

            AMMTran existTransaction = FindIssueRow(header);

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
                SetScanState(PromptLocationForEveryLine ? ScanStates.Location : ScanStates.Item);
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

        protected virtual AMMTran FindIssueRow(Header header)
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

            if (IsLotSerialRequired())
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
            if (!UseDefaultOrderType())
            {
                HeaderView.Current.OrderType = null;
            }
            IsSetQty = false;
        }

        protected override void ApplyState(string state)
        {
            switch (state)
            {
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
                case ScanStates.Item:
                    Prompt(Msg.InventoryPrompt);
                    break;
                case ScanStates.Location:
                    if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                        Prompt(Msg.LocationPrompt);
                    else
                        SetScanState(ScanStates.Item);
                    break;
                case ScanStates.LotSerial:
                    Prompt(Msg.LotSerialPrompt);
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

        protected override string GetDefaultState(Header header = null) => UseDefaultOrderType() ? ScanStates.ProdOrd : ScanStates.OrderType;

        protected override void ClearMode()
        {
            ClearHeaderInfo();
            SetScanState(UseDefaultOrderType() ? ScanStates.ProdOrd : ScanStates.OrderType, Msg.ScreenCleared);
        }

        protected override void ProcessDocumentNumber(string barcode) => throw new NotImplementedException();

        protected override void ProcessCartBarcode(string barcode) => throw new NotImplementedException();

        protected override bool PerformQtyCorrection(decimal qtyDelta)
        {
            if (UseRemainingQty)
            {
                var currentQty = HeaderView.Current.QtyRemaining;
                if (currentQty == null) return false;
                return base.PerformQtyCorrection(qtyDelta + 1 - currentQty.Value);
            }
            return base.PerformQtyCorrection(qtyDelta);
        }

        private DateTime? EnsureExpireDateDefault() => LSSelect.ExpireDateByLot(Base, HeaderView.Current, null);

        protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQtyInMaterials != true;
        protected override bool ExplicitLineConfirmation => Setup.Current.ExplicitLineConfirmation == true;
        protected override bool DocumentLoaded => Batch != null;
        protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItemInMaterials == true;
        protected bool UseRemainingQty => Setup.Current.UseRemainingQtyInMaterials == true;

        protected static bool IsSetQty = false;

        #region Constants & Messages
        public new abstract class Modes : WMSBase.Modes
        {
            public static WMSModeOf<ScanMaterial, ScanMaterialHost> ScanMaterial { get; } = WMSMode("ISSU");

            public class scanMaterial : PX.Data.BQL.BqlString.Constant<scanMaterial> { public scanMaterial() : base(ScanMaterial) { } }
        }

        public new abstract class ScanStates : WMSBase.ScanStates
        {
            public const string Warehouse = "SITE";
            public const string Confirm = "CONF";
            public const string OrderType = "OTYP";
            public const string ProdOrd = "PROD";
            public const string Operation = "OPER";

        }

        public new abstract class ScanCommands : WMSBase.ScanCommands
        {
            public const string Release = Marker + "RELEASE*ISSUE";
            public const string OrderType = Marker + "TYPE";
            public const string Prod = Marker + "PROD";
        }

        [PXLocalizable]
        public new abstract class Msg : WMSBase.Msg
        {
            public const string ScanMaterialMode = "Scan Material";

            public const string ConfirmationPrompt = "Confirm the line, or scan or enter the line quantity.";

            public const string BatchLineMissing = "Line {0} is not found in the batch.";

            public const string DocumentReleasing = "The {0} material is being released.";
            public const string DocumentIsReleased = "The material is successfully released.";
            public const string DocumentReleaseFailed = "The material release failed.";
            public const string OrderTypePrompt = "Scan the Order Type.";
            public const string ProdOrdPrompt = "Scan the Production Order ID";
            public const string OperationPrompt = "Scan the Operation ID.";
            public const string OrderTypeMissing = "The {0} order type is not found.";
            public const string ProdOrdMissing = "The {0} production order is not found.";
            public const string ProdOrdWrongStatus = "The production order {0}, {1} has a status of {2}";
            public const string ProdOrdWrongType = "The production order {0} is a Disassembly type.";
            public const string OperationMissing = "The {0} operation is not found.";
        }
        #endregion
    }
}