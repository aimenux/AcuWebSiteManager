using PX.Data;
using System;

namespace PX.Objects.AM
{
    [Serializable]
    [PXHidden]
    public class AMBOMCompareTreeNode : IBqlTable
    {
        #region ParentID
        public abstract class parentID : PX.Data.BQL.BqlString.Field<parentID> { }

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

        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "LineNbr")]
        public virtual int? LineNbr { get; set; }
        #endregion

        #region CategoryNbr
        public abstract class categoryNbr : PX.Data.BQL.BqlInt.Field<categoryNbr> { }

        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "CategoryNbr")]
        public virtual int? CategoryNbr { get; set; }
        #endregion

        #region DetailLineNbr
        public abstract class detailLineNbr : PX.Data.BQL.BqlString.Field<detailLineNbr> { }

        [PXString(IsKey = true)]
        [PXUIField(DisplayName = "DetailLineNbr")]
        public virtual string DetailLineNbr { get; set; }
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