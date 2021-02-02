using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AP.Standalone;
using APPayment = PX.Objects.AP.Standalone.APPayment;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class InvoiceBase : Document
    {
        #region CuryID
        public abstract class curyID : IBqlField
        {
        }

        public virtual String CuryID { get; set; }
        #endregion
        #region ModuleAccountID
        public abstract class moduleAccountID : IBqlField
        {
        }

        public virtual Int32? ModuleAccountID { get; set; }
        #endregion
        #region ModuleSubID
        public abstract class moduleSubID : PX.Data.IBqlField
        {
        }

        public virtual Int32? ModuleSubID { get; set; }
        #endregion

        public abstract class docType : IBqlField
        {
        }
        public string DocType { get; set; }

        public abstract class refNbr : IBqlField
        {
        }
        public string RefNbr { get; set; }

        public abstract class curyInfoID : IBqlField
        {
        }
        public long? CuryInfoID { get; set; }

        public abstract class hold : IBqlField
        {
        }
        public bool? Hold { get; set; }

        public abstract class released : IBqlField
        {
        }
        public bool? Released { get; set; }

        public abstract class printed : IBqlField
        {
        }
        public bool? Printed { get; set; }

        public abstract class openDoc : IBqlField
        {
        }
        public bool? OpenDoc { get; set; }

        public abstract class finPeriodID : IBqlField
        {
        }
        public string FinPeriodID { get; set; }

        public abstract class invoiceNbr : IBqlField
        {
        }
        public string InvoiceNbr { get; set; }

        public abstract class docDesc : IBqlField
        {
        }
        public string DocDesc { get; set; }

        public abstract class contragentID : IBqlField
        {
        }
        public int? ContragentID { get; set; }

        public abstract class contragentLocationID : IBqlField
        {
        }
        public int? ContragentLocationID { get; set; }

        public abstract class taxZoneID : IBqlField
        {
        }
        public string TaxZoneID { get; set; }

        public abstract class taxCalcMode : IBqlField
        {
        }
        public string TaxCalcMode { get; set; }

        public abstract class origModule : IBqlField
        {
        }
        public string OrigModule { get; set; }

	    public abstract class origDocType : IBqlField
	    {
	    }
	    public string OrigDocType { get; set; }

		public abstract class origRefNbr : IBqlField
        {
        }
        public string OrigRefNbr { get; set; }

        public abstract class curyOrigDocAmt : IBqlField
        {
        }
        public decimal? CuryOrigDocAmt { get; set; }

        public abstract class curyTaxAmt : IBqlField
        {
        }
        public decimal? CuryTaxAmt { get; set; }

        public abstract class curyDocBal : IBqlField
        {
        }
        public decimal? CuryDocBal { get; set; }

        public abstract class curyTaxTotal : IBqlField
        {
        }
        public decimal? CuryTaxTotal { get; set; }

        public abstract class curyTaxRoundDiff : IBqlField
        {
        }
        public decimal? CuryTaxRoundDiff { get; set; }

        public abstract class curyRoundDiff : IBqlField
        {
        }
        public decimal? CuryRoundDiff { get; set; }

        public abstract class taxRoundDiff : IBqlField
        {
        }
        public decimal? TaxRoundDiff { get; set; }

        public abstract class roundDiff : IBqlField
        {
        }
        public decimal? RoundDiff { get; set; }

        public abstract class taxAmt : IBqlField
        {
        }
        public decimal? TaxAmt { get; set; }

        public abstract class docBal : IBqlField
        {
        }
        public decimal? DocBal { get; set; }

        public abstract class approved : IBqlField
        {
        }
        public bool? Approved { get; set; }

        #region Extending to Paid Invoice

        public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
        public int? CashAccountID { get; set; }

        public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
        public String PaymentMethodID { get; set; }

	    public abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }
	    public virtual long? CATranID { get; set; }

	    public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
	    public virtual DateTime? ClearDate { get; set; }

	    public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
	    public virtual bool? Cleared { get; set; }

	    public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
	    public virtual string ExtRefNbr { get; set; }

		#endregion
	}
}
