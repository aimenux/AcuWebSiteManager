using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CR.MassProcess.DacRelations
{
	[Obsolete("Will be removed in 7.0 version")]
	public class OneToOneRelation :Relation
	{
		public OneToOneRelation(Type leftFkField, Type leftIdField,Type rightIdField,Type rightFkField)
		{
			Left = new DacReference {TargetField = leftIdField, ReferenceField = rightFkField};

			Right = new DacReference {TargetField = rightIdField, ReferenceField = leftFkField};
		}

		
	}
	[Obsolete("Will be removed in 7.0 version")]
	public class OneToOneRelation<TChildID, TChildFk,TParentID, TParentFk > : OneToOneRelation
		where TParentID : IBqlField
		where TParentFk : IBqlField
		where TChildID  : IBqlField
		where TChildFk  : IBqlField
	{
		public OneToOneRelation():base(typeof(TParentFk),typeof(TParentID),typeof(TChildID),typeof(TChildFk))
		{
			
		}
	}
	
}
