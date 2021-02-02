using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.IN
{
	public class INItemXRefBAccountAttribute : BAccountAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler<INItemXRef.bAccountID>(SelectorAttribute.FieldUpdating);
			sender.Graph.FieldUpdating.AddHandler<INItemXRef.bAccountID>(FieldUpdating);

			sender.Graph.FieldSelecting.RemoveHandler<INItemXRef.bAccountID>(SelectorAttribute.FieldSelecting);
			sender.Graph.FieldSelecting.AddHandler<INItemXRef.bAccountID>(FieldSelecting);
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var crossItem = (INItemXRef)e.Row;
			if (crossItem?.AlternateType?.IsNotIn(INAlternateType.CPN, INAlternateType.VPN) ?? true)
                e.ReturnValue = null;

            SelectorAttribute.FieldSelecting(sender, e);
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			// suppress PXDimensionSelectorAttribute.FieldUpdating to prevent wrong conversion
			// from not set value BAccountID = 0: surrogate key may be erroneously interpreted as natural key
			var crossItem = (INItemXRef)e.Row;
			if (crossItem?.AlternateType == null
				|| crossItem.AlternateType.IsIn(INAlternateType.CPN, INAlternateType.VPN))
			{
				SelectorAttribute.FieldUpdating(sender, e);
			}
		}
	}
}
