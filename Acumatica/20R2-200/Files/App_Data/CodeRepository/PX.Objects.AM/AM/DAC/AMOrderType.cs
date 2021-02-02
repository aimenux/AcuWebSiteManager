using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Production Order Types
    /// </summary>
    [PXPrimaryGraph(typeof(AMOrderTypeMaint))]
    [PXCacheName(Messages.AMOrderTypes)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    public class AMOrderType : IBqlTable
    {
        internal string DebuggerDisplay => $"OrderType = {OrderType}, Descr = {Descr}";

        #region Keys
        public class PK : PrimaryKeyOf<AMOrderType>.By<orderType>
        {
            public static AMOrderType Find(PXGraph graph, string orderType) => FindBy(graph, orderType);
        }
        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [AMOrderTypeSelector(new Type[] {
            typeof(AMOrderType.orderType),
            typeof(AMOrderType.descr),
            typeof(AMOrderType.function),
            typeof(AMOrderType.active)})]
        [PX.Data.EP.PXFieldDescription]
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
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

        protected Boolean? _Active;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual Boolean? Active
        {
            get
            {
                return this._Active;
            }
            set
            {
                this._Active = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        [PX.Data.EP.PXFieldDescription]
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
        #region Function
        public abstract class function : PX.Data.BQL.BqlInt.Field<function> { }

        protected int? _Function;
        [PXDBInt]
        [PXDefault(OrderTypeFunction.Regular)]
        [PXUIField(DisplayName = "Function", Visibility = PXUIVisibility.SelectorVisible)]
        [OrderTypeFunction.List]
        public virtual int? Function
        {
            get
            {
                return this._Function;
            }
            set
            {
                this._Function = value;
            }
        }
        #endregion
        #region ProdNumberingID

        public abstract class prodNumberingID : PX.Data.BQL.BqlString.Field<prodNumberingID> { }

        protected String _ProdNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Order Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String ProdNumberingID
        {
            get
            {
                return this._ProdNumberingID;
            }
            set
            {
                this._ProdNumberingID = value;
            }
        }
        #endregion
        #region WIPAcctID
        public abstract class wIPAcctID : PX.Data.BQL.BqlInt.Field<wIPAcctID> { }

        protected Int32? _WIPAcctID;
        [PXDefault]
        [Account(DisplayName = "Work in Process Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
        [PXForeignReference(typeof(Field<AMOrderType.wIPAcctID>.IsRelatedTo<Account.accountID>))]
        public virtual Int32? WIPAcctID
        {
            get
            {
                return this._WIPAcctID;
            }
            set
            {
                this._WIPAcctID = value;
            }
        }
        #endregion
        #region WIPSubID
        public abstract class wIPSubID : PX.Data.BQL.BqlInt.Field<wIPSubID> { }

        protected Int32? _WIPSubID;
        [PXDefault]
        [SubAccount(typeof(AMOrderType.wIPAcctID), DisplayName = "Work In Process Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        [PXForeignReference(typeof(Field<AMOrderType.wIPSubID>.IsRelatedTo<Sub.subID>))]
        public virtual Int32? WIPSubID
        {
            get
            {
                return this._WIPSubID;
            }
            set
            {
                this._WIPSubID = value;
            }
        }
        #endregion
        #region WIPVarianceAcctID
        public abstract class wIPVarianceAcctID : PX.Data.BQL.BqlInt.Field<wIPVarianceAcctID> { }

        protected Int32? _WIPVarianceAcctID;
        [PXDefault]
        [Account(DisplayName = "WIP Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
        [PXForeignReference(typeof(Field<AMOrderType.wIPVarianceAcctID>.IsRelatedTo<Account.accountID>))]
        public virtual Int32? WIPVarianceAcctID
        {
            get
            {
                return this._WIPVarianceAcctID;
            }
            set
            {
                this._WIPVarianceAcctID = value;
            }
        }
        #endregion
        #region WIPVarianceSubID
        public abstract class wIPVarianceSubID : PX.Data.BQL.BqlInt.Field<wIPVarianceSubID> { }

        protected Int32? _WIPVarianceSubID;
        [PXDefault]
        [SubAccount(typeof(AMOrderType.wIPVarianceAcctID), DisplayName = "WIP Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        [PXForeignReference(typeof(Field<AMOrderType.wIPVarianceSubID>.IsRelatedTo<Sub.subID>))]
        public virtual Int32? WIPVarianceSubID
        {
            get
            {
                return this._WIPVarianceSubID;
            }
            set
            {
                this._WIPVarianceSubID = value;
            }
        }
        #endregion
        #region CopyNotesItem
        public abstract class copyNotesItem : PX.Data.BQL.BqlBool.Field<copyNotesItem> { }

        protected Boolean? _CopyNotesItem;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Item/Header")]
        public virtual Boolean? CopyNotesItem
        {
            get
            {
                return this._CopyNotesItem;
            }
            set
            {
                this._CopyNotesItem = value;
            }
        }
        #endregion
        #region CopyNotesOper
        public abstract class copyNotesOper : PX.Data.BQL.BqlBool.Field<copyNotesOper> { }

        protected Boolean? _CopyNotesOper;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Operation")]
        public virtual Boolean? CopyNotesOper
        {
            get
            {
                return this._CopyNotesOper;
            }
            set
            {
                this._CopyNotesOper = value;
            }
        }
        #endregion
        #region CopyNotesMatl
        public abstract class copyNotesMatl : PX.Data.BQL.BqlBool.Field<copyNotesMatl> { }

        protected Boolean? _CopyNotesMatl;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Material")]
        public virtual Boolean? CopyNotesMatl
        {
            get
            {
                return this._CopyNotesMatl;
            }
            set
            {
                this._CopyNotesMatl = value;
            }
        }
        #endregion
        #region CopyNotesStep
        public abstract class copyNotesStep : PX.Data.BQL.BqlBool.Field<copyNotesStep> { }

        protected Boolean? _CopyNotesStep;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Step")]
        public virtual Boolean? CopyNotesStep
        {
            get
            {
                return this._CopyNotesStep;
            }
            set
            {
                this._CopyNotesStep = value;
            }
        }
        #endregion
        #region CopyNotesTool
        public abstract class copyNotesTool : PX.Data.BQL.BqlBool.Field<copyNotesTool> { }

        protected Boolean? _CopyNotesTool;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tool")]
        public virtual Boolean? CopyNotesTool
        {
            get
            {
                return this._CopyNotesTool;
            }
            set
            {
                this._CopyNotesTool = value;
            }
        }
        #endregion
        #region CopyNotesOvhd
        public abstract class copyNotesOvhd : PX.Data.BQL.BqlBool.Field<copyNotesOvhd> { }

        protected Boolean? _CopyNotesOvhd;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Overhead")]
        public virtual Boolean? CopyNotesOvhd
        {
            get
            {
                return this._CopyNotesOvhd;
            }
            set
            {
                this._CopyNotesOvhd = value;
            }
        }
        #endregion
        #region DefaultCostMethod
        public abstract class defaultCostMethod : PX.Data.BQL.BqlInt.Field<defaultCostMethod> { }

        protected int? _DefaultCostMethod;
        [PXDBInt]
        [PXDefault(CostMethod.Estimated)]
        [PXUIField(DisplayName = "Costing Method")]
        [CostMethod.ListDefaults]
        public virtual int? DefaultCostMethod
        {
            get
            {
                return this._DefaultCostMethod;
            }
            set
            {
                this._DefaultCostMethod = value;
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
        #region UnderIssueMaterial
        /// <summary>
        /// Check for under issued material during move entry based on operation/current move qty
        /// </summary>
        public abstract class underIssueMaterial : PX.Data.BQL.BqlString.Field<underIssueMaterial> { }

        protected String _UnderIssueMaterial;
        /// <summary>
        /// Check for under issued material during move entry based on operation/current move qty
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SetupMessage.AllowMsg)]
        [PXUIField(DisplayName = "Under Issue Material")]
        [SetupMessage.List]
        public virtual String UnderIssueMaterial
        {
            get
            {
                return this._UnderIssueMaterial;
            }
            set
            {
                this._UnderIssueMaterial = value;
            }
        }
        #endregion
        #region MoveCompletedOrders
        /// <summary>
        /// Check operation completed status during move entry
        /// </summary>
        public abstract class moveCompletedOrders : PX.Data.BQL.BqlString.Field<moveCompletedOrders> { }

        protected String _MoveCompletedOrders;
        /// <summary>
        /// Check operation completed status during move entry
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SetupMessage.AllowMsg)]
        [PXUIField(DisplayName = "Move on Completed Operations")]
        [SetupMessage.List]
        public virtual String MoveCompletedOrders
        {
            get
            {
                return this._MoveCompletedOrders;
            }
            set
            {
                this._MoveCompletedOrders = value;
            }
        }
        #endregion
        #region OverCompleteOrders
        /// <summary>
        /// Check for last operation if move qty > order qty remaining
        /// </summary>
        public abstract class overCompleteOrders : PX.Data.BQL.BqlString.Field<overCompleteOrders> { }

        protected String _OverCompleteOrders;
        /// <summary>
        /// Check for last operation if move qty > order qty remaining
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SetupMessage.AllowMsg)]
        [PXUIField(DisplayName = "Over Complete Orders")]
        [SetupMessage.List]
        public virtual String OverCompleteOrders
        {
            get
            {
                return this._OverCompleteOrders;
            }
            set
            {
                this._OverCompleteOrders = value;
            }
        }
        #endregion
        #region ScrapSource
        public abstract class scrapSource : PX.Data.BQL.BqlInt.Field<scrapSource> { }

        protected int? _ScrapSource;
        [PXDBInt]
        [PXDefault(Attributes.ScrapSource.None)]
        [PXUIField(DisplayName = "Scrap Source")]
        [ScrapSource.List]
        public virtual int? ScrapSource
        {
            get
            {
                return this._ScrapSource;
            }
            set
            {
                this._ScrapSource = value;
            }
        }
        #endregion
        #region ScrapSiteID
        public abstract class scrapSiteID : PX.Data.BQL.BqlInt.Field<scrapSiteID> { }

        protected Int32? _ScrapSiteID;
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site(DisplayName = "Scrap Warehouse")]
        [PXForeignReference(typeof(Field<scrapSiteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? ScrapSiteID
        {
            get
            {
                return this._ScrapSiteID;
            }
            set
            {
                this._ScrapSiteID = value;
            }
        }
        #endregion
        #region ScrapLocationID
        public abstract class scrapLocationID : PX.Data.BQL.BqlInt.Field<scrapLocationID> { }

        protected Int32? _ScrapLocationID;
        [Location(typeof(AMOrderType.scrapSiteID), DisplayName = "Scrap Location")]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), 
            PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        [PXForeignReference(typeof(CompositeKey<Field<scrapSiteID>.IsRelatedTo<INLocation.siteID>, Field<scrapLocationID>.IsRelatedTo<INLocation.locationID>>))]
        public virtual Int32? ScrapLocationID
        {
            get
            {
                return this._ScrapLocationID;
            }
            set
            {
                this._ScrapLocationID = value;
            }
        }
        #endregion
        #region OverIssueMaterial
        /// <summary>
        /// Check for over issued material during material entry
        /// </summary>
        public abstract class overIssueMaterial : PX.Data.BQL.BqlString.Field<overIssueMaterial> { }

        protected String _OverIssueMaterial;
        /// <summary>
        /// Check for over issued material during material entry
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SetupMessage.AllowMsg)]
        [PXUIField(DisplayName = "Over Issue Material")]
        [SetupMessage.List]
        public virtual String OverIssueMaterial
        {
            get
            {
                return this._OverIssueMaterial;
            }
            set
            {
                this._OverIssueMaterial = value;
            }
        }
        #endregion
        #region IncludeUnreleasedOverIssueMaterial
        /// <summary>
        /// When checked, this option tells the calculation for over issue material to include any qty unreleased for the given material item.
        /// (Preference works with OverIssueMaterial.)
        /// </summary>
        public abstract class includeUnreleasedOverIssueMaterial : PX.Data.BQL.BqlBool.Field<includeUnreleasedOverIssueMaterial> { }

        protected Boolean? _IncludeUnreleasedOverIssueMaterial;
        /// <summary>
        /// When checked, this option tells the calculation for over issue material to include any qty unreleased for the given material item.
        /// (Preference works with OverIssueMaterial.)
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include unreleased batch qty")]
        public virtual Boolean? IncludeUnreleasedOverIssueMaterial
        {
            get
            {
                return this._IncludeUnreleasedOverIssueMaterial;
            }
            set
            {
                this._IncludeUnreleasedOverIssueMaterial = value;
            }
        }
        #endregion
        #region BackflushUnderIssueMaterial
        /// <summary>
        /// Check for under issued material during move entry based on operation/current move qty
        /// </summary>
        public abstract class backflushUnderIssueMaterial : PX.Data.BQL.BqlString.Field<backflushUnderIssueMaterial> { }

        protected String _BackflushUnderIssueMaterial;
        /// <summary>
        /// Check for under issued material during move entry based on operation/current move qty
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SetupMessage.AllowMsg)]
        [PXUIField(DisplayName = "Under Issue Backflush Material")]
        [SetupMessage.BackFlushList]
        public virtual String BackflushUnderIssueMaterial
        {
            get
            {
                return this._BackflushUnderIssueMaterial;
            }
            set
            {
                this._BackflushUnderIssueMaterial = value;
            }
        }
        #endregion
        #region IssueMaterialOnTheFly
        /// <summary>
        /// Check for the given material item being added to a material issue transaction if the item exists on the entered production order.
        /// </summary>
        public abstract class issueMaterialOnTheFly : PX.Data.BQL.BqlString.Field<issueMaterialOnTheFly> { }

        protected String _IssueMaterialOnTheFly;
        /// <summary>
        /// Check for the given material item being added to a material issue transaction if the item exists on the entered production order.
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(SetupMessage.AllowMsg)]
        [PXUIField(DisplayName = "Issue Material Not On Order")]
        [SetupMessage.List]
        public virtual String IssueMaterialOnTheFly
        {
            get
            {
                return this._IssueMaterialOnTheFly;
            }
            set
            {
                this._IssueMaterialOnTheFly = value;
            }
        }
        #endregion
        #region ExcludeFromMRP
        public abstract class excludeFromMRP : PX.Data.BQL.BqlBool.Field<excludeFromMRP> { }

        protected Boolean? _ExcludeFromMRP;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Exclude from MRP")]
        public virtual Boolean? ExcludeFromMRP
        {
            get
            {
                return this._ExcludeFromMRP;
            }
            set
            {
                this._ExcludeFromMRP = value;
            }
        }
        #endregion
        #region DefaultOperationMoveQty
        public abstract class defaultOperationMoveQty : PX.Data.BQL.BqlBool.Field<defaultOperationMoveQty> { }

        protected Boolean? _DefaultOperationMoveQty;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Default operation move qty")]
        public virtual Boolean? DefaultOperationMoveQty
        {
            get
            {
                return this._DefaultOperationMoveQty;
            }
            set
            {
                this._DefaultOperationMoveQty = value;
            }
        }
        #endregion
        #region ProductionReportID
        [PXDefault(Reports.ProductionTicketReportParams.ReportID, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName = "Print Production Report ID")]
        [PXSelector(typeof(Search<PX.SM.SiteMap.screenID,
                Where<PX.SM.SiteMap.screenID, Like<Common.AMwildcard_>, And<PX.SM.SiteMap.url, Like<PX.Objects.Common.urlReports>>>,
                OrderBy<Asc<PX.SM.SiteMap.screenID>>>), typeof(PX.SM.SiteMap.screenID), typeof(PX.SM.SiteMap.title),
            Headers = new string[] { PX.Objects.CA.Messages.ReportID, PX.Objects.CA.Messages.ReportName },
            DescriptionField = typeof(PX.SM.SiteMap.title))]
        public virtual String ProductionReportID { get; set; }
        public abstract class productionReportID : PX.Data.BQL.BqlString.Field<productionReportID> { }
        #endregion
        #region SubstituteWorkCenters
        public abstract class substituteWorkCenters : PX.Data.BQL.BqlBool.Field<substituteWorkCenters> { }

        protected Boolean? _SubstituteWorkCenters;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Substitute Work Centers")]
        public virtual Boolean? SubstituteWorkCenters
        {
            get
            {
                return this._SubstituteWorkCenters;
            }
            set
            {
                this._SubstituteWorkCenters = value;
            }
        }
        #endregion
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }
        protected Int32? _LineCntrAttribute;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrAttribute
        {
            get
            {
                return this._LineCntrAttribute;
            }
            set
            {
                this._LineCntrAttribute = value;
            }
        }
        #endregion
        #region CheckSchdMatlAvailability
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        public abstract class checkSchdMatlAvailability : PX.Data.BQL.BqlBool.Field<checkSchdMatlAvailability> { }

        protected Boolean? _CheckSchdMatlAvailability;
        /// <summary>
        /// APS Schedule option - Check for Material Availability.
        /// </summary>
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Check for Material Availability", FieldClass = Features.ADVANCEDPLANNINGFIELDCLASS)]
        public virtual Boolean? CheckSchdMatlAvailability
        {
            get
            {
                return this._CheckSchdMatlAvailability;
            }
            set
            {
                this._CheckSchdMatlAvailability = value;
            }
        }
        #endregion
    }
}