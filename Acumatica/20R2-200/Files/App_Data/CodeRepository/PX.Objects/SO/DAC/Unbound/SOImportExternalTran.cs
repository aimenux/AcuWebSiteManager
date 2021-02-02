using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO.DAC
{
	[PXCacheName(Messages.SOImportCCPayment)]
	[PXVirtual]
	public class SOImportExternalTran : IBqlTable
	{
		#region PaymentMethodID
		public abstract class paymentMethodID : Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method", Required = true)]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : Data.BQL.BqlInt.Field<pMInstanceID> { }

		[PXInt()]
		[PXUIField(DisplayName = "Card/Account No")]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion

		#region TranNumber
		public abstract class tranNumber : Data.BQL.BqlString.Field<tranNumber> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "External Tran. ID")]
		public virtual string TranNumber
		{
			get;
			set;
		}
		#endregion
		#region ProcessingCenterID
		public abstract class processingCenterID : Data.BQL.BqlString.Field<processingCenterID> { }

		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center ID")]
		[PXDefault(typeof(Coalesce<
			Search<CustomerPaymentMethod.cCProcessingCenterID,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<pMInstanceID>>>>,
			Search2<CCProcessingCenterPmntMethod.processingCenterID,
				InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>,
				Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<paymentMethodID>>,
					And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
					And<CCProcessingCenterPmntMethod.isDefault, Equal<True>,
					And<CCProcessingCenter.isActive, Equal<True>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<CCProcessingCenter.processingCenterID,
			InnerJoin<CCProcessingCenterPmntMethod, On<CCProcessingCenterPmntMethod.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>>,
			Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<paymentMethodID>>,
				And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
				And<CCProcessingCenter.isActive, Equal<True>>>>>), DescriptionField = typeof(CCProcessingCenter.name), ValidateValue = false)]
		[DisabledProcCenter(CheckFieldValue = DisabledProcCenterAttribute.CheckFieldVal.ProcessingCenterId)]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
	}
}
