using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Estimate PX Selector Attribute for all Estimates/Revisions
    /// </summary>
    public class EstimateIDSelectPrimaryAttribute : PXSelectorAttribute
    {
        public EstimateIDSelectPrimaryAttribute()
            : base(typeof(Search<AMEstimateItem.estimateID, Where<AMEstimateItem.revisionID, Equal<AMEstimateItem.primaryRevisionID>>>), ColumnList)
        { }

        public EstimateIDSelectPrimaryAttribute(params Type[] colList)
            : base(typeof(Search<AMEstimateItem.estimateID, Where<AMEstimateItem.revisionID, Equal<AMEstimateItem.primaryRevisionID>>>), colList) { }

        public EstimateIDSelectPrimaryAttribute(Type searchType)
            : base(searchType, ColumnList)
        { }

        /// <summary>
        /// Column list for EstimateID selector
        /// </summary>
        public static Type[] ColumnList => new [] {typeof(AMEstimateItem.estimateID),
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.inventoryCD),
            typeof(AMEstimateItem.itemDesc),
            typeof(AMEstimateItem.estimateClassID),
            typeof(AMEstimateItem.estimateStatus),
            typeof(AMEstimateItem.revisionDate)
        };
    }
}