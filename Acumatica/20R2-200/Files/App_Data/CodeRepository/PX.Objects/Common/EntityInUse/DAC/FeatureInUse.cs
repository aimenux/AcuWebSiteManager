using System;
using PX.Data;

namespace PX.Objects.Common.EntityInUse
{
	[Serializable]
	[EntityInUseDBSlotOn]
	[PXHidden]
	public class FeatureInUse : IBqlTable
	{
		public abstract class featureName : IBqlField { }
		[PXDBString(128, IsKey = true, IsUnicode = true)]
		public virtual string FeatureName { get; set; }
	}
}
