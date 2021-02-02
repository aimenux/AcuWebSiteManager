using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public class ChangeOrderSearchableAttribute : PXSearchableAttribute
    {
        public ChangeOrderSearchableAttribute(): base(SM.SearchCategory.PM, Messages.ChangeOrderSearchTitle, new Type[] { typeof(PMChangeOrder.refNbr) },
            new Type[] { typeof(PMChangeOrder.description), typeof(PMChangeOrder.extRefNbr), typeof(PMChangeOrder.projectID), typeof(PMProject.contractCD), typeof(PMProject.description) })
            
       {
            NumberFields = new Type[] { typeof(PMChangeOrder.refNbr) };
            Line1Format = "{0:d}{1}{2}";
            Line1Fields = new Type[] { typeof(PMChangeOrder.date), typeof(PMChangeOrder.status), typeof(PMChangeOrder.projectID) };
            Line2Format = "{0}";
            Line2Fields = new Type[] { typeof(PMChangeOrder.description) };
            SelectForFastIndexing = typeof(Select2<PMChangeOrder, InnerJoin<PMProject, On<PMChangeOrder.projectID, Equal<PMProject.contractID>>>>);
        }
    }
}
