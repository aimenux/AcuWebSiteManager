using PX.Data;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    public class DrawingLogReferenceBase
    {
        [PXDBInt(IsKey = true)]
        public virtual int? DrawingLogId
        {
            get;
            set;
        }
    }
}