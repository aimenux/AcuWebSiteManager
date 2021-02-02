using System;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.RequestsForInformation.PJ.DAC
{
    [Serializable]
    [PXCacheName("Request For Information Drawing Log")]
    public class RequestForInformationDrawingLog : DrawingLogReferenceBase, IBqlTable
    {
        [PXDBInt(IsKey = true)]
        public virtual int? RequestForInformationId
        {
            get;
            set;
        }

        public abstract class requestForInformationId : BqlInt.Field<requestForInformationId>
        {
        }

        public abstract class drawingLogId : BqlInt.Field<drawingLogId>
        {
        }
    }
}