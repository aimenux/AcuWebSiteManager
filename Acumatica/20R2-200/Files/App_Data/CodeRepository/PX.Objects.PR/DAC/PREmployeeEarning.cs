using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PREmployeeEarning)]
	[Serializable]
	public class PREmployeeEarning : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeEarning.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PREmployee.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual int? LineNbr { get; set; }
		#endregion
		#region TypeCD
		public abstract class typeCD : PX.Data.BQL.BqlString.Field<typeCD> { }
		[PXDBString(2, IsUnicode = true, InputMask = ">LL")]
		[PXUIField(DisplayName = "Earning Type")]
		[PXDefault]
		[EmployeeEarningTypeCDSelector(
			typeof(SelectFrom<EPEarningType>.CrossJoin<PRSetup>.SearchFor<EPEarningType.typeCD>),
			DescriptionField = typeof(EPEarningType.description))]
		[PXRestrictor(typeof(Where<EPEarningType.isActive.IsEqual<True>>), Messages.EarningTypeIsNotActive, typeof(EPEarningType.typeCD))]
		[PXForeignReference(typeof(Field<typeCD>.IsRelatedTo<EPEarningType.typeCD>))] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
		public string TypeCD { get; set; }
		#endregion
		#region IsPiecework
		public abstract class isPiecework : PX.Data.BQL.BqlBool.Field<isPiecework> { }
		[PXBool]
		[PXFormula(typeof(Selector<typeCD, PREarningType.isPiecework>))]
		[PXUIField(DisplayName = "Piecework", Visible = false)]
		public bool? IsPiecework { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Start Date")]
		public DateTime? StartDate { get; set; }
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDBDate]
		[PXUIField(DisplayName = "End Date")]
		public DateTime? EndDate { get; set; }
		#endregion
		#region IsActive //ToDo AC-149516: Check that the Earning Type is still correct when the Employee Earning is re-activated.
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public bool? IsActive { get; set; }
		#endregion
		#region PayRate
		public abstract class payRate : PX.Data.BQL.BqlDecimal.Field<payRate> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Pay Rate")]
		public decimal? PayRate { get; set; }
		#endregion
		#region UnitType
		public abstract class unitType : PX.Data.BQL.BqlString.Field<unitType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Unit of Pay")]
		[UnitType.List(typeof(isPiecework), typeof(unitType))]
		public string UnitType { get; set; }
		#endregion
	
		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}

	public class EmployeeEarningTypeCDSelectorAttribute : PXSelectorAttribute
	{
		public EmployeeEarningTypeCDSelectorAttribute(Type fieldType) : base(fieldType) { }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			Type salaryTypeWhere = EarningTypeCategory.ListAttribute.GetWhereClauseForEarningTypeCategory(sender.Graph.Caches[_PrimarySelect.GetFirstTable()], EarningTypeCategory.Salary);
			Type selectorWhere = BqlCommand.Compose(
				typeof(Where2<,>),
				typeof(Where<,,>),
				typeof(PRSetup.enablePieceworkEarningType),
				typeof(Equal<>),
				typeof(True),
				typeof(And<,>),
				typeof(PREarningType.isPiecework),
				typeof(Equal<>),
				typeof(True),
				typeof(Or<>),
				salaryTypeWhere);

			_PrimarySelect = _PrimarySelect.WhereAnd(selectorWhere);
			_PrimarySimpleSelect = _PrimarySimpleSelect.WhereAnd(selectorWhere);
			_LookupSelect = _LookupSelect.WhereAnd(selectorWhere);
		}
	}
}