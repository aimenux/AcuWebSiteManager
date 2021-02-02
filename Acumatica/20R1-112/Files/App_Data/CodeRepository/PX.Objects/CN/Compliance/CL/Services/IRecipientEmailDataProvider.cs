using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CL.Services
{
    public interface IRecipientEmailDataProvider
    {
        string GetRecipientEmail(NotificationRecipient notificationRecipient, int? vendorId);
    }
}