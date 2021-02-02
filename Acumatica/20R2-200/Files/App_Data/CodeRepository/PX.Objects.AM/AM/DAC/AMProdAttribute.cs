using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;
using PX.Objects.CA;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.ProductionAttributes)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMProdAttribute : IBqlTable, IProdOper
    {
        internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}] LineNbr = {LineNbr}, OperationID = {OperationID}, Label = {Label}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdAttribute>.By<orderType, prodOrdID, lineNbr>
        {
            public static AMProdAttribute Find(PXGraph graph, string orderType, string prodOrdID, int? lineNbr) 
                => FindBy(graph, orderType, prodOrdID, lineNbr);
            public static AMProdAttribute FindDirty(PXGraph graph, string orderType, string prodOrdID, int? lineNbr)
                => PXSelect<AMProdAttribute,
                        Where<orderType, Equal<Required<orderType>>,
                            And<prodOrdID, Equal<Required<prodOrdID>>,
                                And<lineNbr, Equal<Required<lineNbr>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, lineNbr);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdAttribute>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdAttribute>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMProdAttribute>.By<orderType, prodOrdID, operationID> { }
        }

        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBDefault(typeof(AMProdItem.orderType))]
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

        protected string _ProdOrdID;
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBDefault(typeof(AMProdItem.prodOrdID))]
        [PXParent(typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdAttribute.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdAttribute.prodOrdID>>>>>))]
        public virtual string ProdOrdID
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
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMProdItem.lineCntrAttribute))]
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
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMProdOper.operationID,
            Where<AMProdOper.orderType, Equal<Current<AMProdAttribute.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMProdAttribute.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
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
        #region Level
        public abstract class level : PX.Data.BQL.BqlInt.Field<level> { }

        protected int? _Level;
        [PXDBInt]
        [PXDefault(AMAttributeLevels.Order)]
        [PXUIField(DisplayName = "Level", Enabled = false)]
        [AMAttributeLevels.ProdOrderList]
        [PXFormula(typeof(Switch<Case<Where<AMProdAttribute.operationID, IsNull>, AMAttributeLevels.order>, AMAttributeLevels.operation>))]
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
        #region Source
        public abstract class source : PX.Data.BQL.BqlInt.Field<source> { }

        protected int? _Source;
        [PXDBInt]
        [PXDefault(AMAttributeSource.Production)]
        [PXUIField(DisplayName = "Source", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [AMAttributeSource.ProductionList]
        public virtual int? Source
        {
            get
            {
                return this._Source;
            }
            set
            {
                this._Source = value;
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
        #region Label
        public abstract class label : PX.Data.BQL.BqlString.Field<label> { }

        protected string _Label;
        [PXDBString(30, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Label", Visibility = PXUIVisibility.SelectorVisible)]
        [PXCheckUnique(typeof(AMProdAttribute.orderType), typeof(AMProdAttribute.prodOrdID))]
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
        #region System Fields
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
        #endregion
    }
}