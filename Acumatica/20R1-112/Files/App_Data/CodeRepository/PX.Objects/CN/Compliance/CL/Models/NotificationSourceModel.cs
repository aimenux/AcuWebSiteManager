using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.CL.Models
{
    public class NotificationSourceModel
    {
        public NotificationSource NotificationSource
        {
            get;
            set;
        }

        public int? VendorId
        {
            get;
            set;
        }

        public bool IsJointCheck
        {
            get;
            set;
        }
    }
}