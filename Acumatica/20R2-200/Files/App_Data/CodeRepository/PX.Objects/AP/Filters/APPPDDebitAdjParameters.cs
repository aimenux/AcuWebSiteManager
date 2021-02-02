using System;
using PX.Data;

using PX.Objects.GL;

namespace PX.Objects.AP
{

	[Serializable]
	public partial class APPPDDebitAdjParameters : IBqlTable
	{
		#region ApplicationDate
		public abstract class applicationDate : PX.Data.BQL.BqlDateTime.Field<applicationDate> { }
		protected DateTime? _ApplicationDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual DateTime? ApplicationDate
		{
			get
			{
				return _ApplicationDate;
			}
			set
			{
				_ApplicationDate = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected int? _BranchID;
		[Branch]
		public virtual int? BranchID
		{
			get
			{
				return _BranchID;
			}
			set
			{
				_BranchID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[Vendor]
		public virtual int? VendorID
		{
			get; set;
		}
		#endregion
		#region GenerateOnePerVendor
		public abstract class generateOnePerVendor : PX.Data.BQL.BqlBool.Field<generateOnePerVendor> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Debit Adjustments by Vendor", Visibility = PXUIVisibility.Visible)]
		public virtual bool? GenerateOnePerVendor
		{
			get; set;
		}
		#endregion
		#region DebitAdjDate
		public abstract class debitAdjDate : PX.Data.BQL.BqlDateTime.Field<debitAdjDate> { }
		protected DateTime? _DebitAdjDate;
		[PXDBDate]
		[PXFormula(typeof(Switch<Case<Where<APPPDDebitAdjParameters.generateOnePerVendor, Equal<True>>, Current<AccessInfo.businessDate>>, Null>))]
		[PXUIField(DisplayName = "Debit Adjustment Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? DebitAdjDate
		{
			get
			{
				return _DebitAdjDate;
			}
			set
			{
				_DebitAdjDate = value;
			}
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected string _FinPeriodID;
		[APOpenPeriod(typeof(APPPDDebitAdjParameters.debitAdjDate), typeof(APPPDDebitAdjParameters.branchID))]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
		public virtual string FinPeriodID
		{
			get
			{
				return _FinPeriodID;
			}
			set
			{
				_FinPeriodID = value;
			}
		}
		#endregion
	}
}
