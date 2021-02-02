using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Data.WorkflowAPI
{
	public static class WorkflowExtensions
	{
		/// <summary>
		/// Get a new instance of an anonymous class, that contains <see cref="BoundedTo{TGraph, TPrimary}.Condition"/>s,
		/// but where condition names are taken from their properties' names.
		/// </summary>
		/// <param name="conditionPack">An instance of an anonymous class, that contains only properties of <see cref="BoundedTo{TGraph, TPrimary}.Condition"/> type.</param>
		public static T AutoNameConditions<T>(this T conditionPack)
			where T : class
		{
			if (!typeof(T).IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
				throw new InvalidOperationException("Only instances of anonymous types are allowed");

			return (T)Activator.CreateInstance(
				typeof(T),
				typeof(T)
					.GetProperties()
					.Select(p => p.PropertyType.GetMethod(nameof(BoundedTo<PXGraph, Table>.Condition.WithSharedName)).Invoke(p.GetValue(conditionPack), new object[] { p.Name }))
					.ToArray());
		}

		[PXHidden]
		private class Table : IBqlTable { }
	}
}