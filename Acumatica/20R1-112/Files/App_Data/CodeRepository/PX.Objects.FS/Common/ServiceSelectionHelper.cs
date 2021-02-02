using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class ServiceSelectionHelper
    {
        #region Selects

        public class ServiceRecords_View : PXSelectJoin<InventoryItem,
                                           LeftJoin<INItemClass,
                                                On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>,
                                           Where2<
                                                Where<
                                                    InventoryItem.itemType, Equal<INItemTypes.serviceItem>,
                                                    And<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
                                                    And<FSxServiceClass.requireRoute, Equal<Current<FSSrvOrdType.requireRoute>>,
                                                    And<Match<Current<AccessInfo.userName>>>>>>,
                                                And<
                                                    Where<
                                                        Current<ServiceSelectionFilter.serviceClassID>, IsNull,
                                                        Or<INItemClass.itemClassID, Equal<Current<ServiceSelectionFilter.serviceClassID>>>>>>>
        {
            public ServiceRecords_View(PXGraph graph) : base(graph)
            {
            }

            public ServiceRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }
        
        #endregion

        #region Static Methods

        /// <summary>
        /// Initialize the EmployeeGrid filter with the existing employees in the Employee tab.
        /// </summary>
        /// <param name="employeesView">Employee view from Appointment or ServiceOrder screen.</param>
        private static IEnumerable<EmployeeGridFilter> PopulateEmployeeFilter(object employeesView)
        {
            HashSet<EmployeeGridFilter> employees = new HashSet<EmployeeGridFilter>();

            if (employeesView is AppointmentCore.AppointmentServiceEmployees_View)
            {
                AppointmentCore.AppointmentServiceEmployees_View appEmployeeView = (AppointmentCore.AppointmentServiceEmployees_View)employeesView;

                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appEmployeeView.Select())
                {
                    EmployeeGridFilter employee = new EmployeeGridFilter();
                    employee.EmployeeID = fsAppointmentEmployeeRow.EmployeeID;
                    employees.Add(employee);
                }
            }

            if (employeesView is ServiceOrderCore.ServiceOrderEmployees_View)
            {
                ServiceOrderCore.ServiceOrderEmployees_View soEmployeeView = (ServiceOrderCore.ServiceOrderEmployees_View)employeesView;

                foreach (FSSOEmployee fsSOEmployeeRow in soEmployeeView.Select())
                {
                    EmployeeGridFilter employee = new EmployeeGridFilter();
                    employee.EmployeeID = fsSOEmployeeRow.EmployeeID;
                    employees.Add(employee);
                }
            }

            return employees;
        }

        /// <summary>
        /// Fills the serviceList list with those services that already exist in the Details tab.
        /// </summary>
        /// <param name="servicesView">Service view of the detail tab of the screen that calls the selector.</param>
        public static List<int?> GetServicesInServiceTab<ServiceDetType>(PXSelectBase<ServiceDetType> servicesView, string serviceLineRefNbr)
            where ServiceDetType : class, IBqlTable, IFSSODetBase, new()
        {
            List<int?> serviceIDList = new List<int?>();

            foreach (ServiceDetType row in servicesView.Select().RowCast<ServiceDetType>().Where(x => x.IsService == true))
            {
                if (string.IsNullOrEmpty(serviceLineRefNbr) || row.LineRef == serviceLineRefNbr)
                {
                    serviceIDList.Add(row.InventoryID);
                }
            }

            return serviceIDList;
        }

        /// <summary>
        /// Return a list of services without their skills.
        /// </summary>
        /// <param name="cmd">Base select over the Inventory Item table.</param>
        /// <param name="serviceList">Services that will be excluded from the returned list.</param>
        private static IEnumerable<InventoryItem> GetListWithServicesOnly(PXSelectBase<InventoryItem> cmd, List<int?> serviceList)
        {
            foreach (InventoryItem inventoryRow in cmd.Select())
            {
                if (serviceList.Exists(delegate(int? serviceID) { return serviceID == inventoryRow.InventoryID; }) == true)
                {
                    continue;
                }

                yield return inventoryRow;
            }
        }

        /// <summary>
        /// Check if the given service skills are contained by each given employee. 
        /// </summary>
        /// <param name="serviceSkills">Service with its skills.</param>
        /// <param name="employeeSkillList">List of employees and their skills.</param>
        private static bool CanThisServiceBeCompleteByTheseEmployeesSkills(SharedClasses.ItemList serviceSkills,
                                                                           List<SharedClasses.ItemList> employeeSkillList)
        {
            if (employeeSkillList.Count > 0)
            {
                foreach (SharedClasses.ItemList employeeSkills in employeeSkillList.Where(x => x.list.Count > 0))
                {
                    if (serviceSkills.list.Except(employeeSkills.list).Any())
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the given service License are contained by each given employee. 
        /// </summary>
        /// <param name="serviceLicenses">Service with its licenses.</param>
        /// <param name="employeeLicenseList">List of employees and their licenses.</param>
        private static bool CanThisServiceBeCompleteByTheseEmployeesLicenses(SharedClasses.ItemList serviceLicenses,
                                                                             List<SharedClasses.ItemList> employeeLicenseList)
        {
            if (employeeLicenseList.Count > 0)
            {
                foreach (SharedClasses.ItemList employeeLicenses in employeeLicenseList.Where(x => x.list.Count > 0))
                {
                    if (serviceLicenses.list.Except(employeeLicenses.list).Any())
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static void InsertCurrentService<ServiceDetType>(PXSelectBase<InventoryItem> inventoryView,
                                                                PXSelectBase<ServiceDetType> serviceDetView)
            where ServiceDetType : class, IBqlTable, IFSSODetBase, new()
        {
            if (inventoryView.Current == null)
            {
                return;
            }

            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(serviceDetView.Cache.Graph, inventoryView.Current.InventoryID);

            var newRow = new ServiceDetType();

            newRow.LineType = ID.LineType_ALL.SERVICE;
            newRow.InventoryID = inventoryItemRow.InventoryID;

            newRow = serviceDetView.Insert(newRow);
        }

        public static void OpenServiceSelector<DocDate>(PXCache documentCache,
                                                        PXFilter<ServiceSelectionFilter> serviceSelectorFilter,
                                                        PXSelect<EmployeeGridFilter> employeeGridFilter)
            where DocDate : class, IBqlField
        {
            if (documentCache.Current != null)
            {
                serviceSelectorFilter.Current.ScheduledDateTimeBegin = (DateTime?)documentCache.GetValue<DocDate>(documentCache.Current);
            }

            employeeGridFilter.Cache.Clear();
            serviceSelectorFilter.AskExt();
        }
        #endregion

        #region Delegates

        public static IEnumerable EmployeeRecordsDelegate(object employeeView, PXSelectBase<EmployeeGridFilter> employeeFilterView)
        {
            foreach (EmployeeGridFilter employeeRow in PopulateEmployeeFilter(employeeView))
            {
                EmployeeGridFilter employeeInView = employeeFilterView.Locate(employeeRow);

                if (employeeInView == null)
                {
                    employeeRow.Mem_Selected = true;
                    employeeInView = employeeFilterView.Insert(employeeRow);
                }

                employeeFilterView.Cache.SetStatus(employeeInView, PXEntryStatus.Held);
                employeeFilterView.Cache.IsDirty = false;

                yield return employeeInView;
            }
        }
      
        public static IEnumerable ServiceRecordsDelegate<ServiceDetType>(PXSelectBase<ServiceDetType> servicesView,
                                                                         PXSelectBase<EmployeeGridFilter> employeesView,
                                                                         PXFilter<ServiceSelectionFilter> filter)
            where ServiceDetType : class, IBqlTable, IFSSODetBase, new()
        {
            if (filter.Current == null)
            {
                yield break;
            }

            PXGraph graph = filter.Cache.Graph;
            PXSelectBase<InventoryItem> cmd = new ServiceRecords_View(graph);

            List<int?> serviceList = GetServicesInServiceTab<ServiceDetType>(servicesView, null);
            List<int?> employeeSelection = employeesView.Select()
														.Where(y => y.GetItem<EmployeeGridFilter>().Mem_Selected == true)
                                                        .Select(y => y.GetItem<EmployeeGridFilter>().EmployeeID)
                                                        .ToList();

            IEnumerable<InventoryItem> inventoryRecords = GetListWithServicesOnly(cmd, serviceList);

            if (employeeSelection.Count == 0)
            {
                foreach (InventoryItem inventoryRow in inventoryRecords)
                {
                    yield return inventoryRow;
                }
            }
            else
            {
                List<int?> serviceIDList = inventoryRecords.Select(y => y.InventoryID).ToList();

                //Loading Skill list for each service in inventoryIDList
                List<SharedClasses.ItemList> serviceSkillList = SharedFunctions.GetItemWithList<FSServiceSkill,
                                                                                                FSServiceSkill.serviceID,
                                                                                                FSServiceSkill.skillID>(graph, serviceIDList);

                //Loading Skill list for each employee in employeeSelection
                List<SharedClasses.ItemList> employeeSkillList = SharedFunctions.GetItemWithList<FSEmployeeSkill,
                                                                                                 FSEmployeeSkill.employeeID,
                                                                                                 FSEmployeeSkill.skillID>(graph, employeeSelection);

                //Loading License list for each service in inventoryIDList
                List<SharedClasses.ItemList> serviceLicenseList = SharedFunctions.GetItemWithList<FSServiceLicenseType,
                                                                                                FSServiceLicenseType.serviceID,
                                                                                                FSServiceLicenseType.licenseTypeID>(graph, serviceIDList);

                //NOTE: TIME info must be removed in order to catch licenses that expire the same day as ScheduledDateTimeBegin
                filter.Current.ScheduledDateTimeBegin = SharedFunctions.RemoveTimeInfo(filter.Current.ScheduledDateTimeBegin);

                //Loading License list for each employee in employeeSelection
                List<SharedClasses.ItemList> employeeLicenseList = SharedFunctions.GetItemWithList<FSLicense,
                                                                                                 FSLicense.employeeID,
                                                                                                 FSLicense.licenseTypeID,
                                                                                                 Where2<
                                                                                                        Where<Current<ServiceSelectionFilter.scheduledDateTimeBegin>, IsNull>,
                                                                                                    Or<
                                                                                                        Where<
                                                                                                            FSLicense.issueDate, LessEqual<Current<ServiceSelectionFilter.scheduledDateTimeBegin>>,
                                                                                                            And<
                                                                                                                Where<
                                                                                                                    FSLicense.expirationDate, GreaterEqual<Current<ServiceSelectionFilter.scheduledDateTimeBegin>>,
                                                                                                                    Or<FSLicense.expirationDate, IsNull>>>>>>>(graph, employeeSelection);

                foreach (InventoryItem inventoryRow in inventoryRecords)
                {
                    SharedClasses.ItemList inventoryItemSkillList = null;
                    SharedClasses.ItemList inventoryItemLicenseList = null;

                    if (serviceSkillList.Count != 0)
                    {
                        inventoryItemSkillList = serviceSkillList.FirstOrDefault(y => y.itemID == inventoryRow.InventoryID);
                    }

                    if (serviceLicenseList.Count != 0)
                    {
                        inventoryItemLicenseList = serviceLicenseList.FirstOrDefault(y => y.itemID == inventoryRow.InventoryID);
                    }

                    if ((inventoryItemSkillList == null 
                        || CanThisServiceBeCompleteByTheseEmployeesSkills(inventoryItemSkillList, employeeSkillList))
                        && (inventoryItemLicenseList == null
                        || CanThisServiceBeCompleteByTheseEmployeesLicenses(inventoryItemLicenseList, employeeLicenseList)))
                    {
                        yield return inventoryRow;
                    }

                    continue;
                }
            }
        }

        #endregion
    }
}
