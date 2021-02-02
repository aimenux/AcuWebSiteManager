using System;
using PX.Data;
using PX.Objects.Common.Attributes;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRPayGroupPeriod)]
	public partial class PRPayGroupPeriod : IBqlTable, IPeriod
	{
		private const int Base26Multiplier = 26;

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region PayGroupID
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		protected string _PayGroupID;
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(PRPayGroupYear.payGroupID))]
		[PXParent(typeof(Select<PRPayGroupYear,
			Where<PRPayGroupYear.payGroupID, Equal<Current<PRPayGroupPeriod.payGroupID>>,
				And<PRPayGroupYear.year, Equal<Current<PRPayGroupPeriod.finYear>>>>>))]
		public virtual string PayGroupID
		{
			get
			{
				return _PayGroupID;
			}
			set
			{
				_PayGroupID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID>
		{
		}
		protected String _FinPeriodID;
		[PRPayGroupPeriodID(typeof(payGroupID), typeof(startDate), typeof(endDate), typeof(endDateUI), typeof(transactionDate), true, IsKey = true)]
		[PXDefault]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false, DisplayName = "Pay Period ID")]
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
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate>
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate>
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region TransactionDate
		public abstract class transactionDate : PX.Data.BQL.BqlDateTime.Field<transactionDate>
		{
		}
		protected DateTime? _TransactionDate;
		[PXDBDate]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Transaction Date", Enabled = false)]
		public virtual DateTime? TransactionDate
		{
			get
			{
				return this._TransactionDate;
			}
			set
			{
				this._TransactionDate = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr>
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed>
		{
		}
		protected Boolean? _Closed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Closed in GL", Enabled = false)]
		public virtual Boolean? Closed
		{
			get
			{
				return this._Closed;
			}
			set
			{
				this._Closed = value;
			}
		}
		#endregion
		#region DateLocked
		public abstract class dateLocked : PX.Data.BQL.BqlBool.Field<dateLocked>
		{
		}
		protected Boolean? _DateLocked;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Date Locked", Enabled = false, Visible = false)]
		public virtual Boolean? DateLocked
		{
			get
			{
				return this._DateLocked;
			}
			set
			{
				this._DateLocked = value;
			}
		}
		#endregion
		#region PeriodNbr
		public abstract class periodNbr : PX.Data.BQL.BqlString.Field<periodNbr> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Period Nbr.", Enabled = false)]
		public virtual string PeriodNbr { get; set; }
		#endregion
		#region PeriodNbrAsInt
		public abstract class periodNbrAsInt : PX.Data.BQL.BqlInt.Field<periodNbrAsInt> { }
		[PXInt]
		public virtual int? PeriodNbrAsInt
		{
			[PXDependsOnFields(typeof(PRPayGroupPeriod.periodNbr))]
			get
			{
				// If PeriodNbr is within "00"-"99", parse it.
				// If PeriodNbr id within "AA"-"ZZ", return 100 + (int equivalent in base-26).
				if (!string.IsNullOrEmpty(PeriodNbr))
				{
					if (PeriodNbr[0] >= '0' && PeriodNbr[0] <= '9')
					{
						return int.Parse(PeriodNbr);
					}
					else
					{
						string upperPeriodNbr = PeriodNbr.ToUpper();
						return (upperPeriodNbr[0] - 'A' + 1) * Base26Multiplier + (upperPeriodNbr[1] - 'A') + 100;
					}
				}
				return null;
			}
		}
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear>
		{
		}
		protected String _FinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDBDefault(typeof(PRPayGroupYear.year))]
		[PXUIField(DisplayName = "FinYear", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXFormula(null, typeof(CountCalc<PRPayGroupYear.finPeriods>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
		#region OriginalDescr
		public abstract class originalDescr : PX.Data.BQL.BqlString.Field<originalDescr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(Visible = false)]
		[FormulaDefault(typeof(descr))]
		public virtual string OriginalDescr { get; set; }
		#endregion
		#region OriginalPeriodNbr
		public abstract class originalPeriodNbr : PX.Data.BQL.BqlString.Field<originalPeriodNbr> { }
		[PXDBString(2, IsFixed = true)]
		[FormulaDefault(typeof(periodNbr))]
		[PXUIField(Visible = false)]
		public virtual string OriginalPeriodNbr { get; set; }
		#endregion
		#region OriginalYear
		public abstract class originalYear : PX.Data.BQL.BqlString.Field<originalYear> { }
		[PXDBString(4, IsFixed = true)]
		[FormulaDefault(typeof(finYear))]
		[PXUIField(Visible = false)]
		public virtual string OriginalYear { get; set; }
		#endregion
		#region OriginalFinPeriodID
		public abstract class originalFinPeriodID : PX.Data.BQL.BqlString.Field<originalFinPeriodID> { }
		[PXDBString(6, IsFixed = true)]
		[FormulaDefault(typeof(finPeriodID))]
		[PXUIField(Visible = false)]
		public virtual string OriginalFinPeriodID { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
		#region EndDateUI
		public abstract class endDateUI : PX.Data.BQL.BqlDateTime.Field<endDateUI>
		{
		}

		[PXDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		[PRPayGroupPeriodEndDateUI(typeof(startDate), typeof(endDate))]
		public virtual DateTime? EndDateUI { get; set; }
		#endregion
		#region Custom
		public abstract class custom : PX.Data.BQL.BqlBool.Field<custom>
		{
		}
		protected Boolean? _Custom;
		public virtual Boolean? Custom
		{
			get
			{
				return this._Custom;
			}
			set
			{
				this._Custom = value;
			}
		}
		#endregion
	}
}
