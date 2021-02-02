using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;
using PX.Objects.CA;

namespace PX.Objects.AM
{
    [Serializable]
    [PXPrimaryGraph(typeof(AMOrderTypeMaint))]
    [PXCacheName(Messages.OrderTypeAttributes)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMOrderTypeAttribute : IBqlTable
    {
        internal string DebuggerDisplay => $"OrderType = {OrderType}, LineNbr = {LineNbr}, Label = {Label}";

        #region Keys
        public class PK : PrimaryKeyOf<AMOrderTypeAttribute>.By<orderType, lineNbr>
        {
            public static AMOrderTypeAttribute Find(PXGraph graph, string orderType, int? lineNbr) => FindBy(graph, orderType, lineNbr);
        }
        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected string _OrderType;
        [AMOrderTypeField(DisplayName = "Order Type", IsKey = true, Visible = false, Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXParent(typeof(Select<AMOrderType,
            Where<AMOrderType.orderType, Equal<Current<AMOrderTypeAttribute.orderType>>>>))]
        [PXDBDefault(typeof(AMOrderType.orderType))]
        public virtual string OrderType
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
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected int? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXLineNbr(typeof(AMOrderType.lineCntrAttribute))]
        [PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
        public virtual int? LineNbr
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
        [PXCheckUnique(typeof(AMOrderTypeAttribute.orderType))]
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
        [PXDBTimestamp()]
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
