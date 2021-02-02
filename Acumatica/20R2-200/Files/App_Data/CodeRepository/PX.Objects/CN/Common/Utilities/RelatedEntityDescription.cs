using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.CN.Common.Utilities
{
	public class RelatedEntityDescription<TRefNoteId> : BqlFormulaEvaluator<TRefNoteId>
		where TRefNoteId : IBqlField
	{
		public const string RelatedEntityDescriptionForSubcontract = "Subcontract, {0}";

		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> entityTypes)
		{
			var refNoteId = (Guid?)entityTypes[typeof(TRefNoteId)];
			var entityHelper = new EntityHelper(cache.Graph);
			var commitment = entityHelper.GetEntityRow(refNoteId) as POOrder;
			var entityDescription = entityHelper.GetEntityDescription(refNoteId, item.GetType());
			return commitment?.OrderType == POOrderType.RegularSubcontract
				? string.Format(RelatedEntityDescriptionForSubcontract, commitment.OrderNbr)
				: entityDescription;
		}
	}
}
