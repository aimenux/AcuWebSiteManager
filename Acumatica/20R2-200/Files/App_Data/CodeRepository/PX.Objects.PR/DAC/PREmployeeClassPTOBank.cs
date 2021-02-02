using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PREmployeeClassPTOBank)]
	public class PREmployeeClassPTOBank : IBqlTable, IPTOBank
	{
		public class PK : PrimaryKeyOf<PREmployeeClassPTOBank>.By<employeeClassID, bankID>
		{
			public static PREmployeeClassPTOBank Find(PXGraph graph, string employeeClassID, string bankID) => FindBy(graph, employeeClassID, bankID);
		}

		public class PTOBankFK : PRPTOBank.PK.ForeignKeyOf<PREmployeeClassPTOBank>.By<bankID> { }

		#region EmployeeClassID
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Employee Class")]
		[PXDBDefault(typeof(PREmployeeClass.employeeClassID))]
		[PXParent(typeof(Select<PREmployeeClass, Where<PREmployeeClass.employeeClassID, Equal<Current<PREmployeeClassPTOBank.employeeClassID>>>>))]
		[PXReferentialIntegrityCheck]
		public virtual string EmployeeClassID { get; set; }
		public abstract class employeeClassID : PX.Data.BQL.BqlString.Field<employeeClassID> { }
		#endregion

		#region BankID
		[PXDBString(3, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "PTO Bank")]
		[PXSelector(typeof(SearchFor<PRPTOBank.bankID>), DescriptionField = typeof(PRPTOBank.description))]
		[PXRestrictor(typeof(Where<PRPTOBank.isActive.IsEqual<True>>), Messages.InactivePTOBank, typeof(PRPTOBank.bankID))]
		[PXForeignReference(typeof(PTOBankFK))]
		[PXReferentialIntegrityCheck]
		public virtual string BankID { get; set; }
		public abstract class bankID : PX.Data.BQL.BqlString.Field<bankID> { }
		#endregion

		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion

		#region AccrualRate
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Accrual %")]
		public virtual Decimal? AccrualRate { get; set; }
		public abstract class accrualRate : PX.Data.BQL.BqlDecimal.Field<accrualRate> { }
		#endregion

		#region AccrualLimit
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Accrual Limit")]
		public virtual Decimal? AccrualLimit
		{
			get => _AccrualLimit != 0 ? _AccrualLimit : null;
			set => _AccrualLimit = value;
		}
		private decimal? _AccrualLimit;
		public abstract class accrualLimit : PX.Data.BQL.BqlDecimal.Field<accrualLimit> { }
		#endregion

		#region StartDate
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion

		#region CarryoverType
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Carryover Type")]
		[CarryoverType.List]
		public virtual string CarryoverType { get; set; }
		public abstract class carryoverType : PX.Data.BQL.BqlString.Field<carryoverType> { }
		#endregion

		#region CarryoverAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Carryover Amount")]
		[PXFormula(typeof(Switch<Case<Where<carryoverType.IsNotEqual<CarryoverType.partial>>, Null>>))]
		[PXUIEnabled(typeof(carryoverType.IsEqual<CarryoverType.partial>))]
		public virtual Decimal? CarryoverAmount { get; set; }
		public abstract class carryoverAmount : PX.Data.BQL.BqlDecimal.Field<carryoverAmount> { }
		#endregion

		#region FrontLoadingAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Front Loading Amount")]
		public virtual Decimal? FrontLoadingAmount { get; set; }
		public abstract class frontLoadingAmount : PX.Data.BQL.BqlDecimal.Field<frontLoadingAmount> { }
		#endregion

		#region AllowNegativeBalance
		[PXBool]
		[PXUIField(DisplayName = "Allow Negative Balance")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.allowNegativeBalance>.Where<PRPTOBank.bankID.IsEqual<PREmployeeClassPTOBank.bankID>>))]
		public virtual bool? AllowNegativeBalance { get; set; }
		public abstract class allowNegativeBalance : PX.Data.BQL.BqlBool.Field<allowNegativeBalance> { }
		#endregion

		#region CarryoverPayMonthLimit
		[PXInt]
		[PXUIField(DisplayName = "Pay Carryover after (Months)")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.carryoverPayMonthLimit>.Where<PRPTOBank.bankID.IsEqual<PREmployeeClassPTOBank.bankID>>))]
		public virtual int? CarryoverPayMonthLimit { get; set; }
		public abstract class carryoverPayMonthLimit : PX.Data.BQL.BqlInt.Field<carryoverPayMonthLimit> { }
		#endregion

		#region DisburseFromCarryover
		[PXBool]
		[PXUIField(DisplayName = "Can Only Disburse from Carryover")]
		[PXDBScalar(typeof(SearchFor<PRPTOBank.disburseFromCarryover>.Where<PRPTOBank.bankID.IsEqual<PREmployeeClassPTOBank.bankID>>))]
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