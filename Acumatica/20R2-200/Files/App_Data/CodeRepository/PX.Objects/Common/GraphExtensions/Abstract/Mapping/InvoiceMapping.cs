using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class InvoiceMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(Invoice);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        /// <summary>Creates the default mapping of the <see cref="DocumentWithLines" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public InvoiceMapping(Type table)
        {
            _table = table;
        }

        /// <exclude />
        public Type BranchID = typeof(Invoice.branchID);

        /// <exclude />
        public Type HeaderDocDate = typeof(Invoice.headerDocDate);

        /// <exclude />
        public Type HeaderTranPeriodID = typeof(Invoice.headerTranPeriodID);

        public Type DocType = typeof(Invoice.docType);

        public Type RefNbr = typeof(Invoice.refNbr);

        public Type CuryInfoID = typeof(Invoice.curyInfoID);

        public Type Hold = typeof(Invoice.hold);

        public Type Released = typeof(Invoice.released);

        public Type Printed = typeof(Invoice.printed);

        public Type OpenDoc = typeof(Invoice.openDoc);

        public Type FinPeriodID = typeof(Invoice.finPeriodID);

        public Type InvoiceNbr = typeof(Invoice.invoiceNbr);

        public Type DocDesc = typeof(Invoice.docDesc);

        public Type ContragentID = typeof(Invoice.contragentID);

        public Type ContragentLocationID = typeof(Invoice.contragentLocationID);

        public Type TaxZoneID = typeof(Invoice.taxZoneID);

        public Type TaxCalcMode = typeof(Invoice.taxCalcMode);

        public Type OrigModule = typeof(Invoice.origModule);

	    public Type OrigDocType = typeof(Invoice.origDocType);

		public Type OrigRefNbr = typeof(Invoice.origRefNbr);

        public Type CuryOrigDocAmt = typeof(Invoice.curyOrigDocAmt);

        public Type CuryTaxAmt = typeof(Invoice.curyTaxAmt);

        public Type CuryDocBal = typeof(Invoice.curyDocBal);

        public Type CuryTaxTotal = typeof(Invoice.curyTaxTotal);

        public Type CuryTaxRoundDiff = typeof(Invoice.curyTaxRoundDiff);

        public Type CuryRoundDiff = typeof(Invoice.curyRoundDiff);

        public Type TaxRoundDiff = typeof(Invoice.taxRoundDiff);

        public Type RoundDiff = typeof(Invoice.roundDiff);

        public Type TaxAmt = typeof(Invoice.taxAmt);

        public Type DocBal = typeof(Invoice.docBal);

        public Type Approved = typeof(Invoice.approved);
    }
}
