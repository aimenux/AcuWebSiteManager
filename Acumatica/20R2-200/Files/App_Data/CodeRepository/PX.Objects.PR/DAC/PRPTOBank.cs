using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRPTOBank)]
	public class PRPTOBank : IBqlTable, IPTOBank
	{
		public class PK : PrimaryKeyOf<PRPTOBank>.By<bankID>
		{
			public static PRPTOBank Find(PXGraph graph, string bankID) => FindBy(graph, bankID);
		}

		#region BankID
		[PXDBString(3, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(SearchFor<PRPTOBank.bankID>), DescriptionField = typeof(PRPTOBank.description))]
		[PXReferentialIntegrityCheck]
		public virtual string BankID { get; set; }
		public abstract class bankID : PX.Data.BQL.BqlString.Field<bankID> { }
		#endregion

		#region Description
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Description { get; set; }
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion

		#region AccrualRate
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Default Accrual %")]
		[PXDefault(TypeCode.Decimal, "0")]
		public virtual Decimal? AccrualRate { get; set; }
		public abstract class accrualRate : PX.Data.BQL.BqlDecimal.Field<accrualRate> { }
		#endregion

		#region EarningTypeCD
		[PXDBString(2, IsFixed = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Disbursing Earning Type")]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>.
			Where<EPEarningType.isActive.IsEqual<True>.
				And<EPEarningType.typeCD.IsNotInSubselect<SearchFor<PRSetup.holidaysType>.Where<PRSetup.holidaysType.IsNotNull>>>.
				And<EPEarningType.typeCD.IsNotInSubselect<SearchFor<PRPTOBank.earningTypeCD>.Where<bankID.FromCurrent.IsNull.Or<PRPTOBank.bankID.IsNotEqual<bankID.FromCurrent>>>>>.
				And<PREarningType.isPTO.IsEqual<True>>>), DescriptionField = typeof(EPEarningType.description))]
		[PXDefault]
		[PXCheckUnique(ErrorMessage = Messages.DuplicateEarningType)]
		[PXForeignReference(typeof(Field<earningTypeCD>.IsRelatedTo<EPEarningType.typeCD>))]
		public virtual string EarningTypeCD { get; set; }
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		#endregion

		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; } //ToDo AC-149516: Check that the Earning Type is still correct when the PTOBank is re-activated.
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion

		#region IsCertifiedJobAccrual
		[PXDBBool]
		[PXUIField(DisplayName = "Accrue on Certified Job Only")]
		[PXDefault(false)]
		public virtual bool? IsCertifiedJobAccrual { get; set; }
		public abstract class isCertifiedJobAccrual : PX.Data.BQL.BqlBool.Field<isCertifiedJobAccrual> { }
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
		[PXUIField(DisplayName = "Start Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		#endregion

		#region CarryoverType
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Carryover Type")]
		[PXDefault(typeof(CarryoverType.none))]
		[CarryoverType.List]
		public virtual string CarryoverType { get; set; }
		public abstract class carryoverType : PX.Data.BQL.BqlString.Field<carryoverType> { }
		#endregion

		#region CarryoverAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Carryover Amount")]
		[PXUIEnabled(typeof(Where<PRPTOBank.carryoverType.IsEqual<CarryoverType.partial>>))]
		public virtual Decimal? CarryoverAmount { get; set; }
		public abstract class carryoverAmount : PX.Data.BQL.BqlDecimal.Field<carryoverAmount> { }
		#endregion

		#region FrontLoadingAmount
		[PXDBDecimal(MinValue = 0)]
		[PXUIField(DisplayName = "Front Loading Amount")]
		[PXDefault(TypeCode.Decimal, "0")]
		public virtual Decimal? FrontLoadingAmount { get; set; }
		public abstract class frontLoadingAmount : PX.Data.BQL.BqlDecimal.Field<frontLoadingAmount> { }
		#endregion

		#region AllowNegativeBalance
		[PXDBBool]
		[PXUIField(DisplayName = "Allow Negative Balance")]
		[PXDefault(true)]
		public virtual bool? AllowNegativeBalance { get; set; }
		public abstract class allowNegativeBalance : PX.Data.BQL.BqlBool.Field<allowNegativeBalance> { }
		#endregion

		#region CarryoverPayMonthLimit
		[PXDBInt(MinValue = 0, MaxValue = 12)]
		[PXUIField(DisplayName = "Pay Carryover after (Months)")]
		[PXUIEnabled(typeof(Where<PRPTOBank.carryoverType.IsEqual<CarryoverType.paidOnTimeLimit>>))]
		public virtual int? CarryoverPayMonthLimit { get; set; }
		public abstract class carryoverPayMonthLimit : PX.Data.BQL.BqlInt.Field<carryoverPayMonthLimit> { }
		#endregion

		#region DisburseFromCarryover
		[PXDBBool]
		[PXUIField(DisplayName = "Can Only Disburse from Carryover")]
		[PXDefault(false)]
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
