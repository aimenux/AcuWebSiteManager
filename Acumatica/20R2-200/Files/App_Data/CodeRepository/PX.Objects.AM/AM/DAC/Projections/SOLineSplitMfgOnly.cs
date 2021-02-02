using System;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM
{
    /// <summary>
    /// PXProjection for <see cref="SOLineSplit"/> only including the Manufacturing related fields
    /// Replacement for old SOLineAMExtension standalone table updates.
    /// </summary>
    [Serializable]
    [PXProjection(typeof(SOLineSplit), Persistent = true)]
    [PXHidden]
    public class SOLineSplitMfgOnly : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLineSplit.orderType))]
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

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLineSplit.orderNbr))]
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
        [PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.lineNbr))]
        [PXDefault]
        [PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
        public virtual Int32? LineNbr
        {
            get { return this._LineNbr; }
            set { this._LineNbr = value; }
        }
        #endregion
        #region SplitLineNbr
        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(SOLineSplit.splitLineNbr))]
        [PXDefault]
        [PXUIField(DisplayName = "Allocation ID", Visible = false, IsReadOnly = true)]
        public virtual Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
            }
        }
        #endregion

        #region AMProdCreate
        public abstract class aMProdCreate : PX.Data.BQL.BqlBool.Field<aMProdCreate> { }
        protected Boolean? _AMProdCreate;
        [PXDBBool(BqlField = typeof(SOLineSplitExt.aMProdCreate))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Mark for Production", Enabled = false)]
        public virtual Boolean? AMProdCreate
        {
            get { return _AMProdCreate; }
            set { _AMProdCreate = value ?? false; }
        }
        #endregion
        #region AMOrderType
        public abstract class aMOrderType : PX.Data.BQL.BqlString.Field<aMOrderType> { }
        protected string _AMOrderType;
        [PXDBString(2, IsFixed = true, InputMask = ">aa", BqlField = typeof(SOLineSplitExt.aMOrderType))]
        [PXUIField(DisplayName = "Prod. Order Type", Enabled = false)]
        public virtual string AMOrderType
        {
            get
            {
                return this._AMOrderType;
            }
            set
            {
                this._AMOrderType = value;
            }
        }
        #endregion
        #region AMProdOrdID
        public abstract class aMProdOrdID : PX.Data.BQL.BqlString.Field<aMProdOrdID> { }
        protected string _AMProdOrdID;
        [ProductionNbr(BqlField = typeof(SOLineSplitExt.aMProdOrdID))]
        public string AMProdOrdID
        {
            get
            {
                return this._AMProdOrdID;
            }
            set
            {
                this._AMProdOrdID = value;

            }
        }
        #endregion
        #region AMProdQtyComplete
        public abstract class aMProdQtyComplete : PX.Data.BQL.BqlDecimal.Field<aMProdQtyComplete> { }
        protected Decimal? _AMProdQtyComplete;
        [PXDBQuantity(BqlField = typeof(SOLineSplitExt.aMProdQtyComplete))]
        [PXUIField(DisplayName = "Production Qty Complete", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
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
        [PXDBQuantity(BqlField = typeof(SOLineSplitExt.aMProdBaseQtyComplete))]
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
        [PXDBString(1, IsFixed = true, BqlField = typeof(SOLineSplitExt.aMProdStatusID))]
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
    }
}