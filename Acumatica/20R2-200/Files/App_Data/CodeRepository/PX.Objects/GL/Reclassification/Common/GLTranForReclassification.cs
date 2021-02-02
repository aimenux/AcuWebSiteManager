using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL.Attributes;

namespace PX.Objects.GL.Reclassification.Common
{
	[PXBreakInheritance]
	public class GLTranForReclassification : GLTran
	{
		public GLTranForReclassification()
		{
			FieldsErrorForInvalidFromValues = new Dictionary<string, ExceptionAndErrorValuesTriple>(4);
		}
        #region SplittedIcon

        public abstract class splittedIcon : PX.Data.BQL.BqlString.Field<splittedIcon> { }

        [PXUIField(DisplayName = "", IsReadOnly = true, Visible = false)]
        [PXImage]
        public virtual string SplittedIcon { get; set; }
        #endregion
        #region DebitAmt

        [PXDBBaseCury(typeof(GLTran.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DebitAmt
		{
			get
			{
				return this._DebitAmt;
			}
			set
			{
				this._DebitAmt = value;
			}
		}
		#endregion
		#region CreditAmt
		[PXDBBaseCury(typeof(GLTran.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? CreditAmt
		{
			get
			{
				return this._CreditAmt;
			}
			set
			{
				this._CreditAmt = value;
			}
		}
		#endregion
		#region CuryDebitAmt
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Amount", Visibility = PXUIVisibility.Visible)]
		[PXDBCurrency(typeof(GLTran.curyInfoID), typeof(GLTran.debitAmt))]
		public override Decimal? CuryDebitAmt
		{
			get
			{
				return this._CuryDebitAmt;
			}
			set
			{
				this._CuryDebitAmt = value;
			}
		}
		#endregion
		#region CuryCreditAmt
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Amount", Visibility = PXUIVisibility.Visible)]
		[PXDBCurrency(typeof(GLTran.curyInfoID), typeof(GLTran.creditAmt))]
		public override Decimal? CuryCreditAmt
		{
			get
			{
				return this._CuryCreditAmt;
			}
			set
			{
				this._CuryCreditAmt = value;
			}
		}
		#endregion
		#region LedgerID

		[PXDBInt]
		[PXDefault]
		public override Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}

		#endregion
		#region ReferenceID
		public new abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDBInt()]
		[PXSelector(typeof(Search<BAccountR.bAccountID>),
			 typeof(BAccountR.bAccountID),
			 typeof(BAccountR.acctName),
			 typeof(BAccountR.type),
			SubstituteKey = typeof(BAccountR.acctCD))]
		[CustomerVendorRestrictor]
		[PXUIField(DisplayName = "Customer/Vendor", Enabled = false, Visible = false)]
		public override Int32? ReferenceID
		{
			get
			{
				return this._ReferenceID;
			}
			set
			{
				this._ReferenceID = value;
			}
		}
		#endregion
		
		#region NewBranchID

		public abstract class newBranchID : PX.Data.BQL.BqlInt.Field<newBranchID> { }

		protected Int32? _NewBranchID;

		[Branch(useDefaulting: false, DisplayName = "To Branch", IsDBField = false)]
		public virtual Int32? NewBranchID
		{
			get { return this._NewBranchID; }
			set { this._NewBranchID = value; }
		}

		#endregion
		#region NewAccountID

		public abstract class newAccountID : PX.Data.BQL.BqlInt.Field<newAccountID> { }

		protected Int32? _NewAccountID;

		[Account(typeof(newBranchID),
			LedgerID = typeof(ledgerID),
			DescriptionField = typeof(Account.description),
			DisplayName = "To Account", IsDBField = false,
			AvoidControlAccounts = true)]
		public virtual Int32? NewAccountID
		{
			get { return this._NewAccountID; }
			set { this._NewAccountID = value; }
		}

		#endregion
		#region NewSubID

		public abstract class newSubID : PX.Data.BQL.BqlInt.Field<newSubID> { }

		protected Int32? _NewSubID;

		[SubAccount(typeof(GLTranForReclassification.newAccountID), typeof(GLTranForReclassification.newBranchID),
			DisplayName = "To Subaccount", IsDBField = false)]
		public virtual Int32? NewSubID
		{
			get { return this._NewSubID; }
			set { this._NewSubID = value; }
		}

        #endregion
        #region NewSubID
        public virtual string NewSubCD { get; set; }
        #endregion
        #region NewTranDate

        public abstract class newTranDate : PX.Data.BQL.BqlDateTime.Field<newTranDate> { }

		protected DateTime? _NewTranDate;

		[PXDate]
		[PXUIField(DisplayName = "New Tran. Date")]
		public virtual DateTime? NewTranDate
		{
			get { return this._NewTranDate; }
			set { this._NewTranDate = value; }
		}

		#endregion
		#region NewFinPeriodID

		public abstract class newFinPeriodID : PX.Data.BQL.BqlString.Field<newFinPeriodID> { }

		protected String _NewFinPeriodID;

		//Used only for validation
		[OpenPeriod(null, 
		    typeof(GLTranForReclassification.newTranDate), 
		    typeof(GLTranForReclassification.newBranchID), 
		    IsDBField = false, 
		    RedefaultOnDateChanged = false)]
		public virtual String NewFinPeriodID
		{
			get { return this._NewFinPeriodID; }
			set { this._NewFinPeriodID = value; }
		}

		#endregion
		#region NewTranDesc
		public abstract class newTranDesc : PX.Data.BQL.BqlString.Field<newTranDesc> { }
		protected String _NewTranDesc;

		[PXString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "New Transaction Description")]
		public virtual String NewTranDesc
		{
			get
			{
				return this._NewTranDesc;
			}
			set
			{
				this._NewTranDesc = value;
			}
		}
        #endregion
        #region  CuryNewAmt
        public abstract class curyNewAmt : PX.Data.BQL.BqlDecimal.Field<curyNewAmt> { }

        [PXDefault]
        [PXCurrency(typeof(GLTran.curyInfoID), typeof(newAmt))]
        [PXUIField(DisplayName = "New Amount", Visible = false)]
        public virtual Decimal? CuryNewAmt { get; set; }
        #endregion
        #region  NewAmt
        public abstract class newAmt : PX.Data.BQL.BqlDecimal.Field<newAmt> { }

        [PXBaseCury(typeof(GLTran.ledgerID))]
        public virtual Decimal? NewAmt { get; set; }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

		[PXString]
		[PXUIField(DisplayName = "Currency")]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
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
        #region SourceCuryDebitAmt
        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? SourceCuryDebitAmt { get; set; }
        #endregion
        #region SourceCuryCreditAmt
        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? SourceCuryCreditAmt { get; set; }
        #endregion

        public virtual ReclassRowTypes ReclassRowType { get; set; }
		public virtual int? EditingPairReclassifyingLineNbr { get; set; }
		
		public virtual bool VerifyingForFromValuesInvoked { get; set; }
		public virtual Dictionary<string, ExceptionAndErrorValuesTriple> FieldsErrorForInvalidFromValues { get; set; }

        public virtual GLTranKey ParentKey { get; set; }

        public bool IsSplitting => ParentKey != null;

        /// <summary>
        /// This field is used for UI only.
        /// </summary>
        public bool IsSplitted { get; set; }

		public class ExceptionAndErrorValuesTriple
		{
			public Exception Error;
			public object ErrorValue;
			public object ErrorUIValue;
		}
    }
}
