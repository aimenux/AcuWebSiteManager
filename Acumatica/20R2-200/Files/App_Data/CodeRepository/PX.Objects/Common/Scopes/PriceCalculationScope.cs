using System;
using PX.Common;
using PX.Data;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class PriceCalculationScope<PriceListTable, KeyField> : UpdateIfFieldsChangedScope
		where PriceListTable : class, IBqlTable, new()
		where KeyField : IBqlField
	{
		public override bool IsUpdateNeeded(params Type[] changes)
			=> base.IsUpdateNeeded(changes) ? true : IsPriceListExist();

		public virtual bool IsPriceListExist()
			=> RecordExistsSlot<PriceListTable, KeyField>.IsRowsExists();
	}
}
