using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.Extensions.PaymentProfile
{
	public class CustomerPaymentMethodDetail : PXMappedCacheExtension, ICCPaymentProfileDetail
	{
		public abstract class pMInstanceID : PX.Data.IBqlField { }
		public virtual int? PMInstanceID{ get; set; }
		public abstract class paymentMethodID : PX.Data.IBqlField { }
		public virtual string PaymentMethodID{ get; set; }
		public abstract class detailID : PX.Data.IBqlField { }
		public virtual string DetailID { get; set; }
		public abstract class value : PX.Data.IBqlField { }
		public virtual string Value { get; set; }
		public abstract class isIdentifier : IBqlField { }
		public virtual bool? IsIdentifier { get; set; }
		public abstract class isCCProcessingID : IBqlField { }
		public virtual bool? IsCCProcessingID { get; set; }
	}
}
