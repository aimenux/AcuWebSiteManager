using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.EP;

namespace PX.Objects.AM.Attributes
{
    using PX.Objects.AM.GraphExtensions;

    // similar to PXEPEmployeeSelectorAttribute

    /// <summary>
    /// Manufacturing/Production employee Selector
    /// </summary>
    public class ProductionEmployeeSelectorAttribute : PXDimensionSelectorAttribute
    {
        /// <summary>
        /// Showing only production employees that are active
        /// </summary>
        public ProductionEmployeeSelectorAttribute()
            : base("BIZACCT", 
            typeof (Search2<EPEmployee.bAccountID,  
                    LeftJoin<EPEmployeePosition, 
                        On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>, 
                        And<EPEmployeePosition.isActive, Equal<True>>>>,
                    Where<EPEmployeeExt.amProductionEmployee, Equal<True>>>)
            , typeof (EPEmployee.acctCD)
            , typeof (EPEmployee.bAccountID), typeof (EPEmployee.acctCD), typeof (EPEmployee.acctName), typeof (EPEmployeePosition.positionID), 
                typeof (EPEmployee.departmentID), typeof (EPEmployee.defLocationID), /*typeof(EPEmployeeExt.amSiteID),*/ typeof(EPEmployee.calendarID))
            {
                this.DescriptionField = typeof (EPEmployee.acctName);
            }

        /// <summary>
        /// Allowing for any search criteria
        /// </summary>
        /// <param name="searchType"></param>
        public ProductionEmployeeSelectorAttribute(System.Type searchType)
            : base("BIZACCT",
            searchType
            , typeof(EPEmployee.acctCD)
            , typeof(EPEmployee.bAccountID), typeof(EPEmployee.acctCD), typeof(EPEmployee.acctName), typeof(EPEmployeePosition.positionID),
                typeof(EPEmployee.departmentID), typeof(EPEmployee.defLocationID), /*typeof(EPEmployeeExt.amSiteID),*/ typeof(EPEmployee.calendarID))
        {
            this.DescriptionField = typeof(EPEmployee.acctName);
        }
    }
}
