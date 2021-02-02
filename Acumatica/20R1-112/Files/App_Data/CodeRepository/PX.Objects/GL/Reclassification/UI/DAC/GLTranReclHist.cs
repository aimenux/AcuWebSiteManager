using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.GL.Reclassification.UI
{
    [PXBreakInheritance]
    public class GLTranReclHist : GLTran
    {
        #region field to reuse
        public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        public new abstract class includedInReclassHistory : PX.Data.BQL.BqlBool.Field<includedInReclassHistory> { }
        public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
        public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
        public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
        public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
        public new abstract class debitAmt : PX.Data.BQL.BqlDecimal.Field<debitAmt> { }
        public new abstract class creditAmt : PX.Data.BQL.BqlDecimal.Field<creditAmt> { }
        public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        public new abstract class curyDebitAmt : PX.Data.BQL.BqlDecimal.Field<curyDebitAmt> { }
        public new abstract class curyCreditAmt : PX.Data.BQL.BqlDecimal.Field<curyCreditAmt> { }
        public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
        public new abstract class posted : PX.Data.BQL.BqlBool.Field<posted> { }
        public new abstract class nonBillable : PX.Data.BQL.BqlBool.Field<nonBillable> { }
        public new abstract class isInterCompany : PX.Data.BQL.BqlBool.Field<isInterCompany> { }
        public new abstract class summPost : PX.Data.BQL.BqlBool.Field<summPost> { }
        public new abstract class zeroPost : PX.Data.BQL.BqlBool.Field<zeroPost> { }
        public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
        public new abstract class origBatchNbr : PX.Data.BQL.BqlString.Field<origBatchNbr> { }
        public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
        public new abstract class origAccountID : PX.Data.BQL.BqlInt.Field<origAccountID> { }
        public new abstract class origSubID : PX.Data.BQL.BqlInt.Field<origSubID> { }
        public new abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }
        public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        public new abstract class tranClass : PX.Data.BQL.BqlString.Field<tranClass> { }
        public new abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
        public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
        public new abstract class tranLineNbr : PX.Data.BQL.BqlInt.Field<tranLineNbr> { }
        public new abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }
        public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
        public new abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }
        public new abstract class pMTranID : PX.Data.BQL.BqlLong.Field<pMTranID> { }
        public new abstract class origPMTranID : PX.Data.BQL.BqlLong.Field<origPMTranID> { }
        public new abstract class ledgerBalanceType : PX.Data.BQL.BqlString.Field<ledgerBalanceType> { }
        public new abstract class accountRequireUnits : PX.Data.BQL.BqlBool.Field<accountRequireUnits> { }
        public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
        public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        public new abstract class reclassificationProhibited : PX.Data.BQL.BqlBool.Field<reclassificationProhibited> { }
        public new abstract class reclassBatchModule : PX.Data.BQL.BqlString.Field<reclassBatchModule> { }
        public new abstract class reclassBatchNbr : PX.Data.BQL.BqlString.Field<reclassBatchNbr> { }
        public new abstract class isReclassReverse : PX.Data.BQL.BqlBool.Field<isReclassReverse> { }
        public new abstract class reclassType : PX.Data.BQL.BqlString.Field<reclassType> { }
        public new abstract class curyReclassRemainingAmt : PX.Data.BQL.BqlDecimal.Field<curyReclassRemainingAmt> { }
        public new abstract class reclassRemainingAmt : PX.Data.BQL.BqlDecimal.Field<reclassRemainingAmt> { }
        public new abstract class reclassified : PX.Data.BQL.BqlBool.Field<reclassified> { }
        public new abstract class reclassSourceTranModule : PX.Data.BQL.BqlString.Field<reclassSourceTranModule> { }
        public new abstract class reclassSourceTranBatchNbr : PX.Data.BQL.BqlString.Field<reclassSourceTranBatchNbr> { }
        public new abstract class reclassSourceTranLineNbr : PX.Data.BQL.BqlInt.Field<reclassSourceTranLineNbr> { }
        public new abstract class reclassSeqNbr : PX.Data.BQL.BqlInt.Field<reclassSeqNbr> { }
        #endregion

        public GLTranReclHist() { }

        public GLTranReclHist(string module, string batchNbr, int? lineNbr)
        {
            Module = module;
            BatchNbr = batchNbr;
            LineNbr = lineNbr;
        }

        public GLTranReclHist(GLTran tran)
        {
            IncludedInReclassHistory = tran.IncludedInReclassHistory;
            BranchID = tran.BranchID;
            Module = tran.Module;
            BatchNbr = tran.BatchNbr;
            LineNbr = tran.LineNbr;
            LedgerID = tran.LedgerID;
            AccountID = tran.AccountID;
            SubID = tran.SubID;
            ProjectID = tran.ProjectID;
            TaskID = tran.TaskID;
            CostCodeID = tran.CostCodeID;
            RefNbr = tran.RefNbr;
            InventoryID = tran.InventoryID;
            UOM = tran.UOM;
            Qty = tran.Qty;
            DebitAmt = tran.DebitAmt;
            CreditAmt = tran.CreditAmt;
            CuryInfoID = tran.CuryInfoID;
            CuryDebitAmt = tran.CuryDebitAmt;
            CuryCreditAmt = tran.CuryCreditAmt;
            Released = tran.Released;
            Posted = tran.Posted;
            NonBillable = tran.NonBillable;
            IsInterCompany = tran.IsInterCompany;
            SummPost = tran.SummPost;
            ZeroPost = tran.ZeroPost;
            OrigModule = tran.OrigModule;
            OrigBatchNbr = tran.OrigBatchNbr;
            OrigLineNbr = tran.OrigLineNbr;
            OrigAccountID = tran.OrigAccountID;
            OrigSubID = tran.OrigSubID;
            TranID = tran.TranID;
            TranType = tran.TranType;
            TranClass = tran.TranClass;
            TranDesc = tran.TranDesc;
            TranDate = tran.TranDate;
            TranLineNbr = tran.TranLineNbr;
            ReferenceID = tran.ReferenceID;
            FinPeriodID = tran.FinPeriodID;
            TranPeriodID = tran.TranPeriodID;
            CATranID = tran.CATranID;
            PMTranID = tran.PMTranID;
            OrigPMTranID = tran.OrigPMTranID;
            LedgerBalanceType = tran.LedgerBalanceType;
            AccountRequireUnits = tran.AccountRequireUnits;
            TaxID = tran.TaxID;
            TaxCategoryID = tran.TaxCategoryID;
            NoteID = tran.NoteID;
            ReclassificationProhibited = tran.ReclassificationProhibited;
            ReclassBatchModule = tran.ReclassBatchModule;
            ReclassBatchNbr = tran.ReclassBatchNbr;
            IsReclassReverse = tran.IsReclassReverse;
            ReclassType = tran.ReclassType;
            CuryReclassRemainingAmt = tran.CuryReclassRemainingAmt;
            ReclassRemainingAmt = tran.ReclassRemainingAmt;
            Reclassified = tran.Reclassified;
            ReclassSourceTranModule = tran.ReclassSourceTranModule;
            ReclassSourceTranBatchNbr = tran.ReclassSourceTranBatchNbr;
            ReclassSourceTranLineNbr = tran.ReclassSourceTranLineNbr;
            ReclassSeqNbr = tran.ReclassSeqNbr;
            tstamp = tran.tstamp;
            CreatedByID = tran.CreatedByID;
            CreatedByScreenID = tran.CreatedByScreenID;
            CreatedDateTime = tran.CreatedDateTime;
            LastModifiedByID = tran.LastModifiedByID;
            LastModifiedByScreenID = tran.LastModifiedByScreenID;
            LastModifiedDateTime = tran.LastModifiedDateTime;
        }

        #region Selected

        [PXBool]
        [PXUIField(DisplayName = "Selected", Visible = true, Enabled = true)]
        public override bool? Selected { get; set; }
        #endregion
        #region SplittedIcon
        public abstract class splitIcon : PX.Data.BQL.BqlString.Field<splitIcon> { }

        [PXUIField(DisplayName = "", Enabled = false, Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
        [PXImage]
        public virtual string SplitIcon { get; set; }
        #endregion
        #region ActionDesc
        public abstract class actionDesc : PX.Data.BQL.BqlString.Field<actionDesc> { }

        [PXString]
        [PXUIField(DisplayName = "Action", IsReadOnly = true, Visibility = PXUIVisibility.Visible)]
        [ReclassAction.List]
        public virtual string ActionDesc { get; set; }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        [PXInt]
        [PXDefault]
        public virtual int? SortOrder
        {
            get;
            set;
        }
        #endregion
        #region BranchID
        [Branch(typeof(Batch.branchID), Enabled = false)]
        public override int? BranchID { get; set; }
        #endregion
        #region BatchNbr
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, IsReadOnly = true)]
        public override string BatchNbr { get; set; }
        #endregion
        #region AccountID
        [Account(typeof(GLTran.branchID), LedgerID = typeof(GLTran.ledgerID), DescriptionField = typeof(Account.description), Enabled = false)]
        public override int? AccountID { get; set; }
        #endregion
        #region SubID
        [SubAccount(typeof(GLTran.accountID), typeof(GLTran.branchID), true, Enabled = false)]
        public override int? SubID { get; set; }
        #endregion
        #region CuryDebitAmt
        [PXDBCurrency(typeof(GLTran.curyInfoID), typeof(GLTran.debitAmt))]
        [PXUIField(DisplayName = "Debit Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public override decimal? CuryDebitAmt { get; set; }
        #endregion
        #region CuryCreditAmt
        [PXDBCurrency(typeof(GLTran.curyInfoID), typeof(GLTran.creditAmt))]
        [PXUIField(DisplayName = "Credit Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public override decimal? CuryCreditAmt { get; set; }
        #endregion
        #region TranDesc
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Transaction Description", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public override string TranDesc { get; set; }
        #endregion
        #region FinPeriodID
        [FinPeriodID]
        [PXUIField(DisplayName = "Post Period", Enabled = false)]
        public override string FinPeriodID { get;  set; }
        #endregion
        #region IsParent
        public abstract class isParent : PX.Data.BQL.BqlBool.Field<isParent> { }

        [PXBool]
        public bool? IsParent { get; set; }
		#endregion
		#region IsSplited
		public abstract class isSplited : PX.Data.BQL.BqlBool.Field<isSplited> { }

		[PXBool]
		public bool? IsSplited { get; set; }
		#endregion
		#region IsCurrent
		public abstract class isCurrent : PX.Data.BQL.BqlBool.Field<isCurrent> { }

        [PXBool]
        public bool? IsCurrent { get; set; }
        #endregion

        public GLTranReclHist ParentTran { get; set; }

        private List<GLTranReclHist> _ChildTrans;
        public List<GLTranReclHist> ChildTrans
        {
            get
            {
                if(_ChildTrans == null)
                {
                    _ChildTrans = new List<GLTranReclHist>();
                }

                return _ChildTrans;
            }
            set
            {
                _ChildTrans = value;
            }
        }
    }
}
