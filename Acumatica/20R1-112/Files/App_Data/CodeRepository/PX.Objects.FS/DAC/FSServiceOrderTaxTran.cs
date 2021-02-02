using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.TX;
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.FSServiceOrderTaxTran)]
    public class FSServiceOrderTaxTran : TaxDetail, PX.Data.IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSServiceOrderTaxTran>.By<srvOrdType, refNbr, recordID, taxID>
        {
            public static FSServiceOrderTaxTran Find(PXGraph graph, string srvOrdType, string refNbr, int? recordID, string taxID)
                => FindBy(graph, srvOrdType, refNbr, recordID, taxID);
        }
        #endregion

        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, IsKey = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSServiceOrder.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
        [PXUIField(DisplayName = "Service Order Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSServiceOrder.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSServiceOrder,
                            Where<FSServiceOrder.srvOrdType, Equal<Current<srvOrdType>>,
                                And<FSServiceOrder.refNbr, Equal<Current<refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID>
        {
        }
        protected Int32? _RecordID;
        [PXDBIdentity(IsKey = true)]
        public virtual Int32? RecordID
        {
            get
            {
                return this._RecordID;
            }
            set
            {
                this._RecordID = value;
            }
        }
        #endregion
        #region TaxID
        public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        [PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Tax ID")]
        [PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), DirtyRead = true)]
        public override String TaxID
        {
            get
            {
                return this._TaxID;
            }
            set
            {
                this._TaxID = value;
            }
        }
        #endregion

        #region JurisType
        public abstract class jurisType : PX.Data.BQL.BqlString.Field<jurisType> { }
        protected String _JurisType;
        [PXDBString(9, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Jurisdiction Type")]
        public virtual String JurisType
        {
            get
            {
                return this._JurisType;
            }
            set
            {
                this._JurisType = value;
            }
        }
        #endregion
        #region JurisName
        public abstract class jurisName : PX.Data.BQL.BqlString.Field<jurisName> { }
        protected String _JurisName;
        [PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Jurisdiction Name")]
        public virtual String JurisName
        {
            get
            {
                return this._JurisName;
            }
            set
            {
                this._JurisName = value;
            }
        }
        #endregion
        #region TaxRate
        public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong()]
        [CurrencyInfo(typeof(FSServiceOrder.curyInfoID))]
        public override Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
        #region CuryTaxableAmt
        public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
        protected decimal? _CuryTaxableAmt;
        [PXDBCurrency(typeof(curyInfoID), typeof(taxableAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        public virtual Decimal? CuryTaxableAmt
        {
            get
            {
                return this._CuryTaxableAmt;
            }
            set
            {
                this._CuryTaxableAmt = value;
            }
        }
        #endregion
        #region TaxableAmt
        public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
        protected Decimal? _TaxableAmt;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        public virtual Decimal? TaxableAmt
        {
            get
            {
                return this._TaxableAmt;
            }
            set
            {
                this._TaxableAmt = value;
            }
        }
        #endregion
        #region CuryTaxAmt
        public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
        protected decimal? _CuryTaxAmt;
        [PXDBCurrency(typeof(curyInfoID), typeof(taxAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
        public virtual Decimal? CuryTaxAmt
        {
            get
            {
                return this._CuryTaxAmt;
            }
            set
            {
                this._CuryTaxAmt = value;
            }
        }
        #endregion
        #region TaxAmt
        public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
        protected Decimal? _TaxAmt;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
        public virtual Decimal? TaxAmt
        {
            get
            {
                return this._TaxAmt;
            }
            set
            {
                this._TaxAmt = value;
            }
        }
        #endregion

        #region CuryExpenseAmt
        public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
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
    }
}
