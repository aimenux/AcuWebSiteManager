using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.IN
{
    public class INSubItemMaint : PXGraph<INSubItemMaint>
    {
        public PXCancel<INSubItem> Cancel;
        public PXSavePerRow<INSubItem, INSubItem.subItemID> Save;

        [PXImport(typeof(INSubItem))]
        [PXFilterable]
        public PXSelectOrderBy<INSubItem,OrderBy<Asc<INSubItem.subItemCD>>> SubItemRecords;

        [IN.SubItemRaw(IsKey = true, DisplayName = "Subitem")]
        [PXDefault()]
        protected virtual void INSubItem_SubItemCD_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        protected virtual void INSubItem_Descr_CacheAttached(PXCache sender)
        {
        }

        protected virtual void INSubItem_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            INSubItem row = e.Row as INSubItem; 
            if (row != null)
            {

                INSiteStatus status = PXSelect<INSiteStatus, Where<INSiteStatus.subItemID, Equal<Required<INSubItem.subItemID>>>>.SelectWindowed(this, 0, 1, row.SubItemID);
                if( status != null)
                {
                    throw new PXSetPropertyException(Messages.SubitemDeleteError);
                }

                INItemXRef itemRef = PXSelect<INItemXRef, Where<INItemXRef.subItemID, Equal<Required<INSubItem.subItemID>>>>.SelectWindowed(this, 0, 1, row.SubItemID);    
                if(itemRef != null)
                {
                    throw new PXSetPropertyException(Messages.SubitemDeleteError);
                }
            }
        }

    }
}