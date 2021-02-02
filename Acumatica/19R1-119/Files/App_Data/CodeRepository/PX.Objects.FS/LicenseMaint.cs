using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class LicenseMaint : PXGraph<LicenseMaint, FSLicense>
    {
        #region Select
        [PXHidden]
        public PXSelect<BAccountStaffMember> DummyView_BAccountStaffMember;
        [PXHidden]
        public PXSelect<BAccountSelectorBase> DummyView_BAccountSelectorBase;
        [PXHidden]
        public PXSelect<Contact> Contacts;
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;
        public PXSelect<FSLicense> LicenseRecords;
        #endregion

        #region Events
        protected virtual void FSLicense_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow != null)
            {
                FSLicenseType fsLicenseTypeRow = PXSelect<FSLicenseType,
                                            Where<FSLicenseType.licenseTypeID, Equal<Required<FSLicenseType.licenseTypeID>>>>.Select(this, fsLicenseRow.LicenseTypeID);

                if (fsLicenseTypeRow != null)
                {
                    if (fsLicenseTypeRow.OwnerType == ID.OwnerType.EMPLOYEE)
                    {
                        PXUIFieldAttribute.SetEnabled<FSLicense.employeeID>(cache, fsLicenseRow, true);
                        PXDefaultAttribute.SetPersistingCheck<FSLicense.employeeID>(cache, fsLicenseRow, PXPersistingCheck.NullOrBlank);
                    }
                    else
                    {
                        PXUIFieldAttribute.SetEnabled<FSLicense.employeeID>(cache, fsLicenseRow, false);
                        PXDefaultAttribute.SetPersistingCheck<FSLicense.employeeID>(cache, fsLicenseRow, PXPersistingCheck.Nothing);
                    }
                }     
            }
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
                            FSLicenseType.licenseTypeID, Equal<Required<FSLicenseType.licenseTypeID>>>>
                    .Select(this, fsLicenseRow.LicenseTypeID);

                    if (fsLicenseTypeRow != null)
                    {
                        fsLicenseRow.Descr = fsLicenseTypeRow.Descr;

                        if (fsLicenseTypeRow.OwnerType == ID.OwnerType.BUSINESS)
                        {
                            fsLicenseRow.EmployeeID = null;
                        }
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
                LicenseRecords.Cache.RaiseExceptionHandling<FSLicense.refNbr>(
                                        fsLicenseRow,
                                        fsLicenseRow.RefNbr,
                                        new PXSetPropertyException(TX.Error.LICENSE_NEED_NUMBERING_ID, PXErrorLevel.Error));
            }
        }

        #region DateValidation
        protected virtual void FSLicense_IssueDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;
            
            if (fsLicenseRow == null)
            {
                return;
            }

            DateTime? issueDate = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (issueDate.HasValue == true)
            {
                if (fsLicenseRow.ExpirationDate < issueDate
                        && fsLicenseRow.ExpirationDate != null)
                {
                    cache.RaiseExceptionHandling<FSLicense.issueDate>
                        (fsLicenseRow, null, new PXSetPropertyException(TX.Error.ISSUE_EXPIRATION_DATE_INCONSISTENCY));
                    e.Cancel = true;
                }
            }
        }

        protected virtual void FSLicense_ExpirationDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow == null)
            {
                return;
            } 

            DateTime? expirationDate = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (expirationDate.HasValue == true)
            {
                if (expirationDate < fsLicenseRow.IssueDate
                            && fsLicenseRow.IssueDate != null)
                {
                    cache.RaiseExceptionHandling<FSLicense.expirationDate>
                        (fsLicenseRow, null, new PXSetPropertyException(TX.Error.ISSUE_EXPIRATION_DATE_INCONSISTENCY));
                    e.Cancel = true;
                }
            }
        }
        #endregion
        #endregion
    }
}