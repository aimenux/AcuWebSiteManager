using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;


namespace PX.Objects.GL
{
    /// <summary>
    /// Contains the main properties of GL vouchers.
    /// GL Vouchers are edited on derivative forms of following forms:
    /// Journal Transactions (GL301000) form (which corresponds to <see cref="JournalEntryExt"/> and <see cref="PostGraphExt"/> graphs),
    /// Transactions (CA304000) form (which corresponds to the <see cref="CA.CATranEntryExt"/> graph),
    /// Bills and Adjustments (AP301000) form (which corresponds to the <see cref="AP.APInvoiceEntryExt"/> graph),
    /// Checks and Payments (AP302000) form (which corresponds to the <see cref="AP.APPaymentEntryExt"/> graph),
    /// Quick Checks (AP304000) form (which corresponds to the <see cref="AP.APQuickCheckEntryExt"/> graph),
    /// Invoices and Memos (AR301000) form (which corresponds to the <see cref="AR.ARInvoiceEntryExt"/> graph), 
    /// Payments and Applications (AR302000) form (which corresponds to the <see cref="AR.ARPaymentEntryExt"/> graph),
    /// Cash Sales (AR304000) form (which corresponds to the <see cref="AR.ARCashSaleEntryExt"/> graph).
    /// </summary>
    [System.SerializableAttribute]
	[PXCacheName(Messages.GLVoucher)]
	public partial class GLVoucher: PX.Data.IBqlTable
	{
		#region WorkBookID
		public abstract class workBookID : PX.Data.BQL.BqlString.Field<workBookID> { }

		/// <summary>
		/// !REV!
		/// The unique voucher code for the selected <see cref="Module"/> and <see cref="TranType">Type</see> of the document.
		/// The code is selected by user on the Journal Vouchers (GL.30.40.00) screen (<see cref="GLWorkBookID.WorkBookID"/> field)
		/// when entering the lines of the batch and determines the module and type of the document or transaction
		/// to be created from the corresponding line of the document batch.
		/// Identifies the record of this DAC associated with a <see cref="GLWorkBookID">line</see> of a <see cref="GLDocBatch">document batch</see>.
		/// Only one code can be created for any combination of <see cref="Module"/> and <see cref="TranType">Document/Transaction Type</see>.
		/// </summary>
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa", IsKey = true)]
		[PXDBDefault(typeof(GLVoucherBatch))]		
		[PXUIField(DisplayName = "Workbook ID", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string WorkBookID
		{
			get;
			set;
		}
		#endregion
		#region VoucherBatchNbr
		public abstract class voucherBatchNbr : PX.Data.BQL.BqlString.Field<voucherBatchNbr> { }

		/// <summary>
		/// !REV!
		/// The unique voucher code for the selected <see cref="Module"/> and <see cref="TranType">Type</see> of the document.
		/// The code is selected by user on the Journal Vouchers (GL.30.40.00) screen (<see cref="GLVoucherBatchNbr.VoucherBatchNbr"/> field)
		/// when entering the lines of the batch and determines the module and type of the document or transaction
		/// to be created from the corresponding line of the document batch.
		/// Identifies the record of this DAC associated with a <see cref="GLVoucherBatchNbr">line</see> of a <see cref="GLDocBatch">document batch</see>.
		/// Only one code can be created for any combination of <see cref="Module"/> and <see cref="TranType">Document/Transaction Type</see>.
		/// </summary>
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsKey = true)]		
		[PXDBDefault(typeof(GLVoucherBatch))]
		[PXParent(typeof(Select<GLVoucherBatch, Where<GLVoucherBatch.workBookID, Equal<Current<GLVoucher.workBookID>>,
								And<GLVoucherBatch.voucherBatchNbr, Equal<Current<GLVoucher.voucherBatchNbr>>>>>))]       
		
		[PXUIField(DisplayName = Messages.VoucherBatchNbr, Visibility = PXUIVisibility.SelectorVisible, Visible=false)]
		public virtual string VoucherBatchNbr
		{
			get;
			set;
		}
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// Key field.
		/// The number of the document/transaction line inside the <see cref="GLDocBatch">document batch</see>.
		/// </summary>
		/// <value>
		/// Note that the sequence of line numbers of the transactions belonging to a single document may include gaps.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXLineNbr(typeof(GLVoucherBatch.lineCntr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion      
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }

        /// <summary>
        /// The code of the module where a document or transaction should be generated.
        /// </summary>
        /// <value>
        /// The field can have one of the following values:
        /// <c>"GL"</c>: General Ledger,
        /// <c>"AP"</c>: Accounts Payable,
        /// <c>"AR"</c>: Accounts Receivable,
        /// <c>"CA"</c>: Cash Managment.
        /// </value>
        [PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(Search<GLWorkBook.module, Where<GLWorkBook.workBookID, Equal<Current<GLVoucherBatch.workBookID>>>>))]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        /// <summary>
        /// The type of the document or transaction.
        /// </summary>
        /// <value>
        /// Depends on the value of the <see cref="Module"/> field.
        /// </value>
        [PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(Search<GLWorkBook.docType, Where<GLWorkBook.workBookID, Equal<Current<GLVoucherBatch.workBookID>>>>))]
		[PXUIField(DisplayName = "Module Tran. Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// Reference number of the document or transaction created from the line.
		/// </summary>
		/// <value>
		/// For the lines defining GL transactions the field corresponds to the <see cref="Batch.BatchNbr"/> field.
		/// For the lines used to create documents in AP and AR modules the field corresponds to the 
		/// <see cref="PX.Objects.AP.APRegister.RefNbr"/> and <see cref="PX.Objects.AR.ARRegister.RefNbr"/> fields, respectively.
		/// For the lines defining CA transactions the field corresponds to the <see cref="CAAdj.AdjRefNbr"/> field.
		/// </value>
		[PXFormula(null, typeof(CountCalc<GLVoucherBatch.docCount>))]		
		[PXDBString(15, IsUnicode = true)]		
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Ref. Number", Visible = true)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

	    [PXDBGuid]
	    public virtual Guid? RefNoteID
	    {
	        get;
            set;
	    }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion		
	}
}