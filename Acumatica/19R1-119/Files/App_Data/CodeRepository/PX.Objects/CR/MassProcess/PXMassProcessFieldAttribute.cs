using System;
using PX.Common;
using PX.Data;

namespace PX.Objects.CR.MassProcess
{	
	public abstract class PXMassProcessFieldAttribute : PXEventSubscriberAttribute
	{
		private Type _searchCommand;

		public Type SearchCommand
		{
			get { return _searchCommand; }
			set
			{
				if (value != null && !typeof(BqlCommand).IsAssignableFrom(value))
					throw new ArgumentException(string.Format("Type '{0}' must inherite '{1}' type.", 
						value.GetLongName(), typeof(BqlCommand).GetLongName()), 
						"value");

				if (value != null && !typeof(IBqlSearch).IsAssignableFrom(value))
					throw new ArgumentException(string.Format("Type '{0}' must implement interface '{1}'.",
						value.GetLongName(), typeof(IBqlSearch).GetLongName()),
						"value");

				_searchCommand = value;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public class PXMassUpdatableFieldAttribute : PXMassProcessFieldAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class PXMassMergableFieldAttribute : PXEventSubscriberAttribute
	{
	}
}
