using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PX.Objects.FS
{
    public class StaffSelectionHelper
    {
        #region Selects
        public class StaffRecords_View : PXSelectJoin<BAccountStaffMember,
                    LeftJoin<Vendor,
                        On<Vendor.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<Vendor.status, NotEqual<CR.BAccount.status.inactive>>>,
                    LeftJoin<EPEmployee,
                        On<EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<EPEmployee.status, NotEqual<CR.BAccount.status.inactive>>>,
                    LeftJoin<FSGeoZoneEmp,
                                         On<
                                             FSGeoZoneEmp.employeeID, Equal<BAccountStaffMember.bAccountID>>,
                    LeftJoin<PMProject,
                                         On<
                                             PMProject.contractID, Equal<Current<FSServiceOrder.projectID>>>,
                    LeftJoin<EPEmployeeContract,
                                         On<
                                             EPEmployeeContract.contractID, Equal<PMProject.contractID>,
                        And<EPEmployeeContract.employeeID, Equal<BAccountStaffMember.bAccountID>>>,
                    LeftJoin<EPEmployeePosition,
                                         On<
                                             EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                        And<EPEmployeePosition.isActive, Equal<True>>>>>>>>>,
                                         Where<
                                             PMProject.isActive, Equal<True>,
                                         And<
                                             PMProject.baseType, Equal<CT.CTPRType.project>,
                        And<
                            Where2<
                                                 Where<
                                                     CurrentValue<StaffSelectionFilter.geoZoneID>, IsNull,
                                                 Or<
                                                     FSGeoZoneEmp.geoZoneID, Equal<Current<StaffSelectionFilter.geoZoneID>>>>,
                                And<
                                    Where2<
                                        Where<
                                            FSxVendor.sDEnabled, Equal<True>,
                                            And<
                                                Where<
                                                    Vendor.status, Equal<BAccountStaffMember.status.active>,
                                                         Or<
                                                             Vendor.status, Equal<BAccountStaffMember.status.oneTime>>>>>,
                                        Or<
                                            Where<
                                                FSxEPEmployee.sDEnabled, Equal<True>,
                                                And<
                                                    Where<
                                                        PMProject.restrictToEmployeeList, Equal<False>,
                                                        Or<
                                                            Where<PMProject.restrictToEmployeeList, Equal<True>,
                                                            And<EPEmployeeContract.employeeID, IsNotNull>>>>>>>>>>>>>,
                        OrderBy<
                            Asc<BAccountStaffMember.acctCD, Asc<FSGeoZoneEmp.geoZoneID>>>>
        {
            public StaffRecords_View(PXGraph graph) : base(graph)
            {
            }

            public StaffRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

       public class SkillRecords_View : PXSelectOrderBy<SkillGridFilter,
                                            OrderBy<
                                                    Desc<SkillGridFilter.mem_ServicesList>>>
        {
            public SkillRecords_View(PXGraph graph) : base(graph)
            {
            }
            
            public SkillRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

       public class LicenseTypeRecords_View : PXSelectOrderBy<LicenseTypeGridFilter,
                                                OrderBy<
                                                    Desc<LicenseTypeGridFilter.mem_FromService>>>
        {
           public LicenseTypeRecords_View(PXGraph graph) : base(graph)
           {
           }

           public LicenseTypeRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
           {
           }
        }
        #endregion

        #region Public Functions
        public void LaunchStaffSelector(PXGraph graph, PXFilter<StaffSelectionFilter> filter)
        {
            if (filter.Current.PostalCode != null && filter.Current.GeoZoneID == null)
            {
                FSGeoZonePostalCode fsGeoZoneRow = GetMatchingGeoZonePostalCode(graph, filter.Current.PostalCode);

                if (fsGeoZoneRow != null)
                {
                    filter.Current.GeoZoneID = fsGeoZoneRow.GeoZoneID;
                }
            }

            filter.Current.ExistContractEmployees = this.ExistContractEmployees(filter.Cache.Graph, filter.Current.ProjectID);
            filter.AskExt();
        }

        public static FSGeoZonePostalCode GetMatchingGeoZonePostalCode(PXGraph graph, string fullPostalCode)
        {
            var fsGeoZoneRows = PXSelectReadonly<FSGeoZonePostalCode>.Select(graph);

            return fsGeoZoneRows.Where(x => Regex.Match(fullPostalCode.Trim(), ((FSGeoZonePostalCode)x).PostalCode.Trim()).Success == true).FirstOrDefault();
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Checks the existence of assigned Employees to the ProjectID.
        /// </summary>
        /// <param name="graph">PXGraph instance for BQL execution.</param>
        /// <param name="projectID">ProjectID to check the existing of Employees.</param>
        /// <returns>True if there are employees assigned to the projectID, False if not.</returns>
        private bool? ExistContractEmployees(PXGraph graph, int? projectID)
        { 
            var epEmployeeContractSet = PXSelect<EPEmployeeContract,
                                         Where<
                                               EPEmployeeContract.contractID, Equal<Required<StaffSelectionFilter.projectID>>>>
                                         .Select(graph, projectID);

            return epEmployeeContractSet.Count > 0;
        }
        #endregion

        #region Private Static Functions
        /// <summary>
        /// Evaluates if the employeeItemList (belonging to an Employee) provided has the items (skills or licenseTypes) selected in filter.
        /// </summary>
        /// <param name="employeeItemList">Employee item list instance.</param>
        /// <param name="itemsSelection">Items list selected in filter.</param>
        /// <returns>True if Employee has items Selected, otherwise False.</returns>
        private static bool HasEmployeeItemsSelected(SharedClasses.ItemList employeeItemList, List<int?> itemsSelection)
        {
            if (employeeItemList != null)
            {
                List<int?> list = employeeItemList.list.Cast<int?>().ToList();

                if (itemsSelection.Except(list).Any())
                {
                    return false;
                }
            }
            else
            {
                if (itemsSelection.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets Staff Members already existing in the Staff tab with the <c>lineRef</c> related. The <c>employeesView</c> can be of type AppointmentEmployees_View or ServiceOrderEmployees_View.
        /// </summary>
        /// <param name="staffView">Object of type AppointmentEmployees_View or ServiceOrderEmployees_View.</param>
        ///  <param name="lineRef">Line ref of related Service Line.</param>
        /// <returns>List of EmployeeID's existing in Employee Tab.</returns>
        private static List<int?> GetStaffByLineRefTab(object staffView, string lineRef)
        {
            List<int?> employeeIDList = new List<int?>();

            if (staffView is AppointmentCore.AppointmentServiceEmployees_View)
            {
                AppointmentCore.AppointmentServiceEmployees_View appEmployeeView = (AppointmentCore.AppointmentServiceEmployees_View)staffView;

                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in appEmployeeView.Select().AsEnumerable().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == lineRef))
                {
                    employeeIDList.Add(fsAppointmentEmployeeRow.EmployeeID);
                }
            }

            if (staffView is ServiceOrderCore.ServiceOrderEmployees_View)
            {
                ServiceOrderCore.ServiceOrderEmployees_View soEmployeeView = (ServiceOrderCore.ServiceOrderEmployees_View)staffView;

                foreach (FSSOEmployee fsSOEmployeeRow in soEmployeeView.Select().AsEnumerable().Where(y => ((FSSOEmployee)y).ServiceLineRef == lineRef))
                {
                    employeeIDList.Add(fsSOEmployeeRow.EmployeeID);
                }
            }

            return employeeIDList;
        }

        /// <summary>
        /// Gets a list of Staff Members related to <c>filter.ServiceLineRef</c>.
        /// </summary>
        /// <param name="filter">Instance of the filter.</param>
        /// <param name="staffView">Current list of Staff Members in the grid.</param>
        /// <returns>A list of EPEmployee ready to be shown in the smartpanel.</returns>
        private static IEnumerable<BAccountStaffMember> GetStaffAvailableForSelect(PXFilter<StaffSelectionFilter> filter, object staffView)
        {
            PXResultset<BAccountStaffMember> bqlResultSet;

            List<int?> staffIDList = GetStaffByLineRefTab(staffView, filter.Current.ServiceLineRef);
            PXSelectBase<BAccountStaffMember> cmd = new StaffSelectionHelper.StaffRecords_View(filter.Cache.Graph);

            bqlResultSet = cmd.Select();            

            var bAccountStaffMemberRowsGrouped = bqlResultSet.AsEnumerable().GroupBy(
                                                                    p => ((BAccountStaffMember)p).BAccountID, 
                                                                    (key, group) => new
                                                                    {
                                                                        Group = (BAccountStaffMember)group.First()
                                                                    })
                                                                    .Select(g => g.Group).ToList();

            foreach (BAccountStaffMember staffMemberRow in bAccountStaffMemberRowsGrouped)
            {
                if (staffIDList.Exists(delegate(int? staffMemberID) { return staffMemberID == staffMemberRow.BAccountID; }) == true)
                {
                    staffMemberRow.Selected = true;
                }
                else
                {
                    staffMemberRow.Selected = false;
                }

                yield return staffMemberRow;
            }
        }

        #endregion

        #region Delegates

        public static IEnumerable SkillFilterDelegate<ServiceDetType>(PXGraph graph,
                                                    PXSelectBase<ServiceDetType> servicesView,
                                                    PXFilter<StaffSelectionFilter> filter,
                                                    PXSelectBase<SkillGridFilter> skillView)
            where ServiceDetType : class, IBqlTable, IFSSODetBase, new()
        {
            List<int?> serviceIDList = new List<int?>();
            List<int?> serviceIDCheckList = new List<int?>();

            IEnumerable<SkillGridFilter> skillRecords = skillView.Cache.Cached.Cast<SkillGridFilter>();
            bool initSkillFilter = skillRecords.Count() == 0;

            if (initSkillFilter)
            {
                serviceIDList = ServiceSelectionHelper.GetServicesInServiceTab<ServiceDetType>(servicesView, null);
                serviceIDCheckList = ServiceSelectionHelper.GetServicesInServiceTab<ServiceDetType>(servicesView, filter.Current.ServiceLineRef);
            }

            foreach (SkillGridFilter skillRow in PXSelect<SkillGridFilter>.Select(graph))
            {
                if (initSkillFilter)
                {
                    skillRow.Mem_ServicesList = SkillGridFilter.GetServiceListField(graph, serviceIDList, skillRow.SkillID);
                    string serviceList = SkillGridFilter.GetServiceListField(graph, serviceIDCheckList, skillRow.SkillID);

                    skillRow.Mem_Selected = string.IsNullOrEmpty(serviceList) == false;
                }

                skillView.Cache.SetStatus(skillRow, PXEntryStatus.Held);
                skillView.Cache.IsDirty = false;

                yield return skillRow;
            }
        }

        public static IEnumerable LicenseTypeFilterDelegate<ServiceDetType>(PXGraph graph,
                                                            PXSelectBase<ServiceDetType> servicesView,
                                                            PXFilter<StaffSelectionFilter> filter,
                                                            PXSelectBase<LicenseTypeGridFilter> licenseTypeView)
            where ServiceDetType : class, IBqlTable, IFSSODetBase, new()
        {
            List<int?> serviceIDList = new List<int?>();

            IEnumerable<LicenseTypeGridFilter> licenseTypeRecords = licenseTypeView.Cache.Cached.Cast<LicenseTypeGridFilter>();
            bool initLicenseTypeFilter = licenseTypeRecords.Count() == 0;

            if (initLicenseTypeFilter)
            {
                serviceIDList = ServiceSelectionHelper.GetServicesInServiceTab<ServiceDetType>(servicesView, filter.Current.ServiceLineRef);
            }

            foreach (LicenseTypeGridFilter licenseTypeRow in PXSelect<LicenseTypeGridFilter>.Select(graph))
            {
                if (initLicenseTypeFilter)
                {
                    licenseTypeRow.Mem_FromService = LicenseTypeGridFilter.IsThisLicenseTypeRequiredByAnyService(graph, licenseTypeRow.LicenseTypeID, serviceIDList);
                    licenseTypeRow.Mem_Selected = licenseTypeRow.Mem_FromService == true;
                }

                licenseTypeView.Cache.SetStatus(licenseTypeRow, PXEntryStatus.Held);
                licenseTypeView.Cache.IsDirty = false;

                yield return licenseTypeRow;
            }
        }

        public static IEnumerable StaffRecordsDelegate(object staffView,
                                                        PXSelectBase<SkillGridFilter> skillView,
                                                        PXSelectBase<LicenseTypeGridFilter> licenseTypeView,
                                                        PXFilter<StaffSelectionFilter> filter)
        {
            if (filter.Current == null)
            {
                yield break;
            }

            PXGraph graphFilter = filter.Cache.Graph;
            IEnumerable<BAccountStaffMember> staffSet = GetStaffAvailableForSelect(filter, staffView);

            List<int?> skillsSelection = skillView.Select()
                                                  .Where(y => ((SkillGridFilter)y).Mem_Selected == true)
                                                  .Select(y => ((SkillGridFilter)y).SkillID)
                                                  .ToList();

            List<int?> licenseTypesSelection = licenseTypeView.Select()
															  .Where(y => ((LicenseTypeGridFilter)y).Mem_Selected == true)
                                                              .Select(y => ((LicenseTypeGridFilter)y).LicenseTypeID)
                                                              .ToList();

            if (skillsSelection.Count == 0 && licenseTypesSelection.Count == 0)
            {
                foreach (BAccountStaffMember staffRow in staffSet)
                {
                    yield return staffRow;
                }
            }
            else 
            {
                List<int?> employeeIDList = staffSet.Select(y => y.BAccountID).ToList();

                //Loading Skill list for each employee in employeeIDList
                List<SharedClasses.ItemList> allStaffSkillList = SharedFunctions.GetItemWithList<FSEmployeeSkill,
                                                                                                 FSEmployeeSkill.employeeID,
                                                                                                 FSEmployeeSkill.skillID>(graphFilter, employeeIDList);

                //NOTE: TIME info must be removed in order to catch licenses that expire the same day as ScheduledDateTimeBegin
                filter.Current.ScheduledDateTimeBegin = SharedFunctions.RemoveTimeInfo(filter.Current.ScheduledDateTimeBegin);

                //Loading LicenseType list for each employee in employeeIDList
                List<SharedClasses.ItemList> allStaffLicenseTypeList = SharedFunctions.GetItemWithList<FSLicense,
                                                                                                       FSLicense.employeeID,
                                                                                                       FSLicense.licenseTypeID,
                                                                                                       Where2<
                                                                                                              Where<Current<StaffSelectionFilter.scheduledDateTimeBegin>, IsNull>,
                                                                                                       Or<
                                                                                                           Where<
                                                                                                                FSLicense.issueDate, LessEqual < Current < StaffSelectionFilter.scheduledDateTimeBegin>>,
                                                                                                           And<
                                                                                                               Where<
                                                                                                                    FSLicense.expirationDate, GreaterEqual<Current<StaffSelectionFilter.scheduledDateTimeBegin>>,
                                                                                                                     Or<FSLicense.expirationDate, IsNull>>>>>>>(graphFilter, employeeIDList);

                foreach (BAccountStaffMember staffRow in staffSet)
                {
                    SharedClasses.ItemList employeeSkillList = allStaffSkillList.FirstOrDefault(y => y.itemID == staffRow.BAccountID);

                    if (HasEmployeeItemsSelected(employeeSkillList, skillsSelection) == false && staffRow.Selected == false)
                    {
                        continue;
                    }

                    SharedClasses.ItemList employeeLicenseTypeList = allStaffLicenseTypeList.FirstOrDefault(y => y.itemID == staffRow.BAccountID);

                    if (HasEmployeeItemsSelected(employeeLicenseTypeList, licenseTypesSelection) == false && staffRow.Selected == false)
                    {
                        continue;
                    }

                    yield return staffRow;
                }
            }
        }
        #endregion
    }
}
