using PX.Data;

namespace PX.Objects.Common
{
	public static class PXSelectBaseExtensions
	{
		/// <summary>
		/// Returns a value indicating whether the view's Select method
		/// will return at least one record.
		/// </summary>
		/// <param name="select">The view to be checked.</param>
		/// <param name="parameters">
		/// The explicit values for such parameters as <see cref="Required{Field}"/>,
		/// <see cref="Optional{Field}"/>, and <see cref="Argument{ArgumentType}"/> that
		/// will be passed into the select method.
		/// </param>
		/// <returns>
		/// <c>true</c>, if the view's Select execution result contains at least one record,
		/// <c>false</c> otherwise.
		/// </returns>
		public static bool Any<T>(this PXSelectBase<T> select, params object[] parameters) 
			where T : class, IBqlTable, new()
		{
			return select.SelectWindowed(0, 1, parameters).Count > 0;
		}
	}
}
