using System;
using PX.Data;

/*
SendOnlyEventCard
IsSimpleNotification
AddContactInformation
 * 
InvitationTemplateID
RescheduleTemplateID
CancelInvitationTemplateID
 * 
SearchOnlyInWorkingTime

 */


namespace PX.Objects.EP
{
	public class EPEventSetupMaint : PXGraph<EPEventSetupMaint>
	{

		#region Selects Declartion
		public PXSelect<EPSetup>
			Setup;

		#endregion

		#region Buttons Declaration

		public PXSave<EPSetup> Save;
		public PXCancel<EPSetup> Cancel;

		#endregion

		#region Events

		protected virtual void EPSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPSetup row = e.Row as EPSetup;
			if (row != null)
			{
				bool sendOnlyCard = row.SendOnlyEventCard == true;
				PXUIFieldAttribute.SetEnabled<EPSetup.isSimpleNotification>(cache, row, !sendOnlyCard);
				bool simpleNotification = row.IsSimpleNotification == true;
				PXUIFieldAttribute.SetEnabled<EPSetup.addContactInformation>(cache, row, !sendOnlyCard && simpleNotification);
				bool enableCustomTemplateIDs = !sendOnlyCard && !simpleNotification;
				PXUIFieldAttribute.SetEnabled<EPSetup.invitationTemplateID>(cache, row, enableCustomTemplateIDs);
				PXUIFieldAttribute.SetEnabled<EPSetup.rescheduleTemplateID>(cache, row, enableCustomTemplateIDs);
				PXUIFieldAttribute.SetEnabled<EPSetup.cancelInvitationTemplateID>(cache, row, enableCustomTemplateIDs);
			}
		}
		#endregion


		protected virtual void EPSetup_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as EPSetup;
			if (row == null || e.Operation == PXDBOperation.Delete) return;
			if (row.IsSimpleNotification == true || row.SendOnlyEventCard == true) return;
			CheckNotificationTemplateForEmpty(cache, e.Row, typeof(EPSetup.invitationTemplateID).Name);
			CheckNotificationTemplateForEmpty(cache, e.Row, typeof(EPSetup.rescheduleTemplateID).Name);
			CheckNotificationTemplateForEmpty(cache, e.Row, typeof(EPSetup.cancelInvitationTemplateID).Name);
		}

		#region Private Methods

		private static void CheckNotificationTemplateForEmpty(PXCache cache, object row, string fieldName)
		{
			if (cache.GetValue(row, fieldName) != null) return;

			if (cache.RaiseExceptionHandling(fieldName, row, null, new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, fieldName))))
				throw new PXRowPersistingException(fieldName, null, ErrorMessages.FieldIsEmpty, fieldName);
		}

		#endregion


	}
}