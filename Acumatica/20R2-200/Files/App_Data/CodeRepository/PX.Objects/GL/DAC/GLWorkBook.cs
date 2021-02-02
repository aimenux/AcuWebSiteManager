using System;
using System.Collections;
using System.Collections.Generic;
using PX.CS;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.GL
{
    /// <summary>
    /// Contains the main properties of workbooks.
    /// Workbooks are edited on the Workbooks (GL107500) form (which corresponds to the <see cref="GLWorkBookMaint"/> graph).
    /// </summary>
    [Serializable]
	[PXCacheName(Messages.GLWorkBook)]
	public partial class GLWorkBook : PX.Data.IBqlTable
	{
		#region WorkBookID
		public abstract class workBookID : PX.Data.BQL.BqlString.Field<workBookID> { }

        /// <summary>
        /// !REV!
        /// The voucher code, which is unique for the selected <see cref="Module">module</see> and <see cref="TranType">type</see> of the document.
        /// The code is selected by a user on the Journal Vouchers (GL304000) form (<see cref="GLWorkBook.WorkBookID"/> field)
        /// when the user enters the lines of the batch. The code determines the module and type of the document or transaction
        /// to be created from the corresponding line of the document batch.
        /// The field identifies the record of the GLWorkBook DAC associated with the <see cref="GLWorkBook">line</see> of the <see cref="GLDocBatch">document batch</see>.
        /// Only one code can be created for any combination of the <see cref="Module">module</see> and the <see cref="TranType">document or transaction type</see>.
        /// </summary>
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault]
		[PXSelector(typeof(Search<GLWorkBook.workBookID>))]
		[PXUIField(DisplayName = "Workbook ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string WorkBookID
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
        [PXDBString(2, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[VoucherModule.List]
		[PXFieldDescription]
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
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Transaction Type", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual string DocType
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
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region VoucherBatchNumberingID
		public abstract class voucherBatchNumberingID : PX.Data.BQL.BqlString.Field<voucherBatchNumberingID> { }

		/// <summary>
		/// The identifier of the <see cref="Numbering">Numbering Sequence</see> used for batches.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Numbering.NumberingID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Voucher Batch")]
		[PXSelector(typeof(Numbering.numberingID),
			DescriptionField = typeof(Numbering.descr))]
		public virtual string VoucherBatchNumberingID
		{
			get;
			set;
		}
		#endregion
		#region VoucherNumberingID
		public abstract class voucherNumberingID : PX.Data.BQL.BqlString.Field<voucherNumberingID> { }

        /// <summary>
        /// The identifier of the <see cref="Numbering">numbering sequence</see> that is used for the <see cref="GLDocBatch">batches of documents</see>
        /// created on the Journal Vouchers (GL304000) form. 
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Voucher")]
		[PXSelector(typeof(Numbering.numberingID),
			DescriptionField = typeof(Numbering.descr))]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, Equal<False>>), Messages.ManualNumberingDisabled)]
		public virtual string VoucherNumberingID
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlShort.Field<status> { }

        /// <summary>
        /// The status of the workbook.
        /// </summary>
        /// <value>
        /// The field can have one of the values described in <see cref="WorkBookStatus"/>.
        /// </value>
        [PXDBShort]
		[PXIntList(new int[] { WorkBookStatus.Active, WorkBookStatus.Inactive, WorkBookStatus.Hidden },
				new string[] { WorkBookStatus.ActiveName, WorkBookStatus.InactiveName, WorkBookStatus.HiddenName })]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual short? Status
		{
			get;
			set;
		}
		#endregion
		#region ReversingType
		public abstract class reversingType : PX.Data.BQL.BqlString.Field<reversingType> { }

        /// <summary>
        /// The type of documents, which reverses documents in this workbook.
        /// The field is on-the-fly generated and cannot be typed manually.
        /// </summary>
        /// <value>
        /// Depends on <see cref="GLWorkBook.Module"/> and <see cref="GLWorkBook.DocType"/> fields.
        /// </value>
		[PXString(3, IsFixed = true)]
		public virtual string ReversingType
		{
			get
			{
				if (Module == BatchModule.AR && DocType == ARDocType.Invoice)
				{
					return ARDocType.CreditMemo;
				}

				if (Module == BatchModule.AP && DocType == APDocType.Invoice)
				{
					return APDocType.DebitAdj;
				}

				if (Module == BatchModule.CA && DocType == CATranType.CAAdjustment)
				{
					return CATranType.CAAdjustment;
				}

				if (Module == BatchModule.GL && DocType == GLTranType.GLEntry)
				{
					return GLTranType.GLEntry;
				}

				return null;
			}
		}
		#endregion
		#region ReversingWorkBookID
		public abstract class reversingWorkBookID : PX.Data.BQL.BqlString.Field<reversingWorkBookID> { }

        /// <summary>
        /// The identifier of the reversing <see cref="GLWorkBook">workbook</see>.
        /// </summary>
        /// <value>
        /// The value of the field can be empty.
        /// </value>
        [PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
	    [PXSelector(
	        typeof(Search<GLWorkBook.workBookID, Where<GLWorkBook.docType, Equal<Current<GLWorkBook.reversingType>>,
	            And<GLWorkBook.module, Equal<Current<GLWorkBook.module>>,
	                And<GLWorkBook.status, Equal<WorkBookStatus.active>>>>>))]
	    [PXUIField(DisplayName = "Reversing Workbook ID", Visibility = PXUIVisibility.SelectorVisible)]
	    public virtual string ReversingWorkBookID
	    {
	        get;
            set;
	    }
		#endregion
		#region IsReversable
		public virtual bool IsReversable => ReversingType != null;
		#endregion
		#region VoucherEditScreen

	    public abstract class voucherEditScreen : PX.Data.BQL.BqlGuid.Field<voucherEditScreen> { }

        /// <summary>
        /// The global unique identifier of the form that is used for editing vouchers in the workbook.
        /// </summary>
	    [PXDBGuid]
	    [PXUIField(DisplayName = "Voucher Entry Form", Required = true)]
	    [PXDefault]
        [PXSelector(typeof(Search<SiteMap.nodeID, Where<SiteMap.screenID, IsNotNull>, OrderBy<Asc<SiteMap.title>>>),
            typeof(SiteMap.title), typeof(SiteMap.screenID), DescriptionField = typeof(SiteMap.title))]
	    public Guid? VoucherEditScreen
	    {
	        get;
            set;
	    }
		#endregion
		#region DefaultDescription
		public abstract class defaultDescription : PX.Data.BQL.BqlString.Field<defaultDescription> { }

		/// <summary>
		/// Default description for the voucher document.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DefaultDescription
		{
			get;
			set;
		}
		#endregion
		#region DefaultCashAccountID
		public abstract class defaultCashAccountID : PX.Data.BQL.BqlInt.Field<defaultCashAccountID> { }

        /// <summary>
        /// The identifier of the default <see cref="CashAccount">cash account</see> for the workbook.
        /// The field is empty by default and manually typed by the user.
        /// The field is visible and enabled for editing if the value of the <see cref="GLWorkBook.Module"/> field equals <see cref="BatchModule.CA"/>.
        /// (This behavior is described in the <see cref="GLWorkBookMaint.GLWorkBook_RowSelected"/> event.)
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
        /// </value>
        [CashAccount(null, typeof(Search<CashAccount.cashAccountID, Where2<Match<Current<AccessInfo.userName>>, And<CashAccount.clearingAccount, Equal<False>>>>), Visible = false)]
		public virtual int? DefaultCashAccountID
		{
			get;
			set;
		}
		#endregion
		#region DefaultEntryTypeID
		public abstract class defaultEntryTypeID : PX.Data.BQL.BqlString.Field<defaultEntryTypeID> { }

        /// <summary>
        /// The identifier of the default <see cref="CAEntryType">entry type</see> for the workbook.
        /// The field is empty by default and manually typed by the user.
        /// /// The field is visible and enabled for editing if the value of the <see cref="GLWorkBook.Module"/> field equals <see cref="BatchModule.CA"/>.
        /// (This behavior is described in the <see cref="GLWorkBookMaint.GLWorkBook_RowSelected"/> event.)
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CAEntryType.EntryTypeId"/> field.
        /// Depends on the <see cref="CashAccount.CashAccountID"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId, InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
									Where<CashAccountETDetail.accountID, Equal<Current<GLWorkBook.defaultCashAccountID>>, And<CAEntryType.module, Equal<BatchModule.moduleCA>>>>), DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Default Entry Type", Visible = false)]
		public virtual string DefaultEntryTypeID
		{
			get;
			set;
		}
		#endregion
		#region DefaultBAccountID
		public abstract class defaultBAccountID : PX.Data.BQL.BqlInt.Field<defaultBAccountID> { }

        /// <summary>
        /// The identifier of the default <see cref="BAccount">business account</see> for the workbook.
        /// The field is invisible and disabled for editing.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="BAccount.BAccountID"/> field.
        /// The value of the field is set from <see cref="GLWorkBook.DefaultVendorID"/> and <see cref="GLWorkBook.DefaultCustomerID"/> fields.
        /// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Business Account", Visible = false)]
		public virtual int? DefaultBAccountID
		{
			get;
			set;
		}
		#endregion
		#region DefaultVendorID
		public abstract class defaultVendorID : PX.Data.BQL.BqlInt.Field<defaultVendorID> { }

        /// <summary>
        /// The identifier of the default <see cref="Vendor">vendor</see> for the workbook.
        /// The field is empty by default and manually typed by the user.
        /// The field is visible and enabled for editing if the value of the <see cref="GLWorkBook.Module"/> field equals <see cref="BatchModule.AP"/>.
        /// (This behavior is described in the <see cref="GLWorkBookMaint.GLWorkBook_RowSelected"/> event.)
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Vendor.BAccountID"/> field.
        /// </value>
        [PXInt]
		[PXSelector(typeof(Search<AP.Vendor.bAccountID>), typeof(AP.Vendor.acctCD), typeof(AP.Vendor.acctName),
			SubstituteKey = typeof(AP.Vendor.acctCD), DescriptionField = typeof(AP.Vendor.acctName))]
		[PXUIField(DisplayName = AP.Messages.Vendor, Visible = false)]
		public virtual int? DefaultVendorID
		{
            [PXDependsOnFields(typeof (GLWorkBook.defaultBAccountID))]
            get
            {
                return DefaultBAccountID;
            }
            set
            {
                DefaultBAccountID = value;
            }
        }
		#endregion
		#region DefaultCustomerID
		public abstract class defaultCustomerID : PX.Data.BQL.BqlInt.Field<defaultCustomerID> { }

        /// <summary>
        /// The identifier of the default <see cref="Customer">customer</see> for the workbook.
        /// The field is empty by default and manually typed by the user.
        /// The field is visible and enabled for editing if the value of the <see cref="GLWorkBook.Module"/> field equals <see cref="BatchModule.AR"/>.
        /// (This behavior is described in the <see cref="GLWorkBookMaint.GLWorkBook_RowSelected"/> event.)
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Customer.BAccountID"/> field.
        /// </value>
        [PXInt]
		[PXSelector(typeof(Search<AR.Customer.bAccountID>), typeof(AR.Customer.acctCD), typeof(AR.Customer.acctName),
			SubstituteKey = typeof(AR.Customer.acctCD), DescriptionField = typeof(AR.Customer.acctName))]
		[PXUIField(DisplayName = AR.Messages.Customer, Visible = false)]
		public virtual int? DefaultCustomerID
		{
			[PXDependsOnFields(typeof(GLWorkBook.defaultBAccountID))]
            get
            {
                return DefaultBAccountID;
            }
            set
            {
                DefaultBAccountID = value;
            }
        }
		#endregion
		#region DefaultLocationID
		public abstract class defaultLocationID : PX.Data.BQL.BqlInt.Field<defaultLocationID> { }

        /// <summary>
        /// The identifier of the default <see cref="Location">location</see> for the workbook.
        /// The field may be empty.
        /// The field is visible and enabled for editing if the value of the <see cref="GLWorkBook.Module"/> field equals <see cref="BatchModule.AP"/> or <see cref="BatchModule.AR"/>.
        /// (This behavior is described in the <see cref="GLWorkBookMaint.GLWorkBook_RowSelected"/> event.)
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Location.LocationID"/> field.
        /// Depends on the <see cref="GLWorkBook.DefaultBAccountID"/> field.
        /// </value>
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<GLWorkBook.defaultBAccountID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr), Visible = false, FieldClass = "LOCATION")]
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<GLWorkBook.defaultBAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? DefaultLocationID
		{
			get;
			set;
		}
		#endregion
		#region SingleOpenVoucherBatch
		public abstract class singleOpenVoucherBatch : PX.Data.BQL.BqlBool.Field<singleOpenVoucherBatch> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the only one unreleased voucher batch is allowed for the workbook.
        /// </summary>
        [PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.AllowOnlyOneUnreleasedVoucherBatchPerWorkbook)]
		public virtual bool? SingleOpenVoucherBatch
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