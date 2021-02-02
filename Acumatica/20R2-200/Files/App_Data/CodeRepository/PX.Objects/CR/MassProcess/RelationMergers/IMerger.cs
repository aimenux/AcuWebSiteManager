using System;
using PX.Data;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	[Obsolete("Will be removed in 7.0 version")]
	public interface IMerger
	{
		void SetField(PXGraph graph,
			       object resultOldValuesRecord,
				   Type changingField,
				   object newValue);
	}
}
