namespace PX.Objects.CN.JointChecks.AP.Models
{
    public class LienWaiverGenerationKey
    {
        public int? ProjectId
        {
            get;
            set;
        }

        public int? VendorId
        {
            get;
            set;
        }

        public int? JointPayeeVendorId
        {
            get;
            set;
        }

        public string OrderNumber
        {
            get;
            set;
        }
    }
}