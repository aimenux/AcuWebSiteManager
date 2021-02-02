namespace PX.Objects.SO
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.AR;
	using PX.Objects.CM;
	using PX.Objects.CS;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOAdjust)]
	public partial class SOAdjust : PX.Data.IBqlTable, IAdjustment
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOAdjust>.By<adjdOrderType, adjdOrderNbr, adjgDocType, adjgRefNbr>
		{
			public static SOAdjust Find(PXGraph graph, string adjdOrderType, string adjdOrderNbr, string adjgDocType, string adjgRefNbr)
				=> FindBy(graph, adjdOrderType, adjdOrderNbr, adjgDocType, adjgRefNbr);
		}
		public static class FK
		{
			public class Order : SOOrder.PK.ForeignKeyOf<SOAdjust>.By<adjdOrderType, adjdOrderNbr> { }
		}
		#endregion

		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDBInt()]
		[PXDefault(typeof(ARPayment.customerID))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		protected string _AdjgDocType;
		public const int AdjgDocTypeLength = 3;
		[PXDBString(AdjgDocTypeLength, IsKey = true, IsFixed = true, InputMask = "")]
		[ARPaymentType.List()]
		[PXDefault(typeof(ARPayment.docType))]
		[PXUIField(DisplayName = "Doc. Type", Enabled = false, Visible = false)]
		public virtual String AdjgDocType
		{
			get
			{
				return this._AdjgDocType;
			}
			set
			{
				this._AdjgDocType = value;
			}
		}
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		protected string _AdjgRefNbr;
		public const int AdjgRefNbrLength = 15;
		[PXDBString(AdjgRefNbrLength, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARPayment.refNbr), DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false, Visible = false)]
		[PXParent(typeof(Select<ARPayment, Where<ARPayment.docType, Equal<Current<SOAdjust.adjgDocType>>, And<ARPayment.refNbr, Equal<Current<SOAdjust.adjgRefNbr>>>>>))]
		public virtual String AdjgRefNbr
		{
			get
			{
				return this._AdjgRefNbr;
			}
			set
			{
				this._AdjgRefNbr = value;
			}
		}
		#endregion
		#region AdjdOrderType
		public abstract class adjdOrderType : PX.Data.BQL.BqlString.Field<adjdOrderType> { }
		protected String _AdjdOrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(SOOrderTypeConstants.SalesOrder)]
		[PXUIField(DisplayName = "Order Type")]
		[PXSelector(typeof(Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<True>, And<Where<SOOrderType.aRDocType, Equal<ARDocType.invoice>, Or<SOOrderType.aRDocType, Equal<ARDocType.debitMemo>>>>>>))]
		public virtual String AdjdOrderType
		{
			get
			{
				return this._AdjdOrderType;
			}
			set
			{
				this._AdjdOrderType = value;
			}
		}
		#endregion
		#region AdjdOrderNbr
		public abstract class adjdOrderNbr : PX.Data.BQL.BqlString.Field<adjdOrderNbr> { }
		protected String _AdjdOrderNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXParent(typeof(FK.Order))]
		[PXUnboundFormula(typeof(Switch<Case<Where<SOAdjust.curyAdjdAmt, Greater<decimal0>>, int1>, int0>), typeof(SumCalc<SOOrder.paymentCntr>))]
        [PXRestrictor(typeof(Where<SOOrder.status,NotEqual<SOOrderStatus.cancelled>, And< SOOrder.status, NotEqual<SOOrderStatus.pendingApproval>,
            And<SOOrder.status, NotEqual<SOOrderStatus.voided>>>>),Messages.DontApprovedDocumentsCannotBeSelected)]
        [PXSelector(typeof(Search2<SOOrder.orderNbr, 
			InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrder.orderType>>,
			InnerJoin<Terms, On<Terms.termsID, Equal<SOOrder.termsID>>>>,
		    Where2<Where<SOOrder.customerID, Equal<Current<SOAdjust.customerID>>, Or<SOOrder.customerID, In2<Search<Customer.bAccountID, Where<Customer.consolidatingBAccountID, Equal<Current<SOAdjust.customerID>>>>>>>,
			  And<SOOrder.orderType, Equal<Optional<SOAdjust.adjdOrderType>>, 
			  And<SOOrder.openDoc, Equal<boolTrue>, 
			  And2<Where<SOOrderType.aRDocType, Equal<ARDocType.invoice>, Or<SOOrderType.aRDocType, Equal<ARDocType.debitMemo>>>,
			  And<SOOrder.orderDate, LessEqual<Current<ARPayment.adjDate>>,
			  And<Terms.installmentType, NotEqual<TermsInstallmentType.multiple>>>>>>>>),
				typeof(SOOrder.orderNbr),
				typeof(SOOrder.orderDate),
				typeof(SOOrder.finPeriodID),
				typeof(SOOrder.customerLocationID),
				typeof(SOOrder.curyID),
				typeof(SOOrder.curyOrderTotal),
				typeof(SOOrder.curyOpenOrderTotal),
				typeof(SOOrder.status),
				typeof(SOOrder.dueDate),
				typeof(SOOrder.invoiceNbr),
				typeof(SOOrder.orderDesc),
				Filterable = true)]
		public virtual String AdjdOrderNbr
		{
			get
			{
				return this._AdjdOrderNbr;
			}
			set
			{
				this._AdjdOrderNbr = value;
			}
		}
        #endregion
        #region CuryAdjgAmt
        public abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
		protected Decimal? _CuryAdjgAmt;
		[PXDBCurrency(typeof(SOAdjust.adjgCuryInfoID), typeof(SOAdjust.adjAmt))]
		[PXFormula(null, typeof(SumCalc<ARPayment.curySOApplAmt>))]
		[PXFormula(typeof(Sub<SOAdjust.curyOrigAdjgAmt, SOAdjust.curyAdjgBilledAmt>))]
		[PXUIField(DisplayName = "Applied To Order")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjgAmt
		{
			get
			{
				return this._CuryAdjgAmt;
			}
			set
			{
				this._CuryAdjgAmt = value;
			}
		}
		#endregion
		#region AdjAmt
		public abstract class adjAmt : PX.Data.BQL.BqlDecimal.Field<adjAmt> { }
		protected Decimal? _AdjAmt;
		[PXDBDecimal(4)]
		[PXFormula(typeof(Sub<SOAdjust.origAdjAmt, SOAdjust.adjBilledAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjAmt
		{
			get
			{
				return this._AdjAmt;
			}
			set
			{
				this._AdjAmt = value;
			}
		}
		#endregion
		#region CuryAdjdAmt
		public abstract class curyAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdAmt> { }
		protected Decimal? _CuryAdjdAmt;
		[PXDBDecimal(4)]
		[PXFormula(typeof(Sub<SOAdjust.curyOrigAdjdAmt, SOAdjust.curyAdjdBilledAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjdAmt
		{
			get
			{
				return this._CuryAdjdAmt;
			}
			set
			{
				this._CuryAdjdAmt = value;
			}
		}
		#endregion

		/**/
		#region CuryOrigAdjdAmt
		public abstract class curyOrigAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigAdjdAmt> { }
		protected Decimal? _CuryOrigAdjdAmt;
		[PXDBCalced(typeof(Add<SOAdjust.curyAdjdAmt, SOAdjust.curyAdjdBilledAmt>), typeof(decimal))]
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryOrigAdjdAmt
		{
			get
			{
				return this._CuryOrigAdjdAmt;
			}
			set
			{
				this._CuryOrigAdjdAmt = value;
			}
		}
		#endregion
		#region OrigAdjAmt
		public abstract class origAdjAmt : PX.Data.BQL.BqlDecimal.Field<origAdjAmt> { }
		protected Decimal? _OrigAdjAmt;
		[PXDBCalced(typeof(Add<SOAdjust.adjAmt, SOAdjust.adjBilledAmt>), typeof(decimal))]
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigAdjAmt
		{
			get
			{
				return this._OrigAdjAmt;
			}
			set
			{
				this._OrigAdjAmt = value;
			}
		}
		#endregion
		#region CuryOrigAdjgAmt
		public abstract class curyOrigAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigAdjgAmt> { }
		protected Decimal? _CuryOrigAdjgAmt;
		[PXDBCalced(typeof(Add<SOAdjust.curyAdjgAmt, SOAdjust.curyAdjgBilledAmt>), typeof(decimal))]
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryOrigAdjgAmt
		{
			get
			{
				return this._CuryOrigAdjgAmt;
			}
			set
			{
				this._CuryOrigAdjgAmt = value;
			}
		}
		#endregion
		/**/


		#region CuryAdjgDiscAmt
		public abstract class curyAdjgDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgDiscAmt> { }
		[PXDecimal(4)]
		public decimal? CuryAdjgDiscAmt
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}
		#endregion
		#region CuryAdjdDiscAmt
		public abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdDiscAmt> { }
		[PXDecimal(4)]
		public decimal? CuryAdjdDiscAmt
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}
		#endregion
		#region AdjDiscAmt
		public abstract class adjDiscAmt : PX.Data.BQL.BqlDecimal.Field<adjDiscAmt> { }
		[PXDecimal(4)]
		public decimal? AdjDiscAmt
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}
		#endregion
		#region AdjdOrigCuryInfoID
		public abstract class adjdOrigCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdOrigCuryInfoID> { }
		protected Int64? _AdjdOrigCuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = GL.BatchModule.SO, CuryIDField = "AdjdOrigCuryID")]
		public virtual Int64? AdjdOrigCuryInfoID
		{
			get
			{
				return this._AdjdOrigCuryInfoID;
			}
			set
			{
				this._AdjdOrigCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgCuryInfoID
		public abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID> { }
		protected Int64? _AdjgCuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(ARPayment.curyInfoID), CuryIDField = "AdjgCuryID")]
		public virtual Int64? AdjgCuryInfoID
		{
			get
			{
				return this._AdjgCuryInfoID;
			}
			set
			{
				this._AdjgCuryInfoID = value;
			}
		}
		#endregion
		#region AdjdCuryInfoID
		public abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }
		protected Int64? _AdjdCuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		[CurrencyInfo(ModuleCode = GL.BatchModule.SO, CuryIDField = "AdjdCuryID")]
		public virtual Int64? AdjdCuryInfoID
		{
			get
			{
				return this._AdjdCuryInfoID;
			}
			set
			{
				this._AdjdCuryInfoID = value;
			}
		}
		#endregion
		#region AdjgDocDate
		public abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate> { }
		protected DateTime? _AdjgDocDate;
		[PXDBDate()]
		[PXDefault(typeof(ARPayment.adjDate))]
		public virtual DateTime? AdjgDocDate
		{
			get
			{
				return this._AdjgDocDate;
			}
			set
			{
				this._AdjgDocDate = value;
			}
		}
		#endregion
		#region AdjdOrderDate
		public abstract class adjdOrderDate : PX.Data.BQL.BqlDateTime.Field<adjdOrderDate> { }
		protected DateTime? _AdjdOrderDate;
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? AdjdOrderDate
		{
			get
			{
				return this._AdjdOrderDate;
			}
			set
			{
				this._AdjdOrderDate = value;
			}
		}
		#endregion


		#region CuryAdjgBilledAmt
		public abstract class curyAdjgBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgBilledAmt> { }
		protected Decimal? _CuryAdjgBilledAmt;
		[PXDBCurrency(typeof(SOAdjust.adjgCuryInfoID), typeof(SOAdjust.adjBilledAmt), BaseCalc = false)]
		[PXUIField(DisplayName = "Transferred to Invoice", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryAdjgBilledAmt
		{
			get
			{
				return this._CuryAdjgBilledAmt;
			}
			set
			{
				this._CuryAdjgBilledAmt = value;
			}
		}
		#endregion
		#region AdjBilledAmt
		public abstract class adjBilledAmt : PX.Data.BQL.BqlDecimal.Field<adjBilledAmt> { }
		protected Decimal? _AdjBilledAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AdjBilledAmt
		{
			get
			{
				return this._AdjBilledAmt;
			}
			set
			{
				this._AdjBilledAmt = value;
			}
		}
		#endregion
		#region CuryAdjdBilledAmt
		public abstract class curyAdjdBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdBilledAmt> { }
		protected Decimal? _CuryAdjdBilledAmt;
		[PXDBCurrency(typeof(SOAdjust.adjdCuryInfoID), typeof(SOAdjust.adjBilledAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transferred to Invoice", Enabled = false)]
		public virtual Decimal? CuryAdjdBilledAmt
		{
			get
			{
				return this._CuryAdjdBilledAmt;
			}
			set
			{
				this._CuryAdjdBilledAmt = value;
			}
		}
		#endregion
		
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		protected Decimal? _CuryDocBal;
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXCurrency(typeof(SOAdjust.adjgCuryInfoID), typeof(SOAdjust.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		[PXDecimal(4)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
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

		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		protected Boolean? _Voided;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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
		#region System Columns
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedDateTime()]
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
		#endregion

		#region IPayBalance Members

		public decimal? CuryPayDocBal
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

		public decimal? PayDocBal
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

		public decimal? CuryPayDiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? PayDiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? CuryPayWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? PayWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}
		#endregion

		#region IAdjustment Members

		
		public DateTime? AdjdDocDate
		{
			get
			{
				return this._AdjdOrderDate;
			}
			set
			{
				this._AdjdOrderDate = value;
			}
		}

		public decimal? RGOLAmt
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public bool? Released
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool? ReverseGainLoss
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public decimal? CuryDiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? DiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? CuryAdjgWhTaxAmt
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? CuryAdjdWhTaxAmt
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? AdjWhTaxAmt
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? CuryWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? WhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		#endregion
	}
}
