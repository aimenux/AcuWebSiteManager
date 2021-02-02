using System;
using System.Web;
using System.Xml;

namespace PX.Objects.EP
{
	public class PXReminderSyncHandler : IHttpHandler
	{
		public virtual void ProcessRequest(HttpContext context)
		{
			var graph = new TasksAndEventsReminder();
			using (var writer = XmlWriter.Create(context.Response.OutputStream))
			{
				writer.WriteStartElement("result");
				writer.WriteAttributeString("count", graph.GetListCount().ToString());
				writer.WriteEndElement();
			}
		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}
