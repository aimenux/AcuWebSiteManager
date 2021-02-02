using System;
using PX.Data;

namespace PX.Objects.Common.Discount.Mappers
{
	public abstract class DiscountedLineMapperBase
	{
		public PXCache Cache { get; }
		public object MappedLine { get; }

		protected DiscountedLineMapperBase(PXCache cache, object row)
		{
			this.Cache = cache;
			this.MappedLine = row;
		}

		public abstract Type GetField<T>() where T : IBqlField;

		public virtual void RaiseFieldUpdating<T>(ref object newValue)
			where T : IBqlField
		{
			Cache.RaiseFieldUpdating(GetField<T>().Name, MappedLine, ref newValue);
		}

		public virtual void RaiseFieldUpdated<T>(object oldValue)
			where T : IBqlField
		{
			Cache.RaiseFieldUpdated(GetField<T>().Name, MappedLine, oldValue);
		}

		public virtual void RaiseFieldVerifying<T>(ref object newValue)
			where T : IBqlField
		{
			Cache.RaiseFieldVerifying(GetField<T>().Name, MappedLine, ref newValue);
		}
	}
}