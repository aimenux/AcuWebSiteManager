using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class SM_EmployeeMaint : PXGraphExtension<EmployeeMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Selects

        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        public PXSelectJoin<FSEmployeeSkill,
               InnerJoin<FSSkill,
               On<
                   FSSkill.skillID, Equal<FSEmployeeSkill.skillID>>>,
               Where<
                   FSEmployeeSkill.employeeID, Equal<Current<EPEmployee.bAccountID>>>> EmployeeSkills;

        public PXSelectJoin<FSGeoZoneEmp,
               InnerJoin<FSGeoZone,
               On<
                   FSGeoZone.geoZoneID, Equal<FSGeoZoneEmp.geoZoneID>>>,
               Where<
                   FSGeoZoneEmp.employeeID, Equal<Current<EPEmployee.bAccountID>>>> EmployeeGeoZones;

        [PXViewDetailsButton(typeof(EPEmployee))]
        public PXSelectJoin<FSLicense,
               InnerJoin<FSLicenseType,
               On<
                   FSLicenseType.licenseTypeID, Equal<FSLicense.licenseTypeID>>>, 
               Where<
                   FSLicense.employeeID, Equal<Current<EPEmployee.bAccountID>>>> EmployeeLicenses;

        public PXSelectJoin<FSEmployeeSkill,
               InnerJoin<FSSkill, 
               On<
                   FSSkill.skillID, Equal<FSEmployeeSkill.skillID>, 
                   And<FSSkill.isDriverSkill, Equal<True>>>>,
               Where<
                   FSEmployeeSkill.employeeID, Equal<Current<EPEmployee.bAccountID>>>> EmployeeDriverSkills;
        #endregion

        #region Actions
        public PXAction<EPEmployee> EmployeeSchedule;
        [PXButton]
        [PXUIField(DisplayName = "Schedule", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void employeeSchedule()
        {
            if (this.Base.Employee.Current.BAccountID != null 
                    && !string.IsNullOrEmpty(this.Base.Employee.Current.AcctName))
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["StaffMember"] = this.Base.Employee.Current.AcctCD;
                throw new PXRedirectToGIWithParametersRequiredException(new Guid(TX.GenericInquiries_GUID.STAFF_SCHEDULE_RULES), parameters);
            }
            else
            {
                throw new PXException(TX.Error.NOT_EMPLOYEE_SELECTED);
            }
        }


        #region OpenLicenseDocumente
        public PXAction<EPEmployee> OpenLicenseDocument;
        [PXButton]
        [PXUIField(DisplayName = "Open License Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openLicenseDocument()
        {
            FSLicense fsLicenseRow = EmployeeLicenses.Current;
            LicenseMaint graphLicenseMaintEntry = PXGraph.CreateInstance<LicenseMaint>();
            graphLicenseMaintEntry.LicenseRecords.Current = graphLicenseMaintEntry.LicenseRecords.Search<FSLicense.refNbr>(EmployeeLicenses.Current.RefNbr);
            throw new PXRedirectRequiredException(graphLicenseMaintEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #endregion

        #region CacheAttached
        #region FSGeoZoneEmp_EmployeeID
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<FSGeoZoneEmp.employeeID>>>>))]
        [PXDBLiteDefault(typeof(EPEmployee.bAccountID))]
        public virtual void FSGeoZoneEmp_EmployeeID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLicense_EmployeeID
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<FSLicense.employeeID>>>>))]
        [PXDBLiteDefault(typeof(EPEmployee.bAccountID))]
        public virtual void FSLicense_EmployeeID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSGeoZoneEmp_GeoZoneID
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Service Area ID")]
        [PXSelector(typeof(FSGeoZone.geoZoneID), SubstituteKey = typeof(FSGeoZone.geoZoneCD))]
        public virtual void FSGeoZoneEmp_GeoZoneID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLicense_LicenseTypeID
        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "License Type ID")]
        [PXSelector( typeof(Search<FSLicenseType.licenseTypeID>), SubstituteKey = typeof(FSLicenseType.licenseTypeCD), DescriptionField = typeof(FSLicense.descr))]
        public virtual void FSLicense_LicenseTypeID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLicense_RefNbr
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "License Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [AutoNumber(typeof(Search<FSSetup.licenseNumberingID>),
                    typeof(AccessInfo.businessDate))]
        public virtual void FSLicense_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLicense_LicenseID
        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "License ID", Enabled = false)]
        public virtual void FSLicense_LicenseID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Virtual Methods
        public virtual void EnableDisableGrids(bool enableGrid)
        {
            EmployeeSkills.Cache.AllowInsert = enableGrid;
            EmployeeSkills.Cache.AllowUpdate = enableGrid;
            EmployeeSkills.Cache.AllowDelete = enableGrid;

            EmployeeGeoZones.Cache.AllowInsert = enableGrid;
            EmployeeGeoZones.Cache.AllowUpdate = enableGrid;
            EmployeeGeoZones.Cache.AllowDelete = enableGrid;

            EmployeeLicenses.Cache.AllowInsert = enableGrid;
            EmployeeLicenses.Cache.AllowUpdate = enableGrid;
            EmployeeLicenses.Cache.AllowDelete = enableGrid;
        }

        public virtual void EnableDisableLicenseFields(PXCache cache, FSLicense fsLicenseRow, bool enabled)
        {
            PXUIFieldAttribute.SetEnabled<FSLicense.licenseTypeID>(cache, fsLicenseRow, !enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.descr>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.issueDate>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.expirationDate>(cache, fsLicenseRow, fsLicenseRow.NeverExpires != null ? (bool)!fsLicenseRow.NeverExpires : true);
        }
        #endregion

        #region Event Handlers

        #region FSEmployeeSkill Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSEmployeeSkill> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSEmployeeSkill> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEmployeeSkill fsEmployeeSkillRow = (FSEmployeeSkill)e.Row;
            PXUIFieldAttribute.SetEnabled<FSEmployeeSkill.skillID>
                (e.Cache, fsEmployeeSkillRow, string.IsNullOrEmpty(fsEmployeeSkillRow.SkillID.ToString()));
        }

        protected virtual void _(Events.RowInserting<FSEmployeeSkill> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEmployeeSkill fsEmployeeSkillRow = (FSEmployeeSkill)e.Row;
            FSEmployeeSkill field = PXSelect<FSEmployeeSkill,
                                    Where<
                                        FSEmployeeSkill.skillID, Equal<Required<FSEmployeeSkill.skillID>>,
                                    And<
                                        FSEmployeeSkill.employeeID, Equal<Current<EPEmployee.bAccountID>>>>>
                                    .SelectWindowed(Base, 0, 1, fsEmployeeSkillRow.SkillID);
            if (field != null)
            {
                e.Cache.RaiseExceptionHandling<FSEmployeeSkill.skillID>
                    (e.Row, fsEmployeeSkillRow.SkillID, new PXException(TX.Error.ID_ALREADY_USED));
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.RowInserted<FSEmployeeSkill> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSEmployeeSkill> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSEmployeeSkill> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSEmployeeSkill> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSEmployeeSkill> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSEmployeeSkill> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSEmployeeSkill> e)
        {
        }

        #endregion

        #region FSLicense Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<FSLicense, FSLicense.licenseTypeID> e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow != null)
            {
                if (fsLicenseRow.LicenseTypeID != null)
                {
                    FSLicenseType fsLicenseTypeRow = PXSelect<FSLicenseType,
                                                     Where<
                                                         FSLicenseType.licenseTypeID, Equal<Required<FSLicenseType.licenseTypeID>>>>
                                                     .Select(new PXGraph(), fsLicenseRow.LicenseTypeID);

                    if (fsLicenseTypeRow != null)
                    {
                        fsLicenseRow.Descr = fsLicenseTypeRow.Descr;
                    }
                }
            }
        }

        protected virtual void _(Events.FieldUpdated<FSLicense, FSLicense.neverExpires> e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow == null)
                return;

            if (fsLicenseRow.NeverExpires == true)
            {
                fsLicenseRow.ExpirationDate = null;
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSLicense> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSLicense fsLicenseRow = (FSLicense)e.Row;
            EnableDisableLicenseFields(e.Cache, fsLicenseRow, fsLicenseRow.LicenseTypeID != null);

            PXDefaultAttribute.SetPersistingCheck<FSLicense.expirationDate>(e.Cache, fsLicenseRow, fsLicenseRow.NeverExpires != null
                                                                                                 && fsLicenseRow.NeverExpires == true ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
        }

        protected virtual void _(Events.RowInserting<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSLicense> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSLicense fsLicenseRow = (FSLicense)e.Row;
            FSSetup fsSetupRow = SetupRecord.Select();
            if (string.IsNullOrEmpty(fsSetupRow.LicenseNumberingID))
            {
                string displayName = DACHelper.GetDisplayName(typeof(FSSetup));

                EmployeeLicenses.Cache.RaiseExceptionHandling<FSLicense.refNbr>(fsLicenseRow,
                                                                                fsLicenseRow.RefNbr,
                                                                                new PXSetPropertyException(TX.Error.SPECIFY_LICENSE_NUMBERINGID_IN_X, PXErrorLevel.RowError,
                                                                                displayName));

                throw new PXException(TX.Error.SPECIFY_LICENSE_NUMBERINGID_IN_X, displayName);
            }
        }

        protected virtual void _(Events.RowPersisted<FSLicense> e)
        {
        }

        #endregion

        #region FSGeoZoneEmp Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSGeoZoneEmp> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSGeoZoneEmp fsGeoZoneEmpRow = (FSGeoZoneEmp)e.Row;
            PXUIFieldAttribute.SetEnabled<FSGeoZoneEmp.geoZoneID>
                (e.Cache, fsGeoZoneEmpRow, string.IsNullOrEmpty(fsGeoZoneEmpRow.GeoZoneID.ToString()));
        }

        protected virtual void _(Events.RowInserting<FSGeoZoneEmp> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSGeoZoneEmp fsGeoZoneEmpRow = (FSGeoZoneEmp)e.Row;
            FSGeoZoneEmp field = PXSelect<FSGeoZoneEmp,
                                 Where<
                                    FSGeoZoneEmp.geoZoneID, Equal<Required<FSGeoZoneEmp.geoZoneID>>,
                                 And<
                                    FSGeoZoneEmp.employeeID, Equal<Current<EPEmployee.bAccountID>>>>>
                                 .SelectWindowed(Base, 0, 1, fsGeoZoneEmpRow.GeoZoneID);
            if (field != null)
            {
                e.Cache.RaiseExceptionHandling<FSGeoZoneEmp.geoZoneID>
                    (e.Row, fsGeoZoneEmpRow.GeoZoneID, new PXException(TX.Error.ID_ALREADY_USED));
                e.Cancel = true;
            }
        }

        protected virtual void _(Events.RowInserted<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSGeoZoneEmp> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSGeoZoneEmp> e)
        {
        }

        #endregion

        #region EPEmployee Events

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<EPEmployee> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

            EmployeeSchedule.SetEnabled(fsxEPEmployeeRow.SDEnabled == true);
        }

        protected virtual void _(Events.RowSelected<EPEmployee> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

            EnableDisableGrids((bool)fsxEPEmployeeRow.SDEnabled);
        }

        protected virtual void _(Events.RowInserting<EPEmployee> e)
        {
        }

        protected virtual void _(Events.RowInserted<EPEmployee> e)
        {
        }

        protected virtual void _(Events.RowUpdating<EPEmployee> e)
        {
        }

        protected virtual void _(Events.RowUpdated<EPEmployee> e)
        {
        }

        protected virtual void _(Events.RowDeleting<EPEmployee> e)
        {
        }

        protected virtual void _(Events.RowDeleted<EPEmployee> e)
        {
        }

        protected virtual void _(Events.RowPersisting<EPEmployee> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

            if (e.Operation != PXDBOperation.Delete)
            {
                LicenseHelper.CheckStaffMembersLicense(e.Cache.Graph, epEmployeeRow.BAccountID, fsxEPEmployeeRow.SDEnabled, epEmployeeRow.Status);
            }

            fsxEPEmployeeRow.IsDriver = EmployeeDriverSkills.Select().Count > 0;
        }

        protected virtual void _(Events.RowPersisted<EPEmployee> e)
        {
        }

        #endregion

        #endregion
    }
}
