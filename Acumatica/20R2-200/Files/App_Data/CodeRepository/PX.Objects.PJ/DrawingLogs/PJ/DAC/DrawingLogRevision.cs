using System;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [Serializable]
    [PXCacheName("Drawing Log Revision")]
    public class DrawingLogRevision : IBqlTable
    {
        [PXDBInt(IsKey = true)]
        public virtual int? DrawingLogId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        public virtual int? DrawingLogRevisionId
        {
            get;
            set;
        }

        public abstract class drawingLogId : IBqlField
        {
        }

        public abstract class drawingLogRevisionId : IBqlField
        {
        }
    }
}