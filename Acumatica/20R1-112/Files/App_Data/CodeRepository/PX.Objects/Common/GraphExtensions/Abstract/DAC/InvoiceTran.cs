using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
	[PXHidden]
    public class InvoiceTran: PXMappedCacheExtension
    {
        #region BranchID

        /// <exclude />
        public abstract class branchID : IBqlField
        {
        }

        /// <summary>The identifier of the branch associated with the document.</summary>
        public virtual Int32? BranchID { get; set; }

        #endregion

        #region TranDate

        public abstract class tranDate : PX.Data.IBqlField
        {
        }

        public virtual DateTime? TranDate { get; set; }

        #endregion

        #region FinPeriodID

        /// <exclude />
        public abstract class finPeriodID : IBqlField
        {
        }

        public virtual string FinPeriodID { get; set; }

        #endregion

        #region TranPeriodID

        /// <exclude />
        public abstract class tranPeriodID : IBqlField
        {
        }

        public virtual string TranPeriodID { get; set; }

        #endregion

        public virtual Int32? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        public virtual String TaxCategoryID { get; set; }
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        public virtual Int32? InventoryID { get; set; }
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        public virtual String TranDesc { get; set; }
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc>{}

        public virtual Boolean? ManualPrice { get; set; }
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }

        public virtual Decimal? CuryUnitCost { get; set; }
        public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost>{}

        public virtual Decimal? Qty { get; set; }
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        public virtual String UOM { get; set; }
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>{}

        public virtual Boolean? NonBillable { get; set; }
        public abstract class nonBillable : PX.Data.BQL.BqlBool.Field<nonBillable> { }

        public virtual DateTime? Date { get; set; }
        public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

        public virtual Int32? ProjectID { get; set; }
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>{}

        public virtual Int32? TaskID { get; set; }
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID>{}

        public virtual Int32? CostCodeID { get; set; }
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        public virtual Int32? AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID>{}

        public virtual int? SubID { get; set; }
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        public virtual Decimal? CuryLineAmt { get; set; }
        public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }

        public virtual decimal? CuryTaxAmt { get; set; }
        public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

        public virtual decimal? CuryTaxableAmt { get; set; }
        public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }

        public virtual Decimal? CuryTranAmt { get; set; }
        public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        public virtual decimal? TaxableAmt { get; set; }
        public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }

        public virtual Decimal? TranAmt { get; set; }
        public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
    }
}
