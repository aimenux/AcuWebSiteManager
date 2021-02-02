using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.DR
{
	public interface ISingleScheduleViewProvider
	{
		PXResultset<ARTran> GetParentDocumentDetails();
	}
}
