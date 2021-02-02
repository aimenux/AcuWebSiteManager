using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.DR
{
	public interface ISalesPriceProvider
	{
		void SetFairValueSalesPrice(DRScheduleDetail scheduleDetail, Location location, CurrencyInfo currencyInfo);

		decimal GetQuantityInBaseUOMs(ARTran tran);
	}
}