using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Extensions
{
    public static class RequestForInformationExtensions
    {
        public static bool IsNew(this RequestForInformation requestsForInformation)
        {
            return requestsForInformation.Status == RequestForInformationStatusAttribute.NewStatus;
        }

        public static bool IsOpen(this RequestForInformation requestsForInformation)
        {
            return requestsForInformation.Status == RequestForInformationStatusAttribute.OpenStatus;
        }

        public static bool IsClosed(this RequestForInformation requestsForInformation)
        {
            return requestsForInformation.Status == RequestForInformationStatusAttribute.ClosedStatus;
        }

        public static bool IsNotClosed(this RequestForInformation requestsForInformation)
        {
            return requestsForInformation.IsOpen() || requestsForInformation.IsNew();
        }

        public static bool IsDefinitelyClosed(this RequestForInformation requestsForInformation)
        {
            return requestsForInformation.MajorStatus == RequestForInformationStatusAttribute.ClosedStatus;
        }
    }
}