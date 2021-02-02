using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.TM;

namespace PX.Objects.EP.DAC
{
    [PXCacheName(Messages.ExpenseClaim)]
    [PXProjection(typeof(
        Select2<EPExpenseClaim, 
                InnerJoin<EPEmployee, 
                        On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>, 
                Where<EPExpenseClaim.createdByID, Equal<CurrentValue<AccessInfo.userID>>, 
                        Or<EPEmployee.defContactID, Equal<CurrentValue<AccessInfo.contactID>>, 
                        Or<EPEmployee.defContactID, IsSubordinateOfContact<CurrentValue<AccessInfo.contactID>>, 
                        Or<EPExpenseClaim.noteID, Approver<CurrentValue<AccessInfo.contactID>>, 
                        Or<EPExpenseClaim.employeeID, WingmanUser<CurrentValue<AccessInfo.userID>>>>>>>>))]
    public class EPExpenseClaimForCurrentUser: EPExpenseClaim
    {
    }
}
