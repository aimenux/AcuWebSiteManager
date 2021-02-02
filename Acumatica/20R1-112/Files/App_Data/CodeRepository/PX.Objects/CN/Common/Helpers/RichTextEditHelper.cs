using HtmlAgilityPack;
using PX.Objects.CN.Common.Descriptor;

namespace PX.Objects.CN.Common.Helpers
{
	public static class RichTextEditHelper
	{
		public static string GetInnerText(string htmlText)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(htmlText);
			var htmlBody = htmlDocument.DocumentNode.SelectSingleNode(Constants.HtmlParser.HtmlBodyXpath);
			return htmlBody.InnerText.Replace(Constants.HtmlParser.HtmlSpace, string.Empty)
				.Replace(Constants.HtmlParser.InnerSpace, string.Empty);
		}
	}
}