using System;
using PX.Data;

namespace PX.Objects.Common.EntityInUse
{
	[Serializable]
	[EntityInUseDBSlotOn]
	[PXHidden]
	public class CurrencyInUse : IBqlTable
	{
		public abstract class curyID : IBqlField { }
		[PXDBString(5, IsKey = true, IsUnicode = true)]
		public virtual string CuryID { get; set; }
	}
}
