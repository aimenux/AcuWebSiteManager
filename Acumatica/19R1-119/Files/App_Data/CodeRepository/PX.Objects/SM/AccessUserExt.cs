using System;
using PX.Data;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.SM
{
	public class AccessExt : PXGraphExtension<Access>
	{
		[PXOverride]
		public void SendUserNotification(int? accountId, Notification notification, Action<int?, Notification> del)
		{
			var gen = TemplateNotificationGenerator.Create(Base, Base.UserList.Current, notification);
			gen.MailAccountId = accountId;            
			gen.To = Base.UserList.Current.Email;
			gen.LinkToEntity = true;
            gen.Body = gen.Body.Replace("((UserList.Password))", Base.UserList.Current.Password);
			gen.Send();            
		}
	}
}
