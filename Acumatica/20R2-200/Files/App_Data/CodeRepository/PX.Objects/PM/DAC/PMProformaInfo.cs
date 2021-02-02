using PX.Data;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// Virtual Table used in Report
	/// </summary>
	/// 
	[PXCacheName(Messages.ProformaInfo)]
	public class PMProformaInfo : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}

		/// <summary>
		/// Gets or sets Proforma Reference Number
		/// </summary>
		[PXDBString(PMProforma.refNbr.Length, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]

		public virtual String RefNbr
		{
			get; set;
		}
		#endregion

		#region OriginalContractTotal
		public abstract class originalContractTotal : PX.Data.BQL.BqlDecimal.Field<originalContractTotal> { }
		
		/// <summary>
		/// Gets or sets Original Contract total
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Contract Total")]
		public virtual Decimal? OriginalContractTotal
		{
			get; set;
		}
		#endregion

		#region ChangeOrderTotal
		public abstract class changeOrderTotal : PX.Data.BQL.BqlDecimal.Field<changeOrderTotal> { }
		/// <summary>
		/// Gets or sets Change Order total
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Order Total")]
		public virtual Decimal? ChangeOrderTotal
		{
			get; set;
		}
		#endregion

		#region RevisedContractTotal
		public abstract class revisedContractTotal : PX.Data.BQL.BqlDecimal.Field<revisedContractTotal> { }
		/// <summary>
		/// Gets or sets Revised Contract total
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revised Contract Total")]
		public virtual Decimal? RevisedContractTotal
		{
			get; set;
		}
		#endregion

		#region PriorProformaLineTotal
		public abstract class priorProformaLineTotal : PX.Data.BQL.BqlDecimal.Field<priorProformaLineTotal> { }
		/// <summary>
		/// Gets or sets Proforma Line total for Previous Proforma
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prior Proforma LineTotal")]
		public virtual Decimal? PriorProformaLineTotal
		{
			get; set;
		}
		#endregion

		#region CompletedToDateLineTotal
		public abstract class completedToDateLineTotal : PX.Data.BQL.BqlDecimal.Field<completedToDateLineTotal> { }
		/// <summary>
		/// Gets or sets completed to date line total
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Completed to date Line Total")]
		public virtual Decimal? CompletedToDateLineTotal
		{
			get; set;
		}
		#endregion

		#region RetainageHeldToDateTotal
		public abstract class retainageHeldToDateTotal : PX.Data.BQL.BqlDecimal.Field<retainageHeldToDateTotal> { }
		/// <summary>
		/// Gets or sets retainage held to date total
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage Held To Date Total")]
		public virtual Decimal? RetainageHeldToDateTotal
		{
			get; set;
		}
		#endregion

		#region ChangeOrderAdditions
		public abstract class changeOrderAdditions : PX.Data.BQL.BqlDecimal.Field<changeOrderAdditions> { }
		/// <summary>
		/// Gets or sets change order additions (positive changes) for current period
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Order Additions")]
		public virtual Decimal? ChangeOrderAdditions
		{
			get; set;
		}
		#endregion

		#region ChangeOrderAdditionsPrevious
		public abstract class changeOrderAdditionsPrevious : PX.Data.BQL.BqlDecimal.Field<changeOrderAdditionsPrevious> { }
		/// <summary>
		/// Gets or sets change order additions (positive changes) for previous period
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Order Additions Previous")]
		public virtual Decimal? ChangeOrderAdditionsPrevious
		{
			get; set;
		}
		#endregion

		#region ChangeOrderDeduction
		public abstract class changeOrderDeduction : PX.Data.BQL.BqlDecimal.Field<changeOrderDeduction> { }
		/// <summary>
		/// Gets or sets change order deductions (negative changes) for current period
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Order Deduction")]
		public virtual Decimal? ChangeOrderDeduction
		{
			get; set;
		}
		#endregion

		#region ChangeOrderDeductionPrevious
		public abstract class changeOrderDeductionPrevious : PX.Data.BQL.BqlDecimal.Field<changeOrderDeductionPrevious> { }
		/// <summary>
		/// Gets or sets change order deductions (negative changes) for previous period
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Change Order Deduction Previous")]
		public virtual Decimal? ChangeOrderDeductionPrevious
		{
			get; set;
		}
		#endregion


	}
}
