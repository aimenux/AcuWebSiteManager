using System;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM
{
    /// <summary>
    /// PXProjection for <see cref="SOLine"/> only including the Manufacturing related fields
    /// Replacement for old SOLineAMExtension standalone table updates.
    /// </summary>
    [Serializable]
    [PXProjection(typeof(SOLine), Persistent = true)]
    [PXHidden]
    public class SOLineMfgOnly : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        protected String _OrderType;
        [PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
        [PXDefault]
        [PXUIField(DisplayName = "Order Type", Enabled = false)]
        public virtual String OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }
        #endregion
        #region OrderNbr
        public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
        protected String _OrderNbr;

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
        [PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
        [PXDefault]
        public virtual String OrderNbr
        {
            get { return this._OrderNbr; }
            set { this._OrderNbr = value; }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
        [PXDefault]
        [PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
        public virtual Int32? LineNbr
        {
            get { return this._LineNbr; }
            set { this._LineNbr = value; }
        }
        #endregion
        #region AMProdCreate
        public abstract class aMProdCreate : PX.Data.BQL.BqlBool.Field<aMProdCreate> { }
        protected Boolean? _AMProdCreate;
        [PXDBBool(BqlField = typeof(SOLineExt.aMProdCreate))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Mark for Production")]
        public virtual Boolean? AMProdCreate
        {
            get { return _AMProdCreate; }
            set { _AMProdCreate = value; }
        }
        #endregion
        #region AMProdQtyComplete
        public abstract class aMProdQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdQtyComplete> { }
        protected Decimal? _AMProdQtyComplete;
        [PXUIField(DisplayName = "Production Qty Complete", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBQuantity(BqlField = typeof(SOLineExt.aMProdQtyComplete))]
        public virtual Decimal? AMProdQtyComplete
        {
            get
            {
                return this._AMProdQtyComplete;
            }
            set
            {
                this._AMProdQtyComplete = value;
            }
        }
        #endregion
        #region AMProdBaseQtyComplete
        public abstract class aMProdBaseQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdBaseQtyComplete> { }
        protected Decimal? _AMProdBaseQtyComplete;
        [PXDBQuantity(BqlField = typeof(SOLineExt.aMProdBaseQtyComplete))]
        [PXUIField(DisplayName = "Production Base Qty Complete", Enabled = false, Visible = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? AMProdBaseQtyComplete
        {
            get
            {
                return this._AMProdBaseQtyComplete;
            }
            set
            {
                this._AMProdBaseQtyComplete = value;
            }
        }
        #endregion
        #region AMProdStatusID
        public abstract class aMProdStatusID : PX.Data.BQL.BqlString.Field<aMProdStatusID> { }
        protected String _AMProdStatusID;
        [PXDBString(1, IsFixed = true, BqlField = typeof(SOLineExt.aMProdStatusID))]
        [PXUIField(DisplayName = "Production Status", Enabled = false)]
        [ProductionOrderStatus.List]
        public virtual String AMProdStatusID
        {
            get
            {
                return this._AMProdStatusID;
            }
            set
            {
                this._AMProdStatusID = value;
            }
        }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }
        protected string _AMOrderType;
        [AMOrderTypeField(DisplayName = "Prod. Order Type", Enabled = false, BqlField = typeof(SOLineExt.aMOrderType))]
        public virtual string AMOrderType
        {
            get { return this._AMOrderType; }
            set { this._AMOrderType = value; }
        }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }
        protected string _AMProdOrdID;

        [ProductionNbr(Enabled = false, BqlField = typeof(SOLineExt.aMProdOrdID))]
        public string AMProdOrdID
        {
            get { return this._AMProdOrdID; }
            set { this._AMProdOrdID = value; }
        }
        #endregion
        #region AMSelected
        public abstract class aMSelected : PX.Data.BQL.BqlBool.Field<aMSelected> { }
        protected bool? _AMSelected = false;
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? AMSelected
        {
            get { return _AMSelected; }
            set { _AMSelected = value; }
        }
        #endregion
        #region AMEstimateID
        public abstract class aMEstimateID : PX.Data.BQL.BqlString.Field<aMEstimateID> { }
        protected string _AMEstimateID;

        [EstimateID(Enabled = false, BqlField = typeof(SOLineExt.aMEstimateID))]
        public virtual string AMEstimateID
        {
            get { return this._AMEstimateID; }
            set { this._AMEstimateID = value; }
        }
        #endregion
        #region AMEstimateRevisionID

        public abstract class aMEstimateRevisionID : PX.Data.BQL.BqlString.Field<aMEstimateRevisionID> { }
        protected string _AMEstimateRevisionID;

        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC", BqlField = typeof(SOLineExt.aMEstimateRevisionID))]
        [PXUIField(DisplayName = "Est. Revision", Enabled = false)]
        public virtual string AMEstimateRevisionID
        {
            get { return this._AMEstimateRevisionID; }
            set { this._AMEstimateRevisionID = value; }
        }

        #endregion
        #region AMConfigurationID
        public abstract class aMConfigurationID : PX.Data.BQL.BqlString.Field<aMConfigurationID> { }
        [PXDBString(BqlField = typeof(SOLineExt.aMConfigurationID))]
        [PXUIField(DisplayName = "Configuration ID", Enabled = false)]
        public virtual string AMConfigurationID { get; set; }
        #endregion
        #region IsConfigurable
        public abstract class isConfigurable : PX.Data.BQL.BqlBool.Field<isConfigurable> { }
        [PXBool]
        [PXUIField(DisplayName = "Is Configurable", Enabled = false)]
        [PXDependsOnFields(typeof(SOLineExt.aMConfigurationID))]
        public virtual bool? IsConfigurable => !string.IsNullOrEmpty(AMConfigurationID);

        #endregion
        #region AMParentLineNbr
        public abstract class aMParentLineNbr : PX.Data.BQL.BqlInt.Field<aMParentLineNbr> { }
        protected Int32? _AMParentLineNbr;
        [PXDBInt(BqlField = typeof(SOLineExt.aMParentLineNbr))]
        [PXUIField(DisplayName = "ParentLine Nbr.", Visible = false, Enabled = false)]
        public virtual Int32? AMParentLineNbr
        {
            get { return this._AMParentLineNbr; }
            set { this._AMParentLineNbr = value; }
        }
        #endregion
        #region AMIsSupplemental
        public abstract class aMIsSupplemental : PX.Data.BQL.BqlBool.Field<aMIsSupplemental> { }
        protected Boolean? _AMIsSupplemental;
        [PXDBBool(BqlField = typeof(SOLineExt.aMIsSupplemental))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Supplemental", Visible = false, Enabled = false)]
        public virtual Boolean? AMIsSupplemental
        {
            get;
            set;
        }
        #endregion
        #region AMOrigParentLineNbr
        public abstract class aMOrigParentLineNbr : PX.Data.BQL.BqlInt.Field<aMOrigParentLineNbr> { }
        protected Int32? _AMOrigParentLineNbr;
        [PXDBInt(BqlField = typeof(SOLineExt.aMOrigParentLineNbr))]
        [PXUIField(DisplayName = "Orig Parent Line Nbr.", Visible = false, Enabled = false)]
        public virtual Int32? AMOrigParentLineNbr
        {
            get { return this._AMOrigParentLineNbr; }
            set { this._AMOrigParentLineNbr = value; }
        }
        #endregion
        #region AMConfigKeyID
        public abstract class aMConfigKeyID : PX.Data.BQL.BqlString.Field<aMConfigKeyID> { }
        protected string _AMConfigKeyID;
        [PXDBString(120, IsUnicode = true, BqlField = typeof(SOLineExt.aMConfigKeyID))]
        [PXUIField(DisplayName = "Config. Key")]
        public virtual string AMConfigKeyID
        {
            get
            {
                return this._AMConfigKeyID;
            }
            set
            {
                this._AMConfigKeyID = value;
            }
        }
        #endregion
    }
}