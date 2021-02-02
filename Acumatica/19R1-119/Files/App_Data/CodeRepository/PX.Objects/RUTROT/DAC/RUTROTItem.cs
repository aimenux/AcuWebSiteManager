using System;
using PX.Data;
using PX.Objects.AR;


namespace PX.Objects.RUTROT
{
    #region internal types definitions
    [Serializable]
    [PXProjection(typeof(Select5<RUTROT,
        InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<RUTROT.docType>, And<ARInvoice.refNbr, Equal<RUTROT.refNbr>>>,
        LeftJoin<ARAdjust, On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>, And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
        LeftJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>>>,
        Where<ARInvoice.released, Equal<True>, And<ARInvoiceRUTROT.isRUTROTDeductible, Equal<True>, And<Where<ARPayment.docType, Equal<ARDocType.payment>, Or<ARPayment.docType, IsNull>>>>>,
        Aggregate<GroupBy<ARInvoice.customerID, GroupBy<ARInvoice.docType, GroupBy<ARInvoice.refNbr>>>>>), Persistent = true)]
    public partial class RUTROTItem : IBqlTable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARInvoice.docType))]
        [PXDefault]
        [ARInvoiceType.List]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
        public string DocType
        {
            get;
            set;
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, BqlField = typeof(ARInvoice.refNbr), InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        [ARInvoiceType.RefNbr(typeof(Search2<AR.Standalone.ARRegisterAlias.refNbr,
            InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>,
                And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>,
            InnerJoinSingleTable<Customer, On<AR.Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
            Where<AR.Standalone.ARRegisterAlias.docType, Equal<Optional<ARInvoice.docType>>,
                And2<Where<AR.Standalone.ARRegisterAlias.origModule, Equal<GL.BatchModule.moduleAR>,
                    Or<AR.Standalone.ARRegisterAlias.released, Equal<True>>>,
                And<Match<Customer, Current<AccessInfo.userName>>>>>,
            OrderBy<Desc<AR.Standalone.ARRegisterAlias.refNbr>>>), Filterable = true, IsPrimaryViewCompatible = true)]
        [ARInvoiceType.Numbering]
        [ARInvoiceNbr]
        public string RefNbr
        {
            get;
            set;
        }
        #endregion
        #region RUTROTType
        public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }

        [PXDBString(1, BqlField = typeof(RUTROT.rUTROTType))]
        [RUTROTTypes.List]
        [PXUIField(DisplayName = "Deduction Type", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
        public string RUTROTType
        {
            get;
            set;
        }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, TabOrder = 2, BqlField = typeof(ARInvoice.customerID))]
        public int? CustomerID
        {
            get;
            set;
        }
        #endregion
        #region DocDate
        public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

        [PXDBDate(BqlField = typeof(ARInvoice.docDate))]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public DateTime? DocDate
        {
            get;
            set;
        }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
        [AROpenPeriod(BqlField = typeof(ARInvoice.finPeriodID))]
        public string FinPeriodID
        {
            get;
            set;
        }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBString(BqlField = typeof(ARInvoice.docDesc))]
        public string DocDesc
        {
            get;
            set;
        }
        #endregion
        #region CuryDistributedAmt
        public abstract class curyDistributedAmt : PX.Data.BQL.BqlDecimal.Field<curyDistributedAmt> { }

        [PXUIField(DisplayName = "Distributed Amount", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
        [PXDBDecimal(BqlField = typeof(RUTROT.curyDistributedAmt))]
        public virtual decimal? CuryDistributedAmt
        {
            get;
            set;
        }
        #endregion
        #region CuryDocBal
        public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }

        [PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDBDecimal(BqlField = typeof(ARInvoice.curyDocBal))]
        public decimal? CuryDocBal
        {
            get;
            set;
        }
        #endregion
        #region CuryOrigDocAmt
        public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }

        [PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBDecimal(BqlField = typeof(ARInvoice.curyOrigDocAmt))]
        public decimal? CuryOrigDocAmt
        {
            get;
            set;
        }
        #endregion
        #region ClaimDate
        public abstract class claimDate : PX.Data.BQL.BqlDateTime.Field<claimDate> { }

        [PXDBDate(BqlField = typeof(RUTROT.claimDate))]
        [PXUIField(DisplayName = "Export Date", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
        public DateTime? ClaimDate
        {
            get;
            set;
        }
        #endregion
        #region ExportRefNbr
        public abstract class exportRefNbr : PX.Data.BQL.BqlInt.Field<exportRefNbr> { }

        [PXDBInt(BqlField = typeof(RUTROT.exportRefNbr))]
        [PXUIField(DisplayName = "Export Ref Nbr.", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
        public virtual int? ExportRefNbr
        {
            get;
            set;
        }
        #endregion
        #region PaymentReleased
        public abstract class paymentReleased : PX.Data.BQL.BqlBool.Field<paymentReleased> { }

        [PXDBBool(BqlField = typeof(ARPayment.released))]
        public virtual bool? PaymentReleased
        {
            get;
            set;
        }
        #endregion
        #region IsClaimed
        public abstract class isClaimed : PX.Data.BQL.BqlBool.Field<isClaimed> { }

        [PXDBBool(BqlField = typeof(RUTROT.isClaimed))]
        public virtual bool? IsClaimed
        {
            get;
            set;
        }
        #endregion
        #region Balancing Credit Memo RefNbr
        public abstract class balancingCreditMemoRefNbr : PX.Data.BQL.BqlString.Field<balancingCreditMemoRefNbr> { }

        [PXDBString(15, IsUnicode = true, BqlField = typeof(RUTROT.balancingCreditMemoRefNbr))]
        [PXUIField(DisplayName = "Balancing Credit Memo Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        public virtual string BalancingCreditMemoRefNbr
        {
            get;
            set;
        }
        #endregion
        #region Balancing Debit Memo RefNbr
        public abstract class balancingDebitMemoRefNbr : PX.Data.BQL.BqlString.Field<balancingDebitMemoRefNbr> { }

        [PXDBString(15, IsUnicode = true, BqlField = typeof(RUTROT.balancingDebitMemoRefNbr))]
        [PXUIField(DisplayName = "Balancing Debit Memo Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        public virtual string BalancingDebitMemoRefNbr
        {
            get;
            set;
        }
        #endregion
        #region Status
        public abstract class invoiceStatus : PX.Data.BQL.BqlString.Field<invoiceStatus> { }

        [PXDBString(1, IsFixed = true, BqlField = typeof(ARInvoice.status))]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [ARDocStatus.List]
        public string InvoiceStatus
        {
            get;
            set;
        }
        #endregion
    }
    #endregion
}
