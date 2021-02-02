using System;
using System.IO;
using System.Runtime.Serialization;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Export;
using PX.Objects.CR;
using Email = PX.Common.Mail.Email;

namespace PX.Objects.EP
{
	public sealed class PXExportHandlerEml : PXFileExportHandler
	{
		private class ExportRedirectException : PXExportRedirectException
		{
			public ExportRedirectException(object data) 
				: base("axd", "ExportEmail", _KEY, data)
			{
			}

			public override string Url
			{
				get { return string.Format("{0}.{1}", Message, Extension); }
			}

			public ExportRedirectException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		public readonly static string _FILENAME_PARAM_NAME = "_" + typeof(PXExportHandlerEml).Name + "_FileName";
		public readonly static string _KEY = "_" + typeof(PXExportHandlerEml).Name + "_";

		#region Protected Methods
		protected override string DataSessionKey
		{
			get { return _KEY; }
		}

		protected override string ContentType
		{
			get { return "message/rfc822"; }
		}

		protected override void Write(Stream stream, ProcessBag bag)
		{
			var emlFile = (bag.Data as Email).With(_ => _.Message);
			emlFile.ToStream(stream);
		}

		protected override string GetFileName(ProcessBag bag)
		{
			var fileName = (bag.Data as Email).With(_ => _.Message).With(_ => _.Subject);
			var name = !string.IsNullOrEmpty(fileName) ? fileName : base.GetFileName(bag);
			return name + ".eml";
		}
		#endregion

		#region Private Methods

		#endregion

		public static PXExportRedirectException GenerateException(Email mail)
		{
			return new ExportRedirectException(mail);
		}
	}
}
