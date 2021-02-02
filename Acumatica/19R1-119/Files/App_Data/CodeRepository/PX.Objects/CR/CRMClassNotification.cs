using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR
{
    public class CRMContactType : NotificationContactType
    {
        public class ClassListAttribute : PXStringListAttribute
        {
            public ClassListAttribute()
                : base(new string[] { Primary, Employee, Contact, Shipping },
                             new string[] { CR.Messages.Primary, EP.Messages.Employee, CR.Messages.Contact, Messages.Shipping })
            {
            }
        }
        public new class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[] { Primary, Employee, Contact, Shipping },
                             new string[] { CR.Messages.Primary, EP.Messages.Employee, CR.Messages.Contact, Messages.Shipping })
            {
            }
        }
    }
}
