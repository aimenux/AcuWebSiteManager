using PX.Data;
using PX.Objects.CR;
using System;

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

        #region FSLicense

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        protected virtual void _(Events.FieldVerifying<FSLicense, FSLicense.expirationDate> e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow == null)
            {
                return;
            }

            DateTime? expirationDate = SharedFunctions.TryParseHandlingDateTime(e.Cache, e.NewValue);

            if (expirationDate.HasValue == true)
            {
                if (expirationDate < fsLicenseRow.IssueDate
                            && fsLicenseRow.IssueDate != null)
                {
                    e.Cache.RaiseExceptionHandling<FSLicense.expirationDate>
                        (fsLicenseRow, null, new PXSetPropertyException(TX.Error.ISSUE_EXPIRATION_DATE_INCONSISTENCY));
                    e.Cancel = true;
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<FSLicense, FSLicense.issueDate> e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow == null)
            {
                return;
            }

            DateTime? issueDate = SharedFunctions.TryParseHandlingDateTime(e.Cache, e.NewValue);

            if (issueDate.HasValue == true)
            {
                if (fsLicenseRow.ExpirationDate != null
                        && fsLicenseRow.ExpirationDate < issueDate)
                {
                    e.Cache.RaiseExceptionHandling<FSLicense.issueDate>
                        (fsLicenseRow, null, new PXSetPropertyException(TX.Error.ISSUE_EXPIRATION_DATE_INCONSISTENCY));
                    e.Cancel = true;
                }
            }
        }
        #endregion
        #region FieldUpdated
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
                                                     .Select(this, fsLicenseRow.LicenseTypeID);

                    if (fsLicenseTypeRow != null)
                    {
                        fsLicenseRow.Descr = fsLicenseTypeRow.Descr;
                    }
                }
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSLicense> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSLicense> e)
        {
            FSLicense fsLicenseRow = (FSLicense)e.Row;

            if (fsLicenseRow != null)
            {
                PXUIFieldAttribute.SetEnabled<FSLicense.expirationDate>(e.Cache, fsLicenseRow, fsLicenseRow.NeverExpires != null ? (bool)!fsLicenseRow.NeverExpires : true);
                PXDefaultAttribute.SetPersistingCheck<FSLicense.expirationDate>(e.Cache, fsLicenseRow, fsLicenseRow.NeverExpires != null
                                                                                                     && fsLicenseRow.NeverExpires == true ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
            }
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
                LicenseRecords.Cache.RaiseExceptionHandling<FSLicense.refNbr>(
                                        fsLicenseRow,
                                        fsLicenseRow.RefNbr,
                                        new PXSetPropertyException(TX.Error.SPECIFY_LICENSE_NUMBERINGID_IN_X, PXErrorLevel.Error,
                                        DACHelper.GetDisplayName(typeof(FSSetup))));
            }
        }

        protected virtual void _(Events.RowPersisted<FSLicense> e)
        {
        }

        #endregion

        #endregion
    }
}