using System;

using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.Common
{
	public interface ISubaccountProvider
	{
		/// <summary>
		/// Creates a <see cref="GL.Sub.SubCD"/> using a given mask and source fields / values.
		/// </summary>
		/// <param name="mask">Subaccount mask.</param>
		/// <param name="sourceFieldValues">Source field values to create a subaccount.</param>
		/// <param name="sourceFields">Source fields that will be used to create a subaccount value.</param>
		string MakeSubaccount<Field>(string mask, object[] sourceFieldValues, Type[] sourceFields) 
			where Field: IBqlField;

		/// <summary>
		/// Retrieves the internal ID of a subaccount by a substitute natural key.
		/// </summary>
		int? GetSubaccountID(string subaccountCD);
	}

	public abstract class SubaccountProviderBase : ISubaccountProvider
	{
		protected readonly PXGraph _graph;

		protected SubaccountProviderBase(PXGraph graph)
		{
			_graph = graph;
		}

		public int? GetSubaccountID(string subaccountCD)
		{
			if (subaccountCD == null) return null;

			object subaccountCodeAsObject = subaccountCD;

			_graph.Caches[typeof(Currency)]
				.RaiseFieldUpdating<Currency.roundingGainSubID>(new Currency(), ref subaccountCodeAsObject);

			return (int?)subaccountCodeAsObject;
		}

		public abstract string MakeSubaccount<Field>(string mask, object[] sourceFieldValues, Type[] sourceFields)
			where Field: IBqlField;
	}
}