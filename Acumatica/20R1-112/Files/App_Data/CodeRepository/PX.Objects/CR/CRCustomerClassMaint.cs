using PX.Data;
using PX.Objects.CA;
using PX.Objects.CR.DAC;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	/// <exclude/>
	public class CRCustomerClassMaint : PXGraph<CRCustomerClassMaint, CRCustomerClass>
	{
		[PXViewName(Messages.CustomerClass)]
		public PXSelect<CRCustomerClass> CustomerClass;

		[PXHidden]
		public PXSelect<CRCustomerClass,
				Where<CRCustomerClass.cRCustomerClassID, Equal<Current<CRCustomerClass.cRCustomerClassID>>>>
			CustomerClassCurrent;

		[PXViewName(Messages.Attributes)]
        public CSAttributeGroupList<CRCustomerClass, BAccount> Mapping;

        public CRClassNotificationSourceList<NotificationSource,
            CRCustomerClass.cRCustomerClassID, CRNotificationSource.bAccount> NotificationSources;

        public PXSelect<NotificationRecipient,
            Where<NotificationRecipient.refNoteID, IsNull,
              And<NotificationRecipient.sourceID, Equal<Optional<NotificationSource.sourceID>>>>> NotificationRecipients;

        [PXHidden]
		public PXSelect<CRSetup> Setup;

        #region Cache Attached
        #region NotificationSource
        [PXDBGuid(IsKey = true)]
        [PXSelector(typeof(Search<NotificationSetup.setupID,
            Where<NotificationSetup.sourceCD, Equal<CRNotificationSource.bAccount>>>),
			DescriptionField = typeof(NotificationSetup.notificationCD),
			SelectorMode = PXSelectorMode.DisplayModeText | PXSelectorMode.NoAutocomplete)]
        [PXUIField(DisplayName = "Mailing ID")]
        protected virtual void NotificationSource_SetupID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(CRCustomerClass.cRCustomerClassID))]
        [PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.Invisible)]
        [PXParent(typeof(Select2<CRCustomerClass,
            InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>,
            Where<CRCustomerClass.cRCustomerClassID, Equal<Current<NotificationSource.classID>>>>))]
        protected virtual void NotificationSource_ClassID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(8, InputMask = "CC.CC.CC.CC")]
        [PXUIField(DisplayName = "Report")]
        [PXDefault(typeof(Search<NotificationSetup.reportID,
            Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<SiteMap.screenID,
        Where<SiteMap.url, Like<urlReports>,
            And<SiteMap.screenID, Like<PXModule.cr_>>>,
        OrderBy<Asc<SiteMap.screenID>>>), typeof(SiteMap.screenID), typeof(SiteMap.title),
        Headers = new string[] { CA.Messages.ReportID, CA.Messages.ReportName },
        DescriptionField = typeof(SiteMap.title))]
        [PXFormula(typeof(Default<NotificationSource.setupID>))]
        protected virtual void NotificationSource_ReportID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region NotificationRecipient
        [PXDBInt]
        [PXDBLiteDefault(typeof(NotificationSource.sourceID))]
        protected virtual void NotificationRecipient_SourceID_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(10)]
        [PXDefault]
        [CRMContactType.List]
        [PXUIField(DisplayName = "Contact Type")]
        [PXCheckUnique(typeof(NotificationRecipient.contactID),
            Where = typeof(Where<NotificationRecipient.refNoteID, IsNull, And<NotificationRecipient.sourceID, Equal<Current<NotificationRecipient.sourceID>>>>))]
        protected virtual void NotificationRecipient_ContactType_CacheAttached(PXCache sender)
        {
        }
        [PXDBInt]
        [PXUIField(DisplayName = "Contact ID")]
        [PXNotificationContactSelector(typeof(NotificationRecipient.contactType))]
        protected virtual void NotificationRecipient_ContactID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        protected virtual void CRCustomerClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRCustomerClass;
			if (row == null) return;
			
			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void _(Events.FieldUpdated<CRCustomerClass, CRCustomerClass.defaultOwner> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == e.OldValue)
				return;

			row.DefaultAssignmentMapID = null;
		}

		protected virtual void CRCustomerClass_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = e.Row as CRCustomerClass;
			if (row == null) return;
			
			CRSetup s = Setup.Select();

			if (s != null && s.DefaultCustomerClassID == row.CRCustomerClassID)
			{
				s.DefaultCustomerClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void CRCustomerClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CRCustomerClass;
			if (row == null) return;
			
			if (!CanDelete(row))
			{
				throw new PXException(Messages.RecordIsReferenced);
			}
		}

		private bool CanDelete(CRCustomerClass row)
		{
			if (row != null)
			{
				BAccount c = PXSelect<BAccount, 
					Where<BAccount.classID, Equal<Required<BAccount.classID>>>>.
					SelectWindowed(this, 0, 1, row.CRCustomerClassID);
				if (c != null)
				{
					return false;
				}
			}

			return true;
		}
	}
}
