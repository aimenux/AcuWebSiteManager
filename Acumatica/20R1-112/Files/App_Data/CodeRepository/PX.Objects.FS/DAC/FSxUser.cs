using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.FS
{
    public class FSxUser : PXCacheExtension<Users>
    {
        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.IBqlField
        {
        }

        [PXBool]
        [PXUIField(Visible = false)]
        public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}