using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Data;
namespace PX.Objects.Extensions.PaymentTransaction
{
	public class Payment : PXMappedCacheExtension, ICCPayment
	{
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		public int? PMInstanceID { get; set; }
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		public decimal? CuryDocBal { get; set; }
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public string CuryID { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public string DocType { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public string RefNbr { get; set; }
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public string OrigDocType { get; set; }
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		public string OrigRefNbr { get; set; }
		public abstract class refTranExtNbr : PX.Data.BQL.BqlString.Field<refTranExtNbr> { }
		public string RefTranExtNbr { get; set; }
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public bool? Released { get; set; }
	}
}
