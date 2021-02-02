using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;

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
                    On<FSSkill.skillID, Equal<FSEmployeeSkill.skillID>>>,
                Where<FSEmployeeSkill.employeeID, Equal<Current<EPEmployee.bAccountID>>>> EmployeeSkills;

        public PXSelectJoin<FSGeoZoneEmp,
                InnerJoin<FSGeoZone,
                    On<FSGeoZone.geoZoneID, Equal<FSGeoZoneEmp.geoZoneID>>>,
                Where<FSGeoZoneEmp.employeeID, Equal<Current<EPEmployee.bAccountID>>>> EmployeeGeoZones;

        [PXViewDetailsButton(typeof(EPEmployee))]
        public PXSelectJoin<FSLicense,
                InnerJoin<FSLicenseType,
                    On<FSLicenseType.licenseTypeID,
                    Equal<FSLicense.licenseTypeID>>>, 
                Where<
                    FSLicense.employeeID,
                    Equal<Current<EPEmployee.bAccountID>>,
                And<
                    FSLicenseType.ownerType,
                    Equal<FSLicenseFilter.ownerType.Employee>>>> EmployeeLicenses;

        public PXSelectJoin<FSEmployeeSkill,
               InnerJoin<FSSkill, 
                On<FSSkill.skillID, Equal<FSEmployeeSkill.skillID>, 
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
        [PXSelector(
            typeof(
                Search<FSLicenseType.licenseTypeID, 
                Where<
                    FSLicenseType.ownerType, Equal<ListField_OwnerType.Employee>>>), 
            SubstituteKey = typeof(FSLicenseType.licenseTypeCD), 
            DescriptionField = typeof(FSLicense.descr))]
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

        #region Private Functions
        private void EnableDisableGrids(bool enableGrid)
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

        private void EnableDisableLicenseFields(PXCache cache, FSLicense fsLicenseRow, bool enabled)
        {
            PXUIFieldAttribute.SetEnabled<FSLicense.licenseTypeID>(cache, fsLicenseRow, !enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.certificateRequired>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.descr>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.expirationDate>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.initialAmount>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.initialTerm>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.initialTermType>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.issueByVendorID>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.issueDate>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.issuingAgencyName>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.renewalAmount>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.renewalTerm>(cache, fsLicenseRow, enabled);
            PXUIFieldAttribute.SetEnabled<FSLicense.renewalTermType>(cache, fsLicenseRow, enabled);
        }
        #endregion

        #region Event Handlers

        #region FSEmployeeSkill Events

        protected virtual void FSEmployeeSkill_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSEmployeeSkill fsEmployeeSkillRow = (FSEmployeeSkill)e.Row;
            PXUIFieldAttribute.SetEnabled<FSEmployeeSkill.skillID>
                (cache, fsEmployeeSkillRow, string.IsNullOrEmpty(fsEmployeeSkillRow.SkillID.ToString()));
        }

        protected virtual void FSEmployeeSkill_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
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
                cache.RaiseExceptionHandling<FSEmployeeSkill.skillID>
                    (e.Row, fsEmployeeSkillRow.SkillID, new PXException(TX.Error.ID_ALREADY_USED));
                e.Cancel = true;
            }
        }

        #endregion

        #region FSLicense Events

        protected virtual void FSLicense_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSLicense fsLicenseRow = (FSLicense)e.Row;
            EnableDisableLicenseFields(cache, fsLicenseRow, fsLicenseRow.LicenseTypeID != null);
        }

        protected virtual void FSLicense_LicenseTypeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow != null)
            {
                if (fsLicenseRow.LicenseTypeID != null)
                {
                    FSLicenseType fsLicenseTypeRow = 
                        PXSelect<FSLicenseType,
                        Where<
                            FSLicenseType.licenseTypeID, 
                            Equal<Required<FSLicenseType.licenseTypeID>>>>
                    .Select(new PXGraph(), fsLicenseRow.LicenseTypeID);

                    if (fsLicenseTypeRow != null)
                    {
                        fsLicenseRow.Descr = fsLicenseTypeRow.Descr;
                    }
                }
            }
        }

        protected virtual void FSLicense_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSLicense fsLicenseRow = (FSLicense)e.Row;
            FSSetup fsSetupRow = SetupRecord.Select();
            if (string.IsNullOrEmpty(fsSetupRow.LicenseNumberingID))
            {
                EmployeeLicenses.Cache.RaiseExceptionHandling<FSLicense.refNbr>(
                        fsLicenseRow,
                        fsLicenseRow.RefNbr,
                        new PXSetPropertyException(TX.Error.LICENSE_NEED_NUMBERING_ID, PXErrorLevel.RowError));

                throw new PXException(TX.Error.LICENSE_NEED_NUMBERING_ID);
            }
        }

        #endregion

        #region FSGeoZoneEmp Events

        protected virtual void FSGeoZoneEmp_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSGeoZoneEmp fsGeoZoneEmpRow = (FSGeoZoneEmp)e.Row;
            PXUIFieldAttribute.SetEnabled<FSGeoZoneEmp.geoZoneID>
                (cache, fsGeoZoneEmpRow, string.IsNullOrEmpty(fsGeoZoneEmpRow.GeoZoneID.ToString()));
        }

        protected virtual void FSGeoZoneEmp_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
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
                cache.RaiseExceptionHandling<FSGeoZoneEmp.geoZoneID>
                    (e.Row, fsGeoZoneEmpRow.GeoZoneID, new PXException(TX.Error.ID_ALREADY_USED));
                e.Cancel = true;
            }
        }

        #endregion

        #region EPEmployee Events

        public virtual void EPEmployee_PositionID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;

            //TODO - SD6594
            //if (epEmployeeRow.PositionID != null)
            //{                
            //    EPPosition epPositionRow        =   PXSelect<EPPosition, 
            //                                        Where<EPPosition.positionID, 
            //                                        Equal<Current<EPEmployee.positionID>>>>
            //                                        .Select(Base);
                
            //    //Extension for the EPPosition table
            //    FSxEPPosition fsxPositionRow      = PXCache<EPPosition>.GetExtension<FSxEPPosition>(epPositionRow);
                
            //    //Extension for the EPEmployee table
            //    FSxEPEmployee fsxEPEmployeeRow  = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);
            //    if (fsxEPEmployeeRow.SDEnabled == false)
            //    {
            //        fsxEPEmployeeRow.SDEnabled = fsxPositionRow.SDEnabled == true;
            //    }
            //}                        
        }

        public virtual void EPEmployee_SDEnabled_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);
            if (fsxEPEmployeeRow.SDEnabled == false)
            {
                fsxEPEmployeeRow.SendAppNotification = false;
            }
        }

        public virtual void EPEmployee_SendAppNotification_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (Base.Contact.Cache.GetStatus(Base.Contact.Cache.Current) != PXEntryStatus.Inserted)
            {
                Base.Contact.Cache.SetStatus(Base.Contact.Current, PXEntryStatus.Updated);
            }
        }

        public virtual void EPEmployee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

            EnableDisableGrids((bool)fsxEPEmployeeRow.SDEnabled);
            PXUIFieldAttribute.SetEnabled<FSxEPEmployee.sendAppNotification>(cache, epEmployeeRow, fsxEPEmployeeRow.SDEnabled == true);
        }

        public virtual void EPEmployee_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

            EmployeeSchedule.SetEnabled(fsxEPEmployeeRow.SDEnabled == true);
        }

        protected virtual void EPEmployee_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }
            
            EPEmployee epEmployeeRow = (EPEmployee)e.Row;
            FSxEPEmployee fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

            if (e.Operation != PXDBOperation.Delete)
            {
                LicenseHelper.CheckStaffMembersLicense(cache.Graph, epEmployeeRow.BAccountID, fsxEPEmployeeRow.SDEnabled, epEmployeeRow.Status);
            }

            fsxEPEmployeeRow.IsDriver = EmployeeDriverSkills.Select().Count > 0;

            Contact contactRow = Base.Contact.Current;

            if (contactRow != null)
            {
                if (fsxEPEmployeeRow.SendAppNotification == true && contactRow.EMail == null)
                {
                    if (Base.Contact.Cache.RaiseExceptionHandling<Contact.eMail>(contactRow, contactRow.EMail, new PXException(TX.Error.EMAIL_CANNOT_BE_NULL_IF_SENDAPPNOTIFICATION_IS_TRUE)))
                    {
                        throw new PXRowPersistingException(typeof(Contact.eMail).Name, contactRow.EMail, TX.Error.EMAIL_CANNOT_BE_NULL_IF_SENDAPPNOTIFICATION_IS_TRUE, typeof(Contact.eMail).Name);
                    }
                }
            }
        }

        #endregion

        #region Contact Events

        public virtual void Contact_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            Contact contactRow = (Contact)e.Row;

            EPEmployee epEmployeeRow = Base.Employee.Current;

            FSxEPEmployee fsxEPEmployeeRow;
            if (epEmployeeRow != null)
            {
                fsxEPEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);
                if (fsxEPEmployeeRow.SendAppNotification == true && contactRow.EMail == null)
                {
                    if (cache.RaiseExceptionHandling<Contact.eMail>(contactRow, contactRow.EMail, new PXException(TX.Error.EMAIL_CANNOT_BE_NULL_IF_SENDAPPNOTIFICATION_IS_TRUE)))
                    {
                        throw new PXRowPersistingException(typeof(Contact.eMail).Name, contactRow.EMail, TX.Error.EMAIL_CANNOT_BE_NULL_IF_SENDAPPNOTIFICATION_IS_TRUE, typeof(Contact.eMail).Name);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
