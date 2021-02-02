using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class Document: PXMappedCacheExtension
    {
        #region BranchID
        /// <exclude />
        public abstract class branchID : IBqlField
        {
        }

        public virtual int? BranchID { get; set; }
        #endregion
        #region HeaderDocDate
        /// <exclude />
        public abstract class headerDocDate : IBqlField
        {
        }

        public virtual DateTime? HeaderDocDate { get; set; }
        #endregion
        #region HeaderTranPeriodID
        /// <exclude />
        public abstract class headerTranPeriodID : IBqlField
        {
        }

        public virtual string HeaderTranPeriodID { get; set; }
        #endregion
    }
}
