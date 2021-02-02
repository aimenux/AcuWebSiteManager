using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	public class SOSetupMaint : PXGraph<SOSetupMaint>
	{
		public PXSave<SOSetup> Save;
		public PXCancel<SOSetup> Cancel;

		public PXSelect<SOSetup> sosetup;
        public PXSelect<SOSetupApproval> SetupApproval;
		public PXSelect<SOPickPackShipSetup, Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>> PickPackShipSetup;
		public PXSelect<SOPickPackShipUserSetup, Where<SOPickPackShipUserSetup.isOverridden, Equal<False>>> PickPackShipUserSetups;

        public CRNotificationSetupList<SONotification> Notifications;
		public PXSelect<NotificationSetupRecipient,
			Where<NotificationSetupRecipient.setupID, Equal<Current<SONotification.setupID>>>> Recipients;


		#region CacheAttached
		[PXDBString(10)]
		[PXDefault]
		[VendorContactType.ClassList]
		[PXUIField(DisplayName = "Contact Type")]
		[PXCheckUnique(typeof(NotificationSetupRecipient.contactID),
			Where = typeof(Where<NotificationSetupRecipient.setupID, Equal<Current<NotificationSetupRecipient.setupID>>>))]
		public virtual void NotificationSetupRecipient_ContactType_CacheAttached(PXCache sender)
		{
		}
		[PXDBInt]
		[PXUIField(DisplayName = "Contact ID")]
		[PXNotificationContactSelector(typeof(NotificationSetupRecipient.contactType),
			typeof(Search2<Contact.contactID,
				LeftJoin<EPEmployee,
							On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
							And<EPEmployee.defContactID, Equal<Contact.contactID>>>>,
				Where<Current<NotificationSetupRecipient.contactType>, Equal<NotificationContactType.employee>,
							And<EPEmployee.acctCD, IsNotNull>>>))]
		public virtual void NotificationSetupRecipient_ContactID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		public void SOSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOSetup setup = e.Row as SOSetup;
			if (setup == null) return;

			PXUIFieldAttribute.SetEnabled<SOSetup.createZeroShipments>(sender, null, setup.AddAllToShipment == true);
			PXUIFieldAttribute.SetEnabled<SOSetup.shippedNotInvoicedAcctID>(sender, null, setup.UseShippedNotInvoiced == true);
			PXUIFieldAttribute.SetEnabled<SOSetup.shippedNotInvoicedSubID>(sender, null, setup.UseShippedNotInvoiced == true);
		}

		protected virtual void SOSetup_DefaultOrderType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOSetup setup = e.Row as SOSetup;
			if (setup == null) return;

			if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				e.NewValue = SOOrderTypeConstants.Invoice;
			}
		}
		public void SOSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            SOSetup setup = e.Row as SOSetup;
            if (setup == null) return;

            if (setup.UseShippedNotInvoiced == true && setup.ShippedNotInvoicedAcctID == null)
                throw new PXRowPersistingException(typeof(SOSetup.shippedNotInvoicedAcctID).Name, null, ErrorMessages.FieldIsEmpty, typeof(SOSetup.shippedNotInvoicedAcctID).Name);
            if (setup.UseShippedNotInvoiced == true && setup.ShippedNotInvoicedSubID == null && PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
                throw new PXRowPersistingException(typeof(SOSetup.shippedNotInvoicedSubID).Name, null, ErrorMessages.FieldIsEmpty, typeof(SOSetup.shippedNotInvoicedSubID).Name);
        }

		public SOSetupMaint()
		{
			
		}

        protected virtual void SOSetup_OrderRequestApproval_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXCache cache = this.Caches[typeof(SOSetupApproval)];
            PXResultset<SOSetupApproval> setups = PXSelect<SOSetupApproval>.Select(sender.Graph, null);
            foreach (SOSetupApproval setup in setups)
            {
				setup.IsActive = (bool?)e.NewValue;
                cache.Update(setup);
            }
        }

		protected virtual IEnumerable pickPackShipSetup()
		{
			SOPickPackShipSetup result = new PXSelect<SOPickPackShipSetup,
				Where<SOPickPackShipSetup.branchID, Equal<Current<AccessInfo.branchID>>>>(this).Select();

			if (result == null) result = PickPackShipSetup.Insert();

			return new SOPickPackShipSetup[] { result };
		}

		protected virtual void _(Events.FieldVerifying<SOPickPackShipSetup, SOPickPackShipSetup.showPickTab> e)
		{
			if ((bool)e.NewValue != true && PickPackShipSetup.Current?.ShowPackTab != true &&
				PickPackShipSetup.Current?.ShowShipTab != true) e.NewValue = true;
		}

		protected virtual void _(Events.FieldVerifying<SOPickPackShipSetup, SOPickPackShipSetup.showPackTab> e)
		{
			if ((bool)e.NewValue != true && PickPackShipSetup.Current?.ShowPickTab != true &&
				PickPackShipSetup.Current?.ShowShipTab != true) e.NewValue = true;
		}

		protected virtual void _(Events.FieldVerifying<SOPickPackShipSetup, SOPickPackShipSetup.showShipTab> e)
		{
			if ((bool)e.NewValue != true && PickPackShipSetup.Current?.ShowPickTab != true &&
				PickPackShipSetup.Current?.ShowPackTab != true) e.NewValue = true;
		}

		protected virtual void _(Events.RowUpdated<SOPickPackShipSetup> e)
		{
			if (e.Row != null)
				foreach (SOPickPackShipUserSetup userSetup in PickPackShipUserSetups.Select())
					PickPackShipUserSetups.Update(userSetup.ApplyValuesFrom(e.Row));
		}
	}
}
