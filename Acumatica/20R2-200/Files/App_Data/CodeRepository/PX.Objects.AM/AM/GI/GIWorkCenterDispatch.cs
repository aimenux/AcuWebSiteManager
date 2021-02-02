using System;

namespace PX.Objects.AM
{
    public class GIWorkCenterDispatch : AMGenericInquiry
    {
        protected const string GIID = "47842ccc-aa5d-4840-9d4a-7642cbf34cbe";

        public GIWorkCenterDispatch() 
            : base(new Guid(GIID))
        {
        }

        /// <summary>
        /// Available parameters for use with this generic inquiry
        /// </summary>
        public static class Parameters
        {
            public const string WorkCenter = "WCID";
        }
    }
}