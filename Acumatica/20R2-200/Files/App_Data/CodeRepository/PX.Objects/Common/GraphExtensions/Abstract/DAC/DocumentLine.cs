using System;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class DocumentLine : PXMappedCacheExtension
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
    }
}
