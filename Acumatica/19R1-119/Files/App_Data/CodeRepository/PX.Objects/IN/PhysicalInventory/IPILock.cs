using System;

namespace PX.Objects.IN.PhysicalInventory
{
	public interface IPILock
	{
		Int32? SiteID
		{
			get;
			set;
		}

		Boolean? Active
		{
			get;
			set;
		}

		String PIID
		{
			get;
			set;
		}

		Boolean? Excluded
		{
			get;
			set;
		}
	}
}
