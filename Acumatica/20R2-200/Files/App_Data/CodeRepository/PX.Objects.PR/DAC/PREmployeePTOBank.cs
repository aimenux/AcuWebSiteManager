using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PREmployeePTOBank)]
	public class PREmployeePTOBank : IBqlTable, IPTOBank
	{
		public class PTOBankFK : PRPTOBank.PK.ForeignKeyOf<PREmployeeClassPTOBank>.By<bankID> { }

		#region BAccountID
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeePTOBank.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion

		#region BankID
		[PXDBString(3, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "PTO Bank")]
		[PXSelector(typeof(SearchFor<PRPTOBank.bankID>), DescriptionField = typeof(PRPTOBank.description))]
		[PXRestrictor(typeof(Where<PRPTOBank.isActive.IsEqual<True>>), Messages.InactivePTOBank, typeof(PRPTOBank.bankID))]
		[PXForeignReference(typeof(PTOBankFK))]
		public virtual string BankID { get; set; }
		public abstract class bankID : PX.Data.BQL.BqlString.Field<bankID> { }
		#endregion

		#region EmployeeClassID
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID")]
		[PXUnboundDefault(typeof(PREmployee.employeeClassID))]
		public string EmployeeClassID { get; set; }
		public abstract class employeeClassID : PX.Data.BQL.BqlString.Field<employeeClassID> { }
		#endregion

		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion

		#region UseClassDefault
		[PXDBBool]
		[PXUIField(DisplayName = "Use Class Default Values")]
		[PXDefault(false)]
		public virtual bool? UseClassDefault { get; set; }
		public abstract class useClassDefault : PX.Data.BQL.BqlBool.Field<useClassDefault> { }
		#endregion

		#region AccrualRate
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Accrual %")]
		[PXDefault]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>.And<isActive.IsEqual<True>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.accrualRate),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual Decimal? AccrualRate { get; set; }
		public abstract class accrualRate : PX.Data.BQL.BqlDecimal.Field<accrualRate> { }
		#endregion

		#region AccrualLimit
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Accrual Limit")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>.And<isActive.IsEqual<True>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.accrualLimit),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual Decimal? AccrualLimit
		{
			get => _AccrualLimit != 0 ? _AccrualLimit : null;
			set => _AccrualLimit = value;
		}
		private decimal? _AccrualLimit;
		public abstract class accrualLimit : PX.Data.BQL.BqlDecimal.Field<accrualLimit> { }
		#endregion

		#region CarryoverType
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Carryover Type")]
		[CarryoverType.List]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>.And<isActive.IsEqual<True>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.carryoverType),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual string CarryoverType { get; set; }
		public abstract class carryoverType : PX.Data.BQL.BqlString.Field<carryoverType> { }
		#endregion

		#region CarryoverAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Carryover Amount")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>
			.And<carryoverType.IsEqual<CarryoverType.partial>>
			.And<isActive.IsEqual<True>>>))]
		[PXFormula(typeof(Switch<Case<Where<carryoverType.IsNotEqual<CarryoverType.partial>>, Null>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.carryoverAmount),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual Decimal? CarryoverAmount { get; set; }
		public abstract class carryoverAmount : PX.Data.BQL.BqlDecimal.Field<carryoverAmount> { }
		#endregion

		#region FrontLoadingAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Front Loading Amount")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>.And<isActive.IsEqual<True>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.frontLoadingAmount),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual Decimal? FrontLoadingAmount { get; set; }
		public abstract class frontLoadingAmount : PX.Data.BQL.BqlDecimal.Field<frontLoadingAmount> { }
		#endregion

		#region StartDate
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date")]
		[PXUIEnabled(typeof(Where<useClassDefault.IsEqual<False>.And<isActive.IsEqual<True>>>))]
		[DefaultSource(typeof(PREmployeePTOBank.useClassDefault),
			typeof(PREmployeeClassPTOBank.startDate),
			new Type[] { typeof(PREmployeeClassPTOBank.bankID), typeof(PREmployeeClassPTOBank.employeeClassID) },
			new Type[] { typeof(PREmployeePTOBank.bankID), typeof(PREmployeePTOBank.employeeClassID) })]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion

		#region AccumulatedAmount
		[PXDecimal]
		[PXUIField(DisplayName = "Hours Accrued", Enabled = false)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AccumulatedAmount { get; set; }
		public abstract class accumulatedAmount : PX.Data.BQL.BqlDecimal.Field<accumulatedAmount> { }
		#endregion

		#region UsedAmount
		[PXDecimal]
		[PXUIField(DisplayName = "Hours Used", Enabled = false)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UsedAmount { get; set; }
		public abstract class usedAmount : PX.Data.BQL.BqlDecimal.Field<usedAmount> { }
		#endregion

		#region AvailableAmount
		[PXDecimal]
		[PXUIField(DisplayName = "Hours Available", Enabled = false)]
		public virtual Decimal? AvailableAmount { get; set; }
		public abstract class availableAmount : PX.Data.BQL.BqlDecimal.Field<availableAmount> { }
		#endregion

		#region AllowDelete
		[PXBool]
		[PXUIField(Visible = false)]
		public virtual bool? AllowDelete { get; set; }
		public abstract class allowDelete : PX.Data.BQL.BqlBool.Field<allowDelete> { }
		#endregion

		#region AllowNegativeBalance
		[PXBool]
		[PXUIField(DisplayName = "Allow Negative Balance")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.allowNegativeBalance>.Where<PRPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>))]
		public virtual bool? AllowNegativeBalance { get; set; }
		public abstract class allowNegativeBalance : PX.Data.BQL.BqlBool.Field<allowNegativeBalance> { }
		#endregion

		#region CarryoverPayMonthLimit
		[PXInt]
		[PXUIField(DisplayName = "Pay Carryover after (Months)")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.carryoverPayMonthLimit>.Where<PRPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>))]
		public virtual int? CarryoverPayMonthLimit { get; set; }
		public abstract class carryoverPayMonthLimit : PX.Data.BQL.BqlInt.Field<carryoverPayMonthLimit> { }
		#endregion

		#region DisburseFromCarryover
		[PXBool]
		[PXUIField(DisplayName = "Can Only Disburse from Carryover")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.disburseFromCarryover>.Where<PRPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>))]
		public virtual bool? DisburseFromCarryover { get; set; }
		public abstract class disburseFromCarryover : PX.Data.BQL.BqlBool.Field<disburseFromCarryover> { }
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
	}
}