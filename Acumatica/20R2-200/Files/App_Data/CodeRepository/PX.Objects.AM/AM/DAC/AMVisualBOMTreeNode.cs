using PX.Data;
using System;

namespace PX.Objects.AM
{
    [Serializable]
    [PXHidden]
    public class AMVisualBOMTreeNode : IBqlTable
    {
        #region ProductID
        public abstract class productID : PX.Data.BQL.BqlString.Field<productID> { }

        [PXString(IsKey = true)]
        [PXUIField(DisplayName = "Product ID")]
        public virtual string ProductID { get; set; }
        #endregion

        #region ParentID
        public abstract class parentID : PX.Data.BQL.BqlString.Field<parentID> { }

        //BOMID + Rev Combination
        protected string _ParentID;
        [PXString(IsKey = true)]
        [PXUIField(DisplayName ="Parent ID")]
        public virtual string ParentID
        {
            get
            {
                return this._ParentID;
            }
            set
            {
                this._ParentID = value;
            }
        }
        #endregion

        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "OperationID")]
        public virtual int? OperationID { get; set; }
        #endregion

        #region LineID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "LineID")]
        public virtual int? LineID { get; set; }
        #endregion

        #region Label
        public abstract class label : PX.Data.BQL.BqlString.Field<label> { }

        [PXString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Label")]
        public virtual string Label { get; set; }
        #endregion

        #region ToolTip
        public abstract class toolTip : PX.Data.BQL.BqlString.Field<toolTip> { }

        [PXString]
        [PXUIField(DisplayName = "ToolTip")]
        public virtual string ToolTip { get; set; }
        #endregion

        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        [PXInt]
        [PXUIField(DisplayName = "Sort Order")]
        public virtual int? SortOrder { get; set; }
        #endregion

        #region Icon
        public abstract class icon : PX.Data.BQL.BqlString.Field<icon> { }

        [PXString(250)]
        public virtual String Icon { get; set; }
        #endregion
    }
}