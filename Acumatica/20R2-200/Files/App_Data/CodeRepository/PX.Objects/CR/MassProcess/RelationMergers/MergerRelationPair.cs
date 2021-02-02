using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Objects.CR.MassProcess.DacRelations;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	[Obsolete("Will be removed in 7.0 version")]
	struct MergerRelationPair
	{
		public IRelationManager Manager;
		public Relation Relation;
	}
}
