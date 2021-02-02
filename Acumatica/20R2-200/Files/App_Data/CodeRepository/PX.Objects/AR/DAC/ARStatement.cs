using System;
using System.Diagnostics;

using PX.Data;

using PX.Objects.CM;
using PX.Objects.AR.CustomerStatements;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	[Serializable]
	[PXPrimaryGraph(typeof(ARStatementUpdate))]
	[PXEMailSource]
	[PXCacheName(Messages.Statement)]
	public partial class ARStatement : PX.Data.IBqlTable
	{
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
        [GL.Branch(IsKey = true)]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXSelector(typeof(CM.Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Customer", IsReadOnly = true)]
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
		#region StatementCustomerID
		public abstract class statementCustomerID : PX.Data.BQL.BqlInt.Field<statementCustomerID> { }
		[PXDBInt]
		[PXDefault]
		public virtual Int32? StatementCustomerID { get; set; }
		#endregion
		#region StatementDate
		public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		protected DateTime? _StatementDate;
		[PXDBDate(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Statement Date")]
		[PXSelector(typeof(Search4<ARStatement.statementDate,Aggregate<GroupBy<ARStatement.statementDate>>>))]
		public virtual DateTime? StatementDate
		{
			get
			{
				return this._StatementDate;
			}
			set
			{
				this._StatementDate = value;
			}
		}
        #endregion
        #region PrevStatementDate
        public abstract class prevStatementDate : PX.Data.BQL.BqlDateTime.Field<prevStatementDate> { }
        protected DateTime? _PrevStatementDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Last Statement Date")]
        public virtual DateTime? PrevStatementDate
        {
            get
            {
                return this._PrevStatementDate;
            }
            set
            {
                this._PrevStatementDate = value;
            }
        }
        #endregion
		#region StatementCycleId
		public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
		protected String _StatementCycleId;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Statement Cycle ID")]
		[PXSelector(typeof(ARStatementCycle.statementCycleId))]
		[PXForeignReference(typeof(Field<ARStatement.statementCycleId>.IsRelatedTo<ARStatementCycle.statementCycleId>))]
		public virtual String StatementCycleId
		{
			get
			{
				return this._StatementCycleId;
			}
			set
			{
				this._StatementCycleId = value;
			}
		}
		#endregion
		#region StatementType
		public abstract class statementType : PX.Data.BQL.BqlString.Field<statementType> { }
		protected String _StatementType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Statement Type")]
		public virtual String StatementType
		{
			get
			{
				return this._StatementType;
			}
			set
			{
				this._StatementType = value;
			}
		}
		#endregion
		#region BegBalance
		public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
		protected Decimal? _BegBalance;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Beg. Balance")]
		public virtual Decimal? BegBalance
		{
			get
			{
				return this._BegBalance;
			}
			set
			{
				this._BegBalance = value;
			}
		}
		#endregion
		#region CuryBegBalance
		public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }
		protected Decimal? _CuryBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Curr. Beg. Balance")]
		public virtual Decimal? CuryBegBalance
		{
			get
			{
				return this._CuryBegBalance;
			}
			set
			{
				this._CuryBegBalance = value;
			}
		}
		#endregion
		#region EndBalance
		public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
		protected Decimal? _EndBalance;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EndBalance
		{
			get
			{
				return this._EndBalance;
			}
			set
			{
				this._EndBalance = value;
			}
		}
		#endregion
		#region CuryEndBalance
		public abstract class curyEndBalance : PX.Data.BQL.BqlDecimal.Field<curyEndBalance> { }
		protected Decimal? _CuryEndBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryEndBalance
		{
			get
			{
				return this._CuryEndBalance;
			}
			set
			{
				this._CuryEndBalance = value;
			}
		}
		#endregion
		#region AgeBalance00
		public abstract class ageBalance00 : PX.Data.BQL.BqlDecimal.Field<ageBalance00> { }
		protected Decimal? _AgeBalance00;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Age00 Balance")]
		public virtual Decimal? AgeBalance00
		{
			get
			{
				return this._AgeBalance00;
			}
			set
			{
				this._AgeBalance00 = value;
			}
		}
		#endregion
		#region CuryAgeBalance00
		public abstract class curyAgeBalance00 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance00> { }
		protected Decimal? _CuryAgeBalance00;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Cury. Age00 Balance")]
		public virtual Decimal? CuryAgeBalance00
		{
			get
			{
				return this._CuryAgeBalance00;
			}
			set
			{
				this._CuryAgeBalance00 = value;
			}
		}
		#endregion
		#region AgeBalance01
		public abstract class ageBalance01 : PX.Data.BQL.BqlDecimal.Field<ageBalance01> { }
		protected Decimal? _AgeBalance01;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Age01 Balance")]
		public virtual Decimal? AgeBalance01
		{
			get
			{
				return this._AgeBalance01;
			}
			set
			{
				this._AgeBalance01 = value;
			}
		}
		#endregion
		#region CuryAgeBalance01
		public abstract class curyAgeBalance01 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance01> { }
		protected Decimal? _CuryAgeBalance01;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Cury. Age01 Balance")]
		public virtual Decimal? CuryAgeBalance01
		{
			get
			{
				return this._CuryAgeBalance01;
			}
			set
			{
				this._CuryAgeBalance01 = value;
			}
		}
		#endregion
		#region AgeBalance02
		public abstract class ageBalance02 : PX.Data.BQL.BqlDecimal.Field<ageBalance02> { }
		protected Decimal? _AgeBalance02;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Age02 Balance")]
		public virtual Decimal? AgeBalance02
		{
			get
			{
				return this._AgeBalance02;
			}
			set
			{
				this._AgeBalance02 = value;
			}
		}
		#endregion
		#region CuryAgeBalance02
		public abstract class curyAgeBalance02 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance02> { }
		protected Decimal? _CuryAgeBalance02;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Cury. Age02 Balance")]
		public virtual Decimal? CuryAgeBalance02
		{
			get
			{
				return this._CuryAgeBalance02;
			}
			set
			{
				this._CuryAgeBalance02 = value;
			}
		}
		#endregion
		#region AgeBalance03
		public abstract class ageBalance03 : PX.Data.BQL.BqlDecimal.Field<ageBalance03> { }
		protected Decimal? _AgeBalance03;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Cury. Age03 Balance")]
		public virtual Decimal? AgeBalance03
		{
			get
			{
				return this._AgeBalance03;
			}
			set
			{
				this._AgeBalance03 = value;
			}
		}
		#endregion
		#region CuryAgeBalance03
		public abstract class curyAgeBalance03 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance03> { }
		protected Decimal? _CuryAgeBalance03;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Cury. Age03 Balance")]
		public virtual Decimal? CuryAgeBalance03
		{
			get
			{
				return this._CuryAgeBalance03;
			}
			set
			{
				this._CuryAgeBalance03 = value;
			}
		}
		#endregion
		#region AgeBalance04
		public abstract class ageBalance04 : PX.Data.BQL.BqlDecimal.Field<ageBalance04> { }
		protected Decimal? _AgeBalance04;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Age04 Balance")]
		public virtual Decimal? AgeBalance04
		{
			get
			{
				return this._AgeBalance04;
			}
			set
			{
				this._AgeBalance04 = value;
			}
		}
		#endregion
		#region CuryAgeBalance04
		public abstract class curyAgeBalance04 : PX.Data.BQL.BqlDecimal.Field<curyAgeBalance04> { }
		protected Decimal? _CuryAgeBalance04;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Cury. Age04 Balance")]
		public virtual Decimal? CuryAgeBalance04
		{
			get
			{
				return this._CuryAgeBalance04;
			}
			set
			{
				this._CuryAgeBalance04 = value;
			}
		}
		#endregion
		#region AgeDays00
		public abstract class ageDays00 : PX.Data.BQL.BqlShort.Field<ageDays00> { }
		[PXDBShort]
		public virtual short? AgeDays00
		{
			get;
			set;
		}
		#endregion
		#region AgeDays01
		public abstract class ageDays01 : PX.Data.BQL.BqlShort.Field<ageDays01> { }
		[PXDBShort]
		public virtual short? AgeDays01
		{
			get;
			set;
		}
		#endregion
		#region AgeDays02
		public abstract class ageDays02 : PX.Data.BQL.BqlShort.Field<ageDays02> { }
		[PXDBShort]
		public virtual short? AgeDays02
		{
			get;
			set;
		}
		#endregion
		#region AgeDays03
		public abstract class ageDays03 : PX.Data.BQL.BqlShort.Field<ageDays03> { }
		[PXDBShort]
		public virtual short? AgeDays03
		{
			get;
			set;
		}
		#endregion
		#region AgeDays04
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public abstract class ageDays04 : PX.Data.BQL.BqlShort.Field<ageDays04> { }
		[PXDBShort]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public virtual short? AgeDays04
		{
			get;
			set;
		}
		#endregion
		#region AgeMsgCurrent
		public abstract class ageBucketCurrentDescription : PX.Data.BQL.BqlString.Field<ageBucketCurrentDescription> { }
		/// <summary>
		/// The description of the current aging period, which incorporates
		/// documents that are no more than <see cref="AgeDays00"/> days 
		/// past due.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 0", Visibility = PXUIVisibility.Visible)]
		public virtual string AgeBucketCurrentDescription
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg00
		public abstract class ageBucket01Description : PX.Data.BQL.BqlString.Field<ageBucket01Description> { }
		/// <summary>
		/// The description of the first aging period, which incorporates documents
		/// that are from 1 to <see cref="AgeDays01"/> days past due.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 1", Visibility = PXUIVisibility.Visible)]
		public virtual string AgeBucket01Description
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg01
		public abstract class ageBucket02Description : PX.Data.BQL.BqlString.Field<ageBucket02Description> { }
		/// <summary>
		/// The description of the second aging period, which incorporates documents
		/// that are from <see cref="AgeDays01"/> + 1 to <see cref="AgeDays02"/> 
		/// days past due.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 2", Visibility = PXUIVisibility.Visible)]
		public virtual string AgeBucket02Description
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg02
		public abstract class ageBucket03Description : PX.Data.BQL.BqlString.Field<ageBucket03Description> { }
		/// <summary>
		/// The description of the third aging period that incorporates documents
		/// that are from <see cref="AgeDays02"/> + 1 to <see cref="AgeDays03"/>
		/// days past due.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 3", Visibility = PXUIVisibility.Visible)]
		public virtual string AgeBucket03Description
		{
			get;
			set;
		}
		#endregion
		#region AgeMsg03
		public abstract class ageBucket04Description : PX.Data.BQL.BqlString.Field<ageBucket04Description> { }
		/// <summary>
		/// The description of the last aging period that incorporates documents
		/// that are over <see cref="AgeDays03"/> days past due.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Age Message 4", Visibility = PXUIVisibility.Visible)]
		public virtual string AgeBucket04Description
		{
			get;
			set;
		}
		#endregion
		#region DontPrint
		public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
		protected Boolean? _DontPrint;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Don't Print")]
		public virtual Boolean? DontPrint
		{
			get
			{
				return this._DontPrint;
			}
			set
			{
				this._DontPrint = value;
			}
		}
		#endregion
		#region Printed
		public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
		protected Boolean? _Printed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Printed")]
		public virtual Boolean? Printed
		{
			get
			{
				return this._Printed;
			}
			set
			{
				this._Printed = value;
			}
		}
        #endregion
        #region PrevPrintedCnt
        public abstract class prevPrintedCnt : PX.Data.BQL.BqlShort.Field<prevPrintedCnt> { }

        [PXDBShort]
        [PXDefault(TypeCode.Int16, "0")]
        [PXUIField(DisplayName = "Previously Printed Count")]
        public virtual Int16? PrevPrintedCnt { get; set; }
        #endregion
        #region DontEmail
        public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
		protected Boolean? _DontEmail;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Don't Email")]
		public virtual Boolean? DontEmail
		{
			get
			{
				return this._DontEmail;
			}
			set
			{
				this._DontEmail = value;
			}
		}
		#endregion
		#region Emailed
		public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
		protected Boolean? _Emailed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Emailed")]
		public virtual Boolean? Emailed
		{
			get
			{
				return this._Emailed;
			}
			set
			{
				this._Emailed = value;
			}
		}
		#endregion
		#region Processed
		public abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }
		[PXBool]
		public virtual Boolean? Processed { get; set; }
		#endregion
        #region PrevEmailedCnt
        public abstract class prevEmailedCnt : PX.Data.BQL.BqlShort.Field<prevEmailedCnt> { }

        [PXDBShort]
        [PXDefault(TypeCode.Int16, "0")]
        [PXUIField(DisplayName = "Previously Emailed Count")]
        public virtual Int16? PrevEmailedCnt { get; set; }
		#endregion
		#region OnDemand
		public abstract class onDemand : PX.Data.BQL.BqlBool.Field<onDemand> { }
		/// <summary>
		/// Gets or sets a value indicating whether the current
		/// customer statement has been generated on demand as
		/// opposed to by schedule.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.OnDemandStatement)]
		public virtual bool? OnDemand
		{
			get;
			set;
		}
		#endregion
		#region LocaleName
		public abstract class localeName : PX.Data.BQL.BqlString.Field<localeName> { }
		/// <summary>
		/// The name of the locale in which the statement
		/// has been generated.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = Messages.Locale)]
		public virtual string LocaleName
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        [PXNote(new Type[0], DescriptionField = typeof(ARStatement.statementDate))]
		public virtual Guid? NoteID
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
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
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
		#region IsParentCustomerStatement
		public abstract class isParentCustomerStatement : PX.Data.BQL.BqlBool.Field<isParentCustomerStatement> { }
		[PXBool]
		[PXDependsOnFields(typeof(customerID), typeof(statementCustomerID))]
		public bool IsParentCustomerStatement => CustomerID == StatementCustomerID;
		#endregion
	}
}