using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR.MassProcess.RelationMergers;

namespace PX.Objects.CR.MassProcess
{
	[Obsolete("Will be removed in 7.0 version")]
	public interface IMassMerge:IMassProcess
	{
		/// <summary>
		///Type - relation for custom process
		/// </summary>
		IEnumerable<IRelationManager> CustomRelationManagers { get; }

		/// <summary>
		/// if null, default survival will be used
		/// </summary>
		Func<IEnumerable<object>, object> MergeResultRecordProvider { get; }
	}
}
