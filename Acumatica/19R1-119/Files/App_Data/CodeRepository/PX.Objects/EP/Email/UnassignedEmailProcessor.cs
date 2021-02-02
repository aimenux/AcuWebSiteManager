using System;

namespace PX.Objects.EP
{
	public class UnassignedEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (package.IsProcessed || 
				account.IncomingProcessing != true || 
				account.ProcessUnassigned != true || 
				account.ResponseNotificationID == null)
			{
				return false;
			}

			var templateId = (int)account.ResponseNotificationID;
			var message = package.Message;
			var sender = TemplateNotificationGenerator.Create(message, templateId);
			sender.LinkToEntity = false;
			sender.MailAccountId = account.EmailAccountID;
			sender.Send();

			return true;
		}
	}
}
