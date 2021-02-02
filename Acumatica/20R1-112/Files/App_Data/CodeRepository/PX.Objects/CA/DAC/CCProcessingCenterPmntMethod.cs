using System;
using PX.Data;
using PX.Objects.Common.Attributes;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(Messages.CCProcessingCenterPmntMethod)]
	public partial class CCProcessingCenterPmntMethod : IBqlTable
	{
		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CCProcessingCenter.processingCenterID))]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID, Where<CCProcessingCenter.isActive, Equal<True>>>))]
		[PXParent(typeof(Select<CCProcessingCenter,
			Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterPmntMethod.processingCenterID>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID")]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
			Where<PaymentMethod.aRIsProcessingRequired, Equal<True>>>))]
		[PXUIField(DisplayName = "Payment Method")]
		[PXParent(typeof(Select<PaymentMethod,
			Where<PaymentMethod.paymentMethodID, Equal<Current<CCProcessingCenterPmntMethod.paymentMethodID>>>>))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		[Common.UniqueBool(typeof(CCProcessingCenterPmntMethod.paymentMethodID))]
		public virtual bool? IsDefault
		{
			get;
			set;
		}
		#endregion
	}
}
