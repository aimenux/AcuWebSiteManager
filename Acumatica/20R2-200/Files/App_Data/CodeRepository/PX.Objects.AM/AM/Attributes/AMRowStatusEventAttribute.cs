using PX.Data;
using System;

namespace PX.Objects.AM.Attributes
{
    public class AMRowStatusEventAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletingSubscriber
    {
        protected Type MasterParentType;
        protected PXGraph Graph;

        public AMRowStatusEventAttribute(Type masterParentType)
        {
            MasterParentType = masterParentType;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Graph = sender.Graph;
        }

        protected bool IsCurrentMasterParentDeleted => Graph?.Caches[MasterParentType]?.IsCurrentRowDeleted() == true;

        protected int? GetRowStatusValue(PXCache cache, object row)
        {
            return row == null ? null : (int?)cache.GetValue(row, FieldName);
        }

        protected void SetRowStatusValue(PXCache cache, object row, int? value)
        {
            if (row == null)
            {
                return;
            }

            cache.SetValue(row, FieldName, value);
        }

        public void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            var rowStatus = GetRowStatusValue(cache, e.Row);
            if (rowStatus == null || rowStatus != AMRowStatus.Unchanged || cache.IsRowInserted(e.Row))
            {
                return;
            }

            SetRowStatusValue(cache, e.Row, AMRowStatus.Updated);
        }

        public void RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (IsCurrentMasterParentDeleted || cache.GetStatus(e.Row) == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            var rowStatus = GetRowStatusValue(cache, e.Row);
            if (rowStatus == null || rowStatus == AMRowStatus.Inserted)
            {
                return;
            }

            e.Cancel = true;
            SetRowStatusValue(cache, e.Row, AMRowStatus.Deleted);
            cache.SetStatus(e.Row, PXEntryStatus.Updated);
            // Setting the cache as updated was not enough to enable the save button
            cache.IsDirty = true;
        }

        public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = AMRowStatus.Inserted;
        }
    }
}