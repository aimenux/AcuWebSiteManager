using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class Payment: PXMappedCacheExtension
    {
        #region BranchID
        public abstract class branchID : PX.Data.IBqlField
        {
        }

        public virtual Int32? BranchID { get; set; }
        #endregion
        #region AdjDate
        public abstract class adjDate : PX.Data.IBqlField
        {
        }

        public virtual DateTime? AdjDate { get; set; }
        #endregion
        #region AdjFinPeriodID
        public abstract class adjFinPeriodID : PX.Data.IBqlField
        {
        }

        public virtual String AdjFinPeriodID { get; set; }
        #endregion
        #region AdjTranPeriodID
        public abstract class adjTranPeriodID : PX.Data.IBqlField
        {
        }

        public virtual String AdjTranPeriodID { get; set; }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.IBqlField
        {
        }

        public virtual String CuryID { get; set; }
        #endregion
    }
}
