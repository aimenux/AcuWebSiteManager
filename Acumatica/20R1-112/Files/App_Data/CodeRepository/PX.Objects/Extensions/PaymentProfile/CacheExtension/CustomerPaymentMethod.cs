using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.Extensions.PaymentProfile
{
	public class CustomerPaymentMethod : PXMappedCacheExtension, ICCPaymentProfile
	{
		public abstract class bAccountID : PX.Data.IBqlField { }
		public virtual int? BAccountID { get; set; }
		public abstract class pMInstanceID : PX.Data.IBqlField { }
		public virtual int? PMInstanceID { get; set; }
		public abstract class cCProcessingCenterID : PX.Data.IBqlField { }
		public virtual string CCProcessingCenterID { get; set; }
		public abstract class customerCCPID : PX.Data.IBqlField { }
		public virtual string CustomerCCPID { get; set; }
		public abstract class paymentMethodID : PX.Data.IBqlField { }
		public virtual string PaymentMethodID { get; set; }
		public abstract class cashAccountID : PX.Data.IBqlField { }
		public virtual int? CashAccountID { get; set; }
		public abstract class descr : PX.Data.IBqlField {  }
		public virtual string Descr { get; set; }
		public abstract class expirationDate : PX.Data.IBqlField { }
		public virtual DateTime? ExpirationDate { get; set; }
	}
}
