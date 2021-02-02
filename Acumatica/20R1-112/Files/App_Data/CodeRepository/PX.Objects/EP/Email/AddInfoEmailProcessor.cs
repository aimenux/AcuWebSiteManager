using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class AddInfoEmailProcessor : BasicEmailProcessor
	{
		private static readonly Regex _HTML_REGEX = new Regex(
			@"^.*\<html( [^\>]*)?\>.*\<head( [^\>]*)?\>(?<head>.*)\</([^\>]* )?head\>.*\<body( [^\>]*)?\>(?<body>.*)\</([^\>]* )?body\>",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		protected override bool Process(Package package)
		{
			var account = package.Account;
			if (account.IncomingProcessing != true || 
				account.AddUpInformation != true)
			{
				return false;
			}

			var graph = package.Graph;
			var message = package.Message;

			var briefInfo = new List<string>();

			var mainReference = GetReferenceEntity(graph, message.RefNoteID);
			AddUpInformation(graph, briefInfo, mainReference);

			// TODO: Remove because it's empty here
			//var parentReference = GetReferenceEntity(graph, message.BAccountID);
			//AddUpInformation(graph, briefInfo, parentReference);

			if (briefInfo.Count > 0) 
				InsertInformationIntoMessage(message, briefInfo);

			return true;
		}

		private void AddUpInformation(PXGraph graph, IList<string> briefInfo, object reference)
		{
			if (reference == null) return;

			var text = EntityHelper.GetEntityDescription(graph, reference).With(_ => _.Trim());
			if (string.IsNullOrEmpty(text)) return;

			var header = EntityHelper.GetFriendlyEntityName(reference.GetType());

			var format = string.IsNullOrEmpty(header) ? "{1}" : "{0}: {1}";
			briefInfo.Add(string.Format(format, header, text));
		}

		private void InsertInformationIntoMessage(CRSMEmail message, IEnumerable<string> briefInfo)
		{
			if (briefInfo == null) return;

			var content = message.Body ?? string.Empty;
			var match = _HTML_REGEX.Match(content);
			if (match.Success)
			{
				var bodyGroup = match.Groups["body"];
				var orgBody = bodyGroup.Value;
				var sb = new StringBuilder();
				foreach (string info in briefInfo)
					sb.AppendFormat("<i>{0}</i><br/>", info);
				sb.AppendFormat("<br/>");
				var htmlText = sb.ToString();
				if (!orgBody.StartsWith(htmlText))
				{
					var newBody = orgBody.Insert(0, htmlText);
					content = content.Substring(0, bodyGroup.Index) + newBody + content.Substring(bodyGroup.Index + bodyGroup.Length);
				}
			}
			else
			{
				var sb = new StringBuilder();
				sb.AppendLine("***");
				foreach (string info in briefInfo)
					sb.AppendLine(info);
				sb.AppendLine("***");
				sb.AppendLine();
				var plainText = sb.ToString();
				if (!content.StartsWith(plainText))
					content = content.Insert(0, plainText);
			}
			message.Body = PX.Web.UI.PXRichTextConverter.NormalizeHtml(content);
		}

		private object GetReferenceEntity(PXGraph graph, Guid? refNoteId)
		{
			if (refNoteId == null) return null;
			var helper = new EntityHelper(graph);
			return helper.GetEntityRow(refNoteId.Value, true);
		}
	}
}
