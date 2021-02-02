using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PO;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.CR;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// BOM Operation
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.BOMOper)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMBomOper : IBqlTable, IOperationMaster, IBomOper, INotable
    {
        internal string DebuggerDisplay => $"BOMID = {BOMID}, RevisionID = {RevisionID}, OperationCD = {OperationCD} ({OperationID}), WcID = {WcID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMBomOper>.By<bOMID, revisionID, operationID>
        {
            public static AMBomOper Find(PXGraph graph, string bOMID, string revisionID, int? operationID)
                => FindBy(graph, bOMID, revisionID, operationID);
            public static AMBomOper FindDirty(PXGraph graph, string bOMID, string prodOrdID, int? operationID)
                => PXSelect<AMBomOper,
                    Where<bOMID, Equal<Required<bOMID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>>>>>
                    .SelectWindowed(graph, 0, 1, bOMID, prodOrdID, operationID);
        }

        public class UK : PrimaryKeyOf<AMBomOper>.By<bOMID, revisionID, operationCD>
        {
            public static AMBomOper Find(PXGraph graph, string bOMID, string revisionID, string operationCD) => FindBy(graph, bOMID, revisionID, operationCD);
        }

        public static class FK
        {
            public class BOM : AMBomItem.PK.ForeignKeyOf<AMBomOper>.By<bOMID, revisionID> { }
            public class Workcenter : AMWC.PK.ForeignKeyOf<AMBomOper>.By<wcID> { }
        }

        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected", Enabled = true)]
        public virtual bool? Selected
        {
            get { return _Selected; }
            set { _Selected = value; }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        protected string _BOMID;
        [BomID(IsKey = true, Visible = false, Enabled = false)]
        [BOMIDSelector(ValidateValue = false)]
        [PXDBDefault(typeof(AMBomItem.bOMID))]
        [PXParent(typeof(Select<AMBomItem, Where<AMBomItem.bOMID, Equal<Current<AMBomOper.bOMID>>,
            And<AMBomItem.revisionID, Equal<Current<AMBomOper.revisionID>>>>>))]
        public virtual string BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
        #region RevisionID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected string _RevisionID;
        [RevisionIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMBomItem.revisionID))]
        public virtual string RevisionID
        {
            get
            {
                return this._RevisionID;
            }
            set
            {
                this._RevisionID = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMBomItem.lineCntrOperation))]
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
        #region OperationCD
        public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }

        protected string _OperationCD;
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [OperationCDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXCheckUnique(typeof(AMBomOper.bOMID), typeof(AMBomOper.revisionID))]
        public virtual string OperationCD
        {
            get { return this._OperationCD; }
            set { this._OperationCD = value; }
        }

        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Oper Desc", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion
        #region WcID
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<AMBSetup.wcID>))]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        [PXForeignReference(typeof(Field<AMBomOper.wcID>.IsRelatedTo<AMWC.wcID>))]
        [PXRestrictor(typeof(Where<AMWC.activeFlg, Equal<True>>), Messages.WorkCenterNotActive)]
        public virtual String WcID
        {
            get
            {
                return this._WcID;
            }
            set
            {
                this._WcID = value;
            }
        }
        #endregion
        #region SetupTime
        public abstract class setupTime : PX.Data.BQL.BqlInt.Field<setupTime> { }

        protected Int32? _SetupTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = "Setup Time")]
        public virtual Int32? SetupTime
        {
            get
            {
                return this._SetupTime;
            }
            set
            {
                this._SetupTime = value;
            }
        }
        #endregion
        #region RunUnitTime
        public abstract class runUnitTime : PX.Data.BQL.BqlInt.Field<runUnitTime> { }

        protected Int32? _RunUnitTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "60")]
        [PXUIField(DisplayName = "Run Time")]
        public virtual Int32? RunUnitTime
        {
            get
            {
                return this._RunUnitTime;
            }
            set
            {
                this._RunUnitTime = value;
            }
        }
        #endregion
        #region RunUnits
        public abstract class runUnits : PX.Data.BQL.BqlDecimal.Field<runUnits> { }

        protected Decimal? _RunUnits;
        [PXDBQuantity(MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Run Units")]
        public virtual Decimal? RunUnits
        {
            get
            {
                return this._RunUnits;
            }
            set
            {
                this._RunUnits = value;
            }
        }
        #endregion
        #region MachineUnitTime
        public abstract class machineUnitTime : PX.Data.BQL.BqlInt.Field<machineUnitTime> { }

        protected Int32? _MachineUnitTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "60")]
        [PXUIField(DisplayName = "Machine Time")]
        public virtual Int32? MachineUnitTime
        {
            get
            {
                return this._MachineUnitTime;
            }
            set
            {
                this._MachineUnitTime = value;
            }
        }
        #endregion
        #region MachineUnits
        public abstract class machineUnits : PX.Data.BQL.BqlDecimal.Field<machineUnits> { }

        protected Decimal? _MachineUnits;
        [PXDBQuantity(MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine Units")]
        public virtual Decimal? MachineUnits
        {
            get
            {
                return this._MachineUnits;
            }
            set
            {
                this._MachineUnits = value;
            }
        }
        #endregion
        #region QueueTime
        public abstract class queueTime : PX.Data.BQL.BqlInt.Field<queueTime> { }

        protected Int32? _QueueTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = "Queue Time")]
        public virtual Int32? QueueTime
        {
            get
            {
                return this._QueueTime;
            }
            set
            {
                this._QueueTime = value;
            }
        }
        #endregion
        #region BFlush
        public abstract class bFlush : PX.Data.BQL.BqlBool.Field<bFlush> { }

        protected Boolean? _BFlush;
        [PXDBBool]
        [PXDefault(false, typeof(Search<AMWC.bflushLbr, Where<AMWC.wcID, Equal<Current<AMBomOper.wcID>>>>))]
        [PXUIField(DisplayName = "Backflush Labor")]
        public virtual Boolean? BFlush
        {
            get
            {
                return this._BFlush;
            }
            set
            {
                this._BFlush = value;
            }
        }
        #endregion
        #region LineCntrMatl
        public abstract class lineCntrMatl : PX.Data.BQL.BqlInt.Field<lineCntrMatl> { }

        protected Int32? _LineCntrMatl;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrMatl
        {
            get
            {
                return this._LineCntrMatl;
            }
            set
            {
                this._LineCntrMatl = value;
            }
        }
        #endregion
        #region LineCntrOvhd
        public abstract class lineCntrOvhd : PX.Data.BQL.BqlInt.Field<lineCntrOvhd> { }

        protected Int32? _LineCntrOvhd;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrOvhd
        {
            get
            {
                return this._LineCntrOvhd;
            }
            set
            {
                this._LineCntrOvhd = value;
            }
        }
        #endregion
        #region LineCntrStep
        public abstract class lineCntrStep : PX.Data.BQL.BqlInt.Field<lineCntrStep> { }

        protected Int32? _LineCntrStep;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrStep
        {
            get
            {
                return this._LineCntrStep;
            }
            set
            {
                this._LineCntrStep = value;
            }
        }
        #endregion
        #region LineCntrTool
        public abstract class lineCntrTool : PX.Data.BQL.BqlInt.Field<lineCntrTool> { }

        protected Int32? _LineCntrTool;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrTool
        {
            get
            {
                return this._LineCntrTool;
            }
            set
            {
                this._LineCntrTool = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region ScrapAction
        public abstract class scrapAction : PX.Data.BQL.BqlInt.Field<scrapAction> { }

        protected int? _ScrapAction;
        [PXDBInt]
        [PXDefault(Attributes.ScrapAction.NoAction, typeof(Search<AMWC.scrapAction, Where<AMWC.wcID,
            Equal<Current<AMBomOper.wcID>>>>))]
        [PXUIField(DisplayName = "Scrap Action")]
        [ScrapAction.List]
        public virtual int? ScrapAction
        {
            get
            {
                return this._ScrapAction;
            }
            set
            {
                this._ScrapAction = value;
            }
        }
        #endregion
        #region RowStatus
        public abstract class rowStatus : PX.Data.BQL.BqlInt.Field<rowStatus> { }
        protected int? _RowStatus;
        [PXDBInt]
        [PXUIField(DisplayName = "Change Status", Enabled = false)]
        [AMRowStatus.List]
        public virtual int? RowStatus
        {
            get
            {
                return this._RowStatus;
            }
            set
            {
                this._RowStatus = value;
            }
        }
        #endregion

        //Outside Processing Values
        #region OutsideProcess
        public abstract class outsideProcess : PX.Data.BQL.BqlBool.Field<outsideProcess> { }

        protected Boolean? _OutsideProcess;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Outside Process")]
        public virtual Boolean? OutsideProcess
        {
            get
            {
                return this._OutsideProcess;
            }
            set
            {
                this._OutsideProcess = value;
            }
        }
        #endregion
        #region DropShippedToVendor
        public abstract class dropShippedToVendor : PX.Data.BQL.BqlBool.Field<dropShippedToVendor> { }

        protected Boolean? _DropShippedToVendor;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Drop Shipped to Vendor")]
        public virtual Boolean? DropShippedToVendor
        {
            get
            {
                return this._DropShippedToVendor;
            }
            set
            {
                this._DropShippedToVendor = value;
            }
        }
        #endregion
        #region VendorID

        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region VendorLocationID

        public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
        protected Int32? _VendorLocationID;
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<AMBomOper.vendorID>>,
            And<Location.isActive, Equal<True>,
            And<MatchWithBranch<Location.vBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible,
            DisplayName = "Vendor Location")]
        [PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
            InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<AMBomOper.vendorID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.vBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<AMBomOper.vendorID>>,
            And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMBomOper.vendorID>))]
        [PXForeignReference(typeof(Field<vendorLocationID>.IsRelatedTo<Location.locationID>))]
        public virtual Int32? VendorLocationID
        {
            get
            {
                return this._VendorLocationID;
            }
            set
            {
                this._VendorLocationID = value;
            }
        }
        #endregion
    }
}
