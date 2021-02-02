using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class Adjust2: PXMappedCacheExtension
    {
        #region AdjgBranchID
        public abstract class adjgBranchID : PX.Data.IBqlField { }

        public virtual int? AdjgBranchID { get; set; }
        #endregion
        #region AdjgFinPeriodID
        public abstract class adjgFinPeriodID : PX.Data.IBqlField{}

        public virtual String AdjgFinPeriodID { get; set; }
        #endregion
        #region AdjgTranPeriodID
        public abstract class adjgTranPeriodID : PX.Data.IBqlField { }

        public virtual String AdjgTranPeriodID { get; set; }
        #endregion
        #region AdjdBranchID
        public abstract class adjdBranchID : PX.Data.IBqlField { }

        public virtual int? AdjdBranchID { get; set; }
        #endregion
        #region AdjdFinPeriodID
        public abstract class adjdFinPeriodID : PX.Data.IBqlField { }

        public virtual String AdjdFinPeriodID { get; set; }
        #endregion
        #region AdjdTranPeriodID
        public abstract class adjdTranPeriodID : PX.Data.IBqlField { }

        public virtual String AdjdTranPeriodID { get; set; }
        #endregion
    }
}
