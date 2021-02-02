using System;
using System.Linq;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;

namespace PX.Objects.PJ.ProjectManagement.PM.Services
{
    public class RequestForInformationConversionService : ConversionServiceBase
    {
        public RequestForInformationConversionService(ChangeRequestEntry graph)
            : base(graph)
        {
        }

        public override void UpdateConvertedEntity(PMChangeRequest changeRequest)
        {
            var requestForInformation = GetRequestForInformation(changeRequest);
            if (requestForInformation != null)
            {
                UpdateRequestForInformation(requestForInformation, RequestForInformationStatusAttribute.OpenStatus,
                    RequestForInformationReasonAttribute.FollowUpNeeded, null);
            }
        }

        public override void SetFieldReadonly(PMChangeRequest changeRequest)
        {
            SetFieldReadOnly<PmChangeRequestExtension.rfiID>(changeRequest);
        }

        protected override void ProcessConvertedChangeRequest(PMChangeRequest changeRequest)
        {
            var requestForInformation = GetRequestForInformation(changeRequest);
            UpdateRequestForInformation(requestForInformation, RequestForInformationStatusAttribute.ClosedStatus,
                RequestForInformationReasonAttribute.ConvertedToChangeRequest, changeRequest.NoteID);
            CopyFilesToChangeRequest<RequestForInformation>(requestForInformation, changeRequest);
        }

        private void UpdateRequestForInformation(RequestForInformation requestForInformation, string status,
            string reason, Guid? noteId)
        {
            requestForInformation.Status = status;
            requestForInformation.MajorStatus = status;
            requestForInformation.Reason = reason;
            requestForInformation.ConvertedTo = noteId;
            Graph.Caches<RequestForInformation>().PersistUpdated(requestForInformation);
        }

        private RequestForInformation GetRequestForInformation(PMChangeRequest changeRequest)
        {
            var changeRequestExt = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            return Graph.Select<RequestForInformation>()
                .SingleOrDefault(rfi => rfi.RequestForInformationId == changeRequestExt.RFIID);
        }
    }
}
