using PX.Data;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	public class PREarningTypeSelectorAttribute : PXSelectorAttribute
	{
		Type _SearchCondition;

		public PREarningTypeSelectorAttribute(Type condition = null) : base(typeof(
			Search2<
				EPEarningType.typeCD,
				CrossJoin<PRSetup>,
				Where<PRSetup.enablePieceworkEarningType, Equal<True>,
					Or<PREarningType.isPiecework, NotEqual<True>>>>))
		{
			DescriptionField = typeof(EPEarningType.description);
			_SearchCondition = condition;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_SearchCondition != null)
			{
				WhereAnd(sender, _SearchCondition); 
			}
		}
	}
}
