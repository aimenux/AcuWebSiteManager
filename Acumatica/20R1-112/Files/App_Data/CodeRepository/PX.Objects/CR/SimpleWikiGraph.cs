using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	public class SimpleWikiGraph : PXGraph<SimpleWikiGraph>
	{
		[PXHidden]
		public PXSelect<WikiPage> wp;

		[PXHidden]
		public PXSelect<WikiPageLanguage> wpl;

		[PXHidden]
		public PXSelect<WikiRevision, Where<WikiRevision.pageID, Equal<Required<WikiRevision.pageID>>>> wr;
	}
}
