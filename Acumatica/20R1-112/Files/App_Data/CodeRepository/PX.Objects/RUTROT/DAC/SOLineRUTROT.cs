using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.RUTROT
{
	public class SOLineRUTROT : PXCacheExtension<SOLine>, IRUTROTableLine
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region IsRUTROTDeductible
		public abstract class isRUTROTDeductible : PX.Data.BQL.BqlBool.Field<isRUTROTDeductible> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the line is subjected to ROT and RUT deduction.
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">ROT and RUT Deduction</see> feature is enabled,
		/// the value of the <see cref="BranchRUTROT.AllowsRUTROT"/> field is <c>true</c> for the 
		/// <see cref="SOOrder.BranchID">branch of the document</see>
		/// and the document has a compatible type (see <see cref="SOOrder.OrderType"/>, <see cref="RUTROTHelper.IsRUTROTcompatibleType"/>).
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "ROT or RUT deductible", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual bool? IsRUTROTDeductible
		{
			get;
			set;
		}
		#endregion
		#region RUTROTItemType
		public abstract class rUTROTItemType : PX.Data.BQL.BqlString.Field<rUTROTItemType> { }
		/// <summary>
		/// The type of the line.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="RUTROTItemTypes.ListAttribute"/>.
		/// By default, the value of the field is the <see cref="InventoryItemRUTROT.RUTROTItemType">type</see>
		///	of the<see cref= "SOLine.InventoryID" > inventory item</see> associated with the line, or
		/// <see cref="RUTROTItemTypes.OtherCost"/> if the inventory item is not specified.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(RUTROTItemTypes.OtherCost, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Item Type", FieldClass = RUTROTMessages.FieldClass)]
		[RUTROTItemTypes.List]
		public virtual string RUTROTItemType
		{
			get;
			set;
		}
		#endregion
		#region RUTROTWorkTypeID
		public abstract class rUTROTWorkTypeID : PX.Data.BQL.BqlInt.Field<rUTROTWorkTypeID> { }
		/// <summary>
		/// The identifier of the <see cref="RUTROTWorkType">work type</see> selected for the line.
		/// The list of available <see cref="RUTROTWorkType">work types</see> depends on 
		/// the current <see cref="SOOrder.OrderDate">document date</see> 
		/// and <see cref="RUTROT.RUTROTType"/> of the document.
		/// </summary>
		/// <value>
		/// Defaults to <see cref="InventoryItemRUTROT.RUTROTWorkTypeID"/>
		/// of the <see cref="SOLine.InventoryID">inventory item</see> associated with the line.
		/// Corresponds to the <see cref="RUTROTWorkType.WorkTypeID"/>.
		/// </value>
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Type of Work", FieldClass = RUTROTMessages.FieldClass)]
		[WorkTypeSelector(typeof(SOOrder.orderDate))]
		public virtual int? RUTROTWorkTypeID
		{
			get;
			set;
		}
		#endregion
		#region CuryRUTROTTaxAmountDeductible
		public abstract class curyRUTROTTaxAmountDeductible : PX.Data.BQL.BqlDecimal.Field<curyRUTROTTaxAmountDeductible> { }
		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="SOLine">line</see> in the selected currency.
		/// </summary>
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(rUTROTTaxAmountDeductible))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRUTROTTaxAmountDeductible
		{
			get;
			set;
		}
		#endregion
		#region RUTROTTaxAmountDeductible
		public abstract class rUTROTTaxAmountDeductible : PX.Data.BQL.BqlDecimal.Field<rUTROTTaxAmountDeductible> { }
		/// <summary>
		/// The amount of tax (VAT) associated with the <see cref="SOLine">line</see> in the base currency.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RUTROTTaxAmountDeductible
		{
			get;
			set;
		}
		#endregion
		#region CuryRUTROTAvailableAmt
		public abstract class curyRUTROTAvailableAmt : PX.Data.BQL.BqlDecimal.Field<curyRUTROTAvailableAmt> { }
		/// <summary>
		/// The portion of the line amount that is RUT and ROT deductible in the selected currency.
		/// This field is relevant only if <see cref="SOOrderRUTROT.IsRUTROTDeductible"/> is <c>true</c>.
		/// </summary>
		[PXParent(typeof(Select<RUTROT, Where<RUTROT.docType, Equal<Current<SOLine.orderType>>, And<RUTROT.refNbr, Equal<Current<SOLine.orderNbr>>>>>), LeaveChildren = true)]
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(rUTROTAvailableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deductible Amount", Enabled = false, FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		[PXFormula(typeof(Switch<Case<Where<isRUTROTDeductible, Equal<True>>,
							Mult<curyRUTROTTotal, Mult<IsNull<Current<RUTROT.deductionPct>, decimal0>, decimalPct>>>,
						decimal0>),
		  typeof(SumCalc<RUTROT.curyTotalAmt>), FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? CuryRUTROTAvailableAmt
		{
			get;
			set;
		}
		#endregion
		#region RUTROTAvailableAmt
		public abstract class rUTROTAvailableAmt : PX.Data.BQL.BqlDecimal.Field<rUTROTAvailableAmt> { }
		/// <summary>
		/// The portion of the line amount that is RUT and ROT deductible in the base currency.
		/// This field is relevant only if <see cref="SOOrderRUTROT.IsRUTROTDeductible"/> is <c>true</c>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RUTROTAvailableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRUTROTTotal
		public abstract class curyRUTROTTotal : PX.Data.BQL.BqlDecimal.Field<curyRUTROTTotal> { }
		/// <summary>
		/// The total amount of the line to be included in one of document 
		/// totals based on <see cref="SOLineRUTROT.RUTROTItemType">line type</see> in the selected currency.
		/// </summary>
		/// <value>
		/// Equals the following value: line amount plus VAT amount minus discuont amount.
		/// </value>
		[PXCurrency(typeof(SOLine.curyInfoID), typeof(rUTROTTotal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Add<Sub<SOLine.curyExtPrice, SOLine.curyDiscAmt>, IsNull<curyRUTROTTaxAmountDeductible, decimal0>>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<rUTROTItemType, Equal<RUTROTItemTypes.materialCost>>,
			curyRUTROTTotal>>),
			typeof(SumCalc<RUTROT.curyMaterialCost>), FieldClass = RUTROTMessages.FieldClass)]
		[PXUnboundFormula(typeof(Switch<Case<Where<rUTROTItemType, Equal<RUTROTItemTypes.otherCost>>,
			curyRUTROTTotal>>),
			typeof(SumCalc<RUTROT.curyOtherCost>), FieldClass = RUTROTMessages.FieldClass)]
		[PXUnboundFormula(typeof(Switch<Case<Where<rUTROTItemType, Equal<RUTROTItemTypes.service>>,
			curyRUTROTTotal>>),
			typeof(SumCalc<RUTROT.curyWorkPrice>), FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? CuryRUTROTTotal
		{
			get;
			set;
		}
		#endregion
		#region RUTROTTotal
		public abstract class rUTROTTotal : PX.Data.BQL.BqlDecimal.Field<rUTROTTotal> { }
		/// <summary>
		/// The total amount of the line to be included in one of document 
		/// totals based on <see cref="SOLineRUTROT.RUTROTItemType">line type</see> in the base currency.
		/// </summary>
		[PXBaseCury]
		public virtual decimal? RUTROTTotal
		{
			get;
			set;
		}
		#endregion
		public int? GetInventoryID()
		{
			return Base.InventoryID;
		}

		public IBqlTable GetBaseDocument()
		{
			return Base;
		}
	}
}
