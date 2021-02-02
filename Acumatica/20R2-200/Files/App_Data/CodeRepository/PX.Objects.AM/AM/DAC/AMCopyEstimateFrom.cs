using System;
using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    [Serializable]
    [PXHidden]
    public class AMCopyEstimateFrom : IBqlTable
    {
        #region CopyFrom
        public abstract class copyFrom : PX.Data.BQL.BqlString.Field<copyFrom> { }

        protected String _CopyFrom;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(CopyFromList.Estimate)]
        [PXUIField(DisplayName = "Copy From")]
        [CopyFromList.List]
        public virtual String CopyFrom
        {
            get
            {
                return this._CopyFrom;
            }
            set
            {
                this._CopyFrom = value;
            }
        }
        #endregion
        #region EstimateID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        protected String _EstimateID;
        [EstimateIDSelectAll]
        [PXUIField(DisplayName = "Estimate ID")]
        [PXString]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion
        #region Revision ID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [PXSelector(typeof(Search<AMEstimateItem.revisionID,
            Where<AMEstimateItem.estimateID, Equal<Current<AMCopyEstimateFrom.estimateID>>,
                And<Where<AMEstimateItem.estimateID, NotEqual<Current<AMEstimateItem.estimateID>>,
                Or<AMEstimateItem.estimateID, Equal<Current<AMEstimateItem.estimateID>>,
                And<AMEstimateItem.revisionID, NotEqual<Current<AMEstimateItem.revisionID>>>>>>>>),
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.revisionDate),
            typeof(AMEstimateItem.isPrimary))]
        [PXUIField(DisplayName = "Revision")]
        [PXString]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        protected String _BOMID;
        [BomID]
        [BOMIDSelector]
        public virtual String BOMID
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
        #region BOMRevisionID
        public abstract class bOMRevisionID : PX.Data.BQL.BqlString.Field<bOMRevisionID> { }

        [RevisionIDField]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMCopyEstimateFrom.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.status)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
        public virtual string BOMRevisionID { get; set; }

        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(Search<AMPSetup.defaultOrderType>))]
        [AMOrderTypeField(Visibility = PXUIVisibility.SelectorVisible)]
        [AMOrderTypeSelector]
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
        [ProductionNbr(Visibility = PXUIVisibility.SelectorVisible)]
        [ProductionOrderSelector(typeof(AMCopyEstimateFrom.orderType), true)]
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
        #region Override InventoryID
        public abstract class overrideInventoryID : PX.Data.BQL.BqlBool.Field<overrideInventoryID> { }

        protected bool? _OverrideInventoryID;
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Override InventoryID")]
        public virtual bool? OverrideInventoryID
        {
            get
            {
                return _OverrideInventoryID;
            }
            set
            {
                _OverrideInventoryID = value;
            }
        }
        #endregion

        #region Copy From List
        public static class CopyFromList
        {
            //Constants declaration 
            public const string Estimate = "0";
            public const string ProductionOrder = "1";
            public const string BOM = "2";

            //List attribute 
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                    new string[] { Estimate, ProductionOrder, BOM },
                    new string[] { Messages.Estimate, Messages.ProductionOrder, Messages.BOM })
                {; }
            }

            //BQL constants declaration
            public class estimate : PX.Data.BQL.BqlString.Constant<estimate>
            {
                public estimate() : base(Estimate) {; }
            }
            public class productionOrder : PX.Data.BQL.BqlString.Constant<productionOrder>
            {
                public productionOrder() : base(ProductionOrder) {; }
            }

            public class bOM : PX.Data.BQL.BqlString.Constant<bOM>
            {
                public bOM() : base(BOM) {; }
            }

        }
        #endregion
    }
}