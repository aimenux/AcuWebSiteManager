using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CR
{
	public class EntityDescription<RefNoteID> : BqlFormulaEvaluator<RefNoteID>, IBqlOperand
		where RefNoteID : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			Guid? refNoteID = (Guid?)pars[typeof(RefNoteID)];
			return new EntityHelper(cache.Graph).GetEntityDescription(refNoteID, item.GetType());
		}
	}
}
