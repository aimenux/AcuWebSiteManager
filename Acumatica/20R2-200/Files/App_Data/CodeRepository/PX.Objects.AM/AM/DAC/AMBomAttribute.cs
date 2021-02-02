using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;
using PX.Objects.CA;

namespace PX.Objects.AM
{
    [Serializable]
    [PXPrimaryGraph(typeof(BOMAttributeMaint))]
    [PXCacheName(Messages.BOMAttributes)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMBomAttribute : IBqlTable, IBomAttr
    {
        internal string DebuggerDisplay => $"BOMID = {BOMID}, RevisionID = {RevisionID}, LineNbr = {LineNbr}, Label = {Label}, OperationID = {OperationID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMBomAttribute>.By<bOMID, revisionID, operationID, lineNbr>
        {
            public static AMBomAttribute Find(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineNbr)
                => FindBy(graph, bOMID, revisionID, operationID, lineNbr);
            public static AMBomAttribute FindDirty(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineNbr)
                => PXSelect<AMBomAttribute,
                    Where<bOMID, Equal<Required<bOMID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineNbr, Equal<Required<lineNbr>>>>>>>
                    .SelectWindowed(graph, 0, 1, bOMID, revisionID, operationID, lineNbr);
        }

        public static class FK
        {
            public class BOM : AMBomItem.PK.ForeignKeyOf<AMBomAttribute>.By<bOMID, revisionID> { }
            public class Operation : AMBomOper.PK.ForeignKeyOf<AMBomAttribute>.By<bOMID, revisionID, operationID> { }
        }

        #endregion

        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected string _BOMID;
        [BomID(IsKey = true, Visible = false, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBDefault(typeof(AMBomItem.bOMID))]
        [PXParent(typeof(Select<AMBomItem,
            Where<AMBomItem.bOMID, Equal<Current<AMBomAttribute.bOMID>>,
                And<AMBomItem.revisionID, Equal<Current<AMBomAttribute.revisionID>>>>>))]
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
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMBomItem.lineCntrAttribute))]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region Level
        public abstract class level : PX.Data.BQL.BqlInt.Field<level> { }
        protected int? _Level;
        [PXDBInt]
        [PXDefault(AMAttributeLevels.BOM)]
        [PXUIField(DisplayName = "Level", Enabled = false)]
        [AMAttributeLevels.BomList]
        [PXFormula(typeof(Switch<Case<Where<AMBomAttribute.operationID, IsNull>, AMAttributeLevels.bOM>, AMAttributeLevels.operation>))]
        public virtual int? Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
            }
        }
        #endregion
        #region AttributeID
        public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
        protected string _AttributeID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Attribute ID")]
        [PXDefault]
        [PXSelector(typeof(CSAttribute.attributeID))]
        public virtual string AttributeID
        {
            get
            {
                return this._AttributeID;
            }
            set
            {
                this._AttributeID = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }
        protected int? _OperationID;
        [OperationIDField]
        [PXSelector(typeof(Search<AMBomOper.operationID,
            Where<AMBomOper.bOMID, Equal<Current<AMBomAttribute.bOMID>>,
                And<AMBomOper.revisionID, Equal<Current<AMBomAttribute.revisionID>>>>>),
            SubstituteKey = typeof(AMBomOper.operationCD))]
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
        #region Label
        public abstract class label : PX.Data.BQL.BqlString.Field<label> { }
        protected string _Label;
        [PXDBString(30, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Label", Visibility = PXUIVisibility.SelectorVisible)]
        [PXCheckUnique(typeof(AMBomAttribute.bOMID), typeof(AMBomAttribute.revisionID))]
        public virtual string Label
        {
            get
            {
                return this._Label;
            }
            set
            {
                this._Label = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected string _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Description")]
        public virtual string Descr
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
        #region Enabled
        public abstract class enabled : PX.Data.BQL.BqlBool.Field<enabled> { }
        protected bool? _Enabled;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Enabled", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
            }
        }
        #endregion
        #region TransactionRequired
        public abstract class transactionRequired : PX.Data.BQL.BqlBool.Field<transactionRequired> { }
        protected bool? _TransactionRequired;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Transaction Required", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual bool? TransactionRequired
        {
            get
            {
                return this._TransactionRequired;
            }
            set
            {
                this._TransactionRequired = value;
            }
        }
        #endregion
        #region Value
        public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
        protected string _Value;
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Value")]
        [AMAttributeValue(typeof(attributeID))]
        [DynamicValueValidation(typeof(Search<CSAttribute.regExp, Where<CSAttribute.attributeID, Equal<Current<attributeID>>>>))]
        public virtual string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
            }
        }
        #endregion
        #region OrderFunction
        public abstract class orderFunction : PX.Data.BQL.BqlInt.Field<orderFunction> { }
        protected int? _OrderFunction;
        [PXDBInt]
        [PXDefault(OrderTypeFunction.All)]
        [PXUIField(DisplayName = "Order Function", Visibility = PXUIVisibility.SelectorVisible)]
        [OrderTypeFunction.ListAll]
        public virtual int? OrderFunction
        {
            get
            {
                return this._OrderFunction;
            }
            set
            {
                this._OrderFunction = value;
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
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID
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
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected byte[] _tstamp;
        [PXDBTimestamp]
        public virtual byte[] tstamp
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
    }
}