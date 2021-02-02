using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public class ChangeRequestSearchableAttribute : PXSearchableAttribute
    {
        public ChangeRequestSearchableAttribute(): base(SM.SearchCategory.PM, Messages.ChangeRequestSearchTitle, new Type[] { typeof(PMChangeRequest.refNbr) },
            new Type[] { typeof(PMChangeRequest.description), typeof(PMChangeRequest.extRefNbr), typeof(PMChangeRequest.extRefNbr), typeof(PMChangeRequest.projectID), typeof(PMProject.contractCD), typeof(PMProject.description) })
            
       {
            NumberFields = new Type[] { typeof(PMChangeRequest.refNbr) };
            Line1Format = "{0:d}{1}{2}";
            Line1Fields = new Type[] { typeof(PMChangeRequest.date), typeof(PMChangeRequest.status), typeof(PMChangeRequest.projectID) };
            Line2Format = "{0}";
            Line2Fields = new Type[] { typeof(PMChangeRequest.description) };
            SelectForFastIndexing = typeof(Select2<PMChangeRequest, InnerJoin<PMProject, On<PMChangeRequest.projectID, Equal<PMProject.contractID>>>>);
        }
    }
}
