using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CM;
using PX.Objects.CS;


namespace PX.Objects.GL
{
    /// <summary>
    /// Contains the main properties of GL voucher batches.
    /// GL voucher batches are edited on the Workbooks (GL307000) form (which corresponds to the <see cref="GLVoucherBatchEntry"/> graph).
    /// </summary>
    [System.SerializableAttribute]
	[PXCacheName(Messages.GLVoucherBatch)]
	public partial class GLVoucherBatch : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        /// <summary>
        /// Indicates whether the record is selected for processing.
        /// </summary>
        [PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
            get;
            set;
        }
		#endregion
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
		[PXDefault]
		[PXUIField(DisplayName = "Workbook ID", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[AutoNumber(typeof(Search<GLWorkBook.voucherBatchNumberingID, Where<GLWorkBook.workBookID, Equal<Current<GLVoucherBatch.workBookID>>>>), typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = Messages.VoucherBatchNbr, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string VoucherBatchNbr
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		/// <summary>
		/// Description of the entry code.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }

		/// <summary>
		/// The code of the module where a document or transaction will be generated according to this entry code.
		/// </summary>
		/// <value>
		/// Allowed values are: <c>"GL"</c>, <c>"AP"</c>, <c>"AR"</c> and <c>"CA"</c>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(Search<GLWorkBook.module, Where<GLWorkBook.workBookID, Equal<Current<GLVoucherBatch.workBookID>>>>))]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		/// <summary>
		/// The type of the document or transaction generated according to the code.
		/// </summary>
		/// <value>
		/// Allowed values set depends on the selected <see cref="Module"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXDefault(typeof(Search<GLWorkBook.docType, Where<GLWorkBook.workBookID, Equal<Current<GLVoucherBatch.workBookID>>>>))]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		/// <summary>
		/// The counter of the document lines, used <i>internally</i> to assign consistent numbers to newly created lines.
		/// It is not recommended to rely on this field to determine the exact count of lines, because it might not reflect the latter under some conditions.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntr
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		/// <summary>
		/// Indicates whether the batch has been released.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
		#region NotReleased
		public abstract class notReleased : PX.Data.BQL.BqlBool.Field<notReleased> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the voucher batch is not released.
        /// The field is virtual and has no representation in the database.
        /// The field is used to enable or disable the Delete button on the Workbooks (GL307000) form.
        /// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Not Released", Visible = false, Visibility = PXUIVisibility.Invisible, Enabled = false)]
		public virtual bool? NotReleased
		{
			[PXDependsOnFields(typeof(released))]
			get
			{
				return this.Released == false;
			}
		}
		#endregion
		#region DocCount
		public abstract class docCount : PX.Data.BQL.BqlInt.Field<docCount> { }

		/// <summary>
		/// Represents number of documents included into the batch. 	
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Documents Count", Enabled = false)]
		public virtual int? DocCount
		{
			get;
			set;
		}
		#endregion
		#region BatchTotal
		public abstract class batchTotal : PX.Data.BQL.BqlDecimal.Field<batchTotal> { }

        /// <summary>
        /// Represents the batch total amount in the base currency.
        /// The field has not been implemented yet.
        /// </summary>
        [PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Batch Total Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual decimal? BatchTotal
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}