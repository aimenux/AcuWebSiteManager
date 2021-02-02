using System;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    /// <summary>
    /// This attribute is not used and will be removed in 2021R1.
    /// </summary>
    [Obsolete]
    public class ComplianceDocumentDisplayNameAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber
    {
        private readonly Type itemType;
        private EntityHelper entityHelper;
        private bool rowSelectingLock;

        public ComplianceDocumentDisplayNameAttribute(Type itemType)
        {
            this.itemType = itemType;
        }

        public override void CacheAttached(PXCache sender)
        {
            entityHelper = new EntityHelper(sender.Graph);
        }

        public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            if (e.Row != null && !rowSelectingLock)
            {
                var displayName = GetDisplayName(sender, e.Row);
                sender.SetValue(e.Row, _FieldName, displayName);
            }
        }

        private string GetDisplayName(PXCache cache, object row)
        {
            try
            {
                rowSelectingLock = true;
                using (new PXConnectionScope())
                {
                    var noteId = entityHelper.GetEntityNoteID(row);
                    if (itemType != typeof(PMRegister))
                    {
                        return noteId.HasValue && noteId.Value != Guid.Empty
                            ? EntityHelper.GetEntityDescription(cache.Graph, row)
                            : null;
                    }
                    return noteId.HasValue && noteId.Value != Guid.Empty
                        ? string.Join(", ",
                            entityHelper.GetEntityRowKeys(itemType, entityHelper.GetEntityRow(itemType, noteId)))
                        : null;
                }
            }
            finally
            {
                rowSelectingLock = false;
            }
        }
    }
}