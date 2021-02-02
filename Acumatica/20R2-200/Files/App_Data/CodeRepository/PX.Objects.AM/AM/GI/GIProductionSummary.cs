using System;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Production summary generic inquiry helper class
    /// </summary>
    public class GIProductionSummary : AMGenericInquiry
    {
        /// <summary>
        /// ID of Production Summary GI in the MFG project.xml
        /// </summary>
        protected const string GIID = "76952228-f314-4b07-a73c-4da7c8f7ba44";

        /// <summary>
        /// Available parameters for use with this generic inquiry
        /// </summary>
        public static class Parameters
        {
            public const string Status = "Status";
            public const string OrderType = "ORDERTYPE";
        }

        public GIProductionSummary()
            : base(new Guid(GIID))
        {
        }

        /// <summary>
        /// Set the GI call to filter for a specific project
        /// </summary>
        /// <param name="projectId"></param>
        public virtual void SetProjectFilter(int? projectId)
        {
            AddFilter(typeof(AMProdItem), typeof(AMProdItem.projectID), PXCondition.EQ, projectId);
        }

        /// <summary>
        /// Set no status for the GI parameter (show all status)
        /// </summary>
        public virtual void SetNoStatus()
        {
            SetParameterNull(Parameters.Status);
        }
    }
}