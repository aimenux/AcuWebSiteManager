using PX.Objects.CR;
using PX.Common;
using PX.Data.EP;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	using PX.Objects.CM;

	public class BatchStatus
	{ 
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
			new string[] { Hold, Balanced, Unposted, Posted, Completed, Voided, Released, PartiallyReleased, Scheduled },
			new string[] { Messages.Hold, Messages.Balanced, Messages.Unposted, Messages.Posted, Messages.Completed, Messages.Voided, Messages.Released, Messages.PartiallyReleased, Messages.Scheduled }) { }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Unposted = "U";
		public const string Posted = "P";
		public const string Completed = "C";
		public const string Voided = "V";
		public const string Released = "R";
		public const string PartiallyReleased = "Q";
		public const string Scheduled = "S";

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { ;}
		}

		public class unposted : PX.Data.BQL.BqlString.Constant<unposted>
		{
			public unposted() : base(Unposted) { ;}
		}

		public class posted : PX.Data.BQL.BqlString.Constant<posted>
		{
			public posted() : base(Posted) { ;}
		}

		public class completed : PX.Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { ;}
		}

		public class voided : PX.Data.BQL.BqlString.Constant<voided>
		{
			public voided() : base(Voided) { ;}
		}

		public class scheduled : PX.Data.BQL.BqlString.Constant<scheduled>
		{
			public scheduled() : base(Scheduled) { ;}
		}
	}

	public static class BatchModule
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { GL, AP, AR, CM, CA, IN, DR, FA, PM, PR},
				new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCM, Messages.ModuleCA, Messages.ModuleIN, Messages.ModuleDR, Messages.ModeleFA, Messages.ModulePM, Messages.ModulePR }) { }
		}

		public class FullListAttribute : PXStringListAttribute
		{
			public FullListAttribute()
				: base(
				new string[] { GL, AP, AR, CM, CA, IN, DR, FA, PM, TX, SO, PO, EP, PR },
				new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCM, Messages.ModuleCA, Messages.ModuleIN, Messages.ModuleDR, Messages.ModeleFA, Messages.ModulePM, Messages.ModuleTX, Messages.ModuleSO, Messages.ModulePO, Messages.ModuleEP, Messages.ModulePR }) { }
		}

		/// <summary>
		/// Specilaized for GL.BatchModule version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the GL Batch. <br/>
		/// References Batch.module and Batch.dateEntered fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in GLSetup (namely GLSetup.batchNumberingID)<br/>
		/// and CADeposit: <br/>
		/// </summary>	
		public class NumberingAttribute : AutoNumberAttribute
		{
            private static string[] _Modules
            {
                get
                {
                    return new string[] { GL, AP, AR, CM, CA, IN, DR, FA, PM, PR };
                }
            }

            private static Type[] _SetupFields
            {
                get
                {
                    return new Type[] 
				    {	
					    typeof(GLSetup.batchNumberingID), 
					    typeof(Search<AP.APSetup.batchNumberingID>), 
					    typeof(Search<AR.ARSetup.batchNumberingID>), 
					    typeof(Search<CM.CMSetup.batchNumberingID>),
					    typeof(Search<CA.CASetup.batchNumberingID>), 
					    typeof(Search<IN.INSetup.batchNumberingID>), 
					    typeof(GLSetup.batchNumberingID),
					    typeof(Search<FA.FASetup.batchNumberingID>),
                        typeof(Search<PM.PMSetup.batchNumberingID>),
                        typeof(Search<PR.Standalone.PRSetup.batchNumberingID>),
                    };
                }
            }

            public static Type GetNumberingIDField(string module)
            {
                foreach (var pair in _Modules.Zip(_SetupFields))
                    if (pair.Item1 == module)
                        return pair.Item2;

                return null;
            }

			public NumberingAttribute()
				:base(typeof(Batch.module), typeof(Batch.dateEntered), _Modules, _SetupFields)
            {
            }
		}

		public class CashManagerListAttribute : PXStringListAttribute
		{
			public CashManagerListAttribute()
				: base(
				new string[] { AP, AR, CA },
				new string[] { Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCA }) { }
		}

		[Obsolete(Common.Messages.AttributeDeprecatedWillRemoveInAcumatica7)]
		public class GLOnlyListAttribute : PXStringListAttribute
		{
			public GLOnlyListAttribute()
				: base(
					new string[] { GL },
					new string[] { Messages.ModuleGL }) { }
		}
				
		/// <summary>
		/// List of Modules supported in Project Management
		/// </summary>
		public class PMListAttribute : PXStringListAttribute
		{
			public PMListAttribute()
				: base(
				new string[] { GL, AP, AR, IN, PM, CA, DR, PR },
				new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleIN, Messages.ModulePM, Messages.ModuleCA, Messages.ModuleDR, Messages.ModulePR }) { }
		}


		public const string GL = "GL";
		public const string TA = "TA";
		public const string EA = "EA";
		public const string AP = "AP";
		public const string AR = "AR";
		public const string CA = "CA";
		public const string CM = "CM";
		public const string IN = "IN";
		public const string SO = "SO";
		public const string PO = "PO";
		public const string DR = "DR";
		public const string FA = "FA";
		public const string EP = "EP";
		public const string PM = "PM";
		public const string TX = "TX";
		public const string CR = "CR";
		public const string WZ = "WZ";
		public const string FS = "FS";
        public const string PR = "PR";

        public class moduleGL : PX.Data.BQL.BqlString.Constant<moduleGL>
		{
			public moduleGL() : base(GL) { ;}
		}

		public class moduleAP : PX.Data.BQL.BqlString.Constant<moduleAP>
		{
			public moduleAP() : base(AP) { ;}
		}

		public class moduleTX : PX.Data.BQL.BqlString.Constant<moduleTX>
		{
			public moduleTX() : base(TX) { ;}
		}

		public class moduleAR : PX.Data.BQL.BqlString.Constant<moduleAR>
		{
			public moduleAR() : base(AR) { ;}
		}

		public class moduleCM : PX.Data.BQL.BqlString.Constant<moduleCM>
		{
			public moduleCM() : base(CM) { ;}
		}

		public class moduleCA : PX.Data.BQL.BqlString.Constant<moduleCA>
		{
			public moduleCA() : base(CA) { ;}
		}

		public class moduleIN : PX.Data.BQL.BqlString.Constant<moduleIN>
		{
			public moduleIN() : base(IN) { ;}
		}

		public class moduleSO : PX.Data.BQL.BqlString.Constant<moduleSO>
		{
			public moduleSO() : base(SO) { ;}
		}

		public class modulePO : PX.Data.BQL.BqlString.Constant<modulePO>
		{
			public modulePO() : base(PO) { ;}
		}

		public class moduleDR : PX.Data.BQL.BqlString.Constant<moduleDR>
		{
			public moduleDR() : base(DR) { ;}
		}

		public class moduleFA : PX.Data.BQL.BqlString.Constant<moduleFA>
		{
			public moduleFA() : base(FA) { ;}
		}

		public class moduleEP : PX.Data.BQL.BqlString.Constant<moduleEP>
		{
			public moduleEP() : base(EP) { ;}
		}

		public class modulePM : PX.Data.BQL.BqlString.Constant<modulePM>
		{
			public modulePM() : base(PM) { ;}
		}

		public class moduleWZ : PX.Data.BQL.BqlString.Constant<moduleWZ>
		{
			public moduleWZ() : base(WZ) { ;}
		}

		public class moduleFS : PX.Data.BQL.BqlString.Constant<moduleFS>
		{
			public moduleFS() : base(FS) {; }
		}

        public class modulePR : PX.Data.BQL.BqlString.Constant<modulePR>
        {
            public modulePR() : base(PR) {; }
        }

        /// <summary>
        /// Returns the localized display name of the specified module.
        /// The list of display names is taken from the <see cref="PX.Objects.GL.Messages"/> class.
        /// </summary>
        /// <example>
        /// <code>
        /// BatchModule.GetDisplayName("GL"); // returns "General Ledger"
        /// </code>
        /// </example>
        public static string GetDisplayName(string module)
		{
			if (string.IsNullOrWhiteSpace(module)) 
				throw new ArgumentException("Module cannot be null or whitespace", nameof(module));

			Type messagesType = typeof(PX.Objects.GL.Messages);
			string fieldName = "ModuleName" + module.ToUpperInvariant();
			var field = messagesType.GetField(fieldName);

			if (field == null || !field.IsLiteral || field.FieldType != typeof(string))
				throw new NotSupportedException($"Module '{module}' doesn't have a corresponding display name.");

			string value = (string) field.GetRawConstantValue();

			return PXLocalizer.Localize(value, messagesType.FullName);
		}

		/// <summary>
		/// Returns the localized display name of the specified module.
		/// The list of display names is taken from the <see cref="PX.Objects.GL.Messages"/> class.
		/// </summary>
		/// <example>
		/// <code>
		/// BatchModule.GetDisplayName&lt;BatchModule.moduleGL&gt;(); // returns "General Ledger"
		/// </code>
		/// </example>
		public static string GetDisplayName<TModule>()
			where TModule : IConstant<string>, IBqlOperand, new()
		{
			return GetDisplayName(new TModule().Value);
		}
	}

	public static class BatchTypeCode
	{
		public const string Normal = "H";
		public const string Consolidation = "C";
		public const string TrialBalance = "T";
		public const string Reclassification = "RCL";
		public const string Allocation = "A";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { BatchTypeCode.Normal, BatchTypeCode.Consolidation, BatchTypeCode.TrialBalance, BatchTypeCode.Reclassification, BatchTypeCode.Allocation},
					new string[] { Messages.BTNormal, Messages.BTConsolidation, Messages.BTTrialBalance, Messages.BTReclassification, Messages.BTAllocation }) { }
		}

		public class normal : PX.Data.BQL.BqlString.Constant<normal>
		{
			public normal() : base(Normal) { }
		}

		public class consolidation : PX.Data.BQL.BqlString.Constant<consolidation>
		{
			public consolidation() : base(Consolidation) { }
		}

		public class trialBalance : PX.Data.BQL.BqlString.Constant<trialBalance>
		{
			public trialBalance() : base(TrialBalance) { }
		}

		public class reclassification : PX.Data.BQL.BqlString.Constant<reclassification>
		{
			public reclassification() : base(Reclassification) { }
		}

		public class allocation : PX.Data.BQL.BqlString.Constant<allocation>
		{
			public allocation() : base(Allocation) { }
		}
	}

    /// <summary>
    /// Represents a batch of <see cref="GLTran">journal transactions</see>.
    /// The records of this type are edited through the Journal Transactions (GL.30.10.00) screen
    /// (corresponds to the <see cref="JournalEntry"/> graph).
    /// GL batches are also created whenever a document that needs posting to GL is released in any other module.
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Batch)]
	[PXPrimaryGraph(typeof(JournalEntry))]
	public partial class Batch : PX.Data.IBqlTable
	{
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;

        /// <summary>
        /// Indicates whether the record is selected for mass processing.
        /// </summary>
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

		/// <summary>
		/// Identifier of the <see cref="Branch"/>, to which the batch belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch()]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;

        /// <summary>
        /// Key field.
        /// The code of the module, to which the batch belongs.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// "GL", "AP", "AR", "CM", "CA", "IN", "DR", "FA", "PM", "TX", "SO", "PO".
        /// </value>
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[BatchModule.List()]
		[PXFieldDescription]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
        protected String _BatchNbr;

        /// <summary>
        /// Key field.
        /// Auto-generated unique number of the batch.
        /// </summary>
        /// <value>
        /// The number is generated from the <see cref="Numbering">Numbering Sequence</see> specified in the
        /// setup record of the <see cref="Module"/>, to which the batch belongs.
        /// For example, the numbering sequence for the batches belonging to the General Ledger module
        /// is specified in the <see cref="GLSetup.BatchNumberingID"/> field. For other modules see corresponding setup DACs.
        /// </value>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Current<Batch.module>>,And<Batch.draft,Equal<False>>>, OrderBy<Desc<Batch.batchNbr>>>), Filterable = true)]
		[PXUIField(DisplayName="Batch Number", Visibility=PXUIVisibility.SelectorVisible)]
		[BatchModule.Numbering()]
		[PXFieldDescription]
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
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
        protected Int32? _LedgerID;

        /// <summary>
        /// Identifier of the <see cref="Ledger"/>, to which the batch belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Ledger.LedgerID"/> field.
        /// </value>
		[PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<Batch.branchID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search5<Ledger.ledgerID, 
								LeftJoin<OrganizationLedgerLink,
									On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>, 
								LeftJoin<Branch,
									On<Branch.organizationID, Equal<OrganizationLedgerLink.organizationID>, And<Branch.branchID,Equal<Current2<Batch.branchID>>>>>>,
								Where<Ledger.balanceType, NotEqual<LedgerBalanceType.budget>>,
								Aggregate<GroupBy<Ledger.ledgerID>>>), 
						SubstituteKey = typeof(Ledger.ledgerCD),
						DescriptionField = typeof(Ledger.descr)
		)]
		[PXRestrictor(typeof(Where<Branch.branchID, Equal<Current2<Batch.branchID>>, Or<Current2<Batch.branchID>, IsNull>>), 
			Messages.TheLedgerIsNotAssociatedWithBranch, typeof(Ledger.ledgerCD)
		)]
		public virtual Int32? LedgerID
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
		#region DateEntered
		public abstract class dateEntered : PX.Data.BQL.BqlDateTime.Field<dateEntered> { }
        protected DateTime? _DateEntered;

        /// <summary>
        /// The date of the batch, specified by user.
        /// </summary>
        /// <value>
        /// Defaults to the current <see cref="AccessInfo.BusinessDate">Business Date</see>.
        /// </value>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DateEntered
		{
			get
			{
				return this._DateEntered;
			}
			set
			{
				this._DateEntered = value;
			}
		}
		#endregion		
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        protected String _FinPeriodID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see>, to which the batch belongs.
        /// </summary>
        /// <value>
        /// By default the period is deducted from the <see cref="DateEntered">date of the batch</see>.
        /// Can be overriden by user.
        /// </value>
        [OpenPeriod(null,
					typeof(Batch.dateEntered), 
					typeof(Batch.branchID),
                    masterFinPeriodIDType: typeof(Batch.tranPeriodID),
                    IsHeader = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        public virtual String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
		#endregion
		#region BatchType
		public abstract class batchType : PX.Data.BQL.BqlString.Field<batchType> { }
        protected String _BatchType;

        /// <summary>
        /// The type of the batch.
        /// </summary>
        /// <value>
        /// Allowed values are: 
        /// <c>"H"</c> - Normal,
        /// <c>"R"</c> - Recurring,
        /// <c>"C"</c> - Consolidation,
		/// <c>"T"</c> - Trial Balance,
		/// <c>"RCL"</c> - Reclassification,
		/// <c>"A"</c> - Allocation.
        /// Defaults to <c>"H"</c> - Normal.
        /// </value>
		[PXDBString(3)]
		[PXDefault(BatchTypeCode.Normal)]
		[PXUIField(DisplayName = "Type")]
		[BatchTypeCode.List()]
		public virtual String BatchType
		{
			get
			{
				return this._BatchType;
			}
			set
			{
				this._BatchType = value;
			}
		}
		#endregion
		#region NumberCode
		public abstract class numberCode : PX.Data.BQL.BqlString.Field<numberCode> { }
		protected String _NumberCode;

        /// <summary>
        /// The identifier of the <see cref="GLNumberCode"/> record used to assign an auto-generated <see cref="RefNbr"/> to the batch.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="GLNumberCode.NumberCode"/> field.
        /// The number codes are assigned by the <see cref="AllocationProcess"/> and <see cref="ScheduleProcess"/> graphs,
        /// which allows to have separate numbering sequences for recurring and allocation batches.
        /// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">aaaaa")]
		[PXUIField(DisplayName = "Number. Code")]
		public virtual String NumberCode
		{
			get
			{
				return this._NumberCode;
			}
			set
			{
				this._NumberCode = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;

        /// <summary>
        /// Auto-generated reference number assigned to the batch according to the <see cref="Numbering">Numbering Sequence</see> specified
        /// in the <see cref="GLNumberCode.NumberingID"/> field of the corresponding <see cref="GLNumberCode"/> record (see the <see cref="NumberCode"/> field).
        /// </summary>
        /// <value>
        /// The field will remain empty if <see cref="NumberCode"/> is not set for the batch.
        /// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(Visible = false)]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;

        /// <summary>
        /// The read-only status of the batch.
        /// </summary>
        /// <value>
        /// The value of this field is determined by the <see cref="Hold"/>, <see cref="Released"/>,
        /// <see cref="Posted"/>, <see cref="Voided"/> and <see cref="Scheduled"/> fields.
        /// Possible values are:
        /// <c>"H"</c> - Hold
        /// <c>"B"</c> - Balanced
        /// <c>"U"</c> - Unposted
        /// <c>"P"</c> - Posted
        /// <c>"C"</c> - Completed
        /// <c>"V"</c> - Voided
        /// <c>"R"</c> - Released
        /// <c>"Q"</c> - Partially Released
        /// <c>"S"</c> - Scheduled
        /// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BatchStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[BatchStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(posted), typeof(voided), typeof(scheduled), typeof(released), typeof(hold))]
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
		#region CuryDebitTotal
		public abstract class curyDebitTotal : PX.Data.BQL.BqlDecimal.Field<curyDebitTotal> { }
        protected Decimal? _CuryDebitTotal;

        /// <summary>
        /// The total debit amount of the batch in its <see cref="CuryID">currency</see>.
        /// </summary>
        /// <value>
        /// See also <see cref="DebitTotal"/>.
        /// </value>
		[PXDBCurrency(typeof(Batch.curyInfoID), typeof(Batch.debitTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryDebitTotal
		{
			get
			{
				return this._CuryDebitTotal;
			}
			set
			{
				this._CuryDebitTotal = value;
			}
		}
		#endregion
		#region CuryCreditTotal
		public abstract class curyCreditTotal : PX.Data.BQL.BqlDecimal.Field<curyCreditTotal> { }
        protected Decimal? _CuryCreditTotal;

        /// <summary>
        /// The total credit amount of the batch in its <see cref="CuryID">currency</see>.
        /// </summary>
        /// <value>
        /// See also <see cref="CreditTotal"/>.
        /// </value>
		[PXDBCurrency(typeof(Batch.curyInfoID), typeof(Batch.creditTotal))]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Credit Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryCreditTotal
		{
			get
			{
				return this._CuryCreditTotal;
			}
			set
			{
				this._CuryCreditTotal = value;
			}
		}
		#endregion
		#region CuryControlTotal
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }
        protected Decimal? _CuryControlTotal;

        /// <summary>
        /// The control total of the batch in its <see cref="CuryID">currency</see>.
        /// </summary>
        /// See also <see cref="ControlTotal"/>.
		[PXDBCurrency(typeof(Batch.curyInfoID), typeof(Batch.controlTotal))]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? CuryControlTotal
		{
			get
			{
				return this._CuryControlTotal;
			}
			set
			{
				this._CuryControlTotal = value;
			}
		}
		#endregion
		#region DebitTotal
		public abstract class debitTotal : PX.Data.BQL.BqlDecimal.Field<debitTotal> { }
        protected Decimal? _DebitTotal;

        /// <summary>
        /// The total debit amount of the batch in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// See also <see cref="CuryDebitTotal"/>.
        /// </value>
		[PXDBBaseCury(typeof(Batch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DebitTotal
		{
			get
			{
				return this._DebitTotal;
			}
			set
			{
				this._DebitTotal = value;
			}
		}
		#endregion
		#region CreditTotal
		public abstract class creditTotal : PX.Data.BQL.BqlDecimal.Field<creditTotal> { }
        protected Decimal? _CreditTotal;

        /// <summary>
        /// The total credit amount of the batch in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// See also <see cref="CuryCreditTotal"/>.
        /// </value>
		[PXDBBaseCury(typeof(Batch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CreditTotal
		{
			get
			{
				return this._CreditTotal;
			}
			set
			{
				this._CreditTotal = value;
			}
		}
		#endregion
		#region ControlTotal
		public abstract class controlTotal : PX.Data.BQL.BqlDecimal.Field<controlTotal> { }
        protected Decimal? _ControlTotal;

        /// <summary>
        /// The control total of the batch in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// See also <see cref="CuryControlTotal"/>.
        /// </value>
		[PXDBBaseCury(typeof(Batch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ControlTotal
		{
			get
			{
				return this._ControlTotal;
			}
			set
			{
				this._ControlTotal = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        protected Int64? _CuryInfoID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> record associated with the batch.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PCurrencyInfo.CurrencyInfoID"/> field.
        /// </value>
		[PXDBLong()]
		[PX.Objects.CM.CurrencyInfo()]
		public virtual Int64? CuryInfoID
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
		#region AutoReverse
		public abstract class autoReverse : PX.Data.BQL.BqlBool.Field<autoReverse> { }
		protected Boolean? _AutoReverse;

        /// <summary>
        /// When set to <c>true</c>, indicates that the batch is auto-reversing.
        /// For a batch of this kind the system automatically generates a reversing batch in the next period.
        /// The reversing batch is generated either when the original batch is posted or when it is released,
        /// depending on the value of the <see cref="GLSetup.AutoRevOption"/> field of the GL preferences record.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName="Auto Reversing", Visibility=PXUIVisibility.Visible)]
		public virtual Boolean? AutoReverse
		{
			get
			{
				return this._AutoReverse;
			}
			set
			{
				this._AutoReverse = value;
			}
		}
		#endregion
		#region AutoReverseCopy
		public abstract class autoReverseCopy : PX.Data.BQL.BqlBool.Field<autoReverseCopy> { }
		protected Boolean? _AutoReverseCopy;

        /// <summary>
        /// When set to <c>true</c>, indicates that the batch is a reversing batch.
        /// See also the <see cref="OrigModule"/> and <see cref="OrigBatchNbr"/> fields.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Reversing Entry", Enabled = false, Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutoReverseCopy
		{
			get
			{
				return this._AutoReverseCopy;
			}
			set
			{
				this._AutoReverseCopy = value;
			}
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		protected String _OrigModule;

        /// <summary>
        /// The module, to which the original batch (e.g. the one reversed by this batch) belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Module"/> field.
        /// </value>
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Orig. Module", Visible = false, Enabled = false)]
		public virtual String OrigModule
		{
			get
			{
				return this._OrigModule;
			}
			set
			{
				this._OrigModule = value;
			}
		}
		#endregion
		#region OrigBatchNbr
		public abstract class origBatchNbr : PX.Data.BQL.BqlString.Field<origBatchNbr> { }
		protected String _OrigBatchNbr;

        /// <summary>
        /// The number of the original batch (e.g. the one reversed by this batch).
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="BatchNbr"/> field.
        /// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="Orig. Batch Number", Visibility=PXUIVisibility.Visible, Enabled =false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Current<Batch.origModule>>>>))]
		public virtual String OrigBatchNbr
		{
			get
			{
				return this._OrigBatchNbr;
			}
			set
			{
				this._OrigBatchNbr = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

        /// <summary>
        /// Indicates whether the batch has been released.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
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
		#region Posted
		public abstract class posted : PX.Data.BQL.BqlBool.Field<posted> { }
		protected Boolean? _Posted;

        /// <summary>
        /// Indicates whether the batch has been posted.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Posted")]
		public virtual Boolean? Posted
		{
			get
			{
				return this._Posted;
			}
			set
			{
				this._Posted = value;
				this.SetStatus();
			}
		}
		#endregion
		#region RequirePost
		public abstract class requirePost : PX.Data.BQL.BqlBool.Field<requirePost> { }
		/// <summary>
		/// Indicates whether the batch is required to be posted.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? RequirePost { get; set; }
		#endregion
		#region PostTryCount
		public class postErrorCountLimit : PX.Data.BQL.BqlInt.Constant<postErrorCountLimit>
		{
			public postErrorCountLimit() : base(5) { }
		}

		public abstract class postErrorCount : PX.Data.BQL.BqlBool.Field<postErrorCount> { }
		/// <summary>
		/// Indicates how many times system has failed while posting batch.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? PostErrorCount { get; set; }
		#endregion
		#region Draft
		public abstract class draft : PX.Data.BQL.BqlBool.Field<draft> { }
		protected Boolean? _Draft;

        /// <summary>
        /// When set to <c>true</c>, indicates the the batch is a draft.
        /// The drafts of batches are not displayed in the Journal Transactions screen.
        /// </summary>
        /// <value>
        /// This field is set to <c>true</c> by the Journal Vouchers (GL.30.40.00) screen (<see cref="JournalWithSubEntry"/> graph)
        /// when a new <see cref="GLTranDoc" /> defining a transactions batch is created.
        /// This allows to reserve a <see cref="BatchNbr">number</see> for the batch, while not showing it in the interface until
        /// the <see cref="GLTranDoc" /> is released and the batch is actually created.
        /// Outside Journal Vouchers defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Draft
		{
			get
			{
				return this._Draft;
			}
			set
			{
				this._Draft = value;
				this.SetStatus();
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
        protected String _TranPeriodID;

        [PeriodID]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
        protected Int32? _LineCntr;

        /// <summary>
        /// The counter of the document lines, used <i>internally</i> to assign consistent numbers to newly created lines.
        /// It is not recommended to rely on this field to determine the exact count of lines, because it might not reflect the latter under some conditions.
        /// </summary>
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        protected String _CuryID;

        /// <summary>
        /// The code of the <see cref="Currency">Currency</see> of the batch.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility=PXUIVisibility.SelectorVisible)]
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
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		protected string _ScheduleID;

        /// <summary>
        /// Identifier of the <see cref="Schedule"/>, associated with the batch.
        /// </summary>
        /// <value>
        /// If <see cref="Scheduled"/> is <c>true</c> for the batch, then this field provides the identifier of the schedule,
        /// into which the batch is included as a template.
        /// Otherwise, the field is set to the identifier of the schedule that produced the batch.
        /// Corresponds to the <see cref="Schedule.ScheduleID"/> field.
        /// </value>
		[PXDBString(15, IsUnicode = true)]
		public virtual string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
		[PXSearchable(SM.SearchCategory.GL, "{0} {1} - {2}", new Type[] { typeof(Batch.module), typeof(Batch.batchNbr), typeof(Batch.branchID) },
			new Type[] { typeof(Batch.ledgerID), typeof(Batch.description) },
			NumberFields = new Type[] { typeof(Batch.batchNbr) },
			Line1Format = "{0}{1}{2:d}", Line1Fields = new Type[] { typeof(Batch.ledgerID), typeof(Batch.finPeriodID), typeof(Batch.dateEntered) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(Batch.description) }
		)]
		[PXNote(DescriptionField = typeof(Batch.batchNbr), ShowInReferenceSelector = true)]
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
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;

        /// <summary>
        /// Indicates whether the batch is on hold.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>, if the <see cref="GLSetup.HoldEntry"/> flag is set in the preferences of the module,
        /// and to <c>false</c> otherwise.
        /// </value>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(Search<GLSetup.holdEntry>))]
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
		#region Scheduled
		public abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		protected Boolean? _Scheduled;

        /// <summary>
        /// When <c>true</c>, indicates that the batch is included as a template into a <see cref="Schedule"/> pointed to by the <see cref="ScheduleID"/> field.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
        protected Boolean? _Voided;

        /// <summary>
        /// Indicates whether the batch has been voided.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Voided")]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        protected String _Description;

        /// <summary>
        /// The description of the batch.
        /// </summary>
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		[PXDBString(256, IsUnicode = true)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
        #region CreateTaxTran
        public abstract class createTaxTrans : PX.Data.BQL.BqlBool.Field<createTaxTrans> { }
        protected Boolean? _CreateTaxTrans;

        /// <summary>
        /// When set to <c>true</c>, indicates that the system must generate <see cref="TaxTran">tax transactions</see> for the batch.
        /// </summary>
        /// <value>
        /// This field is taken into account only if the <see cref="FeaturesSet.TaxEntryFromGL"/> feature is on.
        /// Affects only those batches, which belong to the General Ledger <see cref="Module"/>.
        /// Defaults to <c>false</c>.
        /// </value>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName ="Create Tax Transactions")]
        public virtual Boolean? CreateTaxTrans
        {
            get
            {
                return this._CreateTaxTrans;
            }
            set
            {
                this._CreateTaxTrans = value;        
            }
        }
        #endregion
		#region SkipTaxValidation
		public abstract class skipTaxValidation : PX.Data.BQL.BqlBool.Field<skipTaxValidation> { }

        /// <summary>
        /// When set to <c>true</c>, indicates that the system should not validate the <see cref="TaxTran">tax transactions</see> associated with the batch.
        /// </summary>
        /// <value>
        /// The value of this field is relevant only if <see cref="CreateTaxTrans"/> is <c>true</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Skip Tax Amount Validation")]
		public virtual Boolean? SkipTaxValidation{ get; set;}
		#endregion
        #region ReverseCount
        public abstract class reverseCount : PX.Data.BQL.BqlInt.Field<reverseCount> { }

        private int? _ReverseCount;

        /// <summary>
        /// The read-only field, reflecting the number of batches in the system, which reverse this batch.
        /// </summary>
        /// <value>
        /// This field is populated only by the <see cref="JournalEntry"/> graph (corresponds to the Journal Transactions GL.30.10.00 screen).
        /// </value>
        [PXInt]
        [PXUIField(DisplayName = "Reversing Batches", Visible = false, Enabled = false, IsReadOnly = true)]
        public int? ReverseCount
        {
            get { return _ReverseCount; }
            set { _ReverseCount = value; }
        }
        #endregion
        #region HasRamainingAmount
        public abstract class hasRamainingAmount : PX.Data.BQL.BqlBool.Field<hasRamainingAmount> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the batch has a detail with the visible <see cref="GLTran.CuryReclassRemainingAmt"/> field.
        /// </summary>
        /// <value>
        /// This field is populated only by the <see cref="JournalEntry"/> graph (corresponds to the Journal Transactions GL.30.10.00 screen).
        /// </value>
        [Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2019R2)]
        [PXBool]
        [PXUIField(Enabled = false, Visible = false)]
        public bool? HasRamainingAmount { get; set; }
		#endregion
		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Voided != null && (bool)this._Voided)
			{
				this._Status = "V";
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = "H";
			}
			else if (this._Scheduled != null && (bool)this._Scheduled)
			{
				this._Status = "S";
			}
			else if (this._Released != null && (bool)this._Released == false)
			{
				this._Status = "B";
			}
			else if (this._Posted != null && (bool)this._Posted == false)
			{
				this._Status = "U";
			}
			else if (this._Posted != null && (bool)this._Posted)
			{
				this._Status = "P";
			}
		}
		#endregion
	}

    [Serializable]
	public partial class BatchReport : Batch
	{
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
        [PXSelector(typeof(Search<Batch.batchNbr,
            Where<Batch.finPeriodID, Equal<Optional<Batch.finPeriodID>>,
            And<Batch.ledgerID, Equal<Optional<Batch.ledgerID>>,
            And<Batch.released, Equal<True>,
            And<Batch.posted, Equal<True>,
            And<Where<Batch.module, Equal<BatchModule.moduleGL>,
            Or<Batch.module, Equal<BatchModule.moduleCM>>>>>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		public override String BatchNbr
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
    }

    [Serializable]
    public partial class BatchPostedForModule : Batch
    {
        #region BatchNbr
        public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault()]
        [PXSelector(typeof(Search<Batch.batchNbr,
            Where<Batch.released, Equal<True>,
            And<Batch.posted, Equal<True>,
            And<Batch.module, Equal<Optional<Batch.module>>>>>,
            OrderBy<Desc<Batch.batchNbr>>>),

            typeof(Batch.batchNbr),
            typeof(Batch.status),
            typeof(Batch.ledgerID),
            typeof(Batch.finPeriodID),
            typeof(Batch.curyDebitTotal),
            typeof(Batch.curyCreditTotal),
            typeof(Batch.curyID))]
        [PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
        public override String BatchNbr
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
    }
}
