using System;

namespace PX.Objects.EP
{
	public class ConfirmReceiptEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true || 
				account.ConfirmReceipt != true || 
				account.ConfirmReceiptNotificationID == null)
			{
				return false;
			}

			var templateId = (int)account.ConfirmReceiptNotificationID;
			var message = package.Message;
			var sender = TemplateNotificationGenerator.Create(message, templateId);
			sender.LinkToEntity = false;
			sender.MailAccountId = account.EmailAccountID;
			sender.Send();

			return true;
		}
	}
}