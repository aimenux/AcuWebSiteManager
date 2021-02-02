using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
    public class InvoiceBase: Document
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
    }
}
