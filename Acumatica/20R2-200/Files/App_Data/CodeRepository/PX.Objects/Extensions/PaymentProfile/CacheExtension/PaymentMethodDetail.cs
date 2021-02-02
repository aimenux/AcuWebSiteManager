using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.Extensions.PaymentProfile
{
	public class PaymentMethodDetail : PXMappedCacheExtension, ICCPaymentMethodDetail
	{
		public abstract class paymentMethodID : IBqlField { }
		public virtual string PaymentMethodID { get; set; }
		public abstract class useFor : IBqlField { }
		public virtual string UseFor { get; set; }
		public abstract class detailID : IBqlField { }
		public virtual string DetailID { get; set; }
		public abstract class descr : IBqlField { }
		public virtual string Descr { get; set; }
		public abstract class isEncrypted : IBqlField { }
		public virtual bool? IsEncrypted { get; set; }
		public abstract class isRequired : IBqlField { }
		public virtual bool? IsRequired{ get;set; }
		public abstract class isIdentifier : IBqlField { }
		public virtual bool? IsIdentifier { get; set; }
		public abstract class isExpirationDate : IBqlField { }
		public virtual bool? IsExpirationDate { get; set; }
		public abstract class isOwnerName : IBqlField { }
		public virtual bool? IsOwnerName { get; set; }
		public abstract class isCCProcessingID : IBqlField { }
		public virtual bool? IsCCProcessingID { get; set; }
		public abstract class isCVV : PX.Data.IBqlField { }
		public virtual bool? IsCVV { get; set; }
	}
}
