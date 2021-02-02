using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// BOM Attribute Maintenance graph
    /// Main graph for managing BOM Attributes
    /// </summary>
    public class BOMAttributeMaint : PXRevisionableGraph<BOMAttributeMaint, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID>
    {
        //Primary view "Documents" comes from PXRevisionablegraph

        //Redeclare buttons as we need to change the save button to save-cancel but keep it in the correct order in the UI...
        public new PXSaveCancel<AMBomItem> Save;
        public new PXRevisionableCancel<BOMAttributeMaint, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Cancel;
        public new PXRevisionableInsert<AMBomItem> Insert;
        public new PXDelete<AMBomItem> Delete;
        public new PXCopyPasteAction<AMBomItem> CopyPaste;
        public new PXRevisionableFirst<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> First;
        public new PXRevisionablePrevious<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Previous;
        public new PXRevisionableNext<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Next;
        public new PXRevisionableLast<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Last;

        [PXImport(typeof(AMBomItem))]
        public PXSelect<AMBomAttribute,
                    Where<AMBomAttribute.bOMID, Equal<Current<AMBomItem.bOMID>>,
                        And<AMBomAttribute.revisionID, Equal<Current<AMBomItem.revisionID>>>>> BomAttributes;

        public override bool CanClipboardCopyPaste()
        {
            return false;
        }

        public BOMAttributeMaint()
        {
            Documents.AllowDelete = false;
        }

        public override bool CanCreateNewRevision(BOMAttributeMaint fromGraph, BOMAttributeMaint toGraph, string keyValue, string revisionValue, out string error)
        {
            error = string.Empty;
            return false;
        }

        public override void CopyRevision(BOMAttributeMaint fromGraph, BOMAttributeMaint toGraph, string keyValue, string revisionValue)
        {
            throw new NotImplementedException("CopyRevision Process Not Required for BOM Attributes");
        }

        protected virtual void AMBomItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (AMBomItem)e.Row;
            if (row?.BOMID == null)
            {
                return;
            }
            
            PXUIFieldAttribute.SetEnabled(sender, row, false);
            PXUIFieldAttribute.SetEnabled<AMBomItem.bOMID>(sender, row, true);
            PXUIFieldAttribute.SetEnabled<AMBomItem.revisionID>(sender, row, true);

            BomAttributes.AllowDelete =
                BomAttributes.AllowUpdate =
                    BomAttributes.AllowInsert = row.Hold.GetValueOrDefault();
        }

        protected virtual void AMBomAttribute_AttributeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (AMBomAttribute) e.Row;
            if (row == null)
            {
                return;
            }

            var item = (CSAttribute)PXSelectorAttribute.Select<AMBomAttribute.attributeID>(sender, row);
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                sender.SetValueExt<AMBomAttribute.label>(row, item.AttributeID);
            }
            if (string.IsNullOrWhiteSpace(row.Descr))
            {
                sender.SetValueExt<AMBomAttribute.descr>(row, item.Description);
            }
        }
               
    }
}
