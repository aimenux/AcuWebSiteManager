using System;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class EstimateRevisionIDSelectorAttribute : PXSelectorAttribute
    {
        public EstimateRevisionIDSelectorAttribute()
            : base(typeof(Search3<AMEstimateItem.revisionID, OrderBy<Asc<AMEstimateItem.createdDateTime>>>), ColumnList)
        { }

        public EstimateRevisionIDSelectorAttribute(Type searchType)
            : base(searchType, ColumnList)
        { }

        protected static Type[] ColumnList
        {
            get
            {
                return new [] {typeof(AMEstimateItem.revisionID)
                    , typeof(AMEstimateItem.revisionDate)
                    , typeof(AMEstimateItem.isPrimary)
                    };
            }
        }
    }
}