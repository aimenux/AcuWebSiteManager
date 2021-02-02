using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.Extensions.PaymentTransaction
{
	[PXHidden]
	public class ExternalTransactionDetail : PXMappedCacheExtension, IExternalTransaction
	{
		public abstract class transactionID : PX.Data.BQL.BqlInt.Field<transactionID> { }
		public int? TransactionID { get; set; }
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		public int? PMInstanceID { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public string DocType { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public string RefNbr { get; set; }
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public string OrigDocType { get; set; }
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		public string OrigRefNbr { get; set; }
		public abstract class tranNumber : PX.Data.BQL.BqlString.Field<tranNumber> { }
		public string TranNumber { get; set; }
		public abstract class authNumber : PX.Data.BQL.BqlString.Field<authNumber> { }
		public string AuthNumber { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		public decimal? Amount { get; set; }
		public abstract class processingStatus : PX.Data.BQL.BqlDecimal.Field<processingStatus> { }
		public string ProcessingStatus { get; set; }
		public abstract class lastActivityDate : PX.Data.BQL.BqlDateTime.Field<lastActivityDate> { }
		public DateTime? LastActivityDate { get; set; }
		public abstract class direction : PX.Data.BQL.BqlString.Field<direction> { }
		public string Direction { get; set; }
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		public bool? Active { get; set; }
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		public bool? Completed { get; set; }
		public abstract class parentTranID : PX.Data.BQL.BqlInt.Field<parentTranID> { }
		public int? ParentTranID { get; set; }
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
		public DateTime? ExpirationDate { get; set; }
		public abstract class cVVVerification : PX.Data.BQL.BqlString.Field<cVVVerification> { }
		public string CVVVerification { get; set; }
	}
}
