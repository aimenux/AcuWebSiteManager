using System;
using System.Collections.Generic;

namespace PX.Objects.Common.DataIntegrity
{
	public class InconsistencyHandlingMode : ILabelProvider
	{
		/// <summary>
		/// The release integrity validator will do nothing.
		/// </summary>
		public const string None = "N";
		/// <summary>
		/// The release integrity validator will log errors into the <see cref="DataIntegrityLog"/> table.
		/// </summary>
		public const string Log = "L";
		/// <summary>
		/// The release integrity validator will prevent errors and not log them into the database.
		/// </summary>
		public const string Prevent = "P";

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			// This option is not currently shown on the UI and will be
			// turned on for particular clients on a case-by-case basis, 
			// only if someone complains about performance.
			// -
			// { None, nameof(None) },
			{ Log, Messages.LogErrors },
			{ Prevent, Messages.PreventRelease },
		};
	}
}
