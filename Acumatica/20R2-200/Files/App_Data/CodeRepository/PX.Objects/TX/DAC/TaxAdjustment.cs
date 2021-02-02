using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX.Descriptor;

namespace PX.Objects.TX
{

	public class TaxAdjustmentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { AdjustOutput, AdjustInput },
				new string[] { Messages.AdjustOutput, Messages.AdjustInput }) { }
		}

		public const string AdjustOutput = "INT";
		public const string AdjustInput = "RET";
		public const string InputVAT = "VTI";
		public const string OutputVAT = "VTO";
		public const string ReverseInputVAT = "REI";
		public const string ReverseOutputVAT = "REO";

		public class adjustOutput : PX.Data.BQL.BqlString.Constant<adjustOutput>
		{
			public adjustOutput() : base(AdjustOutput) { ;}
		}

		public class adjustInput : PX.Data.BQL.BqlString.Constant<adjustInput>
		{
			public adjustInput() : base(AdjustInput) { ;}
		}

		public class inputVAT : PX.Data.BQL.BqlString.Constant<inputVAT>
		{
			public inputVAT() : base(InputVAT) { ;}
		}

		public class outputVAT : PX.Data.BQL.BqlString.Constant<outputVAT>
		{
			public outputVAT() : base(OutputVAT) { ;}
		}

		public class reverseInputVAT : PX.Data.BQL.BqlString.Constant<reverseInputVAT>
		{
			public reverseInputVAT() : base(ReverseInputVAT) { ;}
		}

		public class reverseOutputVAT : PX.Data.BQL.BqlString.Constant<reverseOutputVAT>
		{
			public reverseOutputVAT() : base(ReverseOutputVAT) { ;}
		}
	}

	public class TaxAdjustmentStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Hold, Balanced, Released },
				new string[] { Messages.AdjHold, Messages.AdjBalanced, Messages.AdjReleased }) { ; }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Released = "C";

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { ;}
		}

		public class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released) { ;}
		}
	}
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxAdjustment)]
	public partial class TaxAdjustment : PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(TaxAdjustmentType.AdjustOutput)]
		[TaxAdjustmentType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<TaxAdjustment.refNbr, Where<TaxAdjustment.docType, Equal<Optional<TaxAdjustment.docType>>>>))]
		[AutoNumber(typeof(APSetup.invoiceNumberingID), typeof(TaxAdjustment.docDate))]
		[PX.Data.EP.PXFieldDescription]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion

		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		/// <summary>
		/// Reference number of the original (source) document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Orig. Ref. Nbr.")]
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXDefault]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[OpenPeriod(null,
			typeof(TaxAdjustment.docDate),
			typeof(TaxAdjustment.branchID),
			masterFinPeriodIDType: typeof(TaxAdjustment.tranPeriodID),
			IsHeader = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		
		[PeriodID]
		[PXUIField(DisplayName = "Master Period")]
		public virtual string TranPeriodID
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[TaxAgencyActive(Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }

		[LocationID(typeof(
			Where<Location.bAccountID, Equal<Current<TaxAdjustment.vendorID>>,
				And<Location.isActive, Equal<True>,
				And<MatchWithBranch<Location.vBranchID>>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(
			Search2<BAccountR.defLocationID,
			InnerJoin<Location, 
				On<Location.bAccountID, Equal<BAccountR.bAccountID>, 
				And<Location.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<TaxAdjustment.vendorID>>,
				And<Location.isActive, Equal<True>,
				And<MatchWithBranch<Location.vBranchID>>>>>))]
		public virtual int? VendorLocationID
		{
			get;
			set;
		}
		#endregion

		#region TaxPeriod
		public abstract class taxPeriod : PX.Data.BQL.BqlString.Field<taxPeriod> { }

		[TaxAdjsutmentTaxPeriodSelector]
		public virtual string TaxPeriod
		{
			get;
			set;
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
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
		#region AdjAccountID
		public abstract class adjAccountID : PX.Data.BQL.BqlInt.Field<adjAccountID> { }
		protected Int32? _AdjAccountID;
		[Account(DisplayName = "Adjustment Account", Visibility = PXUIVisibility.Visible, AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<Location.vExpenseAcctID, Where<Location.bAccountID, Equal<Current<TaxAdjustment.vendorID>>, And<Location.locationID, Equal<Current<TaxAdjustment.vendorLocationID>>>>>))]
		public virtual Int32? AdjAccountID
		{
			get
			{
				return this._AdjAccountID;
			}
			set
			{
				this._AdjAccountID = value;
			}
		}
		#endregion
		#region AdjSubID
		public abstract class adjSubID : PX.Data.BQL.BqlInt.Field<adjSubID> { }
		protected Int32? _AdjSubID;
		[SubAccount(typeof(TaxAdjustment.adjAccountID), DisplayName = "Adjustment Sub.", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search<Location.vExpenseSubID, Where<Location.bAccountID, Equal<Current<TaxAdjustment.vendorID>>, And<Location.locationID, Equal<Current<TaxAdjustment.vendorLocationID>>>>>))]
		public virtual Int32? AdjSubID
		{
			get
			{
				return this._AdjSubID;
			}
			set
			{
				this._AdjSubID = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXDBLong]
		[CurrencyInfo(ModuleCode = BatchModule.AP)]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		protected Decimal? _CuryOrigDocAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(TaxAdjustment.curyInfoID), typeof(TaxAdjustment.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryOrigDocAmt
		{
			get
			{
				return this._CuryOrigDocAmt;
			}
			set
			{
				this._CuryOrigDocAmt = value;
			}
		}
		#endregion
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		protected Decimal? _OrigDocAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigDocAmt
		{
			get
			{
				return this._OrigDocAmt;
			}
			set
			{
				this._OrigDocAmt = value;
			}
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		protected Decimal? _CuryDocBal;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(TaxAdjustment.curyInfoID), typeof(TaxAdjustment.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}
		#endregion
		#region DocBal
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		protected Decimal? _DocBal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		protected String _DocDesc;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DocDesc
		{
			get
			{
				return this._DocDesc;
			}
			set
			{
				this._DocDesc = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
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
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
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
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
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
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleGL>>>))]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		protected String _Status;

		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxAdjustmentStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[TaxAdjustmentStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(released), typeof(hold))]
			get
			{
				return this._Status;
			}
			set
			{
				//this._Status = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(Search<APSetup.holdEntry>))]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
				this.SetStatus();
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(TaxAdjustment.refNbr))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = TaxAdjustmentStatus.Hold;
			}
			else if (this._Released != null && !(bool)this._Released)
			{
				this._Status = TaxAdjustmentStatus.Balanced;
			}
			else 
			{
				this._Status = TaxAdjustmentStatus.Released;
			}
		}
		#endregion
	}
}
