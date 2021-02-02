using PX.Data;
using PX.SM;
using System;

namespace PX.Objects.CR
{
	[DashboardType((int)DashboardTypeAttribute.Type.WikiArticle)]
	[PXHidden(ServiceVisible = true)]
	public class KBShowReader : WikiShowReader
	{
		protected override void PerformInsert(WikiDescriptor wiki, WikiPage page)
		{
			WikiActions.Insert(wiki, page, null, GetCorrectGraphType);
		}

		protected override void PerformDelete(WikiPage page)
		{
			WikiActions.Delete(page, GetCorrectGraphType);
		}

		private static Type GetCorrectGraphType(int? type)
		{
			if (type == WikiArticleTypeAttribute._KB_ARTICLE_TYPE)
				return typeof(KBArticleMaint);
			return Wiki.GraphType(type);
		}
	}
}
