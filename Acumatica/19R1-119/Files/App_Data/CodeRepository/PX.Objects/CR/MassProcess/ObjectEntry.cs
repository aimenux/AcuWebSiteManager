using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CR.MassProcess
{
	[Obsolete("Will be removed in 7.0 version")]
	struct ObjectEntry
	{
		public object Object;
		public PXEntryStatus Status;

		public ObjectEntry(PXEntryStatus status, object o)
		{
			Status = status;
			Object = o;
		}
	}
}
